namespace WebApplaud.Databases.KeyValue
{

	internal class KeyValueDatabase
	{

		private string m_loadedPath = "";

		private readonly Dictionary<string, KeyValueItem> m_keys = [];

		public KeyValueDatabase()
		{
		}

		public async Task LoadDatabase(string path)
		{
			if (m_keys.Any()) m_keys.Clear();

			if (!File.Exists(path)) await File.WriteAllTextAsync(path, "");

			await foreach (var line in File.ReadLinesAsync(path))
			{
				var firstSpaceIndex = line.IndexOf(' ');
				if (firstSpaceIndex == -1) continue;

				var name = line.Substring(0, firstSpaceIndex);
				if (m_keys.ContainsKey(name)) continue;

				m_keys.Add(name, new KeyValueItem(name, line.Substring(firstSpaceIndex + 1)));
			}
			m_loadedPath = path;
		}

		public async Task SaveData()
		{
			await File.WriteAllLinesAsync(m_loadedPath, m_keys.Values.Select(a => a.ToString()).ToArray());
		}

		internal bool IsNull(string name) => !m_keys.ContainsKey(name);

		internal bool IsNotNull(string name) => m_keys.ContainsKey(name);

		internal string GetString(string name)
		{
			if (!m_keys.TryGetValue(name, out KeyValueItem? value)) return "";

			return value.AsString();
		}

		internal int GetInteger(string name)
		{
			if (!m_keys.TryGetValue(name, out KeyValueItem? value)) return 0;

			return value.AsInteger();
		}

		internal double GetDouble(string name)
		{
			if (!m_keys.TryGetValue(name, out KeyValueItem? value)) return 0.0;

			return value.AsDouble();
		}

		internal void SetString(string name, string value)
		{
			if (!m_keys.ContainsKey(name))
			{
				m_keys.Add(name, new KeyValueItem(name, $"\"{value}\""));
				return;
			}

			m_keys[name] = new KeyValueItem(name, $"\"{value}\"");
		}

		internal void SetInteger(string name, int value)
		{
			if (!m_keys.ContainsKey(name))
			{
				m_keys.Add(name, new KeyValueItem(name, $"{value}"));
				return;
			}

			m_keys[name] = new KeyValueItem(name, $"{value}");
		}

		internal void SetDouble(string name, double value)
		{
			if (!m_keys.ContainsKey(name))
			{
				m_keys.Add(name, new KeyValueItem(name, $"{value}"));
				return;
			}

			m_keys[name] = new KeyValueItem(name, $"{value}");
		}

	}

}
