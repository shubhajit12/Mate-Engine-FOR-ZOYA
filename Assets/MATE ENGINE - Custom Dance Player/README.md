# ZOYA Custom Dance Player

Technical documentation for the custom dance-player components used by the ZOYA desktop companion.

## Structure

- **Core** — avatar and dance playback logic
- **Prefab** — dance-player prefabs and supporting assets
- **Utils** — camera, window, blendshape, drag, and helper components

## Core components

- **DanceAvatarHelper.cs** — avatar/Animator discovery and dance facial-animation support.
- **DancePlayerCore.cs** — dance playback and audio/animation synchronization.
- **DancePlayerUIManager.cs** — dance-player UI visibility and controls.
- **DanceResourceManager.cs** — dance resource management.
- **DanceSettingsHandler.cs** — dance-player settings persistence.

## Utility components

- **DanceCameraDistKeeper.cs** — keeps the dance camera at a stable distance.
- **DanceShadowFollower.cs** — keeps the character shadow aligned during movement.
- **DanceWindowFollower.cs** — follows large character movements.
- **DummyToUniversalSync.cs** — synchronizes compatible facial blendshapes.
- **HipsFollower.cs** — follows avatar hip movement.
- **UIDragHandler.cs** — handles UI dragging.
- **SMRHandle.cs** — manages renderer behavior after avatar changes.
- **GlobalHotkeyListener.cs** — handles the configured dance playback hotkey.

## Dance file format

A dance AssetBundle can contain:

| Resource | Purpose |
| --- | --- |
| Animator Controller | Controls the dance animation state |
| Audio | Synchronized music/audio |
| Animation | Character movement data |

For facial compatibility, dance animations should use standard MMD-style facial blendshape conventions where required by the imported animation.

For camera animation, use the expected Camera_root/Camera_root_1/Camera hierarchy used by the existing dance-player implementation.

## Scope

This documentation describes the components retained in the ZOYA companion engine. Original project promotion, community links, donation information, and unrelated branding have been removed.
