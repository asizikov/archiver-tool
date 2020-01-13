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

        private readonly IDictionary<long, ProcessedBatchItem> processedJobs =
            new Dictionary<long, ProcessedBatchItem>();

        private long lastTakenJobId = -1;

        public bool IsSynchronized { get; } = true;
        public object SyncRoot => syncObject;

        public int Count
        {
            get
            {
                lock (syncObject)
                {
                    return processedJobs.Count;
                }
            }
        }

        public IEnumerator<ProcessedBatchItem> GetEnumerator() => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
        public void CopyTo(ProcessedBatchItem[] array, int index) => throw new NotImplementedException();
        public void CopyTo(Array array, int index) => throw new NotImplementedException();
        public ProcessedBatchItem[] ToArray() => throw new NotImplementedException();

        public bool TryAdd(ProcessedBatchItem item)
        {
            lock (syncObject)
            {
                if (processedJobs.ContainsKey(item.JobBatchItemId))
                {
                    return false;
                }

                processedJobs.Add(item.JobBatchItemId, item);
                Monitor.PulseAll(syncObject);
                return true;
            }
        }

        public bool TryTake(out ProcessedBatchItem item)
        {
            lock (syncObject)
            {
                var next = lastTakenJobId + 1;
                while (!processedJobs.ContainsKey(next))
                {
                    Monitor.Wait(syncObject);
                }

                item = processedJobs[next];
                processedJobs.Remove(next);
                lastTakenJobId++;
                Monitor.PulseAll(syncObject);
                return true;
            }
        }
    }
}