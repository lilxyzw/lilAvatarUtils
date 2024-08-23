using UnityEngine;
#if UNITY_2022_3_OR_NEWER
using UnityEngine.Experimental.Rendering;
#endif

namespace lilAvatarUtils.Utils
{
    internal class MathHelper
    {
        // BitMasks
        internal static bool BitMask(int mask, bool val)
        {
            return (mask & (val ? 0x00000002 : 0x00000001)) != 0x00000000;
        }

        internal static bool BitMask(int mask, int enumVal)
        {
            int val = (int)Mathf.Pow(2,enumVal);
            return (mask & val) != 0x00000000;
        }

        // VRAM Size
        internal static long ComputeVRAMSize(Texture t, double bpp)
        {
            double pixelsBase = t.width * t.height;
            int dimension = 2;
            switch(t)
            {
                case Texture3D o     : pixelsBase *= o.depth; dimension = 3; break;
                case Texture2DArray o: pixelsBase *= o.depth; break;
                case Cubemap _       : pixelsBase *= 6; break;
                case CubemapArray o  : pixelsBase *= 6 * o.cubemapCount; break;
            }
            double pixels = 0;
            for(int i = 0; i < t.mipmapCount; i++)
            {
                pixels += pixelsBase / Mathf.Pow(Mathf.Pow(2,i),dimension);
            }
            return (long)(pixels * bpp / 8d);
        }

        internal static long ComputeVRAMSize(Texture t, TextureFormat format)
        {
            return ComputeVRAMSize(t, FormatToBPP(format, true));
        }

        internal static long ComputeVRAMSize(RenderTexture t, RenderTextureFormat format)
        {
            int aaScale = t.antiAliasing == 1 ? 1 : t.antiAliasing + 1;
            return ComputeVRAMSize(t, FormatToBPP(format, true) * aaScale + FormatToBPPDepthStencil(t) * t.antiAliasing);
        }

        internal static double FormatToBPP(TextureFormat format, bool isVRAM = false)
        {
            var formatName = format.ToString();
            if(formatName.Contains("ASTC"))
            {
                var sts = formatName.Split('x');
                if(sts.Length == 2 && int.TryParse(sts[1], out int size)) return 128.0d / size / size;
                return 0;
            }
            double bit;
            switch(format)
            { 
                case TextureFormat.Alpha8: bit = 8; break;
                case TextureFormat.ARGB4444: bit = 16; break;
                case TextureFormat.RGB24: bit = 24; break;
                case TextureFormat.RGBA32: bit = 32; break;
                case TextureFormat.ARGB32: bit = 32; break;
                case TextureFormat.RGB565: bit = 16; break;
                case TextureFormat.R16: bit = 16; break;
                case TextureFormat.DXT1: bit = 4; break;
                case TextureFormat.DXT5: bit = 8; break;
                case TextureFormat.RGBA4444: bit = 16; break;
                case TextureFormat.BGRA32: bit = 32; break;
                case TextureFormat.RHalf: bit = 16; break;
                case TextureFormat.RGHalf: bit = 32; break;
                case TextureFormat.RGBAHalf: bit = 64; break;
                case TextureFormat.RFloat: bit = 32; break;
                case TextureFormat.RGFloat: bit = 64; break;
                case TextureFormat.RGBAFloat: bit = 128; break;
                case TextureFormat.YUY2: bit = 16; break;
                case TextureFormat.RGB9e5Float: bit = 32; break;
                case TextureFormat.BC6H: bit = 8; break;
                case TextureFormat.BC7: bit = 8; break;
                case TextureFormat.BC4: bit = 4; break;
                case TextureFormat.BC5: bit = 8; break;
                case TextureFormat.DXT1Crunched: bit = 4; break;
                case TextureFormat.DXT5Crunched: bit = 8; break;
                case TextureFormat.PVRTC_RGB2: bit = 2; break;
                case TextureFormat.PVRTC_RGBA2: bit = 2; break;
                case TextureFormat.PVRTC_RGB4: bit = 4; break;
                case TextureFormat.PVRTC_RGBA4: bit = 4; break;
                case TextureFormat.ETC_RGB4: bit = 4; break;
                case TextureFormat.EAC_R: bit = 4; break;
                case TextureFormat.EAC_R_SIGNED: bit = 4; break;
                case TextureFormat.EAC_RG: bit = 8; break;
                case TextureFormat.EAC_RG_SIGNED: bit = 8; break;
                case TextureFormat.ETC2_RGB: bit = 4; break;
                case TextureFormat.ETC2_RGBA1: bit = 4; break;
                case TextureFormat.ETC2_RGBA8: bit = 8; break;
                case TextureFormat.RG16: bit = 16; break;
                case TextureFormat.R8: bit = 8; break;
                case TextureFormat.ETC_RGB4Crunched: bit = 4; break;
                case TextureFormat.ETC2_RGBA8Crunched: bit = 4; break;
                case TextureFormat.RG32: bit = 32; break;
                case TextureFormat.RGB48: bit = 48; break;
                case TextureFormat.RGBA64: bit = 64; break;
                default: return 0;
            }
            if(isVRAM)
            {
                for(int i = 1; i < bit; i *= 2)
                {
                    if(bit < i) bit = i;
                }
            }
            return bit;
        }

        internal static double FormatToBPP(RenderTextureFormat format, bool isVRAM = false)
        {
            double bit;
            switch(format)
            {
                case RenderTextureFormat.ARGB32: bit = 32; break;
                case RenderTextureFormat.Depth: bit = 0; break;
                case RenderTextureFormat.ARGBHalf: bit = 64; break;
                case RenderTextureFormat.Shadowmap: bit = 8; break; // TODO
                case RenderTextureFormat.RGB565: bit = 16; break;
                case RenderTextureFormat.ARGB4444: bit = 16; break;
                case RenderTextureFormat.ARGB1555: bit = 16; break;
                case RenderTextureFormat.Default: bit = 32; break; // TODO
                case RenderTextureFormat.ARGB2101010: bit = 32; break;
                case RenderTextureFormat.DefaultHDR: bit = 128; break; // TODO
                case RenderTextureFormat.ARGB64: bit = 64; break;
                case RenderTextureFormat.ARGBFloat: bit = 128; break;
                case RenderTextureFormat.RGFloat: bit = 64; break;
                case RenderTextureFormat.RGHalf: bit = 32; break;
                case RenderTextureFormat.RFloat: bit = 32; break;
                case RenderTextureFormat.RHalf: bit = 16; break;
                case RenderTextureFormat.R8: bit = 8; break;
                case RenderTextureFormat.ARGBInt: bit = 128; break;
                case RenderTextureFormat.RGInt: bit = 64; break;
                case RenderTextureFormat.RInt: bit = 32; break;
                case RenderTextureFormat.BGRA32: bit = 32; break;
                case RenderTextureFormat.RGB111110Float: bit = 32; break;
                case RenderTextureFormat.RG32: bit = 32; break;
                case RenderTextureFormat.RGBAUShort: bit = 64; break;
                case RenderTextureFormat.RG16: bit = 16; break;
                case RenderTextureFormat.BGRA10101010_XR: bit = 40; break;
                case RenderTextureFormat.BGR101010_XR: bit = 30; break;
                case RenderTextureFormat.R16: bit = 16; break;
                default: return 0;
            }
            return bit;
        }

        internal static long FormatToBPPDepthStencil(RenderTexture t)
        {
            #if UNITY_2022_3_OR_NEWER
            switch(t.depthStencilFormat)
            {
                case GraphicsFormat.D16_UNorm: return 16;
                case GraphicsFormat.D16_UNorm_S8_UInt: return 32;
                case GraphicsFormat.D24_UNorm: return 32;
                case GraphicsFormat.D24_UNorm_S8_UInt: return 32;
                case GraphicsFormat.D32_SFloat: return 32;
                case GraphicsFormat.D32_SFloat_S8_UInt: return 64;
                case GraphicsFormat.S8_UInt: return 8;
            }
            #endif
            return 0;
        }
    }
}
