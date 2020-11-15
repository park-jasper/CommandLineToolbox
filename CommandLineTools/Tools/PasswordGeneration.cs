using CommandLineTools.Options;
using System;

namespace CommandLineTools.Tools
{
    public class PasswordGeneration : ICommandLineTool<PasswordGenerationOptions>
    {
        public int ExecuteCommand(PasswordGenerationOptions options)
        {
            var random = new System.Security.Cryptography.RNGCryptoServiceProvider();
            var result = new byte[options.Length];
            random.GetBytes(result);
            Console.WriteLine(Convert.ToBase64String(result));
            return 0;
        }
    }
}