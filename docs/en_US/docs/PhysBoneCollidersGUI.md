# PBColliders

This is a list of all PhysBone Colliders included in the avatar. Some properties can be edited, and the changed properties will be applied all at once by pressing the Apply button in the upper left.

![PhysBoneCollidersGUI](/images/en_US/PhysBoneCollidersGUI.png "PhysBoneCollidersGUI")
## How to use

This helps to identify the source of collider references and eliminate unnecessary colliders that are not referenced anywhere.

## Property

|Name|Description|
|-|-|
|Name|Object name. Clicking this will select the corresponding object in the Hierarchy window.|
|Root Transform|The Transform used to calculate the collider position.|
|References|The number of PhysBones referencing this collider.|
|Shape|The shape of the collider.|
|Radius|The radius of the collider.|
|Height|The height of the collider.|
|Position|The offset of the collider's position from the root transform.|
|Rotation|The amount of offset of the collider's rotation from the root bone.|
|Inside|Turning this on will act to push the PhysBone inside the collider.|
|As Sphere|When this is turned on, the shape of the collision detection for the PhysBone itself will be calculated as a sphere instead of a capsule.|

