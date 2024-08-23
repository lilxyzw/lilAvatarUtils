using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace lilAvatarUtils
{
    internal class TextureData
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

    internal class MaterialData
    {
        internal HashSet<GameObject> gameObjects;
        internal Dictionary<AnimationClip, AnimationClipData> acds;
    }

    internal class AnimationClipData
    {
        internal Dictionary<RuntimeAnimatorController, AnimatorData> ads;
        internal bool hasHumanoid = false;
        internal bool hasBlendShape = false;
        internal bool hasToggleActive = false;
        internal bool hasToggleEnabled = false;
        internal bool hasTransform = false;
        internal bool hasMaterialReplace = false;
        internal bool hasMaterialProperty = false;
        internal bool hasOther = false;
    }

    internal class AnimatorData
    {
        internal HashSet<GameObject> gameObjects;
        internal HashSet<(AnimatorState,AnimatorControllerLayer)> states;
    }
}
