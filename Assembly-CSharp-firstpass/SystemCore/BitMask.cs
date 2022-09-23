using System;
using UnityEngine.Assertions;

namespace Offworld.SystemCore
{
    //32-bit bitmask, bit-indices are 0-based
    public class BitMask
    {
        private int data = 0;

        public BitMask()
        {
        }

        public bool IsEmpty()
        {
            return (data == 0);
        }

        public void Clear()
        {
            data = 0;
        }

        public int GetRaw()
        {
            return data;
        }

        public void SetRaw(int data)
        {
            this.data = data;
        }

        public void SetBit(int bit, bool value)
        {
            if((bit < 0) || (bit >= 32))
                Assert.IsTrue(false, "Bit: " + bit + " is out of range.");

            if (value) //set bit
                data |= (1 << bit);
            else //clear bit
                data &= ~(1 << bit);
        }

        public bool GetBit(int bit)
        {
            if((bit < 0) || (bit >= 32))
                Assert.IsTrue(false, "Bit: " + bit + " is out of range.");

            int mask = (1 << bit);
            return ((data & mask) == mask);
        }
    }

    //http://stackoverflow.com/questions/6438352/using-enum-as-generic-type-parameter-in-c-sharp
    public class BitMaskEnum<T> where T : struct, IComparable, IConvertible, IFormattable
    {
        private BitMask data;

        public BitMaskEnum()
        {
            data = new BitMask();
        }

        public bool IsEmpty()
        {
            return data.IsEmpty();
        }

        public void Clear()
        {
            data.Clear();
        }

        public int GetRaw()
        {
            return data.GetRaw();
        }

        public void SetRaw(int data)
        {
            this.data.SetRaw(data);
        }

        public void SetBit(T bit, bool value)
        {
            data.SetBit(CastTo<int>.From(bit), value);
        }

        public bool GetBit(T bit)
        {
            return data.GetBit(CastTo<int>.From(bit));
        }
    }

    //multi-word bitmask, bit-indices are 0-based
    public class BitMaskMulti
    {
        private int [] data = null;

        public BitMaskMulti(int numBits)
        {
            int size = (numBits - 1) / 32 + 1;
            data = new int [size];
        }

        public bool IsEmpty()
        {
            for(int i=0; i<data.Length; i++)
            {
                if(data[i] != 0)
                    return false;
            }
            return true;
        }

        public void Clear()
        {
            for(int i=0; i<data.Length; i++)
                data[i] = 0;
        }

        public int [] GetRaw()
        {
            return data;
        }

        public void SetRaw(int [] data)
        {
            this.data = data;
        }

        public void SetBit(int bit, bool value)
        {
            if((bit < 0) || (bit >= 32 * data.Length))
                Assert.IsTrue(false, "Bit: " + bit + " is out of range.");

            int index = bit >> 5; //bit/32
            int newBit = bit & 31; //bit%32

            if(value) //set bit
                data[index] |= (1 << newBit);
            else //clear bit
                data[index] &= ~(1 << newBit);
        }

        public bool GetBit(int bit)
        {
            if((bit < 0) || (bit >= 32 * data.Length))
                Assert.IsTrue(false, "Bit: " + bit + " is out of range.");

            int index = bit >> 5; //bit/32
            int newBit = bit & 31; //bit%32

            int mask = (1 << newBit);
            return ((data[index] & mask) == mask);
        }
    }

    public class BitMaskMultiEnum<T> where T : struct, IComparable, IConvertible, IFormattable
    {
        private BitMaskMulti data;

        public BitMaskMultiEnum(int numBits)
        {
            data = new BitMaskMulti(numBits);
        }

        public bool IsEmpty()
        {
            return data.IsEmpty();
        }

        public void Clear()
        {
            data.Clear();
        }

        public int[] GetRaw()
        {
            return data.GetRaw();
        }

        public void SetRaw(int[] data)
        {
            this.data.SetRaw(data);
        }

        public void SetBit(T bit, bool value)
        {
            data.SetBit(CastTo<int>.From(bit), value);
        }

        public bool GetBit(T bit)
        {
            return data.GetBit(CastTo<int>.From(bit));
        }
    }
}