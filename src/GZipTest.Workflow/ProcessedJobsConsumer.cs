using System;
using System.Collections.Concurrent;
using System.Threading;
using GZipTest.Workflow.Context;

namespace GZipTest.Workflow
{
    public class ProcessedJobsConsumer
    {
        private readonly BlockingCollection<JobBatchItem> processedJobQueue;
        private readonly CountdownEvent countdown;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly CancellationToken cancellationToken;
        private readonly IJobContext jobContext;

        public ProcessedJobsConsumer(BlockingCollection<JobBatchItem> processedJobQueue, IJobContext jobContext, CountdownEvent countdown, CancellationTokenSource cancellationTokenSource)
        {
            this.processedJobQueue = processedJobQueue;
            this.jobContext = jobContext;
            this.countdown = countdown;
            this.cancellationTokenSource = cancellationTokenSource;
            cancellationToken = cancellationTokenSource.Token;
        }

        public void Start()
        {
            var thread = new Thread(ProcessChunk)
            {
                IsBackground = true
            };
            thread.Start();
            countdown.AddCount();
        }

        private void ProcessChunk()
        {
            try
            {
                while (!processedJobQueue.IsCompleted && !cancellationToken.IsCancellationRequested)
                {
                    JobBatchItem jobBatchItem = null;
                    try
                    {
                        jobBatchItem = processedJobQueue.Take();
                    }
                    catch (InvalidOperationException)
                    {
                        continue;
                    }

                    if (jobBatchItem != null)
                    {
                        jobContext.ProcessedId = jobBatchItem.JobBatchItemId;
                        Console.WriteLine($"Writing processed: {jobBatchItem.JobBatchItemId}/{processedJobQueue.Count}");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}");
                jobContext.Error = e.Message;
                jobContext.Result = ExecutionResult.Failure;
                cancellationTokenSource.Cancel();
            }
            finally
            {
                countdown.Signal();
            }
        }
    }
}