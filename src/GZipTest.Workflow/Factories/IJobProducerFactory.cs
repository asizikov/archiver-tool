using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using GZipTest.IO;

namespace GZipTest.Workflow.Factories
{
    public interface IJobProducerFactory
    {
        JobProducer Create(FileInfo fileInfo, BlockingCollection<FileChunk> jobQueue, CountdownEvent countdown);
    }
}