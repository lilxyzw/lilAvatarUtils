using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace jp.lilxyzw.avatarutils
{
    internal abstract class AbstractTabelGUI
    {
        private const int GUI_INDENT_WIDTH = 16;
        private const int GUI_SPACE_WIDTH = 5;
        private const int GUI_FILTER_WIDTH = 50;
        private const int GUI_LABEL_MIN_WIDTH = 10;
        protected static readonly string[] L_ReferencedFrom = {"Referenced from",""};

        public bool[] labelMasks = {};
        public float[] rectWidths = {};
        public bool applyFilter = false;

        protected TableProperties[] libs;
        internal GameObject gameObject;
        protected bool isModified = false;
        protected bool isDescending = true;
        protected int sortIndex = -1;
        protected Vector2 scrollPosition = new(0,0);
        protected Rect rectBase;
        protected delegate bool LineGUIOverride(int count, bool[] emphasizes);
        protected LineGUIOverride lineGUIOverride = null;

        private int rectModifyIndex = -1;
        private Event m_event;
        internal AvatarUtils m_window;
        private float tableWidth = 0;
        private bool isScrolling = false;
        private int selectedLine = -1;

        internal virtual void Draw()
        {
            if(IsEmptyLibs()) return;
            m_event = Event.current;

            if(m_event.type == EventType.MouseDown) isScrolling = true;
            if(m_event.type == EventType.MouseUp)   isScrolling = false;
            if(!isScrolling) tableWidth = scrollPosition.x + m_window.position.width + 100;

            if(rectWidths == null || rectWidths.Length != libs.Length)
            {
                rectWidths = libs.Select(lib => lib.rect.width).ToArray();
            }

            if(labelMasks == null || labelMasks.Length != libs.Length)
            {
                labelMasks = Enumerable.Repeat(true,libs.Length).ToArray();
            }

            labelMasks[0] = true;

            GUI.enabled = isModified;
            var rectButtons = EditorGUILayout.GetControlRect();
            var rectButton1 = new Rect(rectButtons.x, rectButtons.y, 100, rectButtons.height);
            var rectButton2 = new Rect(rectButton1.xMax + GUI_SPACE_WIDTH, rectButtons.y, 100, rectButtons.height);
            if(L10n.Button(rectButton1, "Apply"))
            {
                ApplyModification();
                Set();
                m_window.Analyze();
            }
            if(L10n.Button(rectButton2, "Revert"))
            {
                Set();
            }
            GUI.enabled = true;

            var rectButton3 = new Rect(rectButton2.xMax + GUI_SPACE_WIDTH, rectButtons.y, 100, rectButtons.height);
            rectButton3.xMax = rectButtons.xMax;
            ButtonEx(rectButton3);

            var rect = EditorGUILayout.GetControlRect();
            libs[0].rect = new Rect(
                GUI_INDENT_WIDTH + rect.x,
                rect.y,
                rectWidths[0],
                rect.height
            );
            for(int i = 1; i < libs.Length; i++)
            {
                libs[i].rect = new Rect(
                    GUI_SPACE_WIDTH + libs[i-1].rect.xMax,
                    rect.y,
                    rectWidths[i],
                    rect.height
                );
                if(!labelMasks[i])
                {
                    libs[i].rect.x = libs[i-1].rect.xMax;
                    libs[i].rect.width = 0;
                }
            }
            SetBaseRect();
            var rectShift = GetShiftedRects();

            // Toggle labels
            if(m_event.type == EventType.MouseDown && m_event.button == 1 && rectBase.Contains(m_event.mousePosition))
            {
                var labelMenu = new GenericMenu();
                for(int j = 1; j < libs.Length; j++)
                {
                    var k = j;
                    labelMenu.AddItem(new GUIContent(libs[k].label[0].Replace("/"," \u2044 ")), labelMasks[k], () => labelMasks[k] = !labelMasks[k]);
                }
                labelMenu.ShowAsContext();
            }

            // Resize
            for(int i = 0; i < libs.Length; i++)
            {
                if(!labelMasks[i]) continue;
                var rectHit  = new Rect(rectShift[i].xMax+2-5, rectShift[i].y, 11, rectShift[i].height);
                var rectLine = new Rect(rectShift[i].xMax+2  , rectShift[i].y,  1, rectShift[i].height);
                EditorGUIUtility.AddCursorRect(rectHit, MouseCursor.ResizeHorizontal);
                EditorGUI.DrawRect(rectLine, new Color(0.5f, 0.5f, 0.5f, 0.5f));
                if(rectModifyIndex == i)
                {
                    rectWidths[i] = Mathf.Max(GUI_LABEL_MIN_WIDTH, m_event.mousePosition.x - rectShift[i].x - 5);
                    m_window.Repaint();
                }
                if(m_event.type == EventType.MouseDown && m_event.button == 0 && rectHit.Contains(m_event.mousePosition))
                {
                    rectModifyIndex = i;
                }
                if(m_event.type == EventType.MouseUp)
                {
                    rectModifyIndex = -1;
                }
            }

            // Labels
            for(int i = 0; i < libs.Length; i++)
            {
                if(!labelMasks[i]) continue;
                bool isSorted = sortIndex == i;
                if(isSorted) EditorGUI.DrawRect(rectShift[i], new Color(0.5f, 0.5f, 0.5f, 0.2f));
                if(L10n.Button(rectShift[i], libs[i].label, EditorStyles.label) && m_event.button == 0)
                {
                    if(isSorted) isDescending = !isDescending;
                    sortIndex = i;
                    Sort();
                    SortLibs();
                }
            }

            // Emphasize
            UpdateRects();
            rectShift = GetShiftedRects();
            var rectFirst = rectShift[0];
            var rectEmpFilter = new Rect(
                rectFirst.x - GUI_INDENT_WIDTH,
                rectFirst.y,
                GUI_INDENT_WIDTH,
                rectFirst.height
            );
            applyFilter = EditorGUI.Toggle(rectEmpFilter, applyFilter);
            if(rectFirst.width >= 150)
            {
                var rectEmpPath      = new Rect(rectFirst.x,      rectFirst.y, GUI_FILTER_WIDTH,                 rectFirst.height);
                var rectEmpPathField = new Rect(rectEmpPath.xMax, rectFirst.y, rectFirst.width-GUI_FILTER_WIDTH, rectFirst.height);
                L10n.LabelField(rectEmpPath, "Filters");
                EmphasizeField(0, rectEmpPathField);
            }
            else
            {
                EmphasizeField(0, rectShift[0]);
            }
            for(int i = 1; i < libs.Length; i++)
            {
                if(!labelMasks[i]) continue;
                EmphasizeField(i, rectShift[i]);
            }
            GUIUtils.DrawLine();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            float UIYBuffer = libs[0].rect.y;
            for(int count = 0; count < libs[0].items.Count; count++)
            {
                LineGUI(count);
            }
            if(UIYBuffer == libs[0].rect.y) L10n.LabelField("Nothing found. Please turn off the filter or change the conditions.");
            EditorGUILayout.EndScrollView();
        }

        protected virtual void LineGUI(int count)
        {
            // Check emphasizes
            var emphasizes = new bool[libs.Length];
            for(int i = 0; i < libs.Length; i++)
            {
                if(libs[i].emphasizeCondition != null)
                {
                    emphasizes[i] = libs[i].emphasizeCondition.Invoke(i,count);
                    continue;
                }
                var obj = libs[i].items[count];
                if(libs[i].isMask)
                {
                    if(obj == null)
                    {
                        emphasizes[i] = false;
                    }
                    else if(obj.GetType().IsEnum)
                    {
                        emphasizes[i] = MathHelper.BitMask((int)libs[i].emphasize, (int)obj);
                    }
                    else switch(obj)
                    {
                        case int val    : emphasizes[i] = MathHelper.BitMask((int)libs[i].emphasize, val); break;
                        case bool val   : emphasizes[i] = MathHelper.BitMask((int)libs[i].emphasize, val); break;
                        default         : emphasizes[i] = false; break;
                    }
                }
                else if(obj is Object o && !o)
                {
                    emphasizes[i] = FilterString("None", (string)libs[i].emphasize);
                }
                else if(obj == null)
                {
                    emphasizes[i] = false;
                }
                else if(obj.GetType().IsEnum)
                {
                    emphasizes[i] = FilterString(obj.ToString(), (string)libs[i].emphasize);
                }
                else switch(obj)
                {
                    case int val    : emphasizes[i] = val >= (int)libs[i].emphasize; break;
                    case string val : emphasizes[i] = FilterString(val, (string)libs[i].emphasize); break;
                    case Object val : emphasizes[i] = FilterString(val.name, (string)libs[i].emphasize); break;
                    default         : emphasizes[i] = false; break;
                }
            }

            if(applyFilter && !emphasizes.Any(emp => emp)) return;

            // GUI
            EditorGUI.BeginChangeCheck();
            var rectLine = EditorGUILayout.BeginVertical();
            var rectBox = new Rect(rectLine.x, rectLine.y, rectLine.width, rectLine.height+2);
            var e = Event.current;
            if(e.type == EventType.MouseDown && rectBox.Contains(e.mousePosition))
            {
                selectedLine = count;
                m_window.Repaint();
            }
            if(selectedLine == count)
            {
                EditorGUI.DrawRect(rectBox, GUIUtils.colorActive);
            }
            else
            {
                if(count % 2 != 0) EditorGUI.DrawRect(rectBox, new Color(0.5f,0.5f,0.5f,0.0f));
                else               EditorGUI.DrawRect(rectBox, new Color(0.5f,0.5f,0.5f,0.1f));
            }
            UpdateRects();
            if(lineGUIOverride != null && !lineGUIOverride.Invoke(count, emphasizes)) return;
            for(int i = 0; i < libs.Length; i++)
            {
                if(!labelMasks[i]) continue;
                if(libs[i].mainGUI != null && !libs[i].mainGUI.Invoke(i, count, emphasizes[i])) continue;
                if(libs[i].isEditable)
                {
                    libs[i].items[count] = GUIUtils.AutoField(libs[i].rect, libs[i].items[count], libs[i].type, libs[i].allowSceneObjects, emphasizes[i]);
                }
                else
                {
                    GUIUtils.AutoLabelField(libs[i].rect, libs[i].items[count], emphasizes[i]);
                }
            }
            GUI.enabled = true;
            LineGUIEx(count);
            EditorGUILayout.EndVertical();
            if(EditorGUI.EndChangeCheck()) isModified = true;
        }

        protected virtual void ButtonEx(Rect position)
        {
        }

        protected virtual void LineGUIEx(int count)
        {
        }

        internal virtual void Set()
        {
        }

        protected virtual void Sort()
        {
        }

        protected virtual void SortLibs()
        {
        }

        protected virtual void ApplyModification()
        {
        }

        protected void SortLibs(object[] keys)
        {
            var libsNew = new TableProperties[libs.Length];
            for(int i = 0; i < libs.Length; i++)
            {
                libsNew[i] = TableProperties.CopyWithoutItems(libs[i]);
            }
            int count = 0;
            foreach(var key in keys)
            {
                int index = libs[0].items.IndexOf(key);
                if(index == -1) continue;
                for(int i = 0; i < libs.Length; i++)
                {
                    libsNew[i].items.Add(libs[i].items[index]);
                }
                count++;
            }
            libs = libsNew;
        }

        protected bool IsEmptyLibs()
        {
            if(libs == null || libs.Count() == 0) return true;
            foreach(var lib in libs)
            {
                if(lib.items == null || lib.items.Count == 0) return true;
            }
            return false;
        }

        protected void UpdateRects()
        {
            float width = Mathf.Max(tableWidth, libs[libs.Length-1].rect.xMax - libs[0].rect.x + 16);
            rectBase = EditorGUILayout.GetControlRect(GUILayout.Width(width), GUILayout.Height(16));
            for(int i = 0; i < libs.Length; i++)
            {
                libs[i].rect.y = rectBase.y;
            }
        }

        protected void SetBaseRect()
        {
            rectBase.x = libs[0].rect.x + GUI_INDENT_WIDTH;
            rectBase.y = libs[0].rect.y;
            rectBase.width = Mathf.Max(tableWidth, libs[libs.Length-1].rect.xMax - libs[0].rect.x + 16);
            rectBase.height = 16;
        }

        protected Rect[] GetShiftedRects()
        {
            return libs.Select(lib => new Rect(lib.rect.x - scrollPosition.x, lib.rect.y, lib.rect.width, lib.rect.height)).ToArray();
        }

        protected bool FilterString(string val, string emp)
        {
            if(string.IsNullOrEmpty(emp)) return false;
            else if(emp.StartsWith("!"))  return !val.Contains(emp.Substring(1));
            else                          return val.Contains(emp);
        }

        private void EmphasizeField(int i, Rect rect)
        {
            if(libs[i].emphasizeGUI != null)
            {
                libs[i].emphasize = libs[i].emphasizeGUI.Invoke(i, rect);
            }
            else if(libs[i].isMask)
            {
                if(libs[i].emphasizeLabels != null) libs[i].emphasize = EditorGUI.MaskField(rect, (int)libs[i].emphasize, libs[i].emphasizeLabels);
                else                                libs[i].emphasize = EditorGUI.MaskField(rect, (int)libs[i].emphasize, GUIUtils.MASK_LABELS_BOOL);
            }
            else switch(libs[i].emphasize)
            {
                case int val    : libs[i].emphasize = EditorGUI.IntField(rect, val); break;
                case string val : libs[i].emphasize = EditorGUI.TextField(rect, val); break;
            }
        }

        internal void ReferencesGUI(Object obj)
        {
            if(!m_window.refs.TryGetValue(obj, out var parents) || parents.Count == 0) return;
            foreach(var parent in parents)
            {
                GUIByType(parent);
                EditorGUI.indentLevel++;
                ReferencesGUIInternal(new HashSet<Object>(), parent);
                EditorGUI.indentLevel--;
            }
        }

        private void ReferencesGUIInternal(HashSet<Object> showed, Object obj)
        {
            if(!m_window.refs.TryGetValue(obj, out var parents) || parents.Count == 0) return;
            foreach(var parent in parents)
            {
                if(!showed.Add(parent)) continue;
                GUIByType(parent);
                EditorGUI.indentLevel++;
                ReferencesGUIInternal(showed, parent);
                EditorGUI.indentLevel--;
            }
        }

        private void GUIByType(Object obj)
        {
            if(obj is AnimatorState || obj is AnimatorStateMachine)
            {
                var machine = GetParent<AnimatorStateMachine>(new HashSet<Object>(), obj);
                var controller = GetParent<AnimatorController>(new HashSet<Object>(), obj);
                if(machine && controller) LabelFieldWithSelection(controller, machine, obj);
                else GUIUtils.LabelFieldWithSelection(obj);
            }
            else
            {
                GUIUtils.LabelFieldWithSelection(obj);
            }
        }

        private T GetParent<T>(HashSet<Object> visited, Object obj) where T : Object
        {
            if(!obj || !visited.Add(obj) || !m_window.refs.TryGetValue(obj, out var parents) || parents.Count == 0) return obj as T;
            var first = parents.Select(p => GetParent<T>(visited, p)).FirstOrDefault(p => p);
            return first ? first : obj as T;
        }

        private static void LabelFieldWithSelection(RuntimeAnimatorController controller, AnimatorStateMachine machine, Object target, bool hilight = false)
        {
            Rect rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
            GUIStyle style;
            if(hilight) style = GUIUtils.styleRed;
            else        style = EditorStyles.label;
            GUIContent content = EditorGUIUtility.ObjectContent(target, target.GetType());
            content.text = $"{target.name} ({target.GetType().Name})";
            content.tooltip = AssetDatabase.GetAssetPath(target);

            var sizeCopy = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(new Vector2(rect.height-2, rect.height-2));
            if(GUIUtils.UnchangeButton(rect, content, style) && target)
            {
                Selection.activeObject = controller;
                if(controller is AnimatorController ac)
                {
                    var index = 0;
                    foreach(var l in ac.layers)
                    {
                        if(l.stateMachine == machine)
                        {
                            var type = typeof(UnityEditor.Graphs.AnimationCurveTypeConverter).Assembly.GetType("UnityEditor.Graphs.AnimatorControllerTool");
                            var window = EditorWindow.GetWindow(type);
                            type.GetProperty("selectedLayerIndex", BindingFlags.Public | BindingFlags.Instance).SetValue(window, index);
                            break;
                        }
                        index++;
                    }
                }
                Selection.activeObject = target;
                EditorGUIUtility.PingObject(target);
            }
            EditorGUIUtility.SetIconSize(sizeCopy);
        }
    }

    internal class TableProperties
    {
        public delegate object EmphasizeGUI(int i, Rect rect);      // Return result
        public delegate bool EmphasizeCondition(int i, int count);  // Return result
        public delegate bool MainGUI(int i, int count, bool emp);   // true: Draw Default GUI

        public List<object> items;
        public string[] label;
        public Rect rect;

        public bool isEditable;
        public Type type;
        public bool allowSceneObjects;

        public bool isMask;
        public object emphasize;
        public string[] emphasizeLabels;
        public EmphasizeGUI emphasizeGUI;
        public EmphasizeCondition emphasizeCondition;
        public MainGUI mainGUI;

        public TableProperties(
            List<object> itemsIn,
            string[] labelIn,
            Rect rectIn,
            bool isEditableIn,
            Type typeIn,
            bool allowSceneObjectsIn,
            bool isMaskIn,
            object emphasizeIn,
            string[] emphasizeLabelsIn,
            EmphasizeGUI emphasizeGUIIn,
            EmphasizeCondition emphasizeConditionIn,
            MainGUI mainGUIIn
        )
        {
            items = itemsIn;
            label = labelIn;
            rect = rectIn;
            isEditable = isEditableIn;
            type = typeIn;
            allowSceneObjects = allowSceneObjectsIn;
            isMask = isMaskIn;
            emphasize = emphasizeIn;
            emphasizeLabels = emphasizeLabelsIn;
            emphasizeGUI = emphasizeGUIIn;
            emphasizeCondition = emphasizeConditionIn;
            mainGUI = mainGUIIn;
        }

        public static TableProperties CopyWithoutItems(TableProperties tp)
        {
            return new TableProperties(
                new List<object>(),
                tp.label,
                tp.rect,
                tp.isEditable,
                tp.type,
                tp.allowSceneObjects,
                tp.isMask,
                tp.emphasize,
                tp.emphasizeLabels,
                tp.emphasizeGUI,
                tp.emphasizeCondition,
                tp.mainGUI
            );
        }
    }
}
