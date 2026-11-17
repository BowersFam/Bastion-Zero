#!/usr/bin/env python3
"""Read-only sheet checks. Metrics flag review candidates, not artistic correctness."""
import argparse
import hashlib
import json
from pathlib import Path
from PIL import Image, ImageChops

ACTIONS = ['standing', 'walking', 'running', 'jumping', 'falling', 'death',
           'hit', 'attack_1', 'attack_2', 'attack_3', 'flying']


def inspect(path, rows, cols, alpha_min):
    with Image.open(path) as source:
        sheet = source.convert('RGBA')
    width, height = sheet.size
    cells, hashes, row_prevs = [], {}, {}
    for row in range(rows):
        for col in range(cols):
            bounds = (round(col * width / cols), round(row * height / rows),
                      round((col + 1) * width / cols), round((row + 1) * height / rows))
            cell = sheet.crop(bounds)
            mask = cell.getchannel('A').point(lambda a: 255 if a >= alpha_min else 0)
            bbox = mask.getbbox()
            normalized = cell.resize((64, 64), Image.Resampling.NEAREST)
            digest = hashlib.sha256(normalized.tobytes()).hexdigest()
            duplicate_of = hashes.get(digest)
            hashes.setdefault(digest, [row, col])
            edge = bool(bbox and (bbox[0] <= 1 or bbox[1] <= 1 or
                                 bbox[2] >= cell.width - 1 or bbox[3] >= cell.height - 1))
            previous = row_prevs.get(row)
            difference = None
            if previous is not None:
                changed = ImageChops.difference(previous, normalized)
                values = list(changed.getdata())
                difference = round(sum(any(v) for v in values) / len(values), 5)
            row_prevs[row] = normalized
            cells.append({'row': row, 'col': col,
                          'action': ACTIONS[row] if rows == 11 else str(row),
                          'bounds': bounds, 'bbox': bbox,
                          'occupancy': round(sum(v > 0 for v in mask.getdata()) /
                                             (cell.width * cell.height), 5),
                          'touches_cell_edge': edge,
                          'duplicate_of': duplicate_of,
                          'changed_fraction_vs_previous': difference})
    return {'file': str(path), 'size': [width, height],
            'grid': [cols, rows],
            'nominal_cell': [width / cols, height / rows],
            'fully_opaque': sheet.getchannel('A').getextrema() == (255, 255),
            'warning': 'Equal-grid assumptions require visual verification.',
            'cells': cells}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('sheet', type=Path, nargs='+')
    parser.add_argument('--rows', type=int, default=11)
    parser.add_argument('--cols', type=int, default=4)
    parser.add_argument('--alpha-min', type=int, default=16)
    args = parser.parse_args()
    print(json.dumps([inspect(p, args.rows, args.cols, args.alpha_min)
                      for p in args.sheet], indent=2))


if __name__ == '__main__':
    main()
