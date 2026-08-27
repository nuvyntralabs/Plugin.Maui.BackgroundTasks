using Microsoft.Extensions.Logging;

namespace Plugin.Maui.BackgroundTasks;

sealed class MicrosoftLoggerAdapter(ILogger logger) : IBackgroundTaskLogger
{
	public void Log(BackgroundTaskLogLevel level, string message, Exception? exception = null)
	{
		logger.Log(ToLogLevel(level), exception, "{Message}", message);
	}

	static LogLevel ToLogLevel(BackgroundTaskLogLevel level) => level switch
	{
		BackgroundTaskLogLevel.Trace => LogLevel.Trace,
		BackgroundTaskLogLevel.Debug => LogLevel.Debug,
		BackgroundTaskLogLevel.Information => LogLevel.Information,
		BackgroundTaskLogLevel.Warning => LogLevel.Warning,
		BackgroundTaskLogLevel.Error => LogLevel.Error,
		_ => LogLevel.Information
	};
}
