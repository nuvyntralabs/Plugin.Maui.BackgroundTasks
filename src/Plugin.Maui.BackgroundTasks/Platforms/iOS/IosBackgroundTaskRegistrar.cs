using BackgroundTasks;

namespace Plugin.Maui.BackgroundTasks;

static class IosBackgroundTaskRegistrar
{
	static readonly HashSet<string> Registered = new(StringComparer.Ordinal);
	static readonly object Gate = new();

	public static void Register(string taskId)
	{
		RequestValidator.ValidateTaskId(taskId);

		lock (Gate)
		{
			if (!Registered.Add(taskId))
				return;

			BGTaskScheduler.Shared.Register(taskId, null, task => Handle(task));
		}
	}

	public static void RegisterAllKnown()
	{
		foreach (var taskId in BackgroundTaskRegistry.TaskIds)
			Register(taskId);

		foreach (var record in ScheduleStore.GetAll())
			Register(record.TaskId);
	}

	static void Handle(BGTask task)
	{
		var taskId = task.Identifier;
		using var cts = new CancellationTokenSource();
		task.ExpirationHandler = () =>
		{
			try
			{
				cts.Cancel();
			}
			catch
			{
				// already disposed
			}
		};

		var record = ScheduleStore.Get(taskId);
		if (record?.IsPeriodic == true)
		{
			try
			{
				IosBackgroundScheduler.Submit(record);
			}
			catch
			{
				// The current run should still proceed even if the next submit fails.
			}
		}

		var parametersJson = ScheduleRecordSerializer.SerializeParameters(record?.Parameters);
		_ = Task.Run(async () =>
		{
			var completed = false;
			try
			{
				var result = await BackgroundTaskExecutor.ExecuteAsync(taskId, parametersJson, cts.Token).ConfigureAwait(false);
				completed = result == BackgroundTaskResult.Success;
			}
			catch (OperationCanceledException)
			{
				completed = false;
			}
			catch
			{
				completed = false;
			}
			finally
			{
				task.SetTaskCompleted(completed);
			}
		});
	}
}
