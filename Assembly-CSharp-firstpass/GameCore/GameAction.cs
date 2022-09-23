using System.Collections.Generic;
using System;
using System.IO;
using Offworld.SystemCore;

namespace Offworld.GameCore
{
    public class GameAction
    {
        protected PlayerType mePlayer = PlayerType.NONE;
        protected ItemType meItem = ItemType.NONE;

        protected LinkedList<int> mValueList = new LinkedList<int>();

        public void writeVector(BinaryWriter writer)
        {
            writer.Write((byte)(mePlayer));
            writer.Write7BitInt((int)meItem);

            writer.Write7BitInt(mValueList.Count);
            foreach (int iValue in mValueList)
            {
                writer.Write7BitInt(iValue);
            }
        }

        public void readVector(BinaryReader reader)
        {
            mePlayer = (PlayerType)reader.ReadByte();
            meItem = (ItemType)reader.Read7BitInt();

            int iNum = reader.Read7BitInt();

            mValueList.Clear();
            for (Int16 i = 0; i < iNum; i++)
            {
                mValueList.AddLast(reader.Read7BitInt());
            }
        }

        public PlayerType getPlayer()
        {
            return mePlayer;
        }
        public void setPlayer(PlayerType eNewValue)
        {
            mePlayer = eNewValue;
        }

        public ItemType getItem()
        {
            return meItem;
        }
        public void setItem(ItemType eNewValue)
        {
            meItem = eNewValue;
        }

        public void addLastValue(int iNewValue)
        {
            mValueList.AddLast(iNewValue);
        }
        public int removeFirstValue()
        {
            int iValue = -1;

            if (mValueList.Count > 0)
            {
                iValue = mValueList.First.Value;
                mValueList.RemoveFirst();
            }

            return iValue;
        }
        public bool valuesLeft()
        {
            return (mValueList.Count > 0);
        }
    }
}