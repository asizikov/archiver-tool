using System.Collections.Concurrent;

namespace GZipTest.Workflow
{
    public class OutputBuffer : IOutputBuffer
    {
        private readonly object syncRoot = new object();
        private int producers;

        private readonly BlockingCollection<JobBatchItem> processedJobQueue;

        public OutputBuffer(BlockingCollection<JobBatchItem> processedJobQueue, int count)
        {
            producers = count;
            this.processedJobQueue = processedJobQueue;
        }


        public void SubmitProcessedBatchItem(JobBatchItem processedBatchItem) 
            => processedJobQueue.Add(processedBatchItem);

        public void SubmitCompleted()
        {
            lock (syncRoot)
            {
                producers--;
                if (producers == 0)
                {
                    processedJobQueue.CompleteAdding();
                }
            }
        }

        private static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }
    }
}