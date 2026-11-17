#!/usr/bin/env python3
"""Play indexed guardian sprites in a Unicode/ANSI terminal. Standard library only."""
from __future__ import annotations

import argparse
import itertools
import json
import math
import os
from pathlib import Path
import signal
import shutil
import sys
import time

ROOT = Path(__file__).resolve().parent
CHARACTERS = ("nexus", "talos", "vesper", "archon")
STYLES = ("2d", "3d")
ACTIONS = ("standing", "walking", "running", "jumping", "falling", "death", "hit", "attack_1", "attack_2", "attack_3", "flying")
ANSI16 = [(0,0,0),(128,0,0),(0,128,0),(128,128,0),(0,0,128),(128,0,128),(0,128,128),(192,192,192),
          (128,128,128),(255,0,0),(0,255,0),(255,255,0),(0,0,255),(255,0,255),(0,255,255),(255,255,255)]
LEVELS = (0, 95, 135, 175, 215, 255)
PALETTE = ANSI16 + [(r,g,b) for r in LEVELS for g in LEVELS for b in LEVELS] + [(8+10*i,)*3 for i in range(24)]


def decode_frame(runs, width, height):
    """Reject malformed files instead of allocating from unbounded run counts."""
    pixels = []
    limit = width * height
    for run in runs:
        if not isinstance(run, list) or len(run) != 2:
            raise ValueError("A frame run must be [count, index]")
        count, color = run
        if type(count) is not int or count < 1 or len(pixels) + count > limit:
            raise ValueError("Invalid frame run length")
        if type(color) is not int or color < -1 or color > 255:
            raise ValueError("Invalid xterm color index")
        pixels.extend([color] * count)
    if len(pixels) != limit:
        raise ValueError(f"Frame has {len(pixels)} pixels; expected {limit}")
    return pixels


def resize(pixels, width, height, target_width):
    target_height = max(2, round(height * target_width / width))
    if target_height % 2:
        target_height += 1
    return ([pixels[min(height-1, y*height//target_height)*width + min(width-1, x*width//target_width)]
             for y in range(target_height) for x in range(target_width)], target_width, target_height)


def sgr(color, background=False, mode="256"):
    if color == -1:
        return "\x1b[49m" if background else "\x1b[39m"
    if mode == "truecolor":
        r, g, b = PALETTE[color]
        return f"\x1b[{48 if background else 38};2;{r};{g};{b}m"
    if mode == "16":
        rgb = PALETTE[color]
        index = min(range(16), key=lambda i: sum((rgb[c] - ANSI16[i][c])**2 for c in range(3)))
        code = (40 if background else 30) + index if index < 8 else (100 if background else 90) + index - 8
        return f"\x1b[{code}m"
    return f"\x1b[{48 if background else 38};5;{color}m"


def render_frame(pixels, width, height, color_mode="256"):
    lines = []
    for y in range(0, height, 2):
        chunks = ["\x1b[0m"]
        previous = (None, None)
        for x in range(width):
            top = pixels[y*width+x]
            bottom = pixels[(y+1)*width+x] if y+1 < height else -1
            if top == bottom:
                glyph, fg, bg = " ", -1, top
            elif top == -1:
                glyph, fg, bg = "▄", bottom, -1
            else:
                glyph, fg, bg = "▀", top, bottom
            if fg != previous[0]:
                chunks.append(sgr(fg, False, color_mode))
            if bg != previous[1]:
                chunks.append(sgr(bg, True, color_mode))
            chunks.append(glyph)
            previous = (fg, bg)
        chunks.append("\x1b[0m")
        lines.append("".join(chunks))
    return "\n".join(lines)


def load_sprite(root, manifest, character, style):
    rel = manifest["characters"][character]["styles"][style]
    path = (root / rel).resolve()
    if not path.is_relative_to(root.resolve()):
        raise ValueError("Sprite path leaves the pack directory")
    sprite = json.loads(path.read_text(encoding="utf-8"))
    width, height = sprite["width"], sprite["height"]
    if type(width) is not int or type(height) is not int or not (1 <= width <= 512 and 1 <= height <= 512):
        raise ValueError("Sprite dimensions must be integers from 1 to 512")
    if sprite.get("schema_version") != 1:
        raise ValueError("Unsupported sprite schema")
    return sprite


def prepare_action(sprite, action, size, scale, mode):
    clip = sprite["actions"][action]
    fps = clip.get("fps", 8)
    if not isinstance(fps, (int, float)) or not math.isfinite(fps) or not (0 < fps <= 120):
        raise ValueError("Clip FPS must be between 0 and 120")
    if not clip.get("frames"):
        raise ValueError(f"No frames in action {action}")
    frames = []
    for runs in clip["frames"]:
        pixels = decode_frame(runs, sprite["width"], sprite["height"])
        pixels, width, height = resize(pixels, sprite["width"], sprite["height"], size * scale)
        frames.append(render_frame(pixels, width, height, mode))
    return clip, frames


def enable_windows_ansi():
    if os.name == "nt":
        import ctypes
        kernel = ctypes.windll.kernel32
        handle = kernel.GetStdHandle(-11)
        mode = ctypes.c_uint()
        if kernel.GetConsoleMode(handle, ctypes.byref(mode)):
            kernel.SetConsoleMode(handle, mode.value | 0x0004)
        kernel.SetConsoleOutputCP(65001)


def safe_label(value):
    return "".join(c for c in str(value) if c.isprintable() and c not in "\r\n")


def resolve_size(requested, scale, tty):
    if requested is not None:
        return requested
    if not tty:
        return 64
    columns, rows = shutil.get_terminal_size(fallback=(80, 24))
    for size in (64, 48, 32):
        if size * scale <= columns and size * scale <= (rows - 2) * 2:
            return size
    raise ValueError(f"Terminal pane is {columns} columns by {rows} rows. Enlarge it to at least {32*scale} columns by {16*scale+2} rows, or use --scale 1.")


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--character", choices=CHARACTERS, default="nexus")
    parser.add_argument("--style", choices=STYLES, default="2d")
    parser.add_argument("--action", choices=ACTIONS, default="standing")
    parser.add_argument("--fps", type=float, help="Override animation frames per second")
    parser.add_argument("--size", type=int, choices=(32, 48, 64), help="Terminal columns at scale 1; omitted: fit the current pane")
    parser.add_argument("--scale", type=int, choices=(1, 2), default=1)
    parser.add_argument("--color", choices=("256", "16", "truecolor"), default="256")
    parser.add_argument("--list", action="store_true", help="List included characters, styles and actions")
    parser.add_argument("--demo", action="store_true", help="Play every action once for the selected character and style")
    parser.add_argument("--once", action="store_true", help="Play one cycle and leave its final frame on screen")
    parser.add_argument("--no-alt-screen", action="store_true", help="Animate at the current terminal cursor position")
    args = parser.parse_args(argv)
    tty = sys.stdout.isatty()
    if args.fps is not None and (not math.isfinite(args.fps) or not 0 < args.fps <= 120):
        parser.error("--fps must be between 0 and 120")
    try:
        manifest = json.loads((ROOT / "manifest.json").read_text(encoding="utf-8"))
        if args.list:
            for key, entry in manifest["characters"].items():
                print(f"{key}: {entry.get('name', key)} | styles: {', '.join(entry['styles'])}")
            print("Actions: " + ", ".join(ACTIONS))
            return 0
        args.size = resolve_size(args.size, args.scale, tty)
        sprite = load_sprite(ROOT, manifest, args.character, args.style)
        actions = [a for a in ACTIONS if a in sprite["actions"]] if args.demo else [args.action]
        prepared = [(a, *prepare_action(sprite, a, args.size, args.scale, args.color)) for a in actions]
    except (OSError, ValueError, KeyError, TypeError) as exc:
        parser.exit(2, f"Sprite pack error: {exc}\n")

    enable_windows_ansi()
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8")
    alternate = tty and not args.no_alt_screen
    original_sigterm = signal.getsignal(signal.SIGTERM)
    signal.signal(signal.SIGTERM, lambda *_: (_ for _ in ()).throw(KeyboardInterrupt()))
    if tty:
        sys.stdout.write("\x1b[?1049h\x1b[2J" if alternate else "\x1b[s")
        sys.stdout.write("\x1b[?25l")
        sys.stdout.flush()
    try:
        for action, clip, frames in prepared:
            fps = args.fps or clip.get("fps", 8)
            loop = clip.get("loop", False) and not args.once and not args.demo
            sequence = itertools.cycle(enumerate(frames)) if loop else enumerate(frames)
            for index, frame in sequence:
                start = time.monotonic()
                prefix = "\x1b[H" if alternate else "\x1b[u" if tty else ""
                label = f"{safe_label(args.character)}  {args.style}  {action}  {index+1}/{len(frames)}"
                sys.stdout.write(prefix + "\x1b[0m" + label.ljust(args.size*args.scale) + "\n" + frame + "\x1b[0m")
                if not tty:
                    sys.stdout.write("\n")
                sys.stdout.flush()
                time.sleep(max(0, 1/fps - (time.monotonic() - start)))
            if args.demo:
                time.sleep(0.35)
        if alternate and not args.demo:
            # Keep one-shot final poses reviewable until a key-independent interrupt.
            # Leaving the alternate buffer would otherwise erase the result instantly.
            sys.stdout.write("\n\x1b[0mFinal pose. Press Ctrl+C to close.")
            sys.stdout.flush()
            while True:
                time.sleep(0.1)
    except KeyboardInterrupt:
        pass
    finally:
        sys.stdout.write("\x1b[0m")
        if tty:
            sys.stdout.write("\x1b[?25h")
            if alternate:
                sys.stdout.write("\x1b[?1049l")
            else:
                sys.stdout.write("\n")
        sys.stdout.flush()
        signal.signal(signal.SIGTERM, original_sigterm)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
