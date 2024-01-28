using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLineTools.Options;

namespace CommandLineTools.Tools
{
    public class ReduceCsvTool : CommandLineFileTool<ReduceCsvOptions>
    {
        public override int ExecuteCommand(ReduceCsvOptions options)
        {
            var keepLookup = (options.ColumnsToKeep ?? Enumerable.Empty<string>()).ToLookup(x => x);
            var removeLookup = (options.ColumnsToRemove ?? Enumerable.Empty<string>()).ToLookup(x => x);
            var lines = this.FileService.ReadLinesLazily(options.InFile);
            var enumerator = lines.GetEnumerator();
            var columnKeepIndices = new List<int>();
            if (enumerator.MoveNext())
            {
                var header = enumerator.Current;
                var columns = header.Split(options.Separator);
                for (int i = 0; i < columns.Length; i += 1)
                {
                    if (keepLookup.Contains(columns[i]))
                    {
                        columnKeepIndices.Add(i);
                        continue;
                    }

                    if (removeLookup.Contains(columns[i]))
                    {
                        continue;
                    }

                    Console.Write($"keep column {columns[i]} in the output file? [y/n] ");
                    char reply = ' ';
                    do
                    {
                        var key = Console.ReadKey();
                        reply = key.KeyChar;
                    }
                    while (reply != 'y' && reply != 'n');

                    Console.WriteLine("");
                    if (reply == 'y')
                    {
                        columnKeepIndices.Add(i);
                    }
                }
                this.FileService.WriteAllLines(options.OutputFile, ReduceLines(options.Separator, enumerator.Prepend(header), columnKeepIndices));
            }
            else
            {
                Console.WriteLine("empty input file");
            }

            return 0;
        }

        private static IEnumerable<string> ReduceLines(char separator, IEnumerable<string> lines, IEnumerable<int> columnIndices)
        {
            var sb = new StringBuilder();
            var indexList = columnIndices.Distinct().OrderBy(x => x).ToList();
            if (indexList.Count == 0)
            {
                throw new Exception("no columns to copy to the new file");
            }

            foreach (var line in lines)
            {
                sb.Clear();
                var columns = line.Split(separator);
                sb.Append(columns[indexList[0]]);
                for (int i = 1; i < indexList.Count; i += 1)
                {
                    sb.Append(separator);
                    sb.Append(columns[indexList[i]]);
                }

                yield return sb.ToString();
            }
        }
    }

    public static class EnumeratorExtensions
    {
        public static IEnumerable<T> Prepend<T>(this IEnumerator<T> enumerator, T value)
        {
            yield return value;
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }
    }
}