namespace Plugin.Maui.BackgroundTasks;

/// <summary>
/// A repeating background task request.
/// </summary>
public sealed class PeriodicBackgroundTaskRequest
{
	/// <summary>
	/// Gets the identifier that maps to a registered <see cref="IBackgroundTask"/>.
	/// On iOS this must also appear in <c>BGTaskSchedulerPermittedIdentifiers</c>.
	/// </summary>
	public required string TaskId { get; init; }

	/// <summary>
	/// Gets the requested repeat interval. Android clamps values below 15 minutes.
	/// iOS treats this as the earliest begin date for the next launch; the OS decides when it actually runs.
	/// </summary>
	public required TimeSpan Interval { get; init; }

	/// <summary>
	/// Gets the intended duration class of the work.
	/// </summary>
	public BackgroundTaskKind Kind { get; init; } = BackgroundTaskKind.Refresh;

	/// <summary>
	/// Gets runtime constraints such as network or charging.
	/// </summary>
	public BackgroundTaskConstraints Constraints { get; init; } = BackgroundTaskConstraints.None;

	/// <summary>
	/// Gets optional string parameters delivered to <see cref="BackgroundTaskContext.Parameters"/>.
	/// </summary>
	public IReadOnlyDictionary<string, string>? Parameters { get; init; }

	/// <summary>
	/// Gets the policy used when this task id is already scheduled.
	/// </summary>
	public ExistingSchedulePolicy ExistingPolicy { get; init; } = ExistingSchedulePolicy.Replace;
}
