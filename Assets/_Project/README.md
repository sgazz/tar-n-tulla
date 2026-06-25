# Tar&Tulla — Unity Project

**Current milestone:** Milestone 1C — Playable Vertical Climb Loop

Tar&Tulla is a mobile 2D physics-based vertical platform game about two connected jumpers tied by an elastic rope.

## Milestone 1C — What Works

- Two connected jumpers (Tar & Tulla) with elastic rope physics
- Auto-jump on landing
- Mobile tilt air control (keyboard fallback in Editor)
- Procedural vertical platform path (~24 platforms)
- Camera follows upward climb progress
- Fail and reset when both jumpers fall too far below reached height

## Folder Structure

```
_Project/
├── Art/              Visual assets (characters, platforms, rope, backgrounds, UI)
├── Audio/            Music and SFX
├── Materials/        Shared materials
├── Prefabs/          Reusable scene objects
├── Scenes/           Unity scenes (start with TarTulla_Prototype)
├── Scripts/          Game code organized by domain
│   ├── Core/         Bootstrap, run controller, progress, level builder
│   ├── Characters/   Tar, Tulla, movement, air control
│   ├── Rope/         Elastic rope physics
│   ├── Platforms/    Platform types and layout
│   ├── Camera/       Vertical follow camera
│   ├── Input/        Touch and tilt input
│   └── UI/           Menus and HUD (not used yet)
├── ScriptableObjects/ Tunable data assets
├── Settings/          Project-specific config assets
└── Tests/             Play Mode and Edit Mode tests
```

## How to Test the Climb Loop

### Editor

1. Open `Scenes/TarTulla_Prototype.unity`
2. Set Game View to **9:16** portrait aspect
3. Press **Play**
4. Tar and Tulla fall onto the first platform and begin auto-jumping upward
5. Use **A / D** or **arrow keys** for tilt air control while airborne
6. Camera follows upward; falling far below progress resets the run

### Editor Debug Keys

| Key | Action |
|-----|--------|
| **R** | Reset run (rebuild layout, reset progress and camera) |
| **B** | Rebuild prototype layout only |

### Physical Device

1. Build to iOS or Android (portrait locked)
2. Hold phone upright in portrait
3. Tilt left/right while jumpers are airborne
4. Let both fall far below climbed height to verify automatic reset
5. No on-screen buttons required

## First Tuning Parameters

| System | Parameter | Start here |
|--------|-----------|------------|
| Run | `fallDistanceLimit` (PrototypeRunController) | 12–16 |
| Level | `platformCount`, `verticalSpacingMin/Max` | 24, 2.4–3.2 |
| Level | `horizontalRange`, `generationSeed` | 2.0, 1337 |
| Camera | `verticalOffset`, `smoothTime` | 1.5, 0.25 |
| Camera | `maxDownwardCorrection` | 1.0–2.0 |
| Air control | `tiltSensitivity`, `airAcceleration` | See AirControlSettings_Default |

## Development Rules

1. **Modular and testable systems.** Each system owns a clear responsibility.
2. **Physical device testing is required** for tilt feel and frame pacing.
3. **No final UI, art, sound, or scoring** until later milestones.

## Entry Scene

`Scenes/TarTulla_Prototype.unity` — prototype climb loop wired through `GameBootstrap`, `PrototypeRunController`, and `PrototypeLevelBuilder`.

## Design Documentation

See `/GameDesign/` at the repository root for vision, mechanics, milestones, and tuning notes.
