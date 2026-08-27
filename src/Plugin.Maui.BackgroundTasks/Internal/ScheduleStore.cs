using Microsoft.Maui.Storage;

namespace Plugin.Maui.BackgroundTasks;

static class ScheduleStore
{
	internal const string PreferenceKey = "Plugin.Maui.BackgroundTasks.Schedules";
	static readonly object Gate = new();

	public static IReadOnlyList<ScheduleRecord> GetAll()
	{
		lock (Gate)
		{
			try
			{
				var json = Preferences.Default.Get(PreferenceKey, string.Empty);
				return ScheduleRecordSerializer.Deserialize(json);
			}
			catch
			{
				return [];
			}
		}
	}

	public static ScheduleRecord? Get(string taskId) =>
		GetAll().FirstOrDefault(record => string.Equals(record.TaskId, taskId, StringComparison.Ordinal));

	public static void Upsert(ScheduleRecord record)
	{
		lock (Gate)
		{
			var records = GetAllUnlocked().ToList();
			records.RemoveAll(existing => string.Equals(existing.TaskId, record.TaskId, StringComparison.Ordinal));
			records.Add(record);
			Save(records);
		}
	}

	public static void Remove(string taskId)
	{
		lock (Gate)
		{
			var records = GetAllUnlocked().ToList();
			if (records.RemoveAll(existing => string.Equals(existing.TaskId, taskId, StringComparison.Ordinal)) > 0)
				Save(records);
		}
	}

	public static void Clear()
	{
		lock (Gate)
		{
			try
			{
				Preferences.Default.Remove(PreferenceKey);
			}
			catch
			{
				// Preferences may be unavailable in unit tests.
			}
		}
	}

	static List<ScheduleRecord> GetAllUnlocked()
	{
		try
		{
			var json = Preferences.Default.Get(PreferenceKey, string.Empty);
			return ScheduleRecordSerializer.Deserialize(json);
		}
		catch
		{
			return [];
		}
	}

	static void Save(List<ScheduleRecord> records)
	{
		try
		{
			Preferences.Default.Set(PreferenceKey, ScheduleRecordSerializer.Serialize(records));
		}
		catch
		{
			// Preferences may be unavailable in unit tests.
		}
	}
}
