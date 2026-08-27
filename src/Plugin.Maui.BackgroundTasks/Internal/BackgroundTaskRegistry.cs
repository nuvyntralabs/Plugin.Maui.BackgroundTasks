using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Plugin.Maui.BackgroundTasks;

static class BackgroundTaskRegistry
{
	static readonly ConcurrentDictionary<string, Registration> Registrations = new(StringComparer.Ordinal);

	public static IServiceProvider? Services { get; private set; }

	public static IReadOnlyCollection<string> TaskIds => Registrations.Keys.ToArray();

	public static void SetServiceProvider(IServiceProvider? services) => Services = services;

	public static void Register(string taskId, Type taskType)
	{
		RequestValidator.ValidateTaskId(taskId);
		ArgumentNullException.ThrowIfNull(taskType);

		if (!typeof(IBackgroundTask).IsAssignableFrom(taskType))
			throw new ArgumentException($"Type '{taskType.FullName}' does not implement {nameof(IBackgroundTask)}.", nameof(taskType));

		Registrations[taskId] = new Registration(taskType, null);
	}

	public static void Register(string taskId, Func<IServiceProvider?, IBackgroundTask> factory)
	{
		RequestValidator.ValidateTaskId(taskId);
		ArgumentNullException.ThrowIfNull(factory);
		Registrations[taskId] = new Registration(null, factory);
	}

	public static bool IsRegistered(string taskId) => Registrations.ContainsKey(taskId);

	public static bool TryCreate(string taskId, [NotNullWhen(true)] out IBackgroundTask? task)
	{
		task = null;
		if (!Registrations.TryGetValue(taskId, out var registration))
			return false;

		if (registration.Factory is not null)
		{
			task = registration.Factory(Services);
			return task is not null;
		}

		if (registration.TaskType is null)
			return false;

		if (Services is not null)
		{
			task = (IBackgroundTask)ActivatorUtilities.GetServiceOrCreateInstance(Services, registration.TaskType);
			return true;
		}

		task = (IBackgroundTask?)Activator.CreateInstance(registration.TaskType);
		return task is not null;
	}

	public static void Clear() => Registrations.Clear();

	sealed record Registration(Type? TaskType, Func<IServiceProvider?, IBackgroundTask>? Factory);
}
