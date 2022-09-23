using System.Collections.Generic;
using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Assertions;

namespace Offworld.SystemCore
{
    public static class SimplifyIO
    {
        public static bool IsReading(object stream)
        {
            return (stream is BinaryReader);
        }

        public static bool IsWriting(object stream)
        {
            return (stream is BinaryWriter) || (stream is XmlWriter);
        }

        // byte
        public static void Data(object stream, ref byte value, string name)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;
                writer.Write(value);
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;
                value = reader.ReadByte();
            }
            else if (stream is XmlWriter)
            {
                XmlWriter xmlWriter = stream as XmlWriter;
                
                xmlWriter.WriteElementString(name, "System.Byte");
            }
            else if( stream is MJSONwriter )
            {
                MJSONwriter writer = stream as MJSONwriter;

                writer.WriteValue(name, (int)value);
            }
        }
        
        // sbyte
        public static void Data(object stream, ref sbyte value, string name)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;
                writer.Write(value);
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;
                value = reader.ReadSByte();
            }
            else if (stream is XmlWriter)
            {
                XmlWriter xmlWriter = stream as XmlWriter;
                
                xmlWriter.WriteElementString(name, "System.SByte");
            }
            else if (stream is MJSONwriter)
            {
                MJSONwriter writer = stream as MJSONwriter;

                writer.WriteValue(name, (int)value);
            }
        }
        
        // int
        public static void Data(object stream, ref int value, string name)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;
                writer.Write7BitInt(value);
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;
                value = reader.Read7BitInt();
            }
            else if (stream is XmlWriter)
            {
                XmlWriter xmlWriter = stream as XmlWriter;
                
                xmlWriter.WriteElementString(name, "System.Int32");
            }
            else if (stream is MJSONwriter)
            {
                MJSONwriter writer = stream as MJSONwriter;

                writer.WriteValue(name, (int)value);
            }
        }

        // long
        public static void Data(object stream, ref long value, string name)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;
                writer.Write7BitLong(value);
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;
                value = reader.Read7BitLong();
            }
            else if (stream is XmlWriter)
            {
                XmlWriter xmlWriter = stream as XmlWriter;

                xmlWriter.WriteElementString(name, "System.Int64");
            }
            else if (stream is MJSONwriter)
            {
                MJSONwriter writer = stream as MJSONwriter;

                writer.WriteValue(name, (long)value);
            }
        }

        // int
        public static void DataInt32(object stream, ref int value, string name)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;
                writer.Write(value);
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;
                value = reader.ReadInt32();
            }
            else if (stream is XmlWriter)
            {
                XmlWriter xmlWriter = stream as XmlWriter;
                xmlWriter.WriteElementString(name, "System.Int32");
            }
            else if (stream is MJSONwriter)
            {
                MJSONwriter writer = stream as MJSONwriter;
                writer.WriteValue(name, (int)value);
            }
        }
        
        // uint
        public static void Data(object stream, ref uint value, string name)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;
                writer.Write(value);
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;
                value = reader.ReadUInt32();
            }
            else if (stream is XmlWriter)
            {
                XmlWriter xmlWriter = stream as XmlWriter;
                
                xmlWriter.WriteElementString(name, "System.UInt32");
            }
            else if (stream is MJSONwriter)
            {
                MJSONwriter writer = stream as MJSONwriter;

                writer.WriteValue(name, (int)value);
            }
        }
        
        // ulong
        public static void Data(object stream, ref ulong value, string name)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;
                writer.Write(value);
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;
                value = reader.ReadUInt64();
            }
            else if (stream is XmlWriter)
            {
                XmlWriter xmlWriter = stream as XmlWriter;
                
                xmlWriter.WriteElementString(name, "System.UInt64");
            }
            else if (stream is MJSONwriter)
            {
                MJSONwriter writer = stream as MJSONwriter;

                writer.WriteValue(name, (int)value);
            }
        }
        
        
        // float
        public static void Data(object stream, ref float value, string name)
        {
            if( stream is BinaryWriter )
            {
                BinaryWriter writer = stream as BinaryWriter;
                writer.Write(value);
            }
            else if( stream is BinaryReader )
            {
                BinaryReader reader = stream as BinaryReader;
                value = reader.ReadSingle();
            }
            else if (stream is XmlWriter)
            {
                XmlWriter xmlWriter = stream as XmlWriter;
                
                xmlWriter.WriteElementString(name, "System.Single");
            }
            else if (stream is MJSONwriter)
            {
                MJSONwriter writer = stream as MJSONwriter;

                writer.WriteValue(name, value);
            }
        }
        
        // bool
        public static void Data(object stream, ref bool value, string name)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;
                writer.Write(value);
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;
                value = reader.ReadBoolean();
            }
            /*else if (stream is XmlWriter)
            {
                XmlWriter xmlWriter = stream as XmlWriter;

                xmlWriter.WriteElementString(name, "System.Boolean");
            }*/
            else if (stream is MJSONwriter)
            {
                MJSONwriter writer = stream as MJSONwriter;

                writer.WriteValue(name, value);
            }
        }
        
        // string
        public static void Data(object stream, ref string value, string name)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;
                writer.Write(value);
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;
                value = reader.ReadString();
            }
            else if (stream is MJSONwriter)
            {
                MJSONwriter writer = stream as MJSONwriter;

                writer.WriteValue(name, value);
            }
        }
        
        // BitMask
        public static void Data(object stream, ref BitMask value, string name)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;
                writer.Write7BitInt(value.GetRaw());
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;
                value.SetRaw(reader.Read7BitInt());
            }
        }

        // BitMaskEnum
        public static void Data<T>(object stream, ref BitMaskEnum<T> value, string name) where T : struct, IComparable, IConvertible, IFormattable
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;
                writer.Write7BitInt(value.GetRaw());
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;
                value.SetRaw(reader.Read7BitInt());
            }
        }

        // BitMaskMulti
        public static void Data(object stream, ref BitMaskMulti value, string name)
        {
            int [] raw = value.GetRaw();
            SimplifyIO.Data(stream, ref raw, name);
            value.SetRaw(raw);
        }

        // BitMaskMultiEnum
        public static void Data<T>(object stream, ref BitMaskMultiEnum<T> value, string name) where T : struct, IComparable, IConvertible, IFormattable
        {
            int[] raw = value.GetRaw();
            SimplifyIO.Data(stream, ref raw, name);
            value.SetRaw(raw);
        }

        // int []
        public static void Data(object stream, ref int [] value, string name)
        {
            if(stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;
                writer.Write7BitInt(value.Length);
                for(int i=0; i<value.Length; i++)
                    writer.Write7BitInt(value[i]);
            }
            else if(stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;
                int count = reader.Read7BitInt();
                if (value == null)
                    value = new int[count];
                else
                    ArrayUtilities.SetSize(ref value, count);
                for(int i=0; i<value.Length; i++)
                    value[i] = reader.Read7BitInt();
            }
        }

        // bool []
        public static void Data(object stream, ref bool [] value, string name)
        {
            if(stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;
                writer.Write7BitInt(value.Length);
                for(int i=0; i<value.Length; i++)
                    writer.Write(value[i]);
            }
            else if(stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;
                int count = reader.Read7BitInt();
                if (value == null)
                    value = new bool[count];
                else
                    ArrayUtilities.SetSize(ref value, count);
                for(int i=0; i<value.Length; i++)
                    value[i] = reader.ReadBoolean();
            }
        }
        
        // System.Random
        public static void Data(object stream, ref System.Random value, string name)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;
                
                // Serialize to a memory stream using the binary formatter
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream streamForRandom = new MemoryStream();
                formatter.Serialize(streamForRandom, value);
                
                // Convert the stream to a byte array and serialize it
                byte[] valueAsArray = streamForRandom.ToArray();
                writer.Write7BitInt(valueAsArray.Length);
                writer.Write(valueAsArray);
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;
                
                // Deserialize the saved byte array for random
                BinaryFormatter formatter = new BinaryFormatter();
                int count = reader.Read7BitInt();
                byte[] randomSave = new byte[count];
                reader.Read(randomSave, 0, count);
                
                // Use the binary formatter read the data from the byte array
                MemoryStream streamForRandom = new MemoryStream(randomSave);
                value = (System.Random)formatter.Deserialize(streamForRandom);
            }
        }

        // Guid
        public static void Data(object stream, ref Guid value, string name)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;
                writer.Write(value.ToString());
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;
                string data = reader.ReadString();
                value = new Guid(data);
            }
        }

        // Generic types that will fit into an Int32
        public static void Data<T>(object stream, ref T value, string name)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;
                int data = CastTo<int>.From(value);
                writer.Write7BitInt(data);
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;
                value = CastTo<T>.From(reader.Read7BitInt());
            }
        }

        // list of generic types that will fit into an Int32
        public static void Data<T>(object stream, ref List<T> value, string name)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;
                
                writer.Write7BitInt(value.Count);
                
                foreach (T i in value)
                {
                    int data = CastTo<int>.From(i);
                    writer.Write7BitInt(data);
                }
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;
                
                int iNum = reader.Read7BitInt();

                ClearList(ref value, iNum);
                for (int i = 0; i < iNum; i++)
                {
                    T data = CastTo<T>.From(reader.Read7BitInt());
                    value.Add(data);
                }
            }
        }

        // Lists of list of generic types that will fit into an Int32
        public static void Data<T>(object stream, ref List<List<T>> value, string name)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;

                writer.Write7BitInt(value.Count);

                foreach (List<T> subList in value)
                {
                    writer.Write7BitInt(subList.Count);

                    foreach (T i in subList)
                    {
                        int data = CastTo<int>.From(i);
                        writer.Write7BitInt(data);
                    }
                }
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;

                Int32 count = reader.Read7BitInt();

                ClearList(ref value, count);
                for (int i = 0; i < count; i++)
                {
                    Int32 subCount = reader.Read7BitInt();
                    List<T> subList = new List<T>(subCount);

                    for (int j = 0; j < subCount; j++)
                    {
                        T data = CastTo<T>.From(reader.Read7BitInt());
                        subList.Add(data);
                    }

                    value.Add(subList);
                }
            }
        }

        // Lists of list of generic types that will fit into an Int32
        public static void Data<T>(object stream, ref List<HashSet<T>> value, string name)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;

                writer.Write7BitInt(value.Count);

                foreach (HashSet<T> subSet in value)
                {
                    writer.Write7BitInt(subSet.Count);

                    foreach (T i in subSet)
                    {
                        int data = CastTo<int>.From(i);
                        writer.Write7BitInt(data);
                    }
                }
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;

                Int32 count = reader.Read7BitInt();

                ClearList(ref value, count);
                for (int i = 0; i < count; i++)
                {
                    Int32 subCount = reader.Read7BitInt();
                    HashSet<T> subSet = new HashSet<T>();

                    for (int j = 0; j < subCount; j++)
                    {
                        T data = CastTo<T>.From(reader.Read7BitInt());
                        subSet.Add(data);
                    }

                    value.Add(subSet);
                }
            }
        }

        //For dictionaries with values that can be converted to 7-bit ints
        public static void Data<T>(object stream, ref Dictionary<string, T> value, string name)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;

                int numEntries = value.Count;
                SimplifyIO.Data(stream, ref numEntries, "NumEntries");

                foreach (string key in value.Keys)
                {
                    writer.Write(key);
                    writer.Write7BitInt(CastTo<int>.From(value[key]));
                }
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;

                value = new Dictionary<string, T>();
                int numEntries = 0;
                SimplifyIO.Data(stream, ref numEntries, "NumEntries");

                for (int i = 0; i < numEntries; i++)
                {
                    string key = reader.ReadString();
                    int enumValue = reader.Read7BitInt();
                    value.Add(key, CastTo<T>.From(enumValue));
                }
            }
        }

        // list of ints
        public static void Data(object stream, ref List<int> value, string name)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;
                
                writer.Write7BitInt(value.Count);
                
                foreach (int i in value)
                    writer.Write7BitInt(i);
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;
                
                Int32 iNum = reader.Read7BitInt();

                ClearList(ref value, iNum);
                for (Int32 i = 0; i < iNum; i++)
                    value.Add(reader.Read7BitInt());
            }
            else if( stream is MJSONwriter )
            {
                MJSONwriter writer = stream as MJSONwriter;

                writer.StartArray(name);

                for (Int32 i = 0; i < value.Count; i++)
                    writer.WriteElement(value[i] );

                writer.EndArray(false, false);
            }
        }
        
        // list of floats
        public static void Data(object stream, ref List<float> value, string name)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;
                
                writer.Write7BitInt(value.Count);
                
                foreach (float i in value)
                    writer.Write(i);
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;
                
                Int32 iNum = reader.Read7BitInt();

                ClearList(ref value, iNum);
                for (float i = 0; i < iNum; i++)
                    value.Add(reader.ReadSingle());
            }
        }
        
        // list of bools with fixed count
        public static void Data(object stream, ref List<bool> value, int count, string name)
        {
            if (stream is BinaryWriter)
            {
                Assert.AreEqual(value.Count, count);
                BinaryWriter writer = stream as BinaryWriter;
                
                foreach (bool b in value)
                    writer.Write(b);
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;

                ClearList(ref value, count);
                for (int i = 0; i < count; i++)
                    value.Add(reader.ReadBoolean());
            }
        }
        
        // list of bools 
        public static void Data(object stream, ref List<bool> value, string name)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;
                
                writer.Write7BitInt(value.Count);
                
                foreach (bool b in value)
                    writer.Write(b);
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;
                
                Int32 count = reader.Read7BitInt();

                ClearList(ref value, count);
                for (int i = 0; i < count; i++)
                    value.Add(reader.ReadBoolean());
            }
        }
        
        // Lists of list of bools
        public static void Data(object stream, ref List<List<bool>> value, string name)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;
                
                writer.Write7BitInt(value.Count);
                
                foreach (List<bool> subList in value)
                {
                    writer.Write7BitInt(subList.Count);
                    
                    foreach (bool b in subList)
                    {
                        writer.Write(b);
                    }
                }
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;
                
                Int32 count = reader.Read7BitInt();

                ClearList(ref value, count);
                for (int i = 0; i < count; i++)
                {
                    Int32 subCount = reader.Read7BitInt();
                    List<bool> subList = new List<bool>(subCount);
                    
                    for (int j = 0; j < subCount; j++)
                    {
                        subList.Add(reader.ReadBoolean());
                    }
                    
                    value.Add(subList);
                }
            }
        }

        // Lists of lists of list of bools
        public static void Data(object stream, ref List<List<List<bool>>> value, string name)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;

                writer.Write7BitInt(value.Count);

                foreach (List<List<bool>> listOfListsOfBools in value)
                {
                    writer.Write7BitInt(listOfListsOfBools.Count);

                    foreach (List<bool> listOfBools in listOfListsOfBools)
                    {
                        writer.Write7BitInt(listOfBools.Count);

                        foreach (bool b in listOfBools)
                        {
                            writer.Write(b);
                        }
                    }
                }
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;

                Int32 countOfListOfListofLists = reader.Read7BitInt();

                ClearList(ref value, countOfListOfListofLists);

                for (int i = 0; i < countOfListOfListofLists; i++)
                {
                    Int32 countOfListsOfLists = reader.Read7BitInt();
                    List<List<bool>> listOfListsOfBools = new List<List<bool>>(countOfListsOfLists);

                    for (int j = 0; j < countOfListsOfLists; j++)
                   {
                       Int32 coundOfBools = reader.Read7BitInt();
                       List<bool> listOfBools = new List<bool>(coundOfBools);

                       for (int k = 0; k < coundOfBools; k++)
                       {
                           listOfBools.Add(reader.ReadBoolean());
                       }

                       listOfListsOfBools.Add(listOfBools);
                   }

                    value.Add(listOfListsOfBools);
                }
            }
        }

        // Lists of list of ints
        public static void Data(object stream, ref List<List<int>> value, string name)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;
                
                writer.Write7BitInt(value.Count);
                
                foreach (List<int> subList in value)
                {
                    writer.Write7BitInt(subList.Count);
                    
                    foreach (int i in subList)
                    {
                        writer.Write7BitInt(i);
                    }
                }
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;
                
                Int32 count = reader.Read7BitInt();

                ResizeList(ref value, count);
                for (int i = 0; i < count; i++)
                {
                    Int32 subCount = reader.Read7BitInt();

                    List<int> subList = value[i];
                    ClearList(ref subList, subCount);

                    for (int j = 0; j < subCount; j++)
                    {
                        subList.Add(reader.Read7BitInt());
                    }
                    
                    value[i] = subList;
                }
            }
            else if (stream is MJSONwriter)
            {
                MJSONwriter writer = stream as MJSONwriter;

                writer.StartArray(name);

                for (Int32 i = 0; i < value.Count; i++)
                {
                    writer.StartArray();

                    List<int> subList = value[i];
                    for (Int32 j = 0; j < subList.Count; j++)
                    {
                        writer.WriteElement(subList[j]);
                    }

                    writer.EndArray(true, (i+1) < value.Count);
                }

                writer.EndArray(false, false);
            }
        }

        // Lists of lists of list of list of ints
        public static void Data(object stream, ref List<List<List<List<int>>>> value, string name)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;

                writer.Write7BitInt(value.Count);

                foreach (List<List<List<int>>> listOfListOfListsOfInts in value)
                {
                    writer.Write7BitInt(listOfListOfListsOfInts.Count);

                    foreach (List<List<int>> listOfListsOfInts in listOfListOfListsOfInts)
                    {
                        writer.Write7BitInt(listOfListsOfInts.Count);

                        foreach (List<int> listOfInts in listOfListsOfInts)
                        {
                            writer.Write7BitInt(listOfInts.Count);

                            foreach (int i in listOfInts)
                            {
                                writer.Write7BitInt(i);
                            }
                        }
                    }
                }
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;

                Int32 countOfListOfListofListsOfLists = reader.Read7BitInt();
                ClearList(ref value, countOfListOfListofListsOfLists);

                for (int i = 0; i < countOfListOfListofListsOfLists; i++)
                {
                    Int32 countOfListOfListofLists = reader.Read7BitInt();
                    List<List<List<int>>> ListOflistOfListsOfInts = new List<List<List<int>>>(countOfListOfListofListsOfLists);

                    for (int j = 0; j < countOfListOfListofLists; j++)
                    {
                        Int32 countOfListsOfLists = reader.Read7BitInt();
                        List<List<int>> listOfListsOfInts = new List<List<int>>(countOfListsOfLists);

                        for (int k = 0; k < countOfListsOfLists; k++)
                        {
                            Int32 coundOfInts = reader.Read7BitInt();
                            List<int> listOfInts = new List<int>(coundOfInts);

                            for (int l = 0; l < coundOfInts; l++)
                            {
                                listOfInts.Add(reader.Read7BitInt());
                            }

                            listOfListsOfInts.Add(listOfInts);
                        }

                        ListOflistOfListsOfInts.Add(listOfListsOfInts);
                    }

                    value.Add(ListOflistOfListsOfInts);
                }
            }
            else if (stream is MJSONwriter)
            {
                MJSONwriter writer = stream as MJSONwriter;

                writer.StartArray(name);

                for (Int32 i = 0; i < value.Count; i++)
                {
                    writer.StartArray();

                    List<List<List<int>>> ListOflistOfListsOfInts = value[i];

                    for (Int32 j = 0; j < ListOflistOfListsOfInts.Count; j++)
                    {
                        writer.StartArray();

                        List<List<int>> listOfListsOfInts = ListOflistOfListsOfInts[j];

                        for (Int32 k = 0; k < listOfListsOfInts.Count; k++)
                        {
                            writer.StartArray();

                            List<int> listsOfInts = listOfListsOfInts[k];

                            for (Int32 l = 0; l < listsOfInts.Count; l++)
                            {
                                writer.WriteElement(listsOfInts[l]);
                            }

                            writer.EndArray(true, (k+1) < listOfListsOfInts.Count );
                        }

                        writer.EndArray(false, (j+1) < ListOflistOfListsOfInts.Count);
                    }

                    writer.EndArray(false, (i+1) < value.Count);
                }

                writer.EndArray(false, false);
            }
        }


        // list of strings
        public static void Data(object stream, ref List<string> value, int count, string name)
        {
            if (stream is BinaryWriter)
            {
                Assert.AreEqual(value.Count, count);
                BinaryWriter writer = stream as BinaryWriter;
                
                foreach (string s in value)
                    writer.Write(s);
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;

                ClearList(ref value, count);
                for (int i = 0; i < count; i++)
                    value.Add(reader.ReadString());
            }
        }
        
        //Variable-length list of strings
        public static void Data(object stream, ref List<string> value, string name)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;

                writer.Write7BitInt(value.Count);

                foreach (string s in value)
                    writer.Write(s);
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;

                Int32 iNum = reader.Read7BitInt();

                ClearList(ref value, iNum);
                for (int i = 0; i < iNum; i++)
                    value.Add(reader.ReadString());
            }
        }
        
        
        // list of bytes
        public static void Data(object stream, ref List<byte> value, int count, string name)
        {
            if (stream is BinaryWriter)
            {
                Assert.AreEqual(value.Count, count);
                BinaryWriter writer = stream as BinaryWriter;
                
                foreach (byte b in value)
                    writer.Write(b);
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;

                ClearList(ref value, count);
                for (int i = 0; i < count; i++)
                    value.Add(reader.ReadByte());
            }
        }
        
        // list of sbytes
        public static void Data(object stream, ref List<sbyte> value, int count, string name)
        {
            if (stream is BinaryWriter)
            {
                Assert.AreEqual(value.Count, count);
                BinaryWriter writer = stream as BinaryWriter;
                
                foreach (sbyte b in value)
                    writer.Write(b);
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;

                ClearList(ref value, count);
                for (int i = 0; i < count; i++)
                    value.Add(reader.ReadSByte());
            }
        }
        
        // list of ints with names
        public static void Data(object stream, ref List<int> value, int count, string name)
        {
            if (stream is BinaryWriter)
            {
                Assert.AreEqual(value.Count, count);
                BinaryWriter writer = stream as BinaryWriter;
                
                foreach (int i in value)
                    writer.Write7BitInt(i);
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;

                ClearList(ref value, count);
                for (int i = 0; i < count; i++)
                    value.Add(reader.Read7BitInt());
            }
        }
        
        // Generic list of data that will fit into an Int32 with names
        public static void Data<T>(object stream, ref List<T> value, int count, string name)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;
                
                foreach (T i in value)
                {
                    int data = CastTo<int>.From(i);
                    writer.Write7BitInt(data);
                }
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;

                ClearList(ref value, count);
                for (int i = 0; i < count; i++)
                {
                    T data = CastTo<T>.From(reader.Read7BitInt());
                    value.Add(data);
                }
            }
        }
        
        // Generic list of data that will fit into an SByte with names
        public static void DataSByte<T>(object stream, ref List<T> value, int count, string name)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;
                
                foreach (T i in value)
                {
                    sbyte data = (sbyte)CastTo<int>.From(i);
                    writer.Write(data);
                }
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;

                ClearList(ref value, count);
                for (int i = 0; i < count; i++)
                {
                    T data = CastTo<T>.From((int)reader.ReadSByte());
                    value.Add(data);
                }
            }
        }
        
        // HashSet of generic types that will fit into an Int32
        public static void Data<T>(object stream, ref HashSet<T> value, string name)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;
                
                writer.Write7BitInt(value.Count);

                foreach (T i in value)
                {
                    int data = CastTo<int>.From(i);
                    writer.Write7BitInt(data);
                }
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;

                int iNum = reader.Read7BitInt();

                ClearHashSet(ref value);
                for (int i = 0; i < iNum; i++)
                {
                    T data = CastTo<T>.From(reader.Read7BitInt());
                    value.Add(data);
                }
            }
        }
        
        // HashSet of ints
        public static void Data(object stream, ref HashSet<int> value, string name)
        {
            if (stream is BinaryWriter)
            {
                BinaryWriter writer = stream as BinaryWriter;
                
                writer.Write7BitInt(value.Count);

                foreach (int i in value)
                    writer.Write7BitInt(i);
            }
            else if (stream is BinaryReader)
            {
                BinaryReader reader = stream as BinaryReader;

                int iNum = reader.Read7BitInt();

                ClearHashSet(ref value);
                for (int i = 0; i < iNum; i++)
                {
                    value.Add(reader.Read7BitInt());
                }
            }
#if false
            else if (stream is MJSONwriter)
            {
                MJSONwriter writer = stream as MJSONwriter;

                writer.StartArray(name);

                foreach (int i in value)
                {
                    writer.WriteElement(i);
                }

                writer.EndArray(false, false);
            }
#endif
        }

        public static void Write7BitInt(this BinaryWriter stream, int value)
        {
            //modified from BinaryWriter.Write7BitEncodedInt()
            //zig-zag encoding: https://gist.github.com/mfuerstenau/ba870a29e16536fdbaba
            value = (value >> 31) ^ (value << 1); //zig-zag encode
            uint num;
	        for (num = (uint)value; num >= 128u; num >>= 7)
	        {
		        stream.Write((byte)(num | 128u));
	        }
	        stream.Write((byte)num);
        }

        public static int Read7BitInt(this BinaryReader stream)
        {
            //modified from BinaryReader.Read7BitEncodedInt()
            //zig-zag encoding: https://gist.github.com/mfuerstenau/ba870a29e16536fdbaba
            int num = 0;
	        int num2 = 0;
	        while (num2 != 35)
	        {
		        byte b = stream.ReadByte();
		        num |= ((int)(b & 127)) << num2;
		        num2 += 7;
		        if ((b & 128) == 0)
		        {
                    num = (int)(((uint) num) >> 1) ^ -(num & 1); //zig-zag decode
			        return num;
		        }
	        }
	        throw new FormatException("Format_Bad7BitInt32");
        }

        public static void Write7BitLong(this BinaryWriter stream, long value)
        {
            //modified from BinaryWriter.Write7BitEncodedInt()
            //zig-zag encoding: https://gist.github.com/mfuerstenau/ba870a29e16536fdbaba
            value = (value >> 63) ^ (value << 1); //zig-zag encode
            ulong num;
            for (num = (ulong)value; num >= 128u; num >>= 7)
            {
                stream.Write((byte)(num | 128u));
            }
            stream.Write((byte)num);
        }

        public static long Read7BitLong(this BinaryReader stream)
        {
            //modified from BinaryReader.Read7BitEncodedInt()
            //zig-zag encoding: https://gist.github.com/mfuerstenau/ba870a29e16536fdbaba
            long num = 0;
            int num2 = 0;
            while (num2 != 70)
            {
                byte b = stream.ReadByte();
                num |= ((long)(b & 127)) << num2;
                num2 += 7;
                if ((b & 128) == 0)
                {
                    num = (long)(((ulong)num) >> 1) ^ -(num & 1); //zig-zag decode
                    return num;
                }
            }
            throw new FormatException("Format_Bad7BitInt64");
        }

        public static void ResizeList<T>(ref List<T> value, int count)
        {
            if(value == null)
                value = new List<T>(count);
            value.Resize(count, default(T));
        }

        public static void ClearList<T>(ref List<T> value, int count)
        {
            if (value == null)
                value = new List<T>(count);
            else
                value.Clear();
        }

        public static void ClearHashSet<T>(ref HashSet<T> value)
        {
            if (value == null)
                value = new HashSet<T>();
            else
                value.Clear();
        }
    }
}