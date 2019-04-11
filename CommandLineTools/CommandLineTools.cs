﻿using System;
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
            Parser
                .Default
                .ParseArguments<InFileReplaceOptions, RemoveLinesOptions, ExecuteBatchOptions, StatisticalTableOptions>(args)
                .MapResult(
                    (InFileReplaceOptions opt) => new InFileReplace(_fileService).ExecuteCommand(opt),
                    (RemoveLinesOptions opt) => new RemoveLines(_fileService).ExecuteCommand(opt),
                    (ExecuteBatchOptions opt) => new ExecuteBatch(_fileService).ExecuteCommand(opt),
                    (StatisticalTableOptions opt) => new StatisticalTable(_fileService).ExecuteCommand(opt),
                    errs => 1);
        }
    }
}