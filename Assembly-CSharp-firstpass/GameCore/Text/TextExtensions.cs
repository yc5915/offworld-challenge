using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Offworld.GameCore;

namespace Offworld.GameCore.Text
{
    public static class TextExtensions
    {
        public static TextVariable ToText(this int value)
        {
            return new TextVariable(value, TextManager.CurrentLanguageInfo.GetPluralType(value), TextManager.CurrentLanguageInfo.FormatNumber(value, NumberFormatOptions.NONE));
        }

        public static TextVariable ToText(this int value, NumberFormatOptions options)
        {
            return new TextVariable(value, TextManager.CurrentLanguageInfo.GetPluralType(value), TextManager.CurrentLanguageInfo.FormatNumber(value, options));
        }

        public static TextVariable ToText(this long value, NumberFormatOptions options)
        {
            return new TextVariable(value, TextManager.CurrentLanguageInfo.GetPluralType(value), TextManager.CurrentLanguageInfo.FormatNumber(value, options));
        }

        public static TextVariable ToText(this float value, int minDecimals, int maxDecimals, NumberFormatOptions options)
        {
            return new TextVariable(value, TextManager.CurrentLanguageInfo.FormatNumber(value, minDecimals, maxDecimals, options));
        }

        public static TextVariable ToText(this string value)
        {
            return new TextVariable(value, PluralType.SINGULAR);
        }

        public static TextVariable ToText(this string value, PluralType plural)
        {
            return new TextVariable(value, plural);
        }

        public static TextVariable ToText(this string value, GenderType gender)
        {
            return new TextVariable(value, gender);
        }

        public static TextVariable ToText(this bool value)
        {
            return new TextVariable(value);
        }

        public static TextVariable ToText(this TextType value)
        {
            ParsedInfoText info = Globals.TextManager.GetParsedTextInfo(value);
            return new TextVariable(info, null);
        }

        public static TextVariable ToText(this TextType value, params TextVariable [] arguments)
        {
            ParsedInfoText info = Globals.TextManager.GetParsedTextInfo(value);
            return new TextVariable(info, arguments);
        }

        public static TextVariable ToText(this ParsedInfoText value, params TextVariable [] arguments)
        {
            return new TextVariable(value, arguments);
        }
    }

    //inherit from this class, or the serializable version for serializable classes, in order to call TEXT() more easily
    public class TextHelpers
    {
        public static string TEXT(string key)                               { return TextManager.TEXT(key);             }
        public static string TEXT(TextType type)                            { return TextManager.TEXT(type);            }
        public static string TEXT(string key, params TextVariable [] arguments)   { return TextManager.TEXT(key, arguments);  }
        public static string TEXT(TextType type, params TextVariable [] arguments){ return TextManager.TEXT(type, arguments); }
        public static PluralType GetPluralType(int value)                   { return TextManager.CurrentLanguageInfo.GetPluralType(value); }
    }

    [System.Serializable]
    public class TextHelpersSerializable
    {
        public static string TEXT(string key)                               { return TextManager.TEXT(key);             }
        public static string TEXT(TextType type)                            { return TextManager.TEXT(type);            }
        public static string TEXT(string key, params TextVariable [] arguments)   { return TextManager.TEXT(key, arguments);  }
        public static string TEXT(TextType type, params TextVariable [] arguments){ return TextManager.TEXT(type, arguments); }
        public static PluralType GetPluralType(int value)                   { return TextManager.CurrentLanguageInfo.GetPluralType(value); }
    }
}