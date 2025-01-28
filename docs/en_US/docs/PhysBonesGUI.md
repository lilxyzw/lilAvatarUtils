# PhysBones

This is a list of all PhysBones included in the avatar. Some properties can be edited, and the changed properties will be applied all at once by pressing the Apply button in the upper left.

![PhysBonesGUI](/images/en_US/PhysBonesGUI.png "PhysBonesGUI")
## How to use

It is useful for identifying PhysBone components that have a large impact on performance rank, and for discovering PhysBones that have similar root bones and settings and can be integrated.

## Property

|Name|Description|
|-|-|
|Name|Object name. Clicking this will select the corresponding object in the Hierarchy window.|
|Root Transform|This is the root of the bone or transform that performs PhysBone calculations. Motion is applied to the transforms under this.|
|Parent|Parent object of PhysBone. If the parent is the same, you may be able to reduce the number of components by unifying the components.|
|Multi Child Type|How to determine orientation when there are multiple child bones and the orientation of the parent bone cannot be determined.|
|Bones|The number of bones the sway is calculated for. The higher the number, the higher the cost.|
|Colliders|The number of colliders to calculate the collision detection with this PhysBone.|
|Collision|The number of collision calculations. This number increases according to the number of bones and colliders, and the higher the number, the greater the load.|
|Immobile Type|This is how to calculate Immobile.|
|Allow Collision|Whether or not to enable collisions with colliders other than those set by the component. It will collide with each player's hand.|
|Grabbing|Whether or not to be able to grab PhysBone.|
|Posing|Whether the PhysBone can be posed.|

