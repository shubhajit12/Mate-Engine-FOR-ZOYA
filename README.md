# Zoya Mate Engine

A lightweight desktop companion engine customized specifically for the **ZOYA** project.

---

## About the Project

**Zoya Mate Engine** is the desktop companion runtime used by **ZOYA**.

It provides the desktop-side avatar experience, including:

- Custom VRM avatar support
- Desktop and taskbar interactions
- Idle and dragging animations
- Head, spine, eye and hand movement
- Touch reactions
- Expression and animation systems
- FPS and performance controls
- Always-on-top and companion display options
- AI companion integration support
- Customizable desktop UI

The engine is customized around **ZOYA** and **Carlotta.vrm** as the production companion avatar.

![Zoya Mate Engine Preview](https://i.imgur.com/5cHHH8c.jpeg)

---

## Feature Overview

| Feature | Zoya Mate Engine |
|--------------------------|:----------------:|
| Custom VRM | ✅ |
| Window Sitting | ✅ |
| Taskbar Sitting | ✅ |
| Idle Animation | ✅ |
| Dragging Animation | ✅ |
| Dance to Music | ✅ |
| Head Tracking | ✅ |
| Spine Tracking | ✅ |
| Eye Tracking | ✅ |
| Hand Movement | ✅ |
| Alarm / Timer | ✅ |
| Touch Reactions | ✅ |
| Avatar SFX | ✅ |
| Particle Effects | ✅ |
| FPS Control | ✅ |
| Always On Top Toggle | ✅ |
| Chibi Mode | ✅ |
| Post Processing | ✅ |
| System Icon | ✅ |
| Smooth Animation Transitions | ✅ |
| AI Chat Integration | ✅ |
| Advanced AI Functions | ✅ |
| AI API Functions | ✅ |
| Animation Modding | ✅ |
| Sleep | ✅ |
| Game Compatibility | ✅ |
| Expression Based on Movement | ✅ |
| Inverse Kinematics | ✅ |
| Menu Customizations | ✅ |
| Debugging Menu | ✅ |
| Multiple Avatar Support | ✅ |
| Sync Dances | ✅ |
| Minecraft Integration | ✅ |
| Food System | ✅ |

---

## Smoother Transitions

![Zoya Mate Engine Preview](https://i.imgur.com/qS894h9.gif)

The engine provides smooth animation transitions so the desktop companion can move naturally between different states.

---

## Performance

<img width="1025" height="945" alt="Zoya Mate Engine performance" src="https://github.com/user-attachments/assets/e09368b2-21e2-49e5-a965-774f8007c895" />

**Zoya Mate Engine** is designed to remain lightweight while running as a desktop companion. Actual memory usage depends on the avatar and its texture sizes.

---

## How to Use

1. Build or obtain the Windows companion runtime.
2. Make sure the executable is kept together with its complete Unity data directory and required runtime files.
3. Run `MateEngineX.exe`.
4. Right-click the avatar or use the configured companion controls to open the available settings.

For normal ZOYA use, the companion engine is launched and managed by the main **ZOYA** application.

---

## Developer Guide

Want to contribute to the ZOYA companion engine?

1. Clone this repository.
2. Open **Unity Hub** → **Add Project From Disk**.
3. Select the project folder.
4. Open the Unity project using the version specified in `ProjectSettings/ProjectVersion.txt`.
5. Open the companion scene used by the ZOYA build.
6. Build the Windows player when testing a standalone companion runtime.

> ⚠️ Keep the generated Windows executable together with its complete Unity data directory. Do not copy only the `MateEngineX.exe` file.

---

## ZOYA Customization

The runtime has been customized for **ZOYA**, including:

- **Carlotta.vrm** as the production companion avatar
- ZOYA dark/amber UI styling
- English-only user interface
- ZOYA-specific desktop companion integration
- Removal of unrelated Steam promotion and community features
- Removal of Discord Rich Presence integration
- Runtime configuration controlled by ZOYA where applicable

The internal executable name **MateEngineX.exe** may remain for compatibility with the existing runtime and build pipeline.

---

## Licensing and Third-Party Notices

The repository retains the applicable license and third-party notice files for the source and included dependencies.

Please read `LICENSE.md` and `NOTICE.txt` before redistribution.
