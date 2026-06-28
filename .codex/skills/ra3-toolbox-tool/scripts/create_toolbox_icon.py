#!/usr/bin/env python3
from __future__ import annotations

import argparse
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


SIZE = 512
SCALE = 4


def px(value: float) -> int:
    return int(round(value * SCALE))


def load_font(size: int) -> ImageFont.ImageFont:
    font_paths = [
        Path(r"C:\Windows\Fonts\seguisb.ttf"),
        Path(r"C:\Windows\Fonts\arialbd.ttf"),
    ]
    for path in font_paths:
        if path.exists():
            return ImageFont.truetype(str(path), px(size))
    return ImageFont.load_default()


def draw_background(draw: ImageDraw.ImageDraw) -> None:
    draw.ellipse([px(58), px(58), px(454), px(454)], fill="#1f6dc6")
    draw.ellipse([px(83), px(83), px(429), px(429)], fill="#3f8ede")
    draw.arc([px(83), px(83), px(429), px(429)], 200, 340, fill="#2b7ed2", width=px(7))


def draw_panel(draw: ImageDraw.ImageDraw) -> None:
    draw.rounded_rectangle([px(116), px(145), px(396), px(336)], radius=px(27), fill="#173f66")
    draw.rounded_rectangle([px(137), px(167), px(375), px(314)], radius=px(16), fill="#e8f2fb")


def draw_image_symbol(draw: ImageDraw.ImageDraw) -> None:
    draw.rounded_rectangle([px(158), px(184), px(255), px(199)], radius=px(4), fill="#bad1eb")
    draw.rounded_rectangle([px(158), px(207), px(232), px(222)], radius=px(4), fill="#bad1eb")
    draw.ellipse([px(294), px(188), px(324), px(218)], fill="#43a0f5")
    draw.polygon([(px(150), px(292)), (px(213), px(228)), (px(270), px(292))], fill="#9bbce1")
    draw.polygon([(px(212), px(292)), (px(285), px(231)), (px(362), px(292))], fill="#43a0f5")
    draw.rectangle([px(150), px(288), px(362), px(300)], fill="#43a0f5")


def draw_code_symbol(draw: ImageDraw.ImageDraw) -> None:
    dark = "#173f66"
    accent = "#43a0f5"
    draw.line([(px(206), px(212)), (px(168), px(242)), (px(206), px(272))], fill=accent, width=px(14), joint="curve")
    draw.line([(px(306), px(212)), (px(344), px(242)), (px(306), px(272))], fill=dark, width=px(14), joint="curve")
    draw.line([(px(278), px(202)), (px(236), px(282))], fill=dark, width=px(10))


def draw_table_symbol(draw: ImageDraw.ImageDraw) -> None:
    dark = "#173f66"
    accent = "#43a0f5"
    for y in [198, 238, 278]:
        draw.line([(px(170), px(y)), (px(342), px(y))], fill="#9bbce1", width=px(12))
    draw.line([(px(188), px(188)), (px(188), px(290))], fill=dark, width=px(12))
    for y in [198, 238, 278]:
        draw.ellipse([px(228), px(y - 10), px(248), px(y + 10)], fill=accent)


def draw_window_symbol(draw: ImageDraw.ImageDraw) -> None:
    draw.rounded_rectangle([px(162), px(190), px(350), px(287)], radius=px(10), fill="#d9e9f8")
    draw.rectangle([px(162), px(190), px(350), px(215)], fill="#9bbce1")
    draw.rectangle([px(180), px(232), px(248), px(248)], fill="#43a0f5")
    draw.rectangle([px(180), px(260), px(318), px(276)], fill="#9bbce1")


def draw_tool_symbol(draw: ImageDraw.ImageDraw) -> None:
    dark = "#173f66"
    accent = "#43a0f5"
    draw.rounded_rectangle([px(166), px(238), px(310), px(264)], radius=px(13), fill=dark)
    draw.polygon([(px(312), px(222)), (px(350), px(251)), (px(312), px(280))], fill=accent)
    draw.rounded_rectangle([px(198), px(190), px(224), px(286)], radius=px(12), fill="#9bbce1")


def draw_badge(draw: ImageDraw.ImageDraw, text: str) -> None:
    if not text:
        return
    text = text[:3]
    draw.rounded_rectangle([px(326), px(300), px(421), px(395)], radius=px(16), fill="#ffc444")
    font_size = 58 if len(text) <= 2 else 38
    font = load_font(font_size)
    bbox = draw.textbbox((0, 0), text, font=font)
    text_width = bbox[2] - bbox[0]
    text_height = bbox[3] - bbox[1]
    x = px(373) - text_width // 2
    y = px(347) - text_height // 2 - px(4)
    draw.text((x, y), text, fill="#8f6210", font=font)


def create_icon(output: Path, symbol: str, badge: str) -> None:
    canvas = SIZE * SCALE
    image = Image.new("RGBA", (canvas, canvas), (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)

    draw_background(draw)
    draw_panel(draw)

    symbol_drawers = {
        "image": draw_image_symbol,
        "code": draw_code_symbol,
        "table": draw_table_symbol,
        "window": draw_window_symbol,
        "tool": draw_tool_symbol,
    }
    symbol_drawers[symbol](draw)
    draw_badge(draw, badge)

    image = image.resize((SIZE, SIZE), Image.Resampling.LANCZOS)
    output.parent.mkdir(parents=True, exist_ok=True)
    image.save(output)


def main() -> None:
    parser = argparse.ArgumentParser(description="Create a Ra3MapUtils toolbox entry icon.")
    parser.add_argument("--output", required=True, help="Output PNG path.")
    parser.add_argument("--symbol", choices=["image", "code", "table", "window", "tool"], default="window")
    parser.add_argument("--badge", default="", help="Optional 1-3 character corner badge.")
    args = parser.parse_args()

    create_icon(Path(args.output), args.symbol, args.badge)


if __name__ == "__main__":
    main()
