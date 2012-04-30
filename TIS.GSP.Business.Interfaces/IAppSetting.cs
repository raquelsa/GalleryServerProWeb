using System;

namespace GalleryServerPro.Business.Interfaces
{
	/// <summary>
	/// Represents application level settings.
	/// </summary>
	public interface IAppSetting
	{
		/// <summary>
		/// Gets or sets the size of each block of bytes when transferring files to streams and vice versa. This property was originally
		/// created to specify the buffer size for downloading a media object to the client, but it is now used for all
		/// file/stream copy operations.
		/// </summary>
		int MediaObjectDownloadBufferSize { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether security-sensitive portions of the URL to the media object are encrypted when it is sent 
		/// to the client browser. When false, the URL to the media object is sent in plain text, such as 
		/// "handler/getmediaobject.ashx?moid=34&amp;dt=1&amp;g=1"
		/// These URLs can be seen by viewing the source of the HTML page. From this URL one can determine the album ID 
		/// for this media object is 8, (aid=8), the file path to the media object on the server is 
		/// C:\gs\mypics\birthday.jpeg, and the requested image is a thumbnail (dt=1, where 1 is the value of the 
		/// GalleryServerPro.Business.DisplayObjectType enumeration for a thumbnail). For enhanced security, this property should
		/// be true, which uses Triple DES encryption to encrypt the the query string.
		/// It is recommended to set this to true except when you are	troubleshooting and it is useful to see the 
		/// filename and path in the HTML source. The Triple DES algorithm uses the secret key specified in the 
		/// <see cref="EncryptionKey" /> property.
		/// </summary>
		bool EncryptMediaObjectUrlOnClient { get; set; }

		/// <summary>
		/// Gets or sets the secret key used for the Triple DES algorithm. Applicable when the property <see cref="EncryptMediaObjectUrlOnClient" /> = true.
		/// The string must be 24 characters in length and be sufficiently strong so that it cannot be easily cracked.
		/// An exception is thrown by the .NET Framework if the key is considered weak. Change this to a value known only
		/// to you to prevent others from being able to decrypt.
		/// </summary>
		string EncryptionKey { get; set; }

		/// <summary>
		/// Gets or sets the absolute or relative path to the jQuery script file as stored in the application settings table.
		/// A relative path must be relative to the root of the web application and start with a tilde ("~"). An absolute path must be a full URI
		/// (e.g. http://ajax.googleapis.com/ajax/libs/jquery/1/jquery.min.js, //ajax.googleapis.com/ajax/libs/jquery/1/jquery.min.js).
		/// It is not valid to specify a UNC path, mapped drive path, or path to the local file system (e.g. "C:\scripts\jquery.js").
		/// Specify an empty string to indicate to GSP that the containing application is responsible for adding the jQuery
		/// reference. In this case, GSP will not attempt to add a jQuery reference.  Guaranteed to not return null.
		/// </summary>
		/// <value>The absolute or relative path to the jQuery script file as stored in the application settings table.</value>
		/// <remarks>The path is returned exactly how it appears in the database.</remarks>
		string JQueryScriptPath { get; set; }

		/// <summary>
		/// Gets or sets the absolute or relative path to the jQuery UI script file as stored in the application settings table.
		/// A relative path must be relative to the root of the web application and start with a tilde ("~"). An absolute path must be a full URI
		/// (e.g. http://ajax.googleapis.com/ajax/libs/jqueryui/1.8.9/jquery-ui.min.js.
		/// It is not valid to specify a UNC path, mapped drive path, or path to the local file system (e.g. "C:\scripts\jquery.js").
		/// Specify an empty string to indicate to GSP that the containing application is responsible for adding the jQuery UI
		/// reference. In this case, GSP will not attempt to add a jQuery reference. Guaranteed to not return null.
		/// </summary>
		/// <value>The absolute or relative path to the jQuery UI script file as stored in the application settings table.</value>
		/// <remarks>The path is returned exactly how it appears in the database.</remarks>
		string JQueryUiScriptPath { get; set; }

		/// <summary>
		/// Gets or sets the name of the Membership provider for the gallery users. Optional. When not specified, the default provider specified
		/// in web.config is used.
		/// </summary>
		/// <remarks>The name of the Membership provider for the gallery users.</remarks>
		string MembershipProviderName { get; set; }

		/// <summary>
		/// Gets or sets the name of the Role provider for the gallery users. Optional. When not specified, the default provider specified
		/// in web.config is used.
		/// </summary>
		/// <remarks>The name of the Role provider for the gallery users.</remarks>
		string RoleProviderName { get; set; }

		/// <summary>
		/// Gets or sets the license for the current application.
		/// </summary>
		/// <value>The license for the current application.</value>
		ILicense License { get; set; }

		/// <summary>
		/// Gets or sets the product key for this installation of Gallery Server Pro.
		/// </summary>
		string ProductKey { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to store objects in a cache for quicker retrieval. This significantly improves
		/// performance, but cannot be used in web farms because the cache is local to each server and there is not a cross-server 
		/// mechanism to expire the cache.</summary>
		bool EnableCache { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether gallery administrators are allowed to create, edit, and delete users and roles.</summary>
		bool AllowGalleryAdminToManageUsersAndRoles { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether gallery administrators are allowed to see users and roles that do not have 
		/// access to current gallery.</summary>
		bool AllowGalleryAdminToViewAllUsersAndRoles { get; set; }

		/// <summary>
		/// Indicates the maximum number of error objects to persist to the data store. When the number of errors exceeds this
		/// value, the oldest item is purged to make room for the new item. A value of zero means no limit is enforced.
		/// </summary>
		int MaxNumberErrorItems { get; set; }

		/// <summary>
		/// Gets the physical application path of the currently running application. For web applications this will be equal to
		/// the Request.PhysicalApplicationPath property.
		/// </summary>
		string PhysicalApplicationPath { get; }

		/// <summary>
		/// Gets the trust level of the currently running application. 
		/// </summary>
		ApplicationTrustLevel AppTrustLevel { get; }

		/// <summary>
		/// Gets the name of the currently running application. Default is "Gallery Server Pro".
		/// </summary>
		string ApplicationName { get; }

		/// <summary>
		/// Gets or sets the path, relative to the current application, to the directory
		/// containing the Gallery Server Pro resources such as images, user controls,
		/// scripts, etc. When setting the property, the following scrubbing occurs: (a) leading
		/// or trailing slashes are removed, (b) forward slashes ('/') are replaced with path 
		/// separator characters (i.e. the backward slash '\'). The result should be equivalent to
		/// the value of the galleryResourcesPath setting in the galleryServerPro/core section 
		/// of web.config. Examples: "gs", "GalleryServerPro\resources"
		/// </summary>
		/// <value>A string.</value>
		string GalleryResourcesPath { get; }

		/// <summary>
		/// Gets the full physical path to the directory where files can be temporarily stored. Example:
		/// "C:\inetpub\wwwroot\galleryserverpro\App_Data\_Temp"
		/// </summary>
		string TempUploadDirectory { get; }

		/// <summary>
		/// Gets or sets the maintenance status. During each application restart a maintenance routine is run that helps
		/// ensure data integrity and eliminate unused data. This property describes the status of the maintenance routine.
		/// </summary>
		/// <value>The maintenance status.</value>
		MaintenanceStatus MaintenanceStatus { get; set; }

		///// <summary>
		///// Gets the date/time this application was installed. The timestamp of the oldest gallery's creation date is
		///// considered to be the application install date.
		///// </summary>
		///// <value>The date/time this application was installed.</value>
		//System.DateTime InstallDate { get; } 

		///// <summary>
		///// Gets a value indicating whether the Microsoft .NET Framework 3.0 or higher is installed on the current system.
		///// </summary>
		//bool IsDotNet3OrHigherInstalled { get; }

		/// <summary>
		/// Gets the .NET Framework version the current application is running under. Contains only the major and minor components.
		/// </summary>
		/// <value>The .NET Framework version the current application is running under.</value>
		/// <example>
		/// To verify the current application is running 3.0 or higher, use this:
		/// <code>
		/// if (AppSetting.Instance.DotNetFrameworkVersion > new Version("2.0"))
		/// { /* App is 3.0 or higher */ }
		/// </code>
		/// </example>
		Version DotNetFrameworkVersion { get; }

		/// <summary>
		/// Gets the IIS application pool identity.
		/// </summary>
		/// <value>The application app pool identity.</value>
		string IisAppPoolIdentity { get; }

		/// <summary>
		/// Gets the full file path to the FFmpeg utility. During application initialization the bin directory is inspected for the
		/// presence of ffmpeg.exe. If present, this property is assigned the value of the full path to the utility. If not present,
		/// the property is assigned <see cref="string.Empty" />. FFmpeg is used to extract thumbnails from videos and for video conversion.
		/// Example: C:\inetpub\wwwroot\gallery\bin\ffmpeg.exe
		/// </summary>
		/// <value>
		/// 	Returns the full file path to the FFmpeg utility, or <see cref="string.Empty" /> if the utility is not present.
		/// </value>
		string FFmpegPath { get; }

		/// <summary>
		/// Gets the full file path to the ImageMagick convert.exe utility. During application initialization the bin directory is inspected for the
		/// presence of convert.exe. If present, this property is assigned the value of the full path to the utility. If not present,
		/// the property is assigned <see cref="string.Empty" />. This utility is used to extract thumbnails from .eps and .pdf files.
		/// Example: C:\inetpub\wwwroot\gallery\bin\convert.exe
		/// </summary>
		/// <value>
		/// 	Returns the full file path to the ImageMagick convert.exe utility, or <see cref="string.Empty" /> if the utility is not present.
		/// </value>
		string ImageMagickConvertPath { get; }

		/// <summary>
		/// Gets or sets the version of the objects in the database as reported by the database. Ex: "2.4.1"
		/// </summary>
		/// <value>The version of the objects in the database as reported by the database.</value>
		string DataSchemaVersion { get; set; }

		/// <summary>
		/// Gets a value indicating whether the current library has been populated with data from the calling assembly.
		/// This library is initialized by calling <see cref="Initialize" />.
		/// </summary>
		bool IsInitialized { get; }

		/// <summary>
		/// Gets or sets a value indicating whether a sample album and media object is needed for this application. This property may be set
		/// during application initialization so that later in the code path, when the gallery ID is available, the objects can be
		/// created.
		/// </summary>
		/// <value><c>true</c> if a sample album and media object is needed for this application; otherwise, <c>false</c>.</value>
		bool SampleObjectsNeeded { get; set; }

		/// <summary>
		/// Assign various application-wide properties to be used during the lifetime of the application. This method
		/// should be called once when the application first starts.
		/// </summary>
		/// <param name="trustLevel">The trust level of the current application.</param>
		/// <param name="physicalAppPath">The physical path of the currently executing application. For web applications
		/// this will be equal to the Request.PhysicalApplicationPath property.</param>
		/// <param name="galleryResourcesPath">The path, relative to the current application, to 
		/// the directory containing the Gallery Server Pro resources such as images, user controls, 
		/// scripts, etc. Use the value of the galleryResourcesPath setting in the 
		/// galleryServerPro/core section of web.config. Examples: "gs", "GalleryServerPro\resources"</param>
		/// <param name="appName">The name of the currently running application.</param>
		/// <exception cref="System.InvalidOperationException">Thrown when this method is called more than once during
		/// the application's lifetime.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown if the trustLevel parameter has the value
		/// ApplicationTrustLevel.None.</exception>
		/// <exception cref="System.ArgumentNullException">Thrown if any parameters are null or empty.</exception>
		void Initialize(ApplicationTrustLevel trustLevel, string physicalAppPath, string appName, string galleryResourcesPath);

		/// <summary>
		/// Persist the specified application settings to the data store. Specify a null value for each parameter whose value is
		/// not changing.
		/// </summary>
		/// <param name="productKey">The product key for this installation of Gallery Server Pro.</param>
		/// <param name="mediaObjectDownloadBufferSize">The size of each block of bytes when transferring files to streams and vice versa.</param>
		/// <param name="encryptMediaObjectUrlOnClient">Indicates whether security-sensitive portions of the URL to the media object are
		/// encrypted when it is sent to the client browser.</param>
		/// <param name="encryptionKey">The secret key used for the Triple DES algorithm.</param>
		/// <param name="jQueryScriptPath">The absolute or relative path to the jQuery script file.</param>
		/// <param name="jQueryUiScriptPath">The absolute or relative path to the jQuery UI script file.</param>
		/// <param name="membershipProviderName">The name of the Membership provider for the gallery users.</param>
		/// <param name="roleProviderName">The name of the Role provider for the gallery users.</param>
		/// <param name="enableCache">Indicates whether to store objects in a cache for quicker retrieval.</param>
		/// <param name="allowGalleryAdminToManageUsersAndRoles">Indicates whether gallery administrators are allowed to create, edit, and delete
		/// users and roles.</param>
		/// <param name="allowGalleryAdminViewAllUsersAndRoles">Indicates whether gallery administrators are allowed to see users and roles that 
		/// do not have access to current gallery.</param>
		/// <param name="maxNumberErrorItems">The maximum number of error objects to persist to the data store.</param>
		void Save(string productKey, int? mediaObjectDownloadBufferSize, bool? encryptMediaObjectUrlOnClient, string encryptionKey,
							string jQueryScriptPath, string jQueryUiScriptPath, string membershipProviderName, string roleProviderName, bool? enableCache,
							bool? allowGalleryAdminToManageUsersAndRoles, bool? allowGalleryAdminViewAllUsersAndRoles, int? maxNumberErrorItems);
	}
}