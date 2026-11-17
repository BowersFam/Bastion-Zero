#!/usr/bin/env python3
"""Validate terminal sprite data and report candidates for visual review."""
import argparse
import hashlib
import json
from pathlib import Path

ACTIONS = ['standing', 'walking', 'running', 'jumping', 'falling', 'death',
           'hit', 'attack_1', 'attack_2', 'attack_3', 'flying']


def inspect(root):
    manifest = json.loads((root / 'manifest.json').read_text())
    report, errors = [], []
    for character, meta in manifest['characters'].items():
        for style in ('2d', '3d'):
            label = f'{character}/{style}'
            if style not in meta['styles']:
                errors.append(f'{label}: missing style')
                continue
            source = root / meta['styles'][style]
            data = json.loads(source.read_text())
            width, height = data['width'], data['height']
            if width != 64 or height != 64:
                errors.append(f'{label}: expected 64x64, got {width}x{height}')
            actions = data['actions']
            for action in ACTIONS:
                identifier = f'{label}/{action}'
                if action not in actions:
                    errors.append(f'{identifier}: missing action')
                    continue
                entry = actions[action]
                if action == 'death' and entry['loop']:
                    errors.append(f'{identifier}: death must not loop')
                frames = entry['frames']
                if len(frames) < 4:
                    errors.append(f'{identifier}: fewer than 4 frames')
                digests, counts, edges, changed = [], [], [], []
                previous = None
                for number, runs in enumerate(frames):
                    pixels = []
                    for count, color in runs:
                        if type(count) is not int or count <= 0:
                            errors.append(f'{identifier}/{number}: invalid run length')
                            continue
                        if type(color) is not int or not -1 <= color <= 255:
                            errors.append(f'{identifier}/{number}: invalid color')
                            continue
                        pixels.extend([color] * count)
                    if len(pixels) != width * height:
                        errors.append(f'{identifier}/{number}: wrong decoded size {len(pixels)}')
                        continue
                    foreground = [i for i, p in enumerate(pixels) if p != -1]
                    counts.append(len(foreground))
                    if not foreground:
                        errors.append(f'{identifier}/{number}: empty frame')
                    else:
                        xs, ys = [i % width for i in foreground], [i // width for i in foreground]
                        if min(xs) == 0 or max(xs) == width - 1 or min(ys) == 0 or max(ys) == height - 1:
                            edges.append(number)
                    digests.append(hashlib.sha256(json.dumps(pixels, separators=(',', ':')).encode()).hexdigest())
                    if previous is not None:
                        changed.append(round(sum(a != b for a, b in zip(previous, pixels)) /
                                             (width * height), 5))
                    previous = pixels
                report.append({'character': character, 'style': style, 'action': action,
                               'frames': len(frames), 'unique_frames': len(set(digests)),
                               'foreground_pixels': counts, 'edge_contact_frames': edges,
                               'changed_fraction': changed})
    return {'errors': errors, 'actions': report}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('root', type=Path)
    args = parser.parse_args()
    result = inspect(args.root)
    print(json.dumps(result, indent=2))
    raise SystemExit(bool(result['errors']))


if __name__ == '__main__':
    main()
