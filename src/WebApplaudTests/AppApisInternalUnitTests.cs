using FakeItEasy;
using Microsoft.AspNetCore.Routing;
using WebApplaud.ApplicationApis;
using WebApplaud.Databases.KeyValue;
using WebApplaud.FileSystems;

namespace WebApplaudTests {

	public class AppApisInternalUnitTests {

		[Test]
		public async Task Initialize_AppFolder_Completed() {
			//arrange
			var fileSystem = A.Fake<IInternalFileSystem>();
			var kvDatabase = A.Fake<IKeyValueDatabase>();
			var waAppHost = A.Fake<IEndpointRouteBuilder>();
			var appInternal = new AppApisInternal(fileSystem, kvDatabase);

			A.CallTo(() => fileSystem.GetDirectories("lala")).Returns([]);

			//act
			await appInternal.Initialize("lala", waAppHost);

			//assert
			Assert.Equals("lala", appInternal.AppFolder);
			Assert.Equals(0, appInternal.Apps.Count());
		}

	}

}
