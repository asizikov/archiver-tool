using System.Collections.Concurrent;
using System.Threading;
using GZipTest.Workflow;

namespace GZipTest.Application
{
    public class JobConsumerFactory : IJobConsumerFactory
    {
        private readonly IOutputBuffer outputBuffer;

        public JobConsumerFactory( IOutputBuffer outputBuffer)
        {
            this.outputBuffer = outputBuffer;
        }

        public ChunkProcessor Create(BlockingCollection<JobBatchItem> jobQueue, CountdownEvent countdown)
        {
            return new ChunkProcessor(jobQueue, outputBuffer, countdown);
        }
    }
}
