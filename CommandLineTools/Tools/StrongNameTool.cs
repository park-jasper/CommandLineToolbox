using System;
using System.Reflection;

namespace CommandLineTools.Tools
{
    public class StrongNameTool : CommandLineFileTool<StrongNameOptions>
    {
        public StrongNameTool()
        {

        }

        public override int ExecuteCommand(StrongNameOptions options)
        {
            var fullPath = FileService.GetFullPath(options.Name);
            Console.WriteLine(Assembly.LoadFile(fullPath).FullName);
            return 0;
        }
    }
}