using CommandLine;

namespace CommandLineTools.Options
{
    [Verb("pwGen", HelpText = "Generate a password")]
    public class PasswordGenerationOptions : BaseOptions
    {
        [Option('l', "length", Required = true, HelpText = "How long the pw should be")]
        public int Length { get; set; }
    }
}