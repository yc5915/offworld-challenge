using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Offworld.GameCore.Text;
using Offworld.SystemCore;

namespace Offworld.GameCore
{
    public class ConditionManagerClient : TextHelpers
    {
        protected bool isDirty = true;
        protected ReleasePool<StringBuilder> stringBuilderPool = new ReleasePool<StringBuilder>(() => new StringBuilder(), t => t.Clear(), null);

        protected GameClient mGame;
        protected List<ConditionType> winConditions = new List<ConditionType>();
        protected List<ConditionType> loseConditions = new List<ConditionType>();

        public ConditionManagerClient(GameClient pGame)
        {
            mGame = pGame;
        }

        public void Serialize(object stream, bool bAll)
        {
            SimplifyIO.Data(stream, ref winConditions, "WinConditions");
            SimplifyIO.Data(stream, ref loseConditions, "LoseConditions");
        }

        public bool isAnyDirty()
        {
            return isDirty;
        }

        //HUD
        public virtual string GetConditionsProgressText(PlayerClient player)
        {
            if (!player.isWinEligible())
            {
                return "";
            }

            StringBuilder sbConditionsProgress = stringBuilderPool.Acquire();
            StringBuilder sbWinConditionsProgress = stringBuilderPool.Acquire();

            foreach (ConditionType eCondition in winConditions)
            {
                if (sbWinConditionsProgress.Length > 0)
                {
                    sbConditionsProgress.AppendLine();
                }

                InfoCondition condition = mGame.infos().condition(eCondition);
                sbWinConditionsProgress.Append(GetConditionProgressText(condition, player));
            }

            if (sbWinConditionsProgress.Length > 0)
            {
                //if(sbWinConditionsProgress)
                //sbConditionsProgress.AppendLine(TEXT("TEXT_CONDITIONS_WIN_PROGRESS_TITLE"));
                sbConditionsProgress.Append(sbWinConditionsProgress.ToString());
            }

            int losingDay = 0;
            foreach (ConditionType eCondition in loseConditions)
            {
                InfoCondition condition = mGame.infos().condition(eCondition);
                if (condition.miRequiredDays > 0 && (losingDay == 0 || condition.miRequiredDays < losingDay))
                {
                    losingDay = condition.miRequiredDays;
                }
            }

            if (losingDay > 0)
            {
                if (sbConditionsProgress.Length > 0)
                {
                    sbConditionsProgress.AppendLine();
                }

                sbConditionsProgress.AppendLine(TEXT("TEXT_CONDITION_DAYS_REMAINING", (losingDay - mGame.getDays()).ToText()));
            }

            string result = sbConditionsProgress.ToString();
            stringBuilderPool.Release(ref sbWinConditionsProgress);
            stringBuilderPool.Release(ref sbConditionsProgress);
            return result;
        }

        public virtual string GetConditionProgressText(InfoCondition condition, PlayerClient player)
        {
            StringBuilder sbConditionProgress = stringBuilderPool.Acquire();

            if (condition.miRequiredMoney > 0)
            {
                sbConditionProgress.AppendLine(TEXT("TEXT_CONDITION_PROGRESS_MONEY", ConditionHelpers.GetTeamMoney(player).ToText(NumberFormatOptions.SHOW_CURRENCY), condition.miRequiredMoney.ToText(NumberFormatOptions.SHOW_CURRENCY)));
            }

            if (condition.miRequiredDebt > 0)
            {
                sbConditionProgress.AppendLine(TEXT("TEXT_CONDITION_PROGRESS_DEBT", ConditionHelpers.GetTeamDebt(player).ToText(NumberFormatOptions.SHOW_CURRENCY), condition.miRequiredDebt.ToText(NumberFormatOptions.SHOW_CURRENCY)));
            }

            if (Globals.Infos.HQLevelsNum() > 0 && condition.meRequiredHQLevel > HQLevelType.NONE + 1)
            {
                sbConditionProgress.AppendLine(TEXT("TEXT_CONDITION_PROGRESS_HQ_LEVEL", Globals.Infos.HQLevel(ConditionHelpers.GetTeamHighestHQ(player)).meName.ToText(), Globals.Infos.HQLevel(condition.meRequiredHQLevel).meName.ToText()));
            }

            string buildingProgressString = GetBuildingProgressText(condition, player);
            if (buildingProgressString.Length > 0)
            {
                if (sbConditionProgress.Length > 0)
                    sbConditionProgress.AppendLine();
                sbConditionProgress.Append(buildingProgressString);
            }

            string resourceProgressString = GetResourceProgressText(condition, player);
            if (resourceProgressString.Length > 0)
            {
                if (sbConditionProgress.Length > 0)
                    sbConditionProgress.AppendLine();
                sbConditionProgress.Append(resourceProgressString);
            }

            string orderProgressString = GetOrderProgressText(condition, player);
            if (orderProgressString.Length > 0)
            {
                if (sbConditionProgress.Length > 0)
                    sbConditionProgress.AppendLine();
                sbConditionProgress.Append(orderProgressString);
            }

            string patentProgressString = GetPatentProgressText(condition, player);
            if (patentProgressString.Length > 0)
            {
                if (sbConditionProgress.Length > 0)
                    sbConditionProgress.AppendLine();
                sbConditionProgress.Append(patentProgressString);
            }

            string technologyProgressString = GetTechnologyProgressText(condition, player);
            if (technologyProgressString.Length > 0)
            {
                if (sbConditionProgress.Length > 0)
                    sbConditionProgress.AppendLine();
                sbConditionProgress.Append(technologyProgressString);
            }

            string result = sbConditionProgress.ToString();
            stringBuilderPool.Release(ref sbConditionProgress);
            return result;
        }

        protected virtual string GetBuildingProgressText(InfoCondition condition, PlayerClient player)
        {
            StringBuilder sbBuildingProgress = stringBuilderPool.Acquire();
            for (int i = 0; i < condition.maiRequiredBuildings.Count; i++)
            {
                if (condition.maiRequiredBuildings[i] > 0)
                {
                    BuildingType eBuilding = (BuildingType)i;
                    sbBuildingProgress.AppendLine(TEXT("TEXT_CONDITION_PROGRESS_BUILDING_COUNT", Globals.Infos.building(eBuilding).meName.ToText(), ConditionHelpers.GetTeamBuildingCount(player, eBuilding).ToText(), condition.maiRequiredBuildings[i].ToText()));
                }
            }

            string result = sbBuildingProgress.ToString();
            stringBuilderPool.Release(ref sbBuildingProgress);
            return result;
        }

        protected virtual string GetResourceProgressText(InfoCondition condition, PlayerClient player)
        {
            StringBuilder sbResourceProgress = stringBuilderPool.Acquire();
            for (int i = 0; i < condition.maiRequiredResources.Count; i++)
            {
                if (condition.maiRequiredResources[i] > 0)
                {
                    ResourceType eResource = (ResourceType)i;
                    sbResourceProgress.AppendLine(TEXT("TEXT_CONDITION_PROGRESS_RESOURCE_COUNT", Globals.Infos.resource(eResource).meName.ToText(), ConditionHelpers.GetTeamResourceCount(player, eResource).ToText(), condition.maiRequiredResources[i].ToText()));
                }
            }

            string result = sbResourceProgress.ToString();
            stringBuilderPool.Release(ref sbResourceProgress);
            return result;
        }

        protected virtual string GetPatentProgressText(InfoCondition condition, PlayerClient player)
        {
            StringBuilder sbPatentProgress = stringBuilderPool.Acquire();
            for (int i = 0; i < condition.mabRequiredPatents.Count; i++)
            {
                if (condition.mabRequiredPatents[i])
                {
                    PatentType ePatent = (PatentType)i;
                    sbPatentProgress.AppendLine(TEXT("TEXT_CONDITION_PROGRESS_PATENT", Globals.Infos.patent(ePatent).meName.ToText(), player.isPatentAcquired(ePatent).ToText()));
                }
            }

            string result = sbPatentProgress.ToString();
            stringBuilderPool.Release(ref sbPatentProgress);
            return result;
        }

        protected virtual string GetOrderProgressText(InfoCondition condition, PlayerClient player)
        {
            StringBuilder sbOrderProgress = stringBuilderPool.Acquire();
            for (int i = 0; i < condition.maiRequiredOrders.Count; i++)
            {
                if (condition.maiRequiredOrders[i] > 0)
                {
                    OrderType eOrder = (OrderType)i;
                    sbOrderProgress.AppendLine(TEXT("TEXT_CONDITION_PROGRESS_ORDER_COUNT", Globals.Infos.order(eOrder).meName.ToText(), ConditionHelpers.GetTeamOrderCount(mGame, player, eOrder).ToText(), condition.maiRequiredOrders[i].ToText()));
                }
            }

            string result = sbOrderProgress.ToString();
            stringBuilderPool.Release(ref sbOrderProgress);
            return result;
        }

        protected virtual string GetTechnologyProgressText(InfoCondition condition, PlayerClient player)
        {
            StringBuilder sbTechnologyProgress = stringBuilderPool.Acquire();
            for (int i = 0; i < condition.maeRequiredTechnologyLevels.Count; i++)
            {
                if (condition.maeRequiredTechnologyLevels[i] > TechnologyLevelType.NONE)
                {
                    TechnologyType eTechnology = (TechnologyType)i;
                    sbTechnologyProgress.AppendLine(TEXT("TEXT_CONDITION_PROGRESS_TECHNOLOGY_LEVEL", Globals.Infos.technology(eTechnology).meName.ToText(), Globals.Infos.technologyLevel(ConditionHelpers.GetTeamTechnologyLevelDiscovered(player, eTechnology)).meName.ToText(), Globals.Infos.technologyLevel(condition.maeRequiredTechnologyLevels[i]).meName.ToText()));
                }
            }

            string result = sbTechnologyProgress.ToString();
            stringBuilderPool.Release(ref sbTechnologyProgress);
            return result;
        }
    }

    public class ConditionManagerServer : ConditionManagerClient
    {
        protected void makeDirty()
        {
            isDirty = true;
        }
        public void clearDirty()
        {
            isDirty = false;
        }

        protected PlayerType winnerOverride = PlayerType.NONE;
        protected List<PlayerType> losersOverride = new List<PlayerType>();

        public ConditionManagerServer(GameClient pGame)
            : base(pGame)
        {
        }

        public virtual void init(GameServer pGame, List<ConditionType> eWinConditions, List<ConditionType> eLoseConditions)
        {
            using (new UnityProfileScope("ConditionManagerServer.init"))
            {
                mGame = pGame;

                if (gameServer().isGameOption(GameOptionType.SEVEN_SOLS))
                {
                    AddWinCondition(gameServer().infos().Globals.WIN_CONDITION_DAYS);
                }
                else if (gameServer().infos().rulesSet(gameServer().getRulesSet()).mbWinMaxUpgrade)
                {
                    AddWinCondition(gameServer().infos().Globals.WIN_CONDITION_MAX_UPGRADE);
                }
                else
                {
                    eWinConditions.ForEach(condition => AddWinCondition(condition));
                }

                if (winConditions.Count == 0)
                {
                    AddWinCondition(gameServer().infos().Globals.WIN_CONDITION_DEFAULT);
                }

                eLoseConditions.ForEach(condition => AddLoseCondition(condition));
                makeDirty();
            }
        }

        protected virtual GameServer gameServer()
        {
            return (GameServer)mGame;
        }

        public virtual void AddWinCondition(ConditionType conditionType)
        {
            if (conditionType != ConditionType.NONE)
            {
                winConditions.Add(conditionType);
                makeDirty();
            }
        }

        public virtual void AddLoseCondition(ConditionType conditionType)
        {
            if (conditionType != ConditionType.NONE)
            {
                loseConditions.Add(conditionType);
                makeDirty();
            }
        }

        public virtual void SetWinnerOverride(PlayerType winner)
        {
            winnerOverride = winner;
        }

        public virtual void AddLosersOverride(List<PlayerType> losers)
        {
            losers.Where(loser => !losersOverride.Contains(loser))
                  .ForEach(loser => losersOverride.Add(loser));
        }

        public virtual PlayerType GetWinner()
        {
            if (winnerOverride != PlayerType.NONE)
                return winnerOverride;

            PlayerType winningPlayer = PlayerType.NONE;

            if (winConditions.Count == 0)
                return PlayerType.NONE;

            foreach (ConditionType eCondition in winConditions)
            {
                InfoCondition condition = gameServer().infos().condition(eCondition);
                if (condition.mbNoWin)
                {
                    continue;
                }
                else if (condition.mbUseDaysWin)
                {
                    winningPlayer = CheckDaysWinner();
                }
                else if (condition.mbUseDefaultWin)
                {
                    winningPlayer = CheckDefaultWinner();
                }
                else
                {
                    List<TeamType> evaluatedTeams = new List<TeamType>();
                    for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameServer().getNumPlayers(); eLoopPlayer++)
                    {
                        PlayerClient player = gameServer().playerServer(eLoopPlayer);

                        if (!player.isWinEligible())
                            continue;
                        if (evaluatedTeams.Contains(player.getTeam()))
                            continue;
                        if (!EvaluateCondition(condition, player))
                        {
                            evaluatedTeams.Add(player.getTeam());
                            continue;
                        }

                        winningPlayer = eLoopPlayer;
                        break;
                    }
                }

                if (winningPlayer != PlayerType.NONE)
                    break;
            }

            return winningPlayer;
        }

        public virtual List<PlayerType> GetLosers()
        {
            if (losersOverride.Count > 0)
            {
                return losersOverride;
            }

            List<PlayerType> losers = new List<PlayerType>();
            for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameServer().getNumPlayers(); eLoopPlayer++)
            {
                PlayerClient player = gameServer().playerServer(eLoopPlayer);

                foreach (ConditionType eCondition in loseConditions)
                {
                    InfoCondition condition = gameServer().infos().condition(eCondition);

                    if (EvaluateCondition(condition, player))
                    {
                        losers.Add(eLoopPlayer);
                        break;
                    }
                }
            }

            return losers;
        }

        protected virtual bool EvaluateCondition(InfoCondition condition, PlayerClient player)
        {
            if (!ConditionHelpers.EvaluateDays(condition.miRequiredDays, gameServer().getDays()))
                return false;
            if (!ConditionHelpers.EvaluateMoney(player, condition.miRequiredMoney))
                return false;
            if (!ConditionHelpers.EvaluateDebt(player, condition.miRequiredDebt))
                return false;
            if (!ConditionHelpers.EvaluateHQLevel(player, condition.meRequiredHQLevel))
                return false;
            if (!EvaluateBuildingAmounts(player, condition.maiRequiredBuildings))
                return false;
            if (!EvaluateResourceAmounts(player, condition.maiRequiredResources))
                return false;
            if (!EvaluateOrderCounts(player, gameServer(), condition.maiRequiredOrders))
                return false;
            if (!EvaluatePatents(player, condition.mabRequiredPatents))
                return false;
            if (!EvaluateTechnologyLevels(player, condition.maeRequiredTechnologyLevels))
                return false;

            return true;
        }

        protected virtual PlayerType CheckDaysWinner()
        {
            if(gameServer().gameSettings().mbSkipWin)
                return PlayerType.NONE;

            PlayerType eWinningPlayer = PlayerType.NONE;

            if (gameServer().getDays() >= gameServer().getLastDay())
            {
                if (gameServer().isCampaign())
                {
                    int iBestValue = 0;

                    for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameServer().getNumPlayers(); eLoopPlayer++)
                    {
                        if (gameServer().playerServer(eLoopPlayer).isWinEligible())
                        {
                            iBestValue = Math.Max(iBestValue, gameServer().playerServer(eLoopPlayer).getColonySharesOwned());
                        }
                    }

                    PlayerType eBestPlayer = PlayerType.NONE;
                    int iCount = 0;

                    for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameServer().getNumPlayers(); eLoopPlayer++)
                    {
                        if (gameServer().playerServer(eLoopPlayer).isWinEligible())
                        {
                            if (gameServer().playerServer(eLoopPlayer).getColonySharesOwned() == iBestValue)
                            {
                                eBestPlayer = eLoopPlayer;
                                iCount++;
                            }
                        }
                    }

                    if (iCount == 1)
                    {
                        eWinningPlayer = eBestPlayer;
                    }
                }
                else
                {
                    int iBestValue = 0;

                    for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameServer().getNumPlayers(); eLoopPlayer++)
                    {
                        if (gameServer().playerServer(eLoopPlayer).isWinEligible())
                        {
                            int iValue = gameServer().playerServer(eLoopPlayer).getSharePrice();
                            if (iValue >= iBestValue)
                            {
                                if ((iValue > iBestValue) || (gameServer().playerServer(eLoopPlayer).getColonySharesOwned() > gameServer().playerServer(eWinningPlayer).getColonySharesOwned()))
                                {
                                    eWinningPlayer = eLoopPlayer;
                                    iBestValue = iValue;
                                }
                            }
                        }
                    }
                }
            }

            if (eWinningPlayer == PlayerType.NONE)
            {
                eWinningPlayer = CheckDefaultWinner();
            }

            return eWinningPlayer;
        }

        protected virtual PlayerType CheckDefaultWinner()
        {
            if (gameServer().gameSettings().mbSkipWin)
            {
                return PlayerType.NONE;
            }

            if (gameServer().countTeamsWinEligible() > 1)
            {
                return PlayerType.NONE;
            }

            PlayerType eWinningPlayer = PlayerType.NONE;
            int iBestValue = 0;

            foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAll())
            {
                if (pLoopPlayer.isWinEligible())
                {
                    int iValue = pLoopPlayer.getSharePrice();
                    if (iValue > iBestValue)
                    {
                        eWinningPlayer = pLoopPlayer.getPlayer();
                        iBestValue = iValue;
                    }
                }
            }

            return eWinningPlayer;
        }

        public virtual bool EvaluateBuildingAmounts(PlayerClient player, List<int> requiredBuildingAmounts)
        {
            for (int i = 0; i < requiredBuildingAmounts.Count; i++)
            {
                if (!ConditionHelpers.EvaluateBuildingCount(player, (BuildingType)i, requiredBuildingAmounts[i]))
                    return false;
            }

            return true;
        }

        public virtual bool EvaluateResourceAmounts(PlayerClient player, List<int> requiredResourceAmounts)
        {
            for (int i = 0; i < requiredResourceAmounts.Count; i++)
            {
                if (!ConditionHelpers.EvaluateResourceCount(player, (ResourceType)i, requiredResourceAmounts[i]))
                    return false;
            }

            return true;
        }

        public virtual bool EvaluateOrderCounts(PlayerClient player, GameClient game, List<int> requiredOrderCounts)
        {
            for (int i = 0; i < requiredOrderCounts.Count; i++)
            {
                int numRequired = requiredOrderCounts[i];
                if (numRequired == 0) continue;

                if (!ConditionHelpers.EvaluateOrderCount(player, game, (OrderType)i, numRequired))
                    return false;

            }

            return true;
        }

        public virtual bool EvaluatePatents(PlayerClient player, List<bool> requiredPatents)
        {
            for (int i = 0; i < requiredPatents.Count; i++)
            {
                if (!requiredPatents[i]) continue;

                if (!ConditionHelpers.EvaluatePatent(player, (PatentType)i))
                    return false;
            }

            return true;
        }

        public virtual bool EvaluateTechnologyLevels(PlayerClient player, List<TechnologyLevelType> requiredTechnologyLevels)
        {
            for (int i = 0; i < requiredTechnologyLevels.Count; i++)
            {
                if (!ConditionHelpers.EvaluateTechnologyLevel(player, (TechnologyType)i, requiredTechnologyLevels[i]))
                    return false;
            }

            return true;
        }
    }
}