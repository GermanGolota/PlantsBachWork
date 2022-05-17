REVOKE ALL ON SCHEMA public FROM public;

GRANT USAGE ON SCHEMA public TO public;

REVOKE ALL ON ALL TABLES IN SCHEMA public FROM PUBLIC;

REVOKE ALL ON ALL FUNCTIONS IN SCHEMA public FROM PUBLIC;

REVOKE ALL ON ALL PROCEDURES IN SCHEMA public FROM PUBLIC;

--tables
GRANT SELECT ON plant_to_image TO consumer, producer, manager;

GRANT SELECT ON instruction_to_cover TO consumer, producer, manager;

GRANT SELECT, DELETE ON plant_post TO producer, manager;

GRANT SELECT, DELETE ON plant_order TO producer, manager;

GRANT INSERT ON plant_order TO producer, manager;

--views
GRANT SELECT ON current_user_roles TO consumer, producer, manager;

GRANT SELECT ON dicts_v TO consumer, producer, manager;

GRANT SELECT ON plant_post_v TO consumer, producer, manager;

GRANT SELECT ON current_user_addresses TO consumer;

GRANT SELECT ON current_user_orders TO consumer;

GRANT SELECT ON instruction_v TO consumer, producer, manager;

GRANT SELECT ON plants_v TO producer, manager;

GRANT SELECT ON prepared_for_post_v TO producer, manager;

GRANT SELECT ON plant_orders_v TO producer, manager;

GRANT SELECT ON plant_stats_v TO manager;

--functions
--business
GRANT EXECUTE ON FUNCTION search_plant TO consumer, producer, manager;

GRANT EXECUTE ON FUNCTION place_order TO consumer;

GRANT EXECUTE ON FUNCTION search_instructions TO consumer, producer, manager;

GRANT EXECUTE ON FUNCTION post_plant TO producer, manager;

GRANT EXECUTE ON FUNCTION create_plant TO producer, manager;

GRANT EXECUTE ON FUNCTION create_instruction TO producer, manager;

GRANT EXECUTE ON FUNCTION search_users TO producer, manager;

GRANT EXECUTE ON FUNCTION get_financial TO manager;

--utility
GRANT EXECUTE ON FUNCTION array_length_no_nulls TO consumer, producer, manager;

GRANT EXECUTE ON FUNCTION get_current_user_id TO consumer, producer, manager;

GRANT EXECUTE ON FUNCTION get_current_user_id_throw TO consumer, producer, manager;

GRANT EXECUTE ON FUNCTION parse_role TO consumer, producer, manager;

--procedures
GRANT EXECUTE ON PROCEDURE edit_plant TO producer, manager;

GRANT EXECUTE ON PROCEDURE confirm_received TO consumer;

GRANT EXECUTE ON PROCEDURE edit_instruction TO producer, manager;

GRANT EXECUTE ON PROCEDURE add_user_to_group TO producer, manager;

GRANT EXECUTE ON PROCEDURE remove_user_from_group TO manager;

GRANT EXECUTE ON PROCEDURE create_user TO producer, manager;

