using System.Collections.Generic;
using System.IO;
using Offworld.SystemCore;

namespace Offworld.GameCore
{
    public class TileGroupServer
    {
        protected GameServer mGame = null;

        protected bool mbDirty = true;
        protected int miID = -1;
        protected bool mbHQ = false;
        protected HashSet<int> mTileSet = new HashSet<int>();

        protected virtual void SerializeServer(object stream)
        {
            SimplifyIO.Data(stream, ref miID, "ID");
            SimplifyIO.Data(stream, ref mbHQ, "HQ");
            SimplifyIO.Data(stream, ref mTileSet, "HashSet");
        }

        public virtual void writeServerValues(BinaryWriter stream)
        {
            SerializeServer(stream);
        }

        public virtual void readServerValues(BinaryReader stream, GameServer game)
        {
            SerializeServer(stream);

            mGame = game; // Restore the reference to the game
        }

        protected virtual GameServer gameServer()
        {
            return (GameServer)mGame;
        }

        protected virtual Infos infos()
        {
            return gameServer().infos();
        }

        public virtual bool isDirty()
        {
            return mbDirty;
        }
        protected virtual void makeDirty()
        {
            mbDirty = true;
        }
        public virtual void clearDirty()
        {
            mbDirty = false;
        }

        public TileGroupServer(GameServer pGame)
        {
            mGame = pGame;
        }

        public virtual void init(GameServer game, int iID)
        {
            mGame = game;

            miID = iID;
        }

        public virtual void updateHQ()
        {
            bool bNewValue = false;

            foreach (int iTileID in getTileSet())
            {
                if (gameServer().tileServer(iTileID).isHQ())
                {
                    bNewValue = true;
                    break;
                }
            }

            if (isHQ() != bNewValue)
            {
                mbHQ = bNewValue;

                makeDirty();
            }
        }

        public virtual int getID()
        {
            return miID;
        }

        public virtual bool isHQ()
        {
            return mbHQ;
        }

        public virtual HashSet<int> getTileSet()
        {
            return mTileSet;
        }
        protected virtual int getNumTiles()
        {
            return getTileSet().Count;
        }
        public virtual void addTile(TileServer pTile)
        {
            mTileSet.Add(pTile.getID());

            if (getNumTiles() == 1)
            {
                gameServer().addTileGroup(this);
            }

            updateHQ();

            makeDirty();
        }
        public virtual void removeTile(TileServer pTile)
        {
            mTileSet.Remove(pTile.getID());

            if (getNumTiles() == 0)
            {
                gameServer().removeTileGroup(this);
            }
            else
            {
                updateHQ();
            }

            makeDirty();
        }
        public virtual void resetGroup()
        {
            List<TileServer> oldTileList = new List<TileServer>();

            foreach (int iTileID in getTileSet())
            {
                oldTileList.Add(gameServer().tileServer(iTileID));
            }

            foreach (TileServer pLoopTile in oldTileList)
            {
                pLoopTile.setTileGroup(null);
            }

            foreach (TileServer pLoopTile in oldTileList)
            {
                pLoopTile.updateTileGroup();
            }
        }
        public virtual void join(TileGroupServer pTileGroup)
        {
            List<TileServer> oldTileList = new List<TileServer>();

            foreach (int iTileID in getTileSet())
            {
                oldTileList.Add(gameServer().tileServer(iTileID));
            }

            foreach (TileServer pLoopTile in oldTileList)
            {
                pLoopTile.setTileGroup(pTileGroup);
            }
        }
    }
}