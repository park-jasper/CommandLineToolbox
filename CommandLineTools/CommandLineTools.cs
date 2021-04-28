using System;
using System.Collections.Generic;
using CommandLine;
using CommandLineTools.Contracts;
using CommandLineTools.Options;
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
            Parser
                .Default
                .Builder(_fileService)
                .WithTool<InFileReplaceOptions, InFileReplace>()
                .WithTool<RemoveLinesOptions, RemoveLines>()
                .WithTool<DuplicateLinesOptions, DuplicateLinesTool>()
                .WithTool<ZipLinesOptions, ZipLines>()
                .WithTool<ExecuteBatchOptions, ExecuteBatch>()
                .WithTool<StatisticalTableOptions, StatisticalTable>()
                .WithTool<StatisticalFunctionsOptions, StatisticalFunctions>()
                .WithTool<SqlPlotToolsHackOptions, SqlPlotToolsHack>()
                .WithTool<CryptoOptions, CryptoTool>()
                .WithTool<TryCopyOptions, TryCopyTool>()
                .WithTool<TestOptions, TestTool>()
                .WithTool<PasswordGenerationOptions, PasswordGeneration>()
                .WithTool<StrongNameOptions, StrongNameTool>()
                .WithTool<GetTimeDataFromLogsOptions, GetTimeDataFromLogs>()
                .WithTool<ChecksumOptions, ChecksumTool>()
                .Parse(args);
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

        public static ParserBuilder Builder(this Parser parser, IFileService fs)
        {
            return new ParserBuilder(parser, fs);
        }
    }

    public class ParserBuilder
    {
        private readonly Parser parser;
        private readonly IFileService fileService;
        private readonly List<Type> optionTypes = new List<Type>();
        private readonly List<Func<ParserResult<object>, ParserResult<object>>> withParsedActions = new List<Func<ParserResult<object>, ParserResult<object>>>();

        public ParserBuilder(Parser parser, IFileService fileService)
        {
            this.parser = parser;
            this.fileService = fileService;
        }

        public ParserBuilder WithTool<TOptions, TTool>() where TTool : ICommandLineTool<TOptions>, new()
        {
            withParsedActions.Add(result => result.WithParsed<TOptions, TTool>(this.fileService));
            optionTypes.Add(typeof(TOptions));
            return this;
        }

        public ParserResult<object> Parse(IEnumerable<string> args)
        {
            var result = this.parser.ParseArguments(args, this.optionTypes.ToArray());
            foreach (var withParsed in this.withParsedActions)
            {
                result = withParsed(result);
            }

            return result;
        }
    }
}
