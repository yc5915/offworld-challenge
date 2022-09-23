using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Offworld.SystemCore
{
    public static class ListUtilities
    {
        public static bool Equals<T>(List<T> lhs, List<T> rhs)
        {
            if(lhs.Count != rhs.Count)
                return false;

            for(int i=0; i<lhs.Count; i++)
            {
                T value1 = lhs[i];
                T value2 = rhs[i];
                if((value1 == null) && (value2 != null))
                   return false;

                if(!value1.Equals(value2))
                    return false;
            }
            
            return true;
        }

        public static bool Any<T>(List<T> list, System.Predicate<T> predicate)
        {
            for (int i=0; i<list.Count; i++)
            {
                if(predicate(list[i]))
                    return true;
            }
            return false;
        }

        //determines the hashCode based solely on the contents of the list
        public static int GetHashCode<T>(List<T> list)
        {
            //http://stackoverflow.com/questions/8094867/good-gethashcode-override-for-list-of-foo-objects
            int result = 0x2D2816FE;
            foreach(T item in list)
            {
                result = result * 31 + (item == null ? 0 : item.GetHashCode());
            }
            return result;
        }

        public static string ToString<T>(List<T> list)
        {
            return "{" + string.Join(", ", list.Select(x => x.ToString()).ToArray()) + "}";
        }

        public static void Swap<T>(List<T> list, int i, int j)
        {
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }

        public static List<T> Resize<T>(this List<T> list, int newSize, T defaultValue)
        {
            //shrink
            while (list.Count > newSize)
                list.RemoveAt(list.Count - 1);

            //grow
            while (list.Count < newSize)
                list.Add(defaultValue);

            return list;
        }

        public static List<T> Fill<T>(this List<T> list, T value)
        {
            for (int i=0; i<list.Count; i++)
                list[i] = value;
            return list;
        }

        public static bool IsEmpty<T>(this List<T> list)
        {
            return (list.Count == 0);
        }

        public static T Last<T>(this List<T> list)
        {
            if((list == null) || list.IsEmpty())
                return default(T);
            else
                return list[list.Count - 1];
        }

        public static T Last<T>(this List<T> list, T defaultValue)
        {
            if((list == null) || list.IsEmpty())
                return defaultValue;
            else
                return list[list.Count - 1];
        }

        public static T GetOrDefault<T>(this List<T> list, int index)
        {
            return ((index >= 0) && (index < list.Count)) ? list[index] : default(T);
        }

        public static void CopyTo<T>(this List<T> source, ref List<T> destination)
        {
            if(destination == null)
                destination = new List<T>();
            destination.Resize(source.Count, default(T));
            for(int i=0; i<source.Count; i++)
                destination[i] = source[i];
        }
    }
}