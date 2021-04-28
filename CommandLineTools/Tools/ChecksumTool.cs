using System;
using System.IO;
using System.Security.Cryptography;
using CommandLineTools.Options;

namespace CommandLineTools.Tools
{
    public class ChecksumTool : CommandLineFileTool<ChecksumOptions>
    {
        public override int ExecuteCommand(ChecksumOptions options)
        {
            HashAlgorithm algorithm;
            switch (options.Algorithm.ToLower())
            {
                case ChecksumOptions.MD5:
                    algorithm = MD5.Create();
                    break;
                case ChecksumOptions.SHA256:
                    algorithm = SHA256.Create();
                    break;
                default:
                    Console.WriteLine($"Unknown or unsupported algorithm '{options.Algorithm}'.");
                    return 1;
            }

            using (var stream = File.OpenRead(options.InFile))
            {
                algorithm.ComputeHash(stream);
                Console.WriteLine(BitConverter.ToString(algorithm.Hash).Replace("-", string.Empty));
                return 0;
            }
        }
    }
}