using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace lilAvatarUtils.MainWindow
{
    internal class GUIUtils
    {
        private static readonly string ICON_MENU        = "_Menu";
        private static readonly string ICON_MENU_D      = "d__Menu";
        private static readonly string ICON_REFRESH     = "Refresh";
        private static readonly Color colorLine = EditorGUIUtility.isProSkin ? new Color(0.35f,0.35f,0.35f,1.0f) : new Color(0.4f,0.4f,0.4f,1.0f);
        internal static readonly Color colorActive = new Color(0.1f,0.6f,1.0f,0.333333f);
        internal static readonly string[] MASK_LABELS_BOOL = new[]{"False","True"};

        internal static GUIStyle styleWhite;
        internal static GUIStyle styleWhiteBold;
        internal static GUIStyle styleRed;
        internal static GUIStyle styleRedToggle;
        internal static GUIStyle styleRedNumber;
        internal static GUIStyle styleRedPopup;
        internal static GUIStyle styleRedText;
        internal static GUIStyle styleRedObject;
        internal static Texture iconMenu;
        internal static Texture iconMenu_D;
        internal static Texture iconRefresh;
        internal static Color[] objectFieldColors;
        internal static int objectFieldFontSize;
        internal static FontStyle objectFieldFontStyle;

        internal static void Initialize()
        {
            if(objectFieldColors == null || objectFieldColors.Length == 0)
            {
                objectFieldColors = GetColors(EditorStyles.objectField);
                objectFieldFontSize= EditorStyles.objectField.fontSize;
                objectFieldFontStyle = EditorStyles.objectField.fontStyle;
            }
            if(styleWhite == null)
            {
                styleWhite = new GUIStyle(EditorStyles.label);
                if(!EditorGUIUtility.isProSkin) SetColors(styleWhite, Color.white);
            }
            if(styleWhiteBold == null)
            {
                styleWhiteBold = new GUIStyle(EditorStyles.boldLabel);
                if(!EditorGUIUtility.isProSkin) SetColors(styleWhiteBold, Color.white);
            }
            if(styleRed == null)
            {
                styleRed = new GUIStyle(EditorStyles.boldLabel);
                SetColors(styleRed, Color.red);
            }
            if(styleRedToggle == null)
            {
                styleRedToggle = new GUIStyle(EditorStyles.toggle);
                styleRedToggle.fontStyle = FontStyle.Bold;
                SetColors(styleRedToggle, Color.red);
            }
            if(styleRedNumber == null)
            {
                styleRedNumber = new GUIStyle(EditorStyles.numberField);
                styleRedNumber.fontStyle = FontStyle.Bold;
                SetColors(styleRedNumber, Color.red);
            }
            if(styleRedPopup == null)
            {
                styleRedPopup = new GUIStyle(EditorStyles.popup);
                styleRedPopup.fontStyle = FontStyle.Bold;
                SetColors(styleRedPopup, Color.red);
            }
            if(styleRedText == null)
            {
                styleRedText = new GUIStyle(EditorStyles.textField);
                styleRedText.fontStyle = FontStyle.Bold;
                SetColors(styleRedText, Color.red);
            }
            if(styleRedObject == null)
            {
                styleRedObject = new GUIStyle(EditorStyles.objectField);
                styleRedObject.fontStyle = FontStyle.Bold;
                SetColors(styleRedObject, Color.red);
            }
            if(iconMenu     == null) iconMenu     = EditorGUIUtility.IconContent(ICON_MENU    ).image;
            if(iconMenu_D   == null) iconMenu_D   = EditorGUIUtility.IconContent(ICON_MENU_D  ).image;
            if(iconRefresh  == null) iconRefresh  = EditorGUIUtility.IconContent(ICON_REFRESH ).image;
        }

        private static void SetColors(GUIStyle style, Color color)
        {
            style.active.textColor    = color;
            style.focused.textColor   = color;
            style.hover.textColor     = color;
            style.normal.textColor    = color;
            style.onActive.textColor  = color;
            style.onFocused.textColor = color;
            style.onHover.textColor   = color;
            style.onNormal.textColor  = color;
        }

        private static void SetColors(GUIStyle style, Color[] colors)
        {
            style.active.textColor    = colors[0];
            style.focused.textColor   = colors[1];
            style.hover.textColor     = colors[2];
            style.normal.textColor    = colors[3];
            style.onActive.textColor  = colors[4];
            style.onFocused.textColor = colors[5];
            style.onHover.textColor   = colors[6];
            style.onNormal.textColor  = colors[7];
        }

        private static Color[] GetColors(GUIStyle style)
        {
            return new[]{
                style.active.textColor   ,
                style.focused.textColor  ,
                style.hover.textColor    ,
                style.normal.textColor   ,
                style.onActive.textColor ,
                style.onFocused.textColor,
                style.onHover.textColor  ,
                style.onNormal.textColor ,
            };
        }

        // GUI
        internal static void DrawLine()
        {
            EditorGUI.DrawRect(EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, 1)), colorLine);
        }

        internal static bool Foldout(Rect rect, bool display)
        {
            var rectFoldout = new Rect(rect.x - 4, rect.y, rect.width, rect.height);
            var e = Event.current;
            if(e.type == EventType.Repaint) {
                var toggleRect = new Rect(rectFoldout.x + 4f, rectFoldout.y + 2f, 13f, 13f);
                EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
            }
            if(e.type == EventType.MouseDown && rectFoldout.Contains(e.mousePosition)) {
                display = !display;
                e.Use();
            }
            return display;
        }

        // Fields for non-editable properties
        internal static void LabelField(Rect rect, string text, bool hilight = false)
        {
            if(hilight) EditorGUI.LabelField(rect, text, styleRed);
            else        EditorGUI.LabelField(rect, text);
        }

        internal static void LabelFieldWithSelection(Rect rect, Object obj, bool hilight = false)
        {
            GUIStyle style;
            if(hilight) style = styleRed;
            else        style = EditorStyles.label;
            GUIContent content = EditorGUIUtility.ObjectContent(obj, obj.GetType());
            content.tooltip = AssetDatabase.GetAssetPath(obj);
            if(!string.IsNullOrEmpty(content.tooltip) && !AvatarUtilsWindow.isMaterialsGUITabOpen) content.text = Path.GetFileName(content.tooltip);
            if(AssetDatabase.IsSubAsset(obj)) content.text = obj.name;

            var sizeCopy = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(new Vector2(rect.height-2, rect.height-2));
            if(UnchangeButton(rect, content, style) && obj != null)
            {
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
            }
            EditorGUIUtility.SetIconSize(sizeCopy);
        }

        internal static void LabelFieldWithSelection(Object obj, bool hilight = false)
        {
            Rect rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
            LabelFieldWithSelection(rect, obj, hilight);
        }

        internal static void AutoLabelField(Rect rect, object val, bool hilight)
        {
            if(val is Object o)
            {
                LabelFieldWithSelection(rect, o, hilight);
            }
            else if(val == null)
            {
                // Nothing to do
            }
            else
            {
                LabelField(rect, val.ToString(), hilight);
            }
        }

        // Fields for editable properties
        internal static bool ToggleField(Rect rect, bool val, bool hilight = false)
        {
            var colorCopy = GUI.color;
            if(hilight) GUI.color = Color.red;
            var res = EditorGUI.Toggle(rect, val);
            if(hilight) GUI.color = colorCopy;
            return res;
        }

        internal static int IntField(Rect rect, int val, bool hilight = false)
        {
            GUIStyle style;
            if(hilight) style = styleRedNumber;
            else        style = EditorStyles.numberField;
            return EditorGUI.IntField(rect, val, style);
        }

        internal static float FloatField(Rect rect, float val, bool hilight = false)
        {
            GUIStyle style;
            if(hilight) style = styleRedNumber;
            else        style = EditorStyles.numberField;
            return EditorGUI.FloatField(rect, val, style);
        }

        internal static Enum EnumField(Rect rect, Enum val, bool hilight = false)
        {
            GUIStyle style;
            if(hilight) style = styleRedPopup;
            else        style = EditorStyles.popup;
            return EditorGUI.EnumPopup(rect, val, style);
        }

        internal static int PopupField(Rect rect, int val, string[] labels, bool hilight = false)
        {
            GUIStyle style;
            if(hilight) style = styleRedPopup;
            else        style = EditorStyles.popup;
            return EditorGUI.Popup(rect, val, labels, style);
        }

        internal static string TextField(Rect rect, string val, bool hilight = false)
        {
            GUIStyle style;
            if(hilight) style = styleRedText;
            else        style = EditorStyles.textField;
            return EditorGUI.TextField(rect, val, style);
        }

        internal static Object ObjectField(Rect rect, Object val, Type type, bool allow, bool hilight = false)
        {
            if(hilight) SetColors(EditorStyles.objectField, Color.red);
            var obj2 = EditorGUI.ObjectField(rect, val, type, allow);
            if(hilight) SetColors(EditorStyles.objectField, objectFieldColors);
            return obj2;
        }

        internal static object AutoField(Rect rect, object val, Type type, bool allow, bool hilight)
        {
            if(type != null && type.IsSubclassOf(typeof(Object)) || val is Object)
            {
                return ObjectField(rect, (Object)val, type, allow, hilight);
            }
            if(type == null)
            {
                switch(val)
                {
                    case Enum v   : return EnumField(rect, v, hilight);
                    case bool v   : return ToggleField(rect, v, hilight);
                    case int v    : return IntField(rect, v, hilight);
                    case float v  : return FloatField(rect, v, hilight);
                    case string v : return TextField(rect, v, hilight);
                }
            }
            return val;
        }

        // Button without changes
        // EditorGUI.EndChangeCheck() returns false
        internal static bool UnchangeButton(Rect rect, GUIContent content, GUIStyle style)
        {
            EditorGUI.LabelField(rect, content, style);
            Event e = Event.current;
            return e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition);
        }

        internal static bool UnchangeButton(Rect rect, GUIContent content)
        {
            return UnchangeButton(rect, content, EditorStyles.miniButton);
        }

        internal static bool UnchangeButton(GUIContent content)
        {
            return UnchangeButton(EditorGUILayout.GetControlRect(), content);
        }

        internal static bool UnchangeButton(string label)
        {
            return UnchangeButton(new GUIContent(label));
        }

        internal static string QualityToString(TextureImporterCompression tic)
        {
            switch(tic)
            {
                case TextureImporterCompression.CompressedLQ: return "Low Quality";
                case TextureImporterCompression.Compressed  : return "Normal Quality";
                case TextureImporterCompression.CompressedHQ: return "High Quality";
                default                                     : return "None";
            }
        }
    }
}
