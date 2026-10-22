using WebApplaud.Databases.KeyValue;

namespace WebApplaudTests
{

	public class KeyValueItemUnitTest
	{

		[Test]
		public void Constructor_Completed()
		{
			//arrange
			var valueItem = new KeyValueItem("test", "1");
			
			//assert
			Assert.NotNull(valueItem);
		}

		[Test]
		public void Constructor_Failed_NullName()
		{
			//assert
			Assert.Throws<ArgumentNullException>(() => {
				//act and arrange
				var valueItem = new KeyValueItem(null, "1");
			});
		}

		[Test]
		public void Constructor_Failed_NullContent()
		{
			//assert
			Assert.Throws<ArgumentNullException>(() => {
				//act and arrange
				var valueItem = new KeyValueItem("name", null);
			});
		}

		[Test]
		public void Constructor_Completed_IntegerType_Case1()
		{
			//arrange
			var valueItem = new KeyValueItem("name", "1");
			
			//assert
			Assert.Equals(valueItem.IsType(KeyValueType.Integer), true);
		}

		[Test]
		public void Constructor_Completed_IntegerType_Case2()
		{
			//arrange
			var valueItem = new KeyValueItem("name", "5909345894");

			//assert
			Assert.Equals(valueItem.IsType(KeyValueType.Integer), true);
		}

		[Test]
		public void Constructor_Completed_DoubleType_Case1()
		{
			//arrange
			var valueItem = new KeyValueItem("name", "0.1");

			//assert
			Assert.Equals(valueItem.IsType(KeyValueType.Double), true);
		}

		[Test]
		public void Constructor_Completed_DoubleType_Case2()
		{
			//arrange
			var valueItem = new KeyValueItem("name", "12120.325425234");

			//assert
			Assert.Equals(valueItem.IsType(KeyValueType.Double), true);
		}

		[Test]
		public void Constructor_Completed_StringType_Case1()
		{
			//arrange
			var valueItem = new KeyValueItem("name", "\"Test string\"");

			//assert
			Assert.Equals(valueItem.IsType(KeyValueType.String), true);
		}

		[Test]
		public void Constructor_Completed_StringType_Case2()
		{
			//arrange
			var valueItem = new KeyValueItem("name", "\"lalal 🥰\"");

			//assert
			Assert.Equals(valueItem.IsType(KeyValueType.String), true);
		}


	}

}
