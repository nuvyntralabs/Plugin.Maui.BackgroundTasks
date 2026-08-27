namespace Plugin.Maui.BackgroundTasks;

/// <summary>
/// Cross-platform scheduler for one-time and periodic background work.
/// </summary>
public interface IBackgroundTaskScheduler
{
	/// <summary>
	/// Gets a value indicating whether the current target can schedule native background work.
	/// </summary>
	bool IsSupported { get; }

	/// <summary>
	/// Gets platform capabilities such as the native scheduler name and minimum periodic interval.
	/// </summary>
	BackgroundTaskPlatformInfo Platform { get; }

	/// <summary>
	/// Gets a value indicating whether plugin logging is currently enabled.
	/// </summary>
	bool IsLoggingEnabled { get; }

	/// <summary>
	/// Raised when a registered handler starts executing.
	/// </summary>
	event EventHandler<BackgroundTaskEventArgs>? TaskStarted;

	/// <summary>
	/// Raised when a registered handler finishes without throwing.
	/// </summary>
	event EventHandler<BackgroundTaskCompletedEventArgs>? TaskCompleted;

	/// <summary>
	/// Raised when a registered handler throws or is cancelled by the operating system.
	/// </summary>
	event EventHandler<BackgroundTaskFailedEventArgs>? TaskFailed;

	/// <summary>
	/// Enables or disables plugin logging.
	/// </summary>
	void EnableLogging(bool enabled, IBackgroundTaskLogger? logger = null);

	/// <summary>
	/// Registers a handler type that is constructed from dependency injection when possible.
	/// </summary>
	void RegisterHandler<TTask>(string taskId) where TTask : class, IBackgroundTask;

	/// <summary>
	/// Registers a factory used to create the handler for <paramref name="taskId"/>.
	/// </summary>
	void RegisterHandler(string taskId, Func<IBackgroundTask> factory);

	/// <summary>
	/// Schedules a one-time background task. The operating system chooses the exact run time.
	/// </summary>
	Task ScheduleAsync(BackgroundTaskRequest request, CancellationToken cancellationToken = default);

	/// <summary>
	/// Schedules repeating background work. Android enforces a 15-minute minimum interval.
	/// </summary>
	Task SchedulePeriodicAsync(PeriodicBackgroundTaskRequest request, CancellationToken cancellationToken = default);

	/// <summary>
	/// Cancels a previously scheduled task.
	/// </summary>
	Task CancelAsync(string taskId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Cancels every task previously scheduled through this plugin.
	/// </summary>
	Task CancelAllAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns tasks that this plugin has persisted as scheduled.
	/// </summary>
	Task<IReadOnlyList<ScheduledBackgroundTask>> GetScheduledTasksAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Runs a registered handler immediately in-process. Useful for testing and sample apps.
	/// </summary>
	Task<BackgroundTaskResult> RunNowAsync(string taskId, IReadOnlyDictionary<string, string>? parameters = null, CancellationToken cancellationToken = default);
}
