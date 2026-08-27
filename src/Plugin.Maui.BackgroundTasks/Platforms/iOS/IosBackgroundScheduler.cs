using BackgroundTasks;
using Foundation;

namespace Plugin.Maui.BackgroundTasks;

static class IosBackgroundScheduler
{
	public static void Submit(ScheduleRecord record)
	{
		IosBackgroundTaskRegistrar.Register(record.TaskId);

		var delay = record.IsPeriodic
			? record.Interval ?? TimeSpan.FromMinutes(15)
			: record.Delay ?? TimeSpan.Zero;

		var seconds = Math.Max(1, delay.TotalSeconds);
		NSError? error = null;

		if (record.Kind == BackgroundTaskKind.Processing)
		{
			var request = new BGProcessingTaskRequest(record.TaskId)
			{
				EarliestBeginDate = NSDate.FromTimeIntervalSinceNow(seconds),
				RequiresNetworkConnectivity = record.RequiresNetwork,
				RequiresExternalPower = record.RequiresCharging
			};

			BGTaskScheduler.Shared.Submit(request, out error);
		}
		else
		{
			var request = new BGAppRefreshTaskRequest(record.TaskId)
			{
				EarliestBeginDate = NSDate.FromTimeIntervalSinceNow(seconds)
			};

			BGTaskScheduler.Shared.Submit(request, out error);
		}

		if (error is not null)
		{
			throw new BackgroundTaskException(
				BackgroundTaskError.ScheduleFailed,
				$"BGTaskScheduler rejected '{record.TaskId}': {error.LocalizedDescription}. " +
				"Confirm the identifier is listed in Info.plist under BGTaskSchedulerPermittedIdentifiers " +
				"and that UIBackgroundModes includes fetch and/or processing.");
		}
	}

	public static void Cancel(string taskId) => BGTaskScheduler.Shared.Cancel(taskId);
}
