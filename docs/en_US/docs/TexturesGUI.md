# Textures

This is a list of all textures included in the avatar. Some properties can be edited, and the changed properties will be applied all at once by pressing the Apply button in the upper left.

![TexturesGUI](/images/en_US/TexturesGUI.png "TexturesGUI")
## How to use

This tool is useful for identifying unnecessary textures or textures with excessive resolution to reduce the size of your avatar. For example, there are cases where the texture before modification remains on the material's luminescence or outline, which wastes VRAM, but with this tool you can replace the corresponding texture with the modified texture all at once. You can also efficiently reduce VRAM size by sorting the textures by VRAM capacity and changing the import settings.

## Property

|Name|Description|
|-|-|
|Name|Asset name. Clicking this will select the corresponding asset in the Project window.|
|Replace|If you specify a different texture here, you can replace that texture for all materials present on the avatar at once.|
|Type|The type of texture. Generally, Texture2D is used.|
|VRAM Size|The amount of VRAM used when textures are loaded. It is desirable to adjust this value so that it is as small as possible.|
|Resolution|The texture resolution on Unity. The smaller the resolution, the smaller the VRAM size, so it is recommended to set it as small as possible without noticeable artifacts.|
|Max Resolution|The maximum vertical or horizontal size of the texture resolution. The image will be scaled down on import to be smaller than this value while preserving as much of the image's aspect ratio as possible. The resolution of the original image file is not changed, so you can revert it to a larger setting.|
|Compression|This sets the quality of the texture after compression. The higher the setting, the clearer the texture will be at the expense of the compression rate. However, if the texture contains transparency, it will be clearer without any change in VRAM size due to the compression format.|
|Format|The texture format. This varies depending on whether transparency is enabled and the compression settings.|
|Crunch Compression|Whether or not to use crunch compression. For DXT or ETC formats only, crunch compression can reduce file size at the expense of image quality. However, the VRAM size does not change, so the load does not change. Also, even if you do not use crunch compression, compression is applied to the entire avatar data, so the change is not as large as it seems. If you want to reduce the avatar size, it is more effective to lower the texture resolution.|
|Compression Quality|Texture quality after crunch compression. The higher the setting, the more beautiful the texture will be at the expense of compression rate.|
|sRGB|This setting determines whether to apply inverse gamma correction to textures. Generally, textures with color information (such as albedo and emission) are set to sRGB, and textures with numerical information (such as smoothness and masks) are set to non-sRGB. Please set it appropriately, as the appearance will change depending on whether the project's color space is Linear or Gamma (depending on the avatar's display environment).|
|Alpha Source|This is the source from which Unity generates the alpha channel of a texture.|
|Alpha Is Transparency|Extends the color channels of transparent textures outward to avoid blackening of transparent areas. Set this to off if you want to use the color channels as is.|
|MipMap|Whether to generate mipmaps. It is generally recommended to turn it off, but if the texture is used in the vertex shader (such as for masking outlines), turning it off can reduce VRAM size by 33%.|
|Mip Streaming|Whether to enable Mip Streaming. This reduces VRAM consumption by loading only the mipmaps (reduced textures) required according to the camera position.|
|Read/Write|This setting allows scripts to access textures. Copying textures for script access doubles the RAM consumption, so it is recommended to turn this setting off if not required.|

