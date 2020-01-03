using System.IO;

namespace GZipTest.Workflow.JobConfiguration
{
    public class JobDescription
    {
        public FileInfo InputFile { get; set; }
        public FileInfo OutputFile { get; set; }
        public Operation Operation { get; set; }
    }
}