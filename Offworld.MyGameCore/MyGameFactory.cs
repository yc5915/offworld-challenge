using System;
using System.Linq;
using System.Collections.Generic;
using Offworld.AppCore;
using Offworld.GameCore;

namespace Offworld.MyGameCore
{
    public class MyGameFactory : GameFactory
    {
        public static void Setup()
        {
            var abstractAlgo = typeof(Algorithms.Abstract);
            availableAIs = (
                from t in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                where t.IsSubclassOf(abstractAlgo) && t.Name != "Template"
                select t
            ).ToList();
            handicaps = (
                from info in Globals.Infos.handicaps()
                select info.mzType.Replace("HANDICAP_", "")
            ).ToList();
        }

        public static List<Type> availableAIs;
        public static List<string> handicaps;

        public static List<Type> playerAI = new List<Type>();
        public static List<HandicapType> playerHandicap = new List<HandicapType>();
        public static bool DebugMode = false;
        public static bool LiveGame = false;

        public static void ResetPlayers()
        {
            playerAI.Clear();
            playerHandicap.Clear();
        }

        public override GameServer createGameServer() => new MyGameServer();
        public override PlayerServer createPlayerServer(GameClient pGame)
        {
            var AI = playerAI[0]; playerAI.RemoveAt(0);
            return (PlayerServer)Activator.CreateInstance(AI, pGame, LiveGame, DebugMode);
        }
    }
}
