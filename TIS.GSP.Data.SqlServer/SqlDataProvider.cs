using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;

namespace GalleryServerPro.Data.SqlServer
{
	/// <summary>
	/// Provides functionality for retrieving and persisting information to the SQL Server data store.
	/// </summary>
	public class SqlDataProvider : Provider.DataProvider
	{
		#region Private Fields

		// This variable should reference the current version of the database schema required by this provider.
		// During InitializeDataStore, this value is compared against the schema version stored in the database
		// The database is upgraded if appropriate.
		private const string ExceptionSqlId = "SQL";

		private static readonly object _sharedLock = new object();

		private static string _applicationName;
		private static string _connectionStringName;
		private static string _connectionString;

		#endregion

		#region Enum Declarations

		/// <summary>
		/// References the name of a SQL upgrade script embedded in this assembly that can be used to upgrade the 
		/// database schema.
		/// </summary>
		private enum GalleryDataSchemaUpgradeScript
		{
			/// <summary>
			/// Gets the script file that upgrades the database from 2.4.1 to 2.4.3
			/// </summary>
			SqlUpgrade_2_4_1_to_2_4_3 = 0,
			/// <summary>
			/// Gets the script file that upgrades the database from 2.4.3 to 2.4.4
			/// </summary>
			SqlUpgrade_2_4_3_to_2_4_4 = 1,
			/// <summary>
			/// Gets the script file that upgrades the database from 2.4.4 to 2.4.5
			/// </summary>
			SqlUpgrade_2_4_4_to_2_4_5 = 2,
			/// <summary>
			/// Gets the script file that upgrades the database from 2.4.5 to 2.4.6
			/// </summary>
			SqlUpgrade_2_4_5_to_2_4_6 = 3,
			/// <summary>
			/// Gets the script file that upgrades the database from 2.4.6 to 2.5.0
			/// </summary>
			SqlUpgrade_2_4_6_to_2_5_0 = 4,
			/// <summary>
			/// Gets the script file that upgrades the database from 2.5.0 to 2.6.0
			/// </summary>
			SqlUpgrade_2_5_0_to_2_6_0 = 5
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets the friendly name used to refer to the provider during configuration.
		/// </summary>
		/// <value>The friendly name used to refer to the provider during configuration.</value>
		/// <returns>The friendly name used to refer to the provider during configuration.</returns>
		public override string Name
		{
			get { return ((System.Configuration.Provider.ProviderBase)this).Name; }
		}

		/// <summary>
		/// Gets the data storage technology of this provider.
		/// </summary>
		/// <value>The data storage technology of this provider.</value>
		public override ProviderDataStore DataStore
		{
			get
			{
				return ProviderDataStore.SqlServer;
			}
		}

		/// <summary>
		/// Gets a brief, friendly description suitable for display in administrative tools or other user interfaces (UIs).
		/// </summary>
		/// <value>A brief, friendly description suitable for display in administrative tools or other UIs.</value>
		/// <returns>A brief, friendly description suitable for display in administrative tools or other UIs.</returns>
		public override string Description
		{
			get { return ((System.Configuration.Provider.ProviderBase)this).Description; }
		}

		/// <summary>
		/// Gets the name of the connection string.
		/// </summary>
		/// <value>The name of the connection string.</value>
		public override string ConnectionStringName
		{
			get
			{
				return _connectionStringName;
			}
		}

		/// <summary>
		/// Gets or sets the name of the application to store and retrieve Gallery Server data for.
		/// </summary>
		/// <value>
		/// The name of the application to store and retrieve Gallery Server data for.
		/// </value>
		public override string ApplicationName
		{
			get
			{
				return _applicationName;
			}
			set
			{
				_applicationName = value;
			}
		}

		#endregion

		#region Internal methods

		/// <summary>
		/// Get a reference to an unopened database connection.
		/// </summary>
		/// <returns>A SqlConnection object.</returns>
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		internal static SqlConnection GetDbConnection()
		{
			return new SqlConnection(_connectionString);
		}

		#endregion

		#region Data Provider Methods

		/// <summary>
		/// Initializes the provider.
		/// </summary>
		/// <param name="name">The friendly name of the provider.</param>
		/// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// The name of the provider is null.
		/// </exception>
		/// <exception cref="T:System.ArgumentException">
		/// The name of the provider has a length of zero.
		/// </exception>
		/// <exception cref="T:System.InvalidOperationException">
		/// An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"/> on a provider after the provider has already been initialized.
		/// </exception>
		public override void Initialize(string name, NameValueCollection config)
		{
			// Initialize values from web.config.
			if (config == null)
				throw new ArgumentNullException("config");

			if (String.IsNullOrEmpty(name))
				name = "SqlServerGalleryServerProProvider";

			if (String.IsNullOrEmpty(config["description"]))
			{
				config.Remove("description");
				config.Add("description", "SQL Server gallery data provider");
			}

			// Initialize the abstract base class.
			base.Initialize(name, config);

			if (String.IsNullOrWhiteSpace(config["applicationName"]))
			{
				_applicationName = String.Empty; // If we had a reference to System.Web, we could use HostingEnvironment.ApplicationVirtualPath
			}
			else
			{
				_applicationName = config["applicationName"];
			}

			// Get connection string.
			ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings[config["connectionStringName"]];

			if (connectionStringSettings == null || String.IsNullOrWhiteSpace(connectionStringSettings.ConnectionString))
			{
				throw new System.Configuration.Provider.ProviderException("Connection string cannot be blank.");
			}

			_connectionStringName = connectionStringSettings.Name;
			_connectionString = connectionStringSettings.ConnectionString;

			// Get the SQL Server schema and object qualifier. Note that the object qualifier is typically specified only in the DotNetNuke version;
			// in most cases (even in DotNetNuke) it will be an empty string.
			Util.SqlServerSchema = ConfigurationManager.AppSettings["SqlServerSchema"] ?? "dbo.";
			Util.ObjectQualifier = ConfigurationManager.AppSettings["SqlServerObjectQualifier"] ?? String.Empty;
		}

		#endregion

		#region Gallery Methods

		/// <summary>
		/// Fill the <paramref name="emptyCollection"/> with all the galleries in the current application. The return value is the same reference
		/// as the parameter. The template gallery is not included (that is, the one where the gallery ID = <see cref="Int32.MinValue"/>.
		/// </summary>
		/// <param name="emptyCollection">An empty <see cref="IGalleryCollection"/> object to populate with the list of galleries in the current
		/// application. This parameter is required because the library that implements this interface does not have
		/// the ability to directly instantiate any object that implements <see cref="IGalleryCollection"/>.</param>
		/// <returns>
		/// Returns an <see cref="IGalleryCollection"/> representing the galleries in the current application. The returned object is the
		/// same object in memory as the <paramref name="emptyCollection"/> parameter.
		/// </returns>
		public override IGalleryCollection Gallery_GetGalleries(IGalleryCollection emptyCollection)
		{
			return GalleryData.GetGalleries(emptyCollection);
		}

		/// <summary>
		/// Persist the specified gallery to the data store. Return the ID of the gallery.
		/// </summary>
		/// <param name="gallery">An instance of <see cref="IGallery"/> to persist to the data store.</param>
		/// <returns>
		/// Return the ID of the gallery. If this is a new gallery and a new ID has been
		/// assigned, then this value has also been assigned to the <see cref="IGallery.GalleryId"/> property.
		/// </returns>
		public override int Gallery_Save(IGallery gallery)
		{
			return GalleryData.SaveGallery(gallery);
		}

		/// <summary>
		/// Permanently delete the specified gallery from the data store, including all related records. This action cannot
		/// be undone.
		/// </summary>
		/// <param name="gallery">The <see cref="IGallery"/> to delete from the data store.</param>
		public override void Gallery_Delete(IGallery gallery)
		{
			GalleryData.DeleteGallery(gallery);
		}

		/// <summary>
		/// Configure the specified <paramref name="gallery"/> by verifying that a default set of
		/// records exist in the supporting tables (gs_Album, gs_GallerySetting, gs_MimeTypeGallery, gs_Synchronize, gs_Role_Album).
		/// No changes are made to the file system as part of this operation. This method does not overwrite existing data, but it
		/// does insert missing data. This function can be used during application initialization to validate the data integrity for
		/// a gallery. For example, if the user has added a record to the MIME types or template gallery settings tables, this method
		/// will ensure that the new records are associated with the gallery identified in <paramref name="gallery"/>.
		/// </summary>
		/// <param name="gallery">The gallery to configure.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="gallery" /> is null.</exception>
		public override void Gallery_Configure(IGallery gallery)
		{
			if (gallery == null)
				throw new ArgumentNullException("gallery");

			GalleryData.ConfigureGallery(gallery.GalleryId);
		}

		/// <summary>
		/// Return a collection representing all the gallery settings in the data store.
		/// If no records are found in the data store, an empty collection is returned.
		/// </summary>
		/// <returns>
		/// Returns a collection containing all the gallery settings in the data store.
		/// </returns>
		public override IEnumerable<GallerySettingDto> GallerySetting_GetGallerySettings()
		{
			return GalleryData.GetGallerySettings();
		}

		/// <summary>
		/// Persist the current gallery settings to the data store.
		/// </summary>
		/// <param name="gallerySettings">An instance of <see cref="IGallerySettings"/> to persist to the data store.</param>
		public override void GallerySetting_Save(IGallerySettings gallerySettings)
		{
			GalleryData.SaveGallerySetting(gallerySettings);
		}

		/// <summary>
		/// Return a collection representing all the gallery control settings in the data store.
		/// If no records are found in the data store, an empty collection is returned.
		/// </summary>
		/// <returns>
		/// Returns a collection containing all the gallery control settings in the data store.
		/// </returns>
		public override IEnumerable<GalleryControlSettingDto> GalleryControlSetting_GetGalleryControlSettings()
		{
			return GalleryData.GetDataReaderGalleryControlSettings();
		}

		/// <summary>
		/// Persist the current gallery control settings to the data store.
		/// </summary>
		/// <param name="galleryControlSettings">An instance of <see cref="IGalleryControlSettings"/> to persist to the data store.</param>
		public override void GalleryControlSetting_Save(IGalleryControlSettings galleryControlSettings)
		{
			GalleryData.SaveGalleryControlSettings(galleryControlSettings);
		}

		#endregion

		#region Album methods

		/// <summary>
		/// Persist the specified album to the data store. Return the ID of the album.
		/// </summary>
		/// <param name="album">An instance of <see cref="IAlbum"/> to persist to the data store.</param>
		/// <returns>
		/// Return the ID of the album. If this is a new album and a new ID has been
		/// assigned, then this value has also been assigned to the ID property of the object.
		/// </returns>
		public override int Album_Save(IAlbum album)
		{
			return Album.Save(album);
		}

		/// <summary>
		/// Return a collection of album IDs that are immediate children of the album represented by <paramref name="albumId"/>.
		/// If no matching objects are found in the data store, an empty collection is returned.
		/// </summary>
		/// <param name="albumId">The ID that uniquely identifies the album for which to return the child albums
		/// contained within.</param>
		/// <returns>
		/// Returns a collection of all album IDs directly within the album represented by <paramref name="albumId"/>.
		/// </returns>
		public override IEnumerable<int> Album_GetChildAlbumIdsById(int albumId)
		{
			return Album.GetDataReaderChildAlbumsById(albumId);
		}

		/// <summary>
		/// Return a collection representing the child media objects contained within the album specified by
		/// <paramref name="albumId"/> parameter, optionally including the metadata for each item. If no matching 
		/// objects are found in the data store, an empty collection is returned.
		/// </summary>
		/// <param name="albumId">The ID that uniquely identifies the desired album.</param>
		/// <param name="includeMetadata">When <c>true</c> include metadata items on the 
		/// <see cref="MediaObjectDto.MediaObjectMetadata" /> property. When <c>false</c>, the property is null.</param>
		/// <returns>
		/// Returns a collection of all media objects directly within the album represented by <paramref name="albumId"/>.
		/// </returns>
		public override IEnumerable<MediaObjectDto> Album_GetChildMediaObjectsById(int albumId, bool includeMetadata)
		{
			return Album.GetChildGalleryObjectsById(albumId, includeMetadata);
		}

		/// <summary>
		/// Return the album for the specified <paramref name="albumId"/>. Returns null if no matching object
		/// is found in the data store.
		/// </summary>
		/// <param name="albumId">The ID that uniquely identifies the desired album.</param>
		/// <returns>
		/// Returns an instance of <see cref="AlbumDto"/>, or null if no matching object is found.
		/// </returns>
		public override AlbumDto Album_GetAlbumById(int albumId)
		{
			return Album.GetAlbumById(albumId);
		}

		/// <summary>
		/// Permanently delete the specified album from the data store, including any
		/// child albums and media objects (cascading delete). This action cannot be undone.
		/// </summary>
		/// <param name="album">The <see cref="IAlbum"/> to delete from the data store.</param>
		public override void Album_Delete(IAlbum album)
		{
			Album.Delete(album);
		}

		#endregion

		#region Media Object methods

		/// <summary>
		/// Persist the specified media object to the data store. Return the ID of the media object.
		/// </summary>
		/// <param name="mediaObject">An instance of <see cref="IGalleryObject"/> to persist to the data store.</param>
		/// <returns>
		/// Return the ID of the media object. If this is a new media object and a new ID has been
		/// assigned, then this value has also been assigned to the ID property of the object.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="mediaObject" /> is null.</exception>
		public override int MediaObject_Save(IGalleryObject mediaObject)
		{
			if (mediaObject == null)
				throw new ArgumentNullException("mediaObject");

			return MediaObject.Save(mediaObject);
		}

		/// <summary>
		/// Return the media object for the specified <paramref name="mediaObjectId" />, optionally including the metadata 
		/// for each item. Returns null if no matching object is found in the data store.
		/// </summary>
		/// <param name="mediaObjectId">The ID that uniquely identifies the desired media object.</param>
		/// <param name="includeMetadata">When <c>true</c> include metadata items on the 
		/// <see cref="MediaObjectDto.MediaObjectMetadata" /> property. When <c>false</c>, the property is null.</param>
		/// <returns>Returns an instance of <see cref="MediaObjectDto" />, or null if no matching object is found.</returns>
		public override MediaObjectDto MediaObject_GetMediaObjectById(int mediaObjectId, bool includeMetadata)
		{
			return MediaObject.GetMediaObjectById(mediaObjectId, includeMetadata);
		}

		/// <summary>
		/// Return a collection representing the metadata items for the specified <paramref name="mediaObjectId"/>. If no matching object
		/// is found in the data store, an empty collection is returned.
		/// </summary>
		/// <param name="mediaObjectId">The ID that uniquely identifies the desired media object.</param>
		/// <returns>
		/// Returns a collection of all metadata items.
		/// </returns>
		public override IEnumerable<MediaObjectMetadataDto> MediaObject_GetMetadataItemsByMediaObjectId(int mediaObjectId)
		{
			return MediaObject.GetMetadataItemsByMediaObjectId(mediaObjectId);
		}

		/// <summary>
		/// Permanently delete the specified media object from the data store. This action cannot
		/// be undone.
		/// </summary>
		/// <param name="mediaObject">The <see cref="IGalleryObject"/> to delete from the data store.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="mediaObject" /> is null.</exception>
		public override void MediaObject_Delete(IGalleryObject mediaObject)
		{
			if (mediaObject == null)
				throw new ArgumentNullException("mediaObject");

			MediaObject.Delete(mediaObject);
		}

		/// <summary>
		/// Return a collection representing the hash keys for all media objects in the data store.
		/// </summary>
		/// <returns>
		/// Returns a collection object with one field named "HashKey" containing the hash keys
		/// for all media objects in the data store.
		/// </returns>
		public override StringCollection MediaObject_GetAllHashKeys()
		{
			return MediaObject.GetHashKeys();
		}

		/// <summary>
		/// Return a collection representing media objects currently being processed. If no objects are found
		/// in the data store, an empty collection is returned.
		/// </summary>
		/// <returns>Returns a collection object.</returns>
		public override IEnumerable<MediaQueueDto> MediaQueue_GetMediaQueues()
		{
			return MediaQueue.GetAll();
		}

		/// <summary>
		/// Persist the specified media queue item to the data store. The ID of the new item is assigned to
		/// <see cref="MediaQueueDto.MediaQueueId"/>.
		/// </summary>
		/// <param name="mediaQueue">The media queue item to persist to the data store.</param>
		public override void MediaQueue_Save(MediaQueueDto mediaQueue)
		{
			MediaQueue.Save(mediaQueue);
		}

		/// <summary>
		/// Delete the media queue item from the data store.
		/// </summary>
		/// <param name="mediaQueue">The media queue item to delete from the data store.</param>
		public override void MediaQueue_Delete(MediaQueueDto mediaQueue)
		{
			MediaQueue.Delete(mediaQueue);
		}

		#endregion

		#region Synchronize methods

		/// <summary>
		/// Persist the synchronization information to the data store.
		/// </summary>
		/// <param name="synchStatus">An <see cref="ISynchronizationStatus"/> object containing the synchronization information
		/// to persist to the data store.</param>
		/// <exception cref="GalleryServerPro.ErrorHandler.CustomExceptions.SynchronizationInProgressException">Thrown when the data
		/// store indicates another synchronization is already in progress for this gallery.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="synchStatus" /> is null.</exception>
		public override void Synchronize_SaveStatus(ISynchronizationStatus synchStatus)
		{
			if (synchStatus == null)
				throw new ArgumentNullException("synchStatus");

			Synchronize.SaveStatus(synchStatus);
		}

		/// <summary>
		/// Retrieve the most recent synchronization information from the data store.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <param name="factory">An instance of <see cref="IFactory"/>. It is used to instantiate a <see cref="ISynchronizationStatus"/> object.</param>
		/// <returns>
		/// Returns an <see cref="ISynchronizationStatus"/> object with the most recent synchronization information from the data store.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="factory" /> is null.</exception>
		public override ISynchronizationStatus Synchronize_RetrieveStatus(int galleryId, IFactory factory)
		{
			if (factory == null)
				throw new ArgumentNullException("factory");

			return Synchronize.RetrieveStatus(galleryId, factory);
		}

		#endregion

		#region General methods

		/// <summary>
		/// Perform any needed data store operations to get Gallery Server ready to go. This includes upgrading the
		/// database to the version required by this provider.
		/// </summary>
		public override void InitializeDataStore()
		{
			lock (_sharedLock)
			{
				VerifySchemaVersion();
			}

			Application.Initialize();
		}

		/// <summary>
		/// Return a collection representing the application settings in the data store.
		/// If no records are found in the data store, an empty collection is returned.
		/// </summary>
		/// <returns>
		/// Returns a collection containing the application settings in the data store.
		/// </returns>
		public override IEnumerable<AppSettingDto> AppSetting_GetAppSettings()
		{
			return GalleryData.GetAppSettings();
		}

		/// <summary>
		/// Persist the current application settings to the data store.
		/// </summary>
		/// <param name="appSetting">An instance of <see cref="IAppSetting"/> to persist to the data store.</param>
		public override void AppSetting_Save(IAppSetting appSetting)
		{
			GalleryData.SaveAppSetting(appSetting);
		}

		/// <summary>
		/// Return gallery objects that match the specified search string. A gallery object is considered a match when
		/// all search terms are found in the relevant fields.
		/// For albums, the title and summary fields are searched. For media objects, the title, original filename,
		/// and metadata are searched. The contents of documents are not searched (e.g. the text of a Word or PDF file).
		/// If no matches are found, <paramref name="matchingAlbumIds"/> and <paramref name="matchingMediaObjectIds"/>
		/// will be empty, not null collections.
		/// </summary>
		/// <param name="galleryId">The ID for the gallery containing the objects to search.</param>
		/// <param name="searchTerms">A string array of search terms. Specify a single word for each item of the array, or
		/// combine words in an element to force a phase match. Items with more than one word indicate an exact
		/// phrase match is required. Example: There are three items where item 1="cat", item 2="0 step", and item 3="Mom".
		/// This method will match all gallery objects that contain the strings "cat", "0 step", and "Mom". It will also
		/// match partial words, such as Mom on steps at cathedral</param>
		/// <param name="matchingAlbumIds">The album IDs for all albums that match the search terms.</param>
		/// <param name="matchingMediaObjectIds">The media object IDs for all media objects that match the search terms.</param>
		/// <example>
		/// 	<para>Example 1</para>
		/// 	<para>The search terms are three elements: "cat", "step", and "Mom". All gallery objects that contain all
		/// three strings will be returned, such as an image with the caption "Mom and cat sitting on steps" (Notice the
		/// successful partial match between step and steps. However, the inverse is not true - searching for "steps"
		/// will not match "step".) Also matched would be an image with a caption "Mom at cathedral" and the exposure
		/// compensation metadata is "0 step".</para>
		/// 	<para>Example 2</para>
		/// 	<para>The search terms are two elements: "at the beach" and "Joey". All gallery objects that contain the
		/// phrase "at the beach" and "Joey" will be returned, such as a video with the caption "Joey at the beach with Mary".
		/// An image with the caption "Joey on the beach at Mary's house" will not match because the phrase "at the beach"
		/// is not present.
		/// </para>
		/// </example>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="searchTerms" /> is null.</exception>
		public override void SearchGallery(int galleryId, string[] searchTerms, out List<int> matchingAlbumIds, out List<int> matchingMediaObjectIds)
		{
			if (searchTerms == null)
				throw new ArgumentNullException("searchTerms");
			
			Application.SearchGallery(galleryId, searchTerms, out matchingAlbumIds, out matchingMediaObjectIds);
		}

		/// <summary>
		/// Begins a new database transaction. All subsequent database actions occur within the context of this transaction.
		/// Use <see cref="CommitTransaction"/> to commit this transaction or <see cref="RollbackTransaction"/> to abort it. If a transaction
		/// is already in progress, then this method returns without any action, which preserves the original transaction.
		/// Note: This function is not implemented.
		/// </summary>
		public override void BeginTransaction()
		{
		}

		/// <summary>
		/// Commits the current transaction, if one exists. A transaction is created with the <see cref="BeginTransaction"/> method.
		/// If there is not an existing transaction, no action is taken. If this method is called when a datareader is open, the
		/// actual commit is delayed until all datareaders are disposed. Note: This function is not implemented.
		/// </summary>
		public override void CommitTransaction()
		{
		}

		/// <summary>
		/// Aborts the current transaction, if one exists. A transaction is created with the <see cref="BeginTransaction"/> method.
		/// If there is not an existing transaction, no action is taken. Note: This function is not implemented.
		/// </summary>
		public override void RollbackTransaction()
		{
		}

		/// <summary>
		/// Imports the Gallery Server Pro data into the current database, overwriting any existing data. Does not import the actual media
		/// files; they must be imported manually with a utility such as Windows Explorer. This method makes changes only to the database tables;
		/// no files in the media objects directory are affected. If both the <paramref name="importMembershipData"/> and
		/// <paramref name="importGalleryData"/> parameters are false, then no action is taken.
		/// </summary>
		/// <param name="galleryData">An XML-formatted string containing the gallery data. The data must conform to the schema defined in the
		/// project for the data provider's implementation.</param>
		/// <param name="importMembershipData">If set to <c>true</c>, user accounts and other membership data will be imported.
		/// Current membership data will first be deleted.</param>
		/// <param name="importGalleryData">If set to <c>true</c>, albums, media objects, and other gallery data will be imported.
		/// Current gallery data will first be deleted.</param>
		public override void ImportGalleryData(string galleryData, bool importMembershipData, bool importGalleryData)
		{
			DataUtility.ImportData(galleryData, importMembershipData, importGalleryData);
		}

		/// <summary>
		/// Exports the Gallery Server Pro data in the current database to an XML-formatted string. Does not export the actual media files;
		/// they must be copied manually with a utility such as Windows Explorer. This method does not make any changes to the database tables
		/// or the files in the media objects directory.
		/// </summary>
		/// <param name="exportMembershipData">If set to <c>true</c>, user accounts and other membership data will be exported.</param>
		/// <param name="exportGalleryData">If set to <c>true</c>, albums, media objects, and other gallery data will be exported.</param>
		/// <returns>
		/// Returns an XML-formatted string containing the gallery data.
		/// </returns>
		public override string ExportGalleryData(bool exportMembershipData, bool exportGalleryData)
		{
			return DataUtility.ExportData(exportMembershipData, exportGalleryData);
		}

		/// <summary>
		/// Validates that the backup file specified in the <see cref="IBackupFile.FilePath"/> property of the <paramref name="backupFile"/>
		/// parameter is valid and populates the remaining properties with information about the file.
		/// </summary>
		/// <param name="backupFile">An instance of <see cref="IBackupFile"/> that with only the <see cref="IBackupFile.FilePath"/>
		/// property assigned. The remaining properties should be uninitialized since they will be assigned in this method.</param>
		public override void ValidateBackupFile(ref IBackupFile backupFile)
		{
			Util.ValidateBackupFile(ref backupFile);
		}

		/// <summary>
		/// Reclaims wasted space in the database and recalculates identity column values. Applies only to SQL CE.
		/// </summary>
		/// <exception cref="NotImplementedException">Not implemented in the SQL Server data provider.</exception>
		public override void Compact()
		{
			throw new NotImplementedException("Not implemented in the SQL Server data provider.");
		}

		/// <summary>
		/// Recalculates the checksums for each page in the database and compares the new checksums to the expected values. Also verifies
		/// that each index entry exists in the table and that each table entry exists in the index. Applies only to SQL CE.
		/// </summary>
		/// <returns>
		/// 	<c>True</c> if there is no database corruption; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="NotImplementedException">Not implemented in the SQL Server data provider.</exception>
		public override bool Verify()
		{
			throw new NotImplementedException("Not implemented in the SQL Server data provider.");
		}

		/// <summary>
		/// Repairs a corrupted database. Call this method when <see cref="Verify"/> returns false. Applies only to SQL CE.
		/// </summary>
		/// <exception cref="NotImplementedException">Not implemented in the SQL Server data provider.</exception>
		public override void Repair()
		{
			throw new NotImplementedException("Not implemented in the SQL Server data provider.");
		}

		#endregion

		#region Security Methods

		/// <summary>
		/// Return a collection representing the roles for all galleries. If no matching objects
		/// are found in the data store, an empty collection is returned.
		/// </summary>
		/// <returns>
		/// Returns a collection object representing the roles for all galleries.
		/// </returns>
		public override IEnumerable<RoleDto> Roles_GetRoles()
		{
			return Role.GetRoles();
		}

		/// <summary>
		/// Persist this gallery server role to the data store. The list of top-level albums this role applies to, which is stored
		/// in the <see cref="IGalleryServerRole.RootAlbumIds"/> property, must also be saved. The data provider automatically
		/// repopulates the <see cref="IGalleryServerRole.AllAlbumIds"/> and <see cref="IGalleryServerRole.Galleries"/> properties.
		/// </summary>
		/// <param name="role">An instance of IGalleryServerRole to persist to the data store.</param>
		public override void Role_Save(IGalleryServerRole role)
		{
			Role.Save(role);
		}

		/// <summary>
		/// Permanently delete this gallery server role from the data store, including the list of role/album relationships
		/// associated with this role. This action cannot be undone.
		/// </summary>
		/// <param name="role">An instance of <see cref="IGalleryServerRole"/> to delete from the data store.</param>
		public override void Role_Delete(IGalleryServerRole role)
		{
			Role.Delete(role);
		}

		#endregion

		#region MIME Type Methods

		/// <summary>
		/// Return a collection representing the MIME types. If no objects are found
		/// in the data store, an empty collection is returned.
		/// </summary>
		/// <returns>
		/// Returns a collection object representing the MIME types.
		/// </returns>
		public override IEnumerable<MimeTypeDto> MimeType_GetMimeTypes()
		{
			return MimeType.GetMimeTypes();
		}

		/// <summary>
		/// Return a collection representing the gallery-specific settings for MIME types. If no objects are found
		/// in the data store, an empty collection is returned.
		/// </summary>
		/// <returns>
		/// Returns a collection object representing the gallery-specific settings for MIME types.
		/// </returns>
		public override IEnumerable<MimeTypeGalleryDto> MimeType_GetMimeTypeGalleries()
		{
			return MimeType.GetMimeTypeGalleries();
		}

		/// <summary>
		/// Persist the gallery-specific properties of the <paramref name="mimeType"/> to the data store. Currently, only the
		/// <see cref="IMimeType.AllowAddToGallery"/> property is unique to the gallery identified in <see cref="IMimeType.GalleryId"/>;
		/// the other properties are application-wide and at present there is no API to modify them. In other words, this method saves whether a
		/// particular MIME type is enabled or disabled for a particular gallery.
		/// </summary>
		/// <param name="mimeType">The MIME type instance to save.</param>
		/// <exception cref="ArgumentException">Thrown when the <see cref="IMimeType.MimeTypeGalleryId"/> property of the <paramref name="mimeType" />
		/// parameter is not set to a valid value.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="mimeType" /> is null.</exception>
		public override void MimeType_Save(IMimeType mimeType)
		{
			if (mimeType == null)
				throw new ArgumentNullException("mimeType");

			MimeType.Save(mimeType);
		}

		/// <summary>
		/// Fill the <paramref name="emptyCollection"/> with all the browser templates in the current application. The return value is the same reference
		/// as the parameter.
		/// </summary>
		/// <param name="emptyCollection">An empty <see cref="IBrowserTemplateCollection"/> object to populate with the list of browser templates in the current
		/// application. This parameter is required because the library that implements this interface does not have
		/// the ability to directly instantiate any object that implements <see cref="IBrowserTemplateCollection"/>.</param>
		/// <returns>
		/// Returns an <see cref="IBrowserTemplateCollection"/> representing the browser templates in the current application. The returned object is the
		/// same object in memory as the <paramref name="emptyCollection"/> parameter.
		/// </returns>
		public override IBrowserTemplateCollection MimeType_GetBrowserTemplates(IBrowserTemplateCollection emptyCollection)
		{
			return MimeType.GetBrowserTemplates(emptyCollection);
		}

		#endregion

		#region App Error methods

		/// <summary>
		/// Return a collection representing the application errors. If no objects are found
		/// in the data store, an empty collection is returned.
		/// </summary>
		/// <returns>
		/// Returns a collection object with all application error fields.
		/// </returns>
		public override IEnumerable<AppErrorDto> AppError_GetAppErrors()
		{
			return Error.GetAppErrors();
		}

		/// <summary>
		/// Persist the specified application error to the data store. Return the ID of the error.
		/// </summary>
		/// <param name="appError">The application error to persist to the data store.</param>
		/// <returns>
		/// Return the ID of the error. If this is a new error object and a new ID has been
		/// assigned, then this value has also been assigned to the ID property of the object.
		/// </returns>
		public override int AppError_Save(IAppError appError)
		{
			return Error.Save(appError);
		}

		/// <summary>
		/// Delete the application error from the data store.
		/// </summary>
		/// <param name="appErrorId">The value that uniquely identifies this application error (<see cref="IAppError.AppErrorId"/>).</param>
		public override void AppError_Delete(int appErrorId)
		{
			Error.Delete(appErrorId);
		}

		/// <summary>
		/// Permanently delete all errors from the data store that are system-wide (that is, not associated with a specific gallery) and also
		/// those errors belonging to the specified <paramref name="galleryId"/>.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		public override void AppError_ClearLog(int galleryId)
		{
			Error.DeleteAll(galleryId);
		}

		#endregion

		#region Profile Methods

		/// <summary>
		/// Gets the profile for the specified user. Guaranteed to not return null.
		/// </summary>
		/// <param name="userName">The user name that uniquely identifies the user.</param>
		/// <param name="factory">An instance of <see cref="IFactory"/>. It is used to instantiate the necessary object(s).</param>
		/// <returns>
		/// Returns an <see cref="IUserProfile"/> object containing the profile for the user.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="factory" /> is null.</exception>
		public override IUserProfile Profile_GetUserProfile(string userName, IFactory factory)
		{
			if (factory == null)
				throw new ArgumentNullException("factory");

			return ProfileData.GetUserProfile(userName, factory);
		}

		/// <summary>
		/// Persist the specified <paramref name="profile"/> to the data store.
		/// </summary>
		/// <param name="profile">The profile to persist to the data store.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="profile" /> is null.</exception>
		public override void Profile_Save(IUserProfile profile)
		{
			if (profile == null)
				throw new ArgumentNullException("profile");

			ProfileData.Save(profile);
		}

		/// <summary>
		/// Permanently delete the profile records for the specified <paramref name="userName"/>.
		/// </summary>
		/// <param name="userName">The user name that uniquely identifies the user.</param>
		public override void Profile_DeleteProfileForUser(string userName)
		{
			ProfileData.DeleteProfileForUser(userName);
		}

		/// <summary>
		/// Permanently delete the profile records associated with the specified <paramref name="galleryId"/>.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		public override void Profile_DeleteProfilesForGallery(int galleryId)
		{
			ProfileData.DeleteProfilesForGallery(galleryId);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Check the current version of the database schema, upgrading if necessary. This function is useful when the administrator
		/// upgrades Gallery Server Pro to a newer version which requires a database upgrade. This is the function that executes the
		/// necessary SQL script to upgrade the database. If the version required by this provider does not match the database version, 
		/// and the database cannot be upgraded to the desired version, this function logs a message to the error log and returns
		/// without taking any action.
		/// </summary>
		private static void VerifySchemaVersion()
		{
			const GalleryDataSchemaVersion requiredDataSchemaVersion = Util.RequiredDatabaseSchemaVersion;
			GalleryDataSchemaVersion dataSchemaVersionOfDb = Util.GetDataSchemaVersion();

			if (requiredDataSchemaVersion == dataSchemaVersionOfDb)
				return;

			if (dataSchemaVersionOfDb == GalleryDataSchemaVersion.Unknown)
			{
				string msg = String.Format(CultureInfo.CurrentCulture, "The database structure has a version ({0}) that is not one of the recognized schema versions included in a Gallery Server Pro release. Because of this, Gallery Server Pro cannot determine whether or how to upgrade the data schema, so it will not make an attempt. This is an information message only and does not necessarily represent a problem. This version of Gallery Server Pro is designed to work with data schema version {1}.", Util.GetDataSchemaVersionString(), GalleryDataSchemaVersionEnumHelper.ConvertGalleryDataSchemaVersionToString(requiredDataSchemaVersion));

				ErrorHandler.CustomExceptions.DataException ex = new ErrorHandler.CustomExceptions.DataException(msg);
				try { ErrorHandler.Error.Record(ex); }
				catch { }
				return;
			}

			if (requiredDataSchemaVersion < dataSchemaVersionOfDb)
			{
				string msg = String.Format(CultureInfo.CurrentCulture, "The database structure is a more recent version ({0}) than the application is designed for {1}. Gallery Server Pro will attempt to ignore this difference, and hopefully it will not cause an issue.", Util.GetDataSchemaVersionString(), GalleryDataSchemaVersionEnumHelper.ConvertGalleryDataSchemaVersionToString(requiredDataSchemaVersion));

				ErrorHandler.CustomExceptions.DataException ex = new ErrorHandler.CustomExceptions.DataException(msg);
				try { ErrorHandler.Error.Record(ex); }
				catch { }
				return;
			}

			switch (dataSchemaVersionOfDb)
			{
				case GalleryDataSchemaVersion.V2_4_1:
					if (requiredDataSchemaVersion == GalleryDataSchemaVersion.V2_6_0)
					{
						ExecuteSqlUpgrade(GalleryDataSchemaUpgradeScript.SqlUpgrade_2_4_1_to_2_4_3);
						ExecuteSqlUpgrade(GalleryDataSchemaUpgradeScript.SqlUpgrade_2_4_3_to_2_4_4);
						ExecuteSqlUpgrade(GalleryDataSchemaUpgradeScript.SqlUpgrade_2_4_4_to_2_4_5);
						ExecuteSqlUpgrade(GalleryDataSchemaUpgradeScript.SqlUpgrade_2_4_5_to_2_4_6);
						ExecuteSqlUpgrade(GalleryDataSchemaUpgradeScript.SqlUpgrade_2_4_6_to_2_5_0);
						ExecuteSqlUpgrade(GalleryDataSchemaUpgradeScript.SqlUpgrade_2_5_0_to_2_6_0);
					}
					break;
				case GalleryDataSchemaVersion.V2_4_3:
					if (requiredDataSchemaVersion == GalleryDataSchemaVersion.V2_6_0)
					{
						ExecuteSqlUpgrade(GalleryDataSchemaUpgradeScript.SqlUpgrade_2_4_3_to_2_4_4);
						ExecuteSqlUpgrade(GalleryDataSchemaUpgradeScript.SqlUpgrade_2_4_4_to_2_4_5);
						ExecuteSqlUpgrade(GalleryDataSchemaUpgradeScript.SqlUpgrade_2_4_5_to_2_4_6);
						ExecuteSqlUpgrade(GalleryDataSchemaUpgradeScript.SqlUpgrade_2_4_6_to_2_5_0);
						ExecuteSqlUpgrade(GalleryDataSchemaUpgradeScript.SqlUpgrade_2_5_0_to_2_6_0);
					}
					break;
				case GalleryDataSchemaVersion.V2_4_4:
					if (requiredDataSchemaVersion == GalleryDataSchemaVersion.V2_6_0)
					{
						ExecuteSqlUpgrade(GalleryDataSchemaUpgradeScript.SqlUpgrade_2_4_4_to_2_4_5);
						ExecuteSqlUpgrade(GalleryDataSchemaUpgradeScript.SqlUpgrade_2_4_5_to_2_4_6);
						ExecuteSqlUpgrade(GalleryDataSchemaUpgradeScript.SqlUpgrade_2_4_6_to_2_5_0);
						ExecuteSqlUpgrade(GalleryDataSchemaUpgradeScript.SqlUpgrade_2_5_0_to_2_6_0);
					}
					break;
				case GalleryDataSchemaVersion.V2_4_5:
					if (requiredDataSchemaVersion == GalleryDataSchemaVersion.V2_6_0)
					{
						ExecuteSqlUpgrade(GalleryDataSchemaUpgradeScript.SqlUpgrade_2_4_5_to_2_4_6);
						ExecuteSqlUpgrade(GalleryDataSchemaUpgradeScript.SqlUpgrade_2_4_6_to_2_5_0);
						ExecuteSqlUpgrade(GalleryDataSchemaUpgradeScript.SqlUpgrade_2_5_0_to_2_6_0);
					}
					break;
				case GalleryDataSchemaVersion.V2_4_6:
					if (requiredDataSchemaVersion == GalleryDataSchemaVersion.V2_6_0)
					{
						ExecuteSqlUpgrade(GalleryDataSchemaUpgradeScript.SqlUpgrade_2_4_6_to_2_5_0);
						ExecuteSqlUpgrade(GalleryDataSchemaUpgradeScript.SqlUpgrade_2_5_0_to_2_6_0);
					}
					break;
				case GalleryDataSchemaVersion.V2_5_0:
					if (requiredDataSchemaVersion == GalleryDataSchemaVersion.V2_6_0)
					{
						ExecuteSqlUpgrade(GalleryDataSchemaUpgradeScript.SqlUpgrade_2_5_0_to_2_6_0);
					}
					break;
				default:
					string msg = String.Format(CultureInfo.CurrentCulture, "The database structure cannot be upgraded from version {0} to version {1}. This is an information message only and does not necessarily represent a problem.", Util.GetDataSchemaVersionString(), GalleryDataSchemaVersionEnumHelper.ConvertGalleryDataSchemaVersionToString(requiredDataSchemaVersion));

					ErrorHandler.CustomExceptions.DataException ex = new ErrorHandler.CustomExceptions.DataException(msg);
					try { ErrorHandler.Error.Record(ex); }
					catch { }
					break;
			}
		}

		/// <summary>
		/// Executes the SQL script represented by the specified <paramref name="script"/>. The actual SQL file is stored as an
		/// embedded resource in the current assembly.
		/// </summary>
		/// <param name="script">The script to execute. This value is used to lookup the SQL file stored as an embedded resource
		/// in the current assembly.</param>
		private static void ExecuteSqlUpgrade(GalleryDataSchemaUpgradeScript script)
		{
			System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
			string scriptLocation = String.Format(CultureInfo.InvariantCulture, "GalleryServerPro.Data.SqlServer.{0}.sql", script);
			using (System.IO.Stream stream = asm.GetManifestResourceStream(scriptLocation))
			{
				if (stream == null)
					throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "Unable to find embedded resource file named {0}", scriptLocation));

				ExecuteSqlInStream(stream);
			}
		}

		/// <summary>
		/// Execute the SQL statements in the specified stream.
		/// </summary>
		/// <param name="stream">A stream containing a series of SQL statements separated by the word GO.</param>
		/// <returns>Returns true if the SQL executed without error; otherwise returns false.</returns>
		/// <remarks>This function is copied from the install wizard in the web project.</remarks>
		private static void ExecuteSqlInStream(System.IO.Stream stream)
		{
			const int timeout = 600; // Timeout for SQL Execution (seconds)
			System.IO.StreamReader sr = null;
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			try
			{
				sr = new System.IO.StreamReader(stream);
				using (SqlConnection cn = GetDbConnection())
				{
					cn.Open();

					while (!sr.EndOfStream)
					{
						if (sb.Length > 0) sb.Remove(0, sb.Length); // Clear out string builder

						using (SqlCommand cmd = cn.CreateCommand())
						{
							while (!sr.EndOfStream)
							{
								string s = sr.ReadLine();
								if (s != null && s.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase))
								{
									break;
								}

								sb.AppendLine(s);
							}

							// Replace any replacement parameters with their intended values.
							sb.Replace("{schema}", Util.SqlServerSchema);
							sb.Replace("{objectQualifier}", Util.ObjectQualifier);

							// Execute T-SQL against the target database
							cmd.CommandText = sb.ToString();
							cmd.CommandTimeout = timeout;

							cmd.ExecuteNonQuery();
						}
					}
				}
			}
			catch (Exception ex)
			{
				if (!ex.Data.Contains(ExceptionSqlId))
				{
					ex.Data.Add(ExceptionSqlId, sb.ToString());
				}
				throw;
			}
			finally
			{
				if (sr != null)
					sr.Close();
			}
		}

		#endregion

	}
}
