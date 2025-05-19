using FakeItEasy;
using ONielCms.Services.DatabaseLogic;
using ONielCommon.Entities;
using ONielCommon.Storage;
using SqlKata;
using System.Text;

namespace ONeilCms.Tests {

    public class ResourceManagementServiceIntegrationTests {

        private string TestingConnectionString = "Username=postgres;Password=postgres;Host=localhost;Port=5432;Database=onielcms;Connection Lifetime=0";

        [Fact, Trait ( "Category", "Integration" )]
        public async Task CreateResource_Completed_Case1 () {
            //arrange
            var fakeLogger = A.Fake<IStorageLogger> ();
            var fakeConfiguration = A.Fake<IConfigurationService> ();

            A.CallTo ( () => fakeLogger.LogInformation ( A<string>._ ) ).Invokes ( a => Console.WriteLine ( a.GetArgument<string> ( 0 ) ) );
            A.CallTo ( () => fakeConfiguration.DatabaseConnectionString () ).Returns ( TestingConnectionString );

            var storageContext = new StorageContext ( fakeLogger, fakeConfiguration );
            var service = new ResourceManagementService ( storageContext );

            //delete the resource if it remains after the previous test run
            var oldResource = await storageContext.GetSingleAsync<Resource> ( Kata.Set.Where ( "identifier", "lib.js" ) );
            if ( oldResource != null ) {
                var oldResourceVersions = await storageContext.GetAsync<ResourceVersion> ( Kata.Set.Where ( $"{nameof ( ResourceVersion.ResourceId )}", oldResource.Id ) );

                var contentIds = oldResourceVersions
                    .Select ( a => a.ResourceContentId )
                    .ToArray ();

                await storageContext.MakeNoResult<ResourceVersion> ( Kata.Set.Where ( $"{nameof ( ResourceVersion.ResourceId )}", oldResource.Id ).AsDelete () );
                await storageContext.MakeNoResult<ResourceContent> ( Kata.Set.WhereIn ( "id", contentIds ).AsDelete () );
                await storageContext.MakeNoResult<Resource> ( Kata.Set.Where ( "id", oldResource.Id ).AsDelete () );
            }

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