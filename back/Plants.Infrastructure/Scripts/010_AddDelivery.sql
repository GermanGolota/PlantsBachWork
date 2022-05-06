CREATE TABLE plant_delivery (
  order_id int PRIMARY KEY REFERENCES plant_order (post_id),
  delivery_tracking_number text NOT NULL,
  created date DEFAULT CURRENT_DATE
);

INSERT INTO plant_delivery (order_id, delivery_tracking_number, created)
SELECT
  order_id,
  'Some ttn',
  shipped
FROM
  plant_shipment;

DROP VIEW plant_stats_v;

DROP FUNCTION get_financial;

DROP TABLE plant_shipment;

CREATE TABLE plant_shipment (
  delivery_id int PRIMARY KEY REFERENCES plant_delivery (order_id),
  shipped date DEFAULT CURRENT_DATE
);

INSERT INTO plant_shipment (delivery_id, shipped)
SELECT
  order_id,
  created + INTERVAL '1 day'
FROM
  plant_delivery;

CREATE OR REPLACE VIEW plant_stats_v AS (
  WITH gToInstruction AS (
    SELECT
      plant_group_id AS gid,
      Count(*) AS cnt
    FROM
      plant_caring_instruction
    GROUP BY
      plant_group_id),
    gToPlants AS (
      SELECT
        group_id AS gid,
        Count(*) AS cnt
      FROM
        plant
      GROUP BY
        group_id),
      gToIncome AS (
        SELECT
          p.group_id AS gid,
          SUM(price) AS total
        FROM
          plant_shipment s
          JOIN plant_order o ON o.post_id = s.delivery_id
          JOIN plant_post po ON po.plant_id = o.post_id
          JOIN plant p ON p.id = po.plant_id
        GROUP BY
          group_id),
        gToPopularity AS (
          SELECT
            p.group_id AS gid,
            COUNT(*) AS total
          FROM
            plant_order o
            JOIN plant_post po ON po.plant_id = o.post_id
            JOIN plant p ON p.id = po.plant_id
          GROUP BY
            group_id
)
          SELECT
            g.id,
            g.group_name,
            p.cnt AS plants_count,
            p2.total AS popularity,
            i.total AS income,
            i2.cnt AS instructions
          FROM
            gToPlants p
            JOIN gToPopularity p2 USING (gid)
            JOIN gToIncome i USING (gid)
            JOIN gToInstruction i2 USING (gid)
            JOIN plant_group g ON g.id = (gid));

--financial stats
CREATE OR REPLACE FUNCTION get_financial (start_date timestamp without time zone, end_date timestamp without time zone)
  RETURNS TABLE (
    groupId int,
    group_name text,
    sold_count bigint,
    percent_sold numeric,
    income numeric
  )
  AS $$
BEGIN
  RETURN QUERY ( WITH group_to_post_count AS (
      SELECT
        p.group_id, count(*) AS total FROM plant p
      JOIN plant_post pl ON pl.plant_id = p.id
      LEFT JOIN plant_shipment s ON s.delivery_id = p.id
      WHERE
        s.delivery_id IS NULL
        OR s.shipped BETWEEN start_date AND end_date GROUP BY p.group_id)
    SELECT
      g.id, g.group_name, count(*) AS sold_count, round((count(*) * 1.0 / pc.total) * 100) AS percent_sold, sum(p.price) AS income FROM plant pl
    JOIN plant_post p ON p.plant_id = pl.id
    JOIN plant_order o ON o.post_id = p.plant_id
    JOIN plant_shipment s ON s.delivery_id = o.post_id
    JOIN plant_group g ON g.id = pl.group_id
    JOIN group_to_post_count pc ON pc.group_id = g.id
    WHERE
      s.shipped BETWEEN start_date AND end_date GROUP BY g.id, pc.total);
END
$$
LANGUAGE plpgsql;

