using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Grow.Logging;

public static class LoggingModule
{
    public static void RegisterLogging(this IServiceCollection services)
    {
        var logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        Log.Logger = logger;

        services.AddSerilog(logger);
    }
}
