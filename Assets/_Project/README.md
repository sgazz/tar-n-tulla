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

**Tuning Lab** (`TuningLab/`): five Pass 1 presets — see [Game Feel Tuning Pass 1](#game-feel-tuning-pass-1).

Each profile groups tuning into:

- **Character** — jump, gravity, landing
- **Rope** — length, spring, pull assist
- **Tilt** — air control
- **Camera** — follow behavior
- **Platforms** — procedural layout
- **RunRules** — fail distance, reset delay
- **Feedback** — camera impulse, haptics, danger vignette, height milestones
- **Onboarding** — countdown, tutorial hints, first-run flags

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

## Game Feel Tuning Pass 1

Structured tuning lab for core motion feel on **physical device**. No new mechanics — profile presets + debug readability only.

### Location

```
Settings/GameplayProfiles/TuningLab/
├── TarTulla_Tuning_A_DefaultControl.asset   ← baseline for comparison
├── TarTulla_Tuning_B_TightRhythm.asset
├── TarTulla_Tuning_C_SoftFloat.asset
├── TarTulla_Tuning_D_StrongRescue.asset
├── TarTulla_Tuning_E_FastClimb.asset
└── TuningTestNotes.md                       ← device test score sheet
```

### Tuning profiles

| Profile | Purpose | Main levers |
|---------|---------|-------------|
| **A — Default Control** | Balanced baseline, same as working prototype | Reference for all tests |
| **B — Tight Rhythm** | Shorter rope, tighter gaps, rhythmic Tar/Tulla | ↑ jump/gravity, ↓ rope length, ↑ spring/damping, ↓ platform gaps, faster camera |
| **C — Soft Float** | Forgiving, relaxed, floaty | ↓ gravity, longer/softer rope, wider platforms, slower camera, ↑ air control |
| **D — Strong Rescue** | Visible partner save moments | ↑ pullAssist, ↑ rope feedback/haptics, fair platforms |
| **E — Fast Climb** | High vertical pace, advanced feel | ↑ jump/gravity/fall speed, ↑ tilt speed, larger gaps, faster camera |

**Baseline for Pass 1:** `TarTulla_Tuning_A_DefaultControl` — always compare B–E against this on device.

### Assign a tuning profile

1. Open `TarTulla_Prototype.unity`
2. **Hierarchy** → `GameRoot` → `Systems` → **Tar Tulla Runtime**
3. Set **Startup Profile** to a `TuningLab/` asset
4. Enter Play Mode or build to device
5. Start a new run (Main Menu → Start) so values apply from run prep

### Device testing workflow

1. Pick profile A first — play 2–3 minutes, note impressions in `TuningTestNotes.md`
2. Switch profile on `TarTullaRuntime`, restart app/run
3. Test B → C → D → E in order (or A between each for A/B comparison)
4. **Do not change values while testing** — observe only
5. Same device, portrait orientation, same grip every time

### Developer HUD (optional)

`UIRoot` → `UIManager` → enable **Show Developer Hud In Editor** (Editor / Development builds only).

Displays live:

- Active profile **displayName**
- `jumpForce`, `gravityScale`
- `restLength`, `springStrength`, `pullAssist`
- `tiltSensitivity`
- Active platform count, current height

If HUD values **do not change** when switching profiles, fix profile wiring before continuing tuning.

### Most important values (Pass 1)

| Group | Fields | Affects |
|-------|--------|---------|
| Character | `jumpForce`, `gravityScale`, `maxFallSpeed` | Arc height, pace, landing rhythm |
| Rope | `restLength`, `springStrength`, `pullAssistStrength`, `damping` | Pair tension, rescue, sling |
| Tilt | `tiltSensitivity`, `airAcceleration`, `maxHorizontalAirSpeed` | Air steering, device feel |
| Camera | `smoothTime`, `verticalOffset` | Readability vs responsiveness |
| Platforms | `verticalSpacingMin/Max`, `platformWidth`, `recoveryPlatformEvery` | Difficulty, fairness, rhythm |

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
└── UIRoot                         (UIManager + runtime UI canvas)

Main Camera                        (VerticalCameraFollow2D)
```

### What Happens on Play

1. `TarTullaRuntime` applies the startup profile.
2. `PrototypeUIHierarchyBuilder` creates Canvas, panels, and EventSystem under `UIRoot`.
3. `UIManager` shows the **Main Menu** (`Tap to Start`).
4. After start: `PrototypeRunController` builds the layout, spawns Tar & Tulla, and enters **Playing**.
5. Auto-jump and rope physics begin; HUD shows height and best.
6. If **both** jumpers fall below `HighestReachedY - fallDistanceLimit`, **Game Over** appears with height and best.

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
3. Press **Play** — Main Menu should appear first
4. Tap **Tap to Start** — gameplay begins with HUD
5. Use **A/D** or arrow keys for tilt air control (Editor keyboard fallback)
6. Use **II** pause button (top-right) to test Pause / Resume / Restart / Main Menu
7. Fall far below progress to trigger **Game Over** → Retry or Main Menu

### Editor Debug Keys

| Key | Action |
|-----|--------|
| **R** | Full run reset while Playing (bypasses UI flow) |
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

Prototype platforms use **PlatformEffector2D** (Doodle Jump style). Profile fields `useOneWayPlatforms` and `oneWaySurfaceArc` control this at generation time.

| Setting | Value (default) |
|---------|-----------------|
| `BoxCollider2D.usedByEffector` | `true` when one-way enabled |
| `PlatformEffector2D.useOneWay` | `true` |
| `PlatformEffector2D.surfaceArc` | `150` (from profile) |
| `PlatformEffector2D.useSideFriction` | `false` |
| `PlatformEffector2D.useSideBounce` | `false` |
| `platformHeight` (profile) | `0.28` (thin collider) |

**Behavior:**
- Jumpers pass through platforms when moving **upward**
- Landing / auto-jump triggers only when **falling** onto the top (`contact.normal.y > 0.45`)
- Side blocking is minimized by narrow height + no side friction

**If jumpers still get blocked:**
1. Press **R** to rebuild platforms with current settings
2. Confirm `BasicPlatform` and `PlatformEffector2D` exist on generated platforms
3. Confirm `BoxCollider2D.usedByEffector` is checked
4. Lower `platformHeight` in profile (try `0.25`)
5. Increase `oneWaySurfaceArc` (try `160`)
6. Check Console for `Landing rejected: moving upward` vs `Landing accepted`

## Platform Generation Tuning

Profile group: **Platforms** in `TarTullaGameplayProfile`.

| Area | Key fields | What they do |
|------|------------|--------------|
| **Easy start** | `easyStartPlatformCount`, `safeLandingWidthMultiplier` | First N platforms are closer, wider, easier to land on |
| **Vertical rhythm** | `verticalSpacingMin/Max`, `minVerticalGap`, `maxVerticalGap` | Controls jump height between platforms |
| **Horizontal rhythm** | `horizontalRange`, `maxHorizontalGap`, `horizontalDirectionChangeChance`, `forceAlternatingPattern` | Zig-zag path left/right |
| **Width variation** | `widthVariationEnabled`, `platformWidthMin/Max`, `narrowPlatformChance`, `wideRecoveryPlatformEvery` | Mix of narrow, normal, and recovery platforms |
| **One-way** | `useOneWayPlatforms`, `oneWaySurfaceArc` | Doodle Jump pass-through from below |

**Tune first if climb feels wrong:**
- Too easy → raise `verticalSpacingMax`, enable `widthVariationEnabled`, lower `safeLandingWidthMultiplier`
- Too hard → raise `easyStartPlatformCount`, lower `maxHorizontalGap`, widen `platformWidth`
- Blocked by platforms → lower `platformHeight`, raise `oneWaySurfaceArc`
- Too repetitive → raise `horizontalDirectionChangeChance` or enable `forceAlternatingPattern`

Press **R** after changing platform values to rebuild the layout.

## Procedural Platform Stream

Profile group: **Platforms → Proceduralni stream** in `TarTullaGameplayProfile`.

### Fixed vs procedural mode

| Mode | Field | Behavior |
|------|-------|----------|
| **Fixed** | `useProceduralGeneration = false` | Generates exactly `platformCount` platforms once at run start (legacy behavior). |
| **Procedural** | `useProceduralGeneration = true` | Generates `initialPlatformCount` at start, then streams new platforms as you climb. |

### How streaming works

- **`platformBufferAhead`** — Generator keeps platforms up to this height above the Tar/Tulla midpoint. Higher = more platforms ahead of the player.
- **`cleanupDistanceBelowCamera`** — Platforms below `cameraY - cleanupDistanceBelowCamera` are destroyed. Higher = platforms linger longer below the view.
- **`maxActivePlatforms`** — Hard cap on platform objects in the scene. Cleanup + cap prevent memory growth.
- **`generationSegmentHeight`** — How much vertical height is added per generation batch.

### Seed and randomization

- **`seed`** — Deterministic layout when `randomizeSeedOnRun` is off. Same seed = same platform sequence.
- **`randomizeSeedOnRun`** — New random seed each run (on **R** reset or fall reset). Use for variety; turn off for repeatable testing.

### Difficulty ramp

When `difficultyRampEnabled` is on, after `difficultyRampStartHeight` above `startY`:
- Vertical spacing gradually increases toward `maxVerticalSpacingAtHighDifficulty`
- Platform width gradually decreases toward `minPlatformWidthAtHighDifficulty`
- Rate controlled by `difficultyRampStrength`

### Recovery platforms

Every `recoveryPlatformEvery` platforms (0 = off), a wider platform is placed using `recoveryPlatformWidthMultiplier`.

### Debug logs

On `PrototypeLevelBuilder`, enable **Enable Stream Debug Logs** to see mode, seed, `highestGeneratedY`, placement, and cleanup counts in Console.

### Testing endless generation

1. Set `useProceduralGeneration = true` on the active profile.
2. Play and climb past the initial platforms.
3. New platforms should appear above you; climb should not end at a fixed count.
4. Optional: enable stream debug logs and watch `highestGeneratedY` grow.

### Testing cleanup

1. Climb high enough that platforms fall behind the camera.
2. In Hierarchy under `LevelRoot`, old platform count should stabilize (not grow forever).
3. With debug logs on, watch `Cleanup removed=N` messages.

### Switch back to fixed mode

Set `useProceduralGeneration = false` on the profile and press **R**. Only `platformCount` platforms are built.

### Tune first if layouts feel wrong

| Problem | Tune |
|---------|------|
| **Too easy** | Lower `platformBufferAhead` is not the fix — raise `verticalSpacingMax`, enable `difficultyRampEnabled`, increase `difficultyRampStrength` |
| **Too hard** | Raise `easyStartPlatformCount`, lower `maxVerticalSpacingAtHighDifficulty`, widen `platformWidth`, lower `recoveryPlatformEvery` for more recovery platforms |
| **Too random / unfair** | Lower `horizontalDirectionChangeChance`, enable `forceAlternatingPattern`, lower `maxHorizontalGap`, set fixed `seed`, disable `randomizeSeedOnRun` |
| **Platforms run out** | Raise `platformBufferAhead` or `maxActivePlatforms` |
| **Too many objects** | Lower `maxActivePlatforms` or `cleanupDistanceBelowCamera` |

## Portrait playfield bounds

Profile group: **Platforms → Granice igrališta (portrait)** in `TarTullaGameplayProfile`.

### Visible width formula

For an orthographic portrait camera:

- `visibleHeight = orthographicSize * 2`
- `visibleWidth = visibleHeight * aspect`
- `cameraHalfWidth = orthographicSize * aspect` (half of visible world width)

On 9:16 with `orthographicSize = 8`: `cameraHalfWidth ≈ 4.5` world units.

### How platform X is clamped

After all horizontal placement logic (zig-zag, gaps, recovery width), the generator computes:

1. `allowedHalfX = cameraHalfWidth - platformHalfWidth - screenHorizontalMargin`
2. `effectiveHalfRange = min(horizontalRange, allowedHalfX)`
3. `x = Clamp(x, -effectiveHalfRange, effectiveHalfRange)`

Wider platforms get a smaller allowed center range so the full platform stays on screen.

### Key fields

| Field | Purpose |
|-------|---------|
| `useCameraBasedHorizontalBounds` | Read width from `Camera.main` instead of fallback |
| `screenHorizontalMargin` | Padding from screen edge — increase to keep platforms away from borders |
| `clampPlatformsToVisibleWidth` | Enable camera-aware X clamp (recommended on) |
| `manualHalfWidthFallback` | Used when camera is unavailable (edit mode gizmos, no Main Camera tag) |
| `drawPlayfieldBoundsGizmos` | Cyan vertical lines in Scene view at safe playfield edges |

### Camera horizontal lock

`Camera.lockHorizontalPosition` (profile) / `VerticalCameraFollow2D.lockHorizontalPosition` keeps camera at **X = 0**. Only Y follows the climb. This prevents the view from drifting sideways when jumpers move laterally.

### Testing 9:16 Game View

1. Set Game View aspect to **9:16** (or a phone resolution).
2. Ensure Main Camera is tagged **MainCamera** and orthographic.
3. Enable `drawPlayfieldBoundsGizmos` and check Scene view lines match screen edges.
4. Play and climb — platform edges should stay inside the portrait frame.
5. Optional: enable **Enable Stream Debug Logs** on `PrototypeLevelBuilder` for `effectiveHalfRange` values.

### If objects still leave the screen

| Cause | Fix |
|-------|-----|
| Platforms too wide | Lower `platformWidth`, `recoveryPlatformWidthMultiplier`, or `safeLandingWidthMultiplier` |
| `horizontalRange` too large | Lower `horizontalRange` — camera clamp helps but very wide platforms still need smaller width |
| Jumpers/rope swing off-screen | Not clamped yet — reduce `maxHorizontalGap`, tilt control, or enable soft bounds later |
| Wrong aspect in editor | Match Game View to target device aspect (9:16) |
| No Main Camera at build time | Set `manualHalfWidthFallback` to expected half-width (~4.5 for size 8 @ 9:16) |

## Gameplay Feedback Layer (Prototype)

**Status:** First lightweight feedback pass — placeholder feel, not final VFX/SFX/art.

Goal: help the player read important moments (landing, rope tension, rescue pull, fall danger, height milestones) through subtle camera, haptics hooks, and simple UI overlays.

### Profile tuning (`Feedback` group)

On any `TarTullaGameplayProfile` asset:

| Field | Purpose |
|-------|---------|
| `enableFeedback` | Master switch — disables entire feedback layer |
| `enableCameraImpulse` | Subtle camera shake on landing / rope / danger |
| `landingCameraImpulse` | Landing shake strength |
| `ropeStretchCameraImpulse` | Overstretch shake strength |
| `dangerCameraImpulse` | High-danger shake strength |
| `enableHaptics` | Mobile vibration fallback (`Handheld.Vibrate`) |
| `landingHapticStrength` | Landing vibration weight (0–1) |
| `ropeStretchHapticStrength` | Rope overstretch vibration weight |
| `dangerHapticStrength` | Fall-danger vibration weight |
| `enableScreenDangerVignette` | Red/dark full-screen overlay when falling too far |
| `dangerStartRatio` | Danger ratio where vignette begins (0–1) |
| `dangerMaxAlpha` | Max vignette opacity at full danger |
| `enableHeightMilestonePulse` | HUD pulse every N meters + new best |
| `heightMilestoneInterval` | Meters between milestone pulses (default 10) |

**Disable all feedback:** set `enableFeedback = false` on the active profile.

### Event hub

`Scripts/Core/GameplayFeedbackEvents.cs` — lightweight static events (not a general event bus):

| Event | Source | Listeners |
|-------|--------|-----------|
| `OnJumperLanded` | `JumperController2D` | Camera, Haptics |
| `OnJumpImpulse` | `JumperController2D` | (reserved for future SFX/VFX) |
| `OnRopeOverstretched` | `ElasticRope2D` (0.2s cooldown) | Camera, Haptics |
| `OnPullAssistTriggered` | `ElasticRope2D` (0.2s cooldown) | Haptics |
| `OnDangerRatioChanged` | `PrototypeRunController` | Camera, Haptics, `DangerVignetteView` |
| `OnNewBestHeight` | `ClimbProgressTracker` | `HeightMilestonePulseView` |
| `OnHeightMilestone` | `ClimbProgressTracker` | `HeightMilestonePulseView` |

### Components

| Script | Location | Role |
|--------|----------|------|
| `CameraImpulse2D` | Main Camera | Additive offset read by `VerticalCameraFollow2D` — does not break follow |
| `HapticsFeedbackController` | Systems | Mobile vibrate fallback with cooldowns |
| `DangerVignetteView` | Canvas / SafeArea | Non-blocking red overlay by danger ratio |
| `HeightMilestonePulseView` | HUDPanel child | Brief `+10m` / `New Best!` pulse |

`PrototypeUIHierarchyBuilder` auto-adds vignette + milestone views on build or theme refresh.

### Placeholder vs final

| Now (placeholder) | Later |
|-------------------|-------|
| Solid-color danger overlay | Proper edge vignette sprite/shader |
| `Handheld.Vibrate()` | Rich native haptics (iOS/Android) |
| Random camera offset | Curated impulse curves |
| Text scale pulse | Animated UI + particles |
| No sound | Landing/rope/danger/milestone SFX |

### Feedback testing checklist

**Editor**

- [ ] Landing → subtle camera impulse
- [ ] Rope overstretch → subtle camera impulse (not every frame)
- [ ] Fall near fail line → danger vignette fades in
- [ ] Every `heightMilestoneInterval` meters → `+Nm` pulse
- [ ] New best height → `New Best!` pulse
- [ ] No feedback spam in console

**Device**

- [ ] Landing / pull / danger haptics noticeable but not annoying
- [ ] HUD readable with vignette active
- [ ] Camera follow stable
- [ ] No performance regression

**Tune first:** `landingCameraImpulse`, `dangerStartRatio`, `dangerMaxAlpha`, `heightMilestoneInterval`, haptic strengths.

## Onboarding and Run Start Flow

**Status:** First onboarding pass — countdown, tutorial hints, run summary. Not final tutorial art.

### Flow

```
Boot → MainMenu
MainMenu --Tap Start--> Ready (countdown, timeScale=0)
Ready --Countdown done--> Playing (timeScale=1)
Playing --first run--> Tutorial hints overlay (optional)
Playing --Pause--> Paused
Playing --OnRunFailed--> GameOver (summary + stats)
GameOver --Retry--> Ready → Playing (same countdown flow)
```

Skip countdown when `Onboarding.showCountdown = false` on the active profile → MainMenu goes directly to Playing.

### Profile tuning (`Onboarding` group)

| Field | Purpose |
|-------|---------|
| `showCountdown` | Ready / 3 / 2 / 1 / Climb! before gameplay |
| `countdownStepDuration` | Seconds per countdown step (unscaled time) |
| `showTutorialHints` | Show first-run hint sequence |
| `hintDuration` | Seconds each hint stays visible |
| `rememberTutorialSeen` | Store seen flag in PlayerPrefs |

### Scripts

| Script | Role |
|--------|------|
| `RunCountdownView` | Countdown overlay (CanvasGroup fade, unscaled time) |
| `TutorialHintView` | First-run hints during Playing |
| `RunStatsTracker` | Landings + rope saves (via `GameplayFeedbackEvents`) |
| `RunSummary` | Data struct for Game Over summary |
| `SavedFeedbackView` | Brief “Saved!” on pull assist |

### Run preparation

`PrototypeRunController`:

- `PrepareRun()` — builds layout, resets progress, **run paused** (no tick/fail logic)
- `StartPreparedRun()` — unpauses run (physics active when `timeScale = 1`)
- `StartRun()` — both in sequence (legacy/direct start)

During countdown: `Time.timeScale = 0`, countdown uses `Time.unscaledDeltaTime`.

### Tutorial PlayerPrefs

Key: `TarTulla_TutorialSeen` (value `1` = seen)

**Reset for testing:**

- Context menu on `TutorialHintView` component → **Reset Tutorial Seen**
- Or call `TutorialHintView.ResetTutorialSeen()` from code/console
- Or `PlayerPrefs.DeleteKey("TarTulla_TutorialSeen")`

### Game Over summary

Shows:

- Height / Best
- **New Best!** badge when run beat previous session best
- `Landings X · Saves Y` when stats > 0 (from `RunStatsTracker`)

### Onboarding testing checklist

**Editor**

- [ ] Starts at Main Menu
- [ ] Tap Start → countdown → gameplay
- [ ] First run shows tutorial hints
- [ ] Second run skips hints (if `rememberTutorialSeen`)
- [ ] Game Over shows height, best, stats
- [ ] Retry runs countdown again
- [ ] Pause still works
- [ ] `timeScale` never stuck at 0 after countdown/resume

**Device**

- [ ] Countdown readable
- [ ] Hints readable, inside safe area
- [ ] Tilt ready after countdown
- [ ] UI does not block gameplay center

## UI Foundation (Prototype)

**Status:** UI/UX Pass 3 — visual identity foundation (theme, Tar/Tulla motif, rope line). Not final art.

### Theme asset

Location: `Assets/_Project/Settings/UI/TarTulla_UITheme_Default.asset`

Script: `TarTullaUITheme.cs` — **Create → Tar&Tulla → UI Theme**

Edit colors in the theme asset; assign on `UIRoot` → `PrototypeUIHierarchyBuilder` → **Theme**. `UIThemeApplier` applies on build.

Key colors: Tar cyan (`accentColorTar`), Tulla orange (`accentColorTulla`), rope cream (`ropeColor`).

### Scripts (`Scripts/UI/`)

| Script | Role |
|--------|------|
| `UIState` | Boot, MainMenu, Ready, Playing, Paused, GameOver |
| `UIManager` | State machine, safe transitions, `timeScale`, debug logs |
| `TarTullaUITheme` | ScriptableObject palette |
| `UIThemeApplier` | Applies theme to panels/texts/buttons |
| `RopeLineUI` | UI line between two RectTransforms |
| `UIPanelTransition` | Light fade on panel show |
| `SafeAreaFitter` | Applies `Screen.safeArea` (notch/cutout) |
| `UIStyleDefaults` | Fallback sizes + theme instance |
| `MainMenuView` | Title, subtitle, motif, Tap to Start |
| `HUDView` | Height / Best + climb hint + pause |
| `PauseView` | Resume, Restart, Main Menu |
| `GameOverView` | Motif, results, Retry / Main Menu |
| `DeveloperHUDView` | Dev overlay (off by default) |
| `DangerVignetteView` | Fall-danger red overlay (non-blocking) |
| `RunCountdownView` | Pre-run countdown overlay |
| `TutorialHintView` | First-run tutorial hints |
| `SavedFeedbackView` | Brief “Saved!” on pull assist |
| `HeightMilestonePulseView` | `+Nm` / `New Best!` HUD pulse |
| `PrototypeUIHierarchyBuilder` | Builds themed Canvas on `UIRoot` |

### Tar&Tulla motif

Two colored dots (Tar/Tulla accents) + `RopeLineUI` on **Main Menu** and **Game Over**. No character art.

### Scene hierarchy (runtime)

```
UIRoot
├── EventSystem
└── Canvas (1080×1920, match 0.5)
    ├── CanvasBackground
    ├── FullscreenBlocker
    └── SafeArea (SafeAreaFitter)
        ├── DangerVignette
        ├── MainMenuPanel (+ PairMotif)
        ├── HUDPanel (+ ClimbHint, HeightMilestonePulse)
        ├── PausePanel
        ├── GameOverPanel (+ PairMotif)
        └── DeveloperHUDPanel
```

### SafeAreaFitter

Attach to the `SafeArea` container (done automatically by `PrototypeUIHierarchyBuilder`).

- Reads `Screen.safeArea` each frame when resolution or orientation changes
- Sets anchor min/max on the target `RectTransform`
- Keeps HUD and buttons inside iOS notch / Android cutout zones
- Optional `enableDebugLogs` on the component (off by default)

**Note:** If an old Canvas exists without `SafeArea`, the builder destroys and rebuilds it on next Play.

### UI state flow

```
Boot → MainMenu
MainMenu --Tap Start--> Ready (countdown)
Ready --Countdown done--> Playing
Playing --Pause--> Paused (timeScale=0)
Paused --Resume--> Playing
Paused --Restart--> Ready → Playing
Paused --Main Menu--> MainMenu (StopRun)
Playing --OnRunFailed--> GameOver (timeScale=0)
GameOver --Retry--> Ready → Playing
GameOver --Main Menu--> MainMenu (StopRun, timeScale=1)
```

`UIManager` always hides all panels before showing the active state. Invalid transitions are ignored (e.g. Pause only from Playing).

Enable **Enable Debug Logs** on `UIManager` for `[Tar&Tulla][UI] State changed: ...` messages.

### Main Menu start behavior

- Play begins at **Main Menu** (`PrototypeRunController.autoStartOnPlay = false`)
- **Tap to Start** → countdown (`Ready` state) → gameplay
- Subtitle: *Two jumpers. One rope. Keep climbing.*

### Pause behavior

- `Time.timeScale = 0` — physics frozen
- `FullscreenBlocker` dim + Pause panel inside SafeArea
- Resume restores `timeScale = 1`
- Restart calls `ResetRun()` (clear + rebuild layout)
- Main Menu calls `StopRun()` and returns to menu

### Game Over behavior

- Triggered by `PrototypeRunController.OnRunFailed`
- Shows `Height Xm` / `Best Ym`, optional **New Best!**, landings/saves stats
- Retry: countdown flow → Playing
- Main Menu: `timeScale = 1`, `StopRun()`, MainMenu state

### Developer HUD

Disabled by default. Enable on `UIManager` → **Show Developer Hud In Editor** or `DeveloperHUDView.showDeveloperHUD`.

Shows: profile, UI state, height, best, platform count, tilt, rope stretch. Hidden in release builds.

### Connection to gameplay

`UIManager` → `PrototypeRunController` only:

- `StartRun()` / `ResetRun()` / `StopRun()`
- `SetRunPaused(bool)`
- `OnRunFailed(float height, float bestHeight)`

No circular dependencies. No physics in UI scripts.

### Text rendering

Unity UI `Text` + `LegacyRuntime.ttf`. TextMeshPro is not in the project yet — swap later without changing theme flow.

HUD format: `Height 42m`, `Best 78m`.

### Light transitions

`UIPanelTransition` on Main Menu, Pause, Game Over — short fade-in (~0.12s). HUD stays instant. State logic always uses `HideInstant` when switching panels.

### Still placeholder (final art pass later)

- No character sprites, custom fonts, or TMP styling
- No rounded panel corners (`panelCornerRadius` reserved)
- No sound, settings, profile picker, monetization
- Motif is geometric only (dots + line)
- No progress bar — only subtle climb hint line in HUD

### 9:16 mobile testing checklist

**Editor**

- [ ] Game View **9:16**
- [ ] Play → Main Menu (not gameplay)
- [ ] Tap Start → HUD + gameplay
- [ ] Pause stops movement; Resume continues
- [ ] Restart → clean run, no duplicate Tar/Tulla/platforms
- [ ] Fall fail → Game Over with height/best
- [ ] Retry → clean new run
- [ ] Main Menu from Pause/Game Over → clean menu

- [ ] Main Menu shows Tar/Tulla dots + rope line
- [ ] Game Over shows motif above title
- [ ] Retry button visually primary; Main Menu secondary
- [ ] Dev HUD visually distinct (green-ish mono text, corner)

**Device**

- [ ] UI inside safe area (not under notch)
- [ ] Theme colors readable in sunlight / dark mode
- [ ] Buttons easy to tap (≥80px ref height)
- [ ] HUD readable, pause reachable top-right
- [ ] No clipped text on rounded corners

### Intentionally not final

- No custom art assets imported
- Jumpers not soft-clamped to screen edges yet

## Design Documentation

See `/GameDesign/` at the repository root.
