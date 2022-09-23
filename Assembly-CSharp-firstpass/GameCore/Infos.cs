using System.Xml;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;
using Offworld.SystemCore;

namespace Offworld.GameCore
{
    public interface XmlDataListItemBase
    {
        string GetFileName();
        void ClearTypes();
        void ReadTypes(XmlNodeList nodes, Infos infosClass, bool isAddedType, HashSet<string> removedTypes);
        void ReadData(XmlNodeList nodes, Infos infosClass, bool isAddedData, HashSet<string> removedTypes);
    }

    public class XmlDataListItem<T> : XmlDataListItemBase where T : InfoBase, new()
    {
        private string fileName;
        private List<T> infos;
        private HashSet<string> addedTypes;
        
        public delegate void readTypesDelegate(XmlNodeList nodes, List<T> infos, HashSet<string> removedTypes, bool isAddedType);
        private readTypesDelegate readTypesFunc;
        
        public XmlDataListItem(string fileName, readTypesDelegate readTypesFunc, ref List<T> output)
        {
            this.fileName = fileName;
            this.readTypesFunc = readTypesFunc;
            
            if(output == null)
                output = new List<T>();
            this.infos = output;
            this.addedTypes = new HashSet<string>();
        }
        
        public string GetFileName()
        {
            return fileName;
        }
        
        public void ClearTypes()
        {
            infos.Clear();
            addedTypes.Clear();
        }
        
        public void ReadTypes(XmlNodeList nodes, Infos infosClass, bool isAddedType, HashSet<string> removedTypes)
        {
            using (new UnityProfileScope("XmlDataListItem.ReadTypes"))
            {
                using (new UnityProfileScope(fileName))
                {
                    readTypesFunc(nodes, infos, removedTypes, isAddedType);
                    if (isAddedType)
                        nodes.ForEach<XmlNode>(node => addedTypes.Add(Infos.FindChild(node, "zType").InnerText));
                }
            }
        }
        
        public void ReadData(XmlNodeList nodes, Infos infosClass, bool isAddedData, HashSet<string> removedTypes)
        {
            using (new UnityProfileScope("XmlDataListItem.ReadData"))
            {
                using (new UnityProfileScope(fileName))
                {
                    foreach(XmlNode node in nodes)
                    {
                        string zType = Infos.FindChild(node, "zType").InnerText;
                        if (removedTypes.Contains(zType) || (!isAddedData && addedTypes.Contains(zType)))
                            continue;
                        int index = infosClass.getType<int>(zType);
                        infos[index].ReadData(node, infosClass);
                        infos[index].UpdateReferences(infosClass);
                    }
                }
            }
        }
    }

    public interface IInfosXMLLoader
    {
        bool IsModSet { get; }
        
        void ClearMod();
        void ResetCache(bool resetDefaultXML);
        XmlDocument GetDefaultXML(string resourceName);
        XmlDocument GetModdedXML(string resourceName, string extension);
        XmlDocument GetDLC_XML(string resourceName, string extension);
        List<XmlDocument> GetAllModdedXML(string resourceName);
        void CopyDefaultXML(List<string> resourceNames);
    }

    public class Infos
    {
        private IInfosXMLLoader             mXMLLoader;
        private Dictionary<string, int>     mTypeDictionary = new Dictionary<string, int>();
        private HashSet<string>             mRemovedXmlFields = new HashSet<string>();
        private List<XmlDataListItemBase>   mInfoList;
        private List<IInfosListener>        maListeners;
        
        private HashSet<string>             mReadXmlFields = new HashSet<string>();
        private bool                        mbSuppressError = true;

        private List<InfoAchievement>       maAchievements;
        private List<InfoAdjacencyBonus>    maAdjacencyBonuses;
        private List<InfoArtPack>           maArtPacks;
        private List<InfoAsset>             maAssets;
        private List<InfoAudio>             maAudios;
        private List<InfoBlackMarket>       maBlackMarkets;
        private List<InfoBlackMarketClass>  maBlackMarketClasses;
        private List<InfoBond>              maBonds;
        private List<InfoBuilding>          maBuildings;
        private List<InfoBuildingClass>     maBuildingClasses;
        private List<InfoCampaignMode>      maCampaignModes;
        private List<InfoCharacter>         maCharacters;
        private List<InfoColor>             maColors;
        private List<InfoColony>            maColonies;
        private List<InfoColonyBonus>       maColonyBonuses;
        private List<InfoColonyBonusLevel>  maColonyBonusLevels;
        private List<InfoColonyClass>       maColonyClasses;
        private List<InfoCondition>         maConditions;
        private List<InfoDirection>         maDirections;
        private List<InfoEspionage>         maEspionages;
        private List<InfoEventAudio>        maEventAudios;
        private List<InfoEventGame>         maEventGames;
        private List<InfoEventLevel>        maEventLevels;
        private List<InfoEventState>        maEventStates;
        private List<InfoEventTurn>         maEventTurns;
        private List<InfoEventTurnOption>   maEventTurnOptions;
        private List<InfoExecutive>         maExecutives;
        private List<InfoGameOption>        maGameOptions;
        private List<InfoGamePhase>         maGamePhases;
        private List<InfoGameSetup>         maGameSetups;
        private List<InfoGameSpeed>         maGameSpeeds;
        private List<InfoGender>            maGenders;
        private List<InfoGlobalsInt>        maGlobalsInts;
        private List<InfoGlobalsFloat>      maGlobalsFloats;
        private List<InfoGlobalsString>     maGlobalsStrings;
        private List<InfoGlobalsType>       maGlobalsTypes;
        private List<InfoHandicap>          maHandicaps;
        private List<InfoHeight>            maHeights;
        private List<InfoHQ>                maHQs;
        private List<InfoHQLevel>           maHQLevels;
        private List<InfoIce>               maIces;
        private List<InfoKeyBinding>        maKeyBindings;
        private List<InfoKeyBindingClass>   maKeyBindingClasses;
        private List<InfoLanguage>          maLanguages;
        private List<InfoLatitude>          maLatitudes;
        private List<InfoLevel>             maLevels;
        private List<InfoLightingEnvironment> maLightingEnvironments;
        private List<InfoLocation>          maLocations;
        private List<InfoMapName>           maMapNames;
        private List<InfoMapSize>           maMapSizes;
        private List<InfoMarkup>            maMarkups;
        private List<InfoModule>            maModules;
        private List<InfoOrder>             maOrders;
        private List<InfoOrdinal>           maOrdinals;
        private List<InfoPatent>            maPatents;
        private List<InfoPerk>              maPerks;
        private List<InfoPersonality>       maPersonalities;
        private List<InfoPlayerColor>       maPlayerColors;
        private List<InfoPlayerOption>      maPlayerOptions;
        private List<InfoResource>          maResources;
        private List<InfoResourceColor>     maResourceColors;
        private List<InfoResourceLevel>     maResourceLevels;
        private List<InfoResourceMinimum>   maResourceMinimums;
        private List<InfoResourcePresence>  maResourcePresences;
        private List<InfoRulesSet>          maRulesSets;
        private List<InfoSabotage>          maSabotages;
        private List<InfoScenario>          maScenarios;
        private List<InfoScenarioDifficulty> maScenarioDifficulties;
        private List<InfoScenarioClass>     maScenarioClasses;
        private List<InfoSevenSols>         maSevenSols;
        private List<InfoSpriteGroup>       maSpriteGroups;
        private List<InfoStory>             maStories;
        private List<InfoTechnology>        maTechnologies;
        private List<InfoTechnologyLevel>   maTechnologyLevels;
        private List<InfoTerrain>           maTerrains;
        private List<InfoTerrainClass>      maTerrainClasses;
        private List<InfoTerrainMaterial>   maTerrainMaterials;
        private List<InfoText>              maTexts;
        private List<InfoUnit>              maUnits;
        private List<InfoWind>              maWinds;
        private List<InfoWorldAudio>        maWorldAudios;
        private InfoGlobals                 mGlobals;

        public InfoGlobals Globals { get { return mGlobals; } }
        public static int cTYPE_NONE = -1; //generic NONE
        public static int cTYPE_CUSTOM = -2; //generic CUSTOM, mostly for custom UI data
        public static int cTYPE_USER = -3; //generic USER, mostly for user created data

        public Infos(IInfosXMLLoader xmlLoader)
        {
            using (new UnityProfileScope("Infos::Infos"))
            {
                Assert.IsNotNull(xmlLoader);
                mXMLLoader = xmlLoader;
                mGlobals = new InfoGlobals();
                maListeners = new List<IInfosListener>();
                BuildListOfInfoFiles();
                CopyDefaultXML();
                LoadInfo(true);
            }
        }
        
        public void AddListener(IInfosListener listener)
        {
            maListeners.Add(listener);
        }
        
        public void LoadInfo(bool resetDefaultXMLCache)
        {
            using (new UnityProfileScope("Infos::LoadInfo"))
            {
                try
                {
                    init(resetDefaultXMLCache);
                }
                catch (Exception ex)
                {
                    if (mXMLLoader.IsModSet)
                    {
                        Debug.LogError("Mod failed, disabling mod");
                        Debug.LogException(ex);

                        //re-initialize without the mod
                        mXMLLoader.ClearMod();
                        init(resetDefaultXMLCache);
                    }
                    else
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }
        
        private void BuildListOfInfoFiles()
        {
            mInfoList = new List<XmlDataListItemBase>();
            mInfoList.Add(new XmlDataListItem<InfoAchievement>(     "Data/achievement",         readTypes, ref maAchievements));
            mInfoList.Add(new XmlDataListItem<InfoAdjacencyBonus>(  "Data/adjacency-bonus",     readTypes, ref maAdjacencyBonuses));
            mInfoList.Add(new XmlDataListItem<InfoArtPack>(         "Data/art-pack",            readTypes, ref maArtPacks));
            mInfoList.Add(new XmlDataListItem<InfoAsset>(           "Data/assets",              readTypes, ref maAssets));
            mInfoList.Add(new XmlDataListItem<InfoAudio>(           "Data/audio",               readTypes, ref maAudios));
            mInfoList.Add(new XmlDataListItem<InfoBlackMarket>(     "Data/black-market",        readTypes, ref maBlackMarkets));
            mInfoList.Add(new XmlDataListItem<InfoBlackMarketClass>("Data/black-market-class",  readTypes, ref maBlackMarketClasses));
            mInfoList.Add(new XmlDataListItem<InfoBond>(            "Data/bond",                readTypes, ref maBonds));
            mInfoList.Add(new XmlDataListItem<InfoBuilding>(        "Data/building",            readTypes, ref maBuildings));
            mInfoList.Add(new XmlDataListItem<InfoBuildingClass>(   "Data/building-class",      readTypes, ref maBuildingClasses));
            mInfoList.Add(new XmlDataListItem<InfoCampaignMode>(    "Data/campaign-mode",       readTypes, ref maCampaignModes));
            mInfoList.Add(new XmlDataListItem<InfoCharacter>(       "Data/character",           readTypes, ref maCharacters));
            mInfoList.Add(new XmlDataListItem<InfoColor>(           "Data/color",               readTypes, ref maColors));
            mInfoList.Add(new XmlDataListItem<InfoColony>(          "Data/colony",              readTypes, ref maColonies));
            mInfoList.Add(new XmlDataListItem<InfoColonyBonus>(     "Data/colony-bonus",        readTypes, ref maColonyBonuses));
            mInfoList.Add(new XmlDataListItem<InfoColonyBonusLevel>("Data/colony-bonus-level",  readTypes, ref maColonyBonusLevels));
            mInfoList.Add(new XmlDataListItem<InfoColonyClass>(     "Data/colony-class",        readTypes, ref maColonyClasses));
            mInfoList.Add(new XmlDataListItem<InfoCondition>(       "Data/condition",           readTypes, ref maConditions));
            mInfoList.Add(new XmlDataListItem<InfoDirection>(       "Data/direction",           readTypes, ref maDirections));
            mInfoList.Add(new XmlDataListItem<InfoEspionage>(       "Data/espionage",           readTypes, ref maEspionages));
            mInfoList.Add(new XmlDataListItem<InfoEventAudio>(      "Data/event-audio",         readTypes, ref maEventAudios));
            mInfoList.Add(new XmlDataListItem<InfoEventGame>(       "Data/event-game",          readTypes, ref maEventGames));
            mInfoList.Add(new XmlDataListItem<InfoEventLevel>(      "Data/event-level",         readTypes, ref maEventLevels));
            mInfoList.Add(new XmlDataListItem<InfoEventState>(      "Data/event-state",         readTypes, ref maEventStates));
            mInfoList.Add(new XmlDataListItem<InfoEventTurn>(       "Data/event-turn",          readTypes, ref maEventTurns));
            mInfoList.Add(new XmlDataListItem<InfoEventTurnOption>( "Data/event-turn-option",   readTypes, ref maEventTurnOptions));
            mInfoList.Add(new XmlDataListItem<InfoExecutive>(       "Data/executive",           readTypes, ref maExecutives));
            mInfoList.Add(new XmlDataListItem<InfoGameOption>(      "Data/game-option",         readTypes, ref maGameOptions));
            mInfoList.Add(new XmlDataListItem<InfoGamePhase>(       "Data/game-phase",          readTypes, ref maGamePhases));
            mInfoList.Add(new XmlDataListItem<InfoGameSetup>(       "Data/game-setup",          readTypes, ref maGameSetups));
            mInfoList.Add(new XmlDataListItem<InfoGameSpeed>(       "Data/game-speed",          readTypes, ref maGameSpeeds));
            mInfoList.Add(new XmlDataListItem<InfoGender>(          "Data/gender",              readTypes, ref maGenders));
            mInfoList.Add(new XmlDataListItem<InfoGlobalsInt>(      "Data/globals-int",         readTypes, ref maGlobalsInts));
            mInfoList.Add(new XmlDataListItem<InfoGlobalsFloat>(    "Data/globals-float",       readTypes, ref maGlobalsFloats));
            mInfoList.Add(new XmlDataListItem<InfoGlobalsString>(   "Data/globals-string",      readTypes, ref maGlobalsStrings));
            mInfoList.Add(new XmlDataListItem<InfoGlobalsType>(     "Data/globals-type",        readTypes, ref maGlobalsTypes));
            mInfoList.Add(new XmlDataListItem<InfoHandicap>(        "Data/handicap",            readTypes, ref maHandicaps));
            mInfoList.Add(new XmlDataListItem<InfoHeight>(          "Data/height",              readTypes, ref maHeights));
            mInfoList.Add(new XmlDataListItem<InfoHQ>(              "Data/hq",                  readTypes, ref maHQs));
            mInfoList.Add(new XmlDataListItem<InfoHQLevel>(         "Data/hq-level",            readTypes, ref maHQLevels));
            mInfoList.Add(new XmlDataListItem<InfoIce>(             "Data/ice",                 readTypes, ref maIces));
            mInfoList.Add(new XmlDataListItem<InfoKeyBinding>(      "Data/key-binding",         readTypes, ref maKeyBindings));
            mInfoList.Add(new XmlDataListItem<InfoKeyBindingClass>( "Data/key-binding-class",   readTypes, ref maKeyBindingClasses));
            mInfoList.Add(new XmlDataListItem<InfoLanguage>(        "Data/language",            readTypes, ref maLanguages));
            mInfoList.Add(new XmlDataListItem<InfoLatitude>(        "Data/latitude",            readTypes, ref maLatitudes));
            mInfoList.Add(new XmlDataListItem<InfoLevel>(           "Data/level",               readTypes, ref maLevels));
            mInfoList.Add(new XmlDataListItem<InfoLightingEnvironment>("Data/lighting-environment", readTypes, ref maLightingEnvironments));
            mInfoList.Add(new XmlDataListItem<InfoLocation> (       "Data/location",            readTypes, ref maLocations));
            mInfoList.Add(new XmlDataListItem<InfoMapName>(         "Data/map-name",            readTypes, ref maMapNames));
            mInfoList.Add(new XmlDataListItem<InfoMapSize>(         "Data/map-size",            readTypes, ref maMapSizes));
            mInfoList.Add(new XmlDataListItem<InfoMarkup>(          "Data/markup",              readTypes, ref maMarkups));
            mInfoList.Add(new XmlDataListItem<InfoModule>(          "Data/module",              readTypes, ref maModules));
            mInfoList.Add(new XmlDataListItem<InfoOrder>(           "Data/order",               readTypes, ref maOrders));
            mInfoList.Add(new XmlDataListItem<InfoOrdinal>(         "Data/ordinal",             readTypes, ref maOrdinals));
            mInfoList.Add(new XmlDataListItem<InfoPatent>(          "Data/patent",              readTypes, ref maPatents));
            mInfoList.Add(new XmlDataListItem<InfoPerk>(            "Data/perk",                readTypes, ref maPerks));
            mInfoList.Add(new XmlDataListItem<InfoPersonality>(     "Data/personality",         readTypes, ref maPersonalities));
            mInfoList.Add(new XmlDataListItem<InfoPlayerColor>(     "Data/player-color",        readTypes, ref maPlayerColors));
            mInfoList.Add(new XmlDataListItem<InfoPlayerOption>(    "Data/player-option",       readTypes, ref maPlayerOptions));
            mInfoList.Add(new XmlDataListItem<InfoResource>(        "Data/resource",            readTypes, ref maResources));
            mInfoList.Add(new XmlDataListItem<InfoResourceColor>(   "Data/resource-color",      readTypes, ref maResourceColors));
            mInfoList.Add(new XmlDataListItem<InfoResourceLevel>(   "Data/resource-level",      readTypes, ref maResourceLevels));
            mInfoList.Add(new XmlDataListItem<InfoResourceMinimum>( "Data/resource-minimum",    readTypes, ref maResourceMinimums));
            mInfoList.Add(new XmlDataListItem<InfoResourcePresence>("Data/resource-presence",   readTypes, ref maResourcePresences));
            mInfoList.Add(new XmlDataListItem<InfoRulesSet>(        "Data/rules-set",           readTypes, ref maRulesSets));
            mInfoList.Add(new XmlDataListItem<InfoSabotage>(        "Data/sabotage",            readTypes, ref maSabotages));
            mInfoList.Add(new XmlDataListItem<InfoScenario>(        "Data/scenario",            readTypes, ref maScenarios));
            mInfoList.Add(new XmlDataListItem<InfoScenarioDifficulty>("Data/scenario-difficulty", readTypes, ref maScenarioDifficulties));
            mInfoList.Add(new XmlDataListItem<InfoScenarioClass>(   "Data/scenario-class",      readTypes, ref maScenarioClasses));
            mInfoList.Add(new XmlDataListItem<InfoSevenSols>(       "Data/seven-sols",          readTypes, ref maSevenSols));
            mInfoList.Add(new XmlDataListItem<InfoSpriteGroup>(     "Data/sprite-group",        readTypes, ref maSpriteGroups));
            mInfoList.Add(new XmlDataListItem<InfoStory>(           "Data/story",               readTypes, ref maStories));
            mInfoList.Add(new XmlDataListItem<InfoTechnology>(      "Data/technology",          readTypes, ref maTechnologies));
            mInfoList.Add(new XmlDataListItem<InfoTechnologyLevel>( "Data/technology-level",    readTypes, ref maTechnologyLevels));
            mInfoList.Add(new XmlDataListItem<InfoTerrain>(         "Data/terrain",             readTypes, ref maTerrains));
            mInfoList.Add(new XmlDataListItem<InfoTerrainClass>(    "Data/terrain-class",       readTypes, ref maTerrainClasses));
            mInfoList.Add(new XmlDataListItem<InfoTerrainMaterial>( "Data/terrain-material",    readTypes, ref maTerrainMaterials));
            mInfoList.Add(new XmlDataListItem<InfoUnit>(            "Data/unit",                readTypes, ref maUnits));
            mInfoList.Add(new XmlDataListItem<InfoWind>(            "Data/wind",                readTypes, ref maWinds));
            mInfoList.Add(new XmlDataListItem<InfoWorldAudio>(      "Data/world-audio",         readTypes, ref maWorldAudios));

            //find all Data/text-*.xml files
            List<string> textFiles = GetTextFiles();
            foreach(string filename in textFiles)
            {
                mInfoList.Add(new XmlDataListItem<InfoText>(        filename,                   readTypes, ref maTexts)); //maTexts split across multiple xml's
            }
        }

        public List<string> GetTextFiles()
        {
            using (new UnityProfileScope("Infos::GetTextFiles"))
            {
                string folder = "Data/";

                List<string> textFiles = Resources
                        .LoadAll<TextAsset>(folder)
                        .Where(x => x.name.StartsWith("text-"))
                        .Select(x => folder + x.name)
                        .ToList();

                return textFiles;
            }
        }

        private void CopyDefaultXML()
        {
            List<string> filenames = mInfoList.Select(x => x.GetFileName()).ToList();
            mXMLLoader.CopyDefaultXML(filenames);
        }
        
        private void ClearInfoListTypes()
        {
            using (new UnityProfileScope("Infos::ClearInfoListTypes"))
            {
                mTypeDictionary.Clear();
                mTypeDictionary.Add("NONE", cTYPE_NONE);
                mTypeDictionary.Add("CUSTOM", cTYPE_CUSTOM);

                mRemovedXmlFields.Clear();
                mReadXmlFields.Clear();

                //clear types
                mInfoList.ForEach(i => i.ClearTypes());
            }
        }
            
        private void ReadInfoListTypes()
        {
            using (new UnityProfileScope("Infos::ReadInfoListTypes"))
            {
                //read types
                foreach (XmlDataListItemBase item in mInfoList)
                {
                    XmlDocument xmlDoc = getModdableXML(item.GetFileName(), ".xml");
                    XmlNodeList nodes = xmlDoc.SelectNodes("Root/Entry");
                    item.ReadTypes(nodes, this, false, mRemovedXmlFields);
                }
            }
        }
        
        private void ReadInfoListData()
        {
            using (new UnityProfileScope("Infos::ReadInfoListData"))
            {
                foreach (XmlDataListItemBase item in mInfoList)
                {
                    XmlDocument xmlDoc = getModdableXML(item.GetFileName(), ".xml");
                    XmlNodeList nodes = xmlDoc.SelectNodes("Root/Entry");
                    BeginReadValidation();
                    item.ReadData(nodes, this, false, mRemovedXmlFields);
                    EndReadValidation(item.GetFileName());
                }
            }
        }

    	private void ReadAddedInfoListTypes()
    	{
            using (new UnityProfileScope("Infos::ReadAddedInfoListTypes"))
            {
                foreach (XmlDataListItemBase item in mInfoList)
                {
                    List<XmlDocument> xmlDocs = mXMLLoader.GetAllModdedXML(item.GetFileName() + "-add");

                    if (xmlDocs == null || xmlDocs.Count == 0)
                        continue;

                    foreach (XmlDocument xmlDoc in xmlDocs)
                    {
                        XmlNodeList nodes = xmlDoc.SelectNodes("Root/Entry");
                        item.ReadTypes(nodes, this, true, mRemovedXmlFields);
                    }
                }
            }
    	}

    	private void ReadAddedInfoListData()
    	{
            using (new UnityProfileScope("Infos::ReadAddedInfoListData"))
            {
                mbSuppressError = true;
                foreach (XmlDataListItemBase item in mInfoList)
                {
                    List<XmlDocument> xmlDocs = mXMLLoader.GetAllModdedXML(item.GetFileName() + "-add");

                    if (xmlDocs == null || xmlDocs.Count == 0)
                        continue;

                    foreach (XmlDocument xmlDoc in xmlDocs)
                    {
                        XmlNodeList nodes = xmlDoc.SelectNodes("Root/Entry");
                        item.ReadData(nodes, this, true, mRemovedXmlFields);
                    }
                }
            }
        }

    	private void ReadChangeInfoListData()
    	{
            using (new UnityProfileScope("Infos::ReadChangeInfoListData"))
            {
                mbSuppressError = true;
    		    foreach (XmlDataListItemBase item in mInfoList)
    		    {
                    List<XmlDocument> xmlDocs = mXMLLoader.GetAllModdedXML(item.GetFileName() + "-change");

                    if (xmlDocs == null || xmlDocs.Count == 0)
                        continue;

                    foreach (XmlDocument xmlDoc in xmlDocs)
                    {
                        XmlNodeList nodes = xmlDoc.SelectNodes("Root/Entry");
                        item.ReadData(nodes, this, true, mRemovedXmlFields);
                    }
    		    }
            }
    	}

        private void ReadRemovedInfoListTypes()
        {
            using (new UnityProfileScope("Infos::ReadRemovedInfoListTypes"))
            {
                List<XmlDocument> xmlDocs = mXMLLoader.GetAllModdedXML("Data/remove");

                if(xmlDocs == null || xmlDocs.Count == 0)
                    return;

                foreach(XmlDocument xmlDoc in xmlDocs)
                {
                    XmlNodeList nodes = xmlDoc.SelectNodes("Root/Entry");
                    nodes.ForEach<XmlNode>(node => mRemovedXmlFields.Add(node.InnerText));
                }
            }
        }

        private void BeginReadValidation()
        {
            using (new UnityProfileScope("Infos::BeginReadValidation"))
            {
                mReadXmlFields.Clear();
                mReadXmlFields.Add("zType");
            }
        }

        private void EndReadValidation(string filename)
        {
            using (new UnityProfileScope("Infos::EndReadValidation"))
            {
                List<XmlDocument> xmlDocs = new List<XmlDocument>();
                xmlDocs.Add(getModdableXML(filename, ".xml"));
                xmlDocs.AddRange(mXMLLoader.GetAllModdedXML(filename + "-add"));
                xmlDocs.AddRange(mXMLLoader.GetAllModdedXML(filename + "-change"));
                
                //XmlDocument xmlDoc = mXMLLoader.GetDefaultXML(filename);
                foreach(XmlDocument xmlDoc in xmlDocs)
                {
                    XmlNode root = FindChild(xmlDoc, "Root");
                    for(XmlNode entry = root.FirstChild; entry != null; entry = entry.NextSibling)
                    {
                        if(entry.Name == "Entry")
                        {
                            for(XmlNode child = entry.FirstChild; child != null; child = child.NextSibling)
                            {
                                if(child.NodeType == XmlNodeType.Text || child.NodeType == XmlNodeType.CDATA || child.NodeType == XmlNodeType.Comment)
                                    continue;
                    
                                if(!mReadXmlFields.Contains(child.Name))
                                    Debug.LogError("Extra field: " + child.Name + " in " + filename);
                            }
                        }
                    }
                }
            }
        }

        private void init(bool resetDefaultXMLCache)
        {
            mXMLLoader.ResetCache(resetDefaultXMLCache);

            ClearInfoListTypes();

            ReadRemovedInfoListTypes();
            ReadInfoListTypes();
            ReadAddedInfoListTypes();

            ReadInfoListData ();
            ReadAddedInfoListData();
            ReadChangeInfoListData ();

            //validate sizes
            Assert.IsTrue(maWinds.Count < 128); // Make sure it fits in a byte for serialization
            Assert.IsTrue(maHeights.Count < 128); // Make sure it fits in a byte for serialization
            Assert.IsTrue(maTerrains.Count < 128); // Make sure it fits in a byte for serialization
            Assert.AreEqual(maDirections.Count, (int)DirectionType.NUM_TYPES);
            Assert.AreEqual(maLanguages.Count, (int) LanguageType.NUM_TYPES);
            Assert.AreEqual(maPlayerOptions.Count, (int) PlayerOptionType.NUM_TYPES);
            Assert.AreEqual(maScenarioClasses.Count, (int) ScenarioClassType.NUM_TYPES);

            mGlobals.ReadData(this);
            
            //notify listeners
            maListeners.ForEach(l => l.OnInfosLoaded());
        }

        public static XmlNode FindChild(XmlNode node, string zName)
        {
            if(node == null)
                return null;

            for(XmlNode child = node.FirstChild; child != null; child = child.NextSibling)
            {
                if(child.Name == zName)
                    return child;
            }

            return null;
        }

        public void addReadXmlField(string zName)
        {
            mReadXmlFields.Add(zName);
        }
        
        public void readString(XmlNode node, string zName, ref string zValue)
        {
            //using (new UnityProfileScope("readString"))
            {
                mReadXmlFields.Add(zName);
                XmlNode child = FindChild(node, zName);
                if(child != null)
                {
                    zValue = child.InnerText;
                }
                else
                {
                    if(!mbSuppressError) 
                        Debug.LogError("Failed Infos.readString() reading " + zName);
                }
            }
        }
        
        public string readString(XmlNode node, string zName)
        {
            //using (new UnityProfileScope("readString2"))
            {
                string zResult = "";
                readString(node, zName, ref zResult);
                return zResult;
            }
        }
        
        public void readInt(XmlNode node, string zName, ref int iValue)
        {
            //using (new UnityProfileScope("readInt"))
            {
                string zData = readString(node, zName);
                if (!zData.Equals("")) iValue = Convert.ToInt32(zData);
            }
        }

        public void readFloat(XmlNode node, string zName, ref float fValue)
        {
            //using (new UnityProfileScope("readFloat"))
            {
                string zData = readString(node, zName);
                if (!zData.Equals("")) fValue = Convert.ToSingle(zData);
            }
        }

        public void readBool(XmlNode node, string zName, ref bool bValue)
        {
            //using (new UnityProfileScope("readBool"))
            {
                string zData = readString(node, zName);
                if (!zData.Equals("")) bValue = zData != "0";
            }
        }

        public void readType<T>(XmlNode node, string zName, ref T value) where T : struct
        {
            //using (new UnityProfileScope("readType"))
            {
                string zData = readString(node, zName);
                if (!zData.Equals("")) value = getType<T>(zData);
            }
        }
        
        public T readType<T>(XmlNode node, string zName) where T : struct
        {
            //using (new UnityProfileScope("readType2"))
            {
                return getType<T>(readString(node, zName));
            }
        }
        
        public char readChar(XmlNode node, string zName)
        {
            //using (new UnityProfileScope("readChar"))
            {
                string text = readString(node, zName);
                return (text.Length == 1) ? text[0] : '\0';
            }
        }
        
        public void readIntsByType(List<int> maiValues, int iCount, XmlNode node, string zText)
        {
            //using (new UnityProfileScope("readIntsByType"))
            {
                mReadXmlFields.Add(zText);

                XmlNode entryNode = FindChild(node, zText);
                if (entryNode != null)
                {
                    maiValues.Clear();
                    maiValues.Resize(iCount, 0);
                    for(XmlNode child = entryNode.FirstChild; child != null; child = child.NextSibling)
                    {
                        if(child.Name == "Pair")
                        {
                            int value = 0;
                            readInt(child, "iValue", ref value);
                            int index = getType<int>(readString(child, "zIndex"));
                            if(index >=0)
                            {
                                maiValues[index] = value;
                            }
                        }
                    }
                }
                else
                {
                    if(!mbSuppressError)
                        Debug.LogError("Failed Infos.readIntsByType() reading " + zText);
                    maiValues.Resize(iCount, 0);
                }
            }
        }

        public void readBoolsByType(List<bool> mabValues, int iCount, XmlNode node, string zText)
        {
            //using (new UnityProfileScope("readBoolsByType"))
            {
                mReadXmlFields.Add(zText);

                XmlNode entryNode = FindChild(node, zText);
                if (entryNode != null)
                {
                    mabValues.Clear();
                    mabValues.Resize(iCount, false);

                    for(XmlNode child = entryNode.FirstChild; child != null; child = child.NextSibling)
                    {
                        if(child.Name == "Pair")
                        {
                            bool value = false;
                            readBool(child, "bValue", ref value);
                            int index = getType<int>(readString(child, "zIndex"));
                            if (index >= 0)
                            {
                                mabValues[index] = value;
                            }
                        }
                    }
                }
                else
                {
                    if (!mbSuppressError)
                        Debug.LogError("Failed Infos.readBoolsByType() reading " + zText);
                    mabValues.Resize(iCount, false);
                }
            }
        }

        public void readStringsByType(List<string> mazValues, int iCount, XmlNode node, string zText)
        {
            //using (new UnityProfileScope("readStringsByType"))
            {
                mReadXmlFields.Add(zText);

                XmlNode entryNode = FindChild(node, zText);
                if (entryNode != null)
                {
                    mazValues.Clear();
                    mazValues.Resize(iCount, "");

                    for(XmlNode child = entryNode.FirstChild; child != null; child = child.NextSibling)
                    {
                        if(child.Name == "Pair")
                        {
                            int index = getType<int>(readString(child, "zIndex"));
                            if (index >= 0)
                            {
                                mazValues[index] = readString(child, "zValue");
                            }
                        }
                    }
                }
                else
                {
                    if (!mbSuppressError)
                        Debug.LogError("Failed Infos.readStringsByType() reading " + zText);
                    mazValues.Resize(iCount, "");
                }
            }
        }

        public void readStringsByType<T>(Dictionary<T, string> mazValues, XmlNode node, string zText) where T : struct
        {
            //using (new UnityProfileScope("readStringsByType2"))
            {
                mReadXmlFields.Add(zText);

                XmlNode entryNode = FindChild(node, zText);
                if (entryNode != null)
                {
                    mazValues.Clear();

                    for(XmlNode child = entryNode.FirstChild; child != null; child = child.NextSibling)
                    {
                        if(child.Name == "Pair")
                        {
                            T index = getType<T>(readString(child, "zIndex"));
                            mazValues[index] = readString(child, "zValue");
                        }
                    }
                }
                else
                {
                    if (!mbSuppressError)
                        Debug.LogError("Failed Infos.readStringsByType() reading " + zText);
                    mazValues.Clear();
                }
            }
        }

        public void readTypesByType<T>(List<T> maeValues, int iCount, XmlNode node, string zText) where T : struct
        {
            //using (new UnityProfileScope("readTypesByType"))
            {
                mReadXmlFields.Add(zText);

                XmlNode entryNode = FindChild(node, zText);
                if (entryNode != null)
                {
                    maeValues.Clear();
                    maeValues.Resize(iCount, CastTo<T>.From(-1));

                    for(XmlNode child = entryNode.FirstChild; child != null; child = child.NextSibling)
                    {
                        if(child.Name == "Pair")
                        {
                            int index = getType<int>(readString(child, "zIndex"));
                            if(index >= 0)
                            {
                                maeValues[index] = getType<T>(readString(child, "zValue"));
                            }
                        }
                    }
                }
                else
                {
                    if(!mbSuppressError)
                        Debug.LogError("Failed Infos.readTypesByType() reading " + zText);
                    maeValues.Resize(iCount, CastTo<T>.From(-1));
                }
            }
        }

        public void readBoolListByType(List<List<bool>> maabValues, int iCount, int iSubCount, XmlNode node, string zText)
        {
            //using (new UnityProfileScope("readBoolListByType"))
            {
                maabValues.Clear();
                while (maabValues.Count < iCount)
                {
                    maabValues.Add(new List<bool>());
                    maabValues.Last().Resize(iSubCount, false);
                }

                mReadXmlFields.Add(zText);
                XmlNode entryNode = FindChild(node, zText);
                if (entryNode != null)
                {
                    for (XmlNode child = entryNode.FirstChild; child != null; child = child.NextSibling)
                    {
                        if (child.Name == "Pair")
                        {
                            int index = getType<int>(readString(child, "zIndex"));
                            if (index >= 0)
                            {
                                for (XmlNode subchild = child.FirstChild; subchild != null; subchild = subchild.NextSibling)
                                {
                                    if (subchild.Name == "SubPair")
                                    {
                                        bool value = false;
                                        readBool(subchild, "bValue", ref value);
                                        int subIndex = getType<int>(readString(subchild, "zSubIndex"));
                                        if (subIndex >= 0)
                                        {
                                            maabValues[index][subIndex] = value;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (!mbSuppressError)
                        Debug.LogError("Failed Infos.readBoolListByType() reading " + zText);
                }
            }
        }

        public void readIntListByType(List<List<int>> maabValues, int iCount, int iSubCount, XmlNode node, string zText)
        {
            //using (new UnityProfileScope("readIntListByType"))
            {
                maabValues.Clear();
                while (maabValues.Count < iCount)
                {
                    maabValues.Add(new List<int>());
                    maabValues.Last().Resize(iSubCount, 0);
                }

                mReadXmlFields.Add(zText);
                XmlNode entryNode = FindChild(node, zText);
                if (entryNode != null)
                {
                    for (XmlNode child = entryNode.FirstChild; child != null; child = child.NextSibling)
                    {
                        if (child.Name == "Pair")
                        {
                            int index = getType<int>(readString(child, "zIndex"));
                            if (index >= 0)
                            {
                                for (XmlNode subchild = child.FirstChild; subchild != null; subchild = subchild.NextSibling)
                                {
                                    if (subchild.Name == "SubPair")
                                    {
                                        int value = 0;
                                        readInt(subchild, "iValue", ref value);
                                        int subIndex = getType<int>(readString(subchild, "zSubIndex"));
                                        if (subIndex >= 0)
                                        {
                                            maabValues[index][subIndex] = value;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (!mbSuppressError)
                        Debug.LogError("Failed Infos.readIntListByType() reading " + zText);
                }
            }
        }

        public void readStrings(out List<string> aOutput, XmlNode node, string zText)
        {
            //using (new UnityProfileScope("readStrings"))
            {
                mReadXmlFields.Add(zText);
                aOutput = new List<string>();

                XmlNode entryNode = FindChild(node, zText);
                if (entryNode != null)
                {
                    for(XmlNode child = entryNode.FirstChild; child != null; child = child.NextSibling)
                    {
                        if(child.Name == "zValue")
                            aOutput.Add(child.InnerText);
                    }
                }
                else
                {
                    if (!mbSuppressError)
                        Debug.LogError("Failed Infos.readStrings() reading " + zText);
                }
            }
        }
        
        public void readTypeStrings<T>(out List<T> aOutput, XmlNode node, string zText) where T : struct
        {
            //using (new UnityProfileScope("readTypeStrings"))
            {
                mReadXmlFields.Add(zText);
                aOutput = new List<T>();

                XmlNode entryNode = FindChild(node, zText);
                if (entryNode != null)
                {
                    for(XmlNode child = entryNode.FirstChild; child != null; child = child.NextSibling)
                    {
                        if(child.Name == "zValue")
                            aOutput.Add(getType<T>(child.InnerText));
                    }
                }
                else
                {
                    if (!mbSuppressError)
                        Debug.LogError("Failed Infos.readTypeStrings() reading " + zText);
                }
            }
        }
        
        public void readHotkeys(KeyBinding mHotkeys, string zHotkeyString)
        {
            using (new UnityProfileScope("readHotkeys"))
            {
                Func<string, KeyCombo> keyCodeParser = input =>
                {
                    KeyCombo combo = new KeyCombo();
                    combo.AddRange(
                        input.Split('+')
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .Select(s => KeycodeDictionary.GetKeycode(s.ToLowerInvariant()))
                        );
                    return combo;
                };
            
                mHotkeys.Clear();
                mHotkeys.AddRange(zHotkeyString
                                  .Split(',')
                                  .Select(s => s.Trim())
                                  .Where(s => !string.IsNullOrEmpty(s))
                                  .Select(keyCodeParser));
            }
        }

        public int getGlobalInt(string zType)
        {
            GlobalsIntType eType = getType<GlobalsIntType>(zType);
            return globalsInt(eType).miValue;
        }

        public float getGlobalFloat(string zType)
        {
            GlobalsFloatType eType = getType<GlobalsFloatType>(zType);
            return globalsFloat(eType).mfValue;
        }

        public string getGlobalString(string zType)
        {
            GlobalsStringType eType = getType<GlobalsStringType>(zType);
            return globalsString(eType).mzValue;
        }

        public T getGlobalType<T>(string zType)
        {
            GlobalsTypeType eType = getType<GlobalsTypeType>(zType);
            return CastTo<T>.From(globalsType(eType).meValue);
        }
        
        public T getType<T>(string zType) where T : struct
        {
            return getType<T>(zType, true);
        }

        public T getType<T>(string zType, bool showError) where T : struct
        {
            if((zType == null) || (zType.Length == 0) || mRemovedXmlFields.Contains(zType))
            {
                return CastTo<T>.From(-1);
            }
            else if(mTypeDictionary.ContainsKey(zType))
            {
                return CastTo<T>.From(mTypeDictionary[zType]);
            }
            else
            {
                if(showError)
                    Debug.LogError("Unable to find type: " + zType);
                return CastTo<T>.From(-1);
            }
        }
        
        void readTypes<T>(XmlNodeList nodes, List<T> infos, HashSet<string> removedTypes, bool isAddedType) where T : InfoBase, new()
        {
            foreach (XmlNode node in nodes)
            {
                string zType = "";
                readString(node, "zType", ref zType);

                if(removedTypes.Contains(zType) || (isAddedType && mTypeDictionary.ContainsKey(zType)))
                    continue;

                if(mTypeDictionary.ContainsKey(zType))
                    Debug.LogError("Duplicate typeName: " + zType);

                T info = new T();
                info.mzType = zType;

                int index = infos.Count;

                if (isAddedType && (FindChild(node, "iNewIndex") != null))
                {
                    int newIndex = -1;
                    readInt(node, "iNewIndex", ref newIndex);
                    if (newIndex != -1)
                    {
                        index = newIndex;
                    }
                }

                info.miType = index;
                mTypeDictionary[info.mzType] = index;

                if (index != infos.Count)
                {
                    infos.Insert(index, info);

                    for (int i = (index + 1); i < infos.Count; i++)
                    {
                        infos[i].miType = i;
                        mTypeDictionary[infos[i].mzType] = i;
                    }
                }
                else
                {
                    infos.Add(info);
                }
            }
        }

        public void readHexColor(XmlNode node, string zName, ref Color cValue)
        {
            string zData = readString(node, zName);
            if (!zData.Equals(""))
            {
                if (zData.Length == 6 || zData.Length == 8)
                    cValue = HTMLColorConverter.HexToRGBA(zData);
                else //error
                {
                    if (!mbSuppressError)
                        Debug.LogError("Invalid HexColor: " + zName + "=" + zData);
                    cValue = Color.magenta;
                }
            }
        }

        public XmlDocument getModdableXML(string filename, string extension)
    	{
            using (new UnityProfileScope("Infos::openModdableXML"))
            {
                XmlDocument xmlDoc = mXMLLoader.GetModdedXML(filename, extension);
    		    mbSuppressError = (xmlDoc != null);
    		    if(xmlDoc == null)
                    xmlDoc = mXMLLoader.GetDefaultXML(filename);
                if(xmlDoc == null)
                    xmlDoc = mXMLLoader.GetDLC_XML(filename, extension);

    		    return xmlDoc;
            }
    	}
        
        public List<InfoAchievement> achievements()                         { return maAchievements; }
        public InfoAchievement achievement(AchievementType eIndex)          { return maAchievements.GetOrDefault((int)eIndex); }
        public AchievementType achievementsNum()                            { return (AchievementType)maAchievements.Count; }

        public List<InfoAdjacencyBonus> adjacencyBonuses()                  { return maAdjacencyBonuses; }
        public InfoAdjacencyBonus adjacencyBonus(AdjacencyBonusType eIndex) { return maAdjacencyBonuses.GetOrDefault((int)eIndex); }
        public AdjacencyBonusType adjacencyBonusesNum()                     { return (AdjacencyBonusType)maAdjacencyBonuses.Count; }

        public List<InfoArtPack> artPacks()                                 { return maArtPacks; }
        public InfoArtPack artPack(ArtPackType eIndex)                      { return maArtPacks.GetOrDefault((int)eIndex); }
        public ArtPackType artPacksNum()                                    { return (ArtPackType)maArtPacks.Count; }

        public List<InfoAsset> assets()                                     { return maAssets; }
        public InfoAsset asset(AssetType eIndex)                            { return maAssets.GetOrDefault((int)eIndex); }
        public AssetType assetsNum()                                        { return (AssetType)maAssets.Count; }

        public List<InfoAudio> audios()                                     { return maAudios; }
        public InfoAudio audio(AudioTypeT eIndex)                           { return maAudios.GetOrDefault((int)eIndex); }
        public AudioTypeT audiosNum()                                       { return (AudioTypeT)maAudios.Count; }

        public List<InfoBlackMarket> blackMarkets()                         { return maBlackMarkets; }
        public InfoBlackMarket blackMarket(BlackMarketType eIndex)          { return maBlackMarkets.GetOrDefault((int)eIndex); }
        public BlackMarketType blackMarketsNum()                            { return (BlackMarketType)maBlackMarkets.Count; }

        public List<InfoBlackMarketClass> blackMarketClasses()              { return maBlackMarketClasses; }
        public InfoBlackMarketClass blackMarketClass(BlackMarketClassType eIndex) { return maBlackMarketClasses.GetOrDefault((int)eIndex); }
        public BlackMarketClassType blackMarketClassesNum()                 { return (BlackMarketClassType)maBlackMarketClasses.Count; }

        public List<InfoBond> bonds()                                       { return maBonds; }
        public InfoBond bond(BondType eIndex)                               { return maBonds.GetOrDefault((int)eIndex); }
        public BondType bondsNum()                                          { return (BondType)maBonds.Count; }

        public List<InfoBuilding> buildings()                               { return maBuildings; }
        public InfoBuilding building(BuildingType eIndex)                   { return maBuildings.GetOrDefault((int)eIndex); }
        public BuildingType buildingsNum()                                  { return (BuildingType)maBuildings.Count; }
        
        public List<InfoBuildingClass> buildingClasses()                    { return maBuildingClasses; }
        public InfoBuildingClass buildingClass(BuildingClassType eIndex)    { return maBuildingClasses.GetOrDefault((int)eIndex); }
        public BuildingClassType buildingClassesNum()                       { return (BuildingClassType)maBuildingClasses.Count; }
        
        public List<InfoCharacter> characters()                             { return maCharacters; }
        public InfoCharacter character(CharacterType eIndex)                { return maCharacters.GetOrDefault((int)eIndex); }
        public CharacterType charactersNum()                                { return (CharacterType)maCharacters.Count; }
        
        public List<InfoCampaignMode> campaignModes()                       { return maCampaignModes; }
        public InfoCampaignMode campaignMode(CampaignModeType eIndex)       { return maCampaignModes.GetOrDefault((int)eIndex); }
        public CampaignModeType campaignModeNums()                          { return (CampaignModeType)maCampaignModes.Count; }

        public List<InfoColor> colors()                                     { return maColors; }
        public InfoColor color(ColorType eIndex)                            { return maColors.GetOrDefault((int)eIndex); }
        public ColorType colorsNum()                                        { return (ColorType)maColors.Count; }

        public List<InfoColony> colonies()                                  { return maColonies; }
        public InfoColony colony(ColonyType eIndex)                          { return maColonies.GetOrDefault((int)eIndex); }
        public ColonyType coloniesNum()                                     { return (ColonyType)maColonies.Count; }

        public List<InfoColonyBonus> colonyBonuses()                        { return maColonyBonuses; }
        public InfoColonyBonus colonyBonus(ColonyBonusType eIndex)          { return maColonyBonuses.GetOrDefault((int)eIndex); }
        public ColonyBonusType colonyBonusesNum()                           { return (ColonyBonusType)maColonyBonuses.Count; }

        public List<InfoColonyBonusLevel> colonyBonusLevels()               { return maColonyBonusLevels; }
        public InfoColonyBonusLevel colonyBonusLevel(ColonyBonusLevelType eIndex) { return maColonyBonusLevels.GetOrDefault((int)eIndex); }
        public ColonyBonusLevelType colonyBonusLevelsNum()                  { return (ColonyBonusLevelType)maColonyBonusLevels.Count; }

        public List<InfoColonyClass> colonyClasses()                        { return maColonyClasses; }
        public InfoColonyClass colonyClass(ColonyClassType eIndex)          { return maColonyClasses.GetOrDefault((int)eIndex); }
        public ColonyClassType colonyClassesNum()                           { return (ColonyClassType)maColonyClasses.Count; }

        public List<InfoCondition> conditions()                             { return maConditions; }
        public InfoCondition condition(ConditionType eIndex)                { return maConditions.GetOrDefault((int)eIndex); }
        public ConditionType conditionsNum()                                { return (ConditionType)maConditions.Count; }
        
        public List<InfoDirection> directions()                             { return maDirections; }
        public InfoDirection direction(DirectionType eIndex)                { return maDirections.GetOrDefault((int)eIndex); }
        
        public List<InfoEspionage> espionages()                             { return maEspionages; }
        public InfoEspionage espionage(EspionageType eIndex)                { return maEspionages.GetOrDefault((int)eIndex); }
        public EspionageType espionagesNum()                                { return (EspionageType)maEspionages.Count; }

        public List<InfoEventAudio> eventAudios() { return maEventAudios; }
        public InfoEventAudio eventAudio(EventAudioType eIndex) { return maEventAudios.GetOrDefault((int)eIndex); }
        public EventAudioType eventAudiosNum() { return (EventAudioType)maEventAudios.Count; }

        public List<InfoEventGame> eventGames()                             { return maEventGames; }
        public InfoEventGame eventGame(EventGameType eIndex)                { return maEventGames.GetOrDefault((int)eIndex); }
        public EventGameType eventGamesNum()                                { return (EventGameType)maEventGames.Count; }

        public List<InfoEventLevel> eventLevels()                           { return maEventLevels; }
        public InfoEventLevel eventLevel(EventLevelType eIndex)             { return maEventLevels.GetOrDefault((int)eIndex); }
        public EventLevelType eventLevelsNum()                              { return (EventLevelType)maEventLevels.Count; }

        public List<InfoEventState> eventStates()                           { return maEventStates; }
        public InfoEventState eventState(EventStateType eIndex)             { return maEventStates.GetOrDefault((int)eIndex); }
        public EventStateType eventStatesNum()                              { return (EventStateType)maEventStates.Count; }
        
        public List<InfoEventTurn> eventTurns()                             { return maEventTurns; }
        public InfoEventTurn eventTurn(EventTurnType eIndex)                { return maEventTurns.GetOrDefault((int)eIndex); }
        public EventTurnType eventTurnsNum()                                { return (EventTurnType)maEventTurns.Count; }

        public List<InfoEventTurnOption> eventTurnOptions()                 { return maEventTurnOptions; }
        public InfoEventTurnOption eventTurnOption(EventTurnOptionType eIndex){ return maEventTurnOptions.GetOrDefault((int)eIndex); }
        public EventTurnOptionType eventTurnOptionsNum()                    { return (EventTurnOptionType)maEventTurnOptions.Count; }

        public List<InfoExecutive> executives()                             { return maExecutives; }
        public InfoExecutive executive(ExecutiveType eIndex)                { return maExecutives.GetOrDefault((int)eIndex); }
        public ExecutiveType executivesNum()                                { return (ExecutiveType)maExecutives.Count; }
        
        public List<InfoGameOption> gameOptions()                           { return maGameOptions; }
        public InfoGameOption gameOption(GameOptionType eIndex)             { return maGameOptions.GetOrDefault((int)eIndex); }
        public GameOptionType gameOptionsNum()                              { return (GameOptionType)maGameOptions.Count; }
        
        public List<InfoGamePhase> gamePhases()                             { return maGamePhases; }
        public InfoGamePhase gamePhase(GamePhaseType eIndex)                { return maGamePhases.GetOrDefault((int)eIndex); }
        public GamePhaseType gamePhasesNum()                                { return (GamePhaseType)maGamePhases.Count; }

        public List<InfoGameSetup> gameSetups()                             { return maGameSetups; }
        public InfoGameSetup gameSetup(GameSetupType eIndex)                { return maGameSetups.GetOrDefault((int)eIndex); }
        public GameSetupType gameSetupsNum()                                { return (GameSetupType)maGameSetups.Count; }
        
        public List<InfoGameSpeed> gameSpeeds()                             { return maGameSpeeds; }
        public InfoGameSpeed gameSpeed(GameSpeedType eIndex)                { return maGameSpeeds.GetOrDefault((int)eIndex); }
        public GameSpeedType gameSpeedsNum()                                { return (GameSpeedType)maGameSpeeds.Count; }

        public List<InfoGender> genders()                                   { return maGenders; }
        public InfoGender gender(GenderType eIndex)                         { return maGenders.GetOrDefault((int)eIndex); }

        public List<InfoGlobalsInt> globalsInts()                           { return maGlobalsInts; }
        public InfoGlobalsInt globalsInt(GlobalsIntType eIndex)             { return maGlobalsInts.GetOrDefault((int)eIndex); }
        public GlobalsIntType globalsIntsNum()                              { return (GlobalsIntType)maGlobalsInts.Count; }

        public List<InfoGlobalsFloat> globalsFloats()                       { return maGlobalsFloats; }
        public InfoGlobalsFloat globalsFloat(GlobalsFloatType eIndex)       { return maGlobalsFloats.GetOrDefault((int)eIndex); }
        public GlobalsFloatType globalsFloatNum()                           { return (GlobalsFloatType)maGlobalsFloats.Count; }

        public List<InfoGlobalsString> globalsStrings()                     { return maGlobalsStrings; }
        public InfoGlobalsString globalsString(GlobalsStringType eIndex)    { return maGlobalsStrings.GetOrDefault((int)eIndex); }
        public GlobalsStringType globalsStringsNum()                        { return (GlobalsStringType)maGlobalsStrings.Count; }
        
        public List<InfoGlobalsType> globalsTypes()                         { return maGlobalsTypes; }
        public InfoGlobalsType globalsType(GlobalsTypeType eIndex)          { return maGlobalsTypes.GetOrDefault((int)eIndex); }
        public GlobalsTypeType globalsTypesNum()                            { return (GlobalsTypeType)maGlobalsTypes.Count; }
        
        public List<InfoHandicap> handicaps()                               { return maHandicaps; }
        public InfoHandicap handicap(HandicapType eIndex)                   { return maHandicaps.GetOrDefault((int)eIndex); }
        public HandicapType handicapsNum()                                  { return (HandicapType)maHandicaps.Count; }
        
        public List<InfoHeight> heights()                                   { return maHeights; }
        public InfoHeight height(HeightType eIndex)                         { return maHeights.GetOrDefault((int)eIndex); }
        public HeightType heightsNum()                                      { return (HeightType)maHeights.Count; }
        
        public List<InfoHQ> HQs()                                           { return maHQs; }
        public InfoHQ HQ(HQType eIndex)                                     { return maHQs.GetOrDefault((int)eIndex); }
        public HQType HQsNum()                                              { return (HQType)maHQs.Count; }
        
        public List<InfoHQLevel> HQLevels()                                 { return maHQLevels; }
        public InfoHQLevel HQLevel(HQLevelType eIndex)                      { return maHQLevels.GetOrDefault((int)eIndex); }
        public HQLevelType HQLevelsNum()                                    { return (HQLevelType)maHQLevels.Count; }
        
        public List<InfoIce> ices()                                         { return maIces; }
        public InfoIce ice(IceType eIndex)                                  { return maIces.GetOrDefault((int)eIndex); }
        public IceType icesNum()                                            { return (IceType)maIces.Count; }

        public List<InfoKeyBinding> keyBindings()                           { return maKeyBindings; }
        public InfoKeyBinding keyBinding(KeyBindingType eIndex)             { return maKeyBindings.GetOrDefault((int)eIndex); }
        public KeyBindingType keyBindingsNum()                              { return (KeyBindingType)maKeyBindings.Count; }
        
        public List<InfoKeyBindingClass> keyBindingClasses()                { return maKeyBindingClasses; }
        public InfoKeyBindingClass keyBindingClass(KeyBindingClassType eIndex){ return maKeyBindingClasses.GetOrDefault((int)eIndex); }
        public KeyBindingClassType keyBindingClassesNum()                   { return (KeyBindingClassType)maKeyBindingClasses.Count; }
        
        public List<InfoLanguage> languages()                               { return maLanguages; }
        public InfoLanguage language(LanguageType eIndex)                   { return maLanguages.GetOrDefault((int)eIndex); }
        public LanguageType languagesNum()                                  { return (LanguageType)maLanguages.Count; }
        
        public List<InfoLatitude> latitudes()                               { return maLatitudes; }
        public InfoLatitude latitude(LatitudeType eIndex)                   { return maLatitudes.GetOrDefault((int)eIndex); }
        public LatitudeType latitudesNum()                                  { return (LatitudeType)maLatitudes.Count; }
        
        public List<InfoLevel> levels()                                     { return maLevels; }
        public InfoLevel level(LevelType eIndex)                            { return maLevels.GetOrDefault((int)eIndex); }
        public LevelType levelsNum()                                        { return (LevelType)maLevels.Count; }

        public List<InfoLightingEnvironment> lightingEnvironments()         { return maLightingEnvironments; }
        public InfoLightingEnvironment lightingEnvironment(LightingEnvironmentType eIndex) { return maLightingEnvironments.GetOrDefault((int)eIndex); }
        public LightingEnvironmentType lightingEnvironmentsNum()            { return (LightingEnvironmentType)maLightingEnvironments.Count; }

        public List<InfoLocation> locations()                               { return maLocations; }
        public InfoLocation location(LocationType eIndex)                   { return maLocations.GetOrDefault((int)eIndex); }

        public List<InfoMapName> mapNames()                                 { return maMapNames; }
        public InfoMapName mapName(MapNameType eIndex)                      { return maMapNames.GetOrDefault((int)eIndex); }
        public MapNameType mapNamesNum()                                    { return (MapNameType)maMapNames.Count; }
        
        public List<InfoMapSize> mapSizes()                                 { return maMapSizes; }
        public InfoMapSize mapSize(MapSizeType eIndex)                      { return maMapSizes.GetOrDefault((int)eIndex); }
        public MapSizeType mapSizesNum()                                    { return (MapSizeType)maMapSizes.Count; }

        public List<InfoMarkup> markups()                                   { return maMarkups; }
        public InfoMarkup markup(MarkupType eIndex)                         { return maMarkups.GetOrDefault((int)eIndex); }
        public MarkupType markupsNum()                                      { return (MarkupType)maMarkups.Count; }
        
        public List<InfoModule> modules()                                   { return maModules; }
        public InfoModule module(ModuleType eIndex)                         { return maModules.GetOrDefault((int)eIndex); }
        public ModuleType modulesNum()                                      { return (ModuleType)maModules.Count; }
        
        public List<InfoOrder> orders()                                     { return maOrders; }
        public InfoOrder order(OrderType eIndex)                            { return maOrders.GetOrDefault((int)eIndex); }
        public OrderType ordersNum()                                        { return (OrderType)maOrders.Count; }

        public List<InfoOrdinal> ordinals()                                 { return maOrdinals; }
        public InfoOrdinal ordinal(OrdinalType eIndex)                      { return maOrdinals.GetOrDefault((int)eIndex); }
        public OrdinalType ordinalNum()                                     { return (OrdinalType)maOrdinals.Count; }
        
        public List<InfoPatent> patents()                                   { return maPatents; }
        public InfoPatent patent(PatentType eIndex)                         { return maPatents.GetOrDefault((int)eIndex); }
        public PatentType patentsNum()                                      { return (PatentType)maPatents.Count; }
        
        public List<InfoPerk> perks()                                       { return maPerks; }
        public InfoPerk perk(PerkType eIndex)                               { return maPerks.GetOrDefault((int)eIndex); }
        public PerkType perksNum()                                          { return (PerkType)maPerks.Count; }
        
        public List<InfoPersonality> personalities()                        { return maPersonalities; }
        public InfoPersonality personality(PersonalityType eIndex)          { return maPersonalities.GetOrDefault((int)eIndex); }
        public PersonalityType personalitiesNum()                           { return (PersonalityType)maPersonalities.Count; }

        public List<InfoPlayerColor> playerColors()                         { return maPlayerColors; }
        public InfoPlayerColor playerColor(PlayerColorType eIndex)          { return maPlayerColors.GetOrDefault((int)eIndex); }
        public PlayerColorType playerColorsNum()                            { return (PlayerColorType)maPlayerColors.Count; }

        public List<InfoPlayerOption> playerOptions()                       { return maPlayerOptions; }
        public InfoPlayerOption playerOption(PlayerOptionType eIndex)       { return maPlayerOptions.GetOrDefault((int)eIndex); }
        
        public List<InfoResource> resources()                               { return maResources; }
        public InfoResource resource(ResourceType eIndex)                   { return maResources.GetOrDefault((int)eIndex); }
        public ResourceType resourcesNum()                                  { return (ResourceType)maResources.Count; }

        public List<InfoResourceColor> resourceColors()                     { return maResourceColors; }
        public InfoResourceColor resourceColor(ResourceColorType eIndex)    { return maResourceColors.GetOrDefault((int)eIndex); }
        public ResourceColorType resourceColorsNum()                        { return (ResourceColorType)maResourceColors.Count; }
        
        public List<InfoResourceLevel> resourceLevels()                     { return maResourceLevels; }
        public InfoResourceLevel resourceLevel(ResourceLevelType eIndex)    { return maResourceLevels.GetOrDefault((int)eIndex); }
        public ResourceLevelType resourceLevelsNum()                        { return (ResourceLevelType)maResourceLevels.Count; }
        
        public List<InfoResourceMinimum> resourceMinimums()                 { return maResourceMinimums; }
        public InfoResourceMinimum resourceMinimum(ResourceMinimumType eIndex){ return maResourceMinimums.GetOrDefault((int)eIndex); }
        public ResourceMinimumType resourceMinimumsNum()                    { return (ResourceMinimumType)maResourceMinimums.Count; }
        
        public List<InfoResourcePresence> resourcePresences()               { return maResourcePresences; }
        public InfoResourcePresence resourcePresence(ResourcePresenceType eIndex){ return maResourcePresences.GetOrDefault((int)eIndex); }
        public ResourcePresenceType resourcePresencesNum()                  { return (ResourcePresenceType)maResourcePresences.Count; }
        
        public List<InfoRulesSet> rulesSets()                               { return maRulesSets; }
        public InfoRulesSet rulesSet(RulesSetType eIndex)                   { return maRulesSets.GetOrDefault((int)eIndex); }
        public RulesSetType rulesSetsNum()                                  { return (RulesSetType)maRulesSets.Count; }
        
        public List<InfoSabotage> sabotages()                               { return maSabotages; }
        public InfoSabotage sabotage(SabotageType eIndex)                   { return maSabotages.GetOrDefault((int)eIndex); }
        public SabotageType sabotagesNum()                                  { return (SabotageType)maSabotages.Count; }
        
        public List<InfoScenario> scenarios()                               { return maScenarios; }
        public InfoScenario scenario(ScenarioType eIndex)                   { return maScenarios.GetOrDefault((int)eIndex); }
        public ScenarioType scenariosNum()                                  { return (ScenarioType)maScenarios.Count; }

        public List<InfoScenarioClass> scenarioClasses()                    { return maScenarioClasses; }
        public InfoScenarioClass scenarioClass(ScenarioClassType eIndex)    { return maScenarioClasses.GetOrDefault((int)eIndex); }

        public List<InfoScenarioDifficulty> scenarioDifficulties() { return maScenarioDifficulties; }
        public InfoScenarioDifficulty scenarioDifficulty(ScenarioDifficultyType eIndex) { return maScenarioDifficulties.GetOrDefault((int)eIndex); }
        public ScenarioDifficultyType scenarioDifficultiesNum() { return (ScenarioDifficultyType)maScenarioDifficulties.Count; }

        public List<InfoSevenSols> sevenSols()                              { return maSevenSols; }
        public InfoSevenSols sevenSol(SevenSolsType eIndex)                 { return maSevenSols.GetOrDefault((int)eIndex); }
        public SevenSolsType sevenSolsNum()                                 { return (SevenSolsType)maSevenSols.Count; }

        public List<InfoSpriteGroup> spriteGroups()                         { return maSpriteGroups; }
        public InfoSpriteGroup spriteGroup(SpriteGroupType eIndex)          { return maSpriteGroups.GetOrDefault((int)eIndex); }
        public SpriteGroupType spriteGroupsNum()                            { return (SpriteGroupType)maSpriteGroups.Count; }

        public List<InfoStory> stories()                                    { return maStories; }
        public InfoStory story(StoryType eIndex)                            { return maStories.GetOrDefault((int)eIndex); }
        public StoryType storiesNum()                                       { return (StoryType)maStories.Count; }

        public List<InfoTechnology> technologies()                          { return maTechnologies; }
        public InfoTechnology technology(TechnologyType eIndex)             { return maTechnologies.GetOrDefault((int)eIndex); }
        public TechnologyType technologiesNum()                             { return (TechnologyType)maTechnologies.Count; }
        
        public List<InfoTechnologyLevel> technologyLevels()                 { return maTechnologyLevels; }
        public InfoTechnologyLevel technologyLevel(TechnologyLevelType eIndex){ return maTechnologyLevels.GetOrDefault((int)eIndex); }
        public TechnologyLevelType technologyLevelsNum()                    { return (TechnologyLevelType)maTechnologyLevels.Count; }
        
        public List<InfoTerrain> terrains()                                 { return maTerrains; }
        public InfoTerrain terrain(TerrainType eIndex)                      { return maTerrains.GetOrDefault((int)eIndex); }
        public TerrainType terrainsNum()                                    { return (TerrainType)maTerrains.Count; }
        
        public List<InfoTerrainClass> terrainClasses()                      { return maTerrainClasses; }
        public InfoTerrainClass terrainClass(TerrainClassType eIndex)       { return maTerrainClasses.GetOrDefault((int)eIndex); }
        public TerrainClassType terrainClassesNum()                         { return (TerrainClassType)maTerrainClasses.Count; }

        public List<InfoTerrainMaterial> terrainMaterials()                 { return maTerrainMaterials; }
        public InfoTerrainMaterial terrainMaterial(TerrainMaterialType eIndex)  { return maTerrainMaterials.GetOrDefault((int)eIndex); }

        public List<InfoText> texts()                                       { return maTexts; }
        public InfoText text(TextType eIndex)                               { return maTexts.GetOrDefault((int)eIndex); }
        public TextType textsNum()                                          { return (TextType)maTexts.Count; }
        
        public List<InfoUnit> units()                                       { return maUnits; }
        public InfoUnit unit(UnitType eIndex)                               { return maUnits.GetOrDefault((int)eIndex); }
        public UnitType unitsNum()                                          { return (UnitType)maUnits.Count; }

        public List<InfoWind> winds()                                       { return maWinds; }
        public InfoWind wind(WindType eIndex)                               { return maWinds.GetOrDefault((int)eIndex); }
        public WindType windsNum()                                          { return (WindType)maWinds.Count; }

        public List<InfoWorldAudio> worldAudios()                           { return maWorldAudios; }
        public InfoWorldAudio worldAudio(WorldAudioType eIndex)             { return maWorldAudios.GetOrDefault((int)eIndex); }
        public WorldAudioType worldAudioNum()                               { return (WorldAudioType)maWorldAudios.Count; }
    }
}