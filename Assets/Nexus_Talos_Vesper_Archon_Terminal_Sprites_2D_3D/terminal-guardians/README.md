# Terminal Guardians

Nexus, Talos, Vesper and Archon in 2D pixel and 3D voxel-style terminal sprites.

Open **preview-standalone.html** to compare both versions, switch characters and actions, pause, scrub frames and test dark, light or checkerboard backgrounds. No server or internet connection is needed. **previews/terminal-guardians-animations.gif** cycles through all actions for all eight sets.

## Included

- 4 characters, 2 styles each.
- 11 actions per set, 4 keyframes per action.
- 88 animation clips and 352 distinct indexed frames.
- Transparent 64 x 64 PNG frames and 4-column, 11-row sprite atlases.
- A static `.ans` export of every frame, animated GIF clips, indexed JSON and a terminal player.
- Original generated sheets, generation prompts, rebuild scripts and validation reports.

| Row | Action | Playback |
| --- | --- | --- |
| 1 | Standing | Loop |
| 2 | Walking | Loop |
| 3 | Running | Loop |
| 4 | Jumping | Once |
| 5 | Falling and landing | Once |
| 6 | Death | Once, ends prone |
| 7 | Hit | Once |
| 8 | Attack 1 | Once |
| 9 | Attack 2 | Once |
| 10 | Attack 3 | Once |
| 11 | Flying | Loop |

| Character | Attack 1 | Attack 2 | Attack 3 |
| --- | --- | --- | --- |
| Nexus | Sword slash | Sword thrust | Energy sweep |
| Talos | Heavy punch | Tail sweep | Molten core burst |
| Vesper | Talon rake | Beak lunge | Wing slam |
| Archon | Bite lunge | Armored paw strike | Golden bark pulse |

## Run in a terminal

Python 3.9 or newer is needed only for animated terminal playback. No packages are required to play. Use a UTF-8 terminal and a font with Unicode upper/lower half-block characters.

```sh
python play.py --character nexus --style 2d --action standing
python play.py --character archon --style 3d --action running
python play.py --character vesper --style 2d --action flying
python play.py --character talos --style 3d --demo
```

Press Ctrl+C to stop. See **RUN.md** for sizing, speed, color fallbacks and one-shot controls. See **SCHEMA.md** for integration.

## Terminal format

The canonical sprites contain a fixed 32-color subset of the xterm256 palette. Two vertical logical pixels share one Unicode terminal cell, so a 64 x 64 frame occupies 64 columns and 32 rows, plus the player's label. Smaller 48- and 32-column versions are available through the player. Transparent pixels use the terminal's default background. Exact color appearance depends on terminal support; the ANSI16 fallback reduces detail.

The 3D choice is a voxel-style **raster render with depth and facet shading**. Both choices render as terminal cells. There are no editable meshes or real-time 3D scenes in this pack. The supplied animations use a fixed right-facing view with a slight three-quarter angle. These are four-keyframe animation sets for style selection and terminal integration.

## Character interpretation and source

“Halos” was interpreted as **Talos**, the established volcanic armored guardian. This name interpretation remains unconfirmed. The character designs were recovered from earlier text descriptions. No prior full-resolution character reference image was attached to this conversation.

Nexus retains the gold visor, astronaut armor, sword and matching back wings. Vesper retains the raven/eagle head, eagle foretalons, lion hindquarters and gold feather accents. Archon retains a recognizable German Shepherd head, ears, muzzle, coat and tail with gold armor and guardian wings. Talos retains obsidian armor, molten gold cracks, a blazing chest core and heavy kaiju silhouette; its flying animation uses molten thrusters.

The original art was generated with the built-in image generator, with the exact prompt for each set in **prompts/**. `build_assets.py` performs terminal-format conversion: transparent row extraction, consistent scale within each set, palette conversion and export. Source-sheet rows have slightly variable spacing; use the extracted atlases or JSON for integration rather than dividing the originals at equal row intervals. The JSON and terminal PNGs share the same indexed pixels.

## Rebuild and validation

Playback and previews need no dependencies. Rebuilding the image exports requires Pillow and NumPy in addition to Python.

```sh
python build_assets.py
python build_preview_data.py
python qa/validate_pack.py .
```

The QA report checks action coverage, four frames per clip, unique frames, 64 x 64 decoded length, valid xterm indices, nonempty silhouettes and frame-edge contact. These structural checks supplement visual inspection; they do not certify game-engine integration or behavior on every terminal.
