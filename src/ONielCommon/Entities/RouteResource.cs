using ONielCommon.Storage;

namespace ONielCommon.Entities {

    /// <summary>
    /// Glue for combine route and resources.
    /// </summary>
    [TableName ( "routeresource" )]
    public record RouteResource {

        public Guid Id { get; init; }

        public Guid ResourceId { get; set; }

        public Guid RouteId { get; set; }

        public int RenderOrder { get; set; }

    }

}
