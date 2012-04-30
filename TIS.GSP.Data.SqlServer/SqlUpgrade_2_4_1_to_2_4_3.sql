/*
SQL Server: This script updates the database schema from 2.4.1 to 2.4.3.
This script provides the following changes:

1. Adds MIME type for .f4v file.
2. Adds browser template for .f4v file.
3. Updates browser template for .flv, .mp4, and .m4v files to use FlowPlayer instead of Silverlight.
4. Ensures gallery setting ImageMagickFileTypes contains .tif and .tiff files.
5. Update data schema version to 2.4.3.

*/

IF NOT EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_MimeType] WITH (UPDLOCK, HOLDLOCK) WHERE [FileExtension]='.f4v')
BEGIN
	INSERT INTO {schema}[{objectQualifier}gs_MimeType] ([FileExtension], [MimeTypeValue], [BrowserMimeTypeValue])
	VALUES ('.f4v','video/f4v','');
END
GO

IF NOT EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_BrowserTemplate] WITH (UPDLOCK, HOLDLOCK) WHERE [MimeType]='video/f4v')
BEGIN
	INSERT INTO {schema}[{objectQualifier}gs_BrowserTemplate] ([MimeType], [BrowserId], [HtmlTemplate], [ScriptTemplate])
	VALUES ('video/f4v','default','<script type="text/javascript" src="{GalleryPath}/script/flowplayer-3.2.4.min.js"></script><a href="{MediaObjectUrl}" style="display:block;width:{Width}px;height:{Height}px" id="gsp_player"></a>','$get("gsp_player").href=$get("gsp_player").href.replace(/&/g, "%26");flowplayer("gsp_player", { src: "{GalleryPath}/script/flowplayer-3.2.5.swf", wmode: "opaque" }, { clip:{ autoPlay: {AutoStartMediaObjectText}, scaling: "fit" } });');
END
GO

UPDATE {schema}[{objectQualifier}gs_BrowserTemplate]
SET 
 [HtmlTemplate]='<script type="text/javascript" src="{GalleryPath}/script/flowplayer-3.2.4.min.js"></script><a href="{MediaObjectUrl}" style="display:block;width:{Width}px;height:{Height}px" id="gsp_player"></a>',
 [ScriptTemplate]='$get("gsp_player").href=$get("gsp_player").href.replace(/&/g, "%26");flowplayer("gsp_player", { src: "{GalleryPath}/script/flowplayer-3.2.5.swf", wmode: "opaque" }, { clip:{ autoPlay: {AutoStartMediaObjectText}, scaling: "fit" } });'
WHERE [MimeType] IN ('video/x-flv','video/mp4','video/m4v');
GO

UPDATE {schema}[{objectQualifier}gs_GallerySetting]
SET [SettingValue] = 'pdf,.txt,.eps,.psd,.tif,.tiff'
WHERE [SettingName] = 'ImageMagickFileTypes';
GO

UPDATE {schema}[{objectQualifier}gs_GallerySetting]
SET [SettingValue] = '.mp3,.wma,.wmv,.asf,.asx,.m4a'
WHERE [SettingName] = 'SilverlightFileTypes';
GO

UPDATE {schema}[{objectQualifier}gs_AppSetting]
SET [SettingValue] = '2.4.3'
WHERE [SettingName] = 'DataSchemaVersion';
GO
