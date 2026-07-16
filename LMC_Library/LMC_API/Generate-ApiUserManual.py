#!/usr/bin/env python3
"""Generate the external LASAL Motion Control API reference PDF.

The Markdown source remains internal development material.  The generated PDF
is the only manual shipped in the external distribution package.
"""

from __future__ import annotations

import argparse
import html
import re
import shutil
from pathlib import Path

from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER, TA_LEFT
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import ParagraphStyle, getSampleStyleSheet
from reportlab.lib.units import mm
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import (
    BaseDocTemplate,
    Frame,
    HRFlowable,
    ListFlowable,
    ListItem,
    PageBreak,
    PageTemplate,
    Paragraph,
    Preformatted,
    Spacer,
    Table,
    TableStyle,
)
from reportlab.platypus.tableofcontents import TableOfContents


PAGE_WIDTH, PAGE_HEIGHT = A4
LEFT_MARGIN = 19 * mm
RIGHT_MARGIN = 17 * mm
TOP_MARGIN = 19 * mm
BOTTOM_MARGIN = 17 * mm
CONTENT_WIDTH = PAGE_WIDTH - LEFT_MARGIN - RIGHT_MARGIN

BLUE = colors.HexColor("#123B66")
MID_BLUE = colors.HexColor("#26669A")
ACCENT = colors.HexColor("#00A6D6")
LIGHT_BLUE = colors.HexColor("#EAF5FB")
LIGHT_GRAY = colors.HexColor("#F3F5F7")
MID_GRAY = colors.HexColor("#66717C")
DARK = colors.HexColor("#20262C")
BORDER = colors.HexColor("#CBD4DC")
CODE_BG = colors.HexColor("#F6F8FA")


def register_fonts() -> None:
    font_root = Path(r"C:\Windows\Fonts")
    fonts = {
        "Malgun": font_root / "malgun.ttf",
        "Malgun-Bold": font_root / "malgunbd.ttf",
        "Consolas": font_root / "consola.ttf",
        "Consolas-Bold": font_root / "consolab.ttf",
    }
    for name, path in fonts.items():
        if not path.exists():
            raise FileNotFoundError(f"Required font not found: {path}")
        pdfmetrics.registerFont(TTFont(name, str(path)))

    pdfmetrics.registerFontFamily(
        "Malgun",
        normal="Malgun",
        bold="Malgun-Bold",
        italic="Malgun",
        boldItalic="Malgun-Bold",
    )
    pdfmetrics.registerFontFamily(
        "Consolas",
        normal="Consolas",
        bold="Consolas-Bold",
        italic="Consolas",
        boldItalic="Consolas-Bold",
    )


def create_styles() -> dict[str, ParagraphStyle]:
    base = getSampleStyleSheet()
    return {
        "body": ParagraphStyle(
            "Body",
            parent=base["BodyText"],
            fontName="Malgun",
            fontSize=9.2,
            leading=15.0,
            textColor=DARK,
            spaceAfter=4.5,
            wordWrap="CJK",
        ),
        "small": ParagraphStyle(
            "Small",
            fontName="Malgun",
            fontSize=7.4,
            leading=10.5,
            textColor=MID_GRAY,
            wordWrap="CJK",
        ),
        "cover_title": ParagraphStyle(
            "CoverTitle",
            fontName="Malgun-Bold",
            fontSize=30,
            leading=39,
            textColor=colors.white,
            alignment=TA_LEFT,
            wordWrap="CJK",
        ),
        "cover_subtitle": ParagraphStyle(
            "CoverSubtitle",
            fontName="Malgun",
            fontSize=12,
            leading=18,
            textColor=colors.HexColor("#DCECF5"),
            alignment=TA_LEFT,
            wordWrap="CJK",
        ),
        "cover_meta": ParagraphStyle(
            "CoverMeta",
            fontName="Malgun",
            fontSize=9.5,
            leading=15,
            textColor=DARK,
            wordWrap="CJK",
        ),
        "cover_note": ParagraphStyle(
            "CoverNote",
            fontName="Malgun",
            fontSize=7.4,
            leading=10.5,
            textColor=colors.HexColor("#C8DCE8"),
            wordWrap="CJK",
        ),
        "h1": ParagraphStyle(
            "H1",
            fontName="Malgun-Bold",
            fontSize=19,
            leading=25,
            textColor=BLUE,
            spaceBefore=8,
            spaceAfter=9,
            keepWithNext=True,
            wordWrap="CJK",
        ),
        "h2": ParagraphStyle(
            "H2",
            fontName="Malgun-Bold",
            fontSize=13.2,
            leading=18,
            textColor=MID_BLUE,
            spaceBefore=10,
            spaceAfter=6,
            keepWithNext=True,
            wordWrap="CJK",
        ),
        "h3": ParagraphStyle(
            "H3",
            fontName="Malgun-Bold",
            fontSize=10.8,
            leading=15,
            textColor=DARK,
            spaceBefore=7,
            spaceAfter=4,
            keepWithNext=True,
            wordWrap="CJK",
        ),
        "table_head": ParagraphStyle(
            "TableHead",
            fontName="Malgun-Bold",
            fontSize=7.7,
            leading=10.2,
            textColor=colors.white,
            wordWrap="CJK",
        ),
        "table_cell": ParagraphStyle(
            "TableCell",
            fontName="Malgun",
            fontSize=7.5,
            leading=10.7,
            textColor=DARK,
            wordWrap="CJK",
        ),
        "code": ParagraphStyle(
            "Code",
            fontName="Consolas",
            fontSize=6.65,
            leading=9.1,
            textColor=colors.HexColor("#17202A"),
            leftIndent=0,
            rightIndent=0,
            spaceBefore=2,
            spaceAfter=2,
        ),
        "toc_title": ParagraphStyle(
            "TocTitle",
            fontName="Malgun-Bold",
            fontSize=21,
            leading=28,
            textColor=BLUE,
            spaceAfter=14,
        ),
    }


def inline_markup(text: str) -> str:
    """Escape text and add conservative inline-code/bold markup."""
    chunks = re.split(r"(`[^`]+`)", text)
    rendered: list[str] = []
    for chunk in chunks:
        if chunk.startswith("`") and chunk.endswith("`"):
            value = html.escape(chunk[1:-1])
            rendered.append(
                '<font name="Consolas" size="7.8" color="#1C4E73">'
                + value
                + "</font>"
            )
        else:
            value = html.escape(chunk)
            value = re.sub(r"\*\*([^*]+)\*\*", r"<b>\1</b>", value)
            rendered.append(value)
    return "".join(rendered)


def paragraph(text: str, style: ParagraphStyle) -> Paragraph:
    return Paragraph(inline_markup(text), style)


def column_widths(column_count: int) -> list[float]:
    if column_count == 1:
        factors = [1.0]
    elif column_count == 2:
        factors = [0.42, 0.58]
    elif column_count == 3:
        factors = [0.25, 0.21, 0.54]
    elif column_count == 4:
        factors = [0.19, 0.18, 0.23, 0.40]
    else:
        factors = [1.0 / column_count] * column_count
    return [CONTENT_WIDTH * factor for factor in factors]


def make_table(rows: list[list[str]], styles: dict[str, ParagraphStyle]) -> Table:
    column_count = max(len(row) for row in rows)
    normalized = [row + [""] * (column_count - len(row)) for row in rows]
    data: list[list[Paragraph]] = []
    for row_index, row in enumerate(normalized):
        cell_style = styles["table_head"] if row_index == 0 else styles["table_cell"]
        data.append([paragraph(cell, cell_style) for cell in row])

    table = Table(
        data,
        colWidths=column_widths(column_count),
        repeatRows=1,
        hAlign="LEFT",
        splitByRow=1,
    )
    commands = [
        ("BACKGROUND", (0, 0), (-1, 0), BLUE),
        ("TEXTCOLOR", (0, 0), (-1, 0), colors.white),
        ("GRID", (0, 0), (-1, -1), 0.45, BORDER),
        ("VALIGN", (0, 0), (-1, -1), "TOP"),
        ("LEFTPADDING", (0, 0), (-1, -1), 5),
        ("RIGHTPADDING", (0, 0), (-1, -1), 5),
        ("TOPPADDING", (0, 0), (-1, -1), 4),
        ("BOTTOMPADDING", (0, 0), (-1, -1), 4),
    ]
    for row_index in range(1, len(data)):
        if row_index % 2 == 0:
            commands.append(("BACKGROUND", (0, row_index), (-1, row_index), LIGHT_GRAY))
    table.setStyle(TableStyle(commands))
    return table


def code_blocks(code: str, styles: dict[str, ParagraphStyle]) -> list[Table]:
    """Split long code listings into page-friendly shaded blocks."""
    lines = code.rstrip("\n").splitlines() or [""]
    blocks: list[Table] = []
    for start in range(0, len(lines), 27):
        chunk = "\n".join(lines[start : start + 27])
        pre = Preformatted(chunk, styles["code"], maxLineLength=94)
        block = Table([[pre]], colWidths=[CONTENT_WIDTH], hAlign="LEFT")
        block.setStyle(
            TableStyle(
                [
                    ("BACKGROUND", (0, 0), (-1, -1), CODE_BG),
                    ("BOX", (0, 0), (-1, -1), 0.6, BORDER),
                    ("LEFTPADDING", (0, 0), (-1, -1), 8),
                    ("RIGHTPADDING", (0, 0), (-1, -1), 8),
                    ("TOPPADDING", (0, 0), (-1, -1), 6),
                    ("BOTTOMPADDING", (0, 0), (-1, -1), 6),
                ]
            )
        )
        blocks.append(block)
    return blocks


def parse_table(lines: list[str], index: int) -> tuple[list[list[str]], int]:
    rows: list[list[str]] = []
    while index < len(lines) and lines[index].strip().startswith("|"):
        row = [cell.strip() for cell in lines[index].strip().strip("|").split("|")]
        if not all(re.fullmatch(r":?-{3,}:?", cell or "") for cell in row):
            rows.append(row)
        index += 1
    return rows, index


def parse_list(lines: list[str], index: int, ordered: bool, styles: dict[str, ParagraphStyle]):
    items: list[ListItem] = []
    pattern = r"^\s*\d+\.\s+" if ordered else r"^\s*-\s+"
    while index < len(lines) and re.match(pattern, lines[index]):
        value = re.sub(pattern, "", lines[index]).strip()
        items.append(ListItem(paragraph(value, styles["body"]), leftIndent=9))
        index += 1
    flowable = ListFlowable(
        items,
        bulletType="1" if ordered else "bullet",
        start="1",
        leftIndent=18,
        bulletFontName="Malgun",
        bulletFontSize=8,
        bulletColor=MID_BLUE,
        spaceAfter=5,
    )
    return flowable, index


def parse_markdown(source: Path, styles: dict[str, ParagraphStyle]):
    lines = source.read_text(encoding="utf-8").splitlines()
    story = build_cover(styles)

    # The Markdown header before the first explicit pagebreak is cover metadata.
    try:
        index = lines.index(r"\pagebreak") + 1
    except ValueError as exc:
        raise ValueError("Manual source must contain an initial \\pagebreak") from exc

    paragraph_buffer: list[str] = []

    def flush_paragraph() -> None:
        if paragraph_buffer:
            text = " ".join(part.strip() for part in paragraph_buffer).strip()
            if text:
                story.append(paragraph(text, styles["body"]))
            paragraph_buffer.clear()

    while index < len(lines):
        raw = lines[index]
        stripped = raw.strip()

        if stripped.startswith("```"):
            flush_paragraph()
            index += 1
            code_lines: list[str] = []
            while index < len(lines) and not lines[index].strip().startswith("```"):
                code_lines.append(lines[index])
                index += 1
            if index >= len(lines):
                raise ValueError("Unclosed code fence in manual source")
            story.extend(code_blocks("\n".join(code_lines), styles))
            story.append(Spacer(1, 3))
            index += 1
            continue

        if stripped == r"\pagebreak":
            flush_paragraph()
            story.append(PageBreak())
            index += 1
            continue

        if stripped == r"\toc":
            flush_paragraph()
            story.append(PageBreak())
            story.append(Paragraph("목차", styles["toc_title"]))
            toc = TableOfContents()
            toc.levelStyles = [
                ParagraphStyle(
                    "TOC1",
                    fontName="Malgun-Bold",
                    fontSize=10,
                    leading=15,
                    textColor=BLUE,
                    leftIndent=0,
                    firstLineIndent=0,
                    spaceBefore=4,
                ),
                ParagraphStyle(
                    "TOC2",
                    fontName="Malgun",
                    fontSize=8.2,
                    leading=12,
                    textColor=DARK,
                    leftIndent=14,
                    firstLineIndent=0,
                ),
                ParagraphStyle(
                    "TOC3",
                    fontName="Malgun",
                    fontSize=7.5,
                    leading=11,
                    textColor=MID_GRAY,
                    leftIndent=28,
                    firstLineIndent=0,
                ),
            ]
            toc.dotsMinLevel = 0
            story.append(toc)
            story.append(PageBreak())
            index += 1
            continue

        heading = re.match(r"^(#{1,3})\s+(.+)$", stripped)
        if heading:
            flush_paragraph()
            level = len(heading.group(1))
            text = heading.group(2).strip()
            story.append(Paragraph(inline_markup(text), styles[f"h{level}"]))
            if level == 1:
                story.append(HRFlowable(width="100%", thickness=1.0, color=ACCENT))
                story.append(Spacer(1, 4))
            index += 1
            continue

        if stripped.startswith("|"):
            flush_paragraph()
            rows, index = parse_table(lines, index)
            if rows:
                story.append(make_table(rows, styles))
                story.append(Spacer(1, 7))
            continue

        if re.match(r"^\s*-\s+", raw):
            flush_paragraph()
            value, index = parse_list(lines, index, False, styles)
            story.append(value)
            continue

        if re.match(r"^\s*\d+\.\s+", raw):
            flush_paragraph()
            value, index = parse_list(lines, index, True, styles)
            story.append(value)
            continue

        if not stripped:
            flush_paragraph()
            index += 1
            continue

        paragraph_buffer.append(stripped)
        index += 1

    flush_paragraph()
    return story


def build_cover(styles: dict[str, ParagraphStyle]):
    meta_data = [
        [paragraph("문서", styles["small"]), paragraph("API 기능 설명서", styles["cover_meta"])],
        [paragraph("적용 API", styles["small"]), paragraph("LasalMotionControlLib 0.9.1-preview", styles["cover_meta"])],
        [paragraph("환경", styles["small"]), paragraph("Windows / .NET Framework 4.8", styles["cover_meta"])],
        [paragraph("발행", styles["small"]), paragraph("2026-07-16", styles["cover_meta"])],
    ]
    meta = Table(meta_data, colWidths=[31 * mm, 92 * mm], hAlign="LEFT")
    meta.setStyle(
        TableStyle(
            [
                ("BACKGROUND", (0, 0), (-1, -1), colors.white),
                ("BOX", (0, 0), (-1, -1), 0.7, BORDER),
                ("INNERGRID", (0, 0), (-1, -1), 0.35, BORDER),
                ("VALIGN", (0, 0), (-1, -1), "MIDDLE"),
                ("LEFTPADDING", (0, 0), (-1, -1), 7),
                ("RIGHTPADDING", (0, 0), (-1, -1), 7),
                ("TOPPADDING", (0, 0), (-1, -1), 7),
                ("BOTTOMPADDING", (0, 0), (-1, -1), 7),
            ]
        )
    )
    return [
        Spacer(1, 24 * mm),
        Paragraph("LASAL", styles["cover_subtitle"]),
        Spacer(1, 3 * mm),
        Paragraph("Motion Control API<br/>기능 설명서", styles["cover_title"]),
        Spacer(1, 11 * mm),
        Paragraph(
            "LasalMotionControlLib 공개 C# API의<br/>"
            "기능, 호출 인자 UNIT과 반환값",
            styles["cover_subtitle"],
        ),
        Spacer(1, 50 * mm),
        meta,
        Spacer(1, 13 * mm),
        Paragraph(
            "API 기능과 호출에 필요한 값만 간단히 설명합니다.",
            styles["cover_note"],
        ),
        PageBreak(),
    ]


class ManualDocTemplate(BaseDocTemplate):
    def __init__(self, filename: str, styles: dict[str, ParagraphStyle]):
        super().__init__(
            filename,
            pagesize=A4,
            leftMargin=LEFT_MARGIN,
            rightMargin=RIGHT_MARGIN,
            topMargin=TOP_MARGIN,
            bottomMargin=BOTTOM_MARGIN,
            title="LASAL Motion Control API 기능 설명서",
            author="Elmo Motion Control / LASAL API Project",
            subject="LasalMotionControlLib API usage",
        )
        self.styles = styles
        self.heading_serial = 0
        frame = Frame(
            LEFT_MARGIN,
            BOTTOM_MARGIN,
            CONTENT_WIDTH,
            PAGE_HEIGHT - TOP_MARGIN - BOTTOM_MARGIN,
            id="body",
        )
        self.addPageTemplates(
            [PageTemplate(id="manual", frames=[frame], onPage=self.draw_page)]
        )

    def beforeDocument(self) -> None:
        self.heading_serial = 0

    def draw_page(self, canvas, doc) -> None:
        canvas.saveState()
        canvas.setTitle("LASAL Motion Control API 기능 설명서")
        if doc.page == 1:
            canvas.setFillColor(BLUE)
            canvas.rect(0, 0, PAGE_WIDTH, PAGE_HEIGHT, fill=1, stroke=0)
            canvas.setFillColor(ACCENT)
            canvas.rect(0, 0, 7 * mm, PAGE_HEIGHT, fill=1, stroke=0)
            canvas.restoreState()
            return

        canvas.setStrokeColor(BORDER)
        canvas.setLineWidth(0.5)
        canvas.line(LEFT_MARGIN, PAGE_HEIGHT - 12 * mm, PAGE_WIDTH - RIGHT_MARGIN, PAGE_HEIGHT - 12 * mm)
        # Use a Base14 font for repeated canvas text.  ReportLab's dynamic
        # TrueType subsetting is reserved for flowables; some PDF renderers can
        # otherwise omit a repeated header/footer subset on isolated pages.
        canvas.setFont("Helvetica-Bold", 7.2)
        canvas.setFillColor(BLUE)
        canvas.drawString(LEFT_MARGIN, PAGE_HEIGHT - 9.4 * mm, "LASAL MOTION CONTROL API")
        canvas.setFont("Helvetica", 7.2)
        canvas.setFillColor(MID_GRAY)
        canvas.drawRightString(PAGE_WIDTH - RIGHT_MARGIN, PAGE_HEIGHT - 9.4 * mm, "API GUIDE")

        canvas.line(LEFT_MARGIN, 11 * mm, PAGE_WIDTH - RIGHT_MARGIN, 11 * mm)
        canvas.setFont("Helvetica", 7.0)
        canvas.setFillColor(MID_GRAY)
        canvas.drawString(LEFT_MARGIN, 7.5 * mm, "LasalMotionControlLib 0.9.1-preview")
        canvas.drawRightString(PAGE_WIDTH - RIGHT_MARGIN, 7.5 * mm, str(doc.page))
        canvas.restoreState()

    def afterFlowable(self, flowable) -> None:
        if not isinstance(flowable, Paragraph):
            return
        style_name = flowable.style.name
        level_map = {"H1": 0, "H2": 1, "H3": 2}
        if style_name not in level_map:
            return

        level = level_map[style_name]
        text = flowable.getPlainText()
        self.heading_serial += 1
        key = f"heading-{self.heading_serial}"
        self.canv.bookmarkPage(key)
        self.canv.addOutlineEntry(text, key, level=level, closed=False)
        self.notify("TOCEntry", (level, text, self.page, key))


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "--source",
        type=Path,
        default=Path(__file__).with_name("API_USER_MANUAL_KO.md"),
    )
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--distribution-copy", type=Path)
    args = parser.parse_args()

    register_fonts()
    styles = create_styles()
    story = parse_markdown(args.source.resolve(), styles)

    args.output.parent.mkdir(parents=True, exist_ok=True)
    doc = ManualDocTemplate(str(args.output.resolve()), styles)
    doc.multiBuild(story)

    if args.distribution_copy:
        args.distribution_copy.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(args.output, args.distribution_copy)

    print(f"Generated: {args.output.resolve()}")
    if args.distribution_copy:
        print(f"Copied: {args.distribution_copy.resolve()}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
