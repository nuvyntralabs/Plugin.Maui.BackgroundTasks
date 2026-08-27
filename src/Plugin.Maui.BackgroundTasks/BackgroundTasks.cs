namespace Plugin.Maui.BackgroundTasks;

/// <summary>
/// Entry point for the BackgroundTasks plugin when dependency injection is not used.
/// </summary>
public static class BackgroundTasks
{
	static IBackgroundTaskScheduler? _current;

	/// <summary>
	/// Gets the shared <see cref="IBackgroundTaskScheduler"/> instance.
	/// </summary>
	public static IBackgroundTaskScheduler Current => _current ??= new BackgroundTaskImplementation();

	/// <summary>
	/// Replaces the shared instance. Intended for tests and custom implementations.
	/// </summary>
	public static void SetDefault(IBackgroundTaskScheduler implementation) =>
		_current = implementation ?? throw new ArgumentNullException(nameof(implementation));

	/// <summary>
	/// Registers native iOS <c>BGTaskScheduler</c> callbacks for every known task id.
	/// Called automatically from <c>UseBackgroundTasks</c>. Safe to call more than once.
	/// </summary>
	public static void RegisterPlatformHandlers()
	{
#if IOS
		IosBackgroundTaskRegistrar.RegisterAllKnown();
#endif
	}
}
