
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
        SCHEMAS (MUST BE FIRST DDL, UNCOMMENT IF FIRST TIME RUNNING)
   ------------------------------------------------------------------------ */


--CREATE SCHEMA logEm;
GO

/* ------------------------------------------------------------------------ 
        TABLES
   ------------------------------------------------------------------------ */

IF EXISTS(
	SELECT 1
	FROM sys.tables
	WHERE name = 'ResourceRequest')
BEGIN
	DROP TABLE logEm.ResourceRequest;
END

IF EXISTS(
	SELECT 1
	FROM sys.tables
	WHERE name = 'Session')
BEGIN
	DROP TABLE logEm.Session;
END

CREATE TABLE [logEm].[Session]
(
    [SessionID]				UNIQUEIDENTIFIER NOT NULL,
    [Application]			NVARCHAR(256) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Host]					NVARCHAR(50) NOT NULL,
	[User]					NVARCHAR(256) NULL,
    [Sequence]				INT IDENTITY (1, 1) NOT NULL,
    [ASPSessionID]			NVARCHAR(256) NOT NULL,
    [SessionBeginTimeUtc]	DATETIME NOT NULL    
) 
ON [PRIMARY]
GO

CREATE TABLE [logEm].[ResourceRequest]
(
    [ResourceRequestID]		UNIQUEIDENTIFIER NOT NULL,
    [Application]			NVARCHAR(256) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Host]					NVARCHAR(50) NOT NULL,
    [User]					NVARCHAR(256) NULL,
    [Sequence]				INT IDENTITY (1, 1) NOT NULL,
    [fkSessionID]			UNIQUEIDENTIFIER NULL,
    [ResourceRequestTimeUtc]DATETIME NOT NULL,
    [RequestAcceptTypes]	NVARCHAR(256) NOT NULL,
    [AnonymousID]			NVARCHAR(256) NULL,
    [ApplicationPath]		NVARCHAR(256) NOT NULL,
    [RequestEncoding]		NVARCHAR(100) NOT NULL,
    [RequestType]			NVARCHAR(100) NOT NULL,
    [RequestCookies]		NVARCHAR(max) NULL,
    [RequestFormValues]		NVARCHAR(max) NULL,
    [RequestHttpMethod]		NVARCHAR(25) NOT NULL,
    [RequestIsAuthenticated]BIT NOT NULL,
    [RequestIsLocal]		BIT NOT NULL,
    [RequestIsSecure]		BIT NOT NULL,
    [RequestQueryString]	NVARCHAR(max) NULL,
    [RequestServerVariables]NVARCHAR(max) NULL,
    [RequestBytes]			INT NOT NULL,
    [URL]					NVARCHAR(max) NOT NULL,
    [UserAgent]				NVARCHAR(256) NOT NULL,
    [UserHost]				NVARCHAR(256) NOT NULL,
    [UserHostName]			NVARCHAR(256) NOT NULL,
    [UserRequestTime]		DATETIME NOT NULL,
    [ResponseEncoding]		NVARCHAR(100) NOT NULL,
    [ResponseType]			NVARCHAR(25) NOT NULL,
    [ResponseCookies]		NVARCHAR(max) NULL,
    [ResponseStatus]		NVARCHAR(25) NOT NULL,
    [ResponseBytes]			INT NOT NULL,
    [HandlerName]			NVARCHAR(256) NULL
) 
ON [PRIMARY]
GO

/* ------------------------------------------------------------------------ 
        CONSTRAINTS
   ------------------------------------------------------------------------ */

ALTER TABLE [logEm].[Session] WITH NOCHECK ADD 
    CONSTRAINT [PK_Session] PRIMARY KEY CLUSTERED ([SessionId]) ON [PRIMARY] 
GO

ALTER TABLE [logEm].[Session] ADD 
    CONSTRAINT [DF_Session_SessionID] DEFAULT (NEWID()) FOR [SessionID]
GO

ALTER TABLE [logEm].[ResourceRequest] WITH NOCHECK ADD 
    CONSTRAINT [PK_ResourceRequest] PRIMARY KEY CLUSTERED ([ResourceRequestId]) ON [PRIMARY] 
GO

ALTER TABLE [logEm].[ResourceRequest] ADD 
    CONSTRAINT [DF_ResourceRequest_ResourceRequestID] DEFAULT (NEWID()) FOR [ResourceRequestID]
GO

ALTER TABLE [logEm].[ResourceRequest] ADD 
	CONSTRAINT [FK_ResourceRequest_Session] FOREIGN KEY ([fkSessionID])
	REFERENCES [logEm].[Session]([SessionID]);
GO

/* ------------------------------------------------------------------------ 
        INDICES
   ------------------------------------------------------------------------ */

CREATE NONCLUSTERED INDEX [IX_ResourceRequest_App_Time_Seq] ON [logEm].[ResourceRequest] 
(
    [Application]			ASC,
    [ResourceRequestTimeUtc]DESC,
    [Sequence]				DESC
) 
ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_Session_App_Time_Seq] ON [logEm].[Session] 
(
    [Application]			ASC,
    [SessionBeginTimeUtc]	DESC,
    [Sequence]				DESC
) 
ON [PRIMARY]
GO

COMMIT TRAN