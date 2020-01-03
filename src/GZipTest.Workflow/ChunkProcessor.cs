using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace GZipTest.Workflow
{
    public class ChunkProcessor
    {
        private readonly BlockingCollection<JobBatchItem> jobQueue;
        private readonly IOutputBuffer outputBuffer;
        private readonly CountdownEvent countdown;
        private Thread workThread;
        private CancellationToken cancellationToken;

        public ChunkProcessor(BlockingCollection<JobBatchItem> jobQueue, IOutputBuffer outputBuffer,
            CountdownEvent countdown)
        {
            this.jobQueue = jobQueue;
            this.outputBuffer = outputBuffer;
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
            while (!jobQueue.IsCompleted && !cancellationToken.IsCancellationRequested)
            {
                JobBatchItem jobBatchItem = null;
                try
                {
                    jobBatchItem = jobQueue.Take();
                }
                catch (InvalidOperationException _)
                {
                    break;
                }

                if (jobBatchItem != null)
                {
                    var sw = Stopwatch.StartNew();
                    Console.WriteLine($"{workThread.ManagedThreadId}: running chunk {jobBatchItem.JobBatchItemId}");
                    Thread.Sleep(500);
                    jobBatchItem.Processed = jobBatchItem.Buffer;
                    jobBatchItem.ElapsedTime = sw.ElapsedMilliseconds;
                    Console.WriteLine(
                        $"processed chunk {jobBatchItem.JobBatchItemId} in {jobBatchItem.ElapsedTime} ms");
                    //compress
                    outputBuffer.SubmitProcessedBatchItem(jobBatchItem);
                }
            }

            countdown.Signal();
        }
    }
}