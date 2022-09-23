using UnityEngine;
using System.Collections;

namespace Offworld.AppCore
{
    public interface IModEntryPoint
    {
        void Initialize(); //called after loading each mod .dll
        void Shutdown(); //called when unloading each mod .dll
        void SetScenarioType(string scenarioType); //called when the user chooses a scenario
        void Serialize(object stream); //called to serialize the mod state for saving/loading
        void Update(); //called once per frame
        void PostUpdate(); //called once per frame after all Update() calls
        void OnGUI(); //called by Unity's immediate-mode UI: https://docs.unity3d.com/Manual/GUIScriptingGuide.html
        void OnPreGameServer(); //called before a new server is created
        void OnGameServerReady(); //called after a new server is created
        void OnGameClientReady(); //called after a new client is created
        void OnGameReady(); //called after the loading screen drops
        void OnPreLoad(); //called before a server is deserialized
        void OnPostLoad(); //called after a server is deserialized
        void OnPreLoadGameClient(); //called before a client is deserialized
        void OnPostLoadGameClient(); //called after a client is deserialized
        void OnUIReady(); //called when the HUD is ready for commands
        void OnRendererReady(); //called when the Renderer is ready for commands
        void OnAudioReady(); //called when the Audio system is ready for commands
        void OnGameStateChanged(); //called after the client founds their HQ
        void OnExitGame(); //called when exiting the current game
    }

    public class ModEntryPointAdapter : IModEntryPoint
    {
        public virtual void Initialize() { }
        public virtual void Shutdown() { }
        public virtual void SetScenarioType(string scenarioType) { }
        public virtual void Serialize(object stream) { }
        public virtual void Update() { }
        public virtual void PostUpdate() { }
        public virtual void OnGUI() { }
        public virtual void OnPreGameServer() { }
        public virtual void OnGameServerReady() { }
        public virtual void OnGameClientReady() { }
        public virtual void OnGameReady() { }
        public virtual void OnPreLoad() { }
        public virtual void OnPostLoad() { }
        public virtual void OnPreLoadGameClient() { }
        public virtual void OnPostLoadGameClient() { }
        public virtual void OnUIReady() { }
        public virtual void OnRendererReady() { }
        public virtual void OnAudioReady() { }
        public virtual void OnGameStateChanged() { }
        public virtual void OnExitGame() { }
    }

}