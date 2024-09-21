using System.ComponentModel;

namespace ONielCommon.Migrations {

    [Description ( "Create tables: route and edition" )]
    public class InitialMigration {

        public static int MigrationNumber => 1;

        public static string Issue => "https://github.com/EmptyFlow/ONeilCms/issues/1";

        public string Down () {
            return """
DROP TABLE routeresource;

DROP TABLE resourceversion;
DROP TABLE resourcecontent;
DROP TABLE resource;

DROP TABLE route;
DROP TABLE edition;
""";
        }

        public string Up () => """
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE edition(
    id uuid DEFAULT uuid_generate_v4(),
    name text NOT NULL UNIQUE,
    CONSTRAINT pk_edition_id PRIMARY KEY(id)
);
CREATE TABLE route(
    id uuid DEFAULT uuid_generate_v4(),
    path text NOT NULL,
    contentType text NOT NULL,
    CONSTRAINT pk_route_id PRIMARY KEY (id)
);

CREATE TABLE resource(
    id uuid DEFAULT uuid_generate_v4(),
    identifier text NOT NULL UNIQUE,
    CONSTRAINT pk_textresource_id PRIMARY KEY (id)
);
CREATE TABLE resourcecontent(
    id uuid DEFAULT uuid_generate_v4(),
    content bytea NOT NULL,
    CONSTRAINT pk_resourcecontent_id PRIMARY KEY (id)
);
CREATE TABLE resourceversion(
    id uuid DEFAULT uuid_generate_v4(),
    edition text NOT NULL,
    resourceid uuid NOT NULL,
    resourcecontentid uuid NOT NULL,
    CONSTRAINT pk_resourceversion_id PRIMARY KEY (id),
    CONSTRAINT fk_resourceversion_resourcecontent FOREIGN KEY (resourcecontentid) REFERENCES resourcecontent(id),
    CONSTRAINT fk_resourceversion_resource FOREIGN KEY (resourceid) REFERENCES resource(id)
);

CREATE TABLE routeresource(
    routeid uuid NOT NULL,
    resourceid uuid NOT NULL,
    renderorder int,
    CONSTRAINT fk_routeresource_route FOREIGN KEY (routeid) REFERENCES route(id),
    CONSTRAINT fk_routeresource_resource FOREIGN KEY (resourceid) REFERENCES resource(id)
);
""";

    }

}
