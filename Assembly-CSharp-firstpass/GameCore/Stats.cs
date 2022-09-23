using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Offworld.SystemCore;

namespace Offworld.GameCore
{
    public enum ResourceStatType
    {
        TRADE_PURCHASE,
        AUTO_TRADE_PURCHASE,
        TRADE_SOLD,
        AUTO_TRADE_SOLD,
        OFFWORLD_SOLD,
        PRODUCED,
        IMPORTS_RECEIVED,
        CONSTRUCTION_CONSUMED,
        IMPORTS_CONSUMED,
        UPGRADE_CONSUMED,
        PATENT_CONSUMED,
        RESEARCH_CONSUMED,
        BUILDING_CONSUMED,
        LIFE_SUPPORT_CONSUMED,
        LAUNCHED,
        SHORTAGES_STARTED,
        SURPLUSES_STARTED,
        SHORTAGES_SPENT,
        SURPLUSES_SPENT,

        NUM_TYPES
    }

    public enum BuildingStatType
    {
        CONSTRUCTED,
        ENTERTAINMENT_REVENUE,

        NUM_TYPES
    }

    public enum BlackMarketStatType
    {
        PURCHASED,
        SPENT,

        NUM_TYPES
    }

    public enum TechnologyStatType
    {
        RESEARCHED,

        NUM_TYPES
    }

    public enum PatentStatType
    {
        ACQUIRED,

        NUM_TYPES
    }

    public enum SabotagingStatType
    {
        TARGET,

        NUM_TYPES
    }

    public enum SabotagedStatType
    {
        ATTACKED,
        DEFENDED,
        CAUGHT,

        NUM_TYPES
    }

    public enum StockStatType
    {
        PURCHASED,
        SOLD,

        NUM_TYPES
    }

    public enum MiscellaneousStatType
    {
        STARTING_MONEY,
        STARTING_DEBT,
        ENDING_MONEY,
        ENDING_DEBT,
        ENDING_RESOURCES,
        PATENT,
        IMPORT_SPENT,
        AUCTION_PROCEEDS, // from when the player sells their own property
        AUCTION_SPENT, // from when the player spends money on an auction
        INTEREST,
        DIVIDEND,
        COLONY,
        FOUNDING_TURN,

        NUM_TYPES
    }

    public enum StatEventType
    {
        FOUND,
        UPGRADE,
        BUYOUT_MAJORITY,
        BUYOUT_NORMAL,
        BUY_STOCK,
        SELL_STOCK,
        PATENT,
        ESPIONAGE,
        LAUNCH,
        AUCTION,
        BLACK_MARKET,
        SABOTAGE,
        SABOTAGE_CAUGHT,
    }

    public class StatEventData
    {
        public int miType;
        public int miTurn;
        public int miData1;
        public int miData2;
        public int miData3;
        public int miData4;

        public StatEventData(StatEventType eType, int iTurn, int iData1, int iData2, int iData3, int iData4)
        {
            this.miType = (sbyte)eType;
            this.miTurn = iTurn;
            this.miData1 = iData1;
            this.miData2 = iData2;
            this.miData3 = iData3;
            this.miData4 = iData4;
        }
    }

    public class StatsClient
    {
        protected GameClient mGame = null;
        protected virtual GameClient gameClient()
        {
            return mGame;
        }
        protected virtual Infos infos()
        {
            return gameClient().infos();
        }

        public enum StatsDirtyType
        {
            FIRST,

            mStatChanges,
            mStockPriceChanges,
            mResourcePriceChanges,
            mEventChanges,

            NUM_TYPES
        }

        protected BitMask mDirtyBits = new BitMask();
        protected virtual bool isDirty(StatsDirtyType eType)
        {
            return mDirtyBits.GetBit((int)eType);
        }
        public virtual bool isAnyDirty()
        {
            return !(mDirtyBits.IsEmpty());
        }

        protected List<HashSet<int>> mStatChanges = new List<HashSet<int>>();
        public const int GAME_TURNS_PER_SAMPLE = 20;

        // [StatsType][SubStatType][Player][Item]
        protected List<List<List<List<int>>>> miStats = new List<List<List<List<int>>>>();

        // [PlayerType][Time]
        protected List<List<int>> mStockPrices = new List<List<int>>();
        protected List<List<int>> mStockPriceChanges = new List<List<int>>();

        // [ResourceType][Time]
        protected List<List<int>> mResourcePrices = new List<List<int>>();
        protected List<List<int>> mResourcePriceChanges = new List<List<int>>();

        public List<List<int>> StockPrices { get { return mStockPrices; } }
        public List<List<int>> ResourcePrices { get { return mResourcePrices; } }

        protected List<List<StatEventData>> mEvents = new List<List<StatEventData>>();
        protected List<List<StatEventData>> mEventChanges = new List<List<StatEventData>>();

        public List<List<StatEventData>> Events { get { return mEvents; } }

        public StatsClient(GameClient game)
        {
            mGame = game;

            for (StatsType eLoopStats = 0; eLoopStats < StatsType.NUM_TYPES; eLoopStats++)
            {
                mStatChanges.Add(new HashSet<int>());
            }

            // [StatsType][SubStatType][Player][Item]
            for (StatsType eStats = 0; eStats < StatsType.NUM_TYPES; eStats++)
            {
                miStats.Add(new List<List<List<int>>>());
                for (int eSubStat = 0; eSubStat < GetNumSubStats(eStats); eSubStat++)
                {
                    miStats[(int)eStats].Add(new List<List<int>>());
                    miStats[(int)eStats][eSubStat].Resize((int)gameClient().getNumPlayers(), null);
                    for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameClient().getNumPlayers(); eLoopPlayer++)
                        miStats[(int)eStats][eSubStat][(int)eLoopPlayer] = Enumerable.Repeat(0, GetNumItems(eStats)).ToList();
                }
            }

            // [PlayerType][Time]
            mStockPrices = Enumerable.Range(0, (int)mGame.getNumPlayers()).Select(x => new List<int>()).ToList();
            mStockPriceChanges = Enumerable.Range(0, (int)mGame.getNumPlayers()).Select(x => new List<int>()).ToList();

            // [ResourceType][Time]
            mResourcePrices = infos().resources().Select(x => new List<int>()).ToList();
            mResourcePriceChanges = infos().resources().Select(x => new List<int>()).ToList();

            // [ResourceType][Time]
            mEvents = Enumerable.Range(0, (int)mGame.getNumPlayers()).Select(x => new List<StatEventData>()).ToList();
            mEventChanges = Enumerable.Range(0, (int)mGame.getNumPlayers()).Select(x => new List<StatEventData>()).ToList();
        }

        protected virtual bool shouldRecordData()
        {
            return !(mGame.isGameOver());
        }

        public virtual int getStat(StatsType eStats, int eSubStat, PlayerType ePlayer, int iItem)
        {
            // [StatsType][SubStatType][Player][Item]
            return miStats[(int)eStats][eSubStat][(int)ePlayer][iItem];
        }

        public virtual int getStatPercent(StatsType eStats, int eSubStat, PlayerType ePlayer, int iItem)
        {
            int iResult = 0;
            int sum = getStatSum(eStats, eSubStat, ePlayer);
            if (sum != 0)
            {
                iResult = getStat(eStats, eSubStat, ePlayer, iItem) * 100 / sum;
            }

            return iResult;
        }

        public virtual int getStatSum(StatsType eStats, int eSubStat, PlayerType ePlayer)
        {
            // [StatsType][SubStatType][Player][Item]
            return miStats[(int)eStats][eSubStat][(int)ePlayer].Sum();
        }

        public virtual void SerializeClient(object stream, bool bAll)
        {
            SimplifyIO.Data(stream, ref mDirtyBits, "DirtyBits");

            //serialize lists that don't get serialized with dirty bits
            if (bAll)
            {
                SerializeAllStats(stream);
                SimplifyIO.Data(stream, ref mStockPrices, "StockPrices");
                SimplifyIO.Data(stream, ref mResourcePrices, "ResourcePrices");
                SimplifyIOGame.StatEventData(stream, ref mEvents);
            }

            // No need for delta information when sending the stats to Tachyon
            if (stream is MJSONwriter)
                return;

            //modify individual stats
            if(isDirty(StatsDirtyType.mStatChanges) || bAll)
            {
                for (StatsType eLoopStats = 0; eLoopStats < StatsType.NUM_TYPES; eLoopStats++)
                {
                    HashSet<int> statChanges = mStatChanges[(int)eLoopStats];
                    SimplifyIO.Data(stream, ref statChanges, "StatChanges");
                }

                // [StatsType][SubStatType][Player][Item]
                for (StatsType eStats = 0; eStats < StatsType.NUM_TYPES; eStats++)
                {
                    foreach (int iItemID in mStatChanges[(int)eStats])
                    {
                        int eSubStat, iItem;
                        PlayerType ePlayer;
                        SplitItemID(eStats, iItemID, out eSubStat, out ePlayer, out iItem);
                        DoIO(stream, eStats, eSubStat, ePlayer, iItem);
                    }

                    mStatChanges[(int)eStats].Clear(); //clear cache
                }
            }

            //append stock prices
            if(isDirty(StatsDirtyType.mStockPriceChanges) || bAll)
            {
                SimplifyIO.Data(stream, ref mStockPriceChanges, "StockPriceChanges");
                for(int i=0; i<mStockPriceChanges.Count; i++)
                {
                    mStockPrices[i].AddRange(mStockPriceChanges[i]);
                    mStockPriceChanges[i].Clear();
                }
            }

            //append resource prices
            if (isDirty(StatsDirtyType.mResourcePriceChanges) || bAll)
            {
                SimplifyIO.Data(stream, ref mResourcePriceChanges, "ResourcePriceChanges");
                for (int i = 0; i < mResourcePriceChanges.Count; i++)
                {
                    mResourcePrices[i].AddRange(mResourcePriceChanges[i]);
                    mResourcePriceChanges[i].Clear();
                }
            }

            //append events
            if (isDirty(StatsDirtyType.mEventChanges) || bAll)
            {
                SimplifyIOGame.StatEventData(stream, ref mEventChanges);
                for (int i = 0; i < mEventChanges.Count; i++)
                {
                    mEvents[i].AddRange(mEventChanges[i]);
                    mEventChanges[i].Clear();
                }
            }
        }

        //**********************************************************************************************
        //  WriteStatsKey
        //**********************************************************************************************
        public void WriteStatsKey(MJSONwriter JSONfile)
        {
            JSONfile.WriteNamedElement("_comment", "\"itemStats[StatsType][SubStatType][Player][Item]\"");
            JSONfile.StartArray("StatsType");
            {
                for (int j = 0; j < (int)StatsType.NUM_TYPES; j++)
                {
                    JSONfile.WriteElement(((StatsType)j).ToString());
                }
            }
            JSONfile.EndArray(true, true);

            JSONfile.StartArray("SubStatType");
            {
                // RESOURCE
                JSONfile.StartArray( StatsType.RESOURCE.ToString() );
                for (int j = 0; j < (int)ResourceStatType.NUM_TYPES; j++)
                {
                    JSONfile.WriteElement(((ResourceStatType)j).ToString());
                }
                JSONfile.EndArray(true, true);

                // BUILDING_CLASS
                JSONfile.StartArray( StatsType.BUILDING_CLASS.ToString() );
                for (int j = 0; j < (int)BuildingStatType.NUM_TYPES; j++)
                {
                    JSONfile.WriteElement(((BuildingStatType)j).ToString());
                }
                JSONfile.EndArray(true, true);

                // BLACK_MARKET
                JSONfile.StartArray( StatsType.BLACK_MARKET.ToString() );
                for (int j = 0; j < (int)BlackMarketStatType.NUM_TYPES; j++)
                {
                    JSONfile.WriteElement(((BlackMarketStatType)j).ToString());
                }
                JSONfile.EndArray(true, true);

                // TECHNOLOGY
                JSONfile.StartArray( StatsType.TECHNOLOGY.ToString() );
                for (int j = 0; j < (int)TechnologyStatType.NUM_TYPES; j++)
                {
                    JSONfile.WriteElement(((TechnologyStatType)j).ToString());
                }
                JSONfile.EndArray(true, true);

                 // PATENT
                JSONfile.StartArray( StatsType.PATENT.ToString() );
                for (int j = 0; j < (int)PatentStatType.NUM_TYPES; j++)
                {
                    JSONfile.WriteElement(((PatentStatType)j).ToString());
                }
                JSONfile.EndArray(true, true);

                // SABOTAGING
                JSONfile.StartArray( StatsType.SABOTAGING.ToString() );
                for (int j = 0; j < (int)SabotagingStatType.NUM_TYPES; j++)
                {
                    JSONfile.WriteElement(((SabotagingStatType)j).ToString());
                }
                JSONfile.EndArray(true, true);

                // SABOTAGED
                JSONfile.StartArray( StatsType.SABOTAGED.ToString() );
                for (int j = 0; j < (int)SabotagedStatType.NUM_TYPES; j++)
                {
                    JSONfile.WriteElement(((SabotagedStatType)j).ToString());
                }
                JSONfile.EndArray(true, true);

               // STOCK
                JSONfile.StartArray( StatsType.STOCK.ToString() );
                for (int j = 0; j < (int)StockStatType.NUM_TYPES; j++)
                {
                    JSONfile.WriteElement(((StockStatType)j).ToString());
                }
                JSONfile.EndArray(true, true);

                 // MISCELLANEOUS
                JSONfile.StartArray( StatsType.MISCELLANEOUS.ToString() );
                for (int j = 0; j < (int)MiscellaneousStatType.NUM_TYPES; j++)
                {
                    JSONfile.WriteElement(((MiscellaneousStatType)j).ToString());
                }
                JSONfile.EndArray(true, true);
            }
            JSONfile.EndArray(false, true);

            JSONfile.StartArray("Items");
            {
                // RESOURCE
                JSONfile.StartArray( StatsType.RESOURCE.ToString() );
                for( int j = 0; j < (int)infos().resourcesNum(); j++ )
                {
                    JSONfile.WriteElement(infos().resources()[j].mzType);
                }
                JSONfile.EndArray(true, true);

                 // BUILDING_CLASS
                JSONfile.StartArray( StatsType.BUILDING_CLASS.ToString() );
                for( int j = 0; j < (int)infos().buildingClassesNum(); j++ )
                {
                    JSONfile.WriteElement(infos().buildingClasses()[j].mzType);
                }
                JSONfile.EndArray(true, true);

                // BLACK_MARKET
                JSONfile.StartArray( StatsType.BLACK_MARKET.ToString() );
                for( int j = 0; j < (int)infos().blackMarketsNum(); j++ )
                {
                    JSONfile.WriteElement(infos().blackMarkets()[j].mzType);
                }
                JSONfile.EndArray(true, true);

                 // TECHNOLOGY
                JSONfile.StartArray( StatsType.TECHNOLOGY.ToString() );
                for( int j = 0; j < (int)infos().technologiesNum(); j++ )
                {
                    JSONfile.WriteElement(infos().technologies()[j].mzType);
                }
                JSONfile.EndArray(true, true);

                // PATENT
                JSONfile.StartArray( StatsType.PATENT.ToString() );
                for( int j = 0; j < (int)infos().patentsNum(); j++ )
                {
                    JSONfile.WriteElement(infos().patents()[j].mzType);
                }
                JSONfile.EndArray(true, true);

                // SABOTAGING
                JSONfile.StartArray( StatsType.SABOTAGING.ToString() );
                for( int j = 0; j < (int)gameClient().getNumPlayers(); j++ )
                {
                    JSONfile.WriteElement( j );
                }
                JSONfile.EndArray(true, true);

                // SABOTAGED
                JSONfile.StartArray( StatsType.SABOTAGED.ToString() );
                for( int j = 0; j < (int)infos().sabotagesNum(); j++ )
                {
                    JSONfile.WriteElement(infos().sabotages()[j].mzType);
                }
                JSONfile.EndArray(true, true);

                // STOCK
                JSONfile.StartArray( StatsType.STOCK.ToString() );
                for (int j = 0; j < (int)gameClient().getNumPlayers(); j++)
                {
                    JSONfile.WriteElement( j );
                }
                JSONfile.EndArray(true, true);

                // MISCELLANEOUS
                JSONfile.StartArray(StatsType.MISCELLANEOUS.ToString());
                for (int j = 0; j < (int)1; j++)
                {
                    JSONfile.WriteElement(j);
                }
                JSONfile.EndArray(true, true);

            }
            JSONfile.EndArray(false, true);
        }

        protected virtual void SerializeAllStats(object stream)
        {
            SimplifyIO.Data(stream, ref miStats, "itemStats");
        }

        protected virtual void DoIO(object stream, StatsType eStats, int eSubStat, PlayerType ePlayer, int iItem)
        {
            // [StatsType][SubStatType][Player][Item]

            if (stream is BinaryWriter)
            {
                int stat = miStats[(int)eStats][eSubStat][(int)ePlayer][iItem];
                SimplifyIO.Data(stream, ref stat, "Stat");
            }
            else if (stream is BinaryReader)
            {
                int iStat = 0;
                SimplifyIO.Data(stream, ref iStat, "Stat");
                miStats[(int)eStats][eSubStat][(int)ePlayer][iItem] = iStat;
            }
        }

        protected virtual int GetNumItems(StatsType eStats)
        {
            switch (eStats)
            {
                case StatsType.RESOURCE:        return (int)infos().resourcesNum();
                case StatsType.BUILDING_CLASS:  return (int)infos().buildingClassesNum();
                case StatsType.BLACK_MARKET:    return (int)infos().blackMarketsNum();
                case StatsType.TECHNOLOGY:      return (int)infos().technologiesNum();
                case StatsType.PATENT:          return (int)infos().patentsNum();
                case StatsType.SABOTAGING:      return (int)gameClient().getNumPlayers();
                case StatsType.SABOTAGED:       return (int)infos().sabotagesNum();
                case StatsType.STOCK:           return (int)gameClient().getNumPlayers();
                case StatsType.MISCELLANEOUS:   return (int)1;
                default:    MAssert.Unimplemented(); return 1;
            }
        }

        protected virtual int GetNumSubStats(StatsType eStats)
        {
            switch (eStats)
            {
                case StatsType.RESOURCE:        return (int)ResourceStatType.NUM_TYPES;
                case StatsType.BUILDING_CLASS:  return (int)BuildingStatType.NUM_TYPES;
                case StatsType.BLACK_MARKET:    return (int)BlackMarketStatType.NUM_TYPES;
                case StatsType.TECHNOLOGY:      return (int)TechnologyStatType.NUM_TYPES;
                case StatsType.PATENT:          return (int)PatentStatType.NUM_TYPES;
                case StatsType.SABOTAGING:      return (int)SabotagingStatType.NUM_TYPES;
                case StatsType.SABOTAGED:       return (int)SabotagedStatType.NUM_TYPES;
                case StatsType.STOCK:           return (int)StockStatType.NUM_TYPES;
                case StatsType.MISCELLANEOUS:   return (int)MiscellaneousStatType.NUM_TYPES;
                default:    MAssert.Unimplemented(); return 1;
            }
        }

        protected virtual int GetItemID(StatsType eStats, int eSubStat, PlayerType ePlayer, int iItem)
        {
            // [StatsType][SubStatType][Player][Item]
            int numItems = GetNumItems(eStats);
            int numPlayers = (int)gameClient().getNumPlayers();
            return (eSubStat * numPlayers * numItems) + ((int)ePlayer * numItems) + iItem;
        }

        //inverse of GetItemID()
        protected virtual void SplitItemID(StatsType eStats, int iItemID, out int eSubStat, out PlayerType ePlayer, out int iItem)
        {
            int numItems = GetNumItems(eStats);
            int numPlayers = (int)gameClient().getNumPlayers();

            eSubStat = iItemID / (numPlayers * numItems);
            iItemID = iItemID % (numPlayers * numItems);

            ePlayer = (PlayerType)(iItemID / numItems);
            iItem = iItemID % numItems;
        }
    }

    public class StatsServer : StatsClient
    {
        GameServer gameServer()
        {
            return (GameServer)mGame;
        }

        protected virtual void makeDirty(StatsDirtyType eType)
        {
            mDirtyBits.SetBit((int)eType, true);
        }
        public virtual void clearDirty()
        {
            mDirtyBits.Clear();
        }

        protected virtual void markStatChange(StatsType eType, int iItemID)
        {
            mStatChanges[(int)eType].Add(iItemID);
            makeDirty(StatsDirtyType.mStatChanges);
        }

        public StatsServer(GameClient pGame)
            : base(pGame)
        {
        }

        public virtual void changeStat(StatsType eStats, int eSubStat, PlayerType ePlayer, int iItem, int iAmount)
        {
            if (shouldRecordData() && !gameServer().playerServer(ePlayer).isSubsidiary() && (iAmount != 0))
            {
                // [StatsType][SubStatType][Player][Item]
                miStats[(int)eStats][eSubStat][(int)ePlayer][iItem] += iAmount;
                markStatChange(eStats, GetItemID(eStats, eSubStat, ePlayer, iItem));
            }
        }

        public virtual void addEvent(PlayerType ePlayer, StatEventType eType, int iData1, int iData2 = -1, int iData3 = -1, int iData4 = -1)
        {
            mEventChanges[(int)ePlayer].Add(new StatEventData(eType, mGame.getTurnCount(), iData1, iData2, iData3, iData4));

            makeDirty(StatsDirtyType.mEventChanges);
        }

        //called once per turn
        public virtual void doTurn()
        {
            // don't record any more data when the game has been won
            if (!shouldRecordData())
            {
                return;
            }

            //update records every 20 game turns
            if ((mGame.getTurnCount() == 1) || (mGame.getTurnCount() % GAME_TURNS_PER_SAMPLE == 0))
            {
                // record stock prices
                for (PlayerType eLoopPlayer = 0; eLoopPlayer < mGame.getNumPlayers(); eLoopPlayer++)
                {
                    PlayerClient player = mGame.playerClient(eLoopPlayer);

                    if (!(player.isSubsidiary()))
                    {
                        mStockPriceChanges[(int)eLoopPlayer].Add(player.getSharePrice());
                    }
                }

                // record resource prices
                for(ResourceType eResource=0; eResource<infos().resourcesNum(); eResource++)
                {
                    mResourcePriceChanges[(int)eResource].Add(mGame.marketClient().getPrice(eResource));
                }

                makeDirty(StatsDirtyType.mStockPriceChanges);
                makeDirty(StatsDirtyType.mResourcePriceChanges);
            }
        }

    }
}