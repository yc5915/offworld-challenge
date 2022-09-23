using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Offworld.SystemCore;
using UnityEngine.Assertions;

namespace Offworld.GameCore
{
    [Serializable]
    public class Corporation
    {
        public string mzName = "";
        public CorporationType meID = CorporationType.NONE;
        public ExecutiveType meExecutive = ExecutiveType.NONE;
        public PersonalityType mePersonality = PersonalityType.NONE;
        public PlayerColorType mePlayerColor = PlayerColorType.NONE;
        public int miRank = -1;
        public int miHQs = 0;
        public int miMoney = 0;
        public int miDebt = 0;
        public int miStructures = 0;
        public int miModules = 0;
        public int miNumLevels = 0;
        public bool mbDead = false;
        public LevelType meCurrentLevel = LevelType.NONE;
        public List<int> maiTempPerkCount = new List<int>();
        public List<int> maiPermPerkCount = new List<int>();
        public List<int> maiLevelPerkCount = new List<int>();
        public List<int> maiLevelPerkTime = new List<int>();
        public List<bool> mabEventTurnSeen = new List<bool>();
        public List<ColonyBonusLevelType> maeColonyBonusLevel = new List<ColonyBonusLevelType>();
        public Dictionary<LevelType, int> mdiLevelColonyShares = new Dictionary<LevelType, int>();
        
        public int getPerkCount(PerkType ePerk)
        {
            return (maiTempPerkCount[(int)ePerk] + maiPermPerkCount[(int)ePerk] + maiLevelPerkCount[(int)ePerk]);
        }
    }

    public class GameEventSabotage
    {
        public SabotageType meSabotage;
        public int miTileID;
        public PlayerType mePerpetrator;
        public PlayerType meOriginalOwner;
        public List<int> mAffectedTiles = new List<int>();
        public List<int> mAffectedUnits = new List<int>();
        public List<int> mFrozenTimes = new List<int>();
        public HashSet<PlayerType> mTargetedPlayers = new HashSet<PlayerType>();
        
        public GameEventSabotage()
        {
        }
        
        public GameEventSabotage(SabotageType eSabotage, int iTileID, PlayerType perpetrator, PlayerType originalOwner, List<int> affectedTiles, List<int> affectedUnits, List<int> frozenTimes, HashSet<PlayerType> targetedPlayers)
        {
            meSabotage = eSabotage;
            miTileID = iTileID;
            mePerpetrator = perpetrator;
            meOriginalOwner = originalOwner;
            mAffectedTiles.AddRange(affectedTiles);
            mAffectedUnits.AddRange(affectedUnits);
            mFrozenTimes.AddRange(frozenTimes);
            mTargetedPlayers = new HashSet<PlayerType>(targetedPlayers);
        }
        
        public void Serialize(object stream)
        {
            SimplifyIO.Data(stream, ref meSabotage, "Sabotage");
            SimplifyIO.Data(stream, ref miTileID, "TileID");
            SimplifyIO.Data(stream, ref mePerpetrator, "Perpetrator");
            SimplifyIO.Data(stream, ref meOriginalOwner, "Original owner");
            SimplifyIO.Data(stream, ref mAffectedTiles, "AffectedTiles");
            SimplifyIO.Data(stream, ref mAffectedUnits, "AffectedUnits");
            SimplifyIO.Data(stream, ref mFrozenTimes, "FrozenTimes");
            SimplifyIO.Data(stream, ref mTargetedPlayers, "TargetedPlayers");
        }
    }

    public class GameEventSabotageReversal
    {
        public SabotageType meSabotageReversal; // what sabotage caused the reversal
        public SabotageType meSabotageAttempted;
        public int miTileID;
        public PlayerType meAttacker;
        public PlayerType meDefender;

        public GameEventSabotageReversal()
        {
        }

        public GameEventSabotageReversal(SabotageType eSabotageReversal, SabotageType eSabotageAttempted, int iTileID, PlayerType eAttackingPlayer, PlayerType eDefendingPlayer)
        {
            meSabotageReversal = eSabotageReversal;
            meSabotageAttempted = eSabotageAttempted;
            miTileID = iTileID;
            meAttacker = eAttackingPlayer;
            meDefender = eDefendingPlayer;
        }

        public void Serialize(object stream)
        {
            SimplifyIO.Data(stream, ref meSabotageReversal, "SabotageReversal");
            SimplifyIO.Data(stream, ref meSabotageAttempted, "SabotageAttempted");
            SimplifyIO.Data(stream, ref miTileID, "TileID");
            SimplifyIO.Data(stream, ref meAttacker, "AttackingPlayer");
            SimplifyIO.Data(stream, ref meDefender, "DefendingPlayer");
        }
    }

    public class GameEventMessage
    {
        public PlayerType meFromPlayer;
        public PlayerType meToPlayer;
        public TextType meMessage;

        public GameEventMessage()
        {
        }

        public GameEventMessage(PlayerType eFromPlayer, PlayerType eToPlayer, TextType eMessage)
        {
            meFromPlayer = eFromPlayer;
            meToPlayer = eToPlayer;
            meMessage = eMessage;
        }

        public void Serialize(object stream)
        {
            SimplifyIO.Data(stream, ref meFromPlayer, "FromPlayer");
            SimplifyIO.Data(stream, ref meToPlayer, "ToPlayer");
            SimplifyIO.Data(stream, ref meMessage, "Message");
        }
    }

    public class GameEventSteamStat
    {
        public PlayerType mePlayer;
        public string mzStat;

        public GameEventSteamStat()
        {
        }

        public GameEventSteamStat(PlayerType ePlayer, string zStat)
        {
            mePlayer = ePlayer;
            mzStat = zStat;
        }

        public void Serialize(object stream)
        {
            SimplifyIO.Data(stream, ref mePlayer, "Player");
            SimplifyIO.Data(stream, ref mzStat, "Stat");
        }
    }

    public class GameEventData
    {
        public GameEventsClient.DataType meType;
        public PlayerType mePlayer;
        public List<int> maiData = new List<int>();

        public GameEventData()
        {
        }

        public GameEventData(GameEventsClient.DataType eType, PlayerType ePlayer, List<int> aiData)
        {
            meType = eType;
            mePlayer = ePlayer;
            if(aiData != null)
                maiData.AddRange(aiData);
        }

        public void Serialize(object stream)
        {
            SimplifyIO.Data(stream, ref meType, "Type");
            SimplifyIO.Data(stream, ref mePlayer, "Player");
            SimplifyIO.Data(stream, ref maiData, "Data");
        }
    }

    public class ArtPackList
    {
        private List<ArtPackType> activeArtPacks = new List<ArtPackType>();
        private bool artPackHiddenIdentities = false; //whether to show art-packs when HiddenIdentities game

        public List<ArtPackType> ActiveArtPacks     { get { return activeArtPacks;          } }
        public bool ArtPackHiddenIdentities         { get { return artPackHiddenIdentities; } }

        public ArtPackList()
        {
        }

        public ArtPackList(ArtPackList source)
        {
            if(source != null)
                Set(source.activeArtPacks, source.artPackHiddenIdentities);
        }

        public ArtPackList(List<ArtPackType> activeArtPacks, bool artPackHiddenIdentities)
        {
            Set(activeArtPacks, artPackHiddenIdentities);
        }

        public void Set(List<ArtPackType> activeArtPacks, bool artPackHiddenIdentities)
        {
            this.activeArtPacks = (activeArtPacks != null) ? activeArtPacks : new List<ArtPackType>(); //make list copy
            this.artPackHiddenIdentities = artPackHiddenIdentities;
        }

        public bool SequenceEqual(ArtPackList rhs)
        {
            return activeArtPacks.SequenceEqual(rhs.activeArtPacks) && (artPackHiddenIdentities == rhs.artPackHiddenIdentities);
        }

        public static void Serialize(object stream, ref ArtPackList value)
        {
            if(SimplifyIO.IsReading(stream) && (value == null))
                value = new ArtPackList();

            Assert.IsNotNull(value.activeArtPacks);
            SimplifyIO.Data(stream, ref value.activeArtPacks, "ActiveArtPacks");
            SimplifyIO.Data(stream, ref value.artPackHiddenIdentities, "ArtPackHiddenIdentities");
        }
    }

    //Europa stuff
    public enum ModuleInfoType
    {
        STANDARD,
        INCOME_FIXED,
        INCOME_PRICE,
        INCOME_CEILING,
        HUMANITARIAN,
        NUM_TYPES
    }

    public enum PopulationBarType
    {
        STANDARD,
        POPULATION,
        HOUSING
    }
    public class CustomColonySettings
    {
        private ModuleInfoType infoType = ModuleInfoType.STANDARD;
        private PopulationBarType barType = PopulationBarType.STANDARD;
        public ModuleInfoType InfoType { get { return infoType; } set { infoType = value; } }
        public PopulationBarType BarType { get { return barType; } set { barType = value; } }

        public static void Serialize(object stream, ref CustomColonySettings value, int compatibilityNumber)
        {
            if (SimplifyIO.IsReading(stream))
            {
                if (value == null)
                    value = new CustomColonySettings();
            }

            SimplifyIO.Data(stream, ref value.infoType, "InfoType");
            SimplifyIO.Data(stream, ref value.barType, "BarType");
        }
    }

    public class ScenarioShipmentRequest
    {
        private ResourceType meResource = ResourceType.NONE;
        private int miAmount = -1;
        private int miDeadline = -1;
        private int miPenalty = 0;
        private int miInterval = -1;

        public ResourceType shipmentResource { get { return meResource; } set { meResource = value; } }
        public int shipmentAmount { get { return miAmount; } set { miAmount = value; } }
        public int shipmentDeadline { get { return miDeadline; } set { miDeadline = value; } }
        public int shipmentPenalty { get { return miPenalty; } set { miPenalty = value; } }
        public int shipmentInterval { get { return miInterval; } set { miInterval = value; } }

        public static void Serialize(object stream, ref ScenarioShipmentRequest value, int compatibilityNumber)
        {
            if (SimplifyIO.IsReading(stream))
            {
                if (value == null)
                    value = new ScenarioShipmentRequest();
            }

            SimplifyIO.Data(stream, ref value.meResource , "ShipmentResource");
            SimplifyIO.Data(stream, ref value.miAmount, "ShipmentAmount");
            SimplifyIO.Data(stream, ref value.miDeadline, "ShipmentDeadline");
            SimplifyIO.Data(stream, ref value.miPenalty, "ShipmentPenalty");
            SimplifyIO.Data(stream, ref value.miInterval, "ShipmentInterval");
        }
    }
    //end Europa stuff

    public class PlayerSettings
    {
        private string name = "";
        private sbyte team = 0;
        private sbyte handicap = 0;
        private sbyte gender = 0;
        private ArtPackList artPackList = new ArtPackList();

        public string Name              { get { return name;                    }   set { name = value;               } }
        public sbyte Team               { get { return team;                    }   set { team = value;               } }
        public HandicapType Handicap    { get { return (HandicapType)handicap;  }   set { handicap = (sbyte) value;   } }
        public GenderType Gender        { get { return (GenderType)gender;      }   set { gender = (sbyte) value;     } }
        public ArtPackList ArtPackList  { get { return artPackList;             }   set { artPackList = new ArtPackList(value); } } //make list copy

        public PlayerSettings()
        {
        }

        public PlayerSettings(PlayerSettings source)
        {
            Set(source.Name, source.Team, source.Handicap, source.Gender, source.ArtPackList);
        }

        public PlayerSettings(string name, GenderType gender, ArtPackList artPackList)
        {
            Set(name, gender, artPackList);
        }

        public PlayerSettings(string name, sbyte team, HandicapType handicap, GenderType gender, ArtPackList artPackList)
        {
            Set(name, team, handicap, gender, artPackList);
        }

        public void Set(string name, GenderType gender, ArtPackList artPackList)
        {
            this.name = name;
            this.team = (sbyte)TeamType.NONE;
            this.handicap = (sbyte)Globals.Infos.Globals.DEFAULT_HANDICAP;
            this.gender = (sbyte)gender;
            this.artPackList = new ArtPackList(artPackList);
        }

        public void Set(string name, sbyte team, HandicapType handicap, GenderType gender, ArtPackList artPackList)
        {
            this.name = name;
            this.team = team;
            this.handicap = (sbyte)handicap;
            this.gender = (sbyte)gender;
            this.artPackList = new ArtPackList(artPackList); //make list copy
        }

        public static void Serialize(object stream, ref PlayerSettings value)
        {
            if(SimplifyIO.IsReading(stream) && (value == null))
                value = new PlayerSettings();

            Assert.IsNotNull(value.artPackList);
            SimplifyIO.Data(stream, ref value.name, "PlayerName");
            SimplifyIO.Data(stream, ref value.team, "PlayerTeam");
            SimplifyIO.Data(stream, ref value.handicap, "PlayerHandicap");
            SimplifyIO.Data(stream, ref value.gender, "PlayerGender");
            ArtPackList.Serialize(stream, ref value.artPackList);
        }
    }

    public class GameSettings
    {
        public int miSeed = 0;
        public int miWidth = 0;
        public int miHeight = 0;
        public int miEdgeTilePadding = 0;
        public int miNumPlayers = 0;
        public int miNumObservers = 0;
        public int miNumHumans = 0;
        public int miNumUniqueTeams = 0;
        public GameSpeedType meGameSpeed = (GameSpeedType)0;
        public MapSizeType meMapSize = (MapSizeType)0;
        public TerrainClassType meTerrainClass = (TerrainClassType)0;
        public LatitudeType meLatitude = (LatitudeType)0;
        public ResourceMinimumType meResourceMinimum = (ResourceMinimumType)0;
        public ResourcePresenceType meResourcePresence = (ResourcePresenceType)0;
        public ColonyClassType meColonyClass = ColonyClassType.NONE;
        public RulesSetType meRulesSetType = (RulesSetType)0;
        public GameSetupType meGameSetupType = GameSetupType.NONE;
        public LevelType meLevelType = LevelType.NONE;
        public LocationType meLocation = LocationType.MARS;
        public ScenarioDifficultyType meScenarioDifficulty = ScenarioDifficultyType.NONE;
        public string mzMap = "";
        public string mzMapName = "";
        public bool mbSkipWin = false;

        public List<bool> mabGameOptions = new List<bool>();
        public List<bool> mabInvalidHumanHQ = new List<bool>();
        public List<PlayerSettings> playerSlots = new List<PlayerSettings>();
        
        public static void Serialize(object stream, ref GameSettings value, int compatibilityNumber)
        {
            if (SimplifyIO.IsReading(stream))
            {
                if(value == null)
                    value = new GameSettings();
                value.playerSlots.Resize(Constants.MAX_LOBBY_SLOTS, null);
            }

            SimplifyIO.Data(stream, ref value.miSeed, "Seed");
            SimplifyIO.Data(stream, ref value.miWidth, "Width");
            SimplifyIO.Data(stream, ref value.miHeight, "Height");
            SimplifyIO.Data(stream, ref value.miEdgeTilePadding, "EdgeTilePadding");
            SimplifyIO.Data(stream, ref value.miNumPlayers, "NumPlayers");
            SimplifyIO.Data(stream, ref value.miNumObservers, "NumObservers");
            SimplifyIO.Data(stream, ref value.miNumHumans, "NumHumans");
            SimplifyIO.Data(stream, ref value.miNumUniqueTeams, "NumUniqueTeams");
            SimplifyIO.Data(stream, ref value.meGameSpeed, "GameSpeed");
            SimplifyIO.Data(stream, ref value.meMapSize, "MapSize");
            SimplifyIO.Data(stream, ref value.meLatitude, "Latitude");
            SimplifyIO.Data(stream, ref value.meTerrainClass, "TerrainClass");
            SimplifyIO.Data(stream, ref value.meResourceMinimum, "ResourceMinimum");
            SimplifyIO.Data(stream, ref value.meResourcePresence, "ResourcePresence");
            SimplifyIO.Data(stream, ref value.meColonyClass, "ColonyClass");
            SimplifyIO.Data(stream, ref value.meRulesSetType, "RulesSetType");
            SimplifyIO.Data(stream, ref value.meGameSetupType, "GameSetupType");
            SimplifyIO.Data(stream, ref value.meLevelType, "LevelType");
            SimplifyIO.Data(stream, ref value.meLocation, "Location");
            SimplifyIO.Data(stream, ref value.meScenarioDifficulty, "ScenarioDifficulty");
            SimplifyIO.Data(stream, ref value.mzMap, "MapFile");
            SimplifyIO.Data(stream, ref value.mzMapName, "MapName");
            SimplifyIO.Data(stream, ref value.mbSkipWin, "SkipWin");

            SimplifyIO.Data(stream, ref value.mabGameOptions, (int)(Globals.Infos.gameOptionsNum()), "GameOption");
            SimplifyIO.Data(stream, ref value.mabInvalidHumanHQ, (int)(Globals.Infos.HQsNum()), "InvalidHumanHQ");

            Assert.AreEqual(value.playerSlots.Count, Constants.MAX_LOBBY_SLOTS);
            for(int i=0; i<value.playerSlots.Count; i++)
            {
                PlayerSettings entry = value.playerSlots[i];
                PlayerSettings.Serialize(stream, ref entry);
                value.playerSlots[i] = entry;
            }
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine("-------GameSettings--------");
            output.Append("Seed: ").Append(miSeed).AppendLine();
            output.Append("Width: ").Append(miWidth).AppendLine();
            output.Append("Height: ").Append(miHeight).AppendLine();
            output.Append("NumPlayers: ").Append(miNumPlayers).AppendLine();
            output.Append("NumObservers: ").Append(miNumObservers).AppendLine();
            output.Append("NumHumans: ").Append(miNumHumans).AppendLine();
            output.Append("NumTeams: ").Append(miNumUniqueTeams).AppendLine();
            output.Append("GameSpeed: ").Append(meGameSpeed).AppendLine();
            output.Append("MapSize: ").Append(meMapSize).AppendLine();
            output.Append("TerrainClass: ").Append(meTerrainClass).AppendLine();
            output.Append("Latitude: ").Append(meLatitude).AppendLine();
            output.Append("ResourceMinimum: ").Append(meResourceMinimum).AppendLine();
            output.Append("ResourcePresence: ").Append(meResourcePresence).AppendLine();
            output.Append("ColonyClass: ").Append(meColonyClass).AppendLine();
            output.Append("RulesSet: ").Append(meRulesSetType).AppendLine();
            output.Append("GameSetup: ").Append(meGameSetupType).AppendLine();
            output.Append("LevelType: ").Append(meLevelType).AppendLine();
            output.Append("Location: ").Append(meLocation).AppendLine();
            output.Append("ScenarioDifficulty: ").Append(meScenarioDifficulty).AppendLine();
            output.Append("MapPath: ").Append(mzMap).AppendLine();
            output.Append("MapName: ").Append(mzMapName).AppendLine();
            output.Append("SkipWin: ").Append(mbSkipWin).AppendLine();
            output.AppendLine("---------------------------");

            /* Add these if useful
            public List<bool> mabGameOptions = new List<bool>();
            public List<bool> mabInvalidHumanHQ = new List<bool>();
            public List<PlayerSettings> playerSlots = new List<PlayerSettings>();
            */

            return output.ToString();
        }
    }

    public class HelpInfo
    {
        public string strHelp;
    }

    public class MessageInfo
    {
        public string mzText;
        public AudioTypeT meAudio;
        public int miID;
        public int miUpdateCount;
        public int miTileID;
        
        public MessageInfo(string zText, AudioTypeT eAudio, int iID, int iUpdateCount, int iTileID)
        {
            mzText = zText;
            meAudio = eAudio;
            miID = iID;
            miUpdateCount = iUpdateCount;
            miTileID = iTileID;
        }
    }

    public class MissionInfo
    {
        public MissionType meMission;
        public int miData;
        
        public MissionInfo(MissionType eMission, int iData)
        {
            meMission = eMission;
            miData = iData;
        }
    }

    public class OrderInfo
    {
        public OrderType meType;
        public int miIndex;
        public int miTime;
        public int miOriginalTime;
        public int miData1;
        public int miData2;
        public int miBuildingID;

        public OrderInfo()
        {
            Set(OrderType.NONE, -1, -1, -1, -1, -1, -1);
        }
        
        public OrderInfo(OrderType eType, int iIndex, int iTime, int iOriginalTime, int iData1, int iData2, int iBuildingID)
        {
            Set(eType, iIndex, iTime, iOriginalTime, iData1, iData2, iBuildingID);
        }

        private void Set(OrderType eType, int iIndex, int iTime, int iOriginalTime, int iData1, int iData2, int iBuildingID)
        {
            meType = eType;
            miIndex = iIndex;
            miTime = iTime;
            miOriginalTime = iOriginalTime;
            miData1 = iData1;
            miData2 = iData2;
            miBuildingID = iBuildingID;
        }
    }

    public class EventGameTime
    {
        public EventGameType meEventGame;
        public PlayerType mePlayer;
        public int miDelay;
        public int miTime;
        public int miMultiplier;
        public int miStartTurn;

        public EventGameTime(EventGameType eEventGame, PlayerType ePlayer, int iDelay, int iTime, int iMultiplier, int iStartTurn)
        {
            meEventGame = eEventGame;
            mePlayer = ePlayer;
            miDelay = iDelay;
            miTime = iTime;
            miMultiplier = iMultiplier;
            miStartTurn = iStartTurn;
        }
    }

    public class TerrainInfo
    {
        public TerrainType Terrain;
        public HeightType  Height;
        public WindType    Wind;
        public IceType     Ice;
        
        public int CraterChunkID;
        public DirectionType CraterChunkDir;
        public int IsCrater;
        
        public TerrainInfo(TerrainType eTerrain, HeightType eHeight, WindType eWind, IceType eIce)
        {
            Terrain = eTerrain;
            Height = eHeight;
            Wind = eWind;
            Ice = eIce;
            
            CraterChunkID = -1;
            CraterChunkDir = DirectionType.E;
            IsCrater = 0;
        }
    }
}