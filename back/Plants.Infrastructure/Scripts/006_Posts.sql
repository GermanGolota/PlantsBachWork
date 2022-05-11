CREATE TYPE plant_post_model AS (
  id integer,
  plant_name text,
  description text,
  price numeric,
  soil_name text,
  regions text[],
  group_name text,
  created date,
  seller_name text,
  seller_phone text,
  seller_cared bigint,
  seller_sold bigint,
  seller_instructions bigint,
  care_taker_cared bigint,
  care_taker_sold bigint,
  care_taker_isntructions bigint,
  images int[]
);

CREATE OR REPLACE FUNCTION array_length_no_nulls (arr integer[])
  RETURNS bigint
  SECURITY DEFINER
  AS $$
BEGIN
  RETURN coalesce(array_length(array_remove(arr, NULL), 1), 0);
END;
$$
LANGUAGE plpgsql;

CREATE OR REPLACE VIEW person_creds_v AS (
  SELECT
    p.id,
    array_length_no_nulls (ARRAY_AGG(DISTINCT pl.id)) AS cared_count,
    array_length_no_nulls (ARRAY_AGG(DISTINCT po.plant_id)) AS sold_count,
    array_length_no_nulls (ARRAY_AGG(DISTINCT i.id)) AS instructions_count
  FROM
    person p
  LEFT JOIN plant pl ON pl.care_taker_id = p.id
  LEFT JOIN plant_post po ON po.seller_id = p.id
  LEFT JOIN plant_caring_instruction i ON i.posted_by_id = p.id
GROUP BY
  p.id);

CREATE OR REPLACE VIEW plant_post_v AS (
  WITH posts_extended AS (
    SELECT
      p.id,
      p.plant_name,
      po.price,
      gr.group_name,
      s.soil_name,
      p.description,
      po.seller_id,
      p.care_taker_id,
      array_remove(array_agg(DISTINCT rg.region_name), NULL) AS regions,
      p.created,
      array_remove(array_agg(DISTINCT img.relation_id), NULL) AS img_ids
    FROM
      plant_post po
      JOIN plant p ON p.id = po.plant_id
      JOIN plant_group gr ON gr.id = p.group_id
      JOIN plant_soil s ON s.id = p.soil_id
      LEFT JOIN plant_to_region prg ON prg.plant_id = p.id
      LEFT JOIN plant_region rg ON rg.id = prg.plant_region_id
      LEFT JOIN plant_to_image img ON img.plant_id = p.id
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
      post.description,
      post.price,
      post.soil_name,
      post.regions,
      post.group_name,
      post.created,
      FORMAT('%s %s', seller.first_name, seller.last_name) AS seller_name,
      seller.phone_number AS seller_phone,
      seller_creds.cared_count AS seller_cared,
      seller_creds.sold_count AS seller_sold,
      seller_creds.instructions_count AS seller_instructions,
      care_taker_creds.cared_count AS care_taker_cared,
      care_taker_creds.sold_count AS care_taker_sold,
      care_taker_creds.instructions_count AS care_taker_instructions,
      post.img_ids AS images
    FROM
      posts_extended post
      JOIN person seller ON seller.id = post.seller_id
      LEFT JOIN person_creds_v seller_creds ON seller_creds.id = post.seller_id
      LEFT JOIN person_creds_v care_taker_creds ON care_taker_creds.id = post.care_taker_id);

ALTER TABLE plant_order
  ADD COLUMN created date;

UPDATE
  plant_order
SET
  created = CURRENT_DATE
WHERE
  created IS NULL;

ALTER TABLE plant_post
  ALTER COLUMN created SET NOT NULL;

ALTER TABLE plant_post
  ALTER COLUMN created SET DEFAULT CURRENT_DATE;

ALTER TABLE plant_order
  ADD COLUMN delivery_address_id INT REFERENCES delivery_address (id);

ALTER TABLE delivery_address
  DROP COLUMN region_id;

ALTER TABLE plant_order
  ALTER COLUMN created SET DEFAULT CURRENT_DATE;

ALTER TABLE plant_order
  ALTER COLUMN created SET NOT NULL;

INSERT INTO delivery_address (city, nova_poshta_number, person_id)
  VALUES ('Odessa', 15, 1);

CREATE VIEW person_addresses_v AS (
  SELECT
    p.id,
    array_agg(d.city) AS cities,
    array_agg(d.nova_poshta_number) AS posts
  FROM
    delivery_address d
    JOIN person p ON p.id = d.person_id
  GROUP BY
    p.id);

CREATE VIEW current_user_addresses AS (
  SELECT
    cities,
    posts
  FROM
    person_addresses_v
  WHERE
    id = get_current_user_id ());

UPDATE
  plant_order p
SET
  delivery_address_id = (
    SELECT
      pa.id
    FROM
      person_addresses_v pa
    WHERE
      pa.id = p.customer_id
    LIMIT 1);

ALTER TABLE plant_order
  ALTER COLUMN delivery_address_id SET NOT NULL;

CREATE OR REPLACE FUNCTION get_current_user_id_throw ()
  RETURNS integer
  AS $BODY$
DECLARE
  userId int;
BEGIN
  userId := get_current_user_id ();
  IF userId = - 1 THEN
    RAISE EXCEPTION 'There is no person attached to %', CURRENT_USER
      USING HINT = 'Please, consider using credentials that have a person attached to them';
    ELSE
      RETURN userId;
    END IF;
END;
$BODY$
LANGUAGE 'plpgsql';

CREATE OR REPLACE FUNCTION set_current_user_id_order ()
  RETURNS TRIGGER
  AS $BODY$
DECLARE
  userId int;
BEGIN
  userId := get_current_user_id_throw ();
  NEW.customer_id = userId;
  RETURN NEW;
END;
$BODY$
LANGUAGE 'plpgsql';

CREATE TRIGGER order_set_customer
  BEFORE INSERT ON plant_order
  FOR EACH ROW
  EXECUTE PROCEDURE set_current_user_id_order ();

