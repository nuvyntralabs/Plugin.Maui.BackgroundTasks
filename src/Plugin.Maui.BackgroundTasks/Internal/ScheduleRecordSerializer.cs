using System.Text.Json;
using System.Text.Json.Serialization;

namespace Plugin.Maui.BackgroundTasks;

static class ScheduleRecordSerializer
{
	static readonly JsonSerializerOptions Options = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		WriteIndented = false
	};

	public static string Serialize(IReadOnlyList<ScheduleRecord> records) =>
		JsonSerializer.Serialize(records, Options);

	public static List<ScheduleRecord> Deserialize(string? json)
	{
		if (string.IsNullOrWhiteSpace(json))
			return [];

		return JsonSerializer.Deserialize<List<ScheduleRecord>>(json, Options) ?? [];
	}

	public static string SerializeParameters(IReadOnlyDictionary<string, string>? parameters)
	{
		if (parameters is null || parameters.Count == 0)
			return string.Empty;

		return JsonSerializer.Serialize(parameters, Options);
	}

	public static IReadOnlyDictionary<string, string> DeserializeParameters(string? json)
	{
		if (string.IsNullOrWhiteSpace(json))
			return new Dictionary<string, string>();

		return JsonSerializer.Deserialize<Dictionary<string, string>>(json, Options)
			?? new Dictionary<string, string>();
	}
}
