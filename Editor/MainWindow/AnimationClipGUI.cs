using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using lilAvatarUtils.Utils;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace lilAvatarUtils.MainWindow
{
    [Serializable]
    internal class AnimationClipGUI : AbstractTabelGUI
    {
        public string empClips              = ""; private const int indClips            = 0;
        public int    empHumanoids          = 2 ; private const int indHumanoid         = 1;
        public int    empBlendShapes        = 2 ; private const int indBlendShape       = 2;
        public int    empToggleActives      = 2 ; private const int indToggleActive     = 3;
        public int    empToggleEnableds     = 2 ; private const int indToggleEnabled    = 4;
        public int    empTransforms         = 2 ; private const int indTransform        = 5;
        public int    empMaterialReplaces   = 2 ; private const int indMaterialReplace  = 6;
        public int    empMaterialPropertys  = 2 ; private const int indMaterialProperty = 7;
        public int    empOthers             = 2 ; private const int indOther            = 8;
        public bool[] showReferences = {false};
        internal Dictionary<AnimationClip, AnimationClipData> acds = new Dictionary<AnimationClip, AnimationClipData>();

        internal override void Draw(AvatarUtilsWindow window)
        {
            if(IsEmptyLibs()) return;

            if(showReferences.Length != libs[0].items.Count) showReferences = Enumerable.Repeat(false, libs[0].items.Count).ToArray();
            base.Draw(window);

            empClips              = (string)libs[indClips           ].emphasize;
            empHumanoids          = (int   )libs[indHumanoid        ].emphasize;
            empBlendShapes        = (int   )libs[indBlendShape      ].emphasize;
            empToggleActives      = (int   )libs[indToggleActive    ].emphasize;
            empToggleEnableds     = (int   )libs[indToggleEnabled   ].emphasize;
            empTransforms         = (int   )libs[indTransform       ].emphasize;
            empMaterialReplaces   = (int   )libs[indMaterialReplace ].emphasize;
            empMaterialPropertys  = (int   )libs[indMaterialProperty].emphasize;
            empOthers             = (int   )libs[indOther           ].emphasize;
        }

        protected override void LineGUIEx(int count)
        {
            showReferences[count] = GUIUtils.Foldout(new Rect(libs[0].rect.x - 16, libs[0].rect.y, 16, libs[0].rect.height), showReferences[count]);
            if(showReferences[count])
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Referenced from");
                var acd = acds[(AnimationClip)libs[indClips].items[count]];
                foreach(var ad in acd.ads)
                {
                    foreach(var state in ad.Value.states)
                    {
                        LabelFieldWithSelection(ad.Key, state.Item2, state.Item1);
                    }
                    EditorGUI.indentLevel++;
                    foreach(var obj in ad.Value.gameObjects)
                    {
                        GUIUtils.LabelFieldWithSelection(obj);
                    }
                    EditorGUI.indentLevel--;
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }

        private static void LabelFieldWithSelection(RuntimeAnimatorController controller, AnimatorControllerLayer layer, AnimatorState state, bool hilight = false)
        {
            Rect rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
            GUIStyle style;
            if(hilight) style = GUIUtils.styleRed;
            else        style = EditorStyles.label;
            GUIContent content = EditorGUIUtility.ObjectContent(state, state.GetType());
            content.tooltip = AssetDatabase.GetAssetPath(state);
            if(!string.IsNullOrEmpty(content.tooltip)) content.text = Path.GetFileName(content.tooltip) + " -> " + layer.name + " -> " + state.name;
            if(AssetDatabase.IsSubAsset(state)) content.text = state.name;

            var sizeCopy = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(new Vector2(rect.height-2, rect.height-2));
            if(GUIUtils.UnchangeButton(rect, content, style) && state != null)
            {
                Selection.activeObject = controller;
                if(controller is AnimatorController ac)
                {
                    var index = 0;
                    foreach(var l in ac.layers)
                    {
                        if(l.stateMachine == layer.stateMachine)
                        {
                            var type = typeof(UnityEditor.Graphs.AnimationCurveTypeConverter).Assembly.GetType("UnityEditor.Graphs.AnimatorControllerTool");
                            var window = EditorWindow.GetWindow(type);
                            type.GetProperty("selectedLayerIndex", BindingFlags.Public | BindingFlags.Instance).SetValue(window, index);
                            break;
                        }
                        index++;
                    }
                }
                Selection.activeObject = state;
                EditorGUIUtility.PingObject(state);
            }
            EditorGUIUtility.SetIconSize(sizeCopy);
        }

        internal override void Set()
        {
            isModified = false;
            var matType = typeof(Material);
            //                                          items               label          rect                 isEdit type     scene  isMask emp                  labs  empGUI empCon mainGUI
            var clips              = new TableProperties(new List<object>(), "Name"       , new Rect(0,0,200,0), false, null   , false, false, empClips            , null, null,  null , null);
            var humanoids          = new TableProperties(new List<object>(), "Humanoid"   , new Rect(0,0,80 ,0), false, null   , false, true , empHumanoids        , null, null,  null , null);
            var blendShapes        = new TableProperties(new List<object>(), "BlendShape" , new Rect(0,0,80 ,0), false, null   , false, true , empBlendShapes      , null, null,  null , null);
            var toggleActives      = new TableProperties(new List<object>(), "Obj Active" , new Rect(0,0,80 ,0), false, null   , false, true , empToggleActives    , null, null,  null , null);
            var toggleEnableds     = new TableProperties(new List<object>(), "Comp Enable", new Rect(0,0,80 ,0), false, null   , false, true , empToggleEnableds   , null, null,  null , null);
            var transforms         = new TableProperties(new List<object>(), "Transform"  , new Rect(0,0,80 ,0), false, null   , false, true , empTransforms       , null, null,  null , null);
            var materialReplaces   = new TableProperties(new List<object>(), "Mat Replace", new Rect(0,0,80 ,0), false, null   , false, true , empMaterialReplaces , null, null,  null , null);
            var materialPropertys  = new TableProperties(new List<object>(), "Mat Prop"   , new Rect(0,0,80 ,0), false, null   , false, true , empMaterialPropertys, null, null,  null , null);
            var others             = new TableProperties(new List<object>(), "Others"     , new Rect(0,0,80 ,0), false, null   , false, true , empOthers           , null, null,  null , null);

            Sort();
            foreach(var acd in acds)
            {
                clips            .items.Add(acd.Key);
                humanoids        .items.Add(acd.Value.hasHumanoid);
                blendShapes      .items.Add(acd.Value.hasBlendShape);
                toggleActives    .items.Add(acd.Value.hasToggleActive);
                toggleEnableds   .items.Add(acd.Value.hasToggleEnabled);
                transforms       .items.Add(acd.Value.hasTransform);
                materialReplaces .items.Add(acd.Value.hasMaterialReplace);
                materialPropertys.items.Add(acd.Value.hasMaterialProperty);
                others           .items.Add(acd.Value.hasOther);
            }

            libs = new []{
                clips            ,
                humanoids        ,
                blendShapes      ,
                toggleActives    ,
                toggleEnableds   ,
                transforms       ,
                materialReplaces ,
                materialPropertys,
                others
            };
        }

        protected override void Sort()
        {
            switch(sortIndex)
            {
                case indClips           : acds = acds.Sort(acd => acd.Key.name                  , isDescending); break;
                case indHumanoid        : acds = acds.Sort(acd => acd.Value.hasHumanoid         , isDescending); break;
                case indBlendShape      : acds = acds.Sort(acd => acd.Value.hasBlendShape       , isDescending); break;
                case indToggleActive    : acds = acds.Sort(acd => acd.Value.hasToggleActive     , isDescending); break;
                case indToggleEnabled   : acds = acds.Sort(acd => acd.Value.hasToggleEnabled    , isDescending); break;
                case indTransform       : acds = acds.Sort(acd => acd.Value.hasTransform        , isDescending); break;
                case indMaterialReplace : acds = acds.Sort(acd => acd.Value.hasMaterialReplace  , isDescending); break;
                case indMaterialProperty: acds = acds.Sort(acd => acd.Value.hasMaterialProperty , isDescending); break;
                case indOther           : acds = acds.Sort(acd => acd.Value.hasOther            , isDescending); break;
            }
        }

        protected override void SortLibs()
        {
            SortLibs(acds.Keys.ToArray());
        }
    }
}
