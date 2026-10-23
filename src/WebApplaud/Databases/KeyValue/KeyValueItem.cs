namespace WebApplaud.Databases.KeyValue
{
	internal record KeyValueItem
	{

		private readonly string m_name = "";

		private readonly string m_content = "";

		private readonly KeyValueType m_type = KeyValueType.Unknown;

		internal bool IsType(KeyValueType type) => m_type == type;

		internal KeyValueItem(string name, string content)
		{
			m_name = name ?? throw new ArgumentNullException(nameof(name));
			m_content = content ?? throw new ArgumentNullException(nameof(content));

			if (m_content.StartsWith("\"") && m_content.EndsWith("\""))
			{
				m_type = KeyValueType.String;
				return;
			}
			if (double.TryParse(m_content, out _))
			{
				m_type = KeyValueType.Double;
				return;
			}
			if (int.TryParse(m_content, out _))
			{
				m_type = KeyValueType.Integer;
				return;
			}
		}

		internal string AsString()
		{
			if (m_type != KeyValueType.String) return string.Empty;
			if (m_content.Length <= 2) return string.Empty;

			return m_content[..^1].Substring(1);
		}

		internal int AsInteger() => m_type == KeyValueType.Integer ? int.Parse(m_content) : 0;

		internal double AsDouble() => m_type == KeyValueType.Double ? double.Parse(m_content) : 0.0;

		public override string ToString() => $"{m_name} {m_content}";

	}

}
