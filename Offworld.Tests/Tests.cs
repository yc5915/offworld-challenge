using Xunit;
using Offworld.Challenge;

namespace Offworld.Tests
{
    [Collection("Offworld")]
    public class Tests
    {

        [Theory]
        [InlineData("Offworld,0:0,Heirloom-5,1,1850,1242546b5c8dff013ac466e8eeb4d1f2,DefaultAI,134102148")]
        public void Solve_Works(string encoding)
        {
            var challengeParams = Params.FromEncoding(encoding);
            challengeParams.Solve(60);
        }

        [Theory]
        [InlineData("Offworld,0:0,Heirloom-5,1,1850,1242546b5c8dff013ac466e8eeb4d1f2,DefaultAI,134102148")]
        public void Solution_Encoding_Works(string encoding)
        {
            var _params = Params.FromEncoding(encoding);
            var solveResult = _params.Solve(60);
            var proof = solveResult.Solution.ToProof();
            var b = Solution.FromProof(proof).VerifySolutionAndMethod(_params, 30);
        }
    }
}
