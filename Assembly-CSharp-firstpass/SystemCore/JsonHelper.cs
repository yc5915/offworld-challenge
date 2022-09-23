//using System;
//using UnityEngine;

////https://forum.unity.com/threads/how-to-load-an-array-with-jsonutility.375735/
//namespace Offworld.SystemCore
//{
//    public static class JsonHelper
//    {
//        //JsonUtility doesn't natively support arrays as base type, so we wrap it into a helper object
//        public static T[] GetJsonArray<T>(string json)
//        {
//            string newJson = "{\"array\":" + json + "}";
//            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
//            return wrapper.array;
//        }

//        [Serializable]
//        private class Wrapper<T>
//        {
//            public T[] array = null;
//        }
//    }
//}