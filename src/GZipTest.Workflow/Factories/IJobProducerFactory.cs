using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace GZipTest.Workflow.Factories
{
    public interface IJobProducerFactory
    {
        JobProducer Create(FileInfo fileInfo, BlockingCollection<JobBatchItem> jobQueue, CountdownEvent countdown);
    }
}