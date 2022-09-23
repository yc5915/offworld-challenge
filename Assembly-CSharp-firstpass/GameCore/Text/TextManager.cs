using UnityEngine;
using UnityEngine.Assertions;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Offworld.SystemCore;

namespace Offworld.GameCore.Text
{
    [System.Flags]
    public enum NumberFormatOptions
    {
        NONE            = 0,
        SHOW_CURRENCY   = 1 << 1,
        SHOW_SIGN       = 1 << 2,
        SHOW_THOUSAND   = 1 << 3,
        SHOW_MILLION    = 1 << 4,
        SHOW_PERCENT    = 1 << 5,
        NO_DIGIT_GROUPING = 1 << 6,
        
        SHOW_SIGN_AND_CURRENCY = SHOW_SIGN | SHOW_CURRENCY,
        SHOW_SIGN_AND_PERCENT = SHOW_SIGN | SHOW_PERCENT
    }

    public class TextManager : IInfosListener
    {
        private Infos infos;
        private List<ParsedInfoText> parsedTextInfos;
        private List<ParsedInfoLanguage> parsedLanguageInfos;
        private static LanguageType currentLanguage = LanguageType.ENGLISH;
        private static ParsedInfoLanguage currentLanguageInfo = new ParsedInfoLanguage();
        private static TextManager gInstance = null;

        public static LanguageType CurrentLanguage
        {
            get { return currentLanguage; }
            set { SetCurrentLanguage(value); }
        }

        public static ParsedInfoLanguage CurrentLanguageInfo
        {
            get { return currentLanguageInfo; }
            set { currentLanguageInfo = value; }
        }

        //helper functions
        public static string TEXT(string key)
        {
            if (Globals.TextManager == null)
                return "UNINITIALIZED";

            return Globals.TextManager.GetText(key);
        }
        
        public static string TEXT(TextType type)
        {
            return Globals.TextManager.GetText(type);
        }
        
        public static string TEXT(string key, params TextVariable [] arguments)
        {
            StringBuilder output = Globals.PoolManager.AcquireScratchStringBuilder();
            return Globals.TextManager.GetText(output, key, arguments).ToString();
        }
        
        public static string TEXT(TextType type, params TextVariable [] arguments)
        {
            StringBuilder output = Globals.PoolManager.AcquireScratchStringBuilder();
            return Globals.TextManager.GetText(output, type, arguments).ToString();
        }

        //instance functions
        public TextManager(Infos infos)
        {
            gInstance = this;
            this.infos = infos;
            infos.AddListener(this);
            OnInfosLoaded(); //call first time
        }

        public ParsedInfoText GetParsedTextInfo(TextType textType)
        {
            return parsedTextInfos [(int)textType];
        }

        public void ParseAllTextInfos()
        {
            using (new UnityProfileScope("TextManager.ParseAllTextInfos"))
            {
                foreach (InfoLanguage language in infos.languages())
                {
                    parsedTextInfos.ForEach(x => x.GetEntry(language.meType).Parse());
                }
            }
        }

        public void OnInfosLoaded()
        {
            using (new UnityProfileScope("TextManager.OnInfosLoaded"))
            {
                ParseSubstitutionInfos();
                ParseTextInfos();
                ParseLanguageInfos();
                UpdateCurrentLanguageInfo();
            }
        }

        private void ParseSubstitutionInfos()
        {
            try
            {
                Dictionary<string, string> substitutions = new Dictionary<string, string>();

                //add color substitutions
                infos.colors().ForEach(x => substitutions.Add(x.mzType, "#" + x.mzColorCode));

                //add markup substitutions
                infos.markups().ForEach(x => substitutions.Add(x.mzTag, x.mzOpening));
                infos.markups().ForEach(x => substitutions.Add("/" + x.mzTag, x.mzClosing));

                //assign
                TextEntryGlobals.Substitutions = substitutions;
            }
            catch(System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void ParseTextInfos()
        {
            using (new UnityProfileScope("TextManager.ParseTextInfos"))
            {
                //initialize array
                parsedTextInfos = infos.texts()
                        .Select(info => new ParsedInfoText(info))
                        .ToList();

                //store the references in the globals
                TextEntryGlobals.ParsedTextInfos = parsedTextInfos.ToDictionary(x => x.Name, x => x);
            }
        }

        private void ParseLanguageInfos()
        {
            using (new UnityProfileScope("TextManager.ParseLanguageInfos"))
            {
                parsedLanguageInfos = infos.languages()
                        .Select(info => new ParsedInfoLanguage(info))
                        .ToList();
            }
        }

        private static void SetCurrentLanguage(LanguageType language)
        {
            if(currentLanguage != language)
            {
                currentLanguage = language;
                Debug.Log("[Text] SetLanguage: " + currentLanguage);
                if(gInstance != null)
                    gInstance.UpdateCurrentLanguageInfo();
            }
        }

        private void UpdateCurrentLanguageInfo()
        {
            using (new UnityProfileScope("TextManager.UpdateCurrentLanguageInfo"))
            {
                currentLanguageInfo = parsedLanguageInfos[(int)currentLanguage];
            }
        }

        //simple text with no arguments
        public string GetText(string textType)
        {
            TextType type = infos.getType<TextType>(textType);
            return GetText(type);
        }

        //simple text with no arguments
        public string GetText(TextType textType)
        {
            if(!MathUtilities.InRange((int)textType, 0, parsedTextInfos.Count - 1))
                return "TEXT_TYPE_NONE";
            
            ParsedInfoText textInfo = parsedTextInfos[(int)textType];
            string text = textInfo.Evaluate(currentLanguage, 0);
            return text;
        }

        //more expensive text with argument substitution
        public StringBuilder GetText(StringBuilder output, string textType, params TextVariable [] arguments)
        {
            TextType type = infos.getType<TextType>(textType);
            return GetText(output, type, arguments);
        }

        //more expensive text with argument substitution
        public StringBuilder GetText(StringBuilder output, TextType textType, params TextVariable [] arguments)
        {
            if (textType == TextType.NONE)
            {
                return output.Append("TEXT_TYPE_NONE");
            }

            if((arguments.Length > 0) && (currentLanguage == LanguageType.TEXT_DEBUG)) //debug display
            {
                string [] parameters = arguments.Select(o => o.Evaluate(currentLanguage, 0)).ToArray();
                string parameterString = string.Join(",", parameters);
                ParsedInfoText textInfo = parsedTextInfos[(int)textType];
                return output.AppendFormat("{0}({1})", textInfo.Evaluate(currentLanguage, 0), parameterString);
            }
            else //regular case
            {
                ParsedInfoText textInfo = parsedTextInfos[(int)textType];
                return textInfo.Evaluate(output, currentLanguage, 0, arguments);
            }
        }
    }

    public class ParsedInfoText
    {
        private string mzName = "";
        private TextEntry [] mEntries = null;

        public string Name { get { return mzName; } }
        
        public ParsedInfoText(InfoText info)
        {
            Initialize(info.mzType, info.mazEntries.ToArray());
        }

        public ParsedInfoText(string zName, string zEnglish)
        {
            Initialize(zName, new string [] {zEnglish});
        }

        private void Initialize(string zName, string [] zEntries)
        {
            Assert.IsTrue((zEntries != null) && (zEntries.Length > 0));
            mzName = zName;
            mEntries = new TextEntry [zEntries.Length];
            for (int i=0; i<mEntries.Length; i++)
            {
                mEntries[i] = new TextEntry(zEntries[i], (LanguageType) i);
            }
        }

        public string Evaluate(LanguageType language, int index)
        {
            //debug
            if(language == LanguageType.TEXT_DEBUG)
                return mzName.Replace("TEXT_", "*");

            TextEntry entry = GetEntry(language);
            return entry.Evaluate(index);
        }

        public StringBuilder Evaluate(StringBuilder output, LanguageType language, int index, TextVariable [] arguments)
        {
            TextEntry entry = GetEntry(language);
            return entry.Evaluate(output, index, arguments);
        }

        public GenderType GetGender(LanguageType language)
        {
            TextEntry entry = GetEntry(language);
            return entry.Gender;
        }

        public TextEntry GetEntry(LanguageType language)
        {
            int value = (int) language;
            if (!MathUtilities.InRange(value, 0, mEntries.Length - 1))
                value = 0;

            return mEntries [value];
        }
    }

    public class ParsedInfoLanguage
    {
        private System.Predicate<long> singularPredicate;
        private System.Predicate<long> pluralPredicate;
        private string thousandsSeparator = "";
        private string decimalSeparator = "";
        private string currencyPrefix = "";
        private string currencyPostfix = "";
        private string thousandPostfix = "";
        private string millionPostfix = "";
        private StringBuilder tempBuffer = new StringBuilder();
        private Stack<int> tempDigitGroupings = new Stack<int>();

        public ParsedInfoLanguage()
        {
            Initialize("x==1", "", ",", ".", "$", "", "K", "M");
        }

        public ParsedInfoLanguage(InfoLanguage info)
        {
            Initialize(info.mzSingularExpression, info.mzPluralExpression, info.mzThousandsSeparator, info.mzDecimalSeparator, info.mzCurrencyPrefix, info.mzCurrencyPostfix, info.mzThousandPostfix, info.mzMillionPostfix);
        }

        public ParsedInfoLanguage(string zSingularExpression, string zPluralExpression, string zThousandsSeparator, string zDecimalSeparator, string zCurrencyPrefix, string zCurrencyPostfix, string zThousandPostfix, string zMillionPostfix)
        {
            Initialize(zSingularExpression, zPluralExpression, zThousandsSeparator, zDecimalSeparator, zCurrencyPrefix, zCurrencyPostfix, zThousandPostfix, zMillionPostfix);
        }

        private void Initialize(string zSingularExpression, string zPluralExpression, string zThousandsSeparator, string zDecimalSeparator, string zCurrencyPrefix, string zCurrencyPostfix, string zThousandPostfix, string zMillionPostfix)
        {
            singularPredicate = ParsePredicate(zSingularExpression);
            pluralPredicate = ParsePredicate(zPluralExpression);
            thousandsSeparator = zThousandsSeparator;
            decimalSeparator = zDecimalSeparator;
            currencyPrefix = zCurrencyPrefix;
            currencyPostfix = zCurrencyPostfix;
            thousandPostfix = zThousandPostfix;
            millionPostfix = zMillionPostfix;
        }

        private System.Predicate<long> ParsePredicate(string zExpression)
        {
            System.Predicate<long> defaultPredicate = x => true;
            if(string.IsNullOrEmpty(zExpression))
                return defaultPredicate;

            string error;
            System.Predicate<long> newPredicate = MDynamicExpression.ParsePredicate<long>(zExpression, "x", out error);
            if(newPredicate == null)
            {
                Debug.LogError("Error parsing language expression (" + zExpression + "): " + error);
                return defaultPredicate;
            }

            return newPredicate;
        }

        public PluralType GetPluralType(long value)
        {
            if(singularPredicate(value))
                return PluralType.SINGULAR;
            else if(pluralPredicate(value))
                return PluralType.PLURAL;
            else
                return PluralType.PLURAL2;
        }

        public string FormatNumber(long value, NumberFormatOptions options)
        {
            //trivial case
            if((options == NumberFormatOptions.NONE) && (Mathf.Abs(value) < 1000))
                return value.ToString();

            tempBuffer.Clear();

            //sign
            if (value < 0)
            {
                tempBuffer.Append('-');
                value *= -1;
            }
            else if ((options & NumberFormatOptions.SHOW_SIGN) != 0)
            {
                tempBuffer.Append('+');
            }

            //currency
            if ((options & NumberFormatOptions.SHOW_CURRENCY) != 0)
                tempBuffer.Append(currencyPrefix);

            //digit groupings
            AppendDigitGroupings(tempBuffer, value, options);

            //thousands and millions
            if ((options & NumberFormatOptions.SHOW_THOUSAND) != 0)
                tempBuffer.Append(thousandPostfix);
            else if ((options & NumberFormatOptions.SHOW_MILLION) != 0)
                tempBuffer.Append(millionPostfix);

            //currency
            if ((options & NumberFormatOptions.SHOW_CURRENCY) != 0)
                tempBuffer.Append(currencyPostfix);

            if ((options & NumberFormatOptions.SHOW_PERCENT) != 0)
                tempBuffer.Append("%");

            return tempBuffer.ToString();
        }

        public string FormatNumber(float value, int minDecimals, int maxDecimals, NumberFormatOptions options)
        {
            //trivial case
            minDecimals = Mathf.Clamp(minDecimals, 0, maxDecimals);
            if (maxDecimals <= 0)
                return FormatNumber(Mathf.RoundToInt(value), options);

            //regular decimal numbers
            tempBuffer.Clear();

            //sign
            if (value < 0)
            {
                tempBuffer.Append('-');
                value *= -1;
            }
            else if ((options & NumberFormatOptions.SHOW_SIGN) != 0)
            {
                tempBuffer.Append('+');
            }

            //currency
            if ((options & NumberFormatOptions.SHOW_CURRENCY) != 0)
                tempBuffer.Append(currencyPrefix);

            //decimals
            int whole = (int)value;
            int multiplier = MathUtilities.Pow(10, maxDecimals);
            int fraction = Mathf.RoundToInt((value - whole) * multiplier);
            if (fraction >= multiplier) //rounding 0.999 up to 1.000
            {
                whole++;
                fraction -= multiplier;
            }

            while ((maxDecimals > minDecimals) && (fraction % 10 == 0)) //trim trailing zeros
            {
                maxDecimals--;
                fraction /= 10;
            }
            AppendDigitGroupings(tempBuffer, whole, options);
            tempBuffer.Append(decimalSeparator);
            PadZerosLeft(tempBuffer, fraction, maxDecimals);

            //thousands and millions
            if ((options & NumberFormatOptions.SHOW_THOUSAND) != 0)
                tempBuffer.Append(thousandPostfix);
            else if ((options & NumberFormatOptions.SHOW_MILLION) != 0)
                tempBuffer.Append(millionPostfix);

            //currency
            if ((options & NumberFormatOptions.SHOW_CURRENCY) != 0)
                tempBuffer.Append(currencyPostfix);

            return tempBuffer.ToString();
        }

        //assumes value >= 0
        private void AppendDigitGroupings(StringBuilder output, long value, NumberFormatOptions options)
        {
            Assert.IsTrue(value >= 0);

            //no digit groups
            if((options & NumberFormatOptions.NO_DIGIT_GROUPING) != 0)
            {
                output.Append(value);
                return;
            }

            //edge case
            if (value == 0)
            {
                output.Append('0');
                return;
            }

            tempDigitGroupings.Clear();
            while (value > 0)
            {
                tempDigitGroupings.Push((int)(value % 1000));
                value /= 1000;
            }

            //the first digit grouping doesn't get padded
            output.Append(tempDigitGroupings.Pop());
            while (tempDigitGroupings.Count > 0)
            {
                //pad remaining digit groupings with 0's
                int number = tempDigitGroupings.Pop();
                output.Append(thousandsSeparator);
                PadZerosLeft(output, number, 3);
            }                            
        }

        //assumes value >= 0
        private static void PadZerosLeft(StringBuilder output, int value, int length)
        {
            Assert.IsTrue(value >= 0);

            int digits = CountDigits(value);
            output.Append('0', Mathf.Max(length - digits, 0));
            output.Append(value);
        }

        //assumes value >= 0
        private static int CountDigits(int value)
        {
            Assert.IsTrue(value >= 0);

            int digits = 1;
            while (value >= 10)
            {
                digits++;
                value /= 10;
            }
            return digits;
        }
    }
}