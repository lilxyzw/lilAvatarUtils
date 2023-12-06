#if UNITY_EDITOR
using System.Collections.Generic;
using lilAvatarUtils.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
#if LIL_VRCSDK3_AVATARS
using VRCAvatarDescriptor = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor;
#endif
#if LIL_MODULAR_AVATAR
using ModularAvatarMergeAnimator = nadena.dev.modular_avatar.core.ModularAvatarMergeAnimator;
#endif

namespace lilAvatarUtils.Analyzer
{
    internal class TextureAnalyzer
    {
        internal static void Analyze(
            GameObject gameObject,
            out Dictionary<Texture, TextureData> tds,
            out Dictionary<Material, MaterialData> mds
        )
        {
            tds = new Dictionary<Texture, TextureData>();
            mds = GetMaterialDataFromGameObject(gameObject);
            var acds = GetAnimationClipDataFromGameObject(gameObject);
            GetMaterialDataFromAnimationClipData(mds, acds);

            foreach(KeyValuePair<Material, MaterialData> md in mds)
            {
                Material m = md.Key;
                var so = new SerializedObject(md.Key);
                var props = so.FindProperty("m_SavedProperties").FindPropertyRelative("m_TexEnvs");
                for(int i = 0; i < props.arraySize; i++)
                {
                    var prop = props.GetArrayElementAtIndex(i).FindPropertyRelative("second").FindPropertyRelative("m_Texture").objectReferenceValue;
                    if(prop == null) continue;
                    if(prop is Texture t)
                    {
                        if(tds.ContainsKey(t))
                        {
                            tds[t].mds[m] = md.Value;
                            continue;
                        }

                        TextureType type = TextureType.Texture;
                        TextureFormat format = TextureFormat.RGBA32;
                        RenderTextureFormat rtformat = RenderTextureFormat.ARGB32;
                        long vramSize = 0;
                        switch(t)
                        {
                            case Texture2D o           : type = TextureType.Texture2D          ; format = o.format;   vramSize = MathHelper.ComputeVRAMSize(t, o.format); break;
                            case Cubemap o             : type = TextureType.Cubemap            ; format = o.format;   vramSize = MathHelper.ComputeVRAMSize(t, o.format); break;
                            case Texture3D o           : type = TextureType.Texture3D          ; format = o.format;   vramSize = MathHelper.ComputeVRAMSize(t, o.format); break;
                            case Texture2DArray o      : type = TextureType.Texture2DArray     ; format = o.format;   vramSize = MathHelper.ComputeVRAMSize(t, o.format); break;
                            case CubemapArray o        : type = TextureType.CubemapArray       ; format = o.format;   vramSize = MathHelper.ComputeVRAMSize(t, o.format); break;
                            case CustomRenderTexture o : type = TextureType.CustomRenderTexture; rtformat = o.format; vramSize = MathHelper.ComputeVRAMSize(t, o.format, o.depth); break;
                            case RenderTexture o       : type = TextureType.RenderTexture      ; rtformat = o.format; vramSize = MathHelper.ComputeVRAMSize(t, o.format, o.depth); break;
                        }

                        var td = new TextureData(){
                            type = type,
                            format = format,
                            rtformat = rtformat,
                            vramSize = vramSize,
                            memorySize = Profiler.GetRuntimeMemorySizeLong(t),
                            mds = new Dictionary<Material, MaterialData>()
                        };
                        td.mds[m] = md.Value;

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
                }
            }
        }

        internal static void Analyze(GameObject gameObject, out Dictionary<Texture, TextureData> tds)
        {
            Analyze(gameObject, out tds, out _);
        }

        private static Dictionary<Material, MaterialData> GetMaterialDataFromGameObject(GameObject gameObject)
        {
            var mds = new Dictionary<Material, MaterialData>();
            foreach(var renderer in gameObject.GetBuildComponents<Renderer>())
            {
                foreach(Material m in renderer.sharedMaterials)
                {
                    AddMaterialData(mds, m, renderer.gameObject);
                }
            }
            return mds;
        }

        private static Dictionary<AnimationClip, AnimationClipData> GetAnimationClipDataFromGameObject(GameObject gameObject)
        {
            var acds = new Dictionary<AnimationClip, AnimationClipData>();

            foreach(var animator in gameObject.GetBuildComponents<Animator>())
            {
                AddAnimationClipData(acds, animator.runtimeAnimatorController, animator.gameObject);
            }

            #if LIL_VRCSDK3_AVATARS
            foreach(var descriptor in gameObject.GetBuildComponents<VRCAvatarDescriptor>())
            {
                foreach(var layer in descriptor.specialAnimationLayers)
                {
                    AddAnimationClipData(acds, layer.animatorController, descriptor.gameObject);
                }
                if(descriptor.customizeAnimationLayers)
                {
                    foreach(var layer in descriptor.baseAnimationLayers)
                    {
                        AddAnimationClipData(acds, layer.animatorController, descriptor.gameObject);
                    }
                }
            }
            #endif

            #if LIL_MODULAR_AVATAR
            foreach(var ma in gameObject.GetBuildComponents<ModularAvatarMergeAnimator>())
            {
                AddAnimationClipData(acds, ma.animator, ma.gameObject);
            }
            #endif

            return acds;
        }

        private static void GetMaterialDataFromAnimationClipData(Dictionary<Material, MaterialData> mds, Dictionary<AnimationClip, AnimationClipData> acds)
        {
            foreach(KeyValuePair<AnimationClip, AnimationClipData> acd in acds)
            {
                foreach(EditorCurveBinding binding in AnimationUtility.GetObjectReferenceCurveBindings(acd.Key))
                {
                    foreach(ObjectReferenceKeyframe frame in AnimationUtility.GetObjectReferenceCurve(acd.Key, binding))
                    {
                        if(frame.value is Material m) AddMaterialData(mds, m, acd.Key, acd.Value);
                    }
                }
            }
        }

        private static void AddMaterialData(Dictionary<Material, MaterialData> mds, Material m, GameObject gameObject)
        {
            if(m == null) return;
            if(mds.ContainsKey(m))
            {
                if(!mds[m].gameObjects.Contains(gameObject)) mds[m].gameObjects.Add(gameObject);
                return;
            }
            mds[m] = new MaterialData(){
                gameObjects = new HashSet<GameObject>(){gameObject},
                acds = new Dictionary<AnimationClip, AnimationClipData>()
            };
        }

        private static void AddMaterialData(Dictionary<Material, MaterialData> mds, Material m, AnimationClip c, AnimationClipData acd)
        {
            if(!mds.ContainsKey(m))
            {
                mds[m] = new MaterialData(){
                    acds = new Dictionary<AnimationClip, AnimationClipData>()
                };
            }
            if(!mds[m].acds.ContainsKey(c)) mds[m].acds[c] = acd;
        }

        private static void AddAnimationClipData(Dictionary<AnimationClip, AnimationClipData> acds, RuntimeAnimatorController controller, GameObject gameObject)
        {
            if(controller == null) return;
            foreach(AnimationClip c in controller.animationClips)
            {
                if(!acds.ContainsKey(c))
                {
                    acds[c] = new AnimationClipData(){
                        ads = new Dictionary<RuntimeAnimatorController, AnimatorData>()
                    };
                }

                if(acds[c].ads.ContainsKey(controller))
                {
                    if(!acds[c].ads[controller].gameObjects.Contains(gameObject))
                    {
                        acds[c].ads[controller].gameObjects.Add(gameObject);
                    }
                    continue;
                }

                acds[c].ads[controller] = new AnimatorData(){
                    gameObjects = new HashSet<GameObject>(){gameObject}
                };
            }
        }
    }
}
#endif