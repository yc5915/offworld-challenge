using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Offworld.GameCore.Text;
using Offworld.SystemCore;

namespace Offworld.GameCore
{
    public class BuildingClient : TextHelpers
    {
        protected GameClient mGame = null;
        GameClient gameClient()
        {
            return mGame;
        }
        protected Infos infos()
        {
            return gameClient().infos();
        }

        public enum BuildingDirtyType
        {
            FIRST,

            miConnections,
            mbNeedsPower,
            mbWorked,
            mbOff,
            mbAutoOff,
            meOwner,
            maiConnectedBuildings,
            maiResourceStockpile,
            maiResourceCost,

            NUM_TYPES
        }

        protected BitMask mDirtyBits = new BitMask();
        bool isDirty(BuildingDirtyType eType)
        {
            return mDirtyBits.GetBit((int)eType);
        }
        public bool isAnyDirty()
        {
            return !(mDirtyBits.IsEmpty());
        }

        protected int miID = -1;
        protected int miTileID = -1;
        protected int miConnections = 0;

        protected bool mbNeedsPower = false;
        protected bool mbWorked = false;
        protected bool mbOff = false;
        protected bool mbAutoOff = false;

        protected PlayerType meOwner = PlayerType.NONE;
        protected BuildingType meType = BuildingType.NONE;

        protected List<int> maiConnectedBuildings = new List<int>();
        protected List<int> maiResourceStockpile = new List<int>();
        protected List<int> maiResourceCost = new List<int>();

        public BuildingClient(GameClient pGame)
        {
            mGame = pGame;
        }

        protected virtual void SerializeClient(object stream, bool bAll, int compatibilityNumber)
        {
            SimplifyIO.Data(stream, ref mDirtyBits, "DirtyBits");

            if (isDirty(BuildingDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref miID, "ID");
            }
            if (isDirty(BuildingDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref miTileID, "TileID");
            }
            if (isDirty(BuildingDirtyType.miConnections) || bAll)
            {
                SimplifyIO.Data(stream, ref miConnections, "Connections");
            }

            if (isDirty(BuildingDirtyType.mbWorked) || bAll)
            {
                SimplifyIO.Data(stream, ref mbWorked, "Worked");
            }
            if (isDirty(BuildingDirtyType.mbOff) || bAll)
            {
                SimplifyIO.Data(stream, ref mbOff, "Off");
            }
            if (isDirty(BuildingDirtyType.mbAutoOff) || bAll)
            {
                SimplifyIO.Data(stream, ref mbAutoOff, "AutoOff");
            }
            if (isDirty(BuildingDirtyType.mbNeedsPower) || bAll)
            {
                SimplifyIO.Data(stream, ref mbNeedsPower, "NeedsPower");
            }

            if (isDirty(BuildingDirtyType.meOwner) || bAll)
            {
                SimplifyIO.Data(stream, ref meOwner, "Owner");
            }
            if (isDirty(BuildingDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref meType, "Type");
            }

            if (isDirty(BuildingDirtyType.maiConnectedBuildings) || bAll)
            {
                SimplifyIO.Data(stream, ref maiConnectedBuildings, (int)DirectionType.NUM_TYPES, "ConnectedBuildings_");
            }
            if (isDirty(BuildingDirtyType.maiResourceStockpile) || bAll)
            {
                SimplifyIO.Data(stream, ref maiResourceStockpile, (int)infos().resourcesNum(), "ResourceStockpile_");
            }
            if (isDirty(BuildingDirtyType.maiResourceCost) || bAll)
            {
                SimplifyIO.Data(stream, ref maiResourceCost, (int)infos().resourcesNum(), "ResourceCost_");
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

        public virtual bool canScrap()
        {
            if (infos().rulesSet(mGame.getRulesSet()).mbDisableScrapping)
            {
                return false;
            }

            if (!(infos().building(getType()).mbCanScrap))
            {
                return false;
            }

            if (!(tileClient().isOwnerReal()))
            {
                return false;
            }

            if (tileClient().isVirus())
            {
                return false;
            }

            if (gameClient().auctionTileClient() == tileClient())
            {
                return false;
            }

            return true;
        }

        public virtual bool canSendResourcesManual()
        {
            if (!(infos().building(getType()).mbCanShip))
            {
                return false;
            }

            if (ownerClient().isTeleportation())
            {
                return false;
            }

            if (tileClient().isConnectedToHQ())
            {
                return false;
            }

            return true;
        }

        public virtual bool canSupplyBuilding()
        {
            if (ownerClient().isTeleportation())
            {
                return false;
            }

            if (tileClient().isConnectedToHQ())
            {
                return false;
            }

            return infos().resources().Any(resource => wantsInput(resource.meType));
        }

        public bool canUpgrade(bool bTestMoney)
        {
            InfoBuilding infoBuilding = infos().building(meType);
            if (infoBuilding.meUpgrade == BuildingType.NONE)
                return false;
            if (bTestMoney)
            {
                int iCost = 0;
                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    int iResourceCost = ownerClient().getNeededResourceCost(eLoopResource, infoBuilding.maiUpgradeCosts[(int)eLoopResource]);
                    if (iResourceCost > 0 && ownerClient().isResourceNoBuy(eLoopResource))
                        return false;
                    iCost += iResourceCost;
                }
                return iCost <= ownerClient().getMoney();
            }
            return true;
        }

        public virtual string getSpecialStatusText() { return string.Empty; }

        public BuildingType getUpgrade()
        {
            return infos().building(this.getType()).meUpgrade;
        }

        public virtual int getEntertainmentProfit()
        {
            return gameClient().entertainmentProfit(getType(), tileClient(), getOwner(), false, false);
        }

        public virtual bool isFrozen()
        {
            return tileClient().isFrozen();
        }

        public virtual bool isDouble()
        {
            return tileClient().isDouble();
        }

        public virtual bool isHalf()
        {
            return tileClient().isHalf();
        }

        public virtual bool isOverload()
        {
            return tileClient().isOverload();
        }

        public virtual bool isVirus()
        {
            return tileClient().isVirus();
        }

        public virtual bool isClosed(TeamType eVisibleTeam)
        {
            return gameClient().buildingClosed(gameClient().getHours(), getVisibleType(eVisibleTeam), getOwner());
        }

        public virtual int calculateOpenCount(int iTurns, TeamType eVisibleTeam)
        {
            return gameClient().calculateOpenCount(iTurns, getVisibleType(eVisibleTeam), getOwner());
        }

        public virtual bool isStoppedVisible(TeamType eVisibleTeam = TeamType.NONE)
        {
            if (isFrozen())
            {
                return true;
            }

            if (isClosed(eVisibleTeam))
            {
                return true;
            }

            return false;
        }

        public virtual bool isStopped(TeamType eVisibleTeam = TeamType.NONE)
        {
            if (isStoppedVisible(eVisibleTeam))
            {
                return true;
            }

            if (isOff() && !isVirus())
            {
                return true;
            }

            InfoEventState eventState = infos().eventState(gameClient().getEventStateGame());
            if (eventState != null && eventState.mbDisableBuildings && (eventState.meAffectedTerrain == TerrainType.NONE || eventState.meAffectedTerrain == tileClient().getTerrain()))
            {
                return true;
            }

            return false;
        }

        public virtual bool isVisible(TeamType eTeam)
        {
            if (getTeam() == eTeam)
            {
                return true;
            }
            else if ((getType() == getVisibleType(eTeam)) && !(tileClient().isTakeover()))
            {
                return true;
            }

            return false;
        }

        public virtual int getResourceCapacity(ResourceType eResource)
        {
            return (gameClient().shippingCapacity(eResource) * Constants.RESOURCE_MULTIPLIER);
        }
        public virtual int getResourceSpace(ResourceType eResource)
        {
            return Math.Max(0, (getResourceCapacity(eResource) - getResourceStockpile(eResource)));
        }

        public virtual bool wantsInput(ResourceType eResource)
        {
            return gameClient().buildingWantsInput(getType(), eResource, tileClient(), ownerClient());
        }

        public virtual bool wantsAnyInput()
        {
            return gameClient().buildingWantsAnyInput(getType(), tileClient(), ownerClient());
        }

        public virtual bool isConnectionPossible(TileClient pFromTile)
        {
            TileClient pTile = tileClient();

            for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
            {
                if (getConnectedBuilding(eLoopDirection) != -1)
                {
                    TileClient pAdjacentTile = gameClient().mapClient().tileClientAdjacent(pTile, eLoopDirection);

                    if ((pAdjacentTile != null) && (pAdjacentTile != pFromTile))
                    {
                        if (!(Utils.adjacentTiles(pAdjacentTile, pFromTile)))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public int resourceMining(ResourceType eResource, bool bIgnoreTemp)
        {
            return gameClient().resourceMiningTile(getType(), eResource, tileClient(), getOwner(), infos().building(getType()).maiResourceMining[(int)eResource], getConnections(), bIgnoreTemp);
        }

        public int resourceOutput(ResourceType eResource, bool bIgnoreTemp)
        {
            return gameClient().resourceOutput(getType(), eResource, tileClient(), getOwner(), getConnections(), bIgnoreTemp);
        }

        public int resourceInput(ResourceType eResource, bool bIgnoreTemp)
        {
            return gameClient().resourceInput(getType(), eResource, tileClient(), getOwner(), bIgnoreTemp);
        }

        public virtual int getID()
        {
            return miID;
        }

        public virtual int getTileID()
        {
            return miTileID;
        }
        public virtual TileClient tileClient()
        {
            return gameClient().tileClient(getTileID());
        }
        public virtual int getX()
        {
            return tileClient().getX();
        }
        public virtual int getY()
        {
            return tileClient().getY();
        }
        public virtual Vector3 getWorldPosition()
        {
            return tileClient().getWorldPosition();
        }

        public virtual int getConnections()
        {
            return miConnections;
        }

        public virtual bool isWorked()
        {
            return mbWorked;
        }

        public virtual bool isOff()
        {
            return mbOff;
        }

        public virtual bool isAutoOff()
        {
            return mbAutoOff;
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

        public virtual BuildingType getType()
        {
            return meType;
        }
        public virtual BuildingType getVisibleType(TeamType eTeam)
        {
            BuildingType eVisibleType = BuildingType.NONE;

            if (!(tileClient().showCorrectBuilding(eTeam, true)))
            {
                eVisibleType = tileClient().getVisibleBuilding();
            }

            if (eVisibleType == BuildingType.NONE)
            {
                eVisibleType = getType();
            }

            return eVisibleType;
        }
        public virtual BuildingClassType getClass()
        {
            return infos().building(getType()).meClass;
        }

        public virtual int getConnectedBuilding(DirectionType eIndex)
        {
            return maiConnectedBuildings[(int)eIndex];
        }
        public static bool isConnectionValid(BuildingType eThisBuilding, BuildingType eOtherBuilding, TileClient pOtherTile, PlayerClient pPlayer, GameClient pGame, Infos pInfos)
        {
            using (new UnityProfileScope("Building::isConnectionValid"))
            {
                if (pInfos.building(eThisBuilding).meClass == pInfos.building(eOtherBuilding).meClass)
                {
                    return true;
                }

                if (pInfos.buildingClass(pInfos.building(eThisBuilding).meClass).mabAdjacentBuildingClassBonus[(int)(pInfos.building(eOtherBuilding).meClass)])
                {
                    return true;
                }

                UnityEngine.Profiling.Profiler.BeginSample("Building::isConnectionValid::doRoboticAdjacency");
                if (pPlayer.getHQ() != HQType.NONE)
                {
                    if (pInfos.HQ(pPlayer.getHQ()).mbAdjacentInputBonus)
                    {
                        if (pInfos.building(eThisBuilding).miPowerConsumption > 0)
                        {
                            if (pPlayer.isBorehole())
                            {
                                if (pOtherTile.onOrAdjacentToGeothermal())
                                {
                                    return true;
                                }
                            }

                            for (ResourceType eLoopResource = 0; eLoopResource < pInfos.resourcesNum(); eLoopResource++)
                            {
                                if ((eLoopResource == pInfos.Globals.ENERGY_RESOURCE) || pPlayer.isAlternatePowerResource(eLoopResource))
                                {
                                    if (Utils.isBuildingYield(eOtherBuilding, eLoopResource, pGame))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }

                        for (ResourceType eLoopResource = 0; eLoopResource < pInfos.resourcesNum(); eLoopResource++)
                        {
                            if (pInfos.building(eThisBuilding).maiResourceInput[(int)eLoopResource] > 0)
                            {
                                if (pGame.resourceLevelMining(eOtherBuilding, eLoopResource, pOtherTile, pPlayer.getPlayer()) > 0)
                                {
                                    return true;
                                }
                                else if (GameClient.resourceOutputTile(pGame, pInfos, eOtherBuilding, eLoopResource, pOtherTile) > 0)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                UnityEngine.Profiling.Profiler.EndSample();

                return false;
            }
        }

        public virtual int getResourceStockpile(ResourceType eIndex)
        {
            return maiResourceStockpile[(int)eIndex];
        }
        public virtual int getWholeResourceStockpile(ResourceType eIndex)
        {
            return (getResourceStockpile(eIndex) / Constants.RESOURCE_MULTIPLIER);
        }

        public virtual int getResourceCost(ResourceType eIndex)
        {
            return maiResourceCost[(int)eIndex];
        }

        public bool isNeedsPower()
        {
            return mbNeedsPower;
        }
    }

    public class BuildingServer : BuildingClient
    {
        GameServer gameServer()
        {
            return (GameServer)mGame;
        }

        protected void makeDirty(BuildingDirtyType eType)
        {
            mDirtyBits.SetBit((int)eType, true);
        }
        protected void makeAllDirty()
        {
            for (BuildingDirtyType eLoopType = 0; eLoopType < BuildingDirtyType.NUM_TYPES; eLoopType++)
            {
                makeDirty(eLoopType);
            }
        }
        public void clearDirty()
        {
            mDirtyBits.Clear();
        }

        public BuildingServer(GameClient pGame)
            : base(pGame)
        {
        }

        public virtual void init(GameServer pGame, int iID, PlayerType eOwner, BuildingType eType, TileServer pTile, ConstructionServer pConstruction)
        {
            mGame = pGame;

            miID = iID;
            miTileID = pTile.getID();

            meOwner = eOwner;
            meType = eType;

            if (ownerServer().isHuman())
            {
                if (!(infos().rulesSet(pGame.getRulesSet()).mbNoAutoOff))
                {
                    if ((pGame.isOriginalSinglePlayer()) ? ownerServer().isPlayerOption(PlayerOptionType.TURN_OFF_UNPROFITABLE_SP) : ownerServer().isPlayerOption(PlayerOptionType.TURN_OFF_UNPROFITABLE_MP))
                    {
                        if (Utils.isBuildingInputAny(getType()))
                        {
                            mbAutoOff = true;
                        }
                    }
                }
            }

            maiConnectedBuildings = Enumerable.Repeat(-1, (int)DirectionType.NUM_TYPES).ToList();

            maiResourceStockpile = Enumerable.Repeat(0, (int)infos().resourcesNum()).ToList();

            maiResourceCost.Clear();
            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                maiResourceCost.Add((pConstruction != null) ? pConstruction.getResourceCost(eLoopResource) : ownerServer().getBuildingResourceCost(getType(), eLoopResource));
            }

            tileServer().setBuilding(this);

            ownerServer().changeBuildingCount(getType(), 1);

            tileServer().realOwnerServer().changeRealBuildingCount(getType(), 1);

            tileServer().doVisibleBuilding();

            BuildingClassType eClass = infos().building(getType()).meClass;

            if (tileServer().isOwnerReal() && !(tileServer().isShowWrongBuilding()))
            {
                if (!pGame.hasBuildingClassBeenConstructed(getType()))
                {
                    gameServer().makeBuildingClassFinished(eClass);
                }
            }

            if (infos().buildingClass(getClass()).meOrderType == OrderType.NONE)
            {
                if (!isStopped())
                {
                    setWorked(true);
                }
            }

            if (infos().building(getType()).miEntertainment > 0)
            {
                gameServer().doEntertainmentSupply();
            }

            supplyBuilding(true);

            ownerServer().AI_forceBuildingOrder(this);

            makeAllDirty();
        }

        public virtual void kill()
        {
            tileServer().removeBuilding(this);

            ownerServer().changeBuildingCount(getType(), -1);

            tileServer().realOwnerServer().changeRealBuildingCount(getType(), -1);

            {
                OrderType eOrder = infos().buildingClass(getClass()).meOrderType;

                if (eOrder != OrderType.NONE)
                {
                    bool bClear = false;

                    foreach (OrderInfo pLoopOrderInfo in ownerServer().getOrderInfos(eOrder))
                    {
                        if (pLoopOrderInfo.miBuildingID == getID())
                        {
                            bClear = true;
                            break;
                        }
                    }

                    if (bClear)
                    {
                        foreach (OrderInfo pLoopOrderInfo in ownerServer().getOrderInfos(eOrder))
                        {
                            pLoopOrderInfo.miBuildingID = -1;
                        }
                    }
                }
            }

            if (infos().building(getType()).miEntertainment > 0)
            {
                gameServer().doEntertainmentSupply();
            }
        }

        public virtual void upgrade()
        {
            InfoBuilding infoBuilding = infos().building(this.getType());
            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                ownerServer().spend(eLoopResource, infoBuilding.maiUpgradeCosts[(int)eLoopResource]);
            meType = infoBuilding.meUpgrade;
            makeDirty(BuildingDirtyType.FIRST);
        }

        protected virtual void setNeedsPower(bool bNeedsPower)
        {
            if (mbNeedsPower == bNeedsPower)
                return;
            mbNeedsPower = bNeedsPower;
            makeDirty(BuildingDirtyType.mbNeedsPower);
        }

        public virtual void doTurn()
        {
            using (new UnityProfileScope("BuildingServer::doTurn"))
            {
                TileServer pTile = tileServer();

                bool bNeedPower = false;
                bool bWorked = false;

                if (isAutoOff() && pTile.isOwnerReal())
                {
                    int iNet = gameServer().calculateRevenue(getType(), pTile, getOwner(), getConnections(), false, isOff(), 0, 0, 100, false);
                    if ((iNet < 0) != isOff())
                    {
                        toggleOff();
                    }
                }

                ResourceType ePower = infos().Globals.ENERGY_RESOURCE;
                List<int> aiResourceInputs = Enumerable.Repeat(0, (int)infos().resourcesNum()).ToList();
                if (!isStopped())
                {
                    int iPowerConsumption = gameServer().powerConsumption(getType(), tileClient(), getOwner());
                    if (ownerServer().isResourceNoBuy(ePower))
                    {
                        if (AppCore.AppGlobals.GameGlobals.ActiveScenario == ScenarioType.NONE || infos().scenario(AppCore.AppGlobals.GameGlobals.ActiveScenario).meClass != ScenarioClassType.TUTORIAL)
                        {
                            if (iPowerConsumption > 0)
                            {
                                if (iPowerConsumption > ownerServer().getResourceRate(ePower))
                                {
                                    if (iPowerConsumption - ownerServer().getResourceRate(ePower) > ownerServer().getResourceExtraStockpile(ePower))
                                    {
                                        bNeedPower = true;
                                        this.setWorked(false);
                                        this.setNeedsPower(bNeedPower);
                                    }
                                }
                            }
                        }
                    }

                    bool bNeedInput = bNeedPower;

                    using (new UnityProfileScope("BuildingServer::doTurn::checkOtherInputs"))
                    {
                        if (!bNeedPower)
                        {
                            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                            {
                                int iMining = resourceMining(eLoopResource, false);
                                if (iMining > 0)
                                {
                                    changeResourceStockpile(eLoopResource, iMining);

                                    ownerServer().changeResourceRate(eLoopResource, iMining);

                                    gameServer().statsServer().changeStat(StatsType.RESOURCE, (int)ResourceStatType.PRODUCED, getOwner(), (int)eLoopResource, iMining);

                                    if (!(infos().HQ(ownerServer().getHQ()).mbNoDeplete) && !(infos().rulesSet(gameServer().getRulesSet()).mbNoDiminishingResources))
                                    {
                                        ((pTile.isAdjacentMining(getOwner())) ? (TileServer)(pTile.getResourceAdjacentTile(eLoopResource)) : pTile).changeResourceMined(eLoopResource, iMining);
                                    }

                                    bWorked = true;
                                }
                            }

                            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                            {
                                aiResourceInputs[(int)eLoopResource] = resourceInput(eLoopResource, false);
                                if (aiResourceInputs[(int)eLoopResource] > 0 && aiResourceInputs[(int)eLoopResource] > getResourceStockpile(eLoopResource))
                                {
                                    TileGroupServer pTileGroup = pTile.tileGroup();

                                    bool bSupplied = false;
                                    if (!bNeedInput)
                                    {
                                        foreach (int iTileID in pTileGroup.getTileSet())
                                        {
                                            BuildingServer pLoopBuilding = gameServer().tileServer(iTileID).buildingServer();

                                            if (pLoopBuilding != null)
                                            {
                                                if (pLoopBuilding != this)
                                                {
                                                    if (pLoopBuilding.getResourceStockpile(eLoopResource) >= aiResourceInputs[(int)eLoopResource])
                                                    {
                                                        pLoopBuilding.changeResourceStockpile(eLoopResource, -aiResourceInputs[(int)eLoopResource]);
                                                        changeResourceStockpile(eLoopResource, aiResourceInputs[(int)eLoopResource]);

                                                        bSupplied = true;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    if (!bSupplied)
                                        bNeedInput = true;
                                }
                            }
                        }
                    }

                    using (new UnityProfileScope("BuildingServer::doTurn::doOutput"))
                    {
                        for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                        {
                            if (!bNeedInput || aiResourceInputs[(int)eLoopResource] > 0)
                            {
                                int iOutput = resourceOutput(eLoopResource, false);

                                int iTotalDiff = iOutput - aiResourceInputs[(int)eLoopResource];
                                if (iTotalDiff != 0)
                                {
                                    if (!bNeedInput)
                                    {
                                        changeResourceStockpile(eLoopResource, iTotalDiff);

                                        if (eLoopResource == ePower)
                                        {
                                            int iStockpile = getResourceStockpile(eLoopResource);
                                            ownerServer().changeResourceStockpile(eLoopResource, iStockpile, true);
                                            changeResourceStockpile(eLoopResource, -iStockpile);
                                        }

                                        ownerServer().changeResourceRate(eLoopResource, iTotalDiff);
                                        ownerServer().changeResourceInput(eLoopResource, -aiResourceInputs[(int)eLoopResource]);
                                        ownerServer().changeResourceOutput(eLoopResource, iOutput);

                                        if (iTotalDiff > 0)
                                        {
                                            gameServer().statsServer().changeStat(StatsType.RESOURCE, (int)ResourceStatType.PRODUCED, getOwner(), (int)eLoopResource, iTotalDiff);
                                        }
                                        else
                                        {
                                            gameServer().statsServer().changeStat(StatsType.RESOURCE, (int)ResourceStatType.BUILDING_CONSUMED, getOwner(), (int)eLoopResource, -iTotalDiff);
                                        }

                                        bWorked = true;
                                    }
                                    else if (iTotalDiff < 0)
                                    {
                                        ownerServer().changeResourceShortfall(eLoopResource, iTotalDiff);
                                    }
                                }
                            }
                        }
                    }

                    if (!bNeedInput)
                    {
                        int iEntertainmentProfit = getEntertainmentProfit();
                        if (iEntertainmentProfit > 0)
                        {
                            ownerServer().changeEntertainment(iEntertainmentProfit);
                            gameServer().statsServer().changeStat(StatsType.BUILDING_CLASS, (int)BuildingStatType.ENTERTAINMENT_REVENUE, getOwner(), (int)getClass(), iEntertainmentProfit);
                            bWorked = true;
                        }

                        OrderType eOrderType = infos().buildingClass(getClass()).meOrderType;
                        if (eOrderType != OrderType.NONE)
                        {
                            if (eOrderType == OrderType.SPECIAL && infos().building(getType()).miCraftSpeed == 0)
                            {
                                bWorked = true;
                            }
                            else
                            {
                                if ((eOrderType == OrderType.HACK) && infos().HQ(ownerServer().getHQ()).mbDoubleHack)
                                {
                                    ownerServer().changeOrderCapacity(eOrderType, 2);
                                }
                                else
                                {
                                    ownerServer().changeOrderCapacity(eOrderType, 1);
                                }

                                if (ownerServer().getOrderInfo(eOrderType, getID()) != null)
                                {
                                    bWorked = true;
                                }
                            }
                        }
                    }

                    if (bWorked) //building.resourceInput() returns 0 for power, so this is needed as well
                    {
                        if (iPowerConsumption > 0)
                        {
                            ResourceType ePowerResource = ownerServer().getEnergyResource();

                            ownerServer().changeResourceStockpile(ePowerResource, -iPowerConsumption, false);
                            ownerServer().changeResourceRate(ePowerResource, -iPowerConsumption);
                            ownerServer().changeResourceInput(ePowerResource, -iPowerConsumption);

                            gameServer().statsServer().changeStat(StatsType.RESOURCE, (int)ResourceStatType.BUILDING_CONSUMED, getOwner(), (int)ePowerResource, iPowerConsumption);
                        }
                    }

                    if (((gameServer().getTurnCount() + getID()) % infos().Globals.AUTO_SUPPLY_TIME) == 0)
                    {
                        supplyBuilding(false);
                    }
                }

                setWorked(bWorked);

                {
                    int iStockpile;
                    for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                    {
                        if (getResourceStockpile(eLoopResource) > 0 && aiResourceInputs[(int)eLoopResource] == 0)
                        {
                            if (ownerServer().isTeleportation() || pTile.isConnectedToHQ())
                            {
                                int iQuantity = getResourceStockpile(eLoopResource);

                                changeResourceStockpile(eLoopResource, -(iQuantity));
                                ownerServer().processResource(eLoopResource, iQuantity);
                            }
                            else if ((iStockpile = getWholeResourceStockpile(eLoopResource)) > 0)
                            {
                                TileGroupServer pTileGroup = pTile.tileGroup();

                                if (pTileGroup != null)
                                {
                                    IEnumerable<BuildingServer> buildingTiles = pTileGroup.getTileSet().Select(tile => gameServer().tileServer(tile).buildingServer()).Where(building => building != null && building.isStopped() && building.wantsInput(eLoopResource));
                                    buildingTiles.OrderBy(building => building.getWholeResourceStockpile(eLoopResource));
                                    
                                    bool bSentAny = true;
                                    while (iStockpile > 0 && bSentAny)
                                    {
                                        bSentAny = false;
                                        foreach (BuildingServer pLoopBuilding in buildingTiles)
                                        {
                                            if (pLoopBuilding.getWholeResourceStockpile(eLoopResource) <= pLoopBuilding.getResourceCapacity(eLoopResource))
                                            {
                                                changeResourceStockpile(eLoopResource, -(Constants.RESOURCE_MULTIPLIER));
                                                pLoopBuilding.changeResourceStockpile(eLoopResource, Constants.RESOURCE_MULTIPLIER);

                                                bSentAny = true;
                                                if (iStockpile-- <= 0)
                                                    break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (!this.tileServer().isConnectedToHQ()) //if the building is connected to HQ, it will be emptying its stockpile into there first
                {
                    HQServer pHQ = ownerServer().findClosestHQServer(pTile);

                    if (pHQ != null)
                    {
                        for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                        {
                            if (!wantsInput(eLoopResource))
                            {
                                if (getWholeResourceStockpile(eLoopResource) > 0)
                                {
                                    int iQuantity = getResourceStockpile(eLoopResource);

                                    bool bShip = isClosed(TeamType.NONE) || iQuantity >= getResourceCapacity(eLoopResource);

                                    if (!bShip)
                                    {
                                        if ((infos().building(getType()).maiResourceMining[(int)eLoopResource] == 0) &&
                                            (GameServer.resourceOutputTile(gameServer(), infos(), getType(), eLoopResource, tileServer()) == 0))
                                        {
                                            bShip = true;
                                        }
                                    }

                                    if (!bShip)
                                    {
                                        if (!(ownerServer().isHuman()) && bWorked)
                                        {
                                            if (iQuantity >= Math.Max((20 * Constants.RESOURCE_MULTIPLIER), (getResourceCapacity(eLoopResource) / 2)))
                                            {
                                                int iRevenue = (iQuantity * gameServer().marketServer().getWholePrice(eLoopResource)) / Constants.RESOURCE_MULTIPLIER;
                                                if (iRevenue > 3000)
                                                {
                                                    if (gameServer().random().Next(Math.Max(10, (getResourceCapacity(eLoopResource) - iQuantity))) == 0)
                                                    {
                                                        int iGasRate = ownerServer().getGas(infos().Globals.SHIP_UNIT);
                                                        if (iGasRate < 0)
                                                        {
                                                            int iTurns = gameServer().getTravelTimeForUnit(infos().Globals.SHIP_UNIT, ownerServer().getHQ(), pTile, pHQ.tileServer());

                                                            int iCost = (((iGasRate * iTurns) * gameServer().marketServer().getWholePrice(ownerServer().getGasResource())) / Constants.RESOURCE_MULTIPLIER);

                                                            if ((iRevenue / 10) > -(iCost))
                                                            {
                                                                bShip = true;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    if (bShip)
                                    {
                                        UnitServer pUnit = ownerServer().createUnit(infos().Globals.SHIP_UNIT, BuildingType.NONE, pTile);

                                        if (pUnit != null)
                                        {
                                            iQuantity = Math.Min(iQuantity, getResourceCapacity(eLoopResource));

                                            changeResourceStockpile(eLoopResource, -(iQuantity));
                                            pUnit.setCargo(eLoopResource, iQuantity);

                                            pUnit.setMissionInfo(MissionType.SHIP_HQ, pHQ.getTileID(), false);

                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    

        public virtual void doAutoSupply()
        {
            if (!isStopped())
            {
                if (ownerServer().isTeleportation() || tileServer().isConnectedToHQ())
                {
                    for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                    {
                        int iInput = resourceInput(eLoopResource, false);
                        if (iInput > 0)
                        {
                            if ((iInput > getResourceStockpile(eLoopResource)) && (iInput <= ownerServer().getResourceStockpile(eLoopResource, true)))
                            {
                                ownerServer().changeResourceStockpile(eLoopResource, -(iInput), true);
                                changeResourceStockpile(eLoopResource, iInput);
                            }

                            if (iInput > getResourceStockpile(eLoopResource))
                            {
                                if (!(ownerServer().isHuman()) || ownerServer().isAutoSupplyBuildingsInput(getType()) || isVirus())
                                {
                                    int iWholeInput = (iInput / Constants.RESOURCE_MULTIPLIER);

                                    if ((iWholeInput * Constants.RESOURCE_MULTIPLIER) < iInput)
                                    {
                                        iWholeInput++;
                                    }

                                    int iCost = (gameServer().marketServer().getWholePrice(eLoopResource) * iWholeInput);

                                    if (!(ownerServer().isHuman()))
                                    {
                                        if (ownerServer().getMoney() < iCost)
                                        {
                                            foreach (InfoResource pOutputResource in infos().resources())
                                            {
                                                if (GameServer.resourceOutputTile(gameServer(), infos(), getType(), pOutputResource.meType, tileServer()) > 0)
                                                {
                                                    ownerServer().trade(pOutputResource.meType, -1, false);
                                                }
                                            }
                                        }

                                        if (ownerServer().getMoney() < iCost)
                                        {
                                            ownerServer().AI_fundResources(iCost, 20, false, false, null, null, false, PlayerType.NONE);
                                        }
                                    }

                                    if (isVirus())
                                    {
                                        if (ownerServer().getMoney() < iCost)
                                        {
                                            int iCredit = (int)(iCost - ownerServer().getMoney());

                                            ownerServer().changeMoney(iCredit);
                                            ownerServer().changeDebt(-(iCredit));
                                        }
                                    }

                                    int iQuantity = ownerServer().trade(eLoopResource, iWholeInput, false);
                                    if (iQuantity > 0)
                                    {
                                        ownerServer().changeWholeResourceStockpile(eLoopResource, -(iQuantity), true);
                                        changeWholeResourceStockpile(eLoopResource, iQuantity);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public virtual void scrap()
        {
            if (!canScrap())
            {
                return;
            }

            sendResources(true);

            if (ownerServer().isRecycleScrap())
            {
                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    ownerServer().changeWholeResourceStockpile(eLoopResource, getResourceCost(eLoopResource), false);
                }
            }

            ownerServer().killBuilding(this);

            if (!(ownerServer().isHuman()))
            {
                ownerServer().AI_updateRates();
            }
        }

        public virtual void sendResources(bool bAll)
        {
            TileServer pTile = tileServer();

            if (ownerServer().isTeleportation() || pTile.isConnectedToHQ())
            {
                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    if ((resourceInput(eLoopResource, false) == 0) || bAll)
                    {
                        int iQuantity = getResourceStockpile(eLoopResource);
                        if (iQuantity > 0)
                        {
                            changeResourceStockpile(eLoopResource, -(iQuantity));
                            ownerServer().processResource(eLoopResource, iQuantity);
                        }
                    }
                }
            }
            else
            {
                HQServer pHQ = ownerServer().findClosestHQServer(pTile);

                if (pHQ != null)
                {
                    for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                    {
                        if ((resourceInput(eLoopResource, false) == 0) || bAll)
                        {
                            int iQuantity = getResourceStockpile(eLoopResource);
                            if (iQuantity > 0)
                            {
                                UnitServer pUnit = ownerServer().createUnit(infos().Globals.SHIP_UNIT, BuildingType.NONE, pTile);

                                if (pUnit != null)
                                {
                                    changeResourceStockpile(eLoopResource, -(iQuantity));
                                    pUnit.setCargo(eLoopResource, iQuantity);

                                    pUnit.setMissionInfo(MissionType.SHIP_HQ, pHQ.getTileID(), false);
                                }
                            }
                        }
                    }
                }
            }
        }

        public virtual void supplyBuilding(bool bForce)
        {
            if (!canSupplyBuilding())
            {
                return;
            }

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                if (wantsInput(eLoopResource))
                {
                    if (bForce || (getResourceSpace(eLoopResource) > getResourceStockpile(eLoopResource)))
                    {
                        if (bForce || !(Utils.isBuildingBeingResupplied(this, eLoopResource, gameServer())))
                        {
                            int iQuantity;

                            if (getResourceSpace(eLoopResource) < ownerServer().getResourceStockpile(eLoopResource, true))
                            {
                                iQuantity = getResourceSpace(eLoopResource);
                            }
                            else
                            {
                                iQuantity = ownerServer().getResourceStockpile(eLoopResource, true);

                                if (!(ownerServer().isHuman()) ||
                                    (ownerServer().isAutoSupplyBuildingsInput(getType()) && !(ownerServer().isManualSale())) ||
                                    (isVirus()))
                                {
                                    int iDiff = ((getResourceSpace(eLoopResource) - ownerServer().getResourceStockpile(eLoopResource, true)) / Constants.RESOURCE_MULTIPLIER) + 1;

                                    int iCost = gameServer().marketServer().calculateBuyCost(eLoopResource, iDiff);

                                    if (!(ownerServer().isHuman()))
                                    {
                                        if (ownerServer().getMoney() < iCost)
                                        {
                                            foreach (InfoResource pOutputResource in infos().resources())
                                            {
                                                if (GameServer.resourceOutputTile(gameServer(), infos(), getType(), pOutputResource.meType, tileServer()) > 0)
                                                {
                                                    ownerServer().trade(pOutputResource.meType, -(iDiff), false);
                                                }
                                            }
                                        }

                                        if (ownerServer().getMoney() < iCost)
                                        {
                                            ownerServer().AI_fundResources(iCost, 20, true, false, null, null, false, PlayerType.NONE);
                                        }
                                    }

                                    if (isVirus())
                                    {
                                        if (ownerServer().getMoney() < iCost)
                                        {
                                            int iCredit = (int)(iCost - ownerServer().getMoney());

                                            ownerServer().changeMoney(iCredit);
                                            ownerServer().changeDebt(-(iCredit));
                                        }
                                    }

                                    iQuantity += (ownerServer().trade(eLoopResource, iDiff, false) * Constants.RESOURCE_MULTIPLIER);
                                }
                            }

                            if (iQuantity >= Constants.RESOURCE_MULTIPLIER)
                            {
                                HQServer pHQ = ownerServer().findClosestHQServer(tileServer());

                                if (pHQ != null)
                                {
                                    UnitServer pUnit = ownerServer().createUnit(infos().Globals.SHIP_UNIT, BuildingType.NONE, pHQ.tileServer());

                                    if (pUnit != null)
                                    {
                                        ownerServer().changeResourceStockpile(eLoopResource, -(iQuantity), true);
                                        pUnit.setCargo(eLoopResource, iQuantity);

                                        pUnit.setMissionInfo(MissionType.SHIP_BUILDING, getTileID(), false);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public virtual TileServer tileServer()
        {
            return (TileServer)tileClient();
        }

        public virtual void changeConnections(int iChange)
        {
            if (iChange != 0)
            {
                miConnections += iChange;

                makeDirty(BuildingDirtyType.miConnections);
            }
        }

        public virtual void setWorked(bool bNewValue)
        {
            if (isWorked() != bNewValue)
            {
                mbWorked = bNewValue;

                makeDirty(BuildingDirtyType.mbWorked);
            }
        }

        public virtual void toggleOff()
        {
            mbOff = !isOff();

            if (infos().building(getType()).miEntertainment > 0)
            {
                gameServer().doEntertainmentSupply();
            }

            makeDirty(BuildingDirtyType.mbOff);
        }
        public virtual void toggleOffManual()
        {
            toggleOff();

            if (isAutoOff())
            {
                toggleAutoOff();
            }
        }

        public virtual void toggleAutoOff()
        {
            if (!(tileClient().isOwnerReal()))
            {
                return;
            }

            mbAutoOff = !isAutoOff();

            makeDirty(BuildingDirtyType.mbAutoOff);
        }

        public virtual PlayerServer ownerServer()
        {
            return (PlayerServer)ownerClient();
        }
        public virtual void setOwner(PlayerType eNewValue)
        {
            PlayerType eOldValue = getOwner();

            if (eOldValue != eNewValue)
            {
                if (eOldValue != PlayerType.NONE)
                {
                    ownerServer().changeBuildingCount(getType(), -1);

                    ownerServer().removeBuilding(this);
                }

                meOwner = eNewValue;

                if (eNewValue != PlayerType.NONE)
                {
                    ownerServer().changeBuildingCount(getType(), 1);

                    ownerServer().addBuilding(getID());
                }

                if (infos().building(getType()).mbConnections)
                {
                    if (eOldValue != PlayerType.NONE)
                    {
                        gameServer().playerServer(eOldValue).updateConnectedBuildings();
                    }
                    if (eNewValue != PlayerType.NONE)
                    {
                        gameServer().playerServer(eNewValue).updateConnectedBuildings();
                    }
                }

                makeDirty(BuildingDirtyType.meOwner);
            }
        }

        public virtual void setConnectedBuilding(DirectionType eIndex, int iNewValue)
        {
            int iOldValue = getConnectedBuilding(eIndex);
            if (iOldValue != iNewValue)
            {
                maiConnectedBuildings[(int)eIndex] = iNewValue;

                if (iOldValue == -1)
                {
                    changeConnections(1);
                }
                else if (iNewValue == -1)
                {
                    changeConnections(-1);
                }

                makeDirty(BuildingDirtyType.maiConnectedBuildings);
            }
        }
        public virtual void updateConnectedBuildings()
        {
            if (!(infos().building(getType()).mbConnections))
            {
                return;
            }

            {
                TileServer pTile = tileServer();

                for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                {
                    TileServer pAdjacentTile = gameServer().tileServerAdjacent(pTile, eLoopDirection);

                    if (pAdjacentTile != null)
                    {
                        BuildingServer pBuilding = pAdjacentTile.buildingServer();

                        if ((pBuilding != null) &&
                            (pBuilding.getOwner() == getOwner()) &&
                            (BuildingServer.isConnectionValid(getType(), pBuilding.getType(), pAdjacentTile, ownerServer(), gameServer(), infos())))
                        {
                            setConnectedBuilding(eLoopDirection, pBuilding.getID());
                        }
                        else
                        {
                            setConnectedBuilding(eLoopDirection, -1);
                        }
                    }
                }
            }
        }

        public virtual void changeResourceStockpile(ResourceType eIndex, int iChange)
        {
            if (iChange != 0)
            {
                maiResourceStockpile[(int)eIndex] += iChange;

                makeDirty(BuildingDirtyType.maiResourceStockpile);
            }
        }
        public virtual void changeWholeResourceStockpile(ResourceType eIndex, int iChange)
        {
            changeResourceStockpile(eIndex, (iChange * Constants.RESOURCE_MULTIPLIER));
        }

        public virtual void setResourceCost(ResourceType eIndex, int iNewValue)
        {
            if (getResourceCost(eIndex) != iNewValue)
            {
                maiResourceCost[(int)eIndex] = iNewValue;

                makeDirty(BuildingDirtyType.maiResourceCost);
            }
        }
    }
}