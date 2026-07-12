import sys
import os
from PIL import Image

def parse_args():
    if len(sys.argv) < 7:
        print("Usage: spritesheet_pack.py <src> <dst> <rows> <cols> <frame_w> <frame_h> [order]")
        sys.exit(1)
    src = sys.argv[1]
    dst = sys.argv[2]
    rows = int(sys.argv[3])
    cols = int(sys.argv[4])
    frame_w = int(sys.argv[5])
    frame_h = int(sys.argv[6])
    order = None
    if len(sys.argv) >= 8 and sys.argv[7].strip() != "":
        order = [int(x) for x in sys.argv[7].split(",") if x.strip() != ""]
    return src, dst, rows, cols, frame_w, frame_h, order

def remove_bg(img):
    try:
        from rembg import remove
        out = remove(img)
        if out.mode != "RGBA":
            out = out.convert("RGBA")
        return out, "rembg"
    except Exception as e:
        print("rembg unavailable (" + str(e) + "), fallback to white-tolerance")
        return white_tolerance(img), "fallback"

def white_tolerance(img, tol=18):
    img = img.convert("RGBA")
    px = img.load()
    w, h = img.size
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            if r >= 255 - tol and g >= 255 - tol and b >= 255 - tol:
                px[x, y] = (r, g, b, 0)
    return img

def alpha_bbox(img):
    bbox = img.split()[-1].getbbox()
    if bbox is None:
        return img
    return img.crop(bbox)

def fit_center(img, tw, th):
    iw, ih = img.size
    if iw == 0 or ih == 0:
        return Image.new("RGBA", (tw, th), (0, 0, 0, 0))
    scale = min(tw / iw, th / ih)
    nw = max(1, int(round(iw * scale)))
    nh = max(1, int(round(ih * scale)))
    resized = img.resize((nw, nh), Image.LANCZOS)
    canvas = Image.new("RGBA", (tw, th), (0, 0, 0, 0))
    ox = (tw - nw) // 2
    oy = (th - nh) // 2
    canvas.paste(resized, (ox, oy), resized)
    return canvas

def main():
    src, dst, rows, cols, frame_w, frame_h, order = parse_args()
    print("src=" + src + " dst=" + dst + " grid=" + str(rows) + "x" + str(cols) + " frame=" + str(frame_w) + "x" + str(frame_h) + " order=" + str(order))

    src_img = Image.open(src).convert("RGBA")
    rgba, mode = remove_bg(src_img)
    print("bg-remove mode: " + mode)

    W, H = rgba.size
    cw = W // cols
    ch = H // rows
    print("src size: " + str(W) + "x" + str(H) + ", cell size: " + str(cw) + "x" + str(ch))

    frames = []
    for r in range(rows):
        for c in range(cols):
            box = (c * cw, r * ch, (c + 1) * cw, (r + 1) * ch)
            cell = rgba.crop(box)
            cell = alpha_bbox(cell)
            fitted = fit_center(cell, frame_w, frame_h)
            frames.append(fitted)

    if order is not None:
        if len(order) == 0 or any(i < 0 or i >= len(frames) for i in order):
            print("invalid order " + str(order) + ", expected indices in [0," + str(len(frames)-1) + "]")
            sys.exit(2)
        frames = [frames[i] for i in order]

    n = len(frames)
    sheet = Image.new("RGBA", (frame_w * n, frame_h), (0, 0, 0, 0))
    for i, f in enumerate(frames):
        sheet.paste(f, (i * frame_w, 0), f)

    os.makedirs(os.path.dirname(os.path.abspath(dst)), exist_ok=True)
    sheet.save(dst, "PNG")
    print("Done: " + dst + "  (" + str(frame_w * n) + "x" + str(frame_h) + ", " + str(n) + " frames)")

if __name__ == "__main__":
    main()
