CREATE TABLE [dbo].[projects] (
    [id]          INT           IDENTITY (1, 1) NOT NULL,
    [projectname] VARCHAR (100) NOT NULL,
    [partname]    VARCHAR (100) NOT NULL,
    [madeby]      VARCHAR (100) NOT NULL,
    [typeofwork]  VARCHAR (100) NOT NULL,
    [created_at]  DATETIME      DEFAULT (getdate()) NOT NULL,
    [comments]    VARCHAR (255) NOT NULL,
    [ischecked]   VARCHAR (5)   DEFAULT ('No') NOT NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
);