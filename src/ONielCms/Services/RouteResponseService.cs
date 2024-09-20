using ONielCommon.Storage;
using ONielCommon.Storage.EntityServices;
using SqlKata;

namespace ONielCms.Services {

    public class RouteResponseService : IRouteResponseService {

        private readonly IStorageContext m_storageContext;

        public RouteResponseService ( IStorageContext storageContext ) => m_storageContext = storageContext ?? throw new ArgumentNullException ( nameof ( storageContext ) );

        public async Task<byte[]> GetRouteResponse ( Guid id ) {
            var currentVersion = "";
            var fallbackVersion = "";
            var editions = new List<string> { currentVersion, fallbackVersion };

            /*
             SELECT
	resourceversion.resourcecontentid,
	resourceversion.edition
FROM
	resourceversion
	INNER JOIN routeresource ON routeresource.resourceid = routeresource.resourceid 
WHERE resourceversion.edition IN ('1', '2')	
ORDER BY
	routeresource.renderorder
             */

            var contentVersions = ( await m_storageContext
                .GetAsync<ContentVersion> (
                    new Query ( "resourceversion" )
                        .Join ( "routeresource", "resourceversion.resourceid", "routeresource.resourceid" )
                        .Join ( "resource", "resourceversion.resourceid", "resource.resourceid" )
                        .WhereIn ( "resourceversion.edition", editions )
                        .OrderBy ( "routeresource.renderorder" )
                        .Select ( "resourceversion.resourcecontentid", "resourceversion.edition", "resource.identifier", "routeresource.renderorder" )
                )
            ).ToLookup ( a => a.Identifier );

            var contentIdentifiers = contentVersions
                .Where (
                    a => a.Any ( b => editions.Contains ( b.Edition ) )
                )
                .Select (
                    a => {
                        var currentVersionIdentifier = a.FirstOrDefault ( a => a.Edition == currentVersion );
                        if ( currentVersionIdentifier != null ) {
                            return new ContentIdentifier {
                                Id = currentVersionIdentifier.ResourceContentId,
                                Order = currentVersionIdentifier.RenderOrder
                            };
                        }

                        var fallbackVersionIdentifier = a.First ( a => a.Edition == fallbackVersion );

                        return new ContentIdentifier {
                            Id = fallbackVersionIdentifier.ResourceContentId,
                            Order = fallbackVersionIdentifier.RenderOrder
                        };
                    }
                )
                .ToArray ();

            //if no content for response return empty array
            if ( !contentIdentifiers.Any () ) return [];

            var contents = ( await m_storageContext
                .GetAsync<SectionContent> (
                    new Query ( "resourcecontent" )
                        .WhereIn ( "id", contentIdentifiers.Select(a => a.Id) )
                        .Select ( "id", "content" )
                )
            ).ToDictionary(a => a.Id);

            byte[] result = [];
            foreach ( var contentIdentifier in contentIdentifiers ) {
                var content = contents[contentIdentifier.Id].Content;
                Array.Copy ( content, 0, result, result.Length, content.Length );
            }

            return result;
        }

        private record ContentVersion {

            public Guid ResourceContentId { get; init; }

            public string Edition { get; init; } = "";

            public string Identifier { get; init; } = "";

            public int RenderOrder { get; init; }

        }

        private record ContentIdentifier {

            public Guid Id { get; init; }

            public int Order { get; init; }

        }

        private record SectionContent {
            public Guid Id { get; set; }
            public byte[] Content { get; init; } = [];
        }

    }

}
