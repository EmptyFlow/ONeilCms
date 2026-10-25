using WebApplaud.InnerApis;

namespace WebApplaudTests
{

	public class HashPasswordUnitTests
	{

		[Test]
		public void HashPassword_Completed_HappyPath()
		{
			//arrange
			var valueItem = HashPassword.Hash("lalala");

			var verify = HashPassword.Verify(valueItem, "lalala");

			//assert
			Assert.Equals(verify, true);
		}

	}

}
