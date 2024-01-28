using System.Collections;
using System.Collections.Generic;
using CommandLine;

namespace CommandLineTools.Options
{
    [Verb("reduceCsv")]
    public class ReduceCsvOptions : FileOptions
    {
        [Option('i', "inFile", Required = true, HelpText = "Csv file to reduce")]
        public string InFile { get; set; }

        [Option('s', "separator", Required = false,
            HelpText = "character by which to split the lines. Comma is used if not specified")]
        public char Separator { get; set; } = ',';

        [Option("keepColumns", Required = false, HelpText = "list of columns to keep in the output file")]
        public IEnumerable<string> ColumnsToKeep { get; set; }

        [Option("removeColumns", Required = false, HelpText = "list of columns to not copy to the output file")]
        public IEnumerable<string> ColumnsToRemove { get; set; }
    }
}