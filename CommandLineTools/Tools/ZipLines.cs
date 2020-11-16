using CommandLineTools.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace CommandLineTools.Tools
{
    public class ZipLines : CommandLineFileTool<ZipLinesOptions>
    {
        public override int ExecuteCommand(ZipLinesOptions options)
        {
            var log = new VerboseLogger(options);
            var inputStreams = options.InputFiles.Select(f => FileService.ReadLinesLazily(f).GetEnumerator()).ToList();
            var coeffs = (options.Coefficients ?? Enumerable.Repeat(1, inputStreams.Count)).ToArray();

            
            var result = new List<string>();

            var hasLines = Enumerable.Repeat(true, inputStreams.Count).ToArray();
            while (hasLines.Any(x => x))
            {
                for (int stream = 0; stream < inputStreams.Count; stream += 1)
                {
                    for (int times = 0; times < coeffs[stream]; times += 1)
                    {
                        if (!hasLines[stream])
                        {
                            break;
                        }
                        if (inputStreams[stream].MoveNext())
                        {
                            result.Add(inputStreams[stream].Current);
                        }
                        else
                        {
                            hasLines[stream] = false;
                        }
                    }
                }
            }
            FileService.WriteAllLines(options.OutputFile, result);
            return 0;
        }
    }
}