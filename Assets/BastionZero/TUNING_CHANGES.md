# Bastion Zero — tuning change log

Every edit to an `[estimated]` value, and every place where the 60 Hz implementation
measurably differs from the spec table, is recorded here. The spec in `Assets/_Docs/` stays
read-only; the numbers it owns are the baseline, this file is the running delta.

## SYS1 — Player movement (`Assets/BastionZero/Tuning/PlayerMovementTuning.asset`)

### v0 — 14 Nov 2026: built from spec v3.2, no `[estimated]` value retuned

All values are the spec's (`05-movement.md` §3.1). What follows is measured behaviour in
`Assets/BastionZero/Scenes/MV_Gym.unity` at 60 Hz, not a retune.

| ID | Spec value | Measured at 60 Hz | Why |
|---|---|---|---|
| MV-01 run speed | 8.5 tiles/s | 8.50 u/s | exact |
| MV-07 / MV-08 held rise | 13.8 tiles/s for 0.18 s | same, window rounded to 11 ticks (0.1833 s) | 0.18 s is 10.8 ticks; truncating to 10 ticks cost ~0.3 tiles of jump height |
| MV-10 full jump height | 4.0 tiles | 3.94 tiles | the continuous-math arc is 3.945 at this tick alignment; matches |
| MV-11 time to apex | 0.40 s | 0.45 s | whole-tick hold window plus the apex band; within one 0.05 s beat |
| MV-12 / MV-13 tap hop | 1.07 tiles (0.05 s tap) | 1.03 tiles | 0.05 s is 3 ticks; continuous value is 1.07 |
| MV-14 / MV-15 apex band | 2.0 tiles/s at 0.5 × g | hold inside the band measured | — |
| MV-17 fall cap | 19 tiles/s | 19.0 u/s | exact |
| MV-19 coyote | 0.10 s | 0.10 s | exact |
| MV-20 jump buffer | 0.10 s | 0.10 s, timestamped from the input event | — |
| MV-21 head-bump suppression | 0.06 s | 0.06 s; rise stops at the ceiling, coyote reads 0, no free jump afterwards | — |
| MV-22 stored air-jumps | 1 count, granted by a bounce | 0 by default; only `PlayerMovement.GrantBounceAirJump()` sets it | landing and clinging **spend** it, they do not grant it — otherwise every ground jump gets a free double jump and a 5.0-tile ledge stops being an MV-D3 gate |
| MV-39 wall climb | 4 tiles/s | 3.85–3.99 u/s | exact within sampling |
| MV-40 wall hold | hold, not slide | 0.000 u of slide over 0.6 s | exact |
| MV-41 wall-jump push | 6 tiles/s | −6.0 u/s away from the wall | exact |
| MV-42 re-cling lockout | 0.12 s | 0.12 s | — |
| MV-43 hold-away release | 0.08 s | 0.08 s | — |
| MV-D1 reachable ledge | ≤ 3.5 tiles | 3.5-tile ledge landable; 5.0-tile ledge not landable from the floor, the 3.5 ledge or a one-way platform | gate maths holds with the bounce-only air jump |
| MV-D2 crossable gap | ≤ 6 tiles | 6-tile gap cleared at full speed, landing on the far floor; 9-tile gap falls short (reaches x 31.2 into a pit starting at 22) | exact |

Open deviations for the designer:

1. **MV-11** is 0.45 s rather than 0.40 s. The rise is tick-quantised and the apex band slows the
   last 2 tiles/s; bringing it to 0.40 s would need MV-09 or MV-15 retuned, which changes MV-10.
2. **MV-10** reads 3.94 tiles. If the level metrics want the full 4.0, raise MV-08 to 0.19 s
   (12 ticks) and re-measure — that is a tuning change and needs your sign-off.
3. **Wall cling is cling-able on any solid wall** for now. The spec has the concept ("cling-able
   wall") but no rule for marking walls, so SYS5's gate work will need a marker (component or
   layer) before gates GT1/GT2 are placed.

## Cross-checks outside SYS1

- `Time.fixedDeltaTime` was 0.02 (50 Hz); it is now 1/60 to match the spec. Every system built
  after this point assumes 60 Hz.
- The opening-level prototype's values (7 u/s run, 11.3 u/s jump, 2.5 × gravity, 0.12 s
  coyote/buffer, wing glide) are superseded by the spec motor; its glide fields were removed
  with the glide state, which the slice defers (13 §5.1 / §5.7).

## SYS2 — Collision and damage (`Assets/BastionZero/Tuning/DamageTuning.asset`)

### v0 — 14 Nov 2026: built from spec v3.2, no `[estimated]` value retuned

All values are the spec's (`06-collision-damage.md` §3.2). What follows is measured behaviour in
`Assets/BastionZero/Scenes/DM_Gym.unity` at 60 Hz, not a retune.

| ID | Spec value | Measured at 60 Hz | Why |
|---|---|---|---|
| CD-01 / CD-02 collision box | 0.75 × 1.75 tiles | applied on `Awake`, bottom pinned at feet y −0.90625 | grown from the feet so a taller box never sinks into the floor |
| CD-04 / CD-05 hurtbox | 0.5 × 1.375 tiles | applied on `Awake`, centred on the body | — |
| CD-17 / CD-18 pips | start 5, max 10 | start 5/10, respawn 10/10 | — |
| CD-19 D-Light | 1 pip | 5 → 4 on contact | — |
| CD-21 D-Hazard | 3 pips | 5 → 2 on a spike strip | — |
| CD-41 D-Pit | 2 pips | 7 → 5 on the pit volume, then the warp | — |
| CD-24 post-hit i-frames | 1.0 s | 0.61 s left after 0.39 s; blocks D-Light/D-Heavy only | hazards and pits are deliberately not blocked (§8.2) |
| CD-25 hazard re-hit lockout | 0.5 s | second hit from the same strip at ≈ 0.5 s | one spike strip is not a guaranteed death |
| CD-26 contact re-arm | one frame without overlap | re-armed only after the dummy left the overlap; second contact landed 1 pip | a hit *refused* by an i-frame window still consumes the arm, so standing inside an enemy during the arrival window deals no damage — §6 rule 4 as written |
| CD-27 knockback hold | 0.5 × duration | horizontal speed held for 0.125 s, then linear decay | — |
| CD-28 knockback tie zone | 0.25 tiles | centred hit pushed opposite the facing (−1.2 tiles) | — |
| CD-29 warp arrival i-frames | 1.0 s | granted after the warp fade, blocks every class | — |
| CD-30…CD-34 death | 0.15 / 0.8 / 0.4 / full HP / 1.5 s | D-1 freeze → D-2 → D-3 fade → D-4 respawn at 10/10 → D-5 grace; a hazard sitting on the hero lands its hit on the first frame grace expires | measured end to end |
| CD-S49 / CD-S50 knockback | 0.25 s, 7 h / 9 up | floor spikes pushed −1.2 tiles, wall spikes pushed +1.2 tiles (surface decides the sign) | — |
| CD-42 pit warp fade | 0.4 s each way | warp returned the hero to (−9.00, 0.91), the recorded safe ground | — |

Open deviations for the designer:

1. **Start 5 vs respawn 10.** CD-17 starts the run at 5 pips but CD-33 respawns at full HP, so the
   first death silently raises the hero's working maximum from 5 to 10. Implemented literally. If
   that is not the intent, either CD-33 becomes "start HP" or CD-17 becomes 10.
2. **Respawn point** is the last safe ground (SYS1 row 52), because X17's "later of the last
   checkpoint and the last rest point" needs SYS5/SYS6. `HeroHealth.SetRespawnOverride()` is the hook
   those systems will call.
3. **Grace covers pits.** CD-29/CD-34 are all-damage windows, so a pit entered during either is
   refused. C41 only says pits ignore *post-hit* i-frames. Interpretation, not spec text.
4. **One hit per frame.** The priority rule rejects a second same-frame source whose priority is not
   strictly higher, so two hazards overlapping on one tick resolve once.
5. **Deferred by scope:** CD-03/CD-06 crouch boxes (need G-CROUCH), CD-09/CD-10 one-way tolerance
   and drop-through, CD-11 slopes, CD-12…CD-16 moving/collapsing platforms and CD-14 crush,
   CD-35/CD-36 lethal-hit guard, CD-39 retracting spikes, plus SYS6's coin penalty and death marker
   and SYS5's room-state reset.

## SYS3 — Combat (`Assets/BastionZero/Tuning/CombatTuning.asset`)

### v0 — 14 Nov 2026: built from spec v3.2, no `[estimated]` value retuned

All values are the spec's (`system-3-combat.md` §3, §5–§9). What follows is measured behaviour in
`Assets/BastionZero/Scenes/CM_Gym.unity` at 60 Hz, not a retune. Measured with a Fodder target
(EN-P1, 10 HP) and a Soldier target (EN-P9 band, 30 HP, poise threshold 25).

| ID | Spec value | Measured at 60 Hz | Why |
|---|---|---|---|
| §9 damage, G1 | W_base 10 × tier 1.0 × M_hit 1.0 = 10 | 10 units: the 10-HP Fodder died to one swing | — |
| §9 damage, G2 | 10 × 1.0 × 1.0 = 10 | 10 units: Soldier 30 → 20 | — |
| §9 damage, G3 | 10 × 1.0 × 2.0 = 20 | 20 units: Soldier 20 → 10 → 0 across the chain | the finisher's M_hit 2.0 is the only chain multiplier |
| §3.1 chain window | opens at recovery frame 4 (G1 frame 13) | G1 accepted the next press from frame 13; chain counter read 0 → 1 → 2 through G1 → G2 → G3 | — |
| §5.2 dash duration | 12 f | C_DASH frame counter reached 12 | — |
| §5.2 dash distance | 17.5 tiles/s for 3.5 tiles | 3.5 tiles (x −7.95 → −4.45) | exact |
| §5.2 dash cooldown | 0.40 s after the dash ends | readout showed 0.40 s at exit | — |
| DB-1 dash i-frames | f3–f10, enemy sources only | a contact hit taken mid-dash was negated (readout `negations 1`) | hazards are excluded by design (§10 row D-Hazard) |
| DB-3 air dash | locked | a mid-air DASH press left the state at K_IDLE with no displacement | `Dash_AirDashUnlocked = false` |
| §5.3 block raise | BK-RAISE 4 f, not guarding | BK_RAISE held for 4 frames before BK_HOLD | — |
| §5.3 guard points | 4 | 4/4 at every sample through a fresh hold | — |
| §5.4 parry window | first 8 f of BK-HOLD | PARRY marker seen on BK_HOLD frames 3, 5, 6, 7 and 8 | the window opens on BK_HOLD frame 1 and closes after 8 |
| §5.3 block is ground-only | ground states only | a mid-air BLOCK hold left the state at K_IDLE, no guard, no displacement | — |
| §6 poise per hit | no per-hit number in the spec | Soldier poise read 8/25 shortly after the first hit | §6 gives thresholds and decay, not the per-hit amount: `PoiseBasePerHit = 10` is a gap fill (see deviation 1) |

Open deviations for the designer:

1. **`PoiseBasePerHit` (10) is a gap fill.** §6 rule 3 gives the tier thresholds (EN-P1 0,
   EN-P9 25, EN-S4 60, EN-B9 120) and the decay rates but never states how much poise one hit
   deals, so 10 was chosen to make a Soldier stagger on the third hit of the ground chain. Retune it
   here, not in the spec.
2. **Hit-stop freezes `Animator` components, not actors.** §6 rule 1 says the attacker and target
   hold; the implementation holds their animators, because SYS3 has no authority over another
   system's movement. The gym dummies have no animator, so target-side stop is invisible until SYS4
   art exists; the hero's own stop is visible.
3. **Chip is damage but never the last pip.** §5.3 says chip stops at 1 pip. `HeroHealth.ApplyChip`
   clamps the applied amount to `CurrentHP − 1`, starts the C24 window as normal, and leaves the
   hero's pip count untouched at 1 if the clamp lands on zero.
4. **`HERO` guard order.** §1.7's one-resolution-per-frame rule is applied before the guard is
   offered the hit, and a guard consumes the frame's resolution whether it negates or chips, so two
   overlapping sources still resolve once.
5. ~~**Air attack has no animation.** `Nexus.controller` owns one Attack clip (Nexus_Attack_1) and
   has no air-attack node, so G1/G2/G3/AIR1/WALL1 all play the same clip; the animator trigger is
   only raised while grounded, so a stale trigger cannot fire later.~~ **Superseded** by Art +
   Animation v2: G1/G2/G3 are their own `Nexus_Attack_G1|G2|G3` states, driven by `AttackChain`.
   AIR1/WALL1 still have no node and the trigger is still raised only while grounded.
6. **Dash and block are visually silent.** SYS1's animator has no dash or block state, so both read
   only through the F8 overlay and the movement itself.
7. ~~**Sprint overlaps Dash on left Shift.** `Player/Sprint` already owned `<Keyboard>/leftShift`;
   `Player/Dash` now binds the same key. Nothing reads Sprint, so nothing conflicts today, but one
   of the two needs a new key before SYS6.~~ **Superseded** by the Polish pass: left Shift was taken
   off `Player/Sprint`, so it belongs to Dash alone.
8. **Deferred by scope:** the air dash (DB-3 unlock), out-of-reach attacks that could not be hit in
   the gym, slot-B secondary weapons, active skills and cooldowns (§4), `GLIDE_DOWN` (no A-GLIDE in
   SYS1), `CROUCH1` (no G-CROUCH), projectile reflection and the unblockable-attack flag (S2's
   `DamageInfo` has no field for it). Enemy knockback is computed into `CombatHit.Knockback` and
   left for SYS4 to translate — §8's enemy column is not applied by a stationary target.


---

## Art + Animation v1 — Nexus archon pass (HD-2D)

Scope: the hero sprite set and its animation graph only. No motor, combat or damage numbers changed.

### Art

- Six body states rebuilt from `_ref/nexus_archon_reference.png` at 256 px / PPU 128:
  `standing` 8, `walking` 8, `running` 8, `jumping` 6, `falling` 4, `attack_1` 6.
- Three wing states rebuilt from `_ref/nexus_wing_reference.png` at 512 px / PPU 128:
  `wings/idle` 8, `wings/flap` 8, `wings/glide` 8.
- Sliced by `NexusArtTools` with **one** measured union per grid and **one shared `ReferenceSpan` of
  486 px** across every body state, so the hero is 1.797 u tall in all of them (scale 0.4733).
  Per-frame bbox normalisation is deliberately not used — it is what made the figure breathe.
- Helm corrected mid-pass to a smooth astronaut dome with a ball finial and a thin amber X visor,
  after the first anchor drifted back to the old horned, faceted, gold-grilled head. The correction
  is now a mandatory block in every grid prompt and is recorded in `_ref/design_spec.md`.

### Animation

| Clip | Frames | Length | Loop |
|---|---|---|---|
| `Nexus_Idle` | 8 | 0.6667 s | yes |
| `Nexus_Walk` | 8 | 0.6667 s | yes |
| `Nexus_Run` | 8 | 0.6667 s | yes |
| `Nexus_Jump` | 6 | 0.5000 s | no |
| `Nexus_Takeoff` | 2 | 0.1667 s | no |
| `Nexus_Fall` | 4 | 0.3333 s | yes |
| `Nexus_Land` | 2 | 0.1667 s | no |
| `Nexus_Attack_1` | 6 | 0.5000 s | no |
| `Nexus_Wing_Idle` / `_Flap` / `_Glide` | 8 | 0.6667 s | yes |

All keys sit on exact `i / 12` boundaries and the clip stops a whole frame after the last key, so
there is no repeated tail pose. `Nexus_Walk`'s old duplicate `03` key at 0.4167 s — the loop-seam
hitch — is gone.

**Every transition in both layers is now 0 s.** The old `0.05–0.08 s` cross-dissolves on
`Fall` / `Takeoff` / `Land` / `Attack` / `Glide` blended two sprites and read as ghosting.

### Tuning IDs (`PlayerAnimationTuning.asset`)

| ID | Value | Meaning |
|---|---|---|
| `AN_01_WalkCycleDistance` | 2.40 u | Ground distance one 8-pose walk cycle covers. Rate = speed × 0.6667 ÷ this. |
| `AN_02_RunCycleDistance` | 5.60 u | Same for the sprint; at `MV_01` (8.5 u/s) the rate resolves to ≈ 1.01. |
| `AN_03_MinPoseHoldFrames` | 3 | Pose-hold floor in 60 fps frames; caps the cycle rate at ≈ 1.67. |
| `AN_04_WingFlapRateScale` | 1.0 | Multiplier on the wing flap beat. |

`Walk`, `Run` and `WingFlap` state playback is now driven by `WalkCycleSpeed`, `RunCycleSpeed` and
`WingFlapRate`, written each frame by `HeroAnimationDriver` from `PlayerMovement.Velocity`. That is
what removes foot slide: the clip advances at the rate the hero actually travels.

### Wings

- `Wings` is layer 1 of `Nexus.controller`, **default weight 0**, three states, zero-duration
  transitions, driving `VisualRoot/WingRoot` only. A layer only touches the paths its clips bind, so
  no AvatarMask is needed and the body clips are untouched by it.
- `VisualRoot/WingRoot` carries a `SpriteRenderer` at `sortingOrder 9` (the body is 10) with no
  sprite assigned, so a hero without the gem renders nothing there.
- `NexusWingAppearance` no longer swaps an `AnimatorOverrideController`. `UnlockAndReveal` sets the
  layer weight to 1 and fires `WingsGranted`; `ApplyStoredUnlock` does the same silently at spawn.
  The unlock therefore cannot change the body's resolution — the previous behaviour, and the reason
  the winged set was a second, worse-looking art set.
- The wings mirror the body's `flipX` in `LateUpdate`, because the motor flips the body renderer
  only.

### Tooling

`Assets/BastionZero/Editor/NexusAnimatorTools.cs` — idempotent menu items
`Bastion Zero/Art/Build Nexus Wing Layer` and `Bastion Zero/Tuning/Create Player Animation Tuning`.
Re-run the first one after regenerating wing art.

---

## Art + Animation v2 — combo swings and wing-state cleanup

Scope: the hero's attack animation chain, the wing-reveal state, and dead assets. No motor, combat
or damage numbers changed — this pass exists so the visuals match the SYS3 chain that was already
tuned.

### Combo art — three states replace `attack_1`

| Art state | Grid | Frames | Union (cell px) | Scale | Swing |
|---|---|---|---|---|---|
| `attack_g1` | 1536×1024, 3 × 2 | 6 | 512×482 | 0.4733 | downward diagonal cut |
| `attack_g2` | 1536×1024, 3 × 2 | 6 | 459×512 | 0.4733 | rising backhand |
| `attack_g3` | 1536×1024, 3 × 2 | 6 | 512×446 | 0.5157 | overhead finisher |

- Same twelve prompts, palette and mandatory astronaut-dome HEAD block as the body set.
- **No painted sword trails or motion-smear arcs.** The generator draws "motion smear" as a bright
  filled amber crescent that violates the pinstripe-only rule; G3 was regenerated four times to get
  a clean one. The swing reads from blade position alone.
- G1/G2 slice at the shared `ReferenceSpan = 486`, so the hero stays 1.797 u. **G3 slices with its
own measured span** (446 px): the art model caps a sheet at ~1.57 MP and would not draw G3's figure
  above ~446 px, so slicing it at 486 shrank the hero 8.8 % on the finisher. This is the only
  sanctioned per-grid span and it is recorded in `_ref/design_spec.md`.
- Accepted loss: a few pixels of blade tip are clipped where a pose touches its cell edge
  (`attack_g1` x0/x511, `attack_g2` y0/y511, `attack_g3` frames 04/05). The slicer logs
  `CLIPPED-AT-CELL-EDGE` as a note and still returns OK.

### Combo clips

| Clip | Frames | Fps | Length | Loop |
|---|---|---|---|---|
| `Nexus_Attack_G1` | 6 | 20 | 0.3000 s | no |
| `Nexus_Attack_G2` | 6 | 20 | 0.3000 s | no |
| `Nexus_Attack_G3` | 6 | ~~13.3333~~ 20 | 0.4500 s | no |

**Superseded (G3 fps):** G3 now runs at **20 fps with `KeyTimes`** `0 .08 .16 .22 .28 .40` — see the
Framing pass's Timing section. The `6 / 0.45` uniform grid below is the v2 history.

Clip lengths are dictated by the SYS3 windows, not by the 12 fps art budget: `CF-A1`/`CF-A2` = 18 f
= 0.30 s and `CF-A3` = 27 f = 0.45 s. `G3Fps` is `6 / 0.45` in `NexusArtTools`, so the 6-pose clip
stretches to the finisher's window. Keys are `i / Fps`, `stopTime = frames / Fps`.

### Controller

- Base Layer states are now `Idle`, `Walk`, `Run`, `Jump`, `Fall`, `Takeoff`, `Land`, `G1`, `G2`,
  `G3`. The `Attack` node, the `WingReveal` node and the `Glide` node are gone.
- New parameter `AttackChain` (Int, default 0). **`WingsGranted` (Trigger) removed** — 12 parameters.
- `Idle`/`Walk`/`Run`/`Land` → `G1`/`G2`/`G3` on `[Attack If, AttackChain Equals n]`, 0 s.
- `G1 → G2` and `G2 → G3` on `[Attack If, AttackChain Equals n]`, `hasExitTime false`, 0 s — this
  is the cancel that lets `HeroCombat` cut from recovery frame 4.
- `G1`/`G2`/`G3` → `Idle` / `Walk` / `Run` on `Speed`, `hasExitTime true`, `exitTime 1`, 0 s.
- Every transition in both layers is still 0 s.

### Combat → animator plumbing (`HeroCombat`)

`BeginAttack` writes `SetInteger(AttackChain, ChainIndexOf(attack))` **unconditionally**, in the same
frame it commits to the swing, then fires `Attack` only when the hero is grounded (there is no air or
wall attack node). `ResetAttackAnimator()` clears the trigger *and* zeroes `AttackChain`, and replaces
the old `ResetTrigger(Attack)` in `OnDisable`, `FinishAttack`, `EnterGuard` and `AbortToIdle`. The int
and the node therefore cannot disagree about which swing is playing.

### Wings

`NexusWingAppearance` lost `WingsGrantedParam` and `revealPlayed`; `UnlockAndReveal` now only calls
`WingUnlockState.UnlockWings()` + `ApplyWings()`. The reveal is purely the layer weight going 0 → 1,
since `WingReveal` no longer exists as a state.

### Deleted

`Nexus_Glide.anim`, `Nexus_WingReveal.anim`, `Unwinged/Nexus_Attack_1.anim` and the whole
`HD2D/attack_1/` art folder (6 PNGs). Reference scan before deleting: the controller's dependency
list no longer contains any of them; the only remaining holders were the legacy `Assets/Prefabs/Nexus.prefab`
and `Assets/Scenes/OpeningLevel.unity`, whose `Animator` only carried the old clips in its cached
`m_AnimationClips` array (display cache, repopulated from the controller). Still on disk and
untouched: the 64 px duplicates in `Assets/Animations/` root — they are the same cache case.

### Verified in play mode

`CM_Gym`, driving the real `Attack` binding (Enter), two full chains:

- press → Base Layer draws `attack_g1` within 0.047–0.054 s;
- three presses ~0.24 s apart → `G1 → G2 → G3` in order, twice; G1/G2 cancelled at 0.23–0.25 s,
  uncancelled G1 = 0.31 s, G3 = 0.454 s ≈ the 0.45 s clip;
- after G3 → `Idle` and holds 0.81 s, never re-entering `G2`; `IsInTransition` never true;
- `Wings` layer weight stayed 0.0 over all 359 sampled frames and the wing renderer stayed empty;
- `AttackChain` int matched `HeroCombat`'s attack every frame; no console errors.

---

## SYS5 — World and camera, the camera half (`Assets/BastionZero/Tuning/WorldCameraTuning.asset`)

Scope: SYS5 §10 only — the camera state list and the WC-W value table. Room graph, gates,
transitions, map and rest points (§3–§9, §11–§12) are not built yet.

### What replaced the debug camera

`GymCameraFollow` is gone from `Main Camera` in `CM_Gym`. The camera is now **`WorldCamera`**
(`Assets/BastionZero/SYS5_World/WorldCamera.cs`, execution order −100, steps on
`Time.unscaledDeltaTime` so hit-stop never freezes the framing — WC-W13). `CombatCameraShake`
stays at order 1000 and layers its offset on top.

### Tuning IDs (`WorldCameraTuning.asset`)

| ID | Value | Meaning |
|---|---|---|
| `W1_ViewportWidthTiles` / `W1_ViewportHeightTiles` | 24 / **17** | WC-W1. One orthographic size is resolved from both and the game-view aspect, so neither is cropped. At 16:9 that is **8.5**. Height was raised from the spec's 13.5 estimate — at 13.5 the ground the hero lands on sat off the bottom of the screen. |
| `W2_HorizontalDeadZoneTiles` | 1.5 | WC-W2, centred on the framing point. |
| `W3_HorizontalTrailSeconds` | 0.12 | WC-W3 time constant of the trail. |
| `W4_LookAheadTiles` | 3 | WC-W4. |
| `W4_HoldSeconds` | 0.25 | WC-W4: the look-ahead only arms after this much held input. |
| `W4_EaseSeconds` | 0.5 | WC-W4 ease-in, and the same rate eases it out. |
| `W4_TurnThresholdTiles` | 1.5 | WC-W4 dual-forward focus: a flip waits for this much travel. |
| `W5_BounceLookAheadTiles` | 4 | WC-W5, driven by `HeroCombat.AirborneHitChain > 0`. Glide is cut, so this is W5's only live case. |
| `W6_JumpFollowDelaySeconds` | 0.2 | WC-W6 hold after take-off. |
| `W6_JumpFollowFactor` | 0.6 | WC-W6, fraction of the rise the camera follows. |
| `W7_PlatformSnapSeconds` | 0.25 | WC-W7 platform snap on landing. |
| `W7_SnapDeadbandTiles` | 0.25 | Below this a grounded height change is followed directly, so flat ground never restarts the snap. |
| `W8_FallLockSeconds` | 0.15 | WC-W8: full descent follow from the apex. |
| `W8_FallSpeedFraction` | 0.8 | WC-W8, of SYS1 `MV_17_FallCap` (19 u/s). |
| `W8_LookDownHoldSeconds` / `W8_LookDownTiles` | 0.2 / 3 | WC-W8 look-down. A steady lead, eased in once — never accumulates. |
| `W9_FeetBelowCentreTiles` | 3 | WC-W9 grounded placement. |
| `W10_ClampEaseSeconds` | 0.15 | WC-W10 room-bound clamp. |
| `W15_ZoneEaseSeconds` | 0.4 | WC-W15 C-ZONE ease out. |
| `W15_LookHoldSeconds` / `W15_LookTiles` / `W15_LookEaseSeconds` | 0.4 / 4 / 0.3 | WC-W15 C-LOOK. |
| `W15_LookInputDeadZone` | 0.5 | Look input dead zone before a pan counts as held. |
| `LookIdleSpeedTilesPerSecond` | 0.1 | C-LOOK gate: "grounded and idle". |

`heroFeetLocalY` is not a spec value: it is −0.91, the hero origin to the soles of the 256 px sprite
frame (the same point `GroundCheck` sits on). Wall-cling framing (WC-W11) needs no number and no
tuning row: it flips the look-ahead to `PlayerMovement.ClingSide`, which is already the jump
direction.

### Camera states (§10.1)

`C_FOLLOW`, `C_ZONE`, `C_LOOK`, `C_ARENA`, `C_CUT`, `C_HOLD` all exist as `CameraState`, with
`WorldCamera.State` readable at runtime.

- **C-FOLLOW** — the two channels described above.
- **C-ZONE** — `CameraZone` volumes in the scene; `LockHorizontal`, `LockVertical` or `FixedAnchor`.
  Entering is immediate, leaving eases over WC-W15 (0.4 s).
- **C-LOOK** — up/down on `Player/Move` while grounded and under `LookIdleSpeedTilesPerSecond`,
  held 0.4 s, pans 4 tiles; eases back on release.
- **C-ARENA** — `EnterArena(Rect)` / `ExitArena()`, a fixed frame.
- **C-CUT** — `CutTo(Vector2)`, one frame, for transitions, H-WARP and respawn.
- **C-HOLD** — `Hold()` / `ReleaseHold()`, frozen for SYS2 D-2.

`WorldViewport` exposes the live camera rectangle in world units, look-ahead included, for
WC-W14 — SYS4's off-screen telegraph and R3's off-camera respawn checks.

### Room clamp (WC-W10)

`CameraRoomBounds` sits on the room root, not on the camera, so a room prefab carries its own
bounds and the room loader hands the current one over with `WorldCamera.SetRoomBounds`. An axis
shorter than the viewport is **centred on the room** rather than clamped. `CM_Gym`'s bounds are
x −20…20, y −6…20, which leaves the WC-W9 framing (camera y 3) inside the clamp at the 17-tile
viewport — the camera clamp is what silently lifted the framing 1.5 tiles when the height first
went to 17 against the old y −4 bound.

### Verified in play mode

`CM_Gym`, driven through the real bindings (D/A on `Player/Move`, Space on `Player/Jump`):

- ~~orthographic size **6.75** in `C_FOLLOW`, viewport 24 × 13.5 tiles at aspect 1.7778;~~
  **Superseded** by the Framing pass: `W1_ViewportHeightTiles` 17 → orthographic size **8.5**;
- at rest camera y **3.000** with the hero's feet at 0.000, stable to 0.0000 over 1 s;
- look-ahead 0 → **+3.000**, first movement 0.307 s after the hold began, easing in over 0.47 s,
  back to 0 in 0.556 s after release;
- dead zone + trail: with look-ahead still 0 the hero ran up to **1.612** tiles ahead of the camera;
- jump (take-off 14.955 s): camera y held 3.000 through 0.170 s of air time, then hero 0.910 →
  4.855 while camera 3.000 → 5.320 = **0.600×** of the rise; second jump the same;
- landing: `VerticalSnapActive` true, snap 2.985 → 3.006 (= feet + 3) with `snapElapsed` 0.248 s;
- WC-W10 over 550 samples: camera x −8.000…4.072, y 2.750…5.366, viewport never outside the room
  rect (max outside 0.0);
- no console errors.

---

## Framing pass — the ground you can land on, and swings that read as swings

Two play-test complaints drove this: *"I didn't see the ground"* and *"the sword animations are very
awkward still"*. Both were composition problems, not plumbing problems.

### The ground was on screen but not on screen

At WC-W1's 13.5 tiles the floor was a one-tile bar 78 % of the way down the frame with 3.8 tiles of
black nothing beneath it, and the hero stood on the far side of it. Three changes:

1. `W1_ViewportHeightTiles` 13.5 → **17** (orthographic size 6.75 → 8.5 on a 16:9 game view). This is
the framing that was asked for twice before the SYS5 table arrived; the table's value is `[estimated]`
and lost to the observation.
2. `CM_Gym/Floor` grew from `(40, 1)` at y −0.5 to **`(40, 6)` at y −3**, so the ground fills the
bottom of the frame instead of ending a tile below the soles. Its top surface is still exactly y 0, so
`GroundCheck`, the safe-ground recorder and the SYS2 pit warp are untouched.
3. `CM_Gym`'s `CameraRoomBounds` grew to **x −20…20, y −6…20** so the room clamp stops lifting the
camera. This mattered: with the 17-tile viewport against the old `yMin −4`, the clamp forced camera y
to 4.5 and the WC-W9 3-tile placement never happened — the hero landed at 4.4 tiles below centre
instead of 3.

Result at rest: orthographic size 8.5, feet 3 tiles below centre, 5.5 tiles of solid ground under the
feet, ledge and wall both in frame.

### The swings: the sheets, not the animator

The controller was already correct — 0 s transitions, no exit time into the chain, per-state clips
0.30 / 0.30 / 0.45 s. The awkwardness was in the art and in one timing choice:

- **Frame `00` of all three old sheets was an idle stance.** The attack clip starts the instant the
trigger fires, so the first 50 ms of every swing showed the hero standing still and then jumping to a
pose. The first pose is now a wind-up.
- **The old bodies were re-posed per cell**, so the figure's foot line moved up to 36 px (0.13 u)
mid-swing and the torso swung from side-on to camera-facing between frames. Regenerated with a
registration block: planted pelvis and boots, cape behind, only the right arm, the sword and a torso
lean change. Lowest-row spread is now 15 px (0.06 u) on G1, 12 px on G2 and 23 px on G3, and no pose
touches a cell edge.
- **G1's old frames `04`/`05` were near-duplicates and its arc never reached low**, so the cut read as
a thrust that faded out. All three arcs are now monotonic: G1 overhead → level → down 45°, G2 low →
waist → chest → up 45°, G3 drawn back → shoulder → overhead → level → held.

### Timing

`ClipRecipe` gained optional `KeyTimes` and `Length`. G1/G2 keep the even 50 ms grid. **G3 now
travels its whole overhead arc by 280 ms and holds the forward pose to the end of `CF-A3`**, because
a uniform 13.333 fps grid stepped the heaviest swing every 75 ms.

The last key sits one frame short of the target length: `AnimationClip.length` resolves to *final key
+ one frame at the clip rate*, and the animator measures a state by that length. G3 was rebuilt with
`stopTime = 0.45` and keys ending at 0.34 s and came back **0.415 s** long; moving the last key to
0.40 s at 20 fps gives exactly **0.450 s**. `CF-A3`'s 27 frames are intact.

### G3's scale exception is gone

The old G3 sheet measured 446 px against standing's 489 px and was sliced at its own span (0.5157)
to keep the figure the same size. The new sheet measures **482 px** against G1's 484 px, so G3 now
slices at the shared `BodyReferenceSpan` 486 with G1 and G2 — **one scale (0.4733) for every body
state again**, and `CLIPPED-AT-CELL-EDGE` is no longer logged for any combo grid.

### New editor tool

`Assets/BastionZero/Editor/NexusGridKeyer.cs` — menu **Bastion Zero/Art/Key Generated Combo Grids**.
The art model always returns an opaque sheet, but the slicer keys cell bounds on `alpha > 8`. This
samples the backdrop from eight border points and steps it out of the alpha channel, so a generated
sheet reaches the slicer byte-identical to what was drawn — no second model pass that could smear the
pinstripes or nudge a pose. Backdrops measured this pass: 254,254,254 on all three sheets.

---

## SYS4 — Enemies (`Assets/BastionZero/Tuning/EnemyTuning.asset`)

Scope: §3.1 the patroller `E_*` state machine, §3.2 the universal attack sign, §3.3 aggro /
leash, §3.4 the melee attack-token pool, and §4's fodder and soldier columns. §5–§14 are not
built (see deviation 5).

### v0 — 15 Nov 2026: built from spec v3.2, no `[estimated]` value retuned

All per-variant values are §4's table; all shared values are §3.3/§3.4. What follows is measured
behaviour in `Assets/BastionZero/Scenes/EM_Gym.unity` at 60 Hz, not a retune.

| ID | Fodder | Soldier | Measured at 60 Hz |
|---|---|---|---|
| EN patrol speed | 2.5 tiles/s | 2.0 | walked their bounds and reversed, 6 / 14 / 3 reversals |
| EN chase speed | 4.0 tiles/s | 4.5 | closed on the hero from detect range |
| EN detect range | 6 tiles (±2) | 8 (±2) | `E_NOTICE` fired at the near edge of the band |
| EN attack range | 1.25 tiles | 1.5 | engaged from inside `strikeOffset + strikeSize` |
| §3.2 telegraph | 0.40 s (24 f) | 0.45 s (27 f) | mode **24** and **27** frames exactly |
| §3.1 active window | 0.10 s (6 f) | 0.12 s (7 f) | 6 and 7 frames |
| EN lunge | 0.5 tiles | 1.0 | closed the gap inside the active window |
| §3.1 recovery | 0.50 s | 0.60 | returned to `E_COOLDOWN` on the tick count |
| §3.1 cooldown | 0.80 s | 1.00 | next telegraph only after it expired |
| §3.3 leash radius | 10 tiles | 14 | see the leash fix note below |
| §3.4 melee attack tokens | 2 | 2 | max **2** across 553 samples, zero samples above |
| §3.3 notice | 0.30 s | 0.30 s | `E_NOTICE` held 18 frames |
| §3.3 lost sight | 3.0 s | 3.0 s | `E_SEARCH` entered after the loss window |
| §3.3 search | 2.0 s | 2.0 s | — |
| §3.3 return timeout | 6.0 s | 6.0 s | — |
| §3.3 re-aggro lockout | 1.0 s | 1.0 s | no `E_NOTICE` during the lockout |
| §3.3 direction lock | 0.15 s | 0.15 s | aim frozen, then the sign tracks the hero only if still in range |

Body and strike geometry: `bodyHalfExtents` (0.55, 0.4), `ledgeProbeDepth` 0.6,
`strikeSize` (1.2, 0.9), `strikeOffset` (0.7, 0), `attackSignScale` 1.0.

| Behaviour | Measured |
|---|---|
| §3.2 sign visibility | sign renderer on in **80/87** fodder and **42/43** soldier `E_TELEGRAPH` samples, **false** in every `E_ATTACK` sample; renderer enabled matched `IsVisible` with 0 mismatches |
| §3.2 sign colour | sprite authored white, tinted at runtime — one sprite serves both the blockable and unblockable colours |
| §4 damage out | one pip per landed swing: 5 → 4 → 3 → 2 → 1 → 0 on the fodder, 8 → 7 on the soldier; `HeroHealth.LastDamage.Source` resolved to `EnemyAI` |
| §3.4 token release | released on entering `E_RECOVER`, on leaving `E_TELEGRAPH`/`E_ATTACK`, and in `OnDisable`/`OnDestroy` |
| §15 death hand-off | one Enter swing took a 10 HP target 10 → 0, state `E_DEAD`, `ContactDamageEnabled` false, position frozen |
| Ledge discipline | the fodder on `Ledge_A` stayed at **y 2.45 in all 296 samples** — never walked off |

Open deviations for the designer:

1. **The ground probe starts above the feet, not on them.** A downward ray from the foot line begins
   inside the floor collider, so `GroundAhead` casts from `halfY × 1.5` above the feet and reaches
   `ledgeProbeDepth` past them. A ray authored from the foot line would read ground everywhere and
   the climb-off would never trigger.
2. **The sign is a child object.** `AttackSign` lives on its own child so
   `attackSignScale` scales the sign and never the body — scaling the enemy root would change the
   hurtbox the SYS3 combo maths was verified against.
3. **The token pool is static and reset on subsystem registration.** It has to survive the enemy
   instance that holds a token being destroyed, because §3.4 is a *global* melee concurrency cap.
   `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]` clears it on domain reload so a token
   cannot leak across an editor play session.
4. **Contact damage is off in `E_SPAWN`, `E_STAGGER` and `E_DEAD`** (§15). `EnemyCombatTarget.Kill()`
   additionally disables `ContactDamager2D`, so the two switches AND together rather than one
   overriding the other.
5. **Deferred by scope:** §5 ranged, §6 charger, §7 flyer, §8 shielded, §9 elites and modifier M6,
   §10 bosses, §11 respawn classes R0–R3 and slot states, §12 the cross-archetype matrix, §13
   encounter budgets and threat points, §14 assist hooks. `E_SPAWN`, `E_DORMANT` and `E_STATUS` exist
   in the enum but no spawn-in telegraph, patrol slot or status source drives them yet.
6. **Enemy art is still the Cinder Mite placeholder.** Both patroller prefabs clone the SYS3 test
   mite; there is no §13 enemy sprite set yet. The state machine is art-independent, so the sprite
   pass can land without touching these numbers.

### Leash fix — applied, verified in the Polish pass

Play-mode verification surfaced one failure: an enemy held at its leash radius with the hero still in
sight bounced `E_ENGAGE` ↔ `E_SEARCH` on **every** fixed tick, because `TickSearch` tested
`heroVisible` for re-engage before it tested the leash. `TickSearch` now returns to `E_RETURN` when
beyond leash *first*, so the leash wins over sight.

**Verified in play mode (Polish pass):** a fodder lured to x −12.033 with the hero at x −16.19, in
sight, went `E_ENGAGE → E_SEARCH → E_RETURN` on consecutive ticks, made **zero** `E_SEARCH → E_ENGAGE`
flips and held `E_RETURN` for 3.5 s. A residual slow cycle remains by design: an enemy already
at its leash limit whose home sits inside the hero's detect range will still step
`E_IDLE → E_NOTICE → E_ENGAGE → E_RETURN` at tick pace — that is §3.3 working, not the bug.

---

## Polish pass — clean motion, hit impact, toolchain

Driven by "polish up what we have": smooth, clean motion and hit impact / enemy death. No spec value
was retuned; the two new numbers are gap fills named below.

### Sprite sampling — Point + mipmaps + `ignoreMipmapLimit`

At orthographic size 8.5 a 1080-px-tall view draws **63.5 px per unit**, but hero frames are
authored at **128 px per unit**: every frame is minified about 2×. With Point filtering and no
mipmaps that skips every other texel, so the 1-px pinstripes broke up and crawled as the hero moved.
All 76 HD-2D frames (52 body + 24 wing) and the Cinder Mite now import with **mipmaps on** and
**`ignoreMipmapLimit` on**, keeping Point, uncompressed, PPU 128 / 512. `ignoreMipmapLimit` is not
optional: quality level 0 ("Very Low") sets `globalTextureMipmapLimit 1`, which would drop mip 0 and
render every sprite at half resolution once mips exist. `NexusArtTools.ApplySpriteSettings` and
`Sys3Tools.ImportMiteSprite` both set this; `NexusArtTools.ReapplyImportSettings` re-applies it to
all 77 images and re-reads every importer to confirm it.

### Edge-matte repair

`NexusGridKeyer.BleedEdgeColour`'s flatten pass now writes RGB only and keeps each pixel's alpha.
Menu **Bastion Zero/Art/Repair Sprite Edge Mattes** (`RepairEdgeMattes`, replacing
`RepairAttackFrames`) re-bleeds every frame plus the mite, re-reads each written file and compares
every alpha byte: **alpha unchanged on 77/77**, totals opaque 197 941 / semi 1 860 561 /
clear 8 689 402. The 59 non-attack images had no fully opaque seed and came back byte-identical.
`Hero.prefab/VisualRoot`'s edit-time sprite now points at `HD2D/standing/00.png`.

### Enemy render smoother

`Assets/BastionZero/SYS4_Enemies/EnemyRenderSmoother.cs` on both patroller prefabs (added by
`Sys4Tools.BuildOne`). Patrollers move on the 60 Hz tick, so on a faster display they drew in steps.
The smoother draws the body between the last two tick positions
(`alpha = (time − fixedTime) / fixedDeltaTime`). The body's SpriteRenderer and BoxCollider2D share
the root transform, and in Unity 6.4 a collider follows its transform the moment it moves
(`Physics2D.autoSyncTransforms` is obsolete), so the drawn offset exists **only while a frame
renders**: `LateUpdate` computes it, `RenderPipelineManager.beginContextRendering` applies it and
`endContextRendering` removes it. No gameplay, input or physics code runs in that window, so every
probe, slash overlap and collider read sees the true tick position. Order −1000 keeps a fallback
restore at the start of `FixedUpdate`, `Update` and `LateUpdate`. A step over 1 tile snaps instead
of blending.

The first version applied the offset in `LateUpdate` and kept it until the next `Update`; play-mode
verification at ~180 fps caught the collider following the drawn x in 107/150 samples (up to 0.042
tiles). A render probe then proved the render-window approach reaches the image: moving the body
+2 tiles inside `beginCameraRendering` shifted it **42.6 px** on a 640×360 render (expected 42.4),
while transform, tick and collider read the same −2.1000 outside the window.

`EnemyCombatTarget.ResolveKnockback` also gained a 0.0001-tile tolerance: a normalised 2D vector
× 0.5 can measure 0.49999997, which relabelled an H17 parry that broke poise as `H9_Stagger` (same
0.5-tile push, wrong log label).

### §8 enemy push — now applied

SYS3 deviation 8 left `CombatHit.Knockback` computed but unapplied. It is now applied:

- `EnemyCombatTarget` resolves `ResolvedKnockbackTiles` before it raises `Hit`: the hit's own push,
  at least `H9_EnemyKnockbackTiles` on the hit that breaks poise, exactly `H7_BodyFlungTiles` on a
  lethal hit, zero for status kills.
- `HeroCombat.ResolveParry` now sends `away × H17_EnemyKnockbackTiles` on the H17 hit.
- `EnemyAI` slides the body horizontally over `knockbackSeconds`, suspending its own walking. The
  push stops at a wall (body half-width + 0.06 clearance) or at the last supported point before a
  ledge, and a new hit replaces a running push. Every push logs
  `[SYS4] <name> push <kind> requested X → travelled Y tiles`, plus what stopped it.

| Tunable | Value | Source |
|---|---|---|
| `EnemyTuning.knockbackSeconds` | 0.15 s (9 ticks) | gap fill, `[estimated]`: equal to SYS3 `LightFlinchSeconds` |
| `EnemyTuning.deadFadeSeconds` | 0.4 s | gap fill, `[estimated]`: equal to the SYS2 D-3 fade |

### E-DORMANT

E-DEAD no longer freezes the body where it fell: it finishes the kill fling, fades the body's alpha
over `deadFadeSeconds`, then enters **E_DORMANT**, which hides the body and the sign and keeps
contact damage off. Each entry is logged.

### Toolchain

- **EM_Gym camera-tuning restore.** Regenerating EM_Gym brought `WorldCamera.tuning` back as
  `{fileID: 0}`: the first save of the brand-new scene drops that asset reference. `Sys4Tools.BuildGymScene`
  now re-applies it after the first save and saves again; the regenerated scene holds
  `WorldCameraTuning.asset`.
- **CM_Gym builder.** `Sys3Tools.BuildGymContents` still built the pre-SYS5 gym (`GymCameraFollow`,
  a `(40,1)` floor at y −0.5), so a rebuild would have silently undone the Framing pass. It now builds
  the live rig: `WorldCamera` (WorldCameraTuning, `heroFeetLocalY −0.91`, `findRoomBounds`,
  `logTransitions`) with `CombatCameraShake` and URP camera data, `CameraRoomBounds` on the root at
  centre (0, 7) size (40, 26), and the `(40, 6)` floor at y −3. It re-applies the camera tuning after
  the first save too.
- **Sprint Shift removal.** `<Keyboard>/leftShift` is off `Player/Sprint` (it keeps
  `<Gamepad>/leftStickPress` and `<XRController>/trigger`, so Sprint has no keyboard key; nothing
  reads it). Dash keeps `<Keyboard>/leftShift` + `<Gamepad>/rightShoulder`; its Shift binding was
  re-created in the process and has a new binding id.
- **CS0618 cleared.** `Sys3Tools`' obsolete `spritesheet` clear was removed (the mite is Single mode
  with no slices, where that array is unused). Unity 6.4 deprecations also fixed: `WorldCamera`
  (`FindObjectsByType(FindObjectsInactive.Exclude)`, `FindAnyObjectByType`),
  `OpeningLevelRunInitializer` (stores a `SceneHandle`, no int conversion), `BrazierFlicker`
  (`GetEntityId().GetHashCode()` seed).
- Rebuilt in order: `Sys4Tools.BuildPatrollerPrefabs` → `Sys3Tools.BuildGym` → `Sys4Tools.BuildGymScene`.

### Verified in play mode

Two autopilot passes in `EM_Gym` (the second after the smoother and parry-label fixes), driven through
the real keys; no game errors logged.

| Check | Measured |
|---|---|
| Compile | ExitCode 0; no CS0618; only the pre-existing CS0108 on `NexusFeelFeedback.Mote.renderer` |
| Leash | zero `E_ENGAGE`↔`E_SEARCH` flips past the leash, settled in `E_RETURN` (see the SYS4 leash note) |
| SYS4 v0 table | telegraph 24 f fodder / 27 f soldier mode; tokens in use max 1; ledge fodder y 2.45 in every sample; notice 18 f |
| Fodder G1 / G2 / G3 push | 0.25 / 0.25 / 1.0 tiles (HP raised at runtime: a 10 HP fodder dies to one G1) |
| Soldier G3 push | `H2_ChainFinisher` 1.0 → 1.0 |
| Parry that breaks poise | `H17_Parry` 0.5 → 0.5 (was mislabelled `H9_Stagger` before the tolerance fix) |
| Kill fling, open floor | `H7_Kill` 1.5 → 1.5 (fodder ×3, soldier ×2) |
| Kill fling off `Ledge_A` | `H7_Kill` 1.5 → **1.025** `(stopped by ledge)`; a wall stop was not exercised |
| Death | alpha 1 → ~0.12 over ~0.4 s, then `E_DORMANT (from E_DEAD)`, body `SpriteRenderer` disabled |
| Smoother | uncapped 177–250 fps: transform = `TickPosition` = collider x in all 843 single-frame samples; drawn x changed on every consecutive rendered frame except patrol turnarounds and hit-stop |
| Motes | `LandDust` and `SlashSpark` at sortingOrder 12 (hero 10), visible in captured frames |
| `CM_Gym` rebuild | ortho 8.5; camera y 3.000 with feet at 0.000; floor top y 0; `WorldCamera.State` `C_FOLLOW` |
| Sprites | 76 hero frames + mite: mipmaps on (runtime mipmapCount 9), `ignoreMipmapLimit` on, alpha counts unchanged 77/77 |
| Pinstripes | continuous on a 3840×2160 capture (art ≈ 1:1) of walk frame 03; at 1280×720 the hero is ~60 px tall and stripes cannot resolve at all |
