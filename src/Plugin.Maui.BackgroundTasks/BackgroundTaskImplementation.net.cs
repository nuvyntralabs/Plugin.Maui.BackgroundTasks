#if !ANDROID && !IOS

namespace Plugin.Maui.BackgroundTasks;

partial class BackgroundTaskImplementation
{
	public bool IsSupported => false;

	public BackgroundTaskPlatformInfo Platform { get; } = new(
		isSupported: false,
		nativeScheduler: "None",
		minimumPeriodicInterval: TimeSpan.FromMinutes(15));

	void ScheduleNative(ScheduleRecord record) => throw FeatureNotSupported();

	void CancelNative(string taskId)
	{
	}

	void CancelAllNative()
	{
	}

	static BackgroundTaskException FeatureNotSupported() =>
		new(BackgroundTaskError.FeatureNotSupported, "Background task scheduling is only supported on Android and iOS.");
}

#endif
