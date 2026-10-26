namespace WebApplaud
{

	internal static class IdentifierHub
	{

		private static readonly HashSet<string> m_keys = new HashSet<string>();

		internal static string GenerateIdentifier()
		{
			var newId = Guid.NewGuid().ToString().Replace("-", "");
			if (m_keys.Contains(newId)) throw new Exception($"Conflict keys: {newId}");

			m_keys.Add(newId);
			return newId;
		}

	}

}
