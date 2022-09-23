using System;
using System.Collections.Generic;
using System.Linq;

namespace Offworld.GameCore
{
    public static class ConditionHelpers
    {
        public static long GetTeamMoney(PlayerClient player)
        {
            long money = 0;
            List<PlayerClient> teamMembers = player.getAliveTeammatesAll().ToList();
            foreach (PlayerClient member in teamMembers)
            {
                money += member.getMoney();
            }

            return money;
        }

        public static int GetTeamDebt(PlayerClient player)
        {
            int debt = 0;
            List<PlayerClient> teamMembers = player.getAliveTeammatesAll().ToList();
            foreach (PlayerClient member in teamMembers)
            {
                debt += member.getDebt();
            }

            return debt;
        }

        public static HQLevelType GetTeamHighestHQ(PlayerClient player)
        {
            HQLevelType highestHQ = HQLevelType.NONE;
            List<PlayerClient> teamMembers = player.getAliveTeammatesAll().ToList();
            foreach (PlayerClient member in teamMembers)
            {
                if (member.getHQLevel() > highestHQ)
                {
                    highestHQ = member.getHQLevel();
                }
            }

            return highestHQ;
        }

        public static int GetTeamBuildingCount(PlayerClient player, BuildingType building)
        {
            return GetTeamBuildingCount(player, building, true);
        }

        public static int GetTeamBuildingCount(PlayerClient player, BuildingType building, bool bReal)
        {
            List<PlayerClient> teamMembers = player.getAliveTeammatesAll().ToList();
            int numBuilding = 0;

            foreach (PlayerClient member in teamMembers)
            {
                if (bReal)
                    numBuilding += member.getRealBuildingCount(building);
                else
                    numBuilding += member.getBuildingCount(building);
            }

            return numBuilding;
        }

        public static int GetTeamConstructionCount(PlayerClient player, BuildingType building, GameClient game)
        {
            List<PlayerClient> teamMembers = player.getAliveTeammatesAll().ToList();
            int numConstructions = 0;

            foreach (PlayerClient member in teamMembers)
            {
                foreach (int constructionID in member.getConstructionList())
                {
                    if (game.constructionClient(constructionID).getType() == building)
                        numConstructions++;
                }
            }

            return numConstructions;
        }

        public static int GetTeamConstructionUnitCount(PlayerClient player, BuildingType building, GameClient game)
        {
            List<PlayerClient> teamMembers = player.getAliveTeammatesAll().ToList();
            int numUnits = 0;

            foreach (PlayerClient member in teamMembers)
            {
                foreach (int unitID in member.getUnitList())
                {
                    if (game.unitClient(unitID).getConstructionType() == building)
                        numUnits++;
                }
            }

            return numUnits;
        }

        public static int GetTeamBuildingConstructionCount(PlayerClient player, BuildingType building, GameClient game)
        {
            return GetTeamBuildingCount(player, building) + GetTeamConstructionCount(player, building, game);
        }

        public static int GetTeamBuildingCountAll(PlayerClient player, BuildingType building, GameClient game)
        {
            return GetTeamBuildingCount(player, building) + GetTeamConstructionCount(player, building, game) + GetTeamConstructionUnitCount(player, building, game);
        }

        public static int GetTeamResourceBuildingCount(PlayerClient player, BuildingType building, ResourceType resource, GameClient game)
        {
            return GetTeamResourceBuildingCount(player, building, resource, game, true);
        }

        public static int GetTeamResourceBuildingCount(PlayerClient player, BuildingType building, ResourceType resource, GameClient game, bool bReal)
        {
            List<PlayerClient> teamMembers = player.getAliveTeammatesAll().ToList();
            int numBuilding = 0;

            foreach (PlayerClient member in teamMembers)
            {
                List<int> buildingList = member.getBuildingList();

                for (int i = 0; i < buildingList.Count; i++)
                {
                    BuildingClient buildingClient = game.buildingClient(buildingList[i]);

                    if (bReal && !buildingClient.tileClient().isOwnerReal())
                        continue;

                    if (buildingClient.getType() == building && buildingClient.tileClient().getResourceLevel(resource, false) > ResourceLevelType.NONE)
                        numBuilding++;
                }
            }

            return numBuilding;
        }

        public static int GetTeamResourceCount(PlayerClient player, ResourceType resource)
        {
            List<PlayerClient> teamMembers = player.getAliveTeammatesAll().ToList();
            int numResource = 0;
            foreach (PlayerClient member in teamMembers)
            {
                numResource += member.getWholeResourceStockpile(resource, false);
            }

            return numResource;
        }

        public static int GetTeamOrderCount(GameClient game, PlayerClient player, OrderType order)
        {
            int completedOrders = 0;
            List<PlayerClient> teamMembers = player.getAliveTeammatesAll().ToList();

            foreach (PlayerClient member in teamMembers)
            {
                if (order == OrderType.HACK)
                {
                    for (int resourceIndex = 0; resourceIndex < (int)Globals.Infos.resourcesNum(); resourceIndex++)
                    {
                        completedOrders += game.statsClient().getStat(StatsType.RESOURCE, (int)ResourceStatType.SHORTAGES_STARTED, member.getPlayer(), resourceIndex);
                        completedOrders += game.statsClient().getStat(StatsType.RESOURCE, (int)ResourceStatType.SURPLUSES_STARTED, member.getPlayer(), resourceIndex);
                    }
                }
                else if (order == OrderType.LAUNCH)
                {
                    for (int resourceIndex = 0; resourceIndex < (int)Globals.Infos.resourcesNum(); resourceIndex++)
                    {
                        completedOrders += game.statsClient().getStat(StatsType.RESOURCE, (int)ResourceStatType.LAUNCHED, member.getPlayer(), resourceIndex);
                    }
                }
                else if (order == OrderType.PATENT)
                {
                    for (int patentIndex = 0; patentIndex < (int)Globals.Infos.patentsNum(); patentIndex++)
                    {
                        completedOrders += game.statsClient().getStat(StatsType.PATENT, (int)PatentStatType.ACQUIRED, member.getPlayer(), patentIndex);
                    }
                }
                else if (order == OrderType.RESEARCH)
                {
                    for (int technologyIndex = 0; technologyIndex < (int)Globals.Infos.technologiesNum(); technologyIndex++)
                    {
                        completedOrders += game.statsClient().getStat(StatsType.TECHNOLOGY, (int)TechnologyStatType.RESEARCHED, member.getPlayer(), technologyIndex);
                    }
                }
            }

            return completedOrders;
        }

        public static bool GetTeamPatentAcquired(PlayerClient player, PatentType patent)
        {
            List<PlayerClient> teamMembers = player.getAliveTeammatesAll().ToList();

            foreach (PlayerClient member in teamMembers)
            {
                if (member.isPatentAcquired(patent))
                    return true;
            }

            return false;
        }

        public static TechnologyLevelType GetTeamTechnologyLevelDiscovered(PlayerClient player, TechnologyType technology)
        {
            List<PlayerClient> teamMembers = player.getAliveTeammatesAll().ToList();
            TechnologyLevelType highestLevel = TechnologyLevelType.NONE;
            foreach (PlayerClient member in teamMembers)
            {
                if (member.getTechnologyLevelDiscovered(technology) > highestLevel)
                {
                    highestLevel = member.getTechnologyLevelDiscovered(technology);
                }
            }

            return highestLevel;
        }

        public static bool EvaluateDays(int requiredDays, int currentDays)
        {
            return requiredDays <= 0 || currentDays >= requiredDays;
        }

        public static bool EvaluateMoney(PlayerClient player, int requiredMoney)
        {
            return GetTeamMoney(player) >= requiredMoney;
        }

        public static bool EvaluateDebt(PlayerClient player, int requiredDebt)
        {
            if (requiredDebt < 0)
            {
                return GetTeamDebt(player) >= requiredDebt;
            }
            else
            {
                return true;
            }
        }

        public static bool EvaluateHQLevel(PlayerClient player, HQLevelType requiredHQLevel)
        {
            return GetTeamHighestHQ(player) >= requiredHQLevel;
        }

        public static bool EvaluateHasBuilding(PlayerClient player, BuildingType building)
        {
            return GetTeamBuildingCount(player, building) > 0;
        }

        public static bool EvaluateBuildingCount(PlayerClient player, BuildingType building, int requiredBuildingCount)
        {
            return GetTeamBuildingCount(player, building) >= requiredBuildingCount;
        }

        public static bool EvaluateHasResourceBuilding(PlayerClient player, GameClient game, BuildingType building, ResourceType resource)
        {
            return GetTeamResourceBuildingCount(player, building, resource, game) > 0;
        }

        public static bool EvaluateResourceBuildingCount(PlayerClient player, GameClient game, BuildingType building, ResourceType resource, int requiredCount)
        {
            return GetTeamResourceBuildingCount(player, building, resource, game) >= requiredCount;
        }

        public static bool EvaluateHasResource(PlayerClient player, ResourceType resource)
        {
            return GetTeamResourceCount(player, resource) > 0;
        }

        public static bool EvaluateResourceCount(PlayerClient player, ResourceType resource, int requiredResourceCount)
        {
            return GetTeamResourceCount(player, resource) >= requiredResourceCount;
        }

        public static bool EvaluateOrderCount(PlayerClient player, GameClient game, OrderType order, int requiredORderCount)
        {
            return GetTeamOrderCount(game, player, order) >= requiredORderCount;
        }

        public static bool EvaluatePatent(PlayerClient player, PatentType patent)
        {
            return GetTeamPatentAcquired(player, patent);
        }

        public static bool EvaluateTechnologyLevel(PlayerClient player, TechnologyType technology, TechnologyLevelType requiredTechnologyLevel)
        {
               return GetTeamTechnologyLevelDiscovered(player, technology) >= requiredTechnologyLevel;
        }
    }
}
