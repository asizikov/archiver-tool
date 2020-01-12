using System.Collections.Concurrent;
using Shouldly;
using Xunit;

namespace GZipTest.Workflow.Tests
{
    public class OutputBufferTests
    {
        private readonly OutputBuffer buffer;
        private readonly int workers = 100;
        private readonly BlockingCollection<ProcessedBatchItem> queue = new BlockingCollection<ProcessedBatchItem>();

        public OutputBufferTests()
        {
            buffer = new OutputBuffer(queue, workers);
        }

        [Fact]
        public void CompletesWorkWhenAllWorkersAreDone()
        {
            queue.IsCompleted.ShouldBeFalse();
            for (var i = 0; i < workers - 1; i++)
            {
                buffer.SubmitCompleted();
            }

            queue.IsCompleted.ShouldBeFalse();
            buffer.SubmitCompleted();
            queue.IsCompleted.ShouldBeTrue();
        }
    }
}