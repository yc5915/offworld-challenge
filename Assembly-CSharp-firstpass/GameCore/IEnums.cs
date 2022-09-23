namespace Offworld.GameCore
{
    //-------------------------------------------------------------------------------------------------
    // Regular enums
    //-------------------------------------------------------------------------------------------------

    public enum GameModeType
    {
        NONE = -1,

        SKIRMISH,
        TUTORIAL,
        DAILY_CHALLENGE,
        INFINITE_CHALLENGE,
        CAMPAIGN,
        MULTIPLAYER,
        STAND_ALONE_SERVER,
        SCENARIOS,

        NUM_TYPES
    }

    public enum ScenarioClassType
    {
        GENERAL,
        TUTORIAL,
        BLUES,
        PRACTICE_CHALLENGES,
        BOB,
        EUROPA,

        NUM_TYPES
    };

    public enum TutorialCategoryIndex
    {
        GENERAL = 0,
        IO,

        NUM_TYPES
    };

    public enum AuctionState
    {
        START,
        END_WINNER,
        END_NO_WINNER,
        FIRST_BID,
        OUTBID,
        
        NUM_TYPES
    }

    public enum AuctionType
    {
        NONE = -1,
        
        PATENT,
        BLACK_MARKET_SABOTAGE,
        TILE,
        TILE_BUILDING,
        CLAIM,
        
        NUM_TYPES,
        
        PERK
    }

    public enum CampaignState
    {
        NONE = -1,

        GROWTH_ROUND,
        ELIMINATION_ROUND,
        FINAL_ROUND,
        LOST,
        WON,
    }

    public enum GenderType
    {
        MALE,
        FEMALE,
    }

    public enum ItemType
    {
        NONE = 0,
        TRADE,
        STOCK,
        BUYOUT,
        PAY_DEBT,
        AUTO_PAY_DEBT,
        ORDER,
        FOUND,
        SABOTAGE,
        SABOTAGE_TILE,
        BLACK_MARKET,
        UPGRADE,
        PING_TILE,
        CLAIM_TILE,
        RETURN_CLAIM,
        RETURN_CLAIM_SABOTAGE,
        CANCEL_CONSTRUCT,
        CONSTRUCT_BUILDING, // actually place the building 
        REPAIR,
        SCRAP,
        SCRAP_ALL,
        SEND_RESOURCES,
        SEND_RESOURCES_ALL,
        SUPPLY_BUILDING,
        TOGGLE_AUTO_OFF,
        TOGGLE_AUTO_OFF_ALL,
        TOGGLE_OFF,
        TOGGLE_OFF_ALL, // toggle off all the buildings of a certain type
        TOGGLE_OFF_EVERYTHING, // toggle off all buildings
        AUTO_SUPPLY_BUILDING_INPUT, // automatically purchase resources for this building
        HOLOGRAM_BUILDING,
        SEND_PATENT,
        CRAFT_BLACK_MARKET,
        PATENT,
        RESEARCH,
        ESPIONAGE,
        LAUNCH,
        IMPORT,
        SHIPMENT,
        AUCTION_TILE,
        TILE,
        SCAN,
        AUCTION_BID,
        AUCTION_SKIP,
        CANCEL_ORDER,
        SELL_ALL_RESOURCES,
        TOGGLE_HOLD_RESOURCE,
        TOGGLE_SHARE_RESOURCE,
        TOGGLE_SHARE_ALL_RESOURCES,
        TOGGLE_CHEATING,
        BUY_COLONY_STOCK,
        BUY_COLONY_MODULE,
        SUPPLY_WHOLESALE,
        COLONY_MODULES,
        COLONY_STAT_JOBS,
        COLONY_STAT_HOUSING,
        COLONY_STAT_POPULATION,
        COLONY_STAT_OVERVIEW,
        PLAYER_OPTION,
        CONCEDE,
        BEAT_SOREN,
        IS_SOREN,
        BEAT_ZULTAR,
        IS_ZULTAR,
        PLAYER_RANK,
        SCENARIO_GAME_INFO,
        IMPORT_BUTTON,
        COLONY_CLASS_HOVER,
        EVENT_STATE_GAME_HOVER,
        EVENT_STATE_LEVEL_HOVER,

        TOGGLE_PAUSE, // top right
        SPEED_UP,
        SLOW_DOWN,
        SPEED_DISPLAY,
        TIME_DISPLAY,
        SEEKING_QUICKMATCH,
        MENU,
        HELP,

        SELECT_AND_VIEW_TILE, // networked, but also modifies the camera
        SV_EMPTY_TILE,
        SV_DESTROYED_BUILDING,
        SV_CONSTRUCTING_BUILDING,
        SV_MUTINY,
        SV_TURNED_OFF,
        CLOSE_FLYOUT, // changes selection

        // non-networked item types
        RESOURCE, // when you mouseover the resource itself, like the iron icon. Also when you click it it focuses on the next iron spot
        RESOURCE_LEVEL, // when you mouseover the resource level in the resource deposit display. When you click it, it will take you to the next spot with that resource.
        RESOURCE_RATE, // when you mouseover the resource rate
        RESOURCE_MONEY_CHANGE, // when you mouseover the -$3 or +$3 value for a resource
        RESOURCE_QUANTITY, // when you mouseover the resource quantity
        RESOURCE_PRICE, // when you mouseover the resource price
        RESOURCE_WARNING,
        TOGGLE_RESOURCE_VIEW,
        CLAIM, // go into claim tile mode
        CONSTRUCT, // go into building placement mode
        DEBT, // mouseover for debt value
        POPUP_CHOICE,
        CLOSE_POPUP,
        CHARACTER_DIALOG_CHOICE,
        CLOSE_CHARACTER_DIALOG,
        SPECIAL_BUILDING_SELECT,
        VIEW_TILE,
        PLAYER_INFO,
        PLAYER_PERK_INFO_HOVER,
        BOUGHT_PLAYER,
        BUILDING_TOGGLE,
        RESOURCE_BUILDING_DISPLAY,
        RESOURCE_SURPLUS,
        RESOURCE_SHORTAGE,
        OBSERVER_CHANGE_PLAYER,
        SCAN_RESOURCE, // the resource bars at the bottom of the screen in the scan mode
        SCAN_GEOTHERMAL,
        CYCLE_SPECIAL_BUILDINGS,
        SHARED_RESOURCE, // tooltip for sharing a resource
        PERK,
        STOCK_LINE, // the line that shows all the player's stock in little boxes
        TERRAIN_TYPE,
        BLACK_MARKET_COOLDOWN,
        FOUND_ORDER_BONUS,
        BUYOUT_THREAT,
        WINNER_HOVER, // in the player list, a crown for who the current winner is

        // NEW UI
        TAB,
        RESOURCE_BUILDING_FLYOUT,
        FUNDS_CUSTOM_HOVER,

        REVEAL_MAP_DEBT,
        REVEAL_MAP_STOCK_PRICE,
        REVEAL_MAP_BOND_RATING,

        AUCTION_ITEM,
        AUCTION_MINIMIZE,
        AUCTION_UPCOMING,
        UPGRADE_BUILDING,

        NUM_ITEM_TYPES
    }

    public enum LevelStateType
    {
        UNOPENED,
        OPENED,
        CLOSED,

        NUM_TYPES
    }

    public enum MissionType
    {
        NONE = -1,
        
        CONSTRUCT,
        REPAIR,
        MINE,
        SHIP_HQ,
        SHIP_BUILDING,
        
        NUM_TYPES
    }

    public enum PlayerType
    {
        UNKNOWN = -2,
        NONE = -1
    }

    public enum PluralType
    {
        SINGULAR,
        PLURAL,
        PLURAL2,
    }

    public enum StatsType
    {
        RESOURCE,
        BUILDING_CLASS,
        BLACK_MARKET,
        TECHNOLOGY,
        PATENT,
        SABOTAGING,
        SABOTAGED,
        STOCK,
        MISCELLANEOUS,
        
        NUM_TYPES
    }

    public enum TeamType
    {
        NONE = -1
    }

    public enum VisibilityType
    {
        NONE = -1,
        
        FOGGED,
        REVEALED,
        VISIBLE,
        
        NUM_TYPES
    }

    public enum StockState
    {
        UNPROTECTED,
        PROTECTED,
        DANGER,
        IMMEDIATE_DANGER,

        NUM_TYPES
    }

    //Parallel with the material slots on the TerrainCollectors
    public enum TerrainMaterialType
    {
        TERRAIN_MATERIAL_CRATERS,
        TERRAIN_MATERIAL_HILLS,
        TERRAIN_MATERIAL_EDGES,
        TERRAIN_MATERIAL_SCULPTED_HILLS,
        TERRAIN_MATERIAL_CANYON_EDGES,
        TERRAIN_MATERIAL_CAVES,
        TERRAIN_MATERIAL_LAKES,
        TERRAIN_MATERIAL_LOCATION_HILLS,

        NUM_TYPES
    }

    //-------------------------------------------------------------------------------------------------
    // XML enums for Infos
    //-------------------------------------------------------------------------------------------------

    public enum AchievementType
    {
        NONE = -1
    }

    public enum AdjacencyBonusType
    {
        NONE = -1,
        ZERO,
        ONE,
        TWO,
        THREE,
        FOUR,
        FIVE,
        SIX,
    }

    public enum ArtPackType
    {
        NONE = -1
    }

    public enum AssetType
    {
        NONE = -1
    }

    public enum AudioTypeT //renamed to prevent conflict with UnityEngine.AudioTypeT
    {
        NONE = -1
    }

    public enum BlackMarketType
    {
        NONE = -1,

        NEW_CLAIM,
        RETURN_CLAIM,
        COOK_BOOKS,
        AUCTION,
        HOLOGRAM,
        SPY,
        MULE,
        PIRATES,
        MAGNETIC_STORM,
        EMP,
        POWER_SURGE,
        CIRCUIT_OVERLOAD,
        ADRENALINE_BOOST,
        SLOWDOWN_STRIKE,
        NETWORK_VIRUS,
        CORE_SAMPLE,
        DRILL,
        UNDERGROUND_NUKE,
        DYNAMITE,
        MUTINY,
        GOON_SQUAD
    }

    public enum BlackMarketClassType
    {
        NONE = -1,
        FROZEN,
        SINGLE
    }

    public enum BondType
    {
        NONE = 0,
        AAA,
        AA,
        A,
        B,
        C,
        D
    }

    public enum BuildingType
    {
        NONE = -1,

        SOLAR_PANEL,
        SOLAR_PANEL_EUROPA,
        WIND_TURBINE,
        GEOTHERMAL_PLANT,
        NUCLEAR_PLANT,
        NUCLEAR_PLANT_SCIENTIFIC,
        ION_COLLECTOR,
        WATER_PROCESSOR,
        WATER_PROCESSOR_SCIENTIFIC,
        WATER_PUMP,
        HYDROPONIC_FARM,
        HYDROPONIC_FARM_SCIENTIFIC,
        ELECTROLYSIS_REACTOR,
        ELECTROLYSIS_REACTOR_SCIENTIFIC,
        METAL_MINE,
        STEEL_MILL,
        STEEL_MILL_SCIENTIFIC,
        ELEMENTAL_QUARRY,
        CHEMICALS_LAB,
        CHEMICALS_LAB_SCIENTIFIC,
        GLASS_FURNACE,
        GLASS_FURNACE_SCIENTIFIC,
        ELECTRONICS_FACTORY,
        ELECTRONICS_FACTORY_SCIENTIFIC,
        ELECTRONICS_FACTORY_EUROPA,
        ELECTRONICS_FACTORY_EUROPA_SCIENTIFIC,
        ICE_CONDENSER,
        BASALT_PLATFORM,
        PATENT_LAB,
        ENGINEERING_LAB,
        HACKER_ARRAY,
        PLEASURE_DOME,
        PLEASURE_DOME_CHEMICALS,
        OFFWORLD_MARKET,
        SPACE_ELEVATOR,
        HYDROTHERMAL_PLANT,
        GAS_PLANT,
        GAS_PLANT_SCIENTIFIC,
        METHANE_EXTRACTOR,
    }

    public enum BuildingClassType
    {
        NONE = -1,

        SOLAR_PANEL,
        WIND_TURBINE,
        GEOTHERMAL_PLANT,
        NUCLEAR_PLANT,
        ION_COLLECTOR,
        WATER_PROCESSOR,
        WATER_PUMP,
        HYDROPONIC_FARM,
        ELECTROLYSIS_REACTOR,
        METAL_MINE,
        STEEL_MILL,
        ELEMENTAL_QUARRY,
        CHEMICALS_LAB,
        GLASS_FURNACE,
        ELECTRONICS_FACTORY,
        ICE_CONDENSER,
        BASALT_PLATFORM,
        PATENT_LAB,
        ENGINEERING_LAB,
        HACKER_ARRAY,
        PLEASURE_DOME,
        OFFWORLD_MARKET,
        SPACE_ELEVATOR,
        HYDROTHERMAL_PLANT,
        GAS_PLANT,
        METHANE_EXTRACTOR,
    }

    public enum CampaignModeType
    {
        NONE = -1
    }

    public enum CharacterType
    {
        NONE = -1,

        HUMAN,
        BUNNIES,
        EXPANSIVE_FEMALE,
        EXPANSIVE_MALE,
        ROBOTIC_LARGE,
        ROBOTIC_SMALL,
        SCAVENGER_MALE,
        SCAVENGER_FEMALE,
        SCIENTIFIC_FEMALE,
        SCIENTIFIC_MALE,
        ENTERTAINMENT,
        BLACK_MARKET,
        INVENTOR,
        BANKER,
        BOREHOLE,
        ADVANCED,
    }

    public enum ColorType
    {
        NONE = -1,

        CYAN,
        WARNING,
        POSITIVE,
        NEGATIVE,
        MUTED,
        GOAL_ACTIVE,
        GOAL_COMPLETE,
        HIGHLIGHT,
        COLONY_HABITAT,
        COLONY_WORKPLACE,
        HIGHLIGHT_SALTS,
        HIGHLIGHT_CAVES,
        HIGHLIGHT_BASALT,
        HIGHLIGHT_ICE_WATER,
        HIGHLIGHT_ICE_DRY,
        HIGHLIGHT_ICE_OXIDE,
        HIGHLIGHT_TEXT,
    }

    public enum ColonyType
    {
        NONE = -1,
    }

    public enum ColonyBonusType
    {
        NONE = -1,
    }

    public enum ColonyBonusLevelType
    {
        NONE = -1,
    }

    public enum ColonyClassType
    {
        NONE = -1,

        RESIDENTIAL,
        AUTOMATED,
        PENAL,
        RESEARCH,
        TRANSIT_HUB,
        SECURITY_CENTER,
        SHIPYARD,
        ENGINEERING,
        POWER_STATION,
        FARMING,
        LIFE_SUPPORT,
        RESERVOIR,
        MINING,
        QUARRY,
        CONSTRUCTION,
        INDUSTRY,
        MEDICAL,
        PENAL_IO,
        RESEARCH_IO,
        SECURITY_CENTER_IO,
        ENGINEERING_IO,
        POWER_STATION_IO,
        FARMING_IO,
        OXYGEN_EXTRACTION,
        ION_COLLECTION,
        WATER_PROCESSING,
        QUARRY_IO,
        RADIOACTIVES,
        NUCLEAR_POWER,
        WASTE_COLLECTION,
        RADIATION_SHIELDING_ASSEMBLY,
        LAVA_PROCESSING,
        MARKET_CENTER_IO,
    }

    public enum ConditionType
    {
        NONE = -1,

        DEFAULT,
        DAYS,
        MAX_UPGRADE,
        SCRIPTED_WIN,
    }

    public enum CorporationType
    {
        NONE = -1,

        HUMAN
    }

    public enum DirectionType
    {
        NONE = -1,

        NW,
        NE,
        E,
        SE,
        SW,
        W,

        NUM_TYPES
    }

    public enum EspionageType
    {
        NONE = -1,

        POWER_SHORTAGE,
        POWER_SURPLUS,
        WATER_SHORTAGE1,
        WATER_SURPLUS1,
        WATER_SHORTAGE2,
        WATER_SURPLUS2,
        WATER_SHORTAGE,
        WATER_SURPLUS,
        FOOD_SHORTAGE,
        FOOD_SURPLUS,
        OXYGEN_SHORTAGE,
        OXYGEN_SURPLUS,
        FUEL_SHORTAGE,
        FUEL_SURPLUS,
        URANIUM_SHORTAGE,
        URANIUM_SURPLUS,
        MAGNESIUM_SHORTAGE,
        MAGNESIUM_SURPLUS,
        ALUMINUM_SHORTAGE,
        ALUMINUM_SURPLUS,
        IRON_SHORTAGE,
        IRON_SURPLUS,
        STEEL_SHORTAGE,
        STEEL_SURPLUS,
        CARBON_SHORTAGE,
        CARBON_SURPLUS,
        SILICON_SHORTAGE,
        SILICON_SURPLUS,
        CHEMICALS_SHORTAGE,
        CHEMICALS_SURPLUS,
        GLASS_SHORTAGE,
        GLASS_SURPLUS,
        ELECTRONICS_SHORTAGE,
        ELECTRONICS_SURPLUS,
    }

    public enum EventGameType
    {
        NONE = -1,

        DUST_STORM,
        SOLAR_FLARE,
        SULFUR_FROST,
        TREMORS,
        TREMORS_EUROPA,
        LANDSLIDE,
        RADIATION_STORM,
        RADIATION_STORM_EUROPA,
        ECLIPSE,
        NEW_CLAIM,
        IOQUAKE,
        MARSQUAKE,
        EUROPAQUAKE,
        DEPRESSURIZED,
        SHORT_CIRCUIT,
        ROBBERY,
        OXIDATION,
        LEAK,
        FIRE,
        EXPLOSION,
        COLLAPSE,
        POWER_SURPLUS,
        WATER_SURPLUS,
        FOOD_SURPLUS,
        OXYGEN_SURPLUS,
        FUEL_SURPLUS,
        URANIUM_SURPLUS,
        MAGNESIUM_SURPLUS,
        ALUMINUM_SURPLUS,
        IRON_SURPLUS,
        STEEL_SURPLUS,
        CARBON_SURPLUS,
        CHEMICALS_SURPLUS,
        SILICON_SURPLUS,
        GLASS_SURPLUS,
        ELECTRONICS_SURPLUS,
        POWER_SHORTAGE,
        WATER_SHORTAGE,
        FOOD_SHORTAGE,
        OXYGEN_SHORTAGE,
        FUEL_SHORTAGE,
        URANIUM_SHORTAGE,
        MAGNESIUM_SHORTAGE,
        ALUMINUM_SHORTAGE,
        IRON_SHORTAGE,
        STEEL_SHORTAGE,
        CARBON_SHORTAGE,
        CHEMICALS_SHORTAGE,
        SILICON_SHORTAGE,
        GLASS_SHORTAGE,
        ELECTRONICS_SHORTAGE,
    }

    public enum EventLevelType
    {
        NONE = -1,
    }

    public enum EventStateType
    {
        NONE = -1,

        EASTWARD_WIND,
        WESTWARD_WIND,
        EXPENSIVE_UPGRADES,
        HIGHER_LIFE_SUPPORT,
        FASTER_BLACK_MARKET,
        NO_ADJACENCY,
        DUST_STORM,
        SOLAR_FLARE,
        SULFUR_FROST,
        TREMORS,
        TREMORS_EUROPA,
        LANDSLIDE,
        RADIATION_STORM,
        RADIATION_STORM_EUROPA,
        ECLIPSE,
        EUROPAQUAKE,
        MARSQUAKE,
        IOQUAKE,
        WATER_PUMP_STRIKE,
        ELECTROLYSIS_FARM_STRIKE,
        ELECTROLYSIS_REACTOR_STRIKE,
        METAL_MINE_STRIKE,
        STEEL_MILL_STRIKE,
        ELEMENTAL_QUARRY_STRIKE,
        CHEMICALS_LAB_STRIKE,
        GLASS_FURNACE_STRIKE,
        ELECTRONICS_FACTORY_STRIKE,
        ION_COLLECTOR_STRIKE,
        WATER_PROCESSOR_STRIKE,
        NEW_CLAIM,
    }

    public enum EventTurnType
    {
        NONE = -1,
    }

    public enum EventAudioType
    {
        NONE = -1,
    }

    public enum EventTurnOptionType
    {
        NONE = -1
    }

    public enum ExecutiveType
    {
        NONE = -1,

        PRODUCTION,
        STOCK,
        UPGRADE,
        LAUNCH,
        SABOTAGE,
        ESPIONAGE,
        PATENT,
        RESEARCH,
        ENTERTAINMENT,
        INVENTOR,
        BANKER,
        BOREHOLE,
        ADVANCED,
    }

    public enum GameOptionType
    {
        NONE = -1,

        NO_AUCTIONS,
        NO_RANDOM_EVENTS,
        NO_SABOTAGE,
        ADVANCED_SABOTAGE,
        REVEAL_MAP,
        RANDOM_PRICES,
        STOCK_DELAY,
        CHANGE_SPEED,
        SEVEN_SOLS,
        IRONMAN,
        DAILY_SEED,
        REAL_MAPS,
        SIM_MISSIONS,
        AI_BUYOUT,
        HIDDEN_IDENTITIES,
        INCLUDE_CERES,
        ALL_HQS,
        MARATHON_MODE,
    }

    public enum GamePhaseType
    {
        NONE = -1,
        
        GAMEPHASE_SCAN,
        GAMEPHASE_FOUNDED,
        
        NUM_TYPES
    }

    public enum GameSetupType
    {
        NONE = -1,
        DEFAULT,
        TUTORIAL_ROBOTIC,
        TUTORIAL_EXPANSIVE,
        TUTORIAL_SCAVENGER,
        TUTORIAL_SCIENTIFIC,
        TUTORIAL_MANAGER,
        TUTORIAL_NOMADIC,
        TUTORIAL_ELITE,
    }

    public enum GameSpeedType
    {
        NONE = -1,
        SNAIL,
        SLOW,
        NORMAL,
        FAST,
        BLAZING,
        FAST_FORWARD,
    }
    
    public enum GlobalsIntType
    {
        NONE = -1
    }

    public enum GlobalsFloatType
    {
        NONE = -1
    }

    public enum GlobalsStringType
    {
        NONE = -1
    }
    
    public enum GlobalsTypeType
    {
        NONE = -1
    }

    public enum HandicapType
    {
        OBSERVER = -2,
        NONE = -1,
        APPLICANT,
        INTERN,
        ASSISTANT,
        EMPLOYEE,
        MANAGER,
        EXECUTIVE,
        VP,
        CEO,
        GURU,
        NUM_TYPES
    }

    public enum HeightType
    {
        NONE = -1
    }

    public enum HintType
    {
        NONE = -1
    }

    public enum HQType
    {
        NONE = -1,

        EXPANSIVE,
        ROBOTIC,
        SCAVENGER,
        SCIENTIFIC,
        NOMADIC,
        ELITE,
    }

    public enum HQLevelType
    {
        NONE = -1,

        ZERO,
        ONE,
        TWO,
        THREE,
        FOUR,
        FIVE
    }

    public enum IceType
    {
        NONE = -1,

        WATER,
        DRY,
        OXIDE
    }

    public enum KeyBindingType
    {
        NONE = -1
    }
    
    public enum KeyBindingClassType
    {
        NONE = -1
    }

    public enum LanguageType
    {
        ENGLISH = 0,
        BRITISH,
        GERMAN,
        RUSSIAN,
        FRENCH,
        SPANISH,
        KOREAN,
        POLISH,
        BRAZILIAN,
        CHINESE,
        TEXT_DEBUG,
        MOD,
        NUM_TYPES
    }

    public enum LatitudeType
    {
        NONE = -1,

        NORTH,
        EQUATOR,
        SOUTH,

        NUM_TYPES
    }

    public enum LevelType
    {
        NONE = -1,
    }

    public enum LightingEnvironmentType
    {
        NONE = -1
    }

    public enum LocationType
    {
        NONE = -1,

        MARS,
        CERES,
        IO,
        EUROPA,

        NUM_TYPES
    }

    public enum MapNameType
    {
        NONE = -1,

        DEFAULT,
        CANYONS,
        PEAK,
        PLATEAU,
        STEAMS,
        STRIATIONS,
    }

    public enum MapSizeType
    {
        NONE = -1,

        TINY,
        SMALL,
        MEDIUM,
        LARGE,
        HUGE,
    }

    public enum MarkupType
    {
        NONE = -1
    }

    public enum ModuleType
    {
        NONE = -1,

        COLONY,
        PORT,
        FOUNDRY,
        HABITAT,
        LABORATORY,
        TOOLSHOP,
        OFFICE,
        WAREHOUSE,
        GARAGE,
        PRISON,
        SOLAR_PANEL,
        WIND_TURBINE,
        GREENHOUSE_FARM,
        ELECTROLYSIS_REACTOR,
        DEEP_PUMP,
        DEEP_MINE,
        DEEP_QUARRY,
        STEEL_MILL,
        GLASS_KILN,
        CHEMICALS_LAB,
        ELECTRONICS_FACTORY,
        HOSPITAL_WARD,
        OUTPATIENT_CENTER,
        NUCLEAR_PLANT,
        ION_COLLECTOR,
        ICE_CONDENSER_OXIDE,
        BASALT_PLATFORM,
        GEOTHERMAL_PLANT,
        WATER_PROCESSOR,
        SHIELDED_STORAGE,
        ROCKET_LAUNCHER,
        RADIATION_SHIELDING_FACTORY,
        STOCK_MARKET,
        COMMODITIES_MARKET_LIFE_SUPPORT,
        COMMODITIES_MARKET_RAW_MATERIALS,
        COMMODITIES_MARKET_REFINED_GOODS,
        CORPORATE_OFFICES,
    }

    public enum OrderType
    {
        NONE = -1,

        PATENT,
        RESEARCH,
        HACK,
        LAUNCH,
        IMPORT,
        SPECIAL, //for special buildings with one-off effects that don't need to interact with OrderInfo or create flyouts

        NUM_TYPES
    }

    public enum OrdinalType
    {
        NONE = -1
    }

    public enum PatentRandomizationType
    {
        NONE = -1
    }

    public enum PatentType
    {
        NONE = -1,

        ENERGY_VAULT,
        FINANCIAL_INSTRUMENTS,
        WATER_ENGINE,
        NUCLEAR_ENGINE,
        GEOTHERMAL_BOREHOLE,
        HYDROTHERMAL_BOREHOLE,
        PERPETUAL_MOTION,
        PERFECTED_PROSPECTING,
        TRANSPARENT_ALUMINUM,
        SUPERCONDUCTOR,
        VIRTUAL_REALITY,
        NANOTECH,
        COLD_FUSION,
        SYNTHETIC_MEAT,
        CARBON_SCRUBBING,
        LIQUID_BATTERIES,
        SLANT_DRILLING,
        THINKING_MACHINES,
        TELEPORTATION,
    }

    public enum PerkType
    {
        NONE = -1,
    }

    public enum PersonalityType
    {
        NONE = -1,

        HUMAN,
        PRODUCTION,
        STOCK,
        UPGRADE,
        LAUNCH,
        SABOTAGE,
        ESPIONAGE,
        PATENT,
        RESEARCH,
        ENTERTAINMENT,
        INVENTOR,
        BANKER,
        BOREHOLE,
        ADVANCED,
    }

    public enum PlayerColorType
    {
        NONE = -1,

        TURQUOISE,
        ORANGE,
        LIGHT_GREEN,
        RED,
        GREEN,
        BLUE,
        PURPLE,
        PINK,
        YELLOW,
        GREY,
    }

    //NOTE: this enum is used for serialization, so add new entries to the end (before NUM_TYPES of course)
    public enum PlayerOptionType
    {
        AUTO_SUPPLY_ALL,
        TEAM_SHARE_ALL,
        VERBOSE_MESSAGES,
        SHOW_TOTAL_COSTS,
        SHOW_BUILDING_NET,
        SHOW_TILE_TEXT,
        SHOW_TILE_MOUSEOVER,
        BUILD_MENU_STARTS_OPEN,
        BUILD_MENU_STAYS_OPEN,
        SHOW_REGION_TEXT,
        PAUSE_SELECTION,
        PAUSE_TAB,
        SHOW_NET_REVENUE_ALL,
        TURN_OFF_UNPROFITABLE_SP,
        TURN_OFF_UNPROFITABLE_MP,
        SHOW_ELO_SCORE,
        AUTO_DELETE_OLD_REPLAYS,

        NUM_TYPES
    }

    public enum ResourceType
    {
        NONE = -1,

        POWER,
        WATER,
        FOOD,
        OXYGEN,
        FUEL,
        URANIUM,
        ALUMINUM,
        MAGNESIUM,
        IRON,
        STEEL,
        CARBON,
        SILICON,
        CHEMICALS,
        GLASS,
        ELECTRONICS,
    }

    public enum ResourceColorType
    {
        NONE = -1,

        POWER,
        WATER,
        FOOD,
        OXYGEN,
        FUEL,
        URANIUM,
        ALUMINUM,
        IRON,
        STEEL,
        CARBON,
        CHEMICALS,
        SILICON,
        GLASS,
        ELECTRONICS,
    }

    public enum ResourceLevelType
    {
        NONE = 0,

        TRACE,
        LOW,
        MEDIUM,
        HIGH,
        VERY_HIGH,
    }

    public enum ResourceMinimumType
    {
        NONE = 0,
        NORMAL,
        DOUBLE
    }

    public enum ResourcePresenceType
    {
        NONE = 0, // RARE

        LOW,
        MEDIUM,
        HIGH,
    }

    public enum RulesSetType
    {
        NONE = -1,

        DEFAULT,
        TUTORIAL_ROBOTIC,
        TUTORIAL_EXPANSIVE,
        TUTORIAL_SCAVENGER,
        TUTORIAL_SCIENTIFIC,
        TUTORIAL_MANAGER,
        TUTORIAL_NOMADIC,
        TUTORIAL_ELITE,
    }

    public enum SabotageType
    {
        NONE = -1,

        RETURN_CLAIM,
        AUCTION,
        HOLOGRAM,
        SPY,
        MULE,
        PIRATES,
        MAGNETIC_STORM,
        EMP,
        POWER_SURGE,
        CIRCUIT_OVERLOAD,
        ADRENALINE_BOOST,
        SLOWDOWN_STRIKE,
        NETWORK_VIRUS,
        CORE_SAMPLE,
        DRILL,
        UNDERGROUND_NUKE,
        DYNAMITE,
        MUTINY,
        GOON_SQUAD,
    }

    public enum ScenarioType
    {
        NONE = -1
    }

    public enum ScenarioDifficultyType
    {
        NONE = -1
    }

    public enum SevenSolsType
    {
        NONE = -1,

        COLONY,
        WHOLESALE,
    }

    public enum SpriteGroupType
    {
        NONE = -1
    }

    public enum StoryType
    {
        NONE = -1
    }

    public enum TechnologyType
    {
        NONE = -1,

        POWER_PRODUCTION,
        WATER_PRODUCTION,
        FOOD_PRODUCTION,
        OXYGEN_PRODUCTION,
        FUEL_PRODUCTION,
        URANIUM_PRODUCTION,
        MAGNESIUM_PRODUCTION,
        ALUMINUM_PRODUCTION,
        IRON_PRODUCTION,
        STEEL_PRODUCTION,
        CARBON_PRODUCTION,
        SILICON_PRODUCTION,
        CHEMICAL_PRODUCTION,
        GLASS_PRODUCTION,
        ELECTRONICS_PRODUCTION,
    }

    public enum TechnologyLevelType
    {
        NONE = 0,

        IMPROVED,
        EFFICIENT,
        ADVANCED,
        PERFECT,
    }

    public enum TerrainType
    {
        NONE = -1,

        NORMAL,
        RIVERBED,
        LAKEBED,
        CRACK,
        VOLCANIC,
        ROCKY,
        CRATER,
        SAND,
        HILLS,
        SLOPE,
        CANYON,
        CLAY,
        SALTS,
        CAVE,
        BASALT,
        LAVA_FLOW,
        METHANE,
        NORMAL_LIGHT,
    }

    public enum TerrainClassType
    {
        NONE = -1,

        NORMAL,
        CHAOS,
        RIVERBEDS,
        LAKEBEDS,
        CANYON,
        PLATEAU,
        PLAINS,
        VOLCANO,
        CRATERS,
        BASIN,
        PATERA,
        NORMAL_EUROPA,
        CHAOS_EUROPA,
        STRIATIONS,
        METHANE_EUROPA,
        CRYOVOLCANO,
        CRATERS_EUROPA,
    }

    public enum TextType
    {
        NONE = -1
    }

    public enum UnitType
    {
        NONE = -1,

        DRONE,
        ENGINEER,
        BLIMP,
        MULE_SABOTAGE,
        PIRATE_SABOTAGE,
    }

    public enum WindType
    {
        NONE = -1,

        VERY_WEAK,
        WEAK,
        MODERATE,
        STRONG,
        VERY_STRONG,
    }

    public enum WorldAudioType
    {
        NONE = -1,
    }
}
