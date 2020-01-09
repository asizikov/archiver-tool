using System.Collections.Concurrent;
using System.Threading;

namespace GZipTest.Workflow.Factories
{
    public interface IJobConsumerFactory
    {
        ChunkProcessor Create(BlockingCollection<JobBatchItem> jobQueue, IOutputBuffer outputBuffer, CountdownEvent countdown, CancellationTokenSource cancellationTokenSource);
    }
}