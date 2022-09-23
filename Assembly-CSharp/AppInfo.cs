using System;
using Offworld.GameCore;

public class AppInfo : IAppInfo
{
    public int GetDailyChallengeSeed => 1337;
    public bool IsInternalBuild => false;
    public bool IsObserver => false;
    public bool IsInternalMod => false;
    public GameModeType GameMode => GameModeType.SKIRMISH;
    public string UserDataPath => string.Empty;
    public string UserCloudDataPath => string.Empty;
    public string UserMapSavePath => string.Empty;
    public string UserSkirmishSavePath => string.Empty;
    public string UserCampaignSavePath => string.Empty;
    public string UserDailyChallengeSavePath => string.Empty;
    public string UserInfiniteChallengeSavePath => string.Empty;
    public bool OwnsDLCRealMaps => false;
    public bool OwnsDLCScenarioToolkit => false;
    public bool OwnsDLCCeres => false;
    public bool OwnsDLCCampaign => false;
    public bool OwnsDLCBlues => false;
    public bool OwnsDLCBobScenarios => false;
    public bool OwnsDLCBobCampaign => false;
    public bool OwnsDLCIo => false;
    public bool OwnsDLCEuropa => false;
    public bool OwnsDLCDimension => false;
    public bool OwnsMapDLC => false;
    public bool OwnsDLCArtPack => false;
    public bool OwnsFullGame => false;
    public string GetPlayerRank(PlayerType playerIndex) => string.Empty;
    public Guid GetTachyonID(PlayerType playerIndex) => Guid.Empty;
    public Guid GetLocalTachyonID() => Guid.Empty;
}
