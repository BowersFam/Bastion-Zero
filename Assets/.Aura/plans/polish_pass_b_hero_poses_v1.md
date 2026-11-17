# Polish Pass B — Hero Poses for Existing Mechanics

## Objective
Give every hero mechanic that currently borrows another pose its own HD-2D frames: dash, block and parry, AIR1, WALL1, wall cling and hurt. Also bring the darker G1–G3 attack frames onto the walk/run palette.

## Changes
- `Assets/BastionZero/Editor/NexusArtTools.cs`: new `MatchPaletteToStanding(state)`, an `ActionStates` table with `SliceActionStates()`, and new `BodyRecipes`.
- `Assets/BastionZero/Editor/NexusGridKeyer.cs`: the key list also covers the new `_work/body_<state>_grid.png` sheets.
- `Assets/BastionZero/Editor/NexusAnimatorTools.cs`: new `BuildActionStates()` and menu item `Bastion Zero/Art/Build Nexus Action States` (safe to re-run).
- `Assets/BastionZero/SYS3_Combat/HeroCombat.cs`: publishes `CombatState` (Int) from `SetState`, and `ParryActive` (Bool).
- `Assets/BastionZero/Scripts/HeroAnimationDriver.cs`: draws the body facing `ClingSide` during W_CLING and WALL1.
- `Assets/Art/Characters/Nexus/HD2D/<dash|guard|parry|air_attack|wall_attack|wall_cling|hurt>/` (create): sliced frames.
- `Assets/Animations/Unwinged/Nexus_*.anim` (create): one clip per new state.
- `Assets/Animations/Nexus.controller`: new params and states, added only through the tool.
- `HD2D/_ref/design_spec.md`, `Assets/BastionZero/TUNING_CHANGES.md` and project memories.

## Relevant Assets and Quirks
- **Pipeline** (memory `hd2d-sprite-pipeline`, `_ref/design_spec.md`): generate, key, slice, clip.
  - Every body state shares `BodyReferenceSpan` 486, which gives scale 0.4733.
  - The union's lowest row lands on frame y 14, and each cell's centre line on x 128.
  - Grids: 3×2 at 1536×1024 for 6 poses, 2×2 at 1024×1024 for 4 poses.
- **Every prompt carries:**
  - the HEAD block: smooth dome, ball finial, amber X visor;
  - the palette hexes: plate `#17171a` / `#3a3b40` / `#6f7076`, amber `#f0a020`;
  - amber as pinstripes only, figure facing right;
  - "soles to finial 480 px in every cell";
  - no motion smear and no sword trails;
  - for attacks, cell 00 is a wind-up.
- **Palette.** attack_g1..g3 render near-black, while walk/run show the grey plate. The fix is RGB-only and touches only low-saturation plate pixels; amber, silver and alpha stay the same, so registration does not move.
- **Clip lengths come from tuning, not the art budget:**
  - C-DASH: 12 f (0.20 s);
  - BK-RAISE and BK-RELEASE: 4 f each;
  - light blockstun: 0.10 s; guard break: 0.8 s;
  - parry cover: 6 f (0.10 s);
  - AIR1 and WALL1: 2+6+10 f (0.30 s);
  - H-KNOCK: 0.25 s (CD-S49).
  - A clip's length is its last key plus 1/fps.
- **State numbers.**
  - `CombatState`: K_IDLE 0, G1–G3 1–3, AIR1 4, WALL1 5, C_DASH 6, BK_RAISE 7, BK_HOLD 8, BK_STUN 9, BK_RELEASE 10, BK_BREAK 11.
  - `HeroCombat.SetState` is the one place every combat state change passes through.
  - `MovementState` is already published: W_CLING 6, W_JUMP 7, H_KNOCK 8.
- **Animator rules.**
  - Leave G1/G2/G3's Attack-trigger and AttackChain wiring alone.
  - New states use Int and Bool conditions only. No new Triggers, because a stale trigger fires later.
  - Every transition is 0 s.
  - The old Attack, WingReveal and Glide states and the WingsGranted param must not come back.
- **Cling facing.** `BeginCling` never sets facing, so the sprite keeps whatever input last set, normally toward the wall. `ClingSide` is the jump direction, away from the wall.
  - Cling and WALL1 art face right with the wall behind, so the render must face `ClingSide`.
  - Gameplay `IsFacingLeft` stays unchanged.
- **Clip bindings.** Clips animate only the `VisualRoot` SpriteRenderer `m_Sprite`, with no Transform curves.

## Steps
- [ ] 1. **Palette match.** [depends: none]
  - Add `NexusArtTools.MatchPaletteToStanding`: match the brightness distribution of low-saturation opaque pixels to `standing`'s, keeping coloured accents and alpha.
  - Run it on attack_g1..g3.
  - Write `_work/palette_check.png` (standing and attack side by side) and log mean plate brightness before and after.
- [ ] 2. **Seven new sheets.** [depends: 1]
  - Use `generate_images` for:
    - `dash`: 2×2;
    - `guard`: 3×2, with raise in 00–01, hold in 02–03, stun in 04, break in 05;
    - `parry`: 2×2;
    - `air_attack`: 3×2;
    - `wall_attack`: 3×2;
    - `wall_cling`: 2×2;
    - `hurt`: 2×2.
  - Copy each to `_work/body_<state>_grid.png`, key it, and run `SliceActionStates()` at span 486.
  - Regenerate any sheet whose validate log shows a union height far from 486 or `CLIPPED-AT-CELL-EDGE`.
  - Run the palette match on each new set.
- [ ] 3. **Clips.** [depends: 2]
  - Add recipes for `Nexus_Dash`, `Nexus_GuardRaise`, `Nexus_GuardHold` (loop), `Nexus_GuardStun`, `Nexus_GuardRelease`, `Nexus_GuardBreak`, `Nexus_Parry`, `Nexus_Attack_Air1`, `Nexus_Attack_Wall1`, `Nexus_Cling` (loop) and `Nexus_Hurt`.
  - Pin each length to its tuning window, then run `RebuildBodyClips`.
- [ ] 4. **Animator inputs.** [depends: none]
  - `HeroCombat.SetState` writes `CombatState`.
  - `ParryActive` is true while the parry cover timer is above 0.
  - `HeroAnimationDriver` turns the body toward `ClingSide` during W_CLING and WALL1.
  - Log each published change behind a `logAnimatorStates` toggle.
- [ ] 5. **Controller states.** Add `NexusAnimatorTools.BuildActionStates()` and run it with `execute_script`. [depends: 3,4]
  - Add the `CombatState` and `ParryActive` params.
  - AnyState → Dash, Guard*, AIR1 and WALL1 on `CombatState Equals n`.
  - AnyState → Hurt on `MovementState Equals 8`.
  - Jump, Fall and Takeoff → Cling on `MovementState Equals 6`.
  - GuardHold ↔ Parry on `ParryActive`.
  - Exits fire on `CombatState Equals 0` and pick the next state by Grounded, Speed, VerticalSpeed or MovementState.
  - All transitions 0 s, with `canTransitionToSelf` off.
- [ ] 6. **Verify** with `unity_verification_agent` in CM_Gym and EM_Gym against the Verify list below. [depends: 5]
- [ ] 7. **Docs.** Update the frame-budget table in `_ref/design_spec.md`, add "Art + Animation v3" to `TUNING_CHANGES.md`, and update memories `nexus-animation-input-contract` and `hd2d-sprite-pipeline`. [depends: 6]

## Verify
- **Each input draws its own frames:**
  - Shift: dash frames for 0.20 s.
  - Q: raise, then hold. A blocked hit shows stun, a parry shows the parry frames, a guard break shows the break frames.
  - Attack in the air: AIR1. Attack on a wall: WALL1, facing away from the wall.
  - Wall cling: cling frames, facing away from the wall.
  - Contact damage: hurt frames for the H-KNOCK window.
- **Existing combo:** G1 → G2 → G3 still play 0.30 / 0.30 / 0.45 s, with cancels at 0.23–0.25 s. No spontaneous attacks or states, and `IsInTransition` is never true.
- **Size:** the hero stays 1.797 u tall in every new state, feet on frame y 14.
- **Palette:** in `palette_check.png`, attack plate brightness sits within a small tolerance of standing's, and amber is unchanged.
- The project compiles clean with no console errors.
