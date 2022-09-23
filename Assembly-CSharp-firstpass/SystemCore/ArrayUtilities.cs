using System.Linq;

namespace Offworld.SystemCore
{
    public static class ArrayUtilities
    {
        public static void Fill<T>(this T [] array, T value)
        {
            for (int i=0; i<array.Length; i++)
                array [i] = value;
        }

        public static void Fill<T>(this T [,] array, T value)
        {
            int length0 = array.GetLength(0);
            int length1 = array.GetLength(1);
            for(int i=0; i<length0; i++)
                for(int j=0; j<length1; j++)
                    array[i,j] = value;
        }

        public static void SetSize<T>(ref T [] array, int size)
        {
            if ((array == null) || (array.Length != size))
                array = new T [size];
        }

        public static void SetSize<T>(ref T [,] array, int size0, int size1)
        {
            if ((array == null) || (array.GetLength(0) != size0) || (array.GetLength(1) != size1))
                array = new T [size0, size1];
        }

        public static void Copy<T>(T [] source, T [] destination)
        {
            System.Array.Copy(source, destination, source.Length);
        }

        public static void Copy<T>(T [] source, int sourceIndex, T [] destination, int destinationIndex, int length)
        {
            System.Array.Copy(source, sourceIndex, destination, destinationIndex, length);
        }

        public static bool Equals<T>(T [] lhs, T [] rhs)
        {
            if(lhs == rhs)
                return true;

            if((lhs == null) || (rhs == null))
                return false;

            return lhs.SequenceEqual(rhs);
        }

        public static void Swap<T>(this T [] array, int index1, int index2)
        {
            T temp = array[index1];
            array[index1] = array[index2];
            array[index2] = temp;
        }

        public static bool IsEmpty<T>(this T [] array)
        {
            return (array.Length == 0);
        }

        public static T Last<T>(this T [] array)
        {
            if((array == null) || array.IsEmpty())
                return default(T);
            else
                return array[array.Length - 1];
        }

        public static T Last<T>(this T [] array, T defaultValue)
        {
            if((array == null) || array.IsEmpty())
                return defaultValue;
            else
                return array[array.Length - 1];
        }

        public static T GetOrDefault<T>(this T [] array, int index)
        {
            return ((index >= 0) && (index < array.Length)) ? array[index] : default(T);
        }
    }
}