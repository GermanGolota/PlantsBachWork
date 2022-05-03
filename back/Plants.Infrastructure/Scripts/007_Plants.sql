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

