namespace Plugin.Maui.BackgroundTasks;

/// <summary>
/// Event data for a background task that finished without throwing.
/// </summary>
public sealed class BackgroundTaskCompletedEventArgs : EventArgs
{
	public BackgroundTaskCompletedEventArgs(string taskId, BackgroundTaskResult result)
	{
		TaskId = taskId;
		Result = result;
	}

	public string TaskId { get; }

	public BackgroundTaskResult Result { get; }
}
