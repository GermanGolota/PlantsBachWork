CREATE VIEW dicts_v AS (
  SELECT
    array_agg(g.id) AS ids,
    array_agg(g.group_name) AS
  VALUES
,
    'group' AS type
  FROM
    plant_group g
  UNION
  SELECT
    array_agg(s.id),
    array_agg(s.soil_name),
    'soil' AS type
  FROM
    plant_soil s
  UNION
  SELECT
    array_agg(r.id),
    array_agg(r.region_name),
    'region' AS type
  FROM
    plant_region r);

