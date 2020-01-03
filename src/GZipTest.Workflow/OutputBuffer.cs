using System;
using System.Threading;
using GZipTest.IO;

namespace GZipTest.Workflow
{
    public class OutputBuffer : IOutputBuffer
    {

        private long size = 0;

        public void SubmitProcessedBatchItem(JobBatchItem processedBatchItem)
        {
            
            Interlocked.Add(ref size, processedBatchItem.Processed.LongLength);
            Console.WriteLine($"submitted processed batch item {processedBatchItem?.JobBatchItemId}, total size {ConvertBytesToMegabytes(size)} mb");
            //currentBatch[processedBatchItem.JobBatchItemId % currentBatch.Length] = processedBatchItem;
        }



        private static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }
    }

    public interface IOutputBuffer
    {
        void SubmitProcessedBatchItem(JobBatchItem processedBatchItem);
    }
}