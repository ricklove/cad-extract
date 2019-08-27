using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CadExtract.Library.TablePatterns
{
    public class TablePattern
    {
        public string PatternDocument { get; internal set; }
        public string Name { get; internal set; }
        public List<TablePatternColumn> Columns { get; internal set; }

        public void UpdatePatternDocument(string patternDocument)
        {
            PatternDocument = patternDocument;
            Columns = TablePatternParser.ParseDataPatternColumns(patternDocument);
        }
    }

    public class TablePatternColumn
    {
        public string Name { get; set; }
        public bool IsRequired { get; set; }
        public string ColumnMatchPattern { get; set; }
        public string ValueMatchPattern { get; set; }

        private Regex _columnMatchRegex;
        public Regex ColumnMatchRegex => _columnMatchRegex ?? (_columnMatchRegex = new Regex(ColumnMatchPattern, RegexOptions.IgnoreCase));

        private Regex _valueMatchRegex;
        public Regex ValueMatchRegex => _valueMatchRegex ?? (_valueMatchRegex = new Regex(ValueMatchPattern, RegexOptions.IgnoreCase));

        public override string ToString() => $"{Name}\t {ColumnMatchPattern}\t {ValueMatchPattern}";
    }

    public static class TablePatternParser
    {
        public static TablePattern ParseDataPattern(string name, string patternDocument)
        {
            var columns = ParseDataPatternColumns(patternDocument);

            return new TablePattern()
            {
                PatternDocument = patternDocument,
                Name = name,
                Columns = columns
            };
        }

        internal static List<TablePatternColumn> ParseDataPatternColumns(string patternDocument)
        {
            var columnSections = patternDocument.Replace("\r\n", "\n").Replace("\n#", "\r").Split('\r').Select(x => x.Trim()).Where(x => !x.IsNullOrEmpty()).ToList();

            var columns = columnSections.Select(section =>
            {
                var lines = section.Split('\n').Select(x => x.Trim()).Where(x => !x.IsNullOrEmpty()).ToList();

                var title = lines[0].TrimEnd('?');
                var isRequired = !lines[0].EndsWith("?");
                var columnMatches = lines.Skip(1).Where(x => x.StartsWith("-")).Select(x => x.TrimStart('-', ' ')).ToList();
                var valueMatches = lines.Skip(1).Where(x => x.StartsWith("*")).Select(x => x.TrimStart('*', ' ')).ToList();

                return new TablePatternColumn()
                {
                    Name = title,
                    IsRequired = isRequired,
                    ColumnMatchPattern = "(?:" + columnMatches.Select(x => $"(?:{x})").ConcatString("|").Replace("  ", " ").Replace("  ", " ").Replace(" ", "\\s+") + ")",
                    ValueMatchPattern = "(?:" + valueMatches.Select(x => $"(?:{x})").ConcatString("|").Replace("  ", " ").Replace("  ", " ").Replace(" ", "\\s+") + ")",
                };
            }).ToList();
            return columns;
        }
    }

    public static class TablePattern_Samples
    {
        public static TablePattern BomPattern => TablePatternParser.ParseDataPattern("Bom", BomPatternText);
        public static string BomPatternText = @"
# ItemNumber

    - item
    - item no
    - item num
    - item #

    * [0-9]+

# PartNumber

    - part no
    - part num
    - part #

    * ([0-9][0-9A-Za-z]+)-([0-9A-Za-z]+)(?:-[0-9A-Za-z]+)*
    * Not Used

# Quantity

    - quan
    - qty

    * [0-9]+

# Description

    - description

";

        public static TablePattern WireHarnessPattern => TablePatternParser.ParseDataPattern("WireHarness", WireHarnessPatternText);
        public static string WireHarnessPatternText = @"
# ItemNumber                    

    - (item|wire|tag)\s?(no\.?|#|num)

	* [0-9]+

# PartNumber              

    - (part|pt.) (no\.?|#|num)
    - raw material

	* ([0-9][0-9A-Za-z]+)-([0-9A-Za-z]+)(?:-[0-9A-Za-z]+)*

# Quantity                

    - (qty|quan)                                                                                                                                  
	* [0-9]+    

# WireLength?              

    - (wire|cut|cutting) length

	* [0-9\.]+[""]?    

# Guage                   

    - (guage|ga.)                                                          

# Color                   

    - color                                                              

# WireTagging?            

    - wire (tag|stamp)                                                              

# WireUsage?              

    - wire usage                                                              

# WireArea?               

    - (wire|wiring) area    

# InsulationThickness?               

    - insu[a-z\.]* th[a-z\.]*

# TermA_PartNumber?       

    - #1 (terminal|term.) (part|pt.) (no\.?|#|num)

	* ([0-9][0-9A-Za-z]+)-([0-9A-Za-z]+)(?:-[0-9A-Za-z]+)*
    * None

# TermA_Description?      

    - #1 (terminal|term.) description                                                    

# TermA_Insulation?       

    - (heat|shrink|sleeve|part no\. 46 ?-|ins[a-z\.]*)

# TermA_TinnedEnd?        

    - #1 tinned                     

# TermA_StripLength?      

    - #1 strip (length|end)                     

# TermB_PartNumber?       

    - (#2 (terminal|term.) (part|pt.) (no\.?|#|num)|^part (no\.?|#|num))

	* ([0-9][0-9A-Za-z]+)-([0-9A-Za-z]+)(?:-[0-9A-Za-z]+)*   
    * None

# TermB_Description?      

    - #2 (terminal|term.) description

# TermB_Insulation?       

    - (heat|shrink|sleeve|part no\. 46 ?-|ins[a-z\.]*)

# TermB_TinnedEnd?        

    - #2 tinned                    

# TermB_StripLength?      
    
    - #2 strip (length|end)     

# ItemNumber_Duplicate?                    

    - (item|wire|tag)\s?(no\.?|#|num)

	* [0-9]+
";
    }

}
