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
			var path = "LoadData_Completed_GetString.kv";
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

		[Test]
		public async Task LoadData_Completed_GetInteger()
		{
			//arrange
			var database = new KeyValueDatabase();
			var path = "LoadData_Completed_GetInteger.kv";
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
			var value = database.GetInteger("digitvalue");

			//assert
			Assert.Equals(value, 12343);
		}

		[Test]
		public async Task LoadData_Completed_GetDouble()
		{
			//arrange
			var database = new KeyValueDatabase();
			var path = "LoadData_Completed_GetDouble.kv";
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
			var value = database.GetDouble("doublevalue");

			//assert
			Assert.Equals(value, 4546.0908);
		}

		[Test]
		public async Task LoadData_Completed_SetString()
		{
			//arrange
			var database = new KeyValueDatabase();
			var path = "LoadData_Completed_SetString.kv";
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
			database.SetString("test1", "blablabla");
			var newValue = database.GetString("test1");

			//assert
			Assert.Equals(newValue, "blablabla");
		}

		[Test]
		public async Task LoadData_Completed_SetInteger()
		{
			//arrange
			var database = new KeyValueDatabase();
			var path = "LoadData_Completed_SetInteger.kv";
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
			database.SetInteger("digitvalue", 4676);
			var newValue = database.GetInteger("digitvalue");

			//assert
			Assert.Equals(newValue, 4676);
		}

		[Test]
		public async Task LoadData_Completed_SetDouble()
		{
			//arrange
			var database = new KeyValueDatabase();
			var path = "LoadData_Completed_SetDouble.kv";
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
			database.SetDouble("doublevalue", 123.456);
			var newValue = database.GetString("doublevalue");

			//assert
			Assert.Equals(newValue, 123.456);
		}

		[Test]
		public async Task LoadData_Completed_SaveData()
		{
			//arrange
			var database = new KeyValueDatabase();
			var path = "LoadData_Completed_SaveData.kv";
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
			database.SetDouble("doublevalue", 489.02);
			database.SetString("test1", "blablablab");
			database.SetInteger("digitvalue", 123456);
			await database.SaveData();

			await database.LoadData(path);
			var stringValue = database.GetString("test1");
			var digitValue = database.GetInteger("digitvalue");
			var doubleValue = database.GetDouble("doublevalue");

			//assert
			Assert.Equals(stringValue, "blablablab");
			Assert.Equals(digitValue, 123456);
			Assert.Equals(doubleValue, 489.02);
		}

	}

}
