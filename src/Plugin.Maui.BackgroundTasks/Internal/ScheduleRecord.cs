namespace Plugin.Maui.BackgroundTasks;

sealed class ScheduleRecord
{
	public string TaskId { get; set; } = string.Empty;

	public bool IsPeriodic { get; set; }

	public long? IntervalTicks { get; set; }

	public long? DelayTicks { get; set; }

	public BackgroundTaskKind Kind { get; set; }

	public bool RequiresNetwork { get; set; }

	public bool RequiresCharging { get; set; }

	public bool RequiresBatteryNotLow { get; set; }

	public Dictionary<string, string>? Parameters { get; set; }

	public DateTimeOffset ScheduledAt { get; set; }

	public ExistingSchedulePolicy Policy { get; set; }

	public TimeSpan? Interval => IntervalTicks is { } ticks ? TimeSpan.FromTicks(ticks) : null;

	public TimeSpan? Delay => DelayTicks is { } ticks ? TimeSpan.FromTicks(ticks) : null;

	public static ScheduleRecord From(BackgroundTaskRequest request) => new()
	{
		TaskId = request.TaskId,
		IsPeriodic = false,
		DelayTicks = request.Delay?.Ticks,
		Kind = request.Kind,
		RequiresNetwork = request.Constraints.RequiresNetwork,
		RequiresCharging = request.Constraints.RequiresCharging,
		RequiresBatteryNotLow = request.Constraints.RequiresBatteryNotLow,
		Parameters = request.Parameters is null ? null : new Dictionary<string, string>(request.Parameters),
		ScheduledAt = DateTimeOffset.UtcNow,
		Policy = request.ExistingPolicy
	};

	public static ScheduleRecord From(PeriodicBackgroundTaskRequest request) => new()
	{
		TaskId = request.TaskId,
		IsPeriodic = true,
		IntervalTicks = request.Interval.Ticks,
		Kind = request.Kind,
		RequiresNetwork = request.Constraints.RequiresNetwork,
		RequiresCharging = request.Constraints.RequiresCharging,
		RequiresBatteryNotLow = request.Constraints.RequiresBatteryNotLow,
		Parameters = request.Parameters is null ? null : new Dictionary<string, string>(request.Parameters),
		ScheduledAt = DateTimeOffset.UtcNow,
		Policy = request.ExistingPolicy
	};

	public ScheduledBackgroundTask ToScheduled(BackgroundTaskState state) =>
		new(
			TaskId,
			IsPeriodic,
			Interval,
			Delay,
			Kind,
			new BackgroundTaskConstraints
			{
				RequiresNetwork = RequiresNetwork,
				RequiresCharging = RequiresCharging,
				RequiresBatteryNotLow = RequiresBatteryNotLow
			},
			ScheduledAt,
			state);
}
