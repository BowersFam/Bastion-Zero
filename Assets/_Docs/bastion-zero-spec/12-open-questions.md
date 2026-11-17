# Bastion Zero spec — Open questions (ranked)

> Part of the Bastion Zero clean-room mechanics spec v3.2 (14 Nov 2026). Playtest and tuning questions with suggested defaults. Implement the default; do not block on these.
> Precedence: user decisions (file 01) > owning system > later system. Tags: [sourced], [community-measured], [estimated] (file 01).

---

## 4. Combined Open Questions (ranked)
De-duplicated across all six files. Ranks:
- **Blocker**: implementation of a core loop piece can't start without an answer;
- **High**: can start on the default, but the answer changes tuning across systems;
- **Medium**: local tuning or measurement;
- **Low / polish**: reference curiosity or post-v1.

"Suggested default" is a **Recommendation** from the consolidation; numbers in it are [estimated].

### 4.1 Blocker
System 3 decisions (a)–(e) (the former OQ-B1–OQ-B5) and DB-1 and DB-3 (the former OQ-B6–OQ-B7) were **decided (user, 14 Nov 2026)** and are removed (see §0.1). **No blockers remain.**


### 4.2 High
| ID | Question | Systems | Blocks | Suggested default | Merged from |
|---|---|---|---|---|---|
| OQ-H1 | What triggers the corridor-act opening event, and how is it shown? | SYS5, SYS6 | WP-OPENING, fast-travel switch-on, ST2, end of the corridor no-penalty rule | The defeat of the last corridor boss (the glide reward, PE-U3) | SYS5 OQ6; SYS6 OQ9 |
| OQ-H2 | Flask tuning: 3 [estimated] pips × 3 [estimated] charges restores 9 [estimated] between rest points | SYS4, SYS6 | Pip-loss budgets EN-Q3–EN-Q6, boss tuning (CR36) | 3 [estimated] pips per charge; fall back to 2 [estimated] if T3–T4 budgets are trivialised | SYS6 OQ7; CR36 |
| OQ-H3 | TM movement numbers are unmeasured (run, rise, height, gravity, fall cap, glide, grapple, coyote, buffer) | SYS1, SYS5 | Final feel; gate dimensions MV-D1–MV-D3 | Keep MV estimates; frame-track footage only if the user approves | SYS1 OQ1 |
| OQ-H4 | No published telegraph, detection or leash data for any reference | SYS4 | All EN timing rows | Keep estimates; tune in playtest | SYS4 OQ1 |
| OQ-H5 | Knockback per damage class: should D-Heavy knock further? | SYS1, SYS2, SYS3 | CF-H12, pit-edge reach rule EN-Q7 | Same distance (≈ 1.31 [estimated] tiles) | SYS3 OQ4 |
| OQ-H6 | No public income data, total currency or death frequency | SYS6 | PE-C/PE-E rows, marker-loss target | Keep estimates; log coin and deaths in playtests | SYS6 OQ1, OQ2, OQ3 |
| OQ-H10 | GT17 air-dash gap verification: does the 3-tile ceiling, 6–8-tile width (shape decided, FD-5) hold against the capped jump, glide, recovery jump and bounce anchors? | SYS1, SYS5 | WC-K19, every post-unlock gate | Verify on a blockout; widen within 6–8 tiles if a bypass is found | SYS5 OQ5, OQ8 |

### 4.3 Medium
| ID | Question | Systems | Blocks | Suggested default | Merged from |
|---|---|---|---|---|---|
| OQ-M1 | No public metrics for room sizes, degrees, loops, gate spacing | SYS5 | WC-G/WC-Z targets | Check on a blockout graph | SYS5 OQ1 |
| OQ-M2 | HK room count: 307 [community-measured] vs 459 [community-measured] | SYS5 | WC-G1 reference only | Keep 180–260 [estimated] (decided) | SYS5 OQ2 |
| OQ-M3 | Look-ahead tuning W4–W5 with bounce chains and grapple | SYS5 | WC-W4, WC-W5 | 3 / 4 [estimated] tiles | SYS5 OQ7 |
| OQ-M4 | Block and guard tuning: 4 [estimated] guard points, regeneration, 0.8 [estimated] s break, 2.0 [estimated] s guard-pressure window | SYS3, SYS4 | CF-BK8–CF-BK10, EN-Q8 | Keep; playtest against SYS4 pressure tables | SYS3 OQ8; SYS4 OQ2 (guard part) |
| OQ-M5 | Hit-stop and shake values unpublished except DC's crit freeze | SYS3 | CF-H rows | Keep estimates | SYS3 OQ3 |
| OQ-M6 | M6 clone tuning (0.6 [estimated] s gap, 10% [estimated] HP premium) | SYS4 | EN-M6 | Keep | SYS4 OQ5 |
| OQ-M7 | R3 off-screen respawn feel | SYS4 | EN-RS1 R3 | Keep R3 for anchors only (decided f); playtest | SYS4 OQ4 |
| OQ-M8 | Tier pacing by coin with no materials | SYS6 | PE-S rows | Stock tiers cap it; playtest | SYS6 OQ8 |
| OQ-M9 | Wall-cling trigger (automatic vs press-toward) and whether cling refreshes the stored air-jump | SYS1 | W-CLING entry; bounce chain rules | Accept either input; cling ends the chain | SYS1 OQ2, OQ3 |
| OQ-M10 | TM post-hit and grapple i-frame lengths; whether they block hazards | SYS1, SYS2 | Reference only (CD-24 set) | Keep 1.0 [estimated] s | SYS1 OQ4; SYS2 OQ2 |
| OQ-M11 | Pits in boss arenas and challenge rooms: same rule or lethal? | SYS2, SYS4, SYS5 | Arena design | Same damage + warp rule everywhere (CR35) | SYS2 OQ9 |
| OQ-M12 | Pit damage amount 2 [estimated] pips | SYS2 | CD-41 | Keep | SYS2 OQ8 |
| OQ-M13 | Portable warp point | SYS6 | PE-V rows | None in v1 | SYS6 OQ5 |
| OQ-M14 | Can chip damage take the last pip? | SYS2, SYS3 | CF-BK6 | No: chip stops at 1 pip | SYS3 OQ9 |
| OQ-M15 | Parry tuning: 8 [estimated] f window, 25 / 50 [estimated] poise, 0.6 [estimated] s tap lockout, and the length of the 3.0 [estimated] s parry-stagger cap (the cap itself is decided, FD-3); do parry chains trivialise soldiers (25 poise = their threshold)? | SYS3, SYS4 | CF-BK12–CF-BK18, EN-K4 | Keep all values; playtest | SYS3 OQ10; SYS4 OQ2 (parry part) |
| OQ-M16 | Dash i-frame abuse and the dash-through limit: ≈ 22% [estimated] invulnerable uptime when spammed; the soldier strike passes with only 1 [estimated] f of margin | SYS3, SYS4 | CF-DA8, EN-K3 | Keep the 0.40 s cooldown and the ≤ 6 f limit; playtest dense rooms | SYS3 OQ11; SYS4 OQ2 (dash part) |
| OQ-M17 | Air-dash timing (5.5–7.0 [estimated] h; source decided, FD-6): enough GT17 revisits left afterwards? | SYS5, SYS6 | PE-U11, WC-K21 | Keep the timing and playtest | SYS6 OQ10 |

### 4.4 Low / polish
| ID | Question | Systems | Blocks | Suggested default | Merged from |
|---|---|---|---|---|---|
| OQ-L1 | Coin after the catalogue: cosmetic sink or nothing? | SYS6 | Late economy | Nothing in v1 | SYS6 OQ4 |
| OQ-L2 | Permadeath or hard mode | SYS6 | Nothing in v1 | Out of scope (PE11) | SYS6 OQ6 |
| OQ-L3 | Slopes in TM and DC | SYS2 | Slopes are not in v1 | None | SYS2 OQ1 |
| OQ-L4 | TM knockback distance and direction | SYS2 | Reference only | Deterministic Bastion Zero rule | SYS2 OQ3 |
| OQ-L5 | HK hurtbox vs sprite and hit recoil | SYS2 | Reference only (excluded sources) | Keep CD estimates | SYS2 OQ4 |
| OQ-L6 | DC contact damage and post-hit i-frames (community-backed only) | SYS2 | Reference only | Keep | SYS2 OQ5 |
| OQ-L7 | DC health tables (provenance unclear) | SYS2 | Reference only | Shape only | SYS2 OQ6 |
| OQ-L8 | HK moving-platform rules | SYS2 | Reference only | Keep CD-12–CD-13 | SYS2 OQ7 |
| OQ-L9 | TM bounce from hazards; fast-fall; apex behaviour; frame rate | SYS1 | Reference only | Off; absent; hang; not used | SYS1 OQ5–OQ8 |
| OQ-L10 | HK dash distance and duration (excluded sources only) and wiki values that may come from data | SYS1, SYS3 | Reference for CF-DA3–CF-DA4 | Keep estimates; drop if the clean-room line is strict | SYS1 OQ10, OQ11; SYS3 OQ2 |
| OQ-L11 | DC per-hit timings, crit range, interrupt-stun meter (provenance unclear) | SYS3 | Reference only | Keep estimates | SYS3 OQ1, OQ5, OQ6 |
| OQ-L12 | HK boss thresholds and stagger counts (provenance unclear) | SYS4 | Reference only | Ratio shapes only | SYS4 OQ3 |
| OQ-L13 | HK breakable-wall hit counts; TM and DC transition and camera timings | SYS5 | Reference only | 1–3 [estimated] hits; keep WC-T rows | SYS5 OQ3, OQ4 |
| OQ-L14 | DC roll i-frame window and cooldown (0.37 [sourced] s vs 0.75 [community-measured] s start to start) | SYS1 | Reference only: the roll is declined (decisions b–c) | None | SYS1 OQ9 |

**Resolved and removed from the list:** SYS3 (a)–(e) (former OQ-B1–OQ-B5; CR26 and X11 resolved); SYS3 DB-1–DB-4 (former OQ-B6, OQ-B7, OQ-H8, OQ-H9; X24, X26 updated, X27–X28 added; GT10 removed); FD-1–FD-10 (former OQ-M18, OQ-L15, OQ-L16 and the decision parts of OQ-H10, OQ-M15, OQ-M17; CR34 resolved, X29 added; the three consolidation questions below); decision (g) confirmed (former OQ-H7, SYS4 OQ6; CR37 resolved); the former OQ-M4 (DC roll reference, now OQ-L14); SYS3 OQ7 (cooldown reduction: excluded by decision w); SYS2 "resolved by the user" list (D2-1–D2-7); SYS5 (l)–(q); SYS6 (r)–(z), including the TM follower drain rate (no longer needed).

**Count check:** the six files hold 54 numbered questions (SYS1 11, SYS2 9, SYS3 11 including the resolved OQ7, SYS4 5, SYS5 8, SYS6 10) and no open user decisions (all six were decided as FD-1–FD-6). Merges: SYS5 OQ5 + OQ8 → OQ-H10; SYS3 OQ10 + SYS4 OQ2 (parry) → OQ-M15; SYS3 OQ11 + SYS4 OQ2 (dash) → OQ-M16; SYS6 OQ10 → OQ-M17; SYS3 OQ8 + SYS4 OQ2 (guard) → OQ-M4; SYS3 OQ2 + SYS1 OQ10–OQ11 → OQ-L10; SYS1 OQ4 + SYS2 OQ2; SYS5 OQ6 + SYS6 OQ9; SYS6 OQ1–OQ3; SYS1 OQ5–OQ8; SYS3 OQ1, OQ5, OQ6; SYS5 OQ3–OQ4. Result: 38 entries (0 Blocker, 7 High, 17 Medium, 14 Low), plus SYS3 OQ7 resolved. Retired IDs (OQ-B6, OQ-B7, OQ-H7, OQ-H8, OQ-H9, OQ-M18, OQ-L15, OQ-L16) are not reused.

---

## Open Questions
The combined, ranked list is §4. The three questions about this consolidation itself are **resolved (user, 14 Nov 2026)**:
1. ~~CR34 tag policy~~: copied defaults are tagged [estimated] with the reference as basis (FD-8). SYS1 keeps its old tags (X29).
2. ~~Slice world phase~~: the death marker is on in the slice, with the WP-OPEN flag (FD-9).
3. ~~Slice elite base~~: the elite is built on a fodder enemy (FD-10).

No open consolidation questions or user decisions remain. Open items are the playtest and measurement questions in §4 (0 blockers).

