# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.3.0] - 2024-09-30

## Added
- PBColliders tab
- Texture replace

### Changed
- End of support for Unity 2019

### Fixed
- Wrong shader name in material tab

## [1.2.0] - 2024-08-24

## Added
- Animation tab

### Fixed
- Clear DisableShaderPasses from shader fallback materials by whiteflare (#17)
- wrong display name for sub assets by ReinaS-64892 (#21)
- light and gameobject positions are always centered at the origin by Tliks (#22)
- VRAM size calculation for RenderTexture (#18)
- `Clean up AnimatorControllers` button will empty any `Animator Override Controller` (#23)

## [1.1.0] - 2023-12-31

## Added
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