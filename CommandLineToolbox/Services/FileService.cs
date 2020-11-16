using System.Collections.Generic;
using System.IO;
using CommandLineTools.Contracts;

namespace CommandLineToolbox.Services
{
    public class FileService : IFileService
    {
        public string ReadAllText(string path) => File.ReadAllText(path);

        public string[] ReadAllLines(string path) => File.ReadAllLines(path);

        public IEnumerable<string> ReadLinesLazily(string path) => File.ReadLines(path);

        public bool Exists(string path) => File.Exists(path);

        public void WriteAllLines(string path, IEnumerable<string> lines) => File.WriteAllLines(path, lines);

        public void WriteAllText(string path, string content) => File.WriteAllText(path, content);

        public void WriteAllLines(string path, string[] lines) => File.WriteAllLines(path, lines);

        public void DeleteFile(string path) => File.Delete(path);

        public byte[] ReadAllBytes(string path) => File.ReadAllBytes(path);

        public void WriteAllBytes(string path, byte[] arr) => File.WriteAllBytes(path, arr);
        public void CopyDirectory(string sourcePath, string destinationPath)
        {
            var sourceDir = new DirectoryInfo(sourcePath);
            var destDir = new DirectoryInfo(destinationPath);

            CopyDirectory(sourceDir, destDir);
        }

        private void CopyDirectory(DirectoryInfo source, DirectoryInfo dest)
        {
            if (!dest.Exists)
            {
                dest.Create();
            }
            CopyFiles(source, dest);
            foreach (var dir in source.GetDirectories())
            {
                var newDest = new DirectoryInfo(Path.Combine(dest.FullName, dir.Name));
                CopyDirectory(dir, newDest);
            }
        }

        private void CopyFiles(DirectoryInfo source, DirectoryInfo dest)
        {
            foreach (var file in source.GetFiles())
            {
                file.CopyTo(Path.Combine(dest.FullName, file.Name));
            }
        }

        public string GetFullPath(string path)
        {
            return Path.GetFullPath(path);
        }
    }
}