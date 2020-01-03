using System.IO;
using GZipTest.Workflow;
using GZipTest.Workflow.JobConfiguration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GZipTest.Application
{
    public class ApplicationFlow : IApplicationFlow
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<ApplicationFlow> logger;
        private readonly IJobBatchOrchestrator jobBatchOrchestrator;

        public ApplicationFlow(IConfiguration configuration, ILogger<ApplicationFlow> logger, IJobBatchOrchestrator jobBatchOrchestrator)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.jobBatchOrchestrator = jobBatchOrchestrator;
        }
        public void Run(string[] args)
        {
            logger.LogInformation($"Application started with {string.Join(",", args)}");
            jobBatchOrchestrator.StartProcess(new JobDescription
            {
                Operation = Operation.Compress,
                InputFile = new FileInfo(@"C:\file.blah")
            });
            // Parse command args
            // Print help if needed
            // Start process
            // Report process
            // report result
        }
    }
}