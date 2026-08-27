namespace Plugin.Maui.BackgroundTasks;

/// <summary>
/// Runtime information passed to <see cref="IBackgroundTask.RunAsync"/>.
/// </summary>
public sealed class BackgroundTaskContext
{
	public BackgroundTaskContext(
		string taskId,
		IReadOnlyDictionary<string, string> parameters,
		IServiceProvider? services)
	{
		TaskId = taskId;
		Parameters = parameters;
		Services = services;
	}

	/// <summary>
	/// Gets the scheduled task identifier.
	/// </summary>
	public string TaskId { get; }

	/// <summary>
	/// Gets parameters supplied when the work was scheduled.
	/// </summary>
	public IReadOnlyDictionary<string, string> Parameters { get; }

	/// <summary>
	/// Gets the MAUI service provider when the plugin was registered with <c>UseBackgroundTasks</c>.
	/// </summary>
	public IServiceProvider? Services { get; }

	/// <summary>
	/// Gets the UTC time when this execution started.
	/// </summary>
	public DateTimeOffset StartedAt { get; } = DateTimeOffset.UtcNow;
}
