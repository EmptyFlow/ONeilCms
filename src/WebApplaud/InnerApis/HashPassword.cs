using Microsoft.AspNetCore.Identity;

namespace WebApplaud.InnerApis
{

	internal sealed class PasswordHash
	{
		public string Hash { get; set; } = "";
	}

	internal static class HashPassword
	{
		private static readonly PasswordHasher<PasswordHash> Hasher = new();

		public static string Hash(string password)
		{
			var user = new PasswordHash();
			return Hasher.HashPassword(user, password);
		}

		public static bool Verify(string hash, string password)
		{
			var passwordHash = new PasswordHash { Hash = hash };
			var result = Hasher.VerifyHashedPassword(passwordHash, passwordHash.Hash, password);

			return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
		}
	}

}
