--creating tables
CREATE TABLE plant_group (
    id serial PRIMARY KEY,
    group_name text
);

CREATE TABLE plant_region (
    id serial PRIMARY KEY,
    region_name text
);

CREATE TABLE plant_soil (
    id serial PRIMARY KEY,
    soil_name text
);

CREATE TABLE delivery_address (
    id serial PRIMARY KEY,
    city text,
    nova_poshta_number smallint,
    UNIQUE (city, nova_poshta_number)
);

CREATE TABLE person (
    id serial PRIMARY KEY,
    first_name text NOT NULL,
    last_name text NOT NULL,
    phone_number text NOT NULL,
    delivery_address_id int REFERENCES delivery_address (id)
);

CREATE TABLE plant (
    id serial PRIMARY KEY,
    group_id int NOT NULL REFERENCES plant_group (id),
    soil_id int NOT NULL REFERENCES plant_soil (id),
    region_id int NOT NULL REFERENCES plant_region (id),
    care_taker_id int NOT NULL REFERENCES person (id),
    plant_name text NOT NULL,
    description text NOT NULL,
    created date NOT NULL
);

CREATE TABLE plant_caring_instruction (
    id serial PRIMARY KEY,
    instruction_text text,
    posted_by_id int NOT NULL REFERENCES person (id),
    plant_group_id int NOT NULL REFERENCES plant_group (id),
    title text NOT NULL,
    description text NOT NULL
);

CREATE TABLE plant_post (
    plant_id serial PRIMARY KEY REFERENCES plant (id),
    seller_id int NOT NULL REFERENCES person (id),
    price decimal NOT NULL CHECK (price >= 0),
    created date NOT NULL DEFAULT CURRENT_DATE
);

CREATE TABLE plant_order (
    post_id int PRIMARY KEY REFERENCES plant_post (plant_id),
    customer_id int NOT NULL REFERENCES person (id),
    delivery_address_id int REFERENCES delivery_address (id),
    created timestamptz DEFAULT now() NOT NULL
);

CREATE TABLE plant_delivery (
    order_id int PRIMARY KEY REFERENCES plant_order (post_id),
    delivery_tracking_number text NOT NULL,
    created timestamptz DEFAULT now() NOT NULL
);

CREATE TABLE plant_shipment (
    delivery_id int PRIMARY KEY REFERENCES plant_delivery (order_id),
    shipped timestamptz DEFAULT now() NOT NULL
);

CREATE TABLE plant_to_image (
    relation_id serial PRIMARY KEY,
    plant_id int REFERENCES plant (id) NOT NULL,
    image bytea NOT NULL
);

CREATE TABLE person_to_delivery (
    id serial PRIMARY KEY,
    person_id int NOT NULL REFERENCES person (id),
    delivery_address_id int NOT NULL REFERENCES delivery_address (id) ON DELETE CASCADE
);

CREATE TABLE instruction_to_cover (
    instruction_id serial PRIMARY KEY REFERENCES plant_caring_instruction (id) ON DELETE CASCADE,
    image bytea NOT NULL
);

--adding logins
CREATE TABLE person_to_login (
    person_id int PRIMARY KEY REFERENCES person (id) ON DELETE CASCADE,
    login name UNIQUE CHECK (LOGIN = lower(LOGIN))
);

CREATE GROUP consumer;

CREATE GROUP producer;

CREATE GROUP manager;

CREATE TYPE UserRoles AS ENUM (
    'consumer',
    'producer',
    'manager',
    'other'
);

CREATE OR REPLACE PROCEDURE create_user_login (username name, userPass text, userRoles UserRoles[])
SECURITY DEFINER
AS $$
BEGIN
    EXECUTE FORMAT('CREATE USER %s WITH PASSWORD %L in group %s', username, userPass, array_to_string(userRoles, ', '));
END;
$$
LANGUAGE plpgsql;

--Creating roles for persons
DO $$
DECLARE
    person record;
    currentLogin name;
BEGIN
    FOR person IN (
        SELECT
            *
        FROM
            person)
        LOOP
            currentLogin := person.first_name || person.last_name || person.id;
            CALL create_user_login (currentLogin, 'tempPass', ARRAY['producer'::UserRoles, 'consumer'::UserRoles]);
            INSERT INTO person_to_login
                VALUES (person.id, currentLogin);
        END LOOP;
END
$$;

CREATE OR REPLACE FUNCTION person_check_login ()
    RETURNS TRIGGER
    SECURITY DEFINER
    AS $BODY$
BEGIN
    IF NOT EXISTS (
        SELECT
            1
        FROM
            pg_user
        WHERE
            usename = NEW.login) THEN
    RAISE EXCEPTION 'There is no login with id %', NEW.login
        USING HINT = 'Please, consider creating person through specified sp';
    END IF;
    RETURN NEW;
END;
$BODY$
LANGUAGE 'plpgsql';

CREATE TRIGGER person_prevent_bad_login
    BEFORE INSERT OR UPDATE ON person_to_login
    FOR EACH ROW
    EXECUTE PROCEDURE person_check_login ();

-- Configuring root user
INSERT INTO person (id, first_name, last_name, phone_number)
    VALUES (0, 'Admin', 'Admin', '0503035050');

INSERT INTO person_to_login (person_id, login)
    VALUES (0, 'postgres');

ALTER
GROUP manager
    ADD USER postgres;

