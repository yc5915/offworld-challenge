using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Offworld.GameCore;

namespace Offworld.AppCore
{
    public struct GoalDisplay
    {
        public string goalText;
        public string goalProgress;
        public string goalTooltip;
        public bool isComplete;
        public bool isSeparator;

        public GoalDisplay(string text, string progress, string tooltip, bool completeState)
        {
            goalText = text;
            goalProgress = progress;
            goalTooltip = tooltip;
            isComplete = completeState;
            isSeparator = false;
        }

        public GoalDisplay(string text, string progress, string tooltip, bool completeState, bool separator)
        {
            goalText = text;
            goalProgress = progress;
            goalTooltip = tooltip;
            isComplete = completeState;
            isSeparator = separator;
        }
    }

    public interface IUIEventListener
    {
        void onCannotFoundEvent(UIEventCannotFound cannotFoundEvent);
        void onCannotConstructEvent(UIEventCannotConstruct cannotConstructEvent);
        void onCannotUpgradeEvent(UIEventCannotUpgrade cannotUpgradeEvent);
        void onFailedTradeEvent(UIEventFailedTrade failedTradeEvent);
        void onFailedBlackMarketEvent(UIEventFailedBlackMarket failedBlackMarketEvent);
        void onFailedStockTradeEvent(UIEventFailedStockTrade failedStockTradeEvent);
        void onFailedLaunchEvent(UIEventFailedLaunch failedLaunchEvent);
        void onFailedHackEvent(UIEventFailedHack failedHackEvent);
        void onPayDebtEvent(UIEventPayDebt payDebtEvent);
    }

    public class UIEventAdapter : IUIEventListener
    {
        public virtual void onCannotFoundEvent(UIEventCannotFound cannotFoundEvent) { }
        public virtual void onCannotConstructEvent(UIEventCannotConstruct cannotConstructEvent) { }
        public virtual void onFailedTradeEvent(UIEventFailedTrade failedTradeEvent) { }
        public virtual void onFailedBlackMarketEvent(UIEventFailedBlackMarket failedBlackMarketEvent) { }
        public virtual void onCannotUpgradeEvent(UIEventCannotUpgrade cannotUpgradeEvent) { }
        public virtual void onFailedStockTradeEvent(UIEventFailedStockTrade failedStockTradeEvent) { }
        public virtual void onFailedLaunchEvent(UIEventFailedLaunch failedLaunchEvent) { }
        public virtual void onFailedHackEvent(UIEventFailedHack failedHackEvent) { }
        public virtual void onPayDebtEvent(UIEventPayDebt payDebtEvent) { }
    }

    public class UIEventManager
    {
        protected List<IUIEventListener> maUIEventListeners = new List<IUIEventListener>();

        protected List<UIEventCannotFound> maCannotFoundEvents = new List<UIEventCannotFound>();
        protected List<UIEventCannotConstruct> maCannotConstructEvents = new List<UIEventCannotConstruct>();
        protected List<UIEventCannotUpgrade> maCannotUpgradeEvents = new List<UIEventCannotUpgrade>();
        protected List<UIEventFailedTrade> maFailedTradeEvents = new List<UIEventFailedTrade>();
        protected List<UIEventPayDebt> maPayDebtEvents = new List<UIEventPayDebt>();
        protected List<UIEventFailedBlackMarket> maFailedBlackMarketEvents = new List<UIEventFailedBlackMarket>();
        protected List<UIEventFailedStockTrade> maFailedStockTradeEvents = new List<UIEventFailedStockTrade>();
        protected List<UIEventFailedLaunch> maFailedLaunchEvents = new List<UIEventFailedLaunch>();
        protected List<UIEventFailedHack> maFailedHackEvent = new List<UIEventFailedHack>();

        public UIEventManager()
        {

        }

        public void Update()
        {
            for(int i=0; i < maCannotFoundEvents.Count; i++)
            {
                foreach(IUIEventListener listener in maUIEventListeners)
                    listener.onCannotFoundEvent(maCannotFoundEvents[i]);

                maCannotFoundEvents.Clear();
            }

            for (int i = 0; i < maCannotConstructEvents.Count; i++)
            {
                foreach (IUIEventListener listener in maUIEventListeners)
                    listener.onCannotConstructEvent(maCannotConstructEvents[i]);

                maCannotConstructEvents.Clear();
            }

            for (int i = 0; i < maCannotUpgradeEvents.Count; i++)
            {
                foreach (IUIEventListener listener in maUIEventListeners)
                    listener.onCannotUpgradeEvent(maCannotUpgradeEvents[i]);

                maCannotUpgradeEvents.Clear();
            }

            for (int i = 0; i < maFailedTradeEvents.Count; i++)
            {
                foreach (IUIEventListener listener in maUIEventListeners)
                    listener.onFailedTradeEvent(maFailedTradeEvents[i]);

                maFailedTradeEvents.Clear();
            }

            for (int i = 0; i < maFailedBlackMarketEvents.Count; i++)
            {
                foreach (IUIEventListener listener in maUIEventListeners)
                    listener.onFailedBlackMarketEvent(maFailedBlackMarketEvents[i]);

                maFailedTradeEvents.Clear();
            }

            for (int i = 0; i < maFailedStockTradeEvents.Count; i++)
            {
                foreach (IUIEventListener listener in maUIEventListeners)
                    listener.onFailedStockTradeEvent(maFailedStockTradeEvents[i]);

                maFailedStockTradeEvents.Clear();
            }

            for (int i = 0; i < maFailedLaunchEvents.Count; i++)
            {
                foreach (IUIEventListener listener in maUIEventListeners)
                    listener.onFailedLaunchEvent(maFailedLaunchEvents[i]);

                maFailedLaunchEvents.Clear();
            }

            for (int i = 0; i < maFailedHackEvent.Count; i++)
            {
                foreach (IUIEventListener listener in maUIEventListeners)
                    listener.onFailedHackEvent(maFailedHackEvent[i]);

                maFailedHackEvent.Clear();
            }

            for (int i = 0; i < maPayDebtEvents.Count; i++)
            {
                foreach (IUIEventListener listener in maUIEventListeners)
                    listener.onPayDebtEvent(maPayDebtEvents[i]);

                maPayDebtEvents.Clear();
            }
        }

        public void AddUIEventListener(IUIEventListener uiEventListener)
        {
            maUIEventListeners.Add(uiEventListener);
        }

        public void AddCannotFoundEvent(int tile, HQType hq)
        {
            maCannotFoundEvents.Add(new UIEventCannotFound(tile, hq));
        }
        public void AddCannotConstructEvent(int tile, BuildingType building, int claims, List<int> resourcesNeeded)
        {
            maCannotConstructEvents.Add(new UIEventCannotConstruct(tile, building, claims, resourcesNeeded));
        }

        public void AddCannotUpgradeEvent()
        {
            maCannotUpgradeEvents.Add(new UIEventCannotUpgrade());
        }

        public void AddFailedTradeEvent(PlayerType player, ResourceType resource, int amount, int cost)
        {
            maFailedTradeEvents.Add(new UIEventFailedTrade(player, resource, amount, cost));
        }

        public void AddFailedBlackMarketEvent(PlayerType player, BlackMarketType blackMarket)
        {
            maFailedBlackMarketEvents.Add(new UIEventFailedBlackMarket(player, blackMarket));
        }

        public void AddFailedStockTradeEvent(PlayerType player, PlayerType tradedPlayerPurchase, bool isPurchase)
        {
            maFailedStockTradeEvents.Add(new UIEventFailedStockTrade(player, tradedPlayerPurchase, isPurchase));
        }

        public void AddFailedLaunchEvent(PlayerType player, ResourceType resource)
        {
            maFailedLaunchEvents.Add(new UIEventFailedLaunch(player, resource));
        }

        public void AddFailedHackEvent(PlayerType player, EspionageType espionage)
        {
            maFailedHackEvent.Add(new UIEventFailedHack(player, espionage));
        }

        public void AddPayDebtEvent(PlayerType player, bool isSuccess, int amount)
        {
            maPayDebtEvents.Add(new UIEventPayDebt(player, isSuccess, amount));
        }
    }

    public class UIEventCannotFound
    {
        public int tileID;
        public HQType hqType;

        public UIEventCannotFound() { }
        public UIEventCannotFound(int tile, HQType hq)
        {
            tileID = tile;
            hqType = hq;
        }
    }

    public class UIEventCannotConstruct
    {
        public int tileID;
        public BuildingType buildingType;
        public int playerClaims;
        public List<int> resourcesNeeded;

        public UIEventCannotConstruct() { }
        public UIEventCannotConstruct(int tile, BuildingType building, int claims, List<int> resources)
        {
            tileID = tile;
            buildingType = building;
            playerClaims = claims;
            resourcesNeeded = resources;
        }
    }

    public class UIEventCannotUpgrade
    {
    }

    public class UIEventFailedTrade
    {
        public PlayerType mePlayer;
        public ResourceType meResource;
        public int miAmount;
        public int miCost;

        public UIEventFailedTrade() { }
        public UIEventFailedTrade(PlayerType player, ResourceType resource, int amount, int cost)
        {
            mePlayer = player;
            meResource = resource;
            miAmount = amount;
            miCost = cost;
        }
    }

    public class UIEventFailedBlackMarket
    {
        public PlayerType mePlayer;
        public BlackMarketType meBlackMarket;

        public UIEventFailedBlackMarket() { }
        public UIEventFailedBlackMarket(PlayerType player, BlackMarketType blackMarket)
        {
            mePlayer = player;
            meBlackMarket = blackMarket;
        }
    }

    public class UIEventFailedStockTrade
    {
        public PlayerType mePlayer;
        public PlayerType meTradedPlayer;
        public bool mbPurchase;

        public UIEventFailedStockTrade() { }
        public UIEventFailedStockTrade(PlayerType player, PlayerType tradedPlayer, bool isPurchase)
        {
            mePlayer = player;
            meTradedPlayer = tradedPlayer;
            mbPurchase = isPurchase;
        }
    }

    public class UIEventFailedHack
    {
        public PlayerType mePlayer;
        public EspionageType meEspionage;

        public UIEventFailedHack() { }
        public UIEventFailedHack(PlayerType player, EspionageType espionage)
        {
            mePlayer = player;
            meEspionage = espionage;
        }
    }
    public class UIEventFailedLaunch
    {
        public PlayerType mePlayer;
        public ResourceType meResource;

        public UIEventFailedLaunch() { }
        public UIEventFailedLaunch(PlayerType player, ResourceType resource)
        {
            mePlayer = player;
            meResource = resource;
        }
    }

    public class UIEventPayDebt
    {
        public PlayerType mePlayer;
        public bool bSuccess;
        public int miAmount;

        public UIEventPayDebt(PlayerType player, bool isSuccess, int amount)
        {
            mePlayer = player;
            bSuccess = isSuccess;
            miAmount = amount;
        }
    }
}
