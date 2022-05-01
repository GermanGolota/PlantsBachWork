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
      array_agg(DISTINCT rg.region_name) AS regions,
      p.created,
      array_remove(array_agg(DISTINCT img.relation_id), NULL) AS img_ids
    FROM
      plant_post po
      JOIN plant p ON p.id = po.plant_id
      JOIN plant_group gr ON gr.id = p.group_id
      JOIN plant_soil s ON s.id = p.soil_id
      JOIN plant_to_region prg ON prg.plant_id = p.id
      JOIN plant_region rg ON rg.id = prg.plant_region_id
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

CREATE OR REPLACE FUNCTION get_post (plantId integer)
  RETURNS plant_post_model
  AS $$
DECLARE
  res plant_post_model;
BEGIN
  SELECT
    *
  FROM
    plant_post_v post
  WHERE
    id = plantId INTO res;
  RETURN res;
END;
$$
LANGUAGE plpgsql;

