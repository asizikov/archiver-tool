using System.Collections.Concurrent;
using System.Threading;
using GZipTest.IO;
using GZipTest.Workflow.Context;

namespace GZipTest.Workflow.Factories
{
    public class ProcessedJobConsumerFactory : IProcessedJobConsumerFactory
    {
        private readonly IJobContext jobContext;
        private readonly IFileWriter fileWriter;

        public ProcessedJobConsumerFactory(IJobContext jobContext, IFileWriter fileWriter)
        {
            this.jobContext = jobContext;
            this.fileWriter = fileWriter;
        }
        public ProcessedJobsConsumer Create(BlockingCollection<ProcessedBatchItem> processedJobQueue, CountdownEvent countdown, CancellationTokenSource cancellationTokenSource) 
            => new ProcessedJobsConsumer(processedJobQueue, jobContext, fileWriter, countdown, cancellationTokenSource);
    }
}