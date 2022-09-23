using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using System.IO;
using Offworld.GameCore.Text;
using Offworld.SystemCore;

namespace Offworld.GameCore
{
    public class PlayerClient : TextHelpers
    {
        protected GameClient mGame = null;
        protected Infos mInfos = null;
        protected virtual GameClient gameClient()
        {
            return mGame;
        }
        protected virtual Infos infos()
        {
            return mInfos;
        }

        public enum PlayerDirtyType
        {
            FIRST,

            mzName,
            mzPseudonym,
            mzSuffix,
            mzRank,
            miMoney,
            miDebt,
            miExcessBond,
            miClaims,
            miEntertainment,
            miBaseSharePrice,
            miSharesAvailable,
            miColonySharesOwned,
            miStartTileID,
            miScans,
            miScanTime,
            miVisibleTiles,
            miManualSaleTime,
            miBlackMarketTime,
            miSabotagedCount,
            miEspionageShortageCount,
            miEspionageSurplusCount,
            miBondRatingChange,
            miDebtCut,
            miAutoPayDebtTarget,
            miEntertainmentModifier,
            miPowerConsumptionModifier,
            miConnectedHQPowerProductionModifier,
            miAdjacentHQSabotageModifier,
            miInterestModifier,
            miFinalMarketCap,
            miHighestBuyoutTenths,
            miNumCampaignLead,
            miNumCampaignTiePlayer,
            miScore,
            miImportsDone,
            miImportLimit,
            miNextImportTurn,
            mbHuman,
            mbConcede,
            mbDropped,
            mbSubsidiary,
            mbHQFounded,
            mbSkipAuction,
            mbBorehole,
            mbRecycleScrap,
            mbAdjacentMining,
            mbTeleportation,
            mbCaveMining,
            mbGeothermalDiscovered,
            mbAutoPayDebt,
            mbBeatSoren,
            mbIsSoren,
            mbBeatZultar,
            mbIsZultar,
            mbShouldCalculateCashResources,
            meHQ,
            meHQLevel,
            meHQClaimBonus,
            meBondRating,
            meEnergyResource,
            meGasResource,
            meLaunchResource,
            meGender,
            meColor,
            meBoughtByPlayer,
            maiResourceStockpile,
            maiResourceExtraStockpile,
            maiResourceExtraCapacity,
            maiResourceAutoTrade,
            maiResourceRate,
            maiResourceShortfall,
            maiResourceInput,
            maiColonyConsumption,
            maiColonyConsumptionModifier,
            maiColonyPayments,
            maiResourceOutput,
            maiResourceAutoPurchased,
            maiResourceAutoSold,
            maiResourceProductionModifier,
            maiUpgradeResourceCost,
            maiBlackMarketCount,
            maiSabotageCount,
            maiRealConstructionCount,
            maiBuildingCount,
            maiRealBuildingCount,
            maiBuildingClassInputModifier,
            maiBuildingClassLevel,
            maiHQUnlockCount,
            maiOrderCapacity,
            maiOrderCostModifier,
            maiOrderRateModifier,
            maiSharesOwned,
            maiStockDelay,
            maiBuyDelay,
            maiSellDelay,
            maiDividendTotal,
            maiPerkCount,
            mabPlayerOptions,
            mabAutoSupplyBuildingInput,
            mabBuildingsAlwaysOn,
            mabBuildingImmune,
            mabBuildingDestroyEvent,
            mabBuildingClassBoost,
            mabPatentStarted,
            mabPatentAcquiredLab,
            mabPatentAcquiredPerk,
            mabResourceNoBuy,
            mabResourceNoSell,
            mabBuildingFreeResource,
            mabAlternateGasResource,
            mabAlternatePowerResourcePatent,
            mabAlternatePowerResourcePerk,
            mabHoldResource,
            mabAutoLaunchResource,
            mabTeamShareResource,
            maePlayerList,
            maeResourceReplace,
            maeResourceLevelDiscovered,
            maeMinimumMining,
            maiFreeBuildings,
            maeTechnologyLevelResearching,
            maeTechnologyLevelDiscovered,
            maeActiveArtPacks,
            maaiUpgradeResourceCost,
            maaiBuildingResourceCost,
            maabResourceReplaceValid,
            maabIgnoreInputIce,
            mTileSet,
            mHQSet,
            mConstructionSet,
            mBuildingSet,
            mUnitSet,
            maaOrderInfos,

            NUM_TYPES
        }

        protected BitMaskMulti mDirtyBits = new BitMaskMulti((int)PlayerDirtyType.NUM_TYPES);
        protected virtual bool isDirty(PlayerDirtyType eType)
        {
            return mDirtyBits.GetBit((int)eType);
        }
        public virtual bool isAnyDirty()
        {
            return !(mDirtyBits.IsEmpty());
        }

        protected string mzName = "";
        protected string mzPseudonym = "";
        protected string mzSuffix = "";
        protected string mzRank = "";

        protected long miMoney = 0;
        protected int miDebt = 0;
        protected int miExcessBond = 0;
        protected int miClaims = 0;
        protected int miEntertainment = 0;
        protected int miBaseSharePrice = 0;
        protected int miSharesAvailable = 0;
        protected int miColonySharesOwned = 0;
        protected int miStartTileID = -1;
        protected int miScans = 0;
        protected int miScanTime = 0;
        protected int miVisibleTiles = 0;
        protected int miManualSaleTime = 0;
        protected int miBlackMarketTime = -1;
        protected int miSabotagedCount = 0;
        protected int miEspionageShortageCount = 0;
        protected int miEspionageSurplusCount = 0;
        protected int miBondRatingChange = 0;
        protected int miDebtCut = 0;
        protected int miAutoPayDebtTarget = 0;
        protected int miEntertainmentModifier = 0;
        protected int miPowerConsumptionModifier = 0;
        protected int miConnectedHQPowerProductionModifier = 0;
        protected int miAdjacentHQSabotageModifier = 0;
        protected int miInterestModifier = 0;
        protected int miFinalMarketCap = 0;
        protected int miHighestBuyoutTenths = 0;
        protected int miNumCampaignLead = 0;
        protected int miNumCampaignTiePlayer = 0;
        protected int miScore = 0;
        protected int miImportsDone = 0;
        protected int miImportLimit = -1;
        protected int miNextImportTurn = 0;

        protected long mlCashResourceValue = 0;

        protected bool mbHuman = false;
        protected bool mbEverHuman = false;
        protected bool mbConcede = false;
        protected bool mbDropped = false;
        protected bool mbSubsidiary = false;
        protected bool mbHQFounded = false;
        protected bool mbSkipAuction = false;
        protected bool mbBorehole = false;
        protected bool mbRecycleScrap = false;
        protected bool mbAdjacentMining = false;
        protected bool mbTeleportation = false;
        protected bool mbCaveMining = false;
        protected bool mbGeothermalDiscovered = false;
        protected bool mbAutoPayDebt = false;
        protected bool mbBeatSoren = false;
        protected bool mbIsSoren = false;
        protected bool mbBeatZultar = false;
        protected bool mbIsZultar = false;
        protected bool mbShouldCalculateCashResources = true;

        protected PlayerType mePlayer = PlayerType.NONE;
        protected TeamType meTeam = TeamType.NONE;
        protected PersonalityType mePersonality = PersonalityType.NONE;
        protected CorporationType meCorporation = CorporationType.NONE;
        protected HQType meHQ = HQType.NONE;
        protected HQLevelType meHQLevel = HQLevelType.ZERO;
        protected HQLevelType meHQClaimBonus = HQLevelType.NONE;
        protected BondType meBondRating = BondType.NONE;
        protected ResourceType meEnergyResource = ResourceType.NONE;
        protected ResourceType meGasResource = ResourceType.NONE;
        protected ResourceType meLaunchResource = ResourceType.NONE;
        protected HandicapType meHandicap = HandicapType.NONE;
        protected GenderType meGender = GenderType.MALE;
        protected PlayerColorType meColor = PlayerColorType.NONE;
        protected PlayerType meBoughtByPlayer = PlayerType.NONE;

        protected List<int> maiResourceStockpile = new List<int>();
        protected List<int> maiResourceExtraStockpile = new List<int>();
        protected List<int> maiResourceExtraCapacity = new List<int>();
        protected List<int> maiResourceAutoTrade = new List<int>();
        protected List<int> maiResourceRate = new List<int>();
        protected List<int> maiResourceShortfall = new List<int>();
        protected List<int> maiResourceInput = new List<int>();
        protected List<int> maiColonyConsumption = new List<int>();
        protected List<int> maiColonyConsumptionModifier = new List<int>();
        protected List<int> maiColonyPayments = new List<int>();
        protected List<int> maiResourceOutput = new List<int>();
        protected List<int> maiResourceAutoPurchased = new List<int>();
        protected List<int> maiResourceAutoSold = new List<int>();
        protected List<int> maiResourceProductionModifier = new List<int>();
        protected List<int> maiUpgradeResourceCost = new List<int>();
        protected List<int> maiBlackMarketCount = new List<int>();
        protected List<int> maiSabotageCount = new List<int>();
        protected List<int> maiRealConstructionCount = new List<int>();
        protected List<int> maiBuildingCount = new List<int>();
        protected List<int> maiRealBuildingCount = new List<int>();
        protected List<int> maiBuildingClassInputModifier = new List<int>();
        protected List<int> maiBuildingClassLevel = new List<int>();
        protected List<int> maiHQUnlockCount = new List<int>();
        protected List<int> maiOrderCapacity = new List<int>();
        protected List<int> maiOrderCostModifier = new List<int>();
        protected List<int> maiOrderRateModifier = new List<int>();
        protected List<int> maiSharesOwned = new List<int>();
        protected List<int> maiStockDelay = new List<int>();
        protected List<int> maiBuyDelay = new List<int>();
        protected List<int> maiSellDelay = new List<int>();
        protected List<int> maiDividendTotal = new List<int>();
        protected List<int> maiPerkCount = new List<int>();

        protected List<bool> mabPlayerOptions = new List<bool>();
        protected List<bool> mabAutoSupplyBuildingInput = new List<bool>();
        protected List<bool> mabBuildingsAlwaysOn = new List<bool>();
        protected List<bool> mabBuildingImmune = new List<bool>();
        protected List<bool> mabBuildingDestroyEvent = new List<bool>();
        protected List<bool> mabBuildingClassBoost = new List<bool>();
        protected List<bool> mabPatentStarted = new List<bool>();
        protected List<bool> mabPatentAcquiredLab = new List<bool>();
        protected List<bool> mabPatentAcquiredPerk = new List<bool>();
        protected List<bool> mabResourceNoBuy = new List<bool>();
        protected List<bool> mabResourceNoSell = new List<bool>();
        protected List<bool> mabBuildingFreeResource = new List<bool>();
        protected List<bool> mabAlternateGasResource = new List<bool>();
        protected List<bool> mabAlternatePowerResourcePatent = new List<bool>();
        protected List<bool> mabAlternatePowerResourcePerk = new List<bool>();
        protected List<bool> mabHoldResource = new List<bool>();
        protected List<bool> mabAutoLaunchResource = new List<bool>();
        protected List<bool> mabTeamShareResource = new List<bool>();

        protected List<PlayerType> maePlayerList = new List<PlayerType>();

        protected List<ResourceType> maeResourceReplace = new List<ResourceType>();

        protected List<ResourceLevelType> maeResourceLevelDiscovered = new List<ResourceLevelType>();
        protected List<ResourceLevelType> maeMinimumMining = new List<ResourceLevelType>();

        protected List<int> maiFreeBuildings = new List<int>();

        protected List<TechnologyLevelType> maeTechnologyLevelResearching = new List<TechnologyLevelType>();
        protected List<TechnologyLevelType> maeTechnologyLevelDiscovered = new List<TechnologyLevelType>();
        protected ArtPackList maeArtPackList = new ArtPackList();

        protected List<List<int>> maaiBuildingResourceCost = new List<List<int>>();

        protected List<List<bool>> maabResourceReplaceValid = new List<List<bool>>();
        protected List<List<bool>> maabIgnoreInputIce = new List<List<bool>>();

        protected HashSet<int> maiNearbyTiles = new HashSet<int>();
        protected HashSet<int> maiNearbyIceTiles = new HashSet<int>();
        protected List<int> maTiles = new List<int>();
        protected List<int> maHQs = new List<int>();
        protected List<int> maConstructions = new List<int>();
        protected List<int> maBuildings = new List<int>();
        protected List<int> maUnits = new List<int>();

        protected List<LinkedList<OrderInfo>> maaOrderInfos = new List<LinkedList<OrderInfo>>();

        public PlayerClient(GameClient pGame)
        {
            mGame = pGame;
            mInfos = pGame.infos();
        }

        protected virtual void SerializeClient(object stream, bool bAll, int compatibilityNumber)
        {
            SimplifyIO.Data(stream, ref mDirtyBits, "DirtyBits");

            if (isDirty(PlayerDirtyType.mzName) || bAll)
            {
                SimplifyIO.Data(stream, ref mzName, "Name");
            }
            if (isDirty(PlayerDirtyType.mzPseudonym) || bAll)
            {
                SimplifyIO.Data(stream, ref mzPseudonym, "Psuedonym");
            }
            if (isDirty(PlayerDirtyType.mzSuffix) || bAll)
            {
                SimplifyIO.Data(stream, ref mzSuffix, "Suffix");
            }
            if (isDirty(PlayerDirtyType.mzRank) || bAll)
            {
                SimplifyIO.Data(stream, ref mzRank, "Rank");
            }
            if (isDirty(PlayerDirtyType.miMoney) || bAll)
            {
                SimplifyIO.Data(stream, ref miMoney, "Money");
            }
            if (isDirty(PlayerDirtyType.miDebt) || bAll)
            {
                SimplifyIO.Data(stream, ref miDebt, "Debt");
            }
            if (isDirty(PlayerDirtyType.miExcessBond) || bAll)
            {
                SimplifyIO.Data(stream, ref miExcessBond, "ExcessBond");
            }
            if (isDirty(PlayerDirtyType.miClaims) || bAll)
            {
                SimplifyIO.Data(stream, ref miClaims, "Claims");
            }
            if (isDirty(PlayerDirtyType.miEntertainment) || bAll)
            {
                SimplifyIO.Data(stream, ref miEntertainment, "Entertainment");
            }
            if (isDirty(PlayerDirtyType.miBaseSharePrice) || bAll)
            {
                SimplifyIO.Data(stream, ref miBaseSharePrice, "BaseSharePrice");
            }
            if (isDirty(PlayerDirtyType.miSharesAvailable) || bAll)
            {
                SimplifyIO.Data(stream, ref miSharesAvailable, "SharesAvailable");
            }
            if (isDirty(PlayerDirtyType.miColonySharesOwned) || bAll)
            {
                SimplifyIO.Data(stream, ref miColonySharesOwned, "ColonySharesOwned");
            }
            if (isDirty(PlayerDirtyType.miStartTileID) || bAll)
            {
                SimplifyIO.Data(stream, ref miStartTileID, "miStartTileID");
            }
            if (isDirty(PlayerDirtyType.miScans) || bAll)
            {
                SimplifyIO.Data(stream, ref miScans, "Scans");
            }
            if (isDirty(PlayerDirtyType.miScanTime) || bAll)
            {
                SimplifyIO.Data(stream, ref miScanTime, "ScanTime");
            }
            if (isDirty(PlayerDirtyType.miVisibleTiles) || bAll)
            {
                SimplifyIO.Data(stream, ref miVisibleTiles, "VisibleTiles");
            }
            if (isDirty(PlayerDirtyType.miManualSaleTime) || bAll)
            {
                SimplifyIO.Data(stream, ref miManualSaleTime, "ManualSaleTime");
            }
            if (isDirty(PlayerDirtyType.miBlackMarketTime) || bAll)
            {
                SimplifyIO.Data(stream, ref miBlackMarketTime, "BlackMarketTime");
            }
            if (isDirty(PlayerDirtyType.miSabotagedCount) || bAll)
            {
                SimplifyIO.Data(stream, ref miSabotagedCount, "SabotagedCount");
            }
            if (isDirty(PlayerDirtyType.miEspionageShortageCount) || bAll)
            {
                SimplifyIO.Data(stream, ref miEspionageShortageCount, "EspionageShortageCount");
            }
            if (isDirty(PlayerDirtyType.miEspionageSurplusCount) || bAll)
            {
                SimplifyIO.Data(stream, ref miEspionageSurplusCount, "EspionageSurplusCount");
            }
            if (isDirty(PlayerDirtyType.miBondRatingChange) || bAll)
            {
                SimplifyIO.Data(stream, ref miBondRatingChange, "BondRatingChange");
            }
            if (isDirty(PlayerDirtyType.miDebtCut) || bAll)
            {
                SimplifyIO.Data(stream, ref miDebtCut, "DebtCut");
            }
            if (isDirty(PlayerDirtyType.miAutoPayDebtTarget) || bAll)
            {
                SimplifyIO.Data(stream, ref miAutoPayDebtTarget, "AutoPayDebtTarget");
            }
            if (isDirty(PlayerDirtyType.miEntertainmentModifier) || bAll)
            {
                SimplifyIO.Data(stream, ref miEntertainmentModifier, "EntertainmentModifier");
            }
            if (isDirty(PlayerDirtyType.miPowerConsumptionModifier) || bAll)
            {
                SimplifyIO.Data(stream, ref miPowerConsumptionModifier, "PowerConsumptionModifier");
            }
            if (isDirty(PlayerDirtyType.miConnectedHQPowerProductionModifier) || bAll)
            {
                SimplifyIO.Data(stream, ref miConnectedHQPowerProductionModifier, "ConnectedHQPowerProductionModifier");
            }
            if (isDirty(PlayerDirtyType.miAdjacentHQSabotageModifier) || bAll)
            {
                SimplifyIO.Data(stream, ref miAdjacentHQSabotageModifier, "AdjacentHQSabotageModifier");
            }
            if (isDirty(PlayerDirtyType.miInterestModifier) || bAll)
            {
                SimplifyIO.Data(stream, ref miInterestModifier, "InterestModifier");
            }
            if (isDirty(PlayerDirtyType.miFinalMarketCap) || bAll)
            {
                SimplifyIO.Data(stream, ref miFinalMarketCap, "FinalMarketCap");
            }
            if (isDirty(PlayerDirtyType.miHighestBuyoutTenths) || bAll)
            {
                if (compatibilityNumber >= 16)
                {
                    SimplifyIO.Data(stream, ref miHighestBuyoutTenths, "HighestBuyoutTenths");
                }
            }
            if (isDirty(PlayerDirtyType.miNumCampaignLead) || bAll)
            {
                SimplifyIO.Data(stream, ref miNumCampaignLead, "NumCampaignLeads");
            }
            if (isDirty(PlayerDirtyType.miNumCampaignTiePlayer) || bAll)
            {
                SimplifyIO.Data(stream, ref miNumCampaignTiePlayer, "NumCampaignTiePlayer");
            }
            if (isDirty(PlayerDirtyType.miScore) || bAll)
            {
                SimplifyIO.Data(stream, ref miScore, "Score");
            }
            if (isDirty(PlayerDirtyType.miImportsDone) || bAll)
            {
                SimplifyIO.Data(stream, ref miImportsDone, "ImportsDone");
            }
            if (isDirty(PlayerDirtyType.miImportLimit) || bAll)
            {
                SimplifyIO.Data(stream, ref miImportLimit, "ImportLimit");
            }
            if (isDirty(PlayerDirtyType.miNextImportTurn) || bAll)
            {
                SimplifyIO.Data(stream, ref miNextImportTurn, "ImportTurn");
            }

            if (isDirty(PlayerDirtyType.mbHuman) || bAll)
            {
                SimplifyIO.Data(stream, ref mbHuman, "Human");
            }
            if (isDirty(PlayerDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref mbEverHuman, "EverHuman");
            }
            if (isDirty(PlayerDirtyType.mbConcede) || bAll)
            {
                SimplifyIO.Data(stream, ref mbConcede, "Concede");
            }
            if (isDirty(PlayerDirtyType.mbDropped) || bAll)
            {
                SimplifyIO.Data(stream, ref mbDropped, "Dropped");
            }
            if (isDirty(PlayerDirtyType.mbSubsidiary) || bAll)
            {
                SimplifyIO.Data(stream, ref mbSubsidiary, "Alive");
            }
            if (isDirty(PlayerDirtyType.mbHQFounded) || bAll)
            {
                SimplifyIO.Data(stream, ref mbHQFounded, "HQFounded");
            }
            if (isDirty(PlayerDirtyType.mbSkipAuction) || bAll)
            {
                SimplifyIO.Data(stream, ref mbSkipAuction, "SkipAuction");
            }
            if (isDirty(PlayerDirtyType.mbBorehole) || bAll)
            {
                SimplifyIO.Data(stream, ref mbBorehole, "Borehole");
            }
            if (isDirty(PlayerDirtyType.mbRecycleScrap) || bAll)
            {
                SimplifyIO.Data(stream, ref mbRecycleScrap, "RecycleScrap");
            }
            if (isDirty(PlayerDirtyType.mbAdjacentMining) || bAll)
            {
                SimplifyIO.Data(stream, ref mbAdjacentMining, "AdjacentMining");
            }
            if (isDirty(PlayerDirtyType.mbTeleportation) || bAll)
            {
                SimplifyIO.Data(stream, ref mbTeleportation, "Teleportation");
            }
            if (isDirty(PlayerDirtyType.mbCaveMining) || bAll)
            {
                SimplifyIO.Data(stream, ref mbCaveMining, "CaveMining");
            }
            if (isDirty(PlayerDirtyType.mbGeothermalDiscovered) || bAll)
            {
                SimplifyIO.Data(stream, ref mbGeothermalDiscovered, "GeothermalDiscovered");
            }
            if (isDirty(PlayerDirtyType.mbAutoPayDebt) || bAll)
            {
                SimplifyIO.Data(stream, ref mbAutoPayDebt, "AutoPayDebt");
            }
            if (isDirty(PlayerDirtyType.mbBeatSoren) || bAll)
            {
                SimplifyIO.Data(stream, ref mbBeatSoren, "BeatSoren");
            }
            if (isDirty(PlayerDirtyType.mbIsSoren) || bAll)
            {
                SimplifyIO.Data(stream, ref mbIsSoren, "IsSoren");
            }
            if (isDirty(PlayerDirtyType.mbBeatZultar) || bAll)
            {
                SimplifyIO.Data(stream, ref mbBeatZultar, "BeatZultar");
            }
            if (isDirty(PlayerDirtyType.mbIsZultar) || bAll)
            {
                SimplifyIO.Data(stream, ref mbIsZultar, "IsZultar");
            }
            if(isDirty(PlayerDirtyType.mbShouldCalculateCashResources) || bAll)
            {
                SimplifyIO.Data(stream, ref mbShouldCalculateCashResources, "shouldUpdateCashResources");
            }

            if (isDirty(PlayerDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref mePlayer, "Player");
            }
            if (isDirty(PlayerDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref meTeam, "Player");
            }
            if (isDirty(PlayerDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref mePersonality, "Personality");
            }
            if (isDirty(PlayerDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref meCorporation, "Corporation");
            }
            if (isDirty(PlayerDirtyType.meHQ) || bAll)
            {
                SimplifyIO.Data(stream, ref meHQ, "HQ");
            }
            if (isDirty(PlayerDirtyType.meHQLevel) || bAll)
            {
                SimplifyIO.Data(stream, ref meHQLevel, "Level");
            }
            if (isDirty(PlayerDirtyType.meHQClaimBonus) || bAll)
            {
                SimplifyIO.Data(stream, ref meHQClaimBonus, "HQClaimBonus");
            }
            if (isDirty(PlayerDirtyType.meBondRating) || bAll)
            {
                SimplifyIO.Data(stream, ref meBondRating, "BondRating");
            }
            if (isDirty(PlayerDirtyType.meEnergyResource) || bAll)
            {
                SimplifyIO.Data(stream, ref meEnergyResource, "EnergyResource");
            }
            if (isDirty(PlayerDirtyType.meGasResource) || bAll)
            {
                SimplifyIO.Data(stream, ref meGasResource, "GasResource");
            }
            if (isDirty(PlayerDirtyType.meLaunchResource) || bAll)
            {
                SimplifyIO.Data(stream, ref meLaunchResource, "LaunchResource");
            }
            if (isDirty(PlayerDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref meHandicap, "Handicap");
            }
            if (isDirty(PlayerDirtyType.meGender) || bAll)
            {
                SimplifyIO.Data(stream, ref meGender, "Gender");
            }
            if (isDirty(PlayerDirtyType.meColor) || bAll)
            {
                SimplifyIO.Data(stream, ref meColor, "Color");
            }
            if (isDirty(PlayerDirtyType.meBoughtByPlayer) || bAll)
            {
                SimplifyIO.Data(stream, ref meBoughtByPlayer, "BoughtByPlayer");
            }

            if (isDirty(PlayerDirtyType.maiResourceStockpile) || bAll)
            {
                SimplifyIO.Data(stream, ref maiResourceStockpile, (int)infos().resourcesNum(), "ResourceStockpile_");
            }
            if (isDirty(PlayerDirtyType.maiResourceExtraStockpile) || bAll)
            {
                SimplifyIO.Data(stream, ref maiResourceExtraStockpile, (int)infos().resourcesNum(), "ResourceExtraStockpile_");
            }
            if (isDirty(PlayerDirtyType.maiResourceExtraCapacity) || bAll)
            {
                SimplifyIO.Data(stream, ref maiResourceExtraCapacity, (int)infos().resourcesNum(), "ResourceExtraCapacity_");
            }
            if (isDirty(PlayerDirtyType.maiResourceAutoTrade) || bAll)
            {
                SimplifyIO.Data(stream, ref maiResourceAutoTrade, (int)infos().resourcesNum(), "ResourceAutoSell_");
            }
            if (isDirty(PlayerDirtyType.maiResourceRate) || bAll)
            {
                SimplifyIO.Data(stream, ref maiResourceRate, (int)infos().resourcesNum(), "ResourceRate_");
            }
            if (isDirty(PlayerDirtyType.maiResourceShortfall) || bAll)
            {
                SimplifyIO.Data(stream, ref maiResourceShortfall, (int)infos().resourcesNum(), "ResourceShortfall_");
            }
            if (isDirty(PlayerDirtyType.maiResourceInput) || bAll)
            {
                SimplifyIO.Data(stream, ref maiResourceInput, (int)infos().resourcesNum(), "ResourceInput_");
            }
            if (isDirty(PlayerDirtyType.maiResourceOutput) || bAll)
            {
                SimplifyIO.Data(stream, ref maiResourceOutput, (int)infos().resourcesNum(), "ResourceOutput_");
            }
            if (isDirty(PlayerDirtyType.maiResourceAutoPurchased) || bAll)
            {
                SimplifyIO.Data(stream, ref maiResourceAutoPurchased, (int)infos().resourcesNum(), "ResourceAutoPurchased_");
            }
            if (isDirty(PlayerDirtyType.maiResourceAutoSold) || bAll)
            {
                SimplifyIO.Data(stream, ref maiResourceAutoSold, (int)infos().resourcesNum(), "ResourceAutoSold_");
            }
            if (isDirty(PlayerDirtyType.maiResourceProductionModifier) || bAll)
            {
                SimplifyIO.Data(stream, ref maiResourceProductionModifier, (int)infos().resourcesNum(), "ResourceProductionModifier_");
            }
            if (isDirty(PlayerDirtyType.maiUpgradeResourceCost) || bAll)
            {
                SimplifyIO.Data(stream, ref maiUpgradeResourceCost, "UpgradeResourceCost_");
            }
            if (isDirty(PlayerDirtyType.maiBlackMarketCount) || bAll)
            {
                SimplifyIO.Data(stream, ref maiBlackMarketCount, (int)infos().blackMarketsNum(), "BlackMarketCount_");
            }
            if (isDirty(PlayerDirtyType.maiSabotageCount) || bAll)
            {
                SimplifyIO.Data(stream, ref maiSabotageCount, (int)infos().sabotagesNum(), "SabotageCount_");
            }
            if (isDirty(PlayerDirtyType.maiRealConstructionCount) || bAll)
            {
                SimplifyIO.Data(stream, ref maiRealConstructionCount, (int)infos().buildingsNum(), "RealConstructionCount_");
            }
            if (isDirty(PlayerDirtyType.maiBuildingCount) || bAll)
            {
                SimplifyIO.Data(stream, ref maiBuildingCount, (int)infos().buildingsNum(), "BuildingCount_");
            }
            if (isDirty(PlayerDirtyType.maiRealBuildingCount) || bAll)
            {
                SimplifyIO.Data(stream, ref maiRealBuildingCount, (int)infos().buildingsNum(), "RealBuildingCount_");
            }
            if (isDirty(PlayerDirtyType.maiBuildingClassInputModifier) || bAll)
            {
                SimplifyIO.Data(stream, ref maiBuildingClassInputModifier, (int)infos().buildingClassesNum(), "BuildingClassInputModifier_");
            }
            if (isDirty(PlayerDirtyType.maiBuildingClassLevel) || bAll)
            {
                SimplifyIO.Data(stream, ref maiBuildingClassLevel, (int)infos().buildingClassesNum(), "BuildingClassLevel_");
            }
            if (isDirty(PlayerDirtyType.maiHQUnlockCount) || bAll)
            {
                SimplifyIO.Data(stream, ref maiHQUnlockCount, (int)infos().HQsNum(), "HQUnlock_");
            }
            if (isDirty(PlayerDirtyType.maiOrderCapacity) || bAll)
            {
                SimplifyIO.Data(stream, ref maiOrderCapacity, (int)infos().ordersNum(), "OrderCapacity_");
            }
            if (isDirty(PlayerDirtyType.maiOrderCostModifier) || bAll)
            {
                SimplifyIO.Data(stream, ref maiOrderCostModifier, (int)infos().ordersNum(), "OrderCostModifier_");
            }
            if (isDirty(PlayerDirtyType.maiOrderRateModifier) || bAll)
            {
                SimplifyIO.Data(stream, ref maiOrderRateModifier, (int)infos().ordersNum(), "OrderRateModifier_");
            }
            if (isDirty(PlayerDirtyType.maiSharesOwned) || bAll)
            {
                SimplifyIO.Data(stream, ref maiSharesOwned, (int)gameClient().getNumPlayers(), "SharesOwned_");
            }
            if (isDirty(PlayerDirtyType.maiStockDelay) || bAll)
            {
                SimplifyIO.Data(stream, ref maiStockDelay, (int)gameClient().getNumPlayers(), "StockDelay_");
            }
            if (isDirty(PlayerDirtyType.maiBuyDelay) || bAll)
            {
                SimplifyIO.Data(stream, ref maiBuyDelay, (int)gameClient().getNumPlayers(), "BuyDelay_");
            }
            if (isDirty(PlayerDirtyType.maiSellDelay) || bAll)
            {
                SimplifyIO.Data(stream, ref maiSellDelay, (int)gameClient().getNumPlayers(), "SellDelay_");
            }
            if (isDirty(PlayerDirtyType.maiDividendTotal) || bAll)
            {
                SimplifyIO.Data(stream, ref maiDividendTotal, (int)gameClient().getNumPlayers(), "DividendTotal_");
            }
            if (isDirty(PlayerDirtyType.maiPerkCount) || bAll)
            {
                SimplifyIO.Data(stream, ref maiPerkCount, (int)infos().perksNum(), "PerkCount_");
            }
            if (isDirty(PlayerDirtyType.maiColonyConsumption) || bAll)
            {
                SimplifyIO.Data(stream, ref maiColonyConsumption, (int)infos().resourcesNum(), "ColonyConsumption_");
            }
            if (isDirty(PlayerDirtyType.maiColonyConsumptionModifier) || bAll)
            {
                SimplifyIO.Data(stream, ref maiColonyConsumptionModifier, (int)infos().resourcesNum(), "ColonyConsumptionModifier_");
            }
            if (isDirty(PlayerDirtyType.maiColonyPayments) || bAll)
            {
                SimplifyIO.Data(stream, ref maiColonyPayments, (int)infos().resourcesNum(), "ColonyPayments_");
            }

            if (isDirty(PlayerDirtyType.mabPlayerOptions) || bAll)
            {
                SimplifyIO.Data(stream, ref mabPlayerOptions, (int)PlayerOptionType.NUM_TYPES, "PlayerOptions_");
            }
            if (isDirty(PlayerDirtyType.mabAutoSupplyBuildingInput) || bAll)
            {
                SimplifyIO.Data(stream, ref mabAutoSupplyBuildingInput, (int)infos().buildingsNum(), "AutoSupplyBuildingInput_");
            }
            if (isDirty(PlayerDirtyType.mabBuildingsAlwaysOn) || bAll)
            {
                SimplifyIO.Data(stream, ref mabBuildingsAlwaysOn, (int)infos().buildingsNum(), "BuildingsAlwaysOn_");
            }
            if (isDirty(PlayerDirtyType.mabBuildingImmune) || bAll)
            {
                SimplifyIO.Data(stream, ref mabBuildingImmune, (int)infos().buildingsNum(), "BuildingImmune_");
            }
            if (isDirty(PlayerDirtyType.mabBuildingDestroyEvent) || bAll)
            {
                SimplifyIO.Data(stream, ref mabBuildingDestroyEvent, (int)infos().buildingsNum(), "BuildingDestroyEvent_");
            }
            if (isDirty(PlayerDirtyType.mabBuildingClassBoost) || bAll)
            {
                SimplifyIO.Data(stream, ref mabBuildingClassBoost, (int)infos().buildingClassesNum(), "BuildingClassBoost_");
            }
            if (isDirty(PlayerDirtyType.mabPatentStarted) || bAll)
            {
                SimplifyIO.Data(stream, ref mabPatentStarted, (int)infos().patentsNum(), "PatentStarted_");
            }
            if (isDirty(PlayerDirtyType.mabPatentAcquiredLab) || bAll)
            {
                SimplifyIO.Data(stream, ref mabPatentAcquiredLab, (int)infos().patentsNum(), "PatentAcquiredLab_");
            }
            if (isDirty(PlayerDirtyType.mabPatentAcquiredPerk) || bAll)
            {
                SimplifyIO.Data(stream, ref mabPatentAcquiredPerk, (int)infos().patentsNum(), "PatentAcquired_");
            }
            if (isDirty(PlayerDirtyType.mabResourceNoBuy) || bAll)
            {
                SimplifyIO.Data(stream, ref mabResourceNoBuy, (int)(infos().resourcesNum()), "ResourceNoBuy_");
            }
            if (isDirty(PlayerDirtyType.mabResourceNoSell) || bAll)
            {
                SimplifyIO.Data(stream, ref mabResourceNoSell, (int)(infos().resourcesNum()), "ResourceNoSell_");
            }
            if (isDirty(PlayerDirtyType.mabBuildingFreeResource) || bAll)
            {
                SimplifyIO.Data(stream, ref mabBuildingFreeResource, (int)infos().resourcesNum(), "BuildingFreeResource_");
            }
            if (isDirty(PlayerDirtyType.mabAlternateGasResource) || bAll)
            {
                SimplifyIO.Data(stream, ref mabAlternateGasResource, (int)infos().resourcesNum(), "AlternateGasResource_");
            }
            if (isDirty(PlayerDirtyType.mabAlternatePowerResourcePatent) || bAll)
            {
                SimplifyIO.Data(stream, ref mabAlternatePowerResourcePatent, (int)infos().resourcesNum(), "AlternatePowerResourcePatent_");
            }
            if (isDirty(PlayerDirtyType.mabAlternatePowerResourcePerk) || bAll)
            {
                SimplifyIO.Data(stream, ref mabAlternatePowerResourcePerk, (int)infos().resourcesNum(), "AlternatePowerResourcePerk_");
            }
            if (isDirty(PlayerDirtyType.mabHoldResource) || bAll)
            {
                SimplifyIO.Data(stream, ref mabHoldResource, (int)infos().resourcesNum(), "HoldResource_");
            }
            if (isDirty(PlayerDirtyType.mabAutoLaunchResource) || bAll)
            {
                SimplifyIO.Data(stream, ref mabAutoLaunchResource, (int)infos().resourcesNum(), "AutoLaunchResource_");
            }
            if (isDirty(PlayerDirtyType.mabTeamShareResource) || bAll)
            {
                SimplifyIO.Data(stream, ref mabTeamShareResource, (int)infos().resourcesNum(), "TeamShareResource_");
            }

            if (isDirty(PlayerDirtyType.maePlayerList) || bAll)
            {
                SimplifyIO.Data(stream, ref maePlayerList, (int)gameClient().getNumPlayers(), "PlayerList_");
            }
            if (isDirty(PlayerDirtyType.maeResourceReplace) || bAll)
            {
                SimplifyIO.Data(stream, ref maeResourceReplace, (int)infos().resourcesNum(), "ResourceReplace_");
            }
            if (isDirty(PlayerDirtyType.maeResourceLevelDiscovered) || bAll)
            {
                SimplifyIO.Data(stream, ref maeResourceLevelDiscovered, (int)infos().resourcesNum(), "ResourcesDiscovered_");
            }
            if (isDirty(PlayerDirtyType.maeMinimumMining) || bAll)
            {
                SimplifyIO.Data(stream, ref maeMinimumMining, (int)infos().buildingClassesNum(), "MinimumMining_");
            }
            if (isDirty(PlayerDirtyType.maiFreeBuildings) || bAll)
            {
                SimplifyIO.Data(stream, ref maiFreeBuildings, (int)infos().buildingsNum(), "FreeBuildings_");
            }
            if (isDirty(PlayerDirtyType.maeTechnologyLevelResearching) || bAll)
            {
                SimplifyIO.Data(stream, ref maeTechnologyLevelResearching, (int)infos().technologiesNum(), "TechnologyLevelResearching_");
            }
            if (isDirty(PlayerDirtyType.maeTechnologyLevelDiscovered) || bAll)
            {
                SimplifyIO.Data(stream, ref maeTechnologyLevelDiscovered, (int)infos().technologiesNum(), "TechnologyLevelDiscovered_");
            }
            if (isDirty(PlayerDirtyType.maeActiveArtPacks) || bAll)
            {
                ArtPackList.Serialize(stream, ref maeArtPackList);
            }

            if (isDirty(PlayerDirtyType.maaiBuildingResourceCost) || bAll)
            {
                SimplifyIO.Data(stream, ref maaiBuildingResourceCost, "BuildingResourceCost_");
            }

            if (isDirty(PlayerDirtyType.maabResourceReplaceValid) || bAll)
            {
                SimplifyIO.Data(stream, ref maabResourceReplaceValid, "ResourceReplaceValid_");
            }
            if (isDirty(PlayerDirtyType.maabIgnoreInputIce) || bAll)
            {
                SimplifyIO.Data(stream, ref maabIgnoreInputIce, "IgnoreInputIce_");
            }

            if (isDirty(PlayerDirtyType.mTileSet) || bAll)
            {
                SimplifyIO.Data(stream, ref maTiles, "TileSet");
            }
            if (isDirty(PlayerDirtyType.mHQSet) || bAll)
            {
                SimplifyIO.Data(stream, ref maHQs, "HQSet");
            }
            if (isDirty(PlayerDirtyType.mConstructionSet) || bAll)
            {
                SimplifyIO.Data(stream, ref maConstructions, "ConstructionSet");
            }
            if (isDirty(PlayerDirtyType.mBuildingSet) || bAll)
            {
                SimplifyIO.Data(stream, ref maBuildings, "BuildingSet");
            }
            if (isDirty(PlayerDirtyType.mUnitSet) || bAll)
            {
                SimplifyIO.Data(stream, ref maUnits, "UnitSet");
            }

            if (isDirty(PlayerDirtyType.maaOrderInfos) || bAll)
            {
                SimplifyIOGame.OrderData(stream, ref maaOrderInfos, infos());
            }
        }

        public virtual void writeClientValues(BinaryWriter stream, bool bAll, int compatibilityNumber)
        {
            SerializeClient(stream, bAll, compatibilityNumber);
        }

        public virtual void readClientValues(BinaryReader stream, bool bAll, int compatibilityNumber)
        {
            SerializeClient(stream, bAll, compatibilityNumber);
        }

        public virtual int getStartingShares(bool bIncludeHandicap)
        {
            if (gameClient().isSevenSols())
            {
                return 0;
            }
            else
            {
                int iShares = 0;

                if (getHQ() != HQType.NONE)
                {
                    iShares += infos().HQ(getHQ()).miShares;
                }

                if (bIncludeHandicap)
                {
                    iShares += infos().handicap(getHandicap()).miShares;
                }

                if ((int)(gameClient().getNumPlayers()) > (Constants.MAX_NUM_PLAYERS / 2))
                {
                    iShares += 3;
                }
                else
                {
                    iShares += 2;
                }

                if (gameClient().isCampaign())
                {
                    for (ColonyType eLoopColony = 0; eLoopColony < Globals.Infos.coloniesNum(); eLoopColony++)
                    {
                        ColonyBonusLevelType eColonyBonusLevel = corporation().maeColonyBonusLevel[(int)eLoopColony];

                        if (eColonyBonusLevel != ColonyBonusLevelType.NONE)
                        {
                            iShares += infos().colonyBonus(infos().colony(eLoopColony).maeColonyBonus[(int)eColonyBonusLevel]).miStartingShares;
                        }
                    }
                }

                return Math.Max(0, iShares);
            }
        }

        public virtual int getDividend(long iMoney, int iShares)
        {
            return (int)((Math.Max(0, (iMoney - infos().Globals.SUBSIDIARY_MIN_MONEY)) * iShares) / (gameClient().getInitialShares() * 100));
        }

        public virtual int getBuySharePriceBase(PlayerType ePlayer, int iSharesAvailableChange = 0)
        {
            return ((gameClient().playerClient(ePlayer).getSharePrice(iSharesAvailableChange) * infos().Globals.SHARE_PURCHASE_SIZE) / Constants.STOCK_MULTIPLIER);
        }

        public virtual int getBuySharePrice(PlayerType ePlayer, int iSharesAvailableChange = 0)
        {
            int iPrice = getBuySharePriceBase(ePlayer, iSharesAvailableChange);

            if (gameClient().playerClient(ePlayer).getSharesAvailable() + iSharesAvailableChange == 0)
            {
                iPrice *= 2;
            }

            return iPrice;
        }

        public virtual int getSellSharePrice(PlayerType ePlayer)
        {
            PlayerClient pPlayer = gameClient().playerClient(ePlayer);
            int iPrice = pPlayer.getSharePrice(1);

            iPrice = ((iPrice * infos().Globals.SHARE_PURCHASE_SIZE) / Constants.STOCK_MULTIPLIER);

            iPrice *= 9;
            iPrice /= 10;

            return iPrice;
        }

        protected virtual int getStockBuyoutPrice(PlayerType ePlayer, PlayerType eOwningPlayer)
        {
            int iPrice = (getBuySharePriceBase(ePlayer) * gameClient().playerClient(eOwningPlayer).getSharesOwned(ePlayer));

            if (gameClient().playerClient(ePlayer).getTeam() == gameClient().playerClient(eOwningPlayer).getTeam())
            {
                iPrice *= 2;
            }

            return iPrice;
        }

        public virtual int getTotalBuyoutPrice(PlayerType ePlayer)
        {
            int iPrice = 0;

            for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameClient().getNumPlayers(); eLoopPlayer++)
            {
                PlayerClient pLoopPlayer = gameClient().playerClient(eLoopPlayer);

                if (pLoopPlayer.getTeam() != getTeam())
                {
                    iPrice += getStockBuyoutPrice(ePlayer, eLoopPlayer);
                }
            }

            iPrice += (getBuySharePriceBase(ePlayer) * gameClient().playerClient(ePlayer).getSharesAvailable() * 2);

            return iPrice;
        }

        public virtual int getBuyoutPercent(PlayerType ePlayer)
        {
            if (infos().rulesSet(gameClient().getRulesSet()).mbNoStockMarket)
            {
                return 0;
            }

            if (gameClient().isSevenSols())
            {
                return 0;
            }

            if (isSubsidiary())
            {
                return 0;
            }

            PlayerClient pPlayer = gameClient().playerClient(ePlayer);

            if (getTeam() == pPlayer.getTeam())
            {
                return 0;
            }

            if (pPlayer.isSubsidiary())
            {
                return 0;
            }

            long iCash = 0;

            for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameClient().getNumPlayers(); eLoopPlayer++)
            {
                PlayerClient pLoopPlayer = gameClient().playerClient(eLoopPlayer);

                if (!(pLoopPlayer.isSubsidiary()))
                {
                    if (pLoopPlayer.getTeam() == getTeam())
                    {
                        iCash += pLoopPlayer.getCashResourceValue();
                    }
                }
            }

            {
                int iPrice = 0;
                int iCount = 0;

                if ((pPlayer.getSharesBoughtRivals() + pPlayer.getSharesAvailable()) > gameClient().getMajorityShares())
                {
                    while ((pPlayer.getSharesBoughtRivals() + iCount) <= gameClient().getMajorityShares())
                    {
                        iPrice += getBuySharePrice(ePlayer, -(iCount));
                        iCount++;
                    }

                    if (iPrice > 0)
                    {
                        return (int)Math.Min(100, ((iCash * 100) / iPrice));
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            {
                int iPrice = getTotalBuyoutPrice(ePlayer);

                if (iPrice > 0)
                {
                    return (int)Math.Min(100, ((iCash * 100) / iPrice));
                }
                else
                {
                    return 0;
                }
            }
        }

        public virtual int getHighestStockDelay(PlayerType ePlayer, bool bBuying)
        {
            int iDelay = getStockDelay(ePlayer);

            if (bBuying)
            {
                iDelay = Math.Max(iDelay, getBuyDelay(ePlayer));
            }
            else
            {
                iDelay = Math.Max(iDelay, getSellDelay(ePlayer));
            }

            return iDelay;
        }
        public virtual bool isSharesInCooldown(PlayerType ePlayer, bool bBuying)
        {
            if (getHighestStockDelay(ePlayer, bBuying) > gameClient().getTurnCount())
            {
                return true;
            }

            return false;
        }

        public virtual bool canBuyout(PlayerType ePlayer, bool bTestMoney)
        {
            if (gameClient().isSevenSols())
            {
                return false;
            }

            if (infos().rulesSet(gameClient().getRulesSet()).mbNoStockMarket)
            {
                return false;
            }

            if (isSubsidiary())
            {
                return false;
            }

            PlayerClient pPlayer = gameClient().playerClient(ePlayer);

            if (pPlayer.isSubsidiary())
            {
                return false;
            }

            if (getTeam() == pPlayer.getTeam())
            {
                return false;
            }

            if (isSharesInCooldown(ePlayer, true))
            {
                return false;
            }

            if (gameClient().isGameOption(GameOptionType.AI_BUYOUT))
            {
                if (getTeam() != pPlayer.getTeam())
                {
                    if (wasEverHuman() && pPlayer.wasEverHuman())
                    {
                        if (gameClient().countOriginalAIAlive() > 0)
                        {
                            return false;
                        }
                    }
                }
            }

            if (bTestMoney)
            {
                if (getMoney(true) < getTotalBuyoutPrice(ePlayer))
                {
                    return false;
                }
            }

            return true;
        }

        public virtual bool canOnlyBuyout(PlayerType ePlayer)
        {
            PlayerClient pPlayer = gameClient().playerClient(ePlayer);

            if (pPlayer.getSharesAvailable() > 0)
            {
                return false;
            }

            if (getTeam() == pPlayer.getTeam())
            {
                return false;
            }

            if (pPlayer.countOwnSharesOwned() > gameClient().getMajorityShares())
            {
                return false;
            }

            return true;
        }

        public virtual bool canBuyShares(PlayerType ePlayer, bool bTestMoney)
        {
            if (gameClient().isSevenSols())
            {
                return false;
            }

            if (infos().rulesSet(gameClient().getRulesSet()).mbNoStockMarket)
            {
                return false;
            }

            if (canOnlyBuyout(ePlayer))
            {
                return false;
            }

            if (isSubsidiary())
            {
                return false;
            }

            PlayerClient pPlayer = gameClient().playerClient(ePlayer);

            if (pPlayer.getSharesAvailable() == 0)
            {
                if (pPlayer.isSubsidiary())
                {
                    return false;
                }

                if (getTeam() == pPlayer.getTeam())
                {
                    return false;
                }
            }

            if (isSharesInCooldown(ePlayer, true))
            {
                return false;
            }

            if (bTestMoney)
            {
                if (getMoney() < getBuySharePrice(ePlayer))
                {
                    return false;
                }
            }

            if (gameClient().isGameOption(GameOptionType.AI_BUYOUT))
            {
                if (getTeam() != pPlayer.getTeam())
                {
                    if (wasEverHuman() && pPlayer.wasEverHuman())
                    {
                        if (gameClient().countOriginalAIAlive() > 0)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public virtual bool canSellShares(PlayerType ePlayer)
        {
            if (gameClient().isSevenSols())
            {
                return false;
            }

            if (infos().rulesSet(gameClient().getRulesSet()).mbNoStockMarket)
            {
                return false;
            }

            PlayerClient pPlayer = gameClient().playerClient(ePlayer);

            if (pPlayer.isSubsidiary())
            {
                return false;
            }

            if (getSharesOwned(ePlayer) == 0)
            {
                return false;
            }

            if (ePlayer == getPlayer())
            {
                if (getSharesOwned(ePlayer) <= getStartingShares(true))
                {
                    return false;
                }
            }

            if (isSharesInCooldown(ePlayer, false))
            {
                return false;
            }

            return true;
        }

        public virtual bool canBuyColonyShares(bool bTestMoney)
        {
            if (!(gameClient().isSevenSols()))
            {
                return false;
            }

            if (gameClient().getSevenSols() == SevenSolsType.COLONY)
            {
                if ((gameClient().getMaxPopulation() < gameClient().getColonyCap()) ||
                    (gameClient().getLabor() < gameClient().getColonyCap()))
                {
                    return false;
                }
            }
            else if (gameClient().getSevenSols() == SevenSolsType.WHOLESALE)
            {
                return false;
            }

            if (bTestMoney)
            {
                if (getMoney() < gameClient().getSharePrice())
                {
                    return false;
                }
            }

            return true;
        }

        public virtual int getModuleMoneyCost(ModuleType eModule, bool bIgnoreStockpile = false)
        {
            int iCost = gameClient().getSharePrice();

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                if (!gameClient().isResourceValid(eLoopResource))
                    continue;
                if (bIgnoreStockpile)
                {
                    iCost += getIgnoreStockpileCost(eLoopResource, infos().module(eModule).maiResourceCost[(int)eLoopResource]);
                }
                else
                {
                    iCost += getNeededResourceCost(eLoopResource, infos().module(eModule).maiResourceCost[(int)eLoopResource]);
                }
            }

            return iCost;
        }

        public virtual bool isAnyRequiredResourceNoBuy(List<int> aiResourceRequirements)
        {
            for(ResourceType eLoopResource = ~ResourceType.NONE; eLoopResource < infos().resourcesNum(); ++eLoopResource)
            {
                if (!gameClient().isResourceValid(eLoopResource))
                    continue;
                if (aiResourceRequirements[(int)eLoopResource] > getWholeResourceStockpile(eLoopResource, true) && isResourceNoBuy(eLoopResource))
                    return true;
            }
            return false;
        }

        public virtual bool isRequiredResourceNoBuy(ResourceType eResource, int iQuantity)
        {
            if (isResourceNoBuy(eResource) && iQuantity > getWholeResourceStockpile(eResource, true))
                return true;
            return false;
        }

        public virtual bool canBuyColonyModule(ModuleType eModule, bool bTestMoney)
        {
            if (gameClient().getSevenSols() != SevenSolsType.COLONY)
            {
                return false;
            }

            if (infos().rulesSet(gameClient().getRulesSet()).mbNoColony)
            {
                return false;
            }

            if (!(gameClient().canSpreadModule(eModule, true)))
            {
                return false;
            }

            if (bTestMoney)
            {
                int iCost = getModuleMoneyCost(eModule);

                if (isHuman() && (infos().rulesSet(gameClient().getRulesSet()).mbNoAutoBuy || isAnyRequiredResourceNoBuy(infos().module(eModule).maiResourceCost)))
                {
                    if (iCost > 0)
                    {
                        return false;
                    }
                }
                else
                {
                    if (iCost > getMoney())
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public virtual int getWholesaleMoneyCost(int iSlot, bool bIgnoreStockpile = false)
        {
            int iCost = gameClient().getSharePrice();

            if (bIgnoreStockpile)
            {
                iCost += getIgnoreStockpileCost(gameClient().getWholesaleSlotResource(iSlot), gameClient().getWholesaleSlotShipment(iSlot));
            }
            else
            {
                iCost += getNeededResourceCost(gameClient().getWholesaleSlotResource(iSlot), gameClient().getWholesaleSlotShipment(iSlot));
            }

            return iCost;
        }

        public virtual bool canSupplyWholesale(int iSlot, bool bTestMoney)
        {
            if (gameClient().getSevenSols() != SevenSolsType.WHOLESALE)
            {
                return false;
            }

            if (infos().rulesSet(gameClient().getRulesSet()).mbNoColony)
            {
                return false;
            }

            if (bTestMoney)
            {
                int iCost = getWholesaleMoneyCost(iSlot);

                if (isHuman() && (infos().rulesSet(gameClient().getRulesSet()).mbNoAutoBuy || isResourceNoBuy(gameClient().getWholesaleSlotResource(iSlot))))
                {
                    if (iCost > 0)
                    {
                        return false;
                    }
                }
                else
                {
                    if (iCost > getMoney())
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public virtual bool canImportAny(bool bTestMoney)
        {
            if (gameClient().getNumImportSlots() == 0)
                return false;
            return Enumerable.Range(0, gameClient().getNumImportSlots()).Any(x => canImport(x, bTestMoney));
        }

        public virtual bool canImport(int iSlot, bool bTestMoney)
        {
            if (infos().rulesSet(gameClient().getRulesSet()).mabimportUnavailablePlayer[(int)gameClient().getImportSlotResource(iSlot)])
            {
                return false;
            }
            if (miImportLimit != -1 && miImportLimit <= getImportsDone())
            {
                return false;
            }
            if (bTestMoney)
            {
                if (isImportInCooldown())
                    return false;
                int iCost = getImportResourceCost(iSlot, false);
                if (iCost > 0 && isResourceNoBuy(gameClient().getImportResourcePayment(iSlot)))
                    return false;
                return iCost + getImportMoneyCost(iSlot) <= getMoney();
            }
            return true;
        }

        public int getImportsDone()
        {
            return miImportsDone;
        }

        public int getImportLimit()
        {
            return miImportLimit;
        }

        public int getImportCooldown()
        {
            return miNextImportTurn - gameClient().getTurnCount();
        }

        public bool isImportInCooldown()
        {
            return gameClient().getTurnCount() <= miNextImportTurn;
        }

        public int getImportMoneyCost(int iSlot)
        {
            return 400 * (miImportsDone + 2) * 40 / infos().resource(gameClient().getImportSlotResource(iSlot)).miImportValue;
        }

        public int getImportResourceCost(int iSlot, bool bIgnoreStockpile)
        {
            if (bIgnoreStockpile)
                return getIgnoreStockpileCost(gameClient().getImportResourcePayment(iSlot), mGame.getImportSlotCost(iSlot));
            else
                return getNeededResourceCost(gameClient().getImportResourcePayment(iSlot), mGame.getImportSlotCost(iSlot));
        }

        public virtual int getImportCost(int iSlot, bool bIgnoreStockpile)
        {
            return getImportMoneyCost(iSlot) + getImportResourceCost(iSlot, bIgnoreStockpile);
        }

        public virtual int getImportRevenue(int iSlot)
        {
            return gameClient().marketClient().calculateSellRevenue(gameClient().getImportSlotResource(iSlot), infos().Globals.IMPORT_AMOUNT, Constants.PRICE_MIN / Constants.PRICE_MULTIPLIER);
        }

        public virtual int getImportNetRevenue(int iSlot)
        {
            return getImportRevenue(iSlot) - getImportCost(iSlot, true);
        }

        public virtual string getColonyIncomeString() { return string.Empty; }

        public virtual int getDebtPayment()
        {
            return (int)Math.Min(-(getDebt()), Math.Min(infos().Globals.DEBT_PAYMENT, getMoney()));
        }

        public virtual bool canPayDebt()
        {
            if (getDebt() == 0)
            {
                return false;
            }

            return true;
        }

        public virtual bool canBid()
        {
            if (!isWinEligible())
            {
                return false;
            }

            if (gameClient().getAuction() == AuctionType.TILE)
            {
                if (gameClient().auctionTileClient().getRealTeam() == getTeam())
                {
                    return false;
                }
            }

            if (gameClient().getAuctionLeader() != PlayerType.NONE)
            {
                PlayerClient pAuctionLeader = gameClient().playerClient(gameClient().getAuctionLeader());

                if (pAuctionLeader.getTeam() == getTeam())
                {
                    if (isHuman())
                    {
                        if (pAuctionLeader.isHuman())
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public virtual int getBlackMarketCost(BlackMarketType eBlackMarket)
        {
            int iCost = infos().blackMarket(eBlackMarket).miBaseCost;

            iCost *= Math.Max(0, (GameServer.saiBlackMarketCostModifier[(int)(gameClient().getNumTeams() - 1)] + 100));
            iCost /= 100;

            iCost *= Math.Max(0, (infos().handicap(getHandicap()).miBlackMarketCostModifier + 100));
            iCost /= 100;

            int iCount = gameClient().getBlackMarketCount(eBlackMarket);

            if ((int)(gameClient().getNumPlayers()) == 2)
            {
                iCount += getBlackMarketCount(eBlackMarket);

                for (int i = 0; i < iCount; i++)
                {
                    iCost += (iCost * infos().blackMarket(eBlackMarket).miPriceGrowthModifier / 400);
                }
            }
            else
            {
                for (int i = 0; i < iCount; i++)
                {
                    iCost += (iCost * infos().blackMarket(eBlackMarket).miPriceGrowthModifier / 200);
                }
            }

            if (iCost < 5000)
            {
                iCost -= (iCost % 500);
            }
            else if (iCost < 10000)
            {
                iCost -= (iCost % 1000);
            }
            else if (iCost < 20000)
            {
                iCost -= (iCost % 2000);
            }
            else if (iCost < 50000)
            {
                iCost -= (iCost % 5000);
            }
            else
            {
                iCost -= (iCost % 10000);
            }

            return Math.Max(iCost, 1000);
        }

        public virtual int calculateBlackMarketDelay(BlackMarketType eBlackMarket)
        {
            int iDelay = infos().Globals.BLACK_MARKET_DELAY;

            iDelay *= Math.Max(0, (infos().blackMarket(eBlackMarket).miDelayModifier + 100));
            iDelay /= 100;

            bool bHostile = false;

            SabotageType eSabotage = infos().blackMarket(eBlackMarket).meSabotage;

            if (eSabotage != SabotageType.NONE)
            {
                bHostile = infos().sabotage(eSabotage).mbHostile;
            }

            if (bHostile)
            {
                if (getHQ() != HQType.NONE)
                {
                    iDelay *= Math.Max(0, (infos().HQ(getHQ()).miBlackMarketHostileTimeModifier + 100));
                    iDelay /= 100;
                }
            }

            {
                ColonyClassType eColonyClass = gameClient().getColonyClass();

                if (eColonyClass != ColonyClassType.NONE)
                {
                    iDelay *= Math.Max(0, (infos().colonyClass(eColonyClass).miBlackMarketTimeModifier + 100));
                    iDelay /= 100;
                }
            }

            {
                EventStateType eEventStateGame = gameClient().getEventStateGameActive();

                if (eEventStateGame != EventStateType.NONE)
                {
                    iDelay *= Math.Max(0, (infos().eventState(eEventStateGame).miBlackMarketTimeModifier + 100));
                    iDelay /= 100;
                }
            }

            {
                EventStateType eEventStateLevel = gameClient().getEventStateLevel();

                if (eEventStateLevel != EventStateType.NONE)
                {
                    iDelay *= Math.Max(0, (infos().eventState(eEventStateLevel).miBlackMarketTimeModifier + 100));
                    iDelay /= 100;
                }
            }

            return iDelay;
        }

        public virtual bool canBlackMarket()
        {
            if (!isHQFounded())
            {
                return false;
            }

            return true;
        }

        public virtual bool canBlackMarket(BlackMarketType eBlackMarket, bool bTestMoney)
        {
            if (!canBlackMarket())
            {
                return false;
            }

            if (getBlackMarketTime() > 0)
            {
                return false;
            }

            if (!(gameClient().isBlackMarketAvailable(eBlackMarket)))
            {
                return false;
            }

            {
                SabotageType eSabotage = infos().blackMarket(eBlackMarket).meSabotage;

                if (eSabotage != SabotageType.NONE)
                {
                    if (getSabotageCount(eSabotage) > 0)
                    {
                        return false;
                    }

                    if ((isHuman()) ? !(infos().rulesSet(gameClient().getRulesSet()).mbHumanBlackMarketIgnoreDebt) : true)
                    {
                        if (infos().bond(getBondRating()).mbNoBlackMarketSabotage)
                        {
                            if (!(infos().sabotage(eSabotage).mbIgnoresDebt))
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            if (bTestMoney)
            {
                if (getMoney() < getBlackMarketCost(eBlackMarket))
                {
                    return false;
                }
            }

            return true;
        }

        public virtual int innerScanRange()
        {
            return infos().mapSize(gameClient().getMapSize()).miScanChange;
        }

        public virtual int outerScanRange()
        {
            return (innerScanRange() * infos().Globals.SCAN_MULTIPLIER + 1);
        }

        public virtual bool canEverScan() // if scanning is available in the game at all
        {
            if (gameClient().isGameOption(GameOptionType.REVEAL_MAP))
            {
                return false;
            }

            return !isHQFounded();
        }

        protected virtual bool canScan()
        {
            if (!canEverScan())
            {
                return false;
            }

            if (gameClient().isTurnBasedScanning() && wasEverHuman())
            {
                return !(gameClient().isPaused());
            }
            else
            {
                return (getScans() > 0);
            }
        }

        public virtual bool canScan(TileClient pTile)
        {
            if (!canScan())
            {
                return false;
            }

            // THIS LOGIC IS DUPLICATED IN Player.AI_doScan()
            return pTile.isOrAdjacentRevealed(getPlayer());
        }

        public virtual bool canSabotageType(SabotageType eSabotage)
        {
            if (getSabotageCount(eSabotage) == 0)
            {
                return false;
            }

            return true;
        }

        public virtual bool canSabotageTile(TileClient pTile, SabotageType eSabotage, HelpInfo pHelp = null)
        {
            InfoSabotage sabotageInfo = infos().sabotage(eSabotage);

            if (sabotageInfo.mbTriggersDefense)
            {
                if (pTile.isDefend())
                {
                    if (pHelp != null)
                    {
                        pHelp.strHelp = TEXT("TEXT_PLAYER_SABOTAGE_DEFENDED", Utils.getSecondsFromTurns(pTile.getDefendTime(), gameClient()).ToText());
                    }
                    return false;
                }

                if (pTile.isClaimed())
                {
                    BuildingType eBuilding = pTile.getVisibleConstructionOrBuildingType(getTeam());

                    if (eBuilding != BuildingType.NONE)
                    {
                        if (pTile.ownerClient().isBuildingImmune(eBuilding))
                        {
                            if (pHelp != null)
                            {
                                pHelp.strHelp = TEXT("TEXT_PLAYER_SABOTAGE_IMMUNE", infos().building(eBuilding).meName.ToText());
                            }
                            return false;
                        }
                    }
                }
            }

            if (gameClient().auctionTileClient() == pTile)
            {
                if (pHelp != null)
                {
                    pHelp.strHelp = TEXT("TEXT_PLAYER_SABOTAGE_AUCTION");
                }
                return false;
            }

            if (pTile.isHQ())
            {
                if (pHelp != null)
                {
                    pHelp.strHelp = TEXT("TEXT_PLAYER_SABOTAGE_HQ");
                }
                return false;
            }

            if (sabotageInfo.miEffectLength > 0)
            {
                if (!(pTile.isBuilding()) && !(pTile.isConstruction()))
                {
                    if (pHelp != null)
                    {
                        pHelp.strHelp = TEXT("TEXT_PLAYER_SABOTAGE_BUILDING");
                    }
                    return false;
                }
            }

            bool bRivalTile = false;

            if (sabotageInfo.miResourceLevelChange < 0)
            {
                bool bFound = false;

                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    ResourceLevelType eResourceLevel = pTile.getResourceLevel(eLoopResource, false);

                    if ((eResourceLevel > ResourceLevelType.NONE) && infos().resourceLevel(eResourceLevel).mbCanBomb)
                    {
                        bFound = true;
                        break;
                    }
                }

                if (!bFound)
                {
                    if (pHelp != null)
                    {
                        pHelp.strHelp = TEXT("TEXT_PLAYER_SABOTAGE_RESOURCE");
                    }
                    return false;
                }

                bRivalTile = true;
            }

            if (sabotageInfo.miTakeoverTime > 0)
            {
                if (!(pTile.isBuilding()) && !(pTile.isConstruction()))
                {
                    if (pHelp != null)
                    {
                        pHelp.strHelp = TEXT("TEXT_PLAYER_SABOTAGE_BUILDING");
                    }
                    return false;
                }

                bRivalTile = true;
            }

            if (sabotageInfo.miHarvestQuantity > 0)
            {
                if (!(pTile.canMine()))
                {
                    if (!(pTile.isEmpty()))
                    {
                        if (pHelp != null)
                        {
                            pHelp.strHelp = TEXT("TEXT_PLAYER_SABOTAGE_EMPTY");
                        }
                        return false;
                    }

                    if (pHelp != null)
                    {
                        pHelp.strHelp = TEXT("TEXT_PLAYER_SABOTAGE_USABLE");
                    }
                    return false;
                }

                bool bFound = false;

                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    ResourceLevelType eResourceLevel = pTile.getResourceLevel(eLoopResource, isAdjacentMining());

                    if (eResourceLevel > ResourceLevelType.NONE)
                    {
                        bFound = true;
                        break;
                    }
                }

                if (!bFound)
                {
                    if (pHelp != null)
                    {
                        pHelp.strHelp = TEXT("TEXT_PLAYER_SABOTAGE_RESOURCE");
                    }
                    return false;
                }
            }

            if (sabotageInfo.miDamageBuilding > 0)
            {
                if (!(pTile.isBuilding()) && !(pTile.isConstruction()))
                {
                    if (pHelp != null)
                    {
                        pHelp.strHelp = TEXT("TEXT_PLAYER_SABOTAGE_BUILDING");
                    }
                    return false;
                }

                if (pTile.isConstruction())
                {
                    if (pTile.constructionClient().isDamaged())
                    {
                        if (pHelp != null)
                        {
                            pHelp.strHelp = TEXT("TEXT_PLAYER_SABOTAGE_DAMAGED");
                        }
                        return false;
                    }
                }

                bRivalTile = true;
            }

            if (sabotageInfo.mbDoubleBuilding)
            {
                if (pTile.isDoubleAlways(getTeam()))
                {
                    if (pHelp != null)
                    {
                        pHelp.strHelp = TEXT("TEXT_PLAYER_SABOTAGE_BOOSTED");
                    }
                    return false;
                }
            }

            if (sabotageInfo.mbVirusBuilding)
            {
                bRivalTile = true;
            }

            if (sabotageInfo.mbReturnClaim)
            {
                if (!canReturnClaim(pTile, true, pHelp))
                {
                    return false;
                }
            }

            if (sabotageInfo.mbAuctionTile)
            {
                if (gameClient().isAuction())
                {
                    return false;
                }

                if (pTile.isClaimed())
                {
                    if (gameClient().getNumTeams() == 2)
                    {
                        return false;
                    }

                    if (!(pTile.isOwnerReal()) || (pTile.getRealOwner() != getPlayer()))
                    {
                        if (pHelp != null)
                        {
                            pHelp.strHelp = TEXT("TEXT_PLAYER_AUCTION_NOT_YOUR_TILE");
                        }
                        return false;
                    }
                }
                else
                {
                    if (!(pTile.usable()))
                    {
                        if (pHelp != null)
                        {
                            pHelp.strHelp = TEXT("TEXT_PLAYER_SABOTAGE_USABLE");
                        }
                        return false;
                    }

                    if (!(pTile.isEmpty()))
                    {
                        if (pHelp != null)
                        {
                            pHelp.strHelp = TEXT("TEXT_PLAYER_SABOTAGE_EMPTY");
                        }
                        return false;
                    }
                }
            }

            if (sabotageInfo.mbWrongBuilding)
            {
                if (!(pTile.isOwnerReal()) || (pTile.getRealOwner() != getPlayer()))
                {
                    if (pHelp != null)
                    {
                        pHelp.strHelp = TEXT("TEXT_PLAYER_WRONG_BUILDING_NOT_OWNED");
                    }
                    return false;
                }

                if (pTile.isHologram())
                {
                    return false;
                }

                if (pTile.isGeothermal() && !(isBorehole()))
                {
                    if (pHelp != null)
                    {
                        pHelp.strHelp = TEXT("TEXT_PLAYER_SABOTAGE_NO_HOLOGRAM");
                    }
                    return false;
                }
            }

            if (sabotageInfo.mbRevealBuilding)
            {
                if (!(pTile.isClaimed()) || (pTile.getRealTeam() == getTeam()))
                {
                    if (pHelp != null)
                    {
                        pHelp.strHelp = TEXT("TEXT_PLAYER_SABOTAGE_RIVAL_OWNER");
                    }
                    return false;
                }

                if (pTile.isRevealBuilding(getTeam()))
                {
                    return false;
                }
            }

            if (sabotageInfo.mbNewResource)
            {
                if (pTile.noResources() || pTile.isGeothermal())
                {
                    if (pHelp != null)
                    {
                        pHelp.strHelp = TEXT("TEXT_PLAYER_SABOTAGE_USABLE");
                    }
                    return false;
                }

                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    ResourceLevelType eResourceLevel = pTile.getResourceLevel(eLoopResource, false);

                    if (eResourceLevel > ResourceLevelType.NONE)
                    {
                        if (pHelp != null)
                        {
                            pHelp.strHelp = TEXT("TEXT_PLAYER_SABOTAGE_RESOURCES");
                        }
                        return false;
                    }
                }
            }

            if (sabotageInfo.mbChangeTerrain)
            {
                if (pTile.isClaimed())
                {
                    if (!(pTile.isOwnerReal()) || (pTile.getRealOwner() != getPlayer()))
                    {
                        if (pHelp != null)
                        {
                            pHelp.strHelp = TEXT("TEXT_PLAYER_AUCTION_NOT_YOUR_TILE");
                        }
                        return false;
                    }
                    if(pTile.getBuildingType() != BuildingType.NONE && !infos().building(pTile.getBuildingType()).mbCanScrap)
                    {
                        if (pHelp != null)
                        {
                            pHelp.strHelp = TEXT("TEXT_PLAYER_CANNOT_SCRAP_BUILDING");
                        }
                        return false;
                    }
                }
                if (!(pTile.usable()) || pTile.getTerrain() == infos().Globals.CAVE_TERRAIN || pTile.isModule())
                {
                    if (pHelp != null)
                    {
                        pHelp.strHelp = TEXT("TEXT_PLAYER_SABOTAGE_USABLE");
                    }
                    return false;
                }
                if (pTile.hasResources() || pTile.isGeothermal() || pTile.noResources())
                {
                    if (pHelp != null)
                    {
                        pHelp.strHelp = TEXT("TEXT_PLAYER_SABOTAGE_RESOURCES");
                    }
                    return false;
                }
            }

            if (sabotageInfo.mbDefendSabotage)
            {
                if (pTile.getTeam() != getTeam())
                {
                    if (pHelp != null)
                    {
                        pHelp.strHelp = TEXT("TEXT_PLAYER_DEFEND_SABOTAGE_NOT_OWNED");
                    }
                    return false;
                }

                if (pTile.getDefendSabotage() != SabotageType.NONE)
                {
                    return false;
                }
            }

            {
                UnitType eUnit = sabotageInfo.meUnit;

                if (eUnit != UnitType.NONE)
                {
                    if (infos().unit(eUnit).miMovement == 0)
                    {
                        if (!(pTile.usable()))
                        {
                            if (pHelp != null)
                            {
                                pHelp.strHelp = TEXT("TEXT_PLAYER_SABOTAGE_USABLE");
                            }
                            return false;
                        }

                        if (!(pTile.isEmpty()) || pTile.isUnit())
                        {
                            if (pHelp != null)
                            {
                                pHelp.strHelp = TEXT("TEXT_PLAYER_SABOTAGE_EMPTY");
                            }
                            return false;
                        }
                    }
                }
            }

            if (bRivalTile)
            {
                if (pTile.getTeam() == getTeam())
                {
                    if (pHelp != null)
                    {
                        pHelp.strHelp = TEXT("TEXT_PLAYER_SABOTAGE_RIVAL_OWNER");
                    }
                    return false;
                }
            }

            return true;
        }

        public virtual bool canSabotage(TileClient pTile, SabotageType eSabotage, HelpInfo pHelp = null)
        {
            if (!canSabotageType(eSabotage))
            {
                return false;
            }

            if (!canSabotageTile(pTile, eSabotage, pHelp))
            {
                return false;
            }

            if(!isHQFounded() && infos().sabotage(eSabotage).meUnit != UnitType.NONE)
            {
                return false;
            }

            if (pHelp != null)
            {
                if ((infos().sabotage(eSabotage).mbFreezeBuilding) ||
                    (infos().sabotage(eSabotage).mbHalfBuilding) ||
                    (infos().sabotage(eSabotage).mbOverloadBuilding) ||
                    (infos().sabotage(eSabotage).mbVirusBuilding) ||
                    (infos().sabotage(eSabotage).miResourceLevelChange != 0) ||
                    (infos().sabotage(eSabotage).miTakeoverTime != 0) ||
                    (infos().sabotage(eSabotage).miDamageBuilding != 0))
                {
                    int iValue = 100;

                    if (infos().sabotage(eSabotage).mbFreezeBuilding)
                    {
                        if (pTile.isClaimed() && pTile.isOwnerReal())
                        {
                            iValue *= Math.Max(0, (infos().HQ(pTile.ownerClient().getHQ()).miFrozenEffectModifier + 100));
                            iValue /= 100;
                        }
                    }

                    if (pTile.isClaimed() && pTile.isOwnerReal())
                    {
                        if (pTile.adjacentToHQ(pTile.getOwner()))
                        {
                            iValue *= Math.Max(0, (pTile.ownerClient().getAdjacentHQSabotageModifier() + 100));
                            iValue /= 100;
                        }
                    }

                    if (iValue != 100)
                    {
                        pHelp.strHelp = TEXT("TEXT_PLAYER_SABOTAGE_EFFECT", ((iValue - 100) + "%").ToText());
                    }
                }
            }

            return true;
        }

        public virtual int lifeSupport(ResourceType eResource, HQLevelType eHQLevel)
        {
            if (getHQ() == HQType.NONE)
            {
                return 0;
            }

            int iLifeSupport = -(infos().HQ(getHQ()).maiLifeSupport[(int)eResource] * (int)(eHQLevel - 1));

            iLifeSupport *= Math.Max(0, (infos().location(gameClient().getLocation()).miLifeSupportModifier + 100));
            iLifeSupport /= 100;

            {
                EventStateType eEventStateGame = gameClient().getEventStateGameActive();

                if (eEventStateGame != EventStateType.NONE)
                {
                    iLifeSupport *= Math.Max(0, (infos().eventState(eEventStateGame).miLifeSupportModifier + 100));
                    iLifeSupport /= 100;
                }
            }

            {
                EventStateType eEventStateLevel = gameClient().getEventStateLevel();

                if (eEventStateLevel != EventStateType.NONE)
                {
                    iLifeSupport *= Math.Max(0, (infos().eventState(eEventStateLevel).miLifeSupportModifier + 100));
                    iLifeSupport /= 100;
                }
            }

            if (!isHuman())
            {
                iLifeSupport *= Math.Max(0, (infos().handicap(gameClient().getHumanHandicap()).miAILifeSupportModifier + 100));
                iLifeSupport /= 100;
            }

            return Math.Min(0, iLifeSupport);
        }
        public virtual int lifeSupport(ResourceType eResource)
        {
            return lifeSupport(eResource, getHQLevel());
        }

        public virtual int getUpgradeResourceCost(ResourceType eResource, HQLevelType eLevel)
        {
            if (!gameClient().isResourceValid(eResource))
                return 0;

            int iCost = (getUpgradeResourceCost(eResource) * (int)eLevel);

            {
                EventStateType eEventStateGame = gameClient().getEventStateGameActive();

                if (eEventStateGame != EventStateType.NONE)
                {
                    iCost *= Math.Max(0, (infos().eventState(eEventStateGame).miUpgradeModifier + 100));
                    iCost /= 100;
                }
            }

            {
                EventStateType eEventStateLevel = gameClient().getEventStateLevel();

                if (eEventStateLevel != EventStateType.NONE)
                {
                    iCost *= Math.Max(0, (infos().eventState(eEventStateLevel).miUpgradeModifier + 100));
                    iCost /= 100;
                }
            }

            return iCost;
        }

        public virtual int getUpgradeMoneyCost(HQLevelType eLevel, bool bIgnoreStockpile = false)
        {
            int iCost = 0;

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                if (bIgnoreStockpile)
                {
                    iCost += getIgnoreStockpileCost(eLoopResource, getUpgradeResourceCost(eLoopResource, eLevel));
                }
                else
                {
                    iCost += getNeededResourceCost(eLoopResource, getUpgradeResourceCost(eLoopResource, eLevel));
                }
            }

            return iCost;
        }

        public virtual int getLevelClaims(HQLevelType eLevel)
        {
            int iClaims = infos().HQLevel(eLevel).miUpgradeClaims;

            iClaims += infos().HQLevel(eLevel).maiHandicapUpgradeClaims[(int)getHandicap()];

            if (getHQ() != HQType.NONE)
            {
                iClaims += infos().HQ(getHQ()).miClaimsPerUpgrade;
            }

            if (eLevel == getHQClaimBonus())
            {
                iClaims++;
            }

            if (gameClient().isCampaign())
            {
                for (ColonyType eLoopColony = 0; eLoopColony < infos().coloniesNum(); eLoopColony++)
                {
                    ColonyBonusLevelType eColonyBonusLevel = corporation().maeColonyBonusLevel[(int)eLoopColony];

                    if (eColonyBonusLevel != ColonyBonusLevelType.NONE)
                    {
                        iClaims += infos().colonyBonus(infos().colony(eLoopColony).maeColonyBonus[(int)eColonyBonusLevel]).maiHQLevelClaims[(int)eLevel];
                    }
                }
            }

            return Math.Max(1, iClaims);
        }

        public virtual int getRemainingLevelClaims()
        {
            int iRemainingClaims = 0;

            for (HQLevelType eLevel = getHQLevel() + 1; eLevel < infos().HQLevelsNum(); eLevel++)
                iRemainingClaims += getLevelClaims(eLevel);

            return iRemainingClaims;
        }

        public virtual bool canFound(bool bTestMoney = true)
        {
            return !mbHQFounded;
        }

        public virtual bool canFound(HQType eHQ)
        {
            if (!(gameClient().canFound(eHQ, isHuman())))
            {
                return false;
            }

            if (getHQ() != HQType.NONE)
            {
                if (getHQ() != eHQ)
                {
                    return false;
                }
            }

            if (gameClient().isCampaign())
            {
                if (!isHQUnlock(eHQ))
                {
                    return false;
                }
            }

            return true;
        }

        protected virtual bool canFoundTile(TileClient pTile, bool bTestVisibility, HelpInfo pHelp = null)
        {
            if (pTile == null)
            {
                if (pHelp != null)
                {
                    pHelp.strHelp = TEXT("TEXT_PLAYER_INVALID_TILE");
                }
                return false;
            }

            if (bTestVisibility)
            {
                if (pTile.getVisibility(getPlayer()) == VisibilityType.FOGGED)
                {
                    if (pHelp != null)
                    {
                        pHelp.strHelp = TEXT("TEXT_PLAYER_CANNOT_FOUND_UNSCANNED");
                    }
                    return false;
                }

                if (pTile.getVisibility(getPlayer()) == VisibilityType.REVEALED)
                {
                    if (pTile.hasResources() || pTile.isGeothermal())
                    {
                        if (pHelp != null)
                        {
                            pHelp.strHelp = TEXT("TEXT_PLAYER_CANNOT_FOUND_HIDDEN_RESOURCE");
                        }
                        return false;
                    }
                }
            }

            if (!(pTile.usable()) || infos().terrain(pTile.getTerrain()).mbRequiredOnly)
            {
                if (pHelp != null)
                {
                    pHelp.strHelp = TEXT("TEXT_PLAYER_CANNOT_FOUND_TERRAIN", infos().terrain(pTile.getTerrain()).meName.ToText());
                }

                return false;
            }

            if (pTile.isGeothermal())
            {
                if (pHelp != null)
                {
                    if (gameClient().getLocation() == LocationType.EUROPA)
                    {
                        pHelp.strHelp = TEXT("TEXT_PLAYER_CANNOT_FOUND_HYDROTHERMAL");
                    }
                    else
                    {
                        pHelp.strHelp = TEXT("TEXT_PLAYER_CANNOT_FOUND_GEOTHERMAL");
                    }
                }

                return false;
            }

            if (!(pTile.isEmpty()))
            {
                if (pHelp != null)
                {
                    pHelp.strHelp = TEXT("TEXT_PLAYER_CANNOT_FOUND_COLONY");
                }

                return false;
            }

            if (gameClient().auctionTileClient() == pTile)
            {
                if (pHelp != null)
                {
                    pHelp.strHelp = TEXT("TEXT_PLAYER_CANNOT_FOUND_AUCTION");
                }
                return false;
            }

            if (pTile.isClaimed() && (pTile.getOwner() != getPlayer()))
            {
                if (pHelp != null)
                {
                    pHelp.strHelp = TEXT("TEXT_PLAYER_CANNOT_FOUND_OTHER_PLAYER");
                }
                return false;
            }

            if (pTile.isClaimBlock(getPlayer()))
            {
                if (pHelp != null)
                {
                    pHelp.strHelp = TEXT("TEXT_PLAYER_CANNOT_FOUND_BLOCKED", Utils.getSecondsFromTurns(pTile.getClaimBlockTime(), gameClient()).ToText());
                }
                return false;
            }

            return true;
        }

        public virtual List<int> getHQResourceGeneration(HQType eHQ, IEnumerable<TileClient> aTileFootprint, bool ignoreVisibility)
        {
            List<int> aiResources = Enumerable.Repeat(0, (int)infos().resourcesNum()).ToList();

            foreach (TileClient tile in aTileFootprint)
            {
                if (ignoreVisibility || tile.getVisibility(getPlayer()) == VisibilityType.VISIBLE)
                {
                    infos().resources().ForEach(pResource => aiResources[pResource.miType] += HQClient.resourceBonus(pResource.meType, tile, getPlayer(), eHQ, gameClient(), infos()));
                }
            }

            return aiResources;
        }

        public virtual List<int> getHQResourceGeneration(HQType eHQ, TileClient tile, bool ignoreVisibility)
        {
            InfoHQ hqInfo = infos().HQ(eHQ);
            return getHQResourceGeneration(eHQ, HQClient.getHQFootprint(tile, gameClient(), hqInfo), ignoreVisibility);
        }

        public virtual bool canFound(TileClient pTile, HQType eHQ, bool bTestVisibility, HelpInfo pHelp = null)
        {
            //using (new UnityProfileScope("PlayerClient.canFoundTileHQ"))
            {
                if (!canFound())
                {
                    return false;
                }

                foreach (var pair in gameClient().getHQDictionary())
                {
                    HQClient pLoopHQ = pair.Value;
                    if (pLoopHQ != null)
                    {
                        if (Utils.stepDistanceTile(pLoopHQ.tileClient(), pTile) <= infos().Globals.MIN_HQ_DISTANCE)
                        {
                            if (pHelp != null)
                            {
                                pHelp.strHelp = TEXT("TEXT_PLAYER_CANNOT_FOUND_TOO_CLOSE");
                            }
                            return false;
                        }
                    }
                }

                if (!canFoundTile(pTile, bTestVisibility, pHelp))
                {
                    return false;
                }

                if (eHQ != HQType.NONE)
                {
                    if (!canFound(eHQ))
                    {
                        return false;
                    }

                    {
                        HeightType eHeight = pTile.getHeight();

                        for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                        {
                            if (infos().HQ(eHQ).mabFootprint[(int)eLoopDirection])
                            {
                                TileClient pLoopTile = gameClient().mapClient().tileClientAdjacent(pTile, eLoopDirection);

                                if (!canFoundTile(pLoopTile, bTestVisibility, pHelp))
                                {
                                    return false;
                                }

                                if (pLoopTile.getHeight() != eHeight)
                                {
                                    if (pHelp != null)
                                    {
                                        pHelp.strHelp = TEXT("TEXT_PLAYER_CANNOT_FOUND_UNEVEN");
                                    }
                                    return false;
                                }
                            }
                        }
                    }
                }

                if (isHuman())
                {
                    int iRange = 3;

                    for (int iDX = -(iRange); iDX <= iRange; iDX++)
                    {
                        for (int iDY = -(iRange); iDY <= iRange; iDY++)
                        {
                            TileClient pRangeTile = gameClient().tileClientRange(pTile, iDX, iDY, iRange);

                            if (pRangeTile != null)
                            {
                                if (pRangeTile.isClaimed())
                                {
                                    int iTurn = pRangeTile.getFirstClaimTurn();
                                    if (iTurn != 0)
                                    {
                                        int iDiff = (iTurn + 2) - gameClient().getTurnCount();
                                        if (iDiff > 0)
                                        {
                                            if (pHelp != null)
                                            {
                                                pHelp.strHelp = TEXT("TEXT_PLAYER_PLEASE_WAIT", Utils.getSecondsFromTurns(iDiff, gameClient()).ToText());
                                            }
                                            return false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return true;
            }
        }

        public virtual bool canUpgrade(bool bTestMoney)
        {
            if (!isHQFounded())
            {
                return false;
            }

            if (isMaxUpgrade())
            {
                return false;
            }

            if (bTestMoney)
            {
                int iCost = getUpgradeMoneyCost(getHQLevel());

                if (isHuman() && (infos().rulesSet(gameClient().getRulesSet()).mbNoAutoBuy || isAnyRequiredResourceNoBuy(infos().HQ(getHQ()).maiUpgradeResource)))
                {
                    if (iCost > 0)
                    {
                        return false;
                    }
                }
                else
                {
                    if (iCost > getMoney())
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public virtual bool canClaimTile(TileClient pTile, bool bTestClaims, HelpInfo pHelp = null)
        {
            //using (new UnityProfileScope("Player::canClaim"))
            {
                if (pTile.isClaimed())
                {
                    if (pHelp != null)
                    {
                        pHelp.strHelp = TEXT("TEXT_PLAYER_CANNOT_CLAIM_CLAIMED");
                    }
                    return false;
                }
                else if (bTestClaims)
                {
                    if (getClaims() == 0)
                    {
                        if (pHelp != null)
                        {
                            pHelp.strHelp = TEXT("TEXT_PLAYER_CANNOT_CLAIM_NO_CLAIMS");
                        }
                        return false;
                    }
                }

                if (pTile == gameClient().auctionTileClient())
                {
                    if (pHelp != null)
                    {
                        pHelp.strHelp = TEXT("TEXT_PLAYER_CANNOT_CLAIM_AUCTION");
                    }
                    return false;
                }

                if (pTile.isModule())
                {
                    if (pHelp != null)
                    {
                        pHelp.strHelp = TEXT("TEXT_PLAYER_CANNOT_CLAIM_OCCUPIED");
                    }
                    return false;
                }

                if (!(pTile.usable()))
                {
                    if (pHelp != null)
                    {
                        pHelp.strHelp = TEXT("TEXT_PLAYER_CANNOT_CLAIM_TERRAIN", infos().terrain(pTile.getTerrain()).meName.ToText());
                    }
                    return false;
                }

                if (pTile.isClaimBlock(getPlayer()))
                {
                    if (pHelp != null)
                    {
                        pHelp.strHelp = TEXT("TEXT_PLAYER_CANNOT_CLAIM", Utils.getSecondsFromTurns(pTile.getClaimBlockTime(), gameClient()).ToText());
                    }
                    return false;
                }

                return true;
            }
        }

        public virtual bool canReturnClaim(TileClient pTile, bool bSabotage, HelpInfo pHelp = null)
        {
            if (!(pTile.isOwnerReal()))
            {
                return false;
            }

            if (pTile.getOwner() != getPlayer())
            {
                return false;
            }

            if (!(pTile.isEmpty()))
            {
                if (pHelp != null)
                {
                    pHelp.strHelp = TEXT("TEXT_PLAYER_SABOTAGE_EMPTY");
                }
                return false;
            }

            if (pTile.getDefendSabotage() != SabotageType.NONE)
            {
                if (pHelp != null)
                {
                    pHelp.strHelp = TEXT("TEXT_PLAYER_SABOTAGE_SABOTAGED");
                }
                return false;
            }

            if (pTile.isHologram())
            {
                if (pHelp != null)
                {
                    pHelp.strHelp = TEXT("TEXT_PLAYER_SABOTAGE_SABOTAGED");
                }
                return false;
            }

            if (pTile.isFrozen() || pTile.isHalf() || pTile.isOverload() || pTile.isVirus())
            {
                if (pHelp != null)
                {
                    pHelp.strHelp = TEXT("TEXT_PLAYER_SABOTAGE_SABOTAGED");
                }
                return false;
            }

            if (!bSabotage)
            {
                if (pTile.isWasAuctioned())
                {
                    if (pHelp != null)
                    {
                        pHelp.strHelp = TEXT("TEXT_PLAYER_SABOTAGE_AUCTIONED");
                    }
                    return false;
                }

                if (pTile.getLastBuilding() != BuildingType.NONE)
                {
                    return false;
                }
            }

            return true;
        }
        public virtual SabotageType canReturnClaimSabotage(TileClient pTile)
        {
            for (SabotageType eLoopSabotage = 0; eLoopSabotage < infos().sabotagesNum(); eLoopSabotage++)
            {
                if (infos().sabotage(eLoopSabotage).mbReturnClaim)
                {
                    if (getSabotageCount(eLoopSabotage) > 0)
                    {
                        if (canSabotage(pTile, eLoopSabotage))
                        {
                            return eLoopSabotage;
                        }
                    }
                }
            }

            return SabotageType.NONE;
        }

        public virtual bool canCancelConstruct(TileClient pTile)
        {
            return (pTile.getUnitMissionCount(getPlayer(), MissionType.CONSTRUCT) > 0);
        }

        public virtual int getBuildingMoneyCost(BuildingType eBuilding, bool bIgnoreStockpile = false)
        {
            int iCost = 0;

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                if (bIgnoreStockpile)
                {
                    iCost += getIgnoreStockpileCost(eLoopResource, getBuildingResourceCost(eBuilding, eLoopResource));
                }
                else
                {
                    iCost += getNeededResourceCost(eLoopResource, getBuildingResourceCost(eBuilding, eLoopResource));
                }
            }

            return iCost;
        }

        public virtual TileClient getFreeBuildingTile(BuildingType eFreeBuilding, TileClient pTile, HQType eHQ, bool bVisibleOnly, HashSet<int> siHQTiles = null, HashSet<int> siIgnoreTiles = null)
        {
            TileClient pBestTile = null;
            int iBestValue = 0;

            OrderType eOrder = infos().buildingClass(infos().building(eFreeBuilding).meClass).meOrderType;
            int iMaxDist = Utils.maxStepDistance(gameClient());

            foreach (TileClient pLoopTile in gameClient().tileClientAll())
            {
                if ((siHQTiles == null) || !(siHQTiles.Contains(pLoopTile.getID())))
                {
                    if ((siIgnoreTiles == null) || !(siIgnoreTiles.Contains(pLoopTile.getID())))
                    {
                        if (!bVisibleOnly || (pLoopTile.getVisibility(getPlayer()) == VisibilityType.VISIBLE))
                        {
                            if (canConstructTile(pLoopTile, eFreeBuilding, false))
                            {
                                int iValue = 50;

                                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                {
                                    int iTemp = 0;

                                    iTemp += gameClient().resourceMiningTile(eFreeBuilding, eLoopResource, pLoopTile, getPlayer(), infos().building(eFreeBuilding).maiResourceMining[(int)eLoopResource], 0, true);
                                    iTemp += gameClient().resourceOutput(eFreeBuilding, eLoopResource, pLoopTile, getPlayer(), 0, true);

                                    iTemp *= Math.Max(0, (infos().HQ(eHQ).maiFreeResourceModifier[(int)eLoopResource] + 100));
                                    iTemp /= 100;

                                    iValue += iTemp;
                                }

                                iValue += gameClient().entertainmentProfit(eFreeBuilding, pLoopTile, getPlayer(), true, true);

                                bool bAdjacentToHQ = false;

                                for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                                {
                                    TileClient pAdjacentTile = gameClient().tileClientAdjacent(pLoopTile, eLoopDirection);

                                    if (pAdjacentTile != null)
                                    {
                                        if (eOrder != OrderType.NONE)
                                        {
                                            if (pAdjacentTile.isModule())
                                            {
                                                iValue += (gameClient().getModuleOrderModifier(pAdjacentTile.getModule(), eOrder) * 10);
                                            }
                                        }

                                        if (siHQTiles != null)
                                        {
                                            if (siHQTiles.Contains(pAdjacentTile.getID()))
                                            {
                                                bAdjacentToHQ = true;
                                            }
                                        }
                                    }
                                }

                                if (bAdjacentToHQ)
                                {
                                    iValue *= 4;
                                    iValue /= 3;
                                }

                                if (infos().building(eFreeBuilding).mbRequiresModuleOrHQ)
                                {
                                    if (bAdjacentToHQ || pLoopTile.adjacentToModule())
                                    {
                                        iValue *= 2;
                                    }
                                }

                                int iDist = Utils.stepDistanceTile(pLoopTile, pTile);

                                iValue *= (iMaxDist * 2);
                                iValue /= (iMaxDist + iDist);

                                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                {
                                    if (pLoopTile.getResourceLevel(eLoopResource, false) > ResourceLevelType.NONE)
                                    {
                                        if (infos().building(eFreeBuilding).maiResourceMining[(int)eLoopResource] == 0 && getBuildingMoneyCost(eFreeBuilding, true) != 0)
                                        {
                                            iValue /= (int)(pLoopTile.getResourceLevel(eLoopResource, false));
                                        }
                                    }
                                }

                                int iConstructionSpeedModifier = infos().terrain(pLoopTile.getTerrain()).maiConstructionModifier[(int)gameClient().getLocation()];
                                iValue *= (100 + iConstructionSpeedModifier / 5);
                                iValue /= 100;

                                if (iValue > iBestValue)
                                {
                                    pBestTile = pLoopTile;
                                    iBestValue = iValue;
                                }
                            }
                        }
                    }
                }
            }

            return pBestTile;
        }

        public virtual bool requiresMorePerks(BuildingType eBuilding)
        {
            if (gameClient().isCampaign())
            {
                BuildingClassType eBuildingClass = infos().building(eBuilding).meClass;

                if (!(infos().buildingClass(eBuildingClass).mabLocationNoPerks[(int)(Globals.Campaign.getLocation())]))
                {
                    if (infos().buildingClass(eBuildingClass).meOrderType != OrderType.NONE)
                    {
                        int iBuildingCount = getRealBuildingCount(eBuilding) + getRealConstructionCount(eBuilding);

                        if (iBuildingCount >= getBuildingClassLevel(eBuildingClass))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (getBuildingClassLevel(eBuildingClass) == 0)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public virtual bool canHologramAs(TileClient pTile, BuildingType eBuilding)
        {
            if (!(pTile.isShowWrongBuilding()))
            {
                return false;
            }

            if (Globals.Infos.building(eBuilding).mbNoFalse)
            {
                return false;
            }

            if (!(canConstructPlayer(eBuilding, false)))
            {
                return false;
            }

            if (!(gameClient().canTileHaveBuilding(pTile, eBuilding, getPlayer())))
            {
                return false;
            }

            return true;
        }

        public virtual bool canEverConstruct(BuildingType eBuilding, bool bTestLevel, bool bTestUnavailable, HelpInfo pHelp = null)
        {
            if (bTestUnavailable && gameClient().isBuildingUnavailable(eBuilding))
            {
                {
                    ColonyClassType eColonyClass = gameClient().getColonyClass();

                    if (eColonyClass != ColonyClassType.NONE)
                    {
                        if (infos().colonyClass(eColonyClass).mabBuildingClassInvalid[(int)(infos().building(eBuilding).meClass)])
                        {
                            if (pHelp != null)
                            {
                                pHelp.strHelp = TEXT("TEXT_HUDINFO_BUILDING_INVALID", infos().colonyClass(eColonyClass).meName.ToText());
                            }
                        }
                    }
                }

                return false;
            }

            if (getHQ() != HQType.NONE)
            {
                if (!(Utils.isBuildingValid(eBuilding, getHQ())))
                {
                    return false;
                }
            }

            if (bTestLevel)
            {
                if (requiresMorePerks(eBuilding))
                {
                    if (pHelp != null)
                    {
                        pHelp.strHelp = TEXT("TEXT_PLAYER_CONSTRUCT_NEED_ANOTHER_PERK", infos().building(eBuilding).meName.ToText());
                    }
                    return false;
                }
            }

            return true;
        }

        public virtual HQLevelType getBuildingMinLevel(BuildingType eBuilding)
        {
            HQLevelType eHQLevel = infos().building(eBuilding).meMinLevel;

            if (getHQ() != HQType.NONE)
            {
                eHQLevel += infos().HQ(getHQ()).miBuildingHQLevel;
            }

            return (HQLevelType)(Math.Max(0, (int)eHQLevel));
        }

        public virtual bool requiresLargerHQs(BuildingType eBuilding)
        {
            if (gameClient().isCampaign())
            {
                return false;
            }

            if (infos().building(eBuilding).miMaxPerLevel != -1)
            {
                int iCount = Math.Max(0, (getHQLevel() - getBuildingMinLevel(eBuilding) + 1));

                if ((getRealBuildingCount(eBuilding) + getRealConstructionCount(eBuilding)) >= (iCount * infos().building(eBuilding).miMaxPerLevel))
                {
                    return true;
                }
            }

            return false;
        }

        public virtual bool canConstructPlayer(BuildingType eBuilding, bool bTestMoney, HelpInfo pHelp = null)
        {
            //using (new UnityProfileScope("Player::canConstructPlayer"))
            {
                // checks to see if this building can ever be built by this player (defined by HQ type)
                if (!canEverConstruct(eBuilding, true, true, pHelp))
                {
                    return false;
                }

                if (requiresLargerHQs(eBuilding))
                {
                    if (pHelp != null)
                    {
                        pHelp.strHelp = TEXT("TEXT_PLAYER_CONSTRUCT_NEED_LARGER_HQ");
                    }

                    return false;
                }

                {
                    HQLevelType eMinLevel = getBuildingMinLevel(eBuilding);

                    if (getHQLevel() < eMinLevel)
                    {
                        if (pHelp != null)
                        {
                            pHelp.strHelp = TEXT("TEXT_PLAYER_CONSTRUCT_NEED_LEVEL", ((int)eMinLevel).ToText());
                        }
                        return false;
                    }
                }

                if (bTestMoney)
                {
                    int iCost = getBuildingMoneyCost(eBuilding);

                    if (isHuman() && (infos().rulesSet(gameClient().getRulesSet()).mbNoAutoBuy || isAnyRequiredResourceNoBuy(infos().building(eBuilding).maiResourceCost)))
                    {
                        if (iCost > 0)
                        {
                            if (pHelp != null)
                            {
                                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                {
                                    int iResourceNeeded = getBuildingResourceCost(eBuilding, eLoopResource);
                                    if (iResourceNeeded > getWholeResourceStockpile(eLoopResource, true))
                                    {
                                        pHelp.strHelp += TEXT("TEXT_PLAYER_CONSTRUCT_NEED_RESOURCE", iResourceNeeded.ToText(), TEXT(infos().resource(eLoopResource).meName).ToText());
                                    }
                                }
                            }
                            return false;
                        }
                    }
                    else
                    {
                        if (iCost > getMoney())
                        {
                            if (pHelp != null)
                            {
                                pHelp.strHelp = TEXT("TEXT_PLAYER_CONSTRUCT_NEED_MONEY", Utils.makeMoney(iCost, false).ToText());
                            }
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        public virtual bool canConstructTile(TileClient pTile, BuildingType eBuilding, bool bTestClaims, HelpInfo pHelp = null)
        {
            using (new UnityProfileScope("Player::canConstructTile"))
            {
                if (pTile.isClaimed())
                {
                    /*if (!(pTile.isOwnerReal())) //shouldn't ever come up because you can't mutiny empty tiles or scrap buildsing on tiles
                     {
                         if (pHelp != null)
                         {
                             pHelp.strHelp = TEXT("TEXT_PLAYER_CANNOT_CONSTRUCT_MUTINY");
                         }
                         return false;
                     }*/

                    if (pTile.getOwner() != getPlayer())
                    {
                        if (pHelp != null)
                        {
                            pHelp.strHelp = TEXT("TEXT_PLAYER_CANNOT_CONSTRUCT_OTHER_PLAYER");
                        }
                        return false;
                    }
                }
                else if (!canClaimTile(pTile, bTestClaims, pHelp))
                {
                    return false;
                }

                if (!(pTile.isEmpty()))
                {
                    if (pHelp != null)
                    {
                        pHelp.strHelp = TEXT("TEXT_PLAYER_CANNOT_CONSTRUCT_OCCUPIED");
                    }
                    return false;
                }

                if (pTile.getUnitMissionCount(getPlayer(), MissionType.CONSTRUCT) > 0)
                {
                    if (pHelp != null)
                    {
                        pHelp.strHelp = TEXT("TEXT_PLAYER_CANNOT_CONSTRUCT_UNITS");
                    }
                    return false;
                }

                if (!(gameClient().canTileHaveBuilding(pTile, eBuilding, getPlayer(), pHelp)))
                {
                    return false;
                }

                return true;
            }
        }

        public virtual bool canConstruct(TileClient pTile, BuildingType eBuilding, bool bTestMoney, bool bTestClaims, HelpInfo pHelp = null)
        {
            if (!canConstructPlayer(eBuilding, bTestMoney, pHelp))
            {
                return false;
            }

            if (!canConstructTile(pTile, eBuilding, bTestClaims, pHelp))
            {
                return false;
            }

            return true;
        }

        public virtual bool canSendPatent(PatentType ePatent, PlayerType ePlayer)
        {
            if (getPlayer() == ePlayer)
            {
                return false;
            }

            PlayerClient pToPlayer = gameClient().playerClient(ePlayer);

            if (pToPlayer.isSubsidiary())
            {
                return false;
            }

            if (getTeam() != pToPlayer.getTeam())
            {
                return false;
            }

            if (gameClient().getPatentOwner(ePatent) != getPlayer())
            {
                return false;
            }

            if (!isPatentAcquiredLab(ePatent))
            {
                return false;
            }

            if (pToPlayer.isPatentAcquired(ePatent))
            {
                return false;
            }

            return true;
        }

        public virtual bool canCraftBlackMarket(BlackMarketType eBlackMarket) { return false; }

        public virtual int getPatentResourceCost(PatentType ePatent, ResourceType eResource, bool bLicense = false)
        {
            int iCost = infos().patent(ePatent).maiResourceCost[(int)eResource];

            iCost *= Math.Max(0, getOrderCostModifier(OrderType.PATENT) + 100);
            iCost /= 100;

            if (gameClient().getNumPlayers() < (PlayerType)(Constants.MAX_NUM_PLAYERS / 2))
            {
                iCost *= ((int)(gameClient().getNumPlayers()));
                iCost /= (Constants.MAX_NUM_PLAYERS / 2);
            }

            {
                ColonyClassType eColonyClass = gameClient().getColonyClass();

                if (eColonyClass != ColonyClassType.NONE)
                {
                    iCost *= Math.Max(0, (infos().colonyClass(eColonyClass).miPatentCostModifier + 100));
                    iCost /= 100;
                }
            }

            if (bLicense)
            {
                iCost /= 2;
            }

            return iCost;
        }

        public virtual int getPatentCost(PatentType ePatent, bool bIgnoreStockpile = false)
        {
            int iCost = 0;

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                if (bIgnoreStockpile)
                {
                    iCost += getIgnoreStockpileCost(eLoopResource, getPatentResourceCost(ePatent, eLoopResource));
                }
                else
                {
                    iCost += getNeededResourceCost(eLoopResource, getPatentResourceCost(ePatent, eLoopResource));
                }
            }

            return iCost;
        }

        public virtual bool canPatent(PatentType ePatent, bool bTestMoney)
        {
            if (!(gameClient().canEverPatent(ePatent)))
            {
                return false;
            }

            if (isPatentStarted(ePatent))
            {
                return false;
            }

            if (isPatentAcquired(ePatent))
            {
                return false;
            }

            if (getHQ() != HQType.NONE)
            {
                if (!(infos().HQ(getHQ()).mbLicensePatents))
                {
                    if (gameClient().isPatentOwned(ePatent))
                    {
                        return false;
                    }
                }
            }

            if (bTestMoney)
            {
                int iCost = getPatentCost(ePatent);

                if (isHuman() && (infos().rulesSet(gameClient().getRulesSet()).mbNoAutoBuy || isAnyRequiredResourceNoBuy(infos().patent(ePatent).maiResourceCost)))
                {
                    if (iCost > 0)
                    {
                        return false;
                    }
                }
                else
                {
                    if (iCost > getMoney())
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static int getResearchTime(TechnologyType eTechnology, TechnologyLevelType eTechnologyLevel, Infos pInfos)
        {
            int iTime = pInfos.technology(eTechnology).miTime;

            iTime += (pInfos.technology(eTechnology).miTimePerLevel * (int)eTechnologyLevel);

            return iTime;
        }

        public virtual int getResearchResourceCost(TechnologyType eTechnology, TechnologyLevelType eTechnologyLevel, ResourceType eResource)
        {
            int iCost = ((infos().technology(eTechnology).maiResourceCost[(int)eResource] * ((int)eTechnologyLevel + 2)) / 3);

            iCost *= Math.Max(0, getOrderCostModifier(OrderType.RESEARCH) + 100);
            iCost /= 100;

            if (gameClient().getNumPlayers() < (PlayerType)(Constants.MAX_NUM_PLAYERS / 2))
            {
                iCost *= ((int)(gameClient().getNumPlayers()));
                iCost /= (Constants.MAX_NUM_PLAYERS / 2);
            }

            {
                ColonyClassType eColonyClass = gameClient().getColonyClass();

                if (eColonyClass != ColonyClassType.NONE)
                {
                    iCost *= Math.Max(0, (infos().colonyClass(eColonyClass).miTechCostModifier + 100));
                    iCost /= 100;
                }
            }

            return iCost;
        }

        public virtual int getResearchCost(TechnologyType eTechnology, TechnologyLevelType eTechnologyLevel, bool bIgnoreStockpile = false)
        {
            int iCost = 0;

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                if (bIgnoreStockpile)
                {
                    iCost += getIgnoreStockpileCost(eLoopResource, getResearchResourceCost(eTechnology, eTechnologyLevel, eLoopResource));
                }
                else
                {
                    iCost += getNeededResourceCost(eLoopResource, getResearchResourceCost(eTechnology, eTechnologyLevel, eLoopResource));
                }
            }

            return iCost;
        }

        public virtual TechnologyLevelType getMaxTechnologyLevel()
        {
            if (getHQ() == HQType.NONE)
            {
                return (infos().technologyLevelsNum() - 1);
            }

            return infos().HQ(getHQ()).meMaxTechnologyLevel;
        }

        public virtual bool canResearch(TechnologyType eTechnology, TechnologyLevelType eTechnologyLevel, bool bTestMoney)
        {
            if (!(gameClient().canEverResearch(eTechnology)))
            {
                return false;
            }

            if (eTechnologyLevel > getMaxTechnologyLevel())
            {
                return false;
            }

            if (bTestMoney)
            {
                int iCost = getResearchCost(eTechnology, eTechnologyLevel);

                if (isHuman() && (infos().rulesSet(gameClient().getRulesSet()).mbNoAutoBuy || isAnyRequiredResourceNoBuy(infos().technology(eTechnology).maiResourceCost)))
                {
                    if (iCost > 0)
                    {
                        return false;
                    }
                }
                else
                {
                    if (iCost > getMoney())
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public virtual int getHackCost(EspionageType eEspionage, int iCountModifier = 0)
        {
            int iCost = 0;

            iCost += infos().espionage(eEspionage).miCost;

            if (infos().espionage(eEspionage).mbSurplus)
            {
                iCost *= (getEspionageSurplusCount() + 2 + iCountModifier);
                iCost /= 2;
            }
            else
            {
                iCost *= (getEspionageShortageCount() + 2 + iCountModifier);
                iCost /= 2;
            }

            iCost *= Math.Max(0, getOrderCostModifier(OrderType.HACK) + 100);
            iCost /= 100;

            return iCost;
        }

        public virtual bool canHack(EspionageType eEspionage, bool bTestMoney)
        {
            if (!(gameClient().canEverHack(eEspionage)))
            {
                return false;
            }

            if (bTestMoney && getMoney() < getHackCost(eEspionage))
            {
                return false;
            }

            return true;
        }

        public virtual int getLaunchResourceCost(ResourceType eResource, ResourceType eShippingResource)
        {
            if (!gameClient().isResourceValid(eResource))
                return 0;

            int iQuantity = infos().resource(eResource).miLaunchCost;

            if (eResource == getLaunchResource())
            {
                iQuantity += infos().location(gameClient().getLocation()).miLaunchCost;
            }

            iQuantity *= Math.Max(0, getOrderCostModifier(OrderType.LAUNCH) + 100);
            iQuantity /= 100;

            if (eResource == eShippingResource)
            {
                iQuantity += infos().resource(eResource).miLaunchQuantity;
            }

            return iQuantity;
        }

        public virtual int getLaunchMoneyCost(ResourceType eResource)
        {
            int iCost = 0;

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                iCost += getNeededResourceCost(eLoopResource, getLaunchResourceCost(eLoopResource, eResource));
            }

            return iCost;
        }

        public virtual int getLaunchRevenue(ResourceType eResource)
        {
            return gameClient().marketClient().getWholeOffworldPrice(eResource) * infos().resource(eResource).miLaunchQuantity;
        }

        public virtual int getLaunchCost(ResourceType eResource)
        {
            int iCost = 0;

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                iCost += getLaunchResourceCost(eLoopResource, eResource) * gameClient().marketClient().getWholePrice(eLoopResource);
            }

            return iCost;
        }

        public virtual int getLaunchProfit(ResourceType eResource)
        {
            return getLaunchRevenue(eResource) - getLaunchCost(eResource);
        }

        public virtual int getBestLaunchProfit()
        {
            int iBestValue = 0;

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                if (gameClient().isResourceValid(eLoopResource))
                {
                    iBestValue = Math.Max(iBestValue, getLaunchProfit(eLoopResource));
                }
            }

            return iBestValue;
        }

        public virtual bool canLaunch(ResourceType eResource, bool bTestMoney)
        {
            if (!(gameClient().isResourceValid(eResource)))
            {
                return false;
            }

            if (infos().resource(eResource).miLaunchQuantity == 0)
            {
                return false;
            }

            if (bTestMoney)
            {
                int iCost = getLaunchMoneyCost(eResource);

                if (isHuman() && (infos().rulesSet(gameClient().getRulesSet()).mbNoAutoBuy || isRequiredResourceNoBuy(eResource, infos().resource(eResource).miLaunchQuantity) || 
                    isRequiredResourceNoBuy(getLaunchResource(), infos().resource(getLaunchResource()).miLaunchCost)))
                {
                    if (iCost > 0)
                    {
                        return false;
                    }
                }
                else
                {
                    if (iCost > getMoney())
                    {
                        return false;
                    }
                }

            }

            return true;
        }

        public OrderInfo getFirstOverflowPatent()
        {
            foreach (OrderInfo order in getOrderInfos(OrderType.PATENT))
            {
                if (infos().patent((PatentType)order.miData1).mbCanSplitResearch)
                    return order;
            }
            return null;
        }

        public virtual int getOrderRate(OrderType eOrder, TileClient pTile, bool bOverflowPatent = false)
        {
            int iRate = infos().Globals.ORDER_RATE;

            if (getHQ() != HQType.NONE)
            {
                iRate *= Math.Max(0, (infos().HQ(getHQ()).maiOrderModifier[(int)eOrder] + 100));
                iRate /= 100;
            }

            if (eOrder == OrderType.PATENT)
            {
                if (gameClient().getNumPlayers() > (PlayerType)(Constants.MAX_NUM_PLAYERS / 2))
                {
                    iRate *= (Constants.MAX_NUM_PLAYERS + (Constants.MAX_NUM_PLAYERS / 2));
                    iRate /= (Constants.MAX_NUM_PLAYERS + (int)(gameClient().getNumPlayers()));
                }
            }

            iRate *= Math.Max(0, (getOrderRateModifier(eOrder) + 100));
            iRate /= 100;

            if (pTile != null)
            {
                int iModifier = gameClient().getTileOrderRateModifier(eOrder, pTile);
                if (iModifier != 0)
                {
                    iRate *= Math.Max(0, (iModifier + 100));
                    iRate /= 100;
                }

                if (pTile.isDouble())
                {
                    if (eOrder == OrderType.LAUNCH)
                    {
                        iRate *= 3;
                        iRate /= 2;
                    }
                    else
                    {
                        iRate *= 2;
                    }
                }

                if (pTile.isHalf())
                {
                    iRate /= 2;
                }
            }

            return Math.Max(1, iRate);
        }
        public virtual int getOrderTurns(OrderType eOrder, int iTurns, TileClient pTile, bool bOverflowPatent = false)
        {
            return Math.Max(1, (iTurns / getOrderRate(eOrder, pTile, bOverflowPatent)));
        }

        public virtual bool isBuildingIgnoresInputAdjacent(BuildingType eBuilding, ResourceType eResource, TileClient pTile, bool bAdjacentMining)
        {
            if (isBuildingFreeResource(eResource))
            {
                return true;
            }

            if (pTile != null)
            {
                if (pTile.getIce() != IceType.NONE)
                {
                    if (isIgnoreInputIce(infos().building(eBuilding).meClass, pTile.getIce()))
                    {
                        return true;
                    }
                }
            }

            if (infos().building(eBuilding).mbSelfInput)
            {
                if (pTile != null)
                {
                    if (pTile.supplySelfInput(eResource, bAdjacentMining))
                    {
                        return true;
                    }
                }
                else
                {
                    if (infos().resource(eResource).maiLocationAppearanceProb[(int)(gameClient().getLocation())] > 0)
                    {
                        return true;
                    }

                    {
                        TerrainType eTerrainRate = infos().building(eBuilding).meTerrainRate;

                        if (eTerrainRate != TerrainType.NONE)
                        {
                            if (infos().terrain(eTerrainRate).maiResourceRate[(int)eResource] > 0)
                            {
                                if (gameClient().getTerrainCount(eTerrainRate) > 0)
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    for (IceType eLoopIce = 0; eLoopIce < infos().icesNum(); eLoopIce++)
                    {
                        if (infos().ice(eLoopIce).maiAverageResourceRate[(int)eResource] > 0)
                        {
                            if (gameClient().getIceCount(eLoopIce) > 0)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public virtual bool isBuildingIgnoresInput(BuildingType eBuilding, ResourceType eResource, TileClient pTile)
        {
            bool bAdjacentMining = ((pTile != null) ? pTile.isAdjacentMining(getPlayer()) : false);

            return isBuildingIgnoresInputAdjacent(eBuilding, eResource, pTile, bAdjacentMining);
        }

        public virtual int getGas(UnitType eUnit)
        {
            int iGas = -(infos().unit(eUnit).miGas);

            if (isHuman())
            {
                iGas *= Math.Max(0, (infos().rulesSet(gameClient().getRulesSet()).miGasConsumptionModifier + 100));
                iGas /= 100;
            }
            else
            {
                iGas *= Math.Max(0, (infos().handicap(gameClient().getHumanHandicap()).miAIGasConsumptionModifier + 100));
                iGas /= 100;
            }

            return Math.Min(0, iGas);
        }

        public virtual HQClient findClosestHQClient(TileClient pTile)
        {
            if (getNumHQs() <= 1)
            {
                return startingHQClient();
            }

            HQClient pBestHQ = null;
            int iBestValue = int.MaxValue;

            for (int i = 0; i < getNumHQs(); i++)
            {
                HQClient loopHQ = gameClient().hqClient(getHQList()[i]);

                int iValue = Utils.stepDistance(pTile.getX(), pTile.getY(), loopHQ.getX(), loopHQ.getY());
                if (iValue < iBestValue)
                {
                    pBestHQ = loopHQ;
                    iBestValue = iValue;
                }
            }

            return pBestHQ;
        }
        public virtual int findClosestHQClientDistance(TileClient pTile)
        {
            if (getNumHQs() == 0)
                return 0;

            if (getNumHQs() == 1)
            {
                TileClient startingHQTile = startingHQClient().tileClient();
                return Utils.stepDistance(pTile.getX(), pTile.getY(), startingHQTile.getX(), startingHQTile.getY());
            }

            int iBestValue = int.MaxValue;

            for (int i = 0; i < getNumHQs(); i++)
            {
                HQClient loopHQ = gameClient().hqClient(getHQList()[i]);

                int iValue = Utils.stepDistance(pTile.getX(), pTile.getY(), loopHQ.getX(), loopHQ.getY());
                if (iValue < iBestValue)
                {
                    iBestValue = iValue;
                }
            }
            return iBestValue;
        }

        public virtual List<int> getTilesLostToMutiny()
        {
            List<int> aResult = new List<int>();

            for (PlayerType ePlayer = 0; ePlayer < gameClient().getNumPlayers(); ePlayer++)
            {
                if (ePlayer == getPlayer())
                {
                    continue;
                }

                PlayerClient targetPlayer = gameClient().playerClient(ePlayer);
                foreach (int iTileID in targetPlayer.getTileList())
                {
                    TileClient tile = gameClient().tileClient(iTileID);
                    if (tile.getRealOwner() == getPlayer() && tile.getTakeoverTime() > 0)
                    {
                        aResult.Add(iTileID);
                    }
                }
            }

            return aResult;
        }

        public virtual List<int> getTilesIHaveMuntinied()
        {
            PlayerClient player = gameClient().playerClient(getPlayer());
            List<int> aResult = new List<int>();
            foreach (int iTileID in player.getTileList())
            {
                TileClient tile = gameClient().tileClient(iTileID);
                if (tile.getTakeoverTime() > 0)
                {
                    aResult.Add(iTileID);
                }
            }

            return aResult;
        }

        public virtual long getCashResourceValue()
        {
            if(mbShouldCalculateCashResources)
            {
                mlCashResourceValue = calculateCashResourceValue();
                mbShouldCalculateCashResources = false;
            }

            return mlCashResourceValue;
        }

        public virtual long calculateCashResourceValue()
        {
            using (new UnityProfileScope("Player::calculateCashResourceValue"))
            {
                long iCash = getMoney();
                List<int> aiTotalResources = Enumerable.Repeat(0, (int)infos().resourcesNum()).ToList();

                    if (hasOrderInfos(OrderType.HACK))
                    {
                        LinkedList<OrderInfo> queuedHacks = getOrderInfos(OrderType.HACK);
                        List<int> aiQueuedResourceHacks = Enumerable.Repeat(0, (int)infos().espionagesNum()).ToList();
                        foreach(OrderInfo order in queuedHacks)
                        {
                            aiQueuedResourceHacks[order.miData1]--;
                        }
                        for (EspionageType eLoopEspionage = 0; eLoopEspionage < infos().espionagesNum(); eLoopEspionage++)
                        {
                            int offset = aiQueuedResourceHacks[(int)eLoopEspionage];
                            for (; offset < 0; offset++)
                                iCash += getHackCost(eLoopEspionage, offset);
                        }
                    }


                for (OrderType eLoopOrder = 0; eLoopOrder < infos().ordersNum(); eLoopOrder++)
                {
                    if (hasOrderInfos(eLoopOrder))
                    {
                        foreach (OrderInfo pLoopOrderInfo in getOrderInfos(eLoopOrder))
                        {
                            switch (pLoopOrderInfo.meType)
                            {
                                case OrderType.PATENT:
                                    {
                                        PatentType ePatent = (PatentType)(pLoopOrderInfo.miData1);

                                        for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                        {
                                            aiTotalResources[(int)eLoopResource] += getPatentResourceCost(ePatent, eLoopResource);
                                        }

                                        break;
                                    }

                                case OrderType.RESEARCH:
                                    {
                                        TechnologyType eTechnology = (TechnologyType)(pLoopOrderInfo.miData1);

                                        for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                        {
                                            aiTotalResources[(int)eLoopResource] += getResearchResourceCost(eTechnology, (TechnologyLevelType)(pLoopOrderInfo.miData2), eLoopResource);
                                        }

                                        break;
                                    }

                                case OrderType.LAUNCH:
                                    {
                                        ResourceType eResource = (ResourceType)(pLoopOrderInfo.miData1);

                                        for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                        {
                                            aiTotalResources[(int)eLoopResource] += getLaunchResourceCost(eLoopResource, eResource);
                                        }

                                        break;
                                    }
                            }
                        }
                    }
                }

                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    aiTotalResources[(int)eLoopResource] += getWholeResourceStockpile(eLoopResource, false);

                    if (infos().resource(eLoopResource).mbTrade)
                    {
                        for (int i = 0; i < getNumConstructions(); i++)
                        {
                            ConstructionClient pLoopConstruction = gameClient().constructionClient(getConstructionList()[i]);

                            if (!(pLoopConstruction.isWasDamaged()))
                            {
                                aiTotalResources[(int)eLoopResource] += getBuildingResourceCost(pLoopConstruction.getType(), eLoopResource);
                            }
                        }

                        iCash += gameClient().marketClient().calculateSellRevenue(eLoopResource, aiTotalResources[(int)eLoopResource], gameClient().marketClient().getMinPrice(eLoopResource) / Constants.PRICE_MULTIPLIER);
                    }
                }

                return iCash;
            }
        }

        public virtual int calculateHQResourceValue()
        {
            int iResult = 0;

            for (HQLevelType eLevel = (HQLevelType)1; eLevel < getHQLevel(); eLevel++)
            {
                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    iResult += (getUpgradeResourceCost(eLoopResource, eLevel) * infos().resource(eLoopResource).miMarketPrice * 2);
                }
            }

            return iResult;
        }

        public virtual int calculateBuildingResourceValue()
        {
            int iResult = 0;

            for (int i = 0; i < getNumBuildings(); i++)
            {
                BuildingClient pLoopBuilding = gameClient().buildingClient(getBuildingList()[i]);
                TileClient pTile = pLoopBuilding.tileClient();

                BuildingType eBuilding = ((pTile.isShowWrongBuilding()) ? pTile.getVisibleBuilding() : BuildingType.NONE);

                if (eBuilding == BuildingType.NONE)
                {
                    eBuilding = pLoopBuilding.getType();
                }

                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    iResult += (getBuildingResourceCost(eBuilding, eLoopResource) * infos().resource(eLoopResource).miMarketPrice * 2);
                }
            }

            for (int i = 0; i < getNumConstructions(); i++)
            {
                ConstructionClient pLoopConstruction = gameClient().constructionClient(getConstructionList()[i]);

                if (pLoopConstruction.isWasDamaged())
                {
                    for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                    {
                        iResult += (getBuildingResourceCost(pLoopConstruction.getType(), eLoopResource) * infos().resource(eLoopResource).miMarketPrice * 2);
                    }
                }
            }

            return iResult;
        }

        public virtual int calculateHQValue()
        {
            int iValue = GameClient.calculateHQValueBase(Math.Max(1, getHQLevelInt()), infos());

            if (!(gameClient().isCampaign()))
            {
                iValue *= ((Constants.MAX_NUM_PLAYERS * 2) - (int)(gameClient().getNumPlayers()));
                iValue /= (Constants.MAX_NUM_PLAYERS + 2);
            }

            return iValue;
        }

        public virtual int calculateStructuresValue()
        {
            int iValue = 0;

            iValue += calculateHQResourceValue();
            iValue += calculateBuildingResourceValue();

            return iValue;
        }

        public static int getModuleValue(int iShares, Infos pInfos)
        {
            return (iShares * pInfos.Globals.COLONY_SHARE_PRICE_BASE);
        }

        public virtual int calculateModulesValue()
        {
            return getModuleValue(getColonySharesOwned(), infos());
        }

        public virtual long calculateTotalStockValue(int iDebt)
        {
            long iValue = 0;

            iValue += calculateHQValue();

            iValue += getCashResourceValue();

            iValue += calculateStructuresValue();

            iValue += calculateModulesValue();

            iValue += (iDebt * ((iDebt < 0) ? infos().handicap(getHandicap()).miDebtMultiplier : 1));

            return iValue;

            //return Math.Max(1, iValue);
        }

        public virtual BondType calculateBondRating(int iDebt, int iExtraValue, ref int iBetterThreshold, ref int iWorseThreshold, ref int iExcessBond, bool bIgnoreAdjustment = false)
        {
            long iValue = calculateTotalStockValue(0);

            iValue += iExtraValue;

            if (iValue < 0)
            {
                return (infos().bondsNum() - 1);
            }

            BondType eBond = BondType.NONE;
            BondType eAdjustedBond = eBond;

            int iDivisor = 1000;

            for (int i = 0; i < (int)(infos().bondsNum()) + 19; i++)
            {
                int iThreshold = 0;

                if (eBond < infos().bondsNum())
                {
                    iThreshold = (int)(((long)iValue * infos().bond(eBond).miDebtPercent) / iDivisor);
                }
                else
                {
                    int iPercent = infos().bonds().Last().miDebtPercent;

                    iPercent += ((eBond - infos().bondsNum() + 1) * 15);

                    iThreshold = (int)(((long)iValue * iPercent) / iDivisor);
                }

                if (gameClient().getNumPlayers() > (PlayerType)(Constants.MAX_NUM_PLAYERS / 2))
                {
                    iThreshold *= ((int)(gameClient().getNumPlayers()) + Constants.MAX_NUM_PLAYERS);
                    iThreshold /= ((Constants.MAX_NUM_PLAYERS / 2) + Constants.MAX_NUM_PLAYERS);
                }

                if (iThreshold > 0)
                {
                    iThreshold += (1000 - (iThreshold % 1000));
                }

                iWorseThreshold = iThreshold;

                if (-(iDebt) <= iThreshold)
                {
                    break;
                }

                if (eAdjustedBond < (infos().bondsNum() - 1))
                {
                    iBetterThreshold = iThreshold;
                }

                eBond++;

                if (bIgnoreAdjustment)
                {
                    eAdjustedBond = eBond;
                }
                else
                {
                    eAdjustedBond = eBond + getBondRatingChange();
                }
            }

            if (eAdjustedBond < 0)
            {
                eAdjustedBond = 0;
            }
            else if (eAdjustedBond >= infos().bondsNum())
            {
                iExcessBond = (eAdjustedBond - (infos().bondsNum() - 1));

                eAdjustedBond = (infos().bondsNum() - 1);
            }

            return eAdjustedBond;
        }
        public virtual BondType calculateBondRatingFound()
        {
            int iBetterThreshold = 0;
            int iWorseThreshold = 0;
            int iExcessBond = 0;
            return calculateBondRating(gameClient().getFoundMoney(), infos().handicap(getHandicap()).miMoney, ref iBetterThreshold, ref iWorseThreshold, ref iExcessBond);
        }

        public virtual GenderType getGender()
        {
            return meGender;
        }

        public virtual bool isShowRealName(TeamType eActiveTeam, bool bForceReal = false)
        {
            return ( (bForceReal) ||
                     (Globals.AppInfo.IsObserver) ||
                    !(isWinEligible()) ||
                    !(gameClient().isGameOption(GameOptionType.HIDDEN_IDENTITIES)) ||
                     (gameClient().isGameOver()) ||
                     (eActiveTeam == getTeam()) ||
                     (gameClient().countTeamsWinEligible() <= 2));
        }

        public virtual TextVariable getNameText(TeamType eActiveTeam, bool bNoSuffix = false, bool bForceReal = false)
        {
            string zOutputName = (isShowRealName(eActiveTeam, bForceReal)) ? mzName : mzPseudonym;

            if (!string.IsNullOrEmpty(mzSuffix) && !bNoSuffix)
            {
                zOutputName += mzSuffix;
            }

            return zOutputName.ToText(meGender);
        }
        public virtual string getRealName()
        {
            return mzName;
        }
        public virtual string getPseudonym()
        {
            return mzPseudonym;
        }

        public virtual string getRank()
        {
            return mzRank;
        }

        public virtual long getMoney(bool bShared = false)
        {
            if (bShared && gameClient().isTeamGame() && !isSubsidiary())
            {
                long iMoney = 0;

                for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameClient().getNumPlayers(); eLoopPlayer++)
                {
                    PlayerClient pLoopPlayer = gameClient().playerClient(eLoopPlayer);

                    if (!(pLoopPlayer.isSubsidiary()))
                    {
                        if (pLoopPlayer.getTeam() == getTeam())
                        {
                            iMoney += pLoopPlayer.getMoney();
                        }
                    }
                }

                return iMoney;
            }
            else
            {
                return miMoney;
            }
        }

        public virtual int getDebt()
        {
            return miDebt;
        }

        public virtual int getExcessBond()
        {
            return miExcessBond;
        }

        public virtual int getClaims()
        {
            return miClaims;
        }

        public virtual int getEntertainment()
        {
            return miEntertainment;
        }

        public virtual int getBaseSharePrice()
        {
            return miBaseSharePrice;
        }
        public virtual int getDemandModifier(int iSharesAvailableChange)
        {
            const int DEMAND_MODIFIER_MULTIPLIER = 5;

            int iSharesBought = getSharesBought() - iSharesAvailableChange;
            int iStartingShares = getStartingShares(false);

            if (iSharesBought > iStartingShares)
            {
                if (iSharesBought == gameClient().getInitialShares())
                {
                    return (iSharesBought * DEMAND_MODIFIER_MULTIPLIER);
                }
                else
                {
                    return ((iSharesBought - iStartingShares) * DEMAND_MODIFIER_MULTIPLIER);
                }
            }
            else
            {
                return 0;
            }
        }
        public virtual int calculateBaseSharePrice(int iDebt, bool bDampen)
        {
            long iTotalValue = calculateTotalStockValue(iDebt);

            if (gameClient().isGameOption(GameOptionType.MARATHON_MODE))
            {
                iTotalValue *= 3;
                iTotalValue /= 2;
            }

            const int STOCK_STEPS = (1000 * Constants.STOCK_MULTIPLIER);

            long iBaseSharePrice = 0;

            if ((iTotalValue > STOCK_STEPS) && bDampen)
            {
                iBaseSharePrice = STOCK_STEPS;
                iTotalValue -= STOCK_STEPS;

                while (iTotalValue > 0)
                {
                    long iChange = Math.Min(STOCK_STEPS, iTotalValue);

                    iBaseSharePrice += iChange;
                    iTotalValue -= iChange;

                    iTotalValue *= 3;
                    iTotalValue /= 4;
                }
            }
            else
            {
                iBaseSharePrice = iTotalValue;
            }

            iBaseSharePrice /= 100;

            //iBaseSharePrice = Math.Max(iBaseSharePrice, 0);

            return (int)iBaseSharePrice;
        }
        public virtual int getStocksOwnedPrice()
        {
            int iValue = 0;

            int iDivisor = gameClient().getInitialShares();

            if (gameClient().isCampaign())
            {
                for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameClient().getNumPlayers(); eLoopPlayer++)
                {
                    iValue += Math.Max(0, ((getSharesOwned(eLoopPlayer) * Globals.Campaign.calculateSharePrice(gameClient().playerClient(eLoopPlayer).corporation())) / iDivisor));
                }
            }
            else
            {
                for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameClient().getNumPlayers(); eLoopPlayer++)
                {
                    if (eLoopPlayer != getPlayer())
                    {
                        iValue += Math.Max(0, ((getSharesOwned(eLoopPlayer) * gameClient().playerClient(eLoopPlayer).getBaseSharePrice()) / iDivisor));
                    }
                }
            }

            return iValue;
        }
        public virtual int calculateSharePrice(int iSharesAvailableChange, int iBasePrice)
        {
            int iPrice = iBasePrice;

            if (iPrice > 0)
            {
                iPrice *= Math.Max(0, infos().handicap(getHandicap()).miBaseShareModifier + 100);
                iPrice /= 100;

                if (gameClient().isSevenSols())
                {
                    iPrice *= (getColonySharesOwned() * infos().Globals.COLONY_SHARE_STOCK_MODIFIER) + 100;
                    iPrice /= 100;
                }
                else
                {
                    iPrice *= Math.Max(0, getDemandModifier(iSharesAvailableChange) + 100);
                    iPrice /= 100;
                }
            }

            iPrice += getStocksOwnedPrice();

            iPrice = Math.Max(iPrice, Constants.STOCK_MULTIPLIER);

            return iPrice;
        }
        public virtual int getSharePriceCampaign(int iDebt)
        {
            long iValue = Globals.Campaign.calculateShareValue(corporation());

            iValue += calculateTotalStockValue(iDebt);

            iValue /= Campaign.STOCK_DIVISOR;

            iValue += getStocksOwnedPrice();

            return (int)Math.Max(iValue, Constants.STOCK_MULTIPLIER);
        }
        public virtual int getSharePrice(int iSharesAvailableChange = 0)
        {
            if (gameClient().isCampaign())
            {
                return getSharePriceCampaign(getDebt());
            }
            else
            {
                return calculateSharePrice(iSharesAvailableChange, getBaseSharePrice());
            }
        }

        public virtual int getSharesAvailable()
        {
            return miSharesAvailable;
        }
        public virtual int getSharesBought()
        {
            return (gameClient().getInitialShares() - getSharesAvailable());
        }
        public virtual int getSharesBoughtRivals()
        {
            return (getSharesBought() - countOwnSharesOwned());
        }

        public virtual int getColonySharesOwned()
        {
            return miColonySharesOwned;
        }

        public virtual int getStartTileID()
        {
            return miStartTileID;
        }
        public virtual TileClient startTileClient()
        {
            return gameClient().tileClient(getStartTileID());
        }

        public virtual int getScans()
        {
            return miScans;
        }

        public virtual int getScanTime()
        {
            return miScanTime;
        }

        public virtual int getVisibleTilesNum()
        {
            return miVisibleTiles;
        }
        public virtual int getMapRevealedPercent()
        {
            if (gameClient().mapClient().numTiles() > 0)
            {
                return ((getVisibleTilesNum() * 100) / gameClient().mapClient().numTiles());
            }
            else
            {
                return 0;
            }
        }
        public virtual bool isMapRevealed()
        {
            return (getVisibleTilesNum() == gameClient().mapClient().numTiles());
        }

        protected virtual int getManualSaleTime()
        {
            return miManualSaleTime;
        }
        public virtual bool isManualSale()
        {
            return (getManualSaleTime() > 0);
        }

        public virtual int getBlackMarketTime()
        {
            return miBlackMarketTime;
        }

        public virtual int getSabotagedCount()
        {
            return miSabotagedCount;
        }

        public virtual int getEspionageShortageCount()
        {
            return miEspionageShortageCount;
        }

        public virtual int getEspionageSurplusCount()
        {
            return miEspionageSurplusCount;
        }

        public virtual int getBondRatingChange()
        {
            return miBondRatingChange;
        }

        public virtual int getDebtCut()
        {
            return miDebtCut;
        }

        public virtual int getAutoPayDebtTarget()
        {
            return miAutoPayDebtTarget;
        }

        public virtual int getEntertainmentModifier()
        {
            return miEntertainmentModifier;
        }

        public virtual int getPowerConsumptionModifier()
        {
            return miPowerConsumptionModifier;
        }

        public virtual int getConnectedHQPowerProductionModifier()
        {
            return miConnectedHQPowerProductionModifier;
        }

        public virtual int getAdjacentHQSabotageModifier()
        {
            return miAdjacentHQSabotageModifier;
        }

        public virtual int getInterestModifier()
        {
            return miInterestModifier;
        }

        public virtual int getFinalMarketCap()
        {
            return miFinalMarketCap;
        }
        public virtual int getFinalPrice()
        {
            return (getFinalMarketCap() * Constants.STOCK_MULTIPLIER) / (infos().Globals.SHARE_PURCHASE_SIZE * gameClient().getInitialShares());
        }

        public virtual int getHighestBuyoutTenths()
        {
            return miHighestBuyoutTenths;
        }

        public virtual int getNumCampaignLeads()
        {
            return miNumCampaignLead;
        }

        public virtual int getNumCampaignTiePlayer()
        {
            return miNumCampaignTiePlayer;
        }

        public virtual int getScore()
        {
            return miScore;
        }

        public virtual bool isHuman()
        {
            return mbHuman;
        }
        public virtual bool isHumanTeam()
        {
            for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameClient().getNumPlayers(); eLoopPlayer++)
            {
                PlayerClient pLoopPlayer = gameClient().playerClient(eLoopPlayer);

                if (pLoopPlayer.getTeam() == getTeam())
                {
                    if (pLoopPlayer.isHuman())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public virtual bool wasEverHuman()
        {
            return mbEverHuman;
        }

        public virtual bool isConcede()
        {
            return mbConcede;
        }
        public virtual bool isDropped()
        {
            return mbDropped;
        }
        public virtual bool isSubsidiary()
        {
            return mbSubsidiary;
        }
        public virtual bool isWinEligible()
        {
            if (isSubsidiary())
            {
                return false;
            }

            if (isConcede())
            {
                return false;
            }

            if (gameClient().isOriginalAllHumans())
            {
                if (isDropped())
                {
                    return false;
                }
            }

            return true;
        }
        public virtual bool isDeleteOrders()
        {
            if (isSubsidiary())
            {
                return true;
            }

            if (isConcede() || isDropped())
            {
                if (gameClient().countTeamsWinEligible() > 1)
                {
                    return true;
                }
            }

            return false;
        }

        public virtual bool isHQFounded()
        {
            return mbHQFounded;
        }

        public virtual bool isSkipAuction()
        {
            return mbSkipAuction;
        }

        public virtual bool isBorehole()
        {
            return mbBorehole;
        }

        public virtual bool isRecycleScrap()
        {
            return mbRecycleScrap;
        }

        public virtual bool isAdjacentMining()
        {
            return mbAdjacentMining;
        }

        public virtual bool isTeleportation()
        {
            return mbTeleportation;
        }

        public virtual bool isCaveMining()
        {
            return mbCaveMining;
        }

        public virtual bool isGeothermalDiscovered()
        {
            return mbGeothermalDiscovered;
        }

        public virtual bool isAutoPayDebt()
        {
            return mbAutoPayDebt;
        }

        public virtual bool isBeatSoren()
        {
            return mbBeatSoren;
        }

        public virtual bool isIsSoren()
        {
            return mbIsSoren;
        }

        public virtual bool isBeatZultar()
        {
            return mbBeatZultar;
        }

        public virtual bool isIsZultar()
        {
            return mbIsZultar;
        }

        public virtual PlayerType getPlayer()
        {
            return mePlayer;
        }

        public virtual TeamType getTeam()
        {
            return meTeam;
        }
        public virtual IEnumerable<PlayerClient> getTeammatesAll()
        {
            for (PlayerType ePlayer = 0; ePlayer < gameClient().getNumPlayers(); ePlayer++)
            {
                if (gameClient().playerClient(ePlayer).getTeam() == getTeam())
                {
                    yield return gameClient().playerClient(ePlayer);
                }
            }
        }
        public virtual IEnumerable<PlayerClient> getAliveTeammatesAll()
        {
            return getTeammatesAll().Where(o => !(o.isSubsidiary()));
        }
        public virtual int getNumTeammates()
        {
            return getTeammatesAll().Count();
        }
        public virtual int countTeammatesAlive()
        {
            int iCount = 0;

            for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameClient().getNumPlayers(); eLoopPlayer++)
            {
                PlayerClient pLoopPlayer = gameClient().playerClient(eLoopPlayer);

                if (!(pLoopPlayer.isSubsidiary()))
                {
                    if (pLoopPlayer.getTeam() == getTeam())
                    {
                        iCount++;
                    }
                }
            }

            return iCount;
        }

        public virtual PersonalityType getPersonality()
        {
            return mePersonality;
        }

        public virtual CharacterType getCharacter()
        {
            return infos().personality(getPersonality()).meCharacter;
        }

        public virtual CorporationType getCorporation()
        {
            return meCorporation;
        }
        public virtual Corporation corporation()
        {
            if (getCorporation() != CorporationType.NONE)
            {
                return Globals.Campaign.getCorporation(getCorporation());
            }
            else
            {
                return null;
            }
        }

        public virtual HQType getHQ()
        {
            return meHQ;
        }

        public virtual HQLevelType getHQLevel()
        {
            return meHQLevel;
        }
        public virtual int getHQLevelInt()
        {
            return (int)getHQLevel();
        }
        public virtual bool isMaxUpgrade()
        {
            if (getHQLevel() < (infos().HQLevelsNum() - 1))
            {
                return false;
            }

            return true;
        }

        public virtual HQLevelType getHQClaimBonus()
        {
            return meHQClaimBonus;
        }

        public virtual BondType getBondRating()
        {
            return meBondRating;
        }
        public virtual int getInterestRate(BondType eBond = BondType.NONE)
        {
            int iRate = (infos().bond((eBond == BondType.NONE) ? getBondRating() : eBond).miInterestRate + (getExcessBond() * infos().Globals.EXCESS_BOND_INTEREST));
            if (iRate > 0)
            {
                iRate *= Math.Max(0, (infos().location(gameClient().getLocation()).miInterestRateModifier + 100));
                iRate /= 100;

                iRate *= Math.Max(0, (getInterestModifier() + 100));
                iRate /= 100;

                return Math.Max(1, iRate);
            }
            else
            {
                return 0;
            }
        }

        public virtual ResourceType getEnergyResource()
        {
            return meEnergyResource;
        }

        public virtual ResourceType getGasResource()
        {
            return meGasResource;
        }

        public virtual ResourceType getLaunchResource()
        {
            return meLaunchResource;
        }

        public virtual HandicapType getHandicap()
        {
            return meHandicap;
        }

        public virtual PlayerColorType getPlayerColorType()
        {
            return meColor;
        }

        public virtual PlayerType getBoughtByPlayer()
        {
            return meBoughtByPlayer;
        }

        public virtual int getResourceStockpile(ResourceType eIndex, bool bShared)
        {
            if (bShared && gameClient().isTeamGame() && !isSubsidiary() && infos().resource(eIndex).mbTrade)
            {
                int iStockpile = 0;

                for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameClient().getNumPlayers(); eLoopPlayer++)
                {
                    PlayerClient pLoopPlayer = gameClient().playerClient(eLoopPlayer);

                    if (!(pLoopPlayer.isSubsidiary()))
                    {
                        if (pLoopPlayer.getTeam() == getTeam())
                        {
                            if (pLoopPlayer.getPlayer() == getPlayer())
                            {
                                iStockpile += pLoopPlayer.getResourceStockpile(eIndex, false);
                            }
                            else if (pLoopPlayer.isTeamShareResource(eIndex))
                            {
                                iStockpile += Math.Max(0, pLoopPlayer.getResourceStockpile(eIndex, false));
                            }
                        }
                    }
                }

                return iStockpile;
            }
            else
            {
                return maiResourceStockpile[(int)eIndex];
            }
        }
        public virtual int getAvailableResourceStockpile(ResourceType eIndex, bool bShared)
        {
            return getWholeResourceStockpile(eIndex, bShared);
        }
        public virtual int getWholeResourceStockpile(ResourceType eIndex, bool bShared)
        {
            return getResourceStockpile(eIndex, bShared) / Constants.RESOURCE_MULTIPLIER;
        }
        public virtual int getNeededResource(ResourceType eIndex, int iQuantity)
        {
            int iDiff = iQuantity;

            int iStockpile = getResourceStockpile(eIndex, true);

            iDiff -= (iStockpile / Constants.RESOURCE_MULTIPLIER);

            if (iStockpile < 0)
            {
                iDiff--;
            }

            return Math.Max(0, iDiff);
        }
        public virtual int getNeededResourceCost(ResourceType eIndex, int iQuantity)
        {
            int iDiff = getNeededResource(eIndex, iQuantity);

            if (iDiff > 0)
            {
                return gameClient().marketClient().calculateBuyCost(eIndex, iDiff);
            }
            else
            {
                return 0;
            }
        }
        public virtual int getIgnoreStockpileCost(ResourceType eResource, int iQuantity)
        {
            int iCost = 0;

            if (iQuantity > 0)
            {
                int iResourceDiff = getNeededResource(eResource, iQuantity);

                iCost += gameClient().marketClient().calculateSellRevenue(eResource, (iQuantity - iResourceDiff), 1);
                iCost += gameClient().marketClient().calculateBuyCost(eResource, iResourceDiff);
            }

            return iCost;
        }

        public virtual int getResourceExtraStockpile(ResourceType eIndex)
        {
            return maiResourceExtraStockpile[(int)eIndex];
        }
        public virtual int getWholeResourceExtraStockpile(ResourceType eIndex)
        {
            return getResourceExtraStockpile(eIndex) / Constants.RESOURCE_MULTIPLIER;
        }
        public virtual int getResourceExtraCapacity(ResourceType eIndex)
        {
            return maiResourceExtraCapacity[(int)eIndex];
        }
        public virtual int getBackupRate()
        {
            return (Constants.RESOURCE_MULTIPLIER * Constants.BACKUP_RATE);
        }
        public virtual int getStockpileExtraRate(ResourceType eResource)
        {
            int iDiff = (getResourceExtraCapacity(eResource) - getResourceExtraStockpile(eResource));

            if (iDiff > 0)
            {
                return Math.Min(iDiff, getBackupRate());
            }

            return 0;
        }

        protected virtual int getResourceAutoTrade(ResourceType eIndex)
        {
            return maiResourceAutoTrade[(int)eIndex];
        }

        public virtual int getResourceRate(ResourceType eIndex)
        {
            return maiResourceRate[(int)eIndex];
        }
        public virtual int getResourceShortfall(ResourceType eIndex)
        {
            return maiResourceShortfall[(int)eIndex];
        }
        public virtual int getResourceRateAndShortfall(ResourceType eIndex)
        {
            return (getResourceRate(eIndex) + getResourceShortfall(eIndex));
        }

        public virtual int getResourceInput(ResourceType eIndex)
        {
            return maiResourceInput[(int)eIndex];
        }

        public virtual int getColonyConsumption(ResourceType eIndex)
        {
            return maiColonyConsumption[(int)eIndex];
        }

        public virtual int getColonyConsumptionModifier(ResourceType eIndex)
        {
            return maiColonyConsumptionModifier[(int)eIndex];
        }

        public virtual int getColonyPayment(ResourceType eIndex)
        {
            return maiColonyPayments[(int)eIndex];
        }

        public virtual int getResourceOutput(ResourceType eIndex)
        {
            return maiResourceOutput[(int)eIndex];
        }

        public virtual int getResourceAutoPurchased(ResourceType eIndex)
        {
            return maiResourceAutoPurchased[(int)eIndex];
        }
        public virtual int getResourceAutoSold(ResourceType eIndex)
        {
            return maiResourceAutoSold[(int)eIndex];
        }
        public virtual int getResourceAutoTraded(ResourceType eIndex)
        {
            return getResourceAutoSold(eIndex) - getResourceAutoPurchased(eIndex);
        }

        public virtual int getResourceProductionModifier(ResourceType eIndex)
        {
            return maiResourceProductionModifier[(int)eIndex];
        }

        public virtual int getUpgradeResourceCost(ResourceType eIndex)
        {
            return maiUpgradeResourceCost[(int)eIndex];
        }

        public virtual int getBlackMarketCount(BlackMarketType eIndex)
        {
            return maiBlackMarketCount[(int)eIndex];
        }

        public virtual int getSabotageCount(SabotageType eIndex)
        {
            return maiSabotageCount[(int)eIndex];
        }

        public virtual int getRealConstructionCount(BuildingType eIndex)
        {
            return maiRealConstructionCount[(int)eIndex];
        }
        public virtual int getRealConstructionCountEntertainment()
        {
            int iCount = 0;

            for (BuildingType eLoopBuilding = 0; eLoopBuilding < infos().buildingsNum(); eLoopBuilding++)
            {
                if (infos().building(eLoopBuilding).miEntertainment > 0)
                {
                    iCount += getRealConstructionCount(eLoopBuilding);
                }
            }

            return iCount;
        }
        public virtual bool isRealConstructionAny()
        {
            for (BuildingType eLoopBuilding = 0; eLoopBuilding < infos().buildingsNum(); eLoopBuilding++)
            {
                if (getRealConstructionCount(eLoopBuilding) > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public virtual int getBuildingCount(BuildingType eIndex)
        {
            return maiBuildingCount[(int)eIndex];
        }
        public virtual int getBuildingCountEntertainment()
        {
            int iCount = 0;

            for (BuildingType eLoopBuilding = 0; eLoopBuilding < infos().buildingsNum(); eLoopBuilding++)
            {
                if (infos().building(eLoopBuilding).miEntertainment > 0)
                {
                    iCount += getBuildingCount(eLoopBuilding);
                }
            }

            return iCount;
        }

        public virtual int getRealBuildingCount(BuildingType eIndex)
        {
            return maiRealBuildingCount[(int)eIndex];
        }
        public virtual int getRealBuildingCountEntertainment()
        {
            int iCount = 0;

            for (BuildingType eLoopBuilding = 0; eLoopBuilding < infos().buildingsNum(); eLoopBuilding++)
            {
                if (infos().building(eLoopBuilding).miEntertainment > 0)
                {
                    iCount += getRealBuildingCount(eLoopBuilding);
                }
            }

            return iCount;
        }

        public virtual int getBuildingClassInputModifier(BuildingClassType eIndex)
        {
            return maiBuildingClassInputModifier[(int)eIndex];
        }

        public virtual int getBuildingClassLevel(BuildingClassType eIndex)
        {
            return maiBuildingClassLevel[(int)eIndex];
        }

        public virtual int getHQUnlockCount(HQType eIndex)
        {
            return maiHQUnlockCount[(int)eIndex];
        }
        public virtual bool isHQUnlock(HQType eIndex)
        {
            return (getHQUnlockCount(eIndex) > 0);
        }

        public virtual int getOrderCapacity(OrderType eIndex)
        {
            return maiOrderCapacity[(int)eIndex];
        }

        public virtual int getOrderCostModifier(OrderType eIndex)
        {
            return maiOrderCostModifier[(int)eIndex];
        }

        public virtual int getOrderRateModifier(OrderType eIndex)
        {
            return maiOrderRateModifier[(int)eIndex];
        }

        public virtual int getSharesOwned(PlayerType eIndex)
        {
            return maiSharesOwned[(int)eIndex];
        }
        public virtual int countTotalSharesOwned()
        {
            int iCount = 0;

            for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameClient().getNumPlayers(); eLoopPlayer++)
            {
                iCount += getSharesOwned(eLoopPlayer);
            }

            return iCount;
        }
        public virtual int countOwnSharesOwned()
        {
            int iCount = 0;

            for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameClient().getNumPlayers(); eLoopPlayer++)
            {
                PlayerClient pLoopPlayer = gameClient().playerClient(eLoopPlayer);

                if (pLoopPlayer.getTeam() == getTeam())
                {
                    iCount += pLoopPlayer.getSharesOwned(getPlayer());
                }
            }

            return iCount;
        }
        public virtual StockState getStockState(TeamType eActiveTeam, bool bObserver)
        {
            if (infos().rulesSet(gameClient().getRulesSet()).mbNoStockMarket)
            {
                return StockState.PROTECTED;
            }

            if ((getTeam() == eActiveTeam) || bObserver)
            {
                foreach (PlayerClient pLoopPlayer in gameClient().getPlayerClientAliveAll())
                {
                    int iPercent = pLoopPlayer.getBuyoutPercent(getPlayer());
                    if (iPercent == 100)
                    {
                        return StockState.IMMEDIATE_DANGER;
                    }
                    else if (iPercent >= 80)
                    {
                        return StockState.DANGER;
                    }
                }
            }

            if (getSharesAvailable() > 0)
            {
                if (getSharesBoughtRivals() == gameClient().getMajorityShares())
                {
                    return StockState.DANGER;
                }
            }

            if (countOwnSharesOwned() >= gameClient().getMajorityShares())
            {
                return StockState.PROTECTED;
            }

            return StockState.UNPROTECTED;
        }

        public virtual int getStockDelay(PlayerType eIndex)
        {
            return maiStockDelay[(int)eIndex];
        }

        public virtual int getBuyDelay(PlayerType eIndex)
        {
            return maiBuyDelay[(int)eIndex];
        }

        public virtual int getSellDelay(PlayerType eIndex)
        {
            return maiSellDelay[(int)eIndex];
        }

        public virtual int getDividendTotal(PlayerType eIndex)
        {
            return maiDividendTotal[(int)eIndex];
        }

        public virtual int getPerkCount(PerkType eIndex)
        {
            return maiPerkCount[(int)eIndex];
        }

        public virtual bool isPlayerOption(PlayerOptionType eIndex)
        {
            return mabPlayerOptions[(int)eIndex];
        }

        public virtual bool isAutoSupplyBuildingsInput(BuildingType eIndex)
        {
            return mabAutoSupplyBuildingInput[(int)eIndex];
        }

        public virtual bool isBuildingAlwaysOn(BuildingType eIndex)
        {
            return mabBuildingsAlwaysOn[(int)eIndex];
        }

        public virtual bool isBuildingImmune(BuildingType eIndex)
        {
            return mabBuildingImmune[(int)eIndex];
        }

        public virtual bool isBuildingDestroyEvent(BuildingType eIndex)
        {
            return mabBuildingDestroyEvent[(int)eIndex];
        }

        public virtual bool isBuildingClassBoost(BuildingClassType eIndex)
        {
            return mabBuildingClassBoost[(int)eIndex];
        }

        public virtual bool isPatentStarted(PatentType eIndex)
        {
            return mabPatentStarted[(int)eIndex];
        }

        public virtual bool isPatentAcquiredLab(PatentType eIndex)
        {
            return mabPatentAcquiredLab[(int)eIndex];
        }
        public virtual bool isPatentAcquiredPerk(PatentType eIndex)
        {
            return mabPatentAcquiredPerk[(int)eIndex];
        }
        public virtual bool isPatentAcquired(PatentType eIndex)
        {
            return (isPatentAcquiredLab(eIndex) || isPatentAcquiredPerk(eIndex));
        }

        public virtual bool isAutoCraftBlackMarket(BlackMarketType eBlackMarket) { return false; }

        public virtual bool isResourceNoBuy(ResourceType eIndex)
        {
            return mabResourceNoBuy[(int)eIndex];
        }

        public virtual bool isResourceNoSell(ResourceType eIndex)
        {
            return mabResourceNoSell[(int)eIndex];
        }

        public virtual bool isBuildingFreeResource(ResourceType eIndex)
        {
            return mabBuildingFreeResource[(int)eIndex];
        }

        public virtual bool isAlternateGasResource(ResourceType eIndex)
        {
            return mabAlternateGasResource[(int)eIndex];
        }

        public virtual bool isAlternatePowerResourcePatent(ResourceType eIndex)
        {
            return mabAlternatePowerResourcePatent[(int)eIndex];
        }
        public virtual bool isAlternatePowerResourcePerk(ResourceType eIndex)
        {
            return mabAlternatePowerResourcePerk[(int)eIndex];
        }
        public virtual bool isAlternatePowerResource(ResourceType eIndex)
        {
            return isAlternatePowerResourcePatent(eIndex) || isAlternatePowerResourcePerk(eIndex);
        }

        public virtual bool isHoldResource(ResourceType eIndex)
        {
            return mabHoldResource[(int)eIndex];
        }

        public virtual bool isAutoLaunchResource(ResourceType eIndex)
        {
            return mabAutoLaunchResource[(int)eIndex];
        }

        public virtual bool mustTeamShareResource(ResourceType eIndex)
        {
            return !(infos().resource(eIndex).mbTrade);
        }
        public virtual bool isTeamShareResource(ResourceType eIndex)
        {
            return mabTeamShareResource[(int)eIndex];
        }

        public virtual PlayerType getPlayerList(int iIndex)
        {
            return maePlayerList[iIndex];
        }

        public virtual ResourceType getResourceReplace(ResourceType eIndex)
        {
            return maeResourceReplace[(int)eIndex];
        }

        public virtual ResourceLevelType getResourceLevelDiscovered(ResourceType eIndex)
        {
            return maeResourceLevelDiscovered[(int)eIndex];
        }

        public virtual ResourceLevelType getMinimumMining(BuildingClassType eIndex)
        {
            return maeMinimumMining[(int)eIndex];
        }

        public virtual List<int> getFreeBuildings()
        {
            return maiFreeBuildings;
        }

        public virtual TechnologyLevelType getTechnologyLevelResearching(TechnologyType eIndex)
        {
            return maeTechnologyLevelResearching[(int)eIndex];
        }

        public virtual TechnologyLevelType getTechnologyLevelDiscovered(TechnologyType eIndex)
        {
            return maeTechnologyLevelDiscovered[(int)eIndex];
        }

        public virtual ArtPackList getArtPackList()
        {
            return maeArtPackList;
        }

        public virtual int getBuildingResourceCost(BuildingType eIndex1, ResourceType eIndex2)
        {
            return maaiBuildingResourceCost[(int)eIndex1][(int)eIndex2];
        }
        public virtual List<int> getBuildingResourceCost(BuildingType eIndex)
        {
            return maaiBuildingResourceCost[(int)eIndex];
        }

        public virtual bool isResourceReplaceValid(ResourceType eIndex1, ResourceType eIndex2)
        {
            return maabResourceReplaceValid[(int)eIndex1][(int)eIndex2];
        }

        public virtual bool isIgnoreInputIce(BuildingClassType eIndex1, IceType eIndex2)
        {
            return maabIgnoreInputIce[(int)eIndex1][(int)eIndex2];
        }

        public virtual List<int> getTileList()
        {
            return maTiles;
        }
        public virtual int getNumTiles()
        {
            return getTileList().Count;
        }

        public virtual List<int> getHQList()
        {
            return maHQs;
        }
        public virtual int getNumHQs()
        {
            return getHQList().Count;
        }
        public virtual HQClient startingHQClient()
        {
            return (getNumHQs() > 0) ? gameClient().hqClient(getHQList().First()) : null;
        }

        public virtual List<int> getConstructionList()
        {
            return maConstructions;
        }
        public virtual int getNumConstructions()
        {
            return getConstructionList().Count;
        }

        public virtual List<int> getBuildingList()
        {
            return maBuildings;
        }
        public virtual int getNumBuildings()
        {
            return getBuildingList().Count;
        }

        public virtual List<int> getUnitList()
        {
            return maUnits;
        }
        public virtual int getNumUnits()
        {
            return getUnitList().Count;
        }

        public virtual bool canCancelOrder()
        {
            return !(infos().rulesSet(gameClient().getRulesSet()).mbNoCancelingOrders);
        }
        public virtual bool hasOrderInfos(OrderType eIndex)
        {
            return (maaOrderInfos[(int)eIndex].Count > 0);
        }
        public virtual LinkedList<OrderInfo> getOrderInfos(OrderType eIndex)
        {
            return maaOrderInfos[(int)eIndex];
        }
        public virtual OrderInfo getOrderInfo(OrderType eIndex, int iBuildingID)
        {
            foreach (OrderInfo pOrderInfo in getOrderInfos(eIndex))
            {
                if(pOrderInfo.miBuildingID == iBuildingID)
                {
                    return pOrderInfo;
                }
            }

            return null;
        }
        public virtual int countOrders(OrderType eIndex)
        {
            return maaOrderInfos[(int)eIndex].Count;
        }
        public virtual int countSurplusOrders()
        {
            int iCount = 0;

            foreach (OrderInfo pOrderInfo in getOrderInfos(OrderType.HACK))
            {
                if (infos().espionage((EspionageType)(pOrderInfo.miData1)).mbSurplus)
                {
                    iCount++;
                }
            }

            return iCount;
        }
        public virtual int countShortageOrders()
        {
            int iCount = 0;

            foreach (OrderInfo pOrderInfo in getOrderInfos(OrderType.HACK))
            {
                if (!(infos().espionage((EspionageType)(pOrderInfo.miData1)).mbSurplus))
                {
                    iCount++;
                }
            }

            return iCount;
        }
        protected virtual int getOrderIndexCount(OrderType eIndex, int iIndex)
        {
            int iCount = 0;

            if (hasOrderInfos(eIndex))
            {
                foreach (OrderInfo pOrderInfo in getOrderInfos(eIndex))
                {
                    if (pOrderInfo.miData1 == iIndex)
                    {
                        iCount++;
                    }
                }
            }

            return iCount;
        }
    }

    public partial class PlayerServer : PlayerClient
    {
        protected virtual GameServer gameServer()
        {
            return (GameServer)mGame;
        }

        protected virtual void makeDirty(PlayerDirtyType eType)
        {
            mDirtyBits.SetBit((int)eType, true);
        }
        protected virtual void makeAllDirty()
        {
            for (PlayerDirtyType eLoopType = 0; eLoopType < PlayerDirtyType.NUM_TYPES; eLoopType++)
            {
                makeDirty(eLoopType);
            }
        }
        public virtual void clearDirty()
        {
            mDirtyBits.Clear();
        }

        int miNextMessageID = 0;
        int miRealOrderBuildingCount = 0;
        int miAIForceConstruct = 0;
        int miAILastQuip = 0;

        bool mbAIUpdated = false;

        List<int> maiRealBuildingOrderCount = new List<int>();
        List<int> maiOrderInfosIndex = new List<int>();
        List<int> maiAIResourceRateAverage = new List<int>();
        List<int> maiAIResourceLifeSupport = new List<int>();
        List<int> maiAIResourceConstructionCount = new List<int>();
        List<int> maiAIResourceProductionCount = new List<int>();

        List<bool> mabCanProduceResource = new List<bool>();
        List<bool> mabAIForceOrder = new List<bool>();

        //cached temp variables to save allocations, do not serialize
        private int [] doTurnRevealDistances = null;
        private ArrayDeque<TileClient> doTurnRevealQueue = new ArrayDeque<TileClient>(1000);

        public PlayerServer(GameClient pGame)
            : base(pGame)
        {
        }

        public virtual void init(GameServer pGame, PlayerType ePlayer, TeamType eTeam, PersonalityType ePersonality, CorporationType eCorporation, HandicapType eHandicap, GenderType eGender, ArtPackList artPackList, bool bHuman, string zName, string zPseudonym)
        {
            mGame = pGame;
            mInfos = pGame.infos();

            mzName = zName;
            mzPseudonym = zPseudonym;

            miSharesAvailable = gameServer().getInitialShares();
            miScans = 0;

            mbHuman = bHuman;
            mbEverHuman = bHuman;

            mePlayer = ePlayer;
            meTeam = eTeam;
            mePersonality = ePersonality;
            meCorporation = eCorporation;
            meHandicap = eHandicap;
            meGender = eGender;
            maeArtPackList = new ArtPackList(artPackList); //copy list

            for (ResourceType eResource = 0; eResource < infos().resourcesNum(); eResource++)
            {
                maiResourceStockpile.Add(0);
                maiResourceExtraStockpile.Add(0);
                maiResourceExtraCapacity.Add(0);
                maiResourceAutoTrade.Add(0);
                maiResourceRate.Add(0);
                maiResourceShortfall.Add(0);
                maiResourceInput.Add(0);
                maiColonyConsumption.Add(0);
                maiColonyConsumptionModifier.Add(0);
                maiColonyPayments.Add(0);
                maiResourceOutput.Add(0);
                maiResourceAutoPurchased.Add(0);
                maiResourceAutoSold.Add(0);
                maiResourceProductionModifier.Add(0);
                maiUpgradeResourceCost.Add(0);
            }

            for (BlackMarketType eLoopBlackMarket = 0; eLoopBlackMarket < infos().blackMarketsNum(); eLoopBlackMarket++)
            {
                maiBlackMarketCount.Add(0);
            }

            for (SabotageType eLoopSabotage = 0; eLoopSabotage < infos().sabotagesNum(); eLoopSabotage++)
            {
                maiSabotageCount.Add(0);
            }

            for (BuildingType eLoopBuilding = 0; eLoopBuilding < infos().buildingsNum(); eLoopBuilding++)
            {
                maiRealConstructionCount.Add(0);
                maiBuildingCount.Add(0);
                maiRealBuildingCount.Add(0);
                maiFreeBuildings.Add(0);
            }

            for (BuildingClassType eLoopBuildingClass = 0; eLoopBuildingClass < infos().buildingClassesNum(); eLoopBuildingClass++)
            {
                maiBuildingClassInputModifier.Add(0);
                maiBuildingClassLevel.Add(0);
            }

            for (HQType eLoopHQ = 0; eLoopHQ < infos().HQsNum(); eLoopHQ++)
            {
                maiHQUnlockCount.Add(0);
            }

            for (OrderType eLoopOrder = 0; eLoopOrder < infos().ordersNum(); eLoopOrder++)
            {
                maiOrderCapacity.Add(0);
                maiOrderCostModifier.Add(0);
                maiOrderRateModifier.Add(0);
            }

            for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameServer().getNumPlayers(); eLoopPlayer++)
            {
                maiSharesOwned.Add(0);
                maiStockDelay.Add(0);
                maiBuyDelay.Add(0);
                maiSellDelay.Add(0);
                maiDividendTotal.Add(0);
            }

            for (PerkType eLoopPerk = 0; eLoopPerk < infos().perksNum(); eLoopPerk++)
            {
                maiPerkCount.Add(0);
            }

            mabPlayerOptions = Enumerable.Repeat(false, (int)(PlayerOptionType.NUM_TYPES)).ToList();

            mabAutoSupplyBuildingInput = Enumerable.Repeat(false, (int)infos().buildingsNum()).ToList();
            mabBuildingsAlwaysOn = Enumerable.Repeat(false, (int)infos().buildingsNum()).ToList();
            mabBuildingImmune = Enumerable.Repeat(false, (int)infos().buildingsNum()).ToList();
            mabBuildingDestroyEvent = Enumerable.Repeat(false, (int)infos().buildingsNum()).ToList();

            mabBuildingClassBoost = Enumerable.Repeat(false, (int)infos().buildingClassesNum()).ToList();

            mabPatentStarted = Enumerable.Repeat(false, (int)infos().patentsNum()).ToList();
            mabPatentAcquiredLab = Enumerable.Repeat(false, (int)infos().patentsNum()).ToList();
            mabPatentAcquiredPerk = Enumerable.Repeat(false, (int)infos().patentsNum()).ToList();

            mabResourceNoBuy = Enumerable.Repeat(false, (int)infos().resourcesNum()).ToList();
            mabResourceNoSell = Enumerable.Repeat(false, (int)infos().resourcesNum()).ToList();
            mabBuildingFreeResource = Enumerable.Repeat(false, (int)infos().resourcesNum()).ToList();
            mabAlternateGasResource = Enumerable.Repeat(false, (int)infos().resourcesNum()).ToList();
            mabAlternatePowerResourcePatent = Enumerable.Repeat(false, (int)infos().resourcesNum()).ToList();
            mabAlternatePowerResourcePerk = Enumerable.Repeat(false, (int)infos().resourcesNum()).ToList();
            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                mabHoldResource.Add(infos().resource(eLoopResource).mbTrade);
            }
            mabAutoLaunchResource = Enumerable.Repeat(false, (int)infos().resourcesNum()).ToList();
            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                mabTeamShareResource.Add(mustTeamShareResource(eLoopResource));
            }

            maePlayerList = Enumerable.Repeat(PlayerType.NONE, (int)gameServer().getNumPlayers()).ToList();
            maeResourceReplace = Enumerable.Repeat(ResourceType.NONE, (int)infos().resourcesNum()).ToList();
            maeResourceLevelDiscovered = Enumerable.Repeat(ResourceLevelType.NONE, (int)infos().resourcesNum()).ToList();
            maeMinimumMining = Enumerable.Repeat(ResourceLevelType.NONE, (int)infos().buildingClassesNum()).ToList();
            maeTechnologyLevelResearching = Enumerable.Repeat(TechnologyLevelType.NONE, (int)infos().technologiesNum()).ToList();
            maeTechnologyLevelDiscovered = Enumerable.Repeat(TechnologyLevelType.NONE, (int)infos().technologiesNum()).ToList();

            for (BuildingType eLoopBuilding = 0; eLoopBuilding < infos().buildingsNum(); eLoopBuilding++)
            {
                maaiBuildingResourceCost.Add(Enumerable.Repeat(0, (int)(infos().resourcesNum())).ToList());
            }

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                maabResourceReplaceValid.Add(Enumerable.Repeat(false, (int)(infos().resourcesNum())).ToList());
            }

            for (BuildingClassType eLoopBuildingClass = 0; eLoopBuildingClass < infos().buildingClassesNum(); eLoopBuildingClass++)
            {
                maabIgnoreInputIce.Add(Enumerable.Repeat(false, (int)(infos().icesNum())).ToList());
            }

            for (OrderType eLoopOrder = 0; eLoopOrder < infos().ordersNum(); eLoopOrder++)
            {
                maaOrderInfos.Add(new LinkedList<OrderInfo>());
            }

            initServerVariables();

            updateBuildingResourceCost();

            updateBaseSharePrice(true);

            if (gameServer().getLocation() == LocationType.EUROPA)
                AI_initTileArray();
            AI_initFundResourcesValueCache();

            makeAllDirty();

            //Debug.Log("Tachyon: " + Globals.AppInfo.GetTachyonID(getPlayer()));
            //Debug.Log("Tachyon: " + Globals.AppInfo.GetLocalTachyonID());
        }

        protected virtual void initServerVariables()
        {
            for (OrderType eLoopOrder = 0; eLoopOrder < infos().ordersNum(); eLoopOrder++)
            {
                maiRealBuildingOrderCount.Add(0);
            }

            for (OrderType eLoopOrder = 0; eLoopOrder < infos().ordersNum(); eLoopOrder++)
            {
                maiOrderInfosIndex.Add(0);
            }

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                maiAIResourceRateAverage.Add(0);
                maiAIResourceLifeSupport.Add(0);
                maiAIResourceConstructionCount.Add(0);
                maiAIResourceProductionCount.Add(0);
            }

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                mabCanProduceResource.Add(false);
            }

            for (OrderType eLoopOrder = 0; eLoopOrder < infos().ordersNum(); eLoopOrder++)
            {
                mabAIForceOrder.Add(false);
            }
        }

        public virtual void initPost()
        {
            using (new UnityProfileScope("PlayerServer.initPost"))
            {
                if (gameServer().isCampaign())
                {
                    EventLevelType eEventLevel = Globals.Campaign.getLevelEvent(gameServer().getLevel());

                    if (eEventLevel != EventLevelType.NONE)
                    {
                        PerkType ePerk = infos().eventLevel(eEventLevel).mePerk;

                        if (ePerk != PerkType.NONE)
                        {
                            incrementPerkCount(ePerk);
                        }
                    }

                    for (PerkType eLoopPerk = 0; eLoopPerk < infos().perksNum(); eLoopPerk++)
                    {
                        int iCount = Globals.Campaign.getCorporation(getCorporation()).getPerkCount(eLoopPerk);

                        for (int i = 0; i < iCount; i++)
                        {
                            incrementPerkCount(eLoopPerk);
                        }
                    }

                    if (gameServer().isSevenSols())
                    {
                        changeColonySharesOwned(1);
                        gameServer().changeSharesBought(1);

                        if (gameServer().isSevenSols())
                        {
                            changeColonySharesOwned(infos().handicap(getHandicap()).miShares);
                        }

                        if (gameServer().isCampaign())
                        {
                            if (gameServer().isSevenSols())
                            {
                                for (ColonyType eLoopColony = 0; eLoopColony < Globals.Infos.coloniesNum(); eLoopColony++)
                                {
                                    ColonyBonusLevelType eColonyBonusLevel = corporation().maeColonyBonusLevel[(int)eLoopColony];

                                    if (eColonyBonusLevel != ColonyBonusLevelType.NONE)
                                    {
                                        changeColonySharesOwned(infos().colonyBonus(infos().colony(eLoopColony).maeColonyBonus[(int)eColonyBonusLevel]).miStartingShares);
                                    }
                                }
                            }
                        }
                    }
                }

                if (canEverScan() && !(gameServer().isTurnBasedScanning()))
                {
                    changeScans(infos().Globals.INITIAL_SCANS);
                }

                updateValues();
            }
        }

        protected virtual void Serialize(object stream, int compatibilityNumber)
        {
            SimplifyIO.Data(stream, ref miNextMessageID, "NextMessageID");
            SimplifyIO.Data(stream, ref miRealOrderBuildingCount, "RealOrderBuildingCount");
            SimplifyIO.Data(stream, ref miAIForceConstruct, "AIForceConstruct");
            SimplifyIO.Data(stream, ref miAILastQuip, "AILastQuip");

            SimplifyIO.Data(stream, ref mbAIUpdated, "AIUpdated");

            SimplifyIO.Data(stream, ref maiRealBuildingOrderCount, "RealBuildingOrderCount");
            SimplifyIO.Data(stream, ref maiOrderInfosIndex, "OrderInfosIndex");
            SimplifyIO.Data(stream, ref maiAIResourceRateAverage, "AIResourceRateAverage");
            SimplifyIO.Data(stream, ref maiAIResourceLifeSupport, "AIResourceLifeSupport");
            SimplifyIO.Data(stream, ref maiAIResourceConstructionCount, "AIResourceConstructionCount");
            SimplifyIO.Data(stream, ref maiAIResourceProductionCount, "AIResourceProductionCount");

            SimplifyIO.Data(stream, ref mabCanProduceResource, "CanProduceResource");
            SimplifyIO.Data(stream, ref mabAIForceOrder, "AIForceOrder");

            SimplifyIO.Data(stream, ref maiNearbyTiles, "AINearbyTiles");
            SimplifyIO.Data(stream, ref maiNearbyIceTiles, "AINearbyIceTiles");
        }

        public virtual void writeValues(BinaryWriter stream, bool bAll, int compatibilityNumber)
        {
            writeClientValues(stream, bAll, compatibilityNumber);
            Serialize(stream, compatibilityNumber);
        }

        public virtual void readValues(BinaryReader stream, bool bAll, int compatibilityNumber)
        {
            readClientValues(stream, bAll, compatibilityNumber);
            Serialize(stream, compatibilityNumber);
        }

        public virtual void rebuildFromClient(PlayerClient pPlayerClient)
        {
            initServerVariables();

            foreach (TileServer pLoopTile in gameServer().tileServerAll())
            {
                if (pLoopTile.getRealOwner() == getPlayer())
                {
                    BuildingType eBuilding = BuildingType.NONE;

                    if (eBuilding == BuildingType.NONE)
                    {
                        ConstructionServer pConstruction = pLoopTile.constructionServer();
                        if (pConstruction != null)
                        {
                            eBuilding = pConstruction.getType();
                        }
                    }

                    if (eBuilding == BuildingType.NONE)
                    {
                        BuildingServer pBuilding = pLoopTile.buildingServer();
                        if (pBuilding != null)
                        {
                            eBuilding = pBuilding.getType();
                        }
                    }

                    if (eBuilding != BuildingType.NONE)
                    {
                        if (isOrderBuilding(eBuilding))
                        {
                            miRealOrderBuildingCount++;

                            {
                                OrderType eOrder = infos().buildingClass(infos().building(eBuilding).meClass).meOrderType;
                                if (eOrder != OrderType.NONE)
                                {
                                    maiRealBuildingOrderCount[(int)eOrder]++;
                                }
                            }
                        }
                    }
                }
            }

            for (OrderType eLoopOrder = 0; eLoopOrder < infos().ordersNum(); eLoopOrder++)
            {
                foreach (OrderInfo pLoopOrder in getOrderInfos(eLoopOrder))
                {
                    maiOrderInfosIndex[(int)eLoopOrder] = Math.Max(maiOrderInfosIndex[(int)eLoopOrder], (pLoopOrder.miIndex + 1));
                }
            }

            updateCanProduceResource();
        }

        public virtual void saveEndingMoneyDebt()
        {
            gameServer().statsServer().changeStat(StatsType.MISCELLANEOUS, (int)MiscellaneousStatType.ENDING_MONEY, getPlayer(), 0, (int)getMoney());
            gameServer().statsServer().changeStat(StatsType.MISCELLANEOUS, (int)MiscellaneousStatType.ENDING_DEBT, getPlayer(), 0, getDebt());
            gameServer().statsServer().changeStat(StatsType.MISCELLANEOUS, (int)MiscellaneousStatType.ENDING_RESOURCES, getPlayer(), 0, (int)(getCashResourceValue() - getMoney()));
        }

        protected virtual void addPerk(PerkType ePerk)
        {
            changeInterestModifier(infos().perk(ePerk).miInterestModifier);

            if (infos().perk(ePerk).mbUnlockHQAll)
            {
                for (HQType eLoopHQ = 0; eLoopHQ < infos().HQsNum(); eLoopHQ++)
                {
                    changeHQUnlockCount(eLoopHQ, 1);
                }
            }

            if (infos().perk(ePerk).meHQUnlock != HQType.NONE)
            {
                changeHQUnlockCount(infos().perk(ePerk).meHQUnlock, 1);
            }

            {
                BuildingType eBuildingImmune = infos().perk(ePerk).meBuildingImmune;

                if (eBuildingImmune != BuildingType.NONE)
                {
                    makeBuildingImmune(eBuildingImmune);
                }
            }

            {
                BuildingClassType eBuildingClass = infos().perk(ePerk).meBuildingClassLevel;

                if (eBuildingClass != BuildingClassType.NONE)
                {
                    changeBuildingClassLevel(eBuildingClass, 1);
                }
            }

            {
                PatentType ePatent = infos().perk(ePerk).mePatent;

                if (ePatent != PatentType.NONE)
                {
                    if (gameServer().canEverPatent(ePatent, false))
                    {
                        makePatentAcquiredPerk(infos().perk(ePerk).mePatent);
                    }
                }
            }

            {
                ResourceType eAlternatePowerResource = infos().perk(ePerk).meAlternatePowerResource;

                if (eAlternatePowerResource != ResourceType.NONE)
                {
                    makeAlternatePowerResourcePerk(eAlternatePowerResource);
                }
            }

            for (BuildingClassType eLoopBuildingClass = 0; eLoopBuildingClass < infos().buildingClassesNum(); eLoopBuildingClass++)
            {
                changeBuildingClassInputModifier(eLoopBuildingClass, infos().perk(ePerk).maiBuildingClassInputModifier[(int)eLoopBuildingClass]);
            }

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                changeResourceProductionModifier(eLoopResource, infos().perk(ePerk).maiResourceProductionModifier[(int)eLoopResource]);
            }

            for (OrderType eLoopOrder = 0; eLoopOrder < infos().ordersNum(); eLoopOrder++)
            {
                changeOrderCostModifier(eLoopOrder, infos().perk(ePerk).maiOrderCostModifier[(int)eLoopOrder]);
            }

            for (OrderType eLoopOrder = 0; eLoopOrder < infos().ordersNum(); eLoopOrder++)
            {
                changeOrderRateModifier(eLoopOrder, infos().perk(ePerk).maiOrderRateModifier[(int)eLoopOrder]);
            }

            for (BuildingClassType eLoopBuildingClass = 0; eLoopBuildingClass < infos().buildingClassesNum(); eLoopBuildingClass++)
            {
                ResourceLevelType eMinimumMining = infos().perk(ePerk).maeMinimumMining[(int)eLoopBuildingClass];

                if (eMinimumMining > ResourceLevelType.NONE)
                {
                    makeMinimumMining(eLoopBuildingClass, eMinimumMining);
                }
            }
        }

        public virtual void doPerk(PerkType ePerk, TileServer pTile, HashSet<int> siHQTiles = null)
        {
            changeClaims(infos().perk(ePerk).miClaims);

            {
                int iCount = infos().perk(ePerk).miSabotages;

                for (int i = 0; i < iCount; i++)
                {
                    SabotageType eBestSabotage = SabotageType.NONE;
                    int iBestValue = 0;

                    foreach (InfoSabotage pLoopSabotage in infos().sabotages())
                    {
                        if (GameServer.isValidSabotage(pLoopSabotage.meType, gameServer().getGameOptions(), infos()))
                        {
                            int iValue = gameServer().random().Next(1000) + 1;
                            if (iValue > iBestValue)
                            {
                                eBestSabotage = pLoopSabotage.meType;
                                iBestValue = iValue;
                            }
                        }
                    }

                    if (eBestSabotage != SabotageType.NONE)
                    {
                        changeSabotageCount(eBestSabotage, infos().sabotage(eBestSabotage).miCount);
                    }
                }
            }

            for (SabotageType eLoopSabotage = 0; eLoopSabotage < infos().sabotagesNum(); eLoopSabotage++)
            {
                changeSabotageCount(eLoopSabotage, infos().perk(ePerk).maiSabotageCount[(int)eLoopSabotage]);
            }

            foreach (InfoResource pLoopResource in infos().resources())
            {
                if (gameServer().isResourceValid(pLoopResource.meType))
                {
                    int iStockpile = infos().perk(ePerk).maiResourceStockpile[pLoopResource.miType];
                    if (iStockpile > 0)
                    {
                        changeWholeResourceStockpile(pLoopResource.meType, iStockpile, false);
                        gameServer().gameEventsServer().AddHQUpgradeBonus(getPlayer(), pLoopResource.meType, iStockpile, pTile.getID());
                    }
                }
            }

            {
                BuildingType eFreeBuilding = infos().perk(ePerk).meFreeBuilding;
                placeFreeBuilding(eFreeBuilding, pTile, siHQTiles);                
            }
        }

        public virtual void placeFreeBuilding(BuildingType eFreeBuilding, TileClient pTile, HashSet<int> siHQTiles)
        {
            if (eFreeBuilding != BuildingType.NONE)
            {
                TileServer pBestTile = (TileServer)(getFreeBuildingTile(eFreeBuilding, pTile, getHQ(), true, siHQTiles));

                if (pBestTile == null)
                {
                    pBestTile = (TileServer)(getFreeBuildingTile(eFreeBuilding, pTile, getHQ(), false, siHQTiles));
                }

                if (pBestTile != null)
                {
                    pBestTile.setOwner(getPlayer(), true);

                    BuildingServer pBuilding = createBuilding(eFreeBuilding, pBestTile, true);

                    if (pBuilding != null)
                    {
                        gameServer().gameEventsServer().AddBuildingFree(pBuilding.getID());
                    }
                }
                else
                {
                    changeClaims(1);
                }
            }
        }

        public virtual void updateConnectedBuildings()
        {
            for (int i = 0; i < getNumBuildings(); i++)
            {
                gameServer().buildingServer(getBuildingList()[i]).updateConnectedBuildings();
            }
        }

        public virtual void doScan()
        {
            if (gameServer().isGameOption(GameOptionType.REVEAL_MAP))
            {
                return;
            }

            if (gameServer().isTurnBasedScanning() && wasEverHuman())
            {
                return;
            }

            using (new UnityProfileScope("Player::doScan"))
            {
                if (!isHQFounded())
                {
                    if (getScanTime() > 0)
                    {
                        changeScanTime(-1);
                    }

                    if (getScanTime() == 0)
                    {
                        setScanTime(gameServer().getScanDelay());

                        changeScans(1);
                    }
                }
            }
        }

        public virtual void doTurn()
        {
            using (new UnityProfileScope("Player::doTurn"))
            {
                using (new UnityProfileScope("Player::clearStuff"))
                {
                    clearEntertainment();
                    clearResourceRate();
                    clearResourceShortfall();
                    clearResourceAutoPurchased();
                    clearResourceAutoSold();
                    clearOrderCapacity();

                    clearResourceInput();
                    clearResourceOutput();

                    clearOrderBuildings();
                }

                updateValues();

                if (getManualSaleTime() > 0)
                {
                    changeManualSaleTime(-1);
                }

                if (getBlackMarketTime() > 0)
                {
                    changeBlackMarketTime(-1);
                }

                if (!(infos().rulesSet(gameServer().getRulesSet()).mbNoAutoReveal))
                {
                    doTurnReveal();
                }

                doDividends();

                for (int i = 0; i < getConstructionList().Count; i++)
                {
                    int iConstructionID = getConstructionList()[i];
                    gameServer().constructionServer(iConstructionID).incrementConstruction();
                }

                using (new UnityProfileScope("Player::doUnit"))
                {
                    for (int i = 0; i < getUnitList().Count; i++)
                    {
                        UnitServer loopUnit = gameServer().unitServer(getUnitList()[i]);

                        if (loopUnit.doTurn())
                        {
                            killUnit(loopUnit, false, true);
                        }
                    }
                }

                using (new UnityProfileScope("Player::doBuildings"))
                {
                    foreach (int iBuildingID in getBuildingList())
                    {
                        gameServer().buildingServer(iBuildingID).doTurn();
                    }

                    for (OrderType eLoopOrder = 0; eLoopOrder < infos().ordersNum(); eLoopOrder++)
                    {
                        doTurnOrders(eLoopOrder);
                    }
                }

                doLifeSupport();

                changeMoney(getEntertainment());

                setShouldCalculateCashResources(true);
                updateHighestBuyout();
            }
        }

        protected virtual void updateValues()
        {
            if (!isSubsidiary())
            {
                setFinalMarketCap((getSharePrice() * infos().Globals.SHARE_PURCHASE_SIZE * gameServer().getInitialShares()) / Constants.STOCK_MULTIPLIER);
            }

            doBondRating();
            doEnergyResource();
            doGasResource();
            doLaunchResource();
            doResourceReplace();
        }

        protected virtual void updateHighestBuyout()
        {
            if (infos().rulesSet(gameClient().getRulesSet()).mbNoStockMarket)
            {
                return;
            }

            PlayerType eBestPlayer = PlayerType.NONE;
            int iBestValue = 0;

            foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAliveAll())
            {
                if (pLoopPlayer.getTeam() != getTeam())
                {
                    int iValue = (pLoopPlayer.getBuyoutPercent(getPlayer()) / 10);
                    if (iValue > iBestValue)
                    {
                        eBestPlayer = pLoopPlayer.getPlayer();
                        iBestValue = iValue;
                    }
                }
            }

            if ((eBestPlayer != PlayerType.NONE) && (iBestValue > getHighestBuyoutTenths()))
            {
                setHighestBuyoutTenths(iBestValue);

                if (iBestValue >= 5)
                {
                    gameServer().gameEventsServer().AddBuyoutHighestTenth(getPlayer(), eBestPlayer, iBestValue);
                }
            }
        }

        protected virtual void doBondRating()
        {
            int iBetterThreshold = 0;
            int iWorseThreshold = 0;
            int iExcessBond = 0;
            setBondRating(calculateBondRating(getDebt(), 0, ref iBetterThreshold, ref iWorseThreshold, ref iExcessBond));
            setExcessBond(iExcessBond);
        }

        protected virtual void doEnergyResource()
        {
            ResourceType eBestResource = ResourceType.NONE;

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                if (gameServer().isResourceValid(eLoopResource))
                {
                    if ((eLoopResource == infos().Globals.ENERGY_RESOURCE) || isAlternatePowerResource(eLoopResource))
                    {
                        if ((eBestResource == ResourceType.NONE) || (gameServer().marketServer().getPrice(eLoopResource) < gameServer().marketServer().getPrice(eBestResource)))
                        {
                            eBestResource = eLoopResource;
                        }
                    }
                }
            }

            setEnergyResource(eBestResource);
        }

        protected virtual void doGasResource()
        {
            ResourceType eBestResource = ResourceType.NONE;

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                if (gameServer().isResourceValid(eLoopResource))
                {
                    if (((getHQ() != HQType.NONE) && (eLoopResource == infos().HQ(getHQ()).meGasResource)) || isAlternateGasResource(eLoopResource))
                    {
                        if ((eBestResource == ResourceType.NONE) || (gameServer().marketServer().getPrice(eLoopResource) < gameServer().marketServer().getPrice(eBestResource)))
                        {
                            eBestResource = eLoopResource;
                        }
                    }
                }
            }

            if (eBestResource == ResourceType.NONE)
            {
                eBestResource = infos().Globals.GAS_RESOURCE;
            }

            setGasResource(eBestResource);
        }

        protected virtual void doLaunchResource()
        {
            ResourceType eBestResource = ResourceType.NONE;

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                if (gameServer().isResourceValid(eLoopResource))
                {
                    if ((eLoopResource == infos().location(gameClient().getLocation()).meLaunchResource) || isAlternateGasResource(eLoopResource))
                    {
                        if ((eBestResource == ResourceType.NONE) || (gameServer().marketServer().getPrice(eLoopResource) < gameServer().marketServer().getPrice(eBestResource)))
                        {
                            eBestResource = eLoopResource;
                        }
                    }
                }
            }

            setLaunchResource(eBestResource);
        }

        protected virtual void doTurnReveal()
        {
            if (!isMapRevealed() && ((gameServer().getTurnCount() % ((gameServer().getHQLevels() > 0) ? 2 : 8)) == 0))
            {
                using (new UnityProfileScope("Player::doTurnReveal"))
                {
                    PlayerType ePlayer = getPlayer();
                    MapServer mapServer = gameServer().mapServer();

                    //reset cached variables
                    ArrayUtilities.SetSize(ref doTurnRevealDistances, mapServer.numTiles());
                    ArrayUtilities.Fill(doTurnRevealDistances, -1);
                    doTurnRevealQueue.Clear();

                    //initialize queue with all visible tiles at distance 0
                    foreach (TileClient pLoopTile in mapServer.tileClientAll())
                    {
                        if (pLoopTile.getVisibility(ePlayer) == VisibilityType.VISIBLE)
                        {
                            doTurnRevealDistances[pLoopTile.getID()] = 0;
                            doTurnRevealQueue.AddBack(pLoopTile);
                        }
                    }

                    //flood fill to adjacent tiles
                    int maxRevealDistance = innerScanRange() + 1;
                    while(doTurnRevealQueue.Count > 0)
                    {
                        TileClient pLoopTile = doTurnRevealQueue.RemoveFront();
                        int currentDistance = doTurnRevealDistances[pLoopTile.getID()];
                        if (currentDistance >= maxRevealDistance) //nothing interesting beyond maxRevealDistance, so save some processing
                        {
                            break;
                        }

                        for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                        {
                            TileClient pAdjacentTile = mapServer.tileClientAdjacent(pLoopTile, eLoopDirection);
                            if ((pAdjacentTile != null) && (doTurnRevealDistances[pAdjacentTile.getID()] == -1))
                            {
                                doTurnRevealQueue.AddBack(pAdjacentTile);
                                doTurnRevealDistances[pAdjacentTile.getID()] = currentDistance + 1;
                            }
                        }
                    }

                    //update visibility
                    for (int i=0; i<doTurnRevealDistances.Length; i++)
                    {
                        if (doTurnRevealDistances[i] == 1) //set new visible tiles
                        {
                            mapServer.tileServer(i).increaseVisibility(ePlayer, VisibilityType.VISIBLE, true);
                        }
                        else if (doTurnRevealDistances[i] > 1) //all tiles further than revealed still have distances[i] == -1
                        {
                            mapServer.tileServer(i).increaseVisibility(ePlayer, VisibilityType.REVEALED, true);
                        }
                    }
                }
            }
        }

        protected virtual void doDividends()
        {
            if (!isSubsidiary())
            {
                return;
            }

            long iOriginalMoney = getMoney();

            for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameServer().getNumPlayers(); eLoopPlayer++)
            {
                PlayerServer pLoopPlayer = gameServer().playerServer(eLoopPlayer);

                int iDividend = getDividend(iOriginalMoney, pLoopPlayer.getSharesOwned(getPlayer()));
                if (iDividend > 0)
                {
                    changeMoney(-(iDividend));
                    pLoopPlayer.changeMoney(iDividend);
                    pLoopPlayer.increaseDividendTotalBy(getPlayer(), iDividend);

                    gameServer().statsServer().changeStat(StatsType.MISCELLANEOUS, (int)MiscellaneousStatType.DIVIDEND, pLoopPlayer.getPlayer(), 0, iDividend);
                }
            }
        }

        protected virtual void doLifeSupport()
        {
            foreach (InfoResource pLoopResource in infos().resources())
            {
                ResourceType eLoopResource = pLoopResource.meType;

                int iLife = lifeSupport(eLoopResource);
                if (iLife < 0)
                {
                    changeResourceStockpile(eLoopResource, iLife, true);
                    changeResourceRate(eLoopResource, iLife);
                    changeResourceInput(eLoopResource, iLife);

                    gameServer().statsServer().changeStat(StatsType.RESOURCE, (int)ResourceStatType.LIFE_SUPPORT_CONSUMED, getPlayer(), (int)eLoopResource, -iLife);
                }
            }
        }

        public virtual void doTurnStockpileExtra()
        {
            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                if (gameServer().isResourceValid(eLoopResource))
                {
                    int iRate = getStockpileExtraRate(eLoopResource);
                    if (iRate != 0)
                    {
                        changeResourceRate(eLoopResource, iRate);
                        changeResourceExtraStockpile(eLoopResource, iRate);
                    }
                }
            }
        }

        public virtual void doTurnStockpileNegative()
        {
            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                int iDiff = getResourceStockpile(eLoopResource, false);
                if (iDiff < 0)
                {
                    setResourceStockpile(eLoopResource, 0);

                    if (iDiff < 0)
                    {
                        int iAuto = getResourceAutoTrade(eLoopResource);
                        if (iAuto > 0)
                        {
                            if (iAuto >= -(iDiff))
                            {
                                changeResourceAutoTrade(eLoopResource, iDiff);
                                iDiff = 0;
                            }
                            else
                            {
                                setResourceAutoTrade(eLoopResource, 0);
                                iDiff += iAuto;
                            }
                        }
                    }

                    if (iDiff < 0)
                    {
                        int iExtra = getResourceExtraStockpile(eLoopResource);
                        if (iExtra > 0)
                        {
                            if (iExtra >= -(iDiff))
                            {
                                changeResourceExtraStockpile(eLoopResource, iDiff);
                                iDiff = 0;
                            }
                            else
                            {
                                setResourceExtraStockpile(eLoopResource, 0);
                                iDiff += iExtra;
                            }
                        }
                    }

                    if (iDiff < 0)
                    {
                        if (gameServer().isTeamGame() && !isSubsidiary())
                        {
                            for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameServer().getNumPlayers(); eLoopPlayer++)
                            {
                                PlayerServer pLoopPlayer = gameServer().playerServer(eLoopPlayer);

                                if (!(pLoopPlayer.isSubsidiary()))
                                {
                                    if (pLoopPlayer.getTeam() == getTeam())
                                    {
                                        if ((pLoopPlayer.getPlayer() != getPlayer()) && pLoopPlayer.isTeamShareResource(eLoopResource))
                                        {
                                            int iStockpile = pLoopPlayer.getResourceStockpile(eLoopResource, false);
                                            if (iStockpile > 0)
                                            {
                                                if (iStockpile >= -(iDiff))
                                                {
                                                    pLoopPlayer.changeResourceStockpile(eLoopResource, iDiff, false);
                                                    iDiff = 0;
                                                }
                                                else
                                                {
                                                    pLoopPlayer.setResourceStockpile(eLoopResource, 0);
                                                    iDiff += iStockpile;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (iDiff < 0)
                    {
                        tradeAuto(eLoopResource, iDiff);
                    }
                }
            }
        }

        public virtual void doTurnStockpilePositive()
        {
            int iRevenue = 0;

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                if (!(infos().resource(eLoopResource).mbTrade))
                {
                    int iDiff = getResourceStockpile(eLoopResource, false);
                    if (iDiff > 0)
                    {
                        changeResourceStockpile(eLoopResource, -(iDiff), false);

                        if (isHoldResource(eLoopResource))
                        {
                            if (getResourceExtraCapacity(eLoopResource) > 0)
                            {
                                setResourceExtraStockpile(eLoopResource, Math.Min(getResourceExtraCapacity(eLoopResource), (getResourceExtraStockpile(eLoopResource) + iDiff)));
                            }
                        }
                        else
                        {
                            iRevenue += tradeAuto(eLoopResource, iDiff);
                        }
                    }
                }
            }

            if (iRevenue > 0)
            {
                int iDebtPayment = Math.Min(iRevenue, -(getDebt()));

                changeDebt(iDebtPayment);
                changeMoney(iRevenue - iDebtPayment);
            }
        }

        public virtual void doTurnAutoSell()
        {
            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                int iValue = getResourceAutoTrade(eLoopResource);
                if (iValue != 0)
                {
                    tradeAuto(eLoopResource, iValue);
                    setResourceAutoTrade(eLoopResource, 0);
                }
            }
        }

        public virtual void doTurnAutoSupply()
        {
            foreach (int iBuildingID in getBuildingList())
            {
                gameServer().buildingServer(iBuildingID).doAutoSupply();
            }
        }

        public virtual void doTurnAutoPayDebt()
        {
            if (isAutoPayDebt())
            {
                int iDebtToPay = getDebt() - getAutoPayDebtTarget();
                if (iDebtToPay < 0)
                {
                    int iPayment = (int)Math.Min(getMoney(), -(iDebtToPay));

                    changeDebt(iPayment);
                    changeMoney(-(iPayment));
                }
            }
        }

        public virtual void buyout(PlayerType ePlayer)
        {
            if (!canBuyout(ePlayer, true))
            {
                return;
            }

            if (isHuman())
            {
                if (gameServer().playerServer(ePlayer).getBuyoutPercent(getPlayer()) == 100)
                {
                    gameServer().gameEventsServer().AddAchievement(getPlayer(), infos().getType<AchievementType>("ACHIEVEMENT_BUYOUT_AT_100_PERCENT"));
                }
            }

            int iCost = -(getTotalBuyoutPrice(ePlayer));
            changeMoney(iCost, true);
            gameServer().statsServer().changeStat(StatsType.STOCK, (int)StockStatType.PURCHASED, getPlayer(), (int)ePlayer, -(iCost));

            PlayerServer pPlayer = gameServer().playerServer(ePlayer);

            if (pPlayer.getSharesAvailable() > 0)
            {
                changeSharesOwned(ePlayer, pPlayer.getSharesAvailable());
                pPlayer.setSharesAvailable(0);

                if (isHuman())
                {
                    gameServer().gameEventsServer().AddAchievement(getPlayer(), infos().getType<AchievementType>("ACHIEVEMENT_INSTANT_BUYOUT"));
                }
            }

            for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameServer().getNumPlayers(); eLoopPlayer++)
            {
                PlayerServer pLoopPlayer = gameServer().playerServer(eLoopPlayer);
                if ((pLoopPlayer.getTeam() != getTeam()) && (pLoopPlayer.getTeam() == pPlayer.getTeam()))
                {
                    int iQuantity = pLoopPlayer.getSharesOwned(ePlayer);
                    if (iQuantity > 0)
                    {
                        changeSharesOwned(ePlayer, iQuantity);
                        pLoopPlayer.changeSharesOwned(ePlayer, -(iQuantity));
                    }
                }
            }

            if (!isHumanTeam())
            {
                CharacterType eCharacter = infos().personality(getPersonality()).meCharacter;
                AI_quip(PlayerType.NONE, infos().character(eCharacter).meQuipBuyoutAI);
            }

            pPlayer.testSubsidiary(getPlayer(), getPlayer());

            if (gameClient().countTeamsWinEligible() > 1)
            {
                int iClaims = infos().Globals.BUYOUT_CLAIMS;
                changeClaims(iClaims);
                gameServer().gameEventsServer().AddBuyoutClaims(getPlayer(), ePlayer, iClaims);
            }
        }

        public virtual void buyShares(PlayerType ePlayer)
        {
            if (!canBuyShares(ePlayer, true))
            {
                return;
            }

            int iCost = -(getBuySharePrice(ePlayer));
            changeMoney(iCost);

            gameServer().statsServer().changeStat(StatsType.STOCK, (int)StockStatType.PURCHASED, getPlayer(), (int)ePlayer, -(iCost));
            gameServer().statsServer().addEvent(getPlayer(), StatEventType.BUY_STOCK, (int)ePlayer);

            gameServer().gameEventsServer().AddStockPurchase(getPlayer(), ePlayer, infos().Globals.SHARE_PURCHASE_SIZE);

            PlayerServer pPlayer = gameServer().playerServer(ePlayer);

            bool bBoughtOut = false;

            if (pPlayer.getSharesAvailable() > 0)
            {
                pPlayer.changeSharesAvailable(-1);
            }
            else
            {
                PlayerServer pBoughtOutPlayer = null;

                if (pPlayer.getSharesOwned(ePlayer) > 0)
                {
                    pBoughtOutPlayer = pPlayer;
                }
                else
                {
                    foreach (PlayerServer pLoopPlayer in pPlayer.getTeammatesAll())
                    {
                        if (pLoopPlayer.getSharesOwned(ePlayer) > 0)
                        {
                            pBoughtOutPlayer = pLoopPlayer;
                            break;
                        }
                    }
                }

                if (pBoughtOutPlayer != null)
                {
                    pBoughtOutPlayer.changeSharesOwned(ePlayer, -1);

                    bBoughtOut = true;
                }
            }
            if (!(pPlayer.isSubsidiary()) && (getTeam() == pPlayer.getTeam()))
            {
                pPlayer.changeSharesOwned(ePlayer, 1);
            }
            else
            {
                changeSharesOwned(ePlayer, 1);
            }

            {
                int iDelay = (infos().Globals.STOCK_DELAY / ((getTeam() == pPlayer.getTeam()) ? 2 : 1));

                if ((gameServer().countPlayersAlive() > 2) && (getTeam() != pPlayer.getTeam()) && bBoughtOut)
                {
                    increaseSellDelayBy(ePlayer, (iDelay * 5));
                }

                if (pPlayer != this)
                {
                    if (pPlayer.getSharesAvailable() == 0)
                    {
                        pPlayer.increaseStockDelayBy(ePlayer, iDelay);
                    }
                }

                foreach (PlayerServer pLoopPlayer in getAliveTeammatesAll())
                {
                    pLoopPlayer.increaseStockDelayBy(ePlayer, iDelay);
                }
            }

            if (isHuman() && !(pPlayer.isHumanTeam()))
            {
                if (gameServer().randomMisc().Next(10) == 0)
                {
                    pPlayer.AI_quip(getPlayer(), infos().character(pPlayer.getCharacter()).meQuipPlayerBuy);
                }
            }

            if (pPlayer.testSubsidiary(getPlayer(), PlayerType.NONE))
            {
                if (isHuman())
                {
                    if (getSharesOwned(ePlayer) == pPlayer.getSharesBought())
                    {
                        gameServer().gameEventsServer().AddAchievement(getPlayer(), infos().getType<AchievementType>("ACHIEVEMENT_MAJORITY_BUYOUT_ALL_SHARES"));
                    }

                    if (getSharesOwned(ePlayer) == 1)
                    {
                        gameServer().gameEventsServer().AddAchievement(getPlayer(), infos().getType<AchievementType>("ACHIEVEMENT_MAJORITY_BUYOUT_ONE_SHARE"));
                    }
                }
            }
            else if ((pPlayer.getSharesAvailable() > 0) &&
                     ((pPlayer.countOwnSharesOwned() + pPlayer.getSharesAvailable()) == gameServer().getMajorityShares()) &&
                     (getTeam() != pPlayer.getTeam()))
            {
                foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAliveAll())
                {
                    if (pLoopPlayer.getTeam() != pPlayer.getTeam())
                    {
                        pLoopPlayer.increaseStockDelayBy(ePlayer, (infos().Globals.STOCK_DELAY * 2));
                    }
                }

                gameServer().gameEventsServer().AddMajorityBuyoutVulnerable(ePlayer);
            }
        }

        public virtual void sellShares(PlayerType ePlayer)
        {
            if (!canSellShares(ePlayer))
            {
                return;
            }

            int iPrice = getSellSharePrice(ePlayer);
            changeMoney(iPrice);

            gameServer().statsServer().changeStat(StatsType.STOCK, (int)StockStatType.SOLD, getPlayer(), (int)ePlayer, iPrice);
            gameServer().statsServer().addEvent(getPlayer(), StatEventType.SELL_STOCK, (int)ePlayer);

            PlayerServer pPlayer = gameServer().playerServer(ePlayer);

            {
                int iDelay = infos().Globals.STOCK_DELAY;

                if ((gameServer().countPlayersAlive() > 2) && (getTeam() != pPlayer.getTeam()) && (pPlayer.countOwnSharesOwned() >= gameClient().getMajorityShares()))
                {
                    increaseBuyDelayBy(ePlayer, (iDelay * 2));
                }

                foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAliveAll())
                {
                    increaseStockDelayBy(pLoopPlayer.getPlayer(), iDelay);
                }

                if (getTeam() != pPlayer.getTeam())
                {
                    foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAliveAll())
                    {
                        if (pLoopPlayer.getTeam() != pPlayer.getTeam())
                        {
                            pLoopPlayer.increaseStockDelayBy(ePlayer, iDelay);
                        }
                    }
                }
            }

            changeSharesOwned(ePlayer, -1);
            pPlayer.changeSharesAvailable(1);
            gameServer().gameEventsServer().AddStockSold(getPlayer(), ePlayer, infos().Globals.SHARE_PURCHASE_SIZE);
        }

        public virtual void buyColonyShares()
        {
            if (!canBuyColonyShares(true))
            {
                return;
            }

            int iCost = gameServer().getSharePrice();
            changeMoney(-(iCost));
            gameServer().statsServer().changeStat(StatsType.MISCELLANEOUS, (int)MiscellaneousStatType.COLONY, getPlayer(), 0, iCost);

            incrementColonyShares();
            gameServer().changeSharesBought(1);

            gameServer().gameEventsServer().AddColonyStock(getPlayer());
        }

        public virtual void buyColonyModule(ModuleType eModule)
        {
            if (!canBuyColonyModule(eModule, true))
            {
                return;
            }

            int iCost = gameServer().getSharePrice();
            changeMoney(-(iCost));
            gameServer().statsServer().changeStat(StatsType.MISCELLANEOUS, (int)MiscellaneousStatType.COLONY, getPlayer(), 0, iCost);

            foreach (InfoResource pLoopResource in infos().resources())
            {
                spend(pLoopResource.meType, infos().module(eModule).maiResourceCost[pLoopResource.miType]);
            }

            TileServer pTile = gameServer().addModule(eModule, false, false);

            if (infos().module(eModule).mbPopulation)
            {
                gameServer().updateNextPopulationModule();
            }
            if (infos().module(eModule).mbLabor)
            {
                gameServer().setNextLaborModule(eModule, false);
                gameServer().updateNextLaborModule();
            }

            incrementColonyShares();
            gameServer().changeSharesBought(1);

            gameServer().gameEventsServer().AddColonyModule(getPlayer(), eModule, ((pTile != null) ? pTile.getID() : -1));
        }

        public virtual void supplyWholesale(int iSlot)
        {
            if (!canSupplyWholesale(iSlot, true))
            {
                return;
            }

            int iCost = gameServer().getSharePrice();
            changeMoney(-(iCost));
            gameServer().statsServer().changeStat(StatsType.MISCELLANEOUS, (int)MiscellaneousStatType.COLONY, getPlayer(), 0, iCost);

            ResourceType eResource = gameServer().getWholesaleSlotResource(iSlot);
            int iShipment = gameServer().getWholesaleSlotShipment(iSlot);

            spend(eResource, iShipment);

            gameServer().incrementWholesaleSlotCount(iSlot);
            gameServer().changeWholesaleSlotDelay(iSlot, 4);

            if ((gameServer().getLabor() < gameServer().getColonyCap()) && (gameServer().getLabor() <= gameServer().getMaxPopulation()))
            {
                gameServer().addModule(ModuleType.NONE, false, true);
            }
            else
            {
                gameServer().addModule(ModuleType.NONE, true, false);
            }

            incrementColonyShares();
            gameServer().changeSharesBought(1);

            gameServer().gameEventsServer().AddSupplyWholesale(getPlayer(), eResource, iShipment);
        }

        public void setImportLimit(int iLimit)
        {
            if (miImportLimit != iLimit)
            {
                miImportLimit = iLimit;
                makeDirty(PlayerDirtyType.miImportLimit);
            }
        }

        public void import(int iSlot)
        {
            if (!canImport(iSlot, true))
                return;

            int iResourceCost = mGame.getImportSlotCost(iSlot);
            int iMoneyCost = getImportMoneyCost(iSlot);
            ResourceType eResourceCost = gameServer().getImportResourcePayment(iSlot);
            spend(eResourceCost, iResourceCost);
            changeMoney(-iMoneyCost);

            changeWholeResourceStockpile(mGame.getImportSlotResource(iSlot), infos().Globals.IMPORT_AMOUNT, true);

            gameServer().processImport(iSlot);
            gameServer().statsServer().changeStat(StatsType.RESOURCE, (int)ResourceStatType.IMPORTS_CONSUMED, getPlayer(), (int)eResourceCost, iResourceCost * Constants.RESOURCE_MULTIPLIER);
            gameServer().statsServer().changeStat(StatsType.RESOURCE, (int)ResourceStatType.IMPORTS_RECEIVED, getPlayer(), (int)mGame.getImportSlotResource(iSlot), infos().Globals.IMPORT_AMOUNT * Constants.RESOURCE_MULTIPLIER);
            gameServer().statsServer().changeStat(StatsType.MISCELLANEOUS, (int)MiscellaneousStatType.IMPORT_SPENT, getPlayer(), 0, -iMoneyCost);

            miNextImportTurn = mGame.getTurnCount() + infos().resource(mGame.getImportSlotResource(iSlot)).miImportCooldown;
            miImportsDone++;
           
            makeDirty(PlayerDirtyType.miImportsDone);
            makeDirty(PlayerDirtyType.miNextImportTurn);
        }

        public virtual void payDebt()
        {
            if (!canPayDebt())
            {
                return;
            }

            int iPayment = getDebtPayment();

            changeMoney(-(iPayment));
            changeDebt(iPayment);

            setAutoPayDebtTarget(getDebt());

            doBondRating();
        }

        public virtual void payDebtAll()
        {
            int iPayment = (int)Math.Min(getMoney(), -(getDebt()));

            changeMoney(-(iPayment));
            changeDebt(iPayment);

            setAutoPayDebtTarget(getDebt());

            doBondRating();
        }

        public virtual bool canTrade(ResourceType eResource, bool bBuy)
        {
            if (infos().rulesSet(gameServer().getRulesSet()).mbNoBuySell)
            {
                return false;
            }

            if (bBuy)
            {
                if (isResourceNoBuy(eResource))
                {
                    return false;
                }
            }
            else
            {
                if (isResourceNoSell(eResource))
                {
                    return false;
                }
            }

            return true;
        }

        public virtual void setShouldCalculateCashResources(bool bNewValue)
        {
            mbShouldCalculateCashResources = bNewValue; //need to always send this value because value is only changed on client send and not serialized from there, should be better than serializing every tick
            makeDirty(PlayerDirtyType.mbShouldCalculateCashResources);
        }

        public virtual int trade(ResourceType eResource, int iQuantity, bool bManual)
        {
            if (!(infos().resource(eResource).mbTrade))
            {
                return 0;
            }

            if (iQuantity == 0)
            {
                return 0;
            }

            if (bManual && !canTrade(eResource, (iQuantity > 0)))
            {
                return 0;
            }

            if (!(gameServer().isResourceValid(eResource)))
            {
                Debug.LogError("Trying to trade " + infos().resource(eResource).meName);

                return 0;
            }

            int iResourceTraded = 0;

            int iMoneyValue = 0;

            if (iQuantity < 0)
            {
                int iQuantityLeft = -(iQuantity);

                while (iQuantityLeft > 0)
                {
                    int iCurrentTrade = -(Utils.GetAmountCanBuySell(this, gameServer().marketServer(), eResource, -(Math.Min(Constants.TRADE_QUANTITY, iQuantityLeft))));

                    if (iCurrentTrade > 0)
                    {
                        int iMoneyChange = gameServer().marketServer().calculateSellRevenue(eResource, iCurrentTrade, Constants.PRICE_MIN / Constants.PRICE_MULTIPLIER);
                        gameServer().marketServer().changeSupply(eResource, (iCurrentTrade * Constants.RESOURCE_MULTIPLIER));
                        iMoneyValue += iMoneyChange;
                        changeMoney(iMoneyChange);
                        changeWholeResourceStockpile(eResource, -(iCurrentTrade), true);

                        iQuantityLeft -= iCurrentTrade;
                        iResourceTraded -= iCurrentTrade;
                    }
                    else
                    {
                        break;
                    }
                }

                if (iMoneyValue != 0)
                {
                    gameServer().statsServer().changeStat(StatsType.RESOURCE, (int)ResourceStatType.TRADE_SOLD, getPlayer(), (int)eResource, iMoneyValue);

                    if (bManual)
                    {
                        setManualSaleTime(infos().Globals.MANUAL_SALE_TIME);
                    }
                }
            }
            else
            {
                int iQuantityLeft = iQuantity;

                while (iQuantityLeft > 0)
                {
                    int iCurrentTrade = Utils.GetAmountCanBuySell(this, gameServer().marketServer(), eResource, Math.Min(Constants.TRADE_QUANTITY, iQuantityLeft));

                    if (iCurrentTrade > 0)
                    {
                        int iMoneySpent = gameServer().marketServer().calculateBuyCost(eResource, iCurrentTrade);
                        gameServer().marketServer().changeSupply(eResource, -(iCurrentTrade * Constants.RESOURCE_MULTIPLIER));
                        iMoneyValue += iMoneySpent;
                        changeMoney(-iMoneySpent);
                        changeWholeResourceStockpile(eResource, iCurrentTrade, false);

                        iQuantityLeft -= iCurrentTrade;
                        iResourceTraded += iCurrentTrade;
                    }
                    else
                    {
                        break;
                    }
                }

                if (iMoneyValue != 0)
                {
                    gameServer().statsServer().changeStat(StatsType.RESOURCE, (int)ResourceStatType.TRADE_PURCHASE, getPlayer(), (int)eResource, iMoneyValue);
                }
            }

            gameServer().gameEventsServer().AddResourceTraded(getPlayer(), eResource, iResourceTraded, iMoneyValue, (iResourceTraded == 0), bManual);

            return iResourceTraded;
        }

        public virtual int tradeAuto(ResourceType eResource, int iDiff)
        {
            if (iDiff == 0)
            {
                return 0;
            }

            if (iDiff > 0)
            {
                int iTotalRevenue = 0;

                while (iDiff > 0)
                {
                    int iQuantity = Math.Min(iDiff, Constants.TRADE_QUANTITY * Constants.RESOURCE_MULTIPLIER);
                    iDiff -= iQuantity;

                    int iRevenue = ((iQuantity * gameServer().marketServer().getWholePrice(eResource)) / Constants.RESOURCE_MULTIPLIER);

                    if (iRevenue == 0)
                    {
                        iRevenue = 1;
                    }

                    changeResourceAutoSold(eResource, iRevenue);

                    gameServer().marketServer().changeSupply(eResource, iQuantity);

                    gameServer().statsServer().changeStat(StatsType.RESOURCE, (int)ResourceStatType.AUTO_TRADE_SOLD, getPlayer(), (int)eResource, iRevenue);

                    if (infos().resource(eResource).mbTrade)
                    {
                        changeMoney(iRevenue);
                    }
                    else
                    {
                        iTotalRevenue += iRevenue;
                    }
                }

                return iTotalRevenue;
            }
            else
            {
                while (iDiff < 0)
                {
                    int iQuantity = -(Math.Min(-(iDiff), Constants.TRADE_QUANTITY * Constants.RESOURCE_MULTIPLIER));
                    iDiff -= iQuantity;

                    int iCost = ((iQuantity * gameServer().marketServer().getWholePrice(eResource)) / Constants.RESOURCE_MULTIPLIER);

                    if (iCost == 0)
                    {
                        iCost = -1;
                    }

                    changeResourceAutoPurchased(eResource, -(iCost));

                    gameServer().marketServer().changeSupply(eResource, iQuantity);

                    gameServer().statsServer().changeStat(StatsType.RESOURCE, (int)ResourceStatType.AUTO_TRADE_PURCHASE, getPlayer(), (int)eResource, -iCost);

                    changeDebt(iCost);
                }

                return 0;
            }
        }

        public virtual void spend(ResourceType eResource, int iQuantity)
        {
            if (!gameClient().isResourceValid(eResource))
                return;

            int iDiff = (iQuantity - getWholeResourceStockpile(eResource, true));
            if (iDiff > 0)
            {
                trade(eResource, iDiff, false);
            }

            changeWholeResourceStockpile(eResource, -(iQuantity), true);
        }

        public virtual bool sellResourcesQuantity(int iQuantity)
        {
            bool bSuccess = false;

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                if (getWholeResourceStockpile(eLoopResource, false) > 0)
                {
                    if (trade(eLoopResource, -(iQuantity), false) != 0)
                    {
                        bSuccess = true;
                    }
                }
            }

            return bSuccess;
        }

        public virtual void sellAllResources(bool bManual)
        {
            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                int iWholeStockpile = getWholeResourceStockpile(eLoopResource, false);
                if (iWholeStockpile > 0)
                {
                    trade(eLoopResource, -(iWholeStockpile), bManual);
                }
            }
        }

        public virtual void processResource(ResourceType eResource, int iQuantity)
        {
            if (isHoldResource(eResource) || (iQuantity < 0))
            {
                changeResourceStockpile(eResource, iQuantity, false);
            }
            else
            {
                changeResourceAutoTrade(eResource, iQuantity);
            }
        }

        public virtual void increaseBid()
        {
            if (!canBid())
            {
                return;
            }

            setSkipAuction(false);

            gameServer().newBid(getPlayer(), gameServer().getNextAuctionBid());
        }

        public virtual void blackMarket(BlackMarketType eBlackMarket)
        {
            if (!canBlackMarket(eBlackMarket, true))
            {
                return;
            }

            int iCost = getBlackMarketCost(eBlackMarket);

            gameServer().statsServer().changeStat(StatsType.BLACK_MARKET, (int)BlackMarketStatType.SPENT, getPlayer(), (int)eBlackMarket, iCost);

            changeMoney(-iCost);

            {
                AchievementType eAchievement = infos().getType<AchievementType>("ACHIEVEMENT_GOON_SQUAD_100K");

                if (isHuman())
                {
                    if (iCost >= infos().achievement(eAchievement).miValue)
                    {
                        if (infos().blackMarket(eBlackMarket).meSabotage == infos().achievement(eAchievement).meSabotage)
                        {
                            gameServer().gameEventsServer().AddAchievement(getPlayer(), eAchievement);
                        }
                    }
                }
            }

            changeBlackMarketTime(calculateBlackMarketDelay(eBlackMarket));

            blackMarketReceive(eBlackMarket);
        }

        public virtual void blackMarketReceive(BlackMarketType eBlackMarket)
        {
            gameServer().statsServer().changeStat(StatsType.BLACK_MARKET, (int)BlackMarketStatType.PURCHASED, getPlayer(), (int)eBlackMarket, 1);

            incrementBlackMarketCount(eBlackMarket);
            gameServer().incrementBlackMarketCount(eBlackMarket);

            changeClaims(infos().blackMarket(eBlackMarket).miNewClaims);
            changeBondRatingChange(infos().blackMarket(eBlackMarket).miBondRatingChange);

            {
                SabotageType eSabotage = infos().blackMarket(eBlackMarket).meSabotage;

                if (eSabotage != SabotageType.NONE)
                {
                    changeSabotageCount(eSabotage, infos().blackMarket(eBlackMarket).miSabotageCount);
                }
            }

            gameServer().statsServer().addEvent(getPlayer(), StatEventType.BLACK_MARKET, (int)eBlackMarket);

            gameServer().gameEventsServer().AddSteamStat(this, infos().blackMarket(eBlackMarket).mzType);
        }

        public virtual void scan(TileServer pTile)
        {
            using (new UnityProfileScope("Player::scan"))
            {
                if (!canScan(pTile))
                {
                    return;
                }

                if (gameServer().isTurnBasedScanning() && wasEverHuman())
                {
                    gameServer().changeTurnBasedTime(gameServer().getScanDelay());
                }
                else
                {
                    changeScans(-1);
                }

                int iRange = innerScanRange();

                foreach (TileServer pLoopTile in gameServer().tileServerRangeIterator(pTile, iRange))
                {
                    pLoopTile.increaseVisibility(getPlayer(), VisibilityType.VISIBLE, true);
                }

                iRange = outerScanRange();

                foreach (TileServer pLoopTile in gameServer().tileServerRangeIterator(pTile, iRange))
                {
                    pLoopTile.increaseVisibility(getPlayer(), VisibilityType.REVEALED, true);
                }

                gameServer().gameEventsServer().AddScan(getPlayer(), pTile.getID());
            }
        }

        public virtual void sabotage(TileServer pTile, SabotageType eSabotage)
        {
            if (!canSabotage(pTile, eSabotage))
            {
                return;
            }

            HashSet<PlayerType> targetedPlayerSet = new HashSet<PlayerType>();
            HashSet<PlayerType> targetedPlayerSetReal = new HashSet<PlayerType>();

            changeSabotageCount(eSabotage, -1);
            PlayerType originalOwner = PlayerType.NONE;

            InfoSabotage sabotageInfo = infos().sabotage(eSabotage);

            if ((pTile.getDefendSabotage() != SabotageType.NONE) && pTile.isClaimed() && sabotageInfo.mbTriggersDefense)
            {
                pTile.ownerServer().changeSabotageCount(eSabotage, 1);

                targetedPlayerSet.Add(pTile.getOwner());

                if (pTile.isOwnerReal())
                {
                    targetedPlayerSetReal.Add(pTile.getOwner());

                    if (isHuman() && !(pTile.ownerServer().isHumanTeam()))
                    {
                        pTile.ownerServer().AI_quip(getPlayer(), infos().character(pTile.ownerServer().getCharacter()).meQuipCaughtSabotage);
                    }
                }

                gameServer().gameEventsServer().AddSabotageReversalEvent(pTile.getDefendSabotage(), eSabotage, pTile.getID(), getPlayer(), pTile.getOwner());
                gameServer().gameEventsServer().AddSteamStat(pTile.ownerServer(), "CaughtSabotage");

                gameServer().statsServer().addEvent(getPlayer(), StatEventType.SABOTAGE_CAUGHT, (int)pTile.getOwner(), (int)eSabotage);

                if (sabotageInfo.mbHostile)
                {
                    gameServer().statsServer().changeStat(StatsType.SABOTAGED, (int)SabotagedStatType.CAUGHT, pTile.getOwner(), (int)eSabotage, 1);
                }

                pTile.setDefendTime(infos().sabotage(pTile.getDefendSabotage()).miDefendTime);
                pTile.setDefendSabotage(SabotageType.NONE);

                if (isHuman())
                {
                    if (pTile.isOwnerReal())
                    {
                        if (!(pTile.ownerServer().wasEverHuman()))
                        {
                            gameServer().gameEventsServer().AddAchievement(getPlayer(), infos().getType<AchievementType>("ACHIEVEMENT_AI_CAUGHT_SABOTAGE"));
                        }
                    }
                }
            }
            else
            {
                List<int> affectedTiles = new List<int>();
                List<int> affectedUnits = new List<int>();
                List<int> frozenTimes = new List<int>();

                AchievementType eDoubleAchievement = infos().getType<AchievementType>("ACHIEVEMENT_BOOST_TWO_OFFWORLDS");
                int iDoubleAchievementCount = 0;

                {
                    int iRange = sabotageInfo.miDestroyUnitRange;
                    if (iRange > 0)
                    {
                        iRange++;

                        List<UnitServer> apUnits = new List<UnitServer>();

                        foreach (KeyValuePair<int, UnitClient> pair in gameServer().getUnitDictionary())
                        {
                            apUnits.Add((UnitServer)(pair.Value));
                        }

                        int iCount = 0;

                        foreach (UnitServer pLoopUnit in apUnits)
                        {
                            if (!(infos().unit(pLoopUnit.getType()).mbImmuneDestroy))
                            {
                                if (Utils.stepDistanceTile(pTile, pLoopUnit.tileServer()) <= iRange)
                                {
                                    MissionType eOldMission = pLoopUnit.getMissionInfo().meMission;
                                    int iOldData = pLoopUnit.getMissionInfo().miData;

                                    pLoopUnit.ownerServer().killUnit(pLoopUnit, true, false);

                                    if ((eOldMission == MissionType.CONSTRUCT) ||
                                        (eOldMission == MissionType.REPAIR))
                                    {
                                        HQServer pHQ = pLoopUnit.ownerServer().findClosestHQServer(pLoopUnit.tileServer());

                                        if (pHQ != null)
                                        {
                                            pLoopUnit.ownerServer().createUnit(pLoopUnit.getType(), pLoopUnit.getConstructionType(), pHQ.tileServer()).setMissionInfo(eOldMission, iOldData, false);
                                        }
                                    }

                                    targetedPlayerSet.Add(pLoopUnit.getOwner());
                                    targetedPlayerSetReal.Add(pLoopUnit.getOwner());

                                    affectedUnits.Add(pLoopUnit.getID());
                                    affectedTiles.Add(pLoopUnit.tileServer().getID());
                                    frozenTimes.Add(0);

                                    iCount++;
                                }
                            }
                        }

                        if (isHuman())
                        {
                            AchievementType eAchievement = infos().getType<AchievementType>("ACHIEVEMENT_DESTROY_10_UNITS");

                            if (iCount >= infos().achievement(eAchievement).miValue)
                            {
                                gameServer().gameEventsServer().AddAchievement(getPlayer(), eAchievement);
                            }
                        }
                    }
                }

                {
                    int iTime = sabotageInfo.miEffectTime;
                    if (iTime > 0)
                    {
                        int iRange = sabotageInfo.miEffectRange;
                        if (iRange > 0)
                        {
                            AchievementType eEMPAchievement = infos().getType<AchievementType>("ACHIEVEMENT_EMP_20_BUILDINGS");
                            int iEMPAchievementCount = 0;

                            foreach (TileServer pLoopTile in gameServer().tileServerRangeIterator(pTile, iRange))
                            {
                                if (canSabotageTile(pLoopTile, eSabotage))
                                {
                                    if ((pLoopTile.getDefendSabotage() != SabotageType.NONE) && sabotageInfo.mbTriggersDefense)
                                    {
                                        pLoopTile.setRevealDefendSabotage(true);
                                    }
                                    else
                                    {
                                        if (pLoopTile.isClaimed())
                                        {
                                            int iAdjustedTime = iTime;
                                            int iEffectTime = 0;

                                            iAdjustedTime /= (Utils.stepDistanceTile(pTile, pLoopTile) + 1);

                                            iAdjustedTime *= (10 + 0);
                                            iAdjustedTime /= (10 + pLoopTile.getSabotagedCount(eSabotage));

                                            if (sabotageInfo.mbDoubleBuilding)
                                            {
                                                if (pLoopTile.getDoubleTime() < iAdjustedTime)
                                                {
                                                    pLoopTile.setDoubleTime(iAdjustedTime);
                                                    iEffectTime = Math.Max(iEffectTime, iAdjustedTime);

                                                    if (pLoopTile.getTeam() == getTeam())
                                                    {
                                                        if (pLoopTile.getBuildingType() == infos().achievement(eDoubleAchievement).meBuilding)
                                                        {
                                                            iDoubleAchievementCount++;
                                                        }
                                                    }
                                                }
                                            }

                                            if (pLoopTile.isOwnerReal())
                                            {
                                                if (pLoopTile.adjacentToHQ(pLoopTile.getOwner()))
                                                {
                                                    iAdjustedTime *= Math.Max(0, (pLoopTile.ownerServer().getAdjacentHQSabotageModifier() + 100));
                                                    iAdjustedTime /= 100;
                                                }
                                            }

                                            if (sabotageInfo.mbHalfBuilding)
                                            {
                                                if (pLoopTile.getHalfTime() < iAdjustedTime)
                                                {
                                                    pLoopTile.setHalfTime(iAdjustedTime);
                                                    iEffectTime = Math.Max(iEffectTime, iAdjustedTime);
                                                }
                                            }

                                            if (sabotageInfo.mbOverloadBuilding)
                                            {
                                                if (pLoopTile.getOverloadTime() < iAdjustedTime)
                                                {
                                                    pLoopTile.setOverloadTime(iAdjustedTime);
                                                    iEffectTime = Math.Max(iEffectTime, iAdjustedTime);
                                                }
                                            }

                                            if (sabotageInfo.mbVirusBuilding)
                                            {
                                                if (pLoopTile.getVirusTime() < iAdjustedTime)
                                                {
                                                    pLoopTile.setVirusTime(iAdjustedTime);
                                                    iEffectTime = Math.Max(iEffectTime, iAdjustedTime);

                                                    testVirusAchievement(pLoopTile);
                                                }

                                                if (pLoopTile.isHologram())
                                                {
                                                    pLoopTile.setHologram(false);

                                                    BuildingType eBuilding = pLoopTile.getConstructionOrBuildingType();

                                                    if ((eBuilding != BuildingType.NONE) && pLoopTile.isOwnerReal())
                                                    {
                                                        gameServer().gameEventsServer().AddHologramRevealed(pLoopTile.getOwner(), eBuilding, pTile.getID());
                                                    }

                                                    if (isHuman())
                                                    {
                                                        gameServer().gameEventsServer().AddAchievement(getPlayer(), infos().getType<AchievementType>("ACHIEVEMENT_DESTROY_HOLOGRAM"));
                                                    }
                                                }
                                            }

                                            if (sabotageInfo.mbFreezeBuilding)
                                            {
                                                int iFrozenTime = iAdjustedTime;

                                                if (pLoopTile.isOwnerReal())
                                                {
                                                    iFrozenTime *= Math.Max(0, (infos().HQ(pLoopTile.ownerServer().getHQ()).miFrozenEffectModifier + 100));
                                                    iFrozenTime /= 100;
                                                }

                                                if (pLoopTile.getFrozenTime() < iFrozenTime)
                                                {
                                                    pLoopTile.setFrozenTime(iFrozenTime);
                                                    pLoopTile.setFrozenSabotage(eSabotage);

                                                    iEffectTime = Math.Max(iEffectTime, iFrozenTime);
                                                }
                                            }

                                            targetedPlayerSet.Add(pLoopTile.getOwner());

                                            if (pLoopTile.isOwnerReal())
                                            {
                                                targetedPlayerSetReal.Add(pLoopTile.getOwner());
                                            }

                                            affectedTiles.Add(pLoopTile.getID());
                                            frozenTimes.Add(iEffectTime);

                                            if (infos().achievement(eEMPAchievement).meSabotage == eSabotage)
                                            {
                                                iEMPAchievementCount++;
                                            }
                                        }
                                    }
                                }
                            }

                            if (isHuman())
                            {
                                if (iEMPAchievementCount >= infos().achievement(eEMPAchievement).miValue)
                                {
                                    gameServer().gameEventsServer().AddAchievement(getPlayer(), eEMPAchievement);
                                }
                            }
                        }


                        int iLength = sabotageInfo.miEffectLength;
                        if (iLength > 0)
                        {
                            AchievementType eSurgeAchievement = infos().getType<AchievementType>("ACHIEVEMENT_MAXIMUM_SURGE");
                            int iSurgeAchievementCount = 0;

                            TileServer pLoopTile = pTile;

                            for (int i = 0; i < iLength; i++)
                            {
                                if ((pLoopTile.getDefendSabotage() != SabotageType.NONE) && sabotageInfo.mbTriggersDefense)
                                {
                                    pLoopTile.setRevealDefendSabotage(true);
                                    break;
                                }
                                else
                                {
                                    {
                                        int iAdjustedTime = ((iTime * (iLength - i)) / iLength);
                                        int iEffectTime = 0;

                                        iAdjustedTime *= (10 + 0);
                                        iAdjustedTime /= (10 + pLoopTile.getSabotagedCount(eSabotage));

                                        if (sabotageInfo.mbDoubleBuilding)
                                        {
                                            if (pLoopTile.getDoubleTime() < iAdjustedTime)
                                            {
                                                pLoopTile.setDoubleTime(iAdjustedTime);
                                                iEffectTime = Math.Max(iEffectTime, iAdjustedTime);

                                                if (pLoopTile.getTeam() == getTeam())
                                                {
                                                    if (pLoopTile.getBuildingType() == infos().achievement(eDoubleAchievement).meBuilding)
                                                    {
                                                        iDoubleAchievementCount++;
                                                    }
                                                }
                                            }
                                        }

                                        if (pLoopTile.isOwnerReal())
                                        {
                                            if (pLoopTile.adjacentToHQ(pLoopTile.getOwner()))
                                            {
                                                iAdjustedTime *= Math.Max(0, (pLoopTile.ownerServer().getAdjacentHQSabotageModifier() + 100));
                                                iAdjustedTime /= 100;
                                            }
                                        }

                                        if (sabotageInfo.mbHalfBuilding)
                                        {
                                            if (pLoopTile.getHalfTime() < iAdjustedTime)
                                            {
                                                pLoopTile.setHalfTime(iAdjustedTime);
                                                iEffectTime = Math.Max(iEffectTime, iAdjustedTime);
                                            }
                                        }

                                        if (sabotageInfo.mbOverloadBuilding)
                                        {
                                            if (pLoopTile.getOverloadTime() < iAdjustedTime)
                                            {
                                                pLoopTile.setOverloadTime(iAdjustedTime);
                                                iEffectTime = Math.Max(iEffectTime, iAdjustedTime);
                                            }
                                        }

                                        if (sabotageInfo.mbVirusBuilding)
                                        {
                                            if (pLoopTile.getVirusTime() < iAdjustedTime)
                                            {
                                                pLoopTile.setVirusTime(iAdjustedTime);
                                                iEffectTime = Math.Max(iEffectTime, iAdjustedTime);

                                                testVirusAchievement(pLoopTile);
                                            }

                                            if (pLoopTile.isHologram())
                                            {
                                                pLoopTile.setHologram(false);

                                                BuildingType eBuilding = pLoopTile.getConstructionOrBuildingType();

                                                if ((eBuilding != BuildingType.NONE) && pLoopTile.isOwnerReal())
                                                {
                                                    gameServer().gameEventsServer().AddHologramRevealed(pLoopTile.getOwner(), eBuilding, pTile.getID());
                                                }

                                                if (isHuman())
                                                {
                                                    gameServer().gameEventsServer().AddAchievement(getPlayer(), infos().getType<AchievementType>("ACHIEVEMENT_DESTROY_HOLOGRAM"));
                                                }
                                            }
                                        }

                                        if (sabotageInfo.mbFreezeBuilding)
                                        {
                                            int iFrozenTime = iAdjustedTime;

                                            if (pLoopTile.isOwnerReal())
                                            {
                                                iFrozenTime *= Math.Max(0, (infos().HQ(pLoopTile.ownerServer().getHQ()).miFrozenEffectModifier + 100));
                                                iFrozenTime /= 100;
                                            }

                                            if (pLoopTile.getFrozenTime() < iFrozenTime)
                                            {
                                                pLoopTile.setFrozenTime(iFrozenTime);
                                                pLoopTile.setFrozenSabotage(eSabotage);

                                                iEffectTime = Math.Max(iEffectTime, iFrozenTime);
                                            }
                                        }

                                        targetedPlayerSet.Add(pLoopTile.getOwner());

                                        if (pLoopTile.isOwnerReal())
                                        {
                                            targetedPlayerSetReal.Add(pLoopTile.getOwner());
                                        }

                                        affectedTiles.Add(pLoopTile.getID());
                                        frozenTimes.Add(iEffectTime);

                                        if (infos().achievement(eSurgeAchievement).meSabotage == eSabotage)
                                        {
                                            iSurgeAchievementCount++;
                                        }
                                    }

                                    TileServer pBestTile = null;
                                    int iBestValue = 0;

                                    foreach (TileServer pAdjacentTile in gameServer().tileServerAdjacentAll(pLoopTile))
                                    {
                                        if (canSabotageTile(pAdjacentTile, eSabotage) && !affectedTiles.Contains(pAdjacentTile.getID()))
                                        {
                                            int iValue = gameServer().random().Next(1000) + 1;

                                            if (pAdjacentTile.getDefendSabotage() == SabotageType.NONE)
                                            {
                                                iValue += 1000;
                                            }

                                            if (iValue > iBestValue)
                                            {
                                                pBestTile = pAdjacentTile;
                                                iBestValue = iValue;
                                            }
                                        }
                                    }

                                    if (pBestTile != null)
                                    {
                                        pLoopTile = pBestTile;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }

                            if (isHuman())
                            {
                                if (iSurgeAchievementCount >= infos().achievement(eSurgeAchievement).miValue)
                                {
                                    gameServer().gameEventsServer().AddAchievement(getPlayer(), eSurgeAchievement);
                                }
                            }
                        }
                    }
                }

                if (isHuman())
                {
                    if (iDoubleAchievementCount > 1)
                    {
                        gameServer().gameEventsServer().AddAchievement(getPlayer(), eDoubleAchievement);
                    }
                }

                {
                    int iChange = sabotageInfo.miResourceLevelChange;
                    if (iChange < 0)
                    {
                        if (pTile.isClaimed() && pTile.isOwnerReal())
                        {
                            if (pTile.adjacentToHQ(pTile.getOwner()))
                            {
                                iChange *= Math.Max(0, (pTile.ownerServer().getAdjacentHQSabotageModifier() + 100));
                                iChange /= 100;
                            }
                        }

                        if (iChange == 0)
                        {
                            iChange = -1;
                        }

                        for (int iPass = 0; iPass < -(iChange); iPass++)
                        {
                            foreach (InfoResource pLoopResource in infos().resources())
                            {
                                ResourceLevelType eResourceLevel = pTile.getResourceLevel(pLoopResource.meType, false);

                                if (eResourceLevel > ResourceLevelType.NONE)
                                {
                                    if (infos().resourceLevel(eResourceLevel).mbCanBomb)
                                    {
                                        pTile.changeResourceLevel(pLoopResource.meType, -1);

                                        if (pTile.isClaimed())
                                        {
                                            targetedPlayerSet.Add(pTile.getOwner());

                                            if (pTile.isOwnerReal())
                                            {
                                                targetedPlayerSetReal.Add(pTile.getOwner());
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (isHuman())
                        {
                            int iCount = 0;

                            foreach (InfoResource pLoopResource in infos().resources())
                            {
                                ResourceLevelType eResourceLevel = pTile.getResourceLevel(pLoopResource.meType, false);

                                if (eResourceLevel > ResourceLevelType.NONE)
                                {
                                    if (!(infos().resourceLevel(eResourceLevel).mbCanBomb))
                                    {
                                        iCount++;
                                    }
                                }
                            }

                            if (iCount > 1)
                            {
                                gameServer().gameEventsServer().AddAchievement(getPlayer(), infos().getType<AchievementType>("ACHIEVEMENT_NUKE_TWO_RESOURCES_TRACE"));
                            }
                        }
                    }
                }

                {
                    int iTime = sabotageInfo.miTakeoverTime;
                    if (iTime > 0)
                    {
                        if (pTile.isClaimed() && pTile.isOwnerReal())
                        {
                            if (pTile.adjacentToHQ(pTile.getOwner()))
                            {
                                iTime *= Math.Max(0, (pTile.ownerServer().getAdjacentHQSabotageModifier() + 100));
                                iTime /= 100;
                            }
                        }

                        if (iTime > 0)
                        {
                            if (isHuman())
                            {
                                if (pTile.isShowWrongBuilding() && !(pTile.showCorrectBuilding(getTeam(), true)))
                                {
                                    BuildingType eBuilding = pTile.getBuildingType();

                                    if (eBuilding != BuildingType.NONE)
                                    {
                                        AchievementType eAchievement = infos().getType<AchievementType>("ACHIEVEMENT_MUTINY_HIDDEN_OFFWORLD");

                                        if (infos().achievement(eAchievement).meBuilding == eBuilding)
                                        {
                                            gameServer().gameEventsServer().AddAchievement(getPlayer(), eAchievement);
                                        }
                                    }
                                }
                            }

                            originalOwner = pTile.getOwner();

                            targetedPlayerSet.Add(pTile.getOwner());

                            if (pTile.isOwnerReal())
                            {
                                targetedPlayerSetReal.Add(pTile.getOwner());
                            }

                            pTile.setOwner(getPlayer(), false);
                            pTile.setTakeoverTime((pTile.isOwnerReal()) ? 0 : iTime);

                            if (pTile.isOwnerReal())
                            {
                                pTile.setDefendTime(infos().Globals.TAKEOVER_DEFEND_TIME * 2);
                            }
                            else if(pTile.isBuilding())
                            {
                                pTile.buildingServer().sendResources(true);
                            }

                            if (isHuman())
                            {
                                if (pTile.isOwnerReal())
                                {
                                    gameServer().gameEventsServer().AddAchievement(getPlayer(), infos().getType<AchievementType>("ACHIEVEMENT_MUTINY_BACK"));
                                }
                                else
                                {
                                    if (pTile.isBuilding())
                                    {
                                        for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                                        {
                                            TileClient pAdjacentTile = gameServer().tileServerAdjacent(pTile, eLoopDirection);

                                            if (pAdjacentTile != null)
                                            {
                                                if (pAdjacentTile.isTakeover() && (pAdjacentTile.getOwner() == getPlayer()))
                                                {
                                                    if (pAdjacentTile.getBuildingType() == pTile.getBuildingType())
                                                    {
                                                        gameServer().gameEventsServer().AddAchievement(getPlayer(), infos().getType<AchievementType>("ACHIEVEMENT_MUTINY_ADJACENT"));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                {
                    UnitType eUnit = sabotageInfo.meUnit;

                    if (eUnit != UnitType.NONE)
                    {
                        HQServer pHQ = findClosestHQServer(pTile);
                        TileServer startTile = (sabotageInfo.miPlunderQuantity > 0 || pHQ == null) ? pTile : pHQ.tileServer();
                        UnitServer pUnit = createUnit(eUnit, BuildingType.NONE, startTile);

                        if (pUnit != null)
                        {
                            pUnit.setHarvestQuantity(sabotageInfo.miHarvestQuantity);
                            pUnit.setPlunderQuantity(sabotageInfo.miPlunderQuantity);
                            pUnit.setSabotageType(eSabotage);

                            if (pUnit.getHarvestQuantity() > 0)
                            {
                                pUnit.setMissionInfo(MissionType.MINE, pTile.getID(), false);

                                targetedPlayerSet.Add(getPlayer());
                                targetedPlayerSetReal.Add(getPlayer());
                            }
                        }
                    }
                }

                {
                    int iDamage = sabotageInfo.miDamageBuilding;
                    if (iDamage > 0)
                    {
                        BuildingType eBuilding = pTile.getConstructionOrBuildingType();

                        if (pTile.isClaimed() && pTile.isOwnerReal())
                        {
                            if (pTile.adjacentToHQ(pTile.getOwner()))
                            {
                                iDamage *= Math.Max(0, (pTile.ownerServer().getAdjacentHQSabotageModifier() + 100));
                                iDamage /= 100;
                            }
                        }

                        if (pTile.isBuilding())
                        {
                            destroyBuilding(pTile.buildingServer(), iDamage, false);
                        }
                        else if (pTile.isConstruction())
                        {
                            ConstructionServer pConstruction = pTile.constructionServer();

                            pConstruction.setDamage(iDamage);
                        }

                        targetedPlayerSet.Add(pTile.getOwner());

                        if (pTile.isOwnerReal())
                        {
                            targetedPlayerSetReal.Add(pTile.getOwner());
                        }

                        if (isHuman())
                        {
                            if (pTile.getRealOwner() == getPlayer())
                            {
                                gameServer().gameEventsServer().AddAchievement(getPlayer(), infos().getType<AchievementType>("ACHIEVEMENT_DYNAMITE_OWN_BUILDING"));
                            }

                            {
                                AchievementType eAchievement = infos().getType<AchievementType>("ACHIEVEMENT_DYNAMITE_OFFWORLD_MARKET");

                                if (eBuilding == infos().achievement(eAchievement).meBuilding)
                                {
                                    gameServer().gameEventsServer().AddAchievement(getPlayer(), eAchievement);
                                }
                            }
                        }
                    }
                }

                if (sabotageInfo.mbReturnClaim)
                {
                    returnClaim(pTile, true);

                    targetedPlayerSet.Add(getPlayer());
                    targetedPlayerSetReal.Add(getPlayer());
                }

                if (sabotageInfo.mbAuctionTile)
                {
                    gameServer().startAuction(AuctionType.TILE, pTile.getID(), -1);

                    targetedPlayerSet.Add(getPlayer());
                    targetedPlayerSetReal.Add(getPlayer());
                }

                if (sabotageInfo.mbWrongBuilding)
                {
                    pTile.setHologram(true);

                    targetedPlayerSet.Add(pTile.getOwner());

                    if (!(sabotageInfo.mbAnnounce))
                    {
                        targetedPlayerSetReal.Add(pTile.getRealOwner());
                    }
                    else if (pTile.isOwnerReal())
                    {
                        targetedPlayerSetReal.Add(pTile.getOwner());
                    }
                }

                if (sabotageInfo.mbRevealBuilding)
                {
                    pTile.setRevealBuilding(getTeam(), true);

                    targetedPlayerSet.Add(pTile.getOwner());

                    if (!(sabotageInfo.mbAnnounce))
                    {
                        targetedPlayerSetReal.Add(pTile.getRealOwner());
                    }
                    else if (pTile.isOwnerReal())
                    {
                        targetedPlayerSetReal.Add(pTile.getOwner());
                    }

                    if (isHuman())
                    {
                        int iRange = 1;

                        for (int iDX = -(iRange); iDX <= iRange; iDX++)
                        {
                            for (int iDY = -(iRange); iDY <= iRange; iDY++)
                            {
                                TileServer pRangeTile = gameServer().tileServerRange(pTile, iDX, iDY, iRange);

                                if (pRangeTile != null)
                                {
                                    if (pRangeTile.isHologram() && (pRangeTile.getTeam() != getTeam()))
                                    {
                                        gameServer().gameEventsServer().AddAchievement(getPlayer(), infos().getType<AchievementType>("ACHIEVEMENT_REVEALED_HOLOGRAM"));
                                    }
                                }
                            }
                        }
                    }
                }

                if (sabotageInfo.mbNewResource)
                {
                    TerrainGenerator.addResource(gameServer(), pTile, true);

                    targetedPlayerSet.Add(getPlayer());
                    targetedPlayerSetReal.Add(getPlayer());

                    pTile.setClaimBlockPlayer(getPlayer());
                    pTile.setClaimBlockTime(infos().Globals.NEW_RESOURCE_CLAIM_BLOCK_TIME);

                    if (isHuman())
                    {
                        if (pTile.getResourceCount() > 1)
                        {
                            gameServer().gameEventsServer().AddAchievement(getPlayer(), infos().getType<AchievementType>("ACHIEVEMENT_MULTIPLE_NEW_RESOURCES"));
                        }
                    }
                }

                if(sabotageInfo.mbChangeTerrain)
                {
                    pTile.changeTerrain(infos().Globals.CAVE_TERRAIN);

                    targetedPlayerSet.Add(getPlayer());
                    targetedPlayerSetReal.Add(getPlayer());

                    pTile.setClaimBlockPlayer(getPlayer());
                    pTile.setClaimBlockTime(infos().Globals.NEW_RESOURCE_CLAIM_BLOCK_TIME);

                    gameServer().addCaveTile(pTile.getID());
                }

                if (sabotageInfo.mbDefendSabotage)
                {
                    pTile.setDefendSabotage(eSabotage);

                    targetedPlayerSet.Add(pTile.getOwner());

                    if (!(sabotageInfo.mbAnnounce))
                    {
                        targetedPlayerSetReal.Add(pTile.getRealOwner());
                    }
                    else if (pTile.isOwnerReal())
                    {
                        targetedPlayerSetReal.Add(pTile.getOwner());
                    }

                    if (isHuman())
                    {
                        if ((pTile.getOwner() == getPlayer()) && (pTile.getRealOwner() != getPlayer()))
                        {
                            gameServer().gameEventsServer().AddAchievement(getPlayer(), infos().getType<AchievementType>("ACHIEVEMENT_DEFEND_MUTINY_BUILDING"));
                        }
                    }
                }

                pTile.incrementSabotagedCount(eSabotage);

                foreach (int iTile in affectedTiles)
                {
                    TileServer pLoopTile = gameServer().tileServer(iTile);

                    if (pLoopTile != pTile)
                    {
                        pLoopTile.incrementSabotagedCount(eSabotage);
                    }
                }

                if (pTile.isClaimed() && pTile.isOwnerReal())
                {
                    if (isHuman() && !(pTile.ownerServer().isHumanTeam()))
                    {
                        if (gameServer().randomMisc().Next(5) == 0)
                        {
                            pTile.ownerServer().AI_quip(getPlayer(), infos().character(pTile.ownerServer().getCharacter()).meQuipHitSabotage);
                        }
                    }
                }

                foreach (PlayerType eLoopPlayer in targetedPlayerSet)
                {
                    gameServer().statsServer().addEvent(getPlayer(), StatEventType.SABOTAGE, (int)eLoopPlayer, (int)eSabotage);
                }

                gameServer().gameEventsServer().AddSabotageEvent(pTile, getPlayer(), originalOwner, affectedTiles, affectedUnits, frozenTimes, targetedPlayerSetReal, eSabotage);
            }

            if (sabotageInfo.mbHostile)
            {
                foreach (PlayerType eLoopPlayer in targetedPlayerSet)
                {
                    gameServer().playerServer(eLoopPlayer).incrementSabotagedCount();

                    gameServer().statsServer().changeStat(StatsType.SABOTAGING, (int)SabotagingStatType.TARGET, getPlayer(), (int)eLoopPlayer, 1);
                    gameServer().statsServer().changeStat(StatsType.SABOTAGED, (int)SabotagedStatType.DEFENDED, eLoopPlayer, (int)eSabotage, 1);
                }

                gameServer().statsServer().changeStat(StatsType.SABOTAGED, (int)SabotagedStatType.ATTACKED, getPlayer(), (int)eSabotage, 1);
            }
        }

        void testVirusAchievement(TileServer pTile)
        {
            if (!isHuman())
            {
                return;
            }

            if (!(pTile.isOwnerReal()))
            {
                return;
            }

            BuildingServer pBuilding = pTile.buildingServer();

            if (pBuilding != null)
            {
                int iNet = gameServer().calculateRevenue(pBuilding.getType(), pTile, pBuilding.getOwner(), pBuilding.getConnections(), false, false, 0, 0, 100, false);
                if (iNet < 0)
                {
                    gameServer().gameEventsServer().AddAchievement(getPlayer(), infos().getType<AchievementType>("ACHIEVEMENT_NETWORK_VIRUS_UNPROFITABLE"));
                }
            }
        }

        public virtual HQServer found(TileServer pTile, HQType eHQ)
        {
            using (new UnityProfileScope("PlayerServer.found"))
            {
                using (new UnityProfileScope("PlayerServer.canFound"))
                {
                    if (!canFound(pTile, eHQ, true))
                    {
                        return null;
                    }
                }

                using (new UnityProfileScope("PlayerServer.setFoundHQStats"))
                {
                    setHQ(eHQ);

                    if (getNumHQs() == 0)
                    {
                        setHQClaimBonus(gameServer().getFoundOrderClaimBonus(gameServer().countPlayersStarted() + 1));
                        setBlackMarketTime(gameServer().getFoundOrderBlackMarketBonus(gameServer().countPlayersStarted() + 1));

                        gameServer().statsServer().changeStat(StatsType.MISCELLANEOUS, (int)MiscellaneousStatType.FOUNDING_TURN, getPlayer(), 0, gameServer().getTurnCount());

                        if (gameServer().isGameOption(GameOptionType.REVEAL_MAP))
                        {
                            int iFoundMoney = gameServer().getFoundMoney();
                            if (iFoundMoney > 0)
                            {
                                changeMoney(iFoundMoney);
                                gameServer().statsServer().changeStat(StatsType.MISCELLANEOUS, (int)MiscellaneousStatType.STARTING_MONEY, getPlayer(), 0, iFoundMoney);

                                {
                                    int iNewMoney = (gameServer().getFoundMoney() / 2);
                                    iNewMoney -= (iNewMoney % 100);
                                    gameServer().setFoundMoney(iNewMoney);
                                }
                            }
                            else if (iFoundMoney < 0)
                            {
                                changeDebt(iFoundMoney);
                                gameServer().statsServer().changeStat(StatsType.MISCELLANEOUS, (int)MiscellaneousStatType.STARTING_DEBT, getPlayer(), 0, iFoundMoney);
                            }
                        }

                        {
                            HashSet<int> siTiles = new HashSet<int>();
                            if (gameServer().isCampaign())
                            {
                                IEnumerable<TileClient> hqFootprint = HQClient.getHQFootprint(pTile, gameClient(), infos().HQ(eHQ));

                                hqFootprint.ForEach(tile => siTiles.Add(tile.getID()));

                                for (PerkType eLoopPerk = 0; eLoopPerk < infos().perksNum(); eLoopPerk++)
                                {
                                    if (infos().perk(eLoopPerk).mbOnFound)
                                    {
                                        int iCount = getPerkCount(eLoopPerk);

                                        for (int i = 0; i < iCount; i++)
                                        {
                                            doPerk(eLoopPerk, pTile, siTiles);
                                        }
                                    }
                                }
                            }

                            for (BuildingType eLoopBuilding = 0; eLoopBuilding < Globals.Infos.buildingsNum(); eLoopBuilding++)
                            {
                                for (int iBuildingIndex = 0; iBuildingIndex < getFreeBuildings()[(int)eLoopBuilding]; iBuildingIndex++)
                                {
                                    placeFreeBuilding(eLoopBuilding, pTile, siTiles);
                                }
                            }
                        }
                    }
                }

                HQServer pHQ = createHQ(pTile);

                using (new UnityProfileScope("PlayerServer.foundIncreaseVisibility"))
                {
                    gameServer().tileServerAll().ForEach(tile => tile.increaseVisibility(getPlayer(), VisibilityType.VISIBLE, false));
                }

                using (new UnityProfileScope("PlayerServer.makeHQFounded"))
                {
                    if (getNumHQs() == (1 + infos().HQ(getHQ()).miExtraHQs))
                    {
                        makeHQFounded(pTile);
                    }
                }

                return pHQ;
            }
        }

        public virtual void upgrade()
        {
            if (!canUpgrade(true))
            {
                return;
            }

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                int iCost = getUpgradeResourceCost(eLoopResource, getHQLevel());
                if (iCost > 0)
                {
                    spend(eLoopResource, iCost);
                    gameServer().statsServer().changeStat(StatsType.RESOURCE, (int)ResourceStatType.UPGRADE_CONSUMED, getPlayer(), (int)eLoopResource, (iCost * Constants.RESOURCE_MULTIPLIER));
                }
            }

            incrementHQLevel();

            changeClaims(getLevelClaims(getHQLevel()));

            if (getHQClaimBonus() == getHQLevel())
            {
                setHQClaimBonus(HQLevelType.NONE);
            }

            for (SabotageType eLoopSabotage = 0; eLoopSabotage < infos().sabotagesNum(); eLoopSabotage++)
            {
                changeSabotageCount(eLoopSabotage, infos().HQ(getHQ()).maiUpgradeSabotage[(int)eLoopSabotage]);
            }

            if (gameServer().isCampaign())
            {
                TileServer pTile = startingHQServer().tileServer();

                foreach (InfoPerk pLoopPerk in Globals.Infos.perks())
                {
                    if (pLoopPerk.mbOnUpgrade)
                    {
                        int iCount = getPerkCount(pLoopPerk.meType);

                        for (int i = 0; i < iCount; i++)
                        {
                            doPerk(pLoopPerk.meType, pTile);
                        }
                    }
                }
            }

            gameServer().gameEventsServer().AddHQUpgrade(getPlayer(), getHQLevel());
            gameServer().statsServer().addEvent(getPlayer(), StatEventType.UPGRADE, (int)getHQLevel());

            if (!isHumanTeam())
            {
                bool bHighest = true;

                foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAliveAll())
                {
                    if (pLoopPlayer.getPlayer() != getPlayer())
                    {
                        if (pLoopPlayer.getHQLevel() >= getHQLevel())
                        {
                            bHighest = false;
                            break;
                        }
                    }
                }

                if (bHighest)
                {
                    TextType eText = infos().character(getCharacter()).maeQuipUpgradeFirst[(int)getHQLevel()];

                    if (eText != TextType.NONE)
                    {
                        AI_quip(PlayerType.NONE, eText);
                    }
                }
            }
        }

        public virtual void startClaim(TileServer pTile)
        {
            if (!canClaimTile(pTile, true))
            {
                return;
            }

            if (infos().rulesSet(gameServer().getRulesSet()).mbNoClaiming)
            {
                return;
            }

            finishClaim(pTile, BuildingType.NONE, false);
        }

        public virtual bool finishClaim(TileServer pTile, BuildingType eBuilding, bool bSpend)
        {
            if (pTile.getOwner() != getPlayer())
            {
                if (!canClaimTile(pTile, true))
                {
                    return false;
                }

                changeClaims(-1);

                pTile.setOwner(getPlayer(), true);
                if (pTile.getFirstClaimTurn() == 0)
                {
                    pTile.setFirstClaimTurn(gameClient().getTurnCount());
                }
            }

            if (eBuilding != BuildingType.NONE)
            {
                createConstruction(eBuilding, pTile, bSpend);

                gameServer().gameEventsServer().AddConstructionStarted(getPlayer(), eBuilding, pTile.getID());
            }
            else
            {
                gameServer().gameEventsServer().AddClaimFinish(getPlayer(), pTile.getID());
            }

            return true;
        }

        public virtual void returnClaim(TileServer pTile, bool bSabotage)
        {
            if (!canReturnClaim(pTile, bSabotage))
            {
                return;
            }

            pTile.setOwner(PlayerType.NONE, true);
            changeClaims(1);

            pTile.setDoubleTime(0);
            pTile.setTakeoverTime(0);
        }

        public virtual void cancelConstruct(TileServer pTile)
        {
            if (!canCancelConstruct(pTile))
            {
                return;
            }

            {
                HashSet<int> unitSet = new HashSet<int>();

                foreach (int iUnitID in getUnitList())
                {
                    unitSet.Add(iUnitID);
                }

                foreach (int iUnitID in unitSet)
                {
                    UnitServer pLoopUnit = gameServer().unitServer(iUnitID);

                    if ((pLoopUnit.getMissionInfo().meMission == MissionType.CONSTRUCT) &&
                        (pLoopUnit.getMissionInfo().miData == pTile.getID()))
                    {
                        killUnit(pLoopUnit, true, false);
                    }
                }
            }
        }

        public virtual bool construct(TileServer pTile, BuildingType eBuilding)
        {
            if (!canConstruct(pTile, eBuilding, true, true))
            {
                return false;
            }

            bool bSuccess = false;

            HQServer pHQ = findClosestHQServer(pTile);

            if (pHQ != null)
            {
                if (pTile.isPotentialHQConnection(getPlayer()))
                {
                    if (finishClaim(pTile, eBuilding, true))
                    {
                        bSuccess = true;
                    }
                }
                else
                {
                    if (finishClaim(pTile, BuildingType.NONE, false))
                    {
                        UnitServer pUnit = createUnit(infos().Globals.CLAIM_UNIT, eBuilding, pHQ.tileServer());

                        if (pUnit != null)
                        {
                            pUnit.setMissionInfo(MissionType.CONSTRUCT, pTile.getID(), false);
                        }

                        bSuccess = true;
                    }
                }
            }

            if (bSuccess)
            {
                if (!isHuman())
                {
                    AI_updateRates();
                }
            }

            return bSuccess;
        }

        public virtual void sendPatent(PatentType ePatent, PlayerType ePlayer)
        {
            if (!canSendPatent(ePatent, ePlayer))
            {
                return;
            }

            setPatentAcquiredLab(ePatent, false, false, false);
            gameServer().playerServer(ePlayer).setPatentAcquiredLab(ePatent, true, false, false);

            gameServer().gameEventsServer().AddPatentSent(getPlayer(), ePlayer, ePatent);
        }

        public virtual void patent(PatentType ePatent)
        {
            addOrder(OrderType.PATENT, infos().patent(ePatent).miTime, (int)ePatent);
        }

        public virtual void research(TechnologyType eTechnology)
        {
            addOrder(OrderType.RESEARCH, getResearchTime(eTechnology, getTechnologyLevelResearching(eTechnology) + 1, infos()), (int)eTechnology);
        }

        public virtual void espionage(EspionageType eEspionage)
        {
            addOrder(OrderType.HACK, infos().espionage(eEspionage).miTime, (int)eEspionage);
        }

        public virtual void launch(ResourceType eResource)
        {
            addOrder(OrderType.LAUNCH, infos().Globals.LAUNCH_TIME, (int)eResource);
        }

        public virtual HQServer findClosestHQServer(TileServer tile)
        {
            return (HQServer)findClosestHQClient(tile);
        }

        public virtual void destroyBuilding(BuildingServer pBuilding, int iDamage, bool bEvent)
        {
            TileServer pTile = pBuilding.tileServer();
            BuildingType eBuilding = pBuilding.getType();

            pBuilding.ownerServer().killBuilding(pBuilding);

            ConstructionServer pConstruction = pTile.ownerServer().createConstruction(eBuilding, pTile, false);

            pConstruction.setDamage(iDamage);

            if (bEvent)
            {
                makeBuildingDestroyEvent(eBuilding);
            }

            gameServer().gameEventsServer().AddBuildingDestroyed(getPlayer(), pTile.getID(), eBuilding);
        }
        public virtual void destroyBuildingEvent(int iTime, TerrainType eAffectedTerrain)
        {
            BuildingServer pBestBuilding = null;
            int iBestValue = 0;

            foreach (int iBuilding in getBuildingList())
            {
                BuildingServer pLoopBuilding = gameServer().buildingServer(iBuilding);

                if (pLoopBuilding.tileServer().isOwnerReal())
                {
                    if (pLoopBuilding.tileServer().getDestroyTime() == 0)
                    {
                        if (infos().building(pLoopBuilding.getType()).mbEventDestroy && (eAffectedTerrain == TerrainType.NONE || pLoopBuilding.tileServer().getTerrain() == eAffectedTerrain))
                        {
                            int iValue = gameServer().random().Next(1000) + 1;
                            if (iValue > iBestValue)
                            {
                                pBestBuilding = pLoopBuilding;
                                iBestValue = iValue;
                            }
                        }
                    }
                }
            }

            if (pBestBuilding != null)
            {
                pBestBuilding.tileServer().setDestroyTime(iTime);
            }
        }

        public virtual void scrapAll(TileServer pTile)
        {
            BuildingType eBuilding = BuildingType.NONE;

            if (pTile.isBuilding())
            {
                BuildingServer pBuilding = pTile.buildingServer();

                if (pBuilding != null)
                {
                    if (pBuilding.canScrap())
                    {
                        eBuilding = pBuilding.getType();
                        pBuilding.scrap();
                    }
                }
            }
            else if (pTile.isConstruction())
            {
                ConstructionServer pConstruction = pTile.constructionServer();

                if (pConstruction != null)
                {
                    if (pConstruction.canAbandon())
                    {
                        eBuilding = pConstruction.getType();
                        pConstruction.abandon();
                    }
                }
            }

            if (eBuilding != BuildingType.NONE)
            {
                for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                {
                    TileServer pAdjacentTile = gameServer().tileServerAdjacent(pTile, eLoopDirection);

                    if (pAdjacentTile != null)
                    {
                        if (pAdjacentTile.getOwner() == getPlayer())
                        {
                            bool bContinue = false;

                            if (pAdjacentTile.isBuilding())
                            {
                                BuildingServer pLoopBuilding = pAdjacentTile.buildingServer();

                                if (pLoopBuilding != null)
                                {
                                    if (pLoopBuilding.getType() == eBuilding)
                                    {
                                        bContinue = true;
                                    }
                                }
                            }
                            else if (pAdjacentTile.isConstruction())
                            {
                                ConstructionServer pLoopConstruction = pAdjacentTile.constructionServer();

                                if (pLoopConstruction != null)
                                {
                                    if (pLoopConstruction.getType() == eBuilding)
                                    {
                                        bContinue = true;
                                    }
                                }
                            }

                            if (bContinue)
                            {
                                bool bValid = true;

                                if (Utils.isBuildingMiningAny(eBuilding))
                                {
                                    bValid = false;

                                    foreach (InfoResource pLoopResource in infos().resources())
                                    {
                                        if ((gameServer().resourceMiningTile(eBuilding, pLoopResource.meType, pTile, getPlayer(), infos().building(eBuilding).maiResourceMining[pLoopResource.miType], 0, true) > 0) &&
                                            (gameServer().resourceMiningTile(eBuilding, pLoopResource.meType, pAdjacentTile, getPlayer(), infos().building(eBuilding).maiResourceMining[pLoopResource.miType], 0, true) > 0))
                                        {
                                            bValid = true;
                                            break;
                                        }
                                    }

                                    if (bValid)
                                    {
                                        foreach (InfoResource pLoopResource in infos().resources())
                                        {
                                            if ((gameServer().resourceMiningTile(eBuilding, pLoopResource.meType, pTile, getPlayer(), infos().building(eBuilding).maiResourceMining[pLoopResource.miType], 0, true) == 0) &&
                                                (gameServer().resourceMiningTile(eBuilding, pLoopResource.meType, pAdjacentTile, getPlayer(), infos().building(eBuilding).maiResourceMining[pLoopResource.miType], 0, true) > 0))
                                            {
                                                bValid = false;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (bValid)
                                {
                                    scrapAll(pAdjacentTile);
                                }
                            }
                        }
                    }
                }
            }
        }

        public virtual void sendResourcesBuilding(BuildingServer pBuilding)
        {
            BuildingType eBuilding = pBuilding.getType();

            foreach (int iBuilding in getBuildingList())
            {
                BuildingServer pLoopBuilding = gameServer().buildingServer(iBuilding);

                if (pLoopBuilding != pBuilding)
                {
                    if (pLoopBuilding.getType() == eBuilding)
                    {
                        bool bValid = true;

                        if (Utils.isBuildingMiningAny(eBuilding))
                        {
                            bValid = false;

                            foreach (InfoResource pLoopResource in infos().resources())
                            {
                                if ((pBuilding.resourceMining(pLoopResource.meType, true) > 0) &&
                                    (pLoopBuilding.resourceMining(pLoopResource.meType, true) > 0))
                                {
                                    bValid = true;
                                    break;
                                }
                            }
                        }

                        if (bValid)
                        {
                            pLoopBuilding.sendResources(false);
                        }
                    }
                }
            }

            pBuilding.sendResources(false);
        }

        public virtual void toggleAutoOffBuildings(BuildingServer pBuilding)
        {
            BuildingType eBuilding = pBuilding.getType();

            foreach (int iBuilding in getBuildingList())
            {
                BuildingServer pLoopBuilding = gameServer().buildingServer(iBuilding);

                if (pLoopBuilding != pBuilding)
                {
                    if (pLoopBuilding.getType() == eBuilding)
                    {
                        if (pLoopBuilding.isAutoOff() == pBuilding.isAutoOff())
                        {
                            bool bValid = true;

                            if (Utils.isBuildingMiningAny(eBuilding))
                            {
                                bValid = false;

                                foreach (InfoResource pLoopResource in infos().resources())
                                {
                                    if ((pBuilding.resourceMining(pLoopResource.meType, true) > 0) &&
                                        (pLoopBuilding.resourceMining(pLoopResource.meType, true) > 0))
                                    {
                                        bValid = true;
                                        break;
                                    }
                                }
                            }

                            if (bValid)
                            {
                                pLoopBuilding.toggleAutoOff();
                            }
                        }
                    }
                }
            }

            pBuilding.toggleAutoOff();
        }

        public virtual void toggleOnOffBuildings(BuildingServer pBuilding)
        {
            BuildingType eBuilding = pBuilding.getType();

            foreach (int iBuilding in getBuildingList())
            {
                BuildingServer pLoopBuilding = gameServer().buildingServer(iBuilding);

                if (pLoopBuilding != pBuilding)
                {
                    if (pLoopBuilding.getType() == eBuilding)
                    {
                        if (pLoopBuilding.isOff() == pBuilding.isOff())
                        {
                            bool bValid = true;

                            if (Utils.isBuildingMiningAny(eBuilding))
                            {
                                bValid = false;

                                foreach (InfoResource pLoopResource in infos().resources())
                                {
                                    if ((pBuilding.resourceMining(pLoopResource.meType, true) > 0) &&
                                        (pLoopBuilding.resourceMining(pLoopResource.meType, true) > 0))
                                    {
                                        bValid = true;
                                        break;
                                    }
                                }
                            }

                            if (bValid)
                            {
                                pLoopBuilding.toggleOffManual();
                            }
                        }
                    }
                }
            }

            pBuilding.toggleOffManual();
        }

        public virtual void toggleOnOffEverything()
        {
            bool isKnown = false;
            bool turnOff = false;

            foreach (int iBuilding in getBuildingList())
            {
                BuildingServer pLoopBuilding = gameServer().buildingServer(iBuilding);

                if (!(Utils.isBuildingYield(pLoopBuilding.getType(), infos().Globals.ENERGY_RESOURCE, gameServer())))
                {
                    if (!isKnown)
                    {
                        turnOff = !pLoopBuilding.isOff();
                        isKnown = true;
                    }

                    if (isKnown)
                    {
                        if (pLoopBuilding.isOff() != turnOff)
                        {
                            pLoopBuilding.toggleOffManual();
                        }
                    }
                }
            }
        }

        public virtual void setName(string zNewValue)
        {
            mzName = zNewValue;

            makeDirty(PlayerDirtyType.mzName);
        }

        public virtual void setPseudonym(string zNewValue)
        {
            mzPseudonym = zNewValue;

            makeDirty(PlayerDirtyType.mzPseudonym);
        }

        public virtual void setSuffix(string zNewValue)
        {
            mzSuffix = zNewValue;

            makeDirty(PlayerDirtyType.mzSuffix);
        }

        public virtual void setRank(string zNewValue)
        {
            mzRank = ((zNewValue == null) ? "" : zNewValue);

            makeDirty(PlayerDirtyType.mzRank);
        }

        public virtual void setGender(GenderType eNewGender)
        {
            meGender = eNewGender;
            makeDirty(PlayerDirtyType.meGender);
        }

        public virtual void setMoney(long iNewValue)
        {
            if (getMoney() != iNewValue)
            {
                miMoney = iNewValue;
                setShouldCalculateCashResources(true);

                makeDirty(PlayerDirtyType.miMoney);
            }
        }
        public virtual void changeMoney(long iChange, bool bShared = false)
        {
            if (bShared && gameServer().isTeamGame() && !isSubsidiary() && (iChange < 0) && (iChange < -(getMoney())))
            {
                if (getMoney() > 0)
                {
                    iChange += getMoney();
                    setMoney(0);
                }

                for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameServer().getNumPlayers(); eLoopPlayer++)
                {
                    PlayerServer pLoopPlayer = gameServer().playerServer(eLoopPlayer);

                    if (!(pLoopPlayer.isSubsidiary()) && (pLoopPlayer != this) && (pLoopPlayer.getTeam() == getTeam()))
                    {
                        long iQuantity = Math.Min(-(iChange), pLoopPlayer.getMoney());
                        if (iQuantity > 0)
                        {
                            pLoopPlayer.changeMoney(-(iQuantity), false);
                            iChange += iQuantity;
                        }
                    }
                }
            }

            setMoney(getMoney() + iChange);
        }

        public virtual void setDebt(int iNewValue)
        {
            if (getDebt() != iNewValue)
            {
                miDebt = iNewValue;

                makeDirty(PlayerDirtyType.miDebt);
            }
        }
        public virtual void changeDebt(int iChange)
        {
            setDebt(getDebt() + iChange);
        }

        public virtual void setColonyConsumption(ResourceType eResource, int iNewValue)
        {
            if(getColonyConsumption(eResource) != iNewValue)
            {
                maiColonyConsumption[(int)eResource] = iNewValue;
                makeDirty(PlayerDirtyType.maiColonyConsumption);
            }
        }
        public virtual void changeColonyConsumption(ResourceType eResource, int iChange)
        {
            setColonyConsumption(eResource, maiColonyConsumption[(int)eResource] + iChange);
        }

        public virtual void setColonyConsumptionModifier(ResourceType eResource, int iNewValue)
        {
            if(getColonyConsumptionModifier(eResource) != iNewValue)
            {
                maiColonyConsumptionModifier[(int)eResource] = iNewValue;
                makeDirty(PlayerDirtyType.maiColonyConsumptionModifier);
            }
        }

        public virtual void setColonyPayment(ResourceType eResource, int iNewValue)
        {
            if(getColonyPayment(eResource) != iNewValue)
            {
                maiColonyPayments[(int)eResource] = iNewValue;
                makeDirty(PlayerDirtyType.maiColonyPayments);
            }
        }

        public virtual void setExcessBond(int iNewValue)
        {
            if (getExcessBond() != iNewValue)
            {
                miExcessBond = iNewValue;

                makeDirty(PlayerDirtyType.miExcessBond);
            }
        }

        public virtual void setClaims(int iNewValue)
        {
            if (getClaims() != iNewValue)
            {
                miClaims = iNewValue;

                makeDirty(PlayerDirtyType.miClaims);
            }
        }
        public virtual void changeClaims(int iChange)
        {
            setClaims(getClaims() + iChange);
        }

        public virtual IEnumerable<TileServer> getHQTileFootprint(TileClient pTile, HQType eHQ)
        {
            InfoHQ hqInfo = infos().HQ(eHQ);
            return HQClient.getHQFootprint(pTile, gameServer(), hqInfo).Cast<TileServer>();
        }

        protected virtual void setEntertainment(int iNewValue)
        {
            if (getEntertainment() != iNewValue)
            {
                miEntertainment = iNewValue;

                makeDirty(PlayerDirtyType.miEntertainment);
            }
        }
        public virtual void changeEntertainment(int iChange)
        {
            setEntertainment(getEntertainment() + iChange);
        }
        protected virtual void clearEntertainment()
        {
            setEntertainment(0);
        }

        public virtual void updateBaseSharePrice(bool bForce)
        {
            int iNewValue = calculateBaseSharePrice(getDebt(), true);

            if (getBaseSharePrice() != iNewValue)
            {
                if (bForce || (Math.Abs(getBaseSharePrice() - iNewValue) < (Constants.STOCK_MULTIPLIER / 10)))
                {
                    miBaseSharePrice = iNewValue;
                }
                else
                {
                    miBaseSharePrice = ((getBaseSharePrice() * 19) + iNewValue) / 20;
                }

                makeDirty(PlayerDirtyType.miBaseSharePrice);
            }
        }

        protected virtual void setSharesAvailable(int iNewValue)
        {
            if (getSharesAvailable() != iNewValue)
            {
                miSharesAvailable = iNewValue;

                makeDirty(PlayerDirtyType.miSharesAvailable);
            }
        }
        public virtual void changeSharesAvailable(int iChange)
        {
            setSharesAvailable(getSharesAvailable() + iChange);
        }

        protected virtual void changeColonySharesOwned(int iChange)
        {
            if (iChange != 0)
            {
                miColonySharesOwned += iChange;

                makeDirty(PlayerDirtyType.miColonySharesOwned);
            }
        }
        protected virtual void incrementColonyShares()
        {
            changeColonySharesOwned(1);

            if (!isHuman())
            {
                {
                    bool bHasLead = true;

                    foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAll())
                    {
                        if (pLoopPlayer != this)
                        {
                            if (pLoopPlayer.getColonySharesOwned() >= getColonySharesOwned())
                            {
                                bHasLead = false;
                                break;
                            }
                        }
                    }

                    if (bHasLead)
                    {
                        foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAll())
                        {
                            if (pLoopPlayer != this)
                            {
                                if (pLoopPlayer.getColonySharesOwned() == (getColonySharesOwned() - 1))
                                {
                                    incrementNumCampaignLead();
                                }
                            }
                        }
                    }
                }

                {
                    foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAll())
                    {
                        if (pLoopPlayer != this)
                        {
                            if (pLoopPlayer.isHuman())
                            {
                                if (pLoopPlayer.getColonySharesOwned() == getColonySharesOwned())
                                {
                                    incrementNumCampaignTiePlayer();
                                }
                            }
                        }
                    }
                }
            }
        }

        public virtual TileServer startTile()
        {
            return (TileServer)startTileClient();
        }
        public virtual void setStartTileID(int iNewValue)
        {
            if (getStartTileID() != iNewValue)
            {
                miStartTileID = iNewValue;

                makeDirty(PlayerDirtyType.miStartTileID);
            }
        }

        public virtual void setScans(int iNewValue)
        {
            if (getScans() != iNewValue)
            {
                miScans = iNewValue;

                makeDirty(PlayerDirtyType.miScans);
            }
        }
        public virtual void changeScans(int iChange)
        {
            setScans(getScans() + iChange);
        }

        protected virtual void setScanTime(int iNewValue)
        {
            if (getScanTime() != iNewValue)
            {
                miScanTime = iNewValue;

                makeDirty(PlayerDirtyType.miScanTime);
            }
        }
        public virtual void changeScanTime(int iChange)
        {
            setScanTime(getScanTime() + iChange);
        }

        public virtual void changeVisibleTiles(int iChange)
        {
            if (iChange != 0)
            {
                miVisibleTiles += iChange;

                makeDirty(PlayerDirtyType.miVisibleTiles);
            }
        }

        public virtual void setManualSaleTime(int iNewValue)
        {
            if (getManualSaleTime() != iNewValue)
            {
                miManualSaleTime = iNewValue;

                makeDirty(PlayerDirtyType.miManualSaleTime);
            }
        }
        public virtual void changeManualSaleTime(int iChange)
        {
            setManualSaleTime(getManualSaleTime() + iChange);
        }

        public virtual void setBlackMarketTime(int iNewValue)
        {
            if (getBlackMarketTime() != iNewValue)
            {
                miBlackMarketTime = iNewValue;

                if (getBlackMarketTime() == 0)
                {
                    gameServer().gameEventsServer().AddDataEvent(GameEventsClient.DataType.BlackMarketOpen, getPlayer(), null);
                }

                makeDirty(PlayerDirtyType.miBlackMarketTime);
            }
        }
        public virtual void changeBlackMarketTime(int iChange)
        {
            setBlackMarketTime(getBlackMarketTime() + iChange);
        }

        public virtual void incrementSabotagedCount()
        {
            miSabotagedCount++;

            makeDirty(PlayerDirtyType.miSabotagedCount);
        }

        public virtual void changeEspionageShortageCount(int iChange)
        {
            if (iChange != 0)
            {
                miEspionageShortageCount += iChange;

                makeDirty(PlayerDirtyType.miEspionageShortageCount);
            }
        }

        public virtual void changeEspionageSurplusCount(int iChange)
        {
            if (iChange != 0)
            {
                miEspionageSurplusCount += iChange;

                makeDirty(PlayerDirtyType.miEspionageSurplusCount);
            }
        }

        public virtual void changeBondRatingChange(int iChange)
        {
            if (iChange != 0)
            {
                miBondRatingChange += iChange;

                makeDirty(PlayerDirtyType.miBondRatingChange);
            }
        }

        public virtual void changeDebtCut(int iChange)
        {
            if (iChange != 0)
            {
                miDebtCut += iChange;

                makeDirty(PlayerDirtyType.miDebtCut);
            }
        }

        public virtual void setAutoPayDebtTarget(int iNewValue)
        {
            if (getAutoPayDebtTarget() != iNewValue)
            {
                miAutoPayDebtTarget = iNewValue;

                makeDirty(PlayerDirtyType.miAutoPayDebtTarget);
            }
        }

        public virtual void changeEntertainmentModifier(int iChange)
        {
            if (iChange != 0)
            {
                miEntertainmentModifier += iChange;

                makeDirty(PlayerDirtyType.miEntertainmentModifier);
            }
        }

        public virtual void changePowerConsumptionModifier(int iChange)
        {
            if (iChange != 0)
            {
                miPowerConsumptionModifier += iChange;

                makeDirty(PlayerDirtyType.miPowerConsumptionModifier);
            }
        }

        public virtual void changeConnectedHQPowerProductionModifier(int iChange)
        {
            if (iChange != 0)
            {
                miConnectedHQPowerProductionModifier += iChange;

                makeDirty(PlayerDirtyType.miConnectedHQPowerProductionModifier);
            }
        }

        public virtual void changeAdjacentHQSabotageModifier(int iChange)
        {
            if (iChange != 0)
            {
                miAdjacentHQSabotageModifier += iChange;

                makeDirty(PlayerDirtyType.miAdjacentHQSabotageModifier);
            }
        }

        public virtual void changeInterestModifier(int iChange)
        {
            if (iChange != 0)
            {
                miInterestModifier += iChange;

                makeDirty(PlayerDirtyType.miInterestModifier);
            }
        }

        protected virtual void setFinalMarketCap(int iNewValue)
        {
            if (getFinalMarketCap() != iNewValue)
            {
                miFinalMarketCap = iNewValue;

                makeDirty(PlayerDirtyType.miFinalMarketCap);
            }
        }

        protected virtual void setHighestBuyoutTenths(int iNewValue)
        {
            if (getHighestBuyoutTenths() != iNewValue)
            {
                miHighestBuyoutTenths = iNewValue;

                makeDirty(PlayerDirtyType.miHighestBuyoutTenths);
            }
        }

        protected virtual void incrementNumCampaignLead()
        {
            miNumCampaignLead++;

            if (getNumCampaignLeads() <= infos().character(getCharacter()).maeQuipCampaignLead.Count)
            {
                TextType eText = infos().character(getCharacter()).maeQuipCampaignLead[(getNumCampaignLeads() - 1)];

                if (eText != TextType.NONE)
                {
                    AI_quip(PlayerType.NONE, eText);
                }
            }

            makeDirty(PlayerDirtyType.miNumCampaignLead);
        }

        protected virtual void incrementNumCampaignTiePlayer()
        {
            miNumCampaignTiePlayer++;

            if (getNumCampaignTiePlayer() <= infos().character(getCharacter()).maeQuipCampaignTiePlayer.Count)
            {
                TextType eText = infos().character(getCharacter()).maeQuipCampaignTiePlayer[(getNumCampaignTiePlayer() - 1)];

                if (eText != TextType.NONE)
                {
                    AI_quip(PlayerType.NONE, eText);
                }
            }

            makeDirty(PlayerDirtyType.miNumCampaignTiePlayer);
        }

        public virtual void changeScore(int iChange)
        {
            if (iChange != 0)
            {
                miScore += iChange;

                makeDirty(PlayerDirtyType.miScore);
            }
        }

        public virtual void setScore(int score)
        {
            if (score != miScore)
            {
                miScore = score;

                makeDirty(PlayerDirtyType.miScore);
            }
        }

        protected virtual void makeNotHuman()
        {
            if (isHuman())
            {
                mbHuman = false;


                foreach (InfoResource pLoopResource in infos().resources())
                {
                    if (isAutoLaunchResource(pLoopResource.meType))
                    {
                        toggleAutoLaunchResource(pLoopResource.meType);
                    }
                }

                makeDirty(PlayerDirtyType.mbHuman);
            }
        }

        protected virtual void deleteOrderBuildings()
        {
            foreach (TileServer pLoopTile in gameServer().tileServerAll())
            {
                if (pLoopTile.getRealOwner() == getPlayer())
                {
                    if (pLoopTile.isOwnerReal())
                    {
                        BuildingType eBuilding = pLoopTile.getConstructionOrBuildingType();

                        if (eBuilding != BuildingType.NONE)
                        {
                            OrderType eOrder = infos().buildingClass(infos().building(eBuilding).meClass).meOrderType;

                            if ((eOrder != OrderType.NONE) && (eOrder != OrderType.LAUNCH))
                            {
                                if (pLoopTile.isBuilding())
                                {
                                    killBuilding(pLoopTile.buildingServer());
                                }
                                else if (pLoopTile.isConstruction())
                                {
                                    killConstruction(pLoopTile.constructionServer(), true);
                                }
                            }
                        }
                    }
                }
            }

            for(int iLoopUnit = getUnitList().Count - 1; iLoopUnit >= 0; iLoopUnit--)
            {
                UnitServer pLoopUnit = gameServer().unitServer(getUnitList()[iLoopUnit]);
                if (pLoopUnit.getConstructionType() != BuildingType.NONE)
                {
                    OrderType eOrder = infos().buildingClass(infos().building(pLoopUnit.getConstructionType()).meClass).meOrderType;

                    if (eOrder == OrderType.HACK || eOrder == OrderType.PATENT || eOrder == OrderType.RESEARCH)
                        killUnit(pLoopUnit, false, false);
                }
            }
        }

        public virtual void makeConcede()
        {
            if (!isConcede())
            {
                bool bWasWinEligible = isWinEligible();

                mbConcede = true;

                if (bWasWinEligible && !isWinEligible())
                {
                    gameServer().gameEventsServer().AddPlayerWinLose(getPlayer(), false, getFinalMarketCap());
                }

                if (gameServer().isCampaign())
                {
                    gameServer().pickAWinner();
                }

                makeNotHuman();

                if (isDeleteOrders())
                {
                    cancelAllOrders();

                    deleteOrderBuildings();
                }

                makeDirty(PlayerDirtyType.mbConcede);
            }
        }

        public virtual void makeDropped()
        {
            if (!isDropped())
            {
                mbDropped = true;

                makeNotHuman();

                if (isDeleteOrders())
                {
                    cancelAllOrders();

                    deleteOrderBuildings();
                }

                makeDirty(PlayerDirtyType.mbDropped);
            }
        }

        protected virtual void makeSubsidiary(PlayerType eBuyingPlayer, PlayerType eBuyoutPlayer)
        {
            if (!isSubsidiary())
            {
                bool bWasWinEligible = isWinEligible();

                saveEndingMoneyDebt();

                if (gameServer().isTeamGame())
                {
                    for (PatentType eLoopPatent = 0; eLoopPatent < infos().patentsNum(); eLoopPatent++)
                    {
                        if (isPatentAcquiredLab(eLoopPatent))
                        {
                            PlayerServer pBestPlayer = null;
                            int iBestValue = 0;

                            foreach (PlayerServer pLoopPlayer in getAliveTeammatesAll())
                            {
                                if (pLoopPlayer != this)
                                {
                                    if (canSendPatent(eLoopPatent, pLoopPlayer.getPlayer()))
                                    {
                                        int iValue = gameServer().random().Next(1000) + 1;
                                        if (iValue > iBestValue)
                                        {
                                            pBestPlayer = pLoopPlayer;
                                            iBestValue = iValue;
                                        }
                                    }
                                }
                            }

                            if (pBestPlayer != null)
                            {
                                sendPatent(eLoopPatent, pBestPlayer.getPlayer());
                            }
                        }
                    }
                }

                mbSubsidiary = true;

                makeNotHuman();

                foreach (TileServer pLoopTile in gameServer().tileServerAll())
                {
                    if (pLoopTile.getRealOwner() == getPlayer())
                    {
                        pLoopTile.setHologram(false);

                        if (pLoopTile.isOwnerReal())
                        {
                            pLoopTile.setDefendSabotage(SabotageType.NONE);

                            if (pLoopTile.isEmpty())
                            {
                                if (pLoopTile.getLastBuilding() != BuildingType.NONE)
                                {
                                    createBuilding(pLoopTile.getLastBuilding(), pLoopTile, false);
                                }
                            }
                        }
                    }
                }

                if (isDeleteOrders())
                {
                    cancelAllOrders();

                    deleteOrderBuildings();
                }

                setMoney(infos().Globals.SUBSIDIARY_MIN_MONEY);
                setDebt(0);

                setAutoPayDebt(false);

                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    setResourceStockpile(eLoopResource, 0);
                    setHoldResource(eLoopResource, infos().resource(eLoopResource).mbTrade);
                    setAutoLaunchResource(eLoopResource, false);
                    setTeamShareResource(eLoopResource, false);
                }

                for (BuildingType eLoopBuilding = 0; eLoopBuilding < infos().buildingsNum(); eLoopBuilding++)
                {
                    setAutoSupplyBuildingsInput(eLoopBuilding, false);
                }

                List<int> aiStockRefund = Enumerable.Repeat(0, (int)(gameServer().getNumPlayers())).ToList();

                {
                    int iSharesOwned = getSharesOwned(getPlayer());
                    if (iSharesOwned > 0)
                    {
                        changeSharesOwned(getPlayer(), -(iSharesOwned));
                        changeSharesAvailable(iSharesOwned);
                    }
                }

                foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAll())
                {
                    {
                        int iSharesOwned = getSharesOwned(pLoopPlayer.getPlayer());
                        if (iSharesOwned > 0)
                        {
                            foreach (PlayerServer pParentPlayer in gameServer().getPlayerServerAliveAll())
                            {
                                int iParentShares = pParentPlayer.getSharesOwned(getPlayer());
                                if (iParentShares > 0)
                                {
                                    int iTransferShares = ((iSharesOwned * iParentShares) / getSharesBought());
                                    if (iTransferShares > 0)
                                    {
                                        changeSharesOwned(pLoopPlayer.getPlayer(), -(iTransferShares));
                                        if ((pLoopPlayer.getTeam() == pParentPlayer.getTeam()) && !(pLoopPlayer.isSubsidiary()))
                                        {
                                            pLoopPlayer.changeSharesOwned(pLoopPlayer.getPlayer(), iTransferShares);
                                        }
                                        else
                                        {
                                            pParentPlayer.changeSharesOwned(pLoopPlayer.getPlayer(), iTransferShares);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    {
                        int iSharesOwned = getSharesOwned(pLoopPlayer.getPlayer());
                        if (iSharesOwned > 0)
                        {
                            int iSellPrice = (getSellSharePrice(pLoopPlayer.getPlayer()) * iSharesOwned);

                            foreach (PlayerServer pParentPlayer in gameServer().getPlayerServerAliveAll())
                            {
                                int iParentShares = pParentPlayer.getSharesOwned(getPlayer());
                                if (iParentShares > 0)
                                {
                                    int iStockRefund = ((iSellPrice * iParentShares) / getSharesBought());
                                    if (iStockRefund > 0)
                                    {
                                        pParentPlayer.changeMoney(iStockRefund);
                                        aiStockRefund[(int)(pParentPlayer.getPlayer())] += iStockRefund;
                                        gameServer().statsServer().changeStat(StatsType.STOCK, (int)StockStatType.SOLD, pParentPlayer.getPlayer(), (int)getPlayer(), iStockRefund);
                                    }
                                }
                            }

                            changeSharesOwned(pLoopPlayer.getPlayer(), -(iSharesOwned));
                            pLoopPlayer.changeSharesAvailable(iSharesOwned);

                            if (eBuyoutPlayer != PlayerType.NONE)
                            {
                                foreach (PlayerServer pOtherPlayer in gameServer().getPlayerServerAliveAll())
                                {
                                    if (pOtherPlayer.getTeam() != gameServer().playerServer(eBuyoutPlayer).getTeam())
                                    {
                                        pOtherPlayer.increaseStockDelayBy(pLoopPlayer.getPlayer(), (gameServer().getInitialShares() - pOtherPlayer.getSharesOwned(pLoopPlayer.getPlayer())));
                                    }
                                }
                            }
                        }
                    }
                }

                {
                    PlayerType eBestPlayer = PlayerType.NONE;
                    int iBestValue = 0;

                    foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAll())
                    {
                        int iValue = pLoopPlayer.getSharesOwned(getPlayer());

                        if (iValue > 0)
                        {
                            if ((iValue > iBestValue) ||
                                ((iValue == iBestValue) && (eBuyingPlayer == pLoopPlayer.getPlayer())))
                            {
                                eBestPlayer = pLoopPlayer.getPlayer();
                                iBestValue = iValue;
                            }
                        }
                    }

                    setBoughtByPlayer(eBestPlayer);
                }

                updateBaseSharePrice(true);

                gameServer().gameEventsServer().AddSubsidiary(getPlayer(), eBuyoutPlayer, aiStockRefund);

                if (bWasWinEligible && !isWinEligible())
                {
                    gameServer().gameEventsServer().AddPlayerWinLose(getPlayer(), false, getFinalMarketCap());
                }

                if (eBuyoutPlayer != PlayerType.NONE)
                {
                    gameServer().statsServer().addEvent(getPlayer(), StatEventType.BUYOUT_NORMAL, (int)eBuyoutPlayer);
                }
                else
                {
                    gameServer().statsServer().addEvent(getPlayer(), StatEventType.BUYOUT_MAJORITY, (int)eBuyingPlayer);
                }

                if (wasEverHuman() && gameServer().isCampaign())
                {
                    gameServer().makeWinningTeam(gameServer().playerServer(eBuyingPlayer).getTeam());
                }

                makeDirty(PlayerDirtyType.mbSubsidiary);
            }
        }
        public virtual bool testSubsidiary(PlayerType eBuyingPlayer, PlayerType eTakeoverPlayer)
        {
            if ((countOwnSharesOwned() + getSharesAvailable()) < gameServer().getMajorityShares())
            {
                makeSubsidiary(eBuyingPlayer, eTakeoverPlayer);
                return true;
            }

            return false;
        }

        public virtual void makeHQFounded(TileServer pTile)
        {
            if (!isHQFounded())
            {
                mbHQFounded = true;

                incrementHQLevel();

                {
                    int iMoney = infos().handicap(getHandicap()).miMoney;
                    changeMoney(iMoney);
                    gameServer().statsServer().changeStat(StatsType.MISCELLANEOUS, (int)MiscellaneousStatType.STARTING_MONEY, getPlayer(), 0, iMoney);
                }

                {
                    int iClaims = 0;
                    iClaims += infos().location(gameServer().getLocation()).miClaims;
                    iClaims += infos().handicap(getHandicap()).miClaims;
                    iClaims += infos().HQ(getHQ()).miClaims;
                    if (isHuman())
                    {
                        iClaims += infos().rulesSet(gameServer().getRulesSet()).miClaims;
                    }
                    changeClaims(iClaims);
                }

                if (gameServer().isSevenSols())
                {
                    changeColonySharesOwned(infos().HQ(getHQ()).miShares);
                    changeColonySharesOwned(infos().handicap(getHandicap()).miShares);
                }
                else if (!isSubsidiary())
                {
                    int iStartingShares = getStartingShares(true);

                    iStartingShares = Math.Min(iStartingShares, getSharesAvailable());

                    changeSharesOwned(getPlayer(), iStartingShares);
                    changeSharesAvailable(-(iStartingShares));
                }

                foreach (InfoResource pLoopResource in infos().resources())
                {
                    if (gameServer().isResourceValid(pLoopResource.meType))
                    {
                        int iStockpile = infos().HQ(getHQ()).maiInitialStockpile[pLoopResource.miType];

                        iStockpile *= Math.Max(0, (infos().handicap(getHandicap()).miStockpileModifier + 100));
                        iStockpile /= 100;

                        iStockpile += ((infos().HQ(getHQ()).maiLifeSupport[pLoopResource.miType] * infos().handicap(getHandicap()).miStockpileLifeSupport) / Constants.RESOURCE_MULTIPLIER);

                        changeWholeResourceStockpile(pLoopResource.meType, iStockpile, false);
                    }
                }

                for (SabotageType eLoopSabotage = 0; eLoopSabotage < infos().sabotagesNum(); eLoopSabotage++)
                {
                    changeSabotageCount(eLoopSabotage, infos().HQ(getHQ()).maiFoundSabotage[(int)eLoopSabotage]);
                }

                if (isHuman())
                {
                    for (SabotageType eSabotage = 0; eSabotage < infos().sabotagesNum(); eSabotage++)
                    {
                        int iCount = infos().rulesSet(mGame.getRulesSet()).maiFreeSabotages[(int)eSabotage];

                        if (gameServer().isCampaign())
                        {
                            for (ColonyType eLoopColony = 0; eLoopColony < infos().coloniesNum(); eLoopColony++)
                            {
                                ColonyBonusLevelType eColonyBonusLevel = corporation().maeColonyBonusLevel[(int)eLoopColony];

                                if (eColonyBonusLevel != ColonyBonusLevelType.NONE)
                                {
                                    iCount += infos().colonyBonus(infos().colony(eLoopColony).maeColonyBonus[(int)eColonyBonusLevel]).maiSabotageCount[(int)eSabotage];
                                }
                            }
                        }

                        if (iCount > 0)
                        {
                            changeSabotageCount(eSabotage, iCount);
                        }
                    }
                }

                updateCanProduceResource();

                if (!isHuman())
                {
                    AI_changeForceConstruct(getClaims());
                }

                updateBaseSharePrice(true);

                gameServer().doFoundMoney();

                gameServer().statsServer().addEvent(getPlayer(), StatEventType.FOUND, (int)getHQ());

                makeDirty(PlayerDirtyType.mbHQFounded);
            }
        }

        public virtual void setSkipAuction(bool bNewValue)
        {
            if (isSkipAuction() != bNewValue)
            {
                mbSkipAuction = bNewValue;

                makeDirty(PlayerDirtyType.mbSkipAuction);
            }
        }

        protected virtual void setBorehole(bool bNewValue)
        {
            if (isBorehole() != bNewValue)
            {
                mbBorehole = bNewValue;

                makeDirty(PlayerDirtyType.mbBorehole);
            }
        }

        protected virtual void setRecycleScrap(bool bNewValue)
        {
            if (isRecycleScrap() != bNewValue)
            {
                mbRecycleScrap = bNewValue;

                makeDirty(PlayerDirtyType.mbRecycleScrap);
            }
        }

        protected virtual void setAdjacentMining(bool bNewValue)
        {
            if (isAdjacentMining() != bNewValue)
            {
                mbAdjacentMining = bNewValue;

                if (isAdjacentMining())
                {
                    foreach (BuildingClient pLoopBuilding in gameServer().getBuildingList())
                    {
                        if (pLoopBuilding.tileClient().getRealOwner() == getPlayer())
                        {
                            handleBuildingAchievements(pLoopBuilding.getType(), (TileServer)pLoopBuilding.tileClient());
                        }
                    }
                }

                makeDirty(PlayerDirtyType.mbAdjacentMining);
            }
        }

        protected virtual void setTeleportation(bool bNewValue)
        {
            if (isTeleportation() != bNewValue)
            {
                mbTeleportation = bNewValue;

                makeDirty(PlayerDirtyType.mbTeleportation);
            }
        }

        protected virtual void setCaveMining(bool bNewValue)
        {
            if (isCaveMining() != bNewValue)
            {
                mbCaveMining = bNewValue;

                makeDirty(PlayerDirtyType.mbCaveMining);
            }
        }

        public virtual void makeGeothermalDiscovered(TileServer pTile)
        {
            if (!isGeothermalDiscovered())
            {
                mbGeothermalDiscovered = true;

                gameServer().gameEventsServer().AddTileDiscovered(getPlayer(), ResourceType.NONE, ResourceLevelType.NONE, pTile.isGeothermal());

                makeDirty(PlayerDirtyType.mbGeothermalDiscovered);
            }
        }

        protected virtual void setAutoPayDebt(bool bNewValue)
        {
            if (isAutoPayDebt() != bNewValue)
            {
                mbAutoPayDebt = bNewValue;

                if (isAutoPayDebt())
                {
                    setAutoPayDebtTarget(getDebt());
                }
                else
                {
                    setAutoPayDebtTarget(0);
                }

                makeDirty(PlayerDirtyType.mbAutoPayDebt);
            }
        }
        public virtual void toggleAutoPayDebt()
        {
            setAutoPayDebt(!isAutoPayDebt());
        }

        public virtual void makeBeatSoren()
        {
            if (!isBeatSoren())
            {
                mbBeatSoren = true;

                makeDirty(PlayerDirtyType.mbBeatSoren);
            }
        }

        public virtual void makeIsSoren()
        {
            if (!isIsSoren())
            {
                mbIsSoren = true;

                foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAll())
                {
                    if (pLoopPlayer != this)
                    {
                        if (pLoopPlayer.isHuman())
                        {
                            gameServer().gameEventsServer().AddAchievement(pLoopPlayer.getPlayer(), infos().getType<AchievementType>("ACHIEVEMENT_PLAY_SOREN"));
                        }
                    }
                }

                makeDirty(PlayerDirtyType.mbIsSoren);
            }
        }

        public virtual void makeBeatZultar()
        {
            if (!isBeatZultar())
            {
                mbBeatZultar = true;

                makeDirty(PlayerDirtyType.mbBeatZultar);
            }
        }

        public virtual void makeIsZultar()
        {
            if (!isIsZultar())
            {
                mbIsZultar = true;

                foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAll())
                {
                    if (pLoopPlayer != this)
                    {
                        if (pLoopPlayer.isHuman())
                        {
                            gameServer().gameEventsServer().AddAchievement(pLoopPlayer.getPlayer(), infos().getType<AchievementType>("ACHIEVEMENT_PLAY_ZULTAR"));
                        }
                    }
                }

                makeDirty(PlayerDirtyType.mbIsZultar);
            }
        }

        protected virtual void setHQ(HQType eNewValue)
        {
            if (getHQ() != eNewValue)
            {
                meHQ = eNewValue;

                updateUpgradeResourceCost();
                updateBuildingResourceCost();

                updateResourceReplaceValid();

                doGasResource();

                makeDirty(PlayerDirtyType.meHQ);
            }
        }

        protected virtual void setHQLevel(HQLevelType eNewValue)
        {
            HQLevelType eOldValue = getHQLevel();

            if (eOldValue != eNewValue)
            {
                meHQLevel = eNewValue;

                int iDiff = (eNewValue - eOldValue);

                gameServer().changeHQLevels(iDiff);
                gameServer().changeEntertainmentDemand(infos().HQ(getHQ()).miEntertainmentDemand * iDiff);

                if (getHQLevel() > gameServer().getHQHighest())
                {
                    gameServer().setHQHighest(getHQLevel());
                }

                makeDirty(PlayerDirtyType.meHQLevel);
            }
        }
        public virtual void incrementHQLevel()
        {
            setHQLevel(getHQLevel() + 1);

            if (gameServer().isCampaign())
            {
                HQServer pHQ = startingHQServer();
                HQLevelType eHQLevel = getHQLevel();

                for (PerkType eLoopPerk = 0; eLoopPerk < infos().perksNum(); eLoopPerk++)
                {
                    if (infos().perk(eLoopPerk).mbOnLevel)
                    {
                        int iCount = getPerkCount(eLoopPerk);

                        for (int i = 0; i < iCount; i++)
                        {
                            doPerk(eLoopPerk, pHQ.tileServer());
                        }
                    }

                    if (infos().perk(eLoopPerk).meOnHQLevel == eHQLevel)
                    {
                        int iCount = getPerkCount(eLoopPerk);

                        for (int i = 0; i < iCount; i++)
                        {
                            doPerk(eLoopPerk, pHQ.tileServer());
                        }
                    }
                }
            }
        }

        public virtual void setHQClaimBonus(HQLevelType eNewValue)
        {
            if (getHQClaimBonus() != eNewValue)
            {
                meHQClaimBonus = eNewValue;

                makeDirty(PlayerDirtyType.meHQClaimBonus);
            }
        }

        public virtual void setBondRating(BondType eNewValue)
        {
            if (getBondRating() != eNewValue)
            {
                meBondRating = eNewValue;

                makeDirty(PlayerDirtyType.meBondRating);
            }
        }

        public virtual void setEnergyResource(ResourceType eNewValue)
        {
            if (getEnergyResource() != eNewValue)
            {
                meEnergyResource = eNewValue;

                makeDirty(PlayerDirtyType.meEnergyResource);
            }
        }

        public virtual void setGasResource(ResourceType eNewValue)
        {
            if (getGasResource() != eNewValue)
            {
                meGasResource = eNewValue;

                makeDirty(PlayerDirtyType.meGasResource);
            }
        }

        public virtual void setLaunchResource(ResourceType eNewValue)
        {
            if (getLaunchResource() != eNewValue)
            {
                meLaunchResource = eNewValue;

                makeDirty(PlayerDirtyType.meLaunchResource);
            }
        }

        public virtual void setPlayerColor(PlayerColorType eNewValue)
        {
            if (getPlayerColorType() != eNewValue)
            {
                meColor = eNewValue;

                makeDirty(PlayerDirtyType.meColor);
            }
        }

        public virtual void setBoughtByPlayer(PlayerType eNewValue)
        {
            if (getBoughtByPlayer() != eNewValue)
            {
                meBoughtByPlayer = eNewValue;

                makeDirty(PlayerDirtyType.meBoughtByPlayer);
            }
        }

        public virtual void setResourceStockpile(ResourceType eIndex, int iNewValue)
        {
            if (getResourceStockpile(eIndex, false) != iNewValue)
            {
                maiResourceStockpile[(int)eIndex] = iNewValue;
                setShouldCalculateCashResources(true);

                makeDirty(PlayerDirtyType.maiResourceStockpile);
            }
        }
        public virtual void changeResourceStockpile(ResourceType eIndex, int iChange, bool bShared)
        {
            if (iChange != 0)
            {
                if (bShared && gameServer().isTeamGame() && !isSubsidiary() && infos().resource(eIndex).mbTrade && (iChange < 0) && (iChange < -(getResourceStockpile(eIndex, false))))
                {
                    if (getResourceStockpile(eIndex, false) > 0)
                    {
                        iChange += getResourceStockpile(eIndex, false);
                        setResourceStockpile(eIndex, 0);
                    }

                    for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameServer().getNumPlayers(); eLoopPlayer++)
                    {
                        PlayerServer pLoopPlayer = gameServer().playerServer(eLoopPlayer);
                        
                        if (!(pLoopPlayer.isSubsidiary()) && (pLoopPlayer != this) && (pLoopPlayer.getTeam() == getTeam()) && pLoopPlayer.isTeamShareResource(eIndex))
                        {
                            int iQuantity = Math.Min(-(iChange), pLoopPlayer.getResourceStockpile(eIndex, false));
                            if (iQuantity > 0)
                            {
                                pLoopPlayer.changeResourceStockpile(eIndex, -(iQuantity), false);
                                iChange += iQuantity;
                            }
                        }
                    }
                }

                setResourceStockpile(eIndex, getResourceStockpile(eIndex, false) + iChange);
            }
        }
        public virtual void changeWholeResourceStockpile(ResourceType eIndex, int iChange, bool bShared)
        {
            changeResourceStockpile(eIndex, (iChange * Constants.RESOURCE_MULTIPLIER), bShared);
        }

        protected virtual void setResourceExtraStockpile(ResourceType eIndex, int iNewValue)
        {
            if (getResourceExtraStockpile(eIndex) != iNewValue)
            {
                maiResourceExtraStockpile[(int)eIndex] = iNewValue;

                makeDirty(PlayerDirtyType.maiResourceExtraStockpile);
            }
        }
        public virtual void changeResourceExtraStockpile(ResourceType eIndex, int iChange)
        {
            setResourceExtraStockpile(eIndex, getResourceExtraStockpile(eIndex) + iChange);
        }

        public virtual void changeResourceExtraCapacity(ResourceType eIndex, int iChange)
        {
            if (iChange != 0)
            {
                maiResourceExtraCapacity[(int)eIndex] += iChange;

                if (iChange > 0)
                {
                    changeResourceExtraStockpile(eIndex, iChange);
                }

                makeDirty(PlayerDirtyType.maiResourceExtraCapacity);
            }
        }

        protected virtual void setResourceAutoTrade(ResourceType eIndex, int iNewValue)
        {
            if (getResourceAutoTrade(eIndex) != iNewValue)
            {
                maiResourceAutoTrade[(int)eIndex] = iNewValue;

                makeDirty(PlayerDirtyType.maiResourceAutoTrade);
            }
        }
        public virtual void changeResourceAutoTrade(ResourceType eIndex, int iChange)
        {
            setResourceAutoTrade(eIndex, getResourceAutoTrade(eIndex) + iChange);
        }

        protected virtual void setResourceRate(ResourceType eIndex, int iNewValue)
        {
            if (getResourceRate(eIndex) != iNewValue)
            {
                maiResourceRate[(int)eIndex] = iNewValue;

                makeDirty(PlayerDirtyType.maiResourceRate);
            }
        }
        public virtual void changeResourceRate(ResourceType eIndex, int iChange)
        {
            setResourceRate(eIndex, getResourceRate(eIndex) + iChange);
        }
        void clearResourceRate()
        {
            for (ResourceType eResource = 0; eResource < infos().resourcesNum(); eResource++)
            {
                setResourceRate(eResource, 0);
            }
        }

        protected virtual void setResourceShortfall(ResourceType eIndex, int iNewValue)
        {
            if (getResourceShortfall(eIndex) != iNewValue)
            {
                maiResourceShortfall[(int)eIndex] = iNewValue;

                makeDirty(PlayerDirtyType.maiResourceShortfall);
            }
        }
        public virtual void changeResourceShortfall(ResourceType eIndex, int iChange)
        {
            setResourceShortfall(eIndex, getResourceShortfall(eIndex) + iChange);
        }
        protected virtual void clearResourceShortfall()
        {
            for (ResourceType eResource = 0; eResource < infos().resourcesNum(); eResource++)
            {
                setResourceShortfall(eResource, 0);
            }
        }

        protected virtual void setResourceInput(ResourceType eIndex, int iNewValue)
        {
            if (getResourceInput(eIndex) != iNewValue)
            {
                maiResourceInput[(int)eIndex] = iNewValue;

                makeDirty(PlayerDirtyType.maiResourceInput);
            }
        }
        public virtual void changeResourceInput(ResourceType eIndex, int iChange)
        {
            setResourceInput(eIndex, getResourceInput(eIndex) + iChange);
        }
        protected virtual void clearResourceInput()
        {
            for (ResourceType eResource = 0; eResource < infos().resourcesNum(); eResource++)
            {
                setResourceInput(eResource, 0);
            }
        }

        protected virtual void setResourceOutput(ResourceType eIndex, int iNewValue)
        {
            if (getResourceOutput(eIndex) != iNewValue)
            {
                maiResourceOutput[(int)eIndex] = iNewValue;

                makeDirty(PlayerDirtyType.maiResourceOutput);
            }
        }
        public virtual void changeResourceOutput(ResourceType eIndex, int iChange)
        {
            setResourceOutput(eIndex, getResourceOutput(eIndex) + iChange);
        }
        protected virtual void clearResourceOutput()
        {
            for (ResourceType eResource = 0; eResource < infos().resourcesNum(); eResource++)
            {
                setResourceOutput(eResource, 0);
            }
        }

        protected virtual void setResourceAutoPurchased(ResourceType eIndex, int iNewValue)
        {
            if (getResourceAutoPurchased(eIndex) != iNewValue)
            {
                maiResourceAutoPurchased[(int)eIndex] = iNewValue;

                makeDirty(PlayerDirtyType.maiResourceAutoPurchased);
            }
        }
        public virtual void changeResourceAutoPurchased(ResourceType eIndex, int iChange)
        {
            setResourceAutoPurchased(eIndex, getResourceAutoPurchased(eIndex) + iChange);
        }
        protected virtual void clearResourceAutoPurchased()
        {
            for (ResourceType eResource = 0; eResource < infos().resourcesNum(); eResource++)
            {
                setResourceAutoPurchased(eResource, 0);
            }
        }

        protected virtual void setResourceAutoSold(ResourceType eIndex, int iNewValue)
        {
            if (getResourceAutoSold(eIndex) != iNewValue)
            {
                maiResourceAutoSold[(int)eIndex] = iNewValue;

                makeDirty(PlayerDirtyType.maiResourceAutoSold);
            }
        }
        public virtual void changeResourceAutoSold(ResourceType eIndex, int iChange)
        {
            setResourceAutoSold(eIndex, getResourceAutoSold(eIndex) + iChange);
        }
        protected virtual void clearResourceAutoSold()
        {
            for (ResourceType eResource = 0; eResource < infos().resourcesNum(); eResource++)
            {
                setResourceAutoSold(eResource, 0);
            }
        }

        public virtual void changeResourceProductionModifier(ResourceType eIndex, int iChange)
        {
            if (iChange != 0)
            {
                maiResourceProductionModifier[(int)eIndex] += iChange;

                makeDirty(PlayerDirtyType.maiResourceProductionModifier);
            }
        }

        protected virtual void updateUpgradeResourceCost()
        {
            if (getHQ() == HQType.NONE)
            {
                return;
            }

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                if (!gameClient().isResourceValid(eLoopResource))
                    continue;
                int iNewValue = 0;

                if (getResourceReplace(eLoopResource) == ResourceType.NONE)
                {
                    iNewValue += infos().HQ(getHQ()).maiUpgradeResource[(int)eLoopResource];
                }

                for (ResourceType eOtherResource = 0; eOtherResource < infos().resourcesNum(); eOtherResource++)
                {
                    if (getResourceReplace(eOtherResource) == eLoopResource)
                    {
                        iNewValue += infos().HQ(getHQ()).maiUpgradeResource[(int)eOtherResource];
                    }
                }

                iNewValue *= Math.Max(0, (infos().handicap(getHandicap()).miUpgradeModifier + 100));
                iNewValue /= 100;

                if (getUpgradeResourceCost(eLoopResource) != iNewValue)
                {
                    maiUpgradeResourceCost[(int)eLoopResource] = iNewValue;

                    makeDirty(PlayerDirtyType.maiUpgradeResourceCost);
                }
            }
        }

        public virtual void incrementBlackMarketCount(BlackMarketType eIndex)
        {
            maiBlackMarketCount[(int)eIndex]++;

            makeDirty(PlayerDirtyType.maiBlackMarketCount);
        }

        protected virtual void setSabotageCount(SabotageType eIndex, int iNewValue)
        {
            if (getSabotageCount(eIndex) != iNewValue)
            {
                maiSabotageCount[(int)eIndex] = iNewValue;

                makeDirty(PlayerDirtyType.maiSabotageCount);
            }
        }
        public virtual void changeSabotageCount(SabotageType eIndex, int iChange)
        {
            setSabotageCount(eIndex, getSabotageCount(eIndex) + iChange);
        }

        public virtual void changeRealConstructionCount(BuildingType eIndex, int iChange)
        {
            if (iChange != 0)
            {
                maiRealConstructionCount[(int)eIndex] += iChange;

                changeRealBuildingOrderCount(eIndex, iChange);

                if (isOrderBuilding(eIndex))
                {
                    changeRealOrderBuildingCount(iChange);
                }

                makeDirty(PlayerDirtyType.maiRealConstructionCount);
            }
        }

        public virtual void changeBuildingCount(BuildingType eIndex, int iChange)
        {
            if (iChange != 0)
            {
                maiBuildingCount[(int)eIndex] += iChange;

                makeDirty(PlayerDirtyType.maiBuildingCount);
            }
        }

        public virtual void changeRealBuildingCount(BuildingType eIndex, int iChange)
        {
            if (iChange != 0)
            {
                maiRealBuildingCount[(int)eIndex] += iChange;

                changeRealBuildingOrderCount(eIndex, iChange);

                if (isOrderBuilding(eIndex))
                {
                    changeRealOrderBuildingCount(iChange);
                }

                makeDirty(PlayerDirtyType.maiRealBuildingCount);
            }
        }

        public virtual void changeBuildingClassInputModifier(BuildingClassType eIndex, int iChange)
        {
            if (iChange != 0)
            {
                maiBuildingClassInputModifier[(int)eIndex] += iChange;

                makeDirty(PlayerDirtyType.maiBuildingClassInputModifier);
            }
        }

        public virtual void changeBuildingClassLevel(BuildingClassType eIndex, int iChange)
        {
            if (iChange != 0)
            {
                maiBuildingClassLevel[(int)eIndex] += iChange;

                makeDirty(PlayerDirtyType.maiBuildingClassLevel);
            }
        }

        public virtual void changeHQUnlockCount(HQType eIndex, int iChange)
        {
            if (iChange != 0)
            {
                maiHQUnlockCount[(int)eIndex] += iChange;

                makeDirty(PlayerDirtyType.maiHQUnlockCount);
            }
        }

        protected virtual void setOrderCapacity(OrderType eIndex, int iNewValue)
        {
            if (getOrderCapacity(eIndex) != iNewValue)
            {
                maiOrderCapacity[(int)eIndex] = iNewValue;

                makeDirty(PlayerDirtyType.maiOrderCapacity);
            }
        }
        public virtual void changeOrderCapacity(OrderType eIndex, int iChange)
        {
            setOrderCapacity(eIndex, getOrderCapacity(eIndex) + iChange);
        }
        protected virtual void clearOrderCapacity()
        {
            for (OrderType eLoopOrder = 0; eLoopOrder < infos().ordersNum(); eLoopOrder++)
            {
                setOrderCapacity(eLoopOrder, 0);
            }
        }

        public virtual void changeOrderCostModifier(OrderType eIndex, int iChange)
        {
            if (iChange != 0)
            {
                maiOrderCostModifier[(int)eIndex] += iChange;

                makeDirty(PlayerDirtyType.maiOrderCostModifier);
            }
        }

        public virtual void changeOrderRateModifier(OrderType eIndex, int iChange)
        {
            if (iChange != 0)
            {
                maiOrderRateModifier[(int)eIndex] += iChange;

                makeDirty(PlayerDirtyType.maiOrderRateModifier);
            }
        }

        protected virtual void setSharesOwned(PlayerType eIndex, int iNewValue)
        {
            if (getSharesOwned(eIndex) != iNewValue)
            {
                maiSharesOwned[(int)eIndex] = iNewValue;

                makeDirty(PlayerDirtyType.maiSharesOwned);
            }
        }
        public virtual void changeSharesOwned(PlayerType eIndex, int iChange)
        {
            setSharesOwned(eIndex, getSharesOwned(eIndex) + iChange);
        }

        public virtual void increaseStockDelayBy(PlayerType eIndex, int iChange)
        {
            if (gameServer().isGameOption(GameOptionType.STOCK_DELAY) || !isHuman())
            {
                if (iChange > 0)
                {
                    maiStockDelay[(int)eIndex] = Math.Max(getStockDelay(eIndex), (gameServer().getTurnCount() + iChange));

                    makeDirty(PlayerDirtyType.maiStockDelay);
                }
            }
        }

        public virtual void increaseBuyDelayBy(PlayerType eIndex, int iChange)
        {
            if (gameServer().isGameOption(GameOptionType.STOCK_DELAY) || !isHuman())
            {
                if (iChange > 0)
                {
                    maiBuyDelay[(int)eIndex] = Math.Max(getBuyDelay(eIndex), (gameServer().getTurnCount() + iChange));

                    makeDirty(PlayerDirtyType.maiBuyDelay);
                }
            }
        }

        public virtual void increaseSellDelayBy(PlayerType eIndex, int iChange)
        {
            if (gameServer().isGameOption(GameOptionType.STOCK_DELAY) || !isHuman())
            {
                if (iChange > 0)
                {
                    maiSellDelay[(int)eIndex] = Math.Max(getSellDelay(eIndex), (gameServer().getTurnCount() + iChange));

                    makeDirty(PlayerDirtyType.maiSellDelay);
                }
            }
        }

        protected virtual void increaseDividendTotalBy(PlayerType eIndex, int iChange)
        {
            if (iChange > 0)
            {
                maiDividendTotal[(int)eIndex] += iChange;

                makeDirty(PlayerDirtyType.maiDividendTotal);
            }
        }

        public virtual void incrementPerkCount(PerkType eIndex)
        {
            maiPerkCount[(int)eIndex]++;

            addPerk(eIndex);

            makeDirty(PlayerDirtyType.maiPerkCount);
        }

        public virtual void setPlayerOption(PlayerOptionType eIndex, bool bNewValue)
        {
            if (isPlayerOption(eIndex) != bNewValue)
            {
                mabPlayerOptions[(int)eIndex] = bNewValue;

                if (bNewValue)
                {
                    if (!(infos().rulesSet(gameServer().getRulesSet()).mbNoAutoSupply))
                    {
                        if (eIndex == PlayerOptionType.AUTO_SUPPLY_ALL)
                        {
                            foreach (InfoBuilding pLoopBuilding in infos().buildings())
                            {
                                if (gameServer().isBuildingHasInput(pLoopBuilding.meType))
                                {
                                    setAutoSupplyBuildingsInput(pLoopBuilding.meType, true);
                                }
                            }
                        }
                    }

                    if ((eIndex == PlayerOptionType.TEAM_SHARE_ALL) && gameServer().isTeamGame())
                    {
                        foreach (InfoResource pLoopResource in infos().resources())
                        {
                            setTeamShareResource(pLoopResource.meType, true);
                        }
                    }
                }

                makeDirty(PlayerDirtyType.mabPlayerOptions);
            }
        }

        protected virtual void setAutoSupplyBuildingsInput(BuildingType eIndex, bool bNewValue)
        {
            if (isAutoSupplyBuildingsInput(eIndex) != bNewValue)
            {
                mabAutoSupplyBuildingInput[(int)eIndex] = bNewValue;

                makeDirty(PlayerDirtyType.mabAutoSupplyBuildingInput);
            }
        }
        public virtual void toggleAutoSupplyBuildingInput(BuildingType eIndex)
        {
            setAutoSupplyBuildingsInput(eIndex, !isAutoSupplyBuildingsInput(eIndex));
        }

        protected virtual void setBuildingsAlwaysOn(BuildingType eIndex, bool bNewValue)
        {
            if (isBuildingAlwaysOn(eIndex) != bNewValue)
            {
                mabBuildingsAlwaysOn[(int)eIndex] = bNewValue;

                makeDirty(PlayerDirtyType.mabBuildingsAlwaysOn);
            }
        }

        protected virtual void makeBuildingImmune(BuildingType eIndex)
        {
            if (!isBuildingImmune(eIndex))
            {
                mabBuildingImmune[(int)eIndex] = true;

                makeDirty(PlayerDirtyType.mabBuildingImmune);
            }
        }

        protected virtual void makeBuildingDestroyEvent(BuildingType eIndex)
        {
            if (!isBuildingDestroyEvent(eIndex))
            {
                mabBuildingDestroyEvent[(int)eIndex] = true;

                makeDirty(PlayerDirtyType.mabBuildingDestroyEvent);
            }
        }

        protected virtual void setBuildingClassBoost(BuildingClassType eIndex, bool bNewValue)
        {
            if (isBuildingClassBoost(eIndex) != bNewValue)
            {
                mabBuildingClassBoost[(int)eIndex] = bNewValue;

                makeDirty(PlayerDirtyType.mabBuildingClassBoost);
            }
        }

        public virtual void setPatentStarted(PatentType eIndex, bool bNewValue)
        {
            if (isPatentStarted(eIndex) != bNewValue)
            {
                mabPatentStarted[(int)eIndex] = bNewValue;

                makeDirty(PlayerDirtyType.mabPatentStarted);
            }
        }

        public virtual void setPatentAcquiredLab(PatentType eIndex, bool bNewValue, bool bBlock, bool bAnnounce)
        {
            if (isPatentAcquiredLab(eIndex) != bNewValue)
            {
                bool bOldAcquired = isPatentAcquired(eIndex);

                mabPatentAcquiredLab[(int)eIndex] = bNewValue;

                if (bOldAcquired != isPatentAcquired(eIndex))
                {
                    changePatentAcquired(eIndex, bBlock, bAnnounce);
                }

                makeDirty(PlayerDirtyType.mabPatentAcquiredLab);
            }
        }
        public virtual void makePatentAcquired(PatentType eIndex, bool bBlock, bool bAnnounce)
        {
            setPatentAcquiredLab(eIndex, true, bBlock, bAnnounce);
        }
        public virtual void makePatentAcquiredPerk(PatentType eIndex)
        {
            if (!isPatentAcquiredPerk(eIndex))
            {
                bool bOldAcquired = isPatentAcquired(eIndex);

                mabPatentAcquiredPerk[(int)eIndex] = true;

                if (bOldAcquired != isPatentAcquired(eIndex))
                {
                    changePatentAcquired(eIndex, false, false);
                }

                makeDirty(PlayerDirtyType.mabPatentAcquiredPerk);
            }
        }
        public virtual void changePatentAcquired(PatentType eIndex, bool bBlock, bool bAnnounce)
        {
            int iChange = ((isPatentAcquired(eIndex)) ? 1 : -1);

            changeDebtCut(infos().patent(eIndex).miDebtCut * iChange);
            changeEntertainmentModifier(infos().patent(eIndex).miEntertainmentModifier * iChange);
            changePowerConsumptionModifier(infos().patent(eIndex).miPowerConsumptionModifier * iChange);
            changeConnectedHQPowerProductionModifier(infos().patent(eIndex).miConnectedHQPowerProductionModifier * iChange);
            changeAdjacentHQSabotageModifier(infos().patent(eIndex).miAdjacentHQSabotageModifier * iChange);

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                if (gameServer().isResourceValid(eLoopResource))
                {
                    changeResourceExtraCapacity(eLoopResource, (infos().patent(eIndex).maiResourceCapacity[(int)eLoopResource] * Constants.RESOURCE_MULTIPLIER * iChange));
                }
            }

            {
                bool bFound = false;

                for (PatentType eLoopPatent = 0; eLoopPatent < infos().patentsNum(); eLoopPatent++)
                {
                    if (infos().patent(eLoopPatent).mbBorehole)
                    {
                        if (isPatentAcquired(eLoopPatent))
                        {
                            bFound = true;
                            break;
                        }
                    }
                }

                setBorehole(bFound);
            }

            {
                bool bFound = false;

                for (PatentType eLoopPatent = 0; eLoopPatent < infos().patentsNum(); eLoopPatent++)
                {
                    if (infos().patent(eLoopPatent).mbRecycleScrap)
                    {
                        if (isPatentAcquired(eLoopPatent))
                        {
                            bFound = true;
                            break;
                        }
                    }
                }

                setRecycleScrap(bFound);
            }

            {
                bool bFound = false;

                for (PatentType eLoopPatent = 0; eLoopPatent < infos().patentsNum(); eLoopPatent++)
                {
                    if (infos().patent(eLoopPatent).mbAdjacentMining)
                    {
                        if (isPatentAcquired(eLoopPatent))
                        {
                            bFound = true;
                            break;
                        }
                    }
                }

                setAdjacentMining(bFound);
            }

            {
                bool bFound = false;

                for (PatentType eLoopPatent = 0; eLoopPatent < infos().patentsNum(); eLoopPatent++)
                {
                    if (infos().patent(eLoopPatent).mbTeleportation)
                    {
                        if (isPatentAcquired(eLoopPatent))
                        {
                            bFound = true;
                            break;
                        }
                    }
                }

                setTeleportation(bFound);
            }

            {
                bool bFound = false;

                for (PatentType eLoopPatent = 0; eLoopPatent < infos().patentsNum(); eLoopPatent++)
                {
                    if (infos().patent(eLoopPatent).mbCaveMining)
                    {
                        if (isPatentAcquired(eLoopPatent))
                        {
                            bFound = true;
                            break;
                        }
                    }
                }

                setCaveMining(bFound);
            }

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                bool bFound = false;

                for (PatentType eLoopPatent = 0; eLoopPatent < infos().patentsNum(); eLoopPatent++)
                {
                    if (infos().patent(eLoopPatent).meBuildingFreeResource == eLoopResource)
                    {
                        if (isPatentAcquired(eLoopPatent))
                        {
                            bFound = true;
                            break;
                        }
                    }
                }

                setBuildingFreeResource(eLoopResource, bFound);
            }

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                bool bFound = false;

                for (PatentType eLoopPatent = 0; eLoopPatent < infos().patentsNum(); eLoopPatent++)
                {
                    if (infos().patent(eLoopPatent).meAlternateGasResource == eLoopResource)
                    {
                        if (isPatentAcquired(eLoopPatent))
                        {
                            bFound = true;
                            break;
                        }
                    }
                }

                setAlternateGasResource(eLoopResource, bFound);
            }

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                bool bFound = false;

                for (PatentType eLoopPatent = 0; eLoopPatent < infos().patentsNum(); eLoopPatent++)
                {
                    if (infos().patent(eLoopPatent).meAlternatePowerResource == eLoopResource)
                    {
                        if (isPatentAcquired(eLoopPatent))
                        {
                            bFound = true;
                            break;
                        }
                    }
                }

                setAlternatePowerResourcePatent(eLoopResource, bFound);
            }

            for (BuildingType eLoopBuilding = 0; eLoopBuilding < infos().buildingsNum(); eLoopBuilding++)
            {
                bool bFound = false;

                for (PatentType eLoopPatent = 0; eLoopPatent < infos().patentsNum(); eLoopPatent++)
                {
                    if (infos().patent(eLoopPatent).mabBuildingAlwaysOn[(int)eLoopBuilding])
                    {
                        if (isPatentAcquired(eLoopPatent))
                        {
                            bFound = true;
                            break;
                        }
                    }
                }

                setBuildingsAlwaysOn(eLoopBuilding, bFound);
            }

            for (BuildingClassType eLoopBuildingClass = 0; eLoopBuildingClass < infos().buildingClassesNum(); eLoopBuildingClass++)
            {
                bool bFound = false;

                for (PatentType eLoopPatent = 0; eLoopPatent < infos().patentsNum(); eLoopPatent++)
                {
                    if (infos().patent(eLoopPatent).mabBuildingClassBoost[(int)eLoopBuildingClass])
                    {
                        if (isPatentAcquired(eLoopPatent))
                        {
                            bFound = true;
                            break;
                        }
                    }
                }

                setBuildingClassBoost(eLoopBuildingClass, bFound);
            }

            for (BuildingClassType eLoopBuildingClass = 0; eLoopBuildingClass < infos().buildingClassesNum(); eLoopBuildingClass++)
            {
                for (IceType eIgnoreInputIce = 0; eIgnoreInputIce < infos().icesNum(); eIgnoreInputIce++)
                {
                    bool bFound = false;

                    for (PatentType eLoopPatent = 0; eLoopPatent < infos().patentsNum(); eLoopPatent++)
                    {
                        if (infos().patent(eLoopPatent).maeIgnoreInputIce[(int)eLoopBuildingClass] == eIgnoreInputIce)
                        {
                            if (isPatentAcquired(eLoopPatent))
                            {
                                bFound = true;
                                break;
                            }
                        }
                    }

                    setIgnoreInputIce(eLoopBuildingClass, eIgnoreInputIce, bFound);
                }
            }

            updateResourceReplaceValid();

            if (isPatentAcquired(eIndex))
            {
                if (bBlock)
                {
                    PlayerType eOwner = gameServer().getPatentOwner(eIndex);

                    if (eOwner == PlayerType.NONE)
                    {
                        gameServer().makePatentOwner(eIndex, getPlayer());
                    }
                    else
                    {
                        PlayerServer pOwner = gameServer().playerServer(eOwner);

                        if (pOwner.getTeam() != getTeam())
                        {
                            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                            {
                                int iQuantity = getPatentResourceCost(eIndex, eLoopResource, true);
                                if (iQuantity > 0)
                                {
                                    pOwner.changeWholeResourceStockpile(eLoopResource, iQuantity, false);
                                    gameServer().statsServer().changeStat(StatsType.RESOURCE, (int)ResourceStatType.PATENT_CONSUMED, pOwner.getPlayer(), (int)eLoopResource, -(iQuantity * Constants.RESOURCE_MULTIPLIER));

                                    gameServer().gameEventsServer().AddPatentLicense(pOwner.getPlayer(), getPlayer(), eIndex, eLoopResource, iQuantity);
                                }
                            }
                        }
                    }
                }

                if (bAnnounce)
                {
                    List<bool> abPatentStarted = Enumerable.Repeat(false, (int)(gameServer().getNumPlayers())).ToList();

                    if (gameServer().getPatentOwner(eIndex) == getPlayer())
                    {
                        foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAll())
                        {
                            if (pLoopPlayer.isPatentStarted(eIndex))
                            {
                                abPatentStarted[(int)(pLoopPlayer.getPlayer())] = true;
                            }
                        }
                    }

                    gameServer().gameEventsServer().AddPatent(getPlayer(), eIndex, abPatentStarted);

                    gameServer().statsServer().changeStat(StatsType.PATENT, (int)PatentStatType.ACQUIRED, getPlayer(), (int)eIndex, 1);
                    gameServer().statsServer().addEvent(getPlayer(), StatEventType.PATENT, (int)eIndex);

                    if (isHuman())
                    {
                        AchievementType eAchievement = infos().getType<AchievementType>("ACHIEVEMENT_FINANCIAL_INSTRUMENTS_DEBT");

                        if (eIndex == infos().achievement(eAchievement).mePatent)
                        {
                            bool bValid = true;

                            for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameServer().getNumPlayers(); eLoopPlayer++)
                            {
                                PlayerServer pLoopPlayer = gameServer().playerServer(eLoopPlayer);

                                if (!(pLoopPlayer.isSubsidiary()))
                                {
                                    if (pLoopPlayer.getPlayer() != getPlayer())
                                    {
                                        if (pLoopPlayer.getDebt() < getDebt())
                                        {
                                            bValid = false;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (bValid)
                            {
                                gameServer().gameEventsServer().AddAchievement(getPlayer(), eAchievement);
                            }
                        }
                    }

                    if (!isHumanTeam())
                    {
                        TextType eText = infos().character(getCharacter()).maeQuipPatentAcquired[(int)eIndex];

                        if (eText != TextType.NONE)
                        {
                            AI_quip(PlayerType.NONE, eText);
                        }
                    }
                }
            }
        }

        public virtual void setResourceNoBuy(ResourceType eIndex, bool bNewValue)
        {
            if (isResourceNoBuy(eIndex) != bNewValue)
            {
                mabResourceNoBuy[(int)eIndex] = bNewValue;

                makeDirty(PlayerDirtyType.mabResourceNoBuy);
            }
        }

        public virtual void setResourceNoSell(ResourceType eIndex, bool bNewValue)
        {
            if (isResourceNoSell(eIndex) != bNewValue)
            {
                mabResourceNoSell[(int)eIndex] = bNewValue;

                makeDirty(PlayerDirtyType.mabResourceNoSell);
            }
        }


        protected virtual void setBuildingFreeResource(ResourceType eIndex, bool bNewValue)
        {
            if (isBuildingFreeResource(eIndex) != bNewValue)
            {
                mabBuildingFreeResource[(int)eIndex] = bNewValue;

                makeDirty(PlayerDirtyType.mabBuildingFreeResource);
            }
        }

        protected virtual void setAlternateGasResource(ResourceType eIndex, bool bNewValue)
        {
            if (isAlternateGasResource(eIndex) != bNewValue)
            {
                mabAlternateGasResource[(int)eIndex] = bNewValue;

                makeDirty(PlayerDirtyType.mabAlternateGasResource);
            }
        }

        protected virtual void setAlternatePowerResourcePatent(ResourceType eIndex, bool bNewValue)
        {
            if (isAlternatePowerResourcePatent(eIndex) != bNewValue)
            {
                mabAlternatePowerResourcePatent[(int)eIndex] = bNewValue;

                makeDirty(PlayerDirtyType.mabAlternatePowerResourcePatent);
            }
        }

        protected virtual void makeAlternatePowerResourcePerk(ResourceType eIndex)
        {
            if (!isAlternatePowerResourcePerk(eIndex))
            {
                mabAlternatePowerResourcePerk[(int)eIndex] = true;

                makeDirty(PlayerDirtyType.mabAlternatePowerResourcePerk);
            }
        }

        protected virtual void setHoldResource(ResourceType eIndex, bool bNewValue)
        {
            if (infos().rulesSet(mGame.getRulesSet()).mbNoAutoSell)
            {
                if (bNewValue != infos().resource(eIndex).mbTrade)
                {
                    return;
                }
            }

            if (isHoldResource(eIndex) != bNewValue)
            {
                mabHoldResource[(int)eIndex] = bNewValue;

                makeDirty(PlayerDirtyType.mabHoldResource);
            }
        }
        public virtual void toggleHoldResource(ResourceType eIndex)
        {
            setHoldResource(eIndex, !isHoldResource(eIndex));
        }

        protected virtual void setAutoLaunchResource(ResourceType eIndex, bool bNewValue)
        {
            if (isAutoLaunchResource(eIndex) != bNewValue)
            {
                mabAutoLaunchResource[(int)eIndex] = bNewValue;

                makeDirty(PlayerDirtyType.mabAutoLaunchResource);
            }
        }
        public virtual void toggleAutoLaunchResource(ResourceType eIndex)
        {
            setAutoLaunchResource(eIndex, !isAutoLaunchResource(eIndex));
        }

        protected virtual void setTeamShareResource(ResourceType eIndex, bool bNewValue)
        {
            if (isTeamShareResource(eIndex) != bNewValue)
            {
                mabTeamShareResource[(int)eIndex] = bNewValue;

                makeDirty(PlayerDirtyType.mabTeamShareResource);
            }
        }
        public virtual void toggleTeamShareResource(ResourceType eIndex)
        {
            setTeamShareResource(eIndex, !isTeamShareResource(eIndex));
        }
        public virtual void setTeamShareAllResources(bool bNewValue)
        {
            foreach (InfoResource pLoopResource in infos().resources())
            {
                setTeamShareResource(pLoopResource.meType, (bNewValue || mustTeamShareResource(pLoopResource.meType)));
            }
        }

        public virtual void updatePlayerList()
        {
            int iIndex = 0;

            maePlayerList[iIndex] = getPlayer();
            iIndex++;

            foreach (PlayerServer pLoopPlayer in getTeammatesAll())
            {
                if (pLoopPlayer != this)
                {
                    maePlayerList[iIndex] = pLoopPlayer.getPlayer();
                    iIndex++;
                }
            }

            for (TeamType eLoopTeam = 0; eLoopTeam < (TeamType)(gameServer().getNumTeams()); eLoopTeam++)
            {
                if (eLoopTeam != getTeam())
                {
                    foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAll())
                    {
                        if (pLoopPlayer.getTeam() == eLoopTeam)
                        {
                            maePlayerList[iIndex] = pLoopPlayer.getPlayer();
                            iIndex++;
                        }
                    }
                }
            }

            makeDirty(PlayerDirtyType.maePlayerList);
        }

        public virtual void setResourceReplace(ResourceType eIndex, ResourceType eNewValue)
        {
            if (getResourceReplace(eIndex) != eNewValue)
            {
                maeResourceReplace[(int)eIndex] = eNewValue;

                updateUpgradeResourceCost();
                updateBuildingResourceCost();

                makeDirty(PlayerDirtyType.maeResourceReplace);
            }
        }
        public virtual void doResourceReplace()
        {
            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                ResourceType eBestResource = ResourceType.NONE;

                for (ResourceType eOtherResource = 0; eOtherResource < infos().resourcesNum(); eOtherResource++)
                {
                    if (gameServer().isResourceValid(eOtherResource))
                    {
                        if (isResourceReplaceValid(eLoopResource, eOtherResource))
                        {
                            if ((eBestResource == ResourceType.NONE) || (gameServer().marketServer().getPrice(eOtherResource) < gameServer().marketServer().getPrice(eBestResource)))
                            {
                                eBestResource = eOtherResource;
                            }
                        }
                    }
                }

                if (gameServer().isResourceValid(eLoopResource))
                {
                    if ((eBestResource == ResourceType.NONE) || (gameServer().marketServer().getPrice(eLoopResource) < gameServer().marketServer().getPrice(eBestResource)))
                    {
                        eBestResource = eLoopResource;
                    }
                }

                setResourceReplace(eLoopResource, ((eBestResource == eLoopResource) ? ResourceType.NONE : eBestResource));
            }
        }

        public virtual void makeResourceDiscovered(ResourceType eIndex, TileServer pTile)
        {
            if (pTile.getOwner() == PlayerType.NONE)
            {
                if (pTile.getResourceLevel(eIndex, false) > maeResourceLevelDiscovered[(int)eIndex])
                {
                    maeResourceLevelDiscovered[(int)eIndex] = pTile.getResourceLevel(eIndex, false);

                    gameServer().gameEventsServer().AddTileDiscovered(getPlayer(), eIndex, pTile.getResourceLevel(eIndex, false), pTile.isGeothermal());

                    makeDirty(PlayerDirtyType.maeResourceLevelDiscovered);
                }
            }
        }

        public virtual void makeMinimumMining(BuildingClassType eIndex, ResourceLevelType eNewValue)
        {
            if (getMinimumMining(eIndex) < eNewValue)
            {
                maeMinimumMining[(int)eIndex] = eNewValue;

                makeDirty(PlayerDirtyType.maeMinimumMining);
            }
        }

        public virtual void addFreeBuilding(BuildingType eBuilding)
        {
            maiFreeBuildings[(int)eBuilding]++;
            makeDirty(PlayerDirtyType.maiFreeBuildings);
        }

        public virtual void setTechnologyLevelResearching(TechnologyType eIndex, TechnologyLevelType eNewValue)
        {
            if (getTechnologyLevelResearching(eIndex) != eNewValue)
            {
                maeTechnologyLevelResearching[(int)eIndex] = eNewValue;

                makeDirty(PlayerDirtyType.maeTechnologyLevelResearching);
            }
        }

        public virtual void incrementTechnologyLevelDiscovered(TechnologyType eIndex)
        {
            int iOldModifier = infos().technologyLevel(getTechnologyLevelDiscovered(eIndex)).miModifier;

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                if (infos().technology(eIndex).mabResourceProduction[(int)eLoopResource])
                {
                    changeResourceProductionModifier(eLoopResource, -(iOldModifier));
                }
            }

            maeTechnologyLevelDiscovered[(int)eIndex]++;

            int iNewModifier = infos().technologyLevel(getTechnologyLevelDiscovered(eIndex)).miModifier;

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                if (infos().technology(eIndex).mabResourceProduction[(int)eLoopResource])
                {
                    changeResourceProductionModifier(eLoopResource, iNewModifier);
                }
            }

            changeClaims(infos().technologyLevel(getTechnologyLevelDiscovered(eIndex)).maiHQClaims[(int)getHQ()]);

            if (getTechnologyLevelDiscovered(eIndex) == getMaxTechnologyLevel())
            {
                gameServer().gameEventsServer().AddSteamStat(this, "PerfectProduction");
            }

            makeDirty(PlayerDirtyType.maeTechnologyLevelDiscovered);
        }

        protected virtual void updateBuildingResourceCost()
        {
            for (BuildingType eLoopBuilding = 0; eLoopBuilding < infos().buildingsNum(); eLoopBuilding++)
            {
                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    int iNewValue = 0;

                    if (getResourceReplace(eLoopResource) == ResourceType.NONE)
                    {
                        iNewValue += infos().building(eLoopBuilding).maiResourceCost[(int)eLoopResource];
                    }

                    for (ResourceType eOtherResource = 0; eOtherResource < infos().resourcesNum(); eOtherResource++)
                    {
                        if (getResourceReplace(eOtherResource) == eLoopResource)
                        {
                            iNewValue += infos().building(eLoopBuilding).maiResourceCost[(int)eOtherResource];
                        }
                    }

                    if (getHQ() != HQType.NONE)
                    {
                        if (eLoopResource == infos().HQ(getHQ()).meBaseResource)
                        {
                            int iTemp = infos().building(eLoopBuilding).miBaseCost;
                            if (iTemp > 0)
                            {
                                iTemp *= Math.Max(0, (infos().HQ(getHQ()).miBaseResourceModifier + 100));
                                iTemp /= 100;

                                iTemp *= Math.Max(0, (infos().handicap(getHandicap()).miBaseResourceModifier + 100));
                                iTemp /= 100;

                                iNewValue += iTemp;
                            }
                        }
                    }

                    {
                        ColonyClassType eColonyClass = gameClient().getColonyClass();

                        if (eColonyClass != ColonyClassType.NONE)
                        {
                            iNewValue *= Math.Max(0, (infos().colonyClass(eColonyClass).maiBuildingClassCostModifier[(int)(infos().building(eLoopBuilding).meClass)] + 100));
                            iNewValue /= 100;
                        }
                    }

                    if (getBuildingResourceCost(eLoopBuilding, eLoopResource) != iNewValue)
                    {
                        maaiBuildingResourceCost[(int)eLoopBuilding][(int)eLoopResource] = iNewValue;

                        makeDirty(PlayerDirtyType.maaiBuildingResourceCost);
                    }
                }
            }
        }

        protected virtual void setResourceReplaceValid(ResourceType eIndex1, ResourceType eIndex2, bool bNewValue)
        {
            if (isResourceReplaceValid(eIndex1, eIndex2) != bNewValue)
            {
                maabResourceReplaceValid[(int)eIndex1][(int)eIndex2] = bNewValue;

                makeDirty(PlayerDirtyType.maabResourceReplaceValid);
            }
        }
        protected virtual void updateResourceReplaceValid()
        {
            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                for (ResourceType eReplaceResource = 0; eReplaceResource < infos().resourcesNum(); eReplaceResource++)
                {
                    bool bFound = false;

                    if (!bFound)
                    {
                        if (getHQ() != HQType.NONE)
                        {
                            if (infos().HQ(getHQ()).maeResourceReplace[(int)eLoopResource] == eReplaceResource)
                            {
                                bFound = true;
                            }
                        }
                    }

                    if (!bFound)
                    {
                        for (PatentType eLoopPatent = 0; eLoopPatent < infos().patentsNum(); eLoopPatent++)
                        {
                            if (infos().patent(eLoopPatent).maeResourceReplace[(int)eLoopResource] == eReplaceResource)
                            {
                                if (isPatentAcquired(eLoopPatent))
                                {
                                    bFound = true;
                                    break;
                                }
                            }
                        }
                    }

                    setResourceReplaceValid(eLoopResource, eReplaceResource, bFound);
                }
            }
        }

        protected virtual void setIgnoreInputIce(BuildingClassType eIndex1, IceType eIndex2, bool bNewValue)
        {
            if (isIgnoreInputIce(eIndex1, eIndex2) != bNewValue)
            {
                maabIgnoreInputIce[(int)eIndex1][(int)eIndex2] = bNewValue;

                makeDirty(PlayerDirtyType.maabIgnoreInputIce);
            }
        }

        public virtual void addTile(TileServer newValue)
        {
            maTiles.Add(newValue.getID());

            makeDirty(PlayerDirtyType.mTileSet);
        }
        public virtual void removeTile(TileServer oldValue)
        {
            maTiles.Remove(oldValue.getID());

            makeDirty(PlayerDirtyType.mTileSet);
        }

        public virtual HQServer startingHQServer()
        {
            return (HQServer)startingHQClient();
        }
        public virtual HQServer createHQ(TileServer pTile)
        {
            using (new UnityProfileScope("PlayerServer.createHQ"))
            {
                HQServer pHQ = Globals.Factory.createHQServer(gameServer());
                int iID = gameServer().nextHQID();
                addHQ(iID);
                gameServer().addHQ(iID, pHQ);
                pHQ.init(gameServer(), iID, getPlayer(), pTile);

                return pHQ;
            }
        }
        public virtual void addHQ(int iID)
        {
            maHQs.Add(iID);

            makeDirty(PlayerDirtyType.mHQSet);
        }
        public virtual void removeHQ(HQServer pHQ)
        {
            maHQs.Remove(pHQ.getID());

            makeDirty(PlayerDirtyType.mHQSet);
        }

        public virtual ConstructionServer createConstruction(BuildingType eBuilding, TileServer pTile, bool bSpend)
        {
            ConstructionServer pConstruction = Globals.Factory.createConstructionServer(gameServer());
            int iID = gameServer().nextConstructionID();
            addConstruction(iID);
            gameServer().addConstruction(iID, pConstruction);
            pConstruction.init(gameServer(), iID, getPlayer(), eBuilding, pTile, bSpend);

            return pConstruction;
        }
        public virtual void killConstruction(ConstructionServer pConstruction, bool bRefund)
        {
            pConstruction.kill(bRefund);
            removeConstruction(pConstruction);
            gameServer().removeConstruction(pConstruction);
            gameServer().gameEventsServer().AddConstructionKilled(pConstruction.getID());
        }
        public virtual void addConstruction(int iID)
        {
            maConstructions.Add(iID);

            makeDirty(PlayerDirtyType.mConstructionSet);
        }
        public virtual void removeConstruction(ConstructionServer pConstruction)
        {
            maConstructions.Remove(pConstruction.getID());

            makeDirty(PlayerDirtyType.mConstructionSet);
        }

        public virtual BuildingServer createBuilding(BuildingType eBuilding, TileServer pTile, bool bNew, ConstructionServer pConstruction = null)
        {
            BuildingServer building = Globals.Factory.createBuildingServer(gameServer());
            int iID = gameServer().nextBuildingID();
            addBuilding(iID);
            gameServer().addBuilding(iID, building);
            building.init(gameServer(), iID, getPlayer(), eBuilding, pTile, pConstruction);

            if (infos().building(eBuilding).mbConnections)
            {
                updateConnectedBuildings();
            }

            if (bNew)
            {
                BuildingClassType eBuildingClass = infos().building(eBuilding).meClass;

                gameServer().statsServer().changeStat(StatsType.BUILDING_CLASS, (int)BuildingStatType.CONSTRUCTED, pTile.getRealOwner(), (int)eBuildingClass, 1);

                if (infos().buildingClass(eBuildingClass).mbSteamStat)
                {
                    gameServer().gameEventsServer().AddSteamStat(this, infos().buildingClass(eBuildingClass).mzType);
                }

                if (pTile.isRealClaimed())
                {
                    pTile.realOwnerServer().handleBuildingAchievements(eBuilding, pTile);
                }
            }

            return building;
        }
        public virtual void killBuilding(BuildingServer pBuilding)
        {
            pBuilding.kill();
            removeBuilding(pBuilding);
            gameServer().removeBuilding(pBuilding);
            gameServer().gameEventsServer().AddBuildingKilled(pBuilding.getID());

            if (infos().building(pBuilding.getType()).mbConnections)
            {
                updateConnectedBuildings();
            }
        }
        public virtual void addBuilding(int iID)
        {
            maBuildings.Add(iID);

            makeDirty(PlayerDirtyType.mBuildingSet);
        }
        public virtual void removeBuilding(BuildingServer pBuilding)
        {
            maBuildings.Remove(pBuilding.getID());

            makeDirty(PlayerDirtyType.mBuildingSet);
        }
        public void handleBuildingAchievements(BuildingType eBuilding, TileClient pTile)
        {
            if (isHuman())
            {
                if (isAdjacentMining())
                {
                    for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                    {
                        if (Globals.Infos.building(eBuilding).maiResourceMining[(int)eLoopResource] > 0)
                        {
                            ResourceLevelType eResourceLevel = pTile.getResourceLevel(eLoopResource, false);
                            ResourceLevelType eHighResourceLevel = infos().getType<ResourceLevelType>("RESOURCELEVEL_HIGH");

                            if (eResourceLevel < eHighResourceLevel)
                            {
                                for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                                {
                                    TileClient pAdjacentTile = gameServer().tileClientAdjacent(pTile, eLoopDirection);

                                    if (pAdjacentTile != null)
                                    {
                                        if (pAdjacentTile.getRealTeam() != getTeam())
                                        {
                                            if (pAdjacentTile.getResourceLevel(eLoopResource, false) >= eHighResourceLevel)
                                            {
                                                gameServer().gameEventsServer().AddAchievement(getPlayer(), infos().getType<AchievementType>("ACHIEVEMENT_ADJACENT_HIGH_RESOURCE"));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                {
                    if (infos().building(eBuilding).miEntertainment > 0)
                    {
                        bool bValid = true;

                        for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                        {
                            TileClient pAdjacentTile = gameServer().tileClientAdjacent(pTile, eLoopDirection);

                            if (pAdjacentTile != null)
                            {
                                ModuleType eModule = pAdjacentTile.getModule();

                                if (eModule == ModuleType.NONE)
                                {
                                    bValid = false;
                                    break;
                                }
                                else if (infos().module(eModule).miEntertainmentModifier == 0)
                                {
                                    bValid = false;
                                    break;
                                }
                            }
                        }

                        if (bValid)
                        {
                            gameServer().gameEventsServer().AddAchievement(getPlayer(), infos().getType<AchievementType>("ACHIEVEMENT_MAXIMUM_ENTERTAINMENT_BONUS"));
                        }
                    }
                }

                {
                    BuildingClassType eBuildingClass = infos().building(eBuilding).meClass;

                    OrderType eOrder = infos().buildingClass(eBuildingClass).meOrderType;

                    if (eOrder != OrderType.NONE)
                    {
                        bool bValid = true;

                        for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                        {
                            TileClient pAdjacentTile = gameServer().tileClientAdjacent(pTile, eLoopDirection);

                            if (pAdjacentTile != null)
                            {
                                ModuleType eModule = pAdjacentTile.getModule();

                                if (eModule == ModuleType.NONE)
                                {
                                    bValid = false;
                                    break;
                                }
                                else if (infos().module(eModule).maiOrderModifier[(int)eOrder] == 0)
                                {
                                    bValid = false;
                                    break;
                                }
                            }
                        }

                        if (bValid)
                        {
                            gameServer().gameEventsServer().AddAchievement(getPlayer(), infos().getType<AchievementType>("ACHIEVEMENT_MAXIMUM_ORDER_BONUS"));
                        }
                    }
                }

                for (AchievementType eLoopAchievement = 0; eLoopAchievement < infos().achievementsNum(); eLoopAchievement++)
                {
                    if (infos().achievement(eLoopAchievement).miAdjacentCount > 0)
                    {
                        if (infos().achievement(eLoopAchievement).meBuilding == eBuilding)
                        {
                            int iCount = 0;

                            for (DirectionType eLoopDirection = 0; eLoopDirection < (DirectionType)((int)DirectionType.NUM_TYPES * 2); eLoopDirection++)
                            {
                                TileClient pAdjacentTile = gameServer().tileClientAdjacent(pTile, (DirectionType)((int)eLoopDirection % (int)DirectionType.NUM_TYPES));

                                bool bFound = false;

                                if (pAdjacentTile != null)
                                {
                                    if (pAdjacentTile.getRealOwner() == getPlayer())
                                    {
                                        if (pAdjacentTile.getConstructionOrBuildingType() == eBuilding)
                                        {
                                            if ((pAdjacentTile.isConstruction()) ? pAdjacentTile.constructionClient().isDamaged() : true)
                                            {
                                                bFound = true;
                                            }
                                        }
                                    }
                                }

                                if (bFound)
                                {
                                    iCount++;

                                    if (iCount >= infos().achievement(eLoopAchievement).miAdjacentCount)
                                    {
                                        gameServer().gameEventsServer().AddAchievement(getPlayer(), eLoopAchievement);
                                        break;
                                    }
                                }
                                else
                                {
                                    iCount = 0;
                                }
                            }
                        }
                    }
                }
            }
        }

        public virtual UnitServer createUnit(UnitType eUnit, BuildingType eConstructionType, TileServer pTile)
        {
            UnitServer unit = Globals.Factory.createUnitServer(gameServer());
            int iID = gameServer().nextUnitID();
            addUnit(iID);
            gameServer().addUnit(iID, unit);
            unit.init(gameServer(), iID, getPlayer(), eUnit, eConstructionType, pTile);
            gameServer().gameEventsServer().AddUnitCreated(iID, pTile.getID());

            return unit;
        }
        public virtual void killUnit(UnitServer pUnit, bool bRefund, bool bMissionComplete)
        {
            pUnit.kill(bRefund);
            removeUnit(pUnit);
            gameServer().removeUnit(pUnit);
            gameServer().gameEventsServer().AddUnitKilled(pUnit.getID(), pUnit.getType(), pUnit.getMissionInfo().meMission, bMissionComplete);
        }
        protected virtual void addUnit(int iID)
        {
            maUnits.Add(iID);

            makeDirty(PlayerDirtyType.mUnitSet);
        }
        protected virtual void removeUnit(UnitServer pUnit)
        {
            maUnits.Remove(pUnit.getID());

            makeDirty(PlayerDirtyType.mUnitSet);
        }

        protected virtual void addOrder(OrderType eType, int iTime, int iData)
        {
            OrderInfo pOrderInfo = new OrderInfo(eType, 0, iTime, iTime, iData, -1, -1);

            bool bAdd = false;

            switch (pOrderInfo.meType)
            {
                case OrderType.PATENT:
                    {
                        if (canPatent((PatentType)(pOrderInfo.miData1), true))
                        {
                            bAdd = true;
                        }

                        break;
                    }

                case OrderType.RESEARCH:
                    {
                        TechnologyType eTechnology = (TechnologyType)(pOrderInfo.miData1);

                        if (canResearch(eTechnology, getTechnologyLevelResearching(eTechnology) + 1, true))
                        {
                            bAdd = true;
                        }

                        break;
                    }

                case OrderType.HACK:
                    {
                        if (canHack((EspionageType)(pOrderInfo.miData1), true))
                        {
                            bAdd = true;
                        }

                        break;
                    }

                case OrderType.LAUNCH:
                    {
                        if (canLaunch((ResourceType)(pOrderInfo.miData1), true))
                        {
                            bAdd = true;
                        }

                        break;
                    }
            }

            if (bAdd)
            {
                pOrderInfo.miIndex = nextOrderInfosIndex(pOrderInfo.meType); // slewis - make the order have a unique identifier so the UI can grab it.
                getOrderInfos(pOrderInfo.meType).AddLast(pOrderInfo);

                switch (pOrderInfo.meType)
                {
                    case OrderType.PATENT:
                        {
                            PatentType ePatent = (PatentType)(pOrderInfo.miData1);

                            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                            {
                                int iCost = getPatentResourceCost(ePatent, eLoopResource);
                                if (iCost > 0)
                                {
                                    spend(eLoopResource, iCost);
                                    gameServer().statsServer().changeStat(StatsType.RESOURCE, (int)ResourceStatType.PATENT_CONSUMED, getPlayer(), (int)eLoopResource, (iCost * Constants.RESOURCE_MULTIPLIER));
                                }
                            }

                            setPatentStarted(ePatent, true);

                            break;
                        }

                    case OrderType.RESEARCH:
                        {
                            TechnologyType eTechnology = (TechnologyType)(pOrderInfo.miData1);
                            TechnologyLevelType eTechnologyLevel = getTechnologyLevelResearching(eTechnology) + 1;

                            pOrderInfo.miData2 = (int)eTechnologyLevel;
                            setTechnologyLevelResearching(eTechnology, eTechnologyLevel);

                            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                            {
                                int iCost = getResearchResourceCost(eTechnology, eTechnologyLevel, eLoopResource);
                                if (iCost > 0)
                                {
                                    spend(eLoopResource, iCost);
                                    gameServer().statsServer().changeStat(StatsType.RESOURCE, (int)ResourceStatType.RESEARCH_CONSUMED, getPlayer(), (int)eLoopResource, (iCost * Constants.RESOURCE_MULTIPLIER));
                                }
                            }

                            break;
                        }

                    case OrderType.HACK:
                        {
                            EspionageType eEspionage = (EspionageType)(pOrderInfo.miData1);

                            int iCost = getHackCost(eEspionage);
                            changeMoney(-(iCost));

                            foreach (InfoResource pLoopResource in infos().resources())
                            {
                                if (infos().eventGame(infos().espionage(eEspionage).meEventGame).maiResourceChange[pLoopResource.miType] != 0)
                                {
                                    if (infos().espionage(eEspionage).mbSurplus)
                                    {
                                        gameServer().statsServer().changeStat(StatsType.RESOURCE, (int)ResourceStatType.SURPLUSES_SPENT, getPlayer(), pLoopResource.miType, iCost);
                                    }
                                    else
                                    {
                                        gameServer().statsServer().changeStat(StatsType.RESOURCE, (int)ResourceStatType.SHORTAGES_SPENT, getPlayer(), pLoopResource.miType, iCost);
                                    }
                                }
                            }

                            if (infos().espionage(eEspionage).mbSurplus)
                            {
                                changeEspionageSurplusCount(1);
                            }
                            else
                            {
                                changeEspionageShortageCount(1);
                            }
                            
                            break;
                        }

                    case OrderType.LAUNCH:
                        {
                            ResourceType eResource = (ResourceType)(pOrderInfo.miData1);

                            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                            {
                                int iCost = getLaunchResourceCost(eLoopResource, eResource);
                                int iStockpile = getWholeResourceStockpile(eLoopResource, true);

                                if (iCost > iStockpile)
                                {
                                    trade(eLoopResource, (iCost - iStockpile), false);
                                }

                                changeWholeResourceStockpile(eLoopResource, -(iCost), true);
                            }

                            break;
                        }
                }

                makeDirty(PlayerDirtyType.maaOrderInfos);
            }
        }
        public virtual void removeOrderInfo(OrderInfo pOrderInfo, bool bFinish)
        {
            if (bFinish)
            {
                switch (pOrderInfo.meType)
                {
                    case OrderType.PATENT:
                        {
                            PatentType ePatent = (PatentType)(pOrderInfo.miData1);

                            makePatentAcquired(ePatent, true, true);

                            if (isHuman())
                            {
                                BuildingServer pBuilding = gameServer().buildingServer(pOrderInfo.miBuildingID);

                                if (pBuilding != null)
                                {
                                    if (!(pBuilding.tileServer().isOwnerReal()))
                                    {
                                        gameServer().gameEventsServer().AddAchievement(getPlayer(), infos().getType<AchievementType>("ACHIEVEMENT_ACQUIRE_PATENT_MUTINY"));
                                    }
                                }
                            }

                            break;
                        }

                    case OrderType.RESEARCH:
                        {
                            TechnologyType eTech = (TechnologyType)(pOrderInfo.miData1);
                            gameServer().statsServer().changeStat(StatsType.TECHNOLOGY, (int)TechnologyStatType.RESEARCHED, getPlayer(), (int)eTech, 1);
                            incrementTechnologyLevelDiscovered(eTech);

                            break;
                        }

                    case OrderType.HACK:
                        {
                            EspionageType eEspionage = (EspionageType)(pOrderInfo.miData1);
                            EventGameType eEvent = infos().espionage(eEspionage).meEventGame;

                            InfoEventGame eventInfo = infos().eventGame(eEvent);
                            for (ResourceType eResource = 0; eResource < infos().resourcesNum(); eResource++)
                            {
                                if (eventInfo.maiResourceChange[(int)eResource] != 0)
                                {
                                    if (infos().espionage(eEspionage).mbSurplus)
                                    {
                                        gameServer().statsServer().changeStat(StatsType.RESOURCE, (int)ResourceStatType.SURPLUSES_STARTED, getPlayer(), (int)eResource, 1);
                                    }
                                    else
                                    {
                                        gameServer().statsServer().changeStat(StatsType.RESOURCE, (int)ResourceStatType.SHORTAGES_STARTED, getPlayer(), (int)eResource, 1);
                                    }
                                }
                            }

                            gameServer().statsServer().addEvent(getPlayer(), StatEventType.ESPIONAGE, (int)eEspionage);

                            if (infos().espionage(eEspionage).mbSurplus)
                            {
                                gameServer().gameEventsServer().AddSteamStat(this, "TriggeredSurplus");
                            }
                            else
                            {
                                gameServer().gameEventsServer().AddSteamStat(this, "TriggeredShortage");
                            }

                            gameServer().changeEspionageCount(eEspionage, 1);

                            int iPercent = infos().espionage(eEspionage).miPercent;

                            iPercent *= (infos().Globals.ESPIONAGE_DIMINISH + 1);
                            iPercent /= (infos().Globals.ESPIONAGE_DIMINISH + gameServer().getEspionageCount(eEspionage));

                            gameServer().triggerEventGame(eEvent, iPercent, getPlayer());

                            if (!isHuman())
                            {
                                AI_espionageStarted(eEspionage);
                            }

                            if (isHuman())
                            {
                                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                {
                                    if (eventInfo.maiResourceChange[(int)eLoopResource] < 0)
                                    {
                                        bool bValid = true;

                                        if (bValid)
                                        {
                                            if (getResourceRate(eLoopResource) <= 0)
                                            {
                                                bValid = false;
                                            }
                                        }

                                        if (bValid)
                                        {
                                            for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameServer().getNumPlayers(); eLoopPlayer++)
                                            {
                                                PlayerServer pLoopPlayer = gameServer().playerServer(eLoopPlayer);

                                                if (pLoopPlayer != this)
                                                {
                                                    if (pLoopPlayer.getResourceRate(eLoopResource) > 0)
                                                    {
                                                        bValid = false;
                                                        break;
                                                    }
                                                }
                                            }
                                        }

                                        if (bValid)
                                        {
                                            gameServer().gameEventsServer().AddAchievement(getPlayer(), infos().getType<AchievementType>("ACHIEVEMENT_MONOPOLY_SHORTAGE"));
                                            break;
                                        }
                                    }
                                }
                            }

                            break;
                        }

                    case OrderType.LAUNCH:
                        {
                            ResourceType eResource = (ResourceType)(pOrderInfo.miData1);
                            int iLaunchQuantity = infos().resource(eResource).miLaunchQuantity;
                            int iMoneyGained = iLaunchQuantity * gameServer().marketServer().getWholeOffworldPrice(eResource);
                            changeMoney(iMoneyGained);

                            gameServer().statsServer().changeStat(StatsType.RESOURCE, (int)ResourceStatType.OFFWORLD_SOLD, getPlayer(), (int)eResource, iMoneyGained);
                            gameServer().statsServer().changeStat(StatsType.RESOURCE, (int)ResourceStatType.LAUNCHED, getPlayer(), (int)eResource, iLaunchQuantity);
                            gameServer().statsServer().addEvent(getPlayer(), StatEventType.LAUNCH, (int)eResource);

                            break;
                        }
                }

                if (!isHuman())
                {
                    AI_setForceOrder(pOrderInfo.meType, true);
                }

                gameServer().gameEventsServer().AddOrderComplete(getPlayer(), pOrderInfo.meType, pOrderInfo.miData1, pOrderInfo.miData2, pOrderInfo.miBuildingID);
            }
            else
            {
                switch (pOrderInfo.meType)
                {
                    case OrderType.PATENT:
                        {
                            PatentType ePatent = (PatentType)(pOrderInfo.miData1);

                            setPatentStarted(ePatent, false);

                            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                            {
                                int iCost = getPatentResourceCost(ePatent, eLoopResource);
                                if (iCost > 0)
                                {
                                    changeWholeResourceStockpile(eLoopResource, iCost, false);
                                    gameServer().statsServer().changeStat(StatsType.RESOURCE, (int)ResourceStatType.PATENT_CONSUMED, getPlayer(), (int)eLoopResource, -(iCost * Constants.RESOURCE_MULTIPLIER));
                                }
                            }

                            break;
                        }

                    case OrderType.RESEARCH:
                        {
                            TechnologyType eTechnology = (TechnologyType)(pOrderInfo.miData1);

                            setTechnologyLevelResearching(eTechnology, getTechnologyLevelResearching(eTechnology) - 1);

                            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                            {
                                int iCost = getResearchResourceCost(eTechnology, (TechnologyLevelType)(pOrderInfo.miData2), eLoopResource);
                                if (iCost > 0)
                                {
                                    changeWholeResourceStockpile(eLoopResource, iCost, false);
                                    gameServer().statsServer().changeStat(StatsType.RESOURCE, (int)ResourceStatType.RESEARCH_CONSUMED, getPlayer(), (int)eLoopResource, -(iCost * Constants.RESOURCE_MULTIPLIER));
                                }
                            }

                            break;
                        }

                    case OrderType.HACK:
                        {
                            EspionageType eEspionage = (EspionageType)(pOrderInfo.miData1);

                            if (infos().espionage(eEspionage).mbSurplus)
                            {
                                changeEspionageSurplusCount(-1);
                            }
                            else
                            {
                                changeEspionageShortageCount(-1);
                            }
                            
                            int iCost = getHackCost(eEspionage);
                            changeMoney(iCost);

                            foreach (InfoResource pLoopResource in infos().resources())
                            {
                                if (infos().eventGame(infos().espionage(eEspionage).meEventGame).maiResourceChange[pLoopResource.miType] != 0)
                                {
                                    if (infos().espionage(eEspionage).mbSurplus)
                                    {
                                        gameServer().statsServer().changeStat(StatsType.RESOURCE, (int)ResourceStatType.SURPLUSES_SPENT, getPlayer(), pLoopResource.miType, -(iCost));
                                    }
                                    else
                                    {
                                        gameServer().statsServer().changeStat(StatsType.RESOURCE, (int)ResourceStatType.SHORTAGES_SPENT, getPlayer(), pLoopResource.miType, -(iCost));
                                    }
                                }
                            }

                            break;
                        }

                    case OrderType.LAUNCH:
                        {
                            ResourceType eResource = (ResourceType)(pOrderInfo.miData1);

                            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                            {
                                changeWholeResourceStockpile(eLoopResource, getLaunchResourceCost(eLoopResource, eResource), false);
                            }

                            if (isAutoLaunchResource(eResource))
                            {
                                toggleAutoLaunchResource(eResource);
                            }

                            break;
                        }
                }
            }

            {
                BuildingServer pBuilding = gameServer().buildingServer(pOrderInfo.miBuildingID);

                if (pBuilding != null)
                {
                    pBuilding.setWorked(false);
                }
            }

            getOrderInfos(pOrderInfo.meType).Remove(pOrderInfo);

            makeDirty(PlayerDirtyType.maaOrderInfos);
        }
        protected virtual void cancelTechOrder(TechnologyType eTech)
        {
            foreach (OrderInfo pOrderInfo in getOrderInfos(OrderType.RESEARCH))
            {
                if ((TechnologyType)(pOrderInfo.miData1) == eTech)
                {
                    if ((TechnologyLevelType)(pOrderInfo.miData2) == getTechnologyLevelResearching(eTech))
                    {
                        removeOrderInfo(pOrderInfo, false);
                        return;
                    }
                }
            }
        }
        public virtual void cancelOrder(OrderType eOrder, int iIndex)
        {
            if (!canCancelOrder())
            {
                return;
            }

            foreach (OrderInfo pOrderInfo in getOrderInfos(eOrder))
            {
                if (pOrderInfo.miIndex == iIndex)
                {
                    if (pOrderInfo.meType == OrderType.RESEARCH)
                    {
                        cancelTechOrder((TechnologyType)(pOrderInfo.miData1));
                    }
                    else
                    {
                        removeOrderInfo(pOrderInfo, false);
                    }

                    return;
                }
            }
        }
        public virtual void cancelAllOrders()
        {
            foreach (InfoOrder pLoopOrder in infos().orders())
            {
                LinkedList<OrderInfo> oldOrderInfos = new LinkedList<OrderInfo>();

                foreach (OrderInfo pLoopOrderInfo in getOrderInfos(pLoopOrder.meType))
                {
                    oldOrderInfos.AddLast(pLoopOrderInfo);
                }

                foreach (OrderInfo pLoopOrderInfo in oldOrderInfos)
                {
                    removeOrderInfo(pLoopOrderInfo, false);
                }
            }
        }
        protected virtual void doTurnOrders(OrderType eOrder)
        {
            {
                LinkedList<OrderInfo> oldOrderInfos = new LinkedList<OrderInfo>();

                foreach (OrderInfo pLoopOrderInfo in getOrderInfos(eOrder))
                {
                    oldOrderInfos.AddLast(pLoopOrderInfo);
                }

                foreach (OrderInfo pLoopOrderInfo in oldOrderInfos)
                {
                    bool bRemove = false;

                    switch (pLoopOrderInfo.meType)
                    {
                        case OrderType.PATENT:
                            {
                                if (getHQ() == HQType.NONE || !(infos().HQ(getHQ()).mbLicensePatents))
                                {
                                    if (gameServer().isPatentOwned((PatentType)(pLoopOrderInfo.miData1)))
                                    {
                                        bRemove = true;
                                    }
                                }
                                break;
                            }

                        case OrderType.RESEARCH:
                            {
                                if (getTechnologyLevelDiscovered((TechnologyType)(pLoopOrderInfo.miData1)) == getMaxTechnologyLevel())
                                {
                                    bRemove = true;
                                }
                                break;
                            }
                    }

                    if (bRemove)
                    {
                        removeOrderInfo(pLoopOrderInfo, false);
                    }
                }
            }

            HashSet<ResourceType> seLauncedResources = new HashSet<ResourceType>();

            {
                LinkedList<OrderInfo> oldOrderInfos = new LinkedList<OrderInfo>();

                foreach (OrderInfo pLoopOrderInfo in getOrderInfos(eOrder))
                {
                    oldOrderInfos.AddLast(pLoopOrderInfo);
                }

                foreach (OrderInfo pLoopOrderInfo in oldOrderInfos)
                {
                    if (pLoopOrderInfo.miBuildingID != -1)
                    {
                        pLoopOrderInfo.miTime = Math.Max(0, (pLoopOrderInfo.miTime - getOrderRate(eOrder, gameServer().buildingServer(pLoopOrderInfo.miBuildingID).tileServer())));

                        if (pLoopOrderInfo.miTime == 0)
                        {
                            removeOrderInfo(pLoopOrderInfo, true);

                            if (pLoopOrderInfo.meType == OrderType.LAUNCH)
                            {
                                seLauncedResources.Add((ResourceType)(pLoopOrderInfo.miData1));
                            }
                        }

                        makeDirty(PlayerDirtyType.maaOrderInfos);
                    }
                }
            }

            if (!isHuman() && gameServer().isLastDay())
            {
                LinkedList<OrderInfo> oldOrderInfos = new LinkedList<OrderInfo>();

                foreach (OrderInfo pLoopOrderInfo in getOrderInfos(eOrder))
                {
                    oldOrderInfos.AddLast(pLoopOrderInfo);
                }

                foreach (OrderInfo pLoopOrderInfo in oldOrderInfos)
                {
                    if (pLoopOrderInfo.miBuildingID != -1)
                    {
                        if (gameServer().getTurnsLeft() < (pLoopOrderInfo.miTime / getOrderRate(eOrder, gameServer().buildingServer(pLoopOrderInfo.miBuildingID).tileServer())))
                        {
                            removeOrderInfo(pLoopOrderInfo, false);
                        }
                    }
                    else
                    {
                        if (gameServer().getTurnsLeft() < (pLoopOrderInfo.miTime / getOrderRate(eOrder, null)))
                        {
                            removeOrderInfo(pLoopOrderInfo, false);
                        }
                    }
                }
            }

            {
                int iCount = Math.Max(0, (getOrderCapacity(OrderType.LAUNCH) - countOrders(OrderType.LAUNCH)));

                for (int iI = 0; iI < iCount; iI++)
                {
                    ResourceType eBestResource = ResourceType.NONE;
                    int iBestValue = 0;

                    foreach (InfoResource pLoopResource in infos().resources())
                    {
                        if (isAutoLaunchResource(pLoopResource.meType))
                        {
                            if (canLaunch(pLoopResource.meType, true))
                            {
                                int iValue = 1000;

                                iValue += gameServer().random().Next(200);

                                if (seLauncedResources.Contains(pLoopResource.meType))
                                {
                                    iValue *= 2;
                                    iValue /= 3;
                                }

                                iValue /= (getOrderIndexCount(OrderType.LAUNCH, pLoopResource.miType) + 1);

                                if (getLaunchProfit(pLoopResource.meType) < 0)
                                {
                                    iValue /= 4;
                                }

                                if (iValue > iBestValue)
                                {
                                    eBestResource = pLoopResource.meType;
                                    iBestValue = iValue;
                                }
                            }
                        }
                    }

                    if (eBestResource != ResourceType.NONE)
                    {
                        launch(eBestResource);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            foreach (int iBuilding in getBuildingList())
            {
                BuildingServer pLoopBuilding = gameServer().buildingServer(iBuilding);

                if (!(pLoopBuilding.isStopped()))
                {
                    OrderType eOrderType = infos().buildingClass(pLoopBuilding.getClass()).meOrderType;

                    if (eOrderType != OrderType.NONE)
                    {
                        if ((eOrderType == OrderType.HACK) && getHQ() != HQType.NONE && infos().HQ(getHQ()).mbDoubleHack)
                        {
                            updateOrderBuilding(eOrderType, pLoopBuilding.getID(), true, false);
                            updateOrderBuilding(eOrderType, pLoopBuilding.getID(), false, true);
                        }
                        else
                        {
                            updateOrderBuilding(eOrderType, pLoopBuilding.getID());
                        }
                    }
                }
            }
        }
        protected virtual void clearOrderBuildings()
        {
            for (OrderType eLoopOrder = 0; eLoopOrder < infos().ordersNum(); eLoopOrder++)
            {
                bool bClear = false;

                foreach (OrderInfo pLoopOrderInfo in getOrderInfos(eLoopOrder))
                {
                    if (pLoopOrderInfo.miBuildingID != -1)
                    {
                        BuildingServer pBuilding = gameServer().buildingServer(pLoopOrderInfo.miBuildingID);

                        if ((pBuilding == null) || pBuilding.isStopped() || (pBuilding.getOwner() != getPlayer()))
                        {
                            bClear = true;
                            break;
                        }
                    }
                }

                if (bClear)
                {
                    foreach (OrderInfo pLoopOrderInfo in getOrderInfos(eLoopOrder))
                    {
                        pLoopOrderInfo.miBuildingID = -1;
                    }

                    makeDirty(PlayerDirtyType.maaOrderInfos);
                }
            }

            {
                HashSet<EspionageType> seEspionageOrder = null;

                foreach (OrderInfo pLoopOrderInfo in getOrderInfos(OrderType.HACK))
                {
                    if (pLoopOrderInfo.miBuildingID != -1)
                    {
                        EspionageType eEspionage = (EspionageType)(pLoopOrderInfo.miData1);

                        if (seEspionageOrder == null)
                        {
                            seEspionageOrder = new HashSet<EspionageType>();
                        }

                        if (seEspionageOrder.Contains(eEspionage))
                        {
                            pLoopOrderInfo.miBuildingID = -1;

                            makeDirty(PlayerDirtyType.maaOrderInfos);
                        }
                        else
                        {
                            seEspionageOrder.Add(eEspionage);
                        }
                    }
                }
            }
        }
        public virtual bool updateOrderBuilding(OrderType eOrder, int iBuildingID, bool bSurplusOnly = false, bool bShortageOnly = false)
        {
            foreach (OrderInfo pLoopOrderInfo in getOrderInfos(eOrder))
            {
                if (pLoopOrderInfo.miBuildingID == iBuildingID)
                {
                    if (pLoopOrderInfo.meType == OrderType.HACK)
                    {
                        EspionageType eEspionage = (EspionageType)(pLoopOrderInfo.miData1);

                        if (bSurplusOnly)
                        {
                            if (!(infos().espionage(eEspionage).mbSurplus))
                            {
                                continue;
                            }
                        }

                        if (bShortageOnly)
                        {
                            if (infos().espionage(eEspionage).mbSurplus)
                            {
                                continue;
                            }
                        }
                    }

                    return true;
                }
            }

            HashSet<EspionageType> seEspionageOrder = null;

            foreach (OrderInfo pLoopOrderInfo in getOrderInfos(OrderType.HACK))
            {
                if (pLoopOrderInfo.miBuildingID != -1)
                {
                    if (seEspionageOrder == null)
                    {
                        seEspionageOrder = new HashSet<EspionageType>();
                    }

                    seEspionageOrder.Add((EspionageType)(pLoopOrderInfo.miData1));
                }
            }

            foreach (OrderInfo pLoopOrderInfo in getOrderInfos(eOrder))
            {
                if (pLoopOrderInfo.miBuildingID == -1)
                {
                    if (pLoopOrderInfo.meType == OrderType.HACK)
                    {
                        EspionageType eEspionage = (EspionageType)(pLoopOrderInfo.miData1);

                        if (seEspionageOrder != null)
                        {
                            if (seEspionageOrder.Contains(eEspionage))
                            {
                                continue;
                            }
                        }

                        if (bSurplusOnly)
                        {
                            if (!(infos().espionage(eEspionage).mbSurplus))
                            {
                                continue;
                            }
                        }

                        if (bShortageOnly)
                        {
                            if (infos().espionage(eEspionage).mbSurplus)
                            {
                                continue;
                            }
                        }
                    }

                    pLoopOrderInfo.miBuildingID = iBuildingID;

                    makeDirty(PlayerDirtyType.maaOrderInfos);

                    return true;
                }
            }

            return false;
        }

        protected virtual int nextMessageID()
        {
            return miNextMessageID++;
        }

        public virtual bool isOrderBuilding(BuildingType eBuilding)
        {
            return (infos().buildingClass(infos().building(eBuilding).meClass).meOrderType != OrderType.NONE);
        }
        public virtual int getRealOrderBuildingCount()
        {
            return miRealOrderBuildingCount;
        }
        public virtual void changeRealOrderBuildingCount(int iChange)
        {
            miRealOrderBuildingCount += iChange;
        }

        public virtual int AI_getForceConstruct()
        {
            return miAIForceConstruct;
        }
        protected virtual void AI_changeForceConstruct(int iChange)
        {
            miAIForceConstruct += iChange;
        }

        protected virtual int AI_getAILastQuip()
        {
            return miAILastQuip;
        }
        protected virtual void AI_setAILastQuip(int iNewValue)
        {
            miAILastQuip = iNewValue;
        }

        public virtual bool AI_isUpdated()
        {
            return mbAIUpdated;
        }
        public virtual void AI_setUpdated(bool bNewValue)
        {
            mbAIUpdated = bNewValue;
        }

        public virtual int getRealBuildingOrderCount(OrderType eIndex)
        {
            return maiRealBuildingOrderCount[(int)eIndex];
        }
        public virtual void changeRealBuildingOrderCount(BuildingType eIndex, int iChange)
        {
            if (iChange != 0)
            {
                OrderType eOrder = infos().buildingClass(infos().building(eIndex).meClass).meOrderType;

                if (eOrder != OrderType.NONE)
                {
                    maiRealBuildingOrderCount[(int)eOrder] += iChange;
                }
            }
        }

        public virtual int nextOrderInfosIndex(OrderType eIndex)
        {
            return maiOrderInfosIndex[(int)eIndex]++;
        }

        public virtual bool isCanProduceResource(ResourceType eIndex)
        {
            return mabCanProduceResource[(int)eIndex];
        }
        protected virtual void updateCanProduceResource()
        {
            if (getHQ() != HQType.NONE)
            {
                foreach (InfoResource pLoopResource in infos().resources())
                {
                    if (gameServer().isResourceValid(pLoopResource.meType))
                    {
                        bool bFound = false;

                        foreach (InfoBuilding pLoopBuilding in infos().buildings())
                        {
                            if (canEverConstruct(pLoopBuilding.meType, true, true))
                            {
                                if (Utils.isBuildingYieldAny(pLoopBuilding.meType, gameServer()))
                                {
                                    bFound = true;
                                    break;
                                }
                            }
                        }

                        mabCanProduceResource[pLoopResource.miType] = bFound;
                    }
                }
            }
        }

        public virtual bool AI_isForceOrder(OrderType eIndex)
        {
            return mabAIForceOrder[(int)eIndex];
        }
        public virtual void AI_setForceOrder(OrderType eIndex, bool bNewValue)
        {
            mabAIForceOrder[(int)eIndex] = bNewValue;
        }
    }
}