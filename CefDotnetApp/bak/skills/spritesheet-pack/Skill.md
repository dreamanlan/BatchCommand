# spritesheet-pack Skill

Split a grid pose sheet (rows x cols) into frames, remove background to transparent, resize each frame to target size, and concat horizontally into a single-row sprite sheet PNG.

## Usage

call_skill('spritesheet-pack', src, dst, rows, cols, frame_w, frame_h, order)

## Parameters

- src: source grid image path (e.g. 2x2 pose sheet)
- dst: output PNG path (single-row sprite sheet with transparent background)
- rows: grid rows
- cols: grid cols
- frame_w: target per-frame width (e.g. 512)
- frame_h: target per-frame height (e.g. 512)
- order: optional, comma-separated frame indices, row-major by default. Example: "0,2,3,1"

## Behavior

1. Load source image.
2. Remove background using rembg (fallback to white-tolerance if rembg unavailable), output RGBA.
3. Split into rows x cols cells (equal size).
4. For each cell, crop by alpha bbox to remove empty margins.
5. Fit each cropped subject into frame_w x frame_h with aspect ratio preserved, centered on transparent canvas.
6. Reorder frames by order if provided.
7. Concat frames horizontally into a (frame_w * N) x frame_h PNG.

## Requirements

- Python 3.x
- Pillow (pip install Pillow)
- rembg (pip install rembg onnxruntime), optional but recommended
