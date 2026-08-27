namespace Plugin.Maui.BackgroundTasks;

/// <summary>
/// Event data for a background task that is starting.
/// </summary>
public sealed class BackgroundTaskEventArgs : EventArgs
{
	public BackgroundTaskEventArgs(string taskId)
	{
		TaskId = taskId;
	}

	public string TaskId { get; }
}
