using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using Offworld.SystemCore;

namespace Offworld.GameCore
{
    public static class SimplifyIOGame
    {
        // Player tile unit mission count
        public static void PlayerMissionData(object stream, ref List<List<sbyte>> value, int playerCount, string name)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;

                for (int i = 0; i < playerCount; i++)
                {
                    foreach (sbyte j in value[i])
                        writer.Write(j);
                }
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;

                value.Clear();
                for (int i = 0; i < playerCount; i++)
                {
                    value.Add(new List<sbyte>());

                    for (int j = 0; j < (int)MissionType.NUM_TYPES; j++)
                        value[i].Add(reader.ReadSByte());
                }
            }
        }

        // MessageData
        public static void MessageData(object stream, ref LinkedList<MessageInfo> value)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;

                writer.Write7BitInt(value.Count);

                foreach (MessageInfo pLoopMessageInfo in value)
                {
                    writer.Write(pLoopMessageInfo.mzText);
                    writer.Write7BitInt((int)pLoopMessageInfo.meAudio);

                    writer.Write7BitInt(pLoopMessageInfo.miID);
                    writer.Write7BitInt(pLoopMessageInfo.miUpdateCount);
                    writer.Write7BitInt(pLoopMessageInfo.miTileID);
                }
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;

                value.Clear();

                int iNum = reader.Read7BitInt();

                for (int i = 0; i < iNum; i++)
                {
                    string zText = reader.ReadString();
                    AudioTypeT eAudio = (AudioTypeT)reader.Read7BitInt();

                    int iID = reader.Read7BitInt();
                    int iUpdateCount = reader.Read7BitInt();
                    int iTileID = reader.Read7BitInt();

                    value.AddLast(new MessageInfo(zText, eAudio, iID, iUpdateCount, iTileID));
                }
            }
        }

        // TerrainData
        public static void TerrainData(object stream, ref List<TerrainInfo> value)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;

                // Write the data a parallel arrays so it compresses better
                writer.Write7BitInt(value.Count);
                foreach (TerrainInfo pLoopTerrainInfo in value)
                {
                    writer.Write((sbyte)(pLoopTerrainInfo.Terrain));
                }
                foreach (TerrainInfo pLoopTerrainInfo in value)
                {
                    writer.Write((sbyte)(pLoopTerrainInfo.Height));
                }
                foreach (TerrainInfo pLoopTerrainInfo in value)
                {
                    writer.Write((sbyte)(pLoopTerrainInfo.Wind));
                }
                foreach (TerrainInfo pLoopTerrainInfo in value)
                {
                    writer.Write((sbyte)(pLoopTerrainInfo.Ice));
                }

                foreach (TerrainInfo pLoopTerrainInfo in value)
                {
                    writer.Write((sbyte)(pLoopTerrainInfo.CraterChunkID));
                }
                foreach (TerrainInfo pLoopTerrainInfo in value)
                {
                    writer.Write((sbyte)(pLoopTerrainInfo.CraterChunkDir));
                }
                foreach (TerrainInfo pLoopTerrainInfo in value)
                {
                    writer.Write((sbyte)(pLoopTerrainInfo.IsCrater));
                }
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;

                value.Clear();

                int iNum = reader.Read7BitInt();

                for (int i = 0; i < iNum; i++)
                {
                    TerrainType eTerrain = (TerrainType)reader.ReadSByte();
                    value.Add(new TerrainInfo(eTerrain, 0, 0, 0));
                }
                for (int i = 0; i < iNum; i++)
                {
                    value[i].Height = (HeightType)reader.ReadSByte();
                }
                for (int i = 0; i < iNum; i++)
                {
                    value[i].Wind = (WindType)reader.ReadSByte();
                }
                for (int i = 0; i < iNum; i++)
                {
                    value[i].Ice = (IceType)reader.ReadSByte();
                }

                for (int i = 0; i < iNum; i++)
                {
                    value[i].CraterChunkID = (int)reader.ReadSByte();
                }
                for (int i = 0; i < iNum; i++)
                {
                    value[i].CraterChunkDir = (DirectionType)reader.ReadSByte();
                }
                for (int i = 0; i < iNum; i++)
                {
                    value[i].IsCrater = (int)reader.ReadSByte();
                }
            }
        }

        // EventGameTime
        public static void EventGameData(object stream, ref LinkedList<EventGameTime> value)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;

                writer.Write7BitInt(value.Count);
                foreach (EventGameTime pLoopEventGame in value)
                {
                    writer.Write7BitInt((Int32)(pLoopEventGame.meEventGame));
                    writer.Write7BitInt((Int32)(pLoopEventGame.mePlayer));
                    writer.Write7BitInt(pLoopEventGame.miDelay);
                    writer.Write7BitInt(pLoopEventGame.miTime);
                    writer.Write7BitInt(pLoopEventGame.miMultiplier);
                    writer.Write7BitInt(pLoopEventGame.miStartTurn);
                }
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;

                value.Clear();
                int iNum = reader.Read7BitInt();
                for (int i = 0; i < iNum; i++)
                {
                    EventGameType eEventGame = (EventGameType)reader.Read7BitInt();
                    PlayerType ePlayer = (PlayerType)reader.Read7BitInt();
                    int iDelay = reader.Read7BitInt();
                    int iTime = reader.Read7BitInt();
                    int iMultiplier = reader.Read7BitInt();
                    int iStartTurn = reader.Read7BitInt();

                    value.AddLast(new EventGameTime(eEventGame, ePlayer, iDelay, iTime, iMultiplier, iStartTurn));
                }
            }
        }

        // OrderData
        public static void OrderData(object stream, ref List<LinkedList<OrderInfo>> value, Infos pInfos)
        {
            if (stream is BinaryReader)
            {
                value.Clear();
            }

            for (OrderType eLoopOrder = 0; eLoopOrder < pInfos.ordersNum(); eLoopOrder++)
            {
                if (stream is BinaryWriter)
                {
                    BinaryWriter writer = stream as BinaryWriter;

                    writer.Write7BitInt(value[(int)eLoopOrder].Count);
                    foreach (OrderInfo pOrderInfo in value[(int)eLoopOrder])
                    {
                        writer.Write7BitInt((Int32)(pOrderInfo.meType));
                        writer.Write7BitInt(pOrderInfo.miIndex);
                        writer.Write7BitInt(pOrderInfo.miTime);
                        writer.Write7BitInt(pOrderInfo.miOriginalTime);
                        writer.Write7BitInt(pOrderInfo.miData1);
                        writer.Write7BitInt(pOrderInfo.miData2);
                        writer.Write7BitInt(pOrderInfo.miBuildingID);
                    }
                }
                else if (stream is BinaryReader)
                {
                    BinaryReader reader = stream as BinaryReader;

                    value.Add(new LinkedList<OrderInfo>());

                    int iNum = reader.Read7BitInt();
                    for (int i = 0; i < iNum; i++)
                    {
                        OrderType eType = (OrderType)reader.Read7BitInt();
                        int iIndex = reader.Read7BitInt();
                        int iTime = reader.Read7BitInt();
                        int iOriginalTime = reader.Read7BitInt();
                        int iData1 = reader.Read7BitInt();
                        int iData2 = reader.Read7BitInt();
                        int iBuildingID = reader.Read7BitInt();

                        value[(int)eLoopOrder].AddLast(new OrderInfo(eType, iIndex, iTime, iOriginalTime, iData1, iData2, iBuildingID));
                    }
                }
            }
        }

        // OrderData
        public static void StatEventData(object stream, ref List<List<StatEventData>> value)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;

                writer.Write7BitInt(value.Count);

                foreach (List<StatEventData> subList in value)
                {
                    writer.Write7BitInt(subList.Count);

                    foreach (StatEventData data in subList)
                    {
                        writer.Write7BitInt(data.miType);
                        writer.Write7BitInt(data.miTurn);
                        writer.Write7BitInt(data.miData1);
                        writer.Write7BitInt(data.miData2);
                        writer.Write7BitInt(data.miData3);
                        writer.Write7BitInt(data.miData4);
                    }
                }
            }
            else if (stream is BinaryReader)
            {
                value.Clear();

                BinaryReader reader = stream as BinaryReader;

                int iNum = reader.Read7BitInt();

                for (int i = 0; i < iNum; i++)
                {
                    List<StatEventData> subList = new List<StatEventData>();

                    value.Add(subList);

                    int iSubNum = reader.Read7BitInt();

                    for (int j = 0; j < iSubNum; j++)
                    {
                        int iType = reader.Read7BitInt();
                        int iTurn = reader.Read7BitInt();
                        int iData1 = reader.Read7BitInt();
                        int iData2 = reader.Read7BitInt();
                        int iData3 = reader.Read7BitInt();
                        int iData4 = reader.Read7BitInt();

                        subList.Add(new StatEventData((StatEventType)iType, iTurn, iData1, iData2, iData3, iData4));
                    }
                }
            }
            else if( stream is MJSONwriter )
            {
                MJSONwriter writer = stream as MJSONwriter;

                writer.StartArray("StatEvents");

                for (int i = 0; i < value.Count; i++)
                {
                    writer.StartArray();

                    List<StatEventData> subList = value[i];

                    for (int j = 0; j < subList.Count; j++)
                    {
                        writer.StartObject();
                        writer.WriteNamedElement("Type",  subList[j].miType);
                        writer.WriteNamedElement("Turn", subList[j].miTurn);
                        writer.WriteNamedElement("Data1", subList[j].miData1);
                        writer.WriteNamedElement("Data2", subList[j].miData2);
                        writer.WriteNamedElement("Data3", subList[j].miData3);
                        writer.WriteNamedElement("Data4", subList[j].miData4);
                        writer.EndObject();
                    }

                    writer.EndArray( false, (i+1) < value.Count);
                }

                writer.EndArray(false, false);
            }
        }

        // A dictionary of TileGroupServer that implement serialization function
        public static void Data(object stream, ref Dictionary<int, TileGroupServer> value, string name, GameServer game)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;

                writer.Write7BitInt(value.Count);

                foreach (int key in value.Keys)
                {
                    writer.Write7BitInt(key);
                    TileGroupServer item = value[key];
                    item.writeServerValues(writer);
                }
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;

                value = new Dictionary<int, TileGroupServer>();

                int count = reader.Read7BitInt();

                for (int i = 0; i < count; i++)
                {
                    int key = reader.Read7BitInt();
                    TileGroupServer item = Globals.Factory.createTileGroupServer(game);
                    item.readServerValues(reader, game);
                    value.Add(key, item);
                }
            }
        }

        // A dictionary of IModuleClient  or Module that implement serialization function
        public static void Data(object stream, ref Dictionary<int, ModuleClient> value, GameClient pGame, string name, bool bLoadSave, bool bRebuildingServer)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;

                int count = value.Count;
                writer.Write7BitInt(count);

                if (count > 0)
                {
                    bool isModuleServer = (value.First().Value is ModuleServer);
                    SimplifyIO.Data(stream, ref isModuleServer, "isModuleServer");

                    foreach (int key in value.Keys)
                    {
                        writer.Write7BitInt(key);
                        ModuleClient item = value[key];
                        item.writeClientValues(writer, (bLoadSave || bRebuildingServer));
                    }
                }
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;

                value = new Dictionary<int, ModuleClient>();

                int count = reader.Read7BitInt();

                if (count > 0)
                {
                    bool isModuleServer = false;
                    SimplifyIO.Data(stream, ref isModuleServer, "isModuleServer");

                    if (isModuleServer)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            int key = reader.Read7BitInt();
                            ModuleServer item = Globals.Factory.createModuleServer(pGame);
                            item.readClientValues(reader, (bLoadSave || bRebuildingServer));
                            value.Add(key, item);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < count; i++)
                        {
                            int key = reader.Read7BitInt();
                            ModuleClient item = ((bRebuildingServer) ? Globals.Factory.createModuleServer(pGame) : Globals.Factory.createModuleClient(pGame));
                            item.readClientValues(reader, (bLoadSave || bRebuildingServer));
                            value.Add(key, item);
                        }
                    }
                }
            }
        }

        // A dictionary of HQClient or HQ that implement serialization function
        public static void Data(object stream, ref Dictionary<int, HQClient> value, GameClient pGame, string name, bool bLoadSave, bool bRebuildingServer)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;

                int count = value.Count;
                writer.Write7BitInt(value.Count);

                if (count > 0)
                {
                    bool isHQServer = (value.First().Value is HQServer);
                    SimplifyIO.Data(stream, ref isHQServer, "isHQServer");

                    foreach (int key in value.Keys)
                    {
                        writer.Write7BitInt(key);
                        HQClient item = value[key];
                        item.writeClientValues(writer, (bLoadSave || bRebuildingServer));
                    }
                }
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;

                value = new Dictionary<int, HQClient>();

                int count = reader.Read7BitInt();

                if (count > 0)
                {
                    bool isHQServer = false;
                    SimplifyIO.Data(stream, ref isHQServer, "isHQServer");

                    if (isHQServer)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            int key = reader.Read7BitInt();
                            HQServer item = Globals.Factory.createHQServer(pGame);
                            item.readClientValues(reader, (bLoadSave || bRebuildingServer));
                            value.Add(key, item);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < count; i++)
                        {
                            int key = reader.Read7BitInt();
                            HQClient item = ((bRebuildingServer) ? Globals.Factory.createHQServer(pGame) : Globals.Factory.createHQClient(pGame));
                            item.readClientValues(reader, (bLoadSave || bRebuildingServer));
                            value.Add(key, item);
                        }
                    }
                }
            }
        }

        // A dictionary of ConstructionClient or Contruction that implement serialization function
        public static void Data(object stream, ref Dictionary<int, ConstructionClient> value, GameClient pGame, string name, bool bLoadSave, bool bRebuildingServer)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;

                int count = value.Count;
                writer.Write7BitInt(count);

                if (count > 0)
                {
                    bool isConstructionServer = (value.First().Value is ConstructionServer);
                    SimplifyIO.Data(stream, ref isConstructionServer, "isConstructionServer");

                    foreach (int key in value.Keys)
                    {
                        writer.Write7BitInt(key);
                        ConstructionClient item = value[key];
                        item.writeClientValues(writer, (bLoadSave || bRebuildingServer));
                    }
                }
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;

                value = new Dictionary<int, ConstructionClient>();

                int count = reader.Read7BitInt();

                if (count > 0)
                {
                    bool isConstructionServer = false;
                    SimplifyIO.Data(stream, ref isConstructionServer, "isConstructionServer");

                    if (isConstructionServer)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            int key = reader.Read7BitInt();
                            ConstructionServer item = Globals.Factory.createConstructionServer(pGame);
                            item.readClientValues(reader, (bLoadSave || bRebuildingServer));
                            value.Add(key, item);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < count; i++)
                        {
                            int key = reader.Read7BitInt();
                            ConstructionClient item = ((bRebuildingServer) ? Globals.Factory.createConstructionServer(pGame) : Globals.Factory.createConstructionClient(pGame));
                            item.readClientValues(reader, (bLoadSave || bRebuildingServer));
                            value.Add(key, item);
                        }
                    }
                }
            }
        }

        // A dictionary of BuildingClient or BuildingServer that implement serialization function
        public static void Data(object stream, ref Dictionary<int, BuildingClient> value, GameClient pGame, string name, bool bLoadSave, bool bRebuildingServer, int compatibilityNumber)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;

                int count = value.Count;
                writer.Write7BitInt(count);

                if (count > 0)
                {
                    bool isBuildingServer = (value.First().Value is BuildingServer);
                    SimplifyIO.Data(stream, ref isBuildingServer, "isBuildingServer");

                    foreach (int key in value.Keys)
                    {
                        writer.Write7BitInt(key);
                        BuildingClient item = value[key];
                        item.writeClientValues(writer, (bLoadSave || bRebuildingServer), compatibilityNumber);
                    }
                }
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;

                value = new Dictionary<int, BuildingClient>();

                int count = reader.Read7BitInt();

                if (count > 0)
                {
                    bool isBuildingServer = false;
                    SimplifyIO.Data(stream, ref isBuildingServer, "isBuildingServer");

                    if (isBuildingServer)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            int key = reader.Read7BitInt();
                            BuildingServer item = Globals.Factory.createBuildingServer(pGame);
                            item.readClientValues(reader, (bLoadSave || bRebuildingServer), compatibilityNumber);
                            value.Add(key, item);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < count; i++)
                        {
                            int key = reader.Read7BitInt();
                            BuildingClient item = ((bRebuildingServer) ? Globals.Factory.createBuildingServer(pGame) : Globals.Factory.createBuildingClient(pGame));
                            item.readClientValues(reader, (bLoadSave || bRebuildingServer), compatibilityNumber);
                            value.Add(key, item);
                        }
                    }
                }
            }
        }

        public static void Data(object stream, ref Dictionary<int, UnitClient> value, GameClient pGame, string name, bool bLoadSave, bool bRebuildingServer)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;

                int count = value.Count;
                writer.Write7BitInt(count);

                if (count > 0)
                {
                    bool isUnitServer = (value.First().Value is UnitServer);
                    SimplifyIO.Data(stream, ref isUnitServer, "isUnitServer");

                    if (isUnitServer)
                    {
                        foreach (int key in value.Keys)
                        {
                            writer.Write7BitInt(key);
                            UnitServer item = value[key] as UnitServer;
                            item.writeClientValues(writer, (bLoadSave || bRebuildingServer));
                        }
                    }
                    else
                    {
                        foreach (int key in value.Keys)
                        {
                            writer.Write7BitInt(key);
                            UnitClient item = value[key];
                            item.writeClientValues(writer, (bLoadSave || bRebuildingServer));
                        }
                    }
                }
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;

                value = new Dictionary<int, UnitClient>();

                int count = reader.Read7BitInt();

                if (count > 0)
                {
                    bool isUnitServer = false;
                    SimplifyIO.Data(stream, ref isUnitServer, "isUnitServer");

                    if (isUnitServer)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            int key = reader.Read7BitInt();
                            UnitServer item = Globals.Factory.createUnitServer(pGame);
                            item.readClientValues(reader, (bLoadSave || bRebuildingServer));
                            value.Add(key, item);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < count; i++)
                        {
                            int key = reader.Read7BitInt();
                            UnitClient item = ((bRebuildingServer) ? Globals.Factory.createUnitServer(pGame) : Globals.Factory.createUnitClient(pGame));
                            item.readClientValues(reader, (bLoadSave || bRebuildingServer));
                            value.Add(key, item);
                        }
                    }
                }
            }
        }

        public static void Data(object stream, ref MarketClient value, GameClient pGame, string name, bool bLoadSave, bool bRebuildingServer, int compatibilityNumber)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;

                bool isMarketServer = (value is MarketServer);
                SimplifyIO.Data(stream, ref isMarketServer, "isMarketServer");

                if (isMarketServer)
                {
                    ((MarketServer)value).writeServerValues(writer, (bLoadSave || bRebuildingServer), compatibilityNumber);
                }
                else
                {
                    value.writeClientValues(writer, (bLoadSave || bRebuildingServer), compatibilityNumber);
                }
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;

                bool isMarketServer = false;
                SimplifyIO.Data(stream, ref isMarketServer, "isMarketServer");

                if (isMarketServer)
                {
                    MarketServer market = Globals.Factory.createMarketServer(pGame);
                    market.readServerValues(reader, (bLoadSave || bRebuildingServer), compatibilityNumber);
                    value = market;
                }
                else
                {
                    if (bRebuildingServer)
                    {
                        value = Globals.Factory.createMarketServer(pGame);
                    }
                    else
                    {
                        value = Globals.Factory.createMarketClient(pGame);
                    }
                    value.readClientValues(reader, (bLoadSave || bRebuildingServer), compatibilityNumber);
                }
            }
        }

        public static void Data(object stream, ref GameEventsClient value, GameClient pGame, string name, bool bLoadSave, bool bRebuildingServer)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;

                bool isGameEventsServer = (value is GameEventsServer);
                SimplifyIO.Data(stream, ref isGameEventsServer, "isGameEventsServer");

                value.writeValues(writer, (bLoadSave || bRebuildingServer));
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;

                bool isGameEventsServer = false;
                SimplifyIO.Data(stream, ref isGameEventsServer, "isGameEventsServer");

                if (isGameEventsServer)
                {
                    GameEventsServer gameEvents = Globals.Factory.createGameEventsServer();
                    gameEvents.readValues(reader, pGame, (bLoadSave || bRebuildingServer));
                    value = gameEvents;
                }
                else
                {
                    if (bRebuildingServer)
                    {
                        value = Globals.Factory.createGameEventsServer();
                    }
                    else
                    {
                        value = Globals.Factory.createGameEventsClient();
                    }
                    value.readValues(reader, pGame, (bLoadSave || bRebuildingServer));
                }
            }
        }

        public static void Data(object stream, ref StatsClient value, GameClient pGame, string name, bool bLoadSave, bool bRebuildingServer)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;

                bool isStatsServer = (value is StatsServer);
                SimplifyIO.Data(stream, ref isStatsServer, "isStatsServer");

                value.SerializeClient(writer, bLoadSave || bRebuildingServer);
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;

                bool isStatsServer = false;
                SimplifyIO.Data(stream, ref isStatsServer, "isStatsServer");

                if (isStatsServer)
                {
                    StatsServer stats = Globals.Factory.createStatsServer(pGame);
                    stats.SerializeClient(reader, bLoadSave || bRebuildingServer);
                    value = stats;
                }
                else
                {
                    if (bRebuildingServer)
                    {
                        value = Globals.Factory.createStatsServer(pGame);
                    }
                    else
                    {
                        value = Globals.Factory.createStatsClient(pGame);
                    }
                    value.SerializeClient(reader, (bLoadSave || bRebuildingServer));
                }
            }
        }

        public static void Data(object stream, ref List<PlayerClient> value, GameClient pGame, string name, bool bLoadSave, bool bRebuildingServer, int compatibilityNumber)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;

                int count = value.Count;
                writer.Write7BitInt(count);

                if (count > 0)
                {
                    bool isPlayerServer = (value[0] is PlayerServer);
                    SimplifyIO.Data(stream, ref isPlayerServer, "isPlayerServer");

                    if (isPlayerServer)
                    {
                        foreach (PlayerServer player in value)
                        {
                            player.writeValues(writer, (bLoadSave || bRebuildingServer), compatibilityNumber);
                        }
                    }
                    else
                    {
                        foreach (PlayerClient player in value)
                        {
                            player.writeClientValues(writer, (bLoadSave || bRebuildingServer), compatibilityNumber);
                        }
                    }
                }
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;

                int count = reader.Read7BitInt();

                value = new List<PlayerClient>(count);

                if (count > 0)
                {
                    bool isPlayerServer = true;
                    SimplifyIO.Data(stream, ref isPlayerServer, "isPlayerServer");

                    if (isPlayerServer)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            PlayerServer player = Globals.Factory.createPlayerServer(pGame);
                            player.readValues(reader, (bLoadSave || bRebuildingServer), compatibilityNumber);
                            value.Add(player);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < count; i++)
                        {
                            PlayerClient player = ((bRebuildingServer) ? Globals.Factory.createPlayerServer(pGame) : Globals.Factory.createPlayerClient(pGame));
                            player.readClientValues(reader, (bLoadSave || bRebuildingServer), compatibilityNumber);
                            value.Add(player);
                        }
                    }
                }
            }
        }

        public static void Data(object stream, ref List<TileClient> value, GameClient pGame, string name, bool bLoadSave, bool bRebuildingServer, int compatibilityNumber)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;

                writer.Write7BitInt(value.Count);

                if (value.Count > 0)
                {
                    bool isTileServer = (value[0] is TileServer);
                    SimplifyIO.Data(stream, ref isTileServer, "isTileServer");

                    if (isTileServer)
                    {
                        foreach (TileServer tile in value)
                        {
                            tile.writeServerValues(writer, (bLoadSave || bRebuildingServer), compatibilityNumber);
                        }
                    }
                    else
                    {
                        foreach (TileClient tile in value)
                        {
                            tile.writeClientValues(writer, (bLoadSave || bRebuildingServer), compatibilityNumber);
                        }
                    }
                }
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;

                int count = reader.Read7BitInt();

                value = new List<TileClient>(count);

                if (count > 0)
                {
                    bool isTileServer = false;
                    SimplifyIO.Data(stream, ref isTileServer, "isTileServer");

                    if (isTileServer)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            TileServer tile = Globals.Factory.createTileServer(pGame);
                            tile.readServerValues(reader, (bLoadSave || bRebuildingServer), compatibilityNumber);
                            value.Add(tile);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < count; i++)
                        {
                            TileClient tile = ((bRebuildingServer) ? Globals.Factory.createTileServer(pGame) : Globals.Factory.createTileClient(pGame));
                            tile.readClientValues(reader, (bLoadSave || bRebuildingServer), compatibilityNumber);
                            value.Add(tile);
                        }
                    }
                }
            }
        }

        public static void Data(object stream, ref ConditionManagerClient value, GameClient pGame, string name, bool bLoadSave, bool bRebuildingServer)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;

                bool isConditionManagerServer = (value is ConditionManagerServer);
                SimplifyIO.Data(stream, ref isConditionManagerServer, "isConditionManagerServer");

                value.Serialize(writer, (bLoadSave || bRebuildingServer));
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;

                bool isConditionManagerServer = false;
                SimplifyIO.Data(stream, ref isConditionManagerServer, "isConditionManagerServer");

                if (isConditionManagerServer)
                {
                    ConditionManagerServer conditionManager = Globals.Factory.createConditionManagerServer(pGame);
                    conditionManager.Serialize(reader, (bLoadSave || bRebuildingServer));
                    value = conditionManager;
                }
                else
                {
                    if (bRebuildingServer)
                    {
                        value = Globals.Factory.createConditionManagerServer(pGame);
                    }
                    else
                    {
                        value = Globals.Factory.createConditionManagerClient(pGame);
                    }
                    value.Serialize(reader, (bLoadSave || bRebuildingServer));
                }
            }
        }

        public static void DataByTypeName<T, U>(object stream, ref T value, string name, List<U> infos, T defaultValue) where U : InfoBase
        {
            if(stream is BinaryWriter)
            {
                int index = CastTo<int>.From(value);
                string data = (index == Infos.cTYPE_CUSTOM) ? "CUSTOM" : (index == Infos.cTYPE_NONE) ? "NONE" : infos[index].mzType;
                SimplifyIO.Data(stream, ref data, name);
            }
            else if(stream is BinaryReader)
            {
                string data = "";
                SimplifyIO.Data(stream, ref data, name);
                value = defaultValue;
                if(data == "CUSTOM")
                    value = CastTo<T>.From(Infos.cTYPE_CUSTOM);
                else if(data == "NONE")
                    value = CastTo<T>.From(Infos.cTYPE_NONE);
                else //search for this name
                {
                    for(int i=0; i<infos.Count; i++)
                    {
                        if(infos[i].mzType == data)
                        {
                            value = CastTo<T>.From(i);
                            break;
                        }
                    }
                }
            }
        }
    }
}