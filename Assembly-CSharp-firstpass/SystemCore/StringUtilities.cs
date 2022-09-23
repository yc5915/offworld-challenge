using System.Text;

namespace Offworld.SystemCore
{
    public static class StringUtilities
    {
        //modified from http://stackoverflow.com/questions/444798/case-insensitive-containsstring
        public static bool ContainsIgnoreCase(this string source, string search)
        {
            return (source.IndexOf(search, System.StringComparison.OrdinalIgnoreCase) >= 0);
        }

        //checks for null first
        public static string SafeToString(this object value)
        {
            return (value != null) ? value.ToString() : null;
        }

        public static void Clear(this StringBuilder value)
        {
            value.Length = 0;
        }
    }
}