using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using GZipTest.Application;
using GZipTest.IO.DependencyInjection;
using GZipTest.Workflow.Context;
using GZipTest.Workflow.DependencyInjection;
using GZipTest.Workflow.IntegrationTests.Utils;
using GZipTest.Workflow.JobConfiguration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using Xunit;

namespace GZipTest.Workflow.IntegrationTests
{
    public sealed class WorkflowIntegrationTests : IntegrationTestBase
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IJobBatchOrchestrator jobBatchOrchestrator;

        public WorkflowIntegrationTests()
        {
            var builder = new ConfigurationBuilder();
            builder.AddInMemoryCollection(new Dictionary<string, string>
            {
                {$"{nameof(Batching)}:{nameof(Batching.ParallelWorkers)}", "100"},
                {$"{nameof(Batching)}:{nameof(Batching.OutputQueueMultiplier)}", "10"},
                {$"{nameof(Batching)}:{nameof(Batching.InputQueueMultiplier)}", "2"},
            });

            var config = builder.Build();
            var serviceCollection = new ServiceCollection()
                .AddWorkflowServices()
                .AddIOServices()
                .AddApplicationServices()
                .AddOptions()
                .AddLogging(logging => logging.AddProvider(NullLoggerProvider.Instance));

            serviceCollection.Configure<Batching>(config.GetSection("Batching"));
            serviceProvider = serviceCollection.BuildServiceProvider();
            jobBatchOrchestrator = serviceProvider.GetService<IJobBatchOrchestrator>();
        }


        [Fact]
        public void CompressedAndDecompressedFilesAreEqual()
        {
            jobBatchOrchestrator.ShouldNotBeNull();
            var thread = new Thread(() => jobBatchOrchestrator.StartProcess(new JobDescription
            {
                InputFile = inputFile,
                OutputFile = outputFile,
                Operation = Operation.Compress
            }));
            thread.Start();
            thread.Join();

            var jobContext = serviceProvider.GetService<IJobContext>();
            jobContext.Result.ShouldBe(ExecutionResult.Success);

            thread = new Thread(() => jobBatchOrchestrator.StartProcess(new JobDescription
            {
                InputFile = outputFile,
                OutputFile = decompressedFile,
                Operation = Operation.Decompress
            }));

            thread.Start();
            thread.Join();

            jobContext.Result.ShouldBe(ExecutionResult.Success);
            outputFile.Refresh();
            decompressedFile.Refresh();

            outputFile.Length.ShouldNotBeNull();
            inputFile.Length.ShouldBe(decompressedFile.Length);

            inputFile.ShouldHaveSameContentAs(decompressedFile);
        }
    }
}