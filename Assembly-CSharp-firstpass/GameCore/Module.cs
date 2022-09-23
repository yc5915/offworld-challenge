using System.Collections.Generic;
using System;
using System.IO;
using System.Xml;
using UnityEngine;
using Offworld.SystemCore;
using System.Linq;

namespace Offworld.GameCore
{
    public class ModuleClient
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

        public enum ModuleDirtyType
        {
            FIRST,

            mbOccupied,

            NUM_TYPES
        }

        protected BitMask mDirtyBits = new BitMask();
        protected virtual bool isDirty(ModuleDirtyType eType)
        {
            return mDirtyBits.GetBit((int)eType);
        }
        public virtual bool isAnyDirty()
        {
            return !(mDirtyBits.IsEmpty());
        }

        protected int miID = -1;
        protected int miTileID = -1;

        protected bool mbOccupied = false;

        protected ModuleType meType = ModuleType.NONE;

        public ModuleClient(GameClient pGame)
        {
            mGame = pGame;
        }

        protected virtual void SerializeClient(object stream, bool bAll)
        {
            SimplifyIO.Data(stream, ref mDirtyBits, "DirtyBits");

            if (isDirty(ModuleDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref miID, "ID");
            }
            if (isDirty(ModuleDirtyType.FIRST) || bAll)
            {
                SimplifyIO.Data(stream, ref miTileID, "TileID");
            }

            if (isDirty(ModuleDirtyType.mbOccupied) || bAll)
            {
                SimplifyIO.Data(stream, ref mbOccupied, "Occupied");
            }

            if (isDirty(ModuleDirtyType.FIRST) || bAll)
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

        public virtual bool isOccupied()
        {
            return mbOccupied;
        }

        public virtual ModuleType getType()
        {
            return meType;
        }

        public static IEnumerable<TileClient> getModuleFootprint(TileClient tile, GameClient game, InfoModule moduleInfo)
        {
            yield return tile;
            for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
            {
                if (moduleInfo.mabFootprint[(int)eDirection])
                {
                    TileClient loopTile = game.mapClient().tileClientAdjacent(tile, eDirection);
                    if (loopTile != null)
                    {
                        yield return loopTile;
                    }
                }
            }
        }
    }

    public class ModuleServer : ModuleClient
    {
        protected virtual GameServer gameServer()
    	{
            return (GameServer)mGame;
    	}

        protected virtual void makeDirty(ModuleDirtyType eType)
        {
            mDirtyBits.SetBit((int)eType, true);
        }
        protected virtual void makeAllDirty()
        {
            for (ModuleDirtyType eLoopType = 0; eLoopType < ModuleDirtyType.NUM_TYPES; eLoopType++)
            {
                makeDirty(eLoopType);
            }
        }
        public virtual void clearDirty()
        {
            mDirtyBits.Clear();
        }

        protected List<int> maiResourceConsumed = new List<int>();

        public ModuleServer(GameClient pGame)
            : base(pGame)
        {
        }

        public virtual void init(GameServer pGame, int iID, ModuleType eType, TileServer pTile)
    	{
            mGame = pGame;

    		miID = iID;
            miTileID = pTile.getID();

            meType = eType;

            pTile.setModule(this);

            for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
            {
                if (infos().module(getType()).mabFootprint[(int)eDirection])
                {
                    TileServer pAdjacentTile = gameServer().tileServerAdjacent(pTile, eDirection);

                    if (pAdjacentTile != null)
                    {
                        pAdjacentTile.setModule(this);
                    }
                }
            }

            gameServer().changeModuleCount(getType(), 1);

            if (infos().module(getType()).mbPopulation)
            {
                gameServer().changeMaxPopulation(1);
            }
            if (infos().module(getType()).mbLabor)
            {
                gameServer().changeLabor(1);
            }

            makeAllDirty();
    	}

        public virtual void doTurn()
        {
            if (!isOccupied())
            {
                return;
            }

            foreach (InfoResource pLoopResource in infos().resources())
            {
                int iDemand = GameClient.getModuleSupply(getType(), pLoopResource.meType, infos(), gameServer(), gameServer().getLocation(), gameServer().isCampaign());
                if (iDemand != 0)
                {
                    gameServer().marketServer().changeSupply(pLoopResource.meType, -(iDemand));
                }
            }
        }

        public virtual void makeOccupied()
        {
            if (!isOccupied())
            {
                mbOccupied = true;

                if (infos().module(getType()).mbPopulation)
                {
                    gameServer().changePopulation(1);
                }

                gameServer().changeEntertainmentDemand(infos().module(getType()).miEntertainmentDemand);

                makeDirty(ModuleDirtyType.mbOccupied);
            }
        }

        public virtual void makeVacant()
        {
            if (isOccupied())
            {
                mbOccupied = false;

                if (infos().module(getType()).mbPopulation)
                {
                    gameServer().changePopulation(-1);
                }

                gameServer().changeEntertainmentDemand(-infos().module(getType()).miEntertainmentDemand);

                makeDirty(ModuleDirtyType.mbOccupied);
            }
        }

        public virtual TileServer tileServer()
        {
            return (TileServer)tileClient();
        }
    }
}