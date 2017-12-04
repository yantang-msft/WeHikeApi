IF OBJECT_ID('dbo.post_comments', 'U') IS NOT NULL
DROP TABLE post_comments

CREATE TABLE post_comments (
	CommentId int IDENTITY(1,1) PRIMARY KEY,
	PostId int,
	UserName varchar(255),
	Content varchar(1024),
	Timestamp datetime
);
