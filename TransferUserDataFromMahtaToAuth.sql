--There are two different commands in order to import some data from a .csv file to the database,
-- and they hame a big difference, which makes them suitable for different situations.
-- The command COPY reads the file from the the server, whereas the \copy command (notice that the "\" character is part 
-- of the command itself) reads the file from the client from which the psql program has been connected to the database server.
-- 1. This one needs the file to be on the SERVER on which the database resides.
COPY users ("id", "first_name", "last_name", "phone_number", "national_code", "created_at", "updated_at")
	from '/home/alid/users.csv' DELIMITER ',' CSV HEADER
	;
-- 2. This command reads the file from the CLIENT from which the psql program has been connected to the db server.
\copy users ("id", "first_name", "last_name", "phone_number", "created_at", "updated_at") 
	from '/home/ali/auth-service/mahta-2-backend/src/mahta_to_auth_users_20210412.csv' csv header
	;
