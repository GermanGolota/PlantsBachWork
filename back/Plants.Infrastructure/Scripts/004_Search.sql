CREATE OR REPLACE VIEW plant_search_v AS
SELECT
  p.id,
  p.plant_name,
  po.price,
  p.created,
  gr.id AS group_id,
  s.id AS soil_id,
  array_agg(DISTINCT rg.id) AS regions
FROM
  plant_post po
  JOIN plant p ON p.id = po.plant_id
  JOIN plant_group gr ON gr.id = p.group_id
  JOIN plant_soil s ON s.id = p.soil_id
  JOIN plant_to_region prg ON prg.plant_id = p.id
  JOIN plant_region rg ON rg.id = prg.plant_region_id
GROUP BY
  p.id,
  gr.id,
  s.id,
  po.price;

--this would search plant table for provided values
--would skip search by specific field when null value is provided
CREATE OR REPLACE FUNCTION search_plant (plantName text, priceRangeBottom numeric, priceRangeTop numeric, lastDate timestamp without time zone, groupIds integer[], soilIds integer[], regionIds integer[])
  RETURNS TABLE (
    id integer,
    plant_name text,
    description text,
	price numeric,
    imageIds integer[]
  )
  AS $$
BEGIN
  RETURN QUERY
  SELECT
    p.id,
    p.plant_name,
    p.description,
	se.price,
    array_remove(array_agg(i.relation_id), NULL)
  FROM
    plant_search_v se
    JOIN plant p ON p.id = se.id
    JOIN plant_group g ON g.id = p.group_id
    JOIN plant_soil s ON s.id = p.soil_id
    LEFT JOIN plant_to_image i ON i.plant_id = p.id
	LEFT JOIN plant_order o on o.post_id = p.id
  WHERE
    o.customer_id is null
    AND (plantName IS NULL
      OR to_tsvector(se.plant_name) @@ to_tsquery(plantName))
    AND (priceRangeBottom IS NULL
      OR se.price >= priceRangeBottom)
    AND (priceRangeTop IS NULL
      OR se.price <= priceRangeTop)
    AND (lastDate IS NULL
      OR se.created >= lastDate)
    AND (groupIds IS NULL
      OR se.group_id = ANY (groupIds))
    AND (soilIds IS NULL
      OR se.soil_id = ANY (soilIds))
    --&& means intersection
    AND (regionIds IS NULL
      OR regionIds && se.regions)
  GROUP BY
    p.id, se.price;
END;
$$
LANGUAGE plpgsql;

