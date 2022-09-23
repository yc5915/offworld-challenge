using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Offworld.GameCore
{
    [Serializable]
    public class CampaignSettings
    {
        public const int serializationVersion = 0;
        public Dictionary<GameOptionType, bool> gameOptionsDictionary;
        public HandicapType handicap;
        public LocationType location;
        public CampaignModeType mode;
        public ExecutiveType executive;

        private static string CampaignSettingsSaveFileName = "/CampaignSettings.bin";
        private static string CampaignSettingsFullPath { get { return Globals.AppInfo.UserCloudDataPath + CampaignSettingsSaveFileName; } }

        public CampaignSettings()
        {
        }

        public CampaignSettings(CampaignSettings previousSettings)
        {
            gameOptionsDictionary = new Dictionary<GameOptionType, bool>(previousSettings.gameOptionsDictionary);
            handicap = previousSettings.handicap;
            location = previousSettings.location;
            mode = previousSettings.mode;
            executive = previousSettings.executive;
        }

        public void Init()
        {
            gameOptionsDictionary = new Dictionary<GameOptionType, bool>();
            handicap = HandicapType.NONE;
            location = LocationType.NONE;
            mode = CampaignModeType.NONE;
            executive = ExecutiveType.NONE;
        }

        public void SetDefaults()
        {
            Infos info = Globals.Infos;
            handicap = info.Globals.CAMPAIGN_HANDICAP;
            location = LocationType.MARS;
            mode = info.location(location).meDefaultCampaignMode;
            foreach (InfoGameOption infoGameOption in info.gameOptions())
            {
                if (infoGameOption.mbCampaignOption)
                {
                    gameOptionsDictionary.Add(infoGameOption.meType, infoGameOption.mbDefaultValueCampaign);
                }
            }
        }

        public void Serialize()
        {
            try
            {
                {
                    IFormatter formatter = new BinaryFormatter();
                    using (Stream stream = new FileStream(CampaignSettingsFullPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        formatter.Serialize(stream, this);
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Error while trying to serialize campaign settings data: " + e.ToString());
            }
        }

        public static CampaignSettings Deserialize()
        {
            if (!File.Exists(CampaignSettingsFullPath))
                return null;

            try
            {
                IFormatter formatter = new BinaryFormatter();
                using (Stream stream = new FileStream(CampaignSettingsFullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    return (CampaignSettings)formatter.Deserialize(stream);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("Error while trying to deserialize campaign data: " + e.ToString());
                return null;
            }
        }
    }
}