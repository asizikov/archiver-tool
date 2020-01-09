using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using GZipTest.Workflow.Context;
using GZipTest.Workflow.Factories;
using GZipTest.Workflow.JobConfiguration;
using Microsoft.Extensions.Logging;

namespace GZipTest.Workflow
{
    public sealed class JobBatchOrchestrator : IJobBatchOrchestrator
    {
        private readonly IJobProducerFactory jobProducerFactory;
        private readonly ILogger<JobBatchOrchestrator> logger;
        private readonly IJobConsumerFactory jobConsumerFactory;
        private readonly IProcessedJobConsumerFactory processedJobConsumerFactory;
        private readonly IOutputBufferFactory outputBufferFactory;
        private readonly IJobContext jobContext;
        private readonly ChunkProcessor[] chunkProcessorPool;

        public JobBatchOrchestrator(IJobProducerFactory jobProducerFactory,
            IJobConsumerFactory jobConsumerFactory,
            IProcessedJobConsumerFactory processedJobConsumerFactory,
            IOutputBufferFactory outputBufferFactory,
            IJobContext jobContext,
            ILogger<JobBatchOrchestrator> logger)
        {
            this.jobProducerFactory = jobProducerFactory;
            this.logger = logger;
            this.jobConsumerFactory = jobConsumerFactory;
            this.processedJobConsumerFactory = processedJobConsumerFactory;
            this.outputBufferFactory = outputBufferFactory;
            this.jobContext = jobContext;
            chunkProcessorPool = new ChunkProcessor[100];
        }

        public void StartProcess(JobDescription description)
        {
            var stopWatch = Stopwatch.StartNew();
            logger.LogInformation(
                $"Ready to perform operation: {description.Operation} on file {description.InputFile.Name}");

            jobContext.Operation = description.Operation;

            using (var countdown = new CountdownEvent(1))
            {
                using var cancellationTokenSource = new CancellationTokenSource();
                using var jobQueue = new BlockingCollection<JobBatchItem>(new ConcurrentQueue<JobBatchItem>(), chunkProcessorPool.Length * 10);
                using var processedJobQueue = new BlockingCollection<ProcessedBatchItem>(new OrderedConcurrentDictionaryWrapper(), chunkProcessorPool.Length * 10);

                var outputBuffer = outputBufferFactory.Create(processedJobQueue, chunkProcessorPool.Length);
                for (var i = 0; i < chunkProcessorPool.Length; i++)
                {
                    chunkProcessorPool[i] = jobConsumerFactory.Create(jobQueue, outputBuffer, countdown, cancellationTokenSource);
                    chunkProcessorPool[i].Start(cancellationTokenSource.Token);
                }

                processedJobConsumerFactory.Create(processedJobQueue, countdown, cancellationTokenSource)
                    .Start(description.OutputFile, cancellationTokenSource.Token);

                jobProducerFactory.Create(description.InputFile, jobQueue, countdown)
                    .Start(cancellationTokenSource);

                countdown.Signal();
                countdown.Wait();
            }
            logger.LogInformation($"Completed processing of file. Submitted {jobContext.SubmittedId} chunks. Processed {jobContext.ProcessedId} chunks");
            jobContext.ElapsedTimeMilliseconds = stopWatch.ElapsedMilliseconds;
        }
    }
}