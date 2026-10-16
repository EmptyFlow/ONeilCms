namespace WebApplaud.Models
{

	internal class UserSessionModel
	{

		public required string Token { get; init; } = "";

		public required string Id { get; init; } = "";
		
		public required DateTime Expire { get; set; }

		public required string App { get; set; } = "";

	}

}
