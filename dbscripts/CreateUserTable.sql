﻿IF OBJECT_ID('dbo.users', 'U') IS NOT NULL
DROP TABLE users

CREATE TABLE users (
	PersonId int IDENTITY(1,1) PRIMARY KEY,
	UserName varchar(255),
	Password varchar(255),
	Email varchar(255),
	AuthToken varchar(255),
	PhotoUrl varchar(1024),
	PhoneNumber varchar(32),
	FacebookUrl varchar(1024),
	TwitterUrl varchar(1024)
);
