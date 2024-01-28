using CommandLineTools.Tools;
using CommandLineTools.Windows.Options;
using RestartManager;

namespace CommandLineTools.Windows.Tools
{
    public class WhoLockMe : ICommandLineTool<WhoLockMeOptions>
    {
        public int ExecuteCommand(WhoLockMeOptions options)
        {
            var locks = FileUtil.GetProcessesLockingFile(options.File);
            foreach (var process in locks.Where(p => p is not null))
            {
                Console.WriteLine($"{process.ProcessName}: {process.MainModule?.FileName}");
            }

            return 0;
        }
    }
}