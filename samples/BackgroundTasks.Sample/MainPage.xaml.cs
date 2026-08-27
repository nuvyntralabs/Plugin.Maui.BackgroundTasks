using Microsoft.Maui.Storage;
using Plugin.Maui.BackgroundTasks;

namespace BackgroundTasks.Sample;

public partial class MainPage : ContentPage, IBackgroundTaskLogger
{
	readonly IBackgroundTaskScheduler _scheduler;
	readonly List<string> _logLines = [];

	public MainPage()
	{
		InitializeComponent();
		_scheduler = Plugin.Maui.BackgroundTasks.BackgroundTasks.Current;
		_scheduler.TaskStarted += OnTaskStarted;
		_scheduler.TaskCompleted += OnTaskCompleted;
		_scheduler.TaskFailed += OnTaskFailed;
		_scheduler.EnableLogging(true, this);

		PlatformLabel.Text = _scheduler.IsSupported
			? $"Platform: {_scheduler.Platform.NativeScheduler} (min periodic {_scheduler.Platform.MinimumPeriodicInterval.TotalMinutes:0} min)"
			: "Platform: not supported";

		_ = RefreshScheduledAsync();
		UpdateLastRun();
	}

	async void OnScheduleOneTimeClicked(object? sender, EventArgs e)
	{
		await RunAsync("Scheduling one-time refresh...", async () =>
		{
			await _scheduler.ScheduleAsync(new BackgroundTaskRequest
			{
				TaskId = MauiProgram.RefreshTaskId,
				Delay = TimeSpan.FromMinutes(1),
				Kind = BackgroundTaskKind.Refresh,
				Constraints = new BackgroundTaskConstraints { RequiresNetwork = true }
			});
		});
	}

	async void OnScheduleProcessingClicked(object? sender, EventArgs e)
	{
		await RunAsync("Scheduling one-time processing...", async () =>
		{
			await _scheduler.ScheduleAsync(new BackgroundTaskRequest
			{
				TaskId = MauiProgram.ProcessingTaskId,
				Delay = TimeSpan.FromMinutes(2),
				Kind = BackgroundTaskKind.Processing,
				Constraints = new BackgroundTaskConstraints
				{
					RequiresNetwork = false,
					RequiresCharging = false
				}
			});
		});
	}

	async void OnSchedulePeriodicClicked(object? sender, EventArgs e)
	{
		await RunAsync("Scheduling periodic refresh...", async () =>
		{
			await _scheduler.SchedulePeriodicAsync(new PeriodicBackgroundTaskRequest
			{
				TaskId = MauiProgram.RefreshTaskId,
				Interval = TimeSpan.FromMinutes(15),
				Kind = BackgroundTaskKind.Refresh,
				Constraints = new BackgroundTaskConstraints { RequiresNetwork = true }
			});
		});
	}

	async void OnRunRefreshNowClicked(object? sender, EventArgs e) =>
		await RunAsync("Running refresh now...", () => _scheduler.RunNowAsync(MauiProgram.RefreshTaskId));

	async void OnRunProcessingNowClicked(object? sender, EventArgs e) =>
		await RunAsync("Running processing now...", () => _scheduler.RunNowAsync(MauiProgram.ProcessingTaskId));

	async void OnRefreshListClicked(object? sender, EventArgs e) =>
		await RefreshScheduledAsync();

	async void OnCancelAllClicked(object? sender, EventArgs e)
	{
		await RunAsync("Cancelling scheduled tasks...", () => _scheduler.CancelAllAsync());
	}

	void OnLoggingToggled(object? sender, ToggledEventArgs e)
	{
		_scheduler.EnableLogging(e.Value, this);
		AppendLog(e.Value ? "Logging enabled by user." : "Logging disabled by user.");
	}

	void OnTaskStarted(object? sender, BackgroundTaskEventArgs e) =>
		AppendLog($"STARTED {e.TaskId}");

	void OnTaskCompleted(object? sender, BackgroundTaskCompletedEventArgs e)
	{
		AppendLog($"COMPLETED {e.TaskId} ({e.Result})");
		MainThread.BeginInvokeOnMainThread(UpdateLastRun);
	}

	void OnTaskFailed(object? sender, BackgroundTaskFailedEventArgs e) =>
		AppendLog($"FAILED {e.TaskId}: {e.Message}");

	async Task RefreshScheduledAsync()
	{
		try
		{
			var tasks = await _scheduler.GetScheduledTasksAsync();
			ScheduledLabel.Text = tasks.Count == 0
				? "None."
				: string.Join(Environment.NewLine, tasks.Select(Describe));
		}
		catch (Exception ex)
		{
			ScheduledLabel.Text = ex.Message;
			AppendLog(ex.ToString());
		}
	}

	async Task RunAsync(string status, Func<Task> operation)
	{
		try
		{
			StatusLabel.Text = status;
			await operation();
			if (StatusLabel.Text == status)
				StatusLabel.Text = "Done.";
			await RefreshScheduledAsync();
			UpdateLastRun();
		}
		catch (Exception ex)
		{
			StatusLabel.Text = ex.Message;
			AppendLog(ex.ToString());
		}
	}

	void UpdateLastRun()
	{
		var refresh = Preferences.Default.Get(SampleRefreshTask.LastRunKey, string.Empty);
		var process = Preferences.Default.Get(SampleProcessingTask.LastRunKey, string.Empty);
		var lastTask = Preferences.Default.Get(SampleProcessingTask.LastTaskKey, string.Empty);

		LastRunLabel.Text =
			$"Last refresh: {(string.IsNullOrEmpty(refresh) ? "none" : refresh)}{Environment.NewLine}" +
			$"Last processing: {(string.IsNullOrEmpty(process) ? "none" : process)}{Environment.NewLine}" +
			$"Last task id: {(string.IsNullOrEmpty(lastTask) ? "none" : lastTask)}";
	}

	static string Describe(ScheduledBackgroundTask task)
	{
		var cadence = task.IsPeriodic
			? $"every {task.Interval}"
			: task.Delay is { } delay ? $"in {delay}" : "as soon as allowed";

		return $"{task.TaskId} — {task.Kind}, {cadence}, {task.State}";
	}

	public void Log(BackgroundTaskLogLevel level, string message, Exception? exception = null)
	{
		var line = exception is null
			? $"{DateTime.Now:HH:mm:ss} {level}: {message}"
			: $"{DateTime.Now:HH:mm:ss} {level}: {message} ({exception.GetType().Name})";

		MainThread.BeginInvokeOnMainThread(() => AppendLog(line));
	}

	void AppendLog(string line)
	{
		_logLines.Insert(0, line);
		if (_logLines.Count > 40)
			_logLines.RemoveAt(_logLines.Count - 1);

		LogLabel.Text = string.Join(Environment.NewLine, _logLines);
	}
}
