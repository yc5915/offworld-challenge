using Offworld.GameCore;
using Offworld.AppCore;
public class GameGlobals : IGameGlobals
{
    public GameServer GameServer => AppMain.gApp.gameServer();
    public GameClient GameClient => null;
    public PlayerType ActivePlayer => PlayerType.NONE;
    public PlayerType ActivePlayerOriginal => PlayerType.NONE;
    public PlayerClient ActivePlayerClient => null;
    public TeamType ActiveTeam => TeamType.NONE;
    public ScenarioType ActiveScenario => ScenarioType.NONE;
}
