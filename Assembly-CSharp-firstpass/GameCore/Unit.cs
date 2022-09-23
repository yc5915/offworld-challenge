using System.Collections.Generic;
using System;
using UnityEngine;
using System.IO;
using Offworld.GameCore.Text;
using Offworld.SystemCore;

namespace Offworld.GameCore
{
    public class UnitClient : TextHelpers
    {
        protected GameClient mGame = null;
        protected virtual GameClient gameClient()
        {
            return mGame;
        }
        protected virtual Infos infos()
        {
            return gameClient().infos();
        }

        public enum UnitDirtyType
        {
            FIRST,

            miHarvestQuantity,
            miPlunderQuantity,
            miPlunderTimer,
            miHoldCount,
            miCargoQuantity,
            miTileTarget,
            miOriginalTarget,
            mbPlundered,
            mWorldPosition,
            meCargoResource,
            meSabotageType,
            mMissionInfo,

            NUM_TYPES
        }

        protected BitMask mDirtyBits = new BitMask();
        public virtual bool isDirty(UnitDirtyType eType)
        {
            return mDirtyBits.GetBit((int)eType);
        }
        public virtual bool isAnyDirty()
        {
            return !(mDirtyBits.IsEmpty());
        }

        protected int miID = -1;
        protected int miHarvestQuantity = 0;
        protected int miPlunderQuantity = 0;
        protected int miPlunderTimer = 0;
        protected int miHoldCount = 0;
        protected int miCargoQuantity = 0;
        protected int miTileTarget = -1;
        protected int miOriginalTarget = -1;

        protected bool mbPlundered = false;

        protected Vector3 mWorldPosition = new Vector3(0, 0, 0);

        protected PlayerType meOwner = PlayerType.NONE;
        protected UnitType meType = UnitType.NONE;
        protected BuildingType meConstructionType = BuildingType.NONE;
        protected ResourceType meCargoResource = ResourceType.NONE;
        protected SabotageType meSabotageType = SabotageType.NONE;

        protected MissionInfo mMissionInfo = new MissionInfo(MissionType.NONE, -1);

        public UnitClient(GameClient pGame)
        {
            mGame = pGame;
        }

        protected virtual void SerializeClient(object stream, bool bAll)
        {
            SimplifyIO.Data(stream, ref mDirtyBits, "DirtyBits");

            if (isDirty(UnitDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref miID, "ID");
            }
            if (isDirty(UnitDirtyType.miHarvestQuantity) || bAll)
            {
                SimplifyIO.Data(stream, ref miHarvestQuantity, "HarvestQuantity");
            }
            if (isDirty(UnitDirtyType.miPlunderQuantity) || bAll)
            {
                SimplifyIO.Data(stream, ref miPlunderQuantity, "PlunderQuantity");
            }
            if (isDirty(UnitDirtyType.miPlunderTimer) || bAll)
            {
                SimplifyIO.Data(stream, ref miPlunderTimer, "PlunderTimer");
            }
            if (isDirty(UnitDirtyType.miHoldCount) || bAll)
            {
                SimplifyIO.Data(stream, ref miHoldCount, "HoldCount");
            }
            if (isDirty(UnitDirtyType.miCargoQuantity) || bAll)
            {
                SimplifyIO.Data(stream, ref miCargoQuantity, "CargoQuantity");
            }
            if (isDirty(UnitDirtyType.miTileTarget) || bAll)
            {
                SimplifyIO.Data(stream, ref miTileTarget, "TileTarget");
            }
            if (isDirty(UnitDirtyType.miOriginalTarget) || bAll)
            {
                SimplifyIO.Data(stream, ref miOriginalTarget, "OriginalTarget");
            }

            if (isDirty(UnitDirtyType.mbPlundered) || bAll)
            {
                SimplifyIO.Data(stream, ref mbPlundered, "Plundered");
            }

            if (isDirty(UnitDirtyType.mWorldPosition) || bAll)
            {
                SimplifyIO.Data(stream, ref mWorldPosition.x, "WorldPosition.x");
                SimplifyIO.Data(stream, ref mWorldPosition.y, "WorldPosition.y");
                SimplifyIO.Data(stream, ref mWorldPosition.z, "WorldPosition.z");
            }

            if (isDirty(UnitDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref meOwner, "Owner");
            }
            if (isDirty(UnitDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref meType, "Type");
            }
            if (isDirty(UnitDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref meConstructionType, "ConstructionType");
            }
            if (isDirty(UnitDirtyType.meCargoResource) || bAll)
            {
                SimplifyIO.Data(stream, ref meCargoResource, "CargoResource");
            }
            if (isDirty(UnitDirtyType.meSabotageType) || bAll)
            {
                SimplifyIO.Data(stream, ref meSabotageType, "SabotageType");
            }

            if (isDirty(UnitDirtyType.mMissionInfo) || bAll)
            {
                SimplifyIO.Data(stream, ref mMissionInfo.meMission, "MissionInfo_MissionType");
                SimplifyIO.Data(stream, ref mMissionInfo.miData, "MissionInfo_Data");
            }
        }

        public virtual void writeClientValues(BinaryWriter stream, bool bAll)
        {
            SerializeClient(stream, bAll);
        }

        public virtual void readClientValues(BinaryReader stream, bool bAll)
        {
            SerializeClient(stream, bAll);
        }

        protected virtual bool withinRangeTile(TileServer tile, float fRange)
        {
            return Utils.withinRange2D(tile.getWorldPosition(), getWorldPosition(), fRange);
        }

        public virtual bool shouldRender(TeamType eTeam)
        {
            if (Globals.AppInfo.IsObserver)
            {
                return true;
            }

            if (getMissionInfo().meMission == MissionType.CONSTRUCT)
            {
                TileClient pTargetTile = gameClient().tileClient(getMissionInfo().miData);

                if (pTargetTile != null)
                {
                    if (pTargetTile.isShowWrongBuilding())
                    {
                        if (!(pTargetTile.showCorrectBuilding(eTeam, true)))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public virtual int getID()
        {
            return miID;
        }

        public virtual int getHarvestQuantity()
        {
            return miHarvestQuantity;
        }

        protected virtual int getPlunderQuantity()
        {
            return miPlunderQuantity;
        }

        protected virtual int getPlunderTimer()
        {
            return miPlunderTimer;
        }

        protected virtual int getHoldCount()
        {
            return miHoldCount;
        }

        public virtual int getCargoQuantity()
        {
            return miCargoQuantity;
        }
        public virtual int getWholeCargoQuantity()
        {
            return (getCargoQuantity() / Constants.RESOURCE_MULTIPLIER);
        }
        public virtual bool hasCargo(ResourceType eResource)
        {
            return ((getCargoResource() == eResource) && (getCargoQuantity() > 0));
        }
        public virtual bool hasCargoAny()
        {
            return ((getCargoResource() != ResourceType.NONE) && (getCargoQuantity() > 0));
        }

        protected virtual int getTileTarget()
        {
            return miTileTarget;
        }
        public virtual TileClient tileTargetBase()
        {
            return gameClient().tileClient(getTileTarget());
        }

        protected virtual int getOriginalTarget()
        {
            return miOriginalTarget;
        }

        protected virtual bool isPlundered()
        {
            return mbPlundered;
        } 

        public virtual Vector3 getWorldPosition()
        {
            return mWorldPosition;
        }
        public virtual TileClient tileClient()
        {
            return gameClient().tileClient(TileClient.IDFromWorldPosition(getWorldPosition(), gameClient()));
        }
        public virtual bool isHideUnit()
        {
            TileClient pTile = tileClient();

            if (pTile.getUnitID() == getID())
            {
                if (pTile.isConstruction())
                {
                    return true;
                }
            }

            return false;
        }
        public virtual bool isRevealed(PlayerType ePlayer)
        {
            TileClient pTile = tileClient();

            if (isHideUnit())
            {
                return false;
            }

            if (pTile != null)
            {
                return (pTile.getVisibility(ePlayer) == VisibilityType.VISIBLE);
            }

            return false;
        }

        public virtual PlayerType getOwner()
        {
            return meOwner;
        }
        public virtual PlayerClient ownerClient()
        {
            return gameClient().playerClient(getOwner());
        }
        public virtual TeamType getTeam()
        {
            return ownerClient().getTeam();
        }

        public virtual UnitType getType()
        {
            return meType;
        }

        public virtual BuildingType getConstructionType()
        {
            return meConstructionType;
        }

        public virtual ResourceType getCargoResource()
        {
            return meCargoResource;
        }

        public virtual SabotageType getSabotageType()
        {
            return meSabotageType;
        }

        public virtual MissionInfo getMissionInfo()
        {
            return mMissionInfo;
        }
    }

    public class UnitServer : UnitClient
    {
        protected virtual GameServer gameServer()
        {
            return (GameServer)mGame;
        }

        protected virtual void makeDirty(UnitDirtyType eType)
        {
            mDirtyBits.SetBit((int)eType, true);
        }
        protected virtual void makeAllDirty()
        {
            for (UnitDirtyType eLoopType = 0; eLoopType < UnitDirtyType.NUM_TYPES; eLoopType++)
            {
                makeDirty(eLoopType);
            }
        }
        public virtual void clearDirty()
        {
            mDirtyBits.Clear();
        }

        public UnitServer(GameClient pGame)
            : base(pGame)
        {
        }

        public virtual void init(GameServer pGame, int iID, PlayerType eOwner, UnitType eType, BuildingType eConstructionType, TileServer pTile)
        {
            mGame = pGame;

            miID = iID;
            miTileTarget = pTile.getID();

            meOwner = eOwner;
            meType = eType;
            meConstructionType = eConstructionType;

            setWorldPosition(pTile.getWorldPosition());

            if ((infos().unit(getType()).miMovement == 0) && !(tileServer().isUnit()))
            {
                tileServer().setUnit(this);
            }

            if (getConstructionType() != BuildingType.NONE)
            {
                ownerServer().changeRealConstructionCount(getConstructionType(), 1);

                for (ResourceType eResource = 0; eResource < infos().resourcesNum(); eResource++)
                {
                    ownerServer().spend(eResource, ownerServer().getBuildingResourceCost(getConstructionType(), eResource));
                }
            }

            makeAllDirty();
        }

        public virtual void kill(bool bRefund)
        {
            clearMissionInfo(false);

            if ((infos().unit(getType()).miMovement == 0) && (tileServer().getUnitID() == getID()))
            {
                tileServer().setUnit(null);
            }

            if (getConstructionType() != BuildingType.NONE)
            {
                ownerServer().changeRealConstructionCount(getConstructionType(), -1);

                if (bRefund)
                {
                    for (ResourceType eResource = 0; eResource < infos().resourcesNum(); eResource++)
                    {
                        ownerServer().changeWholeResourceStockpile(eResource, ownerServer().getBuildingResourceCost(getConstructionType(), eResource), false);
                    }
                }
            }
        }

        public virtual bool doTurn()
        {
            if (ownerServer().getHQ() != HQType.NONE)
            {
                int iGasRate = ownerServer().getGas(getType());
                if (iGasRate < 0)
                {
                    ResourceType eGasResource = ownerServer().getGasResource();

                    ownerServer().changeResourceStockpile(eGasResource, iGasRate, true);
                    ownerServer().changeResourceRate(eGasResource, iGasRate);
                    ownerServer().changeResourceInput(eGasResource, iGasRate);
                }
            }

            if (getPlunderQuantity() > 0)
            {
                if (getPlunderTimer() > 0)
                {
                    changePlunderTimer(-1);
                }
                else
                {
                    UnitServer pBestUnit = null;
                    int iBestValue = 0;

                    foreach (KeyValuePair<int, UnitClient> pair in gameServer().getUnitDictionary())
                    {
                        UnitServer pLoopUnit = (UnitServer)(pair.Value);

                        if (canTarget(pLoopUnit))
                        {
                            int iValue = 1;

                            iValue += gameServer().random().Next(1000);

                            if (iValue > iBestValue)
                            {
                                pBestUnit = pLoopUnit;
                                iBestValue = iValue;
                            }
                        }
                    }

                    if (pBestUnit != null)
                    {
                        ResourceType eResource = pBestUnit.getCargoResource();
                        int iQuantity = ((pBestUnit.getWholeCargoQuantity() + 1) / 2);
                        iQuantity = Math.Min(iQuantity, getPlunderQuantity());

                        if ((eResource != ResourceType.NONE) && (iQuantity > 0))
                        {
                            bool bFirst = false;

                            if (getSabotageType() != SabotageType.NONE)
                            {
                                if (getPlunderQuantity() == infos().sabotage(getSabotageType()).miPlunderQuantity)
                                {
                                    gameServer().statsServer().addEvent(getOwner(), StatEventType.SABOTAGE, (int)pBestUnit.getOwner(), (int)getSabotageType());

                                    if (infos().sabotage(getSabotageType()).mbHostile)
                                    {
                                        pBestUnit.ownerServer().incrementSabotagedCount();

                                        gameServer().statsServer().changeStat(StatsType.SABOTAGING, (int)SabotagingStatType.TARGET, getOwner(), (int)(pBestUnit.getOwner()), 1);
                                        gameServer().statsServer().changeStat(StatsType.SABOTAGED, (int)SabotagedStatType.DEFENDED, pBestUnit.getOwner(), (int)(getSabotageType()), 1);

                                        bFirst = true;
                                    }
                                }
                            }

                            pBestUnit.changeCargoQuantity(-(iQuantity) * Constants.RESOURCE_MULTIPLIER);
                            ownerServer().changeWholeResourceStockpile(eResource, iQuantity, false);

                            changePlunderQuantity(-(iQuantity));
                            gameServer().gameEventsServer().AddPlunder(getOwner(), pBestUnit.getOwner(), eResource, iQuantity, tileServer().getID(), getID(), pBestUnit.getID(), bFirst);

                            setPlunderTimer(infos().unit(getType()).miPlunderTimer);
                            pBestUnit.makePlundered();

                            if (ownerServer().isHuman())
                            {
                                bool bValid = true;

                                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                {
                                    if (gameServer().isResourceValid(eLoopResource))
                                    {
                                        if (eLoopResource != eResource)
                                        {
                                            if (gameServer().marketServer().getWholePrice(eLoopResource) >= gameServer().marketServer().getWholePrice(eResource))
                                            {
                                                bValid = false;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (bValid)
                                {
                                    gameServer().gameEventsServer().AddAchievement(getOwner(), infos().getType<AchievementType>("ACHIEVEMENT_BEST_PIRACY"));
                                }
                            }

                            if (getPlunderQuantity() == 0)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            if (getMissionInfo().meMission != MissionType.NONE)
            {
                TileServer pTargetTile = gameServer().tileServer(getMissionInfo().miData);

                if (pTargetTile != null)
                {
                    if (getHarvestQuantity() > 0)
                    {
                        if (getCargoResource() == ResourceType.NONE)
                        {
                            ResourceType eBestResource = ResourceType.NONE;
                            int iBestValue = 0;

                            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                            {
                                int iValue = gameServer().resourceMiningTile(BuildingType.NONE, eLoopResource, pTargetTile, getOwner(), infos().unit(getType()).miMining, 0, false);
                                if (iValue > 0)
                                {
                                    iValue *= gameServer().marketServer().getWholePrice(eLoopResource);

                                    if (iValue > iBestValue)
                                    {
                                        eBestResource = eLoopResource;
                                        iBestValue = iValue;
                                    }
                                }
                            }

                            setCargo(eBestResource, 0);
                        }
                    }

                    if (withinRangeTile(pTargetTile, Globals.Infos.Globals.UNIT_MISSION_COMPLETE_RANGE))
                    {
                        switch (getMissionInfo().meMission)
                        {
                            case MissionType.CONSTRUCT:
                                {
                                    ownerServer().finishClaim(pTargetTile, getConstructionType(), false);
                                    break;
                                }

                            case MissionType.REPAIR:
                                {
                                    ConstructionServer pConstruction = pTargetTile.constructionServer();
                                    if (pConstruction != null)
                                    {
                                        pConstruction.startRepairConstruction();
                                    }
                                    break;
                                }

                            case MissionType.MINE:
                                {
                                    if (getCargoResource() != ResourceType.NONE)
                                    {
                                        if ((getWholeCargoQuantity() >= gameClient().shippingCapacity(getCargoResource())) || !(pTargetTile.canMine()))
                                        {
                                            HQServer pHQ = ownerServer().findClosestHQServer(pTargetTile);

                                            if (pHQ != null)
                                            {
                                                setMissionInfo(MissionType.SHIP_HQ, pHQ.getTileID(), false);
                                                changeHarvestQuantity(-(getWholeCargoQuantity()));
                                            }
                                        }
                                        else
                                        {
                                            changeCargoQuantity(gameServer().resourceMiningTile(BuildingType.NONE, getCargoResource(), pTargetTile, getOwner(), infos().unit(getType()).miMining, 0, false));
                                        }
                                    }
                                    return false;
                                }

                            case MissionType.SHIP_HQ:
                                {
                                    if (hasCargoAny())
                                    {
                                        bool bTestUpgrade = false;

                                        if (infos().HQ(ownerServer().getHQ()).maiUpgradeResource[(int)getCargoResource()] > 0)
                                        {
                                            bTestUpgrade = true;
                                        }

                                        ownerServer().processResource(getCargoResource(), getCargoQuantity());

                                        setCargoQuantity(0);

                                        ownerServer().AI_setForceCheckUpgrade(bTestUpgrade);
                                    }

                                    if ((getHarvestQuantity() > 0) && (getCargoResource() != ResourceType.NONE))
                                    {
                                        TileServer pOriginalTile = gameServer().tileServer(getOriginalTarget());

                                        if (pOriginalTile != null)
                                        {
                                            if (pOriginalTile.canMine())
                                            {
                                                setMissionInfo(MissionType.MINE, pOriginalTile.getID(), false);

                                                return false;
                                            }
                                        }
                                        else
                                        {
                                            pOriginalTile = ownerServer().findClosestHQServer(tileServer()).tileServer();
                                        }

                                        TileServer pBestTile = null;
                                        int iBestValue = 0;

                                        foreach (TileServer pLoopTile in gameServer().tileServerAll())
                                        {
                                            if (pLoopTile.canMine())
                                            {
                                                int iValue = gameServer().resourceMiningTile(BuildingType.NONE, getCargoResource(), pLoopTile, getOwner(), infos().unit(getType()).miMining, 0, false);
                                                if (iValue > 0)
                                                {
                                                    iValue *= (Utils.maxStepDistance(gameServer()));
                                                    iValue /= (Utils.maxStepDistance(gameServer()) + Utils.stepDistanceTile(pLoopTile, pOriginalTile));

                                                    if (iValue > iBestValue)
                                                    {
                                                        pBestTile = pLoopTile;
                                                        iBestValue = iValue;
                                                    }
                                                }
                                            }
                                        }

                                        if (pBestTile != null)
                                        {
                                            setMissionInfo(MissionType.MINE, pBestTile.getID(), false);

                                            return false;
                                        }
                                    }
                                    break;
                                }

                            case MissionType.SHIP_BUILDING:
                                {
                                    BuildingServer pBuilding = pTargetTile.buildingServer();
                                    if (pBuilding == null || this.getOwner() != pBuilding.getOwner())
                                    {
                                        HQServer closestHQ = this.ownerServer().findClosestHQServer(pTargetTile);
                                        if (closestHQ != null)
                                            setMissionInfo(MissionType.SHIP_HQ, closestHQ.getTileID(), false);
                                        return false;
                                    }
                                    else if (pBuilding != null)
                                    {
                                        pBuilding.changeResourceStockpile(getCargoResource(), getCargoQuantity());

                                        setCargoQuantity(0);
                                    }
                                    break;
                                }
                        }

                        return true;
                    }
                    else
                    {
                        moveToTile(pTargetTile);
                    }
                }
            }

            return false;
        }

        protected virtual void moveToTile(TileServer tile)
        {
            Vector3 startPosition = getWorldPosition();
            Vector3 targetPosition = tile.getWorldPosition();
            startPosition.y = 0;
            targetPosition.y = 0;
            Vector3 newPosition = Vector3.MoveTowards(startPosition, targetPosition, gameServer().getUnitMovement(getType(), ownerServer().getHQ(), tileClient(), tile));
            setWorldPosition(newPosition);
        }

        public virtual void setHarvestQuantity(int iNewValue)
        {
            iNewValue = Math.Max(0, iNewValue);

            if (getHarvestQuantity() != iNewValue)
            {
                miHarvestQuantity = iNewValue;

                makeDirty(UnitDirtyType.miHarvestQuantity);
            }
        }
        public virtual void changeHarvestQuantity(int iChange)
        {
            setHarvestQuantity(getHarvestQuantity() + iChange);
        }

        public virtual void setPlunderQuantity(int iNewValue)
        {
            iNewValue = Math.Max(0, iNewValue);

            if (getPlunderQuantity() != iNewValue)
            {
                miPlunderQuantity = iNewValue;

                makeDirty(UnitDirtyType.miPlunderQuantity);
            }
        }
        public virtual void changePlunderQuantity(int iChange)
        {
            setPlunderQuantity(getPlunderQuantity() + iChange);
        }

        public virtual void setPlunderTimer(int iNewValue)
        {
            if (getPlunderTimer() != iNewValue)
            {
                miPlunderTimer = iNewValue;

                makeDirty(UnitDirtyType.miPlunderTimer);
            }
        }
        public virtual void changePlunderTimer(int iChange)
        {
            setPlunderTimer(getPlunderTimer() + iChange);
        }

        public virtual void changeHoldCount(int iChange)
        {
            if (iChange != 0)
            {
                miHoldCount += iChange;

                makeDirty(UnitDirtyType.miHoldCount);
            }
        }

        public virtual void setCargo(ResourceType eNewResource, int iNewQuantity)
        {
            if ((getCargoResource() != eNewResource) || (getCargoQuantity() != iNewQuantity))
            {
                meCargoResource = eNewResource;
                miCargoQuantity = iNewQuantity;

                makeDirty(UnitDirtyType.meCargoResource);
                makeDirty(UnitDirtyType.miCargoQuantity);
            }
        }
        public virtual void setCargoQuantity(int iNewValue)
        {
            if (getCargoQuantity() != iNewValue)
            {
                miCargoQuantity = iNewValue;

                makeDirty(UnitDirtyType.miCargoQuantity);
            }
        }
        public virtual void changeCargoQuantity(int iChange)
        {
            setCargoQuantity(getCargoQuantity() + iChange);
        }

        public virtual void setTileTarget(int iNewValue)
        {
            if (getTileTarget() != iNewValue)
            {
                miTileTarget = iNewValue;

                makeDirty(UnitDirtyType.miTileTarget);
            }
        }

        public virtual void makeOriginalTarget(int iNewValue)
        {
            if (getOriginalTarget() == -1)
            {
                miOriginalTarget = iNewValue;

                makeDirty(UnitDirtyType.miOriginalTarget);
            }
        }

        public virtual void makePlundered()
        {
            if (!isPlundered())
            {
                mbPlundered = true;

                makeDirty(UnitDirtyType.mbPlundered);
            }
        }

        public virtual TileServer tileServer()
        {
            return (TileServer)tileClient();
        }
        public virtual void setWorldPosition(Vector3 position)
        {
            mWorldPosition = position;
            TileServer pTile = tileServer();
            mWorldPosition.y = ((pTile != null) ? pTile.getWorldPosition().y : 0.0f);
            makeDirty(UnitDirtyType.mWorldPosition);
        }

        public virtual PlayerServer ownerServer()
        {
            return (PlayerServer)ownerClient();
        }

        public virtual void setSabotageType(SabotageType eNewValue)
        {
            if (getSabotageType() != eNewValue)
            {
                meSabotageType = eNewValue;

                makeDirty(UnitDirtyType.meSabotageType);
            }
        }

        public virtual void setMissionInfo(MissionType eNewMission, int iNewData, bool bLost)
        {
            if ((getMissionInfo().meMission != eNewMission) || (getMissionInfo().miData != iNewData))
            {
                MissionType eOldMission = mMissionInfo.meMission;
                int iOldData = mMissionInfo.miData;

                mMissionInfo.meMission = eNewMission;
                mMissionInfo.miData = iNewData;

                if ((eOldMission != MissionType.NONE) && (iOldData != -1))
                {
                    gameServer().tileServer(iOldData).changeUnitMissionCount(getOwner(), eOldMission, -1);
                }

                if ((eNewMission != MissionType.NONE) && (iNewData != -1))
                {
                    setTileTarget(gameServer().tileServer(iNewData).getID());

                    gameServer().tileServer(iNewData).changeUnitMissionCount(getOwner(), eNewMission, 1);
                }

                if (eNewMission == MissionType.MINE)
                {
                    makeOriginalTarget(iNewData);
                }

                makeDirty(UnitDirtyType.mMissionInfo);
            }
        }
        protected virtual void clearMissionInfo(bool bLost)
        {
            setMissionInfo(MissionType.NONE, -1, bLost);
        }

        protected virtual bool canTarget(UnitServer pUnit)
        {
            if (isHideUnit())
            {
                return false;
            }

            if (pUnit.getTeam() == getTeam())
            {
                return false;
            }

            if (!(pUnit.hasCargoAny()))
            {
                return false;
            }

            if (pUnit.isPlundered())
            {
                return false;
            }

            if (pUnit.mMissionInfo.meMission == MissionType.SHIP_BUILDING)
            {
                TileServer pTile = gameServer().tileServer(pUnit.mMissionInfo.miData);
                BuildingType eVisibleBuilding = pTile.getVisibleConstructionOrBuildingType(getTeam());

                if (eVisibleBuilding == BuildingType.NONE)
                {
                    return false;
                }

                if (Utils.isBuildingYieldAny(eVisibleBuilding, gameServer()))
                {
                    return false;
                }
            }
            else if (pUnit.mMissionInfo.meMission != MissionType.SHIP_HQ)
            {
                return false;
            }

            if (!Utils.withinRange2D(pUnit.getWorldPosition(), getWorldPosition(), infos().unit(getType()).miPlunderRange + 1))
            {
                return false;
            }

            return true;
        }
    }
}