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

### Animation (Animator contract)
Parameters `PlayerAnimator` sets — artists must match these exactly in the Animator:
| Parameter | Type    | Notes |
|-----------|---------|-------|
| SpeedX    | float   | absolute horizontal speed |
| IsMoving  | bool    | |
| IsRunning | bool    | |
| IsStunned | bool    | |
| IsDead    | bool    | |
| Hit       | Trigger | one-shot, Has Exit Time OFF |

Recommended animator structure:
- `Locomotion` blend tree (Idle / Walk / Run by SpeedX)
- `Hit` one-shot state
- `Stunned` state
- `Dead` state (optional end state)

---

## Key rules for future changes

1. **Don't break the Input System signature** — keep `OnMove(InputValue value)`
2. **FallingObjectConfig is for type data** — don't put collider sizes or prefab structure there
3. **Spawner skips, never queues** — when at `maxFallingObjects`, just skip that tick
4. **ReviveAndReset** must fully reset controller, health, and spawner on restart
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
| Animation bridge (PlayerAnimator) | Done (needs Animator Controller from artists) |
| GitHub repo | Done |

## Suggested next steps

1. **Art** — wire heart sprites into `HeartsUI` (fullHeart, halfHeart, emptyHeart fields)
2. **Art** — create Animator Controller matching the parameter contract above
3. **Audio** — assign AudioClips to `AudioManager` fields in the Inspector
4. **Design** — tune spawner difficulty curve (`baseSpawnInterval`, `spawnMultiplier`, `difficultyStepSeconds`)
5. **Polish** — hit feedback (screen shake, particle, flash on player hit)
