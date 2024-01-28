using CommandLine;

namespace CommandLineTools.Options
{
    [Verb("lineCount")]
    public class LineCountOptions : BaseOptions
    {
        [Option('i', "in", Required = true)]
        public string InputFile { get; set; }
    }
}