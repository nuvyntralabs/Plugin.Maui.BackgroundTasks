using Android.App;
using Android.App.Job;
using Android.Runtime;

namespace Plugin.Maui.BackgroundTasks;

/// <summary>
/// Android <see cref="JobService"/> that dispatches to a registered <see cref="IBackgroundTask"/>.
/// </summary>
[Service(
	Name = "plugin.maui.backgroundtasks.PluginJobService",
	Permission = "android.permission.BIND_JOB_SERVICE",
	Exported = true)]
[Register("plugin.maui.backgroundtasks.PluginJobService")]
public sealed class PluginJobService : JobService
{
	internal const string TaskIdKey = "taskId";
	internal const string ParametersKey = "parameters";

	public override bool OnStartJob(JobParameters? @params)
	{
		var taskId = @params?.Extras?.GetString(TaskIdKey);
		if (string.IsNullOrWhiteSpace(taskId) || @params is null)
		{
			if (@params is not null)
				JobFinished(@params, false);

			return false;
		}

		var parametersJson = @params.Extras?.GetString(ParametersKey);

		_ = Task.Run(() =>
		{
			var retry = false;
			try
			{
				var result = BackgroundTaskExecutor.Execute(taskId, parametersJson, CancellationToken.None);
				retry = result == BackgroundTaskResult.Retry;
			}
			catch (Exception ex)
			{
				Android.Util.Log.Warn("Plugin.Maui.BackgroundTasks", $"Background task '{taskId}' failed: {ex}");
				retry = ex is not OperationCanceledException;
			}
			finally
			{
				JobFinished(@params, retry);
			}
		});

		return true;
	}

	public override bool OnStopJob(JobParameters? @params) => true;
}
