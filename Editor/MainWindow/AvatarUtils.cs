using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
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
            if(editorMode == EditorMode.Textures)
            {
                texturesGUI.Draw(this);
                return;
            }
            if(editorMode == EditorMode.Materials)
            {
                materialsGUI.Draw(this);
                return;
            }
            if(editorMode == EditorMode.Animations)
            {
                animationClipGUI.Draw(this);
                return;
            }
            if(editorMode == EditorMode.Renderers)
            {
                renderersGUI.Draw(this);
                return;
            }
            #if LIL_VRCSDK3_AVATARS
            if(editorMode == EditorMode.PhysBones)
            {
                physBonesGUI.Draw(this);
                return;
            }
            if(editorMode == EditorMode.PBColliders)
            {
                physBoneCollidersGUI.Draw(this);
                return;
            }
            #endif
            if(editorMode == EditorMode.Lighting)
            {
                lightingTestGUI.Draw(this);
                return;
            }
            if(editorMode == EditorMode.Utils)
            {
                if(gameObject == null) return;
                if(L10n.Button("Clean up Materials"))
                {
                    var cleanedMaterials = MaterialCleaner.RemoveUnusedProperties(materialsGUI.mds.Keys);
                    L10n.DisplayDialog(
                        TEXT_WINDOW_NAME,
                        "Removed unused properties on {0} materials.",
                        "OK",
                        cleanedMaterials.Count
                    );
                }
                if(L10n.Button("Clean up AnimatorControllers"))
                {
                    var controllers = new HashSet<RuntimeAnimatorController>(
                        gameObject.GetBuildComponents<Animator>().Select(a => a.runtimeAnimatorController)
                    );

                    var scaned = new HashSet<UnityEngine.Object>();
                    controllers.UnionWith(gameObject.GetComponentsInChildren<MonoBehaviour>(true).SelectMany(c => ObjectHelper.GetReferenceFromObject<RuntimeAnimatorController>(scaned, c)));

                    var cleanedControllers = SubAssetCleaner.RemoveUnusedSubAssets(controllers.Where(ac => ac is AnimatorController));
                    L10n.DisplayDialog(
                        TEXT_WINDOW_NAME,
                        "Removed unused sub-assets in {0} AnimatorControllers.",
                        "OK",
                        cleanedControllers.Count
                    );
                }
                if(L10n.Button("Remove Missing Components"))
                {
                    int count = 0;
                    foreach(var t in gameObject.GetComponentsInChildren<Transform>(true))
                    {
                        count += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
                    }
                    L10n.DisplayDialog(
                        TEXT_WINDOW_NAME,
                        "Removed {0} missing components.",
                        "OK",
                        count
                    );
                }
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
