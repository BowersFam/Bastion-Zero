# Bastion Zero: System 2, Collision and Damage Model (clean-room spec)

Version 2.1 · 14 Nov 2026 (UTC+8). User decisions applied; v1 backed up as `.v1`. Version 2.1 applies the CR34 tag policy (Bastion Zero defaults copied from a reference are [estimated], with the reference as basis); Version 2 is backed up as `.pre-final`.

References: The Messenger (TM), Hollow Knight (HK), Dead Cells (DC). This system builds on System 1 (`system-1-player-movement.md`, "S1-doc"). Its hazard section is S1-doc §6, and this spec extends it rather than restating it.

## 0. Conventions

### Tags
- **[sourced]**: stated in a public source.
- **[community-measured]**: frame-observed by a third party.
- **[estimated]**: my recommended Bastion Zero value; the reasoning is given.
- **"provenance unclear"**: a wiki number that may come from inspecting game data.
- **Copied defaults (CR34 policy, decided 14 Nov 2026):** a Bastion Zero value copied from a reference is tagged [estimated], with the reference given as its basis, e.g. "5 [estimated] (basis: S4)". The reference value itself stays [sourced].

### Units
These carry over from S1-doc:
- 1 tile = 16 px [estimated];
- hero drawn height H = 2 tiles [estimated];
- 60 fps [estimated].

### Markers
- **⚑S1**: this rule changes how a System 1 state behaves or transitions.
- **↳S1 row n**: this rule depends on S1-doc §9 tuning row n.

System 1 is **not** edited by this document. The user declined the System 1 edits that an earlier version proposed (decision 7, 14 Nov 2026). Where the two systems differ, §12 lists each "known cross-system difference" and says which value governs System 2's own rules.

### User decisions applied (14 Nov 2026)
| # | Decision | Where applied |
|---|---|---|
| 1 | Bottomless pits in exploration areas: damage plus a warp to the last safe ground, not instant death | §5 classes, §8.4, §9, C41–C42, §12 |
| 2 | Adopt the per-hazard re-hit lockout at 0.5 [estimated] s | §8.3, C25 |
| 3 | Respawn health: full | §9 D-4, C33 |
| 4 | Max health: 10 [estimated] pips | §5 rule 1, C18, §12 |
| 5 | Collapsing platforms come back | §4.7, C16 |
| 6 | Contact damage on by default | §6.1 |
| 7 | Leave System 1 as is: all six proposed System 1 edits declined | §12 |
| 8 | CR34 tag policy (consolidation question, decided later the same day): Bastion Zero defaults copied from a reference become [estimated], basis cited | §0 Tags, §5, §8.3, C17, C19, C21, C22, C36, C38, X29 |

These are design decisions. Every value they set is tagged [estimated], never [sourced].

### Clean-room exclusions (not used anywhere below)
- teemo.dev, HK "Game Feel & Juice": the hit-evasion window, the recoil speed and duration, and the hit-pause presets. All of them were read from the compiled assembly.
- teemo.dev, DC "Enemies and Bosses Reference": per-enemy base damage tables and the tier scaling constants. They state these are the raw values from the game's data files.
- DC Wiki "Biomes": the scaling formula's constants. The wiki says they are "located in the game files". Only the existence of tier-based scaling is used here.
- DC Wiki "Stats": the per-stat health multiplier tables are flagged provenance unclear. Only the base value and the diminishing-returns shape are used.

### Source key
Codes S1–S18 match System 1. Only the codes used here are listed.

| Code | Source | URL |
|---|---|---|
| S3 | Kotaku dev interview (TM) | https://kotaku.com/tight-controls-define-this-retro-homage-to-ninja-gaiden-1822282879 |
| S4 | The Messenger Wiki: Shop Upgrades page, wall-cling item page, grapple item page | https://the-messenger.fandom.com/wiki/Shop_Upgrades · /wiki/Climbing_Claws · /wiki/Rope_Dart |
| S5 | TheMessengerRTA Wiki: Out-of-Bounds Techniques | https://themessengerrta.fandom.com/wiki/Out-of-Bounds_Techniques |
| S6 | Steam Community 100% guide (TM) | https://steamcommunity.com/sharedfiles/filedetails/?id=1643339519 |
| S7 | GameFAQs TM guide by KumihoStar | https://gamefaqs.gamespot.com/pc/230087-the-messenger/faqs/77725/general-information |
| S11 | Hollow Knight Wiki: wall-cling page, damage-recovery charm page, weapon page | https://hollowknight.wiki/w/Mantis_Claw · /w/Stalwart_Shell · /w/Nail |
| S12 | Gaming StackExchange, HK i-frames | https://gaming.stackexchange.com/questions/413676 |
| S13 | HK Speedrunning Wiki, "Hazard warp" | https://hollow-knight-speedrunning.fandom.com/wiki/Hazard_warp |
| S14 | Official DC Wiki: Mechanics | https://deadcells.wiki.gg/wiki/Mechanics |
| S15 | Official DC Wiki: Controls | https://deadcells.wiki.gg/wiki/Controls |
| S17 | r/deadcells, ledge-grab i-frames | https://www.reddit.com/r/deadcells/comments/somwau/ |
| S19 | Official DC Wiki: Hazards (incl. version history) | https://deadcells.wiki.gg/wiki/Hazards |
| S20 | Official DC Wiki: Shields | https://deadcells.wiki.gg/wiki/Shields |
| S21 | Official DC Wiki: Stats | https://deadcells.wiki.gg/wiki/Stats |
| S22 | Official DC Wiki: Permadeath | https://deadcells.wiki.gg/wiki/Permadeath |
| S23 | Official DC Wiki: Biomes (scaling section) | https://deadcells.wiki.gg/wiki/Biomes |
| S24 | Hollow Knight Wiki: protagonist page (health section) | https://hollowknight.wiki/w/The_Knight |
| S25 | Hollow Knight Wiki: death-remnant enemy page | https://hollowknight.wiki/w/Shade |
| S26 | Hollow Knight Wiki: a late boss page (2-unit contact damage) | https://hollowknight.wiki/w/Failed_Champion |
| S27 | Ludonauta devlog, "The Hidden Genius Behind Hollow Knight's Terrain Design" | https://ludonauta.itch.io/platformer-essentials/devlog/1084669/the-hidden-genius-behind-hollow-knights-terrain-design |
| S28 | ResetEra forum thread, HK terrain geometry | https://www.resetera.com/threads/how-to-enjoy-hollow-knight.124973/page-3 |
| S29 | Atomic Bob-Omb blog, "Hollow Knight & Knockback" | https://atomicbobomb.home.blog/2019/02/05/hollow-knight-knockback/ |
| S30 | Stack Overflow, frame-by-frame observations of HK melee | https://stackoverflow.com/questions/58929965/cloning-melee-combat-from-hollow-knight |
| S31 | Game Developer, "How does 'The Messenger' make you a master?" | https://www.gamedeveloper.com/design/how-does-the-messenger-make-you-a-master- |
| S32 | Neoseeker TM walkthroughs (two levels with crushers, moving and collapsing platforms) | https://www.neoseeker.com/the-messenger-2018/walkthrough/Catacombs · /walkthrough/Underworld |
| S33 | Speedrun.com TM guide (includes "spike seeding") | https://www.speedrun.com/the_messenger/guides/1sjxb |
| S34 | Steam DC discussion, "Collision detection (Enemy bumps)" | https://steamcommunity.com/app/588650/discussions/0/1457328392105850726/ |
| S35 | r/deadcells, "needs temporary invulnerability after being hit" | https://www.reddit.com/r/deadcells/comments/6n5trx/ |
| S36 | kosmender.com, DC final-boss guide (body contact on charge and slam) | https://www.kosmender.com/hand-of-the-king |

---

## 1. Hitbox and hurtbox conventions

Sources: S6, S11, S14, S30, S34, S36.

### Comparison
| Topic | TM | HK | DC |
|---|---|---|---|
| Player attack hitbox | Hits everything in range in one swing. Active for "a few frames" and moves with the hero, so turning, running or jumping mid-swing sweeps a larger area (S6). | Swings in four directions; reach can be extended by equipment, by 15% [sourced] and 25% [sourced] (S11) | Weapon-dependent; outside this system |
| Crouch hurtbox | Crouching lowers attacks and helps avoid projectiles (S6) | No crouch | Can duck under floor-anchored flails and some arrows (S19) |
| Roll / dodge hurtbox | n/a | n/a | Hurtbox height shrinks to 1 tile [sourced] during a roll (S14) |
| Hurtbox vs sprite | Not documented | Not documented in an opened source. Observers report a hit/recoil separation on every connect (S30). | Hazard and boss bodies "bigger than they look" per a community guide (S36) |
| Body-to-body collision | Not documented | Not documented | Enemies physically push the hero, including mid-roll and off ledges; a frequent player complaint (S34) |
| Multi-hit protection | Not documented | Excluded (only source is compiled code) | Not documented |

### Bastion Zero rules
1. **Three box types per actor:**
   - *collision box*: terrain only;
   - *hurtbox*: receives damage;
   - *hitbox*: deals damage.
   They are authored separately. Hurtboxes are never larger than the collision box.
2. **Player hurtbox inset:** the hurtbox is narrower and shorter than the drawn sprite. This forgives near-misses that look like misses. Values are in §11.
3. **Hazard hitbox inset:** spike and saw hitboxes are inset from their art on every exposed side.
4. **Enemy body:** enemies have a hurtbox and, if they deal contact damage (§6), a contact hitbox slightly smaller than their hurtbox. Enemies do **not** physically push the hero: actor-to-actor collision is off. This avoids DC's ledge-shove problem (S34). Knockback (§7) is the only way an enemy displaces the hero.
5. **Attack hitbox moves with the hero** for its whole active window (TM, S6). Per-target lockout is one hit per swing. ↳S1 row 25, row 28.
6. **Crouch:** hurtbox height becomes 1 tile (§11). ⚑S1 G-CROUCH gains a smaller hurtbox.
7. **Hitbox priority on the same frame:**
   1. instant-death volume;
   2. hazard;
   3. enemy hitbox;
   4. the player's own hitbox resolving a bounce grant.

   The first rule that fires wins.
   - If (2) or (3) damages the hero on the same frame the hero's attack would grant an air-jump, the damage applies and the grant is dropped. ⚑S1 §3: this edge case is new.
   - This ordering is the one used everywhere else in this spec.

---

## 2. One-way platforms

Sources: S7, S15, S27, S31.

### Comparison
| Topic | TM | HK | DC |
|---|---|---|---|
| Present | Yes (S7, S31) | No; deliberately absent, all terrain solid (S27) | Yes (S15) |
| Drop-through input | Crouch + jump (S7) | n/a | Down + jump (S15) |
| Used as a gate | Yes: an early room forces a drop-through and can't be re-climbed (S31) | Absence is the gate (S27) | Not documented |

### Bastion Zero rules
1. **Land condition:** the hero lands on a one-way top only if both are true:
   - vertical speed ≤ 0 [estimated] (falling or level);
   - on the previous frame, the bottom of the hero's collision box was at or above the platform top, within a small tolerance.
2. **Pass-up:** while rising, the hero passes through from below and from the sides.
3. **Drop-through:** crouch + jump while on a one-way. The platform is ignored for a short window. ⚑S1: the G-CROUCH → A-FALL transition (S1-doc §8) is unchanged, but it also starts the ignore timer.
4. **Coyote after leaving a one-way edge by walking:** normal coyote applies. ↳S1 row 19.
5. **Coyote after a drop-through: none.** Otherwise the same jump press, buffered, could fire a coyote jump straight back up. ⚑S1: drop-through clears the coyote timer and the jump buffer.
6. **Wall-cling:** one-way platforms are never cling surfaces, including their side edges. ⚑S1 W-CLING.
7. **Grapple:** grapple rays pass through one-way platforms. ⚑S1 X-GRAPPLE.
8. **Fast-fall does not drop through.** Only the explicit input does. ⚑S1 A-FASTFALL.
9. **Enemies** use the same land rule. Enemies never drop through unless their AI requests it.

---

## 3. Slopes

Sources: S28; gaps elsewhere.

### Comparison
| Topic | TM | HK | DC |
|---|---|---|---|
| Walkable slopes | Not documented in any source opened | None; geometry is right-angled rectangles per a forum observation (S28) | Not documented |
| Effect on speed | n/a | n/a | n/a |

**Gap:** no source confirms whether TM or DC have walkable slopes. Footage review is needed (Open Question 1). Range: none in any reference, up to occasional 45° [estimated] ramps.

### Bastion Zero rules (v1)
1. **No slopes in v1.** Collision is axis-aligned tiles only. This matches the HK observation and keeps S1-doc's arcs exact for level design.
2. **If slopes are added later**, these rules apply:
   - Only 45° [estimated] and 22.5° [estimated] pieces.
   - Run speed is unchanged along the slope; speed is measured horizontally.
   - The hero snaps to the slope while descending, up to the snap distance in §11.
   - Coyote and landing behave as on flat ground.
   - Wall-cling never on a slope (HK vertical-only rule, S11).
   - Spikes on slopes are banned.

---

## 4. Moving platforms, crushers and unstable floors

Sources: S6, S11, S19, S31, S32.

### Comparison
| Topic | TM | HK | DC |
|---|---|---|---|
| Rideable moving platforms | Yes: vertical and horizontal riders over spikes (S32) | Present, but no rules documented. Wall-cling works on moving vertical surfaces such as conveyor walls (S11). | Elevators and lifts (S19) |
| Crushing | Crushed by moving walls, platforms or doors = instant death regardless of HP (S6). Crushers are introduced as a new element mid-game (S31). | Not documented | Elevator landing on the hero, or pinning them against a ceiling = trap damage, not death (S19) |
| Drop-away floors | Collapsing platforms (S32) | Not documented | Crumbling single-tile platforms that never regenerate; trapdoor platforms that drop for "a few seconds" after a slight delay (S19) |
| Platforms vs other hazards | Moving platforms can block lasers (S6) | n/a | n/a |

**Disagreement:** crushing is instant death in TM but capped trap damage in DC. Range: from damage-only (capped at 30% of max HP [sourced], S19) to instant death.

### Bastion Zero rules
1. **Carry:** a grounded hero on a moving platform moves with it each frame before their own input is applied.
2. **Jump-off momentum:** on leaving a platform, by jumping or walking off, the hero inherits the platform's horizontal velocity and half of its upward velocity. Downward platform velocity is ignored. ⚑S1 A-RISE-HELD: the start velocity gains a platform term. ↳S1 rows 7, 17.
3. **Coyote on moving platforms:** the coyote window is measured from when ground contact was lost, as usual. A coyote jump still inherits the platform velocity from the last contact frame. ↳S1 row 19.
4. **Wall-cling on moving walls:** the clinging hero moves with the wall (HK precedent, S11). If the wall moves away from the hero faster than the hero can follow, the cling breaks. ⚑S1 W-CLING.
5. **Crush:** the hero is crushed when solid geometry overlaps their collision box from opposite sides by more than the crush tolerance (§11). Crush → D-DEAD (TM rule). ⚑S1: S1-doc §6 rule 3 already names crush as instant death; this spec defines the detection.
6. **Crush tolerance push-out:** an overlap below the tolerance is resolved by pushing the hero out along the smallest axis. This kills false crushes from one-pixel contacts.
7. **Collapsing platforms:** after a brief shake, the platform becomes non-solid, then returns after a respawn time. Unlike DC, they always return, because Bastion Zero is a backtracking world. Values are in §11. **Decided (user, 14 Nov 2026):** platforms come back.
8. **Grapple vs moving targets:** the grapple pull targets the target's current position each frame. ⚑S1 X-GRAPPLE.
9. **Platforms block beams:** laser-type hazards are stopped by solid moving geometry (TM, S6).

---

## 5. Health representation and damage scaling

Sources: S4, S6, S7, S14, S21, S23, S24, S26.

### Comparison
| Topic | TM | HK | DC |
|---|---|---|---|
| Unit | Integer HP shown as discrete pips | Integer "units" shown as icons | Continuous numeric HP bar |
| Start / max | 5 [sourced] / 8 [sourced] (S4) | 5 [sourced] / 9 [sourced]; 4 [sourced] fragments make one unit (S24) | 100 base [sourced] (S21); max grows with stat upgrades, with diminishing returns (S21, provenance of the multiplier tables unclear) |
| Typical hit | 1 [sourced] from enemies and projectiles; 3 [sourced] from spikes (S6) | 1 [sourced] usually; some late attacks deal 2 [sourced] (S26) | Scales with biome tier (S23; constants excluded) |
| Temporary health | n/a | Temporary extra units that can't be healed and vanish on rest (S24) | "Recovery": 80% [sourced] of lost health becomes regainable by hitting enemies. It starts draining after 0.08 s [sourced] at 30% [sourced] of max HP per second, and each hit heals 12% [sourced] of damage dealt, capped at 20% [sourced] of the original loss per hit (S14) |
| Damage reduction | Hits of 2 or more reduced by 1 [sourced] (S4) | n/a in base | Shield block absorbs 75% [sourced] typical (S20) |
| Lethal-hit guard | None documented | None documented | A hit that would kill while HP is above 25% [sourced] of max leaves 1 [sourced] HP, stuns nearby enemies about 1 [sourced] s, and recharges in 45 [sourced] s (S14) |
| Difficulty scaling | None documented (fixed integers) | None (fixed integers) | Enemy damage and HP scale by biome tier and difficulty (S23) |
| Low-health signal | Not documented | Screen-edge vignette at 1 [sourced] unit left (S24) | Not documented |

### Bastion Zero rules
1. **Health is integer pips** (TM/HK). Start 5 [estimated] (basis: S4, S24) (C17), max 10 [estimated] (C18). **Decided (user, 14 Nov 2026):** max health is 10 pips (design decision). Known cross-system difference: S1-doc row 45 still says max 8; System 2's rules use 10 (§12).
2. **Damage classes**, so designers never pick raw numbers:

| Class | Pips | Use |
|---|---|---|
| D-Light | 1 [estimated] (basis: TM, S6) | Enemy contact, small projectiles |
| D-Heavy | 2 [estimated] (HK precedent, S26) | Telegraphed heavy attacks, boss contact |
| D-Hazard | 3 [estimated] (basis: TM, S6) | Spikes, saws, lasers (↳S1 row 46) |
| D-Pit | 2 [estimated] | Bottomless pits and the kill plane: damage, then warp to the last safe ground (decision 1; the amount is my choice) |
| D-Lethal | all | Lava, crush |

3. **Damage formula (plain math):**
   - `dealt = max(1, classPips − reduction)`, but reduction applies only when classPips ≥ 2 (TM rule, S4). The 1 and 2 here are [estimated] (basis: TM rule, S4).
   - Lethal ignores reduction.
4. **No tier or difficulty scaling** of enemy damage in v1. Fixed classes keep pips readable. An optional assist mode can lower every class by 1 [estimated] pip, to a minimum of 1 [estimated].
5. **No continuous "recovery" or temporary health in v1.** Pips and DC's rally mechanic don't mix well. Healing belongs to a later system.
6. **Lethal-hit guard: off by default** (TM/HK have none). It is an assist-mode option modelled on DC: a hit that would kill from 3 [estimated] or more pips leaves 1 [estimated], then recharges (C35–C36).
7. **Low-health signal** at 1 [estimated] pip (basis: HK precedent, S24).

---

## 6. Contact damage

Sources: S6, S7, S26, S34, S36.

### Comparison
| Topic | TM | HK | DC |
|---|---|---|---|
| Touching an enemy body hurts | Yes: 1 [sourced] for all enemies (S6, S7) | Yes: 1 [sourced] typically, 2 [sourced] for some late attackers (S26) | Generally no damage from mere body overlap. Damage comes from attacks, including charges and slams that hit on contact (S36). Bodies still shove the hero (S34). The general rule rests on community statements, not a wiki line. |
| Projectile contact | 1 [sourced] typical (S6) | Varies | Varies |

**Disagreement:** TM and HK use ambient contact damage, while DC mostly uses attack-only damage.

### Bastion Zero rules
1. **Contact damage on by default**, D-Light (TM/HK). **Decided (user, 14 Nov 2026):** contact damage on by default. A per-enemy flag can turn it off. Use that for heavy brawlers whose danger should come from telegraphed attacks (DC-style).
2. **Contact only through the contact hitbox** (§1 rule 4), never the full body.
3. **Bounce-attack vs contact on the same frame:** priority rule §1.7 applies, so damage wins. Exception: if the hero's attack hitbox kills the enemy that frame, the enemy's contact hitbox is removed **before** contact resolves. This makes killing blows safe and protects the TM chain. ⚑S1 §3 / A-* states.
4. **Contact while invulnerable:** ignored. No re-trigger fires when invulnerability ends while the hero is still overlapping. Instead, contact resumes only after the overlap has been absent for one frame or more, or after a grace window (§11).

---

## 7. Knockback

Sources: S3, S4, S11, S29, S30, S33.

### Comparison
| Topic | TM | HK | DC |
|---|---|---|---|
| On being hit | Knockback. It can be cancelled by attacking a nearby enemy or pushing up (S3), and an upgrade adds a recovery jump (S4). | Recoil with no input for 0.2 [sourced] s, or 0.08 [sourced] s with a charm (S11, provenance unclear) | Displacement varies by attack. Some traps push the hero "a fair distance" (S19). |
| Direction on spikes | Not always away from the spike. Left/right depends on a hidden per-session "spike seed", which speedrunners manipulate (S33). | Not documented | Not documented |
| On landing your own hit | No self-recoil (attacks don't slow movement, S3) | Mutual: hero and enemy are both pushed apart on each connect (S29, S30) | Weapon-dependent |
| Used as a technique | "Damage boosting" across spike pits (S33) | Not documented | Not documented |

**Disagreement:** HK pushes the hero back on their own hits, TM does not. S1-doc already adopts the TM rule (attacks never change horizontal velocity).

### Bastion Zero rules
1. **Direction is deterministic** (fixes TM's seeded behaviour, S33):
   - From an enemy or projectile: away from the source's center along x, plus a fixed upward component.
   - From a hazard: along the hazard surface normal. Floor spikes push up and toward the side the hero came from; wall spikes push away from the wall; ceiling spikes push down.
   - If the source center is within 0.25 [estimated] tile (C28) of the hero center on x, use the direction opposite the hero's facing.
2. **No self-recoil on landing hits** (keeps S1-doc §1 rule 3). Exception: S1's optional down-strike bounce. ⚑S1: none; this confirms S1.
3. **Knockback values** ↳S1 rows 49–50 (0.25 [estimated] s, 7 [estimated] tiles/s horizontal, 9 [estimated] tiles/s up). This spec adds the deceleration, friction and wall rules:
   - during knockback, horizontal speed is held for the first half of the duration, then decays;
   - a wall contact during knockback zeroes the horizontal component;
   - wall-cling cannot start until knockback ends. ⚑S1 H-KNOCK → W-CLING blocked.
4. **Knockback into hazards:** the clamp from S1-doc §6 rule 2 applies. If the computed knockback path would enter a hazard within the knockback distance, flip or zero its horizontal component.
5. **Cancels** are as in S1-doc §6 rule 4 (an attack connecting, or the recovery jump). ⚑S1 H-KNOCK exits are unchanged.
6. **Glide during knockback:** blocked until knockback ends or is cancelled. ⚑S1 H-KNOCK → A-GLIDE not allowed.

---

## 8. Invulnerability windows

Sources: S4, S5, S11, S12, S14, S17, S19, S20, S35.

### Comparison
| Window | TM | HK | DC |
|---|---|---|---|
| After taking damage | Not published | 1.3 [sourced] s; 1.75 [sourced] s with a charm (S11, provenance unclear) | None. Repeated hits can land back to back, a long-standing player complaint (S35, community). Carrying a shield adds a 0.5 [sourced] s force field on being hit (S20). |
| Grapple / pull | Yes, during the pass-through. Speedrunners pass death triggers with it (S4, S5). | n/a | n/a |
| Dash / roll | n/a | None on the base dash; the later upgraded dash is fully invulnerable (S12) | Part of the 0.4 [sourced] s roll (S14); exact window not published |
| Ledge climb | n/a | n/a | Brief i-frames; players use them to pass spikes (S17, S19) |
| Spell or slam | n/a | Some downward spell attacks grant long i-frames (S12) | n/a |
| Do i-frames block hazards? | Damage-boosting across spike pits suggests the post-hit window, or the knockback itself, carries the hero clear (S33). Exact rule undocumented. | No: spikes, thorns, acid always damage (S12) | Rolling dodges some traps (S14) |

**Disagreement:** HK hazards ignore i-frames, while TM's damage-boost technique suggests at least a short hazard grace. S1-doc §6 rule 2 adopts the HK rule.

### Bastion Zero rules
1. **Post-hit i-frames:** ↳S1 row 48 (1.0 [estimated] s). They block enemy hitboxes, contact and projectiles.
2. **Hazards ignore post-hit i-frames** (S1-doc §6 rule 2, unchanged).
3. **Extension: per-hazard re-hit lockout.** The same hazard instance cannot damage the hero twice within the lockout window (§11). A *different* hazard can. With D-Hazard = 3 [estimated] of 5 [estimated] starting pips (basis: S6, S4), this stops a one-frame double hit on a single spike strip from being a guaranteed death. **Decided (user, 14 Nov 2026):** adopted at 0.5 [estimated] s (C25). Known cross-system difference: S1-doc §6 rule 2 has no lockout. System 2's rule governs hazard re-hits (§12).
4. **Grapple i-frames:** ↳S1 row 38. They never cover D-Lethal volumes (S1-doc §4, unchanged), nor D-Pit volumes or the kill plane. A pit always triggers its warp. New here: they **do** cover D-Hazard, which lets the grapple pass a spike-lined ring (TM precedent, S4).
5. **Hazard warp i-frames:** after H-WARP, the hero is invulnerable to enemies **and** hazards for the warp-arrival window (§11). This prevents warp-into-damage loops. ⚑S1 H-WARP → G-IDLE gains i-frames that cover hazards. S1-doc §8 lists "i-frames on" without saying whether they cover hazards; this spec specifies that they do.
6. **Respawn i-frames** after D-DEAD: same as rule 5, with a longer window.
7. **Dash or roll:** System 1 has no dash or roll state. If one is added, its i-frames are a separate flag, default off (HK base dash) (§12 table, dash/roll row).
8. **I-frame feedback:** the hero flickers at a fixed rate for the whole window, and the hurtbox is visibly active again on the frame the flicker stops.

---

## 9. Death and respawn

Sources: S6, S7, S13, S14, S19, S22, S24, S25.

### Comparison
| Topic | TM | HK | DC |
|---|---|---|---|
| Death triggers | HP 0; crush; lava; bottomless pits; "some other hazards" (S6) | HP 0 | HP 0; curse (S14) |
| Pit / liquid hazard | Instant death (S6) | Warp to the room's hazard-respawn point, with damage (S13) | Pits: 30 [sourced] base damage, capped at 30% [sourced] of max HP, then teleport to the last platform the hero stood on (S14, S19). Lava and electric waves also teleport (S19). |
| Respawn point | Last checkpoint, with 3 [sourced] HP (S7); an upgrade makes checkpoints restore up to 5 [sourced] HP (S4) | Last rest point. One third [sourced] of the special-resource gauge is lost and all currency stays with a hostile remnant at the death site (S24, S25). | New run; nearly everything is lost (S22) |
| Death penalty | A follower takes currency for 60 [sourced] s, 30 [sourced] s with an upgrade (S4) | Remnant must be beaten to recover currency. Dying again first loses it permanently (S25). | Permanent loss except up to 2,500 [sourced] gold with meta upgrades (S22) |
| Edge cases | Grapple i-frames can carry the hero past death triggers into out-of-bounds softlocks (S5) | Dying to a hazard while the body slides into another room can skip spawning the remnant, losing the currency (S25, documented bug) | n/a |

### Bastion Zero death state list
⚑S1: System 2 expands D-DEAD into the sub-states below. S1-doc keeps a single D-DEAD; the user declined the edit that would change it. Known cross-system difference: System 2's sub-states govern death handling (§12).

| ID | Sub-state | Enter | Behaviour | Exit |
|---|---|---|---|---|
| D-1 | Lethal-hit freeze | HP reaches 0 (including from D-Pit damage), or a D-Lethal volume | All input off. Hero hurtbox and collision off. World hit-pause. | Timer |
| D-2 | Death animation | From D-1 | Body is locked in place and **does not move between rooms** (fixes HK's remnant bug, S25). Camera holds. | Timer |
| D-3 | Fade out | From D-2 | Screen fades. Penalty is applied: the death-site currency marker is created at the last safe-ground point, **not** the death point (avoids unreachable markers). | Timer |
| D-4 | Respawn load | From D-3 | Hero placed at the last checkpoint. HP set to full (C33; **Decided (user, 14 Nov 2026):** respawn with full health). Room state resets (enemies, collapsing platforms). | Load complete |
| D-5 | Respawn grace | From D-4 | Full invulnerability, including hazards. Input on. | Timer → G-IDLE |

### Respawn and warp trigger table (non-death)
| Trigger | Result | Note |
|---|---|---|
| D-Hazard spike, HP remains | Damage + knockback (S1-doc §6 rule 1) | Per-hazard re-hit lockout (§8.3) |
| Pit-spike, HP remains | Damage + H-WARP to safe ground (S1-doc §6 rule 1) | Warp arrival i-frames cover hazards (§8.5) |
| Bottomless pit (exploration areas) | D-Pit damage (C41). If HP remains, H-WARP to the last safe-ground point; if HP reaches 0, D-1. | **Decided (user, 14 Nov 2026):** damage + warp (HK/DC style). Pit volumes ignore all i-frames. Arrival i-frames C29 apply. Known cross-system difference: S1-doc §6 rule 3 says pits are instant death; System 2's rule governs (§12). |
| Lava | D-Lethal | |
| Crush | D-Lethal | Detection per §4.5 |
| Kill plane (out-of-bounds) | Same as a bottomless pit: D-Pit damage, then warp | Always placed below every room; ignores all i-frames (fixes the TM OOB softlocks, S5). Changed from D-Lethal so that a missed pit trigger never kills where a pit would warp. |

---

## 10. Environmental hazards (extends S1-doc §6)

Sources: S6, S14, S19, S32.

S1-doc §6 already covers spikes, pits, lava, crush and their reference values. These are the additional hazard types.

| Hazard | TM | HK | DC | Bastion Zero |
|---|---|---|---|---|
| Laser / beam | High damage; blocked by moving platforms; can be switched off by striking a target (S6) | n/a | Electric waves: damage + teleport (S19) | D-Hazard. Blocked by solid geometry. Re-hit lockout applies. |
| Swinging / orbiting spike ball | Present; duckable (S32) | n/a | Flails rotate at a fixed rate and are duckable when floor-anchored (S19) | D-Hazard. Constant angular speed. Crouch hurtbox clears floor-anchored ones. |
| Retracting spikes | n/a | n/a | No damage while retracted. Cycle on a timer or pop after a touch, with a warning shine (S19). | D-Hazard when extended. Requires a warning frame window before extending. |
| Falling ceiling hazards | Wobble, then drop when the hero is near; can be destroyed by attacking (S9 in S1-doc) | n/a | Falling crystals in one boss phase (S19) | D-Light. Destroying one in the air counts as a bounce target. ⚑S1 §3 qualifying-target list gains this class. |
| Toxic liquid | n/a | Acid hurts and ignores i-frames (S12) | 10 [sourced] damage over 2 [sourced] s in 5 [sourced] ticks per second, refreshed while standing in it. Touching the surface without reaching the floor deals none. (S14) | v1: none. If added, 1 [estimated] pip per contact plus a warp, not damage over time (pips don't suit ticks). |
| Darkness timer | n/a | n/a | Damage after 12 [sourced] s away from light (S14) | Out of scope |
| Crumbling / trapdoor floors | Collapsing platforms (S32) | n/a | Crumble without regenerating, or drop for a few seconds (S19) | §4 rule 7 |
| Explosive objects | n/a | n/a | Explode on contact; can be deflected by attacks (S19) | Future combat-system item |
| Trap damage cap | n/a | n/a | Trap damage capped at 30% [sourced] of max HP per hit since v1.9 [sourced] (S19 history) | D-Hazard fixed at 3 pips is the equivalent cap |

---

## 11. Bastion Zero tuning table (System 2)

Units: tiles, seconds, pips. H = 2 tiles. 1 tile = 16 px. 60 fps. All three are [estimated] and inherited from S1-doc.

| # | Parameter | Default | Unit | Tag | Reference / range | Reasoning |
|---|---|---|---|---|---|---|
| C1 | Player collision box width | 0.75 | tiles | [estimated] | none published | Fits 1-tile gaps with margin |
| C2 | Player collision box height (standing) | 1.75 | tiles | [estimated] | H = 2 tiles (S1-doc) | Slightly under the drawn height so the hero fits 2-tile corridors |
| C3 | Player collision box height (crouch) | 1.0 | tiles | [estimated] | DC roll shrinks to 1 [sourced] tile (S14) | Matches DC's 1-tile-gap convention |
| C4 | Player hurtbox width | 0.5 | tiles | [estimated] | Not measured in any opened source | Forgiving sideways |
| C5 | Player hurtbox height (standing) | 1.375 | tiles | [estimated] | none | Excludes the top of the head and the feet |
| C6 | Player hurtbox height (crouch) | 0.75 | tiles | [estimated] | TM crouch dodges projectiles (S6) | Clears floor-anchored ball hazards set at 1 tile |
| C7 | Hazard hitbox inset per exposed side | 0.125 | tiles (2 px) | [estimated] | Community notes on hitboxes larger than art in DC (S36) | Bias toward the player |
| C8 | Enemy contact hitbox inset vs enemy hurtbox | 0.125 | tiles | [estimated] | none | Hero can graze an enemy without damage |
| C9 | One-way land tolerance | 0.125 | tiles | [estimated] | none | Catches fast falls at the fall cap ↳S1 row 17 (19 tiles/s ≈ 0.32 tiles per frame [estimated]) |
| C10 | Drop-through ignore window | 0.20 | s | [estimated] | none | Long enough to clear a 1-tile-thick platform at the start of a fall |
| C11 | Slope snap distance (if slopes added) | 0.25 | tiles | [estimated] | n/a in v1 | |
| C12 | Platform vertical-velocity inheritance | 0.5 | × upward platform speed | [estimated] | none | Gives a lift without breaking S1 jump heights |
| C13 | Platform horizontal-velocity inheritance | 1.0 | × | [estimated] | none | Matches player expectation |
| C14 | Crush tolerance (overlap before death) | 0.25 | tiles | [estimated] | TM: crush = death (S6) | Smaller overlaps get pushed out |
| C15 | Collapsing platform shake time | 0.5 | s | [estimated] | DC "slight delay" (S19) | Readable warning |
| C16 | Collapsing platform respawn | 3.0 | s | [estimated] | DC: never regenerates (S19); TM not documented | Backtracking world. Decided (user): platforms come back |
| C17 | Start HP | 5 | pips | [estimated] (basis: S4, S24) | TM 5 [sourced] (S4); HK 5 [sourced] (S24) | Same as S1 row 45 |
| C18 | Max HP | 10 | pips | [estimated] | TM 8 [sourced] (S4); HK 9 [sourced] (S24) | Decided (user): 10 pips, design decision. Known difference: S1 row 45 says 8; this value governs System 2 |
| C19 | D-Light | 1 | pips | [estimated] (basis: S6) | TM 1 [sourced] (S6) | |
| C20 | D-Heavy | 2 | pips | [estimated] | HK 2 [sourced] for some late attacks (S26) | |
| C21 | D-Hazard | 3 | pips | [estimated] (basis: S6) | TM spikes 3 [sourced] (S6) | Same as S1 row 46 |
| C22 | Damage reduction (upgrade) | 1 | pip, on hits ≥ 2 | [estimated] (basis: S4) | TM rule (S4) | |
| C23 | Minimum damage per hit | 1 | pip | [estimated] | none | |
| C24 | Post-hit i-frames | 1.0 | s | [estimated] | Same as S1 row 48; HK 1.3 [sourced] s (S11, provenance unclear); DC none (community statement, S35; no measured number) | Unchanged |
| C25 | Hazard re-hit lockout (same instance) | 0.5 | s | [estimated] | HK hazards ignore i-frames (S12); TM damage-boost implies some grace (S33) | Decided (user): adopted (§8.3) |
| C26 | Contact re-arm (overlap absent) | 1 | frame (≈ 0.017 s) | [estimated] | none | §6 rule 4 |
| C27 | Knockback hold fraction | 0.5 | × duration | [estimated] | none | Then linear decay; ↳S1 row 49 |
| C28 | Knockback same-x tie zone | 0.25 | tiles | [estimated] | Fixes TM seeded direction (S33) | §7 rule 1 |
| C29 | Hazard warp arrival i-frames (all damage) | 1.0 | s | [estimated] | none | Prevents warp loops |
| C30 | Death D-1 freeze | 0.15 | s | [estimated] | none | Readable lethal hit |
| C31 | Death D-2 animation | 0.8 | s | [estimated] | none | Short; TM-style quick retry (S8 in S1-doc) |
| C32 | Death D-3 fade | 0.4 | s | [estimated] | Same as the warp fade in S1 row 51 | |
| C33 | Respawn HP | full (= max HP, 10 [estimated]) | pips | [estimated] | TM 3 [sourced] (S7); TM upgrade 5 [sourced] (S4); HK full at rest point (S24) | Decided (user): full, design decision |
| C34 | Respawn grace i-frames (all damage) | 1.5 | s | [estimated] | none | |
| C35 | Lethal-hit guard threshold (assist only) | 3 | pips or more before the hit | [estimated] | DC 25% [sourced] of max (S14) | Off by default |
| C36 | Lethal-hit guard recharge (assist only) | 45 | s | [estimated] (basis: S14) | DC 45 [sourced] s (S14) | Copies DC |
| C37 | I-frame flicker rate | 15 | Hz | [estimated] | none | Readable at 60 fps (every 4 [estimated] frames) |
| C38 | Low-health signal at | 1 | pip | [estimated] (basis: S24) | HK 1 [sourced] (S24) | |
| C39 | Retracting spike warning window | 0.4 | s | [estimated] | DC shows a warning shine (S19), unmeasured | |
| C40 | Grapple i-frames cover D-Hazard | yes | | [estimated] | TM grapple i-frames (S4) | Never D-Lethal (S1-doc), D-Pit or kill plane |
| C41 | D-Pit damage | 2 | pips | [estimated] | HK: damage + warp (S13); DC: 30 [sourced] base, capped at 30% [sourced] of max HP, + teleport (S14, S19) | Decision 1 sets the behaviour; the amount is my choice: 20% of 10 pips, close to DC's 30% cap, below a spike's 3 |
| C42 | D-Pit warp fade | 0.4 | s | [estimated] | Same as the warp fade in S1 row 51 | Reuses the existing warp |

---

## 12. Interactions with System 1 states

Every System 1 state from S1-doc §8 is listed. "Change" means a ⚑S1 rule in this spec. "None" means the state simply uses the global rules (§1, §5–§8).

| S1 state | Collision / damage interaction | Change? | Section |
|---|---|---|---|
| G-IDLE | Standard hurtbox. Contact damage, hazards and knockback all apply. Carried by moving platforms. | Carry (rule only) | §4.1 |
| G-RUN | As G-IDLE. Inherits platform velocity on walking off; coyote measured from the last contact frame. | Yes: platform velocity term | §4.2–4.3 |
| G-CROUCH | Hurtbox shrinks to C6. Collision box shrinks to C3. Drop-through on one-ways starts the C10 ignore window and clears coyote and the jump buffer. | Yes | §1.6, §2.3, §2.5 |
| A-RISE-HELD | Passes up through one-way platforms. Start velocity includes the platform term on jump-off. A head bump against a moving ceiling can trigger the crush check if the platform is descending. | Yes | §2.2, §4.2, §4.5 |
| A-RISE-FREE | As A-RISE-HELD (pass-up through one-ways). | No | §2.2 |
| A-APEX | Standard. If a qualifying hit and damage land on the same frame, damage wins and the grant is dropped. | Yes: new §3 edge case | §1.7 |
| A-FALL | Lands on one-ways per the C9 tolerance. A killing blow removes the enemy's contact hitbox before contact resolves. Entering a pit volume → D-Pit damage, then H-WARP (not D-DEAD) unless HP reaches 0. | Yes: kill-before-contact; pit warp (differs from S1-doc §6 rule 3) | §2.1, §6.3, §9 |
| A-FASTFALL | Never drops through one-ways. Lands with the same tolerance; the tolerance must cover the higher fast-fall cap (S1 row 18). | Yes | §2.8 |
| A-GLIDE | Standard hurtbox, no change. Cannot be entered during H-KNOCK. Gliding into lasers and swinging hazards uses normal hazard rules; no glide-specific protection. | Yes: blocked during knockback | §7.6 |
| W-CLING | Never on one-ways, slopes or hazard-tagged walls. Moves with moving walls and breaks if the wall pulls away. Cannot start during knockback. A cling surface that becomes a crusher face counts toward crush. | Yes | §2.6, §4.4, §7.3 |
| W-JUMP | Inherits the moving wall's velocity like a platform jump-off. Re-cling lockout (S1 row 42) is unchanged. | Yes: velocity term | §4.2 |
| X-GRAPPLE | Ray passes one-ways. Tracks moving targets. I-frames cover enemy and D-Hazard damage, but not D-Lethal, D-Pit or kill-plane volumes. | Yes | §2.7, §4.8, §8.4 |
| H-KNOCK | Deterministic direction, hold-then-decay. Wall stops the horizontal component. Hazard clamp. No cling or glide until the end or a cancel. | Yes | §7 |
| H-WARP | Now also entered from bottomless pits and the kill plane (D-Pit), as well as pit-spikes. Arrival grants C29 i-frames that cover hazards too. Damage is applied before the warp. | Yes: S1 says "i-frames on" and pits are lethal there; System 2 governs (see differences) | §8.5, §9 |
| D-DEAD | Split into D-1 to D-5 in System 2 only. The body never crosses rooms. The penalty marker goes to the last safe ground. Respawn at full HP (C33) and grace C34. | System 2 only; S1 edit declined | §9 |
| (no state) Dash / roll | System 1 has no dash or roll state. If one is added: an i-frame flag (default off), plus a hurtbox height of 1 tile while rolling (DC convention, S14). | n/a | §8.7 |
| (overlay) Attack | The hitbox moves with the hero; one grant per target per swing; it loses to same-frame damage; a killing blow removes the enemy's contact hitbox first. | Yes | §1.5, §1.7, §6.3 |

### System 1 values this system depends on (not changed)
| S1 row | Value | Used by |
|---|---|---|
| 17 | Fall cap 19 tiles/s [estimated] | C9 one-way tolerance |
| 18 | Fast-fall cap 26 tiles/s [estimated] | §2.8 tolerance check |
| 19 | Coyote 0.10 s [estimated] | §2.4–2.5, §4.3 |
| 25, 28 | Per-swing lockout; attack window 0.10 s [estimated] | §1.5 |
| 38 | Grapple i-frames | §8.4 |
| 42 | Re-cling lockout 0.12 s [estimated] | W-JUMP |
| 46 | Spike damage 3 [sourced] | D-Hazard (C21) |
| 48 | Post-hit i-frames 1.0 s [estimated] | C24 |
| 49–50 | Knockback 0.25 s [estimated]; 7 and 9 tiles/s [estimated] | §7.3 |
| 51–52 | Warp fade 0.4 s [estimated]; safe-ground dwell 0.2 s [estimated] | §9, C32 |

### Known cross-system differences (System 1 edits declined by the user)
The user chose to leave System 1 as is (decision 7, 14 Nov 2026). The six edits an earlier version of this spec proposed are **declined, not pending**, and System 1 is untouched. The table below records each remaining mismatch and which value governs inside System 2's own rules. No System 1 edit is implied.

| # | Topic | System 1 says | System 2 uses | Governs in System 2 |
|---|---|---|---|---|
| X1 | Max HP | S1 row 45: max 8 [sourced] (the TM reference) | 10 [estimated] pips (C18, user decision 4) | System 2 value (10) |
| X2 | Hazard re-hit | S1-doc §6 rule 2: hazards ignore i-frames, no lockout | Same, plus a 0.5 [estimated] s same-instance lockout (C25, user decision 2) | System 2 rule (lockout applies) |
| X3 | Bottomless pits | S1-doc §6 rule 3: instant death | D-Pit damage + warp to safe ground in exploration areas (C41, user decision 1). The kill plane also warps. | System 2 rule (damage + warp) |
| X4 | Death state | Single D-DEAD | Sub-states D-1 to D-5, respawn at full HP (user decision 3) | System 2 sub-states |
| X5 | Warp arrival i-frames | H-WARP → G-IDLE "i-frames on" (coverage unspecified) | Arrival i-frames cover hazards too (C29) | System 2 rule |
| X6 | Knockback exits | No guard on H-KNOCK → W-CLING or → A-GLIDE | Both blocked until knockback ends or is cancelled (§7.3, §7.6) | System 2 rule |
| X7 | Bounce-attack targets and same-frame order | §3 target list without falling ceiling hazards; no same-frame rule | Falling ceiling hazards qualify; damage beats grant; a killing blow removes contact first (§1.7, §6.3, §10) | System 2 rule |
| X8 | Drop-through | G-CROUCH → A-FALL without clearing timers | Drop-through also clears coyote and the jump buffer (§2.5) | System 2 rule |
| X29 | Tags on copied defaults | S1 rows 45–47 and 53 keep [sourced] on values copied from TM (start 5 HP, spike 3, contact 1, penalty 60 s) | The same values tagged [estimated] with the reference as basis (CR34 policy, decision 8) | System 2 tags; System 1 is not edited |

---

## Open Questions
1. **Slopes in TM and DC.** No opened source confirms either way. HK has none per a forum observation only (S28). It needs footage review.
2. **TM post-hit i-frame length,** and whether they block hazards. The spike damage-boost technique (S33) implies some protection, but the rule is undocumented.
3. **TM knockback distance and duration.** Unpublished. The "spike seed" behaviour (S33) suggests direction is not always away from the spike; Bastion Zero deliberately replaces this.
4. **HK hurtbox size vs sprite, and HK hit recoil distance.** Only excluded (compiled-code) sources give numbers.
5. **DC's general lack of contact damage and post-hit i-frames** is backed by community threads (S35, S34) and inference, not a wiki statement. Later patches may have changed it.
6. **DC health multiplier tables (S21):** provenance unclear (possibly derived from game data). Only the base 100 and the diminishing-returns shape are used.
7. **HK moving platforms:** no rules documented beyond wall-cling working on moving walls (S11).
8. **Pit damage amount.** Decision 1 sets the behaviour (damage + warp); the 2-pip amount (C41) is my estimate and can be changed.
9. **Pits outside exploration areas.** Decision 1 covers exploration areas. Whether boss arenas or optional challenge rooms keep the same rule, or use lethal pits, is not decided. Default: same rule everywhere.

**Resolved by the user (14 Nov 2026), no longer open:** pits (damage + warp), hazard re-hit lockout (adopted), respawn HP (full), max HP (10), collapsing platforms (come back), contact damage (on), System 1 edits (declined; see §12 differences); CR34 tag policy (copied defaults become [estimated]; X29).

## Dependencies on other systems
- **System 1 (movement):** all ⚑S1 items above, and the known cross-system differences X1–X8 and X29 (System 1 edits declined; System 2 values govern within System 2). Fall caps and coyote feed the one-way and moving-platform tolerances.
- **Combat:** attack hitbox shapes and active frames, the per-target lockout, projectile destructibility, the killing-blow ordering, and enemy D-class assignments.
- **Enemies / AI:** the contact-damage flag, contact hitbox inset, knockback immunity flags, and one-way platform handling for enemies.
- **Level design / tiles:**
  - collision layers: solid, one-way, cling-able, hazard by class, lethal, kill plane;
  - checkpoint placement, safe-ground eligibility (every pit must have a reachable safe-ground point before it), crusher placement against the crush tolerance;
  - pit volumes, tagged D-Pit;
  - a ban on spikes on slopes if slopes are ever added.
- **Health / healing / economy:** pip UI, healing sources, the death penalty marker and currency recovery, and upgrades (max HP, damage reduction).
- **Camera:** hold during death states, and cuts for hazard warps and respawns.
- **Audio / VFX:** i-frame flicker, warning frames for retracting spikes and collapsing platforms, and the low-health signal.
- **Accessibility / assist:** the lethal-hit guard, lower damage classes, and longer i-frames.
- **Physics / collision implementation:** separate collision/hurt/hit layers, actor-to-actor push disabled, continuous collision for fast falls and the grapple, and the kill plane under every room.
