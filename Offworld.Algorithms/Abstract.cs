using System;
using System.Linq;
using System.Collections.Generic;
using Offworld.AppCore;
using Offworld.GameCore;
using UnityEngine;

namespace Offworld.Algorithms
{
    public abstract class Abstract : PlayerServer
    {
        public Abstract(GameClient pGame, bool liveGame, bool debugMode) : base(pGame)
        {
            LiveGame = liveGame;
            DebugMode = debugMode;
        }
        public readonly bool LiveGame;
        public readonly bool DebugMode;
        public System.Random Random => gameServer().random();

        // YOUR AI SHOULD IMPLEMENT THIS
        public abstract void AI_doActions(bool is_auction);

        // logging utils
        private void logAction(string action, string info = "")
        {
            if (DebugMode)
            {
                string log = $"Turn: {gameServer().getTurnCount()}, Player: {getPlayer()}, Action: {action}";
                if (info != "")
                    log += $", Info: ({info})";
                Debug.Log(log);
                if (LiveGame)
                    AppGlobals.GameHUDHelpers.DisplayChatMessage(getPlayer(), false, log);
            }
        }
        public override int trade(ResourceType eResource, int iQuantity, bool bManual)
        {
            if (DebugMode)
                logAction("TRADE", $"Resource: {TEXT(infos().resource(eResource).meName)}, Quantity: {iQuantity}");
            return base.trade(eResource, iQuantity, bManual); ;
        }
        public override void buyShares(PlayerType ePlayer)
        {
            if (DebugMode)
                logAction("BUY_SHARES", $"TargetPlayer: {ePlayer}");
            base.buyShares(ePlayer);
        }
        public override void sellShares(PlayerType ePlayer)
        {
            if (DebugMode)
                logAction("SELL_SHARES", $"TargetPlayer: {ePlayer}");
            base.sellShares(ePlayer);
        }
        public override void buyout(PlayerType ePlayer)
        {
            if (DebugMode)
                logAction("BUYOUT", $"TargetPlayer: {ePlayer}");
            base.buyout(ePlayer);
        }
        public override void payDebtAll()
        {
            if (DebugMode)
                logAction("PAY_ALL_DEBT");
            base.payDebtAll();
        }
        public override void payDebt()
        {
            if (DebugMode)
                logAction("PAY_DEBT");
            base.payDebt();
        }
        public override void toggleAutoPayDebt()
        {
            if (DebugMode)
                logAction("TOGGLE_AUTO_PAY_DEBT");
            base.toggleAutoPayDebt();
        }
        public override void scan(TileServer pTile)
        {
            if (DebugMode)
                logAction("SCAN", $"Tile: {pTile.getID()}");
            base.scan(pTile);
        }
        public override void sabotage(TileServer pTile, SabotageType eSabotage)
        {
            if (DebugMode)
                logAction("SABOTAGE", $"Tile: {pTile.getID()}, Sabotage: {TEXT(infos().sabotage(eSabotage).meName)}");
            base.sabotage(pTile, eSabotage);
        }
        public override HQServer found(TileServer pTile, HQType eHQ)
        {
            if (DebugMode)
                logAction("FOUND", $"Tile: {pTile.getID()}, HQ: {TEXT(infos().HQ(eHQ).meName)}");
            return base.found(pTile, eHQ);
        }
        public override void upgrade()
        {
            if (DebugMode)
                logAction("UPGRADE_HQ");
            base.upgrade();
        }
        public override void startClaim(TileServer pTile)
        {
            if (DebugMode)
                logAction("CLAIM_TILE", $"Tile: {pTile.getID()}");
            base.startClaim(pTile);
        }
        public override void returnClaim(TileServer pTile, bool bSabotage)
        {
            if (DebugMode)
                logAction("RETURN_CLAIM", $"Tile: {pTile.getID()}, IsSabotage: {bSabotage}");
            base.returnClaim(pTile, bSabotage);
        }
        public override void cancelConstruct(TileServer pTile)
        {
            if (DebugMode)
                logAction("CANCEL_CONSTRUCT", $"Tile: {pTile.getID()}");
            base.cancelConstruct(pTile);
        }
        public override bool construct(TileServer pTile, BuildingType eBuilding)
        {
            if (DebugMode)
                logAction("CONSTRUCT", $"Tile: {pTile.getID()}, Building: {TEXT(infos().building(eBuilding).meName)}");
            return base.construct(pTile, eBuilding);
        }
        public override void sellAllResources(bool bManual)
        {
            if (DebugMode)
                logAction("SELL_ALL_RESOURCES");
            base.sellAllResources(bManual);
        }
        public override void toggleHoldResource(ResourceType eIndex)
        {
            if (DebugMode)
                logAction("TOGGLE_HOLD_RESOURCE", $"Resource: {TEXT(infos().resource(eIndex).meName)}");
            base.toggleHoldResource(eIndex);
        }
        public override void toggleTeamShareResource(ResourceType eIndex)
        {
            if (DebugMode)
                logAction("TOGGLE_SHARE_RESOURCE", $"Resource: {TEXT(infos().resource(eIndex).meName)}");
            base.toggleTeamShareResource(eIndex);
        }
        public override void setTeamShareAllResources(bool bNewValue)
        {
            if (DebugMode)
                logAction("TOGGLE_SHARE_ALL_RESOURCES");
            base.setTeamShareAllResources(bNewValue);
        }
        public override void scrapAll(TileServer pTile)
        {
            if (DebugMode)
                logAction("CONSTRUCT", $"Tile: {pTile.getID()}");
            base.scrapAll(pTile);
        }
        public override void sendResourcesBuilding(BuildingServer pBuilding)
        {
            if (DebugMode)
                logAction("SEND_RESOURCES_ALL", $"Building: {TEXT(infos().building(pBuilding.getType()).meName)}");
            base.sendResourcesBuilding(pBuilding);
        }
        public override void toggleAutoOffBuildings(BuildingServer pBuilding)
        {
            if (DebugMode)
                logAction("TOGGLE_AUTO_OFF_ALL", $"Building: {TEXT(infos().building(pBuilding.getType()).meName)}");
            base.toggleAutoOffBuildings(pBuilding);
        }
        public override void toggleOnOffBuildings(BuildingServer pBuilding)
        {
            if (DebugMode)
                logAction("TOGGLE_OFF_ALL", $"Building: {TEXT(infos().building(pBuilding.getType()).meName)}");
            base.toggleOnOffBuildings(pBuilding);
        }
        public override void toggleOnOffEverything()
        {
            if (DebugMode)
                logAction("TOGGLE_OFF_EVERYTHING");
            base.toggleOnOffEverything();
        }
        public override void toggleAutoSupplyBuildingInput(BuildingType eIndex)
        {
            if (DebugMode)
                logAction("TOGGLE_AUTO_SUPPLY_BUILDING", $"Building: {TEXT(infos().building(eIndex).meName)}");
            base.toggleAutoSupplyBuildingInput(eIndex);
        }
        public override void sendPatent(PatentType ePatent, PlayerType ePlayer)
        {
            if (DebugMode)
                logAction("SEND_PATENT", $"Patent: {TEXT(infos().patent(ePatent).meName)}, TargetPlayer: {ePlayer}");
            base.sendPatent(ePatent, ePlayer);
        }
        public override void patent(PatentType ePatent)
        {
            if (DebugMode)
                logAction("PATENT", $"Patent: {TEXT(infos().patent(ePatent).meName)}");
            base.patent(ePatent);
        }
        public override void research(TechnologyType eTechnology)
        {
            if (DebugMode)
                logAction("RESEARCH", $"Tech: {TEXT(infos().technology(eTechnology).meName)}");
            base.research(eTechnology);
        }
        public override void espionage(EspionageType eEspionage)
        {
            if (DebugMode)
                logAction("ESPIONAGE", $"Espionage: {TEXT(infos().espionage(eEspionage).meName)}");
            base.espionage(eEspionage);
        }
        public override void toggleAutoLaunchResource(ResourceType eIndex)
        {
            if (DebugMode)
                logAction("TOGGLE_AUTO_LAUNCH", $"Resource: {TEXT(infos().resource(eIndex).meName)}");
            base.toggleAutoLaunchResource(eIndex);
        }
        public override void launch(ResourceType eResource)
        {
            if (DebugMode)
                logAction("LAUNCH", $"Resource: {TEXT(infos().resource(eResource).meName)}");
            base.launch(eResource);
        }
        public override void increaseBid()
        {
            if (DebugMode)
                logAction("AUCTION_BID");
            base.increaseBid();
        }
        public override void cancelOrder(OrderType eOrder, int iIndex)
        {
            if (DebugMode)
                logAction("CANCEL_ORDER", $"Order: {TEXT(infos().order(eOrder).meName)}, Index: {iIndex}");
            base.cancelOrder(eOrder, iIndex);
        }
        public override void blackMarket(BlackMarketType eBlackMarket)
        {
            if (DebugMode)
                logAction("BLACK_MARKET", $"BlackMarket: {TEXT(infos().blackMarket(eBlackMarket).meName)}");
            base.blackMarket(eBlackMarket);
        }
        public override void buyColonyShares()
        {
            if (DebugMode)
                logAction("BUY_COLONY_STOCK");
            base.buyColonyShares();
        }
        public override void buyColonyModule(ModuleType eModule)
        {
            if (DebugMode)
                logAction("BUY_COLONY_MODULE", $"Module: {TEXT(infos().module(eModule).meName)}");
            base.buyColonyModule(eModule);
        }
        public override void supplyWholesale(int iSlot)
        {
            if (DebugMode)
                logAction("SUPPLY_WHOLESALE", $"Slot: {iSlot}");
            base.supplyWholesale(iSlot);
        }
        public override void makeConcede()
        {
            if (DebugMode)
                logAction("CONCEDE");
            base.makeConcede();
        }


        // redirecting to AI_doActions
        public override bool AI_doAuction()
        {
            if (!isSubsidiary())
                AI_doActions(true);
            return false;
        }
        public override void AI_doUpdate()
        {
            AI_updateRates();

            if (isHuman())
            {
                return;
            }

            if (isSubsidiary())
            {
                if (isHQFounded())
                {
                    sellAllResources(false);

                    AI_doConstruct(16);

                    AI_turnOffBuildings();

                    AI_doRepair();

                    AI_doLaunch();

                    AI_doUpgradeHQ(false);

                    payDebt();
                }
                return;
            }

            AI_doActions(false);
        }


        // patching buggy AI code
        public override void AI_quip(PlayerType ePlayer, TextType eText) { }
        public override int AI_fundResourcesPercent(int iCost, int iMinPriceMultiplier, bool bSaveUpgrade, bool bTest, List<int> aiResources, List<int> aiExtraResources, bool bSellShares, PlayerType eSkipPlayer)
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

                    //if (iSaved == 0 && maiResourceValue[(int)eLoopResource] >= 0)
                    //{
                    //    iProfit += maiResourceValue[(int)eLoopResource];
                    //    tradeQuantities[(int)eLoopResource] = iQuantity;
                    //    continue;
                    //}

                    iQuantity -= iSaved;

                    if (iQuantity > 0)
                    {
                        tradeQuantities[(int)eLoopResource] = iQuantity;

                        //if (iSaved == iUpgrade && maiResourceValueSaveUpgrade[(int)eLoopResource] >= 0)
                        //{
                        //    iProfit += maiResourceValueSaveUpgrade[(int)eLoopResource];
                        //    continue;
                        //}

                        int iResourceProfit = gameServer().marketServer().calculateSellRevenue(eLoopResource, tradeQuantities[(int)eLoopResource], ((infos().resource(eLoopResource).miMarketPrice * iMinPriceMultiplier) / 100));
                        iProfit += iResourceProfit;

                        //if (bSaveUpgrade && iUpgrade == iSaved)
                        //{
                        //    maiResourceValueSaveUpgrade[(int)eLoopResource] = iResourceProfit;
                        //}
                        //else if (iSaved == 0)
                        //{
                        //    maiResourceValue[(int)eLoopResource] = iResourceProfit;
                        //}
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
                                //maiResourceValue[(int)eLoopResource] -= iTotalProfit;
                                //maiResourceValueSaveUpgrade[(int)eLoopResource] -= iTotalProfit;
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
    }
}
