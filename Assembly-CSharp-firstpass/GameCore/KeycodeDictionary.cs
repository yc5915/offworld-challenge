using UnityEngine;
using System.Collections.Generic;
using Offworld.GameCore.Text;
using Offworld.SystemCore;

namespace Offworld.GameCore
{
    public static class KeycodeDictionary 
    {
        public static Dictionary<string, KeyCode> keyCodes = new Dictionary<string, KeyCode>()
        {
            {"none", KeyCode.None},
            //Letter keys
            {"a", KeyCode.A},
            {"b", KeyCode.B},
            {"c", KeyCode.C},
            {"d", KeyCode.D},
            {"e", KeyCode.E},
            {"f", KeyCode.F},
            {"g", KeyCode.G},
            {"h", KeyCode.H},
            {"i", KeyCode.I},
            {"j", KeyCode.J},
            {"k", KeyCode.K},
            {"l", KeyCode.L},
            {"m", KeyCode.M},
            {"n", KeyCode.N},
            {"o", KeyCode.O},
            {"p", KeyCode.P},
            {"q", KeyCode.Q},
            {"r", KeyCode.R},
            {"s", KeyCode.S},
            {"t", KeyCode.T},
            {"u", KeyCode.U},
            {"v", KeyCode.V},
            {"w", KeyCode.W},
            {"x", KeyCode.X},
            {"y", KeyCode.Y},
            {"z", KeyCode.Z},
            //Number Keys
            {"0", KeyCode.Alpha0},
            {"1", KeyCode.Alpha1},
            {"2", KeyCode.Alpha2},
            {"3", KeyCode.Alpha3},
            {"4", KeyCode.Alpha4},
            {"5", KeyCode.Alpha5},
            {"6", KeyCode.Alpha6},
            {"7", KeyCode.Alpha7},
            {"8", KeyCode.Alpha8},
            {"9", KeyCode.Alpha9},
            {"keypad0", KeyCode.Keypad0},
            {"keypad1", KeyCode.Keypad1},
            {"keypad2", KeyCode.Keypad2},
            {"keypad3", KeyCode.Keypad3},
            {"keypad4", KeyCode.Keypad4},
            {"keypad5", KeyCode.Keypad5},
            {"keypad6", KeyCode.Keypad6},
            {"keypad7", KeyCode.Keypad7},
            {"keypad8", KeyCode.Keypad8},
            {"keypad9", KeyCode.Keypad9},
            {"[0]", KeyCode.Keypad0},
            {"[1]", KeyCode.Keypad1},
            {"[2]", KeyCode.Keypad2},
            {"[3]", KeyCode.Keypad3},
            {"[4]", KeyCode.Keypad4},
            {"[5]", KeyCode.Keypad5},
            {"[6]", KeyCode.Keypad6},
            {"[7]", KeyCode.Keypad7},
            {"[8]", KeyCode.Keypad8},
            {"[9]", KeyCode.Keypad9},
            //Characters
            {"backquote", KeyCode.BackQuote},
            {"equals", KeyCode.Equals},
            {"minus", KeyCode.Minus},
            {"left bracket", KeyCode.LeftBracket},
            {"right bracket", KeyCode.RightBracket},
            {"semicolon", KeyCode.Semicolon},
            {"quote", KeyCode.Quote},
            {"comma", KeyCode.Comma},
            {"period", KeyCode.Period},
            {"slash", KeyCode.Slash},
            {"backslash", KeyCode.Backslash},
            {"space", KeyCode.Space},
            {"keypadperiod", KeyCode.KeypadPeriod},
            {"keypaddivide", KeyCode.KeypadDivide},
            {"keypadmultiply", KeyCode.KeypadMultiply},
            {"keypadminus", KeyCode.KeypadMinus},
            {"keypadequals", KeyCode.KeypadEquals},
            {"[period]", KeyCode.KeypadPeriod},
            {"[divide]", KeyCode.KeypadDivide},
            {"[multiply]", KeyCode.KeypadMultiply},
            {"[minus]", KeyCode.KeypadMinus},
            {"[equals]", KeyCode.KeypadEquals},
            //Function Keys
            {"f1", KeyCode.F1},
            {"f2", KeyCode.F2},
            {"f3", KeyCode.F3},
            {"f4", KeyCode.F4},
            {"f5", KeyCode.F5},
            {"f6", KeyCode.F6},
            {"f7", KeyCode.F7},
            {"f8", KeyCode.F8},
            {"f9", KeyCode.F9},
            {"f10", KeyCode.F10},
            {"f11", KeyCode.F11},
            {"f12", KeyCode.F12},
            {"f13", KeyCode.F13},
            {"f14", KeyCode.F14},
            {"f15", KeyCode.F15},
            //Other
            {"shift", KeyCode.LeftShift},
            {"ctrl", KeyCode.LeftControl},
            {"alt", KeyCode.LeftAlt},
            {"cmd", KeyCode.LeftCommand},
            {"right shift", KeyCode.RightShift},
            {"right ctrl", KeyCode.RightControl},
            {"right alt", KeyCode.RightAlt},
            {"right cmd", KeyCode.RightCommand},
            {"escape", KeyCode.Escape},
            {"return", KeyCode.Return},
            {"backspace", KeyCode.Backspace},
            {"capslock", KeyCode.CapsLock},
            {"tab", KeyCode.Tab},
            {"print", KeyCode.Print},
            {"scroll lock", KeyCode.ScrollLock},
            {"pause", KeyCode.Pause},
            {"insert", KeyCode.Insert},
            {"home", KeyCode.Home},
            {"pageup", KeyCode.PageUp},
            {"pagedown", KeyCode.PageDown},
            {"page up", KeyCode.PageUp},
            {"page down", KeyCode.PageDown},
            {"delete", KeyCode.Delete},
            {"end", KeyCode.End},
            {"numlock", KeyCode.Numlock},
            {"up", KeyCode.UpArrow},
            {"down", KeyCode.DownArrow},
            {"left", KeyCode.LeftArrow},
            {"right", KeyCode.RightArrow}
        };

        public static Dictionary<KeyCode, string> keyCodeStrings = new Dictionary<KeyCode, string>(EnumComparer<KeyCode>.Instance)
        {
            {KeyCode.None, "TEXT_KEYCODE_NONE"},
            //Letter keys
            {KeyCode.A, "TEXT_KEYCODE_A"},
            {KeyCode.B, "TEXT_KEYCODE_B"},
            {KeyCode.C, "TEXT_KEYCODE_C"},
            {KeyCode.D, "TEXT_KEYCODE_D"},
            {KeyCode.E, "TEXT_KEYCODE_E"},
            {KeyCode.F, "TEXT_KEYCODE_F"},
            {KeyCode.G, "TEXT_KEYCODE_G"},
            {KeyCode.H, "TEXT_KEYCODE_H"},
            {KeyCode.I, "TEXT_KEYCODE_I"},
            {KeyCode.J, "TEXT_KEYCODE_J"},
            {KeyCode.K, "TEXT_KEYCODE_K"},
            {KeyCode.L, "TEXT_KEYCODE_L"},
            {KeyCode.M, "TEXT_KEYCODE_M"},
            {KeyCode.N, "TEXT_KEYCODE_N"},
            {KeyCode.O, "TEXT_KEYCODE_O"},
            {KeyCode.P, "TEXT_KEYCODE_P"},
            {KeyCode.Q, "TEXT_KEYCODE_Q"},
            {KeyCode.R, "TEXT_KEYCODE_R"},
            {KeyCode.S, "TEXT_KEYCODE_S"},
            {KeyCode.T, "TEXT_KEYCODE_T"},
            {KeyCode.U, "TEXT_KEYCODE_U"},
            {KeyCode.V, "TEXT_KEYCODE_V"},
            {KeyCode.W, "TEXT_KEYCODE_W"},
            {KeyCode.X, "TEXT_KEYCODE_X"},
            {KeyCode.Y, "TEXT_KEYCODE_Y"},
            {KeyCode.Z, "TEXT_KEYCODE_Z"},
            //Number Keys
            {KeyCode.Alpha0, "TEXT_KEYCODE_0"},
            {KeyCode.Alpha1, "TEXT_KEYCODE_1"},
            {KeyCode.Alpha2, "TEXT_KEYCODE_2"},
            {KeyCode.Alpha3, "TEXT_KEYCODE_3"},
            {KeyCode.Alpha4, "TEXT_KEYCODE_4"},
            {KeyCode.Alpha5, "TEXT_KEYCODE_5"},
            {KeyCode.Alpha6, "TEXT_KEYCODE_6"},
            {KeyCode.Alpha7, "TEXT_KEYCODE_7"},
            {KeyCode.Alpha8, "TEXT_KEYCODE_8"},
            {KeyCode.Alpha9, "TEXT_KEYCODE_9"},
            {KeyCode.Keypad0, "TEXT_KEYCODE_KEYPAD0"},
            {KeyCode.Keypad1, "TEXT_KEYCODE_KEYPAD1"},
            {KeyCode.Keypad2, "TEXT_KEYCODE_KEYPAD2"},
            {KeyCode.Keypad3, "TEXT_KEYCODE_KEYPAD3"},
            {KeyCode.Keypad4, "TEXT_KEYCODE_KEYPAD4"},
            {KeyCode.Keypad5, "TEXT_KEYCODE_KEYPAD5"},
            {KeyCode.Keypad6, "TEXT_KEYCODE_KEYPAD6"},
            {KeyCode.Keypad7, "TEXT_KEYCODE_KEYPAD7"},
            {KeyCode.Keypad8, "TEXT_KEYCODE_KEYPAD8"},
            {KeyCode.Keypad9, "TEXT_KEYCODE_KEYPAD9"},
            //Characters
            {KeyCode.BackQuote, "TEXT_KEYCODE_BACKQUOTE"},
            {KeyCode.Equals, "TEXT_KEYCODE_EQUALS"},
            {KeyCode.Minus, "TEXT_KEYCODE_MINUS"},
            {KeyCode.LeftBracket, "TEXT_KEYCODE_LEFT_BRACKET"},
            {KeyCode.RightBracket, "TEXT_KEYCODE_RIGHT_BRACKET"},
            {KeyCode.Semicolon, "TEXT_KEYCODE_SEMICOLON"},
            {KeyCode.Quote, "TEXT_KEYCODE_QUOTE"},
            {KeyCode.Comma, "TEXT_KEYCODE_COMMA"},
            {KeyCode.Period, "TEXT_KEYCODE_PERIOD"},
            {KeyCode.Slash, "TEXT_KEYCODE_SLASH"},
            {KeyCode.Backslash, "TEXT_KEYCODE_BACKSLASH"},
            {KeyCode.Space, "TEXT_KEYCODE_SPACE"},
            {KeyCode.KeypadPeriod, "TEXT_KEYCODE_KEYPADPERIOD"},
            {KeyCode.KeypadDivide, "TEXT_KEYCODE_KEYPADDIVIDE"},
            {KeyCode.KeypadMultiply, "TEXT_KEYCODE_KEYPADMULTIPLY"},
            {KeyCode.KeypadMinus, "TEXT_KEYCODE_KEYPADMINUS"},
            {KeyCode.KeypadEquals, "TEXT_KEYCODE_KEYPADEQUALS"},
            //Function Keys
            {KeyCode.F1, "TEXT_KEYCODE_F1"},
            {KeyCode.F2, "TEXT_KEYCODE_F2"},
            {KeyCode.F3, "TEXT_KEYCODE_F3"},
            {KeyCode.F4, "TEXT_KEYCODE_F4"},
            {KeyCode.F5, "TEXT_KEYCODE_F5"},
            {KeyCode.F6, "TEXT_KEYCODE_F6"},
            {KeyCode.F7, "TEXT_KEYCODE_F7"},
            {KeyCode.F8, "TEXT_KEYCODE_F8"},
            {KeyCode.F9, "TEXT_KEYCODE_F9"},
            {KeyCode.F10, "TEXT_KEYCODE_F10"},
            {KeyCode.F11, "TEXT_KEYCODE_F11"},
            {KeyCode.F12, "TEXT_KEYCODE_F12"},
            {KeyCode.F13, "TEXT_KEYCODE_F13"},
            {KeyCode.F14, "TEXT_KEYCODE_F14"},
            {KeyCode.F15, "TEXT_KEYCODE_F15"},
            //Other
            {KeyCode.LeftShift, "TEXT_KEYCODE_SHIFT"},
            {KeyCode.LeftControl, "TEXT_KEYCODE_CTRL"},
            {KeyCode.LeftAlt, "TEXT_KEYCODE_ALT"},
            {KeyCode.LeftCommand, "TEXT_KEYCODE_CMD"},
            {KeyCode.RightShift, "TEXT_KEYCODE_SHIFT"},
            {KeyCode.RightControl, "TEXT_KEYCODE_CTRL"},
            {KeyCode.RightAlt, "TEXT_KEYCODE_ALT"},
            {KeyCode.RightCommand, "TEXT_KEYCODE_CMD"},
            {KeyCode.Escape, "TEXT_KEYCODE_ESC"},
            {KeyCode.Return, "TEXT_KEYCODE_RETURN"},
            {KeyCode.Backspace, "TEXT_KEYCODE_BACKSPACE"},
            {KeyCode.CapsLock, "TEXT_KEYCODE_CAPSLOCK"},
            {KeyCode.Tab, "TEXT_KEYCODE_TAB"},
            {KeyCode.Print, "TEXT_KEYCODE_PRINT"},
            {KeyCode.ScrollLock, "TEXT_KEYCODE_SCOLLLOCK"},
            {KeyCode.Pause, "TEXT_KEYCODE_PAUSE"},
            {KeyCode.Insert, "TEXT_KEYCODE_INSERT"},
            {KeyCode.Home, "TEXT_KEYCODE_HOME"},
            {KeyCode.PageUp, "TEXT_KEYCODE_PAGEUP"},
            {KeyCode.PageDown, "TEXT_KEYCODE_PAGEDOWN"},
            {KeyCode.Delete, "TEXT_KEYCODE_DELETE"},
            {KeyCode.End, "TEXT_KEYCODE_END"},
            {KeyCode.Numlock, "TEXT_KEYCODE_NUMLOCK"},
            {KeyCode.UpArrow, "TEXT_KEYCODE_UP"},
            {KeyCode.DownArrow, "TEXT_KEYCODE_DOWN"},
            {KeyCode.LeftArrow, "TEXT_KEYCODE_LEFT"},
            {KeyCode.RightArrow, "TEXT_KEYCODE_RIGHT"}
        };

        public static Dictionary<KeyCode, string> keyCodeIDs = InverseKeyCodeDictionary();
    
        private  static Dictionary<KeyCode, string> InverseKeyCodeDictionary()
        {
            Dictionary<KeyCode, string> inverseDictionary = new Dictionary<KeyCode, string>();
            foreach(KeyValuePair<string, KeyCode> entry in KeycodeDictionary.keyCodes)
            {
                if(!inverseDictionary.ContainsKey(entry.Value))
                    inverseDictionary[entry.Value] = entry.Key;
            }
        
            return inverseDictionary;
        }

        public static KeyCode GetKeycode(string keyCodeString)
        {
            if (keyCodes.ContainsKey(keyCodeString))
            {
                return keyCodes[keyCodeString];
            }
            else
            {
                Debug.LogError("Invalid KeyCode: " + keyCodeString);
                return KeyCode.None;
            }
        }

        public static string GetKeyCodeID(KeyCode key)
        {
            if (keyCodeIDs.ContainsKey(key))
                return keyCodeIDs[key].ToUpperInvariant();
            return "";
        }

        public static string GetKeyCodeString(KeyCode key)
        {
            if (keyCodeStrings.ContainsKey(key))
                return TextHelpers.TEXT(keyCodeStrings[key]).ToUpperInvariant();
            return "";
        }
    }
}