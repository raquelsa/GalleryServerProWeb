namespace GalleryServerPro.Business
{
	/// <summary>
	/// Contains maximum allowed lengths of various values used in Gallery Server. These are used by the data provider to specify the 
	/// length of certain data fields and also by the UI and business layer to enforce maximum lengths.
	/// </summary>
	public static class DataConstants
	{
		public const int NVarCharMaxLength = -1; // The parameter length mapped to nvarchar(max) definitions
		public const int AlbumTitleLength = 1000;
		public const int AlbumDirectoryNameLength = 255;
		public const int AlbumSummaryLength = NVarCharMaxLength;
		public const int MediaObjectTitleLength = NVarCharMaxLength;
		public const int MediaObjectFileNameLength = 255;
		public const int MediaObjectHashKeyLength = 47;
		public const int MediaObjectExternalHtmlSourceLength = NVarCharMaxLength;
		public const int MediaObjectExternalTypeLength = 15;
		public const int MediaObjectMetadataDescriptionLength = 200;
		public const int MediaObjectMetadataValueLength = NVarCharMaxLength;
		public const int RoleNameLength = 256;
		public const int OwnedByLength = 256;
		public const int UserNameLength = 256;
		public const int OwnerRoleNameLength = 256;
		public const int CreatedByLength = 256;
		public const int LastModifiedByLength = 256;
		public const int ErrorExTypeLength = 1000;
		public const int ErrorExMsgLength = 4000;
		public const int ErrorExSourceLength = 1000;
		public const int ErrorUrlLength = 1000;
		public const int GalleryDescriptionLength = 1000;
		public const int SettingNameLength = 200;
		public const int SettingValueLength = NVarCharMaxLength;
		public const int GalleryControlIdLength = 350;
		public const int MimeTypeValueLength = 200;
		public const int MimeTypeBrowserValueLength = 200;
		public const int MimeTypeFileExtensionLength = 10;
		public const int BrowserTemplateBrowserIdLength = 50;
		public const int BrowserTemplateHtmlLength = NVarCharMaxLength;
		public const int BrowserTemplateScriptLength = NVarCharMaxLength;
		public const int MediaQueueStatusLength = 256;
		public const int MediaQueueStatusDetailLength = NVarCharMaxLength;
	}
}
