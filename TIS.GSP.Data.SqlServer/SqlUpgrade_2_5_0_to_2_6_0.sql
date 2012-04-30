/*
SQL CE: This script updates the database schema from 2.5.0 to 2.6.0.
This script provides the following changes:

1. Adds MIME type for .dv file.
2. Changes MIME type video/m4v to video/x-m4v.
3. Updates version #'s on FlowPlayer templates.
4. Adds browser templates for Safari.
5. Adds gallery settings MediaEncoderSettings and MediaEncoderTimeoutMs.
6. Create table gs_MediaQueue.
7. Add stored procedures for gs_MediaQueue table.
8. Update data schema version to 2.6.0

NOTE: If manually running this script, replace {schema} with the desired schema (such as "dbo.") and 
{objectQualifier} with a desired string to be prepended to object names (replace with empty string if no object
qualifier is desired).
*/

/* 1. Adds MIME type for .dv file. */
IF NOT EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_MimeType] WITH (UPDLOCK, HOLDLOCK) WHERE [FileExtension]='.dv')
BEGIN
	INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue])
	VALUES ('.dv','video/x-dv','');
END
GO

/* 2. Changes MIME type video/m4v to video/x-m4v */
UPDATE {schema}[{objectQualifier}gs_MimeType]
SET 
 [MimeTypeValue]='video/x-m4v'
WHERE [FileExtension]='.m4v';
GO

UPDATE {schema}[{objectQualifier}gs_BrowserTemplate]
SET [MimeType]='video/x-m4v'
WHERE [MimeType]='video/m4v'
GO

/* 3. Updates version #'s on FlowPlayer templates */
UPDATE {schema}[{objectQualifier}gs_BrowserTemplate]
SET 
 [HtmlTemplate]=REPLACE(HtmlTemplate, 'flowplayer-3.2.4.min.js', 'flowplayer-3.2.6.min.js'),
 [ScriptTemplate]=REPLACE(ScriptTemplate, 'flowplayer-3.2.4.min.js', 'flowplayer-3.2.6.min.js')
GO

UPDATE {schema}[{objectQualifier}gs_BrowserTemplate]
SET 
 [HtmlTemplate]=REPLACE(HtmlTemplate, 'flowplayer-3.2.5.swf', 'flowplayer-3.2.7.swf'),
 [ScriptTemplate]=REPLACE(ScriptTemplate, 'flowplayer-3.2.5.swf', 'flowplayer-3.2.7.swf')
GO

/* 4. Adds browser templates for Safari */
IF NOT EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_BrowserTemplate] WITH (UPDLOCK, HOLDLOCK) WHERE [MimeType]='video/mp4' AND [BrowserId]='safari')
BEGIN
	INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate])
	VALUES ('video/mp4','safari','<video src="{MediaObjectUrl}" controls autobuffer {AutoPlay}><p>Cannot play: Your browser does not support the <code>video</code> element or the codec of this file. Use another browser or download the file by clicking the download toolbar button above (available only when downloading is enabled).</p></video>', '');
END
GO

IF NOT EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_BrowserTemplate] WITH (UPDLOCK, HOLDLOCK) WHERE [MimeType]='video/x-m4v' AND [BrowserId]='safari')
BEGIN
	INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate])
	VALUES ('video/x-m4v','safari','<video src="{MediaObjectUrl}" controls autobuffer {AutoPlay}><p>Cannot play: Your browser does not support the <code>video</code> element or the codec of this file. Use another browser or download the file by clicking the download toolbar button above (available only when downloading is enabled).</p></video>', '');
END
GO

IF NOT EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_BrowserTemplate] WITH (UPDLOCK, HOLDLOCK) WHERE [MimeType]='video/quicktime' AND [BrowserId]='safari')
BEGIN
	INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate])
	VALUES ('video/quicktime','safari','<video src="{MediaObjectUrl}" controls autobuffer {AutoPlay}><p>Cannot play: Your browser does not support the <code>video</code> element or the codec of this file. Use another browser or download the file by clicking the download toolbar button above (available only when downloading is enabled).</p></video>', '');
END
GO

IF NOT EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_BrowserTemplate] WITH (UPDLOCK, HOLDLOCK) WHERE [MimeType]='audio/x-mp3' AND [BrowserId]='safari')
BEGIN
	INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate])
	VALUES ('audio/x-mp3','safari','<audio src="{MediaObjectUrl}" controls autobuffer {AutoPlay}><p>Cannot play: Your browser does not support the <code>audio</code> element or the codec of this file. Use another browser or download the file by clicking the download toolbar button above (available only when downloading is enabled).</p></audio>', '');
END
GO

IF NOT EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_BrowserTemplate] WITH (UPDLOCK, HOLDLOCK) WHERE [MimeType]='audio/m4a' AND [BrowserId]='safari')
BEGIN
	INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate])
	VALUES ('audio/m4a','safari','<audio src="{MediaObjectUrl}" controls autobuffer {AutoPlay}><p>Cannot play: Your browser does not support the <code>audio</code> element or the codec of this file. Use another browser or download the file by clicking the download toolbar button above (available only when downloading is enabled).</p></audio>', '');
END
GO

/* 5. Adds gallery settings MediaEncoderSettings and MediaEncoderTimeoutMs */
IF NOT EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_GallerySetting] WITH (UPDLOCK, HOLDLOCK) WHERE [SettingName]='MediaEncoderSettings')
BEGIN
 INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue])
 VALUES (-2147483648,1,'MediaEncoderSettings','.mp3||.mp3||~~.mp4||.mp4||~~.flv||.flv||~~.m4a||.m4a||~~*video||.flv||-i "{SourceFilePath}" -y "{DestinationFilePath}"~~*video||.mp4||-i "{SourceFilePath}" -y -vcodec libx264 -fpre "{BinPath}\libx264-medium.ffpreset" "{DestinationFilePath}"~~*audio||.m4a||-i "{SourceFilePath}" -y "{DestinationFilePath}"');
END
GO

IF NOT EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_GallerySetting] WITH (UPDLOCK, HOLDLOCK) WHERE [SettingName]='MediaEncoderTimeoutMs')
BEGIN
 INSERT INTO {schema}[{objectQualifier}gs_GallerySetting] ([FKGalleryId], [IsTemplate], [SettingName], [SettingValue])
 VALUES (-2147483648,1,'MediaEncoderTimeoutMs','900000');
END
GO

/* 6. Create table gs_MediaQueue */
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}FK_gs_MediaQueue_gs_MediaObject]') AND parent_object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_MediaQueue]'))
ALTER TABLE {schema}[{objectQualifier}gs_MediaQueue] DROP CONSTRAINT [{objectQualifier}FK_gs_MediaQueue_gs_MediaObject]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_MediaQueue]') AND type in (N'U'))
DROP TABLE {schema}[{objectQualifier}gs_MediaQueue]
GO

CREATE TABLE {schema}[{objectQualifier}gs_MediaQueue] (
	[MediaQueueId] [int] IDENTITY(1, 1), 
	[FKMediaObjectId] [int] NOT NULL,
	[Status] [nvarchar](256) NOT NULL,
	[StatusDetail] [ntext] NOT NULL,
	[DateAdded] [datetime] NOT NULL,
	[DateConversionStarted] [datetime] NULL,
	[DateConversionCompleted] [datetime] NULL,
 CONSTRAINT [{objectQualifier}PK_gs_MediaQueue] PRIMARY KEY CLUSTERED 
(
	[MediaQueueId] ASC
) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY];
GO

ALTER TABLE {schema}[{objectQualifier}gs_MediaQueue] WITH NOCHECK
ADD CONSTRAINT [{objectQualifier}FK_gs_MediaQueue_gs_MediaObject] FOREIGN KEY([FKMediaObjectId])
REFERENCES {schema}[{objectQualifier}gs_MediaObject] ([MediaObjectId])
ON DELETE CASCADE
GO

/* 7. Add stored procedures for gs_MediaQueue table */
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_MediaQueueDelete]') AND type in (N'P', N'PC'))
DROP PROCEDURE {schema}[{objectQualifier}gs_MediaQueueDelete]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_MediaQueueInsert]') AND type in (N'P', N'PC'))
DROP PROCEDURE {schema}[{objectQualifier}gs_MediaQueueInsert]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_MediaQueueSelect]') AND type in (N'P', N'PC'))
DROP PROCEDURE {schema}[{objectQualifier}gs_MediaQueueSelect]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_MediaQueueUpdate]') AND type in (N'P', N'PC'))
DROP PROCEDURE {schema}[{objectQualifier}gs_MediaQueueUpdate]
GO

CREATE PROCEDURE {schema}[{objectQualifier}gs_MediaQueueDelete]
( @MediaQueueId int )
AS

DELETE FROM {schema}[{objectQualifier}gs_MediaQueue]
WHERE MediaQueueId = @MediaQueueId
GO

CREATE PROCEDURE {schema}[{objectQualifier}gs_MediaQueueInsert]
(@FKMediaObjectId int,@Status nvarchar(256),@StatusDetail nvarchar(max),
 @DateAdded datetime,@DateConversionStarted datetime,@DateConversionCompleted datetime,
 @Identity int OUT)
AS
SET NOCOUNT ON

INSERT INTO {schema}[{objectQualifier}gs_MediaQueue]
 ([FKMediaObjectId],[Status],[StatusDetail],[DateAdded],[DateConversionStarted],[DateConversionCompleted])
VALUES
 (@FKMediaObjectId,@Status,@StatusDetail,@DateAdded,@DateConversionStarted,@DateConversionCompleted)

SET @Identity = SCOPE_IDENTITY()

RETURN
GO

CREATE PROCEDURE {schema}[{objectQualifier}gs_MediaQueueSelect]
AS
SET NOCOUNT ON

SELECT
 [MediaQueueId],[FKMediaObjectId],[Status],[StatusDetail],
 [DateAdded],[DateConversionStarted],[DateConversionCompleted]
FROM {schema}[{objectQualifier}gs_MediaQueue]
ORDER BY DateAdded;

RETURN
GO

CREATE PROCEDURE {schema}[{objectQualifier}gs_MediaQueueUpdate]
(@MediaQueueId int, @FKMediaObjectId int,@Status nvarchar(256),@StatusDetail nvarchar(max),
 @DateAdded datetime,@DateConversionStarted datetime,@DateConversionCompleted datetime)
AS
SET NOCOUNT ON

UPDATE {schema}[{objectQualifier}gs_MediaQueue]
SET
 [FKMediaObjectId] = @FKMediaObjectId,
 [Status] = @Status,
 [StatusDetail] = @StatusDetail,
 [DateAdded] = @DateAdded,
 [DateConversionStarted] = @DateConversionStarted,
 [DateConversionCompleted] = @DateConversionCompleted
WHERE MediaQueueId = @MediaQueueId
GO

GRANT EXECUTE ON {schema}[{objectQualifier}gs_MediaQueueDelete] TO [{objectQualifier}gs_GalleryServerProRole]
GO
GRANT EXECUTE ON {schema}[{objectQualifier}gs_MediaQueueInsert] TO [{objectQualifier}gs_GalleryServerProRole]
GO
GRANT EXECUTE ON {schema}[{objectQualifier}gs_MediaQueueSelect] TO [{objectQualifier}gs_GalleryServerProRole]
GO
GRANT EXECUTE ON {schema}[{objectQualifier}gs_MediaQueueUpdate] TO [{objectQualifier}gs_GalleryServerProRole]
GO

/* 8. Update data schema version to 2.6.0 */
UPDATE {schema}[{objectQualifier}gs_AppSetting]
SET [SettingValue] = '2.6.0'
WHERE [SettingName] = 'DataSchemaVersion';
GO