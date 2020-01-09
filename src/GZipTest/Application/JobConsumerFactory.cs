using System.Collections.Concurrent;
using System.Threading;
using GZipTest.Compression;
using GZipTest.Workflow;
using GZipTest.Workflow.Context;
using GZipTest.Workflow.Factories;
using GZipTest.Workflow.JobConfiguration;

namespace GZipTest.Application
{
    public class JobConsumerFactory : IJobConsumerFactory
    {
        private readonly IJobContext jobContext;

        public JobConsumerFactory(IJobContext jobContext)
        {
            this.jobContext = jobContext;
        }

        public ChunkProcessor Create(BlockingCollection<JobBatchItem> jobQueue, IOutputBuffer outputBuffer, CountdownEvent countdown, CancellationTokenSource cancellationTokenSource)
        {
            var processor = jobContext.Operation == Operation.Compress ? new Compressor() as IByteProcessor : new Decompressor();
            return new ChunkProcessor(jobQueue, outputBuffer, processor, jobContext, cancellationTokenSource, countdown);
        }
    }
}
