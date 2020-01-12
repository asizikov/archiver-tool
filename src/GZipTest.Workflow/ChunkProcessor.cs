using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Threading;
using GZipTest.Compression;
using GZipTest.IO;
using GZipTest.Workflow.Context;

namespace GZipTest.Workflow
{
    public sealed class ChunkProcessor
    {
        private readonly BlockingCollection<FileChunk> jobQueue;
        private readonly IOutputBuffer outputBuffer;
        private readonly IByteProcessor byteProcessor;
        private readonly IJobContext jobContext;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly CountdownEvent countdown;
        private Thread workThread;
        private CancellationToken cancellationToken;

        public ChunkProcessor(BlockingCollection<FileChunk> jobQueue, IOutputBuffer outputBuffer,
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
                        Processed = byteProcessor.Process(jobBatchItem.Memory)
                    };
                    jobBatchItem.ReleaseBuffer();

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