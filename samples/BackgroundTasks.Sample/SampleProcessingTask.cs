using Microsoft.Maui.Storage;
using Plugin.Maui.BackgroundTasks;

namespace BackgroundTasks.Sample;

public sealed class SampleProcessingTask : IBackgroundTask
{
	public const string LastRunKey = "sample.process.lastRun";
	public const string LastTaskKey = "sample.last.task";

	public async Task<BackgroundTaskResult> RunAsync(BackgroundTaskContext context, CancellationToken cancellationToken)
	{
		await Task.Delay(400, cancellationToken).ConfigureAwait(false);
		Preferences.Default.Set(LastRunKey, DateTimeOffset.Now.ToString("u"));
		Preferences.Default.Set(LastTaskKey, context.TaskId);
		return BackgroundTaskResult.Success;
	}
}
