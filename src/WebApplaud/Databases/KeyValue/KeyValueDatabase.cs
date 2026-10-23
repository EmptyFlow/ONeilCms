namespace WebApplaud.Databases.KeyValue
{

	internal class KeyValueDatabase
	{

		private string m_loadedPath = "";

		private readonly Dictionary<string, KeyValueItem> m_keys = [];

		public KeyValueDatabase()
		{
		}

		public async Task LoadData(string path)
		{
			await foreach (var line in File.ReadLinesAsync(path))
			{
				var firstSpaceIndex = line.IndexOf(' ');
				if (firstSpaceIndex == -1) continue;

				var name = line.Substring(0, firstSpaceIndex - 1);
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

		internal void SetString(string name, string value)
		{
			if (!m_keys.ContainsKey(name))
			{
				m_keys.Add(name, new KeyValueItem(name, $"\"{value}\""));
				return;
			}

			m_keys[name] = new KeyValueItem(name, $"\"{value}\"");
		}

	}

}
