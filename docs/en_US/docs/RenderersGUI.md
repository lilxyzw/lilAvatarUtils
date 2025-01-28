# Renderers

This is a list of all renderers included in the avatar. Some properties can be edited, and the changed properties will be applied all at once by pressing the Apply button in the upper left.

![RenderersGUI](/images/en_US/RenderersGUI.png "RenderersGUI")
## How to use

You can check the difference in anchor overrides and root bone settings to resolve avatar lighting issues, or identify meshes with high polygon counts and material slots to help make your avatar lighter.

## Property

|Name|Description|
|-|-|
|Name|Object name. Clicking this will select the corresponding object in the Hierarchy window.|
|Anchor Override|This is the reference point when calculating lighting. It is recommended to set it the same for all meshes in one avatar, as it can cause different brightnesses for each mesh. It is used to calculate light probes and reflection probes.|
|Root Bone|The bone or transform that is the origin of the mesh. Bounds move according to this transform. It may also affect shader calculations.|
|Bounds|This is the volume used to determine if a mesh is outside the range of the screen or lights. If the mesh does not fit within this range, the mesh that should be in the screen may suddenly disappear or the light may suddenly become dark, so you need to set it appropriately.|
|BlendShapes|The number of BlendShapes for the mesh. The larger the number, the larger the mesh size will be.|
|Materials|The number of material slots for the mesh. The larger this number is, the larger the rendering load will be.|
|Polys|The number of polygons in the mesh. The larger this number is, the higher the load on the geometry shader and tessellation will be.|
|Vertices|The number of vertices in the mesh. The larger this number is, the higher the load on the vertex shader and deformation by bones and blendshapes will be.|
|Quality|This is the number of bones that can operate one vertex. The larger the number, the more calculations will be required, which may result in poorer performance. Basically, this is set to Auto, and if you want to limit it, it is recommended to change the model import settings.|
|Update When Offscreen|If this is on, bounds will be calculated even if the mesh is off screen. This setting is off by default to reduce the load.|
|Cast Shadows|Whether the mesh casts shadows on other objects.|
|Receive Shadows|Whether the mesh receives shadows from other objects.|
|Light Probes|How to calculate light probes.|
|Reflection Probes|How to calculate reflection probes.|
|Motion Vector|Whether to output motion vectors, used for post-processing such as motion blur.|
|Dynamic Occlusion|If the mesh is hidden by other static objects, culling will be performed and rendering will be skipped. It is usually recommended to turn it on for better performance, but turn it off if you want to use special effects such as displaying objects hidden by walls.|

