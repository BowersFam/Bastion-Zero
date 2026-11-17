#!/usr/bin/env python3
"""Bundle the indexed sprite JSON for a preview that works directly from file://."""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parent


def build(root=ROOT):
    root = Path(root)
    manifest = json.loads((root / "manifest.json").read_text(encoding="utf-8"))
    sprites = {}
    for character, entry in manifest["characters"].items():
        sprites[character] = {}
        for style, relpath in entry["styles"].items():
            path = (root / relpath).resolve()
            if not path.is_relative_to(root.resolve()):
                raise ValueError("Sprite path leaves the pack directory")
            sprites[character][style] = json.loads(path.read_text(encoding="utf-8"))
    packed = json.dumps({"manifest":manifest,"sprites":sprites},separators=(",",":"))
    packed = packed.replace("<", "\\u003c").replace(">", "\\u003e").replace("&", "\\u0026")
    text = "window.SPRITE_PACK=" + packed + ";\n"
    (root / "preview-data.js").write_text(text, encoding="utf-8")
    template = root / "preview.html"
    if template.exists():
        standalone = template.read_text(encoding="utf-8").replace('<script src="preview-data.js"></script>', '<script>' + text + '</script>')
        (root / "preview-standalone.html").write_text(standalone, encoding="utf-8")
    return root / "preview-data.js"


if __name__ == "__main__":
    print(build())
