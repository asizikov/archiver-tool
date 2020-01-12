using System.Collections.Concurrent;
using System.Threading;

namespace GZipTest.Workflow.Factories
{
    public interface IProcessedJobConsumerFactory
    {
        ProcessedJobsConsumer Create(BlockingCollection<ProcessedBatchItem> processedJobQueue, CountdownEvent countdown,
            CancellationTokenSource cancellationTokenSource);
    }
}