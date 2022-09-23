namespace Offworld.GameCore
{
    public class GameFactory
    {
        public virtual Campaign createCampaign() { return new Campaign(); }

        public virtual BuildingClient createBuildingClient(GameClient pGame) { return new BuildingClient(pGame); }
        public virtual BuildingServer createBuildingServer(GameClient pGame) { return new BuildingServer(pGame); }

        public virtual ConditionManagerClient createConditionManagerClient(GameClient pGame) { return new ConditionManagerClient(pGame); }
        public virtual ConditionManagerServer createConditionManagerServer(GameClient pGame) { return new ConditionManagerServer(pGame); }

        public virtual ConstructionClient createConstructionClient(GameClient pGame) { return new ConstructionClient(pGame); }
        public virtual ConstructionServer createConstructionServer(GameClient pGame) { return new ConstructionServer(pGame); }

        public virtual GameClient createGameClient() { return new GameClient(); }
        public virtual GameServer createGameServer() { return new GameServer(); }

        public virtual GameEventsClient createGameEventsClient() { return new GameEventsClient(); }
        public virtual GameEventsServer createGameEventsServer() { return new GameEventsServer(); }

        public virtual HQClient createHQClient(GameClient pGame) { return new HQClient(pGame); }
        public virtual HQServer createHQServer(GameClient pGame) { return new HQServer(pGame); }

        public virtual MapClient createMapClient(GameClient pGame) { return new MapClient(pGame); }
        public virtual MapServer createMapServer(GameClient pGame) { return new MapServer(pGame); }

        public virtual MarketClient createMarketClient(GameClient pGame) { return new MarketClient(pGame); }
        public virtual MarketServer createMarketServer(GameClient pGame) { return new MarketServer(pGame); }

        public virtual ModuleClient createModuleClient(GameClient pGame) { return new ModuleClient(pGame); }
        public virtual ModuleServer createModuleServer(GameClient pGame) { return new ModuleServer(pGame); }

        public virtual PlayerClient createPlayerClient(GameClient pGame) { return new PlayerClient(pGame); }
        public virtual PlayerServer createPlayerServer(GameClient pGame) { return new PlayerServer(pGame); }

        public virtual StatsClient createStatsClient(GameClient pGame) { return new StatsClient(pGame); }
        public virtual StatsServer createStatsServer(GameClient pGame) { return new StatsServer(pGame); }

        public virtual TileClient createTileClient(GameClient pGame) { return new TileClient(pGame); }
        public virtual TileServer createTileServer(GameClient pGame) { return new TileServer(pGame); }

        public virtual TileGroupServer createTileGroupServer(GameServer pGame) { return new TileGroupServer(pGame); }

        public virtual UnitClient createUnitClient(GameClient pGame) { return new UnitClient(pGame); }
        public virtual UnitServer createUnitServer(GameClient pGame) { return new UnitServer(pGame); }
    }
}