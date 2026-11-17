# Nexus — HD-2D Design Contract

The single art source of truth for the hero sprite pass. Every generated pose grid is
prompted from this file. If a pose deviates, the grid is regenerated — the contract does
not change to match the art.

Reference sheets:
- `nexus_archon_reference.png` — 1024×1536, full body, standing, side profile facing right, no wings.
- `nexus_wing_reference.png` — 1536×1024, wing pair only, spread, no body.

## Silhouette
- Height **1.797 u** (7.5 heads at 0.24 u per head) measured at 128 px/unit = **230 px**.
- Shoulder width ≈ **0.37 × height** — narrow. No chibi, no squat proportions.
- Side profile, facing **right**, at all times. Never front-facing in a gameplay pose.
- Feet planted on the frame baseline; nothing below it.

## Materials
| Role | Value |
|---|---|
| Plate core | `#17171a` |
| Plate mid | `#3a3b40` |
| Plate highlight | `#6f7076` |
| Amber core | `#f0a020` |
| Amber shadow | `#8a5210` |
| Amber highlight | `#ffcf5a` |
| Silver accent | `#c8cbd2` |

- Amber is **pinstripe only** — thin edge lines along plate seams, pauldron rims, belt, boot caps.
  Roughly **10 %** of figure area. Never broad filled slabs, never a lemon-yellow fill.
- Every plate carries a hard interior shadow line. No plate may merge into another plate.
- Rim light from the upper left, warm, one pixel wide on silhouette edges.

## Features
- **Helm:** smooth rounded closed dome, one small round finial on the crown. **No horns, no crest,
  no grille.** A thin amber X-shaped visor slit.
- **Chest:** one small round amber ring emblem, centred.
- **Cape:** hangs from the shoulders to below the knee, scalloped torn hem with a thin amber edge line.
- **Sword:** slender blade, ornate gothic crossguard, long narrow amber-gold blade, held low in the
  right hand with the tip angled down and forward. Never a blunt gold bar, never over the shoulder.
- **Wings** (wing layer only): black feathered vanes, layered, lower third of each long primary fades
  into amber-orange through a **crisp band**, not a gradient.

## Rendering rules
Hard-edged pixels. No anti-aliasing, no blur, no soft gradients inside a pixel, no dithering noise,
no outline halo. Fully transparent background: no ground, floor, cast shadow, border, frame, grid
line, text, watermark or vignette.

## Frame layout contract
The art model must keep each pose at a fixed size inside its grid cell, because the slicer uses
**one scale and one offset for the whole grid — it never normalises per frame**. Per-frame
normalisation is what makes a character bob and change size between frames, and it is the single
biggest source of the stutter this pass exists to remove.

**Prompt rule that follows from it:** state the physical size in pixels ("soles to helmet finial is
480 px, the same in every cell") and tell the model to crop the cape rather than shrink the figure.
Asking for "keep the figure inside the cell" alone produces a figure at roughly half scale.

### Body set — cell counts depend on the frame budget
| Frames | Columns × rows | Cell | Grid |
|---|---|---|---|
| 8 (idle, walk, run) | 4 × 2 | 384×512 | 1536×1024 |
| 6 (jump, attack_g1, attack_g2, attack_g3) | 3 × 2 | 512×512 | 1536×1024 |
| 4 (fall) | 2 × 2 | 512×512 | 1024×1024 |

- Reading order is left to right, top row first; frame `00` is the **top-left** cell.
- The soles of every pose sit on or just above the bottom of its cell, the helm crown near the top.
- Slice mapping, measured **once per grid**: the union bounding box of all cells sets one scale,
  `scale = 230 / unionHeight`, and the union's lowest row lands on frame y **14** (world −0.891 u,
  GroundCheck y −0.906). Horizontally each cell's own centre line lands on frame x **128**.
- Union height therefore has to be the *standing* figure, ~486 px. A state whose tallest pose is
  taller (a raised sword) must be sliced with the shared `ReferenceSpan`, or the body shrinks.
- Output: 256×256 px, PPU 128, `Point`, uncompressed, `FullRect`, centre pivot.

### Wing set — grid 1024×1536, 2 columns × 4 rows, cells 512×384
- Reading order: `00 01`, then `02 03`, then `04 05`, then `06 07` top to bottom.
- Wings only, no body; left and right vane joined on the cell's vertical centre line.
- Slice mapping, measured once per grid: `scale = 358 / unionWidth`, union centre → frame
  **(256, 346)** on a 512×512 canvas, i.e. +0.70 u above the hero origin, so a 2 u tall pair spans
  roughly −0.3…+1.7 u behind the shoulders.
- All three wing states share `ReferenceSpan = 503 px` (the flap grid's own spread) so the wings
  never change size between idle, flap and glide.
- Output: 512×512 px, PPU 128, same import settings.

### Cross-state scale
Every body state is sliced with **`ReferenceSpan = 486 px`**, the idle grid's measured height, so the
body states render at one identical scale (0.4733). A state that measures its own span silently
changes the hero's size when that state plays — that is the second stutter source.

**No exceptions.** `attack_g3` used to measure its own grid because the art model drew it ~8.8 %
short; the regenerated G3 sheet measures 482 px against G1's 484 px, so it slices at the shared 486
like everything else. Any future grid that measures far from 486 is a generation problem to be fixed
in the sheet, not a reason to give that state its own span.

## Measured results (v1)
| State | Union (cell px) | Scale |
|---|---|---|
| standing | 291×489 | 0.4733 |
| walking | 286×494 | 0.4733 |
| running | 355×428 | 0.4733 |
| jumping | 375×481 | 0.4733 |
| falling | 465×498 | 0.4733 |
| attack_g1 | 429×484 | 0.4733 |
| attack_g2 | 443×447 | 0.4733 |
| attack_g3 | 484×482 | 0.4733 |
| wings idle | 470×326 | 0.7117 |
| wings flap | 503×343 | 0.7117 |
| wings glide | 474×275 | 0.7117 |

## Head correction (applied mid-pass)
The first generated anchor drifted back toward the old horned, faceted helm with a gold grille. The
user's correction — *"rounded like an astronaut helmet"* — is now a **mandatory block in every grid
prompt** and outranks any other helm description:

> a smooth rounded astronaut-helmet dome, one single glossy sphere, a small round ball finial on top,
> and a thin amber X-shaped visor across the front. No horns, no crest, no ridges, no facets, no gold
> grille, no eye holes. The dome is the character's whole head.

Any regenerated grid that shows horns, a crest, a faceted crown or a gold grille is wrong and must be
regenerated — the dome is the character's silhouette signature.

## Frame budget
| State | Frames | Clip length |
|---|---|---|
| idle | 8 | 0.6667 s |
| walk | 8 | 0.6667 s |
| run | 8 | 0.6667 s |
| jump | 6 | 0.5000 s |
| fall | 4 | 0.3333 s |
| attack_g1 | 6 | 0.3000 s, keys 0 50 100 150 200 250 ms |
| attack_g2 | 6 | 0.3000 s, keys 0 50 100 150 200 250 ms |
| attack_g3 | 6 | 0.4500 s, keys 0 80 160 220 280 400 ms |
| wing idle | 8 | 0.6667 s |
| wing flap | 8 | 0.6667 s |
| wing glide | 8 | 0.6667 s |

Locomotion keys land on exact **1/12 s** boundaries (`i / 12`) and clip length is `frames / 12`.
No duplicate tail key — a repeated final frame is a loop hitch.

The combo clips are the one exception, because their lengths are dictated by the SYS3 frame windows
(`CF-A1`/`CF-A2` = 18 f = 0.30 s, `CF-A3` = 27 f = 0.45 s) rather than by a 12 fps art budget. All
three run at 20 fps, but their keys are **explicit times** rather than an `i / 20` grid: G1/G2 spread
the six poses evenly, and G3 travels the whole overhead arc by 280 ms and then holds the forward pose.
A uniform 13.333 fps grid stepped the heaviest swing every 75 ms and read as a stutter.

The clip's last key sits **one frame short of the intended length**, because `AnimationClip.length`
resolves to *final key + one frame at the clip's rate* and the animator measures a state by that
length. G3's last key is therefore at 400 ms with a 20 fps rate, giving `clip.length` = 0.45 s exactly;
`stopTime` alone does not survive the clamp (`0.34 + 1/13.333` came back as 0.415 s).

## Ground combo (G1 → G2 → G3)
Three one-shot swings replace the single `attack_1` state. Each is its own 6-pose 3 × 2 grid sliced
to 256×256 @ PPU 128 and its own clip, so `HeroCombat` can cut from one to the next on its own
recovery frame without a shared timeline.

| Clip | State | Art folder | Swing |
|---|---|---|---|
| `Nexus_Attack_G1` | `attack_g1` | `HD2D/attack_g1/` | downward diagonal cut, right shoulder to low left |
| `Nexus_Attack_G2` | `attack_g2` | `HD2D/attack_g2/` | rising backhand, low left to high right |
| `Nexus_Attack_G3` | `attack_g3` | `HD2D/attack_g3/` | committed overhead finisher, held on the forward pose |

Contract points that apply to all three grids:
- Same twelve prompts, same palette, same mandatory HEAD block as the body set.
- **No painted sword trails, no motion-smear arcs, no broad amber fills.** The generator renders
  "motion smear" as a bright filled amber crescent that breaks the pinstripe-only rule; the swing
  must read from the blade's position alone. This was the reason G3 had to be regenerated four times.
- **Registration block — the rule that makes a swing read as a swing.** Every sheet prompt now carries
  it verbatim, because a swing drawn as six re-posed figures pops:

  > the knight's pelvis, hips, both legs and both boots are in EXACTLY the same position and size in
  > all six cells, standing on the same invisible ground line — the boots never lift, never slide,
  > never change angle; the cape stays behind the body in every cell; only the right arm, the sword
  > and a slight lean of the torso change; the knight fills about 70 % of the cell height, centred,
  > with clear white margin, and the whole sword stays inside its cell.

  Measured spread of the lowest opaque row across the six poses: G1 23–38 px, G2 52–64 px,
  G3 22–45 px, all against a 486 px span — under 0.06 u of visible hop, and no cell touches an edge.
- Blade stays in the right hand, tip always visible.
- Feet stay on the frame baseline; a lunge may lift the rear foot, never the whole figure.

No clipping in the current sheets: every pose measures at least 35 px clear of its cell edges, so the
slicer reports no `CLIPPED-AT-CELL-EDGE` note on any of the three grids.
