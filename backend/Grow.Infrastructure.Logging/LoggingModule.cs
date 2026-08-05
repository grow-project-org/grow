using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Grow.Infrastructure.Logging;

public static class LoggingModule
{
    public static IServiceCollection RegisterLogging(this IServiceCollection services)
    {
        var logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        Log.Logger = logger;

        return services.AddSerilog(logger);
    }
}
