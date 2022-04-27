INSERT INTO plant_group(group_name)
values ('Cherry'), ('Apple'), ('Strawberry'), ('Blackberry'), ('Grape'), ('Plum'), ('Raspberry'), ('Blueberry');
INSERT INTO plant_region(region_name)
values ('Taiga'), ('Savana'), ('Desert'), ('Moderate'), ('Mild'), ('Jungle');
INSERT INTO plant_soil(soil_name)
values ('Loamy'), ('Sandy'), ('Peaty'), ('Silty'), ('Chalky'), ('Clay');

INSERT INTO delivery_address(city, nova_poshta_number)
VALUES ('Odessa', 252), ('Lviv', 2), ('Kiev', 9), ('Kherson', 28), 
('Odessa', 2), ('Lviv', 252), ('Kiev', 91), ('Kherson', 8),
('Kiev', 28), ('Kherson', 1)

INSERT INTO Person(first_name, last_name, phone_number, delivery_address_id)
VALUES ('Oleg', 'Olegov', '503165050', NULL),
('Vladimir', 'Vladimirov', '503225050', NULL),
('Stepan', 'Stepanov', '503245050', 1),
('Oleg', 'Stepanov', '503125050', 2),
('Vladimir', 'Olegov', '503925151', 3),
('Oleg', 'Stepanov', '503925151', 4),
('Victor', 'Victorov', '503935151', 6),
('Vladimir', 'Victorov', '503945151', 7),
('Oleg', 'Victorov', '503955151', 8),
('Stepan', 'Olegov', '503965151', 10);

INSERT INTO plant_caring_instruction(instruction_text, plant_group_id, posted_by_id)
VALUES ('we recommend placing it in a shallow tray filled with a layer of gravel with water added', 1, 1),
('Fertilizing is also necessary if your bonsai is to remain healthy and beautiful', 1, 2),
('You will need to fertilize your apple tree so that it maintains the necessary nutrients', 2, 2),
('Humans aren’t the only ones attracted to apple trees. Many pests are known to be drawn to them as well', 2, 1),
('Apple maggot flies are notorious for laying their eggs on apples in the summer months', 2, 3),
('You can prevent pests by placing red sphere traps on your tree.', 2, 4),
('Plum trees love a full sun, so plan where you want to plant your trees', 6, 1),
('Wait a month after you prepare your soil, and then begin planting your trees', 6, 2),
('The amount and frequency of water your plum trees need depends on the weather', 6, 4),
('Water blueberry plants during the day. Keep the soil moist but not soggy', 8, 1);


INSERT INTO plant(plant_name, description, created, care_taker_id, region_id, group_id, soil_id)
VALUES ('Dark Red Cherry', 
		'Dark red cherries tend to be sweet-tasting and incredibly juicy', 
		current_timestamp, 
	   2, 5, 1, 4),
('Sour Cherry', 
		'Morello cherry, though the Montmorency tart cherry is the more popular cherry variety.', 
		current_timestamp, 
	   1, 4, 1, 1),
('Rainier cherry', 
		'The Rainier cherry, named for Washington state’s Mount Rainier, is a sweet cherry variety, as is the stardust cherry', 
		current_timestamp, 
	   1, 4, 1, 1),
('Yellow cherry', 
		'The lightness in color of yellow cherries, it’s difficult to hide bruises on the fruits.', 
		current_timestamp, 
	   2, 5, 1, 4),
('Biloxi Blueberry', 
		'This Southern Highbush type is a relatively new cultivar, developed at Mississippi State University. And it’s great for low-chill or even no-chill environments.', 
		current_timestamp, 
	   2, 4, 8, 6),	   
('Blueray Blueberry', 
		'With sweet, light blue berries that begin to ripen in early to mid-July, this Northern Highbush cultivar is known as a great type to plant with other highbush types for cross-pollination.', 
		current_timestamp, 
	   3, 4, 8, 6),
('Fuji', 
		'On the sweeter side, Fuji apples are a relative to red delicious apples and make a great flavor enhancer', 
		current_timestamp, 
	   4, 1, 2, 3),	 	   
('Desert apple', 
		'Apple that grows in the desert', 
		current_timestamp, 
	   3, 3, 2, 2),	 	   
('Everbearing Strawberries', 
		'Everbearing strawberries have modest crops, but as soon as you get 12 hours of daylight they can start blooming', 
		current_timestamp, 
	   3, 6, 3, 5),	   	   
('Black Satin', 
		'Black Satin" is a mid-season blackberry', 
		current_timestamp, 
	  1, 2, 4, 1),
('Agiorgitiko', 
		'A red Greek wine grape variety', 
		current_timestamp, 
	  2, 2, 5, 4),
('Bababerry', 
		'Bababerry raspberry is an extra-large red variety that can grow up to an inch and a half long', 
		current_timestamp, 
	  2, 2, 7, 3),	
('Kelsey Plum', 
		'The Kelsey plum is a unique plum variety mainly because of its unusual green skin. It got its earliest start in China before moving to Japan and finally to the United States', 
		current_timestamp, 
		  3, 2, 6, 2);	

INSERT INTO plant_post(plant_id, price, seller_id)
VALUES (1, 25.3, 4),
(2, 40.5, 2),
 (3, 26.6, 3),
 (4, 35.5, 1),
 (5, 96.7, 2),
 (6, 12, 3),
 (7, 25.5, 4),
 (8, 25.1, 1),
 (9, 26.6, 2),
 (10, 21.1, 3),
 (11, 19.5, 4);

INSERT INTO plant_order(post_id, customer_id)
VALUES (1, 5),
	 (2, 8),
	 (3, 9),
	 (4, 6),
	 (5, 7),
	 (6, 7),
	 (7, 7),
	 (8, 6),
	 (9, 5),
	 (10, 9);

INSERT INTO plant_shipment(order_id, shipped)
VALUES (1, '2021-10-7'),
	 (2, '2021-10-8'),
	 (3, '2021-10-8'),
	 (4, '2021-10-9'),
	 (5, '2021-10-10'),
	 (6, '2021-10-12'),
	 (7, '2021-10-13'),
	 (8, '2021-10-7'),
	 (9, '2021-10-8'),
	 (10, '2021-10-9');
	   
	  
