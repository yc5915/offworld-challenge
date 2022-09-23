using System;
using System.IO;
using System.Text;
using Offworld.SystemCore;
using UnityEngine;

// Token: 0x02000128 RID: 296
public static class LZF
{
    // Token: 0x0600079F RID: 1951 RVA: 0x0003CD48 File Offset: 0x0003B148
    public static void CompressionTest()
    {
        for (int i = 0; i < 10; i++)
        {
            string text = string.Empty;
            int num = (new System.Random()).Next(1, 2048);
            for (int j = 0; j < num; j++)
            {
                text += Guid.NewGuid();
            }
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            byte[] array = null;
            if (!LZF.CompressMessage(bytes, ref array))
            {
                //Debug.Log("Data not compress");
            }
            else
            {
                byte[] array2 = LZF.DecompressMessage(array);
                int num2 = BitConverter.ToInt32(array, 1);
                if (bytes.Length != num2)
                {
                    //Debug.Log(string.Concat(new object[]
                    //{
                    //    "Lengths don't match: ",
                    //    bytes.Length,
                    //    " != ",
                    //    num2
                    //}));
                }
                for (int k = 0; k < bytes.Length; k++)
                {
                    if (bytes[k] != array2[k])
                    {
                        //Debug.Log("decompressed array doesn't match the original at index: " + k);
                        return;
                    }
                }
                //Debug.Log(string.Concat(new object[]
                //{
                //    "LZF - original: ",
                //    bytes.Length,
                //    " compressed: ",
                //    array.Length
                //}));
            }
        }
    }

    // Token: 0x060007A0 RID: 1952 RVA: 0x0003CE98 File Offset: 0x0003B298
    public static void CompressStream(Stream srcStream, Stream dstStream)
    {
        int num = 32767;
        int idealNumChunks = (int)Math.Ceiling((double)srcStream.Length / num);
        int idealChunkSize = (int)Math.Ceiling((double)srcStream.Length / idealNumChunks);
        num = idealChunkSize;
        byte[] array = new byte[num];
        byte[] buffer = new byte[num * 2];
        int num2;
        while ((num2 = srcStream.Read(array, 0, array.Length)) > 0)
        {
            int num3 = LZF.lzf_compress(array, num2, ref buffer);
            if (num3 < num2 && num3 != 0)
            {
                dstStream.Write(BitConverter.GetBytes((short)num3), 0, 2);
                dstStream.Write(buffer, 0, num3);
            }
            else
            {
                dstStream.Write(BitConverter.GetBytes((short)(-(short)num2)), 0, 2);
                dstStream.Write(array, 0, num2);
            }
        }
    }

    // Token: 0x060007A1 RID: 1953 RVA: 0x0003CF7C File Offset: 0x0003B37C
    public static void DecompressStream(Stream srcStream, Stream dstStream)
    {
        short maxValue = short.MaxValue;
        byte[] array = new byte[(int)maxValue];
        byte[] buffer = new byte[(int)(maxValue * 2)];
        byte[] array2 = new byte[2];
        while (srcStream.Read(array2, 0, 2) == 2)
        {
            int num = BitConverter.ToInt16(array2, 0);
            if (num > 0)
            {
                if (srcStream.Read(array, 0, (int)num) == (int)num)
                {
                    int count = LZF.lzf_decompress(array, (int)num, ref buffer);
                    dstStream.Write(buffer, 0, count);
                }
            }
            else
            {
                num = -num;
                if (srcStream.Read(array, 0, (int)num) == (int)num)
                {
                    dstStream.Write(array, 0, (int)num);
                }
            }
        }
    }

    // Token: 0x060007A2 RID: 1954 RVA: 0x0003D06C File Offset: 0x0003B46C
    public static int lzf_compress(byte[] input, int length, ref byte[] output)
    {
        int num = output.Length;
        Array.Clear(LZF.HashTable, 0, (int)LZF.HSIZE);
        uint num2 = 0U;
        uint num3 = 0U;
        uint num4 = (uint)((int)input[(int)((UIntPtr)num2)] << 8 | (int)input[(int)((UIntPtr)(num2 + 1U))]);
        int num5 = 0;
        for (; ; )
        {
            if ((ulong)num2 < (ulong)((long)(length - 2)))
            {
                num4 = (num4 << 8 | (uint)input[(int)((UIntPtr)(num2 + 2U))]);
                long num6 = (long)((ulong)((num4 ^ num4 << 5) >> (int)(24U - LZF.HLOG - num4 * 5U) & LZF.HSIZE - 1U));
                long num7 = LZF.HashTable[(int)(checked((IntPtr)num6))];
                LZF.HashTable[(int)(checked((IntPtr)num6))] = (long)((ulong)num2);
                long num8;
                if ((num8 = (long)((ulong)num2 - (ulong)num7 - 1UL)) < (long)((ulong)LZF.MAX_OFF) && (ulong)(num2 + 4U) < (ulong)((long)length) && num7 > 0L && input[(int)(checked((IntPtr)num7))] == input[(int)((UIntPtr)num2)] && input[(int)(checked((IntPtr)(unchecked(num7 + 1L))))] == input[(int)((UIntPtr)(num2 + 1U))] && input[(int)(checked((IntPtr)(unchecked(num7 + 2L))))] == input[(int)((UIntPtr)(num2 + 2U))])
                {
                    uint num9 = 2U;
                    uint num10 = (uint)(length - (int)num2 - (int)num9);
                    num10 = ((num10 <= LZF.MAX_REF) ? num10 : LZF.MAX_REF);
                    if ((ulong)num3 + (ulong)((long)num5) + 1UL + 3UL >= (ulong)((long)num))
                    {
                        break;
                    }
                    do
                    {
                        num9 += 1U;
                    }
                    while (num9 < num10 && input[(int)(checked((IntPtr)(unchecked(num7 + (long)((ulong)num9)))))] == input[(int)((UIntPtr)(num2 + num9))]);
                    if (num5 != 0)
                    {
                        output[(int)((UIntPtr)(num3++))] = (byte)(num5 - 1);
                        num5 = -num5;
                        do
                        {
                            output[(int)((UIntPtr)(num3++))] = input[(int)(checked((IntPtr)(unchecked((ulong)num2 + (ulong)((long)num5)))))];
                        }
                        while (++num5 != 0);
                    }
                    num9 -= 2U;
                    num2 += 1U;
                    if (num9 < 7U)
                    {
                        output[(int)((UIntPtr)(num3++))] = (byte)((num8 >> 8) + (long)((ulong)((ulong)num9 << 5)));
                    }
                    else
                    {
                        output[(int)((UIntPtr)(num3++))] = (byte)((num8 >> 8) + 224L);
                        output[(int)((UIntPtr)(num3++))] = (byte)(num9 - 7U);
                    }
                    output[(int)((UIntPtr)(num3++))] = (byte)num8;
                    num2 += num9 - 1U;
                    num4 = (uint)((int)input[(int)((UIntPtr)num2)] << 8 | (int)input[(int)((UIntPtr)(num2 + 1U))]);
                    num4 = (num4 << 8 | (uint)input[(int)((UIntPtr)(num2 + 2U))]);
                    LZF.HashTable[(int)((UIntPtr)((num4 ^ num4 << 5) >> (int)(24U - LZF.HLOG - num4 * 5U) & LZF.HSIZE - 1U))] = (long)((ulong)num2);
                    num2 += 1U;
                    num4 = (num4 << 8 | (uint)input[(int)((UIntPtr)(num2 + 2U))]);
                    LZF.HashTable[(int)((UIntPtr)((num4 ^ num4 << 5) >> (int)(24U - LZF.HLOG - num4 * 5U) & LZF.HSIZE - 1U))] = (long)((ulong)num2);
                    num2 += 1U;
                    continue;
                }
            }
            else if ((ulong)num2 == (ulong)((long)length))
            {
                goto Block_13;
            }
            num5++;
            num2 += 1U;
            if ((long)num5 == (long)((ulong)LZF.MAX_LIT))
            {
                if ((ulong)(num3 + 1U + LZF.MAX_LIT) >= (ulong)((long)num))
                {
                    return 0;
                }
                output[(int)((UIntPtr)(num3++))] = (byte)(LZF.MAX_LIT - 1U);
                num5 = -num5;
                do
                {
                    output[(int)((UIntPtr)(num3++))] = input[(int)(checked((IntPtr)(unchecked((ulong)num2 + (ulong)((long)num5)))))];
                }
                while (++num5 != 0);
            }
        }
        return 0;
    Block_13:
        if (num5 != 0)
        {
            if ((ulong)num3 + (ulong)((long)num5) + 1UL >= (ulong)((long)num))
            {
                return 0;
            }
            output[(int)((UIntPtr)(num3++))] = (byte)(num5 - 1);
            num5 = -num5;
            do
            {
                output[(int)((UIntPtr)(num3++))] = input[(int)(checked((IntPtr)(unchecked((ulong)num2 + (ulong)((long)num5)))))];
            }
            while (++num5 != 0);
        }
        return (int)num3;
    }

    // Token: 0x060007A3 RID: 1955 RVA: 0x0003D3B4 File Offset: 0x0003B7B4
    public static int lzf_decompress(byte[] input, int length, ref byte[] output)
    {
        int num = output.Length;
        uint num2 = 0U;
        uint num3 = 0U;
        for (; ; )
        {
            uint num4 = (uint)input[(int)((UIntPtr)(num2++))];
            if (num4 < 32U)
            {
                num4 += 1U;
                if ((ulong)(num3 + num4) > (ulong)((long)num))
                {
                    break;
                }
                do
                {
                    output[(int)((UIntPtr)(num3++))] = input[(int)((UIntPtr)(num2++))];
                }
                while ((num4 -= 1U) != 0U);
            }
            else
            {
                uint num5 = num4 >> 5;
                int num6 = (int)(num3 - ((num4 & 31U) << 8) - 1U);
                if (num5 == 7U)
                {
                    num5 += (uint)input[(int)((UIntPtr)(num2++))];
                }
                num6 -= (int)input[(int)((UIntPtr)(num2++))];
                if ((ulong)(num3 + num5 + 2U) > (ulong)((long)num))
                {
                    return 0;
                }
                if (num6 < 0)
                {
                    return 0;
                }
                output[(int)((UIntPtr)(num3++))] = output[num6++];
                output[(int)((UIntPtr)(num3++))] = output[num6++];
                do
                {
                    output[(int)((UIntPtr)(num3++))] = output[num6++];
                }
                while ((num5 -= 1U) != 0U);
            }
            if ((ulong)num2 >= (ulong)((long)length))
            {
                return (int)num3;
            }
        }
        return 0;
    }

    // Token: 0x060007A4 RID: 1956 RVA: 0x0003D4B4 File Offset: 0x0003B8B4
    public static bool CompressMessage(byte[] input, ref byte[] output)
    {
        bool result;
        using (new UnityProfileScope("Compression::CompressMessage"))
        {
            if (input == null)
            {
                output = null;
                result = false;
            }
            else if (input.Length <= 1200)
            {
                output = input;
                result = false;
            }
            else
            {
                if (LZF.internalBuffer == null || LZF.internalBuffer.Length < input.Length)
                {
                    LZF.internalBuffer = new byte[input.Length];
                }
                int num = LZF.lzf_compressByteOffset(input, input.Length, ref LZF.internalBuffer);
                if (num >= input.Length || num == 0)
                {
                    output = input;
                    result = false;
                }
                else
                {
                    output = new byte[num];
                    Buffer.BlockCopy(LZF.internalBuffer, 0, output, 0, num);
                    result = true;
                }
            }
        }
        return result;
    }

    // Token: 0x060007A5 RID: 1957 RVA: 0x0003D580 File Offset: 0x0003B980
    public static byte[] DecompressMessage(byte[] input)
    {
        byte[] result;
        using (new UnityProfileScope("Compression::DecompressMessage"))
        {
            int num = BitConverter.ToInt32(input, 1);
            if (num == 0)
            {
                Debug.LogError("Invalid length in message trying to be decompressed");
                result = null;
            }
            else
            {
                if (LZF.internalBuffer == null || LZF.internalBuffer.Length < num)
                {
                    LZF.internalBuffer = new byte[num];
                }
                if (LZF.internalBuffer == null)
                {
                    Debug.LogError("Unable to allocate memory to decompress a message of length: " + num);
                    result = null;
                }
                else
                {
                    LZF.lzf_decompressByteOffset(input, input.Length, ref LZF.internalBuffer);
                    result = LZF.internalBuffer;
                }
            }
        }
        return result;
    }

    // Token: 0x060007A6 RID: 1958 RVA: 0x0003D63C File Offset: 0x0003BA3C
    public static int lzf_compressByteOffset(byte[] input, int length, ref byte[] output)
    {
        int num = output.Length;
        Array.Clear(LZF.HashTable, 0, (int)LZF.HSIZE);
        uint num2 = 1U;
        uint num3 = 1U;
        output[0] = input[0];
        byte[] bytes = BitConverter.GetBytes(length);
        for (int i = 0; i < bytes.Length; i++)
        {
            output[1 + i] = bytes[i];
        }
        num3 += (uint)bytes.Length;
        uint num4 = (uint)((int)input[(int)((UIntPtr)num2)] << 8 | (int)input[(int)((UIntPtr)(num2 + 1U))]);
        int num5 = 0;
        for (; ; )
        {
            if ((ulong)num2 < (ulong)((long)(length - 2)))
            {
                num4 = (num4 << 8 | (uint)input[(int)((UIntPtr)(num2 + 2U))]);
                long num6 = (long)((ulong)((num4 ^ num4 << 5) >> (int)(24U - LZF.HLOG - num4 * 5U) & LZF.HSIZE - 1U));
                long num7 = LZF.HashTable[(int)(checked((IntPtr)num6))];
                LZF.HashTable[(int)(checked((IntPtr)num6))] = (long)((ulong)num2);
                long num8;
                if ((num8 = (long)((ulong)num2 - (ulong)num7 - 1UL)) < (long)((ulong)LZF.MAX_OFF) && (ulong)(num2 + 4U) < (ulong)((long)length) && num7 > 0L && input[(int)(checked((IntPtr)num7))] == input[(int)((UIntPtr)num2)] && input[(int)(checked((IntPtr)(unchecked(num7 + 1L))))] == input[(int)((UIntPtr)(num2 + 1U))] && input[(int)(checked((IntPtr)(unchecked(num7 + 2L))))] == input[(int)((UIntPtr)(num2 + 2U))])
                {
                    uint num9 = 2U;
                    uint num10 = (uint)(length - (int)num2 - (int)num9);
                    num10 = ((num10 <= LZF.MAX_REF) ? num10 : LZF.MAX_REF);
                    if ((ulong)num3 + (ulong)((long)num5) + 1UL + 3UL >= (ulong)((long)num))
                    {
                        break;
                    }
                    do
                    {
                        num9 += 1U;
                    }
                    while (num9 < num10 && input[(int)(checked((IntPtr)(unchecked(num7 + (long)((ulong)num9)))))] == input[(int)((UIntPtr)(num2 + num9))]);
                    if (num5 != 0)
                    {
                        output[(int)((UIntPtr)(num3++))] = (byte)(num5 - 1);
                        num5 = -num5;
                        do
                        {
                            output[(int)((UIntPtr)(num3++))] = input[(int)(checked((IntPtr)(unchecked((ulong)num2 + (ulong)((long)num5)))))];
                        }
                        while (++num5 != 0);
                    }
                    num9 -= 2U;
                    num2 += 1U;
                    if (num9 < 7U)
                    {
                        output[(int)((UIntPtr)(num3++))] = (byte)((num8 >> 8) + (long)((ulong)((ulong)num9 << 5)));
                    }
                    else
                    {
                        output[(int)((UIntPtr)(num3++))] = (byte)((num8 >> 8) + 224L);
                        output[(int)((UIntPtr)(num3++))] = (byte)(num9 - 7U);
                    }
                    output[(int)((UIntPtr)(num3++))] = (byte)num8;
                    num2 += num9 - 1U;
                    num4 = (uint)((int)input[(int)((UIntPtr)num2)] << 8 | (int)input[(int)((UIntPtr)(num2 + 1U))]);
                    num4 = (num4 << 8 | (uint)input[(int)((UIntPtr)(num2 + 2U))]);
                    LZF.HashTable[(int)((UIntPtr)((num4 ^ num4 << 5) >> (int)(24U - LZF.HLOG - num4 * 5U) & LZF.HSIZE - 1U))] = (long)((ulong)num2);
                    num2 += 1U;
                    num4 = (num4 << 8 | (uint)input[(int)((UIntPtr)(num2 + 2U))]);
                    LZF.HashTable[(int)((UIntPtr)((num4 ^ num4 << 5) >> (int)(24U - LZF.HLOG - num4 * 5U) & LZF.HSIZE - 1U))] = (long)((ulong)num2);
                    num2 += 1U;
                    continue;
                }
            }
            else if ((ulong)num2 == (ulong)((long)length))
            {
                goto Block_14;
            }
            num5++;
            num2 += 1U;
            if ((long)num5 == (long)((ulong)LZF.MAX_LIT))
            {
                if ((ulong)(num3 + 1U + LZF.MAX_LIT) >= (ulong)((long)num))
                {
                    return 0;
                }
                output[(int)((UIntPtr)(num3++))] = (byte)(LZF.MAX_LIT - 1U);
                num5 = -num5;
                do
                {
                    output[(int)((UIntPtr)(num3++))] = input[(int)(checked((IntPtr)(unchecked((ulong)num2 + (ulong)((long)num5)))))];
                }
                while (++num5 != 0);
            }
        }
        return 0;
    Block_14:
        if (num5 != 0)
        {
            if ((ulong)num3 + (ulong)((long)num5) + 1UL >= (ulong)((long)num))
            {
                return 0;
            }
            output[(int)((UIntPtr)(num3++))] = (byte)(num5 - 1);
            num5 = -num5;
            do
            {
                output[(int)((UIntPtr)(num3++))] = input[(int)(checked((IntPtr)(unchecked((ulong)num2 + (ulong)((long)num5)))))];
            }
            while (++num5 != 0);
        }
        return (int)num3;
    }

    // Token: 0x060007A7 RID: 1959 RVA: 0x0003D9C0 File Offset: 0x0003BDC0
    public static int lzf_decompressByteOffset(byte[] input, int length, ref byte[] output)
    {
        int num = output.Length;
        uint num2 = 1U;
        uint num3 = 1U;
        output[0] = input[0];
        num2 += 4U;
        for (; ; )
        {
            uint num4 = (uint)input[(int)((UIntPtr)(num2++))];
            if (num4 < 32U)
            {
                num4 += 1U;
                if ((ulong)(num3 + num4) > (ulong)((long)num))
                {
                    break;
                }
                do
                {
                    output[(int)((UIntPtr)(num3++))] = input[(int)((UIntPtr)(num2++))];
                }
                while ((num4 -= 1U) != 0U);
            }
            else
            {
                uint num5 = num4 >> 5;
                int num6 = (int)(num3 - ((num4 & 31U) << 8) - 1U);
                if (num5 == 7U)
                {
                    num5 += (uint)input[(int)((UIntPtr)(num2++))];
                }
                num6 -= (int)input[(int)((UIntPtr)(num2++))];
                if ((ulong)(num3 + num5 + 2U) > (ulong)((long)num))
                {
                    return 0;
                }
                if (num6 < 0)
                {
                    return 0;
                }
                output[(int)((UIntPtr)(num3++))] = output[num6++];
                output[(int)((UIntPtr)(num3++))] = output[num6++];
                do
                {
                    output[(int)((UIntPtr)(num3++))] = output[num6++];
                }
                while ((num5 -= 1U) != 0U);
            }
            if ((ulong)num2 >= (ulong)((long)length))
            {
                return (int)num3;
            }
        }
        return 0;
    }

    // Token: 0x0400073E RID: 1854
    private static readonly uint HLOG = 14U;

    // Token: 0x0400073F RID: 1855
    private static readonly uint HSIZE = 16384U;

    // Token: 0x04000740 RID: 1856
    private static readonly uint MAX_LIT = 32U;

    // Token: 0x04000741 RID: 1857
    private static readonly uint MAX_OFF = 8192U;

    // Token: 0x04000742 RID: 1858
    private static readonly uint MAX_REF = 264U;

    // Token: 0x04000743 RID: 1859
    private static readonly long[] HashTable = new long[LZF.HSIZE];

    // Token: 0x04000744 RID: 1860
    private static byte[] internalBuffer = null;
}
