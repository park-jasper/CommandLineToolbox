using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLineToolbox.Services;
using Microsoft.VisualBasic.FileIO;

namespace CommandLineToolbox
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var (dr, hr) = Test("Tol_Ring.csv", 3);
            //var (ds, hs) = Test("Tol_Rod.csv", 2);
            //File.WriteAllText("out.txt", string.Join(Environment.NewLine, new[] { "Dia Ring " + dr, "Height Right " + hr, "Dia Rod " + ds, "Height Rod " + hs }));
            //return;
            new CommandLineTools
                .CommandLineTools(new FileService())
                .Run(args);
            if (Debugger.IsAttached)
            {
                Console.ReadLine();
            }
        }

        public static (string, string) Test(string filename, int numOfColumns)
        {
            var rows = new List<string[]>();
            try
            {
                using (var parser = new TextFieldParser(filename))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(";");
                    while (!parser.EndOfData)
                    {
                        rows.Add(parser.ReadFields());
                    }
                }
            }
            catch (Exception exc)
            {

            }

            var skip = numOfColumns;

            var diameters = new List<Allowance>();
            var heights = new List<Allowance>();
            foreach (var row in rows)
            {
                var outerDiameter = int.Parse(row[0]);
                var dias = new List<int?>();
                var hs = new List<int?>();
                for (int i = 1; i < row.Length; i += skip)
                {
                    dias.Add(NullParse(row[i]));
                }

                for (int i = skip; i < row.Length; i += skip)
                {
                    hs.Add(NullParse(row[i]));
                }

                diameters.Add(new Allowance
                {
                    OuterDiameter = outerDiameter,
                    Allowances = dias.ToArray()
                });
                heights.Add(new Allowance
                {
                    OuterDiameter = outerDiameter,
                    Allowances = hs.ToArray()
                });
            }

            var dString = string.Join(",", diameters.Select(a => $"({a.OuterDiameter},{string.Join(",", a.Allowances.Select(v => v == null ? "NULL" : v.ToString()))})"));
            var hString = string.Join(",", heights.Select(a => $"({a.OuterDiameter},{string.Join(",", a.Allowances.Select(v => v == null ? "NULL" : v.ToString()))})"));
            return ( dString, hString );
        }

        private static int? NullParse(string value)
        {
            if (value.ToLower() == "null")
            {
                return null;
            }
            else
            {
                return int.Parse(value);
            }
        }

        private class Allowance
        {
            public int OuterDiameter { get; set; }
            public int?[] Allowances { get; set; }
        }
    }
}
