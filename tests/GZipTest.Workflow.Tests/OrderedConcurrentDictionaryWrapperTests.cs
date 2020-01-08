using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;
using GZipTest.Workflow.Tests.Utils;
using Shouldly;

namespace GZipTest.Workflow.Tests
{
    public class OrderedConcurrentDictionaryWrapperTests
    {
        private readonly OrderedConcurrentDictionaryWrapper wrapper;
        private readonly BlockingCollection<ProcessedBatchItem> queue;

        public OrderedConcurrentDictionaryWrapperTests()
        {
            wrapper = new OrderedConcurrentDictionaryWrapper();
            queue = new BlockingCollection<ProcessedBatchItem>(wrapper);
        }

        [Fact]
        public void ItemsReportedInRandomOrderButConsumedInOrder()
        {
            const int count = 1000;
            var items = Enumerable.Range(0, count).ToList();
            items.Shuffle();
            var inQueue = new BlockingCollection<int>(new ConcurrentQueue<int>(items));
            inQueue.CompleteAdding();

            using var countdownEvent = new CountdownEvent(1);

            var producer1 = new Thread(
                () =>
                {
                    foreach (var item in inQueue.GetConsumingEnumerable())
                    {
                        queue.TryAdd(new ProcessedBatchItem
                        {
                            JobBatchItemId = item
                        });
                    }

                    countdownEvent.Signal();
                }
            );
            countdownEvent.AddCount();

            var producer2 = new Thread(
                () =>
                {
                    foreach (var item in inQueue.GetConsumingEnumerable())
                    {
                        queue.TryAdd(new ProcessedBatchItem
                        {
                            JobBatchItemId = item
                        });
                    }

                    countdownEvent.Signal();
                }
            );
            countdownEvent.AddCount();

            var list = new List<long>();
            var consumer = new Thread(() =>
            {
                foreach (var processedBatchItem in queue.GetConsumingEnumerable())
                {
                    list.Add(processedBatchItem.JobBatchItemId);
                }
            });

            consumer.Start();
            producer1.Start();
            producer2.Start();

            countdownEvent.Signal();
            countdownEvent.Wait();
            queue.CompleteAdding();

            consumer.Join();

            list.ShouldSatisfyAllConditions(
                () => list.Count.ShouldBe(count),
                () => list.SequenceEqual(Enumerable.Range(0, count).Select(i => (long) i)).ShouldBeTrue()
            );
        }
    }
}