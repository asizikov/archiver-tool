using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using GZipTest.Workflow.Context;
using GZipTest.Workflow.Factories;
using GZipTest.Workflow.JobConfiguration;
using Microsoft.Extensions.Logging;

namespace GZipTest.Workflow
{
    public class JobBatchOrchestrator : IJobBatchOrchestrator
    {
        private readonly IJobProducerFactory jobProducerFactory;
        private readonly ILogger<JobBatchOrchestrator> logger;
        private readonly IJobConsumerFactory jobConsumerFactory;
        private readonly IOutputBufferFactory outputBufferFactory;
        private readonly IJobContext jobContext;
        private readonly ChunkProcessor[] chunkProcessorPool;

        public JobBatchOrchestrator(IJobProducerFactory jobProducerFactory,
            IJobConsumerFactory jobConsumerFactory,
            IOutputBufferFactory outputBufferFactory,
            IJobContext jobContext,
            ILogger<JobBatchOrchestrator> logger)
        {
            this.jobProducerFactory = jobProducerFactory;
            this.logger = logger;
            this.jobConsumerFactory = jobConsumerFactory;
            this.outputBufferFactory = outputBufferFactory;
            this.jobContext = jobContext;
            chunkProcessorPool = new ChunkProcessor[100];
        }

        public void StartProcess(JobDescription description)
        {
            var stopWatch = Stopwatch.StartNew();
            logger.LogInformation(
                $"Ready to perform operation: {description.Operation} on file {description.InputFile.Name}");

            using (var countdown = new CountdownEvent(1))
            {
                using var cancellationTokenSource = new CancellationTokenSource();
                using var jobQueue = new BlockingCollection<JobBatchItem>(new ConcurrentQueue<JobBatchItem>(), 1000);
                using var processedJobQueue = new BlockingCollection<JobBatchItem>(new OrderedConcurrentDictionaryWrapper(), 100);

                var outputBuffer = outputBufferFactory.Create(processedJobQueue, chunkProcessorPool.Length);
                for (var i = 0; i < chunkProcessorPool.Length; i++)
                {
                    chunkProcessorPool[i] = jobConsumerFactory.Create(jobQueue, outputBuffer, countdown);
                    chunkProcessorPool[i].Start(cancellationTokenSource.Token);
                }
                var processedJobsConsumer = new ProcessedJobsConsumer(processedJobQueue, jobContext, countdown, cancellationTokenSource);
                processedJobsConsumer.Start();

                var jobProducer = jobProducerFactory.Create(description.InputFile, jobQueue, countdown);
                jobProducer.Start(cancellationTokenSource);

                countdown.Signal();
                countdown.Wait();
            }
            logger.LogInformation($"Completed processing of file. Submitted {jobContext.SubmittedId} chunks. Processed {jobContext.ProcessedId} chunks");
            jobContext.ElapsedTimeMilliseconds = stopWatch.ElapsedMilliseconds;
        }
    }
}