using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using System.IO;
using Offworld.GameCore.Text;
using Offworld.SystemCore;
using UnityEngine.Profiling;

namespace Offworld.GameCore
{
    public partial class PlayerServer : PlayerClient
    {
        public static readonly bool AI_LOGGING = (Globals.AppInfo.IsInternalBuild && false);
        public static readonly bool VALIDATE_OPTIMIZATIONS = (Globals.AppInfo.IsInternalBuild && false); //DO NOT CHECK IN ENABLED!

        List<int> maiResourceValue;
        List<int> maiResourceValueSaveUpgrade;
        List<int> maiStockValueSorted;
        List<PlayerType> maiStockValueSortedIndex;

        const int NUM_TURNS = 100;

        public virtual void AI_quip(PlayerType ePlayer, TextType eText)
        {
            if (eText == TextType.NONE)
            {
                return;
            }

            if (gameServer().getTurnCount() < (AI_getAILastQuip() + infos().personality(getPersonality()).miQuipDelay))
            {
                return;
            }

            if (isWinEligible())
            {
                gameServer().gameEventsServer().AddMessageEvent(getPlayer(), ePlayer, eText);
                AI_setAILastQuip(gameServer().getTurnCount());
            }
        }

        public virtual void AI_updateLifeSupport()
        {
            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                int iLife = lifeSupport(eLoopResource);
                if (iLife < 0)
                {
                    AI_changeResourceRateAverage(eLoopResource, iLife);
                    AI_changeResourceLifeSupport(eLoopResource, iLife);
                }
            }
        }

        public virtual void AI_updateBuilding(BuildingServer pBuilding)
        {
            TileServer pTile = pBuilding.tileServer();

            if (pTile.getRealOwner() == getPlayer())
            {
                int iConnections = pTile.countConnections(pBuilding.getType(), getPlayer(), true, false);

                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    int iRate = gameServer().calculateRate(pBuilding.getType(), eLoopResource, pTile, getPlayer(), iConnections, true, 100);

                    AI_changeResourceRateAverage(eLoopResource, iRate);

                    if (iRate > 0)
                    {
                        AI_changeResourceProductionCount(eLoopResource, 1);
                    }
                }
            }
        }

        public virtual void AI_updateConstruction(ConstructionServer pConstruction)
        {
            TileServer pTile = pConstruction.tileServer();

            if (pTile.getRealOwner() == getPlayer())
            {
                int iConnections = pTile.countConnections(pConstruction.getType(), getPlayer(), true, false);

                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    int iRate = gameServer().calculateRate(pConstruction.getType(), eLoopResource, pTile, getPlayer(), iConnections, true, 100);

                    AI_changeResourceRateAverage(eLoopResource, iRate);

                    if (iRate > 0)
                    {
                        AI_changeResourceConstructionCount(eLoopResource, 1);
                        AI_changeResourceProductionCount(eLoopResource, 1);
                    }
                }
            }
        }

        public virtual void AI_updateUnit(UnitServer pUnit)
        {
            {
                int iGasRate = getGas(pUnit.getType());
                if (iGasRate < 0)
                {
                    AI_changeResourceRateAverage(getGasResource(), iGasRate);
                }
            }

            if (pUnit.getConstructionType() != BuildingType.NONE)
            {
                TileServer pTargetTile = gameServer().tileServer(pUnit.getMissionInfo().miData);

                if (pTargetTile != null)
                {
                    int iConnections = pTargetTile.countConnections(pUnit.getConstructionType(), pUnit.getOwner(), true, false);

                    for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                    {
                        int iRate = gameServer().calculateRate(pUnit.getConstructionType(), eLoopResource, pTargetTile, getPlayer(), iConnections, true, 100);

                        AI_changeResourceRateAverage(eLoopResource, iRate);

                        if (iRate > 0)
                        {
                            AI_changeResourceConstructionCount(eLoopResource, 1);
                            AI_changeResourceProductionCount(eLoopResource, 1);
                        }
                    }
                }
            }
        }

        public virtual void AI_updateRates()
        {
            using (new UnityProfileScope("Player::AI_updateRates"))
            {
                AI_clearResourceRateAverage();
                AI_clearResourceLifeSupport();
                AI_clearResourceConstructionCount();
                AI_clearResourceProductionCount();
                AI_clearCachedValues();

                AI_updateLifeSupport();

                foreach (KeyValuePair<int, BuildingClient> pair in gameServer().getBuildingDictionary())
                {
                    AI_updateBuilding((BuildingServer)(pair.Value));
                }

                foreach (KeyValuePair<int, ConstructionClient> pair in gameServer().getConstructionDictionary())
                {
                    AI_updateConstruction((ConstructionServer)(pair.Value));
                }

                foreach (int iUnitID in getUnitList())
                {
                    AI_updateUnit(gameServer().unitServer(iUnitID));
                }
            }
        }

        public virtual int AI_getResourceRateAverage(ResourceType eIndex)
        {
            return maiAIResourceRateAverage[(int)eIndex];
        }
        public virtual int AI_getWholeResourceRateAverage(ResourceType eIndex)
        {
            return (AI_getResourceRateAverage(eIndex) / Constants.RESOURCE_MULTIPLIER);
        }
        void AI_setResourceRateAverage(ResourceType eIndex, int iNewValue)
        {
            maiAIResourceRateAverage[(int)eIndex] = iNewValue;
        }
        public virtual void AI_changeResourceRateAverage(ResourceType eIndex, int iChange)
        {
            AI_setResourceRateAverage(eIndex, AI_getResourceRateAverage(eIndex) + iChange);
        }
        protected virtual void AI_clearResourceRateAverage()
        {
            for (ResourceType eResource = 0; eResource < infos().resourcesNum(); eResource++)
            {
                AI_setResourceRateAverage(eResource, 0);
            }
        }

        public virtual int AI_getResourceLifeSupport(ResourceType eIndex)
        {
            return maiAIResourceLifeSupport[(int)eIndex];
        }
        public virtual void AI_setResourceLifeSupport(ResourceType eIndex, int iNewValue)
        {
            maiAIResourceLifeSupport[(int)eIndex] = iNewValue;
        }
        public virtual void AI_changeResourceLifeSupport(ResourceType eIndex, int iChange)
        {
            AI_setResourceLifeSupport(eIndex, AI_getResourceLifeSupport(eIndex) + iChange);
        }
        protected virtual void AI_clearResourceLifeSupport()
        {
            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                AI_setResourceLifeSupport(eLoopResource, 0);
            }
        }

        public virtual int AI_getResourceConstructionCount(ResourceType eIndex)
        {
            return maiAIResourceConstructionCount[(int)eIndex];
        }
        public virtual void AI_setResourceConstructionCount(ResourceType eIndex, int iNewValue)
        {
            maiAIResourceConstructionCount[(int)eIndex] = iNewValue;
        }
        public virtual void AI_changeResourceConstructionCount(ResourceType eIndex, int iChange)
        {
            AI_setResourceConstructionCount(eIndex, AI_getResourceConstructionCount(eIndex) + iChange);
        }
        protected virtual void AI_clearResourceConstructionCount()
        {
            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                AI_setResourceConstructionCount(eLoopResource, 0);
            }
        }

        public virtual int AI_getResourceProductionCount(ResourceType eIndex)
        {
            return maiAIResourceProductionCount[(int)eIndex];
        }
        public virtual int AI_getResourceProductionCountTeam(ResourceType eIndex)
        {
            if (!(gameServer().isTeamGame()) || isSubsidiary())
            {
                return AI_getResourceProductionCount(eIndex);
            }
            else
            {
                int iCount = 0;

                for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameServer().getNumPlayers(); eLoopPlayer++)
                {
                    PlayerServer pLoopPlayer = gameServer().playerServer(eLoopPlayer);

                    if (!(pLoopPlayer.isSubsidiary()))
                    {
                        if (pLoopPlayer.getTeam() == getTeam())
                        {
                            iCount += AI_getResourceProductionCount(eIndex);
                        }
                    }
                }

                return iCount;
            }
        }
        public virtual void AI_setResourceProductionCount(ResourceType eIndex, int iNewValue)
        {
            maiAIResourceProductionCount[(int)eIndex] = iNewValue;
        }
        public virtual void AI_changeResourceProductionCount(ResourceType eIndex, int iChange)
        {
            AI_setResourceProductionCount(eIndex, AI_getResourceProductionCount(eIndex) + iChange);
        }
        protected virtual void AI_clearResourceProductionCount()
        {
            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                AI_setResourceProductionCount(eLoopResource, 0);
            }
        }
        protected virtual void AI_clearCachedValues()
        {
            if(maiResourceValue == null || maiResourceValueSaveUpgrade == null)
            {
                AI_initFundResourcesValueCache();
            }

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                maiResourceValue[(int)eLoopResource] = -1;
                maiResourceValueSaveUpgrade[(int)eLoopResource] = -1;
            }

            for (int playerIndex = 0; playerIndex < (int)gameServer().getNumPlayers(); playerIndex++)
            {
                maiStockValueSorted[playerIndex] = -1;
                maiStockValueSortedIndex[playerIndex] = PlayerType.NONE;
            }
        }

        public virtual void AI_doUpdate()
        {
            AI_updateRates();

            if (isHuman())
            {
                return;
            }

            using (new UnityProfileScope("Player::AI_doUpdate"))
            {
                if (isHQFounded())
                {
                    if (isSubsidiary())
                    {
                        sellAllResources(false);

                        AI_doConstruct(16);

                        AI_turnOffBuildings();

                        AI_doRepair();

                        AI_doLaunch();

                        AI_doUpgradeHQ(false);

                        payDebt();

                        return;
                    }

                    {
                        bool bConstructed = false;

                        if (AI_getForceConstruct() > 0)
                        {
                            if (((gameServer().getTurnCount() + (int)(getPlayer())) % 4) == 0)
                            {
                                bConstructed = AI_doConstruct(1);

                                AI_changeForceConstruct(-1);
                            }
                        }
                        else
                        {
                            bConstructed = AI_doConstruct((gameServer().getTurnCount() % Constants.MAX_NUM_PLAYERS) == (int)(getPlayer()) ? 1 : Constants.MAX_NUM_PLAYERS);
                        }

                        if (bConstructed)
                        {
                            return;
                        }
                    }

                    if (AI_scrapBuildings())
                    {
                        return;
                    }

                    AI_doImport();

                    AI_turnOffBuildings();

                    AI_doStock();

                    AI_doLaunch();

                    if (AI_doUpgradeHQ(false))
                    {
                        return;
                    }

                    AI_doDebt();

                    AI_doBlackMarketOther();

                    AI_doBlackMarketSabotage();

                    AI_doBlackMarketDefend();

                    AI_doBlackMarketAttack();

                    AI_doSabotage();

                    AI_doRepair();

                    AI_doPatent();

                    AI_doResearch();

                    AI_doEspionage();

                    AI_doTeam();

                    AI_doMarket();
                }
                else
                {
                    AI_doScan();

                    AI_doFound(false);
                }
            }
        }

        private bool [] maCanScanTilesCache = null;
        public virtual void AI_doScan()
        {
            using (new UnityProfileScope("Player::AI_doScan"))
            {
                if (!canScan())
                {
                    return;
                }

                GameServer pGame = gameServer();
                if (pGame.random().Next((pGame.getScanDelay() / 2) + 1) != 0)
                {
                    return;
                }

                TileServer pBestTile = null;
                int iBestValue = 0;
                int iRange = innerScanRange();
                int iSearchRatio = 0;
                MapClient pMapClient = gameServer().mapClient();
                PlayerType ePlayer = getPlayer();

                //precalculate canScan(pTile) efficiently by sharing calculations.  This results in a 8x speedup over calling canScan(pTile) on each tile (8ms->1ms).
                using (new UnityProfileScope("Player::canScanTilesCache"))
                {
                    //clear cache
                    ArrayUtilities.SetSize(ref maCanScanTilesCache, pMapClient.numTiles());
                    ArrayUtilities.Fill(maCanScanTilesCache, false);

                    //populate cache
                    for (int i=0; i<maCanScanTilesCache.Length; i++)
                    {
                        TileClient pTile = pMapClient.tileClient(i);
                        if (pTile.getVisibility(ePlayer) == VisibilityType.REVEALED)
                        {
                            maCanScanTilesCache[i] = true;
                            for (DirectionType eDirection=0; eDirection<DirectionType.NUM_TYPES; eDirection++)
                            {
                                TileClient pAdjacentTile = pMapClient.tileClientAdjacent(pTile, eDirection);
                                if (pAdjacentTile != null)
                                {
                                    maCanScanTilesCache[pAdjacentTile.getID()] = true;
                                }
                            }
                        }
                    }

                    //validate that the optimized cache matches the ground truth
                    if (VALIDATE_OPTIMIZATIONS)
                    {
                        Assert.IsTrue(maCanScanTilesCache.SequenceEqual(pMapClient.tileClientAll().Select(x => canScan(x))));
                    }
                }

                using (new UnityProfileScope("Player::numScannableTiles"))
                {
                    int iNumScannableTiles = 0;
                    for (int i=0; i<maCanScanTilesCache.Length; i++)
                    {
                        if (maCanScanTilesCache[i])
                        {
                            iNumScannableTiles++;
                        }
                    }
                    const int TARGET_SEARCH_TILES = 100;
                    iSearchRatio = iNumScannableTiles / TARGET_SEARCH_TILES;
                }

                using (new UnityProfileScope("Player::scannableTileLoop"))
                {
                    for(int i=0; i<maCanScanTilesCache.Length; i++)
                    {
                        if (maCanScanTilesCache[i])
                        {
                            if (pGame.random().Next(iSearchRatio + 1) == 0)
                            {
                                int iValue = 0;
                                TileClient pLoopTile = pMapClient.tileClient(i);

                                for (int iDX = -(iRange); iDX <= iRange; iDX++)
                                {
                                    for (int iDY = -(iRange); iDY <= iRange; iDY++)
                                    {
                                        TileClient pRangeTile = pGame.tileClientRange(pLoopTile, iDX, iDY, iRange);
                                        if (pRangeTile != null && !pRangeTile.isClaimed())
                                        {
                                            VisibilityType eRangeVisibility = pRangeTile.getVisibility(getPlayer());
                                            if (eRangeVisibility != VisibilityType.VISIBLE)
                                            {
                                                if (eRangeVisibility == VisibilityType.REVEALED)
                                                {
                                                    iValue += (pRangeTile.getResourceCount() * 500);
                                                }
                                                else
                                                {
                                                    iValue += ((pRangeTile.usable()) ? 100 : 50);
                                                }
                                            }
                                        }
                                    }
                                }

                                if (iValue > 0)
                                {
                                    iValue += pGame.random().Next(1000) + pGame.random().Next(1000);

                                    if (iValue > iBestValue)
                                    {
                                        pBestTile = (TileServer)pLoopTile;
                                        iBestValue = iValue;
                                    }
                                }
                            }
                        }
                    }
                }

                if (pBestTile != null)
                {
                    scan(pBestTile);
                }
            }
        }

        protected virtual int AI_HQFoundBaseValue(InfoHQ HQInfo)
        {
            int iBaseValue = 0;

            using (new UnityProfileScope("Player::AI_HQFoundBaseValue"))
            {
                {
                    iBaseValue += (HQInfo.miExtraHQs * 120);
                    iBaseValue += (HQInfo.miBuildingHQLevel * -20);
                    iBaseValue += (HQInfo.miClaims * 60);
                    iBaseValue += (HQInfo.miClaimsPerUpgrade * 120);
                    iBaseValue += (HQInfo.miShares * 40);
                    iBaseValue += (HQInfo.miMovementModifier / 2);
                    iBaseValue += (HQInfo.miBlackMarketHostileTimeModifier * -4);
                    iBaseValue += (HQInfo.miFrozenEffectModifier * -2);

                    if (HQInfo.mbAdjacentInputBonus)
                    {
                        iBaseValue += 100;
                    }

                    if (HQInfo.mbEarlyEventAnnounce)
                    {
                        iBaseValue += 80;
                    }

                    if (HQInfo.mbDoubleHack)
                    {
                        iBaseValue += 20;
                    }

                    if (HQInfo.mbLicensePatents)
                    {
                        iBaseValue += 40;
                    }
                }

                {
                    int iBaseResourceCost = infos().resource(HQInfo.meBaseResource).miMarketPrice;

                    iBaseResourceCost *= Math.Max(0, (HQInfo.miBaseResourceModifier + 100));
                    iBaseResourceCost /= 100;

                    iBaseValue += (Constants.WHOLE_PRICE_MAX * 5) / iBaseResourceCost;
                }

                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    int iStockpile = HQInfo.maiInitialStockpile[(int)eLoopResource];
                    if (iStockpile > 0)
                    {
                        iBaseValue += ((iStockpile * gameServer().marketServer().getWholePrice(eLoopResource)) / 100);
                    }
                }

                for (SabotageType eLoopSabotage = 0; eLoopSabotage < infos().sabotagesNum(); eLoopSabotage++)
                {
                    iBaseValue += (HQInfo.maiFoundSabotage[(int)eLoopSabotage] * 20);
                    iBaseValue += (HQInfo.maiUpgradeSabotage[(int)eLoopSabotage] * 40);
                }

                for (OrderType eLoopOrder = 0; eLoopOrder < infos().ordersNum(); eLoopOrder++)
                {
                    iBaseValue += (HQInfo.maiOrderModifier[(int)eLoopOrder] / 2);
                }

                for (TechnologyLevelType eLoopTechnologyLevel = 0; eLoopTechnologyLevel < infos().technologyLevelsNum(); eLoopTechnologyLevel++)
                {
                    iBaseValue += (infos().technologyLevel(eLoopTechnologyLevel).maiHQClaims[HQInfo.miType] * 40);
                }

                for (BuildingType eLoopBuilding = 0; eLoopBuilding < infos().buildingsNum(); eLoopBuilding++)
                {
                    if (!(gameServer().isBuildingUnavailable(eLoopBuilding)))
                    {
                        if (Utils.isBuildingValid(eLoopBuilding, HQInfo.meType))
                        {
                            iBaseValue += (infos().building(eLoopBuilding).miEntertainment * 20);
                            iBaseValue += (infos().building(eLoopBuilding).miEntertainmentModifier / 2);

                            if (infos().building(eLoopBuilding).mbSelfInput)
                            {
                                iBaseValue += 50;
                            }
                        }
                    }
                }

                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    ResourceType eReplaceResource = HQInfo.maeResourceReplace[(int)eLoopResource];

                    if (eReplaceResource != ResourceType.NONE)
                    {
                        iBaseValue += (Math.Max(0, (infos().resource(eLoopResource).miMarketPrice - infos().resource(eReplaceResource).miMarketPrice)) + 40);
                    }
                }

                if (getPersonality() != PersonalityType.NONE)
                {
                    iBaseValue *= Math.Max(0, (infos().personality(getPersonality()).maiHQModifier[HQInfo.miType] + 100));
                    iBaseValue /= 100;
                }

                foreach (KeyValuePair<int, HQClient> pair in gameServer().getHQDictionary())
                {
                    if ((pair.Value).getType() == HQInfo.meType)
                    {
                        iBaseValue *= 2;
                        iBaseValue /= 3;
                    }
                }

                return Math.Max(1, iBaseValue);
            }
        }

        public virtual HQServer AI_doFound(bool bForce)
        {
            using (new UnityProfileScope("PlayerServer.AI_doFound"))
            {
                if (!canFound())
                {
                    return null;
                }

                if (!bForce)
                {
                    using (new UnityProfileScope("PlayerServer.skipFound"))
                    {
                        if (gameServer().getGameUpdateCount() < 10)
                        {
                            return null;
                        }

                        if (gameServer().isGameOption(GameOptionType.REVEAL_MAP))
                        {
                            if (infos().bond(calculateBondRatingFound()).mbNoBlackMarketSabotage)
                            {
                                return null;
                            }
                        }

                        if ((gameServer().countPlayersStarted() < (int)(gameServer().getNumPlayers() - 1)) && (getNumHQs() == 0))
                        {
                            if (gameServer().isGameOption(GameOptionType.REVEAL_MAP))
                            {
                                if (gameServer().random().Next(10) != 0)
                                {
                                    return null;
                                }
                            }
                            else
                            {
                                if (gameServer().getTurnCount() > 0)
                                {
                                    if ((gameServer().getTurnCount() % Constants.MAX_NUM_PLAYERS) != ((int)(getPlayer()) * (((int)(gameServer().getNumPlayers()) <= (Constants.MAX_NUM_PLAYERS / 2)) ? 2 : 1)))
                                    {
                                        return null;
                                    }
                                }
                                else
                                {
                                    if (gameServer().random().Next(Constants.MAX_NUM_PLAYERS) != 0)
                                    {
                                        return null;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (gameServer().random().Next(5) == 0)
                            {
                                return null;
                            }
                        }

                        if (gameServer().getHQLevels() < infos().rulesSet(gameServer().getRulesSet()).miAIFoundMinLevel)
                        {
                            return null;
                        }
                    }
                }

                MapClient pMapClient = gameClient().mapClient();
                List<TileClient> aTileClients = pMapClient.tileClientAll();
                TileServer pBestTile = null;
                HQType eBestHQ = HQType.NONE;
                int iBestFoundValue = 0;
                int iBestBonusValue = 0;

                int iBestPossibleValue = 0;

                List<TileServer> apValidTiles = new List<TileServer>();

                using (new UnityProfileScope("PlayerServer.validFoundTiles"))
                {
                    foreach (TileServer pLoopTile in pMapClient.tileClientAll())
                    {
                        if (canFound(pLoopTile, HQType.NONE, false))
                        {
                            apValidTiles.Add(pLoopTile);
                        }
                    }
                }

                using (new UnityProfileScope("PlayerServer.findBestFoundHQ"))
                {
                    int iMaxDistance = Utils.maxStepDistance(gameServer());

                    List<TileServer> aHQTiles = new List<TileServer>();

                    foreach (HQClient pLoopHQ in gameServer().getHQDictionary().Values)
                    {
                        aHQTiles.Add(pLoopHQ.tileClient() as TileServer);
                    }

                    List<TileServer> aOwnHQTiles = new List<TileServer>();

                    foreach (int iLoopHQ in getHQList())
                    {
                        aOwnHQTiles.Add(gameServer().hqServer(iLoopHQ).tileServer());
                    }

                    for (int iPass = ((getNumHQs() > 0) ? 1 : 0); iPass < ((getMapRevealedPercent() < 75) ? 1 : 2); iPass++)
                    {
                        using (new UnityProfileScope("Player::foundPass" + iPass))
                        {
                            foreach (InfoHQ loopHQInfo in infos().HQs())
                            {
                                HQType eLoopHQ = loopHQInfo.meType;

                                if (canFound(eLoopHQ))
                                {
                                    bool bValid = ((iPass == 0) ? false : true);

                                    if (!bValid)
                                    {
                                        bValid = AI_checkFoundBuildingValid(bValid, loopHQInfo);

                                        if (iPass == 0)
                                        {
                                            bValid = AI_checkFoundResourceValid(bValid, loopHQInfo);
                                        }
                                    }

                                    if (iPass == 0)
                                    {
                                        bValid = AI_checkFoundRulesValid(bValid, loopHQInfo);                                        
                                    }

                                    if (bValid)
                                    {
                                        int iBaseValue = AI_HQFoundBaseValue(loopHQInfo);
                                        if (iBaseValue > 0)
                                        {
                                            using (new UnityProfileScope("PlayerServer.findBestHQFoundTile"))
                                            {
                                                foreach (TileServer pLoopTile in apValidTiles)
                                                {
                                                    if ((iPass == 0) ? pLoopTile.isHQMinimum(eLoopHQ) : true)
                                                    {
                                                        if (canFound(pLoopTile, eLoopHQ, false))
                                                        {
                                                            int iValue = iBaseValue;

                                                            bool bResourceFound = false;
                                                            bool bClaimBlock = false;

                                                            using (new UnityProfileScope("PlayerServer.adjacentFoundValues"))
                                                            {
                                                                for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
                                                                {
                                                                    TileServer pAdjacentTile = (TileServer) pMapClient.tileClientAdjacent(pLoopTile, eDirection);

                                                                    if (pAdjacentTile != null)
                                                                    {
                                                                        if (loopHQInfo.mabFootprint[(int)eDirection])
                                                                        {
                                                                            for (DirectionType eDirectionNext = 0; eDirectionNext < DirectionType.NUM_TYPES; eDirectionNext++)
                                                                            {
                                                                                TileClient pNextTile = pMapClient.tileClientAdjacent(pAdjacentTile, eDirectionNext);

                                                                                if (pNextTile != null)
                                                                                {
                                                                                    if ((pNextTile.getResourceCount() > 0) || pNextTile.isGeothermal() || pNextTile.isTerrainRate())
                                                                                    {
                                                                                        bResourceFound = true;
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            if (!(pAdjacentTile.isClaimed()) && pAdjacentTile.usable())
                                                                            {
                                                                                if (pAdjacentTile.isClaimBlock())
                                                                                {
                                                                                    bClaimBlock = true;
                                                                                }

                                                                                if (getNumHQs() > 0)
                                                                                {
                                                                                    iValue += ((pAdjacentTile.getHQValueAverage() + pAdjacentTile.getHQValue(eLoopHQ)) / 2);
                                                                                }
                                                                                else
                                                                                {
                                                                                    iValue += pAdjacentTile.getHQValue(eLoopHQ);
                                                                                }

                                                                                iValue += 6;
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }

                                                            using (new UnityProfileScope("PlayerServer.rangeFoundValues"))
                                                            {
                                                                const int RANGE = 3;

                                                                for(int iRadius=2; iRadius<=RANGE; iRadius++)
                                                                {
                                                                    foreach(int iRangeTileID in pMapClient.tileRingIds(pLoopTile.getID(), iRadius))
                                                                    {
                                                                        TileServer pRangeTile = (TileServer) aTileClients[iRangeTileID];

                                                                        if (!(pRangeTile.isClaimed()) && pRangeTile.usable())
                                                                        {
                                                                            if (pRangeTile.isClaimBlock())
                                                                            {
                                                                                bClaimBlock = true;
                                                                            }

                                                                            int iTemp = 0;

                                                                            if (getNumHQs() > 0)
                                                                            {
                                                                                iTemp += ((pRangeTile.getHQValueAverage() + pRangeTile.getHQValue(eLoopHQ)) / 2);
                                                                            }
                                                                            else
                                                                            {
                                                                                iTemp += pRangeTile.getHQValue(eLoopHQ);
                                                                            }

                                                                            if (pRangeTile.getArea() == pLoopTile.getArea())
                                                                            {
                                                                                iTemp += 6;
                                                                            }

                                                                            iTemp *= (RANGE);
                                                                            iTemp /= (RANGE + iRadius);

                                                                            iValue += iTemp;
                                                                        }
                                                                    }
                                                                }
                                                            }

                                                            if ((iPass == 0) ? !bClaimBlock : true)
                                                            {
                                                                if (!bResourceFound)
                                                                {
                                                                    iValue /= 2;
                                                                }

                                                                using (new UnityProfileScope("PlayerServer.HQFoundDistances"))
                                                                {
                                                                    for (int i = 0; i < aHQTiles.Count; i++)
                                                                    {
                                                                        int iDistance = Utils.stepDistanceTile(aHQTiles[i], pLoopTile);
                                                                        if (iDistance <= 4)
                                                                        {
                                                                            iValue *= 1;
                                                                            iValue /= 4;
                                                                        }
                                                                        else if (iDistance <= 6)
                                                                        {
                                                                            iValue *= 2;
                                                                            iValue /= 4;
                                                                        }
                                                                        else if (iDistance <= 8)
                                                                        {
                                                                            iValue *= 3;
                                                                            iValue /= 4;
                                                                        }
                                                                    }
                                                                }

                                                                if (pLoopTile.getArea() != -1)
                                                                {
                                                                    int iAreaTiles = gameServer().getAreaTiles(pLoopTile.getArea());

                                                                    if (getNumHQs() > 0)
                                                                    {
                                                                        if (iAreaTiles < 5)
                                                                        {
                                                                            iValue /= 4;
                                                                        }
                                                                        else if (iAreaTiles < 10)
                                                                        {
                                                                            iValue /= 2;
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        if (iAreaTiles < 10)
                                                                        {
                                                                            iValue /= 8;
                                                                        }
                                                                        else if (iAreaTiles < 15)
                                                                        {
                                                                            iValue /= 4;
                                                                        }
                                                                        else if (iAreaTiles < 20)
                                                                        {
                                                                            iValue /= 2;
                                                                        }
                                                                    }
                                                                }

                                                                if (getNumHQs() > 0)
                                                                {
                                                                    int iDist = findClosestHQClientDistance(pLoopTile);
                                                                    if (iDist < 20)
                                                                    {
                                                                        iValue *= (10 + iDist);
                                                                        iValue /= (10 + 20);
                                                                    }
                                                                }

                                                                for (int i = 0; i < aOwnHQTiles.Count; i++)
                                                                {
                                                                    iValue *= ((Utils.stepDistanceTile(aOwnHQTiles[i], pLoopTile) + iMaxDistance) / 2);
                                                                    iValue /= iMaxDistance;
                                                                }

                                                                if ((iPass == 0) ? gameServer().AI_isTileFoundMinimum(pLoopTile, eLoopHQ, getPlayer()) : true)
                                                                {
                                                                    using (new UnityProfileScope("PlayerServer.updateBestFoundValue"))
                                                                    {
                                                                        if (iValue >= iBestFoundValue)
                                                                        {
                                                                            if (pLoopTile.getVisibility(getPlayer()) == VisibilityType.VISIBLE)
                                                                            {
                                                                                if (canFound(pLoopTile, eLoopHQ, true))
                                                                                {
                                                                                    int iBonusValue = 0;

                                                                                    for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
                                                                                    {
                                                                                        if (loopHQInfo.mabFootprint[(int)eDirection])
                                                                                        {
                                                                                            TileServer pAdjacentTile = gameServer().tileServerAdjacent(pLoopTile, eDirection);

                                                                                            if (pAdjacentTile != null)
                                                                                            {
                                                                                                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                                                                                {
                                                                                                    iBonusValue += HQServer.resourceBonus(eLoopResource, pLoopTile, getPlayer(), eLoopHQ, gameServer(), infos());
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }

                                                                                    if ((iValue > iBestFoundValue) || (iBonusValue > iBestBonusValue))
                                                                                    {
                                                                                        pBestTile = pLoopTile;
                                                                                        eBestHQ = eLoopHQ;
                                                                                        iBestFoundValue = iValue;
                                                                                        iBestBonusValue = iBonusValue;
                                                                                    }
                                                                                }
                                                                            }
                                                                        }

                                                                        if (iValue > iBestPossibleValue)
                                                                        {
                                                                            iBestPossibleValue = iValue;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if ((pBestTile != null) && (eBestHQ != HQType.NONE))
                            {
                                break;
                            }
                        }
                    }
                }

                using (new UnityProfileScope("Player::tryFound"))
                {
                    if ((pBestTile != null) && (eBestHQ != HQType.NONE))
                    {
                        bool bValid = (getNumHQs() > 0);

                        if (gameServer().isGameOption(GameOptionType.REVEAL_MAP))
                        {
                            if ((gameServer().getFoundMoney() / infos().personality(getPersonality()).miFoundMoneyDivisor) > -(iBestFoundValue))
                            {
                                if (AI_LOGGING) Debug.Log("Found " + iBestFoundValue + " vs $" + gameServer().getFoundMoney());

                                bValid = true;
                            }
                        }
                        else
                        {
                            int iOriginalBestPossibleValue = iBestPossibleValue;

                            iBestPossibleValue *= ((int)(gameServer().getNumPlayers()) * Constants.MAX_NUM_PLAYERS);
                            iBestPossibleValue /= ((int)(gameServer().getNumPlayers()) * Constants.MAX_NUM_PLAYERS) + gameServer().getHQLevels();

                            if (gameServer().getHQLevels() >= (int)(gameServer().getNumPlayers() - 1))
                            {
                                iBestPossibleValue *= 19;
                                iBestPossibleValue /= 20;
                            }

                            if (getPersonality() != PersonalityType.NONE)
                            {
                                iBestPossibleValue *= infos().personality(getPersonality()).miFoundValueModifier;
                                iBestPossibleValue /= 100;
                            }

                            if (iBestFoundValue >= iBestPossibleValue)
                            {
                                if (AI_LOGGING) Debug.Log("Found " + iBestFoundValue + " vs " + iBestPossibleValue + " (" + iOriginalBestPossibleValue + ")" + ((pBestTile.isHQMinimum(eBestHQ)) ? "" : " (NOT MINIMUM)"));

                                bValid = true;
                            }
                        }

                        if (bValid)
                        {
                            return found(pBestTile, eBestHQ);
                        }
                    }
                }

                return null;
            }
        }

        protected virtual bool AI_checkFoundBuildingValid(bool bValid, InfoHQ loopHQInfo)
        {
            using (new UnityProfileScope("PlayerServer.AI_checkFoundBuildingValid"))
            {
                for (BuildingType eLoopBuilding = 0; eLoopBuilding < infos().buildingsNum(); eLoopBuilding++)
                {
                    if (canEverConstruct(eLoopBuilding, true, true))
                    {
                        if (infos().building(eLoopBuilding).maiResourceMining[(int)loopHQInfo.meBaseResource] > 0)
                        {
                            bValid = true;
                            break;
                        }
                        else if (gameServer().getBuildingResourceOutput(eLoopBuilding, loopHQInfo.meBaseResource) > 0)
                        {
                            if (!(gameServer().isBuildingHasInput(eLoopBuilding)))
                            {
                                bValid = true;
                                break;
                            }
                            else if (infos().building(eLoopBuilding).mbSelfInput)
                            {
                                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                {
                                    if (infos().building(eLoopBuilding).maiResourceInput[(int)eLoopResource] > 0)
                                    {
                                        if (gameServer().getResourceRateCount(eLoopResource) > 0)
                                        {
                                            bValid = true;
                                            break;
                                        }

                                        for (TerrainType eLoopTerrain = 0; eLoopTerrain < infos().terrainsNum(); eLoopTerrain++)
                                        {
                                            if (infos().terrain(eLoopTerrain).maiResourceRate[(int)eLoopResource] > 0)
                                            {
                                                if (gameServer().getTerrainCount(eLoopTerrain) > 0)
                                                {
                                                    if (gameServer().canTerrainHaveBuilding(eLoopTerrain, eLoopBuilding))
                                                    {
                                                        bValid = true;
                                                        break;
                                                    }
                                                }
                                            }
                                        }

                                        if (bValid)
                                        {
                                            break;
                                        }

                                        for (IceType eLoopIce = 0; eLoopIce < infos().icesNum(); eLoopIce++)
                                        {
                                            if (infos().ice(eLoopIce).maiAverageResourceRate[(int)eLoopResource] > 0)
                                            {
                                                if (gameServer().getIceCount(eLoopIce) > 0)
                                                {
                                                    bValid = true;
                                                    break;
                                                }
                                            }
                                        }

                                        if (bValid)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                {
                                    if (infos().building(eLoopBuilding).maiResourceInput[(int)eLoopResource] > 0)
                                    {
                                        bValid = false;

                                        for (BuildingType eInputBuilding = 0; eInputBuilding < infos().buildingsNum(); eInputBuilding++)
                                        {
                                            if (Utils.isBuildingYield(eInputBuilding, eLoopResource, gameServer()))
                                            {
                                                if (canEverConstruct(eInputBuilding, true, true))
                                                {
                                                    bValid = true;
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

                return bValid;
            }
        }

        private static List<TileClient> tempCheckFoundResourceTiles = new List<TileClient>(); //scratch buffer for AI_checkFoundResourceValid
        protected virtual bool AI_checkFoundResourceValid(bool bValid, InfoHQ loopHQInfo)
        {
            using (new UnityProfileScope("PlayerServer.AI_checkFoundResourceValid"))
            {
                PlayerType ePlayer = getPlayer();
                if (bValid)
                {
                    //cache the interesting tiles
                    tempCheckFoundResourceTiles.Clear();
                    foreach (TileClient pLoopTile in gameServer().tileClientAll())
                    {
                        if ((pLoopTile.getVisibility(ePlayer) == VisibilityType.VISIBLE) && !pLoopTile.isClaimed() && !pLoopTile.isClaimBlock())
                        {
                            tempCheckFoundResourceTiles.Add(pLoopTile);
                        }
                    }

                    for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                    {
                        if (gameServer().isResourceValid(eLoopResource) && (infos().resource(eLoopResource).maiLocationAppearanceProb[(int)(gameServer().getLocation())] > 0))
                        {
                            int iMinRate = loopHQInfo.maiAIResourceMinRate[(int)eLoopResource];
                            int iMinCount = loopHQInfo.maiAIResourceMinCount[(int)eLoopResource];
                            if ((iMinRate > 0) || (iMinCount > 0))
                            {
                                int iTotalRate = 0;
                                int iTotalCount = 0;

                                foreach (TileClient pLoopTile in tempCheckFoundResourceTiles)
                                {
                                    iTotalRate += pLoopTile.getPotentialResourceRate(eLoopResource, -80);

                                    if (pLoopTile.getPotentialResourceRate(eLoopResource) > 0)
                                    {
                                        iTotalCount++;
                                    }
                                }

                                if ((iTotalRate < iMinRate) || (iTotalCount < iMinCount))
                                {
                                    bValid = false;
                                    break;
                                }
                            }
                        }
                    }
                }

                return bValid;
            }
        }

        protected virtual bool AI_checkFoundRulesValid(bool bValid, InfoHQ loopHQInfo)
        {
            using (new UnityProfileScope("PlayerServer.AI_checkFoundRulesValid"))
            {
                if (infos().rulesSet(gameServer().getRulesSet()).mbAIFoundFavorite)
                {
                    if (getPersonality() != PersonalityType.NONE)
                    {
                        if (infos().personality(getPersonality()).maiHQModifier[loopHQInfo.miType] > 0)
                        {
                            if (!bValid)
                            {
                                bValid = true;
                            }
                        }
                        else
                        {
                            if (bValid)
                            {
                                bValid = false;
                            }
                        }
                    }
                }

                return bValid;
            }
        }

        protected virtual bool AI_returnClaimSabotage(TileServer pTile)
        {
            for (SabotageType eLoopSabotage = 0; eLoopSabotage < infos().sabotagesNum(); eLoopSabotage++)
            {
                if (infos().sabotage(eLoopSabotage).mbReturnClaim)
                {
                    if (getSabotageCount(eLoopSabotage) > 0)
                    {
                        if (canReturnClaim(pTile, true))
                        {
                            returnClaim(pTile, true);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        protected virtual void AI_doScrap(BuildingServer pBuilding)
        {
            TileServer pBuildingTile = pBuilding.tileServer();

            pBuilding.scrap();

            AI_changeForceConstruct(1);

            AI_returnClaimSabotage(pBuildingTile);

            if (AI_LOGGING) Debug.Log("**Scrapped " + TEXT(infos().building(pBuilding.getType()).meName) + " at " + pBuilding.getX() + ", " + pBuilding.getY() + " (No Patents Left)");
        }

        protected virtual bool AI_scrapBuildings()
        {
            using (new UnityProfileScope("Player::AI_scrapBuildings"))
            {
                if (gameServer().random().Next(10) != 0)
                {
                    return false;
                }

                foreach (int iBuildingID in getBuildingList())
                {
                    BuildingServer pLoopBuilding = gameServer().buildingServer(iBuildingID);

                    if (pLoopBuilding.canScrap())
                    {
                        if (!isWinEligible())
                        {
                            if ((infos().buildingClass(pLoopBuilding.getClass()).meOrderType != OrderType.NONE) &&
                                (infos().buildingClass(pLoopBuilding.getClass()).meOrderType != OrderType.LAUNCH))
                            {
                                AI_doScrap(pLoopBuilding);
                                return true;
                            }
                        }

                        if (infos().buildingClass(pLoopBuilding.getClass()).meOrderType == OrderType.PATENT)
                        {
                            if (countOrders(OrderType.PATENT) < getOrderCapacity(OrderType.PATENT))
                            {
                                if ((gameServer().countPatentsAvailable() < getOrderCapacity(OrderType.PATENT)) || !AI_wantsAnyPatent())
                                {
                                    AI_doScrap(pLoopBuilding);
                                    return true;
                                }
                            }
                        }
                    }
                }

                return false;
            }
        }

        public virtual bool AI_turnOffBuilding(BuildingType eBuilding, TileServer pTile, int iNetThreshold, int iDebtMultiplier, bool bNew)
        {
            if (infos().buildingClass(infos().building(eBuilding).meClass).meOrderType != OrderType.NONE)
            {
                return false;
            }

            using (new UnityProfileScope("Player::AI_turnOffBuilding"))
            {
                int iConnections = ((pTile != null) ? pTile.countConnections(eBuilding, getPlayer(), bNew, false) : 0);

                if (bNew)
                {
                    if ((getClaims() > 1) || ((getRealConstructionCount(eBuilding) + getRealBuildingCount(eBuilding)) > 0))
                    {
                        iConnections++;
                    }
                }

                return (gameServer().calculateRevenue(eBuilding, pTile, getPlayer(), iConnections, false, bNew, iDebtMultiplier, iDebtMultiplier, 100, false) < iNetThreshold);
            }
        }

        protected virtual void AI_turnOffBuildings()
        {
            using (new UnityProfileScope("Player::AI_turnOffBuildings"))
            {
                if (gameServer().random().Next(5) != 0)
                {
                    return;
                }

                int iDebtMulitplier = ((getHQLevelInt() == 1) ? 0 : 50);

                foreach (int iBuildingID in getBuildingList())
                {
                    BuildingServer pLoopBuilding = gameServer().buildingServer(iBuildingID);
                    TileServer pTile = pLoopBuilding.tileServer();

                    bool bPowerOnly = Utils.isBuildingYield(pLoopBuilding.getType(), infos().Globals.ENERGY_RESOURCE, gameServer());

                    if (bPowerOnly)
                    {
                        for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                        {
                            if (eLoopResource != infos().Globals.ENERGY_RESOURCE)
                            {
                                if (Utils.isBuildingYield(pLoopBuilding.getType(), eLoopResource, gameServer()))
                                {
                                    bPowerOnly = false;
                                    break;
                                }
                            }
                        }
                    }

                    if (pLoopBuilding.isOff() != AI_turnOffBuilding(pLoopBuilding.getType(), pTile, ((bPowerOnly) ? 0 : -5), ((bPowerOnly) ? 100 : iDebtMulitplier), false))
                    {
                        pLoopBuilding.toggleOff();

                        if (AI_LOGGING && pLoopBuilding.isOff()) Debug.Log("Turn Off " + TEXT(infos().building(pLoopBuilding.getType()).meName) + " at " + pTile.getX() + ", " + pTile.getY() + " [" + getRealName() + "]");
                    }
                }
            }
        }

        protected virtual bool AI_tilesAvailable()
        {
            if (getClaims() > 0)
            {
                return true;
            }

            foreach (int iTileID in getTileList())
            {
                TileServer pLoopTile = gameServer().tileServer(iTileID);

                if (pLoopTile.isOwnerReal())
                {
                    if (pLoopTile.isEmpty())
                    {
                        if (pLoopTile.getTotalUnitMissionCount(MissionType.CONSTRUCT) == 0)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public virtual int AI_connectionTileValue(TileServer pTile, BuildingType eBuilding)
        {
            using (new UnityProfileScope("Player::AI_connectionTileValue"))
            {
                int iHighestCount = 0;
                int iFirstCount = 0;
                int iCount = 0;

                for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                {
                    TileServer pAdjacentTile = gameServer().tileServerAdjacent(pTile, eLoopDirection);

                    bool bValid = false;

                    if (pAdjacentTile != null)
                    {
                        if (pAdjacentTile.getHeight() == pTile.getHeight())
                        {
                            if (gameServer().playerServer(getPlayer()).canConstructTile(pAdjacentTile, eBuilding, false))
                            {
                                bValid = true;
                            }
                        }
                    }

                    if (bValid)
                    {
                        if (iCount == (int)eLoopDirection)
                        {
                            iFirstCount++;
                        }

                        iCount++;
                    }
                    else
                    {
                        iCount = 0;
                    }

                    iHighestCount = Math.Max(iHighestCount, iCount);
                }

                if ((iCount > 0) && (iCount < (int)DirectionType.NUM_TYPES))
                {
                    iHighestCount = Math.Max(iHighestCount, (iCount + iFirstCount));
                }

                return (((int)(DirectionType.NUM_TYPES) - iCount) * 10) + ((3 - Math.Min(3, iHighestCount)) * 50);
            }
        }

        protected virtual int AI_constructTileValue(TileServer pTile, BuildingType eBuilding, bool bHQConnection)
        {
            using (new UnityProfileScope("Player::AI_constructTileValue"))
            {
                int iValue = 0;

                iValue += AI_connectionTileValue(pTile, eBuilding);

                iValue += (pTile.countOtherBuildings(eBuilding, getPlayer()) * 40);

                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    if (infos().resource(eLoopResource).maiLocationAppearanceProb[(int)(gameServer().getLocation())] > 0)
                    {
                        iValue += ((infos().resourceLevelsNum() - pTile.getResourceLevel(eLoopResource, false)) * 30);
                    }
                }

                if (pTile.getIce() == IceType.NONE)
                {
                    iValue += 30;
                }

                if (!bHQConnection)
                {
                    HQServer pHQ = findClosestHQServer(pTile);

                    if (pHQ != null)
                    {
                        iValue += (Utils.stepDistance(pTile.getX(), pTile.getY(), pHQ.getX(), pHQ.getY()) * ((isTeleportation()) ? 1 : ((infos().building(eBuilding).mbSelfInput) ? 20 : 10)));
                    }
                }

                return iValue;
            }
        }

        protected virtual int AI_resourceProductionMax(BuildingType eBuilding, bool bHQConnection)
        {
            int iMax = 3;

            if (infos().building(eBuilding).mbSelfInput)
            {
                iMax++;
            }

            if (bHQConnection)
            {
                iMax++;
            }

            if (gameServer().isTeamGame() && !isSubsidiary())
            {
                iMax += ((countTeammatesAlive() - 1) * 2);
            }

            return iMax;
        }

        protected virtual void AI_initTileArray()
        {
            List<TileClient> allTiles = gameServer().mapServer().getTileArray();
            maiNearbyTiles = new HashSet<int>((allTiles.Where(pLoopTile => findClosestHQClientDistance(pLoopTile) < 6)).Select(x => x.getID()));
            maiNearbyIceTiles = new HashSet<int>((gameServer().getIceTiles().Where(iLoopTile => findClosestHQClientDistance(gameServer().tileServer(iLoopTile)) < 6)));
            if (maiNearbyIceTiles.Count < 10)
                maiNearbyIceTiles.Clear();
        }

        protected virtual void AI_initFundResourcesValueCache()
        {
            maiResourceValue = new List<int>();
            maiResourceValueSaveUpgrade = new List<int>();

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                maiResourceValue.Add(-1);
                maiResourceValueSaveUpgrade.Add(-1);
            }

            maiStockValueSorted = new List<int>();
            maiStockValueSortedIndex = new List<PlayerType>();
            for (PlayerType ePlayer = 0; ePlayer < gameServer().getNumPlayers(); ePlayer++)
            {
                maiStockValueSorted.Add(-1);
                maiStockValueSortedIndex.Add(PlayerType.NONE);
            }
        }

        public virtual void addNearbyTileToList(TileClient pTile)
        {
            maiNearbyTiles.Add(pTile.getID());
        }

        protected virtual bool AI_bestBuildingResourceConstruct(BuildingType eBuilding, ResourceType eResource, int iMinPriceMultiplier, bool bTestFunding, ref TileServer pBestTile, ref int iBestValue, List<int> aiExtraResources, bool bTestClaims = true)
        {
            using (new UnityProfileScope("Player::AI_bestBuildingResourceConstruct"))
            {
                if (!(Utils.isBuildingYield(eBuilding, eResource, gameServer())))
                {
                    return false;
                }

                if (!canConstructPlayer(eBuilding, false))
                {
                    return false;
                }

                if (AI_turnOffBuilding(eBuilding, null, 0, 0, true))
                {
                    return false;
                }

                if (bTestFunding && !AI_constructFund(null, eBuilding, iMinPriceMultiplier, true, aiExtraResources))
                {
                    return false;
                }

                int iResourceProduction = AI_getResourceProductionCountTeam(eResource);

                bool bResourceNotMaxed = (iResourceProduction < AI_resourceProductionMax(eBuilding, false));
                bool bResourceNotMaxedHQ = (iResourceProduction < AI_resourceProductionMax(eBuilding, true));

                if (!bResourceNotMaxed && !bResourceNotMaxedHQ)
                {
                    return false;
                }

                bool bFound = false;

                using (new UnityProfileScope("Player::AI_bestBuildingResourceConstruct - Mining"))
                {
                    int iMining = infos().building(eBuilding).maiResourceMining[(int)eResource];
                    TerrainType eMiningTerrain = infos().building(eBuilding).meTerrainRate;
                    bool bCheckIce = infos().building(eBuilding).mbIce && gameServer().getBuildingResourceOutput(eBuilding, eResource) > 0;
                    if (iMining > 0 || (eMiningTerrain != TerrainType.NONE && infos().terrain(eMiningTerrain).maiResourceRate[(int)eResource] > 0) || bCheckIce)
                    {
                        int iBestTileValue = int.MaxValue;

                        HashSet<int> aResourceTiles = new HashSet<int>(gameServer().getResourceTileList(eResource));

                        if (isAdjacentMining())
                            aResourceTiles.UnionWith(gameServer().getResourceAdjacentTileList(eResource));

                        if (bCheckIce)
                        {
                            if (maiNearbyIceTiles.Count > 0)
                                aResourceTiles.UnionWith(maiNearbyIceTiles);
                            else
                                aResourceTiles.UnionWith(gameServer().getIceTiles());
                        }

                        aResourceTiles.UnionWith(gameServer().getCaveTiles());

                        Profiler.BeginSample("Player::AI_bestBuildingResourceConstruct - Mining 2");
                        foreach (int tileID in aResourceTiles)
                        {
                            TileServer pLoopTile = gameServer().tileServer(tileID);
                            bool bHQConnection = pLoopTile.isPotentialHQConnection(getPlayer());

                            if ((bHQConnection) ? bResourceNotMaxedHQ : bResourceNotMaxed)
                            {

                                if (canConstructTile(pLoopTile, eBuilding, bTestClaims))
                                {
                                    int iPotentialConnections = pLoopTile.countConnections(eBuilding, getPlayer(), true, false);

                                    int iValue = gameServer().resourceMiningTile(eBuilding, eResource, pLoopTile, getPlayer(), iMining, iPotentialConnections, true);
                                    if (iValue > 0)
                                    {
                                        Profiler.BeginSample("Player::AI_bestBuildingResourceConstruct - Mining 3");

                                        for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                        {
                                            if (eLoopResource != eResource)
                                            {
                                                iValue += gameServer().resourceMiningTile(eBuilding, eLoopResource, pLoopTile, getPlayer(), infos().building(eBuilding).maiResourceMining[(int)eLoopResource], iPotentialConnections, true);
                                            }
                                        }

                                        int iReverseConnections = pLoopTile.countConnections(eBuilding, getPlayer(), true, true);
                                        if (iReverseConnections > 0)
                                        {
                                            iValue *= (2 + iReverseConnections);
                                            iValue /= (2);
                                        }

                                        if (infos().terrain(pLoopTile.getTerrain()).mbAdjacentResource)
                                        {
                                            if (!(infos().HQ(getHQ()).mbNoDeplete))
                                            {
                                                if (infos().resource(eResource).maiLocationDiminishThreshold[(int)(gameServer().getLocation())] > 0)
                                                {
                                                    TileServer pMinedTile = (TileServer)(pLoopTile.getResourceAdjacentTile(eResource));

                                                    if ((pMinedTile != null) && (pMinedTile != pLoopTile))
                                                    {
                                                        if (pMinedTile.getTeam() == getTeam())
                                                        {
                                                            iValue /= 2;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        if (bHQConnection)
                                        {
                                            iValue *= 2;
                                        }
                                        else
                                        {
                                            int iDist = findClosestHQClientDistance(pLoopTile);

                                            if (iDist < 20)
                                            {
                                                iValue *= (40 - iDist);
                                                iValue /= (20);
                                            }

                                            if (iDist > 8)
                                            {
                                                if (getHQLevelInt() == 1)
                                                {
                                                    if (eResource == infos().HQ(getHQ()).meBaseResource)
                                                    {
                                                        iValue *= (8);
                                                        iValue /= (iDist);
                                                    }
                                                }
                                            }
                                        }

                                        if (isBorehole())
                                        {
                                            if (infos().building(eBuilding).miPowerConsumption > 0)
                                            {
                                                if (pLoopTile.onOrAdjacentToGeothermal())
                                                {
                                                    iValue *= 7;
                                                    iValue /= 6;
                                                }
                                            }
                                        }

                                        if (pLoopTile.getOwner() == getPlayer())
                                        {
                                            iValue *= 2;
                                        }

                                        if (iValue >= iBestValue)
                                        {
                                            int iTileValue = AI_constructTileValue(pLoopTile, eBuilding, bHQConnection);

                                            if (infos().terrain(pLoopTile.getTerrain()).mbAdjacentResource)
                                            {
                                                if (infos().resource(eResource).maiLocationDiminishThreshold[(int)(gameServer().getLocation())] > 0)
                                                {
                                                    TileServer pMinedTile = (TileServer)(pLoopTile.getResourceAdjacentTile(eResource));

                                                    if ((pMinedTile != null) && (pMinedTile != pLoopTile))
                                                    {
                                                        if (pMinedTile.isClaimed())
                                                        {
                                                            if (!(infos().HQ(pMinedTile.realOwnerServer().getHQ()).mbNoDeplete))
                                                                iTileValue += 10;
                                                        }
                                                    }
                                                }
                                            }

                                            iTileValue += gameServer().random().Next(10);

                                            if ((iValue > iBestValue) || (iTileValue < iBestTileValue))
                                            {
                                                pBestTile = pLoopTile;
                                                iBestValue = iValue;

                                                iBestTileValue = iTileValue;

                                                bFound = true;
                                            }
                                        }
                                        Profiler.EndSample();
                                    }
                                }
                            }
                        }
                        Profiler.EndSample();
                    }
                }

                if (gameServer().isBuildingHasInput(eBuilding))
                {
                    using (new UnityProfileScope("Player::AI_bestBuildingResourceConstruct - Output w/ Input"))
                    {
                        if (gameServer().getBuildingResourceOutput(eBuilding, eResource) > 0)
                        {
                            bool bSelfInput = infos().building(eBuilding).mbSelfInput;
                            int iBestTileValue = int.MaxValue;

                            HashSet<int> aResourceTiles = new HashSet<int>(gameServer().getResourceTileList(eResource));

                            if (isAdjacentMining())
                                aResourceTiles.UnionWith(gameServer().getResourceAdjacentTileList(eResource));

                            if (gameServer().getLocation() != LocationType.EUROPA)
                            {
                                for (IceType eLoopIce = 0; eLoopIce < infos().icesNum(); eLoopIce++)
                                {
                                    if (gameServer().getIceCount(eLoopIce) > 0 && infos().ice(eLoopIce).maiAverageResourceRate[(int)eResource] > 0)
                                    {
                                        if (maiNearbyIceTiles.Count > 0)
                                            aResourceTiles.UnionWith(maiNearbyIceTiles);
                                        else
                                            aResourceTiles.UnionWith(gameServer().getIceTiles());
                                        break;
                                    }
                                }
                            }

                            aResourceTiles.UnionWith(gameServer().getCaveTiles());
                            aResourceTiles.UnionWith(maiNearbyTiles);

                            Profiler.BeginSample("Player::AI_bestBuildingResourceConstruct - Output w/ Input 2");
                            foreach (int tileID in aResourceTiles)
                            {
                                TileServer pLoopTile = gameServer().tileServer(tileID);

                                bool bHQConnection = pLoopTile.isPotentialHQConnection(getPlayer());

                                if ((bHQConnection) ? bResourceNotMaxedHQ : bResourceNotMaxed)
                                {

                                    if (canConstructTile(pLoopTile, eBuilding, bTestClaims))
                                    {
                                        bool bTileValid = true;
                                        int iTileValue = 0;
                                        int iInputFound = 0;

                                        if (bSelfInput)
                                        {
                                            int iInputMissing = 0;

                                            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                            {
                                                if ((infos().building(eBuilding).maiResourceInput[(int)eLoopResource] > 0) && !isBuildingFreeResource(eLoopResource))
                                                {
                                                    if (pLoopTile.supplySelfInputPlayer(eLoopResource, getPlayer()))
                                                    {
                                                        //iTileValue += ((gameServer().getHighestResourceLevel() - pLoopTile.getResourceLevel(eLoopResource, isAdjacentMining())) * 20); //too small a modifier to be worth calculating
                                                        iInputFound++;
                                                    }
                                                    else
                                                    {
                                                        iTileValue += 2000;

                                                        iInputMissing++;
                                                    }
                                                }
                                            }

                                            if (iInputMissing > 0)
                                            {
                                                if (iInputFound == 0)
                                                {
                                                    if (!bHQConnection && !isTeleportation())
                                                    {
                                                        bTileValid = false;
                                                    }
                                                }
                                                else
                                                {
                                                        if (findClosestHQClientDistance(pLoopTile) > (iInputMissing * 10))
                                                        {
                                                            bTileValid = false;
                                                        }
                                                }
                                            }
                                        }

                                        if (bTileValid)
                                        {
                                            int iValue = gameServer().resourceOutput(eBuilding, eResource, pLoopTile, getPlayer(), pLoopTile.countConnections(eBuilding, getPlayer(), true, false), true);
                                            if (iValue > 0)
                                            {
                                                Profiler.BeginSample("Player::AI_bestBuildingResourceConstruct - Output w/ Input 3");

                                                iValue *= (1 + iInputFound);

                                                int iReverseConnections = pLoopTile.countConnections(eBuilding, getPlayer(), true, true);
                                                if (iReverseConnections > 0)
                                                {
                                                    iValue *= (2 + iReverseConnections);
                                                    iValue /= (2);
                                                }

                                                if (pLoopTile.getOwner() == getPlayer())
                                                {
                                                    iValue *= 2;
                                                }

                                                if (isBorehole())
                                                {
                                                    if (infos().building(eBuilding).miPowerConsumption > 0)
                                                    {
                                                        if (pLoopTile.onOrAdjacentToGeothermal())
                                                        {
                                                            iValue *= 7;
                                                            iValue /= 6;
                                                        }
                                                    }
                                                }

                                                if (bHQConnection)
                                                {
                                                    if (pLoopTile.adjacentToHQ(getPlayer()))
                                                    {
                                                        iValue *= 5;
                                                        iValue /= 4;
                                                    }
                                                }

                                                if (iValue >= iBestValue)
                                                {
                                                    iTileValue += AI_constructTileValue(pLoopTile, eBuilding, bHQConnection);

                                                    if (bSelfInput)
                                                    {
                                                        int iValidCount = 0;

                                                        for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                                                        {
                                                            TileServer pAdjacentTile = gameServer().tileServerAdjacent(pLoopTile, eLoopDirection);

                                                            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                                            {
                                                                if ((infos().building(eBuilding).maiResourceInput[(int)eLoopResource] > 0) && !isBuildingFreeResource(eLoopResource))
                                                                {
                                                                    if ((pAdjacentTile != null) &&
                                                                        (pAdjacentTile.supplySelfInputPlayer(eLoopResource, getPlayer())) &&
                                                                        (canConstructTile(pAdjacentTile, eBuilding, false)))
                                                                    {
                                                                        iValidCount++;
                                                                    }
                                                                }
                                                            }
                                                        }

                                                        if (bHQConnection)
                                                        {
                                                            if (iValidCount < 2)
                                                            {
                                                                iTileValue += ((2 - iValidCount) * 100);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            iTileValue += (((int)(DirectionType.NUM_TYPES) - iValidCount) * 100);
                                                        }
                                                    }

                                                    if (eResource == infos().Globals.ENERGY_RESOURCE)
                                                    {
                                                        if (!bHQConnection)
                                                        {
                                                            iTileValue += 200;
                                                        }
                                                    }

                                                    iTileValue += gameServer().random().Next(10);

                                                    if ((iValue > iBestValue) || (iTileValue < iBestTileValue))
                                                    {
                                                        pBestTile = pLoopTile;
                                                        iBestValue = iValue;

                                                        iBestTileValue = iTileValue;

                                                        bFound = true;
                                                    }
                                                }
                                                Profiler.EndSample();
                                            }
                                        }
                                    }
                                }
                            }
                            Profiler.EndSample();
                        }
                    }
                }
                else
                {
                    using (new UnityProfileScope("Player::AI_bestBuildingResourceConstruct - Output w/o Input"))
                    {
                        if (gameServer().getBuildingResourceOutput(eBuilding, eResource) > 0)
                        {
                            int iBestTileValue = int.MaxValue;

                            HashSet<int> aRelevantTiles = new HashSet<int>(maiNearbyTiles);
                            if(eResource == infos().Globals.ENERGY_RESOURCE)
                            {
                                InfoBuilding building = infos().building(eBuilding);
                                if (building.mbGeothermal)
                                    aRelevantTiles.UnionWith(gameServer().getGeothermalList());
                                if (building.maiOutputWindChange.Any(x => x != 0))
                                    aRelevantTiles.UnionWith(gameServer().getWindTileList());
                            }

                            Profiler.BeginSample("Player::AI_bestBuildingResourceConstruct - Output w/o Input 2");
                            foreach (int tileID in aRelevantTiles)
                            {
                                TileServer pLoopTile = gameServer().tileServer(tileID);

                                bool bHQConnection = pLoopTile.isPotentialHQConnection(getPlayer());

                                if ((bHQConnection) ? bResourceNotMaxedHQ : bResourceNotMaxed)
                                {
                                    if (canConstructTile(pLoopTile, eBuilding, bTestClaims))
                                    {
                                        int iDist = findClosestHQClientDistance(pLoopTile);

                                        if ((iDist < 20) || !infos().resource(eResource).mbTrade)
                                        {
                                            int iValue = gameServer().resourceOutput(eBuilding, eResource, pLoopTile, getPlayer(), pLoopTile.countConnections(eBuilding, getPlayer(), true, false), true);
                                            if (iValue > 0)
                                            {
                                                Profiler.BeginSample("Player::AI_bestBuildingResourceConstruct - Output w/o Input 3");
                                                int iReverseConnections = pLoopTile.countConnections(eBuilding, getPlayer(), true, true);
                                                if (iReverseConnections > 0)
                                                {
                                                    iValue *= (2 + iReverseConnections);
                                                    iValue /= (2);
                                                }

                                                if (infos().resource(eResource).mbTrade)
                                                {
                                                    if (bHQConnection)
                                                    {
                                                        if (pLoopTile.adjacentToHQ(getPlayer()))
                                                        {
                                                            iValue *= 9;
                                                            iValue /= 4;
                                                        }
                                                        else
                                                        {
                                                            iValue *= 2;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (iDist < 10)
                                                        {
                                                            iValue *= 3;
                                                            iValue /= 2;
                                                        }
                                                        else if (iDist < 20)
                                                        {
                                                            iValue *= 4;
                                                            iValue /= 3;
                                                        }
                                                        else if (iDist < 30)
                                                        {
                                                            iValue *= 5;
                                                            iValue /= 4;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (bHQConnection)
                                                    {
                                                        if (pLoopTile.adjacentToHQ(getPlayer()))
                                                        {
                                                            iValue *= 7;
                                                            iValue /= 4;
                                                        }
                                                        else
                                                        {
                                                            iValue *= 3;
                                                            iValue /= 2;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (iDist < 10)
                                                        {
                                                            iValue *= (30 - iDist);
                                                            iValue /= (20);
                                                        }
                                                    }
                                                }

                                                if (pLoopTile.getOwner() == getPlayer())
                                                {
                                                    iValue *= 2;
                                                }

                                                if (isBorehole())
                                                {
                                                    if (infos().building(eBuilding).miPowerConsumption > 0)
                                                    {
                                                        if (pLoopTile.onOrAdjacentToGeothermal())
                                                        {
                                                            iValue *= 7;
                                                            iValue /= 6;
                                                        }
                                                    }
                                                }

                                                if (iValue >= iBestValue)
                                                {
                                                    int iTileValue = AI_constructTileValue(pLoopTile, eBuilding, bHQConnection);
                                                    //
                                                    iTileValue += gameServer().random().Next(10);

                                                    if ((iValue > iBestValue) || (iTileValue < iBestTileValue))
                                                    {
                                                        pBestTile = pLoopTile;
                                                        iBestValue = iValue;

                                                        iBestTileValue = iTileValue;

                                                        bFound = true;
                                                    }
                                                }
                                                Profiler.EndSample();
                                            }
                                        }
                                    }
                                }
                            }
                            Profiler.EndSample();
                        }
                    }
                }

                return bFound;
            }
        }

        protected virtual void AI_bestResourceConstruct(ResourceType eResource, int iMinPriceMultiplier, ref TileServer pBestTile, ref BuildingType eBestBuilding, ref int iBestValue, List<int> aiExtraResources, bool bTestClaims = true)
        {
            for (BuildingType eLoopBuilding = 0; eLoopBuilding < infos().buildingsNum(); eLoopBuilding++)
            {
                if (AI_bestBuildingResourceConstruct(eLoopBuilding, eResource, iMinPriceMultiplier, true, ref pBestTile, ref iBestValue, aiExtraResources, bTestClaims))
                {
                    eBestBuilding = eLoopBuilding;
                }
            }
        }

        protected virtual TileServer AI_bestEntertainmentConstruct(BuildingType eBuilding, int iMinPriceMultiplier)
        {
            TileServer pBestTile = null;

            if (canConstructPlayer(eBuilding, false))
            {
                if (AI_constructFund(null, eBuilding, iMinPriceMultiplier, true, null))
                {
                    int iBestValue = 0;
                    int iBestTileValue = int.MaxValue;

                    foreach (TileServer pLoopTile in gameServer().tileServerAll())
                    {
                        if (canConstructTile(pLoopTile, eBuilding, true))
                        {
                            int iValue = gameServer().entertainmentProfit(eBuilding, pLoopTile, getPlayer(), true, true);
                            if (iValue > iBestValue)
                            {
                                int iTileValue = 0;

                                iTileValue += (pLoopTile.countEmptyUsableTiles() * 100);

                                {
                                    HQServer pHQ = findClosestHQServer(pLoopTile);

                                    if (pHQ != null)
                                    {
                                        iTileValue += (Utils.stepDistance(pLoopTile.getX(), pLoopTile.getY(), pHQ.getX(), pHQ.getY()) * 10);
                                    }
                                }

                                iTileValue += gameServer().random().Next(10);

                                if ((iValue > iBestValue) || ((iValue == iBestValue) && (iTileValue < iBestTileValue)))
                                {
                                    pBestTile = pLoopTile;
                                    iBestValue = iValue;
                                    iBestTileValue = iTileValue;
                                }
                            }
                        }
                    }
                }
            }

            return pBestTile;
        }

        protected virtual TileServer AI_bestOrderConstruct(BuildingType eBuilding, int iMinPriceMultiplier)
        {
            TileServer pBestTile = null;

            if (canConstructPlayer(eBuilding, false))
            {
                if (AI_constructFund(null, eBuilding, iMinPriceMultiplier, true, null))
                {
                    OrderType eOrder = infos().buildingClass(infos().building(eBuilding).meClass).meOrderType;

                    if (pBestTile == null)
                    {
                        int iBestValue = 0;
                        int iBestAdjacenyBonus = 0;

                        foreach (TileServer pLoopTile in gameServer().tileServerAll())
                        {
                            if (canConstructTile(pLoopTile, eBuilding, true))
                            {
                                bool bValid = true;

                                if (bValid)
                                {
                                    for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                                    {
                                        TileServer pAdjacentTile = gameServer().tileServerAdjacent(pLoopTile, eLoopDirection);

                                        if (pAdjacentTile != null)
                                        {
                                            if (pAdjacentTile.isBuilding())
                                            {
                                                if (pAdjacentTile.buildingServer().getType() == eBuilding)
                                                {
                                                    bValid = false;
                                                    break;
                                                }
                                            }
                                            else if (pAdjacentTile.isConstruction())
                                            {
                                                if (pAdjacentTile.constructionServer().getType() == eBuilding)
                                                {
                                                    bValid = false;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }

                                int iAdjacencyBonus = 0;

                                if (bValid)
                                {
                                    for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                                    {
                                        TileServer pAdjacentTile = gameServer().tileServerAdjacent(pLoopTile, eLoopDirection);

                                        if (pAdjacentTile != null)
                                        {
                                            if (pAdjacentTile.isModule())
                                            {
                                                iAdjacencyBonus += gameServer().getModuleOrderModifier(pAdjacentTile.getModule(), eOrder);
                                            }
                                        }
                                    }

                                    if (iAdjacencyBonus == 0)
                                    {
                                        if (!(pLoopTile.isPotentialHQConnection(getPlayer())))
                                        {
                                            bValid = false;
                                        }
                                    }
                                }

                                if (bValid)
                                {
                                    int iValue = 1;

                                    iValue += (iAdjacencyBonus * 10);

                                    iValue += gameServer().random().Next(100);

                                    iValue *= (10);
                                    iValue /= (10 + pLoopTile.countOtherBuildings(BuildingType.NONE, PlayerType.NONE));

                                    iValue *= 100;

                                    for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                    {
                                        int iRate = ((pLoopTile.getPotentialResourceRate(eLoopResource) * 10) / Constants.RESOURCE_MULTIPLIER);
                                        if (iRate > 0)
                                        {
                                            iValue /= iRate;
                                        }
                                    }

                                    if (iValue > iBestValue)
                                    {
                                        if ((iAdjacencyBonus == 0) || (iAdjacencyBonus >= iBestAdjacenyBonus))
                                        {
                                            pBestTile = pLoopTile;
                                            iBestValue = iValue;
                                            iBestAdjacenyBonus = iAdjacencyBonus;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if ((pBestTile == null) &&
                        ((getRealOrderBuildingCount() < (getHQLevelInt() - 3)) || (eOrder == OrderType.LAUNCH)))
                    {
                        int iBestTileValue = int.MaxValue;

                        foreach (TileServer pLoopTile in gameServer().tileServerAll())
                        {
                            if (pLoopTile.getOwner() == getPlayer())
                            {
                                BuildingServer pBuilding = pLoopTile.buildingServer();

                                if (pBuilding != null)
                                {
                                    if (pBuilding.canScrap())
                                    {
                                        if (pBuilding.getType() != eBuilding)
                                        {
                                            if (infos().buildingClass(pBuilding.getClass()).meOrderType == OrderType.NONE)
                                            {
                                                bool bValid = true;

                                                for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                                                {
                                                    TileServer pAdjacentTile = gameServer().tileServerAdjacent(pLoopTile, eLoopDirection);

                                                    if (pAdjacentTile != null)
                                                    {
                                                        if (pAdjacentTile.isBuilding())
                                                        {
                                                            if (pAdjacentTile.buildingServer().getType() == eBuilding)
                                                            {
                                                                bValid = false;
                                                                break;
                                                            }
                                                        }
                                                        else if (pAdjacentTile.isConstruction())
                                                        {
                                                            if (pAdjacentTile.constructionServer().getType() == eBuilding)
                                                            {
                                                                bValid = false;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }

                                                if (bValid)
                                                {
                                                    if (gameServer().canTileHaveBuilding(pLoopTile, eBuilding, getPlayer()))
                                                    {
                                                        int iTileValue = AI_buildingValueTotal(pBuilding, true);

                                                        if ((iTileValue < infos().personality(getPersonality()).miReplaceForOrderValue) || (eOrder == OrderType.LAUNCH))
                                                        {
                                                            if (iTileValue < iBestTileValue)
                                                            {
                                                                pBestTile = pLoopTile;
                                                                iBestTileValue = iTileValue;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return pBestTile;
        }

        protected virtual TileServer AI_doConstructResource(ResourceType eResource, int iMinPriceMultiplier, bool bIgnoreInput, bool bTest, ref BuildingType eBestBuildingOut, ref BuildingType eBestInputBuildingOut)
        {
            using (new UnityProfileScope("Player::AI_doConstructResource"))
            {
                if (eResource == ResourceType.NONE)
                {
                    return null;
                }

                if (!isCanProduceResource(eResource))
                {
                    return null;
                }

                BuildingType eBestBuilding = BuildingType.NONE;
                TileServer pBestTile = null;

                using (new UnityProfileScope("Player::AI_doConstructResource - Find Best Building"))
                {
                    TileServer pResourceTile = null;
                    BuildingType eResourceBuilding = BuildingType.NONE;
                    int iResourceValue = 0;

                    AI_bestResourceConstruct(eResource, iMinPriceMultiplier, ref pResourceTile, ref eResourceBuilding, ref iResourceValue, null);

                    if (iResourceValue > 0)
                    {
                        eBestBuilding = eResourceBuilding;
                        pBestTile = pResourceTile;
                    }
                }

                if ((eBestBuilding != BuildingType.NONE) && (pBestTile != null))
                {
                    BuildingType eBestInputBuilding = BuildingType.NONE;
                    TileServer pBestTileInput = null;

                    if (!bIgnoreInput)
                    {
                        int iInputCount = 0;

                        {
                            int iConnections = pBestTile.countConnections(eBestBuilding, getPlayer(), true, false);

                            int iTotalOutput = 0;
                            int iTotalInput = 0;

                            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                            {
                                iTotalOutput += (gameServer().resourceOutput(eBestBuilding, eLoopResource, pBestTile, getPlayer(), iConnections, true) * gameServer().marketServer().getPrice(eLoopResource));

                                int iInput = gameServer().resourceInput(eBestBuilding, eLoopResource, pBestTile, getPlayer(), true);
                                if (iInput > 0)
                                {
                                    if (!isBuildingIgnoresInput(eBestBuilding, eLoopResource, pBestTile))
                                    {
                                        if (AI_getWholeResourceRateAverage(eLoopResource) <= 0)
                                        {
                                            iTotalInput += (iInput * gameServer().marketServer().getPrice(eLoopResource));

                                            iInputCount++;
                                        }
                                    }
                                }
                            }

                            if ((iTotalInput * 2) > iTotalOutput)
                            {
                                if (iInputCount > 0)
                                {
                                    if (getClaims() <= 1)
                                    {
                                        return null;
                                    }
                                }

                                if (iInputCount > 1)
                                {
                                    if (getHQLevelInt() <= 1)
                                    {
                                        return null;
                                    }
                                }
                            }
                        }

                        if ((iInputCount > 0) && (getClaims() > 1))
                        {
                            int iBestValue = 0;

                            using (new UnityProfileScope("Player::AI_doConstructResource - Find Best Input Building"))
                            {
                                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                {
                                    if ((eLoopResource != eResource) &&
                                        (gameServer().isResourceInputPlayer(eResource, eLoopResource, getPlayer()) &&
                                        !(isBuildingIgnoresInput(eBestBuilding, eLoopResource, pBestTile)) &&
                                        (AI_getWholeResourceRateAverage(eLoopResource) <= 0) &&
                                        (AI_getResourceProductionCountTeam(eLoopResource) <= AI_getResourceProductionCountTeam(eResource))))
                                    {
                                        if (AI_getResourceConstructionCount(eLoopResource) == 0)
                                        {
                                            TileServer pResourceTile = null;
                                            BuildingType eResourceBuilding = BuildingType.NONE;
                                            int iResourceValue = 0;

                                            AI_bestResourceConstruct(eLoopResource, iMinPriceMultiplier, ref pResourceTile, ref eResourceBuilding, ref iResourceValue, getBuildingResourceCost(eBestBuilding));

                                            if (iResourceValue > 0)
                                            {
                                                iResourceValue *= gameServer().marketServer().getWholePrice(eLoopResource);

                                                if (iResourceValue > iBestValue)
                                                {
                                                    pBestTileInput = pResourceTile;
                                                    eBestInputBuilding = eResourceBuilding;
                                                    iBestValue = iResourceValue;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (bTest)
                    {
                        eBestBuildingOut = eBestBuilding;
                        eBestInputBuildingOut = eBestInputBuilding;

                        return pBestTile;
                    }

                    if ((eBestInputBuilding != BuildingType.NONE) && (pBestTileInput != null))
                    {
                        if (AI_constructFund(pBestTileInput, eBestInputBuilding, iMinPriceMultiplier, false, getBuildingResourceCost(eBestBuilding)))
                        {
                            if (AI_LOGGING) Debug.Log("->Construct INPUT " + TEXT(infos().building(eBestInputBuilding).meName) + " at " + pBestTileInput.getX() + ", " + pBestTileInput.getY() + " [" + getRealName() + "]");

                            {
                                pBestTile = null;
                                eBestBuilding = BuildingType.NONE;
                                int iResourceValue = 0;

                                AI_bestResourceConstruct(eResource, iMinPriceMultiplier, ref pBestTile, ref eBestBuilding, ref iResourceValue, null);

                                if ((eBestBuilding == BuildingType.NONE) || (pBestTile == null))
                                {
                                    if (AI_LOGGING) Debug.Log("### AI_bestResourceConstruct() failed after building " + TEXT(infos().building(eBestInputBuilding).meName) + " at " + pBestTileInput.getX() + ", " + pBestTileInput.getY() + " [" + getRealName() + "]");

                                    return null;
                                }
                            }
                        }
                    }

                    if (AI_constructFund(pBestTile, eBestBuilding, iMinPriceMultiplier, false, null))
                    {
                        if (AI_LOGGING) Debug.Log("Construct " + TEXT(infos().building(eBestBuilding).meName) + " at " + pBestTile.getX() + ", " + pBestTile.getY() + " [" + getRealName() + "]");

                        eBestBuildingOut = eBestBuilding;
                        eBestInputBuildingOut = eBestInputBuilding;

                        return pBestTile;
                    }
                }

                return null;
            }
        }

        protected virtual bool AI_doConstructPowerResource(int iMinPriceMultiplier)
        {
            using (new UnityProfileScope("Player::AI_doConstructPowerResource"))
            {
                ResourceType ePowerResource = getEnergyResource();

                if (AI_getResourceProductionCount(ePowerResource) >= (getHQLevelInt() - 1))
                {
                    return false;
                }

                if (AI_getResourceConstructionCount(ePowerResource) == 0)
                {
                    int iTheshold = infos().personality(getPersonality()).miConstructPowerThreshold;

                    if (gameServer().isCampaignSevenSols())
                    {
                        iTheshold += 20;
                    }

                    if (((AI_getResourceRateAverage(ePowerResource) * gameServer().marketServer().getWholePrice(ePowerResource)) / Constants.RESOURCE_MULTIPLIER) < iTheshold)
                    {
                        BuildingType eBestBuilding = BuildingType.NONE;
                        int iBestValue = 0;

                        TileServer pBestTile = null;
                        int iBestConstructValue = 0;

                        foreach (InfoBuilding pLoopBuilding in infos().buildings())
                        {
                            if (canEverConstruct(pLoopBuilding.meType, true, true))
                            {
                                if (Utils.isBuildingYield(pLoopBuilding.meType, ePowerResource, gameServer()))
                                {
                                    TileServer pConstructTile = null;
                                    int iConstructValue = 0;

                                    if (AI_bestBuildingResourceConstruct(pLoopBuilding.meType, ePowerResource, iMinPriceMultiplier, false, ref pConstructTile, ref iConstructValue, null))
                                    {
                                        int iConnections = pConstructTile.countConnections(pLoopBuilding.meType, getPlayer(), true, false);

                                        int iValue = gameServer().calculateRevenue(pLoopBuilding.meType, pConstructTile, getPlayer(), iConnections, true, true, 100, 100, 100, false);
                                        if (iValue > 0)
                                        {
                                            iValue += ((gameServer().calculateRate(pLoopBuilding.meType, ePowerResource, pConstructTile, getPlayer(), iConnections, true, 100) * gameServer().marketServer().getWholePrice(ePowerResource)) / Constants.RESOURCE_MULTIPLIER);

                                            int iPercent = AI_fundResourcesPercent(0, iMinPriceMultiplier, true, true, getBuildingResourceCost(pLoopBuilding.meType), null, false, PlayerType.NONE);
                                            if (iPercent == 100)
                                            {
                                                iValue *= 6;
                                                iValue /= 5;
                                            }
                                            else
                                            {
                                                iValue *= iPercent;
                                                iValue /= 100;
                                            }

                                            if (iValue >= iBestValue)
                                            {
                                                if ((iValue > iBestValue) || (iConstructValue > iBestConstructValue))
                                                {
                                                    eBestBuilding = pLoopBuilding.meType;
                                                    iBestValue = iValue;

                                                    pBestTile = pConstructTile;
                                                    iBestConstructValue = iConstructValue;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if ((pBestTile != null) && (eBestBuilding != BuildingType.NONE))
                        {
                            if (AI_constructFund(pBestTile, eBestBuilding, iMinPriceMultiplier, false, null))
                            {
                                if (AI_LOGGING) Debug.Log("--->Construct Power (" + TEXT(infos().building(eBestBuilding).meName) + " -$" + -((AI_getResourceRateAverage(ePowerResource) * gameServer().marketServer().getWholePrice(ePowerResource)) / Constants.RESOURCE_MULTIPLIER) + ") at " + pBestTile.getX() + ", " + pBestTile.getY() + " [" + getRealName() + "]");

                                return true;
                            }
                            else
                            {
                                if (AI_LOGGING) Debug.Log("--->(" + getRealName() + " SKIPPED) Construct Power (" + TEXT(infos().building(eBestBuilding).meName) + " -$" + -((AI_getResourceRateAverage(ePowerResource) * gameServer().marketServer().getWholePrice(ePowerResource)) / Constants.RESOURCE_MULTIPLIER) + ") at " + pBestTile.getX() + ", " + pBestTile.getY() + "[" + getRealName() + "]");
                            }
                        }
                    }
                }

                return false;
            }
        }

        protected virtual bool AI_validBuilding(BuildingType eBuilding, BuildingType eInputBuilding, TileServer pTile, int iMinPriceMultiplier)
        {
            if (!(gameServer().isBuildingHasInput(eBuilding)) || (pTile.countConnections(eBuilding, getPlayer(), true, false) > 0))
            {
                return true;
            }
            else if (getClaims() > (((getHQLevelInt() > 1) ? 1 : 0) + ((eInputBuilding != BuildingType.NONE) ? 1 : 0)))
            {
                List<int> aiExtraResources = null;

                if (eInputBuilding != BuildingType.NONE)
                {
                    aiExtraResources = infos().resources().Select(i => 0).ToList();

                    for (ResourceType eExtraResource = 0; eExtraResource < infos().resourcesNum(); eExtraResource++)
                    {
                        aiExtraResources[(int)eExtraResource] += getBuildingResourceCost(eBuilding, eExtraResource);
                        aiExtraResources[(int)eExtraResource] += getBuildingResourceCost(eInputBuilding, eExtraResource);
                    }
                }
                else
                {
                    aiExtraResources = getBuildingResourceCost(eBuilding);
                }

                if (AI_constructFund(null, eBuilding, iMinPriceMultiplier, true, aiExtraResources))
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual bool AI_doConstructRequired(int iMinPriceMultiplier)
        {
            using (new UnityProfileScope("Player::AI_doConstructRequired"))
            {
                if (getHQLevelInt() < 2)
                {
                    return false;
                }

                BuildingType eBestBuilding = BuildingType.NONE;
                BuildingType eBestInputBuilding = BuildingType.NONE;
                ResourceType eBestResource = ResourceType.NONE;
                int iBestValue = ((gameServer().isCampaignSevenSols()) ? 10 : 30);

                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    if (infos().resource(eLoopResource).mbTrade)
                    {
                        if ((AI_getResourceLifeSupport(eLoopResource) < 0) || (eLoopResource == getGasResource()))
                        {
                            int iTheshold = infos().personality(getPersonality()).miConstructRequiredThreshold;

                            if (gameServer().isCampaignSevenSols())
                            {
                                iTheshold += 10;
                            }

                            if (((AI_getResourceRateAverage(eLoopResource) * gameServer().marketServer().getWholePrice(eLoopResource)) / Constants.RESOURCE_MULTIPLIER) < iTheshold)
                            {
                                BuildingType eConstructBuilding = BuildingType.NONE;
                                BuildingType eConstructInputBuilding = BuildingType.NONE;

                                TileServer pConstructTile = AI_doConstructResource(eLoopResource, iMinPriceMultiplier, false, true, ref eConstructBuilding, ref eConstructInputBuilding);
                                if (pConstructTile != null)
                                {
                                    if (AI_validBuilding(eConstructBuilding, eConstructInputBuilding, pConstructTile, iMinPriceMultiplier))
                                    {
                                        int iValue = gameServer().calculateRevenue(eConstructBuilding, pConstructTile, getPlayer(), pConstructTile.countConnections(eConstructBuilding, getPlayer(), true, false), true, true, 50, 50, 100, false);
                                        if (iValue > 0)
                                        {
                                            iValue += -((AI_getResourceRateAverage(eLoopResource) * gameServer().marketServer().getWholePrice(eLoopResource)) / Constants.RESOURCE_MULTIPLIER);

                                            if (iValue > iBestValue)
                                            {
                                                eBestBuilding = eConstructBuilding;
                                                eBestInputBuilding = eConstructInputBuilding;
                                                eBestResource = eLoopResource;
                                                iBestValue = iValue;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if ((eBestBuilding != BuildingType.NONE) && (eBestResource != ResourceType.NONE))
                {
                    if (AI_getResourceConstructionCount(eBestResource) > 0)
                    {
                        return false;
                    }

                    if (gameServer().calculateOpenCount(60, eBestBuilding, getPlayer()) < 40)
                    {
                        return false;
                    }

                    TileServer pFirstTile = AI_doConstructResource(eBestResource, iMinPriceMultiplier, false, false, ref eBestBuilding, ref eBestInputBuilding);

                    if (pFirstTile != null)
                    {
                        if (AI_LOGGING) Debug.Log("-->Construct Required " + TEXT(infos().resource(eBestResource).meName) + " (" + TEXT(infos().building(eBestBuilding).meName) + " -$" + -((AI_getResourceRateAverage(eBestResource) * gameServer().marketServer().getWholePrice(eBestResource)) / Constants.RESOURCE_MULTIPLIER) + ")");

                        if (gameServer().isBuildingHasInput(eBestBuilding))
                        {
                            if (!(gameServer().isNoAdjacency()))
                            {
                                TileServer pSecondTile = AI_doConstructResource(eBestResource, iMinPriceMultiplier, true, true, ref eBestBuilding, ref eBestInputBuilding);

                                if ((pSecondTile != null) && Utils.adjacentTiles(pFirstTile, pSecondTile))
                                {
                                    if (AI_doConstructResource(eBestResource, iMinPriceMultiplier, true, false, ref eBestBuilding, ref eBestInputBuilding) != null)
                                    {
                                        if (AI_LOGGING) Debug.Log("--->Construct Required (X2) " + TEXT(infos().resource(eBestResource).meName) + " (" + TEXT(infos().building(eBestBuilding).meName) + " -$" + -((AI_getResourceRateAverage(eBestResource) * gameServer().marketServer().getWholePrice(eBestResource)) / Constants.RESOURCE_MULTIPLIER) + ")");
                                    }
                                }
                            }
                        }

                        return true;
                    }
                }

                return false;
            }
        }

        protected virtual bool AI_doConstructBaseResource(int iMinPriceMultiplier, bool bTestRevenue = true)
        {
            using (new UnityProfileScope("Player::AI_doConstructBaseResource"))
            {
                ResourceType eBaseResource = infos().HQ(getHQ()).meBaseResource;

                int iMaxProduction = ((getHQLevelInt() == 1) ? 2 : 3);

                if (AI_getResourceProductionCount(eBaseResource) >= iMaxProduction)
                {
                    return false;
                }

                if (gameServer().marketServer().getWholePrice(eBaseResource) < (infos().resource(eBaseResource).miMarketPrice / 2))
                {
                    return false;
                }

                if (AI_getResourceConstructionCount(eBaseResource) > 0)
                {
                    return false;
                }

                if (AI_getResourceRateAverage(eBaseResource) > ((2 * Constants.RESOURCE_MULTIPLIER * Math.Max(0, (infos().HQ(getHQ()).miBaseResourceModifier + 100))) / 100))
                {
                    return false;
                }

                BuildingType eBestBuilding = BuildingType.NONE;
                BuildingType eBestInputBuilding = BuildingType.NONE;

                TileServer pConstructTile = AI_doConstructResource(eBaseResource, iMinPriceMultiplier, false, true, ref eBestBuilding, ref eBestInputBuilding);

                if (pConstructTile != null)
                {
                    if (AI_validBuilding(eBestBuilding, eBestInputBuilding, pConstructTile, iMinPriceMultiplier))
                    {
                        int iValue = gameServer().calculateRevenue(eBestBuilding, pConstructTile, getPlayer(), pConstructTile.countConnections(eBestBuilding, getPlayer(), true, false), true, true, ((getHQLevelInt() > 1) ? 50 : 0), 50, 100, false);
                        if (iValue > ((bTestRevenue) ? infos().personality(getPersonality()).miConstructBaseValue : -5))
                        {
                            if (gameServer().calculateOpenCount(60, eBestBuilding, getPlayer()) < 40)
                            {
                                return false;
                            }

                            TileServer pFirstTile = AI_doConstructResource(eBaseResource, iMinPriceMultiplier, false, false, ref eBestBuilding, ref eBestInputBuilding);

                            if (pFirstTile != null)
                            {
                                if (AI_LOGGING) Debug.Log("-->Construct Base Resource " + TEXT(infos().resource(eBaseResource).meName) + " (" + TEXT(infos().building(eBestBuilding).meName) + ")");

                                if (AI_getResourceProductionCount(eBaseResource) < iMaxProduction)
                                {
                                    if (!(gameServer().isNoAdjacency()))
                                    {
                                        TileServer pSecondTile = AI_doConstructResource(eBaseResource, iMinPriceMultiplier, true, true, ref eBestBuilding, ref eBestInputBuilding);

                                        if ((pSecondTile != null) && Utils.adjacentTiles(pFirstTile, pSecondTile))
                                        {
                                            if (AI_doConstructResource(eBaseResource, iMinPriceMultiplier, true, false, ref eBestBuilding, ref eBestInputBuilding) != null)
                                            {
                                                if (AI_LOGGING) Debug.Log("--->Construct Base Resource (X2) " + TEXT(infos().resource(eBaseResource).meName) + " (" + TEXT(infos().building(eBestBuilding).meName) + ")");
                                            }
                                        }
                                    }
                                }

                                return true;
                            }
                        }
                    }
                }

                return false;
            }
        }

        protected virtual bool AI_doConstructUpgradeResource(int iMinPriceMultiplier)
        {
            using (new UnityProfileScope("Player::AI_doConstructUpgradeResource"))
            {
                if (getHQLevelInt() > 3)
                {
                    return false;
                }

                BuildingType eBestBuilding = BuildingType.NONE;
                BuildingType eBestInputBuilding = BuildingType.NONE;
                ResourceType eBestResource = ResourceType.NONE;
                int iBestValue = 0;

                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    int iValue = getUpgradeResourceCost(eLoopResource);
                    if (iValue > 0)
                    {
                        if (AI_getWholeResourceRateAverage(eLoopResource) < 1)
                        {
                            int iPrice = gameServer().marketServer().getWholePrice(eLoopResource);
                            if (iPrice > (infos().resource(eLoopResource).miMarketPrice / 2))
                            {
                                iValue *= iPrice;

                                if (iValue > iBestValue)
                                {
                                    BuildingType eConstructBuilding = BuildingType.NONE;
                                    BuildingType eConstructInputBuilding = BuildingType.NONE;

                                    TileServer pConstructTile = AI_doConstructResource(eLoopResource, iMinPriceMultiplier, false, true, ref eConstructBuilding, ref eConstructInputBuilding);
                                    if (pConstructTile != null)
                                    {
                                        if (AI_validBuilding(eConstructBuilding, eConstructInputBuilding, pConstructTile, iMinPriceMultiplier))
                                        {
                                            if ((getHQLevelInt() > 1) || !(gameServer().isBuildingHasInput(eConstructBuilding)))
                                            {
                                                int iNet = gameServer().calculateRevenue(eConstructBuilding, pConstructTile, getPlayer(), pConstructTile.countConnections(eConstructBuilding, getPlayer(), true, false), true, true, 50, 50, 100, false);
                                                if (iNet > infos().personality(getPersonality()).miConstructUpgradeValue)
                                                {
                                                    eBestBuilding = eConstructBuilding;
                                                    eBestInputBuilding = eConstructInputBuilding;
                                                    eBestResource = eLoopResource;
                                                    iBestValue = iValue;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if ((eBestBuilding != BuildingType.NONE) && (eBestResource != ResourceType.NONE))
                {
                    if (AI_getResourceConstructionCount(eBestResource) > 0)
                    {
                        return false;
                    }

                    if (gameServer().calculateOpenCount(60, eBestBuilding, getPlayer()) < 40)
                    {
                        return false;
                    }

                    TileServer pFirstTile = AI_doConstructResource(eBestResource, iMinPriceMultiplier, false, false, ref eBestBuilding, ref eBestInputBuilding);

                    if (pFirstTile != null)
                    {
                        if (AI_LOGGING) Debug.Log("-->Construct Upgrade Resource " + TEXT(infos().resource(eBestResource).meName) + " (" + TEXT(infos().building(eBestBuilding).meName) + ")");

                        if (gameServer().isBuildingHasInput(eBestBuilding))
                        {
                            if (!(gameServer().isNoAdjacency()))
                            {
                                TileServer pSecondTile = AI_doConstructResource(eBestResource, iMinPriceMultiplier, true, true, ref eBestBuilding, ref eBestInputBuilding);

                                if ((pSecondTile != null) && Utils.adjacentTiles(pFirstTile, pSecondTile))
                                {
                                    if (AI_doConstructResource(eBestResource, iMinPriceMultiplier, true, false, ref eBestBuilding, ref eBestInputBuilding) != null)
                                    {
                                        if (AI_LOGGING) Debug.Log("--->Construct Upgrade Resource (X2) " + TEXT(infos().resource(eBestResource).meName) + " (" + TEXT(infos().building(eBestBuilding).meName) + ")");
                                    }
                                }
                            }
                        }

                        return true;
                    }
                }

                return false;
            }
        }

        protected virtual bool AI_doConstructOrderResource(int iMinPriceMultiplier)
        {
            if (getHQLevelInt() < 3)
            {
                return false;
            }

            List<int> aiResourceCount = Enumerable.Repeat(0, (int)infos().resourcesNum()).ToList();

            if (getOrderCapacity(OrderType.PATENT) > 0)
            {
                foreach (InfoPatent pLoopPatent in infos().patents())
                {
                    if (canPatent(pLoopPatent.meType, false))
                    {
                        for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                        {
                            aiResourceCount[(int)eLoopResource] += getPatentResourceCost(pLoopPatent.meType, eLoopResource);
                        }
                    }
                }
            }

            if (getOrderCapacity(OrderType.RESEARCH) > 0)
            {
                foreach (InfoTechnology pLoopTechnology in infos().technologies())
                {
                    if (canResearch(pLoopTechnology.meType, getTechnologyLevelResearching(pLoopTechnology.meType) + 1, false))
                    {
                        for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                        {
                            aiResourceCount[(int)eLoopResource] += getResearchResourceCost(pLoopTechnology.meType, getTechnologyLevelResearching(pLoopTechnology.meType) + 1, eLoopResource);
                        }
                    }
                }
            }

            BuildingType eBestBuilding = BuildingType.NONE;
            BuildingType eBestInputBuilding = BuildingType.NONE;
            ResourceType eBestResource = ResourceType.NONE;
            int iBestValue = 0;

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                if (AI_getWholeResourceRateAverage(eLoopResource) <= 0)
                {
                    int iValue = aiResourceCount[(int)eLoopResource];
                    if (iValue > iBestValue)
                    {
                        BuildingType eConstructBuilding = BuildingType.NONE;
                        BuildingType eConstructInputBuilding = BuildingType.NONE;

                        TileServer pConstructTile = AI_doConstructResource(eLoopResource, iMinPriceMultiplier, false, true, ref eConstructBuilding, ref eConstructInputBuilding);
                        if (pConstructTile != null)
                        {
                            if (AI_validBuilding(eConstructBuilding, eConstructInputBuilding, pConstructTile, iMinPriceMultiplier))
                            {
                                int iNet = gameServer().calculateRevenue(eConstructBuilding, pConstructTile, getPlayer(), pConstructTile.countConnections(eConstructBuilding, getPlayer(), true, false), true, true, 50, 50, 100, false);
                                if (iNet > infos().personality(getPersonality()).miConstructOrderValue)
                                {
                                    eBestBuilding = eConstructBuilding;
                                    eBestInputBuilding = eConstructInputBuilding;
                                    eBestResource = eLoopResource;
                                    iBestValue = iValue;
                                }
                            }
                        }
                    }
                }
            }

            if ((eBestBuilding != BuildingType.NONE) && (eBestResource != ResourceType.NONE))
            {
                if (AI_getResourceConstructionCount(eBestResource) > 0)
                {
                    return false;
                }

                if (gameServer().calculateOpenCount(60, eBestBuilding, getPlayer()) < 40)
                {
                    return false;
                }

                TileServer pFirstTile = AI_doConstructResource(eBestResource, iMinPriceMultiplier, false, false, ref eBestBuilding, ref eBestInputBuilding);

                if (pFirstTile != null)
                {
                    if (AI_LOGGING) Debug.Log("-->Construct Order Resource " + TEXT(infos().resource(eBestResource).meName) + " (" + TEXT(infos().building(eBestBuilding).meName) + ")");

                    if (gameServer().isBuildingHasInput(eBestBuilding))
                    {
                        if (!(gameServer().isNoAdjacency()))
                        {
                            TileServer pSecondTile = AI_doConstructResource(eBestResource, iMinPriceMultiplier, true, true, ref eBestBuilding, ref eBestInputBuilding);

                            if ((pSecondTile != null) && Utils.adjacentTiles(pFirstTile, pSecondTile))
                            {
                                if (AI_doConstructResource(eBestResource, iMinPriceMultiplier, true, false, ref eBestBuilding, ref eBestInputBuilding) != null)
                                {
                                    if (AI_LOGGING) Debug.Log("--->Construct Order Resource (X2) " + TEXT(infos().resource(eBestResource).meName) + " (" + TEXT(infos().building(eBestBuilding).meName) + ")");
                                }
                            }
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        protected virtual bool AI_doConstructBestBuilding(int iMinRevenue, int iMinPriceMultiplier)
        {
            using (new UnityProfileScope("Player::AI_doConstructBestBuilding"))
            {
                BuildingType eBestBuilding = BuildingType.NONE;
                ResourceType eBestResource = ResourceType.NONE;
                int iBestValue = iMinRevenue + infos().personality(getPersonality()).miConstructBestDiff;

                TileServer pBestConstructTile = null;
                int iBestConstructValue = 0;

                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    if (gameServer().isResourceValid(eLoopResource))
                    {
                        BuildingType eResourceBuilding = BuildingType.NONE;
                        int iResourceValue = iBestValue;

                        TileServer pResourceConstructTile = null;
                        int iResourceConstructValue = iBestConstructValue;

                        foreach (InfoBuilding pLoopBuilding in infos().buildings())
                        {
                            if (canEverConstruct(pLoopBuilding.meType, true, true))
                            {
                                if (Utils.isBuildingYield(pLoopBuilding.meType, eLoopResource, gameServer()))
                                {
                                    int iPriceThreshold = infos().resource(eLoopResource).miMarketPrice;

                                    if (!(gameServer().isBuildingHasInput(pLoopBuilding.meType)))
                                    {
                                        iPriceThreshold *= (4 + AI_getResourceProductionCountTeam(eLoopResource));
                                        iPriceThreshold /= (4 + 0);
                                    }

                                    if (gameServer().marketServer().getWholePrice(eLoopResource) > iPriceThreshold)
                                    {
                                        TileServer pConstructTile = null;
                                        int iConstructValue = 0;

                                        if (AI_bestBuildingResourceConstruct(pLoopBuilding.meType, eLoopResource, iMinPriceMultiplier, false, ref pConstructTile, ref iConstructValue, null))
                                        {
                                            int iValue = gameServer().calculateRevenue(pLoopBuilding.meType, pConstructTile, getPlayer(), pConstructTile.countConnections(pLoopBuilding.meType, getPlayer(), true, false), true, true, 75, 75, 100, false);
                                            if (iValue > 0)
                                            {
                                                if (getHQLevelInt() < 2)
                                                {
                                                    iValue /= 3;
                                                }
                                                else if (getHQLevelInt() < 3)
                                                {
                                                    iValue *= 2;
                                                    iValue /= 3;
                                                }

                                                if (!(infos().resource(eLoopResource).mbTrade))
                                                {
                                                    if (isSubsidiary())
                                                    {
                                                        iValue /= 3;
                                                    }
                                                    else
                                                    {
                                                        iValue *= 2;
                                                        iValue /= 3;
                                                    }
                                                }

                                                if ((eLoopResource == infos().HQ(getHQ()).meBaseResource) ||
                                                    (gameServer().isResourceInputPlayer(infos().HQ(getHQ()).meBaseResource, eLoopResource, getPlayer())))
                                                {
                                                    iValue *= 6;
                                                    iValue /= 5;
                                                }

                                                iValue /= Math.Max(1, (AI_getWholeResourceRateAverage(eLoopResource) + 1));

                                                iValue /= Math.Max(1, (getWholeResourceStockpile(eLoopResource, true) / 100));

                                                for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameServer().getNumPlayers(); eLoopPlayer++)
                                                {
                                                    if (gameServer().playerServer(eLoopPlayer).AI_getWholeResourceRateAverage(eLoopResource) < 0)
                                                    {
                                                        iValue *= ((int)(gameServer().getNumPlayers()) + 1);
                                                        iValue /= ((int)(gameServer().getNumPlayers()) + 0);
                                                    }
                                                }

                                                if (!(infos().resource(eLoopResource).mbTrade))
                                                {
                                                    iValue /= Math.Max(0, (getDebt() / -10000)) + 1;
                                                }

                                                if (iValue >= iResourceValue)
                                                {
                                                    if ((iValue > iResourceValue) || (iConstructValue > iResourceConstructValue))
                                                    {
                                                        eResourceBuilding = pLoopBuilding.meType;
                                                        iResourceValue = iValue;

                                                        pResourceConstructTile = pConstructTile;
                                                        iResourceConstructValue = iConstructValue;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if ((pResourceConstructTile != null) && (eResourceBuilding != BuildingType.NONE))
                        {
                            if (AI_constructFund(pResourceConstructTile, eResourceBuilding, iMinPriceMultiplier, true, null))
                            {
                                eBestBuilding = eResourceBuilding;
                                eBestResource = eLoopResource;
                                iBestValue = iResourceValue;

                                pBestConstructTile = pResourceConstructTile;
                                iBestConstructValue = iResourceConstructValue;
                            }
                        }
                    }
                }

                if ((pBestConstructTile != null) && (eBestBuilding != BuildingType.NONE) && (eBestResource != ResourceType.NONE))
                {
                    if (AI_getResourceConstructionCount(eBestResource) > 0)
                    {
                        return false;
                    }

                    if (gameServer().calculateOpenCount(60, eBestBuilding, getPlayer()) < 40)
                    {
                        return false;
                    }

                    if (AI_constructFund(pBestConstructTile, eBestBuilding, iMinPriceMultiplier, false, null))
                    {
                        if (AI_LOGGING) Debug.Log("-->Construct Best Building " + TEXT(infos().building(eBestBuilding).meName) + " (" + TEXT(infos().resource(eBestResource).meName) + " $" + gameServer().marketServer().getWholePrice(eBestResource) + ")" + " at " + pBestConstructTile.getX() + ", " + pBestConstructTile.getY() + " [" + getRealName() + "]");

                        if (gameServer().isBuildingHasInput(eBestBuilding))
                        {
                            if (!(gameServer().isNoAdjacency()))
                            {
                                TileServer pSecondTile = null;
                                int iSecondValue = 0;

                                if (AI_bestBuildingResourceConstruct(eBestBuilding, eBestResource, iMinPriceMultiplier, true, ref pSecondTile, ref iSecondValue, null))
                                {
                                    if ((pSecondTile != null) && Utils.adjacentTiles(pBestConstructTile, pSecondTile))
                                    {
                                        if (AI_constructFund(pSecondTile, eBestBuilding, iMinPriceMultiplier, false, null))
                                        {
                                            if (AI_LOGGING) Debug.Log("--->Construct Best Building (X2) " + TEXT(infos().building(eBestBuilding).meName) + " (" + TEXT(infos().resource(eBestResource).meName) + " $" + gameServer().marketServer().getWholePrice(eBestResource) + ")" + " at " + pSecondTile.getX() + ", " + pSecondTile.getY() + " [" + getRealName() + "]");
                                        }
                                    }
                                }
                            }
                        }

                        return true;
                    }

                    if (AI_LOGGING) Debug.Log("-->(" + getRealName() + " ERROR) Construct Best Building " + TEXT(infos().building(eBestBuilding).meName) + " (" + TEXT(infos().resource(eBestResource).meName) + " $" + gameServer().marketServer().getWholePrice(eBestResource) + ")");
                }

                return false;
            }
        }

        public virtual BuildingType AI_doConstructBestBuildingTile(int iMinNet, int iMinPriceMultiplier, TileServer pTile, BuildingType eForceBuilding, bool bTest)
        {
            BuildingType eBestBuilding = BuildingType.NONE;
            int iBestValue = 0;

            bool bHQConnection = pTile.isPotentialHQConnection(getPlayer());

            int iNoTradeMultiplier = 100;

            iNoTradeMultiplier = Math.Max(50, (iNoTradeMultiplier + ((getDebt() + 10000) / 1000)));

            foreach (InfoBuilding pLoopBuilding in infos().buildings())
            {
                BuildingType eLoopBuilding = pLoopBuilding.meType;

                if (canEverConstruct(eLoopBuilding, true, true))
                {
                    if (Utils.isBuildingRevenue(eLoopBuilding, gameServer()))
                    {
                        if ((infos().building(eLoopBuilding).miEntertainment == 0) || AI_entertainmentNeeded(pTile))
                        {
                            if (canConstructPlayer(eLoopBuilding, false) && gameServer().canTileHaveBuilding(pTile, eLoopBuilding, getPlayer()))
                            {
                                bool bValid = true;

                                if (eLoopBuilding != eForceBuilding)
                                {
                                    for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                    {
                                        if (Utils.isBuildingYield(eLoopBuilding, eLoopResource, gameServer()))
                                        {
                                            if (AI_getResourceProductionCountTeam(eLoopResource) >= AI_resourceProductionMax(eLoopBuilding, bHQConnection))
                                            {
                                                bValid = false;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (bValid)
                                {
                                    int iNet = gameServer().calculateRevenue(eLoopBuilding, pTile, getPlayer(), pTile.countConnections(eLoopBuilding, getPlayer(), true, false), true, true, 100, 100, iNoTradeMultiplier, false);
                                    if (iNet >= iMinNet)
                                    {
                                        int iValue = iNet;

                                        int iReverseConnections = pTile.countConnections(eLoopBuilding, getPlayer(), true, true);
                                        if (iReverseConnections > 0)
                                        {
                                            iValue *= (3 + iReverseConnections);
                                            iValue /= (3);
                                        }

                                        if (iValue > iBestValue)
                                        {
                                            eBestBuilding = eLoopBuilding;
                                            iBestValue = iValue;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (eBestBuilding != BuildingType.NONE)
            {
                if (bTest)
                {
                    if (AI_constructFund(pTile, eBestBuilding, iMinPriceMultiplier, true, null))
                    {
                        return eBestBuilding;
                    }
                }
                else
                {
                    if (gameServer().calculateOpenCount(60, eBestBuilding, getPlayer()) < 40)
                    {
                        return BuildingType.NONE;
                    }

                    if (AI_constructFund(pTile, eBestBuilding, iMinPriceMultiplier, false, null))
                    {
                        if (AI_LOGGING) Debug.Log("Construct Best Building Tile " + TEXT(infos().building(eBestBuilding).meName) + " (" + pTile.getX() + " , " + pTile.getY() + ")" + " [" + getRealName() + "]");

                        return eBestBuilding;
                    }
                }
            }

            return BuildingType.NONE;
        }

        protected virtual bool AI_doConstructEmpty()
        {
            using (new UnityProfileScope("Player::AI_doConstructEmpty"))
            {
                foreach (int iTileID in getTileList())
                {
                    TileServer pLoopTile = gameServer().tileServer(iTileID);

                    if (pLoopTile.isOwnerReal())
                    {
                        if (pLoopTile.isEmpty())
                        {
                            if (pLoopTile.getTotalUnitMissionCount(MissionType.CONSTRUCT) == 0)
                            {
                                if (AI_doConstructBestBuildingTile(10, 20, pLoopTile, BuildingType.NONE, false) != BuildingType.NONE)
                                {
                                    if (AI_LOGGING) Debug.Log("-->Construct Empty Tile: " + pLoopTile.getX() + ", " + pLoopTile.getY() + ")");

                                    return true;
                                }
                            }
                        }
                    }
                }

                return false;
            }
        }

        protected virtual bool AI_doConstructMissing()
        {
            using (new UnityProfileScope("Player::AI_doConstructMissing"))
            {
                const int MIN_PRICE_MULTIPLIER = 50;

                ResourceType eBestResource = ResourceType.NONE;
                int iBestValue = infos().personality(getPersonality()).miConstructMissingValue;

                foreach (int iBuildingID in getBuildingList())
                {
                    BuildingServer pLoopBuilding = gameServer().buildingServer(iBuildingID);

                    for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                    {
                        if (pLoopBuilding.wantsInput(eLoopResource))
                        {
                            if (AI_getResourceConstructionCount(eLoopResource) == 0)
                            {
                                if (AI_getWholeResourceRateAverage(eLoopResource) < 0)
                                {
                                    if (gameServer().calculateRevenue(pLoopBuilding.getType(), pLoopBuilding.tileServer(), pLoopBuilding.getOwner(), pLoopBuilding.getConnections(), true, false, 0, 0, 100, false) < 50)
                                    {
                                        BuildingType eBuilding = BuildingType.NONE;
                                        BuildingType eInputBuilding = BuildingType.NONE;

                                        TileServer pTile = AI_doConstructResource(eLoopResource, MIN_PRICE_MULTIPLIER, false, true, ref eBuilding, ref eInputBuilding);
                                        if (pTile != null)
                                        {
                                            int iValue = gameServer().calculateRevenue(eBuilding, pTile, getPlayer(), pTile.countConnections(eBuilding, getPlayer(), true, false), true, true, 100, 100, 100, false);
                                            if (iValue > iBestValue)
                                            {
                                                eBestResource = eLoopResource;
                                                iBestValue = iValue;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }


                if (eBestResource != ResourceType.NONE)
                {
                    BuildingType eBestBuilding = BuildingType.NONE;
                    BuildingType eBestInputBuilding = BuildingType.NONE;

                    TileServer pTile = AI_doConstructResource(eBestResource, MIN_PRICE_MULTIPLIER, false, false, ref eBestBuilding, ref eBestInputBuilding);
                    if (pTile != null)
                    {
                        if (AI_LOGGING) Debug.Log("-->Construct Missing Resource " + TEXT(infos().resource(eBestResource).meName) + " (" + TEXT(infos().building(eBestBuilding).meName) + ": " + pTile.getX() + ", " + pTile.getY() + ")");
                        return true;
                    }
                }

                return false;
            }
        }

        protected virtual bool AI_entertainmentNeeded(TileClient pTile)
        {
            if (!isWinEligible())
            {
                if ((pTile != null) && !(pTile.adjacentToModule()))
                {
                    return false;
                }
            }

            if (getRealConstructionCountEntertainment() > 0)
            {
                return false;
            }

            if (getRealBuildingCountEntertainment() != getBuildingCountEntertainment())
            {
                return false;
            }

            {
                int iCount = (getRealBuildingCountEntertainment() + getRealConstructionCountEntertainment());

                if (iCount > 0)
                {
                    if (!isWinEligible())
                    {
                        return false;
                    }

                    bool bValid = false;

                    for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameServer().getNumPlayers(); eLoopPlayer++)
                    {
                        PlayerServer pLoopPlayer = gameServer().playerServer(eLoopPlayer);

                        if (pLoopPlayer.isWinEligible())
                        {
                            if (pLoopPlayer.getTeam() != getTeam())
                            {
                                if ((pLoopPlayer.getRealBuildingCountEntertainment() + pLoopPlayer.getRealConstructionCountEntertainment()) > iCount)
                                {
                                    bValid = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (!bValid)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        protected virtual bool AI_doConstructEntertainment(int iMinPriceMultiplier)
        {
            if (gameServer().random().Next(infos().personality(getPersonality()).miEntertainmentConstructRoll) == 0)
            {
                return false;
            }

            if (!AI_entertainmentNeeded(null))
            {
                return false;
            }

            TileServer pBestTile = null;
            BuildingType eBestBuilding = BuildingType.NONE;
            int iBestValue = infos().personality(getPersonality()).miMinEntertainmentValue;

            iBestValue *= (4 + getRealConstructionCountEntertainment() + getRealBuildingCountEntertainment());
            iBestValue /= (4);

            for (BuildingType eLoopBuilding = 0; eLoopBuilding < infos().buildingsNum(); eLoopBuilding++)
            {
                if (infos().building(eLoopBuilding).miEntertainment > 0)
                {
                    TileServer pBuildingTile = AI_bestEntertainmentConstruct(eLoopBuilding, iMinPriceMultiplier);

                    if (pBuildingTile != null)
                    {
                        int iValue = gameServer().calculateRevenue(eLoopBuilding, pBuildingTile, getPlayer(), pBuildingTile.countConnections(eLoopBuilding, getPlayer(), true, false), true, true, 100, 100, 100, false);
                        if (iValue > iBestValue)
                        {
                            pBestTile = pBuildingTile;
                            eBestBuilding = eLoopBuilding;
                            iBestValue = iValue;
                        }
                    }
                }
            }

            if ((pBestTile != null) && (eBestBuilding != BuildingType.NONE))
            {
                if (AI_constructFund(pBestTile, eBestBuilding, iMinPriceMultiplier, false, null))
                {
                    if (AI_LOGGING) Debug.Log("Construct Entertainment " + TEXT(infos().building(eBestBuilding).meName) + " at " + pBestTile.getX() + ", " + pBestTile.getY() + " [" + getRealName() + "]");

                    return true;
                }
                else
                {
                    if (AI_LOGGING) Debug.Log("Construct Entertainment: Couldn't Build " + TEXT(infos().building(eBestBuilding).meName) + " at " + pBestTile.getX() + ", " + pBestTile.getY() + " [" + getRealName() + "]");
                }
            }

            return false;
        }

        protected virtual bool AI_constructOrderValid(OrderType eOrder, int iCount)
        {
            if (getNumBuildings() < 6)
            {
                return false;
            }

            if (eOrder == OrderType.PATENT)
            {
                if (gameServer().countPatentsAvailable() < (gameServer().countPatentsPossible() / 3))
                {
                    return false;
                }
            }

            if ((eOrder == OrderType.HACK) || (eOrder == OrderType.LAUNCH))
            {
                if (getHQLevelInt() < 4)
                {
                    return false;
                }
            }

            if (eOrder == OrderType.LAUNCH)
            {
                if (getBestLaunchProfit() < (100 * getOrderTurns(OrderType.LAUNCH, infos().Globals.LAUNCH_TIME, null)))
                {
                    return false;
                }
            }
            else
            {
                if (getRealOrderBuildingCount() >= (getHQLevelInt() - 1))
                {
                    return false;
                }
            }

            return (getRealBuildingOrderCount(eOrder) < iCount);
        }

        protected virtual bool AI_wantLaunchBuilding(BuildingType eBuilding)
        {
            int iRevenue = (getBestLaunchProfit() * NUM_TURNS * ((isSubsidiary()) ? 1 : 2));

            iRevenue /= getOrderTurns(OrderType.LAUNCH, infos().Globals.LAUNCH_TIME, null);

            int iCost = getBuildingMoneyCost(eBuilding, true);

            return (iRevenue >= iCost);
        }

        protected virtual bool AI_doConstructOrder(OrderType eOrder, int iCount, int iMinPriceMultiplier)
        {
            if (AI_constructOrderValid(eOrder, iCount))
            {
                TileServer pBestTile = null;
                BuildingType eBestBuilding = BuildingType.NONE;
                int iBestCost = int.MaxValue;

                for (BuildingType eLoopBuilding = 0; eLoopBuilding < infos().buildingsNum(); eLoopBuilding++)
                {
                    if (infos().buildingClass(infos().building(eLoopBuilding).meClass).meOrderType == eOrder)
                    {
                        if ((eOrder == OrderType.LAUNCH) ? AI_wantLaunchBuilding(eLoopBuilding) : true)
                        {
                            TileServer pBuildingTile = AI_bestOrderConstruct(eLoopBuilding, iMinPriceMultiplier);

                            if (pBuildingTile != null)
                            {
                                int iCost = getBuildingMoneyCost(eLoopBuilding, true);
                                if (iCost < iBestCost)
                                {
                                    pBestTile = pBuildingTile;
                                    eBestBuilding = eLoopBuilding;
                                    iBestCost = iCost;
                                }
                            }
                        }
                    }
                }

                if ((pBestTile != null) && (eBestBuilding != BuildingType.NONE))
                {
                    if (AI_constructFund(pBestTile, eBestBuilding, iMinPriceMultiplier, false, null))
                    {
                        if (AI_LOGGING) Debug.Log("Construct Order " + TEXT(infos().order(eOrder).meName) + " - " + TEXT(infos().building(eBestBuilding).meName) + " at " + pBestTile.getX() + ", " + pBestTile.getY() + " [" + getRealName() + "]");

                        return true;
                    }
                    else
                    {
                        if (AI_LOGGING) Debug.Log("Construct Order " + TEXT(infos().order(eOrder).meName) + ": Couldn't Build " + TEXT(infos().building(eBestBuilding).meName) + " at " + pBestTile.getX() + ", " + pBestTile.getY() + " [" + getRealName() + "]");
                    }
                }
            }

            return false;
        }

        protected virtual bool AI_doConstructNeeds()
        {
            using (new UnityProfileScope("Player::AI_doConstructNeeds"))
            {
                if (AI_doConstructPowerResource(25))
                {
                    return true;
                }

                if (AI_doConstructRequired(50))
                {
                    return true;
                }

                if (AI_doConstructUpgradeResource(50))
                {
                    return true;
                }

                if (AI_doConstructBestBuilding(30, 100))
                {
                    return true;
                }

                return false;
            }
        }

        protected virtual bool AI_doConstructWants()
        {
            using (new UnityProfileScope("Player::AI_doConstructWants"))
            {
                if (gameServer().random().Next(infos().personality(getPersonality()).miBestConstructRoll) == 0)
                {
                    if (AI_doConstructBestBuilding(20, 125))
                    {
                        return true;
                    }
                }

                if (AI_doConstructEntertainment(100))
                {
                    return true;
                }

                if (AI_doConstructOrderAll())
                {
                    return true;
                }

                if (AI_doConstructUpgradeResource(100))
                {
                    return true;
                }

                if (AI_doConstructOrderResource(100))
                {
                    return true;
                }

                if (gameServer().random().Next(2) == 0)
                {
                    if (AI_doConstructPowerResource(50))
                    {
                        return true;
                    }
                }
                else
                {
                    if (AI_doConstructRequired(100))
                    {
                        return true;
                    }
                }

                if (getClaims() > 1)
                {
                    if (AI_doConstructBestBuilding(10, 150))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        protected virtual int AI_rivalEntertainmentValue()
        {
            if (!isWinEligible())
            {
                return 0;
            }

            int iValue = 0;

            for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameServer().getNumPlayers(); eLoopPlayer++)
            {
                PlayerServer pLoopPlayer = gameServer().playerServer(eLoopPlayer);

                if (pLoopPlayer.isWinEligible())
                {
                    if (pLoopPlayer.getTeam() != getTeam())
                    {
                        if (pLoopPlayer.getEntertainment() > 0)
                        {
                            iValue += ((gameServer().entertainmentProfit(BuildingType.NONE, null, pLoopPlayer.getPlayer(), true, false) * pLoopPlayer.getEntertainment()) / (int)(gameServer().getNumPlayers() + 1));
                        }
                    }
                }
            }

            return iValue;
        }

        protected virtual bool AI_doConstructReplace()
        {
            using (new UnityProfileScope("Player::AI_doConstructReplace"))
            {
                if (gameServer().isLastHalfDay())
                {
                    return false;
                }

                if (getHQLevelInt() < 3)
                {
                    return false;
                }

                if (isRealConstructionAny())
                {
                    return false;
                }

                BuildingType eReplacedBuilding = BuildingType.NONE;
                BuildingType eConstructedBuilding = BuildingType.NONE;
                TileServer pReplacedTile = null;
                int iReplacedCount = 0;

                while (iReplacedCount < ((isSubsidiary()) ? 2 : 3))
                {
                    bool bHasReturnSabotage = false;

                    for (SabotageType eLoopSabotage = 0; eLoopSabotage < infos().sabotagesNum(); eLoopSabotage++)
                    {
                        if (infos().sabotage(eLoopSabotage).mbReturnClaim)
                        {
                            if (getSabotageCount(eLoopSabotage) > 0)
                            {
                                bHasReturnSabotage = true;
                            }
                        }
                    }

                    bool bContinue = false;

                    TileServer pBestTile = null;
                    int iBestValue = 0;

                    for (int i = 0; i < getNumBuildings(); i++)
                    {
                        BuildingServer pLoopBuilding = gameServer().buildingServer(getBuildingList()[i]);

                        if (pLoopBuilding.canScrap())
                        {
                            if (infos().buildingClass(pLoopBuilding.getClass()).meOrderType == OrderType.NONE)
                            {
                                if ((eReplacedBuilding == BuildingType.NONE) || (eReplacedBuilding == pLoopBuilding.getType()))
                                {
                                    TileServer pLoopTile = pLoopBuilding.tileServer();

                                    if ((pReplacedTile == null) || Utils.adjacentTiles(pReplacedTile, pLoopTile))
                                    {
                                        int iOldConnections = pLoopTile.countConnections(pLoopBuilding.getType(), getPlayer(), true, false);
                                        int iOldRevenue = gameServer().calculateRevenue(pLoopBuilding.getType(), pLoopTile, getPlayer(), iOldConnections, true, true, 50, 50, 100, false);

                                        if (infos().building(pLoopBuilding.getType()).miEntertainment > 0)
                                        {
                                            iOldRevenue += (AI_rivalEntertainmentValue() / 2);
                                        }

                                        if ((iOldRevenue <= 0) || pLoopTile.isConnectedToHQ() || Utils.isBuildingYield(pLoopBuilding.getType(), infos().Globals.ENERGY_RESOURCE, gameServer()))
                                        {
                                            TileServer pNewTile = null;
                                            BuildingType eNewBuilding = BuildingType.NONE;

                                            if (bHasReturnSabotage && canReturnClaim(pLoopTile, true))
                                            {
                                                int iBestResourceValue = 0;

                                                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                                {
                                                    TileServer pResourceTile = null;
                                                    BuildingType eResourceBuilding = BuildingType.NONE;
                                                    int iResourceValue = 0;

                                                    AI_bestResourceConstruct(eLoopResource, 100, ref pResourceTile, ref eResourceBuilding, ref iResourceValue, null, false);

                                                    if (iResourceValue > iBestResourceValue)
                                                    {
                                                        pNewTile = pResourceTile;
                                                        eNewBuilding = eResourceBuilding;
                                                        iBestResourceValue = iResourceValue;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                pNewTile = pLoopTile;
                                                eNewBuilding = AI_doConstructBestBuildingTile(0, 100, pLoopTile, eConstructedBuilding, true);
                                            }

                                            if ((eNewBuilding != BuildingType.NONE) && (pNewTile != null) && ((pNewTile != pLoopTile) ? true : (eNewBuilding != pLoopBuilding.getType())))
                                            {
                                                int iNewRevenue = gameServer().calculateRevenue(eNewBuilding, pNewTile, getPlayer(), pNewTile.countConnections(eNewBuilding, getPlayer(), true, false), true, true, 100, 100, 100, false);

                                                if (infos().building(eNewBuilding).miEntertainment > 0)
                                                {
                                                    iNewRevenue += (AI_rivalEntertainmentValue() / 2);
                                                }

                                                if (iNewRevenue > 0)
                                                {
                                                    int iValue = (iNewRevenue - iOldRevenue);

                                                    if (iOldRevenue < 10)
                                                    {
                                                        iValue -= (iOldRevenue - 10);
                                                    }
                                                    else if (iOldRevenue > 20)
                                                    {
                                                        iValue -= ((iOldRevenue - 20) / 2);
                                                    }

                                                    if (getHQLevel() < (infos().HQLevelsNum() - 1))
                                                    {
                                                        for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                                        {
                                                            if ((eLoopResource == infos().HQ(getHQ()).meBaseResource) ||
                                                                (gameServer().isResourceInputPlayer(infos().HQ(getHQ()).meBaseResource, eLoopResource, getPlayer())))
                                                            {
                                                                if (Utils.isBuildingYield(pLoopBuilding.getType(), eLoopResource, gameServer()))
                                                                {
                                                                    iValue = Math.Max(0, (iValue - (infos().resource(eLoopResource).miMarketPrice / 2)));
                                                                }
                                                            }
                                                        }
                                                    }

                                                    if (iValue > 0)
                                                    {
                                                        if (infos().building(eNewBuilding).miEntertainment > 0)
                                                        {
                                                            if (!(pNewTile.adjacentToModule()))
                                                            {
                                                                iValue /= 2;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (iOldConnections == 0)
                                                            {
                                                                iValue *= 2;
                                                            }
                                                        }

                                                        if (iValue > ((!(pNewTile.isConnectedToHQ()) && !isTeleportation() && gameServer().buildingWantsAnyInput(eNewBuilding, pNewTile, this)) ?
                                                                        (infos().personality(getPersonality()).miReplaceShippingValue) :
                                                                        (infos().personality(getPersonality()).miReplaceNormalValue)))
                                                        {
                                                            if (iValue > iBestValue)
                                                            {
                                                                pBestTile = pLoopTile;
                                                                iBestValue = iValue;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (pBestTile != null)
                    {
                        if (bHasReturnSabotage && canReturnClaim(pBestTile, true))
                        {
                            BuildingType eOldBuilding = pBestTile.getBuildingType();

                            if (AI_LOGGING) Debug.Log("SCRAPPED and RETURNED " + TEXT(infos().building(eOldBuilding).meName) + " at " + pBestTile.getX() + ", " + pBestTile.getY() + " [" + getRealName() + "]");

                            pBestTile.buildingServer().scrap();

                            AI_returnClaimSabotage(pBestTile);

                            AI_changeForceConstruct(1);

                            if (infos().building(eOldBuilding).miEntertainment == 0)
                            {
                                eReplacedBuilding = eOldBuilding;
                                pReplacedTile = pBestTile;
                                iReplacedCount++;

                                bContinue = true;
                            }
                        }
                        else
                        {
                            BuildingType eOldBuilding = pBestTile.getBuildingType();
                            BuildingType eNewBuilding = AI_doConstructBestBuildingTile(0, 100, pBestTile, eConstructedBuilding, false);

                            if (eNewBuilding != BuildingType.NONE)
                            {
                                if (AI_LOGGING) Debug.Log("---->(REPLACED)");

                                if ((infos().building(eOldBuilding).miEntertainment == 0) && (infos().building(eNewBuilding).miEntertainment == 0))
                                {
                                    eReplacedBuilding = eOldBuilding;
                                    eConstructedBuilding = eNewBuilding;
                                    pReplacedTile = pBestTile;
                                    iReplacedCount++;

                                    bContinue = true;
                                }
                            }
                        }
                    }

                    if (!bContinue)
                    {
                        break;
                    }
                }

                return (iReplacedCount > 0);
            }
        }

        protected virtual bool AI_canConstructOrder(OrderType eOrder)
        {
            for (BuildingType eLoopBuilding = 0; eLoopBuilding < infos().buildingsNum(); eLoopBuilding++)
            {
                if (infos().buildingClass(infos().building(eLoopBuilding).meClass).meOrderType == eOrder)
                {
                    if (canConstructPlayer(eLoopBuilding, false))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected virtual bool AI_doConstructOrderAll()
        {
            using (new UnityProfileScope("Player::AI_doConstructOrderAll"))
            {
                if (gameServer().isLastDay())
                {
                    return false;
                }

                if (getHQLevelInt() < 2)
                {
                    return false;
                }

                if (AI_vulnerableMajorityBuyout())
                {
                    return false;
                }

                if (AI_doConstructOrder(OrderType.LAUNCH, (getNumBuildings() / 7), 25))
                {
                    return true;
                }

                if (!isWinEligible())
                {
                    return false;
                }

                if (getRealOrderBuildingCount() < (getHQLevelInt() - ((gameServer().getNumPlayers() == (PlayerType)2) ? 3 : 2)))
                {
                    {
                        OrderType eBestOrder = OrderType.NONE;
                        int iBestValue = 0;

                        for (OrderType eLoopOrder = 0; eLoopOrder < infos().ordersNum(); eLoopOrder++)
                        {
                            if (AI_canConstructOrder(eLoopOrder) && AI_constructOrderValid(eLoopOrder, 1))
                            {
                                int iValue = 0;

                                switch (eLoopOrder)
                                {
                                    case OrderType.PATENT:
                                        {
                                            PatentType eBestPatent = PatentType.NONE;
                                            int iBestPatentValue = 0;

                                            AI_bestPatent(MIN_PRICE_MULTIPLIER_PATENT, ref eBestPatent, ref iBestPatentValue, 100, false, true);

                                            iValue += (iBestPatentValue / NUM_TURNS);
                                        }
                                        break;

                                    case OrderType.RESEARCH:
                                        {
                                            TechnologyType eBestTechnology = TechnologyType.NONE;
                                            TechnologyLevelType eBestTechnologyLevel = TechnologyLevelType.NONE;
                                            int iBestResearchValue = 0;

                                            AI_bestResearch(MIN_PRICE_MULTIPLIER_RESEARCH, ref eBestTechnology, ref eBestTechnologyLevel, ref iBestResearchValue, false, true);

                                            iValue += (iBestResearchValue / NUM_TURNS);
                                        }
                                        break;

                                    case OrderType.HACK:
                                        {
                                            EspionageType eBestEspionage = EspionageType.NONE;
                                            int iBestEspionageValue = 0;

                                            AI_bestEspionage(MIN_PRICE_MULTIPLIER_ESPIONAGE, ref eBestEspionage, ref iBestEspionageValue, false, true);

                                            iValue += (iBestEspionageValue / NUM_TURNS);
                                        }
                                        break;

                                    case OrderType.LAUNCH:
                                        iValue += (getBestLaunchProfit() / (getOrderTurns(OrderType.LAUNCH, infos().Globals.LAUNCH_TIME, null) * 4));
                                        break;
                                }

                                if (iValue > 0)
                                {
                                    switch (eLoopOrder)
                                    {
                                        case OrderType.PATENT:
                                            iValue += (gameServer().countPatentsAvailable() * 10);
                                            break;

                                        case OrderType.RESEARCH:
                                            iValue += ((getNumBuildings() + getHQLevelInt()) * 5);
                                            break;

                                        case OrderType.HACK:
                                            iValue += (gameServer().marketServer().getHighestWholePrice() / 5);
                                            break;

                                        case OrderType.LAUNCH:
                                            iValue += ((gameServer().countRealBuildingOrderCount(OrderType.LAUNCH) * 50) / (int)(gameServer().getNumPlayers()));
                                            break;
                                    }

                                    if (eLoopOrder != OrderType.LAUNCH)
                                    {
                                        if (gameServer().countRealBuildingOrderCount(eLoopOrder) == 0)
                                        {
                                            iValue *= 2;
                                        }
                                    }

                                    iValue += gameServer().random().Next(100);

                                    if (iValue > iBestValue)
                                    {
                                        eBestOrder = eLoopOrder;
                                        iBestValue = iValue;
                                    }
                                }
                            }
                        }

                        if (eBestOrder != OrderType.NONE)
                        {
                            if (AI_doConstructOrder(eBestOrder, 1, 25))
                            {
                                return true;
                            }
                        }
                    }

                    for (OrderType eLoopOrder = 0; eLoopOrder < infos().ordersNum(); eLoopOrder++)
                    {
                        if (gameServer().random().Next(infos().personality(getPersonality()).maiOrderConstructRoll[(int)eLoopOrder]) == 0)
                        {
                            if (AI_doConstructOrder(eLoopOrder, 1, 100))
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }
        }

        public virtual bool AI_saveForUpgrade()
        {
            if (getClaims() > 0)
            {
                return false;
            }

            if (canUpgrade(false))
            {
                return true;
            }

            if (countOrders(OrderType.LAUNCH) == 0)
            {
                return true;
            }

            return false;
        }

        protected virtual int AI_saveForUpgradeResource(ResourceType eResource)
        {
            if (!AI_saveForUpgrade())
            {
                return 0;
            }

            if (canUpgrade(false))
            {
                return getUpgradeResourceCost(eResource, getHQLevel());
            }

            {
                BuildingType eBestBuilding = BuildingType.NONE;
                int iBestValue = int.MaxValue;

                for (BuildingType eLoopBuilding = 0; eLoopBuilding < infos().buildingsNum(); eLoopBuilding++)
                {
                    if (infos().buildingClass(infos().building(eLoopBuilding).meClass).meOrderType == OrderType.LAUNCH)
                    {
                        if (AI_wantLaunchBuilding(eLoopBuilding))
                        {
                            if (canConstructPlayer(eLoopBuilding, false))
                            {
                                int iValue = infos().building(eLoopBuilding).miBaseCost;
                                if (iValue < iBestValue)
                                {
                                    eBestBuilding = eLoopBuilding;
                                    iBestValue = iValue;
                                }
                            }
                        }
                    }
                }

                if (eBestBuilding != BuildingType.NONE)
                {
                    return getBuildingResourceCost(eBestBuilding, eResource);
                }
            }

            return 0;
        }

        public virtual bool AI_fundResources(int iCost, int iMinPriceMultiplier, bool bSaveUpgrade, bool bTest, List<int> aiResources, List<int> aiExtraResources, bool bSellShares, PlayerType eSkipPlayer)
        {
            using (new UnityProfileScope("Player::AI_fundResources"))
            {
                return (AI_fundResourcesPercent(iCost, iMinPriceMultiplier, bSaveUpgrade, bTest, aiResources, aiExtraResources, bSellShares, eSkipPlayer) >= 100);
            }
        }

        public virtual int AI_fundResourcesPercent(int iCost, int iMinPriceMultiplier, bool bSaveUpgrade, bool bTest, List<int> aiResources, List<int> aiExtraResources, bool bSellShares, PlayerType eSkipPlayer)
        {
            if (aiResources != null)
            {
                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    iCost += getNeededResourceCost(eLoopResource, (aiResources[(int)eLoopResource] + ((aiExtraResources != null) ? aiExtraResources[(int)eLoopResource] : 0)));
                }
            }

            if (iCost == 0)
            {
                return 100;
            }

            if (getMoney() >= iCost)
            {
                return 100;
            }

            int iProfit = 0;

            List<int> tradeQuantities = Enumerable.Repeat(0, (int)infos().resourcesNum()).ToList();
            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                if (infos().resource(eLoopResource).mbTrade)
                {
                    int iQuantity = getWholeResourceStockpile(eLoopResource, true);
                    if (iQuantity == 0)
                        continue;

                    int iSaved = 0;
                    int iUpgrade = 0;

                    iSaved += ((aiResources != null) ? aiResources[(int)eLoopResource] : 0);
                    iSaved += ((aiExtraResources != null) ? aiExtraResources[(int)eLoopResource] : 0);

                    if (bSaveUpgrade)
                    {
                        iUpgrade = AI_saveForUpgradeResource(eLoopResource);
                        iSaved = Math.Max(iSaved, iUpgrade);
                    }

                    if(iSaved == 0 && maiResourceValue[(int)eLoopResource] >= 0)
                    {
                        iProfit += maiResourceValue[(int)eLoopResource];
                        tradeQuantities[(int)eLoopResource] = iQuantity;
                        continue;
                    }

                    iQuantity -= iSaved;

                    if (iQuantity > 0)
                    {
                        tradeQuantities[(int)eLoopResource] = iQuantity;

                        if (iSaved == iUpgrade && maiResourceValueSaveUpgrade[(int)eLoopResource] >= 0)
                        {
                            iProfit += maiResourceValueSaveUpgrade[(int)eLoopResource];
                            continue;
                        }

                        int iResourceProfit = gameServer().marketServer().calculateSellRevenue(eLoopResource, tradeQuantities[(int)eLoopResource], ((infos().resource(eLoopResource).miMarketPrice * iMinPriceMultiplier) / 100));
                        iProfit += iResourceProfit;

                        if (bSaveUpgrade && iUpgrade == iSaved)
                        {
                            maiResourceValueSaveUpgrade[(int)eLoopResource] = iResourceProfit;
                        }
                        else if(iSaved == 0)
                        {
                            maiResourceValue[(int)eLoopResource] = iResourceProfit;
                        }
                    }
                }
            }

            bool bNeedSellShares = false;

            if (bSellShares)
            {
                if ((getMoney() + iProfit) < iCost)
                {
                    int iStockProfit = AI_fundStock(true, eSkipPlayer);
                    if (iStockProfit > 0)
                    {
                        iProfit += iStockProfit;
                        bNeedSellShares = true;
                    }
                }
            }

            int iPercent = (int)Math.Min(100, (((getMoney() + iProfit) * 100) / iCost));

            if (iPercent < 100)
            {
                return iPercent;
            }

            if (bTest)
            {
                return 100;
            }

            while (true)
            {
                if (getMoney() >= iCost)
                {
                    return 100;
                }

                long iOldMoney = getMoney();

                if (bNeedSellShares)
                {
                    AI_fundStock(false, eSkipPlayer);
                }

                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    if (infos().resource(eLoopResource).mbTrade)
                    {
                        if (gameServer().marketServer().getWholePrice(eLoopResource) >= ((infos().resource(eLoopResource).miMarketPrice * iMinPriceMultiplier) / 100))
                        {
                            int iTotalProfit = 0;
                            if (tradeQuantities[(int)eLoopResource] > 0)
                            {
                                iTotalProfit += trade(eLoopResource, -(Math.Min(Constants.TRADE_QUANTITY, Math.Max(0, tradeQuantities[(int)eLoopResource]))), false);
                                maiResourceValue[(int)eLoopResource] -= iTotalProfit;
                                maiResourceValueSaveUpgrade[(int)eLoopResource] -= iTotalProfit;
                            }
                        }
                    }
                }

                if (iOldMoney == getMoney())
                {
                    if (AI_LOGGING) Debug.Log("AI_Fund FAIL!!!! Cost: " + iCost + ", Old Money: " + iOldMoney + ", New Money: " + getMoney());

                    break;
                }
            }

            return iPercent;
        }

        protected virtual int AI_fundStock(bool bTest, PlayerType eSkipPlayer)
        {
            if (maiStockValueSorted[0] == 0)
            {
                return 0;
            }
            else if (maiStockValueSorted[0] < 0)
            {
                PlayerType eBestPlayer = PlayerType.NONE;
                int iBestValue = 0;

                for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameServer().getNumPlayers(); eLoopPlayer++)
                {
                    if (eLoopPlayer != eSkipPlayer)
                    {
                        if (gameServer().playerServer(eLoopPlayer).getTeam() != getTeam())
                        {
                            if (canSellShares(eLoopPlayer))
                            {
                                int iValue = getSellSharePrice(eLoopPlayer);
                                maiStockValueSorted[(int)eLoopPlayer] = iValue;
                                if (iValue > iBestValue)
                                {
                                    eBestPlayer = eLoopPlayer;
                                    iBestValue = iValue;
                                }
                                continue;
                            }
                        }
                    }
                    maiStockValueSorted[(int)eLoopPlayer] = 0;
                }

                var sortedList = maiStockValueSorted.Select((value, index) => new { Value = value, Index = (PlayerType)index }).OrderByDescending(x => x.Value);
                maiStockValueSorted = sortedList.Select(x => x.Value).ToList();
                maiStockValueSortedIndex = sortedList.Select(x => x.Index).ToList();

                if (eBestPlayer != PlayerType.NONE)
                {
                    int iProfit = getSellSharePrice(eBestPlayer);

                    if (!bTest)
                    {
                        sellShares(eBestPlayer);
                        maiStockValueSorted[0] = -1;
                    }

                    return iProfit;
                }
                
                if (eSkipPlayer == getPlayer())
                {
                    return 0;
                }

                if (countOwnSharesOwned() <= gameServer().getMajorityShares())
                {
                    return 0;
                }

                if (canSellShares(getPlayer()))
                {
                    int iProfit = getSellSharePrice(getPlayer());

                    if (!bTest)
                    {
                        sellShares(getPlayer());
                        maiStockValueSorted[0] = -1;
                    }

                    return iProfit;
                }

                return 0;
            }
            else
            {
                for(int index = 0; index < (int)gameServer().getNumPlayers(); index++)
                {
                    if(maiStockValueSortedIndex[index] != eSkipPlayer)
                    {
                        if (!bTest)
                        {
                            sellShares(maiStockValueSortedIndex[index]);
                            maiStockValueSorted[0] = -1;
                        }
                        return maiStockValueSorted[index];
                    }
                }
            }
            return 0;
        }

        protected virtual bool AI_constructFund(TileServer pTile, BuildingType eBuilding, int iMinPriceMultiplier, bool bTest, List<int> aiExtraResources)
        {
            using (new UnityProfileScope("Player::AI_constructFund"))
            {
                bool bValid = ((aiExtraResources == null) ? (getMoney() >= getBuildingMoneyCost(eBuilding)) : false);

                if (!bValid)
                {
                    if (AI_fundResources(0, iMinPriceMultiplier, true, bTest, getBuildingResourceCost(eBuilding), aiExtraResources, false, PlayerType.NONE))
                    {
                        bValid = true;
                    }
                }

                if (bValid)
                {
                    if (bTest)
                    {
                        return true;
                    }
                    else
                    {
                        BuildingServer pOldBuilding = pTile.buildingServer();

                        if (pOldBuilding != null)
                        {
                            pOldBuilding.scrap();

                            if (AI_LOGGING) Debug.Log("**Scrapped a " + TEXT(infos().building(pOldBuilding.getType()).meName) + " to build a " + TEXT(infos().building(eBuilding).meName));
                        }

                        if (construct(pTile, eBuilding))
                        {
                            return true;
                        }
                        else
                        {
                            if (AI_LOGGING) Debug.Log("### AI_construct() failed (canConstruct() returned false) " + TEXT(infos().building(eBuilding).meName) + " at " + pTile.getX() + ", " + pTile.getY() + " [" + getRealName() + "]");

                            return false;
                        }
                    }
                }

                return false;
            }
        }

        protected virtual bool AI_doConstruct(int iRoll)
        {
            using (new UnityProfileScope("Player::AI_doConstruct"))
            {
                if (!AI_tilesAvailable())
                {
                    if (gameServer().random().Next(iRoll) == 0)
                    {
                        if (AI_doConstructReplace())
                        {
                            return true;
                        }
                    }

                    if (gameServer().random().Next(iRoll) == 0)
                    {
                        if (AI_doConstructOrderAll())
                        {
                            if (AI_LOGGING) Debug.Log("---->(because no tiles available)");

                            return true;
                        }
                    }

                    return false;
                }

                if (gameServer().random().Next(iRoll) == 0)
                {
                    if (AI_doConstructEmpty())
                    {
                        return true;
                    }
                }

                if ((getHQLevelInt() == 1) || (gameServer().random().Next(iRoll) == 0))
                {
                    if (AI_doConstructBaseResource(30))
                    {
                        return true;
                    }

                    if (AI_doConstructUpgradeResource(30))
                    {
                        return true;
                    }
                }

                if (getEnergyResource() == getGasResource())
                {
                    if (gameServer().random().Next(iRoll) == 0)
                    {
                        if (AI_doConstructPowerResource(25))
                        {
                            if (AI_LOGGING) Debug.Log("---->(because Robotic)");

                            return true;
                        }
                    }
                }

                if (gameServer().random().Next(iRoll) == 0)
                {
                    if (AI_doConstructMissing())
                    {
                        return true;
                    }
                }

                if (getClaims() == 1)
                {
                    if (gameServer().random().Next(iRoll) == 0)
                    {
                        if (AI_doConstructEntertainment(25))
                        {
                            if (AI_LOGGING) Debug.Log("---->(because of 1 Claim)");

                            return true;
                        }

                        if (AI_doConstructOrderAll())
                        {
                            if (AI_LOGGING) Debug.Log("---->(because of 1 Claim)");

                            return true;
                        }

                        if (AI_doConstructPowerResource(25))
                        {
                            if (AI_LOGGING) Debug.Log("---->(because of 1 Claim)");

                            return true;
                        }

                        if (AI_doConstructBestBuilding(40, 75))
                        {
                            if (AI_LOGGING) Debug.Log("---->(because of 1 Claim)");

                            return true;
                        }
                    }
                }

                if (gameServer().random().Next(iRoll) == 0)
                {
                    if (gameServer().random().Next(2) == 0)
                    {
                        if (AI_doConstructNeeds())
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (AI_doConstructWants())
                        {
                            return true;
                        }
                    }
                }

                if (gameServer().isCampaign())
                {
                    if (gameServer().random().Next(iRoll) == 0)
                    {
                        if (AI_doConstructBestBuilding(50, 50))
                        {
                            return true;
                        }
                    }
                }

                if ((getHQLevelInt() == 1) && (gameServer().random().Next(iRoll) == 0))
                {
                    if (AI_doConstructBaseResource(10, false))
                    {
                        return true;
                    }

                    if (AI_doConstructBestBuilding(0, 10))
                    {
                        return true;
                    }
                }

                if (iRoll == 1)
                {
                    if (AI_LOGGING) Debug.Log(getRealName() + " skipped a build.");
                }

                return false;
            }
        }

        bool mbForceCheckUpgrade = false;

        public void AI_setForceCheckUpgrade(bool bForce)
        {
            mbForceCheckUpgrade = bForce;
        }

        public virtual bool AI_doUpgradeHQ(bool bForce)
        {
            using (new UnityProfileScope("Player::AI_doUpgrade"))
            {
                if (isHuman())
                {
                    return false;
                }

                if (AI_vulnerableMajorityBuyout())
                {
                    if (getHQLevelInt() > 1)
                    {
                        return false;
                    }
                }

                if (!bForce && !mbForceCheckUpgrade)
                {
                    if (gameServer().random().Next(infos().personality(getPersonality()).miUpgradeRoll) != 0)
                    {
                        return false;
                    }
                }

                if (getClaims() > 0)
                {
                    return false;
                }

                if (getHQLevelInt() == 1 && gameServer().getHumanHandicap() < infos().Globals.DEFAULT_HANDICAP && 
                    (getNumConstructions() > 0 || getUnitList().Select(id => gameServer().unitServer(id)).Where(unit => unit.getMissionInfo().meMission == MissionType.CONSTRUCT).Count() > 0)) //delay first upgrade until all buildings are complete for lower handicaps
                {
                    return false;
                }

                bool bUpgraded = false;

                if (canUpgrade(true))
                {
                    upgrade();

                    bUpgraded = true;
                }
                else if (canUpgrade(false))
                {
                    List<int> aiResources = new List<int>();

                    for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                    {
                        aiResources.Add(getUpgradeResourceCost(eLoopResource, getHQLevel()));
                    }

                    if (AI_fundResources(0, Math.Min(200, ((getHQLevelInt() + 1) * 20)), false, false, aiResources, null, true, PlayerType.NONE))
                    {
                        upgrade();

                        bUpgraded = true;
                    }
                }

                if (mbForceCheckUpgrade)
                    AI_setForceCheckUpgrade(false);

                if (bUpgraded && !isSubsidiary())
                {
                    AI_changeForceConstruct(2);

                    return true;
                }

                return false;
            }
        }

        public virtual void AI_doStock()
        {
            if (isHuman() || isSubsidiary())
            {
                return;
            }

            if (gameServer().getSevenSols() == SevenSolsType.COLONY)
            {
                AI_doStockSevenSolsColony();
            }
            else if (gameServer().getSevenSols() == SevenSolsType.WHOLESALE)
            {
                AI_doStockSevenSolsWholesale();
            }
            else
            {
                AI_doStockPlayers();
            }
        }

        protected virtual bool AI_vulnerableMajorityBuyout()
        {
            if (isSubsidiary())
            {
                return false;
            }

            if (gameServer().isSevenSols())
            {
                return false;
            }

            int iOwnShares = countOwnSharesOwned();

            if (iOwnShares >= gameServer().getMajorityShares())
            {
                return false;
            }

            if (iOwnShares < Math.Max(0, (getHQLevelInt() - 1)))
            {
                return true;
            }

            if (getSharesAvailable() <= 2)
            {
                return true;
            }

            foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAliveAll())
            {
                int iThreshold = (getSharesAvailable() * 2);

                if (getSharesBoughtRivals() < iOwnShares)
                {
                    iThreshold += 40;
                }

                if (pLoopPlayer.getBuyoutPercent(getPlayer()) > iThreshold)
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual PlayerType AI_getBestBuyoutTarget(int iMinShares, int iMinPercent)
        {
            if (isHumanTeam())
            {
                return PlayerType.NONE;
            }

            PlayerType eBestPlayer = PlayerType.NONE;
            int iBestValue = 0;

            foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAliveAll())
            {
                if (pLoopPlayer.getTeam() != getTeam())
                {
                    int iShares = getSharesOwned(pLoopPlayer.getPlayer());
                    int iPercent = getBuyoutPercent(pLoopPlayer.getPlayer());

                    if ((iShares >= iMinShares) || (iPercent >= iMinPercent))
                    {
                        int iValue = ((iShares + 1) * (iPercent + 1));
                        if (iValue > iBestValue)
                        {
                            eBestPlayer = pLoopPlayer.getPlayer();
                            iBestValue = iValue;
                        }
                    }
                }
            }

            return eBestPlayer;
        }

        protected virtual void AI_doStockPlayers()
        {
            using (new UnityProfileScope("Player::AI_doStockPlayers"))
            {
                if (getHQLevelInt() == 1)
                {
                    return;
                }

                if (canBuyShares(getPlayer(), false))
                {
                    if (AI_vulnerableMajorityBuyout())
                    {
                        int iPrice = getBuySharePrice(getPlayer());

                        if (AI_fundResources(iPrice, 1, false, false, null, null, true, getPlayer()))
                        {
                            buyShares(getPlayer());
                            return;
                        }
                    }
                }

                if (getHQLevelInt() == 2)
                {
                    if (getClaims() > 0)
                    {
                        return;
                    }
                }

                if (gameServer().random().Next(infos().handicap(gameServer().getHumanHandicap()).miAIStockRoll) != 0)
                {
                    return;
                }

                if (getCashResourceValue() < gameServer().random().Next(infos().personality(getPersonality()).miStockThreshold))
                {
                    if (gameServer().random().Next(10) != 0)
                    {
                        return;
                    }
                }

                if (gameServer().isTeamGame())
                {
                    foreach (PlayerServer pTeamPlayer in getAliveTeammatesAll())
                    {
                        if (pTeamPlayer != this)
                        {
                            if (canBuyShares(pTeamPlayer.getPlayer(), false))
                            {
                                if (pTeamPlayer.AI_vulnerableMajorityBuyout())
                                {
                                    int iPrice = getBuySharePrice(pTeamPlayer.getPlayer());

                                    if (AI_fundResources(iPrice, 1, false, false, null, null, true, pTeamPlayer.getPlayer()))
                                    {
                                        buyShares(pTeamPlayer.getPlayer());
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }

                if (getHQLevelInt() == 2)
                {
                    return;
                }

                if (canBuyShares(getPlayer(), false))
                {
                    if (countOwnSharesOwned() < gameServer().getMajorityShares())
                    {
                        if (getSharesBoughtRivals() > Math.Max(0, (countOwnSharesOwned() - 3)))
                        {
                            int iPrice = getBuySharePrice(getPlayer());

                            if (AI_fundResources(iPrice, 10, false, false, null, null, false, PlayerType.NONE))
                            {
                                buyShares(getPlayer());
                                return;
                            }
                        }
                    }
                }

                if (isWinEligible())
                {
                    const int MIN_PRICE_MULTIPLIER = 1;

                    PlayerType eBestPlayer = PlayerType.NONE;
                    bool bBestSaveUpgrade = false;
                    int iBestValue = int.MaxValue;

                    foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAliveAll())
                    {
                        if (pLoopPlayer.getTeam() != getTeam())
                        {
                            if (canBuyShares(pLoopPlayer.getPlayer(), false))
                            {
                                int iMajoritySharesNeeded = ((gameServer().getMajorityShares() + 1) - pLoopPlayer.getSharesBoughtRivals());

                                if ((iMajoritySharesNeeded > 0) && (iMajoritySharesNeeded <= pLoopPlayer.getSharesAvailable()))
                                {
                                    int iTargetMoney = (getBuySharePrice(pLoopPlayer.getPlayer()) * iMajoritySharesNeeded);
                                    bool bSaveUpgrade = (iMajoritySharesNeeded > 1);

                                    if (AI_fundResources(iTargetMoney, MIN_PRICE_MULTIPLIER, bSaveUpgrade, true, null, null, !bSaveUpgrade, pLoopPlayer.getPlayer()))
                                    {
                                        int iValue = pLoopPlayer.getSharePrice();

                                        iValue += gameServer().random().Next(100);

                                        if (iValue < iBestValue)
                                        {
                                            eBestPlayer = pLoopPlayer.getPlayer();
                                            bBestSaveUpgrade = bSaveUpgrade;
                                            iBestValue = iValue;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (eBestPlayer != PlayerType.NONE)
                    {
                        AI_fundResources(getBuySharePrice(eBestPlayer), MIN_PRICE_MULTIPLIER, bBestSaveUpgrade, false, null, null, !bBestSaveUpgrade, eBestPlayer);
                        buyShares(eBestPlayer);
                        return;
                    }
                }

                if (isWinEligible() && !isHumanTeam())
                {
                    const int MIN_PRICE_MULTIPLIER = 1;

                    PlayerType eBestPlayer = PlayerType.NONE;
                    long iBestValue = 0;

                    foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAliveAll())
                    {
                        if (pLoopPlayer.getTeam() != getTeam())
                        {
                            if (canBuyout(pLoopPlayer.getPlayer(), false))
                            {
                                if (canOnlyBuyout(pLoopPlayer.getPlayer()))
                                {
                                    if (AI_fundResources(getTotalBuyoutPrice(pLoopPlayer.getPlayer()), MIN_PRICE_MULTIPLIER, false, true, null, null, true, pLoopPlayer.getPlayer()))
                                    {
                                        long iValue = gameServer().playerServer(pLoopPlayer.getPlayer()).calculateTotalStockValue(0);

                                        iValue *= (100 + gameServer().random().Next(50));
                                        iValue /= (100);

                                        if (iValue > iBestValue)
                                        {
                                            eBestPlayer = pLoopPlayer.getPlayer();
                                            iBestValue = iValue;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (eBestPlayer != PlayerType.NONE)
                    {
                        AI_fundResources(getTotalBuyoutPrice(eBestPlayer), MIN_PRICE_MULTIPLIER, false, false, null, null, true, eBestPlayer);
                        buyout(eBestPlayer);
                        return;
                    }
                }

                if (isWinEligible())
                {
                    const int MIN_PRICE_MULTIPLIER = 10;

                    PlayerType eBestPlayer = PlayerType.NONE;
                    long iBestValue = 0;

                    foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAliveAll())
                    {
                        if (pLoopPlayer.getTeam() != getTeam())
                        {
                            if (canBuyout(pLoopPlayer.getPlayer(), false))
                            {
                                if (!canOnlyBuyout(pLoopPlayer.getPlayer()))
                                {
                                    if (AI_fundResources(getTotalBuyoutPrice(pLoopPlayer.getPlayer()), MIN_PRICE_MULTIPLIER, true, true, null, null, false, PlayerType.NONE))
                                    {
                                        long iValue = gameServer().playerServer(pLoopPlayer.getPlayer()).calculateTotalStockValue(0);

                                        iValue *= (100 + gameServer().random().Next(50));
                                        iValue /= (100);

                                        if (iValue > iBestValue)
                                        {
                                            eBestPlayer = pLoopPlayer.getPlayer();
                                            iBestValue = iValue;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (eBestPlayer != PlayerType.NONE)
                    {
                        AI_fundResources(getBuySharePrice(eBestPlayer), MIN_PRICE_MULTIPLIER, true, false, null, null, false, PlayerType.NONE);
                        buyShares(eBestPlayer);
                        return;
                    }
                }

                if (getClaims() > 0)
                {
                    return;
                }

                if (canBuyShares(getPlayer(), false))
                {
                    if (getSharesBoughtRivals() >= (countOwnSharesOwned() / 2))
                    {
                        if (AI_fundResources(getBuySharePrice(getPlayer()), 10, true, false, null, null, false, PlayerType.NONE))
                        {
                            buyShares(getPlayer());
                            return;
                        }
                    }
                }

                if (isWinEligible())
                {
                    const int MIN_PRICE_MULTIPLIER = 20;

                    PlayerType eBestPlayer = PlayerType.NONE;
                    bool bBestSaveUpgrade = false;
                    int iBestValue = int.MaxValue;

                    foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAll())
                    {
                        if ((pLoopPlayer != this) && !(pLoopPlayer.isSubsidiary()))
                        {
                            if (canBuyShares(pLoopPlayer.getPlayer(), false))
                            {
                                int iBetterThreshold = 0;
                                int iWorseThreshold = 0;
                                int iExcessBond = 0;
                                BondType eBond = pLoopPlayer.calculateBondRating(pLoopPlayer.getDebt(), 0, ref iBetterThreshold, ref iWorseThreshold, ref iExcessBond, true);

                                if (eBond == (infos().bondsNum() - 1))
                                {
                                    bool bSaveUpgrade = (pLoopPlayer.countOwnSharesOwned() >= gameServer().getMajorityShares());

                                    if (AI_fundResources(getBuySharePrice(pLoopPlayer.getPlayer()), MIN_PRICE_MULTIPLIER, bSaveUpgrade, true, null, null, false, pLoopPlayer.getPlayer()))
                                    {
                                        int iValue = pLoopPlayer.getSharePrice();

                                        iValue += gameServer().random().Next(100);

                                        if (iValue < iBestValue)
                                        {
                                            eBestPlayer = pLoopPlayer.getPlayer();
                                            bBestSaveUpgrade = bSaveUpgrade;
                                            iBestValue = iValue;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (eBestPlayer != PlayerType.NONE)
                    {
                        AI_fundResources(getBuySharePrice(eBestPlayer), MIN_PRICE_MULTIPLIER, bBestSaveUpgrade, false, null, null, false, eBestPlayer);
                        buyShares(eBestPlayer);
                        return;
                    }
                }

                if (getHQLevelInt() == 3)
                {
                    return;
                }

                if (canBuyShares(getPlayer(), false))
                {
                    if ((getMoney() / 3) > getBuySharePrice(getPlayer()))
                    {
                        buyShares(getPlayer());
                        return;
                    }

                    if (countOwnSharesOwned() < gameServer().getMajorityShares())
                    {
                        if (AI_fundResources(getBuySharePrice(getPlayer()), 10, true, false, null, null, false, PlayerType.NONE))
                        {
                            buyShares(getPlayer());
                        }

                        return;
                    }
                    else
                    {
                        if (getSharesBoughtRivals() > (countOwnSharesOwned() - gameServer().getMajorityShares()))
                        {
                            if (AI_fundResources(getBuySharePrice(getPlayer()), 10, true, false, null, null, false, PlayerType.NONE))
                            {
                                buyShares(getPlayer());
                                return;
                            }
                        }
                    }

                    {
                        int iSharesOwnedOfOthers = 0;

                        foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAll())
                        {
                            if (pLoopPlayer != this)
                            {
                                iSharesOwnedOfOthers += getSharesOwned(pLoopPlayer.getPlayer());
                            }
                        }

                        if (countOwnSharesOwned() <= iSharesOwnedOfOthers)
                        {
                            if (AI_fundResources(getBuySharePrice(getPlayer()), 10, true, false, null, null, false, PlayerType.NONE))
                            {
                                buyShares(getPlayer());
                                return;
                            }
                        }
                    }
                }

                if (isWinEligible())
                {
                    PlayerType eBestTarget = AI_getBestBuyoutTarget(3, 50);

                    if (eBestTarget != PlayerType.NONE)
                    {
                        if (canOnlyBuyout(eBestTarget))
                        {
                            if (canBuyout(eBestTarget, true))
                            {
                                buyout(eBestTarget);
                                return;
                            }
                            else if (AI_fundResources(getTotalBuyoutPrice(eBestTarget), 1, false, false, null, null, true, eBestTarget))
                            {
                                buyout(eBestTarget);
                                return;
                            }
                        }
                        else
                        {
                            if (canBuyShares(eBestTarget, true))
                            {
                                buyShares(eBestTarget);
                                return;
                            }
                            else if (AI_fundResources(getBuySharePrice(eBestTarget), 10, true, false, null, null, false, PlayerType.NONE))
                            {
                                buyShares(eBestTarget);
                                return;
                            }
                        }
                    }
                    else
                    {
                        {
                            PlayerType eBestPlayer = PlayerType.NONE;
                            int iBestValue = int.MaxValue;

                            foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAll())
                            {
                                if ((pLoopPlayer != this) && !(pLoopPlayer.isSubsidiary()))
                                {
                                    if (canBuyShares(pLoopPlayer.getPlayer(), true))
                                    {
                                        if ((getMoney() / 2) > getBuySharePrice(pLoopPlayer.getPlayer()))
                                        {
                                            int iValue = pLoopPlayer.getSharePrice();

                                            iValue += gameServer().random().Next(100);

                                            if (iValue < iBestValue)
                                            {
                                                eBestPlayer = pLoopPlayer.getPlayer();
                                                iBestValue = iValue;
                                            }
                                        }
                                    }
                                }
                            }

                            if (eBestPlayer != PlayerType.NONE)
                            {
                                buyShares(eBestPlayer);
                                return;
                            }
                        }

                        {
                            const int MIN_PRICE_MULTIPLIER = 40;

                            PlayerType eBestPlayer = PlayerType.NONE;
                            int iBestValue = int.MaxValue;

                            foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAll())
                            {
                                if ((pLoopPlayer != this) && !(pLoopPlayer.isSubsidiary()))
                                {
                                    if (canBuyShares(pLoopPlayer.getPlayer(), false))
                                    {
                                        if (AI_fundResources(getBuySharePrice(pLoopPlayer.getPlayer()), MIN_PRICE_MULTIPLIER, true, true, null, null, false, PlayerType.NONE))
                                        {
                                            int iValue = pLoopPlayer.getSharePrice();

                                            iValue += gameServer().random().Next(100);

                                            if (iValue < iBestValue)
                                            {
                                                eBestPlayer = pLoopPlayer.getPlayer();
                                                iBestValue = iValue;
                                            }
                                        }
                                    }
                                }
                            }

                            if (eBestPlayer != PlayerType.NONE)
                            {
                                AI_fundResources(getBuySharePrice(eBestPlayer), MIN_PRICE_MULTIPLIER, true, false, null, null, false, PlayerType.NONE);
                                buyShares(eBestPlayer);
                                return;
                            }
                        }

                        {
                            const int MIN_PRICE_MULTIPLIER = 60;

                            PlayerType eBestPlayer = PlayerType.NONE;
                            int iBestValue = 0;

                            foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAll())
                            {
                                if ((pLoopPlayer != this) && pLoopPlayer.isSubsidiary())
                                {
                                    if (canBuyShares(pLoopPlayer.getPlayer(), false))
                                    {
                                        int iPrice = getBuySharePrice(pLoopPlayer.getPlayer());
                                        int iDividend = pLoopPlayer.getDividend(pLoopPlayer.getMoney(), 1);

                                        if (iPrice < (iDividend * 200))
                                        {
                                            if (AI_fundResources(iPrice, MIN_PRICE_MULTIPLIER, true, true, null, null, false, PlayerType.NONE))
                                            {
                                                int iValue = iDividend;

                                                iValue += gameServer().random().Next(10);

                                                if (iValue > iBestValue)
                                                {
                                                    eBestPlayer = pLoopPlayer.getPlayer();
                                                    iBestValue = iValue;
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (eBestPlayer != PlayerType.NONE)
                            {
                                AI_fundResources(getBuySharePrice(eBestPlayer), MIN_PRICE_MULTIPLIER, true, false, null, null, false, PlayerType.NONE);
                                buyShares(eBestPlayer);
                                return;
                            }
                        }
                    }
                }
            }
        }

        protected virtual void AI_doStockSevenSolsColony()
        {
            using (new UnityProfileScope("Player::AI_doStockColony"))
            {
                if (getHQLevelInt() < 2)
                {
                    return;
                }
                else if (getHQLevelInt() == 2)
                {
                    if (getClaims() > 0)
                    {
                        return;
                    }
                }

                if (gameServer().random().Next(infos().handicap(gameServer().getHumanHandicap()).miAIStockRoll) != 0)
                {
                    return;
                }

                if (getCashResourceValue() < gameServer().random().Next(infos().personality(getPersonality()).miStockThreshold))
                {
                    if (gameServer().random().Next((gameServer().isLastDay()) ? 5 : 20) != 0)
                    {
                        return;
                    }
                }

                bool bDoneGrowing = ((getHQLevel() == (infos().HQLevelsNum() - 1)) && (getClaims() == 0));

                if (bDoneGrowing)
                {
                    bool bCanBuild = false;
                    bool bHasBuilt = false;

                    for (BuildingType eLoopBuilding = 0; eLoopBuilding < infos().buildingsNum(); eLoopBuilding++)
                    {
                        if (infos().buildingClass(infos().building(eLoopBuilding).meClass).meOrderType == OrderType.LAUNCH)
                        {
                            if (canEverConstruct(eLoopBuilding, true, true))
                            {
                                bCanBuild = true;

                                if ((getRealConstructionCount(eLoopBuilding) + getRealBuildingCount(eLoopBuilding)) > 0)
                                {
                                    bHasBuilt = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (bCanBuild && !bHasBuilt)
                    {
                        bDoneGrowing = false;
                    }
                }

                bool bValidPopulation = false;
                bool bValidLabor = false;

                if (bDoneGrowing)
                {
                    foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAll())
                    {
                        if (pLoopPlayer.getTeam() != getTeam())
                        {
                            if (((pLoopPlayer.getColonySharesOwned() * 3) / 2) > getColonySharesOwned())
                            {
                                if (gameServer().isCampaign())
                                {
                                    if (gameServer().getLabor() > (gameServer().getMaxPopulation() / 2))
                                    {
                                        bValidPopulation = true;
                                    }
                                    if (gameServer().getMaxPopulation() > (gameServer().getLabor() / 2))
                                    {
                                        bValidLabor = true;
                                    }
                                }
                                else
                                {
                                    bValidPopulation = true;
                                    bValidLabor = true;
                                }

                                break;
                            }
                        }
                    }
                }

                if ((getHQLevel() == (infos().HQLevelsNum() - 1)) || (getHQLevelInt() > getColonySharesOwned()))
                {
                    if (gameServer().isCampaign())
                    {
                        if (gameServer().getLabor() > ((gameServer().getMaxPopulation() * 3) / 4))
                        {
                            bValidPopulation = true;
                        }
                        if (gameServer().getMaxPopulation() > ((gameServer().getLabor() * 3) / 4))
                        {
                            bValidLabor = true;
                        }
                    }
                    else
                    {
                        bValidPopulation = true;
                        bValidLabor = true;
                    }
                }

                if (bValidPopulation || bValidLabor)
                {
                    {
                        int MIN_PRICE_MULTIPLIER = Math.Max(1, (70 - (getHQLevelInt() * 10)));

                        ModuleType eBestModule = ModuleType.NONE;
                        int iBestValue = 0;

                        foreach (InfoModule pLoopModule in infos().modules())
                        {
                            if (canBuyColonyModule(pLoopModule.meType, false))
                            {
                                if (AI_fundResources(getModuleMoneyCost(pLoopModule.meType), MIN_PRICE_MULTIPLIER, true, true, pLoopModule.maiResourceCost, null, false, PlayerType.NONE))
                                {
                                    if ((bValidPopulation && pLoopModule.mbPopulation) ||
                                        (bValidLabor && pLoopModule.mbLabor))
                                    {
                                        int iValue = gameServer().random().Next(100) + 1;

                                        if (pLoopModule.mbPopulation)
                                        {
                                            if (gameServer().getPopulation() < gameServer().getLabor())
                                            {
                                                iValue *= 2;
                                            }
                                        }

                                        if (pLoopModule.mbLabor)
                                        {
                                            if (gameServer().getPopulation() < gameServer().getMaxPopulation())
                                            {
                                                iValue *= 2;
                                            }
                                        }

                                        if (iValue > iBestValue)
                                        {
                                            eBestModule = pLoopModule.meType;
                                            iBestValue = iValue;
                                        }
                                    }
                                }
                            }
                        }

                        if (eBestModule != ModuleType.NONE)
                        {
                            if (AI_fundResources(getModuleMoneyCost(eBestModule), MIN_PRICE_MULTIPLIER, true, false, infos().module(eBestModule).maiResourceCost, null, false, PlayerType.NONE))
                            {
                                buyColonyModule(eBestModule);
                                return;
                            }
                        }
                    }
                }

                if (getHQLevelInt() < 3)
                {
                    return;
                }

                bool bLosing = false;

                foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAll())
                {
                    if (pLoopPlayer.getTeam() != getTeam())
                    {
                        if (gameServer().isLastDay())
                        {
                            if (pLoopPlayer.getColonySharesOwned() >= getColonySharesOwned())
                            {
                                bLosing = true;
                            }
                        }
                        else
                        {
                            if (pLoopPlayer.getColonySharesOwned() > (getColonySharesOwned() + Math.Max(0, (gameServer().getLastDay() - gameServer().getDays()))))
                            {
                                bLosing = true;
                            }
                        }
                    }
                }

                if (gameServer().isLastDay() || bDoneGrowing || bLosing)
                {
                    const int MIN_PRICE_MULTIPLIER = 1;

                    ModuleType eBestModule = ModuleType.NONE;
                    int iBestValue = 0;

                    if (eBestModule == ModuleType.NONE)
                    {
                        foreach (InfoModule pLoopModule in infos().modules())
                        {
                            if (canBuyColonyModule(pLoopModule.meType, false))
                            {
                                if (AI_fundResources(getModuleMoneyCost(pLoopModule.meType), MIN_PRICE_MULTIPLIER, false, true, pLoopModule.maiResourceCost, null, false, PlayerType.NONE))
                                {
                                    if ((bValidPopulation && pLoopModule.mbPopulation) ||
                                        (bValidLabor && pLoopModule.mbLabor))
                                    {
                                        int iValue = gameServer().random().Next(100) + 1;
                                        if (iValue > iBestValue)
                                        {
                                            eBestModule = pLoopModule.meType;
                                            iBestValue = iValue;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (eBestModule == ModuleType.NONE)
                    {
                        if (bLosing)
                        {
                            foreach (InfoModule pLoopModule in infos().modules())
                            {
                                if (canBuyColonyModule(pLoopModule.meType, false))
                                {
                                    if (AI_fundResources(getModuleMoneyCost(pLoopModule.meType), MIN_PRICE_MULTIPLIER, false, true, pLoopModule.maiResourceCost, null, false, PlayerType.NONE))
                                    {
                                        int iValue = gameServer().random().Next(100) + 1;
                                        if (iValue > iBestValue)
                                        {
                                            eBestModule = pLoopModule.meType;
                                            iBestValue = iValue;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (eBestModule != ModuleType.NONE)
                    {
                        if (AI_fundResources(getModuleMoneyCost(eBestModule), MIN_PRICE_MULTIPLIER, false, false, infos().module(eBestModule).maiResourceCost, null, false, PlayerType.NONE))
                        {
                            buyColonyModule(eBestModule);
                            return;
                        }
                    }
                }

                if (getHQLevelInt() < 4)
                {
                    return;
                }

                if (canBuyColonyShares(true))
                {
                    bool bDanger = false;

                    foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAll())
                    {
                        if (pLoopPlayer.getTeam() != getTeam())
                        {
                            if (((pLoopPlayer.getColonySharesOwned() * 3) / 2) > getColonySharesOwned())
                            {
                                bDanger = true;
                                break;
                            }
                        }
                    }

                    if (bDanger)
                    {
                        buyColonyShares();
                    }
                }

                if (gameServer().isLastDay() && bLosing)
                {
                    if (canBuyColonyShares(false))
                    {
                        if (AI_fundResources(gameServer().getSharePrice(), 1, false, false, null, null, false, PlayerType.NONE))
                        {
                            buyColonyShares();
                        }
                    }
                }
            }
        }

        protected virtual void AI_doStockSevenSolsWholesale()
        {
            using (new UnityProfileScope("Player::AI_doStockSevenSolsWholesale"))
            {
                if (getHQLevelInt() < 2)
                {
                    return;
                }
                else if (getHQLevelInt() == 2)
                {
                    if (getClaims() > 0)
                    {
                        return;
                    }
                }

                if (gameServer().random().Next(infos().handicap(gameServer().getHumanHandicap()).miAIStockRoll) != 0)
                {
                    return;
                }

                if (getCashResourceValue() < gameServer().random().Next(infos().personality(getPersonality()).miStockThreshold))
                {
                    if (gameServer().random().Next(10) != 0)
                    {
                        return;
                    }
                }

                bool bDoneGrowing = ((getHQLevel() == (infos().HQLevelsNum() - 1)) && (getClaims() == 0));

                if (bDoneGrowing)
                {
                    bool bCanBuild = false;
                    bool bHasBuilt = false;

                    for (BuildingType eLoopBuilding = 0; eLoopBuilding < infos().buildingsNum(); eLoopBuilding++)
                    {
                        if (infos().buildingClass(infos().building(eLoopBuilding).meClass).meOrderType == OrderType.LAUNCH)
                        {
                            if (canEverConstruct(eLoopBuilding, true, true))
                            {
                                bCanBuild = true;

                                if ((getRealConstructionCount(eLoopBuilding) + getRealBuildingCount(eLoopBuilding)) > 0)
                                {
                                    bHasBuilt = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (bCanBuild && !bHasBuilt)
                    {
                        bDoneGrowing = false;
                    }
                }

                {
                    int MIN_PRICE_MULTIPLIER = Math.Max(1, (70 - (getHQLevelInt() * 10)));

                    int iBestSlot = -1;
                    int iBestValue = 0;

                    for (int iLoopSlot = 0; iLoopSlot < infos().Globals.NUM_WHOLESALE_SLOTS; iLoopSlot++)
                    {
                        if (canSupplyWholesale(iLoopSlot, false) && (gameServer().getWholesaleSlotDelay(iLoopSlot) == 0))
                        {
                            if (AI_fundResources(getWholesaleMoneyCost(iLoopSlot), MIN_PRICE_MULTIPLIER, true, true, gameServer().buildWholesaleSlotResourceCost(iLoopSlot), null, false, PlayerType.NONE))
                            {
                                int iValue = gameServer().random().Next(100) + 1;
                                if (iValue > iBestValue)
                                {
                                    iBestSlot = iLoopSlot;
                                    iBestValue = iValue;
                                }
                            }
                        }
                    }

                    if (iBestSlot != -1)
                    {
                        if (AI_fundResources(getWholesaleMoneyCost(iBestSlot), MIN_PRICE_MULTIPLIER, true, false, gameServer().buildWholesaleSlotResourceCost(iBestSlot), null, false, PlayerType.NONE))
                        {
                            supplyWholesale(iBestSlot);
                            return;
                        }
                    }
                }

                if (getHQLevelInt() < 3)
                {
                    return;
                }

                bool bLosing = false;

                foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAll())
                {
                    if (pLoopPlayer.getTeam() != getTeam())
                    {
                        if (gameServer().isLastDay())
                        {
                            if (pLoopPlayer.getColonySharesOwned() >= getColonySharesOwned())
                            {
                                bLosing = true;
                            }
                        }
                        else
                        {
                            if (pLoopPlayer.getColonySharesOwned() > (getColonySharesOwned() + Math.Max(0, (gameServer().getLastDay() - gameServer().getDays()))))
                            {
                                bLosing = true;
                            }
                        }
                    }
                }

                if (gameServer().isLastDay() || bDoneGrowing || bLosing)
                {
                    const int MIN_PRICE_MULTIPLIER = 1;

                    int iBestSlot = -1;
                    int iBestValue = 0;

                    for (int iLoopSlot = 0; iLoopSlot < infos().Globals.NUM_WHOLESALE_SLOTS; iLoopSlot++)
                    {
                        if (canSupplyWholesale(iLoopSlot, false) && (gameServer().getWholesaleSlotDelay(iLoopSlot) == 0))
                        {
                            if (AI_fundResources(getWholesaleMoneyCost(iLoopSlot), MIN_PRICE_MULTIPLIER, false, true, gameServer().buildWholesaleSlotResourceCost(iLoopSlot), null, false, PlayerType.NONE))
                            {
                                int iValue = gameServer().random().Next(100) + 1;
                                if (iValue > iBestValue)
                                {
                                    iBestSlot = iLoopSlot;
                                    iBestValue = iValue;
                                }
                            }
                        }
                    }

                    if (iBestSlot != -1)
                    {
                        if (AI_fundResources(getWholesaleMoneyCost(iBestSlot), MIN_PRICE_MULTIPLIER, false, false, gameServer().buildWholesaleSlotResourceCost(iBestSlot), null, false, PlayerType.NONE))
                        {
                            supplyWholesale(iBestSlot);
                            return;
                        }
                    }
                }

                if (getHQLevelInt() < 4)
                {
                    return;
                }

                if (canBuyColonyShares(true))
                {
                    bool bDanger = false;

                    foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAll())
                    {
                        if (pLoopPlayer.getTeam() != getTeam())
                        {
                            if (((pLoopPlayer.getColonySharesOwned() * 3) / 2) > getColonySharesOwned())
                            {
                                bDanger = true;
                                break;
                            }
                        }
                    }

                    if (bDanger)
                    {
                        buyColonyShares();
                    }
                }

                if (gameServer().isLastDay() && bLosing)
                {
                    if (canBuyColonyShares(false))
                    {
                        if (AI_fundResources(gameServer().getSharePrice(), 1, false, false, null, null, false, PlayerType.NONE))
                        {
                            buyColonyShares();
                        }
                    }
                }
            }
        }

        protected virtual void AI_doDebt()
        {
            using (new UnityProfileScope("Player::AI_doDebt"))
            {
                if (gameServer().random().Next(5) != 0)
                {
                    return;
                }

                if (!canPayDebt())
                {
                    return;
                }

                if (-(getDebt()) < infos().Globals.DEBT_PAYMENT)
                {
                    return;
                }

                if (getSharesAvailable() == 0)
                {
                    foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAliveAll())
                    {
                        if (pLoopPlayer.getTeam() != getTeam())
                        {
                            int iPercent = pLoopPlayer.getBuyoutPercent(getPlayer());
                            if (iPercent > 80)
                            {
                                while (AI_fundResources(infos().Globals.DEBT_PAYMENT, 10, false, false, null, null, false, PlayerType.NONE))
                                {
                                    if (canPayDebt())
                                    {
                                        payDebt();
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            else if (iPercent > 60)
                            {
                                if (AI_fundResources(infos().Globals.DEBT_PAYMENT, 10, true, false, null, null, false, PlayerType.NONE))
                                {
                                    payDebt();
                                }
                            }
                        }
                    }
                }

                if (infos().bond(getBondRating()).mbNoBlackMarketSabotage)
                {
                    if (AI_fundResources(infos().Globals.DEBT_PAYMENT, 20, (getExcessBond() == 0), false, null, null, false, PlayerType.NONE))
                    {
                        payDebt();
                    }
                }

                int iInterest = getInterestRate();
                if (iInterest >= 10)
                {
                    if (gameServer().isCampaignSevenSols())
                    {
                        if ((iInterest >= ((gameServer().getTurnCount() / 100) + 10)) && !(gameServer().isLastDay()))
                        {
                            if (AI_fundResources(infos().Globals.DEBT_PAYMENT, (100 / iInterest), true, false, null, null, false, PlayerType.NONE))
                            {
                                payDebt();
                            }
                        }
                    }

                    if (iInterest >= (((infos().HQLevelsNum() - getHQLevel() - 1) * 3) + 10))
                    {
                        if (AI_fundResources(infos().Globals.DEBT_PAYMENT, (((getHQLevel() == (infos().HQLevelsNum() - 1)) ? 200 : 400) / iInterest), true, false, null, null, false, PlayerType.NONE))
                        {
                            payDebt();
                        }
                    }
                }
            }
        }

        public virtual bool AI_doAuction()
        {
            if (!(gameServer()).isAuction())
            {
                return false;
            }

            if (isHuman())
            {
                return false;
            }

            if (!canBid())
            {
                return false;
            }

            if (gameServer().getAuctionLeader() != PlayerType.NONE)
            {
                if (gameServer().playerClient(gameServer().getAuctionLeader()).getTeam() == getTeam())
                {
                    return false;
                }
            }

            {
                int iProb = 2;

                if (gameServer().getAuctionTime() >= infos().Globals.AUCTION_TIME_BID)
                {
                    iProb++;
                }

                if (gameServer().getAuctionBid() > 10000)
                {
                    iProb++;
                }

                if (gameServer().random().Next(iProb) != 0)
                {
                    return false;
                }
            }

            int iValue = 0;

            switch (gameServer().getAuction())
            {
                case AuctionType.PATENT:
                    {
                        iValue += AI_patentValue((PatentType)(gameServer().getAuctionData1()), true, false);
                        break;
                    }

                case AuctionType.BLACK_MARKET_SABOTAGE:
                    {
                        iValue += AI_blackMarketValue((BlackMarketType)(gameServer().getAuctionData1()));
                        break;
                    }

                case AuctionType.TILE:
                    {
                        iValue += AI_tileValue(gameServer().tileServer(gameServer().getAuctionData1()));
                        break;
                    }

                case AuctionType.TILE_BUILDING:
                    {
                        TileServer pTile = gameServer().tileServer(gameServer().getAuctionData1());
                        iValue += AI_tileValue(pTile);

                        BuildingType eBuilding = (BuildingType)gameServer().getAuctionData2();
                        iValue += (AI_buildingValueTotal(eBuilding, pTile, getPlayer(), pTile.countConnections(eBuilding, getPlayer(), true, false), true, true, true, false) * 20);
                        break;
                    }

                case AuctionType.CLAIM:
                    {
                        iValue += AI_claimValue();
                        break;
                    }

                case AuctionType.PERK:
                    {
                        iValue += AI_perkValue((PerkType)(gameServer().getAuctionData1()));
                        break;
                    }
            }

            if (iValue > 0)
            {
                iValue *= 5;
                iValue /= 4;

                if (gameServer().isSevenSols())
                {
                    iValue *= ((gameServer().getLastDay() * 3) - gameServer().getDays());
                    iValue /= (gameServer().getLastDay() * 3);
                }

                iValue *= Math.Max(0, (infos().personality(getPersonality()).miAuctionValueModifier + 100));
                iValue /= 100;

                iValue *= (50);
                iValue /= (50 + getInterestRate());

                int iNewBid = gameServer().getAuctionBid() + gameServer().getNextAuctionBid();
                if (iNewBid <= iValue)
                {
                    increaseBid();
                    return true;
                }
            }

            return false;
        }

        protected virtual bool AI_saveResource(ResourceType eResource)
        {
            if (!(infos().resource(eResource).mbTrade))
            {
                return false;
            }

            bool bSave = false;

            if (!bSave)
            {
                int iLife = AI_getResourceLifeSupport(eResource);
                if (iLife < 0)
                {
                    if (getResourceStockpile(eResource, false) < -(50 * iLife))
                    {
                        bSave = true;
                    }
                }
            }

            if (!bSave)
            {
                if (eResource == infos().HQ(getHQ()).meBaseResource)
                {
                    if (getWholeResourceStockpile(eResource, false) <= (getHQLevelInt() * 50))
                    {
                        bSave = true;
                    }
                }
            }

            if (!bSave)
            {
                if (getWholeResourceStockpile(eResource, false) <= AI_saveForUpgradeResource(eResource))
                {
                    bSave = true;
                }
            }

            return bSave;
        }

        protected virtual void AI_doMarket()
        {
            using (new UnityProfileScope("Player::AI_doMarket"))
            {
                if ((int)(getPlayer() + gameServer().getTurnCount()) % (Constants.MAX_NUM_PLAYERS / 2) != 0)
                {
                    return;
                }

                HashSet<ResourceType> seResourceTraded = new HashSet<ResourceType>();

                foreach (EventGameTime eventGameTime in gameServer().getEventGameTimes())
                {
                    for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                    {
                        if (gameServer().isResourceValid(eLoopResource))
                        {
                            if (infos().resource(eLoopResource).mbTrade)
                            {
                                int iChange = infos().eventGame(eventGameTime.meEventGame).maiResourceChange[(int)eLoopResource];
                                if (iChange != 0)
                                {
                                    iChange *= eventGameTime.miMultiplier;
                                    iChange /= 100;

                                    int iTrade = 0;

                                    if (eventGameTime.miDelay > 0)
                                    {
                                        if ((eventGameTime.mePlayer == getPlayer()) ||
                                            (infos().HQ(getHQ()).mbEarlyEventAnnounce))
                                        {
                                            iTrade = -((iChange * (eventGameTime.miDelay + eventGameTime.miTime)) / Constants.RESOURCE_MULTIPLIER);
                                        }
                                    }
                                    else
                                    {
                                        int iTimeOriginal = infos().eventGame(eventGameTime.meEventGame).miTime;

                                        if ((eventGameTime.miTime > ((iTimeOriginal * 2) / 3)) || (eventGameTime.miTime < (iTimeOriginal / 3)))
                                        {
                                            iTrade = -((iChange * (eventGameTime.miTime - (iTimeOriginal / 2))) / Constants.RESOURCE_MULTIPLIER);
                                        }
                                    }

                                    int iPrice = gameServer().marketServer().getWholePrice(eLoopResource);

                                    if ((iTrade > 0) && (iChange > 0))
                                    {
                                        if (iPrice > infos().resource(eLoopResource).miMarketPrice)
                                        {
                                            iTrade = 0;
                                        }
                                    }

                                    if (iTrade > 0)
                                    {
                                        if (iPrice > ((Constants.WHOLE_PRICE_MAX * 4) / 5))
                                        {
                                            iTrade = 0;
                                        }
                                    }

                                    if (iTrade != 0)
                                    {
                                        if (iTrade > 0)
                                        {
                                            List<int> aiResources = Enumerable.Repeat(0, (int)infos().resourcesNum()).ToList();
                                            aiResources[(int)eLoopResource] = (getWholeResourceStockpile(eLoopResource, false) + iTrade);

                                            AI_fundResources(0, 50, true, false, aiResources, null, false, PlayerType.NONE);
                                        }

                                        int iTraded = trade(eLoopResource, iTrade, true);

                                        if (iTraded != 0)
                                        {
                                            seResourceTraded.Add(eLoopResource);

                                            if (AI_LOGGING && (iTraded > 0)) Debug.Log(((iChange > 0) ? "[SURPLUS]" : "[SHORTAGE]") + " Bought " + iTraded + " " + TEXT(infos().resource(eLoopResource).meName) + " at $" + iPrice + " [" + getRealName() + "]");
                                            if (AI_LOGGING && (iTraded < 0)) Debug.Log(((iChange > 0) ? "[SURPLUS]" : "[SHORTAGE]") + " Sold " + iTraded + " " + TEXT(infos().resource(eLoopResource).meName) + " at $" + iPrice + " [" + getRealName() + "]");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (AI_saveForUpgrade())
                {
                    for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                    {
                        if (gameServer().isResourceValid(eLoopResource))
                        {
                            if (infos().resource(eLoopResource).mbTrade)
                            {
                                if (AI_getResourceRateAverage(eLoopResource) <= 0)
                                {
                                    int iNeeded = Math.Max(0, (AI_saveForUpgradeResource(eLoopResource) - getWholeResourceStockpile(eLoopResource, false)));
                                    if (iNeeded > 0)
                                    {
                                        iNeeded = Math.Min(10, iNeeded);

                                        {
                                            List<int> aiResources = Enumerable.Repeat(0, (int)infos().resourcesNum()).ToList();
                                            aiResources[(int)eLoopResource] = (getWholeResourceStockpile(eLoopResource, false) + iNeeded);

                                            AI_fundResources(0, 50, true, false, aiResources, null, false, PlayerType.NONE);
                                        }

                                        int iTraded = trade(eLoopResource, iNeeded, true);
                                        if (iTraded != 0)
                                        {
                                            seResourceTraded.Add(eLoopResource);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (OrderInfo pLoopOrderInfo in getOrderInfos(OrderType.HACK))
                {
                    EventGameType eEventGame = infos().espionage((EspionageType)(pLoopOrderInfo.miData1)).meEventGame;

                    if (eEventGame != EventGameType.NONE)
                    {
                        for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                        {
                            if (infos().eventGame(eEventGame).maiResourceChange[(int)eLoopResource] != 0)
                            {
                                seResourceTraded.Add(eLoopResource);
                            }
                        }
                    }
                }

                int iTargetWholeStockpile = ((getHQLevelInt() * 10) + 20);

                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    if (gameServer().isResourceValid(eLoopResource))
                    {
                        if (infos().resource(eLoopResource).mbTrade)
                        {
                            if (!(seResourceTraded.Contains(eLoopResource)))
                            {
                                int iPrice = gameServer().marketServer().getWholePrice(eLoopResource);
                                int iWholeStockpile = getWholeResourceStockpile(eLoopResource, false);

                                if (iWholeStockpile < (iTargetWholeStockpile * 5))
                                {
                                    int iTrade = 0;

                                    if (iPrice <= Math.Max(1, (infos().resource(eLoopResource).miMarketPrice / 20)))
                                    {
                                        iTrade = 80;
                                    }
                                    else if (iPrice <= Math.Max(2, (infos().resource(eLoopResource).miMarketPrice / 14)))
                                    {
                                        iTrade = 40;
                                    }
                                    else if (iPrice <= Math.Max(3, (infos().resource(eLoopResource).miMarketPrice / 9)))
                                    {
                                        iTrade = 20;
                                    }
                                    else if (iPrice <= Math.Max(4, (infos().resource(eLoopResource).miMarketPrice / 6)))
                                    {
                                        iTrade = 10;
                                    }

                                    if (iTrade != 0)
                                    {
                                        int iTraded = trade(eLoopResource, iTrade, true);

                                        if (AI_LOGGING && (iTraded != 0)) Debug.Log("Bought " + iTraded + " " + TEXT(infos().resource(eLoopResource).meName) + " at $" + iPrice + " [" + getRealName() + "]");
                                    }
                                }
                                else if (iWholeStockpile > iTargetWholeStockpile)
                                {
                                    if (!AI_saveResource(eLoopResource))
                                    {
                                        int iTurnRate = ((AI_getResourceRateAverage(eLoopResource) * 10) / Constants.RESOURCE_MULTIPLIER);

                                        int iTrade = 0;

                                        if (iPrice > (infos().resource(eLoopResource).miMarketPrice * 2))
                                        {
                                            iTrade = -(Math.Max(10, iTurnRate));
                                        }
                                        else if (iPrice > (infos().resource(eLoopResource).miMarketPrice * 3))
                                        {
                                            iTrade = -(Math.Max(20, iTurnRate));
                                        }
                                        else if (iPrice > (infos().resource(eLoopResource).miMarketPrice * 4))
                                        {
                                            iTrade = -(Math.Max(40, iTurnRate));
                                        }
                                        else if (iPrice > (infos().resource(eLoopResource).miMarketPrice * 5))
                                        {
                                            iTrade = -(Math.Max(80, iTurnRate));
                                        }

                                        if (iTrade != 0)
                                        {
                                            int iTraded = trade(eLoopResource, iTrade, true);
                                            if (iTraded != 0)
                                            {
                                                seResourceTraded.Add(eLoopResource);

                                                if (AI_LOGGING && (iTraded != 0)) Debug.Log("Sold " + iTraded + " " + TEXT(infos().resource(eLoopResource).meName) + " at $" + iPrice + " [" + getRealName() + "]");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (gameServer().isLastDay())
                {
                    int iPercent = ((100 * gameServer().getHours()) / infos().location(gameServer().getLocation()).miHoursPerDay);

                    for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                    {
                        if (gameServer().isResourceValid(eLoopResource))
                        {
                            if (infos().resource(eLoopResource).mbTrade)
                            {
                                if (!(seResourceTraded.Contains(eLoopResource)))
                                {
                                    int iSale = ((getWholeResourceStockpile(eLoopResource, false) * iPercent) / 100);

                                    int iTraded = trade(eLoopResource, -(iSale), true);

                                    if (iTraded != 0)
                                    {
                                        seResourceTraded.Add(eLoopResource);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        protected virtual int AI_sabotageValue(SabotageType eSabotage)
        {
            int iBaseValue = 8000;

            iBaseValue += (getNumBuildings() * 500);

            iBaseValue *= Math.Max(100, (infos().personality(getPersonality()).miSabotageValueModifier + 100));
            iBaseValue /= 100;

            iBaseValue *= 2;
            iBaseValue /= (int)(gameServer().getNumPlayers());

            int iValue = 0;

            if (infos().sabotage(eSabotage).miHarvestQuantity > 0)
            {
                int iHighestPrice = 0;

                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    if (gameServer().getResourceRateCount(eLoopResource) > 0)
                    {
                        bool bFound = false;

                        foreach (TileServer pLoopTile in gameServer().tileServerAll())
                        {
                            if (pLoopTile.canMine() && !(pLoopTile.isClaimed()))
                            {
                                if (pLoopTile.getResourceLevel(eLoopResource, isAdjacentMining()) > ResourceLevelType.NONE)
                                {
                                    bFound = true;
                                    break;
                                }
                            }
                        }

                        if (bFound)
                        {
                            iHighestPrice = Math.Max(iHighestPrice, gameServer().marketServer().getWholePrice(eLoopResource));
                        }
                    }
                }

                {
                    const int THRESHOLD = 40;

                    int iOldHighestPrice = iHighestPrice;
                    int iCount = 1;

                    iHighestPrice = 0;

                    while (iOldHighestPrice > THRESHOLD)
                    {
                        int iTemp = THRESHOLD;

                        for (int iI = 0; iI < iCount; iI++)
                        {
                            iTemp *= 9;
                            iTemp /= 10;
                        }

                        iHighestPrice += iTemp;
                        iCount++;

                        iOldHighestPrice -= THRESHOLD;
                    }

                    iHighestPrice += iOldHighestPrice;
                }

                iValue += (iHighestPrice * infos().sabotage(eSabotage).miHarvestQuantity);
            }

            if (infos().sabotage(eSabotage).miPlunderQuantity > 0)
            {
                int iBestPrice = 40;

                for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameServer().getNumPlayers(); eLoopPlayer++)
                {
                    PlayerServer pLoopPlayer = gameServer().playerServer(eLoopPlayer);

                    for (int i = 0; i < pLoopPlayer.getNumUnits(); i++)
                    {
                        UnitServer pLoopUnit = gameServer().unitServer(pLoopPlayer.getUnitList()[i]);

                        if (pLoopUnit.getCargoResource() != ResourceType.NONE)
                        {
                            int iPrice = gameServer().marketServer().getWholePrice(pLoopUnit.getCargoResource());

                            if (!(pLoopPlayer.isWinEligible()) || (pLoopPlayer.getTeam() == getTeam()))
                            {
                                iPrice /= 2;
                            }

                            iBestPrice = Math.Max(iBestPrice, iPrice);
                        }
                    }
                }

                iBestPrice *= 4;
                iBestPrice /= 3;

                iValue += (iBestPrice * infos().sabotage(eSabotage).miPlunderQuantity);
            }

            {
                int iRange = infos().sabotage(eSabotage).miDestroyUnitRange;
                if (iRange > 0)
                {
                    int iBestShipment = 0;

                    foreach (KeyValuePair<int, UnitClient> pair in gameServer().getUnitDictionary())
                    {
                        UnitServer pLoopUnit = (UnitServer)(pair.Value);

                        if (pLoopUnit.getTeam() != getTeam())
                        {
                            if (!(infos().unit(pLoopUnit.getType()).mbImmuneDestroy))
                            {
                                if (pLoopUnit.getCargoResource() != ResourceType.NONE)
                                {
                                    iBestShipment = Math.Max(iBestShipment, (gameServer().marketServer().getWholePrice(pLoopUnit.getCargoResource()) * pLoopUnit.getWholeCargoQuantity()));
                                }
                            }
                        }
                    }

                    iValue += ((iBestShipment * iRange) / 10);
                }
            }

            if (infos().sabotage(eSabotage).miEffectTime > 0)
            {
                int iTemp = 0;

                if (infos().sabotage(eSabotage).mbFreezeBuilding)
                {
                    iTemp += (iBaseValue * 2);
                }
                if (infos().sabotage(eSabotage).mbDoubleBuilding)
                {
                    iTemp += (iBaseValue * 6);
                }
                if (infos().sabotage(eSabotage).mbHalfBuilding)
                {
                    iTemp += ((iBaseValue * 3) / 2);
                }
                if (infos().sabotage(eSabotage).mbOverloadBuilding)
                {
                    iTemp += iBaseValue;
                }
                if (infos().sabotage(eSabotage).mbVirusBuilding)
                {
                    iTemp += iBaseValue;
                }

                iTemp *= infos().sabotage(eSabotage).miEffectTime;
                iTemp /= 100;

                if (infos().sabotage(eSabotage).miEffectRange > 0)
                {
                    iTemp *= infos().sabotage(eSabotage).miEffectRange;
                    iTemp /= 2;
                }

                if (infos().sabotage(eSabotage).miEffectLength > 0)
                {
                    iTemp *= infos().sabotage(eSabotage).miEffectLength;
                    iTemp /= 10;
                }

                iValue += iTemp;
            }

            if (infos().sabotage(eSabotage).miResourceLevelChange < 0)
            {
                int iHighestPrice = 0;

                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    if (gameServer().getResourceRateCount(eLoopResource) > 0)
                    {
                        iHighestPrice = Math.Max(iHighestPrice, gameServer().marketServer().getWholePrice(eLoopResource));
                    }
                }

                iValue += ((iBaseValue * -(infos().sabotage(eSabotage).miResourceLevelChange)) / 2);
                iValue += ((iBaseValue * iHighestPrice * -(infos().sabotage(eSabotage).miResourceLevelChange)) / 100);
            }

            if (infos().sabotage(eSabotage).miTakeoverTime > 0)
            {
                iValue += ((iBaseValue * infos().sabotage(eSabotage).miTakeoverTime) / 60);
            }

            if (infos().sabotage(eSabotage).miDamageBuilding > 0)
            {
                iValue += (iBaseValue * infos().sabotage(eSabotage).miDamageBuilding) / 100;
            }

            if (infos().sabotage(eSabotage).mbNewResource)
            {
                int iTemp = iBaseValue;

                int iHighestPrice = 0;

                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    if (infos().resource(eLoopResource).maiLocationAppearanceProb[(int)(gameServer().getLocation())] > 0)
                    {
                        iHighestPrice = Math.Max(iHighestPrice, gameServer().marketServer().getWholePrice(eLoopResource));
                    }
                }

                iTemp *= (iHighestPrice + 50);
                iTemp /= (100);

                iTemp *= (1000);
                iTemp /= (1000 + gameServer().getTurnCount());

                iValue += iTemp;
            }

            if (infos().sabotage(eSabotage).mbChangeTerrain)
            {
                int iTemp = iBaseValue;

                int iHighestPrice = 0;

                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    if (infos().resource(eLoopResource).maiLocationAppearanceProb[(int)(gameServer().getLocation())] > 0)
                    {
                        iHighestPrice = Math.Max(iHighestPrice, gameServer().marketServer().getWholePrice(eLoopResource) * infos().resourceLevel(gameServer().getHighestResourceLevel()).miRateMultiplier / 100);
                    }
                }

                iTemp *= (iHighestPrice + 50);
                iTemp /= (100);

                iTemp *= (1000);
                iTemp /= (1000 + gameServer().getTurnCount());

                iValue += iTemp;
            }

            if (infos().sabotage(eSabotage).mbDefendSabotage)
            {
                int iTemp = (iBaseValue * 2);

                iTemp += ((iBaseValue * infos().sabotage(eSabotage).miDefendTime) / 100);

                if (gameServer().countTeamsWinEligible() == 2)
                {
                    iValue *= 3;
                    iValue /= 2;
                }

                if (getHQLevel() == (infos().HQLevelsNum() - 1))
                {
                    iTemp *= 2;
                }

                iValue += iTemp;
            }

            if (!(infos().sabotage(eSabotage).mbTriggersDefense) && infos().sabotage(eSabotage).mbHostile)
            {
                iValue += iBaseValue;
            }

            if (gameServer().countTeamsWinEligible() == 2)
            {
                iValue *= 3;
                iValue /= 2;
            }

            return iValue;
        }

        protected virtual int AI_blackMarketValue(BlackMarketType eBlackMarket)
        {
            int iValue = 0;

            iValue += (infos().blackMarket(eBlackMarket).miNewClaims * AI_claimValue());

            {
                int iChange = infos().blackMarket(eBlackMarket).miBondRatingChange;
                if (iChange != 0)
                {
                    BondType eOldBond = getBondRating();
                    BondType eNewBond = (BondType)Utils.clamp((int)(getBondRating() + iChange), 0, (int)(infos().bondsNum() - 1));

                    iValue -= ((getDebt() * getInterestRate(eOldBond)) / 100);
                    iValue += ((getDebt() * getInterestRate(eNewBond)) / 100);
                }
            }

            {
                SabotageType eSabotage = infos().blackMarket(eBlackMarket).meSabotage;

                if (eSabotage != SabotageType.NONE)
                {
                    iValue += (AI_sabotageValue(eSabotage) * infos().blackMarket(eBlackMarket).miSabotageCount);
                }
            }

            return iValue;
        }

        protected virtual void AI_doBlackMarketOther()
        {
            using (new UnityProfileScope("Player::AI_doBlackMarketOther"))
            {
                const int MIN_PRICE_MULTIPLIER = 20;

                if (gameServer().random().Next(10) != 0)
                {
                    return;
                }

                if (!canBlackMarket())
                {
                    return;
                }

                if (getHQLevelInt() < 2)
                {
                    return;
                }

                if (AI_vulnerableMajorityBuyout())
                {
                    return;
                }

                BlackMarketType eBestBlackMarket = BlackMarketType.NONE;
                int iBestValue = 0;

                for (BlackMarketType eLoopBlackMarket = 0; eLoopBlackMarket < infos().blackMarketsNum(); eLoopBlackMarket++)
                {
                    if (canBlackMarket(eLoopBlackMarket, false))
                    {
                        if (infos().blackMarket(eLoopBlackMarket).meSabotage == SabotageType.NONE)
                        {
                            if (AI_fundResources(getBlackMarketCost(eLoopBlackMarket), MIN_PRICE_MULTIPLIER, true, true, null, null, false, PlayerType.NONE))
                            {
                                int iValue = AI_blackMarketValue(eLoopBlackMarket);

                                iValue += gameServer().random().Next(2000);

                                iValue -= getBlackMarketCost(eLoopBlackMarket);

                                if (iValue > iBestValue)
                                {
                                    eBestBlackMarket = eLoopBlackMarket;
                                    iBestValue = iValue;
                                }
                            }
                        }
                    }
                }

                if (eBestBlackMarket != BlackMarketType.NONE)
                {
                    AI_fundResources(getBlackMarketCost(eBestBlackMarket), MIN_PRICE_MULTIPLIER, true, false, null, null, false, PlayerType.NONE);
                    blackMarket(eBestBlackMarket);

                    if (AI_LOGGING) Debug.Log("@@@Black Market Other " + TEXT(infos().blackMarket(eBestBlackMarket).meName));
                }
            }
        }

        protected virtual void AI_doBlackMarketSabotage()
        {
            using (new UnityProfileScope("Player::AI_doBlackMarketSabotage"))
            {
                const int MIN_PRICE_MULTIPLIER = 50;

                if (!isWinEligible())
                {
                    return;
                }

                if (gameServer().random().Next(infos().personality(getPersonality()).miBlackMarketRoll) != 0)
                {
                    return;
                }

                if (gameServer().isLastDay())
                {
                    return;
                }

                if (!canBlackMarket())
                {
                    return;
                }

                if (getHQLevelInt() < 2)
                {
                    return;
                }

                if (getHQLevelInt() < (gameServer().getHQLevels() / (int)(gameServer().getNumPlayers())))
                {
                    return;
                }

                if (getClaims() > 0)
                {
                    return;
                }

                if (AI_vulnerableMajorityBuyout())
                {
                    return;
                }

                BlackMarketType eBestBlackMarket = BlackMarketType.NONE;
                int iBestValue = 0;

                for (BlackMarketType eLoopBlackMarket = 0; eLoopBlackMarket < infos().blackMarketsNum(); eLoopBlackMarket++)
                {
                    if (canBlackMarket(eLoopBlackMarket, false))
                    {
                        if (infos().blackMarket(eLoopBlackMarket).meSabotage != SabotageType.NONE)
                        {
                            if (AI_fundResources(getBlackMarketCost(eLoopBlackMarket), MIN_PRICE_MULTIPLIER, true, true, null, null, false, PlayerType.NONE))
                            {
                                int iValue = AI_blackMarketValue(eLoopBlackMarket);

                                iValue += gameServer().random().Next(2000);

                                iValue -= getBlackMarketCost(eLoopBlackMarket);

                                if (iValue > iBestValue)
                                {
                                    eBestBlackMarket = eLoopBlackMarket;
                                    iBestValue = iValue;
                                }
                            }
                        }
                    }
                }

                if (eBestBlackMarket != BlackMarketType.NONE)
                {
                    AI_fundResources(getBlackMarketCost(eBestBlackMarket), MIN_PRICE_MULTIPLIER, true, false, null, null, false, PlayerType.NONE);
                    blackMarket(eBestBlackMarket);

                    if (AI_LOGGING) Debug.Log("@@@Black Market Sabotage " + TEXT(infos().blackMarket(eBestBlackMarket).meName));
                }
            }
        }

        protected virtual void AI_doBlackMarketDefend()
        {
            using (new UnityProfileScope("Player::AI_doBlackMarketDefend"))
            {
                if (gameServer().random().Next(infos().personality(getPersonality()).miBlackMarketRoll) != 0)
                {
                    return;
                }

                if (!canBlackMarket())
                {
                    return;
                }

                if (getHQLevelInt() < 2)
                {
                    return;
                }

                if (AI_vulnerableMajorityBuyout())
                {
                    return;
                }

                TileServer pBestTile = null;

                int iBestBuildingValue = 100;

                {
                    for (int i = 0; i < getNumConstructions(); i++)
                    {
                        ConstructionServer pLoopConstruction = gameServer().constructionServer(getConstructionList()[i]);
                        TileServer pConstructionTile = pLoopConstruction.tileServer();

                        if (pConstructionTile.isOwnerReal())
                        {
                            int iValue = AI_buildingValueTotal(pLoopConstruction.getType(), pConstructionTile, getPlayer(), pConstructionTile.countConnections(pLoopConstruction.getType(), getPlayer(), true, false), true, false, false, false);
                            if (iValue > iBestBuildingValue)
                            {
                                pBestTile = pConstructionTile;
                                iBestBuildingValue = iValue;
                            }
                        }
                    }

                    for (int i = 0; i < getNumBuildings(); i++)
                    {
                        BuildingServer pLoopBuilding = gameServer().buildingServer(getBuildingList()[i]);
                        TileServer pBuildingTile = pLoopBuilding.tileServer();

                        if (pBuildingTile.isOwnerReal())
                        {
                            int iValue = AI_buildingValueTotal(pLoopBuilding.getType(), pBuildingTile, getPlayer(), pLoopBuilding.getConnections(), true, false, false, false);
                            if (iValue > iBestBuildingValue)
                            {
                                pBestTile = pBuildingTile;
                                iBestBuildingValue = iValue;
                            }
                        }
                    }
                }

                if (pBestTile != null)
                {
                    AI_doBlackMarketDefendTile(pBestTile, iBestBuildingValue);
                }
            }
        }

        public virtual void AI_doBlackMarketDefendBuilding(TileServer pTile)
        {
            if (isHuman())
            {
                return;
            }

            if (isSubsidiary())
            {
                return;
            }

            if (!(pTile.isOwnerReal()))
            {
                return;
            }

            BuildingServer pBuilding = pTile.buildingServer();

            AI_doBlackMarketDefendTile(pTile, AI_buildingValueTotal(pBuilding.getType(), pTile, getPlayer(), pBuilding.getConnections(), true, false, false, false));
        }

        protected virtual void AI_doBlackMarketDefendTile(TileServer pTile, int iBestBuildingValue)
        {
            const int MIN_PRICE_MULTIPLIER = 25;

            BlackMarketType eBestBlackMarket = BlackMarketType.NONE;
            int iBestValue = 0;

            for (BlackMarketType eLoopBlackMarket = 0; eLoopBlackMarket < infos().blackMarketsNum(); eLoopBlackMarket++)
            {
                if (canBlackMarket(eLoopBlackMarket, false))
                {
                    SabotageType eSabotage = infos().blackMarket(eLoopBlackMarket).meSabotage;

                    if (eSabotage != SabotageType.NONE)
                    {
                        if (infos().sabotage(eSabotage).mbDefendSabotage)
                        {
                            if (canSabotageTile(pTile, eSabotage))
                            {
                                if (AI_fundResources(getBlackMarketCost(eLoopBlackMarket), MIN_PRICE_MULTIPLIER, true, true, null, null, false, PlayerType.NONE))
                                {
                                    int iValue = AI_blackMarketValue(eLoopBlackMarket);

                                    iValue += iBestBuildingValue;

                                    iValue -= (getBlackMarketCost(eLoopBlackMarket) / 2);

                                    if (iValue > iBestValue)
                                    {
                                        eBestBlackMarket = eLoopBlackMarket;
                                        iBestValue = iValue;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (eBestBlackMarket != BlackMarketType.NONE)
            {
                AI_fundResources(getBlackMarketCost(eBestBlackMarket), MIN_PRICE_MULTIPLIER, true, false, null, null, false, PlayerType.NONE);
                blackMarket(eBestBlackMarket);
                sabotage(pTile, infos().blackMarket(eBestBlackMarket).meSabotage);

                if (AI_LOGGING) Debug.Log("@@@Black Market Defend " + TEXT(infos().blackMarket(eBestBlackMarket).meName) + ": (" + pTile.getX() + ", " + pTile.getY() + ") [" + getRealName() + "]");
            }
        }

        protected virtual void AI_doBlackMarketAttack()
        {
            using (new UnityProfileScope("Player::AI_doBlackMarketAttack"))
            {
                const int MIN_PRICE_MULTIPLIER = 50;

                if (!isWinEligible())
                {
                    return;
                }

                if (gameServer().random().Next(infos().personality(getPersonality()).miBlackMarketRoll) != 0)
                {
                    return;
                }

                if (!canBlackMarket())
                {
                    return;
                }

                if (getHQLevelInt() < 3)
                {
                    return;
                }

                if (AI_vulnerableMajorityBuyout())
                {
                    return;
                }

                TileServer pBestTile = null;

                {
                    int iBestValue = 200;

                    for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameServer().getNumPlayers(); eLoopPlayer++)
                    {
                        PlayerServer pLoopPlayer = gameServer().playerServer(eLoopPlayer);

                        if (pLoopPlayer.isWinEligible())
                        {
                            if (pLoopPlayer.getTeam() != getTeam())
                            {
                                for (int i = 0; i < pLoopPlayer.getNumBuildings(); i++)
                                {
                                    BuildingServer pLoopBuilding = gameServer().buildingServer(pLoopPlayer.getBuildingList()[i]);
                                    TileServer pBuildingTile = pLoopBuilding.tileServer();

                                    if (pBuildingTile.isOwnerReal())
                                    {
                                        if (!(pLoopBuilding.isFrozen()) &&
                                            !(pLoopBuilding.isHalf()) &&
                                            !(pLoopBuilding.isOverload()) &&
                                            !(pLoopBuilding.isClosed(getTeam())) &&
                                             (pLoopBuilding.calculateOpenCount(60, getTeam()) >= 40))
                                        {
                                            int iValue = AI_buildingValueTotal(pLoopBuilding.getType(), pBuildingTile, pLoopPlayer.getPlayer(), pLoopBuilding.getConnections(), true, false, false, false);
                                            if (iValue > 0)
                                            {
                                                AI_adjustHostileSabotageValue(ref iValue, pLoopPlayer);

                                                iValue *= (75 + gameServer().random().Next(50));
                                                iValue /= (100);

                                                if (iValue > iBestValue)
                                                {
                                                    pBestTile = pBuildingTile;
                                                    iBestValue = iValue;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (pBestTile != null)
                {
                    BlackMarketType eBestBlackMarket = BlackMarketType.NONE;
                    int iBestValue = 0;

                    for (BlackMarketType eLoopBlackMarket = 0; eLoopBlackMarket < infos().blackMarketsNum(); eLoopBlackMarket++)
                    {
                        if (canBlackMarket(eLoopBlackMarket, false))
                        {
                            SabotageType eSabotage = infos().blackMarket(eLoopBlackMarket).meSabotage;

                            if (eSabotage != SabotageType.NONE)
                            {
                                if ((infos().sabotage(eSabotage).mbFreezeBuilding) ||
                                    (infos().sabotage(eSabotage).mbHalfBuilding) ||
                                    (infos().sabotage(eSabotage).miDamageBuilding > 0) ||
                                    (infos().sabotage(eSabotage).miTakeoverTime > 0))
                                {
                                    if (canSabotageTile(pBestTile, eSabotage))
                                    {
                                        if (AI_fundResources(getBlackMarketCost(eLoopBlackMarket), MIN_PRICE_MULTIPLIER, true, true, null, null, false, PlayerType.NONE))
                                        {
                                            int iValue = AI_blackMarketValue(eLoopBlackMarket);

                                            iValue *= (75 + gameServer().random().Next(50));
                                            iValue /= (100);

                                            iValue -= getBlackMarketCost(eLoopBlackMarket);

                                            if (iValue > iBestValue)
                                            {
                                                eBestBlackMarket = eLoopBlackMarket;
                                                iBestValue = iValue;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (eBestBlackMarket != BlackMarketType.NONE)
                    {
                        AI_fundResources(getBlackMarketCost(eBestBlackMarket), MIN_PRICE_MULTIPLIER, true, false, null, null, false, PlayerType.NONE);
                        blackMarket(eBestBlackMarket);
                        sabotage(pBestTile, infos().blackMarket(eBestBlackMarket).meSabotage);

                        if (AI_LOGGING) Debug.Log("@@@Black Market Attack " + TEXT(infos().blackMarket(eBestBlackMarket).meName) + ": (" + pBestTile.getX() + ", " + pBestTile.getY() + ") [" + getRealName() + "]");
                    }
                }
            }
        }

        protected virtual int AI_buildingValueOrders(BuildingType eBuilding, PlayerType ePlayer, TileServer pTile)
        {
            PlayerServer pPlayer = gameServer().playerServer(ePlayer);

            OrderType eOrder = infos().buildingClass(infos().building(eBuilding).meClass).meOrderType;

            if (eOrder != OrderType.NONE)
            {
                int iValue = 0;

                switch (eOrder)
                {
                    case OrderType.PATENT:
                        iValue += (gameServer().countPatentsAvailable() * 20);
                        break;

                    case OrderType.RESEARCH:
                        iValue += ((pPlayer.getNumBuildings() + pPlayer.getHQLevelInt()) * 10);
                        break;

                    case OrderType.HACK:
                        iValue += (gameServer().marketServer().getHighestWholePrice() / 2);
                        break;

                    case OrderType.LAUNCH:
                        iValue += (pPlayer.getBestLaunchProfit() / pPlayer.getOrderTurns(OrderType.LAUNCH, infos().Globals.LAUNCH_TIME, pTile));
                        break;
                }

                if (eOrder != OrderType.LAUNCH)
                {
                    iValue *= pPlayer.getOrderRate(eOrder, pTile);
                    iValue /= 10;
                }

                return iValue;
            }

            return 0;
        }

        protected virtual int AI_buildingValueTotal(BuildingType eBuilding, TileServer pTile, PlayerType ePlayer, int iConnections, bool bOrder, bool bIgnoreTemp, bool bNew, bool bNoExpenses)
        {
            int iValue = 0;

            if (bOrder)
            {
                iValue += AI_buildingValueOrders(eBuilding, ePlayer, pTile);
            }

            iValue += gameServer().calculateRevenue(eBuilding, pTile, ePlayer, iConnections, bIgnoreTemp, bNew, 100, 100, 100, bNoExpenses);

            return iValue;
        }

        protected virtual int AI_buildingValueTotal(BuildingServer pBuilding, bool bIgnoreTemp)
        {
            return AI_buildingValueTotal(pBuilding.getType(), pBuilding.tileServer(), pBuilding.getOwner(), pBuilding.getConnections(), pBuilding.isWorked(), bIgnoreTemp, false, false);
        }

        protected virtual bool AI_validSabotageLine(TileClient pTile)
        {
            if (!(pTile.isClaimed()))
            {
                return false;
            }

            int iAdjacentCount = 0;

            for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
            {
                TileClient pAdjacentTile = gameServer().mapServer().tileClientAdjacent(pTile, eLoopDirection);

                if (pAdjacentTile != null)
                {
                    if (pAdjacentTile.getOwner() == pTile.getOwner())
                    {
                        iAdjacentCount++;
                    }
                }
            }

            if ((iAdjacentCount == 1) || (iAdjacentCount > 2))
            {
                return true;
            }
            else if (iAdjacentCount == 2)
            {
                for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                {
                    TileClient pAdjacentTile = gameServer().mapServer().tileClientAdjacent(pTile, eLoopDirection);

                    if (pAdjacentTile != null)
                    {
                        if (pAdjacentTile.getOwner() == pTile.getOwner())
                        {
                            TileClient pLeftTile = gameServer().mapServer().tileClientAdjacent(pTile, (DirectionType)((int)(eLoopDirection + ((int)(DirectionType.NUM_TYPES) - 1)) % (int)(DirectionType.NUM_TYPES)));

                            if (pLeftTile != null)
                            {
                                if (pLeftTile.getOwner() == pTile.getOwner())
                                {
                                    return true;
                                }
                            }

                            TileClient pRightTile = gameServer().mapServer().tileClientAdjacent(pTile, (DirectionType)((int)(eLoopDirection + 1) % (int)(DirectionType.NUM_TYPES)));

                            if (pRightTile != null)
                            {
                                if (pRightTile.getOwner() == pTile.getOwner())
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        protected virtual void AI_adjustHostileSabotageValue(ref int iValue, PlayerServer pTargetPlayer)
        {
            if (gameServer().isSevenSols())
            {
                iValue *= (10 + pTargetPlayer.getColonySharesOwned());
                iValue /= (10 + 10);
            }
            else
            {
                iValue *= (gameServer().getInitialShares() + pTargetPlayer.countTotalSharesOwned());
                iValue /= (gameServer().getInitialShares() + gameServer().getInitialShares());
            }

            iValue *= (10 + 10);
            iValue /= (10 + pTargetPlayer.getSabotagedCount());

            if (pTargetPlayer.isHuman())
            {
                iValue *= Math.Max(0, (infos().handicap(pTargetPlayer.getHandicap()).miAISabotageModifier + 100));
                iValue /= 100;

                iValue *= Math.Max(0, (infos().rulesSet(gameServer().getRulesSet()).miAISabotageModifier + 100));
                iValue /= 100;
            }
        }

        protected virtual void AI_doSabotage()
        {
            using (new UnityProfileScope("Player::AI_doSabotage"))
            {
                if (gameServer().random().Next(infos().personality(getPersonality()).miSabotageRoll) != 0)
                {
                    return;
                }

                if (gameServer().getHQLevels() < (((int)(gameServer().getNumPlayers()) * 3) / 2))
                {
                    return;
                }

                if (gameServer().random().Next(getHQLevelInt() + 1) == 0)
                {
                    return;
                }

                SabotageType eBestSabotage = SabotageType.NONE;
                TileServer pBestTile = null;
                int iBestValue = 0;

                for (SabotageType eLoopSabotage = 0; eLoopSabotage < infos().sabotagesNum(); eLoopSabotage++)
                {
                    if (canSabotageType(eLoopSabotage) && ((infos().sabotage(eLoopSabotage).mbHostile) ? isWinEligible() : true))
                    {
                        foreach (TileServer pLoopTile in gameServer().tileServerAll())
                        {
                            int iPlunder = infos().sabotage(eLoopSabotage).miPlunderQuantity;
                            if (iPlunder > 0)
                            {
                                using (new UnityProfileScope("Player::AI_doSabotage::Pirates"))
                                {
                                    BuildingServer pLoopBuilding = pLoopTile.buildingServer();

                                    if (pLoopBuilding != null)
                                    {
                                        if (pLoopTile.isOwnerReal() && (pLoopTile.getTeam() != getTeam()) && pLoopTile.ownerClient().isWinEligible())
                                        {
                                            if (!(pLoopTile.isConnectedToHQ()) && !(pLoopBuilding.ownerServer().isTeleportation()))
                                            {
                                                UnitType eUnit = infos().sabotage(eLoopSabotage).meUnit;

                                                if (eUnit != UnitType.NONE)
                                                {
                                                    for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                                    {
                                                        if (infos().resource(eLoopResource).mbTrade)
                                                        {
                                                            int iRate = 0;

                                                            iRate += pLoopBuilding.resourceMining(eLoopResource, false);
                                                            iRate += pLoopBuilding.resourceOutput(eLoopResource, false);

                                                            if (iRate > 0)
                                                            {
                                                                int iValue = iPlunder;

                                                                iValue *= gameServer().marketServer().getWholePrice(eLoopResource);

                                                                iValue *= iRate;
                                                                iValue /= Constants.RESOURCE_MULTIPLIER;

                                                                AI_adjustHostileSabotageValue(ref iValue, pLoopBuilding.ownerServer());

                                                                if (iValue > iBestValue)
                                                                {
                                                                    HQServer pClosestHQ = pLoopBuilding.ownerServer().findClosestHQServer(pLoopTile);

                                                                    if (pClosestHQ != null)
                                                                    {
                                                                        int iPercent = gameServer().random().Next(50) + 25;

                                                                        int iX = pLoopTile.getX() + (((pClosestHQ.getX() - pLoopTile.getX()) * iPercent) / 100);
                                                                        int iY = pLoopTile.getY() + (((pClosestHQ.getY() - pLoopTile.getY()) * iPercent) / 100);

                                                                        TileServer pTargetTile = gameServer().tileServerGrid(iX, iY);

                                                                        if (pTargetTile != null)
                                                                        {
                                                                            if (canSabotage(pTargetTile, eLoopSabotage))
                                                                            {
                                                                                eBestSabotage = eLoopSabotage;
                                                                                pBestTile = pTargetTile;
                                                                                iBestValue = iValue;
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (canSabotage(pLoopTile, eLoopSabotage))
                            {
                                bool bValid = true;
                                int iValue = 0;

                                if (bValid)
                                {
                                    int iRange = infos().sabotage(eLoopSabotage).miDestroyUnitRange;
                                    if (iRange > 0)
                                    {
                                        using (new UnityProfileScope("Player::AI_doSabotage::Magnetic_Storm"))
                                        {
                                            bool bFoundCargo = false;

                                            for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameServer().getNumPlayers(); eLoopPlayer++)
                                            {
                                                PlayerServer pLoopPlayer = gameServer().playerServer(eLoopPlayer);

                                                if (pLoopPlayer.isWinEligible())
                                                {
                                                    for (int i = 0; i < pLoopPlayer.getNumUnits(); i++)
                                                    {
                                                        UnitServer pLoopUnit = gameServer().unitServer(pLoopPlayer.getUnitList()[i]);

                                                        if (!(infos().unit(pLoopUnit.getType()).mbImmuneDestroy))
                                                        {
                                                            if (Utils.stepDistanceTile(pLoopTile, pLoopUnit.tileServer()) <= iRange)
                                                            {
                                                                int iSubValue = 0;

                                                                if (pLoopUnit.hasCargoAny())
                                                                {
                                                                    iSubValue += (gameServer().marketServer().getWholePrice(pLoopUnit.getCargoResource()) * pLoopUnit.getWholeCargoQuantity());
                                                                    bFoundCargo = true;
                                                                }

                                                                iSubValue += 100;

                                                                if (pLoopPlayer.getTeam() == getTeam())
                                                                {
                                                                    iSubValue = -(iSubValue * 2);
                                                                }
                                                                else
                                                                {
                                                                    AI_adjustHostileSabotageValue(ref iSubValue, pLoopPlayer);
                                                                }

                                                                iValue += iSubValue;
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            if (!bFoundCargo)
                                            {
                                                bValid = false;
                                            }
                                        }
                                    }
                                }

                                if (bValid)
                                {
                                    int iTime = infos().sabotage(eLoopSabotage).miEffectTime;
                                    if (iTime > 0)
                                    {
                                        int iRange = infos().sabotage(eLoopSabotage).miEffectRange;
                                        if (iRange > 0)
                                        {
                                            using (new UnityProfileScope("Player::AI_doSabotage::Area_of_Effect"))
                                            {
                                                int iSubValue = 0;

                                                int iCount = 0;

                                                for (int iDX = -(iRange); iDX <= iRange; iDX++)
                                                {
                                                    for (int iDY = -(iRange); iDY <= iRange; iDY++)
                                                    {
                                                        TileServer pRangeTile = gameServer().tileServerRange(pLoopTile, iDX, iDY, iRange);

                                                        if (pRangeTile != null)
                                                        {
                                                            BuildingServer pLoopBuilding = pRangeTile.buildingServer();

                                                            if (pLoopBuilding != null)
                                                            {
                                                                if (pLoopBuilding.isFrozen())
                                                                {
                                                                    bValid = false;
                                                                }

                                                                if (bValid && pLoopBuilding.isDouble())
                                                                {
                                                                    if (infos().sabotage(eLoopSabotage).mbDoubleBuilding)
                                                                    {
                                                                        bValid = false;
                                                                    }
                                                                }

                                                                if (bValid && !(infos().sabotage(eLoopSabotage).mbDoubleBuilding))
                                                                {
                                                                    if (pLoopBuilding.isHalf())
                                                                    {
                                                                        bValid = false;
                                                                    }

                                                                    if (pLoopBuilding.isOverload())
                                                                    {
                                                                        bValid = false;
                                                                    }

                                                                    if (pLoopBuilding.isVirus())
                                                                    {
                                                                        bValid = false;
                                                                    }
                                                                }
                                                                else if (bValid)
                                                                {
                                                                    if (pLoopBuilding.getTeam() != getTeam())
                                                                    {
                                                                        bValid = false;
                                                                    }
                                                                }

                                                                if (bValid && infos().sabotage(eLoopSabotage).mbHostile)
                                                                {
                                                                    if (pLoopBuilding.getTeam() == getTeam())
                                                                    {
                                                                        bValid = false;
                                                                    }
                                                                }

                                                                int iOpenTime = 0;
                                                                if (bValid && pLoopBuilding.isClosed(getTeam()))
                                                                {
                                                                    bValid = false;
                                                                }
                                                                else if (bValid && (iOpenTime = pLoopBuilding.calculateOpenCount(iTime, getTeam())) < iTime / 2)
                                                                {
                                                                    bValid = false;
                                                                }

                                                                if (bValid)
                                                                {
                                                                    if (canSabotageTile(pRangeTile, eLoopSabotage))
                                                                    {
                                                                        if ((pRangeTile == pLoopTile) ||
                                                                            !(pRangeTile.isRevealDefendSabotage(getTeam())) ||
                                                                            !(infos().sabotage(eLoopSabotage).mbTriggersDefense))
                                                                        {
                                                                            if (pLoopBuilding.ownerClient().isWinEligible())
                                                                            {
                                                                                int iBuildingValue = 0;

                                                                                if (infos().sabotage(eLoopSabotage).mbFreezeBuilding ||
                                                                                    infos().sabotage(eLoopSabotage).mbDoubleBuilding ||
                                                                                    infos().sabotage(eLoopSabotage).mbHalfBuilding)
                                                                                {
                                                                                    iBuildingValue += AI_buildingValueTotal(pLoopBuilding, false);
                                                                                }
                                                                                if (infos().sabotage(eLoopSabotage).mbVirusBuilding)
                                                                                {
                                                                                    iBuildingValue += -(AI_buildingValueTotal(pLoopBuilding, false));
                                                                                }

                                                                                if (infos().sabotage(eLoopSabotage).mbOverloadBuilding)
                                                                                {
                                                                                    {
                                                                                        int iPowerOutput = pLoopBuilding.resourceOutput(infos().Globals.ENERGY_RESOURCE, false);
                                                                                        if (iPowerOutput > 0)
                                                                                        {
                                                                                            iBuildingValue += ((iPowerOutput * gameServer().marketServer().getWholePrice(infos().Globals.ENERGY_RESOURCE)) / Constants.RESOURCE_MULTIPLIER);
                                                                                        }
                                                                                    }

                                                                                    iBuildingValue += ((gameServer().powerConsumption(pLoopBuilding.getType(), pLoopBuilding.tileServer(), pLoopBuilding.getOwner()) * gameServer().marketServer().getWholePrice(pLoopBuilding.ownerServer().getEnergyResource())) / Constants.RESOURCE_MULTIPLIER);
                                                                                }

                                                                                if (iBuildingValue > 0)
                                                                                {
                                                                                    iBuildingValue += (pRangeTile.getDoubleTime() / 10);

                                                                                    if (infos().sabotage(eLoopSabotage).mbHostile)
                                                                                    {
                                                                                        if (pRangeTile.isClaimed() && pRangeTile.isOwnerReal())
                                                                                        {
                                                                                            if (pRangeTile.adjacentToHQ(pRangeTile.getOwner()))
                                                                                            {
                                                                                                iBuildingValue *= Math.Max(0, (pRangeTile.ownerServer().getAdjacentHQSabotageModifier() + 100));
                                                                                                iBuildingValue /= 100;
                                                                                            }
                                                                                        }
                                                                                    }

                                                                                    iBuildingValue *= iOpenTime;

                                                                                    iSubValue += iBuildingValue;

                                                                                    iCount++;
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                                if (bValid)
                                                {
                                                    if (iCount > 1)
                                                    {
                                                        iValue += iSubValue;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                if (bValid)
                                {
                                    int iTime = infos().sabotage(eLoopSabotage).miEffectTime;
                                    if (iTime > 0)
                                    {
                                        int iLength = infos().sabotage(eLoopSabotage).miEffectLength;
                                        if (iLength > 0)
                                        {
                                            using (new UnityProfileScope("Player::AI_doSabotage::Power_Surge"))
                                            {
                                                if (AI_validSabotageLine(pLoopTile))
                                                {
                                                    int iSubValue = 0;

                                                    int iCount = 0;

                                                    int iRange = 1;

                                                    for (int iDX = -(iRange); iDX <= iRange; iDX++)
                                                    {
                                                        for (int iDY = -(iRange); iDY <= iRange; iDY++)
                                                        {
                                                            TileServer pRangeTile = gameServer().tileServerRange(pLoopTile, iDX, iDY, iRange);

                                                            if (pRangeTile != null)
                                                            {
                                                                BuildingServer pLoopBuilding = pRangeTile.buildingServer();

                                                                if (pLoopBuilding != null)
                                                                {
                                                                    if (pLoopBuilding.isFrozen())
                                                                    {
                                                                        bValid = false;
                                                                    }

                                                                    if (pLoopBuilding.isDouble())
                                                                    {
                                                                        if (infos().sabotage(eLoopSabotage).mbDoubleBuilding)
                                                                        {
                                                                            bValid = false;
                                                                        }
                                                                    }

                                                                    if (pLoopBuilding.isHalf())
                                                                    {
                                                                        if (!(infos().sabotage(eLoopSabotage).mbDoubleBuilding))
                                                                        {
                                                                            bValid = false;
                                                                        }
                                                                    }

                                                                    if (pLoopBuilding.isOverload())
                                                                    {
                                                                        bValid = false;
                                                                    }

                                                                    if (pLoopBuilding.isVirus())
                                                                    {
                                                                        bValid = false;
                                                                    }

                                                                    if (infos().sabotage(eLoopSabotage).mbFreezeBuilding ||
                                                                        infos().sabotage(eLoopSabotage).mbHalfBuilding ||
                                                                        infos().sabotage(eLoopSabotage).mbOverloadBuilding ||
                                                                        infos().sabotage(eLoopSabotage).mbVirusBuilding)
                                                                    {
                                                                        if (pLoopBuilding.getTeam() == getTeam())
                                                                        {
                                                                            bValid = false;
                                                                        }
                                                                    }

                                                                    if (infos().sabotage(eLoopSabotage).mbDoubleBuilding)
                                                                    {
                                                                        if (pLoopBuilding.getTeam() != getTeam())
                                                                        {
                                                                            bValid = false;
                                                                        }
                                                                    }

                                                                    if (pLoopBuilding.isClosed(getTeam()))
                                                                    {
                                                                        bValid = false;
                                                                    }

                                                                    if (pLoopBuilding.calculateOpenCount(60, getTeam()) < 40)
                                                                    {
                                                                        bValid = false;
                                                                    }

                                                                    if (bValid)
                                                                    {
                                                                        if (canSabotageTile(pRangeTile, eLoopSabotage))
                                                                        {
                                                                            if (!(pRangeTile.isRevealDefendSabotage(getTeam())) ||
                                                                                !(infos().sabotage(eLoopSabotage).mbTriggersDefense))
                                                                            {
                                                                                if (pLoopBuilding.ownerClient().isWinEligible())
                                                                                {
                                                                                    int iBuildingValue = 0;

                                                                                    if (infos().sabotage(eLoopSabotage).mbFreezeBuilding ||
                                                                                        infos().sabotage(eLoopSabotage).mbDoubleBuilding ||
                                                                                        infos().sabotage(eLoopSabotage).mbHalfBuilding)
                                                                                    {
                                                                                        iBuildingValue += AI_buildingValueTotal(pLoopBuilding, false);
                                                                                    }
                                                                                    if (infos().sabotage(eLoopSabotage).mbOverloadBuilding)
                                                                                    {
                                                                                        {
                                                                                            int iPowerOutput = pLoopBuilding.resourceOutput(infos().Globals.ENERGY_RESOURCE, false);
                                                                                            if (iPowerOutput > 0)
                                                                                            {
                                                                                                iBuildingValue += ((iPowerOutput * gameServer().marketServer().getWholePrice(infos().Globals.ENERGY_RESOURCE)) / Constants.RESOURCE_MULTIPLIER);
                                                                                            }
                                                                                        }

                                                                                        iBuildingValue += ((gameServer().powerConsumption(pLoopBuilding.getType(), pLoopBuilding.tileServer(), pLoopBuilding.getOwner()) * gameServer().marketServer().getWholePrice(pLoopBuilding.ownerServer().getEnergyResource())) / Constants.RESOURCE_MULTIPLIER);
                                                                                    }
                                                                                    if (infos().sabotage(eLoopSabotage).mbVirusBuilding)
                                                                                    {
                                                                                        iBuildingValue += -(AI_buildingValueTotal(pLoopBuilding, false));
                                                                                    }

                                                                                    if (iBuildingValue > 0)
                                                                                    {
                                                                                        iBuildingValue += (pRangeTile.getDoubleTime() / 10);

                                                                                        if (infos().sabotage(eLoopSabotage).mbFreezeBuilding ||
                                                                                            infos().sabotage(eLoopSabotage).mbHalfBuilding ||
                                                                                            infos().sabotage(eLoopSabotage).mbOverloadBuilding ||
                                                                                            infos().sabotage(eLoopSabotage).mbVirusBuilding)
                                                                                        {
                                                                                            if (pRangeTile.isClaimed() && pRangeTile.isOwnerReal())
                                                                                            {
                                                                                                if (pRangeTile.adjacentToHQ(pRangeTile.getOwner()))
                                                                                                {
                                                                                                    iBuildingValue *= Math.Max(0, (pRangeTile.ownerServer().getAdjacentHQSabotageModifier() + 100));
                                                                                                    iBuildingValue /= 100;
                                                                                                }
                                                                                            }
                                                                                        }

                                                                                        if (infos().sabotage(eLoopSabotage).mbFreezeBuilding)
                                                                                        {
                                                                                            if (pRangeTile.isClaimed() && pRangeTile.isOwnerReal())
                                                                                            {
                                                                                                iBuildingValue *= Math.Max(0, (infos().HQ(pRangeTile.ownerServer().getHQ()).miFrozenEffectModifier + 100));
                                                                                                iBuildingValue /= 100;
                                                                                            }
                                                                                        }

                                                                                        {
                                                                                            int iTestTime = (iTime / ((pRangeTile == pLoopTile) ? 1 : 2));
                                                                                            if (iTestTime > 1)
                                                                                            {
                                                                                                iBuildingValue *= pLoopBuilding.calculateOpenCount(iTestTime, getTeam());
                                                                                                iBuildingValue /= iTestTime;
                                                                                            }
                                                                                        }

                                                                                        iSubValue += (iBuildingValue * ((pRangeTile == pLoopTile) ? 3 : 1));

                                                                                        iCount++;
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }

                                                            if (!bValid)
                                                            {
                                                                break;
                                                            }
                                                        }

                                                        if (!bValid)
                                                        {
                                                            break;
                                                        }
                                                    }

                                                    if (bValid)
                                                    {
                                                        if (iCount > 1)
                                                        {
                                                            iSubValue *= (iCount * 2);
                                                            iSubValue /= (int)DirectionType.NUM_TYPES;

                                                            iSubValue *= iTime;

                                                            iValue += iSubValue;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                if (bValid)
                                {
                                    int iChange = infos().sabotage(eLoopSabotage).miResourceLevelChange;
                                    if (iChange < 0)
                                    {
                                        using (new UnityProfileScope("Player::AI_doSabotage::Underground_Nuke"))
                                        {
                                            BuildingServer pLoopBuilding = pLoopTile.buildingServer();

                                            if (pLoopBuilding != null)
                                            {
                                                if (pLoopTile.isOwnerReal() && (pLoopTile.getTeam() != getTeam()) && pLoopTile.ownerClient().isWinEligible())
                                                {
                                                    for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                                    {
                                                        ResourceLevelType eOldResourceLevel = pLoopTile.getResourceLevel(eLoopResource, false);

                                                        if (eOldResourceLevel > pLoopBuilding.ownerClient().getMinimumMining(pLoopBuilding.getClass()))
                                                        {
                                                            if (infos().resourceLevel(eOldResourceLevel).mbCanBomb)
                                                            {
                                                                int iSubValue = 0;

                                                                ResourceLevelType eNewResourceLevel = eOldResourceLevel;

                                                                for (int iPass = 0; iPass < -(iChange); iPass++)
                                                                {
                                                                    if (eNewResourceLevel > ResourceLevelType.NONE)
                                                                    {
                                                                        eNewResourceLevel = (eNewResourceLevel - 1);

                                                                        if (!(infos().resourceLevel(eNewResourceLevel).mbCanBomb))
                                                                        {
                                                                            break;
                                                                        }
                                                                    }
                                                                }

                                                                if (eNewResourceLevel < eOldResourceLevel)
                                                                {
                                                                    iSubValue += gameServer().resourceMiningLevel(pLoopBuilding.getType(), eLoopResource, eOldResourceLevel, pLoopTile.getTerrainNoIce(), pLoopBuilding.getOwner(), infos().building(pLoopBuilding.getType()).maiResourceMining[(int)eLoopResource], pLoopBuilding.getConnections(), false);
                                                                    iSubValue -= gameServer().resourceMiningLevel(pLoopBuilding.getType(), eLoopResource, eNewResourceLevel, pLoopTile.getTerrainNoIce(), pLoopBuilding.getOwner(), infos().building(pLoopBuilding.getType()).maiResourceMining[(int)eLoopResource], pLoopBuilding.getConnections(), false);

                                                                    iSubValue *= gameServer().marketServer().getWholePrice(eLoopResource);
                                                                    iSubValue *= (NUM_TURNS * 2);

                                                                    iSubValue /= Constants.RESOURCE_MULTIPLIER;

                                                                    for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
                                                                    {
                                                                        TileServer pAdjacentTile = gameServer().tileServerAdjacent(pLoopTile, eDirection);

                                                                        if (pAdjacentTile != null)
                                                                        {
                                                                            ResourceLevelType eAdjacentResourceLevel = pAdjacentTile.getResourceLevel(eLoopResource, false);

                                                                            if (eNewResourceLevel < eAdjacentResourceLevel)
                                                                            {
                                                                                if (pLoopTile.ownerClient().isAdjacentMining())
                                                                                {
                                                                                    iSubValue /= 2;
                                                                                }
                                                                                else
                                                                                {
                                                                                    iSubValue *= 9;
                                                                                    iSubValue /= 10;
                                                                                }
                                                                                break;
                                                                            }
                                                                        }
                                                                    }

                                                                    if (infos().building(pLoopBuilding.getType()).mbSelfInput)
                                                                    {
                                                                        iSubValue /= 2;
                                                                    }

                                                                    iValue += iSubValue;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                if (bValid)
                                {
                                    int iTime = infos().sabotage(eLoopSabotage).miTakeoverTime;
                                    if (iTime > 0)
                                    {
                                        using (new UnityProfileScope("Player::AI_doSabotage::Mutiny"))
                                        {
                                            BuildingServer pLoopBuilding = pLoopTile.buildingServer();

                                            if (pLoopBuilding != null)
                                            {
                                                if ((pLoopTile.getTeam() != getTeam()) && (pLoopTile.getRealTeam() != getTeam()) && pLoopTile.ownerClient().isWinEligible())
                                                {
                                                    if (pLoopTile.isClaimed() && pLoopTile.isOwnerReal())
                                                    {
                                                        if (pLoopTile.adjacentToHQ(pLoopTile.getOwner()))
                                                        {
                                                            iTime *= Math.Max(0, (pLoopTile.ownerServer().getAdjacentHQSabotageModifier() + 100));
                                                            iTime /= 100;
                                                        }
                                                    }

                                                    if (!(pLoopBuilding.isFrozen()) && !(pLoopBuilding.isClosed(getTeam())) && (pLoopBuilding.calculateOpenCount(iTime, getTeam()) >= ((iTime * 2) / 3)))
                                                    {
                                                        int iSubValue = 0;

                                                        {
                                                            int iConnections = pLoopTile.countConnections(pLoopBuilding.getType(), getPlayer(), false, false);

                                                            int iSubSubValue = (AI_buildingValueOrders(pLoopBuilding.getType(), getPlayer(), pLoopTile) / 2);

                                                            iSubSubValue += gameServer().entertainmentProfit(pLoopBuilding.getType(), pLoopTile, getPlayer(), false, false);

                                                            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                                            {
                                                                int iRate = 0;

                                                                iRate += gameServer().resourceMiningTile(pLoopBuilding.getType(), eLoopResource, pLoopTile, getPlayer(), infos().building(pLoopBuilding.getType()).maiResourceMining[(int)eLoopResource], iConnections, false);
                                                                if (!(gameServer().isBuildingHasInput(pLoopBuilding.getType())))
                                                                {
                                                                    iRate += gameServer().resourceOutput(pLoopBuilding.getType(), eLoopResource, pLoopTile, getPlayer(), iConnections, false);
                                                                }

                                                                if (getEnergyResource() == eLoopResource)
                                                                {
                                                                    iRate -= gameServer().powerConsumption(pLoopBuilding.getType(), pLoopTile, getPlayer());
                                                                }

                                                                iSubSubValue += ((iRate * gameServer().marketServer().getWholePrice(eLoopResource)) / Constants.RESOURCE_MULTIPLIER);
                                                            }

                                                            iSubSubValue *= ((iTime * 2) / 3);

                                                            iSubValue += iSubSubValue;
                                                        }

                                                        {
                                                            int iSubSubValue = AI_buildingValueTotal(pLoopBuilding, false);

                                                            iSubSubValue *= (iTime / 3);

                                                            iSubValue += iSubSubValue;
                                                        }

                                                        if (pLoopTile.adjacentToHQ(pLoopTile.getOwner()))
                                                        {
                                                            if (pLoopTile.ownerServer().getAdjacentHQSabotageModifier() >= 0)
                                                            {
                                                                iSubValue *= 11;
                                                                iSubValue /= 10;
                                                            }
                                                        }

                                                        iValue += iSubValue;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                if (bValid)
                                {
                                    int iHarvest = infos().sabotage(eLoopSabotage).miHarvestQuantity;
                                    if (iHarvest > 0)
                                    {
                                        using (new UnityProfileScope("Player::AI_doSabotage::MULE"))
                                        {
                                            UnitType eUnit = infos().sabotage(eLoopSabotage).meUnit;

                                            if (eUnit != UnitType.NONE)
                                            {
                                                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                                {
                                                    int iRate = gameServer().resourceMiningTile(BuildingType.NONE, eLoopResource, pLoopTile, getPlayer(), infos().unit(eUnit).miMining, 0, false);
                                                    if (iRate > 0)
                                                    {
                                                        int iSubValue = iHarvest;

                                                        iSubValue *= gameServer().marketServer().getWholePrice(eLoopResource);

                                                        iSubValue *= iRate;
                                                        iSubValue /= Constants.RESOURCE_MULTIPLIER;

                                                        {
                                                            HQServer pClosestHQ = findClosestHQServer(pLoopTile);

                                                            if (pClosestHQ != null)
                                                            {
                                                                iSubValue *= (Utils.maxStepDistance(gameServer()));
                                                                iSubValue /= (Utils.maxStepDistance(gameServer()) + Utils.stepDistanceTile(pLoopTile, pClosestHQ.tileServer()));
                                                            }
                                                        }

                                                        iValue += iSubValue;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                if (bValid)
                                {
                                    if (infos().sabotage(eLoopSabotage).miDamageBuilding > 0)
                                    {
                                        using (new UnityProfileScope("Player::AI_doSabotage::Dynamite"))
                                        {
                                            BuildingServer pLoopBuilding = pLoopTile.buildingServer();

                                            if (pLoopBuilding != null)
                                            {
                                                if ((pLoopTile.getTeam() != getTeam()) && (pLoopTile.getRealTeam() != getTeam()) && pLoopTile.ownerClient().isWinEligible())
                                                {
                                                    if (!(pLoopBuilding.isFrozen()) && !(pLoopBuilding.isClosed(getTeam())))
                                                    {
                                                        int iSubValue = AI_buildingValueTotal(pLoopBuilding, false) * 200;

                                                        if (pLoopTile.isClaimed() && pLoopTile.isOwnerReal())
                                                        {
                                                            if (pLoopTile.adjacentToHQ(pLoopTile.getOwner()))
                                                            {
                                                                iSubValue *= Math.Max(0, (pLoopTile.ownerServer().getAdjacentHQSabotageModifier() + 100));
                                                                iSubValue /= 100;
                                                            }
                                                        }

                                                        if (infos().buildingClass(infos().building(pLoopBuilding.getType()).meClass).meOrderType == OrderType.NONE)
                                                        {
                                                            if (pLoopTile.isConnectedToHQ())
                                                            {
                                                                iSubValue /= 3;
                                                            }

                                                            HQServer pClosestHQ = pLoopBuilding.ownerServer().findClosestHQServer(pLoopTile);

                                                            if (pClosestHQ != null)
                                                            {
                                                                iSubValue *= (Utils.stepDistanceTile(pLoopTile, pClosestHQ.tileServer()) * 2);
                                                                iSubValue /= (Utils.maxStepDistance(gameServer()) / 2);
                                                            }
                                                        }

                                                        iValue += iSubValue;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    if (bValid)
                                    {
                                        if (infos().sabotage(eLoopSabotage).mbReturnClaim)
                                        {
                                            bValid = false;
                                        }
                                    }

                                    if (bValid)
                                    {
                                        if (infos().sabotage(eLoopSabotage).mbAuctionTile)
                                        {
                                            bValid = false;
                                        }
                                    }

                                    if (bValid)
                                    {
                                        if (infos().sabotage(eLoopSabotage).mbWrongBuilding)
                                        {
                                            bValid = false;
                                        }
                                    }

                                    if (bValid)
                                    {
                                        if (infos().sabotage(eLoopSabotage).mbRevealBuilding)
                                        {
                                            bValid = false;
                                        }
                                    }

                                    if (bValid)
                                    {
                                        if (infos().sabotage(eLoopSabotage).mbNewResource)
                                        {
                                            if (pLoopTile.getRealTeam() == getTeam())
                                            {
                                                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                                {
                                                    int iProb = TerrainGenerator.getResourceProb(eLoopResource, gameServer(), pLoopTile, true);
                                                    if (iProb > 0)
                                                    {
                                                        iValue += (iProb * gameServer().marketServer().getWholePrice(eLoopResource));
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    if (bValid)
                                    {
                                        if (infos().sabotage(eLoopSabotage).mbChangeTerrain)
                                        {
                                            if (pLoopTile.getRealTeam() == getTeam() || (!pLoopTile.isClaimed() && this.getClaims() > 0))
                                            {
                                                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                                {
                                                    iValue += (infos().resourceLevel(pLoopTile.getResourceLevelAdjacent(eLoopResource, mePlayer)).miRateMultiplier / 100 * gameServer().marketServer().getWholePrice(eLoopResource));
                                                }
                                            }
                                        }
                                    }

                                    if (bValid)
                                    {
                                        if (infos().sabotage(eLoopSabotage).mbDefendSabotage)
                                        {
                                            BuildingServer pLoopBuilding = pLoopTile.buildingServer();

                                            if (pLoopBuilding != null)
                                            {
                                                if (pLoopTile.isOwnerReal() && (pLoopTile.getOwner() == getPlayer()))
                                                {
                                                    int iSubValue = AI_buildingValueTotal(pLoopBuilding, false) * (100 + (infos().sabotage(eLoopSabotage).miDefendTime / 5));

                                                    if (infos().buildingClass(infos().building(pLoopBuilding.getType()).meClass).meOrderType == OrderType.LAUNCH)
                                                    {
                                                        iSubValue *= 2;
                                                    }

                                                    for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
                                                    {
                                                        TileServer pAdjacentTile = gameServer().tileServerAdjacent(pLoopTile, eDirection);

                                                        if (pAdjacentTile != null)
                                                        {
                                                            if (pAdjacentTile.getOwner() == getPlayer())
                                                            {
                                                                BuildingServer pAdjacentBuilding = pAdjacentTile.buildingServer();

                                                                if (pAdjacentBuilding != null)
                                                                {
                                                                    if (pAdjacentBuilding.getType() == pLoopBuilding.getType())
                                                                    {
                                                                        if (pAdjacentTile.getDefendSabotage() != SabotageType.NONE)
                                                                        {
                                                                            iSubValue /= 2;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }

                                                    iValue += iSubValue;
                                                }
                                            }
                                        }
                                    }

                                    if ((iValue > 0) && bValid)
                                    {
                                        iValue *= (90 + gameServer().random().Next(20));
                                        iValue /= (100);

                                        if (infos().sabotage(eLoopSabotage).mbTriggersDefense)
                                        {
                                            if (pLoopTile.isRevealDefendSabotage(getTeam()))
                                            {
                                                iValue *= 2;
                                                iValue /= 3;
                                            }
                                        }

                                        if (infos().sabotage(eLoopSabotage).mbHostile && (infos().sabotage(eLoopSabotage).miDestroyUnitRange == 0))
                                        {
                                            if (pLoopTile.isClaimed() && (pLoopTile.getTeam() != getTeam()))
                                            {
                                                AI_adjustHostileSabotageValue(ref iValue, pLoopTile.ownerServer());
                                            }
                                        }

                                        if (iValue > iBestValue)
                                        {
                                            eBestSabotage = eLoopSabotage;
                                            pBestTile = pLoopTile;
                                            iBestValue = iValue;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if ((pBestTile != null) && (eBestSabotage != SabotageType.NONE))
                {
                    sabotage(pBestTile, eBestSabotage);
                }
            }
        }

        protected virtual void AI_doRepair()
        {
            using (new UnityProfileScope("Player::AI_doRepair"))
            {
                if (gameServer().random().Next(10) != 0)
                {
                    return;
                }

                if (gameServer().isLastHalfDay())
                {
                    return;
                }

                const int MIN_PRICE_MULTIPLIER = 5;

                ConstructionServer pBestConstruction = null;
                int iBestValue = 0;

                foreach (int iConstructionID in getConstructionList())
                {
                    ConstructionServer pLoopConstruction = gameServer().constructionServer(iConstructionID);

                    if (pLoopConstruction.canRepair(false))
                    {
                        if (!(gameServer().isGameAlmostOver()) || pLoopConstruction.tileServer().isConnectedToHQ())
                        {
                            List<int> aiResources = new List<int>();

                            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                            {
                                aiResources.Add(pLoopConstruction.getRepairResourceCost(eLoopResource));
                            }

                            if (AI_fundResources(0, MIN_PRICE_MULTIPLIER, true, true, aiResources, null, false, PlayerType.NONE))
                            {
                                TileServer pLoopTile = pLoopConstruction.tileServer();

                                int iValue = AI_buildingValueTotal(pLoopConstruction.getType(), pLoopTile, pLoopConstruction.getOwner(), pLoopTile.countConnections(pLoopConstruction.getType(), getPlayer(), false, false), true, false, true, false);

                                if (iValue <= 0)
                                {
                                    if (pLoopConstruction.getRepairMoneyCost(true) == 0)
                                    {
                                        iValue = gameServer().random().Next(10) + 1;
                                    }
                                }

                                if (iValue > iBestValue)
                                {
                                    pBestConstruction = pLoopConstruction;
                                    iBestValue = iValue;
                                }
                            }
                        }
                    }
                }

                if (pBestConstruction != null)
                {
                    List<int> aiResources = new List<int>();

                    for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                    {
                        aiResources.Add(pBestConstruction.getRepairResourceCost(eLoopResource));
                    }

                    AI_fundResources(0, MIN_PRICE_MULTIPLIER, true, false, aiResources, null, false, PlayerType.NONE);

                    pBestConstruction.repair();
                }
            }
        }

        protected virtual bool AI_isTeamPatenting(PatentType ePatent)
        {
            if (gameServer().isTeamGame())
            {
                foreach (PlayerServer pLoopPlayer in getAliveTeammatesAll())
                {
                    if (pLoopPlayer.isPatentStarted(ePatent))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected virtual bool AI_wantsAnyPatent()
        {
            for (PatentType eLoopPatent = 0; eLoopPatent < infos().patentsNum(); eLoopPatent++)
            {
                if (canPatent(eLoopPatent, false))
                {
                    if (!AI_isTeamPatenting(eLoopPatent))
                    {
                        if (AI_patentValue(eLoopPatent, false, true) > 0)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        protected virtual void AI_bestPatent(int iMinPriceMultiplier, ref PatentType eBestPatent, ref int iBestValue, int iCostMulitplier, bool bTestFunding, bool bSaveUpgrade)
        {
            for (PatentType eLoopPatent = 0; eLoopPatent < infos().patentsNum(); eLoopPatent++)
            {
                if (canPatent(eLoopPatent, false))
                {
                    if (!AI_isTeamPatenting(eLoopPatent))
                    {
                        List<int> aiResources = new List<int>();

                        for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                        {
                            aiResources.Add(getPatentResourceCost(eLoopPatent, eLoopResource));
                        }

                        if (!bTestFunding || AI_fundResources(0, iMinPriceMultiplier, bSaveUpgrade, true, aiResources, null, false, PlayerType.NONE))
                        {
                            int iValue = AI_patentValue(eLoopPatent, false, false);
                            int iCost = getPatentCost(eLoopPatent, true);

                            iCost *= iCostMulitplier;
                            iCost /= 100;

                            if (iValue > iCost)
                            {
                                if (iValue > iBestValue)
                                {
                                    eBestPatent = eLoopPatent;
                                    iBestValue = iValue;
                                }
                            }
                        }
                    }
                }
            }
        }

        const int MIN_PRICE_MULTIPLIER_PATENT = 40;

        protected virtual void AI_doPatent()
        {
            using (new UnityProfileScope("Player::AI_doPatent"))
            {
                if (!AI_isForceOrder(OrderType.PATENT))
                {
                    if (gameServer().random().Next(infos().personality(getPersonality()).maiOrderRoll[(int)OrderType.PATENT]) != 0)
                    {
                        return;
                    }
                }

                if (gameServer().isLastHalfDay())
                {
                    return;
                }

                if (getHQLevelInt() < 3)
                {
                    return;
                }

                if (AI_vulnerableMajorityBuyout())
                {
                    return;
                }

                bool bSaveUpgrade = true;
                int iMinPriceMultiplier = MIN_PRICE_MULTIPLIER_PATENT;

                if (getRealBuildingOrderCount(OrderType.PATENT) < getOrderCapacity(OrderType.PATENT))
                {
                    bSaveUpgrade = false;
                    iMinPriceMultiplier /= 10;
                }

                if (countOrders(OrderType.PATENT) < getOrderCapacity(OrderType.PATENT))
                {
                    AI_setForceOrder(OrderType.PATENT, false);

                    PatentType eBestPatent = PatentType.NONE;
                    int iBestValue = 0;

                    AI_bestPatent(iMinPriceMultiplier, ref eBestPatent, ref iBestValue, 50, true, bSaveUpgrade);

                    if (eBestPatent != PatentType.NONE)
                    {
                        List<int> aiResources = new List<int>();

                        for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                        {
                            aiResources.Add(getPatentResourceCost(eBestPatent, eLoopResource));
                        }

                        AI_fundResources(0, iMinPriceMultiplier, bSaveUpgrade, false, aiResources, null, false, PlayerType.NONE);
                        patent(eBestPatent);
                        return;
                    }
                }
            }
        }

        protected virtual void AI_bestResearch(int iMinPriceMultiplier, ref TechnologyType eBestTechnology, ref TechnologyLevelType eBestTechnologyLevel, ref int iBestValue, bool bTestFunding, bool bSaveUpgrade)
        {
            for (TechnologyType eLoopTechnology = 0; eLoopTechnology < infos().technologiesNum(); eLoopTechnology++)
            {
                TechnologyLevelType eLoopTechnologyLevel = getTechnologyLevelResearching(eLoopTechnology) + 1;

                if (canResearch(eLoopTechnology, eLoopTechnologyLevel, false))
                {
                    List<int> aiResources = new List<int>();

                    for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                    {
                        aiResources.Add(getResearchResourceCost(eLoopTechnology, eLoopTechnologyLevel, eLoopResource));
                    }

                    if (!bTestFunding || AI_fundResources(0, iMinPriceMultiplier, bSaveUpgrade, true, aiResources, null, false, PlayerType.NONE))
                    {
                        int iValue = 0;

                        for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                        {
                            int iOldProduction = 0;

                            if (infos().technology(eLoopTechnology).mabResourceProduction[(int)eLoopResource])
                            {
                                for (int i = 0; i < getNumBuildings(); i++)
                                {
                                    BuildingServer pLoopBuilding = gameServer().buildingServer(getBuildingList()[i]);

                                    iOldProduction += gameServer().resourceLevelMining(pLoopBuilding.getType(), eLoopResource, pLoopBuilding.tileServer(), getPlayer());
                                    iOldProduction += GameServer.resourceOutputTile(gameServer(), infos(), pLoopBuilding.getType(), eLoopResource, pLoopBuilding.tileServer());
                                }
                            }

                            if (iOldProduction > 0)
                            {
                                int iNewProduction = iOldProduction;

                                iNewProduction *= Math.Max(0, (infos().technologyLevel(eLoopTechnologyLevel).miModifier + 100));
                                iNewProduction /= 100;

                                int iSubValue = (((iNewProduction - iOldProduction) * gameServer().marketServer().getWholePrice(eLoopResource)) * ((infos().technology(eLoopTechnology).maiResourceCost[(int)eLoopResource] > 0) ? 3 : 2));

                                iSubValue /= Constants.RESOURCE_MULTIPLIER;

                                iValue += iSubValue;
                            }
                        }

                        iValue *= NUM_TURNS;

                        {
                            int iClaims = infos().technologyLevel(eLoopTechnologyLevel).maiHQClaims[(int)getHQ()];
                            if (iClaims != 0)
                            {
                                iValue += (iClaims * AI_claimValue());
                            }
                        }

                        if (iValue > getResearchCost(eLoopTechnology, eLoopTechnologyLevel, true))
                        {
                            if (iValue > iBestValue)
                            {
                                eBestTechnology = eLoopTechnology;
                                eBestTechnologyLevel = eLoopTechnologyLevel;
                                iBestValue = iValue;
                            }
                        }
                    }
                }
            }
        }

        const int MIN_PRICE_MULTIPLIER_RESEARCH = 30;

        protected virtual void AI_doResearch()
        {
            using (new UnityProfileScope("Player::AI_doResearch"))
            {
                if (!AI_isForceOrder(OrderType.RESEARCH))
                {
                    if (gameServer().random().Next(infos().personality(getPersonality()).maiOrderRoll[(int)OrderType.RESEARCH]) != 0)
                    {
                        return;
                    }
                }

                if (gameServer().isLastHalfDay())
                {
                    return;
                }

                if (getHQLevelInt() < 2)
                {
                    return;
                }

                if (AI_vulnerableMajorityBuyout())
                {
                    return;
                }

                bool bSaveUpgrade = true;
                int iMinPriceMultiplier = MIN_PRICE_MULTIPLIER_RESEARCH;

                if (getRealBuildingOrderCount(OrderType.RESEARCH) < getOrderCapacity(OrderType.RESEARCH))
                {
                    bSaveUpgrade = false;
                    iMinPriceMultiplier /= 10;
                }

                if (countOrders(OrderType.RESEARCH) < getOrderCapacity(OrderType.RESEARCH))
                {
                    AI_setForceOrder(OrderType.RESEARCH, false);

                    TechnologyType eBestTechnology = TechnologyType.NONE;
                    TechnologyLevelType eBestTechnologyLevel = TechnologyLevelType.NONE;
                    int iBestValue = 0;

                    AI_bestResearch(iMinPriceMultiplier, ref eBestTechnology, ref eBestTechnologyLevel, ref iBestValue, true, bSaveUpgrade);

                    if ((eBestTechnology != TechnologyType.NONE) && (eBestTechnologyLevel != TechnologyLevelType.NONE))
                    {
                        List<int> aiResources = new List<int>();

                        for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                        {
                            aiResources.Add(getResearchResourceCost(eBestTechnology, eBestTechnologyLevel, eLoopResource));
                        }

                        AI_fundResources(0, iMinPriceMultiplier, bSaveUpgrade, false, aiResources, null, false, PlayerType.NONE);
                        research(eBestTechnology);
                        return;
                    }
                }
            }
        }

        protected virtual void AI_bestEspionage(int iMinPriceMultiplier, ref EspionageType eBestEspionage, ref int iBestValue, bool bTestFunding, bool bSaveUpgrade, bool bSurplusOnly = false, bool bShortageOnly = false)
        {
            for (EspionageType eLoopEspionage = 0; eLoopEspionage < infos().espionagesNum(); eLoopEspionage++)
            {
                if (canHack(eLoopEspionage, false))
                {
                    if (((bSurplusOnly) ? infos().espionage(eLoopEspionage).mbSurplus : true) && ((bShortageOnly) ? !(infos().espionage(eLoopEspionage).mbSurplus) : true))
                    {
                        bool bValid = true;

                        foreach (OrderInfo pLoopOrderInfo in getOrderInfos(OrderType.HACK))
                        {
                            if ((EspionageType)(pLoopOrderInfo.miData1) == eLoopEspionage)
                            {
                                bValid = false;
                                break;
                            }
                        }

                        if (bValid)
                        {
                            int iCost = getHackCost(eLoopEspionage);

                            if (!bTestFunding || AI_fundResources(iCost, iMinPriceMultiplier, bSaveUpgrade, true, null, null, false, PlayerType.NONE))
                            {
                                int iValue = 0;

                                EventGameType eEventGame = infos().espionage(eLoopEspionage).meEventGame;

                                if (eEventGame != EventGameType.NONE)
                                {
                                    for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                    {
                                        int iChange = (infos().eventGame(eEventGame).maiResourceChange[(int)eLoopResource] * infos().eventGame(eEventGame).miTime) / (10 * Constants.RESOURCE_MULTIPLIER);
                                        if (iChange != 0)
                                        {
                                            iValue += (((-(iChange) * AI_getResourceRateAverage(eLoopResource) * gameServer().marketServer().getWholePrice(eLoopResource)) / Constants.RESOURCE_MULTIPLIER) * (int)(gameServer().getNumPlayers() - 1));

                                            for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameServer().getNumPlayers(); eLoopPlayer++)
                                            {
                                                PlayerServer pLoopPlayer = gameServer().playerServer(eLoopPlayer);

                                                if (pLoopPlayer != this)
                                                {
                                                    int iSubValue = ((-(iChange) * gameServer().playerServer(eLoopPlayer).AI_getResourceRateAverage(eLoopResource) * gameServer().marketServer().getWholePrice(eLoopResource)) / Constants.RESOURCE_MULTIPLIER);

                                                    if (pLoopPlayer.isSubsidiary())
                                                    {
                                                        for (PlayerType eOtherPlayer = 0; eOtherPlayer < gameServer().getNumPlayers(); eOtherPlayer++)
                                                        {
                                                            PlayerServer pOtherPlayer = gameServer().playerServer(eOtherPlayer);

                                                            int iNumShares = pOtherPlayer.getSharesOwned(eLoopPlayer);
                                                            if (iNumShares > 0)
                                                            {
                                                                if (pOtherPlayer.getTeam() == getTeam())
                                                                {
                                                                    iValue += ((iSubValue * iNumShares) / gameServer().getInitialShares());
                                                                }
                                                                else
                                                                {
                                                                    iValue -= ((iSubValue * iNumShares) / gameServer().getInitialShares());
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (pLoopPlayer.getTeam() == getTeam())
                                                        {
                                                            iValue += iSubValue;
                                                        }
                                                        else
                                                        {
                                                            iValue -= iSubValue;
                                                        }
                                                    }
                                                }
                                            }

                                            iValue /= (int)(gameServer().getNumPlayers());

                                            {
                                                const int HALF_MAX_PLAYERS = (Constants.MAX_NUM_PLAYERS / 2);

                                                int iActiveTeams = gameServer().countTeamsWinEligible();
                                                if (iActiveTeams > HALF_MAX_PLAYERS)
                                                {
                                                    iValue *= (HALF_MAX_PLAYERS);
                                                    iValue /= (HALF_MAX_PLAYERS + iActiveTeams);
                                                }
                                                else if (iActiveTeams == 2)
                                                {
                                                    iValue *= 3;
                                                    iValue /= 2;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (iValue > iCost)
                                {
                                    if (iValue > iBestValue)
                                    {
                                        eBestEspionage = eLoopEspionage;
                                        iBestValue = iValue;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        const int MIN_PRICE_MULTIPLIER_ESPIONAGE = 100;

        protected virtual void AI_doEspionage()
        {
            using (new UnityProfileScope("Player::AI_doEspionage"))
            {
                if (!AI_isForceOrder(OrderType.HACK))
                {
                    if (gameServer().random().Next(infos().personality(getPersonality()).maiOrderRoll[(int)OrderType.HACK]) != 0)
                    {
                        return;
                    }
                }

                if (gameServer().isLastHalfDay())
                {
                    return;
                }

                if (getHQLevelInt() < 3)
                {
                    return;
                }

                if (AI_vulnerableMajorityBuyout())
                {
                    return;
                }

                bool bSaveUpgrade = true;
                int iMinPriceMultiplier = MIN_PRICE_MULTIPLIER_ESPIONAGE;

                if (getRealBuildingOrderCount(OrderType.HACK) < getOrderCapacity(OrderType.HACK))
                {
                    bSaveUpgrade = false;
                    iMinPriceMultiplier /= 10;
                }

                if (countOrders(OrderType.HACK) < getOrderCapacity(OrderType.HACK))
                {
                    bool bSurplusOnly = false;
                    bool bShortageOnly = false;

                    if (infos().HQ(getHQ()).mbDoubleHack)
                    {
                        if (countShortageOrders() >= (getOrderCapacity(OrderType.HACK) / 2))
                        {
                            bSurplusOnly = true;
                        }
                        else if (countSurplusOrders() >= (getOrderCapacity(OrderType.HACK) / 2))
                        {
                            bShortageOnly = true;
                        }
                    }

                    AI_setForceOrder(OrderType.HACK, false);

                    EspionageType eBestEspionage = EspionageType.NONE;
                    int iBestValue = 0;

                    AI_bestEspionage(iMinPriceMultiplier, ref eBestEspionage, ref iBestValue, true, bSaveUpgrade, bSurplusOnly, bShortageOnly);

                    if (eBestEspionage != EspionageType.NONE)
                    {
                        AI_fundResources(getHackCost(eBestEspionage), iMinPriceMultiplier, bSaveUpgrade, false, null, null, false, PlayerType.NONE);
                        espionage(eBestEspionage);
                    }
                }
            }
        }

        public virtual void AI_espionageStarted(EspionageType eEspionage)
        {
            InfoEventGame pEventInfo = infos().eventGame(infos().espionage(eEspionage).meEventGame);

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                if (gameServer().isResourceValid(eLoopResource))
                {
                    int iChange = pEventInfo.maiResourceChange[(int)eLoopResource];
                    if (iChange != 0)
                    {
                        if (iChange > 0)
                        {
                            trade(eLoopResource, getWholeResourceStockpile(eLoopResource, true), false);
                        }
                        else if (iChange < 0)
                        {
                            trade(eLoopResource, ((-(iChange) * pEventInfo.miTime) / Constants.RESOURCE_MULTIPLIER), false);
                        }
                    }
                }
            }
        }

        protected void AI_doImport()
        {
            if (isImportInCooldown())
                return;

            int iBestSlot = -1;
            int iBestValue = 0;

            for (int iLoopSlot = 0; iLoopSlot < gameServer().getNumImportSlots(); iLoopSlot++)
            {
                if (canImport(iLoopSlot, true))
                {
                    int iProfit = gameServer().marketServer().calculateSellRevenue(gameServer().getImportSlotResource(iLoopSlot), infos().getGlobalInt("IMPORT_AMOUNT"), Constants.PRICE_MIN / Constants.PRICE_MULTIPLIER)
                        - getImportResourceCost(iLoopSlot, true) - getImportMoneyCost(iLoopSlot);
                    if (iProfit > iBestValue)
                    {
                        iBestValue = iProfit;
                        iBestSlot = iLoopSlot;
                    }
                }
            }

            if (iBestSlot >= 0)
                import(iBestSlot);
        }

        protected virtual void AI_doLaunch()
        {
            using (new UnityProfileScope("Player::AI_doLaunch"))
            {
                const int MIN_PRICE_MULTIPLIER = 10;

                if (!AI_isForceOrder(OrderType.LAUNCH))
                {
                    if (gameServer().random().Next(infos().personality(getPersonality()).maiOrderRoll[(int)OrderType.LAUNCH]) != 0)
                    {
                        return;
                    }
                }

                if (gameServer().isLastDay())
                {
                    bool bFound = false;

                    for (int i = 0; i < getNumBuildings(); i++)
                    {
                        BuildingServer pLoopBuilding = gameServer().buildingServer(getBuildingList()[i]);

                        if (infos().buildingClass(pLoopBuilding.getClass()).meOrderType == OrderType.LAUNCH)
                        {
                            if (gameServer().getTurnsLeft() < getOrderTurns(OrderType.LAUNCH, infos().Globals.LAUNCH_TIME, pLoopBuilding.tileServer()))
                            {
                                bFound = true;
                            }
                        }
                    }

                    if (!bFound)
                    {
                        return;
                    }
                }

                while (countOrders(OrderType.LAUNCH) < getOrderCapacity(OrderType.LAUNCH))
                {
                    AI_setForceOrder(OrderType.LAUNCH, false);

                    int iLaunchCost = 0;

                    for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                    {
                        int iQuantity = getLaunchResourceCost(eLoopResource, ResourceType.NONE);
                        if (iQuantity > 0)
                        {
                            iLaunchCost += gameServer().marketServer().calculateBuyCost(eLoopResource, iQuantity);
                        }
                    }

                    ResourceType eBestResource = ResourceType.NONE;
                    int iBestValue = 0;

                    for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                    {
                        if (canLaunch(eLoopResource, false))
                        {
                            List<int> aiResources = new List<int>();

                            for (ResourceType eOtherResource = 0; eOtherResource < infos().resourcesNum(); eOtherResource++)
                            {
                                aiResources.Add(getLaunchResourceCost(eOtherResource, eLoopResource));
                            }

                            int iQuantity = infos().resource(eLoopResource).miLaunchQuantity;

                            if (AI_fundResources(0, MIN_PRICE_MULTIPLIER, false, true, aiResources, null, false, PlayerType.NONE))
                            {
                                int iValue = (getLaunchRevenue(eLoopResource) - gameServer().marketServer().calculateBuyCost(eLoopResource, iQuantity) - iLaunchCost);
                                if (iValue > 1000)
                                {
                                    iValue *= Math.Max(1, (10 * getNumHQs()) + AI_getWholeResourceRateAverage(eLoopResource));
                                    iValue /= Math.Max(1, (10 * getNumHQs()));

                                    if (getWholeResourceStockpile(eLoopResource, true) > iQuantity)
                                    {
                                        iValue *= 9;
                                        iValue /= 8;
                                    }

                                    if (iValue > iBestValue)
                                    {
                                        eBestResource = eLoopResource;
                                        iBestValue = iValue;
                                    }
                                }
                            }
                        }
                    }

                    if (eBestResource != ResourceType.NONE)
                    {
                        List<int> aiResources = new List<int>();

                        for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                        {
                            aiResources.Add(getLaunchResourceCost(eLoopResource, eBestResource));
                        }

                        AI_fundResources(0, MIN_PRICE_MULTIPLIER, false, false, aiResources, null, false, PlayerType.NONE);

                        for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                        {
                            trade(eLoopResource, (aiResources[(int)eLoopResource] - getWholeResourceStockpile(eLoopResource, true)), false);
                        }

                        launch(eBestResource);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        public virtual void AI_forceBuildingOrder(BuildingServer pBuilding)
        {
            if (isHuman())
            {
                return;
            }

            OrderType eOrder = infos().buildingClass(pBuilding.getClass()).meOrderType;

            if (eOrder == OrderType.NONE)
            {
                return;
            }

            AI_setForceOrder(eOrder, true);
        }

        protected virtual void AI_doTeam()
        {
            using (new UnityProfileScope("Player::AI_doTeam"))
            {
                if (gameServer().random().Next(10) != 0)
                {
                    return;
                }

                if (!(gameServer().isTeamGame()))
                {
                    return;
                }

                foreach (InfoResource pLoopResource in infos().resources())
                {
                    setTeamShareResource(pLoopResource.meType, ((AI_getResourceRateAverage(pLoopResource.meType) > 0) || mustTeamShareResource(pLoopResource.meType)));
                }
            }
        }

        protected virtual int AI_patentValue(PatentType ePatent, bool bAuction, bool bSkipBase)
        {
            int iValue = 0;

            {
                int iCut = infos().patent(ePatent).miDebtCut;
                if (iCut != 0)
                {
                    for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameServer().getNumPlayers(); eLoopPlayer++)
                    {
                        PlayerServer pLoopPlayer = gameServer().playerServer(eLoopPlayer);

                        int iInterestPayment = -((pLoopPlayer.getDebt() * pLoopPlayer.getInterestRate()) / 100);

                        if (pLoopPlayer.getTeam() == getTeam())
                        {
                            iInterestPayment /= gameServer().getNumTeams();
                        }

                        iValue += ((iInterestPayment * iCut) / 100);
                    }
                }
            }

            {
                int iModifier = infos().patent(ePatent).miEntertainmentModifier;
                if (iModifier != 0)
                {
                    int iPotentialEntertainment = 0;

                    foreach (InfoBuilding pLoopBuilding in infos().buildings())
                    {
                        if (pLoopBuilding.miEntertainment > 0)
                        {
                            if (canEverConstruct(pLoopBuilding.meType, true, true))
                            {
                                iPotentialEntertainment = Math.Max(iPotentialEntertainment, gameServer().entertainmentProfit(pLoopBuilding.meType, null, getPlayer(), true, true));
                            }
                        }
                    }

                    for (int i = 0; i < getNumBuildings(); i++)
                    {
                        iPotentialEntertainment += gameServer().buildingServer(getBuildingList()[i]).getEntertainmentProfit();
                    }

                    iValue += (((iPotentialEntertainment * NUM_TURNS) * iModifier) / 100);
                }
            }

            {
                int iModifier = infos().patent(ePatent).miPowerConsumptionModifier;
                if (iModifier != 0)
                {
                    iValue += (getNumBuildings() * -(iModifier));

                    for (int i = 0; i < getNumBuildings(); i++)
                    {
                        BuildingServer pLoopBuilding = gameServer().buildingServer(getBuildingList()[i]);

                        int iConsumption = gameServer().powerConsumption(pLoopBuilding.getType(), pLoopBuilding.tileServer(), pLoopBuilding.getOwner());
                        if (iConsumption > 0)
                        {
                            iValue += (-(iConsumption) * gameServer().marketServer().getWholePrice(getEnergyResource()) * iModifier * NUM_TURNS) / (Constants.RESOURCE_MULTIPLIER * 100);
                        }
                    }
                }
            }

            {
                int iModifier = infos().patent(ePatent).miConnectedHQPowerProductionModifier;
                if (iModifier != 0)
                {
                    for (int i = 0; i < getNumBuildings(); i++)
                    {
                        BuildingServer pLoopBuilding = gameServer().buildingServer(getBuildingList()[i]);

                        if (pLoopBuilding.tileServer().isConnectedToHQ())
                        {
                            int iProduction = pLoopBuilding.resourceOutput(infos().Globals.ENERGY_RESOURCE, true);
                            if (iProduction > 0)
                            {
                                iValue += (iProduction * gameServer().marketServer().getWholePrice(infos().Globals.ENERGY_RESOURCE) * iModifier * NUM_TURNS) / (Constants.RESOURCE_MULTIPLIER * 100);
                            }
                        }
                    }
                }
            }

            {
                int iModifier = infos().patent(ePatent).miAdjacentHQSabotageModifier;
                if (iModifier != 0)
                {
                    for (int i = 0; i < getNumBuildings(); i++)
                    {
                        BuildingServer pLoopBuilding = gameServer().buildingServer(getBuildingList()[i]);

                        if (pLoopBuilding.tileServer().adjacentToHQ(pLoopBuilding.tileServer().getOwner()))
                        {
                            if (infos().buildingClass(pLoopBuilding.getClass()).meOrderType != OrderType.NONE)
                            {
                                iValue += 150 * -(iModifier);
                            }
                            else
                            {
                                iValue += 50 * -(iModifier);
                            }
                        }
                    }
                }
            }

            if (infos().patent(ePatent).mbBorehole)
            {
                iValue += ((infos().HQLevelsNum() - 1) - getHQLevel()) * 1000;

                for (int i = 0; i < getNumBuildings(); i++)
                {
                    BuildingServer pLoopBuilding = gameServer().buildingServer(getBuildingList()[i]);

                    if (pLoopBuilding.tileServer().onOrAdjacentToGeothermal())
                    {
                        iValue += 500;

                        int iConsumption = gameServer().powerConsumption(pLoopBuilding.getType(), pLoopBuilding.tileServer(), pLoopBuilding.getOwner());
                        if (iConsumption > 0)
                        {
                            iValue += (-(iConsumption) * gameServer().marketServer().getWholePrice(getEnergyResource()) * NUM_TURNS) / Constants.RESOURCE_MULTIPLIER;
                        }
                    }
                }
            }

            if (infos().patent(ePatent).mbRecycleScrap)
            {
                iValue += ((int)getNumBuildings() * 1000);
            }

            if (infos().patent(ePatent).mbAdjacentMining)
            {
                iValue += ((int)getNumBuildings() * 200);

                for (int i = 0; i < getNumBuildings(); i++)
                {
                    BuildingServer pLoopBuilding = gameServer().buildingServer(getBuildingList()[i]);
                    TileServer pBuildingTile = pLoopBuilding.tileServer();

                    for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                    {
                        int iMiningNormal = gameServer().resourceMiningLevel(pLoopBuilding.getType(), eLoopResource, pBuildingTile.getResourceLevel(eLoopResource, false), pBuildingTile.getTerrainNoIce(), pLoopBuilding.getOwner(), infos().building(pLoopBuilding.getType()).maiResourceMining[(int)eLoopResource], pLoopBuilding.getConnections(), true);
                        int iMiningAdjacent = gameServer().resourceMiningLevel(pLoopBuilding.getType(), eLoopResource, pBuildingTile.getResourceLevel(eLoopResource, true), pBuildingTile.getTerrainNoIce(), pLoopBuilding.getOwner(), infos().building(pLoopBuilding.getType()).maiResourceMining[(int)eLoopResource], pLoopBuilding.getConnections(), true);

                        iValue += ((iMiningAdjacent - iMiningNormal) * gameServer().marketServer().getWholePrice(eLoopResource) * NUM_TURNS) / Constants.RESOURCE_MULTIPLIER;

                        if (isBuildingIgnoresInputAdjacent(pLoopBuilding.getType(), eLoopResource, pBuildingTile, true) != isBuildingIgnoresInputAdjacent(pLoopBuilding.getType(), eLoopResource, pBuildingTile, false))
                        {
                            int iInput = pLoopBuilding.resourceInput(eLoopResource, true);
                            if (iInput > 0)
                            {
                                iValue += (iInput * gameServer().marketServer().getWholePrice(eLoopResource) * NUM_TURNS) / Constants.RESOURCE_MULTIPLIER;
                            }
                        }
                    }
                }
            }

            if(infos().patent(ePatent).mbCaveMining)
            {
                iValue += getRemainingLevelClaims() * 100;
                int iCurrentCaves = maTiles.Where(x => gameServer().tileServer(x).getTerrain() == infos().Globals.CAVE_TERRAIN).Count();

                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    if (infos().resource(eLoopResource).maiLocationAppearanceProb[(int)gameServer().getLocation()] > 0)
                        iValue += iCurrentCaves * gameServer().marketServer().getWholePrice(eLoopResource) * NUM_TURNS;
                }
            }

            if (infos().patent(ePatent).mbTeleportation)
            {
                iValue += 8000 + ((int)getNumBuildings() * 200);

                for (int i = 0; i < getNumUnits(); i++)
                {
                    UnitServer pLoopUnit = gameServer().unitServer(getUnitList()[i]);

                    if (pLoopUnit.getType() == infos().Globals.SHIP_UNIT)
                    {
                        int iGasRate = getGas(pLoopUnit.getType());
                        if (iGasRate < 0)
                        {
                            iValue += -(iGasRate * gameServer().marketServer().getWholePrice(getGasResource()) * NUM_TURNS) / Constants.RESOURCE_MULTIPLIER;
                        }
                    }
                }
            }

            {
                ResourceType eBuildingFreeResource = infos().patent(ePatent).meBuildingFreeResource;

                if (eBuildingFreeResource != ResourceType.NONE)
                {
                    iValue += ((int)getNumBuildings() * 100);

                    iValue += ((gameServer().marketServer().getWholePrice(eBuildingFreeResource) + infos().resource(eBuildingFreeResource).miMarketPrice) * NUM_TURNS);

                    for (int i = 0; i < getNumBuildings(); i++)
                    {
                        BuildingServer pLoopBuilding = gameServer().buildingServer(getBuildingList()[i]);

                        int iInput = pLoopBuilding.resourceInput(eBuildingFreeResource, true);
                        if (iInput > 0)
                        {
                            iValue += (iInput * gameServer().marketServer().getWholePrice(eBuildingFreeResource) * NUM_TURNS) / Constants.RESOURCE_MULTIPLIER;
                        }
                    }
                }
            }

            {
                ResourceType eAlternateGasResource = infos().patent(ePatent).meAlternateGasResource;

                if (eAlternateGasResource != ResourceType.NONE)
                {
                    int iTotalGas = 0;

                    if (!isTeleportation())
                    {
                        for (int i = 0; i < getNumUnits(); i++)
                        {
                            UnitServer pLoopUnit = gameServer().unitServer(getUnitList()[i]);

                            if (pLoopUnit.getType() == infos().Globals.SHIP_UNIT)
                            {
                                iTotalGas += getGas(pLoopUnit.getType());
                            }
                        }
                    }

                    int iSubValue = 0;

                    iSubValue += (-(iTotalGas) * gameServer().marketServer().getWholePrice(getGasResource())) / Constants.RESOURCE_MULTIPLIER;
                    iSubValue -= (-(iTotalGas) * gameServer().marketServer().getWholePrice(eAlternateGasResource)) / Constants.RESOURCE_MULTIPLIER;

                    iValue += (iSubValue * NUM_TURNS);

                    iValue += ((Math.Max(0, (gameServer().marketServer().getWholePrice(getGasResource()) - 20)) / 2) * NUM_TURNS);
                }
            }

            {
                ResourceType eAlternatePowerResource = infos().patent(ePatent).meAlternatePowerResource;

                if (eAlternatePowerResource != ResourceType.NONE)
                {
                    int iTotalConsumption = 0;

                    for (int i = 0; i < getNumBuildings(); i++)
                    {
                        BuildingServer pLoopBuilding = gameServer().buildingServer(getBuildingList()[i]);

                        iTotalConsumption += gameServer().powerConsumption(pLoopBuilding.getType(), pLoopBuilding.tileServer(), pLoopBuilding.getOwner());
                    }

                    int iSubValue = 0;

                    iSubValue += (iTotalConsumption * gameServer().marketServer().getWholePrice(getEnergyResource())) / Constants.RESOURCE_MULTIPLIER;
                    iSubValue -= (iTotalConsumption * gameServer().marketServer().getWholePrice(eAlternatePowerResource)) / Constants.RESOURCE_MULTIPLIER;

                    iValue += (iSubValue * NUM_TURNS);

                    iValue += ((Math.Max(0, (gameServer().marketServer().getWholePrice(getEnergyResource()) - 20)) / 2) * NUM_TURNS);
                }
            }

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                if (gameServer().isResourceValid(eLoopResource))
                {
                    iValue += (infos().patent(ePatent).maiResourceCapacity[(int)eLoopResource] * gameServer().marketServer().getWholePrice(eLoopResource));
                }
            }

            for (BuildingType eLoopBuilding = 0; eLoopBuilding < infos().buildingsNum(); eLoopBuilding++)
            {
                if (infos().patent(ePatent).mabBuildingAlwaysOn[(int)eLoopBuilding])
                {
                    if (canEverConstruct(eLoopBuilding, true, true))
                    {
                        iValue += (Math.Max(0, AI_buildingValueTotal(eLoopBuilding, null, getPlayer(), 0, true, true, false, false)) * NUM_TURNS);
                        iValue += (20 * NUM_TURNS);
                    }
                }
            }

            for (BuildingType eLoopBuilding = 0; eLoopBuilding < infos().buildingsNum(); eLoopBuilding++)
            {
                if (infos().patent(ePatent).mabBuildingClassBoost[(int)(infos().building(eLoopBuilding).meClass)])
                {
                    if (canEverConstruct(eLoopBuilding, true, true))
                    {
                        iValue += (Math.Max(0, AI_buildingValueTotal(eLoopBuilding, null, getPlayer(), 0, true, true, false, false)) * NUM_TURNS);
                        iValue += (40 * NUM_TURNS);
                    }
                }
            }

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                ResourceType eReplaceResource = infos().patent(ePatent).maeResourceReplace[(int)eLoopResource];

                if (eReplaceResource != ResourceType.NONE)
                {
                    iValue += (Math.Max(0, (gameServer().marketServer().getWholePrice(eLoopResource) - gameServer().marketServer().getWholePrice(eReplaceResource))) * NUM_TURNS);
                }
            }

            for (BuildingClassType eLoopBuildingClass = 0; eLoopBuildingClass < infos().buildingClassesNum(); eLoopBuildingClass++)
            {
                IceType eIgnoreInputIce = infos().patent(ePatent).maeIgnoreInputIce[(int)eLoopBuildingClass];

                if (eIgnoreInputIce != IceType.NONE)
                {
                    for (BuildingType eLoopBuilding = 0; eLoopBuilding < infos().buildingsNum(); eLoopBuilding++)
                    {
                        if (infos().building(eLoopBuilding).meClass == eLoopBuildingClass)
                        {
                            if (canEverConstruct(eLoopBuilding, true, true))
                            {
                                iValue += (Math.Max(0, AI_buildingValueTotal(eLoopBuilding, null, getPlayer(), 0, false, true, false, true)) * NUM_TURNS);
                                iValue += (10 * NUM_TURNS);
                            }
                        }
                    }
                }
            }

            if (!bSkipBase)
            {
                if (bAuction)
                {
                    foreach (InfoResource pLoopResource in infos().resources())
                    {
                        iValue += ((getPatentResourceCost(ePatent, pLoopResource.meType) * Math.Min(100, gameServer().marketServer().getWholePrice(pLoopResource.meType))) / ((iValue == 0) ? 8 : 2));
                    }
                }

                iValue += 4000;
            }

            iValue *= Math.Max(0, (infos().personality(getPersonality()).maiPatentValueModifier[(int)ePatent] + 100));
            iValue /= 100;

            return iValue;
        }

        public virtual int AI_tileValue(TileServer pTile)
        {
            int iValue = 0;

            for (BuildingType eLoopBuilding = 0; eLoopBuilding < infos().buildingsNum(); eLoopBuilding++)
            {
                if ((pTile.getBuildingType() == eLoopBuilding) ||
                    (canConstructPlayer(eLoopBuilding, false) && gameServer().canTileHaveBuilding(pTile, eLoopBuilding, getPlayer())))
                {
                    int iRevenue = gameServer().calculateRevenue(eLoopBuilding, pTile, getPlayer(), pTile.countConnections(eLoopBuilding, getPlayer(), true, false), true, true, 100, 100, 100, false);
                    if (iRevenue > 0)
                    {
                        iValue += (iRevenue * 50);
                    }

                    OrderType eOrder = infos().buildingClass(infos().building(eLoopBuilding).meClass).meOrderType;
                    if (eOrder != OrderType.NONE)
                    {
                        for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                        {
                            TileServer pAdjacentTile = gameServer().tileServerAdjacent(pTile, eLoopDirection);

                            if (pAdjacentTile != null)
                            {
                                if (pAdjacentTile.isModule())
                                {
                                    iValue += (gameServer().getModuleOrderModifier(pAdjacentTile.getModule(), eOrder) * 200);
                                }
                            }
                        }
                    }
                }
            }

            if (iValue > 0)
            {
                iValue += 2000;

                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    if (getHQ() != HQType.NONE)
                    {
                        iValue += ((int)(pTile.getResourceLevel(eLoopResource, false)) * (infos().HQ(getHQ()).maiAIResourceWeight[(int)eLoopResource] + infos().HQ(getHQ()).maaiAILocationResourceWeightModifier[(int)gameServer().getLocation()][(int)eLoopResource]) * 100);
                    }
                }

                if (pTile.isClaimed())
                {
                    iValue *= 2;
                    iValue /= 3;
                }

                return iValue;
            }
            else
            {
                return 0;
            }
        }

        protected virtual int AI_claimValue()
        {
            int iValue = 2000;

            iValue += ((gameServer().random().Next(10) + gameServer().random().Next(10)) * 500);

            iValue += (getNumBuildings() * 500);

            if (getClaims() == 0)
            {
                iValue *= 4;
                iValue /= 3;
            }

            iValue *= Math.Max(0, (infos().personality(getPersonality()).miClaimValueModifier + 100));
            iValue /= 100;

            return iValue;
        }

        protected virtual int AI_perkValue(PerkType ePerk)
        {
            int iValue = infos().perk(ePerk).miCost / 20;

            BuildingClassType ePerkBuildingClass = infos().perk(ePerk).meBuildingClassLevel;
            OrderType eOrder = infos().buildingClass(ePerkBuildingClass).meOrderType;

            foreach (InfoBuilding pLoopBuilding in infos().buildings())
            {
                if (pLoopBuilding.meClass == ePerkBuildingClass)
                {
                    BuildingType eLoopBuilding = pLoopBuilding.meType;

                    if (canEverConstruct(eLoopBuilding, false, true))
                    {
                        if (eOrder != OrderType.NONE)
                        {
                            int iSubValue = AI_buildingValueOrders(eLoopBuilding, getPlayer(), null);

                            if (getBuildingClassLevel(pLoopBuilding.meClass) == 0)
                            {
                                iSubValue *= 20;
                            }
                            else
                            {
                                if (eOrder == OrderType.RESEARCH)
                                {
                                    iSubValue *= 5;
                                }
                                else if (eOrder == OrderType.LAUNCH)
                                {
                                    iSubValue *= 10;
                                }
                            }

                            iValue += iSubValue;
                        }
                        else
                        {
                            int iSubValue = 1000;

                            {
                                int iNet = gameServer().calculateRevenue(eLoopBuilding, null, getPlayer(), 0, true, true, 0, 0, 100, false);
                                if (iNet > 0)
                                {
                                    iSubValue += (iNet * 50);
                                }
                            }

                            iSubValue *= (4 + getRealBuildingCount(eLoopBuilding) + getRealConstructionCount(eLoopBuilding));
                            iSubValue /= 4;

                            iValue += iSubValue;
                        }
                    }
                }
            }

            iValue *= Math.Max(0, infos().personality(corporation().mePersonality).maiPerkValueModifier[(int)ePerk] + 100);
            iValue /= 100;

            return iValue;
        }
    }
}