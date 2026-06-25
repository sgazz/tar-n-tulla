# Milestones

## Milestone 0: Project Foundation

**Goal:** Establish a clean base before gameplay implementation.

- Repository structure and documentation (this folder).
- Unity project setup: 2D, portrait, mobile build targets.
- Core folders, naming conventions, and scene hierarchy.
- Input abstraction for touch and tilt.
- Placeholder art (simple shapes) and debug UI only.

**Exit criteria:** Project opens, builds to device, and displays a test scene with no gameplay systems yet.

---

## Milestone 1: Core Physical Prototype

**Goal:** Prove the connected-jumper fantasy in code.

- Two rigid bodies connected by an elastic rope.
- Basic jump and gravity.
- Simple static platforms.
- Vertical camera follow and kill zone.
- Restart on fall.

**Exit criteria:** A developer can climb a few platforms using rope physics and basic jumps. Fun is optional; believability is required.

---

## Milestone 2: Game Feel and Rhythm Tuning

**Goal:** Make movement feel physical, rhythmic, and readable.

- Tune rope, jump, pull, landing, and damping (see TuningNotes.md).
- Implement sling/pull and landing forgiveness.
- Add tilt influence with dead zone.
- Iterate on camera smoothing and haptics.
- Playtest on multiple physical devices.

**Exit criteria:** External playtesters understand the loop within 60 seconds and want to retry after failing.

---

## Milestone 3: Character Identity

**Goal:** Tar and Tulla feel like characters, not physics blocks.

- Distinct silhouettes and readable animation states (idle, jump, land, swing, fall).
- Rope visual: stretch, slack, tension feedback.
- Light audio: jump, land, rope stretch, fail.
- Placeholder or first-pass character personality (no lore dependency).

**Exit criteria:** Players can tell Tar from Tulla during motion and describe the "feel" of the pair in one sentence.

---

## Milestone 4: Level Language

**Goal:** Build a vocabulary of platforms and challenges.

- 10–15 handcrafted platform layouts using the core mechanics only.
- Difficulty curve: teach → test → combine.
- Checkpoint or short-run structure.
- Basic scoring (height + simple bonuses).

**Exit criteria:** A 2–3 minute climb exists with intentional difficulty pacing and no new mechanics required.

---

## Milestone 5: Vertical Slice

**Goal:** One polished slice that represents the final game experience.

- Single cohesive level (or short sequence) with final-or-near-final feel.
- Character identity, audio, juice (particles, screen shake, haptics).
- Menu → play → fail → retry loop complete.
- Performance validated on target devices.

**Exit criteria:** The slice can be shown to stakeholders or soft-launched testers as proof of the product.
