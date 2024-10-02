using FakeItEasy;
using ONielCms.Services;
using ONielCommon.Entities;
using ONielCommon.Storage;
using SqlKata;
using System.Text;

namespace ONeilCms.Tests {
    public class ResourceManagementServiceIntegrationTests {

        private string TestingConnectionString = "Username=postgres;Password=postgres;Host=localhost;Port=5432;Database=onielcms";

        [Fact, Trait ( "Category", "Integration" )]
        public async Task CreateResource_Completed_Case1 () {
            //arrange
            var fakeLogger = A.Fake<IStorageLogger> ();
            var fakeConfiguration = A.Fake<IConfigurationService> ();

            A.CallTo ( () => fakeConfiguration.DatabaseConnectionString () ).Returns ( TestingConnectionString );

            var storageContext = new StorageContext ( fakeLogger, fakeConfiguration );
            var service = new ResourceManagementService ( storageContext );

            //act
            var libContent = "function libjs(){ return '';}";
            await service.CreateResource ( "lib.js", "1.8", Encoding.UTF8.GetBytes ( libContent ) );

            //assert
            var resource = await storageContext.GetSingleAsync<Resource> ( new Query ().Where ( "identifier", "lib.js" ) );
            Assert.NotNull ( resource );
            var resourceVersion = await storageContext.GetSingleAsync<ResourceVersion> ( new Query ().Where ( "resourceid", resource.Id ).Where ( "edition", "1.8" ) );
            Assert.NotNull ( resourceVersion );
            var resourceContent = await storageContext.GetSingleAsync<ResourceContent> ( new Query ().Where ( "id", resourceVersion.ResourceContentId ) );
            Assert.NotNull ( resourceContent );
            Assert.Equal ( "function libjs(){ return '';}", Encoding.UTF8.GetString ( resourceContent.Content ) );
        }

    }

}