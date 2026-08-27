namespace Plugin.Maui.BackgroundTasks;

static class BackgroundTaskExecutor
{
	public static BackgroundTaskResult Execute(string taskId, string? parametersJson, CancellationToken cancellationToken) =>
		ExecuteAsync(taskId, parametersJson, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();

	public static async Task<BackgroundTaskResult> ExecuteAsync(string taskId, string? parametersJson, CancellationToken cancellationToken)
	{
		var implementation = BackgroundTasks.Current as BackgroundTaskImplementation;
		var parameters = ScheduleRecordSerializer.DeserializeParameters(parametersJson);

		if (!BackgroundTaskRegistry.TryCreate(taskId, out var task))
		{
			var missing = new BackgroundTaskException(
				BackgroundTaskError.HandlerNotRegistered,
				$"No handler is registered for background task '{taskId}'.");
			implementation?.NotifyFailed(taskId, missing);
			throw missing;
		}

		var context = new BackgroundTaskContext(taskId, parameters, BackgroundTaskRegistry.Services);
		implementation?.NotifyStarted(taskId);

		try
		{
			var result = await task.RunAsync(context, cancellationToken).ConfigureAwait(false);
			implementation?.NotifyCompleted(taskId, result);
			return result;
		}
		catch (OperationCanceledException ex)
		{
			implementation?.NotifyFailed(
				taskId,
				new BackgroundTaskException(BackgroundTaskError.Cancelled, $"Background task '{taskId}' was cancelled.", ex));
			throw;
		}
		catch (Exception ex)
		{
			implementation?.NotifyFailed(taskId, ex);
			throw;
		}
	}
}
