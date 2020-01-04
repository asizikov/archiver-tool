using System.IO;
using GZipTest.Workflow.JobConfiguration;

namespace GZipTest.CommandLineArguments
{
    public interface IArgumentsParser
    {
        JobDescription Parse(string[] args);
    }

    public class ArgumentsParser : IArgumentsParser
    {
        public JobDescription Parse(string[] args)
        {
            return new JobDescription 
            {
                Operation = args[0].ToLower() ==  "compress" ? Operation.Compress : Operation.Decompress,
                InputFile = new FileInfo(args[1]),
                OutputFile = new FileInfo(args[1])
            };
        }
    }
}