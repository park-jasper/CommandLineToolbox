using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using CommandLineTools.Helpers;

namespace CommandLineTools.Tools
{
    public class StatisticalFunctions
    {
        public int ExecuteCommand(StatisticalFunctionsOptions options)
        {
            List<Data> data = RetrieveData(options);

            var funcs = options.Functions.ToArray();

            var results = new string[funcs.Length];

            Data[][] res = new Data[funcs.Length][];

            for (int i = 0; i < funcs.Length; i += 1)
            {
                switch (funcs[i])
                {
                    case "average":
                        res[i] = Calculate(data, d => d.Average(x => x.Value));
                        break;
                    case "median":
                        res[i] = Calculate(data, CalculateMedian);
                        break;
                    case "variance":
                        res[i] = Calculate(data, CalculateVariance);
                        break;
                }
            }

            using (var connection = SQLiteHelpers.CreateConnection(options.DatabaseFile))
            {
                connection.Open();
                var command = new SQLiteCommand(connection)
                {
                    CommandText =
                        $"CREATE TABLE IF NOT EXISTS {options.DatabaseTableOut} ({GetColumns(options.Groups, "TEXT")}, {GetColumns(funcs, "REAL")});"
                };
                command.ExecuteNonQuery();

                List<string> valuesList = new List<string>();

                foreach (var d in res[0])
                {
                    var groups = string.Join(",", d.Groups.Select(g => $"'{g}'"));
                    var values = string.Join(",", FindValues(d, res));
                    valuesList.Add($"({groups}, {values})");
                }

                command.CommandText = $"INSERT INTO {options.DatabaseTableOut} VALUES {string.Join(",", valuesList)};";
                command.ExecuteNonQuery();

                command.Dispose();
                connection.Close();
            }

            return 0;
        }

        private static IEnumerable<string> FindValues(Data d, Data[][] res)
        {
            yield return d.Value.ToString("0.000").Replace(',','.');
            for (int i = 1; i < res.Length; i += 1)
            {
                var match = res[i].First(ele => ArrayEqual(d.Groups, ele.Groups));
                yield return match.Value.ToString("0.000").Replace(',','.');
            }
        }

        private static bool ArrayEqual<TElement>(TElement[] left, TElement[] right) where TElement : IEquatable<TElement>
        {
            if (left.Length != right.Length)
            {
                return false;
            }
            for (int i = 0; i < left.Length; i += 1)
            {
                if (!left[i].Equals(right[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private static string GetColumns(IEnumerable<string> columnNames, string type)
        {
            return string.Join(",", columnNames.Select(n => $"{n} {type}"));
        }

        private static double CalculateMedian(IEnumerable<Data> data)
        {
            var set = data.OrderBy(d => d.Value).ToArray();
            var half = set.Length / 2;
            if (set.Length % 2 == 0)
            {
                return ( set[half].Value + set[half - 1].Value ) / 2;
            }
            else
            {
                return set[half].Value;
            }
        }

        private static double CalculateVariance(IEnumerable<Data> data)
        {
            var dataList = data.ToList();
            var average = dataList.Average(d => d.Value);
            return dataList.Aggregate(0d, (cur, d) => cur + Math.Pow(d.Value - average, 2)) / (dataList.Count - 1);
        }

        private static Data[] Calculate(List<Data> data, Func<IEnumerable<Data>, double> function)
        {
            if (data.FirstOrDefault()?.Groups.Length == 0)
            {
                return new[]
                {
                    new Data
                    {
                        Value = function(data)
                    }
                };
            }
            else
            {
                return CalculateByGroup(data, 0, Enumerable.Empty<string>(), function).ToArray();
            }
        }

        private static IEnumerable<Data> CalculateByGroup(IEnumerable<Data> group, int index,
            IEnumerable<string> groups, Func<IEnumerable<Data>, double> function)
        {
            if (index >= group.First().Groups.Length)
            {
                return new[]
                {
                    new Data
                    {
                        Value = function(group),
                        Groups = groups.ToArray()
                    }
                };
            }
            else
            {
                return group.GroupBy(d => d.Groups[index])
                    .SelectMany(g => CalculateByGroup(g, index + 1, groups.Append(g.Key), function));
            }
        }

        private static List<Data> RetrieveData(StatisticalFunctionsOptions options)
        {
            var valueName = options.Value;
            List<Data> data = new List<Data>();
            using (var connection = SQLiteHelpers.CreateConnection(options.DatabaseFile))
            {
                connection.Open();
                var groups = string.Join(",", options.Groups);
                using (var command = new SQLiteCommand(connection)
                {
                    CommandText = $"SELECT {options.Value} as _value, {groups} FROM {options.DatabaseTable}"
                })
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        data.Add(new Data
                        {
                            Value = TypeHelpers.GetAsDouble(reader["_value"]),
                            Groups = options.Groups.Select(g => TypeHelpers.GetAsString(reader[g])).ToArray()
                        });
                    }

                    reader.Close();
                }

                connection.Close();
            }

            return data;
        }

        private class Data
        {
            public double Value { get; set; }
            public string[] Groups { get; set; }
        }
    }
}