# Falling Knights — Project Guide for Claude

## Team context

Three-person learning team from a game jam:
- One programmer (backend/other-field background, learning Unity/game dev)
- Two artists (learning game design and tooling)

Goal: **structured learning**, not shipping a specific game. Design decisions should prioritize being educational and clear over being clever or over-engineered.

---

## Game concept

2D evasion game:
- Player (squire) moves left/right on screen
- Falling objects (knights) drop from the top
- Survive as long as possible — score = seconds survived
- Theme: squire in a tower avoiding falling knights

---

## Project structure

```
Assets/
  Scripts/
    GameManager.cs          — game state machine, score, high score
    PlayerController2D.cs   — movement, walk/run/stun/dead states
    PlayerHealth.cs         — HP, i-frames, death events
    PlayerAnimator.cs       — drives Animator parameters from controller state
    FallingObject.cs        — per-object physics + collision
    FallingObjectConfig.cs  — ScriptableObject: per-type tuning data
    FallingObjectSpawner.cs — spawning, difficulty scaling, distributions
    UIRoot.cs               — menu/HUD/game-over panel switching
    HeartsUI.cs             — heart icon HP display
    AudioManager.cs         — singleton, music + SFX playback
    DebugOverlay.cs         — F3 toggleable runtime debug panel
    Enums/
      GameState.cs          — BootMenu, Playing, GameOver
      PlayerMoveState.cs    — Idle, MovingWalk, MovingRun, Stunned, Slide, Dead
  Art/
    UI/                     — heart sprites and UI art (artist-provided)
  Prefabs/
    HeartIcon.prefab        — heart UI prefab used by HeartsUI
```

---

## Architecture decisions

### Patterns in use
- **State enums** — `GameState` and `PlayerMoveState` drive all state transitions explicitly
- **Event-driven health** — `PlayerHealth` fires `OnHPChanged`, `OnDied`, `OnHit`; subscribers react
- **ScriptableObject config** — `FallingObjectConfig` holds per-type data (damage, speed, sprite, SFX); prefab holds collider/visual setup
- **Singleton** — `AudioManager.I` only; not overused
- **No magic numbers** — tuning values are serialized fields, not hardcoded

### Input
- Using Unity's **new Input System** in **Send Messages** mode
- Callback signature must be `OnMove(InputValue value)` — NOT `OnMove(InputAction.CallbackContext)`
- This was a critical bug fix; do not change this pattern

### Movement
- Kinematic-style: accumulate `_speed` manually, write to `Rigidbody2D.velocity`
- One `speedMax`, two acceleration modes: `accelWalk` and `accelRun`
- Run mode activates after holding same direction for `runFromTime` seconds
- `brakingForce` decelerates on no-input or direction reversal
- Player is clamped to `[minX, maxX]` bounds

### Hit / damage
- Discrete integer HP
- On hit: lose HP, stun for `stunTime`, gain i-frames for `iFramesInterval`
- During stun: input disabled, player brakes to stop
- HP ≤ 0 → GameOver

### Spawner difficulty
- Base mode: uniform random X
- Second mode: `PlayerGaussian` — Box-Muller Gaussian centered on player X
- Gaussian params: `gaussianSigma`, `gaussianWeight` (mix ratio), `minSpawnDistanceFromPlayer`
- Difficulty: exponential spawn interval decay over time
- Capped by `maxFallingObjects` — skip spawn if at cap, no queuing

### UI flow
- Boot menu → Playing HUD → Game Over
- `UIRoot` owns all panel switching and text updates
- Hearts UI replaces plain HP text — `HeartsUI` converts integer HP units to full/half/empty heart sprites
- `unitsPerHeart` controls granularity (e.g. 2 = half-heart support)

### Audio
- `AudioManager` singleton with two `AudioSource` components: one for music (loop), one for SFX (`PlayOneShot`)
- Convenience methods: `PlayUIClick()`, `PlayPlayerHit()`, `PlayObjectImpact()`

### Animation

**Setup:**
- `CharBody.prefab` (artist-made, nested inside `Player.prefab`) holds the full skeleton: 17 SpriteRenderers + 40+ bones
- `CharBody` has the only `Animator` component — it uses `AC_Player.controller`
- `PlayerAnimator` (on Player root) drives CharBody's Animator via inspector reference

**Rig flipping:**
- Do NOT use `spriteRenderer.flipX` — the rig has 17 sprites across bones
- Flip `CharBody.localScale.x` to ±1 to mirror the entire rig
- `PlayerAnimator.characterBody` holds the CharBody Transform reference

**Animation speed scaling:**
- `animator.speed` is scaled every frame: `Lerp(1f, runAnimSpeed, speed / speedMax)`
- `runAnimSpeed` (default 1.8) is a tunable on `PlayerAnimator`

**Animator parameters** — must match `AC_Player.controller` exactly:
| Parameter | Type    | Notes |
|-----------|---------|-------|
| Speed     | float   | absolute horizontal speed (0–speedMax) |
| IsMoving  | bool    | |
| IsRunning | bool    | |
| IsStunned | bool    | |
| IsDead    | bool    | |
| Hit       | Trigger | one-shot |

**Current controller states:** Idle (no clip), Locomotion → PalyerWalk.anim, Stunned (no clip), Dead (no clip), Hit (no clip)
- Only Locomotion has a clip assigned — remaining states need clips from artists

---

## Key rules for future changes

1. **Don't break the Input System signature** — keep `OnMove(InputValue value)`
2. **FallingObjectConfig is for type data** — don't put collider sizes or prefab structure there
3. **Spawner skips, never queues** — when at `maxFallingObjects`, just skip that tick
4. **ReviveAndReset** is the single player reset entry point — resets controller state, health, and animator. Call only this; do not call health/animator reset separately from GameManager
5. **DebugOverlay** is toggled with F3 — keep it wired to GM/Player/Health/Spawner references
6. **AudioManager.I** is the only singleton — don't add more singletons without discussion

---

## Current status (as of project handoff)

| Feature | Status |
|---|---|
| Game loop (menu → play → game over → restart) | Done |
| Player movement with walk/run/stun/dead | Done |
| Falling object spawning + difficulty scaling | Done |
| Gaussian spawn distribution | Done |
| HP system + i-frames | Done |
| Hearts UI | Done (scripts + prefab, needs art wiring) |
| Audio system | Done (wiring, needs audio clips assigned) |
| Debug overlay (F3) | Done |
| Animation bridge (PlayerAnimator) | Done (walk anim live, idle/stun/dead/hit states need clips) |
| GitHub repo | Done |

## Suggested next steps

1. **Art** — create animation clips for Idle, Stunned, Dead, and Hit states in `AC_Player.controller`
2. **Audio** — assign AudioClips to `AudioManager` fields in the Inspector
3. **Inspector** — wire `PlayerController2D.health` and `PlayerController2D.playerAnimator` on Player prefab
4. **Cleanup** — delete `Assets/Animation/payerWalk.anim` (duplicate, lowercase p, came in from old branch)
5. **Design** — tune spawner difficulty curve (`baseSpawnInterval`, `spawnMultiplier`, `difficultyStepSeconds`)
6. **Polish** — hit feedback (screen shake, particle, flash on player hit)
