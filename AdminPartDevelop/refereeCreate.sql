-- Remove conflicting tables
DROP TABLE IF EXISTS "Excuses" CASCADE;
DROP TABLE IF EXISTS "Referees" CASCADE;
DROP TABLE IF EXISTS "VehicleSlots" CASCADE;
-- End of removing

CREATE TABLE "Excuses" (
    excuse_id SERIAL NOT NULL,
    referee_id INT NOT NULL,
    date_from DATE NOT NULL,
    time_from TIME NOT NULL,
    date_to DATE NOT NULL,
    time_to TIME NOT NULL,
    datetime_added TIMESTAMP NOT NULL,
    note VARCHAR(256),
    reason VARCHAR(256)
);
ALTER TABLE "Excuses" ADD CONSTRAINT pk_excuses PRIMARY KEY (excuse_id, referee_id);

CREATE TABLE "VehicleSlots" (
    slot_id SERIAL NOT NULL,
    referee_id INT NOT NULL,
    date_from DATE NOT NULL,
    time_from TIME NOT NULL,
    date_to DATE NOT NULL,
    time_to TIME NOT NULL,
    datetime_added TIMESTAMP NOT NULL,
    has_car_in_the_slot BOOLEAN
);
ALTER TABLE "VehicleSlots" ADD CONSTRAINT pk_vehicleslots PRIMARY KEY (slot_id, referee_id);


CREATE TABLE "Referees" (
    referee_id SERIAL NOT NULL,
    user_id TEXT,
    facr_id VARCHAR(25),
    name VARCHAR(100) NOT NULL,
    surname VARCHAR(100) NOT NULL,
    email VARCHAR(320) NOT NULL,
    league INTEGER NOT NULL,
    age INTEGER NOT NULL,
    ofs BOOLEAN NOT NULL,
    note VARCHAR(500),
    prague_zone VARCHAR(100) NOT NULL,
    actuall_prague_zone VARCHAR(100),
    car_availability BOOLEAN NOT NULL,
    timestamp_change TIMESTAMP NOT NULL
);
ALTER TABLE "Referees" ADD CONSTRAINT pk_referees PRIMARY KEY (referee_id);
ALTER TABLE "Referees" ADD CONSTRAINT uc_referees_email UNIQUE (email);

ALTER TABLE "Excuses" ADD CONSTRAINT fk_excuses_referees FOREIGN KEY (referee_id) REFERENCES "Referees" (referee_id) ON DELETE CASCADE;
ALTER TABLE "VehicleSlots" ADD CONSTRAINT fk_vehicleslots_referees FOREIGN KEY (referee_id) REFERENCES "Referees" (referee_id) ON DELETE CASCADE;