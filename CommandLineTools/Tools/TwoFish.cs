using System;
using CommandLineTools.Contracts;
using CommandLineTools.Domain;
using CommandLineTools.Helpers;

namespace CommandLineTools.Tools
{
    public class TwoFish : CommandLineFileTool
    {
        public TwoFish(IFileService fileService) : base(fileService)
        {
        }

        public int ExecuteCommand(TwoFishOptions options)
        {
            var log = new VerboseLogger(options);
            var twoFish = new TwoFishCryptor();
            var password = GetPassword();
            if (options.Encrypt)
            {
                var input = _fileService.ReadAllText(options.InputFile);
                var result = twoFish.Encrypt(input, password);
                _fileService.WriteAllBytes(options.OutputFile, result);
            }
            else if (options.Decrypt)
            {
                var input = _fileService.ReadAllBytes(options.InputFile);
                var result = twoFish.Decrypt(input, password);
                _fileService.WriteAllText(options.OutputFile, result);
            }
            else
            {
                //TODO Error no action
                throw new NotImplementedException();
            }
            if (string.IsNullOrEmpty(options.OutputFile))
            {
                //TODO what to do
                throw new NotImplementedException();
            }

            if (options.Temporarily)
            {
                Console.WriteLine(
                    "File encrypted/decrypted. Get the information. When you press any button in this window the temporary file will be deleted");
                Console.ReadKey(true);
                _fileService.DeleteFile(options.OutputFile);
            }
            else if (!options.Keep)
            {
                _fileService.DeleteFile(options.InputFile);
            }

            return 0;
        }

        public string GetPassword()
        {
            Console.WriteLine("Enter your password please:");
            string password = "";

            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    break;
                }
                if (key.Key != ConsoleKey.Backspace)
                {
                    password += key.KeyChar;
                }
                else if(!string.IsNullOrEmpty(password))
                {
                    password = password.Substring(0, password.Length - 1);
                }
            }
            return password;
        }
    }
}