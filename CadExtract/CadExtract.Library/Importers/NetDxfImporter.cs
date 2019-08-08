using netDxf;
using netDxf.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CadExtract.Library.Importers
{
    public class NetDxfImporter
    {
        public static CadData Import(string path)
        {
            DxfDocument doc;

            try
            {
                PreProcessDxfFile(path);

                var debug_version = DxfDocument.CheckDxfFileVersion(path, out var debug_isBinary);

                // Handle failure to load (with no exception)
                doc = null;
                var innerException = null as Exception;
                ActionExtensions.InvokeWithTimeout(() =>
                {
                    try
                    {
                        doc = DxfDocument.Load(path);
                    }
                    catch (Exception ex)
                    {
                        innerException = ex;
                    }
                }, 1000);

                if (doc == null)
                {
                    throw new LibraryException($@"doc did not load: {innerException?.Message}", innerException);
                }
            }
            catch (Exception ex)
            {
                throw new LibraryException($@"DxfNet could not load the Dxf File: {ex.Message}
{nameof(path)}={path}", ex);
            }

            var inserts = doc.Inserts.ToList();
            var insertsBlocks = inserts.Select(x => x.Block).ToList();

            var docUnits = doc.DrawingVariables.InsUnits;

            var insertsData = inserts.Select(x => GetInsertItems(x, docUnits)).ToList();
            var debug_insertsData = InsertData.Debug_Stats_Header + "\r\n" + insertsData.Select(x => x.Debug_Stats).ConcatString("\r\n");
            var debug_insertsTexts = insertsData.Select(x => x.Debug_Texts).ConcatString("\r\n");
            var debug_textsData = doc.Texts.Select(x => x.Value).ConcatString("\r\n");
            var debug_mtextsData = doc.MTexts.Select(x => x.Value).ConcatString("\r\n");
            var debug_docData = $"{doc.Texts.Count()} {doc.MTexts.Count()} {doc.AttributeDefinitions.Where(x => x.Flags == AttributeFlags.Constant).Count()} {doc.Lines.Count()} {doc.Circles.Count()}";

            var flatData = FlattenInsertsData(insertsData);

            //// TEST: No Inserts
            //var flatData = new FlatData()
            //{
            //    AttributeDefinitions = new List<InsertTransform<AttributeDefinition>>(),
            //    Attributes = new List<InsertTransform<netDxf.Entities.Attribute>>(),
            //    Circles = new List<InsertTransform<Circle>>(),
            //    Debug_Name = "",
            //    Lines = new List<InsertTransform<Line>>(),
            //    MTexts = new List<InsertTransform<MText>>(),
            //    Texts = new List<InsertTransform<Text>>(),
            //};

            flatData.Texts.AddRange(doc.Texts.Select(GetInsertOffset_Root));
            flatData.MTexts.AddRange(doc.MTexts.Select(GetInsertOffset_Root));
            flatData.AttributeDefinitions.AddRange(doc.AttributeDefinitions.Where(x => x.Flags == AttributeFlags.Constant).Select(GetInsertOffset_Root));
            flatData.Lines.AddRange(doc.Lines.Select(GetInsertOffset_Root));
            flatData.Circles.AddRange(doc.Circles.Select(GetInsertOffset_Root));

            var d = new CadData
            {
                Texts = flatData.Texts.Select(x => new CadText
                {
                    Debug_Raw = x,
                    Bounds = GetBounds(x),
                    FontHeight = GetFontHeight(x, (float)x.Value.Height),
                    Context = GetContextPath(x),
                    Text = x.Value.Value,
                }).Concat(
                    flatData.MTexts.Select(x => new CadText
                    {
                        Debug_Raw = x,
                        Bounds = GetBounds(x),
                        FontHeight = GetFontHeight(x, (float)x.Value.Height),
                        Context = GetContextPath(x),
                        Text = x.Value.PlainText(),
                    })
                ).Concat(
                    flatData.AttributeDefinitions.Select(x => new CadText
                    {
                        Debug_Raw = x,
                        Bounds = GetBounds(x),
                        FontHeight = GetFontHeight(x, (float)x.Value.Height),
                        Context = GetContextPath(x),
                        Text = x.Value.Value + "",
                    })
                ).Concat(
                    flatData.Attributes.Select(x => new CadText
                    {
                        Debug_Raw = x,
                        Bounds = GetBounds(x.Value),
                        FontHeight = (float)x.Value.Height,
                        Context = GetContextPath(x),
                        Text = x.Value.Value + "",
                    })
                ).ToList()
            };

            d.Texts = SplitSpacedOutTexts(d.Texts);
            // Remove Duplicate Texts
            d.Texts = d.Texts.GroupBy(x => new { x.Text, xPos = Math.Round(x.Bounds.Min.X, 2), yPos = Math.Round(x.Bounds.Min.Y, 2) }).Select(g => g.First()).ToList();
            d.Texts.ForEach(x => x.Text = CleanText(x.Text));

            d.Lines = flatData.Lines.Select(x => new CadLine
            {
                Debug_Raw = x,
                Start = x.Transform(x.Value.StartPoint.ToGeometryVector()),
                End = x.Transform(x.Value.EndPoint.ToGeometryVector()),
                Context = GetContextPath(x),
            }).ToList();

            d.Circles = flatData.Circles.Select(x => new CadCircle
            {
                Debug_Raw = x,
                Circle = new Geometry.Circle(x.Transform(x.Value.Center.ToGeometryVector()).X,
                    x.Transform(x.Value.Center.ToGeometryVector()).Y,
                    x.Transform(new System.Numerics.Vector2((float)x.Value.Radius, (float)x.Value.Radius)).X),
                Context = GetContextPath(x),
            }).ToList();

            //var debug_item = d.Texts.Where(x => x.Text == "21-21571-00").ToList();
            //var debug_lines = !debug_item.Any() ? null : d.Lines.Where(x => Common.Geometry.MathExt.Intersects(debug_item.First().Bounds, Geometry.Bounds.FromPoints(x.Start, x.End))).ToList();


            return d;


        }

        private static List<CadText> SplitSpacedOutTexts(List<CadText> texts)
        {
            return texts.SelectMany(x =>
            {
                if (!x.Text.Contains("  ") || x.Text.Contains("\t")) { return new[] { x }; }
                // Split Texts by multiple spaces

                var text = x.Text.Replace("\t", "    ");
                var width = x.Bounds.Size.X;
                var widthPerChar = width / text.Length;

                var regex_longSpaces = new Regex(@"(  (?:\s)*)");
                var matches = regex_longSpaces.Matches(text);

                var outTexts = new List<CadText>(1);

                void AddText(string t, int iStart)
                {
                    var vMin = x.Bounds.Min;
                    var vMax = x.Bounds.Max;

                    var xMin = vMin.X + iStart * widthPerChar;
                    var xMax = vMin.X + (iStart + t.Length) * widthPerChar;
                    vMin = new System.Numerics.Vector2(xMin, vMin.Y);
                    vMax = new System.Numerics.Vector2(xMax, vMax.Y);

                    outTexts.Add(new CadText()
                    {
                        Text = t,
                        Bounds = new Geometry.Bounds(vMin, vMax),
                        Context = x.Context,
                        Debug_Raw = x.Debug_Raw,
                        FontHeight = x.FontHeight,
                    });
                }

                var i = 0;
                foreach (Match m in matches)
                {
                    var t = text.Substring(i, m.Index - i);
                    if (t.Length > 0)
                    {
                        AddText(t, i);
                    }

                    i = m.Index + m.Length;
                }

                if (i < text.Length - 1)
                {
                    AddText(text.Substring(i, text.Length - i), i);
                }

                return outTexts.ToArray();
            }).ToList();
        }

        private static void PreProcessDxfFile(string path)
        {
            // Prevent dxf bug with extra space (not allowed by .netDxf)
            var fileText = System.IO.File.ReadAllText(path);

            var eol = "\r?\n";
            var regex_tagWithSpace = new Regex($"(AcDbAttributeDefinition{eol}  3{eol}[^\n]+{eol}  2{eol})((?:[^\n ]+ )+[^\n ]+)");
            var debug_match = regex_tagWithSpace.Matches(fileText).Cast<Match>().ToList();
            var fileTextFixed = regex_tagWithSpace.Replace(fileText, x => x.Groups[1].Value + x.Groups[2].Value.Replace(" ", ""));

            //            var fileTextFixed = fileText.Replace(@"AcDbAttributeDefinition
            //  3
            //PART NUMBER :
            //  2
            //PART NUMBER", @"AcDbAttributeDefinition
            //  3
            //PART NUMBER :
            //  2
            //PARTNUMBER");

            if (fileText.Length != fileTextFixed.Length)
            {
                System.IO.File.WriteAllText(path, fileTextFixed);
            }
        }

        public static string CleanText(string v)
        {
            //if (v.Trim() == "XX")
            //{
            //    var breakdance = true;
            //}
            // %%D
            return v
                .Replace("%%p", "±")
                .Replace("%%P", "±")
                .Replace("%%D", "°")
                .Replace("%%d", "°")
                .Replace("%%U", "")
                .Replace("%%u", "")
                .Trim();
        }

        public class FlatData
        {
            // public List<InsertData> Inserts { get; set; }

            public List<InsertTransform<netDxf.Entities.Attribute>> Attributes { get; set; }

            public List<InsertTransform<Text>> Texts { get; set; }
            public List<InsertTransform<MText>> MTexts { get; set; }
            public List<InsertTransform<AttributeDefinition>> AttributeDefinitions { get; set; }
            public List<InsertTransform<Line>> Lines { get; set; }
            public List<InsertTransform<Circle>> Circles { get; set; }


            public string Debug_Name { get; set; }

            public override string ToString() => Debug_Name;
        }

        public class InsertTransform<T>
        {
            public InsertTransform<T> Inner { get; set; }

            public T Value { get; set; }
            public List<Insert> InsertLevels { get; set; }
            public Func<System.Numerics.Vector2, System.Numerics.Vector2> Transform { get; set; }
            public string Debug_TransformStr { get; set; }

            public override string ToString() => Value + "";
        }

        private static FlatData FlattenInsertsData(List<InsertData> insertsData)
        {
            var items = insertsData.Select(x => FlattenInsertsData(x)).ToList();

            return new FlatData()
            {
                Attributes = items.SelectMany(x => x.Attributes).ToList(),
                AttributeDefinitions = items.SelectMany(x => x.AttributeDefinitions).ToList(),
                Texts = items.SelectMany(x => x.Texts).ToList(),
                MTexts = items.SelectMany(x => x.MTexts).ToList(),
                Lines = items.SelectMany(x => x.Lines).ToList(),
                Circles = items.SelectMany(x => x.Circles).ToList(),
            };
        }

        private static FlatData FlattenInsertsData(InsertData data)
        {
            var inner = data.Inserts.Select(x => FlattenInsertsData(x)).ToList();

            return new FlatData()
            {
                Debug_Name = data.Insert.Block.Name,
                Attributes = Combine(data, inner, x => x.Attributes, x => x.Attributes, skipScale: true, skipPos: true),
                AttributeDefinitions = Combine(data, inner, x => x.AttributeDefinitions, x => x.AttributeDefinitions),
                Texts = Combine(data, inner, x => x.Texts, x => x.Texts),
                MTexts = Combine(data, inner, x => x.MTexts, x => x.MTexts),
                Lines = Combine(data, inner, x => x.Lines, x => x.Lines),
                Circles = Combine(data, inner, x => x.Circles, x => x.Circles),
            };
        }

        private static List<InsertTransform<T>> Combine<T>(InsertData data, List<FlatData> inner,
            Func<InsertData, List<T>> selector,
            Func<FlatData, List<InsertTransform<T>>> selector2,
            bool skipScale = false, bool skipPos = false)
            where T : netDxf.DxfObject => selector(data).Select(x => Flatten(data.Insert, x, data.BaseUnits, skipScale, skipPos)).Concat(inner.SelectMany(x => selector2(x).Select(x2 => Flatten(data.Insert, x2, data.BaseUnits)))).ToList();

        private static InsertTransform<T> Flatten<T>(Insert insert, T x, netDxf.Units.DrawingUnits baseUnits, bool skipScale, bool skipPos) where T : netDxf.DxfObject
        {
            var blockRecordUnits = insert.Block.Record.Units;
            var unitFactorScale = skipScale ? 1 : (float)netDxf.Units.UnitHelper.ConversionFactor(blockRecordUnits, baseUnits);
            var scale = skipScale ? new System.Numerics.Vector2(1, 1) : insert.Scale.ToGeometryVector();
            var pos = skipPos ? new System.Numerics.Vector2(0, 0) : insert.Position.ToGeometryVector();

            return new InsertTransform<T>()
            {
                Value = x,
                Transform = v => (v * scale * unitFactorScale) + pos,
                Debug_TransformStr = string.Format("(({0}) * s{1} * {3}) + p{2}", "POS", scale, pos, unitFactorScale),
                InsertLevels = new List<Insert>() { insert },
                Inner = null,
            };
        }

        private static InsertTransform<T> Flatten<T>(Insert insert, InsertTransform<T> inner, netDxf.Units.DrawingUnits baseUnits) where T : netDxf.DxfObject
        {
            var blockRecordUnits = insert.Block.Record.Units;
            var unitFactorScale = (float)netDxf.Units.UnitHelper.ConversionFactor(blockRecordUnits, baseUnits);
            var scale = insert.Scale.ToGeometryVector();
            var pos = insert.Position.ToGeometryVector();

            return new InsertTransform<T>()
            {
                Value = inner.Value,
                Transform = v => (inner.Transform(v) * scale * unitFactorScale) + pos,
                Debug_TransformStr = string.Format("(({0}) * s{1} * {3}) + p{2}", inner.Debug_TransformStr, scale, pos, unitFactorScale),
                InsertLevels = new Insert[] { insert }.Concat(inner.InsertLevels).ToList(),
                Inner = inner,
            };
        }

        private static InsertTransform<T> GetInsertOffset_Root<T>(T x) where T : netDxf.DxfObject
        {
            return new InsertTransform<T>()
            {
                Value = x,
                Transform = v => v,
                // Debug_TransformStr = "(" + x.RawPosition + ")",
                InsertLevels = new List<Insert>(0),
                Inner = null,
            };
        }

        public class InsertData
        {
            public Insert Insert { get; set; }
            public netDxf.Units.DrawingUnits BaseUnits { get; set; }

            public List<InsertData> Inserts { get; set; }

            public List<netDxf.Entities.Attribute> Attributes { get; set; }
            public List<AttributeDefinition> AttributeDefinitions { get; set; }

            public List<Text> Texts { get; set; }
            public List<MText> MTexts { get; set; }
            public List<netDxf.Entities.Line> Lines { get; set; }
            public List<netDxf.Entities.Circle> Circles { get; set; }

            public static string Debug_Stats_Header => $"Insert.Block.Name BaseUnits Inserts Attributes AttributeDefinitions Texts MTexts Lines Circles".Replace(" ", "\t");
            public string Debug_Stats => $"{Insert.Block.Name} {BaseUnits} {Inserts.Count} {Attributes.Count} {AttributeDefinitions.Count} {Texts.Count} {MTexts.Count} {Lines.Count} {Circles.Count}".Replace(" ", "\t");
            public string Debug_Texts => $"{Texts.Select(x => x.Value).ConcatString(" ")} {MTexts.Select(x => x.Value).ConcatString(" ")} {Attributes.Select(x => x.Value + "").ConcatString(" ")}".Replace(" ", "\t");

            public override string ToString() => Insert.Block.Name;
        }

        private static InsertData GetInsertItems(Insert insert, netDxf.Units.DrawingUnits baseUnits)
        {
            if (insert.Block.Name.StartsWith("tutcod"))
            {
#pragma warning disable CS0219 // Variable is assigned but its value is never used
                var breakdance = true;
#pragma warning restore CS0219 // Variable is assigned but its value is never used
            }

            var all = insert.Block.Entities;
            var unitsOfContent = insert.Block.Record.Units;

            var inserts = all.Where(x => x.CodeName == "INSERT").Cast<Insert>().ToList();
            var insertsData = inserts.Select(x => GetInsertItems(x, unitsOfContent)).ToList();

            var attrs = insert.Attributes.ToList();

            var attrDefs = all.Where(x => x.CodeName == "ATTDEF").Cast<AttributeDefinition>().Where(x => x.Flags == AttributeFlags.Constant).ToList();
            var texts = all.Where(x => x.CodeName == "TEXT").Cast<Text>().ToList();
            var mtexts = all.Where(x => x.CodeName == "MTEXT").Cast<MText>().ToList();
            var lines = all.Where(x => x.CodeName == "LINE").Cast<netDxf.Entities.Line>().ToList();
            var circles = all.Where(x => x.CodeName == "CIRCLE").Cast<netDxf.Entities.Circle>().ToList();

            // Default Units is Inches
            if (baseUnits == netDxf.Units.DrawingUnits.Unitless) { baseUnits = netDxf.Units.DrawingUnits.Inches; }

            return new InsertData()
            {
                BaseUnits = baseUnits,
                Insert = insert,
                Inserts = insertsData,
                Attributes = attrs,
                AttributeDefinitions = attrDefs,
                Texts = texts,
                MTexts = mtexts,
                Lines = lines,
                Circles = circles,
            };
        }

        private static string GetContextPath<T>(InsertTransform<T> item) where T : DxfObject => item.InsertLevels.Aggregate(new StringBuilder(), (sb, x) => sb.Append(GetInsertName(x) + "/")).ToString();

        private static string GetInsertName(Insert x) => string.Format(@"{0}({2})", x.Owner.Name, x.Owner.Entities.IndexOf(x), x.Block.Name);

        private static string GetName(DxfObject x)
        {
            var pName = x.GetType().GetProperty("Name");
            if (pName != null)
            {
                return pName.GetValue(x, null) as string;
            }

            var pValue = x.GetType().GetProperty("Value");
            if (pValue != null)
            {
                return pValue.GetValue(x, null) as string;
            }

            return x.CodeName;
        }

        private static float GetFontHeight<T>(InsertTransform<T> x, float fontHeight) where T : DxfObject => Math.Abs((x.Transform(new System.Numerics.Vector2(0, 1)).Y - x.Transform(new System.Numerics.Vector2(0, 0)).Y) * fontHeight);

        private static Geometry.Bounds GetBounds<T>(InsertTransform<T> x) where T : DxfObject
        {
            var b = GetBounds(x.Value);
            return new Geometry.Bounds(x.Transform(b.Min), x.Transform(b.Max));
        }

        private static Geometry.Bounds GetBounds(DxfObject dxfObject)
        {
            if (dxfObject is MText mText)
            {
                return NetDxfSizeCalculator.CalculateBounds(
                    xLeft: (float)mText.Position.X,
                    yBottom: (float)mText.Position.Y,
                    isMText: true,
                    text: mText.Value,
                    // textHeightCache: mtext.Hei,
                    // textWidthCache: GetDoubleValueOrNull(DxfPartKind.TextWidthCache),
                    textWidth: (float)mText.RectangleWidth,
                    fontHeight: (float)mText.Height,
                    attachmentPoint: mText.AttachmentPoint
                );
            }
            else if (dxfObject is Text text)
            {
                var bounds = NetDxfSizeCalculator.CalculateBounds(
                    xLeft: (float)text.Position.X,
                    yBottom: (float)text.Position.Y,
                    isMText: false,
                    text: text.Value,
                    fontHeight: (float)text.Height
                );

                bounds = GetTextAlignmentOffset(bounds, text.Alignment, text.Value);
                return bounds;
            }
            else if (dxfObject is netDxf.Entities.Attribute attrib)
            {
                var bounds = NetDxfSizeCalculator.CalculateBounds(
                    xLeft: (float)attrib.Position.X,
                    yBottom: (float)attrib.Position.Y,
                    // xLeft: attrib.AbsolutePosition.X + attrib.Definition.AbsolutePosition.X,
                    // yBottom: attrib.AbsolutePosition.Y + attrib.Definition.AbsolutePosition.Y,
                    // xLeft: attrib.Definition.AbsolutePosition.X,
                    // yBottom: attrib.Definition.AbsolutePosition.Y,
                    isMText: false,
                    text: attrib.Value + "",
                    fontHeight: (float)attrib.Height
                );

                bounds = GetTextAlignmentOffset(bounds, attrib.Alignment, attrib.Value + "");
                // Don't know why, but attrib needs extra Y
                // bounds = bounds.Translate(new System.Numerics.Vector2(0, bounds.YSize * 0.5));
                return bounds;
            }
            else if (dxfObject is AttributeDefinition attdef)
            {
                var bounds = NetDxfSizeCalculator.CalculateBounds(
                    xLeft: (float)attdef.Position.X,
                    yBottom: (float)attdef.Position.Y,
                    isMText: false,
                    text: attdef.Value + "",
                    fontHeight: (float)attdef.Height
                );

                bounds = GetTextAlignmentOffset(bounds, attdef.Alignment, attdef.Value + "");
                // Don't know why, but attrib needs extra Y
                // bounds = bounds.Translate(new System.Numerics.Vector2(0, bounds.YSize * 0.5));
                return bounds;
            }

            throw new ArgumentException("Unknown Element");
        }

        private static Geometry.Bounds GetTextAlignmentOffset(Geometry.Bounds bounds, TextAlignment alignment, string text)
        {
            if (text == "ITEM")
            {
#pragma warning disable CS0219 // Variable is assigned but its value is never used
                var breakdance = true;
#pragma warning restore CS0219 // Variable is assigned but its value is never used
            }

            if (alignment.ToString().Contains("Top"))
            {
                // bounds = bounds.Translate(new System.Numerics.Vector2(0, bounds.YSize));
            }

            if (alignment.ToString().Contains("Middle"))
            {
                bounds = bounds.Translate(new System.Numerics.Vector2(0, bounds.Size.Y * 0.5f));
            }

            if (alignment.ToString().Contains("Baseline"))
            {
                bounds = bounds.Translate(new System.Numerics.Vector2(0, bounds.Size.Y * 0.8f));
            }

            if (alignment.ToString().Contains("Bottom"))
            {
                bounds = bounds.Translate(new System.Numerics.Vector2(0, bounds.Size.Y));
            }

            if (alignment.ToString().Contains("Center") || alignment == TextAlignment.Middle)
            {
                bounds = bounds.Translate(new System.Numerics.Vector2(-bounds.Size.X * 0.5f, 0));
            }

            if (alignment.ToString().Contains("Right"))
            {
                bounds = bounds.Translate(new System.Numerics.Vector2(-bounds.Size.X, 0));
            }

            return bounds;
        }

    }

    public static class NetDxfImporterHelpers
    {
        public static System.Numerics.Vector2 ToGeometryVector(this Vector3 v) => new System.Numerics.Vector2((float)v.X, (float)v.Y);
        public static System.Numerics.Vector2 ToGeometryVector(this Vector2 v) => new System.Numerics.Vector2((float)v.X, (float)v.Y);
    }
}
