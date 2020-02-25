using System;
using CommandLineTools.Contracts;
using CommandLineTools.Domain;
using CommandLineTools.Helpers;

namespace CommandLineTools.Tools
{
    public class CryptoTool : CommandLineFileTool, ICommandLineTool<CryptoOptions>
    {
        public const string EncryptedPostfix = ".encrypted";
        public CryptoTool()
        {
        }

        public void GenerateOutputFileName(CryptoOptions options)
        {
            if (options.Encrypt)
            {
                if (!_fileService.Exists(options.InputFile + EncryptedPostfix))
                {
                    options.OutputFile = options.InputFile + EncryptedPostfix;
                }
                else
                {
                    var counter = 2;
                    while (true)
                    {
                        var combinedName = $"{options.InputFile}_{counter}{EncryptedPostfix}";
                        if (!_fileService.Exists(combinedName))
                        {
                            options.OutputFile = combinedName;
                            break;
                        }
                        counter += 1;
                    }
                }
            }
            else if (options.Decrypt)
            {
                if (options.InputFile.EndsWith(EncryptedPostfix))
                {
                    var withoutName = options.InputFile.Remove(options.InputFile.LastIndexOf(EncryptedPostfix));
                    if (!withoutName.Contains("."))
                    {
                        withoutName += ".decrypted";
                    }
                    if (!_fileService.Exists(withoutName))
                    {
                        options.OutputFile = withoutName;
                    }
                    else
                    {
                        var counter = 2;
                        var dotPosition = withoutName.LastIndexOf('.');
                        var namePart = withoutName.Remove(dotPosition);
                        var extensionPart = withoutName.Substring(dotPosition + 1, withoutName.Length - dotPosition - 1);
                        if (string.IsNullOrEmpty(extensionPart))
                        {
                            extensionPart = "decrypted";
                        }
                        while (true)
                        {
                            var combinedName = $"{namePart}_{counter}.{extensionPart}";
                            if (!_fileService.Exists(combinedName))
                            {
                                options.OutputFile = combinedName;
                                break;
                            }
                            counter += 1;
                        }
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public int ExecuteCommand(CryptoOptions options)
        {
            var log = new VerboseLogger(options);
            var twoFish = new AESCryptor();
            var password = GetPassword();
            if (string.IsNullOrEmpty(options.OutputFile))
            {
                GenerateOutputFileName(options);
            }
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