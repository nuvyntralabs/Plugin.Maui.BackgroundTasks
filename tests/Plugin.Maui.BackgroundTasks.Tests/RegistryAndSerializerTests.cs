namespace Plugin.Maui.BackgroundTasks.Tests;

public sealed class RegistryAndSerializerTests : IDisposable
{
	public RegistryAndSerializerTests()
	{
		BackgroundTaskRegistry.Clear();
		BackgroundTaskRegistry.SetServiceProvider(null);
		CountingTask.Runs = 0;
	}

	[Fact]
	public void Registry_CreatesTypedHandler()
	{
		BackgroundTaskRegistry.Register("sync", typeof(CountingTask));

		Assert.True(BackgroundTaskRegistry.IsRegistered("sync"));
		Assert.True(BackgroundTaskRegistry.TryCreate("sync", out var task));
		Assert.IsType<CountingTask>(task);
	}

	[Fact]
	public void Registry_CreatesFactoryHandler()
	{
		BackgroundTaskRegistry.Register("factory", _ => new CountingTask());

		Assert.True(BackgroundTaskRegistry.TryCreate("factory", out var task));
		Assert.IsType<CountingTask>(task);
	}

	[Fact]
	public void Serializer_RoundTripsScheduleRecords()
	{
		var records = new List<ScheduleRecord>
		{
			ScheduleRecord.From(new PeriodicBackgroundTaskRequest
			{
				TaskId = "com.example.sync",
				Interval = TimeSpan.FromMinutes(15),
				Kind = BackgroundTaskKind.Refresh,
				Constraints = new BackgroundTaskConstraints { RequiresNetwork = true },
				Parameters = new Dictionary<string, string> { ["reason"] = "demo" }
			})
		};

		var json = ScheduleRecordSerializer.Serialize(records);
		var restored = ScheduleRecordSerializer.Deserialize(json);

		Assert.Single(restored);
		Assert.Equal("com.example.sync", restored[0].TaskId);
		Assert.True(restored[0].IsPeriodic);
		Assert.Equal(TimeSpan.FromMinutes(15), restored[0].Interval);
		Assert.True(restored[0].RequiresNetwork);
		Assert.Equal("demo", restored[0].Parameters?["reason"]);
	}

	[Fact]
	public async Task RunNow_ExecutesRegisteredHandler()
	{
		var scheduler = new BackgroundTaskImplementation();
		BackgroundTasks.SetDefault(scheduler);
		scheduler.RegisterHandler<CountingTask>("run-now");

		var result = await scheduler.RunNowAsync("run-now", new Dictionary<string, string> { ["n"] = "1" });

		Assert.Equal(BackgroundTaskResult.Success, result);
		Assert.Equal(1, CountingTask.Runs);
	}

	[Fact]
	public async Task Schedule_OnNetTarget_ThrowsFeatureNotSupported()
	{
		var scheduler = new BackgroundTaskImplementation();
		BackgroundTasks.SetDefault(scheduler);

		var ex = await Assert.ThrowsAsync<BackgroundTaskException>(() => scheduler.ScheduleAsync(new BackgroundTaskRequest
		{
			TaskId = "com.example.sync"
		}));

		Assert.Equal(BackgroundTaskError.FeatureNotSupported, ex.Error);
		Assert.False(scheduler.IsSupported);
		Assert.Equal("None", scheduler.Platform.NativeScheduler);
	}

	public void Dispose()
	{
		BackgroundTaskRegistry.Clear();
		BackgroundTaskRegistry.SetServiceProvider(null);
		BackgroundTasks.SetDefault(new BackgroundTaskImplementation());
	}

	sealed class CountingTask : IBackgroundTask
	{
		public static int Runs;

		public Task<BackgroundTaskResult> RunAsync(BackgroundTaskContext context, CancellationToken cancellationToken)
		{
			Interlocked.Increment(ref Runs);
			return Task.FromResult(BackgroundTaskResult.Success);
		}
	}
}
