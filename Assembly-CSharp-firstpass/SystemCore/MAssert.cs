using System.Diagnostics;
using UnityEngine.Assertions;

namespace Offworld.SystemCore
{
    public static class MAssert
    {
        public static void Unimplemented()
        {
            Assert.IsTrue(false, "Codepath unimplemented!");
        }

        //used to temporarily fix unreferenced variable compile errors
        public static void Unreferenced<T>(T input)
        {
        }

        public static string GetCallStack()
        {
            return new StackTrace(1, true).ToString();
        }
    }
}