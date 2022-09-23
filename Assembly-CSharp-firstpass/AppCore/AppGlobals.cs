using UnityEngine.Assertions;
using Offworld.SystemCore;

namespace Offworld.AppCore
{
    public static class AppGlobals
    {
        private static IGameHUDHelpers gameHUDHelpers;
        private static IGameGlobals gameGlobals;
        private static IGameRenderHelpers gameRenderHelpers;

        public static IGameHUDHelpers GameHUDHelpers  { get { return gameHUDHelpers;   } }
        public static IGameGlobals GameGlobals        { get { return gameGlobals;      } }
        public static IGameRenderHelpers GameRenderHelpers { get { return gameRenderHelpers;    } }

        public static void Initialize(IGameHUDHelpers gameHUDHelpers, IGameGlobals gameGlobals, IGameRenderHelpers gameRenderHelpers)
        {
            using (new UnityProfileScope("AppGlobals.Initialize"))
            {
                Assert.IsNotNull(gameHUDHelpers);
                Assert.IsNotNull(gameGlobals);
                Assert.IsNotNull(gameRenderHelpers);
                AppGlobals.gameHUDHelpers = gameHUDHelpers;
                AppGlobals.gameGlobals = gameGlobals;
                AppGlobals.gameRenderHelpers = gameRenderHelpers;
            }
        }
    }
}