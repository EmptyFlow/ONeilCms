using System.Globalization;

namespace WebApplaud.Databases.KeyValue
{

	internal enum KeyValueType
	{
		Unknown = 0,
		Integer = 1,
		Double = 2,
		String = 3
	};

	internal abstract record KeyValueItem
	{

		private readonly string m_name = "";

		private readonly string m_content = "";

		private readonly KeyValueType m_type = KeyValueType.Unknown;

		public bool IsType(KeyValueType type) => m_type == type;

		internal string AsString()
		{
			if (m_content.Length <= 2) return "";

			return m_content[..^1].Substring(1);
		}

		internal int AsInteger()
		{
			if (m_content.Length == 0) return 0;

			if (int.TryParse(m_content, out var value)) return value;

			return 0;
		}

		internal double AsDouble()
		{
			if (m_content.Length == 0) return 0;

			if (double.TryParse(m_content, CultureInfo.InvariantCulture, out var value)) return value;

			return 0;
		}

	}

	internal class KeyValueDatabase
	{

		private readonly string m_path = "";

		public KeyValueDatabase(string path)
		{
			m_path = path;
		}

		private IEnumerable<string, >

	}

}
