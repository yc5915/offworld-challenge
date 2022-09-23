using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Offworld.Challenge
{
    public class AlgorithmNotFoundException : Exception
    {
        public AlgorithmNotFoundException(string message) : base(message) { }
    }

    public class Utils
    {
        public static double CalcRemainingMaxSecondsTaken(DateTime start, double maxSecondsTaken)
        {
            return maxSecondsTaken - (DateTime.Now - start).TotalSeconds;
        }

        public static string ComputeProofFileHash(byte[] proof)
        {
            return Convert.ToHexString(MD5.Create().ComputeHash(proof));
        }
        public static string ChallengeType => typeof(Utils).Namespace.Split('.')[0];
        public static Type GetAlgorithm(string algorithmName)
        {
            string ns = $"{ChallengeType}.Algorithms";
            Type algorithm = Type.GetType($"{ns}.{algorithmName}, {ns}");
            if (algorithm == null)
            {
                throw new AlgorithmNotFoundException($"Algorithm '{algorithmName}' does not exist for challenge type '{ChallengeType}'.");
            }
            Type abstractAlgo = Type.GetType($"{ns}.Abstract, {ns}");
            if (!algorithm.IsSubclassOf(abstractAlgo))
            {
                throw new AlgorithmNotFoundException($"Algorithm '{algorithmName}' for challenge type '{ChallengeType}' is not derived from the Abstract class.");
            }
            return algorithm;
        }

        public static List<string> ListAlgorithms()
        {
            string ns = $"{ChallengeType}.Algorithms";
            Type abstractAlgo = Type.GetType($"{ns}.Abstract, {ns}");
            return (
                from t in System.Reflection.Assembly.Load(ns).GetTypes()
                where t.IsSubclassOf(abstractAlgo) && t.FullName.StartsWith(ns) && t.Name != "Template"
                select t.Name
            ).ToList();
        }
    }

    public class Object
    {
        public override string ToString()
        {
            return string.Join(", ", GetType().GetProperties().Select(
                prop => $"{prop.Name}: {prop.GetValue(this)}"
                ).ToArray());
        }

        public override bool Equals(object obj) => this == (obj as Object);

        public override int GetHashCode()
        {
            return GetType().GetProperties().Sum(prop => prop.GetValue(this).GetHashCode());
        }

        public static bool operator ==(Object lhs, Object rhs)
        {
            if (lhs is null || rhs is null)
                return lhs is null && rhs is null;
            return (
                lhs.GetType() == rhs.GetType() &&
                lhs.GetType().GetProperties().All(
                    prop => prop.GetValue(lhs) == prop.GetValue(rhs)
                )
            );
        }
        public static bool operator !=(Object lhs, Object rhs) => !(lhs == rhs);
    }

    public class Params : Object
    {
        public string ChallengeType => GetType().Namespace.Split('.')[0];
        public Difficulty Difficulty { get; init; }
        public string UserHandle { get; init; }
        public int Week { get; init; }
        public int BenchmarkRound { get; init; }
        public string RandHash { get; init; }
        public string AlgorithmName { get; init; }
        public int Nonce { get; init; }

        public Challenge GenerateChallenge()
        {
            int seed = ComputeSeed();
            return Difficulty.GenerateChallenge(seed);
        }

        public int ComputeSeed()
        {
            return BitConverter.ToInt32(
                SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(ToEncoding())),
                28 // 32 bytes total, the least significant 4 bytes (32 bits) are most random
            );
        }

        public static Params FromEncoding(string encoding)
        {
            string[] paramFields = encoding.Split(',');
            string[] difficultyFields = paramFields[1].Split(':');

            Difficulty difficulty = Difficulty.FromEncoding(difficultyFields);

            return new Params()
            {
                Difficulty = difficulty,
                UserHandle = paramFields[2],
                Week = int.Parse(paramFields[3]),
                BenchmarkRound = int.Parse(paramFields[4]),
                RandHash = paramFields[5],
                AlgorithmName = paramFields[6],
                Nonce = int.Parse(paramFields[7])
            };
        }

        public string ToEncoding()
        {
            return string.Join(',', new string[] {
                ChallengeType,
                string.Join(':', Difficulty.Fields()),
                UserHandle.ToString(),
                BenchmarkRound.ToString(),
                RandHash.ToString(),
                AlgorithmName.ToString(),
                Nonce.ToString(),
            });
        }

        public SolveResult Solve(double maxSecondsTaken = double.MaxValue, bool debugMode = false)
        {
            Challenge challenge = GenerateChallenge();
            Type algorithm = GetAlgorithm();
            return challenge.Solve(algorithm, maxSecondsTaken, debugMode);
        }

        public Type GetAlgorithm() => Utils.GetAlgorithm(AlgorithmName);
    }
    public class SolveResult
    {
        public bool IsSolution { init; get; }
        public Solution Solution { init; get; }
    }
    public partial class Solution
    {
        public byte[] ToProof()
        {
            using (MemoryStream data = new MemoryStream())
            using (GZipStream gzip = new GZipStream(data, CompressionMode.Compress))
            using (BinaryWriter writer = new BinaryWriter(gzip, Encoding.UTF8))
            {
                writer.Write(Utils.ChallengeType);
                Write(writer);
                gzip.Close();
                return data.ToArray();
            }
        }

        public static Solution FromProof(byte[] proof)
        {
            using (var data = new MemoryStream(proof))
            using (var gzip = new GZipStream(data, CompressionMode.Decompress))
            using (BinaryReader reader = new BinaryReader(gzip, Encoding.UTF8))
            {
                var challengeType = reader.ReadString();
                if (challengeType != Utils.ChallengeType)
                    throw new ArgumentException($"Expecting ChallengeType '{Utils.ChallengeType}', got '{challengeType}'");
                return Read(reader);
            }
        }

        public bool VerifySolutionAndMethod(
            Params _params, double maxSecondsTaken = double.MaxValue
        )
        {
            Challenge challenge = _params.GenerateChallenge();
            Type algorithm = _params.GetAlgorithm();
            return (
                VerifySolutionOnly(challenge, maxSecondsTaken) &&
                VerifyMethodOnly(challenge, algorithm, maxSecondsTaken)
            );
        }

        public bool VerifySolutionOnly(
            Params _params, double maxSecondsTaken = double.MaxValue
        )
        {
            Challenge challenge = _params.GenerateChallenge();
            return VerifySolutionOnly(challenge, maxSecondsTaken);
        }
    }
}
