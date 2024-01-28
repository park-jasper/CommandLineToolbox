using CommandLine;

namespace CommandLineTools.Windows.Options
{
    [Verb("wholockme")]
    public class WhoLockMeOptions
    {
        [Option('f', "file", Required = true, HelpText = "File to check")]
        public string File { get; set; }
    }
}