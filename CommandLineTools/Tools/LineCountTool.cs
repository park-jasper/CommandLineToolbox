using System;
using System.Linq;
using CommandLineTools.Helpers;
using CommandLineTools.Options;

namespace CommandLineTools.Tools
{
    public class LineCountTool : CommandLineFileTool<LineCountOptions>
    {
        public override int ExecuteCommand(LineCountOptions options)
        {
            var log = new VerboseLogger(options);
            log.Info($"Reading file {options.InputFile}");
            var input = FileService.ReadLinesLazily(options.InputFile);
            var count = input.Count();
            Console.WriteLine($"File has {count} lines");

            return 0;
        }
    }
}