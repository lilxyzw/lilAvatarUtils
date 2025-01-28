# Materials

This is a list of all materials included in the avatar. Some properties can be edited, and the changed properties will be applied all at once by pressing the Apply button in the upper left.

![MaterialsGUI](/images/en_US/MaterialsGUI.png "MaterialsGUI")
## How to use

You can identify materials you forgot to replace and replace them all at once, and check the shader type and render queue to help solve avatar rendering problems. It can also be used as a tool to replace the original avatar materials all at once.

## Property

|Name|Description|
|-|-|
|Name|Asset name. Clicking this will select the corresponding asset in the Project window.|
|Replace|By specifying a different material here, you can replace all materials currently present on the avatar at once.|
|Shader|The shader that the material is using.|
|Render Queue|The rendering priority of the material. Smaller values ​​are rendered first. If a material that includes transparency is set to less than 2500, rendering problems may occur when it overlaps with the skybox. If it is set to 2501 or more, the lens effect will cause the material to lose focus and it will not be able to receive shadows. If a transparent material is set to an excessively low value (such as 2450 or less), it is very likely to cause problems with other materials being erased.|

