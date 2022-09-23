using System;
using System.Collections.Generic;
using Offworld.AppCore;
using Offworld.GameCore;
using UnityEngine;

public class GameHUDHelpers : IGameHUDHelpers
{
    public bool IsHUDReady() => false;
    public bool IsVisible(HUDElement element) => false;
    public void SetVisible(HUDElement element, bool visible) { }
    public void SetVisible(BuildingType building, bool visible) { }
    public void SetVisible(ResourceType resource, bool visible) { }
    public void SetCanMinimizeAuction(bool canMinimize) { }

    // for resource rows
    public void ToggleResourcePanelDisplayMode() { }
    public void ShowResourceNotificationArrow(ResourceType resource, PlayerType player, string title, string description, bool showIcon) { }
    public void ShowResourceNotificationArrow(ResourceType resource, PlayerType player, string title, string description, bool showIcon, BuildingType building) { }
    public void SetResourceWarning(ResourceType resource, int warning) { }
    public void SetResourceWarningInfo(ResourceType resource, int warnLevel, Color color, float alphaMultiplier, float animationSpeed) { }
    public void RemoveResourceWarningInfo(ResourceType resource, int warnLevel) { }
    public void ResetResourceWarningInfo(ResourceType resource) { }
    public void ClearResourceWarningInfo(ResourceType resource) { }
    public void SetResourceWarningToolTip(ResourceType resource, string description) { }
    public void SetResourceRowEnabled(ResourceType resource, bool enabled) { }

    public void ShowPopup(string title, string description, string okButton, Action callback) { }
    public void ShowCharacterPopup(CharacterType eCharacter, string title, string description, List<string> buttons, List<Action> callbacks, bool isLeftAligned) { }
    //void ShowCharacterPopup(CharacterType eCharacter, string title, string description, List<string> buttons, List<Action> callbacks, GuiUtils.CharacterPopupAlignment alignment) { }
    public void ShowOptionsPopup(string title, string description, List<string> buttons, List<Action> callbacks) { }
    public void SetCameraBoundaries(Vector3 min, Vector3 max) { }
    public void SetCameraMinZoom(float newMin) { }
    public void SetCameraMaxZoom(float newMax) { }
    public Vector3 GetCurrentCameraLookAt() => Vector3.one;
    public void SetSelectedTile(TileClient pTile) { }
    public void CameraLookAt(Vector3 position) { }
    public void CameraLookAtZoomed(Vector3 position, float zoomRange) { }
    public void CameraFocus(Vector3 worldPosition, float blendTime) { }
    public void CameraUnfocus() { }
    public void CameraUnfocus(Vector3 position) { }
    public void CameraPan(Vector3 worldPosition, float travelTime) { }
    public void CameraPanFocused(Vector3 worldPosition, float travelTime) { }
    public void SetDataEventMuted(GameEventsClient.DataType dataType, bool isMuted) { }
    public void SetEventAlertSkipped(GameEventsClient.DataType dataType, bool isSkipped) { }
    public void SetPlayerGender(GenderType gender) { }
    public void AddUIEventListener(IUIEventListener listener) { }
    public void SetScriptWorldVolume(float volume) { }
    public float PlayAudio(AudioTypeT audioType, bool ducking = false, float delay = 0.0f, PlayerType owner = PlayerType.NONE) => 0;

    //Funds panel progress
    public void SetFundsPanelProgress(float progress) { }

    // for Goals/Objectives
    public void SetGoalsAlignment(bool isRight) { }
    public void ShowObjectivesTitle(bool show) { }
    public void SetPlayerGoals(List<GoalDisplay> goals) { }

    public void HighlightTiles(List<int> tileIDs, Color highlightColor) { }
    public void SetMapName(string mapName) { }

    public void SetWinScreenText(string winnerText, string mainText, string buttonText) { }

    public void SetGetWinPositionFunc(Func<Vector3> function) { }
    public void SetSkipChartsandGraphsScreen(bool bSkip) { }
    public void SetContinueOnLose(bool bContinue) { }

    public void SetButtonFlashing(BuildingType eBuilding, bool bFlashing) { }
    public void SetButtonFlashing(BlackMarketType eBlackMarket, bool bFlashing) { }
    public void DisplayChatMessage(PlayerType fromPlayer, bool team, string message) { }
    public string GetHotkeyDisplayString(KeyBindingType eKeyBinding) => string.Empty;
    public List<KeyCode> GetPressedKeys() => null;
    public InfoKeyBinding GetKeybindingInfo(KeyBindingType eKeyBinding) => null;
    public bool IsKeyPressed(KeyCode key) => false;
    public void ClearSelection() { }
    public GameObject CreateWorldText(string text, Vector3 position) => null;
    public GameObject CreateRegionText(string text, Vector3 position) => null;
    public bool IsTabOpen(HUDElement element) => false;
    public void SetFundsCustomInfoText(string text, string tooltip, Color color) { }
    public void SetGameInfoPopupText(string title, string description) { }

    public void OverridePauseText(bool overrideText, string bigText = "", string smallText = "") { }

    public void SendBannerNotification(string description) { }

    public void SetMaxHqLevel(int level) { }

    public void ExitGame() { }
    public void RestartGame() { }
    public void ShowChartsAndGraphs() { }

    public void SetHQFoundCharacter(CharacterType character) { }

    public void SetSabotageAutoSelect(SabotageType eSabotage, bool bAutoSelect) { }
}
