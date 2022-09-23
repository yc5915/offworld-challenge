using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Offworld.GameCore;
using Offworld.SystemCore;

public class MNetwork
{
    //private static Queue<ChatMessage> mChatMessages = new Queue<ChatMessage>();
    //public static void SendChatMessage(PlayerType sendingPlayer, bool team, string message, string observerName)
    //{
    //    mChatMessages.Enqueue(new ChatMessage(sendingPlayer, team, message, observerName));
    //}

    // modified version of ProcessUpdateMessage
    public static Dictionary<DirtyType, List<byte[]>> storage = new Dictionary<DirtyType, List<byte[]>>();
    public static void ProcessGameUpdate(byte[] update)
    {
        if (update[0] == 133)
            update = LZF.DecompressMessage(update);

        using (MemoryStream input = new MemoryStream(update))
        {
            using (BinaryReader reader = new BinaryReader(input))
            {
                reader.ReadByte();

                DirtyType dirtyType;
                int compatibilityNumber = VersionAndMod.Minor;
                while ((dirtyType = (DirtyType)reader.ReadInt16()) != DirtyType.END)
                {
                    int start = (int)input.Position;
                    if(!storage.ContainsKey(dirtyType))
                        storage.Add(dirtyType, new List<byte[]>());
                    GameServer gameClient = AppMain.gApp.gameServer();
                    if (gameClient == null && dirtyType != DirtyType.GAME_SETTINGS)
                        throw new Exception("MNetwork.ProcessGameUpdate: First update should be game settings to initialise the server. Is someone cheating?");
                    if (gameClient != null && dirtyType == DirtyType.GAME_SETTINGS)
                        throw new Exception("MNetwork.ProcessGameUpdate: Only the first update should have game settings to initialise the server. Is someone cheating?");

                    switch (dirtyType)
                    {
                        case DirtyType.GAME_SETTINGS:
                            using (new UnityProfileScope("Network::readGameSettings"))
                            {
                                GameSettings value = null;
                                GameSettings.Serialize(reader, ref value, compatibilityNumber);
                                Globals.SetFactory(new GameFactory());
                                gameClient = Globals.Factory.createGameServer();
                                AppMain.gApp.setGameServer(gameClient);
                                gameClient.init(Globals.Infos, value);
                                gameClient.mapServer().setHasResourceInfo(true);
                            }
                            break;
                        case DirtyType.GAME:
                            using (new UnityProfileScope("Network::readGame"))
                            {
                                gameClient.readClientValues(reader, false, false, compatibilityNumber);
                                var map = gameClient.mapServer();
                            }
                            break;
                        case DirtyType.MARKET:
                            using (new UnityProfileScope("Network::readMarket"))
                            {
                                gameClient.marketClient().readClientValues(reader, false, compatibilityNumber);
                            }
                            break;
                        case DirtyType.GAME_EVENTS:
                            using (new UnityProfileScope("Network::readGameEvents"))
                            {
                                gameClient.gameEventsClient().readValues(reader, gameClient, false);
                            }
                            break;
                        case DirtyType.STATS:
                            using (new UnityProfileScope("Network::readStats"))
                            {
                                gameClient.statsClient().SerializeClient(reader, false);
                            }
                            break;
                        case DirtyType.PLAYER:
                            using (new UnityProfileScope("Network::readPlayer"))
                            {
                                short eIndex = reader.ReadInt16();
                                PlayerClient playerClient = gameClient.playerClient((PlayerType)eIndex);
                                playerClient.readClientValues(reader, false, compatibilityNumber);
                            }
                            break;
                        case DirtyType.TILE:
                            using (new UnityProfileScope("Network::readTile"))
                            {
                                short iIndex;
                                while ((iIndex = reader.ReadInt16()) != -1)
                                {
                                    gameClient.tileClient((int)iIndex).readClientValues(reader, false, compatibilityNumber);
                                }
                            }
                            break;
                        case DirtyType.MODULE:
                            using (new UnityProfileScope("Network::readModule"))
                            {
                                int key = (int)reader.ReadInt16();
                                ModuleClient moduleClient;
                                if (gameClient.getModuleDictionary().ContainsKey(key))
                                {
                                    moduleClient = gameClient.getModuleDictionary()[key];
                                }
                                else
                                {
                                    moduleClient = Globals.Factory.createModuleClient(gameClient);
                                    gameClient.getModuleDictionary()[key] = moduleClient;
                                }
                                moduleClient.readClientValues(reader, false);
                            }
                            break;
                        case DirtyType.HQ:
                            using (new UnityProfileScope("Network::readHQ"))
                            {
                                int key2 = (int)reader.ReadInt16();
                                HQClient hqclient;
                                if (gameClient.getHQDictionary().ContainsKey(key2))
                                {
                                    hqclient = gameClient.getHQDictionary()[key2];
                                }
                                else
                                {
                                    hqclient = Globals.Factory.createHQClient(gameClient);
                                    gameClient.getHQDictionary()[key2] = hqclient;
                                }
                                hqclient.readClientValues(reader, false);
                            }
                            break;
                        case DirtyType.CONSTRUCTION:
                            using (new UnityProfileScope("Network::readConstruction"))
                            {
                                int key3 = (int)reader.ReadInt16();
                                ConstructionClient constructionClient;
                                if (gameClient.getConstructionDictionary().ContainsKey(key3))
                                {
                                    constructionClient = gameClient.getConstructionDictionary()[key3];
                                }
                                else
                                {
                                    constructionClient = Globals.Factory.createConstructionClient(gameClient);
                                    gameClient.getConstructionDictionary()[key3] = constructionClient;
                                }
                                constructionClient.readClientValues(reader, false);
                            }
                            break;
                        case DirtyType.BUILDING:
                            using (new UnityProfileScope("Network::readBuilding"))
                            {
                                int key4 = (int)reader.ReadInt16();
                                BuildingClient buildingClient;
                                if (gameClient.getBuildingDictionary().ContainsKey(key4))
                                {
                                    buildingClient = gameClient.getBuildingDictionary()[key4];
                                }
                                else
                                {
                                    buildingClient = Globals.Factory.createBuildingClient(gameClient);
                                    gameClient.getBuildingDictionary()[key4] = buildingClient;
                                }
                                buildingClient.readClientValues(reader, false, compatibilityNumber);
                            }
                            break;
                        case DirtyType.UNIT:
                            using (new UnityProfileScope("Network::readUnit"))
                            {
                                int key5 = (int)reader.ReadInt16();
                                UnitClient unitClient;
                                if (gameClient.getUnitDictionary().ContainsKey(key5))
                                {
                                    unitClient = gameClient.getUnitDictionary()[key5];
                                }
                                else
                                {
                                    unitClient = Globals.Factory.createUnitClient(gameClient);
                                    gameClient.getUnitDictionary()[key5] = unitClient;
                                }
                                unitClient.readClientValues(reader, false);
                            }
                            break;
                        case DirtyType.CONDITION_MANAGER:
                            using (new UnityProfileScope("Network::readConditionManager"))
                            {
                                gameClient.conditionManagerClient().Serialize(reader, false);
                            }
                            break;
                        default:
                            //Debug.LogError("ProcessUpdateMessage: Invalid DirtyType: " + dirtyType);
                            break;
                    }
                    int end = (int)input.Position;
                    storage[dirtyType].Add(update.Skip(start).Take(end - start).ToArray());
                }
            }
        }
    }

    // modified version of writeDirtyGame
    public static byte[] GetGameUpdate()
    {
        GameServer pGame = AppMain.gApp.gameServer();
        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
            {
                binaryWriter.Write((byte)132);
                using (new UnityProfileScope("Network::writeGameSettings"))
                {
                    if (pGame.isSettingsDirty())
                    {
                        binaryWriter.Write((short)32);
                        GameSettings gameSettings = pGame.gameSettings();
                        GameSettings.Serialize(binaryWriter, ref gameSettings, VersionAndMod.Minor);
                        pGame.clearSettingsDirty();
                    }
                }
                using (new UnityProfileScope("Network::writeGame"))
                {
                    if (pGame.isAnyDirty())
                    {
                        binaryWriter.Write((short)33);
                        pGame.writeClientValues(binaryWriter, false, false, VersionAndMod.Minor);
                        pGame.clearDirty();
                    }
                }
                using (new UnityProfileScope("Network::writeMarket"))
                {
                    if (pGame.marketServer().isAnyDirty())
                    {
                        binaryWriter.Write((short)34);
                        pGame.marketServer().writeClientValues(binaryWriter, false, VersionAndMod.Minor);
                        pGame.marketServer().clearDirty();
                    }
                }
                using (new UnityProfileScope("Network::writeStats"))
                {
                    if (pGame.statsServer().isAnyDirty())
                    {
                        binaryWriter.Write((short)36);
                        pGame.statsServer().SerializeClient(binaryWriter, false);
                        pGame.statsServer().clearDirty();
                    }
                }
                using (new UnityProfileScope("Network::writeConditionManager"))
                {
                    if (pGame.conditionManagerServer().isAnyDirty())
                    {
                        binaryWriter.Write((short)44);
                        pGame.conditionManagerServer().Serialize(binaryWriter, false);
                        pGame.conditionManagerServer().clearDirty();
                    }
                }
                using (new UnityProfileScope("Network::writePlayers"))
                {
                    for (PlayerType playerType = (PlayerType)0; playerType < pGame.getNumPlayers(); playerType++)
                    {
                        PlayerServer playerServer = pGame.playerServer(playerType);
                        if (playerServer.isAnyDirty())
                        {
                            binaryWriter.Write((short)37);
                            binaryWriter.Write((short)playerType);
                            playerServer.writeClientValues(binaryWriter, false, VersionAndMod.Minor);
                            playerServer.clearDirty();
                        }
                    }
                }
                using (new UnityProfileScope("Network::writeTiles"))
                {
                    if (pGame.mapServer().isDirtyTiles())
                    {
                        binaryWriter.Write((short)38);
                        foreach (TileServer tileServer in pGame.tileServerAll())
                        {
                            if (tileServer.isAnyDirty())
                            {
                                binaryWriter.Write((short)tileServer.getID());
                                tileServer.writeClientValues(binaryWriter, false, VersionAndMod.Minor);
                                tileServer.clearDirty();
                            }
                        }
                        short value = -1;
                        binaryWriter.Write(value);
                        pGame.mapServer().clearDirtyTiles();
                    }
                }
                using (new UnityProfileScope("Network::writeModules"))
                {
                    foreach (KeyValuePair<int, ModuleClient> keyValuePair in pGame.getModuleDictionary())
                    {
                        ModuleServer moduleServer = (ModuleServer)keyValuePair.Value;
                        if (moduleServer.isAnyDirty())
                        {
                            binaryWriter.Write((short)39);
                            binaryWriter.Write((short)moduleServer.getID());
                            moduleServer.writeClientValues(binaryWriter, false);
                            moduleServer.clearDirty();
                        }
                    }
                }
                using (new UnityProfileScope("Network::writeHQs"))
                {
                    foreach (KeyValuePair<int, HQClient> keyValuePair2 in pGame.getHQDictionary())
                    {
                        HQServer hqserver = (HQServer)keyValuePair2.Value;
                        if (hqserver.isAnyDirty())
                        {
                            binaryWriter.Write((short)40);
                            binaryWriter.Write((short)hqserver.getID());
                            hqserver.writeClientValues(binaryWriter, false);
                            hqserver.clearDirty();
                        }
                    }
                }
                using (new UnityProfileScope("Network::writeConstructions"))
                {
                    foreach (KeyValuePair<int, ConstructionClient> keyValuePair3 in pGame.getConstructionDictionary())
                    {
                        ConstructionServer constructionServer = (ConstructionServer)keyValuePair3.Value;
                        if (constructionServer.isAnyDirty())
                        {
                            binaryWriter.Write((short)41);
                            binaryWriter.Write((short)constructionServer.getID());
                            constructionServer.writeClientValues(binaryWriter, false);
                            constructionServer.clearDirty();
                        }
                    }
                }
                using (new UnityProfileScope("Network::writeBuildings"))
                {
                    foreach (KeyValuePair<int, BuildingClient> keyValuePair4 in pGame.getBuildingDictionary())
                    {
                        BuildingServer buildingServer = (BuildingServer)keyValuePair4.Value;
                        if (buildingServer.isAnyDirty())
                        {
                            binaryWriter.Write((short)42);
                            binaryWriter.Write((short)buildingServer.getID());
                            buildingServer.writeClientValues(binaryWriter, false, VersionAndMod.Minor);
                            buildingServer.clearDirty();
                        }
                    }
                }
                using (new UnityProfileScope("Network::writeUnits"))
                {
                    foreach (KeyValuePair<int, UnitClient> keyValuePair5 in pGame.getUnitDictionary())
                    {
                        UnitServer unitServer = (UnitServer)keyValuePair5.Value;
                        if (unitServer.isAnyDirty())
                        {
                            binaryWriter.Write((short)43);
                            binaryWriter.Write((short)unitServer.getID());
                            unitServer.writeClientValues(binaryWriter, false);
                            unitServer.clearDirty();
                        }
                    }
                }
                using (new UnityProfileScope("Network::writeGameEvents"))
                {
                    if (pGame.gameEventsServer().isAnyDirty())
                    {
                        binaryWriter.Write((short)35);
                        pGame.gameEventsServer().writeValues(binaryWriter, false);
                        pGame.gameEventsServer().clearDirty();
                    }
                }

                byte[] msgData = null;
                binaryWriter.Write((short)(-1));
                if (LZF.CompressMessage(memoryStream.ToArray(), ref msgData))
                {
                    msgData[0] = 133;
                }

                //foreach (var msg in mChatMessages)
                //{
                //    binaryWriter.Write((byte)65);
                //    msg.Serialize((object)binaryWriter);
                //}
                //mChatMessages.Clear();

                return msgData;
            }
        }
    }
}
