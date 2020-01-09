using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using GZipTest.IO;
using GZipTest.Workflow.Context;
using GZipTest.Workflow.JobConfiguration;

namespace GZipTest.Workflow
{
    public sealed class ProcessedJobsConsumer
    {
        private readonly BlockingCollection<ProcessedBatchItem> processedJobQueue;
        private readonly CountdownEvent countdown;
        private readonly CancellationTokenSource cancellationTokenSource;

        private readonly IJobContext jobContext;
        private readonly IFileWriter fileWriter;
        private CancellationToken cancellationToken;
        private FileInfo fileInfo;

        public ProcessedJobsConsumer(BlockingCollection<ProcessedBatchItem> processedJobQueue, IJobContext jobContext, IFileWriter fileWriter, CountdownEvent countdown, CancellationTokenSource cancellationTokenSource)
        {
            this.processedJobQueue = processedJobQueue;
            this.jobContext = jobContext;
            this.fileWriter = fileWriter;
            this.countdown = countdown;
            this.cancellationTokenSource = cancellationTokenSource;
        }

        public void Start(FileInfo path, CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            fileInfo = path;

            var thread = new Thread(SaveProcessedChunks)
            {
                IsBackground = true
            };
            thread.Start();
            countdown.AddCount();
        }

        private void SaveProcessedChunks()
        {
            try
            {
                using var file = fileWriter.OpenFile(fileInfo, jobContext.Operation == Operation.Decompress);
                foreach (var processedBatchItem in processedJobQueue.GetConsumingEnumerable())
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        jobContext.ProcessedId = processedBatchItem.JobBatchItemId;
                        file.Write(processedBatchItem.Processed.Buffer, processedBatchItem.Processed.Size);
                        ArrayPool<byte>.Shared.Return(processedBatchItem.Processed.Buffer);
                    }
                    else
                    {
                        break;
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
        }
    }
}