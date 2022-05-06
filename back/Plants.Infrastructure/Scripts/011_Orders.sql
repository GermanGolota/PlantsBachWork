ALTER TABLE plant_order
  ALTER COLUMN created TYPE timestamptz;

ALTER TABLE plant_order
  ALTER COLUMN created SET DEFAULT now();

DELETE FROM plant_shipment
WHERE delivery_id IN (1, 20);

--case : 0 - created, 1 - delivering, 2 - delivered
CREATE VIEW plant_orders_v AS (
  SELECT
    (
      CASE WHEN s.delivery_id IS NOT NULL THEN
        2
      WHEN d.order_id IS NOT NULL THEN
        1
      ELSE
        0
      END) AS status,
    o.post_id,
    o.created AS ordered,
    da.city,
    da.nova_poshta_number AS mail_number,
    seller.first_name || ' ' || seller.last_name AS seller_name,
    seller.phone_number AS seller_contact,
    po.price,
    d.delivery_tracking_number,
    d.created AS delivery_started,
    s.shipped
  FROM
    plant_order o
    JOIN delivery_address da ON da.id = o.delivery_address_id
    JOIN plant_post po ON po.plant_id = o.post_id
    JOIN person seller ON seller.id = po.seller_id
    LEFT JOIN plant_delivery d ON d.order_id = o.post_id
    LEFT JOIN plant_shipment s ON s.delivery_id = d.order_id
  ORDER BY
    status,
    ordered,
    post_id);

CREATE OR REPLACE VIEW current_user_orders AS (
  SELECT
    v.*
  FROM
    plant_orders_v v
    JOIN plant_order o ON v.post_id = o.post_id
  WHERE
    o.customer_id = get_current_user_id_throw ());

