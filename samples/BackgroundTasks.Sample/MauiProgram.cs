using Microsoft.Extensions.Logging;
using Plugin.Maui.BackgroundTasks;

namespace BackgroundTasks.Sample;

public static class MauiProgram
{
	public const string RefreshTaskId = "com.nugetworld.backgroundtasks.sample.refresh";
	public const string ProcessingTaskId = "com.nugetworld.backgroundtasks.sample.process";

	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseBackgroundTasks(options =>
			{
				options.EnableLogging = true;
				options.Register<SampleRefreshTask>(RefreshTaskId);
				options.Register<SampleProcessingTask>(ProcessingTaskId);
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
