namespace Plugin.Maui.BackgroundTasks;

static class AndroidJobId
{
	public static int FromTaskId(string taskId)
	{
		unchecked
		{
			uint hash = 2166136261;
			foreach (var character in taskId)
			{
				hash ^= character;
				hash *= 16777619;
			}

			var id = (int)(hash & 0x7FFFFFFF);
			return id == 0 ? 1 : id;
		}
	}
}
