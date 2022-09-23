using System.IO;

namespace UnityEngine
{
    public class TextAsset
    {
        public string path { get; set; }
        public string name
        {
            get { return Path.GetFileName(path); }
        }
        public string text
        {
            get { return File.ReadAllText(path); }
        }
    }
    public class Resources
    {
        public static T Load<T>(string path) where T : TextAsset, new()
        {
            return File.Exists(path) ? new() { path = Path.GetFullPath(path) } : null;
        }

        public static T[] LoadAll<T>(string path) where T : TextAsset, new()
        {
            string[] paths = Directory.GetFiles(path);
            T[] assets = new T[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                assets[i] = Load<T>(paths[i]);
            }
            return assets;
        }
    }
}
