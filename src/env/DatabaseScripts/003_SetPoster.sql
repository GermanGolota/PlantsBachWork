--set poster
CREATE OR REPLACE FUNCTION get_current_user_id ()
  RETURNS integer
  SECURITY DEFINER
  AS $$
BEGIN
  RETURN COALESCE((
    SELECT
      p.person_id
    FROM person_to_login p
    WHERE
      p.login = SESSION_USER), -1);
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
    RAISE EXCEPTION 'There is no person attached to %', SESSION_USER
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
    RAISE EXCEPTION 'There is no person attached to %', SESSION_USER
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
    RAISE EXCEPTION 'There is no person attached to %', SESSION_USER
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

