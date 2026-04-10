using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;

namespace PmasApiWpfTestApp.Services
{
    internal sealed class XlsxSheetData
    {
        public XlsxSheetData(string name, IList<IList<string>> rows)
        {
            Name = name;
            Rows = rows ?? new List<IList<string>>();
        }

        public string Name { get; private set; }
        public IList<IList<string>> Rows { get; private set; }
    }

    internal static class SimpleXlsxExporter
    {
        public static void Save(string filePath, IList<XlsxSheetData> sheets)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path is empty.", nameof(filePath));
            }

            if (sheets == null || sheets.Count == 0)
            {
                throw new ArgumentException("No sheets to export.", nameof(sheets));
            }

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using (var stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, false, Encoding.UTF8))
            {
                WriteEntry(archive, "[Content_Types].xml", BuildContentTypesXml(sheets.Count));
                WriteEntry(archive, "_rels/.rels", BuildRootRelsXml());
                WriteEntry(archive, "xl/workbook.xml", BuildWorkbookXml(sheets));
                WriteEntry(archive, "xl/_rels/workbook.xml.rels", BuildWorkbookRelsXml(sheets.Count));
                WriteEntry(archive, "xl/styles.xml", BuildStylesXml());

                for (var i = 0; i < sheets.Count; i++)
                {
                    var sheet = sheets[i];
                    var sheetPath = string.Format(CultureInfo.InvariantCulture, "xl/worksheets/sheet{0}.xml", i + 1);
                    WriteEntry(archive, sheetPath, BuildWorksheetXml(sheet.Rows));
                }
            }
        }

        private static void WriteEntry(ZipArchive archive, string entryPath, string xmlText)
        {
            var entry = archive.CreateEntry(entryPath, CompressionLevel.Optimal);
            using (var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false)))
            {
                writer.Write(xmlText);
            }
        }

        private static string BuildContentTypesXml(int sheetCount)
        {
            var builder = new StringBuilder();
            builder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            builder.Append("<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">");
            builder.Append("<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>");
            builder.Append("<Default Extension=\"xml\" ContentType=\"application/xml\"/>");
            builder.Append("<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>");
            builder.Append("<Override PartName=\"/xl/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml\"/>");
            for (var i = 1; i <= sheetCount; i++)
            {
                builder.AppendFormat(
                    CultureInfo.InvariantCulture,
                    "<Override PartName=\"/xl/worksheets/sheet{0}.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>",
                    i);
            }

            builder.Append("</Types>");
            return builder.ToString();
        }

        private static string BuildRootRelsXml()
        {
            return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                   "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                   "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/>" +
                   "</Relationships>";
        }

        private static string BuildWorkbookXml(IList<XlsxSheetData> sheets)
        {
            var builder = new StringBuilder();
            builder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            builder.Append("<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" ");
            builder.Append("xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">");
            builder.Append("<sheets>");
            for (var i = 0; i < sheets.Count; i++)
            {
                var sheetName = SanitizeSheetName(sheets[i].Name, i + 1);
                builder.AppendFormat(
                    CultureInfo.InvariantCulture,
                    "<sheet name=\"{0}\" sheetId=\"{1}\" r:id=\"rId{1}\"/>",
                    EscapeXml(sheetName),
                    i + 1);
            }

            builder.Append("</sheets>");
            builder.Append("</workbook>");
            return builder.ToString();
        }

        private static string BuildWorkbookRelsXml(int sheetCount)
        {
            var builder = new StringBuilder();
            builder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            builder.Append("<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">");
            for (var i = 1; i <= sheetCount; i++)
            {
                builder.AppendFormat(
                    CultureInfo.InvariantCulture,
                    "<Relationship Id=\"rId{0}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet{0}.xml\"/>",
                    i);
            }

            builder.AppendFormat(
                CultureInfo.InvariantCulture,
                "<Relationship Id=\"rId{0}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\"/>",
                sheetCount + 1);
            builder.Append("</Relationships>");
            return builder.ToString();
        }

        private static string BuildStylesXml()
        {
            return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                   "<styleSheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">" +
                   "<fonts count=\"1\"><font><sz val=\"11\"/><name val=\"Calibri\"/></font></fonts>" +
                   "<fills count=\"1\"><fill><patternFill patternType=\"none\"/></fill></fills>" +
                   "<borders count=\"1\"><border/></borders>" +
                   "<cellStyleXfs count=\"1\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\"/></cellStyleXfs>" +
                   "<cellXfs count=\"1\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\"/></cellXfs>" +
                   "<cellStyles count=\"1\"><cellStyle name=\"Normal\" xfId=\"0\" builtinId=\"0\"/></cellStyles>" +
                   "</styleSheet>";
        }

        private static string BuildWorksheetXml(IList<IList<string>> rows)
        {
            var builder = new StringBuilder();
            builder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            builder.Append("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">");
            builder.Append("<sheetData>");

            if (rows != null)
            {
                for (var r = 0; r < rows.Count; r++)
                {
                    var row = rows[r] ?? new List<string>();
                    var rowIndex = r + 1;
                    builder.AppendFormat(CultureInfo.InvariantCulture, "<row r=\"{0}\">", rowIndex);
                    for (var c = 0; c < row.Count; c++)
                    {
                        var cellRef = GetCellReference(c + 1, rowIndex);
                        var value = EscapeXml(row[c] ?? string.Empty);
                        builder.AppendFormat(
                            CultureInfo.InvariantCulture,
                            "<c r=\"{0}\" t=\"inlineStr\"><is><t xml:space=\"preserve\">{1}</t></is></c>",
                            cellRef,
                            value);
                    }

                    builder.Append("</row>");
                }
            }

            builder.Append("</sheetData>");
            builder.Append("</worksheet>");
            return builder.ToString();
        }

        private static string GetCellReference(int column, int row)
        {
            var col = new StringBuilder();
            var n = column;
            while (n > 0)
            {
                var remainder = (n - 1) % 26;
                col.Insert(0, (char)('A' + remainder));
                n = (n - 1) / 26;
            }

            return col.ToString() + row.ToString(CultureInfo.InvariantCulture);
        }

        private static string SanitizeSheetName(string name, int sheetIndex)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "Sheet" + sheetIndex.ToString(CultureInfo.InvariantCulture);
            }

            var invalidChars = new[] { ':', '\\', '/', '?', '*', '[', ']' };
            var sanitized = name;
            foreach (var ch in invalidChars)
            {
                sanitized = sanitized.Replace(ch, '_');
            }

            if (sanitized.Length > 31)
            {
                sanitized = sanitized.Substring(0, 31);
            }

            if (sanitized.Length == 0)
            {
                sanitized = "Sheet" + sheetIndex.ToString(CultureInfo.InvariantCulture);
            }

            return sanitized;
        }

        private static string EscapeXml(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(value.Length + 16);
            foreach (var ch in value)
            {
                if (!XmlConvert.IsXmlChar(ch))
                {
                    continue;
                }

                switch (ch)
                {
                    case '&':
                        builder.Append("&amp;");
                        break;
                    case '<':
                        builder.Append("&lt;");
                        break;
                    case '>':
                        builder.Append("&gt;");
                        break;
                    case '\"':
                        builder.Append("&quot;");
                        break;
                    case '\'':
                        builder.Append("&apos;");
                        break;
                    default:
                        builder.Append(ch);
                        break;
                }
            }

            return builder.ToString();
        }
    }
}
