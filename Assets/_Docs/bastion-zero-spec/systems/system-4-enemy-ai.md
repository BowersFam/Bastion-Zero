# Bastion Zero: System 4, Enemy AI and Encounters (clean-room spec)

Version 2.4 · 14 Nov 2026 (UTC+8). Version 2.4 records the user's confirmation of decision (g) (attack sign on by default); Version 2.3 is backed up as `system-4-enemy-ai.md.pre-g`. Version 2.3 applies the final decisions (parry-stagger cap kept, ≥ 1 unblockable attack per boss phase kept, reflected shots not bounce targets, CR34 tag policy); Version 2.2 is backed up as `system-4-enemy-ai.md.pre-final`. User decisions (f)–(k) applied; v1 backed up as `system-4-enemy-ai.md.v1`. System 3 decisions (a)–(e) propagated (backup `system-4-enemy-ai.md.pre-dash`); Version 2.2 applies S3 decisions DB-1–DB-4 (backup `system-4-enemy-ai.md.pre-db`).

References: The Messenger (TM), Hollow Knight (HK) and Dead Cells (DC), with equal weight. Bosses are described only as generic phase structures.

This spec builds on:
- System 1, `system-1-player-movement.md` ("S1-doc"); the user decided to leave it as is, so no edits are proposed;
- System 2 v2, `system-2-collision-damage.md` ("S2-doc"), whose governing values and known differences X1–X8 and X29 apply;
- System 3, `system-3-combat.md` ("S3-doc"), whose known differences X9–X11 and X24–X28 apply. Its decisions (a)–(e) are **decided (user, 14 Nov 2026)**: no roll; the player defends with a dash (enemy-only i-frames on f3–f10, DB-1; air dash a later unlock, DB-3) and a held block with a perfect-timing parry (DB-2) (S3-doc §5).

### User decisions applied (14 Nov 2026)
| # | Decision | Applied in |
|---|---|---|
| (f) | All four respawn classes approved, including R3 (off-screen respawn for traversal anchors) | §11 |
| (g) | Universal attack warning sign **on by default** (confirmed by the user) | §3.2, §14 |
| (h) | Enemies that give up the chase **keep their damage** (no healing on return) | §3.3 |
| (i) | Bosses use **weighted random attack choice with no-repeat rules**; no scripted-cycle mode | §1, §10 |
| (j) | **No bounce** from hitting a shield in the air; X12 declined | §8, §12, §15 |
| (k) | Elite clone modifier **reinstated** as a designer-placed option, never rolled | §1 (AE12), §9 (M6), §13 |
| PS-CAP | **Parry-stagger cap kept at 3.0 [estimated] s.** The user wrote "No Stat lock"; reading it as "no stun lock" (so keep the cap that prevents parry locks) is the parent agent's interpretation, recorded here. | §3.5 rule 5, §5 |
| UB-MIN | **At least 1 [estimated] unblockable attack per boss phase from phase 2**, kept (confirmed) | §10 |
| DB-5 | Reflected shots are **not** bounce targets (S3-doc §5.4) | §10 phase 2 |
| CR34 | Copied reference values used as defaults are tagged [estimated], basis cited | §0 Tags, units table, §8 |

**System 3 decisions (a)–(e), decided (user, 14 Nov 2026):** (a) nothing dodges hazards; (b)–(c) no roll, only dash and block (shield archetype and air roll removed); (d) three deterministic crit triggers approved at 2×; (e) four enemy-only statuses approved. The former branches are removed; this file now uses the dash/block-fit rule (§3.5).

**System 3 decisions DB-1–DB-4, decided (user, 14 Nov 2026):** DB-1 dash i-frames 8 f (f3–f10), enemy sources only; DB-2 parry on (first 8 f after the raise); DB-3 air dash is a later unlock, ground dash base kit; DB-4 1-tile tunnels dropped. Applied in §3.5, §4–§10, §12 and §14.

## 0. Conventions

### Tags
- **[sourced]**: stated in a public source.
- **[community-measured]**: frame-observed by a third party.
- **[estimated]**: my Bastion Zero value; the reasoning is given.
- **"provenance unclear"**: a wiki number that may come from game data.
- **Copied defaults (CR34 policy, decided 14 Nov 2026):** a Bastion Zero value copied from a reference is tagged [estimated] with the reference as basis; the reference value itself stays [sourced].

In tables with a Tag column, the tag applies to every number in that row.

### Units
These carry over from earlier systems:
- 60 fps; 1 frame (f) ≈ 0.0167 s [estimated];
- 1 tile = 16 px [estimated];
- player collision box 0.75 × 1.75 tiles [estimated]; hurtbox 0.5 [estimated] tiles wide (S2-doc C1, C2, C4).

### Decided values used throughout
| Value | Setting | Where defined |
|---|---|---|
| Max health | 10 [estimated] pips (user decision) | S2-doc C18 |
| Damage classes | D-Light 1 [estimated], D-Heavy 2 [estimated], D-Hazard 3 [estimated] (basis: S6), D-Pit 2 [estimated] + warp, D-Lethal all | S2-doc §5 |
| Contact damage | On, D-Light (user decision); per-enemy off flag | S2-doc §6 |
| Hazard re-hit lockout | 0.5 [estimated] s (user decision) | S2-doc C25 |
| Post-hit i-frames | 1.0 [estimated] s | S2-doc C24 |
| Player knockback | 0.25 [estimated] s; about 1.31 [estimated] tiles across | S1-doc rows 49–50; S3-doc §8 |
| Dash | 12 [estimated] f (0.20 [estimated] s), 3.5 [estimated] tiles, i-frames f3–f10 = 8 [estimated] f against enemy sources only, 0.40 [estimated] s cooldown after it ends; ground from the start, air after the unlock | S3-doc §5.2 |
| Block and parry | Front 180° [estimated], ground only; D-Light 0 pips and 1 [estimated] guard point; D-Heavy chip 1 [estimated] pip and 2 [estimated] guard points; 4 [estimated] guard points; break 0.8 [estimated] s; hazards never blocked. Parry: first 8 [estimated] f after the raise, 0 guard cost, 25 [estimated] poise (light hit) or 50 [estimated] (heavy hit) to the attacker, projectiles reflected; unblockable attacks not parryable | S3-doc §5.3–§5.4 |
| Input buffer | 6 [estimated] f (attack, dash, block, skill) | S3-doc §3.3 |
| Player damage | Weapon base 10 [estimated] units; chain hits 10, 10 and 20 [estimated] | S3-doc §9 |
| Enemy HP bands | Fodder 10, soldier 30, brute 80, elite 150, boss 600–1,200 [estimated] | S3-doc §9 |
| Stagger | Poise meter; stagger lasts 1.0 [estimated] s; poise damage ×2 [estimated] during an enemy's attack startup | S3-doc §6 |
| Enemy statuses | Stun, slow (5 [estimated] stacks → freeze), burn, bleed; elites and bosses −20% [estimated] duration | S3-doc §7 |

### Clean-room exclusions
- No datamined or game-file enemy values. That rules out per-enemy HP, damage and speed tables taken from game data, and the teemo.dev enemy reference pages.
- The DC wiki boss health tables (S50) give per-difficulty HP. That is run structure (§1), and the provenance is unclear, so they are not used.
- HK boss health and stagger counts on the wiki (S46, S47) are presented as combat values and flagged provenance unclear. Only their *shape* (ratios, hit-count logic) is used.

### Source key
New codes start at S44. Reused: S7 (GameFAQs TM guide), S14 (DC Mechanics), S26 (HK late-game two-unit attacks, from S2-doc).

| Code | Source | URL |
|---|---|---|
| S7 | GameFAQs TM guide by KumihoStar (first-boss HP and pattern thresholds) | https://gamefaqs.gamespot.com/pc/230087-the-messenger/faqs/77725/general-information |
| S14 | Official DC Wiki: Mechanics | https://deadcells.wiki.gg/wiki/Mechanics |
| S44 | HK Wiki: rest-point page (respawn on rest) | https://hollowknight.wiki/w/Bench_(Hollow_Knight) |
| S45 | HK Wiki: Combat (boss stagger rules) | https://hollowknight.wiki/w/Combat_(Hollow_Knight) |
| S46 | HK Wiki: final-boss page (phase thresholds, stagger and combo values; provenance unclear) | https://hollowknight.wiki/w/The_Hollow_Knight |
| S47 | HK Wiki: armoured-boss page (armour-pool stagger) | https://hollowknight.wiki/w/False_Knight |
| S48 | Steam Community, HK discussion "stop enemies respawn" (weak enemies respawn on room re-entry) | https://steamcommunity.com/app/367520/discussions/0/1693795812301576401/ |
| S49 | Official DC Wiki: Enemies (elite rules, behaviours) | https://deadcells.wiki.gg/wiki/Enemies |
| S50 | Official DC Wiki: Bosses (status resistance) | https://deadcells.wiki.gg/wiki/Bosses |
| S51 | Official DC Wiki: a boss page (phases at 75% and 50% with a push-back roar) | https://deadcells.wiki.gg/wiki/The_Giant |
| S52 | Official DC Wiki: a boss page (summon phase, invulnerability, charge into pits, timed fuses) | https://deadcells.wiki.gg/wiki/The_Hand_of_the_King |
| S53 | Official DC Wiki: a boss page (speed-up at 50%) | https://deadcells.wiki.gg/wiki/The_Scarecrow |
| S54 | Reddit r/deadcells, parry-tips thread (attack sign; variable sign-to-hit delay) | https://www.reddit.com/r/deadcells/comments/vur0g8/can_someone_give_me_tips_on_parrying/ |
| S55 | Reddit r/gamedesign, "What draws your attention" (attack sign; off-screen edge indicator) | https://www.reddit.com/r/gamedesign/comments/wyfh5l/what_draws_your_attention/ |
| S56 | teemo.dev DC "UX, UI & Accessibility". Used only for its list of options menus (attack-sign size, assist sliders); no game-file values. | https://teemo.dev/game-design/dead-cells/systems/ux-ui-and-accessibility/ |
| S57 | GameSpew, The Messenger review (off-screen respawn) | https://www.gamespew.com/2018/08/the-messenger-review/ |
| S58 | Chris Davis, "The Messenger (Switch)" blog review (respawn tied to an off-screen spawn point) | https://cdavisgames.wordpress.com/2018/12/03/the-messenger-switch/ |
| S59 | Neoseeker TM walkthrough (lock-in rooms with waves; a boss knock-down stun window) | https://www.neoseeker.com/the-messenger-2018/walkthrough/Catacombs |
| S60 | The Centurion Report, DC boss guide (phase change at about 70% and 50%) | https://thecenturionreport.com/dead-cells-the-time-keeper-boss-guide-kill-the-clock/ |

---

## 1. Excluded (roguelite randomization and run structure)

| # | Excluded | Reference | Why | What remains |
|---|---|---|---|---|
| AE1 | Procedural level layout and random enemy placement | DC | Run structure | Hand-placed spawn slots (§11) |
| AE2 | Random elite spawns, and elites summoned by approaching a shrine object | DC (S49) | Random spawn | Authored elites only (§8) |
| AE3 | Random assignment of elite special abilities | DC (S49) | Random roll | One authored modifier per elite (§8) |
| AE4 | "Starred" enemies carrying random scrolls or items | DC (S49) | Loot roll | None |
| AE5 | Enemy drops: currencies, unlock tokens, item and top-rarity drop chances | DC (S49); HK currency | Loot roll / economy | Out of scope (economy system) |
| AE6 | Enemy and boss stats scaled by area tier and difficulty level | DC (S49, S50) | Run structure | Fixed HP bands (S3-doc §9) and damage classes (S2-doc §5) |
| AE7 | Difficulty-gated enemy pools (new enemy types at higher difficulty) | DC (S49) | Run structure | Authored progression tiers (§13) |
| AE8 | Curse-zone enemies (curse excluded in S3-doc E10) | DC (S49) | Run structure | None |
| AE9 | Random reward for a no-hit boss kill | DC (S50) | Loot roll | None |
| AE10 | Alternate boss per route or difficulty | DC (S50) | Run structure | One boss per authored gate |
| AE11 | "Doesn't respawn on future runs" persistence | DC (S49) | Run framing | The persistence idea is kept as respawn class R0 (§11) |
| AE12 | ~~Elite clone ability~~ **Reinstated without randomness.** **Decided (user, 14 Nov 2026):** the clone (DC splits HP into two 55% [sourced] copies, S49) comes back as designer-placed modifier M6. Only DC's random assignment stays excluded (AE3). | DC | Randomness removed | M6 (§9) |

**Kept, not excluded:** choosing an attack *inside a fight* (boss and elite pattern selection) is AI behaviour, not loot or run randomization. §10 bounds it with no-repeat rules. **Decided (user, 14 Nov 2026):** bosses use weighted random choice with no-repeat rules (decision i).

---

## 2. Reference behaviour (all three games)

### Comparison
| Topic | TM | HK | DC |
|---|---|---|---|
| Attack warning | Animation only; no documented universal marker | Animation and sound; no universal marker | A universal sign appears above the enemy before most attacks (S54, S55). Off-screen shooters show it at the screen edge (S55). Sign size is adjustable from 10% [sourced] to 200% [sourced] (S56). The sign-to-hit delay varies by enemy and distance (S54). |
| Respawn: leaving a room | Enemies respawn as soon as they leave the screen, or the screen leaves their spawn point (S57, S58). Reviewers called it annoying. | Weak enemies respawn on re-entry; tougher ones don't (S48) | n/a: levels are rebuilt each run (excluded, AE1) |
| Respawn: rest or death | Death returns the player to a checkpoint (S7) | Resting respawns defeated enemies everywhere except the current room (S44) | n/a (run structure) |
| Lock-in rooms | Arrival summons waves and locks a gate until all are defeated (S59) | Arena rooms close until cleared (common; no single page opened) | Some boss phases summon adds (S52) |
| Elites | n/a | n/a | Tougher, slightly larger, marked by an aura and label (S49) |
| Status resistance | n/a | n/a | Elites 20% [sourced] crowd-control resistance (S49); bosses' negative effects expire 20% [sourced] faster (S50) |
| Boss phases | Pattern changes at 15, 10 and 5 [sourced] of 20 [sourced] HP, i.e. every 25% (S7) | Thresholds at 75% and 40% [sourced arithmetic: 750 and 400 of 1,000 HP] (S46, provenance unclear). Phases keep earlier attacks; the last one drops some. | Thresholds at 75% [sourced] and 50% [sourced] with a push-back roar that interrupts the current attack (S51); also about 70% and 50% (S60), and a speed-up at 50% [sourced] (S53) |
| Boss vulnerability | A specific hit knocks the boss down, stunned, for a short time (S59) | Stagger by hit count, not damage. Consecutive hits within the combo window count for more. Hitting a staggered boss usually ends the stagger (S45). Example: 13 [sourced] hits or a 9-hit [sourced] combo with under 1 [sourced] s between hits (S46, provenance unclear). Armoured variants stagger when an armour pool empties (S47). | Status effects and the interrupt-stun meter (S14); the summon phase makes the boss invulnerable until its adds die or a timeout (S52) |
| Push into pits | Not documented | Not documented | A boss charge can push the player into the arena's side pits (S52) |

### Disagreements
- **Respawn aggressiveness:** TM respawns off-screen, HK respawns on re-entry or rest, and DC never respawns within a level. **Range:** from "the moment the camera leaves" to "never until rest".
- **Boss stagger:** HK counts hits, DC uses damage and status, and TM uses scripted knock-downs.
- **Telegraph marking:** DC has a universal marker; HK and TM use animation only.
- **Missing data:** no enemy telegraph durations, detection ranges or leash distances are published for any of the three games. Every timing in §4–§10 is [estimated].

---

## 3. Shared enemy framework

### 3.1 Base state list (all archetypes)
| ID | State | Enter | Exit |
|---|---|---|---|
| E-DORMANT | Not simulated (room unloaded or slot dead) | Room unload, death | Room load or respawn rule (§11) |
| E-SPAWN | Spawn-in. Harmless, contact off. | Respawn or wave spawn | Timer |
| E-IDLE / E-PATROL | Unaware | E-SPAWN; E-RETURN finished | Detection (§3.3) |
| E-NOTICE | Reaction beat: turns, shows an alert cue | Detection | Timer → E-ENGAGE |
| E-ENGAGE | Moves to its preferred range | E-NOTICE; E-COOLDOWN | In range and attack token free → E-TELEGRAPH; leash rule → E-SEARCH |
| E-TELEGRAPH | Wind-up. Attack sign shown; direction locks late (§3.2). | E-ENGAGE | Timer → E-ATTACK; stagger, stun or freeze → interrupt |
| E-ATTACK | Hitbox active | E-TELEGRAPH | Timer → E-RECOVER |
| E-RECOVER | Punish window. No attacks; reduced movement. | E-ATTACK | Timer → E-COOLDOWN |
| E-COOLDOWN | May move; may not start the same attack | E-RECOVER | Timer → E-ENGAGE |
| E-FLINCH | Light enemies only: interrupted on every hit (S3-doc §6) | Hit | Timer |
| E-STAGGER | Poise break; contact off | Poise ≥ threshold | Timer (1.0 [estimated] s, S3-doc) |
| E-STATUS | Stunned or frozen (S3-doc §7) | Status applied | Status ends |
| E-SEARCH | Moves to the last known player position | Lost line of sight or leash | Re-detect → E-ENGAGE; timer → E-RETURN |
| E-RETURN | Walks home | E-SEARCH timeout; room-edge rule | Home → E-IDLE |
| E-DEAD | Killing blow: contact hitbox removed first (S2-doc §6 rule 3) | HP ≤ 0 | → E-DORMANT |

**Interrupt priority** [estimated]: E-DEAD > E-STATUS (stun or freeze) > E-STAGGER > E-FLINCH > everything else. A hit during E-TELEGRAPH cancels the attack only through flinch, stagger or status, never from damage alone (DC precedent: stun cancels attacks, S3-doc §7).

### 3.2 Telegraph rules
| Rule | Value | Tag | Reason |
|---|---|---|---|
| Universal attack sign at telegraph start | **Decided (user, 14 Nov 2026):** on by default (decision g, confirmed). Can be turned off in options. Size slider 10%–200%. | [estimated] (DC range 10% [sourced] to 200% [sourced], S56) | DC precedent (S54, S55) |
| Minimum telegraph, D-Light melee | 0.35 s (21 f) | [estimated] | A typical ~0.25 [estimated] s visual reaction (commonly cited; not sourced here) plus a 6 f buffer leaves about 0.1 s of margin |
| Minimum telegraph, D-Heavy | 0.50 s (30 f) | [estimated] | Two pips lost needs a fairer read |
| Minimum telegraph, unblockable attacks | 0.70 s (42 f), distinct sign colour | [estimated] | DC labels some attacks as not blockable or parryable but jumpable (S52) |
| Direction lock | Aim tracks until 0.15 s (9 f) before E-ATTACK, then locks | [estimated] | Lets a late dash or jump beat tracking; the dash i-frames start at f3, so a dash pressed within 2 f of E-ATTACK still gets hit |
| Off-screen | No telegraph may *start* more than 2 tiles outside the camera. Ranged enemies may fire from up to 4 tiles outside, with an edge indicator. | [estimated] | DC edge indicator (S55) |
| Sign-to-hit delay | Always the telegraph length; no hidden delays | [estimated] | Fixes DC's variable delay complaint (S54) |
| Slow status | Each stack lengthens telegraph and recovery by 20% | [estimated] | S3-doc slow = −20% speed per stack |

### 3.3 Aggro and leash rules (defaults; archetypes override)
| Rule | Default | Tag |
|---|---|---|
| Detection box, forward | 8 tiles horizontal × ±2 tiles vertical | [estimated] |
| Detection behind | 50% of forward range | [estimated] |
| Line of sight | Required; solid tiles block, one-way platforms don't | [estimated] |
| Noise alert | Player hits anything within 6 tiles → E-NOTICE | [estimated] |
| Damage alert | Any damage → instant E-ENGAGE (skips E-NOTICE) | [estimated] |
| Group alert | An alerted enemy alerts others within 6 tiles after 0.3 s; one cascade step only | [estimated] |
| E-NOTICE duration | 0.3 s | [estimated] |
| Lost line of sight | After 3.0 s → E-SEARCH | [estimated] |
| E-SEARCH duration | 2.0 s | [estimated] |
| Leash | 14 tiles from home, or the player leaves the room | [estimated] |
| Room boundaries | Enemies never cross room transitions (TM/HK room model) | [estimated] |
| On E-RETURN | **Decided (user, 14 Nov 2026):** HP kept, no healing (decision h). Poise reset to 0; statuses run out normally. | [estimated] |
| Re-aggro lockout after E-RETURN starts | 1.0 s | [estimated] |

### 3.4 Attack tokens (shared pressure cap)
| Rule | Value | Tag |
|---|---|---|
| Enemies allowed in E-TELEGRAPH or E-ATTACK at once (melee) | 2 (early tiers), 3 (late tiers) | [estimated] |
| Ranged volleys in flight at once (all shooters) | 2 | [estimated] |
| Token release | On entering E-RECOVER | [estimated] |
| Elites and bosses | Hold their own token; adds share the rest | [estimated] |

### 3.5 Dash/block-fit rule (why the numbers work)
**Decided (user, 14 Nov 2026)** with S3 decisions (a)–(c) and DB-1–DB-3. The kit now has an i-frame answer (the dash, f3–f10, enemy sources only) and a timing reward (the parry), so the rule checks four answers.

1. **Default: blockable and parryable from the front.** D-Light costs 1 [estimated] guard point and 0 pips; D-Heavy costs 2 [estimated] guard points and chips 1 [estimated] pip (S3-doc §5.3). A parry costs nothing and staggers or builds poise (S3-doc §5.4).
2. **Unblockable attacks** (not blockable, not parryable) must be marked with the distinct sign colour and the 0.70 [estimated] s telegraph (§3.2), and must pass at least one of:
   - **Jump-fit:** a ground attack whose hitbox is no taller than 1.5 [estimated] tiles can be jumped (full jump 4 [estimated] tiles, S1-doc row 10).
   - **Dash-out fit:** a dash started on the attack sign gets clear when (reach past the player's front edge + 0.5 [estimated]-tile hurtbox) ≤ 3.5 [estimated] tiles of dash distance. Travelling attacks (charges) never pass this check.
   - **Dash-through fit (new, DB-1):** overlap time = (hitbox width + 0.5 [estimated]-tile hurtbox) ÷ (attack speed + 17.5 [estimated] tiles/s dash speed when moving toward each other), and for a hitbox that stays in place, the smaller of that traversal time and its active time. The attack fits when the overlap is ≤ 6 [estimated] f, which leaves at least 2 [estimated] f of timing margin inside the 8 i-frames.
   - Ground-only answers count only if the attack targets a grounded player; aerial threats need a jump-arc, fast-fall or glide answer **before** the air-dash unlock (DB-3), because the air dash may not be owned yet.
3. **Hazards are never blocked, parried or dodged** (S3 decision a; the i-frames are enemy-only): hazard attacks (elite M2/M3, arena sweeps) must be answered by position or a jump.
4. **Guard pressure:** blockable hits that one encounter can land in any 2.0 [estimated] s window cost at most 4 [estimated] guard points, so a full guard is never broken by ordinary enemies alone. Elites and bosses may exceed this only with a telegraphed string (sign on every hit).
5. **Parry stagger cap (DB-2; kept by decision PS-CAP):** an enemy that a parry staggered can't be staggered by another parry for 3.0 [estimated] s, so parry chains can't lock a soldier; bosses use their own stagger immunity (§10, EN-B9).

| Attack | Damage class | Blockable / parryable? | Guard cost | Other answers (dash-through overlap) | Tag |
|---|---|---|---|---|---|
| Patroller strike | D-Light | Yes / yes | 1 | Dash out (reach 1.25–1.5 tiles + 0.5 ≤ 3.5: fits); dash through 6 / 7 f (= active time; soldier margin 1 f) | [estimated] |
| Shooter projectile | D-Light | Yes / yes (reflected) | 1 (R-V volley: 3) | Jump, crouch, destroy; dash through (0.5 + 0.5) ÷ (9 + 17.5) ≈ 2.3 f: fits | [estimated] |
| Charger run (C-1) | D-Heavy | Yes / yes (ends the charge) | 2, chip 1 pip | Jump (1.5 tiles tall); dash through (1.5 + 0.5) ÷ (12 + 17.5) ≈ 4.1 f: fits | [estimated] |
| Heavy charger run (C-2) | D-Heavy | **No / no (marked)** | n/a | Jump (1.5 tiles tall); dash through (2.0 + 0.5) ÷ (10 + 17.5) ≈ 5.5 f: fits (≤ 6 f); telegraph 0.75 s | [estimated] |
| Flyer swoop | D-Light | Yes / yes, if the player is on the ground | 1 | Ground dash through (1.0 + 0.5) ÷ (10 + 17.5) ≈ 3.3 f: fits; in the air: fast-fall, stop gliding, AIR1 trade (air dash only after the unlock) | [estimated] |
| Shield thrust (S-1 / S-2) | D-Light (assumed here; §8 sets no class for the thrust) | Yes / yes | 1 / 1 | Dash out (2.0 / 2.5 tiles + 0.5 ≤ 3.5: fits); dash through ≈ 8.6 / 9 f: **does not fit** | [estimated] |
| Shield bash (S-1 / S-2) | D-Light / D-Heavy | Yes / yes | 1 / 2 | Jump over; dash through 6 f (= 0.10 s active): fits at the limit | [estimated] |
| Elite M2 / M3 hazards | D-Hazard | **Never / never** | n/a | Position or jump; the i-frames don't apply | [estimated] |

---

## 4. Patroller (ground melee)

Reference: the basic walker in all three games. HK and TM walkers turn at ledges and walls (common observation; no page opened). Variant P-C "surface crawler" follows walls and ceilings (common in HK).

### State list
`E-SPAWN → E-PATROL (walk, turn at ledge or wall) → E-NOTICE → E-ENGAGE (chase) → E-TELEGRAPH → E-ATTACK (lunging strike) → E-RECOVER → E-COOLDOWN → E-ENGAGE`

Interrupts: fodder → E-FLINCH on every hit; soldier → poise. Leash → E-SEARCH → E-RETURN → E-PATROL.

The crawler variant has no attack states: `E-PATROL` along a surface loop forever, with contact damage only. It never aggroes.

### Timing table
| Parameter | Fodder (P-F) | Soldier (P-S) | Crawler (P-C) | Tag |
|---|---|---|---|---|
| HP (units) | 10 | 30 | 10 | [estimated] (S3-doc bands) |
| Patrol speed (tiles/s) | 2.5 | 2.0 | 1.5 along the surface | [estimated] |
| Chase speed (tiles/s) | 4.0 | 4.5 | n/a | [estimated] |
| Detection (tiles) | 6 × ±2 | 8 × ±2 | none | [estimated] |
| Attack range (tiles) | 1.25 | 1.5 | contact only | [estimated] |
| Telegraph | 0.40 s (24 f) | 0.45 s (27 f) | n/a | [estimated] |
| Active | 0.10 s (6 f), lunge 0.5 tile | 0.12 s (7 f), lunge 1.0 tile | n/a | [estimated] |
| Recovery | 0.50 s | 0.60 s | n/a | [estimated] |
| Cooldown | 0.80 s | 1.00 s | n/a | [estimated] |
| Damage | Strike D-Light; contact D-Light | Strike D-Light; contact D-Light | Contact D-Light | [estimated] (classes per S2-doc) |
| Poise threshold / decay | Flinch on every hit (0.15 s) | 25 / 10 per s | Flinch on every hit | [estimated] |
| Leash (tiles from home) | 10 | 14 | n/a | [estimated] |
| Respawn class (§11) | R2 room re-entry, or R3 if a traversal anchor | R1 rest or death | R2 | [estimated] |

### Pressure on the player's kit
| Kit element | Tested or countered | How |
|---|---|---|
| G-RUN, ground chain (S3-doc §3.1) | Tested (baseline) | One G1 kills fodder; a soldier's poise of 25 breaks on the chain finisher (10 + 10 + 20 = 40 [estimated]). Teaches spacing against the 1.25–1.5-tile range. |
| Bounce-attack, A-* states (S1-doc §3) | Enabled | Fodder is the main bounce anchor: one-hit kills keep chains flowing (TM, S3-doc §9). Placed under gaps and over pits. |
| Block (S3-doc §5.3) | Tested (baseline) | The patroller teaches the block: one D-Light strike costs 1 [estimated] guard point and no pips. A 0.45 [estimated] s telegraph gives a comfortable read. |
| Dash (S3-doc §5.2) | Tested lightly | Dashing back 3.5 [estimated] tiles clears the 1.25–1.5-tile strike. Dashing through the body on the sign puts the player behind (contact ignored when the overlap starts on f3–f10). |
| Parry (S3-doc §5.4) | Rewarded | A parried strike gives 25 [estimated] poise: fodder and soldiers stagger (soldier threshold 25), then the stagger crit applies. The 3.0 [estimated] s parry-stagger cap (§3.5 rule 5) stops parry locks. |
| Back-hit crit (S3-doc §7, decision (d) approved) | Rewarded | A hit from behind during the telegraph = 2× [estimated]. |
| W-CLING, W-JUMP (S1-doc §5) | Crawler counters | Crawlers patrolling a wall deny cling spots on a fixed timing. The player waits for the gap or kills one with WALL1 (S3-doc §3.2). |
| Contact damage (S2-doc §6) | Tested | Walking into a patroller costs D-Light; 1.0 [estimated] s of post-hit i-frames then let the player walk through. |
| Skills, secondary | Not needed | Overkill on fodder by design |

---

## 5. Ranged (shooter)

Reference: DC shooters keep distance, and some teleport away when the player gets close (S49). DC off-screen shots show an edge sign (S55). TM projectiles are bounce targets once unlocked (S1-doc §3).

### State list
`E-IDLE → E-NOTICE → E-ENGAGE (hold preferred band) ⇄ E-REPOSITION (retreat hop or blink when the player is too close) → E-TELEGRAPH (aim) → E-ATTACK (volley) → E-RECOVER → E-COOLDOWN → E-ENGAGE`

- E-REPOSITION: entered when the player is within the panic range and the reposition cooldown is ready.
  - Hop variant: jumps back 3 [estimated] tiles, never off a ledge.
  - Blink variant: teleports to a pre-placed anchor point at least 6 [estimated] tiles away, with a 0.3 [estimated] s fade-out telegraph.
- No line of sight → E-ENGAGE moves to regain it. It never fires blind.

### Timing table
| Parameter | Shooter (R-S) | Volley shooter (R-V, late) | Tag |
|---|---|---|---|
| HP | 20 | 30 | [estimated] |
| Detection | 12 × ±4 tiles, line of sight | 14 × ±5 tiles | [estimated] |
| Preferred band | 6–9 tiles | 7–10 tiles | [estimated] |
| Panic range → reposition | < 3 tiles | < 4 tiles | [estimated] |
| Reposition cooldown | 4.0 s | 3.0 s | [estimated] |
| Telegraph (aim) | 0.60 s, locks at 0.15 s before firing | 0.70 s | [estimated] |
| Shots per volley | 1 | 3, spaced 0.20 s | [estimated] |
| Projectile speed / size | 9 tiles/s / 0.5 tiles | 8 tiles/s / 0.5 tiles | [estimated] |
| Projectile range | 14 tiles, then fades | 14 tiles | [estimated] |
| Recovery | 0.60 s | 0.80 s | [estimated] |
| Cooldown | 1.50 s | 2.00 s | [estimated] |
| Damage | Projectile D-Light; contact D-Light | Same | [estimated] |
| Poise | 15 / 10 per s | 25 / 10 per s | [estimated] |
| Leash | 18 tiles | 18 tiles | [estimated] |
| Respawn class | R1 | R1 | [estimated] |

### Pressure on the player's kit
| Kit element | Tested or countered | How |
|---|---|---|
| A-GLIDE (S1-doc row 29: 2.5 [estimated] tiles/s fall) | Countered | A slow glide is easy to track. The 0.15 s late lock still lets a glider stop gliding (fall) to dodge. |
| Jump, A-FASTFALL | Tested | A 0.5-tile projectile at chest height is cleared by any jump (4 [estimated] tiles max). Fast-fall dodges shots aimed at air positions. |
| G-CROUCH | Tested | Crouch hurtbox 0.75 [estimated] tiles (S2-doc C6) ducks shots fired at standing chest height (TM crouch-dodge precedent) |
| Block | Tested | Shots are blockable from the front: 1 [estimated] guard point each. An R-V volley costs 3 of the 4 [estimated] points, so a second volley inside the regen delay breaks the guard; the 2-volley cap (§3.4) and the guard-pressure rule (§3.5) prevent that. |
| Dash | Tested | Dashing *toward* the shooter closes 3.5 [estimated] tiles; a shot met on f3–f10 passes harmlessly (≈ 2.3 [estimated] f overlap). |
| Parry | Counter | A parried shot is reflected for 10 [estimated] damage units and 10 [estimated] poise. R-S (20 [estimated] HP) needs two reflected shots; the reflect is chip damage plus a little poise, not a kill. Fodder flyers (10 [estimated] HP) die to one. |
| Bounce-attack | Enabled after the unlock | Once projectiles qualify (S1-doc §3 unlock flag), volleys become air-jump sources: R-V's 3-shot [estimated] volley is a stair of bounce targets. |
| Projectile destruction (S3-doc §8 H6) | Counter to the shooter | The player's attack destroys projectiles (TM-style upgrade) |
| Secondary: ranged | Counter | Out-ranges the band, with 3 [estimated] charges (S3-doc §3.4) |
| Skills | Counter | Short-cooldown thrown skills reach the band |

---

## 6. Charger

Reference: DC chargers rush along the floor; a boss charge can push the player into side pits (S52). Charge attacks can be dodged by rolling but not stopped by a parry (S52).

### State list
`E-IDLE → E-NOTICE → E-ENGAGE (face the player, hold distance) → E-TELEGRAPH (paw or rev; direction locks) → E-ATTACK (charge in a straight line) → either E-RECOVER (skid stop) or E-WALL-STUN (if it hits a wall) → E-COOLDOWN → E-ENGAGE`

- The charge ends after its maximum distance, at a wall, or at a ledge. A ledge stop is a skid; the charger never falls off.
- **E-WALL-STUN** (new): the charger is dizzy and counts as staggered for the crit rule (S3-doc §7).

### Timing table
| Parameter | Charger (C-1) | Heavy charger (C-2) | Tag |
|---|---|---|---|
| HP | 30 | 80 | [estimated] |
| Detection | 10 × ±1.5 tiles, line of sight, same floor | 12 × ±1.5 tiles | [estimated] |
| Preferred start distance | 5–10 tiles | 6–12 tiles | [estimated] |
| Telegraph | 0.60 s; locks at 0.15 s before the charge | 0.75 s | [estimated] |
| Charge speed / max distance | 12 tiles/s / 10 tiles | 10 tiles/s / 12 tiles | [estimated] |
| Charge hitbox width | 1.5 tiles | 2.0 tiles | [estimated] |
| Dash/block fit (§3.5) | Blockable and parryable (a parry ends the charge, 50 [estimated] poise); or jump (1.5 tiles tall); or dash through (≈ 4.1 [estimated] f) | **Unblockable (marked)**; the telegraph rises to 0.75 s. Jump it (1.5 tiles tall) or dash through (≈ 5.5 [estimated] f, ≤ 6 f). No dash-out: the charge travels 12 tiles. | [estimated] |
| Recovery (skid) | 0.80 s | 1.00 s | [estimated] |
| Wall-stun | 1.5 s | 2.0 s | [estimated] |
| Cooldown | 2.0 s | 2.5 s | [estimated] |
| Damage | Charge D-Heavy; contact D-Light | Charge D-Heavy; contact D-Heavy | [estimated] |
| Knockback on the player | Standard (S1-doc rows 49–50), in the charge direction | Same | [estimated] |
| Poise | 30 / 10 per s; ×2 during telegraph (S3-doc) | 60 / 15 per s | [estimated] |
| Leash | 16 tiles | 16 tiles | [estimated] |
| Respawn class | R1 | R1 | [estimated] |

### Pressure on the player's kit
| Kit element | Tested or countered | How |
|---|---|---|
| Block, parry, jump or dash | Tested | C-1: jump or dash through for no damage, parry to stop it, block costs a chip pip. C-2 can't be blocked or parried: jump or dash through on the sign. A dash can't outrun a charge (12 tiles/s for up to 10 tiles). |
| Jump, air-jump, W-CLING | Counters | Jumping over the charge lands the player behind a recovering charger. Clinging to a wall puts the player out of reach, and a charger that hits the wall below is stunned. |
| Bounce-attack | Enabled | An AIR1 hit on the charger's back during the charge grants an air-jump over it |
| Pits (S2-doc D-Pit) | Hazard pressure | Charge knockback near a ledge can push the player into a pit: D-Pit 2 [estimated] + warp. Level rule: no charger lane ends within 2 [estimated] tiles of a pit unless the room teaches that risk (DC precedent, S52). |
| Hazards (decision a) | Knockback into spikes = D-Hazard 3 [estimated] (basis: S6); neither the dash nor the block avoids spikes (S2 rule stands). Blockstun push (0.75 [estimated] tiles on D-Heavy) also counts as reach for the pit and hazard rule (§13). | |
| Ground chain | Rewarded | Wall-stun (1.5 s) fits a full chain (0.88 [estimated] s, S3-doc) plus the stagger crit (decision d approved) |
| Heavy secondary | Counter | Heavy hits during telegraph (poise ×2) can cancel the charge before it starts |

---

## 7. Flyer

Reference: HK small flyers respawn on room re-entry (S48). DC flyers mix melee with ranged volleys (S49). TM uses airborne targets over gaps as bounce anchors (S1-doc §3).

### State list
`E-IDLE (hover, bob) → E-NOTICE → E-ENGAGE (orbit at an offset above and to the side of the player) → E-TELEGRAPH (pull back) → E-ATTACK (straight swoop) → E-RECOVER (drift up) → E-COOLDOWN → E-ENGAGE`

- Steering moves directly toward the target offset, sliding along walls. There is no pathfinding through walls; if the path is blocked for 2.0 [estimated] s, the flyer goes to E-SEARCH.
- The shooter variant F-S replaces the swoop with a single projectile (the §5 rules apply).

### Timing table
| Parameter | Swooper (F-W) | Flying shooter (F-S) | Drifter anchor (F-A) | Tag |
|---|---|---|---|---|
| HP | 10 | 20 | 10 | [estimated] |
| Detection | 9-tile radius, line of sight | 11-tile radius | none (never aggroes) | [estimated] |
| Hover offset | 3 tiles up, 2 to the side | 5 tiles away | fixed path | [estimated] |
| Move speed | 3.5 tiles/s | 3.0 tiles/s | 1.5 tiles/s | [estimated] |
| Telegraph | 0.50 s (pull back 0.5 tile); locks at 0.15 s | 0.60 s | n/a | [estimated] |
| Attack | Swoop 10 tiles/s, up to 6 tiles | Projectile 8 tiles/s | contact only | [estimated] |
| Recovery | 0.60 s | 0.60 s | n/a | [estimated] |
| Cooldown | 1.2 s | 1.8 s | n/a | [estimated] |
| Damage | Swoop D-Light; contact D-Light | Projectile D-Light | Contact D-Light, or off (flag) | [estimated] |
| Poise | Flinch on every hit | 15 / 10 per s | Flinch | [estimated] |
| Knockback taken (S3-doc §8) | ×1.5 the normal enemy value (light bodies) | ×1.0 | ×1.5 | [estimated] |
| Leash | 15-tile radius, returns at 3 tiles/s | 15 | path-bound | [estimated] |
| Respawn class | R2 | R1 | R3 | [estimated] |

### Pressure on the player's kit
| Kit element | Tested or countered | How |
|---|---|---|
| Ground chain | Countered | Flyers sit above the ground chain's reach, forcing air play |
| AIR1, bounce chain | Tested and enabled | Each one-hit flyer is a bounce source. Drifter anchors (F-A) are pure traversal targets over pits. |
| GLIDE-DOWN (S3-doc §3.2) | Counter | Striking down from above beats the swooper's upward offset |
| A-GLIDE | Countered | A swoop's line intercepts a slow glide. Glide fall is 2.5 [estimated] tiles/s, so the late lock needs a stop-glide dodge. |
| Air dash (S3-doc §5.2; unlock, DB-3) | Tested after the unlock | Before the unlock the answers are AIR1 trading (the kill removes contact first, S2-doc), fast-fall under the swoop, a ground block or parry, or a ground dash through the swoop (≈ 3.3 [estimated] f). After the unlock, an air dash on f3–f10 passes through a swoop. Swoop telegraph stays 0.50 s. |
| Pits | Hazard pressure | Swoop knockback over a pit causes D-Pit + warp. Rule: at most 2 [estimated] flyers within 6 [estimated] tiles of a pit edge (§13). |
| Skills | Counter | Area skills clear flyer groups |

---

## 8. Shielded

Reference: a DC shield-bearer projects a shield that deflects ranged attacks, charges, and jumps away when the player is close (S49). Other DC enemies block ranged attacks with their weapons (S49).

### State list
`E-IDLE → E-NOTICE → E-GUARD-ADVANCE (shield up, faces the player) → either E-TELEGRAPH (shield lowers) → E-ATTACK (thrust) → E-RECOVER (shield down, front open) → E-GUARD-ADVANCE, or E-BASH-TELEGRAPH → E-BASH → E-GUARD-ADVANCE`

- E-BASH-TELEGRAPH is entered when the player stays within 1 tile for 0.6 [estimated] s.
- E-GUARD-BREAK is entered when shield poise breaks. Exit by timer → E-GUARD-ADVANCE.
- Turn rule: when the player crosses behind, the enemy turns after a 0.4 [estimated] s delay. This is the back window.

### Shield rules
- **Frontal shield:** an arc of 120° [estimated] in front.
  - It blocks all player melee and projectiles from the front.
  - A blocked hit deals no HP damage and adds shield poise equal to the hit's damage (heavy secondary ×2).
  - Feel: 3 [estimated] f of hit-stop and a spark. The player gets no recoil (S2-doc §7 rule 2, governing).
- **No bounce from a shield hit.** **Decided (user, 14 Nov 2026):** an airborne hit that the shield blocks is **not** a qualifying target and grants no air-jump (decision j; X12 declined, §15). A swing that damages the unshielded top or back is an ordinary enemy hit and qualifies (S1-doc §3). Rule: a hit qualifies only if it deals HP damage to this enemy.
- **Open angles:** the top and back are unshielded. A hit from above (AIR1, GLIDE-DOWN) or from behind does full damage.

### Timing table
| Parameter | Shield soldier (S-1) | Tower shield (S-2) | Tag |
|---|---|---|---|
| HP | 30 | 80 | [estimated] |
| Shield poise threshold / decay | 40 / 8 per s | 80 / 10 per s | [estimated] |
| Guard-break duration | 2.0 s | 2.5 s | [estimated] |
| Body poise (when unshielded) | 25 / 10 per s | 60 / 15 per s | [estimated] |
| Detection | 8 × ±2 tiles | 8 × ±2 tiles | [estimated] |
| Advance speed | 2.0 tiles/s | 1.5 tiles/s | [estimated] |
| Thrust telegraph (front opens) | 0.50 s | 0.60 s | [estimated] |
| Thrust active / reach | 0.15 s (9 f) / 2.0 tiles | 0.15 s / 2.5 tiles | [estimated] |
| Thrust recovery (front open) | 0.70 s | 0.90 s | [estimated] |
| Bash telegraph / active | 0.40 s / 0.10 s; pushes back, D-Light | 0.45 s / 0.10 s; D-Heavy | [estimated] |
| Turn delay | 0.40 s | 0.60 s | [estimated] |
| Cooldown | 1.2 s | 1.5 s | [estimated] |
| Leash | 12 tiles | 10 tiles | [estimated] |
| Respawn class | R1 | R1 | [estimated] |

### Pressure on the player's kit
| Kit element | Tested or countered | How |
|---|---|---|
| Ground chain | Countered from the front | All front hits are blocked; the chain only builds shield poise |
| Block | Tested | The thrust and S-1 bash are blockable (1 [estimated] guard point each); S-2's D-Heavy bash costs 2 [estimated] and chips 1 [estimated] pip. Guard pressure: thrust + bash every 1.2–1.5 s stays within 4 [estimated] points per 2.0 s (§3.5). |
| Dash | Tested | Dashing through the body (contact ignored on f3–f10) puts the player behind for the 0.4–0.6 s turn delay. The thrust (≈ 8.6–9 [estimated] f overlap) is too long to dash through; dash back out of it instead. |
| Parry | Rewarded | Parrying the thrust gives 25 [estimated] poise straight into the body (it bypasses the shield, since the thrust lowers it); S-1's body poise 25 breaks. S-2's D-Heavy bash parried gives 50 [estimated]. |
| Bounce-attack, GLIDE-DOWN | Tested | Jump over (full jump 4 [estimated] tiles clears the body) and strike the unshielded top or back; that hit qualifies for an air-jump. Air swings into the shield give no air-jump (decision j), so the player must commit to the arc before swinging. |
| Heavy secondary | Counter | One heavy hit (30 [estimated] damage × 2 [estimated] poise multiplier = 60 [estimated]) breaks S-1's shield poise of 40 [estimated]; S-2's 80 [estimated] takes two |
| Skills | Counter | Thrown skills arc over the shield; deployables behind it ignore the facing |
| Air dash (after the unlock) | Tested | An air dash over the shield from a jump is a second way behind; S-2's 0.6 s turn delay keeps it viable. Before the unlock: jump over or dash through the body. |
| Ranged secondary | Countered | Shots are blocked from the front (DC precedent, S49) |

---

## 9. Elite (modifier on any archetype)

Reference (DC, S49):
- Elites are tougher, faster and slightly larger, with an aura and a label.
- They have 20% [sourced] crowd-control resistance.
- On the first damage taken and again below 50% [sourced] HP, time slows briefly and the player is knocked away.
- Below 50% [sourced] they teleport near the player if out of attack range.
- One modifier type is a shield field kept up by two crystals that come back after 6 [sourced] s; the elite can't be damaged until they're destroyed.
- Modifiers are randomly assigned in DC (excluded, AE3).

### State list (adds to the base archetype)
`E-IDLE → E-NOTICE → E-INTRO-PULSE (first engagement: label shown) → base archetype loop ⇄ E-MOD-ACT (authored modifier on its own cooldown) → at 50% HP: E-ENRAGE-PULSE → base loop with enrage tuning → E-DEAD`

- **E-INTRO-PULSE and E-ENRAGE-PULSE:**
  - a radial push of 2.0 [estimated] tiles with no damage;
  - 0.3 [estimated] s of slow motion at 0.5× [estimated];
  - the enemy is invulnerable during the pulse;
  - the current attack is interrupted (DC precedent, S49).
  - The push is modelled as an external force field, like S1's updraft, not as H-KNOCK; see X13.
- **Enrage tuning:** telegraph and cooldown −15% [estimated], but never below the §3.2 minimums. The teleport-to-player behaviour is replaced by a **gap-close dash** with its own 0.5 [estimated] s telegraph. That makes it readable, unlike DC's teleport.

### Elite parameters
| Parameter | Value | Tag |
|---|---|---|
| HP | 150 (S3-doc elite band), whatever the base archetype | [estimated] |
| Poise threshold | Base × 2.5 | [estimated] |
| Size | Base × 1.25 (hitboxes and hurtboxes scale) | [estimated] |
| Statuses | Durations −20% (S3-doc); stun and freeze 50% shorter on each re-application within 5 s | [estimated] (DC 20% [sourced], S49) |
| Contact damage | D-Light (D-Heavy if the base archetype's is) | [estimated] |
| Leash | None inside arena rooms; 20 tiles elsewhere | [estimated] |
| Max per room | 1 | [estimated] |
| Respawn class | R0 (never respawns once killed) | [estimated] |

### Authored modifier list (one per elite, chosen by the designer, never rolled)
| Modifier | Behaviour | Timing | Tag |
|---|---|---|---|
| M1 Crystal ward | Invulnerable while 2 crystals stand, 4 tiles to each side. Crystals have 20 HP and come back 6 s after both break. | Crystal return 6 s (DC 6 [sourced] s, S49) | [estimated] |
| M2 Orbiting hazard | One beam rotating at 45°/s, radius 4 tiles; D-Hazard | Starts on engagement | [estimated] |
| M3 Floor sweep | Ground line hazard, telegraph 0.70 s (hazard: never blocked or dodged; jump it), D-Hazard | Every 5 s | [estimated] |
| M4 Escort | Summons 2 fodder (R2-type, cleared when the elite dies) | Every 12 s, maximum 2 alive | [estimated] |
| M5 Volley crystal | Hovering emitter firing 3 shots spaced 0.2 s, §5 rules | Every 4 s | [estimated] |
| M6 Clone (reinstated, decision k) | The elite splits into two copies at engagement; see the M6 table below. Placed by the designer, never rolled. | Once, at E-INTRO-PULSE | [estimated] (DC 55% [sourced] HP each, S49) |

### M6 Clone timing table
**Decided (user, 14 Nov 2026):** reinstated as a designer-placed option (decision k). DC assigns it at random (excluded, AE3); here it is authored per elite.

| Parameter | Value | Tag |
|---|---|---|
| Split trigger | At E-INTRO-PULSE (first engagement) | [estimated] |
| Split telegraph | 0.8 s; both copies invulnerable and harmless | [estimated] |
| Copy HP | 55% of elite HP each = 83 units (of 150) | [estimated] (DC 55% [sourced], S49) |
| Combined HP | 110% of one elite | [estimated] (DC 110% [sourced], S49) |
| Spawn spacing | Copies appear 6 tiles apart, one on each side of the player where space allows | [estimated] |
| Attack token | The pair shares the elite's one token: only one copy telegraphs at a time | [estimated] |
| Minimum gap between the copies' attacks | 0.6 s | [estimated] |
| Enrage | Each copy pulses at 50% of its own HP. When one copy dies, the survivor enrages at once if it hasn't. | [estimated] |
| Statuses | Applied per copy; the elite −20% duration rule applies to each | [estimated] |
| Other modifiers | M6 is the elite's only modifier (one per elite) | [estimated] |
| Respawn | Both copies R0; the slot clears when both are dead | [estimated] |

M2 and M3 follow decision (a), **decided (user, 14 Nov 2026)**: the S2 rule stands. Neither the dash nor the block avoids them; jump or position instead. *Considered and declined:* DC-style passing through by roll (DC's rolled lasers behave like walls, S51).

### Pressure on the player's kit
| Kit element | Tested or countered | How |
|---|---|---|
| Whole kit | Tested together | An elite is a mini-exam of its base archetype plus one modifier |
| Skills | Required | Modifiers punish pure melee (M1 forces target switching; M2 forces movement) |
| Dash cooldown (0.40 [estimated] s after end), guard (4 [estimated] points) and parry | Tested | M3 and enrage shorten the gaps between threats to near the dash cooldown; telegraphed elite strings may break the guard (§3.5 rule 4). The elite's melee attacks are parryable (25 or 50 [estimated] poise toward its base × 2.5 threshold, so it needs several parries); M2 and M3 are hazards and are never parried; M5 crystal shots are ordinary projectiles (§5 rules) and can be reflected. |
| Stagger meter | Tested | Base × 2.5 poise needs heavy hits or interrupts during telegraphs |
| Max pips (10 [estimated]) | Budget | An elite room should cost a competent player 2–4 [estimated] pips (§13) |
| M6 Clone: ground chain, skills | Tested | Two bodies split the player's focus. Melee hits all enemies in range (DC, S3-doc §2), so luring the copies together lets one chain hit both: the 10% [estimated] HP premium is recovered by grouping. Area skills are the main counter. |
| M6 Clone: dash, block and parry | Tested | Copies on both sides mean the frontal block and parry cover only one; a dash through one copy (f3–f10) escapes the pinch. The 0.6 [estimated] s attack gap is longer than the dash's 0.20 [estimated] s plus 0.40 [estimated] s cooldown, so one dash per attack still works. Parrying one copy staggers only that copy. |
| M6 Clone: bounce-attack | Enabled | Two bodies give two bounce targets, so aerial play over the pair is rewarded |

---

## 10. Boss: generic phase structure

### Reference shapes
- **TM:** fixed small HP of 20 [sourced]. An interstitial special pattern fires every 25% [sourced arithmetic] (at 15, 10 and 5 [sourced] HP). A targeted hit knocks the boss down for a short stun (S7, S59).
- **HK:**
  - HP-threshold phases at 75% and 40% [sourced arithmetic] (S46, provenance unclear).
  - Phases add attacks and the last one removes some.
  - Hit-count stagger, with a combo bonus for hits under 1 [sourced] s apart. Hitting a staggered boss ends the stagger.
  - The final phase can't be staggered (S45, S46).
  - The armoured variant staggers when an armour pool empties (S47). Its late attacks deal 2 [sourced] units (S26, S46).
- **DC:**
  - Thresholds at 75% [sourced] and 50% [sourced], each with a push-back roar that interrupts the current attack (S51); about 70% [sourced] and 50% [sourced] in another fight (S60).
  - Speed-up at 50% [sourced] (S53).
  - An add-summon phase at about 40% [sourced]: the boss is invulnerable until its elite adds die, or returns after a timeout (S52).
  - Timed explosive objects with a 6 [sourced] s fuse (S52).
  - Attacks are individually labelled as blockable, parryable or rollable (S51, S52). Bastion Zero keeps two labels: blockable-and-parryable, or unblockable.
  - Bosses are resistant to statuses (S50).

### Bastion Zero boss state list
| ID | State | Enter | Behaviour | Exit |
|---|---|---|---|---|
| B-GATE | Arena seal | The player crosses the arena line | Doors close; camera locks | Timer → B-INTRO |
| B-INTRO | Intro | First attempt only | Boss invulnerable; player input on | Timer (2.0 [estimated] s). Skipped on retries. |
| B-SELECT | Pick an attack | Loop | Choice weighted by distance band, with the no-repeat rules below | → B-TELEGRAPH |
| B-TELEGRAPH | Wind-up | B-SELECT | Attack sign; property icon (blockable or not) | → B-ATTACK |
| B-ATTACK | Active | | | → B-RECOVER |
| B-RECOVER | Punish window | | No attacks | → B-GAP |
| B-GAP | Spacing | | Repositions | → B-SELECT |
| B-STAGGER | Poise break | Poise ≥ threshold | Contact off; takes crits (decision d approved) | Timer 1.0 [estimated] s (S3-doc), or the player lands a hit 0.5 [estimated] s into the stagger (HK-style early end) |
| B-TRANSITION | Phase change | HP crosses a threshold | Interrupts the current attack. Invulnerable. Push pulse (as the elite pulse). Arena changes (hazards on or off, platforms move). | Timer → B-SELECT of the next phase |
| B-ADDS | Optional summon phase | Authored threshold | Boss invulnerable and passive; adds spawn | All adds dead, or a timeout → B-SELECT |
| B-DEFEAT | Death | HP ≤ 0 | Contact removed first; all projectiles and hazards cleared that frame | Persistent defeat (R0) |

### Boss phase template
| Element | Small boss | Mid boss | Final-type boss | Tag |
|---|---|---|---|---|
| HP | 600 | 900 | 1,200 | [estimated] (S3-doc band 600–1,200) |
| Phases | 2 | 3 | 3, plus a desperation phase | [estimated] |
| Thresholds | 50% | 66% / 33% | 70% / 40% / 10% desperation | [estimated] (references: TM 25% steps, HK 75%/40%, DC 75%/50%) |
| Attacks in phase 1 | 3 | 3 | 4 | [estimated] |
| Added per phase | +1 or 2 | +1 or 2 | +2; the last phase drops 1–2 old ones (HK) | [estimated] |
| B-TRANSITION | 1.5 s | 1.5 s | 2.0 s | [estimated] |
| Push pulse | 2.0 tiles, no damage | 2.0 | 3.0 | [estimated] |
| Telegraph range | 0.5–1.0 s | 0.45–1.0 s | 0.45–1.2 s; phase speed-ups never go below §3.2 minimums | [estimated] |
| Recovery (punish window) | 0.8–1.5 s | 0.7–1.2 s | 0.6–1.0 s | [estimated] |
| B-GAP | 0.6–1.0 s | 0.5–0.8 s | 0.4–0.7 s | [estimated] |
| Poise threshold / decay | 120 / 20 per s | 160 / 25 per s | 200 / 25 per s; none in desperation (HK final-phase precedent) | [estimated] |
| Stagger immunity after a stagger | 6 s | 6 s | 8 s | [estimated] |
| Optional B-ADDS | No | At 40%: boss invulnerable for up to 12 s, or until the adds die (DC shape, S52) | Optional | [estimated] |
| Damage | Attacks D-Light or D-Heavy; contact D-Heavy (S2-doc table) | Same | Same; arena hazards D-Hazard | [estimated] |

### Selection rules (bounded variance, not loot randomization)
Selection mode: **Decided (user, 14 Nov 2026):** weighted random choice with the no-repeat rules below, for every boss (decision i). There is no scripted-cycle mode.

1. **No repeats:** never the same attack 3 [estimated] times in a row, and never 2 [estimated] in a row from the unblockable set.
2. **Distance bands:** near < 4 tiles, mid 4–9 tiles, far > 9 tiles [estimated]. Each attack lists the bands it may be chosen from.
3. **Weights:** each attack has a base weight per distance band. An attack's weight halves [estimated] for each of the last 2 [estimated] picks it was part of.
4. **Punish guarantee:** every attack ends in B-RECOVER of at least 0.6 [estimated] s, except chained combos, which must end in one.

### Respawn and retry
- **Player death in an arena:**
  - S2-doc D-1 to D-5 apply;
  - the boss resets to full HP and phase 1, and B-INTRO is skipped;
  - the respawn checkpoint sits at most 1 room (about 20 [estimated] s of travel) before B-GATE.
- **Pit in an arena:** D-Pit 2 [estimated] + warp to an arena safe point. The boss is not reset, and its phase continues.
  - The DC push-into-pit shape (S52) is allowed only with D-Pit, never D-Lethal.

### Pressure on the player's kit (per phase)
| Phase | Primary test | Kit elements |
|---|---|---|
| Phase 1 | Reading telegraphs; block vs parry vs jump vs dash choice | Block and guard (4 [estimated] points), parry window (8 [estimated] f), dash i-frames (8 [estimated] f), ground chain in 0.8–1.5 s windows |
| Phase 2 | Air control; vertical arena | Bounce-attack on boss projectiles. Some boss projectiles always qualify (TM precedent, S1-doc §3 [sourced] behaviour); once parried and reflected they no longer do (DB-5). Glide, wall-cling. |
| Phase 3 / desperation | Endurance and resource use | Skill cooldowns (4–15 [estimated] s, S3-doc), dash cooldown, guard, the pip budget |
| Decision (a) | Arena hazards | Hazards are positional puzzles; nothing dodges or blocks them (S2 rule). |
| Decision (b) and DB-2 | Guard pressure and parry | Bosses may break the guard only with a telegraphed string (§3.5 rule 4). Every blockable boss attack is parryable. Parry poise (25 / 50 [estimated]) counts toward the boss threshold (EN-B9: 120–200), so parries speed up a stagger but never cause one alone; stagger immunity (6–8 s) applies. From phase 2, at least 1 [estimated] attack per phase is unblockable (decision UB-MIN), so parry isn't the only answer. Boss projectiles not marked unblockable can be reflected. |
| Decision (c) and DB-3 | Aerial threats | Every aerial threat must be avoidable by movement (jump arc, fast-fall, glide stop) without the air dash, because the boss may be met before the air-dash unlock; the air dash and its i-frames are an extra answer once owned. |

---

## 11. Spawn and respawn rules

### Respawn classes
| Class | Who | Respawns when | Reference | Tag |
|---|---|---|---|---|
| R0 Persistent | Bosses, elites, scripted ambushes once cleared | Never after defeat (saved) | HK bosses; DC persistent elites (S49, run framing excluded) | n/a |
| R1 Rest/death | Standard enemies (soldiers, shooters, chargers, shielded) | The player rests at a rest point (all rooms except the current one), or dies (S2-doc D-4) | HK rest rule (S44) | n/a |
| R2 Re-entry | Fodder, crawlers, swoopers | Every time the room loads | HK weak enemies (S48) | n/a |
| R3 Traversal anchor | Fodder and drifters authored as bounce anchors. **Decided (user, 14 Nov 2026):** approved with R0–R2 (decision f). | 4 s after death once their spawn point is at least 2 tiles off-camera; never on-camera | TM off-screen respawn (S57, S58), limited to anchors | [estimated] |

### Slot state list (one per placed enemy)
`SLOT-ALIVE → (killed) → SLOT-DEAD → { R0: SLOT-CLEARED (saved, final) | R1: SLOT-DEAD until rest or death → SLOT-ALIVE | R2: SLOT-DEAD until room reload → SLOT-ALIVE | R3: SLOT-DEAD → timer and off-camera check → SLOT-ALIVE }`

### Event table
| Event | R0 | R1 | R2 | R3 | Damaged survivors |
|---|---|---|---|---|---|
| Leave room, re-enter | Stays cleared | Stays dead | Respawns | Respawns | Full HP, poise 0, at home |
| Rest at a rest point | Stays cleared | Respawns (except current room) | Respawns | Respawns | Full HP |
| Pass a plain checkpoint (no rest) | No change | No change | No change | No change | No change |
| Player death (S2-doc D-4) | Stays cleared; an *uncleared* boss or arena resets | Respawns everywhere | Respawns | Respawns | Full HP |
| H-WARP (pit or hazard warp) | No change | No change | No change | No change | No change |
| Lock-in arena: player dies mid-waves | Resets to wave 1; doors open | n/a | n/a | n/a | n/a |

### Spawn-in rules
| Rule | Value | Tag |
|---|---|---|
| Room-load spawns | Placed at once, in E-IDLE | [estimated] |
| Spawn on camera (waves, adds) | E-SPAWN telegraph 0.6 s; harmless and contact off | [estimated] |
| Minimum distance from the player | 3 tiles | [estimated] |
| Attack lockout after spawning | 0.5 s | [estimated] |
| Lock-in waves | 2–3 waves; the next wave starts 1.0 s after the last enemy dies (TM lock rooms, S59) | [estimated] |
| Rest-point safety | No R1–R3 spawn within 8 tiles of a rest point | [estimated] |

**Note on S2-doc D-4.** S2 says "room state resets (enemies, collapsing platforms)" on death. This spec refines that: R0 cleared slots never reset. This is a refinement within System 4, not a System 1 issue.

---

## 12. Cross-archetype summary

### Timing overview
| Archetype | Telegraph | Active | Recovery | Cooldown | Detection | Leash | Tag |
|---|---|---|---|---|---|---|---|
| Patroller (fodder / soldier) | 0.40 / 0.45 s | 6 / 7 f | 0.50 / 0.60 s | 0.8 / 1.0 s | 6 / 8 tiles | 10 / 14 tiles | [estimated] |
| Ranged (single / volley) | 0.60 / 0.70 s | projectile | 0.60 / 0.80 s | 1.5 / 2.0 s | 12 / 14 tiles | 18 tiles | [estimated] |
| Charger (light / heavy) | 0.60 / 0.75 s | up to 10 / 12 tiles of charge | 0.80 / 1.00 s | 2.0 / 2.5 s | 10 / 12 tiles | 16 tiles | [estimated] |
| Flyer (swoop / shooter) | 0.50 / 0.60 s | swoop up to 6 tiles | 0.60 s | 1.2 / 1.8 s | 9 / 11-tile radius | 15 tiles | [estimated] |
| Shielded (soldier / tower) | 0.50 / 0.60 s | 9 f | 0.70 / 0.90 s | 1.2 / 1.5 s | 8 tiles | 12 / 10 tiles | [estimated] |
| Elite | base −15% when enraged | base | base | base −15% when enraged | base | none in arenas, else 20 | [estimated] |
| Boss | 0.45–1.2 s | per attack | 0.6–1.5 s | B-GAP 0.4–1.0 s | arena | arena | [estimated] |

### Kit coverage matrix
T = tests, C = counters or threatens it, E = enables it.

| Kit element | Patroller | Ranged | Charger | Flyer | Shielded | Elite | Boss |
|---|---|---|---|---|---|---|---|
| G-RUN / ground chain | T | – | T | C | C (front) | T | T |
| Jump / air-jump | – | T | T | T | T | T | T |
| Bounce-attack | E | E (after unlock) | E | E | T (top or back hits only; shield hits give no bounce, decision j) | T | E (projectiles) |
| A-GLIDE | – | C | – | C | – | – | T |
| W-CLING | C (crawler) | – | E (safe spot) | – | – | – | T |
| Dash | T (through the body) | T (through shots) | T (dash through; can't outrun) | T (air dash after unlock) | T | T | T |
| Parry | E (stagger) | E (reflect) | E (C-1 only) | T (ground) | E (thrust) | T | T |
| Block | T (taught) | T | T (C-2 unblockable) | T (ground only) | T | T | T |
| Skills | – | C | – | C | C | T | T |
| Secondary weapon | – | C (ranged) | C (heavy) | C | C (heavy) | T | T |

---

## 13. Encounter composition

### Threat points (all [estimated])
| Unit | Points | Notes |
|---|---|---|
| Fodder patroller, crawler, drifter anchor | 1 | Anchors that never attack count 0.5 |
| Soldier patroller, swooper | 2 | |
| Shooter, flying shooter, light charger | 3 | |
| Volley shooter, shield soldier | 4 | |
| Heavy charger, tower shield | 5 | |
| Elite | base × 3 + 2 | Max 1 per room |
| Elite with M6 Clone | base × 3 + 5 | The pair counts as the room's one elite |
| Lethal-placement surcharge | +1 per hazard strip or pit edge inside an enemy's attack or knockback reach | |

### Synergy surcharges (added on top)
| Pair | Surcharge | Why | Tag |
|---|---|---|---|
| Shielded + ranged | +2 | "Anvil and hammer": the player can't go around the shield under fire | [estimated] |
| Charger + pit or spikes in its lane | +2 | Knockback into D-Pit or D-Hazard | [estimated] |
| 2+ flyers + ranged | +2 | No safe air space | [estimated] |
| 2 chargers in the same lane | Not allowed | Unreadable crossing charges | [estimated] |
| 3+ ranged on one screen | Not allowed | Exceeds the volley-token cap | [estimated] |

### Budget by progression tier
| Tier | Room budget (points) | Arena per wave × waves | Expected pip loss, median player | Pips lost between rest points (target) | Tag |
|---|---|---|---|---|---|
| T1 (teaching) | 3–6 | 4 × 2 | ≤ 1 | ≤ 4 of 10 | [estimated] |
| T2 | 6–9 | 6 × 2 | ≤ 2 | ≤ 5 | [estimated] |
| T3 | 9–13 | 8 × 3 | ≤ 2 | ≤ 6 | [estimated] |
| T4 | 12–16 | 10 × 3 | ≤ 3 | ≤ 7 | [estimated] |

Expected pip loss for a room, in plain math:

E[pips] = Σ over threats of (attacks per encounter × chance the median player is hit × class pips)

The "chance hit" values come from playtests, so the targets above are design goals, not predictions.

### Mixing rules
1. **Introduce, pair, mix.** Each archetype first appears alone in a teaching room. Then it appears paired with one known archetype, and only after that in mixes.
2. **Release valve.** Every room with 6 [estimated] or more points includes a bounce anchor or a cling wall the player can retreat to.
3. **Token caps** (§3.4) apply on top of the budget.
4. **One new idea per room.** That is a new archetype *or* a new elite modifier *or* new terrain, never two.
5. **Pit and hazard reach.** At most 2 [estimated] attack-capable enemies whose reach (attack plus knockback, about 1.31 [estimated] tiles) touches a pit edge or hazard.
6. **Rest cadence.** A rest point after at most 4 [estimated] combat rooms or 30 [estimated] points cumulatively, whichever comes first.
7. **Elite rooms.** No other attackers within 10 [estimated] tiles at the start, except the modifier's escorts.
8. **Boss approach.** The room before B-GATE has 0 [estimated] threat points and a checkpoint.
9. **Guard pressure:** blockable hits per encounter stay within 4 [estimated] guard points per 2.0 [estimated] s (§3.5 rule 4). The former density allowances for decisions (a) and (c) are removed (decided: no roll).

---

## 14. Accessibility hooks
| Hook | Range | Tag | Reference |
|---|---|---|---|
| Attack sign on/off | **Decided (user, 14 Nov 2026):** on by default (decision g, confirmed); can be turned off | n/a | DC sign (S54, S55) |
| Attack-sign size | 10%–200% | [estimated] | DC 10% [sourced] to 200% [sourced] (S56) |
| Telegraph length multiplier (assist) | 1.0×–1.5× | [estimated] | DC assist has slower traps and an easier parry (S56) |
| Parry window (assist) | 8–12 f (up to +50%) | [estimated] | DC assist easier parry (S56) |
| Enemy damage (assist) | −1 pip per class, minimum 1 | [estimated] | S2-doc §5 rule 4; DC assist reduces enemy damage down to 20% [sourced] (S56) |
| Enemy HP (assist) | 50%–100% | [estimated] | DC assist goes down to 20% [sourced] in 2.5% [sourced] steps (S56) |

---

## 15. Interactions with Systems 1–3 and known differences

### Interactions
| Topic | Rule | Governs |
|---|---|---|
| Contact damage | On, D-Light; heavy archetypes D-Heavy by flag. Off in E-SPAWN, E-STAGGER and E-DEAD. | S2-doc §6 |
| Killing blow vs contact | Contact is removed first (S2-doc §6 rule 3), which makes bounce kills on fodder and flyers safe | S2-doc |
| Hazard lockout | Elite and boss hazards (M2, M3, arena) use the 0.5 [estimated] s same-instance lockout | S2-doc C25 |
| Pits | Charger and boss pushes into pits → D-Pit + warp; enemies are not reset | S2-doc §9 |
| Post-hit i-frames | 1.0 [estimated] s; contact resumes only after the overlap clears (S2-doc §6 rule 4) | S2-doc |
| Dash, block and parry | Every attack is blockable and parryable from the front unless marked unblockable; unblockable attacks pass the jump-fit, dash-out fit or dash-through fit (§3.5). Hazards are never blocked, parried or dodged. Parry-stagger cap 3.0 [estimated] s. | S3-doc §5 |
| Stagger and statuses | Thresholds per archetype here; behaviour per S3 | S3-doc §6–§7 |
| Back-hit crit window | = E-TELEGRAPH (decision d approved) | S3-doc §7 |
| Bounce targets | Fodder, flyers and anchors qualify; projectiles after the unlock (S1-doc §3). Shield-blocked hits never qualify (decision j). | S1-doc §3 |

### Known cross-system differences (continued; no System 1 edits proposed)
| # | Topic | Earlier system says | System 4 uses | Governs in System 4 |
|---|---|---|---|---|
| X12 | Shield hits as bounce targets | S1-doc §3 qualifying-target list doesn't mention blocked hits | **Declined (user, 14 Nov 2026, decision j).** Blocked hits never qualify, so no mismatch remains; System 4 only spells out a case S1 leaves open. No S1 edit. | n/a (declined) |
| X13 | Non-damaging push (elite and boss pulses) | S1 enters H-KNOCK only on damage; external forces exist only as vertical updrafts (S1 row 33) | Radial push as an external force field; no H-KNOCK, no i-frames | System 4 |
| X14 | Death reset scope | S2-doc D-4: room state resets on death | R0 cleared slots never reset; uncleared arenas reset | System 4 (refines S2) |

---

## Open Questions
1. **No published telegraph, detection or leash data** for any of the three games. Every §3–§12 timing is [estimated] and needs playtest tuning.
2. **Guard pressure and parry tuning:** the 4 [estimated] guard points, the 2.0 [estimated] s pressure window, the 6 [estimated] f dash-through limit and the 3.0 [estimated] s parry-stagger cap are untested. The soldier strike (7 f active) fits the dash through with only 1 f of margin.
3. **HK boss values** (thresholds, stagger counts) are flagged provenance unclear. Bastion Zero uses only the ratio shapes.
4. **Respawn trade-off:** R3 is approved (decision f). Playtests should still confirm that off-screen respawns of traversal anchors don't feel like TM's criticised version (S57, S58).
5. **Clone modifier tuning (M6):** the 0.6 [estimated] s gap between the copies' attacks and the 10% [estimated] HP premium need playtesting.

### Resolved (user, 14 Nov 2026)
- (f) Respawn classes R0–R3, including R3: approved.
- (g) Universal attack sign: on by default (confirmed by the user; former Open Question 6 removed).
- (h) Leash: enemies keep their damage, no healing.
- (i) Boss attack choice: weighted random with no-repeat rules.
- (j) Shield hits granting a bounce: no; X12 declined.
- (k) Elite clone: reinstated as designer-placed modifier M6.
- System 3 (a)–(e): no roll; dash and block; crits and statuses approved. Branches removed.
- S3 DB-1–DB-4: dash i-frames f3–f10 (enemy only); parry on; air dash a later unlock; 1-tile tunnels dropped.
- PS-CAP: parry-stagger cap kept at 3.0 [estimated] s ("No Stat lock", read as "no stun lock"; interpretation noted in the decisions table).
- UB-MIN: at least 1 [estimated] unblockable attack per boss phase from phase 2.
- S3 DB-5: reflected shots are not bounce targets. CR34: copied defaults tagged [estimated].

No open decisions for the user remain in this system. Open Question 2 still covers playtest tuning of the cap's length.

## Dependencies on other systems
- **System 1 (movement):**
  - run speed 8.5 [estimated] tiles/s (row 1), against chase speeds below it;
  - jump height (row 10), glide fall (row 29), wall-cling (rows 39–44), knockback (rows 49–50), updraft force model (row 33);
  - bounce-target rules (§3, rows 22–27).
  - Difference X13 (X12 declined).
- **System 2 (collision/damage):**
  - damage classes, contact damage and re-arm (C26), hurtbox sizes (C1–C6), post-hit i-frames (C24), hazard lockout (C25);
  - pit warp (C41–C42), death states D-1 to D-5 and the room reset (X14).
- **System 3 (combat):**
  - dash frames and cooldown, block guard points and chip, buffer, chain damage, HP bands, poise and stagger, statuses, crit triggers;
  - the dash i-frames (f3–f10), the parry window, poise rewards and reflect damage (S3-doc §5.4), and the air-dash unlock timing (S6-doc §3.2).
- **Level design:** spawn slots with respawn classes, home and leash volumes, charger lanes clear of pits, anchor placement, arena safe points, rest-point spacing.
- **Save system:** R0 cleared flags, boss defeat, lock-in arena state.
- **Camera:** off-camera checks for telegraph start and R3 respawn, edge indicators, boss camera lock.
- **UI / audio:** attack sign with size slider, an "unblockable" variant, elite label, boss phase cue, audio cue per telegraph.
- **Economy (future):** drops are excluded here (AE5).
- **Accessibility:** §14 hooks, the S2 assist lethal-hit guard, S3 shake and flash toggles.
