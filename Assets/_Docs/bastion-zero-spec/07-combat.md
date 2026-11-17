# Bastion Zero spec — Numeric table: Combat and feel (SYS3)

> Part of the Bastion Zero clean-room mechanics spec v3.2 (14 Nov 2026). Attacks, dash, block and parry, bounce, hit feel, weapon tiers, statuses, crits.
> Rule text owner: systems/system-3-combat.md.
> Precedence: user decisions (file 01) > owning system > later system. Tags: [sourced], [community-measured], [estimated] (file 01).

---

### 3.3 Combat and feel (SYS3 owns)
**Loadout and attacks**
| ID | Parameter | Value | Tag | Source | Owner | Notes |
|---|---|---|---|---|---|---|
| CF-L1 | Slot A primary blade | 1, fixed (always the sword; bounce-attack source) | [estimated] | DC 2 [sourced] weapon slots, 2 [sourced] skill slots, 1 [sourced] amulet slot, total 5 [sourced] (S41); HK 1 fixed weapon, 5 [sourced] tiers (S11); TM 1 fixed sword (S4) | SYS3 §2 | |
| CF-L2 | Slot B secondary weapon | 1 (heavy or ranged) | [estimated] | S41 | SYS3 §2 | Swappable at checkpoints (SYS3) and at rest points (SYS6 §6.2): CR31 RESOLVED (later system, superset). Found at fixed locations (E1). Shield removed (decision b). |
| CF-L3 | Slots C/D active skills | 2 | [estimated] | S41 | SYS3 §2 | Same swap rule as CF-L2 |
| CF-A1 | G1 opener startup / active / recovery | 3 / 6 / 9 f; next-hit cancel from recovery f4; damage ×1.0 | [estimated] | Window definitions (S38, S14, S41) | SYS3 §3.1 | Dash cancel in startup and recovery, not active; block cancel in recovery only |
| CF-A2 | G2 | 3 / 6 / 9 f; cancel from recovery f4; ×1.0 | [estimated] | | SYS3 §3.1 | |
| CF-A3 | G3 finisher | 5 / 6 / 16 f; chain ends; ×2.0 | [estimated] | | SYS3 §3.1 | |
| CF-A4 | Fastest full chain | 13 + 13 + 27 = 53 f ≈ 0.88 s | [estimated] | DC starter combo 0.69 [sourced] s (S42, provenance unclear) | SYS3 §3.1 | |
| CF-A5 | Attack active window | 6 f = 0.10 s | [estimated] | MV-28 | SYS3 §3.1 | **X10 RESOLVED (consistent):** equals SYS1 row 28 |
| CF-A6 | Combo memory | 0.35 s after a hit's recovery ends | [estimated] | DC rule (S14) | SYS3 §3.1 | Dash, jump and landing don't reset; a hit taken does |
| CF-A7 | AIR1 | 2 / 6 / 10 f; no chain | [estimated] | SYS1 §3 | SYS3 §3.2 | Each connect may grant an air-jump; hit-pause and air stall per MV-26–MV-27 |
| CF-A8 | WALL1 | 2 / 6 / 10 f; no chain | [estimated] | TM (S6) | SYS3 §3.2 | Hits away from the wall only |
| CF-A9 | GLIDE-DOWN | 3 / 8 / 8 f; no chain | [estimated] | SYS1 §3 | SYS3 §3.2 | Unlock; bounces up on a qualifying hit |
| CF-A10 | CROUCH1 | 3 / 6 / 9 f; advances the ground chain as G1 | [estimated] | TM crouch attack (S6) | SYS3 §3.2 | |
| CF-B1 | Attack buffer | 6 f (0.10 s) | [estimated] | | SYS3 §3.3 | |
| CF-B2 | Dash buffer | 6 f | [estimated] | | SYS3 §3.3 | Highest priority |
| CF-B6 | Block buffer | 6 f on press; then held | [estimated] | | SYS3 §3.3 | |
| CF-B3 | Skill buffer | 6 f | [estimated] | | SYS3 §3.3 | |
| CF-B4 | Jump buffer | 0.10 s (= MV-20) | [estimated] | | SYS3 §3.3 | Never blocked by attacks (TM, S3) |
| CF-B5 | Same-frame input priority | dash > jump > attack > block > skill | [estimated] | | SYS3 §3.5 | Decided with (b)–(c). A dash on the same frame as an attack drops the attack. |
| CF-W1 | Heavy secondary | 10 / 6 / 24 f; ×3.0; +2 poise damage multiplier; no chain | [estimated] | S41 | SYS3 §3.4 | Opens GT9; alternative for GT8 |
| CF-W2 | Ranged secondary | 4 f startup / projectile / 12 f; ×0.6; charge pool 3; each charge refills in 2.0 s | [estimated] | TM charge pool of 3 [sourced] (S4) | SYS3 §3.4 | +1 charge purchasable (PE-S rows) |
| CF-W3 | *Retired:* shield secondary | n/a | n/a | DC raise 0.37 [sourced] s, auto-parry 0.2 [sourced] s, lockout 0.6 [sourced] s, absorbs 75% [sourced] (S20), kept as reference for CF-BK | SYS3 §3.4 | Removed by decision (b); the guard moved to the core block (CF-BK rows) |
| CF-K1 | Skill cooldown, short | 4 s | [estimated] | DC low end 4 [sourced] s (S41, provenance unclear) | SYS3 §4 | |
| CF-K2 | Skill cooldown, medium | 8 s | [estimated] | | SYS3 §4 | |
| CF-K3 | Skill cooldown, long | 15 s | [estimated] | DC high end 20 [sourced] s (S41, provenance unclear) | SYS3 §4 | |
| CF-K4 | Skill cast | startup 6 f, active per skill, recovery 8 f | [estimated] | | SYS3 §4 | Usable in all ground and air states, W-CLING, A-GLIDE |
| CF-K5 | Cooldown reduction | none in v1 | n/a | DC mutations (E6, excluded) | SYS3 §4; SYS6 decision (w) | SYS3 OQ7 RESOLVED (user decision w) |

**Dash (C-DASH; decisions (b)–(c), X9 RESOLVED (user decision))** — SYS3 §5.2. CF-R1–CF-R10 (roll) are retired; the former values are in the Version 1 backup.
| ID | Parameter | Value | Tag | Source | Owner | Notes |
|---|---|---|---|---|---|---|
| CF-DA1 | Availability | Ground dash: base kit. Air dash: later unlock (PE-U11) using SYS1's dash flag | [estimated] | HK acquires its dash as an ability (S11) | SYS3 §5.2 | **Decided (user, 14 Nov 2026)**, DB-3; X24 narrowed to the ground dash |
| CF-DA2 | Direction | Facing direction only; no down-dash | [estimated] | HK (S11) | SYS3 §5.2 | |
| CF-DA3 | Duration | 12 f (0.20 s), constant speed | [estimated] | HK duration only in excluded sources (OQ-L10) | SYS3 §5.2 | |
| CF-DA4 | Distance | 3.5 tiles (17.5 tiles/s) | [estimated] | Faster than MV-01 (8.5 [estimated] tiles/s) | SYS3 §5.2 | Sets MV-D2, MV-D3 |
| CF-DA5 | Cooldown | 0.40 s after the dash ends (0.60 s start to start) | [estimated] | HK 0.6 [sourced] s between dashes (S11, provenance unclear) | SYS3 §5.2 | |
| CF-DA6 | Air dash (after the unlock) | 1 per airtime; vertical speed held at 0; a ledge dash uses it up; before the unlock, air DASH input is dropped | [estimated] | HK mid-air dash (S11) | SYS3 §5.2 | DB-3 |
| CF-DA7 | Air-dash refresh | Landing, W-CLING or W-JUMP, bounce grant | [estimated] | HK (S11); SYS1 §7 | SYS3 §5.2 | X11 RESOLVED |
| CF-DA8 | I-frames | f3–f10 of 12 f (8 f ≈ 0.13 s; ≈ 2.3 tiles of travel); enemy attacks, projectiles and contact only | [estimated] | HK base dash has none (S12); SYS2 §8 rule 7 (flag, default off) | SYS3 §5.2 | **Decided (user, 14 Nov 2026)**, DB-1; X27 |
| CF-DA9 | Hurtbox and collision | Standing size; no 1-tile passages | [estimated] | HK: no hitbox change (SYS1 §7) | SYS3 §5.2 | DB-4; X26 |
| CF-DA10 | Bodies and hazards | Passes through bodies; contact that starts on f3–f10 is ignored until the overlap clears, otherwise D-Light; hazards, pits and lethal never avoided | [estimated] | SYS2 §1 rule 4, §6, §8 rule 2, C26 | SYS3 §5.2, §10 | Decision (a), DB-1 |
| CF-DA11 | Ledge dash | No coyote time and no buffered jump after a ground dash leaves a ledge; a jump during a ground dash fires at dash end only if still grounded, with no speed carry | [estimated] | | SYS3 §5.2 | **Decided (user, 14 Nov 2026)**, DB-3; X28 vs MV-19 |

**Block and parry (C-BLOCK; decision (b), DB-2)** — SYS3 §5.3–§5.4. States BK-RAISE → BK-HOLD (first 8 f = parry window) ⇄ BK-STUN → BK-RELEASE; hit in window → BK-PARRY → BK-HOLD; guard 0 → BK-BREAK.
| ID | Parameter | Value | Tag | Source | Owner | Notes |
|---|---|---|---|---|---|---|
| CF-BK1 | Input; where usable | Hold; ground states only (G-IDLE, G-RUN, G-CROUCH) and attack recovery | [estimated] | | SYS3 §5.3 | |
| CF-BK2 | Raise / release | 4 f (≈ 0.07 s) / 4 f | [estimated] | DC shield raise 0.37 [sourced] s (S20) | SYS3 §5.3 | |
| CF-BK3 | Coverage | Front 180° | [estimated] | | SYS3 §5.3 | |
| CF-BK4 | Movement | Walk at 0.35× run (≈ 3.0 tiles/s), facing locked; jump or dash cancels | [estimated] | | SYS3 §5.3 | |
| CF-BK5 | Blocked D-Light | 0 pips; 1 guard point | [estimated] | DC absorbs 75% [sourced] (S20) | SYS3 §5.3 | X25 vs CD-23 |
| CF-BK6 | Blocked D-Heavy | Chip 1 pip; 2 guard points; never takes the last pip; post-hit i-frames start | [estimated] | | SYS3 §5.3 | OQ-M14 |
| CF-BK7 | Never blocked | D-Hazard, D-Pit, D-Lethal, kill plane, attacks marked unblockable | n/a | SYS2 §8 rule 2 | SYS3 §5.3 | Decision (a) |
| CF-BK8 | Guard points | 4 | [estimated] | | SYS3 §5.3 | OQ-M4 |
| CF-BK9 | Guard regeneration | 1 point per 0.5 s after 0.75 s with no blocked hit | [estimated] | | SYS3 §5.3 | |
| CF-BK10 | Guard break | 0.8 s with no actions, no i-frames; guard refills | [estimated] | | SYS3 §5.3 | |
| CF-BK11 | Blockstun and push | D-Light 0.10 s, 0.25 tiles; D-Heavy 0.20 s, 0.75 tiles; no H-KNOCK | [estimated] | | SYS3 §5.3 | Push counts toward the pit and hazard reach rule (EN-Q7) |
| CF-BK12 | Parry window | First 8 f of BK-HOLD after the 4 f raise (frames 5–12 after the press, ≈ 0.13 s); front 180°; ground only | [estimated] | DC auto-parry 0.2 [sourced] s (S20) | SYS3 §5.4 | **Decided (user, 14 Nov 2026)**, DB-2 |
| CF-BK13 | Parry cost | 0 guard points; doesn't restart the regen delay | [estimated] | | SYS3 §5.4 | FD-2 (DB-6): no refund, **Decided (user, 14 Nov 2026)** |
| CF-BK14 | Parried D-Light melee / D-Heavy melee | Negated, 0 pips; 25 / 50 poise to the attacker; a parried charge ends at once | [estimated] | | SYS3 §5.4 | Fodder and soldiers stagger on 25 |
| CF-BK15 | Parried projectile | Reflected along its line at the same speed; 10 damage units and 10 poise to the first enemy hit | [estimated] | DC parry reflects ranged attacks (S20) | SYS3 §5.4 | FD-1 (DB-5): not a bounce target, **Decided (user, 14 Nov 2026)** |
| CF-BK16 | Parried contact | Negated, no poise reward | [estimated] | | SYS3 §5.4 | |
| CF-BK17 | Not parryable | Unblockable attacks, D-Hazard, D-Pit, D-Lethal, kill plane | n/a | DC unparryable labels (S52) | SYS3 §5.4 | |
| CF-BK18 | Whiff | Held through the window: normal block, no penalty. Released during the raise or window: 0.6 s parry lockout (next raise has no window) | [estimated] | DC failed-parry lockout 0.6 [sourced] s (S20) | SYS3 §5.4 | |

**Hit-stop, hit-stun, crits and statuses**
| ID | Parameter | Value | Tag | Source | Owner | Notes |
|---|---|---|---|---|---|---|
| CF-X1 | Crit freeze | 1 frame global, then 0.2 s at 0.5× time | [estimated] (basis for the 1 f: S37) | "a few tenths of a second" (S37) | SYS3 §6 | |
| CF-X2 | Light-enemy flinch | 0.15 s on every hit | [estimated] | | SYS3 §6 | |
| CF-X3 | Stagger duration | 1.0 s | [estimated] | DC interrupt stun 1.4 [sourced] s; meter > 100 [sourced]; decay 20 [sourced] per s (S14, provenance unclear) | SYS3 §6 | Also SYS4 E-STAGGER, B-STAGGER |
| CF-X4 | Poise damage during enemy startup | ×2 | [estimated] | DC meter favours interrupts (S14) | SYS3 §6 | |
| CF-X5 | Player self-recoil | none | n/a | HK ≈ 0.23 [community-measured] H (S10), not used | SYS3 §6; SYS2 §7 rule 2 | |
| CF-X6 | Crit multiplier | 2.0; triggers: staggered enemy; 3rd+ consecutive bounce-grant hit; from behind during startup; no stacking | [estimated] | DC ~1.5× [sourced] to 3× [sourced] (S41, provenance unclear); TM charged strike triple [sourced] (S4) | SYS3 §7 | **Decided (user, 14 Nov 2026)**, decision (d) |
| CF-S1 | Stun (decision e) | 1.0 s; refresh, no stack | [estimated] | DC (S40) | SYS3 §7 | |
| CF-S2 | Slow (decision e) | −20% move and attack speed per stack, up to 5 stacks; 5th freezes 1.5 s and resets; 3.0 s | [estimated] | DC 5 [sourced] stacks (S40) | SYS3 §7 | SYS4: each stack lengthens telegraph and recovery by 20% [estimated] |
| CF-S3 | Burn (decision e) | 1 damage unit per 0.5 s; refreshes; 3.0 s | [estimated] | S40 | SYS3 §7 | |
| CF-S4 | Bleed (decision e) | 1 damage unit per 0.5 s; stacks to 3; 4.0 s | [estimated] | S40 | SYS3 §7 | |
| CF-S5 | Elite/boss status modifier | Durations −20%; stun and freeze 50% shorter on each re-application within 5 s | [estimated] | DC 20% [sourced] (S40, S49) | SYS3 §7; SYS4 §9 | |
| CF-S6 | Player statuses | none in v1 | [estimated] | SYS2 §10 | SYS3 §7 | |

**Combat feel table (H1–H20; hit-stop attacker / target; shake amplitude / duration; knockback player; knockback enemy)**
| ID | Hit type | Value | Tag | Source | Owner | Notes |
|---|---|---|---|---|---|---|
| CF-H1 | Ground chain hit (G1/G2) | 2 f / 2 f; 0 px / 0 s; player 0; enemy 0.25 tiles (light enemies only) | [estimated] | Hit-stop 1–4 f; HK self-recoil ≈ 0.23 [community-measured] H ≈ 0.46 [estimated] tiles (S10), not used | SYS3 §8 | |
| CF-H2 | Chain finisher (G3) | 4 f / 4 f; 1 px / 0.10 s; 0; 1.0 tiles | [estimated] | Hit-stop 3–6 f; shake 0–2 px | SYS3 §8 | |
| CF-H3 | Critical hit | 1 f global freeze, then 0.2 s at 0.5× time; 2 px / 0.15 s; 0; 1.0 tiles | [estimated] (basis for the 1 f: S37) | Slow-down 0.1–0.4 s (S37) | SYS3 §8 | |
| CF-H4 | Airborne bounce-grant hit (AIR1) | 3 f / 3 f (= MV-26, 0.05 s); 0 px; 0 (air stall MV-27); 0.25 tiles | [estimated] | | SYS3 §8 | Keep ≤ 4 f |
| CF-H5 | Glide down-strike | 3 f / 3 f; 1 px / 0.08 s; upward bounce; 0.5 tiles downward | [estimated] | SYS1 §3 | SYS3 §8 | |
| CF-H6 | Projectile destroyed | 2 f / n/a; 0 px / 0 s; 0; n/a | [estimated] | | SYS3 §8 | Counts as H4 if airborne |
| CF-H7 | Kill (adds) | +2 f; +1 px / +0.10 s; 0; body flung 1.5 tiles | [estimated] | Kill bonus 0–4 f | SYS3 §8 | |
| CF-H8 | Heavy secondary hit | 5 f / 5 f; 2 px / 0.15 s; 0; 1.5 tiles | [estimated] | Hit-stop 4–8 f | SYS3 §8 | |
| CF-H9 | Enemy stagger | 4 f / 6 f; 2 px / 0.20 s; 0; 0.5 tiles | [estimated] | | SYS3 §8 | |
| CF-H10 | Skill hit | 2 f / 2 f; 1 px / 0.10 s; 0; 0.5 tiles | [estimated] | | SYS3 §8 | Per-skill override |
| CF-H11 | Player takes D-Light | 4 f global; 2 px / 0.15 s; ≈ 1.31 tiles horizontal, ≈ 0.65 tiles rise; enemy 0 | [estimated] | From MV/CD-S49–CD-S50 and CD-27 | SYS3 §8 | Inherited, not new |
| CF-H12 | Player takes D-Heavy | 6 f global; 3 px / 0.20 s; ≈ 1.31 tiles | [estimated] | | SYS3 §8 | OQ-H5: bigger heavy knockback? |
| CF-H13 | Player takes D-Hazard | 6 f global; 3 px / 0.20 s; ≈ 1.31 tiles along the hazard normal | [estimated] | SYS2 §7 | SYS3 §8 | Re-hit lockout 0.5 [estimated] s (CD-25) |
| CF-H14 | Player takes D-Pit | 0 f; 0 px; warp, fade 0.4 [estimated] s (CD-42) | [estimated] | SYS2 §9 | SYS3 §8 | |
| CF-H15 | Lethal hit | 9 f global (= CD-30, 0.15 s); 4 px / 0.30 s | [estimated] | | SYS3 §8 | |
| CF-H16 | Hit blocked (D-Light, front) | 2 f / 2 f; 1 px / 0.06 s; player 0.25 tiles back; enemy 0 | [estimated] | | SYS3 §8 | Replaces the roll-dodge row |
| CF-H17 | Parry (BK-PARRY) | 4 f / 4 f; 2 px / 0.10 s; 0; 0.5 tiles | [estimated] | | SYS3 §8 | DB-2 |
| CF-H18 | Hit blocked (D-Heavy, chip) | 4 f / 4 f; 2 px / 0.12 s; player 0.75 tiles back; enemy 0 | [estimated] | | SYS3 §8 | |
| CF-H19 | Guard break | 6 f global; 3 px / 0.20 s; player 0.5 tiles back; enemy 0 | [estimated] | | SYS3 §8 | |
| CF-H20 | Dash start | 0 f; 0 px | [estimated] | | SYS3 §8 | |
| CF-H21 | Shake falloff and options | Linear to 0; on/off plus intensity 0%–100% | [estimated] | DC ships on/off only (S38) | SYS3 §8 | |

**Damage model (player → enemy)**
| ID | Parameter | Value | Tag | Source | Owner | Notes |
|---|---|---|---|---|---|---|
| CF-D1 | Formula | dmg = W_base × T[tier] × C × M_hit | [estimated] | DC shape 1.15^(s−1), 1.15 [sourced] (S21, provenance unclear), excluded as a model (E5) | SYS3 §9 | |
| CF-D2 | W_base (slot A) | 10 units | [estimated] | | SYS3 §9 | |
| CF-D3 | Weapon tiers T1–T5 | 1.0, 1.4, 1.8, 2.2, 2.6 | [estimated] | HK 5, 9, 13, 17, 21 [sourced], +4 [sourced] per tier (S11); HK ladder 21/5 = 4.2× [sourced arithmetic] | SYS3 §9 | **X20 RESOLVED (user decision v):** bought for coin only (PE-S rows), not found at fixed locations |
| CF-D4 | Crit factor C | 2.0 on a crit, else 1.0 | [estimated] | | SYS3 §9 | Decision (d) |
| CF-D5 | Chain multiplier M_hit | 1.0, 1.0, 2.0; heavy 3.0; ranged 0.6 | [estimated] | | SYS3 §9 | |
| CF-D6 | Charged strike (shop) | ×3 next slot A hit | [estimated] (basis: S4) | TM | SYS6 §5.3 | Bought at ST4 |
| CF-D7 | HP band fodder | 10 units; 1 hit at T1 | [estimated] | TM one-hit enemies (S7) | SYS3 §9 | |
| CF-D8 | HP band soldier | 30; 3 hits | [estimated] | | SYS3 §9 | |
| CF-D9 | HP band brute | 80; 6–8 hits | [estimated] | | SYS3 §9 | |
| CF-D10 | HP band elite | 150; 10–15 hits | [estimated] | | SYS3 §9 | |
| CF-D11 | HP band boss | 600–1,200 | [estimated] | | SYS3 §9 | |
| CF-D12 | Enemy → player | Damage classes only (§3.2); no tier or difficulty scaling | n/a | SYS2 §5 | SYS2 | E7 excluded |

