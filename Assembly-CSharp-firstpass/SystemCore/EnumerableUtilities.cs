using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace Offworld.SystemCore
{
    public static class EnumerableUtilities
    {
        public static readonly bool [] AllBools = {false, true};

        //modified from http://stackoverflow.com/questions/101265/why-is-there-not-a-foreach-extension-method-on-the-ienumerable-interface
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> values, System.Action<T> action)
        {
            foreach (T entry in values)
                action(entry);

            return values; //allow operations to be chained
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> values, System.Action<T,int> action)
        {
            int i = 0;
            foreach (T entry in values)
                action(entry,i++);
            
            return values; //allow operations to be chained
        }

        public static IEnumerable ForEach<T>(this IEnumerable values, System.Action<T> action)
        {
            foreach (T entry in values)
                action(entry);
            
            return values; //allow operations to be chained
        }

        public static Vector3 Sum(this IEnumerable<Vector3> values)
        {
            Vector3 result = Vector3.zero;
            values.ForEach(v => result += v);
            return result;
        }

        public static float Product(this IEnumerable<float> values)
        {
            float result = 1.0f;
            foreach(float value in values)
                result *= value;
            return result;
        }

        public static Vector3 Average(this IEnumerable<Vector3> values)
        {
            return values.Sum() / values.Count();
        }

        public static IEnumerable< Pair<A, B> > AllPairs<A, B>(this IEnumerable<A> a, IEnumerable<B> b)
        {
            foreach(A first in a)
                foreach(B second in b)
                    yield return Pair.Create(first, second);
        }

        public static string Join<T>(this IEnumerable<T> values, string separator)
        {
            //https://msdn.microsoft.com/en-us/library/system.text.stringbuilder(v=vs.90).aspx
            StringBuilder output = new StringBuilder();
            bool first = true;
            foreach(T entry in values)
            {
                if(first)
                    first = false;
                else
                    output.Append(separator);

                output.Append(entry.ToString());
            }
            return output.ToString();
        }

        public static string ToStringMohawk<T>(this IEnumerable<T> values, string seperator = ", ")
        {
            return "{" + values.Join(seperator) + "}";
        }

        public static IEnumerable<T> EnumValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public static bool IsEnumDefined<T>(T value)
        {
            return Enum.IsDefined(typeof(T), value);
        }
    }
}