-- =============================================
-- Projects table schema
-- =============================================

IF NOT EXISTS (
    SELECT * FROM sys.objects 
    WHERE object_id = OBJECT_ID(N'[dbo].[Projects]') 
    AND type in (N'U')
)
BEGIN
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
END
GO

-- Optional demo data (remove if not needed)
INSERT INTO [dbo].[projects] ([projectname], [partname], [madeby], [typeofwork], [comments], [ischecked])
VALUES 
('P-001', 'Part A', 'User1', 'Type1', 'Demo project', 'No'),
('P-002', 'Part B', 'User2', 'Type2', 'Test project', 'Yes');
GO
