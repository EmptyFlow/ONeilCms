CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE edition(
    id uuid DEFAULT uuid_generate_v4(),
    version text NOT NULL UNIQUE,
    created timestamp(6) NOT NULL DEFAULT now(),
    updated timestamp(6),
    CONSTRAINT pk_edition_id PRIMARY KEY(id)
);
CREATE TABLE route(
    id uuid DEFAULT uuid_generate_v4(),
    path text NOT NULL,
    contentType text NOT NULL,
    method text NOT NULL,
    downloadasfile bool NOT NULL DEFAULT false,
    downloadfilename text,
    CONSTRAINT pk_route_id PRIMARY KEY (id)
);
CREATE TABLE routeversion(
   id uuid DEFAULT uuid_generate_v4(),
   routeid uuid NOT NULL,
   version text,
   CONSTRAINT fk_routeversion_route FOREIGN KEY (routeid) REFERENCES route(id),
   CONSTRAINT fk_routeversion_version FOREIGN KEY (version) REFERENCES edition(version)
);

CREATE TABLE resource(
    id uuid DEFAULT uuid_generate_v4(),
    identifier text NOT NULL,
    content bytea NOT NULL,
    contenthash text NOT NULL,
    CONSTRAINT pk_textresource_id PRIMARY KEY (id)
);

CREATE TABLE resourceversion(
   id uuid DEFAULT uuid_generate_v4(),
   resourceid uuid NOT NULL,
   version text,
   CONSTRAINT fk_routeversion_resource FOREIGN KEY (resourceid) REFERENCES resource(id),
   CONSTRAINT fk_routeversion_version FOREIGN KEY (version) REFERENCES edition(version)
);

CREATE TABLE routeresource(
   id uuid DEFAULT uuid_generate_v4(),
   resourceid uuid NOT NULL,
   routeid uuid NOT NULL,
   renderOrder int4 NOT NULL,
   CONSTRAINT fk_routeversion_resource FOREIGN KEY (resourceid) REFERENCES resource(id),
   CONSTRAINT fk_routeversion_route FOREIGN KEY (routeid) REFERENCES route(id)
);