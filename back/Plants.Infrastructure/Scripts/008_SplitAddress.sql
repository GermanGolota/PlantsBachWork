CREATE OR REPLACE VIEW person_addresses_v AS (
  SELECT
    p.id,
    array_agg(d.city) AS cities,
    array_agg(d.nova_poshta_number) AS posts
  FROM
    person_to_delivery pd
    JOIN delivery_address d ON d.id = pd.delivery_address_id
    JOIN person p ON p.id = pd.person_id
  GROUP BY
    p.id);

--Reason Code:
-- 0 - all good
-- 1 - plant not posted
-- 2 - already ordered
CREATE OR REPLACE FUNCTION place_order (IN postId int, delivery_city text, post_number integer, OUT wasPlaced boolean, OUT reasonCode integer)
AS $$
DECLARE
  userId int;
  postExists boolean;
  orderExists boolean;
  addressId int;
BEGIN
  CREATE TEMP TABLE IF NOT EXISTS order_results AS
  SELECT
    p.plant_id AS post_id,
    o.post_id AS order_id
  FROM
    plant_post p
  LEFT JOIN plant_order o ON p.plant_id = o.post_id
WHERE
  p.plant_id = postId
LIMIT 1;
  postExists := EXISTS (
    SELECT
      post_id
    FROM
      order_results);
  orderExists := (
    SELECT
      order_id
    FROM
      order_results) IS NOT NULL;
  IF postExists THEN
    IF orderExists THEN
      wasPlaced := FALSE;
      reasonCode := 2;
    ELSE
      userId := get_current_user_id_throw ();
      addressId := (
        SELECT
          id
        FROM
          delivery_address
        WHERE
          nova_poshta_number = post_number
          AND city = delivery_city);
      IF addressId IS NULL THEN
        INSERT INTO delivery_address (city, nova_poshta_number)
          VALUES (delivery_city, post_number)
        RETURNING
          id INTO addressId;
      END IF;
      INSERT INTO plant_order (delivery_address_id, post_id)
        VALUES (addressId, postId);
      wasPlaced := TRUE;
      reasonCode := 0;
    END IF;
  ELSE
    wasPlaced := FALSE;
    reasonCode := 1;
  END IF;
  DROP TABLE order_results;
END;
$$
LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION order_store_user_address ()
  RETURNS TRIGGER
  AS $BODY$
DECLARE
  userId int;
BEGIN
  userId := get_current_user_id_throw ();
  IF NOT EXISTS (
    SELECT
      delivery_address_id
    FROM
      person_to_delivery
    WHERE
      delivery_address_id = NEW.delivery_address_id
      AND person_id = userId) THEN
  INSERT INTO person_to_delivery (person_id, delivery_address_id)
    VALUES (userId, NEW.delivery_address_id);
END IF;
  RETURN NEW;
END;
$BODY$
LANGUAGE 'plpgsql';

CREATE TRIGGER order_store_used_address
  AFTER INSERT ON plant_order
  FOR EACH ROW
  EXECUTE PROCEDURE order_store_user_address ();

