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
        private readonly IJobContext jobContext;
        private readonly ChunkProcessor[] chunkProcessorPool;

        public JobBatchOrchestrator(IJobProducerFactory jobProducerFactory,
            IJobConsumerFactory jobConsumerFactory,
            IJobContext jobContext,
            ILogger<JobBatchOrchestrator> logger)
        {
            this.jobProducerFactory = jobProducerFactory;
            this.logger = logger;
            this.jobConsumerFactory = jobConsumerFactory;
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
                using var jobQueue = new BlockingCollection<JobBatchItem>(new ConcurrentQueue<JobBatchItem>(),1000);

                for (var i = 0; i < chunkProcessorPool.Length; i++)
                {
                    chunkProcessorPool[i] = jobConsumerFactory.Create(jobQueue, countdown); 
                    chunkProcessorPool[i].Start(cancellationTokenSource.Token);
                }
                var jobProducer = jobProducerFactory.Create(description.InputFile, jobQueue, countdown);
                jobProducer.Start(cancellationTokenSource);

                countdown.Signal();
                countdown.Wait();
            }

            if (jobContext.Result == ExecutionResult.Failure)
            {
                logger.LogInformation($"Failed to process file due to an error: {jobContext.Error}");
            }
            else
            {
                logger.LogInformation($"Completed file in {stopWatch.ElapsedMilliseconds} ms");
            }
        }
    }
}