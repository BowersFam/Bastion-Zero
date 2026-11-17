# Bastion Zero: System 5, World and Exploration (clean-room spec)

Version 2.3 · 14 Nov 2026 (UTC+8). Version 2.3 records the final decisions (GT17 low-ceiling design confirmed; air dash found behind an elite; CR34 tag policy); Version 2.2 is backed up as `system-5-world-exploration.md.pre-final`. User decisions (l)–(q) applied; v1 backed up as `system-5-world-exploration.md.v1`. System 3 decisions (a)–(e) propagated (backup `.pre-dash`); Version 2.2 applies S3 decisions DB-1–DB-4 (backup `system-5-world-exploration.md.pre-db`).

Primary reference: Hollow Knight (HK). Secondary references: The Messenger (TM), for its shift from linear levels to an open world, and Dead Cells (DC), for its fixed biome graph and permanent path unlocks. Procedural and run randomization are excluded (§1).

**What this spec is:** design rules and abstract graph metrics. **What it isn't:** any reference game's layout, region order or region names. No map is reproduced.

This spec builds on:
- System 1, `system-1-player-movement.md` ("S1-doc"); the user left it as is, so no edits are proposed;
- System 2, `system-2-collision-damage.md` ("S2-doc"), whose X1–X8 and X29 apply;
- System 3, `system-3-combat.md` ("S3-doc"), whose X9–X11 and X24–X28 apply. Its decisions (a)–(e) are **decided (user, 14 Nov 2026)**: no roll; a ground dash from the start and an **air dash as a later unlock** (DB-3), with enemy-only i-frames (DB-1); a held block with a parry (DB-2); no 1-tile passages (DB-4) (S3-doc §5);
- System 4, `system-4-enemy-ai.md` ("S4-doc", v2): respawn classes R0–R3 and resting (decision f) and the universal warning sign on by default (decision g); X12 declined, X13–X14 apply.

### User decisions applied (14 Nov 2026)
| # | Decision | Applied in |
|---|---|---|
| (l) | Map model: **hybrid**. A dotted sketch appears as soon as a room is entered; it is filled in (inked) at the next rest. | §2.2, §11.1–§11.3, §12.1 |
| (m) | World shape: **linear corridor act that then opens up** (TM-style), 15–25% [estimated] of rooms, before the world opens | §2.2, §3.2, §4.3, §5.3 |
| (n) | Camera: **small look-ahead in the direction of travel**. X16 is kept as a known difference vs System 1, now decided. | §2.2, §10.2, §13 (X16) |
| (o) | Gates the player has seen are **auto-marked on the map by gate type** | §11.3 |
| (p) | World size **approved**: 180–260 [estimated] rooms, 8–12 [estimated] regions, 8–12 [estimated] hours | §3.2, §12.4 |
| (q) | **No** late-game region transformation | §4.2, §12.4–§12.5 |
| GT17 | **Low-ceiling air-dash gap as written** (3-tile ceiling, 6–8 tiles wide), confirmed | §5.2 |
| CR34 | Copied reference values used as defaults are tagged [estimated], basis cited (no such value in this spec needed a retag) | §0 Tags |

**System 3 decisions (a)–(e), decided (user, 14 Nov 2026):** no roll, only dash and block; nothing dodges hazards. The former (a)–(c) branches are removed from §5. The follow-on choices are **decided (user, 14 Nov 2026)**: DB-3, the air dash is a later unlock (a new gate ability, GT17); DB-4, 1-tile tunnels are dropped (GT10 removed).

## 0. Conventions

### Tags
- **[sourced]**: stated in a public source.
- **[community-measured]**: counted or frame-measured by a third party.
- **[estimated]**: my Bastion Zero value, or arithmetic on other values; the reasoning is given.
- **"provenance unclear"**: a wiki number that may come from game data.
- **Copied defaults (CR34 policy, decided 14 Nov 2026):** a Bastion Zero value copied from a reference is tagged [estimated] with the reference as basis; the reference value itself stays [sourced].

In tables with a Tag column, the tag applies to every number in that row.

### Units and decided values used here
| Value | Setting | Where defined |
|---|---|---|
| Tile / frame | 1 tile = 16 px; 60 fps | S1-doc [estimated] |
| Viewport | 24 × 13.5 tiles (384 × 216 px) | This spec, W1 [estimated] |
| "Screen" (room-size unit) | One viewport, 24 × 13.5 tiles | [estimated] |
| Run speed | 8.5 tiles/s | S1-doc row 1 [estimated] |
| Full jump height / time to apex | 4.0 tiles / 0.40 s | S1-doc rows 10–11 [estimated] |
| Running jump distance (flat) | about 6.4–7 tiles (run speed × about 0.76–0.8 s airtime) | Arithmetic on S1-doc rows 1, 9–11, 14–15 [estimated] |
| Glide | falls 2.5 tiles/s at run speed, so about 3.4 tiles across per tile dropped | S1-doc rows 29–30 [estimated] |
| Grapple range | 7 tiles horizontal | S1-doc row 34 [estimated] |
| Air-jump | Only from a bounce-attack hit; 1 stored | S1-doc §3, row 22 [estimated] |
| Dash | 3.5 [estimated] tiles in 0.20 [estimated] s; ground from the start; air dash after its unlock (1 per airtime); no jump right after a ledge dash; standing hurtbox | S3-doc §5.2 |
| Block | Ground only; hazards never blocked | S3-doc §5.3 |
| Pits | D-Pit 2 [estimated] pips + warp to the last safe ground (user decision) | S2-doc §9, C41 |
| Safe-ground dwell | 0.2 s before a point counts as safe | S1-doc row 52 [estimated] |
| Warp arrival i-frames | 1.0 s, covers hazards | S2-doc C29 [estimated] |
| Respawn | Last checkpoint, full HP, grace 1.5 s | S2-doc D-4, C33–C34 [estimated] |
| Respawn classes | R0 persistent, R1 rest/death, R2 re-entry, R3 off-camera anchors (user decision f) | S4-doc §11 |
| Warning sign | Universal attack sign on by default (user decision g, as read by the parent agent) | S4-doc §3.2 |

### Source key
Reused: S10 (aigamedev frame measurements, includes camera), S38 (teemo DC game-feel summary; DC camera observation only), S44 (HK rest-point page), S57 and S58 (TM reviews). New codes start at S61.

| Code | Source | URL |
|---|---|---|
| S10 | aigamedev.ai, "Measured movement" (frame tracking; camera row) | https://aigamedev.ai/skills/platformer-feel/references/measured-movement.md |
| S38 | teemo.dev DC "Game Feel & Juice" (camera look-ahead observation only) | https://teemo.dev/game-design/dead-cells/systems/game-feel-and-juice/ |
| S44 | HK Wiki: rest-point page (count; what resting does) | https://hollowknight.wiki/w/Bench_(Hollow_Knight) |
| S57 | GameSpew, TM review (checkpoint spacing) | https://www.gamespew.com/2018/08/the-messenger-review/ |
| S58 | Chris Davis, TM (Switch) review (screens between save points) | https://cdavisgames.wordpress.com/2018/12/03/the-messenger-switch/ |
| S61 | PC Gamer, "How to design a great Metroidvania map" (Team Cherry interview, 2017) | https://www.pcgamer.com/how-to-design-a-great-metroidvania-map/ |
| S62 | Game Informer, "The Making Of Hollow Knight" (2018) | https://gameinformer.com/2018/10/15/the-making-of-hollow-knight |
| S63 | Kotaku Splitscreen interview with Team Cherry (2018, video) | https://www.youtube.com/watch?v=6bQP2rmNBpI |
| S64 | Power Up Gaming, Hollow Knight developer interview (2015) | https://powerupgaming.co.uk/2015/11/20/developer-interview-hollow-knight-promising-platformer-with-lots-to-hide/ |
| S65 | HK Wiki: map and mapping-tool page | https://hollowknight.wiki/w/Map_and_Quill_(Hollow_Knight) |
| S66 | HK Wiki: fast-travel page | https://hollowknight.wiki/w/Stag_Station |
| S67 | Hollow Knight Interactive Map, fan guide to the map system | https://hollowknightinteractivemap.com/guides/map-and-quill |
| S68 | Reddit r/HollowKnight, room count thread (hand count, corrected) | https://www.reddit.com/r/HollowKnight/comments/1f7ki9x/how_many_rooms_are_there_in_hollow_knight/ |
| S69 | Reddit r/HollowKnight, "How many rooms?" (separate hand count) | https://www.reddit.com/r/HollowKnight/comments/vma064/how_many_rooms/ |
| S70 | HK Wiki: world overview page (used only to count named areas) | https://hollowknight.wiki/w/Hallownest |
| S71 | HollowKnight.LoadNormalizer README (load-phase timings measured on players' machines with a race mod) | https://github.com/homothetyhk/HollowKnight.LoadNormalizer |
| S72 | ComicBook.com, interview with Sabotage Studio (TM structure, screen-by-screen design) | https://comicbook.com/gaming/news/the-messenger-interview-sabotage-studios-nintendo-switch-pc/ |
| S73 | The Young Folks, PAX East 2019 interview with TM's creative director | https://www.theyoungfolks.com/video-games/131649/the-messengers-thierry-boulanger-on-dlc-update-picnic-panic-pax-east-2019/ |
| S74 | GamesBeat, TM hands-on interview (shift point; revisiting levels) | https://gamesbeat.com/ninja-gaiden-throwback-the-messenger-is-great-and-makes-me-feel-guilty/ |
| S75 | Destructoid, interview with TM's creator (campaign length) | https://www.destructoid.com/we-talk-ninja-flips-metroidvanias-and-the-25-year-journey-of-the-messenger-with-creator-thierry-boulanger/ |
| S76 | Official DC Wiki: Biomes (biome counts, exits, map views) | https://deadcells.wiki.gg/wiki/Biomes |
| S77 | Official DC Wiki: Runes and upgrades (permanent path unlocks, map reveal) | https://deadcells.wiki.gg/wiki/Runes |
| S78 | Itay Keren, "Scroll Back: The Theory and Practice of Cameras in Side-Scrollers" (GDC 2015 talk, article version) | https://www.gamedeveloper.com/design/scroll-back-the-theory-and-practice-of-cameras-in-side-scrollers |

### Clean-room exclusions (sources not used)
- **teemo.dev HK "systems architecture / rebuild blueprint":** it lists the game's internal progression flags and key counts read from code. Not used.
- **HKViz analytics paper:** its map data was extracted from the game with mods. Not used.
- **Collectible-map sites that say their map was taken from the game files:** not used.
- **DC Wiki biome scaling formulas and tier constants** (on S76): game-file values, as already excluded in Systems 3–4.
- **The DC procedural level-design article** cited by S76: about procedural generation (§1). Not used.

---

## 1. Excluded (roguelite randomization, run structure, out of scope)

| # | Excluded | Reference | Why | What remains |
|---|---|---|---|---|
| WE1 | Procedural generation of each biome's layout | DC (S76) | Run randomization | Hand-built rooms |
| WE2 | Random exit positions inside a biome | DC (S76) | Run randomization | Authored exits and doors |
| WE3 | Random shop and treasure types and positions; cursed chests | DC (S76) | Loot and run randomization | Authored reward rooms (§8) |
| WE4 | "One biome per tier per run" route choice | DC (S76) | Run structure | Only the *shape* is kept: a fixed graph where regions have 2–4 exits (§3) |
| WE5 | Difficulty-gated doors and alternate paths that need higher difficulty | DC (S76) | Run structure | None |
| WE6 | Timed doors (open only if a biome is cleared within 2 [sourced] minutes, S76) and kill-streak or no-hit doors | DC | Run-structure challenge doors | Optional skill rooms with fixed rewards (§7) |
| WE7 | Incentivized-biome bonus | DC (S76) | Run structure | None |
| WE8 | Between-biome passages that sell, reroll and upgrade gear | DC (S76) | Run economy | Rest points (§11) |
| WE9 | The early procedural prototype in which prefab rooms rearranged each play | HK (S61; the developers abandoned it) | Randomization | None |
| WE10 | Currency costs of maps, pins, tolls and fast travel | HK (S44, S65, S66) | Economy, out of scope | Unlock conditions only |
| WE11 | Area-specific story devices (dream realm, time-shift portals as a story mechanic) | HK, TM | Narrative, out of scope | TM's two-state rooms appear only as a generic "room variant" note (§4) |

---

## 2. Reference behaviour

### 2.1 Comparison
| Topic | HK (primary) | TM | DC (fixed parts only) |
|---|---|---|---|
| World model | One continuous world, built as an interconnected space, not levels (S61, S64) | Linear levels at first. Around the midpoint the setting shifts. Later the linear framing drops away and the levels become one explorable world with revisits (S72, S73, S74). | Fixed biome graph: the order and the general location of links are predetermined (S76) |
| Size | 459 [community-measured] rooms including add-on content and interiors (S68); a separate hand count gave about 307 [community-measured] (S69). Range 307–459. | 10 [sourced] levels in the initial run (S74); 8–10 [sourced] hours (S75) | 15 [sourced] main and 4 [sourced] boss biomes in the base game; add-ons add 9 [sourced] and 8 [sourced] (S76) |
| Regions | About 18 [estimated] named areas (my count from S70's area descriptions) | 10 [sourced] levels | Biomes in tiers; each biome has exits to the next tier (S76) |
| Spatial truth | About 98% [sourced] spatially consistent by the developers' own estimate: a few small cheats, such as one room placed about 20 [sourced] tiles too low; no paradox rooms (S61) | Not documented | n/a (procedural inside a biome) |
| Connection philosophy | The developers aimed for as many links between areas as possible, and opened floors and ceilings late in development to add more (S61, S63) | Open-world phase adds paths between former levels (S73) | 2–4 [estimated] exits per biome in the wiki lists (my count from S76) |
| Gating | Started as a strict ability order; the developers later removed much of the hard gating, so many power-ups became optional (S61, S63). Abilities are used sparingly to steer players toward safer areas (S62). | Gates through upgrades during the open-world phase (S73) | Permanent path unlocks: climbable growth, teleport stones, breakable floors, wall-cling (S77) |
| Sequence breaks | Unintended skips left in unless the player can get stuck; a flying enemy was placed on purpose so it can be bounced off to skip part of a map; speedrunners finish with very few power-ups (S61) | Not documented | n/a |
| Rest points | 50 [sourced] in total, including 7 [sourced] in a special realm. Resting saves, sets respawn, heals, commits the map and respawns enemies except in the current room (S44). | Checkpoints every 1 to 5 [sourced] screens (S57); about 4–5 [sourced] screens between save points (S58) | One between each pair of biomes (S76) |
| Fast travel | 11 [sourced] stations, 2 [sourced] working tram lines, and one player-placed warp point at a time (S66) | Portals and teleports in the open-world phase (S73) | Teleport stones inside biomes after a permanent unlock (S77) |
| Map | Area maps bought from a vendor, then expanded by the player; a mapping tool is needed; rooms are committed when resting; a charm shows position; pins mark only already-found places (S62, S65, S67) | Not covered by the sources opened | Auto-map per biome; world view shows visited biomes in colour and not-yet-visited neighbours in grey (S76); a permanent unlock reveals the whole biome map at 80% [sourced] explored (S77) |
| Late-world change | One early area is transformed late in the game, re-routing familiar paths (S62) | Revisited levels change between two states (S74) | n/a |
| Room transition | Camera fade before loading of roughly 0.48 [community-measured] s (S71); load time depends on hardware | Screen-by-screen design (S72) | Not measured |
| Camera | Trails, no look-ahead, about 0.15 [community-measured] s time constant; waits 0.2 [community-measured] s after take-off, then follows about 0.6× [community-measured] of the rise; locks on in falls; feet about 2 H [community-measured] below centre (S10) | Not measured in the sources | Some look-ahead (observational, S38) |

### 2.2 Disagreements and gaps
- **World shape:** HK is one continuous world from the start, TM shifts from linear to open, and DC is a fixed tiered graph. Bastion Zero follows TM: **Decided (user, 14 Nov 2026):** a corridor act, then the world opens (decision m), §4.3.
  - **Range:** a linear-to-open ratio from 0% [estimated] (HK) to roughly the first half [estimated] (TM, S73).
- **Room count:** 307–459 [community-measured] in two hand counts. No official figure.
- **Missing everywhere:**
  - room sizes in tiles, node degrees, loop counts, gate-to-reward spacing;
  - camera dead zones for TM and DC;
  - transition fade lengths for TM and DC.
  Every Bastion Zero metric below is [estimated].
- **Map model:** HK commits explored rooms only at rest (S65, S67), while DC draws as you explore (S76). Bastion Zero uses a hybrid of the two. **Decided (user, 14 Nov 2026):** decision (l), §11.2.
- **Look-ahead:** HK and the other measured games use none (S10), DC shows some (S38). Keren's survey lists both styles (S78). **Decided (user, 14 Nov 2026):** a small look-ahead (decision n), §10.2.

---

## 3. Interconnected room graph

### 3.1 Definitions
| Term | Meaning |
|---|---|
| Room | Node. One loaded space with camera bounds. At least 1 [estimated] screen. |
| Link | Edge. A door, side opening, shaft opening or drop exit between two rooms. |
| Region | A cluster of rooms with one theme, one hazard family and a rest-point set |
| Hub | A room with degree ≥ 4 [estimated] |
| Gate | A link that needs an ability, a key, a switch or a cleared fight (§5) |
| One-way link | A drop, one-way door or breakable-from-one-side wall |
| Critical path | The shortest route that collects the required abilities and reaches the end |

### 3.2 Graph metrics: Bastion Zero targets
| Metric | Target | Tag | Reference |
|---|---|---|---|
| Total rooms | 180–260; **Decided (user, 14 Nov 2026):** approved (decision p) | [estimated] | HK 307–459 [community-measured] (S68, S69), scaled down for a campaign of 8–12 hours (TM 8–10 [sourced] h, S75) |
| Regions | 8–12; **Decided (user, 14 Nov 2026):** approved (decision p) | [estimated] | HK about 18 [estimated] areas (S70 count); DC 15 [sourced] base biomes (S76) |
| Rooms per region | 15–25 | [estimated] | HK about 17–25 [estimated] (307–459 ÷ 18) |
| Region size | 30–70 screens | [estimated] | none published |
| Average room degree | 2.3–2.8 (whole world in WP-OPEN; corridor rooms are ≤ 2 [estimated] until the opening, §4.3) | [estimated] | none published |
| Hub degree | 4–6; 1–2 hubs per region | [estimated] | HK's early design put one central hub area in the middle of the world (S61) |
| Dead-end rooms | ≤ 25% of rooms; each dead end holds a reward, rest point or secret | [estimated] | HK developers wanted lost players to keep finding things rather than only blockades (S61) |
| Loops per region (independent cycles) | ≥ 2 | [estimated] | none |
| World-level loops | ≥ number of regions | [estimated] | HK aimed for as many links as possible (S61) |
| Links from each region to other regions | 3–4 (at least 2 open before the region's own ability is obtained) | [estimated] | DC 2–4 exits per biome (my count from S76) |
| Critical-path share of rooms | 40–55% | [estimated] | none |
| One-way links | 5–10% of links; every one is paired with a later shortcut or loop back | [estimated] | none |
| Spatial consistency | 100%: every room sits at its true position on a global tile grid | [estimated] | HK about 98% [sourced] (S61) |

### 3.3 Graph formulas (plain math)
- **Average degree:** k = 2E ÷ N, where E is the number of links and N the number of rooms in the set.
- **Independent loops** (cyclomatic number) for a connected set: L = E − N + 1. Example: a region with 20 [estimated] rooms and 22 [estimated] links has L = 3 [estimated].
- **Backtrack distance:** for each newly passable gate, the fewest rooms from the nearest fast-travel node to that gate. Target ≤ 8 [estimated] rooms (§10.2).
- **Diameter:** the longest shortest path inside a region. Target ≤ 10 [estimated] rooms.

### 3.4 Room size ranges
| Room type | Size (screens wide × tall) | Size (tiles) | Use | Tag |
|---|---|---|---|---|
| Junction | 1 × 1 | 24 × 14 | Hubs, rest points, shops | [estimated] |
| Corridor | 2–6 × 1 | 48–144 × 14 | Running, combat lanes, charger lanes (S4-doc §6) | [estimated] |
| Shaft | 1 × 2–6 | 24 × 27–81 | Wall-cling climbs, glide descents | [estimated] |
| Hall | 2–4 × 2–3 | 48–96 × 27–41 | Exploration hubs, mixed encounters | [estimated] |
| Arena | 1.5–2 × 1–1.5 | 36–48 × 14–21 | Lock-in fights, bosses (S4-doc §10) | [estimated] |
| Gauntlet | 3–8 × 1–2 | 72–192 × 14–27 | Bounce-chain and glide traversal (TM built many short traversal set pieces screen by screen, S72) | [estimated] |
| Secret pocket | 0.5–1 × 0.5–1 | 12–24 × 7–14, inside a host room | Rewards behind breakable walls | [estimated] |

**Rules:**
- **Minimum room:** 1 screen. Smaller spaces are pockets inside a host room, so the camera never shows outside the bounds (§9).
- **Door positions:** doors snap to a 4-tile [estimated] grid on room edges so neighbouring rooms line up on the global grid (§3.2, spatial consistency).

---

## 4. Region structure

### 4.1 Region template
| Element | Count per region | Tag |
|---|---|---|
| Entry links | 2–4 | [estimated] |
| Hub rooms | 1–2 | [estimated] |
| Rest points | 2–3 (§11) | [estimated] |
| Fast-travel node | 1 | [estimated] (HK 11 [sourced] stations for about 18 [estimated] areas, S66) |
| Region map pickup | 1, within the first 3–5 rooms | [estimated] |
| Ability or major reward | 0–1 traversal ability; 1–2 combat items (S3-doc slots) | [estimated] |
| Gate types inside, needing *other* regions' abilities | 2–3 | [estimated] |
| Secrets | 1 per 4–6 rooms (§7) | [estimated] |
| Shortcuts | ≥ 2 (§8) | [estimated] |
| Boss or elite (R0) | 1 boss or 1–2 elites (S4-doc) | [estimated] |
| Hazard family | 1 primary, plus 1 borrowed from a neighbour | [estimated] |

### 4.2 Region state list
| State | Meaning | Enter | Exit |
|---|---|---|---|
| RG-UNSEEN | Not on the world map | Start | A room linked to this region is entered → RG-ADJACENT |
| RG-ADJACENT | Shown as a grey outline with an icon, no name (DC world-map rule, S76) | | First room entered → RG-ENTERED |
| RG-ENTERED | Name shown; visited rooms drawn per the map model (§10) | | Region map pickup → RG-MAPPED |
| RG-MAPPED | Region outline and major links shown as a sketch (HK vendor-map shape, S62, S65) | | All rooms visited → RG-COMPLETE |
| RG-COMPLETE | 100% [estimated] shown; secrets counted | | Final |

**Decided (user, 14 Nov 2026):** no late-game region transformation (decision q). Considered and declined: HK transforms one early area late in the game (S62). Regions keep their layout and enemy set for the whole campaign; revisits change only through new abilities, shortcuts and opened gates (§12.5).

### 4.3 World shape: corridor act, then opening
**Decided (user, 14 Nov 2026):** a linear corridor act that opens up into the full world (decision m), modelled on TM's shift from linear levels to an explorable world (S72, S73, S74).

*Considered:* open from the start, with a hub-and-spoke start that becomes a lattice (HK, S61). Not used.

#### World phase state list
| State | Structure | Enter | Exit |
|---|---|---|---|
| WP-CORRIDOR | The first 2–3 [estimated] regions are linear: room degree ≤ 2 [estimated], no backtracking needed, forward-only progress. Side branches are visible but sealed (gates already shown, §5.3 rule 1). | New game | The opening event at the end of the last corridor region → WP-OPENING |
| WP-OPENING | The opening event. Links between corridor regions and the wider world are revealed (sealed side branches and gates become usable as loops and backtrack routes), and the world map switches from a strip to the full region view. | Opening event | Event ends → WP-OPEN |
| WP-OPEN | The full lattice. §3.2 metrics apply to the whole world, corridor regions included. | | Final |

#### Corridor-act targets
| Metric | Target | Tag | Reference |
|---|---|---|---|
| Share of total rooms | 15–25% (about 27–65 rooms of 180–260) | [estimated] | TM shifts around the midpoint (S73); its art shift came after level 3 of 10 [sourced] (S74). Bastion Zero opens earlier. |
| Corridor regions | 2–3 | [estimated] | |
| Playtime before the opening | 1.5–2.5 hours | [estimated] | |
| Rest points and checkpoints in the corridor | Same spacing as §12.3 | [estimated] | |
| Abilities gained in the corridor | 1–2 (for example wall-cling, glide) | [estimated] | |
| Sealed side branches or gates seen in the corridor | ≥ 1 per corridor region; each becomes a loop or backtrack route after the opening | [estimated] | TM revisits earlier levels after the shift (S74) |
| Loops during WP-CORRIDOR | 0 required; up to 1 [estimated] per region as a small optional detour | [estimated] | |

**Rules:**
- **Corridor regions get revisit content.** Each one must be worth returning to after the opening: at least 1 [estimated] new gate or reward per region (§12.5).
- **Graph checks after the opening.** The §3.2 loop and link targets are checked on the WP-OPEN graph, not the corridor phase.

---

## 5. Ability gates

### 5.1 Gate dimension rules (derived from Systems 1 and 3)
| Measure | Base kit (no unlocks) | With unlock | Tag |
|---|---|---|---|
| Reachable ledge height | ≤ 3.5 tiles (full jump 4.0, minus margin) | Wall-cling: unlimited. Bounce air-jump: about 7.5 tiles if an anchor is in reach. | [estimated] |
| Crossable gap, flat | ≤ 6 tiles (running jump about 6.4–7). A ledge dash adds nothing: no jump may follow it (S3-doc §5.2, DB-3). | Glide: about 3.4 tiles across per tile dropped. Grapple: up to 7 tiles to a hook. Bounce chains: unlimited while targets last (S1-doc row 24). Air dash: running jump + 3.5 tiles ≈ 10–10.5 tiles. | [estimated] |
| Hard gate heights and gaps | Ledges ≥ 5 tiles with no bounce anchor within 6 tiles. Gaps ≥ 9 tiles with no anchors or hooks for gates whose key comes **before** the air-dash unlock; ≥ 12 tiles for gates that must also hold against the air dash (keys found after it) | | [estimated] |

**Decided (user, 14 Nov 2026), decision (c) and DB-3:** no air roll; the air dash is a later unlock and no jump may follow a ledge dash. A gate only has to hold against the abilities a player can own before its key, so: before the air-dash unlock, hard gaps stay ≥ 9 [estimated] tiles; gates whose key comes after the air dash use ≥ 12 [estimated] tiles, because a running jump plus an air dash covers about 10–10.5 [estimated] tiles (6.4–7 + 3.5 [estimated]).

### 5.2 Gate-type table
SB = sequence-break tolerance:
- **SB0** must hold: breaking it risks a softlock or story break;
- **SB1** skill skip allowed: a precise bounce chain, glide or grapple route may bypass it;
- **SB2** open: an alternative route is intended.

| ID | Gate | Required ability (system) | Reversible? | SB | Dimension rule / notes | Tag |
|---|---|---|---|---|---|---|
| GT1 | High ledge | Wall-cling (S1 W-CLING) **or** a bounce anchor placed nearby | Two-way (dropping down is free) | SB1 | 5–8 tiles high. An anchor makes it a skill route rather than an ability route. | [estimated] |
| GT2 | Tall shaft | Wall-cling | Two-way | SB0 when > 8 tiles with no anchors; SB1 otherwise | Shafts 2–6 screens tall (§3.4) | [estimated] |
| GT3 | Wide gap | Glide (S1 A-GLIDE) | Usually one-way until a return route exists | SB1 | ≥ 9 tiles (glide comes before the air dash, §5.1); no anchors in the gap for SB0 | [estimated] |
| GT4 | Updraft column | Glide (S1 updraft, row 33) | Two-way | SB0 | Lift 6 tiles/s (S1-doc row 33) | [estimated] |
| GT5 | Hook gap or ceiling route | Grapple (S1 X-GRAPPLE) | Two-way | SB1 | Hook points ≤ 7 tiles apart; gap 8–14 tiles (grapple comes before the air dash, so the pre-unlock reach applies) | [estimated] |
| GT6 | Bounce field (enemies, drifting anchors) | Bounce-attack (base kit) | Two-way | SB2 | Anchors are R2 or R3 (S4-doc §11), so the route always refills | [estimated] |
| GT7 | Projectile stair | Projectile bounce unlock (S1-doc §3) | Two-way | SB1 | Fed by a ranged enemy or turret (S4-doc §5) | [estimated] |
| GT8 | Breakable floor | Glide down-strike (S3-doc GLIDE-DOWN) or heavy secondary (S3-doc §3.4) | One-way down; becomes two-way when broken from above | SB0 | Floor 1–2 tiles thick | [estimated] |
| GT9 | Cracked heavy wall | Heavy secondary weapon (S3-doc) or a skill flagged "breaker" | Two-way once broken | SB1 | Different visual tell from a plain secret wall (§7) | [estimated] |
| GT11 | Trap corridor | Timing and position only; the dash and the block never avoid hazards (decision a, S2 rule: hazards ignore i-frames) | Two-way | SB1 | Re-hit lockout 0.5 [estimated] s (S2-doc C25) | [estimated] |
| GT12 | Enemy gate | Defeat an R0 elite or lock-in waves (S4-doc §9, §11) | Two-way once cleared | SB0 | Never the only exit from a dead end | n/a |
| GT13 | *Removed (decision b, 14 Nov 2026).* Formerly the shield-switch gate; no shield or parry in v1. | n/a | n/a | n/a | n/a | n/a |
| GT14 | Switch or lever door | None. Opened from the far side. | Shortcut (§8) | SB0 | | n/a |
| GT15 | Drop ledge | None | One-way until a shortcut | SB2 | Must lead to a rest point or loop within 6 [estimated] rooms | [estimated] |
| GT16 | Pit field | Any crossing ability | Two-way | SB2 | Every pit has a reachable safe point (§9.4); D-Pit + warp, never death | n/a |
| GT17 | Air-dash gap | Air dash (S3-doc §5.2; unlock, DB-3) | Two-way | SB0 | A gap the pre-unlock kit, glide included, can't cross: a **low-ceiling gap** (**Decided (user, 14 Nov 2026)**), with the ceiling 3 tiles above the takeoff floor (1 tile of headroom over the 2-tile hero), landing at the same height, 6–8 tiles wide, no anchors or hooks. The ceiling caps the jump rise to about 1 tile, which cuts the jump distance and leaves almost no drop to glide on; an air dash of 3.5 tiles closes the rest. Verify on a blockout (Open Question 8). | [estimated] |

### 5.3 Gating rules
1. **Tease before key.** Every gate type is seen at least once, 10–40 [estimated] rooms before its ability is obtained.
2. **Lock-and-key show.** The room that gives an ability has its exit behind that ability's gate, within 1–3 [estimated] rooms.
3. **Light gating** (HK, S62). Abilities steer the player toward safer regions, but at least 2 [estimated] regions are reachable in either order at every major point. Exception: the corridor act (WP-CORRIDOR, §4.3) is a fixed region order by decision (m); this rule applies from WP-OPEN.
4. **Optional abilities.** At least 1 [estimated] traversal ability is optional on the critical path. HK let many power-ups become optional (S61).
5. **Combat items never gate traversal on SB0 gates.** Heavy secondary and skills can open SB1 gates (GT9) only, because slot B is swappable (S3-doc §2).
6. **One gate per link.** A link never stacks two different ability gates.
7. **Gates hold against earlier abilities only.** A gate's dimensions must stop every ability a player can own before its key (in any SB1 order), not abilities found later. Sizes in §5.1 follow from this.
8. **Air-dash gate spacing (DB-3).** GT17 follows the same timing as other abilities (§12.4): first GT17 teased 10–40 [estimated] rooms before the unlock; the ability room's exit is a GT17 within 1–3 [estimated] rooms (lock-and-key); the first optional GT17 after the unlock leads to a reward (cache, fragment or shortcut) within 1–2 [estimated] rooms of the gate, and at least 2 [estimated] earlier-region GT17s pay out on the return trip.

---

## 6. Sequence breaking

### 6.1 Policy
HK's policy: keep unintended skips unless a player can get stuck without realising it (S61).

Bastion Zero adopts that policy and adds a reachability check.

### 6.2 Reachability levels
| Level | Meaning | Supported? |
|---|---|---|
| L-INTENDED | Needs the intended ability | Yes |
| L-SKILL | Reachable with base kit plus precise movement: bounce chains, glide tricks, grapple carry-through (S1-doc row 36), air-dash extensions | Yes. Must never softlock. |
| L-EXPLOIT | Collision clips, out-of-bounds, timing glitches | Not supported. The kill plane warps (S2-doc §9). |

### 6.3 Rules
1. **No softlock.** From every room reachable at L-SKILL with any ability set, there is a path to a rest point or fast-travel node using only that set. A tool checks this on the room graph [estimated].
2. **One-way safety.** Every one-way link (GT3, GT8, GT15) leads to a rest point or loop within 6 [estimated] rooms using base kit only.
3. **Out-of-order abilities.** If an SB1 skip lets a player get ability B before ability A, every room that gives B is still exitable without A.
4. **Rewarded skips.** Intentional SB1 routes, like HK's placed flyer (S61), get a minor reward or a time saving, never a required item.
5. **Story triggers** don't assume ability order. Dialogue uses ability checks, not order assumptions.
6. **Bounce anchors (R3)** respawn off-camera (S4-doc §11), so skill routes can be retried without leaving the room.

---

## 7. Secrets and breakable walls

### 7.1 Types
| ID | Type | How opened | Map behaviour | Tell | Tag |
|---|---|---|---|---|---|
| SC1 | Breakable wall | Weapon hits: 1–3 | Hidden room not shown until entered | Hairline crack, distinct hit sound | [estimated] (HK hit counts not documented in opened sources) |
| SC2 | False wall | Walk through | Revealed and drawn when entered | Slight background mismatch, draft particles | [estimated] |
| SC3 | Breakable floor | Glide down-strike or heavy hit (GT8) | Shown after break | Hollow sound on landing | [estimated] |
| SC4 | One-sided breakable wall | Breakable only from one side; acts as a shortcut (§8) | Near side shows a "sealed" icon once seen | Cracks visible only on the breakable side | [estimated] |
| SC5 | Hidden traversal route | Bounce chain or grapple to off-camera ledges | Normal | Partial ledge at the camera edge; a lone anchor | [estimated] |
| SC6 | Optional skill room | A hard traversal room with a fixed reward | Normal | Entrance marked by a distinct frame | [estimated] (TM has 45 [sourced] optional hard rooms with collectibles, S72) |

### 7.2 Breakable-wall state list
`BW-INTACT → (hit) BW-DAMAGED (crack widens; hit count kept while in the room) → (final hit) BW-BROKEN (saved permanently, like R0)`

- Leaving the room before the final hit resets the hit count. No hit counts carry between rooms.
- The hit feedback: 3 [estimated] f of hit-stop, debris, and no recoil on the player (S2-doc §7, consistent).
- An airborne hit on a breakable wall **does not** count as a bounce target, matching decision (j) for shields: terrain is not a qualifying target (S1-doc §3 list). If the wall breaks on that hit, it still gives no grant.

### 7.3 Secret rules
| Rule | Value | Tag |
|---|---|---|
| Density | 1 secret per 4–6 rooms | [estimated] (HK placed many small secrets for players who revisit, S61) |
| Tells | Every secret has at least 1 tell; at least 2 for secrets holding progression-relevant rewards | [estimated] |
| Reward | No required ability behind a secret; health or skill upgrades allowed | [estimated] |
| Universal warning sign | Never used for secrets; reserved for attacks (S4-doc decision g) | n/a |
| Secret count shown | Per region, after the region is complete | [estimated] |

---

## 8. Shortcuts

### 8.1 Types and states
| ID | Type | States | Opened from | Tag |
|---|---|---|---|---|
| SH1 | Lever door | LOCKED (visible from the near side) → OPEN (saved) | Far side | n/a |
| SH2 | One-sided breakable wall (SC4) | INTACT → BROKEN (saved) | Far side | n/a |
| SH3 | Lift | IDLE-AT-BOTTOM ⇄ MOVING ⇄ IDLE-AT-TOP; call switch at both ends once activated | First ride from the start side | n/a |
| SH4 | Drop-and-return | One-way drop (GT15) paired with SH1 or SH2 below | Below | n/a |
| SH5 | Ladder or chain drop-down | RAISED → LOWERED (saved) | Top | n/a |

### 8.2 Shortcut rules
| Rule | Value | Tag |
|---|---|---|
| Shortcuts per region | ≥ 2 | [estimated] |
| Shortcut leads to | A rest point or fast-travel node, or the region hub | [estimated] |
| Saving ratio | The shortcut cuts the return route by ≥ 50% in rooms | [estimated] |
| First shortcut opened | Within 10 minutes of entering a region on the critical path | [estimated] |
| Lift travel speed | 6 tiles/s | [estimated] |
| Locked shortcuts on the map | Shown as a "locked from this side" icon once seen | [estimated] |

---

## 9. Room transitions

### 9.1 Link types
| Type | Trigger | Arrival | Tag |
|---|---|---|---|
| Side opening | Hero collision box crosses the room edge | Placed 1.5 tiles inside the matching edge, same height offset | [estimated] |
| Top opening (going up) | Hero rises through the top edge | Placed 1 tile above the bottom edge, with vertical speed kept; a "transition lift" ensures the hero clears at least 2.5 tiles to reach a ledge within 2 tiles sideways | [estimated] |
| Bottom opening (drop exit) | Hero falls through the bottom edge | Placed at the top; fall continues. The landing zone must be hazard-free. | [estimated] |
| Door (interact) | Press up at the door | Placed in front of the paired door, standing | [estimated] |

### 9.2 Transition state list
| State | Behaviour | Duration | Tag |
|---|---|---|---|
| T-TRIGGER | Edge crossed. Velocity, facing, stored air-jump, glide state and the S1 coyote and buffer timers are recorded and frozen. | 1 frame | [estimated] |
| T-FADE-OUT | Screen fades. Input is buffered (S1 jump buffer and S3 6-frame buffers keep their frozen remaining time). | 0.20 s | [estimated] |
| T-LOAD | Next room loads behind black | Budget ≤ 0.30 s; longer loads extend black, never shown as gameplay | [estimated] |
| T-PLACE | Hero placed (§9.1); camera cut to its clamp position (§10). R2 and R3 enemies respawn per S4-doc. | 1 frame | [estimated] |
| T-FADE-IN | Control on. Frozen timers resume. | 0.20 s | [estimated] |
| T-ENTRY-GRACE | Invulnerable to enemy attacks and contact only, not hazards. No enemy may start a telegraph. | 0.5 s from the start of T-FADE-IN | [estimated] |
| T-RELOCK | The same edge can't trigger again | 0.3 s | [estimated] |

Total transition time is ≤ 0.70 [estimated] s. Comparisons:
- HK's pre-load camera fade alone is about 0.48 [community-measured] s (S71);
- measured room scrolls are 0.58 [community-measured] s in one modern platformer and 1.0 [community-measured] s in another (S10).

### 9.3 Transition safety rules
| Rule | Value | Tag |
|---|---|---|
| Entry zone clear of hazards | ≥ 3 tiles from each arrival point | [estimated] |
| Enemies near the entry at room load | None within 4 tiles; S4-doc spawn minimum is 3 tiles | [estimated] |
| Knockback across an edge | Allowed. H-KNOCK ends at T-TRIGGER; the hero arrives in a normal state. | n/a |
| Pits | No pit volume spans a room edge. A drop exit (bottom opening) is drawn as a door on the map and framed by edge markers so it is never confused with a pit. | n/a |
| Hazard re-hit lockout (S2-doc C25) | Cleared on room change | n/a |
| Safe-ground point | Set to the arrival point when T-FADE-IN ends | n/a |
| Boss arenas | B-GATE closes the entry door (S4-doc §10). Transitions out are disabled until B-DEFEAT. | n/a |

### 9.4 Pit safe-point rule
User decision: a pit deals damage and warps the player to safe ground. That makes every pit a level-design obligation:
1. Every pit volume lists at least 1 [estimated] safe point in the same room, reachable on foot from where it lands.
2. Each safe point is ≤ 12 [estimated] tiles from the pit edge the player most likely jumped from.
3. Each is ≥ 2 [estimated] tiles from any hazard and outside every enemy's attack reach at room load.
4. Each lies on solid, non-collapsing ground. Collapsing platforms come back (S2-doc C16) but never count as safe ground.
5. The S1 safe-ground recorder (row 52, 0.2 [estimated] s dwell) is the runtime source. Authored safe points are the fallback when the recorded point is invalid, for example on a moving platform.
6. Arena pits use arena safe points (S4-doc §10).

---

## 10. Camera framing

### 10.1 Camera state list
| ID | State | Enter | Behaviour | Exit |
|---|---|---|---|---|
| C-FOLLOW | Default | Room load | Dead zone, trailing, look-ahead (W-table) | Triggers below |
| C-ZONE | Authored framing volume | Hero enters the volume | Locks one axis or sets a fixed anchor; for shafts, gauntlets, reveal shots | Hero leaves the volume (0.4 [estimated] s ease out) |
| C-LOOK | Manual look | Hold up or down while grounded and idle for 0.4 [estimated] s | Pans 4 [estimated] tiles | Release (0.3 [estimated] s ease back) |
| C-ARENA | Arena lock | S4-doc B-GATE or lock-in waves | Fixed frame over the arena | B-DEFEAT or waves cleared |
| C-CUT | Instant reposition | Transitions, H-WARP, respawn | Cuts during black or fade (S2-doc dependencies) | 1 frame → C-FOLLOW |
| C-HOLD | Death hold | S2-doc D-2 | Frozen | Respawn → C-CUT |

### 10.2 Camera values
| ID | Parameter | Value | Tag | Reference |
|---|---|---|---|---|
| W1 | Viewport | 24 × 13.5 tiles | [estimated] | none |
| W2 | Horizontal dead zone | 1.5 tiles wide, centred on the framing point | [estimated] | Camera-window technique (S78) |
| W3 | Horizontal trailing (time constant) | 0.12 s | [estimated] | HK about 0.15 [community-measured] s (S10) |
| W4 | Horizontal look-ahead (**Decided (user, 14 Nov 2026):** small look-ahead in the direction of travel, decision n) | 3 tiles toward facing, after 0.25 s of held input, eased over 0.5 s; dual-forward focus with a turn threshold of 1.5 tiles | [estimated] | Dual-forward focus (S78). HK none [community-measured] (S10); DC some (S38). *Considered:* no look-ahead, as in HK. Not used. |
| W5 | Look-ahead during bounce chains and glide | 4 tiles in travel direction | [estimated] | none |
| W6 | Vertical follow on a jump | Wait 0.2 s after take-off, then follow 0.6× the rise | [estimated] | Matches HK 0.2 [community-measured] s and 0.6× [community-measured] (S10) |
| W7 | Vertical platform snap | Ease to the new ground height over 0.25 s on landing | [estimated] | Platform snapping (S78) |
| W8 | Fall lock and look-down | Lock on after 0.15 s of falling. If falling at ≥ 80% of the fall cap (S1-doc row 17) for 0.2 s, lead 3 tiles downward. | [estimated] | HK locks on in falls (S10) |
| W9 | Hero vertical placement (grounded) | Feet 3 tiles below screen centre | [estimated] | HK about 2 H [community-measured] below centre (S10); 2 H = 4 [estimated] tiles here |
| W10 | Room-bound clamp | The camera never shows outside room bounds; clamping eases over 0.15 s when look-ahead pushes into a wall | [estimated] | none |
| W11 | Wall-cling framing | Horizontal look-ahead flips to face away from the wall (the jump direction) | [estimated] | none |
| W12 | Grapple framing | Look-ahead toward the hook target, 4 tiles, while X-GRAPPLE is active | [estimated] | none |
| W13 | Shake and hit-stop | Shake per S3-doc §8; the camera ignores time scale during hit-stop | [estimated] | S3-doc |
| W14 | Off-screen threat check | S4 telegraph start uses the *current* camera rect, look-ahead included; edge indicators per S4-doc §3.2 | n/a | S4-doc |

**Interaction warning:** look-ahead changes what counts as "off-screen" for S4's 2-tile [estimated] telegraph rule and R3's off-camera respawn. Both use the live camera rect, so a look-ahead shift can make a threat visible early. That is allowed: it only ever reveals more.

---

## 11. Map and discovery

### 11.1 Room discovery state list
| State | Shown on map | Enter | Exit |
|---|---|---|---|
| MR-UNKNOWN | Nothing | Start | Region map pickup → MR-SKETCHED; entering → MR-VISITED |
| MR-SKETCHED | Outline from the region map: rough shape, main links, no secrets | Region map pickup (HK vendor maps are incomplete by design, S62, S65) | Entering → MR-VISITED |
| MR-VISITED | Drawn at once as a dotted sketch (hybrid model, decision l) | Hero enters the room | Next rest → MR-INKED |
| MR-INKED | Full room drawn; doors, shortcuts, gates seen, rest points | Next rest after MR-VISITED | Secret found → MR-INKED+SECRET |
| MR-INKED+SECRET | Hidden pocket or room added | Secret entered | Final |

**Partial rooms:** large rooms (≥ 4 [estimated] screens) draw by quarter as each is entered. HK reveals some rooms only partly (S65).

### 11.2 Map model
| Model | Rule | Reference | Effect |
|---|---|---|---|
| M-HYBRID (**Decided (user, 14 Nov 2026):** decision l) | A room is drawn at once as a dotted sketch on entry; it is inked (solid, with secrets and gate icons) at the next rest. Sketches survive death; only inking waits for rest. | Mix of HK (S65, S67) and DC (S76) | Keeps HK's rest ritual without punishing lost players |

*Considered:* HK-style commit at rest only (S65, S67), and DC-style live drawing (S76). Not used.

### 11.3 Map aids
| Aid | Bastion Zero rule | Reference | Tag |
|---|---|---|---|
| Player position | Always shown | HK needs a charm (S65); DC always shows it (S76) | n/a |
| Rest points, fast travel, shops | Auto-pinned once discovered | HK pins mark only discovered places (S65, S67) | n/a |
| Gate memory | **Decided (user, 14 Nov 2026):** each seen gate is auto-marked with its generic type icon (GT table), decision o. Icons appear on the dotted sketch at once and stay when inked. When the matching ability is obtained, those icons highlight. *Considered:* player-placed markers only (HK). | none of the references; my design | n/a |
| Locked shortcuts | "Locked from this side" icon | §8 | n/a |
| Player markers | 6 per category, 4 categories | HK sells marker sets of 6 [sourced], 4 [sourced] kinds (S65) | [estimated] |
| Completion % | Per region, after the region map is found | DC reveal at 80% [sourced] explored (S77) | n/a |
| Assist reveal | Optional: reveal secret locations in a region at 90% explored | DC's permanent map-reveal unlock (S77) | [estimated] |
| Quick map | Hold a button to overlay the current region map while moving | HK quick map (S65) | n/a |
| World view | Regions shown by RG state (§4.2) | DC world map (S76) | n/a |

---

## 12. Rest points, checkpoints and backtracking pacing

### 12.1 Rest points vs checkpoints
| Feature | Rest point (HK-style) | Checkpoint (TM-style) |
|---|---|---|
| Sets respawn | Yes | Yes |
| Heals to full | Yes | No |
| Respawns R1 enemies (except in the current room) | Yes (S4-doc decision f; HK, S44) | No |
| Refreshes skill cooldowns (S3-doc §4) | Yes | No |
| Inks the map (hybrid model) | Yes | No |
| Fast travel | Only at rest points that have a fast-travel node | No |
| Saves the game | Yes | Yes (position only) |

### 12.2 Rest-point state list
`RP-UNFOUND → RP-FOUND (pinned on the map) → RP-ACTIVE (last rested; respawn here) ⇄ RP-FOUND`

On sitting: heal, R1 respawn, map commit, cooldown refresh, save; then input on.

### 12.3 Spacing targets
| Metric | Target | Tag | Reference |
|---|---|---|---|
| Rooms between rest points (critical path) | 6–10 | [estimated] | HK about 7–11 [estimated] (307–459 rooms ÷ 43 [sourced arithmetic] non-special rest points, S44, S68, S69) |
| Median travel time between rest points | ≤ 3 minutes | [estimated] | none |
| Rooms between checkpoints | 1–3 | [estimated] | TM 1–5 [sourced] screens (S57); about 4–5 [sourced] (S58) |
| Rest point before a boss | ≤ 1 room / about 20 [estimated] s of travel before B-GATE | [estimated] | S4-doc §10 |
| Rest point after a region's first fast-travel node | Same room | [estimated] | none |
| Rest-point safety | No R1–R3 spawns within 8 [estimated] tiles | | S4-doc §11 |

### 12.4 Backtracking pacing
| Metric | Target | Tag | Reference |
|---|---|---|---|
| Campaign length | 8–12 hours (first completion, median player); **Decided (user, 14 Nov 2026):** approved (decision p) | [estimated] | TM 8–10 [sourced] h (S75) |
| Traversal abilities | 7: wall-cling, glide, grapple, projectile bounce, glide down-strike, recovery jump, air dash (S1-doc unlock list, whose dash flag now covers the air dash; DB-3) | [estimated] | HK's early blueprint was built around two movement upgrades used to pace content (S63) |
| Ability cadence | 1 per 60–100 minutes | [estimated] | none |
| Gate first seen → ability obtained | 20–60 minutes, or 10–40 rooms | [estimated] | none |
| Ability obtained → first required use | 1–3 rooms | [estimated] | Lock-and-key rule (§5.3) |
| Ability obtained → nearest old gate, from a fast-travel node | ≤ 3 minutes and ≤ 8 rooms | [estimated] | none |
| Reward spacing | Something new (item, secret, shortcut, view, NPC) every 3–5 rooms | [estimated] | HK wanted lost players to keep stumbling on new things (S61) |
| Longest stretch with no new room on the critical path | ≤ 10 minutes | [estimated] | none |
| Old-region revisit share of playtime | 20–35% | [estimated] | none |
| Late transformation | None. **Decided (user, 14 Nov 2026):** declined (decision q). | n/a | HK did one (S62) |
| Fast-travel nodes | 1 per region; unlocked on first visit | [estimated] | HK 11 [sourced] stations, S66 |

### 12.5 Revisit rules
1. **Revisits get easier:** with new abilities, old routes become faster. HK's developers describe old areas feeling transformed when revisited with new moves (S62). Bounce anchors (R3) and new shortcuts make old regions quicker.
2. **Enemies don't scale up on revisit** (S4-doc AE6 excluded tier scaling), and no region transforms (decision q).
3. **Each revisit should offer something:** at least 1 [estimated] new gate opens or new reward per revisit trip into a region.

---

## 13. Interactions with Systems 1–4

| Topic | Rule | Governs |
|---|---|---|
| Pits | D-Pit + warp; safe points per §9.4; no pit crosses a room edge | S2-doc §9 |
| Hazard warp vs transitions | A warp never crosses rooms. The safe point is in the same room. | S2-doc, this spec |
| Death respawn | The later of the last checkpoint and the last rest point; R1 enemies respawn (S4-doc) | S2-doc D-4, S4-doc §11 |
| Rest points | Heal, R1 respawn (except the current room), map commit, cooldown refresh | S4-doc decision f; this spec |
| R3 anchors | Respawn off-camera using the live camera rect (W14) | S4-doc §11 |
| Warning sign | On by default (decision g); off-screen edge indicators use the live camera | S4-doc §3.2 |
| Bounce targets | Breakable walls and terrain never qualify; shields don't either (decision j) | S1-doc §3, S4-doc |
| Dash, block and parry | Pre-unlock gap sizes follow the jump only (§5.1); GT17 is the air-dash gate (DB-3); trap corridors use the S2 rule, since the dash i-frames are enemy-only (GT11; DB-1); 1-tile tunnels are dropped (DB-4); the shield-switch gate is removed (GT13) | S3-doc §5 |
| Combat items | Can open SB1 gates only (§5.3 rule 5) | S3-doc §2 |
| Arena framing | C-ARENA on B-GATE | S4-doc §10 |
| Timers across transitions | Frozen during T-FADE-OUT to T-FADE-IN | This spec (X15) |

### Known cross-system differences (continued; no edits to Systems 1–4)
| # | Topic | Earlier system says | System 5 uses | Governs in System 5 |
|---|---|---|---|---|
| X15 | Timers and stored state across room transitions | S1 defines coyote, buffer, stored air-jump and glide but not their behaviour at room edges | All frozen during a transition and resumed after; the stored air-jump is kept | System 5 |
| X16 | Camera look-ahead | S1-doc's dependency note records that the measured reference games use no look-ahead (S10) | Small dual-forward look-ahead (W4–W5). **Decided (user, 14 Nov 2026, decision n).** No System 1 edit; the S1 note stays as is. | System 5 |
| X17 | Rest point vs checkpoint | S2-doc D-4 respawns at "the last checkpoint" and leaves healing to a later system | Two kinds of save point: checkpoints (respawn only) and rest points (heal, R1 respawn, map commit, cooldowns). Respawn at the later of the two. | System 5 (refines S2) |
| X18 | Transition lift | S1 has no upward boost at room edges | A small lift so upward exits always reach a ledge (§9.1) | System 5 |

---

## Open Questions
1. **No public metrics for room sizes, node degrees, loop counts or gate spacing** in any of the three games. Every §3–§12 target is [estimated] and should be checked on a blockout graph.
2. **Room count:** the two hand counts disagree (307 [community-measured] vs 459 [community-measured]), partly over interiors and add-on content.
3. **HK breakable walls:** hit counts and tells aren't documented in the sources opened. SC1 uses 1–3 [estimated] hits.
4. **TM and DC transition and camera timings** are unmeasured. Only HK's camera (S10) and its pre-load fade (S71) have numbers.
5. **GT17 vs late abilities:** the recovery jump (S1-doc H-KNOCK → A-RISE-HELD) and bounce anchors must not let a player clear a GT17 low-ceiling gap without the air dash; check once both are in.
6. **Corridor-act opening event:** what triggers it (a boss, an ability, a story beat) and how it is shown is left to the narrative and progression systems.
7. **Look-ahead tuning:** W4–W5 values (3–4 [estimated] tiles) need playtesting with bounce chains and grapple carry-through.
8. **GT17 geometry:** the 6–8 [estimated] tile width under a 3-tile ceiling is arithmetic, not measured; verify the capped jump and glide reach on a blockout.

### Resolved (user, 14 Nov 2026)
- (l) Map: hybrid, dotted sketch at once, inked at rest.
- (m) World shape: corridor act (15–25% [estimated] of rooms), then the world opens.
- (n) Camera: small look-ahead in the direction of travel; X16 kept as a decided known difference.
- (o) Seen gates auto-marked by type.
- (p) World size approved: 180–260 [estimated] rooms, 8–12 [estimated] regions, 8–12 [estimated] hours.
- (q) No late-game region transformation.
- System 3 (a)–(e): no roll, only dash and block; nothing dodges hazards. GT11 uses the S2 rule; GT13 removed.
- S3 DB-3: the air dash is a later unlock; new gate GT17; pre-unlock hard gaps ≥ 9 tiles.
- S3 DB-4: 1-tile tunnels dropped; GT10 removed (the ID is not reused).
- GT17: low-ceiling design as written, confirmed. Open Questions 5 and 8 (blockout verification) stay open.
- CR34: copied defaults tagged [estimated] with the reference as basis.

No open decisions for the user remain in this system.

## Dependencies on other systems
- **System 1 (movement):**
  - jump height and distance (rows 1, 9–17), glide (29–33), grapple (34–38), wall-cling (39–44), bounce rules (§3, rows 22–27), safe-ground recorder (row 52), warp fade (row 51);
  - X15, X16, X18.
- **System 2 (collision/damage):** pits and the kill plane (§9, C41–C42), collapsing platforms (C16), warp i-frames (C29), death and respawn states (D-1 to D-5), hazard lockout (C25); X17.
- **System 3 (combat):** dash reach and the ledge-dash rule (gaps), the air-dash unlock (GT17), the dash, block and parry vs hazards (traps), glide down-strike and heavy secondary (floors and walls), skills as SB1 openers, cooldown refresh at rest, DB-1–DB-4.
- **System 4 (enemies):** respawn classes R0–R3 and rest behaviour, bounce anchors, arena locks, off-screen rules against the camera, encounter budgets per room size.
- **Save system:** room discovery states, sketched and inked map data (hybrid model), seen-gate icons, world phase (WP state), opened shortcuts, broken walls, rest points found, region states, R0 flags.
- **Level-design tools:** graph metric checker (§3.3), softlock and reachability checker (§6.3), pit safe-point validator (§9.4), door-grid alignment.
- **UI:** map screen with dotted-sketch and inked styles, quick map, world view (strip view during the corridor act, full view after the opening), auto gate icons and highlight on ability gain, markers, region completion.
- **Audio:** secret-wall hit sound, region ambience changes at transitions.
- **Progression:** ability unlock order (§12.4), 1–2 [estimated] abilities in the corridor act, optional abilities, item placement for slot B and skills (S3-doc).
- **Narrative:** the corridor-act opening event (§4.3).
