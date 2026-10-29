namespace WebApplaud.Models
{
	public class CreateUserModel
	{
		public string FirstName { get; init; } = "";

		public string LastName { get; init; } = "";

		public string Login { get; init; } = "";

		public string Password { get; init; } = "";

		public string? Role { get; set; }
	}
}
