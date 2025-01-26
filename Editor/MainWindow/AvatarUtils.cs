using System;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.avatarutils
{
    internal class AvatarUtils : EditorWindow
    {
        internal const string TEXT_WINDOW_NAME = "lilAvatarUtils";

        public EditorMode editorMode = EditorMode.Textures;
        public GameObject gameObject;
        public TexturesGUI texturesGUI = new TexturesGUI();
        public MaterialsGUI materialsGUI = new MaterialsGUI();
        public AnimationClipGUI animationClipGUI = new AnimationClipGUI();
        public RenderersGUI renderersGUI = new RenderersGUI();
        #if LIL_VRCSDK3_AVATARS
        public PhysBonesGUI physBonesGUI = new PhysBonesGUI();
        public PhysBoneCollidersGUI physBoneCollidersGUI = new PhysBoneCollidersGUI();
        #endif
        public LightingTestGUI lightingTestGUI = new LightingTestGUI();
        public UtilsGUI utilsGUI = new UtilsGUI();

        [NonSerialized] private bool isAnalyzed = false;
        [NonSerialized] Vector3 prevPosition = Vector3.zero;
        [NonSerialized] Quaternion prevRotation = Quaternion.identity;
        [NonSerialized] float prevFov = 0;
        [NonSerialized] float prevNear = 0;
        [NonSerialized] float prevFar = 0;

        [MenuItem("Tools/lilAvatarUtils")]
        internal static void Init()
        {
            string windowName = $"{TEXT_WINDOW_NAME} {PackageJsonReader.GetVersion()}";
            var window = (AvatarUtils)GetWindow(typeof(AvatarUtils), false, windowName);
            window.Show();
        }

        internal void OnGUI()
        {
            GUIUtils.Initialize();

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

            string[] sEditorModeList = Enum.GetNames(typeof(EditorMode));
            editorMode = (EditorMode)L10n.Toolbar((int)editorMode, sEditorModeList);
            switch (editorMode)
            {
                case EditorMode.Textures: texturesGUI.Draw(this); break;
                case EditorMode.Materials: materialsGUI.Draw(this); break;
                case EditorMode.Animations: animationClipGUI.Draw(this); break;
                case EditorMode.Renderers: renderersGUI.Draw(this); break;
                #if LIL_VRCSDK3_AVATARS
                case EditorMode.PhysBones: physBonesGUI.Draw(this); break;
                case EditorMode.PBColliders: physBoneCollidersGUI.Draw(this); break;
                #endif
                case EditorMode.Lighting: lightingTestGUI.Draw(this); break;
                case EditorMode.Utils: utilsGUI.Draw(this); break;
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

            GUI.enabled = gameObject != null;
            bool shouldRefresh = GUI.Button(rectButton, GUIUtils.iconRefresh);
            GUI.enabled = true;

            if((gameObject2 != null && gameObject2 != gameObject) || (gameObject != null && !isAnalyzed) || shouldRefresh)
            {
                if(gameObject2 != null) gameObject = gameObject2;
                Analyze();
                isAnalyzed = true;
            }

            gameObject = gameObject2;
        }

        private void Update()
        {
            if(editorMode != EditorMode.Lighting) return;
            var sceneView = SceneView.lastActiveSceneView;
            if(sceneView == null || sceneView.camera == null) return;

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
            TextureAnalyzer.Analyze(gameObject, out texturesGUI.tds, out materialsGUI.mds, out animationClipGUI.acds);
            RendererAnalyzer.Analyze(gameObject, out renderersGUI.smrs, out renderersGUI.mrs, out renderersGUI.psrs);
            AnimationAnalyzer.Analyze(animationClipGUI.acds);

            texturesGUI.Set();
            materialsGUI.Set();
            animationClipGUI.Set();
            renderersGUI.Set();
            lightingTestGUI.Set(true);

            #if LIL_VRCSDK3_AVATARS
            PhysBonesAnalyzer.Analyze(gameObject, out physBonesGUI.pbs, out physBonesGUI.pbcs);
            physBoneCollidersGUI.pbcs = physBonesGUI.pbcs;
            physBonesGUI.Set();
            physBoneCollidersGUI.Set();
            #endif
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
