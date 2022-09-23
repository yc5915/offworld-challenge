using UnityEngine;
using System.Collections.Generic;

namespace Offworld.SystemCore
{
    //modified from http://stackoverflow.com/questions/166089/what-is-c-sharp-analog-of-c-stdpair
    public class Pair<T, U>
    {
        public T First { get; set; }
        public U Second { get; set; }

        public Pair()
        {
        }
        
        public Pair(T first, U second) 
        {
            this.First = first;
            this.Second = second;
        }

        //modified from http://stackoverflow.com/questions/2080394/c-sharp-implementing-iequatablet-equalt
        public override bool Equals(object obj)
        {
            Pair<T, U> other = obj as Pair<T, U>;
            return (other != null) && First.Equals(other.First) && Second.Equals(other.Second);
        }

        public override int GetHashCode()
        {
            int firstHash = (First != null) ? First.GetHashCode() : 0;
            int secondHash = (Second != null) ? Second.GetHashCode() : 0;
            return firstHash ^ secondHash;
        }
    }

    public static class Pair
    {
        public static Pair<T, U> Create<T, U>(T first, U second)
        {
            return new Pair<T, U>(first, second);
        }
    }
}