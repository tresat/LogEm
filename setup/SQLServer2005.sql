
-- LogEm DDL script for Microsoft SQL Server 2000 or later.

-- $Id$
SET XACT_ABORT ON
BEGIN TRAN

DECLARE @DBCompatibilityLevel INT
DECLARE @DBCompatibilityLevelMajor INT
DECLARE @DBCompatibilityLevelMinor INT

SELECT @DBCompatibilityLevel = cmptlevel 
FROM master.dbo.sysdatabases 
WHERE name = DB_NAME()

IF @DBCompatibilityLevel <> 90
BEGIN
    SELECT @DBCompatibilityLevelMajor = @DBCompatibilityLevel / 10, 
           @DBCompatibilityLevelMinor = @DBCompatibilityLevel % 10
           
    PRINT N'
    ===========================================================================
    WARNING! 
    ---------------------------------------------------------------------------
    
    This script is designed for Microsoft SQL Server 2005 (9.0) but your 
    database is set up for compatibility with version ' 
    + CAST(@DBCompatibilityLevelMajor AS NVARCHAR(80)) 
    + N'.' 
    + CAST(@DBCompatibilityLevelMinor AS NVARCHAR(80)) 
    + N'. Although 
    the script should work with later versions of Microsoft SQL Server, 
    you can ensure compatibility by executing the following statement:
    
    ALTER DATABASE [' 
    + DB_NAME() 
    + N'] 
    SET COMPATIBILITY_LEVEL = 90

    If you are hosting LogEm in the same database as your application 
    database and do not wish to change the compatibility option then you 
    should create a separate database to host LogEm where you can set the 
    compatibility level more freely.

    ===========================================================================
'
END
GO

/* ------------------------------------------------------------------------ 
        SCHEMAS
   ------------------------------------------------------------------------ */
   
CREATE SCHEMA logEm;
GO

/* ------------------------------------------------------------------------ 
        TABLES
   ------------------------------------------------------------------------ */

CREATE TABLE [logEm].[UserRequests]
(
    [UserRequestId]   UNIQUEIDENTIFIER NOT NULL,
    [Application] NVARCHAR(60)  COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [TimeUtc]     DATETIME NOT NULL,
	[User]			NVARCHAR(100) NULL,
    [Sequence]    INT IDENTITY (1, 1) NOT NULL
) 
ON [PRIMARY]
GO

/* ------------------------------------------------------------------------ 
        CONSTRAINTS
   ------------------------------------------------------------------------ */

ALTER TABLE [logEm].[UserRequests] WITH NOCHECK ADD 
    CONSTRAINT [PK_UserRequests] PRIMARY KEY NONCLUSTERED ([UserRequestId]) ON [PRIMARY] 
GO

ALTER TABLE [logEm].[UserRequests] ADD 
    CONSTRAINT [DF_UserRequests_UserRequestId] DEFAULT (NEWID()) FOR [UserRequestId]
GO

/* ------------------------------------------------------------------------ 
        INDICES
   ------------------------------------------------------------------------ */

CREATE NONCLUSTERED INDEX [IX_UserRequests_UserRequest_App_Time_Seq] ON [logEm].[UserRequests] 
(
    [Application]   ASC,
    [TimeUtc]       DESC,
    [Sequence]      DESC
) 
ON [PRIMARY]
GO

COMMIT TRAN