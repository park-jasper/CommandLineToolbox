using CommandLineTools.Contracts;
using CommandLineTools.Helpers;

namespace CommandLineTools.Tools
{
    public abstract class CommandLineFileTool
    {
        protected IFileService _fileService;
        protected CommandLineFileTool(IFileService fileService)
        {
            _fileService = fileService;
        }
    }
}