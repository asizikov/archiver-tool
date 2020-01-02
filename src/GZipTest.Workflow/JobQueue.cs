using System;
using System.Collections.Concurrent;
using System.Threading;

namespace GZipTest.Workflow
{
    public class JobQueue
    {
        private readonly int bufferSize;
        private readonly ConcurrentQueue<JobBatchItem> queue;
        private long batchId;

        public JobQueue(int bufferSize)
        {
            batchId = 0;
            this.bufferSize = bufferSize;
            queue = new ConcurrentQueue<JobBatchItem>();
        }

        // Not thread safe, used by one file reader thread
        public void Enqueue(byte[] memoryBuffer)
        {
            queue.Enqueue(new JobBatchItem
            {
                Buffer = memoryBuffer,
                JobBatchItemId = batchId
            });
            batchId++;
        }

        public bool IsFull => queue.Count == bufferSize;

        public JobBatchItem Dequeue()
        {
            if (queue.TryDequeue(out var batch))
            {
                return batch;
            }
            else
            {
                return null;
            }
        }
    }
}