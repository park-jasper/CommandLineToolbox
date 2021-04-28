using System.Collections.Generic;
using CommandLineTools.Contracts;
using Newtonsoft.Json;

namespace CommandLineTools.Tools
{
    public class ExecuteBatch : CommandLineFileTool<ExecuteBatchOptions>
    {
        public ExecuteBatch()
        {

        }

        public override int ExecuteCommand(ExecuteBatchOptions options)
        {
            var batchInstructionsText = FileService.ReadAllText(options.InstructionFile);
            var test = new List<BatchInstruction>()
            {
                new BatchInstruction { Location = "someString", Command = "SomeCommand" }
            };
            var testText = JsonConvert.SerializeObject(test);
            var batchInstructions = JsonConvert.DeserializeObject<List<BatchInstruction>>(batchInstructionsText);
            foreach (var instruction in batchInstructions)
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo =
                    {
                        CreateNoWindow = true,
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                        FileName = "cmd.exe",
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        WorkingDirectory = instruction.Location
                    },
                };
                process.Start();
                process.StandardInput.WriteLine(instruction.Command);
            }

            return 0;
        }

        private class BatchInstruction
        {
            public string Location { get; set; }
            public string Command { get; set; }
        }
    }

}