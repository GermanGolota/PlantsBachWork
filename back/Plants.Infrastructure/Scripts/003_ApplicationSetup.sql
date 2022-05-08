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

--add create
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

--set poster
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

--instruction reject
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

