using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.IO;
using System.Linq;
using Offworld.GameCore.Text;
using Offworld.SystemCore;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Offworld.GameCore
{
    public class GameClient : TextHelpers
    {
        protected static readonly bool cVERBOSE_LOGGING = false;

        protected Infos mInfos = null;
        public virtual Infos infos()
        {
            return mInfos;
        }

        public static int[] saiBlackMarketCostModifier = new int[Constants.MAX_NUM_PLAYERS] { 50, 50, 0, 0, -25, -25, -50, -50 };

        public enum GameDirtyType
        {
            FIRST,

            miSystemUpdateCount,
            miGameUpdateCount,
            miTurnCount,
            miTurnBasedTime,
            miDelayTime,
            miMinutes,
            miHours,
            miDays,
            miStartModuleTileID,
            miPopulation,
            miMaxPopulation,
            miColonyCap,
            miLabor,
            miHQLevels,
            miEntertainmentDemand,
            miEntertainmentSupply,
            miAuctionCount,
            miAuctionBid,
            miAuctionTime,
            miAuctionData1,
            miAuctionData2,
            miEventStateGameDelay,
            miEventStateGameTime,
            miSharesBought,
            miFoundMoney,
            miGeothermalCount,
            mbGameOver,
            mbPerkAuctioned,
            mbAuctionStarted,
            meHumanHandicap,
            meWinningTeam,
            meCheatingPlayer,
            meAuction,
            meAuctionLeader,
            meLastEventGame,
            meEventStateGame,
            meEventStateLevel,
            meGameSpeed,
            mePrevGameSpeed,
            meHQHighest,
            meNextPopulationModule,
            meScenarioDifficulty,
            maiEventStateTime,
            maiTerrainCount,
            maiResourceRateCount,
            maiModuleCount,
            maiBlackMarketCount,
            maiEspionageCount,
            maiWholesaleSlotCount,
            mabBuildingClassFinished,
            mabBuildingUnavailable,
            mabBlackMarketAvailable,
            mabNextLaborModule,
            mabWholesaleSlotResets,
            maePatentOwner,
            maeWholesaleSlotResource,
            maeWholesaleSlotResourceNext,
            maiImportCost,
            mEventGameTimeList,
            ShipmentRequest,
            ColonySettings,

            NUM_TYPES
        }

        protected BitMaskMulti mDirtyBits = new BitMaskMulti((int)GameDirtyType.NUM_TYPES);
        protected bool isDirty(GameDirtyType eType)
        {
            return mDirtyBits.GetBit((int)eType);
        }
        public bool isAnyDirty()
        {
            return !mDirtyBits.IsEmpty();
        }
        public bool isAnyDirtyBesidesSystemUpdateCount()
        {
            int bit = (int)GameDirtyType.miSystemUpdateCount;
            bool oldBit = mDirtyBits.GetBit(bit); //save bit
            mDirtyBits.SetBit(bit, false); //clear bit
            bool anyDirty = !mDirtyBits.IsEmpty();
            mDirtyBits.SetBit(bit, oldBit); //restore bit
            return anyDirty;
        }

        protected int miNumTeams = 0;
        protected int miSystemUpdateCount = 0;
        protected int miGameUpdateCount = 0;
        protected int miTurnCount = 0;
        protected int miTurnBasedTime = 0;
        protected int miDelayTime = 0;
        protected int miMinutes = 0;
        protected int miHours = 0;
        protected int miDays = 0;
        protected int miStartModuleTileID = -1;
        protected int miPopulation = 0;
        protected int miMaxPopulation = 0;
        protected int miColonyCap = 0;
        protected int miLabor = 0;
        protected int miHQLevels = 0;
        protected int miEntertainmentDemand = 0;
        protected int miEntertainmentSupply = 0;
        protected int miAuctionCount = 0;
        protected int miAuctionBid = 0;
        protected int miAuctionTime = 0;
        protected int miAuctionData1 = 0;
        protected int miAuctionData2 = 0;
        protected int miEventStateGameDelay = 0;
        protected int miEventStateGameTime = 0;
        protected int miSharesBought = 0;
        protected int miFoundMoney = 0;
        protected int miGeothermalCount = 0;

        protected bool mbGameOver = false;
        protected bool mbPerkAuctioned = false;
        protected bool mbAuctionStarted = false;

        protected HandicapType meHumanHandicap = HandicapType.NONE;
        protected TeamType meWinningTeam = TeamType.NONE;
        protected PlayerType meCheatingPlayer = PlayerType.NONE;
        protected AuctionType meAuction = AuctionType.NONE;
        protected PlayerType meAuctionLeader = PlayerType.NONE;
        protected EventGameType meLastEventGame = EventGameType.NONE;
        protected EventStateType meEventStateGame = EventStateType.NONE;
        protected EventStateType meEventStateLevel = EventStateType.NONE;
        protected GameSpeedType meGameSpeed = GameSpeedType.NONE;
        protected GameSpeedType mePrevGameSpeed = GameSpeedType.NONE;
        protected ColonyType meColony = ColonyType.NONE;
        protected ColonyClassType meColonyClass = ColonyClassType.NONE;
        protected HQLevelType meHQHighest = HQLevelType.NONE;
        protected ModuleType meNextPopulationModule = ModuleType.NONE;
        protected SevenSolsType meSevenSols = SevenSolsType.NONE;
        protected ScenarioDifficultyType meScenarioDifficultyType = ScenarioDifficultyType.NONE;

        protected List<int> maiIceCount = new List<int>();
        protected List<int> maiTerrainCount = new List<int>();
        protected List<int> maiResourceRateCount = new List<int>();
        protected List<int> maiModuleCount = new List<int>();
        protected List<int> maiBlackMarketCount = new List<int>();
        protected List<int> maiEspionageCount = new List<int>();
        protected List<int> maiWholesaleSlotCount = new List<int>();
        protected List<int> maiBuildingAverageModifier = new List<int>();
        protected List<int> maiImportCost = new List<int>();

        protected List<bool> mabGameOptions = new List<bool>();
        protected List<bool> mabInvalidHQ = new List<bool>();
        protected List<bool> mabBuildingClassFinished = new List<bool>();
        protected List<bool> mabBuildingUnavailable = new List<bool>();
        protected List<bool> mabBlackMarketAvailable = new List<bool>();
        protected List<bool> mabNextLaborModule = new List<bool>();
        protected List<bool> mabWholesaleSlotResets = new List<bool>();

        protected List<PlayerType> maePatentOwner = new List<PlayerType>();

        protected List<ResourceType> maeWholesaleSlotResource = new List<ResourceType>();
        protected List<ResourceType> maeWholesaleSlotResourceNext = new List<ResourceType>();
        protected List<ResourceType> maeImportResource = new List<ResourceType>();
        protected List<ResourceType> maeImportPaymentResource = new List<ResourceType>();

        protected List<string> maTeamNames = new List<String>();

        protected List<List<int>> maaiBuildingResourceOutput = new List<List<int>>();

        protected LinkedList<EventGameTime> mEventGameTimeList = new LinkedList<EventGameTime>();

        protected GameSettings mGameSettings = null;
        protected CustomColonySettings mColonySettings = null;
        protected ScenarioShipmentRequest mShipmentRequest = null;
        protected MarketClient mMarket = null;
        protected GameEventsClient mGameEvents = null;
        protected StatsClient mStats = null;
        protected ConditionManagerClient mConditionManager = null;
        protected MapClient mMap = null;

        protected List<PlayerClient> maPlayers = new List<PlayerClient>();

        protected Dictionary<int, ModuleClient> mModuleDictionary = new Dictionary<int, ModuleClient>();
        protected Dictionary<int, HQClient> mHQDictionary = new Dictionary<int, HQClient>();
        protected Dictionary<int, ConstructionClient> mConstructionDictionary = new Dictionary<int, ConstructionClient>();
        protected Dictionary<int, BuildingClient> mBuildingDictionary = new Dictionary<int, BuildingClient>();
        protected Dictionary<int, UnitClient> mUnitDictionary = new Dictionary<int, UnitClient>();

        //unserialized state
        protected float startGameTimeUnscaled;
        protected Stopwatch timer;

        public GameClient()
        {
            timer = new Stopwatch();
            timer.Start();
        }

        public virtual void initClient(Infos pInfos, GameSettings pSettings)
        {
            using (new UnityProfileScope("GameClient::initClient"))
            {
                mInfos = pInfos;

                mGameSettings = pSettings;

                maTeamNames = Enumerable.Repeat("", Constants.MAX_NUM_PLAYERS).ToList();

                mMap = Globals.Factory.createMapClient(this);
                mMap.initClient(this);

                mMarket = Globals.Factory.createMarketClient(this);
                mGameEvents = Globals.Factory.createGameEventsClient();
                mStats = Globals.Factory.createStatsClient(this);
                mConditionManager = Globals.Factory.createConditionManagerClient(this);

                maPlayers = Enumerable.Range(0, pSettings.miNumPlayers).Select(o => (PlayerClient)(Globals.Factory.createPlayerClient(this))).ToList();
                meScenarioDifficultyType = pSettings.meScenarioDifficulty;

                mMap.initClientTiles();

                refreshCachedValues();

                startGameTimeUnscaled = (float)timer.Elapsed.TotalSeconds;
            }
        }

        protected virtual void refreshCachedValues()
        {
            using (new UnityProfileScope("GameClient.refreshCachedValues"))
            {
                mMap.UpdateAdjacency();
            }
        }

        protected virtual void SerializeClient(object stream, bool bLoadSave, bool bRebuildingServer, int compatibilityNumber)
        {
            SimplifyIO.Data(stream, ref mDirtyBits, "DirtyBits");

            bool bAll = (bLoadSave || bRebuildingServer);

            if (bLoadSave || bRebuildingServer)
            {
                if (stream is BinaryReader)
                {
                    if (this is GameServer)
                    {
                        mMap = Globals.Factory.createMapServer(this);
                    }
                    else
                    {
                        mMap = Globals.Factory.createMapClient(this);
                    }
                }
            }

            if (isDirty(GameDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref miNumTeams, "NumTeams");
            }
            if (isDirty(GameDirtyType.miSystemUpdateCount) || bAll)
            {
                SimplifyIO.Data(stream, ref miSystemUpdateCount, "SystemUpdateCount");
            }
            if (isDirty(GameDirtyType.miGameUpdateCount) || bAll)
            {
                SimplifyIO.Data(stream, ref miGameUpdateCount, "GameUpdateCount");
            }
            if (isDirty(GameDirtyType.miTurnCount) || bAll)
            {
                SimplifyIO.Data(stream, ref miTurnCount, "TurnCount");
            }
            if (isDirty(GameDirtyType.miTurnBasedTime) || bAll)
            {
                SimplifyIO.Data(stream, ref miTurnBasedTime, "TurnBasedTime");
            }
            if (isDirty(GameDirtyType.miDelayTime) || bAll)
            {
                SimplifyIO.Data(stream, ref miDelayTime, "DelayTime");
            }
            if (isDirty(GameDirtyType.miMinutes) || bAll)
            {
                SimplifyIO.Data(stream, ref miMinutes, "Minutes");
            }
            if (isDirty(GameDirtyType.miHours) || bAll)
            {
                SimplifyIO.Data(stream, ref miHours, "Hours");
            }
            if (isDirty(GameDirtyType.miDays) || bAll)
            {
                SimplifyIO.Data(stream, ref miDays, "Days");
            }
            if (isDirty(GameDirtyType.miStartModuleTileID) || bAll)
            {
                SimplifyIO.Data(stream, ref miStartModuleTileID, "StartModuleTileID");
            }
            if (isDirty(GameDirtyType.miPopulation) || bAll)
            {
                SimplifyIO.Data(stream, ref miPopulation, "Population");
            }
            if (isDirty(GameDirtyType.miMaxPopulation) || bAll)
            {
                SimplifyIO.Data(stream, ref miMaxPopulation, "MaxPopulation");
            }
            if (isDirty(GameDirtyType.miColonyCap) || bAll)
            {
                SimplifyIO.Data(stream, ref miColonyCap, "ColonyCap");
            }
            if (isDirty(GameDirtyType.miLabor) || bAll)
            {
                SimplifyIO.Data(stream, ref miLabor, "Labor");
            }
            if (isDirty(GameDirtyType.miHQLevels) || bAll)
            {
                SimplifyIO.Data(stream, ref miHQLevels, "HQLevels");
            }
            if (isDirty(GameDirtyType.miEntertainmentDemand) || bAll)
            {
                SimplifyIO.Data(stream, ref miEntertainmentDemand, "EntertainmentDemand");
            }
            if (isDirty(GameDirtyType.miEntertainmentSupply) || bAll)
            {
                SimplifyIO.Data(stream, ref miEntertainmentSupply, "EntertainmentSupply");
            }
            if (isDirty(GameDirtyType.miAuctionCount) || bAll)
            {
                SimplifyIO.Data(stream, ref miAuctionCount, "AuctionCount");
            }
            if (isDirty(GameDirtyType.miAuctionBid) || bAll)
            {
                SimplifyIO.Data(stream, ref miAuctionBid, "AuctionBid");
            }
            if (isDirty(GameDirtyType.miAuctionTime) || bAll)
            {
                SimplifyIO.Data(stream, ref miAuctionTime, "AuctionTime");
            }
            if (isDirty(GameDirtyType.miAuctionData1) || bAll)
            {
                SimplifyIO.Data(stream, ref miAuctionData1, "AuctionData1");
            }
            if (isDirty(GameDirtyType.miAuctionData2) || bAll)
            {
                SimplifyIO.Data(stream, ref miAuctionData2, "AuctionData2");
            }
            if (isDirty(GameDirtyType.miEventStateGameDelay) || bAll)
            {
                SimplifyIO.Data(stream, ref miEventStateGameDelay, "EventStateGameDelay");
            }
            if (isDirty(GameDirtyType.miEventStateGameTime) || bAll)
            {
                SimplifyIO.Data(stream, ref miEventStateGameTime, "EventStateGameTime");
            }
            if (isDirty(GameDirtyType.miSharesBought) || bAll)
            {
                SimplifyIO.Data(stream, ref miSharesBought, "SharesBought");
            }
            if (isDirty(GameDirtyType.miFoundMoney) || bAll)
            {
                SimplifyIO.Data(stream, ref miFoundMoney, "FoundMoney");
            }
            if (isDirty(GameDirtyType.miGeothermalCount) || bAll)
            {
                SimplifyIO.Data(stream, ref miGeothermalCount, "GeothermalCount");
            }
            if (isDirty(GameDirtyType.FIRST) || bAll)
            {
                int lat = mapClient().getMapLatitude();
                SimplifyIO.Data(stream, ref lat, "MapLatitude");
                mapClient().setMapLatitude(lat);
            }

            if (isDirty(GameDirtyType.mbGameOver) || bAll)
            {
                SimplifyIO.Data(stream, ref mbGameOver, "GameOver");
            }
            if (isDirty(GameDirtyType.mbPerkAuctioned) || bAll)
            {
                SimplifyIO.Data(stream, ref mbPerkAuctioned, "PerkAuctioned");
            }
            if (isDirty(GameDirtyType.mbAuctionStarted) || bAll)
            {
                SimplifyIO.Data(stream, ref mbAuctionStarted, "AuctionStarted");
            }

            if (isDirty(GameDirtyType.meHumanHandicap) || bAll)
            {
                SimplifyIO.Data(stream, ref meHumanHandicap, "HumanHandicap");
            }
            if (isDirty(GameDirtyType.meWinningTeam) || bAll)
            {
                SimplifyIO.Data(stream, ref meWinningTeam, "WinningTeam");
            }
            if (isDirty(GameDirtyType.meCheatingPlayer) || bAll)
            {
                SimplifyIO.Data(stream, ref meCheatingPlayer, "CheatingPlayer");
            }
            if (isDirty(GameDirtyType.meAuction) || bAll)
            {
                SimplifyIO.Data(stream, ref meAuction, "Auction");
            }
            if (isDirty(GameDirtyType.meAuctionLeader) || bAll)
            {
                SimplifyIO.Data(stream, ref meAuctionLeader, "AuctionLeader");
            }
            if (isDirty(GameDirtyType.meLastEventGame) || bAll)
            {
                SimplifyIO.Data(stream, ref meLastEventGame, "LastEventGame");
            }
            if (isDirty(GameDirtyType.meEventStateGame) || bAll)
            {
                SimplifyIO.Data(stream, ref meEventStateGame, "EventStateGame");
            }
            if (isDirty(GameDirtyType.meEventStateLevel) || bAll)
            {
                SimplifyIO.Data(stream, ref meEventStateLevel, "EventStateLevel");
            }
            if (isDirty(GameDirtyType.meGameSpeed) || bAll)
            {
                SimplifyIO.Data(stream, ref meGameSpeed, "GameSpeed");
            }
            if (isDirty(GameDirtyType.mePrevGameSpeed) || bAll)
            {
                SimplifyIO.Data(stream, ref mePrevGameSpeed, "PrevGameSpeed");
            }
            if (isDirty(GameDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref meColony, "Colony");
            }
            if (isDirty(GameDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref meColonyClass, "ColonyClass");
            }
            if (isDirty(GameDirtyType.meHQHighest) || bAll)
            {
                SimplifyIO.Data(stream, ref meHQHighest, "HQHighest");
            }
            if (isDirty(GameDirtyType.meNextPopulationModule) || bAll)
            {
                SimplifyIO.Data(stream, ref meNextPopulationModule, "NextPopulationModule");
            }
            if (isDirty(GameDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref meSevenSols, "SevenSols");
            }
            if (isDirty(GameDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref meScenarioDifficultyType, "ScenarioDifficultyType");
            }

            if (isDirty(GameDirtyType.FIRST) || bAll)
            {
                List<int> tempThickness = mMap.getThicknessArray();
                SimplifyIO.Data(stream, ref tempThickness, (int)(infos().heightsNum()), "TerrainThickness_");
                mMap.setThicknessArray(tempThickness);
            }
            if (isDirty(GameDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref maiIceCount, (int)(infos().icesNum()), "IceCount");
            }
            if (isDirty(GameDirtyType.maiTerrainCount) || bAll)
            {
                SimplifyIO.Data(stream, ref maiTerrainCount, (int)(infos().terrainsNum()), "TerrainCount_");
            }
            if (isDirty(GameDirtyType.maiResourceRateCount) || bAll)
            {
                SimplifyIO.Data(stream, ref maiResourceRateCount, (int)(infos().resourcesNum()), "ResourceRateCount_");
            }
            if (isDirty(GameDirtyType.maiModuleCount) || bAll)
            {
                SimplifyIO.Data(stream, ref maiModuleCount, (int)(infos().modulesNum()), "ModuleCount_");
            }
            if (isDirty(GameDirtyType.maiBlackMarketCount) || bAll)
            {
                SimplifyIO.Data(stream, ref maiBlackMarketCount, (int)(infos().blackMarketsNum()), "BlackMarketCount_");
            }
            if (isDirty(GameDirtyType.maiEspionageCount) || bAll)
            {
                SimplifyIO.Data(stream, ref maiEspionageCount, (int)(infos().espionagesNum()), "EspionageCount_");
            }
            if (isDirty(GameDirtyType.maiWholesaleSlotCount) || bAll)
            {
                SimplifyIO.Data(stream, ref maiWholesaleSlotCount, infos().Globals.NUM_WHOLESALE_SLOTS, "WholesaleSlotCount_");
            }
            if (isDirty(GameDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref maiBuildingAverageModifier, (int)(infos().buildingsNum()), "BuildingAverageModifier_");
            }
            if (isDirty(GameDirtyType.maiImportCost) || bAll)
            {
                SimplifyIO.Data(stream, ref maiImportCost, "ImportCosts");
            }

            if (isDirty(GameDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref mabGameOptions, (int)(infos().gameOptionsNum()), "GameOptions_");
            }
            if (isDirty(GameDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref mabInvalidHQ, (int)(infos().HQsNum()), "InvalidHQ_");
            }
            if (isDirty(GameDirtyType.mabBuildingClassFinished) || bAll)
            {
                SimplifyIO.Data(stream, ref mabBuildingClassFinished, (int)(infos().buildingClassesNum()), "BuildingClassFinished_");
            }
            if (isDirty(GameDirtyType.mabBuildingUnavailable) || bAll)
            {
                SimplifyIO.Data(stream, ref mabBuildingUnavailable, (int)(infos().buildingsNum()), "BuildingUnavailable_");
            }
            if (isDirty(GameDirtyType.mabBlackMarketAvailable) || bAll)
            {
                SimplifyIO.Data(stream, ref mabBlackMarketAvailable, (int)(infos().blackMarketsNum()), "BlackMarketAvailable_");
            }
            if (isDirty(GameDirtyType.mabNextLaborModule) || bAll)
            {
                SimplifyIO.Data(stream, ref mabNextLaborModule, (int)(infos().modulesNum()), "NextLaborModule_");
            }
            if (isDirty(GameDirtyType.mabWholesaleSlotResets) || bAll)
            {
                SimplifyIO.Data(stream, ref mabWholesaleSlotResets, infos().Globals.NUM_WHOLESALE_SLOTS, "WholesaleSlotResets_");
            }

            if (isDirty(GameDirtyType.maePatentOwner) || bAll)
            {
                SimplifyIO.Data(stream, ref maePatentOwner, (int)(infos().patentsNum()), "PatentOwner_");
            }
            if (isDirty(GameDirtyType.maeWholesaleSlotResource) || bAll)
            {
                SimplifyIO.Data(stream, ref maeWholesaleSlotResource, infos().Globals.NUM_WHOLESALE_SLOTS, "WholesaleSlotResource_");
            }
            if (isDirty(GameDirtyType.maeWholesaleSlotResourceNext) || bAll)
            {
                SimplifyIO.Data(stream, ref maeWholesaleSlotResourceNext, infos().Globals.NUM_WHOLESALE_SLOTS, "WholesaleSlotResourceNext_");
            }
            if (isDirty(GameDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref maeImportResource, "ImportResources");
            }
            if (isDirty(GameDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref maeImportPaymentResource, "ImportPaymentResources");
            }

            if (isDirty(GameDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref maTeamNames, Constants.MAX_NUM_PLAYERS, "TeamNames_");
            }

            if (isDirty(GameDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref maaiBuildingResourceOutput, "BuildingResourceOutput_");
            }

            if (isDirty(GameDirtyType.mEventGameTimeList) || bAll)
            {
                SimplifyIOGame.EventGameData(stream, ref mEventGameTimeList);
            }

            if (isDirty(GameDirtyType.FIRST) || bAll)
            {
                List<TerrainInfo> maTerrainInfos = mMap.getTerrainArray();
                SimplifyIOGame.TerrainData(stream, ref maTerrainInfos);
                mMap.setTerrainArray(maTerrainInfos);
            }

            if (isDirty(GameDirtyType.ColonySettings) || bAll)
            {
                CustomColonySettings.Serialize(stream, ref mColonySettings, compatibilityNumber);
            }

            if (isDirty(GameDirtyType.ShipmentRequest) || bAll)
            {
                ScenarioShipmentRequest.Serialize(stream, ref mShipmentRequest, compatibilityNumber);
            }

            if (bLoadSave || bRebuildingServer)
            {
                List<TileClient> maTiles = mMap.getTileArray();

                GameSettings.Serialize(stream, ref mGameSettings, compatibilityNumber);
                SimplifyIOGame.Data(stream, ref mMarket, this, "Market", bLoadSave, bRebuildingServer, compatibilityNumber);
                SimplifyIOGame.Data(stream, ref mGameEvents, this, "GameEvents", bLoadSave, bRebuildingServer);
                SimplifyIOGame.Data(stream, ref mStats, this, "Stats", bLoadSave, bRebuildingServer);
                SimplifyIOGame.Data(stream, ref mConditionManager, this, "ConditionManager", bLoadSave, bRebuildingServer);
                SimplifyIOGame.Data(stream, ref maPlayers, this, "Players", bLoadSave, bRebuildingServer, compatibilityNumber);
                SimplifyIOGame.Data(stream, ref maTiles, this, "Tiles", bLoadSave, bRebuildingServer, compatibilityNumber);
                SimplifyIOGame.Data(stream, ref mModuleDictionary, this, "ModuleDictionary", bLoadSave, bRebuildingServer);
                SimplifyIOGame.Data(stream, ref mHQDictionary, this, "HQDictionary", bLoadSave, bRebuildingServer);
                SimplifyIOGame.Data(stream, ref mConstructionDictionary, this, "ConstructionDictionary", bLoadSave, bRebuildingServer);
                SimplifyIOGame.Data(stream, ref mBuildingDictionary, this, "BuildingDictionary", bLoadSave, bRebuildingServer, compatibilityNumber);
                SimplifyIOGame.Data(stream, ref mUnitDictionary, this, "UnitDictionary", bLoadSave, bRebuildingServer);

                if (stream is BinaryReader)
                {
                    if (this is GameServer)
                    {
                        ((MapServer)mMap).init((GameServer)this);
                    }
                    else
                    {
                        mMap.initClient(this);
                    }
                }
                mMap.setTileArray(maTiles);

                refreshCachedValues();
            }
        }

        public virtual void writeClientValues(BinaryWriter stream, bool bLoadSave, bool bRebuildingServer, int compatibilityNumber)
        {
            SerializeClient(stream, bLoadSave, bRebuildingServer, compatibilityNumber);
        }

        public virtual void readClientValues(BinaryReader stream, bool bLoadSave, bool bRebuildingServer, int compatibilityNumber)
        {
            mInfos = Globals.Infos;

            SerializeClient(stream, bLoadSave, bRebuildingServer, compatibilityNumber);
        }

        public virtual GameServer rebuildGame(int compatibilityNumber)
        {
            GameServer pGame = Globals.Factory.createGameServer();

            using (MemoryStream mWrite = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(mWrite))
                {
                    writeClientValues(writer, false, true, compatibilityNumber);
                }

                using (MemoryStream mRead = new MemoryStream(mWrite.ToArray()))
                {
                    using (BinaryReader reader = new BinaryReader(mRead))
                    {
                        pGame.readClientValues(reader, false, true, compatibilityNumber);
                    }
                }
            }

            pGame.rebuildFromClient(this);

            pGame.setLastUpdateTimeToNow(); // Fixup tick counter

            return pGame;
        }

        public static HandicapType getHandicapFromSeed(int iSeed)
        {
            HandicapType eHandicap = Globals.Infos.Globals.CHALLENGE_START_HANDICAP;

            eHandicap += (iSeed / 3);

            eHandicap = (HandicapType)(Math.Min((int)eHandicap, (int)(Globals.Infos.Globals.CHALLENGE_HANDICAP)));

            return eHandicap;
        }

        public static LocationType getDailyChallengeLocationFromSeed(int iSeed)
        {
            if ((iSeed % 10) == 1)
            {
                return LocationType.MARS;
            }

            System.Random pRandom = new CrossPlatformRandom(iSeed + 1);

            LocationType[] validLocations = { LocationType.MARS, LocationType.CERES, LocationType.IO, LocationType.EUROPA }; // valid locations to appear on Daily Challenge and Infinite Map Challenge
            return validLocations[pRandom.Next(validLocations.Length)];
        }

        public static LocationType getInfiniteChallengeLocationFromSeed(int iSeed)
        {
            if ((iSeed % 10) == 1)
            {
                return LocationType.MARS;
            }

            System.Random pRandom = new CrossPlatformRandom(iSeed + 1);

            LocationType[] validLocations = { LocationType.MARS, LocationType.CERES, LocationType.IO }; // valid locations to appear on Daily Challenge and Infinite Map Challenge
            return validLocations[pRandom.Next(validLocations.Length)];
        }

        public static int getNumPlayersFromSeed(int iSeed)
        {
            System.Random pRandom = new CrossPlatformRandom(iSeed + 2);
            return ((pRandom.Next(Offworld.GameCore.Constants.MAX_NUM_PLAYERS / 2) + 1) * 2);
        }

        public static MapSizeType getMapSizeFromSeed(int iSeed)
        {
            System.Random pRandom = new CrossPlatformRandom(iSeed + 3);
            return (MapSizeType)pRandom.Next((int)Globals.Infos.mapSizesNum());
        }

        public static LatitudeType getLatitudeFromSeed(int iSeed)
        {
            System.Random pRandom = new CrossPlatformRandom(iSeed + 4);
            return Utils.getRandomLatitude(Globals.Infos, pRandom);
        }

        public static ResourceMinimumType getResourceMinimumFromSeed(int iSeed)
        {
            if (iSeed < 3)
            {
                return Globals.Infos.Globals.DEFAULT_RESOURCEMINIMUM;
            }

            System.Random pRandom = new CrossPlatformRandom(iSeed + 5);
            return (ResourceMinimumType)pRandom.Next((int)Globals.Infos.resourceMinimumsNum());
        }

        public static ResourcePresenceType getResourcePresenceFromSeed(int iSeed)
        {
            if (iSeed < 2)
            {
                return Globals.Infos.Globals.DEFAULT_RESOURCEPRESENCE;
            }

            System.Random pRandom = new CrossPlatformRandom(iSeed + 6);
            return (ResourcePresenceType)pRandom.Next((int)Globals.Infos.resourcePresencesNum());
        }

        public static TerrainClassType getTerrainClassFromSeed(int iSeed, LocationType eLocation)
        {
            System.Random pRandom = new CrossPlatformRandom(iSeed + 7);

            TerrainClassType eBestTerrainClass = TerrainClassType.NONE;
            int iBestValue = 0;

            foreach (InfoTerrainClass pLoopTerrainClass in Globals.Infos.terrainClasses())
            {
                if (!(pLoopTerrainClass.mabLocationInvalid[(int)eLocation]))
                {
                    int iValue = pRandom.Next(1000) + 1 + pLoopTerrainClass.miRollModifier;
                    if (iValue > iBestValue)
                    {
                        eBestTerrainClass = pLoopTerrainClass.meType;
                        iBestValue = iValue;
                    }
                }
            }

            return eBestTerrainClass;
        }

        public static void getInvalidHumanHQsFromSeed(int iSeed, List<bool> abInvalidHumanHQ)
        {
            for (HQType eLoopHQ = 0; eLoopHQ < Globals.Infos.HQsNum(); eLoopHQ++)
            {
                abInvalidHumanHQ.Add(false);
            }

            if (iSeed < 5)
            {
                return;
            }

            System.Random pRandom = new CrossPlatformRandom(iSeed + 8);

            if (pRandom.Next(3) == 0)
            {
                HQType eValidHQ = (HQType)(pRandom.Next((int)(Globals.Infos.HQsNum())));

                for (HQType eLoopHQ = 0; eLoopHQ < Globals.Infos.HQsNum(); eLoopHQ++)
                {
                    if (eLoopHQ != eValidHQ)
                    {
                        abInvalidHumanHQ[(int)eLoopHQ] = true;
                    }
                }
            }
        }

        public static bool isTeamGameFromSeed(int iSeed, int iNumPlayers)
        {
            if (iSeed < 3)
            {
                return false;
            }

            System.Random pRandom = new CrossPlatformRandom(iSeed + 9);

            return ((pRandom.Next(6) == 0) && (iNumPlayers >= 4));
        }

        public static ColonyClassType getColonyClassFromSeed(int iSeed, LocationType eLocation)
        {
            if (iSeed < 10)
            {
                return ColonyClassType.NONE;
            }

            System.Random pRandom = new CrossPlatformRandom(iSeed + 10);

            ColonyClassType eBestColonyClass = ColonyClassType.NONE;
            int iBestValue = 0;

            foreach (InfoColonyClass pLoopColonyClass in Globals.Infos.colonyClasses())
            {
                if (!(pLoopColonyClass.mabLocationInvalid[(int)eLocation]))
                {
                    int iValue = pRandom.Next(1000) + 1;
                    if (iValue > iBestValue)
                    {
                        eBestColonyClass = pLoopColonyClass.meType;
                        iBestValue = iValue;
                    }
                }
            }

            return eBestColonyClass;
        }

        public static ColonyType getColonyTypeFromSeed(int iSeed)
        {
            System.Random pRandom = new CrossPlatformRandom(iSeed + 11);

            ColonyType eBestColony = ColonyType.NONE;
            int iBestValue = 0;

            for (ColonyType eLoopColony = 0; eLoopColony < Globals.Infos.coloniesNum(); eLoopColony++)
            {
                int iValue = pRandom.Next(100) + 1;
                if (iValue > iBestValue)
                {
                    eBestColony = eLoopColony;
                    iBestValue = iValue;
                }
            }

            return eBestColony;
        }

        public void saveMap()
        {
            GameSettings settings = new GameSettings();

            settings.meLocation = getLocation();
            settings.miNumPlayers = (int)getNumPlayers();
            settings.meMapSize = getMapSize();
            settings.miSeed = getSeed();
            settings.meLatitude = getLatitudeFromSeed(getSeed());
            settings.meResourceMinimum = getResourceMinimum();
            settings.meResourcePresence = getResourcePresence();
            settings.meTerrainClass = getTerrainClass();
            settings.meColonyClass = getColonyClass();

            settings.mabInvalidHumanHQ = mabInvalidHQ.ToList();

            bool bTeamGame = isTeamGame();

            settings.meGameSetupType = Globals.Infos.Globals.DEFAULT_GAMESETUP;
            settings.meRulesSetType = Globals.Infos.Globals.DEFAULT_RULESSET;

            settings.mzMap = "";
            settings.mzMapName = "";

            int iWidth = Globals.Infos.mapSize(settings.meMapSize).miWidth;
            int iHeight = Globals.Infos.mapSize(settings.meMapSize).miHeight;

            iWidth *= Mathf.Max(0, (Globals.Infos.terrainClass(settings.meTerrainClass).miSizeModifier + 100));
            iWidth /= 100;

            iHeight *= Mathf.Max(0, (Globals.Infos.terrainClass(settings.meTerrainClass).miSizeModifier + 100));
            iHeight /= 100;

            settings.miWidth = iWidth;
            settings.miHeight = iHeight;
            settings.miEdgeTilePadding = Constants.DEFAULT_EDGE_TILE_PADDING;

            //**************** Fill in game options ********************
            for (int i = 0; i < (int)(Globals.Infos.gameOptionsNum()); i++)
            {
                settings.mabGameOptions.Add(Globals.Infos.gameOption((GameOptionType)i).mbDefaultValueSP);
            }

            //fill in active player teams
            for (int iCount = 0; iCount < settings.miNumPlayers; iCount++)
                settings.playerSlots.Add(new PlayerSettings("", (sbyte)(iCount / (bTeamGame ? 2 : 1)), Globals.Infos.Globals.DEFAULT_HANDICAP, GenderType.MALE, null));

            //fill in the rest of the slots with defaults
            while (settings.playerSlots.Count < Constants.MAX_LOBBY_SLOTS)
                settings.playerSlots.Add(new PlayerSettings("", 0, Globals.Infos.Globals.DEFAULT_HANDICAP, GenderType.MALE, null));

            settings.miNumUniqueTeams = settings.miNumPlayers / (bTeamGame ? 2 : 1);

            GameServer pGame = Globals.Factory.createGameServer();
            pGame.init(Globals.Infos, settings);
            pGame.mapServer().setHasResourceInfo(true);

            DateTime currentTime = DateTime.Now;
            string mapName = "Map_" + Globals.AppInfo.GameMode.ToString() + "_" + currentTime.ToString("yyyyMMddHHmmss");
            pGame.mapServer().writeTerrainXML(Globals.AppInfo.UserMapSavePath + "/" + mapName + ".map", null);
        }

        public static void saveInfiniteChallengeMap(int iSeed)
        {
            GameSettings settings = new GameSettings();

            settings.meLocation = getInfiniteChallengeLocationFromSeed(iSeed);
            settings.miNumPlayers = getNumPlayersFromSeed(iSeed);
            settings.meMapSize = getMapSizeFromSeed(iSeed);
            settings.miSeed = iSeed;
            settings.meLatitude = getLatitudeFromSeed(iSeed);
            settings.meResourceMinimum = getResourceMinimumFromSeed(iSeed);
            settings.meResourcePresence = getResourcePresenceFromSeed(iSeed);
            settings.meTerrainClass = getTerrainClassFromSeed(iSeed, settings.meLocation);
            settings.meColonyClass = getColonyClassFromSeed(iSeed, settings.meLocation);

            getInvalidHumanHQsFromSeed(iSeed, settings.mabInvalidHumanHQ);

            bool bTeamGame = isTeamGameFromSeed(iSeed, settings.miNumPlayers);

            settings.meGameSetupType = Globals.Infos.Globals.DEFAULT_GAMESETUP;
            settings.meRulesSetType = Globals.Infos.Globals.DEFAULT_RULESSET;

            settings.mzMap = "";
            settings.mzMapName = "";

            int iWidth = Globals.Infos.mapSize(settings.meMapSize).miWidth;
            int iHeight = Globals.Infos.mapSize(settings.meMapSize).miHeight;

            iWidth *= Mathf.Max(0, (Globals.Infos.terrainClass(settings.meTerrainClass).miSizeModifier + 100));
            iWidth /= 100;

            iHeight *= Mathf.Max(0, (Globals.Infos.terrainClass(settings.meTerrainClass).miSizeModifier + 100));
            iHeight /= 100;

            settings.miWidth = iWidth;
            settings.miHeight = iHeight;
            settings.miEdgeTilePadding = Constants.DEFAULT_EDGE_TILE_PADDING;

            //**************** Fill in game options ********************
            for (int i = 0; i < (int)(Globals.Infos.gameOptionsNum()); i++)
            {
                settings.mabGameOptions.Add(Globals.Infos.gameOption((GameOptionType)i).mbDefaultValueSP);
            }

            //fill in active player teams
            for (int iCount = 0; iCount < settings.miNumPlayers; iCount++)
                settings.playerSlots.Add(new PlayerSettings("", (sbyte)(iCount / (bTeamGame ? 2 : 1)), Globals.Infos.Globals.DEFAULT_HANDICAP, GenderType.MALE, null));

            //fill in the rest of the slots with defaults
            while (settings.playerSlots.Count < Constants.MAX_LOBBY_SLOTS)
                settings.playerSlots.Add(new PlayerSettings("", 0, Globals.Infos.Globals.DEFAULT_HANDICAP, GenderType.MALE, null));

            settings.miNumUniqueTeams = settings.miNumPlayers / (bTeamGame ? 2 : 1);

            GameServer pGame = Globals.Factory.createGameServer();
            pGame.init(Globals.Infos, settings);
            pGame.mapServer().setHasResourceInfo(true);
            pGame.mapServer().writeTerrainXML("Assets/MarsGame/DLC_Io/Maps/CMap" + ((iSeed < 1000) ? "0" : "") + ((iSeed < 100) ? "0" : "") + ((iSeed < 10) ? "0" : "") + iSeed + ".map", null);
        }

        public virtual float getElapsedGameplayTime()
        {
            return (float)timer.Elapsed.TotalSeconds - startGameTimeUnscaled;
        }

        public virtual int getXWindSpeedModifier()
        {
            int iModifier = 0;

            {
                EventStateType eEventStateGame = getEventStateGameActive();

                if (eEventStateGame != EventStateType.NONE)
                {
                    iModifier += infos().eventState(eEventStateGame).miXWindSpeedModifier;
                }
            }

            {
                EventStateType eEventStateLevel = getEventStateLevel();

                if (eEventStateLevel != EventStateType.NONE)
                {
                    iModifier += infos().eventState(eEventStateLevel).miXWindSpeedModifier;
                }
            }

            return iModifier;
        }

        public virtual int getUnitMovement(UnitType eUnit, HQType eHQ, TileClient pSourceTile, TileClient pTargetTile)
        {
            int iMovement = infos().unit(eUnit).miMovement;

            if (eHQ != HQType.NONE)
            {
                iMovement *= Math.Max(0, infos().HQ(eHQ).miMovementModifier + 100);
                iMovement /= 100;
            }

            if (!(infos().unit(eUnit).mbGroundUnit))
            {
                int iXWindSpeedModifier = getXWindSpeedModifier();
                if (iXWindSpeedModifier != 0)
                {
                    if ((pSourceTile != null) && (pTargetTile != null))
                    {
                        int iXDiff = (pTargetTile.getX() - pSourceTile.getX());
                        if (iXDiff != 0)
                        {
                            int iYDiffAbs = Math.Abs(pTargetTile.getY() - pSourceTile.getY());
                            if (iYDiffAbs > 0)
                            {
                                int iXDiffAbs = Math.Abs(iXDiff);

                                iXWindSpeedModifier *= iXDiffAbs;
                                iXWindSpeedModifier /= (iXDiffAbs + iYDiffAbs);
                            }

                            if (Math.Sign(iXWindSpeedModifier) == Math.Sign(iXDiff))
                            {
                                iMovement *= Math.Max(0, (Math.Abs(iXWindSpeedModifier) + 100));
                                iMovement /= 100;
                            }
                            else
                            {
                                iMovement *= Math.Max(0, (-(Math.Abs(iXWindSpeedModifier)) + 100));
                                iMovement /= 100;
                            }
                        }
                    }
                }
            }

            return iMovement;
        }

        public virtual int getTravelTimeForUnit(UnitType eUnit, HQType eHQ, TileClient pSourceTile, TileClient pTargetTile)
        {
            int denominator = Mathf.Max(getUnitMovement(eUnit, eHQ, pSourceTile, pTargetTile), 1);
            int travelTime = Mathf.CeilToInt((Utils.getDistance2D(pSourceTile.getWorldPosition(), pTargetTile.getWorldPosition()) - Globals.Infos.Globals.UNIT_MISSION_COMPLETE_RANGE) / denominator);
            return travelTime + 1; //Mission Complete check happens before movement, which is adding a turn to the unit's life.
        }

        public static int getBuildingClassLevelBonus(BuildingClassType eBuildingClass, int iNumLevels, LocationType eCampaignLocation, Infos pInfos)
        {
            if (pInfos.buildingClass(eBuildingClass).mabLocationNoPerks[(int)eCampaignLocation])
            {
                return 100;
            }
            else
            {
                return (Math.Max(0, (iNumLevels - 1)) * pInfos.Globals.CAMPAIGN_PERK_BUILDING_BONUS) + pInfos.Globals.CAMPAIGN_PERK_BUILDING_BASE;
            }
        }

        public virtual int totalLevelIncome()
        {
            return (getPopulation() * Globals.Campaign.getIncomePerPopulation(getLevel()));
        }

        public virtual int estimateLevelIncome(PlayerType ePlayer = PlayerType.NONE)
        {
            int iTotalShares = 0;

            foreach (PlayerClient pLoopPlayer in getPlayerClientAll())
            {
                iTotalShares += pLoopPlayer.getColonySharesOwned();
            }

            if (iTotalShares > 0)
            {
                int iValue = totalLevelIncome();

                if (ePlayer != PlayerType.NONE)
                {
                    iValue = (iValue * playerClient(ePlayer).getColonySharesOwned()) / iTotalShares;
                }

                return iValue;
            }
            else
            {
                return 0;
            }
        }

        public virtual int getConnectionModifier(int iConnections)
        {
            if (isNoAdjacency())
            {
                return 100;
            }

            return infos().adjacencyBonus((AdjacencyBonusType)iConnections).miBonusModifier;
        }

        public virtual ResourceLevelType resourceLevelTile(BuildingType eBuilding, ResourceType eResource, TileClient pTile, PlayerType ePlayer)
        {
            ResourceLevelType eResourceLevel = pTile.getResourceLevelAdjacent(eResource, ePlayer);

            if ((eBuilding != BuildingType.NONE) && (ePlayer != PlayerType.NONE))
            {
                ResourceLevelType eMinimumMining = playerClient(ePlayer).getMinimumMining(infos().building(eBuilding).meClass);

                if (eResourceLevel < eMinimumMining)
                {
                    eResourceLevel = eMinimumMining;
                }
            }

            return eResourceLevel;
        }
        public virtual int resourceLevelMining(BuildingType eBuilding, ResourceType eResource, TileClient pTile, PlayerType ePlayer)
        {
            int iMining = infos().building(eBuilding).maiResourceMining[(int)eResource];

            iMining *= (infos().resourceLevel(resourceLevelTile(eBuilding, eResource, pTile, ePlayer)).miRateMultiplier + extraResourcesTile(ePlayer, pTile));
            iMining /= 100;

            return iMining;
        }

        public virtual int resourceMiningTile(BuildingType eBuilding, ResourceType eResource, TileClient pTile, PlayerType ePlayer, int iMining, int iConnections, bool bIgnoreTemp)
        {
            if (iMining == 0)
            {
                return 0;
            }

            if (infos().resource(eResource).mabLocationInvalid[(int)getLocation()])
            {
                return 0;
            }

            if (!bIgnoreTemp)
            {
                if (pTile.isDouble())
                {
                    iMining *= 2;
                }

                if (pTile.isHalf())
                {
                    iMining /= 2;
                }
            }

            TerrainType eTerrain = ePlayer != PlayerType.NONE && playerClient(ePlayer).isCaveMining() && pTile.getTerrain() == infos().Globals.CAVE_TERRAIN ? pTile.getTerrain() : pTile.getTerrainNoIce();

            return resourceMiningLevel(eBuilding, eResource, resourceLevelTile(eBuilding, eResource, pTile, ePlayer), eTerrain, ePlayer, iMining, iConnections, bIgnoreTemp);
        }

        public virtual int resourceMiningLevel(BuildingType eBuilding, ResourceType eResource, ResourceLevelType eResourceLevel, TerrainType eTerrain, PlayerType ePlayer, int iMining, int iConnections, bool bIgnoreTemp)
        {
            if (iMining == 0)
            {
                return 0;
            }

            HandicapType eHandicap = ((ePlayer != PlayerType.NONE) ? playerClient(ePlayer).getHandicap() : HandicapType.NONE);
            int iBuildingClassLevel = ((ePlayer != PlayerType.NONE) && (eBuilding != BuildingType.NONE)) ? playerClient(ePlayer).getBuildingClassLevel(infos().building(eBuilding).meClass) : 0;

            return GameClient.resourceMining(this, infos(), eHandicap, isCampaign(), iBuildingClassLevel, eBuilding, eResource, eResourceLevel, eTerrain, ePlayer, iMining, iConnections, bIgnoreTemp);
        }

        public static int resourceMining(GameClient pGame, Infos pInfos, HandicapType eHandicap, bool bCampaign, int iBuildingClassLevel, BuildingType eBuilding, ResourceType eResource, ResourceLevelType eResourceLevel, TerrainType eTerrain, PlayerType ePlayer, int iMining, int iConnections, bool bIgnoreTemp)
        {
            int iCaveBonus = 0;

            if (pGame != null)
            {
                InfoEventState eventStateGame = pInfos.eventState(pGame.getEventStateGameActive());

                if (eventStateGame != null && eBuilding != BuildingType.NONE && (eventStateGame.meAffectedTerrain == TerrainType.NONE || eventStateGame.meAffectedTerrain == eTerrain))
                    iMining += eventStateGame.maaiBuildingProductionChange[(int)pInfos.building(eBuilding).meClass][(int)eResource];

                iCaveBonus = pGame.extraResourcesTerrain(ePlayer, eTerrain);
            }

            if ((iMining == 0) || (eResourceLevel == ResourceLevelType.NONE && iCaveBonus == 0))
            {
                return 0;
            }

            iMining *= (pInfos.resourceLevel(eResourceLevel).miRateMultiplier + iCaveBonus);
            iMining /= 100;

            if (pGame != null)
            {
                {
                    int iBonus = pGame.getConnectionModifier(iConnections);

                    if ((eBuilding != BuildingType.NONE) && (eTerrain != TerrainType.NONE))
                    {
                        iBonus += pInfos.building(eBuilding).maiTerrainProductionModifier[(int)eTerrain];
                    }

                    iMining *= Math.Max(0, iBonus);
                    iMining /= 100;
                }

                if (eBuilding != BuildingType.NONE)
                {
                    if (bIgnoreTemp)
                    {
                        iMining *= Math.Max(0, (pGame.getBuildingAverageModifier(eBuilding) + 100));
                        iMining /= 100;
                    }
                    else
                    {
                        {
                            InfoEventState eventStateGame = pInfos.eventState(pGame.getEventStateGameActive());

                            if (eventStateGame != null && (eventStateGame.meAffectedTerrain == TerrainType.NONE || eventStateGame.meAffectedTerrain == eTerrain))
                            {
                                iMining *= Math.Max(0, (eventStateGame.miProductionModifier + eventStateGame.maiBuildingClassModifier[(int)pInfos.building(eBuilding).meClass] + 100));
                                iMining /= 100;
                            }
                        }

                        {
                            InfoEventState eventStateLevel = pInfos.eventState(pGame.getEventStateLevel());

                            if (eventStateLevel != null && (eventStateLevel.meAffectedTerrain == TerrainType.NONE || eventStateLevel.meAffectedTerrain == eTerrain))
                            {
                                iMining *= Math.Max(0, (eventStateLevel.miProductionModifier + eventStateLevel.maiBuildingClassModifier[(int)pInfos.building(eBuilding).meClass] + 100));
                                iMining /= 100;
                            }
                        }
                    }
                }

                if (ePlayer != PlayerType.NONE)
                {
                    iMining *= Math.Max(0, (pGame.playerClient(ePlayer).getResourceProductionModifier(eResource) + 100));
                    iMining /= 100;

                    if (!(pGame.playerClient(ePlayer).isHuman()))
                    {
                        iMining *= Math.Max(0, (pInfos.rulesSet(pGame.getRulesSet()).miAIProductionModifier + 100));
                        iMining /= 100;

                        iMining *= Math.Max(0, (pInfos.handicap(pGame.getHumanHandicap()).miAIProductionModifier + 100));
                        iMining /= 100;
                    }
                }
            }

            if (bCampaign)
            {
                if (eBuilding != BuildingType.NONE)
                {
                    BuildingClassType eBuildingClass = pInfos.building(eBuilding).meClass;

                    if (pInfos.buildingClass(eBuildingClass).meOrderType == OrderType.NONE)
                    {
                        iMining *= getBuildingClassLevelBonus(eBuildingClass, iBuildingClassLevel, Globals.Campaign.getLocation(), pInfos);
                        iMining /= 100;
                    }
                }
            }

            if (eHandicap != HandicapType.NONE)
            {
                iMining *= Math.Max(0, pInfos.handicap(eHandicap).miProductionModifier + 100);
                iMining /= 100;
            }

            return Math.Max(0, iMining);
        }
        public virtual int extraResourcesTile(PlayerType ePlayer, TileClient pTile)
        {
            if (ePlayer != PlayerType.NONE && playerClient(ePlayer).isCaveMining() && pTile.getTerrain() == infos().Globals.CAVE_TERRAIN)
                return 50;
            return 0;
        }
        public virtual int extraResourcesTerrain(PlayerType ePlayer, TerrainType eTerrain)
        {
            if (ePlayer != PlayerType.NONE && playerClient(ePlayer).isCaveMining() && eTerrain == infos().Globals.CAVE_TERRAIN)
                return 50;
            return 0;
        }

        public virtual int resourceInput(BuildingType eBuilding, ResourceType eResource, TileClient pTile, bool bCampaign, int iInputModifier, int iBuildingClassLevel, bool bIgnoreTemp)
        {
            //using (new UnityProfileScope("Game::resourceInput")) //Called too many times per frame
            {
                int iInput = infos().building(eBuilding).maiResourceInput[(int)eResource];

                if (iInput == 0)
                {
                    return 0;
                }

                if (!bIgnoreTemp)
                {
                    if (pTile != null)
                    {
                        if (pTile.isDouble())
                        {
                            iInput *= 2;
                        }

                        if (pTile.isHalf())
                        {
                            iInput /= 2;
                        }
                    }
                }

                if (iInputModifier != 0)
                {
                    iInput *= Math.Max(0, (iInputModifier + 100));
                    iInput /= 100;
                }

                if (bCampaign)
                {
                    BuildingClassType eBuildingClass = infos().building(eBuilding).meClass;

                    if (infos().buildingClass(eBuildingClass).meOrderType == OrderType.NONE)
                    {
                        iInput *= getBuildingClassLevelBonus(eBuildingClass, iBuildingClassLevel, Globals.Campaign.getLocation(), infos());
                        iInput /= 100;
                    }
                }

                return Math.Max(0, iInput);
            }
        }

        public virtual int resourceInput(BuildingType eBuilding, ResourceType eResource, TileClient pTile, PlayerType ePlayer, bool bIgnoreTemp)
        {
            if (ePlayer != PlayerType.NONE)
            {
                if (!buildingWantsInput(eBuilding, eResource, pTile, playerClient(ePlayer)))
                {
                    return 0;
                }
            }

            PlayerClient pPlayer = ((ePlayer != PlayerType.NONE) ? playerClient(ePlayer) : null);

            BuildingClassType eBuildingClass = infos().building(eBuilding).meClass;

            int iInputModifier = ((pPlayer != null) ? pPlayer.getBuildingClassInputModifier(eBuildingClass) : 0);
            int iClassLevel = ((pPlayer != null) ? pPlayer.getBuildingClassLevel(eBuildingClass) : 0);

            return resourceInput(eBuilding, eResource, pTile, isCampaign(), iInputModifier, iClassLevel, bIgnoreTemp);
        }

        public static int resourceOutputTile(GameClient pGame, Infos pInfos, BuildingType eBuilding, ResourceType eResource, TileClient pTile)
        {
            if (pTile != null)
            {
                if (pInfos.building(eBuilding).mbIce)
                {
                    bool bFoundIce = false;

                    for (IceType eLoopIce = 0; eLoopIce < pInfos.icesNum(); eLoopIce++)
                    {
                        if (pInfos.ice(eLoopIce).maiAverageResourceRate[(int)eResource] > 0)
                        {
                            if (pTile.getIce() == eLoopIce)
                            {
                                bFoundIce = true;
                                break;
                            }
                        }
                    }

                    if (!bFoundIce)
                    {
                        return 0;
                    }
                }
            }

            return Utils.getBuildingOutput(eBuilding, eResource, pGame);
        }

        public virtual int resourceOutputTile(BuildingType eBuilding, ResourceType eResource, TileClient pTile)
        {
            return GameClient.resourceOutputTile(this, infos(), eBuilding, eResource, pTile);
        }

        public static int resourceOutput(GameClient pGame, Infos pInfos, HandicapType eHandicap, bool bCampaign, int iBuildingClassLevel, BuildingType eBuilding, ResourceType eResource, TileClient pTile, PlayerType ePlayer, int iConnections, bool bIgnoreTemp)
        {
            //using (new UnityProfileScope("Game::resourceOutput")) //Called too many times per frame
            {
                int iOutput = resourceOutputTile(pGame, pInfos, eBuilding, eResource, pTile);

                if (pGame != null)
                {
                    InfoEventState eventStateGame = pInfos.eventState(pGame.getEventStateGameActive());

                    if (eventStateGame != null && (eventStateGame.meAffectedTerrain == TerrainType.NONE || (pTile != null && eventStateGame.meAffectedTerrain == pTile.getTerrain())))
                        iOutput += eventStateGame.maaiBuildingProductionChange[(int)pInfos.building(eBuilding).meClass][(int)eResource];
                }

                if (iOutput > 0)
                {
                    if (pGame != null)
                    {
                        iOutput += pInfos.building(eBuilding).maiOutputLocationChange[(int)pGame.getLocation()];
                    }

                    if (pTile != null)
                    {
                        iOutput += pInfos.building(eBuilding).maiOutputHeightChange[(int)pTile.getHeight()];

                        if (pTile.getWind() != WindType.NONE)
                        {
                            iOutput += pInfos.building(eBuilding).maiOutputWindChange[(int)pTile.getWind()];
                        }
                    }
                }

                if (eResource == pInfos.Globals.ENERGY_RESOURCE && pTile != null && ePlayer != PlayerType.NONE)
                {
                    if (pGame.playerClient(ePlayer).isBorehole() && pTile.onOrAdjacentToGeothermal())
                    {
                        iOutput += pInfos.Globals.BOREHOLE_ENERGY_RATE;
                    }

                    if (iOutput > 0)
                    {
                        int iModifier = pGame.playerClient(ePlayer).getConnectedHQPowerProductionModifier();
                        if (iModifier != 0)
                        {
                            if (pTile.isPotentialHQConnection(ePlayer))
                            {
                                iOutput *= Math.Max(0, iModifier + 100);
                                iOutput /= 100;
                            }
                        }
                    }
                }

                if (!bIgnoreTemp)
                {
                    if (pTile != null)
                    {
                        if (pTile.isDouble())
                        {
                            iOutput *= 2;
                        }

                        if (pTile.isHalf())
                        {
                            iOutput /= 2;
                        }

                        if (pTile.isOverload() && (eResource == pInfos.Globals.ENERGY_RESOURCE))
                        {
                            iOutput = 0;
                        }
                    }
                }

                if (iOutput == 0)
                {
                    return 0;
                }

                if (pGame != null)
                {
                    {
                        int iBonus = pGame.getConnectionModifier(iConnections);

                        if (pTile != null)
                        {
                            if (pTile.getTerrainNoIce() != TerrainType.NONE)
                            {
                                iBonus += pInfos.building(eBuilding).maiTerrainProductionModifier[(int)(pTile.getTerrainNoIce())];
                            }
                        }

                        iOutput *= Math.Max(0, iBonus);
                        iOutput /= 100;
                    }

                    if (bIgnoreTemp)
                    {
                        iOutput *= Math.Max(0, (pGame.getBuildingAverageModifier(eBuilding) + 100));
                        iOutput /= 100;
                    }
                    else
                    {
                        {
                            InfoEventState eventStateGame = pInfos.eventState(pGame.getEventStateGameActive());

                            if (eventStateGame != null && (eventStateGame.meAffectedTerrain == TerrainType.NONE || (pTile != null && eventStateGame.meAffectedTerrain == pTile.getTerrain())))
                            {
                                iOutput *= Math.Max(0, (eventStateGame.miProductionModifier + eventStateGame.maiBuildingClassModifier[(int)pInfos.building(eBuilding).meClass] + 100));
                                iOutput /= 100;
                            }
                        }

                        {
                            InfoEventState eventStateLevel = pInfos.eventState(pGame.getEventStateLevel());

                            if (eventStateLevel != null && (eventStateLevel.meAffectedTerrain == TerrainType.NONE || (pTile != null && eventStateLevel.meAffectedTerrain == pTile.getTerrain())))
                            {
                                iOutput *= Math.Max(0, (eventStateLevel.miProductionModifier + eventStateLevel.maiBuildingClassModifier[(int)pInfos.building(eBuilding).meClass] + 100));
                                iOutput /= 100;
                            }
                        }
                    }

                    if (ePlayer != PlayerType.NONE)
                    {
                        // research
                        iOutput *= Math.Max(0, pGame.playerClient(ePlayer).getResourceProductionModifier(eResource) + 100);
                        iOutput /= 100;

                        if (!(pGame.playerClient(ePlayer).isHuman()))
                        {
                            iOutput *= Math.Max(0, pInfos.rulesSet(pGame.getRulesSet()).miAIProductionModifier + 100);
                            iOutput /= 100;

                            iOutput *= Math.Max(0, pInfos.handicap(pGame.getHumanHandicap()).miAIProductionModifier + 100);
                            iOutput /= 100;
                        }
                    }
                }

                if (eHandicap != HandicapType.NONE)
                {
                    iOutput *= Math.Max(0, pInfos.handicap(eHandicap).miProductionModifier + 100);
                    iOutput /= 100;
                }

                if (bCampaign)
                {
                    BuildingClassType eBuildingClass = pInfos.building(eBuilding).meClass;

                    if (pInfos.buildingClass(eBuildingClass).meOrderType == OrderType.NONE)
                    {
                        iOutput *= getBuildingClassLevelBonus(eBuildingClass, iBuildingClassLevel, Globals.Campaign.getLocation(), pInfos);
                        iOutput /= 100;
                    }
                }

                return Math.Max(0, iOutput);
            }
        }

        public virtual int resourceOutput(BuildingType eBuilding, ResourceType eResource, TileClient pTile, PlayerType ePlayer, int iConnections, bool bIgnoreTemp)
        {
            HandicapType eHandicap = ((ePlayer != PlayerType.NONE) ? playerClient(ePlayer).getHandicap() : HandicapType.NONE);
            int iBuildingClassLevel = ((ePlayer != PlayerType.NONE) ? playerClient(ePlayer).getBuildingClassLevel(infos().building(eBuilding).meClass) : 0);

            return GameServer.resourceOutput(this, infos(), eHandicap, isCampaign(), iBuildingClassLevel, eBuilding, eResource, pTile, ePlayer, iConnections, bIgnoreTemp);
        }

        public virtual int powerConsumption(BuildingType eBuilding, TileClient pTile, PlayerType ePlayer)
        {
            int iPower = infos().building(eBuilding).miPowerConsumption;
            if (iPower > 0)
            {
                if (pTile != null)
                {
                    if (ePlayer != PlayerType.NONE)
                    {
                        if (playerClient(ePlayer).isBorehole())
                        {
                            if (pTile.onOrAdjacentToGeothermal())
                            {
                                return 0;
                            }
                        }
                    }
                }

                if (pTile != null)
                {
                    if (pTile.isOverload())
                    {
                        iPower *= 2;
                    }
                }

                if (ePlayer != PlayerType.NONE)
                {
                    iPower *= Math.Max(0, playerClient(ePlayer).getPowerConsumptionModifier() + 100);
                    iPower /= 100;

                    if (playerClient(ePlayer).isHuman())
                    {
                        iPower *= Math.Max(0, infos().rulesSet(getRulesSet()).miPowerConsumptionModifier + 100);
                        iPower /= 100;
                    }
                    else
                    {
                        iPower *= Math.Max(0, infos().handicap(getHumanHandicap()).miAIPowerConsumptionModifier + 100);
                        iPower /= 100;
                    }

                    if (isCampaign())
                    {
                        BuildingClassType eBuildingClass = infos().building(eBuilding).meClass;

                        if (infos().buildingClass(infos().building(eBuilding).meClass).meOrderType == OrderType.NONE)
                        {
                            iPower *= getBuildingClassLevelBonus(eBuildingClass, playerClient(ePlayer).getBuildingClassLevel(eBuildingClass), Globals.Campaign.getLocation(), infos());
                            iPower /= 100;
                        }
                    }
                }

                return Math.Max(0, iPower);
            }
            else
            {
                return 0;
            }
        }

        public virtual int calculateRate(BuildingType eBuilding, ResourceType eResource, TileClient pTile, PlayerType ePlayer, int iConnections, bool bIgnoreTemp, int iPowerMultiplier)
        {
            PlayerClient pPlayer = ((ePlayer != PlayerType.NONE) ? playerClient(ePlayer) : null);

            int iRate = 0;

            if (pTile != null)
            {
                iRate += resourceMiningTile(eBuilding, eResource, pTile, ePlayer, infos().building(eBuilding).maiResourceMining[(int)eResource], iConnections, bIgnoreTemp);
            }
            else
            {
                iRate += resourceMiningLevel(eBuilding, eResource, getHighestResourceLevel(), TerrainType.NONE, ePlayer, infos().building(eBuilding).maiResourceMining[(int)eResource], iConnections, bIgnoreTemp);
            }

            iRate += resourceOutput(eBuilding, eResource, pTile, ePlayer, iConnections, bIgnoreTemp);

            iRate -= resourceInput(eBuilding, eResource, pTile, ePlayer, bIgnoreTemp);

            if (iPowerMultiplier > 0)
            {
                if ((pPlayer != null) ? (pPlayer.getEnergyResource() == eResource) : (infos().Globals.ENERGY_RESOURCE == eResource))
                {
                    int iPower = powerConsumption(eBuilding, pTile, ePlayer);
                    if (iPower > 0)
                    {
                        if (iPowerMultiplier != 100)
                        {
                            iPower *= iPowerMultiplier;
                            iPower /= 100;
                        }

                        iRate -= iPower;
                    }
                }
            }

            return iRate;
        }

        public virtual int shippingCapacity(ResourceType eResource)
        {
            int iCapacity = infos().resource(eResource).miShipment;

            {
                EventStateType eEventStateGame = getEventStateGameActive();

                if (eEventStateGame != EventStateType.NONE)
                {
                    iCapacity *= Math.Max(0, infos().eventState(eEventStateGame).miShippingCapacityModifier + 100);
                    iCapacity /= 100;
                }
            }

            {
                EventStateType eEventStateLevel = getEventStateLevel();

                if (eEventStateLevel != EventStateType.NONE)
                {
                    iCapacity *= Math.Max(0, infos().eventState(eEventStateLevel).miShippingCapacityModifier + 100);
                    iCapacity /= 100;
                }
            }

            return iCapacity;
        }

        public virtual int calculateGasUsage(ResourceType eResource, int iRate, TileClient pTile, PlayerClient pPlayer, int iGasMultiplier)
        {
            if (pTile != null)
            {
                if (!pPlayer.isTeleportation() && !pTile.isPotentialHQConnection(pPlayer.getPlayer()))
                {
                    int iRateAbs = Math.Abs(iRate);

                    if ((iRateAbs > 0) && infos().resource(eResource).mbTrade)
                    {
                        HQClient pHQ = pPlayer.findClosestHQClient(pTile);
                        if (pHQ != null)
                        {
                            int iGasRate = pPlayer.getGas(infos().Globals.SHIP_UNIT);
                            if (iGasRate < 0)
                            {
                                if (iGasMultiplier != 100)
                                {
                                    iGasRate *= iGasMultiplier;
                                    iGasRate /= 100;
                                }

                                int iUsage = getTravelTimeForUnit(infos().Globals.SHIP_UNIT, pPlayer.getHQ(), pTile, pHQ.tileClient());
                                iUsage = (iUsage * iGasRate);
                                iUsage = (iUsage * iRateAbs) / (shippingCapacity(eResource) * Constants.RESOURCE_MULTIPLIER);
                                return iUsage;
                            }
                        }
                    }
                }
            }

            return 0;
        }

        public virtual int calculateTotalGasUsage(BuildingType pBuilding, TileClient pTile, PlayerType ePlayer, int iNumConnections, bool bIgnoreTemp)
        {
            PlayerClient pPlayer = playerClient(ePlayer);

            int iUsage = 0;

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                iUsage += calculateGasUsage(eLoopResource, calculateRate(pBuilding, eLoopResource, pTile, ePlayer, iNumConnections, bIgnoreTemp, 100), pTile, pPlayer, 100);
            }

            return iUsage;
        }

        public virtual int calculateGasExpense(ResourceType eResource, int iRate, TileClient pTile, PlayerClient pPlayer, int iGasMultiplier)
        {
            return (calculateGasUsage(eResource, iRate, pTile, pPlayer, iGasMultiplier) * marketClient().getWholePrice(pPlayer.getGasResource())) / Constants.RESOURCE_MULTIPLIER;
        }

        public int calculateTotalGasExpense(BuildingType eBuilding, TileClient pTile, PlayerType ePlayer, int iConnections, bool bIgnoreTemp)
        {
            using (new UnityProfileScope("Game::calculateTotalGasExpense"))
            {
                PlayerClient pPlayer = playerClient(ePlayer);

                int iExpense = 0;

                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    iExpense += calculateGasExpense(eLoopResource, calculateRate(eBuilding, eLoopResource, pTile, ePlayer, iConnections, bIgnoreTemp, 0), pTile, pPlayer, 100);
                }

                return iExpense;
            }
        }

        public virtual int calculateRevenue(BuildingType eBuilding, TileClient pTile, PlayerType ePlayer, int iConnections, bool bIgnoreTemp, bool bNew, int iPowerMultiplier, int iGasMultiplier, int iNoTradeMultiplier, bool bNoExpenses)
        {
            using (new UnityProfileScope("Game::calculateRevenue"))
            {
                PlayerClient pPlayer = ((ePlayer != PlayerType.NONE) ? playerClient(ePlayer) : null);

                int iRevenue = entertainmentProfit(eBuilding, pTile, ePlayer, bIgnoreTemp, bNew);
                int iExpense = 0;

                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    int iRate = calculateRate(eBuilding, eLoopResource, pTile, ePlayer, iConnections, bIgnoreTemp, iPowerMultiplier);

                    if (iRate > 0)
                    {
                        int iTemp = ((iRate * marketClient().getWholePrice(eLoopResource)) / Constants.RESOURCE_MULTIPLIER);

                        if (!(infos().resource(eLoopResource).mbTrade))
                        {
                            if (iNoTradeMultiplier != 100)
                            {
                                iTemp *= iNoTradeMultiplier;
                                iTemp /= 100;
                            }
                        }

                        iRevenue += iTemp;
                    }

                    if (!bNoExpenses)
                    {
                        if (iRate < 0)
                        {
                            iExpense += ((iRate * marketClient().getWholePrice(eLoopResource)) / Constants.RESOURCE_MULTIPLIER);
                        }

                        if ((pPlayer != null) && (iGasMultiplier > 0))
                        {
                            iExpense += calculateGasExpense(eLoopResource, calculateRate(eBuilding, eLoopResource, pTile, ePlayer, iConnections, bIgnoreTemp, 0), pTile, pPlayer, iGasMultiplier);
                        }
                    }
                }

                return iRevenue + iExpense;
            }
        }

        public virtual int entertainmentProfit(BuildingType eBuilding, TileClient pTile, PlayerType ePlayer, bool bIgnoreTemp, bool bNew)
        {
            int iEntertainment = ((eBuilding == BuildingType.NONE) ? 1 : infos().building(eBuilding).miEntertainment);
            if (iEntertainment > 0)
            {
                int iProfit = (getEntertainmentDemand() + 1);

                iProfit *= infos().Globals.ENTERTAINMENT_PROFIT;
                iProfit /= 100;

                iProfit *= (iEntertainment + 1);
                iProfit /= (getEntertainmentSupply() + 1) + ((bNew) ? iEntertainment : 0);

                if (isCampaign())
                {
                    iProfit *= 5;
                    iProfit /= 4;
                }

                if (pTile != null)
                {
                    int iModifier = getTileEntertainmentModifier(pTile);
                    if (iModifier != 0)
                    {
                        iProfit *= Math.Max(0, (iModifier + 100));
                        iProfit /= 100;
                    }
                }

                if (!bIgnoreTemp)
                {
                    if (pTile != null)
                    {
                        if (pTile.isDouble())
                        {
                            iProfit *= 2;
                        }

                        if (pTile.isHalf())
                        {
                            iProfit /= 2;
                        }
                    }
                }

                if (eBuilding != BuildingType.NONE)
                {
                    iProfit *= Math.Max(0, (infos().building(eBuilding).miEntertainmentModifier + 100));
                    iProfit /= 100;

                    if (bIgnoreTemp)
                    {
                        iProfit *= Math.Max(0, (getBuildingAverageModifier(eBuilding) + 100));
                        iProfit /= 100;
                    }
                    else
                    {
                        {
                            EventStateType eEventStateGame = getEventStateGameActive();

                            if (eEventStateGame != EventStateType.NONE)
                            {
                                iProfit *= Math.Max(0, (infos().eventState(eEventStateGame).miProductionModifier + infos().eventState(eEventStateGame).maiBuildingClassModifier[(int)infos().building(eBuilding).meClass] + 100));
                                iProfit /= 100;
                            }
                        }

                        {
                            EventStateType eEventStateLevel = getEventStateLevel();

                            if (eEventStateLevel != EventStateType.NONE)
                            {
                                iProfit *= Math.Max(0, (infos().eventState(eEventStateLevel).miProductionModifier + infos().eventState(eEventStateLevel).maiBuildingClassModifier[(int)infos().building(eBuilding).meClass] + 100));
                                iProfit /= 100;
                            }
                        }
                    }
                }

                if (ePlayer != PlayerType.NONE)
                {
                    iProfit *= Math.Max(0, (playerClient(ePlayer).getEntertainmentModifier() + 100));
                    iProfit /= 100;

                    if (eBuilding != BuildingType.NONE)
                    {
                        if (isCampaign())
                        {
                            BuildingClassType eBuildingClass = infos().building(eBuilding).meClass;

                            if (infos().buildingClass(eBuildingClass).meOrderType == OrderType.NONE)
                            {
                                iProfit *= getBuildingClassLevelBonus(eBuildingClass, playerClient(ePlayer).getBuildingClassLevel(eBuildingClass), Globals.Campaign.getLocation(), infos());
                                iProfit /= 100;
                            }
                        }
                    }
                }

                {
                    ColonyClassType eColonyClass = getColonyClass();

                    if (eColonyClass != ColonyClassType.NONE)
                    {
                        iProfit *= Math.Max(0, (infos().colonyClass(eColonyClass).miEntertainmentModifier + 100));
                        iProfit /= 100;
                    }
                }

                return Math.Max(0, iProfit);
            }
            else
            {
                return 0;
            }
        }

        public virtual int getModuleEntertainmentModifier(ModuleType eModule)
        {
            int iModifier = infos().module(eModule).miEntertainmentModifier;

            {
                ColonyClassType eColonyClass = getColonyClass();

                if (eColonyClass != ColonyClassType.NONE)
                {
                    iModifier *= Math.Max(0, (infos().colonyClass(eColonyClass).miModuleBonusModifier + 100));
                    iModifier /= 100;
                }
            }

            return iModifier;
        }
        public virtual int getTileEntertainmentModifier(TileClient pTile)
        {
            int iModifier = 0;

            for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
            {
                TileClient pAdjacentTile = mapClient().tileClientAdjacent(pTile, eLoopDirection);

                if (pAdjacentTile != null)
                {
                    if (pAdjacentTile.isModule())
                    {
                        ModuleClient pModule = pAdjacentTile.moduleClient();

                        if (pModule.isOccupied())
                        {
                            iModifier += getModuleEntertainmentModifier(pModule.getType());
                        }
                    }
                }
            }

            return iModifier;
        }

        public virtual int getModuleOrderModifier(ModuleType eModule, OrderType eOrder)
        {
            int iModifier = infos().module(eModule).maiOrderModifier[(int)eOrder];

            {
                ColonyClassType eColonyClass = getColonyClass();

                if (eColonyClass != ColonyClassType.NONE)
                {
                    iModifier *= Math.Max(0, (infos().colonyClass(eColonyClass).miModuleBonusModifier + 100));
                    iModifier /= 100;
                }
            }

            return iModifier;
        }
        public virtual int getTileOrderRateModifier(OrderType eOrder, TileClient pTile)
        {
            using (new UnityProfileScope("Game::getTileOrderRateModifier"))
            {
                int iModifier = 0;

                for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                {
                    TileClient pAdjacentTile = mapClient().tileClientAdjacent(pTile, eLoopDirection);

                    if (pAdjacentTile != null)
                    {
                        if (pAdjacentTile.isModule())
                        {
                            ModuleClient pModule = pAdjacentTile.moduleClient();

                            if (pModule.isOccupied())
                            {
                                iModifier += getModuleOrderModifier(pModule.getType(), eOrder);
                            }
                        }
                    }
                }

                return iModifier;
            }
        }

        public virtual bool canTerrainHaveBuilding(TerrainType eTerrain, BuildingType eBuilding, HelpInfo pHelp = null)
        {
            InfoBuilding buildingInfo = infos().building(eBuilding);

            if (buildingInfo.meTerrainRate != TerrainType.NONE)
            {
                if (buildingInfo.meTerrainRate != eTerrain)
                {
                    if (pHelp != null)
                    {
                        pHelp.strHelp = TEXT("TEXT_PLAYER_CONSTRUCT_REQUIRES_TERRAIN", infos().terrain(buildingInfo.meTerrainRate).meName.ToText());
                    }
                    return false;
                }
            }
            else if (infos().terrain(eTerrain).mbRequiredOnly)
            {
                if (!(buildingInfo.mabTerrainValid[(int)eTerrain]))
                {
                    if (pHelp != null)
                    {
                        pHelp.strHelp = TEXT("TEXT_PLAYER_CONSTRUCT_INVALID_TERRAIN", infos().terrain(eTerrain).meName.ToText(), buildingInfo.meName.ToText());
                    }
                    return false;
                }
            }

            return true;
        }

        public virtual bool canTileHaveBuilding(TileClient pTile, BuildingType eBuilding, PlayerType ePlayer, HelpInfo pHelp = null)
        {
            //using (new UnityProfileScope("Player::canTileHaveBuilding"))
            {
                if (isBuildingUnavailable(eBuilding))
                {
                    return false;
                }

                InfoBuilding buildingInfo = infos().building(eBuilding);

                if (pTile.isGeothermal() != buildingInfo.mbGeothermal)
                {
                    if (buildingInfo.mbGeothermal || (ePlayer == PlayerType.NONE) || !(playerClient(ePlayer).isBorehole()))
                    {
                        if (pHelp != null)
                        {
                            if (getLocation() == LocationType.EUROPA)
                            {
                                if (buildingInfo.mbGeothermal)
                                {
                                    pHelp.strHelp = TEXT("TEXT_PLAYER_CONSTRUCT_HYDROTHERMAL");
                                }
                                else
                                {
                                    pHelp.strHelp = TEXT("TEXT_PLAYER_CONSTRUCT_NO_HYDROTHERMAL");
                                }
                            }
                            else
                            {
                                if (buildingInfo.mbGeothermal)
                                {
                                    pHelp.strHelp = TEXT("TEXT_PLAYER_CONSTRUCT_GEOTHERMAL");
                                }
                                else
                                {
                                    pHelp.strHelp = TEXT("TEXT_PLAYER_CONSTRUCT_NO_GEOTHERMAL");
                                }
                            }
                        }
                        return false;
                    }
                }

                if (buildingInfo.mbRequiresModuleOrHQ)
                {
                    if (ePlayer == PlayerType.NONE)
                    {
                        if (!(pTile.adjacentToModule()))
                        {
                            if (pHelp != null)
                            {
                                pHelp.strHelp = TEXT("TEXT_PLAYER_CONSTRUCT_CONNECTED_HQ_MODULE");
                            }
                            return false;
                        }
                    }
                    else
                    {
                        if (!(pTile.isPotentialHQConnection(ePlayer)) && !(pTile.adjacentToModule()))
                        {
                            if (pHelp != null)
                            {
                                pHelp.strHelp = TEXT("TEXT_PLAYER_CONSTRUCT_CONNECTED_HQ_MODULE");
                            }
                            return false;
                        }
                    }
                }

                if (!canTerrainHaveBuilding(pTile.getTerrain(), eBuilding, pHelp))
                {
                    return false;
                }

                if (infos().building(eBuilding).mbIce)
                {
                    if (pTile.getIce() == IceType.NONE)
                    {
                        if (pHelp != null)
                        {
                            pHelp.strHelp = TEXT("TEXT_PLAYER_NEEDS_ICE");
                        }
                        return false;
                    }
                }

                {
                    bool bFound = false;
                    bool isMiningBuilding = false;
                    List<string> astr = new List<string>();

                    for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                    {
                        int iMining = buildingInfo.maiResourceMining[(int)eLoopResource];
                        if (iMining > 0)
                        {
                            if (isResourceValid(eLoopResource))
                            {
                                isMiningBuilding = true;

                                if (resourceLevelTile(eBuilding, eLoopResource, pTile, ePlayer) > ResourceLevelType.NONE || extraResourcesTile(ePlayer, pTile) > 0)
                                {
                                    bFound = true;
                                    break;
                                }
                                else if (pHelp != null)
                                {
                                    astr.Add(TEXT(infos().resource(eLoopResource).meName));
                                }
                            }
                        }
                    }

                    if (!bFound && isMiningBuilding)
                    {
                        if (pHelp != null)
                            pHelp.strHelp = TEXT("TEXT_PLAYER_CONSTRUCT_RESOURCE", Utils.buildCommaOrList(astr).ToText());
                        return false;
                    }
                }

                return true;
            }
        }

        public virtual bool buildingWantsInput(BuildingType eBuilding, ResourceType eResource, TileClient pTile, PlayerClient pPlayer)
        {
            if (infos().building(eBuilding).maiResourceInput[(int)eResource] > 0)
            {
                return !(pPlayer.isBuildingIgnoresInput(eBuilding, eResource, pTile));
            }
            else
            {
                return false;
            }
        }

        public virtual bool buildingWantsAnyInput(BuildingType eBuilding, TileClient pTile, PlayerClient pPlayer)
        {
            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                if (buildingWantsInput(eBuilding, eLoopResource, pTile, pPlayer))
                {
                    return true;
                }
            }

            return false;
        }

        public static int getModuleSupply(ModuleType eModule, ResourceType eResource, Infos pInfos, GameClient pGame, LocationType eLocation, bool bCampaign)
        {
            if (pInfos.resource(eResource).mabLocationInvalid[(int)eLocation])
            {
                return 0;
            }

            int iSupply = pInfos.module(eModule).maiResourceSupply[(int)eResource];

            if (pInfos.module(eModule).mbLabor)
            {
                if (pGame != null)
                {
                    ColonyClassType eColonyClass = pGame.getColonyClass();

                    if (eColonyClass != ColonyClassType.NONE)
                    {
                        iSupply += pInfos.colonyClass(eColonyClass).maiResourceLaborSupply[(int)eResource];
                    }
                }
            }

            if (bCampaign)
            {
                if (pInfos.module(eModule).mbPopulation)
                {
                    iSupply /= 2;
                }
            }

            return iSupply;
        }

        public static bool canHaveModule(ModuleType eModule, LocationType eLocation, ColonyType eColony, ColonyClassType eColonyClass, Infos pInfos)
        {
            if (eColonyClass != ColonyClassType.NONE)
            {
                if (pInfos.colonyClass(eColonyClass).mabModuleInvalid[(int)eModule])
                {
                    return false;
                }

                if (pInfos.colonyClass(eColonyClass).mabModuleValid[(int)eModule])
                {
                    return true;
                }
            }

            return (pInfos.module(eModule).mabColonyValid[(int)eColony] || pInfos.module(eModule).mabLocationValid[(int)eLocation] || pInfos.module(eModule).maabColonyLocationValid[(int)eColony][(int)eLocation]);
        }

        public virtual bool canHaveModule(ModuleType eModule)
        {
            return canHaveModule(eModule, getLocation(), getColony(), getColonyClass(), infos());
        }

        public virtual bool canSpreadModule(ModuleType eModule, bool bTestState = false)
        {
            if (!canHaveModule(eModule))
            {
                return false;
            }

            if (!(infos().module(eModule).mbSpread))
            {
                return false;
            }

            if (bTestState)
            {
                if (infos().module(eModule).mbPopulation)
                {
                    if (getMaxPopulation() >= getColonyCap())
                    {
                        return false;
                    }
                }

                if (infos().module(eModule).mbLabor)
                {
                    if (getLabor() >= getColonyCap())
                    {
                        return false;
                    }
                }

                if (infos().module(eModule).mbPopulation)
                {
                    if (getNextPopulationModule() != eModule)
                    {
                        return false;
                    }
                }

                if (infos().module(eModule).mbLabor)
                {
                    if (!isNextLaborModule(eModule))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public virtual TileClient getBestModuleTile(ModuleType eModule)
        {
            TileClient pBestTile = null;

            TileClient pStartTile = startModuleTileClient();

            System.Random pRandom = new CrossPlatformRandom(getSeed() + getModuleCount(eModule));

            if (pBestTile == null)
            {
                int iBestValue = 0;

                foreach (TileClient pLoopTile in tileClientAll())
                {
                    if (pLoopTile.isModule())
                    {
                        bool bValid = false;
                        int iValue = 0;

                        for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
                        {
                            TileClient pAdjacentTile = tileClientAdjacent(pLoopTile, eDirection);

                            if (pAdjacentTile != null)
                            {
                                if (pAdjacentTile.canHaveModule(eModule))
                                {
                                    iValue += 100;
                                    bValid = true;
                                }
                                else if (!(pAdjacentTile.usable()))
                                {
                                    iValue += 50;
                                }
                            }
                        }

                        if (bValid)
                        {
                            if (pLoopTile.getModule() == eModule)
                            {
                                iValue += 600;
                            }
                            else if (infos().module(pLoopTile.getModule()).mbShuttle)
                            {
                                iValue += 300;
                            }

                            iValue += pRandom.Next(500);

                            iValue *= (Utils.maxStepDistance(this) * 3);
                            iValue /= ((Utils.maxStepDistance(this) * 2) + Utils.stepDistanceTile(pLoopTile, pStartTile));

                            if (iValue > iBestValue)
                            {
                                pBestTile = pLoopTile;
                                iBestValue = iValue;
                            }
                        }
                    }
                }
            }

            if (pBestTile == null)
            {
                int iBestValue = 0;

                foreach (TileClient pLoopTile in tileClientAll())
                {
                    if (pLoopTile.canHaveModule(eModule))
                    {
                        int iValue = int.MaxValue;

                        iValue /= (Utils.stepDistanceTile(pLoopTile, pStartTile) + 1);

                        iValue += pRandom.Next(100);

                        if (iValue > iBestValue)
                        {
                            pBestTile = pLoopTile;
                            iBestValue = iValue;
                        }
                    }
                }
            }

            return pBestTile;
        }

        public virtual TileClient getRandomModuleTile(ModuleType eModule, TileClient pTile)
        {
            TileClient pBestTile = null;

            System.Random pRandom = new CrossPlatformRandom(getSeed() + getModuleCount(eModule)); //

            if (pTile.canHaveModule(eModule))
            {
                pBestTile = pTile;
            }

            if (pBestTile == null)
            {
                int iBestValue = 0;

                foreach (TileClient pAdjacentTile in tileClientAdjacentAll(pTile))
                {
                    if (!(pAdjacentTile.isClaimed()))
                    {
                        if (pAdjacentTile.canHaveModule(eModule))
                        {
                            int iValue = 1;

                            foreach (InfoResource pLoopResource in infos().resources())
                            {
                                if (pLoopResource.maiLocationAppearanceProb[(int)getLocation()] > 0)
                                {
                                    if (pAdjacentTile.getResourceLevel(pLoopResource.meType, false) == ResourceLevelType.NONE)
                                    {
                                        iValue += 50;
                                    }
                                }
                            }

                            foreach (TileClient pOtherAdjacentTile in tileClientAdjacentAll(pAdjacentTile))
                            {
                                if (pOtherAdjacentTile == null)
                                {
                                    iValue += 100;
                                }
                                else if (!(pOtherAdjacentTile.isModule()))
                                {
                                    iValue += 150;
                                }
                                else
                                {
                                    if (infos().module(pOtherAdjacentTile.getModule()).mbShuttle)
                                    {
                                        if (pTile.isModule() && infos().module(pTile.getModule()).mbShuttle)
                                        {
                                            iValue += 250;
                                        }
                                        else
                                        {
                                            iValue += 200;
                                        }
                                    }

                                    if (pOtherAdjacentTile.getModule() == eModule)
                                    {
                                        iValue += 300;
                                    }
                                }
                            }

                            iValue += pRandom.Next(1000);

                            if (iValue > iBestValue)
                            {
                                pBestTile = pAdjacentTile;
                                iBestValue = iValue;
                            }
                        }
                    }
                }
            }

            return pBestTile;
        }

        public static int getColonyCap(bool bCampaign, int iNumPlayers, ColonyClassType eColonyClass, Infos pInfo)
        {
            int iCap = Globals.Infos.Globals.COLONY_CAP_BASE;

            if (bCampaign)
            {
                iCap += (Globals.Campaign.getTurn() * 2);
            }

            iCap *= (iNumPlayers + 3);
            iCap /= 5;

            if (eColonyClass != ColonyClassType.NONE)
            {
                iCap *= Math.Max(0, (pInfo.colonyClass(eColonyClass).miColonyCapModifier + 100));
                iCap /= 100;
            }

            return Math.Max(iCap, 10);
        }

        public static bool canFound(HQType eHQ, bool bHuman, GameSettings pGameSettings, Infos pInfos)
        {
            if (bHuman)
            {
                if (pInfos.rulesSet(pGameSettings.meRulesSetType).mabHQInvalidHuman[(int)eHQ])
                {
                    return false;
                }

                if (pGameSettings.mabInvalidHumanHQ[(int)eHQ])
                {
                    return false;
                }
            }
            else
            {
                if (pInfos.rulesSet(pGameSettings.meRulesSetType).mabHQInvalidAI[(int)eHQ])
                {
                    return false;
                }
            }

            return true;
        }

        public virtual bool canFound(HQType eHQ, bool bHuman)
        {
            if (isInvalidHQ(eHQ))
            {
                return false;
            }

            return GameServer.canFound(eHQ, bHuman, gameSettings(), infos());
        }

        public virtual bool isLastDay()
        {
            if (isSevenSols())
            {
                if (getDays() >= (getLastDay() - 1))
                {
                    return true;
                }
            }

            return false;
        }

        public virtual bool hasLastDay()
        {
            if (isSevenSols())
                return true;

            return false;
        }

        public virtual bool isLastHalfDay()
        {
            if (isSevenSols())
            {
                if (getDays() >= (getLastDay() - 1))
                {
                    if (getHours() >= (infos().location(getLocation()).miHoursPerDay / 2))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public virtual bool isGameAlmostOver()
        {

            if (isSevenSols())
            {
                if (getDays() >= (getLastDay() - 1))
                {
                    if (getHours() >= (infos().location(getLocation()).miHoursPerDay - 20))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public virtual int getTurnsLeft()
        {
            if (isSevenSols())
            {
                int iTurnsLeft = 0;

                int iDaysLeft = (getLastDay() - 1 - getDays());

                if (iDaysLeft > 0)
                {
                    iTurnsLeft += (iDaysLeft * Utils.getTurnsPerDay(infos(), getLocation()));
                }

                int iMinutesLeft = 0;

                int iHoursLeft = (infos().location(getLocation()).miHoursPerDay - getHours());

                if (iHoursLeft > 1)
                {
                    iMinutesLeft += (infos().location(getLocation()).miMinutesPerHour - getMinutes());
                    iMinutesLeft += ((iHoursLeft - 2) * infos().location(getLocation()).miMinutesPerHour);
                    iMinutesLeft += infos().location(getLocation()).miLastHourMinutes;
                }
                else if (iHoursLeft == 1)
                {
                    iMinutesLeft += (infos().location(getLocation()).miLastHourMinutes - getMinutes());
                }

                iTurnsLeft += (iMinutesLeft / Globals.Infos.Globals.MINUTES_PER_TURN);

                return iTurnsLeft;
            }

            return 0;
        }

        public virtual int getNextInterestTurn()
        {
            int turnsPerSol = Utils.getTurnsPerDay(infos(), getLocation());
            int interestTime = turnsPerSol - infos().location(getLocation()).miStartingHour * 6;
            if (getLocation() == LocationType.EUROPA)
            {
                int secondInterestTime = interestTime + infos().location(getLocation()).miSecondInterestHour * 6;
                while (interestTime <= getTurnCount() && secondInterestTime <= getTurnCount())
                {
                    interestTime += turnsPerSol;
                    secondInterestTime += turnsPerSol;
                }

                if (interestTime >= getTurnCount() && secondInterestTime >= getTurnCount())
                    return Math.Min(interestTime, secondInterestTime);
                else if (interestTime >= getTurnCount())
                    return interestTime;
                else
                    return secondInterestTime;
            }
            else
            {
                while (interestTime <= getTurnCount())
                    interestTime += turnsPerSol;
            }
            return interestTime;
        }

        public virtual int getInitialShares()
        {
            return infos().Globals.INITIAL_SHARES;
        }

        public virtual int getMajorityShares()
        {
            return infos().Globals.MAJORITY_SHARES;
        }

        public static int calculateHQValueBase(int iHQLevels, Infos pInfos)
        {
            int iValue = pInfos.Globals.BASE_STOCK_VALUE;

            iValue += (pInfos.Globals.BASE_STOCK_VALUE_PER_LEVEL * iHQLevels);

            return iValue;
        }

        public virtual int countPlayersAlive()
        {
            int iCount = 0;

            for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
            {
                if (!(playerClient(eLoopPlayer).isSubsidiary()))
                {
                    iCount++;
                }
            }

            return iCount;
        }

        public virtual int countPlayersStarted()
        {
            int iCount = 0;

            for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
            {
                if (playerClient(eLoopPlayer).getNumHQs() > 0)
                {
                    iCount++;
                }
            }

            return iCount;
        }

        public virtual int countTeamsWinEligible()
        {
            int iCount = 0;

            for (TeamType eLoopTeam = 0; eLoopTeam < (TeamType)(getNumTeams()); eLoopTeam++)
            {
                foreach (PlayerClient pLoopPlayer in getPlayerClientAll())
                {
                    if (pLoopPlayer.getTeam() == eLoopTeam)
                    {
                        if (pLoopPlayer.isWinEligible())
                        {
                            iCount++;
                            break;
                        }
                    }
                }
            }

            return iCount;
        }

        public virtual int countOriginalAIAlive()
        {
            int iCount = 0;

            for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
            {
                if (!(playerClient(eLoopPlayer).wasEverHuman()))
                {
                    if (!(playerClient(eLoopPlayer).isSubsidiary()))
                    {
                        iCount++;
                    }
                }
            }

            return iCount;
        }

        public virtual bool wasEverHumanGame()
        {
            for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
            {
                if (playerClient(eLoopPlayer).wasEverHuman())
                {
                    return true;
                }
            }

            return false;
        }

        public virtual bool wasEverHumanTeam(TeamType eTeam)
        {
            for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
            {
                if (playerClient(eLoopPlayer).getTeam() == eTeam)
                {
                    if (playerClient(eLoopPlayer).wasEverHuman())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected virtual int countSharesAvailable(bool bIncludeSubsidiaries)
        {
            int iCount = 0;

            for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
            {
                PlayerClient pLoopPlayer = playerClient(eLoopPlayer);

                if (!(pLoopPlayer.isSubsidiary()) || bIncludeSubsidiaries)
                {
                    iCount += pLoopPlayer.getSharesAvailable();
                }
            }

            return iCount;
        }

        public virtual int countPatentsAvailable()
        {
            int iCount = 0;

            for (PatentType eLoopPatent = 0; eLoopPatent < infos().patentsNum(); eLoopPatent++)
            {
                if (canEverPatent(eLoopPatent))
                {
                    if (!isPatentOwned(eLoopPatent))
                    {
                        iCount++;
                    }
                }
            }

            return iCount;
        }

        public virtual int countPatentsPossible()
        {
            int iCount = 0;

            for (PatentType eLoopPatent = 0; eLoopPatent < infos().patentsNum(); eLoopPatent++)
            {
                if (canEverPatent(eLoopPatent))
                {
                    iCount++;
                }
            }

            return iCount;
        }

        public virtual bool isResourceValid(ResourceType eResource)
        {
            return !(infos().resource(eResource).mabLocationInvalid[(int)getLocation()]);
        }

        public virtual bool canEverPatent(PatentType ePatent, bool bTestLocation = true)
        {
            {
                ColonyClassType eColonyClass = getColonyClass();

                if (eColonyClass != ColonyClassType.NONE)
                {
                    if (infos().colonyClass(eColonyClass).mabPatentInvalid[(int)ePatent])
                    {
                        return false;
                    }
                }
            }

            if (bTestLocation)
            {
                return !(infos().patent(ePatent).mabLocationInvalid[(int)getLocation()]);
            }
            else
            {
                return true;
            }
        }

        public virtual bool canEverResearch(TechnologyType eTech)
        {
            return !(infos().technology(eTech).mabLocationInvalid[(int)getLocation()]);
        }

        public virtual bool canEverHack(EspionageType eEspionage)
        {
            return !(infos().espionage(eEspionage).mabLocationInvalid[(int)getLocation()]);
        }

        public virtual int getEclipseHour()
        {
            if (getLevel() == LevelType.NONE)
            {
                return -1;
            }

            int iEclipseLength = infos().location(getLocation()).miEclipseLength;

            if (iEclipseLength == 0)
            {
                return -1;
            }

            int iLongitude = (infos().level(getLevel()).miLongitude % 360);

            if ((iLongitude >= 90) && (iLongitude < 270))
            {
                return -1;
            }

            return Math.Max(0, ((infos().location(getLocation()).miSunriseHour + (((infos().location(getLocation()).miSunsetHour - infos().location(getLocation()).miSunriseHour) * ((iLongitude + 90) % 360)) / 180)) - (iEclipseLength / 2)));
        }
        public virtual bool isEclipse(int iHour)
        {
            if (getEventStateGameActive() != EventStateType.NONE && infos().eventState(getEventStateGameActive()).mbEclipse)
                return true;

            int iEclipseHour = getEclipseHour();

            if (iEclipseHour == -1)
            {
                return false;
            }

            int iEclipseLength = infos().location(getLocation()).miEclipseLength;

            if ((iHour >= iEclipseHour) && (iHour < (iEclipseHour + iEclipseLength)))
            {
                return true;
            }

            return false;
        }

        public virtual bool sunHidden(int iHour)
        {
            if (isEclipse(iHour))
            {
                return true;
            }

            if (iHour < infos().location(getLocation()).miSunriseHour)
            {
                return true;
            }

            if (iHour >= infos().location(getLocation()).miSunsetHour)
            {
                return true;
            }

            return false;
        }

        public virtual bool buildingClosed(int iHour, BuildingType eBuilding, PlayerType ePlayer)
        {
            if (ePlayer != PlayerType.NONE)
            {
                if (playerClient(ePlayer).isBuildingAlwaysOn(eBuilding))
                {
                    return false;
                }
            }

            if (infos().building(eBuilding).mbUsesSun)
            {
                if (sunHidden(iHour))
                {
                    return true;
                }
            }

            return false;
        }
        public int calculateOpenCount(int iTurns, BuildingType eBuilding, PlayerType ePlayer)
        {
            int iCount = 0;

            int iHour = getHours();
            int iMinutes = getMinutes();

            for (int i = 0; i < iTurns; i++)
            {
                if (!(buildingClosed(iHour, eBuilding, ePlayer)))
                {
                    iCount++;
                }

                iMinutes += infos().Globals.MINUTES_PER_TURN;

                if (iHour == (infos().location(getLocation()).miHoursPerDay - 1))
                {
                    if (iMinutes >= infos().location(getLocation()).miLastHourMinutes)
                    {
                        iHour = 0;
                        iMinutes = 0;
                    }
                }
                else
                {
                    if (iMinutes >= infos().location(getLocation()).miMinutesPerHour)
                    {
                        iHour++;
                        iMinutes = 0;
                    }
                }
            }

            return iCount;
        }

        public virtual bool isEventStateOff(EventStateType eEventState)
        {
            if (infos().eventState(eEventState).mbUsesSun)
            {
                if (sunHidden(getHours()))
                {
                    return true;
                }
            }

            return false;
        }

        public virtual bool isTurnBasedScanning()
        {
            if (isGameOption(GameOptionType.REVEAL_MAP))
            {
                return false;
            }

            if (!isOriginalSinglePlayer())
            {
                return false;
            }

            for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
            {
                PlayerClient pLoopPlayer = playerClient(eLoopPlayer);

                if (pLoopPlayer.wasEverHuman())
                {
                    if (pLoopPlayer.isHQFounded())
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public virtual int getScanDelay()
        {
            if (isTurnBasedScanning())
            {
                return 1;
            }

            int iScanDelay = infos().mapSize(getMapSize()).miScanTime;

            iScanDelay *= Math.Max(1, (((int)(getNumPlayers()) * 2) - getHQLevels()));
            iScanDelay /= ((int)(getNumPlayers()) * 2);

            iScanDelay = Math.Max(1, iScanDelay);

            return iScanDelay;
        }

        public virtual HQLevelType getFoundOrderClaimBonus(int iFoundNum)
        {
            if (iFoundNum > ((int)(getNumPlayers() + 2) - (int)(infos().HQLevelsNum())))
            {
                return (HQLevelType)(Utils.clamp(-(iFoundNum - (int)(getNumPlayers() + 2)), 2, (int)(infos().HQLevelsNum() - 1)));
            }
            else
            {
                return HQLevelType.NONE;
            }
        }

        public virtual int getFoundOrderBlackMarketBonus(int foundNum)
        {
            return Math.Max(0, (((int)(getNumPlayers()) - foundNum + 1) * 90) / (int)(getNumPlayers()));
        }

        public virtual bool canStartAuction()
        {
            if (isGameOption(GameOptionType.NO_AUCTIONS))
            {
                return false;
            }

            if (getDays() < infos().location(getLocation()).miAuctionMinDay)
            {
                return false;
            }

            if (countSharesAvailable(false) == 0)
            {
                return false;
            }

            if (isLastDay())
            {
                return false;
            }

            return true;
        }

        public virtual GameSettings gameSettings()
        {
            return mGameSettings;
        }
        public virtual CustomColonySettings customColonySettings()
        {
            return mColonySettings;
        }
        public virtual ScenarioShipmentRequest scenarioShipmentRequest()
        {
            return mShipmentRequest;
        }
        public virtual int getSeed()
        {
            return mGameSettings.miSeed;
        }
        public virtual int getMapWidth()
        {
            return mGameSettings.miWidth;
        }
        public virtual int getMapHeight()
        {
            return mGameSettings.miHeight;
        }
        public virtual int getTerrainWidth()
        {
            return mMap.getTerrainWidth();
        }
        public virtual int getTerrainHeight()
        {
            return mMap.getTerrainLength();
        }
        public virtual PlayerType getNumPlayers()
        {
            return (PlayerType)(gameSettings().miNumPlayers);
        }
        protected virtual int getOriginalHumanPlayers()
        {
            return gameSettings().miNumHumans;
        }
        public virtual bool isOriginalSinglePlayer()
        {
            return (getOriginalHumanPlayers() == 1);
        }
        public virtual bool isOriginalAllHumans()
        {
            return ((PlayerType)(getOriginalHumanPlayers()) == getNumPlayers());
        }
        public virtual MapSizeType getMapSize()
        {
            return gameSettings().meMapSize;
        }
        public virtual TerrainClassType getTerrainClass()
        {
            return gameSettings().meTerrainClass;
        }
        public virtual ResourceMinimumType getResourceMinimum()
        {
            return gameSettings().meResourceMinimum;
        }
        public virtual ResourcePresenceType getResourcePresence()
        {
            return gameSettings().meResourcePresence;
        }
        public virtual RulesSetType getRulesSet()
        {
            return gameSettings().meRulesSetType;
        }
        public virtual GameSetupType getGameSetup()
        {
            return gameSettings().meGameSetupType;
        }
        public virtual LevelType getLevel()
        {
            return gameSettings().meLevelType;
        }
        public virtual LocationType getLocation()
        {
            return gameSettings().meLocation;
        }
        public virtual ResourceLevelType getHighestResourceLevel()
        {
            ResourceLevelType eResourceLevel = ResourceLevelType.NONE;

            for (ResourceLevelType eLoopResourceLevel = 0; eLoopResourceLevel < infos().resourceLevelsNum(); eLoopResourceLevel++)
            {
                if (!(infos().resourceLevel(eLoopResourceLevel).mabLocationInvalid[(int)getLocation()]))
                {
                    eResourceLevel = eLoopResourceLevel;
                }
            }

            return eResourceLevel;
        }
        public static bool isCampaign(LevelType eLevel)
        {
            return (eLevel != LevelType.NONE);
        }
        public virtual bool isCampaign()
        {
            return isCampaign(getLevel());
        }
        public virtual bool isCampaignSevenSols()
        {
            return (isCampaign() && isSevenSols());
        }

        public virtual int getNumTeams()
        {
            return miNumTeams;
        }
        public virtual bool isTeamGame()
        {
            return (getNumTeams() < (int)(getNumPlayers()));
        }

        public virtual float getTerrainThickness(HeightType eHeight)
        {
            return mMap.getPlateauThickness(eHeight);
        }

        public virtual float getPlateauHeight(HeightType eHeight)
        {
            return mMap.getPlateauHeight(eHeight);
        }

        public virtual int numTiles()
        {
            return mMap.numTiles();
        }

        public virtual int getSystemUpdateCount()
        {
            return miSystemUpdateCount;
        }

        public virtual int getGameUpdateCount()
        {
            return miGameUpdateCount;
        }

        public virtual int getTurnCount()
        {
            return miTurnCount;
        }
        public virtual bool reportGame()
        {
            return (getTurnCount() > 100);
        }

        protected virtual int getTurnBasedTime()
        {
            return miTurnBasedTime;
        }
        public virtual bool isTurnBased()
        {
            return (getTurnBasedTime() > 0);
        }
        public virtual bool isTurnBasedPaused()
        {
            if (!isTurnBasedScanning())
            {
                return false;
            }

            return !isTurnBased();
        }

        protected virtual int getDelayTime()
        {
            return miDelayTime;
        }
        public virtual bool isDelay()
        {
            return (getDelayTime() > 0);
        }

        public virtual int getMinutes()
        {
            return miMinutes;
        }

        public virtual int getHours()
        {
            return miHours;
        }

        public virtual int getDays()
        {
            return miDays;
        }
        public static int getAdjustedLastDay(int iDay, bool bMarathon)
        {
            if (bMarathon)
            {
                return ((iDay * 3) / 2);
            }
            else
            {
                return iDay;
            }
        }
        public virtual int getLastDay()
        {
            return getAdjustedLastDay(infos().location(getLocation()).miLastDay, isGameOption(GameOptionType.MARATHON_MODE));
        }

        public virtual int getStartModuleTileID()
        {
            return miStartModuleTileID;
        }
        public virtual TileClient startModuleTileClient()
        {
            return tileClient(getStartModuleTileID());
        }

        public virtual int getPopulation()
        {
            return miPopulation;
        }

        public virtual int getMaxPopulation()
        {
            return miMaxPopulation;
        }

        public virtual int getColonyCap()
        {
            return miColonyCap;
        }

        public virtual int getLabor()
        {
            return miLabor;
        }

        public virtual int getHQLevels()
        {
            return miHQLevels;
        }

        protected virtual int getEntertainmentDemand()
        {
            return miEntertainmentDemand;
        }

        protected virtual int getEntertainmentSupply()
        {
            return miEntertainmentSupply;
        }

        protected virtual int getAuctionCount()
        {
            return miAuctionCount;
        }
        public virtual int getStartingBid()
        {
            return (infos().Globals.MIN_AUCTION_BID + (getAuctionCount() * ((isCampaign() || (getAuctionCount() > 10)) ? 2000 : 1000)));
        }
        public virtual int getNextAuctionBid()
        {
            int iBid = 0;

            if (getAuctionLeader() == PlayerType.NONE)
            {
                iBid = getStartingBid();
            }
            else
            {
                iBid = infos().Globals.AUCTION_BID;

                while (iBid <= (getAuctionBid() / 10))
                {
                    iBid *= 2;
                }
            }

            return iBid;
        }

        public virtual int getAuctionBid()
        {
            return miAuctionBid;
        }

        public virtual int getAuctionTime()
        {
            return miAuctionTime;
        }

        public virtual int getAuctionData1()
        {
            return miAuctionData1;
        }
        public virtual TileClient auctionTileClient()
        {
            if ((getAuction() == AuctionType.TILE) || (getAuction() == AuctionType.TILE_BUILDING))
            {
                return (TileClient)tileClient(getAuctionData1());
            }
            else
            {
                return null;
            }
        }

        public virtual int getAuctionData2()
        {
            return miAuctionData2;
        }

        public virtual int getEventStateGameDelay()
        {
            return miEventStateGameDelay;
        }

        public virtual int getEventStateGameTime()
        {
            return miEventStateGameTime;
        }

        public virtual int getSharesBought()
        {
            return miSharesBought;
        }
        public virtual int getSharePrice()
        {
            int iPrice = infos().Globals.COLONY_SHARE_PRICE_BASE;

            int iSharesBought = getSharesBought();

            if ((getMaxPopulation() < getColonyCap()) || (getLabor() < getColonyCap()))
            {
                iSharesBought *= 2;
                iSharesBought /= 3;
            }

            for (int i = 0; i < iSharesBought; i++)
            {
                iPrice += (infos().Globals.COLONY_SHARE_PRICE_PER * ((i / 10) + 1));
            }

            iPrice *= (Constants.MAX_NUM_PLAYERS);
            iPrice /= (Constants.MAX_NUM_PLAYERS + (int)(getNumPlayers()));

            {
                ColonyClassType eColonyClass = getColonyClass();

                if (eColonyClass != ColonyClassType.NONE)
                {
                    iPrice *= Math.Max(0, (infos().colonyClass(eColonyClass).miBaseShareModifier + 100));
                    iPrice /= 100;
                }
            }

            if (getSevenSols() == SevenSolsType.WHOLESALE)
            {
                iPrice /= 2;
            }

            if (iPrice < 20000)
            {
                iPrice -= (iPrice % 1000);
            }
            else
            {
                iPrice -= (iPrice % 2000);
            }

            return iPrice;
        }

        public virtual int getFoundMoney()
        {
            return miFoundMoney;
        }

        public virtual int getGeothermalCount()
        {
            return miGeothermalCount;
        }

        public virtual bool isGameOver()
        {
            return mbGameOver;
        }

        public virtual bool isPerkAuctioned()
        {
            return mbPerkAuctioned;
        }

        public virtual bool isAuctionStarted()
        {
            return mbAuctionStarted;
        }

        public virtual bool isCheating()
        {
            return meCheatingPlayer != PlayerType.NONE;
        }
        public virtual PlayerType getCheatingPlayer()
        {
            return meCheatingPlayer;
        }

        public virtual HandicapType getHumanHandicap()
        {
            return meHumanHandicap;
        }

        public virtual TeamType getWinningTeam()
        {
            return meWinningTeam;
        }

        public virtual AuctionType getAuction()
        {
            return meAuction;
        }
        public virtual bool isAuction()
        {
            return (getAuction() != AuctionType.NONE);
        }

        public virtual PlayerType getAuctionLeader()
        {
            return meAuctionLeader;
        }

        public virtual EventGameType getLastEventGame()
        {
            return meLastEventGame;
        }

        public virtual EventStateType getEventStateGame()
        {
            return meEventStateGame;
        }
        public virtual EventStateType getEventStateGameActive()
        {
            if (getEventStateGameDelay() > 0)
            {
                return EventStateType.NONE;
            }
            else
            {
                return getEventStateGame();
            }
        }
        public virtual EventStateType getEventStateLevel()
        {
            return meEventStateLevel;
        }
        public virtual bool isNoAdjacency()
        {
            {
                EventStateType eEventStateGame = getEventStateGameActive();

                if ((eEventStateGame != EventStateType.NONE) && infos().eventState(eEventStateGame).mbNoAdjaceny)
                {
                    return true;
                }
            }

            {
                EventStateType eEventStateLevel = getEventStateLevel();

                if ((eEventStateLevel != EventStateType.NONE) && infos().eventState(eEventStateLevel).mbNoAdjaceny)
                {
                    return true;
                }
            }

            return false;
        }

        public virtual GameSpeedType getGameSpeed()
        {
            return meGameSpeed;
        }
        public virtual GameSpeedType getPrevGameSpeed()
        {
            return mePrevGameSpeed;
        }
        public virtual bool isPaused()
        {
            return (getGameSpeed() == GameSpeedType.NONE);
        }
        public virtual bool isGraphicsPaused()
        {
            return (isPaused() || isTurnBasedPaused() || isDelay() || isAuction());
        }

        public virtual ColonyType getColony()
        {
            return meColony;
        }
        public virtual string getColonyName()
        {
            string zText = "";

            if (getLevel() != LevelType.NONE)
            {
                zText += TEXT(infos().level(getLevel()).meName);
            }
            else if (infos().rulesSet(getRulesSet()).meColonyName != TextType.NONE)
            {
                zText = TEXT(infos().rulesSet(getRulesSet()).meColonyName);
            }
            else
            {
                InfoColony colonyInfo = infos().colony(getColony());
                int size = colonyInfo.maeNames.Count;
                TextType textType = (size == 0) ? TextType.NONE : colonyInfo.maeNames[getSeed() % size];
                zText += (textType == TextType.NONE) ? "UNKNOWN_COLONY_NAME" : TEXT(textType);
            }

            return zText;
        }

        public virtual ColonyClassType getColonyClass()
        {
            return meColonyClass;
        }

        public virtual HQLevelType getHQHighest()
        {
            return meHQHighest;
        }

        public virtual ModuleType getNextPopulationModule()
        {
            return meNextPopulationModule;
        }

        public virtual SevenSolsType getSevenSols()
        {
            return meSevenSols;
        }
        public virtual bool isSevenSols()
        {
            return (getSevenSols() != SevenSolsType.NONE);
        }

        public virtual ScenarioDifficultyType getScenarioDifficultyType()
        {
            return meScenarioDifficultyType;
        }

        public virtual int getIceCount(IceType eIndex)
        {
            return maiIceCount[(int)eIndex];
        }

        public virtual int getTerrainCount(TerrainType eIndex)
        {
            return maiTerrainCount[(int)eIndex];
        }

        public virtual int getResourceRateCount(ResourceType eIndex)
        {
            return maiResourceRateCount[(int)eIndex];
        }

        public virtual int getModuleCount(ModuleType eIndex)
        {
            return maiModuleCount[(int)eIndex];
        }

        public virtual int getBlackMarketCount(BlackMarketType eIndex)
        {
            return maiBlackMarketCount[(int)eIndex];
        }

        public virtual int getEspionageCount(EspionageType eIndex)
        {
            return maiEspionageCount[(int)eIndex];
        }

        public virtual int getWholesaleSlotCount(int iIndex)
        {
            return maiWholesaleSlotCount[iIndex];
        }
        public virtual int getWholesaleSlotShipment(int iIndex)
        {
            return ((getWholesaleSlotCount(iIndex) * infos().Globals.WHOLESALE_PER) + infos().Globals.WHOLESALE_BASE);
        }

        public virtual int getBuildingAverageModifier(BuildingType eIndex)
        {
            return maiBuildingAverageModifier[(int)eIndex];
        }

        public virtual bool isGameOption(GameOptionType eIndex)
        {
            return mabGameOptions[(int)eIndex];
        }
        public virtual List<bool> getGameOptions()
        {
            return mabGameOptions;
        }

        public virtual bool isInvalidHQ(HQType eIndex)
        {
            return mabInvalidHQ[(int)eIndex];
        }

        public virtual bool hasBuildingClassBeenConstructed(BuildingType building)
        {
            BuildingClassType buildingClass = infos().building(building).meClass;
            return isBuildingClassFinished(buildingClass);
        }
        public virtual bool isBuildingClassFinished(BuildingClassType eIndex)
        {
            return mabBuildingClassFinished[(int)eIndex];
        }

        public virtual bool isBuildingUnavailable(BuildingType eIndex)
        {
            return mabBuildingUnavailable[(int)eIndex];
        }

        public virtual bool isBlackMarketAvailable(BlackMarketType eIndex)
        {
            return mabBlackMarketAvailable[(int)eIndex];
        }

        public virtual bool isNextLaborModule(ModuleType eIndex)
        {
            return mabNextLaborModule[(int)eIndex];
        }

        public virtual bool isWholesaleSlotResets(int iIndex)
        {
            return mabWholesaleSlotResets[iIndex];
        }

        public virtual PlayerType getPatentOwner(PatentType eIndex)
        {
            return maePatentOwner[(int)eIndex];
        }
        public virtual bool isPatentOwned(PatentType eIndex)
        {
            return (getPatentOwner(eIndex) != PlayerType.NONE);
        }
        public virtual bool isPatentAcquiredAny(PatentType eIndex)
        {
            for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
            {
                if (playerClient(eLoopPlayer).isPatentAcquired(eIndex))
                {
                    return true;
                }
            }

            return false;
        }

        public virtual ResourceType getWholesaleSlotResource(int iIndex)
        {
            return maeWholesaleSlotResource[iIndex];
        }
        public virtual List<int> buildWholesaleSlotResourceCost(int iSlot)
        {
            List<int> aiResourceCost = Enumerable.Repeat(0, (int)(infos().resourcesNum())).ToList();
            aiResourceCost[(int)(getWholesaleSlotResource(iSlot))] = getWholesaleSlotShipment(iSlot);
            return aiResourceCost;
        }

        public virtual ResourceType getWholesaleSlotResourceNext(int iIndex)
        {
            return maeWholesaleSlotResourceNext[iIndex];
        }

        public ResourceType getImportSlotResource(int iSlot)
        {
            return maeImportResource[iSlot];
        }

        public int getNumImportSlots()
        {
            return maeImportResource.Count;
        }

        public int getImportSlotCost(int iSlot)
        {
            return maiImportCost[iSlot];
        }

        public ResourceType getImportResourcePayment(int iSlot)
        {
            return maeImportPaymentResource[iSlot % maeImportPaymentResource.Count];
        }

        public virtual string getTeamName(TeamType team)
        {
            return maTeamNames[(int)team];
        }

        public virtual int getBuildingResourceOutput(BuildingType eIndex1, ResourceType eIndex2)
        {
            return maaiBuildingResourceOutput[(int)eIndex1][(int)eIndex2];
        }
        public virtual List<int> getBuildingResourceOutput(BuildingType eIndex)
        {
            return maaiBuildingResourceOutput[(int)eIndex];
        }

        public virtual string getGameWonMessage()
        {
            return string.Empty;
        }

        public virtual string getGameLostMessage()
        {
            return TEXT("TEXT_CHARTS_AND_GRAPHS_OVERVIEW_CONCEDED_LOSS_TITLE", "".ToText(AppCore.AppGlobals.GameGlobals.ActivePlayerClient.getGender()));
        }

        public virtual string getBuyoutCheckMessage()
        {
            return TEXT("TEXT_LOST_SCREEN_REASON_LABEL");
        }

        public virtual List<TerrainInfo> terrainInfoAll()
        {
            return mMap.terrainInfoAll();
        }

        public virtual int terrainInfoIDRange(int iID, int iDX, int iDY, int iRange)
        {
            /*
            int iX = mapClient().getTerrainInfoX(iID);
            int iY = mapClient().getTerrainInfoY(iID);

            int iNewX = iX + iDX;
            int iNewY = iY + iDY;

            if (Utils.stepDistance(iX, iY, iNewX, iNewY) > iRange)
            {
                return -1;
            }
            else
            {
                return mapClient().terrainInfoIDGrid(iNewX, iNewY);
            }
            */
            return mapClient().terrainInfoIDRange(iID, iDX, iDY, iRange);
        }

        public virtual LinkedList<EventGameTime> getEventGameTimes()
        {
            return mEventGameTimeList;
        }

        public virtual MarketClient marketClient()
        {
            return mMarket;
        }

        public virtual MapClient mapClient()
        {
            return mMap;
        }

        public virtual GameEventsClient gameEventsClient()
        {
            return mGameEvents;
        }

        public virtual StatsClient statsClient()
        {
            return mStats;
        }

        public virtual ConditionManagerClient conditionManagerClient()
        {
            return mConditionManager;
        }

        public virtual PlayerClient playerClient(PlayerType eIndex)
        {
            return maPlayers[(int)eIndex];
        }
        public virtual IEnumerable<PlayerClient> getPlayerClientAll()
        {
            for (PlayerType i = 0; i < getNumPlayers(); i++)
            {
                yield return playerClient(i);
            }
        }
        public virtual IEnumerable<PlayerClient> getPlayerClientAliveAll()
        {
            return getPlayerClientAll().Where(player => !(player.isSubsidiary()));
        }

        public virtual TileClient tileClient(int iIndex)
        {
            return mMap.tileClient(iIndex);
        }
        public virtual List<TileClient> tileClientAll()
        {
            return mMap.tileClientAll();
        }

        public virtual TileClient tileClientAdjacent(TileClient tileClient, DirectionType eDirection)
        {
            return mMap.tileClientAdjacent(tileClient, eDirection);
        }

        public virtual IEnumerable<TileClient> tileClientAdjacentAll(TileClient tileClient)
        {
            for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
            {
                TileClient adjacentTile = mapClient().tileClientAdjacent(tileClient, eDirection);
                if (adjacentTile != null)
                {
                    yield return adjacentTile;
                }
            }
        }

        public virtual TileClient tileClientRange(TileClient tileClient, int iDX, int iDY, int iRange)
        {
            int iNewX = tileClient.getX() + iDX;
            int iNewY = tileClient.getY() + iDY;

            if (Utils.stepDistance(tileClient.getX(), tileClient.getY(), iNewX, iNewY) > iRange)
            {
                return null;
            }
            else
            {
                return mapClient().tileClientGrid(iNewX, iNewY);
            }
        }
        public virtual IEnumerable<TileClient> tileClientRangeIterator(TileClient tileClient, int iRange)
        {
            for (int iDX = -(iRange); iDX <= iRange; iDX++)
            {
                for (int iDY = -(iRange); iDY <= iRange; iDY++)
                {
                    TileClient pLoopTile = tileClientRange(tileClient, iDX, iDY, iRange);
                    if (pLoopTile != null)
                    {
                        yield return pLoopTile;
                    }
                }
            }
        }

        public virtual ModuleClient moduleClient(int iID)
        {
            if (mModuleDictionary.ContainsKey(iID))
            {
                return mModuleDictionary[iID];
            }
            else
            {
                return null;
            }
        }
        public virtual Dictionary<int, ModuleClient> getModuleDictionary()
        {
            return mModuleDictionary;
        }
        public virtual List<ModuleClient> getModuleList()
        {
            return mModuleDictionary.Values.ToList<ModuleClient>();
        }

        public virtual HQClient hqClient(int iID)
        {
            if (mHQDictionary.ContainsKey(iID))
            {
                return mHQDictionary[iID];
            }
            else
            {
                return null;
            }
        }
        public virtual Dictionary<int, HQClient> getHQDictionary()
        {
            return mHQDictionary;
        }
        public virtual int getNumHQs()
        {
            return getHQDictionary().Count;
        }

        public virtual ConstructionClient constructionClient(int iID)
        {
            if (mConstructionDictionary.ContainsKey(iID))
            {
                return mConstructionDictionary[iID];
            }
            else
            {
                return null;
            }
        }
        public virtual Dictionary<int, ConstructionClient> getConstructionDictionary()
        {
            return mConstructionDictionary;
        }

        public virtual BuildingClient buildingClient(int iID)
        {
            if (mBuildingDictionary.ContainsKey(iID))
            {
                return mBuildingDictionary[iID];
            }
            else
            {
                return null;
            }
        }
        public virtual Dictionary<int, BuildingClient> getBuildingDictionary()
        {
            return mBuildingDictionary;
        }
        public virtual List<BuildingClient> getBuildingList()
        {
            return mBuildingDictionary.Values.ToList<BuildingClient>();
        }

        public virtual UnitClient unitClient(int iID)
        {
            if (mUnitDictionary.ContainsKey(iID))
            {
                return mUnitDictionary[iID];
            }
            else
            {
                return null;
            }
        }
        public virtual Dictionary<int, UnitClient> getUnitDictionary()
        {
            return mUnitDictionary;
        }
    }

    public class GameServer : GameClient
    {
        protected virtual void makeDirty(GameDirtyType eType)
        {
            mDirtyBits.SetBit((int)eType, true);
        }
        protected virtual void makeAllDirty()
        {
            using (new UnityProfileScope("GameServer.makeAllDirty"))
            {
                for (GameDirtyType eLoopType = 0; eLoopType < GameDirtyType.NUM_TYPES; eLoopType++)
                {
                    makeDirty(eLoopType);
                }
            }
        }
        public virtual void clearDirty()
        {
            mDirtyBits.Clear();
        }

        protected bool mbSettingsDirty = true;
        public virtual bool isSettingsDirty()
        {
            return mbSettingsDirty;
        }
        public virtual void clearSettingsDirty()
        {
            mbSettingsDirty = false;
        }

        protected bool mbPlayGame = false;
        public bool PlayGame { get { return mbPlayGame; } set { mbPlayGame = value; } }

        protected int miNextTileGroupID = 0;
        protected int miNextModuleID = 0;
        protected int miNextHQID = 0;
        protected int miNextConstructionID = 0;
        protected int miNextBuildingID = 0;
        protected int miNextUnitID = 0;
        protected int miLastUpdateTimeMS = 0;
        protected int miModuleRevealTime = 0;

        protected System.Random mRandom = null;
        protected System.Random mRandomEvent = null;
        protected System.Random mRandomColony = null;
        protected System.Random mRandomAuction = null;
        protected System.Random mRandomAuctionPatent = null;
        protected System.Random mRandomAuctionSabotage = null;
        protected System.Random mRandomAuctionTileBuilding = null;
        protected System.Random mRandomAuctionPerk = null;
        protected System.Random mRandomMisc = null;

        protected List<int> maiAreaTiles = new List<int>();
        protected List<int> maiWholesaleSlotDelay = new List<int>();
        protected List<int> maiImportValueModifier = new List<int>();
        protected List<int> maiExportValueModifier = new List<int>();
        protected List<int> maiCaveTiles = new List<int>();
        protected List<int> maiGeothermalTiles = new List<int>();

        protected HashSet<int> maiIceTiles = new HashSet<int>();
        protected HashSet<int> maiWindTiles = new HashSet<int>(); //AIs consider wind and not solar because there's a lot more variance for wind power
        protected HashSet<int> maiColonyAdjacentTiles = new HashSet<int>();

        protected List<HashSet<int>> maiResourceTiles = new List<HashSet<int>>();
        protected List<HashSet<int>> maiResourceAdjacentTiles = new List<HashSet<int>>();

        protected List<bool> mabBuildingHasInput = new List<bool>();

        protected List<EventGameType> maeEventGameDie = new List<EventGameType>();

        protected List<List<List<bool>>> maaabResourceInput = new List<List<List<bool>>>();

        protected Dictionary<int, TileGroupServer> mTileGroupDictionary = new Dictionary<int, TileGroupServer>();

        public GameServer()
        {
        }

        public virtual void init(Infos pInfos, GameSettings pSettings)
        {
            using (new UnityProfileScope("GameServer.init"))
            {
                using (new UnityProfileScope("GameServer.initializeValues"))
                {
                    mInfos = pInfos;

                    mGameSettings = pSettings;
                    mShipmentRequest = new ScenarioShipmentRequest();
                    mColonySettings = new CustomColonySettings();

                    ColonyType eColony = ((isCampaign()) ? Globals.Campaign.getLevelColony(getLevel()) : ColonyType.NONE);

                    miNumTeams = pSettings.miNumUniqueTeams;
                    miHours = infos().location(getLocation()).miStartingHour;
                    miColonyCap = GameClient.getColonyCap(isCampaign(), (int)(getNumPlayers()), pSettings.meColonyClass, infos());

                    if (isCampaign())
                    {
                        EventLevelType eEventLevel = Globals.Campaign.getLevelEvent(getLevel());

                        if (eEventLevel != EventLevelType.NONE)
                        {
                            meEventStateLevel = infos().eventLevel(eEventLevel).meEventState;
                        }
                    }

                    meGameSpeed = gameSettings().meGameSpeed;
                    meColony = eColony;
                    meColonyClass = pSettings.meColonyClass;
                    meScenarioDifficultyType = pSettings.meScenarioDifficulty;
                    maiIceCount = infos().ices().Select(i => 0).ToList();
                    maiTerrainCount = infos().terrains().Select(i => 0).ToList();
                    maiResourceRateCount = infos().resources().Select(i => 0).ToList();
                    maiModuleCount = infos().modules().Select(i => 0).ToList();
                    maiBlackMarketCount = infos().blackMarkets().Select(i => 0).ToList();
                    maiEspionageCount = infos().espionages().Select(i => 0).ToList();
                    maiWholesaleSlotCount = Enumerable.Repeat(0, infos().Globals.NUM_WHOLESALE_SLOTS).ToList();
                    maiBuildingAverageModifier = infos().buildings().Select(i => 0).ToList();

                    mabInvalidHQ = infos().HQs().Select(i => false).ToList();
                    mabBuildingClassFinished = infos().buildingClasses().Select(i => false).ToList();
                    mabBuildingUnavailable = infos().buildings().Select(i => false).ToList();
                    mabBlackMarketAvailable = infos().blackMarkets().Select(i => false).ToList();
                    mabNextLaborModule = infos().modules().Select(i => false).ToList();
                    mabWholesaleSlotResets = Enumerable.Repeat(false, infos().Globals.NUM_WHOLESALE_SLOTS).ToList();

                    maePatentOwner = infos().patents().Select(i => PlayerType.NONE).ToList();
                    maeWholesaleSlotResource = Enumerable.Repeat(ResourceType.NONE, infos().Globals.NUM_WHOLESALE_SLOTS).ToList();
                    maeWholesaleSlotResourceNext = Enumerable.Repeat(ResourceType.NONE, infos().Globals.NUM_WHOLESALE_SLOTS).ToList();

                    for (GameOptionType eLoopGameOption = 0; eLoopGameOption < (infos().gameOptionsNum()); eLoopGameOption++)
                    {
                        mabGameOptions.Add(gameSettings().mabGameOptions[(int)eLoopGameOption] || infos().rulesSet(getRulesSet()).mabGameOption[(int)eLoopGameOption]);
                    }
                }

                using (new UnityProfileScope("GameServer.initializeSystems"))
                {
                    mMarket = Globals.Factory.createMarketServer(this);
                    marketServer().init(this);

                    mGameEvents = Globals.Factory.createGameEventsServer();
                    mStats = Globals.Factory.createStatsServer(this);

                    mConditionManager = Globals.Factory.createConditionManagerServer(this);
                    conditionManagerServer().init(this, infos().gameSetup(gameSettings().meGameSetupType).winConditions, infos().gameSetup(gameSettings().meGameSetupType).loseConditions);

                    mMap = Globals.Factory.createMapServer(this);
                    mapServer().init(this);

                    initServerVariables(false);

                    GameServer.fillInvalidHQs(mabInvalidHQ, gameSettings(), infos());
                }

                if (getColony() == ColonyType.NONE)
                {
                    meColony = getColonyTypeFromSeed(getSeed());
                }

                if (isGameOption(GameOptionType.SEVEN_SOLS))
                {
                    if (isCampaign())
                    {
                        meSevenSols = Globals.Campaign.getLevelSevenSols(getLevel());
                    }
                    else
                    {
                        meSevenSols = SevenSolsType.COLONY;
                    }

                    if (getSevenSols() == SevenSolsType.WHOLESALE)
                    {
                        int iNumResets = random().Next(5);

                        for (int iLoopSlot = 0; iLoopSlot < infos().Globals.NUM_WHOLESALE_SLOTS; iLoopSlot++)
                        {
                            setWholesaleSlotResets(iLoopSlot, (iLoopSlot < iNumResets));
                            resetWholesaleSlotResource(iLoopSlot);
                        }
                    }
                }

                if (getLocation() == LocationType.EUROPA)
                    initImportSlots();

                using (new UnityProfileScope("GameServer.organizePlayers"))
                {
                    // Number of humans = number of players - number of AI players
                    int numHumans = getOriginalHumanPlayers();

                    string[] names = { "Asimov", "Clarke", "Bradbury", "Herbert", "Niven", "Wells", "Burroughs", "Ellison" }; // move to xml

                    List<PlayerColorType> aPlayerColors = new List<PlayerColorType>();
                    List<InfoPlayerColor> aSortedPlayerColors = Globals.Infos.playerColors().OrderBy(color => color.miPriority).ToList();
                    aSortedPlayerColors.ForEach(color => aPlayerColors.Add(color.meType));

                    List<string> aPseudonyms = Enumerable.Range(0, (int)getNumPlayers()).Select(o => names[o]).ToList();
                    List<TeamType> aTeams = Enumerable.Range(0, getNumTeams()).Cast<TeamType>().ToList();

                    maTeamNames = new string[] { "Zeus", "Hera", "Poseidon", "Artemis", "Athena", "Hermes", "Hades", "Hephaestus" }.ToList();

                    if (isGameOption(GameOptionType.HIDDEN_IDENTITIES))
                    {
                        System.Random pRandom = new CrossPlatformRandom(getSeed());

                        int i = (int)getNumPlayers();
                        while (i > 1)
                        {
                            int iTargetIndex = pRandom.Next(i--);
                            ListUtilities.Swap(aPlayerColors, i, iTargetIndex);
                            ListUtilities.Swap(aPseudonyms, i, iTargetIndex);
                        }

                        i = aTeams.Count;
                        while (i > 1)
                        {
                            int iTargetIndex = pRandom.Next(i--);
                            ListUtilities.Swap(aTeams, i, iTargetIndex);
                        }
                    }

                    List<bool> abCorporationAdded = ((isCampaign()) ? Enumerable.Repeat(false, Globals.Campaign.campaignMode().miStartingCorps).ToList() : null);
                    List<bool> abPersonalityAdded = infos().personalities().Select(i => false).ToList();

                    maPlayers = Enumerable.Range(0, (int)(getNumPlayers())).Select(o => (PlayerClient)null).ToList();

                    for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
                    {
                        bool bHuman = ((int)eLoopPlayer < numHumans);

                        PlayerSettings playerSettings = pSettings.playerSlots[(int)eLoopPlayer];
                        TeamType eTeam = aTeams[playerSettings.Team];
                        HandicapType eHandicap = playerSettings.Handicap;
                        GenderType eGender = playerSettings.Gender;
                        ArtPackList artPackList = playerSettings.ArtPackList;
                        string zName = playerSettings.Name;
                        string zPseudonym = aPseudonyms[(int)eLoopPlayer];

                        if (bHuman)
                        {
                            if (infos().rulesSet(getRulesSet()).meHandicap != HandicapType.NONE)
                            {
                                eHandicap = infos().rulesSet(getRulesSet()).meHandicap;
                            }
                        }

                        CorporationType eCorporation = CorporationType.NONE;
                        PersonalityType ePersonality = PersonalityType.NONE;

                        if (isCampaign())
                        {
                            foreach (Corporation pLoopCorporation in Globals.Campaign.getCorporations())
                            {
                                if (bHuman == (pLoopCorporation.meID == CorporationType.HUMAN))
                                {
                                    if (!abCorporationAdded[(int)pLoopCorporation.meID])
                                    {
                                        if (pLoopCorporation.meCurrentLevel == getLevel())
                                        {
                                            eCorporation = pLoopCorporation.meID;
                                            ePersonality = pLoopCorporation.mePersonality;

                                            abCorporationAdded[(int)eCorporation] = true;

                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        if (ePersonality == PersonalityType.NONE)
                        {
                            if (bHuman)
                            {
                                ePersonality = infos().Globals.DEFAULT_PERSONALITY_HUMAN;
                            }
                            else
                            {
                                ePersonality = getBestPersonality(eLoopPlayer, abPersonalityAdded);
                            }
                        }

                        //replace AI gender and name
                        if (!bHuman)
                        {
                            eGender = infos().character(infos().personality(ePersonality).meCharacter).meGender;
                            zName = TEXT(infos().character(infos().personality(ePersonality).meCharacter).meName);
                        }

                        PlayerServer pLoopPlayer = Globals.Factory.createPlayerServer(this);
                        pLoopPlayer.init(this, eLoopPlayer, eTeam, ePersonality, eCorporation, eHandicap, eGender, artPackList, bHuman, zName, zPseudonym);
                        maPlayers[(int)eLoopPlayer] = pLoopPlayer;
                    }

                    List<bool> abPlayerColorAdded = infos().playerColors().Select(i => false).ToList();

                    for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
                    {
                        PlayerColorType ePlayerColor = aPlayerColors[(int)eLoopPlayer];

                        if (!(isGameOption(GameOptionType.HIDDEN_IDENTITIES)))
                        {
                            if (isCampaign())
                            {
                                ePlayerColor = playerClient(eLoopPlayer).corporation().mePlayerColor;
                            }
                            else
                            {
                                ePlayerColor = infos().character(playerServer(eLoopPlayer).getCharacter()).mePlayerColor;
                            }
                        }

                        if (!abPlayerColorAdded[(int)ePlayerColor])
                        {
                            abPlayerColorAdded[(int)ePlayerColor] = true;

                            playerServer(eLoopPlayer).setPlayerColor(ePlayerColor);
                        }
                    }

                    for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
                    {
                        if (playerServer(eLoopPlayer).getPlayerColorType() == PlayerColorType.NONE)
                        {
                            foreach (PlayerColorType eLoopPlayerColor in aPlayerColors)
                            {
                                if (!abPlayerColorAdded[(int)eLoopPlayerColor])
                                {
                                    abPlayerColorAdded[(int)eLoopPlayerColor] = true;

                                    playerServer(eLoopPlayer).setPlayerColor(eLoopPlayerColor);
                                    break;
                                }
                            }
                        }
                    }
                }

                if (isTeamGame())
                {
                    for (PlayerType ePlayer = 0; ePlayer < getNumPlayers(); ePlayer++)
                    {
                        playerServer(ePlayer).setSuffix(TEXT("TEXT_PLAYER_SUFFIX", getTeamName(playerServer(ePlayer).getTeam()).ToText()));
                    }
                }

                {
                    int iHandicap = 0;
                    int iCount = 0;

                    foreach (PlayerClient pLoopPlayer in getPlayerClientAll())
                    {
                        if (pLoopPlayer.isHuman())
                        {
                            iHandicap += (int)(pLoopPlayer.getHandicap());
                            iCount++;
                        }
                    }

                    if (iCount > 0)
                    {
                        meHumanHandicap = (HandicapType)(iHandicap / iCount);
                    }
                    else
                    {
                        meHumanHandicap = Globals.Infos.Globals.DEFAULT_HANDICAP;
                    }
                }

                for (BuildingType eLoopBuilding = 0; eLoopBuilding < infos().buildingsNum(); eLoopBuilding++)
                {
                    int iModifier = 0;
                    int iCount = 0;

                    for (int iLoopHour = 0; iLoopHour < infos().location(getLocation()).miHoursPerDay; iLoopHour++)
                    {
                        int iTurns = (Utils.getHourMinutes(iLoopHour, getLocation()) / infos().Globals.MINUTES_PER_TURN);

                        if (buildingClosed(iLoopHour, eLoopBuilding, PlayerType.NONE))
                        {
                            iModifier += (-100 * iTurns);
                        }

                        iCount += iTurns;
                    }

                    iModifier /= iCount;

                    {
                        EventStateType eEventStateLevel = getEventStateLevel();

                        if (eEventStateLevel != EventStateType.NONE)
                        {
                            iModifier += infos().eventState(eEventStateLevel).miProductionModifier + infos().eventState(eEventStateLevel).maiBuildingClassModifier[(int)(infos().building(eLoopBuilding).meClass)];
                        }
                    }

                    maiBuildingAverageModifier[(int)eLoopBuilding] = iModifier;
                }

                TerrainGenerator terrainGenerator = new TerrainGenerator();

                using (new UnityProfileScope("GameServer.generateTerrain"))
                {
                    mapServer().initTiles();
                    refreshCachedValues();

                    mapServer().setMapLatitude(gameSettings().meLatitude, this);

                    if (pSettings.mzMap.Length > 0)
                    {
                        terrainGenerator.readTerrain(this);
                    }
                    else
                    {
                        terrainGenerator.generateRandomTerrain(this);

                        terrainGenerator.postProcessTileHeights(this);

                        terrainGenerator.PlaceCraterPrefabs(this);

                        terrainGenerator.generateIce(this);
                    }

                    terrainGenerator.generateWind(this);

                    mMap.randomizeTerrainThicknesses();

                    foreach (TileServer pLoopTile in tileServerAll())
                    {
                        IceType eIce = pLoopTile.getIce();

                        if (eIce != IceType.NONE)
                        {
                            maiIceCount[(int)eIce]++;
                            maiIceTiles.Add(pLoopTile.getID());
                        }
                    }

                    for (BuildingType eLoopBuilding = 0; eLoopBuilding < infos().buildingsNum(); eLoopBuilding++)
                    {
                        maaiBuildingResourceOutput.Add(new List<int>());

                        for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                        {
                            int iValue = 0;

                            {
                                TerrainType eTerrainRate = infos().building(eLoopBuilding).meTerrainRate;

                                if (eTerrainRate != TerrainType.NONE)
                                {
                                    iValue += infos().terrain(eTerrainRate).maiResourceRate[(int)eLoopResource];
                                }
                            }

                            if (pInfos.building(eLoopBuilding).mbIce)
                            {
                                bool bFoundIce = false;

                                for (IceType eLoopIce = 0; eLoopIce < pInfos.icesNum(); eLoopIce++)
                                {
                                    if (pInfos.ice(eLoopIce).maiAverageResourceRate[(int)eLoopResource] > 0)
                                    {
                                        if (getIceCount(eLoopIce) > 0)
                                        {
                                            bFoundIce = true;
                                            break;
                                        }
                                    }
                                }

                                if (bFoundIce)
                                {
                                    iValue += infos().building(eLoopBuilding).maiResourceOutput[(int)eLoopResource];
                                }
                            }
                            else
                            {
                                iValue += infos().building(eLoopBuilding).maiResourceOutput[(int)eLoopResource];
                            }

                            maaiBuildingResourceOutput[(int)eLoopBuilding].Add(iValue);
                        }
                    }

                    initServerResourceInputs();

                    doAreas();

                    doTerrainCount();

                    doTerrainRegions();
                }

                using (new UnityProfileScope("GameServer.initPost"))
                {
                    if (mapClient().getHasResourceInfo())
                    {
                        terrainGenerator.loadResources(this);
                    }
                    else
                    {
                        terrainGenerator.generateResources(this);
                    }

                    doModules();

                    marketServer().initPost();

                    for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
                    {
                        playerServer(eLoopPlayer).initPost();
                    }

                    doStarts();

                    if (infos().rulesSet(getRulesSet()).mbBlackMarketAvailable)
                        doBlackMarkets();

                    using (new UnityProfileScope("GameServer.unavailableBuildings"))
                    {
                        foreach (InfoBuilding pLoopBuilding in infos().buildings())
                        {
                            bool bUnavailable = false;

                            if (!bUnavailable)
                            {
                                if (pLoopBuilding.mabLocationInvalid[(int)getLocation()])
                                {
                                    bUnavailable = true;
                                }
                            }

                            if (!bUnavailable)
                            {
                                if (infos().rulesSet(getRulesSet()).mabBuildingUnavailable[pLoopBuilding.miType])
                                {
                                    bUnavailable = true;
                                }
                            }

                            if (!bUnavailable)
                            {
                                if (getColonyClass() != ColonyClassType.NONE)
                                {
                                    if (infos().colonyClass(getColonyClass()).mabBuildingClassInvalid[(int)(pLoopBuilding.meClass)])
                                    {
                                        bUnavailable = true;
                                    }
                                }
                            }

                            if (!bUnavailable)
                            {
                                if (pLoopBuilding.mbGeothermal)
                                {
                                    bool bValid = false;

                                    foreach (TileServer pLoopTile in tileServerAll())
                                    {
                                        if (pLoopTile.isGeothermal())
                                        {
                                            bValid = true;
                                            break;
                                        }
                                    }

                                    if (!bValid)
                                    {
                                        bUnavailable = true;
                                    }
                                }
                            }

                            if (!bUnavailable)
                            {
                                if (pLoopBuilding.mbIce)
                                {
                                    bool bFoundIce = false;

                                    for (IceType eLoopIce = 0; eLoopIce < infos().icesNum(); eLoopIce++)
                                    {
                                        if (getIceCount(eLoopIce) > 0)
                                        {
                                            bFoundIce = true;
                                            break;
                                        }
                                    }

                                    if (!bFoundIce)
                                    {
                                        bUnavailable = true;
                                    }
                                }
                            }

                            if (!bUnavailable)
                            {
                                if (Utils.isBuildingMiningAny(pLoopBuilding.meType))
                                {
                                    bool bValid = false;

                                    for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                    {
                                        if (infos().building(pLoopBuilding.meType).maiResourceMining[(int)eLoopResource] > 0)
                                        {
                                            foreach (TileServer pLoopTile in tileServerAll())
                                            {
                                                if (pLoopTile.getResourceLevel(eLoopResource, false) > ResourceLevelType.NONE)
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

                                    if (!bValid)
                                    {
                                        bUnavailable = true;
                                    }
                                }
                            }

                            if (bUnavailable)
                            {
                                setBuildingUnavailable(pLoopBuilding.meType, true);
                            }
                        }
                    }

                    if (isGameOption(GameOptionType.REVEAL_MAP))
                    {
                        setFoundMoney(infos().mapSize(getMapSize()).miFoundMoney);

                        if (!isGameOption(GameOptionType.ALL_HQS))
                        {
                            changeFoundMoney(infos().Globals.NO_ALL_HQS_FOUND_MONEY);
                        }
                    }

                    updatePlayerLists();

                    makeAllDirty();
                }
            }
        }

        void initImportSlots()
        {
            maiImportValueModifier = Enumerable.Repeat(100, (int)infos().resourcesNum()).ToList();
            maiExportValueModifier = Enumerable.Repeat(100, (int)infos().resourcesNum()).ToList();

            foreach (InfoResource infoResource in Globals.Infos.resources())
            {
                if (infoResource.miExportValue > 0)
                    maeImportPaymentResource.Add(infoResource.meType);
            }
            foreach (InfoResource infoResource in Globals.Infos.resources())
            {
                if (infoResource.miImportValue > 0)
                    maeImportResource.AddRange(Enumerable.Repeat(infoResource.meType, maeImportPaymentResource.Count));
            }

            for (int iSlot = 0; iSlot < maeImportResource.Count; iSlot++)
            {
                maiImportCost.Add(100 * infos().resource(getImportResourcePayment(iSlot)).miExportValue / infos().resource(maeImportResource[iSlot]).miImportValue);
            }
        }

        protected virtual void initServerVariables(bool bInputs)
        {
            using (new UnityProfileScope("GameServer.initServerVariables"))
            {
                mRandom = new CrossPlatformRandom(getSeed());
                mRandomEvent = new CrossPlatformRandom(getSeed());
                mRandomColony = new CrossPlatformRandom(getSeed());
                mRandomAuction = new CrossPlatformRandom(getSeed());
                mRandomAuctionPatent = new CrossPlatformRandom(getSeed());
                mRandomAuctionSabotage = new CrossPlatformRandom(getSeed());
                mRandomAuctionTileBuilding = new CrossPlatformRandom(getSeed());
                mRandomAuctionPerk = new CrossPlatformRandom(getSeed());
                mRandomMisc = new CrossPlatformRandom(getSeed());

                setLastUpdateTimeToNow();

                miModuleRevealTime = infos().Globals.MODULE_REVEAL_TIME;

                maiWholesaleSlotDelay = Enumerable.Repeat(0, infos().Globals.NUM_WHOLESALE_SLOTS).ToList();

                for(ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    maiResourceTiles.Add(new HashSet<int>());
                    maiResourceAdjacentTiles.Add(new HashSet<int>());
                }

                for (BuildingType eLoopBuilding = 0; eLoopBuilding < infos().buildingsNum(); eLoopBuilding++)
                {
                    bool bFound = false;

                    for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                    {
                        if (infos().building(eLoopBuilding).maiResourceInput[(int)eLoopResource] > 0)
                        {
                            bFound = true;
                            break;
                        }
                    }

                    mabBuildingHasInput.Add(bFound);
                }

                for (EventGameType eLoopEventGame = 0; eLoopEventGame < infos().eventGamesNum(); eLoopEventGame++)
                {
                    if (!(infos().eventGame(eLoopEventGame).mabLocationInvalid[(int)getLocation()]))
                    {
                        if (!isCampaign() || !(Globals.Campaign.campaignMode().mabEventGameInvalid[(int)eLoopEventGame]))
                        {
                            for (int iI = 0; iI < infos().eventGame(eLoopEventGame).miDieRolls; iI++)
                            {
                                maeEventGameDie.Add(eLoopEventGame);
                            }
                        }
                    }
                }

                if (isCampaign())
                {
                    EventGameType eEventGame = Globals.Campaign.getEventGame();

                    if (eEventGame != EventGameType.NONE)
                    {
                        if (!(infos().eventGame(eEventGame).mabLocationInvalid[(int)getLocation()]))
                        {
                            int iCount = (maeEventGameDie.Count + 1);

                            for (int iI = 0; iI < iCount; iI++)
                            {
                                maeEventGameDie.Add(eEventGame);
                            }
                        }
                    }
                }

                if (bInputs)
                {
                    initServerResourceInputs();
                }
            }
        }

        protected virtual void initServerResourceInputs()
        {
            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                maaabResourceInput.Add(new List<List<bool>>());

                for (ResourceType eResourceInput = 0; eResourceInput < infos().resourcesNum(); eResourceInput++)
                {
                    maaabResourceInput[(int)eLoopResource].Add(new List<bool>());

                    for (HQType eLoopHQ = 0; eLoopHQ < infos().HQsNum(); eLoopHQ++)
                    {
                        maaabResourceInput[(int)eLoopResource][(int)eResourceInput].Add(false);
                    }
                }
            }

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                for (ResourceType eResourceInput = 0; eResourceInput < infos().resourcesNum(); eResourceInput++)
                {
                    for (HQType eLoopHQ = 0; eLoopHQ < infos().HQsNum(); eLoopHQ++)
                    {
                        for (BuildingType eLoopBuilding = 0; eLoopBuilding < infos().buildingsNum(); eLoopBuilding++)
                        {
                            if (Utils.isBuildingValid(eLoopBuilding, eLoopHQ))
                            {
                                if (getBuildingResourceOutput(eLoopBuilding, eLoopResource) > 0)
                                {
                                    if (infos().building(eLoopBuilding).maiResourceInput[(int)eResourceInput] > 0)
                                    {
                                        maaabResourceInput[(int)eLoopResource][(int)eResourceInput][(int)eLoopHQ] = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            {
                while (true)
                {
                    bool bFound = false;

                    for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                    {
                        for (ResourceType eResourceInput = 0; eResourceInput < infos().resourcesNum(); eResourceInput++)
                        {
                            for (HQType eLoopHQ = 0; eLoopHQ < infos().HQsNum(); eLoopHQ++)
                            {
                                if (maaabResourceInput[(int)eLoopResource][(int)eResourceInput][(int)eLoopHQ])
                                {
                                    for (ResourceType eResourceInputInput = 0; eResourceInputInput < infos().resourcesNum(); eResourceInputInput++)
                                    {
                                        if (maaabResourceInput[(int)eResourceInput][(int)eResourceInputInput][(int)eLoopHQ])
                                        {
                                            if (!maaabResourceInput[(int)eLoopResource][(int)eResourceInputInput][(int)eLoopHQ])
                                            {
                                                maaabResourceInput[(int)eLoopResource][(int)eResourceInputInput][(int)eLoopHQ] = true;
                                                bFound = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (!bFound)
                    {
                        break;
                    }
                }
            }
        }

        public HashSet<int> getResourceTileList(ResourceType eResource)
        {
            return maiResourceTiles[(int)eResource];
        }

        public void addResourceTileToList(TileServer pTile, ResourceType eResource) //use when resources are created
        {
            maiResourceTiles[(int)eResource].Add(pTile.getID());
            addResourceAdjacentTilesToList(pTile, eResource);
        }

        public void addResourceTileToList(int tileID, ResourceType eResource) //for terrain with resource rates
        {
            if (tileID != -1)
                maiResourceTiles[(int)eResource].Add(tileID);
        }

        public HashSet<int> getResourceAdjacentTileList(ResourceType eResource)
        {
            return maiResourceAdjacentTiles[(int)eResource];
        }

        private void addResourceAdjacentTilesToList(TileServer pTile, ResourceType eResource)
        {
            for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
            {
                TileServer adjacentTile = tileServerAdjacent(pTile, eDirection);

                if (adjacentTile != null)
                {
                    int adjacentTileID = adjacentTile.getID();
                    if (!maiResourceTiles[(int)eResource].Contains(adjacentTileID) && !maiResourceAdjacentTiles[(int)eResource].Contains(adjacentTileID) && adjacentTile.usable())
                    {
                        maiResourceAdjacentTiles[(int)eResource].Add(adjacentTileID);
                    }
                }
            }
        }

        public List<int> getGeothermalList()
        {
            return maiGeothermalTiles;
        }

        public void addGeothermalTileToList(int tileID)
        {
            if (tileID != -1)
                maiGeothermalTiles.Add(tileID);
        }

        public HashSet<int> getWindTileList()
        {
            return maiWindTiles;
        }

        public void addWindTileToList(int tileID)
        {
            if (tileID != -1)
                maiWindTiles.Add(tileID);
        }

        public HashSet<int> getColonyAdjacentTileList()
        {
            return maiColonyAdjacentTiles;
        }

        public HashSet<int> getIceTiles()
        {
            return maiIceTiles;
        }

        public List<int> getCaveTiles()
        {
            return maiCaveTiles;
        }

        public void addCaveTile(int tileID)
        {
            if (tileID != -1)
                maiCaveTiles.Add(tileID);
        }

        public void setColonyPopulationType(PopulationBarType eType)
        {
            mColonySettings.BarType = eType;
            makeDirty(GameDirtyType.ColonySettings);
        }
        public void setColonyModuleType(ModuleInfoType eType)
        {
            mColonySettings.InfoType = eType;
            makeDirty(GameDirtyType.ColonySettings);
        }
        public void setColonyCap(int iCap)
        {
            miColonyCap = iCap;
            makeDirty(GameDirtyType.miColonyCap);
        }

        public void setShipmentRequest(ResourceType shipmentResource, int shipmentAmount, int shipmentDeadline, int shipmentPenalty, int shipmentInterval)
        {
            scenarioShipmentRequest().shipmentResource = shipmentResource;
            scenarioShipmentRequest().shipmentAmount = shipmentAmount;
            scenarioShipmentRequest().shipmentDeadline = shipmentDeadline;
            scenarioShipmentRequest().shipmentPenalty = shipmentPenalty;
            scenarioShipmentRequest().shipmentInterval = shipmentInterval;
            makeDirty(GameDirtyType.ShipmentRequest);
        }

        protected virtual void SerializeServer(object stream, int compatibilityNumber)
        {
            SimplifyIO.Data(stream, ref mbPlayGame, "PlayGame");
            SimplifyIO.Data(stream, ref miNextTileGroupID, "NextTileGroupID");
            SimplifyIO.Data(stream, ref miNextModuleID, "NextModuleID");
            SimplifyIO.Data(stream, ref miNextHQID, "NextHQID");
            SimplifyIO.Data(stream, ref miNextConstructionID, "NextConstructionID");
            SimplifyIO.Data(stream, ref miNextBuildingID, "NextBuildingID");
            SimplifyIO.Data(stream, ref miNextUnitID, "NextUnitID");
            SimplifyIO.Data(stream, ref miLastUpdateTimeMS, "LastUpdateTimeMS");
            SimplifyIO.Data(stream, ref miModuleRevealTime, "ModuleRevealTime");

            SimplifyIO.Data(stream, ref mRandom, "Random");
            SimplifyIO.Data(stream, ref mRandomEvent, "RandomEvent");
            SimplifyIO.Data(stream, ref mRandomColony, "RandomColony");
            SimplifyIO.Data(stream, ref mRandomAuction, "RandomAuction");
            SimplifyIO.Data(stream, ref mRandomAuctionPatent, "RandomAuctionPatent");
            SimplifyIO.Data(stream, ref mRandomAuctionSabotage, "RandomAuctionSabotage");
            SimplifyIO.Data(stream, ref mRandomAuctionTileBuilding, "RandomAuctionTileBuilding");
            SimplifyIO.Data(stream, ref mRandomAuctionPerk, "RandomAuctionPerk");
            SimplifyIO.Data(stream, ref mRandomMisc, "mRandomMisc");

            SimplifyIO.Data(stream, ref maiAreaTiles, "AreaTiles");
            SimplifyIO.Data(stream, ref maiWholesaleSlotDelay, "WholesaleSlotDelay");
            SimplifyIO.Data(stream, ref maiImportValueModifier, "ImportValueModifier");
            SimplifyIO.Data(stream, ref maiExportValueModifier, "ExportValueModifier");

            SimplifyIO.Data(stream, ref mabBuildingHasInput, "BuildingHasInput");

            SimplifyIO.Data(stream, ref maeEventGameDie, "EventGameDie");

            SimplifyIO.Data(stream, ref maaabResourceInput, "ResourceInput");

            SimplifyIOGame.Data(stream, ref mTileGroupDictionary, "TileGroupDictionary", this);

            //Tile subsets to reduce the number of tiles the AI iterates over, could theoretically be generated on load instead of serialized
            SimplifyIO.Data(stream, ref maiResourceTiles, "ResourceTiles");
            SimplifyIO.Data(stream, ref maiResourceAdjacentTiles, "ResourceAdjacentTiles");
            SimplifyIO.Data(stream, ref maiWindTiles, "WindTiles");
            SimplifyIO.Data(stream, ref maiColonyAdjacentTiles, "ColonyAdjacentTiles");
            SimplifyIO.Data(stream, ref maiIceTiles, "IceTiles");
            SimplifyIO.Data(stream, ref maiCaveTiles, "CaveTiles");
        }

        public virtual void writeServerValues(BinaryWriter stream, int compatibilityNumber)
        {
            writeClientValues(stream, true, false, compatibilityNumber);
            SerializeServer(stream, compatibilityNumber);
        }

        public virtual void readServerValues(BinaryReader stream, int compatibilityNumber)
        {
            mInfos = Globals.Infos;
            readClientValues(stream, true, false, compatibilityNumber);
            SerializeServer(stream, compatibilityNumber);
            updateTerrainCache();
        }

        public virtual void rebuildFromClient(GameClient pGameClient)
        {
            initServerVariables(true);

            foreach (KeyValuePair<int, ModuleClient> pair in pGameClient.getModuleDictionary())
            {
                miNextModuleID = Math.Max(miNextModuleID, (pair.Key + 1));
            }
            foreach (KeyValuePair<int, HQClient> pair in pGameClient.getHQDictionary())
            {
                miNextHQID = Math.Max(miNextHQID, (pair.Key + 1));
            }
            foreach (KeyValuePair<int, ConstructionClient> pair in pGameClient.getConstructionDictionary())
            {
                miNextConstructionID = Math.Max(miNextConstructionID, (pair.Key + 1));
            }
            foreach (KeyValuePair<int, BuildingClient> pair in pGameClient.getBuildingDictionary())
            {
                miNextBuildingID = Math.Max(miNextBuildingID, (pair.Key + 1));
            }
            foreach (KeyValuePair<int, UnitClient> pair in pGameClient.getUnitDictionary())
            {
                miNextUnitID = Math.Max(miNextUnitID, (pair.Key + 1));
            }

            marketServer().rebuildFromClient();

            foreach (PlayerServer pLoopPlayer in getPlayerServerAll())
            {
                pLoopPlayer.rebuildFromClient(pGameClient.playerClient(pLoopPlayer.getPlayer()));
            }

            foreach (TileServer pLoopTile in tileServerAll())
            {
                pLoopTile.rebuildFromClient();
            }

            foreach (TileServer pLoopTile in tileServerAll())
            {
                pLoopTile.updateTileGroup();
            }

            updateTerrainCache();

            updateConnectedToHQ();

            doAreas();

            AI_calculateFoundValues();
            AI_calculateFoundMinimums();

            //initAICacheTileLists();
        }

        public virtual void updateTerrainCache()
        {
            using (new UnityProfileScope("GameServer.updateCachedValues"))
            {
                foreach (TileServer pLoopTile in tileServerAll())
                {
                    pLoopTile.cacheTerrainInfo();
                }
            }
        }

        protected virtual PersonalityType getBestPersonality(PlayerType ePlayer, List<bool> abPersonalityAdded)
        {
            System.Random random = new CrossPlatformRandom(getSeed());

            PersonalityType eBestPersonality = PersonalityType.NONE;
            int iBestValue = 0;

            for (int iPass = 0; iPass < 2; iPass++)
            {
                for (PersonalityType eLoopPersonality = 0; eLoopPersonality < infos().personalitiesNum(); eLoopPersonality++)
                {
                    if (eLoopPersonality != infos().Globals.DEFAULT_PERSONALITY_HUMAN)
                    {
                        if (!(infos().personality(eLoopPersonality).mbIoDLC) || (Globals.AppInfo.OwnsDLCIo || Globals.AppInfo.IsInternalMod))
                        {
                            if (!abPersonalityAdded[(int)eLoopPersonality])
                            {
                                if ((iPass > 0) || infos().rulesSet(getRulesSet()).mabPersonalityValidAI[(int)eLoopPersonality])
                                {
                                    int iValue = random.Next(1000) + 1;
                                    if (iValue > iBestValue)
                                    {
                                        eBestPersonality = eLoopPersonality;
                                        iBestValue = iValue;
                                    }
                                }
                            }
                        }
                    }
                }

                if (eBestPersonality != PersonalityType.NONE)
                {
                    break;
                }
            }

            if (eBestPersonality != PersonalityType.NONE)
            {
                abPersonalityAdded[(int)eBestPersonality] = true;
            }

            return eBestPersonality;
        }

        public virtual int countRealBuildingOrderCount(OrderType eOrder)
        {
            int iCount = 0;

            for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
            {
                iCount += playerServer(eLoopPlayer).getRealBuildingOrderCount(eOrder);
            }

            return iCount;
        }

        public virtual int countRealBuildingCountEntertainment()
        {
            int iCount = 0;

            for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
            {
                iCount += playerServer(eLoopPlayer).getRealBuildingCountEntertainment();
            }

            return iCount;
        }

        void updateImportCost()
        {
            for (int iIndex = 0; iIndex < getNumImportSlots(); iIndex++)
            {
                maiImportCost[iIndex] = 100 * (infos().resource(getImportResourcePayment(iIndex)).miExportValue *
                    maiExportValueModifier[(int)getImportResourcePayment(iIndex)]) / (infos().resource(getImportSlotResource(iIndex)).miImportValue *
                    maiImportValueModifier[(int)getImportSlotResource(iIndex)]);
            }
            makeDirty(GameDirtyType.maiImportCost);
        }

        public void processImport(int iSlot)
        {
            maiImportValueModifier[(int)maeImportResource[iSlot]] -= maiImportValueModifier[(int)maeImportResource[iSlot]] / 10;
            maiExportValueModifier[(int)getImportResourcePayment(iSlot)] += maiExportValueModifier[(int)getImportResourcePayment(iSlot)] / 10;

            updateImportCost();
        }

        public virtual void handleAction(GameAction pAction)
        {
            using (new UnityProfileScope("Game::handleAction"))
            {
                PlayerType eActionPlayer = pAction.getPlayer();
                PlayerServer pActionPlayer = playerServer(eActionPlayer);

                if (pActionPlayer.isWinEligible())
                {
                    switch (pAction.getItem())
                    {
                        case ItemType.TRADE:
                            {
                                ResourceType eResource = (ResourceType)(pAction.removeFirstValue());
                                int iQuantity = pAction.removeFirstValue();
                                pActionPlayer.trade(eResource, iQuantity, true);
                                break;
                            }

                        case ItemType.STOCK:
                            {
                                PlayerType ePlayer = (PlayerType)(pAction.removeFirstValue());
                                int iQuantity = pAction.removeFirstValue();
                                if (iQuantity == 1)
                                {
                                    pActionPlayer.buyShares(ePlayer);
                                }
                                else if (iQuantity == -1)
                                {
                                    pActionPlayer.sellShares(ePlayer);
                                }
                                break;
                            }

                        case ItemType.BUYOUT:
                            {
                                PlayerType ePlayer = (PlayerType)(pAction.removeFirstValue());
                                pActionPlayer.buyout(ePlayer);
                                break;
                            }

                        case ItemType.PAY_DEBT:
                            {
                                bool bAll = (pAction.removeFirstValue() == 1);
                                if (bAll)
                                {
                                    pActionPlayer.payDebtAll();
                                }
                                else
                                {
                                    pActionPlayer.payDebt();
                                }
                                break;
                            }

                        case ItemType.AUTO_PAY_DEBT:
                            {
                                pActionPlayer.toggleAutoPayDebt();
                                break;
                            }

                        case ItemType.SCAN:
                            {
                                TileServer pTile = tileServer(pAction.removeFirstValue());
                                pActionPlayer.scan(pTile);
                                break;
                            }

                        case ItemType.SABOTAGE:
                            {
                                TileServer pTile = tileServer(pAction.removeFirstValue());
                                SabotageType eSabotage = (SabotageType)(pAction.removeFirstValue());
                                pActionPlayer.sabotage(pTile, eSabotage);
                                break;
                            }

                        case ItemType.FOUND:
                            {
                                TileServer pTile = tileServer(pAction.removeFirstValue());
                                HQType eHQ = (HQType)(pAction.removeFirstValue());
                                pActionPlayer.found(pTile, eHQ);
                                break;
                            }

                        case ItemType.UPGRADE:
                            {
                                pActionPlayer.upgrade();
                                break;
                            }

                        case ItemType.UPGRADE_BUILDING:
                            {
                                int iID = pAction.removeFirstValue();
                                BuildingServer building = (BuildingServer)getBuildingDictionary()[iID];
                                if (building.canUpgrade(true))
                                    building.upgrade();
                                break;
                            }

                        case ItemType.PING_TILE:
                            {
                                TileServer pTile = tileServer(pAction.removeFirstValue());
                                gameEventsServer().AddPing(eActionPlayer, pTile.getID());
                                break;
                            }

                        case ItemType.CLAIM_TILE:
                            {
                                TileServer pTile = tileServer(pAction.removeFirstValue());
                                pActionPlayer.startClaim(pTile);
                                break;
                            }

                        case ItemType.RETURN_CLAIM:
                            {
                                TileServer pTile = tileServer(pAction.removeFirstValue());
                                pActionPlayer.returnClaim(pTile, false);
                                break;
                            }

                        case ItemType.CANCEL_CONSTRUCT:
                            {
                                TileServer pTile = tileServer(pAction.removeFirstValue());
                                pActionPlayer.cancelConstruct(pTile);
                                break;
                            }

                        case ItemType.CONSTRUCT_BUILDING:
                            {
                                TileServer pTile = tileServer(pAction.removeFirstValue());
                                BuildingType eBuilding = (BuildingType)(pAction.removeFirstValue());
                                if (pActionPlayer.construct(pTile, eBuilding))
                                {
                                    gameEventsServer().AddConstructionPlaced(eActionPlayer, eBuilding, pTile.getID());
                                }
                                break;
                            }

                        case ItemType.SELL_ALL_RESOURCES:
                            {
                                pActionPlayer.sellAllResources(true);
                                break;
                            }

                        case ItemType.TOGGLE_HOLD_RESOURCE:
                            {
                                ResourceType eResource = (ResourceType)(pAction.removeFirstValue());
                                pActionPlayer.toggleHoldResource(eResource);
                                break;
                            }

                        case ItemType.TOGGLE_SHARE_RESOURCE:
                            {
                                ResourceType eResource = (ResourceType)(pAction.removeFirstValue());
                                if (!(pActionPlayer.mustTeamShareResource(eResource)))
                                {
                                    pActionPlayer.toggleTeamShareResource(eResource);
                                }
                                break;
                            }

                        case ItemType.TOGGLE_SHARE_ALL_RESOURCES:
                            {
                                pActionPlayer.setTeamShareAllResources(Convert.ToBoolean(pAction.removeFirstValue()));
                                break;
                            }

                        case ItemType.REPAIR:
                            {
                                ConstructionServer pConstruction = constructionServer(pAction.removeFirstValue());
                                if ((pConstruction != null) && (pConstruction.getOwner() == pActionPlayer.getPlayer()))
                                {
                                    pConstruction.repair();
                                }
                                break;
                            }

                        case ItemType.SCRAP:
                            {
                                TileServer pTile = tileServer(pAction.removeFirstValue());
                                if (pTile.isBuilding())
                                {
                                    BuildingServer pBuilding = pTile.buildingServer();
                                    if ((pBuilding != null) && (pBuilding.getOwner() == pActionPlayer.getPlayer()))
                                    {
                                        pBuilding.scrap();
                                    }
                                }
                                else if (pTile.isConstruction())
                                {
                                    ConstructionServer pConstruction = pTile.constructionServer();
                                    if ((pConstruction != null) && (pConstruction.getOwner() == pActionPlayer.getPlayer()))
                                    {
                                        pConstruction.abandon();
                                    }
                                }
                                break;
                            }

                        case ItemType.SCRAP_ALL:
                            {
                                TileServer pTile = tileServer(pAction.removeFirstValue());
                                if ((pTile != null) && (pTile.getOwner() == pActionPlayer.getPlayer()))
                                {
                                    pActionPlayer.scrapAll(pTile);
                                }
                                break;
                            }

                        case ItemType.SEND_RESOURCES:
                            {
                                BuildingServer pBuilding = buildingServer(pAction.removeFirstValue());
                                if ((pBuilding != null) && (pBuilding.getOwner() == pActionPlayer.getPlayer()))
                                {
                                    pBuilding.sendResources(false);
                                }
                                break;
                            }

                        case ItemType.SEND_RESOURCES_ALL:
                            {
                                BuildingServer pBuilding = buildingServer(pAction.removeFirstValue());
                                if ((pBuilding != null) && (pBuilding.getOwner() == pActionPlayer.getPlayer()))
                                {
                                    pActionPlayer.sendResourcesBuilding(pBuilding);
                                }
                                break;
                            }

                        case ItemType.SUPPLY_BUILDING:
                            {
                                BuildingServer pBuilding = buildingServer(pAction.removeFirstValue());
                                if ((pBuilding != null) && (pBuilding.getOwner() == pActionPlayer.getPlayer()))
                                {
                                    pBuilding.supplyBuilding(true);
                                }
                                break;
                            }

                        case ItemType.TOGGLE_AUTO_OFF:
                            {
                                BuildingServer pBuilding = buildingServer(pAction.removeFirstValue());
                                if ((pBuilding != null) && (pBuilding.getOwner() == pActionPlayer.getPlayer()))
                                {
                                    pBuilding.toggleAutoOff();
                                }
                                break;
                            }

                        case ItemType.TOGGLE_AUTO_OFF_ALL:
                            {
                                BuildingServer pBuilding = buildingServer(pAction.removeFirstValue());
                                if ((pBuilding != null) && (pBuilding.getOwner() == pActionPlayer.getPlayer()))
                                {
                                    pActionPlayer.toggleAutoOffBuildings(pBuilding);
                                }
                                break;
                            }

                        case ItemType.TOGGLE_OFF:
                            {
                                BuildingServer pBuilding = buildingServer(pAction.removeFirstValue());
                                if ((pBuilding != null) && (pBuilding.getOwner() == pActionPlayer.getPlayer()))
                                {
                                    pBuilding.toggleOffManual();
                                }
                                break;
                            }

                        case ItemType.TOGGLE_OFF_ALL:
                            {
                                BuildingServer pBuilding = buildingServer(pAction.removeFirstValue());
                                if ((pBuilding != null) && (pBuilding.getOwner() == pActionPlayer.getPlayer()))
                                {
                                    pActionPlayer.toggleOnOffBuildings(pBuilding);
                                }
                                break;
                            }

                        case ItemType.TOGGLE_OFF_EVERYTHING:
                            {
                                pActionPlayer.toggleOnOffEverything();
                                break;
                            }

                        case ItemType.AUTO_SUPPLY_BUILDING_INPUT:
                            {
                                BuildingType eBuilding = (BuildingType)pAction.removeFirstValue();
                                pActionPlayer.toggleAutoSupplyBuildingInput(eBuilding);
                                break;
                            }

                        case ItemType.HOLOGRAM_BUILDING:
                            {
                                TileServer pTile = tileServer(pAction.removeFirstValue());
                                if (pTile.getOwner() == pActionPlayer.getPlayer())
                                {
                                    BuildingType eBuilding = (BuildingType)(pAction.removeFirstValue());
                                    if (pTile.getHologramBuilding() == eBuilding)
                                    {
                                        pTile.setHologramBuilding(BuildingType.NONE);
                                    }
                                    else
                                    {
                                        pTile.setHologramBuilding(eBuilding);
                                    }
                                }
                                break;
                            }

                        case ItemType.SEND_PATENT:
                            {
                                PatentType ePatent = (PatentType)(pAction.removeFirstValue());
                                PlayerType ePlayer = (PlayerType)(pAction.removeFirstValue());
                                pActionPlayer.sendPatent(ePatent, ePlayer);
                                break;
                            }

                        case ItemType.PATENT:
                            {
                                PatentType ePatent = (PatentType)(pAction.removeFirstValue());
                                pActionPlayer.patent(ePatent);
                                break;
                            }

                        case ItemType.RESEARCH:
                            {
                                TechnologyType eTechnology = (TechnologyType)(pAction.removeFirstValue());
                                pActionPlayer.research(eTechnology);
                                break;
                            }

                        case ItemType.ESPIONAGE:
                            {
                                EspionageType eEspionage = (EspionageType)(pAction.removeFirstValue());
                                pActionPlayer.espionage(eEspionage);
                                break;
                            }

                        case ItemType.LAUNCH:
                            {
                                ResourceType eResource = (ResourceType)(pAction.removeFirstValue());
                                if (pAction.removeFirstValue() == 1)
                                {
                                    pActionPlayer.toggleAutoLaunchResource(eResource);
                                }
                                else
                                {
                                    pActionPlayer.launch(eResource);
                                }
                                break;
                            }

                        case ItemType.IMPORT:
                            {
                                int iSlot = pAction.removeFirstValue();
                                PlayerServer importPlayer = playerServer(pAction.getPlayer());
                                if (importPlayer.canImport(iSlot, true))
                                {
                                    importPlayer.import(iSlot);
                                }
                                break;
                            }

                        case ItemType.AUCTION_BID:
                            {
                                int iOldBid = pAction.removeFirstValue();
                                if (iOldBid == getAuctionBid())
                                {
                                    pActionPlayer.increaseBid();
                                }
                                break;
                            }

                        case ItemType.AUCTION_SKIP:
                            {
                                pActionPlayer.setSkipAuction(true);
                                skipAuction(false);
                                break;
                            }

                        case ItemType.CANCEL_ORDER:
                            {
                                OrderType eOrder = (OrderType)(pAction.removeFirstValue());
                                int iIndex = pAction.removeFirstValue();

                                pActionPlayer.cancelOrder(eOrder, iIndex);
                                break;
                            }

                        case ItemType.BLACK_MARKET:
                            {
                                BlackMarketType eBlackMarket = (BlackMarketType)pAction.removeFirstValue();

                                if (pActionPlayer.canBlackMarket(eBlackMarket, true))
                                {
                                    pActionPlayer.blackMarket(eBlackMarket);
                                }
                                break;
                            }

                        case ItemType.TOGGLE_CHEATING:
                            {
                                PlayerType cheatingPlayer = pAction.getPlayer();
                                if (getCheatingPlayer() == cheatingPlayer)
                                {
                                    cheatingPlayer = PlayerType.NONE;
                                }

                                setCheating(cheatingPlayer);
                                break;
                            }

                        case ItemType.BUY_COLONY_STOCK:
                            {
                                pActionPlayer.buyColonyShares();
                                break;
                            }

                        case ItemType.BUY_COLONY_MODULE:
                            {
                                ModuleType eModule = (ModuleType)(pAction.removeFirstValue());
                                pActionPlayer.buyColonyModule(eModule);
                                break;
                            }

                        case ItemType.SUPPLY_WHOLESALE:
                            {
                                pActionPlayer.supplyWholesale(pAction.removeFirstValue());
                                break;
                            }

                        case ItemType.PLAYER_OPTION:
                            {
                                PlayerOptionType ePlayerOption = (PlayerOptionType)(pAction.removeFirstValue());
                                bool bValue = ((pAction.removeFirstValue() == 1) ? true : false);

                                pActionPlayer.setPlayerOption(ePlayerOption, bValue);
                                break;
                            }

                        case ItemType.CONCEDE:
                            {
                                pActionPlayer.makeConcede();
                                break;
                            }

                        case ItemType.BEAT_SOREN:
                            {
                                pActionPlayer.makeBeatSoren();
                                break;
                            }

                        case ItemType.IS_SOREN:
                            {
                                pActionPlayer.makeIsSoren();
                                break;
                            }

                        case ItemType.BEAT_ZULTAR:
                            {
                                pActionPlayer.makeBeatZultar();
                                break;
                            }

                        case ItemType.IS_ZULTAR:
                            {
                                pActionPlayer.makeIsZultar();
                                break;
                            }

                        case ItemType.PLAYER_RANK:
                            {
                                string zRank = "";

                                while (pAction.valuesLeft())
                                {
                                    zRank += Convert.ToChar(pAction.removeFirstValue());
                                }

                                pActionPlayer.setRank(zRank);
                                break;
                            }
                    }
                }
            }
        }

        protected virtual void spreadArea(TileServer pTile)
        {
            foreach (TileServer pAdjacentTile in tileServerAdjacentAll(pTile))
            {
                if (pAdjacentTile.getArea() == -1)
                {
                    if (pAdjacentTile.usable())
                    {
                        if (pAdjacentTile.getHeight() == pTile.getHeight())
                        {
                            if (infos().terrain(pAdjacentTile.getTerrain()).mbRequiredOnly == infos().terrain(pTile.getTerrain()).mbRequiredOnly)
                            {
                                pAdjacentTile.makeArea(pTile.getArea());

                                spreadArea(pAdjacentTile);
                            }
                        }
                    }
                }
            }
        }

        protected virtual void doAreas()
        {
            foreach (TileServer pLoopTile in tileServerAll())
            {
                if (pLoopTile.getArea() == -1)
                {
                    if (pLoopTile.usable())
                    {
                        pLoopTile.makeArea(newArea());

                        spreadArea(pLoopTile);
                    }
                }
            }
        }

        protected virtual void randomModule(ModuleType eModule, TileServer pTile)
        {
            TileServer pBestTile = (TileServer)getRandomModuleTile(eModule, pTile);

            if (pBestTile != null)
            {
                createModule(eModule, pBestTile);
            }
        }

        protected virtual void doModules()
        {
            using (new UnityProfileScope("GameServer.doModules"))
            {
                if (infos().rulesSet(getRulesSet()).mbNoColony)
                {
                    return;
                }

                ModuleType START_MODULE = infos().location(getLocation()).meStartModule;
                int COLONY_FLAT_RANGE = infos().Globals.COLONY_FLAT_RANGE;

                TileServer pBestTile = null;
                int iBestValue = 0;

                foreach (TileServer pLoopTile in tileServerAll())
                {
                    if (pLoopTile.getArea() != -1)
                    {
                        if (pLoopTile.canHaveModule(START_MODULE))
                        {
                            if (getAreaTiles(pLoopTile.getArea()) > 10)
                            {
                                int iValue = randomColony().Next(1000);

                                foreach (TileServer pRangeTile in tileServerRangeIterator(pLoopTile, COLONY_FLAT_RANGE))
                                {
                                    if (pRangeTile.getArea() == pLoopTile.getArea())
                                    {
                                        iValue += 100;
                                    }
                                }

                                iValue += (Math.Abs(Math.Min(pLoopTile.getX(), getMapWidth() - pLoopTile.getX()) + Math.Min(pLoopTile.getY(), getMapHeight() - pLoopTile.getY())) * 200);

                                iValue += (getAreaTiles(pLoopTile.getArea()) * 10);

                                if (iValue > iBestValue)
                                {
                                    pBestTile = pLoopTile;
                                    iBestValue = iValue;
                                }
                            }
                        }
                    }
                }

                if (pBestTile != null)
                {
                    setStartModuleTileID(pBestTile.getID());

                    createModule(START_MODULE, pBestTile);

                    int iNumModules = 0;

                    if (getSevenSols() != SevenSolsType.NONE)
                    {
                        iNumModules += infos().Globals.SEVEN_SOLS_MODULES;
                    }
                    else
                    {
                        iNumModules += ((int)(getNumPlayers()) + 1);
                    }

                    for (int i = 0; i < iNumModules; i++)
                    {
                        addModule(ModuleType.NONE, true, false);

                        if (!isCampaign() || (getColonyClass() == ColonyClassType.NONE) || !(infos().colonyClass(getColonyClass()).mbNoColonyLabor))
                        {
                            addModule(infos().colony(getColony()).meLaborModule, false, true);
                        }
                    }

                    foreach (InfoModule pLoopModule in infos().modules())
                    {
                        int iCount = 0;

                        if (isCampaign())
                        {
                            EventLevelType eEventLevel = Globals.Campaign.getLevelEvent(getLevel());

                            if (eEventLevel != EventLevelType.NONE)
                            {
                                iCount += infos().eventLevel(eEventLevel).maiStartingModules[pLoopModule.miType];
                            }
                        }

                        if (getColonyClass() != ColonyClassType.NONE)
                        {
                            iCount += infos().colonyClass(getColonyClass()).maiStartingModules[pLoopModule.miType];
                        }

                        for (int i = 0; i < iCount; i++)
                        {
                            addModule(pLoopModule.meType, false, false);

                            if (!isCampaign() || (Globals.Campaign.getCampaignState() == CampaignState.FINAL_ROUND))
                            {
                                if (infos().module(pLoopModule.meType).mbPopulation && !(infos().module(pLoopModule.meType).mbLabor))
                                {
                                    addModule(ModuleType.NONE, false, true);
                                }
                                else if (infos().module(pLoopModule.meType).mbLabor && !(infos().module(pLoopModule.meType).mbPopulation))
                                {
                                    addModule(ModuleType.NONE, true, false);
                                }
                            }
                        }
                    }

                    InfoLocation location = infos().location(getLocation());
                    int iExtraModules = location.miExtraStartingModules * (int)(getNumPlayers() + 1) / Constants.MAX_NUM_PLAYERS;
                    if (iExtraModules > 0)
                    {
                        for (int i = 0; i < iExtraModules; i++)
                        {
                            addModule(location.meExtraHousingModule, true, false);
                            addModule(location.meExtraLaborModule, false, true);
                        }
                    }

                    doPopulation();
                }

                if (getSevenSols() == SevenSolsType.COLONY)
                {
                    updateNextPopulationModule();
                    updateNextLaborModule();
                }
            }
        }

        public virtual TileServer addModule(ModuleType eModule, bool bPopulation, bool bLabor)
        {
            if (eModule == ModuleType.NONE)
            {
                int iBestValue = 0;

                for (ModuleType eLoopModule = 0; eLoopModule < infos().modulesNum(); eLoopModule++)
                {
                    if (canSpreadModule(eLoopModule))
                    {
                        if ((infos().module(eLoopModule).mbPopulation == bPopulation) ||
                            (infos().module(eLoopModule).mbLabor == bLabor))
                        {
                            int iProb = infos().module(eLoopModule).maiAppearanceProb[(int)getLocation()];
                            if (iProb == 0)
                            {
                                iProb = 1000;
                            }

                            int iValue = randomColony().Next(iProb) + 1;
                            if (iValue > iBestValue)
                            {
                                eModule = eLoopModule;
                                iBestValue = iValue;
                            }
                        }
                    }
                }
            }

            if (eModule == ModuleType.NONE)
            {
                return null;
            }

            TileServer pBestTile = (TileServer)(getBestModuleTile(eModule));

            if (pBestTile != null)
            {
                randomModule(eModule, pBestTile);
                return pBestTile;
            }
            else
            {
                return null;
            }
        }

        protected virtual void doTerrainCount()
        {
            foreach (InfoTerrain pLoopTerrain in infos().terrains())
            {
                foreach (TileServer pLoopTile in tileServerAll())
                {
                    if (pLoopTile.getTerrain() == pLoopTerrain.meType)
                    {
                        incrementTerrainCount(pLoopTerrain.meType);
                    }
                }
            }
        }

        protected virtual void doTerrainRegions()
        {
            const int SEARCH_RANGE = 4;

            HashSet<TileServer> spTileSet = new HashSet<TileServer>();

            for (int iPass = 0; iPass < 2; iPass++)
            {
                foreach (InfoTerrain pLoopTerrain in infos().terrains())
                {
                    if (pLoopTerrain.mbRegion)
                    {
                        TileServer pBestTile = null;
                        int iBestValue = (20 * (iPass + 1));

                        foreach (TileServer pLoopTile in tileServerAll())
                        {
                            if (pLoopTile.getTerrain() == pLoopTerrain.meType)
                            {
                                bool bSkip = false;
                                int iValue = 0;

                                if (!bSkip)
                                {
                                    foreach (TileServer pOtherTile in spTileSet)
                                    {
                                        if (Utils.stepDistanceTile(pLoopTile, pOtherTile) < 10)
                                        {
                                            bSkip = true;
                                            break;
                                        }
                                    }
                                }

                                if (!bSkip)
                                {
                                    foreach (TileServer pRangeTile in tileServerRangeIterator(pLoopTile, SEARCH_RANGE))
                                    {
                                        if (pRangeTile.getTerrainRegion() != TerrainType.NONE)
                                        {
                                            bSkip = true;
                                            break;
                                        }

                                        if (pRangeTile.isModule())
                                        {
                                            bSkip = true;
                                            break;
                                        }

                                        if (pRangeTile.getTerrain() == pLoopTerrain.meType)
                                        {
                                            iValue += (SEARCH_RANGE + 1) - Utils.stepDistanceTile(pLoopTile, pRangeTile);
                                        }
                                    }
                                }

                                if (!bSkip)
                                {
                                    if (iValue > iBestValue)
                                    {
                                        pBestTile = pLoopTile;
                                        iBestValue = iValue;
                                    }
                                }
                            }
                        }

                        if (pBestTile != null)
                        {
                            pBestTile.setTerrainRegion(pLoopTerrain.meType);
                            spTileSet.Add(pBestTile);
                        }
                    }
                }
            }
        }

        protected virtual void doStarts()
        {
            using (new UnityProfileScope("GameServer.doStarts"))
            {
                updateTerrainCache();

                AI_calculateFoundValues();
                AI_calculateFoundMinimums();

                for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
                {
                    if (infos().rulesSet(getRulesSet()).mbAutoStart && playerServer(eLoopPlayer).isHuman())
                    {
                        foreach (TileServer pLoopTile in tileServerAll())
                        {
                            pLoopTile.setVisibility(eLoopPlayer, VisibilityType.VISIBLE, false);
                        }

                        HQServer pHQ = playerServer(eLoopPlayer).AI_doFound(true);

                        if (pHQ != null)
                        {
                            TileServer pStartTile = pHQ.tileServer();

                            playerServer(eLoopPlayer).setStartTileID(pStartTile.getID());

                            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                            {
                                pStartTile.setResourceLevel(eLoopResource, ResourceLevelType.NONE);
                            }

                            pStartTile.setGeothermal(false);

                            for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
                            {
                                if (infos().HQ(pHQ.getType()).mabFootprint[(int)eDirection])
                                {
                                    TileServer pAdjacentTile = tileServerAdjacent(pStartTile, eDirection);

                                    if (pAdjacentTile != null)
                                    {
                                        for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                        {
                                            pAdjacentTile.setResourceLevel(eLoopResource, ResourceLevelType.NONE);
                                        }

                                        pAdjacentTile.setGeothermal(false);
                                    }
                                }
                            }

                            foreach (TileServer pLoopTile in tileServerAll())
                            {
                                if (Utils.stepDistanceTile(pStartTile, pLoopTile) > 4)
                                {
                                    pLoopTile.setVisibility(eLoopPlayer, VisibilityType.FOGGED, false);
                                }
                                else if (Utils.stepDistanceTile(pStartTile, pLoopTile) > 3)
                                {
                                    pLoopTile.setVisibility(eLoopPlayer, VisibilityType.REVEALED, false);
                                }
                            }
                        }
                    }

                    if (playerServer(eLoopPlayer).getStartTileID() == -1)
                    {
                        TileServer pBestTile = (TileServer)startModuleTileClient();

                        if (pBestTile == null)
                        {
                            pBestTile = tileServer(TileServer.IDFromWorldPosition(Vector3.zero, this));
                        }

                        if (pBestTile != null)
                        {
                            playerServer(eLoopPlayer).setStartTileID(pBestTile.getID());
                        }
                    }

                    {
                        TileServer pStartTile = playerServer(eLoopPlayer).startTile();

                        if (pStartTile != null)
                        {
                            pStartTile.increaseVisibility(eLoopPlayer, VisibilityType.REVEALED, false);

                            int iRange = infos().Globals.INITIAL_REVEAL_RANGE;

                            foreach (TileServer pLoopTile in tileServerRangeIterator(pStartTile, iRange))
                            {
                                pLoopTile.increaseVisibility(eLoopPlayer, VisibilityType.REVEALED, false);
                            }
                        }
                    }

                    if (playerServer(eLoopPlayer).isHuman())
                    {
                        TileServer pStartTile = (TileServer)(playerServer(eLoopPlayer).startTileClient());

                        if (pStartTile != null)
                        {
                            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                            {
                                if (isResourceValid(eLoopResource))
                                {
                                    int iCount = infos().rulesSet(getRulesSet()).maiFreeResources[(int)eLoopResource];

                                    while (iCount > 0)
                                    {
                                        TileServer pBestTile = null;
                                        int iBestValue = 0;

                                        int iRange = 5;

                                        foreach (TileServer pLoopTile in tileServerRangeIterator(pStartTile, iRange))
                                        {
                                            if (!(pLoopTile.noResources()) && pLoopTile.isEmpty() && !(pLoopTile.isGeothermal()))
                                            {
                                                bool bValid = true;

                                                for (ResourceType eAltResource = 0; eAltResource < infos().resourcesNum(); eAltResource++)
                                                {
                                                    if (pLoopTile.getResourceLevel(eAltResource, false) > ResourceLevelType.NONE)
                                                    {
                                                        bValid = false;
                                                        break;
                                                    }
                                                }

                                                if (bValid)
                                                {
                                                    int iValue = 1;

                                                    iValue += random().Next(1000);

                                                    if (iValue > iBestValue)
                                                    {
                                                        pBestTile = pLoopTile;
                                                        iBestValue = iValue;
                                                    }
                                                }
                                            }
                                        }

                                        if (pBestTile != null)
                                        {
                                            ResourceLevelType eResourceLevel = infos().Globals.FREE_RESOURCELEVEL;
                                            pBestTile.setResourceLevel(eLoopResource, eResourceLevel);
                                            iCount -= (int)eResourceLevel;
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

                if (infos().rulesSet(getRulesSet()).mbRevealMapHuman)
                {
                    for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
                    {
                        if (playerServer(eLoopPlayer).isHuman())
                        {
                            foreach (TileServer pLoopTile in tileServerAll())
                            {
                                pLoopTile.increaseVisibility(eLoopPlayer, VisibilityType.VISIBLE, false);
                            }
                        }
                    }
                }

                if (isGameOption(GameOptionType.REVEAL_MAP))
                {
                    for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
                    {
                        foreach (TileServer pLoopTile in tileServerAll())
                        {
                            pLoopTile.increaseVisibility(eLoopPlayer, VisibilityType.VISIBLE, false);
                        }
                    }
                }
            }
        }

        public static bool isValidSabotage(SabotageType eSabotage, List<bool> abGameOptions, Infos pInfos)
        {
            if (abGameOptions[(int)GameOptionType.NO_SABOTAGE])
            {
                if (pInfos.sabotage(eSabotage).mbHostile)
                {
                    return false;
                }
            }

            if (!(abGameOptions[(int)GameOptionType.ADVANCED_SABOTAGE]))
            {
                if (pInfos.sabotage(eSabotage).mbAdvanced)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool isValidBlackMarket(BlackMarketType eBlackMarket, List<bool> abGameOptions, int iNumTeams, Infos pInfos)
        {
            if (pInfos.blackMarket(eBlackMarket).mbIoDLC)
            {
                if (!(Globals.AppInfo.OwnsDLCIo) && !(Globals.AppInfo.IsInternalMod))
                {
                    if ((Globals.AppInfo.GameMode != GameModeType.DAILY_CHALLENGE) &&
                        (Globals.AppInfo.GameMode != GameModeType.STAND_ALONE_SERVER))
                    {
                        return false;
                    }
                }
            }

            if (iNumTeams < pInfos.blackMarket(eBlackMarket).miMinTeams)
            {
                return false;
            }

            if (pInfos.blackMarket(eBlackMarket).meSabotage != SabotageType.NONE)
            {
                if (!isValidSabotage(pInfos.blackMarket(eBlackMarket).meSabotage, abGameOptions, pInfos))
                {
                    return false;
                }
            }

            return true;
        }

        public static void fillBlackMarket(ref List<bool> abBlackMarketAvailable, List<bool> abGameOption, PlayerType eNumPlayers, int iNumTeams, int iSeed, Infos pInfos)
        {
            System.Random pRandom = new CrossPlatformRandom(iSeed);

            int iSlots = pInfos.Globals.BLACK_MARKET_SLOTS;
            int iCount = 0;

            if (abGameOption[(int)GameOptionType.NO_SABOTAGE])
            {
                iSlots /= 2;
            }

            foreach (InfoBlackMarketClass pLoopBlackMarketClass in pInfos.blackMarketClasses())
            {
                BlackMarketType eBestBlackMarket = BlackMarketType.NONE;
                int iBestValue = 0;

                foreach (InfoBlackMarket pLoopBlackMarket in pInfos.blackMarkets())
                {
                    if (pLoopBlackMarket.meClass == pLoopBlackMarketClass.meType)
                    {
                        if (isValidBlackMarket(pLoopBlackMarket.meType, abGameOption, iNumTeams, pInfos))
                        {
                            if (!(abBlackMarketAvailable[(int)pLoopBlackMarket.meType]))
                            {
                                int iValue = (pRandom.Next(pLoopBlackMarket.miAppearanceProb) + pLoopBlackMarket.maiLocationAppearanceModifiers[(int)AppCore.AppGlobals.GameGlobals.GameServer.getLocation()] + 1);
                                if (iValue > iBestValue)
                                {
                                    eBestBlackMarket = pLoopBlackMarket.meType;
                                    iBestValue = iValue;
                                }
                            }
                        }
                    }
                }

                if (eBestBlackMarket != BlackMarketType.NONE)
                {
                    abBlackMarketAvailable[(int)eBestBlackMarket] = true;
                    iCount++;
                }
                else
                {
                    break;
                }
            }

            while (iCount < iSlots)
            {
                BlackMarketType eBestBlackMarket = BlackMarketType.NONE;
                int iBestValue = 0;

                foreach (InfoBlackMarket pLoopBlackMarket in pInfos.blackMarkets())
                {
                    if (isValidBlackMarket(pLoopBlackMarket.meType, abGameOption, iNumTeams, pInfos))
                    {
                        if (!(abBlackMarketAvailable[(int)pLoopBlackMarket.meType]))
                        {
                            int iValue = (pRandom.Next(pLoopBlackMarket.miAppearanceProb) + pLoopBlackMarket.maiLocationAppearanceModifiers[(int)AppCore.AppGlobals.GameGlobals.GameServer.getLocation()] + 1);
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
                    abBlackMarketAvailable[(int)eBestBlackMarket] = true;
                    iCount++;
                }
                else
                {
                    break;
                }
            }

            if (abGameOption[(int)GameOptionType.ADVANCED_SABOTAGE])
            {
                BlackMarketType eBestBlackMarket = BlackMarketType.NONE;
                int iBestValue = 0;

                foreach (InfoBlackMarket pLoopBlackMarket in pInfos.blackMarkets())
                {
                    if ((pLoopBlackMarket.meSabotage != SabotageType.NONE) &&
                        (pInfos.sabotage(pLoopBlackMarket.meSabotage).mbAdvanced))
                    {
                        if (isValidBlackMarket(pLoopBlackMarket.meType, abGameOption, iNumTeams, pInfos))
                        {
                            if (!(abBlackMarketAvailable[(int)pLoopBlackMarket.meType]))
                            {
                                int iValue = (pRandom.Next(pLoopBlackMarket.miAppearanceProb) + pLoopBlackMarket.maiLocationAppearanceModifiers[(int)AppCore.AppGlobals.GameGlobals.GameServer.getLocation()] + 1);
                                if (iValue > iBestValue)
                                {
                                    eBestBlackMarket = pLoopBlackMarket.meType;
                                    iBestValue = iValue;
                                }
                            }
                        }
                    }
                }

                if (eBestBlackMarket != BlackMarketType.NONE)
                {
                    abBlackMarketAvailable[(int)eBestBlackMarket] = true;
                }
            }

            if (eNumPlayers >= (PlayerType)(Constants.MAX_NUM_PLAYERS / 2))
            {
                BlackMarketType eBestBlackMarket = BlackMarketType.NONE;
                int iBestValue = 0;

                foreach (InfoBlackMarket pLoopBlackMarket in pInfos.blackMarkets())
                {
                    if (isValidBlackMarket(pLoopBlackMarket.meType, abGameOption, iNumTeams, pInfos))
                    {
                        if (!(abBlackMarketAvailable[(int)pLoopBlackMarket.meType]))
                        {
                            if (pLoopBlackMarket.meTriggerLarge != BlackMarketType.NONE)
                            {
                                if (abBlackMarketAvailable[(int)pLoopBlackMarket.meTriggerLarge])
                                {
                                    int iValue = (pRandom.Next(pLoopBlackMarket.miAppearanceProb) + pLoopBlackMarket.maiLocationAppearanceModifiers[(int)AppCore.AppGlobals.GameGlobals.GameServer.getLocation()] + 1);
                                    if (iValue > iBestValue)
                                    {
                                        eBestBlackMarket = pLoopBlackMarket.meType;
                                        iBestValue = iValue;
                                    }
                                }
                            }
                        }
                    }
                }

                if (eBestBlackMarket != BlackMarketType.NONE)
                {
                    abBlackMarketAvailable[(int)eBestBlackMarket] = true;
                }
            }

            {
                int iTriggersDefense = 0;

                foreach (InfoBlackMarket pLoopBlackMarket in pInfos.blackMarkets())
                {
                    if (abBlackMarketAvailable[(int)pLoopBlackMarket.meType])
                    {
                        SabotageType eSabotage = pLoopBlackMarket.meSabotage;

                        if (eSabotage != SabotageType.NONE)
                        {
                            if (pInfos.sabotage(eSabotage).mbTriggersDefense)
                            {
                                iTriggersDefense++;
                            }
                        }
                    }
                }

                BlackMarketType eBestBlackMarket = BlackMarketType.NONE;
                int iBestValue = 0;

                foreach (InfoBlackMarket pLoopBlackMarket in pInfos.blackMarkets())
                {
                    if (isValidBlackMarket(pLoopBlackMarket.meType, abGameOption, iNumTeams, pInfos))
                    {
                        if (!(abBlackMarketAvailable[(int)pLoopBlackMarket.meType]))
                        {
                            if (pLoopBlackMarket.miTriggersDefense > 0)
                            {
                                if (pLoopBlackMarket.miTriggersDefense <= iTriggersDefense)
                                {
                                    int iValue = (pRandom.Next(pLoopBlackMarket.miAppearanceProb) + pLoopBlackMarket.maiLocationAppearanceModifiers[(int)AppCore.AppGlobals.GameGlobals.GameServer.getLocation()] + 1);
                                    if (iValue > iBestValue)
                                    {
                                        eBestBlackMarket = pLoopBlackMarket.meType;
                                        iBestValue = iValue;
                                    }
                                }
                            }
                        }
                    }
                }

                if (eBestBlackMarket != BlackMarketType.NONE)
                {
                    abBlackMarketAvailable[(int)eBestBlackMarket] = true;
                }
            }
        }

        protected virtual void doBlackMarkets()
        {
            using (new UnityProfileScope("GameServer.doBlackMarkets"))
            {
                if (isCampaign())
                {
                    foreach (InfoBlackMarket pLoopBlackMarket in infos().blackMarkets())
                    {
                        if (Globals.Campaign.isLevelBlackMarket(getLevel(), pLoopBlackMarket.meType))
                        {
                            makeBlackMarketAvailable(pLoopBlackMarket.meType);
                        }
                    }
                }
                else
                {
                    List<bool> abBlackMarketAvailable = Enumerable.Repeat(false, (int)(infos().blackMarketsNum())).ToList();

                    fillBlackMarket(ref abBlackMarketAvailable, getGameOptions(), getNumPlayers(), getNumTeams(), getSeed(), infos());

                    if (getLocation() == LocationType.EUROPA && random().Next(100) > 9)
                    {
                        makeBlackMarketAvailable(infos().Globals.CAVE_TERRAIN_SABOTAGE);
                    }

                    foreach (InfoBlackMarket pLoopBlackMarket in infos().blackMarkets())
                    {
                        if (abBlackMarketAvailable[pLoopBlackMarket.miType])
                        {
                            makeBlackMarketAvailable(pLoopBlackMarket.meType);
                        }
                    }
                }
            }
        }

        protected virtual int getCurrentTimeMS()
        {
            return (int)timer.ElapsedMilliseconds;
        }

        public virtual void testUpdate()
        {
            using (new UnityProfileScope("Game::update"))
            {
                bool bShouldCallUpdate = (!isPaused() && !isGameOver() && PlayGame); //don't advance game updateCount while game is paused or the game is over or not everyone hasn't finished loading the game
                int iCurrentTimeMS = getCurrentTimeMS();
                int iDeltaTime = iCurrentTimeMS - getLastUpdateTimeMS();

                //clamp delta time
                iDeltaTime = Mathf.Min(iDeltaTime, Constants.MAXIMUM_MILLISECS_PER_FRAME);
                while (iDeltaTime >= Constants.MILLISECS_PER_UPDATE)
                {
                    if (bShouldCallUpdate)
                    {
                        doUpdate();
                    }
                    iDeltaTime -= Constants.MILLISECS_PER_UPDATE;
                }
                setLastUpdateTimeMS(iCurrentTimeMS - iDeltaTime); //save any left over time for next frame
            }
        }

        public virtual void doUpdate()
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
                        if ((getSystemUpdateCount() % (infos().gameSpeed(getGameSpeed()).miSkipUpdates / (int)(getNumPlayers()))) == 0)
                        {
                            foreach (PlayerServer pLoopPlayer in getPlayerServerAll())
                            {
                                if (!(pLoopPlayer.AI_isUpdated()))
                                {
                                    pLoopPlayer.AI_doUpdate();
                                    pLoopPlayer.AI_setUpdated(true);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public virtual void forceUpdates(int iCount)
        {
            GameSpeedType eOldSpeed = getGameSpeed();

            setGameSpeed(infos().Globals.DEFAULT_GAMESPEED);

            iCount *= infos().gameSpeed(getGameSpeed()).miSkipUpdates;

            for (int i = 0; i < iCount; i++)
            {
                doUpdate();
            }

            setGameSpeed(eOldSpeed);
        }

        protected virtual void doDebtInterest()
        {
            int[] playerInterestPercent = Enumerable.Repeat(0, (int)getNumPlayers()).ToArray();
            int[] playersInterest = Enumerable.Repeat(0, (int)getNumPlayers()).ToArray();

            // do the debt calculations for all the players
            for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
            {
                PlayerServer pLoopPlayer = playerServer(eLoopPlayer);

                int iDebt = pLoopPlayer.getDebt();
                if (iDebt < 0)
                {
                    int iDebtInterest = iDebt;
                    int iInterest = pLoopPlayer.getInterestRate();
                    playerInterestPercent[(int)eLoopPlayer] = iInterest;
                    iDebtInterest *= iInterest;
                    iDebtInterest /= 100;
                    playersInterest[(int)eLoopPlayer] = iDebtInterest;

                    pLoopPlayer.changeDebt(iDebtInterest);
                    statsServer().changeStat(StatsType.MISCELLANEOUS, (int)MiscellaneousStatType.INTEREST, eLoopPlayer, 0, iDebtInterest);
                    //if (cVERBOSE_LOGGING)
                    //{
                    //    Debug.Log("[Game] Sol " + getDays() + " Debt: $" + pLoopPlayer.getDebt() + " [" + pLoopPlayer.getPlayer() + "]");
                    //}
                }
            }

            List<int[]> aaiPlayerDebtCut = new List<int[]>();

            for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
            {
                aaiPlayerDebtCut.Add(Enumerable.Repeat(0, (int)getNumPlayers()).ToArray());
            }

            // send the messages and do the financial instruments calcuations
            for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
            {
                PlayerServer pLoopPlayer = playerServer(eLoopPlayer);

                int iDebtCut = pLoopPlayer.getDebtCut();
                if (iDebtCut > 0)
                {
                    for (PlayerType eOtherPlayer = 0; eOtherPlayer < getNumPlayers(); eOtherPlayer++)
                    {
                        PlayerServer pOtherPlayer = playerServer(eOtherPlayer);

                        if (pOtherPlayer.getTeam() != pLoopPlayer.getTeam())
                        {
                            int iCut = -((playersInterest[(int)eOtherPlayer] * iDebtCut) / 100);
                            pLoopPlayer.changeMoney(iCut);
                            statsServer().changeStat(StatsType.MISCELLANEOUS, (int)MiscellaneousStatType.PATENT, eLoopPlayer, 0, iCut);
                            aaiPlayerDebtCut[(int)eLoopPlayer][(int)eOtherPlayer] = iCut;
                        }
                    }
                }
            }

            for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
            {
                if (getHours() == 0)
                {
                    this.gameEventsServer().AddNewDay(eLoopPlayer, playerInterestPercent[(int)eLoopPlayer], playersInterest[(int)eLoopPlayer], this.getDays(), aaiPlayerDebtCut[(int)eLoopPlayer].ToList<int>());
                }
                else
                {
                    //send a banner without the "new day" message for the midday debt tick on Europa
                    this.gameEventsServer().AddDebtNotification(eLoopPlayer, playerInterestPercent[(int)eLoopPlayer], playersInterest[(int)eLoopPlayer], aaiPlayerDebtCut[(int)eLoopPlayer].ToList<int>());
                }
            }
        }

        protected virtual void updateStocks()
        {
            using (new UnityProfileScope("Game::updateStocks"))
            {
                for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
                {
                    playerServer(eLoopPlayer).updateBaseSharePrice(false);
                }
            }
        }

        public virtual void updatePlayerLists()
        {
            using (new UnityProfileScope("GameServer.updatePlayerLists"))
            {
                for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
                {
                    playerServer(eLoopPlayer).updatePlayerList();
                }
            }
        }

        protected virtual void doModuleReveal()
        {
            if (isGameOption(GameOptionType.REVEAL_MAP))
            {
                return;
            }

            if (countPlayersStarted() == (int)(getNumPlayers()))
            {
                return;
            }

            if (getModuleRevealTime() > 0)
            {
                changeModuleRevealTime(-1);
            }
            else
            {
                setModuleRevealTime(infos().Globals.MODULE_REVEAL_TIME);

                List<TileServer> revealTileList = new List<TileServer>();

                for (int i = 0; i < mMap.numTiles(); i++)
                {
                    TileServer pLoopTile = tileServer(i);

                    if (pLoopTile.isModuleRevealed())
                    {
                        foreach (TileServer pAdjacentTile in tileServerAdjacentAll(pLoopTile))
                        {
                            if (!(pAdjacentTile.isModuleRevealed()))
                            {
                                revealTileList.Add(pAdjacentTile);
                            }
                        }
                    }
                }

                foreach (TileServer pLoopTile in revealTileList)
                {
                    for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
                    {
                        pLoopTile.increaseVisibility(eLoopPlayer, VisibilityType.VISIBLE, true);
                        pLoopTile.makeModuleRevealed();

                        int iRange = playerServer(eLoopPlayer).innerScanRange();

                        foreach (TileServer pRangeTile in tileServerRangeIterator(pLoopTile, iRange))
                        {
                            pRangeTile.increaseVisibility(eLoopPlayer, VisibilityType.REVEALED, true);
                        }
                    }
                }
            }
        }

        public virtual void doFoundMoney()
        {
            if (!isGameOption(GameOptionType.REVEAL_MAP))
            {
                return;
            }

            if (countPlayersStarted() == (int)(getNumPlayers()))
            {
                return;
            }

            if ((int)(getNumPlayers()) == 2)
            {
                if (countPlayersStarted() == 1)
                {
                    setFoundMoney(0);
                    return;
                }
            }

            if (getFoundMoney() < 0)
            {
                if (countPlayersStarted() == (int)(getNumPlayers() - 1))
                {
                    setFoundMoney(0);
                }
                else
                {
                    int iDiff = 10000;

                    if (getFoundMoney() >= -100000)
                    {
                        iDiff = -(getFoundMoney() / ((isGameOption(GameOptionType.RANDOM_PRICES) ? 15 : 10)));

                        iDiff = Utils.clamp((iDiff - (iDiff % 2000)), 2000, 8000);
                    }

                    if (getFoundMoney() < -(iDiff))
                    {
                        changeFoundMoney(iDiff);
                    }
                    else
                    {
                        setFoundMoney(0);
                    }
                }
            }
            else if (countPlayersStarted() < (int)(getNumPlayers() - 1))
            {
                bool bValid = false;

                if (isTeamGame())
                {
                    int iTeamsLeft = 0;

                    for (TeamType eLoopTeam = 0; eLoopTeam < (TeamType)(getNumTeams()); eLoopTeam++)
                    {
                        foreach (PlayerClient pLoopPlayer in getPlayerClientAll())
                        {
                            if (pLoopPlayer.getTeam() == eLoopTeam)
                            {
                                if (pLoopPlayer.getNumHQs() == 0)
                                {
                                    iTeamsLeft++;
                                    break;
                                }
                            }
                        }
                    }

                    if (iTeamsLeft > 1)
                    {
                        bValid = true;
                    }
                }
                else
                {
                    bValid = true;
                }

                if (bValid)
                {
                    changeFoundMoney(100);
                }
            }
        }

        protected virtual void doPopulation()
        {
            List<int> aiModuleCount = Enumerable.Repeat(0, (int)infos().modulesNum()).ToList();
            int iCount = 0;

            while (true)
            {
                ModuleServer pBestPopulationModule = null;

                {
                    int iBestValue = 0;

                    foreach (ModuleClient pLoopModule in getModuleList())
                    {
                        if (infos().module(pLoopModule.getType()).mbPopulation)
                        {
                            if (!(pLoopModule.isOccupied()))
                            {
                                int iValue = random().Next(1000) + 1;
                                if (iValue > iBestValue)
                                {
                                    pBestPopulationModule = (ModuleServer)pLoopModule;
                                    iBestValue = iValue;
                                }
                            }
                        }
                    }
                }

                if (pBestPopulationModule == null)
                {
                    break;
                }

                ModuleServer pBestLaborModule = null;

                {
                    int iBestValue = 0;

                    foreach (ModuleClient pLoopModule in getModuleList())
                    {
                        if (infos().module(pLoopModule.getType()).mbLabor)
                        {
                            if (!(pLoopModule.isOccupied()))
                            {
                                int iValue = random().Next(1000) + 1;
                                if (iValue > iBestValue)
                                {
                                    pBestLaborModule = (ModuleServer)pLoopModule;
                                    iBestValue = iValue;
                                }
                            }
                        }
                    }
                }

                if (pBestLaborModule == null)
                {
                    break;
                }

                pBestPopulationModule.makeOccupied();
                pBestLaborModule.makeOccupied();

                aiModuleCount[(int)pBestLaborModule.getType()]++;
                iCount++;
            }

            if (iCount > 0)
            {
                gameEventsServer().AddNewColonists(iCount, aiModuleCount);
            }
        }

        public virtual void doEntertainmentSupply()
        {
            using (new UnityProfileScope("Game::doEntertainmentSupply"))
            {
                setEntertainmentSupply(0);

                foreach (KeyValuePair<int, BuildingClient> pair in getBuildingDictionary())
                {
                    BuildingServer pLoopBuilding = (BuildingServer)(pair.Value);

                    if (!(pLoopBuilding.isStopped()) && pLoopBuilding.isWorked())
                    {
                        changeEntertainmentSupply(infos().building(pLoopBuilding.getType()).miEntertainment);
                    }
                }
            }
        }

        private static bool[,] tempPotentialHQConnections; //scratch memory for updateConnectedToHQ
        public virtual void updateConnectedToHQ()
        {
            using (new UnityProfileScope("GameServer.updateConnectedToHQ"))
            {
                foreach (TileServer pLoopTile in tileServerAll())
                {
                    pLoopTile.updateConnectedToHQ();
                }
            }

            using (new UnityProfileScope("GameServer.updatePotentialHQConnections"))
            {
                MapClient pMapClient = mapClient();
                int[,] adjacentTileIDs = pMapClient.tileAdjacentIds();
                List<TileClient> allTiles = pMapClient.tileClientAll();
                PlayerType numPlayers = getNumPlayers();

                using (new UnityProfileScope("GameServer.initializePotentialHQConnections"))
                {
                    ArrayUtilities.SetSize(ref tempPotentialHQConnections, allTiles.Count, (int)numPlayers);
                    tempPotentialHQConnections.Fill(false);
                }

                using (new UnityProfileScope("GameServer.loopPotentialHQConnections"))
                {
                    for (int i = 0; i < allTiles.Count; i++)
                    {
                        TileClient pLoopTile = allTiles[i];
                        PlayerType eOwner = pLoopTile.getOwner();
                        if ((eOwner >= 0) && pLoopTile.isConnectedToHQ())
                        {
                            tempPotentialHQConnections[i, (int)eOwner] = true;
                            HeightType eHeight = pLoopTile.getHeight();

                            //fill in potential adjacent tiles
                            for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
                            {
                                int iNextTileID = adjacentTileIDs[i, (int)eDirection];
                                if (iNextTileID >= 0)
                                {
                                    TileClient pNextTile = allTiles[iNextTileID];
                                    if (!pNextTile.isClaimed() && (pNextTile.getHeight() == eHeight))
                                    {
                                        tempPotentialHQConnections[iNextTileID, (int)eOwner] = true;
                                    }
                                }
                            }
                        }
                    }
                }

                using (new UnityProfileScope("GameServer.assignPotentialHQConnections"))
                {
                    for (int i = 0; i < allTiles.Count; i++)
                    {
                        TileServer pLoopTile = (TileServer)allTiles[i];
                        for (PlayerType ePlayer = 0; ePlayer < numPlayers; ePlayer++)
                        {
                            bool bPotentialConnection = tempPotentialHQConnections[i, (int)ePlayer];
                            pLoopTile.setPotentialHQConnection(ePlayer, bPotentialConnection);
                            if (bPotentialConnection)
                                playerServer(ePlayer).addNearbyTileToList(pLoopTile);
                        }
                    }
                }
            }
        }

        protected virtual void testWinner()
        {
            using (new UnityProfileScope("Game::testWinner"))
            {
                PlayerType eWinningPlayer = conditionManagerServer().GetWinner();

                if (eWinningPlayer != PlayerType.NONE)
                {
                    makeWinningTeam(playerServer(eWinningPlayer).getTeam());
                }
            }
        }

        protected virtual void testLosers()
        {
            using (new UnityProfileScope("Game::testLosers"))
            {
                List<PlayerType> eLosingPlayers = conditionManagerServer().GetLosers();

                foreach (PlayerType ePlayer in eLosingPlayers)
                {
                    playerServer(ePlayer).makeConcede();
                }

                if (countTeamsWinEligible() == 0)
                {
                    makeGameOver();
                }
            }
        }

        public virtual void triggerEventGame(EventGameType eEventGame, int iPercent, PlayerType ePlayer, int iDelay, int iTime)
        {
            {
                EventStateType eEventState = infos().eventGame(eEventGame).meEventState;

                if (eEventState != EventStateType.NONE)
                {
                    setEventState(eEventState);
                    changeEventStateGameDelay(iDelay);
                    changeEventStateGameTime(iTime);
                }
            }

            {
                int iMultiplier = 100;

                iMultiplier *= (50 + getHQLevels());
                iMultiplier /= (50);

                iMultiplier *= (int)(getNumPlayers());
                iMultiplier /= (int)(Constants.MAX_NUM_PLAYERS);

                if ((int)getNumPlayers() == 2)
                {
                    iMultiplier *= 4;
                    iMultiplier /= 5;
                }
                else if ((int)getNumPlayers() == 3)
                {
                    iMultiplier *= 9;
                    iMultiplier /= 10;
                }

                iMultiplier *= Math.Max(0, iPercent);
                iMultiplier /= 100;

                addEventGameTime(new EventGameTime(eEventGame, ePlayer, iDelay, iTime, iMultiplier, miTurnCount));
            }

            if (iDelay > 0)
            {
                for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
                {
                    if ((infos().eventGame(eEventGame).mbAnnounceDelay) ||
                        ((playerServer(eLoopPlayer).getHQ() != HQType.NONE) && infos().HQ(playerServer(eLoopPlayer).getHQ()).mbEarlyEventAnnounce))
                    {
                        gameEventsServer().AddEventGameDelay(eLoopPlayer, eEventGame);
                    }
                }
            }
            else
            {
                gameEventsServer().AddEventGame(eEventGame);
            }

            foreach (PlayerServer pLoopPlayer in getPlayerServerAll())
            {
                pLoopPlayer.changeClaims(infos().eventGame(eEventGame).miClaims);
            }

            InfoEventGame eventGame = infos().eventGame(eEventGame);
            if (eventGame.miDestroyPercent > 0)
            {
                foreach (PlayerServer pLoopPlayer in getPlayerServerAll())
                {
                    int iCount = 0;

                    foreach (int iBuilding in pLoopPlayer.getBuildingList())
                    {
                        BuildingServer pLoopBuilding = buildingServer(iBuilding);

                        if (pLoopBuilding.tileServer().isOwnerReal())
                        {
                            if (infos().building(pLoopBuilding.getType()).mbEventDestroy)
                            {
                                if (eventGame.meAffectedTerrain == TerrainType.NONE || eventGame.meAffectedTerrain == pLoopBuilding.tileServer().getTerrain())
                                    iCount++;
                            }
                        }
                    }

                    iCount = (((iCount * infos().eventGame(eEventGame).miDestroyPercent) / 100) + 1);

                    for (int iI = 0; iI < iCount; iI++)
                    {
                        pLoopPlayer.destroyBuildingEvent(iI + 1, eventGame.meAffectedTerrain);
                    }
                }
            }

            if (infos().eventGame(eEventGame).mbCreateResources)
            {
                IEnumerable<TileServer> tileList;
                if (infos().eventGame(eEventGame).meAffectedTerrain == TerrainType.NONE)
                    tileList = tileServerAll();
                else
                    tileList = tileServerAll().Where(x => x.getTerrain() == infos().eventGame(eEventGame).meAffectedTerrain && !x.isHQ() && !x.isModule());
                foreach (TileServer pLoopTile in tileList)
                {
                    if (random().Next(5) == 0)
                    {
                        List<ResourceType> aeResourcesAdded = TerrainGenerator.addResource(this, pLoopTile, false, true);
                        if (aeResourcesAdded != null)
                        {
                            for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
                            {
                                if (pLoopTile.isClaimed() || pLoopTile.adjacentToBuilding(eLoopPlayer) || pLoopTile.adjacentToHQ(eLoopPlayer))
                                    gameEventsServer().AddNewResource(pLoopTile.getID(), eLoopPlayer, aeResourcesAdded);
                            }
                        }
                        if (!pLoopTile.hasResources())
                        {
                            if (random().Next(100) == 0)
                            {
                                pLoopTile.setGeothermal(true);
                                gameEventsServer().AddDataEvent(GameEventsClient.DataType.NewGeothermal, PlayerType.NONE, new List<int> { pLoopTile.getID() });
                                addGeothermalTileToList(pLoopTile.getID());
                            }
                        }
                    }
                }
            }
        }

        public virtual void triggerEventGame(EventGameType eEventGame, int iPercent, PlayerType ePlayer = PlayerType.NONE)
        {
            triggerEventGame(eEventGame, iPercent, ePlayer, infos().eventGame(eEventGame).miDelay, infos().eventGame(eEventGame).miTime);
        }

        protected virtual void doEventGame()
        {
            if (isGameOption(GameOptionType.NO_RANDOM_EVENTS))
            {
                return;
            }

            for (int iPass = 0; iPass < 10; iPass++)
            {
                EventGameType eEventGame = getEventGameRoll(randomEvent().Next(getEventGameSize()));

                if (eEventGame != getLastEventGame())
                {
                    EventStateType eEventState = infos().eventGame(eEventGame).meEventState;

                    if ((eEventState == EventStateType.NONE) || !isEventStateOff(eEventState))
                    {
                        if ((eEventState == EventStateType.NONE) || (getEventStateGame() == EventStateType.NONE))
                        {
                            if ((eEventState == EventStateType.NONE) || (getEventStateLevel() != eEventState))
                            {
                                using (new UnityProfileScope("Game::doEventGame"))
                                {
                                    triggerEventGame(eEventGame, 100);
                                }

                                setLastEventGame(eEventGame);

                                break;
                            }
                        }
                    }
                }
            }
        }

        protected virtual int auctionBestTileValue(TileServer pTile, int iData, object pData)
        {
            if (pTile.isClaimed())
            {
                return 0;
            }

            if (pTile.isClaimBlock())
            {
                return 0;
            }

            if (!(pTile.usable()))
            {
                return 0;
            }

            if (!(pTile.isEmpty()))
            {
                return 0;
            }

            GameServer pGame = (GameServer)pData;

            bool bNonResource = (((pGame.getSeed() + pGame.getDays()) % 2) == 0);

            int iValue = 1;

            if (pTile.isGeothermal())
            {
                if (bNonResource)
                {
                    for (BuildingType eLoopBuilding = 0; eLoopBuilding < pGame.infos().buildingsNum(); eLoopBuilding++)
                    {
                        if (pGame.infos().building(eLoopBuilding).mbGeothermal)
                        {
                            bool bValid = false;

                            foreach (PlayerServer loopPlayer in pGame.getPlayerServerAliveAll())
                            {
                                if (loopPlayer.canEverConstruct(eLoopBuilding, true, true))
                                {
                                    bValid = true;
                                    break;
                                }
                            }

                            if (bValid)
                            {
                                for (ResourceType eLoopResource = 0; eLoopResource < pGame.infos().resourcesNum(); eLoopResource++)
                                {
                                    int iRate = pGame.resourceOutput(eLoopBuilding, eLoopResource, pTile, PlayerType.NONE, 0, true);
                                    if (iRate > 0)
                                    {
                                        iValue = Math.Max(iValue, (iRate * pGame.marketServer().getWholePrice(eLoopResource)) / Constants.RESOURCE_MULTIPLIER);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (bNonResource)
                {
                    for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                    {
                        TileServer pAdjacentTile = pGame.tileServerAdjacent(pTile, eLoopDirection);

                        if (pAdjacentTile != null)
                        {
                            if (pAdjacentTile.isModule())
                            {
                                iValue += 20;
                            }
                        }
                    }

                    if (pTile.adjacentToModule())
                    {
                        iValue += (pGame.entertainmentProfit(BuildingType.NONE, pTile, PlayerType.NONE, true, true) / (pGame.getEntertainmentSupply() + 2));
                    }
                }
                else
                {
                    for (ResourceType eLoopResource = 0; eLoopResource < pGame.infos().resourcesNum(); eLoopResource++)
                    {
                        int iRate = pTile.getPotentialResourceRate(eLoopResource);
                        if (iRate > 0)
                        {
                            iValue += (iRate * pGame.marketServer().getWholePrice(eLoopResource)) / Constants.RESOURCE_MULTIPLIER;
                        }
                    }
                }
            }

            iValue *= 10;

            iValue += pGame.random().Next(200);

            return iValue;
        }

        protected virtual int auctionBestTileBuildingValue(TileServer pTile, int iData, object pData)
        {
            if (pTile.isClaimed())
            {
                return 0;
            }

            if (!(pTile.usable()))
            {
                return 0;
            }

            if (!(pTile.isEmpty()))
            {
                return 0;
            }

            if (pTile.isGeothermal())
            {
                return 0;
            }

            GameServer pGame = (GameServer)pData;
            Infos pInfos = pGame.infos();

            int iValue = 0;

            for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
            {
                TileServer pAdjacentTile = pGame.tileServerAdjacent(pTile, eLoopDirection);

                if (pAdjacentTile != null)
                {
                    if (pAdjacentTile.isModule())
                    {
                        bool bBonusPossible = false;
                        bool bBonusFound = false;

                        for (OrderType eLoopOrder = 0; eLoopOrder < pInfos.ordersNum(); eLoopOrder++)
                        {
                            if (pGame.getModuleOrderModifier(pAdjacentTile.getModule(), eLoopOrder) > 0)
                            {
                                bBonusPossible = true;

                                if (pGame.countRealBuildingOrderCount(eLoopOrder) <= ((int)(pGame.getNumPlayers()) / 2))
                                {
                                    bBonusFound = true;
                                }
                            }
                        }

                        if (pGame.getModuleEntertainmentModifier(pAdjacentTile.getModule()) > 0)
                        {
                            bBonusPossible = true;

                            if (pGame.countRealBuildingCountEntertainment() <= ((int)(pGame.getNumPlayers()) / 2))
                            {
                                bBonusFound = true;
                            }
                        }

                        if (bBonusFound)
                        {
                            iValue += 100;
                        }
                        else if (!bBonusPossible)
                        {
                            iValue += 50;
                        }
                    }
                }
            }

            if (iValue > 0)
            {
                iValue += pGame.random().Next(100);
            }

            return iValue;
        }

        public virtual void startAuction(AuctionType eAuction, int iData1, int iData2, bool bForce = false)
        {
            if (isAuction())
            {
                return;
            }

            if (eAuction != AuctionType.NONE)
            {
                setAuction(eAuction);
                setAuctionData1(iData1);
                setAuctionData2(iData2);
            }
            else
            {
                if (!canStartAuction() && !bForce)
                {
                    return;
                }

                if (eAuction == AuctionType.NONE)
                {
                    if ((isCampaign()) &&
                        !(isPerkAuctioned()) &&
                        (isSevenSols()) &&
                        (getDays() < (getLastDay() / 2)) &&
                        (randomAuction().Next(3) == 0))
                    {
                        eAuction = AuctionType.PERK;
                        setAuctionData1(-1);
                        setAuctionData2(-1);
                    }
                }
            }

            if (eAuction == AuctionType.NONE)
            {
                eAuction = (AuctionType)(randomAuction().Next((int)(AuctionType.NUM_TYPES)));
                setAuctionData1(-1);
                setAuctionData2(-1);

                if (getTurnCount() <= 200)
                {
                    if ((eAuction == AuctionType.PATENT) ||
                        (eAuction == AuctionType.BLACK_MARKET_SABOTAGE) ||
                        (eAuction == AuctionType.TILE_BUILDING))
                    {
                        if (randomAuction().Next(2) == 0)
                        {
                            eAuction = AuctionType.CLAIM;
                        }
                        else
                        {
                            eAuction = AuctionType.TILE;
                        }
                    }
                }
                else
                {
                    if (getNumPlayers() < (PlayerType)(Constants.MAX_NUM_PLAYERS / 2))
                    {
                        if (randomAuction().Next(5) == 0)
                        {
                            eAuction = AuctionType.PATENT;
                        }
                    }
                }
            }

            if (getAuctionData1() == -1)
            {
                switch (eAuction)
                {
                    case AuctionType.PATENT:
                        {
                            PatentType eForcePatent = (((Globals.AppInfo.GameMode == GameModeType.DAILY_CHALLENGE) || (Globals.AppInfo.GameMode == GameModeType.INFINITE_CHALLENGE)) ? (PatentType)(randomAuctionPatent().Next((int)(infos().patentsNum()))) : PatentType.NONE);

                            PatentType eBestPatent = PatentType.NONE;
                            int iBestValue = 0;

                            for (PatentType eLoopPatent = 0; eLoopPatent < infos().patentsNum(); eLoopPatent++)
                            {
                                if (infos().patent(eLoopPatent).mbNoAuction)
                                    continue;
                                if (canEverPatent(eLoopPatent) && !isPatentAcquiredAny(eLoopPatent))
                                {
                                    bool bValid = true;

                                    if (bValid)
                                    {
                                        if (eForcePatent != PatentType.NONE)
                                        {
                                            if (eLoopPatent != eForcePatent)
                                            {
                                                bValid = false;
                                            }
                                        }
                                    }

                                    if (bValid)
                                    {
                                        for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
                                        {
                                            if (playerServer(eLoopPlayer).isPatentStarted(eLoopPatent))
                                            {
                                                bValid = false;
                                                break;
                                            }
                                        }
                                    }

                                    if (bValid)
                                    {
                                        if (infos().patent(eLoopPatent).miEntertainmentModifier > 0)
                                        {
                                            bool bFound = false;

                                            foreach (InfoBuilding pLoopBuilding in infos().buildings())
                                            {
                                                if (pLoopBuilding.miEntertainment > 0)
                                                {
                                                    for (PlayerType eLoopPlayer = 0; eLoopPlayer < getNumPlayers(); eLoopPlayer++)
                                                    {
                                                        if (playerServer(eLoopPlayer).isWinEligible())
                                                        {
                                                            if (playerServer(eLoopPlayer).canEverConstruct(pLoopBuilding.meType, true, true))
                                                            {
                                                                bFound = true;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }

                                                if (bFound)
                                                {
                                                    break;
                                                }
                                            }

                                            if (!bFound)
                                            {
                                                bValid = false;
                                            }
                                        }
                                    }

                                    if (bValid)
                                    {
                                        if (infos().patent(eLoopPatent).miAdjacentHQSabotageModifier != 0)
                                        {
                                            if (isGameOption(GameOptionType.NO_SABOTAGE))
                                            {
                                                bValid = false;
                                            }
                                        }
                                    }

                                    if (bValid)
                                    {
                                        if (infos().patent(eLoopPatent).mbBorehole)
                                        {
                                            if (getGeothermalCount() == 0)
                                            {
                                                bValid = false;
                                            }
                                        }
                                    }

                                    if (bValid)
                                    {
                                        int iValue = 1;

                                        iValue += randomAuctionPatent().Next(1000);

                                        if (iValue > iBestValue)
                                        {
                                            eBestPatent = eLoopPatent;
                                            iBestValue = iValue;
                                        }
                                    }
                                }
                            }

                            if (eBestPatent != PatentType.NONE)
                            {
                                setAuction(AuctionType.PATENT);
                                setAuctionData1((int)eBestPatent);
                            }
                            break;
                        }

                    case AuctionType.BLACK_MARKET_SABOTAGE:
                        {
                            BlackMarketType eBestBlackMarket = BlackMarketType.NONE;
                            int iBestValue = 0;

                            for (BlackMarketType eLoopBlackMarket = 0; eLoopBlackMarket < infos().blackMarketsNum(); eLoopBlackMarket++)
                            {
                                if (infos().blackMarket(eLoopBlackMarket).mbAuction)
                                {
                                    if (isValidBlackMarket(eLoopBlackMarket, getGameOptions(), (int)getNumTeams(), infos()))
                                    {
                                        if (!isBlackMarketAvailable(eLoopBlackMarket))
                                        {
                                            SabotageType eSabotage = infos().blackMarket(eLoopBlackMarket).meSabotage;

                                            if (eSabotage != SabotageType.NONE)
                                            {
                                                int iValue = 1;

                                                iValue += randomAuctionSabotage().Next(1000);

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

                            if (eBestBlackMarket != BlackMarketType.NONE)
                            {
                                setAuction(AuctionType.BLACK_MARKET_SABOTAGE);
                                setAuctionData1((int)eBestBlackMarket);
                            }
                            break;
                        }

                    case AuctionType.TILE:
                        {
                            TileServer pTile = findBestTile(auctionBestTileValue, -1, this);
                            if (pTile != null)
                            {
                                setAuction(AuctionType.TILE);
                                setAuctionData1(pTile.getID());
                            }
                            break;
                        }

                    case AuctionType.TILE_BUILDING:
                        {
                            TileServer pTile = findBestTile(auctionBestTileBuildingValue, -1, this);
                            if (pTile != null)
                            {
                                BuildingType eBestBuilding = BuildingType.NONE;
                                int iBestValue = 0;

                                for (BuildingType eLoopBuilding = 0; eLoopBuilding < infos().buildingsNum(); eLoopBuilding++)
                                {
                                    if (infos().building(eLoopBuilding).mbAuction)
                                    {
                                        if (canTileHaveBuilding(pTile, eLoopBuilding, PlayerType.NONE))
                                        {
                                            OrderType eOrder = infos().buildingClass(infos().building(eLoopBuilding).meClass).meOrderType;

                                            int iValue = 1;

                                            if (eOrder != OrderType.NONE)
                                            {
                                                iValue += (200 / (countRealBuildingOrderCount(eOrder) + 1));
                                            }

                                            if (infos().building(eLoopBuilding).miEntertainment > 0)
                                            {
                                                iValue += (200 / (countRealBuildingCountEntertainment() + 1));
                                            }

                                            for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                                            {
                                                TileServer pAdjacentTile = tileServerAdjacent(pTile, eLoopDirection);

                                                if (pAdjacentTile != null)
                                                {
                                                    if (pAdjacentTile.isModule())
                                                    {
                                                        if (eOrder != OrderType.NONE)
                                                        {
                                                            iValue += (getModuleOrderModifier(pAdjacentTile.getModule(), eOrder) * 10);
                                                        }

                                                        if (infos().building(eLoopBuilding).miEntertainment > 0)
                                                        {
                                                            iValue += (getModuleEntertainmentModifier(pAdjacentTile.getModule()) * 10);
                                                        }
                                                    }
                                                }
                                            }

                                            iValue += randomAuctionTileBuilding().Next(100);

                                            if (iValue > iBestValue)
                                            {
                                                eBestBuilding = eLoopBuilding;
                                                iBestValue = iValue;
                                            }
                                        }
                                    }
                                }

                                if (eBestBuilding != BuildingType.NONE)
                                {
                                    bool bValid = true;

                                    if (infos().buildingClass(infos().building(eBestBuilding).meClass).meOrderType == OrderType.PATENT)
                                    {
                                        if (countPatentsAvailable() < (countPatentsPossible() / 2))
                                        {
                                            bValid = false;
                                        }
                                    }

                                    if (bValid)
                                    {
                                        setAuction(AuctionType.TILE_BUILDING);
                                        setAuctionData1(pTile.getID());
                                        setAuctionData2((int)eBestBuilding);
                                    }
                                }
                            }
                            break;
                        }

                    case AuctionType.CLAIM:
                        {
                            setAuction(AuctionType.CLAIM);
                            break;
                        }

                    case AuctionType.PERK:
                        {
                            PerkType eBestPerk = PerkType.NONE;
                            int iBestValue = 0;

                            foreach (InfoPerk pLoopPerk in infos().perks())
                            {
                                if (pLoopPerk.mbAuction)
                                {
                                    bool bValid = true;

                                    {
                                        BuildingClassType eBuildingClassLevel = pLoopPerk.meBuildingClassLevel;

                                        if (eBuildingClassLevel != BuildingClassType.NONE)
                                        {
                                            foreach (InfoBuilding pLoopBuilding in infos().buildings())
                                            {
                                                if (pLoopBuilding.meClass == eBuildingClassLevel)
                                                {
                                                    if (isBuildingUnavailable(pLoopBuilding.meType))
                                                    {
                                                        bValid = false;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    if (bValid)
                                    {
                                        int iValue = randomAuctionPerk().Next(1000) + 1;
                                        if (iValue > iBestValue)
                                        {
                                            eBestPerk = pLoopPerk.meType;
                                            iBestValue = iValue;
                                        }
                                    }
                                }
                            }

                            if (eBestPerk != PerkType.NONE)
                            {
                                setAuction(AuctionType.PERK);
                                setAuctionData1((int)eBestPerk);
                                makePerkAuctioned();
                            }
                            break;
                        }
                }

                if (getAuction() == AuctionType.TILE)
                {
                    if (isCampaign())
                    {
                        TileServer pTile = auctionTile();

                        if (pTile.isGeothermal())
                        {
                            BuildingType eBestBuilding = BuildingType.NONE;
                            int iBestValue = 0;

                            for (BuildingType eLoopBuilding = 0; eLoopBuilding < infos().buildingsNum(); eLoopBuilding++)
                            {
                                if (canTileHaveBuilding(pTile, eLoopBuilding, PlayerType.NONE))
                                {
                                    if (calculateRevenue(eLoopBuilding, pTile, PlayerType.NONE, 0, true, true, 100, 100, 100, false) > 0)
                                    {
                                        int iValue = randomAuctionTileBuilding().Next(100);
                                        if (iValue > iBestValue)
                                        {
                                            eBestBuilding = eLoopBuilding;
                                            iBestValue = iValue;
                                        }
                                    }
                                }
                            }

                            if (eBestBuilding != BuildingType.NONE)
                            {
                                setAuction(AuctionType.TILE_BUILDING);
                                setAuctionData1(pTile.getID());
                                setAuctionData2((int)eBestBuilding);
                            }
                        }
                    }
                }
            }

            if (!isAuction())
            {
                return;
            }

            setAuctionTime(infos().Globals.AUCTION_TIME_LEFT);
            setAuctionStarted(true);
            gameEventsServer().AddAuction(getAuction(), AuctionState.START, PlayerType.NONE, getAuctionBid(), getAuctionData1(), getAuctionData2());
        }


        protected virtual void endAuction()
        {
            if (isAuction())
            {
                if (getAuctionLeader() != PlayerType.NONE)
                {
                    PlayerServer pAuctionPlayer = playerServer(getAuctionLeader());

                    if (pAuctionPlayer.isWinEligible())
                    {
                        switch (getAuction())
                        {
                            case AuctionType.PATENT:
                                {
                                    playerServer(getAuctionLeader()).makePatentAcquired((PatentType)getAuctionData1(), true, true);
                                    break;
                                }

                            case AuctionType.BLACK_MARKET_SABOTAGE:
                                {
                                    playerServer(getAuctionLeader()).blackMarketReceive((BlackMarketType)getAuctionData1());
                                    break;
                                }

                            case AuctionType.TILE:
                                {
                                    TileServer pTile = auctionTile();

                                    if (pTile.isRealClaimed())
                                    {
                                        statsServer().changeStat(StatsType.MISCELLANEOUS, (int)MiscellaneousStatType.AUCTION_PROCEEDS, pTile.realOwnerServer().getPlayer(), 0, getAuctionBid());
                                        pTile.realOwnerServer().changeMoney(getAuctionBid());
                                    }

                                    pTile.setOwner(getAuctionLeader(), true);
                                    pTile.setDefendTime(infos().Globals.AUCTION_DEFEND_TIME);
                                    pTile.makeWasAuctioned();

                                    if (!(playerServer(getAuctionLeader()).isHuman()))
                                    {
                                        playerServer(getAuctionLeader()).AI_doConstructBestBuildingTile(5, 10, pTile, BuildingType.NONE, false);
                                    }

                                    break;
                                }

                            case AuctionType.TILE_BUILDING:
                                {
                                    TileServer pTile = auctionTile();

                                    pTile.setOwner(getAuctionLeader(), true);
                                    pTile.setDefendTime(infos().Globals.AUCTION_DEFEND_TIME);
                                    pTile.makeWasAuctioned();

                                    pTile.ownerServer().createBuilding((BuildingType)getAuctionData2(), pTile, true);

                                    break;
                                }

                            case AuctionType.CLAIM:
                                {
                                    playerServer(getAuctionLeader()).changeClaims(1);
                                    break;
                                }

                            case AuctionType.PERK:
                                {
                                    playerServer(getAuctionLeader()).incrementPerkCount((PerkType)getAuctionData1());
                                    break;
                                }
                        }

                        playerServer(getAuctionLeader()).changeDebt(-(getAuctionBid()));

                        statsServer().changeStat(StatsType.MISCELLANEOUS, (int)MiscellaneousStatType.AUCTION_SPENT, getAuctionLeader(), 0, getAuctionBid());
                        statsServer().addEvent(getAuctionLeader(), StatEventType.AUCTION, (int)getAuction(), getAuctionData1(), getAuctionData2(), getAuctionBid());

                        if (pAuctionPlayer.isHuman())
                        {
                            if (getAuctionBid() == getStartingBid())
                            {
                                gameEventsServer().AddAchievement(getAuctionLeader(), infos().getType<AchievementType>("ACHIEVEMENT_WIN_UNCONTESTED_AUCTION"));
                            }
                        }
                    }
                }
            }

            gameEventsServer().AddAuction(getAuction(), getAuctionLeader() != PlayerType.NONE ? AuctionState.END_WINNER : AuctionState.END_NO_WINNER, getAuctionLeader(), getAuctionBid(), getAuctionData1(), getAuctionData2());

            setAuction(AuctionType.NONE);
            setAuctionLeader(PlayerType.NONE);
            setAuctionBid(0);
            setAuctionTime(0);
            setAuctionData1(0);
            setAuctionData2(0);

            incrementAuctionCount();

            foreach (PlayerServer pLoopPlayer in getPlayerServerAll())
            {
                pLoopPlayer.setSkipAuction(false);
            }
        }

        protected virtual void doAuction()
        {
            using (new UnityProfileScope("Game::doAuction"))
            {
                if (getAuctionTime() > 0)
                {
                    changeAuctionTime(-1);
                }
                else
                {
                    endAuction();
                }
            }
        }

        public virtual void skipAuction(bool bForce)
        {
            bool bSkip = true;
            bool bLeaderSkip = true;
            bool bAnyAIs = false;

            if (!bForce)
            {
                foreach (PlayerServer pLoopPlayer in getPlayerServerAliveAll())
                {
                    if (pLoopPlayer.isHuman())
                    {
                        if (!(pLoopPlayer.isSkipAuction()))
                        {
                            if (getAuctionLeader() == pLoopPlayer.getPlayer())
                                bLeaderSkip = false;
                            else if (pLoopPlayer.canBid())
                            {
                                bSkip = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        bAnyAIs = true;
                    }
                }
            }

            if (!bLeaderSkip && bAnyAIs)
                bSkip = false;

            if (bSkip)
            {
                while (isAuction())
                {
                    doUpdate();
                }
            }
        }

        public virtual void newBid(PlayerType eNewLeader, int iChange)
        {
            if (!isAuction())
            {
                return;
            }

            if (getAuctionBid() >= 1000000)
            {
                return;
            }

            changeAuctionBid(iChange);

            PlayerType eOldLeader = getAuctionLeader();

            if (getAuctionTime() < infos().Globals.AUCTION_TIME_BID)
            {
                setAuctionTime(infos().Globals.AUCTION_TIME_BID);
            }

            if (eOldLeader != eNewLeader)
            {
                setAuctionLeader(eNewLeader);
            }

            gameEventsServer().AddAuction(getAuction(), (eOldLeader == PlayerType.NONE) ? AuctionState.FIRST_BID : AuctionState.OUTBID, eNewLeader, getAuctionBid(), getAuctionData1(), getAuctionData2());
        }

        protected virtual TileServer findBestTile(Func<TileServer, int, object, int> valueFunc, int iData, object pData)
        {
            TileServer pBestTile = null;
            int iBestValue = 0;

            foreach (TileServer pLoopTile in tileServerAll())
            {
                int iValue = valueFunc(pLoopTile, iData, pData);

                if (iValue > iBestValue)
                {
                    pBestTile = pLoopTile;
                    iBestValue = iValue;
                }
            }

            return pBestTile;
        }

        protected virtual void AI_calculateFoundValues()
        {
            using (new UnityProfileScope("GameServer.AI_calculateFoundValues"))
            {
                foreach (TileServer pLoopTile in tileServerAll())
                {
                    if (pLoopTile.usable())
                    {
                        for (HQType eLoopHQ = 0; eLoopHQ < infos().HQsNum(); eLoopHQ++)
                        {
                            int iHQValue = 0;
                            int iResourceCount = 0;

                            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                            {
                                int iRate = pLoopTile.getPotentialResourceRate(eLoopResource, -40);
                                if (iRate > 0)
                                {
                                    int iSubValue = (((infos().HQ(eLoopHQ).maiAIResourceWeight[(int)eLoopResource] + infos().HQ(eLoopHQ).maaiAILocationResourceWeightModifier[(int)getLocation()][(int)eLoopResource]) * iRate) / Constants.RESOURCE_MULTIPLIER) + infos().HQ(eLoopHQ).maiAIResourceNearby[(int)eLoopResource];

                                    iSubValue *= marketServer().getWholePrice(eLoopResource);
                                    iSubValue /= infos().resource(eLoopResource).miMarketPrice;

                                    iHQValue += iSubValue;
                                }

                                if (pLoopTile.getResourceLevel(eLoopResource, false) > ResourceLevelType.NONE)
                                {
                                    iResourceCount++;
                                }
                            }

                            if (iResourceCount > 1)
                            {
                                foreach (InfoBuilding pLoopBuilding in infos().buildings())
                                {
                                    if (!isBuildingUnavailable(pLoopBuilding.meType))
                                    {
                                        int iRateValue = 0;
                                        int iRateCount = 0;

                                        if (Utils.isBuildingValid(pLoopBuilding.meType, eLoopHQ))
                                        {
                                            foreach (InfoResource pLoopResource in infos().resources())
                                            {
                                                int iWeight = infos().HQ(eLoopHQ).maiAIResourceWeight[pLoopResource.miType] + infos().HQ(eLoopHQ).maaiAILocationResourceWeightModifier[(int)getLocation()][(int)pLoopResource.miType];
                                                if (iWeight != 0)
                                                {
                                                    int iRate = resourceMiningTile(pLoopBuilding.meType, pLoopResource.meType, pLoopTile, PlayerType.NONE, infos().building(pLoopBuilding.meType).maiResourceMining[pLoopResource.miType], 0, true);
                                                    if (iRate > 0)
                                                    {
                                                        iRateValue += ((iWeight * iRate) / Constants.RESOURCE_MULTIPLIER);
                                                        iRateCount++;
                                                    }
                                                }
                                            }
                                        }

                                        if (iRateCount > 1)
                                        {
                                            iHQValue += (iRateValue * iRateCount);
                                        }
                                    }
                                }
                            }

                            if (pLoopTile.isGeothermal())
                            {
                                iHQValue += infos().HQ(eLoopHQ).miAIGeothermalWeight;
                            }

                            pLoopTile.setHQValue(eLoopHQ, iHQValue);
                        }
                    }
                }
            }
        }

        public virtual bool AI_isTileFoundMinimum(TileServer pTile, HQType eHQ, PlayerType ePlayer)
        {
            using (new UnityProfileScope("GameServer.AI_isTileFoundMinimum"))
            {
                bool bValid = true;
                MapClient pMapClient = mapClient();
                List<TileClient> aTileClients = pMapClient.tileClientAll();
                int iCenterTileID = pTile.getID();
                InfoHQ pHQInfo = infos().HQ(eHQ);

                foreach (InfoResource pResourceInfo in infos().resources())
                {
                    if (isResourceValid(pResourceInfo.meType) && (pResourceInfo.maiLocationAppearanceProb[(int)getLocation()] > 0))
                    {
                        bool bTestRate = (pHQInfo.maiAIResourceMinRateFound[pResourceInfo.miType] > 0);
                        bool bTestCount = (pHQInfo.maiAIResourceMinCountFound[pResourceInfo.miType] > 0);

                        if(bTestRate || bTestCount)
                        {
                            int iResourceRate = 0;
                            int iResourceCount = 0;

                            int iRange = pHQInfo.miAIResourceMinRangeFound;
                            if (iRange > 0)
                            {
                                for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
                                {
                                    if (!pHQInfo.mabFootprint[(int)eDirection])
                                    {
                                        TileClient pAdjacentTile = pMapClient.tileClientAdjacent(pTile, eDirection);

                                        if ((pAdjacentTile != null) && pAdjacentTile.usable())
                                        {
                                            if ((ePlayer == PlayerType.NONE) || (!pAdjacentTile.isClaimed() && !pAdjacentTile.isClaimBlock()))
                                            {
                                                if(bTestRate)
                                                {
                                                    int iRate = pAdjacentTile.getPotentialResourceRate(pResourceInfo.meType, -60);
                                                    iResourceRate += (iRate > 0) ? iRate : 0;
                                                }

                                                if(bTestCount)
                                                {
                                                    int iRate = pAdjacentTile.getPotentialResourceRate(pResourceInfo.meType);
                                                    iResourceCount += (iRate > 0) ? 1 : 0;
                                                }
                                            }
                                        }
                                    }
                                }

                                //add iRate to search radius tiles
                                for(int iRadius=2; iRadius<=iRange; iRadius++)
                                {
                                    foreach(int iTileID in pMapClient.tileRingIds(iCenterTileID, iRadius))
                                    {
                                        TileClient pRangeTile = aTileClients[iTileID];

                                        if (pRangeTile.usable())
                                        {
                                            if ((ePlayer == PlayerType.NONE) || (!pRangeTile.isClaimed() && !pRangeTile.isClaimBlock()))
                                            {
                                                if(bTestRate)
                                                {
                                                    int iRate = pRangeTile.getPotentialResourceRate(pResourceInfo.meType, -60);
                                                    iResourceRate += (iRate > 0) ? iRate : 0;
                                                }

                                                if(bTestCount)
                                                {
                                                    int iRate = pRangeTile.getPotentialResourceRate(pResourceInfo.meType);
                                                    iResourceCount += (iRate > 0) ? 1 : 0;
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (iResourceRate < pHQInfo.maiAIResourceMinRateFound[pResourceInfo.miType])
                            {
                                bValid = false;
                                break;
                            }

                            if (iResourceCount < pHQInfo.maiAIResourceMinCountFound[pResourceInfo.miType])
                            {
                                bValid = false;
                                break;
                            }
                        }
                    }
                }

                return bValid;
            }
        }

        protected virtual void AI_calculateFoundMinimums()
        {
            using (new UnityProfileScope("GameServer.AI_calculateFoundMinimums"))
            {
                MapClient pMapClient = mapClient();
                int [] aiTileResourceRates = new int [pMapClient.numTiles()];
                int [] aiTileResourceCounts = new int [pMapClient.numTiles()];
                bool [] abTileFoundMinimum = new bool [pMapClient.numTiles()];

                for (HQType eLoopHQ = 0; eLoopHQ < infos().HQsNum(); eLoopHQ++)
                {
                    if (canFound(eLoopHQ, false))
                    {
                        //initialize found minimums
                        for(int i=0; i<abTileFoundMinimum.Length; i++)
                            abTileFoundMinimum[i] = pMapClient.tileClient(i).usable();

                        for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                        {
                            if (isResourceValid(eLoopResource) && (infos().resource(eLoopResource).maiLocationAppearanceProb[(int)getLocation()] > 0))
                            {
                                bool bTestRate = (infos().HQ(eLoopHQ).maiAIResourceMinRateFound[(int)eLoopResource] > 0);
                                bool bTestCount = (infos().HQ(eLoopHQ).maiAIResourceMinCountFound[(int)eLoopResource] > 0);

                                if(bTestRate || bTestCount)
                                {
                                    aiTileResourceRates.Fill(0);
                                    aiTileResourceCounts.Fill(0);

                                    int iRange = infos().HQ(eLoopHQ).miAIResourceMinRangeFound;
                                    if (iRange > 0)
                                    {
                                        using (new UnityProfileScope("foundOuter"))
                                        {
                                            for(int i=0; i<aiTileResourceRates.Length; i++)
                                            {
                                                TileClient pLoopTile = pMapClient.tileClient(i);
                                                if(pLoopTile.usable())
                                                {
                                                    int iRate = pLoopTile.getPotentialResourceRate(eLoopResource, -60);
                                                    if(iRate > 0)
                                                    {
                                                        //add iRate to adjacent tiles that aren't part of the HQ footprint
                                                        for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
                                                        {
                                                            //reverse the footprint direction since the footprint is centered on the adjacent tile
                                                            if (!infos().HQ(eLoopHQ).mabFootprint[(int)Utils.directionOpposite(eDirection)])
                                                            {
                                                                TileClient pAdjacentTile = tileClientAdjacent(pLoopTile, eDirection);
                                                                if((pAdjacentTile != null) && pAdjacentTile.usable())
                                                                {
                                                                    int iTileID = pAdjacentTile.getID();
                                                                    aiTileResourceRates[iTileID] += iRate;
                                                                    aiTileResourceCounts[iTileID]++;
                                                                }
                                                            }
                                                        }

                                                        //add iRate to search radius tiles
                                                        for(int iRadius=2; iRadius<=iRange; iRadius++)
                                                        {
                                                            foreach(int iTileID in pMapClient.tileRingIds(i, iRadius))
                                                            {
                                                                TileClient pRangeTile = pMapClient.tileClient(iTileID);

                                                                if (pRangeTile.usable())
                                                                {
                                                                    aiTileResourceRates[iTileID] += iRate;
                                                                    aiTileResourceCounts[iTileID]++;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    //verify it passes minimum
                                    int resourceMinRate = infos().HQ(eLoopHQ).maiAIResourceMinRateFound[(int)eLoopResource];
                                    int resourceMinCount = infos().HQ(eLoopHQ).maiAIResourceMinCountFound[(int)eLoopResource];
                                    for(int i=0; i<abTileFoundMinimum.Length; i++)
                                    {
                                        abTileFoundMinimum[i] &= (aiTileResourceRates[i] >= resourceMinRate);
                                        abTileFoundMinimum[i] &= (aiTileResourceCounts[i] >= resourceMinCount);
                                    }
                                }
                            }
                        }

                        foreach (TileServer pLoopTile in tileClientAll())
                        {
                            if (pLoopTile.usable())
                            {
                                pLoopTile.setHQMinmum(eLoopHQ, abTileFoundMinimum[pLoopTile.getID()]);
                            
                                if(PlayerServer.VALIDATE_OPTIMIZATIONS)
                                {
                                    Assert.AreEqual(abTileFoundMinimum[pLoopTile.getID()], AI_isTileFoundMinimum(pLoopTile, eLoopHQ, PlayerType.NONE));
                                }    
                            }
                        }
                    }
                }
            }
        }

        protected virtual void incrementSystemUpdateCount()
        {
            miSystemUpdateCount++;

            makeDirty(GameDirtyType.miSystemUpdateCount);
        }

        protected virtual void incrementGameUpdateCount()
        {
            miGameUpdateCount++;

            makeDirty(GameDirtyType.miGameUpdateCount);
        }

        protected virtual void incrementTurnCount()
        {
            miTurnCount++;

            makeDirty(GameDirtyType.miTurnCount);
        }

        public virtual void changeTurnBasedTime(int iChange)
        {
            if (iChange != 0)
            {
                miTurnBasedTime = Math.Max(0, (getTurnBasedTime() + iChange));

                makeDirty(GameDirtyType.miTurnBasedTime);
            }
        }

        public virtual void changeDelayTime(int iChange)
        {
            if (iChange != 0)
            {
                miDelayTime = Math.Max(0, (getDelayTime() + iChange));

                makeDirty(GameDirtyType.miDelayTime);
            }
        }

        protected virtual void incrementMinutes(int iChange)
        {
            if (iChange > 0)
            {
                miMinutes += iChange;

                if (getMinutes() >= Utils.getHourMinutes(getHours(), getLocation()))
                {
                    miMinutes = 0;

                    incrementHour();
                }

                makeDirty(GameDirtyType.miMinutes);
            }
        }

        protected virtual void incrementHour()
        {
            InfoLocation pLocation = infos().location(getLocation());
            if (getHours() < (pLocation.miHoursPerDay - 1))
            {
                miHours++;
            }
            else
            {
                miHours = 0;
                incrementDays();
                //Debug.Log("[Game] Sol " + getDays());
            }

            if (getHours() == 0 || getHours() == pLocation.miSecondInterestHour)
            {
                if (getDays() > 0)
                {
                    if (!isSevenSols() || (getDays() < getLastDay()))
                    {
                        doDebtInterest();
                    }
                }
            }

            if (pLocation.miAuctionHour == getHours())
            {
                if ((getDays() % pLocation.miAuctionDayDiv) == 0)
                {
                    if (!isAuctionStarted())
                    {
                        startAuction(AuctionType.NONE, -1, -1);
                    }

                    setAuctionStarted(false);
                }
            }
            if (getHours() == pLocation.miSecondAuctionHour && getDays() > pLocation.miSecondAuctionMinDay && getDays() % pLocation.miAuctionDayDiv == 0)
            {
                if (!isAuctionStarted())
                {
                    startAuction(AuctionType.NONE, -1, -1);
                }
                setAuctionStarted(false);
            }

            if (!infos().rulesSet(getRulesSet()).mbNoColony && !infos().rulesSet(getRulesSet()).mbCustomColony)
            {
                if (!isSevenSols())
                {
                    if ((randomColony().Next(Constants.MAX_NUM_PLAYERS * infos().Globals.COLONY_GROWTH_ROLL) < (int)(getNumPlayers() + 2)) ||
                        (getPopulation() < ((getTurnCount() * (int)(getNumPlayers() + 4)) / 1000) + (int)(getNumPlayers() + 2)))
                    {
                        addModule(ModuleType.NONE, true, false);
                        addModule(ModuleType.NONE, false, true);
                    }
                }
            }


            if (getHours() % 3 == 0 && getLocation() == LocationType.EUROPA)
            {
                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    maiImportValueModifier[(int)eLoopResource] += (100 - maiImportValueModifier[(int)eLoopResource]) / 10;
                    maiExportValueModifier[(int)eLoopResource] += (100 - maiExportValueModifier[(int)eLoopResource]) / 10;
                }
                updateImportCost();
            }

            doPopulation();

            makeDirty(GameDirtyType.miHours);
        }

        protected virtual void incrementDays()
        {
            miDays++;

            makeDirty(GameDirtyType.miDays);
        }

        protected virtual void setStartModuleTileID(int iNewValue)
        {
            if (getStartModuleTileID() != iNewValue)
            {
                miStartModuleTileID = iNewValue;

                makeDirty(GameDirtyType.miStartModuleTileID);
            }
        }

        protected virtual void setPopulation(int iNewValue)
        {
            int iOldValue = getPopulation();
            if (iOldValue != iNewValue)
            {
                miPopulation = iNewValue;

                makeDirty(GameDirtyType.miPopulation);
            }
        }
        public virtual void changePopulation(int iChange)
        {
            setPopulation(getPopulation() + iChange);
        }

        public virtual void changeMaxPopulation(int iChange)
        {
            if (iChange != 0)
            {
                miMaxPopulation += iChange;

                makeDirty(GameDirtyType.miMaxPopulation);
            }
        }

        public virtual void changeLabor(int iChange)
        {
            if (iChange != 0)
            {
                miLabor += iChange;

                makeDirty(GameDirtyType.miLabor);
            }
        }

        public virtual void changeHQLevels(int iChange)
        {
            if (iChange != 0)
            {
                miHQLevels += iChange;

                makeDirty(GameDirtyType.miHQLevels);
            }
        }

        public virtual void changeEntertainmentDemand(int iChange)
        {
            if (iChange != 0)
            {
                miEntertainmentDemand += iChange;

                makeDirty(GameDirtyType.miEntertainmentDemand);
            }
        }

        protected virtual void setEntertainmentSupply(int iNewValue)
        {
            if (getEntertainmentSupply() != iNewValue)
            {
                miEntertainmentSupply = iNewValue;

                makeDirty(GameDirtyType.miEntertainmentSupply);
            }
        }
        public virtual void changeEntertainmentSupply(int iChange)
        {
            setEntertainmentSupply(getEntertainmentSupply() + iChange);
        }

        protected virtual void incrementAuctionCount()
        {
            miAuctionCount++;

            makeDirty(GameDirtyType.miAuctionCount);
        }

        protected virtual void setAuctionBid(int iNewValue)
        {
            if (getAuctionBid() != iNewValue)
            {
                miAuctionBid = iNewValue;

                makeDirty(GameDirtyType.miAuctionBid);
            }
        }
        protected virtual void changeAuctionBid(int iChange)
        {
            setAuctionBid(getAuctionBid() + iChange);
        }

        protected virtual void setAuctionTime(int iNewValue)
        {
            if (getAuctionTime() != iNewValue)
            {
                miAuctionTime = iNewValue;

                makeDirty(GameDirtyType.miAuctionTime);
            }
        }
        protected virtual void changeAuctionTime(int iChange)
        {
            setAuctionTime(getAuctionTime() + iChange);
        }

        protected virtual TileServer auctionTile()
        {
            return (TileServer)auctionTileClient();
        }
        protected virtual void setAuctionData1(int iNewValue)
        {
            if (getAuctionData1() != iNewValue)
            {
                miAuctionData1 = iNewValue;

                makeDirty(GameDirtyType.miAuctionData1);
            }
        }

        protected virtual void setAuctionData2(int iNewValue)
        {
            if (getAuctionData2() != iNewValue)
            {
                miAuctionData2 = iNewValue;

                makeDirty(GameDirtyType.miAuctionData2);
            }
        }

        protected virtual void changeEventStateGameDelay(int iChange)
        {
            if (iChange != 0)
            {
                miEventStateGameDelay += iChange;

                makeDirty(GameDirtyType.miEventStateGameDelay);
            }
        }

        protected virtual void changeEventStateGameTime(int iChange)
        {
            if (iChange != 0)
            {
                miEventStateGameTime += iChange;

                makeDirty(GameDirtyType.miEventStateGameTime);
            }
        }

        public virtual void changeSharesBought(int iChange)
        {
            if (iChange != 0)
            {
                miSharesBought += iChange;

                makeDirty(GameDirtyType.miSharesBought);
            }
        }

        public virtual void setFoundMoney(int iNewValue)
        {
            if (getFoundMoney() != iNewValue)
            {
                miFoundMoney = iNewValue;

                makeDirty(GameDirtyType.miFoundMoney);
            }
        }
        protected virtual void changeFoundMoney(int iChange)
        {
            setFoundMoney(getFoundMoney() + iChange);
        }

        public void changeGeothermalCount(int iChange)
        {
            if (iChange != 0)
            {
                miGeothermalCount += iChange;

                makeDirty(GameDirtyType.miGeothermalCount);
            }
        }

        protected virtual void makeGameOver()
        {
            if (!isGameOver())
            {
                if (isCampaignSevenSols())
                {
                    doDebtInterest();

                    doPopulation();

                    foreach (PlayerServer pLoopPlayer in getPlayerServerAll())
                    {
                        pLoopPlayer.cancelAllOrders();

                        foreach (int iBuilding in pLoopPlayer.getBuildingList())
                        {
                            BuildingClient pLoopBuilding = buildingClient(iBuilding);

                            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                            {
                                pLoopPlayer.changeWholeResourceStockpile(eLoopResource, pLoopBuilding.getWholeResourceStockpile(eLoopResource), false);

                                if (pLoopPlayer.isRecycleScrap() && pLoopBuilding.canScrap())
                                {
                                    pLoopPlayer.changeWholeResourceStockpile(eLoopResource, pLoopPlayer.getBuildingResourceCost(pLoopBuilding.getType(), eLoopResource), false);
                                }
                            }
                        }

                        foreach (int iUnitID in pLoopPlayer.getUnitList())
                        {
                            UnitClient pLoopUnit = unitClient(iUnitID);

                            if (pLoopUnit.getCargoResource() != ResourceType.NONE)
                            {
                                pLoopPlayer.changeWholeResourceStockpile(pLoopUnit.getCargoResource(), pLoopUnit.getWholeCargoQuantity(), false);
                            }
                        }
                    }

                    while (true)
                    {
                        bool bSold = false;

                        foreach (PlayerServer pLoopPlayer in getPlayerServerAll())
                        {
                            if (pLoopPlayer.sellResourcesQuantity(1))
                            {
                                bSold = true;
                            }
                        }

                        if (!bSold)
                        {
                            break;
                        }
                    }

                    foreach (PlayerServer pLoopPlayer in getPlayerServerAll())
                    {
                        pLoopPlayer.payDebtAll();
                    }
                }

                foreach (PlayerServer pLoopPlayer in getPlayerServerAliveAll())
                {
                    pLoopPlayer.saveEndingMoneyDebt();
                }

                mbGameOver = true;

                if (isCampaign())
                {
                    Campaign pCampaign = Globals.Campaign;

                    List<CorporationType> aeCorporationIDs = new List<CorporationType>();
                    List<int> aiCorporationColonyShares = new List<int>();
                    List<int> aiCorporationHQs = new List<int>();
                    List<int> aiCorporationCash = new List<int>();
                    List<int> aiCorporationDebt = new List<int>();
                    List<int> aiCorporationStructures = new List<int>();
                    List<int> aiCorporationModules = new List<int>();
                    List<int> aiResourcePrices = new List<int>();
                    List<int> aiResourcePricesOffworld = new List<int>();

                    foreach (PlayerServer pLoopPlayer in getPlayerServerAll())
                    {
                        aeCorporationIDs.Add(pLoopPlayer.getCorporation());
                        aiCorporationColonyShares.Add(pLoopPlayer.getColonySharesOwned());
                        aiCorporationHQs.Add(pLoopPlayer.calculateHQValue());
                        aiCorporationCash.Add((int)pLoopPlayer.getMoney());
                        aiCorporationDebt.Add(pLoopPlayer.getDebt());
                        aiCorporationStructures.Add(pLoopPlayer.calculateStructuresValue());
                        aiCorporationModules.Add(pLoopPlayer.calculateModulesValue());
                    }

                    foreach (InfoResource pLoopResource in infos().resources())
                    {
                        aiResourcePrices.Add(marketServer().getWholePrice(pLoopResource.meType));

                        if (wasEverHumanGame())
                        {
                            aiResourcePricesOffworld.Add(marketServer().getWholeOffworldPrice(pLoopResource.meType));
                        }
                        else
                        {
                            aiResourcePricesOffworld.Add(0);
                        }
                    }

                    PlayerType firstWinner = PlayerType.NONE;

                    foreach (PlayerServer pLoopPlayer in getPlayerServerAliveAll())
                    {
                        if (pLoopPlayer.getTeam() == getWinningTeam())
                        {
                            firstWinner = pLoopPlayer.getPlayer();
                            break;
                        }
                    }

                    Assert.IsTrue(firstWinner != PlayerType.NONE, "no one won the campaign game?");
                    pCampaign.addResult(new Campaign.LevelResult(getLevel(), playerServer(firstWinner).getCorporation(), getPopulation(), aeCorporationIDs, aiCorporationColonyShares, aiCorporationHQs, aiCorporationCash, aiCorporationDebt, aiCorporationStructures, aiCorporationModules, aiResourcePrices, aiResourcePricesOffworld));

                    if (wasEverHumanGame())
                    {
                        pCampaign.doTurn();
                    }
                }

                foreach (PlayerServer pLoopPlayer in getPlayerServerAll())
                {
                    if (pLoopPlayer.isWinEligible())
                    {
                        gameEventsServer().AddPlayerWinLose(pLoopPlayer.getPlayer(), pLoopPlayer.getTeam() == getWinningTeam(), pLoopPlayer.getFinalMarketCap());
                    }
                    else if (pLoopPlayer.getTeam() == getWinningTeam())
                    {
                        gameEventsServer().AddPlayerWinScreen(pLoopPlayer.getPlayer());
                    }
                }

                makeDirty(GameDirtyType.mbGameOver);
            }
        }

        public virtual void pickAWinner()
        {
            TeamType winningTeam = getWinningTeam();

            if (winningTeam == TeamType.NONE)
            {
                int iBestValue = 0;

                foreach (PlayerServer pLoopPlayer in getPlayerServerAliveAll())
                {
                    if (!(pLoopPlayer.isHuman()))
                    {
                        int iValue = ((isSevenSols()) ? pLoopPlayer.getColonySharesOwned() : pLoopPlayer.getSharePrice());

                        iValue *= (50 + random().Next(100));
                        iValue /= 100;

                        if (iValue > iBestValue)
                        {
                            winningTeam = pLoopPlayer.getTeam();
                            iBestValue = iValue;
                        }
                    }
                }

            }

            if (winningTeam != TeamType.NONE)
            {
                makeWinningTeam(winningTeam);
            }
        }

        protected virtual void makePerkAuctioned()
        {
            if (!isPerkAuctioned())
            {
                mbPerkAuctioned = true;

                makeDirty(GameDirtyType.mbPerkAuctioned);
            }
        }

        protected virtual void setAuctionStarted(bool bNewValue)
        {
            if (isAuctionStarted() != bNewValue)
            {
                mbAuctionStarted = bNewValue;

                makeDirty(GameDirtyType.mbAuctionStarted);
            }
        }

        public virtual void makeWinningTeam(TeamType eNewValue)
        {
            if (getWinningTeam() == TeamType.NONE)
            {
                meWinningTeam = eNewValue;
                makeGameOver();
                makeDirty(GameDirtyType.meWinningTeam);
            }
        }

        protected virtual void setCheating(PlayerType eCheatingPlayer)
        {
            if (getCheatingPlayer() != eCheatingPlayer)
            {
                // if we're not cheating, flag cheating player
                if (getCheatingPlayer() == PlayerType.NONE)
                {
                    meCheatingPlayer = eCheatingPlayer;
                }
                else
                {
                    // if we were cheating, disable cheating
                    meCheatingPlayer = PlayerType.NONE;
                }

                makeDirty(GameDirtyType.meCheatingPlayer);
            }
        }

        protected virtual void setAuction(AuctionType eNewValue)
        {
            if (getAuction() != eNewValue)
            {
                meAuction = eNewValue;

                makeDirty(GameDirtyType.meAuction);
            }
        }

        protected virtual void setAuctionLeader(PlayerType eNewValue)
        {
            if (getAuctionLeader() != eNewValue)
            {
                meAuctionLeader = eNewValue;

                makeDirty(GameDirtyType.meAuctionLeader);
            }
        }

        protected virtual void setLastEventGame(EventGameType eNewValue)
        {
            if (getLastEventGame() != eNewValue)
            {
                meLastEventGame = eNewValue;

                makeDirty(GameDirtyType.meLastEventGame);
            }
        }

        protected virtual void setEventState(EventStateType eNewValue)
        {
            if (getEventStateGame() != eNewValue)
            {
                meEventStateGame = eNewValue;

                makeDirty(GameDirtyType.meEventStateGame);
            }
        }
        public virtual void doEventStates()
        {
            if (getEventStateGame() == EventStateType.NONE)
            {
                return;
            }

            if (getEventStateGameDelay() > 0)
            {
                changeEventStateGameDelay(-1);
            }
            else if (getEventStateGameTime() > 0)
            {
                changeEventStateGameTime(-1);

                if (getEventStateGameTime() == 0)
                {
                    setEventState(EventStateType.NONE);
                }
            }
        }

        public virtual void setGameSpeed(GameSpeedType eNewValue)
        {
            int clampValue = (int)(infos().gameSpeedsNum() - 1);

            clampValue -= Globals.AppInfo.IsInternalBuild ? 0 : 1; // this disables AI_TEST speed from the consumer version

            eNewValue = (GameSpeedType)(Utils.clamp((int)eNewValue, (int)(GameSpeedType.NONE), clampValue));

            if (getGameSpeed() != eNewValue)
            {
                mePrevGameSpeed = meGameSpeed;
                meGameSpeed = eNewValue;

                makeDirty(GameDirtyType.meGameSpeed);
                makeDirty(GameDirtyType.mePrevGameSpeed);

                gameEventsServer().AddSpeedChange(mePrevGameSpeed, meGameSpeed);
            }
        }
        public void changeGameSpeed(int iChange)
        {
            setGameSpeed(getGameSpeed() + iChange);
        }
        public void togglePaused()
        {
            if (getGameSpeed() == GameSpeedType.NONE)
            {
                setGameSpeed(getPrevGameSpeed() != GameSpeedType.NONE ? getPrevGameSpeed() : gameSettings().meGameSpeed);
            }
            else
            {
                setGameSpeed(GameSpeedType.NONE);
            }
        }

        public virtual void setHQHighest(HQLevelType eNewValue)
        {
            if (getHQHighest() != eNewValue)
            {
                meHQHighest = eNewValue;

                makeDirty(GameDirtyType.meHQHighest);
            }
        }

        public virtual void updateNextPopulationModule()
        {
            ModuleType eBestModule = ModuleType.NONE;
            int iBestValue = 0;

            for (ModuleType eLoopModule = 0; eLoopModule < infos().modulesNum(); eLoopModule++)
            {
                if (infos().module(eLoopModule).mbPopulation)
                {
                    if (canSpreadModule(eLoopModule))
                    {
                        int iProb = infos().module(eLoopModule).maiAppearanceProb[(int)getLocation()];
                        if (iProb == 0)
                        {
                            iProb = 1000;
                        }

                        int iValue = randomColony().Next(iProb) + 1;
                        if (iValue > iBestValue)
                        {
                            eBestModule = eLoopModule;
                            iBestValue = iValue;
                        }
                    }
                }
            }

            if (getNextPopulationModule() != eBestModule)
            {
                meNextPopulationModule = eBestModule;

                makeDirty(GameDirtyType.meNextPopulationModule);
            }
        }

        public virtual void setTerrainCount(TerrainType eIndex, int iNewValue)
        {
            if (getTerrainCount(eIndex) != iNewValue)
            {
                maiTerrainCount[(int)eIndex] = iNewValue;

                makeDirty(GameDirtyType.maiTerrainCount);
            }
        }
        public virtual void incrementTerrainCount(TerrainType eIndex)
        {
            setTerrainCount(eIndex, (getTerrainCount(eIndex) + 1));
        }

        public virtual void changeResourceRateCount(ResourceType eIndex, int iChange)
        {
            if (iChange != 0)
            {
                maiResourceRateCount[(int)eIndex] += iChange;

                makeDirty(GameDirtyType.maiResourceRateCount);
            }
        }

        public virtual void changeModuleCount(ModuleType eIndex, int iChange)
        {
            if (iChange != 0)
            {
                maiModuleCount[(int)eIndex] += iChange;

                makeDirty(GameDirtyType.maiModuleCount);
            }
        }

        public virtual void incrementBlackMarketCount(BlackMarketType eIndex)
        {
            maiBlackMarketCount[(int)eIndex]++;

            makeDirty(GameDirtyType.maiBlackMarketCount);
        }

        public virtual void changeEspionageCount(EspionageType eIndex, int iChange)
        {
            if (iChange != 0)
            {
                maiEspionageCount[(int)eIndex] += iChange;

                makeDirty(GameDirtyType.maiEspionageCount);
            }
        }

        public virtual void incrementWholesaleSlotCount(int iIndex)
        {
            maiWholesaleSlotCount[iIndex]++;

            if (isWholesaleSlotResets(iIndex))
            {
                resetWholesaleSlotResource(iIndex);
            }

            makeDirty(GameDirtyType.maiWholesaleSlotCount);
        }

        public static void fillInvalidHQs(List<bool> abInvalidHQ, GameSettings pGameSettings, Infos pInfos)
        {
            using (new UnityProfileScope("GameServer.fillInvalidHQs"))
            {
                System.Random pRandom = new CrossPlatformRandom(pGameSettings.miSeed);

                if ((Globals.AppInfo.GameMode != GameModeType.DAILY_CHALLENGE) &&
                    (Globals.AppInfo.GameMode != GameModeType.STAND_ALONE_SERVER))
                {
                    for (HQType eLoopHQ = 0; eLoopHQ < pInfos.HQsNum(); eLoopHQ++)
                    {
                        if (pInfos.HQ(eLoopHQ).mbIoDLC)
                        {
                            if (!(Globals.AppInfo.OwnsDLCIo) && !(Globals.AppInfo.IsInternalMod))
                            {
                                abInvalidHQ[(int)eLoopHQ] = true;
                            }
                        }
                    }
                }

                if (isCampaign(pGameSettings.meLevelType) || pGameSettings.mabGameOptions[(int)GameOptionType.ALL_HQS])
                {
                    return;
                }

                int iCount = 0;

                for (HQType eLoopHQ = 0; eLoopHQ < pInfos.HQsNum(); eLoopHQ++)
                {
                    if (canFound(eLoopHQ, true, pGameSettings, pInfos) && !abInvalidHQ[(int)eLoopHQ])
                    {
                        iCount++;
                    }
                }

                while (iCount > Constants.DEFAULT_NUM_HQS)
                {
                    HQType eBestHQ = HQType.NONE;
                    int iBestValue = 0;

                    for (HQType eLoopHQ = 0; eLoopHQ < pInfos.HQsNum(); eLoopHQ++)
                    {
                        if (canFound(eLoopHQ, true, pGameSettings, pInfos) && !abInvalidHQ[(int)eLoopHQ])
                        {
                            int iValue = pRandom.Next(1000) + 1;
                            if (iValue > iBestValue)
                            {
                                eBestHQ = eLoopHQ;
                                iBestValue = iValue;
                            }
                        }
                    }

                    if (eBestHQ != HQType.NONE)
                    {
                        abInvalidHQ[(int)eBestHQ] = true;
                        iCount--;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        public virtual void makeBuildingClassFinished(BuildingClassType eIndex)
        {
            if (!isBuildingClassFinished(eIndex))
            {
                mabBuildingClassFinished[(int)eIndex] = true;

                makeDirty(GameDirtyType.mabBuildingClassFinished);
            }
        }

        public virtual void setBuildingUnavailable(BuildingType eIndex, bool bNewValue)
        {
            if (isBuildingUnavailable(eIndex) != bNewValue)
            {
                mabBuildingUnavailable[(int)eIndex] = bNewValue;

                makeDirty(GameDirtyType.mabBuildingUnavailable);
            }
        }

        public virtual void makeBlackMarketAvailable(BlackMarketType eIndex)
        {
            if (!isBlackMarketAvailable(eIndex))
            {
                mabBlackMarketAvailable[(int)eIndex] = true;

                makeDirty(GameDirtyType.mabBlackMarketAvailable);
            }
        }

        public virtual void setNextLaborModule(ModuleType eIndex, bool bNewValue)
        {
            if (isNextLaborModule(eIndex) != bNewValue)
            {
                mabNextLaborModule[(int)eIndex] = bNewValue;

                makeDirty(GameDirtyType.mabNextLaborModule);
            }
        }
        public virtual void updateNextLaborModule()
        {
            int iCount = 0;

            for (ModuleType eLoopModule = 0; eLoopModule < infos().modulesNum(); eLoopModule++)
            {
                if (isNextLaborModule(eLoopModule))
                {
                    iCount++;
                }
            }

            while (iCount < infos().Globals.NUM_LABOR_SLOTS)
            {
                ModuleType eBestModule = ModuleType.NONE;
                int iBestValue = 0;

                for (ModuleType eLoopModule = 0; eLoopModule < infos().modulesNum(); eLoopModule++)
                {
                    if (!isNextLaborModule(eLoopModule))
                    {
                        if (infos().module(eLoopModule).mbLabor)
                        {
                            if (canSpreadModule(eLoopModule, false))
                            {
                                int iProb = infos().module(eLoopModule).maiAppearanceProb[(int)getLocation()];
                                if (iProb == 0)
                                {
                                    iProb = 1000;
                                }

                                int iValue = randomColony().Next(iProb) + 1;
                                if (iValue > iBestValue)
                                {
                                    eBestModule = eLoopModule;
                                    iBestValue = iValue;
                                }
                            }
                        }
                    }
                }

                if (eBestModule != ModuleType.NONE)
                {
                    setNextLaborModule(eBestModule, true);

                    iCount++;
                }
                else
                {
                    break;
                }
            }
        }

        public virtual void setWholesaleSlotResets(int iIndex, bool bNewValue)
        {
            if (isWholesaleSlotResets(iIndex) != bNewValue)
            {
                mabWholesaleSlotResets[iIndex] = bNewValue;

                makeDirty(GameDirtyType.mabWholesaleSlotResets);
            }
        }

        public virtual void makePatentOwner(PatentType eIndex, PlayerType eNewValue)
        {
            if (getPatentOwner(eIndex) != eNewValue)
            {
                maePatentOwner[(int)eIndex] = eNewValue;

                makeDirty(GameDirtyType.maePatentOwner);
            }
        }

        protected virtual void setWholesaleSlotResource(int iIndex, ResourceType eNewValue)
        {
            if (getWholesaleSlotResource(iIndex) != eNewValue)
            {
                maeWholesaleSlotResource[iIndex] = eNewValue;

                makeDirty(GameDirtyType.maeWholesaleSlotResource);
            }
        }
        protected virtual void setWholesaleSlotResourceNext(int iIndex, ResourceType eNewValue)
        {
            if (getWholesaleSlotResourceNext(iIndex) != eNewValue)
            {
                maeWholesaleSlotResourceNext[iIndex] = eNewValue;

                makeDirty(GameDirtyType.maeWholesaleSlotResourceNext);
            }
        }
        protected virtual void resetWholesaleSlotResource(int iIndex)
        {
            ResourceType eBestResource = ResourceType.NONE;
            int iBestValue = 0;

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                if (isResourceValid(eLoopResource))
                {
                    if (infos().resource(eLoopResource).mbTrade)
                    {
                        bool bValid = true;

                        for (int iLoopSlot = 0; iLoopSlot < infos().Globals.NUM_WHOLESALE_SLOTS; iLoopSlot++)
                        {
                            if (iLoopSlot != iIndex)
                            {
                                if (getWholesaleSlotResource(iLoopSlot) == eLoopResource)
                                {
                                    bValid = false;
                                    break;
                                }
                            }
                        }

                        {
                            ColonyClassType eColonyClass = getColonyClass();

                            if (eColonyClass != ColonyClassType.NONE)
                            {
                                if (infos().colonyClass(eColonyClass).mabResourceNoWholesale[(int)eLoopResource])
                                {
                                    bValid = false;
                                }
                            }
                        }

                        if (bValid)
                        {
                            int iValue = random().Next(1000) + 1;
                            if (iValue > iBestValue)
                            {
                                eBestResource = eLoopResource;
                                iBestValue = iValue;
                            }
                        }
                    }
                }
            }


            if (getWholesaleSlotResourceNext(iIndex) == ResourceType.NONE)
            {
                if (isWholesaleSlotResets(iIndex))
                {
                    setWholesaleSlotResourceNext(iIndex, eBestResource);
                    resetWholesaleSlotResource(iIndex);
                }
                else
                {
                    setWholesaleSlotResource(iIndex, eBestResource);
                }
            }
            else
            {
                setWholesaleSlotResource(iIndex, getWholesaleSlotResourceNext(iIndex));
                setWholesaleSlotResourceNext(iIndex, eBestResource);
            }
        }

        protected virtual void addEventGameTime(EventGameTime pEventGameTime)
        {
            mEventGameTimeList.AddLast(pEventGameTime);

            makeDirty(GameDirtyType.mEventGameTimeList);
        }
        public virtual void doEventGameTimes()
        {
            LinkedListNode<EventGameTime> pNode = mEventGameTimeList.First;

            while (pNode != null)
            {
                LinkedListNode<EventGameTime> pEraseNode = null;

                if (pNode.Value.miDelay > 0)
                {
                    pNode.Value.miDelay--;

                    if (pNode.Value.miDelay == 0)
                    {
                        gameEventsServer().AddEventGame(pNode.Value.meEventGame);
                    }
                }
                else
                {
                    for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                    {
                        int iChange = infos().eventGame(pNode.Value.meEventGame).maiResourceChange[(int)eLoopResource];
                        if (iChange != 0)
                        {
                            iChange *= pNode.Value.miMultiplier;
                            iChange /= 100;

                            marketServer().changeSupply(eLoopResource, iChange);
                        }
                    }

                    if (pNode.Value.miTime > 0)
                    {
                        pNode.Value.miTime--;
                    }
                    else
                    {
                        pEraseNode = pNode;
                    }
                }

                pNode = pNode.Next;

                if (pEraseNode != null)
                {
                    mEventGameTimeList.Remove(pEraseNode);
                }

                makeDirty(GameDirtyType.mEventGameTimeList);
            }
        }

        protected virtual void doWholesaleDelay()
        {
            for (int iI = 0; iI < infos().Globals.NUM_WHOLESALE_SLOTS; iI++)
            {
                if (getWholesaleSlotDelay(iI) > 0)
                {
                    changeWholesaleSlotDelay(iI, -1);
                }
            }
        }

        public virtual MarketServer marketServer()
        {
            return (MarketServer)marketClient();
        }

        public virtual MapServer mapServer()
        {
            return (MapServer)mapClient();
        }

        public virtual GameEventsServer gameEventsServer()
        {
            return (GameEventsServer)gameEventsClient();
        }

        public virtual StatsServer statsServer()
        {
            return (StatsServer)statsClient();
        }

        public virtual ConditionManagerServer conditionManagerServer()
        {
            return (ConditionManagerServer)conditionManagerClient();
        }

        public virtual PlayerServer playerServer(PlayerType eIndex)
        {
            return (PlayerServer)playerClient(eIndex);
        }
        public virtual IEnumerable<PlayerServer> getPlayerServerAll()
        {
            return getPlayerClientAll().Cast<PlayerServer>();
        }
        public virtual IEnumerable<PlayerServer> getPlayerServerAliveAll()
        {
            return getPlayerClientAliveAll().Cast<PlayerServer>();
        }

        public virtual TileServer tileServer(int iIndex)
        {
            return (TileServer)tileClient(iIndex);
        }
        public virtual TileServer tileServerGrid(int iX, int iY)
        {
            return (TileServer)mapClient().tileClientGrid(iX, iY);
        }
        public virtual IEnumerable<TileServer> tileServerAll()
        {
            return tileClientAll().Cast<TileServer>();
        }
        public virtual TileServer tileServerAdjacent(TileServer pTile, DirectionType eDirection)
        {
            return (TileServer)mapClient().tileClientAdjacent(pTile, eDirection);
        }
        public virtual IEnumerable<TileServer> tileServerAdjacentAll(TileServer pTile)
        {
            return tileClientAdjacentAll(pTile).Cast<TileServer>();
        }
        public virtual TileServer tileServerRange(TileServer pTile, int iDX, int iDY, int iRange)
        {
            return (TileServer)tileClientRange(pTile, iDX, iDY, iRange);
        }
        public virtual IEnumerable<TileServer> tileServerRangeIterator(TileServer tile, int iRange)
        {
            return tileClientRangeIterator(tile, iRange).Cast<TileServer>();
        }

        public virtual TileGroupServer tileGroup(int iID)
        {
            if (mTileGroupDictionary.ContainsKey(iID))
            {
                return mTileGroupDictionary[iID];
            }
            else
            {
                return null;
            }
        }
        public virtual Dictionary<int, TileGroupServer> getTileGroupDictionary()
        {
            return mTileGroupDictionary;
        }
        public virtual void addTileGroup(TileGroupServer pTileGroup)
        {
            mTileGroupDictionary.Add(pTileGroup.getID(), pTileGroup);
        }
        public virtual void removeTileGroup(TileGroupServer pTileGroup)
        {
            mTileGroupDictionary.Remove(pTileGroup.getID());
        }

        public virtual ModuleServer moduleServer(int iID)
        {
            return (ModuleServer)moduleClient(iID);
        }
        protected virtual void addModule(int iID, ModuleServer pModule)
        {
            mModuleDictionary.Add(iID, pModule);
        }
        protected virtual void removeModule(ModuleServer pModule)
        {
            mModuleDictionary.Remove(pModule.getID());
        }
        protected virtual void createModule(ModuleType eType, TileServer pTile)
        {
            ModuleServer pModule = Globals.Factory.createModuleServer(this);
            int iID = nextModuleID();
            addModule(iID, pModule);
            pModule.init(this, iID, eType, pTile);

            for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
            {
                TileServer adjacentTile = tileServerAdjacent(pTile, eDirection);
                if (adjacentTile != null)
                {
                    if (infos().module(eType).mabFootprint[(int)eDirection])
                    {
                        for (DirectionType eFootprintDirection = 0; eFootprintDirection < DirectionType.NUM_TYPES; eFootprintDirection++)
                        {
                            TileServer adjacentFootprintTile = tileServerAdjacent(adjacentTile, eFootprintDirection);
                            if (adjacentFootprintTile.usable())
                                maiColonyAdjacentTiles.Add(adjacentFootprintTile.getID());
                        }
                    }
                    else
                    {
                        if (adjacentTile.usable())
                            maiColonyAdjacentTiles.Add(adjacentTile.getID());
                    }
                }
            }
        }

        public virtual HQServer hqServer(int iID)
        {
            return (HQServer)hqClient(iID);
        }
        public virtual void addHQ(int iID, HQServer pHQ)
        {
            mHQDictionary.Add(iID, pHQ);
        }
        protected virtual void removeHQ(HQServer pHQ)
        {
            mHQDictionary.Remove(pHQ.getID());
        }

        public virtual ConstructionServer constructionServer(int iID)
        {
            return (ConstructionServer)constructionClient(iID);
        }
        public virtual void addConstruction(int iID, ConstructionServer pConstruction)
        {
            mConstructionDictionary.Add(iID, pConstruction);
        }
        public virtual void removeConstruction(ConstructionServer pConstruction)
        {
            mConstructionDictionary.Remove(pConstruction.getID());
        }

        public virtual BuildingServer buildingServer(int iID)
        {
            return (BuildingServer)buildingClient(iID);
        }
        public virtual void addBuilding(int iID, BuildingServer pBuilding)
        {
            mBuildingDictionary.Add(iID, pBuilding);
        }
        public virtual void removeBuilding(BuildingServer pBuilding)
        {
            mBuildingDictionary.Remove(pBuilding.getID());
        }

        public virtual UnitServer unitServer(int iID)
        {
            return (UnitServer)unitClient(iID);
        }
        public virtual void addUnit(int iID, UnitServer pUnit)
        {
            mUnitDictionary.Add(iID, pUnit);
        }
        public virtual void removeUnit(UnitServer pUnit)
        {
            mUnitDictionary.Remove(pUnit.getID());
        }

        public virtual int nextTileGroupID()
        {
            return miNextTileGroupID++;
        }
        public virtual int nextModuleID()
        {
            return miNextModuleID++;
        }
        public virtual int nextHQID()
        {
            return miNextHQID++;
        }
        public virtual int nextConstructionID()
        {
            return miNextConstructionID++;
        }
        public virtual int nextBuildingID()
        {
            return miNextBuildingID++;
        }
        public virtual int nextUnitID()
        {
            return miNextUnitID++;
        }

        public virtual void setLastUpdateTimeToNow()
        {
            setLastUpdateTimeMS(getCurrentTimeMS());
        }

        protected virtual int getLastUpdateTimeMS()
        {
            return miLastUpdateTimeMS;
        }
        protected virtual void setLastUpdateTimeMS(int iNewValue)
        {
            miLastUpdateTimeMS = iNewValue;
        }

        protected virtual int getModuleRevealTime()
        {
            return miModuleRevealTime;
        }
        protected virtual void setModuleRevealTime(int iNewValue)
        {
            miModuleRevealTime = iNewValue;
        }
        protected virtual void changeModuleRevealTime(int iChange)
        {
            setModuleRevealTime(getModuleRevealTime() + iChange);
        }

        public virtual System.Random random()
        {
            return mRandom;
        }

        public virtual System.Random randomEvent()
        {
            return mRandomEvent;
        }

        public virtual System.Random randomColony()
        {
            return mRandomColony;
        }

        public virtual System.Random randomAuction()
        {
            return mRandomAuction;
        }

        public virtual System.Random randomAuctionPatent()
        {
            return mRandomAuctionPatent;
        }

        public virtual System.Random randomAuctionSabotage()
        {
            return mRandomAuctionSabotage;
        }

        public virtual System.Random randomAuctionTileBuilding()
        {
            return mRandomAuctionTileBuilding;
        }

        public virtual System.Random randomAuctionPerk()
        {
            return mRandomAuctionPerk;
        }

        public virtual System.Random randomMisc()
        {
            return mRandomMisc;
        }

        public virtual int getAreaTiles(int iIndex)
        {
            return maiAreaTiles[iIndex];
        }
        public virtual void incrementAreaTile(int iIndex)
        {
            maiAreaTiles[iIndex]++;
        }
        protected virtual int newArea()
        {
            maiAreaTiles.Add(0);
            return (maiAreaTiles.Count - 1);
        }

        public virtual int getWholesaleSlotDelay(int iIndex)
        {
            return maiWholesaleSlotDelay[iIndex];
        }
        public virtual void changeWholesaleSlotDelay(int iIndex, int iChange)
        {
            maiWholesaleSlotDelay[iIndex] += iChange;
        }

        public virtual bool isBuildingHasInput(BuildingType eIndex)
        {
            return mabBuildingHasInput[(int)eIndex];
        }

        protected virtual EventGameType getEventGameRoll(int iIndex)
        {
            return maeEventGameDie[iIndex];
        }
        protected virtual int getEventGameSize()
        {
            return maeEventGameDie.Count;
        }

        protected virtual bool isResourceInput(ResourceType eIndex1, ResourceType eIndex2, HQType eIndex3)
        {
            return maaabResourceInput[(int)eIndex1][(int)eIndex2][(int)eIndex3];
        }
        public virtual bool isResourceInputPlayer(ResourceType eIndex1, ResourceType eIndex2, PlayerType ePlayer)
        {
            if (playerServer(ePlayer).isBuildingFreeResource(eIndex2))
            {
                return false;
            }

            return isResourceInput(eIndex1, eIndex2, playerClient(ePlayer).getHQ());
        }
    }
}