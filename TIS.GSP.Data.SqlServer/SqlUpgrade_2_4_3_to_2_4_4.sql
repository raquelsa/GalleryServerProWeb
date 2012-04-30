/*
SQL Server: This script updates the database schema from 2.4.3 to 2.4.4.
This script provides the following changes:

1. Updates browser template for Silverlight to ensure javascript runs after page load event fires.

*/
UPDATE {schema}[{objectQualifier}gs_BrowserTemplate]
SET 
 [ScriptTemplate]='if (_pageLoadHasFired) gsp_runSilverlight(); else Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(gsp_runSilverlight); function gsp_runSilverlight() { Sys.UI.Silverlight.Control.createObject(''mp1p'', ''<object type="application/x-silverlight" id="mp1" style="height:{Height}px;width:{Width}px;"><param name="Windowless" value="True" /><a href="http://go2.microsoft.com/fwlink/?LinkID=114576&amp;v=1.0"><img src="http://go2.microsoft.com/fwlink/?LinkID=108181" alt="Get Microsoft Silverlight" style="border-width:0;" /></a></object>''); Sys.Application.add_init(function() { $create(Sys.UI.Silverlight.MediaPlayer, { "mediaSource": "{MediaObjectUrl}", "scaleMode": 1, "source": "{GalleryPath}/skins/mediaplayer/Professional.xaml","autoPlay":{AutoStartMediaObjectText} }, null, null, $get("mp1p")); }); Sys.Application.initialize();Array.add(_mediaObjectsToDispose, "mp1");}'
WHERE [MimeType] IN ('video/x-ms-wmv','video/x-ms-asf');
GO

UPDATE {schema}[{objectQualifier}gs_BrowserTemplate]
SET 
 [ScriptTemplate]='if (_pageLoadHasFired) gsp_runSilverlight(); else Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(gsp_runSilverlight); function gsp_runSilverlight() { Sys.UI.Silverlight.Control.createObject(''mp1p'', ''<object type="application/x-silverlight" id="mp1" style="height:{Height}px;width:{Width}px;"><param name="Windowless" value="True" /><a href="http://go2.microsoft.com/fwlink/?LinkID=114576&amp;v=1.0"><img src="http://go2.microsoft.com/fwlink/?LinkID=108181" alt="Get Microsoft Silverlight" style="border-width:0;" /></a></object>''); Sys.Application.add_init(function() { $create(Sys.UI.Silverlight.MediaPlayer, { "mediaSource": "{MediaObjectUrl}", "scaleMode": 1, "source": "{GalleryPath}/skins/mediaplayer/AudioGray.xaml","autoPlay":{AutoStartMediaObjectText} }, null, null, $get("mp1p")); }); Sys.Application.initialize();Array.add(_mediaObjectsToDispose, "mp1");}'
WHERE [MimeType] IN ('audio/m4a','audio/x-mp3','audio/x-ms-wma');
GO

UPDATE {schema}[{objectQualifier}gs_AppSetting]
SET [SettingValue] = '2.4.4'
WHERE [SettingName] = 'DataSchemaVersion';
GO

