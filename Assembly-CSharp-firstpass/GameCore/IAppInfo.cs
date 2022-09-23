using System;

namespace Offworld.GameCore
{
    public interface IAppInfo
    {
        int GetDailyChallengeSeed { get;  }
        bool IsInternalBuild { get; }
        bool IsObserver { get; }
        bool IsInternalMod { get; }
        GameModeType GameMode { get; }
        string UserDataPath { get; }
        string UserCloudDataPath { get; }
        string UserMapSavePath { get ; }
        string UserSkirmishSavePath { get; }
        string UserCampaignSavePath { get; }
        bool OwnsDLCRealMaps { get; }
        bool OwnsDLCScenarioToolkit { get; }
        bool OwnsDLCCeres { get; }
        bool OwnsDLCCampaign { get; }
        bool OwnsDLCBlues { get; }
        bool OwnsDLCBobScenarios { get; }
        bool OwnsDLCBobCampaign { get; }
        bool OwnsDLCIo { get; }
        bool OwnsDLCEuropa { get; }
        bool OwnsMapDLC { get; }
        bool OwnsDLCArtPack { get; }
        bool OwnsFullGame { get; }
        string GetPlayerRank(PlayerType playerIndex);
        Guid GetTachyonID(PlayerType playerIndex);
        Guid GetLocalTachyonID();
    }
}
