namespace Plugin.Maui.BackgroundTasks;

/// <summary>
/// Outcome returned by <see cref="IBackgroundTask.RunAsync"/>.
/// </summary>
public enum BackgroundTaskResult
{
	/// <summary>
	/// The work completed and should not be retried.
	/// </summary>
	Success,

	/// <summary>
	/// The work did not finish and the platform should try again later when supported.
	/// </summary>
	Retry,

	/// <summary>
	/// The work failed and should not be retried automatically.
	/// </summary>
	Failure
}
