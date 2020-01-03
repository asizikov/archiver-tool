using GZipTest.Workflow;
using Microsoft.Extensions.DependencyInjection;

namespace GZipTest.Application
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IJobConsumerFactory, JobConsumerFactory>();
            serviceCollection.AddTransient<IJobProducerFactory, JobProducerFactory>();

            return serviceCollection;
        }
    }
}
