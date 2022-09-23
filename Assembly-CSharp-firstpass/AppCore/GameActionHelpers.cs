using System;
using Offworld.GameCore;

namespace Offworld.AppCore
{
    public static class GameActionHelpers
    {
        public static void sendTrade(ResourceType eResource, int iQuantity)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.TRADE);

            pAction.addLastValue((int)eResource);
            pAction.addLastValue(iQuantity);

            Globals.Network.sendAction(pAction);
        }

        public static void sendStock(PlayerType ePlayer, int iQuantity)
        {
            GameAction pAction = new GameAction();

			pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.STOCK);

            pAction.addLastValue((int)ePlayer);
            pAction.addLastValue(iQuantity);

            Globals.Network.sendAction(pAction);
        }

        public static void sendBuyout(PlayerType ePlayer)
        {
            GameAction pAction = new GameAction();

			pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.BUYOUT);

            pAction.addLastValue((int)ePlayer);

            Globals.Network.sendAction(pAction);
        }

        public static void sendPayDebt(bool bAll)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.PAY_DEBT);

            pAction.addLastValue((bAll) ? 1 : 0);

            Globals.Network.sendAction(pAction);
        }

        public static void sendAutoPayDebt()
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.AUTO_PAY_DEBT);

            Globals.Network.sendAction(pAction);
        }

        public static void sendScan(TileClient pTile)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.SCAN);

            pAction.addLastValue(pTile.getID());

            Globals.Network.sendAction(pAction);
        }

        public static void sendSabotage(TileClient pTile, SabotageType eSabotage)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.SABOTAGE);

            pAction.addLastValue(pTile.getID());
            pAction.addLastValue((int)eSabotage);

            Globals.Network.sendAction(pAction);
        }

        public static void sendFound(TileClient pTile, HQType eHQ)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.FOUND);

            pAction.addLastValue(pTile.getID());
            pAction.addLastValue((int)eHQ);

            Globals.Network.sendAction(pAction);
        }

        public static void sendUpgrade(HQClient pHQ)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.UPGRADE);

            Globals.Network.sendAction(pAction);
        }

        public static void sendUpgradeBuilding(int iBuildingID)
        {
            GameAction pAction = new GameAction();
            pAction.setItem(ItemType.UPGRADE_BUILDING);
            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.addLastValue(iBuildingID);
            Globals.Network.sendAction(pAction);
        }

        public static void sendCraftBlackMarket(BlackMarketType eBlackMarket)
        {
            GameAction pAction = new GameAction();
            pAction.setItem(ItemType.CRAFT_BLACK_MARKET);
            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.addLastValue((int)eBlackMarket);
            pAction.addLastValue(1);
            Globals.Network.sendAction(pAction);
        }

        public static void sendToggleAutoCraftBlackMarket(BlackMarketType eBlackMarket)
        {
            GameAction pAction = new GameAction();
            pAction.setItem(ItemType.CRAFT_BLACK_MARKET);
            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.addLastValue((int)eBlackMarket);
            pAction.addLastValue(2);
            Globals.Network.sendAction(pAction);
        }

        public static void sendCancelCraftBlackMarket(BlackMarketType eBlackMarket)
        {
            GameAction pAction = new GameAction();
            pAction.setItem(ItemType.CRAFT_BLACK_MARKET);
            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.addLastValue((int)eBlackMarket);
            pAction.addLastValue(0);
            Globals.Network.sendAction(pAction);
        }

        public static void sendPing(TileClient pTile)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.PING_TILE);

            pAction.addLastValue(pTile.getID());

            Globals.Network.sendAction(pAction);
        }

        public static void sendClaimTile(TileClient pTile)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.CLAIM_TILE);

            pAction.addLastValue(pTile.getID());

            Globals.Network.sendAction(pAction);
        }

        public static void sendReturnClaim(TileClient pTile)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.RETURN_CLAIM);

            pAction.addLastValue(pTile.getID());

            Globals.Network.sendAction(pAction);
        }

        public static void sendCancelConstruct(TileClient pTile)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.CANCEL_CONSTRUCT);

            pAction.addLastValue(pTile.getID());

            Globals.Network.sendAction(pAction);
        }

        public static void sendConstruct(TileClient pTile, BuildingType eBuilding)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.CONSTRUCT_BUILDING);

            pAction.addLastValue(pTile.getID());
            pAction.addLastValue((int)eBuilding);

            Globals.Network.sendAction(pAction);
        }

        public static void sendSellAllResources()
        {
            GameAction pAction = new GameAction();
            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.SELL_ALL_RESOURCES);

            Globals.Network.sendAction(pAction);
        }

        public static void sendHoldResource(ResourceType eResource)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.TOGGLE_HOLD_RESOURCE);

            pAction.addLastValue((int)eResource);

            Globals.Network.sendAction(pAction);
        }

        public static void sendShareResource(ResourceType eResource)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.TOGGLE_SHARE_RESOURCE);

            pAction.addLastValue((int)eResource);

            Globals.Network.sendAction(pAction);
        }

        public static void sendShareAllResources(bool on)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.TOGGLE_SHARE_ALL_RESOURCES);

            pAction.addLastValue(Convert.ToInt16(on));

            Globals.Network.sendAction(pAction);
        }

        public static void sendRepair(ConstructionClient pConstruction)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.REPAIR);

            pAction.addLastValue(pConstruction.getID());

            Globals.Network.sendAction(pAction);
        }

        public static void sendScrap(TileClient pTile)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.SCRAP);

            pAction.addLastValue(pTile.getID());

            Globals.Network.sendAction(pAction);
        }

        public static void sendScrapAll(TileClient pTile)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.SCRAP_ALL);

            pAction.addLastValue(pTile.getID());

            Globals.Network.sendAction(pAction);
        }

        public static void sendSendResources(BuildingClient pBuilding)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.SEND_RESOURCES);

            pAction.addLastValue(pBuilding.getID());

            Globals.Network.sendAction(pAction);
        }

        public static void sendSendResourcesAll(BuildingClient pBuilding)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.SEND_RESOURCES_ALL);

            pAction.addLastValue(pBuilding.getID());

            Globals.Network.sendAction(pAction);
        }

        public static void sendSupplyBuilding(BuildingClient pBuilding)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.SUPPLY_BUILDING);

            pAction.addLastValue(pBuilding.getID());

            Globals.Network.sendAction(pAction);
        }

        public static void sendToggleAutoOff(BuildingClient pBuilding)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.TOGGLE_AUTO_OFF);

            pAction.addLastValue(pBuilding.getID());

            Globals.Network.sendAction(pAction);
        }

        public static void sendToggleAutoOffAll(BuildingClient pBuilding)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.TOGGLE_AUTO_OFF_ALL);

            pAction.addLastValue(pBuilding.getID());

            Globals.Network.sendAction(pAction);
        }

        public static void sendToggleOff(BuildingClient pBuilding)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.TOGGLE_OFF);

            pAction.addLastValue(pBuilding.getID());

            Globals.Network.sendAction(pAction);
        }

        public static void sendToggleOffAll(BuildingClient pBuilding)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.TOGGLE_OFF_ALL);

            pAction.addLastValue(pBuilding.getID());

            Globals.Network.sendAction(pAction);
        }

        public static void sendToggleOffEverything()
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.TOGGLE_OFF_EVERYTHING);

            Globals.Network.sendAction(pAction);
        }

        public static void sendAutoSupplyBuildingInput(BuildingType eBuilding)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.AUTO_SUPPLY_BUILDING_INPUT);

            pAction.addLastValue((int)eBuilding);

            Globals.Network.sendAction(pAction);
        }

        public static void sendHologramBuilding(int iTileID, BuildingType eHologramBuilding)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.HOLOGRAM_BUILDING);

            pAction.addLastValue(iTileID);
            pAction.addLastValue((int)eHologramBuilding);

            Globals.Network.sendAction(pAction);
        }

        public static void sendSendPatent(PatentType ePatent, PlayerType ePlayer)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.SEND_PATENT);

            pAction.addLastValue((int)ePatent);
            pAction.addLastValue((int)ePlayer);

            Globals.Network.sendAction(pAction);
        }

        public static void sendPatent(PatentType ePatent)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.PATENT);

            pAction.addLastValue((int)ePatent);

            Globals.Network.sendAction(pAction);
        }

        public static void sendResearch(TechnologyType eTechnology)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.RESEARCH);

            pAction.addLastValue((int)eTechnology);

            Globals.Network.sendAction(pAction);
        }

        public static void sendEspionage(EspionageType eEspionage)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.ESPIONAGE);

            pAction.addLastValue((int)eEspionage);

            Globals.Network.sendAction(pAction);
        }

        public static void sendLaunch(ResourceType eResource, bool bRepeat)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.LAUNCH);

            pAction.addLastValue((int)eResource);
            pAction.addLastValue((bRepeat) ? 1 : 0);

            Globals.Network.sendAction(pAction);
        }

        public static void sendAuctionBid(int iCurrentBid)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.AUCTION_BID);

            pAction.addLastValue(iCurrentBid);

            Globals.Network.sendAction(pAction);
        }

        public static void sendAuctionSkip()
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.AUCTION_SKIP);

            Globals.Network.sendAction(pAction);
        }

        public static void sendCancelOrder(OrderType eType, int iOrder)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.CANCEL_ORDER);

            pAction.addLastValue((int)eType);
            pAction.addLastValue(iOrder);

            Globals.Network.sendAction(pAction);
        }

        public static void sendBlackMarketPurchase(BlackMarketType blackMarketType)
        {
            GameAction pAction = new GameAction();
            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.BLACK_MARKET);

            pAction.addLastValue((int)blackMarketType);

            Globals.Network.sendAction(pAction);
        }

        public static void sendToggleCheating()
        {
            GameAction pAction = new GameAction();
            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.TOGGLE_CHEATING);

            Globals.Network.sendAction(pAction);
        }

        public static void sendBuyColonyStock()
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.BUY_COLONY_STOCK);

            Globals.Network.sendAction(pAction);
        }

        public static void sendBuyColonyModule(ModuleType eModule)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.BUY_COLONY_MODULE);

            pAction.addLastValue((int)eModule);

            Globals.Network.sendAction(pAction);
        }

        public static void sendSupplyWholesale(int iSlot)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.SUPPLY_WHOLESALE);

            pAction.addLastValue(iSlot);

            Globals.Network.sendAction(pAction);
        }

        public static void sendImport(int iSlot)
        {
            GameAction pAction = new GameAction();
            pAction.setItem(ItemType.IMPORT);
            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.addLastValue(iSlot);
            Globals.Network.sendAction(pAction);
        }

        public static void sendShipment()
        {
            GameAction pAction = new GameAction();
            pAction.setItem(ItemType.SHIPMENT);
            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            Globals.Network.sendAction(pAction);
        }

        public static void sendPlayerOption(PlayerOptionType ePlayerOption, bool bValue)
        {
            PlayerType eOriginalPlayer = AppGlobals.GameGlobals.ActivePlayerOriginal;

            if (eOriginalPlayer == PlayerType.NONE)
            {
                return;
            }

            GameAction pAction = new GameAction();

            pAction.setPlayer(eOriginalPlayer);
            pAction.setItem(ItemType.PLAYER_OPTION);

            pAction.addLastValue((int)ePlayerOption);
            pAction.addLastValue((bValue) ? 1 : 0);

            Globals.Network.sendAction(pAction);
        }

        public static void sendConcede()
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.CONCEDE);

            Globals.Network.sendAction(pAction);
        }

        public static void sendBeatSoren()
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.BEAT_SOREN);

            Globals.Network.sendAction(pAction);
        }

        public static void sendIsSoren()
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.IS_SOREN);

            Globals.Network.sendAction(pAction);
        }

        public static void sendBeatZultar()
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.BEAT_ZULTAR);

            Globals.Network.sendAction(pAction);
        }

        public static void sendIsZultar()
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.IS_ZULTAR);

            Globals.Network.sendAction(pAction);
        }

        public static void sendPlayerRank(string zRank)
        {
            GameAction pAction = new GameAction();

            pAction.setPlayer(AppGlobals.GameGlobals.ActivePlayer);
            pAction.setItem(ItemType.PLAYER_RANK);

            for (int i = 0; i < zRank.Length; i++)
            {
                pAction.addLastValue(zRank[i]);
            }

            Globals.Network.sendAction(pAction);
        }
    }
}