#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace lilAvatarUtils.MainWindow
{
    [Serializable]
    internal class LightingTestGUI
    {
        internal GameObject gameObject;
        private GameObject prevGameObject = null;
        private GameObject renderedGameObject = null;
        #if LIL_VRCSDK3_AVATARS
        private GameObject renderedSafetyGameObject = null;
        #endif
        private GameObject gameObjectMainLight = null;
        private List<GameObject> gameObjectSubLights = new List<GameObject>();
        private GameObject gameObjectCube = null;

        private PreviewRenderUtility preview = null;
        #if !UNITY_2020_1_OR_NEWER
        private RenderTexture previewRenderTexture = null;
        #endif
        private float intensityCopy = 1.0f;
        private Material skyboxCopy;

        public bool isSafetyOn = false;
        public bool isMenuOpened = false;
        public Color colorAmbient = new Color(0.75f, 0.58f, 0.49f);
        public Color colorDirectional = Color.black;
        public Color colorSpot0 = Color.black;
        public Color colorSpot1 = Color.black;
        public Color colorSpot2 = Color.black;
        public LightShadows lightShadows = LightShadows.None;
        public float reflectionIntensity = 0;

        internal void Draw(EditorWindow window)
        {
            #if LIL_VRCSDK3_AVATARS
            if(isSafetyOn != EditorGUILayout.Toggle("Safety On", isSafetyOn))
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

            var rect = EditorGUILayout.GetControlRect(GUILayout.MaxWidth(window.position.width), GUILayout.MaxHeight(window.position.height));
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

            intensityCopy = RenderSettings.reflectionIntensity;
            skyboxCopy = RenderSettings.skybox;

            InitializePreviewScene();

            if(gameObjectCube == null)
            {
                gameObjectCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                gameObjectCube.transform.localScale = new Vector3(500,500,500);
                preview.AddSingleGO(gameObjectCube);
            }

            if(gameObjectMainLight == null)
            {
                gameObjectMainLight = new GameObject("Main Light", typeof(Light));
                var mainLightComponent = gameObjectMainLight.GetComponent<Light>();
                mainLightComponent.transform.position = new Vector3(0,3,0);
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
                gameObjectSubLights[0] == null ||
                gameObjectSubLights[1] == null ||
                gameObjectSubLights[2] == null
            )
            {
                gameObjectSubLights = new List<GameObject>();
                gameObjectSubLights.Add(new GameObject("Spot Light 0", typeof(Light)));
                preview.AddSingleGO(gameObjectSubLights[0]);
                var subLight0Component = gameObjectSubLights[0].GetComponent<Light>();
                InitializeSpotLight(subLight0Component);
                subLight0Component.transform.position = new Vector3(-4.5f, 5f, -3.5f);
                subLight0Component.transform.rotation = Quaternion.Euler(43,50,0);

                gameObjectSubLights.Add(new GameObject("Spot Light 1", typeof(Light)));
                preview.AddSingleGO(gameObjectSubLights[1]);
                var subLight1Component = gameObjectSubLights[1].GetComponent<Light>();
                InitializeSpotLight(subLight1Component);
                subLight1Component.transform.position = new Vector3(4.5f, 5f, -3.5f);
                subLight1Component.transform.rotation = Quaternion.Euler(43,-50,0);

                gameObjectSubLights.Add(new GameObject("Spot Light 2", typeof(Light)));
                preview.AddSingleGO(gameObjectSubLights[2]);
                var subLight2Component = gameObjectSubLights[2].GetComponent<Light>();
                InitializeSpotLight(subLight2Component);
                subLight2Component.transform.position = new Vector3(0f, 5f, 4f);
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
            if(sceneView != null && sceneView.camera != null)
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
            RenderSettings.reflectionIntensity = 0;
            preview.camera.Render();
            DrawLightPreview(rects[0], "No light");

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
            RenderSettings.reflectionIntensity = 1;
            preview.camera.Render();
            DrawLightPreview(rects[1], "Overexposure");

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
            RenderSettings.reflectionIntensity = 1;
            preview.camera.Render();
            DrawLightPreview(rects[2], "In Shadow");

            // Spot Light
            preview.ambientColor = new Color(0,0,0,1);
            ResetScene(rects[3]);
            preview.camera.clearFlags = CameraClearFlags.SolidColor;
            preview.camera.backgroundColor = Color.black;
            mainLight.enabled = false;
            subLight0.enabled = false;
            subLight1.enabled = false;
            subLight2.enabled = true;
            RenderSettings.reflectionIntensity = 0;
            preview.camera.Render();
            DrawLightPreview(rects[3], "Spot Light");

            // Spot Lights
            preview.ambientColor = new Color(0,0,0,1);
            ResetScene(rects[4]);
            preview.camera.clearFlags = CameraClearFlags.SolidColor;
            preview.camera.backgroundColor = Color.black;
            mainLight.enabled = false;
            subLight0.enabled = true;
            subLight1.enabled = true;
            subLight2.enabled = true;
            RenderSettings.reflectionIntensity = 0;
            preview.camera.Render();
            DrawLightPreview(rects[4], "3 Spot Lights");

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
            RenderSettings.reflectionIntensity = reflectionIntensity;
            mainLight.shadows = lightShadows;
            preview.camera.Render();
            DrawLightPreview(rects[5], "Custom");
            DrawCustomSettings(rects[5]);

            RenderSettings.reflectionIntensity = intensityCopy;
            RenderSettings.skybox = skyboxCopy;
        }

        internal void OnDisable()
        {
            if(preview != null) preview.Cleanup();
            #if !UNITY_2020_1_OR_NEWER
            SafeDestroy(previewRenderTexture);
            #endif
        }

        internal void Set(bool forceUpdate)
        {
            if(gameObject != null && (prevGameObject != gameObject || renderedGameObject == null || forceUpdate))
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
                preview.AddSingleGO(plane);
            }
        }

        private void ResetScene(Rect rect)
        {
            preview.BeginPreview(rect, GUIStyle.none);
            foreach(var light in preview.lights) light.enabled = false;

            #if !UNITY_2020_1_OR_NEWER
            var rt = preview.camera.targetTexture; // targetTexture is initialized at BeginPreview()
            int width = rt.width;
            int height = rt.height;
            if(previewRenderTexture == null || previewRenderTexture.width != width || previewRenderTexture.height != height)
            {
                SafeDestroy(previewRenderTexture);
                previewRenderTexture = new RenderTexture(width, height, 32, rt.format);
                previewRenderTexture.hideFlags = HideFlags.HideAndDontSave;
                preview.camera.targetTexture = previewRenderTexture;
            }
            #endif
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

        private void DrawLightPreview(Rect rect, string label)
        {
            #if !UNITY_2020_1_OR_NEWER
            preview.EndPreview();
            if(previewRenderTexture != null) GUI.DrawTexture(rect, previewRenderTexture, ScaleMode.ScaleToFit, false);
            #else
            GUI.DrawTexture(rect, preview.EndPreview(), ScaleMode.ScaleToFit, false);
            #endif
            DrawHeader(rect, label);
        }

        private void DrawHeader(Rect rect, string label)
        {
            var rectHeader = new Rect(rect.x,rect.y,rect.width,16);
            EditorGUI.DrawRect(rectHeader, new Color(0.0f, 0.0f, 0.0f, 0.75f));
            EditorGUI.LabelField(rectHeader, label, GUIUtils.styleWhiteBold);
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
                EditorGUI.LabelField(new Rect(rect.xMax-80-150,rect.y+ 16,80,16), "Ambient", GUIUtils.styleWhite);
                EditorGUI.LabelField(new Rect(rect.xMax-80-150,rect.y+ 32,80,16), "Main Light", GUIUtils.styleWhite);
                EditorGUI.LabelField(new Rect(rect.xMax-80-150,rect.y+ 48,80,16), "Spot Light 0", GUIUtils.styleWhite);
                EditorGUI.LabelField(new Rect(rect.xMax-80-150,rect.y+ 64,80,16), "Spot Light 1", GUIUtils.styleWhite);
                EditorGUI.LabelField(new Rect(rect.xMax-80-150,rect.y+ 80,80,16), "Spot Light 2", GUIUtils.styleWhite);
                EditorGUI.LabelField(new Rect(rect.xMax-80-150,rect.y+ 96,80,16), "Shadows", GUIUtils.styleWhite);
                EditorGUI.LabelField(new Rect(rect.xMax-80-150,rect.y+112,80,16), "Reflection", GUIUtils.styleWhite);
                colorAmbient        = EditorGUI.ColorField(             new Rect(rect.xMax-150,rect.y+ 16,150,16), new GUIContent(""), colorAmbient    , true, false, true);
                colorDirectional    = EditorGUI.ColorField(             new Rect(rect.xMax-150,rect.y+ 32,150,16), new GUIContent(""), colorDirectional, true, false, true);
                colorSpot0          = EditorGUI.ColorField(             new Rect(rect.xMax-150,rect.y+ 48,150,16), new GUIContent(""), colorSpot0      , true, false, true);
                colorSpot1          = EditorGUI.ColorField(             new Rect(rect.xMax-150,rect.y+ 64,150,16), new GUIContent(""), colorSpot1      , true, false, true);
                colorSpot2          = EditorGUI.ColorField(             new Rect(rect.xMax-150,rect.y+ 80,150,16), new GUIContent(""), colorSpot2      , true, false, true);
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
            if(obj != null) Object.DestroyImmediate(obj);
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
                material == null ||
                material.shader != null && material.shader.name.StartsWith("VRChat/")
            ) return material;

            var tag = material.GetTag("VRCFallback", true);
            if(string.IsNullOrEmpty(tag) && material.shader != null) tag = material.shader.name.Replace("Hidden","");

            var materialFallback = new Material(TagToSafetyShader(tag));
            materialFallback.CopyPropertiesFromMaterial(material);
            if(tag.Contains("DoubleSided")) materialFallback.SetInt("_Cull", 0);
            else                            materialFallback.SetInt("_Cull", 2);

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
            return Shader.Find("Standard");
        }
        #endif
    }
}
#endif