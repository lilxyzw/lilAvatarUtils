using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.avatarutils
{
    [Serializable]
    internal class MaterialsGUI : AbstractTabelGUI
    {
        public string empMats    = "";   private const int indMats    = 0;
        public string empRepMats = "";   private const int indRepMats = 1;
        public string empShaders = "";   private const int indShaders = 2;
        public int    empQueues  = 5000; private const int indQueues  = 3;
        public bool[] showReferences = {false};
        internal Dictionary<Material, MaterialData> mds = new Dictionary<Material, MaterialData>();

        private static readonly string[] L_Mats    = {"Name"        , "Asset name. Clicking this will select the corresponding asset in the Project window."};
        private static readonly string[] L_RepMats = {"Replace"     , "By specifying a different material here, you can replace all materials currently present on the avatar at once."};
        private static readonly string[] L_Shaders = {"Shader"      , "The shader that the material is using."};
        private static readonly string[] L_Queues  = {"Render Queue", "The rendering priority of the material. Smaller values ​​are rendered first. If a material that includes transparency is set to less than 2500, rendering problems may occur when it overlaps with the skybox. If it is set to 2501 or more, the lens effect will cause the material to lose focus and it will not be able to receive shadows. If a transparent material is set to an excessively low value (such as 2450 or less), it is very likely to cause problems with other materials being erased."};

        internal override void Draw(AvatarUtils window)
        {
            if(IsEmptyLibs()) return;

            if(showReferences.Length != libs[0].items.Count) showReferences = Enumerable.Repeat(false, libs[0].items.Count).ToArray();
            base.Draw(window);

            empMats    = (string)libs[indMats   ].emphasize;
            //empRepMats = (string)libs[indRepMats].emphasize;
            empShaders = (string)libs[indShaders].emphasize;
            empQueues  = (int   )libs[indQueues].emphasize;
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
                var md = mds[(Material)libs[indMats].items[count]];
                if(md.gameObjects != null)
                {
                    foreach(GameObject obj in md.gameObjects)
                    {
                        GUIUtils.LabelFieldWithSelection(obj);
                    }
                }
                if(md.acds != null)
                {
                    foreach(KeyValuePair<AnimationClip, AnimationClipData> acd in md.acds)
                    {
                        GUIUtils.LabelFieldWithSelection(acd.Key);
                        EditorGUI.indentLevel++;
                        foreach(KeyValuePair<RuntimeAnimatorController, AnimatorData> ad in acd.Value.ads)
                        {
                            GUIUtils.LabelFieldWithSelection(ad.Key);
                            EditorGUI.indentLevel++;
                            foreach(GameObject obj in ad.Value.gameObjects)
                            {
                                GUIUtils.LabelFieldWithSelection(obj);
                            }
                            EditorGUI.indentLevel--;
                        }
                        EditorGUI.indentLevel--;
                    }
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }

        internal override void Set()
        {
            isModified = false;
            var matType = typeof(Material);
            //                                items               label      rect                 isEdit type     scene  isMask emp         labs  empGUI empCon         mainGUI
            var mats    = new TableProperties(new List<object>(), L_Mats   , new Rect(0,0,200,0), false, null   , false, false, empMats   , null, null,  null         , null);
            var repmats = new TableProperties(new List<object>(), L_RepMats, new Rect(0,0,200,0), true , matType, false, false, null      , null, null,  EmpConRepMats, null);
            var shaders = new TableProperties(new List<object>(), L_Shaders, new Rect(0,0,300,0), false, null   , false, false, empShaders, null, null,  null         , null);
            var queues  = new TableProperties(new List<object>(), L_Queues , new Rect(0,0,100,0), true , null   , false, false, empQueues , null, null,  null         , null);

            Sort();
            foreach(var md in mds)
            {
                mats.items.Add(md.Key);
                repmats.items.Add(md.Key);
                shaders.items.Add(md.Key.shader);
                queues.items.Add(md.Key.renderQueue);
            }

            libs = new []{
                mats   ,
                repmats,
                shaders,
                queues
            };
        }

        protected override void Sort()
        {
            switch(sortIndex)
            {
                case indMats   :  mds = mds.Sort(md => md.Key.name       , isDescending); break;
                case indShaders:  mds = mds.Sort(md => md.Key.shader.name, isDescending); break;
                case indQueues :  mds = mds.Sort(md => md.Key.renderQueue, isDescending); break;
            }
        }

        protected override void SortLibs()
        {
            SortLibs(mds.Keys.ToArray());
        }

        protected override void ApplyModification()
        {
            for(int count = 0; count < libs[0].items.Count; count++)
            {
                var matOrig = (Material)libs[indMats].items[count];
                var matRep = (Material)libs[indRepMats].items[count];
                var queue = (int)libs[indQueues].items[count];
                var path = AssetDatabase.GetAssetPath(matRep);
                bool isAsset = path.Contains("Assets") || path.Contains("Packages");

                // Set render queue
                if(matRep != null && isAsset && matRep.renderQueue != queue)
                {
                    if(queue == matRep.shader.renderQueue) matRep.renderQueue = -1;
                    else                                   matRep.renderQueue = queue;
                }

                // Material replace
                if(matOrig == matRep) continue;
                var md = mds[matOrig];

                // Replace references in material slots (Renderer)
                if(md.gameObjects != null)
                {
                    foreach(GameObject obj in md.gameObjects)
                    {
                        foreach(var renderer in obj.GetBuildComponents<Renderer>())
                        {
                            var mats = renderer.sharedMaterials;
                            for(int i = 0; i < mats.Length; i++)
                            {
                                if(mats[i] == matOrig) mats[i] = matRep;
                            }
                            if(renderer.sharedMaterials != mats) renderer.sharedMaterials = mats;
                        }
                    }
                }

                // Replace references in AnimationClip
                if(md.acds != null)
                {
                    foreach(var clip in md.acds.Keys)
                    {
                        foreach(EditorCurveBinding binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
                        {
                            var curves = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                            bool isReplaced = false;
                            for(int i = 0; i < curves.Length; i++)
                            {
                                if(curves[i].value is Material m && m == matOrig)
                                {
                                    curves[i].value = matRep;
                                    isReplaced = true;
                                }
                            }
                            if(isReplaced) AnimationUtility.SetObjectReferenceCurve(clip, binding, curves);
                        }
                    }
                }
            }
        }

        private bool EmpConRepMats(int i, int count)
        {
            return libs[i].items[count] != libs[indMats].items[count];
        }
    }
}
