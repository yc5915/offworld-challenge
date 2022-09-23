using System.Collections.Generic;
using System;
using System.IO;
using System.Xml;
using System.Linq;
using UnityEngine;
using Offworld.GameCore.Text;
using Offworld.SystemCore;

namespace Offworld.GameCore
{
    public class HQClient : TextHelpers
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

        public enum HQDirtyType
        {
            FIRST,

            meOwner,

            NUM_TYPES
        }

        protected BitMask mDirtyBits = new BitMask();
        protected virtual bool isDirty(HQDirtyType eType)
        {
            return mDirtyBits.GetBit((int)eType);
        }
        public virtual bool isAnyDirty()
        {
            return !(mDirtyBits.IsEmpty());
        }

        protected int miID = -1;
        protected int miTileID = -1;

        protected PlayerType meOwner = PlayerType.NONE;
        protected PlayerType meFoundingOwner = PlayerType.NONE;
        protected HQType meType = HQType.NONE;

        public HQClient(GameClient pGame)
        {
            mGame = pGame;
        }

        protected virtual void SerializeClient(object stream, bool bAll)
        {
            SimplifyIO.Data(stream, ref mDirtyBits, "DirtyBits");

            if (isDirty(HQDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref miID, "ID");
            }
            if (isDirty(HQDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref miTileID, "TileID");
            }

            if (isDirty(HQDirtyType.meOwner) || bAll)
            {
                SimplifyIO.Data(stream, ref meOwner, "Owner");
            }
            if (isDirty(HQDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref meFoundingOwner, "FoundingOwner");
            }
            if (isDirty(HQDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref meType, "Type");
            }
        }

        public virtual void writeClientValues(BinaryWriter stream, bool bAll)
        {
            SerializeClient(stream, bAll);
        }

        public virtual void readClientValues(BinaryReader stream, bool bAll)
        {
            SerializeClient(stream, bAll);
        }

        public static int resourceBonus(ResourceType eResource, TileClient pTile, PlayerType eOwner, HQType eHQ, GameClient game, Infos infos)
        {
            int iMining = infos.resource(eResource).miHQBonus;

            iMining *= infos.resourceLevel(pTile.getResourceLevel(eResource, false)).miRateMultiplier;

            iMining *= Math.Max(0, (infos.HQ(eHQ).miHQBonusModifier + 100));
            iMining /= 100;

            return Math.Max(0, iMining);
        }

        public virtual Vector3 getAverageWorldPosition()
        {
            InfoHQ hqInfo = infos().HQ(getType());
            return getHQFootprint(tileClient(), gameClient(), hqInfo).Select(t => t.getWorldPosition()).Average();
        }

        public static IEnumerable<TileClient> getHQFootprint(TileClient pTile, GameClient pGame, InfoHQ hqInfo)
        {
            yield return pTile;
            for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
            {
                if (hqInfo.mabFootprint[(int)eDirection])
                {
                    TileClient pResultTile = pGame.mapClient().tileClientAdjacent(pTile, eDirection);
                    if (pResultTile != null)
                    {
                        yield return pResultTile;
                    }
                }
            }
        }

    	public virtual int getID()
    	{
    		return miID;
    	}

        public virtual int getTileID()
        {
            return miTileID;
        }
        public virtual TileClient tileClient()
        {
            return gameClient().tileClient(getTileID());
        }
        public virtual int getX()
        {
            return tileClient().getX();
        }
        public virtual int getY()
        {
            return tileClient().getY();
        }
        public virtual Vector3 getWorldPosition()
        {
            return tileClient().getWorldPosition();
        }

        public virtual PlayerType getOwner()
        {
            return meOwner;
        }
        public virtual PlayerType getFoundingOwner()
        {
            return meFoundingOwner;
        }
        public virtual PlayerClient ownerClient()
        {
            return gameClient().playerClient(getOwner());
        }
        public virtual TeamType getTeam()
        {
            return ownerClient().getTeam();
        }

        public virtual HQType getType()
        {
            return meType;
        }
    }

    public class HQServer : HQClient
    {
        protected virtual GameServer gameServer()
    	{
            return (GameServer)mGame;
    	}

        protected virtual void makeDirty(HQDirtyType eType)
        {
            mDirtyBits.SetBit((int)eType, true);
        }
        protected virtual void makeAllDirty()
        {
            for (HQDirtyType eLoopType = 0; eLoopType < HQDirtyType.NUM_TYPES; eLoopType++)
            {
                makeDirty(eLoopType);
            }
        }
        public virtual void clearDirty()
        {
            mDirtyBits.Clear();
        }

        public HQServer(GameClient pGame)
            : base(pGame)
        {
        }

        public virtual void init(GameServer pGame, int iID, PlayerType eOwner, TileServer pTile)
    	{
            using (new UnityProfileScope("HQServer.init"))
            {
                mGame = pGame;

    		    miID = iID;
                miTileID = pTile.getID();

                meOwner = eOwner;
                meFoundingOwner = eOwner;
                meType = ownerServer().getHQ();

                List<TileServer> aHQFootprint;
                using (new UnityProfileScope("HQServer.getFootprint"))
                {
                    aHQFootprint = ownerServer().getHQTileFootprint(pTile, getType()).ToList();
                }

                using (new UnityProfileScope("HQServer.setFootprintOwner"))
                {
                    foreach (TileServer pLoopTile in aHQFootprint)
                    {
                        pLoopTile.setOwner(getOwner(), true);
                        pLoopTile.setHQ(this);
                    }
                }

                using (new UnityProfileScope("HQServer.increaseVisibility"))
                {
                    gameServer().getPlayerServerAll().ForEach(player => pTile.increaseVisibility(player.getPlayer(), VisibilityType.VISIBLE, false));

                    foreach (TileServer pLoopTile in aHQFootprint)
                    {
                        gameServer().getPlayerServerAll().ForEach(player => pLoopTile.increaseVisibility(player.getPlayer(), VisibilityType.VISIBLE, false));

                        for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                        {
                            TileServer pAdjacentTile = gameServer().tileServerAdjacent(pLoopTile, eLoopDirection);

                            if (pAdjacentTile != null)
                            {
                                gameServer().getPlayerServerAll().ForEach(player => pAdjacentTile.increaseVisibility(player.getPlayer(), VisibilityType.VISIBLE, false));
                            }
                        }
                    }
                }

                if ((int)gameClient().getNumPlayers() > 1)
                {
                    using (new UnityProfileScope("HQServer.setClaimBlock"))
                    {
                        int iNumHQs = (infos().HQ(getType()).miExtraHQs + 1);
                        int iRadius = infos().Globals.MIN_HQ_DISTANCE;

                        iRadius = ((iRadius + (iNumHQs / 2)) / iNumHQs);

                        foreach (TileServer pRangeTile in gameServer().tileServerRangeIterator(pTile, iRadius))
                        {
                            if (pRangeTile.hasResources() || pRangeTile.hasResourcesAdjacent() || pRangeTile.isGeothermal() || (pRangeTile.getIce() != IceType.NONE && gameClient().getLocation() != LocationType.EUROPA) || infos().terrain(pRangeTile.getTerrain()).mbRequiredOnly || infos().terrain(pRangeTile.getTerrain()).maiResourceRate.Any(x => x > 0))
                            {
                                if (ownerServer().canClaimTile(pRangeTile, false))
                                {
                                    int iFactor = iRadius - Math.Max(2, Utils.stepDistanceTile(pRangeTile, pTile)) + 1;

                                    pRangeTile.setClaimBlockPlayer(getOwner());
                                    pRangeTile.setClaimBlockTime(infos().Globals.FOUND_CLAIM_BLOCK_TIME * iFactor);
                                }
                            }
                        }
                    }
                }

                using (new UnityProfileScope("HQServer.collectResources"))
                {
                    List<int> aiResourcesAmountTotal = Enumerable.Repeat(0, (int)infos().resourcesNum()).ToList();

                    foreach (TileServer pLoopTile in aHQFootprint)
                    {
                        List<int> aiResourcesAmount = Enumerable.Repeat(0, (int)infos().resourcesNum()).ToList();

                        foreach (InfoResource pResource in infos().resources())
                        {
                            int iBonusAmount = resourceBonus(pResource.meType, pLoopTile, eOwner, getType(), gameServer(), infos());
                            if (iBonusAmount > 0)
                            {
                                ownerServer().changeResourceStockpile(pResource.meType, iBonusAmount, false);
                                aiResourcesAmount[pResource.miType] = iBonusAmount;
                                aiResourcesAmountTotal[pResource.miType] += iBonusAmount;
                            }
                            pLoopTile.setResourceLevel(pResource.meType, ResourceLevelType.NONE);
                        }

                        gameServer().gameEventsServer().AddHQFoundResources(iID, pLoopTile.getID(), aiResourcesAmount);
                    }

                    gameServer().gameEventsServer().AddHQFound(getOwner(), pTile.getHQID(), aiResourcesAmountTotal);
                }

                gameServer().updateConnectedToHQ();

                makeAllDirty();
            }
    	}

        public virtual void setOwner(PlayerType eNewValue)
        {
            PlayerType eOldValue = getOwner();

            if (eOldValue != eNewValue)
            {
                if (eOldValue != PlayerType.NONE)
                {
                    ownerServer().removeHQ(this);
                }

                meOwner = eNewValue;

                if (eNewValue != PlayerType.NONE)
                {
                    ownerServer().addHQ(getID());
                }

                makeDirty(HQDirtyType.meOwner);
            }
        }

        public virtual TileServer tileServer()
        {
            return (TileServer)tileClient();
        }

        public PlayerServer ownerServer()
        {
            return (PlayerServer)ownerClient();
        }
    }
}
