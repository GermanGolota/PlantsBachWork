--This view displays plants that have not been posted yet
CREATE OR REPLACE VIEW plants_v AS (
  SELECT
    p.id,
    p.plant_name,
    p.description,
    p.care_taker_id = get_current_user_id_throw () AS isMine
  FROM
    plant p
  LEFT JOIN plant_post po ON po.plant_id = p.id
WHERE
  po.plant_id IS NULL);

--this view would display posts as they would be seen after posting
CREATE VIEW prepared_for_post_v AS (
  WITH plant_extended AS (
    SELECT
      p.id,
      p.plant_name,
      gr.group_name,
      s.soil_name,
      p.description,
      p.care_taker_id,
      array_remove(array_agg(DISTINCT rg.region_name), NULL) AS regions,
      p.created,
      array_remove(array_agg(DISTINCT img.relation_id), NULL) AS images
    FROM
      plant p
      JOIN plant_group gr ON gr.id = p.group_id
      JOIN plant_soil s ON s.id = p.soil_id
      LEFT JOIN plant_to_region prg ON prg.plant_id = p.id
      LEFT JOIN plant_region rg ON rg.id = prg.plant_region_id
      LEFT JOIN plant_post po ON po.plant_id = p.id
      LEFT JOIN plant_to_image img ON img.plant_id = p.id
    WHERE
      po.plant_id IS NULL
    GROUP BY
      p.id,
      gr.group_name,
      s.soil_name
)
    SELECT
      p.id,
      p.plant_name,
      p.description,
      p.soil_name,
      p.regions,
      p.group_name,
      p.created,
      FORMAT('%s %s', seller.first_name, seller.last_name) AS seller_name,
      seller.phone_number AS seller_phone,
      seller_creds.cared_count AS seller_cared,
      seller_creds.sold_count AS seller_sold,
      seller_creds.instructions_count AS seller_instructions,
      care_taker_creds.cared_count AS care_taker_cared,
      care_taker_creds.sold_count AS care_taker_sold,
      care_taker_creds.instructions_count AS care_taker_instructions,
      p.images AS images
    FROM
      plant_extended p
      JOIN person seller ON seller.id = get_current_user_id_throw ()
      LEFT JOIN person_creds_v seller_creds ON seller_creds.id = seller.id
      LEFT JOIN person_creds_v care_taker_creds ON care_taker_creds.id = p.care_taker_id);


--Reason Code:
-- 0 - all good
-- 1 - plant does not exist
-- 2 - already posted
-- 3 - bad price
CREATE OR REPLACE FUNCTION post_plant (IN plantId int, IN price numeric, OUT wasPlaced boolean, OUT reasonCode integer)
AS $$
DECLARE
  plantExists boolean;
  postExists boolean;
BEGIN
  CREATE TEMP TABLE IF NOT EXISTS post_results AS
  SELECT
    p.id AS plant_id,
    po.plant_id AS post_id
  FROM
    plant p
  LEFT JOIN plant_post po ON po.plant_id = p.id
WHERE
  p.id = plantId
LIMIT 1;
  plantExists := EXISTS (
    SELECT
      plant_id
    FROM
      post_results);
  postExists := (
    SELECT
      post_id
    FROM
      post_results) IS NOT NULL;
  IF plantExists THEN
    IF postExists THEN
      wasPlaced := FALSE;
      reasonCode := 2;
    ELSE
		IF price <= 0 THEN
			 wasPlaced := FALSE;
      	     reasonCode := 3;
		ELSE
			 INSERT INTO plant_post (plant_id, price)
        VALUES (plantId, price);
      wasPlaced := TRUE;
      reasonCode := 0;
		END IF;
    END IF;
  ELSE
    wasPlaced := FALSE;
    reasonCode := 1;
  END IF;
  DROP TABLE post_results;
END;
$$
LANGUAGE plpgsql;

DELETE FROM plant_post
where price <= 0;

ALTER TABLE plant_post
	ADD CHECK (price >= 0);

select *
from plant_post
order by plant_id


DELETE FROM plant_post
where price <= 0;

ALTER TABLE plant_post
	ADD CHECK (price >= 0);
