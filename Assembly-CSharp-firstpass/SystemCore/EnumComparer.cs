using System;
using System.Collections.Generic;
using System.Linq.Expressions;

//Improves Dictionary<Enum, Value> performance by ~40x due to bypassing auto-boxing and unboxing
//modified from http://www.codeproject.com/Articles/33528/Accelerating-Enum-Based-Dictionaries-with-Generic
namespace Offworld.SystemCore
{
    public sealed class EnumComparer<TEnum> : IEqualityComparer<TEnum>
        where TEnum : struct, IComparable, IConvertible, IFormattable
    {
        private static readonly Func<TEnum, TEnum, bool> equals;
        private static readonly Func<TEnum, int> getHashCode;
        public static readonly EnumComparer<TEnum> Instance;

        static EnumComparer()
        {
            getHashCode = generateGetHashCode();
            equals = generateEquals();
            Instance = new EnumComparer<TEnum>();
        }

        private EnumComparer()
        {
            assertTypeIsEnum();
            assertUnderlyingTypeIsSupported();
        }

        public bool Equals(TEnum x, TEnum y)
        {
            return equals(x, y);
        }

        public int GetHashCode(TEnum obj)
        {
            return getHashCode(obj);
        }

        private static void assertTypeIsEnum()
        {
            if (typeof (TEnum).IsEnum)
                return;
            var message = string.Format("The type parameter {0} is not an Enum. LcgEnumComparer supports Enums only.", typeof (TEnum));
            throw new NotSupportedException(message);
        }

        private static void assertUnderlyingTypeIsSupported()
        {
            var underlyingType = Enum.GetUnderlyingType(typeof (TEnum));
            ICollection<Type> supportedTypes =
                new[]
            {
                typeof (byte), typeof (sbyte), typeof (short), typeof (ushort),
                typeof (int), typeof (uint), typeof (long), typeof (ulong)
            };
            if (supportedTypes.Contains(underlyingType))
                return;
            var message =
                string.Format("The underlying type of the type parameter {0} is {1}. " +
                              "LcgEnumComparer only supports Enums with underlying type of " +
                              "byte, sbyte, short, ushort, int, uint, long, or ulong.",
                              typeof (TEnum), underlyingType);
            throw new NotSupportedException(message);
        }

        private static Func<TEnum, TEnum, bool> generateEquals()
        {
            var xParam = Expression.Parameter(typeof(TEnum), "x");
            var yParam = Expression.Parameter(typeof(TEnum), "y");
            var equalExpression = Expression.Equal(xParam, yParam);
            return Expression.Lambda<Func<TEnum, TEnum, bool>>(equalExpression, new[] { xParam, yParam }).Compile();
        }

        private static Func<TEnum, int> generateGetHashCode()
        {
            var objParam = Expression.Parameter(typeof(TEnum), "obj");
            var underlyingType = Enum.GetUnderlyingType(typeof (TEnum));
            var convertExpression = Expression.Convert(objParam, underlyingType);
            var getHashCodeMethod = underlyingType.GetMethod("GetHashCode");
            var getHashCodeExpression = Expression.Call(convertExpression, getHashCodeMethod);
            return Expression.Lambda<Func<TEnum, int>>(getHashCodeExpression, new[] { objParam }).Compile();
        }
    }
}