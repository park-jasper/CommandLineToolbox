using System;
using System.Collections.Generic;
using CommandLineTools.Contracts;
using System.Linq;
using System.Data.SQLite;
using System.Text;
using MoreLinq;

namespace CommandLineTools.Tools
{
    public class StatisticalTable : CommandLineFileTool
    {
        public StatisticalTable(IFileService fileService) : base(fileService)
        {
        }

        public int ExecuteCommand(StatisticalTableOptions options)
        {
            List<Data> data = RetrieveData(options);

            var secs = options.Secondaries.ToArray();
            int numberOfGroups = 0;
            var grouping = data.GroupBy(d => d.Secondaries[0]).ToList();
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
                    GeoM = Math.Pow(group.Aggregate(1.0d, (current, d) => current * d.RelativeValue),
                        1d / numberOfGroups),
                    Data = group.OrderBy(d => d.Secondaries[0]).ToList()
                })
                .ToList();
            var byGeom = result.OrderBy(dr => dr.GeoM).ToArray();
            for (int i = 0; i < byGeom.Length; i += 1)
            {
                byGeom[i].Rank = i + 1;
            }

            var sb = new StringBuilder();
            const string columnSep = "@{~~}";
            sb.AppendLine(@"\begin{tabular}{l | r " + columnSep + " r | " + string.Join(columnSep, Enumerable.Repeat("r", numberOfGroups + 1)) + "|}");
            sb.AppendLine(@" & \multicolumn{2}{c}{Overall} & \multicolumn{" + numberOfGroups + "}{c}{" +
                          ( options.SecondaryAliases.FirstOrDefault() ?? secs[0] ) + @"} \\");
            sb.AppendLine(" & Rank & GeoM & " + string.Join("&", grouping.Select(g => g.Key).OrderBy(x => x)) +
                          @"\\ \hline");
            foreach (var res in result)
            {
                sb.AppendLine(
                    $"{GetFonted(res.Main, options.MainFont)} & {res.Rank} & {FormatDouble(res.GeoM)} & {string.Join("&", res.Data.Select(BoldRank1))}" +
                    @"\\");
            }

            sb.AppendLine(@"\end{tabular}");

            _fileService.WriteAllText(options.OutputFile, sb.ToString());

            return 0;
        }

        public static List<Data> RetrieveData(StatisticalTableOptions options)
        {
            var metric = GetMetric(options.Metric);
            List<Data> data = new List<Data>();
            using (var connection = new SQLiteConnection($"Data Source={options.DatabaseFile}"))
            {
                connection.Open();
                var secondaries = string.Join(",", options.Secondaries);
                using (var command = new SQLiteCommand(connection)
                {
                    CommandText =
                        $"SELECT {options.Main} as _main, {metric}({options.Value}) as _value, {secondaries} FROM {options.DatabaseTable} group by {options.Main}, {secondaries}"
                })
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        data.Add(new Data
                        {
                            Main = GetAsString(reader["_main"]),
                            Value = GetAsDouble(reader["_value"]),
                            Secondaries = options.Secondaries.Select(name => GetAsInt(reader[name])).ToArray()
                        });
                    }

                    reader.Close();
                }

                connection.Close();
            }

            return data;
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

        public static string FormatDouble(double value)
        {
            var rounded = Math.Round(value, 2);
            return rounded.ToString("0.00").Replace(',', '.');
        }

        public static string GetFonted(string main, string font)
        {
            return string.IsNullOrEmpty(font) ? main : string.Format(font, main);
        }

        public static string BoldRank1(Data data)
        {
            var result = FormatDouble(data.RelativeValue);
            if (data.IsRelativeOne)
            {
                result = @"\textbf{" + result + @"}";
            }

            return result;
        }

        public static int GetAsInt(object value)
        {
            switch (value)
            {
                case int ivalue:
                    return ivalue;
                case long lvalue:
                    return (int) lvalue;
                case float fvalue:
                    return (int) fvalue;
                case double dvalue:
                    return (int) dvalue;
                case decimal dvalue:
                    return (int) dvalue;
                case string svalue:
                    if (int.TryParse(svalue, out int result))
                    {
                        return result;
                    }
                    throw new InvalidCastException(
                        $"Could not cast object {value} with type {value.GetType()} to int.");
                default:
                    throw new InvalidCastException(
                        $"Could not cast object {value} with type {value.GetType()} to int.");
            }
        }

        public static double GetAsDouble(object value)
        {
            switch (value)
            {
                case int ivalue:
                    return (double) ivalue;
                case long lvalue:
                    return (double) lvalue;
                case float fvalue:
                    return (double) fvalue;
                case double dvalue:
                    return dvalue;
                case decimal dvalue:
                    return (double) dvalue;
                case string svalue:
                    if (double.TryParse(svalue, out var result))
                    {
                        return result;
                    }
                    throw new InvalidCastException(
                        $"Could not cast object {value} with type {value.GetType()} to double.");
                default:
                    throw new InvalidCastException(
                        $"Could not cast object {value} with type {value.GetType()} to double.");
            }
        }

        public static string GetAsString(object value)
        {
            switch (value)
            {
                case string svalue:
                    return svalue;
                default:
                    return value.ToString();
            }
        }
    }

    public class Data
    {
        public string Main { get; set; }
        public double Value { get; set; }

        public double RelativeValue { get; set; }
        public bool IsRelativeOne { get; set; } = false;

        public int[] Secondaries { get; set; }
    }

    public class DataResult
    {
        public string Main { get; set; }
        public double GeoM { get; set; }
        public List<Data> Data { get; set; }
        public int Rank { get; set; }
    }
}