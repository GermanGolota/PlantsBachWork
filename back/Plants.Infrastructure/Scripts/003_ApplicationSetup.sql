--Lab 4
CREATE OR REPLACE VIEW plant_stats_v AS (
  WITH gToInstruction AS (
    SELECT
      plant_group_id AS gid,
      Count(*) AS cnt
    FROM
      plant_caring_instruction
    GROUP BY
      plant_group_id),
    gToPlants AS (
      SELECT
        group_id AS gid,
        Count(*) AS cnt
      FROM
        plant
      GROUP BY
        group_id),
      gToIncome AS (
        SELECT
          p.group_id AS gid,
          SUM(price) AS total
        FROM
          plant_shipment s
          JOIN plant_order o ON o.post_id = s.order_id
          JOIN plant_post po ON po.plant_id = o.post_id
          JOIN plant p ON p.id = po.plant_id
        GROUP BY
          group_id),
        gToPopularity AS (
          SELECT
            p.group_id AS gid,
            COUNT(*) AS total
          FROM
            plant_order o
            JOIN plant_post po ON po.plant_id = o.post_id
            JOIN plant p ON p.id = po.plant_id
          GROUP BY
            group_id
)
          SELECT
            g.id,
            g.group_name,
            p.cnt AS plants_count,
            p2.total AS popularity,
            i.total AS income,
            i2.cnt AS instructions
          FROM
            gToPlants p
            JOIN gToPopularity p2 USING (gid)
            JOIN gToIncome i USING (gid)
            JOIN gToInstruction i2 USING (gid)
            JOIN plant_group g ON g.id = (gid));

--Part of Lab5
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
  AS $$
BEGIN
  EXECUTE FORMAT('CREATE USER %s WITH PASSWORD %L in group %s', username, userPass, array_to_string(userRoles, ', '));
END;
$$
LANGUAGE plpgsql;

--Creating roles for persons
CREATE TABLE person_to_login (
  person_id int PRIMARY KEY REFERENCES person (id) ON DELETE CASCADE,
  login name UNIQUE
);

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

--New
CREATE OR REPLACE FUNCTION parse_role (roleName regrole)
  RETURNS UserRoles
  AS $$
BEGIN
  RETURN roleName::text::UserRoles;
EXCEPTION
  WHEN OTHERS THEN
    RETURN 'other'::UserRoles;
END
$$
LANGUAGE plpgsql;

CREATE OR REPLACE VIEW current_user_roles AS (
  SELECT DISTINCT
    parse_role (auth.roleid::regrole) AS roleName
  FROM
    pg_auth_members auth
  WHERE
    auth.member::regrole = CURRENT_USER::regrole
  EXCEPT (
    SELECT
      'other'::userroles));

GRANT SELECT ON current_user_roles TO consumer, producer, manager;

CREATE OR REPLACE PROCEDURE create_user (LOGIN name, userPass text, userRoles UserRoles[], firstName text, lastName text, phoneNumber text)
  AS $$
DECLARE
  userId int;
BEGIN
  login := lower(login);
  CALL create_user_login (username, userPass, userRoles);
  INSERT INTO person (first_name, last_name, phone_number)
    VALUES (firstName, lastName, phoneNumber)
  RETURNING
    personId;
  INSERT INTO person_to_login (person_id, login)
    VALUES (person_id, login);
END;
$$
LANGUAGE plpgsql;

CREATE TABLE plant_to_image (
  relation_id serial PRIMARY KEY,
  plant_id int REFERENCES plant (id),
  image bytea
);

ALTER TABLE plant_post
  ADD COLUMN created date;

UPDATE
  plant_post
SET
  created = CURRENT_DATE
WHERE
  created IS NULL;

ALTER TABLE plant_post
  ALTER COLUMN created SET NOT NULL;

CREATE OR REPLACE FUNCTION get_current_user_id ()
  RETURNS integer
  AS $$
BEGIN
  RETURN COALESCE((
    SELECT
      p.person_id
    FROM person_to_login p
    WHERE
      p.login = CURRENT_USER), -1);
END
$$
LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION set_current_user_id_care_taker ()
  RETURNS TRIGGER
  AS $BODY$
DECLARE
  userId int;
BEGIN
  userId := get_current_user_id ();
  IF userId = - 1 THEN
    RAISE EXCEPTION 'There is no person attached to %', CURRENT_USER
      USING HINT = 'Please, consider using credentials that have a person attached to them';
    ELSE
      NEW.care_taker_id = userId;
    END IF;
    RETURN NEW;
END;
$BODY$
LANGUAGE 'plpgsql';

CREATE TRIGGER plant_set_poster
  BEFORE INSERT ON plant
  FOR EACH ROW
  EXECUTE PROCEDURE set_current_user_id_care_taker ();

CREATE OR REPLACE FUNCTION set_current_user_id_seller ()
  RETURNS TRIGGER
  AS $BODY$
DECLARE
  userId int;
BEGIN
  userId := get_current_user_id ();
  IF userId = - 1 THEN
    RAISE EXCEPTION 'There is no person attached to %', CURRENT_USER
      USING HINT = 'Please, consider using credentials that have a person attached to them';
    ELSE
      NEW.seller_id = userId;
    END IF;
    RETURN NEW;
END;
$BODY$
LANGUAGE 'plpgsql';

CREATE TRIGGER post_set_poster
  BEFORE INSERT ON plant_post
  FOR EACH ROW
  EXECUTE PROCEDURE set_current_user_id_seller ();

CREATE OR REPLACE FUNCTION set_current_user_id_instruction ()
  RETURNS TRIGGER
  AS $BODY$
DECLARE
  userId int;
BEGIN
  userId := get_current_user_id ();
  IF userId = - 1 THEN
    RAISE EXCEPTION 'There is no person attached to %', CURRENT_USER
      USING HINT = 'Please, consider using credentials that have a person attached to them';
    ELSE
      NEW.posted_by_id = userId;
    END IF;
    RETURN NEW;
END;
$BODY$
LANGUAGE 'plpgsql';

CREATE TRIGGER instruction_set_poster
  BEFORE INSERT ON plant_caring_instruction
  FOR EACH ROW
  EXECUTE PROCEDURE set_current_user_id_instruction ();

INSERT INTO person (id, first_name, last_name, phone_number)
  VALUES (0, 'Admin', 'Admin', '0503035050');

INSERT INTO person_to_login (person_id, login)
  VALUES (0, 'postgres');

ALTER
GROUP manager
  ADD USER postgres;

CREATE TYPE plant_post_model AS (
  id integer,
  plant_name text,
  price numeric,
  group_name text,
  soil_name text,
  description text,
  regions text[],
  seller_name text,
  seller_phone text,
  caretaker_experience bigint
);

CREATE OR REPLACE VIEW plant_post_v AS (
  WITH caretaker_to_plant_count AS (
    SELECT
      p.id,
      count(*) AS pcount
    FROM
      person p
      JOIN plant pl ON pl.care_taker_id = p.id
    GROUP BY
      p.id),
    posts_extended AS (
      SELECT
        p.id,
        p.plant_name,
        po.price,
        gr.group_name,
        s.soil_name,
        p.description,
        po.seller_id,
        p.care_taker_id,
        array_agg(DISTINCT rg.region_name) AS regions
      FROM
        plant_post po
        JOIN plant p ON p.id = po.plant_id
        JOIN plant_group gr ON gr.id = p.group_id
        JOIN plant_soil s ON s.id = p.soil_id
        JOIN plant_to_region prg ON prg.plant_id = p.id
        JOIN plant_region rg ON rg.id = prg.plant_region_id
      GROUP BY
        p.id,
        gr.group_name,
        s.soil_name,
        po.price,
        po.seller_id,
        p.care_taker_id
)
      SELECT
        post.id,
        post.plant_name,
        post.price,
        post.group_name,
        post.soil_name,
        post.description,
        post.regions,
        FORMAT('%s %s', seller.first_name, seller.last_name) AS seller_name,
        seller.phone_number AS seller_phone,
        pc.pcount AS caretaker_experience
      FROM
        posts_extended post
        JOIN caretaker_to_plant_count pc ON pc.id = post.care_taker_id
        JOIN person seller ON seller.id = post.seller_id);

CREATE OR REPLACE FUNCTION get_post (plantId integer)
  RETURNS plant_post_model
  AS $$
DECLARE
  res plant_post_model;
BEGIN
  SELECT
    *
  FROM
    plant_post_v post
  WHERE
    id = plantId INTO res;
  RETURN res;
END;
$$
LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION reject_instruction_no_soils ()
  RETURNS TRIGGER
  AS $$
DECLARE
  typicalSoils text[];
  soilQ tsquery;
  textV tsvector;
BEGIN
  typicalSoils := (
    SELECT
      array_agg(DISTINCT s.soil_name)
    FROM
      plant p
      JOIN plant_soil s ON p.soil_id = s.id
    WHERE
      p.group_id = NEW.plant_group_id);
  soilQ := to_tsquery(array_to_string(typicalSoils, ' | '));
  textV := to_tsvector(NEW.instruction_text);
  IF NOT (textV @@ soilQ) THEN
    RAISE EXCEPTION 'Instruction text must contain soil names that are typical for tihs plants group'
      USING HINT = 'Such soils include ' || soilQ::text;
    END IF;
    RETURN NEW;
END;
$$
LANGUAGE 'plpgsql';

CREATE TRIGGER plant_instruction_reject_no_soils
  BEFORE INSERT OR UPDATE ON plant_caring_instruction
  FOR EACH ROW
  EXECUTE PROCEDURE reject_instruction_no_soils ();

CREATE OR REPLACE FUNCTION get_financial (start_date timestamp without time zone, end_date timestamp without time zone)
  RETURNS TABLE (
    groupId int,
    group_name text,
    sold_count bigint,
    percent_sold numeric,
    income numeric
  )
  AS $$
BEGIN
  RETURN QUERY ( WITH group_to_post_count AS (
      SELECT
        p.group_id, count(*) AS total FROM plant p
      JOIN plant_post pl ON pl.plant_id = p.id
      LEFT JOIN plant_shipment s ON s.order_id = p.id
      WHERE
        s.order_id IS NULL
        OR s.shipped BETWEEN start_date AND end_date GROUP BY p.group_id)
    SELECT
      g.id, g.group_name, count(*) AS sold_count, round((count(*) * 1.0 / pc.total) * 100) AS percent_sold, sum(p.price) AS income FROM plant pl
    JOIN plant_post p ON p.plant_id = pl.id
    JOIN plant_order o ON o.post_id = p.plant_id
    JOIN plant_shipment s ON s.order_id = o.post_id
    JOIN plant_group g ON g.id = pl.group_id
    JOIN group_to_post_count pc ON pc.group_id = g.id
    WHERE
      s.shipped BETWEEN start_date AND end_date GROUP BY g.id, pc.total);
END
$$
LANGUAGE plpgsql;

--Add user to group
CREATE OR REPLACE FUNCTION get_role_priority (userRole UserRoles)
  RETURNS integer
  AS $$
DECLARE
  resultNumber int;
BEGIN
  IF userRole = 'consumer' THEN
    resultNumber = 1;
  ELSIF userRole = 'producer' THEN
    resultNumber = 2;
  ELSIF userRole = 'manager' THEN
    resultNumber = 3;
  ELSE
    RAISE EXCEPTION 'There is no priority for this group';
  END IF;
  RETURN resultNumber;
END
$$
LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION current_user_can_create_role (userRole UserRoles)
  RETURNS boolean
  AS $$
BEGIN
  RETURN (
    SELECT
      coalesce(MAX(get_role_priority (rolename)), -1) >= get_role_priority (userRole)
    FROM
      current_user_roles);
END;
$$
LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE add_user_to_group (userName text, userRole UserRoles)
  AS $$
BEGIN
  IF current_user_can_create_role (userRole) THEN
    EXECUTE FORMAT('ALTER GROUP %s ADD USER %s', userRole, userName);
  ELSE
    RAISE EXCEPTION 'You cannot create role %', userRole::text
      USING HINT = 'Yours role priority is lower than the priority of this role';
    END IF;
END;
$$
LANGUAGE plpgsql;

GRANT EXECUTE ON PROCEDURE add_user_to_group TO consumer, producer, manager;

