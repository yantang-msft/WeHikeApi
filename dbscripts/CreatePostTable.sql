IF OBJECT_ID('dbo.posts', 'U') IS NOT NULL
DROP TABLE posts

CREATE TABLE posts (
	PostId int IDENTITY(1,1) PRIMARY KEY,
	UserName varchar(255),
	ImageUrl varchar(1024),
	Description varchar(255),
	Longitude real Not Null,
	Latitude real Not Null,
	Timestamp datetime
);

select * from posts