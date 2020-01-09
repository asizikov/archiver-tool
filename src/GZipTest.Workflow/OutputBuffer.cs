﻿using System.Collections.Concurrent;

namespace GZipTest.Workflow
{
    public sealed class OutputBuffer : IOutputBuffer
    {
        private readonly object syncRoot = new object();
        private int producers;

        private readonly BlockingCollection<ProcessedBatchItem> processedJobQueue;

        public OutputBuffer(BlockingCollection<ProcessedBatchItem> processedJobQueue, int count)
        {
            producers = count;
            this.processedJobQueue = processedJobQueue;
        }


        public void SubmitProcessedBatchItem(ProcessedBatchItem processedBatchItem)
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
    }
}