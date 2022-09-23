//using System;
//using System.Collections.Generic;
//using UnityEngine;

//namespace Offworld.SystemCore
//{
//    public class RemoteSettingsHelper
//    {

//        public static event RemoteSettingsEventUpdated Updated;
//        private static bool hasUpdated = false;

//        public static bool HasUpdated()
//        {
//            return hasUpdated;
//        }

//        public static void CallOnUpdate()
//        {
//            RemoteSettings.CallOnUpdate();
//        }

//        public static bool GetBool(string key, bool defaultResponse = false)
//        {
//            if (!hasUpdated)
//            {
//                Debug.LogWarning("[Remote Settings] hasn't updated, using default value: " + defaultResponse + " for key: " + key);
//                return defaultResponse;
//            }
//            if (!RemoteSettings.HasKey(key))
//            {
//                Debug.LogWarning("[Remote Settings] key:" + key + " doesn't exist, using default value: " + defaultResponse + " instead");
//                return defaultResponse;
//            }
//            return RemoteSettings.GetBool(key, defaultResponse);
//        }

//        public static float GetFloat(string key, float defaultResponse = 0.0f)
//        {
//            if (!hasUpdated)
//            {
//                Debug.LogWarning("[Remote Settings] hasn't updated, using default value: " + defaultResponse + "f for key: " + key);
//                return defaultResponse;
//            }
//            if (!RemoteSettings.HasKey(key))
//            {
//                Debug.LogWarning("[Remote Settings] key:" + key + " doesn't exist, using default value: " + defaultResponse + "f instead");
//                return defaultResponse;
//            }
//            return RemoteSettings.GetFloat(key, defaultResponse);
//        }

//        public static int GetInt(string key, int defaultResponse = 0)
//        {
//            if (!hasUpdated)
//            {
//                Debug.LogWarning("[Remote Settings] hasn't updated, using default value: " + defaultResponse + " for key: " + key);
//                return defaultResponse;
//            }
//            if (!RemoteSettings.HasKey(key))
//            {
//                Debug.LogWarning("[Remote Settings] key:" + key + " doesn't exist, using default value: " + defaultResponse + " instead");
//                return defaultResponse;
//            }
//            return RemoteSettings.GetInt(key, defaultResponse);
//        }

//        public static string GetString(string key, string defaultResponse = "")
//        {
//            if (!hasUpdated)
//            {
//                Debug.LogWarning("[Remote Settings] hasn't updated, using default value: \"" + defaultResponse + "\" for key: " + key);
//                return defaultResponse;
//            }
//            if (!RemoteSettings.HasKey(key))
//            {
//                Debug.LogWarning("[Remote Settings] key:" + key + " doesn't exist, using default value: \"" + defaultResponse + "\" instead");
//                return defaultResponse;
//            }
//            return RemoteSettings.GetString(key, defaultResponse);
//        }

//        public static int GetCount()
//        {
//            if (!hasUpdated)
//            {
//                Debug.LogWarning("[Remote Settings] hasn't updated, can't get count");
//                return -1;
//            }
//            return RemoteSettings.GetCount();
//        }

//        public static bool HasKey(string key)
//        {
//            if (!hasUpdated)
//            {
//                Debug.LogWarning("[Remote Settings] hasn't updated, can't check if key exists");
//                return false;
//            }
//            return RemoteSettings.HasKey(key);
//        }

//        public static string[] GetKeys()
//        {
//            if (!hasUpdated)
//            {
//                Debug.LogWarning("[Remote Settings] hasn't updated, can't get keys");
//                return new string[] { };
//            }
//            return RemoteSettings.GetKeys();
//        }
        
//        public static void AddAction(RemoteSettingsEventUpdated action)
//        {
//            if (action != null)
//            {
//                Updated -= action;
//                Updated += action;
//            }
//        }

//        public static void RemoveAction(RemoteSettingsEventUpdated action)
//        {
//            if (action != null)
//            {
//                Updated -= action;
//                Updated += action;
//            }
//        }

//        private static void RemoteSettingsUpdated()
//        {
//            UnityEngine.Debug.Log("[Remote Settings] Updated remote settings");
//            hasUpdated = true;
//            Updated();
//        }

//        public static void Init()
//        {
//            Updated = null;
//            RemoteSettings.Updated += RemoteSettingsUpdated;
//        }


//        public delegate void RemoteSettingsEventUpdated();

//    }
//}