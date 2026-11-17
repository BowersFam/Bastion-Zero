# Bastion Zero spec — Numeric table: Collision and damage (SYS2)

> Part of the Bastion Zero clean-room mechanics spec v3.2 (14 Nov 2026). Hitboxes, hazards, pips, death states D-1 to D-5, respawn, pit rules.
> Rule text owner: systems/system-2-collision-damage.md.
> Precedence: user decisions (file 01) > owning system > later system. Tags: [sourced], [community-measured], [estimated] (file 01).

---

### 3.2 Collision and damage (SYS2 owns; SYS1 rows 45–52 shown with their governing status)
| ID | Parameter | Value | Tag | Source | Owner | Notes |
|---|---|---|---|---|---|---|
| CD-S45 | Starting HP | 5 HP | [estimated] (basis: S4) | max 8 [sourced] (S4) | SYS1 row 45 | **SUPERSEDED (X1).** Start 5 stands; max 8 [sourced] is superseded by CD-18: governing max = 10 [estimated] pips (D2-4). Copy the TM scale for hazard math |
| CD-S46 | Spike damage | 3 HP | [estimated] (basis: S6) | HK per-hit amount unconfirmed | SYS1 row 46 | Same value as CD-21 (D-Hazard). Consistent. 3 of 5 starting HP: a spike hit is frightening but survivable once |
| CD-S47 | Enemy contact damage | 1 HP | [estimated] (basis: S6, S7) | none | SYS1 row 47 | Same as CD-19 (D-Light). Consistent.  |
| CD-S48 | Post-hit i-frames | 1.0 s | [estimated] | HK 1.3 s [sourced] (S11, provenance unverified); TM not published | SYS1 row 48 | Same as CD-24. SYS1 says "provenance unverified"; SYS2 says "provenance unclear" for the same HK value: treated as provenance unclear (CR38). Slightly shorter than HK, given TM's faster pace |
| CD-S49 | Knockback duration (no cancel) | 0.25 s | [estimated] | HK no-input recoil 0.2 s [sourced] (S11) | SYS1 row 49 | Governing; SYS2 C27 adds the hold fraction. Cancellable (S3) |
| CD-S50 | Knockback speed | 7 horizontal, 9 up tiles/s | [estimated] | none | SYS1 row 50 | Governing. Horizontal distance ≈ 1.31 [estimated] tiles (SYS3 H11). Clears a 1-tile spike strip |
| CD-S51 | Hazard warp fade | 0.4 s | [estimated] | Behaviour (S13), unmeasured | SYS1 row 51 | Reused by CD-32 and CD-42.  |
| CD-S52 | Safe-ground dwell time | 0.2 s | [estimated] | none | SYS1 row 52 | Runtime source for the pit safe-point rule (SYS5 §9.4) and the marker (SYS6 §7.3). Prevents saving a safe point mid-hazard |
| CD-01 | Player collision box width | 0.75 tiles | [estimated] | none published | SYS2 C1 | Fits 1-tile gaps with margin |
| CD-02 | Player collision box height (standing) | 1.75 tiles | [estimated] | H = 2 tiles (SYS1) | SYS2 C2 | Slightly under the drawn height so the hero fits 2-tile corridors |
| CD-03 | Player collision box height (crouch) | 1.0 tiles | [estimated] | DC roll shrinks to 1 [sourced] tile (S14) | SYS2 C3 | Matches DC's 1-tile-gap convention |
| CD-04 | Player hurtbox width | 0.5 tiles | [estimated] | Not measured in any opened source | SYS2 C4 | Forgiving sideways |
| CD-05 | Player hurtbox height (standing) | 1.375 tiles | [estimated] | none | SYS2 C5 | Excludes the top of the head and the feet |
| CD-06 | Player hurtbox height (crouch) | 0.75 tiles | [estimated] | TM crouch dodges projectiles (S6) | SYS2 C6 | Clears floor-anchored ball hazards set at 1 tile |
| CD-07 | Hazard hitbox inset per exposed side | 0.125 tiles (2 px) | [estimated] | Community notes on hitboxes larger than art in DC (S36) | SYS2 C7 | Bias toward the player |
| CD-08 | Enemy contact hitbox inset vs enemy hurtbox | 0.125 tiles | [estimated] | none | SYS2 C8 | Hero can graze an enemy without damage |
| CD-09 | One-way land tolerance | 0.125 tiles | [estimated] | none | SYS2 C9 | Catches fast falls at the fall cap ↳S1 row 17 (19 tiles/s ≈ 0.32 tiles per frame [estimated]) |
| CD-10 | Drop-through ignore window | 0.20 s | [estimated] | none | SYS2 C10 | Long enough to clear a 1-tile-thick platform at the start of a fall |
| CD-11 | Slope snap distance (if slopes added) | 0.25 tiles | [estimated] | n/a in v1 | SYS2 C11 |  |
| CD-12 | Platform vertical-velocity inheritance | 0.5 × upward platform speed | [estimated] | none | SYS2 C12 | Gives a lift without breaking S1 jump heights |
| CD-13 | Platform horizontal-velocity inheritance | 1.0 × | [estimated] | none | SYS2 C13 | Matches player expectation |
| CD-14 | Crush tolerance (overlap before death) | 0.25 tiles | [estimated] | TM: crush = death (S6) | SYS2 C14 | Smaller overlaps get pushed out |
| CD-15 | Collapsing platform shake time | 0.5 s | [estimated] | DC "slight delay" (S19) | SYS2 C15 | Readable warning |
| CD-16 | Collapsing platform respawn | 3.0 s | [estimated] | DC: never regenerates (S19); TM not documented | SYS2 C16 | Backtracking world. Decided (user): platforms come back Decided (D2-5). |
| CD-17 | Start HP | 5 pips | [estimated] (basis: S4, S24) | TM 5 [sourced] (S4); HK 5 [sourced] (S24) | SYS2 C17 | Same as S1 row 45 Same as SYS1 row 45 start value. |
| CD-18 | Max HP | 10 pips | [estimated] | TM 8 [sourced] (S4); HK 9 [sourced] (S24) | SYS2 C18 | Decided (user): 10 pips, design decision. Known difference: S1 row 45 says 8; this value governs System 2 **Governs X1** (D2-4): supersedes SYS1 row 45 max 8. |
| CD-19 | D-Light | 1 pips | [estimated] (basis: S6) | TM 1 [sourced] (S6) | SYS2 C19 |  |
| CD-20 | D-Heavy | 2 pips | [estimated] | HK 2 [sourced] for some late attacks (S26) | SYS2 C20 |  |
| CD-21 | D-Hazard | 3 pips | [estimated] (basis: S6) | TM spikes 3 [sourced] (S6) | SYS2 C21 | Same as S1 row 46 |
| CD-22 | Damage reduction (upgrade) | 1 pip, on hits ≥ 2 | [estimated] (basis: S4) | TM rule (S4) | SYS2 C22 |  |
| CD-23 | Minimum damage per hit | 1 pip | [estimated] | none | SYS2 C23 | X25 RESOLVED (owning system SYS3, decision b): a blocked D-Light deals 0 pips; outside the block the floor stands. |
| CD-24 | Post-hit i-frames | 1.0 s | [estimated] | Same as S1 row 48; HK 1.3 [sourced] s (S11, provenance unclear); DC none (community statement, S35; no measured number) | SYS2 C24 | Unchanged |
| CD-25 | Hazard re-hit lockout (same instance) | 0.5 s | [estimated] | HK hazards ignore i-frames (S12); TM damage-boost implies some grace (S33) | SYS2 C25 | Decided (user): adopted (§8.3) **RESOLVED X2** (D2-2). |
| CD-26 | Contact re-arm (overlap absent) | 1 frame (≈ 0.017 s) | [estimated] | none | SYS2 C26 | §6 rule 4 |
| CD-27 | Knockback hold fraction | 0.5 × duration | [estimated] | none | SYS2 C27 | Then linear decay; ↳S1 row 49 |
| CD-28 | Knockback same-x tie zone | 0.25 tiles | [estimated] | Fixes TM seeded direction (S33) | SYS2 C28 | §7 rule 1 |
| CD-29 | Hazard warp arrival i-frames (all damage) | 1.0 s | [estimated] | none | SYS2 C29 | Prevents warp loops |
| CD-30 | Death D-1 freeze | 0.15 s | [estimated] | none | SYS2 C30 | Readable lethal hit |
| CD-31 | Death D-2 animation | 0.8 s | [estimated] | none | SYS2 C31 | Short; TM-style quick retry (S8 in SYS1) |
| CD-32 | Death D-3 fade | 0.4 s | [estimated] | Same as the warp fade in S1 row 51 | SYS2 C32 |  |
| CD-33 | Respawn HP | full (= max HP, 10 [estimated]) pips | [estimated] | TM 3 [sourced] (S7); TM upgrade 5 [sourced] (S4); HK full at rest point (S24) | SYS2 C33 | Decided (user): full, design decision **RESOLVED X4** (D2-3). Respawn location: later of checkpoint and rest point (X17). |
| CD-34 | Respawn grace i-frames (all damage) | 1.5 s | [estimated] | none | SYS2 C34 |  |
| CD-35 | Lethal-hit guard threshold (assist only) | 3 pips or more before the hit | [estimated] | DC 25% [sourced] of max (S14) | SYS2 C35 | Off by default |
| CD-36 | Lethal-hit guard recharge (assist only) | 45 s | [estimated] (basis: S14) | DC 45 [sourced] s (S14) | SYS2 C36 | Copies DC. CR34 RESOLVED (FD-8): retagged [estimated], DC value kept as basis. |
| CD-37 | I-frame flicker rate | 15 Hz | [estimated] | none | SYS2 C37 | Readable at 60 fps (every 4 [estimated] frames) |
| CD-38 | Low-health signal at | 1 pip | [estimated] (basis: S24) | HK 1 [sourced] (S24) | SYS2 C38 |  |
| CD-39 | Retracting spike warning window | 0.4 s | [estimated] | DC shows a warning shine (S19), unmeasured | SYS2 C39 |  |
| CD-40 | Grapple i-frames cover D-Hazard | yes | [estimated] | TM grapple i-frames (S4) | SYS2 C40 | Never D-Lethal (SYS1), D-Pit or kill plane |
| CD-41 | D-Pit damage | 2 pips | [estimated] | HK: damage + warp (S13); DC: 30 [sourced] base, capped at 30% [sourced] of max HP, + teleport (S14, S19) | SYS2 C41 | Decision 1 sets the behaviour; the amount is my choice: 20% of 10 pips, close to DC's 30% cap, below a spike's 3 **RESOLVED X3** (D2-1). |
| CD-42 | D-Pit warp fade | 0.4 s | [estimated] | Same as the warp fade in S1 row 51 | SYS2 C42 | Reuses the existing warp |

**Death states (SYS2 §9; replaces SYS1 single D-DEAD, X4 RESOLVED (user decision D2-3 + owning system))**
| State | Duration | Tag | Behaviour | Owner |
|---|---|---|---|---|
| D-1 Freeze | 0.15 s (CD-30) | [estimated] | Lethal hit read; SYS6 fixes the coin penalty here (X21) | SYS2, SYS6 |
| D-2 Animation | 0.8 s (CD-31) | [estimated] | Camera C-HOLD | SYS2, SYS5 |
| D-3 Fade | 0.4 s (CD-32) | [estimated] | Penalty applied, marker placed at last safe ground (SYS6 §7.3 rules) | SYS2, SYS6 |
| D-4 Respawn | load | n/a | Full HP (D2-3); at the later of checkpoint and rest point (X17); R0 cleared slots never reset (X14) | SYS2, SYS4, SYS5 |
| D-5 Grace | 1.5 s (CD-34) | [estimated] | I-frames against all damage | SYS2 |

**Damage classes (SYS2 §5; restated by SYS3 §9 and SYS6 §0)**
| Class | Pips | Tag | Notes |
|---|---|---|---|
| D-Light | 1 | [estimated] (basis: S6) | Enemy attacks, projectiles, contact |
| D-Heavy | 2 | [estimated] | HK 2 [sourced] for some late attacks (S26) |
| D-Hazard | 3 | [estimated] (basis: S6) | Hazards ignore i-frames; 0.5 [estimated] s re-hit lockout |
| D-Pit | 2 | [estimated] | Plus warp (D2-1) |
| D-Lethal | all | n/a | Kill volumes |
| Damage reduction | −1 pip on hits ≥ 2 | [estimated] (basis: S4) | Shop item (PE-S rows); minimum damage per hit 1 [estimated] (CD-23) |

