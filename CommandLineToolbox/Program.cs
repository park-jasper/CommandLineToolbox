using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLineToolbox.Services;
using CommandLineTools.Windows.Options;
using CommandLineTools.Windows.Tools;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;

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
            if (args.Contains("--verbose"))
            {
                Console.WriteLine("Current Arguments: " + string.Join(" ", args));
            }
#if DEBUG
            if (Debugger.IsAttached)
            {
                //    args = new[]
                //    {
                //        "inFileReplace",
                //        "--in=inFileReplaceTest.txt",
                //        "--out=inFileReplaceTestOut.txt",
                //        "--parseAsNewline=NL",
                //        "--parseAsSpace=SPACE",
                //        "--find=NL",
                //        "--replaceWith=SPACE",
                //    };
                //args = new[]
                //{
                //    "crypto",
                //    "--decrypt",
                //    "--in=E:\\Jasper\\temp\\encryption\\bahn_security_question.txt.encrypted",
                //    "--keep",
                //};
                //args = new[]
                //{
                //    "timeFromLogs",
                //    "--in=c:\\ProgramData\\Logs\\HappyAppy.Server\\HappyAppy.Server_21108_2021-04-28.log",
                //    "--out=tempout.sqlite",
                //};
            }
#endif
            new CommandLineTools
                .CommandLineTools(new FileService())
                .Build()
                .WithTool<WhoLockMeOptions, WhoLockMe>()
                .Parse(args);
            if (Debugger.IsAttached)
            {
                Console.WriteLine("Exit delayed because of attached debugger. Press any key to exit");
                Console.ReadKey();
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
            catch (Exception)
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
