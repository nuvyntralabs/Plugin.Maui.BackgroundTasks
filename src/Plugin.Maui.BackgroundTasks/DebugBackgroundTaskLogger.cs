using System.Diagnostics;

namespace Plugin.Maui.BackgroundTasks;

/// <summary>
/// Writes plugin diagnostics to <see cref="Debug.WriteLine(string?)"/>.
/// </summary>
public sealed class DebugBackgroundTaskLogger : IBackgroundTaskLogger
{
	public void Log(BackgroundTaskLogLevel level, string message, Exception? exception = null)
	{
		var line = exception is null
			? $"[BackgroundTasks] {level}: {message}"
			: $"[BackgroundTasks] {level}: {message}{Environment.NewLine}{exception}";

		Debug.WriteLine(line);
	}
}
