--This view displays plants that have not been posted yet
CREATE OR REPLACE VIEW plants_v AS (
  SELECT
    p.id,
    p.plant_name,
    p.description,
    p.care_taker_id = get_current_user_id_throw () AS is_mine,
	p.group_id,
	p.soil_id,
	ARRAY_REMOVE( ARRAY_AGG(DISTINCT img.relation_id), NULL) as images,
	ARRAY_REMOVE( ARRAY_AGG(DISTINCT prg.plant_region_id), NULL) as regions,
	p.created
  FROM
    plant p
  LEFT JOIN plant_to_region prg on prg.plant_id = p.id
  LEFT JOIN plant_to_image img on img.plant_id = p.id
  LEFT JOIN plant_post po ON po.plant_id = p.id
 
WHERE
  po.plant_id IS NULL
	GROUP BY p.id
);

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



CREATE OR REPLACE function create_plant (plantName text, description text, regionIds int[], soilId int, groupId int, created timestamp without time zone, pictures bytea[])
  returns int
  AS $$
DECLARE 
	plantId int;
	regionId int;
	picture bytea;
BEGIN
	INSERT INTO plant(created, description, group_id, plant_name, soil_id)
	values (created, description, groupId, plantName, soilId)
	returning id into plantId;
	
	FOREACH regionId IN ARRAY regionIds
  	LOOP
		INSERT INTO plant_to_region(plant_id, plant_region_id)
		values (plantId, regionId);
  	END LOOP;
	
	FOREACH picture IN ARRAY pictures
  	LOOP
		INSERT INTO plant_to_image(plant_id, image)
		values (plantId, picture);
  	END LOOP;
	
	RETURN plantId;
END;
$$
LANGUAGE plpgsql;



CREATE OR REPLACE procedure edit_plant (plantId int, plantName text,plantDescription text, regionIds int[], soilId int, groupId int, removedImages int[], newImages bytea[])
  AS $$
DECLARE
	regionId int;
	picture bytea;
BEGIN
	update plant
	set plant_name = plantName, description = plantDescription, soil_id = soilId, group_id = groupId
	where id = plantId;
	
	DELETE FROM plant_to_region 
	WHERE plant_id = plantId;
	
	FOREACH regionId IN ARRAY regionIds
  	LOOP
		INSERT INTO plant_to_region(plant_id, plant_region_id)
		values (plantId, regionId);
  	END LOOP;
	
	DELETE FROM plant_to_image
	WHERE plant_id = plantId AND relation_id = ANY(removedImages);
	
	FOREACH picture IN ARRAY newImages
  	LOOP
		INSERT INTO plant_to_image(plant_id, image)
		values (plantId, picture);
  	END LOOP;
END;
$$
LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION plant_no_update_posted ()
  RETURNS TRIGGER
  AS $BODY$
BEGIN
  IF EXISTS (SELECT plant_id FROM plant_post WHERE plant_id = NEW.id) THEN
  	RAISE EXCEPTION 'You cannot edit posted plant';
  ELSE
  	RETURN NEW;
  END IF;
END;
$BODY$
LANGUAGE 'plpgsql';

CREATE TRIGGER plant_prevent_update_of_posted
  BEFORE UPDATE ON plant
  FOR EACH ROW
  EXECUTE PROCEDURE plant_no_update_posted ();
