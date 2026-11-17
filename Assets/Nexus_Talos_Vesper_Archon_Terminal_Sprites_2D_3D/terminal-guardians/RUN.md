# Play the terminal sprite pack

Unzip the entire pack. Open `preview.html` to compare the two styles without installing anything. Choose a character, action, background and speed. The preview shows the exact indexed sprite pixels used by the terminal player.

For terminal playback, use Python 3.9 or later in a UTF-8 terminal with ANSI color support. Windows Terminal, modern PowerShell terminals and macOS/Linux terminals support the required escape sequences. A font with `▀` and `▄` glyphs is required. No Python packages are needed.

```sh
python play.py --list
python play.py --character nexus --style 2d --action standing
python play.py --character archon --style 3d --action running
python play.py --character vesper --style 2d --action flying
python play.py --character talos --style 3d --demo
```

Press Ctrl+C to stop. The player resets colors and restores the cursor. Looping actions repeat until stopped. Hit, death and attacks play once and keep their final pose visible. `--once` plays one cycle of a looping action. `--demo` plays all eleven actions once and then exits.

The player automatically picks the largest of 64, 48 or 32 pixel widths that fits the current terminal pane. A typical 80-column × 24-row pane uses 32 pixels. A 64 × 64 pixel frame occupies **64 terminal columns × 32 rows**, plus its label and a spare row. Two vertical pixels share one terminal character cell. Use `--size 48` or `--size 32` to choose an explicit size; explicit sizes are preserved even if the pane is smaller. `--scale 2` doubles the displayed pixel dimensions, and automatic fitting accounts for that scale. If even the smallest size cannot fit, the player asks you to enlarge the pane. Redirected output defaults to 64 pixels. Font cell proportions vary, so the browser preview and terminal may differ slightly in apparent aspect ratio.

```sh
python play.py --character nexus --style 3d --action walking --size 48
python play.py --character archon --style 2d --action attack_2 --fps 10
python play.py --character vesper --style 3d --action flying --color 16
python play.py --character nexus --style 2d --action jumping --once --no-alt-screen
```

`--color 256` uses the exact exported xterm indices. `--color 16` maps them to the closest classic ANSI color and loses detail. `--color truecolor` sends the same 256 palette colors through RGB escape sequences. Transparent pixels use the terminal’s default background, rather than painting a fixed rectangle.

The 3D option is a voxel-style sprite render. It is still displayed as terminal cells. No 3D model file is included.

For integration, see `SCHEMA.md`. Every action has its own frame list, playback speed and loop behavior. Keep each sprite file and its declared dimensions together.
