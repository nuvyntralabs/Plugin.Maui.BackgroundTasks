namespace Plugin.Maui.BackgroundTasks;

/// <summary>
/// Receives diagnostic messages from the BackgroundTasks plugin.
/// </summary>
public interface IBackgroundTaskLogger
{
	void Log(BackgroundTaskLogLevel level, string message, Exception? exception = null);
}
