﻿using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Threading;
using GZipTest.IO;
using GZipTest.Workflow.Context;
using GZipTest.Workflow.JobConfiguration;

namespace GZipTest.Workflow
{
    public sealed class JobProducer
    {
        private readonly IFileReaderFactory fileReaderFactory;
        private readonly IJobContext jobContext;
        private readonly BlockingCollection<FileChunk> queue;
        private readonly FileInfo fileInfo;
        private readonly CountdownEvent countdown;

        private long batchItemId = 0;
        private CancellationTokenSource cancellationTokenSource;

        public JobProducer(IFileReaderFactory fileReaderFactory, IJobContext jobContext,
            BlockingCollection<FileChunk> queue, FileInfo fileInfo,
            CountdownEvent countdown)
        {
            this.fileReaderFactory = fileReaderFactory;
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
            var fileReader = fileReaderFactory.Create(jobContext.Operation == Operation.Decompress);

            try
            {
                foreach (var chunk in fileReader.Read(fileInfo))
                {
                    var local = chunk;
                    local.JobBatchItemId = batchItemId;
                    queue.Add(local);
                    jobContext.SubmittedId = batchItemId;
                    Interlocked.Increment(ref batchItemId);
                }
            }
            catch (IOException e)
            {
                jobContext.Failure(e, e.Message);
                cancellationTokenSource.Cancel();
            }
            finally
            {
                countdown.Signal();
            }

            queue.CompleteAdding();
        }
    }
}