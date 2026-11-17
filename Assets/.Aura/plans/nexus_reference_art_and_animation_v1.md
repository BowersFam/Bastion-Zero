# Nexus Reference Art + Smooth Animation

## Objective
Rebuild Nexus to the archon reference sheet (tall 7.5-head silhouette, fine amber pinstripe trim, smooth dome helm with X visor, cape, slender blade), with 8-frame cycles timed on exact 12 fps keys and wings on a separate animated layer so the wing unlock never changes resolution.

## Changes
- `Assets/Art/Characters/Nexus/HD2D/_ref/nexus_archon_reference.png` — the reference sheet on disk, single art source of truth.
- `Assets/Art/Characters/Nexus/HD2D/_ref/design_spec.md` — (create) written design contract derived from the reference; every state grid prompt is generated from this file.
- `Assets/BastionZero/Editor/NexusArtTools.cs` — generalise the grid slicer from hard-coded 2×2 to `cols × rows × frameCount`; replace the `oldBox × 4` remap with explicit per-set target boxes; add the wing set (512 px canvas) and a `Path` argument so one tool serves body and wings.
- `Assets/Art/Characters/Nexus/HD2D/<state>/00..07.png` — regenerate the six body states at 8 frames, 256 px @ PPU 128.
- `Assets/Art/Characters/Nexus/HD2D/wings/<state>/00..07.png` — (create) three wing states (`idle`, `flap`, `glide`) at 512 px @ PPU 128.
- `Assets/Animations/Unwinged/Nexus_*.anim` — rebuild all eight sprite curves on exact 1/12 s keys, drop the duplicate tail key in `Nexus_Walk`.
- `Assets/Animations/Wing/Nexus_Wing_Idle.anim`, `Nexus_Wing_Flap.anim`, `Nexus_Wing_Glide.anim` — (create) sprite curves binding `WingRoot`/`SpriteRenderer`/`m_Sprite`.
- `Assets/Animations/Nexus.controller` — add a `Wings` layer with those three states, add `WalkCycleSpeed` / `RunCycleSpeed` float params, zero out cross-dissolve durations on sprite states.
- `Assets/Scripts/HeroAnimationDriver.cs` — (create) maps `PlayerMovement.Velocity` onto the two cycle-speed params.
- `Assets/Scripts/NexusWingAppearance.cs` — replace the `AnimatorOverrideController` path with wing-layer weight 0→1 plus the reveal trigger.
- `Assets/BastionZero/Tuning/PlayerAnimationTuning.cs` + `.asset` — (create) `AN_01_WalkCycleDistance`, `AN_02_RunCycleDistance`, `AN_03_MinPoseHoldFrames`, `AN_04_WingFlapRateScale`.
- `Assets/BastionZero/TUNING_CHANGES.md` — art + animation v1 section.

## Relevant Assets and Quirks
- **Pivot maths (do not drift).** `VisualRoot` is at `(0,0,0)` with `sortingOrder 10`; body canvas 256 px @ PPU 128 = 2 u, centre pivot puts canvas bottom at y −1.0. Feet must stay at y −0.891 — derive from `GroundCheck` y −0.906, collider offset −0.1875, half-height 0.7185. Keep feet 12 px above the canvas bottom (1.797 u figure).
- **Wing canvas needs no custom pivot** because it is its own object: 512 px @ PPU 128 = 4 u, centre pivot, `WingRoot` at `(0,0,0)` covers ±2 u — wings need ±1.4 u wide, −0.2..+1.6 u tall.
- **A layer only drives the paths its clips bind.** The `Wings` layer needs no AvatarMask: wing clips bind `WingRoot/SpriteRenderer/m_Sprite`, body clips bind `VisualRoot`. `modify_animator_controller`'s `add_layer` creates the layer at weight 1 — `NexusWingAppearance.Awake` must set `animator.SetLayerWeight(1, 0f)`.
- **`NexusArtTools` current shape:** constants at lines 18–36; `SliceGrid` lines 90–150 requires `_work/<state>_grid_edited.png` and maps `newBox` onto `oldBox × 4`; `Blit` line 380 flips Y (Unity textures are bottom-up, so grid reading order is 0..3 on the *top* row); `ApplySpriteSettings` line 343 sets PPU 128 / Point / Uncompressed / FullRect / centre pivot.
- **Stutter sources confirmed in the assets:** `Nexus_Walk.anim` has 5 keys with a duplicate `03` at 0.4167 s — a loop-seam hitch; all four poses hold 125 ms, i.e. effective 8 fps. Controller transitions `Fall`/`Takeoff`/`Land`/`Attack`/`Glide` run 0.05–0.08 s, which cross-dissolves two sprites and reads as ghosting; grounded Idle↔Walk↔Run are already 0.
- `Animator.m_CullingMode: 0` (AlwaysAnimate) on `Hero.prefab` — keep it.
- `PlayerMovement.Velocity` is public (line 123) and `PublishAnimator` (line 1008) owns the existing params. Do not modify the motor.
- `MV_01_RunSpeed = 8.5`; controller switches Walk→Run at `Speed > 3.5`, Run→Walk at `< 3`.
- `WingsGranted` trigger drives AnyState→`WingReveal`; `WingUnlockState.IsUnlocked` gates the silent re-apply at spawn.
- **Grid geometry:** body state grids 1536×1024 = 4 cols × 2 rows of 384×512 px; wing state grids 1024×1536 = 2 cols × 4 rows of 512×384 px. One `edit_images` call per grid — never per frame, or limbs wobble between poses.
- **Design contract from the reference:** profile facing right; 7.5 heads to 1.797 u; helm is a smooth dome with a small finial and a thin amber X visor, no horns; amber is ~10 % of area as pinstripe along plate edges, deep orange-gold with brown-orange shadow, never a bright lemon fill; slender blade with a gothic crossguard held low; cape with amber-trimmed scalloped hem; hard-edged pixels, fully transparent background.

## Steps
- [x] 1. Save the posted reference to `Assets/Art/Characters/Nexus/HD2D/_ref/nexus_archon_reference.png` and write `_ref/design_spec.md` from it (proportions, helm, trim, sword, cape, palette hexes) [depends: none]
- [x] 2. Generalise `NexusArtTools.cs`: `BuildGrid(path, state, cols, rows, count)` / `SliceGrid(path, state, cols, rows, count, outW, outH, targetBox)` with explicit target boxes, keep `Blit`'s Y flip and `ApplySpriteSettings`, parameterise PPU [depends: 1]
- [x] 3. Build the six body grids and `edit_images` each against `nexus_archon_reference.png`, then slice to 256 px / 8 frames — approve `standing` before running the other five [depends: 2]
- [x] 4. Build and restyle the three wing grids from the same reference, slice to 512 px / 8 frames, register `WingRoot` under `VisualRoot` on `Hero.prefab` with a `SpriteRenderer` at `sortingOrder 9` [depends: 2,3]
- [x] 5. Rebuild the eight `Unwinged/Nexus_*.anim` clips at 8 frames on exact 1/12 s keys (no duplicate tail key, length = frames ÷ 12), create the three `Animations/Wing/Nexus_Wing_*.anim`, and add the `Wings` layer + `WalkCycleSpeed`/`RunCycleSpeed` to `Nexus.controller` with all sprite-state transition durations set to 0 [depends: 3,4]
- [x] 6. Add `PlayerAnimationTuning` + `HeroAnimationDriver.cs` (cycle speed = velocity ÷ `AN_0x` cycle distance, clamped by `AN_03_MinPoseHoldFrames`), and rewrite `NexusWingAppearance.ApplyWings` to set layer weight 1 + trigger the reveal instead of swapping a controller [depends: 5]
- [x] 7. Log art/animation v1 in `TUNING_CHANGES.md`, delete `Tools/ArtRun.cs`, and verify in `CM_Gym` [depends: 6]
      Verified 2026-11-16 by unity_verification_agent in CM_Gym: new 256 px HD2D sprites drawn (never the 64 px terminal-guardians art), Idle/Walk/Run each cycle all 8 frames in order and wrap, 41/41 transitions at 0 s with IsInTransition never true, Wings layer weight 0 and WingRoot sprite None at start, HeroAnimationDriver writes WalkCycleSpeed/RunCycleSpeed from PlayerAnimationTuning (AN_01 2.4, AN_02 5.6, AN_03 3, AN_04 1; 0.826/0.354 at 2.98 u/s, 1.667/1.012 at 8.5 u/s). Not covered: the wing-gem unlock path — CM_Gym has no gem, so the Wings reveal and the `WingReveal`/`Glide` Base Layer states are still unobserved and still carry 64 px art.

## Verify
- `check_compile_errors` clean, then `unity_verification_agent` play-mode check in `Assets/BastionZero/Scenes/CM_Gym.unity`.
- Stand still, walk, sprint, jump, fall, attack: no sprite ghosting during transitions, no foot slide at sprint, and no visible loop hitch on the walk.
- Collect the wing gem: hero must not change resolution or shift position — wings fade in on `WingRoot` only; `WingRoot` disappears entirely before unlock.
- `Debug.Log` in `HeroAnimationDriver` prints the resolved cycle speed and `SetLayerWeight` value once on unlock.
- Screenshot the hero at `standing` and compare against `_ref/nexus_archon_reference.png` for head count, trim weight, helm shape and blade form.
