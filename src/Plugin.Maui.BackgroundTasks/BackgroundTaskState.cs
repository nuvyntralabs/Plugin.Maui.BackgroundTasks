namespace Plugin.Maui.BackgroundTasks;

/// <summary>
/// High-level state of a persisted schedule.
/// </summary>
public enum BackgroundTaskState
{
	/// <summary>
	/// The task is registered with the operating system or persisted by the plugin.
	/// </summary>
	Scheduled,

	/// <summary>
	/// The task is currently executing in-process.
	/// </summary>
	Running,

	/// <summary>
	/// The plugin does not have a live status from the operating system.
	/// </summary>
	Unknown
}
