using CommandLine;

namespace CommandLineTools.Options
{
    [Verb("checksum")]
    public class ChecksumOptions
    {
        public const string MD5 = "md5";
        public const string SHA1 = "sha1";
        public const string SHA256 = "sha256";

        [Option('a', "algorithm", Required = true, HelpText = "Which Hash Algorithm to use. Currently supported: '" + MD5 + "', '" + SHA1 + "', '" + SHA256 +  "'.")]
        public string Algorithm { get; set; }

        [Option('i', "inFile", Required = true, HelpText = "File of which the content is to be hashed")]
        public string InFile { get; set; }
    }
}