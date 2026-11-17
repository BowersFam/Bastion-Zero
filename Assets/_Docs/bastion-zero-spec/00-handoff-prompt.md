# Bastion Zero: opening handoff prompt for the coding AI

Paste everything below the line as your first message. Attach or open the `bastion-zero-spec/` folder in the same session.

---

You are implementing **Bastion Zero**, a 2D platformer-adventure in Unity, from a finished clean-room mechanics spec. Movement feel comes from The Messenger, exploration from Hollow Knight, combat from Dead Cells (without roguelite randomization). The spec is the design. Your job is to turn it into clean, data-driven C# and test scenes, not to redesign it.

## 1. What is in the folder

Read in this order. Do not skim 01.

| File | Use |
|---|---|
| `01-overview-and-decisions.md` | Tags, units, every user decision, how to read the tables. **Decisions always win.** |
| `03-dependency-graph.md` | What each system feeds into. Sets build order. |
| `04-glossary.md` | Neutral names. Use them for classes, enums and UI strings. |
| `05` to `10` | One numeric table per system (IDs like `MV-01`, `CD-xx`, `CF-xx`, `EN-xx`, `WC-xx`, `PE-xx`). |
| `systems/system-2..6-*.md` | Full rule text for each system. The tables say *what number*; these say *how it behaves*. |
| `11-conflict-register.md` | If two places disagree, this file says which wins. |
| `12-open-questions.md` | Playtest questions. **Implement the suggested default; do not block on them.** |
| `13-vertical-slice.md` | The scope of the first build. Implement this first and nothing else. |
| `02-sources.md`, `15-section-index.md` | Reference only. |
| `bastion-zero-consolidated-spec.md` | The same content as one file, for searching. |

**Known gap:** the System 1 rule file (`system-1-player-movement.md`) is not in this folder. `05-movement.md` has every movement number (`MV-01` to `MV-44`) and the other systems restate the movement states they depend on (slice list in `13`, §5.1). Where movement *behaviour* is unclear beyond those numbers, ask me one targeted question; do not guess.

## 2. Rules of engagement

1. **No invented mechanics.** If it is not in the spec, it is not in the build. Anything listed under "deferred" in `13-vertical-slice.md` §5.7 or excluded in a system's exclusion list stays out.
2. **Precedence:** user decisions (`01`) > owning system > later system. Check `11` before assuming a conflict is a bug.
3. **Tags matter.** `[sourced]` and `[community-measured]` are reference facts. `[estimated]` is a Bastion Zero default, expected to be tuned. Every `[estimated]` number must be a tunable field, never a magic number in code.
4. **Clean-room.** Do not use names, art, text or assets from the reference games in code, comments, scene names or placeholders. Use the generic terms in `04-glossary.md`.
5. **Units:** 60 fps fixed timestep, 1 tile = 16 px, hero height = 2 tiles. Spec timings given in frames are frames; timings in seconds convert at 60 fps. Keep spec units in code and convert in one place.
6. **Traceability.** Name states, enums and tuning fields after spec IDs (for example `G_RUN`, `C_DASH`, `MV_01_RunSpeed`) and put the ID in a short comment so a value can be traced back.
7. **One question at a time.** If something blocks you, ask a single targeted question with your proposed answer. Do not stack questions.

## 3. Architecture requests

- Tuning lives in ScriptableObjects (or a similarly editable asset), one per system, keyed by spec ID. Include a change log field or a `TUNING_CHANGES.md` so edits to `[estimated]` values are recorded.
- Kinematic character motor with continuous collision (the spec depends on a safe-ground recorder, knockback timing, coyote time and buffers).
- Explicit state machines for movement, attack overlay, defence (dash, block, parry), death (D-1 to D-5) and progression overlay, with the state names from the spec.
- Attacks are an overlay: they never block movement unless the spec says so.
- Save system hooks for persistence classes P0 to P3 (see `10-progression-economy.md`); the slice needs rest-point full writes, checkpoints and the death marker.
- Debug tooling from day one: a hitbox and hurtbox overlay, a state readout, frame stepping, and a room-graph reachability check (the spec requires a no-softlock check).

## 4. Build order

Follow the dependency graph, with one adjustment: the camera and room bounds (SYS5) must exist before enemy off-screen checks (SYS4).

1. Project setup, units, fixed timestep, tuning assets, debug overlays.
2. **SYS1 Movement** (`05`): slice states only.
3. **SYS2 Collision and damage** (`06`, `system-2`): pips, hazards, pits with safe-ground warp, death states.
4. **SYS3 Combat** (`07`, `system-3`): ground attacks, bounce, dash (ground only), block and parry, hit feel.
5. **SYS5 World and camera** (`09`, `system-5`): rooms, transitions, camera states, gates, rest points, checkpoints, map hybrid.
6. **SYS4 Enemies** (`08`, `system-4`): the slice roster, telegraphs, respawn classes, elite, small boss.
7. **SYS6 Progression** (`10`, `system-6`): coin, death marker, one shop, flask, wall-cling unlock, saves.
8. Assemble the 13 slice rooms (VS-01 to VS-13) and run the success criteria VS-S1 to VS-S11 as far as they can be checked without playtesters (VS-S9 and VS-S10 are checkable by tooling).

## 5. How each step works

For every step, in this order:

1. List the files and IDs you are about to use.
2. Restate the behaviour in 10 bullets or fewer. I will correct anything wrong.
3. Ask at most one blocking question, with your proposed answer.
4. Implement, with a small test scene that exercises it.
5. Report: IDs implemented, IDs skipped on purpose (deferred), any deviation, and how I can verify it in under two minutes.

Stop after each step and wait for me to say continue. Do not start the next system on your own.

## 6. Your first reply

Do not write code yet. Reply with:

1. Confirmation that you read `01`, `03`, `04` and `13`.
2. A 10-bullet summary of the project and the slice, in your own words.
3. The folder and assembly layout you propose for the Unity project.
4. The single most important question you have about Step 1 (SYS1 movement), with your proposed answer.
