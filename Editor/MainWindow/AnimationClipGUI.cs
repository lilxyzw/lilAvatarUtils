using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.avatarutils
{
    [Docs(T_Title,T_Description)][DocsHowTo(T_HowTo)]
    [Serializable]
    internal class AnimationClipGUI : AbstractTabelGUI
    {
        internal const string T_Title = "Animations";
        internal const string T_Description = "This is a list of all animations included in the avatar. You can also see what types of keyframes the animation contains.";
        internal const string T_HowTo = "Problems that don't occur in the editor but occur in the game are often caused by animations, but if there are a lot of animations, it can take time to identify the cause. This tool lists all the animations included in the avatar and also displays the animation type, helping you identify the animation that is causing the problem.";
        internal static readonly string[] T_TD = {T_Title, T_Description};

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
        internal Dictionary<AnimationClip, AnimationClipData> acds;

        [DocsField] private static readonly string[] L_Clips             = {"Name"             , "Asset name. Clicking this will select the corresponding asset in the Project window."};
        [DocsField] private static readonly string[] L_Humanoids         = {"Humanoid"         , "Whether the animation includes humanoid manipulation."};
        [DocsField] private static readonly string[] L_BlendShapes       = {"BlendShape"       , "Whether the animation includes blendshape manipulation."};
        [DocsField] private static readonly string[] L_ToggleActives     = {"Object Active"    , "Whether the animation includes toggling objects."};
        [DocsField] private static readonly string[] L_ToggleEnableds    = {"Component Enable" , "Whether the animation includes toggling components."};
        [DocsField] private static readonly string[] L_Transforms        = {"Transform"        , "Whether the animation includes transform manipulation."};
        [DocsField] private static readonly string[] L_MaterialReplaces  = {"Material Replace" , "Whether the animation includes material replacement."};
        [DocsField] private static readonly string[] L_MaterialPropertys = {"Material Property", "Whether the animation includes material property manipulation."};
        [DocsField] private static readonly string[] L_Others            = {"Others"           , "Whether the animation includes any other actions."};

        internal override void Draw()
        {
            if(IsEmptyLibs()) return;

            if(showReferences.Length != libs[0].items.Count) showReferences = Enumerable.Repeat(false, libs[0].items.Count).ToArray();
            base.Draw();

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
                L10n.LabelField(L_ReferencedFrom);
                ReferencesGUI((AnimationClip)libs[indClips].items[count]);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }

        internal override void Set()
        {
            isModified = false;
            var matType = typeof(Material);
            //                                          items               label                 rect                 isEdit type     scene  isMask emp                  labs  empGUI empCon mainGUI
            var clips              = new TableProperties(new List<object>(), L_Clips            , new Rect(0,0,200,0), false, null   , false, false, empClips            , null, null,  null , null);
            var humanoids          = new TableProperties(new List<object>(), L_Humanoids        , new Rect(0,0,100,0), false, null   , false, true , empHumanoids        , null, null,  null , null);
            var blendShapes        = new TableProperties(new List<object>(), L_BlendShapes      , new Rect(0,0,100,0), false, null   , false, true , empBlendShapes      , null, null,  null , null);
            var toggleActives      = new TableProperties(new List<object>(), L_ToggleActives    , new Rect(0,0,100,0), false, null   , false, true , empToggleActives    , null, null,  null , null);
            var toggleEnableds     = new TableProperties(new List<object>(), L_ToggleEnableds   , new Rect(0,0,100,0), false, null   , false, true , empToggleEnableds   , null, null,  null , null);
            var transforms         = new TableProperties(new List<object>(), L_Transforms       , new Rect(0,0,100,0), false, null   , false, true , empTransforms       , null, null,  null , null);
            var materialReplaces   = new TableProperties(new List<object>(), L_MaterialReplaces , new Rect(0,0,100,0), false, null   , false, true , empMaterialReplaces , null, null,  null , null);
            var materialPropertys  = new TableProperties(new List<object>(), L_MaterialPropertys, new Rect(0,0,100,0), false, null   , false, true , empMaterialPropertys, null, null,  null , null);
            var others             = new TableProperties(new List<object>(), L_Others           , new Rect(0,0,100,0), false, null   , false, true , empOthers           , null, null,  null , null);

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
