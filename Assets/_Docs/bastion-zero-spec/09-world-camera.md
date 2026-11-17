# Bastion Zero spec — Numeric table: World and camera (SYS5)

> Part of the Bastion Zero clean-room mechanics spec v3.2 (14 Nov 2026). Room and gate metrics, camera, map, rest points, transitions.
> Rule text owner: systems/system-5-world-exploration.md.
> Precedence: user decisions (file 01) > owning system > later system. Tags: [sourced], [community-measured], [estimated] (file 01).

---

### 3.5 World and camera (SYS5 owns)
**Graph, rooms and regions**
| ID | Parameter | Value | Tag | Source | Owner | Notes |
|---|---|---|---|---|---|---|
| WC-G1 | Total rooms | 180–260 | [estimated] | HK 307–459 [community-measured] (S68, S69); TM 8–10 [sourced] h (S75) | SYS5 §3.2 | **Decided (user, 14 Nov 2026), decision (p)** |
| WC-G2 | Regions | 8–12 | [estimated] | HK about 18 [estimated] areas (S70 count); DC 15 [sourced] base biomes (S76) | SYS5 §3.2 | Decision (p) |
| WC-G3 | Rooms per region | 15–25 | [estimated] | HK about 17–25 [estimated] (307–459 ÷ 18) | SYS5 §3.2 | |
| WC-G4 | Region size | 30–70 screens | [estimated] | none published | SYS5 §3.2 | |
| WC-G5 | Average room degree | 2.3–2.8 (WP-OPEN graph); corridor rooms ≤ 2 [estimated] until the opening | [estimated] | none published | SYS5 §3.2 | k = 2E ÷ N |
| WC-G6 | Hub degree; hubs per region | 4–6; 1–2 | [estimated] | HK central hub (S61) | SYS5 §3.2 | |
| WC-G7 | Dead-end rooms | ≤ 25%, each with a reward, rest point or secret | [estimated] | HK (S61) | SYS5 §3.2 | |
| WC-G8 | Loops per region; world loops | ≥ 2; ≥ number of regions | [estimated] | HK (S61) | SYS5 §3.2 | L = E − N + 1; example 20 [estimated] rooms, 22 [estimated] links → L = 3 [estimated] |
| WC-G9 | Links from each region | 3–4 (≥ 2 open before its own ability) | [estimated] | DC 2–4 exits per biome (count from S76) | SYS5 §3.2 | |
| WC-G10 | Critical-path share | 40–55% | [estimated] | none | SYS5 §3.2 | |
| WC-G11 | One-way links | 5–10%, each paired with a later shortcut or loop | [estimated] | none | SYS5 §3.2 | |
| WC-G12 | Spatial consistency | 100% on a global tile grid | [estimated] | HK about 98% [sourced] (S61) | SYS5 §3.2 | |
| WC-G13 | Backtrack distance target | ≤ 8 rooms from the nearest node to a newly passable gate | [estimated] | | SYS5 §3.3 | |
| WC-G14 | Region diameter | ≤ 10 rooms | [estimated] | | SYS5 §3.3 | |
| WC-Z1 | Junction room | 1 × 1 screens; 24 × 14 tiles | [estimated] | | SYS5 §3.4 | Hubs, rest points, shops |
| WC-Z2 | Corridor room | 2–6 × 1; 48–144 × 14 | [estimated] | | SYS5 §3.4 | |
| WC-Z3 | Shaft | 1 × 2–6; 24 × 27–81 | [estimated] | | SYS5 §3.4 | |
| WC-Z4 | Hall | 2–4 × 2–3; 48–96 × 27–41 | [estimated] | | SYS5 §3.4 | |
| WC-Z5 | Arena | 1.5–2 × 1–1.5; 36–48 × 14–21 | [estimated] | | SYS5 §3.4 | |
| WC-Z6 | Gauntlet | 3–8 × 1–2; 72–192 × 14–27 | [estimated] | TM set pieces (S72) | SYS5 §3.4 | |
| WC-Z7 | Secret pocket | 0.5–1 × 0.5–1; 12–24 × 7–14, inside a host room | [estimated] | | SYS5 §3.4 | |
| WC-Z8 | Door grid | 4 tiles on room edges | [estimated] | | SYS5 §3.4 | |
| WC-N1 | Region template | Entry links 2–4; hubs 1–2; rest points 2–3; node 1; map pickup 1 within the first 3–5 rooms; 0–1 traversal ability and 1–2 combat items; 2–3 foreign gate types; secrets 1 per 4–6 rooms; shortcuts ≥ 2; 1 boss or 1–2 elites; 1 hazard family + 1 borrowed | [estimated] | HK 11 [sourced] stations for about 18 [estimated] areas (S66) | SYS5 §4.1 | |
| WC-N2 | Corridor-act share | 15–25% of rooms (about 27–65 of 180–260) | [estimated] | TM shift around the midpoint (S73); after level 3 of 10 [sourced] (S74) | SYS5 §4.3 | **Decided (user, 14 Nov 2026), decision (m)** |
| WC-N3 | Corridor regions; playtime before the opening | 2–3; 1.5–2.5 h | [estimated] | | SYS5 §4.3 | |
| WC-N4 | Corridor abilities; sealed branches; loops | 1–2; ≥ 1 per corridor region; 0 required, up to 1 optional per region | [estimated] | TM revisits (S74) | SYS5 §4.3 | |
| WC-N5 | Corridor revisit content | ≥ 1 new gate or reward per corridor region | [estimated] | | SYS5 §4.3 | |

**Gates, secrets, shortcuts**
| ID | Parameter | Value | Tag | Source | Owner | Notes |
|---|---|---|---|---|---|---|
| WC-K1 | GT1 high ledge (wall-cling or nearby anchor) | 5–8 tiles high; SB1 | [estimated] | | SYS5 §5.2 | |
| WC-K2 | GT2 tall shaft (wall-cling) | SB0 when > 8 tiles with no anchors; SB1 otherwise | [estimated] | | SYS5 §5.2 | |
| WC-K3 | GT3 wide gap (glide) | ≥ 9 tiles (glide comes before the air dash); SB1 | [estimated] | | SYS5 §5.2 | DB-3 |
| WC-K4 | GT4 updraft (glide) | Lift 6 tiles/s (MV-33); SB0 | [estimated] | | SYS5 §5.2 | |
| WC-K5 | GT5 hook gap (grapple) | Hooks ≤ 7 tiles apart; gap 8–14 tiles; SB1 | [estimated] | | SYS5 §5.2 | DB-3: grapple comes before the air dash, so the pre-unlock reach applies (restored from Version 2's 12–14) |
| WC-K6 | GT6 bounce field | Anchors R2 or R3; SB2 | [estimated] | | SYS5 §5.2 | |
| WC-K7 | GT7 projectile stair | Fed by a ranged enemy or turret; SB1 | [estimated] | | SYS5 §5.2 | Only gate a bought item may open |
| WC-K8 | GT8 breakable floor | 1–2 tiles thick; SB0 | [estimated] | | SYS5 §5.2 | |
| WC-K9 | GT9 cracked heavy wall | SB1 | n/a | | SYS5 §5.2 | |
| WC-K10 | *Removed:* GT10 low tunnel | n/a | n/a | | SYS5 §5.2 | DB-4: 1-tile tunnels dropped; ID not reused |
| WC-K11 | GT11 trap corridor | Re-hit lockout 0.5 s; timing and position only; SB1 | [estimated] | CD-25 | SYS5 §5.2 | Decision (a) |
| WC-K12 | GT12 enemy gate | R0 elite or lock-in waves; SB0 | n/a | | SYS5 §5.2 | |
| WC-K13 | *Removed:* GT13 shield-switch gate | n/a | n/a | | SYS5 §5.2 | Decision (b) |
| WC-K14 | GT14 lever door; GT15 drop ledge; GT16 pit field | SB0; SB2 (rest point or loop within 6 rooms); SB2 | [estimated] | | SYS5 §5.2 | |
| WC-K15 | Tease before key | Each gate type seen 10–40 rooms before its ability | [estimated] | | SYS5 §5.3 | |
| WC-K16 | Lock-and-key show | Ability room exit behind its gate within 1–3 rooms | [estimated] | | SYS5 §5.3; SYS6 §3.1 | |
| WC-K17 | Light gating | ≥ 2 regions reachable in either order (from WP-OPEN); ≥ 1 optional traversal ability | [estimated] | HK (S61, S62) | SYS5 §5.3 | |
| WC-K18 | One-way safety | Rest point or loop within 6 rooms using base kit | [estimated] | | SYS5 §6.3 | |
| WC-K19 | GT17 air-dash gap | Low-ceiling gap: ceiling 3 tiles above the takeoff floor, landing at the same height, 6–8 tiles wide, no anchors or hooks; SB0 | [estimated] | | SYS5 §5.2 | DB-3; FD-5 shape **Decided (user, 14 Nov 2026)**; blockout verification open (OQ-H10) |
| WC-K20 | Gates hold against earlier abilities only | Pre-air-dash gates ≥ 9-tile hard gaps; later gates ≥ 12 | [estimated] | | SYS5 §5.3 rule 7 | DB-3 |
| WC-K21 | Air-dash gate spacing | GT17 teased 10–40 rooms before; ability-room exit via GT17 within 1–3 rooms; first optional GT17 reward within 1–2 rooms of the gate; ≥ 2 earlier-region GT17s pay out on return | [estimated] | | SYS5 §5.3 rule 8 | DB-3 |
| WC-SC1 | Breakable wall hits | 1–3 | [estimated] | HK hit counts not documented in opened sources | SYS5 §7.1 | |
| WC-SC2 | Secret density; tells | 1 per 4–6 rooms; ≥ 1 tell, ≥ 2 for progression rewards | [estimated] | HK (S61) | SYS5 §7.3 | |
| WC-SC3 | Breakable feedback | 3 f hit-stop; no bounce from walls | [estimated] | | SYS5 §7.2 | Consistent with decision (j) |
| WC-SC4 | Optional skill rooms | count authored | [estimated] | TM 45 [sourced] optional hard rooms (S72) | SYS5 §7.1 | |
| WC-SH1 | Shortcuts per region; saving ratio | ≥ 2; cut return route ≥ 50% | [estimated] | | SYS5 §8.2 | |
| WC-SH2 | First shortcut; lift speed | within 10 minutes of entering a region; 6 tiles/s | [estimated] | | SYS5 §8.2 | |

**Transitions**
| ID | Parameter | Value | Tag | Source | Owner | Notes |
|---|---|---|---|---|---|---|
| WC-T1 | Side opening arrival | 1.5 tiles inside the matching edge | [estimated] | | SYS5 §9.1 | |
| WC-T2 | Top opening arrival and lift | 1 tile above the bottom edge; lift so the hero clears ≥ 2.5 tiles to reach a ledge within 2 tiles sideways | [estimated] | | SYS5 §9.1 | **X18 RESOLVED (owning system SYS5)** |
| WC-T3 | T-TRIGGER; T-PLACE | 1 frame each | [estimated] | | SYS5 §9.2 | **X15 RESOLVED (owning system SYS5):** coyote, buffer, stored air-jump and glide frozen and resumed |
| WC-T4 | T-FADE-OUT; T-FADE-IN | 0.20 s each | [estimated] | | SYS5 §9.2 | |
| WC-T5 | T-LOAD budget | ≤ 0.30 s | [estimated] | | SYS5 §9.2 | |
| WC-T6 | Entry grace | 0.5 s from T-FADE-IN start (enemy attacks and contact only) | [estimated] | | SYS5 §9.2 | |
| WC-T7 | T-RELOCK | 0.3 s | [estimated] | | SYS5 §9.2 | |
| WC-T8 | Total transition | ≤ 0.70 s | [estimated] | HK pre-load fade about 0.48 [community-measured] s (S71); room scrolls 0.58 [community-measured] s and 1.0 [community-measured] s (S10) | SYS5 §9.2 | |
| WC-T9 | Entry safety | Hazards ≥ 3 tiles from arrival; no enemies within 4 tiles at room load | [estimated] | | SYS5 §9.3 | Compatible with EN-RS3 (3 tiles) |
| WC-T10 | Pit safe points | ≥ 1 per pit; ≤ 12 tiles from the likely jump edge; ≥ 2 tiles from hazards | [estimated] | | SYS5 §9.4 | Implements D2-1 |

**Camera**
| ID | Parameter | Value | Tag | Source | Owner | Notes |
|---|---|---|---|---|---|---|
| WC-W1 | Viewport | 24 × 13.5 tiles | [estimated] | none | SYS5 W1 | CR24 RESOLVED (owning system): governs over SYS1 row 10's "11–12-tile-tall screens" note. |
| WC-W2 | Horizontal dead zone | 1.5 tiles wide, centred on the framing point | [estimated] | Camera-window technique (S78) | SYS5 W2 |  |
| WC-W3 | Horizontal trailing (time constant) | 0.12 s | [estimated] | HK about 0.15 [community-measured] s (S10) | SYS5 W3 |  |
| WC-W4 | Horizontal look-ahead (**Decided (user, 14 Nov 2026):** small look-ahead in the direction of travel, decision n) | 3 tiles toward facing, after 0.25 s of held input, eased over 0.5 s; dual-forward focus with a turn threshold of 1.5 tiles | [estimated] | Dual-forward focus (S78). HK none [community-measured] (S10); DC some (S38). *Considered:* no look-ahead, as in HK. Not used. | SYS5 W4 | **X16 RESOLVED (user decision n).** SYS1's no-look-ahead note stays as a reference only. |
| WC-W5 | Look-ahead during bounce chains and glide | 4 tiles in travel direction | [estimated] | none | SYS5 W5 |  |
| WC-W6 | Vertical follow on a jump | Wait 0.2 s after take-off, then follow 0.6× the rise | [estimated] | Matches HK 0.2 [community-measured] s and 0.6× [community-measured] (S10) | SYS5 W6 |  |
| WC-W7 | Vertical platform snap | Ease to the new ground height over 0.25 s on landing | [estimated] | Platform snapping (S78) | SYS5 W7 |  |
| WC-W8 | Fall lock and look-down | Lock on after 0.15 s of falling. If falling at ≥ 80% of the fall cap (SYS1 row 17) for 0.2 s, lead 3 tiles downward. | [estimated] | HK locks on in falls (S10) | SYS5 W8 |  |
| WC-W9 | Hero vertical placement (grounded) | Feet 3 tiles below screen centre | [estimated] | HK about 2 H [community-measured] below centre (S10); 2 H = 4 [estimated] tiles here | SYS5 W9 |  |
| WC-W10 | Room-bound clamp | The camera never shows outside room bounds; clamping eases over 0.15 s when look-ahead pushes into a wall | [estimated] | none | SYS5 W10 |  |
| WC-W11 | Wall-cling framing | Horizontal look-ahead flips to face away from the wall (the jump direction) | [estimated] | none | SYS5 W11 |  |
| WC-W12 | Grapple framing | Look-ahead toward the hook target, 4 tiles, while X-GRAPPLE is active | [estimated] | none | SYS5 W12 |  |
| WC-W13 | Shake and hit-stop | Shake per SYS3 §8; the camera ignores time scale during hit-stop | [estimated] | SYS3 | SYS5 W13 |  |
| WC-W14 | Off-screen threat check | S4 telegraph start uses the *current* camera rect, look-ahead included; edge indicators per SYS4 §3.2 | n/a | SYS4 | SYS5 W14 | Feeds EN-T6 and R3 off-camera checks |
| WC-W15 | C-ZONE ease out; C-LOOK hold / pan / ease back | 0.4 s; 0.4 s / 4 tiles / 0.3 s | [estimated] | | SYS5 §10.1 | |

**Map, rest points and pacing**
| ID | Parameter | Value | Tag | Source | Owner | Notes |
|---|---|---|---|---|---|---|
| WC-M1 | Map model | Hybrid: dotted sketch on entry; inked at next rest | n/a | HK (S65, S67), DC (S76) | SYS5 §11.2 | **Decided (user, 14 Nov 2026), decision (l)** |
| WC-M2 | Gate memory | Seen gates auto-marked by type | n/a | | SYS5 §11.3 | **Decided (user, 14 Nov 2026), decision (o)** |
| WC-M3 | Player markers | 6 per category, 4 categories | [estimated] | HK 6 [sourced], 4 [sourced] kinds (S65) | SYS5 §11.3 | |
| WC-M4 | Assist reveal | Secrets revealed at 90% explored | [estimated] | DC reveal at 80% [sourced] (S77) | SYS5 §11.3 | |
| WC-M5 | Region completion | 100 × (0.7 × rooms inked ÷ rooms + 0.3 × secrets found ÷ secrets) | [estimated] | | SYS6 §11.2 | |
| WC-P1 | Campaign length | 8–12 h | [estimated] | TM 8–10 [sourced] h (S75) | SYS5 §12.4 | Decision (p) |
| WC-P2 | Rooms between rest points (critical path) | 6–10 | [estimated] | HK about 7–11 [estimated] (307–459 ÷ 43 [sourced arithmetic] rest points, S44, S68, S69) | SYS5 §12.3 | CR28 RESOLVED: EN-Q7 cap (≤ 4 combat rooms or 30 points) also applies, whichever comes first |
| WC-P3 | Travel between rest points | ≤ 3 minutes | [estimated] | | SYS5 §12.3 | |
| WC-P4 | Rooms between checkpoints | 1–3 | [estimated] | TM 1–5 [sourced] screens (S57); about 4–5 [sourced] (S58) | SYS5 §12.3 | |
| WC-P5 | Rest point before a boss | ≤ 1 room, about 20 s before B-GATE | [estimated] | | SYS5 §12.3 | CR33 RESOLVED (owning system SYS5): SYS4/SYS6 say "checkpoint ≤ 1 room"; a rest point satisfies both |
| WC-P6 | Traversal abilities | 7 (wall-cling, glide, grapple, projectile bounce, glide down-strike, recovery jump, air dash) | [estimated] | HK two-upgrade blueprint (S63) | SYS5 §12.4 | CR40 RESOLVED (later/owning SYS6): projectile bounce and recovery jump are bought techniques. Air dash added by DB-3. |
| WC-P7 | Ability cadence | 1 per 60–100 minutes | [estimated] | | SYS5 §12.4 | CR39 RESOLVED (later/owning SYS6): template gives about 1 per 56 [estimated] (arithmetic) minutes with the air dash added (7 between 0.5 and 7.0 h), accepted for the corridor act's quick start |
| WC-P8 | Gate seen → ability; ability → first use | 20–60 minutes or 10–40 rooms; 1–3 rooms | [estimated] | | SYS5 §12.4 | |
| WC-P9 | Reward spacing; longest stretch without a new room | every 3–5 rooms; ≤ 10 minutes | [estimated] | HK (S61) | SYS5 §12.4 | |
| WC-P10 | Revisit share; late transformation | 20–35%; none | [estimated] | HK did one (S62) | SYS5 §12.4 | **Decided (user, 14 Nov 2026), decision (q)** |
| WC-P11 | Rest point vs checkpoint | Rest: heal, R1 respawn, cooldowns, ink, full save. Checkpoint: respawn (+ partial save) | n/a | HK (S44); TM (S57, S58) | SYS5 §12.1; SYS6 §6.2 | **X17 RESOLVED (later system):** respawn at the later of the two. CR29 RESOLVED (later/owning SYS6): checkpoint writes position, wallet and flags, not position only |
| WC-P12 | Revisit reward | ≥ 1 new gate or reward per revisit trip | [estimated] | HK (S62) | SYS5 §12.5 | |
| WC-P13 | Fast-travel nodes | 1 per region + 0–2 in hub regions; 8–14 total | [estimated] | HK 11 [sourced] stations (S66), about 61% [estimated] (arithmetic) of areas | SYS6 §9.3 | CR30 RESOLVED (later/owning SYS6) over SYS5 "1 per region" |

