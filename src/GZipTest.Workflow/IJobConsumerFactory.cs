using System.Collections.Concurrent;
using System.Threading;

namespace GZipTest.Workflow
{
    public interface IJobConsumerFactory
    {
        ChunkProcessor Create(BlockingCollection<JobBatchItem> jobQueue, CountdownEvent countdown);
    }
}