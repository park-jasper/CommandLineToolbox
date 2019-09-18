using System;
using System.Runtime.CompilerServices;

namespace CommandLineTools.Helpers
{
    public class VerboseLogger
    {
        private readonly BaseOptions _options;
        public VerboseLogger(BaseOptions options)
        {
            _options = options;
        }
        public void Info(string text, [CallerFilePath] string callerFile = null, [CallerMemberName] string callerName = null, [CallerLineNumber] int callerLine = -1)
        {
            if (_options.Verbose)
            {
                var debugLog = _options.DebugLog ? $"{callerFile}::{callerName}::Line#{callerLine}: " : "";
                Console.WriteLine(debugLog + text);
            }
        }

        public void Error(string text, [CallerFilePath] string callerFile = null,
            [CallerMemberName] string callerName = null, [CallerLineNumber] int callerLine = -1) =>
            Error(null, text, callerFile, callerName, callerLine);
        
        public void Error(Exception exc, string text, [CallerFilePath] string callerFile = null, [CallerMemberName] string callerName = null, [CallerLineNumber] int callerLine = -1)
        {
            var debugLog = _options.DebugLog ? $"{callerFile}::{callerName}::Line#{callerLine}: " : "";
            var excString = exc?.ToString() ?? "";
            Console.WriteLine($"{debugLog}{excString};{text}");
        }
    }
}