using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;

namespace Plugin.Maui.BackgroundTasks;

sealed class BackgroundTaskInitializer : IMauiInitializeService
{
	public void Initialize(IServiceProvider services)
	{
		var options = services.GetService<BackgroundTasksOptions>() ?? new BackgroundTasksOptions();
		BackgroundTaskRegistry.SetServiceProvider(services);

		foreach (var register in options.BuildRegistrations())
			register();

		var scheduler = BackgroundTasks.Current;
		if (options.EnableLogging)
		{
			var logger = options.Logger
				?? MauiAppBuilderExtensions.CreateLoggerAdapter(services)
				?? new DebugBackgroundTaskLogger();
			scheduler.EnableLogging(true, logger);
		}

		BackgroundTasks.RegisterPlatformHandlers();
	}
}
