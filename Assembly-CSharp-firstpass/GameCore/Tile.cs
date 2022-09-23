using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.IO;
using Offworld.SystemCore;
using System.Linq;
using System;

namespace Offworld.GameCore
{
    public class TileClient
    {
        protected GameClient mGame = null;
        protected Infos mInfos = null;
        GameClient gameClient()
        {
            return mGame;
        }
        protected Infos infos()
        {
            return mInfos;
        }

        public enum TileDirtyType
        {
            FIRST,

            miTileGroupID,
            miModuleID,
            miHQID,
            miConstructionID,
            miBuildingID,
            miUnitID,
            miFrozenTime,
            miDoubleTime,
            miHalfTime,
            miOverloadTime,
            miVirusTime,
            miTakeoverTime,
            miDefendTime,
            miDestroyTime,
            miClaimBlockTime,
            miFirstClaimTurn,
            miResourceCount,
            mbGeothermal,
            mbConnectedToHQ,
            mbHologram,
            mbRevealDefendSabotage,
            mbModuleRevealed,
            mbWasAuctioned,
            meOwner,
            meRealOwner,
            meClaimBlockPlayer,
            meFrozenSabotage,
            meDefendSabotage,
            meLastBuilding,
            meVisibleBuilding,
            meHologramBuilding,
            meTerrainRegion,
            maiSabotagedCount,
            maiResourceMined,
            mabPotentialHQConnection,
            mabRevealBuilding,
            maeVisibility,
            maeResourceLevel,
            maaiUnitMissionCount,
            meTerrain,

            NUM_TYPES
        }

        protected BitMaskMulti mDirtyBits = new BitMaskMulti((int)TileDirtyType.NUM_TYPES);
        protected virtual bool isDirty(TileDirtyType eType)
        {
            return mDirtyBits.GetBit((int)eType);
        }
        public virtual bool isAnyDirty()
        {
            return !(mDirtyBits.IsEmpty());
        }

        protected int miID = -1;
        protected int miTerrainID = -1;
        protected int miTileGroupID = -1;
        protected int miModuleID = -1;
        protected int miHQID = -1;
        protected int miConstructionID = -1;
        protected int miBuildingID = -1;
        protected int miUnitID = -1;
        protected int miFrozenTime = 0;
        protected int miDoubleTime = 0;
        protected int miHalfTime = 0;
        protected int miOverloadTime = 0;
        protected int miVirusTime = 0;
        protected int miTakeoverTime = 0;
        protected int miDefendTime = 0;
        protected int miDestroyTime = 0;
        protected int miClaimBlockTime = 0;
        protected int miFirstClaimTurn = 0;
        protected int miResourceCount = 0;

        protected bool mbGeothermal = false;
        protected bool mbConnectedToHQ = false;
        protected bool mbHologram = false;
        protected bool mbRevealDefendSabotage = false;
        protected bool mbModuleRevealed = false;
        protected bool mbWasAuctioned = false;

        protected PlayerType meOwner = PlayerType.NONE;
        protected PlayerType meRealOwner = PlayerType.NONE;
        protected PlayerType meClaimBlockPlayer = PlayerType.NONE;
        protected SabotageType meFrozenSabotage = SabotageType.NONE;
        protected SabotageType meDefendSabotage = SabotageType.NONE;
        protected BuildingType meLastBuilding = BuildingType.NONE;
        protected BuildingType meVisibleBuilding = BuildingType.NONE;
        protected BuildingType meHologramBuilding = BuildingType.NONE;
        protected TerrainType meTerrainRegion = TerrainType.NONE;

        protected List<int> maiSabotagedCount = new List<int>();
        protected List<int> maiResourceMined = new List<int>();

        protected List<bool> mabPotentialHQConnection = new List<bool>();
        protected List<bool> mabRevealBuilding = new List<bool>();

        protected List<VisibilityType> maeVisibility = new List<VisibilityType>();
        protected List<ResourceLevelType> maeResourceLevel = new List<ResourceLevelType>();

        protected List<List<sbyte>> maaiUnitMissionCount = new List<List<sbyte>>();

        //unserialized cached variables
        protected int miTileX = -1;
        protected int miTileY = -1;
        protected TerrainInfo mTerrainInfoCache = null;
        protected InfoTerrain mInfoTerrainCache = null;

        public TileClient(GameClient pGame)
        {
            mGame = pGame;
            mInfos = pGame.infos();
        }

        protected virtual void updateTileXY()
        {
            miTileX = miID % gameClient().getMapWidth();
            miTileY = miID / gameClient().getMapWidth();
        }

        public virtual void cacheTerrainInfo()
        {
            mTerrainInfoCache = gameClient().mapClient().terrainInfo(miTerrainID);
            mInfoTerrainCache = infos().terrain(mTerrainInfoCache.Terrain);
        }

        protected virtual void SerializeClient(object stream, bool bAll, int compatibilityNumber)
        {
            //using (new UnityProfileScope("Tile::Serialize"))
            {
                SimplifyIO.Data(stream, ref mDirtyBits, "DirtyBits");

                if (mDirtyBits.GetBit((int)TileDirtyType.FIRST) && !bAll)
                {
                    if (stream is BinaryReader)
                    {
                        initVariablesClient();
                    }
                }

                if (mDirtyBits.GetBit((int)TileDirtyType.FIRST) || bAll)
                {
                    SimplifyIO.Data(stream, ref miID, "ID");
                    SimplifyIO.Data(stream, ref miTerrainID, "TerrainID");

                    if (stream is BinaryReader)
                    {
                        updateTileXY();
                    }
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.miTileGroupID) || bAll)
                {
                    SimplifyIO.Data(stream, ref miTileGroupID, "TileGroupID");
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.miModuleID) || bAll)
                {
                    SimplifyIO.Data(stream, ref miModuleID, "ModuleID");
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.miHQID) || bAll)
                {
                    SimplifyIO.Data(stream, ref miHQID, "HQID");
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.miConstructionID) || bAll)
                {
                    SimplifyIO.Data(stream, ref miConstructionID, "ConstructionID");
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.miBuildingID) || bAll)
                {
                    SimplifyIO.Data(stream, ref miBuildingID, "BuildingID");
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.miUnitID) || bAll)
                {
                    SimplifyIO.Data(stream, ref miUnitID, "UnitID");
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.miFrozenTime) || bAll)
                {
                    SimplifyIO.Data(stream, ref miFrozenTime, "FrozenTime");
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.miDoubleTime) || bAll)
                {
                    SimplifyIO.Data(stream, ref miDoubleTime, "DoubleTime");
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.miHalfTime) || bAll)
                {
                    SimplifyIO.Data(stream, ref miHalfTime, "HalfTime");
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.miOverloadTime) || bAll)
                {
                    SimplifyIO.Data(stream, ref miOverloadTime, "OverloadTime");
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.miVirusTime) || bAll)
                {
                    SimplifyIO.Data(stream, ref miVirusTime, "VirusTime");
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.miTakeoverTime) || bAll)
                {
                    SimplifyIO.Data(stream, ref miTakeoverTime, "TakeoverTime");
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.miDefendTime) || bAll)
                {
                    SimplifyIO.Data(stream, ref miDefendTime, "DefendTime");
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.miDestroyTime) || bAll)
                {
                    SimplifyIO.Data(stream, ref miDestroyTime, "DestroyTime");
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.miClaimBlockTime) || bAll)
                {
                    SimplifyIO.Data(stream, ref miClaimBlockTime, "ClaimBlockTime");
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.miFirstClaimTurn) || bAll)
                {
                    SimplifyIO.Data(stream, ref miFirstClaimTurn, "FirstClaimTurn");
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.miResourceCount) || bAll)
                {
                    SimplifyIO.Data(stream, ref miResourceCount, "ResourceCount");
                }

                if (mDirtyBits.GetBit((int)TileDirtyType.mbGeothermal) || bAll)
                {
                    SimplifyIO.Data(stream, ref mbGeothermal, "Geothermal");
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.mbConnectedToHQ) || bAll)
                {
                    SimplifyIO.Data(stream, ref mbConnectedToHQ, "ConnectedToHQ");
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.mbHologram) || bAll)
                {
                    SimplifyIO.Data(stream, ref mbHologram, "Hologram");
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.mbRevealDefendSabotage) || bAll)
                {
                    SimplifyIO.Data(stream, ref mbRevealDefendSabotage, "RevealDefendSabotage");
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.mbModuleRevealed) || bAll)
                {
                    SimplifyIO.Data(stream, ref mbModuleRevealed, "ModuleRevealed");
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.mbWasAuctioned) || bAll)
                {
                    SimplifyIO.Data(stream, ref mbWasAuctioned, "WasAuctioned");
                }

                if (mDirtyBits.GetBit((int)TileDirtyType.meOwner) || bAll)
                {
                    SimplifyIO.Data(stream, ref meOwner, "Owner");
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.meRealOwner) || bAll)
                {
                    SimplifyIO.Data(stream, ref meRealOwner, "RealOwner");
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.meClaimBlockPlayer) || bAll)
                {
                    SimplifyIO.Data(stream, ref meClaimBlockPlayer, "ClaimBlockPlayer");
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.meFrozenSabotage) || bAll)
                {
                    SimplifyIO.Data(stream, ref meFrozenSabotage, "FrozenSabotage");
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.meDefendSabotage) || bAll)
                {
                    SimplifyIO.Data(stream, ref meDefendSabotage, "DefendSabotage");
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.meLastBuilding) || bAll)
                {
                    SimplifyIO.Data(stream, ref meLastBuilding, "LastBuilding");
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.meVisibleBuilding) || bAll)
                {
                    SimplifyIO.Data(stream, ref meVisibleBuilding, "VisibleBuilding");
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.meHologramBuilding) || bAll)
                {
                    SimplifyIO.Data(stream, ref meHologramBuilding, "HologramBuilding");
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.meTerrainRegion) || bAll)
                {
                    SimplifyIO.Data(stream, ref meTerrainRegion, "TerrainRegion");
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.meTerrain))
                {
                    List<TerrainInfo> maTerrainInfos = gameClient().mapClient().getTerrainArray();
                    SimplifyIOGame.TerrainData(stream, ref maTerrainInfos);
                    if (!bAll)
                    {
                        gameClient().mapClient().setTerrainArray(maTerrainInfos);
                        cacheTerrainInfo();
                    }
                }

                if (mDirtyBits.GetBit((int)TileDirtyType.maiSabotagedCount) || bAll)
                {
                    SimplifyIO.Data(stream, ref maiSabotagedCount, (int)infos().sabotagesNum(), "SabotagedCount");
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.maiResourceMined) || bAll)
                {
                    SimplifyIO.Data(stream, ref maiResourceMined, (int)infos().resourcesNum(), "ResourceMined");
                }

                if (mDirtyBits.GetBit((int)TileDirtyType.mabPotentialHQConnection) || bAll)
                {
                    SimplifyIO.Data(stream, ref mabPotentialHQConnection, (int)gameClient().getNumPlayers(), "PotentialHQConnection");
                }
                if (mDirtyBits.GetBit((int)TileDirtyType.mabRevealBuilding) || bAll)
                {
                    SimplifyIO.Data(stream, ref mabRevealBuilding, (int)gameClient().getNumTeams(), "RevealBuilding");
                }

                if (mDirtyBits.GetBit((int)TileDirtyType.maeVisibility) || bAll)
                {
                    SimplifyIO.Data(stream, ref maeVisibility, (int)gameClient().getNumPlayers(), "Visibility");
                }

                if (mDirtyBits.GetBit((int)TileDirtyType.maeResourceLevel) || bAll)
                {
                    SimplifyIO.DataSByte(stream, ref maeResourceLevel, (int)infos().resourcesNum(), "ResourceLevel_");
                }

                if (mDirtyBits.GetBit((int)TileDirtyType.maaiUnitMissionCount) || bAll)
                {
                    SimplifyIOGame.PlayerMissionData(stream, ref maaiUnitMissionCount, (int)gameClient().getNumPlayers(), "UnitMissionCount_");
                }
            }
        }

        public virtual void writeClientValues(BinaryWriter stream, bool bAll, int compatibilityNumber)
        {
            SerializeClient(stream, bAll, compatibilityNumber);
        }

        public virtual void readClientValues(BinaryReader stream, bool bAll, int compatibilityNumber)
        {
            SerializeClient(stream, bAll, compatibilityNumber);
        }

        protected virtual void initVariablesClient()
        {
            maiSabotagedCount.Clear();
            for (SabotageType eLoopSabotage = 0; eLoopSabotage < infos().sabotagesNum(); eLoopSabotage++)
            {
                maiSabotagedCount.Add(0);
            }

            maiResourceMined.Clear();
            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                maiResourceMined.Add(0);
            }

            mabPotentialHQConnection.Clear();
            for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameClient().getNumPlayers(); eLoopPlayer++)
            {
                mabPotentialHQConnection.Add(false);
            }

            mabRevealBuilding.Clear();
            for (TeamType eLoopTeam = 0; eLoopTeam < (TeamType)(gameClient().getNumTeams()); eLoopTeam++)
            {
                mabRevealBuilding.Add(false);
            }

            maeVisibility.Clear();
            for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameClient().getNumPlayers(); eLoopPlayer++)
            {
                maeVisibility.Add(VisibilityType.FOGGED);
            }

            maeResourceLevel.Clear();
            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                maeResourceLevel.Add(ResourceLevelType.NONE);
            }

            maaiUnitMissionCount.Clear();
            for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameClient().getNumPlayers(); eLoopPlayer++)
            {
                maaiUnitMissionCount.Add(new List<sbyte>());

                for (MissionType eMission = 0; eMission < MissionType.NUM_TYPES; eMission++)
                {
                    maaiUnitMissionCount[(int)eLoopPlayer].Add(0);
                }
            }
        }

        public virtual bool showCorrectBuilding(TeamType eTeam, bool bTypeOnly)
        {
            if (eTeam == TeamType.NONE)
            {
                return true;
            }

            if (getTeam() == eTeam)
            {
                return true;
            }

            if (bTypeOnly)
            {
                if (getRealTeam() == eTeam)
                {
                    return true;
                }
            }

            if (isRevealBuildingAdjacent(eTeam))
            {
                return true;
            }

            return false;
        }

        public virtual bool isEmpty()
        {
            if (isModule())
            {
                return false;
            }

            if (isHQ())
            {
                return false;
            }

            if (isConstruction())
            {
                return false;
            }

            if (isBuilding())
            {
                return false;
            }

            return true;
        }

        public virtual bool canMine()
        {
            if (!usable())
            {
                return false;
            }

            if (isGeothermal())
            {
                return false;
            }

            if (!isEmpty())
            {
                return false;
            }

            return true;
        }

        protected virtual bool canHaveModuleTileOnly(ModuleType eModule)
        {
            if (!usable() || isGeothermal() || isClaimed() || !isEmpty() || (gameClient().auctionTileClient() == this))
            {
                return false;
            }

            {
                IceType eIceRequired = infos().module(eModule).meIceRequired;

                if (eIceRequired != IceType.NONE)
                {
                    if (getIce() != eIceRequired)
                    {
                        return false;
                    }
                }
            }

            {
                TerrainType eTerrainRequired = infos().module(eModule).meTerrainRequired;

                if (eTerrainRequired != TerrainType.NONE)
                {
                    if (getTerrain() != eTerrainRequired)
                    {
                        return false;
                    }
                }
                else if (infos().terrain(getTerrain()).mbRequiredOnly)
                {
                    return false;
                }
            }

            return true;
        }

        public virtual bool canHaveModule(ModuleType eModule)
        {
            if (!canHaveModuleTileOnly(eModule))
            {
                return false;
            }

            for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
            {
                if (infos().module(eModule).mabFootprint[(int)eDirection])
                {
                    TileClient pAdjacentTile = gameClient().tileClientAdjacent(this, eDirection);

                    if (pAdjacentTile == null)
                    {
                        return false;
                    }

                    if (!(pAdjacentTile.canHaveModuleTileOnly(eModule)))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public virtual bool isOrAdjacentRevealed(PlayerType ePlayer)
        {
            if (getVisibility(ePlayer) == VisibilityType.REVEALED)
            {
                return true;
            }

            for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
            {
                TileClient pAdjacentTile = gameClient().mapClient().tileClientAdjacent(this, eLoopDirection);

                if (pAdjacentTile != null)
                {
                    if (pAdjacentTile.getVisibility(ePlayer) == VisibilityType.REVEALED)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public virtual int countAdjacentNotVisible(PlayerType ePlayer)
        {
            int iCount = 0;

            for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
            {
                TileClient pAdjacentTile = gameClient().mapClient().tileClientAdjacent(this, eLoopDirection);

                if (pAdjacentTile != null)
                {
                    if (pAdjacentTile.getVisibility(ePlayer) < VisibilityType.VISIBLE)
                    {
                        iCount++;
                    }
                }
            }

            return iCount;
        }

        public virtual bool onOrAdjacentToGeothermal()
        {
            if (isGeothermal())
            {
                return true;
            }

            for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
            {
                TileClient pAdjacentTile = gameClient().mapClient().tileClientAdjacent(this, eLoopDirection);

                if (pAdjacentTile != null)
                {
                    if (pAdjacentTile.isGeothermal())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public virtual bool adjacentToHQ(PlayerType ePlayer)
        {
            for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
            {
                TileClient pAdjacentTile = gameClient().mapClient().tileClientAdjacent(this, eLoopDirection);

                if (pAdjacentTile != null)
                {
                    if ((pAdjacentTile.hqClient() != null) &&
                        ((ePlayer == PlayerType.NONE) || (ePlayer == pAdjacentTile.hqClient().getOwner())))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public virtual bool adjacentToBuilding(PlayerType ePlayer)
        {
            for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
            {
                TileClient pAdjacentTile = gameClient().mapClient().tileClientAdjacent(this, eLoopDirection);

                if (pAdjacentTile != null)
                {
                    if ((pAdjacentTile.buildingClient() != null) &&
                        (pAdjacentTile.buildingClient().getOwner() == ePlayer) &&
                        (pAdjacentTile.getHeight() == getHeight()))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public virtual bool adjacentToModule()
        {
            for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
            {
                TileClient pAdjacentTile = gameClient().mapClient().tileClientAdjacent(this, eLoopDirection);

                if (pAdjacentTile != null)
                {
                    if (pAdjacentTile.isModule())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public virtual int countAdjacentUnconnectedBuildings(PlayerType ePlayer)
        {
            int iCount = 0;

            for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
            {
                TileClient pAdjacentTile = gameClient().mapClient().tileClientAdjacent(this, eLoopDirection);

                if (pAdjacentTile != null)
                {
                    if ((pAdjacentTile.buildingClient() != null) &&
                        (pAdjacentTile.buildingClient().getOwner() == ePlayer) &&
                        !(pAdjacentTile.buildingClient().tileClient().isConnectedToHQ()) &&
                        (pAdjacentTile.getHeight() == getHeight()))
                    {
                        iCount++;
                    }
                }
            }

            return iCount;
        }

        public virtual int countConnections(BuildingType eBuilding, PlayerType ePlayer, bool bCountConstructions, bool bReverse, TeamType eVisibleTeam)
        {
            //using (new UnityProfileScope("Tile::countConnections")) //Called too many times per frame
            {
                PlayerClient pPlayer = gameClient().playerClient(ePlayer);

                int iCount = 0;

                for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                {
                    TileClient pAdjacentTile = gameClient().mapClient().tileClientAdjacent(this, eLoopDirection);

                    if (pAdjacentTile != null)
                    {
                        if (pAdjacentTile.getHeight() == getHeight())
                        {
                            if (pAdjacentTile.isBuilding())
                            {
                                BuildingClient pAdjacentBuilding = pAdjacentTile.buildingClient();
                                BuildingType eAdjacentBuilding = pAdjacentBuilding.getVisibleType(eVisibleTeam);

                                if ((pAdjacentBuilding.getOwner() == ePlayer) &&
                                    ((bReverse) ? (BuildingServer.isConnectionValid(eAdjacentBuilding, eBuilding, this, pPlayer, gameClient(), infos())) :
                                                  (BuildingServer.isConnectionValid(eBuilding, eAdjacentBuilding, pAdjacentTile, pPlayer, gameClient(), infos()))))
                                {
                                    iCount++;
                                }
                            }
                            else if (bCountConstructions && pAdjacentTile.isConstruction())
                            {
                                ConstructionClient pAdjacentConstruction = pAdjacentTile.constructionClient();

                                if ((pAdjacentConstruction.getOwner() == ePlayer) &&
                                    ((bReverse) ? (BuildingServer.isConnectionValid(pAdjacentConstruction.getType(), eBuilding, this, pPlayer, gameClient(), infos())) :
                                                  (BuildingServer.isConnectionValid(eBuilding, pAdjacentConstruction.getType(), pAdjacentTile, pPlayer, gameClient(), infos()))))
                                {
                                    iCount++;
                                }
                            }
                            else if (bCountConstructions && (pAdjacentTile.getUnitMissionCount(ePlayer, MissionType.CONSTRUCT) > 0))
                            {
                                for (int i = 0; i < pPlayer.getNumUnits(); i++)
                                {
                                    UnitClient pLoopUnit = gameClient().unitClient(pPlayer.getUnitList()[i]);

                                    if (pLoopUnit.getMissionInfo().miData == pAdjacentTile.getID())
                                    {
                                        if (pLoopUnit.getMissionInfo().meMission == MissionType.CONSTRUCT)
                                        {
                                            if ((bReverse) ? (BuildingServer.isConnectionValid(pLoopUnit.getConstructionType(), eBuilding, this, pPlayer, gameClient(), infos())) :
                                                             (BuildingServer.isConnectionValid(eBuilding, pLoopUnit.getConstructionType(), pAdjacentTile, pPlayer, gameClient(), infos())))
                                            {
                                                iCount++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return iCount;
            }
        }
        public virtual int countConnections(BuildingType eBuilding, PlayerType ePlayer, bool bCountConstructions, bool bReverse)
        {
            return countConnections(eBuilding, ePlayer, bCountConstructions, bReverse, TeamType.NONE);
        }

        public virtual int countOtherBuildings(BuildingType eBuilding, PlayerType ePlayer)
        {
            using (new UnityProfileScope("Tile::countOtherBuildings"))
            {
                int iCount = 0;

                for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                {
                    TileClient pAdjacentTile = gameClient().mapClient().tileClientAdjacent(this, eLoopDirection);

                    if (pAdjacentTile != null)
                    {
                        if (pAdjacentTile.isBuilding())
                        {
                            BuildingClient pBuilding = pAdjacentTile.buildingClient();

                            if (pBuilding != null)
                            {
                                if ((pBuilding.getType() != eBuilding) || (pBuilding.getOwner() != ePlayer))
                                {
                                    iCount++;
                                }
                            }
                        }
                        else if (pAdjacentTile.isClaimed())
                        {
                            ConstructionClient pConstruction = pAdjacentTile.constructionClient();

                            if (pConstruction != null)
                            {
                                if ((pConstruction.getType() != eBuilding) || (pConstruction.getOwner() != ePlayer))
                                {
                                    iCount++;
                                }
                            }
                        }
                    }
                }

                return iCount;
            }
        }

        public virtual int countEmptyUsableTiles()
        {
            int iCount = 0;

            for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
            {
                TileClient pAdjacentTile = gameClient().mapClient().tileClientAdjacent(this, eLoopDirection);

                if (pAdjacentTile != null)
                {
                    if (pAdjacentTile.usable() && pAdjacentTile.isEmpty())
                    {
                        iCount++;
                    }
                }
            }

            return iCount;
        }

        public virtual int getID()
        {
            return miID;
        }
        public virtual int getX()
        {
            return miTileX;
        }
        public virtual int getY()
        {
            return miTileY;
        }
        public virtual Vector3 getWorldPosition()
        {
            Vector3 position = getWorldPosition2D(miTileX, miTileY, mGame);
            position.y = mGame.getPlateauHeight(getHeight());
            position.y -= 2.5f * getTerrainInfo().IsCrater;
            return position;
        }
        public static Vector3 getTerrainPosition(int iIndex, GameClient game)
        {
            return getTerrainPosition(iIndex, game.mapClient());
        }
        public static Vector3 getTerrainPosition(int iIndex, MapClient map)
        {
            Vector3 position = getTerrainPosition2D(map.getTerrainInfoX(iIndex), map.getTerrainInfoY(iIndex), map);//zzz
            if (Utils.inRange(iIndex, 0, map.numTerrains()))
            {
                position.y = map.getPlateauHeight(map.terrainInfo(iIndex).Height);
            }
            position.y -= 2.5f * map.terrainInfo(iIndex).IsCrater;
            return position;
        }

        public virtual int getTerrainID()
        {
            return miTerrainID;
        }
        public virtual TerrainInfo getTerrainInfo()
        {
            return mTerrainInfoCache ?? gameClient().mapClient().terrainInfo(miTerrainID);
        }
        public virtual InfoTerrain getInfoTerrain()
        {
            return mInfoTerrainCache ?? infos().terrain(getTerrain());
        }
        public virtual TerrainType getTerrain()
        {
            return getTerrainInfo().Terrain;
        }
        public virtual TerrainType getTerrainNoIce()
        {
            if (getIce() == IceType.NONE)
            {
                return getTerrain();
            }
            else
            {
                return TerrainType.NONE;
            }
        }
        public virtual bool usable()
        {
            InfoTerrain pTerrainInfo = mInfoTerrainCache ?? infos().terrain(getTerrain());
            return pTerrainInfo.mbUsable;
        }
        public virtual bool noResources()
        {
            InfoTerrain pTerrainInfo = mInfoTerrainCache ?? infos().terrain(getTerrain());
            return pTerrainInfo.mbNoResources;
        }
        public virtual bool isTerrainRate()
        {
            InfoTerrain pTerrainInfo = mInfoTerrainCache ?? infos().terrain(getTerrain());
            return pTerrainInfo.mbAnyResourceRate;
        }
        public virtual HeightType getHeight()
        {
            TerrainInfo pTerrainInfo = mTerrainInfoCache ?? gameClient().mapClient().terrainInfo(miTerrainID);
            return pTerrainInfo.Height;
        }
        public virtual WindType getWind()
        {
            TerrainInfo pTerrainInfo = mTerrainInfoCache ?? gameClient().mapClient().terrainInfo(miTerrainID);
            return pTerrainInfo.Wind;
        }
        public virtual IceType getIce()
        {
            TerrainInfo pTerrainInfo = mTerrainInfoCache ?? gameClient().mapClient().terrainInfo(miTerrainID);
            return pTerrainInfo.Ice;
        }

        //returns the XZ position of the tile grid coordinate, without clamping to the map
        public static Vector3 getWorldPosition2D(int iX, int iY, GameClient game)
        {
            return getWorldPosition2D(iX, iY, game.mapClient());
        }
        public static Vector3 getWorldPosition2D(int iX, int iY, MapClient map)
        {
            return getWorldPosition2D(iX, iY, map.getMapWidth(), map.getMapLength());
        }

        public static Vector3 getWorldPosition2D(int iX, int iY, int mapWidth, int mapHeight)
        {
            Vector3 position;
            float xOffset = ((iY & 1) == 0) ? (Constants.TILE_WIDTH * .5f) : 0.0f;
            position.x = ((iX - (mapWidth >> 1)) * Constants.TILE_WIDTH) + xOffset;
            position.y = 0;
            position.z = (iY - (mapHeight >> 1)) * Constants.ROW_OFFSET;
            return position;
        }

        public static Vector3 getTerrainPosition2D(int iX, int iY, MapClient map)
        {
            return getWorldPosition2D(iX - map.getMapEdgeTilePadding(), iY - map.getMapEdgeTilePadding(), map);
        }

        private static Vector3[] adjacentTileOffsets = null;
        public static Vector3 getAdjacentTileOffset(DirectionType eDirection)
        {
            if (adjacentTileOffsets == null) //initialize
            {
                adjacentTileOffsets = new Vector3[(int)DirectionType.NUM_TYPES];
                Vector3 eastOffset = Vector3.right * Constants.TILE_WIDTH;
                for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                {
                    Quaternion rotation = getEdgeRotation(eLoopDirection);
                    adjacentTileOffsets[(int)eLoopDirection] = rotation * eastOffset;
                }
            }

            return adjacentTileOffsets[(int)eDirection];
        }

        //offset vector to the corner between eDirection and eDirection+1
        private static Vector3[] cornerTileOffsets = null;
        public static Vector3 getCornerTileOffset(DirectionType eDirection)
        {
            if(cornerTileOffsets == null) //initialize
            {
                cornerTileOffsets = new Vector3 [(int)DirectionType.NUM_TYPES];
                Vector3 northCorner = new Vector3(0, 0, 0.5f * Constants.TILE_HEIGHT);
                for(DirectionType eLoopDirection=0; eLoopDirection<DirectionType.NUM_TYPES; eLoopDirection++)
                {
                    Quaternion rotation = Quaternion.Euler(0, (int)eLoopDirection * 60, 0); //due north returns identity
                    cornerTileOffsets[(int)eLoopDirection] = rotation * northCorner;
                }
            }
            return cornerTileOffsets[(int)eDirection];
        }

        private static Quaternion[] edgeRotations = null;
        public static Quaternion getEdgeRotation(DirectionType eDirection) //East returns identity
        {
            if (edgeRotations == null) //initialize
            {
                edgeRotations = new Quaternion[(int)DirectionType.NUM_TYPES];
                for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                {
                    int turnsFromEast = ((int)eLoopDirection + (int)DirectionType.NUM_TYPES - (int)DirectionType.E) % (int)DirectionType.NUM_TYPES;
                    edgeRotations[(int)eLoopDirection] = Quaternion.Euler(0, turnsFromEast * 60, 0); //60 degrees clockwise per turn
                }
            }

            return edgeRotations[(int)eDirection];
        }

        public static DirectionType GetClosestDirectionType(Vector3 direction)
        {
            Vector3 normalizedDirection = Vector3.Normalize(direction);
            DirectionType bestDirection = DirectionType.NONE;
            float bestDot = 0;

            for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
            {
                Vector3 directionTypeVector = Vector3.Normalize(getAdjacentTileOffset(eLoopDirection));
                float dotProduct = Vector3.Dot(normalizedDirection, directionTypeVector);

                if (dotProduct > bestDot || bestDirection == DirectionType.NONE)
                {
                    bestDirection = eLoopDirection;
                    bestDot = dotProduct;
                }
            }
            return bestDirection;
        }

        public static TileClient TileClientFromWorldPosition(Vector3 position, GameClient pGame)
        {
            int tileID = IDFromWorldPosition(position, pGame);

            if (tileID == -1)
                return null;

            return pGame.tileClient(tileID);
        }

        public static int IDFromWorldPosition(Vector3 position, GameClient pGame)
        {
            if (pGame == null)
            {
                return -1;
            }

            //remove any height information
            position.y = 0;

            //Note: this code has to exactly match FogOfWarShader::tileFromWorldPosition()
            //find an tile close to this position
            int centerY = Mathf.RoundToInt(position.z / Constants.ROW_OFFSET + (pGame.getMapHeight() >> 1));
            float xOffset = ((centerY & 1) == 0) ? (Constants.TILE_WIDTH * .5f) : 0.0f;
            int centerX = Mathf.RoundToInt((position.x - xOffset) / Constants.TILE_WIDTH + (pGame.getMapWidth() >> 1));
            int centerIndex = pGame.mapClient().tileIDGrid(centerX, centerY);
            if (centerIndex < 0)
            {
                return -1;
            }

            //search adjacent tiles and return closest match
            TileClient centerTile = pGame.tileClient(centerIndex);
            float closestDistance = Utils.getDistance2D(centerTile.getWorldPosition(), position);
            TileClient closestTile = centerTile;
            foreach (TileClient pAdjacentTile in pGame.tileClientAdjacentAll(centerTile))
            {
                float adjacentDistance = Utils.getDistance2D(pAdjacentTile.getWorldPosition(), position);
                if (adjacentDistance < closestDistance)
                {
                    closestDistance = adjacentDistance;
                    closestTile = pAdjacentTile;
                }
            }

            return closestTile.getID();
        }

        public virtual int getTileGroupID()
        {
            return miTileGroupID;
        }
        protected virtual bool isTileGroup()
        {
            return (miTileGroupID != -1);
        }

        public virtual int getModuleID()
        {
            return miModuleID;
        }
        public virtual bool isModule()
        {
            return (miModuleID != -1);
        }
        public virtual ModuleClient moduleClient()
        {
            if (isModule())
            {
                return gameClient().moduleClient(getModuleID());
            }
            else
            {
                return null;
            }
        }
        public virtual ModuleType getModule()
        {
            return moduleClient() != null ? moduleClient().getType() : ModuleType.NONE;
        }

        public virtual int getHQID()
        {
            return miHQID;
        }
        public virtual bool isHQ()
        {
            return (miHQID != -1);
        }
        public virtual HQClient hqClient()
        {
            if (isHQ())
            {
                return gameClient().hqClient(getHQID());
            }
            else
            {
                return null;
            }
        }

        public virtual int getConstructionID()
        {
            return miConstructionID;
        }
        public virtual bool isConstruction()
        {
            return (getConstructionID() != -1);
        }
        public virtual ConstructionClient constructionClient()
        {
            if (isConstruction())
            {
                return gameClient().constructionClient(getConstructionID());
            }
            else
            {
                return null;
            }
        }
        public virtual BuildingType getConstructionType()
        {
            return isConstruction() ? constructionClient().getType() : BuildingType.NONE;
        }

        public virtual int getBuildingID()
        {
            return miBuildingID;
        }
        public virtual bool isBuilding()
        {
            return (getBuildingID() != -1);
        }
        public virtual BuildingClient buildingClient()
        {
            if (isBuilding())
            {
                return gameClient().buildingClient(getBuildingID());
            }
            else
            {
                return null;
            }
        }
        public virtual BuildingType getBuildingType()
        {
            return isBuilding() ? buildingClient().getType() : BuildingType.NONE;
        }

        public virtual int getUnitID()
        {
            return miUnitID;
        }
        public virtual bool isUnit()
        {
            return (getUnitID() != -1);
        }
        public virtual UnitClient unitClient()
        {
            if (isUnit())
            {
                return gameClient().unitClient(getUnitID());
            }
            else
            {
                return null;
            }
        }

        public virtual BuildingType getConstructionOrBuildingType()
        {
            if (isConstruction())
            {
                return getConstructionType();
            }
            else if (isBuilding())
            {
                return getBuildingType();
            }

            return BuildingType.NONE;
        }
        public virtual BuildingType getVisibleConstructionOrBuildingType(TeamType eTeam)
        {
            if (isConstruction())
            {
                return constructionClient().getVisibleType(eTeam);
            }
            else if (isBuilding())
            {
                return buildingClient().getVisibleType(eTeam);
            }

            return BuildingType.NONE;
        }

        public virtual int getFrozenTime()
        {
            return miFrozenTime;
        }
        public virtual bool isFrozen()
        {
            return (getFrozenTime() > 0);
        }

        public virtual int getDoubleTime()
        {
            return miDoubleTime;
        }
        public virtual bool isDoubleAlways(TeamType eTeam)
        {
            if (isClaimed())
            {
                BuildingType eBuilding = getVisibleConstructionOrBuildingType(eTeam);

                if (eBuilding != BuildingType.NONE)
                {
                    if (ownerClient().isBuildingClassBoost(infos().building(eBuilding).meClass))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        public virtual bool isDouble()
        {
            return (isDoubleAlways(TeamType.NONE) || (getDoubleTime() > 0));
        }

        public virtual int getHalfTime()
        {
            return miHalfTime;
        }
        public virtual bool isHalf()
        {
            return (getHalfTime() > 0);
        }

        public virtual int getOverloadTime()
        {
            return miOverloadTime;
        }
        public virtual bool isOverload()
        {
            return (getOverloadTime() > 0);
        }

        public virtual int getVirusTime()
        {
            return miVirusTime;
        }
        public virtual bool isVirus()
        {
            return (getVirusTime() > 0);
        }

        public virtual int getTakeoverTime()
        {
            return miTakeoverTime;
        }
        public virtual bool isTakeover()
        {
            return (getTakeoverTime() > 0);
        }

        public virtual int getDefendTime()
        {
            return miDefendTime;
        }
        public virtual bool isDefend()
        {
            return (getDefendTime() > 0);
        }

        public virtual int getDestroyTime()
        {
            return miDestroyTime;
        }

        public virtual int getClaimBlockTime()
        {
            return miClaimBlockTime;
        }
        public virtual bool isClaimBlock(PlayerType ePlayer = PlayerType.NONE)
        {
            if (miClaimBlockTime > 0)
            {
                if ((ePlayer == PlayerType.NONE) || (meClaimBlockPlayer != ePlayer))
                {
                    return true;
                }
            }

            return false;
        }

        public virtual int getFirstClaimTurn()
        {
            return miFirstClaimTurn;
        }

        public virtual int getResourceCount()
        {
            return miResourceCount;
        }
        public virtual bool hasResources()
        {
            return (getResourceCount() > 0);
        }
        public virtual bool hasResourcesAdjacent()
        {
            InfoTerrain pTerrainInfo = mInfoTerrainCache ?? infos().terrain(getTerrain());

            if (pTerrainInfo.mbAdjacentResource)
            {
                for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
                {
                    TileClient pAdjacentTile = gameClient().mapClient().tileClientAdjacent(this, eDirection);

                    if (pAdjacentTile != null)
                    {
                        if (pAdjacentTile.hasResources())
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public virtual bool isGeothermal()
        {
            return mbGeothermal;
        }

        public virtual bool isConnectedToHQ()
        {
            return mbConnectedToHQ;
        }

        public virtual bool isHologram()
        {
            return mbHologram;
        }
        public virtual bool isShowWrongBuilding()
        {
            if (gameClient().isGameOption(GameOptionType.ADVANCED_SABOTAGE))
            {
                BuildingType eBuilding = getConstructionOrBuildingType();

                if (eBuilding != BuildingType.NONE)
                {
                    if (infos().building(eBuilding).mbShowWrongBuilding)
                    {
                        if (!isVirus())
                        {
                            return true;
                        }
                    }
                }
            }

            return isHologram();
        }

        protected virtual bool isRevealDefendSabotage()
        {
            return mbRevealDefendSabotage;
        }
        public virtual bool isRevealDefendSabotage(TeamType eIndex)
        {
            if (getTeam() == eIndex)
            {
                return true;
            }

            if (isRevealDefendSabotage())
            {
                return true;
            }

            if (isRevealBuildingAdjacent(eIndex))
            {
                return true;
            }

            return false;
        }

        public virtual bool isModuleRevealed()
        {
            return mbModuleRevealed;
        }

        public virtual bool isWasAuctioned()
        {
            return mbWasAuctioned;
        }

        public virtual PlayerType getOwner()
        {
            return meOwner;
        }
        public virtual PlayerType getRealOwner()
        {
            return meRealOwner;
        }
        public virtual bool isClaimed()
        {
            return (meOwner != PlayerType.NONE);
        }
        public virtual bool isRealClaimed()
        {
            return (meRealOwner != PlayerType.NONE);
        }
        public virtual bool isOwnerReal()
        {
            return (meOwner == meRealOwner);
        }
        public virtual PlayerClient ownerClient()
        {
            if (isClaimed())
            {
                return gameClient().playerClient(getOwner());
            }
            else
            {
                return null;
            }
        }
        public virtual PlayerClient realOwnerClient()
        {
            if (isRealClaimed())
            {
                return gameClient().playerClient(getRealOwner());
            }
            else
            {
                return null;
            }
        }
        public virtual TeamType getTeam()
        {
            if (isClaimed())
            {
                return ownerClient().getTeam();
            }
            else
            {
                return TeamType.NONE;
            }
        }
        public virtual TeamType getRealTeam()
        {
            if (isRealClaimed())
            {
                return gameClient().playerClient(getRealOwner()).getTeam();
            }
            else
            {
                return TeamType.NONE;
            }
        }

        public virtual PlayerType getClaimBlockPlayer()
        {
            return meClaimBlockPlayer;
        }

        public virtual SabotageType getFrozenSabotageType()
        {
            return meFrozenSabotage;
        }

        public virtual SabotageType getDefendSabotage()
        {
            return meDefendSabotage;
        }

        public virtual BuildingType getLastBuilding()
        {
            return meLastBuilding;
        }

        public virtual BuildingType getVisibleBuilding()
        {
            return meVisibleBuilding;
        }

        public virtual BuildingType getHologramBuilding()
        {
            return meHologramBuilding;
        }

        public virtual TerrainType getTerrainRegion()
        {
            return meTerrainRegion;
        }

        public virtual int getSabotagedCount(SabotageType eIndex)
        {
            return maiSabotagedCount[(int)eIndex];
        }

        public virtual int getResourceMined(ResourceType eIndex)
        {
            return maiResourceMined[(int)eIndex];
        }

        public virtual bool isPotentialHQConnection(PlayerType eIndex)
        {
            return mabPotentialHQConnection[(int)eIndex];
        }

        public virtual bool isRevealBuilding(TeamType eIndex)
        {
            return mabRevealBuilding[(int)eIndex];
        }
        public virtual bool isRevealBuildingAdjacent(TeamType eIndex)
        {
            if (isRevealBuilding(eIndex))
            {
                return true;
            }

            for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
            {
                TileClient pAdjacentTile = gameClient().mapClient().tileClientAdjacent(this, eDirection);

                if (pAdjacentTile != null)
                {
                    if (pAdjacentTile.isRevealBuilding(eIndex))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        public virtual VisibilityType getVisibility(PlayerType eIndex)
        {
            return maeVisibility[(int)eIndex];
        }

        public virtual List<ResourceLevelType> getResourceLevels()
        {
            return maeResourceLevel;
        }
        public virtual TileClient getResourceAdjacentTile(ResourceType eIndex)
        {
            TileClient pBestTile = this;
            ResourceLevelType eBestResourceLevel = getResourceLevel(eIndex, false);
            int iBestResourceMined = getResourceMined(eIndex);

            for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
            {
                TileClient pAdjacentTile = gameClient().mapClient().tileClientAdjacent(this, eLoopDirection);

                if (pAdjacentTile != null)
                {
                    ResourceLevelType eAdjacentResourceLevel = pAdjacentTile.getResourceLevel(eIndex, false);
                    int iAdjacentResourceMined = pAdjacentTile.getResourceMined(eIndex);

                    if ((eAdjacentResourceLevel > eBestResourceLevel) || ((eAdjacentResourceLevel == eBestResourceLevel) && (iAdjacentResourceMined < iBestResourceMined)))
                    {
                        pBestTile = pAdjacentTile;
                        eBestResourceLevel = eAdjacentResourceLevel;
                        iBestResourceMined = iAdjacentResourceMined;
                    }
                }
            }

            return pBestTile;
        }
        public virtual ResourceLevelType getResourceLevel(ResourceType eIndex, bool bAdjacent)
        {
            if (bAdjacent)
            {
                return getResourceAdjacentTile(eIndex).getResourceLevel(eIndex, false);
            }

            return maeResourceLevel[(int)eIndex];
        }
        public virtual ResourceLevelType getResourceLevelAdjacent(ResourceType eIndex, PlayerType ePlayer)
        {
            return getResourceLevel(eIndex, ((ePlayer != PlayerType.NONE) ? isAdjacentMining(ePlayer) : false));
        }
        public virtual bool isResourceRateAny()
        {
            InfoTerrain pTerrainInfo = mInfoTerrainCache ?? infos().terrain(getTerrain());
            return pTerrainInfo.mbAnyResourceRate;
        }
        public virtual int getPotentialResourceRate(ResourceType eResource, int iIceModifier = 0)
        {
            InfoTerrain pTerrainInfo = mInfoTerrainCache ?? infos().terrain(getTerrain());
            int iRate = pTerrainInfo.maiResourceRate[(int)eResource];

            ResourceLevelType eResourceLevel = getResourceLevel(eResource, false);

            if (eResourceLevel > ResourceLevelType.NONE)
            {
                int iMining = Constants.RESOURCE_MULTIPLIER;

                iMining *= mInfos.resourceLevel(eResourceLevel).miRateMultiplier;
                iMining /= 100;

                iRate += iMining;
            }

            IceType eIce = getIce();
            if (eIce != IceType.NONE)
            {
                int iValue = mInfos.ice(eIce).maiAverageResourceRate[(int)eResource];

                iValue *= iIceModifier + 100;
                iValue /= 100;

                iRate += iValue;
            }

            return iRate;
        }
        public bool isAdjacentMining(PlayerType ePlayer)
        {
            InfoTerrain pTerrainInfo = mInfoTerrainCache ?? infos().terrain(getTerrain());

            if (pTerrainInfo.mbAdjacentResource)
            {
                return true;
            }

            if (ePlayer != PlayerType.NONE)
            {
                if (gameClient().playerClient(ePlayer).isAdjacentMining())
                {
                    return true;
                }
            }

            return false;
        }
        public virtual bool supplySelfInput(ResourceType eResource, bool bAdjacentMining)
        {
            if (getResourceLevel(eResource, bAdjacentMining) > ResourceLevelType.NONE)
            {
                return true;
            }

            InfoTerrain pTerrainInfo = mInfoTerrainCache ?? infos().terrain(getTerrain());
            if (pTerrainInfo.maiResourceRate[(int)eResource] > 0)
            {
                return true;
            }

            {
                IceType eIce = getIce();

                if (eIce != IceType.NONE)
                {
                    if (infos().ice(eIce).maiAverageResourceRate[(int)eResource] > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        public bool supplySelfInputPlayer(ResourceType eResource, PlayerType ePlayer)
        {
            return supplySelfInput(eResource, isAdjacentMining(ePlayer));
        }

        public virtual int getUnitMissionCount(PlayerType eIndex1, MissionType eIndex2)
        {
            return maaiUnitMissionCount[(int)eIndex1][(int)eIndex2];
        }
        public virtual int getTotalUnitMissionCount(MissionType eIndex)
        {
            int iCount = 0;

            for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameClient().getNumPlayers(); eLoopPlayer++)
            {
                iCount += getUnitMissionCount(eLoopPlayer, eIndex);
            }

            return iCount;
        }
    }

    public class TileServer : TileClient
    {
        protected virtual GameServer gameServer()
        {
            return (GameServer)mGame;
        }

        protected virtual void makeDirty(TileDirtyType eType)
        {
            mDirtyBits.SetBit((int)eType, true);

            gameServer().mapServer().makeDirtyTiles();
        }
        protected virtual void makeAllDirty()
        {
            for (TileDirtyType eLoopType = 0; eLoopType < TileDirtyType.NUM_TYPES; eLoopType++)
            {
                if ((eLoopType != TileDirtyType.maiSabotagedCount) &&
                    (eLoopType != TileDirtyType.maiResourceMined) &&
                    (eLoopType != TileDirtyType.mabPotentialHQConnection) &&
                    (eLoopType != TileDirtyType.mabRevealBuilding) &&
                    (eLoopType != TileDirtyType.maeVisibility) &&
                    (eLoopType != TileDirtyType.maeResourceLevel) &&
                    (eLoopType != TileDirtyType.maaiUnitMissionCount) &&
                    (eLoopType != TileDirtyType.meTerrain))
                {
                    makeDirty(eLoopType);
                }
            }
        }
        public virtual void clearDirty()
        {
            mDirtyBits.Clear();
        }

        int miArea = -1;

        List<int> maiHQValue = new List<int>();

        List<bool> mabHQMinimum = new List<bool>();

        public TileServer(GameClient pGame)
            : base(pGame)
        {
        }

        public virtual void init(GameServer game, int iID)
        {
            mGame = game;
            mInfos = game.infos();

            miID = iID;
            updateTileXY();

            MapClient map = mGame.mapClient();
            miTerrainID = map.terrainInfoIDGrid(getX() + map.getMapEdgeTilePadding(), getY() + map.getMapEdgeTilePadding());

            initVariablesClient();

            initVariablesServer();

            makeAllDirty();
        }

        protected virtual void initVariablesServer()
        {
            for (HQType eLoopHQ = 0; eLoopHQ < infos().HQsNum(); eLoopHQ++)
            {
                maiHQValue.Add(0);
            }

            for (HQType eLoopHQ = 0; eLoopHQ < infos().HQsNum(); eLoopHQ++)
            {
                mabHQMinimum.Add(false);
            }
        }

        protected virtual void SerializeServer(object stream)
        {
            SimplifyIO.Data(stream, ref miArea, "Area");

            SimplifyIO.Data(stream, ref maiHQValue, "HQValue");

            SimplifyIO.Data(stream, ref mabHQMinimum, "HQMinium");
        }

        public virtual void writeServerValues(BinaryWriter stream, bool bAll, int compatibilityNumber)
        {
            writeClientValues(stream, bAll, compatibilityNumber);
            SerializeServer(stream);
        }

        public virtual void readServerValues(BinaryReader stream, bool bAll, int compatibilityNumber)
        {
            readClientValues(stream, bAll, compatibilityNumber);
            SerializeServer(stream);
        }

        public virtual void rebuildFromClient()
        {
            initVariablesServer();

            miTileGroupID = -1;
        }

        public virtual void doTurn()
        {
            if (getDestroyTime() > 0)
            {
                changeDestroyTime(-1);

                if (getDestroyTime() == 0)
                {
                    if (isBuilding())
                    {
                        BuildingServer pBuilding = buildingServer();

                        pBuilding.ownerServer().destroyBuilding(pBuilding, 100, true);
                    }
                }
            }

            doVisibleBuilding();

            if (isModule())
            {
                moduleServer().doTurn();
            }

            if (isTakeover())
            {
                changeTakeoverTime(-1);
            }

            if (!isTakeover() && !isOwnerReal())
            {
                setOwner(getRealOwner(), true);
                setDefendTime(infos().Globals.TAKEOVER_DEFEND_TIME);
            }

            if (getFrozenTime() > 0)
            {
                changeFrozenTime(-1);
            }

            if (getDoubleTime() > 0)
            {
                changeDoubleTime(-1);
            }

            if (getHalfTime() > 0)
            {
                changeHalfTime(-1);
            }

            if (getOverloadTime() > 0)
            {
                changeOverloadTime(-1);
            }

            if (getVirusTime() > 0)
            {
                changeVirusTime(-1);
            }

            if (getDefendTime() > 0)
            {
                changeDefendTime(-1);
            }

            if (getClaimBlockTime() > 0)
            {
                changeClaimBlockTime(-1);
            }

            if (isBuilding() && (infos().buildingClass(infos().building(getBuildingType()).meClass).meOrderType == OrderType.LAUNCH))
            {
                if (getDefendSabotage() == SabotageType.NONE)
                {
                    if ((isDefend() && (getDefendTime() < 10)) || (gameServer().random().Next(30) == 0))
                    {
                        ownerServer().AI_doBlackMarketDefendBuilding(this);
                    }
                }
            }
        }

        public virtual void createResource(ResourceType eResource)
        {
            if (this.getResourceLevel(eResource, false) == ResourceLevelType.NONE)
            {
                InfoTerrain pTerrainInfo = mInfoTerrainCache ?? infos().terrain(getTerrain());
                int iRoll = gameServer().random().Next(100) + pTerrainInfo.maaiLevelRollChange[(int)(gameServer().getLocation())][(int)eResource];

                for (ResourceLevelType eResourceLevel = 0; eResourceLevel < (infos().resourceLevelsNum() - 1); eResourceLevel++)
                {
                    if (!(infos().resourceLevel(eResourceLevel).mabLocationInvalid[(int)(gameServer().getLocation())]))
                    {
                        if (iRoll < infos().resource(eResource).maiLevelRoll[(int)eResourceLevel] + infos().resource(eResource).maaiLocationLevelRollModifier[(int)gameServer().getLocation()][(int)eResourceLevel])
                        {
                            setResourceLevel(eResource, eResourceLevel);
                            return;
                        }
                    }
                }

                setResourceLevel(eResource, gameServer().getHighestResourceLevel());
            }
            else
            {
                if (!(infos().resourceLevel(this.getResourceLevel(eResource, false) + 1).mabLocationInvalid[(int)(gameServer().getLocation())]))
                {
                    if (gameServer().random().Next(50) == 0)
                    {
                        setResourceLevel(eResource, this.getResourceLevel(eResource, false) + 1);
                        return;
                    }
                }
            }
        }

        public void changeTerrain(TerrainType eTerrain)
        {
            MapClient map = gameServer().mapClient();
            map.terrainInfo(miTerrainID).Terrain = eTerrain;

            makeDirty(TileDirtyType.meTerrain);
            gameServer().updateTerrainCache();

            if (eTerrain == infos().Globals.CAVE_TERRAIN)
            {
                gameServer().gameEventsServer().AddTerrainChange(this.getID());
            }
        }

        public virtual TileGroupServer tileGroup()
        {
            return gameServer().tileGroup(miTileGroupID);
        }
        public virtual void setTileGroup(TileGroupServer pNewValue)
        {
            TileGroupServer pOldValue = tileGroup();

            if (pOldValue != pNewValue)
            {
                miTileGroupID = ((pNewValue != null) ? pNewValue.getID() : -1);

                if (pOldValue != null)
                {
                    pOldValue.removeTile(this);
                }
                if (pNewValue != null)
                {
                    pNewValue.addTile(this);
                }

                makeDirty(TileDirtyType.miTileGroupID);
            }
        }
        public virtual void updateTileGroup()
        {
            TileGroupServer pOldTileGroup = tileGroup();

            if (pOldTileGroup != null)
            {
                pOldTileGroup.resetGroup();
            }
            else if (isClaimed())
            {
                TileGroupServer pNewTileGroup = null;

                foreach (TileServer pAdjacentTile in gameServer().tileServerAdjacentAll(this))
                {
                    if (pAdjacentTile.getHeight() == getHeight())
                    {
                        if (pAdjacentTile.getOwner() == getOwner())
                        {
                            if (pNewTileGroup == null)
                            {
                                pNewTileGroup = pAdjacentTile.tileGroup();
                            }
                            else
                            {
                                TileGroupServer pAdjacentTileGroup = pAdjacentTile.tileGroup();

                                if (pAdjacentTileGroup != null)
                                {
                                    pAdjacentTileGroup.join(pNewTileGroup);
                                }
                            }
                        }
                    }
                }

                if (pNewTileGroup == null)
                {
                    pNewTileGroup = createTileGroup();
                }

                setTileGroup(pNewTileGroup);
            }
        }
        protected virtual TileGroupServer createTileGroup()
        {
            TileGroupServer pTileGroup = Globals.Factory.createTileGroupServer(gameServer());
            pTileGroup.init(gameServer(), gameServer().nextTileGroupID());
            return pTileGroup;
        }

        public virtual ModuleServer moduleServer()
        {
            return (ModuleServer)moduleClient();
        }
        public virtual void setModule(ModuleServer newValue)
        {
            if (moduleServer() != newValue)
            {
                miModuleID = ((newValue != null) ? newValue.getID() : -1);

                if (isModule())
                {
                    removeUnit();

                    foreach (InfoResource pLoopResource in infos().resources())
                    {
                        setResourceLevel(pLoopResource.meType, ResourceLevelType.NONE);
                    }

                    for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameServer().getNumPlayers(); eLoopPlayer++)
                    {
                        increaseVisibility(eLoopPlayer, VisibilityType.VISIBLE, true);
                    }

                    makeModuleRevealed();

                    foreach (TileServer pAdjacentTile in gameServer().tileServerAdjacentAll(this))
                    {
                        for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameServer().getNumPlayers(); eLoopPlayer++)
                        {
                            pAdjacentTile.increaseVisibility(eLoopPlayer, VisibilityType.VISIBLE, true);
                        }

                        pAdjacentTile.makeModuleRevealed();

                        if (pAdjacentTile.isRealClaimed())
                        {
                            BuildingServer pBuilding = pAdjacentTile.buildingServer();

                            if (pBuilding != null)
                            {
                                pBuilding.tileServer().realOwnerServer().handleBuildingAchievements(pBuilding.getType(), pAdjacentTile);
                            }
                        }
                    }
                }

                makeDirty(TileDirtyType.miModuleID);
            }
        }

        public virtual HQServer hqServer()
        {
            return (HQServer)hqClient();
        }
        public virtual void setHQ(HQServer newValue)
        {
            using (new UnityProfileScope("TileServer.setHQ"))
            {
                if (hqServer() != newValue)
                {
                    miHQID = ((newValue != null) ? newValue.getID() : -1);

                    if (isTileGroup())
                    {
                        tileGroup().updateHQ();
                    }

                    if (isHQ())
                    {
                        removeUnit();
                    }

                    makeDirty(TileDirtyType.miHQID);
                }
            }
        }

        public virtual ConstructionServer constructionServer()
        {
            return (ConstructionServer)constructionClient();
        }
        public virtual void addConstruction(ConstructionServer newValue)
        {
            miConstructionID = newValue.getID();

            makeDirty(TileDirtyType.miConstructionID);
        }
        public virtual void removeConstruction(ConstructionServer oldValue)
        {
            if (constructionServer() == oldValue)
            {
                miConstructionID = -1;

                makeDirty(TileDirtyType.miConstructionID);
            }
        }

        public virtual BuildingServer buildingServer()
        {
            return (BuildingServer)buildingClient();
        }
        public virtual void setBuilding(BuildingServer pNewValue)
        {
            if (buildingServer() != pNewValue)
            {
                miBuildingID = ((pNewValue != null) ? pNewValue.getID() : -1);

                if (isBuilding())
                {
                    setLastBuilding(pNewValue.getType());

                    removeUnit();
                }

                makeDirty(TileDirtyType.miBuildingID);
            }
        }
        public virtual void removeBuilding(BuildingServer pOldValue)
        {
            if (buildingServer() == pOldValue)
            {
                miBuildingID = -1;

                makeDirty(TileDirtyType.miBuildingID);
            }
        }

        public virtual UnitServer unitServer()
        {
            return (UnitServer)unitClient();
        }
        public virtual void setUnit(UnitServer pNewValue)
        {
            if (unitServer() != pNewValue)
            {
                miUnitID = ((pNewValue != null) ? pNewValue.getID() : -1);

                makeDirty(TileDirtyType.miUnitID);
            }
        }
        protected virtual void removeUnit()
        {
            UnitServer pUnit = unitServer();

            if (pUnit != null)
            {
                pUnit.ownerServer().killUnit(pUnit, false, false);

                gameServer().gameEventsServer().AddPirateDestroyed(pUnit.getOwner(), pUnit.getType(), getID());

                if (isClaimed() && isBuilding())
                {
                    if (ownerServer().isHuman())
                    {
                        gameServer().gameEventsServer().AddAchievement(getOwner(), infos().getType<AchievementType>("ACHIEVEMENT_REMOVE_PIRATE"));
                    }
                }
            }
        }

        public virtual void setFrozenTime(int iNewValue)
        {
            int iOldValue = getFrozenTime();
            if (iOldValue != iNewValue)
            {
                miFrozenTime = iNewValue;

                if (iOldValue == 0)
                {
                    if (isBuilding())
                    {
                        buildingServer().sendResources(false);
                    }
                }
                else if (iNewValue == 0)
                {
                    setFrozenSabotage(SabotageType.NONE);
                }

                makeDirty(TileDirtyType.miFrozenTime);
            }
        }
        public virtual void changeFrozenTime(int iChange)
        {
            setFrozenTime(getFrozenTime() + iChange);
        }

        public virtual void setDoubleTime(int iNewValue)
        {
            if (getDoubleTime() != iNewValue)
            {
                miDoubleTime = iNewValue;

                makeDirty(TileDirtyType.miDoubleTime);
            }
        }
        public virtual void changeDoubleTime(int iChange)
        {
            setDoubleTime(getDoubleTime() + iChange);
        }

        public virtual void setHalfTime(int iNewValue)
        {
            if (getHalfTime() != iNewValue)
            {
                miHalfTime = iNewValue;

                makeDirty(TileDirtyType.miHalfTime);
            }
        }
        void changeHalfTime(int iChange)
        {
            setHalfTime(getHalfTime() + iChange);
        }

        public virtual void setOverloadTime(int iNewValue)
        {
            if (getOverloadTime() != iNewValue)
            {
                miOverloadTime = iNewValue;

                makeDirty(TileDirtyType.miOverloadTime);
            }
        }
        void changeOverloadTime(int iChange)
        {
            setOverloadTime(getOverloadTime() + iChange);
        }

        public virtual void setVirusTime(int iNewValue)
        {
            if (getVirusTime() != iNewValue)
            {
                miVirusTime = iNewValue;

                makeDirty(TileDirtyType.miVirusTime);
            }
        }
        void changeVirusTime(int iChange)
        {
            setVirusTime(getVirusTime() + iChange);
        }

        public virtual void setTakeoverTime(int iNewValue)
        {
            if (getTakeoverTime() != iNewValue)
            {
                miTakeoverTime = iNewValue;

                makeDirty(TileDirtyType.miTakeoverTime);
            }
        }
        public virtual void changeTakeoverTime(int iChange)
        {
            setTakeoverTime(getTakeoverTime() + iChange);
        }

        public virtual void setDefendTime(int iNewValue)
        {
            if (getDefendTime() != iNewValue)
            {
                miDefendTime = iNewValue;

                makeDirty(TileDirtyType.miDefendTime);
            }
        }
        public virtual void changeDefendTime(int iChange)
        {
            setDefendTime(getDefendTime() + iChange);
        }

        public virtual void setDestroyTime(int iNewValue)
        {
            if (getDestroyTime() != iNewValue)
            {
                miDestroyTime = iNewValue;

                makeDirty(TileDirtyType.miDestroyTime);
            }
        }
        public virtual void changeDestroyTime(int iChange)
        {
            setDestroyTime(getDestroyTime() + iChange);
        }

        public virtual void setClaimBlockTime(int iNewValue)
        {
            if (getClaimBlockTime() != iNewValue)
            {
                miClaimBlockTime = iNewValue;

                if (getClaimBlockTime() == 0)
                {
                    setClaimBlockPlayer(PlayerType.NONE);
                }

                makeDirty(TileDirtyType.miClaimBlockTime);
            }
        }
        public virtual void changeClaimBlockTime(int iChange)
        {
            setClaimBlockTime(getClaimBlockTime() + iChange);
        }

        public virtual void setFirstClaimTurn(int iNewValue)
        {
            if (getFirstClaimTurn() != iNewValue)
            {
                miFirstClaimTurn = iNewValue;

                makeDirty(TileDirtyType.miFirstClaimTurn);
            }
        }

        public virtual void changeResourceCount(int iChange)
        {
            if (iChange != 0)
            {
                miResourceCount += iChange;

                makeDirty(TileDirtyType.miResourceCount);
            }
        }

        public virtual void setGeothermal(bool bNewValue)
        {
            if (isGeothermal() != bNewValue)
            {
                mbGeothermal = bNewValue;

                gameServer().changeGeothermalCount((isGeothermal()) ? 1 : -1);

                makeDirty(TileDirtyType.mbGeothermal);
            }
        }

        public virtual void updateConnectedToHQ()
        {
            bool bNewValue = isTileGroup() && tileGroup().isHQ();

            if (mbConnectedToHQ != bNewValue)
            {
                mbConnectedToHQ = bNewValue;

                makeDirty(TileDirtyType.mbConnectedToHQ);
            }
        }

        public virtual void setHologram(bool bNewValue)
        {
            if (isHologram() != bNewValue)
            {
                mbHologram = bNewValue;

                makeDirty(TileDirtyType.mbHologram);
            }
        }

        public virtual void setRevealDefendSabotage(bool bNewValue)
        {
            if (isRevealDefendSabotage() != bNewValue)
            {
                mbRevealDefendSabotage = bNewValue;

                makeDirty(TileDirtyType.mbRevealDefendSabotage);
            }
        }

        public virtual void makeModuleRevealed()
        {
            if (!isModuleRevealed())
            {
                mbModuleRevealed = true;

                makeDirty(TileDirtyType.mbModuleRevealed);
            }
        }

        public virtual void makeWasAuctioned()
        {
            if (!isWasAuctioned())
            {
                mbWasAuctioned = true;

                makeDirty(TileDirtyType.mbWasAuctioned);
            }
        }

        public virtual PlayerServer ownerServer()
        {
            return (PlayerServer)ownerClient();
        }
        public virtual PlayerServer realOwnerServer()
        {
            return (PlayerServer)realOwnerClient();
        }

        public virtual void setOwner(PlayerType eNewValue, bool bReal)
        {
            using (new UnityProfileScope("Tile.setOwner"))
            {
                PlayerType eOldValue = getOwner();

                if ((eOldValue != eNewValue) || (bReal && (getRealOwner() != eNewValue)))
                {
                    if (bReal)
                    {
                        setRealOwner(eNewValue);
                    }

                    BuildingServer pBuilding = buildingServer();
                    ConstructionServer pConstruction = constructionServer();
                    HQServer pHQ = hqServer();

                    if (eOldValue != PlayerType.NONE)
                    {
                        if (pBuilding != null)
                        {
                            pBuilding.sendResources(true);
                        }

                        ownerServer().removeTile(this);
                    }

                    meOwner = eNewValue;

                    if (eNewValue != PlayerType.NONE)
                    {
                        ownerServer().addTile(this);

                        for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameServer().getNumPlayers(); eLoopPlayer++)
                        {
                            increaseVisibility(eLoopPlayer, VisibilityType.VISIBLE, false);

                            int iRange = gameServer().playerServer(eLoopPlayer).innerScanRange();

                            foreach (TileServer pRangeTile in gameServer().tileServerRangeIterator(this, iRange))
                            {
                                pRangeTile.increaseVisibility(eLoopPlayer, VisibilityType.REVEALED, false);
                            }
                        }
                    }

                    if ((pHQ != null) && (pHQ.tileServer() == this))
                    {
                        pHQ.setOwner(eNewValue);
                    }

                    if (pBuilding != null)
                    {
                        pBuilding.setOwner(eNewValue);
                        if (bReal && gameServer().playerServer(eNewValue).isSubsidiary())
                        {
                            OrderType eOrder = infos().buildingClass(infos().building(pBuilding.getType()).meClass).meOrderType;
                            if (eOrder == OrderType.HACK || eOrder == OrderType.PATENT || eOrder == OrderType.RESEARCH)
                                pBuilding.scrap();
                        }

                        if (pBuilding.isOff())
                        {
                            pBuilding.toggleOff();
                        }

                        pBuilding.ownerServer().AI_forceBuildingOrder(pBuilding);
                    }

                    if (pConstruction != null)
                    {
                        pConstruction.setOwner(eNewValue);
                    }

                    setDefendTime(0);
                    setDefendSabotage(SabotageType.NONE);

                    updateTileGroup();

                    gameServer().updateConnectedToHQ();

                    if ((eNewValue != PlayerType.NONE) && ownerServer().isDeleteOrders())
                    {
                        if (pBuilding != null)
                        {
                            OrderType eOrder = infos().buildingClass(pBuilding.getClass()).meOrderType;

                            if ((eOrder != OrderType.NONE) && (eOrder != OrderType.LAUNCH))
                            {
                                ownerServer().killBuilding(pBuilding);
                            }
                        }
                        else if (pConstruction != null)
                        {
                            OrderType eOrder = infos().buildingClass(pConstruction.getClass()).meOrderType;

                            if ((eOrder != OrderType.NONE) && (eOrder != OrderType.LAUNCH))
                            {
                                ownerServer().killConstruction(pConstruction, true);
                            }
                        }
                    }

                    if (eNewValue == PlayerType.NONE)
                    {
                        setLastBuilding(BuildingType.NONE);
                    }

                    makeDirty(TileDirtyType.meOwner);
                }
            }
        }
        public virtual void setRealOwner(PlayerType eNewValue)
        {
            if (getRealOwner() != eNewValue)
            {
                ConstructionServer pConstruction = constructionServer();
                BuildingServer pBuilding = buildingServer();

                if (isRealClaimed())
                {
                    if (pConstruction != null)
                    {
                        realOwnerServer().changeRealConstructionCount(pConstruction.getType(), -1);
                    }

                    if (pBuilding != null)
                    {
                        realOwnerServer().changeRealBuildingCount(pBuilding.getType(), -1);
                    }
                }

                meRealOwner = eNewValue;

                if (isRealClaimed())
                {
                    if (pConstruction != null)
                    {
                        realOwnerServer().changeRealConstructionCount(pConstruction.getType(), 1);
                    }

                    if (pBuilding != null)
                    {
                        realOwnerServer().changeRealBuildingCount(pBuilding.getType(), 1);
                    }
                }

                makeDirty(TileDirtyType.meRealOwner);
            }
        }

        public virtual void setClaimBlockPlayer(PlayerType eNewValue)
        {
            if (getClaimBlockPlayer() != eNewValue)
            {
                meClaimBlockPlayer = eNewValue;

                makeDirty(TileDirtyType.meClaimBlockPlayer);
            }
        }

        public virtual void setFrozenSabotage(SabotageType eNewValue)
        {
            if (getFrozenSabotageType() != eNewValue)
            {
                meFrozenSabotage = eNewValue;

                makeDirty(TileDirtyType.meFrozenSabotage);
            }
        }

        public virtual void setDefendSabotage(SabotageType eNewValue)
        {
            if (getDefendSabotage() != eNewValue)
            {
                meDefendSabotage = eNewValue;

                if (getDefendSabotage() == SabotageType.NONE)
                {
                    setRevealDefendSabotage(false);
                }

                makeDirty(TileDirtyType.meDefendSabotage);
            }
        }

        public virtual void setLastBuilding(BuildingType eNewValue)
        {
            if (getLastBuilding() != eNewValue)
            {
                meLastBuilding = eNewValue;

                makeDirty(TileDirtyType.meLastBuilding);
            }
        }

        public virtual void doVisibleBuilding()
        {
            BuildingType eNewType = BuildingType.NONE;

            BuildingType eRealBuilding = getConstructionOrBuildingType();

            if (eRealBuilding != BuildingType.NONE)
            {
                if (isShowWrongBuilding())
                {
                    System.Random pRandom = new CrossPlatformRandom(getID() + gameServer().getSeed());

                    if (eNewType == BuildingType.NONE)
                    {
                        if (isRealClaimed())
                        {
                            if (infos().building(eRealBuilding).mbRequiresModuleOrHQ)
                            {
                                if (!(isPotentialHQConnection(getRealOwner())) && !adjacentToModule())
                                {
                                    eNewType = eRealBuilding;
                                }
                            }
                        }
                    }

                    if (eNewType == BuildingType.NONE)
                    {
                        eNewType = getHologramBuilding();
                    }

                    if (eNewType == BuildingType.NONE)
                    {
                        int iBestValue = 0;

                        for (BuildingType eLoopBuilding = 0; eLoopBuilding < infos().buildingsNum(); eLoopBuilding++)
                        {
                            if (ownerClient().canHologramAs(this, eLoopBuilding))
                            {
                                if ((infos().buildingClass(infos().building(eLoopBuilding).meClass).meOrderType == OrderType.NONE) &&
                                    (infos().building(eLoopBuilding).miEntertainment == 0))
                                {
                                    int iValue = 0;

                                    for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                                    {
                                        TileClient pAdjacentTile = gameServer().mapClient().tileClientAdjacent(this, eLoopDirection);

                                        if (pAdjacentTile != null)
                                        {
                                            BuildingType eAdjacentBuilding = BuildingType.NONE;

                                            {
                                                BuildingClient pBuilding = pAdjacentTile.buildingClient();
                                                ConstructionClient pConstruction = pAdjacentTile.constructionClient();

                                                if ((pBuilding != null) &&
                                                    (pBuilding.getOwner() == getOwner()) &&
                                                    (pBuilding.getClass() != infos().building(eRealBuilding).meClass))
                                                {
                                                    eAdjacentBuilding = pBuilding.getType();
                                                }
                                                else if ((pConstruction != null) &&
                                                         (pConstruction.getOwner() == getOwner()) &&
                                                         (pConstruction.getClass() != infos().building(eRealBuilding).meClass))
                                                {
                                                    eAdjacentBuilding = pConstruction.getType();
                                                }
                                            }

                                            if (eAdjacentBuilding == eLoopBuilding)
                                            {
                                                int iSubValue = 1000;

                                                iSubValue += (500 * pAdjacentTile.countConnections(eAdjacentBuilding, getOwner(), true, false));

                                                if (pAdjacentTile.isBuilding())
                                                {
                                                    iSubValue *= 2;
                                                }

                                                iSubValue += pRandom.Next(500);

                                                iValue += iSubValue;
                                            }
                                        }
                                    }

                                    if (iValue > iBestValue)
                                    {
                                        eNewType = eLoopBuilding;
                                        iBestValue = iValue;
                                    }
                                }
                            }
                        }
                    }

                    if (eNewType == BuildingType.NONE)
                    {
                        eNewType = getVisibleBuilding();
                    }

                    if (eNewType == BuildingType.NONE)
                    {
                        int iBestValue = 0;

                        for (BuildingType eLoopBuilding = 0; eLoopBuilding < infos().buildingsNum(); eLoopBuilding++)
                        {
                            if (ownerClient().canConstructPlayer(eLoopBuilding, false) && gameServer().canTileHaveBuilding(this, eLoopBuilding, getOwner()))
                            {
                                if (!(infos().building(eLoopBuilding).mbNoFalse))
                                {
                                    BuildingClassType eLoopBuildingClass = infos().building(eLoopBuilding).meClass;

                                    if (eLoopBuildingClass != infos().building(eRealBuilding).meClass)
                                    {
                                        int iValue = 1;

                                        if (infos().building(eLoopBuilding).miEntertainment > 0)
                                        {
                                            for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                                            {
                                                TileServer pAdjacentTile = gameServer().tileServerAdjacent(this, eLoopDirection);

                                                if (pAdjacentTile != null)
                                                {
                                                    if (pAdjacentTile.isModule())
                                                    {
                                                        iValue += (gameServer().getModuleEntertainmentModifier(pAdjacentTile.moduleClient().getType()) * 100);
                                                    }
                                                }
                                            }
                                        }

                                        {
                                            OrderType eOrder = infos().buildingClass(eLoopBuildingClass).meOrderType;

                                            if (eOrder != OrderType.NONE)
                                            {
                                                if ((eOrder != OrderType.PATENT) || (gameServer().countPatentsAvailable() > 0))
                                                {
                                                    iValue += 1000;

                                                    for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                                                    {
                                                        TileServer pAdjacentTile = gameServer().tileServerAdjacent(this, eLoopDirection);

                                                        if (pAdjacentTile != null)
                                                        {
                                                            if (pAdjacentTile.isModule())
                                                            {
                                                                iValue += (gameServer().getModuleOrderModifier(pAdjacentTile.moduleClient().getType(), eOrder) * 100);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        iValue += pRandom.Next(1000);

                                        if (iValue > iBestValue)
                                        {
                                            eNewType = eLoopBuilding;
                                            iBestValue = iValue;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (getVisibleBuilding() != eNewType)
            {
                meVisibleBuilding = eNewType;

                makeDirty(TileDirtyType.meVisibleBuilding);
            }
        }

        public virtual void setHologramBuilding(BuildingType eNewValue)
        {
            if (getHologramBuilding() != eNewValue)
            {
                meHologramBuilding = eNewValue;

                makeDirty(TileDirtyType.meHologramBuilding);
            }
        }

        public virtual void setTerrainRegion(TerrainType eNewValue)
        {
            if (getTerrainRegion() != eNewValue)
            {
                meTerrainRegion = eNewValue;

                makeDirty(TileDirtyType.meTerrainRegion);
            }
        }

        public virtual void incrementSabotagedCount(SabotageType eIndex)
        {
            maiSabotagedCount[(int)eIndex]++;

            makeDirty(TileDirtyType.maiSabotagedCount);
        }

        public virtual void setResourceMined(ResourceType eIndex, int iNewValue)
        {
            if (getResourceMined(eIndex) != iNewValue)
            {
                maiResourceMined[(int)eIndex] = iNewValue;

                if (infos().resourceLevel(getResourceLevel(eIndex, false)).mabLocationDiminish[(int)(gameServer().getLocation())] && !infos().resource(eIndex).mabLocationNoDiminish[(int)gameServer().getLocation()])
                {
                    int iThreshold = infos().resource(eIndex).maiLocationDiminishThreshold[(int)(gameServer().getLocation())];
                    if (iThreshold > 0)
                    {
                        if ((getResourceMined(eIndex) / Constants.RESOURCE_MULTIPLIER) > iThreshold)
                        {
                            changeResourceLevel(eIndex, -1);
                            setResourceMined(eIndex, 0);

                            gameServer().gameEventsServer().AddResourceDiminished(getID(), eIndex);
                        }
                    }
                }

                makeDirty(TileDirtyType.maiResourceMined);
            }
        }
        public virtual void changeResourceMined(ResourceType eIndex, int iChange)
        {
            setResourceMined(eIndex, (getResourceMined(eIndex) + iChange));
        }

        public virtual void setPotentialHQConnection(PlayerType eIndex, bool bValue)
        {
            if (mabPotentialHQConnection[(int)eIndex] != bValue)
            {
                mabPotentialHQConnection[(int)eIndex] = bValue;
                makeDirty(TileDirtyType.mabPotentialHQConnection);
            }
        }

        public virtual void setRevealBuilding(TeamType eIndex, bool bValue)
        {
            if (isRevealBuilding(eIndex) != bValue)
            {
                mabRevealBuilding[(int)eIndex] = bValue;

                makeDirty(TileDirtyType.mabRevealBuilding);
            }
        }

        public virtual void setVisibility(PlayerType eIndex, VisibilityType eNewValue, bool bDiscovered)
        {
            VisibilityType eOldValue = getVisibility(eIndex);

            if (eOldValue != eNewValue)
            {
                maeVisibility[(int)eIndex] = eNewValue;

                if (eOldValue == VisibilityType.VISIBLE)
                {
                    gameServer().playerServer(eIndex).changeVisibleTiles(-1);
                }
                else if (getVisibility(eIndex) == VisibilityType.VISIBLE)
                {
                    gameServer().playerServer(eIndex).changeVisibleTiles(1);

                    if (bDiscovered)
                    {
                        if (getOwner() == PlayerType.NONE)
                        {
                            if (isGeothermal())
                            {
                                gameServer().playerServer(eIndex).makeGeothermalDiscovered(this);
                            }

                            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                            {
                                if (getResourceLevel(eLoopResource, false) > ResourceLevelType.NONE)
                                {
                                    gameServer().playerServer(eIndex).makeResourceDiscovered(eLoopResource, this);
                                }
                            }
                        }
                    }
                }

                makeDirty(TileDirtyType.maeVisibility);
            }
        }
        public virtual void increaseVisibility(PlayerType eIndex, VisibilityType eNewValue, bool bDiscovered)
        {
            if (getVisibility(eIndex) < eNewValue)
            {
                setVisibility(eIndex, eNewValue, bDiscovered);
            }
        }

        public virtual void setResourceLevel(ResourceType eIndex, ResourceLevelType eNewValue)
        {
            if (eNewValue < ResourceLevelType.NONE)
            {
                eNewValue = ResourceLevelType.NONE;
            }
            else if (eNewValue >= infos().resourceLevelsNum())
            {
                eNewValue = (infos().resourceLevelsNum() - 1);
            }

            ResourceLevelType eOldValue = getResourceLevel(eIndex, false);

            if (eOldValue != eNewValue)
            {
                maeResourceLevel[(int)eIndex] = eNewValue;

                if (eOldValue == ResourceLevelType.NONE)
                {
                    changeResourceCount(1);
                }
                else if (eNewValue == ResourceLevelType.NONE)
                {
                    changeResourceCount(-1);
                }

                gameServer().changeResourceRateCount(eIndex, (infos().resourceLevel(eNewValue).miRateMultiplier - infos().resourceLevel(eOldValue).miRateMultiplier));

                makeDirty(TileDirtyType.maeResourceLevel);
            }
        }
        public virtual void changeResourceLevel(ResourceType eIndex, int iChange)
        {
            setResourceLevel(eIndex, (getResourceLevel(eIndex, false) + iChange));
        }

        public virtual void changeUnitMissionCount(PlayerType eIndex1, MissionType eIndex2, sbyte iChange)
        {
            if (iChange != 0)
            {
                maaiUnitMissionCount[(int)eIndex1][(int)eIndex2] += iChange;

                makeDirty(TileDirtyType.maaiUnitMissionCount);
            }
        }

        public virtual int getArea()
        {
            return miArea;
        }
        public virtual void makeArea(int iNewValue)
        {
            if (getArea() == -1)
            {
                miArea = iNewValue;
                gameServer().incrementAreaTile(iNewValue);
            }
        }

        public virtual int getHQValue(HQType eIndex)
        {
            return maiHQValue[(int)eIndex];
        }
        public virtual int getHQValueAverage()
        {
            int iValue = 0;

            for (HQType eLoopHQ = 0; eLoopHQ < infos().HQsNum(); eLoopHQ++)
            {
                iValue += getHQValue(eLoopHQ);
            }

            return (iValue / (int)(infos().HQsNum()));
        }
        public virtual void setHQValue(HQType eIndex, int iNewValue)
        {
            maiHQValue[(int)eIndex] = iNewValue;
        }

        public virtual bool isHQMinimum(HQType eIndex)
        {
            return mabHQMinimum[(int)eIndex];
        }
        public virtual void setHQMinmum(HQType eIndex, bool bNewValue)
        {
            mabHQMinimum[(int)eIndex] = bNewValue;
        }
    }
}