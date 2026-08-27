namespace Plugin.Maui.BackgroundTasks;

/// <summary>
/// Describes what the current platform can do for background work.
/// </summary>
public sealed class BackgroundTaskPlatformInfo
{
	public BackgroundTaskPlatformInfo(bool isSupported, string nativeScheduler, TimeSpan minimumPeriodicInterval)
	{
		IsSupported = isSupported;
		NativeScheduler = nativeScheduler;
		MinimumPeriodicInterval = minimumPeriodicInterval;
	}

	/// <summary>
	/// Gets a value indicating whether native scheduling is available.
	/// </summary>
	public bool IsSupported { get; }

	/// <summary>
	/// Gets the native API name, such as <c>JobScheduler</c> or <c>BGTaskScheduler</c>.
	/// </summary>
	public string NativeScheduler { get; }

	/// <summary>
	/// Gets the shortest periodic interval the platform will honor.
	/// </summary>
	public TimeSpan MinimumPeriodicInterval { get; }
}
