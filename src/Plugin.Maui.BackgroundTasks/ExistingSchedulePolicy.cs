namespace Plugin.Maui.BackgroundTasks;

/// <summary>
/// What to do when a task id is already scheduled.
/// </summary>
public enum ExistingSchedulePolicy
{
	/// <summary>
	/// Replace the existing schedule with the new request.
	/// </summary>
	Replace,

	/// <summary>
	/// Keep the existing schedule and ignore the new request.
	/// </summary>
	Keep
}
