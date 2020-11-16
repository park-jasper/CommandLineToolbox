using CommandLineTools.Contracts;
using CommandLineTools.Helpers;

namespace CommandLineTools.Tools
{
    public abstract class CommandLineFileTool
    {
        protected IFileService FileService;

        protected CommandLineFileTool()
        {

        }

        public void SetFileService(IFileService fileService)
        {
            FileService = fileService;
        }
    }
    public abstract class CommandLineFileTool<TOptions> : CommandLineFileTool, ICommandLineTool<TOptions>
    {
        public abstract int ExecuteCommand(TOptions options);
    }
}