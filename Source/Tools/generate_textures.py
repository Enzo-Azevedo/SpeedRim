#!/usr/bin/env python3
"""Generates the SpeedRim button textures and the About/Preview.png image.

Run from the mod folder:  python3 Source/Tools/generate_textures.py
Requires Pillow (pip install pillow).
"""

import os

from PIL import Image, ImageDraw, ImageFont

HERE = os.path.dirname(os.path.abspath(__file__))
MOD_ROOT = os.path.abspath(os.path.join(HERE, "..", ".."))
TEXTURE_DIR = os.path.join(MOD_ROOT, "Textures", "SpeedRim")
ABOUT_DIR = os.path.join(MOD_ROOT, "About")

# RimWorld draws the time buttons in a 32x24 rect; author at 4x and downscale.
BUTTON_SIZE = (64, 48)
SUPERSAMPLE = 8

BORDER = (222, 222, 222, 255)
FILL = (26, 26, 26, 205)
GLYPH = (245, 245, 245, 255)

FONT_CANDIDATES = [
    "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf",
    "/usr/share/fonts/truetype/liberation/LiberationSans-Bold.ttf",
    "/Library/Fonts/Arial Bold.ttf",
    "C:\\Windows\\Fonts\\arialbd.ttf",
]


def load_font(size):
    for path in FONT_CANDIDATES:
        if os.path.exists(path):
            return ImageFont.truetype(path, size)
    return ImageFont.load_default()


def rounded_box(draw, box, radius, fill, outline, width):
    draw.rounded_rectangle(box, radius=radius, fill=fill, outline=outline, width=width)


def centered_text(draw, box, text, font, fill):
    left, top, right, bottom = draw.textbbox((0, 0), text, font=font)
    x = box[0] + (box[2] - box[0] - (right - left)) / 2 - left
    y = box[1] + (box[3] - box[1] - (bottom - top)) / 2 - top
    draw.text((x, y), text, font=font, fill=fill)


def fitted_font(draw, text, max_width, max_height):
    """Largest font size that keeps the label inside the button with a little breathing room."""
    size = max_height
    while size > 8:
        font = load_font(size)
        left, top, right, bottom = draw.textbbox((0, 0), text, font=font)
        if right - left <= max_width and bottom - top <= max_height:
            return font
        size -= SUPERSAMPLE
    return load_font(size)


def make_button(label):
    width, height = BUTTON_SIZE[0] * SUPERSAMPLE, BUTTON_SIZE[1] * SUPERSAMPLE
    image = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)

    inset = 2 * SUPERSAMPLE
    box = (inset, inset, width - inset - 1, height - inset - 1)
    rounded_box(draw, box, radius=4 * SUPERSAMPLE, fill=FILL, outline=BORDER, width=int(1.5 * SUPERSAMPLE))

    padding = 6 * SUPERSAMPLE
    text_box = (box[0] + padding, box[1] + padding, box[2] - padding, box[3] - padding)
    font = fitted_font(draw, label, text_box[2] - text_box[0], text_box[3] - text_box[1])
    centered_text(draw, text_box, label, font, GLYPH)

    return image.resize(BUTTON_SIZE, Image.LANCZOS)


def make_preview():
    width, height = 640, 360
    image = Image.new("RGBA", (width, height), (24, 26, 28, 255))
    draw = ImageDraw.Draw(image)

    for y in range(height):
        shade = int(22 + 24 * (y / height))
        draw.line([(0, y), (width, y)], fill=(shade, shade + 2, shade + 4, 255))

    draw.rectangle([(0, 0), (width - 1, height - 1)], outline=(96, 98, 102, 255), width=3)

    centered_text(draw, (0, 40, width, 116), "SpeedRim", load_font(62), (242, 242, 242, 255))
    centered_text(draw, (0, 112, width, 152), "Extra game speeds for RimWorld 1.6",
                  load_font(23), (172, 174, 178, 255))

    display = (128, 96)
    gap = 28
    labels = ["5x", "10x", "20x"]
    total = display[0] * len(labels) + gap * (len(labels) - 1)
    x = (width - total) // 2
    top = 178
    for label in labels:
        image.alpha_composite(make_button(label).resize(display, Image.LANCZOS), (x, top))
        x += display[0] + gap

    centered_text(draw, (0, top + display[1] + 18, width, height - 16),
                  "One click - or one key - past Superfast", load_font(22), (166, 168, 172, 255))
    return image.convert("RGB")


def main():
    os.makedirs(TEXTURE_DIR, exist_ok=True)
    os.makedirs(ABOUT_DIR, exist_ok=True)

    for index, label in enumerate(["5x", "10x", "20x"], start=1):
        path = os.path.join(TEXTURE_DIR, "SpeedButton_Tier{0}.png".format(index))
        make_button(label).save(path)
        print("wrote", path)

    preview_path = os.path.join(ABOUT_DIR, "Preview.png")
    make_preview().save(preview_path)
    print("wrote", preview_path)


if __name__ == "__main__":
    main()
