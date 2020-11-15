using CommandLineTools.Helpers;
using System.Linq;

namespace CommandLineTools.Tools
{
    public class DuplicateLinesTool : CommandLineFileTool, ICommandLineTool<DuplicateLinesOptions>
    {
        public int ExecuteCommand(DuplicateLinesOptions options)
        {
            var log = new VerboseLogger(options);
            if (options.Factor <= 1)
            {
                log.Error("Please specify a factor greater than 1");
            }
            log.Info($"reading file {options.InputFile}");
            var lines = _fileService.ReadLinesLazily(options.InputFile);
            var duplicated = lines.SelectMany(l => Enumerable.Repeat(l, options.Factor));
            _fileService.WriteAllLines(options.OutputFile, duplicated);
            return 0;
        }
    }
}