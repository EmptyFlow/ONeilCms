using System.ComponentModel;

namespace ONielCommon.Migrations {

    [Description ( "Create tables: route and edition" )]
    public class InitialMigration {

        public static int MigrationNumber => 1;

        public static string Issue => "https://github.com/EmptyFlow/PostgreSQL.Migrations/issues/1";

        public string Down () {
            return """
DROP TABLE resourcecontent;
DROP TABLE resourceversion;
DROP TABLE resource;

DROP TABLE binaryresourcecontent;
DROP TABLE binaryresourceversion;
DROP TABLE binaryresource;

DROP TABLE route;
DROP TABLE edition;
""";
        }

        public string Up () => """
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE edition(
    id uuid DEFAULT uuid_generate_v4(),
    name text NOT NULL UNIQUE,
    CONSTRAINT pk_edition_id PRIMARY KEY
);
CREATE TABLE route(
    id uuid DEFAULT uuid_generate_v4(),
    path text NOT NULL,
    edition text NOT NULL,
    CONSTRAINT pk_route_id PRIMARY KEY,
    CONSTRAINT fk_route_edition FOREIGN KEY (edition) REFERENCES edition(name)
);

CREATE TABLE resource(
    id uuid DEFAULT uuid_generate_v4(),
    identifier text NOT NULL UNIQUE,
    CONSTRAINT pk_textresource_id PRIMARY KEY
);
CREATE TABLE resourceversion(
    id uuid DEFAULT uuid_generate_v4(),
    edition text NOT NULL,
    resourcecontentid NOT NULL,
    CONSTRAINT pk_resourceversion_id PRIMARY KEY,
    CONSTRAINT fk_resourceversion_resourcecontent FOREIGN KEY (resourcecontentid) REFERENCES resourcecontent(id)
);
CREATE TABLE resourcecontent(
    id uuid DEFAULT uuid_generate_v4(),
    content text NOT NULL,
    CONSTRAINT pk_binaryresource_id PRIMARY KEY
);

CREATE TABLE binaryresource(
    id uuid DEFAULT uuid_generate_v4(),
    identifier text NOT NULL UNIQUE,
    CONSTRAINT pk_binaryresource_id PRIMARY KEY
);
CREATE TABLE binaryresourceversion(
    id uuid DEFAULT uuid_generate_v4(),
    edition text NOT NULL,
    binaryresourcecontentid NOT NULL,
    CONSTRAINT pk_binaryresourceversion_id PRIMARY KEY,
    CONSTRAINT fk_binaryresourceversion_binaryresourcecontent FOREIGN KEY (binaryresourcecontentid) REFERENCES binaryresourcecontent(id)
);
CREATE TABLE binaryresourcecontent(
    id uuid DEFAULT uuid_generate_v4(),
    content bytea NOT NULL,
    CONSTRAINT pk_binaryresourcecontent_id PRIMARY KEY
);
""";

    }

}
