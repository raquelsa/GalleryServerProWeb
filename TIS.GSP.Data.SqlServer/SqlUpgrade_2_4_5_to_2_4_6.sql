/*
SQL Server: This script updates the database schema from 2.4.5 to 2.4.6.
This script provides the following changes:

1. Edit stored proc to remove unnecessary join (improves synchs).
2. Add a non-clustered index to the gs_MediaObject table (improves searches).
3. Adds a non-clustered index to the gs_Album table (improves synchs).
4. Adds four statistics (improves synchs).
5. Update app settings.
6. Update gallery settings.

*/

/* 1. Edit stored proc to remove unnecessary join (improves synchs). */
ALTER PROCEDURE {schema}[{objectQualifier}gs_MediaObjectSelectHashKeys]
AS
SET NOCOUNT ON

SELECT HashKey
FROM {schema}[{objectQualifier}gs_MediaObject]

RETURN
GO

/* 2. Add a non-clustered index to the gs_MediaObject table (improves searches). */
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_MediaObject]') AND name = N'IDX_gs_MediaObject_MediaObjectId_FKAlbumId')
CREATE NONCLUSTERED INDEX [IDX_gs_MediaObject_MediaObjectId_FKAlbumId] ON {schema}[{objectQualifier}gs_MediaObject] 
( 
 [OriginalFilename] ASC
)
 INCLUDE ([MediaObjectId], [FKAlbumId])
 WITH (PAD_INDEX=OFF, STATISTICS_NORECOMPUTE=OFF, SORT_IN_TEMPDB=OFF, IGNORE_DUP_KEY=OFF, DROP_EXISTING=OFF, ONLINE=OFF, ALLOW_ROW_LOCKS=ON, ALLOW_PAGE_LOCKS=ON) ON [PRIMARY]
GO

/* 3. Adds a non-clustered index to the gs_Album table (improves synchs). */
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_Album]') AND name = N'IDX_gs_Album_AlbumId')
CREATE NONCLUSTERED INDEX [IDX_gs_Album_AlbumId] ON {schema}[{objectQualifier}gs_Album] 
(
 [AlbumId] ASC
)
 WITH (SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
GO

/* 4. Adds four statistics (improves synchs). */
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
CREATE STATISTICS [STAT_gs_MediaObjectMetadata_MediaObjectMetadataId_FKMediaObjectId] ON {schema}[{objectQualifier}gs_MediaObjectMetadata]([MediaObjectMetadataId], [FKMediaObjectId])
GO

/* 5. Update app settings. */
UPDATE {schema}[{objectQualifier}gs_AppSetting]
SET [SettingValue] = '//ajax.googleapis.com/ajax/libs/jquery/1/jquery.min.js'
WHERE [SettingName] = 'JQueryScriptPath';
GO

IF NOT EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_AppSetting] WITH (UPDLOCK, HOLDLOCK) WHERE [SettingName]='JQueryUiScriptPath')
BEGIN
 INSERT INTO {schema}[{objectQualifier}gs_AppSetting] ([SettingName], [SettingValue])
 VALUES ('JQueryUiScriptPath','//ajax.googleapis.com/ajax/libs/jqueryui/1/jquery-ui.min.js');
END
GO

/* 6. Update gallery settings. */
IF NOT EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_GallerySetting] WITH (UPDLOCK, HOLDLOCK) WHERE [SettingName]='DiscardOriginalImageDuringImport')
BEGIN
 INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue])
 VALUES (-2147483648,1,'DiscardOriginalImageDuringImport','False');
END
GO

IF NOT EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_GallerySetting] WITH (UPDLOCK, HOLDLOCK) WHERE [SettingName]='EnableAlbumZipDownload')
BEGIN
 INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue])
 VALUES (-2147483648,1,'EnableAlbumZipDownload','True');
END
GO

UPDATE {schema}[{objectQualifier}gs_GallerySetting]
SET [SettingName] = 'EnableGalleryObjectZipDownload'
WHERE [SettingName] = 'EnableMediaObjectZipDownload';
GO

IF NOT EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_GallerySetting] WITH (UPDLOCK, HOLDLOCK) WHERE [SettingName]='EnableAutoSync')
BEGIN
 INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue])
 VALUES (-2147483648,1,'EnableAutoSync','False');
END
GO

IF NOT EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_GallerySetting] WITH (UPDLOCK, HOLDLOCK) WHERE [SettingName]='AutoSyncIntervalMinutes')
BEGIN
 INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue])
 VALUES (-2147483648,1,'AutoSyncIntervalMinutes','1440');
END
GO

IF NOT EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_GallerySetting] WITH (UPDLOCK, HOLDLOCK) WHERE [SettingName]='LastAutoSync')
BEGIN
 INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue])
 VALUES (-2147483648,1,'LastAutoSync','');
END
GO

IF NOT EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_GallerySetting] WITH (UPDLOCK, HOLDLOCK) WHERE [SettingName]='EnableRemoteSync')
BEGIN
 INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue])
 VALUES (-2147483648,1,'EnableRemoteSync','False');
END
GO

IF NOT EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_GallerySetting] WITH (UPDLOCK, HOLDLOCK) WHERE [SettingName]='RemoteAccessPassword')
BEGIN
 INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue])
 VALUES (-2147483648,1,'RemoteAccessPassword','');
END
GO

IF NOT EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_GallerySetting] WITH (UPDLOCK, HOLDLOCK) WHERE [SettingName]='MediaObjectCaptionTemplate')
BEGIN
 INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue])
 VALUES (-2147483648,1,'MediaObjectCaptionTemplate','{Title}');
END
GO

IF NOT EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_GallerySetting] WITH (UPDLOCK, HOLDLOCK) WHERE [SettingName]='MetadataDisplaySettings')
BEGIN
 INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue])
 VALUES (-2147483648,1,'MetadataDisplaySettings','29:T,34:T,35:F,8:T,102:T,106:T,22:T,14:T,9:T,5:T,28:T,2:T,26:T,4:T,6:T,7:T,12:T,13:T,15:T,16:T,17:T,18:T,21:T,23:T,24:T,10:T,25:T,27:T,11:T,1:T,32:T,3:T,0:T,31:T,20:T,30:T,33:T,19:T,36:T,37:F,38:T,39:F,40:T,101:F,103:F,104:F,105:F,108:F,107:F,110:T,109:T,1012:T,1013:T,1010:T,1011:T,1014:T,1017:T,1018:T,1015:T,1016:T,1003:T,1004:T,1001:T,1002:T,1005:T,1008:T,1009:T,1006:T,1007:T');
END
GO

IF NOT EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_GallerySetting] WITH (UPDLOCK, HOLDLOCK) WHERE [SettingName]='GpsMapUrlTemplate')
BEGIN
 INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue])
 VALUES (-2147483648,1,'GpsMapUrlTemplate','<a href=''http://bing.com/maps/default.aspx?sp=point.{GpsLatitude}_{GpsLongitude}_{TitleNoHtml}__{MediaObjectPageUrl}_{MediaObjectUrl}&style=a&lvl=13'' target=''_blank'' title=''View map''>{GpsLocation}</a>');
END
GO

UPDATE {schema}[{objectQualifier}gs_AppSetting]
SET [SettingValue] = '2.4.6'
WHERE [SettingName] = 'DataSchemaVersion';
GO