using System;
using GZipTest.CommandLineArguments;
using GZipTest.Workflow;
using GZipTest.Workflow.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GZipTest.Application
{
    public class ApplicationFlow : IApplicationFlow
    {
        private readonly ILogger<ApplicationFlow> logger;
        private readonly IConfiguration configuration;
        private readonly IJobBatchOrchestrator jobBatchOrchestrator;
        private readonly ICommandLineValidator commandLineValidator;
        private readonly IArgumentsParser argumentsParser;
        private readonly IJobContext jobContext;

        public ApplicationFlow(IConfiguration configuration,
            ILogger<ApplicationFlow> logger,
            IJobBatchOrchestrator jobBatchOrchestrator,
            ICommandLineValidator commandLineValidator,
            IArgumentsParser argumentsParser,
            IJobContext jobContext)
        {
            this.commandLineValidator = commandLineValidator;
            this.argumentsParser = argumentsParser;
            this.jobContext = jobContext;
            this.configuration = configuration;
            this.logger = logger;
            this.jobBatchOrchestrator = jobBatchOrchestrator;
        }

        public void Run(string[] args)
        {
            logger.LogInformation($"Application started with {string.Join(",", args)}");
            var validationResult = commandLineValidator.Validate(args);
            if (!validationResult.IsValid)
            {
                logger.LogInformation($"invalid command line arguments {string.Join(Environment.NewLine, validationResult.Errors)}");
                PrintHelp();
                return;
            }

            var jobDescription = argumentsParser.Parse(args);
            jobBatchOrchestrator.StartProcess(jobDescription);
            logger.LogInformation(jobContext.Result == ExecutionResult.Failure
                ? $"Failed to process file due to an error: {jobContext.Error} reported by {jobContext.ReportedBy}"
                : $"Completed file in {jobContext.ElapsedTimeMilliseconds} ms");
            void PrintHelp() => logger.LogInformation(Constants.Help);
        }
    }
}