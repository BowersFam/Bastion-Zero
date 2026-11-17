#!/usr/bin/env python3
"""Convert generated RGBA sprite sheets to exact ANSI-indexed terminal assets.

This is a format export for the requested terminal sprites, not an art generator.
Pillow and numpy are required only to rebuild; the player is standard-library only.
"""
from pathlib import Path
import json
import itertools
import sys
import numpy as np
from PIL import Image, ImageDraw, ImageFont
import play

ROOT = Path(__file__).resolve().parent
ACTIONS = list(play.ACTIONS)
CHARACTERS = list(play.CHARACTERS)
FPS = dict(standing=4, walking=7, running=10, jumping=8, falling=8,
           death=6, hit=9, attack_1=9, attack_2=9, attack_3=8, flying=7)
LOOPS = {'standing', 'walking', 'running', 'flying'}
# All entries 16+ have fixed RGB values independent of the user's ANSI16 theme.
COLORS = [16, 232, 234, 235, 236, 237, 238, 239, 240, 241, 243, 245,
          52, 58, 94, 95, 130, 136, 137, 172, 178, 180, 214, 215,
          220, 221, 223, 229, 230, 250, 254, 255]
COLOR_RGB = np.array([play.PALETTE[i] for i in COLORS], dtype=np.int32)
ATTACKS = {
    'nexus': ['Sword slash', 'Sword thrust', 'Energy sweep'],
    'talos': ['Heavy punch', 'Tail sweep', 'Molten core burst'],
    'vesper': ['Talon rake', 'Beak lunge', 'Wing slam'],
    'archon': ['Bite lunge', 'Armored paw strike', 'Golden bark pulse'],
}


def row_bounds(rgba):
    """Find transparent separators near expected rows; avoid cutting feet."""
    alpha = np.asarray(rgba)[:, :, 3]
    clear = (alpha >= 96).sum(axis=1) <= 2
    runs, start = [], None
    for y, empty in enumerate(list(clear) + [False]):
        if empty and start is None:
            start = y
        elif not empty and start is not None:
            if y-start >= 4:
                runs.append((start, y))
            start = None
    bounds = [0]
    for i in range(1, 11):
        expected = i*rgba.height/11
        candidates = [r for r in runs if abs(sum(r)/2-expected) < 90
                      and r[0] > bounds[-1]+40]
        if not candidates:
            raise ValueError(f'No transparent separator for row {i}')
        chosen = min(candidates, key=lambda r: abs(sum(r)/2-expected))
        bounds.append(round(sum(chosen)/2))
    return bounds + [rgba.height]


def clean_cell(cell):
    """Remove isolated export specks and disconnected neighboring-cell fragments."""
    arr = np.array(cell)
    mask = arr[:, :, 3] >= 96
    seen = np.zeros(mask.shape, dtype=bool)
    components = []
    for y, x in zip(*np.where(mask)):
        if seen[y, x]:
            continue
        todo, component = [(y, x)], []
        seen[y, x] = True
        while todo:
            cy, cx = todo.pop()
            component.append((cy, cx))
            for dy, dx in ((1,0),(-1,0),(0,1),(0,-1)):
                ny, nx = cy+dy, cx+dx
                if 0 <= ny < mask.shape[0] and 0 <= nx < mask.shape[1] and mask[ny,nx] and not seen[ny,nx]:
                    seen[ny,nx] = True
                    todo.append((ny,nx))
        components.append(component)
    if not components:
        raise ValueError('Empty sprite cell')
    largest = max(components, key=len)
    ys, xs = zip(*largest)
    mainbox = (min(xs), min(ys), max(xs), max(ys))
    keep = np.zeros(mask.shape, dtype=bool)
    for component in components:
        ys, xs = zip(*component)
        # Preserve deliberate disconnected sparks near the figure, but no tiny noise.
        near = min(xs) <= mainbox[2]+24 and max(xs) >= mainbox[0]-24 and min(ys) <= mainbox[3]+24 and max(ys) >= mainbox[1]-24
        if len(component) >= 12 and near:
            for y, x in component:
                keep[y, x] = True
    arr[:, :, 3] = np.where(keep, arr[:, :, 3], 0)
    return Image.fromarray(arr)


def encode(pixels):
    return [[sum(1 for _ in group), int(color)] for color, group in itertools.groupby(pixels)]


def raster(pixels, background=None):
    arr = np.array([(0,0,0,0) if c < 0 else (*play.PALETTE[c],255) for c in pixels],dtype=np.uint8).reshape((64,64,4))
    image = Image.fromarray(arr)
    if background is not None:
        out = Image.new('RGBA', image.size, background)
        out.alpha_composite(image)
        return out.convert('RGB')
    return image


def font(size):
    try:
        return ImageFont.truetype('/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf',size)
    except OSError:
        return ImageFont.load_default(size=size)


def main():
    manifest = {
        'schema_version':1,
        'title':'Terminal Guardians',
        'version':'1.0.0',
        'canvas':[64,64],
        'terminal_cells':[64,32],
        'palette':'fixed xterm256 subset',
        'palette_indices':COLORS,
        'note':'3d means voxel-style raster sprites, not a 3D mesh. Halos interpreted as Talos.',
        'characters':{},
    }
    exports, extraction = {}, {}
    for character in CHARACTERS:
        manifest['characters'][character]={'name':character.title(), 'styles':{}}
        exports[character]={}
        for style in play.STYLES:
            key = f'{character}_{style}'
            source = Image.open(ROOT/'source'/f'{key}.png').convert('RGBA')
            boundaries = row_bounds(source)
            cells=[]
            for row in range(11):
                cells.append([clean_cell(source.crop((col*189,boundaries[row],(col+1)*189,boundaries[row+1]))) for col in range(4)])
            baselines=[max(c.getbbox()[3] for c in row) for row in cells]
            # Preserve one common scale across every action and frame in this set.
            max_span=max(b-min(c.getbbox()[1] for c in row) for row,b in zip(cells,baselines))
            scale=min(60/189,56/max_span)
            sprite={'schema_version':1,'character':character,'style':style,'width':64,'height':64,'actions':{}}
            exports[character][style]={}
            atlas=Image.new('RGBA',(64*4,64*11))
            for row,action in enumerate(ACTIONS):
                raw=[]
                png_dir=ROOT/'frames'/character/style/action
                ans_dir=ROOT/'ansi'/character/style/action
                png_dir.mkdir(parents=True,exist_ok=True)
                ans_dir.mkdir(parents=True,exist_ok=True)
                for col,cell in enumerate(cells[row]):
                    # BOX averages generated detail into terminal-sized pixel clusters.
                    resized=cell.resize((round(cell.width*scale),round(cell.height*scale)),Image.Resampling.BOX)
                    canvas=Image.new('RGBA',(64,64))
                    left=round(32-94.5*scale)
                    lift=6 if character=='archon' and action=='flying' else 0
                    top=round(60-baselines[row]*scale)-lift
                    canvas.alpha_composite(resized,(left,top))
                    rgba=np.asarray(canvas)
                    rgb=rgba[:,:,:3].astype(np.int32)
                    distances=((rgb[:,:,None,:]-COLOR_RGB[None,None,:,:])**2).sum(axis=3)
                    indices=np.array(COLORS)[distances.argmin(axis=2)]
                    indices=np.where(rgba[:,:,3]>=96,indices,-1)
                    pixels=indices.ravel().tolist()
                    raw.append(pixels)
                    image=raster(pixels)
                    image.save(png_dir/f'{col:02d}.png')
                    atlas.alpha_composite(image,(64*col,64*row))
                    (ans_dir/f'{col:02d}.ans').write_text(play.render_frame(pixels,64,64)+'\x1b[0m\n',encoding='utf-8')
                clip={'fps':FPS[action],'loop':action in LOOPS,'frames':[encode(f) for f in raw]}
                if action.startswith('attack_'):
                    clip['description']=ATTACKS[character][int(action[-1])-1]
                sprite['actions'][action]=clip
                exports[character][style][action]=raw
                gif_frames=[raster(p,(18,21,25,255)).resize((256,256),Image.Resampling.NEAREST) for p in raw]
                gif_dir=ROOT/'gifs'/character/style
                gif_dir.mkdir(parents=True,exist_ok=True)
                gif_args={'save_all':True,'append_images':gif_frames[1:],'duration':round(1000/FPS[action]),'disposal':2}
                if action in LOOPS:gif_args['loop']=0
                gif_frames[0].save(gif_dir/f'{action}.gif',**gif_args)
            path=ROOT/'sprites'/character/f'{style}.json'
            path.parent.mkdir(parents=True,exist_ok=True)
            path.write_text(json.dumps(sprite,separators=(',',':')),encoding='utf-8')
            manifest['characters'][character]['styles'][style]=str(path.relative_to(ROOT))
            sheetdir=ROOT/'sheets';sheetdir.mkdir(exist_ok=True)
            atlas.save(sheetdir/f'{key}-ansi256.png')
            extraction[key]={'source_dimensions':source.size,'row_boundaries':boundaries,'baselines':baselines,'scale':scale,'export_canvas':[64,64]}
    (ROOT/'manifest.json').write_text(json.dumps(manifest,indent=2)+'\n',encoding='utf-8')
    (ROOT/'qa'/'extraction.json').write_text(json.dumps(extraction,indent=2)+'\n',encoding='utf-8')
    preview_dir=ROOT/'previews';preview_dir.mkdir(exist_ok=True)
    cards=[]
    for action in ACTIONS:
        for frame in range(4):
            view=Image.new('RGB',(576,1100),(13,17,22))
            draw=ImageDraw.Draw(view)
            draw.text((20,14),'TERMINAL GUARDIANS',font=font(23),fill='#efd28c')
            draw.text((20,50),f'{action.replace("_"," ").title()}  |  frame {frame+1}/4',font=font(17),fill='#c0c6ce')
            draw.text((95,84),'2D PIXEL',font=font(16),fill='#e7b64f')
            draw.text((374,84),'3D VOXEL',font=font(16),fill='#e7b64f')
            for r,character in enumerate(CHARACTERS):
                y=116+r*241
                draw.text((20,y),character.title(),font=font(18),fill='#f2eadb')
                for col,style in enumerate(play.STYLES):
                    image=raster(exports[character][style][action][frame],(22,27,34,255)).resize((192,192),Image.Resampling.NEAREST)
                    x=47+col*280
                    view.paste(image,(x,y+28))
            draw.text((20,1082),'Exact ANSI256 pixels | 64 x 64 logical canvas',font=font(12),fill='#939da9')
            cards.append(view)
    cards[0].save(preview_dir/'terminal-guardians-comparison.png')
    cards[0].save(preview_dir/'terminal-guardians-animations.gif',save_all=True,append_images=cards[1:],duration=220,loop=0,disposal=2)
    print('Exported 8 sets, 88 clips, 352 PNG frames and 352 ANSI frames.')


if __name__=='__main__':
    main()
