using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommandLineTools.Contracts;

namespace CommandLineTools.Tools
{
    public class RemoveLines : CommandLineFileTool
    {
        private const int LineParallelThreshold = 100000; //100k
        public RemoveLines(IFileService fileService) : base(fileService)
        {

        }

        public int ExecuteCommand(RemoveLinesOptions options)
        {
            var input = _fileService.ReadAllLines(options.InputFile);
            var patternsConcatenated = options.Patterns;
            patternsConcatenated = patternsConcatenated.Replace("\\#", " ");
            var patterns = patternsConcatenated.Split('#');
            for (int i = 0; i < patterns.Length; i += 1)
            {
                patterns[i] = patterns[i].Replace(" ", "#");
            }
            
            List<string>[] result;
            List<string> compare = new List<string>();
            if (input.Length < LineParallelThreshold)
            {
                result = new List<string>[1];
                result[0] = MatchSequentially(input, 0, input.Length, MatchesPatterns(patterns, options.ConjunctivePatterns));
            }
            else
            {
                var availableCores = System.Environment.ProcessorCount;
                var linesPerCore = input.Length / availableCores;
                result = new List<string>[availableCores];
                var tasks = new Task<List<string>>[availableCores - 1];
                for (int i = 0; i < availableCores - 1; i += 1)
                {
                    var i1 = i;
                    tasks[i] = Task.Run(() => MatchSequentially(input, i1 * linesPerCore,
                        ( i1 + 1 ) * linesPerCore,
                        MatchesPatterns(patterns, options.ConjunctivePatterns)));
                }

                result[availableCores - 1] = MatchSequentially(input, ( availableCores - 1 ) * linesPerCore,
                    input.Length, MatchesPatterns(patterns, options.ConjunctivePatterns));
                for (int i = 0; i < availableCores - 1; i += 1)
                {
                    result[i] = tasks[i].Result;
                }
            }

            //var result = input.WhereNot(MatchesPatterns(patterns, options.ConjunctivePatterns)).ToArray();
            var destination = options.OutputFile ?? options.InputFile;
            var arrayResult = MakeArray(result);
            _fileService.WriteAllLines(destination, arrayResult);

            return 0;
        }

        List<string> MatchSequentially(string[] input, int startIndexInclusive, int stopIndexExclusive, Func<string, bool> invertedPred)
        {
            List<string> result = new List<string>();
            for (int i = startIndexInclusive; i < stopIndexExclusive; i += 1)
            {
                if (!invertedPred(input[i]))
                {
                    result.Add(input[i]);
                }
            }
            return result;
        }

        string[] MakeArray(params List<string>[] inputs)
        {
            var totalLength = inputs.Sum(i => i.Count);
            var result = new string[totalLength];
            int index = 0;
            foreach (var list in inputs)
            {
                foreach (var line in list)
                {
                    result[index] = line;
                    index += 1;
                }
            }

            return result;
        }

        private Func<string, bool> MatchesPatterns(string[] patterns, bool matchConjunctive)
        {
            var regexes = patterns.Select(p => new Regex(p, RegexOptions.Compiled & RegexOptions.CultureInvariant)).ToImmutableList();
            return line =>
            {
                if (matchConjunctive)
                {
                    return regexes.All(regex => regex.DoesMatch(line));
                }
                else
                {
                    return regexes.Any(regex => regex.DoesMatch(line));
                }
            };
        }
    }

    public static class RegexExtensions
    {
        public static bool DoesMatch(this Regex regex, string input)
        {
            return regex.Match(input).Success;
        }

        public static IEnumerable<T> WhereNot<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            return source.Where(item => !predicate(item));
        }
    }
}