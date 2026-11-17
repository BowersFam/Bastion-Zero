# Bastion Zero: System 3, Combat (clean-room spec)

Version 3.1 · 14 Nov 2026 (UTC+8). User decisions (a)–(e), DB-1–DB-6 and the CR34 tag policy applied; v1 backed up as `system-3-combat.md.v1`, v2 as `system-3-combat.md.pre-db`, v3 as `system-3-combat.md.pre-final`.

### User decisions applied (14 Nov 2026)
| # | Decision | Applied in |
|---|---|---|
| (a) | **No:** rolling does not dodge traps, and with the roll removed no defensive move does. Hazards ignore invulnerability; S2-doc §8 rule 2 stands. | §5, §10, §12 |
| (b) | **No roll, only dash and block.** The shield/parry slot B archetype is removed; the block is a core action. | §2, §3.4, §5.3, §8, §10–§12 |
| (c) | **No roll, only dash and block.** The roll and the air roll are removed; the dash works on the ground and in the air. | §3, §5.2, §11–§13 |
| (d) | The three deterministic crit triggers are **approved** at 2× | §7 |
| (e) | The four enemy-only statuses are **approved**; no player statuses | §7 |
| DB-1 | Dash gets **8 frames of invulnerability against enemy sources only**, never hazards ("DB-1 8 frames") | §5.2, §10, §12, X27 |
| DB-2 | **Parry on**: a perfect-timing window in the first 8 frames after the guard is raised ("DB-2 perfect timing") | §5.4, §8, §10, §11 |
| DB-3 | **Air dash is a later unlock**; the ground dash stays base kit; no jump right after dashing off a ledge ("DB-3 later unlock") | §5.2, §12, X24, X28 |
| DB-4 | **1-tile tunnels dropped** entirely; the dash keeps the standing hurtbox ("DB-4 drop") | §5.2, §12, X26 |
| DB-5 | Reflected (parried) shots are **not** bounce targets | §5.4 |
| DB-6 | **No guard refund** on a parry (confirmed): a parry costs 0 guard points and gives none back | §5.4 |
| CR34 | Tag policy: Bastion Zero defaults copied from a reference are tagged [estimated], with the reference cited as basis | §0 Tags, §5.3, §8, §12 |

These are design decisions; values they set are tagged [estimated], never [sourced]. The owner's words for DB-1–DB-4 are quoted above; DB-5, DB-6 and CR34 are recorded as relayed by the parent agent; frame placements, parry effects and other details are this spec's proposals, tagged [estimated].

Primary reference: Dead Cells (DC). Secondary references: Hollow Knight (HK) and The Messenger (TM).

This spec builds on:
- System 1, `system-1-player-movement.md` ("S1-doc"), which the user decided to leave as is;
- System 2 v2, `system-2-collision-damage.md` ("S2-doc"), whose governing values and known cross-system differences (S2-doc §12, X1–X8) apply here.

No System 1 edits are proposed. Where this system adds something System 1 lacks, it is logged as a new known difference in §13.

## 0. Conventions

### Tags
- **[sourced]**: stated in a public source.
- **[community-measured]**: frame-observed by a third party.
- **[estimated]**: my Bastion Zero value, or a user design decision; the reasoning is given.
- **"provenance unclear"**: a wiki number that may come from game data.
- User-decided defaults are never tagged [sourced].
- **Copied defaults (CR34 policy, decided 14 Nov 2026):** a Bastion Zero value copied from a reference is tagged [estimated] with the reference as basis, e.g. "1 [estimated] (basis: S37)". The reference value itself stays [sourced].

### Units
These carry over from S1-doc and S2-doc:
- 60 fps; 1 frame (f) ≈ 0.0167 s [estimated];
- 1 tile = 16 px [estimated];
- H = 2 tiles [estimated];
- player health in pips: 10 [estimated] max (S2-doc C18, user decision).

### Frame-table tags
In every frame-window table, each row carries one Tag cell. That tag applies to every number in the row.

### Clean-room exclusions
1. **teemo.dev DC pages with game-file values:** "Enemies and Bosses Reference" and "Biomes and Progression Reference" (base damage tables and tier constants). Not used.
2. **teemo.dev HK "Game Feel & Juice":** compiled-code constants such as hit-pause presets, enemy recoil and attack queues. Not used.
3. **DC per-weapon hit timing breakdowns.** The DC Wiki starter-sword page splits each hit into three timed parts. The same page shows an internal item name and internal tags, which strongly suggests data-file origin. **Not used as numbers.** Only the structure (wind-up, attack, cooldown per hit) informs §3. The page's overall combo rate is cited once as provenance unclear.
4. **DC stat and gear tables** (S21, S41): used only for the shape of the scaling, flagged provenance unclear.

### Source key
Codes match Systems 1–2 where reused.

| Code | Source | URL |
|---|---|---|
| S3 | Kotaku dev interview (TM): attacking never slows running; jumping is never delayed by an attack | https://kotaku.com/tight-controls-define-this-retro-homage-to-ninja-gaiden-1822282879 |
| S4 | The Messenger Wiki, Shop Upgrades page | https://the-messenger.fandom.com/wiki/Shop_Upgrades |
| S6 | Steam Community 100% guide (TM) | https://steamcommunity.com/sharedfiles/filedetails/?id=1643339519 |
| S7 | GameFAQs TM guide by KumihoStar | https://gamefaqs.gamespot.com/pc/230087-the-messenger/faqs/77725/general-information |
| S10 | aigamedev.ai, "Measured movement" (frame tracking) | https://aigamedev.ai/skills/platformer-feel/references/measured-movement.md |
| S11 | Hollow Knight Wiki: weapon page; dash page | https://hollowknight.wiki/w/Nail · https://hollowknight.wiki/w/Mothwing_Cloak |
| S12 | Gaming StackExchange, "How does the invincibility frame system work in Hollow Knight?" | https://gaming.stackexchange.com/questions/413676 |
| S14 | Official DC Wiki: Mechanics (roll, interrupt-stun meter, air stall, dive attack) | https://deadcells.wiki.gg/wiki/Mechanics |
| S15 | Official DC Wiki: Controls | https://deadcells.wiki.gg/wiki/Controls |
| S20 | Official DC Wiki: Shields | https://deadcells.wiki.gg/wiki/Shields |
| S21 | Official DC Wiki: Stats | https://deadcells.wiki.gg/wiki/Stats |
| S29 | Atomic Bob-Omb blog, "Hollow Knight & Knockback" | https://atomicbobomb.home.blog/2019/02/05/hollow-knight-knockback/ |
| S30 | Stack Overflow, HK melee frame-by-frame observations | https://stackoverflow.com/questions/58929965/cloning-melee-combat-from-hollow-knight |
| S37 | 80.lv, "Interview With the Developers of Dead Cells" (hit-stop on crits) | https://80.lv/articles/interview-with-the-developers-of-dead-cells |
| S38 | teemo.dev, DC "Game Feel & Juice". Used only for its summary of the 2019 GDC talk by DC's designer and the accessibility toggles; no game-file values. | https://teemo.dev/game-design/dead-cells/systems/game-feel-and-juice/ |
| S39 | Ars Technica "War Stories" video with DC's designer (input leniency) | https://www.youtube.com/watch?v=Uv5NwboDDhk |
| S40 | Official DC Wiki: Status effects | https://deadcells.wiki.gg/wiki/Status_effects |
| S41 | Official DC Wiki: Gear (slots, melee/ranged/shield mechanics, skill tables) | https://deadcells.wiki.gg/wiki/Weapons |
| S42 | Official DC Wiki: starter-sword page (combo rate; provenance unclear) | https://deadcells.wiki.gg/wiki/Rusty_Sword |
| S43 | Nestor's Long Stuff, "Dead Cells: Movement" (critique: cancels and roll) | https://nestorslongstuff.home.blog/2018/12/14/dead-cells-movement/ |

---

## 1. Excluded (roguelite randomization)

The user asked to exclude loot rolls and run structure. Each item below is excluded from Bastion Zero's model; some are still mentioned as reference shapes.

| # | Excluded DC element | Why excluded | What remains in this spec |
|---|---|---|---|
| E1 | Random weapon, skill and amulet drops; drop rates; elite and boss drop tables (S41) | Loot roll | Fixed-location, hand-placed weapons and skills (§2) |
| E2 | Gear level by area, location bonus and difficulty bonus (S41) | Loot roll and run structure | Fixed upgrade tiers (§9) |
| E3 | Gear quality and gear-power damage boost table (S41) | Run-structure meta-progression on random items | None |
| E4 | Affixes and affix rerolls, including roll i-frame affixes | Loot roll | None. Dash and block values are fixed (§5). |
| E5 | Stat scrolls, scroll choices, scroll fragments, and the three-colour stat system (S21) | Run structure. Picks and offers are randomized per run. | The *shape* of DC's damage scaling (§9), flagged. Bastion Zero uses fixed tiers instead. |
| E6 | Mutations picked per run, including kill-based skill cooldown reduction | Run structure | None in v1. Cooldown reduction is an Open Question. |
| E7 | Difficulty-tier scaling of enemy damage and HP, and area tiers (S23 in S2-doc) | Run structure | Fixed damage classes (S2-doc §5) |
| E8 | Health-flask charges and refills per area, food, shops | Run structure / economy | Healing is out of scope (S2-doc Dependencies) |
| E9 | Permadeath, cell economy, gold retention | Run structure | S2-doc §9 death model |
| E10 | Cursed chests and the curse status (one-hit death until enough kills) | Random event / run structure | None |
| E11 | Colorless and legendary item variants | Loot roll | None |
| E12 | Backpack (third stored weapon) | Exists to manage random loot | None |
| E13 | Two-handed weapon slot sharing | Tied to the random weapon pool | Optional; noted in §2 |
| E14 | One-hit protection and "recovery" health (S14) | Not random, but built for a continuous HP bar and long runs. They conflict with pips (S2-doc §5 rules 5–6). | Already decided in S2-doc: recovery not adopted; lethal-hit guard is assist-only |

---

## 2. Weapon slots and loadout

Sources: S3, S4, S6, S7, S11, S41.

### Comparison
| Topic | DC | HK | TM |
|---|---|---|---|
| Weapon slots | 2 [sourced] weapon slots, 2 [sourced] skill slots, 1 [sourced] amulet slot, total 5 [sourced] (S41) | 1 fixed melee weapon, upgraded in place across 5 [sourced] tiers (S11). Spells come from a meter, not slots. | 1 fixed sword. A ranged throw uses a charge pool of 3 [sourced], raised to 5 [sourced] by upgrades (S4). |
| Weapon types | Melee (hit multiple enemies in range), ranged (some ammo-limited, auto-refill), shields (block and parry) (S41) | Melee only. Spells are separate. | Melee plus ranged throw |
| Two-handed | Some weapons occupy both slots (S41) | n/a | n/a |
| Swap rule | Picking up a third skill opens a choose-one-to-discard menu (S41) | n/a | n/a |
| Movement while attacking | Free turn-around, even mid-attack, per the designer's talk (S38) | Not documented | Attacking never slows the run; jumping is never delayed (S3) |

### Bastion Zero loadout
| Slot | Count | Content | Swappable | Rule |
|---|---|---|---|---|
| A: Primary blade | 1 [estimated] | Always the sword. It is the bounce-attack source (S1-doc §3). | No | Ground chain (§3.1), air slash (§3.2), down-strike in glide (S1-doc) |
| B: Secondary weapon | 1 [estimated] | One of the archetypes in §3.4: heavy or ranged. The shield archetype is removed (decision b); the block is a core action (§5.3). | Yes, at checkpoints | Found at fixed locations (E1 excluded) |
| C/D: Active skills | 2 [estimated] | Cooldown skills (§4) | Yes, at checkpoints | Fixed locations |

**Design rationale.** DC's two-weapon freedom is kept as one free slot. The TM identity (sword drives traversal) is locked in slot A, so a level designer can always rely on the bounce-attack.

---

## 3. Attack chains (state lists with frame windows)

Sources: S3, S6, S14, S38, S41, S42, S43.

### Reference behaviour
- **DC:**
  - Melee weapons have per-weapon movesets that advance on continued input.
  - There is a short window in which the player may roll or jump and still continue the combo (S41). Rolling does not reset a weapon's combo (S14).
  - Each hit has a wind-up, attack and cooldown structure (S42; numbers excluded).
  - A starter sword's three-hit combo takes 0.69 [sourced] s in total (S42, provenance unclear).
  - The roll cancels the player's own attack recovery (S38, S43).
- **TM:** the swing hits everything in range, stays active for "a few frames" and moves with the hero (S6). Jumping during an attack happens immediately (S3).
- **HK:** an equipment upgrade raises attack speed by 32% [sourced] (S11). Swings go in four directions; down-swings bounce (S11).

### Window definitions
| Window | Meaning |
|---|---|
| Startup | Frames before the hitbox appears. Facing can still flip (DC free turn-around, S38). |
| Active | Hitbox live. Per-target lockout: one hit per target per swing (S1-doc row 25). |
| Recovery | No new attack. Movement stays free (S1-doc overlay rule; TM, S3). |
| Chain window | Frames in which an attack input advances to the next hit |
| Combo memory | Time after a chain hit during which a later attack still continues the chain, even after a dash or jump (DC rule for its roll, S14, S41) |
| Dash / block cancel | Frames in which a dash input (startup or recovery) or a block input (recovery only) cancels the attack |
| Buffer | How early an input may arrive and still fire at the first legal frame |

### 3.1 Ground chain (slot A), state list
Frame counts are relative to that hit's first frame.

| State | Startup | Active | Recovery | Next-hit cancel from | Dash cancel allowed (block: recovery only) | Damage (× weapon base) | Tag |
|---|---|---|---|---|---|---|---|
| G1 (opener) | 3 | 6 | 9 | recovery f4 (13th frame of the hit) | startup and recovery, not active | 1.0 | [estimated] |
| G2 | 3 | 6 | 9 | recovery f4 | startup and recovery | 1.0 | [estimated] |
| G3 (finisher) | 5 | 6 | 16 | none (chain ends) | startup and recovery | 2.0 | [estimated] |

- **Fastest full chain:** 13 + 13 + 27 = 53 frames ≈ 0.88 s [estimated]. DC's starter combo is 0.69 [sourced] s (S42, provenance unclear). Bastion Zero is slightly slower because every hit can also be a traversal tool.
- **Active window = 6 frames = 0.10 s** [estimated]. This matches S1-doc row 28.
- **Combo memory:** 0.35 [estimated] s after a hit's recovery ends. A dash or jump inside it does not reset the chain (DC rule for its roll, S14; applied to the dash). Landing from a jump does not reset it either.
- **Chain reset:** combo memory expires, or a hit is taken (H-KNOCK).

States: `IDLE → G1 → G2 → G3 → IDLE`.
- From G1 or G2, a buffered attack inside the chain window goes to the next G state.
- A dash in an allowed window goes to C-DASH (§5.2), and combo memory is kept. A block in recovery goes to C-BLOCK (§5.3).
- When combo memory ends, the chain returns to IDLE.

### 3.2 Air and wall attacks (slot A)
| State | Context (S1 state) | Startup | Active | Recovery | Chain | Notes | Tag |
|---|---|---|---|---|---|---|---|
| AIR1 | A-RISE-*, A-APEX, A-FALL, A-FASTFALL | 2 | 6 | 10 | No: each swing stands alone | Each connect may grant an air-jump (S1-doc §3). Hit-pause and air stall per S1 rows 26–27. Does not advance or reset the ground chain. | [estimated] |
| WALL1 | W-CLING | 2 | 6 | 10 | No | Hits away from the wall only (TM, S6, in S1-doc §5) | [estimated] |
| GLIDE-DOWN | A-GLIDE (unlock) | 3 | 8 | 8 | No | Downward hitbox. Bounces the hero up on a qualifying hit (S1-doc §3). | [estimated] |
| CROUCH1 | G-CROUCH | 3 | 6 | 9 | Advances the ground chain as G1 | Low hitbox (TM crouch attack, S6) | [estimated] |

### 3.3 Buffering (all attacks)
| Input | Buffer | Tag | Notes |
|---|---|---|---|
| Attack | 6 f (0.10 s) | [estimated] | Fires at the first frame that allows a new attack |
| Dash | 6 f | [estimated] | Highest priority (§3.5) |
| Block | 6 f on press; then held | [estimated] | §5.3 |
| Skill | 6 f | [estimated] | |
| Jump | per S1-doc row 20: 0.10 s | [estimated] | Never blocked by attacks (TM, S3) |

### 3.4 Secondary weapon archetypes (slot B)
| Archetype | Startup | Active | Recovery | Damage (× base) | Special | Tag |
|---|---|---|---|---|---|---|
| Heavy | 10 | 6 | 24 | 3.0 | +2 poise damage multiplier; no chain | [estimated] |
| Ranged | 4 | projectile | 12 | 0.6 | Charge pool of 3; each charge refills in 2.0 s | [estimated] |

*Removed (decision b, 14 Nov 2026):* the shield archetype (DC raise 0.37 [sourced] s, auto-parry 0.2 [sourced] s, failed-parry lockout 0.6 [sourced] s, absorb 75% [sourced], S20). Its guard role moved to the core block (§5.3).

### 3.5 Same-frame input priority
**Decided (user, 14 Nov 2026)** with decisions (b)–(c):
1. Dash.
2. Jump. Jump never cancels the attack overlay (S1-doc); both happen.
3. Attack.
4. Block.
5. Skill.

The order is [estimated]. A dash that arrives on the same frame as an attack consumes the frame, and the attack is dropped. A block press on the same frame as an attack is dropped; held block starts on the next legal frame.

---

## 4. Active skills and cooldowns

Sources: S4, S41.

### Comparison
| Topic | DC | HK | TM |
|---|---|---|---|
| Resource model | Per-skill cooldown timers. The listed base cooldowns span 4 [sourced] s to 20 [sourced] s across grenades, traps and powers (S41; probably shown on item cards, provenance unclear). | A shared meter filled by landing hits; spells spend it. No cooldowns. | A charge pool (3 [sourced] to 5 [sourced]) refilled by pickups and checkpoints (S4) |
| Cooldown reduction | Through per-run mutations (excluded, E6) | n/a | n/a |
| Skill types | Deployables, thrown explosives, powers (S41) | Projectile, dive, scream | Ranged throw |

### Bastion Zero rules
1. **Cooldowns, not a meter.** Each skill has its own timer, starting on use (DC).
2. **Cooldown classes:**

| Class | Cooldown | Use | Tag |
|---|---|---|---|
| Short | 4 s | Quick utility (thrown, small burst) | [estimated] (DC low end 4 [sourced] s) |
| Medium | 8 s | Area control, status application | [estimated] |
| Long | 15 s | Deployables, big burst | [estimated] (DC high end 20 [sourced] s) |

3. **Use states:** `READY → CAST (startup 6 f, active per skill, recovery 8 f) → COOLDOWN → READY`. Startup and recovery are [estimated]. Movement is free during cast (S1-doc overlay rule), except that skills flagged "rooted" lock horizontal input during startup.
4. **Usable in:** all ground and air S1 states, W-CLING and A-GLIDE. Not usable in X-GRAPPLE, H-KNOCK, H-WARP or D-DEAD.
5. **No cooldown reduction in v1** (E6 excluded).

---

## 5. Defence: dash and block

**Decided (user, 14 Nov 2026), decisions (b) and (c):** "no roll, only dash and block". The roll is removed entirely. Defence in v1 is a **dash** (C-DASH) plus a **held block** (C-BLOCK), both core actions. **Decision (a):** nothing dodges hazards; S2-doc §8 rule 2 stands.

Sources: S11, S12, S14, S15, S20, S38, S43; S1-doc §7; S2-doc §8.

### 5.1 Comparison
| Topic | HK (dash: the reference for C-DASH) | DC | TM |
|---|---|---|---|
| Defensive move | Dash: a short burst in the facing direction, on the ground or in the air (S11) | Roll: 0.4 [sourced] s, i-frames, passes through enemies, 1 [sourced]-tile hurtbox (S14); shields block and parry (S20) | No dash, roll or block |
| Cooldown | 0.6 [sourced] s between dashes; 0.4 [sourced] s with a charm (S11, provenance unclear) | Roll: 0.37 [sourced] s after it ends (S14); speedrun notes say 0.75 [community-measured] s, probably start to start (S16 in S1-doc) | n/a |
| Refresh | Landing, a weapon bounce off an enemy or object, clinging to or jumping off a wall (S11) | Time only | n/a |
| I-frames | Base dash: none. A later upgrade makes the whole dash invulnerable, with its own cooldown (S12). | Part of the roll; window unpublished (S14) | n/a |
| Hitbox change | None (S1-doc §7) | Roll hurtbox 1 [sourced] tile (S14) | n/a |
| Distance and duration | Only published in excluded compiled-code sources (S1-doc Open Question 10) | 0.4 [sourced] s (S14) | n/a |
| Block | None | Shield: raise 0.37 [sourced] s, auto-parry window 0.2 [sourced] s, failed-parry lockout 0.6 [sourced] s, a block absorbs 75% [sourced] (S20) | None |

*Considered and declined (user, 14 Nov 2026):* the DC-style roll (former C-ROLL: 0.35 [estimated] s, 13 [estimated] i-frames, 3.5 [estimated] tiles, 1-tile hurtbox), the air roll, and the shield/parry slot B archetype. Kept here only as a record; backup `system-3-combat.md.v1` holds the full former text.

### 5.2 Dash, state C-DASH (**Decided (user, 14 Nov 2026)**)
⚑ System 1 defines no dash state. Its §7 takeaways are partly followed: the dash refreshes on the bounce-attack (S1-doc §7 "Refresh"), and the air dash is gated like an unlock (S1-doc dash unlock flag). Unlike the S1 takeaway, the dash now has enemy-only i-frames (DB-1; X27). No S1 value is duplicated; see §13, X9 and X24.

| Parameter | Value | Tag | Reference |
|---|---|---|---|
| Availability | **Ground dash:** base kit from the start. **Air dash:** a later traversal unlock (DB-3), using S1-doc's dash unlock flag | [estimated] | HK acquires its dash as an ability (S11) |
| Input and direction | DASH button; always in the facing direction; no down-dash | [estimated] | HK facing-direction dash (S11) |
| Duration | 12 f (0.20 s) at constant speed | [estimated] | HK duration unpublished outside excluded sources (S1-doc Open Question 10) |
| Distance | 3.5 tiles (17.5 tiles/s, about 2.1× run speed of 8.5 [estimated] tiles/s, S1-doc row 1) | [estimated] | none published |
| Cooldown | 0.40 s after the dash ends (start to start 0.60 s) | [estimated] | HK 0.6 [sourced] s between dashes (S11, provenance unclear); S1-doc §7 asks for "cooldown after end" semantics |
| Air dash (after the unlock) | Vertical speed held at 0 for the dash; 1 per airtime. Before the unlock, DASH in the air does nothing (input dropped, no buffer) | [estimated] | HK dashes in mid-air (S11) |
| Air-dash refresh (after the unlock) | Landing, W-CLING or W-JUMP, or a bounce-attack grant | [estimated] | HK (S11); S1-doc §7 takeaway |
| I-frames | **f3–f10 of the 12 f dash (8 f ≈ 0.13 s)**, against enemy sources only: enemy attacks, enemy projectiles and enemy contact. f1–f2 and f11–f12 are vulnerable, so a dash pressed after a hitbox is already overlapping still gets hit, and the exit can be punished. The i-frames cover about 2.3 [estimated] tiles of travel (f3–f10 of 3.5 tiles). Never hazards, pits, lethal volumes or the kill plane. | [estimated] | **Decided (user, 14 Nov 2026)**, DB-1 ("8 frames"). HK base dash has none (S12); S2-doc §8 rule 7 (separate flag, default off; turned on here, X27). |
| Hurtbox and collision | Standing size, unchanged. No 1-tile hurtbox: 1-tile passages are dropped (DB-4) | [estimated] | HK: no hitbox change (S1-doc §7) |
| Enemy bodies | Actor collision is off (S2-doc §1 rule 4), so the dash passes through bodies. Contact that starts during f3–f10 is ignored until that overlap clears (S2-doc C26 re-arm rule); contact that starts on f1–f2 or f11–f12 deals D-Light as normal | [estimated] | S2-doc §6 |
| Exit momentum | Run speed if a direction is held, otherwise stop | [estimated] | none |
| Off a ledge | A ground dash that leaves a ledge finishes flat, then A-FALL. **No jump right after a ledge dash:** coyote time (S1-doc row 19) is not granted, and a jump buffered during the dash is discarded (X28). After the unlock, the ledge dash also uses up that airtime's air dash. | [estimated] | **Decided (user, 14 Nov 2026)**, DB-3 |
| Jump during a ground dash | Buffered to dash end; fires only if the hero is still grounded; no dash speed carries into the jump | [estimated] | none |
| Base reach (for S5-doc gates) | **Before the air-dash unlock:** running jump 6.4–7 tiles (S5-doc §5.1); a ledge dash adds nothing because no jump may follow it, so hard gaps stay ≥ 9 tiles. **After the unlock:** running jump + air dash ≈ 10–10.5 tiles, so hard gaps for later gates are ≥ 12 tiles. | [estimated] | derived; DB-3 |
| Hazards and pits | Never avoided (decision a; S2-doc §8 rule 2) | n/a | S2-doc |

**Rules:**
- **Entry:** from G-*, A-* (only after the air-dash unlock), W-CLING and A-GLIDE (both count as air: unlock only; a W-CLING dash goes away from the wall; an A-GLIDE dash ends the glide), C-BLOCK (drops the guard), and the startup or recovery frames of attacks. Not from the active frames.
- **Exit:** to G-IDLE or G-RUN on the ground, or A-FALL in the air.
- **Blocked:** C-DASH can't be entered from X-GRAPPLE, H-KNOCK, H-WARP or D-states.
- **Combo memory is kept** through a dash (DC rule for the former roll, S14; applied to the dash) [estimated].
- **Stored air-jump** (S1-doc §3) is not consumed or granted by a dash.

### 5.3 Block, state C-BLOCK (**Decided (user, 14 Nov 2026)**)
A generic held guard, with a perfect-timing parry at the start of each raise (§5.4; DB-2).

**State list:** `BK-RAISE (raise) → BK-HOLD (guarding; its first 8 f are the parry window) ⇄ BK-STUN (blockstun after a blocked hit) → BK-RELEASE (lower) → none`; a hit inside the window → `BK-PARRY → BK-HOLD`; from any guarding state, guard points at 0 → `BK-BREAK → none`.

| Parameter | Value | Tag | Reference |
|---|---|---|---|
| Input | Hold BLOCK; press buffer 6 f | [estimated] | none |
| Where usable | Ground states only (G-IDLE, G-RUN, G-CROUCH). Not in the air, W-CLING, A-GLIDE, X-GRAPPLE, H-KNOCK, H-WARP, D-states or C-DASH. | [estimated] | none |
| Startup (BK-RAISE) | 4 f (≈ 0.07 s); not guarding yet | [estimated] | DC shield raise 0.37 [sourced] s (S20); faster here because the block is core |
| Release (BK-RELEASE) | 4 f before an attack can start | [estimated] | none |
| Coverage | Front half (180°) of the facing direction; hits from behind land normally | [estimated] | none |
| Movement while guarding | Walk at 0.35× run speed (≈ 3.0 tiles/s); facing locked; jump or dash cancels the guard at once | [estimated] | none |
| D-Light attack, projectile or front contact | 0 pips; costs 1 guard point | [estimated] | DC block absorbs 75% [sourced] (S20) |
| D-Heavy | Chip 1 pip; costs 2 guard points | [estimated] | Same |
| Chip and the last pip | Chip never takes the last pip (stops at 1) | [estimated] | none |
| Chip and damage reduction | The −1 [estimated] upgrade (basis: TM rule, S4) applies only to hits of 2 or more, so chip stays 1 | [estimated] | S2-doc §5 rule 3 |
| Chip and i-frames | Chip is damage, so post-hit i-frames (S2-doc C24, 1.0 [estimated] s) start as normal | [estimated] | S2-doc §8 |
| Unblockable attacks | Marked with the distinct attack-sign colour; not blocked or parried (normal hit). Answered by a jump, a dash out of reach, or a dash through on the i-frames (S4-doc §3.5). | [estimated] | S4-doc §3.2 |
| D-Hazard, D-Pit, D-Lethal, kill plane | **Never blocked** (decision a; S2-doc §8 rule 2) | n/a | S2-doc |
| Guard points | 4 | [estimated] | none |
| Guard regeneration | 1 point per 0.5 s, after 0.75 s with no blocked hit | [estimated] | none |
| Guard break (BK-BREAK) | At 0 points: 0.8 s with no actions, no i-frames; guard refills to full when it ends | [estimated] | none |
| Blockstun and push (BK-STUN) | D-Light: 0.10 s, 0.25 tiles back. D-Heavy: 0.20 s, 0.75 tiles back. No H-KNOCK. | [estimated] | none |
| Bounce-attack | Blocking is ground-only, so it never grants an air-jump | n/a | S1-doc §3 |

**Rules:**
- **Entry:** from G-* and from attack recovery frames (not startup or active).
- **Pits:** blockstun push can carry the hero into a pit; the D-Pit + warp rule applies (S2-doc decision 1).
- **Contact:** front contact is blocked once per overlap; it re-arms only after the overlap clears (S2-doc C26).

### 5.4 Parry, state BK-PARRY (**Decided (user, 14 Nov 2026)**, DB-2 "perfect timing")
A parry is a block whose hit arrives inside the window. There is no separate button.

| Parameter | Value | Tag | Reference |
|---|---|---|---|
| Window | The first 8 f of BK-HOLD, right after the 4 f BK-RAISE completes (frames 5–12 after the press, ≈ 0.13 s) | [estimated] | DC shield auto-parry window 0.2 [sourced] s (S20) |
| Coverage | Same front 180° as the block; ground only | [estimated] | §5.3 |
| Guard-point cost | 0. A parry also doesn't restart the guard regeneration delay. **No refund** (DB-6): a parry never gives guard points back | [estimated] | none |
| D-Light melee hit | Negated (0 pips). Attacker takes 25 poise damage: fodder and soldier patrollers stagger (S4-doc thresholds); tougher enemies just build poise | [estimated] | none |
| D-Heavy melee hit, including charges | Negated (0 pips, no chip). Attacker takes 50 poise damage. A parried charge ends at once; the charger goes to E-STAGGER if its poise breaks, otherwise to E-RECOVER | [estimated] | none |
| Projectile | Reflected back along its incoming line at the same speed. It now belongs to the player: 10 damage units (W_base, no tier or crit) and 10 poise to the first enemy it hits; destroyed on terrain. **Not a bounce target** (DB-5): the bounce-attack passes through a reflected shot | [estimated] | DC shield parries reflect ranged attacks (S20) |
| Front contact (body touch) | Negated like a block, but with no poise reward and 0 guard cost | [estimated] | none |
| Unblockable attacks | **Not parryable.** The hit lands as normal | [estimated] | DC marks some attacks unparryable (S52) |
| D-Hazard, D-Pit, D-Lethal, kill plane | Never parried (decision a) | n/a | S2-doc §8 rule 2 |
| Multi-hit attacks | A parry covers the parried hitbox for the rest of its active frames | [estimated] | none |
| Elites and bosses | Poise damage counts toward their thresholds (S4-doc §9–§10); no automatic stagger; boss stagger immunity still applies | [estimated] | S4-doc EN-B9 |
| Crit follow-up | A staggered enemy takes crits under the existing trigger (§7, decision d); no extra parry multiplier | [estimated] | §7 |
| Whiff (no hit in the window) | Holding on simply continues as a normal block; no penalty | [estimated] | none |
| Whiff tap (BLOCK released during BK-RAISE or the window without a parry) | Parry lockout 0.6 s: the next raise inside that time blocks normally but has no window. Blocks spam-tapping. | [estimated] | DC failed-parry lockout 0.6 [sourced] s (S20) |
| Feel | H17 (§8): 4 f / 4 f hit-stop, 2 px / 0.10 s shake, enemy pushed 0.5 tiles | [estimated] | §8 |

---

## 6. Hit-stop and hit-stun

Sources: S29, S30, S37, S38, S14, S40.

### Reference behaviour
- **DC hit-stop:** a critical hit freezes the game for 1 [sourced] frame, then slows it down for "a few tenths of a second", with particles and a specific sound (S37). No other hit-stop values are published (S38).
- **DC screen shake:** exists, with an on/off accessibility toggle; amplitude is unpublished (S38).
- **HK:** when the player is hit, the action briefly slows and the music cuts. Both the player and the enemy are pushed apart on the player's own hits (S29, S30). Numbers are only in excluded sources.
- **DC enemy hit-stun:** an interrupt-stun meter. Heavy damage during an enemy's attack or defensive move interrupts it and stuns it for 1.4 [sourced] s. Meter damage must exceed 100 [sourced] and decays at 20 [sourced] points per second at the lowest tier (S14, provenance unclear). The **stun** status cancels most enemy attacks and refreshes rather than stacks (S40).
- **DC player stun:** caused by certain enemy attacks, and by long falls without a dive attack (S40, S14).
- **DC air stall:** a weapon in range briefly suspends the attacker in the air (S14).

### Bastion Zero rules
1. **Hit-stop is local**, affecting the attacker and target only. Global freeze is used only for crits, kills and lethal hits. Values are in §8.
2. **Crit feel copies DC's stated stack:** a 1 [estimated]-frame freeze (basis: S37), then a slow-down. The slow-down is set at 0.2 [estimated] s at 0.5× [estimated] time scale, interpreting "a few tenths of a second" (S37).
3. **Enemy hit-stun, two tiers:**
   - **Light enemies:** flinch on every hit for 0.15 [estimated] s; their attack is interrupted.
   - **Medium, heavy and boss enemies:** a poise meter (like DC's interrupt-stun meter).
     - Each hit adds poise damage.
     - Poise decays at a fixed rate.
     - When it exceeds the threshold, the enemy is staggered for 1.0 [estimated] s and its attack is cancelled.
     - Poise damage taken during an enemy attack's startup counts ×2 [estimated] (DC's meter favours interrupting attacks, S14).
4. **Player hit-stun = knockback** (S1-doc rows 49–50 and S2-doc §7). There is no separate player stun from attacks in v1. Fall stun does not exist: S1 has no fall damage or stun.
5. **No recoil on the player's own hits** (S2-doc §7 rule 2, governing). HK's self-recoil, measured at about 0.23 [community-measured] H (S10), is deliberately not used.

---

## 7. Critical hits and status effects

Sources: S37, S40, S41, S14.

### Reference behaviour
- **DC crits are conditional, not random.** Each weapon states its own crit trigger, such as a hit from behind, after a run of successive hits, or against a target with a given status (S41). Listed crit damage ranges from about 1.5× [sourced] to 3× [sourced] of normal damage across the gear table's paired values (S41, provenance unclear).
- **DC statuses** (S40):
  - Stun, root, freeze and slow restrict movement or attacks. Re-applying them refreshes the duration. Slow stacks to 5 [sourced] and then freezes briefly.
  - Fire, poison, bleed and shock deal damage over time and can stack.
  - Statuses expire 20% [sourced] faster on elites and bosses, which also build up resistance to crowd control when it is applied repeatedly.
- **HK:** no crits documented. **TM:** a charged-strike upgrade deals triple [sourced] damage (S4).

### Bastion Zero crit rules (deterministic; no random crits): **Decided (user, 14 Nov 2026), decision (d): approved at 2×**
| Crit trigger | Applies to | Multiplier | Tag |
|---|---|---|---|
| Hitting a staggered enemy (§6 rule 3) | All player attacks | 2.0 | [estimated] |
| Third or later consecutive airborne bounce-grant hit without landing or clinging (S1-doc §3 chain) | AIR1, GLIDE-DOWN | 2.0 | [estimated] |
| Hitting an enemy from behind during its attack startup | Ground chain | 2.0 | [estimated] |
| Crits do not stack: one multiplier per hit | | | [estimated] |

Range check: 2.0 sits inside DC's ~1.5×–3× [sourced] (S41, provenance unclear).

### Bastion Zero status rules: **Decided (user, 14 Nov 2026), decision (e): four enemy-only statuses approved; no player statuses**
| Status | Target | Effect | Stacking | Duration | Tag |
|---|---|---|---|---|---|
| Stun | Enemies | Cancels the current attack; no action | Refresh, no stack (DC, S40) | 1.0 s | [estimated] |
| Slow | Enemies | −20% move and attack speed per stack | Up to 5 stacks (DC 5 [sourced], S40); the 5th freezes for 1.5 s and resets | 3.0 s | [estimated] |
| Burn | Enemies | 1 damage-unit tick per 0.5 s | Refreshes; does not stack | 3.0 s | [estimated] |
| Bleed | Enemies | 1 damage-unit tick per 0.5 s | Stacks to 3 | 4.0 s | [estimated] |
| Elite/boss modifier | Elites, bosses | Durations −20%; crowd control (stun, freeze) gets 50% shorter on each re-application within 5 s | n/a | n/a | [estimated] (DC 20% [sourced], S40) |
| Player statuses | Player | None in v1. Pips can't carry ticks well (S2-doc §10). | n/a | n/a | [estimated] |

---

## 8. Combat feel table

One row per hit type. Columns:
- hit-stop, in frames at 60 fps, for the world/attacker and for the target;
- screen shake, as amplitude in px (at 16 px per tile) and duration in seconds;
- knockback distance in tiles, for the player and the enemy.

Public sources frame-measure almost none of this. The only sourced value is DC's 1-frame crit freeze (S37). Every Bastion Zero value is [estimated] and has a range.

| Hit type | Hit-stop (attacker / target) | Shake (amp / dur) | Player knockback | Enemy knockback | Tag | Range / reference |
|---|---|---|---|---|---|---|
| H1 Ground chain hit (G1/G2) | 2 f / 2 f | 0 px / 0 s | 0 tiles (no self-recoil, S2-doc §7) | 0.25 tiles (light enemies only) | [estimated] | Hit-stop 1–4 f; HK self-recoil ≈ 0.23 [community-measured] H ≈ 0.46 [estimated] tiles (S10), not used |
| H2 Chain finisher (G3) | 4 f / 4 f | 1 px / 0.10 s | 0 | 1.0 tiles | [estimated] | Hit-stop 3–6 f; shake 0–2 px |
| H3 Critical hit (any) | 1 f global freeze, then 0.2 s at 0.5× time | 2 px / 0.15 s | 0 | 1.0 tiles | [estimated] (basis for the 1 f: S37) | Slow-down "a few tenths of a second" (S37) → 0.1–0.4 s |
| H4 Airborne bounce-grant hit (AIR1) | 3 f / 3 f (= S1-doc row 26, 0.05 s) | 0 px / 0 s | 0 (air stall per S1-doc row 27) | 0.25 tiles | [estimated] | Keep ≤ 4 f so chains keep their rhythm |
| H5 Glide down-strike | 3 f / 3 f | 1 px / 0.08 s | Upward bounce per S1-doc §3 (not knockback) | 0.5 tiles downward | [estimated] | |
| H6 Projectile destroyed | 2 f / n/a | 0 px / 0 s | 0 | n/a | [estimated] | Counts as H4 if airborne |
| H7 Kill (adds to the hit type) | +2 f | +1 px / +0.10 s | 0 | Body flung 1.5 tiles | [estimated] | Kill bonus 0–4 f |
| H8 Heavy secondary hit | 5 f / 5 f | 2 px / 0.15 s | 0 | 1.5 tiles | [estimated] | Hit-stop 4–8 f |
| H9 Enemy stagger (poise break) | 4 f / 6 f | 2 px / 0.20 s | 0 | 0.5 tiles | [estimated] | |
| H10 Skill hit | 2 f / 2 f | 1 px / 0.10 s | 0 | 0.5 tiles | [estimated] | Per-skill override allowed |
| H11 Player takes D-Light (enemy or contact) | 4 f global | 2 px / 0.15 s | ≈ 1.31 tiles horizontal, ≈ 0.65 tiles rise (from S1-doc rows 49–50 and S2-doc C27) | 0 | [estimated] | Knockback values are inherited, not new |
| H12 Player takes D-Heavy | 6 f global | 3 px / 0.20 s | ≈ 1.31 tiles (S1 has one knockback value) | 0 | [estimated] | Open Question: a bigger knockback for heavy hits |
| H13 Player takes D-Hazard | 6 f global | 3 px / 0.20 s | ≈ 1.31 tiles along the hazard normal (S2-doc §7) | n/a | [estimated] | Re-hit lockout 0.5 [estimated] s (S2-doc C25) |
| H14 Player takes D-Pit | 0 f | 0 px | Warp, no knockback (S2-doc §9, fade 0.4 [estimated] s, C42) | n/a | [estimated] | |
| H15 Lethal hit | 9 f global (= S2-doc C30, 0.15 s) | 4 px / 0.30 s | n/a (death states) | n/a | [estimated] | |
| H16 Hit blocked (D-Light, front) | 2 f / 2 f | 1 px / 0.06 s | 0.25 tiles back (blockstun push) | 0 | [estimated] | **Decided (user, 14 Nov 2026):** replaces the former roll-dodge row |
| H17 Parry (BK-PARRY) | 4 f / 4 f | 2 px / 0.10 s | 0 | 0.5 tiles | [estimated] | **Decided (user, 14 Nov 2026), DB-2** |
| H18 Hit blocked (D-Heavy, chip) | 4 f / 4 f | 2 px / 0.12 s | 0.75 tiles back | 0 | [estimated] | **Decided (user, 14 Nov 2026)** |
| H19 Guard break | 6 f global | 3 px / 0.20 s | 0.5 tiles back | 0 | [estimated] | **Decided (user, 14 Nov 2026)** |
| H20 Dash start | 0 f | 0 px | n/a | n/a | [estimated] | No freeze, so the dash stays responsive |

**Shake falloff:** linear to 0 over the duration [estimated]. One global on/off toggle plus an intensity slider from 0% to 100% [estimated]. DC ships on/off only (S38).

---

## 9. Damage scaling model (plain math)

Sources: S11, S21, S41, S4; S2-doc §5.

### DC, non-random shape only
- **Item damage:** damage = baseDamage × 1.15^(s − 1). Here s is the player's level in the stat that the item scales with; 1.15 [sourced] (S21, provenance unclear).
  - Shape: +15% [sourced] compounding per level; roughly doubles every 5 [sourced] levels (S21).
  - **Excluded:** how s rises (random scroll offers, E5).
- **Max HP:** baseHP × f(s_a) × f(s_b) × f(s_c), with baseHP = 100 [sourced]. Each f grows with diminishing returns and is capped (S21, provenance unclear). Excluded as a player model: Bastion Zero uses pips.
- **Gear power damage boost** (S41): excluded (E2/E3).
- **Enemy damage and HP** scale by area tier (S23 in S2-doc); constants excluded (E7).

### HK, non-random
- Weapon damage by tier: 5, 9, 13, 17, 21 [sourced] (S11). That is +4 [sourced] per tier, linear.

### TM
- Fixed small integers: most enemies die to one hit (S7). A charged-strike upgrade deals ×3 [sourced] (S4).

### Bastion Zero model
1. **Player → enemy:**
   - dmg = W_base × T[tier] × C × M_hit
   - W_base: weapon base, 10 [estimated] units for slot A.
   - T: fixed upgrade tiers found at fixed locations: T = 1.0, 1.4, 1.8, 2.2, 2.6 [estimated]. Linear like HK's ladder; tier 5 ≈ 2.6× tier 1. HK's ladder is 21/5 = 4.2× [sourced arithmetic].
   - C = 2.0 on a crit, else 1.0 [estimated].
   - M_hit: the chain-hit multiplier from §3 (1.0, 1.0, 2.0; heavy 3.0; ranged 0.6) [estimated].
2. **Enemy HP bands:**

| Band | HP (damage units) | Hits to kill at T1 with G1/G2 | Tag |
|---|---|---|---|
| Fodder | 10 | 1 | [estimated] (TM one-hit enemies keep bounce chains flowing, S7) |
| Soldier | 30 | 3 | [estimated] |
| Brute | 80 | 6–8 | [estimated] |
| Elite | 150 | 10–15 | [estimated] |
| Boss | 600–1,200 | | [estimated] |

3. **Enemy → player:** damage classes only (S2-doc §5, governing).
   - D-Light 1 [estimated], D-Heavy 2 [estimated], D-Hazard 3 [estimated] (basis: S6), D-Pit 2 [estimated], D-Lethal all.
   - No tier or difficulty scaling.
   - Upgrade reduction: −1 [estimated] pip on hits of 2 or more (basis: TM rule, S4).
4. **Pips vs DC's HP model.** DC damage is a continuous number scaled by tier. It's softened by recovery health and one-hit protection, which only make sense on a large HP pool (E14). Bastion Zero maps each enemy attack to a class instead. Designers tune difficulty through attack frequency, telegraph length and enemy count, not damage numbers.

---

## 10. Dash, block and hit interactions with damage classes
**Decided (user, 14 Nov 2026):** decisions (a)–(c).

| Incoming | Dash i-frames (f3–f10, enemy sources only) | Post-hit i-frames (1.0 [estimated] s) | Block, from the front | Parry (first 8 f after the raise) |
|---|---|---|---|---|
| D-Light enemy attack | Avoided on f3–f10; lands on f1–f2, f11–f12 | Avoided | 0 pips, 1 guard point | Negated; 25 poise to the attacker |
| Enemy projectile | Avoided on f3–f10 | Avoided | 0 pips, 1 guard point | Reflected |
| D-Light contact | Ignored if the overlap starts on f3–f10 | Avoided | 0 pips, 1 guard point | Negated, no reward |
| D-Heavy | Avoided on f3–f10 | Avoided | Chip 1 pip, 2 guard points | Negated, no chip; 50 poise |
| Unblockable (marked) attack | Avoided on f3–f10 (it is an enemy source) | Avoided | Not blocked | Not parried |
| D-Hazard | **Not avoided** (S2-doc §8 rule 2 governs: hazards ignore i-frames); re-hit lockout applies | Not avoided | Not blocked | Not parried |
| D-Pit, D-Lethal, kill plane | Not avoided | Not avoided | Not blocked | Not parried |

The table uses no numbers beyond those already tagged.

**DB-1 and decision (a):** the dash i-frames are enemy-only, so the D-Hazard, D-Pit and lethal rows are unchanged. **Decision (a), resolved:** the DC roll dodges some traps (S14), but Bastion Zero has no roll and nothing dodges or blocks hazards (S2-doc §8 rule 2 stands).

---

## 11. Combat state list (player overlay)
The attack, skill, dash and block layers sit on top of S1 movement states.

| ID | State | Layer | Enter | Exit |
|---|---|---|---|---|
| K-IDLE | No attack | Attack | n/a | Attack input → G1/AIR1/WALL1/CROUCH1/GLIDE-DOWN by S1 state |
| G1–G3 | Ground chain | Attack | §3.1 | Recovery end → K-IDLE (combo memory runs) |
| AIR1 / WALL1 / GLIDE-DOWN / CROUCH1 | Context attacks | Attack | §3.2 | Recovery end |
| SEC-* | Secondary weapon action | Attack | Slot B input | Recovery end |
| SK-CAST / SK-CD | Skill cast / cooldown | Skill | §4 | Timers |
| C-DASH | Dash | Movement override | §5.2 | End of 12 [estimated] f → S1 ground or air state |
| C-BLOCK (BK-RAISE, BK-HOLD, BK-PARRY, BK-STUN, BK-RELEASE, BK-BREAK) | Block and parry | Guard (ground only) | §5.3–§5.4 | Release, jump, dash, or guard break timer |

**Interrupts:**
- H-KNOCK, H-WARP and D-states (Systems 1 and 2) clear the attack layer, cancel any skill in startup, end C-DASH and drop C-BLOCK. A skill already on cooldown keeps cooling.
- An attack never interrupts S1 movement (S1-doc overlay rule).

---

## 12. Interactions with Systems 1 and 2
| Topic | Rule | Governs | Reference |
|---|---|---|---|
| Dash, block, parry and hazards | None of them ever avoids D-Hazard, D-Pit, D-Lethal or the kill plane. The dash i-frames (f3–f10) are a separate flag that covers enemy sources only (S2-doc §8 rule 7; X27). A hazard re-hit lockout of 0.5 [estimated] s still applies after a hazard hit. **Decided (user, 14 Nov 2026), decisions (a) and DB-1.** | S2-doc §8 rules 2–3, rule 7, and C25 | §5, §10 |
| Dash vs hurtbox | Standing hurtbox and collision during the dash (HK reference). S2-doc §12 anticipated a 1-tile hurtbox for a dash or roll (DC convention); not used, and 1-tile passages are dropped (DB-4; X26). | S1-doc §7; S2-doc §12 | §5.2 |
| Dash vs one-way platforms | A dash never drops through a one-way (analogue of S2-doc §2 rule 8). An air dash over a one-way continues flat. | S2-doc §2 | |
| Block and parry vs damage floor | A blocked D-Light and any parried hit deal 0 pips, below S2-doc C23's 1-pip minimum. System 3 governs inside the block (difference X25). | S2-doc C23 | §5.3–§5.4 |
| Bounce-attack vs ground chain | AIR1 neither advances nor resets the ground chain; combo memory keeps running. Each AIR1 swing may grant one air-jump (S1-doc §3, one per target per swing). The third or later consecutive bounce-grant hit is a crit (§7). | S1-doc §3 | §3.2, §7 |
| Bounce-attack hit-pause | H4 uses 3 f = S1-doc row 26 (0.05 [estimated] s). Air stall per S1-doc row 27. | S1-doc rows 26–27 | §8 |
| Same-frame hit and damage | Damage beats the bounce grant. A killing blow removes the enemy's contact hitbox first. | S2-doc §1 rule 7 and §6 (difference X7) | |
| Attacks during glide | Only GLIDE-DOWN (unlock). Ground-chain and secondary-weapon inputs are ignored during A-GLIDE. Skills are allowed and the glide continues. | S1-doc §4 | §3.2, §4 |
| Attacks during wall-cling | WALL1 hits away from the wall only. Skills allowed. A dash from W-CLING goes away from the wall; no block on the wall. | S1-doc §5 | §3.2, §5 |
| Attacks during grapple | None; X-GRAPPLE auto-strikes on arrival (S1-doc §4). Skills, dash and block unavailable. | S1-doc §4 | |
| Player knockback and hit-stun | Player hit-stun is the S1 H-KNOCK state (0.25 [estimated] s, cancellable by a connecting attack or the recovery jump). It clears the attack layer and the ground chain. Knockback is deterministic (S2-doc §7). | S1-doc rows 49–50, S2-doc §7 | §6, §8 |
| No self-recoil | The player's own hits never push the player (H1–H10 player column = 0). | S2-doc §7 rule 2 | §8 |
| Contact damage | On by default (S2-doc user decision 6). A front block stops it (0 pips, 1 guard point); a dash through a body ignores it if the overlap starts on f3–f10 (DB-1). | S2-doc §6 | §10 |
| Max health | 10 [estimated] pips (S2-doc C18). S1-doc row 45 still says 8; System 2's value governs here too (difference X1). | S2-doc §12 | |
| Pits during combat | Knockback into a pit → D-Pit damage + warp (S2-doc user decision 1). The attack layer and combo memory are cleared on warp. | S2-doc §9 | |
| Dash and block vs S1 states | C-DASH and C-BLOCK are new; S1 has no dash or block state (differences X9, X24). Before the unlock the dash is ground-only; afterwards the air dash refreshes on landing, W-CLING or W-JUMP, or a bounce grant (S1-doc §7 takeaway; X11). No coyote jump after a ledge dash (X28). | System 3 | §5 |

---

## 13. Known cross-system differences (continued from S2-doc §12)
The user declined System 1 edits. These new mismatches are recorded, and the System 3 value governs within System 3's rules. No System 1 edit is implied.

| # | Topic | System 1 says | System 3 uses | Governs in System 3 |
|---|---|---|---|---|
| X9 | Defensive move | No roll, dash or block state. S1-doc §7 suggests a dash "for exploration gating", with defence left to combat. | C-DASH (ground base kit, air dash unlock; enemy-only i-frames f3–f10) and C-BLOCK (§5). **Decided (user, 14 Nov 2026), decisions (b)–(c):** no roll. | System 3 |
| X10 | Attack windows | Attack active window 0.10 [estimated] s (row 28); no chain defined | Full startup, active, recovery, chain and memory windows (§3). Active stays 6 f = 0.10 [estimated] s. | System 3 (consistent with row 28) |
| X11 | Air-jump refresh sources | Bounce-attack grants an air-jump | Same, plus a bounce grant also refreshes the air dash (as S1-doc §7 recommends for a dash) | System 3 (consistent with the S1 takeaway; the air roll it once referred to is removed) |
| X1 (from S2) | Max HP | 8 [sourced] | 10 [estimated] | Systems 2–3 |
| X24 | Dash availability | S1-doc §7 keeps a dash "for exploration gating", and S1-doc Dependencies lists dash as an unlock flag | The ground dash is base kit; the air dash is the unlock behind S1's dash flag (DB-3), so the mismatch is now limited to the ground dash | System 3 |
| X25 | Block vs damage floor | S2-doc C23: minimum damage per hit 1 [estimated] pip | A blocked D-Light deals 0 pips; a blocked D-Heavy chips 1 | System 3 (inside C-BLOCK only) |
| X26 | Dash hurtbox | S2-doc §12 dash/roll row: if one is added, a 1-tile hurtbox (DC convention) | Standing hurtbox (HK reference); 1-tile passages dropped (DB-4) | System 3 |
| X27 | Dash i-frame flag | S2-doc §8 rule 7: a dash's i-frames are a separate flag, default off; S1-doc §7: "dash without i-frames by default" | Flag on: f3–f10, enemy sources only (DB-1) | System 3 (user decision) |
| X28 | Coyote time after a ledge dash | S1-doc row 19 grants coyote time whenever the hero leaves a ledge | No coyote time and no buffered jump after a ground dash leaves a ledge (DB-3) | System 3 (user decision) |

---

## Open Questions
1. **DC per-hit attack timings.** The only public breakdown appears to come from game data (exclusion 3). Clean frame counts would need footage measurement.
2. **HK dash distance and duration** are only published in excluded sources (S1-doc Open Question 10). The dash values in §5.2 are estimates; measure footage if a comparison is needed.
3. **Hit-stop values other than DC's crit freeze, and every screen-shake amplitude,** are unpublished for all three games (S37, S38). Every §8 value is an estimate.
4. **Knockback per damage class.** S1 has a single knockback value, so D-Heavy uses the same distance as D-Light. Should heavy hits knock further? That would be a System 3-only override.
5. **DC crit multipliers.** The ~1.5×–3× range is inferred from paired gear-table values (provenance unclear).
6. **DC interrupt-stun meter numbers** (threshold, decay, stun length) are provenance unclear. Bastion Zero's poise numbers are estimates.
7. ~~Cooldown reduction~~: resolved; excluded (System 6 decision w).
8. **Block tuning:** guard points 4 [estimated], regeneration and the 0.8 [estimated] s guard break have no reference values; playtest against S4-doc pressure.
9. **Chip on the last pip:** chip stops at 1 pip (§5.3). Confirm, or let chip kill.
10. **Parry tuning:** the 8 [estimated] f window, 25 [estimated] and 50 [estimated] poise, and the 0.6 [estimated] s tap lockout are untested. Watch for parry chains trivialising soldiers (25 [estimated] poise = the soldier threshold).
11. **Dash i-frame abuse:** 8 i-frames every 0.60 s start-to-start is about 22% [estimated] invulnerable uptime when spammed; check against S4-doc dense rooms.

### Resolved (user, 14 Nov 2026)
- (a) Nothing dodges traps; hazards ignore invulnerability (S2-doc rule stands).
- (b), (c) No roll, only dash and block. Roll, air roll and the shield archetype removed.
- (d) Three deterministic crit triggers approved at 2×.
- (e) Four enemy-only statuses approved; no player statuses.
- DB-1 Dash i-frames: 8 f, f3–f10, enemy sources only.
- DB-2 Parry: on; window = first 8 f after the raise (§5.4).
- DB-3 Air dash: later unlock; ground dash base kit; no jump after a ledge dash.
- DB-4 1-tile passages: dropped.

- DB-5 Reflected shots: not bounce targets (§5.4).
- DB-6 Parry guard refund: none (§5.4).
- CR34 Copied defaults: tagged [estimated] with the reference as basis (§0).

No open decisions for the user remain in this system.

## Dependencies on other systems
- **System 1 (movement):**
  - S1 states for context attacks;
  - rows 19–20 (coyote, jump buffer), 25–28 (bounce lockout, hit-pause, air stall, active window), 1 (run speed vs dash speed) and 48–50 (i-frames, knockback); §7 dash takeaways;
  - known differences X9–X11, X24, X27 (S1 §7 "no i-frames" takeaway) and X28 (coyote time after a ledge dash).
- **System 2 (collision/damage):**
  - damage classes, contact damage, i-frame and hazard rules (§8, C25);
  - pit warp, death states, the dash i-frame flag (§8 rule 7, turned on, X27), the 1-pip damage floor (C23, X25), the dash/roll hurtbox note (X26), contact re-arm (C26), and the same-frame priority (§1 rule 7);
  - known differences X1–X8 and X29 (copied-default tags, CR34).
- **Enemies / AI:** which attacks are blockable or unblockable (dash/block-fit rule), guard pressure per attack, HP bands, poise thresholds and decay, attack telegraphs (startup windows determine back-hit crits), damage class per attack, status immunities, and elite/boss modifiers.
- **Progression / upgrades:**
  - fixed-location weapon tiers T1–T5;
  - slot B weapons and skills;
  - shop-style unlocks (projectile destruction, glide down-strike, charged strike);
  - the air-dash unlock placement (S6-doc §3.2; DB-3).
  - No random loot (E1–E5).
- **Level design:** fodder placement for bounce chains, arena sizes against dash distance, blockstun push and knockback, and the rule that hazards near combat must leave space for H11–H13 knockback.
- **Camera / VFX / audio:**
  - shake toggle and slider;
  - hit-stop and slow-motion handling, so the camera and audio ignore time-scale where needed;
  - crit flash with a photosensitivity toggle (DC precedent, S38);
  - hit sounds per H-row.
- **UI:** cooldown display for 2 skills, charge pips for the ranged archetype, poise or stagger indicators on large enemies.
- **Accessibility:** shake intensity, flash toggle, longer i-frames, and the S2 assist lethal-hit guard.
