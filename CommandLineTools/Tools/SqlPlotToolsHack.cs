using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text.RegularExpressions;
using CommandLineTools.Contracts;
using CommandLineTools.Helpers;

namespace CommandLineTools.Tools
{
    public class SqlPlotToolsHack : CommandLineFileTool
    {
        public SqlPlotToolsHack(IFileService fileService) : base(fileService)
        {
        }

        public int ExecuteCommand(SqlPlotToolsHackOptions options)
        {
            var input = _fileService.ReadLinesLazily(options.InputFile);
            using (var connection = SQLiteHelpers.CreateConnection(options.DatabaseFile))
            {
                connection.Open();
                foreach (var block in GetBlocks(input.Where(IsResultLine).Select(GetValue), blockSize: 1000))
                {
                    using (var command = new SQLiteCommand(connection))
                    {
                        command.CommandText = $"INSERT INTO {options.DatabaseTable} VALUES {block};";
                        command.ExecuteNonQuery();
                    }
                }

                connection.Close();
            }
            return 0;
        }

        public IEnumerable<string> GetBlocks(IEnumerable<string> flatSource, int blockSize = 100)
        {
            int count = 0;
            var block = "";
            foreach (var ele in flatSource)
            {
                if (count == 0)
                {
                    block = $"({ele})";
                }
                else
                {
                    block += "," + $"({ele})";
                }

                count += 1;
                if (count >= blockSize)
                {
                    yield return block;
                    count = 0;
                }
            }
            yield return block;
        }

        public bool IsResultLine(string line)
        {
            return line.StartsWith("RESULT");
        }

        public string GetValue(string resultLine)
        {
            return resultLine.Split('=')[1];
        }
    }

    public class ValueBlock<TBlock>
    {
        public TBlock BlockValue { get; set; }
    }
}