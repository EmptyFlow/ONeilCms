namespace WebApplaud.Models
{

	internal record UserProfileModel
	{

		public required string Id { get; set; } = "";

		public string FirstName { get; init; } = "";

		public string LastName { get; init; } = "";

		public string Login { get; init; } = "";

		public string Password { get; init; } = "";

		public string? Role { get; set; }

		public string? Permissions { get; set; }

	}

}
