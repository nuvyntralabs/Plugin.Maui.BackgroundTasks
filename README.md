# Plugin.Maui.BackgroundTasks

[NuGet](https://www.nuget.org/packages/Plugin.Maui.BackgroundTasks)

A .NET MAUI plugin that gives **Android** and **iOS** a single API for scheduling background work.

- One-time and periodic tasks
- Shared constraints (network, charging, battery)
- Registered handlers that run when the OS launches the work
- Optional logging and execution events
- In-process `RunNowAsync` for tests and demos

Android uses **JobScheduler**. iOS uses **BGTaskScheduler** (`BGAppRefreshTask` / `BGProcessingTask`).

## Install

Package: [https://www.nuget.org/packages/Plugin.Maui.BackgroundTasks](https://www.nuget.org/packages/Plugin.Maui.BackgroundTasks)

```bash
dotnet add package Plugin.Maui.BackgroundTasks
```

Or reference the project:

```xml
<ProjectReference Include="..\src\Plugin.Maui.BackgroundTasks\Plugin.Maui.BackgroundTasks.csproj" />
```

## Register the plugin

```csharp
builder
    .UseMauiApp<App>()
    .UseBackgroundTasks(options =>
    {
        options.EnableLogging = true;
        options.Register<SyncTask>("com.example.app.sync");
        options.Register<CleanupTask>("com.example.app.cleanup");
    });
```

Resolve `IBackgroundTaskScheduler` from dependency injection, or use `BackgroundTasks.Current`.

## Define a task

```csharp
public sealed class SyncTask : IBackgroundTask
{
    public async Task<BackgroundTaskResult> RunAsync(
        BackgroundTaskContext context,
        CancellationToken cancellationToken)
    {
        // Keep iOS refresh work short. The OS can expire the task.
        await SyncAsync(context.Parameters, cancellationToken);
        return BackgroundTaskResult.Success;
    }
}
```

Return `Retry` when the work should be attempted again (honored by Android JobScheduler).

## Schedule work

```csharp
var scheduler = BackgroundTasks.Current;

await scheduler.ScheduleAsync(new BackgroundTaskRequest
{
    TaskId = "com.example.app.sync",
    Delay = TimeSpan.FromMinutes(5),
    Kind = BackgroundTaskKind.Refresh,
    Constraints = new BackgroundTaskConstraints
    {
        RequiresNetwork = true,
        RequiresBatteryNotLow = true
    }
});

await scheduler.SchedulePeriodicAsync(new PeriodicBackgroundTaskRequest
{
    TaskId = "com.example.app.sync",
    Interval = TimeSpan.FromMinutes(15),
    Kind = BackgroundTaskKind.Refresh,
    Constraints = new BackgroundTaskConstraints { RequiresNetwork = true }
});
```

Cancel, inspect, or run immediately:

```csharp
await scheduler.CancelAsync("com.example.app.sync");
var scheduled = await scheduler.GetScheduledTasksAsync();
var result = await scheduler.RunNowAsync("com.example.app.sync");
```

## Host app setup

### Android

The package declares `WAKE_LOCK`, `RECEIVE_BOOT_COMPLETED`, and `ACCESS_NETWORK_STATE`, and registers a `JobService`. Jobs persist across process death and reboots.

No extra manifest identifiers are required. Use reverse-DNS task ids so they stay unique across the app.

### iOS

Declare background modes and every task identifier in `Platforms/iOS/Info.plist`:

```xml
<key>UIBackgroundModes</key>
<array>
    <string>fetch</string>
    <string>processing</string>
</array>
<key>BGTaskSchedulerPermittedIdentifiers</key>
<array>
    <string>com.example.app.sync</string>
    <string>com.example.app.cleanup</string>
</array>
```

`UseBackgroundTasks` registers `BGTaskScheduler` handlers during MAUI startup (inside `FinishedLaunching`). Identifiers that are missing from Info.plist will fail at schedule time with a clear error.

Because the plugin stores schedules in `Preferences`, add the User Defaults reason to the iOS privacy manifest if you have not already:

```xml
<key>NSPrivacyAccessedAPIType</key>
<string>NSPrivacyAccessedAPICategoryUserDefaults</string>
```

### Testing iOS tasks

iOS decides when refresh and processing tasks actually run. In the Xcode debugger you can force a launch:

```
e -l objc -- (void)[[BGTaskScheduler sharedScheduler] _simulateLaunchForTaskWithIdentifier:@"com.example.app.sync"]
```

If the user force-quits the app, iOS will not launch scheduled background tasks until the user opens the app again.

## Platform notes

| | Android | iOS |
| --- | --- | --- |
| Native API | JobScheduler | BGTaskScheduler |
| Periodic minimum | 15 minutes (clamped) | Requested interval is an earliest-begin hint |
| Survives process death | Yes | Yes, unless the user force-quits |
| Survives reboot | Yes | The plugin resubmits when the app launches or backgrounds |
| Exact timing | Not guaranteed | Not guaranteed |

`net10.0` is included so shared code can reference the package. Scheduling APIs throw `BackgroundTaskException` (`FeatureNotSupported`) on that target. `RunNowAsync` still executes registered handlers.

This plugin is for **deferrable** work. Long-running, user-visible operations (music, navigation, large uploads) still need a host-app Android foreground service and the matching iOS background mode.

## Sample

`samples/BackgroundTasks.Sample` schedules, cancels, and runs the sample refresh/processing handlers.

```bash
dotnet build src/Plugin.Maui.BackgroundTasks/Plugin.Maui.BackgroundTasks.csproj
dotnet pack src/Plugin.Maui.BackgroundTasks/Plugin.Maui.BackgroundTasks.csproj -c Release
dotnet build samples/BackgroundTasks.Sample/BackgroundTasks.Sample.csproj -f net10.0-android
```

## Pack

```bash
dotnet pack src/Plugin.Maui.BackgroundTasks/Plugin.Maui.BackgroundTasks.csproj -c Release
```

Packages are written to `artifacts/`.
