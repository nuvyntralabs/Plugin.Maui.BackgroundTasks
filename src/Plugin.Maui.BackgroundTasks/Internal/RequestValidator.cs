namespace Plugin.Maui.BackgroundTasks;

static class RequestValidator
{
	public static void ValidateTaskId(string taskId)
	{
		if (string.IsNullOrWhiteSpace(taskId))
			throw new ArgumentException("TaskId is required.", nameof(taskId));

		if (taskId.Length > 255)
			throw new ArgumentException("TaskId must be 255 characters or fewer.", nameof(taskId));
	}

	public static void Validate(BackgroundTaskRequest request)
	{
		ArgumentNullException.ThrowIfNull(request);
		ValidateTaskId(request.TaskId);

		if (request.Delay is { } delay && delay < TimeSpan.Zero)
			throw new ArgumentOutOfRangeException(nameof(request), "Delay cannot be negative.");
	}

	public static void Validate(PeriodicBackgroundTaskRequest request)
	{
		ArgumentNullException.ThrowIfNull(request);
		ValidateTaskId(request.TaskId);

		if (request.Interval <= TimeSpan.Zero)
			throw new ArgumentOutOfRangeException(nameof(request), "Interval must be greater than zero.");
	}
}
