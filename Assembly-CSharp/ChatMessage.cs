
using Offworld.GameCore;
using Offworld.SystemCore;

public class ChatMessage
{
    public int sendingPlayer;
    public bool team;
    public string message;
    public string observerName;

    public ChatMessage()
    {
    }

    public ChatMessage(PlayerType sendingPlayer, bool team, string message, string observerName)
    {
        this.sendingPlayer = (int)(sbyte)sendingPlayer;
        this.team = team;
        this.message = message;
        this.observerName = observerName;
    }

    public void Serialize(object stream)
    {
        SimplifyIO.Data(stream, ref this.sendingPlayer, "sendingPlayer");
        SimplifyIO.Data(stream, ref this.team, "team");
        SimplifyIO.Data(stream, ref this.message, "message");
        SimplifyIO.Data(stream, ref this.observerName, "observerName");
    }
}
