namespace Plugin.Maui.BackgroundTasks.Tests;

public sealed class RequestValidatorTests
{
	[Fact]
	public void ValidateTaskId_RejectsEmpty()
	{
		Assert.Throws<ArgumentException>(() => RequestValidator.ValidateTaskId(""));
		Assert.Throws<ArgumentException>(() => RequestValidator.ValidateTaskId("   "));
	}

	[Fact]
	public void ValidateTaskId_RejectsOverlong()
	{
		Assert.Throws<ArgumentException>(() => RequestValidator.ValidateTaskId(new string('a', 256)));
	}

	[Fact]
	public void Validate_OneTime_RejectsNegativeDelay()
	{
		var request = new BackgroundTaskRequest
		{
			TaskId = "com.example.sync",
			Delay = TimeSpan.FromMinutes(-1)
		};

		Assert.Throws<ArgumentOutOfRangeException>(() => RequestValidator.Validate(request));
	}

	[Fact]
	public void Validate_Periodic_RejectsNonPositiveInterval()
	{
		var request = new PeriodicBackgroundTaskRequest
		{
			TaskId = "com.example.sync",
			Interval = TimeSpan.Zero
		};

		Assert.Throws<ArgumentOutOfRangeException>(() => RequestValidator.Validate(request));
	}

	[Fact]
	public void Validate_AcceptsValidRequests()
	{
		RequestValidator.Validate(new BackgroundTaskRequest
		{
			TaskId = "com.example.sync",
			Delay = TimeSpan.FromMinutes(5)
		});

		RequestValidator.Validate(new PeriodicBackgroundTaskRequest
		{
			TaskId = "com.example.sync",
			Interval = TimeSpan.FromMinutes(15)
		});
	}
}
