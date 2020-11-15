using System;
using CommandLineTools.Contracts;

namespace CommandLineTools.Tools
{
    public class InFileReplace : CommandLineFileTool, ICommandLineTool<InFileReplaceOptions>
    {
        public InFileReplace()
        {

        }

        public int ExecuteCommand(InFileReplaceOptions options)
        {
            var input = _fileService.ReadAllText(options.InputFile);

            void ReplaceIf(string parseAs, string newValue)
            {
                if (!string.IsNullOrEmpty(parseAs))
                {
                    options.TextToFind = options.TextToFind.Replace(parseAs, newValue);
                    options.ReplacementText = options.ReplacementText.Replace(parseAs, newValue);
                }
            }

            ReplaceIf(options.ParseAsNewline, Environment.NewLine);
            ReplaceIf(options.ParseAsSpace, " ");
            var replaced = input.Replace(options.TextToFind, options.ReplacementText);
            var destination = options.OutputFile ?? options.InputFile;
            _fileService.WriteAllText(destination, replaced);
            return 0;
        }
    }
}