lilAvatarUtils
====

## Install

Please install by **one** of the following methods.

### VPM

Add my [repos](https://github.com/lilxyzw/vpm-repos) and add `jp.lilxyzw.avatar-utils` to your project.

### UPM

Import the following Git URL in UPM.

```
https://github.com/lilxyzw/lilAvatarUtils.git
```

## Window

Opening a window from `Window/_lil/AvatarUtils`, and set the avatar to display assets.

|Function|Operation|
|-|-|
|Sort|Left click labels|
|Toggle visibility|Right click labels|
|Resize|Left click the edge of labels|
|Search|Changing the fields below the labels|
|Filter|Checking the leftmost check box|

Asset properties can sometimes be edited. After changing properties, you can press `Apply` to apply the change, or `Revert` to revert to the previous value.

### Textures

Textures used in the avatar. Open the foldout to see materials that reference textures, press `Remove references` to remove all references.

### Materials

Materials used by the avatar. After changing materials, you can press `Apply` to replace materials. Open the foldout to see the `Renderer`s and `AnimationClip`s that reference textures.

### Animations

Animations used by the avatar.

### Renderers

Renderers in the avatar.

### PhysBones

PhysBones in the avatar.

### PBColliders

PhysBoneColliders in the avatar.

### Lighting

This tab is for checking avatar lighting. You can also see what it looks like when the shader is blocked by VRChat's safety.

### Utils

|Name|Description|
|-|-|
|Clean up Materials|Remove unused properties and shader keywords from materials|
|Clean up AnimatorControllers|Remove unused sub assets from AnimatorControllers|
|Remove Missing Components|Remove components that have no scripts|