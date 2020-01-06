using System.Collections.Concurrent;

namespace GZipTest.Workflow.Factories
{
    public interface IOutputBufferFactory
    {
        IOutputBuffer Create(BlockingCollection<JobBatchItem> processedItemsQueue, int processorsCount);
    }
}