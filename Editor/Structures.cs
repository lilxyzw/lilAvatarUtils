#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace lilAvatarUtils
{
    internal struct TextureData
    {
        internal Dictionary<Material, MaterialData> mds;
        internal TextureType type;
        internal long memorySize;
        internal long vramSize;
        internal TextureFormat format;
        internal RenderTextureFormat rtformat;

        internal int maxTextureSize;
        internal TextureImporterCompression textureCompression;
        internal bool crunchedCompression;
        internal int compressionQuality;
        internal bool sRGBTexture;
        internal TextureImporterAlphaSource alphaSource;
        internal bool alphaIsTransparency;
        internal bool mipmapEnabled;
        internal bool streamingMipmaps;
        internal bool isReadable;
    }

    internal enum TextureType
    {
        Texture,
        Texture2D,
        Cubemap,
        Texture3D,
        Texture2DArray,
        CubemapArray,
        //SparseTexture,
        RenderTexture,
        CustomRenderTexture
    }

    internal struct MaterialData
    {
        internal HashSet<GameObject> gameObjects;
        internal Dictionary<AnimationClip, AnimationClipData> acds;
    }

    internal struct AnimationClipData
    {
        internal Dictionary<RuntimeAnimatorController, AnimatorData> ads;
    }

    internal struct AnimatorData
    {
        internal HashSet<GameObject> gameObjects;
    }
}
#endif