using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Backend.Helpers
{
    // Minimal PDF 1.4 writer: A4 pages, text in Helvetica / Helvetica-Bold, lines and filled rectangles.
    // Uses the standard Type1 fonts every PDF reader ships with, so no font embedding or third-party library is needed.
    // Coordinates are in points with the origin at the TOP-left of the page.
    public class SimplePdfDocument
    {
        public const float PageWidth=595f;
        public const float PageHeight=842f;

        private readonly List<StringBuilder> pages=new List<StringBuilder>();

        public SimplePdfDocument()
        {
            NewPage();
        }

        public int PageCount => pages.Count;

        private StringBuilder Current => pages[pages.Count - 1];

        public void NewPage()
        {
            pages.Add(new StringBuilder());
        }

        public void Text(float x, float y, string text, float size=10f, bool bold=false)
        {
            Current.Append(FormattableString.Invariant(
                $"BT /{(bold ? "F2" : "F1")} {size:0.##} Tf {x:0.##} {PageHeight - y:0.##} Td ({Escape(text)}) Tj ET\n"));
        }

        public void Line(float x1, float y1, float x2, float y2, float width=0.5f)
        {
            Current.Append(FormattableString.Invariant(
                $"{width:0.##} w {x1:0.##} {PageHeight - y1:0.##} m {x2:0.##} {PageHeight - y2:0.##} l S\n"));
        }

        // gray: 0 = black, 1 = white.
        public void FillRect(float x, float y, float width, float height, float gray)
        {
            Current.Append(FormattableString.Invariant(
                $"{gray:0.##} g {x:0.##} {PageHeight - y - height:0.##} {width:0.##} {height:0.##} re f 0 g\n"));
        }

        // Greedy word wrap using an average Helvetica glyph width; good enough for report text.
        public static List<string> Wrap(string text, float maxWidth, float size)
        {
            var maxChars=Math.Max(10, (int)(maxWidth / (size * 0.5f)));
            var lines=new List<string>();

            foreach (var paragraph in (text ?? string.Empty).Replace("\r", string.Empty).Split('\n'))
            {
                var line=new StringBuilder();
                foreach (var word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (line.Length > 0 && line.Length + 1 + word.Length > maxChars)
                    {
                        lines.Add(line.ToString());
                        line.Clear();
                    }

                    if (line.Length > 0)
                    {
                        line.Append(' ');
                    }
                    line.Append(word);
                }
                lines.Add(line.ToString());
            }

            return lines;
        }

        public static string Truncate(string text, float maxWidth, float size)
        {
            var maxChars=Math.Max(4, (int)(maxWidth / (size * 0.5f)));
            return text.Length <= maxChars ? text : text.Substring(0, maxChars - 3) + "...";
        }

        public byte[] Build()
        {
            var encoding=Encoding.Latin1;
            using var stream=new MemoryStream();
            var offsets=new List<long>();

            void Write(string s)
            {
                var bytes=encoding.GetBytes(s);
                stream.Write(bytes, 0, bytes.Length);
            }

            void BeginObject(int number)
            {
                offsets.Add(stream.Position);
                Write($"{number} 0 obj\n");
            }

            // Object layout: 1 catalog, 2 page tree, 3-4 fonts, then (page, content) pairs from 5.
            Write("%PDF-1.4\n%âãÏÓ\n");

            BeginObject(1);
            Write("<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");

            var kids=string.Join(" ", Enumerable.Range(0, pages.Count).Select(i=>$"{5 + 2 * i} 0 R"));
            BeginObject(2);
            Write($"<< /Type /Pages /Kids [{kids}] /Count {pages.Count} >>\nendobj\n");

            BeginObject(3);
            Write("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>\nendobj\n");

            BeginObject(4);
            Write("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>\nendobj\n");

            for (var i=0; i < pages.Count; i++)
            {
                BeginObject(5 + 2 * i);
                Write(FormattableString.Invariant(
                    $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {PageWidth:0} {PageHeight:0}] /Resources << /Font << /F1 3 0 R /F2 4 0 R >> >> /Contents {6 + 2 * i} 0 R >>\nendobj\n"));

                var content=encoding.GetBytes(pages[i].ToString());
                BeginObject(6 + 2 * i);
                Write($"<< /Length {content.Length} >>\nstream\n");
                stream.Write(content, 0, content.Length);
                Write("\nendstream\nendobj\n");
            }

            var xrefPosition=stream.Position;
            Write($"xref\n0 {offsets.Count + 1}\n");
            Write("0000000000 65535 f \n");
            foreach (var offset in offsets)
            {
                Write($"{offset:D10} 00000 n \n");
            }
            Write($"trailer\n<< /Size {offsets.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefPosition}\n%%EOF\n");

            return stream.ToArray();
        }

        private static string Escape(string text)
        {
            var builder=new StringBuilder(text.Length);
            foreach (var ch in text)
            {
                switch (ch)
                {
                    case '\\': builder.Append("\\\\"); break;
                    case '(': builder.Append("\\("); break;
                    case ')': builder.Append("\\)"); break;
                    default:
                        // Outside printable Latin-1 (incl. the 0x80-0x9F range WinAnsi maps differently) -> '?'.
                        builder.Append(ch < 32 || (ch >= 127 && ch < 160) || ch > 255 ? '?' : ch);
                        break;
                }
            }
            return builder.ToString();
        }
    }
}
