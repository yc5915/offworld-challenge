using System;
using System.IO;

namespace UnityEngine
{
    public class Debug
    {
        private static void _Log(object message, string level) =>
            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff")} - [{level}] - {message}");
        public static void Log(object message) => _Log(message, "DEBUG");
        public static void LogError(object message) => _Log(message, "ERROR");
        public static void LogWarning(object message) => _Log(message, "WARNING");
        public static void LogException(object message) => _Log(message, "EXCEPTION");
    }
}
