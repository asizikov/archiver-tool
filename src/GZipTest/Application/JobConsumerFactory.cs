using System.Collections.Concurrent;
using System.Threading;
using GZipTest.Compression;
using GZipTest.Workflow;
using GZipTest.Workflow.Factories;
using GZipTest.Workflow.JobConfiguration;

namespace GZipTest.Application
{
    public class JobConsumerFactory : IJobConsumerFactory
    {
        public ChunkProcessor Create(BlockingCollection<JobBatchItem> jobQueue, IOutputBuffer outputBuffer, Operation operation, CountdownEvent countdown)
        {
            var processor = operation == Operation.Compress ? new Compressor() as IByteProcessor : new Decompressor();
            return new ChunkProcessor(jobQueue, outputBuffer, processor, countdown);
        }
    }
}
