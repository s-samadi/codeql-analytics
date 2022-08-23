create table person
(
    id   integer primary key,
    name varchar
);

create table animal
(
    id          integer primary key,
    person_id   integer,
    animal_type varchar,
    name        varchar
);