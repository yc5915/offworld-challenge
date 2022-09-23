using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using System.Linq;
using Offworld.SystemCore;

namespace Offworld.GameCore
{
    public class MarketClient
    {
        protected GameClient mGame = null;
        protected virtual GameClient gameClient()
        {
            return mGame;
        }
        public virtual void setGame(GameServer pGame)
        {
            mGame = pGame;
        }
        protected virtual Infos infos()
        {
            return gameClient().infos();
        }

        public enum MarketDirtyType
        {
            FIRST,

            maiPrice,
            maiOffworldPrice,

            NUM_TYPES
        }

        protected BitMask mDirtyBits = new BitMask();
        protected virtual bool isDirty(MarketDirtyType eType)
        {
            return mDirtyBits.GetBit((int)eType);
        }
        public virtual bool isAnyDirty()
        {
            return !(mDirtyBits.IsEmpty());
        }

        protected List<int> maiPrice = new List<int>();
        protected List<int> maiMinPrice = new List<int>();
        protected List<int> maiMaxPrice = new List<int>();
        protected List<int> maiOffworldPrice = new List<int>();

        public MarketClient(GameClient pGame)
        {
            mGame = pGame;
        }

        // Note: Since prices are limit to 1000 then it probably can be written as Int16
        protected virtual void SerializeClient(object stream, bool bAll, int compatibilityNumber)
        {
            SimplifyIO.Data(stream, ref mDirtyBits, "DirtyBits");

            if (isDirty(MarketDirtyType.maiPrice) || bAll)
            {
                SimplifyIO.Data(stream, ref maiPrice, (int)infos().resourcesNum(), "Price_");
            }
            if (isDirty(MarketDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref maiMinPrice, (int)infos().resourcesNum(), "MinPrice_");
            }
            if (isDirty(MarketDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref maiMaxPrice, (int)infos().resourcesNum(), "MaxPrice_");
            }
            if (isDirty(MarketDirtyType.maiOffworldPrice) || bAll)
            {
                SimplifyIO.Data(stream, ref maiOffworldPrice, (int)infos().resourcesNum(), "OffworldPrice_");
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

        protected virtual int supplyDemand(int iWholePrice)
        {
            int iValue = iWholePrice;

            if (iWholePrice > 100)
            {
                iValue = (iValue / 100) + 20;
            }
            else if (iWholePrice > 10)
            {
                iValue = (iValue / 10) + 10;
            }

            iValue *= ((Constants.MAX_NUM_PLAYERS / 2) + 1);
            iValue /= (((int)(gameClient().getNumPlayers())) + 1);

            return Math.Max(1, iValue);
        }

        public virtual int calculateBuyCost(ResourceType eResource, int iQuantity)
        {
            int iTotalCost = 0;

            int iQuantityLeft = iQuantity;
            int iCurrentPrice = getPrice(eResource);

            while (iQuantityLeft > 0)
            {
                int iCurrentTrade = Math.Min(Constants.TRADE_QUANTITY, iQuantityLeft);

                for (int i = 0; i < iCurrentTrade; i++)
                {
                    iCurrentPrice = Utils.clamp((iCurrentPrice + supplyDemand(iCurrentPrice / Constants.PRICE_MULTIPLIER)), getMinPrice(eResource), getMaxPrice(eResource));
                    iTotalCost += (iCurrentPrice / Constants.PRICE_MULTIPLIER);
                }


                iQuantityLeft -= iCurrentTrade;
            }

            return iTotalCost;
        }

        public virtual int calculateSellRevenue(ResourceType eResource, int iQuantity, int iMinPrice)
        {
            int iTotalProfit = 0;
            int iQuantityLeft = iQuantity;
            int iCurrentPrice = getPrice(eResource);
            int iPriceFloor = getMinPrice(eResource);
            int iPriceCeiling = getMaxPrice(eResource);

            while ((iQuantityLeft > 0) && ((iCurrentPrice / Constants.PRICE_MULTIPLIER) >= iMinPrice))
            {
                int iCurrentTrade = Math.Min(Constants.TRADE_QUANTITY, iQuantityLeft);

                for (int i = 0; i < iCurrentTrade; i++)
                {
                    iCurrentPrice = Utils.clamp((iCurrentPrice - supplyDemand(iCurrentPrice / Constants.PRICE_MULTIPLIER)), iPriceFloor, iPriceCeiling);
                    iTotalProfit += (iCurrentPrice / Constants.PRICE_MULTIPLIER);
                }

                iQuantityLeft -= iCurrentTrade;
            }

            return iTotalProfit;
        }

        public virtual int getPrice(ResourceType eIndex)
        {
            return maiPrice[(int)eIndex];
        }
        public virtual int getWholePrice(ResourceType eIndex)
        {
            return (getPrice(eIndex) / Constants.PRICE_MULTIPLIER);
        }
        public virtual int getHighestWholePrice()
        {
            int iBestValue = 0;

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                if (gameClient().isResourceValid(eLoopResource))
                {
                    iBestValue = Math.Max(iBestValue, getWholePrice(eLoopResource));
                }
            }

            return iBestValue;
        }

        public virtual int getMinPrice(ResourceType eIndex)
        {
            return maiMinPrice[(int)eIndex];
        }

        public virtual int getMaxPrice(ResourceType eIndex)
        {
            return maiMaxPrice[(int)eIndex];
        }

        protected virtual int getOffworldPrice(ResourceType eIndex)
        {
            return maiOffworldPrice[(int)eIndex];
        }
        public virtual int getWholeOffworldPrice(ResourceType eIndex)
        {
            return (getOffworldPrice(eIndex) / Constants.PRICE_MULTIPLIER);
        }
    }

    public class MarketServer : MarketClient
    {
        GameServer gameServer()
        {
            return (GameServer)mGame;
        }

        protected virtual void makeDirty(MarketDirtyType eType)
        {
            mDirtyBits.SetBit((int)eType, true);
        }
        protected virtual void makeAllDirty()
        {
            for (MarketDirtyType eLoopType = 0; eLoopType < MarketDirtyType.NUM_TYPES; eLoopType++)
            {
                makeDirty(eLoopType);
            }
        }
        public virtual void clearDirty()
        {
            mDirtyBits.Clear();
        }

        protected List<int> maiSupply = new List<int>();

        public MarketServer(GameClient pGame)
            : base(pGame)
        {
        }

        public virtual void init(GameServer pGame)
        {
            using (new UnityProfileScope("MarketServer.init"))
            {
                mGame = pGame;

                maiPrice = Enumerable.Repeat(0, (int)infos().resourcesNum()).ToList();
                maiMinPrice = Enumerable.Repeat(Constants.PRICE_MIN, (int)infos().resourcesNum()).ToList();
                maiMaxPrice = Enumerable.Repeat(Constants.PRICE_MAX, (int)infos().resourcesNum()).ToList();
                maiOffworldPrice = Enumerable.Repeat(0, (int)infos().resourcesNum()).ToList();

                initVariablesServer();

                if (gameServer().getColonyClass() != ColonyClassType.NONE)
                {
                    for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                    {
                        {
                            int iMinPrice = infos().colonyClass(gameServer().getColonyClass()).maiResourceMinPrice[(int)eLoopResource];
                            if (iMinPrice > 0)
                            {
                                maiMinPrice[(int)eLoopResource] = (iMinPrice * Constants.PRICE_MULTIPLIER);
                            }
                        }

                        {
                            int iMaxPrice = infos().colonyClass(gameServer().getColonyClass()).maiResourceMaxPrice[(int)eLoopResource];
                            if (iMaxPrice > 0)
                            {
                                maiMaxPrice[(int)eLoopResource] = (iMaxPrice * Constants.PRICE_MULTIPLIER);
                            }
                        }
                    }
                }

                makeAllDirty();
            }
        }

        protected virtual void initVariablesServer()
        {
            maiSupply = Enumerable.Repeat(0, (int)infos().resourcesNum()).ToList();
        }

        protected virtual void SerializeServer(object stream)
        {
            SimplifyIO.Data(stream, ref maiSupply, "Supply");
        }

        public virtual void writeServerValues(BinaryWriter stream, bool bAll, int compatibilityNumber)
        {
            writeClientValues(stream, bAll, compatibilityNumber);
            SerializeServer(stream);
        }

        public virtual void readServerValues(BinaryReader stream, bool bAll, int compatibilityNumber)
        {
            readClientValues(stream, bAll, compatibilityNumber);
            SerializeServer(stream);
        }

        public virtual void rebuildFromClient()
        {
            initVariablesServer();
        }

        public virtual void initPost()
        {
            using (new UnityProfileScope("MarketServer.initPost"))
            {
                System.Random pLocalRandom = new CrossPlatformRandom(gameServer().getSeed());

                foreach (InfoResource pLoopResource in infos().resources())
                {
                    ResourceType eLoopResource = pLoopResource.meType;
                    if (gameServer().isCampaign())
                    {
                        setPrice(eLoopResource, (Globals.Campaign.getResourcePrice(eLoopResource) * Constants.PRICE_MULTIPLIER));
                        setOffworldPrice(eLoopResource, (Globals.Campaign.getResourcePriceOffworld(eLoopResource) * Constants.PRICE_MULTIPLIER));
                    }
                    else
                    {
                        System.Random pRandom = ((Globals.AppInfo.GameMode == GameModeType.INFINITE_CHALLENGE) ? gameServer().random() : pLocalRandom);

                        {
                            int iPrice = pLoopResource.miMarketPrice;

                            if (gameServer().isGameOption(GameOptionType.RANDOM_PRICES))
                            {
                                switch (pRandom.Next(4))
                                {
                                    case 0:
                                        iPrice /= 2;
                                        break;

                                    case 1:
                                    case 2:
                                        break;

                                    case 3:
                                        iPrice *= 2;
                                        break;
                                }
                            }

                            setPrice(eLoopResource, (iPrice * Constants.PRICE_MULTIPLIER));
                        }

                        if (pLoopResource.mbTrade)
                        {
                            int iOffworldPrice = pLoopResource.miOffworldPrice;
                            iOffworldPrice += pRandom.Next(pLoopResource.miOffworldRand);
                            if (gameServer().getNumPlayers() < (PlayerType)(Constants.MAX_NUM_PLAYERS / 2))
                            {
                                iOffworldPrice *= 3;
                                iOffworldPrice /= 4;
                            }

                            if (iOffworldPrice > 10)
                            {
                                iOffworldPrice -= (iOffworldPrice % 10);
                            }
                            else
                            {
                                iOffworldPrice = 10;
                            }

                            setOffworldPrice(eLoopResource, (iOffworldPrice * Constants.PRICE_MULTIPLIER));
                        }
                    }
                }
            }
        }

        public virtual void doTurn()
        {
            using (new UnityProfileScope("Market::doTurn"))
            {
                int iDemand = infos().location(gameServer().getLocation()).miMarketDemandBase;

                iDemand *= (gameServer().getTurnCount());
                iDemand /= (120);

                iDemand *= (int)(gameServer().getNumPlayers());

                foreach (InfoResource pLoopResource in infos().resources())
                {
                    if (gameServer().isResourceValid(pLoopResource.meType))
                    {
                        changePrice(pLoopResource.meType, iDemand);

                        if (pLoopResource.miLaunchQuantity > 0)
                        {
                            if (gameServer().random().Next(pLoopResource.miOffworldDemandProb) == 0)
                            {
                                if (!(gameServer().isCampaign()) || (gameServer().random().Next(Globals.Campaign.getTurnsTotal()) == 0))
                                {
                                    changeOffworldPrice(pLoopResource.meType, 10 * Constants.PRICE_MULTIPLIER);
                                }
                            }
                        }
                    }
                }
            }
        }

        public virtual void setPrice(ResourceType eIndex, int iNewValue)
        {
            iNewValue = Utils.clamp(iNewValue, getMinPrice(eIndex), getMaxPrice(eIndex));

            if (getPrice(eIndex) != iNewValue)
            {
                maiPrice[(int)eIndex] = iNewValue;
                foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAll())
                {
                    pLoopPlayer.setShouldCalculateCashResources(true);
                }

                makeDirty(MarketDirtyType.maiPrice);
            }
        }
        public virtual void changePrice(ResourceType eIndex, int iChange)
        {
            if (iChange != 0)
            {
                setPrice(eIndex, (getPrice(eIndex) + iChange));
            }
        }

        public virtual void setOffworldPrice(ResourceType eIndex, int iNewValue)
        {
            if (getOffworldPrice(eIndex) != iNewValue)
            {
                maiOffworldPrice[(int)eIndex] = iNewValue;

                makeDirty(MarketDirtyType.maiOffworldPrice);
            }
        }
        public virtual void changeOffworldPrice(ResourceType eIndex, int iChange)
        {
            if (iChange != 0)
            {
                setOffworldPrice(eIndex, (getOffworldPrice(eIndex) + iChange));
            }
        }

        public virtual int getSupply(ResourceType eIndex)
        {
            return maiSupply[(int)eIndex];
        }
        public virtual void changeSupply(ResourceType eIndex, int iChange)
        {
            if (iChange != 0)
            {
                int iNewSupply = (getSupply(eIndex) + iChange);

                while (iNewSupply <= -(Constants.RESOURCE_MULTIPLIER))
                {
                    changePrice(eIndex, supplyDemand(getWholePrice(eIndex)));
                    iNewSupply += Constants.RESOURCE_MULTIPLIER;
                }

                while (iNewSupply >= Constants.RESOURCE_MULTIPLIER)
                {
                    changePrice(eIndex, -(supplyDemand(getWholePrice(eIndex))));
                    iNewSupply -= Constants.RESOURCE_MULTIPLIER;
                }

                maiSupply[(int)eIndex] = iNewSupply;
            }
        }
    }
}