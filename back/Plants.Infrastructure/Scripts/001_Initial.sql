CREATE TABLE plant_group (
    id serial PRIMARY KEY,
    group_name text
);

CREATE TABLE plant_region (
    id serial PRIMARY KEY,
    region_name text
);

CREATE TABLE plant_soil (
    id serial PRIMARY KEY,
    soil_name text
);

CREATE TABLE delivery_address (
    id serial PRIMARY KEY,
    city text,
    nova_poshta_number smallint
);

CREATE TABLE person (
    id serial PRIMARY KEY,
    first_name text NOT NULL,
    last_name text NOT NULL,
    phone_number text NOT NULL,
    delivery_address_id int REFERENCES delivery_address (id)
);

CREATE TABLE plant (
    id serial PRIMARY KEY,
    group_id int NOT NULL REFERENCES plant_group (id),
    soil_id int NOT NULL REFERENCES plant_soil (id),
    region_id int NOT NULL REFERENCES plant_region (id),
    care_taker_id int NOT NULL REFERENCES person (id),
    plant_name text NOT NULL,
    description text NOT NULL,
    created date NOT NULL
);

CREATE TABLE plant_caring_instruction (
    id serial PRIMARY KEY,
    instruction_text text,
    posted_by_id int NOT NULL REFERENCES person (id),
    plant_group_id int NOT NULL REFERENCES plant_group (id)
);

CREATE TABLE plant_post (
    plant_id serial PRIMARY KEY REFERENCES plant (id),
    seller_id int NOT NULL REFERENCES person (id),
    price decimal
);

CREATE TABLE plant_order (
    post_id int PRIMARY KEY REFERENCES plant_post (plant_id),
    customer_id int NOT NULL REFERENCES person (id)
);

CREATE TABLE plant_delivery (
    order_id int PRIMARY KEY REFERENCES plant_order (post_id),
    delivery_tracking_number text NOT NULL,
    created timestamptz DEFAULT now() NOT NULL
);

CREATE TABLE plant_shipment (
    delivery_id int PRIMARY KEY REFERENCES plant_delivery (order_id),
    shipped timestamptz DEFAULT now() NOT NULL
);

