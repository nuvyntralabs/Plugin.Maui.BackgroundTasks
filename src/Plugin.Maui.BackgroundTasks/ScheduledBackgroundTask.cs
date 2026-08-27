namespace Plugin.Maui.BackgroundTasks;

/// <summary>
/// A task that this plugin has persisted as scheduled.
/// </summary>
public sealed class ScheduledBackgroundTask
{
	public ScheduledBackgroundTask(
		string taskId,
		bool isPeriodic,
		TimeSpan? interval,
		TimeSpan? delay,
		BackgroundTaskKind kind,
		BackgroundTaskConstraints constraints,
		DateTimeOffset scheduledAt,
		BackgroundTaskState state)
	{
		TaskId = taskId;
		IsPeriodic = isPeriodic;
		Interval = interval;
		Delay = delay;
		Kind = kind;
		Constraints = constraints;
		ScheduledAt = scheduledAt;
		State = state;
	}

	public string TaskId { get; }

	public bool IsPeriodic { get; }

	public TimeSpan? Interval { get; }

	public TimeSpan? Delay { get; }

	public BackgroundTaskKind Kind { get; }

	public BackgroundTaskConstraints Constraints { get; }

	public DateTimeOffset ScheduledAt { get; }

	public BackgroundTaskState State { get; }
}
