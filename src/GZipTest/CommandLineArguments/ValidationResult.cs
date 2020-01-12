using System.Collections.Generic;

namespace GZipTest.CommandLineArguments
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; } = new List<string>();
    }
}