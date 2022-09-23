using System;
using System.Linq.Expressions;

namespace Offworld.SystemCore
{
    //int value = CastTo<int>.From(eBuildingType); //casts an enum to an int (and vice-versa) without boxing
    //modified from https://stackoverflow.com/questions/1189144/c-sharp-non-boxing-conversion-of-generic-enum-to-int
    public static class CastTo<T>
    {
        public static T From<S>(S s)
        {
            return Cache<S>.caster(s);
        }    

        private static class Cache<S>
        {
            public static readonly Func<S, T> caster = Get();

            private static Func<S, T> Get()
            {
                var p = Expression.Parameter(typeof(S),"a");
                var c = Expression.ConvertChecked(p, typeof(T));
                return Expression.Lambda<Func<S, T>>(c, p).Compile();
            }
        }
    }
}