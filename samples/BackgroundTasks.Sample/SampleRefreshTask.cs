using Microsoft.Maui.Storage;
using Plugin.Maui.BackgroundTasks;

namespace BackgroundTasks.Sample;

public sealed class SampleRefreshTask : IBackgroundTask
{
	public const string LastRunKey = "sample.refresh.lastRun";

	public async Task<BackgroundTaskResult> RunAsync(BackgroundTaskContext context, CancellationToken cancellationToken)
	{
		await Task.Delay(250, cancellationToken).ConfigureAwait(false);
		Preferences.Default.Set(LastRunKey, DateTimeOffset.Now.ToString("u"));
		Preferences.Default.Set(SampleProcessingTask.LastTaskKey, context.TaskId);
		return BackgroundTaskResult.Success;
	}
}
