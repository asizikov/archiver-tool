using Microsoft.Extensions.DependencyInjection;

namespace GZipTest.IO.DependencyInjection
{
    public static class IoServiceCollectionExtensions
    {
        public static IServiceCollection AddIOServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IFileReader, BufferedFileReader>();
            serviceCollection.AddTransient<IFileWriter, FileWriter>();
            return serviceCollection;
        }
    }
}
