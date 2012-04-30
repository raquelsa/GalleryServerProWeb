
namespace GalleryServerPro.Business
{
	/// <summary>
	/// Contains constants used throughout Gallery Server Pro.
	/// </summary>
	public static class GlobalConstants
	{
		/// <summary>
		/// The default name for a directory when a valid name cannot be generated from the album title. This occurs
		/// when a user enters an album title consisting entirely of characters that are invalid for a directory
		/// name, such as ?, *, :.
		/// </summary>
		public const string DefaultAlbumDirectoryName = "Album";
		
		/// <summary>
		/// Gets the name of the dictionary key that references the <see cref="Interfaces.IGalleryServerRoleCollection" /> item containing
		/// all roles for the current gallery in the cache item named <see cref="CacheItem.GalleryServerRoles" />. Note that other items 
		/// in the dictionary have keys identified by a concatenation of the user's session ID and username.
		/// </summary>
		public const string GalleryServerRoleAllRolesCacheKey = "AllRoles";

		/// <summary>
		/// Gets the string that is used for the beginning of every role name used for album ownership. The role name has
		/// this format: {RoleNamePrefix} - {AlbumOwnerUserName} - {AlbumTitle} (album {AlbumID}) For example:
		/// "Album Owner - rdmartin - rdmartin's album (album 193)" Current value: "Album Owner"
		/// </summary>
		public const string AlbumOwnerRoleNamePrefix = "Album Owner";

		/// <summary>
		/// Gets the name of the role that defines the permissions to use for album ownership roles.
		/// Current value: _Album Owner Template"
		/// </summary>
		public const string AlbumOwnerRoleTemplateName = "_Album Owner Template";

		/// <summary>
		/// Gets the name of the session variable that stores a List&lt;String&gt; of filenames that were skipped
		/// when the user added one or more files to Gallery Server on the Add objects page.
		/// </summary>
		public const string SkippedFilesDuringUploadSessionKey = "SkippedFiles";

		/// <summary>
		/// Gets the name of the thumbnail file that is created to represent an external media object.
		/// </summary>
		public const string ExternalMediaObjectFilename = "external";

		/// <summary>
		/// Gets the maximum number of skipped objects to display to the user after a synchronization. If the number is too high, 
		/// it can take a long time to transmit the data to the browser, or it it can exceed the maxJsonLength value set in web.config,
		/// which causes a "maximum length exceed" error.
		/// </summary>
		public const int MaxNumberOfSkippedObjectsToDisplayAfterSynch = 500;

		/// <summary>
		/// Gets the path, relative to the web application root, where files may be temporarily persisted. Ex: "App_Data\\_Temp"
		/// </summary>
		public const string TempUploadDirectory = "App_Data\\_Temp";

		/// <summary>
		/// Gets the path, relative to the web application root, of the application data directory. Ex: "App_Data"
		/// </summary>
		public const string AppDataDirectory = "App_Data";

		/// <summary>
		/// Gets the name of the file that may be created during installation. This file is intended to survive the application
		/// restart that may occur when web.config is modified. It contains data to be processed
		/// by application startup code to complete the installation. Ex: "_SqlMembership.txt"
		/// </summary>
		public const string InstallMembershipFileName = "_SqlMembership.txt";

		/// <summary>
		/// Gets the name of the file that, when present in the App_Data directory, causes the Upgrade Wizard to automatically run.
		/// Ex: "upgrade.txt"
		/// </summary>
		public const string UpgradeTriggerFileName = "upgrade.txt";

		/// <summary>
		/// Gets the name of the file that, when present in the App_Data directory, causes the Install Wizard to automatically run.
		/// Ex: "install.txt"
		/// </summary>
		public const string InstallTriggerFileName = "install.txt";

		/// <summary>
		/// Gets the name of the name of the product key for this version of Gallery Server Pro. This value is compared to the 
		/// value entered by the user.
		/// </summary>
		public const string ProductKey = "KOSHKONONG";

		/// <summary>
		/// Gets the name of the name of the product key for this version of Gallery Server Pro. This value is compared to the 
		/// value entered by the user. When this product key is used, no footer is displayed at the bottom of the page (except
		/// on admin pages, since they always display the footer).
		/// </summary>
		public const string ProductKeyNoPageFooter = "PRAIRIE_MOON";

		/// <summary>
		/// Gets the number of days Gallery Server Pro is fully functional before it requires a product key to be entered.
		/// Default value = 30.
		/// </summary>
		public const int TrialNumberOfDays = 30;

		/// <summary>
		/// Gets the format string used for storing dates/times in metadata. Note: If modifying this value, also update the
		/// equivalent constant defined in the WpfMetadataExtractor in the WPF project.
		/// </summary>
		public const string DateTimeFormatStringForMetadata = "ddd, dd MMM yyyy h:mm:ss tt";
	}
}
