using System;
using System.Collections.Generic;
using UnityEngine;
using Offworld.GameCore;
//using Offworld.UICore;

namespace Offworld.AppCore
{
    public enum HUDElement
    {
        PLAYER_LIST,
        BLACK_MARKET,
        FUNDS_PANEL,
        CHAT_LOG,
        PAY_DEBT_BUTTON,
        DEBT_DISPLAY,
        TILE_TOOLTIPS,
        SPEED,
        SPECIAL_BUILDINGS,
        BUILDING_QUEUE,
        FLYOUT_PANEL,
        SUB_FLYOUT_PANEL,
        RESOURCE_PRICES,
        RESOURCE_RATES,
        RESOURCE_DEPOSIT_TOGGLE,
        BUILD_TAB,
        HACKER_TAB,
        RESEARCH_TAB,
        PATENT_TAB,
        OFFWORLD_MARKET_TAB,
        ANIMATING_POINTERS,
        HQ_FOUND_PANEL,
        FOUNDING_INFO_PANEL,
        CAMERA_CONTROLS,
        STOCK_CONTROLS,
        NOTICE_OF_TERMINATION,
        FUNDS_PROGRESS_BAR
    }

    public interface IGameHUDHelpers
    {
        bool IsHUDReady();
        bool IsVisible(HUDElement element);
        void SetVisible(HUDElement element, bool visible);
        void SetVisible(BuildingType building, bool visible);
        void SetVisible(ResourceType resource, bool visible);
        void SetCanMinimizeAuction(bool canMinimize);

        // for resource rows
        void ToggleResourcePanelDisplayMode();
        void ShowResourceNotificationArrow(ResourceType resource, PlayerType player, string title, string description, bool showIcon);
        void ShowResourceNotificationArrow(ResourceType resource, PlayerType player, string title, string description, bool showIcon, BuildingType building);
        void SetResourceWarning(ResourceType resource, int warning);
        void SetResourceWarningInfo(ResourceType resource, int warnLevel, Color color, float alphaMultiplier, float animationSpeed);
        void RemoveResourceWarningInfo(ResourceType resource, int warnLevel);
        void ResetResourceWarningInfo(ResourceType resource);
        void ClearResourceWarningInfo(ResourceType resource);
        void SetResourceWarningToolTip(ResourceType resource, string description);
        void SetResourceRowEnabled(ResourceType resource, bool enabled);

        void ShowPopup(string title, string description, string okButton, Action callback);
        void ShowCharacterPopup(CharacterType eCharacter, string title, string description, List<string> buttons, List<Action> callbacks, bool isLeftAligned);
        //void ShowCharacterPopup(CharacterType eCharacter, string title, string description, List<string> buttons, List<Action> callbacks, GuiUtils.CharacterPopupAlignment alignment);
        void ShowOptionsPopup(string title, string description, List<string> buttons, List<Action> callbacks);
        void SetCameraBoundaries(Vector3 min, Vector3 max);
        void SetCameraMinZoom(float newMin);
        void SetCameraMaxZoom(float newMax);
        Vector3 GetCurrentCameraLookAt();
        void SetSelectedTile(TileClient pTile);
        void CameraLookAt(Vector3 position);
        void CameraLookAtZoomed(Vector3 position, float zoomRange);
        void CameraFocus(Vector3 worldPosition, float blendTime);
        void CameraUnfocus();
        void CameraUnfocus(Vector3 position);
        void CameraPan(Vector3 worldPosition, float travelTime);
        void CameraPanFocused(Vector3 worldPosition, float travelTime);
        void SetDataEventMuted(GameEventsClient.DataType dataType, bool isMuted);
        void SetEventAlertSkipped(GameEventsClient.DataType dataType, bool isSkipped);
        void SetPlayerGender(GenderType gender);
        void AddUIEventListener(IUIEventListener listener);
        void SetScriptWorldVolume(float volume);
        float PlayAudio(AudioTypeT audioType, bool ducking = false, float delay = 0.0f, PlayerType owner = PlayerType.NONE); //returns the duration of the audio clip

        //Funds panel progress
        void SetFundsPanelProgress(float progress);

        // for Goals/Objectives
        void SetGoalsAlignment(bool isRight);
        void ShowObjectivesTitle(bool show);
        void SetPlayerGoals(List<GoalDisplay> goals);

        void HighlightTiles(List<int> tileIDs, Color highlightColor);
        void SetMapName(string mapName);

        void SetWinScreenText(string winnerText, string mainText, string buttonText);

        void SetGetWinPositionFunc(Func<Vector3> function);
        void SetSkipChartsandGraphsScreen(bool bSkip);
        void SetContinueOnLose(bool bContinue);

        void SetButtonFlashing(BuildingType eBuilding, bool bFlashing);
        void SetButtonFlashing(BlackMarketType eBlackMarket, bool bFlashing);
        void DisplayChatMessage(PlayerType fromPlayer, bool team, string message);
        string GetHotkeyDisplayString(KeyBindingType eKeyBinding);
        List<KeyCode> GetPressedKeys();
        InfoKeyBinding GetKeybindingInfo(KeyBindingType eKeyBinding);
        bool IsKeyPressed(KeyCode key);
        void ClearSelection();
        GameObject CreateWorldText(string text, Vector3 position);
        GameObject CreateRegionText(string text, Vector3 position);
        bool IsTabOpen(HUDElement element);
        void SetFundsCustomInfoText(string text, string tooltip, Color color);
        void SetGameInfoPopupText(string title, string description);

        void OverridePauseText(bool overrideText, string bigText = "", string smallText = "");

        void SendBannerNotification(string description);

        void SetMaxHqLevel(int level);

        void ExitGame();
        void RestartGame();
        void ShowChartsAndGraphs();

        void SetHQFoundCharacter(CharacterType character);

        void SetSabotageAutoSelect(SabotageType eSabotage, bool bAutoSelect);
    }
}
