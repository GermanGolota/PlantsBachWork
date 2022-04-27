CREATE TABLE plant_group
(
    id Serial PRIMARY KEY,
    group_name text
);


CREATE TABLE plant_region
(
    id Serial PRIMARY KEY,
    region_name text
);

CREATE TABLE plant_soil
(
    id Serial PRIMARY KEY,
    soil_name text
);

CREATE TABLE delivery_address
(
	id Serial PRIMARY KEY,
	city TEXT,
	nova_poshta_number SMALLINT
);

CREATE TABLE person
(
    id Serial PRIMARY KEY,
    first_name TEXT NOT NULL,
    last_name TEXT NOT NULL,
    phone_number TEXT NOT NULL,
    delivery_address_id INT REFERENCES delivery_address(id)
);

CREATE TABLE plant
(
    id Serial PRIMARY KEY,
    group_id int NOT NULL REFERENCES plant_group(id),
    soil_id INT NOT NULL REFERENCES plant_soil(id) ,
    region_id INT NOT NULL REFERENCES plant_region(id),
    care_taker_id INT NOT NULL REFERENCES person(id),
    plant_name Text,
    description TEXT,
    created DATE NOT NULL       
);

CREATE TABLE plant_caring_instruction
(
    id Serial PRIMARY KEY,
    instruction_text TEXT,
    posted_by_id INT NOT NULL REFERENCES person(id),
    plant_group_id INT NOT NULL REFERENCES plant_group(id)
);

CREATE TABLE plant_post
(
 plant_id Serial Primary KEY REFERENCES plant(id), 
 seller_id INT NOT NULL REFERENCES person(id),
 price decimal
);

CREATE TABLE plant_order
(
 post_id INT Primary KEY REFERENCES plant_post(plant_id),
 customer_id INT NOT NULL REFERENCES person(id)
);

CREATE TABLE plant_shipment
(
 order_id INT Primary KEY REFERENCES plant_order(post_id )  ,
 Shipped Date NOT NULL
);