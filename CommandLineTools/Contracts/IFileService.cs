

using System.Collections.Generic;

namespace CommandLineTools.Contracts
{
    public interface IFileService
    {
        string ReadAllText(string path);
        string[] ReadAllLines(string path);
        IEnumerable<string> ReadLinesLazily(string path);
        bool Exists(string path);

        void WriteAllText(string path, string content);
        void WriteAllLines(string path, string[] lines);
        void WriteAllLines(string path, IEnumerable<string> lines);
        void DeleteFile(string path);
        byte[] ReadAllBytes(string path);
        void WriteAllBytes(string path, byte[] arr);
    }
}