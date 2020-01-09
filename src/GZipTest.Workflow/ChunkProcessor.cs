using System;
using System.Collections.Concurrent;
using System.Threading;
using GZipTest.Compression;
using GZipTest.Workflow.Context;

namespace GZipTest.Workflow
{
    public sealed class ChunkProcessor
    {
        private readonly BlockingCollection<JobBatchItem> jobQueue;
        private readonly IOutputBuffer outputBuffer;
        private readonly IByteProcessor byteProcessor;
        private readonly IJobContext jobContext;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly CountdownEvent countdown;
        private Thread workThread;
        private CancellationToken cancellationToken;

        public ChunkProcessor(BlockingCollection<JobBatchItem> jobQueue, IOutputBuffer outputBuffer,
            IByteProcessor byteProcessor, IJobContext jobContext, CancellationTokenSource cancellationTokenSource,
            CountdownEvent countdown)
        {
            this.jobQueue = jobQueue;
            this.outputBuffer = outputBuffer;
            this.byteProcessor = byteProcessor;
            this.jobContext = jobContext;
            this.cancellationTokenSource = cancellationTokenSource;
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
            try
            {
                foreach (var jobBatchItem in jobQueue.GetConsumingEnumerable())
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    var processed = new ProcessedBatchItem
                    {
                        JobBatchItemId = jobBatchItem.JobBatchItemId,
                        Processed = byteProcessor.Process(jobBatchItem.Buffer)
                    };
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        outputBuffer.SubmitProcessedBatchItem(processed);
                    }
                }
            }
            catch (Exception e)
            {
                jobContext.Failure(e, e.Message);
                cancellationTokenSource.Cancel();
            }
            finally
            {
                countdown.Signal();
            }

            outputBuffer.SubmitCompleted();
        }
    }
}