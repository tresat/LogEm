
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
    [SessionBeginTimeUtc]	DATETIME NOT NULL,
    [ActiveXControls]		BIT NOT NULL,
	[AOL]					BIT NOT NULL,
	[BackgroundSounds]		BIT NOT NULL,
	[Beta]					BIT NOT NULL,
	[Browser]				NVARCHAR(256) NOT NULL,
	[BrowserInfo]			NVARCHAR(max) NULL,
	[CanCombineFormsInDeck]	BIT NOT NULL,
	[CanInitiateVoiceCall]	BIT NOT NULL,
	[CanRenderAfterInputOrSelectElement]	BIT NOT NULL,
	[CanRenderEmptySelects]	BIT NOT NULL,
	[CanRenderInputAndSelectElementsTogether]	BIT NOT NULL,
	[CanRenderMixedSelects]	BIT NOT NULL,
	[CanRenderOneventAndPrevElementsTogether]	BIT NOT NULL,
	[CanRenderPostBackCars]	BIT NOT NULL,
	[CanRenderSetvarZeroWithMultiSelectionList]	BIT NOT NULL,
	[CanSendMail]			BIT NOT NULL,
	[Capabilities]			NVARCHAR(max) NULL,
	[ClrVersion]			NVARCHAR(100) NULL,
	[Cookies]				BIT NOT NULL,
	[Crawler]				BIT NOT NULL,
	[DefaultSubmitButtonLimit]	INT NOT NULL,
	[EcmaScriptVersion]		NVARCHAR(256) NOT NULL,
	[Frames]				BIT NOT NULL,
	[HasBackButton]			BIT NOT NULL,
	[HidesRightAlignedMultiselectScrollbars]	BIT NOT NULL,
	[Id]					NVARCHAR(100) NULL,
	[InputType]				NVARCHAR(100) NULL,
	[IsColor]				BIT NOT NULL,
	[IsMobileDevice]		BIT NOT NULL,
	[JavaApplets]			BIT NOT NULL,
	[JScriptVersion]		NVARCHAR(256) NULL,
	[MajorVersion]			INT NOT NULL,
	[MaximumHrefLength]		INT NOT NULL,
	[MaximumRenderedPageSize]	INT NOT NULL,
	[MaximumSoftkeyLabelLength]	INT NOT NULL,
	[MinorVersion]			DECIMAL NOT NULL,
	[MobileDeviceManufacturer]	NVARCHAR(256) NULL,
	[MobileDeviceModel]		NVARCHAR(256) NULL,	
	[MSDomVersion]			NVARCHAR(256) NULL,
	[NumberOfSoftKeys]		INT NOT NULL,
	[Platform]				NVARCHAR(256) NULL,
	[PreferredImageMime]	NVARCHAR(256) NULL,
	[PreferredRenderingMime]	NVARCHAR(256) NULL,
	[PreferredRenderingType]	NVARCHAR(256) NULL,
	[PreferredRequestEncoding]	NVARCHAR(256) NULL,
	[PreferredResponseEncoding]	NVARCHAR(256) NULL,
	[RendersBreakBeforeWmlSelectAndInput]	BIT NOT NULL,
	[RendersBreaksAfterHtmlLists]	BIT NOT NULL,
	[RendersBreaksAfterWmlAnchor]	BIT NOT NULL,
	[RendersBreaksAfterWmlInput]	BIT NOT NULL,
	[RendersWmlDoAcceptsInline]	BIT NOT NULL,
	[RendersWmlSelectsAsMenuCards]	BIT NOT NULL,
	[RequiredMetaTagNameValue]	NVARCHAR(256) NULL,
	[RequiresAttributeColonSubstitution]	BIT NOT NULL,
	[RequiresContentTypeMetaTag]	BIT NOT NULL,
	[RequiresContentStateInSession]	BIT NOT NULL,
	[RequiresDBCSCharacter]	BIT NOT NULL,
	[RequiresHtmlAdaptiveErrorReporting] BIT NOT NULL,
	[RequiresLeadingPageBreak]	BIT NOT NULL,
	[RequiresNoBreakInFormatting]	BIT NOT NULL,
	[RequiresOutputOptimization]	BIT NOT NULL,
	[RequiresPhoneNumbersAsPlainText]	BIT NOT NULL,
	[RequiresSpecialViewStateEncoding]	BIT NOT NULL,
	[RequiresUniqueFilePathSuffix]	BIT NOT NULL,
	[RequiresUniqueHtmlCheckboxNames]	BIT NOT NULL,
	[RequiresUniqueHtmlInputNames]	BIT NOT NULL,
	[RequiresUrlEncodedPostfieldValues]	BIT NOT NULL,
	[ScreenBitDepth]			INT NOT NULL,
	[ScreenCharactersHeight]	INT NOT NULL,
	[ScreenCharactersWidth]		INT NOT NULL,
	[ScreenPixelsHeight]		INT NOT NULL,
	[ScreenPixelsWidth]			INT NOT NULL,
	[SupportsAccesskeyAttribute]	BIT NOT NULL,
	[SupportsBodyColor]			BIT NOT NULL,
	[SupportsBold]				BIT NOT NULL,
	[SupportsCacheControlMetaTag]	BIT NOT NULL,
	[SupportsCallback]			BIT NOT NULL,
	[SupportsCss]				BIT NOT NULL,
	[SupportsDivAlign]			BIT NOT NULL,
	[SupportsDivNoWrap]			BIT NOT NULL,
	[SupportsEmptyStringInCookieValue]	BIT NOT NULL,
	[SupportsFontColor]			BIT NOT NULL,
	[SupportsFontName]			BIT NOT NULL,
	[SupportsFontSize]			BIT NOT NULL,
	[SupportsImageSubmit]		BIT NOT NULL,
	[SupportsIModeSymbols]		BIT NOT NULL,
	[SupportsInputIStyle]		BIT NOT NULL,
	[SupportsInputMode]			BIT NOT NULL,
	[SupportsItalic]			BIT NOT NULL,
	[SupportsJPhoneMultiMediaAttributes]	BIT NOT NULL,
	[SupportsJPhoneSymbols]		BIT NOT NULL,
	[SupportsQueryStringInFormAction]	BIT NOT NULL,
	[SupportsRedirectWithCookie]BIT NOT NULL,
	[SupportsSelectMultiple]	BIT NOT NULL,
	[SupportsUncheck]			BIT NOT NULL,
	[SupportsXmlHttp]			BIT NOT NULL,
	[Tables]					BIT NOT NULL,
	[Type]						NVARCHAR(256) NULL,
	[UseOptimizedCacheKey]		BIT NOT NULL,
	[VBScript]					BIT NOT NULL,
	[Version]					NVARCHAR(256) NULL,
	[W3CDOMVersion]				NVARCHAR(256) NULL,
	[Win16]						BIT NOT NULL,
	[Win32]						BIT NOT NULL
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