create table person
(
    id   bigint identity primary key,
    name varchar(50)
);

create table animal
(
    id          bigint identity primary key,
    person_id   bigint,
    animal_type varchar(20),
    name        varchar(50)
);