using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using Offworld.SystemCore;

namespace Offworld.GameCore
{
    public interface IGameEventListener
    {
        bool isGameEventAdapterMuted();
        void onSabotageEvent(GameEventSabotage sabotageEvent);
        void onSabotageReversalEvent(GameEventSabotageReversal sabotageReversalEvent);
        void onMessageEvent(GameEventMessage messageEvent);
        void onSteamStatEvent(GameEventSteamStat steamStatEvent);
        void onDataEvent(GameEventData dataEvent);
    }

    public class GameEventAdapter : IGameEventListener
    {
        protected bool gameEventAdapterMuted = false;
        public virtual bool isGameEventAdapterMuted() { return gameEventAdapterMuted; }
        public virtual void onSabotageEvent(GameEventSabotage sabotageEvent) { }
        public virtual void onSabotageReversalEvent(GameEventSabotageReversal sabotageReversalEvent) { }
        public virtual void onMessageEvent(GameEventMessage messageEvent) { }
        public virtual void onSteamStatEvent(GameEventSteamStat steamStatEvent) { }
        public virtual void onDataEvent(GameEventData dataEvent) { }
    }

    public class GameEventsClient
    {
        public enum GameEventsDirtyType
        {
            FIRST,

            SabotageEvents,
            SabotageReversalEvents,
            MessageEvents,
            SteamStatEvents,
            DataEvents,

            NUM_TYPES
        }

        public enum DataType
        {
            Achievement,
            Auction,
            Ping,
            Scan,
            TileDiscovered,
            HQFound,
            HQUpgrade,
            HQUpgradeBonus,
            HQFoundResources,
            ConstructionPlaced,
            ConstructionStarted,
            ConstructionKilled,
            BuildingConstructed,
            BuildingKilled,
            BuildingFree,
            UnitCreated,
            UnitKilled,
            ClaimFinish,
            Plunder,
            HologramRevealed,
            PirateDestroyed,
            ResourceTraded,
            OrderComplete,
            Patent,
            PatentLicensed,
            PatentSent,
            BlackMarketOpen,
            StockPurchase,
            StockSold,
            BuyoutHighestTenth,
            ColonyStock,
            ColonyModule,
            SupplyWholesale,
            NewDay,
            Eclipse,
            NewColonists,
            NewResource,
            NewGeothermal,
            SpeedChange,
            BuyoutClaims,
            Subsidiary,
            PlayerWinLose,
            PlayerWinScreen,
            EventGame,
            EventGameDelay,
            PlayerExited,
            LostCommsWithPlayer,
            MajorityBuyoutVulnerable,
            ResourceDiminished,
            BuildingDestroyed,
            TerrainChange,
            Import,

            NUM_TYPES
        }

        protected BitMask mDirtyBits = new BitMask();
        protected virtual bool isDirty(GameEventsDirtyType eType)
        {
            return mDirtyBits.GetBit((int)eType);
        }
        public virtual bool isAnyDirty()
        {
            return !mDirtyBits.IsEmpty();
        }

        protected List<GameEventSabotage> maSabotageEvents = new List<GameEventSabotage>();
        protected List<GameEventSabotageReversal> maSabotageReveralEvents = new List<GameEventSabotageReversal>();
        protected List<GameEventMessage> maMessageEvents = new List<GameEventMessage>();
        protected List<GameEventSteamStat> maSteamStatEvents = new List<GameEventSteamStat>();
        protected List<GameEventData> maDataEvents = new List<GameEventData>();

        protected List<IGameEventListener> maGameEventListeners = new List<IGameEventListener>();

        public virtual void writeValues(BinaryWriter writer, bool bAll)
        {
            using (new UnityProfileScope("GameEventsClient::writeValues"))
            {
                SimplifyIO.Data(writer, ref mDirtyBits, "DirtyBits");

                if (isDirty(GameEventsDirtyType.SabotageEvents) || bAll)
                {
                    writer.Write7BitInt(maSabotageEvents.Count);
                    foreach (GameEventSabotage sabotageEvent in maSabotageEvents)
                        sabotageEvent.Serialize(writer);
                    maSabotageEvents.Clear();
                }

                if (isDirty(GameEventsDirtyType.SabotageReversalEvents) || bAll)
                {
                    writer.Write7BitInt(maSabotageReveralEvents.Count);
                    foreach (GameEventSabotageReversal sabotageReversalEvent in maSabotageReveralEvents)
                        sabotageReversalEvent.Serialize(writer);
                    maSabotageReveralEvents.Clear();
                }

                if (isDirty(GameEventsDirtyType.MessageEvents) || bAll)
                {
                    writer.Write7BitInt(maMessageEvents.Count);
                    foreach (GameEventMessage messageEvent in maMessageEvents)
                        messageEvent.Serialize(writer);
                    maMessageEvents.Clear();
                }

                if (isDirty(GameEventsDirtyType.SteamStatEvents) || bAll)
                {
                    writer.Write7BitInt(maSteamStatEvents.Count);
                    foreach (GameEventSteamStat steamStatEvent in maSteamStatEvents)
                        steamStatEvent.Serialize(writer);
                    maSteamStatEvents.Clear();
                }

                if (isDirty(GameEventsDirtyType.DataEvents) || bAll)
                {
                    writer.Write7BitInt(maDataEvents.Count);
                    foreach (GameEventData newDataEvent in maDataEvents)
                        newDataEvent.Serialize(writer);
                    maDataEvents.Clear();
                }
            }
        }


        public virtual void readValues(BinaryReader reader, GameClient pGame, bool bAll)
        {
            using (new UnityProfileScope("GameEventsClient::readValues"))
            {

                SimplifyIO.Data(reader, ref mDirtyBits, "DirtyBits");

                List<IGameEventListener> activeListeners = new List<IGameEventListener>();

                foreach(IGameEventListener listener in maGameEventListeners)
                {
                    if (!listener.isGameEventAdapterMuted())
                        activeListeners.Add(listener);
                }

                if (isDirty(GameEventsDirtyType.SabotageEvents) || bAll)
                {
                    int iNum = reader.Read7BitInt();
                    for (int i = 0; i < iNum; i++)
                    {
                        GameEventSabotage sabotageEvent = new GameEventSabotage();
                        sabotageEvent.Serialize(reader);
                        foreach (IGameEventListener listener in activeListeners)
                        {
                            listener.onSabotageEvent(sabotageEvent);
                        }
                    }
                }

                if (isDirty(GameEventsDirtyType.SabotageReversalEvents) || bAll)
                {
                    int iNum = reader.Read7BitInt();
                    for (int i = 0; i < iNum; i++)
                    {
                        GameEventSabotageReversal sabotageReversalEvent = new GameEventSabotageReversal();
                        sabotageReversalEvent.Serialize(reader);
                        foreach (IGameEventListener listener in activeListeners)
                        {
                            listener.onSabotageReversalEvent(sabotageReversalEvent);
                        }
                    }
                }

                if (isDirty(GameEventsDirtyType.MessageEvents) || bAll)
                {
                    int iNum = reader.Read7BitInt();
                    for (int i = 0; i < iNum; i++)
                    {
                        GameEventMessage messageEvent = new GameEventMessage();
                        messageEvent.Serialize(reader);
                        foreach (IGameEventListener listener in activeListeners)
                        {
                            listener.onMessageEvent(messageEvent);
                        }
                    }
                }

                if (isDirty(GameEventsDirtyType.SteamStatEvents) || bAll)
                {
                    int iNum = reader.Read7BitInt();
                    for (int i = 0; i < iNum; i++)
                    {
                        GameEventSteamStat steamStatEvent = new GameEventSteamStat();
                        steamStatEvent.Serialize(reader);
                        foreach (IGameEventListener listener in activeListeners)
                        {
                            listener.onSteamStatEvent(steamStatEvent);
                        }
                    }
                }

                if (isDirty(GameEventsDirtyType.DataEvents) || bAll)
                {
                    int iNum = reader.Read7BitInt();
                    for (int i = 0; i < iNum; i++)
                    {
                        GameEventData dataEvent = new GameEventData();
                        dataEvent.Serialize(reader);

                        if (dataEvent.meType == DataType.ConstructionKilled)
                        {
                            pGame.getConstructionDictionary().Remove(dataEvent.maiData[0]);
                        }
                        else if (dataEvent.meType == DataType.BuildingKilled)
                        {
                            pGame.getBuildingDictionary().Remove(dataEvent.maiData[0]);
                        }
                        else if (dataEvent.meType == DataType.UnitKilled)
                        {
                            pGame.getUnitDictionary().Remove(dataEvent.maiData[0]);
                        }

                        foreach (IGameEventListener listener in activeListeners)
                        {
                            listener.onDataEvent(dataEvent);
                        }
                    }
                }
            }
        }

        public virtual void addGameEventListener(IGameEventListener gameEventListener)
        {
            if(maGameEventListeners.Contains(gameEventListener))
                Debug.LogWarning("[GameEvents] Adding same listener multiple times!");
            else
                maGameEventListeners.Add(gameEventListener);
        }
    }

    public class GameEventsServer : GameEventsClient
    {
        protected virtual void makeDirty(GameEventsDirtyType eType)
        {
            mDirtyBits.SetBit((int)eType, true);
        }
        public virtual void clearDirty()
        {
            mDirtyBits.Clear();
        }

        public virtual void AddSabotageEvent(TileServer pTile, PlayerType perpetrator, PlayerType originalOwner, List<int> affectedTiles, List<int> affectedUnits, List<int> frozenTimes, HashSet<PlayerType> targetedPlayers, SabotageType eSabotage)
        {
            maSabotageEvents.Add(new GameEventSabotage(eSabotage, pTile.getID(), perpetrator, originalOwner, affectedTiles, affectedUnits, frozenTimes, targetedPlayers));
            makeDirty(GameEventsDirtyType.SabotageEvents);
        }

        public virtual void AddSabotageReversalEvent(SabotageType sabotageReversal, SabotageType sabotageAttempted, int tileID, PlayerType attackingPlayer, PlayerType defendingPlayer)
        {
            maSabotageReveralEvents.Add(new GameEventSabotageReversal(sabotageReversal, sabotageAttempted, tileID, attackingPlayer, defendingPlayer));
            makeDirty(GameEventsDirtyType.SabotageReversalEvents);
        }

        public virtual void AddMessageEvent(PlayerType eFromPlayer, PlayerType eToPlayer, TextType eText)
        {
            maMessageEvents.Add(new GameEventMessage(eFromPlayer, eToPlayer, eText));
            makeDirty(GameEventsDirtyType.MessageEvents);
        }

        public void AddSteamStat(PlayerServer pPlayer, string zStat)
        {
            if (pPlayer.isHuman())
            {
                maSteamStatEvents.Add(new GameEventSteamStat(pPlayer.getPlayer(), zStat));
                makeDirty(GameEventsDirtyType.SteamStatEvents);
            }
        }

        public virtual void AddDataEvent(DataType eType, PlayerType ePlayer, List<int> aiData)
        {
            maDataEvents.Add(new GameEventData(eType, ePlayer, aiData));
            makeDirty(GameEventsDirtyType.DataEvents);
        }

        public virtual void AddAchievement(PlayerType ePlayer, AchievementType eAchievement)
        {
            List<int> aiData = new List<int>();
            aiData.Add((int)eAchievement);

            AddDataEvent(GameEventsClient.DataType.Achievement, ePlayer, aiData);
        }

        public virtual void AddAuction(AuctionType eType, AuctionState eState, PlayerType eWinningPlayer, int iMoneyAmount, int iData1, int iData2)
        {
            List<int> aiData = new List<int>();
            aiData.Add((int)eType);
            aiData.Add((int)eState);
            aiData.Add((int)eWinningPlayer);
            aiData.Add(iMoneyAmount);
            aiData.Add(iData1);
            aiData.Add(iData2);

            AddDataEvent(GameEventsClient.DataType.Auction, PlayerType.NONE, aiData);
        }

        public virtual void AddPing(PlayerType ePlayer, int iTileID)
        {
            List<int> aiData = new List<int>();
            aiData.Add(iTileID);

            AddDataEvent(GameEventsClient.DataType.Ping, ePlayer, aiData);
        }

        public virtual void AddScan(PlayerType ePlayer, int iTileID)
        {
            List<int> aiData = new List<int>();
            aiData.Add(iTileID);

            AddDataEvent(GameEventsClient.DataType.Scan, ePlayer, aiData);
        }

        public virtual void AddTileDiscovered(PlayerType ePlayer, ResourceType eResource, ResourceLevelType eResourceLevel, bool bGeothermal)
        {
            List<int> aiData = new List<int>();
            aiData.Add((int)eResource);
            aiData.Add((int)eResourceLevel);
            aiData.Add((bGeothermal) ? 1 : 0);

            AddDataEvent(GameEventsClient.DataType.TileDiscovered, ePlayer, aiData);
        }

        public virtual void AddHQFound(PlayerType ePlayer, int iHQID, List<int> aiResourceAmounts)
        {
            List<int> aiData = new List<int>();
            aiData.Add(iHQID);
            foreach (int i in aiResourceAmounts)
            {
                aiData.Add(i);
            }

            AddDataEvent(GameEventsClient.DataType.HQFound, ePlayer, aiData);
        }

        public virtual void AddHQUpgrade(PlayerType ePlayer, HQLevelType eHQLevel)
        {
            List<int> aiData = new List<int>();
            aiData.Add((int)eHQLevel);

            AddDataEvent(GameEventsClient.DataType.HQUpgrade, ePlayer, aiData);
        }

        public virtual void AddHQUpgradeBonus(PlayerType ePlayer, ResourceType eResource, int iQuantity, int iTileID)
        {
            List<int> aiData = new List<int>();
            aiData.Add((int)eResource);
            aiData.Add(iQuantity);
            aiData.Add(iTileID);

            AddDataEvent(GameEventsClient.DataType.HQUpgradeBonus, ePlayer, aiData);
        }

        public virtual void AddHQFoundResources(int iHQID, int iTileID, List<int> aiResourceAmounts)
        {
            List<int> aiData = new List<int>();
            aiData.Add(iHQID);
            aiData.Add(iTileID);
            foreach (int i in aiResourceAmounts)
            {
                aiData.Add(i);
            }

            AddDataEvent(GameEventsClient.DataType.HQFoundResources, PlayerType.NONE, aiData);
        }

        public virtual void AddConstructionPlaced(PlayerType ePlayer, BuildingType eBuilding, int iTileID)
        {
            List<int> aiData = new List<int>();
            aiData.Add((int)eBuilding);
            aiData.Add(iTileID);

            AddDataEvent(GameEventsClient.DataType.ConstructionPlaced, ePlayer, aiData);
        }

        public virtual void AddConstructionStarted(PlayerType ePlayer, BuildingType eBuilding, int iTileID)
        {
            List<int> aiData = new List<int>();
            aiData.Add((int)eBuilding);
            aiData.Add(iTileID);

            AddDataEvent(GameEventsClient.DataType.ConstructionStarted, ePlayer, aiData);
        }

        public virtual void AddConstructionKilled(int iConstructionID)
        {
            List<int> aiData = new List<int>();
            aiData.Add(iConstructionID);

            AddDataEvent(GameEventsClient.DataType.ConstructionKilled, PlayerType.NONE, aiData);
        }

        public virtual void AddBuildingConstructed(PlayerType ePlayer, BuildingType eBuilding, int iTileID, bool firstOfClassConstructed)
        {
            List<int> aiData = new List<int>();
            aiData.Add((int)eBuilding);
            aiData.Add(iTileID);
            aiData.Add((firstOfClassConstructed) ? 1 : 0);

            AddDataEvent(GameEventsClient.DataType.BuildingConstructed, ePlayer, aiData);
        }

        public virtual void AddBuildingKilled(int iBuildingID)
        {
            List<int> aiData = new List<int>();
            aiData.Add(iBuildingID);

            AddDataEvent(GameEventsClient.DataType.BuildingKilled, PlayerType.NONE, aiData);
        }

        public virtual void AddBuildingFree(int iBuildingID)
        {
            List<int> aiData = new List<int>();
            aiData.Add(iBuildingID);

            AddDataEvent(GameEventsClient.DataType.BuildingFree, PlayerType.NONE, aiData);
        }

        public virtual void AddUnitCreated(int iUnitID, int iTileID)
        {
            List<int> aiData = new List<int>();
            aiData.Add(iUnitID);
            aiData.Add(iTileID);

            AddDataEvent(GameEventsClient.DataType.UnitCreated, PlayerType.NONE, aiData);
        }

        public virtual void AddUnitKilled(int iUnitID, UnitType eUnit, MissionType eMission, bool missionCompleted)
        {
            List<int> aiData = new List<int>();
            aiData.Add(iUnitID);
            aiData.Add((int)eUnit);
            aiData.Add((int)eMission);
            aiData.Add((missionCompleted) ? 1 : 0);

            AddDataEvent(GameEventsClient.DataType.UnitKilled, PlayerType.NONE, aiData);
        }

        public virtual void AddClaimFinish(PlayerType ePlayer, int iTileID)
        {
            List<int> aiData = new List<int>();
            aiData.Add(iTileID);

            AddDataEvent(GameEventsClient.DataType.ClaimFinish, ePlayer, aiData);
        }

        public virtual void AddPlunder(PlayerType ePlayer, PlayerType eAttackedPlayer, ResourceType eResource, int iQuantity, int iTileID, int iUnitID, int iAffectedUnitID, bool bFirst)
        {
            List<int> aiData = new List<int>();
            aiData.Add((int)eAttackedPlayer);
            aiData.Add((int)eResource);
            aiData.Add(iQuantity);
            aiData.Add(iTileID);
            aiData.Add(iUnitID);
            aiData.Add(iAffectedUnitID);
            aiData.Add((bFirst) ? 1 : 0);

            AddDataEvent(GameEventsClient.DataType.Plunder, ePlayer, aiData);
        }

        public virtual void AddHologramRevealed(PlayerType ePlayer, BuildingType eBuilding, int iTileID)
        {
            List<int> aiData = new List<int>();
            aiData.Add((int)ePlayer);
            aiData.Add((int)eBuilding);
            aiData.Add(iTileID);

            AddDataEvent(GameEventsClient.DataType.HologramRevealed, ePlayer, aiData);
        }

        public virtual void AddPirateDestroyed(PlayerType ePlayer, UnitType eUnit, int iTileID)
        {
            List<int> aiData = new List<int>();
            aiData.Add((int)ePlayer);
            aiData.Add((int)eUnit);
            aiData.Add(iTileID);

            AddDataEvent(GameEventsClient.DataType.PirateDestroyed, ePlayer, aiData);
        }

        public virtual void AddResourceTraded(PlayerType ePlayer, ResourceType eResource, int iAmount, int iMoneyValue, bool bFailure, bool bManual)
        {
            List<int> aiData = new List<int>();
            aiData.Add((int)eResource);
            aiData.Add(iAmount);
            aiData.Add(iMoneyValue);
            aiData.Add((bFailure) ? 1 : 0);
            aiData.Add((bManual) ? 1 : 0);

            AddDataEvent(GameEventsClient.DataType.ResourceTraded, ePlayer, aiData);
        }

        public virtual void AddOrderComplete(PlayerType ePlayer, OrderType eOrder, int iData1, int iData2, int iBuildingID)
        {
            List<int> aiData = new List<int>();
            aiData.Add((int)eOrder);
            aiData.Add(iData1);
            aiData.Add(iData2);
            aiData.Add(iBuildingID);

            AddDataEvent(GameEventsClient.DataType.OrderComplete, ePlayer, aiData);
        }

        public virtual void AddPatent(PlayerType ePlayer, PatentType ePatent, List<bool> abPatentStarted)
        {
            List<int> aiData = new List<int>();
            aiData.Add((int)ePatent);
            foreach (bool b in abPatentStarted)
            {
                aiData.Add((b) ? 1 : 0);
            }

            AddDataEvent(GameEventsClient.DataType.Patent, ePlayer, aiData);
        }

        public virtual void AddPatentLicense(PlayerType ePlayer, PlayerType eLicensee, PatentType ePatent, ResourceType eResource, int iQuantity)
        {
            List<int> aiData = new List<int>();
            aiData.Add((int)eLicensee);
            aiData.Add((int)ePatent);
            aiData.Add((int)eResource);
            aiData.Add(iQuantity);

            AddDataEvent(GameEventsClient.DataType.PatentLicensed, ePlayer, aiData);
        }

        public virtual void AddPatentSent(PlayerType ePlayer, PlayerType eToPlayer, PatentType ePatent)
        {
            List<int> aiData = new List<int>();
            aiData.Add((int)eToPlayer);
            aiData.Add((int)ePatent);

            AddDataEvent(GameEventsClient.DataType.PatentSent, ePlayer, aiData);
        }

        public virtual void AddBlackMarketOpen(PlayerType ePlayer)
        {
            AddDataEvent(GameEventsClient.DataType.BlackMarketOpen, ePlayer, null);
        }

        public virtual void AddStockPurchase(PlayerType eBuyingPlayer, PlayerType eBoughtPlayer, int iNumShares)
        {
            List<int> aiData = new List<int>();
            aiData.Add((int)eBoughtPlayer);
            aiData.Add(iNumShares);

            AddDataEvent(GameEventsClient.DataType.StockPurchase, eBuyingPlayer, aiData);
        }

        public virtual void AddStockSold(PlayerType eSellingPlayer, PlayerType eSoldPlayer, int iNumShares)
        {
            List<int> aiData = new List<int>();
            aiData.Add((int)eSoldPlayer);
            aiData.Add(iNumShares);

            AddDataEvent(GameEventsClient.DataType.StockSold, eSellingPlayer, aiData);
        }

        public virtual void AddBuyoutHighestTenth(PlayerType eTargetPlayer, PlayerType eBuyingPlayer, int iTenths)
        {
            List<int> aiData = new List<int>();
            aiData.Add((int)eBuyingPlayer);
            aiData.Add(iTenths);

            AddDataEvent(GameEventsClient.DataType.BuyoutHighestTenth, eTargetPlayer, aiData);
        }

        public virtual void AddColonyStock(PlayerType ePlayer)
        {
            AddDataEvent(GameEventsClient.DataType.ColonyStock, ePlayer, null);
        }

        public virtual void AddColonyModule(PlayerType ePlayer, ModuleType eModule, int iTileID)
        {
            List<int> aiData = new List<int>();
            aiData.Add((int)eModule);
            aiData.Add(iTileID);

            AddDataEvent(GameEventsClient.DataType.ColonyModule, ePlayer, aiData);
        }

        public virtual void AddSupplyWholesale(PlayerType ePlayer, ResourceType eResource, int iShipment)
        {
            List<int> aiData = new List<int>();
            aiData.Add((int)eResource);
            aiData.Add(iShipment);

            AddDataEvent(GameEventsClient.DataType.SupplyWholesale, ePlayer, aiData);
        }

        public virtual void AddNewDay(PlayerType ePlayer, int iDebtPercent, int iDebtMoney, int iDay, List<int> aiOtherPlayersInterestAward)
        {
            List<int> aiData = new List<int>();
            aiData.Add(iDebtPercent);
            aiData.Add(iDebtMoney);
            aiData.Add(iDay);
            foreach (int i in aiOtherPlayersInterestAward)
            {
                aiData.Add(i);
            }

            AddDataEvent(GameEventsClient.DataType.NewDay, ePlayer, aiData);
        }

        public void AddDebtNotification(PlayerType ePlayer, int iDebtPercent, int iDebtMoney, List<int> aiOtherPlayersInterestAward)
        {
            List<int> aiData = new List<int>();
            aiData.Add(iDebtPercent);
            aiData.Add(iDebtMoney);
            aiData.Add(0);
            foreach (int i in aiOtherPlayersInterestAward)
            {
                aiData.Add(i);
            }

            AddDataEvent(GameEventsClient.DataType.NewDay, ePlayer, aiData);
        }

        public virtual void AddTerrainChange(int tileID)
        {
            AddDataEvent(GameEventsClient.DataType.TerrainChange, PlayerType.NONE, new List<int> { tileID });
        }
        
       public virtual void AddEclipse()
        {
            AddDataEvent(GameEventsClient.DataType.Eclipse, PlayerType.NONE, null);
        }

        public virtual void AddNewColonists(int iCount, List<int> aiModuleCount)
        {
            List<int> aiData = new List<int>();
            aiData.Add(iCount);
            foreach (int i in aiModuleCount)
            {
                aiData.Add(i);
            }

            AddDataEvent(GameEventsClient.DataType.NewColonists, PlayerType.NONE, aiData);
        }

        public virtual void AddNewResource(int tileID, PlayerType ePlayer, List<ResourceType> aeResourcesChanged)
        {
            List<int> aiData = new List<int>();
            aiData.Add(tileID);
            aiData.AddRange(aeResourcesChanged.Select(x => (int)x));
            AddDataEvent(GameEventsClient.DataType.NewResource, ePlayer, aiData);
        }

        public virtual void AddSpeedChange(GameSpeedType ePrevSpeed, GameSpeedType eNewSpeed)
        {
            List<int> aiData = new List<int>();
            aiData.Add((int)ePrevSpeed);
            aiData.Add((int)eNewSpeed);

            AddDataEvent(GameEventsClient.DataType.SpeedChange, PlayerType.NONE, aiData);
        }

        public virtual void AddBuyoutClaims(PlayerType ePlayer, PlayerType eBoughtPlayer, int iClaims)
        {
            List<int> aiData = new List<int>();
            aiData.Add((int)eBoughtPlayer);
            aiData.Add(iClaims);

            AddDataEvent(GameEventsClient.DataType.BuyoutClaims, ePlayer, aiData);
        }

        public virtual void AddSubsidiary(PlayerType ePlayer, PlayerType eBuyingPlayer, List<int> aiStockRefund)
        {
            List<int> aiData = new List<int>();
            aiData.Add((int)eBuyingPlayer);
            foreach (int i in aiStockRefund)
            {
                aiData.Add(i);
            }

            AddDataEvent(GameEventsClient.DataType.Subsidiary, ePlayer, aiData);
        }

        public virtual void AddPlayerWinLose(PlayerType ePlayer, bool bHasWon, int iBuyoutAmount)
        {
            List<int> aiData = new List<int>();
            aiData.Add((bHasWon) ? 1 : 0);
            aiData.Add(iBuyoutAmount);

            AddDataEvent(GameEventsClient.DataType.PlayerWinLose, ePlayer, aiData);
        }

        public virtual void AddPlayerWinScreen(PlayerType ePlayer)
        {
            AddDataEvent(GameEventsClient.DataType.PlayerWinScreen, ePlayer, null);
        }

        public virtual void AddEventGame(EventGameType eEventGame)
        {
            List<int> aiData = new List<int>();
            aiData.Add((int)eEventGame);

            AddDataEvent(GameEventsClient.DataType.EventGame, PlayerType.NONE, aiData);
        }

        public virtual void AddEventGameDelay(PlayerType ePlayer, EventGameType eEventGame)
        {
            List<int> aiData = new List<int>();
            aiData.Add((int)eEventGame);

            AddDataEvent(GameEventsClient.DataType.EventGameDelay, ePlayer, aiData);
        }

        public virtual void AddPlayerExited(PlayerType ePlayer)
        {
            AddDataEvent(GameEventsClient.DataType.PlayerExited, ePlayer, null);
        }

        public virtual void AddLostCommsWithPlayer(PlayerType ePlayer)
        {
            AddDataEvent(GameEventsClient.DataType.LostCommsWithPlayer, ePlayer, null);
        }

        public virtual void AddMajorityBuyoutVulnerable(PlayerType ePlayer)
        {
            AddDataEvent(GameEventsClient.DataType.MajorityBuyoutVulnerable, ePlayer, null);
        }

        public virtual void AddResourceDiminished(int iTileID, ResourceType eResource)
        {
            List<int> aiData = new List<int>();
            aiData.Add(iTileID);
            aiData.Add((int)eResource);

            AddDataEvent(GameEventsClient.DataType.ResourceDiminished, PlayerType.NONE, aiData);
        }

        public virtual void AddBuildingDestroyed(PlayerType ePlayer, int iTileID, BuildingType eBuilding)
        {
            List<int> aiData = new List<int>();
            aiData.Add(iTileID);
            aiData.Add((int)eBuilding);

            AddDataEvent(GameEventsClient.DataType.BuildingDestroyed, ePlayer, aiData);
        }
    }
}