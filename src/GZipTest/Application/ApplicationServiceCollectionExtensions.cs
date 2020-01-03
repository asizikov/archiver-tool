using GZipTest.Workflow.Factories;
using GZipTest.CommandLineArguments;
using Microsoft.Extensions.DependencyInjection;

namespace GZipTest.Application
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IJobConsumerFactory, JobConsumerFactory>();
            serviceCollection.AddTransient<IJobProducerFactory, JobProducerFactory>();
            serviceCollection.AddTransient<ICommandLineValidator, CommandLineValidator>();
            serviceCollection.AddTransient<IArgumentsParser, ArgumentsParser>();

            return serviceCollection;
        }
    }
}
