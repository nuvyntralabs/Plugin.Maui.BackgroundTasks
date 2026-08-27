namespace Plugin.Maui.BackgroundTasks;

/// <summary>
/// Categorized failure reasons for scheduling or running background work.
/// </summary>
public enum BackgroundTaskError
{
	Unknown,
	InvalidRequest,
	HandlerNotRegistered,
	ScheduleFailed,
	Cancelled,
	FeatureNotSupported
}
