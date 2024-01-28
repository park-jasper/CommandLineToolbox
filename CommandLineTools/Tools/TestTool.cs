using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using CommandLineTools.Contracts;
using CommandLineTools.Helpers;
using MathNet.Numerics;
using MoreLinq;

namespace CommandLineTools.Tools
{
    public class TestTool : CommandLineFileTool<TestOptions>
    {
        public TestTool()
        {
        }

        public override int ExecuteCommand(TestOptions options)
        {
            CurseOfDimensionality();
            return 0;
            var data = RetrieveData(options);

            List<MachineRow> result = new List<MachineRow>();

            var byMachine = data.GroupBy(d => d.Machine);
            foreach (var machineValues in byMachine)
            {
                var row = new MachineRow
                {
                    MachineName = machineValues.Key
                };
                result.Add(row);
                var byArraySize = machineValues.GroupBy(v => v.ArraySize);
                foreach (var sizeValues in byArraySize)
                {
                    //if (sizeValues.Key == options.ArraySizeExclude && machineValues.Key == options.MachineExclude)
                    //{
                    //    row.Values.Add(new MachineRow.RowEntry()
                    //    {
                    //        SnName = "-",
                    //        IsName = "-",
                    //        ArraySize = sizeValues.Key,
                    //        Factor = 0
                    //    });
                    //    continue;
                    //}
                    var sized = sizeValues.ToList();
                    var snTypes = sized.Where(d => d.Sorter.Contains(options.SnFilter));
                    var isTypes = sized.Where(d => d.Sorter.Contains(options.IsFilter));

                    var bestSn = snTypes.MinBy(d => d.Average).First();
                    var bestIs = isTypes.MinBy(d => d.Average).First();

                    row.Values.Add(new MachineRow.RowEntry()
                    {
                        SnName = bestSn.Sorter,
                        IsName = bestIs.Sorter,
                        ArraySize = sizeValues.Key,
                        Factor = bestIs.Average / bestSn.Average
                    });
                }
                row.Average = row.Values.Where(v => v.Factor > 0).Average(v => v.Factor);
                row.Values.Sort((left, right) => left.ArraySize.CompareTo(right.ArraySize));
            }

            var mList = options.MachineNames.ToList();
            result.Sort((left, right) => mList.IndexOf(left.MachineName).CompareTo(mList.IndexOf(right.MachineName)));

            var flat = result.SelectMany(r => r.Values);
            var allByArraySize = flat.GroupBy(f => f.ArraySize);
            var arraySizeAverages = allByArraySize.Select(byArraySize => byArraySize.Where(v => v.Factor > 0).Average(r => r.Factor)).ToList();

            var sb = new StringBuilder();

            CreateText(sb, result, arraySizeAverages);

            FileService.WriteAllText(options.OutputFilePath, sb.ToString());

            return 0;
        }

        struct CodData
        {
            public int RandomVars { get; set; }
            public int Degree { get; set; }
            public int Polynomials { get; set; }
        }
        /// <summary>
        /// Choose n over k
        /// </summary>
        /// <param name="n"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        private int Choose(int n, int k)
        {
            return (int) (SpecialFunctions.Factorial(n) / (SpecialFunctions.Factorial(k) * SpecialFunctions.Factorial(n - k)));
        }
        private void CurseOfDimensionality()
        {
            List<CodData> codData = new List<CodData>();
            for (int p = 6; p <= 12; p += 2)
            {
                for (int m = 1; m <= 20; m += 1)
                {
                    Math.Exp(4);
                    codData.Add(new CodData()
                    {
                        RandomVars = m,
                        Degree = p,
                        Polynomials = Choose(m + p, m),
                    });
                }
            }
            var tuples = codData.Select(x => $"({x.RandomVars},{x.Degree},{x.Polynomials})");
            var values = string.Join(",", tuples);
            using (var connection = SQLiteHelpers.CreateConnection("curse_of_dimensionality.sqlite"))
            {
                connection.Open();
                var command = new SQLiteCommand(connection)
                {
                    CommandText = $"INSERT INTO cod VALUES {values}",
                };
                command.ExecuteNonQuery();
                command.Dispose();
                connection.Close();
            }
        }

        private void CreateText(StringBuilder sb, IList<MachineRow> values, IList<double> arraySizeAverages)
        {
            const string columnSep = "@{~~}";
            sb.AppendLine(@"\begin{tabular}{l | " +
                          string.Join("|", Enumerable.Repeat("r", values.First().Values.Count)) + "||r|}");

            sb.AppendLine(@"Machine & " + string.Join("&", values.First().Values.Select(v => v.ArraySize)) + " & Avg" + 
                          @"\\ \hline");

            foreach (var machine in values)
            {
                sb.AppendLine($"{machine.MachineName} & " +
                              string.Join("&", machine.Values.Select(v => FormatDouble(v.Factor))) +
                              $"& {StatisticalTable.FormatDouble(machine.Average)}" + @"\\");
            }
            sb.AppendLine(@"\hline");
            sb.AppendLine("Average & " + string.Join("&", arraySizeAverages.Select(v => StatisticalTable.FormatDouble(v))) +
                          "&" + StatisticalTable.FormatDouble(arraySizeAverages.Average()));

            sb.AppendLine(@"\end{tabular}");
        }

        private static string FormatDouble(double x)
        {
            if (x > 0)
            {
                return StatisticalTable.FormatDouble(x);
            }
            else
            {
                return "--";
            }
        }

        private static List<Data> RetrieveData(TestOptions options)
        {
            var data = new List<Data>();
            using (var connection = SQLiteHelpers.CreateConnection(options.DatabaseFile))
            {
                connection.Open();
                string cText = $"SELECT s, a, average, _machine FROM {GetUnion(options)}";
                if (options.Verbose)
                {
                    Console.WriteLine("Command Text: " + cText);
                }
                using (var command = new SQLiteCommand(connection) { CommandText = cText})
                using (var reader = command.ExecuteReader())
                {
                    var getMachine = GetMachine(options);
                    while (reader.Read())
                    {
                        var d = new Data
                        {
                            Sorter = TypeHelpers.GetAsString(reader["s"]),
                            ArraySize = TypeHelpers.GetAsInt(reader["a"]),
                            Average = TypeHelpers.GetAsDouble(reader["average"]),
                            Machine = getMachine(TypeHelpers.GetAsInt(reader["_machine"]))
                        };
                        data.Add(d);
                    }

                    reader.Close();
                }

                connection.Close();
            }

            return data;
        }

        private static Func<int, string> GetMachine(TestOptions options)
        {
            var array = options.MachineNames.ToArray();
            return i => array[i];
        }

        private static string GetUnion(TestOptions options)
        {
            var sb = new StringBuilder();
            sb.AppendLine("(");
            bool first = true;
            int i = 0;
            foreach (var t in options.TablePostfixes)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.AppendLine("UNION");
                }

                sb.AppendLine($"SELECT *, {i} as _machine FROM {options.TablePrefix}{t}");

                i += 1;
            }
            sb.Append(")");
            return sb.ToString();
        }

        private class MachineRow
        {
            public string MachineName { get; set; }
            public List<RowEntry> Values { get; set; } = new List<RowEntry>();
            public double Average { get; set; }

            public class RowEntry
            {
                public int ArraySize { get; set; }
                public string SnName { get; set; }
                public string IsName { get; set; }
                public double Factor { get; set; }
            }
        }

        private class Data
        {
            public string Sorter { get; set; }
            public int ArraySize { get; set; }
            public double Average { get; set; }
            public string Machine { get; set; }
        }
    }
}
