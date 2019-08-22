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

            // Reverse Rows
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

            // Reverse Rows
            for (var r = rMax; r >= 0; r--)
            {
                sb.Append("<tr>");

                for (var c = 0; c <= cMax; c++)
                {
                    var v = table.LineBoxes.Where(x => x.Col_Min <= c && x.Col_Max >= c && x.Row_Min <= r && x.Row_Max >= r).OrderBy(x => x.Col_Min).ThenBy(x => x.Row_Min).FirstOrDefault();

                    // Skip if in span
                    if (v?.Col_Min != c || v?.Row_Max != r) { continue; }

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
