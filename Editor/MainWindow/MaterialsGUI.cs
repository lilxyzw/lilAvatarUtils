using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.avatarutils
{
    [Docs(T_Title,T_Description)][DocsHowTo(T_HowTo)]
    [Serializable]
    internal class MaterialsGUI : AbstractTabelGUI
    {
        internal const string T_Title = "Materials";
        internal const string T_Description = "This is a list of all materials included in the avatar. Some properties can be edited, and the changed properties will be applied all at once by pressing the Apply button in the upper left.";
        internal const string T_HowTo = "You can identify materials you forgot to replace and replace them all at once, and check the shader type and render queue to help solve avatar rendering problems. It can also be used as a tool to replace the original avatar materials all at once.";
        internal static readonly string[] T_TD = {T_Title, T_Description};

        public string empMats    = "";   private const int indMats    = 0;
        public string empRepMats = "";   private const int indRepMats = 1;
        public string empShaders = "";   private const int indShaders = 2;
        public int    empQueues  = 5000; private const int indQueues  = 3;
        public bool[] showReferences = {false};
        internal HashSet<Material> mds;

        [DocsField] private static readonly string[] L_Mats    = {"Name"        , "Asset name. Clicking this will select the corresponding asset in the Project window."};
        [DocsField] private static readonly string[] L_RepMats = {"Replace"     , "By specifying a different material here, you can replace all materials currently present on the avatar at once."};
        [DocsField] private static readonly string[] L_Shaders = {"Shader"      , "The shader that the material is using."};
        [DocsField] private static readonly string[] L_Queues  = {"Render Queue", "The rendering priority of the material. Smaller values ​​are rendered first. If a material that includes transparency is set to less than 2500, rendering problems may occur when it overlaps with the skybox. If it is set to 2501 or more, the lens effect will cause the material to lose focus and it will not be able to receive shadows. If a transparent material is set to an excessively low value (such as 2450 or less), it is very likely to cause problems with other materials being erased."};

        internal override void Draw()
        {
            if(IsEmptyLibs()) return;

            if(showReferences.Length != libs[0].items.Count) showReferences = Enumerable.Repeat(false, libs[0].items.Count).ToArray();
            base.Draw();

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
                ReferencesGUI((Material)libs[indMats].items[count]);
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
                mats.items.Add(md);
                repmats.items.Add(md);
                shaders.items.Add(md.shader);
                queues.items.Add(md.renderQueue);
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
                case indMats   :  mds = mds.Sort(md => md.name       , isDescending); break;
                case indShaders:  mds = mds.Sort(md => md.shader.name, isDescending); break;
                case indQueues :  mds = mds.Sort(md => md.renderQueue, isDescending); break;
            }
        }

        protected override void SortLibs()
        {
            SortLibs(mds.ToArray());
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
                if(matRep && isAsset && matRep.renderQueue != queue)
                {
                    if(queue == matRep.shader.renderQueue) matRep.renderQueue = -1;
                    else                                   matRep.renderQueue = queue;
                }

                // Material replace
                if(matOrig == matRep) continue;

                if(!m_window.refs.TryGetValue(matOrig, out var parents) || parents.Count == 0) return;
                foreach(var parent in parents)
                {
                    ObjectHelper.ReplaceReferences(parent, matOrig, matRep);
                }
            }
        }

        private bool EmpConRepMats(int i, int count)
        {
            return libs[i].items[count] != libs[indMats].items[count];
        }
    }
}
