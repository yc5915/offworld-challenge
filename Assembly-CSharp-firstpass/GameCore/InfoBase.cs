using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using UnityEngine;
using Offworld.SystemCore;

namespace Offworld.GameCore
{
    [System.Serializable]
    public abstract class InfoBase
    {
        public string mzType = "";
        public int miType = -1;

        public abstract void ReadData(XmlNode node, Infos infos);
        public virtual void UpdateReferences(Infos infos) { }
    }

    public class InfoAchievement : InfoBase
    {
        public string mzAchievement = "";
        public int miValue = 0;
        public int miAdjacentCount = 0;
        public BuildingType meBuilding = BuildingType.NONE;
        public PatentType mePatent = PatentType.NONE;
        public SabotageType meSabotage = SabotageType.NONE;
        public List<string> mazStats = new List<string>();
        public AchievementType meType { get { return (AchievementType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readString(node, "zAchievement", ref mzAchievement);
            infos.readInt(node, "iValue", ref miValue);
            infos.readInt(node, "iAdjacentCount", ref miAdjacentCount);
            infos.readType<BuildingType>(node, "Building", ref meBuilding);
            infos.readType<PatentType>(node, "Patent", ref mePatent);
            infos.readType<SabotageType>(node, "Sabotage", ref meSabotage);
            infos.readStrings(out mazStats, node, "Stats");
        }
    }

    public class InfoAdjacencyBonus : InfoBase
    {
        public int miBonusModifier = 100;
        public AdjacencyBonusType meType { get { return (AdjacencyBonusType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readInt(node, "iBonusModifier", ref miBonusModifier);
        }
    }

    public class InfoArtPack : InfoBase
    {
        public TextType meName = TextType.NONE;
        public TextType meGroupName = TextType.NONE;
        public string mzGroupIcon = "";
        public string mzIcon = "";
        public Dictionary<AssetType, string> mReplacementAssets = new Dictionary<AssetType, string>(EnumComparer<AssetType>.Instance);
        public Dictionary<AudioTypeT, string> mReplacementAudio = new Dictionary<AudioTypeT, string>(EnumComparer<AudioTypeT>.Instance);

        public ArtPackType meType { get { return (ArtPackType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readType<TextType>(node, "GroupName", ref meGroupName);
            infos.readString(node, "zGroupIcon", ref mzGroupIcon);
            infos.readString(node, "zIcon", ref mzIcon);
            infos.readStringsByType(mReplacementAssets, node, "ReplacementAssets");
            infos.readStringsByType(mReplacementAudio, node, "ReplacementAudio");
        }
    }

    public class InfoAsset : InfoBase
    {
        public string mzAsset = "";

        public AssetType meType { get { return (AssetType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readString(node, "zAsset", ref mzAsset);
        }
    }

    public class InfoAudio : InfoBase
    {
        public string mzAsset = "";
        public string mzClassicAsset = "";

        public AudioTypeT meType { get { return (AudioTypeT)miType; } } 

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readString(node, "zAsset", ref mzAsset);
            infos.readString(node, "zClassicAsset", ref mzClassicAsset);
        }
    }

    public class InfoBlackMarket : InfoBase
    {
        public TextType meName = TextType.NONE;
        public TextType meHelptext = TextType.NONE;
        public string mzIconName = "";
        public int miBaseCost = 0;
        public int miDelayModifier = 0;
        public int miAppearanceProb = 0;
        public int miMinTeams = 0;
        public int miNewClaims = 0;
        public int miBondRatingChange = 0;
        public int miSabotageCount = 0;
        public int miTriggersDefense = 0;
        public int miCraftTime = 0;
        public int miPriceGrowthModifier = 100;
        public bool mbAuction = false;
        public bool mbIoDLC = false;
        public bool mbCanCraft = false;
        public AudioTypeT mePurchaseAudio = AudioTypeT.NONE;
        public BlackMarketClassType meClass = BlackMarketClassType.NONE;
        public BlackMarketType meTriggerLarge = BlackMarketType.NONE;
        public SabotageType meSabotage = SabotageType.NONE;
        public BlackMarketType meType { get { return (BlackMarketType)miType; } }
        public List<int> maiLocationAppearanceModifiers = new List<int>();

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readType<TextType>(node, "Helptext", ref meHelptext);
            infos.readString(node, "zIconName", ref mzIconName);
            infos.readInt(node, "iBaseCost", ref miBaseCost);
            infos.readInt(node, "iDelayModifier", ref miDelayModifier);
            infos.readInt(node, "iAppearanceProb", ref miAppearanceProb);
            infos.readInt(node, "iMinTeams", ref miMinTeams);
            infos.readInt(node, "iNewClaims", ref miNewClaims);
            infos.readInt(node, "iBondRatingChange", ref miBondRatingChange);
            infos.readInt(node, "iSabotageCount", ref miSabotageCount);
            infos.readInt(node, "iTriggersDefense", ref miTriggersDefense);
            infos.readInt(node, "iCraftTime", ref miCraftTime);
            infos.readInt(node, "iPriceGrowthModifier", ref miPriceGrowthModifier);
            infos.readBool(node, "bAuction", ref mbAuction);
            infos.readBool(node, "bIoDLC", ref mbIoDLC);
            infos.readBool(node, "bCanCraft", ref mbCanCraft);
            infos.readType<AudioTypeT>(node, "PurchaseAudio", ref mePurchaseAudio);
            infos.readType<BlackMarketClassType>(node, "Class", ref meClass);
            infos.readType<BlackMarketType>(node, "TriggerLarge", ref meTriggerLarge);
            infos.readType<SabotageType>(node, "Sabotage", ref meSabotage);
            infos.readIntsByType(maiLocationAppearanceModifiers, (int)infos.resourcesNum(), node, "LocationAppearanceModifier");
        }
    }

    public class InfoBlackMarketClass : InfoBase
    {
        public BlackMarketClassType meType { get { return (BlackMarketClassType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
        }
    }

    public class InfoBond : InfoBase
    {
        public TextType meName = TextType.NONE;
        public int miDebtPercent = 0;
        public int miInterestRate = 0;
        public bool mbNoBlackMarketSabotage = false;
        public BondType meType { get { return (BondType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readInt(node, "iDebtPercent", ref miDebtPercent);
            infos.readInt(node, "iInterestRate", ref miInterestRate);
            infos.readBool(node, "bNoBlackMarketSabotage", ref mbNoBlackMarketSabotage);
        }
    }

    public class InfoBuilding : InfoBase
    {
        public TextType meName = TextType.NONE;
        public string mzIconName = "";
        public string mzOrderBackgroundIconName = "";
        public AssetType meBuildingAsset = AssetType.NONE;
        public AssetType mePreviewAsset = AssetType.NONE;
        public AssetType meConstructionAsset = AssetType.NONE;
        public AssetType meDestroyedAsset = AssetType.NONE;
        public List<AssetType> maeResourceLevelBuildingAssets = new List<AssetType>();
        public List<AssetType> maeResourceLevelConstructionAssets = new List<AssetType>();
        public int miConstructionAnimDuration = 0;
        public int miOrderAnimDuration = 0;
        public int miBaseCost = 0;
        public int miMaxPerLevel = 0;
        public int miConstructionThreshold = 0;
        public int miPowerConsumption = 0;
        public int miEntertainment = 0;
        public int miEntertainmentModifier = 0;
        public bool mbAllValid = false;
        public bool mbRequiresModuleOrHQ = false;
        public bool mbUsesSun = false;
        public bool mbIce = false;
        public bool mbGeothermal = false;
        public bool mbConnections = false;
        public bool mbCanScrap = false;
        public bool mbCanShip = false;
        public bool mbSelfInput = false;
        public bool mbNoFalse = false;
        public bool mbAuction = false;
        public bool mbUsesLevelAssets = false;
        public bool mbReplacesTerrain = false;
        public bool mbShowWrongBuilding = false;
        public bool mbEventDestroy = false;
        public KeyBindingType meKeyBinding = KeyBindingType.NONE;
        public BuildingClassType meClass = BuildingClassType.NONE;
        public HQLevelType meMinLevel = HQLevelType.NONE;
        public TerrainType meTerrainRate = TerrainType.NONE;
        public WorldAudioType meWorldAudio = WorldAudioType.NONE;
        public WorldAudioType meConstructionWorldAudio = WorldAudioType.NONE;
        public AudioTypeT meConstructionCompleteAudio = AudioTypeT.NONE;
        public AudioTypeT meConstructionStartedAudio = AudioTypeT.NONE;
        public EventAudioType meConstructedEventAudio = EventAudioType.NONE;
        public EventAudioType meOrderCompleteEventAudio = EventAudioType.NONE;
        public List<int> maiResourceCost = new List<int>();
        public List<int> maiResourceMining = new List<int>();
        public List<int> maiResourceInput = new List<int>();
        public List<int> maiResourceOutput = new List<int>();
        public List<int> maiOutputLocationChange = new List<int>();
        public List<int> maiOutputHeightChange = new List<int>();
        public List<int> maiOutputWindChange = new List<int>();
        public List<int> maiTerrainProductionModifier = new List<int>();
        public List<int> maiFirstTankIndex = new List<int>();
        public List<bool> mabTerrainValid = new List<bool>();
        public List<bool> mabLocationInvalid = new List<bool>();
        public List<bool> mabIgnoreResourceRow = new List<bool>();
        public BuildingType meType { get { return (BuildingType)miType; } }
        public int miScans;
        public int miCraftSpeed;
        public BuildingType meUpgrade = BuildingType.NONE;
        public List<int> maiUpgradeCosts = new List<int>();
        public string mzUpgradeDesc = string.Empty;

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readString(node, "zIconName", ref mzIconName);
            infos.readString(node, "zOrderBackgroundIconName", ref mzOrderBackgroundIconName);
            infos.readType<AssetType>(node, "ConstructionAsset", ref meConstructionAsset);
            infos.readType<AssetType>(node, "DestroyedAsset", ref meDestroyedAsset);
            infos.readType<AssetType>(node, "BuildingAsset", ref meBuildingAsset);
            infos.readType<AssetType>(node, "PreviewAsset", ref mePreviewAsset);
            infos.readTypesByType(maeResourceLevelBuildingAssets, infos.resourceLevels().Count, node, "ResourceLevelBuildingAssets");
            infos.readTypesByType(maeResourceLevelConstructionAssets, infos.resourceLevels().Count, node, "ResourceLevelConstructionAssets");
            infos.readInt(node, "iConstructionAnimDuration", ref miConstructionAnimDuration);
            infos.readInt(node, "iOrderAnimDuration", ref miOrderAnimDuration);
            infos.readInt(node, "iBaseCost", ref miBaseCost);
            infos.readInt(node, "iMaxPerLevel", ref miMaxPerLevel);
            infos.readInt(node, "iConstructionThreshold", ref miConstructionThreshold);
            infos.readInt(node, "iPowerConsumption", ref miPowerConsumption);
            infos.readInt(node, "iEntertainment", ref miEntertainment);
            infos.readInt(node, "iEntertainmentModifier", ref miEntertainmentModifier);
            infos.readBool(node, "bAllValid", ref mbAllValid);
            infos.readBool(node, "bRequiresModuleOrHQ", ref mbRequiresModuleOrHQ);
            infos.readBool(node, "bUsesSun", ref mbUsesSun);
            infos.readBool(node, "bIce", ref mbIce);
            infos.readBool(node, "bGeothermal", ref mbGeothermal);
            infos.readBool(node, "bConnections", ref mbConnections);
            infos.readBool(node, "bCanScrap", ref mbCanScrap);
            infos.readBool(node, "bCanShip", ref mbCanShip);
            infos.readBool(node, "bSelfInput", ref mbSelfInput);
            infos.readBool(node, "bNoFalse", ref mbNoFalse);
            infos.readBool(node, "bAuction", ref mbAuction);
            infos.readBool(node, "bUsesLevelAssets", ref mbUsesLevelAssets);
            infos.readBool(node, "bReplacesTerrain", ref mbReplacesTerrain);
            infos.readBool(node, "bShowWrongBuilding", ref mbShowWrongBuilding);
            infos.readBool(node, "bEventDestroy", ref mbEventDestroy);
            infos.readType<KeyBindingType>(node, "KeyBinding", ref meKeyBinding);
            infos.readType<BuildingClassType>(node, "Class", ref meClass);
            infos.readType<HQLevelType>(node, "MinLevel", ref meMinLevel);
            infos.readType<TerrainType>(node, "TerrainRate", ref meTerrainRate);
            infos.readType<WorldAudioType>(node, "WorldAudio", ref meWorldAudio);
            infos.readType<WorldAudioType>(node, "ConstructionWorldAudio", ref meConstructionWorldAudio);
            infos.readType<AudioTypeT>(node, "ConstructionCompleteAudio", ref meConstructionCompleteAudio);
            infos.readType<AudioTypeT>(node, "ConstructionStartedAudio", ref meConstructionStartedAudio);
            infos.readType<EventAudioType>(node, "ConstructedEventAudio", ref meConstructedEventAudio);
            infos.readType<EventAudioType>(node, "OrderCompleteEventAudio", ref meOrderCompleteEventAudio);
            infos.readIntsByType(maiResourceCost, infos.resources().Count, node, "ResourceCost");
            infos.readIntsByType(maiResourceMining, infos.resources().Count, node, "ResourceMining");
            infos.readIntsByType(maiResourceInput, infos.resources().Count, node, "ResourceInput");
            infos.readIntsByType(maiResourceOutput, infos.resources().Count, node, "ResourceOutput");
            infos.readIntsByType(maiOutputLocationChange, infos.locations().Count, node, "OutputLocationChange");
            infos.readIntsByType(maiOutputHeightChange, infos.heights().Count, node, "OutputHeightChange");
            infos.readIntsByType(maiOutputWindChange, infos.winds().Count, node, "OutputWindChange");
            infos.readIntsByType(maiTerrainProductionModifier, infos.terrains().Count, node, "TerrainProductionModifier");
            infos.readIntsByType(maiFirstTankIndex, infos.resourceLevels().Count, node, "FirstTankIndex");
            infos.readBoolsByType(mabTerrainValid, infos.terrains().Count, node, "TerrainValid");
            infos.readBoolsByType(mabLocationInvalid, infos.locations().Count, node, "LocationInvalid");
            infos.readBoolsByType(mabIgnoreResourceRow, infos.resources().Count, node, "IgnoreResourceRow");

            infos.readInt(node, "iScans", ref miScans);
            infos.readInt(node, "iCraftSpeed", ref miCraftSpeed);
            infos.readType<BuildingType>(node, "UpgradeType", ref meUpgrade);
            infos.readIntsByType(maiUpgradeCosts, (int)infos.resourcesNum(), node, "UpgradeCosts");
            infos.readString(node, "UpgradeDesc", ref mzUpgradeDesc);
        }
    }

    public class InfoBuildingClass : InfoBase
    {
        public TextType meName = TextType.NONE;
        public TextType meHelptext = TextType.NONE;
        public TextType meSummaryOutputText = TextType.NONE;
        public bool mbSteamStat = false;
        public BuildingType meBaseBuilding = BuildingType.NONE;
        public OrderType meOrderType = OrderType.NONE;
        public List<bool> mabAdjacentBuildingClassBonus = new List<bool>();
        public List<bool> mabLocationNoPerks = new List<bool>();
        public BuildingClassType meType { get { return (BuildingClassType)miType; } } 

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readType<TextType>(node, "Helptext", ref meHelptext);
            infos.readType<TextType>(node, "SummaryOutputText", ref meSummaryOutputText);
            infos.readBool(node, "bSteamStat", ref mbSteamStat);
            infos.readType<BuildingType>(node, "BaseBuilding", ref meBaseBuilding);
            infos.readType<OrderType>(node, "OrderType", ref meOrderType);
            infos.readBoolsByType(mabAdjacentBuildingClassBonus, infos.buildingClasses().Count, node, "AdjacentBuildingClassBonus");
            infos.readBoolsByType(mabLocationNoPerks, infos.locations().Count, node, "LocationNoPerks");
        }
    }

    public class InfoCampaignMode : InfoBase
    {
        public TextType meName = TextType.NONE;
        public TextType meLinkedIntroTitle = TextType.NONE;
        public TextType meLinkedIntroText = TextType.NONE;
        public string mzAchievement1 = "";
        public string mzAchievement2 = "";
        public int miSeed = 0;
        public int miStartingCorps = 0;
        public int miFinalCorps = 0;
        public int miStartingMoney = 0;
        public int miBaseIncome = 0;
        public int miIncomePerPopulationBase = 0;
        public int miIncomePerPopulationPer = 0;
        public int miAdjacencyIncome = 0;
        public int miBaseShareValue = 0;
        public int miDebtInterest = 0;
        public int miNumGrowthRounds = 0;
        public int miFinalPerkCostModifier = 0;
        public int miFirstTurnBuyPermanent = 0;
        public bool mbMission = false;
        public bool mbOneFinal = false;
        public bool mbValidAll = false;
        public bool mbCampaignDLC = false;
        public bool mbIoDLC = false;
        public bool mbBobCampaignDLC = false;
        public bool mbNoCeres = false;
        public bool mbAlternateSevenSols = false;
        public bool mbPerkVictory = false;
        public bool mbSecondVictory = false;
        public bool mbFinalBuyout = false;
        public bool mbColonyClassAll = false;
        public bool mbLevelAll = false;
        public ColonyBonusLevelType meStartingColonyBonusLevel = ColonyBonusLevelType.NONE;
        public ColonyBonusLevelType meMaxColonyBonusLevel = ColonyBonusLevelType.NONE;
        public List<int> maiEventGameProb = new List<int>();
        public List<int> maiPerkAvailable = new List<int>();
        public List<bool> mabPerkVictory = new List<bool>();
        public List<bool> mabExecutiveValid = new List<bool>();
        public List<bool> mabColonyClassValid = new List<bool>();
        public List<bool> mabColonyClassInvalid = new List<bool>();
        public List<bool> mabLinkedIntroMode = new List<bool>();
        public List<bool> mabWonModeValid = new List<bool>();
        public List<bool> mabModeClear = new List<bool>();
        public List<bool> mabWonModeSecond = new List<bool>();
        public List<bool> mabWonModePerk = new List<bool>();
        public List<bool> mabLocationInvalid = new List<bool>();
        public List<bool> mabLevelValid = new List<bool>();
        public List<bool> mabEventGameInvalid = new List<bool>();
        public List<TextType> maeWonPerkTitle = new List<TextType>();
        public List<TextType> maeWonPerkText = new List<TextType>();
        public List<EventGameType> maeWeekEventGame = new List<EventGameType>();
        public CampaignModeType meType { get { return (CampaignModeType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readType<TextType>(node, "LinkedIntroTitle", ref meLinkedIntroTitle);
            infos.readType<TextType>(node, "LinkedIntroText", ref meLinkedIntroText);
            infos.readString(node, "zAchievement1", ref mzAchievement1);
            infos.readString(node, "zAchievement2", ref mzAchievement2);
            infos.readInt(node, "iSeed", ref miSeed);
            infos.readInt(node, "iStartingCorps", ref miStartingCorps);
            infos.readInt(node, "iFinalCorps", ref miFinalCorps);
            infos.readInt(node, "iStartingMoney", ref miStartingMoney);
            infos.readInt(node, "iBaseIncome", ref miBaseIncome);
            infos.readInt(node, "iIncomePerPopulationBase", ref miIncomePerPopulationBase);
            infos.readInt(node, "iIncomePerPopulationPer", ref miIncomePerPopulationPer);
            infos.readInt(node, "iAdjacencyIncome", ref miAdjacencyIncome);
            infos.readInt(node, "iBaseShareValue", ref miBaseShareValue);
            infos.readInt(node, "iDebtInterest", ref miDebtInterest);
            infos.readInt(node, "iNumGrowthRounds", ref miNumGrowthRounds);
            infos.readInt(node, "iFinalPerkCostModifier", ref miFinalPerkCostModifier);
            infos.readInt(node, "iFirstTurnBuyPermanent", ref miFirstTurnBuyPermanent);
            infos.readBool(node, "bMission", ref mbMission);
            infos.readBool(node, "bOneFinal", ref mbOneFinal);
            infos.readBool(node, "bValidAll", ref mbValidAll);
            infos.readBool(node, "bCampaignDLC", ref mbCampaignDLC);
            infos.readBool(node, "bIoDLC", ref mbIoDLC);
            infos.readBool(node, "bBobCampaignDLC", ref mbBobCampaignDLC);
            infos.readBool(node, "bNoCeres", ref mbNoCeres);
            infos.readBool(node, "bAlternateSevenSols", ref mbAlternateSevenSols);
            infos.readBool(node, "bPerkVictory", ref mbPerkVictory);
            infos.readBool(node, "bSecondVictory", ref mbSecondVictory);
            infos.readBool(node, "bFinalBuyout", ref mbFinalBuyout);
            infos.readBool(node, "bColonyClassAll", ref mbColonyClassAll);
            infos.readBool(node, "bLevelAll", ref mbLevelAll);
            infos.readType<ColonyBonusLevelType>(node, "StartingColonyBonusLevel", ref meStartingColonyBonusLevel);
            infos.readType<ColonyBonusLevelType>(node, "MaxColonyBonusLevel", ref meMaxColonyBonusLevel);
            infos.readIntsByType(maiEventGameProb, infos.eventGames().Count, node, "EventGameProb");
            infos.readIntsByType(maiPerkAvailable, infos.perks().Count, node, "PerkAvailable");
            infos.readBoolsByType(mabPerkVictory, infos.perks().Count, node, "PerkVictory");
            infos.readBoolsByType(mabExecutiveValid, infos.executives().Count, node, "ExecutiveValid");
            infos.readBoolsByType(mabColonyClassValid, infos.colonyClasses().Count, node, "ColonyClassValid");
            infos.readBoolsByType(mabColonyClassInvalid, infos.colonyClasses().Count, node, "ColonyClassInvalid");
            infos.readBoolsByType(mabLinkedIntroMode, infos.campaignModes().Count, node, "LinkedIntroMode");
            infos.readBoolsByType(mabModeClear, infos.campaignModes().Count, node, "ModeClear");
            infos.readBoolsByType(mabWonModeValid, infos.campaignModes().Count, node, "WonModeValid");
            infos.readBoolsByType(mabWonModePerk, infos.campaignModes().Count, node, "WonModePerk");
            infos.readBoolsByType(mabWonModeSecond, infos.campaignModes().Count, node, "WonModeSecond");
            infos.readBoolsByType(mabLocationInvalid, infos.locations().Count, node, "LocationInvalid");
            infos.readBoolsByType(mabLevelValid, infos.levels().Count, node, "LevelValid");
            infos.readBoolsByType(mabEventGameInvalid, infos.eventGames().Count, node, "EventGameInvalid");
            infos.readTypesByType(maeWonPerkTitle, infos.perks().Count, node, "WonPerkTitle");
            infos.readTypesByType(maeWonPerkText, infos.perks().Count, node, "WonPerkText");
            infos.readTypeStrings<EventGameType>(out maeWeekEventGame, node, "WeekEventGame");
        }
    }

    public class InfoCharacter : InfoBase
    {
        public TextType meName = TextType.NONE;
        public TextType meDesc = TextType.NONE;
        public HQType meDefaultHQ = HQType.NONE;
        public GenderType meGender = GenderType.FEMALE;
        public PlayerColorType mePlayerColor = PlayerColorType.NONE;
        public string mzDialogArt = "";
        public string mzPortraitName = "";
        public string mzPortraitThinName = "";
        public int miCampaignCharacterSelectOrder = -1;
        public TextType meQuipBuyoutAI = TextType.NONE;
        public TextType meQuipPlayerBuy = TextType.NONE;
        public TextType meQuipCaughtSabotage = TextType.NONE;
        public TextType meQuipHitSabotage = TextType.NONE;
        public TextType mePlayerVictoryQuote = TextType.NONE;
        public TextType mePlayerHasDefeatedQuote = TextType.NONE;
        public TextType meAIVictoryQuote = TextType.NONE;
        public List<TextType> maeQuipUpgradeFirst = new List<TextType>();
        public List<TextType> maeQuipBuildingClassFirst = new List<TextType>();
        public List<TextType> maeQuipPatentAcquired = new List<TextType>();
        public List<TextType> maePlayerVictoryQuoteLocation = new List<TextType>();
        public List<TextType> maePlayerHasDefeatedQuoteLocation = new List<TextType>();
        public List<TextType> maeAIVictoryQuoteLocation = new List<TextType>();
        public List<TextType> maeQuipCampaignLead = new List<TextType>();
        public List<TextType> maeQuipCampaignTiePlayer = new List<TextType>();

        public CharacterType meType { get { return (CharacterType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readType<TextType>(node, "Desc", ref meDesc);
            infos.readType<HQType>(node, "DefaultHQ", ref meDefaultHQ);
            infos.readType<GenderType>(node, "Gender", ref meGender);
            infos.readType<PlayerColorType>(node, "PlayerColor", ref mePlayerColor);
            infos.readString(node, "zDialogArt", ref mzDialogArt);
            infos.readString(node, "zPortraitName", ref mzPortraitName);
            infos.readString(node, "zPortraitThinName", ref mzPortraitThinName);
            infos.readInt(node, "CampaignCharacterSelectOrder", ref miCampaignCharacterSelectOrder);
            infos.readType<TextType>(node, "QuipBuyoutAI", ref meQuipBuyoutAI);
            infos.readType<TextType>(node, "QuipPlayerBuy", ref meQuipPlayerBuy);
            infos.readType<TextType>(node, "QuipCaughtSabotage", ref meQuipCaughtSabotage);
            infos.readType<TextType>(node, "QuipHitSabotage", ref meQuipHitSabotage);
            infos.readType<TextType>(node, "PlayerVictoryQuote", ref mePlayerVictoryQuote);
            infos.readType<TextType>(node, "PlayerHasDefeatedQuote", ref mePlayerHasDefeatedQuote);
            infos.readType<TextType>(node, "AIVictoryQuote", ref meAIVictoryQuote);
            infos.readTypesByType(maeQuipUpgradeFirst, infos.HQLevels().Count, node, "QuipUpgradeFirst");
            infos.readTypesByType(maeQuipBuildingClassFirst, infos.buildingClasses().Count, node, "QuipBuildingClassFirst");
            infos.readTypesByType(maeQuipPatentAcquired, infos.patents().Count, node, "QuipPatentAcquired");
            infos.readTypesByType(maePlayerVictoryQuoteLocation, infos.locations().Count, node, "PlayerVictoryQuoteLocation");
            infos.readTypesByType(maePlayerHasDefeatedQuoteLocation, infos.locations().Count, node, "PlayerHasDefeatedQuoteLocation");
            infos.readTypesByType(maeAIVictoryQuoteLocation, infos.locations().Count, node, "AIVictoryQuoteLocation");
            infos.readTypeStrings<TextType>(out maeQuipCampaignLead, node, "QuipCampaignLead");
            infos.readTypeStrings<TextType>(out maeQuipCampaignTiePlayer, node, "QuipCampaignTiePlayer");
        }
    }

    public class InfoColor : InfoBase
    {
        public string mzColorCode = "";
        public Color mColor = Color.white;

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readString(node, "zColorCode", ref mzColorCode);
            infos.readHexColor(node, "zColorCode", ref mColor);
        }
    }

    public class InfoColony : InfoBase
    {
        public TextType meName = TextType.NONE;
        public string mzIconName = "";
        public ModuleType meLaborModule = ModuleType.NONE;
        public List<TextType> maeNames = new List<TextType>();
        public List<ColonyBonusType> maeColonyBonus = new List<ColonyBonusType>();
        public ColonyType meType { get { return (ColonyType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readString(node, "zIconName", ref mzIconName);
            infos.readType<ModuleType>(node, "LaborModule", ref meLaborModule);
            infos.readTypeStrings<TextType>(out maeNames, node, "Names");
            infos.readTypesByType(maeColonyBonus, infos.colonyBonusLevels().Count, node, "ColonyBonus");
        }
    }

    public class InfoColonyBonus : InfoBase
    {
        public TextType meName = TextType.NONE;
        public int miStartingShares = 0;
        public int miPerkCostModifier = 0;
        public List<int> maiSabotageCount = new List<int>();
        public List<int> maiHQLevelClaims = new List<int>();
        public ColonyBonusType meType { get { return (ColonyBonusType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readInt(node, "iStartingShares", ref miStartingShares);
            infos.readInt(node, "iPerkCostModifier", ref miPerkCostModifier);
            infos.readIntsByType(maiSabotageCount, infos.sabotages().Count, node, "SabotageCount");
            infos.readIntsByType(maiHQLevelClaims, infos.HQLevels().Count, node, "HQLevelClaims");
        }
    }

    public class InfoColonyBonusLevel : InfoBase
    {
        public TextType meName = TextType.NONE;
        public ColonyBonusLevelType meType { get { return (ColonyBonusLevelType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
        }
    }

    public class InfoColonyClass : InfoBase
    {
        public TextType meName = TextType.NONE;
        public TextType meDesc = TextType.NONE;
        public TextType meDescSkirmish = TextType.NONE;
        public TextType meStory = TextType.NONE;
        public int miAppearanceProb = 0;
        public int miColonyCapModifier = 0;
        public int miBaseShareModifier = 0;
        public int miBlackMarketTimeModifier = 0;
        public int miTechCostModifier = 0;
        public int miPatentCostModifier = 0;
        public int miModuleBonusModifier = 0;
        public int miEntertainmentModifier = 0;
        public bool mbNoColonyLabor = false;
        public List<int> maiResourceMinPrice = new List<int>();
        public List<int> maiResourceMaxPrice = new List<int>();
        public List<int> maiResourceLaborSupply = new List<int>();
        public List<int> maiBuildingClassCostModifier = new List<int>();
        public List<int> maiStartingModules = new List<int>();
        public List<bool> mabResourceNoWholesale = new List<bool>();
        public List<bool> mabBuildingClassInvalid = new List<bool>();
        public List<bool> mabPatentInvalid = new List<bool>();
        public List<bool> mabModuleValid = new List<bool>();
        public List<bool> mabModuleInvalid = new List<bool>();
        public List<bool> mabLocationInvalid = new List<bool>();
        public List<TextType> maeLocationDesc = new List<TextType>();
        public List<TextType> maeLocationDescSkirmish = new List<TextType>();
        public List<TextType> maeLocationStory = new List<TextType>();

        public ColonyClassType meType { get { return (ColonyClassType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readType<TextType>(node, "Desc", ref meDesc);
            infos.readType<TextType>(node, "DescSkirmish", ref meDescSkirmish);
            infos.readType<TextType>(node, "Story", ref meStory);
            infos.readInt(node, "iAppearanceProb", ref miAppearanceProb);
            infos.readInt(node, "iColonyCapModifier", ref miColonyCapModifier);
            infos.readInt(node, "iBaseShareModifier", ref miBaseShareModifier);
            infos.readInt(node, "iBlackMarketTimeModifier", ref miBlackMarketTimeModifier);
            infos.readInt(node, "iTechCostModifier", ref miTechCostModifier);
            infos.readInt(node, "iPatentCostModifier", ref miPatentCostModifier);
            infos.readInt(node, "iModuleBonusModifier", ref miModuleBonusModifier);
            infos.readInt(node, "iEntertainmentModifier", ref miEntertainmentModifier);
            infos.readBool(node, "bNoColonyLabor", ref mbNoColonyLabor);
            infos.readIntsByType(maiResourceMinPrice, infos.resources().Count, node, "ResourceMinPrice");
            infos.readIntsByType(maiResourceMaxPrice, infos.resources().Count, node, "ResourceMaxPrice");
            infos.readIntsByType(maiResourceLaborSupply, infos.resources().Count, node, "ResourceLaborSupply");
            infos.readIntsByType(maiBuildingClassCostModifier, infos.buildingClasses().Count, node, "BuildingClassCostModifier");
            infos.readIntsByType(maiStartingModules, infos.modules().Count, node, "StartingModules");
            infos.readBoolsByType(mabResourceNoWholesale, infos.buildings().Count, node, "ResourceNoWholesale");
            infos.readBoolsByType(mabBuildingClassInvalid, infos.buildings().Count, node, "BuildingClassInvalid");
            infos.readBoolsByType(mabPatentInvalid, infos.patents().Count, node, "PatentInvalid");
            infos.readBoolsByType(mabModuleValid, infos.modules().Count, node, "ModuleValid");
            infos.readBoolsByType(mabModuleInvalid, infos.modules().Count, node, "ModuleInvalid");
            infos.readBoolsByType(mabLocationInvalid, infos.locations().Count, node, "LocationInvalid");
            infos.readTypesByType(maeLocationDesc, infos.locations().Count, node, "LocationDesc");
            infos.readTypesByType(maeLocationDescSkirmish, infos.locations().Count, node, "LocationDescSkirmish");
            infos.readTypesByType(maeLocationStory, infos.locations().Count, node, "LocationStory");
        }
    }

    public class InfoCondition : InfoBase
    {
        public bool mbUseDefaultWin = false;
        public bool mbUseDaysWin = false;
        public bool mbNoWin = false;
        public int miRequiredMoney = 0;
        public int miRequiredDebt = 0;
        public int miRequiredDays = 0;
        public HQLevelType meRequiredHQLevel = HQLevelType.NONE;
        public List<int> maiRequiredBuildings = new List<int>();
        public List<int> maiRequiredResources = new List<int>();
        public List<int> maiRequiredOrders = new List<int>();
        public List<bool> mabRequiredPatents = new List<bool>();
        public List<TechnologyLevelType> maeRequiredTechnologyLevels = new List<TechnologyLevelType>();
        public ConditionType meType { get { return (ConditionType)miType; } } 

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readBool(node, "bUseDefaultWin", ref mbUseDefaultWin);
            infos.readBool(node, "bUseDaysWin", ref mbUseDaysWin);
            infos.readBool(node, "bNoWin", ref mbNoWin);
            infos.readInt(node, "iRequiredMoney", ref miRequiredMoney);
            infos.readInt(node, "iRequiredDebt", ref miRequiredDebt);
            infos.readInt(node, "iRequiredDays", ref miRequiredDays);
            infos.readType<HQLevelType>(node, "RequiredHQLevel", ref meRequiredHQLevel);
            infos.readIntsByType(maiRequiredBuildings, infos.buildings().Count, node, "RequiredBuildings");
            infos.readIntsByType(maiRequiredResources, infos.resources().Count, node, "RequiredResources");
            infos.readIntsByType(maiRequiredOrders, infos.orders().Count, node, "RequiredOrders");
            infos.readBoolsByType(mabRequiredPatents, infos.patents().Count, node, "RequiredPatents");
            infos.readTypesByType(maeRequiredTechnologyLevels, infos.technologies().Count, node, "RequiredTechnologyLevels");
        }
    }

    public class InfoDirection : InfoBase
    {
        public DirectionType meType { get { return (DirectionType)miType; } } 

        public override void ReadData(XmlNode node, Infos infos)
        {
        }
    }

    public class InfoEspionage : InfoBase
    {
        public TextType meName = TextType.NONE;
        public int miCost = 0;
        public int miTime = 0;
        public int miPercent = 0;
        public bool mbSurplus = false;
        public EventGameType meEventGame = EventGameType.NONE;
        public List<bool> mabLocationInvalid = new List<bool>();
        public EspionageType meType { get { return (EspionageType)miType; } } 

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readInt(node, "iCost", ref miCost);
            infos.readInt(node, "iTime", ref miTime);
            infos.readInt(node, "iPercent", ref miPercent);
            infos.readBool(node, "bSurplus", ref mbSurplus);
            infos.readType<EventGameType>(node, "EventGame", ref meEventGame);
            infos.readBoolsByType(mabLocationInvalid, infos.locations().Count, node, "LocationInvalid");
        }
    }

    public class InfoEventAudio : InfoBase
    {
        public List<AudioTypeT> maeMaleAudio = new List<AudioTypeT>();
        public List<AudioTypeT> maeFemaleAudio = new List<AudioTypeT>();

        public EventAudioType meType { get { return (EventAudioType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readTypeStrings(out maeMaleAudio, node, "MaleAudio");
            infos.readTypeStrings(out maeFemaleAudio, node, "FemaleAudio");
        }
    }

    public class InfoEventGame : InfoBase
    {
        public TextType meName = TextType.NONE;
        public TextType meMessage = TextType.NONE;
        public TextType meMessageCenter = TextType.NONE;
        public TextType meMessageDelay = TextType.NONE;
        public AudioTypeT meAudio = AudioTypeT.NONE;
        public bool mbCameraShake = false;
        public int miDieRolls = 0;
        public int miDelay = 0;
        public int miTime = 0;
        public int miClaims = 0;
        public int miDestroyPercent = 0;
        public TerrainType meAffectedTerrain = TerrainType.NONE;
        public bool mbAnnounceChat = false;
        public bool mbAnnounceDelay = false;
        public bool mbDelayCenter = false;
        public EventStateType meEventState = EventStateType.NONE;
        public EventStateType meEventStateIcon = EventStateType.NONE;
        public List<int> maiResourceChange = new List<int>();
        public List<bool> mabLocationInvalid = new List<bool>();
        public EventGameType meType { get { return (EventGameType)miType; } }
        public bool mbCreateResources = false;

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readType<TextType>(node, "Message", ref meMessage);
            infos.readType<TextType>(node, "MessageCenter", ref meMessageCenter);
            infos.readType<TextType>(node, "MessageDelay", ref meMessageDelay);
            infos.readType<AudioTypeT>(node, "Audio", ref meAudio);
            infos.readBool(node, "bCameraShake", ref mbCameraShake);
            infos.readInt(node, "iDieRolls", ref miDieRolls);
            infos.readInt(node, "iDelay", ref miDelay);
            infos.readInt(node, "iTime", ref miTime);
            infos.readInt(node, "iClaims", ref miClaims);
            infos.readInt(node, "iDestroyPercent", ref miDestroyPercent);
            infos.readType<TerrainType>(node, "AffectedTerrain", ref meAffectedTerrain);
            infos.readBool(node, "bAnnounceChat", ref mbAnnounceChat);
            infos.readBool(node, "bAnnounceDelay", ref mbAnnounceDelay);
            infos.readBool(node, "bDelayCenter", ref mbDelayCenter);
            infos.readType<EventStateType>(node, "EventState", ref meEventState);
            infos.readType<EventStateType>(node, "EventStateIcon", ref meEventStateIcon);
            infos.readIntsByType(maiResourceChange, infos.resources().Count, node, "ResourceChange");
            infos.readBoolsByType(mabLocationInvalid, infos.locations().Count, node, "LocationInvalid");
            infos.readBool(node, "bCreateResources", ref mbCreateResources);
        }
    }

    public class InfoEventLevel : InfoBase
    {
        public TextType meName = TextType.NONE;
        public TextType meDesc = TextType.NONE;
        public int miAppearanceProb1 = 0;
        public int miAppearanceProb2 = 0;
        public bool mbIoDLC = false;
        public EventStateType meEventState = EventStateType.NONE;
        public PerkType mePerk = PerkType.NONE;
        public List<int> maiStartingModules = new List<int>();
        public List<bool> mabLocationInvalid = new List<bool>();
        public List<bool> mabColonyClassInvalid = new List<bool>();
        public EventLevelType meType { get { return (EventLevelType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readType<TextType>(node, "Desc", ref meDesc);
            infos.readInt(node, "iAppearanceProb1", ref miAppearanceProb1);
            infos.readInt(node, "iAppearanceProb2", ref miAppearanceProb2);
            infos.readBool(node, "bIoDLC", ref mbIoDLC);
            infos.readType<EventStateType>(node, "EventState", ref meEventState);
            infos.readType<PerkType>(node, "Perk", ref mePerk);
            infos.readIntsByType(maiStartingModules, infos.modules().Count, node, "StartingModules");
            infos.readBoolsByType(mabLocationInvalid, infos.locations().Count, node, "LocationInvalid");
            infos.readBoolsByType(mabColonyClassInvalid, infos.colonyClasses().Count, node, "ColonyClassInvalid");
        }
    }

    public class InfoEventState : InfoBase
    {
        public TextType meName = TextType.NONE;
        public TextType meHelp = TextType.NONE;
        public string mzIconName = "";
        public int miXWindSpeedModifier = 0;
        public int miUpgradeModifier = 0;
        public int miLifeSupportModifier = 0;
        public int miBlackMarketTimeModifier = 0;
        public int miProductionModifier = 0;
        public int miConstructionModifier = 0;
        public int miShippingCapacityModifier = 0;
        public bool mbUsesSun = false;
        public bool mbEclipse = false;
        public bool mbNoAdjaceny = false;
        public List<int> maiBuildingClassModifier = new List<int>();
        public List<AssetType> maeLocationAsset = new List<AssetType>();
        public List<AssetType> maeLocationTileAsset = new List<AssetType>();
        public EventStateType meType { get { return (EventStateType)miType; } }
        public bool mbDisableBuildings = false;
        public TerrainType meAffectedTerrain = TerrainType.NONE;
        public List<List<int>> maaiBuildingProductionChange = new List<List<int>>();

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readType<TextType>(node, "Help", ref meHelp);
            infos.readString(node, "zIconName", ref mzIconName);
            infos.readInt(node, "iXWindSpeedModifier", ref miXWindSpeedModifier);
            infos.readInt(node, "iUpgradeModifier", ref miUpgradeModifier);
            infos.readInt(node, "iLifeSupportModifier", ref miLifeSupportModifier);
            infos.readInt(node, "iBlackMarketTimeModifier", ref miBlackMarketTimeModifier);
            infos.readInt(node, "iProductionModifier", ref miProductionModifier);
            infos.readInt(node, "iConstructionModifier", ref miConstructionModifier);
            infos.readInt(node, "iShippingCapacityModifier", ref miShippingCapacityModifier);
            infos.readBool(node, "bUsesSun", ref mbUsesSun);
            infos.readBool(node, "bEclipse", ref mbEclipse);
            infos.readBool(node, "bNoAdjaceny", ref mbNoAdjaceny);
            infos.readIntsByType(maiBuildingClassModifier, infos.buildings().Count, node, "BuildingClassModifier");
            infos.readTypesByType(maeLocationAsset, infos.locations().Count, node, "LocationAsset");
            infos.readTypesByType(maeLocationTileAsset, infos.locations().Count, node, "LocationTileAsset");
            infos.readBool(node, "bDisableBuildings", ref mbDisableBuildings);
            infos.readType<TerrainType>(node, "AffectedTerrain", ref meAffectedTerrain);
            infos.readIntListByType(maaiBuildingProductionChange, infos.buildingClasses().Count, infos.resources().Count, node, "BuildingClassProductionChange");
        }
    }

    public class InfoEventTurn : InfoBase
    {
        public TextType meText = TextType.NONE;
        public TextType meTextTitle = TextType.NONE;
        public TextType meTextOption1 = TextType.NONE;
        public TextType meTextOption2 = TextType.NONE;
        public TextType meTextOption3 = TextType.NONE;
        public EventTurnOptionType meOption1 = EventTurnOptionType.NONE;
        public EventTurnOptionType meOption2 = EventTurnOptionType.NONE;
        public EventTurnOptionType meOption3 = EventTurnOptionType.NONE;
        public int miAppearanceProb1 = 0;
        public int miAppearanceProb2 = 0;
        public EventTurnType meType { get { return (EventTurnType)miType; } } 

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Text", ref meText);
            infos.readType<TextType>(node, "TextTitle", ref meTextTitle);
            infos.readType<TextType>(node, "TextOption1", ref meTextOption1);
            infos.readType<TextType>(node, "TextOption2", ref meTextOption2);
            infos.readType<TextType>(node, "TextOption3", ref meTextOption3);
            infos.readType<EventTurnOptionType>(node, "Option1", ref meOption1);
            infos.readType<EventTurnOptionType>(node, "Option2", ref meOption2);
            infos.readType<EventTurnOptionType>(node, "Option3", ref meOption3);
            infos.readInt(node, "iAppearanceProb1", ref miAppearanceProb1);
            infos.readInt(node, "iAppearanceProb2", ref miAppearanceProb2);
        }
    }

    public class InfoEventTurnOption : InfoBase
    {
        public int miMoney = 0;
        public int miMoneyPerk = 0;
        public int miDebt = 0;
        public bool mbPerkUnique = false;
        public bool mbIoDLC = false;
        public List<int> maiPerkChange = new List<int>();
        public List<int> maiPerkTime = new List<int>();
        public List<bool> mabLocationInvalid = new List<bool>();
        public EventTurnOptionType meType { get { return (EventTurnOptionType)miType; } } 

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readInt(node, "iMoney", ref miMoney);
            infos.readInt(node, "iMoneyPerk", ref miMoneyPerk);
            infos.readInt(node, "iDebt", ref miDebt);
            infos.readBool(node, "bPerkUnique", ref mbPerkUnique);
            infos.readBool(node, "bIoDLC", ref mbIoDLC);
            infos.readIntsByType(maiPerkChange, infos.perks().Count, node, "PerkChange");
            infos.readIntsByType(maiPerkTime, infos.perks().Count, node, "PerkTime");
            infos.readBoolsByType(mabLocationInvalid, infos.locations().Count, node, "LocationInvalid");
        }
    }

    public class InfoExecutive : InfoBase
    {
        public PersonalityType mePersonality = PersonalityType.NONE;
        public bool mbCampaignDLC = false;
        public bool mbIoDLC = false;
        public List<int> maiPerkCount = new List<int>();
        public List<bool> mabExecutiveUnlock = new List<bool>();
        public ExecutiveType meType { get { return (ExecutiveType)miType; } } 

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<PersonalityType>(node, "Personality", ref mePersonality);
            infos.readBool(node, "bCampaignDLC", ref mbCampaignDLC);
            infos.readBool(node, "bIoDLC", ref mbIoDLC);
            infos.readIntsByType(maiPerkCount, infos.perks().Count, node, "PerkCount");
            infos.readBoolsByType(mabExecutiveUnlock, infos.executives().Count, node, "ExecutiveUnlock");
        }
    }

    public class InfoGameOption : InfoBase
    {
        public TextType meName = TextType.NONE;
        public TextType meDescription = TextType.NONE;
        public TextType meLobbyToggleOutput = TextType.NONE;
        public bool mbDefaultValueSP = false;
        public bool mbDefaultValueCampaign = false;
        public bool mbDefaultValueMP = false;
        public bool mbDefaultValueQM = false;
        public bool mbMultiPlayerOption = false; // Only for multiplayer games
        public bool mbSinglePlayerOption = false;  // Only for single player games
        public bool mbCampaignOption = false;  // Only for campaign games
        public bool mbPriorityOption = false; // should this option be displayed in a seperate list as other options?
        public bool mbRequiresDLCMaps = false;
        public bool mbRequiresDLCCeres = false;
        public List<bool> mabLocationInvalid = new List<bool>();
        public GameOptionType meType { get { return (GameOptionType)miType; } } 

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readType<TextType>(node, "Description", ref meDescription);
            infos.readType<TextType>(node, "LobbyToggleOutput", ref meLobbyToggleOutput);
            infos.readBool(node, "bDefaultValueSP", ref mbDefaultValueSP);
            infos.readBool(node, "bDefaultValueCampaign", ref mbDefaultValueCampaign);
            infos.readBool(node, "bDefaultValueMP", ref mbDefaultValueMP);
            infos.readBool(node, "bDefaultValueQM", ref mbDefaultValueQM);
            infos.readBool(node, "bMultiPlayerOption", ref mbMultiPlayerOption);
            infos.readBool(node, "bSinglePlayerOption", ref mbSinglePlayerOption);
            infos.readBool(node, "bCampaignOption", ref mbCampaignOption);
            infos.readBool(node, "bPriorityOption", ref mbPriorityOption);
            infos.readBool(node, "bRequiresDLCMaps", ref mbRequiresDLCMaps);
            infos.readBool(node, "bRequiresDLCCeres", ref mbRequiresDLCCeres);
            infos.readBoolsByType(mabLocationInvalid, infos.locations().Count, node, "LocationInvalid");
        }
    }

    public class InfoGamePhase : InfoBase
    {
        public GamePhaseType meType { get { return (GamePhaseType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
        }
    }

    public class InfoGameSetup : InfoBase
    {
        public TextType meIntroTitle = TextType.NONE;
        public TextType meIntroBody = TextType.NONE;
        public TextType meIntroButton = TextType.NONE;
        public bool mbDelayIntro = false;
        public string mzMap = "";
        public bool mbSkipColonyClassIntro = false;
        public LocationType meLocation = LocationType.NONE;
        public RulesSetType meRulesSet = RulesSetType.NONE;
        public List<LightingEnvironmentType> lightingEnvironments = new List<LightingEnvironmentType>();
        public List<ConditionType> winConditions = new List<ConditionType>();
        public List<ConditionType> loseConditions = new List<ConditionType>();
        public GameSetupType meType { get { return (GameSetupType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "IntroTitle", ref meIntroTitle);
            infos.readType<TextType>(node, "IntroBody", ref meIntroBody);
            infos.readType<TextType>(node, "IntroButton", ref meIntroButton);
            infos.readBool(node, "bDelayIntro", ref mbDelayIntro);
            infos.readString(node, "Map", ref mzMap);
            infos.readBool(node, "bSkipColonyClassIntro", ref mbSkipColonyClassIntro);
            infos.readType<LocationType>(node, "Location", ref meLocation);
            infos.readType<RulesSetType>(node, "RulesSet", ref meRulesSet);
            infos.readTypeStrings<LightingEnvironmentType>(out lightingEnvironments, node, "LightingEnvironments");
            infos.readTypeStrings<ConditionType>(out winConditions, node, "WinConditions");
            infos.readTypeStrings<ConditionType>(out loseConditions, node, "LoseConditions");
        }
    }

    public class InfoGameSpeed : InfoBase
    {
        public TextType meName = TextType.NONE;
        public int miSkipUpdates = 0;
        public AudioTypeT mAudioSpeedUp = AudioTypeT.NONE;
        public AudioTypeT mAudioSpeedDown = AudioTypeT.NONE;
        public GameSpeedType meType { get { return (GameSpeedType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readType<AudioTypeT>(node, "AudioSpeedUp", ref mAudioSpeedUp);
            infos.readType<AudioTypeT>(node, "AudioSpeedDown", ref mAudioSpeedDown);
            infos.readInt(node, "iSkipUpdates", ref miSkipUpdates);
        }
    }

    public class InfoGender : InfoBase
    {
        public GenderType meType { get { return (GenderType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
        }
    }

    public class InfoGlobalsInt : InfoBase
    {
        public int miValue = 0;
        public GlobalsIntType meType { get { return (GlobalsIntType)miType; } } 

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readInt(node, "iValue", ref miValue);
        }
    }

    public class InfoGlobalsFloat : InfoBase
    {
        public float mfValue = 0;
        public GlobalsFloatType meType { get { return (GlobalsFloatType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readFloat(node, "fValue", ref mfValue);
        }
    }

    public class InfoGlobalsString : InfoBase
    {
        public string mzValue = "";
        public GlobalsStringType meType { get { return (GlobalsStringType)miType; } } 

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readString(node, "zValue", ref mzValue);
        }
    }

    public class InfoGlobalsType : InfoBase
    {
        public int meValue = 0;
        public GlobalsTypeType meType { get { return (GlobalsTypeType)miType; } } 

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<int>(node, "zValue", ref meValue);
        }
    }

    public class InfoHandicap : InfoBase
    {
        public TextType meName = TextType.NONE;
        public TextType meNameSP = TextType.NONE;
        public TextType meDesc = TextType.NONE;
        public int miMoney = 0;
        public int miClaims = 0;
        public int miShares = 0;
        public int miDebtMultiplier = 0;
        public int miStockpileModifier = 0;
        public int miStockpileLifeSupport = 0;
        public int miProductionModifier = 0;
        public int miUpgradeModifier = 0;
        public int miBaseResourceModifier = 0;
        public int miBlackMarketCostModifier = 0;
        public int miBlackMarketTimeModifier = 0;
        public int miBaseShareModifier = 0;
        public int miAIStockRoll = 0;
        public int miAIProductionModifier = 0;
        public int miAILifeSupportModifier = 0;
        public int miAIPowerConsumptionModifier = 0;
        public int miAIGasConsumptionModifier = 0;
        public int miAISabotageModifier = 0;
        public int miAICampaignIncome = 0;
        public bool mbAchievement = false;
        public HandicapType meType { get { return (HandicapType)miType; } } 

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readType<TextType>(node, "NameSP", ref meNameSP);
            infos.readType<TextType>(node, "Desc", ref meDesc);
            infos.readInt(node, "iMoney", ref miMoney);
            infos.readInt(node, "iClaims", ref miClaims);
            infos.readInt(node, "iShares", ref miShares);
            infos.readInt(node, "iDebtMultiplier", ref miDebtMultiplier);
            infos.readInt(node, "iStockpileModifier", ref miStockpileModifier);
            infos.readInt(node, "iStockpileLifeSupport", ref miStockpileLifeSupport);
            infos.readInt(node, "iProductionModifier", ref miProductionModifier);
            infos.readInt(node, "iUpgradeModifier", ref miUpgradeModifier);
            infos.readInt(node, "iBaseResourceModifier", ref miBaseResourceModifier);
            infos.readInt(node, "iBlackMarketCostModifier", ref miBlackMarketCostModifier);
            infos.readInt(node, "iBlackMarketTimeModifier", ref miBlackMarketTimeModifier);
            infos.readInt(node, "iBaseShareModifier", ref miBaseShareModifier);
            infos.readInt(node, "iAIStockRoll", ref miAIStockRoll);
            infos.readInt(node, "iAIProductionModifier", ref miAIProductionModifier);
            infos.readInt(node, "iAILifeSupportModifier", ref miAILifeSupportModifier);
            infos.readInt(node, "iAIPowerConsumptionModifier", ref miAIPowerConsumptionModifier);
            infos.readInt(node, "iAIGasConsumptionModifier", ref miAIGasConsumptionModifier);
            infos.readInt(node, "iAISabotageModifier", ref miAISabotageModifier);
            infos.readInt(node, "iAICampaignIncome", ref miAICampaignIncome);
            infos.readBool(node, "bAchievement", ref mbAchievement);
        }
    }

    public class InfoHeight : InfoBase
    {
        public TextType meName = TextType.NONE;
        public AssetType meAsset = AssetType.NONE;
        public HeightType meType { get { return (HeightType)miType; } } 

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readType<AssetType>(node, "Asset", ref meAsset);

        }
    }

    public class InfoHQ : InfoBase
    {
        public TextType meName = TextType.NONE;
        public TextType meNameShort = TextType.NONE;
        public TextType meBonusText = TextType.NONE;
        public string mzIconName = "";
        public string mzFootprintIconName = "";
        public AssetType meHQAsset = AssetType.NONE;
        public AssetType mePreviewAsset = AssetType.NONE;
        public AssetType meWinScreenAsset = AssetType.NONE;
        public AssetType meUpgradeAsset = AssetType.NONE;
        public AssetType meFoundButtonAsset = AssetType.NONE;
        public AssetType meFoundTooltipAsset = AssetType.NONE;
        public int miExtraHQs = 0;
        public int miBuildingHQLevel = 0;
        public int miClaims = 0;
        public int miClaimsPerUpgrade = 0;
        public int miShares = 0;
        public int miBaseResourceModifier = 0;
        public int miHQBonusModifier = 0;
        public int miMovementModifier = 0;
        public int miBlackMarketHostileTimeModifier = 0;
        public int miFrozenEffectModifier = 0;
        public int miEntertainmentDemand = 0;
        public int miAIGeothermalWeight = 0;
        public int miAIResourceMinRangeFound = 0;
        public bool mbAdjacentInputBonus = false;
        public bool mbEarlyEventAnnounce = false;
        public bool mbNoDeplete = false;
        public bool mbDoubleHack = false;
        public bool mbLicensePatents = false;
        public bool mbIoDLC = false;
        public ResourceType meBaseResource = ResourceType.NONE;
        public ResourceType meGasResource = ResourceType.NONE;
        public TechnologyLevelType meMaxTechnologyLevel = TechnologyLevelType.NONE;
        public KeyBindingType meKeyBinding = KeyBindingType.NONE;
        public List<WorldAudioType> mezUpgradeAnimationAudio = new List<WorldAudioType>();
        public List<AudioTypeT> mazFoundAudio1st = new List<AudioTypeT>();
        public List<AudioTypeT> mazUpgradeAudio1st = new List<AudioTypeT>();
        public EventAudioType meFoundEventAudio3rd = EventAudioType.NONE;
        public EventAudioType meUpgradeEventAudio3rd = EventAudioType.NONE;
        public List<int> maiInitialStockpile = new List<int>();
        public List<int> maiUpgradeResource = new List<int>();
        public List<int> maiLifeSupport = new List<int>();
        public List<int> maiFoundSabotage = new List<int>();
        public List<int> maiUpgradeSabotage = new List<int>();
        public List<int> maiOrderModifier = new List<int>();
        public List<int> maiFreeResourceModifier = new List<int>();
        public List<int> maiAIResourceWeight = new List<int>();
        public List<List<int>> maaiAILocationResourceWeightModifier = new List<List<int>>();
        public List<int> maiAIResourceNearby = new List<int>();
        public List<int> maiAIResourceMinRate = new List<int>();
        public List<int> maiAIResourceMinCount = new List<int>();
        public List<int> maiAIResourceMinRateFound = new List<int>();
        public List<int> maiAIResourceMinCountFound = new List<int>();
        public List<bool> mabFootprint = new List<bool>();
        public List<bool> mabBuildingValid = new List<bool>();
        public List<ResourceType> maeResourceReplace = new List<ResourceType>();
        public List<TextType> maeBonusLocationText = new List<TextType>();
        public HQType meType { get { return (HQType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readType<TextType>(node, "NameShort", ref meNameShort);
            infos.readType<TextType>(node, "BonusText", ref meBonusText);
            infos.readString(node, "zIconName", ref mzIconName);
            infos.readString(node, "zFootprintIconName", ref mzFootprintIconName);
            infos.readType<AssetType>(node, "HQAsset", ref meHQAsset);
            infos.readType<AssetType>(node, "PreviewAsset", ref mePreviewAsset);
            infos.readType<AssetType>(node, "WinScreenAsset", ref meWinScreenAsset);
            infos.readType<AssetType>(node, "UpgradeAsset", ref meUpgradeAsset);
            infos.readType<AssetType>(node, "FoundButtonAsset", ref meFoundButtonAsset);
            infos.readType<AssetType>(node, "FoundTooltipAsset", ref meFoundTooltipAsset);
            infos.readInt(node, "iExtraHQs", ref miExtraHQs); 
            infos.readInt(node, "iBuildingHQLevel", ref miBuildingHQLevel);
            infos.readInt(node, "iClaims", ref miClaims);
            infos.readInt(node, "iClaimsPerUpgrade", ref miClaimsPerUpgrade);
            infos.readInt(node, "iShares", ref miShares);
            infos.readInt(node, "iBaseResourceModifier", ref miBaseResourceModifier);
            infos.readInt(node, "iHQBonusModifier", ref miHQBonusModifier);
            infos.readInt(node, "iMovementModifier", ref miMovementModifier);
            infos.readInt(node, "iBlackMarketHostileTimeModifier", ref miBlackMarketHostileTimeModifier);
            infos.readInt(node, "iFrozenEffectModifier", ref miFrozenEffectModifier);
            infos.readInt(node, "iEntertainmentDemand", ref miEntertainmentDemand);
            infos.readInt(node, "iAIGeothermalWeight", ref miAIGeothermalWeight);
            infos.readInt(node, "iAIResourceMinRangeFound", ref miAIResourceMinRangeFound);
            infos.readBool(node, "bAdjacentInputBonus", ref mbAdjacentInputBonus);
            infos.readBool(node, "bEarlyEventAnnounce", ref mbEarlyEventAnnounce);
            infos.readBool(node, "bNoDeplete", ref mbNoDeplete);
            infos.readBool(node, "bDoubleHack", ref mbDoubleHack);
            infos.readBool(node, "bLicensePatents", ref mbLicensePatents);
            infos.readBool(node, "bIoDLC", ref mbIoDLC);
            infos.readType<ResourceType>(node, "BaseResource", ref meBaseResource);
            infos.readType<ResourceType>(node, "GasResource", ref meGasResource);
            infos.readType<TechnologyLevelType>(node, "MaxTechnologyLevel", ref meMaxTechnologyLevel);
            infos.readType<KeyBindingType>(node, "KeyBinding", ref meKeyBinding);
            infos.readTypesByType(mezUpgradeAnimationAudio, infos.HQLevels().Count, node, "UpgradeAnimationAudio");
            infos.readTypeStrings(out mazFoundAudio1st, node, "FoundAudio1st");
            infos.readTypeStrings(out mazUpgradeAudio1st, node, "UpgradeAudio1st");
            infos.readType<EventAudioType>(node, "FoundEventAudio3rd", ref meFoundEventAudio3rd);
            infos.readType<EventAudioType>(node, "UpgradeEventAudio3rd", ref meUpgradeEventAudio3rd);
            infos.readIntsByType(maiInitialStockpile, infos.resources().Count, node, "InitialStockpile");
            infos.readIntsByType(maiUpgradeResource, infos.resources().Count, node, "UpgradeResource");
            infos.readIntsByType(maiLifeSupport, infos.resources().Count, node, "LifeSupport");
            infos.readIntsByType(maiFoundSabotage, infos.sabotages().Count, node, "FoundSabotage");
            infos.readIntsByType(maiUpgradeSabotage, infos.sabotages().Count, node, "UpgradeSabotage");
            infos.readIntsByType(maiOrderModifier, infos.orders().Count, node, "OrderModifier");
            infos.readIntsByType(maiFreeResourceModifier, infos.resources().Count, node, "FreeResourceModifier");
            infos.readIntsByType(maiAIResourceWeight, infos.resources().Count, node, "AIResourceWeight");
            infos.readIntListByType(maaiAILocationResourceWeightModifier, infos.locations().Count, infos.resources().Count, node, "AILocationResourceWeightModifier");
            infos.readIntsByType(maiAIResourceNearby, infos.resources().Count, node, "AIResourceNearby");
            infos.readIntsByType(maiAIResourceMinRate, infos.resources().Count, node, "AIResourceMinRate");
            infos.readIntsByType(maiAIResourceMinCount, infos.resources().Count, node, "AIResourceMinCount");
            infos.readIntsByType(maiAIResourceMinRateFound, infos.resources().Count, node, "AIResourceMinRateFound");
            infos.readIntsByType(maiAIResourceMinCountFound, infos.resources().Count, node, "AIResourceMinCountFound");
            infos.readBoolsByType(mabFootprint, infos.directions().Count, node, "Footprint");
            infos.readBoolsByType(mabBuildingValid, infos.buildings().Count, node, "BuildingValid");
            infos.readTypesByType(maeResourceReplace, infos.resources().Count, node, "ResourceReplace");
            infos.readTypesByType(maeBonusLocationText, infos.locations().Count, node, "BonusLocationText");
        }
    }

    public class InfoHQLevel : InfoBase
    {
        public TextType meName = TextType.NONE;
        public string mzFoleySound = "";
        public int miUpgradeClaims = 0;
        public List<int> maiHandicapUpgradeClaims = new List<int>();
        public HQLevelType meType { get { return (HQLevelType)miType; } } 

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readString(node, "zFoleySound", ref mzFoleySound);
            infos.readInt(node, "iUpgradeClaims", ref miUpgradeClaims);
            infos.readIntsByType(maiHandicapUpgradeClaims, infos.handicaps().Count, node, "HandicapUpgradeClaims");
        }
    }

    public class InfoIce : InfoBase
    {
        public TextType meName = TextType.NONE;
        public string mzTileIconName = "";
        public IceType meType { get { return (IceType)miType; } }
        public ColorType meHighlightColor = ColorType.NONE;
        public List<int> maiLocationLatitudeMin = new List<int>();
        public List<int> maiLocationLatitudeMax = new List<int>();
        public List<int> maiLocationAppearanceProb = new List<int>();
        public List<int> maiLocationAppearancePercent = new List<int>();
        public List<int> maiIgnoreMinimum = new List<int>();
        public List<int> maiAverageResourceRate = new List<int>();
        public List<List<int>> maaiResourceProbModifier = new List<List<int>>();

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readString(node, "zTileIconName", ref mzTileIconName);
            infos.readType<ColorType>(node, "HighlightColor", ref meHighlightColor);
            infos.readIntsByType(maiLocationLatitudeMin, infos.locations().Count, node, "LocationLatitudeMin");
            infos.readIntsByType(maiLocationLatitudeMax, infos.locations().Count, node, "LocationLatitudeMax");
            infos.readIntsByType(maiLocationAppearanceProb, infos.locations().Count, node, "LocationAppearanceProb");
            infos.readIntsByType(maiLocationAppearancePercent, infos.locations().Count, node, "LocationAppearancePercent");
            infos.readIntsByType(maiIgnoreMinimum, infos.resources().Count, node, "IgnoreMinimum");
            infos.readIntsByType(maiAverageResourceRate, infos.resources().Count, node, "AverageResourceRate");
            infos.readIntListByType(maaiResourceProbModifier, infos.resources().Count, infos.locations().Count, node, "ResourceProbModifier");
        }
    }

    [System.Serializable]
    public class KeyCombo : List<KeyCode> 
    {
        public KeyCombo() { }
        public KeyCombo(IEnumerable<KeyCode> collection) : base(collection) { }
    }

    [System.Serializable]
    public class KeyBinding : List<KeyCombo> 
    {
        public KeyBinding() { }
        public KeyBinding(IEnumerable<KeyCombo> collection) : base(collection) { }
    }

    [System.Serializable]
    public class InfoKeyBinding : InfoBase
    {
        public KeyBinding mcKeyCodes = new  KeyBinding();
        public KeyBinding mcKeyCodesSorted = new  KeyBinding();
        public KeyBinding mcKeyCodesOSX = new KeyBinding();
        public KeyBinding mcKeyCodesSortedOSX = new KeyBinding();
        public TextType meText = TextType.NONE;
        public bool mbOnKeyUp = false;
        public bool mbOnKeyHold = false;
        public KeyBindingClassType meClass = KeyBindingClassType.NONE;
        public KeyBindingType meType { get { return (KeyBindingType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readBool(node, "bOnKeyUp", ref mbOnKeyUp);
            infos.readBool(node, "bOnKeyHold", ref mbOnKeyHold);
            infos.readType<KeyBindingClassType>(node, "zClass", ref meClass);
            infos.readType<TextType>(node, "zText", ref meText);
            
            infos.readHotkeys(mcKeyCodes, infos.readString(node, "Hotkey"));
            infos.readHotkeys(mcKeyCodesOSX, infos.readString(node, "OSXHotkey"));
            mcKeyCodesSorted.AddRange(mcKeyCodes.Select(keyCombo => new KeyCombo(keyCombo.OrderBy(key => key))));
            mcKeyCodesSortedOSX.AddRange(mcKeyCodesOSX.Select(keyCombo => new KeyCombo(keyCombo.OrderBy(key => key))));
        }
    }

    public class InfoKeyBindingClass : InfoBase
    {
        public bool mbGameData = false;
        public GamePhaseType mePhase = GamePhaseType.NONE;
        public KeyBindingClassType meType { get { return (KeyBindingClassType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<GamePhaseType>(node, "zPhase", ref mePhase);
            infos.readBool(node, "bGameData", ref mbGameData);
        }
    }

    public class InfoLanguage : InfoBase
    {
        public TextType meName = TextType.NONE;
        public string mzFieldName = "";
        public string mzISO6301Name = "";
        public int miExportColumn = 0;
        public bool mbPublicBuild = false;
        public bool mbTranslation = true;
        public string mzSingularExpression = "";
        public string mzPluralExpression = "";
        public string mzThousandsSeparator = "";
        public string mzDecimalSeparator = "";
        public string mzCurrencyPrefix = "";
        public string mzCurrencyPostfix = "";
        public string mzThousandPostfix = "";
        public string mzMillionPostfix = "";
        public LanguageType meType { get { return (LanguageType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readString(node, "zFieldName", ref mzFieldName);
            infos.readString(node, "zISO630-1", ref mzISO6301Name);
            infos.readInt(node, "iExportColumn", ref miExportColumn);
            infos.readBool(node, "bPublicBuild", ref mbPublicBuild);
            infos.readBool(node, "bTranslation", ref mbTranslation);
            infos.readString(node, "zSingularExpression", ref mzSingularExpression);
            infos.readString(node, "zPluralExpression", ref mzPluralExpression);
            infos.readString(node, "zThousandsSeparator", ref mzThousandsSeparator);
            infos.readString(node, "zDecimalSeparator", ref mzDecimalSeparator);
            infos.readString(node, "zCurrencyPrefix", ref mzCurrencyPrefix);
            infos.readString(node, "zCurrencyPostfix", ref mzCurrencyPostfix);
            infos.readString(node, "zThousandPostfix", ref mzThousandPostfix);
            infos.readString(node, "zMillionPostfix", ref mzMillionPostfix);
        }
    }

    public class InfoLatitude : InfoBase
    {
        public TextType meName = TextType.NONE;
        public int miBase = 0;
        public int miRange = 0;
        public int miDieRoll = 0;
        public int miIceModifier = 0;
        public List<TextType> maeLocationText = new List<TextType>();
        public LatitudeType meType { get { return (LatitudeType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readInt(node, "iBase", ref miBase);
            infos.readInt(node, "iRange", ref miRange);
            infos.readInt(node, "iDieRoll", ref miDieRoll);
            infos.readInt(node, "iIceModifier", ref miIceModifier);
            infos.readTypesByType(maeLocationText, infos.locations().Count, node, "LocationText");
        }
    }

    public class InfoLevel : InfoBase
    {
        public TextType meName = TextType.NONE;
        public string mzMap = "";
        public string mzMapAlt = "";
        public int miLatitude = 0;
        public int miLongitude = 0;
        public int miDLC = 0;
        public int miDLCAlt = 0;
        public LocationType meLocation = LocationType.NONE;
        public MapSizeType meMapSize = MapSizeType.NONE;
        public TerrainClassType meTerrainClass = TerrainClassType.NONE;
        public LatitudeType meLatitude = LatitudeType.NONE;
        public List<bool> mabAdjacentLevel = new List<bool>();
        public List<bool> mabModeInvalid = new List<bool>();
        public LevelType meType { get { return (LevelType)miType; } } 

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readString(node, "zMap", ref mzMap);
            infos.readString(node, "zMapAlt", ref mzMapAlt);
            infos.readInt(node, "iLatitude", ref miLatitude);
            infos.readInt(node, "iLongitude", ref miLongitude);
            infos.readInt(node, "iDLC", ref miDLC);
            infos.readInt(node, "iDLCAlt", ref miDLCAlt);
            infos.readType<LocationType>(node, "Location", ref meLocation);
            infos.readType<MapSizeType>(node, "MapSize", ref meMapSize);
            infos.readType<TerrainClassType>(node, "TerrainClass", ref meTerrainClass);
            infos.readType<LatitudeType>(node, "Latitude", ref meLatitude);
            infos.readBoolsByType(mabAdjacentLevel, infos.levels().Count, node, "AdjacentLevel");
            infos.readBoolsByType(mabModeInvalid, infos.campaignModes().Count, node, "ModeInvalid");
        }
    }

    public class InfoLightingEnvironment : InfoBase
    {
        public AssetType meAsset = AssetType.NONE;
        public LightingEnvironmentType meType { get { return (LightingEnvironmentType)miType; } }
        public List<bool> mabLocationValid = new List<bool>();

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<AssetType>(node, "Asset", ref meAsset);
            infos.readBoolsByType(mabLocationValid, infos.locations().Count, node, "LocationValid");
        }
    }

    public class InfoLocation : InfoBase
    {
        public TextType meName = TextType.NONE;
        public TextType meHelptext = TextType.NONE;
        public LocationType meType { get { return (LocationType)miType; } }
        public AssetType meTerrainCollectorAsset = AssetType.NONE;
        public AssetType meTerrainPatternA = AssetType.NONE;
        public AssetType meTerrainPatternB = AssetType.NONE;
        public AssetType meTerrainColorRamp = AssetType.NONE;
        public List<AssetType> maeTerrainTextures = new List<AssetType>();
        public List<AssetType> maeTerrainTexturesSecondary = new List<AssetType>();
        public Color lakeColorPrimary = Color.clear;
        public Color lakeColorAdd = Color.clear;
        public int miLakeEmissive = -1;
        public int miLakeIntensity = -1;
        public int miClaims = 0;
        public int miLastDay = 0;
        public int miMarketDemandBase = 0;
        public int miLifeSupportModifier = 0;
        public int miInterestRateModifier = 0;
        public int miStartingHour = 0;
        public int miLightingStartHour = 0;
        public int miHoursPerDay = 0;
        public int miMinutesPerHour = 0;
        public int miLastHourMinutes = 0;
        public int miAuctionHour = 0;
        public int miAuctionMinDay = 0;
        public int miAuctionDayDiv = 0;
        public int miEclipseLength = 0;
        public int miSunriseHour = 0;
        public int miSunsetHour = 0;
        public int miLaunchCost = 0;
        public bool mbNoCraters = false;
        public bool mbNoGeothermal = false;
        public bool mbNoWind = false;
        public ResourceType meLaunchResource = ResourceType.NONE;
        public CampaignModeType meDefaultCampaignMode = CampaignModeType.NONE;
        public ModuleType meStartModule = ModuleType.NONE;
        public ModuleType meExtraHousingModule = ModuleType.NONE;
        public ModuleType meExtraLaborModule = ModuleType.NONE;
        public List<CampaignModeType> maeExecutiveDefaultCampaignMode = new List<CampaignModeType>();
        public int miExtraStartingModules = 0;
        public int miSecondInterestHour = -1;
        public int miSecondAuctionHour = -1;
        public int miSecondAuctionMinDay = -1;
        public string mzWonSkirmishAchievement = "";
        public string mzWonDailyChallengeAchievement = "";
        public string mzWonInfiniteChallengeAchievement = "";
        public string mzWonQuickMatchAchievement = "";
        public string mzWonCustomLobbyAchievement = "";

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readType<TextType>(node, "Helptext", ref meHelptext);
            infos.readType<AssetType>(node, "TerrainCollectorAsset", ref meTerrainCollectorAsset);
            infos.readType<AssetType>(node, "TerrainPatternA", ref meTerrainPatternA);
            infos.readType<AssetType>(node, "TerrainPatternB", ref meTerrainPatternB);
            infos.readType<AssetType>(node, "TerrainColorRamp", ref meTerrainColorRamp);
            infos.readTypesByType<AssetType>(maeTerrainTextures, infos.terrainMaterials().Count, node, "TerrainTextures");
            infos.readTypesByType<AssetType>(maeTerrainTexturesSecondary, infos.terrainMaterials().Count, node, "TerrainTexturesSecondary");
            infos.readHexColor(node, "LakeColorPrimary", ref lakeColorPrimary);
            infos.readHexColor(node, "LakeColorAdd", ref lakeColorAdd);
            infos.readInt(node, "iLakeEmissive", ref miLakeEmissive);
            infos.readInt(node, "iLakeIntensity", ref miLakeIntensity);
            infos.readInt(node, "iClaims", ref miClaims);
            infos.readInt(node, "iLastDay", ref miLastDay);
            infos.readInt(node, "iMarketDemandBase", ref miMarketDemandBase);
            infos.readInt(node, "iLifeSupportModifier", ref miLifeSupportModifier);
            infos.readInt(node, "iInterestRateModifier", ref miInterestRateModifier);
            infos.readInt(node, "iStartingHour", ref miStartingHour);
            infos.readInt(node, "iLightingStartHour", ref miLightingStartHour);
            infos.readInt(node, "iHoursPerDay", ref miHoursPerDay);
            infos.readInt(node, "iMinutesPerHour", ref miMinutesPerHour);
            infos.readInt(node, "iLastHourMinutes", ref miLastHourMinutes);
            infos.readInt(node, "iAuctionHour", ref miAuctionHour);
            infos.readInt(node, "iAuctionMinDay", ref miAuctionMinDay);
            infos.readInt(node, "iAuctionDayDiv", ref miAuctionDayDiv);
            infos.readInt(node, "iEclipseLength", ref miEclipseLength);
            infos.readInt(node, "iSunriseHour", ref miSunriseHour);
            infos.readInt(node, "iSunsetHour", ref miSunsetHour);
            infos.readInt(node, "iLaunchCost", ref miLaunchCost);
            infos.readBool(node, "bNoCraters", ref mbNoCraters);
            infos.readBool(node, "bNoGeothermal", ref mbNoGeothermal);
            infos.readBool(node, "bNoWind", ref mbNoWind);
            infos.readType<ResourceType>(node, "LaunchResource", ref meLaunchResource);
            infos.readType<CampaignModeType>(node, "DefaultCampaignMode", ref meDefaultCampaignMode);
            infos.readType<ModuleType>(node, "StartModule", ref meStartModule);
            infos.readType<ModuleType>(node, "ExtraHousingModule", ref meExtraHousingModule);
            infos.readType<ModuleType>(node, "ExtraLaborModule", ref meExtraLaborModule);
            infos.readTypesByType(maeExecutiveDefaultCampaignMode, infos.executives().Count, node, "ExecutiveDefaultCampaignMode");
            infos.readInt(node, "iExtraStartingModules", ref miExtraStartingModules);
            infos.readInt(node, "iSecondInterestHour", ref miSecondInterestHour);
            infos.readInt(node, "iSecondAuctionHour", ref miSecondAuctionHour);
            infos.readInt(node, "iSecondAuctionMinDay", ref miSecondAuctionMinDay);
            infos.readString(node, "zWonSkirmishAchievement", ref mzWonSkirmishAchievement);
            infos.readString(node, "zWonDailyChallengeAchievement", ref mzWonDailyChallengeAchievement);
            infos.readString(node, "zWonInfiniteChallengeAchievement", ref mzWonInfiniteChallengeAchievement);
            infos.readString(node, "zWonQuickMatchAchievement", ref mzWonQuickMatchAchievement);
            infos.readString(node, "zWonCustomLobbyAchievement", ref mzWonCustomLobbyAchievement);
        }
    }

    public class InfoMapName : InfoBase
    {
        public TextType meName = TextType.NONE;
        public string mzAsset = "";
        public LocationType meLocation = LocationType.NONE;
        public MapNameType meType { get { return (MapNameType)miType; } } 

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readString(node, "zAsset", ref mzAsset);
            infos.readType<LocationType>(node, "Location", ref meLocation);
        }
    }

    public class InfoMapSize : InfoBase
    {
        public TextType meName = TextType.NONE;
        public TextType meDesc = TextType.NONE;
        public AssetType meScanAsset = AssetType.NONE;
        public int miWidth = 0;
        public int miHeight = 0;
        public int miCoefficient = 0;
        public int miScanChange = 0;
        public int miScanTime = 0;
        public int miFoundMoney = 0;
        public MapSizeType meType { get { return (MapSizeType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readType<TextType>(node, "Desc", ref meDesc);
            infos.readType<AssetType>(node, "ScanAsset", ref meScanAsset);
            infos.readInt(node, "iWidth", ref miWidth);
            infos.readInt(node, "iHeight", ref miHeight);
            infos.readInt(node, "iCoefficient", ref miCoefficient);
            infos.readInt(node, "iScanChange", ref miScanChange);
            infos.readInt(node, "iScanTime", ref miScanTime);
            infos.readInt(node, "iFoundMoney", ref miFoundMoney);
        }
    }

    public class InfoMarkup : InfoBase
    {
        public string mzTag = "";
        public string mzOpening = "";
        public string mzClosing = "";
        public MarkupType meType { get { return (MarkupType)miType; } } 

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readString(node, "zTag", ref mzTag);
            infos.readString(node, "zOpening", ref mzOpening);
            infos.readString(node, "zClosing", ref mzClosing);
        }
    }

    public class InfoModule : InfoBase
    {
        public TextType meName = TextType.NONE;
        public TextType meEffectOnColony = TextType.NONE;
        public string mzIconName = "";
        public int miEntertainmentDemand = 0;
        public int miEntertainmentModifier = 0;
        public int miRandomRotation = 0;
        public bool mbPopulation = false;
        public bool mbLabor = false;
        public bool mbSpread = false;
        public bool mbShuttle = false;
        public bool mbReplacesTerrain = false;
        public BuildingType meBuildingSprite = BuildingType.NONE;
        public IceType meIceRequired = IceType.NONE;
        public TerrainType meTerrainRequired = TerrainType.NONE;
        public List<AssetType> maeAssets = new List<AssetType>();
        public List<int> maiResourceCost = new List<int>();
        public List<int> maiResourceSupply = new List<int>();
        public List<int> maiOrderModifier = new List<int>();
        public List<int> maiAppearanceProb = new List<int>();
        public List<bool> mabLocationValid = new List<bool>();
        public List<bool> mabColonyValid = new List<bool>();
        public List<bool> mabFootprint = new List<bool>();
        public List<List<bool>> maabColonyLocationValid = new List<List<bool>>();
        public ModuleType meType { get { return (ModuleType)miType; } }
        public bool mbRequiresAdjacentColony = false;

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readType<TextType>(node, "EffectOnColony", ref meEffectOnColony);
            infos.readString(node, "zIconName", ref mzIconName);
            infos.readInt(node, "iEntertainmentDemand", ref miEntertainmentDemand);
            infos.readInt(node, "iEntertainmentModifier", ref miEntertainmentModifier);
            infos.readInt(node, "iRandomRotation", ref miRandomRotation);
            infos.readBool(node, "bPopulation", ref mbPopulation);
            infos.readBool(node, "bLabor", ref mbLabor);
            infos.readBool(node, "bSpread", ref mbSpread);
            infos.readBool(node, "bShuttle", ref mbShuttle);
            infos.readBool(node, "bReplacesTerrain", ref mbReplacesTerrain);
            infos.readType<BuildingType>(node, "BuildingSprite", ref meBuildingSprite);
            infos.readType<IceType>(node, "IceRequired", ref meIceRequired);
            infos.readType<TerrainType>(node, "TerrainRequired", ref meTerrainRequired);
            infos.readTypeStrings(out maeAssets, node, "Assets");
            infos.readIntsByType(maiResourceCost, infos.resources().Count, node, "ResourceCost");
            infos.readIntsByType(maiResourceSupply, infos.resources().Count, node, "ResourceSupply");
            infos.readIntsByType(maiOrderModifier, infos.orders().Count, node, "OrderModifier");
            infos.readIntsByType(maiAppearanceProb, infos.locations().Count, node, "AppearanceProb");
            infos.readBoolsByType(mabLocationValid, infos.locations().Count, node, "LocationValid");
            infos.readBoolsByType(mabColonyValid, infos.colonies().Count, node, "ColonyValid");
            infos.readBoolsByType(mabFootprint, infos.directions().Count, node, "Footprint");
            infos.readBoolListByType(maabColonyLocationValid, infos.colonies().Count, infos.locations().Count, node, "ColonyLocationValid");
            infos.readBool(node, "bRequiresAdjacentColony", ref mbRequiresAdjacentColony);
        }
    }

    public class InfoOrder : InfoBase
    {
        public TextType meName = TextType.NONE;
        public AudioTypeT meEnqueSound = AudioTypeT.NONE;
        public AudioTypeT meDequeSound = AudioTypeT.NONE;
        public OrderType meType { get { return (OrderType)miType; } } 

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readType<AudioTypeT>(node, "EnqueSound", ref meEnqueSound);
            infos.readType<AudioTypeT>(node, "DequeSound", ref meDequeSound);
        }
    }

    public class InfoOrdinal : InfoBase
    {
        public TextType meName = TextType.NONE;
        public int miDivisor = 0;
        public int miRemainder = 0;
        public OrdinalType meType { get { return (OrdinalType)miType; } } 

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readInt(node, "iDivisor", ref miDivisor);
            infos.readInt(node, "iRemainder", ref miRemainder);
        }
    }

    public class InfoPatent : InfoBase
    {
        public TextType meName = TextType.NONE;
        public string mzIconName = "";
        public int miTime = 0;
        public int miDebtCut = 0;
        public int miEntertainmentModifier = 0;
        public int miPowerConsumptionModifier = 0;
        public int miConnectedHQPowerProductionModifier = 0;
        public int miAdjacentHQSabotageModifier = 0;
        public bool mbBorehole = false;
        public bool mbRecycleScrap = false;
        public bool mbAdjacentMining = false;
        public bool mbTeleportation = false;
        public AudioTypeT meAudio = AudioTypeT.NONE;
        public ResourceType meBuildingFreeResource = ResourceType.NONE;
        public ResourceType meAlternateGasResource = ResourceType.NONE;
        public ResourceType meAlternatePowerResource = ResourceType.NONE;
        public List<int> maiResourceCost = new List<int>();
        public List<int> maiResourceCapacity = new List<int>();
        public List<bool> mabBuildingAlwaysOn = new List<bool>();
        public List<bool> mabBuildingClassBoost = new List<bool>();
        public List<bool> mabLocationInvalid = new List<bool>();
        public List<ResourceType> maeResourceReplace = new List<ResourceType>();
        public List<IceType> maeIgnoreInputIce = new List<IceType>();
        public PatentType meType { get { return (PatentType)miType; } }
        public bool mbCanSplitResearch = false;
        public bool mbNoAuction = false;
        public bool mbCaveMining = false;

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readString(node, "zIconName", ref mzIconName);
            infos.readInt(node, "iTime", ref miTime);
            infos.readInt(node, "iDebtCut", ref miDebtCut);
            infos.readInt(node, "iEntertainmentModifier", ref miEntertainmentModifier);
            infos.readInt(node, "iPowerConsumptionModifier", ref miPowerConsumptionModifier);
            infos.readInt(node, "iConnectedHQPowerProductionModifier", ref miConnectedHQPowerProductionModifier);
            infos.readInt(node, "iAdjacentHQSabotageModifier", ref miAdjacentHQSabotageModifier);
            infos.readBool(node, "bBorehole", ref mbBorehole);
            infos.readBool(node, "bRecycleScrap", ref mbRecycleScrap);
            infos.readBool(node, "bAdjacentMining", ref mbAdjacentMining);
            infos.readBool(node, "bTeleportation", ref mbTeleportation);
            infos.readType<AudioTypeT>(node, "Audio", ref meAudio);
            infos.readType<ResourceType>(node, "BuildingFreeResource", ref meBuildingFreeResource);
            infos.readType<ResourceType>(node, "AlternateGasResource", ref meAlternateGasResource);
            infos.readType<ResourceType>(node, "AlternatePowerResource", ref meAlternatePowerResource);
            infos.readIntsByType(maiResourceCost, infos.resources().Count, node, "ResourceCost");
            infos.readIntsByType(maiResourceCapacity, infos.resources().Count, node, "ResourceCapacity");
            infos.readBoolsByType(mabBuildingAlwaysOn, infos.buildings().Count, node, "BuildingAlwaysOn");
            infos.readBoolsByType(mabBuildingClassBoost, infos.buildingClasses().Count, node, "BuildingClassBoost");
            infos.readBoolsByType(mabLocationInvalid, infos.locations().Count, node, "LocationInvalid");
            infos.readTypesByType(maeResourceReplace, infos.resources().Count, node, "ResourceReplace");
            infos.readTypesByType(maeIgnoreInputIce, infos.buildingClasses().Count, node, "IgnoreInputIce");
            infos.readBool(node, "bCanSplitResearch", ref mbCanSplitResearch);
            infos.readBool(node, "bNoAuction", ref mbNoAuction);
            infos.readBool(node, "bCaveMining", ref mbCaveMining);
        }
    }

    public class InfoPerk : InfoBase
    {
        public TextType meName = TextType.NONE;
        public string mzIconName = "";
        public int miAvailableRoll = 0;
        public int miAvailableTurn = 0;
        public int miLevelRoll = 0;
        public int miLevelTurn = 0;
        public int miLevelTime = 0;
        public int miCost = 0;
        public int miClaims = 0;
        public int miSabotages = 0;
        public int miInterestModifier = 0;
        public bool mbBuilding = false;
        public bool mbMultiples = false;
        public bool mbAuction = false;
        public bool mbAvailableUnique = false;
        public bool mbOnFound = false;
        public bool mbOnUpgrade = false;
        public bool mbOnLevel = false;
        public bool mbUnlockHQAll = false;
        public bool mbIoDLC = false;
        public HQType meHQUnlock = HQType.NONE;
        public HQLevelType meOnHQLevel = HQLevelType.NONE;
        public BuildingType meFreeBuilding = BuildingType.NONE;
        public BuildingType meBuildingImmune = BuildingType.NONE;
        public BuildingClassType meBuildingClassLevel = BuildingClassType.NONE;
        public PatentType mePatent = PatentType.NONE;
        public ResourceType meAlternatePowerResource = ResourceType.NONE;
        public List<int> maiResourceStockpile = new List<int>();
        public List<int> maiResourceProductionModifier = new List<int>();
        public List<int> maiBuildingClassInputModifier = new List<int>();
        public List<int> maiOrderCostModifier = new List<int>();
        public List<int> maiOrderRateModifier = new List<int>();
        public List<int> maiSabotageCount = new List<int>();
        public List<int> maiPerkCostModifier = new List<int>();
        public List<bool> mabPerkSkip = new List<bool>();
        public List<bool> mabLocationInvalid = new List<bool>();
        public List<ResourceLevelType> maeMinimumMining = new List<ResourceLevelType>();
        public PerkType meType { get { return (PerkType)miType; } } 

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readString(node, "zIconName", ref mzIconName);
            infos.readInt(node, "iAvailableRoll", ref miAvailableRoll);
            infos.readInt(node, "iAvailableTurn", ref miAvailableTurn);
            infos.readInt(node, "iLevelRoll", ref miLevelRoll);
            infos.readInt(node, "iLevelTurn", ref miLevelTurn);
            infos.readInt(node, "iLevelTime", ref miLevelTime);
            infos.readInt(node, "iCost", ref miCost);
            infos.readInt(node, "iClaims", ref miClaims);
            infos.readInt(node, "iSabotages", ref miSabotages);
            infos.readInt(node, "iInterestModifier", ref miInterestModifier);
            infos.readBool(node, "bBuilding", ref mbBuilding);
            infos.readBool(node, "bMultiples", ref mbMultiples);
            infos.readBool(node, "bAuction", ref mbAuction);
            infos.readBool(node, "bAvailableUnique", ref mbAvailableUnique);
            infos.readBool(node, "bOnFound", ref mbOnFound);
            infos.readBool(node, "bOnUpgrade", ref mbOnUpgrade);
            infos.readBool(node, "bOnLevel", ref mbOnLevel);
            infos.readBool(node, "bUnlockHQAll", ref mbUnlockHQAll);
            infos.readBool(node, "bIoDLC", ref mbIoDLC);
            infos.readType<HQType>(node, "HQUnlock", ref meHQUnlock);
            infos.readType<HQLevelType>(node, "OnHQLevel", ref meOnHQLevel);
            infos.readType<BuildingType>(node, "FreeBuilding", ref meFreeBuilding);
            infos.readType<BuildingType>(node, "BuildingImmune", ref meBuildingImmune);
            infos.readType<BuildingClassType>(node, "BuildingClassLevel", ref meBuildingClassLevel);
            infos.readType<PatentType>(node, "Patent", ref mePatent);
            infos.readType<ResourceType>(node, "AlternatePowerResource", ref meAlternatePowerResource);
            infos.readIntsByType(maiResourceStockpile, infos.resources().Count, node, "ResourceStockpile");
            infos.readIntsByType(maiResourceProductionModifier, infos.resources().Count, node, "ResourceProductionModifier");
            infos.readIntsByType(maiBuildingClassInputModifier, infos.buildingClasses().Count, node, "BuildingClassInputModifier");
            infos.readIntsByType(maiOrderCostModifier, infos.orders().Count, node, "OrderCostModifier");
            infos.readIntsByType(maiOrderRateModifier, infos.orders().Count, node, "OrderRateModifier");
            infos.readIntsByType(maiSabotageCount, infos.sabotages().Count, node, "SabotageCount");
            infos.readIntsByType(maiPerkCostModifier, infos.perks().Count, node, "PerkCostModifier");
            infos.readBoolsByType(mabPerkSkip, infos.perks().Count, node, "PerkSkip");
            infos.readBoolsByType(mabLocationInvalid, infos.locations().Count, node, "LocationInvalid");
            infos.readTypesByType(maeMinimumMining, infos.buildingClasses().Count, node, "MinimumMining");
        }
    }

    public class InfoPersonality : InfoBase
    {
        public CharacterType meCharacter = CharacterType.NONE;
        public int miFoundValueModifier = 0;
        public int miUpgradeRoll = 0;
        public int miBlackMarketRoll = 0;
        public int miSabotageRoll = 0;
        public int miBestConstructRoll = 0;
        public int miEntertainmentConstructRoll = 0;
        public int miClaimValueModifier = 0;
        public int miSabotageValueModifier = 0;
        public int miAuctionValueModifier = 0;
        public int miMinEntertainmentValue = 0;
        public int miStockThreshold = 0;
        public int miFoundMoneyDivisor = 0;
        public int miReplaceNormalValue = 0;
        public int miReplaceShippingValue = 0;
        public int miReplaceForOrderValue = 0;
        public int miConstructBaseValue = 0;
        public int miConstructUpgradeValue = 0;
        public int miConstructOrderValue = 0;
        public int miConstructMissingValue = 0;
        public int miConstructPowerThreshold = 0;
        public int miConstructRequiredThreshold = 0;
        public int miConstructBestDiff = 0;
        public int miQuipDelay = 0;
        public bool mbIoDLC = false;
        public List<int> maiPerkValueModifier = new List<int>();
        public List<int> maiTerrainClassValue = new List<int>();
        public List<int> maiLatitudeValue = new List<int>();
        public List<int> maiLocationValue = new List<int>();
        public List<int> maiHQModifier = new List<int>();
        public List<int> maiPatentValueModifier = new List<int>();
        public List<int> maiOrderRoll = new List<int>();
        public List<int> maiOrderConstructRoll = new List<int>();
        public PersonalityType meType { get { return (PersonalityType)miType; } } 

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<CharacterType>(node, "Character", ref meCharacter);
            infos.readInt(node, "iFoundValueModifier", ref miFoundValueModifier);
            infos.readInt(node, "iUpgradeRoll", ref miUpgradeRoll);
            infos.readInt(node, "iBlackMarketRoll", ref miBlackMarketRoll);
            infos.readInt(node, "iSabotageRoll", ref miSabotageRoll);
            infos.readInt(node, "iBestConstructRoll", ref miBestConstructRoll);
            infos.readInt(node, "iEntertainmentConstructRoll", ref miEntertainmentConstructRoll);
            infos.readInt(node, "iClaimValueModifier", ref miClaimValueModifier);
            infos.readInt(node, "iSabotageValueModifier", ref miSabotageValueModifier);
            infos.readInt(node, "iAuctionValueModifier", ref miAuctionValueModifier);
            infos.readInt(node, "iMinEntertainmentValue", ref miMinEntertainmentValue);
            infos.readInt(node, "iStockThreshold", ref miStockThreshold);
            infos.readInt(node, "iFoundMoneyDivisor", ref miFoundMoneyDivisor);
            infos.readInt(node, "iReplaceNormalValue", ref miReplaceNormalValue);
            infos.readInt(node, "iReplaceShippingValue", ref miReplaceShippingValue);
            infos.readInt(node, "iReplaceForOrderValue", ref miReplaceForOrderValue);
            infos.readInt(node, "iConstructBaseValue", ref miConstructBaseValue);
            infos.readInt(node, "iConstructUpgradeValue", ref miConstructUpgradeValue);
            infos.readInt(node, "iConstructOrderValue", ref miConstructOrderValue);
            infos.readInt(node, "iConstructMissingValue", ref miConstructMissingValue);
            infos.readInt(node, "iConstructPowerThreshold", ref miConstructPowerThreshold);
            infos.readInt(node, "iConstructRequiredThreshold", ref miConstructRequiredThreshold);
            infos.readInt(node, "iConstructBestDiff", ref miConstructBestDiff);
            infos.readInt(node, "iQuipDelay", ref miQuipDelay);
            infos.readBool(node, "bIoDLC", ref mbIoDLC);
            infos.readIntsByType(maiTerrainClassValue, infos.terrainClasses().Count, node, "TerrainClassValue");
            infos.readIntsByType(maiLatitudeValue, infos.latitudes().Count, node, "LatitudeValue");
            infos.readIntsByType(maiLocationValue, infos.locations().Count, node, "LocationValue");
            infos.readIntsByType(maiPerkValueModifier, infos.perks().Count, node, "PerkValueModifier");
            infos.readIntsByType(maiHQModifier, infos.HQs().Count, node, "HQModifier");
            infos.readIntsByType(maiPatentValueModifier, infos.patents().Count, node, "PatentValueModifier");
            infos.readIntsByType(maiOrderRoll, infos.orders().Count, node, "OrderRoll");
            infos.readIntsByType(maiOrderConstructRoll, infos.orders().Count, node, "OrderConstructRoll");
        }
    }

    public class InfoPlayerColor : InfoBase
    {
        public Color textColor = Color.white;
        public Color tileColor = Color.white;
        public Color iconColor = Color.white;
        public int miPriority = 0;
        public PlayerColorType meType { get { return (PlayerColorType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readHexColor(node, "TextColor", ref textColor);
            infos.readHexColor(node, "TileColor", ref tileColor);
            infos.readHexColor(node, "IconColor", ref iconColor);
            infos.readInt(node, "iPriority", ref miPriority);
        }
    }

    public class InfoPlayerOption : InfoBase
    {
        public TextType meName = TextType.NONE;
        public TextType meDescription = TextType.NONE;
        public PlayerOptionType meType { get { return (PlayerOptionType)miType; } } 

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readType<TextType>(node, "Desc", ref meDescription);
        }
    }

    public class InfoResource : InfoBase
    {
        public TextType meName = TextType.NONE;
        public TextType meMeasurementUnit = TextType.NONE;
        public ResourceColorType meResourceColor = ResourceColorType.NONE;
        public List<AssetType> maeLevelAssets = new List<AssetType>();
        public AssetType mePathAsset = AssetType.NONE;
        public string mzIconName = "";
        public string mzInlineIcon = "";
        public string mzInlineLevelIcon = "";
        public string mzNewUIInlineIcon = "";
        public List<string> mazResourceLevelIconNames = new List<string>();
        public int miMinRate = 0;
        public int miShipment = 0;
        public int miMarketPrice = 0;
        public int miOffworldPrice = 0;
        public int miOffworldRand = 0;
        public int miOffworldDemandProb = 0;
        public int miHQBonus = 0;
        public int miLaunchQuantity = 0;
        public int miLaunchCost = 0;
        public bool mbTrade = false;
        public AudioTypeT meBuyAudio = AudioTypeT.NONE;
        public List<EventAudioType> maeAudioDiscovered = new List<EventAudioType>();
        public List<int> maiLevelRoll = new List<int>();
        public List<int> maiHQMinRate = new List<int>();
        public List<int> maiAvoidRange = new List<int>();
        public List<int> maiLocationDiminishThreshold = new List<int>();
        public List<int> maiLocationAppearanceProb = new List<int>();
        public List<bool> mabLocationInvalid = new List<bool>();
        public List<bool> mabLocationNoDiminish = new List<bool>();
        public List<bool> mabNoSpread = new List<bool>();
        public List<List<int>> maaiLocationLevelRollModifier = new List<List<int>>();
        public ResourceType meType { get { return (ResourceType)miType; } }
        public int miImportValue = -1;
        public int miExportValue = -1;
        public int miImportCooldown = -1;
        public bool mbHidden = false;

        //references
        public InfoResourceColor mResourceColor = null;

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readType<TextType>(node, "MeasurementUnit", ref meMeasurementUnit);
            infos.readType<ResourceColorType>(node, "ResourceColor", ref meResourceColor);
            infos.readTypesByType(maeLevelAssets, infos.resourceLevels().Count, node, "LevelAssets");
            infos.readType<AssetType>(node, "PathAsset", ref mePathAsset);
            infos.readString(node, "zIconName", ref mzIconName);
            infos.readString(node, "zUIInlineIcon", ref mzInlineIcon);
            infos.readString(node, "zUIInlineLevelIcon", ref mzInlineLevelIcon);
            infos.readString(node, "zNewUIInlineIcon", ref mzNewUIInlineIcon);
            infos.readStringsByType(mazResourceLevelIconNames, infos.resourceLevels().Count, node, "ResourceLevelIconNames");
            infos.readInt(node, "iMinRate", ref miMinRate);
            infos.readInt(node, "iShipment", ref miShipment);
            infos.readInt(node, "iMarketPrice", ref miMarketPrice);
            infos.readInt(node, "iOffworldPrice", ref miOffworldPrice);
            infos.readInt(node, "iOffworldRand", ref miOffworldRand);
            infos.readInt(node, "iOffworldDemandProb", ref miOffworldDemandProb);
            infos.readInt(node, "iHQBonus", ref miHQBonus);
            infos.readInt(node, "iLaunchQuantity", ref miLaunchQuantity);
            infos.readInt(node, "iLaunchCost", ref miLaunchCost);
            infos.readBool(node, "bTrade", ref mbTrade);
            infos.readType<AudioTypeT>(node, "BuyAudio", ref meBuyAudio);
            infos.readTypesByType(maeAudioDiscovered, infos.resourceLevels().Count, node, "AudioDiscovered");
            infos.readIntsByType(maiLevelRoll, infos.resourceLevels().Count, node, "LevelRoll");
            infos.readIntsByType(maiHQMinRate, infos.resources().Count, node, "HQMinRate");
            infos.readIntsByType(maiAvoidRange, infos.resources().Count, node, "AvoidRange");
            infos.readIntsByType(maiLocationDiminishThreshold, infos.locations().Count, node, "LocationDiminishThreshold");
            infos.readIntsByType(maiLocationAppearanceProb, infos.locations().Count, node, "LocationAppearanceProb");
            infos.readBoolsByType(mabLocationInvalid, infos.locations().Count, node, "LocationInvalid");
            infos.readBoolsByType(mabLocationNoDiminish, infos.locations().Count, node, "LocationNoDiminish");
            infos.readBoolsByType(mabNoSpread, infos.locations().Count, node, "LocationNoSpread");
            infos.readIntListByType(maaiLocationLevelRollModifier, infos.locations().Count, infos.resourceLevels().Count, node, "LocationLevelRollModifier");
            infos.readInt(node, "iImportValue", ref miImportValue);
            infos.readInt(node, "iExportValue", ref miExportValue);
            infos.readInt(node, "iImportCooldown", ref miImportCooldown);
            infos.readBoolsByType(mabNoSpread, infos.locations().Count, node, "LocationNoSpread");
            infos.readBool(node, "bHidden", ref mbHidden);
        }

        public override void UpdateReferences(Infos infos)
        {
            mResourceColor = infos.resourceColor(meResourceColor);
        }
    }

    public class InfoResourceColor : InfoBase
    {
        public Color worldTextColor = Color.white;
        public Color graphColor = Color.white;
        public Color textColor = Color.white;
        public Color iconColor = Color.white;
        public Color containerColor = Color.white;
        public Color worldLineColor = Color.white;
        public ResourceColorType meType { get { return (ResourceColorType)miType; } } 

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readHexColor(node, "WorldTextColor", ref worldTextColor);
            infos.readHexColor(node, "GraphColor", ref graphColor);
            infos.readHexColor(node, "TextColor", ref textColor);
            infos.readHexColor(node, "IconColor", ref iconColor);
            infos.readHexColor(node, "ContainerColor", ref containerColor);
            infos.readHexColor(node, "WorldLineColor", ref worldLineColor);
        }
    }

    public class InfoResourceLevel : InfoBase
    {
        public TextType meName = TextType.NONE;
        public AssetType meHighlightAsset = AssetType.NONE;
        public string mzAssetSuffix = "";
        public string mzIconSuffix = "";
        public int miBuildingTanks = 0;
        public int miRateMultiplier = 0;
        public int miAdjacentProb = 0;
        public bool mbUsable = false;
        public bool mbCanBomb = false;
        public List<int> maiSpreadRoll = new List<int>();
        public List<bool> mabLocationDiminish = new List<bool>();
        public List<bool> mabLocationInvalid = new List<bool>();
        public ResourceLevelType meType { get { return (ResourceLevelType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readType<AssetType>(node, "HighlightAsset", ref meHighlightAsset);
            infos.readString(node, "zAssetSuffix", ref mzAssetSuffix);
            infos.readString(node, "zIconSuffix", ref mzIconSuffix);
            infos.readInt(node, "iBuildingTanks", ref miBuildingTanks);
            infos.readInt(node, "iRateMultiplier", ref miRateMultiplier);
            infos.readInt(node, "iAdjacentProb", ref miAdjacentProb);
            infos.readBool(node, "bUsable", ref mbUsable);
            infos.readBool(node, "bCanBomb", ref mbCanBomb);
            infos.readIntsByType(maiSpreadRoll, infos.resourceLevels().Count, node, "SpreadRoll");
            infos.readBoolsByType(mabLocationDiminish, infos.locations().Count, node, "LocationDiminish");
            infos.readBoolsByType(mabLocationInvalid, infos.locations().Count, node, "LocationInvalid");
        }
    }

    public class InfoResourceMinimum : InfoBase
    {
        public TextType meName = TextType.NONE;
        public int miModifier = 0;
        public ResourceMinimumType meType { get { return (ResourceMinimumType)miType; } } 

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readInt(node, "iModifier", ref miModifier);
        }
    }

    public class InfoResourcePresence : InfoBase
    {
        public TextType meName = TextType.NONE;
        public int miModifier = 0;
        public ResourcePresenceType meType { get { return (ResourcePresenceType)miType; } } 

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readInt(node, "iModifier", ref miModifier);
        }
    }

    public class InfoRulesSet : InfoBase
    {
        public TextType meColonyName = TextType.NONE;
        public int miNumPlayers = 0;
        public int miSeed = 0;
        public int miClaims = 0;
        public int miPowerConsumptionModifier = 0;
        public int miGasConsumptionModifier = 0;
        public int miAIFoundMinLevel = 0;
        public int miAISabotageModifier = 0;
        public int miAIProductionModifier = 0;
        public bool mbRevealMapHuman = false;
        public bool mbNoAutoReveal = false;
        public bool mbAutoStart = false;
        public bool mbWinMaxUpgrade = false;
        public bool mbNoTime = false;
        public bool mbNoStockMarket = false;
        public bool mbNoAutoSupply = false;
        public bool mbNoAutoOff = false;
        public bool mbNoAutoBuy = false;
        public bool mbNoAutoSell = false;
        public bool mbNoBuySell = false;
        public bool mbNoColony = false;
        public bool mbNoClaiming = false;
        public bool mbNoCancelingOrders = false;
        public bool mbNoDiminishingResources = false;
        public bool mbDisableScrapping = false;
        public bool mbHumanBlackMarketIgnoreDebt = false;
        public bool mbAIFoundFavorite = false;
        public bool mbCustomColony = false;
        public HandicapType meHandicap = HandicapType.NONE;
        public ColonyClassType meColonyClass = ColonyClassType.NONE;
        public List<int> maiFreeResources = new List<int>();
        public List<int> maiFreeSabotages = new List<int>();
        public List<bool> mabGameOption = new List<bool>();
        public List<bool> mabHQInvalidHuman = new List<bool>();
        public List<bool> mabHQInvalidAI = new List<bool>();
        public List<bool> mabPersonalityValidAI = new List<bool>();
        public List<bool> mabBuildingUnavailable = new List<bool>();
        public List<bool> mabResourceNoBuySell = new List<bool>();
        public RulesSetType meType { get { return (RulesSetType)miType; } }
        public List<bool> mabimportUnavailablePlayer = new List<bool>();
        public List<bool> mabResourceNecessary = new List<bool>();
        public List<bool> mabValidModules = new List<bool>();
        public List<bool> mabInvalidModulesHuman = new List<bool>();
        public List<bool> mabInvalidModulesAI = new List<bool>();
        public bool mbBlackMarketAvailable = true;
        public List<bool> mabRequiredBlackMarket = new List<bool>();

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "ColonyName", ref meColonyName);
            infos.readInt(node, "iNumPlayers", ref miNumPlayers);
            infos.readInt(node, "iSeed", ref miSeed);
            infos.readInt(node, "iClaims", ref miClaims);
            infos.readInt(node, "iPowerConsumptionModifier", ref miPowerConsumptionModifier);
            infos.readInt(node, "iGasConsumptionModifier", ref miGasConsumptionModifier);
            infos.readInt(node, "iAIFoundMinLevel", ref miAIFoundMinLevel);
            infos.readInt(node, "iAISabotageModifier", ref miAISabotageModifier);
            infos.readInt(node, "iAIProductionModifier", ref miAIProductionModifier);
            infos.readBool(node, "bRevealMapHuman", ref mbRevealMapHuman);
            infos.readBool(node, "bNoAutoReveal", ref mbNoAutoReveal);
            infos.readBool(node, "bAutoStart", ref mbAutoStart);
            infos.readBool(node, "bWinMaxUpgrade", ref mbWinMaxUpgrade);
            infos.readBool(node, "bNoTime", ref mbNoTime);
            infos.readBool(node, "bNoStockMarket", ref mbNoStockMarket);
            infos.readBool(node, "bNoAutoSupply", ref mbNoAutoSupply);
            infos.readBool(node, "bNoAutoOff", ref mbNoAutoOff);
            infos.readBool(node, "bNoAutoBuy", ref mbNoAutoBuy);
            infos.readBool(node, "bNoAutoSell", ref mbNoAutoSell);
            infos.readBool(node, "bNoBuySell", ref mbNoBuySell);
            infos.readBool(node, "bNoColony", ref mbNoColony);
            infos.readBool(node, "bNoClaiming", ref mbNoClaiming);
            infos.readBool(node, "bNoCancelingOrders", ref mbNoCancelingOrders);
            infos.readBool(node, "bNoDiminishingResources", ref mbNoDiminishingResources);
            infos.readBool(node, "bDisableScrapping", ref mbDisableScrapping);
            infos.readBool(node, "bHumanBlackMarketIgnoreDebt", ref mbHumanBlackMarketIgnoreDebt);
            infos.readBool(node, "bAIFoundFavorite", ref mbAIFoundFavorite);
            infos.readBool(node, "bCustomColony", ref mbCustomColony);
            infos.readType<HandicapType>(node, "Handicap", ref meHandicap);
            infos.readType<ColonyClassType>(node, "ColonyClass", ref meColonyClass);
            infos.readIntsByType(maiFreeResources, infos.resources().Count, node, "FreeResources");
            infos.readIntsByType(maiFreeSabotages, infos.sabotages().Count, node, "FreeSabotages");
            infos.readBoolsByType(mabGameOption, infos.gameOptions().Count, node, "GameOption");
            infos.readBoolsByType(mabHQInvalidHuman, infos.HQs().Count, node, "HQInvalidHuman");
            infos.readBoolsByType(mabHQInvalidAI, infos.HQs().Count, node, "HQInvalidAI");
            infos.readBoolsByType(mabPersonalityValidAI, infos.personalities().Count, node, "PersonalityValidAI");
            infos.readBoolsByType(mabBuildingUnavailable, infos.buildings().Count, node, "BuildingUnavailable");
            infos.readBoolsByType(mabimportUnavailablePlayer, infos.resources().Count, node, "ImportUnavailablePlayer");
            infos.readBoolsByType(mabResourceNecessary, infos.resources().Count, node, "ResourcesNecessary");
            infos.readBoolsByType(mabValidModules, infos.modules().Count, node, "ValidModules");
            infos.readBoolsByType(mabInvalidModulesHuman, infos.modules().Count, node, "InvalidModulesHuman");
            infos.readBoolsByType(mabInvalidModulesAI, infos.modules().Count, node, "InvalidModulesAI");
            infos.readBool(node, "bBlackMarketAvailable", ref mbBlackMarketAvailable);
            infos.readBoolsByType(mabRequiredBlackMarket, infos.blackMarkets().Count, node, "RequiredBlackMarket");
        }
    }

    public class InfoSabotage : InfoBase
    {
        public TextType meName = TextType.NONE;
        public AssetType meSabotageAsset = AssetType.NONE;
        public AssetType meTileAsset = AssetType.NONE;
        public AssetType meChainAsset = AssetType.NONE;
        public AssetType meUnitAsset = AssetType.NONE;
        public AssetType meReversalAsset = AssetType.NONE;
        public TextType meHelptext = TextType.NONE;
        public TextType meAppearanceText = TextType.NONE;
        public TextType meDefenderAlert = TextType.NONE;
        public TextType meDefenderNotificationTitle = TextType.NONE;
        public TextType meDefenderNotificationDesc = TextType.NONE;
        public EventAudioType meAuctionEventAudio = EventAudioType.NONE;
        public AudioTypeT meAttackerAudio = AudioTypeT.NONE;
        public AudioTypeT meDefenderAudio = AudioTypeT.NONE;
        public AudioTypeT meThirdPartyAudio = AudioTypeT.NONE;
        public WorldAudioType meWorldAudio = WorldAudioType.NONE;
        public WorldAudioType meTileWorldAudio = WorldAudioType.NONE;
        public WorldAudioType meChainWorldAudio = WorldAudioType.NONE;
        public WorldAudioType meUnitWorldAudio = WorldAudioType.NONE;
        public WorldAudioType meReversalWorldAudio = WorldAudioType.NONE;
        public int miCount = 0;
        public int miDestroyUnitRange = 0;
        public int miEffectTime = 0;
        public int miEffectRange = 0;
        public int miEffectLength = 0;
        public int miBuildingModifier = 0;
        public int miResourceLevelChange = 0;
        public int miTakeoverTime = 0;
        public int miDefendTime = 0;
        public int miHarvestQuantity = 0;
        public int miPlunderQuantity = 0;
        public int miDamageBuilding = 0;
        public bool mbAnnounce = false;
        public bool mbHostile = false;
        public bool mbFreezeBuilding = false;
        public bool mbDoubleBuilding = false;
        public bool mbHalfBuilding = false;
        public bool mbOverloadBuilding = false;
        public bool mbVirusBuilding = false;
        public bool mbReturnClaim = false;
        public bool mbAuctionTile = false;
        public bool mbWrongBuilding = false;
        public bool mbRevealBuilding = false;
        public bool mbNewResource = false;
        public bool mbChangeTerrain = false;
        public bool mbDefendSabotage = false;
        public bool mbTriggersDefense = false;
        public bool mbIgnoresDebt = false;
        public bool mbAdvanced = false;
        public UnitType meUnit = UnitType.NONE;
        public SabotageType meType { get { return (SabotageType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readType<AssetType>(node, "SabotageAsset", ref meSabotageAsset);
            infos.readType<AssetType>(node, "TileAsset", ref meTileAsset);
            infos.readType<AssetType>(node, "ChainAsset", ref meChainAsset);
            infos.readType<AssetType>(node, "UnitAsset", ref meUnitAsset);
            infos.readType<AssetType>(node, "ReversalAsset", ref meReversalAsset);
            infos.readType<TextType>(node,"Helptext", ref meHelptext);
            infos.readType<TextType>(node, "AppearanceText", ref meAppearanceText);
            infos.readType<TextType>(node, "DefenderAlert", ref meDefenderAlert);
            infos.readType<TextType>(node, "DefenderNotificationTitle", ref meDefenderNotificationTitle);
            infos.readType<TextType>(node, "DefenderNotificationDesc", ref meDefenderNotificationDesc);
            infos.readType<EventAudioType>(node, "AuctionEventAudio", ref meAuctionEventAudio);
            infos.readType<AudioTypeT>(node, "AttackerAudio", ref meAttackerAudio);
            infos.readType<AudioTypeT>(node, "DefenderAudio", ref meDefenderAudio);
            infos.readType<AudioTypeT>(node, "ThirdPartyAudio", ref meThirdPartyAudio);
            infos.readType<WorldAudioType>(node, "WorldAudio", ref meWorldAudio);
            infos.readType<WorldAudioType>(node, "TileWorldAudio", ref meTileWorldAudio);
            infos.readType<WorldAudioType>(node, "ChainWorldAudio", ref meChainWorldAudio);
            infos.readType<WorldAudioType>(node, "UnitWorldAudio", ref meUnitWorldAudio);
            infos.readType<WorldAudioType>(node, "ReversalWorldAudio", ref meReversalWorldAudio);
            infos.readInt(node, "iCount", ref miCount);
            infos.readInt(node, "iDestroyUnitRange", ref miDestroyUnitRange);
            infos.readInt(node, "iEffectTime", ref miEffectTime);
            infos.readInt(node, "iEffectRange", ref miEffectRange);
            infos.readInt(node, "iEffectLength", ref miEffectLength);
            infos.readInt(node, "iResourceLevelChange", ref miResourceLevelChange);
            infos.readInt(node, "iTakeoverTime", ref miTakeoverTime);
            infos.readInt(node, "iDefendTime", ref miDefendTime);
            infos.readInt(node, "iHarvestQuantity", ref miHarvestQuantity);
            infos.readInt(node, "iPlunderQuantity", ref miPlunderQuantity);
            infos.readInt(node, "iDamageBuilding", ref miDamageBuilding);
            infos.readBool(node, "bAnnounce", ref mbAnnounce);
            infos.readBool(node, "bHostile", ref mbHostile);
            infos.readBool(node, "bFreezeBuilding", ref mbFreezeBuilding);
            infos.readBool(node, "bDoubleBuilding", ref mbDoubleBuilding);
            infos.readBool(node, "bHalfBuilding", ref mbHalfBuilding);
            infos.readBool(node, "bOverloadBuilding", ref mbOverloadBuilding);
            infos.readBool(node, "bVirusBuilding", ref mbVirusBuilding);
            infos.readBool(node, "bReturnClaim", ref mbReturnClaim);
            infos.readBool(node, "bAuctionTile", ref mbAuctionTile);
            infos.readBool(node, "bWrongBuilding", ref mbWrongBuilding);
            infos.readBool(node, "bRevealBuilding", ref mbRevealBuilding);
            infos.readBool(node, "bNewResource", ref mbNewResource);
            infos.readBool(node, "bChangeTerrain", ref mbChangeTerrain);
            infos.readBool(node, "bDefendSabotage", ref mbDefendSabotage);
            infos.readBool(node, "bTriggersDefense", ref mbTriggersDefense);
            infos.readBool(node, "bIgnoresDebt", ref mbIgnoresDebt);
            infos.readBool(node, "bAdvanced", ref mbAdvanced);
            infos.readType<UnitType>(node, "Unit", ref meUnit);
        }
    }

    public class InfoScenario : InfoBase
    {
        public TextType meName = TextType.NONE;
        public TextType meTooltip = TextType.NONE;
        public TextType meDescription = TextType.NONE;
        public TextType meCustomWinText = TextType.NONE;
        public ScenarioClassType meClass = ScenarioClassType.GENERAL;
        public ScenarioType meNextScenario = ScenarioType.NONE;
        public int miCategoryIndex = 0;
        public bool mbCanSave = false;
        public bool mbOptional = false;
        public bool mbIoDLC = false;
        public bool mbNoPauseSelection = false;
        public TextType meLoadingHintText = TextType.NONE;
        public AssetType meBackgroundAsset = AssetType.NONE;
        public AssetType meLoadingAsset = AssetType.NONE;
        public string mzModName = "";
        public string mzXmlRoot = "";
        public string mzGameSetup = "";
        public string mzAchievement = "";
        public string mzStat = "";
        public List<string> mazPrerequisites = new List<string>();
        public List<string> mazVisible = new List<string>();
        public List<ScenarioDifficultyType> maeDifficulties = new List<ScenarioDifficultyType>();
        public ScenarioType meType { get { return (ScenarioType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readType<TextType>(node, "Tooltip", ref meTooltip);
            infos.readType<TextType>(node, "Description", ref meDescription);
            infos.readType<TextType>(node, "WinText", ref meCustomWinText);
            infos.readType<ScenarioClassType>(node, "Class", ref meClass);
            infos.readType<ScenarioType>(node, "NextScenario", ref meNextScenario);
            infos.readInt(node, "iCategoryIndex", ref miCategoryIndex);
            infos.readBool(node, "bCanSave", ref mbCanSave);
            infos.readBool(node, "bOptional", ref mbOptional);
            infos.readBool(node, "bIoDLC", ref mbIoDLC);
            infos.readBool(node, "bNoPauseSelection", ref mbNoPauseSelection);
            infos.readType<TextType>(node, "LoadingHintText", ref meLoadingHintText);
            infos.readType<AssetType>(node, "BackgroundAsset", ref meBackgroundAsset);
            infos.readType<AssetType>(node, "LoadingAsset", ref meLoadingAsset);
            infos.readString(node, "zMod", ref mzModName);
            infos.readString(node, "zXmlRoot", ref mzXmlRoot);
            infos.readString(node, "zGameSetup", ref mzGameSetup);
            infos.readString(node, "zAchievement", ref mzAchievement);
            infos.readString(node, "zStat", ref mzStat);
            infos.readStrings(out mazPrerequisites, node, "Prerequisites");
            infos.readStrings(out mazVisible, node, "Visible");
            infos.readTypeStrings(out maeDifficulties, node, "Difficulties");
        }
    }

    public class InfoScenarioClass : InfoBase
    {
        public ScenarioClassType meType { get { return (ScenarioClassType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
        }
    }

    public class InfoScenarioDifficulty : InfoBase
    {
        public TextType meName = TextType.NONE;
        public ScenarioDifficultyType meType { get { return (ScenarioDifficultyType)miType; } }
        public int miScoreValue = 0;
        public bool mbAchievement = false;

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readInt(node, "iScoreValue", ref miScoreValue);
            infos.readBool(node, "bAchievement", ref mbAchievement);
        }
    }

    public class InfoSevenSols : InfoBase
    {
        public TextType meName = TextType.NONE;
        public SevenSolsType meType { get { return (SevenSolsType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
        }
    }

    public class InfoSpriteGroup : InfoBase
    {
        public List<AssetType> maeSpriteSheets = new List<AssetType>();
        public SpriteGroupType meType { get { return (SpriteGroupType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readTypeStrings<AssetType>(out maeSpriteSheets, node, "SpriteSheets");
        }
    }

    public class InfoStory : InfoBase
    {
        public TextType meTitle = TextType.NONE;
        public TextType meText = TextType.NONE;
        public int miTurn = 0;
        public bool mbUniqueSet = false;
        public bool mbOnNewGame = false;
        public bool mbOnWinGame = false;
        public bool mbOnLoseGame = false;
        public bool mbOnCampaignStart = false;
        public bool mbOnCampaignWon = false;
        public bool mbOnCampaignLost = false;
        public bool mbGeothermal = false;
        public bool mbNoGeothermal = false;
        public bool mbCanReuse = false;
        public PersonalityType mePersonality = PersonalityType.NONE;
        public PersonalityType mePersonalityOpponent = PersonalityType.NONE;
        public CampaignModeType meCampaignMode = CampaignModeType.NONE;
        public ColonyType meColony = ColonyType.NONE;
        public ColonyClassType meColonyClass = ColonyClassType.NONE;
        public SevenSolsType meSevenSols = SevenSolsType.NONE;
        public LocationType meLocation = LocationType.NONE;
        public LatitudeType meLatitude = LatitudeType.NONE;
        public BuildingType meBuildingDestroyEvent = BuildingType.NONE;
        public PerkType meHighestPerk = PerkType.NONE;
        public List<bool> mabWonCampaignMode = new List<bool>();
        public List<bool> mabNotWonCampaignMode = new List<bool>();
        public List<bool> mabWonCampaignPerk = new List<bool>();
        public StoryType meType { get { return (StoryType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Title", ref meTitle);
            infos.readType<TextType>(node, "Text", ref meText);
            infos.readInt(node, "iTurn", ref miTurn);
            infos.readBool(node, "bUniqueSet", ref mbUniqueSet);
            infos.readBool(node, "bOnNewGame", ref mbOnNewGame);
            infos.readBool(node, "bOnWinGame", ref mbOnWinGame);
            infos.readBool(node, "bOnLoseGame", ref mbOnLoseGame);
            infos.readBool(node, "bOnCampaignStart", ref mbOnCampaignStart);
            infos.readBool(node, "bOnCampaignWon", ref mbOnCampaignWon);
            infos.readBool(node, "bOnCampaignLost", ref mbOnCampaignLost);
            infos.readBool(node, "bGeothermal", ref mbGeothermal);
            infos.readBool(node, "bNoGeothermal", ref mbNoGeothermal);
            infos.readBool(node, "bCanReuse", ref mbCanReuse);
            infos.readType<PersonalityType>(node, "Personality", ref mePersonality);
            infos.readType<PersonalityType>(node, "PersonalityOpponent", ref mePersonalityOpponent);
            infos.readType<CampaignModeType>(node, "CampaignMode", ref meCampaignMode);
            infos.readType<ColonyType>(node, "Colony", ref meColony);
            infos.readType<ColonyClassType>(node, "ColonyClass", ref meColonyClass);
            infos.readType<SevenSolsType>(node, "SevenSols", ref meSevenSols);
            infos.readType<LocationType>(node, "Location", ref meLocation);
            infos.readType<LatitudeType>(node, "Latitude", ref meLatitude);
            infos.readType<BuildingType>(node, "BuildingDestroyEvent", ref meBuildingDestroyEvent);
            infos.readType<PerkType>(node, "HighestPerk", ref meHighestPerk);
            infos.readBoolsByType(mabWonCampaignMode, infos.campaignModes().Count, node, "WonCampaignMode");
            infos.readBoolsByType(mabNotWonCampaignMode, infos.campaignModes().Count, node, "NotWonCampaignMode");
            infos.readBoolsByType(mabWonCampaignPerk, infos.perks().Count, node, "WonCampaignPerk");
        }
    }

    public class InfoTechnology : InfoBase
    {
        public TextType meName = TextType.NONE;
        public TextType meShortName = TextType.NONE;
        public int miTime = 0;
        public int miTimePerLevel = 0;
        public List<int> maiResourceCost = new List<int>();
        public List<bool> mabResourceProduction = new List<bool>();
        public List<bool> mabLocationInvalid = new List<bool>();
        public TechnologyType meType { get { return (TechnologyType)miType; } } 

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readType<TextType>(node, "ShortName", ref meShortName);
            infos.readInt(node, "iTime", ref miTime);
            infos.readInt(node, "iTimePerLevel", ref miTimePerLevel);
            infos.readIntsByType(maiResourceCost, infos.resources().Count, node, "ResourceCost");
            infos.readBoolsByType(mabResourceProduction, infos.resources().Count, node, "ResourceProduction");
            infos.readBoolsByType(mabLocationInvalid, infos.locations().Count, node, "LocationInvalid");
        }
    }

    public class InfoTechnologyLevel : InfoBase
    {
        public TextType meName = TextType.NONE;
        public string mzIconName = "";
        public int miModifier = 0;
        public List<int> maiHQClaims = new List<int>();
        public TechnologyLevelType meType { get { return (TechnologyLevelType)miType; } } 

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readString(node, "zIconName", ref mzIconName);
            infos.readInt(node, "iModifier", ref miModifier);
            infos.readIntsByType(maiHQClaims, infos.HQs().Count, node, "HQClaims");
        }
    }

    public class InfoTerrain : InfoBase
    {
        public TextType meName = TextType.NONE;
        public List<string> mazTileIconNames = new List<string>();
        public AssetType meEditorAsset = AssetType.NONE;
        public List<int> maiGeothermalProb = new List<int>();
        public List<int> maiConstructionModifier = new List<int>();
        public bool mbEditorMaterial = false;
        public bool mbUsable = false;
        public bool mbRequiredOnly = false;
        public bool mbNoAdjacentIce = false;
        public bool mbNoResources = false;
        public bool mbAdjacentResource = false;
        public bool mbRegion = false;
        public bool mbUseBuildingPlatform = false;
        public ResourceType meResourceHint = ResourceType.NONE;
        public ColorType meHighlightColor = ColorType.NONE;
        public List<List<int>> maaiResourceProb = new List<List<int>>();
        public List<int> maiResourceRate = new List<int>();
        public List<List<int>> maaiLevelRollChange = new List<List<int>>();
        public List<bool> mabLocationInvalid = new List<bool>();
        public List<List<bool>> maabIceInvalid = new List<List<bool>>();
        public TerrainType meType { get { return (TerrainType)miType; } }

        //cached values
        public bool mbAnyResourceRate = false;

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readType<AssetType>(node, "EditorAsset", ref meEditorAsset);
            infos.readStringsByType(mazTileIconNames, infos.locations().Count, node, "TileIconNames");
            infos.readIntsByType(maiGeothermalProb, infos.locations().Count, node, "LocationGeothermalProb");
            infos.readIntsByType(maiConstructionModifier, infos.locations().Count, node, "LocationConstructionModifier");
            infos.readBool(node, "bEditorMaterial", ref mbEditorMaterial);
            infos.readBool(node, "bUsable", ref mbUsable);
            infos.readBool(node, "bRequiredOnly", ref mbRequiredOnly);
            infos.readBool(node, "bNoAdjacentIce", ref mbNoAdjacentIce);
            infos.readBool(node, "bNoResources", ref mbNoResources);
            infos.readBool(node, "bAdjacentResource", ref mbAdjacentResource);
            infos.readBool(node, "bRegion", ref mbRegion);
            infos.readBool(node, "bUseBuildingPlatform", ref mbUseBuildingPlatform);
            infos.readType<ResourceType>(node, "ResourceHint", ref meResourceHint);
            infos.readType<ColorType>(node, "HighlightColor", ref meHighlightColor);
            infos.readIntListByType(maaiResourceProb, infos.locations().Count, infos.resources().Count, node, "LocationResourceProb");
            infos.readIntsByType(maiResourceRate, infos.resources().Count, node, "ResourceRate");
            infos.readIntListByType(maaiLevelRollChange, infos.locations().Count, infos.resources().Count, node, "LocationLevelRollChange");
            infos.readBoolsByType(mabLocationInvalid, infos.locations().Count, node, "LocationInvalid");
            infos.readBoolListByType(maabIceInvalid, infos.locations().Count, infos.ices().Count, node, "LocationIceInvalid");
        }

        public override void UpdateReferences(Infos infos)
        {
            mbAnyResourceRate = ListUtilities.Any(maiResourceRate, x => (x > 0));
        }
    }

    public class InfoTerrainClass : InfoBase
    {
        public TextType meName = TextType.NONE;
        public TextType meDesc = TextType.NONE;
        public int miSizeModifier = 0;
        public int miHeightVariance = 0;
        public int miHeightOffset = 0;
        public int miRollModifier = 0;
        public int miStriationRolls = 0;
        public int miCraterWidthBase = 0;
        public int miCraterWidthRoll = 0;
        public List<int> maiTerrainRoll = new List<int>();
        public List<int> maiTerrainPercent = new List<int>();
        public List<int> maiTilesPerTerrain = new List<int>();
        public List<bool> mabLocationInvalid = new List<bool>();
        public TerrainClassType meType { get { return (TerrainClassType)miType; } } 

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readType<TextType>(node, "Desc", ref meDesc);
            infos.readInt(node, "iSizeModifier", ref miSizeModifier);
            infos.readInt(node, "iHeightVariance", ref miHeightVariance);
            infos.readInt(node, "iHeightOffset", ref miHeightOffset);
            infos.readInt(node, "iRollModifier", ref miRollModifier);
            infos.readInt(node, "iStriationRolls", ref miStriationRolls);
            infos.readInt(node, "iCraterWidthBase", ref miCraterWidthBase);
            infos.readInt(node, "iCraterWidthRoll", ref miCraterWidthRoll);
            infos.readIntsByType(maiTerrainRoll, infos.terrains().Count, node, "TerrainRoll");
            infos.readIntsByType(maiTerrainPercent, infos.terrains().Count, node, "TerrainPercent");
            infos.readIntsByType(maiTilesPerTerrain, infos.terrains().Count, node, "TilesPerTerrain");
            infos.readBoolsByType(mabLocationInvalid, infos.locations().Count, node, "LocationInvalid");
        }
    }

    public class InfoTerrainMaterial : InfoBase
    {
        public TerrainMaterialType meType { get { return (TerrainMaterialType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
        }
    }

    public class InfoText : InfoBase
    {
        public List<string> mazEntries = new List<string>();
        public TextType meType { get { return (TextType)miType; } } 

        public override void ReadData(XmlNode node, Infos infos)
        {
            //Note: InfoText requires that InfoLanguages be read first
            List<InfoLanguage> languages = infos.languages();
            mazEntries.Resize(languages.Count, "");
            for (int i=0; i<languages.Count; i++)
            {
                string fieldName = languages[i].mzFieldName;
                if(!string.IsNullOrEmpty(fieldName))
                {
                    XmlNode child = Infos.FindChild(node, fieldName);
                    if(child != null) //ignore missing languages
                    {
                        infos.addReadXmlField(fieldName); //validate xml fields
                        mazEntries[i] = child.InnerText;
                    }
                }

                //patch empty entries with the first (English) text
                if(string.IsNullOrEmpty(mazEntries[i]))
                {
                    mazEntries[i] = mazEntries[0];
                }
            }
        }
    }

    public class InfoUnit : InfoBase
    {
        public TextType meName = TextType.NONE;
        public AssetType meUnitAsset = AssetType.NONE;
        public AssetType meAttackedAsset = AssetType.NONE;
        public AssetType meFailedAsset = AssetType.NONE;
        public int miMovement = 0;
        public int miMining = 0;
        public int miPlunderRange = 0;
        public int miPlunderTimer = 0;
        public int miGas = 0;
        public bool mbImmuneDestroy = false;
        public bool mbGroundUnit = false;
        public bool mbHideTeamColor = false;
        public WorldAudioType meWorldAudio = WorldAudioType.NONE;
        public UnitType meType { get { return (UnitType)miType; } }

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readType<AssetType>(node, "UnitAsset", ref meUnitAsset);
            infos.readType<AssetType>(node, "AttackedAsset", ref meAttackedAsset);
            infos.readType<AssetType>(node, "FailedAsset", ref meFailedAsset);
            infos.readInt(node, "iMovement", ref miMovement);
            infos.readInt(node, "iMining", ref miMining);
            infos.readInt(node, "iPlunderRange", ref miPlunderRange); 
            infos.readInt(node, "iPlunderTimer", ref miPlunderTimer);
            infos.readInt(node, "iGas", ref miGas);
            infos.readBool(node, "bImmuneDestroy", ref mbImmuneDestroy);
            infos.readBool(node, "bGroundUnit", ref mbGroundUnit);
            infos.readBool(node, "bHideTeamColor", ref mbHideTeamColor);
            infos.readType<WorldAudioType>(node, "WorldAudio", ref meWorldAudio);
        }
    }

    public class InfoWind : InfoBase
    {
        public TextType meName = TextType.NONE;
        public AssetType meAsset = AssetType.NONE;
        public WindType meType { get { return (WindType)miType; } } 

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<TextType>(node, "Name", ref meName);
            infos.readType<AssetType>(node, "Asset", ref meAsset);
        }
    }

    public class InfoWorldAudio : InfoBase
    {
        public AudioTypeT meAudio = AudioTypeT.NONE;
        public bool mbLoop = false;
        public List<AudioTypeT> maeAudioFiles = new List<AudioTypeT>();
        public string mzBehavior = "";
        public WorldAudioType meType { get { return (WorldAudioType)miType; } } 

        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType<AudioTypeT>(node, "Audio", ref meAudio);
            infos.readBool(node, "bLoop", ref mbLoop);
            infos.readTypeStrings(out maeAudioFiles, node, "AudioFiles");
            infos.readString(node, "zBehavior", ref mzBehavior);
        }
    }

    public class InfoGlobals
    {
        public int RAYCAST_MINIMUM_WORLD_HEIGHT = 0;
        public int NUM_HINTS = 0;
        public int MINUTES_PER_TURN = 0;
        public int INITIAL_REVEAL_RANGE = 0;
        public int INITIAL_SCANS = 0;
        public int WARN_FOUND_BARRIER = 0;
        public int NO_ALL_HQS_FOUND_MONEY = 0;
        public int INITIAL_SHARES = 0;
        public int MAJORITY_SHARES = 0;
        public int SUBSIDIARY_MIN_MONEY = 0;
        public int SHARE_PURCHASE_SIZE = 0;
        public int BUYOUT_CLAIMS = 0;
        public int SEVEN_SOLS_MODULES = 0;
        public int COLONY_FLAT_RANGE = 0;
        public int COLONY_SHARE_PRICE_BASE = 0;
        public int COLONY_SHARE_PRICE_PER = 0;
        public int BASE_STOCK_VALUE = 0;
        public int BASE_STOCK_VALUE_PER_LEVEL = 0;
        public int STOCK_DELAY = 0;
        public int MIN_HQ_DISTANCE = 0;
        public int FOUND_CLAIM_BLOCK_TIME = 0;
        public int NEW_RESOURCE_CLAIM_BLOCK_TIME = 0;
        public int MANUAL_SALE_TIME = 0;
        public int AUTO_SUPPLY_TIME = 0;
        public int DEBT_PAYMENT = 0;
        public int EXCESS_BOND_INTEREST = 0;
        public int BLACK_MARKET_SLOTS = 0;
        public int BLACK_MARKET_DELAY = 0;
        public int ENTERTAINMENT_PROFIT = 0;
        public int ESPIONAGE_DIMINISH = 0;
        public int MODULE_REVEAL_TIME = 0;
        public int ORDER_RATE = 0;
        public int LAUNCH_TIME = 0;
        public int BOREHOLE_ENERGY_RATE = 0;
        public int MIN_AUCTION_BID = 0;
        public int AUCTION_BID = 0;
        public int AUCTION_TIME_LEFT = 0;
        public int AUCTION_TIME_BID = 0;
        public int AUCTION_DEFEND_TIME = 0;
        public int TAKEOVER_DEFEND_TIME = 0;
        public int SCAN_MULTIPLIER = 0;
        public int CAMPAIGN_PERKS_AVAILABLE_BUILDINGS = 0;
        public int CAMPAIGN_PERKS_AVAILABLE_OTHERS = 0;
        public int CAMPAIGN_PERK_BUILDING_BASE = 0;
        public int CAMPAIGN_PERK_BUILDING_BONUS = 0;
        public int COLONY_SHARE_STOCK_MODIFIER = 0;
        public int COLONY_CAP_BASE = 0;
        public int COLONY_GROWTH_ROLL = 0;
        public int WHOLESALE_BASE = 0;
        public int WHOLESALE_PER = 0;
        public int NUM_WHOLESALE_SLOTS = 0;
        public int NUM_LABOR_SLOTS = 0;
        public int USE_LOCAL_AUDIO_FILES = 0;
        public int PRINT_AUDIO_REQUESTS = 0;
        public int SHOW_PRICE_DISRUPTIONS_EARLY = 0;
        public int SKIRMISH_SERIALIZATION_VERSION = 0;
        public int UNIT_MISSION_COMPLETE_RANGE = 1;
        public int USE_FEMALE_AUCTION_VOICE = 0;
        public int USE_FEMALE_BLACK_MARKET_VOICE = 0;
        public int USE_FEMALE_SCAN_VOICE = 1;
        public int USE_FEMALE_HQ_EVENT_VOICE = 1;
        public int USE_FEMALE_BUILDING_EVENT_VOICE = 0;
        public int IMPORT_AMOUNT = 0;

        public GameSpeedType DEFAULT_GAMESPEED = GameSpeedType.NONE;
        public HandicapType DEFAULT_HANDICAP = HandicapType.NONE;
        public HandicapType SKIRMISH_HANDICAP = HandicapType.NONE;
        public HandicapType CAMPAIGN_HANDICAP = HandicapType.NONE;
        public HandicapType CHALLENGE_HANDICAP = HandicapType.NONE;
        public HandicapType CHALLENGE_START_HANDICAP = HandicapType.NONE;
        public MapSizeType DEFAULT_MAPSIZE = MapSizeType.NONE;
        public MapSizeType RANKED_2P_MAPSIZE = MapSizeType.NONE;
        public MapSizeType RANKED_4P_MAPSIZE = MapSizeType.NONE;
        public ResourceMinimumType DEFAULT_RESOURCEMINIMUM = ResourceMinimumType.NONE;
        public ResourcePresenceType DEFAULT_RESOURCEPRESENCE = ResourcePresenceType.NONE;
        public GameSetupType DEFAULT_GAMESETUP = GameSetupType.NONE;
        public RulesSetType DEFAULT_RULESSET = RulesSetType.NONE;
        public LightingEnvironmentType DEFAULT_LIGHTING_ENVIRONMENT = LightingEnvironmentType.NONE;
        public TerrainType DEFAULT_TERRAIN = TerrainType.NONE;
        public TerrainType CAVE_TERRAIN = TerrainType.NONE;
        public ExecutiveType DEFAULT_EXECUTIVE_HUMAN = ExecutiveType.NONE;
        public PersonalityType DEFAULT_PERSONALITY_HUMAN = PersonalityType.NONE;
        public ResourceType ENERGY_RESOURCE = ResourceType.NONE;
        public ResourceType GAS_RESOURCE = ResourceType.NONE;
        public ResourceLevelType FREE_RESOURCELEVEL = ResourceLevelType.NONE;
        public UnitType CLAIM_UNIT = UnitType.NONE;
        public UnitType REPAIR_UNIT = UnitType.NONE;
        public UnitType SHIP_UNIT = UnitType.NONE;
        public SabotageType NEW_RESOURCE_SABOTAGE = SabotageType.NONE;
        public BlackMarketType CAVE_TERRAIN_SABOTAGE = BlackMarketType.NONE;

        public KeyBindingType KEYBINDING_MOVE_HORIZONTAL_POSITIVE = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_MOVE_HORIZONTAL_NEGATIVE = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_MOVE_VERTICAL_POSITIVE = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_MOVE_VERTICAL_NEGATIVE = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_ZOOM_IN = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_ZOOM_OUT = KeyBindingType.NONE;
        
        public KeyBindingType KEYBINDING_TOGGLE_MUTE = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_SHOW_POST_GAME_SCREEN = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_SHOW_REVENUE = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_SHOW_NEW_RESOURCE = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_HIDE_BUILDINGS = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_TOGGLE_HEX = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_TOGGLE_SHIPMENT_PATHS = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_CYCLE_ALL_HQ = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_CYCLE_ALL_HQ_BACK = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_TOGGLE_RESOURCES = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_CANCEL_SELECTION = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_SELECT_RESOURCE_UP = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_SELECT_RESOURCE_DOWN = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_SELECT_RESOURCE_BUY_SMALL = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_SELECT_RESOURCE_BUY_MEDIUM = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_SELECT_RESOURCE_BUY_LARGE = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_SELECT_RESOURCE_SELL_SMALL = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_SELECT_RESOURCE_SELL_MEDIUM = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_SELECT_RESOURCE_SELL_LARGE = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_SELECT_RESOURCE_TOGGLE_AUTOSELL = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_SELECT_RESOURCE_CONSTRUCT = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_SHOW_CHAT_SCREEN = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_SHOW_CHAT_SCREEN_ALL = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_UPGRADE_HQ = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_CLAIM_MODE = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_BID_AUCTION = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_BID_AUCTION_OR_PAY_DEBT = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_SKIP_AUCTION = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_SELL_ALL_RESOURCES = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_TOGGLE_OFF_EVERYTHING = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_DELETE_BUILDING = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_DELETE_ALL_BUILDING = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_REPAIR_BUILDING = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_INCREASE_SPEED = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_DECREASE_SPEED = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_PAUSE = KeyBindingType.NONE;

        public KeyBindingType KEYBINDING_TOGGLE_BUILD_TAB = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_TOGGLE_PATENT_TAB = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_TOGGLE_OPTIMIZE_TAB = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_TOGGLE_HACK_TAB = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_TOGGLE_LAUNCH_TAB = KeyBindingType.NONE;

        public KeyBindingType KEYBINDING_CYCLE_PLAYER_1 = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_CYCLE_PLAYER_2 = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_CYCLE_PLAYER_3 = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_CYCLE_PLAYER_4 = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_CYCLE_PLAYER_5 = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_CYCLE_PLAYER_6 = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_CYCLE_PLAYER_7 = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_CYCLE_PLAYER_8 = KeyBindingType.NONE;

        public KeyBindingType KEYBINDING_TOGGLE_PLAYER_1 = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_TOGGLE_PLAYER_2 = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_TOGGLE_PLAYER_3 = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_TOGGLE_PLAYER_4 = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_TOGGLE_PLAYER_5 = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_TOGGLE_PLAYER_6 = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_TOGGLE_PLAYER_7 = KeyBindingType.NONE;
        public KeyBindingType KEYBINDING_TOGGLE_PLAYER_8 = KeyBindingType.NONE;

        public KeyBindingClassType KEYBINDCLASS_BUILD_BUILDING = KeyBindingClassType.NONE;
        public KeyBindingClassType KEYBINDCLASS_BUILD_HQ = KeyBindingClassType.NONE;
        public KeyBindingClassType KEYBINDCLASS_CAMERA_CONTROL = KeyBindingClassType.NONE;
        public KeyBindingClassType KEYBINDCLASS_TIME_CONTROL = KeyBindingClassType.NONE;
        
        public ConditionType WIN_CONDITION_DEFAULT = ConditionType.NONE;
        public ConditionType WIN_CONDITION_DAYS = ConditionType.NONE;
        public ConditionType WIN_CONDITION_MAX_UPGRADE = ConditionType.NONE;

        public MapNameType MAPNAME_DEFAULT = MapNameType.NONE;

        public AssetType ASSET_BORDER_SPLINE_LOCAL = AssetType.NONE;
        public AssetType ASSET_BORDER_SPLINE_OPPONENT = AssetType.NONE;
        public AssetType ASSET_WORLD_TEXT = AssetType.NONE;
        public AssetType ASSET_REGION_TEXT = AssetType.NONE;
        public AssetType ASSET_BUILDING_RESOURCE_TEXT = AssetType.NONE;
        public AssetType ASSET_FOG_OF_WAR_PROJECTOR = AssetType.NONE;
        public AssetType ASSET_FOG_OF_WAR_CAMERA = AssetType.NONE;
        public AssetType ASSET_HEX_GRID_PROJECTOR = AssetType.NONE;
        public AssetType ASSET_HEX_GRID_CAMERA = AssetType.NONE;
        public AssetType ASSET_CONSTRUCT_FLAG = AssetType.NONE;
        public AssetType ASSET_HIGHLIGHT_NORMAL = AssetType.NONE;
        public AssetType ASSET_HIGHLIGHT_BEST_TILE = AssetType.NONE;
        public AssetType ASSET_HIGHLIGHT_TEAM_COLOR_TILE = AssetType.NONE;
        public AssetType ASSET_HIGHLIGHT_SCAN_RESOURCE_FOUND = AssetType.NONE;
        public AssetType ASSET_HIGHLIGHT_SCAN_NORMAL = AssetType.NONE;
        public AssetType ASSET_HIGHLIGHT_SCAN_EMPTY = AssetType.NONE;
        public AssetType ASSET_HIGHLIGHT_SCENARIO = AssetType.NONE;
        public AssetType ASSET_BUILDING_PREVIEW_EFFECT = AssetType.NONE;
        public AssetType ASSET_SCAN_EFFECT = AssetType.NONE;
        public AssetType ASSET_PING_EFFECT = AssetType.NONE;
        public AssetType ASSET_SHIPMENT_INDICATOR = AssetType.NONE;
        public AssetType ASSET_SHIPPING_PATH = AssetType.NONE;
        public AssetType ASSET_GEOTHERMAL_VENT = AssetType.NONE;
        public AssetType ASSET_RESOURCE_HINT = AssetType.NONE;
        public AssetType ASSET_RESOURCE_STOCKPILE = AssetType.NONE;
        public AssetType ASSET_SHIPPING_CONTAINER = AssetType.NONE;
        public AssetType ASSET_SHIPPING_CONTAINERS_ROW = AssetType.NONE;
        public AssetType ASSET_DELIVERY_SHIP = AssetType.NONE;
        public AssetType ASSET_TRACK = AssetType.NONE;
        public AssetType ASSET_CLICKTOSCAN = AssetType.NONE;
        public AssetType ASSET_BUILDING_STATUS_OVERLAY = AssetType.NONE;
        public AssetType ASSET_ICON_MUTINY = AssetType.NONE;
        public AssetType ASSET_ICON_GOON_SQUAD = AssetType.NONE;
        public AssetType ASSET_HOLOGRAM_DOME = AssetType.NONE;
        public AssetType ASSET_SPY_REGION = AssetType.NONE;
        public AssetType ASSET_TELEPORTATION_FX = AssetType.NONE;
        public AssetType ASSET_BOOSTED_PRODUCTION_FX = AssetType.NONE;
        public AssetType ASSET_UNUSED_TILE_ASSET = AssetType.NONE;
        public AssetType ASSET_HQ_NAME_TEXT = AssetType.NONE;
        //UI
        public AssetType ASSET_FILE_BROWSER = AssetType.NONE;
        public AssetType ASSET_DEFAULT_SCREEN_BG = AssetType.NONE;
        public AssetType ASSET_UI_MENUBUTTON_VERTICAL = AssetType.NONE;
        public AssetType ASSET_UI_MENUBUTTON_NORMAL = AssetType.NONE;
        public AssetType ASSET_UI_MENU_SPACER = AssetType.NONE;
        public AssetType ASSET_UI_SUBMENUCONTROL = AssetType.NONE; // Submenu manager - appears when you click skirmish, campaign, etc
        public AssetType ASSET_UI_SUBMENUBUTTON = AssetType.NONE;
        public AssetType ASSET_UI_SMALLBUTTON = AssetType.NONE;// used on the skirmish/lobby screen for changing the game options
        public AssetType ASSET_UI_SECONDARYBUTTON = AssetType.NONE;
        public AssetType ASSET_UI_TEXTICONBUTTON = AssetType.NONE;// used on the skirmish/lobby screen for the back arrow/invite friends button. Text and then icon
        public AssetType ASSET_UI_GLOSSYBUTTON = AssetType.NONE;// used on the skirmish/lobby screen for the play button
        public AssetType ASSET_UI_TUTORIALCHECKBUTTON = AssetType.NONE;// used on the tutorial screen. Has a checkbox in it that we can toggle on and off.
        public AssetType ASSET_UI_TOGGLEINPUT = AssetType.NONE;
        public AssetType ASSET_UI_DROPDOWN = AssetType.NONE;
        public AssetType ASSET_UI_DROPDOWN_OPTION = AssetType.NONE;
        public AssetType ASSET_UI_DROPDOWN_LIST = AssetType.NONE;
        public AssetType ASSET_UI_SLIDER = AssetType.NONE;
        public AssetType ASSET_UI_NOTCHED_SLIDER = AssetType.NONE;
        public AssetType ASSET_UI_INPUT_HORIZONTAL_GROUP = AssetType.NONE;
        public AssetType ASSET_UI_LABEL = AssetType.NONE;
        public AssetType ASSET_UI_INPUT_FIELD = AssetType.NONE;
        public AssetType ASSET_UI_INPUT_FIELD_MULTILINE = AssetType.NONE;
        public AssetType ASSET_UI_CHAT_MESSAGE = AssetType.NONE;
        public AssetType ASSET_UI_POPUP_MANAGER = AssetType.NONE;
        public AssetType ASSET_UI_POPUP_DEFAULT = AssetType.NONE;
        public AssetType ASSET_UI_POPUP_CHARACTER_LEFT = AssetType.NONE;
        public AssetType ASSET_UI_POPUP_CHARACTER_RIGHT = AssetType.NONE;
        public AssetType ASSET_UI_POPUP_CHARACTER_CENTER = AssetType.NONE;
        public AssetType ASSET_UI_POPUP_CREDITS = AssetType.NONE;
        public AssetType ASSET_UI_POPUP_LOADING = AssetType.NONE;
        public AssetType ASSET_UI_POPUP_GAMERULES = AssetType.NONE;
        public AssetType ASSET_UI_POPUP_EDIT_HOTKEY = AssetType.NONE;
        public AssetType ASSET_UI_POPUP_LOBBY_INVITE_FRIENDS = AssetType.NONE;
        public AssetType ASSET_UI_POPUP_IN_GAME_QUICK_MATCH_FOUND = AssetType.NONE;
        public AssetType ASSET_UI_POPUP_BUG_REPORT = AssetType.NONE;
        public AssetType ASSET_UI_POPUP_ONE_LINE_QUESTION = AssetType.NONE;
        public AssetType ASSET_UI_POPUP_INPUT_FIELD = AssetType.NONE;
        public AssetType ASSET_UI_POPUP_CAMPAIGN_SETTINGS = AssetType.NONE;
        public AssetType ASSET_UI_POPUP_CAMPAIGN_HELP = AssetType.NONE;
        public AssetType ASSET_UI_OPTIONS_PANEL = AssetType.NONE;
        public AssetType ASSET_UI_OPTIONS_TAB = AssetType.NONE;
        public AssetType ASSET_UI_HOTKEYS_PANEL = AssetType.NONE;
        public AssetType ASSET_UI_HOTKEYS_PANEL_LISTING = AssetType.NONE;
        public AssetType ASSET_UI_ART_PACKS_PANEL = AssetType.NONE;
        public AssetType ASSET_UI_ART_PACKS_LISTING = AssetType.NONE;
        // tooltips
        public AssetType ASSET_UI_TOOLTIP_BASE = AssetType.NONE;
        public AssetType ASSET_UI_TOOLTIP_GENERAL = AssetType.NONE;
        public AssetType ASSET_UI_TOOLTIP_DESCRIPTION_TEXT = AssetType.NONE;
        public AssetType ASSET_UI_TOOLTIP_COST = AssetType.NONE;
        public AssetType ASSET_UI_TOOLTIP_COST_DESCRIPTION_YIELDS = AssetType.NONE;
        public AssetType ASSET_UI_TOOLTIP_COST_DESCRIPTION_YIELD_RESOURCE_LINE = AssetType.NONE;
        public AssetType ASSET_UI_TOOLTIP_COST_REQUIREMENT = AssetType.NONE;
        public AssetType ASSET_UI_TOOLTIP_BUYSELL = AssetType.NONE;
        public AssetType ASSET_UI_TOOLTIP_SCAN_RESOURCE_LEVELS = AssetType.NONE;
        public AssetType ASSET_UI_TOOLTIP_CHARTS_AND_GRAPHS = AssetType.NONE;
        public AssetType ASSET_UI_TOOLTIP_CAMPAIGN_SCREEN_GOVERNMENT_SUPPORT = AssetType.NONE;
        public AssetType ASSET_UI_TOOLTIP_CAMPAIGN_SCREEN_GOVERNMENT_SUPPORT_BONUS_ENTRY = AssetType.NONE;
        public AssetType ASSET_UI_WORLD_TOOLTIP_UNUSED_RESOURCE = AssetType.NONE;
        // Screen-Specific UI Components
        public AssetType ASSET_UI_SKIRMISH_PLAYER_LISTING = AssetType.NONE;
        public AssetType ASSET_UI_FINDLOBBY_SERVER_LISTING = AssetType.NONE;
        public AssetType ASSET_UI_LOBBY_INVITE_FRIENDS_ENTRY = AssetType.NONE;
        public AssetType ASSET_GENERIC_VICTORY = AssetType.NONE;
        // In-game UI Components
        public AssetType ASSET_UI_HUD_FOUND_HQ = AssetType.NONE;
        public AssetType ASSET_UI_HUD_SCAN_WORLD_RESOURCE_FOUND = AssetType.NONE;
        public AssetType ASSET_UI_HUD_FLYOUT_ORDER = AssetType.NONE;
        public AssetType ASSET_UI_HUD_RESEARCH_ROW_PROGRESS_ITEM = AssetType.NONE;
        public AssetType ASSET_UI_HUD_BUTTON_CONSTRUCTION_TIMER = AssetType.NONE;
        public AssetType ASSET_UI_HUD_WIN_GAME_FIREWORKS = AssetType.NONE;
        public AssetType ASSET_MOHAWK_TEXT_INLINE_MATERIAL = AssetType.NONE;
        //Terrain
        public AssetType ASSET_TERRAIN_COLLISION = AssetType.NONE;
        public AssetType ASSET_MAP_EDITOR_NORMAL_TILE = AssetType.NONE;

        public string ATTACH_NODE_BUILDING_STATUS = "";
        public string ATTACH_NODE_UNIT_TAKEOFF = "";
        public string ATTACH_NODE_UNIT_LANDING = "";
        public string ATTACH_NODE_IMPORT = "";
        public string ATTACH_NODE_EXPORT = "";
        public string ATTACH_NODE_RESOURCE = "";
        public string ATTACH_NODE_HQ_HATCH = "";
        public string ATTACH_NODE_HQ_IMPORT = "";
        public string ATTACH_NODE_HQ_EXPORT = "";
        public string ATTACH_NODE_HATCH = "";
        public string ICON_NAME_NEED_RESOURCES = "";
        public string ICON_NAME_SHORTAGE = "";
        public string ICON_NAME_SURPLUS = "";
        public string ICON_NAME_SHORTAGE_TRIANGLE = "";
        public string ICON_NAME_SURPLUS_TRIANGLE = "";
        public string ICON_NAME_LAUNCH = "";
        public string ICON_NAME_RESEARCH = "";
        public string GEOTHERMAL_TAG = "";

        public string GUI_ATLAS_SAVE_PATH = "";
        public string GUI_GEOTHERMAL_TILE_MARS_ICON_NAME = "";
        public string GUI_GEOTHERMAL_TILE_CERES_ICON_NAME = "";
        public string GUI_GEOTHERMAL_TILE_IO_ICON_NAME = "";
        public string GUI_GEOTHERMAL_TILE_EUROPA_ICON_NAME = "";

        public SpriteGroupType SPRITE_GROUP_BUILDING_ICONS = SpriteGroupType.NONE;
        public SpriteGroupType SPRITE_GROUP_RESOURCE_ICONS = SpriteGroupType.NONE;
        public SpriteGroupType SPRITE_GROUP_BLACKMARKET_ICONS = SpriteGroupType.NONE;
        public SpriteGroupType SPRITE_GROUP_SABOTAGE_ICONS = SpriteGroupType.NONE;
        public SpriteGroupType SPRITE_GROUP_PATENT_ICONS = SpriteGroupType.NONE;
        public SpriteGroupType SPRITE_GROUP_ORDER_ICONS = SpriteGroupType.NONE;
        public SpriteGroupType SPRITE_GROUP_GOVERNMENT_SUBSIDY_ICONS = SpriteGroupType.NONE;
        public SpriteGroupType SPRITE_GROUP_HQ_ICONS = SpriteGroupType.NONE;
        public SpriteGroupType SPRITE_GROUP_MODULE_ICONS = SpriteGroupType.NONE;
        public SpriteGroupType SPRITE_GROUP_HQ_FOOTPRINT_ICONS = SpriteGroupType.NONE;
        public SpriteGroupType SPRITE_GROUP_TECHNOLOGY_LEVEL_ICONS = SpriteGroupType.NONE;
        public SpriteGroupType SPRITE_GROUP_PERK_ICONS = SpriteGroupType.NONE;
        public SpriteGroupType SPRITE_GROUP_EVENT_STATE_ICONS = SpriteGroupType.NONE;
        public SpriteGroupType SPRITE_GROUP_CHARACTER_PORTRAITS = SpriteGroupType.NONE;
        public SpriteGroupType SPRITE_GROUP_CHARACTER_THIN_PORTRAITS = SpriteGroupType.NONE;
        public SpriteGroupType SPRITE_GROUP_CHARACTER_DIALOG_ART = SpriteGroupType.NONE;
        public SpriteGroupType SPRITE_GROUP_TERRAIN_TILE_ICONS = SpriteGroupType.NONE;
        public SpriteGroupType SPRITE_GROUP_ICE_TILE_ICONS = SpriteGroupType.NONE;
        public SpriteGroupType SPRITE_GROUP_ART_PACK_ICONS = SpriteGroupType.NONE;
        public SpriteGroupType SPRITE_GROUP_INLINE_ICONS = SpriteGroupType.NONE;

        public float UNIT_FLIGHT_HEIGHT = 0;

        public void ReadData(Infos infos)
        {
            RAYCAST_MINIMUM_WORLD_HEIGHT = infos.getGlobalInt("RAYCAST_MINIMUM_WORLD_HEIGHT");
            NUM_HINTS                   = infos.getGlobalInt("NUM_HINTS");
            MINUTES_PER_TURN            = infos.getGlobalInt("MINUTES_PER_TURN");
            INITIAL_REVEAL_RANGE        = infos.getGlobalInt("INITIAL_REVEAL_RANGE");
            INITIAL_SCANS               = infos.getGlobalInt("INITIAL_SCANS");
            WARN_FOUND_BARRIER          = infos.getGlobalInt("WARN_FOUND_BARRIER");
            NO_ALL_HQS_FOUND_MONEY      = infos.getGlobalInt("NO_ALL_HQS_FOUND_MONEY");
            INITIAL_SHARES              = infos.getGlobalInt("INITIAL_SHARES");
            MAJORITY_SHARES             = infos.getGlobalInt("MAJORITY_SHARES");
            SUBSIDIARY_MIN_MONEY        = infos.getGlobalInt("SUBSIDIARY_MIN_MONEY");
            SHARE_PURCHASE_SIZE         = infos.getGlobalInt("SHARE_PURCHASE_SIZE");
            BUYOUT_CLAIMS               = infos.getGlobalInt("BUYOUT_CLAIMS");
            SEVEN_SOLS_MODULES          = infos.getGlobalInt("SEVEN_SOLS_MODULES");
            COLONY_FLAT_RANGE           = infos.getGlobalInt("COLONY_FLAT_RANGE");
            COLONY_SHARE_PRICE_BASE     = infos.getGlobalInt("COLONY_SHARE_PRICE_BASE");
            COLONY_SHARE_PRICE_PER      = infos.getGlobalInt("COLONY_SHARE_PRICE_PER");
            BASE_STOCK_VALUE            = infos.getGlobalInt("BASE_STOCK_VALUE");
            BASE_STOCK_VALUE_PER_LEVEL  = infos.getGlobalInt("BASE_STOCK_VALUE_PER_LEVEL");
            STOCK_DELAY                 = infos.getGlobalInt("STOCK_DELAY");
            MIN_HQ_DISTANCE             = infos.getGlobalInt("MIN_HQ_DISTANCE");
            FOUND_CLAIM_BLOCK_TIME      = infos.getGlobalInt("FOUND_CLAIM_BLOCK_TIME");
            NEW_RESOURCE_CLAIM_BLOCK_TIME = infos.getGlobalInt("NEW_RESOURCE_CLAIM_BLOCK_TIME");
            AUTO_SUPPLY_TIME            = infos.getGlobalInt("AUTO_SUPPLY_TIME");
            DEBT_PAYMENT                = infos.getGlobalInt("DEBT_PAYMENT");
            EXCESS_BOND_INTEREST        = infos.getGlobalInt("EXCESS_BOND_INTEREST");
            BLACK_MARKET_SLOTS          = infos.getGlobalInt("BLACK_MARKET_SLOTS");
            BLACK_MARKET_DELAY          = infos.getGlobalInt("BLACK_MARKET_DELAY");
            ENTERTAINMENT_PROFIT        = infos.getGlobalInt("ENTERTAINMENT_PROFIT");
            ESPIONAGE_DIMINISH          = infos.getGlobalInt("ESPIONAGE_DIMINISH");
            MODULE_REVEAL_TIME          = infos.getGlobalInt("MODULE_REVEAL_TIME");
            ORDER_RATE                  = infos.getGlobalInt("ORDER_RATE");
            LAUNCH_TIME                 = infos.getGlobalInt("LAUNCH_TIME");
            BOREHOLE_ENERGY_RATE        = infos.getGlobalInt("BOREHOLE_ENERGY_RATE");
            MIN_AUCTION_BID             = infos.getGlobalInt("MIN_AUCTION_BID");
            AUCTION_BID                 = infos.getGlobalInt("AUCTION_BID");
            AUCTION_TIME_LEFT           = infos.getGlobalInt("AUCTION_TIME_LEFT");
            AUCTION_TIME_BID            = infos.getGlobalInt("AUCTION_TIME_BID");
            AUCTION_DEFEND_TIME         = infos.getGlobalInt("AUCTION_DEFEND_TIME");
            TAKEOVER_DEFEND_TIME        = infos.getGlobalInt("TAKEOVER_DEFEND_TIME");
            CAMPAIGN_PERKS_AVAILABLE_BUILDINGS = infos.getGlobalInt("CAMPAIGN_PERKS_AVAILABLE_BUILDINGS");
            CAMPAIGN_PERKS_AVAILABLE_OTHERS = infos.getGlobalInt("CAMPAIGN_PERKS_AVAILABLE_OTHERS");
            CAMPAIGN_PERK_BUILDING_BASE = infos.getGlobalInt("CAMPAIGN_PERK_BUILDING_BASE");
            CAMPAIGN_PERK_BUILDING_BONUS = infos.getGlobalInt("CAMPAIGN_PERK_BUILDING_BONUS");
            COLONY_SHARE_STOCK_MODIFIER = infos.getGlobalInt("COLONY_SHARE_STOCK_MODIFIER");
            COLONY_CAP_BASE             = infos.getGlobalInt("COLONY_CAP_BASE");
            COLONY_GROWTH_ROLL          = infos.getGlobalInt("COLONY_GROWTH_ROLL");
            WHOLESALE_BASE              = infos.getGlobalInt("WHOLESALE_BASE");
            WHOLESALE_PER               = infos.getGlobalInt("WHOLESALE_PER");
            NUM_WHOLESALE_SLOTS         = infos.getGlobalInt("NUM_WHOLESALE_SLOTS");
            NUM_LABOR_SLOTS             = infos.getGlobalInt("NUM_LABOR_SLOTS");
            SCAN_MULTIPLIER             = infos.getGlobalInt("SCAN_MULTIPLIER");
            USE_LOCAL_AUDIO_FILES       = infos.getGlobalInt("USE_LOCAL_AUDIO_FILES");
            PRINT_AUDIO_REQUESTS        = infos.getGlobalInt("PRINT_AUDIO_REQUESTS");
            SHOW_PRICE_DISRUPTIONS_EARLY = infos.getGlobalInt("SHOW_PRICE_DISRUPTIONS_EARLY");
            SKIRMISH_SERIALIZATION_VERSION = infos.getGlobalInt("SKIRMISH_SERIALIZATION_VERSION");
            UNIT_MISSION_COMPLETE_RANGE = infos.getGlobalInt("UNIT_MISSION_COMPLETE_RANGE");
            USE_FEMALE_AUCTION_VOICE    = infos.getGlobalInt("USE_FEMALE_AUCTION_VOICE");
            USE_FEMALE_BLACK_MARKET_VOICE = infos.getGlobalInt("USE_FEMALE_BLACK_MARKET_VOICE");
            USE_FEMALE_SCAN_VOICE       = infos.getGlobalInt("USE_FEMALE_SCAN_VOICE");
            USE_FEMALE_HQ_EVENT_VOICE   = infos.getGlobalInt("USE_FEMALE_HQ_EVENT_VOICE");
            USE_FEMALE_BUILDING_EVENT_VOICE = infos.getGlobalInt("USE_FEMALE_BUILDING_EVENT_VOICE");
            IMPORT_AMOUNT               = infos.getGlobalInt("IMPORT_AMOUNT");

            UNIT_FLIGHT_HEIGHT = infos.getGlobalFloat("UNIT_FLIGHT_HEIGHT");

            DEFAULT_GAMESPEED = infos.getGlobalType<GameSpeedType>("DEFAULT_GAMESPEED");
            DEFAULT_HANDICAP            = infos.getGlobalType<HandicapType>("DEFAULT_HANDICAP");
            SKIRMISH_HANDICAP           = infos.getGlobalType<HandicapType>("SKIRMISH_HANDICAP");
            CAMPAIGN_HANDICAP           = infos.getGlobalType<HandicapType>("CAMPAIGN_HANDICAP");
            CHALLENGE_HANDICAP          = infos.getGlobalType<HandicapType>("CHALLENGE_HANDICAP");
            CHALLENGE_START_HANDICAP    = infos.getGlobalType<HandicapType>("CHALLENGE_START_HANDICAP");
            DEFAULT_MAPSIZE             = infos.getGlobalType<MapSizeType>("DEFAULT_MAPSIZE");
            RANKED_2P_MAPSIZE           = infos.getGlobalType<MapSizeType>("RANKED_2P_MAPSIZE");
            RANKED_4P_MAPSIZE           = infos.getGlobalType<MapSizeType>("RANKED_4P_MAPSIZE");
            DEFAULT_RESOURCEMINIMUM     = infos.getGlobalType<ResourceMinimumType>("DEFAULT_RESOURCEMINIMUM");
            DEFAULT_RESOURCEPRESENCE    = infos.getGlobalType<ResourcePresenceType>("DEFAULT_RESOURCEPRESENCE");
            DEFAULT_TERRAIN             = infos.getGlobalType<TerrainType>("DEFAULT_TERRAIN");
            CAVE_TERRAIN                = infos.getGlobalType<TerrainType>("CAVE_TERRAIN");
            DEFAULT_GAMESETUP           = infos.getGlobalType<GameSetupType>("DEFAULT_GAMESETUP");
            DEFAULT_RULESSET            = infos.getGlobalType<RulesSetType>("DEFAULT_RULESSET");
            DEFAULT_LIGHTING_ENVIRONMENT= infos.getGlobalType<LightingEnvironmentType>("DEFAULT_LIGHTING_ENVIRONMENT");
            DEFAULT_EXECUTIVE_HUMAN     = infos.getGlobalType<ExecutiveType>("DEFAULT_EXECUTIVE_HUMAN");
            DEFAULT_PERSONALITY_HUMAN   = infos.getGlobalType<PersonalityType>("DEFAULT_PERSONALITY_HUMAN");
            ENERGY_RESOURCE             = infos.getGlobalType<ResourceType>("ENERGY_RESOURCE");
            GAS_RESOURCE                = infos.getGlobalType<ResourceType>("GAS_RESOURCE");
            FREE_RESOURCELEVEL          = infos.getGlobalType<ResourceLevelType>("FREE_RESOURCELEVEL");
            CLAIM_UNIT                  = infos.getGlobalType<UnitType>("CLAIM_UNIT");
            REPAIR_UNIT                 = infos.getGlobalType<UnitType>("REPAIR_UNIT");
            SHIP_UNIT                   = infos.getGlobalType<UnitType>("SHIP_UNIT");
            NEW_RESOURCE_SABOTAGE       = infos.getGlobalType<SabotageType>("NEW_RESOURCE_SABOTAGE");
            CAVE_TERRAIN_SABOTAGE       = infos.getGlobalType<BlackMarketType>("CAVE_TERRAIN_SABOTAGE");

            KEYBINDING_MOVE_HORIZONTAL_POSITIVE = infos.getType<KeyBindingType>("KEYBINDING_MOVE_HORIZONTAL_POSITIVE");
            KEYBINDING_MOVE_HORIZONTAL_NEGATIVE = infos.getType<KeyBindingType>("KEYBINDING_MOVE_HORIZONTAL_NEGATIVE");
            KEYBINDING_MOVE_VERTICAL_POSITIVE   = infos.getType<KeyBindingType>("KEYBINDING_MOVE_VERTICAL_POSITIVE");
            KEYBINDING_MOVE_VERTICAL_NEGATIVE   = infos.getType<KeyBindingType>("KEYBINDING_MOVE_VERTICAL_NEGATIVE");
            KEYBINDING_ZOOM_IN                  = infos.getType<KeyBindingType>("KEYBINDING_ZOOM_IN");
            KEYBINDING_ZOOM_OUT                 = infos.getType<KeyBindingType>("KEYBINDING_ZOOM_OUT");
            KEYBINDING_TOGGLE_MUTE              = infos.getType<KeyBindingType>("KEYBINDING_TOGGLE_MUTE");
            KEYBINDING_SHOW_POST_GAME_SCREEN    = infos.getType<KeyBindingType>("KEYBINDING_SHOW_POST_GAME_SCREEN");
            KEYBINDING_SHOW_REVENUE             = infos.getType<KeyBindingType>("KEYBINDING_SHOW_REVENUE");
            KEYBINDING_SHOW_NEW_RESOURCE        = infos.getType<KeyBindingType>("KEYBINDING_SHOW_NEW_RESOURCE");
            KEYBINDING_HIDE_BUILDINGS           = infos.getType<KeyBindingType>("KEYBINDING_HIDE_BUILDINGS");
            KEYBINDING_TOGGLE_HEX               = infos.getType<KeyBindingType>("KEYBINDING_TOGGLE_HEX");
            KEYBINDING_TOGGLE_SHIPMENT_PATHS    = infos.getType<KeyBindingType>("KEYBINDING_TOGGLE_SHIPMENT_PATHS");
            KEYBINDING_CYCLE_ALL_HQ             = infos.getType<KeyBindingType>("KEYBINDING_CYCLE_ALL_HQ");
            KEYBINDING_CYCLE_ALL_HQ_BACK        = infos.getType<KeyBindingType>("KEYBINDING_CYCLE_ALL_HQ_BACK");
            KEYBINDING_TOGGLE_RESOURCES         = infos.getType<KeyBindingType>("KEYBINDING_TOGGLE_RESOURCES");
            KEYBINDING_CANCEL_SELECTION         = infos.getType<KeyBindingType>("KEYBINDING_CANCEL_SELECTION");
            KEYBINDING_SELECT_RESOURCE_UP       = infos.getType<KeyBindingType>("KEYBINDING_SELECT_RESOURCE_UP");
            KEYBINDING_SELECT_RESOURCE_DOWN     = infos.getType<KeyBindingType>("KEYBINDING_SELECT_RESOURCE_DOWN");
            KEYBINDING_SELECT_RESOURCE_BUY_SMALL        = infos.getType<KeyBindingType>("KEYBINDING_SELECT_RESOURCE_BUY_SMALL");
            KEYBINDING_SELECT_RESOURCE_BUY_MEDIUM       = infos.getType<KeyBindingType>("KEYBINDING_SELECT_RESOURCE_BUY_MEDIUM");
            KEYBINDING_SELECT_RESOURCE_BUY_LARGE        = infos.getType<KeyBindingType>("KEYBINDING_SELECT_RESOURCE_BUY_LARGE");
            KEYBINDING_SELECT_RESOURCE_SELL_SMALL       = infos.getType<KeyBindingType>("KEYBINDING_SELECT_RESOURCE_SELL_SMALL");
            KEYBINDING_SELECT_RESOURCE_SELL_MEDIUM      = infos.getType<KeyBindingType>("KEYBINDING_SELECT_RESOURCE_SELL_MEDIUM");
            KEYBINDING_SELECT_RESOURCE_SELL_LARGE       = infos.getType<KeyBindingType>("KEYBINDING_SELECT_RESOURCE_SELL_LARGE");
            KEYBINDING_SELECT_RESOURCE_TOGGLE_AUTOSELL  = infos.getType<KeyBindingType>("KEYBINDING_SELECT_RESOURCE_TOGGLE_AUTOSELL");
            KEYBINDING_SELECT_RESOURCE_CONSTRUCT        = infos.getType<KeyBindingType>("KEYBINDING_SELECT_RESOURCE_CONSTRUCT");
            KEYBINDING_SHOW_CHAT_SCREEN         = infos.getType<KeyBindingType>("KEYBINDING_SHOW_CHAT_SCREEN");
            KEYBINDING_SHOW_CHAT_SCREEN_ALL     = infos.getType<KeyBindingType>("KEYBINDING_SHOW_CHAT_SCREEN_ALL");
            KEYBINDING_UPGRADE_HQ               = infos.getType<KeyBindingType>("KEYBINDING_UPGRADE_HQ");
            KEYBINDING_CLAIM_MODE               = infos.getType<KeyBindingType>("KEYBINDING_CLAIM_MODE");
            KEYBINDING_BID_AUCTION              = infos.getType<KeyBindingType>("KEYBINDING_BID_AUCTION");
            KEYBINDING_BID_AUCTION_OR_PAY_DEBT  = infos.getType<KeyBindingType>("KEYBINDING_BID_AUCTION_OR_PAY_DEBT");
            KEYBINDING_SKIP_AUCTION             = infos.getType<KeyBindingType>("KEYBINDING_SKIP_AUCTION");
            KEYBINDING_SELL_ALL_RESOURCES       = infos.getType<KeyBindingType>("KEYBINDING_SELL_ALL_RESOURCES");
            KEYBINDING_TOGGLE_OFF_EVERYTHING    = infos.getType<KeyBindingType>("KEYBINDING_TOGGLE_OFF_EVERYTHING");
            KEYBINDING_DELETE_BUILDING          = infos.getType<KeyBindingType>("KEYBINDING_DELETE_BUILDING");
            KEYBINDING_DELETE_ALL_BUILDING      = infos.getType<KeyBindingType>("KEYBINDING_DELETE_ALL_BUILDING");
            KEYBINDING_REPAIR_BUILDING          = infos.getType<KeyBindingType>("KEYBINDING_REPAIR_BUILDING");
            KEYBINDING_INCREASE_SPEED           = infos.getType<KeyBindingType>("KEYBINDING_INCREASE_SPEED");
            KEYBINDING_DECREASE_SPEED           = infos.getType<KeyBindingType>("KEYBINDING_DECREASE_SPEED");
            KEYBINDING_PAUSE                    = infos.getType<KeyBindingType>("KEYBINDING_PAUSE");

            KEYBINDING_TOGGLE_BUILD_TAB         = infos.getType<KeyBindingType>("KEYBINDING_TOGGLE_BUILD_TAB");
            KEYBINDING_TOGGLE_PATENT_TAB        = infos.getType<KeyBindingType>("KEYBINDING_TOGGLE_PATENT_TAB");
            KEYBINDING_TOGGLE_OPTIMIZE_TAB      = infos.getType<KeyBindingType>("KEYBINDING_TOGGLE_OPTIMIZE_TAB");
            KEYBINDING_TOGGLE_HACK_TAB          = infos.getType<KeyBindingType>("KEYBINDING_TOGGLE_HACK_TAB");
            KEYBINDING_TOGGLE_LAUNCH_TAB        = infos.getType<KeyBindingType>("KEYBINDING_TOGGLE_LAUNCH_TAB");
            
            KEYBINDING_CYCLE_PLAYER_1           = infos.getType<KeyBindingType>("KEYBINDING_CYCLE_PLAYER_1");
            KEYBINDING_CYCLE_PLAYER_2           = infos.getType<KeyBindingType>("KEYBINDING_CYCLE_PLAYER_2");
            KEYBINDING_CYCLE_PLAYER_3           = infos.getType<KeyBindingType>("KEYBINDING_CYCLE_PLAYER_3");
            KEYBINDING_CYCLE_PLAYER_4           = infos.getType<KeyBindingType>("KEYBINDING_CYCLE_PLAYER_4");
            KEYBINDING_CYCLE_PLAYER_5           = infos.getType<KeyBindingType>("KEYBINDING_CYCLE_PLAYER_5");
            KEYBINDING_CYCLE_PLAYER_6           = infos.getType<KeyBindingType>("KEYBINDING_CYCLE_PLAYER_6");
            KEYBINDING_CYCLE_PLAYER_7           = infos.getType<KeyBindingType>("KEYBINDING_CYCLE_PLAYER_7");
            KEYBINDING_CYCLE_PLAYER_8           = infos.getType<KeyBindingType>("KEYBINDING_CYCLE_PLAYER_8");

            KEYBINDING_TOGGLE_PLAYER_1          = infos.getType<KeyBindingType>("KEYBINDING_TOGGLE_PLAYER_1");
            KEYBINDING_TOGGLE_PLAYER_2          = infos.getType<KeyBindingType>("KEYBINDING_TOGGLE_PLAYER_2");
            KEYBINDING_TOGGLE_PLAYER_3          = infos.getType<KeyBindingType>("KEYBINDING_TOGGLE_PLAYER_3");
            KEYBINDING_TOGGLE_PLAYER_4          = infos.getType<KeyBindingType>("KEYBINDING_TOGGLE_PLAYER_4");
            KEYBINDING_TOGGLE_PLAYER_5          = infos.getType<KeyBindingType>("KEYBINDING_TOGGLE_PLAYER_5");
            KEYBINDING_TOGGLE_PLAYER_6          = infos.getType<KeyBindingType>("KEYBINDING_TOGGLE_PLAYER_6");
            KEYBINDING_TOGGLE_PLAYER_7          = infos.getType<KeyBindingType>("KEYBINDING_TOGGLE_PLAYER_7");
            KEYBINDING_TOGGLE_PLAYER_8          = infos.getType<KeyBindingType>("KEYBINDING_TOGGLE_PLAYER_8");

            KEYBINDCLASS_BUILD_BUILDING         = infos.getType<KeyBindingClassType>("KEYBINDCLASS_BUILD_BUILDING");
            KEYBINDCLASS_BUILD_HQ               = infos.getType<KeyBindingClassType>("KEYBINDCLASS_BUILD_HQ");
            KEYBINDCLASS_CAMERA_CONTROL         = infos.getType<KeyBindingClassType>("KEYBINDCLASS_CAMERA_CONTROL");
            KEYBINDCLASS_TIME_CONTROL           = infos.getType<KeyBindingClassType>("KEYBINDCLASS_TIME_CONTROL");
            
            WIN_CONDITION_DEFAULT               = infos.getType<ConditionType>("WIN_CONDITION_DEFAULT");
            WIN_CONDITION_DAYS                  = infos.getType<ConditionType>("WIN_CONDITION_DAYS");
            WIN_CONDITION_MAX_UPGRADE           = infos.getType<ConditionType>("WIN_CONDITION_MAX_UPGRADE");

            MAPNAME_DEFAULT                     = infos.getType<MapNameType>("MAPNAME_DEFAULT");

            ASSET_BORDER_SPLINE_LOCAL = infos.getType<AssetType>("ASSET_BORDER_SPLINE_LOCAL");
            ASSET_BORDER_SPLINE_OPPONENT = infos.getType<AssetType>("ASSET_BORDER_SPLINE_OPPONENT");
            ASSET_WORLD_TEXT = infos.getType<AssetType>("ASSET_WORLD_TEXT");
            ASSET_REGION_TEXT = infos.getType<AssetType>("ASSET_REGION_TEXT");
            ASSET_BUILDING_RESOURCE_TEXT = infos.getType<AssetType>("ASSET_BUILDING_RESOURCE_TEXT");
            ASSET_FOG_OF_WAR_PROJECTOR = infos.getType<AssetType>("ASSET_FOG_OF_WAR_PROJECTOR");
            ASSET_FOG_OF_WAR_CAMERA = infos.getType<AssetType>("ASSET_FOG_OF_WAR_CAMERA");
            ASSET_HEX_GRID_PROJECTOR = infos.getType<AssetType>("ASSET_HEX_GRID_PROJECTOR");
            ASSET_HEX_GRID_CAMERA = infos.getType<AssetType>("ASSET_HEX_GRID_CAMERA");
            ASSET_CONSTRUCT_FLAG = infos.getType<AssetType>("ASSET_CONSTRUCT_FLAG");
            ASSET_HIGHLIGHT_NORMAL = infos.getType<AssetType>("ASSET_HIGHLIGHT_NORMAL");
            ASSET_HIGHLIGHT_BEST_TILE = infos.getType<AssetType>("ASSET_HIGHLIGHT_BEST_TILE");
            ASSET_HIGHLIGHT_TEAM_COLOR_TILE = infos.getType<AssetType>("ASSET_HIGHLIGHT_TEAM_COLOR_TILE");
            ASSET_HIGHLIGHT_SCAN_RESOURCE_FOUND = infos.getType<AssetType>("ASSET_HIGHLIGHT_SCAN_RESOURCE_FOUND");
            ASSET_HIGHLIGHT_SCAN_NORMAL = infos.getType<AssetType>("ASSET_HIGHLIGHT_SCAN_NORMAL");
            ASSET_HIGHLIGHT_SCAN_EMPTY = infos.getType<AssetType>("ASSET_HIGHLIGHT_SCAN_EMPTY");
            ASSET_HIGHLIGHT_SCENARIO = infos.getType<AssetType>("ASSET_HIGHLIGHT_SCENARIO");
            ASSET_BUILDING_PREVIEW_EFFECT = infos.getType<AssetType>("ASSET_BUILDING_PREVIEW_EFFECT");
            ASSET_SCAN_EFFECT = infos.getType<AssetType>("ASSET_SCAN_EFFECT");
            ASSET_PING_EFFECT = infos.getType<AssetType>("ASSET_PING_EFFECT");
            ASSET_SHIPMENT_INDICATOR = infos.getType<AssetType>("ASSET_SHIPMENT_INDICATOR");
            ASSET_SHIPPING_PATH = infos.getType<AssetType>("ASSET_SHIPPING_PATH");
            ASSET_GEOTHERMAL_VENT = infos.getType<AssetType>("ASSET_GEOTHERMAL_VENT");
            ASSET_RESOURCE_HINT = infos.getType<AssetType>("ASSET_RESOURCE_HINT");
            ASSET_RESOURCE_STOCKPILE = infos.getType<AssetType>("ASSET_RESOURCE_STOCKPILE");
            ASSET_SHIPPING_CONTAINER = infos.getType<AssetType>("ASSET_SHIPPING_CONTAINER");
            ASSET_SHIPPING_CONTAINERS_ROW = infos.getType<AssetType>("ASSET_SHIPPING_CONTAINERS_ROW");
            ASSET_DELIVERY_SHIP = infos.getType<AssetType>("ASSET_DELIVERY_SHIP");
            ASSET_TRACK = infos.getType<AssetType>("ASSET_TRACK");
            ASSET_CLICKTOSCAN = infos.getType<AssetType>("ASSET_CLICKTOSCAN");
            ASSET_BUILDING_STATUS_OVERLAY = infos.getType<AssetType>("ASSET_BUILDING_STATUS_OVERLAY");
            ASSET_ICON_MUTINY = infos.getType<AssetType>("ASSET_ICON_MUTINY");
            ASSET_ICON_GOON_SQUAD = infos.getType<AssetType>("ASSET_ICON_GOON_SQUAD");
            ASSET_HOLOGRAM_DOME = infos.getType<AssetType>("ASSET_HOLOGRAM_DOME");
            ASSET_SPY_REGION = infos.getType<AssetType>("ASSET_SPY_REGION");
            ASSET_TELEPORTATION_FX = infos.getType<AssetType>("ASSET_TELEPORTATION_FX");
            ASSET_BOOSTED_PRODUCTION_FX = infos.getType<AssetType>("ASSET_BOOSTED_PRODUCTION_FX");
            ASSET_UNUSED_TILE_ASSET = infos.getType<AssetType>("ASSET_UNUSED_TILE_ASSET");
            ASSET_HQ_NAME_TEXT = infos.getType<AssetType>("ASSET_HQ_NAME_TEXT");
            //UI
            ASSET_FILE_BROWSER = infos.getType<AssetType>("ASSET_FILE_BROWSER");
            ASSET_DEFAULT_SCREEN_BG = infos.getType<AssetType>("ASSET_DEFAULT_SCREEN_BG");
            ASSET_UI_MENUBUTTON_VERTICAL = infos.getType<AssetType>("ASSET_UI_MENUBUTTON_VERTICAL");
            ASSET_UI_MENUBUTTON_NORMAL = infos.getType<AssetType>("ASSET_UI_MENUBUTTON_NORMAL");
            ASSET_UI_MENU_SPACER = infos.getType<AssetType>("ASSET_UI_MENU_SPACER");
            ASSET_UI_SUBMENUCONTROL = infos.getType<AssetType>("ASSET_UI_SUBMENUCONTROL");
            ASSET_UI_SUBMENUBUTTON = infos.getType<AssetType>("ASSET_UI_SUBMENUBUTTON");
            ASSET_UI_SMALLBUTTON = infos.getType<AssetType>("ASSET_UI_SMALLBUTTON");
            ASSET_UI_SECONDARYBUTTON = infos.getType<AssetType>("ASSET_UI_SECONDARYBUTTON");
            ASSET_UI_TEXTICONBUTTON = infos.getType<AssetType>("ASSET_UI_TEXTICONBUTTON");
            ASSET_UI_GLOSSYBUTTON = infos.getType<AssetType>("ASSET_UI_GLOSSYBUTTON");
            ASSET_UI_TUTORIALCHECKBUTTON = infos.getType<AssetType>("ASSET_UI_TUTORIALCHECKBUTTON");
            ASSET_UI_TOGGLEINPUT = infos.getType<AssetType>("ASSET_UI_TOGGLEINPUT");
            ASSET_UI_DROPDOWN = infos.getType<AssetType>("ASSET_UI_DROPDOWN");
            ASSET_UI_DROPDOWN_OPTION = infos.getType<AssetType>("ASSET_UI_DROPDOWN_OPTION");
            ASSET_UI_DROPDOWN_LIST = infos.getType<AssetType>("ASSET_UI_DROPDOWN_LIST");
            ASSET_UI_SLIDER = infos.getType<AssetType>("ASSET_UI_SLIDER");
            ASSET_UI_NOTCHED_SLIDER = infos.getType<AssetType>("ASSET_UI_NOTCHED_SLIDER");
            ASSET_UI_INPUT_HORIZONTAL_GROUP = infos.getType<AssetType>("ASSET_UI_INPUT_HORIZONTAL_GROUP");
            ASSET_UI_LABEL = infos.getType<AssetType>("ASSET_UI_LABEL");
            ASSET_UI_INPUT_FIELD = infos.getType<AssetType>("ASSET_UI_INPUT_FIELD");
            ASSET_UI_INPUT_FIELD_MULTILINE = infos.getType<AssetType>("ASSET_UI_INPUT_FIELD_MULTILINE");
            ASSET_UI_CHAT_MESSAGE = infos.getType<AssetType>("ASSET_UI_CHAT_MESSAGE");
            // Popups
            ASSET_UI_POPUP_MANAGER = infos.getType<AssetType>("ASSET_UI_POPUP_MANAGER");
            ASSET_UI_POPUP_DEFAULT = infos.getType<AssetType>("ASSET_UI_POPUP_DEFAULT");
            ASSET_UI_POPUP_CHARACTER_LEFT = infos.getType<AssetType>("ASSET_UI_POPUP_CHARACTER_LEFT");
            ASSET_UI_POPUP_CHARACTER_RIGHT = infos.getType<AssetType>("ASSET_UI_POPUP_CHARACTER_RIGHT");
            ASSET_UI_POPUP_CHARACTER_CENTER = infos.getType<AssetType>("ASSET_UI_POPUP_CHARACTER_CENTER");
            ASSET_UI_POPUP_CREDITS = infos.getType<AssetType>("ASSET_UI_POPUP_CREDITS");
            ASSET_UI_POPUP_LOADING = infos.getType<AssetType>("ASSET_UI_POPUP_LOADING");
            ASSET_UI_POPUP_GAMERULES = infos.getType<AssetType>("ASSET_UI_POPUP_GAMERULES");
            ASSET_UI_POPUP_EDIT_HOTKEY = infos.getType<AssetType>("ASSET_UI_POPUP_EDIT_HOTKEY");
            ASSET_UI_POPUP_LOBBY_INVITE_FRIENDS = infos.getType<AssetType>("ASSET_UI_POPUP_LOBBY_INVITE_FRIENDS");
            ASSET_UI_POPUP_IN_GAME_QUICK_MATCH_FOUND = infos.getType<AssetType>("ASSET_UI_POPUP_IN_GAME_QUICK_MATCH_FOUND");
            ASSET_UI_POPUP_BUG_REPORT = infos.getType<AssetType>("ASSET_UI_POPUP_BUG_REPORT");
            ASSET_UI_POPUP_ONE_LINE_QUESTION = infos.getType<AssetType>("ASSET_UI_POPUP_ONE_LINE_QUESTION");
            ASSET_UI_POPUP_INPUT_FIELD = infos.getType<AssetType>("ASSET_UI_POPUP_INPUT_FIELD");
            ASSET_UI_POPUP_CAMPAIGN_SETTINGS = infos.getType<AssetType>("ASSET_UI_POPUP_CAMPAIGN_SETTINGS");
            ASSET_UI_POPUP_CAMPAIGN_HELP = infos.getType<AssetType>("ASSET_UI_POPUP_CAMPAIGN_HELP");
            // Tooltip
            ASSET_UI_TOOLTIP_BASE = infos.getType<AssetType>("ASSET_UI_TOOLTIP_BASE");
            ASSET_UI_TOOLTIP_GENERAL = infos.getType<AssetType>("ASSET_UI_TOOLTIP_GENERAL");
            ASSET_UI_TOOLTIP_DESCRIPTION_TEXT = infos.getType<AssetType>("ASSET_UI_TOOLTIP_DESCRIPTION_TEXT");
            ASSET_UI_TOOLTIP_COST = infos.getType<AssetType>("ASSET_UI_TOOLTIP_COST");
            ASSET_UI_TOOLTIP_COST_DESCRIPTION_YIELDS = infos.getType<AssetType>("ASSET_UI_TOOLTIP_COST_DESCRIPTION_YIELDS");
            ASSET_UI_TOOLTIP_COST_DESCRIPTION_YIELD_RESOURCE_LINE = infos.getType<AssetType>("ASSET_UI_TOOLTIP_COST_DESCRIPTION_YIELD_RESOURCE_LINE");
            ASSET_UI_TOOLTIP_COST_REQUIREMENT = infos.getType<AssetType>("ASSET_UI_TOOLTIP_COST_REQUIREMENT");
            ASSET_UI_TOOLTIP_BUYSELL = infos.getType<AssetType>("ASSET_UI_TOOLTIP_BUYSELL");
            ASSET_UI_TOOLTIP_SCAN_RESOURCE_LEVELS = infos.getType<AssetType>("ASSET_UI_TOOLTIP_SCAN_RESOURCE_LEVELS");
            ASSET_UI_TOOLTIP_CHARTS_AND_GRAPHS = infos.getType<AssetType>("ASSET_UI_TOOLTIP_CHARTS_AND_GRAPHS");
            ASSET_UI_TOOLTIP_CAMPAIGN_SCREEN_GOVERNMENT_SUPPORT = infos.getType<AssetType>("ASSET_UI_TOOLTIP_CAMPAIGN_SCREEN_GOVERNMENT_SUPPORT");
            ASSET_UI_TOOLTIP_CAMPAIGN_SCREEN_GOVERNMENT_SUPPORT_BONUS_ENTRY = infos.getType<AssetType>("ASSET_UI_TOOLTIP_CAMPAIGN_SCREEN_GOVERNMENT_SUPPORT_BONUS_ENTRY");
            ASSET_UI_WORLD_TOOLTIP_UNUSED_RESOURCE = infos.getType<AssetType>("ASSET_UI_WORLD_TOOLTIP_UNUSED_RESOURCE");
            // Screen-Specific UI Prefabs
            ASSET_UI_SKIRMISH_PLAYER_LISTING = infos.getType<AssetType>("ASSET_UI_SKIRMISH_PLAYER_LISTING");
            ASSET_UI_LOBBY_INVITE_FRIENDS_ENTRY = infos.getType<AssetType>("ASSET_UI_LOBBY_INVITE_FRIENDS_ENTRY");
            ASSET_GENERIC_VICTORY = infos.getType<AssetType>("ASSET_GENERIC_VICTORY");
            // In-game UI prefabs
            ASSET_UI_HUD_FOUND_HQ = infos.getType<AssetType>("ASSET_UI_HUD_FOUND_HQ");
            ASSET_UI_HUD_SCAN_WORLD_RESOURCE_FOUND = infos.getType<AssetType>("ASSET_UI_HUD_SCAN_WORLD_RESOURCE_FOUND");
            ASSET_UI_HUD_FLYOUT_ORDER = infos.getType<AssetType>("ASSET_UI_HUD_FLYOUT_ORDER");
            ASSET_UI_HUD_RESEARCH_ROW_PROGRESS_ITEM = infos.getType<AssetType>("ASSET_UI_HUD_RESEARCH_ROW_PROGRESS_ITEM");
            ASSET_UI_HUD_BUTTON_CONSTRUCTION_TIMER = infos.getType<AssetType>("ASSET_UI_HUD_BUTTON_CONSTRUCTION_TIMER");
            ASSET_UI_HUD_WIN_GAME_FIREWORKS = infos.getType<AssetType>("ASSET_UI_HUD_WIN_GAME_FIREWORKS");
            ASSET_MOHAWK_TEXT_INLINE_MATERIAL = infos.getType<AssetType>("ASSET_MOHAWK_TEXT_INLINE_MATERIAL");
            // Option UI prefabs
            ASSET_UI_OPTIONS_PANEL = infos.getType<AssetType>("ASSET_UI_OPTIONS_PANEL");
            ASSET_UI_OPTIONS_TAB = infos.getType<AssetType>("ASSET_UI_OPTIONS_TAB");
            ASSET_UI_FINDLOBBY_SERVER_LISTING = infos.getType<AssetType>("ASSET_UI_FINDLOBBY_SERVER_LISTING");
            ASSET_UI_HOTKEYS_PANEL = infos.getType<AssetType>("ASSET_UI_HOTKEYS_PANEL");
            ASSET_UI_HOTKEYS_PANEL_LISTING = infos.getType<AssetType>("ASSET_UI_HOTKEYS_PANEL_LISTING");
            ASSET_UI_ART_PACKS_PANEL = infos.getType<AssetType>("ASSET_UI_ART_PACKS_PANEL");
            ASSET_UI_ART_PACKS_LISTING = infos.getType<AssetType>("ASSET_UI_ART_PACKS_LISTING");
            //Terrain
            ASSET_TERRAIN_COLLISION = infos.getType<AssetType>("ASSET_TERRAIN_COLLISION");
            ASSET_MAP_EDITOR_NORMAL_TILE = infos.getType<AssetType>("ASSET_MAP_EDITOR_NORMAL_TILE");

            ATTACH_NODE_BUILDING_STATUS = infos.getGlobalString("ATTACH_NODE_BUILDING_STATUS");
            ATTACH_NODE_UNIT_TAKEOFF = infos.getGlobalString("ATTACH_NODE_UNIT_TAKEOFF");
            ATTACH_NODE_UNIT_LANDING = infos.getGlobalString("ATTACH_NODE_UNIT_LANDING");
            ATTACH_NODE_IMPORT = infos.getGlobalString("ATTACH_NODE_IMPORT");
            ATTACH_NODE_EXPORT = infos.getGlobalString("ATTACH_NODE_EXPORT");
            ATTACH_NODE_RESOURCE = infos.getGlobalString("ATTACH_NODE_RESOURCE");
            ATTACH_NODE_HQ_HATCH = infos.getGlobalString("ATTACH_NODE_HQ_HATCH");
            ATTACH_NODE_HQ_EXPORT = infos.getGlobalString("ATTACH_NODE_HQ_EXPORT");
            ATTACH_NODE_HQ_IMPORT = infos.getGlobalString("ATTACH_NODE_HQ_IMPORT");
            ATTACH_NODE_HATCH = infos.getGlobalString("ATTACH_NODE_HATCH");
            ICON_NAME_NEED_RESOURCES = infos.getGlobalString("ICON_NAME_NEED_RESOURCES");
            ICON_NAME_SHORTAGE = infos.getGlobalString("ICON_NAME_SHORTAGE");
            ICON_NAME_SURPLUS = infos.getGlobalString("ICON_NAME_SURPLUS");
            ICON_NAME_SHORTAGE_TRIANGLE = infos.getGlobalString("ICON_NAME_SHORTAGE_TRIANGLE");
            ICON_NAME_SURPLUS_TRIANGLE = infos.getGlobalString("ICON_NAME_SURPLUS_TRIANGLE");
            ICON_NAME_LAUNCH = infos.getGlobalString("ICON_NAME_LAUNCH");
            ICON_NAME_RESEARCH = infos.getGlobalString("ICON_NAME_RESEARCH");
            GEOTHERMAL_TAG = infos.getGlobalString("GEOTHERMAL_TAG");

            // UI Prefabs
            GUI_ATLAS_SAVE_PATH = infos.getGlobalString("GUI_ATLAS_SAVE_PATH");
            GUI_GEOTHERMAL_TILE_MARS_ICON_NAME = infos.getGlobalString("GUI_GEOTHERMAL_TILE_MARS_ICON_NAME");
            GUI_GEOTHERMAL_TILE_CERES_ICON_NAME = infos.getGlobalString("GUI_GEOTHERMAL_TILE_CERES_ICON_NAME");
            GUI_GEOTHERMAL_TILE_IO_ICON_NAME = infos.getGlobalString("GUI_GEOTHERMAL_TILE_IO_ICON_NAME");
            GUI_GEOTHERMAL_TILE_EUROPA_ICON_NAME = infos.getGlobalString("GUI_GEOTHERMAL_TILE_EUROPA_ICON_NAME");

            SPRITE_GROUP_BUILDING_ICONS                 = infos.getType<SpriteGroupType>("SPRITE_GROUP_BUILDING_ICONS");
            SPRITE_GROUP_RESOURCE_ICONS                 = infos.getType<SpriteGroupType>("SPRITE_GROUP_RESOURCE_ICONS");
            SPRITE_GROUP_BLACKMARKET_ICONS              = infos.getType<SpriteGroupType>("SPRITE_GROUP_BLACKMARKET_ICONS");
            SPRITE_GROUP_SABOTAGE_ICONS                 = infos.getType<SpriteGroupType>("SPRITE_GROUP_SABOTAGE_ICONS");
            SPRITE_GROUP_PATENT_ICONS                   = infos.getType<SpriteGroupType>("SPRITE_GROUP_PATENT_ICONS");
            SPRITE_GROUP_ORDER_ICONS                    = infos.getType<SpriteGroupType>("SPRITE_GROUP_ORDER_ICONS");
            SPRITE_GROUP_GOVERNMENT_SUBSIDY_ICONS       = infos.getType<SpriteGroupType>("SPRITE_GROUP_GOVERNMENT_SUBSIDY_ICONS");
            SPRITE_GROUP_MODULE_ICONS                   = infos.getType<SpriteGroupType>("SPRITE_GROUP_MODULE_ICONS");
            SPRITE_GROUP_HQ_ICONS                       = infos.getType<SpriteGroupType>("SPRITE_GROUP_HQ_ICONS");
            SPRITE_GROUP_HQ_FOOTPRINT_ICONS             = infos.getType<SpriteGroupType>("SPRITE_GROUP_HQ_FOOTPRINT_ICONS");
            SPRITE_GROUP_TECHNOLOGY_LEVEL_ICONS         = infos.getType<SpriteGroupType>("SPRITE_GROUP_TECHNOLOGY_LEVEL_ICONS");
            SPRITE_GROUP_PERK_ICONS                     = infos.getType<SpriteGroupType>("SPRITE_GROUP_PERK_ICONS");
            SPRITE_GROUP_EVENT_STATE_ICONS              = infos.getType<SpriteGroupType>("SPRITE_GROUP_EVENT_STATE_ICONS");
            SPRITE_GROUP_CHARACTER_PORTRAITS            = infos.getType<SpriteGroupType>("SPRITE_GROUP_CHARACTER_PORTRAITS");
            SPRITE_GROUP_CHARACTER_THIN_PORTRAITS       = infos.getType<SpriteGroupType>("SPRITE_GROUP_CHARACTER_THIN_PORTRAITS");
            SPRITE_GROUP_CHARACTER_DIALOG_ART           = infos.getType<SpriteGroupType>("SPRITE_GROUP_CHARACTER_DIALOG_ART");
            SPRITE_GROUP_TERRAIN_TILE_ICONS             = infos.getType<SpriteGroupType>("SPRITE_GROUP_TERRAIN_TILE_ICONS");
            SPRITE_GROUP_ICE_TILE_ICONS                 = infos.getType<SpriteGroupType>("SPRITE_GROUP_ICE_TILE_ICONS");
            SPRITE_GROUP_ART_PACK_ICONS                 = infos.getType<SpriteGroupType>("SPRITE_GROUP_ART_PACK_ICONS");
            SPRITE_GROUP_INLINE_ICONS                   = infos.getType<SpriteGroupType>("SPRITE_GROUP_INLINE_ICONS");
        }
    }

    public interface IInfosListener
    {
        void OnInfosLoaded();
    }
    
    public class InfosListenerAdapter : IInfosListener
    {
        private Action onInfosLoadedAction;
        public InfosListenerAdapter(Action onInfosLoadedAction) { this.onInfosLoadedAction = onInfosLoadedAction; }
        public void OnInfosLoaded() { onInfosLoadedAction(); }
    }
}