namespace Plugin.Maui.BackgroundTasks;

/// <summary>
/// An <see cref="IBackgroundTask"/> backed by a delegate. Convenient for tests and small handlers.
/// </summary>
public sealed class DelegateBackgroundTask : IBackgroundTask
{
	readonly Func<BackgroundTaskContext, CancellationToken, Task<BackgroundTaskResult>> _run;

	public DelegateBackgroundTask(Func<BackgroundTaskContext, CancellationToken, Task<BackgroundTaskResult>> run)
	{
		_run = run ?? throw new ArgumentNullException(nameof(run));
	}

	public DelegateBackgroundTask(Func<BackgroundTaskContext, CancellationToken, Task> run)
	{
		ArgumentNullException.ThrowIfNull(run);
		_run = async (context, cancellationToken) =>
		{
			await run(context, cancellationToken).ConfigureAwait(false);
			return BackgroundTaskResult.Success;
		};
	}

	/// <inheritdoc />
	public Task<BackgroundTaskResult> RunAsync(BackgroundTaskContext context, CancellationToken cancellationToken) =>
		_run(context, cancellationToken);
}
