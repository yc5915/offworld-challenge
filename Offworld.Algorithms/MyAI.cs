using Offworld.AppCore;
using Offworld.GameCore;
using UnityEngine;

namespace Offworld.Algorithms
{
    public class MyAI : Abstract
    {
        public MyAI(GameClient pGame, bool liveGame, bool debugMode) : base(pGame, liveGame, debugMode)
        {
        }
        public override void AI_doActions(bool is_auction)
        {
            /*
                Your Objective:
                     Buyout the opponent AI's corporation within 7 days
                     AI_doActions will be called once per turn in the game
                Rules:
                     1. You must implement this function
                     2. You are not allowed to use external libraries
                     3. If you need to generate random numbers, you must use this.Random
                     4. You are allowed to read any state from the game
                     5. You are only allowed to make actions for your player
                        (see Assembly-CSharp-firstpass->GameCore->Game.cs->handleAction for all the actions you can make)
                     6. You are not allowed to change the gameoptions
                     7. You are allowed to add new stubs for Assembly-CSharp and/or UnityEngine as necessary for debugging in-game
                     8. If you want to give up on the challenge (e.g. maybe its unwinnable), you should call concede()
                     9. Your class name and filename must be `<algorithm_name>.cs`
                     10. All your utility classes should be contained in a separate namespace `TheInnovationGame.<challenge_type>.Algorithms.<algorithm_name>Utils`
                     11. If you are improving an existing algorithm, make a copy of the code before making modifications
                     12. Your algorithm name must be less than or equal to 20 characters
                Tips:
                    This is the hardest challenge type
                    It is highly recommended that you get started by copying DefaultAI and making modifications
                    See the README.md
             */
            if (DebugMode)
            {
                Debug.Log($"Hello world!");
                if (LiveGame)
                {
                    // Only invoke AppGlobals functions if its a live game
                    AppGlobals.GameHUDHelpers.DisplayChatMessage(getPlayer(), false, "Hello world!");
                    // Other useful debug functions
                    // AppGlobals.GameHUDHelpers.CreateWorldText
                    // AppGlobals.GameHUDHelpers.HighlightTiles
                }
            }
        }
    }
}
