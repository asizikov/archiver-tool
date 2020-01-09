using System.Collections.Concurrent;
using System.Threading;
using GZipTest.IO;

namespace GZipTest.Workflow.Factories
{
    public interface IJobConsumerFactory
    {
        ChunkProcessor Create(BlockingCollection<FileChunk> jobQueue, IOutputBuffer outputBuffer, CountdownEvent countdown, CancellationTokenSource cancellationTokenSource);
    }
}