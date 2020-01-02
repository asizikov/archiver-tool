using Microsoft.Extensions.DependencyInjection;

namespace GZipTest.Workflow.DependencyInjection
{
    public static class WorkflowServiceCollectionExtensions
    {
        public static IServiceCollection AddWorkflowServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IJobBatchOrchestrator, JobBatchOrchestrator>();
            return serviceCollection;
        }
    }
}
