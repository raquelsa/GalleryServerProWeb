SET ANSI_NULLS, ANSI_PADDING, ANSI_WARNINGS, ARITHABORT, CONCAT_NULL_YIELDS_NULL, QUOTED_IDENTIFIER ON;
SET NUMERIC_ROUNDABORT OFF;
GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_gs_Album_Summary]') AND type = 'D')
	ALTER TABLE {schema}[{objectQualifier}gs_Album] DROP CONSTRAINT [DF_gs_Album_Summary]
GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_gs_Album_Title]') AND type = 'D')
	ALTER TABLE {schema}[{objectQualifier}gs_Album] DROP CONSTRAINT [DF_gs_Album_Title]
GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_gs_MediaObject_Title]') AND type = 'D')
	ALTER TABLE {schema}[{objectQualifier}gs_MediaObject] DROP CONSTRAINT [DF_gs_MediaObject_Title]
GO

/* Delete foreign keys to gs_Gallery table */
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}FK_gs_Album_gs_Gallery]') AND parent_object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_Album]'))
	ALTER TABLE {schema}[{objectQualifier}gs_Album] DROP CONSTRAINT [{objectQualifier}FK_gs_Album_gs_Gallery]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}FK_gs_AppError_gs_Gallery]') AND parent_object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_AppError]'))
	ALTER TABLE {schema}[{objectQualifier}gs_AppError] DROP CONSTRAINT [{objectQualifier}FK_gs_AppError_gs_Gallery]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}FK_gs_MimeTypeGallery_gs_Gallery]') AND parent_object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_MimeTypeGallery]'))
	ALTER TABLE {schema}[{objectQualifier}gs_MimeTypeGallery] DROP CONSTRAINT [{objectQualifier}FK_gs_MimeTypeGallery_gs_Gallery]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}FK_gs_MimeTypeGallery_gs_MimeType]') AND parent_object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_MimeTypeGallery]'))
	ALTER TABLE {schema}[{objectQualifier}gs_MimeTypeGallery] DROP CONSTRAINT [{objectQualifier}FK_gs_MimeTypeGallery_gs_MimeType]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}FK_gs_Synchronize_gs_Gallery]') AND parent_object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_Synchronize]'))
	ALTER TABLE {schema}[{objectQualifier}gs_Synchronize] DROP CONSTRAINT [{objectQualifier}FK_gs_Synchronize_gs_Gallery]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}FK_gs_UserGalleryProfile_gs_Gallery]') AND parent_object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_UserGalleryProfile]'))
	ALTER TABLE {schema}[{objectQualifier}gs_UserGalleryProfile] DROP CONSTRAINT [{objectQualifier}FK_gs_UserGalleryProfile_gs_Gallery]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}FK_gs_GallerySetting_gs_Gallery]') AND parent_object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_GallerySetting]'))
	ALTER TABLE {schema}[{objectQualifier}gs_GallerySetting] DROP CONSTRAINT [{objectQualifier}FK_gs_GallerySetting_gs_Gallery]
GO

/* Delete foreign keys to gs_Role table */
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}FK_gs_Role_Album_gs_Role]') AND parent_object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_Role_Album]'))
	ALTER TABLE {schema}[{objectQualifier}gs_Role_Album] DROP CONSTRAINT [{objectQualifier}FK_gs_Role_Album_gs_Role]
GO

/* Delete foreign keys to gs_UserGalleryProfile table */
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}FK_gs_UserGalleryProfile_gs_Gallery]') AND parent_object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_UserGalleryProfile]'))
	ALTER TABLE {schema}[{objectQualifier}gs_UserGalleryProfile] DROP CONSTRAINT [{objectQualifier}FK_gs_UserGalleryProfile_gs_Gallery]
GO

/**************/
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_TYPE = 'PRIMARY KEY' AND TABLE_NAME = '[{objectQualifier}gs_Role]' AND CONSTRAINT_NAME ='[{objectQualifier}PK_gs_Roles]')
	ALTER TABLE {schema}[{objectQualifier}gs_Role] DROP CONSTRAINT [{objectQualifier}PK_gs_Roles];
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_VerifyMinimumRecords]') AND type in (N'P', N'PC'))
	DROP PROCEDURE {schema}[{objectQualifier}gs_VerifyMinimumRecords];
GO

/* Drop tables that may have been created by a previous execution of this script. */
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_AppSetting]') AND type in (N'U'))
	DROP TABLE {schema}[{objectQualifier}gs_AppSetting];
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_BrowserTemplate]') AND type in (N'U'))
	DROP TABLE {schema}[{objectQualifier}gs_BrowserTemplate];
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_GalleryControlSetting]') AND type in (N'U'))
	DROP TABLE {schema}[{objectQualifier}gs_GalleryControlSetting];
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_GallerySetting]') AND type in (N'U'))
	DROP TABLE {schema}[{objectQualifier}gs_GallerySetting];
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_MimeType]') AND type in (N'U'))
	DROP TABLE {schema}[{objectQualifier}gs_MimeType];
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_MimeTypeGallery]') AND type in (N'U'))
	DROP TABLE {schema}[{objectQualifier}gs_MimeTypeGallery];
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_UserGalleryProfile]') AND type in (N'U'))
	DROP TABLE {schema}[{objectQualifier}gs_UserGalleryProfile];
GO

ALTER TABLE {schema}[{objectQualifier}gs_Album] ALTER COLUMN [Summary] NVARCHAR (MAX) NOT NULL;
ALTER TABLE {schema}[{objectQualifier}gs_Album] ALTER COLUMN [Title] NVARCHAR (1000) NOT NULL;
GO

/* Update table gs_Gallery */
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

CREATE TABLE {schema}[{objectQualifier}tmp_ms_xx_gs_Gallery] (
		[GalleryId]   INT             IDENTITY (0, 1) NOT NULL,
		[Description] NVARCHAR (1000) NOT NULL,
		[DateAdded]   DATETIME        NOT NULL
);

ALTER TABLE {schema}[{objectQualifier}tmp_ms_xx_gs_Gallery]
		ADD CONSTRAINT [tmp_ms_xx_clusteredindex_PK_gs_Gallery] PRIMARY KEY CLUSTERED ([GalleryId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF);

IF EXISTS (SELECT TOP 1 1 FROM {schema}[{objectQualifier}gs_Gallery])
		BEGIN
				SET IDENTITY_INSERT {schema}[{objectQualifier}tmp_ms_xx_gs_Gallery] ON;
				INSERT INTO {schema}[{objectQualifier}tmp_ms_xx_gs_Gallery] ([GalleryId], [Description], [DateAdded])
				SELECT   [GalleryId],
								 [Description],
								 [DateAdded]
				FROM     {schema}[{objectQualifier}gs_Gallery]
				ORDER BY [GalleryId] ASC;
				SET IDENTITY_INSERT {schema}[{objectQualifier}tmp_ms_xx_gs_Gallery] OFF;
		END

DROP TABLE {schema}[{objectQualifier}gs_Gallery];

EXECUTE sp_rename N'{schema}[{objectQualifier}tmp_ms_xx_gs_Gallery]', N'{objectQualifier}gs_Gallery';
EXECUTE sp_rename N'{schema}[{objectQualifier}tmp_ms_xx_clusteredindex_PK_gs_Gallery]', N'{objectQualifier}PK_gs_Gallery', N'OBJECT';

COMMIT TRANSACTION;
SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
GO

/* Update table gs_MediaObject */
ALTER TABLE {schema}[{objectQualifier}gs_MediaObject] ALTER COLUMN [ExternalHtmlSource] NVARCHAR (MAX) NOT NULL;
ALTER TABLE {schema}[{objectQualifier}gs_MediaObject] ALTER COLUMN [Title] NVARCHAR (MAX) NOT NULL;
GO

ALTER TABLE {schema}[{objectQualifier}gs_MediaObjectMetadata] ALTER COLUMN [Description] NVARCHAR (200) NOT NULL;
ALTER TABLE {schema}[{objectQualifier}gs_MediaObjectMetadata] ALTER COLUMN [Value] NVARCHAR (MAX) NOT NULL;
GO

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

CREATE TABLE {schema}[{objectQualifier}tmp_ms_xx_gs_Role] (
		[RoleName]                  NVARCHAR (256) NOT NULL,
		[AllowViewAlbumsAndObjects] BIT            NOT NULL,
		[AllowViewOriginalImage]    BIT            NOT NULL,
		[AllowAddChildAlbum]        BIT            NOT NULL,
		[AllowAddMediaObject]       BIT            NOT NULL,
		[AllowEditAlbum]            BIT            NOT NULL,
		[AllowEditMediaObject]      BIT            NOT NULL,
		[AllowDeleteChildAlbum]     BIT            NOT NULL,
		[AllowDeleteMediaObject]    BIT            NOT NULL,
		[AllowSynchronize]          BIT            NOT NULL,
		[HideWatermark]             BIT            NOT NULL,
		[AllowAdministerGallery]    BIT            NOT NULL,
		[AllowAdministerSite]       BIT            NOT NULL
);

ALTER TABLE {schema}[{objectQualifier}tmp_ms_xx_gs_Role]
		ADD CONSTRAINT [tmp_ms_xx_clusteredindex_PK_gs_Role] PRIMARY KEY CLUSTERED ([RoleName] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF);

IF EXISTS (SELECT TOP 1 1 FROM {schema}[{objectQualifier}gs_Role])
		BEGIN
				/* First insert roles for the current gallery */
				INSERT INTO {schema}[{objectQualifier}tmp_ms_xx_gs_Role] ([RoleName], [AllowViewAlbumsAndObjects], [AllowViewOriginalImage], [AllowAddChildAlbum], [AllowAddMediaObject], [AllowEditAlbum], [AllowEditMediaObject], [AllowDeleteChildAlbum], [AllowDeleteMediaObject], [AllowSynchronize], [HideWatermark], [AllowAdministerGallery], [AllowAdministerSite])
				SELECT [RoleName],
								 [AllowViewAlbumsAndObjects],
								 [AllowViewOriginalImage],
								 [AllowAddChildAlbum],
								 [AllowAddMediaObject],
								 [AllowEditAlbum],
								 [AllowEditMediaObject],
								 [AllowDeleteChildAlbum],
								 [AllowDeleteMediaObject],
								 [AllowSynchronize],
								 [HideWatermark],
								 [AllowAdministerSite],
								 [AllowAdministerSite]
				FROM     {schema}[{objectQualifier}gs_Role]
				WHERE [FKGalleryId]={galleryId}
				ORDER BY [RoleName] ASC;

				/* Now insert roles from other galleries, as long as their role name doesn't match one from the previous gallery */
				INSERT INTO {schema}[{objectQualifier}tmp_ms_xx_gs_Role] ([RoleName], [AllowViewAlbumsAndObjects], [AllowViewOriginalImage], [AllowAddChildAlbum], [AllowAddMediaObject], [AllowEditAlbum], [AllowEditMediaObject], [AllowDeleteChildAlbum], [AllowDeleteMediaObject], [AllowSynchronize], [HideWatermark], [AllowAdministerGallery], [AllowAdministerSite])
				SELECT [RoleName],
								 [AllowViewAlbumsAndObjects],
								 [AllowViewOriginalImage],
								 [AllowAddChildAlbum],
								 [AllowAddMediaObject],
								 [AllowEditAlbum],
								 [AllowEditMediaObject],
								 [AllowDeleteChildAlbum],
								 [AllowDeleteMediaObject],
								 [AllowSynchronize],
								 [HideWatermark],
								 [AllowAdministerSite],
								 [AllowAdministerSite]
				FROM     {schema}[{objectQualifier}gs_Role]
				WHERE [FKGalleryId]<>{galleryId}
				  AND [RoleName] NOT IN (SELECT [RoleName] FROM {schema}[{objectQualifier}tmp_ms_xx_gs_Role])
				ORDER BY [RoleName] ASC;
		END

DROP TABLE {schema}[{objectQualifier}gs_Role];

EXECUTE sp_rename N'{schema}[{objectQualifier}tmp_ms_xx_gs_Role]', N'{objectQualifier}gs_Role';
EXECUTE sp_rename N'{schema}[{objectQualifier}tmp_ms_xx_clusteredindex_PK_gs_Role]', N'{objectQualifier}PK_gs_Role', N'OBJECT';

COMMIT TRANSACTION;
SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
GO

/* Create table gs_AppSetting */
CREATE TABLE {schema}[{objectQualifier}gs_AppSetting] (
		[AppSettingId] INT            IDENTITY (1, 1) NOT NULL,
		[SettingName]  VARCHAR (200)  NOT NULL,
		[SettingValue] NVARCHAR (MAX) NOT NULL
);
GO

ALTER TABLE {schema}[{objectQualifier}gs_AppSetting]
		ADD CONSTRAINT [{objectQualifier}PK_gs_AppSetting] PRIMARY KEY CLUSTERED ([AppSettingId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF);
GO

/* Create table gs_BrowserTemplate */
CREATE TABLE {schema}[{objectQualifier}gs_BrowserTemplate] (
		[BrowserTemplateId] INT            IDENTITY (1, 1) NOT NULL,
		[MimeType]          NVARCHAR (100) NOT NULL,
		[BrowserId]         NVARCHAR (50)  NOT NULL,
		[HtmlTemplate]      NVARCHAR (MAX) NOT NULL,
		[ScriptTemplate]    NVARCHAR (MAX) NOT NULL
);
GO

ALTER TABLE {schema}[{objectQualifier}gs_BrowserTemplate]
		ADD CONSTRAINT [{objectQualifier}PK_gs_BrowserTemplate] PRIMARY KEY CLUSTERED ([BrowserTemplateId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF);
GO

ALTER TABLE {schema}[{objectQualifier}gs_BrowserTemplate]
		ADD CONSTRAINT [UC_gs_BrowserTemplate_MimeType_BrowserId] UNIQUE NONCLUSTERED ([MimeType] ASC, [BrowserId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF) ON [PRIMARY];
GO

/* Create table gs_GalleryControlSetting */
CREATE TABLE {schema}[{objectQualifier}gs_GalleryControlSetting] (
		[GalleryControlSettingId] INT            IDENTITY (1, 1) NOT NULL,
		[ControlId]               NVARCHAR (350) NOT NULL,
		[SettingName]             VARCHAR (200)  NOT NULL,
		[SettingValue]            NVARCHAR (MAX) NOT NULL
);
GO

ALTER TABLE {schema}[{objectQualifier}gs_GalleryControlSetting]
		ADD CONSTRAINT [{objectQualifier}PK_gs_GalleryControlSetting] PRIMARY KEY CLUSTERED ([GalleryControlSettingId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF);
GO

ALTER TABLE {schema}[{objectQualifier}gs_GalleryControlSetting]
		ADD CONSTRAINT [UC_gs_GalleryControlSetting_ControlId_SettingName] UNIQUE NONCLUSTERED ([ControlId] ASC, [SettingName] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF) ON [PRIMARY];
GO

/* Create table gs_GallerySetting */
CREATE TABLE {schema}[{objectQualifier}gs_GallerySetting] (
		[GallerySettingId] INT            IDENTITY (1, 1) NOT NULL,
		[FKGalleryId]      INT            NOT NULL,
		[IsTemplate]       BIT            NOT NULL,
		[SettingName]      VARCHAR (200)  NOT NULL,
		[SettingValue]     NVARCHAR (MAX) NOT NULL
);
GO

ALTER TABLE {schema}[{objectQualifier}gs_GallerySetting]
		ADD CONSTRAINT [{objectQualifier}PK_gs_GallerySetting] PRIMARY KEY CLUSTERED ([GallerySettingId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF);
GO

ALTER TABLE {schema}[{objectQualifier}gs_GallerySetting]
		ADD CONSTRAINT [UC_gs_GallerySetting_FKGalleryId_SettingName] UNIQUE NONCLUSTERED ([FKGalleryId] ASC, [SettingName] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF) ON [PRIMARY];
GO

/* Create table gs_MimeType */
CREATE TABLE {schema}[{objectQualifier}gs_MimeType] (
		[MimeTypeId]           INT            IDENTITY (1, 1) NOT NULL,
		[FileExtension]        NVARCHAR (10)  NOT NULL,
		[MimeTypeValue]        NVARCHAR (200) NOT NULL,
		[BrowserMimeTypeValue] NVARCHAR (200) NOT NULL
);
GO

ALTER TABLE {schema}[{objectQualifier}gs_MimeType]
		ADD CONSTRAINT [{objectQualifier}PK_gs_MimeType] PRIMARY KEY CLUSTERED ([MimeTypeId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF);
GO

ALTER TABLE {schema}[{objectQualifier}gs_MimeType]
		ADD CONSTRAINT [UC_gs_MimeType_FileExtension] UNIQUE NONCLUSTERED ([FileExtension] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF) ON [PRIMARY];
GO

/* Create table gs_MimeTypeGallery */
CREATE TABLE {schema}[{objectQualifier}gs_MimeTypeGallery] (
		[MimeTypeGalleryId] INT IDENTITY (1, 1) NOT NULL,
		[FKGalleryId]       INT NOT NULL,
		[FKMimeTypeId]      INT NOT NULL,
		[IsEnabled]         BIT NOT NULL
);
GO

ALTER TABLE {schema}[{objectQualifier}gs_MimeTypeGallery]
		ADD CONSTRAINT [{objectQualifier}PK_gs_MimeTypeGallery] PRIMARY KEY CLUSTERED ([MimeTypeGalleryId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF);
GO

ALTER TABLE {schema}[{objectQualifier}gs_MimeTypeGallery]
		ADD CONSTRAINT [UC_gs_MimeTypeGallery_FKGalleryId_FKMimeTypeId] UNIQUE NONCLUSTERED ([FKGalleryId] ASC, [FKMimeTypeId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF) ON [PRIMARY];
GO

/* Create table gs_UserGalleryProfile */
CREATE TABLE {schema}[{objectQualifier}gs_UserGalleryProfile] (
		[ProfileId]    INT            IDENTITY (1, 1) NOT NULL,
		[UserName]     NVARCHAR (256) NOT NULL,
		[FKGalleryId]  INT            NOT NULL,
		[SettingName]  VARCHAR (200)  NOT NULL,
		[SettingValue] NVARCHAR (MAX) NOT NULL
);
GO

ALTER TABLE {schema}[{objectQualifier}gs_UserGalleryProfile]
		ADD CONSTRAINT [{objectQualifier}PK_gs_UserGalleryProfile] PRIMARY KEY CLUSTERED ([ProfileId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF);
GO

ALTER TABLE {schema}[{objectQualifier}gs_UserGalleryProfile]
		ADD CONSTRAINT [UC_gs_UserGalleryProfile_UserName_FKGalleryId_SettingName] UNIQUE NONCLUSTERED ([UserName] ASC, [FKGalleryId] ASC, [SettingName] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF) ON [PRIMARY];
GO

ALTER TABLE {schema}[{objectQualifier}gs_Album]
		ADD CONSTRAINT [{objectQualifier}DF_gs_Album_Summary] DEFAULT ('') FOR [Summary];
GO

ALTER TABLE {schema}[{objectQualifier}gs_Album]
		ADD CONSTRAINT [{objectQualifier}DF_gs_Album_Title] DEFAULT ('') FOR [Title];
GO

ALTER TABLE {schema}[{objectQualifier}gs_MediaObject]
		ADD CONSTRAINT [{objectQualifier}DF_gs_MediaObject_Title] DEFAULT ('') FOR [Title];
GO

ALTER TABLE {schema}[{objectQualifier}gs_Album] WITH NOCHECK
		ADD CONSTRAINT [{objectQualifier}FK_gs_Album_gs_Gallery] FOREIGN KEY ([FKGalleryId]) REFERENCES {schema}[{objectQualifier}gs_Gallery] ([GalleryId]) ON DELETE CASCADE ON UPDATE NO ACTION;
GO

ALTER TABLE {schema}[{objectQualifier}gs_AppError] WITH NOCHECK
		ADD CONSTRAINT [{objectQualifier}FK_gs_AppError_gs_Gallery] FOREIGN KEY ([FKGalleryId]) REFERENCES {schema}[{objectQualifier}gs_Gallery] ([GalleryId]) ON DELETE CASCADE ON UPDATE NO ACTION;
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}FK_gs_GallerySetting_gs_Gallery]') AND parent_object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_GallerySetting]'))
	ALTER TABLE {schema}[{objectQualifier}gs_GallerySetting] WITH NOCHECK
		ADD CONSTRAINT [{objectQualifier}FK_gs_GallerySetting_gs_Gallery] FOREIGN KEY ([FKGalleryId]) REFERENCES {schema}[{objectQualifier}gs_Gallery] ([GalleryId]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}FK_gs_MimeTypeGallery_gs_Gallery]') AND parent_object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_MimeTypeGallery]'))
	ALTER TABLE {schema}[{objectQualifier}gs_MimeTypeGallery] WITH NOCHECK
		ADD CONSTRAINT [{objectQualifier}FK_gs_MimeTypeGallery_gs_Gallery] FOREIGN KEY ([FKGalleryId]) REFERENCES {schema}[{objectQualifier}gs_Gallery] ([GalleryId]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}FK_gs_MimeTypeGallery_gs_MimeType]') AND parent_object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_MimeTypeGallery]'))
	ALTER TABLE {schema}[{objectQualifier}gs_MimeTypeGallery] WITH NOCHECK
		ADD CONSTRAINT [{objectQualifier}FK_gs_MimeTypeGallery_gs_MimeType] FOREIGN KEY ([FKMimeTypeId]) REFERENCES {schema}[{objectQualifier}gs_MimeType] ([MimeTypeId]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}FK_gs_UserGalleryProfile_gs_Gallery]') AND parent_object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_UserGalleryProfile]'))
	ALTER TABLE {schema}[{objectQualifier}gs_UserGalleryProfile] WITH NOCHECK
		ADD CONSTRAINT [{objectQualifier}FK_gs_UserGalleryProfile_gs_Gallery] FOREIGN KEY ([FKGalleryId]) REFERENCES {schema}[{objectQualifier}gs_Gallery] ([GalleryId]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}FK_gs_Role_Album_gs_Role]') AND parent_object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_Role_Album]'))
	ALTER TABLE {schema}[{objectQualifier}gs_Role_Album] WITH NOCHECK
		ADD CONSTRAINT [{objectQualifier}FK_gs_Role_Album_gs_Role] FOREIGN KEY ([FKRoleName]) REFERENCES {schema}[{objectQualifier}gs_Role] ([RoleName]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}FK_gs_Synchronize_gs_Gallery]') AND parent_object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_Synchronize]'))
	ALTER TABLE {schema}[{objectQualifier}gs_Synchronize] WITH NOCHECK
		ADD CONSTRAINT [{objectQualifier}FK_gs_Synchronize_gs_Gallery] FOREIGN KEY ([FKGalleryId]) REFERENCES {schema}[{objectQualifier}gs_Gallery] ([GalleryId]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

ALTER PROCEDURE {schema}[{objectQualifier}gs_AlbumDelete]
	@AlbumId int
AS
SET NOCOUNT ON
/* Delete the specified album and its objects, including any child albums.  Instead of using 
built-in cascading delete features of the database, we delete all objects manually. This is
primarily because SQL Server does not support cascade delete for hierarchal tables, which is
how album data is stored (the AlbumParentId field refers to the AlbumId field).*/

/* First, build a table containing this album ID and all child album IDs. */
CREATE TABLE #a (aid int, apid int, processed int)

/* Insert this album into our temporary table. */
INSERT #a 
	SELECT AlbumId, AlbumParentId, 0
	FROM {schema}[{objectQualifier}gs_Album] WHERE AlbumId = @AlbumId

/* Set up a loop where we insert the children of the first album, and their children, and so on, until no 
children are left. The end result is that the table is filled with info about @AlbumId and all his descendents.
The processed field in #a represents the # of levels from the bottom. Thus the records
with the MAX processed value is @AlbumId, and the records with the MIN level (should always be 1)
represent the most distant descendents. */
WHILE EXISTS (SELECT * FROM #a WHERE processed = 0)
BEGIN
	/* Insert the children of all albums in #a into the table. We use the 'processed = 0' criterion because we
	only want to get the children once. Each loop increases the value of the processed field by one.  */
	INSERT #a SELECT AlbumId, AlbumParentId, -1
		FROM {schema}[{objectQualifier}gs_Album] WHERE AlbumParentId IN 
			(SELECT aid FROM #a WHERE processed = 0)
	
	/* Increment the processed value to preserve the heiarchy of albums. */
	UPDATE #a SET processed = processed + 1
END

/* At this point #a contains info about @AlbumId and all his descendents. Delete all media objects 
and roles associated with these albums, and then delete the albums. */
BEGIN TRAN
	DELETE {schema}[{objectQualifier}gs_MediaObject] WHERE FKAlbumId IN (SELECT aid FROM #a)
	
	/* Only delete role_album and album records that are not associated with the root album. */
	DELETE {schema}[{objectQualifier}gs_Role_Album] WHERE FKAlbumId IN (SELECT aid FROM #a WHERE apid <> 0)
	
	DELETE {schema}[{objectQualifier}gs_Album] WHERE AlbumId IN (SELECT aid FROM #a WHERE apid <> 0)
COMMIT TRAN
GO

ALTER PROCEDURE {schema}[{objectQualifier}gs_AlbumInsert]
(@AlbumParentId int, @GalleryId int, @Title nvarchar(1000), @DirectoryName nvarchar(255),
 @Summary nvarchar(max), @ThumbnailMediaObjectId int, @Seq int, 
 @DateStart datetime, @DateEnd datetime, @CreatedBy nvarchar(256), @DateAdded datetime, 
 @LastModifiedBy nvarchar(256), @DateLastModified datetime, @OwnedBy nvarchar(256),
 @OwnerRoleName nvarchar(256), @IsPrivate bit, @Identity int OUT)
AS
/* Insert a new album. */
INSERT {schema}[{objectQualifier}gs_Album] (AlbumParentId, FKGalleryId, Title, DirectoryName, 
 Summary, ThumbnailMediaObjectId, Seq, DateStart, DateEnd, 
 CreatedBy, DateAdded, LastModifiedBy, DateLastModified, OwnedBy, 
 OwnerRoleName, IsPrivate)
VALUES (@AlbumParentId, @GalleryId, @Title, @DirectoryName, 
 @Summary, @ThumbnailMediaObjectId, @Seq, @DateStart, @DateEnd, 
 @CreatedBy, @DateAdded, @LastModifiedBy, @DateLastModified, @OwnedBy, 
 @OwnerRoleName, @IsPrivate)

SET @Identity = SCOPE_IDENTITY()
GO

ALTER PROCEDURE {schema}[{objectQualifier}gs_AlbumUpdate]
(@AlbumId int, @FKGalleryId int, @AlbumParentId int, @Title nvarchar(1000), @DirectoryName nvarchar(255),
 @Summary nvarchar(max), @ThumbnailMediaObjectId int,  @Seq int, 
 @DateStart datetime, @DateEnd datetime, @LastModifiedBy nvarchar(256), 
 @DateLastModified datetime, @OwnedBy nvarchar(256), @OwnerRoleName nvarchar(256), @IsPrivate bit)
 
AS
SET NOCOUNT ON

UPDATE {schema}[{objectQualifier}gs_Album]
SET
	FKGalleryId = @FKGalleryId,
	AlbumParentId = @AlbumParentId,
	Title = @Title,
	DirectoryName = @DirectoryName,
	Summary = @Summary,
	ThumbnailMediaObjectId = @ThumbnailMediaObjectId,
	Seq = @Seq,
	DateStart = @DateStart,
	DateEnd = @DateEnd,
	LastModifiedBy = @LastModifiedBy,
	DateLastModified = @DateLastModified,
	OwnedBy = @OwnedBy,
	OwnerRoleName = @OwnerRoleName,
	IsPrivate = @IsPrivate
WHERE (AlbumId = @AlbumId)

RETURN
GO

ALTER PROCEDURE {schema}[{objectQualifier}gs_AppErrorDeleteAll]
(
	@GalleryId int
)
AS
/* Delete all application errors for the gallery, including errors not associated with any gallery */
DELETE {schema}[{objectQualifier}gs_AppError]
WHERE FKGalleryId IN (@GalleryId, -2147483648);
GO

ALTER PROCEDURE {schema}[{objectQualifier}gs_AppErrorSelect]

AS
SET NOCOUNT ON;

SELECT
	AppErrorId, FKGalleryId, [TimeStamp], ExceptionType, [Message], [Source], TargetSite, StackTrace, ExceptionData, 
	InnerExType, InnerExMessage, InnerExSource, InnerExTargetSite, InnerExStackTrace, InnerExData, Url, 
	FormVariables, Cookies, SessionVariables, ServerVariables
FROM {schema}[{objectQualifier}gs_AppError]
GO

ALTER PROCEDURE {schema}[{objectQualifier}gs_DeleteData]
(	@DeleteMembershipData bit, @DeleteGalleryData bit	)

AS
SET NOCOUNT ON

IF @DeleteMembershipData = 1
BEGIN
	DELETE FROM dbo.[aspnet_UsersInRoles]
	DELETE FROM dbo.[aspnet_Profile]
	DELETE FROM dbo.[aspnet_Membership]
	DELETE FROM dbo.[aspnet_Users]
	DELETE FROM dbo.[aspnet_Roles]
	DELETE FROM dbo.[aspnet_Applications]
END

IF @DeleteGalleryData = 1 
BEGIN
	DELETE FROM {schema}[{objectQualifier}gs_UserGalleryProfile]
	DELETE FROM {schema}[{objectQualifier}gs_MediaObjectMetadata]
	DELETE FROM {schema}[{objectQualifier}gs_MediaObject]
	DELETE FROM {schema}[{objectQualifier}gs_Role_Album]
	DELETE FROM {schema}[{objectQualifier}gs_Role]
	DELETE FROM {schema}[{objectQualifier}gs_Album]
	DELETE FROM {schema}[{objectQualifier}gs_AppError]
	DELETE FROM {schema}[{objectQualifier}gs_AppSetting]
	DELETE FROM {schema}[{objectQualifier}gs_GalleryControlSetting]
	DELETE FROM {schema}[{objectQualifier}gs_GallerySetting]
	DELETE FROM {schema}[{objectQualifier}gs_BrowserTemplate]
	DELETE FROM {schema}[{objectQualifier}gs_MimeTypeGallery]
	DELETE FROM {schema}[{objectQualifier}gs_MimeType]
	DELETE FROM {schema}[{objectQualifier}gs_Synchronize]
	DELETE FROM {schema}[{objectQualifier}gs_Gallery]
END

RETURN
GO

ALTER PROCEDURE {schema}[{objectQualifier}gs_ExportGalleryData]
AS
SET NOCOUNT ON

SELECT AlbumId, FKGalleryId, AlbumParentId, Title, DirectoryName, Summary, ThumbnailMediaObjectId, Seq, DateStart, DateEnd, DateAdded, CreatedBy, 
 LastModifiedBy, DateLastModified, OwnedBy, OwnerRoleName, IsPrivate
FROM {schema}[{objectQualifier}gs_Album]

SELECT GalleryId, Description, DateAdded
FROM {schema}[{objectQualifier}gs_Gallery]

SELECT MediaObjectId, FKAlbumId, Title, HashKey, ThumbnailFilename, ThumbnailWidth, ThumbnailHeight, ThumbnailSizeKB, OptimizedFilename, OptimizedWidth, 
 OptimizedHeight, OptimizedSizeKB, OriginalFilename, OriginalWidth, OriginalHeight, OriginalSizeKB, ExternalHtmlSource, ExternalType, Seq, CreatedBy, 
 DateAdded, LastModifiedBy, DateLastModified, IsPrivate
FROM {schema}[{objectQualifier}gs_MediaObject]

SELECT MediaObjectMetadataId, FKMediaObjectId, MetadataNameIdentifier, Description, Value
FROM {schema}[{objectQualifier}gs_MediaObjectMetadata]

SELECT RoleName, AllowViewAlbumsAndObjects, AllowViewOriginalImage, AllowAddChildAlbum, AllowAddMediaObject, AllowEditAlbum, 
 AllowEditMediaObject, AllowDeleteChildAlbum, AllowDeleteMediaObject, AllowSynchronize, HideWatermark, AllowAdministerGallery,
 AllowAdministerSite
FROM {schema}[{objectQualifier}gs_Role]

SELECT FKRoleName, FKAlbumId
FROM {schema}[{objectQualifier}gs_Role_Album]

SELECT AppErrorId, FKGalleryId, TimeStamp, ExceptionType, Message, Source, TargetSite, StackTrace, ExceptionData, InnerExType, InnerExMessage, InnerExSource, 
 InnerExTargetSite, InnerExStackTrace, InnerExData, Url, FormVariables, Cookies, SessionVariables, ServerVariables
FROM {schema}[{objectQualifier}gs_AppError]

SELECT AppSettingId, SettingName, SettingValue
FROM {schema}[{objectQualifier}gs_AppSetting]

SELECT GalleryControlSettingId, ControlId, SettingName, SettingValue
FROM {schema}[{objectQualifier}gs_GalleryControlSetting]

SELECT GallerySettingId, FKGalleryId, IsTemplate, SettingName, SettingValue
FROM {schema}[{objectQualifier}gs_GallerySetting]

SELECT BrowserTemplateId, MimeType, BrowserId, HtmlTemplate, ScriptTemplate
FROM {schema}[{objectQualifier}gs_BrowserTemplate]

SELECT MimeTypeId, FileExtension, MimeTypeValue, BrowserMimeTypeValue
FROM {schema}[{objectQualifier}gs_MimeType]

SELECT MimeTypeGalleryId, FKGalleryId, FKMimeTypeId, IsEnabled
FROM {schema}[{objectQualifier}gs_MimeTypeGallery]

SELECT ProfileId, UserName, FKGalleryId, SettingName, SettingValue
FROM {schema}[{objectQualifier}gs_UserGalleryProfile]

RETURN
GO

ALTER PROCEDURE {schema}[{objectQualifier}gs_GallerySelect]

AS
SET NOCOUNT ON

SELECT GalleryId, Description, DateAdded
FROM {schema}[{objectQualifier}gs_Gallery]
WHERE GalleryId > -2147483648;

RETURN
GO

ALTER PROCEDURE {schema}[{objectQualifier}gs_MediaObjectInsert]
(@HashKey char(47), @FKAlbumId int, @ThumbnailFilename nvarchar(255), 
 @ThumbnailWidth int, @ThumbnailHeight int,
 @ThumbnailSizeKB int, @OptimizedFilename nvarchar(255), 
 @OptimizedWidth int, @OptimizedHeight int,
 @OptimizedSizeKB int, @OriginalFilename nvarchar(255),	 
 @OriginalWidth int, @OriginalHeight int, @OriginalSizeKB int, 
 @ExternalHtmlSource nvarchar(max), @ExternalType nvarchar(15),
 @Title nvarchar(max), @Seq int, @CreatedBy nvarchar(256), @DateAdded datetime, 
 @LastModifiedBy nvarchar(256), @DateLastModified datetime, @IsPrivate bit,
 @Identity int OUT)
AS

/* Insert media object information. */
 INSERT {schema}[{objectQualifier}gs_MediaObject] (HashKey, FKAlbumId, ThumbnailFilename, ThumbnailWidth, ThumbnailHeight,
 ThumbnailSizeKB, OptimizedFilename, OptimizedWidth, OptimizedHeight, OptimizedSizeKB,
 OriginalFilename, OriginalWidth, OriginalHeight, OriginalSizeKB, ExternalHtmlSource, ExternalType, Title, Seq, CreatedBy, 
 DateAdded, LastModifiedBy, DateLastModified, IsPrivate)
VALUES (@HashKey, @FKAlbumId, @ThumbnailFilename, @ThumbnailWidth, @ThumbnailHeight,
 @ThumbnailSizeKB, @OptimizedFilename, @OptimizedWidth, @OptimizedHeight, @OptimizedSizeKB,
 @OriginalFilename, @OriginalWidth, @OriginalHeight, @OriginalSizeKB, @ExternalHtmlSource, @ExternalType, @Title, @Seq, @CreatedBy, 
 @DateAdded, @LastModifiedBy, @DateLastModified, @IsPrivate)
 
SET @Identity = SCOPE_IDENTITY()
GO

ALTER PROCEDURE {schema}[{objectQualifier}gs_MediaObjectMetadataInsert]
(@FKMediaObjectId int, @MetadataNameIdentifier int, @Description nvarchar(200), @Value nvarchar(max),
 @Identity int OUT)
AS
/* Insert a new media object meta data item */
INSERT {schema}[{objectQualifier}gs_MediaObjectMetadata] (FKMediaObjectId, MetadataNameIdentifier, Description, Value)
VALUES (@FKMediaObjectId, @MetadataNameIdentifier, @Description, @Value)

SET @Identity = SCOPE_IDENTITY()
GO

ALTER PROCEDURE {schema}[{objectQualifier}gs_MediaObjectMetadataSelect]
(
	@MediaObjectId int
)
AS
SET NOCOUNT ON

SELECT
	md.MediaObjectMetadataId, md.FKMediaObjectId, md.MetadataNameIdentifier, md.Description, md.Value
FROM {schema}[{objectQualifier}gs_MediaObjectMetadata] md JOIN {schema}[{objectQualifier}gs_MediaObject] mo ON md.FKMediaObjectId = mo.MediaObjectId
WHERE md.FKMediaObjectId = @MediaObjectId

RETURN
GO

ALTER PROCEDURE {schema}[{objectQualifier}gs_MediaObjectMetadataUpdate]
(@FKMediaObjectId int, @MetadataNameIdentifier int, @Description nvarchar(200), @Value nvarchar(max),
 @MediaObjectMetadataId int)
AS
SET NOCOUNT ON
/* Update a new media object meta data item */
UPDATE {schema}[{objectQualifier}gs_MediaObjectMetadata]
SET FKMediaObjectId = @FKMediaObjectId,
 MetadataNameIdentifier = @MetadataNameIdentifier,
 Description = @Description,
 Value = @Value
WHERE MediaObjectMetadataId = @MediaObjectMetadataId
GO

ALTER PROCEDURE {schema}[{objectQualifier}gs_MediaObjectSelect]
(
	@MediaObjectId int
)
AS
SET NOCOUNT ON

SELECT
	MediaObjectId, FKAlbumId, Title, HashKey, ThumbnailFilename, ThumbnailWidth, ThumbnailHeight, 
	ThumbnailSizeKB, OptimizedFilename, OptimizedWidth, OptimizedHeight, OptimizedSizeKB, 
	OriginalFilename, OriginalWidth, OriginalHeight, OriginalSizeKB, ExternalHtmlSource, ExternalType, Seq, 
	CreatedBy, DateAdded, LastModifiedBy, DateLastModified, IsPrivate
FROM {schema}[{objectQualifier}gs_MediaObject]
WHERE MediaObjectId = @MediaObjectId

RETURN
GO

ALTER PROCEDURE {schema}[{objectQualifier}gs_MediaObjectSelectHashKeys]

AS
SET NOCOUNT ON

SELECT HashKey
FROM {schema}[{objectQualifier}gs_MediaObject]

RETURN
GO

ALTER PROCEDURE {schema}[{objectQualifier}gs_MediaObjectUpdate]
(
 @MediaObjectId int, @HashKey char(47), @FKAlbumId int, 
 @ThumbnailFilename nvarchar(255), @ThumbnailWidth int, @ThumbnailHeight int, @ThumbnailSizeKB int,
 @OriginalFilename nvarchar(255),	@OriginalWidth int, @OriginalHeight int, @OriginalSizeKB int, 
 @OptimizedFilename nvarchar(255),	@OptimizedWidth int, @OptimizedHeight int, @OptimizedSizeKB int, 
 @ExternalHtmlSource nvarchar(max), @ExternalType nvarchar(15),
 @Title nvarchar(max), @Seq int, @LastModifiedBy nvarchar(256), @DateLastModified datetime, @IsPrivate bit
)
AS
SET NOCOUNT ON

/* Update the media object. */
UPDATE {schema}[{objectQualifier}gs_MediaObject]
SET HashKey = @HashKey, FKAlbumId = @FKAlbumId,
 ThumbnailFilename = @ThumbnailFilename, ThumbnailWidth = @ThumbnailWidth, 
 ThumbnailHeight = @ThumbnailHeight, ThumbnailSizeKB = @ThumbnailSizeKB,
 OptimizedFilename = @OptimizedFilename, OptimizedWidth = @OptimizedWidth,
 OptimizedHeight = @OptimizedHeight, OptimizedSizeKB = @OptimizedSizeKB, 
 OriginalFilename = @OriginalFilename, OriginalWidth = @OriginalWidth,
 OriginalHeight = @OriginalHeight, OriginalSizeKB = @OriginalSizeKB, 
 ExternalHtmlSource = @ExternalHtmlSource, ExternalType = @ExternalType,
 Title = @Title, Seq = @Seq, LastModifiedBy = @LastModifiedBy, 
 DateLastModified = @DateLastModified, IsPrivate = @IsPrivate
WHERE MediaObjectId = @MediaObjectId
GO

ALTER PROCEDURE {schema}[{objectQualifier}gs_Role_AlbumSelectAllAlbumsByRoleName]
(	@RoleName nvarchar(256), @GalleryId int )

AS
SET NOCOUNT ON

/* Retrieve all the album IDs that are affected by the specified role name. The album IDs that are stored in
	 the gs_Role_Album table only hold the highest ranking album ID. */
	 
/* If the role is applied to the root album, then we can just return all albums in the gallery. This is 
much cheaper than drilling down by album.  */
DECLARE @RootAlbumId int
SELECT @RootAlbumId = AlbumId FROM {schema}[{objectQualifier}gs_Album] WHERE AlbumParentId = 0 AND FKGalleryId = @GalleryId
 
IF EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_Role_Album] WHERE FKRoleName = @RoleName AND FKAlbumId = @RootAlbumId)
BEGIN
	SELECT AlbumId FROM {schema}[{objectQualifier}gs_Album] WHERE FKGalleryId = @GalleryId
END
ELSE
BEGIN
	/* The role applies to an album or albums below the root album, so we need to drill down and retrieve all
	 the children. Start by creating a temporary table to hold our data. */
	DECLARE @AlbumList table
		(AlbumId int not null,
		AlbumParentId int not null,
		AlbumDepth int not null)

	/* Insert the top level album IDs. */
	INSERT @AlbumList (AlbumId, AlbumParentId, AlbumDepth)
	SELECT FKAlbumId, 0, 1
	FROM {schema}[{objectQualifier}gs_Role_Album] ra INNER JOIN {schema}[{objectQualifier}gs_Album] a ON ra.FKAlbumId = a.AlbumId
	WHERE (ra.FKRoleName = @RoleName) AND (a.FKGalleryId = @GalleryId)

	/* Continue drilling down, level by level, until we reach a level where there are no more child albums. */
	WHILE (@@rowcount > 0) BEGIN
		INSERT @AlbumList (AlbumId, AlbumParentId, AlbumDepth)
		SELECT a.AlbumId, a.AlbumParentId, al.AlbumDepth + 1
		FROM {schema}[{objectQualifier}gs_Album] a JOIN @AlbumList al ON a.AlbumParentId = al.AlbumId
		WHERE al.AlbumDepth = (SELECT MAX(AlbumDepth) FROM @AlbumList)
	END

	/* Retrieve the list of album IDs. */
	SELECT AlbumId
	FROM @AlbumList
END

RETURN
GO

ALTER PROCEDURE {schema}[{objectQualifier}gs_Role_AlbumSelectRootAlbumsByRoleName]
(	@RoleName nvarchar(256) )

AS
SET NOCOUNT ON

SELECT FKAlbumId
FROM {schema}[{objectQualifier}gs_Role_Album] 
WHERE FKRoleName = @RoleName

RETURN
GO

ALTER PROCEDURE {schema}[{objectQualifier}gs_RoleDelete]
( @RoleName nvarchar(256) )
AS
/* Delete a gallery server role. This procedure only deletes it from the custom gallery server tables,
not the ASP.NET role membership table(s). The web application code that invokes this procedure also
uses the standard ASP.NET technique to delete the role from the membership table(s). */

-- First delete the records from the role/album association table.
DELETE {schema}[{objectQualifier}gs_Role_Album]
WHERE FKRoleName = @RoleName

-- Finally delete the role.
DELETE {schema}[{objectQualifier}gs_Role]
WHERE RoleName = @RoleName
GO

ALTER PROCEDURE {schema}[{objectQualifier}gs_RoleInsert]
(
	@RoleName nvarchar(256), @AllowViewAlbumsAndObjects bit, @AllowViewOriginalImage bit,
	@AllowAddChildAlbum bit, @AllowAddMediaObject bit, @AllowEditAlbum bit, @AllowEditMediaObject bit,
	@AllowDeleteChildAlbum bit, @AllowDeleteMediaObject bit, @AllowSynchronize bit, @HideWatermark bit,
	@AllowAdministerGallery bit, @AllowAdministerSite bit
)
AS
SET NOCOUNT ON

INSERT {schema}[{objectQualifier}gs_Role] (RoleName, AllowViewAlbumsAndObjects, AllowViewOriginalImage, AllowAddChildAlbum,
	AllowAddMediaObject, AllowEditAlbum, AllowEditMediaObject, AllowDeleteChildAlbum, AllowDeleteMediaObject, 
	AllowSynchronize, HideWatermark, AllowAdministerGallery, AllowAdministerSite)
VALUES (@RoleName, @AllowViewAlbumsAndObjects, @AllowViewOriginalImage, @AllowAddChildAlbum,
	@AllowAddMediaObject, @AllowEditAlbum, @AllowEditMediaObject, @AllowDeleteChildAlbum, @AllowDeleteMediaObject, 
	@AllowSynchronize, @HideWatermark, @AllowAdministerGallery, @AllowAdministerSite)

RETURN
GO

ALTER PROCEDURE {schema}[{objectQualifier}gs_RoleSelect]

AS
SET NOCOUNT ON

SELECT RoleName, AllowViewAlbumsAndObjects, AllowViewOriginalImage, AllowAddChildAlbum,
	AllowAddMediaObject, AllowEditAlbum, AllowEditMediaObject, AllowDeleteChildAlbum, 
	AllowDeleteMediaObject, AllowSynchronize, HideWatermark, AllowAdministerGallery, 
	AllowAdministerSite
FROM {schema}[{objectQualifier}gs_Role]

RETURN
GO

ALTER PROCEDURE {schema}[{objectQualifier}gs_RoleUpdate] 
(
	@RoleName nvarchar(256), @AllowViewAlbumsAndObjects bit, @AllowViewOriginalImage bit,
	@AllowAddChildAlbum bit, @AllowAddMediaObject bit, @AllowEditAlbum bit, @AllowEditMediaObject bit,
	@AllowDeleteChildAlbum bit, @AllowDeleteMediaObject bit, @AllowSynchronize bit, @HideWatermark bit,
	@AllowAdministerGallery bit, @AllowAdministerSite bit
)
AS
SET NOCOUNT, XACT_ABORT ON

/* Update the specified role. If the role does not exist, assume it is new and call the insert proc. */
BEGIN TRAN
IF EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_Role] WITH (UPDLOCK, HOLDLOCK) WHERE RoleName = @RoleName)
BEGIN
	UPDATE {schema}[{objectQualifier}gs_Role]
	SET AllowViewAlbumsAndObjects = @AllowViewAlbumsAndObjects,
		AllowViewOriginalImage = @AllowViewOriginalImage,
		AllowAddChildAlbum = @AllowAddChildAlbum,
		AllowAddMediaObject = @AllowAddMediaObject,
		AllowEditAlbum = @AllowEditAlbum,
		AllowEditMediaObject = @AllowEditMediaObject,
		AllowDeleteChildAlbum = @AllowDeleteChildAlbum,
		AllowDeleteMediaObject = @AllowDeleteMediaObject,
		AllowSynchronize = @AllowSynchronize,
		HideWatermark = @HideWatermark,
		AllowAdministerGallery = @AllowAdministerGallery,
		AllowAdministerSite = @AllowAdministerSite
	WHERE RoleName = @RoleName
END
ELSE
BEGIN
	EXECUTE {schema}[{objectQualifier}gs_RoleInsert] 
		@RoleName,
		@AllowViewAlbumsAndObjects,
		@AllowViewOriginalImage,
		@AllowAddChildAlbum,
		@AllowAddMediaObject,
		@AllowEditAlbum,
		@AllowEditMediaObject,
		@AllowDeleteChildAlbum,
		@AllowDeleteMediaObject,
		@AllowSynchronize,
		@HideWatermark,
		@AllowAdministerGallery,
		@AllowAdministerSite
END
COMMIT

RETURN
GO

ALTER PROCEDURE {schema}[{objectQualifier}gs_SearchGallery]
(     @SearchTerms nvarchar(4000), @GalleryId int)
AS
SET NOCOUNT ON

/* Search for albums and media objects that match the specified search
terms. The album or media object must match
ALL search terms to be considered a match. There is no 'OR' capability. For
albums, the Title and Summary columns 
are searched. For media objects, the Title and OriginalFilename columns and
all metadata in the MediaObjectMetadata 
tables is searched.

Inputs:
@SearchTerms - A comma-delimited set of search terms. May include spaces.
Ex: "cat,dog", "cat videos, dog videos"
	Multiple words in a single search term (such as "cat videos" in the
previous example) are treated as a phrase
	that must be matched, exactly like how Google treats phrases contained in
quotation marks. That is, "cat videos"
	requires the phrase "cat videos" to appear somewhere in the data, and it
will not match "cat and dog videos"
	(to match "cat and dog videos", you can use "cat,videos").
@GalleryId - The ID of the gallery to search.

Returns:
Returns a set of records with two columns:
gotype - A single character containing either 'a' for album or 'm' for media
object. Indicates whether the id
				 stored in the second column is an album or media object.
id - The ID of a matching album or media object.

Algorithm:
The search follows these steps:
1. Create a temporary table #searchTerms and insert the search terms into
it, prepending and appending the wildcard
	 character (%). Ex: If @SearchTerms = "cat videos,dog,fish", #searchTerms
will get 3 records: %cat videos%,
	 %dog%, %fish%.
2. Create a second temporary table #searchResults to hold intermediate
search results.
3. Insert into #searchResults all albums where the title matches one of more
search terms. There will be one record
	 inserted for each matching search term. Ex: If @SearchTerms = "cat
videos,dog,fish" and the album title =
	 "My dog and cat videos", there will be two records inserted into
#searchResults, one with matchingSearchTerm =
	 "%cat videos%" and the other "%dog%" (gotype='a', id=album ID,
fieldname='Album.Title' for both).
4. Repeat the above step for other fields: Album.Summary, MediaObject.Title,
MediaObject.OriginalFilename, and
	 all media object metadata for each media object
5. Select those records from #searchResults where we made a successful match
for EVERY search term for each album or
	 media object.
	 
Note: The fieldname column in #searchResults is not used except for manual
debugging purposes. This column can
be removed if desired. 
*/

CREATE TABLE #searchTerms (searchTerm nvarchar(4000) COLLATE database_default)
CREATE TABLE #searchResults (
			gotype char(1), 
			id int, 
			fieldname nvarchar(50) COLLATE database_default, 
			matchingSearchTerm nvarchar(3000) COLLATE database_default)

INSERT #searchTerms
SELECT '%' + nstr + '%' FROM
{schema}[{objectQualifier}gs_func_convert_string_array_to_table](@SearchTerms, ',')

-- Search album title
INSERT #searchResults
SELECT 'a', a.AlbumId, 'Album.Title', '%' + SUBSTRING(a.Title,
PATINDEX(#searchTerms.searchTerm, a.Title),
LEN(#searchTerms.searchTerm) - 2) + '%'
 FROM {schema}[{objectQualifier}gs_Album] a CROSS JOIN #searchTerms
 WHERE a.FKGalleryId = @GalleryId AND a.Title LIKE #searchTerms.searchTerm

-- Search album summary
INSERT #searchResults
SELECT 'a', a.AlbumId, 'Album.Summary', '%' + SUBSTRING(a.Summary,
PATINDEX(#searchTerms.searchTerm, a.Summary), LEN(#searchTerms.searchTerm) -
2) + '%'
 FROM {schema}[{objectQualifier}gs_Album] a CROSS JOIN #searchTerms
 WHERE a.FKGalleryId = @GalleryId AND a.Summary LIKE #searchTerms.searchTerm

-- Search media object title
INSERT #searchResults
SELECT 'm', m.MediaObjectId, 'MediaObject.Title', '%' +
SUBSTRING(m.Title, PATINDEX(#searchTerms.searchTerm,
m.Title), LEN(#searchTerms.searchTerm) - 2) + '%'
 FROM {schema}[{objectQualifier}gs_MediaObject] m JOIN {schema}[{objectQualifier}gs_Album] a
 ON a.AlbumId = m.FKAlbumId CROSS JOIN #searchTerms
 WHERE a.FKGalleryId = @GalleryId AND m.Title LIKE
#searchTerms.searchTerm

-- Search media object original filename
INSERT #searchResults
SELECT 'm', m.MediaObjectId, 'MediaObject.OriginalFilename',
'%' + SUBSTRING(m.OriginalFilename,
PATINDEX(#searchTerms.searchTerm, m.OriginalFilename),
LEN(#searchTerms.searchTerm) - 2) + '%'
 FROM {schema}[{objectQualifier}gs_MediaObject] m JOIN {schema}[{objectQualifier}gs_Album] a ON a.AlbumId =
m.FKAlbumId CROSS JOIN #searchTerms
 WHERE a.FKGalleryId = @GalleryId AND m.OriginalFilename
LIKE #searchTerms.searchTerm --AND 0=1

-- Search media object metadata
INSERT #searchResults
SELECT DISTINCT 'm', m.MediaObjectId, 'MediaObjectMetadata',
'%' + SUBSTRING(md.Value,
PATINDEX(#searchTerms.searchTerm, md.Value),
LEN(#searchTerms.searchTerm) - 2) + '%'
 FROM {schema}[{objectQualifier}gs_MediaObjectMetadata] md JOIN {schema}[{objectQualifier}gs_MediaObject] m
 ON md.FKMediaObjectId = m.MediaObjectId
JOIN {schema}[{objectQualifier}gs_Album] a
 ON a.AlbumId = m.FKAlbumId CROSS JOIN #searchTerms
 WHERE a.FKGalleryId = @GalleryId AND md.Value
LIKE #searchTerms.searchTerm

-- Uncomment for debug purposes:
--SELECT * from #searchTerms
--SELECT * FROM #searchResults

SELECT sr.gotype, sr.id
FROM #searchTerms AS st INNER JOIN (SELECT DISTINCT gotype, id,
matchingSearchTerm FROM #searchResults) AS sr ON st.searchTerm =
sr.matchingSearchTerm
GROUP BY sr.gotype, sr.id
HAVING (COUNT(*) >= (SELECT COUNT(*) FROM #searchTerms))

RETURN
GO

ALTER PROCEDURE {schema}[{objectQualifier}gs_SelectChildAlbums]
(
	@AlbumId int
)
AS
SET NOCOUNT ON

SELECT AlbumId
FROM {schema}[{objectQualifier}gs_Album]
WHERE AlbumParentId = @AlbumId

RETURN
GO

ALTER PROCEDURE {schema}[{objectQualifier}gs_SelectRootAlbum]
(
	@GalleryId int
)
AS
SET NOCOUNT ON

/* Return the root album. First, check to make sure it exists.
If not, call the stored proc that will insert a default record. */

IF NOT EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_Album] WHERE AlbumParentId = 0 AND FKGalleryId = @GalleryId)
	EXEC {schema}[{objectQualifier}gs_GalleryConfig] @GalleryId, 'All albums', 'Welcome to Gallery Server Pro!'

SELECT
	AlbumId, FKGalleryId as GalleryID, AlbumParentId, Title, DirectoryName, Summary, ThumbnailMediaObjectId, 
	Seq, DateStart, DateEnd, CreatedBy, DateAdded, LastModifiedBy, DateLastModified, OwnedBy, OwnerRoleName, IsPrivate
FROM {schema}[{objectQualifier}gs_Album]
WHERE AlbumParentId = 0 AND FKGalleryId = @GalleryId
GO

ALTER PROCEDURE {schema}[{objectQualifier}gs_SynchronizeSave]
	(@SynchId nchar(50), @GalleryId int, @SynchState int, @TotalFiles int, @CurrentFileIndex int)
AS
SET NOCOUNT, XACT_ABORT ON
/* UPDATE the synchronize table with the specified data. */

/* Check if another synchronization is in progress. Return with error code if it is. */
IF EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_Synchronize] WHERE FKGalleryId = @GalleryId AND SynchId <> @SynchId AND (SynchState = 2 OR SynchState = 3))
BEGIN
	RETURN 250000
END

BEGIN TRAN
IF EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_Synchronize] WITH (UPDLOCK, HOLDLOCK) WHERE FKGalleryId = @GalleryId)
	UPDATE {schema}[{objectQualifier}gs_Synchronize]
	SET SynchId = @SynchId,
		FKGalleryId = @GalleryId,
		SynchState = @SynchState,
		TotalFiles = @TotalFiles,
		CurrentFileIndex = @CurrentFileIndex
	WHERE FKGalleryId = @GalleryId
ELSE
	INSERT {schema}[{objectQualifier}gs_Synchronize] (SynchId, FKGalleryId, SynchState, TotalFiles, CurrentFileIndex)
	VALUES (@SynchId, @GalleryId, @SynchState, @TotalFiles, @CurrentFileIndex)
COMMIT
	
RETURN
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_AppSettingSelect]') AND type in (N'P', N'PC'))
	DROP PROCEDURE {schema}[{objectQualifier}gs_AppSettingSelect]
GO

CREATE PROCEDURE {schema}[{objectQualifier}gs_AppSettingSelect]

AS
SET NOCOUNT ON

SELECT
	AppSettingId, SettingName, SettingValue
FROM {schema}[{objectQualifier}gs_AppSetting];

RETURN
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_AppSettingUpdate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE {schema}[{objectQualifier}gs_AppSettingUpdate]
GO

CREATE PROCEDURE {schema}[{objectQualifier}gs_AppSettingUpdate]
(@SettingName varchar(200), @SettingValue nvarchar(max))

AS

UPDATE {schema}[{objectQualifier}gs_AppSetting]
SET SettingValue = @SettingValue
WHERE SettingName = @SettingName;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_BrowserTemplateSelect]') AND type in (N'P', N'PC'))
	DROP PROCEDURE {schema}[{objectQualifier}gs_BrowserTemplateSelect]
GO

CREATE PROCEDURE {schema}[{objectQualifier}gs_BrowserTemplateSelect]

AS
SET NOCOUNT ON

SELECT BrowserTemplateId, MimeType, BrowserId, HtmlTemplate, ScriptTemplate
FROM {schema}[{objectQualifier}gs_BrowserTemplate]
ORDER BY MimeType;

RETURN
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_GalleryConfig]') AND type in (N'P', N'PC'))
	DROP PROCEDURE {schema}[{objectQualifier}gs_GalleryConfig]
GO

CREATE PROCEDURE {schema}[{objectQualifier}gs_GalleryConfig]
(
	@GalleryId int, @RootAlbumTitle nvarchar(1000),	@RootAlbumSummary nvarchar(max)
)
AS
SET NOCOUNT, XACT_ABORT ON

BEGIN TRAN

-- Verify the gallery record exists.
IF NOT EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_Gallery] WITH (UPDLOCK, HOLDLOCK) WHERE GalleryId = @GalleryId)
BEGIN
	SET IDENTITY_INSERT {schema}[{objectQualifier}gs_Gallery] ON

	INSERT INTO {schema}[{objectQualifier}gs_Gallery] ([GalleryId], [Description], [DateAdded]) VALUES (@GalleryId, 'My Gallery', GETDATE());

	SET IDENTITY_INSERT {schema}[{objectQualifier}gs_Gallery] OFF
END

-- Create a new set of gallery settings by copying the template settings (do nothing if already present).
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] (FKGalleryId, IsTemplate, SettingName, SettingValue)
SELECT @GalleryId, 0, t.SettingName, t.SettingValue
FROM {schema}[{objectQualifier}gs_GallerySetting] t
WHERE t.IsTemplate = 1
	AND t.SettingName NOT IN 
		(SELECT g.SettingName FROM {schema}[{objectQualifier}gs_GallerySetting] g
		 WHERE g.FKGalleryId = @GalleryId);

-- Create a new set of gallery MIME types (do nothing if already present).
IF NOT EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_MimeTypeGallery] WITH (UPDLOCK, HOLDLOCK) WHERE FKGalleryId = @GalleryId)
BEGIN
	-- No records exist. Copy the whole set from our template list and enable .jpg and .jpeg
	INSERT INTO {schema}[{objectQualifier}gs_MimeTypeGallery] (FKGalleryId, FKMimeTypeId, IsEnabled)
	SELECT @GalleryId, MimeTypeId, 0
	FROM {schema}[{objectQualifier}gs_MimeType];
	
	-- Configure .jpg and .jpeg as enabled by default
	UPDATE {schema}[{objectQualifier}gs_MimeTypeGallery]
	SET IsEnabled = 1
	WHERE FKGalleryId = @GalleryId AND FKMimeTypeId IN (SELECT MimeTypeId FROM {schema}[{objectQualifier}gs_MimeType] WHERE FileExtension IN ('.jpg', '.jpeg'));
END
ELSE
BEGIN
	-- At least one record exists, but make sure we insert any new items that may have been
	-- added to the template list.
	INSERT INTO {schema}[{objectQualifier}gs_MimeTypeGallery] (FKGalleryId, FKMimeTypeId, IsEnabled)
	SELECT @GalleryId, mt.MimeTypeId, 0
	FROM {schema}[{objectQualifier}gs_MimeType] mt
	WHERE mt.MimeTypeId NOT IN
		(SELECT mtg.FKMimeTypeId FROM {schema}[{objectQualifier}gs_MimeTypeGallery] mtg
		 WHERE mtg.FKGalleryId = @GalleryId);
END

-- Create the root album.
INSERT INTO {schema}[{objectQualifier}gs_Album] (AlbumParentId, FKGalleryId, Title, DirectoryName, Summary, ThumbnailMediaObjectId, Seq, CreatedBy, DateAdded, LastModifiedBy, DateLastModified, OwnedBy, OwnerRoleName, IsPrivate)
SELECT 0, @GalleryId, @RootAlbumTitle, '', @RootAlbumSummary, 0, 0, 'System', GETDATE(), 'System', GETDATE(), '', '', '0'
WHERE NOT EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_Album] WHERE AlbumParentId = 0 AND FKGalleryId = @GalleryId);

-- For each role with AllowAdministerSite permission, add a corresponding record in gs_Role_Album giving it 
-- access to the root album.
DECLARE @RootAlbumId int
SELECT @RootAlbumId = AlbumId FROM {schema}[{objectQualifier}gs_Album] WHERE AlbumParentId = 0 AND FKGalleryId = @GalleryId;

INSERT INTO {schema}[{objectQualifier}gs_Role_Album] (FKRoleName, FKAlbumId)
SELECT R.RoleName, @RootAlbumId
FROM {schema}[{objectQualifier}gs_Role] R LEFT JOIN {schema}[{objectQualifier}gs_Role_Album] RA ON R.RoleName = RA.FKRoleName
WHERE R.AllowAdministerSite = 1 AND RA.FKRoleName IS NULL;

-- Insert a record into the synch table if it doesn't already exist.
INSERT INTO {schema}[{objectQualifier}gs_Synchronize]
SELECT '', g.GalleryId, 1, 0, 0
FROM {schema}[{objectQualifier}gs_Gallery] g LEFT OUTER JOIN {schema}[{objectQualifier}gs_Synchronize] s
	ON g.GalleryId = s.FKGalleryId
WHERE (g.GalleryId = @GalleryId) AND (s.FKGalleryId IS NULL);

-- Now update it to default values. This is redundant when the previous insert inserted a record,
-- but is necessary for resetting previously existing records.
UPDATE {schema}[{objectQualifier}gs_Synchronize]
SET SynchId = '', SynchState = 1, TotalFiles = 0, CurrentFileIndex = 0
WHERE FKGalleryId = @GalleryId

COMMIT
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_GalleryControlSettingSelect]') AND type in (N'P', N'PC'))
	DROP PROCEDURE {schema}[{objectQualifier}gs_GalleryControlSettingSelect]
GO

CREATE PROCEDURE {schema}[{objectQualifier}gs_GalleryControlSettingSelect]

AS
SET NOCOUNT ON

SELECT GalleryControlSettingId, ControlId, SettingName, SettingValue
FROM {schema}[{objectQualifier}gs_GalleryControlSetting]
ORDER BY ControlId

RETURN
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_GalleryControlSettingUpdate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE {schema}[{objectQualifier}gs_GalleryControlSettingUpdate]
GO

CREATE PROCEDURE {schema}[{objectQualifier}gs_GalleryControlSettingUpdate]
(@ControlId nvarchar(350), @SettingName varchar(200), @SettingValue nvarchar(max))

AS
SET NOCOUNT ON

IF @SettingValue IS NULL
BEGIN
	DELETE FROM {schema}[{objectQualifier}gs_GalleryControlSetting]
	WHERE (ControlId = @ControlId) AND (SettingName = @SettingName)
END
ELSE
BEGIN
	IF EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_GalleryControlSetting] WITH (UPDLOCK, HOLDLOCK)
		WHERE (ControlId = @ControlId) AND (SettingName = @SettingName))
	BEGIN
		-- Record exists. If we have a value, update it; otherwise delete the record.
			UPDATE {schema}[{objectQualifier}gs_GalleryControlSetting]
			SET SettingValue = @SettingValue
			WHERE (ControlId = @ControlId) AND (SettingName = @SettingName)
	END
	ELSE
	BEGIN
		-- Existing record does not exist, so insert it (if we have something to insert)
		INSERT INTO {schema}[{objectQualifier}gs_GalleryControlSetting] (ControlId, SettingName, SettingValue)
		VALUES (@ControlId, @SettingName, @SettingValue)
	END
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_GalleryDelete]') AND type in (N'P', N'PC'))
	DROP PROCEDURE {schema}[{objectQualifier}gs_GalleryDelete]
GO

CREATE PROCEDURE {schema}[{objectQualifier}gs_GalleryDelete]
(
	@GalleryId int
)
AS
SET NOCOUNT, XACT_ABORT ON

BEGIN TRAN

	DELETE FROM {schema}[{objectQualifier}gs_MediaObjectMetadata]
	WHERE FKMediaObjectId IN 
		(SELECT mo.MediaObjectId
		 FROM {schema}[{objectQualifier}gs_Album] a INNER JOIN {schema}[{objectQualifier}gs_MediaObject] mo ON a.AlbumId = mo.FKAlbumId
		 WHERE a.FKGalleryId = @GalleryId)
	
	DELETE FROM {schema}[{objectQualifier}gs_MediaObject]
	WHERE FKAlbumId IN 
		(SELECT AlbumId
		 FROM {schema}[{objectQualifier}gs_Album]
		 WHERE FKGalleryId = @GalleryId)
	
	DELETE FROM {schema}[{objectQualifier}gs_Role_Album]
	WHERE FKAlbumId IN 
		(SELECT AlbumId
		 FROM {schema}[{objectQualifier}gs_Album]
		 WHERE FKGalleryId = @GalleryId)
	
	DELETE FROM {schema}[{objectQualifier}gs_Album]
	WHERE FKGalleryId = @GalleryId
		 
	DELETE FROM {schema}[{objectQualifier}gs_AppError]
	WHERE FKGalleryId = @GalleryId
				 
	DELETE FROM {schema}[{objectQualifier}gs_GalleryControlSetting]
	WHERE ControlId IN
		(SELECT ControlId
		 FROM {schema}[{objectQualifier}gs_GalleryControlSetting]
		 WHERE SettingName = 'GalleryId' AND SettingValue = @GalleryId)

	DELETE FROM {schema}[{objectQualifier}gs_GallerySetting]
	WHERE FKGalleryId = @GalleryId
		 
	DELETE FROM {schema}[{objectQualifier}gs_MimeTypeGallery]
	WHERE FKGalleryId = @GalleryId
	
	DELETE FROM {schema}[{objectQualifier}gs_Synchronize]
	WHERE FKGalleryId = @GalleryId
		
	DELETE FROM {schema}[{objectQualifier}gs_UserGalleryProfile]
	WHERE FKGalleryId = @GalleryId

	DELETE FROM {schema}[{objectQualifier}gs_Gallery]
	WHERE [GalleryId] = @GalleryId

COMMIT
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_GalleryInsert]') AND type in (N'P', N'PC'))
	DROP PROCEDURE {schema}[{objectQualifier}gs_GalleryInsert]
GO

CREATE PROCEDURE {schema}[{objectQualifier}gs_GalleryInsert]
(
	@Description nvarchar(1000), @Identity int OUT
)
AS
SET NOCOUNT ON

-- Insert a gallery record.
INSERT {schema}[{objectQualifier}gs_Gallery] (Description, DateAdded)
VALUES (@Description, GETDATE())
 
SET @Identity = SCOPE_IDENTITY()
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_GallerySettingSelect]') AND type in (N'P', N'PC'))
	DROP PROCEDURE {schema}[{objectQualifier}gs_GallerySettingSelect]
GO

CREATE PROCEDURE {schema}[{objectQualifier}gs_GallerySettingSelect]

AS
SET NOCOUNT ON

SELECT GallerySettingId, FKGalleryId, IsTemplate, SettingName, SettingValue
FROM {schema}[{objectQualifier}gs_GallerySetting]
ORDER BY FKGalleryId;

RETURN
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_GallerySettingUpdate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE {schema}[{objectQualifier}gs_GallerySettingUpdate]
GO

CREATE PROCEDURE {schema}[{objectQualifier}gs_GallerySettingUpdate]
(@GalleryId int, @SettingName varchar(200), @SettingValue nvarchar(max))

AS

UPDATE {schema}[{objectQualifier}gs_GallerySetting]
SET SettingValue = @SettingValue
WHERE FKGalleryId = @GalleryId AND SettingName = @SettingName;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_GalleryUpdate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE {schema}[{objectQualifier}gs_GalleryUpdate]
GO

CREATE PROCEDURE {schema}[{objectQualifier}gs_GalleryUpdate]
(
	@GalleryId int, @Description nvarchar(1000)
)
AS
SET NOCOUNT ON

-- Try to update the record. If no record is updated, then insert one.
UPDATE {schema}[{objectQualifier}gs_Gallery]
SET [Description] = @Description
WHERE [GalleryId] = @GalleryId
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_MimeTypeGallerySelect]') AND type in (N'P', N'PC'))
	DROP PROCEDURE {schema}[{objectQualifier}gs_MimeTypeGallerySelect]
GO

CREATE PROCEDURE {schema}[{objectQualifier}gs_MimeTypeGallerySelect]

AS
SET NOCOUNT ON

SELECT mtg.MimeTypeGalleryId, mtg.FKGalleryId, mt.FileExtension, mtg.IsEnabled
FROM {schema}[{objectQualifier}gs_MimeType] mt INNER JOIN {schema}[{objectQualifier}gs_MimeTypeGallery] mtg ON mt.MimeTypeId = mtg.FKMimeTypeId
ORDER BY mt.FileExtension;

RETURN
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_MimeTypeGalleryUpdate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE {schema}[{objectQualifier}gs_MimeTypeGalleryUpdate]
GO

CREATE PROCEDURE {schema}[{objectQualifier}gs_MimeTypeGalleryUpdate]
(@MimeTypeGalleryId int, @IsEnabled bit)

AS

UPDATE {schema}[{objectQualifier}gs_MimeTypeGallery]
SET IsEnabled = @IsEnabled
WHERE MimeTypeGalleryId = @MimeTypeGalleryId;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_MimeTypeSelect]') AND type in (N'P', N'PC'))
	DROP PROCEDURE {schema}[{objectQualifier}gs_MimeTypeSelect]
GO

CREATE PROCEDURE {schema}[{objectQualifier}gs_MimeTypeSelect]

AS
SET NOCOUNT ON

SELECT
	MimeTypeId, FileExtension, MimeTypeValue, BrowserMimeTypeValue
FROM {schema}[{objectQualifier}gs_MimeType]
ORDER BY FileExtension;

RETURN
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_UserGalleryProfileDeleteForGallery]') AND type in (N'P', N'PC'))
	DROP PROCEDURE {schema}[{objectQualifier}gs_UserGalleryProfileDeleteForGallery]
GO

CREATE PROCEDURE {schema}[{objectQualifier}gs_UserGalleryProfileDeleteForGallery]
(
	@GalleryId int
)
AS
SET NOCOUNT ON
	
DELETE FROM {schema}[{objectQualifier}gs_UserGalleryProfile]
WHERE [FKGalleryId] = @GalleryId
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_UserGalleryProfileDeleteForUser]') AND type in (N'P', N'PC'))
	DROP PROCEDURE {schema}[{objectQualifier}gs_UserGalleryProfileDeleteForUser]
GO

CREATE PROCEDURE {schema}[{objectQualifier}gs_UserGalleryProfileDeleteForUser]
(
	@UserName nvarchar(256)
)
AS
SET NOCOUNT ON
	
DELETE FROM {schema}[{objectQualifier}gs_UserGalleryProfile]
WHERE [UserName] = @UserName
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_UserGalleryProfileSave]') AND type in (N'P', N'PC'))
	DROP PROCEDURE {schema}[{objectQualifier}gs_UserGalleryProfileSave]
GO

CREATE PROCEDURE {schema}[{objectQualifier}gs_UserGalleryProfileSave]
	(@UserName nvarchar(256), @GalleryId int, @SettingName varchar(200), @SettingValue nvarchar(max))
AS
SET NOCOUNT, XACT_ABORT ON

/* INSERT or UPDATE the user gallery profile table with the specified data. */
BEGIN TRAN
IF EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_UserGalleryProfile] WITH (UPDLOCK, HOLDLOCK) 
			WHERE UserName = @UserName AND FKGalleryId = @GalleryId and SettingName = @SettingName)
	UPDATE {schema}[{objectQualifier}gs_UserGalleryProfile]
	SET SettingValue = @SettingValue
	WHERE UserName = @UserName AND FKGalleryId = @GalleryId and SettingName = @SettingName
ELSE
	INSERT {schema}[{objectQualifier}gs_UserGalleryProfile] (UserName, FKGalleryId, SettingName, SettingValue)
	VALUES (@UserName, @GalleryId, @SettingName, @SettingValue)
COMMIT
	
RETURN
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_UserGalleryProfileSelect]') AND type in (N'P', N'PC'))
	DROP PROCEDURE {schema}[{objectQualifier}gs_UserGalleryProfileSelect]
GO

CREATE PROCEDURE {schema}[{objectQualifier}gs_UserGalleryProfileSelect]
(
	@UserName nvarchar(256)
)

AS
SET NOCOUNT ON

SELECT
	ProfileId, UserName, FKGalleryId, SettingName, SettingValue
FROM {schema}[{objectQualifier}gs_UserGalleryProfile]
WHERE UserName=@UserName
ORDER BY UserName, FKGalleryId;

RETURN
GO

ALTER TABLE {schema}[{objectQualifier}gs_Album] WITH CHECK CHECK CONSTRAINT [{objectQualifier}FK_gs_Album_gs_Gallery];
ALTER TABLE {schema}[{objectQualifier}gs_AppError] WITH CHECK CHECK CONSTRAINT [{objectQualifier}FK_gs_AppError_gs_Gallery];
ALTER TABLE {schema}[{objectQualifier}gs_GallerySetting] WITH CHECK CHECK CONSTRAINT [{objectQualifier}FK_gs_GallerySetting_gs_Gallery];
ALTER TABLE {schema}[{objectQualifier}gs_MimeTypeGallery] WITH CHECK CHECK CONSTRAINT [{objectQualifier}FK_gs_MimeTypeGallery_gs_Gallery];
ALTER TABLE {schema}[{objectQualifier}gs_MimeTypeGallery] WITH CHECK CHECK CONSTRAINT [{objectQualifier}FK_gs_MimeTypeGallery_gs_MimeType];
ALTER TABLE {schema}[{objectQualifier}gs_UserGalleryProfile] WITH CHECK CHECK CONSTRAINT [{objectQualifier}FK_gs_UserGalleryProfile_gs_Gallery];
ALTER TABLE {schema}[{objectQualifier}gs_Role_Album] WITH CHECK CHECK CONSTRAINT [{objectQualifier}FK_gs_Role_Album_gs_Role];
ALTER TABLE {schema}[{objectQualifier}gs_Synchronize] WITH CHECK CHECK CONSTRAINT [{objectQualifier}FK_gs_Synchronize_gs_Gallery];
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_MediaObject]') AND name = N'IDX_gs_MediaObject_MediaObjectId_FKAlbumId')
CREATE NONCLUSTERED INDEX [IDX_gs_MediaObject_MediaObjectId_FKAlbumId] ON {schema}[{objectQualifier}gs_MediaObject] 
( 
 [OriginalFilename] ASC
)
 INCLUDE ([MediaObjectId], [FKAlbumId])
 WITH (PAD_INDEX=OFF, STATISTICS_NORECOMPUTE=OFF, SORT_IN_TEMPDB=OFF, IGNORE_DUP_KEY=OFF, DROP_EXISTING=OFF, ONLINE=OFF, ALLOW_ROW_LOCKS=ON, ALLOW_PAGE_LOCKS=ON) ON [PRIMARY]
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_Album]') AND name = N'IDX_gs_Album_AlbumId')
CREATE NONCLUSTERED INDEX [IDX_gs_Album_AlbumId] ON {schema}[{objectQualifier}gs_Album] 
(
 [AlbumId] ASC
)
 WITH (SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
GO

IF NOT EXISTS (SELECT * FROM sys.stats WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_Album]') AND name = N'STAT_gs_Album_FKGalleryId_AlbumId')
CREATE STATISTICS [STAT_gs_Album_FKGalleryId_AlbumId] ON {schema}[{objectQualifier}gs_Album] ([FKGalleryId], [AlbumId])
GO

IF NOT EXISTS (SELECT * FROM sys.stats WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_Album]') AND name = N'STAT_gs_Album_AlbumParentId_FKGalleryId_AlbumId')
CREATE STATISTICS [STAT_gs_Album_AlbumParentId_FKGalleryId_AlbumId] ON {schema}[{objectQualifier}gs_Album] ([AlbumParentId], [FKGalleryId], [AlbumId])
GO

IF NOT EXISTS (SELECT * FROM sys.stats WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_Album]') AND name = N'STAT_gs_Album_AlbumId_AlbumParentId')
CREATE STATISTICS [STAT_gs_Album_AlbumId_AlbumParentId] ON {schema}[{objectQualifier}gs_Album] ([AlbumId], [AlbumParentId])
GO

IF NOT EXISTS (SELECT * FROM sys.stats WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_MediaObjectMetadata]') AND name = N'STAT_gs_MediaObjectMetadata_MediaObjectMetadataId_FKMediaObjectId')
CREATE STATISTICS [STAT_gs_MediaObjectMetadata_MediaObjectMetadataId_FKMediaObjectId] ON [dbo].[gs_MediaObjectMetadata]([MediaObjectMetadataId], [FKMediaObjectId])
GO

/* --------------------------------------------------------------------------- */
/*                 Insert default data                                         */
/* --------------------------------------------------------------------------- */

/* Table gs_Gallery */
IF NOT EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_Gallery] WITH (UPDLOCK, HOLDLOCK) WHERE GalleryId = -2147483648)
BEGIN
	SET IDENTITY_INSERT {schema}[{objectQualifier}gs_Gallery] ON

	INSERT INTO {schema}[{objectQualifier}gs_Gallery] ([GalleryId], [Description], [DateAdded]) VALUES (-2147483648, 'Template Gallery', GETDATE());

	SET IDENTITY_INSERT {schema}[{objectQualifier}gs_Gallery] OFF
END
GO

/* Update the DateAdded field of all gallery records to today's date, so that new 30-day trial is activated. */
UPDATE {schema}[{objectQualifier}gs_Gallery]
SET [DateAdded] = GETDATE();
GO

/* Clear the tables before inserting the new default data (they should be empty anyway). */
DELETE FROM {schema}[{objectQualifier}gs_AppSetting];
GO
DELETE FROM {schema}[{objectQualifier}gs_GallerySetting];
GO
DELETE FROM {schema}[{objectQualifier}gs_MimeTypeGallery];
GO
DELETE FROM {schema}[{objectQualifier}gs_MimeType];
GO
DELETE FROM {schema}[{objectQualifier}gs_BrowserTemplate];
GO

/* Table gs_AppSetting */
INSERT INTO {schema}[{objectQualifier}gs_AppSetting] ([SettingName], [SettingValue]) VALUES ('MediaObjectDownloadBufferSize','32768');
INSERT INTO {schema}[{objectQualifier}gs_AppSetting] ([SettingName], [SettingValue]) VALUES ('EncryptMediaObjectUrlOnClient','False');
INSERT INTO {schema}[{objectQualifier}gs_AppSetting] ([SettingName], [SettingValue]) VALUES ('EncryptionKey','mNU-h7:5f_)3=c%@^}#U9Tn*');
INSERT INTO {schema}[{objectQualifier}gs_AppSetting] ([SettingName], [SettingValue]) VALUES ('JQueryScriptPath','//ajax.googleapis.com/ajax/libs/jquery/1/jquery.min.js');
INSERT INTO {schema}[{objectQualifier}gs_AppSetting] ([SettingName], [SettingValue]) VALUES ('JQueryUiScriptPath','//ajax.googleapis.com/ajax/libs/jqueryui/1/jquery-ui.min.js');
INSERT INTO {schema}[{objectQualifier}gs_AppSetting] ([SettingName], [SettingValue]) VALUES ('MembershipProviderName','');
INSERT INTO {schema}[{objectQualifier}gs_AppSetting] ([SettingName], [SettingValue]) VALUES ('RoleProviderName','');
INSERT INTO {schema}[{objectQualifier}gs_AppSetting] ([SettingName], [SettingValue]) VALUES ('ProductKey','');
INSERT INTO {schema}[{objectQualifier}gs_AppSetting] ([SettingName], [SettingValue]) VALUES ('EnableCache','True');
INSERT INTO {schema}[{objectQualifier}gs_AppSetting] ([SettingName], [SettingValue]) VALUES ('AllowGalleryAdminToManageUsersAndRoles','True');
INSERT INTO {schema}[{objectQualifier}gs_AppSetting] ([SettingName], [SettingValue]) VALUES ('AllowGalleryAdminToViewAllUsersAndRoles','True');
INSERT INTO {schema}[{objectQualifier}gs_AppSetting] ([SettingName], [SettingValue]) VALUES ('MaxNumberErrorItems','200');
INSERT INTO {schema}[{objectQualifier}gs_AppSetting] ([SettingName], [SettingValue]) VALUES ('DataSchemaVersion','2.4.6');
GO

/* Table gs_GallerySetting */
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'MediaObjectPath','gs\mediaobjects');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'ThumbnailPath','');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'OptimizedPath','');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'MediaObjectPathIsReadOnly','False');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'ShowHeader','True');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'GalleryTitle','Media Gallery');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'GalleryTitleUrl','~/');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'ShowLogin','True');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'ShowSearch','True');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'ShowErrorDetails','False');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'EnableExceptionHandler','True');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'DefaultAlbumDirectoryNameLength','25');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'SynchAlbumTitleAndDirectoryName','True');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'EmptyAlbumThumbnailBackgroundColor','#369');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'EmptyAlbumThumbnailText','Empty');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'EmptyAlbumThumbnailFontName','Verdana');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'EmptyAlbumThumbnailFontSize','13');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'EmptyAlbumThumbnailFontColor','White');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'EmptyAlbumThumbnailWidthToHeightRatio','1.33');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'MaxAlbumThumbnailTitleDisplayLength','20');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'MaxMediaObjectThumbnailTitleDisplayLength','16');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'MediaObjectCaptionTemplate','{Title}');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'AllowUserEnteredHtml','False');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'AllowUserEnteredJavascript','False');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'AllowedHtmlTags','p,a,div,span,br,ul,ol,li,table,tr,td,th,h1,h2,h3,h4,h5,h6,strong,b,em,i,u,cite,blockquote,address,pre,hr,img,dl,dt,dd,code,tt');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'AllowedHtmlAttributes','href,class,style,id,src,title,alt,target,name');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'AllowCopyingReadOnlyObjects','False');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'AllowManageOwnAccount','True');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'AllowDeleteOwnAccount','True');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'MediaObjectTransitionType','Fade');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'MediaObjectTransitionDuration','0.2');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'SlideshowInterval','4000');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'AllowUnspecifiedMimeTypes','False');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'ImageTypesStandardBrowsersCanDisplay','.jpg,.jpeg,.gif,.png');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'SilverlightFileTypes','.mp3,.wma,.wmv,.asf,.asx,.m4a');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'ImageMagickFileTypes','.pdf,.txt,.eps,.psd,.tif,.tiff');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'AllowAnonymousHiResViewing','True');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'EnableMetadata','True');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'ExtractMetadata','True');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'ExtractMetadataUsingWpf','True');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'MetadataDisplaySettings','29:T,34:T,35:F,8:T,102:T,106:T,22:T,14:T,9:T,5:T,28:T,2:T,26:T,4:T,6:T,7:T,12:T,13:T,15:T,16:T,17:T,18:T,21:T,23:T,24:T,10:T,25:T,27:T,11:T,1:T,32:T,3:T,0:T,31:T,20:T,30:T,33:T,19:T,36:T,37:F,38:T,39:F,40:T,101:F,103:F,104:F,105:F,108:F,107:F,110:T,109:T,1012:T,1013:T,1010:T,1011:T,1014:T,1017:T,1018:T,1015:T,1016:T,1003:T,1004:T,1001:T,1002:T,1005:T,1008:T,1009:T,1006:T,1007:T');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'GpsMapUrlTemplate','<a href=''http://bing.com/maps/default.aspx?sp=point.{GpsLatitude}_{GpsLongitude}_{TitleNoHtml}__{MediaObjectPageUrl}_{MediaObjectUrl}&style=a&lvl=13'' target=''_blank'' title=''View map''>{GpsLocation}</a>');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'EnableMediaObjectDownload','True');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'EnableGalleryObjectZipDownload','True');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'EnableAlbumZipDownload','True');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'EnablePermalink','True');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'EnableSlideShow','True');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'MaxThumbnailLength','115');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'ThumbnailImageJpegQuality','70');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'ThumbnailClickShowsOriginal','False');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'ThumbnailWidthBuffer','30');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'ThumbnailHeightBuffer','70');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'ThumbnailFileNamePrefix','zThumb_');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'MaxOptimizedLength','640');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'OptimizedImageJpegQuality','70');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'OptimizedImageTriggerSizeKb','50');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'OptimizedFileNamePrefix','zOpt_');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'OriginalImageJpegQuality','95');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'DiscardOriginalImageDuringImport','False');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'ApplyWatermark','False');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'ApplyWatermarkToThumbnails','False');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'WatermarkText','Copyright 2010, Your Company Name, All Rights Reserved');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'WatermarkTextFontName','Verdana');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'WatermarkTextFontSize','13');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'WatermarkTextWidthPercent','50');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'WatermarkTextColor','White');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'WatermarkTextOpacityPercent','35');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'WatermarkTextLocation','BottomCenter');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'WatermarkImagePath','gs/images/gsplogo.png');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'WatermarkImageWidthPercent','85');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'WatermarkImageOpacityPercent','25');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'WatermarkImageLocation','MiddleCenter');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'SendEmailOnError','True');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'EmailFromName','Gallery Server Pro');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'EmailFromAddress','webmaster@yourisp.com');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'SmtpServer','');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'SmtpServerPort','');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'SendEmailUsingSsl','False');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'AutoStartMediaObject','False');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'DefaultVideoPlayerWidth','640');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'DefaultVideoPlayerHeight','480');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'DefaultAudioPlayerWidth','600');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'DefaultAudioPlayerHeight','60');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'DefaultGenericObjectWidth','640');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'DefaultGenericObjectHeight','480');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'MaxUploadSize','2097151');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'AllowAddLocalContent','True');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'AllowAddExternalContent','True');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'AllowAnonymousBrowsing','True');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'PageSize','0');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'PagerLocation','TopAndBottom');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'EnableSelfRegistration','False');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'RequireEmailValidationForSelfRegisteredUser','False');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'RequireApprovalForSelfRegisteredUser','False');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'UseEmailForAccountName','False');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'DefaultRolesForSelfRegisteredUser','');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'UsersToNotifyWhenAccountIsCreated','');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'UsersToNotifyWhenErrorOccurs','');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'EnableUserAlbum','False');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'EnableUserAlbumDefaultForUser','True');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'UserAlbumParentAlbumId','0');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'UserAlbumNameTemplate','{UserName}''s gallery');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'UserAlbumSummaryTemplate','Welcome to your personal gallery. You can easily add photos, video, and other files. When you are logged in, an Actions menu appears in the upper left to help you manage your gallery.');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'RedirectToUserAlbumAfterLogin','False');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'VideoThumbnailPosition','3');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'EnableAutoSync','False');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'AutoSyncIntervalMinutes','1440');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'LastAutoSync','');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'EnableRemoteSync','False');
INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue]) VALUES (-2147483648,1,'RemoteAccessPassword','');
GO

/* Table gs_MimeType */
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.afl','video/animaflex','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.aif','audio/aiff','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.aifc','audio/aiff','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.aiff','audio/aiff','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.asf','video/x-ms-asf','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.asx','video/x-ms-asf','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.au','audio/basic','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.avi','video/x-ms-wvx','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.avs','video/avs-video','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.bm','image/bmp','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.bmp','image/bmp','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.chm','application/vnd.ms-htmlhelp','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.css','text/css','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.divx','video/divx','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.dl','video/dl','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.doc','application/msword','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.docm','application/vnd.ms-word.document.macroEnabled.12','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.docx','application/vnd.openxmlformats-officedocument.wordprocessingml.document','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.dotx','application/vnd.openxmlformats-officedocument.wordprocessingml.template','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.dot','application/msword','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.dotm','application/vnd.ms-word.template.macroEnabled.12','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.dtd','application/xml-dtd','text/plain');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.dwg','image/vnd.dwg','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.dxf','image/vnd.dwg','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.emf','image/x-emf','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.eps','image/postscript','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.exe','application/octet-stream','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.f4v','video/f4v','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.fif','image/fif','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.fli','video/fli','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.flo','image/florian','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.flv','video/x-flv','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.fpx','image/vnd.fpx','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.funk','audio/make','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.g3','image/g3fax','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.gif','image/gif','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.gl','video/gl','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.htm','text/html','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.html','text/html','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.ico','image/ico','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.ief','image/ief','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.iefs','image/ief','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.it','audio/it','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.jar','application/java-archive','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.jfif','image/jpeg','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.jfif-tbnl','image/jpeg','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.jpe','image/jpeg','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.jpeg','image/jpeg','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.jpg','image/jpeg','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.js','text/javascript','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.jut','image/jutvision','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.kar','audio/midi','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.la','audio/nspaudio','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.lma','audio/nspaudio','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.m1v','video/mpeg','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.m2a','audio/mpeg','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.m2v','video/mpeg','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.m4a','audio/m4a','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.m4v','video/m4v','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.mcf','image/vasa','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.mht','message/rfc822','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.mid','audio/midi','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.midi','audio/midi','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.mod','audio/mod','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.moov','video/quicktime','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.mov','video/mp4','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.mp2','audio/mpeg','application/x-mplayer2');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.mp3','audio/x-mp3','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.mp4','video/mp4','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.mpa','audio/mpeg','application/x-mplayer2');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.mpe','video/mpeg','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.mpeg','video/mpeg','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.mpg','video/mpeg','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.mpga','audio/mpeg','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.my','audio/make','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.nap','image/naplps','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.naplps','image/naplps','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.oga','audio/ogg','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.ogg','video/ogg','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.ogv','video/ogg','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.pdf','application/pdf','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.pfunk','audio/make','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.pic','image/pict','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.pict','image/pict','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.png','image/png','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.potm','application/vnd.ms-powerpoint.template.macroEnabled.12','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.potx','application/vnd.openxmlformats-officedocument.presentationml.template','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.ppam','application/vnd.ms-powerpoint.addin.macroEnabled.12','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.pps','application/vnd.ms-powerpoint','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.ppsm','application/vnd.ms-powerpoint.slideshow.macroEnabled.12','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.ppsx','application/vnd.openxmlformats-officedocument.presentationml.slideshow','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.ppt','application/vnd.ms-powerpoint','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.pptm','application/vnd.ms-powerpoint.presentation.macroEnabled.12','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.pptx','application/vnd.openxmlformats-officedocument.presentationml.presentation','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.psd','image/psd','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.qcp','audio/vnd.qcelp','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.qt','video/quicktime','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.ra','audio/x-pn-realaudio','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.ram','audio/x-pn-realaudio','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.ras','image/cmu-raster','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.rast','image/cmu-raster','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.rf','image/vnd.rn-realflash','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.rmi','audio/mid','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.rp','image/vnd.rn-realpix','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.rtf','application/rtf','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.rv','video/vnd.rn-realvideo','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.sgml','text/sgml','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.s3m','audio/s3m','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.snd','audio/basic','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.svf','image/vnd.dwg','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.svg','image/svg+xml','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.swf','application/x-shockwave-flash','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.tif','image/tiff','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.tiff','image/tiff','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.tsi','audio/tsp-audio','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.tsp','audio/tsplayer','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.turbot','image/florian','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.txt','text/plain','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.vdo','video/vdo','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.viv','video/vivo','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.vivo','video/vivo','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.voc','audio/voc','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.vos','video/vosaic','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.vox','audio/voxware','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.wax','audio/x-ms-wax','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.wav','audio/wav','application/x-mplayer2');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.wbmp','image/vnd.wap.wbmp','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.webm','video/webm','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.wmf','image/wmf','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.wma','audio/x-ms-wma','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.wmv','video/x-ms-wmv','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.wvx','video/x-ms-wvx','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.xbap','application/x-ms-xbap','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.xaml','application/xaml+xml','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.xlam','application/vnd.ms-excel.addin.macroEnabled.12','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.xls','application/vnd.ms-excel','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.xlsb','application/vnd.ms-excel.sheet.binary.macroEnabled.12','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.xlsm','application/vnd.ms-excel.sheet.macroEnabled.12','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.xlsx','application/vnd.openxmlformats-officedocument.spreadsheetml.sheet','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.xltm','application/vnd.ms-excel.template.macroEnabled.12','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.xltx','application/vnd.openxmlformats-officedocument.spreadsheetml.template','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.xif','image/vnd.xiff','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.xml','text/xml','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.xps','application/vnd.ms-xpsdocument','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.x-png','image/png','');
INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue]) VALUES ('.zip','application/octet-stream','');
GO

/* Table gs_BrowserTemplate */
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('image/*','default','<div class="gsp_i_c" style="width:{Width}px;"><img id="mo_img" src="{MediaObjectUrl}" class="{CssClass}" alt="{TitleNoHtml}" title="{TitleNoHtml}" style="height:{Height}px;width:{Width}px;" /></div>','');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('image/*','ie1to8','<div class="gsp_floatcontainer"><div class="op1"><div class="op2"><div class="sb"><div class="ib"><img id="mo_img" src="{MediaObjectUrl}" class="{CssClass}" alt="{TitleNoHtml}" title="{TitleNoHtml}" style="height:{Height}px;width:{Width}px;" /></div></div></div></div></div>','');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('audio/*','default','<object type="{MimeType}" data="{MediaObjectUrl}" style="width:{Width}px;height:{Height}px;" ><param name="autostart" value="{AutoStartMediaObjectInt}" /><param name="controller" value="true" /></object>','');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('audio/*','ie','<object classid="clsid:6BF52A52-394A-11D3-B153-00C04F79FAA6" standby="Loading audio..." style="width:{Width}px;height:{Height}px;"><param name="url" value="{MediaObjectUrl}" /><param name="src" value="{MediaObjectUrl}" /><param name="autostart" value="{AutoStartMediaObjectText}" /><param name="showcontrols" value="true" /></object>','');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('audio/m4a','default','<div id=''mp1p''></div>','if (_pageLoadHasFired) gsp_runSilverlight(); else Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(gsp_runSilverlight); function gsp_runSilverlight() { Sys.UI.Silverlight.Control.createObject(''mp1p'', ''<object type="application/x-silverlight" id="mp1" style="height:{Height}px;width:{Width}px;"><param name="Windowless" value="True" /><a href="http://go2.microsoft.com/fwlink/?LinkID=114576&amp;v=1.0"><img src="http://go2.microsoft.com/fwlink/?LinkID=108181" alt="Get Microsoft Silverlight" style="border-width:0;" /></a></object>''); Sys.Application.add_init(function() { $create(Sys.UI.Silverlight.MediaPlayer, { "mediaSource": "{MediaObjectUrl}", "scaleMode": 1, "source": "{GalleryPath}/skins/mediaplayer/AudioGray.xaml","autoPlay":{AutoStartMediaObjectText} }, null, null, $get("mp1p")); }); Sys.Application.initialize();Array.add(_mediaObjectsToDispose, "mp1");}');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('audio/ogg','default','<audio src="{MediaObjectUrl}" controls autobuffer {AutoPlay}><p>Cannot play: Your browser does not support the <code>audio</code> element or the codec of this file. Use another browser or download the file by clicking the download toolbar button above (available only when downloading is enabled).</p></audio>','');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('audio/ogg','ie','<p>Cannot play: Internet Explorer cannot play Ogg Theora files. Use another browser or download the file by clicking the download toolbar button above (available only when downloading is enabled).</p>','');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('audio/wav','default','<audio src="{MediaObjectUrl}" controls autobuffer {AutoPlay}><p>Cannot play: Your browser does not support the <code>audio</code> element or the codec of this file. Use another browser or download the file by clicking the download toolbar button above (available only when downloading is enabled).</p></audio>','');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('audio/wav','ie','<object classid="clsid:6BF52A52-394A-11D3-B153-00C04F79FAA6" standby="Loading audio..." style="width:{Width}px;height:{Height}px;"><param name="url" value="{MediaObjectUrl}" /><param name="src" value="{MediaObjectUrl}" /><param name="autostart" value="{AutoStartMediaObjectText}" /><param name="showcontrols" value="true" /></object>','');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('audio/x-mp3','default','<div id=''mp1p''></div>','if (_pageLoadHasFired) gsp_runSilverlight(); else Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(gsp_runSilverlight); function gsp_runSilverlight() { Sys.UI.Silverlight.Control.createObject(''mp1p'', ''<object type="application/x-silverlight" id="mp1" style="height:{Height}px;width:{Width}px;"><param name="Windowless" value="True" /><a href="http://go2.microsoft.com/fwlink/?LinkID=114576&amp;v=1.0"><img src="http://go2.microsoft.com/fwlink/?LinkID=108181" alt="Get Microsoft Silverlight" style="border-width:0;" /></a></object>''); Sys.Application.add_init(function() { $create(Sys.UI.Silverlight.MediaPlayer, { "mediaSource": "{MediaObjectUrl}", "scaleMode": 1, "source": "{GalleryPath}/skins/mediaplayer/AudioGray.xaml","autoPlay":{AutoStartMediaObjectText} }, null, null, $get("mp1p")); }); Sys.Application.initialize();Array.add(_mediaObjectsToDispose, "mp1");}');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('audio/x-ms-wma','default','<div id=''mp1p''></div>','if (_pageLoadHasFired) gsp_runSilverlight(); else Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(gsp_runSilverlight); function gsp_runSilverlight() { Sys.UI.Silverlight.Control.createObject(''mp1p'', ''<object type="application/x-silverlight" id="mp1" style="height:{Height}px;width:{Width}px;"><param name="Windowless" value="True" /><a href="http://go2.microsoft.com/fwlink/?LinkID=114576&amp;v=1.0"><img src="http://go2.microsoft.com/fwlink/?LinkID=108181" alt="Get Microsoft Silverlight" style="border-width:0;" /></a></object>''); Sys.Application.add_init(function() { $create(Sys.UI.Silverlight.MediaPlayer, { "mediaSource": "{MediaObjectUrl}", "scaleMode": 1, "source": "{GalleryPath}/skins/mediaplayer/AudioGray.xaml","autoPlay":{AutoStartMediaObjectText} }, null, null, $get("mp1p")); }); Sys.Application.initialize();Array.add(_mediaObjectsToDispose, "mp1");}');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('video/*','default','<object type="{MimeType}" data="{MediaObjectUrl}" style="width:{Width}px;height:{Height}px;" ><param name="src" value="{MediaObjectUrl}" /><param name="autostart" value="{AutoStartMediaObjectInt}" /></object>','');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('video/*','ie','<object type="{MimeType}" data="{MediaObjectUrl}" style="width:{Width}px;height:{Height}px;"><param name="src" value="{MediaObjectUrl}" /><param name="autostart" value="{AutoStartMediaObjectText}" /></object>','');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('video/ogg','default','<video src="{MediaObjectUrl}" controls autobuffer {AutoPlay}><p>Cannot play: Your browser does not support the <code>video</code> element or the codec of this file. Use another browser or download the file by clicking the download toolbar button above (available only when downloading is enabled).</p></video>','');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('video/ogg','ie','<p>Cannot play: Internet Explorer cannot play Ogg Theora files. Use another browser or download the file by clicking the download toolbar button above (available only when downloading is enabled).</p>','');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('video/x-ms-wmv','default','<div id=''mp1p''></div>','if (_pageLoadHasFired) gsp_runSilverlight(); else Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(gsp_runSilverlight); function gsp_runSilverlight() { Sys.UI.Silverlight.Control.createObject(''mp1p'', ''<object type="application/x-silverlight" id="mp1" style="height:{Height}px;width:{Width}px;"><param name="Windowless" value="True" /><a href="http://go2.microsoft.com/fwlink/?LinkID=114576&amp;v=1.0"><img src="http://go2.microsoft.com/fwlink/?LinkID=108181" alt="Get Microsoft Silverlight" style="border-width:0;" /></a></object>''); Sys.Application.add_init(function() { $create(Sys.UI.Silverlight.MediaPlayer, { "mediaSource": "{MediaObjectUrl}", "scaleMode": 1, "source": "{GalleryPath}/skins/mediaplayer/Professional.xaml","autoPlay":{AutoStartMediaObjectText} }, null, null, $get("mp1p")); }); Sys.Application.initialize();Array.add(_mediaObjectsToDispose, "mp1");}');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('video/mp4','default','<script type="text/javascript" src="{GalleryPath}/script/flowplayer-3.2.4.min.js"></script><a href="{MediaObjectUrl}" style="display:block;width:{Width}px;height:{Height}px" id="gsp_player"></a>','$get("gsp_player").href=$get("gsp_player").href.replace(/&/g, "%26");flowplayer("gsp_player", { src: "{GalleryPath}/script/flowplayer-3.2.5.swf", wmode: "opaque" }, { clip:{ autoPlay: {AutoStartMediaObjectText}, scaling: "fit" } });');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('video/m4v','default','<script type="text/javascript" src="{GalleryPath}/script/flowplayer-3.2.4.min.js"></script><a href="{MediaObjectUrl}" style="display:block;width:{Width}px;height:{Height}px" id="gsp_player"></a>','$get("gsp_player").href=$get("gsp_player").href.replace(/&/g, "%26");flowplayer("gsp_player", { src: "{GalleryPath}/script/flowplayer-3.2.5.swf", wmode: "opaque" }, { clip:{ autoPlay: {AutoStartMediaObjectText}, scaling: "fit" } });');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('video/x-ms-asf','default','<div id=''mp1p''></div>','if (_pageLoadHasFired) gsp_runSilverlight(); else Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(gsp_runSilverlight); function gsp_runSilverlight() { Sys.UI.Silverlight.Control.createObject(''mp1p'', ''<object type="application/x-silverlight" id="mp1" style="height:{Height}px;width:{Width}px;"><param name="Windowless" value="True" /><a href="http://go2.microsoft.com/fwlink/?LinkID=114576&amp;v=1.0"><img src="http://go2.microsoft.com/fwlink/?LinkID=108181" alt="Get Microsoft Silverlight" style="border-width:0;" /></a></object>''); Sys.Application.add_init(function() { $create(Sys.UI.Silverlight.MediaPlayer, { "mediaSource": "{MediaObjectUrl}", "scaleMode": 1, "source": "{GalleryPath}/skins/mediaplayer/Professional.xaml","autoPlay":{AutoStartMediaObjectText} }, null, null, $get("mp1p")); }); Sys.Application.initialize();Array.add(_mediaObjectsToDispose, "mp1");}');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('video/quicktime','default','<object type="{MimeType}" data="{MediaObjectUrl}" style="width:{Width}px;height:{Height}px;"><param name="autoplay" value="{AutoStartMediaObjectText}" /><param name="controller" value="true" /><param name="pluginurl" value="http://www.apple.com/quicktime/download/" /></object>','');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('video/quicktime','ie','<object classid="clsid:02BF25D5-8C17-4B23-BC80-D3488ABDDC6B" codebase="http://www.apple.com/qtactivex/qtplugin.cab" style="width:{Width}px;height:{Height}px;"><param name="src" value="{MediaObjectUrl}" /><param name="autoplay" value="{AutoStartMediaObjectText}" /><param name="controller" value="true" /><param name="pluginspage" value="http://www.apple.com/quicktime/download/" /></object>','');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('video/divx','default','<object type="{MimeType}" data="{HostUrl}{MediaObjectUrl}" style="width:{Width}px;height:{Height}px;"><param name="src" value="{HostUrl}{MediaObjectUrl}" /><param name="mode" value="full" /><param name="minVersion" value="1.0.0" /><param name="allowContextMenu" value="true" /><param name="autoPlay" value="{AutoStartMediaObjectText}" /><param name="loop" value="false" /><param name="bannerEnabled" value="false" /><param name="bufferingMode" value="auto" /><param name="previewMessage" value="Click to start video" /><param name="previewMessageFontSize" value="24" /><param name="movieTitle" value="{TitleNoHtml}" /></object>','');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('video/divx','ie','<object classid="clsid:67DABFBF-D0AB-41fa-9C46-CC0F21721616" codebase="http://go.divx.com/plugin/DivXBrowserPlugin.cab" style="width:{Width}px;height:{Height}px;"><param name="src" value="{HostUrl}{MediaObjectUrl}" /><param name="mode" value="full" /><param name="minVersion" value="1.0.0" /><param name="allowContextMenu" value="true" /><param name="autoPlay" value="{AutoStartMediaObjectText}" /><param name="loop" value="false" /><param name="bannerEnabled" value="false" /><param name="bufferingMode" value="auto" /><param name="previewMessage" value="Click to start video" /><param name="previewMessageFontSize" value="24" /><param name="movieTitle" value="{TitleNoHtml}" /></object>','');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('video/webm','default','<video src="{MediaObjectUrl}" controls autobuffer {AutoPlay}><p>Cannot play: Your browser does not support the <code>video</code> element or the codec of this file. Use another browser or download the file by clicking the download toolbar button above (available only when downloading is enabled).</p></video>','');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('application/x-shockwave-flash','default','<object type="{MimeType}" data="{MediaObjectUrl}" style="width:{Width}px;height:{Height}px;" id="flash_plugin" standby="loading movie..."><param name="movie" value="{MediaObjectUrl}" /><param name="allowScriptAccess" value="sameDomain" /><param name="quality" value="best" /><param name="wmode" value="opaque" /><param name="scale" value="default" /><param name="bgcolor" value="#FFFFFF" /><param name="salign" value="TL" /><param name="FlashVars" value="playerMode=embedded" /><p><strong>Cannot play Flash content</strong> Your browser does not have the Flash plugin or it is disabled. To view the content, install the Macromedia Flash plugin or, if it is already installed, enable it.</p></object>','');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('application/x-shockwave-flash','ie','<object type="{MimeType}" classid="clsid:D27CDB6E-AE6D-11cf-96B8-444553540000" codebase="http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=6,0,40,0&quot; id="flash_activex" standby="loading movie..." style="width:{Width}px;height:{Height}px;"><param name="movie" value="{MediaObjectUrl}" /><param name="quality" value="high" /><param name="wmode" value="opaque" /><param name="bgcolor" value="#FFFFFF" /><p><strong>Cannot play Flash content</strong> Your browser does not have the Flash plugin or it is disabled. To view the content, install the Macromedia Flash plugin or, if it is already installed, enable it.</p></object>','');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('application/x-shockwave-flash','ie5to9mac','<object type="{MimeType}" data="{MediaObjectUrl}" style="width:{Width}px;height:{Height}px;" id="flash_plugin" standby="loading movie..."><param name="movie" value="{MediaObjectUrl}" /><param name="allowScriptAccess" value="sameDomain" /><param name="quality" value="best" /><param name="scale" value="default" /><param name="bgcolor" value="#FFFFFF" /><param name="wmode" value="opaque" /><param name="salign" value="TL" /><param name="FlashVars" value="playerMode=embedded" /><strong>Cannot play Flash content</strong> Your browser does not have the Flash plugin or it is disabled. To view the content, install the Macromedia Flash plugin or, if it is already installed, enable it.</object>','');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('video/f4v','default','<script type="text/javascript" src="{GalleryPath}/script/flowplayer-3.2.4.min.js"></script><a href="{MediaObjectUrl}" style="display:block;width:{Width}px;height:{Height}px" id="gsp_player"></a>','$get("gsp_player").href=$get("gsp_player").href.replace(/&/g, "%26");flowplayer("gsp_player", { src: "{GalleryPath}/script/flowplayer-3.2.5.swf", wmode: "opaque" }, { clip:{ autoPlay: {AutoStartMediaObjectText}, scaling: "fit" } });');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('video/x-flv','default','<script type="text/javascript" src="{GalleryPath}/script/flowplayer-3.2.4.min.js"></script><a href="{MediaObjectUrl}" style="display:block;width:{Width}px;height:{Height}px" id="gsp_player"></a>','$get("gsp_player").href=$get("gsp_player").href.replace(/&/g, "%26");flowplayer("gsp_player", { src: "{GalleryPath}/script/flowplayer-3.2.5.swf", wmode: "opaque" }, { clip:{ autoPlay: {AutoStartMediaObjectText}, scaling: "fit" } });');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('application/pdf','default','<p><a href="{MediaObjectUrl}">Enlarge PDF to fit browser window</a></p><iframe src="{MediaObjectUrl}" frameborder="0" style="width:680px;height:600px;border:1px solid #000;"></iframe>','');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('text/plain','default','<p><a href="{MediaObjectUrl}">Enlarge file to fit browser window</a></p><iframe src="{MediaObjectUrl}" frameborder="0" style="width:680px;height:600px;border:1px solid #000;"></iframe>','');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('text/html','default','<p><a href="{MediaObjectUrl}">Enlarge file to fit browser window</a></p><iframe src="{MediaObjectUrl}" frameborder="0" style="width:680px;height:600px;border:1px solid #000;"></iframe>','');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('application/vnd.openxmlformats-officedocument.wordprocessingml.document','default','<p style="margin-bottom:5em;"><a href="{MediaObjectUrl}" title="Download {TitleNoHtml}">Download {TitleNoHtml}</a></p>','');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('application/msword','default','<p style="margin-bottom:5em;"><a href="{MediaObjectUrl}" title="Download {TitleNoHtml}">Download {TitleNoHtml}</a></p>','');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('message/rfc822','default','<p class="gsp_msgfriendly">This browser cannot display web archive files (.mht). Use Internet Explorer or download it by clicking the download link in the toolbar.</p>','');
INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate]) VALUES ('message/rfc822','ie','<p><a href="{MediaObjectUrl}">Enlarge to fit browser window</a></p><iframe src="{MediaObjectUrl}" frameborder="0" style="width:680px;height:600px;border:1px solid #000;"></iframe>','');
GO

/* Drop the function, since it isn't needed in 2.4. We do this last so that it still exists if there is a failure earlier in this script. */
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_GetVersion]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION {schema}[{objectQualifier}gs_GetVersion]
GO
