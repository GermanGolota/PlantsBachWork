CREATE OR REPLACE FUNCTION delete_only_creator_or_manager ()
  RETURNS TRIGGER
  AS $BODY$
DECLARE
  userId int;
BEGIN
  userId := get_current_user_id_throw ();
  IF NOT (
    SELECT
      'manager'::UserRoles = ANY (ur.roles) OR OLD.seller_id = ur.person_id
  FROM
    user_to_roles ur
  WHERE
    ur.person_id = userId) THEN
    RAISE EXCEPTION 'You cannot delete post you have not created';
  END IF;
  IF EXISTS (
    SELECT
      o.post_id
    FROM
      plant_order o
    WHERE
      o.post_id = OLD.plant_id) THEN
  RAISE EXCEPTION 'You cannot delete ordered post';
END IF;
  RETURN OLD;
END;
$BODY$
LANGUAGE 'plpgsql';

CREATE TRIGGER post_prevent_unlawfull_delete
  BEFORE DELETE ON plant_post
  FOR EACH ROW
  EXECUTE PROCEDURE delete_only_creator_or_manager ();

