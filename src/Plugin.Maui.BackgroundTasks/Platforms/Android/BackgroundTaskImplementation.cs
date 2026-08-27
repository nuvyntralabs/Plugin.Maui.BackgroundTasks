using Android.App.Job;
using Android.Content;
using Android.OS;

namespace Plugin.Maui.BackgroundTasks;

partial class BackgroundTaskImplementation
{
	public bool IsSupported => true;

	public BackgroundTaskPlatformInfo Platform { get; } = new(
		isSupported: true,
		nativeScheduler: "JobScheduler",
		minimumPeriodicInterval: TimeSpan.FromMinutes(15));

	void ScheduleNative(ScheduleRecord record)
	{
		var context = Android.App.Application.Context;
		var scheduler = context.GetSystemService(Context.JobSchedulerService) as JobScheduler
			?? throw new BackgroundTaskException(
				BackgroundTaskError.ScheduleFailed,
				"Android JobScheduler is not available.");

		var extras = new PersistableBundle();
		extras.PutString(PluginJobService.TaskIdKey, record.TaskId);
		extras.PutString(PluginJobService.ParametersKey, ScheduleRecordSerializer.SerializeParameters(record.Parameters));

		var component = new ComponentName(context, Java.Lang.Class.FromType(typeof(PluginJobService)));
		var builder = new JobInfo.Builder(AndroidJobId.FromTaskId(record.TaskId), component);
		builder.SetPersisted(true);
		builder.SetExtras(extras);
		builder.SetRequiredNetworkType(record.RequiresNetwork ? NetworkType.Any : NetworkType.None);

		if (record.RequiresCharging)
			builder.SetRequiresCharging(true);

		if (record.RequiresBatteryNotLow && OperatingSystem.IsAndroidVersionAtLeast(26))
			builder.SetRequiresBatteryNotLow(true);

		if (record.IsPeriodic)
		{
			var interval = record.Interval ?? TimeSpan.FromMinutes(15);
			if (interval < TimeSpan.FromMinutes(15))
				interval = TimeSpan.FromMinutes(15);

			builder.SetPeriodic((long)interval.TotalMilliseconds);
		}
		else if (record.Delay is { } delay && delay > TimeSpan.Zero)
		{
			builder.SetMinimumLatency((long)delay.TotalMilliseconds);
		}

		var job = builder.Build()
			?? throw new BackgroundTaskException(
				BackgroundTaskError.ScheduleFailed,
				$"Could not build JobInfo for '{record.TaskId}'.");

		if (scheduler.Schedule(job) == JobScheduler.ResultFailure)
		{
			throw new BackgroundTaskException(
				BackgroundTaskError.ScheduleFailed,
				$"JobScheduler rejected '{record.TaskId}'.");
		}
	}

	void CancelNative(string taskId)
	{
		var context = Android.App.Application.Context;
		if (context.GetSystemService(Context.JobSchedulerService) is JobScheduler scheduler)
			scheduler.Cancel(AndroidJobId.FromTaskId(taskId));
	}

	void CancelAllNative()
	{
		// Unique job ids are cancelled individually in CancelAllAsync.
	}
}
