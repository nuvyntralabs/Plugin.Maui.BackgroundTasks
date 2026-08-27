using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Plugin.Maui.BackgroundTasks;

/// <summary>
/// Registers the BackgroundTasks plugin with the MAUI dependency injection container.
/// </summary>
public static class MauiAppBuilderExtensions
{
	/// <summary>
	/// Adds <see cref="IBackgroundTaskScheduler"/> as a singleton and registers native platform hooks.
	/// </summary>
	/// <example>
	/// <code>
	/// builder.UseBackgroundTasks(options =>
	/// {
	///     options.EnableLogging = true;
	///     options.Register&lt;SyncTask&gt;("com.example.app.sync");
	/// });
	/// </code>
	/// </example>
	public static MauiAppBuilder UseBackgroundTasks(this MauiAppBuilder builder, Action<BackgroundTasksOptions>? configure = null)
	{
		ArgumentNullException.ThrowIfNull(builder);

		var options = new BackgroundTasksOptions();
		configure?.Invoke(options);

		builder.Services.AddSingleton(options);

		foreach (var taskType in options.TaskTypes)
			builder.Services.AddTransient(taskType);

		builder.Services.AddSingleton<IBackgroundTaskScheduler>(_ => BackgroundTasks.Current);
		builder.Services.AddTransient<IMauiInitializeService, BackgroundTaskInitializer>();

#if IOS
		builder.ConfigureLifecycleEvents(events =>
		{
			events.AddiOS(ios => ios.DidEnterBackground(_ =>
			{
				if (BackgroundTasks.Current is BackgroundTaskImplementation implementation)
					implementation.ResubmitPersistedSchedules();
			}));
		});
#endif

		return builder;
	}

	internal static IBackgroundTaskLogger? CreateLoggerAdapter(IServiceProvider serviceProvider)
	{
		var factory = serviceProvider.GetService<ILoggerFactory>();
		return factory is null ? null : new MicrosoftLoggerAdapter(factory.CreateLogger("Plugin.Maui.BackgroundTasks"));
	}
}
