using Microsoft.Maui.ApplicationModel;

namespace Plugin.Maui.BackgroundTasks;

partial class BackgroundTaskImplementation
{
	public bool IsSupported => true;

	public BackgroundTaskPlatformInfo Platform { get; } = new(
		isSupported: true,
		nativeScheduler: "BGTaskScheduler",
		minimumPeriodicInterval: TimeSpan.FromMinutes(15));

	void ScheduleNative(ScheduleRecord record) =>
		RunOnMainThread(() => IosBackgroundScheduler.Submit(record));

	void CancelNative(string taskId) =>
		RunOnMainThread(() => IosBackgroundScheduler.Cancel(taskId));

	void CancelAllNative()
	{
		// Individual identifiers are cancelled in CancelAllAsync.
	}

	static void RunOnMainThread(Action action)
	{
		if (MainThread.IsMainThread)
		{
			action();
			return;
		}

		MainThread.InvokeOnMainThreadAsync(action).GetAwaiter().GetResult();
	}

	internal void ResubmitPersistedSchedules()
	{
		foreach (var record in ScheduleStore.GetAll())
		{
			try
			{
				IosBackgroundScheduler.Submit(record);
			}
			catch (Exception ex)
			{
				Log(BackgroundTaskLogLevel.Warning, $"Could not resubmit '{record.TaskId}' when entering the background.", ex);
			}
		}
	}
}
