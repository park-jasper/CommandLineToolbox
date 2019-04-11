using System;
using CommandLineTools.Contracts;

namespace CommandLineTools.Tools
{
    public class InFileReplace : CommandLineFileTool
    {
        public InFileReplace(IFileService fileService) : base(fileService)
        {

        }

        public int ExecuteCommand(InFileReplaceOptions options)
        {
            var input = _fileService.ReadAllText(options.InputFile);
            if (!string.IsNullOrEmpty(options.ParseAsNewline))
            {
                options.TextToFind = options.TextToFind.Replace(options.ParseAsNewline, Environment.NewLine);
                options.ReplacementText = options.ReplacementText.Replace(options.ParseAsNewline, Environment.NewLine);
            }
            var replaced = input.Replace(options.TextToFind, options.ReplacementText);
            var destination = options.OutputFile ?? options.InputFile;
            _fileService.WriteAllText(destination, replaced);
            return 0;
        }
    }
}