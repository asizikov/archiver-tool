using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using GZipTest.IO;
using GZipTest.Workflow;
using GZipTest.Workflow.Context;
using GZipTest.Workflow.Factories;

namespace GZipTest.Application
{
    public class JobProducerFactory : IJobProducerFactory
    {
        private readonly IFileReaderFactory fileReader;
        private readonly IJobContext jobContext;

        public JobProducerFactory(IFileReaderFactory fileReader, IJobContext jobContext)
        {
            this.fileReader = fileReader;
            this.jobContext = jobContext;
        }
        public JobProducer Create(FileInfo fileInfo,
            BlockingCollection<JobBatchItem> jobQueue, CountdownEvent countdown)
        {
            return new JobProducer(fileReader, jobContext, jobQueue, fileInfo, countdown);
        }
    }
}