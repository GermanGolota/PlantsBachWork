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

--NEW
UPDATE
  person_to_login
SET
  login = lower(login);

ALTER TABLE person_to_login
  ADD CHECK (LOGIN = lower(LOGIN));

ALTER TABLE person_to_login
  ADD UNIQUE (LOGIN);

CREATE OR REPLACE VIEW user_to_roles AS (
  SELECT
    pl.login,
    ARRAY_REMOVE(ARRAY_AGG(parse_role (auth.roleid::regrole)), 'other'::UserRoles)
  FROM
    person_to_login pl
    JOIN pg_auth_members auth ON auth.member::regrole::name = pl.login
  GROUP BY
    pl.login);

UPDATE
  person_to_login
SET
  login = lower(login);

ALTER TABLE person_to_login
  ADD CHECK (LOGIN = lower(LOGIN));

ALTER TABLE person_to_login
  ADD UNIQUE (LOGIN);

CREATE OR REPLACE VIEW user_to_roles AS (
  SELECT
    pl.person_id,
    pl.login,
    ARRAY_REMOVE(ARRAY_AGG(parse_role (auth.roleid::regrole)), 'other'::UserRoles) AS roles
  FROM
    person_to_login pl
    JOIN pg_auth_members auth ON auth.member::regrole::name = pl.login
  GROUP BY
    pl.login,
    pl.person_id);

CREATE OR REPLACE FUNCTION current_user_can_create_all (userRoles UserRoles[])
  RETURNS boolean
  AS $$
DECLARE
  currentRole UserRoles;
  canCreate boolean;
BEGIN
  canCreate := TRUE;
  FOREACH currentRole IN ARRAY userRoles LOOP
    canCreate := canCreate
      AND current_user_can_create_role (currentRole);
  END LOOP;
  RETURN canCreate;
END
$$
LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION search_users (userName text, mobileNumber text, userRoles UserRoles[])
  RETURNS TABLE (
    full_name text,
    mobile text,
    roles UserRoles[],
    LOGIN text
  )
  AS $$
BEGIN
  RETURN QUERY (
    SELECT
      p.first_name || ' ' || p.last_name AS full_name, p.phone_number, ur.roles, ur.login::text
    FROM user_to_roles ur
    JOIN person p ON ur.person_id = p.id
    WHERE
      current_user_can_create_all (ur.roles)
    AND (userName IS NULL
      OR to_tsvector(p.first_name || ' ' || p.last_name) @@ to_tsquery(userName))
    AND (mobileNumber IS NULL
      OR to_tsvector(p.phone_number) @@ to_tsquery(mobileNumber))
    AND (userRoles IS NULL
      OR userRoles && ur.roles));
END
$$
LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE remove_user_from_group (userName text, userRole UserRoles)
  AS $$
BEGIN
  IF current_user_can_create_role (userRole) THEN
    IF (
      SELECT
        ARRAY_LENGTH(roles, 1)
      FROM
        user_to_roles
      WHERE
        login = userName) > 1 THEN
      EXECUTE FORMAT('ALTER GROUP %s DROP USER %s', userRole, userName);
    ELSE
      RAISE EXCEPTION 'You cannot remove last role of the user%', userRole::text;
    END IF;
  ELSE
    RAISE EXCEPTION 'You cannot create role %', userRole::text
      USING HINT = 'Yours role priority is lower than the priority of this role';
    END IF;
END;
$$
LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE create_user (username name, userPass text, userRoles UserRoles[], firstName text, lastName text, phoneNumber text)
  AS $$
DECLARE
  userId int;
BEGIN
  username := lower(username);
  CALL create_user_login (username, userPass, userRoles);
  INSERT INTO person (first_name, last_name, phone_number)
    VALUES (firstName, lastName, phoneNumber)
  RETURNING
    id INTO userId;
  INSERT INTO person_to_login (person_id, login)
    VALUES (userId, username);
END;
$$
LANGUAGE plpgsql;

