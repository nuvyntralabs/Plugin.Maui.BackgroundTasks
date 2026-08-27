namespace Plugin.Maui.BackgroundTasks;

/// <summary>
/// Selects the native background-task class used on iOS and the intended duration of the work.
/// </summary>
public enum BackgroundTaskKind
{
	/// <summary>
	/// Short, deferrable refresh work. Maps to <c>BGAppRefreshTask</c> on iOS.
	/// </summary>
	Refresh,

	/// <summary>
	/// Longer processing that may require power or network. Maps to <c>BGProcessingTask</c> on iOS.
	/// </summary>
	Processing
}
