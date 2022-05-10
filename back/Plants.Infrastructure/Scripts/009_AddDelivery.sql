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

ALTER TABLE plant_delivery
  ALTER COLUMN created TYPE timestamptz;

ALTER TABLE plant_delivery
  ALTER COLUMN created SET DEFAULT now();

ALTER TABLE plant_shipment
  ALTER COLUMN shipped TYPE timestamptz;

ALTER TABLE plant_shipment
  ALTER COLUMN shipped SET DEFAULT now();

