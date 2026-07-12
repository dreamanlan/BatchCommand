import sys
from PIL import Image

if len(sys.argv) < 3:
    print("Usage: flip-image.py <image_path> <flip_direction>")
    print("  flip_direction: 0=horizontal, 1=vertical")
    sys.exit(1)

image_path = sys.argv[1]
flip_direction = int(sys.argv[2])

img = Image.open(image_path)

if flip_direction == 0:
    img = img.transpose(Image.FLIP_LEFT_RIGHT)
    print(f"Image flipped horizontally: {image_path}")
elif flip_direction == 1:
    img = img.transpose(Image.FLIP_TOP_BOTTOM)
    print(f"Image flipped vertically: {image_path}")
else:
    print(f"Unknown flip direction: {flip_direction}, expected 0 or 1")
    sys.exit(1)

img.save(image_path)
print("Done")
