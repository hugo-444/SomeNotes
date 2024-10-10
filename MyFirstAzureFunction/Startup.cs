using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyFirstAzureFunction.Implementations.Services;
using MyFirstAzureFunction.Interfaces;
using Serilog.Extensions.Logging;
using Serilog;

[assembly: FunctionsStartup(typeof(MyFirstAzureFunction.Startup))]
namespace MyFirstAzureFunction
{
    [ExcludeFromCodeCoverage]
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Configure the Serilog logger factory
            builder.Services.AddSingleton<ILoggerFactory>(sp =>
            {
                // Create a new Serilog logger factory
                var loggerFactory = new SerilogLoggerFactory();
                
                // Add any additional providers if necessary
                var providerCollection = sp.GetService<LoggerProviderCollection>();
                if (providerCollection != null)
                {
                    foreach (var provider in sp.GetServices<ILoggerProvider>())
                    {
                        loggerFactory.AddProvider(provider);
                    }
                }

                return loggerFactory;
            });

            // Register application services
            builder.Services.AddSingleton<INoteService, NoteService>();  // Un-commenting to register NoteService
            builder.Services.AddSingleton<IAuthorService, AuthorService>(); // Register AuthorService
            builder.Services.AddSingleton<ITagService, TagService>();  // Register TagService if necessary
        }
    }
}