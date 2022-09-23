using System;
using Offworld.GameCore;
using Offworld.SystemCore;

namespace Offworld.MyGameCore
{
    public class MyGameServer : GameServer
    {
        public override void init(Infos pInfos, GameSettings pSettings)
        {
            for (int i = 0; i < pSettings.miNumHumans; i++)
            {
                MyGameFactory.playerAI.Insert(i, typeof(Algorithms.DefaultAI));
                MyGameFactory.playerHandicap.Insert(i, pSettings.playerSlots[i].Handicap);
            }
            for (int i = 0; i < pSettings.miNumPlayers; i++)
            {
                pSettings.playerSlots[i].Handicap = MyGameFactory.playerHandicap[0];
                MyGameFactory.playerHandicap.RemoveAt(0);
            }
            base.init(pInfos, pSettings);
        }
        public override void doUpdate()
        {
            using (new UnityProfileScope("Game::doUpdate"))
            {
                incrementSystemUpdateCount();

                testWinner();
                testLosers();

                if (isGameOver())
                {
                    return;
                }

                if (isTurnBasedPaused())
                {
                    return;
                }

                if (getDelayTime() > 0)
                {
                    changeDelayTime(-1);
                    return;
                }

                if (isAuction())
                {
                    if ((getSystemUpdateCount() % Constants.UPDATE_PER_SECOND) == 0)
                    {
                        doAuction();

                        if (isAuction())
                        {
                            if (getAuctionTime() != (infos().Globals.AUCTION_TIME_BID - 1))
                            {
                                foreach (PlayerServer pLoopPlayer in getPlayerServerAliveAll())
                                {
                                    if (pLoopPlayer.AI_doAuction())
                                    {
                                        break;
                                    }
                                }
                            }

                            foreach (PlayerServer pLoopPlayer in getPlayerServerAliveAll())
                            {
                                if (random().Next(4) == 0)
                                {
                                    pLoopPlayer.AI_doStock();
                                }
                            }
                        }
                        else
                        {
                            foreach (PlayerServer pLoopPlayer in getPlayerServerAll())
                            {
                                pLoopPlayer.AI_setUpdated(true);
                            }
                        }
                    }

                    return;
                }

                if (!isAuction())
                {
                    if ((getSystemUpdateCount() % infos().gameSpeed(getGameSpeed()).miSkipUpdates) == 0)
                    {
                        foreach (PlayerServer pLoopPlayer in getPlayerServerAll())
                        {
                            if (!(pLoopPlayer.AI_isUpdated()))
                            {
                                pLoopPlayer.AI_doUpdate();
                            }

                            pLoopPlayer.AI_setUpdated(false);
                        }

                        incrementGameUpdateCount();

                        if (getTurnBasedTime() > 0)
                        {
                            changeTurnBasedTime(-1);
                        }

                        updateStocks();

                        doModuleReveal();

                        doFoundMoney();

                        for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
                        {
                            playerServer(eLoopPlayer).doScan();
                        }

                        if (getHQLevels() > 0)
                        {
                            incrementTurnCount();

                            if (!(infos().rulesSet(getRulesSet()).mbNoTime))
                            {
                                incrementMinutes(infos().Globals.MINUTES_PER_TURN);

                                if (getMinutes() == 0)
                                {
                                    if (getHours() == getEclipseHour())
                                    {
                                        gameEventsServer().AddEclipse();
                                    }
                                }
                            }

                            doEntertainmentSupply();

                            doEventStates();

                            doEventGameTimes();

                            doWholesaleDelay();

                            if ((getTurnCount() % 15) == 0)
                            {
                                marketServer().doTurn();
                            }

                            if (getTurnCount() > 100)
                            {
                                if ((getTurnCount() % 21) == 0)
                                {
                                    if (randomEvent().Next(6) == 0)
                                    {
                                        doEventGame();
                                    }
                                }
                            }

                            foreach (TileServer pLoopTile in tileServerAll())
                            {
                                pLoopTile.doTurn();
                            }

                            for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
                            {
                                playerServer(eLoopPlayer).doTurn();
                            }

                            for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
                            {
                                playerServer(eLoopPlayer).doTurnStockpileNegative();
                            }

                            for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
                            {
                                playerServer(eLoopPlayer).doTurnStockpilePositive();
                            }

                            for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
                            {
                                playerServer(eLoopPlayer).doTurnStockpileExtra();
                            }

                            for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
                            {
                                playerServer(eLoopPlayer).doTurnAutoSell();
                            }

                            for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
                            {
                                playerServer(eLoopPlayer).doTurnAutoSupply();
                            }

                            for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
                            {
                                playerServer(eLoopPlayer).doTurnAutoPayDebt();
                            }

                            statsServer().doTurn();
                        }
                    }
                    else
                    {
                        // Disable so that the game has same result for all game speeds
                        //if ((getSystemUpdateCount() % (infos().gameSpeed(getGameSpeed()).miSkipUpdates / (int)(getNumPlayers()))) == 0)
                        //{
                        //    foreach (PlayerServer pLoopPlayer in getPlayerServerAll())
                        //    {
                        //        if (!(pLoopPlayer.AI_isUpdated()))
                        //        {
                        //            pLoopPlayer.AI_doUpdate();
                        //            pLoopPlayer.AI_setUpdated(true);
                        //            break;
                        //        }
                        //    }
                        //}
                    }
                }
            }
        }
    }
}
