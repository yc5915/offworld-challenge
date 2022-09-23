//using UnityEngine;
//using UnityEngine.UI;
//using System;
//using System.Linq;
//using System.Text;
//using System.Collections.Generic;
//using Offworld.GameCore;
//using Offworld.GameCore.Text;
//using Offworld.SystemCore;

//namespace Offworld.UICore
//{
//    public static class GuiUtils
//    {
//        public const int POPUP_LAYER = 1000;
//        public const int DROPDOWN_LAYER = 2000;
//        public const int TOOLTIP_LAYER = 3000;

//        public enum RectTransformCorners
//        {
//            BOTTOM_LEFT,
//            TOP_LEFT,
//            TOP_RIGHT,
//            BOTTOM_RIGHT,
//            NUM_CORNERS
//        }

//        public enum IconSize
//        {
//            SMALL,
//            MEDIUM,
//            LARGE
//        };

//        public enum SpanClassType
//        {
//            POSITIVE,
//            NEGATIVE
//        };

//        public enum CharacterPopupAlignment
//        {
//            LEFT,
//            RIGHT,
//            CENTER
//        }

//        // slewis - I hate including this in nearly every file
//        private static string TEXT(string key) { return TextManager.TEXT(key); }
//        private static string TEXT(TextType type) { return TextManager.TEXT(type); }
//        private static string TEXT(string key, params TextVariable[] arguments) { return TextManager.TEXT(key, arguments); }
//        private static string TEXT(TextType type, params TextVariable[] arguments) { return TextManager.TEXT(type, arguments); }
//        private static PluralType GetPluralType(int value) { return TextManager.CurrentLanguageInfo.GetPluralType(value); }
//        // end slewis hate

//        private static GetInlineIconDelegate getInlineDelegate;
//        public delegate string GetInlineIconDelegate(string lookupString, float scaleFactor);
//        public static void SetGetInlineIconDelegate(GetInlineIconDelegate del)
//        {
//            getInlineDelegate = del;
//        }

//        public static string unityColorize(string str, Color c)
//        {
//            return "<color=#" + ColorUtility.ToHtmlStringRGBA(c) + ">" + str + "</color>";
//        }

//        public static string bulletText(string text)
//        {
//            return TEXT("TEXT_HUD_BULLETED_ITEM", unityIcon("BULLET").ToText(), text.ToText());
//        }

//        public static string bulletText(string text, Color bulletColor)
//        {
//            return TEXT("TEXT_HUD_BULLETED_ITEM", unityColorize(unityIcon("BULLET"), bulletColor).ToText(), text.ToText());
//        }

//        public static string convertKeyCodeToText(KeyCode key)
//        {
//            if (key == KeyCode.LeftShift || key == KeyCode.RightShift)
//                return "Shift";
//            else if (key == KeyCode.LeftControl || key == KeyCode.RightControl)
//                return "Control";
//            else if (key == KeyCode.LeftAlt || key == KeyCode.RightAlt)
//                return "Alt";
//            else if (key == KeyCode.LeftCommand || key == KeyCode.RightCommand)
//                return "Command";
//            else
//                return key.ToString();
//        }

//        public static string getConstructionPercentage(TileClient tile)
//        {
//            int iResult = 0;

//            ConstructionClient construction = tile.constructionClient();

//            if (construction != null)
//            {
//                if (!construction.isDamaged())
//                {
//                    InfoBuilding buildingInfo = Globals.Infos.building(construction.getType());
//                    if (buildingInfo.miConstructionThreshold != 0)
//                    {
//                        iResult = ((100 * construction.getConstruction()) / (buildingInfo.miConstructionThreshold * 10));
//                    }
//                }
//            }

//            return iResult + "%";
//        }

//        public static string buildResourceLevelString(ResourceType eResource, ResourceLevelType eLevel)
//        {
//            return Globals.Infos.resource(eResource).mzInlineLevelIcon + Globals.Infos.resourceLevel(eLevel).mzIconSuffix;
//        }

//        public static string getHTMLIcon(string innerText, IconSize eSize)
//        {
//            string result = "<image src='" + innerText + "' class='icon " + innerText + " ";
//            switch (eSize)
//            {
//                case IconSize.SMALL:
//                    result += "smallIcon";
//                    break;
//                case IconSize.MEDIUM:
//                    result += "mediumIcon";
//                    break;
//                case IconSize.LARGE:
//                    result += "largeIcon";
//                    break;
//            }
//            result += "'/>";
//            return result;
//        }

//        public static string getHTMLHeightIcon(IconSize eSize)
//        {
//            return getHTMLIcon("images/icons/inline/sun.svg", eSize);
//        }

//        public static string getHTMLWindIcon(IconSize eSize)
//        {
//            return getHTMLIcon("images/icons/inline/wind.svg", eSize);
//        }

//        public static string getHTMLGeothermalIcon(IconSize eSize)
//        {
//            return getHTMLIcon("images/icons/inline/geothermal.svg", eSize);
//        }

//        public static string getHTMLIcon(ResourceType eResource, IconSize eSize)
//        {
//            return getHTMLIcon(Globals.Infos.resource(eResource).mzInlineIcon, eSize);
//        }

//        public static string getHTMLIcon(ResourceType eResource, ResourceLevelType eLevel, IconSize eSize)
//        {
//            return getHTMLIcon(buildResourceLevelString(eResource, eLevel), eSize);
//        }

//        private static string getColor(float color)
//        {
//            return ((int)(color * 255.0f)).ToString();
//        }

//        public static string getColor(Color color)
//        {
//            return getColor(color.r) + "," + getColor(color.g) + "," + getColor(color.b);
//        }

//        public static string resourceIconAndText(ResourceType eResource, IconSize eSize)
//        {
//            return getHTMLIcon(eResource, eSize) + TEXT(Globals.Infos.resource(eResource).meName);
//        }

//        public static string unityIcon(ResourceType resource)
//        {
//            return unityIcon(resource, -1);
//        }

//        public static string unityIcon(ResourceType resource, float scaleFactor)
//        {
//            string resourceName = Globals.Infos.resource(resource).mzNewUIInlineIcon;
//            return GetIconString(resourceName, scaleFactor);
//        }

//        public static string unityIcon(ResourceType resource, ResourceLevelType resourceLevel)
//        {
//            return unityIcon(resource, resourceLevel, -1);
//        }

//        public static string unityIcon(ResourceType resource, ResourceLevelType resourceLevel, float scaleFactor)
//        {
//            string spriteLookup = Globals.Infos.resource(resource).mzNewUIInlineIcon + "_" + Globals.Infos.resourceLevel(resourceLevel).mzAssetSuffix;
//            return GetIconString(spriteLookup, scaleFactor);
//        }

//        //cache values for performance
//        public static string GetIconString(string lookupString, float scaleFactor)
//        {
//            if (getInlineDelegate != null)
//                return getInlineDelegate(lookupString, scaleFactor);

//            return "";
//        }

//        public static TextVariable resourceUnityIconAndText(ResourceType resource)
//        {
//            TextType textType = Globals.Infos.getType<TextType>("TEXT_RESOURCE_ICON_AND_NAME");
//            return textType.ToText(unityIcon(resource).ToText(), Globals.Infos.resource(resource).meName.ToText());
//        }

//        public static string resourceUnityIconAndTextString(ResourceType resource)
//        {
//            return resourceUnityIconAndText(resource).Evaluate(TextManager.CurrentLanguage, 0);
//        }

//        public static TextVariable resourceUnityIconAndText(ResourceType resource, ResourceLevelType resourceLevel)
//        {
//            TextType textType = Globals.Infos.getType<TextType>("TEXT_RESOURCE_LEVEL_AND_NAME");
//            return textType.ToText(unityIcon(resource, resourceLevel).ToText(), Globals.Infos.resourceLevel(resourceLevel).meName.ToText(), Globals.Infos.resource(resource).meName.ToText());
//        }

//        public static string resourceUnityIconAndTextString(ResourceType resource, ResourceLevelType resourceLevel)
//        {
//            return resourceUnityIconAndText(resource, resourceLevel).Evaluate(TextManager.CurrentLanguage, 0);
//        }

//        public static string unityIcon(string iconName)
//        {
//            return unityIcon(iconName, -1);
//        }

//        public static string unityIcon(string iconName, float scaleFactor)
//        {
//            return GetIconString(iconName, scaleFactor);
//        }

//        public static string unityIcon(BuildingType building)
//        {
//            return unityIcon(building, -1);
//        }

//        public static string unityIcon(BuildingType building, float scaleFactor)
//        {
//            string buildingType = Globals.Infos.building(building).mzType;
//            return GetIconString(buildingType, scaleFactor);
//        }

//        public static string unityIcon(HQType hq)
//        {
//            return unityIcon(hq, -1);
//        }

//        public static string unityIcon(HQType hq, float scaleFactor)
//        {
//            string hqType = Globals.Infos.HQ(hq).mzType;
//            return GetIconString(hqType, scaleFactor);
//        }

//        public static string unityIcon(SabotageType sabotage)
//        {
//            return unityIcon(sabotage, -1);
//        }

//        public static string unityIcon(SabotageType sabotage, float scaleFactor)
//        {
//            for (BlackMarketType eBlackMarket = 0; eBlackMarket < Globals.Infos.blackMarketsNum(); eBlackMarket++)
//            {
//                if (Globals.Infos.blackMarket(eBlackMarket).meSabotage == sabotage)
//                    return unityIcon(eBlackMarket, scaleFactor);
//            }

//            return "";
//        }

//        public static string unityIcon(BlackMarketType blackMarket)
//        {
//            return unityIcon(blackMarket, -1);
//        }

//        public static string unityIcon(BlackMarketType blackMarket, float scaleFactor)
//        {
//            string blackMarketType = Globals.Infos.blackMarket(blackMarket).mzType;
//            return GetIconString(blackMarketType, scaleFactor);
//        }

//        public static string unityIcon(PatentType patentType)
//        {
//            return unityIcon(patentType, -1);
//        }

//        public static string unityIcon(PatentType patent, float scaleFactor)
//        {
//            string patentType = Globals.Infos.patent(patent).mzType;
//            return GetIconString(patentType, scaleFactor);
//        }

//        public static string buildSpan(string str, string classStr)
//        {
//            return "<span class='" + classStr + "'>" + str + "</span>";
//        }

//        public static string buildSpan(string str, SpanClassType eClassType)
//        {
//            string strClass = "";

//            switch (eClassType)
//            {
//                case SpanClassType.NEGATIVE:
//                    strClass = "negative";
//                    break;
//                case SpanClassType.POSITIVE:
//                    strClass = "positive";
//                    break;
//            }

//            return buildSpan(str, strClass);
//        }

//        public static string BOLD(string str)
//        {
//            return "<b>" + str + "</b>";
//        }

//        public static string ITALICS(string str)
//        {
//            return "<i>" + str + "</i>";
//        }

//        public static string HIGHLIGHT(string str)
//        {
//            return "<color=#ffffffff>" + str + "</color>";
//        }

//        public static string WARNING(string str)
//        {
//            return BuildColorText(str, Color.red);
//        }

//        public static string BuildColorText(string str, Color color)
//        {
//            return "<color=#" + HTMLColorConverter.RGBToHex(color) + ">" + str + "</color>";
//        }

//        public static TextVariable BuildHighlightText(TextType eType)
//        {
//            return GuiUtils.BuildColorText(TEXT(eType), Color.cyan).ToText();
//        }

//        public static string clearHTMLNonsense(string text)
//        {
//            string result = text.Replace("<", "&lt;");
//            result = result.Replace(">", "&gt;");
//            return result;
//        }

//        public static bool isWithinCanvas(Canvas canvas, RectTransform rect)
//        {
//            Vector3[] fourCorners = new Vector3[4];
//            rect.GetWorldCorners(fourCorners);

//            return isWithinCanvas(canvas, fourCorners);
//        }

//        public static bool isWithinCanvas(Canvas canvas, Vector3[] fourCorners)
//        {
//            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
//            Vector3[] canvasFourCorners = new Vector3[4];
//            canvasRect.GetWorldCorners(canvasFourCorners);

//            foreach (Vector3 corner in fourCorners)
//            {
//                if (!MathUtilities.InRange(corner.x, canvasFourCorners[(int)RectTransformCorners.TOP_LEFT].x, canvasFourCorners[(int)RectTransformCorners.TOP_RIGHT].x) ||
//                    !MathUtilities.InRange(corner.y, canvasFourCorners[(int)RectTransformCorners.BOTTOM_LEFT].y, canvasFourCorners[(int)RectTransformCorners.TOP_LEFT].y))
//                {
//                    return false;
//                }
//            }

//            return true;

//        }

//        // Make sure UI elements aren't positioned in-between two pixels
//        public static void setPixelPosition(RectTransform rect, Vector3 pos)
//        {
//            rect.position = pos;
//            rect.anchoredPosition3D = rect.anchoredPosition3D.round();
//        }

//        public static string FormatTimeHoursMinutes(int hours, int minutes)
//        {
//            string hoursFormatted = hours.ToString().PadLeft(2, '0');
//            string minsFormatted = minutes.ToString().PadLeft(2, '0');
//            return TEXT("TEXT_HUDINFO_TIME_HOURS_MINUTES", hoursFormatted.ToText(), minsFormatted.ToText());
//        }

//        public static Canvas getParentCanvas(GameObject gameObject)
//        {
//            while (gameObject != null)
//            {
//                Canvas foundCanvas = gameObject.GetComponent<Canvas>();
//                if (foundCanvas != null)
//                    return foundCanvas;

//                gameObject = gameObject.transform.parent.gameObject;
//            }

//            return null;
//        }

//        public static Camera getCameraOnControl(GameObject control)
//        {
//            Canvas controlCanvas = getParentCanvas(control);
//            if (controlCanvas != null)
//                return controlCanvas.worldCamera;

//            return null;
//        }

//        public static void SetVisible(GameObject uiThing, bool visible)
//        {
//            CanvasGroup canvasGroup = uiThing.GetComponent<CanvasGroup>();
//            if (canvasGroup != null)
//            {
//                // slewis - this is to address when someone accidentally disabled something in a scene when editing
//                // and we need to reactivate it for visibility purposes
//                if (visible && !uiThing.activeSelf)
//                    uiThing.SetActive(true);

//                SetVisible(canvasGroup, visible);
//            }
//            else
//            {
//                uiThing.SetActive(visible);
//            }
//        }

//        public static void SetVisible(CanvasGroup canvasGroup, bool visible)
//        {
//            Animator animator = canvasGroup.gameObject.GetComponent<Animator>();
//            if (animator != null)
//                animator.enabled = visible;

//            canvasGroup.alpha = visible ? 1.0f : 0.0f;
//            canvasGroup.interactable = visible;
//            canvasGroup.blocksRaycasts = visible;
//        }

//        public static bool IsVisible(CanvasGroup canvasGroup)
//        {
//            return canvasGroup.alpha != 0;
//        }

//        public static void SetParent(GameObject original, GameObject parent)
//        {
//            original.transform.SetParent(parent.transform);
//            original.transform.localScale = Vector3.one;
//            original.transform.localPosition = Vector3.zero;
//        }
//    }


//    // Jim found this and I thought it was a great idea.
//    // https://unity3d.com/learn/tutorials/modules/intermediate/scripting/extension-methods
//    public static class ExtensionMethods
//    {
//        // Vector3
//        public static Vector3 round(this Vector3 vector)
//        {
//            return new Vector3(Mathf.Round(vector.x), Mathf.Round(vector.y), Mathf.Round(vector.z));
//        }

//        // RectTransform
//        //this assumes the transform is parallel with the XY plane
//        public static Vector2 GetWorldDimensions(this RectTransform trans)
//        {
//            Rect localRect = trans.rect;
//            Vector3 worldDimensions = trans.TransformVector(localRect.width, localRect.height, 0);
//            Vector2 result = new Vector2(Mathf.Abs(worldDimensions.x), Mathf.Abs(worldDimensions.y));
//            return result;
//        }

//        public static Rect GetWorldRect(this RectTransform trans)
//        {
//            using (new UnityProfileScope("ExtensionMethods::GetWorldRect"))
//            {
//                Vector3[] worldCorners = new Vector3[4];
//                trans.GetWorldCorners(worldCorners);
//                float minX = worldCorners.Select(corner => corner.x).Min();
//                float minY = worldCorners.Select(corner => corner.y).Min();
//                float maxX = worldCorners.Select(corner => corner.x).Max();
//                float maxY = worldCorners.Select(corner => corner.y).Max();
//                return new Rect(minX, minY, maxX - minX, maxY - minY);
//            }
//        }

//        public static Vector3 NormalizedToWorld(this RectTransform trans, Vector2 normalizedPoint)
//        {
//            Rect localRect = trans.rect;
//            float localX = MathUtilities.LerpUnclamped(localRect.xMin, localRect.xMax, normalizedPoint.x);
//            float localY = MathUtilities.LerpUnclamped(localRect.yMin, localRect.yMax, normalizedPoint.y);
//            Vector3 worldPoint = trans.TransformPoint(localX, localY, 0);
//            return worldPoint;
//        }

//        public static float GetWorldWidth(this RectTransform trans)
//        {
//            return trans.GetWorldDimensions().x;
//        }

//        public static float GetWorldHeight(this RectTransform trans)
//        {
//            return trans.GetWorldDimensions().y;
//        }

//        public static bool overlaps(this RectTransform rect1, RectTransform rect2)
//        {
//            Vector3[] rect1WorldCorners = new Vector3[4];
//            rect1.GetWorldCorners(rect1WorldCorners);

//            Vector3[] rect2WorldCorners = new Vector3[4];
//            rect2.GetWorldCorners(rect2WorldCorners);

//            float minX = rect2WorldCorners[1].x;
//            float maxX = rect2WorldCorners[3].x;
//            float minY = rect2WorldCorners[3].y;
//            float maxY = rect2WorldCorners[1].y;

//            foreach (Vector3 corner in rect1WorldCorners)
//            {
//                bool inRangeX = MathUtilities.InRange(corner.x, minX, maxX);
//                bool inRangeY = MathUtilities.InRange(corner.y, minY, maxY);

//                if (!inRangeX || !inRangeY)
//                    return false;
//            }

//            return true;
//        }

//        public static bool viewportContains(this RectTransform trans, Camera camera, Vector2 point)
//        {
//            if (camera == null)
//                return false;

//            Vector3[] worldCorners = new Vector3[4];
//            trans.GetWorldCorners(worldCorners);
//            worldCorners = worldCorners.Select(corner => camera.WorldToScreenPoint(corner)).ToArray();

//            return (MathUtilities.InRange(point.x, worldCorners[1].x, worldCorners[3].x) &&
//                    MathUtilities.InRange(point.y, worldCorners[3].y, worldCorners[1].y));
//        }

//        // StringBuilder
//        // This will append a new line only if the string builder is not empty. This removes the addtional carriage returns at the end
//        public static StringBuilder AppendLineMohawk(this StringBuilder sb, string addedString)
//        {
//            if (sb.Length != 0)
//                sb.AppendLine();

//            sb.Append(addedString);
//            return sb;
//        }
//    }

//    public class UIPool<T> where T : Component
//    {
//        private List<T> pooled = new List<T>();
//        private T prefab;

//        public UIPool(T prefab, int startSize = 1)
//        {
//            this.prefab = prefab;

//            for (int i = 0; i < startSize; i++)
//                Spawn();
//        }

//        private T Spawn()
//        {
//            T spawned = GameObject.Instantiate(prefab).gameObject.GetComponent<T>();
//            pooled.Add(spawned);
//            spawned.gameObject.SetActive(false);

//            return spawned;
//        }

//        public T Acquire(GameObject parent)
//        {
//            T instance = pooled.Find(x => !x.gameObject.activeSelf);

//            if (instance == null)
//                instance = Spawn();

//            GuiUtils.SetParent(instance.gameObject, parent);
//            instance.gameObject.SetActive(true);

//            return instance;
//        }

//        public void Reset(T obj)
//        {
//            if (pooled.Contains(obj))
//            {
//                obj.gameObject.SetActive(false);
//            }
//        }
//    }
//}
