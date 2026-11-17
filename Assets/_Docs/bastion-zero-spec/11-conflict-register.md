# Bastion Zero spec — Conflict register

> Part of the Bastion Zero clean-room mechanics spec v3.2 (14 Nov 2026). Every cross-system disagreement and how it was resolved. If two files disagree, this decides.
> Precedence: user decisions (file 01) > owning system > later system. Tags: [sourced], [community-measured], [estimated] (file 01).

---

### 3.7 Conflict register
Rule: user decisions win; otherwise the later or owning system governs. **RESOLVED (rule)** names the rule used; **FLAGGED** means the consolidation can't settle it without the user or a playtest.

| Conflict | Systems | Resolution | Reason | Status |
|---|---|---|---|---|
| X1 Max HP 8 vs 10 | SYS1 row 45; SYS2 C18; SYS3 §12 | 10 [estimated] pips (CD-18); SYS1 max 8 [sourced] superseded | User decision D2-4 | RESOLVED (user decision) |
| X2 Hazard re-hit lockout | SYS1 §6 rule 2; SYS2 C25 | Hazards ignore i-frames, plus 0.5 [estimated] s same-instance lockout | User decision D2-2 | RESOLVED (user decision) |
| X3 Bottomless pits | SYS1 §6 rule 3; SYS2 C41 | D-Pit 2 [estimated] pips + warp; kill plane warps | User decision D2-1 | RESOLVED (user decision) |
| X4 Death state | SYS1 D-DEAD; SYS2 §9 | Sub-states D-1–D-5, full-HP respawn | User decision D2-3; SYS2 owns death | RESOLVED (user decision + owning system) |
| X5 Warp arrival i-frames | SYS1 H-WARP; SYS2 C29 | Arrival i-frames 1.0 [estimated] s cover hazards too | SYS2 owns damage | RESOLVED (owning system) |
| X6 Knockback exits | SYS1; SYS2 §7.3, §7.6 | H-KNOCK → W-CLING / A-GLIDE blocked until knockback ends or is cancelled | SYS2 owns knockback rules | RESOLVED (owning system) |
| X7 Bounce targets and same-frame order | SYS1 §3; SYS2 §1, §6, §10 | Falling ceiling hazards qualify; damage beats grant; kill removes contact first | SYS2 later and owns collision order | RESOLVED (later/owning system) |
| X8 Drop-through timers | SYS1; SYS2 §2.5 | Drop-through clears coyote and jump buffer | SYS2 owns one-way platforms | RESOLVED (owning system) |
| X9 Defensive move | SYS1 §7; SYS3 §5 | C-DASH (CF-DA1–CF-DA10) + C-BLOCK (CF-BK1–CF-BK12); no roll | User decisions (b)–(c) | RESOLVED (user decision) |
| X10 Attack window | SYS1 row 28; SYS3 §3 | 6 f = 0.10 [estimated] s; consistent | Values agree | RESOLVED (consistent) |
| X11 Air-dash refresh (was air-roll refresh) | SYS1 §3, §7; SYS3 §5.2 | The air dash refreshes on landing, W-CLING/W-JUMP or a bounce grant (CF-DA7) | User decision (c) replaced the air roll with the dash; the refresh rule follows SYS1 §7's dash takeaway | RESOLVED (user decision + consistent with SYS1) |
| X12 Shield hits as bounce targets | SYS1 §3; SYS4 §8 | Blocked hits never qualify; no mismatch remains | User decision (j) declined X12 | RESOLVED (user decision) |
| X13 Non-damaging push | SYS1 H-KNOCK; SYS4 §9–§10 | External force field; no H-KNOCK, no i-frames | SYS4 owns elite and boss pulses | RESOLVED (owning system) |
| X14 Death reset scope | SYS2 D-4; SYS4 §11 | R0 cleared slots never reset; uncleared arenas reset | SYS4 later, refines | RESOLVED (later system) |
| X15 Timers across transitions | SYS1; SYS5 §9.2 | Frozen during transition, resumed after; stored air-jump kept | SYS5 owns transitions | RESOLVED (owning system) |
| X16 Camera look-ahead | SYS1 dependency note; SYS5 W4–W5 | Small dual-forward look-ahead, 3–4 [estimated] tiles | User decision (n) | RESOLVED (user decision) |
| X17 Rest point vs checkpoint | SYS2 D-4; SYS5 §12.1; SYS6 §6.2 | Two save-point kinds; respawn at the later of the two | SYS5 later, owns save points; SYS6 extends | RESOLVED (later/owning system) |
| X18 Transition lift | SYS1; SYS5 §9.1 | Small lift on upward exits | SYS5 owns transitions | RESOLVED (owning system) |
| X19 Death penalty | SYS1 row 53; SYS6 §7 | P-C half-coin marker (PE-D1); 60 [sourced] s follower not used | User decision (s) | RESOLVED (user decision) |
| X20 Weapon tier acquisition | SYS3 §9; SYS6 §5.3 | T2–T5 bought for a coin fee only; multipliers unchanged | User decision (v) | RESOLVED (user decision) |
| X21 Marker timing and placement | SYS2 D-3; SYS6 §7.3 | Penalty fixed at D-1, applied at D-3; arena marker before B-GATE; never across a one-way link | SYS6 later, owns death penalty | RESOLVED (later/owning system) |
| X22 Max-health growth path | SYS2 C17–C18; SYS6 §4.1 | 3 [estimated] fragment sets + 2 [estimated] shop pips | SYS6 fills a gap it owns | RESOLVED (owning system) |
| X23 Flask vs E8/E14 | SYS3 E8, E14; SYS2 §5 rule 5; SYS6 §8 | Fixed-charge flask 1→3 [estimated] charges, 3 [estimated] pips each; rally-style recovery and food stay excluded | User decision (u) | RESOLVED (user decision) |
| X24 Dash availability | SYS1 §7 and dependencies (dash as an exploration-gating unlock flag); SYS3 §5.2 | Ground dash is base kit; the air dash is the unlock behind SYS1's dash flag (CF-DA1, PE-U11) | User decisions (b)–(c), DB-3; the remaining mismatch is the ground dash only | RESOLVED (user decision) |
| X25 Block vs damage floor | SYS2 C23 (min 1 [estimated] pip); SYS3 §5.3 | A blocked D-Light deals 0 pips; blocked D-Heavy chips 1 [estimated]; floor applies everywhere else | SYS3 later, owns the block (decision b) | RESOLVED (later/owning system) |
| X26 Dash hurtbox | SYS2 §12 (a dash or roll would use a 1-tile hurtbox); SYS3 §5.2 | Standing hurtbox (CF-DA9); 1-tile tunnels dropped | User decision DB-4 | RESOLVED (user decision) |
| X27 Dash i-frame flag | SYS2 §8 rule 7 (separate flag, default off); SYS1 §7 ("dash without i-frames"); SYS3 §5.2 | Flag on: f3–f10, enemy sources only; hazards still ignore it | User decision DB-1 | RESOLVED (user decision) |
| X28 Coyote time after a ledge dash | SYS1 row 19 / MV-19 (coyote on leaving any ledge); SYS3 §5.2 | No coyote time and no buffered jump after a ledge dash (CF-DA11) | User decision DB-3 | RESOLVED (user decision) |
| X29 Tags on copied defaults | SYS1 rows 45–47 and 53 (start 5, spike 3, contact 1, penalty 60 s, all [sourced]); SYS2 C17, C21, C19; SYS6 PE-D0 | Same values tagged [estimated] with the reference as basis (CD-S45–CD-S47, CD-17, CD-19, CD-21, PE-D0); SYS1 keeps its old tags | User decision FD-8; SYS1 not edited | RESOLVED (user decision) |
| CR24 Screen height | SYS1 row 10 note (11–12 tiles tall); SYS5 W1 (13.5 [estimated]) | 24 × 13.5 [estimated] tiles | SYS5 owns the camera | RESOLVED (owning system) |
| CR25 Block floor (was shield block floor) | SYS2 C23 (min 1 [estimated] pip); SYS3 §5.3 | Superseded by X25: the core block reduces D-Light to 0 [estimated] | SYS3 later, owns the block (decision b) | RESOLVED (later/owning system; see X25) |
| CR26 Defensive moves vs traps (was roll vs traps) | SYS2 §8 rule 2; SYS3 §10; SYS4 M2/M3; SYS5 GT11 | Hazards ignore all invulnerability; neither the dash nor the block avoids them | User decision (a) | RESOLVED (user decision) |
| CR27 Dash unlock flag | SYS1 dependencies list a dash flag; SYS3 §5.2; SYS6 §3.2 | The flag now gates the air dash (PE-U11); the ground dash needs no flag (X24) | User decisions (b)–(c), DB-3 | RESOLVED (user decision) |
| CR28 Rest spacing | SYS5 §12.3 (6–10 [estimated] rooms); SYS4 §13 rule 6 (≤ 4 [estimated] combat rooms or 30 [estimated] points) | Both apply; whichever comes first | SYS5 owns placement; SYS4 cap is compatible | RESOLVED (owning system, compatible) |
| CR29 Checkpoint save content | SYS5 §12.1 (position only); SYS6 §6.2 (position, wallet, flags) | Position, wallet and P0 flags | SYS6 later, owns saving | RESOLVED (later/owning system) |
| CR30 Fast-travel node count | SYS5 §12.4 (1 per region); SYS6 §9.3 (8–14 [estimated]) | 1 per region + 0–2 [estimated] in hub regions | SYS6 later, owns fast travel | RESOLVED (later/owning system) |
| CR31 Loadout swap location | SYS3 §2 (checkpoints); SYS6 §6.2 (rest points and checkpoints) | Swap at both | SYS6 later; superset | RESOLVED (later system) |
| CR32 Reload behaviour | SYS4 §11 event table (no reload row); SYS6 §6.3 rule 3 | Reload = soft death without penalty: later of checkpoint and rest point, full health, R1 respawned, no marker | SYS6 later, fills gap | RESOLVED (later system) |
| CR33 Boss respawn point | SYS4 §13 rule 8 / SYS6 §0 ("checkpoint ≤ 1 room"); SYS5 §12.3 ("rest point ≤ 1 room") | A rest point ≤ 1 [estimated] room before B-GATE (it also sets respawn) | SYS5 owns save-point placement | RESOLVED (owning system) |
| CR34 [sourced] tags on Bastion Zero defaults copied from references | SYS1 rows 45–47, 53; SYS2 C17, C19, C21, C22, C36, C38; also SYS3–SYS6 restatements | Copied defaults retagged [estimated] with the reference as basis in SYS2–SYS6 and here; SYS1 untouched (X29) | User decision FD-8 | RESOLVED (user decision) |
| CR35 Pits outside exploration areas | SYS2 OQ9; SYS4 §15 (arena pushes → D-Pit + warp); SYS5 §9.4 rule 6 (arena safe points) | Same damage + warp rule everywhere | Later systems already apply SYS2's default | RESOLVED (later system); question kept as OQ-M11 |
| CR36 Flask vs pip budgets | SYS4 §13 (budgets assume no healing); SYS6 §8 rule 3 | Boss budgets may rise ≤ 3 [estimated] pips per charge carried | Needs playtest | **FLAGGED** (OQ-H2) |
| CR37 Attack-sign default | SYS4 decision (g) | On by default | User confirmed decision (g) | RESOLVED (user decision) |
| CR38 HK i-frame provenance wording | SYS1 row 48 ("provenance unverified"); SYS2 C24 ("provenance unclear") | Treated as provenance unclear | Same value, same source (S11) | RESOLVED (later system wording) |
| CR39 Ability cadence | SYS5 §12.4 (1 per 60–100 [estimated] min); SYS6 §3.2 (about 1 per 56 [estimated] min with the air dash) | SYS6 template accepted | SYS6 later, owns unlocks; difference explained there | RESOLVED (later/owning system) |
| CR40 Traversal abilities vs techniques | SYS5 §12.4 (7 [estimated] traversal abilities, air dash added); SYS6 §3.1 (projectile bounce and recovery jump bought as techniques) | Seven flags stand; two are shop techniques | SYS6 later, owns acquisition | RESOLVED (later/owning system) |

**Totals:** 46 rows; X1–X29 all present; 45 RESOLVED, 1 FLAGGED (CR36, a playtest check, not an open decision). Version 2 resolved X11 and CR26 (decisions a and c) and added X24–X26; Version 3 added X27–X28 and updated X24, X26, CR27, CR39 and CR40 (DB-1–DB-4); Version 3.1 resolved CR34 and added X29 (FD-8); Version 3.2 resolved CR37 (decision g confirmed).

---

