using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Offworld.GameCore;
using Offworld.SystemCore;

namespace Offworld.GameCore.Text
{
    public class TextVariable
    {
        private enum TextVariableType
        {
            STRING,
            INTEGER,
            FLOAT,
            BOOL,
            TEXT_INFO,
        }

        private TextVariableType meType = TextVariableType.STRING;
        private string mzValue = "";
        private long miValue = 0;
        private float mfValue = 0.0f;
        private ParsedInfoText mInfo = null;
        private bool mbValue = true;
        private GenderType meGender = GenderType.MALE;
        private PluralType mePlural = PluralType.SINGULAR;
        private TextVariable[] mArguments = null;

        public TextVariable(string value, PluralType plural)
        {
            meType = TextVariableType.STRING;
            mzValue = value;
            mePlural = plural;
        }

        public TextVariable(string value, GenderType gender)
        {
            meType = TextVariableType.STRING;
            mzValue = value;
            meGender = gender;
        }

        public TextVariable(long value, PluralType plural, string text)
        {
            meType = TextVariableType.INTEGER;
            miValue = value;
            mePlural = plural;
            mzValue = text;
        }

        public TextVariable(float value, string text)
        {
            meType = TextVariableType.FLOAT;
            mfValue = value;
            mzValue = text;
        }

        public TextVariable(bool value)
        {
            meType = TextVariableType.BOOL;
            mbValue = value;
        }

        public TextVariable(ParsedInfoText info, TextVariable [] arguments)
        {
            meType = TextVariableType.TEXT_INFO;
            mInfo = info;
            mArguments = arguments;
        }

        public string Evaluate(LanguageType language, int index)
        {
            switch (meType)
            {
                case TextVariableType.STRING:               return mzValue;
                case TextVariableType.INTEGER:              return mzValue;
                case TextVariableType.FLOAT:                return mzValue;
                case TextVariableType.BOOL:                 return mbValue ? "0" : "1";
                case TextVariableType.TEXT_INFO:            return EvaluateTextInfoString(mInfo, mArguments, language, index);
                default: MAssert.Unimplemented();           return ""; //unimplemented
            }
        }

        public StringBuilder Evaluate(StringBuilder output, LanguageType language, int index)
        {
            switch (meType)
            {
                case TextVariableType.STRING:               return output.Append(mzValue);
                case TextVariableType.INTEGER:              return output.Append(mzValue);
                case TextVariableType.FLOAT:                return output.Append(mzValue);
                case TextVariableType.BOOL:                 return output.Append(mbValue ? "0" : "1");
                case TextVariableType.TEXT_INFO:            return (mInfo != null) ? mInfo.Evaluate(output, language, index, mArguments) : output.Append("NULL_TEXT_INFO");
                default: MAssert.Unimplemented();           return output; //unimplemented
            }
        }

        //helper function to handle the different cases
        private static string EvaluateTextInfoString(ParsedInfoText info, TextVariable [] arguments, LanguageType language, int index)
        {
            if (info == null)
                return "NULL_TEXT_INFO";
            else if (arguments == null)
                return info.Evaluate(language, index);
            else
                return info.Evaluate(new StringBuilder(), language, index, arguments).ToString();
        }

        public bool IsTrue()
        {
            switch (meType)
            {
                case TextVariableType.STRING:               return mbValue;
                case TextVariableType.INTEGER:              return (miValue != 0);
                case TextVariableType.FLOAT:                return (mfValue != 0.0f);
                case TextVariableType.BOOL:                 return mbValue;
                case TextVariableType.TEXT_INFO:            return true; //unimplemented
                default: MAssert.Unimplemented();           return true; //unimplemented
            }
        }

        public GenderType GetGender(LanguageType language)
        {
            switch (meType)
            {
                case TextVariableType.STRING:               return meGender;
                case TextVariableType.INTEGER:              return GenderType.MALE; //unimplemented
                case TextVariableType.FLOAT:                return GenderType.MALE; //unimplemented
                case TextVariableType.BOOL:                 return GenderType.MALE; //unimplemented
                case TextVariableType.TEXT_INFO:            return (mInfo != null) ? mInfo.GetGender(language) : GenderType.MALE;
                default: MAssert.Unimplemented();           return GenderType.MALE; //unimplemented
            }
        }

        public PluralType GetPluralType()
        {
            switch (meType)
            {
                case TextVariableType.STRING:               return mePlural;
                case TextVariableType.INTEGER:              return mePlural;
                case TextVariableType.FLOAT:                return PluralType.SINGULAR; //unimplemented
                case TextVariableType.BOOL:                 return PluralType.SINGULAR; //unimplemented
                case TextVariableType.TEXT_INFO:            return PluralType.SINGULAR; //unimplemented
                default: MAssert.Unimplemented();           return PluralType.SINGULAR; //unimplemented
            }
        }
    }
}