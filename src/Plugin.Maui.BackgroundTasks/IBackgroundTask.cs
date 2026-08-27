namespace Plugin.Maui.BackgroundTasks;

/// <summary>
/// Work that the plugin can run when the operating system launches a scheduled background task.
/// </summary>
public interface IBackgroundTask
{
	/// <summary>
	/// Executes the background work. Keep iOS refresh work short; the OS may expire the task.
	/// </summary>
	Task<BackgroundTaskResult> RunAsync(BackgroundTaskContext context, CancellationToken cancellationToken);
}
