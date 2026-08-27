namespace Plugin.Maui.BackgroundTasks;

/// <summary>
/// Shared configuration applied when the plugin is registered with <c>UseBackgroundTasks</c>.
/// </summary>
public sealed class BackgroundTasksOptions
{
	readonly List<(string TaskId, Type Type)> _typed = [];
	readonly List<(string TaskId, Func<IBackgroundTask> Factory)> _factories = [];

	/// <summary>
	/// Gets or sets a value indicating whether plugin logging starts enabled.
	/// </summary>
	public bool EnableLogging { get; set; }

	/// <summary>
	/// Gets or sets a custom logger. When <c>null</c>, the plugin uses Microsoft.Extensions.Logging if available, otherwise a debug logger.
	/// </summary>
	public IBackgroundTaskLogger? Logger { get; set; }

	/// <summary>
	/// Registers a handler type resolved from dependency injection when the OS launches <paramref name="taskId"/>.
	/// </summary>
	public BackgroundTasksOptions Register<TTask>(string taskId) where TTask : class, IBackgroundTask
	{
		RequestValidator.ValidateTaskId(taskId);
		_typed.Add((taskId, typeof(TTask)));
		return this;
	}

	/// <summary>
	/// Registers a factory that creates the handler for <paramref name="taskId"/>.
	/// </summary>
	public BackgroundTasksOptions Register(string taskId, Func<IBackgroundTask> factory)
	{
		RequestValidator.ValidateTaskId(taskId);
		ArgumentNullException.ThrowIfNull(factory);
		_factories.Add((taskId, factory));
		return this;
	}

	internal IEnumerable<Type> TaskTypes => _typed.Select(entry => entry.Type).Distinct();

	internal IEnumerable<Action> BuildRegistrations()
	{
		foreach (var (taskId, type) in _typed)
			yield return () => BackgroundTaskRegistry.Register(taskId, type);

		foreach (var (taskId, factory) in _factories)
			yield return () => BackgroundTaskRegistry.Register(taskId, _ => factory());
	}
}
