# Polish Pass A — Clean Motion, Hit Impact, Toolchain

## Objective
Make what already exists read cleanly in motion and land with weight, and close the toolchain gaps that could silently undo earlier fixes. That means stable sprite sampling, smooth enemy movement, visible feel motes, the §8 enemy push, and the kill fling with E-DEAD → E-DORMANT.

## Changes
- `Assets/BastionZero/Editor/NexusGridKeyer.cs`: the flatten pass keeps each pixel's own alpha. New `RepairEdgeMattes(paths)` covers every body state, every wing state and `Assets/Generated_Images/cinder_mite.png`.
- `Assets/BastionZero/Editor/NexusArtTools.cs`: `ApplySpriteSettings` keeps Point and turns on `mipmapEnabled` and `ignoreMipmapLimit`. New `ReapplyImportSettings()` and menu item `Bastion Zero/Art/Reapply Sprite Import Settings`.
- `Assets/BastionZero/Editor/Sys3Tools.cs`: `ImportMiteSprite` gets the same sampling. Drop the obsolete `importer.spritesheet` line (CS0618). `BuildGymContents` rebuilds CM_Gym the way the live scene is.
- `Assets/BastionZero/Editor/Sys4Tools.cs`: `BuildOne` adds `EnemyRenderSmoother` to both patroller prefabs.
- `Assets/BastionZero/SYS4_Enemies/EnemyRenderSmoother.cs` (create): render interpolation for enemies moved by transform.
- `Assets/BastionZero/SYS4_Enemies/EnemyAI.cs` and `EnemyTuning.cs`: push and fling, E-DEAD → E-DORMANT, two gap-fill fields.
- `Assets/BastionZero/SYS3_Combat/EnemyCombatTarget.cs`: new `ResolvedKnockbackTiles`. `HeroCombat.cs`: H17 parry knockback.
- `Assets/Scripts/NexusFeelFeedback.cs`: mote sorting order and dust spawn offsets.
- `Assets/BastionZero/Prefabs/Hero.prefab`: `VisualRoot` sprite → `Assets/Art/Characters/Nexus/HD2D/standing/00.png`.
- `Assets/InputSystem_Actions.inputactions`: remove `<Keyboard>/leftShift` from `Player/Sprint`.
- `Assets/BastionZero/TUNING_CHANGES.md` and project memories: a polish section, plus fixes to stale rows.

## Relevant Assets and Quirks
- **Sprite sampling.** Hero frames are 256 px at PPU 128, Point filter, no mipmaps. The mite is 1024 px at PPU 512. At ortho 8.5 on 1080p (63.5 px/unit) that is about 2× and 8× minification. Point sampling skips texels, so pinstripes shimmer as sprites move. Point + mipmaps keeps the design spec's "hard-edged pixels" rule but samples a pre-filtered level.
- **Mipmap limit.** The editor runs quality level 0 "Very Low", which has `globalTextureMipmapLimit: 1`. Without `ignoreMipmapLimit` that halves every mipmapped texture.
- **Bleed flatten pass.** `BleedEdgeColour` writes alpha 0 into every unclaimed pixel. That breaks the "alpha is never written" rule for any semi-transparent pixel more than 24 px from the figure.
- **Enemy stepping.** Patrollers move by `transform.position` in `FixedUpdate`, with no Rigidbody2D (memory rule, keep it). So they step at 60 Hz while the hero (Rigidbody2D Interpolate) and the camera move smoothly. `Physics2D.autoSyncTransforms` is off.
- **Enemy knockback.** `HeroCombat.BuildHit` computes the H1–H4 push into `CombatHit.Knockback`, and `EnemyCombatTarget.LastHit` stores it, but nothing reads it.
  - H9 (0.5), H7 (`H7_BodyFlungTiles` 1.5) and H17 (0.5) are never computed. `ResolveParry` sends `Knockback` = 0.
  - Spec `E-DEAD → E-DORMANT`, but today dead enemies stay on screen forever, tinted `deadTint`.
- **Hit order.** `EnemyCombatTarget.ReceiveHit` runs `ApplyHitStun` before raising `Hit`, and raises `Hit` before `Kill()`. So a poise break and a lethal outcome are both known before SYS4 hears the hit.
  - SYS3 keeps the numbers; SYS4 only moves the body.
  - Enemies have no gravity, so a push must stop at walls and ledge edges (`WallAhead` / `GroundAhead`).
- **Feel motes.** `NexusFeelFeedback.SpawnMote` hard-codes `sortingOrder = 9` (the hero is 10), and dust spawns within ±0.3 u. Both break the `feel-feedback-mote-visibility` convention (hero order + 2, at least ±0.6 u).
- **CM_Gym builder drift.** `Sys3Tools.BuildGymContents` still builds `GymCameraFollow` and a `(40,1)` floor at y −0.5. The live CM_Gym has:
  - `WorldCamera` with tuning `7a96dea4…`, `heroFeetLocalY −0.91`, `findRoomBounds 1` and `logTransitions 1`;
  - root `CameraRoomBounds` with centre (0,7) and size (40,26);
  - a `(40,6)` floor at y −3.
  - Re-running SYS3/3 today would undo the framing fix.
- **Hero default sprite.** `VisualRoot` still points at the old `Nexus_Talos…/standing/00.png` (guid `74335a4f…`). The HD-2D `standing/00.png` is guid `8d2065f1…`. No scene overrides it.
- **Regeneration rule.** Gyms and prefabs are rebuilt from their menu methods, never edited by hand. Drive them with `execute_script` on the named static methods.

## Steps
- [x] 1. **Sprite sampling.** [depends: none]
  - Make the flatten pass keep alpha, then run `RepairEdgeMattes` on every body frame, every wing frame and the mite.
  - In `ApplySpriteSettings` and `ImportMiteSprite`, keep Point and turn on mipmaps and `ignoreMipmapLimit`, then run `ReapplyImportSettings`.
  - Log each frame's opaque, semi-transparent and clear pixel counts before and after; they must match exactly.
  - Use `set_property` to point `Hero.prefab/VisualRoot` at HD-2D `standing/00.png`.
  - Note: 77/77 images (52 body, 24 wing, mite) repaired, alpha byte-identical; 77/77 now Point + mipmaps + ignoreMipmapLimit. The 59 non-attack images peak at alpha 253–254 (no a = 255 seed), so the bleed leaves them byte-identical; their clear pixels already carry dark figure-tone RGB.
- [x] 2. **Smooth enemies and visible motes.** [depends: none]
  - Create `EnemyRenderSmoother` with `[DefaultExecutionOrder(-1000)]`:
    - put the transform back at its true tick position at the start of `FixedUpdate` and `Update`;
    - in `LateUpdate`, draw it at `Lerp(prevTick, curTick, (Time.time − Time.fixedTime) / Time.fixedDeltaTime)`;
    - snap instead of blending on any jump over 1 tile.
  - Add the smoother in `Sys4Tools.BuildOne`.
  - Draw motes at the hero's `sortingOrder + 2` (11 if the hero renderer is not found), and spawn dust at least ±0.6 u to each side.
- [x] 3. **Enemy push and death.** [depends: none]
  - `HeroCombat.ResolveParry` sets `Knockback = away × H17_EnemyKnockbackTiles`.
  - `EnemyCombatTarget` sets `ResolvedKnockbackTiles` before it raises `Hit`. It is the hit's own push, at least H9 on the hit that breaks poise, exactly H7 on a lethal hit, and zero for status kills.
  - `EnemyAI` applies that push horizontally over a new `EnemyTuning.knockbackSeconds`, suspending its own walking. The push stops at walls and ledges.
    - `knockbackSeconds` is a gap fill of 0.15 s, equal to SYS3 `LightFlinchSeconds`.
  - E-DEAD plays the fling, then fades the body over a new `deadFadeSeconds`, then enters E-DORMANT and hides the body and the sign.
    - `deadFadeSeconds` is a gap fill of 0.4 s, equal to the SYS2 D-3 fade.
  - Log `[SYS4] <name> push <kind> requested X → travelled Y tiles` on every push, and log each E-DORMANT entry.
- [x] 4. **Toolchain.** [depends: 1,2,3]
  - Make `Sys3Tools.BuildGymContents` build the live CM_Gym camera, room bounds and floor values.
  - Delete the `spritesheet` line.
  - Use `remove_bindings` to take `<Keyboard>/leftShift` off `Player/Sprint`.
  - Regenerate with `execute_script` in this order: `Sys4Tools.BuildPatrollerPrefabs`, `Sys3Tools.BuildGym`, `Sys4Tools.BuildGymScene`. EM_Gym should be the open scene at the end.
  - Run `check_compile_errors`: 0 errors and no CS0618.
- [x] 5. **Docs and memory.** [depends: 4]
  - Add a "Polish pass" section to `TUNING_CHANGES.md` covering:
    - the sampling change and why;
    - the smoother;
    - the §8 enemy push now applied, and the two gap fills;
    - E-DORMANT;
    - the EM_Gym camera-tuning restore and the edge-matte repair;
    - the CM_Gym builder fix and the Sprint Shift removal.
  - Mark these rows as superseded: SYS3 deviations 5 and 7, SYS5 "orthographic size 6.75", and the Art v2 note that G3 runs at 13.333 fps.
  - Update memories `hd2d-sprite-pipeline`, `sys4-enemy-layer`, `sys3-combat-layer`, `nexus-animation-input-contract` (G3 is 20 fps with KeyTimes) and `feel-feedback-mote-visibility`.
- [x] 6. **Verify** with `unity_verification_agent` against the Verify list below. Stop and report. Do not start Plan B on your own. [depends: 4]

## Verify
- The project compiles clean and the console has no CS0618.
- **EM_Gym leash test (pending since SYS4):** push a patroller past its leash with the hero in detection range and sample every fixed tick for at least 3 s.
  - Expect zero E_ENGAGE↔E_SEARCH flips, settling in E_RETURN.
  - The SYS4 v0 table in `TUNING_CHANGES.md` still measures the same: telegraph 24/27 f, tokens ≤ 2, ledge fodder at y 2.45, notice 18 f.
- **Push distances:**
  - fodder G1/G2: 0.25 tiles; G3: 1.0;
  - soldier poise break: at least 0.5;
  - parry: 0.5;
  - kill fling: 1.5 tiles (shorter only when a wall or ledge stops it, and the log says so).
  - The dead body fades out and stops drawing (E_DORMANT).
- A walking patroller's drawn x changes on every rendered frame at an uncapped frame rate, and its collider stays at the true tick position.
- Landing dust and hit sparks draw at sortingOrder 12 and show up in a captured frame.
- **Regenerated CM_Gym:** ortho 8.5, camera y = feet + 3.000 at rest, floor top at y 0, `WorldCamera.State` C_FOLLOW.
- **Every HD-2D frame and the mite:** mipmaps on, `ignoreMipmapLimit` on, alpha counts unchanged. A game-view capture of the hero mid-walk shows unbroken pinstripes.
