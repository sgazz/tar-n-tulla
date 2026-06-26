# Tar&Tulla — Game Feel Tuning Pass 1

Physical device test notes. Fill in scores **immediately** after each 2–3 minute session.

## Testing instructions (device)

1. Use the **same device**, orientation (portrait), and grip for every profile.
2. Assign one tuning profile on `TarTullaRuntime` → **Startup Profile**.
3. Build to device (or use Unity Remote only for rough checks — prefer real build).
4. Play **2–3 minutes** per profile. **Do not tune values during the test.**
5. Write first impressions right away.
6. Always compare against **Tuning A — Default Control**.
7. Optional: enable **Developer HUD** on `UIManager` (`showDeveloperHudInEditor`) to confirm active values on screen.

## Score guide (1–5)

| Score | Meaning |
|-------|---------|
| 1 | Bad / broken / frustrating |
| 2 | Weak / noticeable problems |
| 3 | Acceptable / neutral |
| 4 | Good / close to target |
| 5 | Excellent / keep this direction |

---

## Results table

| Profile | Goal | What to observe | Tilt 1–5 | Rope 1–5 | Landing 1–5 | Camera 1–5 | Fun / retry 1–5 | Notes |
|---------|------|-----------------|----------|----------|-------------|------------|-----------------|-------|
| **A — Default Control** | Balanced baseline for comparison | Predictable jumps, stable rope, readable platforms | | | | | | |
| **B — Tight Rhythm** | Shorter rope, rhythmic Tar/Tulla exchange | Faster handoff, tighter gaps, dance-like tension | | | | | | |
| **C — Soft Float** | Forgiving, relaxed feel | Floaty arcs, wide platforms, calm camera | | | | | | |
| **D — Strong Rescue** | Visible partner saves | Pull assist moments, “Saved!” feedback, recovery without auto-win | | | | | | |
| **E — Fast Climb** | High vertical pace | Speed, challenge, tilt response, chaos vs control | | | | | | |

---

## Per-profile observation prompts

### A — Default Control
- Does this feel like the current prototype?
- Anything unexpectedly stiff or floaty?
- Use as reference score for other profiles.

### B — Tight Rhythm
- Do Tar and Tulla swap roles quickly and clearly?
- Does the rope feel too snappy or nicely rhythmic?
- Are platform gaps fair at your skill level?

### C — Soft Float
- Is landing forgiving without feeling sluggish?
- Does extra air control compensate for softness?
- Is it *too* easy / boring?

### D — Strong Rescue
- Do you notice partner saves without the game playing itself?
- Is pull assist feedback clear (haptic / “Saved!” / rope stretch)?
- Any chaos from high assist?

### E — Fast Climb
- Does vertical pace feel exciting or overwhelming?
- Can you still read platforms and rope state?
- Would this work for advanced players only?

---

## Key values to verify (Developer HUD)

When HUD is enabled, confirm these change per profile:

- `jumpForce`, `gravityScale`
- `restLength`, `springStrength`, `pullAssist`
- `tiltSensitivity`
- Active profile **displayName**

If values do not change when switching profiles, **stop testing** and fix profile wiring first.

---

## Session log (optional)

| Date | Device | Tester | Winner | Next action |
|------|--------|--------|--------|-------------|
| | | | | |
