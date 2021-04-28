using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CommandLineTools.Helpers;
using CommandLineTools.Options;
using IRE;

namespace CommandLineTools.Tools
{
    public class GetTimeDataFromLogs : CommandLineFileTool<GetTimeDataFromLogsOptions>
    {
        private readonly Regex fullReportParseRegex;

        public GetTimeDataFromLogs()
        {
            fullReportParseRegex = new RegexBuilder()
                .BeginString()
                .CaptureGroup(
                    builder => builder
                        .AnyDecimalDigit()
                        .Or()
                        .AppendText('-')
                        .Or()
                        .AppendText(':')
                        .Or()
                        .AppendText('.')
                        .Or()
                        .AnyWhiteSpaceCharacter(),
                    true)
                .OptionallyAnyQuantity() //Date and time information
                .AnyWhiteSpaceCharacter().OptionallyAnyQuantity()
                .AnyWordCharacter().OptionallyAnyQuantity() //Log level
                .AnyWhiteSpaceCharacter().OptionallyAnyQuantity()
                .AnyWordCharacter().OptionallyAnyQuantity() //method stack info
                .AnyWhiteSpaceCharacter().OptionallyAnyQuantity()
                .CaptureGroup(
                    builder => builder
                        .AnyWordCharacter().OptionallyAnyQuantity(),
                    false) //class name
                .AppendText(':')
                .CaptureGroup(
                    builder => builder
                        .AnyCharacter().OptionallyAnyQuantity(),
                    false) //method name
                .AnyWhiteSpaceCharacter().OptionallyAnyQuantity()
                .AnyWhiteSpaceCharacter()
                .AppendText("[Leave ")
                .CaptureGroup(
                    builder => builder
                        .CaptureGroup(
                            innerBuilder => innerBuilder
                                .AnyDecimalDigit()
                                .Or()
                                .AppendText('.')
                                .Or()
                                .AnyWordCharacter(),
                            true)
                        .OptionallyAnyQuantity(),
                    false)
                .AppendText(']')
                .AnyWhiteSpaceCharacter().OptionallyAnyQuantity()
                .EndString()
                .CreateRegex();
        }

        public override int ExecuteCommand(GetTimeDataFromLogsOptions options)
        {
            if (options.InFiles == null || !options.InFiles.Any())
            {
                options.InFiles = Directory.EnumerateFiles(".");
            }

            List<Report> data = new List<Report>();

            foreach (var file in options.InFiles)
            {
                data.AddRange(
                    this.FileService.ReadAllLines(file)
                        .Where(IsTimedEntry)
                        .Select(ToReport));
            }

            WriteToDatabase(data, options.OutFile);

            return 0;
        }

        private bool IsTimedEntry(string line)
        {
            return this.fullReportParseRegex.IsMatch(line);
        }

        private Report ToReport(string line)
        {
            var match = this.fullReportParseRegex.Match(line);
            var className = match.Groups[1].Value;
            var methodName = match.Groups[2].Value;
            var timeString = match.Groups[3].Value;
            TimeSpan time = TimeSpan.Zero;
            if (timeString.EndsWith("ms"))
            {
                timeString = timeString.Substring(0, timeString.Length - 2);
                var timeInMs = double.Parse(timeString, CultureInfo.InvariantCulture);
                time = TimeSpan.FromMilliseconds(timeInMs);
            }
            return new Report()
            {
                ClassName = className,
                MethodName = methodName,
                Time = time,
            };
        }

        private void WriteToDatabase(IEnumerable<Report> reports, string filename)
        {
            using (var conn = SQLiteHelpers.CreateConnection(filename))
            {
                conn.Open();
                var command = new SQLiteCommand(conn);
                command.CommandText =
                    "CREATE TABLE IF NOT EXISTS reports (id INTEGER PRIMARY KEY, class TEXT NULL, method TEXT NULL, timeInMs INTEGER NULL);";
                command.ExecuteNonQuery();
                command.CommandText = "DELETE FROM reports;";
                command.ExecuteNonQuery();
                command.Dispose();
                foreach (var report in reports)
                {
                    using (command = new SQLiteCommand(conn))
                    {
                        command.CommandText = $"INSERT INTO reports (class, method, timeInMs) VALUES (@Param1, @Param2, @Param3);";
                        command.Parameters.AddWithValue("@Param1", report.ClassName);
                        command.Parameters.AddWithValue("@Param2", report.MethodName);
                        command.Parameters.AddWithValue("@Param3", report.Time.TotalMilliseconds);
                        command.ExecuteNonQuery();
                    }
                }

                conn.Close();
            }
        }

        private class Report
        {
            public string ClassName { get; set; }
            public string MethodName { get; set; }
            public TimeSpan Time { get; set; }
        }
    }
}
