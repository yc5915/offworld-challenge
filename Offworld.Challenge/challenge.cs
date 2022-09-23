using System;
using System.IO;
using System.Linq;
using Offworld.AppCore;
using Offworld.GameCore;
using Offworld.MyGameCore;

namespace Offworld.Challenge
{
    public class MismatchedAlgorithm : Exception
    {
        public MismatchedAlgorithm(string message) : base(message) { }
    }
    public class Difficulty
    {
        public int YourHandicap { get; init; }
        public int OpponentDifficulty { get; init; }

        public static Difficulty FromEncoding(string[] fields)
        {
            return new Difficulty(
                    yourHandicap: int.Parse(fields[0]),
                    opponentDifficulty: int.Parse(fields[1])
                );
        }

        public Difficulty(int yourHandicap, int opponentDifficulty)
        {
            if (yourHandicap < 0 || yourHandicap > 8)
                throw new ArgumentException("Value for Offworld.Difficulty.YourHandicap must be between 0 and 8");
            if (opponentDifficulty < 0 || opponentDifficulty > 8)
                throw new ArgumentException("Value for Offworld.Difficulty.OpponentDifficulty must be between 0 and 8");
            YourHandicap = yourHandicap;
            OpponentDifficulty = opponentDifficulty;
        }

        public string[] Fields()
        {
            return new[] {
                YourHandicap.ToString(),
                OpponentDifficulty.ToString(),
            };
        }

        public Challenge GenerateChallenge(int seed)
        {
            return new Challenge(this, seed);
        }
    }

    public partial class Solution
    {
        public byte[] Replay { get; init; }

        public Solution(byte[] replay)
        {
            Replay = replay;
        }
        protected void Write(BinaryWriter writer)
        {
            writer.Write(Replay.Length);
            writer.Write(Replay);
        }

        public bool VerifySolutionOnly(Challenge challenge, double maxSecondsTaken = double.MaxValue)
        {
            DateTime start = DateTime.Now;

            byte[] update;

            // first message should have game settings
            using (MemoryStream replayStream = new MemoryStream(Replay))
            {
                update = new ReplayFile(replayStream).Read();

                // check game setup in replay matches the game setup generated from challenge
                challenge.CreateGameServer(typeof(Algorithms.DefaultAI));
                if (!update.SequenceEqual(MNetwork.GetGameUpdate()))
                    return false;

                // actually playback the replay
                AppMain.gApp.setGameServer(null); // set to null as GameServer is created in MNetwork.ProcessGameUpdate
                replayStream.Position = 0;
                ReplayFile replayFile = new ReplayFile(replayStream);
                while ((update = replayFile.Read()) != null)
                {
                    if (Utils.CalcRemainingMaxSecondsTaken(start, maxSecondsTaken) <= 0)
                        throw new TimeoutException($"VerifySolutionSolvesChallenge exceeded maxSecondsTaken '{maxSecondsTaken}'");
                    MNetwork.ProcessGameUpdate(update);
                }

                bool gameWon = AppMain.gApp.gameServer().getWinningTeam() == (TeamType)challenge.GameSettings.playerSlots[0].Team;
                return gameWon;
            }
        }

        public bool VerifyMethodOnly(Challenge challenge, Type algorithm, double maxSecondsTaken = double.MaxValue)
        {
            DateTime start = DateTime.Now;

            using (MemoryStream replayStream = new MemoryStream(Replay))
            {
                ReplayFile replayFile = new ReplayFile(replayStream);

                GameServer gameServer = challenge.CreateGameServer(algorithm);

                // check every update from replay matches every update from simulating the game
                bool valid = MNetwork.GetGameUpdate().SequenceEqual(replayFile.Read());
                while (valid && !gameServer.isGameOver() && gameServer.getDays() < 7)
                {
                    if (Utils.CalcRemainingMaxSecondsTaken(start, maxSecondsTaken) <= 0)
                        throw new TimeoutException($"VerifyAlgorithmUsedToSolveChallenge exceeded maxSecondsTaken '{maxSecondsTaken}'");
                    gameServer.doUpdate();
                    valid = MNetwork.GetGameUpdate().SequenceEqual(replayFile.Read());

                    if (!challenge.IsTeamWinEligible()) // stop if our team lost
                        break;
                }

                if (valid)
                    return replayFile.Read() == null; // check replay has finished
                else
                    return false;
            }
        }

        public static Solution Read(BinaryReader reader)
        {
            return new Solution(
                replay: reader.ReadBytes(reader.ReadInt32())
            );
        }
    }

    public class ChallengeGameSettings : GameSettings
    {
        public ChallengeGameSettings(Difficulty difficulty, int seed)
        {
            meLocation = LocationType.MARS;
            miNumPlayers = 2;
            miNumHumans = 0;
            meMapSize = MapSizeType.SMALL;
            meGameSpeed = GameSpeedType.FAST;
            miSeed = seed;
            meLatitude = GameClient.getLatitudeFromSeed(seed);
            meResourceMinimum = GameClient.getResourceMinimumFromSeed(seed);
            meResourcePresence = GameClient.getResourcePresenceFromSeed(seed);
            meTerrainClass = GameClient.getTerrainClassFromSeed(seed, meLocation);
            meColonyClass = ColonyClassType.NONE;

            GameClient.getInvalidHumanHQsFromSeed(seed, mabInvalidHumanHQ);

            meGameSetupType = 0;
            meRulesSetType = 0;

            mzMap = "";
            mzMapName = "";

            int iWidth = Globals.Infos.mapSize(meMapSize).miWidth;
            int iHeight = Globals.Infos.mapSize(meMapSize).miHeight;

            iWidth *= UnityEngine.Mathf.Max(0, (Globals.Infos.terrainClass(meTerrainClass).miSizeModifier + 100));
            iWidth /= 100;

            iHeight *= UnityEngine.Mathf.Max(0, (Globals.Infos.terrainClass(meTerrainClass).miSizeModifier + 100));
            iHeight /= 100;

            miWidth = iWidth;
            miHeight = iHeight;
            miEdgeTilePadding = Constants.DEFAULT_EDGE_TILE_PADDING;

            //**************** Fill in game options ********************
            for (int i = 0; i < (int)(Globals.Infos.gameOptionsNum()); i++)
                mabGameOptions.Add(Globals.Infos.gameOption((GameOptionType)i).mbDefaultValueMP); // use Multiplayer settings

            //fill in active player teams
            HandicapType yourHandicap = (HandicapType)difficulty.YourHandicap;
            HandicapType opponentHandicap = (HandicapType)(8 - difficulty.OpponentDifficulty);
            System.Random randGender = new System.Random(seed + 1337);
            for (int iCount = 0; iCount < 10; iCount++)
            {
                var team = (sbyte)iCount;
                playerSlots.Add(new PlayerSettings(
                    name: "",
                    team: team,
                    handicap: team == 0 ? yourHandicap : opponentHandicap,
                    gender: (GenderType)randGender.Next(2),
                    artPackList: null
                ));
            }

            miNumUniqueTeams = miNumPlayers;
        }
    }
    public class Challenge
    {
        public int Seed { get; init; }
        public GameSettings GameSettings { get; init; }

        static Challenge()
        {
            AppGlobals.Initialize(new GameHUDHelpers(), new GameGlobals(), new GameRenderHelpers());
            Globals.Initialize(new InfosXMLLoader(), new NetworkImpl(), new AppInfo());
            MyGameFactory.Setup();
        }

        public Challenge(Difficulty difficulty, int seed)
        {
            Seed = seed;
            GameSettings = new ChallengeGameSettings(difficulty, seed);
        }

        public SolveResult Solve(Type algorithm, double maxSecondsTaken = double.MaxValue, bool debugMode = false)
        {
            DateTime start = DateTime.Now;

            MemoryStream replayStream = new MemoryStream();
            ReplayFile replayFile = new ReplayFile();

            GameServer gameServer = CreateGameServer(algorithm, debugMode);

            replayFile.Write(MNetwork.GetGameUpdate());
            while (!gameServer.isGameOver())
            {
                if (gameServer.getDays() >= 7)
                    break;
                if (Utils.CalcRemainingMaxSecondsTaken(start, maxSecondsTaken) <= 0)
                    throw new TimeoutException($"SolveChallenge exceeded maxSecondsTaken '{maxSecondsTaken}'");
                gameServer.doUpdate();
                replayFile.Write(MNetwork.GetGameUpdate());

                if (!IsTeamWinEligible()) // stop if our team lost
                    break;
            }

            replayFile.Close(replayStream);
            replayStream.Position = 0;
            bool gameWon = gameServer.getWinningTeam() == (TeamType)GameSettings.playerSlots[0].Team;
            return new SolveResult()
            {
                IsSolution = gameWon,
                Solution = new Solution(replay: replayStream.ToArray())
            };
        }

        public GameServer CreateGameServer(Type algorithm, bool debugMode = false)
        {
            for (int i = 0; i < GameSettings.miNumPlayers; i++)
            {
                bool myTeam = GameSettings.playerSlots[i].Team == GameSettings.playerSlots[0].Team;
                Type playerAI = myTeam ? algorithm : typeof(Algorithms.DefaultAI);
                MyGameFactory.playerAI.Add(playerAI);
                MyGameFactory.playerHandicap.Add(GameSettings.playerSlots[i].Handicap);
            }
            Globals.SetFactory(new MyGameFactory());
            MyGameFactory.DebugMode = debugMode;
            GameServer gameServer = Globals.Factory.createGameServer();
            AppMain.gApp.setGameServer(gameServer);
            gameServer.init(Globals.Infos, GameSettings);
            gameServer.mapServer().setHasResourceInfo(true);
            return gameServer;
        }

        public bool IsTeamWinEligible()
        {
            bool teamIsWinEligible = false;
            for (int i = 0; i < GameSettings.miNumPlayers; i++)
            {
                bool myTeam = GameSettings.playerSlots[i].Team == GameSettings.playerSlots[0].Team;
                if (myTeam)
                    teamIsWinEligible |= AppMain.gApp.gameServer().playerServer((PlayerType)i).isWinEligible();
            }
            return teamIsWinEligible;
        }
    }
}
