# Tar&Tulla — Unity Project

**Current milestone:** Milestone 1C — Vertical Climb Loop (playable prototype)

Tar&Tulla is a mobile 2D physics-based vertical platform game about two connected jumpers tied by an elastic rope.

## Architecture Layers

### Framework (`Scripts/Framework/`)

Reusable, game-agnostic tuning infrastructure for similar Unity 2D mobile physics games.

```
Framework/
├── Tuning/GameplayProfileBase.cs    Abstract ScriptableObject profile
├── Runtime/GameRuntime.cs             Abstract runtime profile coordinator
└── Debug/DebugTuningLogger.cs         Consistent tuning logs
```

The Framework layer contains **no** Tar&Tulla jump, rope, platform, tilt, or camera parameters.

### Game (`Scripts/Game/TarTulla/`)

Tar&Tulla-specific configuration:

```
Game/TarTulla/
├── TarTullaGameplayProfile.cs   Full gameplay tuning profile
└── TarTullaRuntime.cs         Active profile coordinator (singleton)
```

### Legacy Settings (still supported)

Individual assets remain as fallbacks when no gameplay profile is active:

- `Settings/CharacterSettings_Default.asset`
- `Settings/RopeSettings_Default.asset`
- `Settings/AirControlSettings_Default.asset`

Gameplay systems read `TarTullaRuntime.Instance.Profile` first, then fall back to these assets.

## Gameplay Profiles

Profiles live in `Settings/GameplayProfiles/`.

| Profile | Purpose |
|-------|---------|
| `TarTulla_DefaultPrototype` | Baseline — close to current working prototype |
| `TarTulla_StrongSling` | Shorter rope, stronger spring, more energetic sling |
| `TarTulla_SoftRope` | Longer rope, softer spring, relaxed feel |
| `TarTulla_ExtremeTest` | Extreme values — use to verify profile wiring |

Each profile groups tuning into:

- **Character** — jump, gravity, landing
- **Rope** — length, spring, pull assist
- **Tilt** — air control
- **Camera** — follow behavior
- **Platforms** — procedural layout
- **RunRules** — fail distance, reset delay

Profile field names stay in **English** (for code and assets). Unity Inspector **tooltips are in Serbian** to make tuning faster — hover any field in a `TarTullaGameplayProfile` asset to read it.

### Active Profile Assignment

Open scene `TarTulla_Prototype.unity` → **Hierarchy** → `GameRoot` → `Systems` → Inspector → **Tar Tulla Runtime** → **Startup Profile**.

Profile assets live in Project: `Settings/GameplayProfiles/`.

Current default: `TarTulla_DefaultPrototype`.

### How to Create a New Profile

1. Right-click in `Settings/GameplayProfiles/`
2. **Create → Tar&Tulla → Gameplay Profile**
3. Fill metadata (`profileId`, `displayName`, `description`)
4. Tune nested groups
5. Assign on `TarTullaRuntime` or call `ApplyProfile()` at runtime

### How to Test Profile Switching

1. Open `TarTulla_Prototype.unity`
2. Select `Systems → TarTullaRuntime`
3. Change **Startup Profile** asset
4. Enter Play Mode (or use context menu **Reapply Active Profile**)
5. Press **R** in Editor to restart the run with new tuning

Expected feel:

- **DefaultPrototype** — current baseline
- **StrongSling** — snappier rope, faster sling recovery
- **SoftRope** — looser pair movement, less chaotic oscillation

---

## Milestone 1C — Vertical Climb Loop

### Status: Playable

The prototype implements a full vertical climb loop:

| System | Role |
|--------|------|
| `ClimbProgressTracker` | Tracks midpoint height; exposes `CurrentHeight`, `BestHeight`, `HighestReachedY` |
| `PrototypeRunController` | Starts run, detects fail, resets cleanly |
| `PrototypeLevelBuilder` | Clears and rebuilds procedural vertical path |
| `VerticalCameraFollow2D` | Upward-only camera follow with optional small downward correction |
| `TarTullaRuntime` | Supplies profile-driven tuning to all systems |

### Scene Hierarchy

```
GameRoot
├── Systems
│   ├── TarTullaRuntime          (active gameplay profile)
│   ├── PrototypeLevelBuilder
│   ├── ClimbProgressTracker
│   └── PrototypeRunController
├── LevelRoot                      (generated platforms)
├── CharactersRoot                 (generated Tar & Tulla)
└── UIRoot                         (empty — no UI yet)

Main Camera                        (VerticalCameraFollow2D)
```

### What Happens on Play

1. `TarTullaRuntime` applies the startup profile.
2. `PrototypeRunController` clears old content and builds the layout.
3. Tar and Tulla spawn above the first platform.
4. Auto-jump and rope physics begin.
5. Camera follows upward progress.
6. If **both** jumpers fall below `HighestReachedY - fallDistanceLimit`, the run resets.

### Profile Values That Affect the Climb Loop

| Profile group | Affects |
|---------------|---------|
| **Platforms** | Path length, spacing, width, layout seed, start height |
| **RunRules** | `fallDistanceLimit`, `resetDelay` |
| **Camera** | Follow smoothness, vertical offset, downward correction |
| **Character** | Jump height, gravity, landing forgiveness |
| **Rope** | Sling feel, pair coupling |
| **Tilt** | Air control while climbing |

> **Do not hardcode tuning constants** in gameplay scripts. Edit `TarTullaGameplayProfile` assets or legacy settings instead.

---

## How to Test the Climb Loop

### Editor

1. Open `Scenes/TarTulla_Prototype.unity`
2. Set Game View to **9:16** portrait aspect
3. Press **Play**
4. Tar and Tulla should land on the first platform and begin climbing
5. Use **A/D** or arrow keys for tilt air control (Editor keyboard fallback)

### Editor Debug Keys

| Key | Action |
|-----|--------|
| **R** | Full run reset — clears content, rebuilds layout, resets progress and camera |
| **B** | Rebuild layout only — same as R but without full run restart semantics |

### Physical Device

1. Build to iOS or Android with portrait orientation locked
2. Launch the prototype scene build
3. Tilt the device left/right while airborne to steer
4. No on-screen buttons required
5. Let both jumpers fall far below progress to verify automatic reset

### Verify Profile-Driven Tuning

1. Change `Platforms.platformCount` or `RunRules.fallDistanceLimit` in a profile asset
2. Press **R** in Editor to rebuild with new values
3. Confirm path length or fail distance changed without code edits

---

## Tuning Rules

- Prefer editing gameplay profiles or legacy settings assets.
- Validate profiles via `ValidateProfile()` — invalid values are clamped/warned.
- Press **R** after profile changes that affect layout or gravity (`Platforms`, `Character.gravityScale`).

## First Parameters to Tune

| Group | Start here |
|-------|------------|
| Platforms | `platformCount`, `verticalSpacingMin/Max`, `seed` |
| RunRules | `fallDistanceLimit` |
| Character | `jumpForce`, `gravityScale` |
| Rope | `restLength`, `springStrength`, `pullAssistStrength` |
| Tilt | `tiltSensitivity`, `airAcceleration` |
| Camera | `verticalOffset`, `smoothTime` |

## Profile Troubleshooting

### Assign the active profile

**Important:** `TarTullaRuntime` is not a folder in Project. It is a **component on a GameObject inside the scene**.

1. Open the scene: `Assets/_Project/Scenes/TarTulla_Prototype.unity`
2. In the **Hierarchy** window (not Project), expand:
   ```
   TarTulla_Prototype
   └── GameRoot
       └── Systems
   ```
3. Click **Systems**
4. In the **Inspector**, find the component **Tar Tulla Runtime** (script: `TarTullaRuntime.cs`)
5. Set **Startup Profile** to a profile from `Assets/_Project/Settings/GameplayProfiles/`
6. **Stop Play Mode** before switching profiles (or use context menu **Reapply Active Profile** during play)
7. Press **Play**, then **R** to rebuild with the new profile

**Project window (assets only):**
- Script: `Assets/_Project/Scripts/Game/TarTulla/TarTullaRuntime.cs`
- Profiles: `Assets/_Project/Settings/GameplayProfiles/*.asset`

You assign the profile on the **Systems** object in the scene, not on the `.cs` file.

> Changing a profile asset field during Play Mode does not save unless you manually save the asset afterward.

### Confirm the profile is loaded

On Play, the Console should show:

```
[Tuning] Active profile: Default Prototype (tar_tulla_default_prototype) v1
[Tuning] Profile applied: Default Prototype (tar_tulla_default_prototype)
[Tar&Tulla][Runtime] Active profile: TarTulla_DefaultPrototype (Default Prototype)
[Tar&Tulla][Runtime] jumpForce=20.14, gravityScale=3, ropeRestLength=3, springStrength=50, ...
```

If you see `[Tar&Tulla][Runtime] No active TarTullaGameplayProfile assigned`, the Startup Profile field is empty or the wrong asset type is assigned.

### Confirm each system reads the profile

| Log prefix | Confirms |
|------------|----------|
| `[Tar&Tulla][Runtime]` | Profile loaded with key values |
| `[Tar&Tulla][Builder] Using ... platformCount=` | Platform generation from profile |
| `[Tar&Tulla][Tar] Character tuning:` | Jumper character values + source profile name |
| `[Tar&Tulla][Rope] Rope tuning:` | Rope physics values + source profile name |

Each gameplay log includes `source=TarTulla_<ProfileName>` when the profile is active, or `source=CharacterSettings_Default` / `fallback` when not.

### If all profiles feel the same

1. Check **Startup Profile** on `TarTullaRuntime` — editing a profile asset alone does not switch the active profile
2. Confirm runtime logs show different `ropeRestLength` / `springStrength` per profile
3. Press **R** after switching — layout values (`platformCount`, `seed`) only apply on rebuild
4. Test `TarTulla_ExtremeTest` — very short rope (0.6), high spring (250), 10 platforms; should look obviously different
5. Ensure `logProfileSnapshotOnAwake` is enabled on `TarTullaRuntime`
6. Ensure `logResolvedProfileValuesOnStart` is enabled on jumpers and rope

## One-Way Platforms

Prototype platforms use **PlatformEffector2D** (Doodle Jump style):

| Setting | Value |
|---------|-------|
| `BoxCollider2D.usedByEffector` | `true` |
| `PlatformEffector2D.useOneWay` | `true` |
| `PlatformEffector2D.surfaceArc` | `140` |
| `PlatformEffector2D.useSideFriction` | `false` |
| `PlatformEffector2D.useSideBounce` | `false` |
| `platformHeight` (profile) | `0.3` (thin collider) |

**Behavior:**
- Jumpers pass through platforms when moving **upward**
- Landing / auto-jump triggers only when **falling** onto the top (`contact.normal.y > 0.45`)
- Side blocking is minimized by narrow height + no side friction

**If jumpers still get blocked:**
1. Press **R** to rebuild platforms with current settings
2. Confirm `BasicPlatform` and `PlatformEffector2D` exist on generated platforms
3. Confirm `BoxCollider2D.usedByEffector` is checked
4. Lower `platformHeight` in profile (try `0.25`)
5. Increase `surfaceArc` on `BasicPlatform` (try `160`)
6. Check Console for `Landing rejected: moving upward` vs `Landing accepted`

## Design Documentation

See `/GameDesign/` at the repository root.
