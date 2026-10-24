using WebApplaud.Databases.KeyValue;

namespace WebApplaudTests
{

	public class KeyValueDatabaseIntegrationTests
	{

		[Test]
		public async Task LoadData_Completed_IsNull()
		{
			//arrange
			var database = new KeyValueDatabase();
			var path = "LoadData_Completed_Case1.kv";
			if (File.Exists(path)) File.Delete(path);

			await File.WriteAllTextAsync(
				path,
"""
test1 "lalalala pruha bruha muhaha 😘"
digitvalue 12343
doublevalue 4546.0908
"""
			);

			//act
			await database.LoadData(path);
			var muhahaNull = database.IsNull("muhaha");

			//assert
			Assert.Equals(muhahaNull, true);
		}

		[Test]
		public async Task LoadData_Completed_GetString()
		{
			//arrange
			var database = new KeyValueDatabase();
			var path = "LoadData_Completed_Case1.kv";
			if (File.Exists(path)) File.Delete(path);

			await File.WriteAllTextAsync(
				path,
"""
test1 "lalalala pruha bruha muhaha 😘"
digitvalue 12343
doublevalue 4546.0908
"""
			);

			//act
			await database.LoadData(path);
			var value = database.GetString("test1");

			//assert
			Assert.Equals(value, "lalalala pruha bruha muhaha 😘");
		}


	}
}
