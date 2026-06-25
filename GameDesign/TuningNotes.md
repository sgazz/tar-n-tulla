# Tuning Notes

This document tracks feel parameters and playtest observations. Values are starting targets for Milestone 1–2 prototyping—not final numbers.

## Rope Length

| Parameter | Starting Target | Notes |
|-----------|-----------------|-------|
| Rest length | TBD | Distance at zero tension; defines default spacing between Tar and Tulla. |
| Max stretch | 1.3–1.5× rest | Beyond rest length, tension ramps sharply. |
| Min length | ~0.6× rest | Prevents characters collapsing into each other. |

**Feel goal:** Slack is visible and useful; max stretch feels dangerous but recoverable.

---

## Rope Elasticity

| Parameter | Starting Target | Notes |
|-----------|-----------------|-------|
| Spring stiffness | Medium | Too stiff = snappy, arcade; too soft = mushy, unresponsive. |
| Damping (rope) | Low–medium | Reduces oscillation without killing sling energy. |

**Feel goal:** Clear stretch-and-snap; sling releases energy upward on well-timed jumps.

---

## Air Control

| Parameter | Starting Target | Notes |
|-----------|-----------------|-------|
| Horizontal influence | Low | Brief nudge only; no full mid-air steering. |
| Influence window | Post-jump only | Strongest in first 0.2–0.3s after leave ground. |

**Feel goal:** Player can correct slightly, not fly sideways across the screen.

---

## Tilt Sensitivity

| Parameter | Starting Target | Notes |
|-----------|-----------------|-------|
| Dead zone | ±3–5° | Ignore micro-tilt when device is flat. |
| Force scale | Low–medium | Tilt shapes arc; does not replace jump timing. |
| Smoothing | 0.1–0.15s | Filter sudden device shakes. |

**Feel goal:** Natural lean-in portrait; no constant drift on a table.

---

## Jump Force

| Parameter | Starting Target | Notes |
|-----------|-----------------|-------|
| Base impulse | TBD | Enough to reach next platform at tutorial spacing. |
| Variable jump | Optional | Hold = slightly higher (evaluate in M2). |
| Coyote time | 0.05–0.1s | Small forgiveness after leaving platform edge. |

**Feel goal:** Jump is committed but fair; height is predictable with practice.

---

## Pull Force

| Parameter | Starting Target | Notes |
|-----------|-----------------|-------|
| Trigger | Rope near max tension + one anchored | Auto or input-triggered. |
| Impulse toward anchor | Medium | Hauls trailing character without breaking sling fantasy. |
| Cooldown | 0.3–0.5s | Prevents spam-pull. |

**Feel goal:** Pull feels like a deliberate beat in the rhythm, not a panic button.

---

## Damping

| Parameter | Starting Target | Notes |
|-----------|-----------------|-------|
| Linear drag (air) | Low | Preserve swing momentum. |
| Linear drag (grounded) | Higher | Settle quickly on landing. |
| Angular drag | Medium | Reduce wild spinning without killing character rotation entirely. |

**Feel goal:** Airy swings, stable landings.

---

## Landing Forgiveness

| Parameter | Starting Target | Notes |
|-----------|-----------------|-------|
| Landing angle tolerance | ±15–25° | Sloped or slightly off-center still counts. |
| Edge grace | 10–15% platform width | Partial overlap counts as landed briefly. |
| Stabilize time | 0.1–0.2s | Brief window before slip-off from bad landings. |

**Feel goal:** Strict enough to matter; forgiving enough to encourage retry.

---

## Camera Responsiveness

| Parameter | Starting Target | Notes |
|-----------|-----------------|-------|
| Follow speed | Medium | Keeps lead character in upper third. |
| Lookahead | 0.5–1.0 units above lead | Show next platform early. |
| Death zone lag | 0.5–1.0s below frame | Clear fail without instant surprise. |

**Feel goal:** Smooth upward flow; both characters readable during swings.

---

## Haptics

| Event | Intensity | Notes |
|-------|-----------|-------|
| Jump | Light | Confirm input. |
| Land | Medium | Anchor beat. |
| Rope max tension | Light pulse | Warn before snap or fail. |
| Pull | Medium | Rhythmic emphasis. |
| Death | Heavy (single) | Clear run end. |

**Feel goal:** Haptics reinforce rhythm, not noise. Must be toggleable in settings.

---

## Physical Device Testing Notes

Use this table during Milestone 2 playtests. One row per session or device.

| Date | Device | OS | Rope Feel | Jump | Tilt | Camera | Landing | Notes |
|------|--------|-----|-----------|------|------|--------|---------|-------|
| | | | | | | | | |
| | | | | | | | | |
| | | | | | | | | |

**Rating scale (optional):** 1 = too weak / loose / slow — 3 = target — 5 = too strong / stiff / fast

**Test checklist:**
- [ ] Play 5 minutes without hand fatigue (tilt + tap).
- [ ] Rope tension readable without UI.
- [ ] Fail state understood on first fall.
- [ ] Retry within 2 seconds.
- [ ] No motion discomfort (camera + swing).
- [ ] Performance stable at 60 FPS (or device target).
