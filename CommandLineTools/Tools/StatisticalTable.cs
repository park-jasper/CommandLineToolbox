using System;
using System.Collections.Generic;
using CommandLineTools.Contracts;
using System.Linq;
using System.Data.SQLite;
using System.Text;
using CommandLineTools.Helpers;
using MoreLinq;

namespace CommandLineTools.Tools
{
    public class StatisticalTable : CommandLineFileTool, ICommandLineTool<StatisticalTableOptions>
    {
        public StatisticalTable()
        {
        }

        public int ExecuteCommand(StatisticalTableOptions options)
        {
            List<Data> data = RetrieveData(options);

            var secs = options.Secondaries.ToArray();
            int numberOfGroups = 0;
            var grouping = data.GroupBy(d => d.Secondaries[0].ToString() + d.Machine.ToString()).ToList();
            foreach (var group in grouping)
            {
                numberOfGroups += 1;
                var relativeOne = group.MinBy(d => d.Value);
                relativeOne.IsRelativeOne = true;
                foreach (var d in group)
                {
                    d.RelativeValue = d.Value / relativeOne.Value;
                }
            }

            var resultGrouping = data.GroupBy(d => d.Main);
            var result = resultGrouping
                .Select(group => new DataResult
                {
                    Main = group.Key,
                    MainGroup = group.First().MainGroup,
                    GeoM = CalculateGeom(group, options, numberOfGroups),
                    Data = group.OrderBy(d => d.Secondaries[0]).ToList()
                })
                .ToList();
            var byGeom = result.OrderBy(dr => dr.GeoM).ToList();
            for (int i = 0; i < byGeom.Count; i += 1)
            {
                byGeom[i].Rank = i + 1;
            }

            var sb = new StringBuilder();

            CreateText(sb, options, byGeom, numberOfGroups, secs, grouping);

            _fileService.WriteAllText(options.OutputFile, sb.ToString());

            return 0;
        }

        private double CalculateGeom(IGrouping<string, Data> group, StatisticalTableOptions options, int numberOfGroups)
        {
            var relevantData = group.WhereNot(g =>
                g.Secondaries.Any(s => options.ExcludeFromGeoM.Contains(s)) ||
                g.Machine == options.ExcludeFromGeomMachine);
            double result = 1;
            foreach (var data in relevantData)
            {
                result *= data.RelativeValue;
            }
            return Math.Pow(result, 1d / numberOfGroups);
        }

        private void CreateText(StringBuilder sb, StatisticalTableOptions options, IList<DataResult> result, int numberOfGroups, string[] secs, IList<IGrouping<string, Data>> grouping)
        {
            const string columnSep = "@{~~}";
            if (options.OnlyGeoms)
            {
                sb.AppendLine(@"\begin{tabular}{ l | l || l | l }");
                sb.AppendLine(@"Sorter&GeoM&Sorter&GeoM\\ \hline");
                var resultLength = result.Count;
                var halfLength = resultLength / 2;
                for (int i = 0; i < halfLength; i += 1)
                {
                    sb.Append(GetFonted(result[i].Main, options.MainFont) + "&" + FormatDouble(result[i].GeoM, 3));
                    if (i + halfLength < resultLength)
                    {
                        sb.Append("&" + GetFonted(result[i + halfLength].Main, options.MainFont) + "&" + FormatDouble(result[i + halfLength].GeoM, 3));
                    }
                    else
                    {
                        sb.Append("&&");
                    }
                    sb.AppendLine(@"\\");
                }

                sb.AppendLine(@"\end{tabular}");
                return;
            }

            sb.AppendLine(@"\begin{tabular}{l | r " + columnSep + " r | " + string.Join(columnSep, Enumerable.Repeat("r", numberOfGroups + 1)) + "|}");
            sb.AppendLine(@" & \multicolumn{2}{c|}{Overall} & \multicolumn{" + numberOfGroups + "}{c}{" +
                          (options.SecondaryAliases.FirstOrDefault() ?? secs[0]) + @"} \\");
            sb.AppendLine(" & Rank & GeoM & " + string.Join("&", grouping.Select(g => g.Key).OrderBy(x => x)) +
                          @"\\ \hline");
            if (!string.IsNullOrEmpty(options.MainGroup))
            {
                var mainGrouped = result.GroupBy(res => res.MainGroup).ToArray();
                for (int i = 0; i < mainGrouped.Length - 1; i += 1)
                {
                    AppendLines(sb, mainGrouped[i].OrderBy(dr => dr.Rank), options, false);
                }

                AppendLines(sb, mainGrouped[mainGrouped.Length - 1].OrderBy(dr => dr.Rank), options, true);
            }
            else
            {
                AppendLines(sb, result, options, true);
            }


            sb.AppendLine(@"\end{tabular}");
        }

        private static void AppendLines(StringBuilder sb, IEnumerable<DataResult> result, StatisticalTableOptions options, bool lastBlock)
        {
            var arr = result.ToArray();
            DataResult res;
            for (int i = 0; i < (lastBlock ? arr.Length : arr.Length - 1); i += 1)
            {
                res = arr[i];
                sb.AppendLine(
                    $"{GetFonted(res.Main, options.MainFont)} & {res.Rank} & {FormatDouble(res.GeoM)} & {string.Join("&", res.Data.Select(d => BoldRank1(d, options)))}" +
                    @"\\");
            }

            if (!lastBlock)
            {
                res = arr[arr.Length - 1];
                sb.AppendLine(
                    $"{GetFonted(res.Main, options.MainFont)} & {res.Rank} & {FormatDouble(res.GeoM)} & {string.Join("&", res.Data.Select(d => BoldRank1(d, options)))}" +
                    (options.MainGroupSeparator ?? @"\\"));
            }
        }

        private static List<Data> RetrieveData(StatisticalTableOptions options)
        {
            var metric = GetMetric(options.Metric);
            List<Data> data = new List<Data>();
            using (var connection = SQLiteHelpers.CreateConnection(options.DatabaseFile))
            {
                connection.Open();
                var secondaries = string.Join(",", options.Secondaries);
                string cText =
                    $"SELECT {options.Main} as _main, {metric}({options.Value}) as _value, _machine, {secondaries}{GetMainGroup(options)} FROM {GetUnion(options.DatabaseTable)} group by {options.Main}, {secondaries}";

                if (options.Verbose)
                {
                    Console.WriteLine("Command Text: " + cText);
                }
                using (var command = new SQLiteCommand(connection)
                {
                    CommandText =  cText
                })
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var d = new Data
                        {
                            Main = TypeHelpers.GetAsString(reader["_main"]),
                            Value = TypeHelpers.GetAsDouble(reader["_value"]),
                            Secondaries = options.Secondaries.Select(name => TypeHelpers.GetAsInt(reader[name]))
                                .ToArray(),
                            Machine = TypeHelpers.GetAsInt(reader["_machine"])
                        };
                        if (!string.IsNullOrEmpty(options.MainGroup))
                        {
                            d.MainGroup = TypeHelpers.GetAsString(reader[options.MainGroup]);
                        }

                        data.Add(d);
                    }

                    reader.Close();
                }

                connection.Close();
            }

            return data;
        }

        public static string GetUnion(IEnumerable<string> tables)
        {
            var sb = new StringBuilder();
            sb.AppendLine("(");
            bool first = true;
            int i = 1;
            foreach (var t in tables)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.AppendLine("UNION");
                }

                sb.AppendLine($"SELECT *, {i} as _machine FROM {t}");
                i += 1;
            }
            sb.Append(")");
            return sb.ToString();
        }

        public static string GetMetric(string metric)
        {
            switch (metric.ToLower())
            {
                case "average":
                    return "AVG";
                case "median":
                    return "MEDIAN";
                case "min":
                    return "MIN";
                case "max":
                    return "MAX";
                default:
                    return "AVG";
            }
        }

        public static string GetMainGroup(StatisticalTableOptions options)
        {
            return string.IsNullOrEmpty(options.MainGroup) ? "" : $", {options.MainGroup}";
        }

        public static string FormatDouble(double value, int decimals = -1)
        {
            if (decimals == -1)
            {
                decimals = value < 10 ? 2 : ( value < 100 ? 1 : 0 );
            }

            string format = "0." + new string(Enumerable.Repeat('0', decimals).ToArray());
            var rounded = Math.Round(value, decimals);
            return rounded.ToString(format).Replace(',', '.');
        }

        public static string GetFonted(string main, string font)
        {
            return string.IsNullOrEmpty(font) ? main : string.Format(font, main);
        }

        private static string BoldRank1(Data data, StatisticalTableOptions options)
        {
            var result = FormatDouble(options.PrintAbsoluteValues ? data.Value : data.RelativeValue, data.Main == "2" ? 2 : -1);
            if (data.IsRelativeOne)
            {
                result = @"\textbf{" + result + @"}";
            }

            return result;
        }

        private class Data
        {
            public string Main { get; set; }
            public string MainGroup { get; set; }
            public double Value { get; set; }

            public double RelativeValue { get; set; }
            public bool IsRelativeOne { get; set; } = false;

            public int[] Secondaries { get; set; }
            public int Machine { get; set; }
        }

        private class DataResult
        {
            public string Main { get; set; }
            public string MainGroup { get; set; }
            public double GeoM { get; set; }
            public List<Data> Data { get; set; }
            public int Rank { get; set; }
        }
    }
}