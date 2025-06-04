using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace jp.lilxyzw.avatarutils
{
    [Docs(T_Title,T_Description)][DocsHowTo(T_HowTo)]
    [Serializable]
    internal class LightingTestGUI
    {
        internal const string T_Title = "Lighting";
        internal const string T_Description = "This is a tool to check how your avatar looks in various environments. When you install VRCSDK, you can also check how your avatar looks when shaders are not applied due to safety.";
        internal const string T_HowTo = "It is useful for checking the appearance of avatars under special lighting conditions that cannot be detected in the Unity default scene. It also allows you to check rendering problems that you cannot detect by yourself when safety is activated.";
        internal static readonly string[] T_TD = {T_Title, T_Description};

        internal GameObject gameObject;
        private GameObject prevGameObject = null;
        private GameObject renderedGameObject = null;
        #if LIL_VRCSDK3_AVATARS
        private GameObject renderedSafetyGameObject = null;
        #endif
        private GameObject gameObjectMainLight = null;
        private List<GameObject> gameObjectSubLights = new();
        private GameObject gameObjectCube = null;

        private PreviewRenderUtility preview = null;
        private AmbientMode ambientModeCopy;
        private Color ambientLightCopy;
        private float intensityCopy = 1.0f;
        private Material skyboxCopy;

        public bool isSafetyOn = false;
        public bool isMenuOpened = false;
        public Color colorAmbient = new(0.75f, 0.58f, 0.49f);
        public Color colorDirectional = Color.black;
        public Color colorSpot0 = Color.black;
        public Color colorSpot1 = Color.black;
        public Color colorSpot2 = Color.black;
        public LightShadows lightShadows = LightShadows.None;
        public float reflectionIntensity = 0;
        internal AvatarUtils m_window;

        [DocsField] private static readonly string[] L_NoLight      = {"No light"     ,"There is no light, including ambient light."};
        [DocsField] private static readonly string[] L_Overexposure = {"Overexposure" ,"It is illuminated by excessively bright directional light."};
        [DocsField] private static readonly string[] L_InShadow     = {"In Shadow"    ,"The whole avatar is in shadow."};
        [DocsField] private static readonly string[] L_SpotLight    = {"Spot Light"   ,"It is lit by one spotlight."};
        [DocsField] private static readonly string[] L_3SpotLights  = {"3 Spot Lights","It is lit by three spotlights."};
        [DocsField] private static readonly string[] L_Custom       = {"Custom"       ,"User can customize lighting."};

        private static readonly string[] L_Ambient    = {"Ambient"     , ""};
        private static readonly string[] L_MainLight  = {"Main Light"  , ""};
        private static readonly string[] L_SpotLight0 = {"Spot Light 0", ""};
        private static readonly string[] L_SpotLight1 = {"Spot Light 1", ""};
        private static readonly string[] L_SpotLight2 = {"Spot Light 2", ""};
        private static readonly string[] L_Shadows    = {"Shadows"     , ""};
        private static readonly string[] L_Reflection = {"Reflection"  , ""};

        private static readonly string[] L_SimulateSafetyEnabled = {"Simulate safety enabled", "This simulates the appearance when safety is enabled on VRChat. Since the implementation of VRChat is unknown, it may not be reproduced completely."};

        internal void Draw()
        {
            if(!gameObject) return;
            #if LIL_VRCSDK3_AVATARS
            if(isSafetyOn != L10n.ToggleLeft(L_SimulateSafetyEnabled, isSafetyOn))
            {
                isSafetyOn = !isSafetyOn;
                if(isSafetyOn)
                {
                    renderedGameObject.SetActive(false);
                    renderedSafetyGameObject.SetActive(true);
                }
                else
                {
                    renderedGameObject.SetActive(true);
                    renderedSafetyGameObject.SetActive(false);
                }
            }
            #endif

            var rect = EditorGUILayout.GetControlRect(GUILayout.MaxWidth(m_window.position.width), GUILayout.MaxHeight(m_window.position.height));
            if(rect.width < 32 || rect.height < 32) return;
            float width = Mathf.Round(rect.width/3-4);
            float height = Mathf.Round(rect.height/2-4);
            var rects = new[]{
                new Rect(rect.x          , rect.y         , width, height),
                new Rect(rect.x+width  +4, rect.y         , width, height),
                new Rect(rect.x+width*2+8, rect.y         , width, height),
                new Rect(rect.x          , rect.y+height+4, width, height),
                new Rect(rect.x+width  +4, rect.y+height+4, width, height),
                new Rect(rect.x+width*2+8, rect.y+height+4, width, height)
            };

            ambientModeCopy = RenderSettings.ambientMode;
            ambientLightCopy = RenderSettings.ambientLight;
            intensityCopy = RenderSettings.reflectionIntensity;
            skyboxCopy = RenderSettings.skybox;

            InitializePreviewScene();

            if(!gameObjectCube)
            {
                gameObjectCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                gameObjectCube.transform.position = gameObject.transform.position;
                gameObjectCube.transform.localScale = new Vector3(500,500,500);
                preview.AddSingleGO(gameObjectCube);
            }

            if(!gameObjectMainLight)
            {
                gameObjectMainLight = new GameObject("Main Light", typeof(Light));
                var mainLightComponent = gameObjectMainLight.GetComponent<Light>();
                mainLightComponent.transform.position = gameObject.transform.position + new Vector3(0,3,0);
                mainLightComponent.transform.rotation = Quaternion.Euler(50,-120,0);
                mainLightComponent.type = LightType.Directional;
                mainLightComponent.useColorTemperature = false;
                mainLightComponent.color = Color.white;
                mainLightComponent.lightmapBakeType = LightmapBakeType.Realtime;
                mainLightComponent.intensity = 1;
                mainLightComponent.shadows = LightShadows.None;
                mainLightComponent.renderMode = LightRenderMode.ForcePixel;
                preview.AddSingleGO(gameObjectMainLight);
            }

            if(
                gameObjectSubLights == null ||
                gameObjectSubLights.Count != 3 ||
                !gameObjectSubLights[0] ||
                !gameObjectSubLights[1] ||
                !gameObjectSubLights[2]
            )
            {
                gameObjectSubLights = new List<GameObject>();
                gameObjectSubLights.Add(new GameObject("Spot Light 0", typeof(Light)));
                preview.AddSingleGO(gameObjectSubLights[0]);
                var subLight0Component = gameObjectSubLights[0].GetComponent<Light>();
                InitializeSpotLight(subLight0Component);
                subLight0Component.transform.position = gameObject.transform.position + new Vector3(-4.5f, 5f, -3.5f);
                subLight0Component.transform.rotation = Quaternion.Euler(43,50,0);

                gameObjectSubLights.Add(new GameObject("Spot Light 1", typeof(Light)));
                preview.AddSingleGO(gameObjectSubLights[1]);
                var subLight1Component = gameObjectSubLights[1].GetComponent<Light>();
                InitializeSpotLight(subLight1Component);
                subLight1Component.transform.position = gameObject.transform.position + new Vector3(4.5f, 5f, -3.5f);
                subLight1Component.transform.rotation = Quaternion.Euler(43,-50,0);

                gameObjectSubLights.Add(new GameObject("Spot Light 2", typeof(Light)));
                preview.AddSingleGO(gameObjectSubLights[2]);
                var subLight2Component = gameObjectSubLights[2].GetComponent<Light>();
                InitializeSpotLight(subLight2Component);
                subLight2Component.transform.position = gameObject.transform.position + new Vector3(0f, 5f, 4f);
                subLight2Component.transform.rotation = Quaternion.Euler(130,0,0);
            }

            Set(false);

            var mainLight = gameObjectMainLight.GetComponent<Light>();
            var subLight0 = gameObjectSubLights[0].GetComponent<Light>();
            var subLight1 = gameObjectSubLights[1].GetComponent<Light>();
            var subLight2 = gameObjectSubLights[2].GetComponent<Light>();
            subLight0.color = new Color(0.8f,0.8f,0.8f,1.0f);
            subLight1.color = new Color(0.8f,0.8f,0.8f,1.0f);
            subLight2.color = new Color(0.8f,0.8f,0.8f,1.0f);

            var sceneView = SceneView.lastActiveSceneView;
            if(sceneView && sceneView.camera)
            {
                var sceneCamera = sceneView.camera;
                preview.camera.transform.position = sceneCamera.transform.position;
                preview.camera.transform.rotation = sceneCamera.transform.rotation;
                preview.camera.fieldOfView        = sceneCamera.fieldOfView       ;
                preview.camera.nearClipPlane      = sceneCamera.nearClipPlane     ;
                preview.camera.farClipPlane       = sceneCamera.farClipPlane      ;
            }
            else
            {
                preview.camera.transform.position   = new Vector3(0,2,1);
                preview.camera.transform.rotation   = Quaternion.Euler(0,180,0);
                preview.camera.fieldOfView          = 60;
                preview.camera.nearClipPlane        = 0.01f;
                preview.camera.farClipPlane         = 1000;
            }

            subLight0.enabled = false;
            subLight1.enabled = false;
            subLight2.enabled = false;
            gameObjectCube.SetActive(true);

            // No light
            preview.ambientColor = new Color(0,0,0,1);
            ResetScene(rects[0]);
            preview.camera.clearFlags = CameraClearFlags.SolidColor;
            preview.camera.backgroundColor = Color.black;
            mainLight.enabled = false;
            subLight0.enabled = false;
            subLight1.enabled = false;
            subLight2.enabled = false;
            SetPreviewRenderSettings();
            preview.camera.Render();
            DrawLightPreview(rects[0], L_NoLight);

            // Overexposure
            preview.ambientColor = new Color(0.21f,0.22f,0.25f,1);
            ResetScene(rects[1]);
            preview.camera.clearFlags = CameraClearFlags.Skybox;
            mainLight.color = new Color(1.25f,1.25f,1.25f,1);
            mainLight.enabled = true;
            subLight0.enabled = false;
            subLight1.enabled = false;
            subLight2.enabled = false;
            mainLight.shadows = LightShadows.None;
            SetPreviewRenderSettings();
            preview.camera.Render();
            DrawLightPreview(rects[1], L_Overexposure);

            // In Shadow
            preview.ambientColor = new Color(0.21f,0.22f,0.25f,1);
            ResetScene(rects[2]);
            preview.camera.clearFlags = CameraClearFlags.Skybox;
            mainLight.color = Color.white;
            mainLight.enabled = true;
            subLight0.enabled = false;
            subLight1.enabled = false;
            subLight2.enabled = false;
            mainLight.shadows = LightShadows.Soft;
            SetPreviewRenderSettings();
            preview.camera.Render();
            DrawLightPreview(rects[2], L_InShadow);

            // Spot Light
            preview.ambientColor = new Color(0,0,0,1);
            ResetScene(rects[3]);
            preview.camera.clearFlags = CameraClearFlags.SolidColor;
            preview.camera.backgroundColor = Color.black;
            mainLight.enabled = false;
            subLight0.enabled = false;
            subLight1.enabled = false;
            subLight2.enabled = true;
            SetPreviewRenderSettings();
            preview.camera.Render();
            DrawLightPreview(rects[3], L_SpotLight);

            // Spot Lights
            preview.ambientColor = new Color(0,0,0,1);
            ResetScene(rects[4]);
            preview.camera.clearFlags = CameraClearFlags.SolidColor;
            preview.camera.backgroundColor = Color.black;
            mainLight.enabled = false;
            subLight0.enabled = true;
            subLight1.enabled = true;
            subLight2.enabled = true;
            SetPreviewRenderSettings();
            preview.camera.Render();
            DrawLightPreview(rects[4], L_3SpotLights);

            // Custom
            gameObjectCube.SetActive(false);
            preview.ambientColor = colorAmbient;
            ResetScene(rects[5]);
            preview.camera.clearFlags = CameraClearFlags.Skybox;
            mainLight.enabled = true;
            subLight0.enabled = true;
            subLight1.enabled = true;
            subLight2.enabled = true;
            mainLight.color = colorDirectional;
            subLight0.color = colorSpot0;
            subLight1.color = colorSpot1;
            subLight2.color = colorSpot2;
            SetPreviewRenderSettings();
            mainLight.shadows = lightShadows;
            preview.camera.Render();
            DrawLightPreview(rects[5], L_Custom);
            DrawCustomSettings(rects[5]);

            RenderSettings.ambientMode = ambientModeCopy;
            RenderSettings.ambientLight = ambientLightCopy;
            RenderSettings.reflectionIntensity = intensityCopy;
            RenderSettings.skybox = skyboxCopy;
        }

        internal void OnDisable()
        {
            if(preview != null) preview.Cleanup();
        }

        internal void Set(bool forceUpdate)
        {
            if(gameObject && (prevGameObject != gameObject || !renderedGameObject || forceUpdate))
            {

                prevGameObject = gameObject;
                InitializePreviewScene();
                SafeDestroy(renderedGameObject);
                renderedGameObject = Object.Instantiate(gameObject);
                preview.AddSingleGO(renderedGameObject);

                #if LIL_VRCSDK3_AVATARS
                SafeDestroy(renderedSafetyGameObject);
                renderedSafetyGameObject = Object.Instantiate(gameObject);
                preview.AddSingleGO(renderedSafetyGameObject);
                SetSafetyMaterial(renderedSafetyGameObject);
                if(isSafetyOn) renderedGameObject.SetActive(false);
                else           renderedSafetyGameObject.SetActive(false);
                SafeDestroy(renderedGameObject.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>());
                SafeDestroy(renderedSafetyGameObject.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>());
                #endif
            }
        }

        private void InitializePreviewScene()
        {
            if(preview == null)
            {
                preview = new PreviewRenderUtility();

                var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                plane.transform.position = gameObject.transform.position;
                preview.AddSingleGO(plane);
            }
        }

        private void ResetScene(Rect rect)
        {
            preview.BeginPreview(rect, GUIStyle.none);
            foreach(var light in preview.lights) light.enabled = false;
        }

        private void InitializeSpotLight(Light light)
        {
            light.type = LightType.Spot;
            light.useColorTemperature = false;
            light.lightmapBakeType = LightmapBakeType.Realtime;
            light.intensity = 1;
            light.shadows = LightShadows.None;
            light.renderMode = LightRenderMode.ForcePixel;
            light.spotAngle = 20;
            light.range = 100;
        }

        private void DrawLightPreview(Rect rect, string[] label)
        {
            GUI.DrawTexture(rect, preview.EndPreview(), ScaleMode.ScaleToFit, false);
            DrawHeader(rect, label);
        }

        private void DrawHeader(Rect rect, string[] label)
        {
            var rectHeader = new Rect(rect.x,rect.y,rect.width,16);
            EditorGUI.DrawRect(rectHeader, new Color(0.0f, 0.0f, 0.0f, 0.75f));
            L10n.LabelField(rectHeader, label, GUIUtils.styleWhiteBold);
        }

        private void DrawCustomSettings(Rect rect)
        {
            if(GUI.Button(new Rect(rect.xMax-16,rect.y,16,16), GUIUtils.iconMenu_D, EditorStyles.label))
            {
                isMenuOpened = !isMenuOpened;
            }

            if(isMenuOpened)
            {
                Rect rectSetting = new Rect(rect.xMax-80-150,rect.y+16,230,16*7);
                EditorGUI.DrawRect(rectSetting, new Color(0.0f, 0.0f, 0.0f, 0.75f));
                L10n.LabelField(new Rect(rect.xMax-80-150,rect.y+ 16,80,16), L_Ambient   , GUIUtils.styleWhite);
                L10n.LabelField(new Rect(rect.xMax-80-150,rect.y+ 32,80,16), L_MainLight , GUIUtils.styleWhite);
                L10n.LabelField(new Rect(rect.xMax-80-150,rect.y+ 48,80,16), L_SpotLight0, GUIUtils.styleWhite);
                L10n.LabelField(new Rect(rect.xMax-80-150,rect.y+ 64,80,16), L_SpotLight1, GUIUtils.styleWhite);
                L10n.LabelField(new Rect(rect.xMax-80-150,rect.y+ 80,80,16), L_SpotLight2, GUIUtils.styleWhite);
                L10n.LabelField(new Rect(rect.xMax-80-150,rect.y+ 96,80,16), L_Shadows   , GUIUtils.styleWhite);
                L10n.LabelField(new Rect(rect.xMax-80-150,rect.y+112,80,16), L_Reflection, GUIUtils.styleWhite);
                colorAmbient        = EditorGUI.ColorField(             new Rect(rect.xMax-150,rect.y+ 16,150,16), GUIContent.none, colorAmbient    , true, false, true);
                colorDirectional    = EditorGUI.ColorField(             new Rect(rect.xMax-150,rect.y+ 32,150,16), GUIContent.none, colorDirectional, true, false, true);
                colorSpot0          = EditorGUI.ColorField(             new Rect(rect.xMax-150,rect.y+ 48,150,16), GUIContent.none, colorSpot0      , true, false, true);
                colorSpot1          = EditorGUI.ColorField(             new Rect(rect.xMax-150,rect.y+ 64,150,16), GUIContent.none, colorSpot1      , true, false, true);
                colorSpot2          = EditorGUI.ColorField(             new Rect(rect.xMax-150,rect.y+ 80,150,16), GUIContent.none, colorSpot2      , true, false, true);
                lightShadows        = (LightShadows)EditorGUI.EnumPopup(new Rect(rect.xMax-150,rect.y+ 96,150,16), lightShadows);
                reflectionIntensity = EditorGUI.Slider(                 new Rect(rect.xMax-150,rect.y+112,150,16), reflectionIntensity, 0, 1);

                var e = Event.current;
                if(e.type == EventType.MouseDown && !rectSetting.Contains(e.mousePosition))
                {
                    isMenuOpened = false;
                }
            }
        }

        private void SafeDestroy(Object obj)
        {
            if(obj) Object.DestroyImmediate(obj);
        }

        private void SetPreviewRenderSettings()
        {
            RenderSettings.reflectionIntensity = reflectionIntensity;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = preview.ambientColor;
        }

        #if LIL_VRCSDK3_AVATARS
        private void SetSafetyMaterial(GameObject obj)
        {
            foreach(var renderer in obj.GetComponentsInChildren<Renderer>(true))
            {
                var materials = new Material[renderer.sharedMaterials.Length];
                for(int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    materials[i] = GetSafetyMaterial(renderer.sharedMaterials[i]);
                }
                renderer.sharedMaterials = materials;
            }
        }

        private Material GetSafetyMaterial(Material material)
        {
            if(
                !material ||
                material.shader && material.shader.name.StartsWith("VRChat/")
            ) return material;

            var tag = material.GetTag("VRCFallback", true);
            if(string.IsNullOrEmpty(tag) && material.shader) tag = material.shader.name.Replace("Hidden","");

            var materialFallbackCopy = new Material(TagToSafetyShader(tag));
            materialFallbackCopy.CopyPropertiesFromMaterial(material);

            // Properties copied via CopyPropertiesFromMaterial remain incomplete until the scene is saved.
            // Creating a new material instance ensures all properties are correctly applied.
            var materialFallback = new Material(materialFallbackCopy);
            Object.DestroyImmediate(materialFallbackCopy);

            materialFallback.renderQueue = materialFallback.shader.renderQueue;
            if(tag.Contains("DoubleSided"))
            {
                materialFallback.SetInt("_Cull", 0);
                materialFallback.SetInt("_Culling", 0);
            }
            else
            {
                materialFallback.SetInt("_Cull", 2);
                materialFallback.SetInt("_Culling", 2);
            }

            foreach (var pass in new string[] { "Always", "ForwardBase", "ForwardAdd", "Deferred", "ShadowCaster", "MotionVectors", "Vertex", "VertexLMRGBM", "VertexLM", "Meta" })
            {
                materialFallback.SetShaderPassEnabled(pass, true);
            }

            return materialFallback;
        }

        private Shader TagToSafetyShader(string tag)
        {
            // Fallback on the following priorities
            if(tag.Contains("Hidden"))
            {
                return Shader.Find("Hidden/lilAvatarUtils/FallbackHidden");
            }
            if(tag.Contains("Sprite"))
            {
                return Shader.Find("Sprites/Default");
            }
            if(tag.Contains("Particle"))
            {
                return Shader.Find("Legacy Shaders/Particles/Multiply");
            }
            if(tag.Contains("Matcap"))
            {
                // _MatCap property is not copied
                //return Shader.Find("VRChat/Mobile/MatCap Lit");
                return Shader.Find("Hidden/lilAvatarUtils/FallbackToon");
            }
            if(tag.Contains("Toon"))
            {
                if(tag.Contains("Transparent")) return Shader.Find("Hidden/lilAvatarUtils/FallbackUnlitTransparent");
                else if(tag.Contains("Cutout")) return Shader.Find("Hidden/lilAvatarUtils/FallbackToonCutout");
                else if(tag.Contains("Fade"))   return Shader.Find("Hidden/lilAvatarUtils/FallbackUnlitTransparent");
                else                            return Shader.Find("Hidden/lilAvatarUtils/FallbackToon");
            }
            if(tag.Contains("Unlit"))
            {
                if(tag.Contains("Transparent")) return Shader.Find("Hidden/lilAvatarUtils/FallbackUnlitTransparent");
                else if(tag.Contains("Cutout")) return Shader.Find("Hidden/lilAvatarUtils/FallbackUnlitCutout");
                else if(tag.Contains("Fade"))   return Shader.Find("Hidden/lilAvatarUtils/FallbackUnlitTransparent");
                else                            return Shader.Find("Hidden/lilAvatarUtils/FallbackUnlit");
            }
            if(tag.Contains("VertexLit"))
            {
                return Shader.Find("Legacy Shaders/VertexLit");
            }
            if(tag.Contains("toonstandard"))
            {
                var shader = default(Shader);
                if(tag.Contains("outline")) shader = Shader.Find("VRChat/Mobile/Toon Standard (Outline)");
                else                        shader = Shader.Find("VRChat/Mobile/Toon Standard");
                if(shader != null) return shader;
            }
            return Shader.Find("Standard");
        }
        #endif
    }
}
