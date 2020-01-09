using System;
using System.IO;
using System.Linq;
using System.Threading;
using GZipTest.Application;
using GZipTest.IO.DependencyInjection;
using GZipTest.Workflow.Context;
using GZipTest.Workflow.DependencyInjection;
using GZipTest.Workflow.JobConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace GZipTest.Workflow.IntegrationTests
{
    public sealed class WorkflowIntegrationTests : IntegrationTestBase
    {
        private readonly ITestOutputHelper outputHelper;
        private readonly IServiceProvider serviceProvider;
        private readonly IJobBatchOrchestrator jobBatchOrchestrator;

        public WorkflowIntegrationTests(ITestOutputHelper output)
        {
            this.outputHelper = output;
            var serviceCollection = new ServiceCollection()
                .AddWorkflowServices()
                .AddIOServices()
                .AddApplicationServices()
                .AddLogging(logging => logging.AddProvider(NullLoggerProvider.Instance));

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

            File.ReadAllBytes(inputFile.FullName)
                .SequenceEqual(File.ReadAllBytes(decompressedFile.FullName))
                .ShouldBeTrue();
        }
    }
}