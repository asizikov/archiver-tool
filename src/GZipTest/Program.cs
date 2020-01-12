using System.IO;
using GZipTest.Application;
using GZipTest.IO.DependencyInjection;
using GZipTest.Workflow;
using GZipTest.Workflow.DependencyInjection;
using GZipTest.Workflow.JobConfiguration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GZipTest
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var services = ConfigureServices();
            using var serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IApplicationFlow>().Run(args);
            return (int) ExecutionResult.Success;
        }

        private static IServiceCollection ConfigureServices()
        {
            var config = LoadConfiguration();
            var serviceCollection = new ServiceCollection()
                .AddWorkflowServices()
                .AddIOServices()
                .AddApplicationServices()
                .AddOptions()
                .AddLogging(logging =>
                {
                    logging.AddConfiguration(config.GetSection("Logging"));
                    logging.AddConsole();
                })
                .Configure<Batching>(config.GetSection("Batching"))
                .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information);


            serviceCollection.AddSingleton(config);
            serviceCollection.AddTransient<IApplicationFlow, ApplicationFlow>();
            return serviceCollection;
        }

        private static IConfiguration LoadConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                ;
            return configurationBuilder.Build();
        }
    }
}