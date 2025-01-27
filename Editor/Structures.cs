using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.avatarutils
{
    internal class TextureData
    {
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

    internal class AnimationClipData
    {
        internal bool hasHumanoid = false;
        internal bool hasBlendShape = false;
        internal bool hasToggleActive = false;
        internal bool hasToggleEnabled = false;
        internal bool hasTransform = false;
        internal bool hasMaterialReplace = false;
        internal bool hasMaterialProperty = false;
        internal bool hasOther = false;
    }
}
