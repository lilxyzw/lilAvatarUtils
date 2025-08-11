# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.1.2] - 2025-08-11

### Added
- zh-hans localization

### Fixed
- Editor errors may occur when using some file systems on macOS

## [2.1.1] - 2025-08-02

### Fixed
- Material parent textures are not excluded if they are overridden

## [2.1.0] - 2025-07-27

### Added
- Supports Toon Standard fallback

### Changed
- Match the current VRChat specifications, which do not allow Unlit Shaders to be cut off

### Fixed
- Scanning material parents
- Overwrite the render queue of the replaced material

## [2.0.0] - 2025-01-28

### Added
- Supports localization

### Changed
- Move the menu path to `Tools/lilAvatarUtils`
- Automatically update avatar information when changes are applied
- The window will remain even after restarting Unity
- Change namespace
- Changes to avatar analysis method

### Fixed
- Error when sorting renderers with no meshes

## [1.3.0] - 2024-09-30

### Added
- PBColliders tab
- Texture replace

### Changed
- End of support for Unity 2019

### Fixed
- Wrong shader name in material tab

## [1.2.0] - 2024-08-24

### Added
- Animation tab

### Fixed
- Clear DisableShaderPasses from shader fallback materials by whiteflare (#17)
- wrong display name for sub assets by ReinaS-64892 (#21)
- light and gameobject positions are always centered at the origin by Tliks (#22)
- VRAM size calculation for RenderTexture (#18)
- `Clean up AnimatorControllers` button will empty any `Animator Override Controller` (#23)

## [1.1.0] - 2023-12-31

### Added
- Support for ModularAvatarMergeAnimator  by nekobako (#9)
- Show asset extension (#8)

### Fixed
- Support for material variant by nekobako (#11)
- Ambient light in the Lighting window does not work in unity 2022 (#13)

## [1.0.3] - 2023-04-01

### Fixed
- Error due to property type change in PhysBones (#7)

## [1.0.2] - 2023-03-27

### Fixed
- Incorrect render queue for fallback material (#5)
- `Remove Missing Components` ignores inactive transforms (#6)

## [1.0.1] - 2023-03-17

### Fixed
- Stencil is not enabled in the lighting tab by nekobako (#1)
- `Remove Missing Components` not working with child objects (#3)

## [1.0.0] - 2023-03-13