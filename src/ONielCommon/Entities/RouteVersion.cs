using ONielCommon.Storage;

namespace ONielCommon.Entities {

    [TableName ( "routeversion" )]
    public class RouteVersion {

        public Guid Id { get; init; }

        public Guid RouteId { get; set; }

        public string Version { get; set; } = "";

    }

}
