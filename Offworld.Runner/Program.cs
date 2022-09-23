using System.Runtime.InteropServices;
using Offworld.Challenge;

namespace Offworld.Runner
{
    public class Program
    {
        static Random Random = new Random(1337);
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                args = new string[] {
                    $"DefaultAI", // typeof(Algorithms.DefaultAI).Name
                    $"{true}",
                    $"{int.MaxValue}"
                };
            }

            Type algorithm = Utils.GetAlgorithm(args[0]);
            bool debug = bool.Parse(args[1]);
            int numRuns = int.Parse(args[2]);

            for (int i = 0; i < numRuns; i++)
            {
                Run(algorithm, debug);
            }
        }
        public static void Run(Type algorithm, bool debug = false)
        {
            var start = DateTime.Now;
            var difficulty = new Difficulty(
                // handicap for your AI. 0 = Applicant, ..., 8 = Guru (in-game difficulty setting)
                yourHandicap: Random.Next(0, 9),

                //handicap for opponent AI. 0 = Guru, ..., 8 = Applicant (in-game difficulty setting)
                opponentDifficulty: Random.Next(0, 9)
            );
            int seed = Random.Next();
            var challenge = difficulty.GenerateChallenge(seed);
            var solveResult = challenge.Solve(algorithm, debugMode: debug);

            Console.WriteLine($"Offworld, {algorithm.Name}, {seed}, {difficulty.YourHandicap}, {difficulty.OpponentDifficulty}, {solveResult.IsSolution}, {(DateTime.Now - start).TotalSeconds}");

            // you can save the replay and watch it in-game
            string replayFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                replayFolder = Path.Combine(replayFolder, "My Games/Offworld/Replays");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                replayFolder = Path.Combine(replayFolder, "Library/Application Support/Offworld/Replays");

            if (Directory.Exists(replayFolder))
            {
                var replayPath = Path.Combine(replayFolder, "the-innovation-game-replay.rmp");
                File.WriteAllBytes(
                    replayPath,
                    solveResult.Solution.Replay
                );
                if (debug) Console.WriteLine($"Replay saved to: {replayPath}");
            }
        }
    }
}
