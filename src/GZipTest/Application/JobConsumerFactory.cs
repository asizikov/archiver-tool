using System.Collections.Concurrent;
using System.Threading;
using GZipTest.Compression;
using GZipTest.IO;
using GZipTest.Workflow;
using GZipTest.Workflow.Context;
using GZipTest.Workflow.Factories;
using GZipTest.Workflow.JobConfiguration;
using Microsoft.IO;

namespace GZipTest.Application
{
    public class JobConsumerFactory : IJobConsumerFactory
    {
        private readonly IJobContext jobContext;
        private RecyclableMemoryStreamManager recyclableMemoryStreamManager;

        public JobConsumerFactory(IJobContext jobContext)
        {
            recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
            this.jobContext = jobContext;
        }

        public ChunkProcessor Create(BlockingCollection<FileChunk> jobQueue, IOutputBuffer outputBuffer, CountdownEvent countdown, CancellationTokenSource cancellationTokenSource)
        {
            var processor = jobContext.Operation == Operation.Compress ? new Compressor(recyclableMemoryStreamManager) as IByteProcessor : new Decompressor(recyclableMemoryStreamManager);
            return new ChunkProcessor(jobQueue, outputBuffer, processor, jobContext, cancellationTokenSource, countdown);
        }
    }
}
