using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace GZipTest.Workflow
{
    public sealed class OrderedConcurrentDictionaryWrapper : IProducerConsumerCollection<ProcessedBatchItem>
    {
        private readonly object syncObject = new object();

        private readonly ConcurrentDictionary<long, ProcessedBatchItem> processedJobs = new ConcurrentDictionary<long, ProcessedBatchItem>();
        private long lastTakenJobId = -1;

        public IEnumerator<ProcessedBatchItem> GetEnumerator()
            => processedJobs.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => processedJobs.Count;
        public bool IsSynchronized { get; } = true;
        public object SyncRoot => syncObject;

        public void CopyTo(ProcessedBatchItem[] array, int index) => throw new NotImplementedException();
        public void CopyTo(Array array, int index) => throw new NotImplementedException();
        public ProcessedBatchItem[] ToArray() => throw new NotImplementedException();

        public bool TryAdd(ProcessedBatchItem item) => processedJobs.TryAdd(item.JobBatchItemId, item);

        public bool TryTake(out ProcessedBatchItem item)
        {
            lock (syncObject)
            {
                var next = lastTakenJobId + 1;
                if (processedJobs.TryRemove(next, out item))
                {
                    lastTakenJobId++;
                    return true;
                }

                var reset = new ManualResetEvent(false);
                PollForNext(next, reset);
                reset.WaitOne();

                if (processedJobs.TryRemove(next, out item))
                {
                    lastTakenJobId++;
                    return true;
                }
                return false;
            }
        }

        private void PollForNext(long next, EventWaitHandle reset)
        {
            var thread = new Thread(() =>
            {
                while (true)
                {
                    if (processedJobs.ContainsKey(next))
                    {
                        reset.Set();
                        return;
                    }

                    Thread.Sleep(100);
                }
            });
            thread.Start();
        }
    }
}