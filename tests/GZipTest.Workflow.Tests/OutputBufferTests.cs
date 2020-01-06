using System.Collections.Generic;
using GZipTest.Workflow.Tests.Utils;
using Shouldly;
using Xunit;

namespace GZipTest.Workflow.Tests
{
    public class OutputBufferTests
    {
        private readonly OutputBuffer buffer;

        public OutputBufferTests()
        {
            buffer = null; //new OutputBuffer();
        }

        [Fact]
        public void BatchItemsSubmittedInOrder()
        {
            var list = new List<JobBatchItem>();
            var upper = 1000;
            var lower = 123L;

            for (var i = lower; i <= upper; i++)
            {
                list.Add(new JobBatchItem
                {
                    JobBatchItemId = i
                });
            }


            list.Shuffle();
            foreach (var jobBatchItem in list)
            {
                buffer.SubmitProcessedBatchItem(jobBatchItem);
            }

            var expected = lower;
            for (var i = 0L; i < (lower - upper); i++)
            {
                expected++;
            }
        }
    }
}