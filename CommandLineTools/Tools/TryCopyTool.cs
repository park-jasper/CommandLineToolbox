using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CommandLineTools.Tools
{
    public class TryCopyTool : CommandLineFileTool<TryCopyOptions>
    {

        public override int ExecuteCommand(TryCopyOptions options)
        {
            if (options.NeverFail)
            {
                try
                {
                    TryCopy(options);
                }
                catch (Exception exc)
                {
                    Console.WriteLine($"Caught exception {exc}. Stopping the tool");
                }
                return 0;
            }
            return TryCopy(options) ? 0 : 1;
        }

        private bool TryCopy(TryCopyOptions options)
        {
            if (options.IsDirectory)
            {
                FileService.CopyDirectory(options.SourcePath, options.DestinationPath);
            }

            return false;
        }
    }
}
