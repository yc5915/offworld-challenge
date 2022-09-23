using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Offworld.GameCore.Text;
using Offworld.SystemCore;

namespace Offworld.GameCore
{
    public static class Utils
    {
        // slewis - I hate including this in nearly every file
        private static string TEXT(string key) { return TextManager.TEXT(key); }
        private static string TEXT(TextType type) { return TextManager.TEXT(type); }
        private static string TEXT(string key, params TextVariable[] arguments) { return TextManager.TEXT(key, arguments); }
        private static string TEXT(TextType type, params TextVariable[] arguments) { return TextManager.TEXT(type, arguments); }
        private static PluralType GetPluralType(int value) { return TextManager.CurrentLanguageInfo.GetPluralType(value); }
        // end slewis hate

        public static int clamp(int iValue, int iMin, int iMax)
        {
            if (iValue < iMin)
            {
                return iMin;
            }
            else if (iValue > iMax)
            {
                return iMax;
            }

            return iValue;
        }

        public static int stepDistance(int iX1, int iY1, int iX2, int iY2)
        {
            iX1 += (iY1 >> 1);
            iX2 += (iY2 >> 1);

            int iDX = iX2 - iX1;
            int iDY = iY2 - iY1;

            //calculate sign
            bool bSX = (iDX >= 0);
            bool bSY = (iDY >= 0);

            //take absolute value
            iDX *= bSX ? 1 : -1;
            iDY *= bSY ? 1 : -1;

            if (bSX == bSY)
            {
                return Math.Max(iDX, iDY);
            }
            else
            {
                return iDX + iDY;
            }
        }

        public static int stepDistanceTile(TileClient tile1, TileClient tile2)
        {
            return stepDistance(tile1.getX(), tile1.getY(), tile2.getX(), tile2.getY());
        }

        public static bool adjacentTiles(TileClient tile1, TileClient tile2)
        {
            return (stepDistanceTile(tile1, tile2) == 1);
        }

        public static int maxStepDistance(GameClient pGame)
        {
            int iRight = pGame.getMapWidth() - 1;
            int iBottom = pGame.getMapHeight() - 1;

            return stepDistance(0, 0, iRight, iBottom);
        }

        //                                                  NW, NE,  E, SE, SW,  W
        public static readonly int[] DIRECTION_OFFSET_X_EVEN    = {  0,  1,  1,  1,  0, -1 };
        public static readonly int[] DIRECTION_OFFSET_X_ODD     = { -1,  0,  1,  0, -1, -1 };
        public static readonly int[] DIRECTION_OFFSET_Y         = {  1,  1,  0, -1, -1,  0 };
        public static int directionOffsetX(DirectionType eDirection, int iY)
        {
            if (eDirection == DirectionType.NONE)
            {
                return 0;
            }

            if ((iY & 1) == 0) //even
            {
                return DIRECTION_OFFSET_X_EVEN[(int)eDirection];
            }
            else //odd
            {
                return DIRECTION_OFFSET_X_ODD[(int)eDirection];
            }
        }

        public static int directionOffsetY(DirectionType eDirection)
        {
            if (eDirection == DirectionType.NONE)
            {
                return 0;
            }

            return DIRECTION_OFFSET_Y[(int)eDirection];
        }

        public static DirectionType directionOpposite(DirectionType eDirection)
        {
            if (eDirection == DirectionType.NONE)
            {
                return eDirection;
            }

            return (DirectionType)(((int)eDirection + 3) % 6);
        }

        public static DirectionType wrapDirection(DirectionType eDirection, int turns)
        {
            int result = MathUtilities.WrapInt((int)eDirection + turns, (int)DirectionType.NUM_TYPES);
            return (DirectionType)result;
        }

        public static int turnsFromDirToDir(DirectionType fromDir, DirectionType toDir)
        {
            int result = (int)toDir - (int)fromDir;
            return result;
        }

        public static string addCommasDeprecated(int iValue)
        {
            return TextManager.CurrentLanguageInfo.FormatNumber(iValue, NumberFormatOptions.NONE);
        }

        public static string signedNumber(int iValue)
        {
            return TextManager.CurrentLanguageInfo.FormatNumber(iValue, NumberFormatOptions.SHOW_SIGN);
        }

        public static string signedPercent(int iValue)
        {
            return TextManager.CurrentLanguageInfo.FormatNumber(iValue, NumberFormatOptions.SHOW_SIGN_AND_PERCENT);
        }

        public static string makeMoney(long iValue, bool bRate = false, bool bDenomination = true, bool bThousands = false, bool bDecimals = true)
        {
            string result;

            NumberFormatOptions options = NumberFormatOptions.NONE;
            options |= bDenomination ? NumberFormatOptions.SHOW_CURRENCY : 0;
            options |= bRate ? NumberFormatOptions.SHOW_SIGN : 0;

            if ((Mathf.Abs(iValue) >= 100000) || bThousands)
            {
                if ((Mathf.Abs(iValue) < 10000) && ((iValue % 1000) != 0) && bDecimals) //thousands with decimals
                {
                    result = TextManager.CurrentLanguageInfo.FormatNumber((iValue / (float)1000), 0, 1, options | NumberFormatOptions.SHOW_THOUSAND);
                }
                else if(Mathf.Abs(iValue) < 100000000) //thousands
                {
                    result = TextManager.CurrentLanguageInfo.FormatNumber(iValue / 1000, options | NumberFormatOptions.SHOW_THOUSAND);
                }
                else //millions
                {
                    result = TextManager.CurrentLanguageInfo.FormatNumber(iValue / 1000000, options | NumberFormatOptions.SHOW_MILLION);
                }
            }
            else
            {
                result = TextManager.CurrentLanguageInfo.FormatNumber(iValue, options);
            }

            return result;
        }

        public static string getChangeString(int iChange) { return getChangeString(iChange, 2); }
        public static string getChangeString(int iChange, int iDigits)
        {
            string result = TextManager.CurrentLanguageInfo.FormatNumber(iChange, 0, iDigits, NumberFormatOptions.SHOW_SIGN);
            return result;
        }

        public static string getRateString(int iRate) { return getRateString(iRate, 2); }
        public static string getRateString(int iRate, int iDigits, bool showSign = true)
        {
            float fValue = iRate / (float)Constants.RESOURCE_MULTIPLIER;
            string result = TextManager.CurrentLanguageInfo.FormatNumber(fValue, 0, iDigits, showSign ? NumberFormatOptions.SHOW_SIGN : NumberFormatOptions.NONE);
            return result;
        }

        public static string getShareString(int iPrice) { return getShareString(iPrice, true); }
        public static string getShareString(int iPrice, bool bDenomination)
        {
            float fValue = iPrice / (float)Constants.STOCK_MULTIPLIER;
            NumberFormatOptions options = bDenomination ? NumberFormatOptions.SHOW_CURRENCY : 0;
            string result = TextManager.CurrentLanguageInfo.FormatNumber(fValue, 2, 2, options);
            return result;
        }

        public static string getPercentageString(int iRate, bool showSign = true)
        {
            string result = TextManager.CurrentLanguageInfo.FormatNumber(iRate, NumberFormatOptions.SHOW_SIGN) + "%";
            return result;
        }

        private static string buildConnectedList(List<string> stringList, string noCommaConnector, string finalCommaConnector)
        {
            if (stringList.Count == 0)
                return "";
            else if (stringList.Count == 2)
                return TEXT(noCommaConnector, stringList[0].ToText(), stringList[1].ToText());
            else
            {
                TextVariable result = stringList[0].ToText();
                for (int i = 1; i < stringList.Count; i++)
                    result = TEXT((i == stringList.Count - 1) ? finalCommaConnector : "TEXT_COMMA_CONNECTOR", result, stringList[i].ToText()).ToText();

                return TEXT("TEXT_GENERIC_VARIABLE", result);
            }
        }

        public static string buildCommaAndList(List<string> astr)
        {
            return buildConnectedList(astr, "TEXT_AND_CONNECTOR", "TEXT_AND_COMMA_CONNECTOR");
        }

        public static string buildCommaOrList(List<string> astr)
        {
            return buildConnectedList(astr, "TEXT_OR_CONNECTOR", "TEXT_OR_COMMA_CONNECTOR");
        }

        public static string buildCommaNoAndOrList(List<string> astr)
        {
            return buildConnectedList(astr, "TEXT_COMMA_CONNECTOR", "TEXT_COMMA_CONNECTOR");
        }

        public static int getSecondsFromTurns(int iTurns, GameClient pGame)
        {
            int iSeconds = iTurns;

            GameSpeedType eGameSpeed = pGame.getGameSpeed();
            if (eGameSpeed != GameSpeedType.NONE)
            {
                iSeconds *= pGame.infos().gameSpeed(eGameSpeed).miSkipUpdates;
                iSeconds /= Constants.UPDATE_PER_SECOND;
            }

            return iSeconds;
        }

        public static int getNormalSecondsFromTurns(int turns)
        {
            return turns * Globals.Infos.gameSpeed(Globals.Infos.Globals.DEFAULT_GAMESPEED).miSkipUpdates / Constants.UPDATE_PER_SECOND;
        }

        public static string secondsToTime(int iSeconds)
        {
            string hours = string.Format("{0:00}", iSeconds / 3600);
            string minutes = string.Format("{0:00}", (iSeconds / 60) % 60);
            string seconds = string.Format("{0:00}", iSeconds % 60);

            return TextManager.TEXT("TEXT_TIME_HH_MM_SS", hours.ToText(), minutes.ToText(), seconds.ToText());
        }

        public static int getMinutesTotal(Infos infos, LocationType eLocation, int iHours, int iMinutes)
        {
            return (iHours * infos.location(eLocation).miMinutesPerHour) + iMinutes;
        }

        public static int getMinutesPerDay(Infos infos, LocationType eLocation)
        {
            return ((infos.location(eLocation).miHoursPerDay - 1) * infos.location(eLocation).miMinutesPerHour) + infos.location(eLocation).miLastHourMinutes;
        }

        public static int getTurnsPerDay(Infos infos, LocationType eLocation)
        {
            return (getMinutesPerDay(infos, eLocation) / infos.Globals.MINUTES_PER_TURN);
        }

        public static int getNumTurnsUntilAuction(GameClient game)
        {
            Infos info = game.infos();

            int auctionHour = info.location(game.getLocation()).miAuctionHour;
            int auctionMinutes = 0;

            int turnCount = 0;
            int currentDay = game.getDays();
            int currentHour = game.getHours();
            int currentMinutes = game.getMinutes();
            int turnsInDay = getTurnsPerDay(info, game.getLocation());

            do
            {
                if (auctionHour == currentHour)
                {
                    if (auctionMinutes >= currentMinutes)
                    {
                        if ((currentDay % info.location(game.getLocation()).miAuctionDayDiv) == 0)
                        {
                            turnCount += (auctionMinutes - currentMinutes) / info.Globals.MINUTES_PER_TURN;
                            return turnCount;
                        }
                    }
                }

                int minutesToAdvance = getHourMinutes(currentHour, game.getLocation()) - currentMinutes;
                currentMinutes = 0;
                turnCount += minutesToAdvance / info.Globals.MINUTES_PER_TURN;

                if (currentHour == (info.location(game.getLocation()).miHoursPerDay - 1))
                {
                    currentHour = 0;
                    currentDay++;
                }
                else
                {
                    currentHour++;
                }

            }
            while (turnCount < turnsInDay);

            return Mathf.Min(turnCount, turnsInDay);
        }

        public static int GetSecondsUntilAuction(GameClient game)
        {
            return getSecondsFromTurns(getNumTurnsUntilAuction(game), game);
        }

        public static int getTurnsUntilConstructionComplete(ConstructionClient construction)
        {
            int remainingConstruction = Globals.Infos.building(construction.getType()).miConstructionThreshold * 10 - construction.getConstruction();
            if (construction.getRate() != 0)
                return remainingConstruction / construction.getRate();
            else
                return 0;
        }

        public static int getPercentUntilNextTurn(GameClient game)
        {
            return ((game.getSystemUpdateCount() % Constants.UPDATE_PER_SECOND) * 100) / Constants.UPDATE_PER_SECOND;
        }
                
        //returns the distance ignoring the height difference
        public static float getDistance2D(Vector3 lhs, Vector3 rhs)
        {
            Vector3 offset = lhs - rhs;
            offset.y = 0;
            return offset.magnitude;
        }

        //returns the distance squared ignoring the height difference
        public static float getDistanceSquared2D(Vector3 lhs, Vector3 rhs)
        {
            Vector3 offset = lhs - rhs;
            offset.y = 0;
            return offset.sqrMagnitude;
        }

        public static bool withinRange2D(Vector3 lhs, Vector3 rhs, float fRange)
        {
            return (getDistanceSquared2D(lhs, rhs) <= fRange * fRange);
        }

        //value => [min, max] (inclusive)
        public static bool inRange(int value, int min, int max)
        {
            return ((value >= min) && (value <= max));
        }

        public static string addLineToList(string existingString, string newLine, string breaker)
        {
            if (newLine == null || newLine == "")
            {
                return existingString;
            }
            else if (existingString != null && existingString != "")
            {
                return existingString + breaker + newLine;
            }
            else
            {
                return newLine;
            }
        }

        public static bool isBuildingSpecial(BuildingType eBuilding) // special is a building that does not natively generate a resource
        {
            return !(isBuildingYieldAny(eBuilding, null));
        }

        public static bool isBuildingBeingResupplied(BuildingClient building, ResourceType eResource, GameClient pGame)
        {
            PlayerClient pPlayer = building.ownerClient();
            TileClient pTile = building.tileClient();

            if (pTile.getUnitMissionCount(pPlayer.getPlayer(), MissionType.SHIP_BUILDING) > 0)
            {
                for (int i = 0; i < pPlayer.getNumUnits(); i++)
                {
                    UnitClient pLoopUnit = pGame.unitClient(pPlayer.getUnitList()[i]);

                    if (pLoopUnit.getMissionInfo().meMission == MissionType.SHIP_BUILDING)
                    {
                        if (pLoopUnit.hasCargo(eResource))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static bool isBuildingPowerEffectedByWind(BuildingType eBuilding)
        {
            InfoBuilding buildingInfo = Globals.Infos.building(eBuilding);
            return Globals.Infos.winds().Any(wind => buildingInfo.maiOutputWindChange[wind.miType] != 0);
        }

        public static bool isBuildingEffectedByHeight(BuildingType eBuilding)
        {
            InfoBuilding buildingInfo = Globals.Infos.building(eBuilding);
            return Globals.Infos.heights().Any(height => buildingInfo.maiOutputHeightChange[height.miType] != 0);
        }

        public static bool buildingUsesWind(BuildingType eBuilding)
        {
            InfoBuilding building = Globals.Infos.building(eBuilding);

            for (WindType eWind = 0; eWind < Globals.Infos.windsNum(); eWind++)
            {
                if (building.maiOutputWindChange[(int)eWind] != 0)
                    return true;
            }

            return false;
        }

        public static bool buildingUsesHeight(BuildingType eBuilding)
        {
            InfoBuilding building = Globals.Infos.building(eBuilding);

            for (HeightType eHeight = 0; eHeight < Globals.Infos.heightsNum(); eHeight++)
            {
                if (building.maiOutputHeightChange[(int)eHeight] != 0)
                {
                    return true;
                }
            }

            return false;
        }


        // from http://stackoverflow.com/questions/20156/is-there-an-easy-way-to-create-ordinals-in-c#
        public static string AddOrdinal(int num)
        {
            if (num <= 0) return num.ToString();

            foreach (InfoOrdinal info in Globals.Infos.ordinals())
            {
                int divisor = Mathf.Max(info.miDivisor, 1);
                if (num % divisor == info.miRemainder)
                    return TextManager.TEXT(info.meName, num.ToText());
            }

            return num.ToString();
        }

        public static bool isBuildingMiningAny(BuildingType eBuilding)
        {
            return ListUtilities.Any(Globals.Infos.building(eBuilding).maiResourceMining, value => value > 0);
        }

        public static bool isBuildingInputAny(BuildingType eBuilding)
        {
            return ListUtilities.Any(Globals.Infos.building(eBuilding).maiResourceInput, value => value > 0);
        }

        public static bool isBuildingOutputAny(BuildingType eBuilding, GameClient pGame)
        {
            if (pGame != null)
            {
                return ListUtilities.Any(pGame.getBuildingResourceOutput(eBuilding), value => value > 0);
            }
            else
            {
                TerrainType eTerrainRate = Globals.Infos.building(eBuilding).meTerrainRate;

                if (eTerrainRate != TerrainType.NONE)
                {
                    if (ListUtilities.Any(Globals.Infos.terrain(eTerrainRate).maiResourceRate, value => value > 0))
                    {
                        return true;
                    }
                }

                return ListUtilities.Any(Globals.Infos.building(eBuilding).maiResourceOutput, value => value > 0);
            }
        }

        public static int getBuildingOutput(BuildingType eBuilding, ResourceType eResource, GameClient pGame)
        {
            if (pGame != null)
            {
                return pGame.getBuildingResourceOutput(eBuilding, eResource);
            }
            else
            {
                int iOutput = Globals.Infos.building(eBuilding).maiResourceOutput[(int)eResource];

                TerrainType eTerrainRate = Globals.Infos.building(eBuilding).meTerrainRate;

                if (eTerrainRate != TerrainType.NONE)
                {
                    iOutput += Globals.Infos.terrain(eTerrainRate).maiResourceRate[(int)eResource];
                }

                return iOutput;
            }
        }

        public static bool isBuildingOutput(BuildingType eBuilding, ResourceType eResource, GameClient pGame)
        {
            return (getBuildingOutput(eBuilding, eResource, pGame) > 0);
        }

        public static bool isBuildingYieldAny(BuildingType eBuilding, GameClient pGame)
        {
            return (isBuildingMiningAny(eBuilding) || isBuildingOutputAny(eBuilding, pGame));
        }

        public static bool isBuildingYield(BuildingType eBuilding, ResourceType eResource, GameClient pGame)
        {
            InfoBuilding buildingInfo = Globals.Infos.building(eBuilding);

            if (pGame != null)
            {
                return (pGame.getBuildingResourceOutput(eBuilding, eResource) > 0 || buildingInfo.maiResourceMining[(int)eResource] > 0);
            }
            else
            {
                return (isBuildingOutput(eBuilding, eResource, null) || buildingInfo.maiResourceMining[(int)eResource] > 0);
            }
        }

        public static bool isBuildingRevenue(BuildingType eBuilding, GameClient pGame)
        {
            return (isBuildingYieldAny(eBuilding, pGame) || (Globals.Infos.building(eBuilding).miEntertainment > 0));
        }

        public static bool isBuildingValid(BuildingType eBuilding, HQType eHQ)
        {
            return (Globals.Infos.building(eBuilding).mbAllValid || Globals.Infos.HQ(eHQ).mabBuildingValid[(int)eBuilding]);
        }

        public static ResourceType GetTechFirstResourceImprovement(Infos info, TechnologyType techType)
        {
            int foundIndex = info.technology(techType).mabResourceProduction.FindIndex(value => value);
            if (foundIndex >= 0)
                return (ResourceType)foundIndex;

            return ResourceType.NONE;
        }

        public static TechnologyType GetTechType(Infos info, ResourceType resource)
        {
            for (TechnologyType techType = 0; techType < info.technologiesNum(); techType++)
            {
                if (info.technology(techType).mabResourceProduction[(int)resource])
                    return techType;
            }

            return TechnologyType.NONE;
        }

        public static ResourceType GetResourceModified(Infos info, EspionageType espionage)
        {
            EventGameType eEvent = info.espionage(espionage).meEventGame;
            InfoEventGame eventInfo = info.eventGame(eEvent);

            for (ResourceType resource = 0; resource < info.resourcesNum(); resource++)
            {
                if (eventInfo.maiResourceChange[(int)resource] != 0)
                {
                    return resource;
                }
            }

            return ResourceType.NONE;
        }

        public static ResourceType GetResourceProduction(Infos info, TechnologyType tech)
        {
            InfoTechnology techInfo = info.technology(tech);
            for (ResourceType resource = 0; resource < info.resourcesNum(); resource++)
            {
                if (techInfo.mabResourceProduction[(int)resource])
                {
                    return resource;

                }
            }

            return ResourceType.NONE;
        }

        public static BuildingType GetTileVisiblBuilding(TeamType viewingTeam, GameClient game, TileClient tile)
        {
            int buildingID = tile.getBuildingID();
            if (buildingID != -1)
                return game.buildingClient(buildingID).getVisibleType(viewingTeam);

            return BuildingType.NONE;
        }

        public static int GetAmountCanBuySell(PlayerClient player, MarketClient market, ResourceType resource, int intendedQuantity)
        {
            int actualQuantity = intendedQuantity;

            if (actualQuantity > 0)
            {
                // this is. . . a dumb way to do this, but it works.
                while (actualQuantity > 0)
                {
                    if (market.calculateBuyCost(resource, actualQuantity) > player.getMoney())
                    {
                        actualQuantity--;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else if (actualQuantity < 0)
            {
                int availableStockpile = player.getAvailableResourceStockpile(resource, true);

                if (actualQuantity < -(availableStockpile))
                {
                    actualQuantity = -(availableStockpile);
                }
            }

            return actualQuantity;
        }

        public static int getHourMinutes(int iHour, LocationType eLocation)
        {
            return ((iHour == (Globals.Infos.location(eLocation).miHoursPerDay - 1)) ? Globals.Infos.location(eLocation).miLastHourMinutes : Globals.Infos.location(eLocation).miMinutesPerHour);
        }

        //public static string GetTimeString(int iDays, int iHours, int iMinutes)
        //{
        //    string time = Offworld.UICore.GuiUtils.FormatTimeHoursMinutes(iHours, iMinutes);

        //    return TextManager.TEXT("TEXT_HUDINFO_TIME_DAY_NUM", (iDays + 1).ToText(), time.ToText());
        //}

        //public static string GetTimeFromTurnsString(int iTurns, LocationType eLocation)
        //{
        //    int iDays = 0;
        //    int iHours = Globals.Infos.location(eLocation).miStartingHour;
        //    int iMinutes = 0;

        //    GetTimeFromTurns(iTurns, eLocation, ref iDays, ref iHours, ref iMinutes);

        //    return GetTimeString(iDays, iHours, iMinutes);
        //}

        public static void GetTimeFromTurns(int iTurns, LocationType eLocation, ref int iDays, ref int iHours, ref int iMinutes)
        {
            // XXX do this a better way...
            for (int i = 0; i < iTurns; i++)
            {
                iMinutes += Globals.Infos.Globals.MINUTES_PER_TURN;

                if (iMinutes >= getHourMinutes(iHours, eLocation))
                {
                    if (iHours == (Globals.Infos.location(eLocation).miHoursPerDay - 1))
                    {
                        iDays++;
                        iHours = 0;
                        iMinutes = 0;
                    }
                    else
                    {
                        iHours++;
                        iMinutes = 0;
                    }
                }
            }
        }

        public static CharacterType GetCharacterFromExecutive(ExecutiveType exec)
        {
            Infos infos = Globals.Infos;
            return infos.character(infos.personality(infos.executive(exec).mePersonality).meCharacter).meType;
        }
       
        public static bool isLegendary(string zText)
        {
            if (zText == null)
            {
                return false;
            }

            return (zText.ToLower() == "legendary");
        }

        public static int getPlayerTile(PlayerType ePlayer, GameClient pGame)
        {
            if (ePlayer == PlayerType.NONE)
            {
                return -1;
            }

            HQClient pHQ = pGame.playerClient(ePlayer).startingHQClient();

            if (pHQ == null)
            {
                return -1;
            }

            return pHQ.tileClient().getID();
        }

        public static LatitudeType getRandomLatitude(Infos pInfos, System.Random pRandom)
        {
            int iTotalRolls = 0;

            for (LatitudeType eLoopLatitude = 0; eLoopLatitude < pInfos.latitudesNum(); eLoopLatitude++)
            {
                iTotalRolls += pInfos.latitude(eLoopLatitude).miDieRoll;
            }

            int iRoll = pRandom.Next(iTotalRolls);

            for (LatitudeType eLoopLatitude = 0; eLoopLatitude < pInfos.latitudesNum(); eLoopLatitude++)
            {
                if (iRoll < pInfos.latitude(eLoopLatitude).miDieRoll)
                {
                    return eLoopLatitude;
                }
                else
                {
                    iRoll -= pInfos.latitude(eLoopLatitude).miDieRoll;
                }
            }

            return (LatitudeType)0;
        }

        public static bool[] SetLifeSupportResourcesData(LocationType eLocation, IceType eIce, out bool[] lifeSupportBuildings, out bool[] lifeSupportResources)
        {
            return SetLifeSupportResourcesData(HQType.NONE, eLocation, eIce, out lifeSupportBuildings, out lifeSupportResources);
        }

        public static bool[] SetLifeSupportResourcesData(HQType eHQ, LocationType eLocation, IceType eIce, out bool[] lifeSupportBuildings, out bool[] lifeSupportResources)
        {
            lifeSupportBuildings = Enumerable.Repeat(false, (int)Globals.Infos.buildingsNum()).ToArray();
            lifeSupportResources = Enumerable.Repeat(false, (int)Globals.Infos.resourcesNum()).ToArray();

            for (HQType eLoopHQ = 0; eLoopHQ < Globals.Infos.HQsNum(); eLoopHQ++)
            {
                if (eHQ != HQType.NONE && eLoopHQ != eHQ)
                    continue;

                for (ResourceType eLoopResource = 0; eLoopResource < Globals.Infos.resourcesNum(); eLoopResource++)
                {
                    if (Globals.Infos.HQ(eLoopHQ).maiLifeSupport[(int)eLoopResource] > 0 && !Globals.Infos.resource(eLoopResource).mabLocationInvalid[(int)eLocation])
                    {
                        lifeSupportResources[(int)eLoopResource] = true;

                        for (BuildingType eLoopBuilding = 0; eLoopBuilding < Globals.Infos.buildingsNum(); eLoopBuilding++)
                        {
                            InfoBuilding buildingInfo = Globals.Infos.building(eLoopBuilding);

                            if (buildingInfo.mbIce && (eIce == IceType.NONE || Globals.Infos.ice(eIce).maiAverageResourceRate[(int)eLoopResource] == 0))
                                continue;

                            if (!buildingInfo.mabLocationInvalid[(int)eLocation] && (buildingInfo.maiResourceMining[(int)eLoopResource] > 0 || buildingInfo.maiResourceOutput[(int)eLoopResource] > 0))
                            {
                                bool isLifeSupportBuilding = true;
                                for(int i=0; i < lifeSupportBuildings.Length; i++)
                                {
                                    if (lifeSupportBuildings[i] && Globals.Infos.building((BuildingType)i).meClass == buildingInfo.meClass)
                                        isLifeSupportBuilding = false;

                                    if (!isLifeSupportBuilding)
                                        break;
                                }

                                lifeSupportBuildings[(int)eLoopBuilding] |= isLifeSupportBuilding;
                                continue;
                            }
                        }
                    }
                }
            }

            return lifeSupportResources;
        }

        public static bool IsValidTerrainCollection(BuildingType eBuilding, LocationType eLocation, ResourceType eResource)
        {
            TerrainType eTerrain;
            if ((eTerrain = Globals.Infos.building(eBuilding).meTerrainRate) == TerrainType.NONE)
                return false;

            InfoTerrain infoTerrain = Globals.Infos.terrain(eTerrain);
            if (infoTerrain.mabLocationInvalid[(int)eLocation])
                return false;

            if (infoTerrain.maiResourceRate[(int)eResource] > 0)
                return true;

            return false;
        }

        public static void SetCollectedResourcesData(LocationType eLocation, IceType eIce, out bool[] collectingBuildings, out bool[] collectedResources)
        {
            SetCollectedResourcesData(HQType.NONE, eLocation, eIce, out collectingBuildings, out collectedResources);
        }

        public static void SetCollectedResourcesData(HQType eHQ, LocationType eLocation, IceType eIce, out bool[] collectingBuildings, out bool[] collectedResources)
        {
            collectingBuildings = Enumerable.Repeat(false, (int)Globals.Infos.buildingsNum()).ToArray();
            collectedResources = Enumerable.Repeat(false, (int)Globals.Infos.resourcesNum()).ToArray();

            foreach (InfoBuilding building in Globals.Infos.buildings())
            {
                if (building.mabLocationInvalid[(int)eLocation] || (eHQ != HQType.NONE && !isBuildingValid(building.meType, eHQ)))
                    continue;

                if (building.mbIce && (eIce == IceType.NONE))
                    continue;

                bool hasInputs = building.maiResourceInput.Any(x => x > 0);
                bool isCollectingBuilding = false;

                for (ResourceType eLoopResource = 0; eLoopResource < Globals.Infos.resourcesNum(); eLoopResource++)
                {
                    if (Globals.Infos.resource(eLoopResource).mabLocationInvalid[(int)eLocation])
                        continue;

                    if (building.maiResourceMining[(int)eLoopResource] > 0)
                        isCollectingBuilding = collectedResources[(int)eLoopResource] = true;

                    if (!hasInputs && building.maiResourceOutput[(int)eLoopResource] > 0)
                        isCollectingBuilding = collectedResources[(int)eLoopResource] = true;

                    if (IsValidTerrainCollection(building.meType, eLocation, eLoopResource))
                        isCollectingBuilding = collectedResources[(int)eLoopResource] = true;

                    if (building.mbIce && eIce != IceType.NONE && (Globals.Infos.ice(eIce).maiAverageResourceRate[(int)eLoopResource] == 0))
                        isCollectingBuilding = false;
                }

                if (isCollectingBuilding)
                {
                    for (int i = 0; i < collectingBuildings.Length; i++)
                    {
                        if (collectingBuildings[i] && Globals.Infos.building((BuildingType)i).meClass == building.meClass)
                            isCollectingBuilding = false;

                        if (!isCollectingBuilding)
                            break;
                    }
                }
                
                collectingBuildings[(int)building.meType] |= isCollectingBuilding;
            }
        }

        public static void SetManufacturedResourcesData(LocationType eLocation, IceType eIce, out bool[] manufacturingBuildings, out bool[] manufacturedResources)
        {
            SetManufacturedResourcesData(HQType.NONE, eLocation, eIce, out manufacturingBuildings, out manufacturedResources);
        }

        public static void SetManufacturedResourcesData(HQType eHQ, LocationType eLocation, IceType eIce, out bool[] manufacturingBuildings, out bool[] manufacturedResources)
        {
            manufacturingBuildings = Enumerable.Repeat(false, (int)Globals.Infos.buildingsNum()).ToArray();
            manufacturedResources = Enumerable.Repeat(false, (int)Globals.Infos.resourcesNum()).ToArray();

            foreach (InfoBuilding building in Globals.Infos.buildings())
            {
                if (building.mabLocationInvalid[(int)eLocation] || (eHQ != HQType.NONE && !isBuildingValid(building.meType, eHQ)))
                    continue;

                if (building.mbIce && (eIce == IceType.NONE))
                    continue;

                bool hasInputs = building.maiResourceInput.Any(x => x > 0);
                for (ResourceType eLoopResource = 0; eLoopResource < Globals.Infos.resourcesNum(); eLoopResource++)
                {
                    if (hasInputs && building.maiResourceOutput[(int)eLoopResource] > 0)
                    {
                        bool isManufacturingBuilding = true;
                        manufacturedResources[(int)eLoopResource] = true;

                        if (building.mbIce && eIce != IceType.NONE && (Globals.Infos.ice(eIce).maiAverageResourceRate[(int)eLoopResource] == 0))
                            isManufacturingBuilding = false;

                        if (isManufacturingBuilding)
                        {
                            for (int i = 0; i < manufacturingBuildings.Length; i++)
                            {
                                if (manufacturingBuildings[i] && Globals.Infos.building((BuildingType)i).meClass == building.meClass)
                                    isManufacturingBuilding = false;

                                if (!isManufacturingBuilding)
                                    break;
                            }
                        }

                        manufacturingBuildings[(int)building.meType] |= isManufacturingBuilding;
                    }
                }
            }
        }

        public static TextType getColonyClassDesc(ColonyClassType eColonyClass, LocationType eLocation)
        {
            TextType eText = Globals.Infos.colonyClass(eColonyClass).maeLocationDesc[(int)eLocation];

            if (eText == TextType.NONE)
            {
                eText = Globals.Infos.colonyClass(eColonyClass).meDesc;
            }

            return eText;
        }

        public static TextType getColonyClassDescSkirmish(ColonyClassType eColonyClass, LocationType eLocation)
        {
            TextType eText = Globals.Infos.colonyClass(eColonyClass).maeLocationDescSkirmish[(int)eLocation];

            if (eText == TextType.NONE)
            {
                eText = Globals.Infos.colonyClass(eColonyClass).meDescSkirmish;
            }

            return eText;
        }

        public static TextType getColonyClassStory(ColonyClassType eColonyClass, LocationType eLocation)
        {
            TextType eText = Globals.Infos.colonyClass(eColonyClass).maeLocationStory[(int)eLocation];

            if (eText == TextType.NONE)
            {
                eText = Globals.Infos.colonyClass(eColonyClass).meStory;
            }

            return eText;
        }
    }
}