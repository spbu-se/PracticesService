create table Practice
(
    ID int         not null GENERATED ALWAYS AS IDENTITY,
    Title   varchar(50) not null,
    Description   varchar(500) not null,
    Owner   varchar(255) not null,
    CONSTRAINT Practice_PK PRIMARY KEY (ID)
);


-- drop table Practice;
