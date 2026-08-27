using Microsoft.Maui.ApplicationModel;

namespace Plugin.Maui.BackgroundTasks;

partial class BackgroundTaskImplementation : IBackgroundTaskScheduler
{
	readonly HashSet<string> _running = [];
	IBackgroundTaskLogger? _logger;

	public bool IsLoggingEnabled { get; private set; }

	public event EventHandler<BackgroundTaskEventArgs>? TaskStarted;

	public event EventHandler<BackgroundTaskCompletedEventArgs>? TaskCompleted;

	public event EventHandler<BackgroundTaskFailedEventArgs>? TaskFailed;

	public void EnableLogging(bool enabled, IBackgroundTaskLogger? logger = null)
	{
		IsLoggingEnabled = enabled;
		_logger = enabled ? logger ?? new DebugBackgroundTaskLogger() : null;
		Log(BackgroundTaskLogLevel.Information, enabled ? "Logging enabled." : "Logging disabled.");
	}

	public void RegisterHandler<TTask>(string taskId) where TTask : class, IBackgroundTask
	{
		BackgroundTaskRegistry.Register(taskId, typeof(TTask));
		Log(BackgroundTaskLogLevel.Debug, $"Registered handler {typeof(TTask).Name} for '{taskId}'.");
#if IOS
		IosBackgroundTaskRegistrar.Register(taskId);
#endif
	}

	public void RegisterHandler(string taskId, Func<IBackgroundTask> factory)
	{
		ArgumentNullException.ThrowIfNull(factory);
		BackgroundTaskRegistry.Register(taskId, _ => factory());
		Log(BackgroundTaskLogLevel.Debug, $"Registered factory handler for '{taskId}'.");
#if IOS
		IosBackgroundTaskRegistrar.Register(taskId);
#endif
	}

	public Task ScheduleAsync(BackgroundTaskRequest request, CancellationToken cancellationToken = default)
	{
		RequestValidator.Validate(request);
		EnsureSupported();
		cancellationToken.ThrowIfCancellationRequested();

		var record = ScheduleRecord.From(request);
		if (record.Policy == ExistingSchedulePolicy.Keep && ScheduleStore.Get(record.TaskId) is not null)
		{
			Log(BackgroundTaskLogLevel.Information, $"Keeping existing schedule for '{record.TaskId}'.");
			return Task.CompletedTask;
		}

		ScheduleNative(record);
		ScheduleStore.Upsert(record);
		Log(BackgroundTaskLogLevel.Information, $"Scheduled one-time task '{record.TaskId}' ({record.Kind}).");
		return Task.CompletedTask;
	}

	public Task SchedulePeriodicAsync(PeriodicBackgroundTaskRequest request, CancellationToken cancellationToken = default)
	{
		RequestValidator.Validate(request);
		EnsureSupported();
		cancellationToken.ThrowIfCancellationRequested();

		var record = ScheduleRecord.From(request);
		if (record.Policy == ExistingSchedulePolicy.Keep && ScheduleStore.Get(record.TaskId) is not null)
		{
			Log(BackgroundTaskLogLevel.Information, $"Keeping existing periodic schedule for '{record.TaskId}'.");
			return Task.CompletedTask;
		}

		if (record.Interval is { } interval && interval < Platform.MinimumPeriodicInterval)
		{
			Log(
				BackgroundTaskLogLevel.Warning,
				$"Interval {interval} is below the platform minimum of {Platform.MinimumPeriodicInterval}. The OS may clamp or delay the work.");
		}

		ScheduleNative(record);
		ScheduleStore.Upsert(record);
		Log(BackgroundTaskLogLevel.Information, $"Scheduled periodic task '{record.TaskId}' every {record.Interval}.");
		return Task.CompletedTask;
	}

	public Task CancelAsync(string taskId, CancellationToken cancellationToken = default)
	{
		RequestValidator.ValidateTaskId(taskId);
		EnsureSupported();
		cancellationToken.ThrowIfCancellationRequested();

		CancelNative(taskId);
		ScheduleStore.Remove(taskId);
		Log(BackgroundTaskLogLevel.Information, $"Cancelled task '{taskId}'.");
		return Task.CompletedTask;
	}

	public Task CancelAllAsync(CancellationToken cancellationToken = default)
	{
		EnsureSupported();
		cancellationToken.ThrowIfCancellationRequested();

		foreach (var record in ScheduleStore.GetAll())
			CancelNative(record.TaskId);

		CancelAllNative();
		ScheduleStore.Clear();
		Log(BackgroundTaskLogLevel.Information, "Cancelled all scheduled tasks.");
		return Task.CompletedTask;
	}

	public Task<IReadOnlyList<ScheduledBackgroundTask>> GetScheduledTasksAsync(CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();

		IReadOnlyList<ScheduledBackgroundTask> tasks = ScheduleStore.GetAll()
			.Select(record =>
			{
				var state = IsRunning(record.TaskId) ? BackgroundTaskState.Running : BackgroundTaskState.Scheduled;
				return record.ToScheduled(state);
			})
			.ToArray();

		return Task.FromResult(tasks);
	}

	public Task<BackgroundTaskResult> RunNowAsync(
		string taskId,
		IReadOnlyDictionary<string, string>? parameters = null,
		CancellationToken cancellationToken = default)
	{
		RequestValidator.ValidateTaskId(taskId);
		var json = ScheduleRecordSerializer.SerializeParameters(parameters);
		return BackgroundTaskExecutor.ExecuteAsync(taskId, json, cancellationToken);
	}

	internal void NotifyStarted(string taskId)
	{
		lock (_running)
			_running.Add(taskId);

		Log(BackgroundTaskLogLevel.Information, $"Task '{taskId}' started.");
		var args = new BackgroundTaskEventArgs(taskId);
		Dispatch(() => TaskStarted?.Invoke(this, args));
	}

	internal void NotifyCompleted(string taskId, BackgroundTaskResult result)
	{
		lock (_running)
			_running.Remove(taskId);

		Log(BackgroundTaskLogLevel.Information, $"Task '{taskId}' completed with {result}.");
		var args = new BackgroundTaskCompletedEventArgs(taskId, result);
		Dispatch(() => TaskCompleted?.Invoke(this, args));
	}

	internal void NotifyFailed(string taskId, Exception exception)
	{
		lock (_running)
			_running.Remove(taskId);

		Log(BackgroundTaskLogLevel.Error, $"Task '{taskId}' failed: {exception.Message}", exception);
		var args = new BackgroundTaskFailedEventArgs(taskId, exception);
		Dispatch(() => TaskFailed?.Invoke(this, args));
	}

	bool IsRunning(string taskId)
	{
		lock (_running)
			return _running.Contains(taskId);
	}

	void Log(BackgroundTaskLogLevel level, string message, Exception? exception = null)
	{
		if (!IsLoggingEnabled)
			return;

		try
		{
			_logger?.Log(level, message, exception);
		}
		catch
		{
			// Logging must never break scheduling.
		}
	}

	static void Dispatch(Action action)
	{
		try
		{
			if (MainThread.IsMainThread)
				action();
			else
				MainThread.BeginInvokeOnMainThread(action);
		}
		catch
		{
			action();
		}
	}

	void EnsureSupported()
	{
		if (!IsSupported)
		{
			throw new BackgroundTaskException(
				BackgroundTaskError.FeatureNotSupported,
				"Background task scheduling is only supported on Android and iOS.");
		}
	}
}
