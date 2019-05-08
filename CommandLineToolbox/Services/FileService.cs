using System.Collections.Generic;
using System.IO;
using CommandLineTools.Contracts;

namespace CommandLineToolbox.Services
{
    public class FileService : IFileService
    {
        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        public string[] ReadAllLines(string path)
        {
            return File.ReadAllLines(path);
        }

        public IEnumerable<string> ReadLinesLazily(string path)
        {
            return File.ReadLines(path);
        }

        public void WriteAllLines(string path, IEnumerable<string> lines)
        {
            File.WriteAllLines(path, lines);
        }

        public void WriteAllText(string path, string content)
        {
            File.WriteAllText(path, content);
        }

        public void WriteAllLines(string path, string[] lines)
        {
            File.WriteAllLines(path, lines);
        }
    }
}