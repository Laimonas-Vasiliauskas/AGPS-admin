-- =============================================
-- Projects table schema
-- =============================================

IF NOT EXISTS (
    SELECT * FROM sys.objects 
    WHERE object_id = OBJECT_ID(N'[dbo].[Projects]') 
    AND type in (N'U')
)
BEGIN
    CREATE TABLE [dbo].[Projects] (
        [id] INT IDENTITY(1,1) NOT NULL,
        [project_number] VARCHAR(50) NOT NULL,
        [comments] VARCHAR(255) NOT NULL,
        [ischecked] BIT NOT NULL DEFAULT (0),
        [created_at] DATETIME NOT NULL DEFAULT (GETDATE()),
        CONSTRAINT [PK_Projects] PRIMARY KEY CLUSTERED ([id] ASC)
    );
END
GO

-- Optional demo data (remove if not needed)
INSERT INTO [dbo].[Projects] ([project_number], [comments], [ischecked])
VALUES 
('P-001', 'Demo project', 0),
('P-002', 'Test project', 1);
GO
