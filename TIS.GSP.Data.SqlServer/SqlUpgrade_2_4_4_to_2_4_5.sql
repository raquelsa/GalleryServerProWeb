/*
SQL Server: This script updates the database schema from 2.4.4 to 2.4.5.
This script provides the following changes:

1. Adds application settings AllowGalleryAdminToManageUsersAndRoles and AllowGalleryAdminToViewAllUsersAndRoles.
2. Updates schema to latest version.

*/
IF NOT EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_AppSetting] WITH (UPDLOCK, HOLDLOCK) WHERE [SettingName]='AllowGalleryAdminToManageUsersAndRoles')
BEGIN
	INSERT INTO {schema}[{objectQualifier}gs_AppSetting] ([SettingName], [SettingValue])
	VALUES ('AllowGalleryAdminToManageUsersAndRoles','True');
END
GO

IF NOT EXISTS (SELECT * FROM {schema}[{objectQualifier}gs_AppSetting] WITH (UPDLOCK, HOLDLOCK) WHERE [SettingName]='AllowGalleryAdminToViewAllUsersAndRoles')
BEGIN
	INSERT INTO {schema}[{objectQualifier}gs_AppSetting] ([SettingName], [SettingValue])
	VALUES ('AllowGalleryAdminToViewAllUsersAndRoles','True');
END
GO

UPDATE {schema}[{objectQualifier}gs_AppSetting]
SET [SettingValue] = '2.4.5'
WHERE [SettingName] = 'DataSchemaVersion';
GO