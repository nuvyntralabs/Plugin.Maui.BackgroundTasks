namespace Plugin.Maui.BackgroundTasks;

/// <summary>
/// A one-time background task request.
/// </summary>
public sealed class BackgroundTaskRequest
{
	/// <summary>
	/// Gets the identifier that maps to a registered <see cref="IBackgroundTask"/>.
	/// On iOS this must also appear in <c>BGTaskSchedulerPermittedIdentifiers</c>.
	/// </summary>
	public required string TaskId { get; init; }

	/// <summary>
	/// Gets an optional delay before the work becomes eligible to run.
	/// </summary>
	public TimeSpan? Delay { get; init; }

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
