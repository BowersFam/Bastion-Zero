# Bastion Zero: System 6, Progression and Persistence (clean-room spec)

Version 2.3 · 14 Nov 2026 (UTC+8). Version 2.3 records the final decisions (air dash from an ability room behind an elite; CR34 tag policy); Version 2.2 is backed up as `system-6-progression-persistence.md.pre-final`. User decisions (r)–(z) applied; v1 backed up as `system-6-progression-persistence.md.v1`. System 3 decisions (a)–(e) propagated (backup `.pre-dash`); Version 2.2 applies S3 decisions DB-1–DB-4 (backup `system-6-progression-persistence.md.pre-db`).

References: The Messenger (TM) for its shop, currency and death-penalty follower; Hollow Knight (HK) for dropped currency and recovery, rest points, loadout notches, the map vendor, fast travel and completion %; Dead Cells (DC) for permanent unlocks, permanent path unlocks and its forge. No single primary reference: this system reconciles the three into one model for a **persistent, non-roguelite world**.

This spec builds on, and does not edit:
- System 1, `system-1-player-movement.md` ("S1-doc"): left as is by the user; its unlock-flag list and row 53 (death penalty duration) are used here.
- System 2 v2, `system-2-collision-damage.md` ("S2-doc"): 10 [estimated] max pips, start 5 [estimated] (basis: S4), full health on respawn, pit damage plus warp, the hazard re-hit lockout, contact damage on, death states D-1 to D-5 and the death-site currency marker (D-3).
- System 3, `system-3-combat.md` ("S3-doc"): exclusions E1–E14, fixed upgrade tiers T = 1.0–2.6 [estimated], slot A/B/C/D loadout, cooldown classes, and the ground dash and block (with parry) as base-kit defence. Its decisions (a)–(e) and DB-1–DB-4 are **decided (user, 14 Nov 2026)**: no roll, only dash and block; the air dash is a later unlock (DB-3).
- System 4 v2, `system-4-enemy-ai.md` ("S4-doc"): respawn classes R0–R3, weighted no-repeat boss attacks, the elite clone as designer-placed modifier M6, exclusions AE1–AE12.
- System 5 v2, `system-5-world-exploration.md` ("S5-doc"): hybrid map, corridor act then opening, look-ahead camera, auto-marked gates, 180–260 [estimated] rooms in 8–12 [estimated] regions over 8–12 [estimated] h, no region transformation, exclusions WE1–WE11, gate table GT1–GT16, rest points vs checkpoints (X17).

New mismatches with earlier systems are logged in §13 (X19–X23).

### User decisions applied (14 Nov 2026)
| # | Decision | Applied in |
|---|---|---|
| (r) | **One persistent coin** with set, fixed values per source (lifts the currency part of S3-doc E9 and S4-doc AE5, non-random) | §4, §12 |
| (s) | **P-C death penalty:** half of unspent coin drops into a recoverable marker | §7, §12, X19, X21 |
| (t) | **One shop reached through doors at rest points** (lifts the shop part of S3-doc E8 and S5-doc WE3, fixed stock) | §5, §9.3, §12 |
| (u) | **Fixed-charge healing flask.** Partly reinstates S3-doc E8 (flask) and E14 (health recovery) in non-random, fixed-charge form; logged as X23 | §8, §12, X23 |
| (v) | **Coin fee only** for weapon tiers T2–T5: no tier materials | §4.1, §5.3, X20 |
| (w) | Cooldown reduction **stays excluded** (S3-doc E6) | §12 |
| (x) | Trinkets and notches: **optional, add later.** Moved to §10 "Deferred (post-v1, optional)" and removed from the v1 economy, catalogue and completion formula | §4–§6, §10, §11 |
| (y) | Map, toll and fast-travel prices **stay excluded** (S5-doc WE10) | §9, §12 |
| (z) | Stat growth **stays excluded** (S3-doc E5) | §12 |
| AD-SRC | **Air dash found in an ability room behind an R0 elite** (confirmed), never bought | §3.2 row 6A |
| CR34 | Copied reference values used as defaults are tagged [estimated], basis cited | §0 Tags, §2, §5.3 |

These are design decisions; values they set are tagged [estimated], never [sourced]. System 3 decisions (a)–(e) are decided (user, 14 Nov 2026); §3.3 records their effect on unlocks.

## 0. Conventions

### Tags
- **[sourced]**: stated in a public source.
- **[community-measured]**: counted, timed or polled by third parties (for example player-submitted completion times).
- **[estimated]**: my Bastion Zero value, a user design decision, or arithmetic on other values; the reasoning is given.
- **"provenance unclear"**: a wiki number that may come from game data.
- **Copied defaults (CR34 policy, decided 14 Nov 2026):** a Bastion Zero value copied from a reference is tagged [estimated] with the reference as basis; the reference value itself stays [sourced].
- User-decided defaults are never tagged [sourced].

In tables with a Tag column, the tag applies to every number in that row.

### Units and decided values used here
| Value | Setting | Where defined |
|---|---|---|
| Health | Integer pips; start 5, max 10 | S2-doc C17 (5 [estimated], basis: TM, S4), C18 (10 [estimated], user decision) |
| Respawn health | Full | S2-doc C33 [estimated], user decision |
| Pits | D-Pit 2 [estimated] pips + warp to last safe ground | S2-doc C41, user decision |
| Hazard re-hit lockout | 0.5 [estimated] s | S2-doc C25, user decision |
| Contact damage | On by default | S2-doc §6, user decision |
| Death states | D-1 freeze, D-2 animation, D-3 fade (penalty applied, marker at last safe ground), D-4 respawn load, D-5 grace | S2-doc §9 |
| Damage reduction rule | −1 [estimated] pip on hits of 2 [estimated] or more (basis: TM, S4) | S2-doc §5 rule 3; S3-doc §9 |
| Weapon tiers | T = 1.0, 1.4, 1.8, 2.2, 2.6 [estimated], fixed tiers at fixed locations | S3-doc §9 |
| Loadout | Slot A sword (fixed), slot B secondary, slots C/D skills; B–D swappable at checkpoints | S3-doc §2 |
| Skill cooldowns | Short 4, medium 8, long 15 [estimated] s; no cooldown reduction in v1 (E6) | S3-doc §4 |
| Respawn classes | R0 persistent, R1 rest/death, R2 re-entry, R3 off-camera anchors | S4-doc §11, user decision (f) |
| Bosses | Weighted random attack choice with no-repeat rules; reset to full HP and phase 1 on player death; respawn checkpoint ≤ 1 [estimated] room before the boss gate | S4-doc §10, user decision (i) |
| Elite clone | Designer-placed modifier M6 | S4-doc §9, user decision (k) |
| Map | Hybrid: dotted sketch at once, inked at rest; sketches survive death | S5-doc §11, user decision (l) |
| World shape | Corridor act (15–25% [estimated] of rooms, 2–3 [estimated] regions) then opening | S5-doc §4.3, user decision (m) |
| Camera | Small look-ahead in the direction of travel | S5-doc §10, user decision (n) |
| Gates seen | Auto-marked by type on the map | S5-doc §11.3, user decision (o) |
| World size | 180–260 [estimated] rooms, 8–12 [estimated] regions, 8–12 [estimated] h | S5-doc §3.2, user decision (p) |
| Region transformation | None | S5-doc §4.2, user decision (q) |
| Rest point vs checkpoint | Rest: heal, R1 respawn, map ink, cooldown refresh, full save. Checkpoint: respawn point only. Respawn at the later of the two. | S5-doc §12.1, X17 |
| Fast-travel nodes | 1 [estimated] per region, unlocked on first visit | S5-doc §12.4 |

### Generic terms used in this spec
| Term | Meaning |
|---|---|
| Coin | Bastion Zero's single currency (all three references have one; none of their names is used) |
| Marker | The death-site pickup holding lost coin (S2-doc D-3) |
| Pip fragment | A collectible; a set of 4 [estimated] (HK 4 [sourced], S86) adds 1 [estimated] max pip |
| Tier fee | The coin price of a weapon tier T2–T5 at the shop (decision v) |
| Flask | A fixed-charge healing item (decision u) |
| Shop door | A door at a rest point that leads to the shop (decision t) |
| Technique | A shop-bought move that changes traversal or combat (TM precedent, S4) |

### Clean-room exclusions (sources and values not used)
- **HK death-remnant health formula** on S25 (weapon damage × half of max health): it reads like a game-data formula. Provenance unclear; **not used**. Only the observable behaviour (the remnant holds the currency; dying again loses it) is used.
- **TM penalty-follower drain rate** on S80 ("every 1 currency removes 0.5 s"): not observable from the screen without frame counting and not attributed. Cited once as provenance unclear; **not used as a value**.
- **DC drop chances** on S88 (rare-pickup and gem drop percentages): loot-roll values, not reproduced, excluded with E1.
- **DC forge quality percentages and caps** on S90: random-quality tables, excluded with E2/E3.
- **teemo.dev HK "systems architecture / rebuild blueprint"**: lists internal progression flags read from code (already excluded in S5-doc). Not used.
- **Item and NPC names** on every wiki page opened: not reproduced. Only counts, prices shown in the in-game shops, and behaviour are used.

### Source key
Reused codes keep their earlier meaning. New codes start at S79.

| Code | Source | URL |
|---|---|---|
| S4 | The Messenger Wiki, Shop Upgrades page (catalogue, prices shown in the in-game shop, HP 5→8, charge pool, checkpoint-heal upgrade, penalty upgrade) | https://the-messenger.fandom.com/wiki/Shop_Upgrades |
| S6 | Steam Community, TM "100% Completion Guide & Walkthrough" | https://steamcommunity.com/sharedfiles/filedetails/?id=1643339519 |
| S11 | Hollow Knight Wiki: weapon page (damage tiers; upgrade prices and materials) | https://hollowknight.wiki/w/Nail |
| S22 | Official DC Wiki: Permadeath | https://deadcells.wiki.gg/wiki/Permadeath |
| S24 | Hollow Knight Wiki: protagonist page (health section) | https://hollowknight.wiki/w/The_Knight |
| S25 | Hollow Knight Wiki: death-remnant enemy page | https://hollowknight.wiki/w/Shade |
| S44 | HK Wiki: rest-point page (what resting does; count; tolls) | https://hollowknight.wiki/w/Bench_(Hollow_Knight) |
| S65 | HK Wiki: map and mapping-tool page | https://hollowknight.wiki/w/Map_and_Quill_(Hollow_Knight) |
| S66 | HK Wiki: fast-travel page (station tolls, call bell, trams) | https://hollowknight.wiki/w/Fast_Travel_(Hollow_Knight) (redirect target of /w/Stag_Station) |
| S73 | The Young Folks, PAX East 2019 interview with TM's creative director (portals in the open-world phase) | https://www.theyoungfolks.com/video-games/131649/the-messengers-thierry-boulanger-on-dlc-update-picnic-panic-pax-east-2019/ |
| S75 | Destructoid, interview with TM's creator (campaign length) | https://www.destructoid.com/we-talk-ninja-flips-metroidvanias-and-the-25-year-journey-of-the-messenger-with-creator-thierry-boulanger/ |
| S76 | Official DC Wiki: Biomes (passages between biomes) | https://deadcells.wiki.gg/wiki/Biomes |
| S77 | Official DC Wiki: Runes and upgrades (permanent path unlocks; permanent upgrades bought with the meta currency, with prices; gold kept on death) | https://deadcells.wiki.gg/wiki/Runes |
| S79 | The Messenger Wiki: currency page (sources, sinks, farming by reload) | https://the-messenger.fandom.com/wiki/Time_Shards |
| S80 | The Messenger Wiki: death-penalty follower character page | https://the-messenger.fandom.com/wiki/Quarble |
| S81 | The Messenger Wiki: shop location page (reached from shop checkpoints) | https://the-messenger.fandom.com/wiki/The_Shop |
| S82 | The Messenger Wiki: challenge-room collectible page (45 collectibles; reward) | https://the-messenger.fandom.com/wiki/Power_Seals |
| S83 | Hollow Knight Wiki: currency page (denominations, tolls, merchant totals, sources) | https://hollowknight.wiki/w/Geo |
| S84 | Hollow Knight Wiki: charms page (notches, equip only at rest points, overcharm) | https://hollowknight.wiki/w/Charms |
| S85 | Hollow Knight Wiki: completion page (percentage weights) | https://hollowknight.wiki/w/Completion_(Hollow_Knight) |
| S86 | Hollow Knight Wiki: max-health fragment page (4 per unit; shop prices) | https://hollowknight.wiki/w/Mask_Shard_(Hollow_Knight) |
| S87 | Hollow Knight Wiki: soul meter page (healing spends a third of the meter) | https://hollowknight.wiki/w/Soul |
| S88 | Official DC Wiki: Pickups (meta currency, gold, food; lost on death). Its drop-chance rows are excluded. | https://deadcells.wiki.gg/wiki/Cells |
| S90 | Official DC Wiki: the forge NPC page (permanent investment of meta currency) | https://deadcells.wiki.gg/wiki/Forge |
| S91 | Official DC Wiki: the meta-currency NPC page (located in passages between biomes; purchases carry over) | https://deadcells.wiki.gg/wiki/The_Collector |
| S92 | Game Rant, "How Long it Takes to Beat Hollow Knight…" (HowLongToBeat player polls) | https://gamerant.com/hollow-knight-how-long-to-beat-all-dlc-collectibles-silksong/ |
| S93 | MoonCellar, The Messenger page (HowLongToBeat figures) | https://mooncellar.space/games/the-messenger |

---

## 1. Excluded (roguelite randomization, run structure, out of scope)

| # | Excluded | Reference | Why | What remains |
|---|---|---|---|---|
| PE1 | Permadeath: losing items, meta currency, upgrades picked up in the run and most gold on death (S22) | DC | Run structure | Persistent world: nothing but unspent coin is ever at risk (§7) |
| PE2 | Meta currency that is lost on death and can only be spent in passages between biomes (S88, S91) | DC | Run structure | One persistent coin, spendable at any time at a shop (§4–§5) |
| PE3 | Gold kept on death up to a purchased cap of 2,500 [sourced] (S22, S77) | DC | Run framing | The marker model (§7) does the same job |
| PE4 | Forge investment that raises the chance of higher-quality random items (S90) | DC | Loot roll (E2/E3) | Fixed weapon tiers (S3-doc §9) |
| PE5 | Blueprints dropped by enemies, random starter weapons, shop restock and category picks (S77) | DC | Loot and shop randomization (E1, WE3) | Fixed-location items, fixed shop stock (§5) |
| PE6 | Recycling found items into gold (S77) | DC | Exists to absorb random loot | None |
| PE7 | Upfront revival payment that scales per new-game-plus cycle, with a game over when unpaid (S80) | TM | New-game-plus mode, out of scope | None in v1 |
| PE8 | Enemy currency drops that happen "sometimes" (S79) | TM | Random drop | Fixed coin per kill (§4.2) |
| PE9 | Daily runs, custom mode and difficulty-ladder unlocks (S77) | DC | Run structure | None |
| PE10 | Breakable equippables that break on death and need paid repair (S83) | HK | A second death penalty on top of the currency one | None in v1 (Open Question 6) |
| PE11 | A permadeath difficulty mode | HK (optional mode) | Out of scope for v1 | None in v1 (Open Question 7) |
| PE12 | Story-only currency and trade items (relics sold to a trader NPC, eggs traded for services) (S83, S25) | HK | Narrative economy | None |

---

## 2. Reference behaviour

### 2.1 Comparison
| Topic | TM | HK | DC |
|---|---|---|---|
| Ability unlocks | Traversal items from story beats and bosses; three further moves (projectile bounce, recovery jump, glide down-strike) are **bought** in the shop (S4) | Abilities found in fixed places or given after bosses; never bought | Permanent path runes dropped by guardian elites in fixed rooms; collecting one unlocks it at once, no payment (S77) |
| Currency | One currency from enemies (1 [sourced] or nothing), lanterns (3 [sourced]), large crystals (65 [sourced] in total, respawning on area change or reload) and pickups (S79) | One currency in denominations of 1, 5 and 25 [sourced] (S83), from enemies, deposits, chests, quests and arena trials (S83) | Two: gold (spent in the run) and a meta currency (spent on permanent unlocks); both mostly lost on death (S88, S22) |
| Main sinks | 19 [sourced] shop upgrades costing 6,845 [estimated] in total (my sum of the prices listed on S4); a hint service; a bottomless sink after everything is bought (S79) | Maps, pins, max-health fragments, notches, equippables, weapon upgrades, tolls for stations and rest points; 69,210 [sourced] to buy everything at the lowest prices (S83) | Permanent upgrades such as healing-flask charges 1→4 [sourced] for 5, 50, 150 and 300 [sourced] meta currency (S77); forge investment (S90) |
| Shop | One shop, reached through shop checkpoints in nearly every area (S81) | Several merchants in fixed places; stock grows with progress (S83, S86) | Random shops inside biomes; the meta-currency NPC sits in passages between biomes (S91) |
| Max health growth | 5 → 8 [sourced] by shop purchases (S4) | 5 → 9 [sourced]; 4 [sourced] fragments make one unit, 16 [sourced] fragments in total; 4 [sourced] of them sold at 150, 500, 800 and 1,500 [sourced] (S86) | Stat points from random scrolls (E5) |
| Weapon growth | Charged-strike upgrade (×3 [sourced]) and a damage-reduction upgrade (S4) | 4 [sourced] paid upgrades, 7,050 [sourced] in total, the last three also needing 1, 2 and 3 [sourced] fixed-location materials (S11, S83) | Random gear levels and quality (E2/E3) |
| Healing | Enemy health drops that appear "sometimes" (bought upgrade), potions, and a checkpoint-heal upgrade (S4) | Spend a third of a meter filled by hitting enemies (S87); rest points heal fully (S44) | Flask charges, food (S77, S88) |
| Loadout | None | Equippables that cost notches; 3 → 11 [sourced] notches; swap only at rest points; overfilling doubles damage taken (S84) | 2 [sourced] weapons, 2 [sourced] skills, 1 [sourced] amulet (S3-doc §2) |
| Save points | Frequent checkpoints (S5-doc S57, S58); some open a door to the shop (S81) | 50 [sourced] rest points; resting saves, heals, re-inks the map and respawns enemies outside the current room (S44) | Between-biome passages |
| Death penalty | A follower takes newly collected currency for 60 [sourced] s, 30 [sourced] s with an upgrade, and leaves before bosses (S4, S80) | All currency stays with a hostile remnant at the death site; a third of the meter is broken until it is beaten; dying again first loses that currency (S25) | Run ends; nearly everything is lost (S22) |
| Fast travel | Portals in the open-world phase (S73) | 11 [sourced] stations with tolls of 50–300 [sourced] each, 1,560 [sourced] in total; a call bell; 2 [sourced] tram lines needing a pass (S66, S83) | Teleport stones inside biomes after a permanent rune (S77) |
| Completion | 45 [sourced] challenge-room collectibles with a reward for all (S82); a community 100% guide (S6) | A percentage of up to 112 [sourced]: 100 [sourced] in the base game, 1 [sourced] point per boss, equippable, spell, etc.; rooms explored are **not** counted (S85) | Unlock counts and runes shown on the save slot (S77) |
| Campaign length | 8–10 [sourced] h by its creator (S75); 12 / 14.5 / 16 [community-measured] h main / extras / completionist (S93) | 27 / 41.5 / 64.5 [community-measured] h (S92) | Not used (run-based) |

### 2.2 Disagreements and gaps
- **Where currency goes on death:** TM taxes future income, HK parks all of it in a fight, DC deletes it. Three different stakes. §7 reconciles them: **Decided (user, 14 Nov 2026):** P-C half drop (decision s).
- **Who sells abilities:** TM sells three moves; HK and DC never sell traversal. §3 keeps gate-critical abilities as found items and sells only optional or SB1 techniques.
- **Where to shop:** TM's one shop with many doors vs HK's scattered merchants. §5 uses TM's doors: **Decided (user, 14 Nov 2026):** decision (t).
- **Healing:** three unrelated models. S2-doc deferred healing; S3-doc E8 and E14 excluded flasks and recovery health. §8: **Decided (user, 14 Nov 2026):** a fixed-charge flask (decision u), partly reinstating E8 and E14.
- **Completion:** HK counts items and bosses, DC counts unlocks, TM counts collectibles. None counts rooms. §11 adds map exploration because S5's region completion already exists.
- **No public income data.** None of the sources gives currency earned per hour. TM's shard totals in the world and HK's total currency available are not in the pages opened. Every rate in §4 is [estimated].
- **Campaign-length figures disagree:** TM's creator says 8–10 [sourced] h (S75); player polls say 12 [community-measured] h for the main story (S93).

---

## 3. Ability unlocks

### 3.1 Rules
1. **Gate-critical abilities are found, never bought.** Every ability that opens an SB0 gate (S5-doc §5.2) comes from a fixed ability room behind an R0 elite or boss (S4-doc GT12, R0). HK and DC both do this (S77). Reason: a price on an SB0 key can softlock a poor player.
2. **Techniques may be bought.** Moves that open only SB1 or SB2 gates, or no gate, may be shop items (TM sold three, S4).
3. **Abilities are permanent.** Nothing in §7 removes an ability, upgrade or purchase. Only unspent coin is at risk.
4. **Collected at once.** Picking up an ability sets its flag and saves immediately (§6.3). No need to rest (DC runes unlock at once, S77).
5. **Lock-and-key show** (S5-doc §5.3 rule 2): the room that gives an ability has its exit behind that ability's gate, within 1–3 [estimated] rooms.
6. **Last traversal ability** is obtained at least 1.5 [estimated] h before the end of a median campaign, so the 20–35% [estimated] revisit share (S5-doc §12.4) can happen.

### 3.2 Unlock order template
Times are for a median first completion of 10 [estimated] h (mid-point of the approved 8–12). Gate IDs refer to S5-doc §5.2.

| # | Phase (S5-doc §4.3) | Time window | Ability or technique | Source | Opens | SB | Tag |
|---|---|---|---|---|---|---|---|
| 0 | Start | 0 h | Base kit: run, jump, bounce-attack (S1-doc), ground dash and block with parry (S3-doc §5; decisions b–c, DB-1, DB-2) | Start | GT6 bounce field; GT11 trap corridor (timing only, decision a) | SB2 | [estimated] |
| 1 | WP-CORRIDOR, region 1 end | 0.5–1.0 h | Wall-cling | Ability room after an R0 elite | GT1 high ledge, GT2 tall shaft | SB0–SB1 | [estimated] |
| 2 | WP-CORRIDOR, shop opens | 0.75–1.5 h | Projectile bounce, including destroying enemy projectiles (S1-doc §3; S3-doc shop-style unlock) | Shop, stock tier ST1 (§5) | GT7 projectile stair | SB1 | [estimated] |
| 3 | WP-CORRIDOR, last region end | 1.5–2.5 h | Glide | Boss reward (R0) | GT3 wide gap, GT4 updraft | SB0–SB1 | [estimated] |
| — | WP-OPENING | 1.5–2.5 h | The opening event (S5-doc Open Question 6). Fast travel and shop stock tier ST2 switch on (§9, §5). | — | — | — | [estimated] |
| 4 | WP-OPEN | 2.5–4.0 h | Grapple | Ability room after an R0 elite | GT5 hook gap | SB1 | [estimated] |
| 5 | WP-OPEN | 2.5–4.5 h | Heavy secondary weapon (S3-doc §3.4, slot B) | Fixed location | GT9 cracked heavy wall; alternative for GT8 | SB1 | [estimated] |
| 6 | WP-OPEN | 4.0–6.0 h | Glide down-strike | Found (ability room). *Considered:* sold, as TM did (S4); not used because GT8 is SB0. | GT8 breakable floor | SB0 | [estimated] |
| 6A | WP-OPEN | 5.5–7.0 h | **Air dash** (S3-doc §5.2; **Decided (user, 14 Nov 2026)**, DB-3: a later unlock). Uses S1-doc's dash unlock flag. | Ability room after an R0 elite (gate-critical, so found, never bought: rule 1; **Decided (user, 14 Nov 2026)**, AD-SRC) | GT17 air-dash gap (S5-doc §5.2); hard gaps for later gates rise to ≥ 12 tiles | SB0 | [estimated] |
| 7 | WP-OPEN, optional | from 2.5 h | Recovery jump during knockback (S1-doc H-KNOCK → A-RISE-HELD) | Shop, stock tier ST2 | No gate (comfort; the optional ability of S5-doc §5.3 rule 4) | n/a | [estimated] |
| 8 | Late | by 8.5 h | Last traversal ability placed (rule 6) | — | — | — | [estimated] |

**Corridor act check:** abilities 1–3 give 2 [estimated] traversal abilities and 1 [estimated] technique in the corridor, inside S5-doc's 1–2 [estimated] ability target (the technique is not a traversal ability).
**Cadence check:** 7 [estimated] abilities and techniques between 0.5 and 7.0 [estimated] h (the air dash added as row 6A) is about 1 per 56 [estimated] (arithmetic) minutes. That is at the fast end of S5-doc's 1 per 60–100 [estimated] minutes; the corridor act's quick start accounts for the difference. Row 6A ends by 7.0 h, inside rule 6 (last traversal ability by 8.5 [estimated] h).

### 3.3 Effect of System 3 decisions (a)–(e) on unlocks (**Decided (user, 14 Nov 2026)**)
| Decision | Outcome | Effect on unlocks |
|---|---|---|
| (a) Traps | Nothing dodges hazards (S2 rule) | GT11 needs timing only; no unlock |
| (b) Shield archetype | Removed; the block is a core action | No shield item in slot B; GT13 removed (S5-doc) |
| (c) Roll and air roll | Removed; the dash replaces them | Nothing to sell |
| DB-1 Dash i-frames | 8 f, enemy sources only | No effect on unlocks |
| DB-2 Parry | On, part of the base-kit block | No effect on unlocks; no parry item or upgrade in v1 |
| DB-3 Air dash | Later unlock | New row 6A: found in an ability room behind an R0 elite; opens GT17. Ground dash stays base kit (X24 now limited to the ground dash). |
| DB-4 1-tile tunnels | Dropped | GT10 removed from the base-kit row |
| (d), (e) | Approved | No effect on unlocks |

**DB-3 applied:** before row 6A, S5-doc hard gaps are ≥ 9 [estimated] tiles; gates whose key comes after the air dash use ≥ 12 [estimated] tiles.

---

## 4. Currency and upgrade loops

### 4.1 The loops
| Loop | Earn | Spend | Saved as | Reference |
|---|---|---|---|---|
| Ability loop | Tease a gate → find the ability → open gates → backtrack | n/a | P0 flags | S5-doc §5.3 |
| Coin loop | Kills, caches, bosses, challenge rooms | Shop catalogue (§5) | Wallet (P0 when spent; at risk when unspent, §7) | TM (S4, S79), HK (S83) |
| Pip loop | 3 [estimated] sets of 4 [sourced, HK S86] pip fragments + 2 [estimated] shop pips | +1 max pip each; 5 → 10 [estimated] | P0 | HK (S86), TM (S4) |
| Tier loop | Coin | **Decided (user, 14 Nov 2026):** weapon tier T2–T5 (S3-doc §9) bought for a coin fee only, no materials (decision v); each tier appears in a later stock tier (§5.3) | P0 | HK paid coin plus 1, 2, 3 [sourced] materials (S11); Bastion Zero drops the materials |
| Flask loop (**Decided (user, 14 Nov 2026):** decision u) | 1 [estimated] flask charge found, 1 [estimated] bought | Charges refill at rest points | P0 capacity; P1 charges | DC (S77) |

### 4.2 Coin sources (fixed, no randomness)
Coin per kill scales with the progression tier of the room (S4-doc §13). This is a reward scale, not enemy scaling, so AE6 is untouched.

| Source | Base coin | Tier multiplier T1 / T2 / T3 / T4 | Respawns? | Tag |
|---|---|---|---|---|
| Fodder (R2, R3) | 1 | ×1 / 1 / 1 / 1 (flat, to cap farming) | Yes | [estimated] (TM 1 [sourced] per enemy, S79) |
| Soldier, shooter, charger, shielded (R1) | 3 | ×1 / 1.5 / 2 / 2.5 | At rest or death | [estimated] |
| Brute (R1) | 8 | ×1 / 1.5 / 2 / 2.5 | At rest or death | [estimated] |
| Elite (R0) | 60 | ×1 / 1.5 / 2 / 2.5 | Never | [estimated] |
| Elite with M6 clone | 80, split equally between the two copies | Same | Never | [estimated] |
| Boss (R0) | 300 | ×1 / 1.5 / 2 / 2.5 | Never | [estimated] |
| Small cache (breakable, one-time) | 25 | Flat | Never (P0) | [estimated] (TM lantern 3 [sourced], S79) |
| Large cache (secret room, one-time) | 150 | Flat | Never | [estimated] (TM large crystal 65 [sourced] in total, S79) |
| Challenge room (S5-doc §7, optional skill room) | 250 | Flat | Never | [estimated] |

**Plain math, earn per hour:**
- Enemy income per hour: E_enemy = Σ (kills per hour of class k × base_k × tier multiplier).
- Cache income per hour: E_cache = new rooms per hour ÷ rooms per reward × share of rewards that are coin × average cache value.
- Boss and elite income per hour: E_R0 = (bosses per hour × 300 + elites per hour × 60) × tier multiplier, with 300 and 60 [estimated] from the table above.
- Total: E = E_enemy + E_cache + E_R0.

**Worked median, at tier T2:**
- New rooms per hour = 220 [estimated] rooms ÷ 10 [estimated] h ≈ 22 [estimated] (arithmetic); rooms entered per hour including revisits ≈ 40 [estimated].
- E_enemy ≈ 40 rooms × 2 R1 kills × 3 × 1.5 + 40 × 3 fodder × 1 ≈ 360 + 120 = 480 [estimated] (arithmetic).
- E_cache ≈ 22 ÷ 4 rooms per reward × 0.5 coin share × 60 average ≈ 165 [estimated] (arithmetic).
- E_R0 ≈ (1 boss per hour × 300 + 1.5 elites × 60) × 1.5 ≈ 585 [estimated] (arithmetic).
- E ≈ 1,230 coin per hour [estimated] (arithmetic).

### 4.3 Target economy curve
Hours are a median first completion of 10 [estimated] h. "Catalogue" is the v1 shop set in §5.3 (10,000 [estimated] coin after the decisions).

| Hour | Tier | Earn rate (coin/h) | Cumulative earned | Catalogue unlocked for sale | Cumulative earned ÷ unlocked | Tag |
|---|---|---|---|---|---|---|
| 0–2 (corridor act) | T1 | 450–650 | 900–1,300 | ST1: 1,100 | 0.8–1.2 | [estimated] |
| 2–4 | T2 | 900–1,250 | 2,700–3,800 | ST1+ST2: 3,600 | 0.75–1.05 | [estimated] |
| 4–7 | T3 | 1,100–1,450 | 6,000–8,150 | ST1–ST3: 6,900 | 0.85–1.2 | [estimated] |
| 7–10 | T4 | 1,200–1,600 | 9,600–12,950 | ST1–ST4: 10,000 | 0.95–1.3 | [estimated] |

**Curve rules:**
1. **Scarcity in the middle.** Between 2 and 7 [estimated] h the ratio sits at 0.75–1.2 [estimated] (rebalanced after decisions v and x), and below 1.0 [estimated] for most players just after each stock tier opens: the player must choose what to buy first.
2. **Everything by the end.** A median player who explores (not 100%) can afford the whole catalogue by about 90% [estimated] of the campaign.
3. **Critical-path floor.** A player who skips all optional caches and challenge rooms still earns at least 60% [estimated] of the full-exploration figure.
4. **Required purchases are cheap.** Shop items that open any gate (only GT7 in the template) cost ≤ 25% [estimated] of the guaranteed critical-path coin earned before that gate's first required use, and are on sale ≥ 30 [estimated] minutes before it.
5. **Death losses.** Planned permanent loss from markers (§7) is ≤ 5% [estimated] of coin earned over a campaign. The curve above already includes it.
6. **Farming is allowed but slow.** Re-killing R1 and R2 enemies yields at most 50% [estimated] of the exploration rate per hour. TM let players farm large crystals by reloading (S79); Bastion Zero allows farming but makes exploration pay better.
7. **Scaling if the catalogue changes.** If items are added later (for example the deferred trinket notches, §10), multiply every coin source by k = catalogue total ÷ 10,000 [estimated].

### 4.4 Reference ratios behind the numbers
| Ratio | TM | HK | Bastion Zero | Tag |
|---|---|---|---|---|
| Total catalogue ÷ main-story hours | 6,845 ÷ 12 ≈ 570 per hour (S4, S93) | 69,210 ÷ 41.5 ≈ 1,670 per hour, buy-everything over main + extras (S83, S92) | 10,000 ÷ 10 = 1,000 per hour | [estimated] (arithmetic) on [sourced] and [community-measured] inputs |
| Most expensive item ÷ catalogue | 2,000 ÷ 6,845 ≈ 29% (S4) | 4,000 ÷ 69,210 ≈ 6% (S11, S83) | 1,600 ÷ 10,000 = 16% | [estimated] (arithmetic) |
| Damage-reduction upgrade ÷ catalogue | 1,000 ÷ 6,845 ≈ 15% (S4) | n/a | 1,500 ÷ 10,000 = 15% | [estimated] (arithmetic) |
| Max-health price ladder (first ÷ last) | 30 → 350 (S4) | 150 → 1,500 (S86) | 300 → 900 | [estimated] |

---

## 5. Shops

### 5.1 Shop rules
1. **Fixed stock, fixed prices.** No rerolls, restocks, discounts or random items (PE5; S5-doc WE3).
2. **Stock grows with progress.** Items appear in stock tiers tied to world flags, as HK's merchants add stock after a key or a count of items (S83, S86):

| Stock tier | Unlocks when | Tag |
|---|---|---|
| ST1 | The shop is first found (corridor region 1–2) | [estimated] |
| ST2 | The opening event (WP-OPEN) | [estimated] |
| ST3 | 3 [estimated] bosses defeated | [estimated] |
| ST4 | 6 [estimated] bosses defeated, or 75% [estimated] of regions entered | [estimated] |

3. **Visible prices.** Items not yet in stock show as silhouettes with their unlock condition.
4. **Purchases are permanent** and saved at once (P0). No selling, no refunds.
5. **One shop, many doors** (**Decided (user, 14 Nov 2026):** decision t): one shop room reached through a shop door at 1 [estimated] rest point per region. TM's shop opened from shop checkpoints in nearly every area (S81). Leaving returns the player to the same door. *Considered:* separate merchants placed in the world, as in HK (S83); not used.
6. **Shop doors are rest points**, never checkpoints, so a shop visit also heals and inks the map.
7. **Shop is safe.** No enemies, no R1 respawn on entry; entering does not count as resting.
8. **Coin past the catalogue.** Once everything is bought, extra coin has no use in v1. TM added a bottomless sink (S79). Open Question 4.

### 5.2 Shop and item state lists
**Shop:** `SH-UNFOUND → SH-FOUND (first shop door used) → stock tier ST1 → ST2 → ST3 → ST4`

| Item state | Shown | Enter | Exit |
|---|---|---|---|
| IT-HIDDEN | Nothing | Start | Its stock tier unlocks → IT-LOCKED or IT-AVAILABLE |
| IT-LOCKED | Silhouette and condition (for example "needs glide") | Stock tier unlocked, a prerequisite missing | Prerequisite met → IT-AVAILABLE |
| IT-AVAILABLE | Name, effect, price | Stock tier and prerequisites met | Bought → IT-OWNED |
| IT-OWNED | Shown as owned | Purchase (coin deducted, saved) | Final |

### 5.3 v1 catalogue (generic items)
Rebalanced after decisions (v) and (x): notches removed (deferred, §10) and tier fees raised so the tiers carry their own cost without materials. The total stays 10,000 [estimated].

| Item | Effect | Stock tier | Price (coin) | Source of decision | Tag |
|---|---|---|---|---|---|
| Projectile bounce | Attacks destroy enemy projectiles and can bounce off them (S1-doc §3) | ST1 | 100 | Base | [estimated] (TM 40 [sourced], S4) |
| Max pip A | +1 max pip | ST1 | 300 | Base | [estimated] (TM 30 [sourced]; HK 150 [sourced]) |
| Ranged charge +1 | +1 charge pip for the ranged archetype (S3-doc §3.4) | ST1 | 400 | Base | [estimated] (TM 250 [sourced]) |
| Weapon tier T2 | Slot A damage ×1.4 [estimated] (S3-doc §9) | ST1 | 300 | Decided (v) | [estimated] (HK 250 [sourced], S11) |
| Recovery jump | Jump out of knockback | ST2 | 300 | Base | [estimated] (TM 80 [sourced]) |
| Max pip B | +1 max pip | ST2 | 900 | Base | [estimated] (TM 320 [sourced]; HK 500–800 [sourced]) |
| Flask charge +1 | See §8 | ST2 | 700 | Decided (u) | [estimated] (DC 50–300 [sourced] meta currency, S77) |
| Weapon tier T3 | ×1.8 [estimated] | ST2 | 600 | Decided (v) | [estimated] (HK 800 [sourced] plus 1 [sourced] material, S11) |
| Ranged charge +1 | +1 charge pip | ST3 | 800 | Base | [estimated] (TM 350 [sourced]) |
| Damage reduction | −1 pip on hits of 2 or more (S2-doc §5 rule 3) | ST3 | 1,500 | Base | [estimated] (TM 1,000 [sourced]) |
| Weapon tier T4 | ×2.2 [estimated] | ST3 | 1,000 | Decided (v) | [estimated] (HK 2,000 [sourced] plus 2 [sourced] materials, S11) |
| Charged strike | Holding still charges the next slot A hit to ×3 [estimated] (basis: TM, S4) | ST4 | 1,600 | Base | [estimated] (TM 2,000 [sourced]) |
| Weapon tier T5 | ×2.6 [estimated] | ST4 | 1,500 | Decided (v) | [estimated] (HK 4,000 [sourced] plus 3 [sourced] materials, S11) |

**Totals (plain math, rebalanced):**
- Base items: 100 + 300 + 400 + 300 + 900 + 800 + 1,500 + 1,600 = 5,900 [estimated] (arithmetic), unchanged.
- Flask: 700 [estimated], unchanged.
- Tier fees: 300 + 600 + 1,000 + 1,500 = 3,400 [estimated] (arithmetic), was 0 + 300 + 600 + 900 = 1,800 [estimated] with materials.
- Notches: 0 in v1, was 200 + 500 + 900 = 1,600 [estimated] (deferred, §10).
- v1 catalogue: 5,900 + 700 + 3,400 = 10,000 [estimated] (arithmetic), unchanged.
- By stock tier: ST1 1,100; ST2 2,500; ST3 3,300; ST4 3,100 [estimated] (arithmetic). Was ST1 1,000; ST2 2,700; ST3 3,800; ST4 2,500 [estimated].
- Weapon tiers are 34% [estimated] (arithmetic) of the catalogue, the largest sink. HK's weapon upgrades were about 10% [estimated] (arithmetic, 7,050 ÷ 69,210, S83) of its total, but HK also gated them on materials; here coin is the only gate.

**Tier rules (decision v):**
1. Tiers are bought in order; each appears in the stock tier above, so weapon power can't outrun the region tiers (S4-doc §13).
2. No tier materials and no fixed-location tier pickups (X20).
3. A tier is never required to open a gate.

---

## 6. Save, checkpoint and rest

### 6.1 Persistence classes
| Class | What | Cleared by | Reference |
|---|---|---|---|
| P0 Permanent | Abilities, techniques, purchases, pip fragments and pips, weapon tiers, R0 defeats, broken walls, opened shortcuts, one-time caches, rest points and fast-travel nodes found, inked map, seen-gate icons, world phase, spent coin | Never | S4-doc R0; S5-doc §7–§8, §11 |
| P0-W Wallet | Unspent coin | Death moves part of it to a marker (§7) | S2-doc D-3 |
| P1 Rest-scoped | R1 enemies, flask charges (decision u), skill cooldowns, healing | Rest, death or reload | S4-doc R1; S5-doc §12.1 |
| P2 Room-scoped | R2 enemies, collapsing platforms (S2-doc C16), R3 anchors | Room load | S4-doc R2–R3 |
| P3 Death-scoped | The marker and its coin | Recovery or the next death | §7 |
| Sketch | Dotted map sketch | Never cleared; inked at rest | S5-doc decision (l) |

### 6.2 Save point types
| Feature | Rest point | Checkpoint | Shop door (rest point with a door) |
|---|---|---|---|
| Sets respawn | Yes | Yes | Yes |
| Heals, refills the flask, refreshes cooldowns | Yes | No | Yes |
| R1 respawn (not in the current room) | Yes | No | Yes |
| Inks the map | Yes | No | Yes |
| Full save write | Yes | Position, wallet and flags | Yes |
| Swap slot B–D (S3-doc §2) | Yes | Yes | Yes |
| Fast travel | Only if it has a node (§9) | No | Only if it has a node |

### 6.3 Save write triggers
| Trigger | What is written | Tag |
|---|---|---|
| Sitting at a rest point | Everything (full save) | n/a |
| Touching a checkpoint | Respawn point, wallet, all P0 flags | n/a |
| Ability, upgrade or fragment pickup | That flag and the wallet, within 1 [estimated] s | [estimated] |
| Purchase | Item flag and wallet | n/a |
| R0 defeat (elite or boss) | R0 flag, wallet | n/a |
| Shortcut opened, wall broken, cache opened | That flag, wallet | n/a |
| Marker created or recovered | Marker state and wallet | n/a |
| Fast travel arrival | Respawn point (the node's rest point is **not** rested at) | n/a |

**Rules:**
1. **Autosave only.** No manual save; HK and TM both save at fixed points (S44, S81).
2. **Coin picked up after the last write is at risk on a crash or quit.** That is at most one checkpoint gap: 1–3 [estimated] rooms (S5-doc §12.3).
3. **Reload = soft death without penalty:** resume at the later of the last checkpoint and the last rest point, full health, R1 respawned, no marker. One-time caches stay opened, so reloading can't farm them (unlike TM's reload-respawning crystals, S79).
4. **No quit-to-dodge.** The death penalty is fixed at D-1 and written at D-3. Quitting during D-1 to D-3 still applies it (X21).
5. **Save slots:** 3 [estimated].

### 6.4 State lists
**Rest point** (extends S5-doc §12.2):
`RP-UNFOUND → RP-FOUND (pinned) → RP-RESTING (heal, flask refill, cooldowns, R1 respawn, ink, full save) → RP-ACTIVE (respawn here) ⇄ RP-FOUND (when a later checkpoint or rest point becomes active)`

**Checkpoint:**
`CP-UNTOUCHED → CP-ACTIVE (touched; respawn here; partial save) → CP-STALE (a newer checkpoint or rest point is active) → CP-ACTIVE (touched again)`

**Save file:**
| State | Meaning | Exit |
|---|---|---|
| SV-CLEAN | Disk matches game state | Any P0 or wallet change → SV-DIRTY |
| SV-DIRTY | Unwritten changes | A write trigger (§6.3) → SV-WRITING |
| SV-WRITING | Write in progress; a save icon shows; input stays on | Done → SV-CLEAN; error → SV-FAIL |
| SV-FAIL | Write failed | Retry at the next trigger; warn the player after 2 [estimated] failures |

---

## 7. Death penalty and recovery

### 7.1 Model (**Decided (user, 14 Nov 2026):** P-C, decision s)
**P-C half drop:** 50% [estimated] of unspent coin goes to a marker at the last safe ground; touching it recovers the coin; dying again first loses that marker's coin; no penalty in the corridor act. Mix of HK (S25) and TM (S80).

**Why:** Bastion Zero has TM-style checkpoints every 1–3 [estimated] rooms (S5-doc §12.3), so deaths are frequent and quick. HK's full drop is built for rest points far apart; TM's follower was its designer's replacement for lives and continues (S80). Half the wallet keeps a real stake without punishing a run of retries.

*Considered, not used:* P-A, a TM-style follower taking new coin for 60 [sourced] s (S4, S80); P-B, HK's full drop (S25); P-D, no penalty (kept only as an assist option, see Dependencies); an HK-style remnant enemy that must be beaten (S4-doc has no such archetype, and a static marker is easier to read).

### 7.2 Death and recovery state list (P-C)
Runs alongside S2-doc D-1 to D-5.

| State | Enter | Behaviour | Exit |
|---|---|---|---|
| DP-ALIVE | Respawn or load | Normal play | HP 0 or D-Lethal → DP-DYING |
| DP-DYING | S2-doc D-1 | Penalty fixed now: lost = floor(0.5 × wallet) [estimated]. In the corridor act (WP-CORRIDOR) lost = 0. | S2-doc D-3 → DP-MARKED or DP-ALIVE |
| DP-MARKED | D-3, with lost > 0 | Any older marker is destroyed and its coin lost. A new marker holding `lost` is placed (§7.3). Wallet −= lost. Saved. | D-4/D-5 → DP-ALIVE-OWED |
| DP-ALIVE-OWED | Respawn with a marker in the world | Marker pinned on the map (sketch or ink). Coin picked up goes to the wallet as normal. | Touch marker → DP-RECOVERED; die → DP-DYING |
| DP-RECOVERED | Marker touched | Wallet += marker coin; marker removed; saved | → DP-ALIVE |

### 7.3 Marker placement rules
1. **At the last safe-ground point** (S2-doc D-3), never inside a pit, hazard or kill-plane volume.
2. **Arena deaths:** the marker goes in the room before the boss gate, next to the respawn checkpoint (S4-doc: ≤ 1 [estimated] room before B-GATE). HK places the remnant somewhere safe after boss deaths (S25). The player can recover it between attempts without starting the fight (X21).
3. **Lock-in wave rooms** (S4-doc §9): the marker goes outside the lock volume.
4. **Never across a one-way link** from the respawn point: if the last safe ground is beyond a one-way link (S5-doc GT3, GT8, GT15), the marker moves to the nearest safe ground on the respawn side.
5. **Marker has no collision, no enemy and no timer.** It never decays.
6. **Map:** auto-pinned with a distinct icon (S5-doc §11.3), shown on the sketch and on ink.

### 7.4 Death penalty values
| Value | Setting | Tag | Reference |
|---|---|---|---|
| Share of wallet dropped | 50% | [estimated] | HK 100% [sourced] (S25); TM taxes future income instead (S80) |
| Markers in the world | 1 at a time | [estimated] | HK 1 [sourced] remnant (S25) |
| Penalty in the corridor act | None | [estimated] | none (teaching phase) |
| Pit damage, hazard re-hit, respawn health | Unchanged: D-Pit 2, lockout 0.5 s, full | [estimated] | S2-doc, user decisions |
| Other losses | None: abilities, upgrades, map sketch and ink, R0 flags all kept | n/a | Persistent world (PE1) |

---

## 8. Healing: fixed-charge flask (**Decided (user, 14 Nov 2026):** decision u)

This partly reinstates two System 3 exclusions in a non-random, fixed-charge form: E8 (flask charges) and E14 (health recovery). See §12.1 and X23.

| Value | Setting | Tag | Reference |
|---|---|---|---|
| Charges at start | 1 | [estimated] | DC starts at 1 [sourced] after its first upgrade (S77) |
| Max charges | 3 (1 found at a fixed location + 1 bought, §5.3) | [estimated] | DC 4 [sourced] (S77) |
| Heal per use | 3 pips (30% [estimated] of 10) | [estimated] | DC 60% [sourced] of health (S77) |
| Use | 0.5 s channel at 50% move speed; the heal lands at the end | [estimated] | DC heals over a few seconds (S77) |
| Interrupt | Damage during the channel cancels it; the charge is kept | [estimated] | none |
| Refill | Rest points only (not checkpoints); also on death and reload (P1) | [estimated] | S5-doc §12.1 |
| Randomness | None: no drops, no random refills | n/a | E1, E8 food stay excluded |

**Rules:**
1. **Only the flask is reinstated.** DC's rally-style recovery health (S3-doc E14, S2-doc §5 rule 5), one-hit protection and food stay excluded. The lethal-hit guard stays assist-only.
2. **Never above max pips.**
3. **Usable in arenas.** S4-doc expected pip-loss budgets (≤ 4–7 [estimated] of 10 between rest points) assumed no healing. Boss budgets may rise by at most 3 [estimated] pips per charge carried. Playtest.
4. **Usable in:** ground states and A-GLIDE; not in X-GRAPPLE, H-KNOCK, H-WARP, D-states, C-DASH or BK-BREAK (guard break) [estimated]. Drinking drops the block (S3-doc §5.3).

*Considered, not used:* rest-point healing only; fixed pickups from breakables (TM health drops made non-random, S4); a meter heal (HK, S87), which conflicts with S3-doc §4 rule 1.

---

## 9. Fast travel

### 9.1 Rules
1. **Nodes sit at rest points** (S5-doc §12.1) and unlock on first visit (S5-doc §12.4). HK stations needed a toll and a bell (S66); Bastion Zero has neither. **Decided (user, 14 Nov 2026):** travel stays free (decision y).
2. **No fast travel in the corridor act.** The network switches on at WP-OPENING; corridor nodes found earlier become usable then.
3. **Any found node to any found node.** Free.
4. **Only from a node, not from the menu.** Keeps rest points meaningful. *Considered:* a single player-placed warp point as in HK (S66); Open Question 5.
5. **Arrival does not rest.** R1 doesn't respawn; the player may sit to rest.
6. **Blocked** while an arena or lock-in room is active, and during D-states.
7. **Travel time:** fade 0.4 [estimated] s (S2-doc C32) + load + fade-in 0.4 [estimated] s; target ≤ 3 [estimated] s in total.
8. **Marker:** unaffected; travelling away from it is allowed.

### 9.2 Node state list
`FT-UNFOUND → FT-FOUND-DORMANT (found during WP-CORRIDOR) → FT-ACTIVE (WP-OPEN, or found after it) → FT-IN-USE (destination menu; fade) → FT-ACTIVE`

### 9.3 Placement metrics
| Metric | Target | Tag | Reference |
|---|---|---|---|
| Nodes | 1 per region + 0–2 in hub regions; 8–14 in total | [estimated] | HK 11 [sourced] stations (S66) over about 18 [estimated] areas (S5-doc S70 count): about 61% [estimated] (arithmetic) of areas |
| From any rest point to the nearest node | ≤ 12 rooms and ≤ 4 minutes | [estimated] | none |
| From a region's node to its main hub entrance | ≤ 4 rooms | [estimated] | none |
| From a boss's respawn checkpoint to the nearest node | ≤ 6 rooms | [estimated] | none |
| From a node to the nearest old gate after a new ability | ≤ 3 minutes and ≤ 8 rooms | [estimated] | S5-doc §12.4 |
| Node with a shop door | Every node rest point is also its region's shop door when possible | [estimated] | TM shop doors (S81) |

---

## 10. Deferred (post-v1, optional): trinkets and notches

**Decided (user, 14 Nov 2026):** trinkets are optional and may be added later (decision x). **Nothing in this section is part of v1:** no trinket or notch is in the v1 economy (§4), catalogue (§5.3), save-point features (§6.2) or completion formula (§11). The design below is kept for a later version.

HK's equippables are the model: passive items costing notches, swapped only at rest points (S84).

| Value | Proposed setting (deferred) | Tag | Reference |
|---|---|---|---|
| Notches | 3 at start → 6 max (3 shop) | [estimated] | HK 3 → 11 [sourced] (S84) |
| Notch prices | 200 / 500 / 900 coin | [estimated] | HK 120, 500, 900, 1,400 [sourced] (S84) |
| Trinkets | 12–16, all at fixed locations or in the shop | [estimated] | HK 45 [sourced] (S84) |
| Notch cost per trinket | 1–3 | [estimated] | none |
| Overfilling | Not allowed | [estimated] | HK allows it at double damage taken (S84); it conflicts with fixed damage classes (S2-doc §5) |
| Swap | Rest points only | [estimated] | HK (S84) |

**Content rules if added:**
1. **No stat scrolls by another name.** No raw damage % or max pips (E5 stays excluded, decision z).
2. **No cooldown reduction** (E6 stays excluded, decision w).
3. **Allowed effects:** traversal comfort within S1-doc limits, a coin magnet, information (a secret counter per region, as TM sold, S4), and combat utility S3-doc already defines (status duration, poise damage).
4. **Never a gate key.**
5. **When added:** raise coin sources by k = (10,000 + notch prices) ÷ 10,000 [estimated] (§4.3 rule 7), add a "Swap trinkets: rest points only" save-point feature, and add a trinket category to §11 with weight 0.10 [estimated], scaling the others down.
6. **Breakable trinkets** (HK, PE10) stay out even then, unless death needs more stakes.

---

## 11. Completion tracking

### 11.1 Formula
Completion % = 100 × Σ_c w_c × (found_c ÷ total_c), with Σ w_c = 1. The v1 weights below sum to 1.00 [estimated].

| Category c | Weight w_c (v1, rebalanced after decision x) | Was (v1 draft, with trinkets) | Counted | Tag |
|---|---|---|---|---|
| Abilities and techniques | 0.22 | 0.20 | Each ability and technique in §3.2 | [estimated] |
| Bosses (R0) | 0.17 | 0.15 | Each boss defeated | [estimated] (HK counts 1 [sourced] point per boss, S85) |
| Max-pip growth | 0.12 | 0.10 | Pips gained (fragments count only as whole sets) | [estimated] |
| Weapon tiers | 0.11 | 0.10 | T2–T5 bought | [estimated] |
| Map: rooms inked | 0.17 | 0.15 | Rooms in MR-INKED or MR-INKED+SECRET (S5-doc §11.1) | [estimated] (none of the references counts rooms) |
| Secrets and caches | 0.11 | 0.10 | Secret rooms entered and caches opened | [estimated] |
| Challenge rooms | 0.05 | 0.05 | Challenge rooms cleared | [estimated] (TM 45 [sourced] collectibles, S82) |
| Shop catalogue | 0.05 | 0.05 | Items owned ÷ items in the catalogue, **excluding weapon tiers** (counted above) | [estimated] |

**Rules:**
1. **Capped at 100%.** HK goes to 112 [sourced] with extra content (S85); Bastion Zero tracks extra content separately.
2. **Shown as an integer, rounded down,** so 100 appears only when everything is done.
3. **Where it's shown:** on the save slot and the pause screen from the start (HK hides it behind an ability, S85). Per-region completion stays on the map as in S5-doc §11.3.
4. **No missables.** Every counted item stays reachable after the final boss; there is no point of no return [estimated].
5. **Death never lowers completion.** Coin is not counted.

### 11.2 Region completion (from S5-doc §11.3)
Region % = 100 × (0.7 × rooms inked ÷ rooms + 0.3 × secrets found ÷ secrets) [estimated], shown once the region map is found.

---

## 12. Conflicts with System 3 exclusions (E1–E14), System 4 (AE1–AE12) and System 5 (WE1–WE11)

Exclusions are reversed only where the user decided so on 14 Nov 2026; those rows say **Reinstated by user decision** and name the non-random form. Every other exclusion stands.

### 12.1 System 3 exclusions
| # | Excluded item | What a persistent model would want | Conflict | Proposal | Decision |
|---|---|---|---|---|---|
| E1 | Random weapon, skill and amulet drops | Rewards to place on the map | Only the randomness | **Keep excluded.** Fixed-location items already cover it (S3-doc §2). | None needed |
| E2 | Gear level by area and difficulty | Weapon growth | Growth is wanted; scaling by area is not | **Stays excluded.** Fixed tiers T1–T5 (S3-doc §9), bought for coin only. | **Decided (user, 14 Nov 2026):** (v) |
| E3 | Gear quality and gear-power boosts | Weapon growth | Same as E2 | **Keep excluded.** | None needed |
| E4 | Affixes and rerolls | Variety in items | Loot roll | **Keep excluded.** Deferred trinkets (§10) could add variety later without rolls. | None needed |
| E5 | Stat scrolls and stat points | Stat growth | Growth by random choice | **Stays excluded.** Growth comes only from pips, tiers and flask charges, all fixed. | **Decided (user, 14 Nov 2026):** (z) |
| E6 | Per-run mutations, including kill-based cooldown reduction | Cooldown reduction as a reward | S3-doc §4 rule 5: no cooldown reduction in v1 | **Stays excluded.** *Considered:* a fixed shop item at −15% [estimated], or a trinket. Not used. S3-doc §4 rule 5 stands. | **Decided (user, 14 Nov 2026):** (w) |
| E7 | Difficulty-tier scaling of enemies | n/a | None | **Keep excluded.** §4.2 scales only the coin reward. | None needed |
| E8 | Flask charges and refills, food, **shops** | Healing between rest points; places to spend coin | Flasks and shops are both on this list | **Reinstated by user decision, in part.** Shops: one fixed-stock, fixed-price shop (§5). Flask: fixed charges 1→3 [estimated], refilled only at rest points, no random drops (§8). Food and per-area random refills stay excluded. Logged as X23. | **Decided (user, 14 Nov 2026):** (t), (u) |
| E9 | Permadeath, the meta-currency economy, gold retention | A currency economy | A currency is wanted; permadeath and run retention are not | **Reinstated by user decision, in part:** one persistent coin with set, fixed values (§4) and the P-C marker penalty (§7). Permadeath (PE1), the run-bound meta currency (PE2) and gold retention (PE3) stay excluded. | **Decided (user, 14 Nov 2026):** (r), (s) |
| E10 | Cursed chests and the curse status | n/a | None | **Keep excluded.** | None needed |
| E11 | Colourless and legendary variants | n/a | None | **Keep excluded.** | None needed |
| E12 | Backpack | n/a | None | **Keep excluded.** | None needed |
| E13 | Two-handed slot sharing | n/a | None | Unchanged (optional, S3-doc §2) | None needed |
| E14 | One-hit protection and recovery health | Healing | Recovery health doesn't fit pips | **Reinstated by user decision, in part:** health recovery comes back **only** as the fixed-charge flask (§8). DC's rally-style recovery health stays excluded (S2-doc §5 rule 5); one-hit protection stays assist-only. Logged as X23. | **Decided (user, 14 Nov 2026):** (u) |

### 12.2 System 4 exclusions
| # | Excluded item | Conflict | Proposal | Decision |
|---|---|---|---|---|
| AE5 | Enemy drops: currencies, unlock tokens, item drop chances (economy out of scope) | Coin must come from somewhere | **Reinstated by user decision, in part:** fixed coin per kill by archetype band (§4.2), no drop chances. Unlock tokens and item drop chances stay excluded. | **Decided (user, 14 Nov 2026):** (r) |
| AE6 | Enemy stats scaled by area tier | §4.2 scales coin by tier | Not a conflict: coin is a reward, enemy stats stay fixed | None needed |
| AE9 | Random reward for a no-hit boss kill | A skill reward could be wanted | **Keep excluded.** No fixed version proposed. | None needed |
| AE11 | "Doesn't respawn on future runs" | None | Already kept as R0 | None needed |

### 12.3 System 5 exclusions
| # | Excluded item | Conflict | Proposal | Decision |
|---|---|---|---|---|
| WE3 | Random shops and treasure | Shops are wanted | **Reinstated by user decision, in part:** one fixed shop (§5); randomness stays excluded | **Decided (user, 14 Nov 2026):** (t) |
| WE8 | Between-biome passages that sell, reroll and upgrade | Shop location | **Keep excluded.** Shop doors sit at in-world rest points instead. | None needed |
| WE10 | Currency costs of maps, pins, tolls and fast travel | Now that coin exists, these are possible sinks | **Stays excluded:** maps are found, markers free, fast travel free. *Considered:* HK-style tolls (HK paid 1,560 [sourced] for all stations and 700 [sourced] for other tolls, S83). | **Decided (user, 14 Nov 2026):** (y) |

---

## 13. Interactions and known cross-system differences

### 13.1 Interactions
| Topic | Rule | Governs |
|---|---|---|
| Death states | Penalty fixed at D-1, applied at D-3 | S2-doc §9; this spec §7 |
| Respawn | Later of checkpoint and rest point, full health | S2-doc C33, S5-doc X17 |
| R0 rewards | Elites and bosses pay coin once; ability rooms sit behind R0 fights | S4-doc §11 |
| R1/R2 farming | Capped at 50% [estimated] of exploration income | §4.3 rule 6 |
| Elite clone (M6) | Coin split between copies | S4-doc §9 |
| Boss retries | Marker beside the respawn checkpoint before B-GATE | S4-doc §10; §7.3 rule 2 |
| Map | Marker, shop doors and nodes auto-pinned; completion counts inked rooms | S5-doc §11 |
| Corridor act | No penalty, no fast travel, shop stock ST1 only | S5-doc §4.3 |
| Gates | Gate-critical abilities found, techniques bought; GT7 affordability rule | S5-doc §5 |
| Skills | Cooldown refresh at rest; no cooldown reduction (decision w) | S3-doc §4 |
| Healing | Flask refills at rest, death and reload; S4-doc budgets may rise ≤ 3 [estimated] pips per charge | S4-doc §13; §8 |
| Weapon tiers | Bought in order for coin, stocked by tier | S3-doc §9; §5.3 |
| Damage reduction | Shop item; S2-doc formula unchanged | S2-doc §5 |

### 13.2 Known cross-system differences (continued; no edits to Systems 1–5)
| # | Topic | Earlier system says | System 6 uses | Governs in System 6 |
|---|---|---|---|---|
| X19 | Death penalty | S1-doc row 53: penalty duration 60 [sourced] s (TM follower), "economy system owns the final value" | Marker model P-C (§7). **Decided (user, 14 Nov 2026):** decision (s). Row 53's follower is not used. No S1 edit. | System 6 (the owner S1 names) |
| X20 | Weapon tier acquisition | S3-doc §9: tiers "found at fixed locations" | Tiers T2–T5 are **bought at the shop for a coin fee only**: no materials and no fixed-location tier pickups (§5.3). **Decided (user, 14 Nov 2026):** decision (v). The multipliers 1.0–2.6 [estimated] are unchanged. No S3 edit. | System 6 |
| X21 | Marker placement and timing | S2-doc D-3: penalty applied at D-3; marker at the last safe-ground point | Penalty fixed at D-1 (quit-safe); arena deaths place the marker before B-GATE; never across a one-way link (§7.3) | System 6 (refines S2) |
| X22 | Max health growth path | S2-doc C17–C18: start 5, max 10, no path defined | 3 [estimated] fragment sets + 2 [estimated] shop pips (§4.1) | System 6 (fills a gap) |
| X23 | Healing flask and health recovery | S3-doc E8 excludes flask charges and refills; E14 excludes recovery health; S2-doc §5 rule 5 leaves healing to a later system | Fixed-charge flask (1→3 [estimated] charges, 3 [estimated] pips each, refilled at rest points), **reinstated by user decision (u)** in non-random form; rally-style recovery health and food stay excluded | System 6 (no S3 edit) |

---

## Open Questions
1. **No public income data.** No source gives currency earned per hour in any of the three games. Every §4 rate is [estimated] and needs a playtest with a coin log.
2. **Total currency available** in TM and HK is not in the pages opened, so "catalogue ÷ total available" can't be compared.
3. **Death frequency** of a median player is unknown, so the ≤ 5% [estimated] marker-loss target is a guess.
4. **Coin after the catalogue:** add a cosmetic sink (TM precedent, S79), or nothing?
5. **Portable warp point** (HK, S66): one player-placed return point, or none in v1?
6. **A permadeath or hard mode** (PE11): out of scope for v1.
7. **Flask tuning:** 3 [estimated] pips per charge with 3 [estimated] charges can restore 9 [estimated] pips between rest points, close to the full 10 [estimated]. Playtest against S4-doc's pip-loss budgets; 2 [estimated] pips per charge is the fallback.
8. **Tier pacing by coin:** with no materials, a player who farms can buy tiers as soon as each stock tier opens. Stock tiers cap this; playtest whether T4–T5 arrive too early for careful savers.
9. **Opening event** (S5-doc Open Question 6) also switches on fast travel and stock tier ST2 here; its trigger is still open.
10. **Air-dash timing:** row 6A sits at 5.5–7.0 [estimated] h, after the glide down-strike. Playtest whether a late air dash leaves enough GT17 revisits (S5-doc §5.3 rule 8) in the last 1.5–3 [estimated] h.

### Resolved (user, 14 Nov 2026)
- (r) One persistent coin with set, fixed values.
- (s) P-C: half of unspent coin into a recoverable marker.
- (t) One shop reached through doors at rest points.
- (u) Fixed-charge flask; E8 and E14 partly reinstated (X23).
- (v) Coin fee only for weapon tiers; X20 updated.
- (w) Cooldown reduction stays excluded.
- (x) Trinkets deferred (post-v1, optional), §10.
- (y) Map, toll and fast-travel prices stay excluded.
- (z) Stat growth stays excluded.
- The TM follower's drain rate (S80, provenance unclear) is no longer needed, since P-A was not chosen.
- System 3 (a)–(e): no roll, only dash and block; no shield item; no air-roll technique; crits and statuses approved.
- S3 DB-1–DB-4: dash i-frames and parry need no unlocks; the air dash is unlock row 6A; GT10 removed.
- AD-SRC: the air dash comes from an ability room behind an R0 elite. Open Question 10 (timing) stays open.
- CR34: copied defaults tagged [estimated] with the reference as basis.

No open decisions for the user remain in this system.

## Dependencies on other systems
- **System 1 (movement):** unlock flags for wall-cling, glide, grapple, projectile bounce, glide down-strike, recovery jump and the dash flag (now the air dash, row 6A); flask use per movement state (§8 rule 4); row 53 (X19, not used).
- **System 2 (collision/damage):** pips and damage reduction (§5), healing never above max pips (§8), death states D-1 to D-5 and the marker (X21), full-health respawn, max-health range (X22), §5 rule 5 (X23).
- **System 3 (combat):** weapon tier multipliers bought for coin (X20), slot B items, ranged charges, charged strike, cooldowns (no reduction), exclusions E1–E14 with E8 and E14 partly reinstated (X23), the ground dash, block and parry as base kit and the air dash as unlock 6A (decisions a–e, DB-1–DB-4; X24).
- **System 4 (enemies):** R0–R3 for coin and farming, archetype bands for coin values, M6 coin split, boss retry and arena marker placement, expected pip-loss budgets now that the flask exists (§8 rule 3).
- **System 5 (world):** gate table and unlock order, corridor act, rest points and checkpoints, fast-travel nodes, map pins and completion, exclusions WE3, WE8, WE10.
- **Save system:** persistence classes P0–P3, write triggers, marker state, wallet, flask capacity and charges, weapon tier, shop stock tiers, 3 [estimated] slots.
- **UI:** wallet counter, shop screens with silhouettes, marker icon, flask charges, fast-travel destination list, completion on save slot and pause screen, flask charges.
- **Level design:** cache and challenge-room placement (§4.2), fragment and found-flask-charge placement, shop-door and node placement (§9.3), the GT7 affordability rule.
- **Audio / VFX:** coin pickup, marker presence sound, purchase, save icon.
- **Narrative:** the opening event; who runs the shop (no reference names used).
- **Accessibility:** an assist option to turn the death penalty off (the considered P-D, as an assist only), alongside S2-doc's lethal-hit guard.
- **Deferred (post-v1):** trinkets and notches (§10) would touch UI (a rest-point trinket menu), save data, the catalogue and the completion formula.
