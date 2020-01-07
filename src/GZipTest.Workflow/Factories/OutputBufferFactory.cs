using System.Collections.Concurrent;

namespace GZipTest.Workflow.Factories
{
    public class OutputBufferFactory : IOutputBufferFactory
    {
        public IOutputBuffer Create(BlockingCollection<ProcessedBatchItem> processedItemsQueue, int processorsCount) 
            => new OutputBuffer(processedItemsQueue, processorsCount);
    }
}