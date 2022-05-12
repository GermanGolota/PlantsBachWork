CREATE OR REPLACE FUNCTION delete_post (postId int)
  RETURNS integer
  AS $$
DECLARE
  deletedId int;
BEGIN
  RETURN delete_post_as (postId, get_current_user_id_throw ())
END
$$
LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION delete_post_as (postId int, userId int)
  RETURNS integer
  SECURITY DEFINER
  AS $$
DECLARE
  deletedId int;
BEGIN
  DELETE p FROM plant_post p
  JOIN user_to_roles ur ON ur.person_id = userId
  LEFT JOIN plant_order o ON o.post_id = p.plant_id
  WHERE p.plant_id = postId
    AND ('manager'::UserRoles = ANY (ur.roles)
      OR p.seller_id = ur.person_id)
    AND o.post_id IS NULL
  RETURNING
    id INTO deletedId;
END
$$
LANGUAGE plpgsql;

