using Microsoft.Extensions.Logging;

namespace Grow.Infrastructure.Logging;

public static partial class LoggerExtensions
{
    [LoggerMessage(EventId = (int)LoggerEvent.ExecutingQueryStarted, EventName = nameof(LoggerEvent.ExecutingQueryStarted), Level = LogLevel.Information, Message = "Executing query started {queryName}", SkipEnabledCheck = false)]
    public static partial void ExecutingQueryStarted(this ILogger logger, string queryName);
    [LoggerMessage(EventId = (int)LoggerEvent.ExecutingQueryFinished, EventName = nameof(LoggerEvent.ExecutingQueryFinished), Level = LogLevel.Information, Message = "Executing query finished {queryName}", SkipEnabledCheck = false)]
    public static partial void ExecutingQueryFinished(this ILogger logger, string queryName);


    [LoggerMessage(EventId = (int)LoggerEvent.ExecutingCommandStarted, EventName = nameof(LoggerEvent.ExecutingCommandStarted), Level = LogLevel.Information, Message = "Executing command started {commandName}", SkipEnabledCheck = false)]
    public static partial void ExecutingCommandStarted(this ILogger logger, string commandName);

    [LoggerMessage(EventId = (int)LoggerEvent.ExecutingCommandFinished, EventName = nameof(LoggerEvent.ExecutingCommandFinished), Level = LogLevel.Information, Message = "Executing command finished {commandName}", SkipEnabledCheck = false)]
    public static partial void ExecutingCommandFinished(this ILogger logger, string commandName);

}
