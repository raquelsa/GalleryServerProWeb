/*
SQL Server: This script updates the database schema from 2.4.6 to 2.5.0.
This script provides the following changes:

1. Delete FK_gs_Album_gs_Album (it wasn't doing anything useful)
2. Delete stored proc gs_Role_AlbumSelectAllAlbumsByRoleName
3. Delete stored proc gs_SelectRootAlbum
4. Modify gs_GalleryInsert to take DateAdded parameter
5. (bug fix) Insert missing period in front of pdf in ImageMagickFileTypes
6. Increase the transition time when navigating media objects from .2 to .3 seconds.
7. Remove JOIN from stored proc gs_MediaObjectMetadataSelect
8. Modify gs_RoleSelect to include FKAlbumId
9. Add additional columns to stored proc gs_MimeTypeGallerySelect
10. Add stored proc gs_AlbumSelectAll
11. Update data schema version to 2.5.0

NOTE: If manually running this script, replace {schema} with the desired schema (such as "dbo.") and 
{objectQualifier} with a desired string to be prepended to object names (replace with empty string if no object
qualifier is desired).
*/

/* 1. Delete FK_gs_Album_gs_Album */
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}FK_gs_Album_gs_Album]') AND parent_object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_Album]'))
ALTER TABLE {schema}[{objectQualifier}gs_Album] DROP CONSTRAINT [{objectQualifier}FK_gs_Album_gs_Album]
GO

/* 2. Delete stored proc gs_Role_AlbumSelectAllAlbumsByRoleName */
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_Role_AlbumSelectAllAlbumsByRoleName]') AND type in (N'P', N'PC'))
DROP PROCEDURE {schema}[{objectQualifier}gs_Role_AlbumSelectAllAlbumsByRoleName]
GO

/* 3. Delete stored proc gs_SelectRootAlbum */
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_SelectRootAlbum]') AND type in (N'P', N'PC'))
DROP PROCEDURE {schema}[{objectQualifier}gs_SelectRootAlbum]
GO

/* 4. Modify gs_GalleryInsert to take DateAdded parameter */
ALTER PROCEDURE {schema}[{objectQualifier}gs_GalleryInsert]
(
	@Description nvarchar(1000), @DateAdded datetime, @Identity int OUT
)
AS
SET NOCOUNT ON

INSERT {schema}[{objectQualifier}gs_Gallery] (Description, DateAdded)
VALUES (@Description, @DateAdded)
 
SET @Identity = SCOPE_IDENTITY()
GO

/* 5. Insert missing period in front of pdf in ImageMagickFileTypes */
UPDATE {schema}[{objectQualifier}gs_GallerySetting]
SET [SettingValue] = '.pdf,.txt,.eps,.psd,.tif,.tiff'
WHERE [SettingName] = 'ImageMagickFileTypes';
GO

/* 5. Increase the transition time when navigating media objects from .2 to .3 seconds. */
UPDATE {schema}[{objectQualifier}gs_GallerySetting]
SET [SettingValue] = '0.3'
WHERE [SettingName] = 'MediaObjectTransitionDuration';
GO

/* 7. Remove JOIN from stored proc gs_MediaObjectMetadataSelect */
ALTER PROCEDURE {schema}[{objectQualifier}gs_MediaObjectMetadataSelect]
(
	@MediaObjectId int
)
AS
SET NOCOUNT ON

SELECT
	MediaObjectMetadataId, FKMediaObjectId, MetadataNameIdentifier, Description, Value
FROM {schema}[{objectQualifier}gs_MediaObjectMetadata]
WHERE FKMediaObjectId = @MediaObjectId

RETURN
GO

/* 8. Modify gs_RoleSelect to include FKAlbumId */
ALTER PROCEDURE {schema}[{objectQualifier}gs_RoleSelect]

AS
SET NOCOUNT ON

SELECT r.RoleName, r.AllowViewAlbumsAndObjects, r.AllowViewOriginalImage, r.AllowAddChildAlbum,
	r.AllowAddMediaObject, r.AllowEditAlbum, r.AllowEditMediaObject, r.AllowDeleteChildAlbum, 
	r.AllowDeleteMediaObject, r.AllowSynchronize, r.HideWatermark, r.AllowAdministerGallery, 
	r.AllowAdministerSite, ra.FKAlbumId
FROM {schema}[{objectQualifier}gs_Role] r LEFT OUTER JOIN {schema}[{objectQualifier}gs_Role_Album] ra ON r.RoleName = ra.FKRoleName
ORDER BY r.RoleName

RETURN
GO

/* 9. Add additional columns to stored proc gs_MimeTypeGallerySelect */
ALTER PROCEDURE {schema}[{objectQualifier}gs_MimeTypeGallerySelect]

AS
SET NOCOUNT ON

SELECT mtg.MimeTypeGalleryId, mtg.FKGalleryId, mtg.FKMimeTypeId, mtg.IsEnabled,
 mt.FileExtension, mt.MimeTypeValue, mt.BrowserMimeTypeValue
FROM {schema}[{objectQualifier}gs_MimeType] mt INNER JOIN {schema}[{objectQualifier}gs_MimeTypeGallery] mtg ON mt.MimeTypeId = mtg.FKMimeTypeId
ORDER BY mt.FileExtension;

RETURN
GO

/* 10. Add stored proc gs_AlbumSelectAll */
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{schema}[{objectQualifier}gs_AlbumSelectAll]') AND type in (N'P', N'PC'))
DROP PROCEDURE {schema}[{objectQualifier}gs_AlbumSelectAll]
GO

CREATE PROCEDURE {schema}[{objectQualifier}gs_AlbumSelectAll]
(
	@GalleryId int
)
AS
SET NOCOUNT ON

SELECT AlbumId, AlbumParentId
FROM {schema}[{objectQualifier}gs_Album]
WHERE FKGalleryId = @GalleryId

RETURN
GO

GRANT EXECUTE ON {schema}[{objectQualifier}gs_AlbumSelectAll] TO [{objectQualifier}gs_GalleryServerProRole]
GO

/* 11. Update data schema version to 2.5.0 */
UPDATE {schema}[{objectQualifier}gs_AppSetting]
SET [SettingValue] = '2.5.0'
WHERE [SettingName] = 'DataSchemaVersion';
GO