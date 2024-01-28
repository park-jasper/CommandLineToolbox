using System;
using System.IO;
using System.Security.Cryptography;
using CommandLineTools.Options;

namespace CommandLineTools.Tools
{
    public class ChecksumTool : CommandLineFileTool<ChecksumOptions>
    {
        public byte[] CalculateChecksum(ChecksumOptions options)
        {
            HashAlgorithm algorithm;
            switch (options.Algorithm.ToLower())
            {
                case ChecksumOptions.MD5:
                    algorithm = MD5.Create();
                    break;
                case ChecksumOptions.SHA1:
                    algorithm = SHA1.Create();
                    break;
                case ChecksumOptions.SHA256:
                    algorithm = SHA256.Create();
                    break;
                default:
                    return null;
            }

            using (var stream = File.OpenRead(options.InFile))
            {
                algorithm.ComputeHash(stream);
                return algorithm.Hash;
            }
        }

        public override int ExecuteCommand(ChecksumOptions options)
        {
            var hash = this.CalculateChecksum(options);
            if (hash is null)
            {
                Console.WriteLine($"Unknown or unsupported algorithm '{options.Algorithm}'.");
                return 1;
            }
            Console.WriteLine(BitConverter.ToString(hash).Replace("-", string.Empty));
            return 0;
        }
    }
}