using System.Collections.Concurrent;
using System.Threading;
using GZipTest.Workflow.JobConfiguration;

namespace GZipTest.Workflow.Factories
{
    public interface IJobConsumerFactory
    {
        ChunkProcessor Create(BlockingCollection<JobBatchItem> jobQueue, IOutputBuffer outputBuffer, Operation operation, CountdownEvent countdown);
    }
}