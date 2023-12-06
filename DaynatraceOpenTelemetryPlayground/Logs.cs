// credit: https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/examples/AspNetCore/Instrumentation.cs

namespace Bnaya.Examples;

public static partial class Logs
{
    [LoggerMessage(Level = LogLevel.Information,
        Message = "Anonymous player is rolling the dice: {result}")]
    public static partial void LogAnonymous(this ILogger logger, int result);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "{player} is rolling the dice: {result}")]
    public static partial void LogPlayer(this ILogger logger, string player, int result);
    
    [LoggerMessage(Level = LogLevel.Information,
        Message = "Internal span: {result}")]
    public static partial void LogInternal(this ILogger logger, int result);
}
