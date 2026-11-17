# Bastion Zero spec — Numeric table: Enemies (SYS4)

> Part of the Bastion Zero clean-room mechanics spec v3.2 (14 Nov 2026). Archetypes, telegraphs, aggro, respawn classes R0-R3, bosses, arenas.
> Rule text owner: systems/system-4-enemy-ai.md.
> Precedence: user decisions (file 01) > owning system > later system. Tags: [sourced], [community-measured], [estimated] (file 01).

---

### 3.4 Enemies (SYS4 owns; HP bands from SYS3)
Values with several variants list them in the order given in the parameter name.

**Telegraph, aggro and tokens**
| ID | Parameter | Value | Tag | Source | Owner | Notes |
|---|---|---|---|---|---|---|
| EN-T1 | Universal attack sign | On by default; can be turned off; size slider 10%–200% | [estimated] (DC range 10% [sourced] to 200% [sourced], S56) | DC precedent (S54, S55) | SYS4 §3.2, §14 | **Decided (user, 14 Nov 2026), decision (g).** CR37 RESOLVED (user confirmed) |
| EN-T2 | Minimum telegraph, D-Light melee | 0.35 s (21 f) | [estimated] | ~0.25 [estimated] s visual reaction (commonly cited; not sourced) + 6 f buffer | SYS4 §3.2 | |
| EN-T3 | Minimum telegraph, D-Heavy | 0.50 s (30 f) | [estimated] | | SYS4 §3.2 | |
| EN-T4 | Minimum telegraph, unblockable | 0.70 s (42 f), distinct sign colour | [estimated] | DC labels attacks that can't be blocked or parried but can be jumped (S52) | SYS4 §3.2 | |
| EN-T5 | Direction lock | 0.15 s (9 f) before E-ATTACK | [estimated] | | SYS4 §3.2 | |
| EN-T6 | Off-screen start limit | Telegraphs ≤ 2 tiles outside the camera; ranged fire ≤ 4 tiles outside, with edge indicator | [estimated] | DC edge indicator (S55) | SYS4 §3.2 | Uses the live camera rect incl. look-ahead (WC-W14) |
| EN-T7 | Slow-status telegraph stretch | +20% telegraph and recovery per slow stack | [estimated] | SYS3 slow | SYS4 §3.2 | Decision (e) |
| EN-A1 | Detection box forward | 8 tiles × ±2 tiles | [estimated] | | SYS4 §3.3 | Archetypes override |
| EN-A2 | Detection behind | 50% of forward | [estimated] | | SYS4 §3.3 | |
| EN-A3 | Noise alert radius | 6 tiles | [estimated] | | SYS4 §3.3 | |
| EN-A4 | Group alert | 6 tiles after 0.3 s; one cascade step | [estimated] | | SYS4 §3.3 | |
| EN-A5 | E-NOTICE duration | 0.3 s | [estimated] | | SYS4 §3.3 | |
| EN-A6 | Lost line of sight → E-SEARCH | 3.0 s | [estimated] | | SYS4 §3.3 | |
| EN-A7 | E-SEARCH duration | 2.0 s | [estimated] | | SYS4 §3.3 | |
| EN-A8 | Default leash | 14 tiles from home, or the player leaves the room | [estimated] | | SYS4 §3.3 | Enemies never cross room transitions |
| EN-A9 | Re-aggro lockout after E-RETURN | 1.0 s | [estimated] | | SYS4 §3.3 | |
| EN-A10 | On E-RETURN | HP kept, poise reset to 0 | [estimated] | | SYS4 §3.3 | **Decided (user, 14 Nov 2026), decision (h)** |
| EN-K1 | Melee attack tokens | 2 (early tiers), 3 (late tiers) | [estimated] | | SYS4 §3.4 | Released on E-RECOVER |
| EN-K2 | Ranged volleys in flight | 2 | [estimated] | | SYS4 §3.4 | |
| EN-K3 | Dash/block-fit rule | Blockable and parryable from the front by default (D-Light 1 guard point; D-Heavy 2 + chip 1); unblockable attacks use EN-T4 and must pass jump-fit (≤ 1.5 tiles tall), dash-out fit (reach + 0.5 ≤ 3.5 tiles) or dash-through fit (overlap ≤ 6 f inside the 8 i-frames); aerial threats need a non-air-dash answer; hazards never blocked, parried or dodged; guard pressure ≤ 4 guard points per 2.0 s | [estimated] | CF-DA4, CF-DA8, CF-BK5–CF-BK8 | SYS4 §3.5 | Dash-through overlaps: projectile ≈ 2.3 f, C-1 ≈ 4.1 f, C-2 ≈ 5.5 f, swoop ≈ 3.3 f, shield thrust ≈ 8.6–9 f (no fit) |
| EN-K4 | Parry-stagger cap | An enemy staggered by a parry can't be parry-staggered again for 3.0 s; bosses use EN-B9 immunity; from boss phase 2, ≥ 1 unblockable attack per phase | [estimated] | | SYS4 §3.5, §10 | DB-2; FD-3 (cap kept, interpretation of "No Stat lock"), FD-4 (≥ 1 unblockable per phase), **Decided (user, 14 Nov 2026)** |

**Archetypes**
| ID | Parameter | Value | Tag | Source | Owner | Notes |
|---|---|---|---|---|---|---|
| EN-P1 | Patroller HP: fodder / soldier / crawler | 10 / 30 / 10 | [estimated] | SYS3 bands | SYS4 §4 | |
| EN-P2 | Patroller patrol speed | 2.5 / 2.0 / 1.5 tiles/s (crawler along the surface) | [estimated] | | SYS4 §4 | |
| EN-P3 | Patroller chase speed | 4.0 / 4.5 tiles/s / n/a | [estimated] | | SYS4 §4 | |
| EN-P4 | Patroller detection | 6 × ±2 / 8 × ±2 tiles / none | [estimated] | | SYS4 §4 | |
| EN-P5 | Patroller attack range | 1.25 / 1.5 tiles / contact only | [estimated] | | SYS4 §4 | |
| EN-P6 | Patroller telegraph | 0.40 s (24 f) / 0.45 s (27 f) | [estimated] | | SYS4 §4 | ≥ EN-T2 |
| EN-P7 | Patroller active and lunge | 0.10 s (6 f), 0.5 tile / 0.12 s (7 f), 1.0 tile | [estimated] | | SYS4 §4 | |
| EN-P8 | Patroller recovery / cooldown | 0.50 / 0.60 s; 0.80 / 1.00 s | [estimated] | | SYS4 §4 | |
| EN-P9 | Patroller poise | flinch every hit (0.15 s) / 25, decay 10 per s / flinch | [estimated] | | SYS4 §4 | |
| EN-P10 | Patroller leash | 10 / 14 tiles / n/a | [estimated] | | SYS4 §4 | |
| EN-P11 | Patroller respawn class | R2 (R3 if an anchor) / R1 / R2 | [estimated] | | SYS4 §4 | |
| EN-R1 | Ranged HP: shooter / volley | 20 / 30 | [estimated] | | SYS4 §5 | |
| EN-R2 | Ranged detection | 12 × ±4 / 14 × ±5 tiles, line of sight | [estimated] | | SYS4 §5 | |
| EN-R3 | Ranged preferred band; panic range | 6–9 / 7–10 tiles; < 3 / < 4 tiles | [estimated] | DC shooters keep distance (S49) | SYS4 §5 | Hop back 3 [estimated] tiles or blink ≥ 6 [estimated] tiles with 0.3 [estimated] s fade |
| EN-R4 | Ranged reposition cooldown | 4.0 / 3.0 s | [estimated] | | SYS4 §5 | |
| EN-R5 | Ranged telegraph | 0.60 s (locks 0.15 s before) / 0.70 s | [estimated] | | SYS4 §5 | |
| EN-R6 | Shots per volley | 1 / 3 spaced 0.20 s | [estimated] | | SYS4 §5 | |
| EN-R7 | Projectile speed / size / range | 9 / 8 tiles/s; 0.5 tiles; 14 tiles | [estimated] | | SYS4 §5 | |
| EN-R8 | Ranged recovery / cooldown | 0.60 / 0.80 s; 1.50 / 2.00 s | [estimated] | | SYS4 §5 | |
| EN-R9 | Ranged poise / leash / respawn | 15 / 25, decay 10 per s; 18 tiles; R1 | [estimated] | | SYS4 §5 | |
| EN-C1 | Charger HP: light / heavy | 30 / 80 | [estimated] | | SYS4 §6 | |
| EN-C2 | Charger detection; start distance | 10 / 12 × ±1.5 tiles; 5–10 / 6–12 tiles | [estimated] | | SYS4 §6 | |
| EN-C3 | Charger telegraph | 0.60 / 0.75 s | [estimated] | | SYS4 §6 | C-1 blockable (chip 1) and parryable (ends the charge); heavy C-2 is unblockable: jump it (1.5 tiles tall) or dash through (≈ 5.5 f) |
| EN-C4 | Charge speed / max distance / hitbox | 12 / 10 tiles/s; 10 / 12 tiles; 1.5 / 2.0 tiles | [estimated] | DC chargers (S52) | SYS4 §6 | |
| EN-C5 | Charger recovery / wall-stun / cooldown | 0.80 / 1.00 s; 1.5 / 2.0 s; 2.0 / 2.5 s | [estimated] | | SYS4 §6 | Wall-stun counts as staggered for crits |
| EN-C6 | Charger poise / leash / respawn | 30, decay 10 per s / 60, decay 15 per s; 16 tiles; R1 | [estimated] | | SYS4 §6 | |
| EN-F1 | Flyer HP: swooper / flying shooter / drifter anchor | 10 / 20 / 10 | [estimated] | | SYS4 §7 | |
| EN-F2 | Flyer detection | 9 / 11-tile radius / none | [estimated] | | SYS4 §7 | Blocked path 2.0 [estimated] s → E-SEARCH |
| EN-F3 | Flyer hover offset / speed | 3 up, 2 side / 5 away / fixed path; 3.5 / 3.0 / 1.5 tiles/s | [estimated] | | SYS4 §7 | |
| EN-F4 | Flyer telegraph | 0.50 s (pull back 0.5 tile) / 0.60 s / n/a | [estimated] | | SYS4 §7 | |
| EN-F5 | Flyer attack | Swoop 10 tiles/s up to 6 tiles / projectile 8 tiles/s / contact | [estimated] | HK flyers (S48), DC (S49) | SYS4 §7 | |
| EN-F6 | Flyer recovery / cooldown | 0.60 / 0.60 s; 1.2 / 1.8 s | [estimated] | | SYS4 §7 | |
| EN-F7 | Flyer knockback taken | ×1.5 / ×1.0 / ×1.5 | [estimated] | SYS3 §8 | SYS4 §7 | |
| EN-F8 | Flyer leash / respawn | 15-tile radius, returns at 3 tiles/s / 15 / path; R2 / R1 / R3 | [estimated] | | SYS4 §7 | |
| EN-S1 | Shielded HP: soldier / tower | 30 / 80 | [estimated] | DC shield-bearer (S49) | SYS4 §8 | |
| EN-S2 | Frontal shield arc | 120° | [estimated] | | SYS4 §8 | Blocked hit: no HP damage, adds shield poise; 3 [estimated] f hit-stop; **no bounce (decision j, X12)** |
| EN-S3 | Shield poise / guard-break | 40, decay 8 per s / 80, decay 10 per s; 2.0 / 2.5 s | [estimated] | | SYS4 §8 | |
| EN-S4 | Body poise | 25, decay 10 per s / 60, decay 15 per s | [estimated] | | SYS4 §8 | |
| EN-S5 | Shielded detection / advance | 8 × ±2 tiles; 2.0 / 1.5 tiles/s | [estimated] | | SYS4 §8 | |
| EN-S6 | Thrust telegraph / active / reach / recovery | 0.50 / 0.60 s; 0.15 s (9 f); 2.0 / 2.5 tiles; 0.70 / 0.90 s | [estimated] | | SYS4 §8 | |
| EN-S7 | Bash telegraph / active | 0.40 / 0.45 s; 0.10 s; D-Light / D-Heavy | [estimated] | | SYS4 §8 | Entered after 0.6 [estimated] s within 1 tile |
| EN-S8 | Turn delay / cooldown / leash / respawn | 0.40 / 0.60 s; 1.2 / 1.5 s; 12 / 10 tiles; R1 | [estimated] | | SYS4 §8 | |

**Elites and modifiers**
| ID | Parameter | Value | Tag | Source | Owner | Notes |
|---|---|---|---|---|---|---|
| EN-E1 | Elite HP | 150, any base archetype | [estimated] | SYS3 elite band | SYS4 §9 | |
| EN-E2 | Elite poise / size | base × 2.5; base × 1.25 | [estimated] | DC elites larger (S49) | SYS4 §9 | |
| EN-E3 | Elite pulses (intro, enrage at 50% HP) | Radial push 2.0 tiles, no damage; 0.3 s at 0.5× time; invulnerable | [estimated] | DC 50% [sourced] (S49) | SYS4 §9 | **X13 RESOLVED (owning system SYS4):** external force field, not H-KNOCK, no i-frames |
| EN-E4 | Enrage tuning | Telegraph and cooldown −15%, never below EN-T2–EN-T4; gap-close dash with 0.5 s telegraph | [estimated] | DC teleport below 50% [sourced] (S49), replaced | SYS4 §9 | |
| EN-E5 | Elite statuses | −20% durations; CC 50% shorter per re-application within 5 s | [estimated] (DC 20% [sourced], S49) | | SYS4 §9 | = CF-S5 |
| EN-E6 | Elite leash / max per room / respawn | none in arenas, else 20 tiles; 1; R0 | [estimated] | | SYS4 §9 | |
| EN-M1 | Crystal ward | 2 crystals 4 tiles each side, 20 HP; return 6 s | [estimated] | DC 6 [sourced] s (S49) | SYS4 §9 | |
| EN-M2 | Orbiting hazard | 45°/s, radius 4 tiles, D-Hazard | [estimated] | | SYS4 §9 | Decision (a): never dodged or blocked |
| EN-M3 | Floor sweep | Telegraph 0.70 s (hazard; jump it), every 5 s, D-Hazard | [estimated] | | SYS4 §9 | Decision (a): never dodged or blocked |
| EN-M4 | Escort | 2 fodder every 12 s, max 2 alive | [estimated] | | SYS4 §9 | |
| EN-M5 | Volley crystal | 3 shots spaced 0.2 s, every 4 s | [estimated] | | SYS4 §9 | |
| EN-M6 | Clone (decision k) | Split telegraph 0.8 s; copies 55% HP each = 83 units; combined 110%; 6 tiles apart; shared token; min 0.6 s between attacks | [estimated] (DC 55% [sourced], 110% [sourced], S49) | | SYS4 §9 | **Decided (user, 14 Nov 2026), decision (k)**; designer-placed, never rolled (AE3) |

**Bosses**
| ID | Parameter | Value (small / mid / final-type) | Tag | Source | Owner | Notes |
|---|---|---|---|---|---|---|
| EN-B1 | Boss HP | 600 / 900 / 1,200 | [estimated] | SYS3 band 600–1,200; TM boss 20 [sourced] HP (S7, S59) | SYS4 §10 | |
| EN-B2 | Phases | 2 / 3 / 3 plus desperation | [estimated] | | SYS4 §10 | |
| EN-B3 | Thresholds | 50% / 66%, 33% / 70%, 40%, 10% | [estimated] | TM 25% [sourced arithmetic] steps at 15, 10, 5 [sourced] HP (S7, S59); HK 75%, 40% [sourced arithmetic] (S46, provenance unclear); DC 75% [sourced], 50% [sourced] (S51); about 70% [sourced], 50% [sourced] (S60); speed-up 50% [sourced] (S53) | SYS4 §10 | |
| EN-B4 | Attacks in phase 1; added per phase | 3 / 3 / 4; +1 or 2 / +1 or 2 / +2, last phase drops 1–2 | [estimated] | HK (S45, S46) | SYS4 §10 | |
| EN-B5 | B-INTRO; B-TRANSITION | 2.0 s (first attempt only); 1.5 / 1.5 / 2.0 s | [estimated] | | SYS4 §10 | |
| EN-B6 | Push pulse | 2.0 / 2.0 / 3.0 tiles, no damage | [estimated] | DC roar (S51) | SYS4 §10 | X13 |
| EN-B7 | Telegraph range | 0.5–1.0 / 0.45–1.0 / 0.45–1.2 s | [estimated] | | SYS4 §10 | Never below EN-T2–EN-T4 |
| EN-B8 | Recovery; B-GAP | 0.8–1.5 / 0.7–1.2 / 0.6–1.0 s; 0.6–1.0 / 0.5–0.8 / 0.4–0.7 s | [estimated] | | SYS4 §10 | |
| EN-B9 | Poise; stagger immunity | 120, 20 per s / 160, 25 per s / 200, 25 per s, none in desperation; 6 / 6 / 8 s | [estimated] | HK hit-count stagger, combo bonus under 1 [sourced] s (S45, S46); late attacks 2 [sourced] units (S26, S46) | SYS4 §10 | Early stagger end if hit 0.5 [estimated] s in |
| EN-B10 | Optional adds phase | mid boss at 40%: invulnerable up to 12 s | [estimated] | DC add phase about 40% [sourced]; 6 [sourced] s fuse objects (S52) | SYS4 §10 | |
| EN-B11 | Selection rules | No attack 3 times in a row; no 2 in a row from the unblockable set; bands near < 4, mid 4–9, far > 9 tiles; weight halves for each of the last 2 picks; recovery ≥ 0.6 s | [estimated] | | SYS4 §10 | **Decided (user, 14 Nov 2026), decision (i):** weighted random with no-repeat rules |

**Spawning, respawn and encounters**
| ID | Parameter | Value | Tag | Source | Owner | Notes |
|---|---|---|---|---|---|---|
| EN-RS1 | Respawn classes | R0 never; R1 on rest (not current room) or death; R2 room load; R3 4 s after death once ≥ 2 tiles off-camera | [estimated] (R3 timing) | HK rest (S44), HK weak enemies (S48), TM off-screen (S57, S58) | SYS4 §11 | **Decided (user, 14 Nov 2026), decision (f).** **X14 RESOLVED (later system refines):** R0 cleared slots never reset on death; uncleared arenas reset |
| EN-RS2 | Spawn-in telegraph | 0.6 s, harmless | [estimated] | | SYS4 §11 | |
| EN-RS3 | Minimum spawn distance from player | 3 tiles | [estimated] | | SYS4 §11 | SYS5 entry rule: none within 4 [estimated] tiles at room load (WC-T9) |
| EN-RS4 | Attack lockout after spawn | 0.5 s | [estimated] | | SYS4 §11 | |
| EN-RS5 | Lock-in waves | 2–3 waves; next wave 1.0 s after the last dies | [estimated] | TM lock rooms (S59) | SYS4 §11 | |
| EN-RS6 | Rest-point safety | No R1–R3 spawn within 8 tiles | [estimated] | | SYS4 §11; SYS5 §12.3 | |
| EN-Q1 | Threat points | Fodder, crawler, drifter 1 (non-attacking anchors 0.5); soldier, swooper 2; shooter, flying shooter, light charger 3; volley, shield soldier 4; heavy charger, tower 5; elite base × 3 + 2; elite with M6 base × 3 + 5 | [estimated] | | SYS4 §13 | |
| EN-Q2 | Surcharges | +1 per hazard strip or pit edge in reach; shielded + ranged +2; charger + pit or spikes in lane +2; 2+ flyers + ranged +2; 2 chargers in one lane and 3+ ranged on one screen not allowed | [estimated] | | SYS4 §13 | |
| EN-Q3 | Tier T1 budget | Room 3–6; arena 4 × 2 waves; pip loss ≤ 1; between rest points ≤ 4 of 10 | [estimated] | | SYS4 §13 | |
| EN-Q4 | Tier T2 budget | 6–9; 6 × 2; ≤ 2; ≤ 5 | [estimated] | | SYS4 §13 | |
| EN-Q5 | Tier T3 budget | 9–13; 8 × 3; ≤ 2; ≤ 6 | [estimated] | | SYS4 §13 | |
| EN-Q6 | Tier T4 budget | 12–16; 10 × 3; ≤ 3; ≤ 7 | [estimated] | | SYS4 §13 | CR36 FLAGGED: budgets assumed no healing; the flask (decision u) lets boss budgets rise ≤ 3 [estimated] pips per charge; needs playtest |
| EN-Q7 | Mixing rules | Release valve at ≥ 6 points; ≤ 2 attack-capable enemies with reach (≈ 1.31 tiles) touching a pit or hazard; rest after ≤ 4 combat rooms or 30 points; elite rooms clear within 10 tiles; room before B-GATE 0 points | [estimated] | | SYS4 §13 | CR28 RESOLVED (both apply; whichever comes first) vs WC-P2 |
| EN-Q8 | Guard pressure | Blockable hits per encounter ≤ 4 guard points per 2.0 s; elites and bosses exceed it only with a telegraphed string | [estimated] | CF-BK8 | SYS4 §3.5, §13 | Replaces the (a)/(c) density allowances |
| EN-X1 | Assist: telegraph multiplier | 1.0×–1.5× | [estimated] | DC assist (S56) | SYS4 §14 | |
| EN-X2 | Assist: enemy damage | −1 pip per class, minimum 1 | [estimated] | DC down to 20% [sourced] (S56) | SYS4 §14 | |
| EN-X3 | Assist: enemy HP | 50%–100% | [estimated] | DC down to 20% [sourced] in 2.5% [sourced] steps (S56) | SYS4 §14 | |
| EN-X4 | Assist: parry window | 8–12 f (up to +50%) | [estimated] | DC assist easier parry (S56) | SYS4 §14 | DB-2 |

