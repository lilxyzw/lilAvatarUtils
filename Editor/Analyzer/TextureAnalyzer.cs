#if UNITY_EDITOR
using System.Collections.Generic;
using lilAvatarUtils.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

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

                #if UNITY_2022_1_OR_NEWER
                var flattened = new Material(m);
                flattened.parent = null;
                var so = new SerializedObject(flattened);
                #else
                var so = new SerializedObject(md.Key);
                #endif

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

                #if UNITY_2022_1_OR_NEWER
                Object.DestroyImmediate(flattened);
                #endif
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
                foreach(Material m in renderer.sharedMaterials)
                    AddMaterialData(mds, m, renderer.gameObject);

            var scaned = new HashSet<Object>();
            foreach(var c in gameObject.GetComponentsInChildren<MonoBehaviour>(true))
                foreach(Material m in ObjectHelper.GetReferenceFromObject<Material>(scaned, c))
                    AddMaterialData(mds, m, c.gameObject);
            return mds;
        }

        private static Dictionary<AnimationClip, AnimationClipData> GetAnimationClipDataFromGameObject(GameObject gameObject)
        {
            var acds = new Dictionary<AnimationClip, AnimationClipData>();

            foreach(var animator in gameObject.GetBuildComponents<Animator>())
                AddAnimationClipData(acds, animator.runtimeAnimatorController, animator.gameObject);

            var scaned = new HashSet<Object>();
            foreach(var mb in gameObject.GetComponentsInChildren<MonoBehaviour>(true))
            {
                foreach(var obj in ObjectHelper.GetReferenceFromObject(scaned, mb))
                {
                    if(obj is AnimationClip c) AddAnimationClipData(acds, c, mb.gameObject);
                    else if(obj is RuntimeAnimatorController ac) AddAnimationClipData(acds, ac, mb.gameObject);
                }
            }

            return acds;
        }

        private static void GetMaterialDataFromAnimationClipData(Dictionary<Material, MaterialData> mds, Dictionary<AnimationClip, AnimationClipData> acds)
        {
            foreach(KeyValuePair<AnimationClip, AnimationClipData> acd in acds)
                foreach(EditorCurveBinding binding in AnimationUtility.GetObjectReferenceCurveBindings(acd.Key))
                    foreach(ObjectReferenceKeyframe frame in AnimationUtility.GetObjectReferenceCurve(acd.Key, binding))
                        if(frame.value is Material m) AddMaterialData(mds, m, acd.Key, acd.Value);
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

        // TODO: support clip in component
        private static void AddAnimationClipData(Dictionary<AnimationClip, AnimationClipData> acds, AnimationClip clip, GameObject gameObject)
        {
            if(!acds.ContainsKey(clip))
            {
                acds[clip] = new AnimationClipData(){
                    ads = new Dictionary<RuntimeAnimatorController, AnimatorData>()
                };
            }
        }
    }
}
#endif