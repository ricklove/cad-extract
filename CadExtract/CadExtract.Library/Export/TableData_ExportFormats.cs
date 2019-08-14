using CadExtract.Library.Layout;
using CadExtract.Library.TablePatterns;
using System.Linq;
using System.Text;

namespace CadExtract.Library
{
    public static class ExportFormat_Extensions
    {
        public static string ToClipboard_HtmlTableFormat(this TableData table)
        {
            var sb = new StringBuilder();
            sb.Append("<table>");

            sb.Append("<tr>");
            table.Columns.ForEach(col =>
            {
                sb.Append("<th>");
                sb.Append(col.Name);
                sb.Append("</th>");
            });
            sb.Append("</tr>");

            foreach (var row in table.Rows.AsEnumerable().Reverse())
            {
                var values_byColumn = row.Values.ToDictionary(x => table.Columns.IndexOf(x.Column), x => x);

                sb.Append("<tr>");

                for (var c = 0; c <= table.Columns.Count; c++)
                {
                    var v = values_byColumn.GetOrNull(c);
                    // Force excel text format
                    sb.Append($"<td style='mso-number-format:\"\\@\"'>");
                    sb.Append(v?.Value ?? "");
                    sb.Append("</td>");
                }

                sb.Append("</tr>");
            }

            sb.Append("</table>");

            var html = sb.ToString();
            return html;
        }

        public static string ToClipboard_HtmlTableFormat(this LineTable table)
        {
            var sb = new StringBuilder();
            sb.Append("<table>");

            var cMax = table.LineBoxes.Max(x => x.Col_Max);
            var rMax = table.LineBoxes.Max(x => x.Row_Max);

            var boxDict = table.LineBoxes.ToLookup(x => new { c = x.Col_Min, r = rMax - x.Row_Max }, x => x);

            for (var r = 0; r <= rMax; r++)
            {
                sb.Append("<tr>");

                for (var c = 0; c <= cMax; c++)
                {
                    var boxes = boxDict[new { c, r }];
                    var v = boxes.FirstOrDefault();
                    var span = $"{(v?.Row_Span > 1 ? $"rowspan={v.Row_Span}" : "")} {(v?.Col_Span > 1 ? $"colspan={v.Col_Span}" : "")}";

                    // Force excel text format
                    sb.Append($"<td {span} style='mso-number-format:\"\\@\"'>");
                    sb.Append(v?.CellText ?? "");
                    sb.Append("</td>");
                }

                sb.Append("</tr>");
            }

            sb.Append("</table>");

            var html = sb.ToString();
            return html;
        }
    }
}
