--tables
GRANT INSERT ON plant_delivery TO producer;

GRANT INSERT ON plant_shipment TO consumer;

GRANT SELECT ON instruction_to_cover TO consumer, producer, manager;

GRANT INSERT ON instruction_to_cover TO producer, manager;

GRANT SELECT ON plant_to_image TO consumer, producer, manager;

GRANT INSERT ON plant_to_image TO producer, manager;

GRANT SELECT ON person_to_login TO consumer, producer, manager;

GRANT SELECT, INSERT ON plant TO producer, manager;

GRANT UPDATE ON plant_id_seq TO producer, manager;

GRANT UPDATE ON plant_to_region_id_seq TO producer, manager;

GRANT UPDATE ON plant_to_image_relation_id_seq TO producer, manager;

GRANT SELECT, INSERT ON plant_post TO producer, manager;

GRANT SELECT ON plant_post TO consumer;

GRANT INSERT ON plant_caring_instruction TO producer, manager;

GRANT UPDATE ON plant_caring_instruction_id_seq TO producer, manager;

GRANT SELECT, INSERT ON plant_order TO consumer;

GRANT SELECT, INSERT ON delivery_address TO consumer;

GRANT SELECT, INSERT ON person_to_delivery TO consumer;

GRANT UPDATE ON person_to_delivery_id_seq TO consumer;

GRANT INSERT ON plant_to_region TO producer;

GRANT SELECT ON person TO consumer, producer, manager;

--procedures
GRANT EXECUTE ON PROCEDURE add_user_to_group TO consumer, producer, manager;

GRANT EXECUTE ON PROCEDURE create_person TO consumer, producer, manager;

GRANT EXECUTE ON PROCEDURE create_user TO consumer, producer, manager;

GRANT EXECUTE ON PROCEDURE remove_user_from_group TO consumer, producer, manager;

GRANT EXECUTE ON PROCEDURE edit_instruction TO producer, manager;

GRANT EXECUTE ON PROCEDURE edit_plant TO producer, manager;

--functions
GRANT EXECUTE ON FUNCTION create_instruction TO producer, manager;

GRANT EXECUTE ON FUNCTION create_plant TO producer, manager;

GRANT EXECUTE ON FUNCTION get_financial TO manager;

GRANT EXECUTE ON FUNCTION place_order TO consumer;

GRANT EXECUTE ON FUNCTION post_plant TO producer, manager;

GRANT EXECUTE ON FUNCTION search_instructions TO consumer, producer, manager;

GRANT EXECUTE ON FUNCTION search_plant TO consumer, producer, manager;

GRANT EXECUTE ON FUNCTION search_users TO consumer, producer, manager;

GRANT EXECUTE ON FUNCTION set_current_user_id_care_taker TO producer, manager;

GRANT EXECUTE ON FUNCTION set_current_user_id_seller TO producer, manager;

GRANT EXECUTE ON FUNCTION set_current_user_id_instruction TO producer, manager;

GRANT EXECUTE ON FUNCTION set_current_user_id_order TO consumer;

GRANT EXECUTE ON FUNCTION order_store_user_address TO consumer;

GRANT EXECUTE ON FUNCTION delete_post TO producer, manager;

--views
GRANT SELECT ON dicts_v TO consumer, producer, manager;

GRANT SELECT ON current_user_addresses TO consumer, producer, manager;

GRANT SELECT ON instruction_v TO consumer, producer, manager;

GRANT SELECT ON current_user_orders TO consumer;

GRANT SELECT ON plant_orders_v TO producer, manager;

GRANT SELECT ON plants_v TO producer, manager;

GRANT SELECT ON prepared_for_post_v TO producer, manager;

GRANT SELECT ON plant_post_v TO consumer, producer, manager;

GRANT SELECT ON plant_stats_v TO manager;

GRANT SELECT ON user_to_roles TO consumer, producer, manager;

