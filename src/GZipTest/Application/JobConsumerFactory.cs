using System.Collections.Concurrent;
using System.Threading;
using GZipTest.Workflow;
using GZipTest.Workflow.Factories;

namespace GZipTest.Application
{
    public class JobConsumerFactory : IJobConsumerFactory
    {
        public ChunkProcessor Create(BlockingCollection<JobBatchItem> jobQueue, IOutputBuffer outputBuffer, CountdownEvent countdown) 
            => new ChunkProcessor(jobQueue, outputBuffer, countdown);
    }
}
