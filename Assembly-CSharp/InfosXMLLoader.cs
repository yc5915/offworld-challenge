using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using Offworld.GameCore;
using Offworld.SystemCore;
using UnityEngine;

public class InfosXMLLoader : IInfosXMLLoader
{
    private static Dictionary<string, XmlDocument> defaultFileCache = new Dictionary<string, XmlDocument>();

    public bool IsModSet => false;

    public void ClearMod() { }

    public void ResetCache(bool resetDefaultXML) { }

    public XmlDocument GetDefaultXML(string resourceName)
    {
        using (new UnityProfileScope("ModPath::GetDefaultXML"))
        {
            using (new UnityProfileScope(resourceName))
            {
                if (!defaultFileCache.ContainsKey(resourceName))
                {
                    resourceName = Path.GetExtension(resourceName) == ".xml" ? resourceName : resourceName + ".xml";
                    TextAsset textAsset = Resources.Load<TextAsset>(resourceName);
                    if (textAsset != null)
                    {
                        try
                        {
                            XmlDocument xmlDocument = new XmlDocument();
                            using (new UnityProfileScope("LoadXmlDocument"))
                                xmlDocument.LoadXml(textAsset.text);
                            defaultFileCache[resourceName] = xmlDocument;
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError((object)("[Infos] Error loading " + resourceName + ": " + ex.ToString()));
                            throw;
                        }
                    }
                    else
                        defaultFileCache[resourceName] = (XmlDocument)null;
                }
                return defaultFileCache[resourceName];
            }
        }
    }

    public XmlDocument GetModdedXML(string resourceName, string extension) => null;

    public XmlDocument GetDLC_XML(string resourceName, string extension) => null;

    public List<XmlDocument> GetAllModdedXML(string resourceName) => new List<XmlDocument>();
    public XmlDocument GetDLC_Map_XML(string resourceName, string extension) => null;

    public void CopyDefaultXML(List<string> resourceNames) { }
}
