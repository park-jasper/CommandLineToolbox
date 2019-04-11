using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLineToolbox.Services;

namespace CommandLineToolbox
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new CommandLineTools
                .CommandLineTools(new FileService())
                .Run(args);
        }
    }
}
