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
SECURITY DEFINER
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

CREATE TABLE plant_to_image (
  relation_id serial PRIMARY KEY,
  plant_id int REFERENCES plant (id) NOT NULL,
  image bytea NOT NULL
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

