CREATE OR REPLACE FUNCTION delete_only_creator_or_manager ()
  RETURNS TRIGGER
  SECURITY DEFINER
  AS $BODY$
DECLARE
  isOrder boolean;
  userId int;
  posterId int;
BEGIN
  isOrder := TG_ARGV[0] = 'order';
  IF isOrder THEN
    posterId := (
      SELECT
        seller_id
      FROM
        plant_post
      WHERE
        plant_id = OLD.post_id);
  ELSE
    posterId := OLD.seller_id;
  END IF;
  userId := get_current_user_id_throw ();
  IF NOT (
    SELECT
      'manager'::UserRoles = ANY (ur.roles) OR posterId = ur.person_id
  FROM
    user_to_roles ur
  WHERE
    ur.person_id = userId) THEN
    RAISE EXCEPTION 'You cannot delete post you have not created';
  END IF;
  RETURN OLD;
END;
$BODY$
LANGUAGE 'plpgsql';

CREATE TRIGGER post_prevent_unlawfull_delete
  BEFORE DELETE ON plant_post
  FOR EACH ROW
  EXECUTE PROCEDURE delete_only_creator_or_manager ('post');

CREATE TRIGGER order_prevent_unlawfull_delete
  BEFORE DELETE ON plant_order
  FOR EACH ROW
  EXECUTE PROCEDURE delete_only_creator_or_manager ('order');

