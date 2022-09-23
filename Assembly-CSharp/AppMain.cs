using Offworld.GameCore;

public class AppMain
{
    public static AppMain gApp = new AppMain();
    private GameServer mGameServer;
    public GameServer gameServer() => mGameServer;
    public void setGameServer(GameServer gameServer) => mGameServer = gameServer;
}
