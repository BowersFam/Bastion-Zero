# Bastion Zero spec — Overview, tags, and user decisions

> Part of the Bastion Zero clean-room mechanics spec v3.2 (14 Nov 2026). READ FIRST. Defines the tag system, units, every user decision (these always win), how to read the numeric tables, and the old-code mapping.
> Rule text owner: All six system files.
> Precedence: user decisions (file 01) > owning system > later system. Tags: [sourced], [community-measured], [estimated] (file 01).

---

# Bastion Zero: Consolidated Mechanics Spec, Systems 1–6 (clean-room)

Version 3.2 · 14 Nov 2026 (UTC+8). Version 3.2 records the user's confirmation of decision (g) and resolves CR37; Version 3.1 is backed up as `bastion-zero-consolidated-spec.md.pre-g`. Version 3.1 applies the final decisions FD-1–FD-10 (§0.1), including the CR34 tag policy, and leaves no open user decisions; Version 3 is backed up as `bastion-zero-consolidated-spec.md.pre-final`. Consolidates the latest versions of the six system specs. Version 2 applied System 3 decisions (a)–(e) (no roll; dash and block); Version 3 applies DB-1–DB-4 (dash i-frames, parry, air-dash unlock, no 1-tile tunnels). Backups: `bastion-zero-consolidated-spec.md.v1` (Version 1), `bastion-zero-consolidated-spec.md.pre-db` (Version 2). SYS1 and SYS2 are unchanged:

| System | File | Version used |
|---|---|---|
| SYS1 Player movement | `system-1-player-movement.md` | Version 1, 13 Nov 2026 |
| SYS2 Collision and damage | `system-2-collision-damage.md` | Version 2.1, 14 Nov 2026 |
| SYS3 Combat | `system-3-combat.md` | Version 3.1, 14 Nov 2026 |
| SYS4 Enemy AI and encounters | `system-4-enemy-ai.md` | Version 2.4, 14 Nov 2026 |
| SYS5 World and exploration | `system-5-world-exploration.md` | Version 2.3, 14 Nov 2026 |
| SYS6 Progression and persistence | `system-6-progression-persistence.md` | Version 2.3, 14 Nov 2026 |

References: The Messenger (TM), Hollow Knight (HK), Dead Cells (DC).

**Consolidation rules used here**
- Consolidation only: no new mechanics and no new research. Every value, tag and citation is carried over from the system that owns it, except where the CR34 tag policy (FD-8) retags a copied default.
- Anything new in this document (vertical-slice counts, suggested defaults, success targets) is tagged [estimated] and labelled **Recommendation**.
- User decisions win; otherwise the later or owning system governs. Each conflict row says **RESOLVED (rule)** or **FLAGGED**.
- No names of characters, items, areas, currencies or abilities. URLs may contain names, by precedent.

### Tags and markers
- **[sourced]**: stated in a public source (named by code in §0.2).
- **[community-measured]**: counted, timed or polled by third parties.
- **[estimated]**: a Bastion Zero value, a user design decision, or arithmetic on other values.
- **"provenance unclear"**: a wiki number that may come from game data. Datamined, decompiled and game-file values are excluded throughout.
- **Copied defaults (CR34 policy, FD-8, decided 14 Nov 2026):** a Bastion Zero default copied from a reference is tagged [estimated] with the reference as basis, e.g. "[estimated] (basis: S4)"; the reference value itself (Source column) stays [sourced]. This overrides the "carry every tag over" rule above. SYS1 is not edited, so its copied defaults keep [sourced] in that file (X29); the SYS1-mirrored rows here use the new policy.
- User-decided defaults are never tagged [sourced]. In tables with a Tag column, the tag applies to every number in that row unless a number carries its own tag.
- **Units:** 60 [estimated] fps; 1 tile = 16 [estimated] px; hero height H = 2 [estimated] tiles.

### Code prefixes used in this document
- **SYS1–SYS6**: the six system specs (the source files call them "S1-doc" … "S6-doc").
- **S1–S93**: public sources (§0.2). These codes were already globally unique across the six files.
- **X1–X29**: known cross-system differences; **CR24+**: further conflicts found during consolidation (§3.7).
- **MV / CD / CF / EN / WC / PE**: merged parameter IDs (§3). CF-DA* = dash, CF-BK* = block and parry (Versions 2–3).
- **DB-1–DB-6**: dash/block follow-on decisions, all decided 14 Nov 2026 (§0.1). **FD-1–FD-10**: final decisions on the remaining open items, 14 Nov 2026 (§0.1).
- **OQ-B / OQ-H / OQ-M / OQ-L**: combined Open Questions by rank (§4).
- **VS-**: vertical-slice items (§5).

---

## 0. Overview

### 0.1 User decisions (all systems)
| ID | Date | System | Decision | Governs |
|---|---|---|---|---|
| D2-1 | 14 Nov 2026 | SYS2 | Bottomless pits in exploration areas: damage plus a warp to the last safe ground, not instant death | CD-31, CD-32, X3 |
| D2-2 | 14 Nov 2026 | SYS2 | Per-hazard re-hit lockout adopted at 0.5 [estimated] s | CD-25, X2 |
| D2-3 | 14 Nov 2026 | SYS2 | Respawn health: full | CD-33, X4 |
| D2-4 | 14 Nov 2026 | SYS2 | Max health: 10 [estimated] pips | CD-18, X1 |
| D2-5 | 14 Nov 2026 | SYS2 | Collapsing platforms come back | CD-16 |
| D2-6 | 14 Nov 2026 | SYS2 | Contact damage on by default | SYS2 §6; EN contact rows |
| D2-7 | 14 Nov 2026 | SYS2 | Leave System 1 as is: all six proposed System 1 edits declined | All X rows ("no SYS1 edit") |
| (f) | 14 Nov 2026 | SYS4 | All four respawn classes approved, including R3 (off-screen respawn for traversal anchors) | EN-RS1 |
| (g) | 14 Nov 2026 | SYS4 | Universal attack warning sign **on by default** (confirmed by the user; CR37 resolved) | EN-T1 |
| (h) | 14 Nov 2026 | SYS4 | Enemies that give up the chase keep their damage (no healing on return) | EN-A10 |
| (i) | 14 Nov 2026 | SYS4 | Bosses use weighted random attack choice with no-repeat rules; no scripted-cycle mode | EN boss rows |
| (j) | 14 Nov 2026 | SYS4 | No bounce from hitting a shield in the air; X12 declined | X12 |
| (k) | 14 Nov 2026 | SYS4 | Elite clone modifier reinstated as a designer-placed option (M6), never rolled | EN-M6 rows |
| (l) | 14 Nov 2026 | SYS5 | Map model hybrid: dotted sketch on entry, inked at the next rest | WC-M1, WC-M2 |
| (m) | 14 Nov 2026 | SYS5 | World shape: linear corridor act, then the world opens (15–25% [estimated] of rooms) | WC-N2 |
| (n) | 14 Nov 2026 | SYS5 | Camera: small look-ahead in the direction of travel (X16 decided) | WC-W4, X16 |
| (o) | 14 Nov 2026 | SYS5 | Seen gates auto-marked on the map by gate type | WC-M1, WC-M2 |
| (p) | 14 Nov 2026 | SYS5 | World size approved: 180–260 [estimated] rooms, 8–12 [estimated] regions, 8–12 [estimated] hours | WC-G1, WC-G2, WC-P1 |
| (q) | 14 Nov 2026 | SYS5 | No late-game region transformation | WC-P10 |
| (r) | 14 Nov 2026 | SYS6 | One persistent coin with set, fixed values per source | PE-C rows |
| (s) | 14 Nov 2026 | SYS6 | Death penalty P-C: half of unspent coin drops into a recoverable marker | PE-D rows, X19, X21 |
| (t) | 14 Nov 2026 | SYS6 | One shop reached through doors at rest points | PE-S rows |
| (u) | 14 Nov 2026 | SYS6 | Fixed-charge healing flask (partly reinstates SYS3 E8 and E14) | PE-F rows, X23 |
| (v) | 14 Nov 2026 | SYS6 | Coin fee only for weapon tiers T2–T5 (no tier materials) | PE-S rows, X20 |
| (w) | 14 Nov 2026 | SYS6 | Cooldown reduction stays excluded (SYS3 E6) | CF-K5 |
| (x) | 14 Nov 2026 | SYS6 | Trinkets and notches: optional, add later (deferred post-v1) | PE-T rows |
| (y) | 14 Nov 2026 | SYS6 | Map, toll and fast-travel prices stay excluded (SYS5 WE10) | PE-V rows |
| (z) | 14 Nov 2026 | SYS6 | Stat growth stays excluded (SYS3 E5) | PE rows |
| (a) | 14 Nov 2026 | SYS3 | No: nothing dodges traps; hazards ignore invulnerability (SYS2 §8 rule 2 stands) | CR26, WC-K11, EN-M2, EN-M3 |
| (b) | 14 Nov 2026 | SYS3 | No roll, only dash and block: the shield slot B archetype is removed; the block is a core action | CF-BK rows, CF-L2, X25, WC-K13 |
| (c) | 14 Nov 2026 | SYS3 | No roll, only dash and block: roll and air roll removed; the dash works on the ground and in the air | CF-DA rows, X9, X11, X24, X26, MV-D2, MV-D3 |
| (d) | 14 Nov 2026 | SYS3 | Three deterministic crit triggers approved at 2× | CF-X6, CF-D4 |
| (e) | 14 Nov 2026 | SYS3 | Four enemy-only statuses approved; no player statuses | CF-S1–CF-S6, EN-T7 |
| DB-1 | 14 Nov 2026 | SYS3 | Dash i-frames: 8 f, enemy sources only, never hazards (placed on f3–f10 [estimated]) | CF-DA8, CF-DA10, EN-K3, X27 |
| DB-2 | 14 Nov 2026 | SYS3 | Parry on: perfect-timing window in the first 8 f after the guard is raised | CF-BK12–CF-BK18, CF-H17, EN-K4, EN-X4 |
| DB-3 | 14 Nov 2026 | SYS3 | Air dash is a later unlock; ground dash stays base kit; no jump right after a ledge dash | CF-DA1, CF-DA6, CF-DA11, MV-D2, MV-D3, WC-K3, WC-K5, WC-K19–WC-K21, PE-U11, X24, X28 |
| DB-4 | 14 Nov 2026 | SYS3 | 1-tile tunnels dropped entirely | CF-DA9, X26; GT10 removed |
| FD-1 (DB-5) | 14 Nov 2026 | SYS3 | Reflected (parried) shots are not bounce targets | CF-BK15; OQ-L15 resolved |
| FD-2 (DB-6) | 14 Nov 2026 | SYS3 | No guard refund on a parry (confirmed) | CF-BK13; OQ-L16 resolved |
| FD-3 | 14 Nov 2026 | SYS4 | Parry-stagger cap kept at 3.0 [estimated] s. The user wrote "No Stat lock"; reading it as "no stun lock" (keep the cap that prevents parry locks) is the parent agent's **interpretation** of the user's words | EN-K4; OQ-M15 (cap part) |
| FD-4 | 14 Nov 2026 | SYS4 | At least 1 [estimated] unblockable attack per boss phase from phase 2 (confirmed) | EN-K4; OQ-M18 resolved |
| FD-5 | 14 Nov 2026 | SYS5 | GT17 is the low-ceiling gap as written, 6–8 tiles wide (confirmed) | WC-K19; OQ-H10 (shape part) |
| FD-6 | 14 Nov 2026 | SYS6 | The air dash comes from an ability room behind an elite (confirmed) | PE-U11; OQ-M17 (source part) |
| FD-7 | 14 Nov 2026 | Slice | Air dash excluded from the vertical slice (confirmed) | §5.1, §5.7, §5.8 |
| FD-8 | 14 Nov 2026 | All | CR34 tag policy: Bastion Zero defaults copied from a reference are tagged [estimated], basis cited; SYS1 left untouched (X29) | Tags and markers; CR34; X29 |
| FD-9 | 14 Nov 2026 | Slice | Death marker on in the vertical slice (world-phase flag WP-OPEN) (confirmed) | §5.4 |
| FD-10 | 14 Nov 2026 | Slice | The slice elite is built on a regular (fodder) enemy (confirmed) | §5.2, VS-07 |

SYS1 (13 Nov 2026) contains no user decisions. The owner's words for (a)–(e) were "A no / B no roll, only dash and block / C no roll, only dash and block / D approve / E approve"; the dash and block values that follow from them are SYS3 proposals tagged [estimated], with four follow-on choices DB-1–DB-4, which the owner answered "DB-3 later unlock / DB-1 8 frames / DB-2 perfect timing / DB-4 drop". Frame placements, parry effects and gate geometry are SYS3–SYS6 proposals tagged [estimated]. FD-1–FD-10 are recorded as relayed by the parent agent; only FD-3 quotes the user's own words ("No Stat lock"), and its reading is an interpretation. Values set by decisions are tagged [estimated], never [sourced].

## 3. Merged numeric tables
One row per parameter. Columns:
- **Value**: the governing Bastion Zero value;
- **Tag**: the tag carried from the owning system;
- **Source**: reference values and source codes, with their own tags, as in the owning system;
- **Owner**: the system and row or section that defines the value;
- **Notes**: reasoning carried over, plus conflict status (**RESOLVED (rule)** / **FLAGGED**) where systems disagree. Full register in §3.7.

Superseded SYS1 values are kept as rows with **SUPERSEDED** and the governing value, so nothing silently disappears (SYS1 is unedited, D2-7).

### 0.3 Mapping from old codes
| Old code (in the system files) | New code here | Mapping |
|---|---|---|
| "S1-doc" … "S6-doc" | SYS1 … SYS6 | Renamed to avoid clashing with source codes S1–S93 |
| Source codes S1–S93 | S1–S93 | Identity (codes were globally unique); S89 retired → S88 |
| SYS1 §9 tuning rows 1–44 | MV-01 … MV-44 | Row *n* → MV-*nn* |
| SYS1 §9 rows 45–52 | CD-S45 … CD-S52 | Kept with a superseded/governing note (§3.2) |
| SYS1 §9 row 53 | PE-D0 | Superseded by P-C (X19) |
| SYS2 C1–C42 | CD-01 … CD-42 | C*n* → CD-*nn* |
| SYS2 user decisions 1–7 | D2-1 … D2-7 | §0.1 |
| SYS3 attack states, buffers, slot B, skills, dash, block, feel H1–H20, crits, statuses, damage model | CF-A*, CF-B*, CF-K*, CF-DA*, CF-BK* (CF-R* retired in Version 2), CF-H1–CF-H21, CF-X*, CF-S*, CF-D* | §3.3 |
| SYS3 E1–E14 | E1–E14 | Identity |
| SYS4 telegraph, aggro, token, archetype, elite, boss, respawn, budget tables | EN-T*, EN-A*, EN-K*, EN-P/R/C/F/S*, EN-E*, EN-M*, EN-B*, EN-R*, EN-Q* | §3.4 |
| SYS4 AE1–AE12, decisions (f)–(k) | AE1–AE12, (f)–(k) | Identity |
| SYS5 graph, room, region, corridor, gate-dimension, secret, shortcut, transition, camera W1–W14, spacing, pacing rows | WC-G*, WC-Z*, WC-N*, WC-K*, WC-D*, WC-SC*, WC-SH*, WC-T*, WC-W1–WC-W14, WC-P* | §3.5 |
| SYS5 GT1–GT16, SC1–SC6, SH1–SH5, MR-*, RG-*, WE1–WE11, decisions (l)–(q) | Identity | |
| SYS6 unlock, coin, economy, shop, save, death, flask, travel, deferred, completion rows | PE-U*, PE-C*, PE-E*, PE-S*, PE-W*, PE-D*, PE-F*, PE-V*, PE-T*, PE-Q* | §3.6 |
| SYS6 P0–P3, ST1–ST4, PE1–PE12, decisions (r)–(z) | Identity | |
| X1–X23 | X1–X23 | Identity; resolutions in §3.7 |

---

