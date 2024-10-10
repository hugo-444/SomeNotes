using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MyFirstAzureFunction
{
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        public static void Main()
        {
            try
            {
                var host = new HostBuilder()
                    .ConfigureFunctionsWorkerDefaults()
                    .ConfigureServices(services => { ConfigureServices(ref services); })
                    .Build();
                Task.Delay(5000).Wait();
                host.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Startup failed: {ex.Message}");
                throw;
            }
        }

        static void ConfigureServices(ref IServiceCollection services)
        {
            try
            {
                var startUp = new Startup();
                var projectServices = startUp.GetType()
                    .Assembly
                    .GetTypes()
                    .Where(t => t.Namespace != null && t.Namespace.StartsWith("MyFirstAzureFunction.Implementations"));

                foreach (var serviceType in projectServices)
                {
                    var interfaceType = serviceType
                        .GetInterfaces()
                        .FirstOrDefault(t => t.Namespace != null && t.Namespace.StartsWith("MyFirstAzureFunction.Interfaces"));

                    if (interfaceType == null)
                    {
                        continue;
                    }
                    services.AddScoped(interfaceType, serviceType);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ConfigureServices: {ex.Message}");
                throw;
            }
        }
    }
}