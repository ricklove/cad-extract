﻿using CadExtract.Library.Importers;
using CadExtract.Library.Layout;
using CadExtract.Library.TablePatterns;
using System.Collections.Generic;
using System.Linq;

namespace CadExtract.Library.Process
{
    public class ExtractionData
    {
        public CadData CadData { get; set; }
        public LineTableData LineTableData { get; set; }
        public List<TableData> DataTables { get; set; }
        public List<CadText> MissingTexts { get; set; }
    }

    public static class ExtractionProcess
    {
        public static ExtractionData ExtractData(string filePath, params TablePattern[] patterns)
        {
            var cadData = NetDxfImporter.Import(filePath);
            var lineTableData = new LineTableData() { LineBoxes = LineBoxFinder.FindLineBoxesWithTexts(cadData.Lines, cadData.Texts, cadData.Circles) };
            lineTableData.LineBoxNeighbors = LineBoxNeighborsPushAlgorithm.Solve(lineTableData.LineBoxes);
            lineTableData.LineTables_Uncondensed = LineTablesLayout.FindLineTables(lineTableData.LineBoxNeighbors, shouldCondense: false);
            lineTableData.LineTables = LineTablesLayout.FindLineTables(lineTableData.LineBoxNeighbors, shouldCondense: true);

            var dataTables = patterns.SelectMany(p => lineTableData.LineTables.Select(lineTable => TablePatternDataExtraction.ExtractTable(lineTable, p))).Where(x => x != null && x.Rows.Any()).ToList();

            var allDataValues = dataTables.SelectMany(x => x.Rows.SelectMany(y => y.Values)).ToList();
            var missingTexts = cadData.Texts.Where(t => !allDataValues.Where(v => v.Value.Contains(t.Text) && v.SourceBounds.Intersects(t.Bounds)).Any()).ToList();
            // var missingTextsInTableArea = missingTexts.Where(x => dataTables.Where(d => x.Bounds.Intersects(d.SourceBounds)).Any()).ToList();

            var extractionData = new ExtractionData()
            {
                CadData = cadData,
                LineTableData = lineTableData,
                DataTables = dataTables,
                MissingTexts = missingTexts,
            };

            return extractionData;
        }
    }
}
