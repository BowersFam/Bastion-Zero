# Bastion Zero spec — Numeric table: Movement (SYS1)

> Part of the Bastion Zero clean-room mechanics spec v3.2 (14 Nov 2026). Run, jump, wall, glide, grapple, buffers and timers. Rule text lives in system-1-player-movement.md (NOT in this package; see 00-handoff-prompt.md).
> Rule text owner: SYS1 (file not included).
> Precedence: user decisions (file 01) > owning system > later system. Tags: [sourced], [community-measured], [estimated] (file 01).

---

### 3.1 Movement (SYS1 owns; SYS3 dash in §3.3)
| ID | Parameter | Value | Tag | Source | Owner | Notes |
|---|---|---|---|---|---|---|
| MV-01 | Run speed | 8.5 tiles/s | [estimated] | 3.32–5.3 H/s = 6.6–10.6 tiles/s [community-measured] (S10) | SYS1 row 1 | Mid-high in the range; TM reads fast and "frictionless" (S2) |
| MV-02 | Ground accel time to full speed | 0.05 s | [estimated] | 0–0.33 s [community-measured] (S10) | SYS1 row 2 | Precision platformer end of the range |
| MV-03 | Ground stop time | 0.04 s | [estimated] | 1 frame–0.13 s [community-measured] (S10) | SYS1 row 3 | Near-instant like HK; slight ease avoids jitter |
| MV-04 | Turnaround time | 0.05 s | [estimated] | 0.05–0.13 s [community-measured] (S10) | SYS1 row 4 | Matches HK |
| MV-05 | Air accel multiplier | 0.85 × ground | [estimated] | 0.65× [community-measured] (Celeste) to total control (HK) (S10) | SYS1 row 5 | Bounce chains need fine mid-air correction |
| MV-06 | Air turn multiplier | 1.0 × ground turn | [estimated] | Reverse in ≤ 0.3 s recommended; HK ~0.05 s [community-measured] (S10) | SYS1 row 6 | Never punish a reversal in the air |
| MV-07 | Held-rise speed v | 13.8 tiles/s | [estimated] | 6.1–9.1 H/s = 12.2–18.2 tiles/s [community-measured] (S10) | SYS1 row 7 | Solved for h = 4 tiles, T = 0.40 s |
| MV-08 | Max hold time t_hold | 0.18 s | [estimated] | 0.17–0.22 s [community-measured] (S10) | SYS1 row 8 | Middle of range |
| MV-09 | Gravity g (rise and fall) | 62.7 tiles/s² | [estimated] | HK ≈ 35 H/s² = 70 tiles/s² [community-measured] (S10) | SYS1 row 9 | From g = v²/(2(h − v·t)) |
| MV-10 | Full jump height | 4.0 tiles (2.0 H) | [estimated] | 1.63–2.95 H [community-measured] (S10) | SYS1 row 10 | Leaves headroom for chained air-jumps in 11–12-tile-tall screens. CR24 RESOLVED (owning system): the viewport is 13.5 [estimated] tiles tall (WC-W1, SYS5); jump height unchanged. |
| MV-11 | Time to apex | 0.40 s | [estimated] | 0.34–0.46 s [community-measured] (S10) | SYS1 row 11 | Middle of range |
| MV-12 | Release cut c | 0.5 × vy | [estimated] | Shortest hop 0.25–0.5 of full is the target range (S10 recommendation) | SYS1 row 12 | A 0.05 s tap gives about 1.07 tiles ≈ 0.27 of full |
| MV-13 | Minimum hold before release counts | 0.05 s | [estimated] | none published | SYS1 row 13 | Guarantees a readable minimum hop |
| MV-14 | Apex band v_hang | 2.0 tiles/s | [estimated] | Hang 0.16–0.25 s [community-measured] (S10) | SYS1 row 14 | Targets about 0.2 s of hang with row 15 |
| MV-15 | Apex gravity multiplier k_hang | 0.5 × g | [estimated] | Measured games show a hang but no published multiplier (S10) | SYS1 row 15 | TM "the game takes over at the top" (S3) |
| MV-16 | Fall gravity multiplier | 1.0 × g | [estimated] | Fall/rise ratio 0.9–1.0 [community-measured] (S10) | SYS1 row 16 | Feel comes from the held rise, not a heavy fall |
| MV-17 | Fall cap | 19 tiles/s (9.5 H/s) | [estimated] | 9.3–11.1 H/s [community-measured]; 1.2–1.5× rise [community-measured] (S10) | SYS1 row 17 | 1.38× rise speed |
| MV-18 | Fast-fall cap (optional) | 26 tiles/s | [estimated] | none for TM; DC dive exists, unmeasured (S14) | SYS1 row 18 | About 1.4× normal cap; off by default |
| MV-19 | Coyote time | 0.10 s | [estimated] | TM/HK/DC not published (HK value excluded on clean-room grounds) | SYS1 row 19 | Common modern practice; verify in playtest |
| MV-20 | Jump buffer | 0.10 s | [estimated] | not published | SYS1 row 20 | Same |
| MV-21 | Head-bump coyote suppression | 0.06 s | [estimated] | none | SYS1 row 21 | Stops ceiling-clip free jumps |
| MV-22 | Stored air-jumps max | 1 count | [estimated] | TM grants "an additional jump" (S4); a cap of 1 is inferred from the same-target technique (S5) | SYS1 row 22 | Matches TM |
| MV-23 | Air-jump impulse | same as rows 7–8 | [estimated] | not published | SYS1 row 23 | Predictable arcs for level design |
| MV-24 | Bounce-chain limit | none | [sourced] behaviour, no number (S2, S4) | none | SYS1 row 24 | TM rule |
| MV-25 | Same-target regrant | once per swing | [estimated] | S5 shows the same target can grant twice | SYS1 row 25 | Prevents multi-frame double grants |
| MV-26 | Hit-pause on airborne qualifying hit | 0.05 s | [estimated] | none for TM | SYS1 row 26 | Short enough not to break rhythm |
| MV-27 | Air stall on hit | 0.06 s, vy held at ≥ 0 | [estimated] | DC has an air stall, unmeasured (S14) | SYS1 row 27 | Helps chain timing; tunable to 0 |
| MV-28 | Attack active window (hitbox) | 0.10 s | [estimated] | TM "a few frames", moves with the hero (S6) | SYS1 row 28 | Combat system owns the final value |
| MV-29 | Glide fall speed | 2.5 tiles/s | [estimated] | not published | SYS1 row 29 | ~13% of fall cap; slow enough to cross a 10-tile gap from 2 tiles up |
| MV-30 | Glide horizontal speed | 1.0 × run speed | [estimated] | not published | SYS1 row 30 | Keeps air control consistent |
| MV-31 | Glide duration limit | none | [sourced] behaviour (S2) | none | SYS1 row 31 | TM rule |
| MV-32 | Quick-glide (flip) slow-fall | 0.15 s at glide fall speed | [estimated] | Behaviour exists (S5), length unmeasured | SYS1 row 32 |  |
| MV-33 | Updraft lift speed | 6 tiles/s up | [estimated] | Behaviour exists (S4, S9), unmeasured | SYS1 row 33 | Clearly faster than the glide fall |
| MV-34 | Grapple max range | 7 tiles (horizontal) | [estimated] | Horizontal hook (S1); range not published | SYS1 row 34 | Less than half of a 24-tile-wide screen |
| MV-35 | Grapple pull speed | 30 tiles/s | [estimated] | not published | SYS1 row 35 | About 3.5× run speed so it reads as a burst |
| MV-36 | Grapple carry-through exit speed | 1.5 × run speed, decays to run cap | [estimated] | Behaviour (S4, S6) | SYS1 row 36 |  |
| MV-37 | Grapple bounce-back (no input) | 1.0 tile | [estimated] | "small distance" (S4) | SYS1 row 37 |  |
| MV-38 | Grapple i-frames | whole pull + 0.1 s | [estimated] | Behaviour (S4) | SYS1 row 38 | Never covers instant-death volumes |
| MV-39 | Wall climb speed | 4 tiles/s | [estimated] | Unlimited climbing (S4, S7); speed not published | SYS1 row 39 | Slower than a wall-jump climb, matching S6 |
| MV-40 | Wall slide speed (if slide mode) | 4 tiles/s | [estimated] | HK slides; speed unmeasured (S11) | SYS1 row 40 | Default mode is hold, not slide |
| MV-41 | Wall-jump horizontal push | 6 tiles/s | [estimated] | not published | SYS1 row 41 | Decays into normal air control |
| MV-42 | Re-cling lockout after wall-jump | 0.12 s | [estimated] | none | SYS1 row 42 | Stops instant re-stick |
| MV-43 | Hold-away release delay | 0.08 s | [estimated] | none | SYS1 row 43 | Fixes eager-cling complaint (S4) |
| MV-44 | Wall stamina | none | [sourced] behaviour (S4, S7, S11) | none | SYS1 row 44 |  |

**Movement derived reach (used for gates; owner SYS5 §5.1)**
| ID | Parameter | Value | Tag | Source | Owner | Notes |
|---|---|---|---|---|---|---|
| MV-D1 | Reachable ledge height, base kit | ≤ 3.5 tiles (full jump 4.0 minus margin) | [estimated] | MV-10 | SYS5 §5.1 | Wall-cling: unlimited. Bounce air-jump: about 7.5 tiles if an anchor is in reach. |
| MV-D2 | Crossable flat gap, base kit | ≤ 6 tiles (running jump about 6.4–7); a ledge dash adds nothing (no jump may follow it). After the air-dash unlock: ≈ 10–10.5 tiles | [estimated] | MV-01, MV-11, CF-DA4, CF-DA11 | SYS5 §5.1 | DB-3. Glide about 3.4 tiles across per tile dropped; grapple up to 7 tiles to a hook; bounce chains unlimited while targets last (MV-24) |
| MV-D3 | Hard gate thresholds | Ledges ≥ 5 tiles with no anchor within 6 tiles; gaps ≥ 9 tiles with no anchors or hooks for gates whose key comes before the air-dash unlock; ≥ 12 tiles for gates whose key comes after it | [estimated] | MV-D1, MV-D2 | SYS5 §5.1, §5.3 rule 7 | **Decided (user, 14 Nov 2026)**, DB-3. A gate only has to hold against abilities owned before its key. |

