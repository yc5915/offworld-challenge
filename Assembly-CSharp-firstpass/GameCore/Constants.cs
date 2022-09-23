using UnityEngine;

namespace Offworld.GameCore
{
    public static class Constants
    {
        public const int UPDATE_PER_SECOND = 20;
        public const int MILLISECS_PER_UPDATE = (1000 / UPDATE_PER_SECOND);
        public const int MAXIMUM_MILLISECS_PER_FRAME = 5 * MILLISECS_PER_UPDATE; //allow at most 5 game updates per frame

        public const int DEFAULT_EDGE_TILE_PADDING = 16; // MUST BE A MULTIPLE OF 2 DUE TO HEX GRID STORED AS SQUARE GRID

    	public const int PRICE_MULTIPLIER = 100;
    	public const int PRICE_MIN = (1 * PRICE_MULTIPLIER);
        public const int WHOLE_PRICE_MAX = 1000;
        public const int PRICE_MAX = (WHOLE_PRICE_MAX * PRICE_MULTIPLIER);
        public const int TRADE_QUANTITY = 10;
        
        public const int RESOURCE_MULTIPLIER = 1000;
        public const int STOCK_MULTIPLIER = 1000;
        public const int BACKUP_RATE = 2;

        public const int LARGE_TRADE_SIZE = 100;
        public const int MEDIUM_TRADE_SIZE = 10;
        public const int SMALL_TRADE_SIZE = 1;

        public const int MAX_LOBBY_SLOTS = 10;
        public const int MAX_NUM_PLAYERS = 8;
        public const int DEFAULT_NUM_HQS = 4;

        public const float TILE_WIDTH = 8.66f;
        public const float TILE_HEIGHT = 10f;
        public const float ROW_OFFSET = TILE_HEIGHT * .75f;

        public const int TERRAIN_THICKNESS_MULTIPLIER = 1000;
        public const float UNIT_HEIGHT_OFFSET = 2.0f;
        public const float UNIT_TURN_SPEED = 1000.0f;

        public const int RESOURCES_PER_STOCKPILE_BARREL = 5;
        public const int STOCKPILE_BARRELS_PER_ROW = 10;
        public const int NUM_RESOURCES = 13;
        public const int MAX_RESOURCE_TANKS_PER_BUILDING = 3;

        public const int CONSTRUCTION_RATE = 100;

        public const float PRODUCTION_SPEED_MEDIUM = 0.5f;
        public const float PRODUCTION_SPEED_FAST = 1.25f;

        public const float MAX_TERRAIN_HEIGHT = 8f;

        public const float MIN_HOLOGRAM_HEIGHT = 3f;
        public const float MAX_HOLOGRAM_HEIGHT = 10f;

        public static readonly Vector3 NULL_VECTOR = new Vector3(12345, 23456, 34567);

        public const string HUD_FONT_SETTINGS_PATH = "HUDFontSettings";
        public const string MENU_BANNERS_PATH = "UI/Data/MainMenuBanners";
    }
}