using System;
using System.Collections.Concurrent;
using System.Threading;
using GZipTest.Compression;

namespace GZipTest.Workflow
{
    public class ChunkProcessor
    {
        private readonly BlockingCollection<JobBatchItem> jobQueue;
        private readonly IOutputBuffer outputBuffer;
        private readonly IByteProcessor byteProcessor;
        private readonly CountdownEvent countdown;
        private Thread workThread;
        private CancellationToken cancellationToken;

        public ChunkProcessor(BlockingCollection<JobBatchItem> jobQueue, IOutputBuffer outputBuffer, IByteProcessor byteProcessor,
            CountdownEvent countdown)
        {
            this.jobQueue = jobQueue;
            this.outputBuffer = outputBuffer;
            this.byteProcessor = byteProcessor;
            this.countdown = countdown;
        }

        public void Start(CancellationToken token)
        {
            cancellationToken = token;
            workThread = new Thread(ProcessChunk)
            {
                IsBackground = true
            };
            workThread.Start();
            countdown.AddCount();
        }

        private void ProcessChunk()
        {
            foreach (var jobBatchItem in jobQueue.GetConsumingEnumerable())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var processed = new ProcessedBatchItem
                {
                    JobBatchItemId = jobBatchItem.JobBatchItemId
                };
                try
                {
                    processed.Processed = byteProcessor.Process(jobBatchItem.Buffer);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                if (!cancellationToken.IsCancellationRequested)
                {
                    outputBuffer.SubmitProcessedBatchItem(processed);
                }
            }

            outputBuffer.SubmitCompleted();
            countdown.Signal();
        }
    }
}