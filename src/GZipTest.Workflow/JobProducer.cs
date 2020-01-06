using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using GZipTest.IO;
using GZipTest.Workflow.Context;

namespace GZipTest.Workflow
{
    public class JobProducer
    {
        private readonly IFileReader fileReader;
        private readonly IJobContext jobContext;
        private readonly BlockingCollection<JobBatchItem> queue;
        private readonly FileInfo fileInfo;
        private readonly CountdownEvent countdown;

        private long batchItemId = 0;
        private CancellationTokenSource cancellationTokenSource;

        public JobProducer(IFileReader fileReader, IJobContext jobContext, BlockingCollection<JobBatchItem> queue, FileInfo fileInfo,
            CountdownEvent countdown)
        {
            this.fileReader = fileReader;
            this.jobContext = jobContext;
            this.queue = queue;
            this.fileInfo = fileInfo;
            this.countdown = countdown;
        }

        public void Start(CancellationTokenSource tokenSource)
        {
            cancellationTokenSource = tokenSource;
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
                foreach (var chunk in fileReader.Read(fileInfo))
                {
                    queue.Add(new JobBatchItem
                    {
                        Buffer = chunk,
                        JobBatchItemId = batchItemId
                    });
                    jobContext.SubmittedId = batchItemId;
                    Interlocked.Increment(ref batchItemId);
                }
            }
            catch (IOException e)
            {
                jobContext.Error = e.Message;
                jobContext.Result = ExecutionResult.Failure;
                cancellationTokenSource.Cancel();
            }

            queue.CompleteAdding();
            countdown.Signal();
        }
    }
}