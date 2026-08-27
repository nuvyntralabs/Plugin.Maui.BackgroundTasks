namespace Plugin.Maui.BackgroundTasks;

/// <summary>
/// Thrown when a background task cannot be scheduled or executed.
/// </summary>
public sealed class BackgroundTaskException : Exception
{
	public BackgroundTaskException(BackgroundTaskError error, string message, Exception? innerException = null)
		: base(message, innerException)
	{
		Error = error;
	}

	public BackgroundTaskError Error { get; }
}
