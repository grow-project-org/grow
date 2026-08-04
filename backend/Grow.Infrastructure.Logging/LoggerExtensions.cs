using Microsoft.Extensions.Logging;

namespace Grow.Infrastructure.Logging;

public static partial class LoggerExtensions
{
    [LoggerMessage(EventId = (int)LoggerEvent.ExecutingQuery, EventName = nameof(LoggerEvent.ExecutingQuery), Level = LogLevel.Information, Message = "Executing query {queryName}", SkipEnabledCheck = false)]
    public static partial void ExecutingQuery(this ILogger logger, string queryName);

    [LoggerMessage(EventId = (int)LoggerEvent.ExecutingCommand, EventName = nameof(LoggerEvent.ExecutingCommand), Level = LogLevel.Information, Message = "Executing command {commandName}", SkipEnabledCheck = false)]
    public static partial void ExecutingCommand(this ILogger logger, string commandName);
}
