using System;
using Offworld.GameCore;
using Offworld.SystemCore;

namespace Offworld.Algorithms
{
    public class MyAI2 : Abstract
    {
        public MyAI2(GameClient pGame, bool liveGame, bool debugMode) : base(pGame, liveGame, debugMode)
        {
        }
        public override void AI_doActions(bool is_auction)
        {
            if (is_auction)
                AI_doAuctionBid();
            else
                AI_doOtherActions();
        }

        // duplicate of AI_doAuction
        public bool AI_doAuctionBid()
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

        // modified version of AI_doUpdate
        public void AI_doOtherActions()
        {
            using (new UnityProfileScope("Player::AI_doOtherActions"))
            {
                if (isHQFounded())
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
    }
}
