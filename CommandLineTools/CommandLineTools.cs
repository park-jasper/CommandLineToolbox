using System;
using CommandLine;
using CommandLineTools.Contracts;
using CommandLineTools.Tools;

namespace CommandLineTools
{
    public class CommandLineTools
    {
        private readonly IFileService _fileService;
        public CommandLineTools(IFileService fileService)
        {
            _fileService = fileService;
        }

        public void Run(string[] args)
        {
            var fs = _fileService;
            Parser
                .Default
                .ParseArguments<InFileReplaceOptions, RemoveLinesOptions, ExecuteBatchOptions, StatisticalTableOptions,
                    StatisticalFunctionsOptions, SqlPlotToolsHackOptions, CryptoOptions, TryCopyOptions, TestOptions>(args)
                .WithParsed<InFileReplaceOptions, InFileReplace>(fs)
                .WithParsed<RemoveLinesOptions, RemoveLines>(fs)
                .WithParsed<ExecuteBatchOptions, ExecuteBatch>(fs)
                .WithParsed<StatisticalTableOptions, StatisticalTable>(fs)
                .WithParsed<StatisticalFunctionsOptions, StatisticalFunctions>(fs)
                .WithParsed<SqlPlotToolsHackOptions, SqlPlotToolsHack>(fs)
                .WithParsed<CryptoOptions, CryptoTool>(fs)
                .WithParsed<TryCopyOptions, TryCopyTool>(fs)
                .WithParsed<TestOptions, TestTool>(fs);
        }
    }

    public static class CommandLineToolsExtensions
    {
        public static ParserResult<object> WithParsed<TOptions, TClass>(this ParserResult<object> result, IFileService fs) where TClass : ICommandLineTool<TOptions>, new()
        {
            return result.WithParsed<TOptions>(opt =>
            {
                var tool = new TClass();
                if (tool is CommandLineFileTool fileTool)
                {
                    fileTool.SetFileService(fs);
                }
                tool.ExecuteCommand(opt);
            });
        }
    }
}
