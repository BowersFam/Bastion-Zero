# Bastion Zero spec — Numeric table: Progression and economy (SYS6)

> Part of the Bastion Zero clean-room mechanics spec v3.2 (14 Nov 2026). Coin, death marker, shop, flask, unlock order, persistence classes.
> Rule text owner: systems/system-6-progression-persistence.md.
> Precedence: user decisions (file 01) > owning system > later system. Tags: [sourced], [community-measured], [estimated] (file 01).

---

### 3.6 Progression and economy (SYS6 owns)
**Unlock order (median first completion 10 [estimated] h)**
| ID | Parameter | Value | Tag | Source | Owner | Notes |
|---|---|---|---|---|---|---|
| PE-U0 | Base kit | Run, jump, bounce-attack, ground dash, block with parry (0 h); opens GT6; GT11 by timing | [estimated] | | SYS6 §3.2 | DB-2, DB-3 |
| PE-U1 | Wall-cling | 0.5–1.0 h, ability room after an R0 elite; GT1, GT2 | [estimated] | | SYS6 §3.2 | Gate-critical abilities are found, never bought (HK, DC, S77) |
| PE-U2 | Projectile bounce | 0.75–1.5 h, shop ST1; GT7 | [estimated] | | SYS6 §3.2 | |
| PE-U3 | Glide | 1.5–2.5 h, boss reward; GT3, GT4 | [estimated] | | SYS6 §3.2 | |
| PE-U4 | Opening event | 1.5–2.5 h; fast travel and ST2 switch on | [estimated] | | SYS6 §3.2 | Trigger open: OQ-H1 |
| PE-U5 | Grapple | 2.5–4.0 h, ability room; GT5 | [estimated] | | SYS6 §3.2 | |
| PE-U6 | Heavy secondary | 2.5–4.5 h, fixed location; GT9, alt GT8 | [estimated] | | SYS6 §3.2 | |
| PE-U7 | Glide down-strike | 4.0–6.0 h, found; GT8 | [estimated] | TM sold it (S4); not used because GT8 is SB0 | SYS6 §3.2 | |
| PE-U8 | Recovery jump | from 2.5 h, shop ST2; no gate | [estimated] | | SYS6 §3.2 | |
| PE-U9 | Last traversal ability | by 8.5 h; ≥ 1.5 h before the end | [estimated] | | SYS6 §3.1–§3.2 | |
| PE-U10 | *Retired:* air roll technique | none | n/a | | SYS6 §3.3 | Decision (c) |
| PE-U11 | Air dash | 5.5–7.0 h, ability room after an R0 elite (found, never bought); opens GT17; later gates use ≥ 12-tile gaps | [estimated] | | SYS6 §3.2 row 6A | **Decided (user, 14 Nov 2026)**, DB-3; FD-6 source confirmed |

**Coin sources and economy**
| ID | Parameter | Value | Tag | Source | Owner | Notes |
|---|---|---|---|---|---|---|
| PE-C1 | Fodder (R2, R3) | 1 coin, flat across tiers | [estimated] | TM 1 [sourced] per enemy (S79) | SYS6 §4.2 | **Decided (user, 14 Nov 2026), decision (r)** |
| PE-C2 | Soldier, shooter, charger, shielded (R1) | 3 | [estimated] | | SYS6 §4.2 | |
| PE-C3 | Brute (R1) | 8 | [estimated] | | SYS6 §4.2 | |
| PE-C4 | Elite (R0) | 60 | [estimated] | | SYS6 §4.2 | |
| PE-C5 | Elite with M6 clone | 80, split between copies | [estimated] | | SYS6 §4.2 | |
| PE-C6 | Boss (R0) | 300 | [estimated] | | SYS6 §4.2 | |
| PE-C7 | Tier multiplier T1 / T2 / T3 / T4 (non-fodder) | ×1 / 1.5 / 2 / 2.5 | [estimated] | | SYS6 §4.2 | Reward scale only; AE6 untouched |
| PE-C8 | Small cache; large cache; challenge room | 25; 150; 250 (one-time) | [estimated] | TM lantern 3 [sourced], large crystal 65 [sourced] (S79) | SYS6 §4.2 | |
| PE-E1 | Worked income at T2 | ≈ 1,230 coin per hour (enemy 480 + cache 165 + R0 585) | [estimated] (arithmetic) | 220 [estimated] rooms ÷ 10 [estimated] h ≈ 22 [estimated] new rooms per hour; ≈ 40 [estimated] entered | SYS6 §4.2 | |
| PE-E2 | Curve 0–2 h (T1) | 450–650 coin/h; 900–1,300 cumulative; ST1 1,100 unlocked; ratio 0.8–1.2 | [estimated] | | SYS6 §4.3 | |
| PE-E3 | Curve 2–4 h (T2) | 900–1,250; 2,700–3,800; 3,600; 0.75–1.05 | [estimated] | | SYS6 §4.3 | |
| PE-E4 | Curve 4–7 h (T3) | 1,100–1,450; 6,000–8,150; 6,900; 0.85–1.2 | [estimated] | | SYS6 §4.3 | |
| PE-E5 | Curve 7–10 h (T4) | 1,200–1,600; 9,600–12,950; 10,000; 0.95–1.3 | [estimated] | | SYS6 §4.3 | |
| PE-E6 | Curve rules | Mid ratio 0.75–1.2 between 2–7 h; whole catalogue by about 90% of the campaign; critical-path floor ≥ 60%; required purchases ≤ 25% of guaranteed coin and on sale ≥ 30 minutes before use; marker losses ≤ 5%; farming ≤ 50% of exploration rate; rescale k = total ÷ 10,000 | [estimated] | TM reload farming (S79) | SYS6 §4.3 | |
| PE-E7 | Reference ratios | Catalogue per hour: TM 6,845 ÷ 12 ≈ 570 (S4, S93); HK 69,210 ÷ 41.5 ≈ 1,670 (S83, S92); BZ 10,000 ÷ 10 = 1,000. Top item: TM ≈ 29%, HK ≈ 6% (S11, S83), BZ 16%. Damage reduction: TM ≈ 15%, BZ 15%. Max-pip ladder: TM 30 → 350 (S4), HK 150 → 1,500 (S86), BZ 300 → 900 | [estimated] (arithmetic) on [sourced] and [community-measured] inputs | | SYS6 §4.4 | |

**Shop (decision t) and catalogue (decision v)**
| ID | Parameter | Value | Tag | Source | Owner | Notes |
|---|---|---|---|---|---|---|
| PE-S1 | Stock tiers | ST1 shop found; ST2 opening event; ST3 3 bosses; ST4 6 bosses or 75% of regions entered | [estimated] | HK stock growth (S83, S86) | SYS6 §5.1 | |
| PE-S2 | Shop doors | 1 rest point per region | [estimated] | TM shop checkpoints (S81) | SYS6 §5.1 | **Decided (user, 14 Nov 2026), decision (t)** |
| PE-S3 | ST1 items | Projectile bounce 100; max pip A 300; ranged charge +1 400; weapon tier T2 300 | [estimated] | TM 40 [sourced], 30 [sourced], 250 [sourced] (S4); HK 150 [sourced]; HK 250 [sourced] (S11) | SYS6 §5.3 | Total 1,100 [estimated] (arithmetic) |
| PE-S4 | ST2 items | Recovery jump 300; max pip B 900; flask charge +1 700; weapon tier T3 600 | [estimated] | TM 80 [sourced], 320 [sourced]; HK 500–800 [sourced]; DC 50–300 [sourced] meta currency (S77); HK 800 [sourced] + 1 [sourced] material (S11) | SYS6 §5.3 | Total 2,500 [estimated] (arithmetic) |
| PE-S5 | ST3 items | Ranged charge +1 800; damage reduction 1,500; weapon tier T4 1,000 | [estimated] | TM 350 [sourced], 1,000 [sourced] (S4); HK 2,000 [sourced] + 2 [sourced] materials (S11) | SYS6 §5.3 | Total 3,300 [estimated] (arithmetic) |
| PE-S6 | ST4 items | Charged strike 1,600 (×3 [estimated], basis: S4); weapon tier T5 1,500 | [estimated] | TM 2,000 [sourced]; HK 4,000 [sourced] + 3 [sourced] materials (S11) | SYS6 §5.3 | Total 3,100 [estimated] (arithmetic) |
| PE-S7 | Catalogue total | 5,900 base + 700 flask + 3,400 tier fees = 10,000 | [estimated] (arithmetic) | | SYS6 §5.3 | Tier fees 34% [estimated] (arithmetic); HK about 10% [estimated] (arithmetic, 7,050 ÷ 69,210, S83). **X20 RESOLVED (user decision v)** |

**Save, death penalty, flask, fast travel**
| ID | Parameter | Value | Tag | Source | Owner | Notes |
|---|---|---|---|---|---|---|
| PE-W1 | Persistence classes | P0 permanent; P0-W wallet; P1 rest-scoped (R1, flask charges, cooldowns, healing); P2 room-scoped (R2, collapsing platforms, R3); P3 death-scoped (marker) | n/a | | SYS6 §6.1 | |
| PE-W2 | Pickup save write | within 1 s | [estimated] | | SYS6 §6.3 | Autosave only (HK, TM: S44, S81) |
| PE-W3 | Coin at risk on crash | ≤ one checkpoint gap, 1–3 rooms | [estimated] | | SYS6 §6.3 | |
| PE-W4 | Save slots; failure warning | 3; after 2 failures | [estimated] | | SYS6 §6.3–§6.4 | |
| PE-D0 | Death respawn penalty duration | 60 s | [estimated] (basis: S4) | 30 s [sourced] with upgrade (S4) | SYS1 row 53 | **SUPERSEDED (X19).** Governing model: P-C half-coin marker (PE-D1–PE-D5), decision (s). The follower penalty is not used. SYS1 note: Economy system owns the final value |
| PE-D1 | Share of wallet dropped | 50% (lost = floor(0.5 × wallet)) | [estimated] | HK 100% [sourced] (S25); TM taxes future income (S80) | SYS6 §7 | **Decided (user, 14 Nov 2026), decision (s)**; **X19 RESOLVED (user decision s)** |
| PE-D2 | Markers in the world | 1 at a time | [estimated] | HK 1 [sourced] (S25) | SYS6 §7.4 | |
| PE-D3 | Penalty in the corridor act | none | [estimated] | | SYS6 §7.4 | |
| PE-D4 | Timing and placement | Fixed at D-1, applied at D-3; at last safe ground; arena deaths before B-GATE; never across a one-way link | n/a | HK (S25) | SYS6 §7.2–§7.3 | **X21 RESOLVED (later system refines SYS2)** |
| PE-D5 | Marker loss target | ≤ 5% of campaign coin | [estimated] | | SYS6 §4.3 | OQ-H6 |
| PE-F1 | Flask charges at start; max | 1; 3 (1 found + 1 bought) | [estimated] | DC 1 [sourced] start, 4 [sourced] max (S77) | SYS6 §8 | **Decided (user, 14 Nov 2026), decision (u)**; **X23 RESOLVED (user decision u)** |
| PE-F2 | Heal per use | 3 pips (30% [estimated] of 10) | [estimated] | DC 60% [sourced] (S77) | SYS6 §8 | Fallback 2 [estimated] (OQ-H2) |
| PE-F3 | Use | 0.5 s channel at 50% move speed; damage cancels and keeps the charge | [estimated] | DC heals over a few seconds (S77) | SYS6 §8 | Not in X-GRAPPLE, H-KNOCK, H-WARP, D-states, C-DASH, BK-BREAK; drinking drops the block |
| PE-F4 | Refill | rest points, death, reload | [estimated] | | SYS6 §8 | |
| PE-F5 | Budget allowance | boss budgets may rise ≤ 3 pips per charge carried | [estimated] | | SYS6 §8 rule 3 | CR36 FLAGGED |
| PE-H1 | Max pips growth | 5 → 10 via 3 sets of 4 fragments + 2 shop pips | [estimated] (HK 4 [sourced] fragments per pip, S86) | HK (S86), TM (S4) | SYS6 §4.1 | **X22 RESOLVED (owning system SYS6 fills a gap)** |
| PE-V1 | Fast travel switch-on | at WP-OPENING; free; node to node; only from a node | n/a | HK toll and bell not used (S66) | SYS6 §9.1 | **Decided (user, 14 Nov 2026), decision (y)** (no price) |
| PE-V2 | Travel time | fade 0.4 s + load + fade 0.4 s; ≤ 3 s total | [estimated] | CD-32 | SYS6 §9.1 | |
| PE-V3 | Node distances | rest point → node ≤ 12 rooms and ≤ 4 minutes; node → hub ≤ 4 rooms; boss checkpoint → node ≤ 6 rooms; node → old gate ≤ 3 minutes and ≤ 8 rooms | [estimated] | | SYS6 §9.3 | |

**Deferred (post-v1, decision x) and completion**
| ID | Parameter | Value | Tag | Source | Owner | Notes |
|---|---|---|---|---|---|---|
| PE-T1 | Notches (deferred) | 3 → 6 (3 bought at 200 / 500 / 900) | [estimated] | HK 3 → 11 [sourced]; 120, 500, 900, 1,400 [sourced] (S84) | SYS6 §10 | Not in v1 |
| PE-T2 | Trinkets (deferred) | 12–16; cost 1–3 notches; no overfill; swap at rest points only | [estimated] | HK 45 [sourced] (S84) | SYS6 §10 | If added: trinket completion weight 0.10 [estimated] |
| PE-Q1 | Completion weights | Abilities 0.22; bosses 0.17; max pips 0.12; weapon tiers 0.11; rooms inked 0.17; secrets and caches 0.11; challenge rooms 0.05; shop 0.05 (sum 1.00) | [estimated] | HK 1 [sourced] point per boss (S85); TM 45 [sourced] collectibles (S82) | SYS6 §11.1 | Capped at 100 (HK goes to 112 [sourced], S85); integer, rounded down; no missables |

