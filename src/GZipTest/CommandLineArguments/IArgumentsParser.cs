using GZipTest.Workflow.JobConfiguration;

namespace GZipTest.CommandLineArguments
{
    public interface IArgumentsParser
    {
        JobDescription Parse(string[] args);
    }
}