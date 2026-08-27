namespace Plugin.Maui.BackgroundTasks;

/// <summary>
/// Conditions that must be met before the operating system should run the work.
/// </summary>
public sealed class BackgroundTaskConstraints
{
	/// <summary>
	/// Gets or sets a value indicating whether the device must have network connectivity.
	/// On iOS this applies to <see cref="BackgroundTaskKind.Processing"/> tasks.
	/// </summary>
	public bool RequiresNetwork { get; init; }

	/// <summary>
	/// Gets or sets a value indicating whether the device must be charging.
	/// On iOS this maps to <c>RequiresExternalPower</c> for processing tasks.
	/// </summary>
	public bool RequiresCharging { get; init; }

	/// <summary>
	/// Gets or sets a value indicating whether the battery must not be low.
	/// Honored on Android. iOS does not expose an equivalent constraint.
	/// </summary>
	public bool RequiresBatteryNotLow { get; init; }

	/// <summary>
	/// An empty constraint set.
	/// </summary>
	public static BackgroundTaskConstraints None { get; } = new();
}
