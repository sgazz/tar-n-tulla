# Mechanics

## Tar and Tulla — Two Connected Jumpers

- **Tar** and **Tulla** are two distinct characters with shared fate.
- Each has a rigid body; both are always connected by the rope.
- One may be grounded while the other is airborne; roles swap constantly.
- The player influences the pair, not each character independently.

## Elastic Rope

- A physics-based distance constraint between Tar and Tulla.
- The rope stretches under load and snaps back, storing and releasing energy.
- Slack vs. tension is visually and mechanically readable.
- Maximum length prevents infinite separation; minimum length prevents overlap collapse.

## Landing

- Landing on a platform converts downward momentum into stability.
- A clean landing on one character can anchor the pair for the next jump or pull.
- Poor landings (edge, angle, timing) risk slip, bounce, or both characters falling.
- Landing is the primary beat in the movement rhythm.

## Sling / Pull Mechanic

- When one character is anchored and the other swings, the rope can act as a sling.
- Releasing or jumping at the right moment transfers stored elastic energy upward.
- Pull input (or automatic pull on tension threshold) can haul the trailing character toward the lead.
- Expert play chains sling → land → pull → jump without losing momentum.

## Tilt Influence

- Device tilt applies gentle horizontal force or rotation bias to the pair.
- Tilt is assistive, not dominant—it shapes trajectory, not replaces timing.
- Tilt should feel natural in portrait: lean to shift weight, recover, or set up a sling.
- Dead zone near neutral to prevent drift on flat surfaces.

## Vertical Camera

- Camera tracks upward progress; primary motion is vertical.
- Slight lookahead above the lead character to show upcoming platforms.
- Death zone below the visible frame—falling off-screen or past threshold = fail.
- Camera smoothing tuned to keep both characters readable without motion sickness.

## Platforms

- Static and moving platforms at varied widths and spacing.
- Early platforms: wide, forgiving, close together.
- Later platforms: narrower, farther, angled, or with motion.
- Platform types to explore later: bounce pads, breakables, rope catch points.

## Falling / Death Condition

- If both characters fall below the kill line (relative to camera or world), the run ends.
- Partial falls (one character dangling) are recoverable if the pair can regain height.
- No health bar in early design—death is binary and clear.

## Scoring Ideas

| Category | Description |
|----------|-------------|
| **Height** | Primary score—max vertical distance reached. |
| **Clean landings** | Bonus for centered, stable landings. |
| **Momentum chains** | Multiplier for consecutive sling/land/pull actions without reset. |
| **Air time control** | Small bonus for recoveries from near-fall states. |
| **Run time** | Optional speed-climb variant for advanced players. |

Scoring should reinforce good movement, not encourage reckless jumping.
