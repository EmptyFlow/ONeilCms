using ONielCommon.Entities;
using ONielCommon.Storage;
using SqlKata;

namespace ONielCms.Services.DatabaseLogic {

    public class ResourceManagementService : IResourceManagementService {

        private readonly IStorageContext m_storageContext;

        public ResourceManagementService ( IStorageContext storageContext ) => m_storageContext = storageContext;

        public Task CreateResource ( string identifier, string edition, byte[] content ) {
            return m_storageContext.MakeInTransaction (
                async () => {
                    var resource = new Resource {
                        Identifier = identifier,
                    };
                    await m_storageContext.AddOrUpdate ( resource );

                    var resourceContent = new ResourceContent {
                        Content = content
                    };
                    await m_storageContext.AddOrUpdate ( resourceContent );

                    var resourceVersion = new ResourceVersion {
                        Edition = edition,
                        ResourceContentId = resourceContent.Id,
                        ResourceId = resource.Id
                    };
                    await m_storageContext.AddOrUpdate ( resourceVersion );
                }
            );
        }

        public Task CreateResourceVersion ( string identifier, string edition, byte[] content ) {
            return m_storageContext.MakeInTransaction (
                async () => {
                    var resourceId = await m_storageContext.GetSingleAsync<Guid?> (
                        new Query ()
                            .Where ( "identifier", identifier )
                            .Select ( "id" )
                            .Limit ( 1 )
                    );
                    if ( resourceId == null ) throw new Exception ( "Not found resource" );

                    var resourceContent = new ResourceContent {
                        Content = content
                    };
                    await m_storageContext.AddOrUpdate ( resourceContent );

                    var resourceVersion = new ResourceVersion {
                        Edition = edition,
                        ResourceContentId = resourceContent.Id,
                        ResourceId = resourceId.Value
                    };
                    await m_storageContext.AddOrUpdate ( resourceVersion );
                }
            );
        }

        public Task EditResourceVersion ( string identifier, string edition, byte[] content ) {
            return m_storageContext.MakeInTransaction (
                async () => {
                    var resourceContent = await m_storageContext.GetSingleAsync<ResourceContent?> (
                        new Query ()
                            .Join ( "resourceversion", "resourceversion.resourcecontentid", "resourcecontent.id" )
                            .Join ( "resource", "resourceversion.resourceid", "resource.id" )
                            .Where ( "resource.identifier", identifier )
                            .Where ( "resourceversion.edition", edition )
                            .Select ( "resourcecontent.*" )
                            .Limit ( 1 )
                    ) ?? throw new Exception ( "Not found resource content" );

                    resourceContent.Content = content;
                    await m_storageContext.AddOrUpdate ( resourceContent );
                }
            );
        }

    }

}
