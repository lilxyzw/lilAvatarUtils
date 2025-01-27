using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace jp.lilxyzw.avatarutils
{
    [Docs("Main Window", "This is the main window of the tool. You can assign an avatar and view and edit data in each tab.")]
    [DocsMenuLocation(MENU_PATH)]
    internal class AvatarUtils : EditorWindow
    {
        internal const string TEXT_WINDOW_NAME = "lilAvatarUtils";
        private const string MENU_PATH = "Tools/lilAvatarUtils";
        internal static readonly (Type,string[],EditorMode)[] TabTypes = {
            (typeof(TexturesGUI),          TexturesGUI.T_TD,          EditorMode.Textures),
            (typeof(MaterialsGUI),         MaterialsGUI.T_TD,         EditorMode.Materials),
            (typeof(AnimationClipGUI),     AnimationClipGUI.T_TD,     EditorMode.Animations),
            (typeof(RenderersGUI),         RenderersGUI.T_TD,         EditorMode.Renderers),
            #if LIL_VRCSDK3_AVATARS
            (typeof(PhysBonesGUI),         PhysBonesGUI.T_TD,         EditorMode.PhysBones),
            (typeof(PhysBoneCollidersGUI), PhysBoneCollidersGUI.T_TD, EditorMode.PBColliders),
            #endif
            (typeof(LightingTestGUI),      LightingTestGUI.T_TD,      EditorMode.Lighting),
            (typeof(UtilsGUI),             UtilsGUI.T_TD,             EditorMode.Utils),
        };
        private static readonly string[][] L_EditorModeList = TabTypes.Select(t => t.Item2).ToArray();

        public EditorMode editorMode = EditorMode.Textures;
        public GameObject gameObject;
        public TexturesGUI texturesGUI = new();
        public MaterialsGUI materialsGUI = new();
        public AnimationClipGUI animationClipGUI = new();
        public RenderersGUI renderersGUI = new();
        #if LIL_VRCSDK3_AVATARS
        public PhysBonesGUI physBonesGUI = new();
        public PhysBoneCollidersGUI physBoneCollidersGUI = new();
        #endif
        public LightingTestGUI lightingTestGUI = new();
        public UtilsGUI utilsGUI = new();
        internal Dictionary<UnityEngine.Object, HashSet<UnityEngine.Object>> refs;

        [NonSerialized] private bool isAnalyzed = false;
        [NonSerialized] Vector3 prevPosition = Vector3.zero;
        [NonSerialized] Quaternion prevRotation = Quaternion.identity;
        [NonSerialized] float prevFov = 0;
        [NonSerialized] float prevNear = 0;
        [NonSerialized] float prevFar = 0;

        [MenuItem(MENU_PATH)]
        internal static void Init()
        {
            string windowName = $"{TEXT_WINDOW_NAME} {PackageJsonReader.GetVersion()}";
            var window = (AvatarUtils)GetWindow(typeof(AvatarUtils), false, windowName);
            window.Show();
        }

        internal void OnGUI()
        {
            GUIUtils.Initialize();
            texturesGUI.m_window = this;
            materialsGUI.m_window = this;
            animationClipGUI.m_window = this;
            renderersGUI.m_window = this;
            #if LIL_VRCSDK3_AVATARS
            physBonesGUI.m_window = this;
            physBoneCollidersGUI.m_window = this;
            #endif
            lightingTestGUI.m_window = this;
            utilsGUI.m_window = this;

            // 言語設定
            var langs = L10n.GetLanguages();
            var names = L10n.GetLanguageNames();
            EditorGUI.BeginChangeCheck();
            var ind = EditorGUILayout.Popup("Language", Array.IndexOf(langs, AvatarUtilsSettings.instance.language), names);
            if(EditorGUI.EndChangeCheck())
            {
                AvatarUtilsSettings.instance.language = langs[ind];
                AvatarUtilsSettings.instance.Save();
                L10n.Load();
            }

            GameObjectSelectionGUI();

            editorMode = (EditorMode)L10n.Toolbar((int)editorMode, L_EditorModeList);
            switch (editorMode)
            {
                case EditorMode.Textures: texturesGUI.Draw(); break;
                case EditorMode.Materials: materialsGUI.Draw(); break;
                case EditorMode.Animations: animationClipGUI.Draw(); break;
                case EditorMode.Renderers: renderersGUI.Draw(); break;
                #if LIL_VRCSDK3_AVATARS
                case EditorMode.PhysBones: physBonesGUI.Draw(); break;
                case EditorMode.PBColliders: physBoneCollidersGUI.Draw(); break;
                #endif
                case EditorMode.Lighting: lightingTestGUI.Draw(); break;
                case EditorMode.Utils: utilsGUI.Draw(); break;
            }
        }

        private void GameObjectSelectionGUI()
        {
            var rect = EditorGUILayout.GetControlRect(GUILayout.Height(32));
            var rectButton = new Rect(rect.x, rect.y, 32, rect.height);
            var rectField = new Rect(rectButton.xMax + 2, rect.y, rect.width - rectButton.width - 2, rect.height);

            EditorStyles.objectField.fontSize = 20;
            GameObject gameObject2 = (GameObject)EditorGUI.ObjectField(rectField, gameObject, typeof(GameObject), true);
            EditorStyles.objectField.fontSize = GUIUtils.objectFieldFontSize;

            GUI.enabled = gameObject;
            bool shouldRefresh = GUI.Button(rectButton, GUIUtils.iconRefresh);
            GUI.enabled = true;

            if((gameObject2 && gameObject2 != gameObject) || (gameObject && !isAnalyzed) || shouldRefresh)
            {
                if(gameObject2) gameObject = gameObject2;
                Analyze();
                isAnalyzed = true;
            }

            gameObject = gameObject2;
        }

        private void Update()
        {
            if(editorMode != EditorMode.Lighting) return;
            var sceneView = SceneView.lastActiveSceneView;
            if(!sceneView || !sceneView.camera) return;

            bool shouldRepaint = false;
            var sceneCamera = sceneView.camera;
            if(prevPosition != sceneCamera.transform.position){prevPosition = sceneCamera.transform.position; shouldRepaint = true;};
            if(prevRotation != sceneCamera.transform.rotation){prevRotation = sceneCamera.transform.rotation; shouldRepaint = true;};
            if(prevFov      != sceneCamera.fieldOfView       ){prevFov      = sceneCamera.fieldOfView       ; shouldRepaint = true;};
            if(prevNear     != sceneCamera.nearClipPlane     ){prevNear     = sceneCamera.nearClipPlane     ; shouldRepaint = true;};
            if(prevFar      != sceneCamera.farClipPlane      ){prevFar      = sceneCamera.farClipPlane      ; shouldRepaint = true;};
            if(shouldRepaint) Repaint();
        }

        internal void Analyze()
        {
            SetGameObject();
            refs = CommonAnalyzer.GetObjectReferences(gameObject);

            var texs = refs.Where(kv => kv.Key is Texture).Select(kv => kv.Key as Texture);
            var tds = new Dictionary<Texture, TextureData>();
            foreach(var t in texs.ToArray())
            {
                TextureType type = TextureType.Texture;
                TextureFormat format = TextureFormat.RGBA32;
                RenderTextureFormat rtformat = RenderTextureFormat.ARGB32;
                long vramSize = 0;
                switch(t)
                {
                    case Texture2D o           : type = TextureType.Texture2D          ; format = o.format;   vramSize = MathHelper.ComputeVRAMSize(o, o.format); break;
                    case Cubemap o             : type = TextureType.Cubemap            ; format = o.format;   vramSize = MathHelper.ComputeVRAMSize(o, o.format); break;
                    case Texture3D o           : type = TextureType.Texture3D          ; format = o.format;   vramSize = MathHelper.ComputeVRAMSize(o, o.format); break;
                    case Texture2DArray o      : type = TextureType.Texture2DArray     ; format = o.format;   vramSize = MathHelper.ComputeVRAMSize(o, o.format); break;
                    case CubemapArray o        : type = TextureType.CubemapArray       ; format = o.format;   vramSize = MathHelper.ComputeVRAMSize(o, o.format); break;
                    case CustomRenderTexture o : type = TextureType.CustomRenderTexture; rtformat = o.format; vramSize = MathHelper.ComputeVRAMSize(o, o.format); break;
                    case RenderTexture o       : type = TextureType.RenderTexture      ; rtformat = o.format; vramSize = MathHelper.ComputeVRAMSize(o, o.format); break;
                }

                var td = new TextureData(){
                    type = type,
                    format = format,
                    rtformat = rtformat,
                    vramSize = vramSize,
                    memorySize = Profiler.GetRuntimeMemorySizeLong(t)
                };

                string path = AssetDatabase.GetAssetPath(t);
                var textureImporter = AssetImporter.GetAtPath(path);
                if(textureImporter is TextureImporter ti)
                {
                    td.maxTextureSize      = ti.maxTextureSize;
                    td.textureCompression  = ti.textureCompression;
                    td.crunchedCompression = ti.crunchedCompression;
                    td.compressionQuality  = ti.compressionQuality;
                    td.sRGBTexture         = ti.sRGBTexture;
                    td.alphaSource         = ti.alphaSource;
                    td.alphaIsTransparency = ti.alphaIsTransparency;
                    td.mipmapEnabled       = ti.mipmapEnabled;
                    td.streamingMipmaps    = ti.streamingMipmaps;
                    td.isReadable          = ti.isReadable;
                }
                tds[t] = td;
            }
            texturesGUI.tds = tds;

            materialsGUI.mds = refs.Where(kv => kv.Key is Material).Select(kv => kv.Key as Material).ToHashSet();

            var clips = refs.Where(kv => kv.Key is AnimationClip).Select(kv => kv.Key as AnimationClip);
            var acds = new Dictionary<AnimationClip, AnimationClipData>();
            foreach(var clip in clips.ToArray())
            {
                var clipData = new AnimationClipData();
                var bindings = AnimationUtility.GetCurveBindings(clip);
                foreach(var binding in AnimationUtility.GetCurveBindings(clip))
                    CheckBindingType(binding, clipData);
                foreach(var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
                    CheckBindingType(binding, clipData);
                clipData.hasHumanoid = clip.humanMotion;
                acds[clip] = clipData;
            }
            animationClipGUI.acds = acds;

            renderersGUI.smrs = gameObject.GetBuildComponents<SkinnedMeshRenderer>().ToHashSet();
            renderersGUI.psrs = gameObject.GetBuildComponents<ParticleSystemRenderer>().ToHashSet();
            renderersGUI.mrs = new HashSet<(MeshRenderer,MeshFilter)>();
            foreach(var mr in gameObject.GetBuildComponents<MeshRenderer>().ToArray())
            {
                var mf = mr.gameObject.GetComponent<MeshFilter>();
                renderersGUI.mrs.Add((mr, mf));
            }

            texturesGUI.Set();
            materialsGUI.Set();
            animationClipGUI.Set();
            renderersGUI.Set();
            lightingTestGUI.Set(true);

            #if LIL_VRCSDK3_AVATARS
            physBonesGUI.pbs = gameObject.GetBuildComponents<VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone>().ToHashSet();
            physBonesGUI.pbcs = gameObject.GetBuildComponents<VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBoneCollider>().ToHashSet();

            physBoneCollidersGUI.pbcs = physBonesGUI.pbcs;
            physBonesGUI.Set();
            physBoneCollidersGUI.Set();
            #endif
        }

        private static void CheckBindingType(EditorCurveBinding binding, AnimationClipData clipData)
        {
            if(binding.type == typeof(Animator))
            {
            }
            else if(binding.type == typeof(SkinnedMeshRenderer))
            {
                if(binding.propertyName.StartsWith("blendShape.")) clipData.hasBlendShape = true;
                else if(binding.propertyName.StartsWith("m_Materials.Array.data["))  clipData.hasMaterialReplace = true;
                else if(binding.propertyName.StartsWith("material."))  clipData.hasMaterialReplace = true;
            }
            else if(binding.type == typeof(GameObject) && binding.propertyName.StartsWith("m_IsActive"))
            {
                clipData.hasToggleActive = true;
            }
            else if(binding.type.IsSubclassOf(typeof(Component)) && binding.propertyName.StartsWith("m_Enabled"))
            {
                clipData.hasToggleEnabled = true;
            }
            else if(binding.type == typeof(Transform))
            {
                clipData.hasTransform = true;
            }
            else
            {
                clipData.hasOther = true;
            }
        }

        private void SetGameObject()
        {
            texturesGUI.gameObject = gameObject;
            materialsGUI.gameObject = gameObject;
            animationClipGUI.gameObject = gameObject;
            renderersGUI.gameObject = gameObject;
            lightingTestGUI.gameObject = gameObject;
            utilsGUI.gameObject = gameObject;
            #if LIL_VRCSDK3_AVATARS
            physBonesGUI.gameObject = gameObject;
            physBoneCollidersGUI.gameObject = gameObject;
            #endif
        }

        internal void OnDisable()
        {
            lightingTestGUI.OnDisable();
        }

        internal enum EditorMode
        {
            Textures,
            Materials,
            Animations,
            Renderers,
            #if LIL_VRCSDK3_AVATARS
            PhysBones,
            PBColliders,
            #endif
            Lighting,
            Utils
        }
    }
}
