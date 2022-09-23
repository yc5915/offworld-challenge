using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Offworld.GameCore.Text;
using System.Xml;
//using Offworld.UICore;
using Offworld.SystemCore;

namespace Offworld.GameCore
{
    [Serializable]
    public class Campaign : TextHelpersSerializable
    {
        [Serializable]
        public class LevelResult
        {
            public LevelType meLevel = LevelType.NONE;
            public CorporationType meWinner = CorporationType.NONE;
            public int miPopulation = 0;
            public List<CorporationType> maePlayerIDs = new List<CorporationType>();
            public List<int> maiPlayerColonyShares = new List<int>();
            public List<int> maiPlayerHQs = new List<int>();
            public List<int> maiPlayerCash = new List<int>();
            public List<int> maiPlayerDebt = new List<int>();
            public List<int> maiPlayerStructures = new List<int>();
            public List<int> maiPlayerModules = new List<int>();
            public List<int> maiResourcePrices = new List<int>();
            public List<int> maiResourcePricesOffworld = new List<int>();

            public LevelResult(LevelType eLevel, CorporationType eWinner, int iPopulation, List<CorporationType> aePlayerIDs, List<int> aiPlayerColonyShares, List<int> aiPlayerHQs, List<int> aiPlayerCash, List<int> aiPlayerDebt, List<int> aiPlayerStructures, List<int> aiPlayerModules, List<int> aiResourcePrices, List<int> aiResourcePricesOffworld)
            {
                meLevel = eLevel;
                meWinner = eWinner;
                miPopulation = iPopulation;
                maePlayerIDs = aePlayerIDs;
                maiPlayerColonyShares = aiPlayerColonyShares;
                maiPlayerHQs = aiPlayerHQs;
                maiPlayerCash = aiPlayerCash;
                maiPlayerDebt = aiPlayerDebt;
                maiPlayerStructures = aiPlayerStructures;
                maiPlayerModules = aiPlayerModules;
                maiResourcePrices = aiResourcePrices;
                maiResourcePricesOffworld = aiResourcePricesOffworld;
            }
        }

        [Serializable]
        public class TurnResult
        {
            public List<LevelResult> mLevelResults = new List<LevelResult>();

            public TurnResult()
            {
            }

            public void addResult(LevelResult result)
            {
                mLevelResults.Add(result);
            }
        }

        private CampaignModeType meCampaignMode = CampaignModeType.NONE;
        private CampaignState meCampaignState = CampaignState.NONE;
        private HandicapType meHandicap = HandicapType.NONE;
        private LocationType meLocation = LocationType.NONE;
        private int miActiveCorporations = 0;
        private int miTurn = 0;
        private int miSeed = 0;
        private bool mbPlayingTheGame = false;
        private bool mbAutoSave = true;
        private bool mbShowPopups = true; // XXX unused but left for save compatibility
        private bool mbShowEvent = true;
        private bool mbIsDLCBobCampaign = false;
        private EventTurnType meEventTurn = EventTurnType.NONE;
        private EventGameType meEventGame = EventGameType.NONE;

        public System.Random mRandom = null;
        public System.Random mRandomSim = null;
        public System.Random mRandomAI = null;

        private List<Corporation> mapCorporations = new List<Corporation>();
        private List<List<string>> maazEventText = new List<List<string>>();

        private List<int> maiResourcePrice = new List<int>();
        private List<int> maiResourcePriceOffworld = new List<int>();
        private List<int> maiPerksAvailable = new List<int>();
        private List<int> maiPermPerksBought = new List<int>();
        private List<int> maiTempPerksBought = new List<int>();
        private List<int> maiLevelPopulation = new List<int>();
        private List<int> maiLevelTurn = new List<int>();
        private List<int> maiCampaignModePerks = new List<int>();
        private List<bool> mabGameOption = new List<bool>();
        private List<bool> mabStoryUsed = new List<bool>();
        private List<bool> mabCampaignModeWon = new List<bool>();
        private List<LevelStateType> maeLevelState = new List<LevelStateType>();
        private List<PerkType> maeLevelPerk = new List<PerkType>();
        private List<ColonyType> maeLevelColony = new List<ColonyType>();
        private List<ColonyClassType> maeLevelColonyClass = new List<ColonyClassType>();
        private List<SevenSolsType> maeLevelSevenSols = new List<SevenSolsType>();
        private List<ResourcePresenceType> maeLevelResourcePresence = new List<ResourcePresenceType>();
        private List<EventLevelType> maeLevelEvent = new List<EventLevelType>();
        private List<CorporationType> maeLevelWinner = new List<CorporationType>();

        private List<List<bool>> maabLevelBlackMarket = new List<List<bool>>();
        private List<List<CorporationType>> maaeLevelCorporation = new List<List<CorporationType>>();

        private static string CampaignSaveFileName = "/CampaignSave.bin";
        private static string CampaignSaveFullPath { get { return Globals.AppInfo.UserCampaignSavePath + CampaignSaveFileName; } }

        private static string CampaignAutoFileName = "/CampaignAuto.bin";
        private static string CampaignAutoFullPath { get { return Globals.AppInfo.UserCampaignSavePath + CampaignAutoFileName; } }

        List<TurnResult> mTurnResults = new List<TurnResult>();

        public const int STOCK_DIVISOR = 1000;
        public const int MAX_OPEN_MISSIONS = 3;
        public const int MAX_MISSION_AI = 3;

        public virtual void init(string humanPlayerName, CampaignModeType eCampaignMode)
        {
            mbShowPopups = !(!mbShowPopups); // XXX unused but left for save compatibility

            meCampaignMode = eCampaignMode;

            miActiveCorporations = campaignMode().miStartingCorps;
            meCampaignState = CampaignState.GROWTH_ROUND;

            mapCorporations.Clear();
            for (int i = 0; i < campaignMode().miStartingCorps; i++)
            {
                Corporation pLoopCorporation = new Corporation();

                pLoopCorporation.meID = (CorporationType)i;

                pLoopCorporation.mzName = (pLoopCorporation.meID == CorporationType.HUMAN) ? humanPlayerName : "";

                pLoopCorporation.miMoney = campaignMode().miStartingMoney;

                pLoopCorporation.maiTempPerkCount = Enumerable.Repeat(0, (int)Globals.Infos.perksNum()).ToList();
                pLoopCorporation.maiPermPerkCount = Enumerable.Repeat(0, (int)Globals.Infos.perksNum()).ToList();
                pLoopCorporation.maiLevelPerkCount = Enumerable.Repeat(0, (int)Globals.Infos.perksNum()).ToList();
                pLoopCorporation.maiLevelPerkTime = Enumerable.Repeat(0, (int)Globals.Infos.perksNum()).ToList();

                pLoopCorporation.mabEventTurnSeen = Enumerable.Repeat(false, (int)Globals.Infos.eventTurnsNum()).ToList();

                pLoopCorporation.maeColonyBonusLevel = Enumerable.Repeat(campaignMode().meStartingColonyBonusLevel, (int)Globals.Infos.coloniesNum()).ToList();

                mapCorporations.Add(pLoopCorporation);
                maazEventText.Add(new List<string>());
            }

            maiResourcePrice = Enumerable.Repeat(0, (int)Globals.Infos.resourcesNum()).ToList();
            maiResourcePriceOffworld = Enumerable.Repeat(0, (int)Globals.Infos.resourcesNum()).ToList();

            maiPerksAvailable = Enumerable.Repeat(0, (int)Globals.Infos.perksNum()).ToList();
            maiPermPerksBought = Enumerable.Repeat(0, (int)Globals.Infos.perksNum()).ToList();
            maiTempPerksBought = Enumerable.Repeat(0, (int)Globals.Infos.perksNum()).ToList();
            maiLevelPopulation = Enumerable.Repeat(0, (int)Globals.Infos.levelsNum()).ToList();
            maiLevelTurn = Enumerable.Repeat(0, (int)Globals.Infos.levelsNum()).ToList();
            maiCampaignModePerks.Clear();

            mabGameOption.Clear();
            for (GameOptionType eLoopGameOption = 0; eLoopGameOption < Globals.Infos.gameOptionsNum(); eLoopGameOption++)
            {
                mabGameOption.Add(Globals.Infos.gameOption(eLoopGameOption).mbDefaultValueCampaign);
            }
            mabStoryUsed = Enumerable.Repeat(false, (int)Globals.Infos.storiesNum()).ToList();
            mabCampaignModeWon.Clear();

            maeLevelState = Enumerable.Repeat(LevelStateType.UNOPENED, (int)Globals.Infos.levelsNum()).ToList();
            maeLevelPerk = Enumerable.Repeat(PerkType.NONE, (int)Globals.Infos.levelsNum()).ToList();
            maeLevelColony = Enumerable.Repeat(ColonyType.NONE, (int)Globals.Infos.levelsNum()).ToList();
            maeLevelColonyClass = Enumerable.Repeat(ColonyClassType.NONE, (int)Globals.Infos.levelsNum()).ToList();
            maeLevelSevenSols = Enumerable.Repeat(SevenSolsType.NONE, (int)Globals.Infos.levelsNum()).ToList();
            maeLevelResourcePresence = Enumerable.Repeat(ResourcePresenceType.NONE, (int)Globals.Infos.levelsNum()).ToList();
            maeLevelEvent = Enumerable.Repeat(EventLevelType.NONE, (int)Globals.Infos.levelsNum()).ToList();
            maeLevelWinner = Enumerable.Repeat(CorporationType.NONE, (int)Globals.Infos.levelsNum()).ToList();

            maabLevelBlackMarket.Clear();
            maaeLevelCorporation.Clear();

            for (int i = 0; i < Globals.Infos.levels().Count; i++)
            {
                maabLevelBlackMarket.Add(Enumerable.Repeat(false, (int)Globals.Infos.blackMarketsNum()).ToList());
                maaeLevelCorporation.Add(new List<CorporationType>());
            }
        }

        public virtual void start(ExecutiveType eExecutive, CampaignSettings settings, List<int> aiCampaignModePerks, List<int> aiCampaignModeExecutives, List<bool> abCampaignModeWon)
        {
            if ((settings.mode != CampaignModeType.NONE) && (Globals.Infos.campaignMode(settings.mode).miSeed != 0))
            {
                miSeed = Globals.Infos.campaignMode(settings.mode).miSeed;
            }
            else if (settings.gameOptionsDictionary[GameOptionType.DAILY_SEED])
            {
                miSeed = Globals.AppInfo.GetDailyChallengeSeed;
            }
            else
            {
                miSeed = ((int)(System.DateTime.Now.Ticks % 10000));
            }

            mRandom = new CrossPlatformRandom(getSeed());
            mRandomSim = new CrossPlatformRandom(getSeed());
            mRandomAI = new CrossPlatformRandom(getSeed());

            meHandicap = settings.handicap;
            meLocation = settings.location;

            maiCampaignModePerks = aiCampaignModePerks;

            mabCampaignModeWon = abCampaignModeWon;

            List<bool> abExecutivesActive = Enumerable.Repeat(false, (int)Globals.Infos.executivesNum()).ToList();

            abExecutivesActive[(int)eExecutive] = true;

            foreach (Corporation pLoopCorporation in getCorporations())
            {
                if (pLoopCorporation.meID != CorporationType.HUMAN)
                {
                    ExecutiveType eBestExecutive = ExecutiveType.NONE;
                    int iBestValue = 0;

                    foreach (InfoExecutive pLoopExecutive in Globals.Infos.executives())
                    {
                        if (!(pLoopExecutive.mbIoDLC) || Globals.AppInfo.OwnsDLCIo)
                        {
                            if (!(abExecutivesActive[pLoopExecutive.miType]))
                            {
                                int iValue = mRandom.Next(1000) + 1;

                                iValue += (aiCampaignModeExecutives[pLoopExecutive.miType] * 1000);

                                if (iValue > iBestValue)
                                {
                                    eBestExecutive = pLoopExecutive.meType;
                                    iBestValue = iValue;
                                }
                            }
                        }
                    }

                    pLoopCorporation.meExecutive = eBestExecutive;
                    pLoopCorporation.mzName = TEXT(Globals.Infos.character(Globals.Infos.personality(Globals.Infos.executive(pLoopCorporation.meExecutive).mePersonality).meCharacter).meName);

                    abExecutivesActive[(int)eBestExecutive] = true;
                }
                else
                {
                    pLoopCorporation.meExecutive = eExecutive;
                }

                pLoopCorporation.mePersonality = Globals.Infos.executive(pLoopCorporation.meExecutive).mePersonality;
                pLoopCorporation.mePlayerColor = Globals.Infos.character(Globals.Infos.personality(pLoopCorporation.mePersonality).meCharacter).mePlayerColor;
            }

            foreach (Corporation pLoopCorporation in getCorporations())
            {
                foreach (Corporation pOtherCorporation in getCorporations())
                {
                    if (pOtherCorporation.meID < pLoopCorporation.meID)
                    {
                        if (pOtherCorporation.mePlayerColor == pLoopCorporation.mePlayerColor)
                        {
                            pLoopCorporation.mePlayerColor = PlayerColorType.NONE;
                        }
                    }
                }

                if (pLoopCorporation.mePlayerColor == PlayerColorType.NONE)
                {
                    PlayerColorType eBestPlayerColor = PlayerColorType.NONE;
                    int iBestValue = 0;

                    for (PlayerColorType eLoopPlayerColor = 0; eLoopPlayerColor < Globals.Infos.playerColorsNum(); eLoopPlayerColor++)
                    {
                        bool bValid = true;

                        foreach (Corporation pOtherCorporation in getCorporations())
                        {
                            if (pOtherCorporation.mePlayerColor == eLoopPlayerColor)
                            {
                                bValid = false;
                                break;
                            }
                        }
                        
                        if (bValid)
                        {
                            int iValue = mRandom.Next(1000) + 1;
                            if (iValue > iBestValue)
                            {
                                eBestPlayerColor = eLoopPlayerColor;
                                iBestValue = iValue;
                            }
                        }
                    }

                    pLoopCorporation.mePlayerColor = eBestPlayerColor;
                }
            }

            foreach (Corporation pLoopCorporation in getCorporations())
            {
                foreach (InfoPerk pLoopPerk in Globals.Infos.perks())
                {
                    pLoopCorporation.maiPermPerkCount[pLoopPerk.miType] += getOriginalPerkCount(pLoopCorporation, pLoopPerk.meType);

                    if (pLoopCorporation.meID == CorporationType.HUMAN)
                    {
                        pLoopCorporation.maiPermPerkCount[pLoopPerk.miType] += aiCampaignModePerks[pLoopPerk.miType];
                    }
                }
            }

            for (LevelType eLoopLevel = 0; eLoopLevel < Globals.Infos.levelsNum(); eLoopLevel++)
            {
                fillLevelSettings(eLoopLevel);
            }

            for (int i = 0; i < settings.gameOptionsDictionary.Count; i++)
            {
                KeyValuePair<GameOptionType, bool> value = settings.gameOptionsDictionary.ElementAt(i);
                mabGameOption[(int)value.Key] = value.Value;
            }

            foreach (InfoResource pLoopResource in Globals.Infos.resources())
            {
                {
                    int iPrice = pLoopResource.miMarketPrice;

                    if (isGameOption(GameOptionType.RANDOM_PRICES))
                    {
                        switch (mRandom.Next(4))
                        {
                            case 0:
                                iPrice /= 2;
                                break;

                            case 1:
                            case 2:
                                break;

                            case 3:
                                iPrice *= 2;
                                break;
                        }
                    }

                    maiResourcePrice[pLoopResource.miType] = iPrice;
                }

                if (pLoopResource.mbTrade)
                {
                    int iOffworldPrice = (pLoopResource.miOffworldPrice + mRandom.Next(pLoopResource.miOffworldRand));

                    if (iOffworldPrice > 10)
                    {
                        iOffworldPrice -= (iOffworldPrice % 10);
                    }
                    else
                    {
                        iOffworldPrice = 10;
                    }

                    maiResourcePriceOffworld[pLoopResource.miType] = iOffworldPrice;
                }
            }

            updateCampaignState();

            sortCorporations();

            doEventGame();

            openLevels();

            AI_buyPerks();

            fillPerks();

            Serialize(false, true);
        }

        public virtual void fillLevelSettings(LevelType eLevel)
        {
            maeLevelResourcePresence[(int)eLevel] = (ResourcePresenceType)mRandom.Next((int)Globals.Infos.resourcePresencesNum());

            {
                int iSlots = Globals.Infos.Globals.BLACK_MARKET_SLOTS;
                int iCount = 0;

                while (iCount < iSlots)
                {
                    BlackMarketType eBestBlackMarket = BlackMarketType.NONE;
                    int iBestValue = 0;

                    foreach (InfoBlackMarket pLoopBlackMarket in Globals.Infos.blackMarkets())
                    {
                        if (GameServer.isValidBlackMarket(pLoopBlackMarket.meType, getGameOptions(), 2, Globals.Infos))
                        {
                            if (!maabLevelBlackMarket[(int)eLevel][pLoopBlackMarket.miType])
                            {
                                int iValue = mRandom.Next(pLoopBlackMarket.miAppearanceProb + pLoopBlackMarket.maiLocationAppearanceModifiers[(int)getLocation()]);
                                if (iValue > iBestValue)
                                {
                                    eBestBlackMarket = pLoopBlackMarket.meType;
                                    iBestValue = iValue;
                                }
                            }
                        }
                    }

                    if (eBestBlackMarket != BlackMarketType.NONE)
                    {
                        maabLevelBlackMarket[(int)eLevel][(int)eBestBlackMarket] = true;
                        iCount++;
                    }
                    else
                    {
                        break;
                    }
                }

                {
                    int iTriggersDefense = 0;

                    foreach (InfoBlackMarket pLoopBlackMarket in Globals.Infos.blackMarkets())
                    {
                        if (maabLevelBlackMarket[(int)eLevel][pLoopBlackMarket.miType])
                        {
                            SabotageType eSabotage = pLoopBlackMarket.meSabotage;

                            if (eSabotage != SabotageType.NONE)
                            {
                                if (Globals.Infos.sabotage(eSabotage).mbTriggersDefense)
                                {
                                    iTriggersDefense++;
                                }
                            }
                        }
                    }

                    foreach (InfoBlackMarket pLoopBlackMarket in Globals.Infos.blackMarkets())
                    {
                        if (GameServer.isValidBlackMarket(pLoopBlackMarket.meType, getGameOptions(), 2, Globals.Infos))
                        {
                            if (!maabLevelBlackMarket[(int)eLevel][pLoopBlackMarket.miType])
                            {
                                if (pLoopBlackMarket.miTriggersDefense > 0)
                                {
                                    if (pLoopBlackMarket.miTriggersDefense <= iTriggersDefense)
                                    {
                                        maabLevelBlackMarket[(int)eLevel][pLoopBlackMarket.miType] = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public virtual void doTurn()
        {
            createAIResults();

            doPerkTime();

            processResults();

            doElimination();

            doMoney();

            sortCorporations();

            setAutoSave(true);
            setShowEvent(true);

            miTurn++;

            if (getHuman().mbDead)
            {
                meCampaignState = CampaignState.LOST;
            }
            else if (miActiveCorporations == 1)
            {
                meCampaignState = CampaignState.WON;
            }
            else
            {
                updateCampaignState();

                doEvents();

                doEventGame();

                openLevels();

                AI_buyPerks();

                fillPerks();

                fillLevelEvents();
            }
        }

        public virtual void doElimination()
        {
            if (getCampaignState() == CampaignState.ELIMINATION_ROUND)
            {
                CorporationType eBestCorporation = CorporationType.NONE;
                int iBestValue = int.MaxValue;

                foreach (Corporation pLoopCorporation in getCorporations())
                {
                    bool bWinner = false;

                    foreach (LevelResult levelResult in mTurnResults[getTurn()].mLevelResults)
                    {
                        if (levelResult.meLevel == getHuman().meCurrentLevel)
                        {
                            if (levelResult.meWinner == pLoopCorporation.meID)
                            {
                                bWinner = true;
                                break;
                            }
                        }
                    }

                    if (!bWinner)
                    {
                        if (!(pLoopCorporation.mbDead))
                        {
                            int iValue = calculateSharePrice(pLoopCorporation);
                            if (iValue < iBestValue)
                            {
                                eBestCorporation = pLoopCorporation.meID;
                                iBestValue = iValue;
                            }
                        }
                    }
                }

                if (eBestCorporation != CorporationType.NONE)
                {
                    killCorporation(eBestCorporation);

                    addEventText(eBestCorporation, getTurn(), TEXT("TEXT_CAMPAIGN_CORPORATION_ELIMINATED"));
                }
            }
        }

        public virtual void doMoney()
        {
            foreach (Corporation pLoopCorporation in getCorporations())
            {
                if (!(pLoopCorporation.mbDead))
                {
                    int iIncome = calculateIncome(pLoopCorporation.meID);

                    addEventText(pLoopCorporation.meID, getTurn(), TEXT("TEXT_CAMPAIGN_INCOME_DISPLAY", Utils.makeMoney(iIncome, false, true, true).ToText()));

                    //addEventText(pLoopCorporation.meID, getTurn(), GuiUtils.bulletText(TEXT("TEXT_CAMPAIGN_SCREEN_TOOLTIP_REVENUE_BASE", Utils.makeMoney(campaignMode().miBaseIncome, true, true, true).ToText())));

                    if (pLoopCorporation.meID != CorporationType.HUMAN)
                    {
                        int iAIIncome = Globals.Infos.handicap(Globals.Campaign.getHandicap()).miAICampaignIncome;
                        if (iAIIncome != 0)
                        {
                            //addEventText(pLoopCorporation.meID, getTurn(), GuiUtils.bulletText(TEXT("TEXT_CAMPAIGN_SCREEN_TOOLTIP_REVENUE_HANDICAP", Globals.Infos.handicap(Globals.Campaign.getHandicap()).meName.ToText(), Utils.makeMoney(iAIIncome, true, true, true).ToText())));
                        }
                    }

                    foreach (InfoLevel pLoopLevel in Globals.Infos.levels())
                    {
                        int iLevelIncome = getLevelCorporationIncome(pLoopLevel.meType, pLoopCorporation.meID);
                        if (iLevelIncome != 0)
                        {
                            //addEventText(pLoopCorporation.meID, getTurn(), GuiUtils.bulletText(TEXT("TEXT_CAMPAIGN_LEVEL_INCOME_DISPLAY", pLoopLevel.meName.ToText(), Utils.makeMoney(iLevelIncome, true, true, true).ToText())));
                        }
                    }

                    addEventText(pLoopCorporation.meID, getTurn(), TEXT("TEXT_CAMPAIGN_FINAL_CASH", Utils.makeMoney(pLoopCorporation.miMoney + iIncome, false, true, true).ToText()));

                    int iOldDebt = pLoopCorporation.miDebt;
                    if (iOldDebt != 0)
                    {
                        addEventText(pLoopCorporation.meID, getTurn(), TEXT("TEXT_CAMPAIGN_FINAL_DEBT", Utils.makeMoney(pLoopCorporation.miDebt, false, true, true).ToText()));
                    }

                    pLoopCorporation.miMoney += iIncome;

                    if (pLoopCorporation.miMoney < 0)
                    {
                        pLoopCorporation.miDebt += pLoopCorporation.miMoney;
                        pLoopCorporation.miMoney = 0;
                    }

                    int iPayment = Math.Min(-(pLoopCorporation.miDebt), pLoopCorporation.miMoney);
                    if (iPayment > 0)
                    {
                        pLoopCorporation.miDebt += iPayment;
                        pLoopCorporation.miMoney -= iPayment;
                    }

                    if (iOldDebt != 0)
                    {
                        addEventText(pLoopCorporation.meID, getTurn(), "-----------------");
                        addEventText(pLoopCorporation.meID, getTurn(), TEXT("TEXT_CAMPAIGN_CASH_AFTER_PAY_DEBT", Utils.makeMoney(pLoopCorporation.miMoney, false, true, true).ToText()));

                        if (pLoopCorporation.miDebt != 0)
                        {
                            addEventText(pLoopCorporation.meID, getTurn(), TEXT("TEXT_CAMPAIGN_REMAINING_DEBT", Utils.makeMoney(pLoopCorporation.miDebt, false, true, true).ToText()));
                        }
                    }
                }
            }
        }

        public virtual int getInterestPayment(Corporation pCorporation)
        {
            int iPayment = pCorporation.miDebt;

            iPayment *= campaignMode().miDebtInterest;
            iPayment /= 100;

            return iPayment;
        }

        public void doInterest()
        {
            foreach (Corporation pLoopCorporation in getCorporations())
            {
                if (!(pLoopCorporation.mbDead))
                {
                    addEventText(pLoopCorporation.meID, getTurn(), TEXT("TEXT_CAMPAIGN_BEFORE_MISSION_TITLE"));
                    addEventText(pLoopCorporation.meID, getTurn(), TEXT("TEXT_CAMPAIGN_CASH", Utils.makeMoney(pLoopCorporation.miMoney, false, true, true).ToText()));

                    if (pLoopCorporation.miDebt != 0)
                    {
                        addEventText(pLoopCorporation.meID, getTurn(), TEXT("TEXT_CAMPAIGN_DEBT_BEFORE", Utils.makeMoney(pLoopCorporation.miDebt, false, true, true).ToText()));

                        int iInterest = getInterestPayment(pLoopCorporation);

                        pLoopCorporation.miDebt += iInterest;

                        addEventText(pLoopCorporation.meID, getTurn(), TEXT("TEXT_CAMPAIGN_INTEREST_PAYMENT", Utils.makeMoney(iInterest, false, true, true).ToText()));
                        addEventText(pLoopCorporation.meID, getTurn(), TEXT("TEXT_CAMPAIGN_DEBT_AFTER", Utils.makeMoney(pLoopCorporation.miDebt, false, true, true).ToText()));
                    }

                    addEventText(pLoopCorporation.meID, getTurn(), "-----------------");
                }
            }
        }

        public virtual void doPerkTime()
        {
            foreach (Corporation pLoopCorporation in getCorporations())
            {
                pLoopCorporation.maiTempPerkCount = Enumerable.Repeat(0, (int)Globals.Infos.perksNum()).ToList();

                foreach (InfoPerk pLoopPerk in Globals.Infos.perks())
                {
                    int iCount = pLoopCorporation.maiLevelPerkCount[pLoopPerk.miType];
                    if (iCount > 0)
                    {
                        int iTime = pLoopCorporation.maiLevelPerkTime[pLoopPerk.miType];

                        iTime = Math.Max(0, (iTime - iCount));

                        if (iTime == 0)
                        {
                            pLoopCorporation.maiLevelPerkCount[pLoopPerk.miType] = 0;
                        }
                        else if (iTime < iCount)
                        {
                            pLoopCorporation.maiLevelPerkCount[pLoopPerk.miType] = iTime;
                        }

                        pLoopCorporation.maiLevelPerkTime[pLoopPerk.miType] = iTime;
                    }
                }
            }
        }

        public virtual bool canStartMission(LevelType eLevel, CorporationType eCorporation)
        {
            if (eLevel == LevelType.NONE)
            {
                return false;
            }

            if (getLevelState(eLevel) != LevelStateType.OPENED)
            {
                return false;
            }

            Corporation pCorporation = getCorporation(eCorporation);

            if (pCorporation.mbDead)
            {
                return false;
            }

            if ((getCampaignState() == CampaignState.LOST) ||
                (getCampaignState() == CampaignState.WON))
            {
                return false;
            }
            else if (getCampaignState() == CampaignState.FINAL_ROUND)
            {
                if (pCorporation.meID != CorporationType.HUMAN)
                {
                    if (getHuman().meCurrentLevel != eLevel)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public virtual void adjustPerkCost(ref int iCost, bool bPermanent)
        {
            if (bPermanent)
            {
                int iTurnsLeft = getTurnsLeft();
                if (iTurnsLeft > 1)
                {
                    iCost *= (iTurnsLeft + 3);
                    iCost /= 2;

                    iCost -= (iCost % 10000);
                }
            }

            iCost *= 100 + Math.Max(0, (getTurn() - (getTurnsTotal() / 2) + 1) * 25);
            iCost /= 100;

            if (getCampaignState() == CampaignState.FINAL_ROUND)
            {
                iCost *= Math.Max(0, (100 + campaignMode().miFinalPerkCostModifier));
                iCost /= 100;
            }
        }

        public virtual int getPerkCost(PerkType ePerk, Corporation pCorporation, bool bPermanent)
        {
            int iCost = Globals.Infos.perk(ePerk).miCost;

            if (bPermanent)
            {
                adjustPerkCost(ref iCost, bPermanent);
            }

            for (ColonyType eLoopColony = 0; eLoopColony < Globals.Infos.coloniesNum(); eLoopColony++)
            {
                ColonyBonusLevelType eColonyBonusLevel = pCorporation.maeColonyBonusLevel[(int)eLoopColony];

                if (eColonyBonusLevel != ColonyBonusLevelType.NONE)
                {
                    int iModifier = Globals.Infos.colonyBonus(Globals.Infos.colony(eLoopColony).maeColonyBonus[(int)eColonyBonusLevel]).miPerkCostModifier;
                    if (iModifier != 0)
                    {
                        iCost *= Math.Max(0, (100 + iModifier));
                        iCost /= 100;
                    }
                }
            }

            for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
            {
                int iModifier = Globals.Infos.perk(eLoopPerk).maiPerkCostModifier[(int)ePerk];
                if (iModifier != 0)
                {
                    int iCount = pCorporation.getPerkCount(eLoopPerk);

                    for (int iI = 0; iI < iCount; iI++)
                    {
                        iCost *= Math.Max(0, (100 + iModifier));
                        iCost /= 100;
                    }
                }
            }

            iCost -= (iCost % 10000);

            return Math.Max(10000, iCost);
        }

        public virtual bool canBuyPerkPermanent(bool bPermanent)
        {
            if (getCampaignState() == CampaignState.FINAL_ROUND)
            {
                return false;
            }

            if (getTurn() < campaignMode().miFirstTurnBuyPermanent)
            {
                return false;
            }

            return true;
        }

        public virtual bool canBuyPerk(CorporationType eCorporation, PerkType ePerk, bool bPermanent)
        {
            Corporation pCorporation = getCorporation(eCorporation);

            if (pCorporation.miMoney < getPerkCost(ePerk, pCorporation, bPermanent))
            {
                return false;
            }

            if (bPermanent)
            {
                if (!canBuyPerkPermanent(bPermanent))
                {
                    return false;
                }
            }

            if (eCorporation == CorporationType.HUMAN)
            {
                if ((getPerksAvailable(ePerk) - getPermPerksBought(ePerk) - getTempPerksBought(ePerk)) == 0)
                {
                    return false;
                }
            }

            return true;
        }

        public virtual void buyPerk(CorporationType eCorporation, PerkType ePerk, bool bPermanent)
        {
            if (!canBuyPerk(eCorporation, ePerk, bPermanent))
            {
                return;
            }

            Corporation pCorporation = getCorporation(eCorporation);

            pCorporation.miMoney -= getPerkCost(ePerk, pCorporation, bPermanent);

            if (bPermanent)
            {
                pCorporation.maiPermPerkCount[(int)ePerk]++;

                if (eCorporation == CorporationType.HUMAN)
                {
                    maiPermPerksBought[(int)ePerk]++;
                }
            }
            else
            {
                pCorporation.maiTempPerkCount[(int)ePerk]++;

                if (eCorporation == CorporationType.HUMAN)
                {
                    maiTempPerksBought[(int)ePerk]++;
                }
            }

            sortCorporations();
        }

        public virtual bool canReturnPerk(CorporationType eCorporation, PerkType ePerk, bool bPermanent)
        {
            if (eCorporation != CorporationType.HUMAN)
            {
                return false;
            }

            if (bPermanent)
            {
                if (getPermPerksBought(ePerk) == 0)
                {
                    return false;
                }
            }
            else
            {
                if (getTempPerksBought(ePerk) == 0)
                {
                    return false;
                }
            }

            return true;
        }

        public virtual void returnPerk(CorporationType eCorporation, PerkType ePerk, bool bPermanent)
        {
            if (!canReturnPerk(eCorporation, ePerk, bPermanent))
            {
                return;
            }

            Corporation pCorporation = getCorporation(eCorporation);

            pCorporation.miMoney += getPerkCost(ePerk, pCorporation, bPermanent);

            if (bPermanent)
            {
                pCorporation.maiPermPerkCount[(int)ePerk]--;

                maiPermPerksBought[(int)ePerk]--;
            }
            else
            {
                pCorporation.maiTempPerkCount[(int)ePerk]--;

                maiTempPerksBought[(int)ePerk]--;
            }

            sortCorporations();
        }

        public virtual void addResult(LevelResult result)
        {
            if (mTurnResults.Count - 1 < miTurn)
            {
                mTurnResults.Add(new TurnResult());
            }

            mTurnResults[getTurn()].addResult(result);
        }

        public virtual void createAIResults()
        {
            for (LevelType eLoopLevel = 0; eLoopLevel < Globals.Infos.levelsNum(); eLoopLevel++)
            {
                if ((getLevelState(eLoopLevel) == LevelStateType.OPENED) && getHuman().meCurrentLevel != eLoopLevel)
                {
                    List<Corporation> corporationsInLevel = getCorporations().Where(o => o.meCurrentLevel == eLoopLevel).ToList();

                    if (corporationsInLevel.Count > 0)
                    {
                        if (isGameOption(GameOptionType.SIM_MISSIONS))
                        {
                            GameSettings settings = new GameSettings();

                            int numPlayers = countCorporations(eLoopLevel);

                            settings.miNumPlayers = numPlayers;

                            settings.miSeed = Globals.Campaign.getSeed() + Globals.Campaign.getTurn() + (int)eLoopLevel;

                            Debug.Log("[App] Seed: " + settings.miSeed);

                            settings.meGameSpeed = (GameSpeedType)(Globals.Infos.gameSpeedsNum() - 1);

                            settings.meTerrainClass = Globals.Infos.level(eLoopLevel).meTerrainClass;

                            settings.meResourceMinimum = Globals.Infos.Globals.DEFAULT_RESOURCEMINIMUM;
                            settings.meResourcePresence = Globals.Campaign.getLevelResourcePresence(eLoopLevel);
                            settings.meColonyClass = getLevelColonyClass(eLoopLevel);
                            settings.meGameSetupType = Globals.Infos.Globals.DEFAULT_GAMESETUP;
                            settings.meRulesSetType = Globals.Infos.Globals.DEFAULT_RULESSET;
                            settings.meLevelType = eLoopLevel;
                            settings.meLocation = Globals.Infos.level(eLoopLevel).meLocation;

                            settings.mzMap = "";
                            settings.mzMapName = "";

                            settings.meMapSize = Globals.Infos.level(eLoopLevel).meMapSize;

                            int iWidth = Globals.Infos.mapSize(settings.meMapSize).miWidth;
                            int iHeight = Globals.Infos.mapSize(settings.meMapSize).miHeight;

                            iWidth *= Mathf.Max(0, (Globals.Infos.terrainClass(settings.meTerrainClass).miSizeModifier + 100));
                            iWidth /= 100;

                            iHeight *= Mathf.Max(0, (Globals.Infos.terrainClass(settings.meTerrainClass).miSizeModifier + 100));
                            iHeight /= 100;

                            settings.miWidth = iWidth;
                            settings.miHeight = iHeight;
                            settings.miEdgeTilePadding = Constants.DEFAULT_EDGE_TILE_PADDING;

                            settings.meLatitude = Globals.Infos.level(eLoopLevel).meLatitude;

                            //**************** Fill in game options ********************
                            for (int i = 0; i < (int)(Globals.Infos.gameOptionsNum()); i++)
                            {
                                settings.mabGameOptions.Add(isGameOption((GameOptionType)i));
                            }

                            settings.mabGameOptions[(int)GameOptionType.SEVEN_SOLS] = true;

                            //fill in active player teams
                            for (int iCount = 0; iCount < numPlayers; iCount++)
                                settings.playerSlots.Add(new PlayerSettings("", (sbyte)iCount, Globals.Infos.Globals.DEFAULT_HANDICAP, GenderType.MALE, null));

                            //fill in the rest of the slots with defaults
                            while(settings.playerSlots.Count < Constants.MAX_LOBBY_SLOTS)
                                settings.playerSlots.Add(new PlayerSettings("", 0, Globals.Infos.Globals.DEFAULT_HANDICAP, GenderType.MALE, null));

                            settings.miNumUniqueTeams = numPlayers;

                            GameServer pGame = Globals.Factory.createGameServer();
                            pGame.init(Globals.Infos, settings);

                            while (!(pGame.isGameOver()))
                            {
                                pGame.doUpdate();
                            }
                        }
                        else
                        {
                            int iMaxPopulation = GameClient.getColonyCap(true, corporationsInLevel.Count, getLevelColonyClass(eLoopLevel), Globals.Infos);
                            int iPopulation = Math.Min(iMaxPopulation, (iMaxPopulation * Math.Min(100, (60 + mRandomSim.Next(40) + (getTurn() * 5))) / 100));

                            List<CorporationType> aeCorporationIDs = new List<CorporationType>();
                            List<int> aiCorporationColonyShares = new List<int>();
                            List<int> aiCorporationHQs = new List<int>();
                            List<int> aiCorporationCash = new List<int>();
                            List<int> aiCorporationDebt = new List<int>();
                            List<int> aiCorporationStructures = new List<int>();
                            List<int> aiCorporationModules = new List<int>();
                            List<int> aiResourcePrices = new List<int>();
                            List<int> aiResourcePricesOffworld = new List<int>();

                            foreach (Corporation pLoopCorporation in corporationsInLevel)
                            {
                                aeCorporationIDs.Add(pLoopCorporation.meID);
                                aiCorporationColonyShares.Add(1);
                                aiCorporationHQs.Add(GameClient.calculateHQValueBase((int)(Globals.Infos.HQLevelsNum() - 1), Globals.Infos));
                                aiCorporationCash.Add((mRandomSim.Next(100) + mRandomSim.Next(100) + mRandomSim.Next(100) + (getTurn() * 50)) * 1000);
                                aiCorporationDebt.Add(-(((mRandomSim.Next(5) == 0) ? 0 : mRandomSim.Next(100)) * 1000));
                                aiCorporationStructures.Add((mRandomSim.Next(50) + 150 + (getTurn() * 20)) * 1000);
                                aiCorporationModules.Add(PlayerClient.getModuleValue(1, Globals.Infos));
                            }

                            for (int i = Globals.Infos.Globals.SEVEN_SOLS_MODULES; i < (iPopulation * 2); i++)
                            {
                                int iIndex = mRandomSim.Next(aeCorporationIDs.Count);

                                aiCorporationColonyShares[iIndex]++;
                                aiCorporationModules[iIndex] += PlayerClient.getModuleValue(1, Globals.Infos);
                            }

                            for (int i = 0; i < (int)Globals.Infos.resourcesNum(); i++)
                            {
                                aiResourcePrices.Add(0);
                                aiResourcePricesOffworld.Add(0);
                            }

                            CorporationType eWinner = CorporationType.NONE;

                            {
                                int iBestValue = 0;

                                foreach (int iValue in aiCorporationColonyShares)
                                {
                                    iBestValue = Math.Max(iValue, iBestValue);
                                }

                                int iCount = 0;

                                foreach (int iValue in aiCorporationColonyShares)
                                {
                                    if (iValue == iBestValue)
                                    {
                                        iCount++;
                                    }
                                }

                                for (int iIndex = 0; iIndex < corporationsInLevel.Count; iIndex++)
                                {
                                    if (aiCorporationColonyShares[iIndex] == iBestValue)
                                    {
                                        if (iCount > 1)
                                        {
                                            aiCorporationColonyShares[iIndex]++;
                                        }

                                        eWinner = aeCorporationIDs[iIndex];
                                        break;
                                    }
                                }
                            }

                            addResult(new LevelResult(eLoopLevel, eWinner, iPopulation, aeCorporationIDs, aiCorporationColonyShares, aiCorporationHQs, aiCorporationCash, aiCorporationDebt, aiCorporationStructures, aiCorporationModules, aiResourcePrices, aiResourcePricesOffworld));
                        }
                    }
                }
            }
        }

        public virtual bool humanWonTurn(int iTurn)
        {
            foreach (LevelResult levelResult in mTurnResults[iTurn].mLevelResults)
            {
                if (levelResult.meWinner == CorporationType.HUMAN)
                {
                    return true;
                }
            }

            return false;
        }

        public virtual void processResults()
        {
            foreach (LevelResult levelResult in mTurnResults[getTurn()].mLevelResults)
            {
                processResult(levelResult);
            }

            foreach (InfoResource pLoopResource in Globals.Infos.resources())
            {
                int iPrice = 0;
                int iCount = 0;

                foreach (LevelResult levelResult in mTurnResults[getTurn()].mLevelResults)
                {
                    int iLevelPrice = levelResult.maiResourcePrices[pLoopResource.miType];
                    if (iLevelPrice != 0)
                    {
                        iPrice += iLevelPrice;
                        iCount++;
                    }
                }

                {
                    int iResults = mTurnResults[getTurn()].mLevelResults.Count;

                    for (int iPass = 1; iPass < iResults; iPass++)
                    {
                        int iExtraPrice = (mRandomSim.Next(pLoopResource.miMarketPrice / 2) * (getTurn() + 1)) + (pLoopResource.miMarketPrice / 2);

                        if (isGameOption(GameOptionType.RANDOM_PRICES))
                        {
                            switch (mRandomSim.Next(4))
                            {
                                case 0:
                                    iExtraPrice /= 2;
                                    break;

                                case 1:
                                case 2:
                                    break;

                                case 3:
                                    iExtraPrice *= 2;
                                    break;
                            }
                        }

                        iPrice += iExtraPrice;
                        iCount++;
                    }
                }

                if (iCount > 0)
                {
                    iPrice /= iCount;

                    if (iPrice > 10)
                    {
                        iPrice -= (iPrice % 10);
                    }
                    else
                    {
                        iPrice = 10;
                    }

                    setResourcePrice(pLoopResource.meType, iPrice);
                }
            }

            foreach (InfoResource pLoopResource in Globals.Infos.resources())
            {
                int iPrice = 0;
                int iCount = 0;

                foreach (LevelResult levelResult in mTurnResults[getTurn()].mLevelResults)
                {
                    int iLevelPrice = levelResult.maiResourcePricesOffworld[pLoopResource.miType];
                    if (iLevelPrice != 0)
                    {
                        iPrice += iLevelPrice;
                        iCount++;
                    }
                }

                if (iCount > 0)
                {
                    iPrice /= iCount;

                    if (iPrice > 10)
                    {
                        iPrice -= (iPrice % 10);
                    }
                    else
                    {
                        iPrice = 10;
                    }

                    setResourcePriceOffworld(pLoopResource.meType, iPrice);
                }
            }
        }

        private void processResult(LevelResult result)
        {
            setLevelState(result.meLevel, LevelStateType.CLOSED);

            makeLevelWinner(result.meLevel, result.meWinner);
            setLevelPopulation(result.meLevel, result.miPopulation);

            int iIndex = 0;

            foreach (CorporationType eLoopCorporation in result.maePlayerIDs)
            {
                Corporation pLoopCorporation = getCorporation(eLoopCorporation);

                pLoopCorporation.mdiLevelColonyShares.Add(result.meLevel, result.maiPlayerColonyShares[iIndex]);
                pLoopCorporation.miHQs += result.maiPlayerHQs[iIndex];
                pLoopCorporation.miMoney += result.maiPlayerCash[iIndex];
                pLoopCorporation.miDebt += result.maiPlayerDebt[iIndex];
                pLoopCorporation.miStructures += result.maiPlayerStructures[iIndex];
                pLoopCorporation.miModules += result.maiPlayerModules[iIndex];

                addEventText(pLoopCorporation.meID, getTurn(), TEXT("TEXT_CAMPAIGN_FROM_MISSION_TITLE"));
                addEventText(pLoopCorporation.meID, getTurn(), TEXT("TEXT_HUDINFO_CASH_RESOURCE_VALUE", Utils.makeMoney(result.maiPlayerCash[iIndex], false, true, true).ToText()));
                addEventText(pLoopCorporation.meID, getTurn(), TEXT("TEXT_CAMPAIGN_DEBT_DISPLAY", Utils.makeMoney(result.maiPlayerDebt[iIndex], false, true, true).ToText()));
                addEventText(pLoopCorporation.meID, getTurn(), "-----------------");

                iIndex++;
            }

            if (getCampaignState() == CampaignState.FINAL_ROUND)
            {
                foreach (CorporationType eLoopCorporation in result.maePlayerIDs)
                {
                    if (eLoopCorporation != result.meWinner)
                    {
                        killCorporation(eLoopCorporation);
                    }
                }
            }
        }

        public virtual List<TurnResult> getTurnResults()
        {
            return mTurnResults;
        }
        public virtual TurnResult getTurnResult(int turn)
        {
            return mTurnResults[turn];
        }

        public virtual  void openLevels()
        {
            List<PerkType> aePerkRolls = new List<PerkType>();

            for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
            {
                if (!(Globals.Infos.perk(eLoopPerk).mabLocationInvalid[(int)getLocation()]))
                {
                    if (!(Globals.Infos.perk(eLoopPerk).mbIoDLC) || Globals.AppInfo.OwnsDLCIo)
                    {
                        for (int i = 0; i < Globals.Infos.perk(eLoopPerk).miLevelRoll; i++)
                        {
                            bool bValid = true;
                            bool bTurnsLeft = false;

                            int iTurnsLeft = (getTurnsLeft() - 1);
                            if (iTurnsLeft <= 2)
                            {
                                if (Globals.Infos.perk(eLoopPerk).miLevelTime == iTurnsLeft)
                                {
                                    bTurnsLeft = true;
                                }
                                else
                                {
                                    bValid = false;
                                }
                            }

                            if (bValid)
                            {
                                if ((getTurn() >= Globals.Infos.perk(eLoopPerk).miLevelTurn) || bTurnsLeft)
                                {
                                    aePerkRolls.Add(eLoopPerk);
                                }
                            }
                        }
                    }
                }
            }

            foreach (Corporation pLoopCorporation in getCorporations())
            {
                pLoopCorporation.meCurrentLevel = LevelType.NONE;
            }

            int iLevelCountTarget = MAX_OPEN_MISSIONS;

            if (getCampaignState() == CampaignState.FINAL_ROUND)
            {
                if (campaignMode().mbOneFinal)
                {
                    iLevelCountTarget = 1;
                }
            }
            else
            {
                while ((miActiveCorporations - 1) < (iLevelCountTarget * 2))
                {
                    iLevelCountTarget--;
                }
            }

            HashSet<PerkType> seUsedPerks = new HashSet<PerkType>();
            HashSet<ColonyType> seUsedColonies = new HashSet<ColonyType>();
            HashSet<ColonyClassType> seUsedColonyClasses = new HashSet<ColonyClassType>();

            bool bCeresLevel = false;
            int iLevelCount = 0;

            while (iLevelCount < iLevelCountTarget)
            {
                LevelType eBestLevel = LevelType.NONE;

                {
                    int iBestValue = 0;

                    for (LevelType eLoopLevel = 0; eLoopLevel < Globals.Infos.levelsNum(); eLoopLevel++)
                    {
                        if (getLevelState(eLoopLevel) == LevelStateType.UNOPENED)
                        {
                            LocationType eLocation = Globals.Infos.level(eLoopLevel).meLocation;

                            bool bValid = true;

                            if (bValid)
                            {
                                if (!(campaignMode().mbLevelAll))
                                {
                                    if (!(campaignMode().mabLevelValid[(int)eLoopLevel]))
                                    {
                                        bValid = false;
                                    }
                                }
                            }

                            if (bValid)
                            {
                                if (eLocation == LocationType.CERES)
                                {
                                    if (!(Globals.AppInfo.OwnsDLCCeres) || !(isGameOption(GameOptionType.INCLUDE_CERES)) || campaignMode().mbNoCeres || (getTurn() == 0))
                                    {
                                        bValid = false;
                                    }
                                }
                                else
                                {
                                    if (eLocation != getLocation())
                                    {
                                        bValid = false;
                                    }
                                }
                            }

                            if (bValid)
                            {
                                if (Globals.Infos.level(eLoopLevel).mabModeInvalid[(int)(getCampaignMode())])
                                {
                                    bValid = false;
                                }
                            }

                            if (bValid)
                            {
                                int iValue = mRandom.Next(100) + 1;

                                if (eLocation == LocationType.CERES)
                                {
                                    if ((getTurn() > 0) && !bCeresLevel)
                                    {
                                        iValue += 100;
                                    }
                                }
                                else
                                {
                                    for (LevelType eAltLevel = 0; eAltLevel < Globals.Infos.levelsNum(); eAltLevel++)
                                    {
                                        if (getLevelState(eAltLevel) > LevelStateType.UNOPENED)
                                        {
                                            if (Globals.Infos.level(eLoopLevel).mabAdjacentLevel[(int)eAltLevel])
                                            {
                                                iValue += 100;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (iValue > iBestValue)
                                {
                                    eBestLevel = eLoopLevel;
                                    iBestValue = iValue;
                                }
                            }
                        }
                    }
                }

                if (eBestLevel != LevelType.NONE)
                {
                    if (Globals.Infos.level(eBestLevel).meLocation == LocationType.CERES)
                    {
                        bCeresLevel = true;
                    }

                    setLevelState(eBestLevel, LevelStateType.OPENED);
                    setLevelTurn(eBestLevel, getTurn());

                    if (aePerkRolls.Count > 0)
                    {
                        if (getCampaignState() != CampaignState.FINAL_ROUND)
                        {
                            PerkType ePerk = PerkType.NONE;

                            {
                                int iCount = 0;

                                while (true)
                                {
                                    ePerk = aePerkRolls[mRandom.Next(aePerkRolls.Count)];

                                    bool bSkip = false;

                                    for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                                    {
                                        if (Globals.Infos.perk(ePerk).mabPerkSkip[(int)eLoopPerk])
                                        {
                                            if (getHuman().getPerkCount(eLoopPerk) > 0)
                                            {
                                                bSkip = true;
                                                break;
                                            }
                                        }
                                    }

                                    if (seUsedPerks.Contains(ePerk) || (getHuman().getPerkCount(ePerk) > 0) || bSkip)
                                    {
                                        iCount++;

                                        if (iCount > 100)
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }

                            setLevelPerk(eBestLevel, ePerk);
                            seUsedPerks.Add(ePerk);
                        }
                    }

                    {
                        ColonyType eBestColony = ColonyType.NONE;

                        {
                            int iCount = 0;

                            while (true)
                            {
                                int iBestValue = 0;

                                for (ColonyType eLoopColony = 0; eLoopColony < Globals.Infos.coloniesNum(); eLoopColony++)
                                {
                                    int iValue = mRandom.Next(100) + 1;
                                    if (iValue > iBestValue)
                                    {
                                        eBestColony = eLoopColony;
                                        iBestValue = iValue;
                                    }
                                }

                                if (seUsedColonies.Contains(eBestColony))
                                {
                                    iCount++;

                                    if (iCount > 100)
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }

                        setLevelColony(eBestLevel, eBestColony);
                        seUsedColonies.Add(eBestColony);
                    }

                    if (Globals.AppInfo.OwnsDLCCampaign || (Globals.AppInfo.OwnsDLCIo && (getLocation() == LocationType.IO)))
                    {
                        if (getTurn() > 0)
                        {
                            TurnResult pTurnResult = getTurnResult(getTurn() - 1);

                            foreach (LevelResult pLoopLevelResult in pTurnResult.mLevelResults)
                            {
                                ColonyClassType eColonyClass = getLevelColonyClass(pLoopLevelResult.meLevel);

                                if (eColonyClass != ColonyClassType.NONE)
                                {
                                    seUsedColonyClasses.Add(eColonyClass);
                                }
                            }
                        }

                        ColonyClassType eBestColonyClass = ColonyClassType.NONE;
                        int iBestValue = 0;

                        for (ColonyClassType eLoopColonyClass = 0; eLoopColonyClass < Globals.Infos.colonyClassesNum(); eLoopColonyClass++)
                        {
                            if (!(Globals.Infos.colonyClass(eLoopColonyClass).mabLocationInvalid[(int)(Globals.Infos.level(eBestLevel).meLocation)]))
                            {
                                if (campaignMode().mbColonyClassAll || campaignMode().mabColonyClassValid[(int)eLoopColonyClass])
                                {
                                    if (!(campaignMode().mabColonyClassInvalid[(int)eLoopColonyClass]))
                                    {
                                        int iProb = Globals.Infos.colonyClass(eLoopColonyClass).miAppearanceProb;
                                        if (iProb > 0)
                                        {
                                            int iValue = mRandom.Next(iProb);

                                            if (seUsedColonyClasses.Contains(eLoopColonyClass))
                                            {
                                                iValue /= 2;
                                            }
                                            else
                                            {
                                                iValue *= 2;
                                            }

                                            iValue++;

                                            if (iValue > iBestValue)
                                            {
                                                eBestColonyClass = eLoopColonyClass;
                                                iBestValue = iValue;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        setLevelColonyClass(eBestLevel, eBestColonyClass);
                        seUsedColonyClasses.Add(eBestColonyClass);
                    }

                    {
                        SevenSolsType eSevenSols = SevenSolsType.COLONY;

                        if (Globals.AppInfo.OwnsDLCCampaign || (Globals.AppInfo.OwnsDLCIo && (getLocation() == LocationType.IO)))
                        {
                            if (campaignMode().mbAlternateSevenSols)
                            {
                                if ((getTurn() % 2) == 1)
                                {
                                    eSevenSols = SevenSolsType.WHOLESALE;
                                }
                            }
                            else
                            {
                                if (mRandom.Next(2) == 0)
                                {
                                    eSevenSols = SevenSolsType.WHOLESALE;
                                }
                            }
                        }

                        setLevelSevenSols(eBestLevel, eSevenSols);
                    }

                    iLevelCount++;
                }
                else
                {
                    break;
                }
            }

            AI_fillLevels();
        }

        public virtual void fillLevelEvents()
        {
            List<bool> abEventLevelChosen = Enumerable.Repeat(false, (int)Globals.Infos.eventLevelsNum()).ToList();

            foreach (InfoLevel pLoopLevel in Globals.Infos.levels())
            {
                if (getLevelState(pLoopLevel.meType) == LevelStateType.OPENED)
                {
                    EventLevelType eBestEventLevel = EventLevelType.NONE;
                    int iBestValue = 0;

                    foreach (InfoEventLevel pLoopEventLevel in Globals.Infos.eventLevels())
                    {
                        if (!abEventLevelChosen[pLoopEventLevel.miType])
                        {
                            if (!(pLoopEventLevel.mabLocationInvalid[(int)(pLoopLevel.meLocation)]))
                            {
                                ColonyClassType eLevelColonyClass = getLevelColonyClass(pLoopLevel.meType);

                                if ((eLevelColonyClass == ColonyClassType.NONE) || !(pLoopEventLevel.mabColonyClassInvalid[(int)eLevelColonyClass]))
                                {
                                    if (!(pLoopEventLevel.mbIoDLC) || Globals.AppInfo.OwnsDLCIo)
                                    {
                                        PerkType ePerk = pLoopEventLevel.mePerk;

                                        if ((ePerk == PerkType.NONE) || (getHuman().getPerkCount(ePerk) == 0))
                                        {
                                            int iValue = mRandom.Next(((getTurn() % 2) == 0) ? pLoopEventLevel.miAppearanceProb1 : pLoopEventLevel.miAppearanceProb2) + 1;
                                            if (iValue > iBestValue)
                                            {
                                                eBestEventLevel = pLoopEventLevel.meType;
                                                iBestValue = iValue;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (eBestEventLevel != EventLevelType.NONE)
                    {
                        maeLevelEvent[pLoopLevel.miType] = eBestEventLevel;
                        abEventLevelChosen[(int)eBestEventLevel] = true;
                    }
                }
            }
        }

        public virtual int AI_getModuleLevelValue(Corporation pCorporation, ModuleType eModule, int iCount, LocationType eLocation)
        {
            int iValue = 0;

            for (ResourceType eLoopResource = 0; eLoopResource < Globals.Infos.resourcesNum(); eLoopResource++)
            {
                int iDemand = GameClient.getModuleSupply(eModule, eLoopResource, Globals.Infos, null, eLocation, true);
                if (iDemand != 0)
                {
                    for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                    {
                        int iPerkCount = pCorporation.getPerkCount(eLoopPerk);
                        if (iPerkCount > 0)
                        {
                            BuildingClassType eBuildingClass = Globals.Infos.perk(eLoopPerk).meBuildingClassLevel;

                            if (eBuildingClass != BuildingClassType.NONE)
                            {
                                if (Utils.isBuildingYield(Globals.Infos.buildingClass(eBuildingClass).meBaseBuilding, eLoopResource, null))
                                {
                                    iValue += ((iDemand * iCount * iPerkCount * 100) / Constants.RESOURCE_MULTIPLIER);
                                }
                            }
                        }
                    }
                }
            }

            return iValue;
        }

        public virtual int AI_getColonyClassLevelValue(Corporation pCorporation, ColonyClassType eColonyClass, ColonyType eColony, SevenSolsType eSevenSols, LocationType eLocation)
        {
            int iValue = 1000;

            if (eColonyClass == ColonyClassType.NONE)
            {
                return iValue;
            }

            iValue += Globals.Infos.colonyClass(eColonyClass).miColonyCapModifier;

            iValue += -(Globals.Infos.colonyClass(eColonyClass).miBaseShareModifier);

            {
                int iModifier = Globals.Infos.colonyClass(eColonyClass).miBlackMarketTimeModifier;
                if (iModifier != 0)
                {
                    iValue += iModifier;

                    for (HQType eLoopHQ = 0; eLoopHQ < Globals.Infos.HQsNum(); eLoopHQ++)
                    {
                        if (canFoundHQ(pCorporation, eLoopHQ))
                        {
                            int iHQModifier = Globals.Infos.HQ(eLoopHQ).miBlackMarketHostileTimeModifier;
                            if (iHQModifier != 0)
                            {
                                if (iModifier > 0)
                                {
                                    iValue += -(iHQModifier);
                                }
                                else
                                {
                                    iValue += iHQModifier;
                                }
                            }
                        }
                    }
                }
            }

            {
                int iModifier = Globals.Infos.colonyClass(eColonyClass).miTechCostModifier;
                if (iModifier != 0)
                {
                    for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                    {
                        int iPerkCount = pCorporation.getPerkCount(eLoopPerk);
                        if (iPerkCount > 0)
                        {
                            BuildingClassType eBuildingClass = Globals.Infos.perk(eLoopPerk).meBuildingClassLevel;

                            if (eBuildingClass != BuildingClassType.NONE)
                            {
                                if (Globals.Infos.buildingClass(eBuildingClass).meOrderType == OrderType.RESEARCH)
                                {
                                    iValue += -(iModifier * iPerkCount);
                                }
                            }
                        }
                    }
                }
            }

            {
                int iModifier = Globals.Infos.colonyClass(eColonyClass).miPatentCostModifier;
                if (iModifier != 0)
                {
                    for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                    {
                        int iPerkCount = pCorporation.getPerkCount(eLoopPerk);
                        if (iPerkCount > 0)
                        {
                            BuildingClassType eBuildingClass = Globals.Infos.perk(eLoopPerk).meBuildingClassLevel;

                            if (eBuildingClass != BuildingClassType.NONE)
                            {
                                if (Globals.Infos.buildingClass(eBuildingClass).meOrderType == OrderType.PATENT)
                                {
                                    iValue += -(iModifier * iPerkCount);
                                }
                            }
                        }
                    }
                }
            }

            {
                int iModifier = Globals.Infos.colonyClass(eColonyClass).miModuleBonusModifier;
                if (iModifier != 0)
                {
                    for (ModuleType eLoopModule = 0; eLoopModule < Globals.Infos.modulesNum(); eLoopModule++)
                    {
                        if (GameClient.canHaveModule(eLoopModule, eLocation, eColony, eColonyClass, Globals.Infos))
                        {
                            for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                            {
                                int iPerkCount = pCorporation.getPerkCount(eLoopPerk);
                                if (iPerkCount > 0)
                                {
                                    BuildingClassType eBuildingClass = Globals.Infos.perk(eLoopPerk).meBuildingClassLevel;

                                    if (eBuildingClass != BuildingClassType.NONE)
                                    {
                                        if (Globals.Infos.building(Globals.Infos.buildingClass(eBuildingClass).meBaseBuilding).miEntertainment > 0)
                                        {
                                            int iBeforeValue = Globals.Infos.module(eLoopModule).miEntertainmentModifier;
                                            int iAfterValue = iBeforeValue;

                                            iAfterValue *= Math.Max(0, (iModifier + 100));
                                            iAfterValue /= 100;

                                            iValue += (iAfterValue - iBeforeValue) * 5;
                                        }

                                        {
                                            OrderType eOrder = Globals.Infos.buildingClass(eBuildingClass).meOrderType;

                                            if (eOrder != OrderType.NONE)
                                            {
                                                int iBeforeValue = Globals.Infos.module(eLoopModule).maiOrderModifier[(int)eOrder];
                                                int iAfterValue = iBeforeValue;

                                                iAfterValue *= Math.Max(0, (iModifier + 100));
                                                iAfterValue /= 100;

                                                iValue += (iAfterValue - iBeforeValue) * 5;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            for (ResourceType eLoopResource = 0; eLoopResource < Globals.Infos.resourcesNum(); eLoopResource++)
            {
                int iMinPrice = Globals.Infos.colonyClass(eColonyClass).maiResourceMinPrice[(int)eLoopResource];
                if (iMinPrice > 0)
                {
                    for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                    {
                        int iPerkCount = pCorporation.getPerkCount(eLoopPerk);
                        if (iPerkCount > 0)
                        {
                            BuildingClassType eBuildingClass = Globals.Infos.perk(eLoopPerk).meBuildingClassLevel;

                            if (eBuildingClass != BuildingClassType.NONE)
                            {
                                if (Utils.isBuildingYield(Globals.Infos.buildingClass(eBuildingClass).meBaseBuilding, eLoopResource, null))
                                {
                                    iValue += (iMinPrice * iPerkCount);
                                }
                            }
                        }
                    }
                }
            }

            for (ResourceType eLoopResource = 0; eLoopResource < Globals.Infos.resourcesNum(); eLoopResource++)
            {
                int iMaxPrice = Globals.Infos.colonyClass(eColonyClass).maiResourceMaxPrice[(int)eLoopResource];
                if (iMaxPrice > 0)
                {
                    for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                    {
                        int iPerkCount = pCorporation.getPerkCount(eLoopPerk);
                        if (iPerkCount > 0)
                        {
                            BuildingClassType eBuildingClass = Globals.Infos.perk(eLoopPerk).meBuildingClassLevel;

                            if (eBuildingClass != BuildingClassType.NONE)
                            {
                                if (Utils.isBuildingYield(Globals.Infos.buildingClass(eBuildingClass).meBaseBuilding, eLoopResource, null))
                                {
                                    int iSubValue = -(100 * iPerkCount);

                                    iSubValue /= (iMaxPrice);

                                    iValue += iSubValue;
                                }
                            }
                        }
                    }
                }
            }

            for (ResourceType eLoopResource = 0; eLoopResource < Globals.Infos.resourcesNum(); eLoopResource++)
            {
                int iDemand = Globals.Infos.colonyClass(eColonyClass).maiResourceLaborSupply[(int)eLoopResource];
                if (iDemand > 0)
                {
                    for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                    {
                        int iPerkCount = pCorporation.getPerkCount(eLoopPerk);
                        if (iPerkCount > 0)
                        {
                            BuildingClassType eBuildingClass = Globals.Infos.perk(eLoopPerk).meBuildingClassLevel;

                            if (eBuildingClass != BuildingClassType.NONE)
                            {
                                if (Utils.isBuildingYield(Globals.Infos.buildingClass(eBuildingClass).meBaseBuilding, eLoopResource, null))
                                {
                                    iValue += ((iDemand * iPerkCount) / 2);
                                }
                            }
                        }
                    }
                }
            }

            for (BuildingClassType eLoopBuildingClass = 0; eLoopBuildingClass < Globals.Infos.buildingClassesNum(); eLoopBuildingClass++)
            {
                int iModifier = Globals.Infos.colonyClass(eColonyClass).maiBuildingClassCostModifier[(int)eLoopBuildingClass];
                if (iModifier != 0)
                {
                    for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                    {
                        int iPerkCount = pCorporation.getPerkCount(eLoopPerk);
                        if (iPerkCount > 0)
                        {
                            if (Globals.Infos.perk(eLoopPerk).meBuildingClassLevel == eLoopBuildingClass)
                            {
                                iValue += -(iModifier * iPerkCount);
                            }
                        }
                    }
                }
            }

            for (ModuleType eLoopModule = 0; eLoopModule < Globals.Infos.modulesNum(); eLoopModule++)
            {
                int iModuleCount = Globals.Infos.colonyClass(eColonyClass).maiStartingModules[(int)eLoopModule];
                if (iModuleCount > 0)
                {
                    iValue += AI_getModuleLevelValue(pCorporation, eLoopModule, iModuleCount, eLocation);
                }
            }

            if (eSevenSols == SevenSolsType.WHOLESALE)
            {
                for (ResourceType eLoopResource = 0; eLoopResource < Globals.Infos.resourcesNum(); eLoopResource++)
                {
                    if (Globals.Infos.colonyClass(eColonyClass).mabResourceNoWholesale[(int)eLoopResource])
                    {
                        for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                        {
                            int iPerkCount = pCorporation.getPerkCount(eLoopPerk);
                            if (iPerkCount > 0)
                            {
                                BuildingClassType eBuildingClass = Globals.Infos.perk(eLoopPerk).meBuildingClassLevel;

                                if (eBuildingClass != BuildingClassType.NONE)
                                {
                                    if (Utils.isBuildingYield(Globals.Infos.buildingClass(eBuildingClass).meBaseBuilding, eLoopResource, null))
                                    {
                                        iValue += -(20 * iPerkCount);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            for (BuildingClassType eLoopBuildingClass = 0; eLoopBuildingClass < Globals.Infos.buildingClassesNum(); eLoopBuildingClass++)
            {
                if (Globals.Infos.colonyClass(eColonyClass).mabBuildingClassInvalid[(int)eLoopBuildingClass])
                {
                    for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                    {
                        int iPerkCount = pCorporation.getPerkCount(eLoopPerk);
                        if (iPerkCount > 0)
                        {
                            if (Globals.Infos.perk(eLoopPerk).meBuildingClassLevel == eLoopBuildingClass)
                            {
                                iValue += -(100 * iPerkCount);
                            }

                            {
                                BuildingType eFreeBuilding = Globals.Infos.perk(eLoopPerk).meFreeBuilding;

                                if (eFreeBuilding != BuildingType.NONE)
                                {
                                    if (Globals.Infos.building(eFreeBuilding).meClass == eLoopBuildingClass)
                                    {
                                        iValue += -(200 * iPerkCount);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            for (PatentType eLoopPatent = 0; eLoopPatent < Globals.Infos.patentsNum(); eLoopPatent++)
            {
                if (Globals.Infos.colonyClass(eColonyClass).mabPatentInvalid[(int)eLoopPatent])
                {
                    for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                    {
                        if (pCorporation.getPerkCount(eLoopPerk) > 0)
                        {
                            if (Globals.Infos.perk(eLoopPerk).mePatent == eLoopPatent)
                            {
                                iValue -= 200;
                            }
                        }
                    }
                }
            }

            for (ModuleType eLoopModule = 0; eLoopModule < Globals.Infos.modulesNum(); eLoopModule++)
            {
                if (Globals.Infos.colonyClass(eColonyClass).mabModuleInvalid[(int)eLoopModule])
                {
                    iValue -= AI_getModuleLevelValue(pCorporation, eLoopModule, 4, eLocation);
                }
                else if (Globals.Infos.colonyClass(eColonyClass).mabModuleValid[(int)eLoopModule])
                {
                    iValue += AI_getModuleLevelValue(pCorporation, eLoopModule, 2, eLocation);
                }
            }

            return Math.Max(0, iValue);
        }

        public virtual int AI_getEventLevelValue(Corporation pCorporation, EventLevelType eEventLevel, LocationType eLocation)
        {
            int iValue = 1000;

            if (eEventLevel == EventLevelType.NONE)
            {
                return iValue;
            }

            {
                EventStateType eEventState = Globals.Infos.eventLevel(eEventLevel).meEventState;

                if (eEventState != EventStateType.NONE)
                {
                    {
                        int iModifier = Math.Abs(Globals.Infos.eventState(eEventState).miXWindSpeedModifier);
                        if (iModifier > 0)
                        {
                            for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                            {
                                int iPerkCount = pCorporation.getPerkCount(eLoopPerk);
                                if (iPerkCount > 0)
                                {
                                    BuildingClassType eBuildingClass = Globals.Infos.perk(eLoopPerk).meBuildingClassLevel;

                                    if (eBuildingClass != BuildingClassType.NONE)
                                    {
                                        if (Utils.isBuildingMiningAny(Globals.Infos.buildingClass(eBuildingClass).meBaseBuilding))
                                        {
                                            iValue -= ((iModifier * iPerkCount) / 5);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    {
                        int iModifier = Globals.Infos.eventState(eEventState).miUpgradeModifier;
                        if (iModifier != 0)
                        {
                            int iMaxUpgradeCost = 0;

                            for (HQType eLoopHQ = 0; eLoopHQ < Globals.Infos.HQsNum(); eLoopHQ++)
                            {
                                if (canFoundHQ(pCorporation, eLoopHQ))
                                {
                                    int iUpgradeCost = 0;

                                    for (ResourceType eLoopResource = 0; eLoopResource < Globals.Infos.resourcesNum(); eLoopResource++)
                                    {
                                        int iUpgrade = Globals.Infos.HQ(eLoopHQ).maiUpgradeResource[(int)eLoopResource];
                                        if (iUpgrade > 0 && !Globals.Infos.resource(eLoopResource).mabLocationInvalid[(int)eLocation])
                                        {
                                            iUpgradeCost += (getResourcePrice(eLoopResource) * iUpgrade);
                                        }
                                    }

                                    iMaxUpgradeCost = Math.Max(iMaxUpgradeCost, iUpgradeCost);
                                }
                            }

                            int iExtraCost = ((iMaxUpgradeCost * Math.Max(0, 100 + iModifier)) / 100);
                            iExtraCost -= iMaxUpgradeCost;

                            iValue -= (iExtraCost / 100);
                        }
                    }

                    {
                        int iModifier = Globals.Infos.eventState(eEventState).miLifeSupportModifier;
                        if (iModifier != 0)
                        {
                            int iMaxLifeSupportCost = 0;

                            for (HQType eLoopHQ = 0; eLoopHQ < Globals.Infos.HQsNum(); eLoopHQ++)
                            {
                                if (canFoundHQ(pCorporation, eLoopHQ))
                                {
                                    int iLifeSupportCost = 0;

                                    for (ResourceType eLoopResource = 0; eLoopResource < Globals.Infos.resourcesNum(); eLoopResource++)
                                    {
                                        int iLifeSupport = Globals.Infos.HQ(eLoopHQ).maiLifeSupport[(int)eLoopResource];
                                        if (iLifeSupport > 0)
                                        {
                                            iLifeSupportCost += ((getResourcePrice(eLoopResource) * iLifeSupport) / Constants.RESOURCE_MULTIPLIER);
                                        }
                                    }

                                    iLifeSupportCost *= 200;

                                    iMaxLifeSupportCost = Math.Max(iMaxLifeSupportCost, iLifeSupportCost);
                                }
                            }

                            int iExtraCost = ((iMaxLifeSupportCost * Math.Max(0, 100 + iModifier)) / 100);
                            iExtraCost -= iMaxLifeSupportCost;

                            iValue -= (iExtraCost / 100);
                        }
                    }

                    {
                        int iModifier = Globals.Infos.eventState(eEventState).miBlackMarketTimeModifier;
                        if (iModifier != 0)
                        {
                            iValue += iModifier;

                            for (HQType eLoopHQ = 0; eLoopHQ < Globals.Infos.HQsNum(); eLoopHQ++)
                            {
                                if (canFoundHQ(pCorporation, eLoopHQ))
                                {
                                    int iHQModifier = Globals.Infos.HQ(eLoopHQ).miBlackMarketHostileTimeModifier;
                                    if (iHQModifier != 0)
                                    {
                                        if (iModifier > 0)
                                        {
                                            iValue += -(iHQModifier);
                                        }
                                        else
                                        {
                                            iValue += iHQModifier;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (Globals.Infos.eventState(eEventState).mbNoAdjaceny)
                    {
                        for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                        {
                            int iPerkCount = pCorporation.getPerkCount(eLoopPerk);
                            if (iPerkCount == 0)
                            {
                                BuildingClassType eBuildingClass = Globals.Infos.perk(eLoopPerk).meBuildingClassLevel;

                                if (eBuildingClass != BuildingClassType.NONE)
                                {
                                    if (Globals.Infos.buildingClass(eBuildingClass).meOrderType == OrderType.NONE)
                                    {
                                        iValue -= 50;
                                    }
                                }
                            }
                        }

                        for (HQType eLoopHQ = 0; eLoopHQ < Globals.Infos.HQsNum(); eLoopHQ++)
                        {
                            if (canFoundHQ(pCorporation, eLoopHQ))
                            {
                                if (Globals.Infos.HQ(eLoopHQ).mbAdjacentInputBonus)
                                {
                                    iValue -= 200;
                                    break;
                                }
                            }
                        }
                    }

                    for (BuildingClassType eLoopBuildingClass = 0; eLoopBuildingClass < Globals.Infos.buildingClassesNum(); eLoopBuildingClass++)
                    {
                        int iModifier = Globals.Infos.eventState(eEventState).miProductionModifier + Globals.Infos.eventState(eEventState).maiBuildingClassModifier[(int)eLoopBuildingClass];
                        if (iModifier != 0)
                        {
                            for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                            {
                                int iPerkCount = pCorporation.getPerkCount(eLoopPerk);
                                if (iPerkCount > 0)
                                {
                                    BuildingClassType eBuildingClass = Globals.Infos.perk(eLoopPerk).meBuildingClassLevel;

                                    if (eBuildingClass == eLoopBuildingClass)
                                    {
                                        iValue += (iModifier * iPerkCount);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            for (ModuleType eLoopModule = 0; eLoopModule < Globals.Infos.modulesNum(); eLoopModule++)
            {
                int iModuleCount = Globals.Infos.eventLevel(eEventLevel).maiStartingModules[(int)eLoopModule];
                if (iModuleCount > 0)
                {
                    iValue += AI_getModuleLevelValue(pCorporation, eLoopModule, iModuleCount, eLocation);
                }
            }

            {
                PerkType ePerk = Globals.Infos.eventLevel(eEventLevel).mePerk;

                if (ePerk != PerkType.NONE)
                {
                    iValue += AI_getPerkValue(pCorporation, ePerk);
                }
            }

            return Math.Max(0, iValue);
        }

        public virtual void AI_fillLevels()
        {
            for (int iRank = 0; iRank < campaignMode().miStartingCorps; iRank++)
            {
                foreach (Corporation pLoopCorporation in getCorporations())
                {
                    if (pLoopCorporation.meID != CorporationType.HUMAN)
                    {
                        if (!(pLoopCorporation.mbDead))
                        {
                            if (pLoopCorporation.miRank == iRank)
                            {
                                if (pLoopCorporation.meCurrentLevel == LevelType.NONE)
                                {
                                    LevelType eBestLevel = LevelType.NONE;
                                    int iBestValue = 0;

                                    for (int iPass = 0; iPass < MAX_MISSION_AI; iPass++)
                                    {
                                        for (LevelType eLoopLevel = 0; eLoopLevel < Globals.Infos.levelsNum(); eLoopLevel++)
                                        {
                                            if ((countCorporations(eLoopLevel) < Math.Min(MAX_MISSION_AI, (iPass + 2))) || (getCampaignState() == CampaignState.FINAL_ROUND))
                                            {
                                                if (canStartMission(eLoopLevel, pLoopCorporation.meID))
                                                {
                                                    int iValue = mRandomAI.Next(400);

                                                    iValue += Globals.Infos.personality(pLoopCorporation.mePersonality).maiTerrainClassValue[(int)(Globals.Infos.level(eLoopLevel).meTerrainClass)];
                                                    iValue += Globals.Infos.personality(pLoopCorporation.mePersonality).maiLatitudeValue[(int)(Globals.Infos.level(eLoopLevel).meLatitude)];
                                                    iValue += Globals.Infos.personality(pLoopCorporation.mePersonality).maiLocationValue[(int)(Globals.Infos.level(eLoopLevel).meLocation)];

                                                    iValue += (int)(pLoopCorporation.maeColonyBonusLevel[(int)getLevelColony(eLoopLevel)] + 1) * 400;

                                                    iValue += AI_getEventLevelValue(pLoopCorporation, getLevelEvent(eLoopLevel), Globals.Infos.level(eLoopLevel).meLocation);

                                                    iValue += AI_getColonyClassLevelValue(pLoopCorporation, getLevelColonyClass(eLoopLevel), getLevelColony(eLoopLevel), getLevelSevenSols(eLoopLevel), Globals.Infos.level(eLoopLevel).meLocation);

                                                    {
                                                        PerkType eLevelPerk = getLevelPerk(eLoopLevel);

                                                        if (eLevelPerk != PerkType.NONE)
                                                        {
                                                            int iPerkValue = AI_getPerkValue(pLoopCorporation, eLevelPerk);
                                                            int iLevelTime = Globals.Infos.perk(eLevelPerk).miLevelTime;

                                                            if (iLevelTime == 0)
                                                            {
                                                                iPerkValue *= getTurnsLeft();
                                                            }
                                                            else
                                                            {
                                                                iPerkValue *= iLevelTime;
                                                            }

                                                            iValue += iPerkValue;
                                                        }
                                                    }

                                                    for (LevelType eAltLevel = 0; eAltLevel < Globals.Infos.levelsNum(); eAltLevel++)
                                                    {
                                                        if (Globals.Infos.level(eAltLevel).mabAdjacentLevel[(int)eLoopLevel])
                                                        {
                                                            if (getLevelWinner(eAltLevel) == pLoopCorporation.meID)
                                                            {
                                                                iValue += (campaignMode().miAdjacencyIncome / 100);
                                                            }
                                                        }
                                                    }

                                                    if (iValue >= iBestValue)
                                                    {
                                                        eBestLevel = eLoopLevel;
                                                        iBestValue = iValue;
                                                    }
                                                }
                                            }
                                        }

                                        if (eBestLevel != LevelType.NONE)
                                        {
                                            break;
                                        }
                                    }

                                    if (eBestLevel != LevelType.NONE)
                                    {
                                        pLoopCorporation.meCurrentLevel = eBestLevel;
                                    }
                                    else
                                    {
                                        if (getCampaignState() != CampaignState.FINAL_ROUND)
                                        {
                                            Debug.Log("WARNING: LEVEL NOT FOUND: " + pLoopCorporation.mzName);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public virtual void sortCorporations()
        {
            foreach (Corporation pLoopCorporation in getCorporations())
            {
                pLoopCorporation.miRank = -1;
            }

            int iRank = 0;

            for (int iPass = 0; iPass < 2; iPass++)
            {
                while (true)
                {
                    Corporation pBestCorporation = null;
                    int iBestValue = 0;

                    foreach (Corporation pLoopCorporation in getCorporations())
                    {
                        if ((iPass == 0) == !(pLoopCorporation.mbDead))
                        {
                            if (pLoopCorporation.miRank == -1)
                            {
                                int iValue = calculateSharePrice(pLoopCorporation);
                                if (iValue > iBestValue)
                                {
                                    pBestCorporation = pLoopCorporation;
                                    iBestValue = iValue;
                                }
                            }
                        }
                    }

                    if (pBestCorporation != null)
                    {
                        pBestCorporation.miRank = iRank;
                        iRank++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        public virtual int AI_getEventTurnOptionValue(Corporation pCorporation, EventTurnOptionType eEventTurnOption)
        {
            int iValue = 0;

            iValue += (Globals.Infos.eventTurnOption(eEventTurnOption).miMoney / 2000);
            iValue += -(Globals.Infos.eventTurnOption(eEventTurnOption).miDebt / 4000);

            {
                int iPerkMoney = Globals.Infos.eventTurnOption(eEventTurnOption).miMoneyPerk;
                adjustPerkCost(ref iPerkMoney, true);
                iValue += (iPerkMoney / 2000);
            }

            for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
            {
                int iChange = Globals.Infos.eventTurnOption(eEventTurnOption).maiPerkChange[(int)eLoopPerk];
                if (iChange > 0)
                {
                    iValue += (AI_getPerkValue(pCorporation, eLoopPerk) * getTurnsLeft() * iChange);
                }

                int iTime = Globals.Infos.eventTurnOption(eEventTurnOption).maiPerkTime[(int)eLoopPerk];
                if (iTime > 0)
                {
                    iValue += (AI_getPerkValue(pCorporation, eLoopPerk) * iTime);
                }
            }

            return Math.Max(1, iValue);
        }

        public virtual void AI_doEventTurn(Corporation pCorporation, EventTurnType eEventTurn)
        {
            EventTurnOptionType eBestEventTurnOption = EventTurnOptionType.NONE;
            int iBestValue = 0;

            EventTurnOptionType eOption1 = Globals.Infos.eventTurn(eEventTurn).meOption1;
            if (eOption1 != EventTurnOptionType.NONE)
            {
                int iValue = AI_getEventTurnOptionValue(pCorporation, eOption1);

                iValue += mRandomAI.Next(100);

                if (iValue > iBestValue)
                {
                    eBestEventTurnOption = eOption1;
                    iBestValue = iValue;
                }
            }

            EventTurnOptionType eOption2 = Globals.Infos.eventTurn(eEventTurn).meOption2;
            if (eOption2 != EventTurnOptionType.NONE)
            {
                int iValue = AI_getEventTurnOptionValue(pCorporation, eOption2);

                iValue += mRandomAI.Next(100);

                if (iValue > iBestValue)
                {
                    eBestEventTurnOption = eOption2;
                    iBestValue = iValue;
                }
            }

            EventTurnOptionType eOption3 = Globals.Infos.eventTurn(eEventTurn).meOption3;
            if (eOption3 != EventTurnOptionType.NONE)
            {
                int iValue = AI_getEventTurnOptionValue(pCorporation, eOption3);

                iValue += mRandomAI.Next(100);

                if (iValue > iBestValue)
                {
                    eBestEventTurnOption = eOption3;
                    iBestValue = iValue;
                }
            }

            if (eBestEventTurnOption != EventTurnOptionType.NONE)
            {
                doEventTurnOption(eBestEventTurnOption, pCorporation);
            }
        }

        void doEvents()
        {
            foreach (Corporation pLoopCorporation in getCorporations())
            {
                if (!(pLoopCorporation.mbDead))
                {
                    EventTurnType eEventTurn = findEventTurn(pLoopCorporation);

                    if (eEventTurn != EventTurnType.NONE)
                    {
                        if (pLoopCorporation.meID == CorporationType.HUMAN)
                        {
                            setEventTurn(eEventTurn);
                        }
                        else
                        {
                            AI_doEventTurn(pLoopCorporation, eEventTurn);
                        }

                        pLoopCorporation.mabEventTurnSeen[(int)eEventTurn] = true;
                    }
                }
            }
        }

        public virtual bool canFoundHQ(Corporation pCorporation, HQType eHQ)
        {
            for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
            {
                if (pCorporation.getPerkCount(eLoopPerk) > 0)
                {
                    if (Globals.Infos.perk(eLoopPerk).mbUnlockHQAll)
                    {
                        return true;
                    }

                    if (Globals.Infos.perk(eLoopPerk).meHQUnlock == eHQ)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public virtual bool canFillPerk(PerkType ePerk, Corporation pCorporation)
        {
            if (getTurn() < Globals.Infos.perk(ePerk).miAvailableTurn)
            {
                return false;
            }

            if (Globals.Infos.perk(ePerk).mbAvailableUnique)
            {
                if (pCorporation.getPerkCount(ePerk) > 0)
                {
                    return false;
                }
            }

            if (Globals.Infos.perk(ePerk).mbIoDLC && !(Globals.AppInfo.OwnsDLCIo))
            {
                return false;
            }

            if (Globals.Infos.perk(ePerk).mabLocationInvalid[(int)getLocation()])
            {
                return false;
            }

            for (LevelType eLoopLevel = 0; eLoopLevel < Globals.Infos.levelsNum(); eLoopLevel++)
            {
                if (getLevelState(eLoopLevel) == LevelStateType.OPENED)
                {
                    ColonyClassType eColonyClass = getLevelColonyClass(eLoopLevel);

                    if (eColonyClass != ColonyClassType.NONE)
                    {
                        for (BuildingClassType eLoopBuildingClass = 0; eLoopBuildingClass < Globals.Infos.buildingClassesNum(); eLoopBuildingClass++)
                        {
                            if (Globals.Infos.colonyClass(eColonyClass).mabBuildingClassInvalid[(int)eLoopBuildingClass])
                            {
                                if (Globals.Infos.perk(ePerk).meBuildingClassLevel == eLoopBuildingClass)
                                {
                                    return false;
                                }

                                if (Globals.Infos.perk(ePerk).meFreeBuilding != BuildingType.NONE)
                                {
                                    if (Globals.Infos.building(Globals.Infos.perk(ePerk).meFreeBuilding).meClass == eLoopBuildingClass)
                                    {
                                        return false;
                                    }
                                }

                                if (Globals.Infos.perk(ePerk).meBuildingImmune != BuildingType.NONE)
                                {
                                    if (Globals.Infos.building(Globals.Infos.perk(ePerk).meBuildingImmune).meClass == eLoopBuildingClass)
                                    {
                                        return false;
                                    }
                                }

                                if (Globals.Infos.perk(ePerk).maiBuildingClassInputModifier[(int)eLoopBuildingClass] < 0)
                                {
                                    return false;
                                }

                                if (Globals.Infos.perk(ePerk).maeMinimumMining[(int)eLoopBuildingClass] > ResourceLevelType.NONE)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }

        public virtual void fillPerksArray(List<int> aiPerksAvailable)
        {
            int iCountBuilding = 0;
            int iCountOther = 0;

            for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
            {
                if (Globals.Infos.perk(eLoopPerk).mbBuilding)
                {
                    if (canFillPerk(eLoopPerk, getHuman()))
                    {
                        for (int i = 0; i < campaignMode().maiPerkAvailable[(int)eLoopPerk]; i++)
                        {
                            if (iCountBuilding < Globals.Infos.Globals.CAMPAIGN_PERKS_AVAILABLE_BUILDINGS)
                            {
                                aiPerksAvailable[(int)eLoopPerk]++;
                                iCountBuilding++;
                            }
                        }
                    }
                }
            }

            {
                List<PerkType> aePerkRolls = new List<PerkType>();

                for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                {
                    if (Globals.Infos.perk(eLoopPerk).mbBuilding)
                    {
                        if (canFillPerk(eLoopPerk, getHuman()))
                        {
                            for (int i = 0; i < Globals.Infos.perk(eLoopPerk).miAvailableRoll; i++)
                            {
                                aePerkRolls.Add(eLoopPerk);
                            }
                        }
                    }
                }

                while (iCountBuilding < Globals.Infos.Globals.CAMPAIGN_PERKS_AVAILABLE_BUILDINGS)
                {
                    PerkType ePerk = aePerkRolls[mRandom.Next(aePerkRolls.Count)];

                    if ((aiPerksAvailable[(int)ePerk] == 0) ||
                        (Globals.Infos.perk(ePerk).mbMultiples))
                    {
                        if (aiPerksAvailable[(int)ePerk] < 4)
                        {
                            aiPerksAvailable[(int)ePerk]++;
                            iCountBuilding++;
                        }
                    }
                }
            }

            for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
            {
                if (!(Globals.Infos.perk(eLoopPerk).mbBuilding))
                {
                    if (canFillPerk(eLoopPerk, getHuman()))
                    {
                        for (int i = 0; i < campaignMode().maiPerkAvailable[(int)eLoopPerk]; i++)
                        {
                            if (iCountOther < Globals.Infos.Globals.CAMPAIGN_PERKS_AVAILABLE_OTHERS)
                            {
                                aiPerksAvailable[(int)eLoopPerk]++;
                                iCountOther++;
                            }
                        }
                    }
                }
            }

            {
                List<PerkType> aePerkRolls = new List<PerkType>();

                for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                {
                    if (!(Globals.Infos.perk(eLoopPerk).mbBuilding))
                    {
                        if (canFillPerk(eLoopPerk, getHuman()))
                        {
                            for (int i = 0; i < Globals.Infos.perk(eLoopPerk).miAvailableRoll; i++)
                            {
                                aePerkRolls.Add(eLoopPerk);
                            }
                        }
                    }
                }

                while (iCountOther < Globals.Infos.Globals.CAMPAIGN_PERKS_AVAILABLE_OTHERS)
                {
                    PerkType ePerk = aePerkRolls[mRandom.Next(aePerkRolls.Count)];

                    if ((aiPerksAvailable[(int)ePerk] == 0) ||
                        (Globals.Infos.perk(ePerk).mbMultiples))
                    {
                        if (aiPerksAvailable[(int)ePerk] < 3)
                        {
                            aiPerksAvailable[(int)ePerk]++;
                            iCountOther++;
                        }
                    }
                }
            }
        }

        public virtual void fillPerks()
        {
            for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
            {
                maiPerksAvailable[(int)eLoopPerk] = 0;
                maiPermPerksBought[(int)eLoopPerk] = 0;
                maiTempPerksBought[(int)eLoopPerk] = 0;
            }

            fillPerksArray(maiPerksAvailable);
        }

        public virtual int AI_getBuildingResourceValue(Corporation pCorporation, BuildingType eBuilding)
        {
            int iValue = 0;

            for (ResourceType eLoopResource = 0; eLoopResource < Globals.Infos.resourcesNum(); eLoopResource++)
            {
                iValue += ((Globals.Infos.building(eBuilding).maiResourceMining[(int)eLoopResource] * getResourcePrice(eLoopResource) * 2) / Constants.RESOURCE_MULTIPLIER);

                iValue += ((Utils.getBuildingOutput(eBuilding, eLoopResource, null) * getResourcePrice(eLoopResource)) / Constants.RESOURCE_MULTIPLIER);

                iValue -= ((Globals.Infos.building(eBuilding).maiResourceInput[(int)eLoopResource] * getResourcePrice(eLoopResource)) / Constants.RESOURCE_MULTIPLIER);
            }

            {
                int iCount = 0;

                for (LocationType eLocation = 0; eLocation < LocationType.NUM_TYPES; eLocation++)
                {
                    if (!(Globals.Infos.building(eBuilding).mabLocationInvalid[(int)eLocation]))
                    {
                        iCount++;
                    }
                }

                iValue *= (iCount + 1);
                iValue /= (int)(LocationType.NUM_TYPES + 1);
            }

            return iValue;
        }

        public virtual int AI_getBuildingOrderValue(Corporation pCorporation, BuildingType eBuilding)
        {
            int iValue = 0;

            BuildingClassType eBuildingClass = Globals.Infos.building(eBuilding).meClass;

            OrderType eOrder = Globals.Infos.buildingClass(eBuildingClass).meOrderType;

            if (eOrder != OrderType.NONE)
            {
                switch (eOrder)
                {
                    case OrderType.PATENT:
                        iValue += 100;
                        break;

                    case OrderType.RESEARCH:
                        iValue += 150;
                        break;

                    case OrderType.HACK:
                        iValue += 150;
                        break;

                    case OrderType.LAUNCH:
                        iValue += 200;
                        break;
                }

                for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                {
                    int iPerkCount = pCorporation.getPerkCount(eLoopPerk);
                    if (iPerkCount > 0)
                    {
                        iValue += -(Globals.Infos.perk(eLoopPerk).maiOrderCostModifier[(int)eOrder] * iPerkCount);
                        iValue += (Globals.Infos.perk(eLoopPerk).maiOrderRateModifier[(int)eOrder] * iPerkCount / 2);
                    }
                }
            }

            {
                int iCount = 0;

                for (LocationType eLocation = 0; eLocation < LocationType.NUM_TYPES; eLocation++)
                {
                    if (!(Globals.Infos.building(eBuilding).mabLocationInvalid[(int)eLocation]))
                    {
                        iCount++;
                    }
                }

                iValue *= (iCount + 1);
                iValue /= (int)(LocationType.NUM_TYPES + 1);
            }

            return iValue;
        }

        public virtual bool AI_buyPerkPermanent(Corporation pCorporation, PerkType ePerk)
        {
            if (Globals.Infos.perk(ePerk).miClaims > 0)
            {
                return true;
            }

            if (Globals.Infos.perk(ePerk).miSabotages > 0)
            {
                return true;
            }

            if (Globals.Infos.perk(ePerk).miInterestModifier < 0)
            {
                return true;
            }

            if (Globals.Infos.perk(ePerk).meBuildingClassLevel != BuildingClassType.NONE)
            {
                return true;
            }

            if (Globals.Infos.perk(ePerk).mePatent != PatentType.NONE)
            {
                return true;
            }

            for (OrderType eLoopOrder = 0; eLoopOrder < Globals.Infos.ordersNum(); eLoopOrder++)
            {
                if (Globals.Infos.perk(ePerk).maiOrderCostModifier[(int)eLoopOrder] < 0)
                {
                    return true;
                }

                if (Globals.Infos.perk(ePerk).maiOrderRateModifier[(int)eLoopOrder] > 0)
                {
                    return true;
                }
            }

            if (Globals.Infos.perk(ePerk).meFreeBuilding != BuildingType.NONE)
            {
                return true;
            }

            {
                BuildingType eBuildingImmune = Globals.Infos.perk(ePerk).meBuildingImmune;

                if (eBuildingImmune != BuildingType.NONE)
                {
                    if (AI_getBuildingClassLevel(pCorporation, ePerk, Globals.Infos.building(eBuildingImmune).meClass) > 0)
                    {
                        return true;
                    }
                }
            }

            for (SabotageType eLoopSabotage = 0; eLoopSabotage < Globals.Infos.sabotagesNum(); eLoopSabotage++)
            {
                if (Globals.Infos.perk(ePerk).maiSabotageCount[(int)eLoopSabotage] > 0)
                {
                    if (Globals.Infos.sabotage(eLoopSabotage).miHarvestQuantity > 0)
                    {
                        return true;
                    }

                    if (Globals.Infos.sabotage(eLoopSabotage).miPlunderQuantity > 0)
                    {
                        return true;
                    }

                    if (Globals.Infos.sabotage(eLoopSabotage).miTakeoverTime > 0)
                    {
                        return true;
                    }

                    if (Globals.Infos.sabotage(eLoopSabotage).mbDoubleBuilding)
                    {
                        return true;
                    }

                    if (Globals.Infos.sabotage(eLoopSabotage).mbDefendSabotage)
                    {
                        return true;
                    }
                }
            }

            for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
            {
                if (Globals.Infos.perk(ePerk).maiPerkCostModifier[(int)eLoopPerk] < 0)
                {
                    return true;
                }
            }

            for (BuildingClassType eLoopBuildingClass = 0; eLoopBuildingClass < Globals.Infos.buildingClassesNum(); eLoopBuildingClass++)
            {
                if (Globals.Infos.perk(ePerk).maiBuildingClassInputModifier[(int)eLoopBuildingClass] < 0)
                {
                    if (AI_getBuildingClassLevel(pCorporation, ePerk, eLoopBuildingClass) > 0)
                    {
                        return true;
                    }
                }
            }

            for (ResourceType eLoopResource = 0; eLoopResource < Globals.Infos.resourcesNum(); eLoopResource++)
            {
                if (Globals.Infos.perk(ePerk).maiResourceStockpile[(int)eLoopResource] > 0)
                {
                    return true;
                }

                if (Globals.Infos.perk(ePerk).maiResourceProductionModifier[(int)eLoopResource] > 0)
                {
                    return true;
                }
            }

            for (BuildingClassType eLoopBuildingClass = 0; eLoopBuildingClass < Globals.Infos.buildingClassesNum(); eLoopBuildingClass++)
            {
                if (Globals.Infos.perk(ePerk).maeMinimumMining[(int)eLoopBuildingClass] > ResourceLevelType.NONE)
                {
                    if (AI_getBuildingClassLevel(pCorporation, ePerk, eLoopBuildingClass) > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public virtual int AI_getBuildingClassLevel(Corporation pCorporation, PerkType ePerk, BuildingClassType eBuildingClass)
        {
            if (Globals.Infos.buildingClass(eBuildingClass).mabLocationNoPerks[(int)getLocation()])
            {
                int iCount = 1;
                int iBonus = Globals.Infos.Globals.CAMPAIGN_PERK_BUILDING_BASE;

                while (iCount < 10)
                {
                    iCount++;
                    iBonus += Globals.Infos.Globals.CAMPAIGN_PERK_BUILDING_BONUS;

                    if (iBonus >= 100)
                    {
                        break;
                    }
                }

                return iCount;
            }
            else
            {
                int iCount = 0;

                if (ePerk != PerkType.NONE)
                {
                    if (Globals.Infos.perk(ePerk).meBuildingClassLevel == eBuildingClass)
                    {
                        iCount++;
                    }
                }

                for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                {
                    int iPerkCount = pCorporation.getPerkCount(eLoopPerk);
                    if (iPerkCount > 0)
                    {
                        if (Globals.Infos.perk(eLoopPerk).meBuildingClassLevel == eBuildingClass)
                        {
                            iCount += iPerkCount;
                        }
                    }
                }

                return iCount;
            }
        }

        public virtual int AI_getPerkValue(Corporation pCorporation, PerkType ePerk)
        {
            int iValue = 0;

            iValue += (Globals.Infos.perk(ePerk).miClaims * 50);
            iValue += (Globals.Infos.perk(ePerk).miSabotages * 50);
            iValue += -(Globals.Infos.perk(ePerk).miInterestModifier);

            for (HQType eLoopHQ = 0; eLoopHQ < Globals.Infos.HQsNum(); eLoopHQ++)
            {
                if ((Globals.Infos.perk(ePerk).mbUnlockHQAll) || (Globals.Infos.perk(ePerk).meHQUnlock == eLoopHQ))
                {
                    if (!canFoundHQ(pCorporation, eLoopHQ))
                    {
                        iValue += 100;
                    }
                }
            }

            {
                BuildingType eBuildingImmune = Globals.Infos.perk(ePerk).meBuildingImmune;

                if (eBuildingImmune != BuildingType.NONE)
                {
                    BuildingClassType eBuildingClassImmune = Globals.Infos.building(eBuildingImmune).meClass;

                    int iClassLevel = AI_getBuildingClassLevel(pCorporation, ePerk, eBuildingClassImmune);
                    if (iClassLevel > 0)
                    {
                        iValue += ((AI_getBuildingResourceValue(pCorporation, eBuildingImmune) * GameClient.getBuildingClassLevelBonus(eBuildingClassImmune, iClassLevel, getLocation(), Globals.Infos)) / 100);
                        iValue += ((AI_getBuildingOrderValue(pCorporation, eBuildingImmune) * (iClassLevel + 1)) / 2);
                    }
                }
            }

            {
                BuildingClassType eBuildingClass = Globals.Infos.perk(ePerk).meBuildingClassLevel;

                if (eBuildingClass != BuildingClassType.NONE)
                {
                    if (!(Globals.Infos.buildingClass(eBuildingClass).mabLocationNoPerks[(int)getLocation()]))
                    {
                        iValue += AI_getBuildingResourceValue(pCorporation, Globals.Infos.buildingClass(eBuildingClass).meBaseBuilding);

                        {
                            int iBuildingOrderValue = AI_getBuildingOrderValue(pCorporation, Globals.Infos.buildingClass(eBuildingClass).meBaseBuilding);

                            OrderType eOrder = Globals.Infos.buildingClass(eBuildingClass).meOrderType;

                            if (eOrder != OrderType.LAUNCH)
                            {
                                int iCount = 0;

                                for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                                {
                                    int iPerkCount = pCorporation.getPerkCount(eLoopPerk);
                                    if (iPerkCount > 0)
                                    {
                                        BuildingClassType eLoopBuildingClass = Globals.Infos.perk(eLoopPerk).meBuildingClassLevel;

                                        if (eLoopBuildingClass != BuildingClassType.NONE)
                                        {
                                            if (Globals.Infos.buildingClass(eLoopBuildingClass).meOrderType == eOrder)
                                            {
                                                iCount += iPerkCount;
                                            }
                                        }
                                    }
                                }

                                if (iCount > 0)
                                {
                                    iBuildingOrderValue /= (iCount + 2);
                                }
                            }

                            iValue += iBuildingOrderValue;
                        }
                    }
                }
            }

            {
                PatentType ePatent = Globals.Infos.perk(ePerk).mePatent;

                if (ePatent != PatentType.NONE)
                {
                    int iPatentValue = 0;

                    {
                        iPatentValue += (Globals.Infos.patent(ePatent).miDebtCut * 4);
                    }

                    {
                        int iModifier = Globals.Infos.patent(ePatent).miEntertainmentModifier;
                        if (iModifier != 0)
                        {
                            for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                            {
                                int iPerkCount = pCorporation.getPerkCount(eLoopPerk);
                                if (iPerkCount > 0)
                                {
                                    BuildingClassType eBuildingClass = Globals.Infos.perk(eLoopPerk).meBuildingClassLevel;

                                    if (eBuildingClass != BuildingClassType.NONE)
                                    {
                                        if (Globals.Infos.building(Globals.Infos.buildingClass(eBuildingClass).meBaseBuilding).miEntertainment > 0)
                                        {
                                            iPatentValue += ((iModifier * iPerkCount) * 4);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    {
                        int iModifier = Globals.Infos.patent(ePatent).miPowerConsumptionModifier;
                        if (iModifier != 0)
                        {
                            for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                            {
                                int iPerkCount = pCorporation.getPerkCount(eLoopPerk);
                                if (iPerkCount > 0)
                                {
                                    BuildingClassType eBuildingClass = Globals.Infos.perk(eLoopPerk).meBuildingClassLevel;

                                    if (eBuildingClass != BuildingClassType.NONE)
                                    {
                                        if (Globals.Infos.building(Globals.Infos.buildingClass(eBuildingClass).meBaseBuilding).miPowerConsumption > 0)
                                        {
                                            iPatentValue += ((-(iModifier) * iPerkCount) / 5);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    {
                        int iModifier = Globals.Infos.patent(ePatent).miConnectedHQPowerProductionModifier;
                        if (iModifier != 0)
                        {
                            for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                            {
                                int iPerkCount = pCorporation.getPerkCount(eLoopPerk);
                                if (iPerkCount > 0)
                                {
                                    BuildingClassType eBuildingClass = Globals.Infos.perk(eLoopPerk).meBuildingClassLevel;

                                    if (eBuildingClass != BuildingClassType.NONE)
                                    {
                                        if (Utils.isBuildingOutput(Globals.Infos.buildingClass(eBuildingClass).meBaseBuilding, Globals.Infos.Globals.ENERGY_RESOURCE, null))
                                        {
                                            iPatentValue += ((iModifier * iPerkCount) / 4);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    {
                        int iModifier = Globals.Infos.patent(ePatent).miAdjacentHQSabotageModifier;
                        if (iModifier != 0)
                        {
                            for (BuildingClassType eLoopBuildingClass = 0; eLoopBuildingClass < Globals.Infos.buildingClassesNum(); eLoopBuildingClass++)
                            {
                                int iBuildingClassLevel = AI_getBuildingClassLevel(pCorporation, PerkType.NONE, eLoopBuildingClass);
                                if (iBuildingClassLevel > 0)
                                {
                                    if (Globals.Infos.buildingClass(eLoopBuildingClass).meOrderType != OrderType.NONE)
                                    {
                                        iPatentValue += (-(iModifier) * iBuildingClassLevel);
                                    }
                                }
                            }
                        }
                    }

                    if (Globals.Infos.patent(ePatent).mbBorehole)
                    {
                        for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                        {
                            int iPerkCount = pCorporation.getPerkCount(eLoopPerk);
                            if (iPerkCount > 0)
                            {
                                BuildingClassType eBuildingClass = Globals.Infos.perk(eLoopPerk).meBuildingClassLevel;

                                if (eBuildingClass != BuildingClassType.NONE)
                                {
                                    if (Globals.Infos.building(Globals.Infos.buildingClass(eBuildingClass).meBaseBuilding).miPowerConsumption > 0)
                                    {
                                        iPatentValue += (10 * iPerkCount);
                                    }
                                }
                            }
                        }
                    }

                    if (Globals.Infos.patent(ePatent).mbRecycleScrap)
                    {
                        for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                        {
                            int iPerkCount = pCorporation.getPerkCount(eLoopPerk);
                            if (iPerkCount > 0)
                            {
                                BuildingClassType eBuildingClass = Globals.Infos.perk(eLoopPerk).meBuildingClassLevel;

                                if (eBuildingClass != BuildingClassType.NONE)
                                {
                                    iPatentValue += 10;
                                }
                            }
                        }
                    }

                    if (Globals.Infos.patent(ePatent).mbAdjacentMining)
                    {
                        for (BuildingType eLoopBuilding = 0; eLoopBuilding < Globals.Infos.buildingsNum(); eLoopBuilding++)
                        {
                            bool bValid = false;

                            for (HQType eLoopHQ = 0; eLoopHQ < Globals.Infos.HQsNum(); eLoopHQ++)
                            {
                                if (canFoundHQ(pCorporation, eLoopHQ))
                                {
                                    if (Utils.isBuildingValid(eLoopBuilding, eLoopHQ))
                                    {
                                        bValid = true;
                                        break;
                                    }
                                }
                            }

                            if (bValid)
                            {
                                if (Globals.Infos.building(eLoopBuilding).mbSelfInput)
                                {
                                    iPatentValue += 10;
                                }
                            }
                        }

                        for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                        {
                            int iPerkCount = pCorporation.getPerkCount(eLoopPerk);
                            if (iPerkCount > 0)
                            {
                                BuildingClassType eBuildingClass = Globals.Infos.perk(eLoopPerk).meBuildingClassLevel;

                                if (eBuildingClass != BuildingClassType.NONE)
                                {
                                    if (Utils.isBuildingMiningAny(Globals.Infos.buildingClass(eBuildingClass).meBaseBuilding))
                                    {
                                        iPatentValue += (10 * iPerkCount);
                                    }
                                }
                            }
                        }
                    }

                    if (Globals.Infos.patent(ePatent).mbTeleportation)
                    {
                        for (BuildingType eLoopBuilding = 0; eLoopBuilding < Globals.Infos.buildingsNum(); eLoopBuilding++)
                        {
                            bool bValid = false;

                            for (HQType eLoopHQ = 0; eLoopHQ < Globals.Infos.HQsNum(); eLoopHQ++)
                            {
                                if (canFoundHQ(pCorporation, eLoopHQ))
                                {
                                    if (Utils.isBuildingValid(eLoopBuilding, eLoopHQ))
                                    {
                                        bValid = true;
                                        break;
                                    }
                                }
                            }

                            if (bValid)
                            {
                                if (Globals.Infos.building(eLoopBuilding).mbSelfInput)
                                {
                                    iPatentValue += 20;
                                }
                            }
                        }

                        for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                        {
                            int iPerkCount = pCorporation.getPerkCount(eLoopPerk);
                            if (iPerkCount > 0)
                            {
                                BuildingClassType eBuildingClass = Globals.Infos.perk(eLoopPerk).meBuildingClassLevel;

                                if (eBuildingClass != BuildingClassType.NONE)
                                {
                                    if (Utils.isBuildingYieldAny(Globals.Infos.buildingClass(eBuildingClass).meBaseBuilding, null))
                                    {
                                        iPatentValue += (5 * iPerkCount);
                                    }
                                }
                            }
                        }
                    }

                    {
                        ResourceType eBuildingFreeResource = Globals.Infos.patent(ePatent).meBuildingFreeResource;

                        if (eBuildingFreeResource != ResourceType.NONE)
                        {
                            iPatentValue += (getResourcePrice(eBuildingFreeResource) / 5);

                            for (HQType eLoopHQ = 0; eLoopHQ < Globals.Infos.HQsNum(); eLoopHQ++)
                            {
                                if (canFoundHQ(pCorporation, eLoopHQ))
                                {
                                    if (Globals.Infos.HQ(eLoopHQ).meBaseResource == eBuildingFreeResource)
                                    {
                                        iPatentValue += 50;
                                        break;
                                    }
                                }
                            }

                            for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                            {
                                int iPerkCount = pCorporation.getPerkCount(eLoopPerk);
                                if (iPerkCount > 0)
                                {
                                    BuildingClassType eBuildingClass = Globals.Infos.perk(eLoopPerk).meBuildingClassLevel;

                                    if (eBuildingClass != BuildingClassType.NONE)
                                    {
                                        if (Globals.Infos.building(Globals.Infos.buildingClass(eBuildingClass).meBaseBuilding).maiResourceInput[(int)eBuildingFreeResource] > 0)
                                        {
                                            iPatentValue += (20 * iPerkCount);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    for (ResourceType eLoopResource = 0; eLoopResource < Globals.Infos.resourcesNum(); eLoopResource++)
                    {
                        int iCapacity = Globals.Infos.patent(ePatent).maiResourceCapacity[(int)eLoopResource];
                        if (iCapacity > 0)
                        {
                            iPatentValue += (iCapacity * getResourcePrice(eLoopResource)) / 200;

                            for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                            {
                                int iPerkCount = pCorporation.getPerkCount(eLoopPerk);
                                if (iPerkCount > 0)
                                {
                                    BuildingClassType eBuildingClass = Globals.Infos.perk(eLoopPerk).meBuildingClassLevel;

                                    if (eBuildingClass != BuildingClassType.NONE)
                                    {
                                        if (Utils.isBuildingYield(Globals.Infos.buildingClass(eBuildingClass).meBaseBuilding, eLoopResource, null))
                                        {
                                            iPatentValue += ((iCapacity * iPerkCount) / 10);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    {
                        ResourceType eAlternateGasResource = Globals.Infos.patent(ePatent).meAlternateGasResource;

                        if (eAlternateGasResource != ResourceType.NONE)
                        {
                            iPatentValue += (Math.Max(0, (getResourcePrice(Globals.Infos.Globals.GAS_RESOURCE) - getResourcePrice(eAlternateGasResource))) * 2);

                            for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                            {
                                int iPerkCount = pCorporation.getPerkCount(eLoopPerk);
                                if (iPerkCount > 0)
                                {
                                    BuildingClassType eBuildingClass = Globals.Infos.perk(eLoopPerk).meBuildingClassLevel;

                                    if (eBuildingClass != BuildingClassType.NONE)
                                    {
                                        if (Utils.isBuildingYield(Globals.Infos.buildingClass(eBuildingClass).meBaseBuilding, eAlternateGasResource, null))
                                        {
                                            iPatentValue += (50 * iPerkCount);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    {
                        ResourceType eAlternatePowerResource = Globals.Infos.patent(ePatent).meAlternatePowerResource;

                        if (eAlternatePowerResource != ResourceType.NONE)
                        {
                            iPatentValue += (Math.Max(0, (getResourcePrice(Globals.Infos.Globals.ENERGY_RESOURCE) - getResourcePrice(eAlternatePowerResource))) * 2);

                            for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                            {
                                int iPerkCount = pCorporation.getPerkCount(eLoopPerk);
                                if (iPerkCount > 0)
                                {
                                    BuildingClassType eBuildingClass = Globals.Infos.perk(eLoopPerk).meBuildingClassLevel;

                                    if (eBuildingClass != BuildingClassType.NONE)
                                    {
                                        if (Utils.isBuildingYield(Globals.Infos.buildingClass(eBuildingClass).meBaseBuilding, eAlternatePowerResource, null))
                                        {
                                            iPatentValue += (50 * iPerkCount);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    for (BuildingType eLoopBuilding = 0; eLoopBuilding < Globals.Infos.buildingsNum(); eLoopBuilding++)
                    {
                        bool bValid = false;

                        for (HQType eLoopHQ = 0; eLoopHQ < Globals.Infos.HQsNum(); eLoopHQ++)
                        {
                            if (canFoundHQ(pCorporation, eLoopHQ))
                            {
                                if (Utils.isBuildingValid(eLoopBuilding, eLoopHQ))
                                {
                                    bValid = true;
                                    break;
                                }
                            }
                        }

                        if (bValid)
                        {
                            if (Globals.Infos.patent(ePatent).mabBuildingAlwaysOn[(int)eLoopBuilding])
                            {
                                iPatentValue += 25;
                            }

                            if (Globals.Infos.patent(ePatent).mabBuildingClassBoost[(int)(Globals.Infos.building(eLoopBuilding).meClass)])
                            {
                                iPatentValue += 25;
                            }
                        }
                    }

                    for (ResourceType eLoopResource = 0; eLoopResource < Globals.Infos.resourcesNum(); eLoopResource++)
                    {
                        ResourceType eReplaceResource = Globals.Infos.patent(ePatent).maeResourceReplace[(int)eLoopResource];

                        if (eReplaceResource != ResourceType.NONE)
                        {
                            iPatentValue += Math.Max(0, (getResourcePrice(eLoopResource) - getResourcePrice(eReplaceResource)));
                        }
                    }

                    for (BuildingClassType eLoopBuildingClass = 0; eLoopBuildingClass < Globals.Infos.buildingClassesNum(); eLoopBuildingClass++)
                    {
                        IceType eIgnoreInputIce = Globals.Infos.patent(ePatent).maeIgnoreInputIce[(int)eLoopBuildingClass];

                        if (eIgnoreInputIce != IceType.NONE)
                        {
                            for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                            {
                                int iPerkCount = pCorporation.getPerkCount(eLoopPerk);
                                if (iPerkCount > 0)
                                {
                                    if (Globals.Infos.perk(eLoopPerk).meBuildingClassLevel == eLoopBuildingClass)
                                    {
                                        iPatentValue += (50 * iPerkCount);
                                    }
                                }
                            }
                        }
                    }

                    {
                        int iCount = 0;

                        for (LocationType eLocation = 0; eLocation < LocationType.NUM_TYPES; eLocation++)
                        {
                            if (!(Globals.Infos.patent(ePatent).mabLocationInvalid[(int)eLocation]))
                            {
                                iCount++;
                            }
                        }

                        iPatentValue *= (iCount + 1);
                        iPatentValue /= (int)(LocationType.NUM_TYPES + 1);
                    }

                    iPatentValue *= Math.Max(0, (Globals.Infos.personality(pCorporation.mePersonality).maiPatentValueModifier[(int)ePatent] + 100));
                    iPatentValue /= 100;

                    iValue += iPatentValue;
                }
            }

            for (ResourceType eLoopResource = 0; eLoopResource < Globals.Infos.resourcesNum(); eLoopResource++)
            {
                int iModifier = Globals.Infos.perk(ePerk).maiResourceProductionModifier[(int)eLoopResource];
                if (iModifier != 0)
                {
                    int iSubValue = (((getResourcePrice(eLoopResource) * 2) * iModifier) / 100);

                    {
                        int iCount = 0;

                        for (LocationType eLocation = 0; eLocation < LocationType.NUM_TYPES; eLocation++)
                        {
                            if (!(Globals.Infos.resource(eLoopResource).mabLocationInvalid[(int)eLocation]))
                            {
                                iCount++;
                            }
                        }

                        iSubValue *= (iCount + 1);
                        iSubValue /= (int)(LocationType.NUM_TYPES + 1);
                    }

                    iValue += iSubValue;
                }
            }

            for (BuildingClassType eLoopBuildingClass = 0; eLoopBuildingClass < Globals.Infos.buildingClassesNum(); eLoopBuildingClass++)
            {
                int iModifier = Globals.Infos.perk(ePerk).maiBuildingClassInputModifier[(int)eLoopBuildingClass];
                if (iModifier != 0)
                {
                    BuildingType eBaseBuilding = Globals.Infos.buildingClass(eLoopBuildingClass).meBaseBuilding;

                    int iSubValue = 0;

                    for (ResourceType eLoopResource = 0; eLoopResource < Globals.Infos.resourcesNum(); eLoopResource++)
                    {
                        int iInput = Globals.Infos.building(eBaseBuilding).maiResourceInput[(int)eLoopResource];
                        if (iInput > 0)
                        {
                            iSubValue += -((((getResourcePrice(eLoopResource) * iInput) / Constants.RESOURCE_MULTIPLIER) * iModifier) / 100);
                        }
                    }

                    if (iSubValue != 0)
                    {
                        int iClassLevel = AI_getBuildingClassLevel(pCorporation, ePerk, eLoopBuildingClass);
                        if (iClassLevel > 0)
                        {
                            iValue += ((iSubValue * GameClient.getBuildingClassLevelBonus(eLoopBuildingClass, iClassLevel, getLocation(), Globals.Infos)) / 100);
                        }
                    }
                }
            }

            for (OrderType eLoopOrder = 0; eLoopOrder < Globals.Infos.ordersNum(); eLoopOrder++)
            {
                int iModifier = 0;

                iModifier += -(Globals.Infos.perk(ePerk).maiOrderCostModifier[(int)eLoopOrder] * 2);
                iModifier += Globals.Infos.perk(ePerk).maiOrderRateModifier[(int)eLoopOrder];

                if (iModifier != 0)
                {
                    for (BuildingClassType eLoopBuildingClass = 0; eLoopBuildingClass < Globals.Infos.buildingClassesNum(); eLoopBuildingClass++)
                    {
                        int iBuildingClassLevel = AI_getBuildingClassLevel(pCorporation, PerkType.NONE, eLoopBuildingClass);
                        if (iBuildingClassLevel > 0)
                        {
                            if (Globals.Infos.buildingClass(eLoopBuildingClass).meOrderType == eLoopOrder)
                            {
                                iValue += (-(iModifier) * iBuildingClassLevel);
                            }
                        }
                    }
                }
            }

            for (BuildingClassType eLoopBuildingClass = 0; eLoopBuildingClass < Globals.Infos.buildingClassesNum(); eLoopBuildingClass++)
            {
                ResourceLevelType eMininumMining = Globals.Infos.perk(ePerk).maeMinimumMining[(int)eLoopBuildingClass];
                
                if (eMininumMining > ResourceLevelType.NONE)
                {
                    int iClassLevel = AI_getBuildingClassLevel(pCorporation, ePerk, eLoopBuildingClass);
                    if (iClassLevel > 0)
                    {
                        BuildingType eBaseBuilding = Globals.Infos.buildingClass(eLoopBuildingClass).meBaseBuilding;

                        for (ResourceType eLoopResource = 0; eLoopResource < Globals.Infos.resourcesNum(); eLoopResource++)
                        {
                            int iMining = GameClient.resourceMining(null, Globals.Infos, getHandicap(), true, iClassLevel, eBaseBuilding, eLoopResource, eMininumMining, TerrainType.NONE, PlayerType.NONE, Globals.Infos.building(eBaseBuilding).maiResourceMining[(int)eLoopResource], 0, true);
                            if (iMining > 0)
                            {
                                iValue += (((getResourcePrice(eLoopResource) * 2) * iMining) / Constants.RESOURCE_MULTIPLIER);
                            }
                        }
                    }
                }
            }

            for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
            {
                int iModifier = Globals.Infos.perk(ePerk).maiPerkCostModifier[(int)eLoopPerk];
                if (iModifier != 0)
                {
                    iValue += (-(iModifier) / ((pCorporation.getPerkCount(eLoopPerk) == 0) ? 5 : 10));
                }
            }

            {
                int iEventValue = 0;

                {
                    BuildingType eFreeBuilding = Globals.Infos.perk(ePerk).meFreeBuilding;

                    if (eFreeBuilding != BuildingType.NONE)
                    {
                        BuildingClassType eFreeBuildingClass = Globals.Infos.building(eFreeBuilding).meClass;

                        iEventValue += ((AI_getBuildingResourceValue(pCorporation, eFreeBuilding) * GameClient.getBuildingClassLevelBonus(eFreeBuildingClass, AI_getBuildingClassLevel(pCorporation, ePerk, eFreeBuildingClass), getLocation(), Globals.Infos)) / 100);
                        iEventValue += (AI_getBuildingOrderValue(pCorporation, eFreeBuilding) * 2);
                    }
                }

                for (ResourceType eLoopResource = 0; eLoopResource < Globals.Infos.resourcesNum(); eLoopResource++)
                {
                    int iSubValue = Globals.Infos.perk(ePerk).maiResourceStockpile[(int)eLoopResource];
                    if (iSubValue > 0)
                    {
                        {
                            int iCount = 0;

                            for (LocationType eLocation = 0; eLocation < LocationType.NUM_TYPES; eLocation++)
                            {
                                if (!(Globals.Infos.resource(eLoopResource).mabLocationInvalid[(int)eLocation]))
                                {
                                    iCount++;
                                }
                            }

                            iSubValue *= (iCount + 1);
                            iSubValue /= (int)(LocationType.NUM_TYPES + 1);
                        }

                        iEventValue += ((getResourcePrice(eLoopResource) * iSubValue) / 100);

                        {
                            int iMultiplier = 0;

                            for (HQType eLoopHQ = 0; eLoopHQ < Globals.Infos.HQsNum(); eLoopHQ++)
                            {
                                if (canFoundHQ(pCorporation, eLoopHQ))
                                {
                                    if (Globals.Infos.HQ(eLoopHQ).meBaseResource == eLoopResource)
                                    {
                                        iMultiplier = Math.Max(iMultiplier, 3);
                                    }
                                    else if (Globals.Infos.HQ(eLoopHQ).maiUpgradeResource[(int)eLoopResource] > 0 && (AppCore.AppGlobals.GameGlobals.GameClient == null || AppCore.AppGlobals.GameGlobals.GameClient.isResourceValid(eLoopResource)))
                                    {
                                        iMultiplier = Math.Max(iMultiplier, 2);
                                    }
                                    else if (Globals.Infos.HQ(eLoopHQ).maiLifeSupport[(int)eLoopResource] > 0)
                                    {
                                        iMultiplier = Math.Max(iMultiplier, 1);
                                    }
                                }
                            }

                            iEventValue += (iSubValue * iMultiplier);
                        }
                    }
                }

                for (SabotageType eLoopSabotage = 0; eLoopSabotage < Globals.Infos.sabotagesNum(); eLoopSabotage++)
                {
                    int iCount = Globals.Infos.perk(ePerk).maiSabotageCount[(int)eLoopSabotage];
                    if (iCount > 0)
                    {
                        int iSubValue = 10;

                        iSubValue += (Globals.Infos.sabotage(eLoopSabotage).miHarvestQuantity / 10);

                        iSubValue += (Globals.Infos.sabotage(eLoopSabotage).miPlunderQuantity / 5);

                        iSubValue += (Globals.Infos.sabotage(eLoopSabotage).miDestroyUnitRange / 2);

                        if (Globals.Infos.sabotage(eLoopSabotage).miEffectTime > 0)
                        {
                            int iTemp = 0;

                            if (Globals.Infos.sabotage(eLoopSabotage).mbFreezeBuilding)
                            {
                                iTemp += 8;
                            }
                            if (Globals.Infos.sabotage(eLoopSabotage).mbDoubleBuilding)
                            {
                                iTemp += 24;
                            }
                            if (Globals.Infos.sabotage(eLoopSabotage).mbHalfBuilding)
                            {
                                iTemp += 6;
                            }
                            if (Globals.Infos.sabotage(eLoopSabotage).mbOverloadBuilding)
                            {
                                iTemp += 4;
                            }
                            if (Globals.Infos.sabotage(eLoopSabotage).mbVirusBuilding)
                            {
                                iTemp += 4;
                            }

                            iTemp *= Globals.Infos.sabotage(eLoopSabotage).miEffectTime;
                            iTemp /= 100;

                            if (Globals.Infos.sabotage(eLoopSabotage).miEffectRange > 0)
                            {
                                iTemp *= Globals.Infos.sabotage(eLoopSabotage).miEffectRange;
                                iTemp /= 2;
                            }

                            if (Globals.Infos.sabotage(eLoopSabotage).miEffectLength > 0)
                            {
                                iTemp *= Globals.Infos.sabotage(eLoopSabotage).miEffectLength;
                                iTemp /= 10;
                            }

                            iSubValue += iTemp;
                        }

                        iSubValue += (-(Globals.Infos.sabotage(eLoopSabotage).miResourceLevelChange) * 5);

                        iSubValue += (Globals.Infos.sabotage(eLoopSabotage).miTakeoverTime / 4);

                        iSubValue += (Globals.Infos.sabotage(eLoopSabotage).miDamageBuilding / 10);

                        if (Globals.Infos.sabotage(eLoopSabotage).mbNewResource)
                        {
                            iSubValue += 10;
                        }

                        if (Globals.Infos.sabotage(eLoopSabotage).mbDefendSabotage)
                        {
                            iSubValue += 30;
                        }

                        if (!(Globals.Infos.sabotage(eLoopSabotage).mbTriggersDefense) && Globals.Infos.sabotage(eLoopSabotage).mbHostile)
                        {
                            iSubValue += 4;
                        }

                        iEventValue += (iSubValue * iCount);
                    }
                }

                if (Globals.Infos.perk(ePerk).mbOnFound)
                {
                    iValue += iEventValue;
                }

                if (Globals.Infos.perk(ePerk).mbOnUpgrade)
                {
                    iValue += (iEventValue * 2);
                }

                if (Globals.Infos.perk(ePerk).mbOnLevel)
                {
                    iValue += ((iEventValue * 5) / 2);
                }

                if (Globals.Infos.perk(ePerk).meOnHQLevel > HQLevelType.ZERO)
                {
                    iValue += ((iEventValue * (((int)(Globals.Infos.HQLevelsNum()) * 2) - (int)(Globals.Infos.perk(ePerk).meOnHQLevel + 1))) / 10);
                }
            }

            iValue *= Math.Max(0, 100 + Globals.Infos.personality(pCorporation.mePersonality).maiPerkValueModifier[(int)ePerk]);
            iValue /= 100;

            return Math.Max(1, iValue);
        }

        public virtual void AI_buyPerks()
        {
            foreach (Corporation pLoopCorporation in getCorporations())
            {
                if (!(pLoopCorporation.mbDead))
                {
                    if (pLoopCorporation.meID != CorporationType.HUMAN)
                    {
                        List<int> aiPerksAvailable = new List<int>();

                        for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                        {
                            aiPerksAvailable.Add(0);
                        }

                        fillPerksArray(aiPerksAvailable);

                        for (int iPass = 0; iPass < 2; iPass++)
                        {
                            bool bPermanent = (iPass == 0);

                            while (pLoopCorporation.miMoney > 100000)
                            {
                                PerkType eBestPerk = PerkType.NONE;
                                int iBestValue = 0;

                                for (PerkType eLoopPerk = 0; eLoopPerk < Globals.Infos.perksNum(); eLoopPerk++)
                                {
                                    if (aiPerksAvailable[(int)eLoopPerk] > 0)
                                    {
                                        if ((bPermanent) ? AI_buyPerkPermanent(pLoopCorporation, eLoopPerk) : true)
                                        {
                                            if (canBuyPerk(pLoopCorporation.meID, eLoopPerk, bPermanent))
                                            {
                                                int iValue = AI_getPerkValue(pLoopCorporation, eLoopPerk);

                                                if ((Globals.Infos.perk(eLoopPerk).meBuildingClassLevel != BuildingClassType.NONE) && (pLoopCorporation.getPerkCount(eLoopPerk) == 0))
                                                {
                                                    iValue += 200;
                                                }

                                                iValue += mRandomAI.Next(200);

                                                if (iValue > iBestValue)
                                                {
                                                    eBestPerk = eLoopPerk;
                                                    iBestValue = iValue;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (eBestPerk != PerkType.NONE)
                                {
                                    buyPerk(pLoopCorporation.meID, eBestPerk, bPermanent);

                                    aiPerksAvailable[(int)eBestPerk]--;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public virtual int getLevelCorporationPercent(LevelType eIndex, CorporationType eCorporation)
        {
            int iCorporationShares = 0;
            int iTotalShares = 0;

            foreach (Corporation pLoopCorporation in getCorporations())
            {
                if (pLoopCorporation.mdiLevelColonyShares.ContainsKey(eIndex))
                {
                    if (pLoopCorporation.meID == eCorporation)
                        iCorporationShares = pLoopCorporation.mdiLevelColonyShares[eIndex];

                    iTotalShares += pLoopCorporation.mdiLevelColonyShares[eIndex];
                }
            }

            if (iTotalShares > 0)
            {
                return ((iCorporationShares * 100) / iTotalShares);
            }
            else
            {
                return 0;
            }
        }

        public virtual int getLevelCorporationAdjacentIncome(LevelType level, CorporationType corp, bool pretendWinner = false)
        {
            int income = 0;

            if (getLevelWinner(level) == corp || pretendWinner)
            {
                for (LevelType eAltLevel = 0; eAltLevel < Globals.Infos.levelsNum(); eAltLevel++)
                {
                    if (Globals.Infos.level(eAltLevel).mabAdjacentLevel[(int)level])
                    {
                        if (getLevelWinner(eAltLevel) == corp)
                        {
                            income += campaignMode().miAdjacencyIncome;
                        }
                    }
                }
            }

            return income;
        }

        public virtual int getLevelCorporationIncome(LevelType eLevel, CorporationType eCorporation)
        {
            int iIncome = ((getLevelIncome(eLevel) * getLevelCorporationPercent(eLevel, eCorporation)) / 100);
            iIncome += getLevelCorporationAdjacentIncome(eLevel, eCorporation);

            return iIncome;
        }

        public virtual int calculateIncome(CorporationType eCorporation)
        {
            int iIncome = 0;

            iIncome += campaignMode().miBaseIncome;

            if (eCorporation != CorporationType.HUMAN)
            {
                iIncome += Globals.Infos.handicap(getHandicap()).miAICampaignIncome;
            }

            for (LevelType eLoopLevel = 0; eLoopLevel < Globals.Infos.levelsNum(); eLoopLevel++)
            {
                iIncome += getLevelCorporationIncome(eLoopLevel, eCorporation);
            }

            return iIncome;
        }

        public virtual int calculatePerkValue(Corporation pCorporation)
        {
            using (new UnityProfileScope("calculatePerkValue"))
            {
                int iValue = 0;

                foreach (InfoPerk pLoopPerk in Globals.Infos.perks())
                {
                    {
                        int iCount = pCorporation.maiTempPerkCount[pLoopPerk.miType];
                        if (iCount > 0)
                        {
                            iValue += iCount * getPerkCost(pLoopPerk.meType, pCorporation, false);
                        }
                    }

                    {
                        int iCount = pCorporation.maiPermPerkCount[pLoopPerk.miType];

                        iCount -= getOriginalPerkCount(pCorporation, pLoopPerk.meType);

                        if (iCount > 0)
                        {
                            iValue += iCount * getPerkCost(pLoopPerk.meType, pCorporation, true);
                        }
                    }
                }

                return iValue;
            }
        }

        public virtual int calculateShareValue(Corporation pCorporation)
        {
            int iValue = 0;

            iValue += campaignMode().miBaseShareValue;
            iValue += pCorporation.miHQs;
            iValue += pCorporation.miMoney;
            iValue += (pCorporation.miDebt * Globals.Infos.handicap(getCorporationHandicap(pCorporation.meID)).miDebtMultiplier);
            iValue += pCorporation.miStructures;
            iValue += pCorporation.miModules;
            iValue += calculatePerkValue(pCorporation);

            return iValue;
        }

        public virtual int calculateSharePrice(Corporation pCorporation)
        {
            return Math.Max(10, (calculateShareValue(pCorporation) / STOCK_DIVISOR));
        }

        public virtual int getOriginalPerkCount(Corporation pCorporation, PerkType ePerk)
        {
            if (Globals.Infos.perk(ePerk).mabLocationInvalid[(int)getLocation()])
            {
                return 0;
            }

            return Globals.Infos.executive(pCorporation.meExecutive).maiPerkCount[(int)ePerk];
        }

        public virtual void Serialize(bool bInGame, bool bAuto)
        {
            try
            {
                mbPlayingTheGame = bInGame; // Keep track of if the player was in the game for loading
                mbIsDLCBobCampaign = campaignMode().mbBobCampaignDLC;

                {
                    IFormatter formatter = new BinaryFormatter();
                    using (Stream stream = new FileStream(CampaignSaveFullPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        formatter.Serialize(stream, this);
                    }
                }

                if (bAuto)
                {
                    IFormatter formatter = new BinaryFormatter();
                    using (Stream stream = new FileStream(CampaignAutoFullPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        formatter.Serialize(stream, this);
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Error while trying to serialize campaign data: " + e.ToString());
            }
        }

        public virtual void fixLoadedCampaign()
        {
            if (meLocation == LocationType.NONE)
            {
                meLocation = LocationType.MARS;
            }

            foreach (Corporation pLoopCorporation in getCorporations())
            {
                if (pLoopCorporation.mePlayerColor == PlayerColorType.NONE)
                {
                    pLoopCorporation.mePlayerColor = Globals.Infos.character(Globals.Infos.personality(pLoopCorporation.mePersonality).meCharacter).mePlayerColor;
                }

                while (pLoopCorporation.maiTempPerkCount.Count < (int)(Globals.Infos.perksNum()))
                {
                    pLoopCorporation.maiTempPerkCount.Add(0);
                    pLoopCorporation.maiPermPerkCount.Add(0);
                    pLoopCorporation.maiLevelPerkCount.Add(0);
                    pLoopCorporation.maiLevelPerkTime.Add(0);
                }

                while (pLoopCorporation.mabEventTurnSeen.Count < (int)(Globals.Infos.eventTurnsNum()))
                {
                    pLoopCorporation.mabEventTurnSeen.Add(false);
                }
            }

            while (maiPerksAvailable.Count < (int)(Globals.Infos.perksNum()))
            {
                maiPerksAvailable.Add(0);
                maiPermPerksBought.Add(0);
                maiTempPerksBought.Add(0);
                maiCampaignModePerks.Add(0);
            }

            while (maiLevelPopulation.Count < (int)(Globals.Infos.levelsNum()))
            {
                maiLevelPopulation.Add(0);
                maiLevelTurn.Add(0);
                maeLevelState.Add(LevelStateType.UNOPENED);
                maeLevelPerk.Add(PerkType.NONE);
                maeLevelColony.Add(ColonyType.NONE);
                maeLevelColonyClass.Add(ColonyClassType.NONE);
                maeLevelSevenSols.Add(SevenSolsType.COLONY);
                maeLevelResourcePresence.Add(ResourcePresenceType.NONE);
                maeLevelEvent.Add(EventLevelType.NONE);
                maeLevelWinner.Add(CorporationType.NONE);
                maabLevelBlackMarket.Add(Enumerable.Repeat(false, (int)Globals.Infos.blackMarketsNum()).ToList());
                maaeLevelCorporation.Add(new List<CorporationType>());
                fillLevelSettings((LevelType)(maiLevelPopulation.Count - 1));
            }

            while (mabGameOption.Count < (int)(Globals.Infos.gameOptionsNum()))
            {
                mabGameOption.Add(Globals.Infos.gameOption((GameOptionType)(mabGameOption.Count)).mbDefaultValueCampaign);
            }

            while (mabStoryUsed.Count < (int)(Globals.Infos.storiesNum()))
            {
                mabStoryUsed.Add(false);
            }

            while (mabCampaignModeWon.Count < (int)(Globals.Infos.campaignModeNums()))
            {
                mabCampaignModeWon.Add(false);
            }

            while (maabLevelBlackMarket.First().Count < (int)(Globals.Infos.blackMarketsNum()))
            {
                for (int i = 0; i < Globals.Infos.levels().Count; i++)
                {
                    maabLevelBlackMarket[i].Add(false);
                }
            }

            if (maiResourcePrice.Count < (int)(Globals.Infos.resourcesNum()))
            {
                maiResourcePrice.Clear();
                maiResourcePriceOffworld.Clear();

                foreach (InfoResource pLoopResource in Globals.Infos.resources())
                {
                    maiResourcePrice.Add(pLoopResource.miMarketPrice);
                    maiResourcePriceOffworld.Add(pLoopResource.miOffworldPrice + mRandom.Next(pLoopResource.miOffworldRand));
                }

                foreach (TurnResult turnResult in mTurnResults)
                {
                    foreach (LevelResult levelResult in turnResult.mLevelResults)
                    {
                        levelResult.maiResourcePrices.Clear();
                        levelResult.maiResourcePricesOffworld.Clear();

                        foreach (InfoResource pLoopResource in Globals.Infos.resources())
                        {
                            levelResult.maiResourcePrices.Add(pLoopResource.miMarketPrice);
                            levelResult.maiResourcePricesOffworld.Add(pLoopResource.miOffworldPrice + mRandom.Next(pLoopResource.miOffworldRand));
                        }
                    }
                }
            }
        }

        public static Campaign Deserialize(bool bAuto)
        {
            string zPath = (bAuto) ? CampaignAutoFullPath : CampaignSaveFullPath;

            if (!File.Exists(zPath))
                return null;

            try
            {
                IFormatter formatter = new BinaryFormatter();
                using (Stream stream = new FileStream(zPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Campaign pCampaign = (Campaign)formatter.Deserialize(stream);
                    pCampaign.fixLoadedCampaign();
                    return pCampaign;
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("Error while trying to deserialize campaign data: " + e.ToString());
                return null;
            }
        }

        public static Campaign Deserialize(Stream stream )
        {
            try
            {
                IFormatter formatter = new BinaryFormatter();
                Campaign pCampaign = (Campaign)formatter.Deserialize(stream);
                pCampaign.fixLoadedCampaign();
                return pCampaign;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("Error while trying to deserialize campaign data: " + e.ToString());
                return null;
            }
        }

        public void Serialize(Stream stream )
        {
            try
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, this);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Error while trying to serialize campaign data: " + e.ToString());
            }
        }


        public static bool ValidCampaignSave(Func<bool> IsValidSaveGame, bool bAuto)
        {
            string zPath = (bAuto) ? CampaignAutoFullPath : CampaignSaveFullPath;

            if (!File.Exists(zPath))
            {
                return false;
            }

            Campaign campaign = Deserialize(bAuto);

            if (campaign == null) // File no longer compatable so delete it
            {
                DeleteCampaignSave(zPath);
                return false;
            }

            if (campaign.mbIsDLCBobCampaign)
            {
                return Globals.AppInfo.OwnsDLCBobCampaign;
            }

            if (campaign.isPlayingTheGame() && !IsValidSaveGame()) //check if save game is valid too
            {
                return false;
            }

            return true;
        }

        private static void DeleteCampaignSave(string filePath)
        {
            try
            {
                File.Delete(filePath);
            }
            catch
            {
                UnityEngine.Debug.Log("Unable to delete old campaign file");
            }
        }

        public virtual int getActiveCorporations()
        {
            return miActiveCorporations;
        }

        public virtual int getTurn()
        {
            return miTurn;
        }
        public static int getTurnsTotalStatic(int iGrowthRounds, int iStartingCorps, int iFinalCorps)
        {
            return (iGrowthRounds + iStartingCorps - iFinalCorps + 1);
        }
        public virtual int getTurnsTotal()
        {
            return getTurnsTotalStatic(campaignMode().miNumGrowthRounds, campaignMode().miStartingCorps, campaignMode().miFinalCorps);
        }
        public virtual int getTurnsLeft()
        {
            return (getTurnsTotal() - getTurn());
        }

        public virtual int getSeed()
        {
            return miSeed;
        }

        public virtual bool isPlayingTheGame()
        {
            return mbPlayingTheGame;
        }

        public virtual bool isAutoSave()
        {
            return mbAutoSave;
        }
        public virtual void setAutoSave(bool bNewValue)
        {
            mbAutoSave = bNewValue;
        }

        public virtual bool isShowEvent()
        {
            return mbShowEvent;
        }
        public virtual void setShowEvent(bool bNewValue)
        {
            mbShowEvent = bNewValue;
        }

        public virtual bool isDLCBobCampaign()
        {
            return mbIsDLCBobCampaign;
        }

        public virtual EventTurnType getEventTurn()
        {
            return meEventTurn;
        }
        public virtual void setEventTurn(EventTurnType eNewValue)
        {
            meEventTurn = eNewValue;
        }
        public virtual bool canDoEventTurnOption(EventTurnOptionType eEventTurnOption, Corporation pCorporation)
        {
            if (eEventTurnOption == EventTurnOptionType.NONE)
            {
                return true;
            }

            if (Globals.Infos.eventTurnOption(eEventTurnOption).mbIoDLC && !(Globals.AppInfo.OwnsDLCIo))
            {
                return false;
            }

            if (Globals.Infos.eventTurnOption(eEventTurnOption).mabLocationInvalid[(int)getLocation()])
            {
                return false;
            }

            if (Globals.Infos.eventTurnOption(eEventTurnOption).miDebt < 0)
            {
                if (getCampaignState() == CampaignState.FINAL_ROUND)
                {
                    return false;
                }
            }

            bool bLoss = false;
            bool bGain = false;

            foreach (InfoPerk pLoopPerk in Globals.Infos.perks())
            {
                int iChange = Globals.Infos.eventTurnOption(eEventTurnOption).maiPerkChange[pLoopPerk.miType];
                int iTime = Globals.Infos.eventTurnOption(eEventTurnOption).maiPerkTime[pLoopPerk.miType];

                if (iChange < 0)
                {
                    bLoss = true;
                }
                else if (iChange > 0)
                {
                    bGain = true;
                }

                if (iChange < 0)
                {
                    if ((pCorporation.maiPermPerkCount[pLoopPerk.miType] + iChange) < Math.Min(1, getOriginalPerkCount(pCorporation, pLoopPerk.meType)))
                    {
                        return false;
                    }
                }

                if ((iChange > 0) || (iTime > 0))
                {
                    if (Globals.Infos.eventTurnOption(eEventTurnOption).mbPerkUnique)
                    {
                        if (pCorporation.getPerkCount(pLoopPerk.meType) > 0)
                        {
                            return false;
                        }
                    }

                    for (PerkType eOtherPerk = 0; eOtherPerk < Globals.Infos.perksNum(); eOtherPerk++)
                    {
                        if (pLoopPerk.mabPerkSkip[(int)eOtherPerk])
                        {
                            if (pCorporation.getPerkCount(eOtherPerk) > 0)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            if (bLoss && !bGain)
            {
                if (getTurnsLeft() <= 2)
                {
                    return false;
                }
            }

            return true;
        }
        public virtual bool canDoEventTurn(EventTurnType eEventTurn, Corporation pCorporation)
        {
            if (pCorporation.mabEventTurnSeen[(int)eEventTurn])
            {
                return false;
            }

            if (!canDoEventTurnOption(Globals.Infos.eventTurn(eEventTurn).meOption1, pCorporation))
            {
                return false;
            }

            if (!canDoEventTurnOption(Globals.Infos.eventTurn(eEventTurn).meOption2, pCorporation))
            {
                return false;
            }

            if (!canDoEventTurnOption(Globals.Infos.eventTurn(eEventTurn).meOption3, pCorporation))
            {
                return false;
            }

            return true;
        }
        public virtual EventTurnType findEventTurn(Corporation pCorporation)
        {
            EventTurnType eBestTurnType = EventTurnType.NONE;
            int iBestValue = 0;

            foreach (InfoEventTurn pLoopEventTurnType in Globals.Infos.eventTurns())
            {
                if (canDoEventTurn(pLoopEventTurnType.meType, pCorporation))
                {
                    int iValue = 1;

                    iValue += mRandom.Next(((getTurn() % 2) == 0) ? pLoopEventTurnType.miAppearanceProb1 : pLoopEventTurnType.miAppearanceProb2);

                    if (iValue > iBestValue)
                    {
                        eBestTurnType = pLoopEventTurnType.meType;
                        iBestValue = iValue;
                    }
                }
            }

            return eBestTurnType;
        }
        public virtual void doEventTurnOption(EventTurnOptionType eEventTurnOption, Corporation pCorporation)
        {
            pCorporation.miMoney += Globals.Infos.eventTurnOption(eEventTurnOption).miMoney;

            {
                int iPerkMoney = Globals.Infos.eventTurnOption(eEventTurnOption).miMoneyPerk;
                adjustPerkCost(ref iPerkMoney, true);
                pCorporation.miMoney += iPerkMoney;
            }

            pCorporation.miDebt += Globals.Infos.eventTurnOption(eEventTurnOption).miDebt;

            foreach (InfoPerk pLoopPerk in Globals.Infos.perks())
            {
                int iChange = Globals.Infos.eventTurnOption(eEventTurnOption).maiPerkChange[pLoopPerk.miType];
                if (iChange != 0)
                {
                    pCorporation.maiPermPerkCount[pLoopPerk.miType] = Math.Max(0, (pCorporation.maiPermPerkCount[pLoopPerk.miType] + iChange));
                }

                int iTime = Globals.Infos.eventTurnOption(eEventTurnOption).maiPerkTime[pLoopPerk.miType];
                if (iTime > 0)
                {
                    pCorporation.maiLevelPerkCount[pLoopPerk.miType]++;
                    pCorporation.maiLevelPerkTime[pLoopPerk.miType] += iTime;
                }
            }

            sortCorporations();

            if (pCorporation.meID == CorporationType.HUMAN)
            {
                setEventTurn(EventTurnType.NONE);
            }
        }

        public virtual EventGameType getEventGame()
        {
            return meEventGame;
        }
        public virtual void doEventGame()
        {
            EventGameType eNewValue = EventGameType.NONE;

            if (campaignMode().maeWeekEventGame.Count > getTurn())
            {
                eNewValue = campaignMode().maeWeekEventGame[getTurn()];
            }

            if (eNewValue == EventGameType.NONE)
            {
                if (mRandom.Next(3) == 0)
                {
                    int iBestValue = 0;

                    for (EventGameType eLoopEventGame = 0; eLoopEventGame < Globals.Infos.eventGamesNum(); eLoopEventGame++)
                    {
                        if (getEventGame() != eLoopEventGame)
                        {
                            int iProb = campaignMode().maiEventGameProb[(int)eLoopEventGame];
                            if (iProb > 0)
                            {
                                int iValue = mRandom.Next(iProb) + 1;
                                if (iValue > iBestValue)
                                {
                                    eNewValue = eLoopEventGame;
                                    iBestValue = iValue;
                                }
                            }
                        }
                    }
                }
            }

            meEventGame = eNewValue;
        }

        public virtual Corporation getCorporation(CorporationType eIndex)
        {
            if (eIndex < 0 || eIndex >= (CorporationType)mapCorporations.Count)
            {
                return null;
            }
            else
            {
                return mapCorporations[(int)eIndex];
            }
        }
        public virtual Corporation getHuman()
        {
            return getCorporation(CorporationType.HUMAN);
        }
        public virtual Corporation getCorporationByRank(int iRank)
        {
            foreach (Corporation pLoopCorporation in getCorporations())
            {
                if (pLoopCorporation.miRank == iRank)
                {
                    return pLoopCorporation;
                }
            }

            return null;
        }
        public virtual int countCorporations(LevelType eLevel)
        {
            int iCount = 0;

            foreach (Corporation pLoopCorporation in getCorporations())
            {
                if (pLoopCorporation.meCurrentLevel == eLevel)
                {
                    iCount++;
                }
            }

            return iCount;
        }
        public virtual List<Corporation> getCorporations()
        {
            return mapCorporations;
        }
        public virtual void killCorporation(CorporationType eIndex)
        {
            Corporation pCorporation = getCorporation(eIndex);

            if (pCorporation != null)
            {
                if (!(pCorporation.mbDead))
                {
                    pCorporation.mbDead = true;
                    miActiveCorporations--;
                }
            }
        }

        public virtual string getEventText(CorporationType eIndex1, int iTurn2)
        {
            return maazEventText[(int)eIndex1][iTurn2];
        }
        public virtual void addEventText(CorporationType eIndex1, int iTurn2, string zNewValue)
        {
            if (maazEventText[(int)eIndex1][iTurn2].Length > 0)
            {
                maazEventText[(int)eIndex1][iTurn2] += "\n";
            }

            maazEventText[(int)eIndex1][iTurn2] += zNewValue;
        }
        public virtual void incrementEventText()
        {
            for (int i = 0; i < maazEventText.Count; i++)
            {
                maazEventText[i].Add("");
            }
        }

        public virtual CampaignModeType getCampaignMode()
        {
            return meCampaignMode;
        }
        public virtual InfoCampaignMode campaignMode()
        {
            return Globals.Infos.campaignMode(meCampaignMode);
        }

        public virtual CampaignState getCampaignState()
        {
            return meCampaignState;
        }
        public virtual void updateCampaignState()
        {
            if (getActiveCorporations() <= campaignMode().miFinalCorps)
            {
                meCampaignState = CampaignState.FINAL_ROUND;
            }
            else if (getTurn() == campaignMode().miNumGrowthRounds)
            {
                meCampaignState = CampaignState.ELIMINATION_ROUND;
            }
        }

        public virtual HandicapType getHandicap()
        {
            return meHandicap;
        }
        public virtual HandicapType getCorporationHandicap(CorporationType eCorporation)
        {
            if (eCorporation == CorporationType.HUMAN)
            {
                return getHandicap();
            }
            else
            {
                return Globals.Infos.Globals.DEFAULT_HANDICAP;
            }
        }

        public virtual LocationType getLocation()
        {
            return meLocation;
        }

        public virtual int getResourcePrice(ResourceType eIndex)
        {
            return maiResourcePrice[(int)eIndex];
        }
        public virtual void setResourcePrice(ResourceType eIndex, int iNewValue)
        {
            maiResourcePrice[(int)eIndex] = Utils.clamp(iNewValue, (Constants.PRICE_MIN / Constants.PRICE_MULTIPLIER), (Constants.PRICE_MAX / Constants.PRICE_MULTIPLIER));
        }

        public virtual int getResourcePriceOffworld(ResourceType eIndex)
        {
            return maiResourcePriceOffworld[(int)eIndex];
        }
        public virtual void setResourcePriceOffworld(ResourceType eIndex, int iNewValue)
        {
            maiResourcePriceOffworld[(int)eIndex] = iNewValue;
        }

        public virtual int getPerksAvailable(PerkType eIndex)
        {
            return maiPerksAvailable[(int)eIndex];
        }

        public virtual int getPermPerksBought(PerkType eIndex)
        {
            return maiPermPerksBought[(int)eIndex];
        }

        public virtual int getTempPerksBought(PerkType eIndex)
        {
            return maiTempPerksBought[(int)eIndex];
        }

        public virtual int getLevelPopulation(LevelType eIndex)
        {
            return maiLevelPopulation[(int)eIndex];
        }
        public virtual int getLevelIncome(LevelType eIndex)
        {
            return (getLevelPopulation(eIndex) * getIncomePerPopulation(eIndex));
        }
        public virtual void setLevelPopulation(LevelType eIndex, int iNewValue)
        {
            maiLevelPopulation[(int)eIndex] = iNewValue;
        }

        public virtual int getLevelTurn(LevelType eIndex)
        {
            return maiLevelTurn[(int)eIndex];
        }
        public virtual int getIncomePerPopulation(LevelType eIndex)
        {
            int iTurns = Math.Max(0, (getLevelTurn(eIndex) - campaignMode().miNumGrowthRounds + 1));

            return campaignMode().miIncomePerPopulationBase + (iTurns * campaignMode().miIncomePerPopulationPer);
        }
        public virtual void setLevelTurn(LevelType eIndex, int iNewValue)
        {
            maiLevelTurn[(int)eIndex] = iNewValue;
        }

        public virtual int getCampaignModePerks(PerkType eIndex)
        {
            return maiCampaignModePerks[(int)eIndex];
        }

        public virtual bool isGameOption(GameOptionType eIndex)
        {
            return mabGameOption[(int)eIndex];
        }
        public virtual List<bool> getGameOptions()
        {
            return mabGameOption;
        }

        public virtual bool isStoryUsed(StoryType eStory)
        {
            return mabStoryUsed[(int)eStory];
        }
        public virtual void makeStoryUsed(StoryType eStory)
        {
            mabStoryUsed[(int)eStory] = true;
        }

        public virtual bool isCampaignModeWon(CampaignModeType eIndex)
        {
            return mabCampaignModeWon[(int)eIndex];
        }

        public virtual LevelStateType getLevelState(LevelType eIndex)
        {
            return maeLevelState[(int)eIndex];
        }
        public virtual void setLevelState(LevelType eIndex, LevelStateType eNewValue)
        {
            maeLevelState[(int)eIndex] = eNewValue;
        }

        public virtual PerkType getLevelPerk(LevelType eIndex)
        {
            return maeLevelPerk[(int)eIndex];
        }
        public virtual void setLevelPerk(LevelType eIndex, PerkType eNewValue)
        {
            maeLevelPerk[(int)eIndex] = eNewValue;
        }

        public virtual ColonyType getLevelColony(LevelType eIndex)
        {
            return maeLevelColony[(int)eIndex];
        }
        public virtual void setLevelColony(LevelType eIndex, ColonyType eNewValue)
        {
            maeLevelColony[(int)eIndex] = eNewValue;
        }

        public virtual ColonyClassType getLevelColonyClass(LevelType eIndex)
        {
            return maeLevelColonyClass[(int)eIndex];
        }
        public virtual void setLevelColonyClass(LevelType eIndex, ColonyClassType eNewValue)
        {
            maeLevelColonyClass[(int)eIndex] = eNewValue;
        }

        public virtual SevenSolsType getLevelSevenSols(LevelType eIndex)
        {
            return maeLevelSevenSols[(int)eIndex];
        }
        public virtual void setLevelSevenSols(LevelType eIndex, SevenSolsType eNewValue)
        {
            maeLevelSevenSols[(int)eIndex] = eNewValue;
        }

        public virtual ResourcePresenceType getLevelResourcePresence(LevelType eIndex)
        {
            return maeLevelResourcePresence[(int)eIndex];
        }

        public virtual EventLevelType getLevelEvent(LevelType eIndex)
        {
            return maeLevelEvent[(int)eIndex];
        }

        public virtual CorporationType getLevelWinner(LevelType eIndex)
        {
            return maeLevelWinner[(int)eIndex];
        }
        public virtual void makeLevelWinner(LevelType eIndex, CorporationType eNewValue)
        {
            if ((getLevelWinner(eIndex) == CorporationType.NONE) && (eNewValue != CorporationType.NONE))
            {
                maeLevelWinner[(int)eIndex] = eNewValue;

                Corporation pCorporation = getCorporation(getLevelWinner(eIndex));

                PerkType eLevelPerk = getLevelPerk(eIndex);

                if (eLevelPerk != PerkType.NONE)
                {
                    int iTime = Globals.Infos.perk(eLevelPerk).miLevelTime;
                    if (iTime == 0)
                    {
                        pCorporation.maiPermPerkCount[(int)getLevelPerk(eIndex)]++;
                    }
                    else
                    {
                        pCorporation.maiLevelPerkCount[(int)getLevelPerk(eIndex)]++;
                        pCorporation.maiLevelPerkTime[(int)getLevelPerk(eIndex)] += Globals.Infos.perk(eLevelPerk).miLevelTime;
                    }
                }

                ColonyType eLevelColony = getLevelColony(eIndex);

                if (eLevelColony != ColonyType.NONE)
                {
                    pCorporation.maeColonyBonusLevel[(int)eLevelColony] = (ColonyBonusLevelType)(Math.Min((int)(campaignMode().meMaxColonyBonusLevel), (int)(pCorporation.maeColonyBonusLevel[(int)eLevelColony] + 1)));
                }
            }
        }

        public virtual bool isLevelBlackMarket(LevelType eIndex1, BlackMarketType eIndex2)
        {
            return maabLevelBlackMarket[(int)eIndex1][(int)eIndex2];
        }
    }
}