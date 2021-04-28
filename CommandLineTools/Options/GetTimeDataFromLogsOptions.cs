using System.Collections.Generic;
using CommandLine;

namespace CommandLineTools.Options
{
    [Verb("timeFromLogs", HelpText="Extract Time Data from Bluehands Logging Extensions Logs")]
    public class GetTimeDataFromLogsOptions : BaseOptions
    {
        [Option('o', "out", HelpText="File to write the report to", Required = true)]
        public string OutFile { get; set; }

        [Option('i', "in", HelpText="Files to get the logs from. If not specified reads all .log files from the current directory", Required = false)]
        public IEnumerable<string> InFiles { get; set; }
    }
}