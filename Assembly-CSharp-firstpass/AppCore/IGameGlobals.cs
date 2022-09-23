using Offworld.GameCore;

namespace Offworld.AppCore
{
    public interface IGameGlobals
    {
        GameServer GameServer { get; }
        GameClient GameClient { get; }
        PlayerType ActivePlayer { get; }
        PlayerType ActivePlayerOriginal { get; }
        PlayerClient ActivePlayerClient { get; }
        TeamType ActiveTeam { get; }
        ScenarioType ActiveScenario { get; }
    }
}