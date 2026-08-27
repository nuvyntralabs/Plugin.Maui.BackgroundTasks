namespace Plugin.Maui.BackgroundTasks;

/// <summary>
/// Event data for a background task that failed or was cancelled.
/// </summary>
public sealed class BackgroundTaskFailedEventArgs : EventArgs
{
	public BackgroundTaskFailedEventArgs(string taskId, Exception exception)
	{
		TaskId = taskId;
		Exception = exception;
		Message = exception.Message;
	}

	public string TaskId { get; }

	public string Message { get; }

	public Exception Exception { get; }
}
