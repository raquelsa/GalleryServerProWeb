namespace GalleryServerPro.Web
{
	/// <summary>
	/// Contains constant values used within Gallery Server Pro.
	/// </summary>
	public static class Constants
	{
		public const string APP_NAME = "Gallery Server Pro";
		public const string SAMPLE_IMAGE_FILENAME = "RogerMartin&Family.jpeg"; // The name of an embedded resource in the gs\images\ directory
		public const string ENCRYPTION_KEY = "mNU-h7:5f_)3=c%@^}#U9Tn*"; // The default encryption key as stored in a new installation. It is updated
																																			// to a new value the first time the application is run.

		// Name of SQL Server connection string in web.config. Note that if you change this,
		// also change it in the Installer_Finished_WebCfg_Need_Updating_Dtl resource setting.
		public const string SQLITE_CN_STRING_NAME = "SQLiteDbConnection";
		public const string SQL_SERVER_CN_STRING_NAME = "SqlServerDbConnection";
		public const string SQLCE_STRING_NAME = "SqlServerCeGalleryDb";

		// Note this field is also defined in GalleryServerPro.Business.DataConstants. We also define it here because DotNetNuke has a 50-char
		// limit but we don't want to change the value going to the stored procs, so we "override" it here.
		public const int RoleNameLength = 256;
	}

	#region Public Enums

	/// <summary>
	/// Specifies a distinct web page within Gallery Server Pro.
	/// </summary>
	public enum PageId
	{
		none = 0,
		admin_albums = 1,
		admin_backuprestore,
		admin_errorlog,
		admin_sitesettings,
		admin_galleries,
		admin_gallerysettings,
		admin_gallerycontrolsettings,
		admin_images,
		admin_manageroles,
		admin_manageusers,
		admin_mediaobjects,
		admin_metadata,
		admin_mediaobjecttypes,
		admin_usersettings,
		admin_videoaudioother,
		/// <summary>
		/// Represents an album view
		/// </summary>
		album,
		albumtreeview,
		changepassword,
		createaccount,
		error_cannotwritetodirectory,
		error_generic,
		//error_unauthorized, // Removed from use 2009.01.22 (feature # 128)
		install,
		login,
		/// <summary>
		/// Represents the media object view
		/// </summary>
		mediaobject,
		myaccount,
		recoverpassword,
		task_addobjects,
		task_assignthumbnail,
		task_createalbum,
		task_deletealbum,
		task_deletehires,
		task_deleteobjects,
		task_downloadobjects,
		task_editcaptions,
		task_rearrange,
		task_rotateimage,
		task_rotateimages,
		task_synchronize,
		task_transferobject,
		search,
		upgrade
	}

	/// <summary>
	/// Specifies a particular message that is to be displayed to the user. The text of the message is extracted from the resource file.
	/// </summary>
	public enum Message
	{
		None = 0,
		ThumbnailSuccessfullyAssigned = 1,
		CannotAssignThumbnailNoObjectsExistInAlbum = 2,
		CannotEditCaptionsNoEditableObjectsExistInAlbum = 3,
		CannotRearrangeNoObjectsExistInAlbum = 4,
		CannotRotateNoRotatableObjectsExistInAlbum = 5,
		CannotMoveNoObjectsExistInAlbum = 6,
		CannotCopyNoObjectsExistInAlbum = 7,
		CannotDeleteOriginalFilesNoObjectsExistInAlbum = 8,
		CannotDeleteObjectsNoObjectsExistInAlbum = 9,
		//OneOrMoreCaptionsExceededMaxLength = 10,
		CaptionExceededMaxLength = 11,
		MediaObjectDoesNotExist = 12,
		AlbumDoesNotExist = 13,
		//NoScriptDefaultText = 14,
		//SynchronizationSuccessful = 15,
		//SynchronizationFailure = 16,
		ObjectsSuccessfullyDeleted = 17,
		OriginalFilesSuccessfullyDeleted = 18,
		UserNameOrPasswordIncorrect = 19,
		AlbumNameExceededMaxLength = 20,
		//AlbumSummaryExceededMaxLength = 21,
		//AlbumNameAndSummaryExceededMaxLength = 22,
		AlbumNotAuthorizedForUser = 23,
		NoAuthorizedAlbumForUser = 24,
		CannotOverlayWatermarkOnImage = 25,
		CannotRotateObjectNotRotatable = 26,
		ObjectsSuccessfullyMoved = 27,
		ObjectsSuccessfullyCopied = 28,
		ObjectsSuccessfullyRearranged = 29,
		ObjectsSuccessfullyRotated = 30,
		ObjectsSkippedDuringUpload = 31,
		CannotRotateInvalidImage = 32,
		CannotEditGalleryIsReadOnly = 33,
		CannotDownloadObjectsNoObjectsExistInAlbum = 34,
		GallerySuccessfullyChanged = 35,
		SettingsSuccessfullyChanged = 36
	}

	/// <summary>
	/// Specifies the style of message to be displayed to the user.
	/// </summary>
	public enum MessageStyle
	{
		None,
		Information,
		Warning,
		Error
	}

	#endregion
}
