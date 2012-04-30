using System;
using System.Collections.Generic;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.Data;

namespace GalleryServerPro.Provider.Interfaces
{
	/// <summary>
	/// Provides the interface definition for retrieving and persisting information to the data store.
	/// </summary>
	public interface IDataProvider
	{
		#region Properties

		/// <summary>
		/// Gets the friendly name used to refer to the provider during configuration.
		/// </summary>
		/// <value>The friendly name used to refer to the provider during configuration.</value>
		string Name { get; }

		/// <summary>
		/// Gets the data storage technology of this provider.
		/// </summary>
		/// <value>The data storage technology of this provider.</value>
		ProviderDataStore DataStore { get; }

		/// <summary>
		/// Gets a brief, friendly description suitable for display in administrative tools or other user interfaces (UIs).
		/// </summary>
		/// <value>A brief, friendly description suitable for display in administrative tools or other user interfaces (UIs).</value>
		string Description { get; }

		/// <summary>
		/// Gets the name of the connection string.
		/// </summary>
		/// <value>The name of the connection string.</value>
		string ConnectionStringName { get; }

		/// <summary>
		/// Gets or sets the name of the application to store and retrieve Gallery Server data for.
		/// </summary>
		/// <value>The name of the application to store and retrieve Gallery Server data for.</value>
		string ApplicationName { get; set; }

		#endregion

		#region Gallery Methods

		/// <summary>
		/// Fill the <paramref name="emptyCollection"/> with all the galleries in the current application. The return value is the same reference
		/// as the parameter. The template gallery is not included (that is, the one where the gallery ID = <see cref="Int32.MinValue" />.
		/// </summary>
		/// <param name="emptyCollection">An empty <see cref="IGalleryCollection"/> object to populate with the list of galleries in the current 
		/// application. This parameter is required because the library that implements this interface does not have
		/// the ability to directly instantiate any object that implements <see cref="IGalleryCollection"/>.</param>
		/// <returns>
		/// Returns an <see cref="IGalleryCollection" /> representing the galleries in the current application. The returned object is the
		/// same object in memory as the <paramref name="emptyCollection"/> parameter.
		/// </returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		IGalleryCollection Gallery_GetGalleries(IGalleryCollection emptyCollection);

		/// <summary>
		/// Persist the specified gallery to the data store. Return the ID of the gallery.
		/// </summary>
		/// <param name="gallery">An instance of <see cref="IGallery" /> to persist to the data store.</param>
		/// <returns>Return the ID of the gallery. If this is a new gallery and a new ID has been
		/// assigned, then this value has also been assigned to the <see cref="IGallery.GalleryId" /> property.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		int Gallery_Save(IGallery gallery);

		/// <summary>
		/// Permanently delete the specified gallery from the data store, including all related records. This action cannot
		/// be undone.
		/// </summary>
		/// <param name="gallery">The <see cref="IGallery" /> to delete from the data store.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		void Gallery_Delete(IGallery gallery);

		/// <summary>
		/// Configure the specified <paramref name="gallery" /> by verifying that a default set of
		/// records exist in the supporting tables (gs_Album, gs_GallerySetting, gs_MimeTypeGallery, gs_Synchronize, gs_Role_Album). 
		/// No changes are made to the file system as part of this operation. This method does not overwrite existing data, but it
		/// does insert missing data. This function can be used during application initialization to validate the data integrity for 
		/// a gallery. For example, if the user has added a record to the MIME types or template gallery settings tables, this method 
		/// will ensure that the new records are associated with the gallery identified in <paramref name="gallery" />.
		/// </summary>
		/// <param name="gallery">The gallery to configure.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="gallery" /> is null.</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		void Gallery_Configure(IGallery gallery);

		/// <summary>
		/// Return a collection representing all the gallery settings in the data store. 
		/// If no records are found in the data store, an empty collection is returned.
		/// </summary>
		/// <returns>Returns a collection containing all the gallery settings in the data store.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		IEnumerable<GallerySettingDto> GallerySetting_GetGallerySettings();

		/// <summary>
		/// Persist the current gallery settings to the data store.
		/// </summary>
		/// <param name="gallerySettings">An instance of <see cref="IGallerySettings" /> to persist to the data store.</param>
		void GallerySetting_Save(IGallerySettings gallerySettings);

		/// <summary>
		/// Return a collection representing all the gallery control settings in the data store. 
		/// If no records are found in the data store, an empty collection is returned.
		/// </summary>
		/// <returns>Returns a collection containing all the gallery control settings in the data store.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		IEnumerable<GalleryControlSettingDto> GalleryControlSetting_GetGalleryControlSettings();

		/// <summary>
		/// Persist the current gallery control settings to the data store.
		/// </summary>
		/// <param name="galleryControlSettings">An instance of <see cref="IGalleryControlSettings" /> to persist to the data store.</param>
		void GalleryControlSetting_Save(IGalleryControlSettings galleryControlSettings);

		#endregion

		#region Album Methods

		/// <summary>
		/// Return the album for the specified <paramref name="albumId" />. Returns null if no matching object
		/// is found in the data store.
		/// </summary>
		/// <param name="albumId">The ID that uniquely identifies the desired album.</param>
		/// <returns>Returns an instance of <see cref="AlbumDto" />, or null if no matching object is found.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		AlbumDto Album_GetAlbumById(int albumId);

		/// <summary>
		/// Return a collection of album IDs that are immediate children of the album represented by <paramref name="albumId" />.
		/// If no matching objects are found in the data store, an empty collection is returned.
		/// </summary>
		/// <param name="albumId">The ID that uniquely identifies the album for which to return the child albums
		/// 	contained within.</param>
		/// <returns>Returns a collection of all album IDs directly within the album represented by <paramref name="albumId" />.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		IEnumerable<int> Album_GetChildAlbumIdsById(int albumId);

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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		IEnumerable<MediaObjectDto> Album_GetChildMediaObjectsById(int albumId, bool includeMetadata);

		///// <summary>
		///// Return the top-level album for the specified <paramref name="galleryId" />. Returns null if no matching object
		///// is found in the data store.
		///// </summary>
		///// <param name="galleryId">The value that uniquely identifies the gallery.</param>
		///// <returns>Returns an instance of <see cref="AlbumDto" />, or null if no matching object is found.</returns>
		//[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		//AlbumDto Album_GetRootAlbum(int galleryId);

		/// <summary>
		/// Persist the specified album to the data store. Return the ID of the album.
		/// </summary>
		/// <param name="album">An instance of <see cref="IAlbum" /> to persist to the data store.</param>
		/// <returns>Return the ID of the album. If this is a new album and a new ID has been
		/// assigned, then this value has also been assigned to the ID property of the object.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		int Album_Save(IAlbum album);

		/// <summary>
		/// Permanently delete the specified album from the data store, including any
		/// child albums and media objects (cascading delete). This action cannot be undone.
		/// </summary>
		/// <param name="album">The <see cref="IAlbum" /> to delete from the data store.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		void Album_Delete(IAlbum album);

		#endregion

		#region Media Object Methods

		/// <summary>
		/// Return the media object for the specified <paramref name="mediaObjectId" />, optionally including the metadata 
		/// for each item. Returns null if no matching object is found in the data store.
		/// </summary>
		/// <param name="mediaObjectId">The ID that uniquely identifies the desired media object.</param>
		/// <param name="includeMetadata">When <c>true</c> include metadata items on the 
		/// <see cref="MediaObjectDto.MediaObjectMetadata" /> property. When <c>false</c>, the property is null.</param>
		/// <returns>Returns an instance of <see cref="MediaObjectDto" />, or null if no matching object is found.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		MediaObjectDto MediaObject_GetMediaObjectById(int mediaObjectId, bool includeMetadata);

		/// <summary>
		/// Return a collection representing the metadata items for the specified <paramref name="mediaObjectId" />. If no matching object
		/// is found in the data store, an empty collection is returned.
		/// </summary>
		/// <param name="mediaObjectId">The ID that uniquely identifies the desired media object.</param>
		/// <returns>Returns a collection of all metadata items.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		IEnumerable<MediaObjectMetadataDto> MediaObject_GetMetadataItemsByMediaObjectId(int mediaObjectId);

		/// <summary>
		/// Persist the specified media object to the data store. Return the ID of the media object.
		/// </summary>
		/// <param name="mediaObject">An instance of <see cref="IGalleryObject" /> to persist to the data store.</param>
		/// <returns>Return the ID of the media object. If this is a new media object and a new ID has been
		/// assigned, then this value has also been assigned to the ID property of the object.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="mediaObject" /> is null.</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		int MediaObject_Save(IGalleryObject mediaObject);

		/// <summary>
		/// Permanently delete the specified media object from the data store. This action cannot
		/// be undone.
		/// </summary>
		/// <param name="mediaObject">The <see cref="IGalleryObject" /> to delete from the data store.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="mediaObject" /> is null.</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		void MediaObject_Delete(IGalleryObject mediaObject);

		/// <summary>
		/// Return a collection representing the hash keys for all media objects in the data store.
		/// </summary>
		/// <returns>Returns a collection object with one field named "HashKey" containing the hash keys
		/// for all media objects in the data store.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		System.Collections.Specialized.StringCollection MediaObject_GetAllHashKeys();

		#endregion

		#region Media Queue Methods

		/// <summary>
		/// Return a collection representing media objects currently being processed. If no objects are found 
		/// in the data store, an empty collection is returned.
		/// </summary>
		/// <returns>Returns a collection object.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		IEnumerable<MediaQueueDto> MediaQueue_GetMediaQueues();

		/// <summary>
		/// Persist the specified media queue item to the data store. The ID of the new item is assigned to 
		/// <see cref="MediaQueueDto.MediaQueueId" />.
		/// </summary>
		/// <param name="mediaQueue">The media queue item to persist to the data store.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		void MediaQueue_Save(MediaQueueDto mediaQueue);

		/// <summary>
		/// Delete the media queue item from the data store.
		/// </summary>
		/// <param name="mediaQueue">The media queue item to delete from the data store.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		void MediaQueue_Delete(MediaQueueDto mediaQueue);

		#endregion

		#region Synchronize Methods

		/// <summary>
		/// Persist the synchronization information to the data store.
		/// </summary>
		/// <param name="synchStatus">An <see cref="ISynchronizationStatus" /> object containing the synchronization information
		/// to persist to the data store.</param>
		/// <exception cref="GalleryServerPro.ErrorHandler.CustomExceptions.SynchronizationInProgressException">Thrown when the data
		/// store indicates another synchronization is already in progress for this gallery.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="synchStatus" /> is null.</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		void Synchronize_SaveStatus(ISynchronizationStatus synchStatus);

		/// <summary>
		/// Retrieve the most recent synchronization information from the data store.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <param name="factory">An instance of <see cref="IFactory" />. It is used to instantiate a <see cref="ISynchronizationStatus" /> object.</param>
		/// <returns>
		/// Returns an <see cref="ISynchronizationStatus"/> object with the most recent synchronization information from the data store.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="factory" /> is null.</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		ISynchronizationStatus Synchronize_RetrieveStatus(int galleryId, IFactory factory);

		#endregion

		#region General Methods

		/// <summary>
		/// Perform any needed data store operations to get Gallery Server ready to go. This includes upgrading the 
		/// database to the version required by this provider.
		/// </summary>
		void InitializeDataStore();

		/// <summary>
		/// Return a collection representing the application settings in the data store. 
		/// If no records are found in the data store, an empty collection is returned.
		/// </summary>
		/// <returns>Returns a collection containing the application settings in the data store.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		IEnumerable<AppSettingDto> AppSetting_GetAppSettings();

		/// <summary>
		/// Persist the current application settings to the data store.
		/// </summary>
		/// <param name="appSetting">An instance of <see cref="IAppSetting" /> to persist to the data store.</param>
		void AppSetting_Save(IAppSetting appSetting);

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
		void SearchGallery(int galleryId, string[] searchTerms, out List<int> matchingAlbumIds, out List<int> matchingMediaObjectIds);

		/// <summary>
		/// Begins a new database transaction. All subsequent database actions occur within the context of this transaction. 
		/// Use <see cref="CommitTransaction" /> to commit this transaction or <see cref="RollbackTransaction" /> to abort it. If a transaction
		/// is already in progress, then this method returns without any action, which preserves the original transaction.
		/// Note: This function is not implemented.
		/// </summary>
		void BeginTransaction();

		/// <summary>
		/// Commits the current transaction, if one exists. A transaction is created with the <see cref="BeginTransaction" /> method.
		/// If there is not an existing transaction, no action is taken. If this method is called when a datareader is open, the 
		/// actual commit is delayed until all datareaders are disposed. Note: This function is not implemented.
		/// </summary>
		void CommitTransaction();

		/// <summary>
		/// Aborts the current transaction, if one exists. A transaction is created with the <see cref="BeginTransaction" /> method.
		/// If there is not an existing transaction, no action is taken. Note: This function is not implemented.
		/// </summary>
		void RollbackTransaction();

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
		void ImportGalleryData(string galleryData, bool importMembershipData, bool importGalleryData);

		/// <summary>
		/// Exports the Gallery Server Pro data in the current database to an XML-formatted string. Does not export the actual media files;
		/// they must be copied manually with a utility such as Windows Explorer. This method does not make any changes to the database tables
		/// or the files in the media objects directory.
		/// </summary>
		/// <param name="exportMembershipData">If set to <c>true</c>, user accounts and other membership data will be exported.</param>
		/// <param name="exportGalleryData">If set to <c>true</c>, albums, media objects, and other gallery data will be exported.</param>
		/// <returns>Returns an XML-formatted string containing the gallery data.</returns>
		string ExportGalleryData(bool exportMembershipData, bool exportGalleryData);

		/// <summary>
		/// Validates that the backup file specified in the <see cref="IBackupFile.FilePath" /> property of the <paramref name="backupFile"/> 
		/// parameter is valid and populates the remaining properties with information about the file.
		/// </summary>
		/// <param name="backupFile">An instance of <see cref="IBackupFile" /> that with only the <see cref="IBackupFile.FilePath" /> 
		/// property assigned. The remaining properties should be uninitialized since they will be assigned in this method.</param>
		void ValidateBackupFile(ref IBackupFile backupFile);

		/// <summary>
		/// Reclaims wasted space in the database and recalculates identity column values. Applies only to SQL CE.
		/// </summary>
		void Compact();

		/// <summary>
		/// Recalculates the checksums for each page in the database and compares the new checksums to the expected values. Also verifies
		/// that each index entry exists in the table and that each table entry exists in the index. Applies only to SQL CE.
		/// </summary>
		/// <returns><c>True</c> if there is no database corruption; otherwise, <c>false</c>.</returns>
		bool Verify();

		/// <summary>
		/// Repairs a corrupted database. Call this method when <see cref="Verify" /> returns false. Applies only to SQL CE.
		/// </summary>
		void Repair();

		#endregion

		#region Security Methods

		/// <summary>
		/// Return a collection representing the roles for all galleries. If no matching objects
		/// are found in the data store, an empty collection is returned.
		/// </summary>
		/// <returns>Returns a collection object representing the roles for all galleries.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		IEnumerable<RoleDto> Roles_GetRoles();

		/// <summary>
		/// Persist this gallery server role to the data store. The list of top-level albums this role applies to, which is stored
		/// in the <see cref="IGalleryServerRole.RootAlbumIds" /> property, must also be saved. The data provider automatically 
		/// repopulates the <see cref="IGalleryServerRole.AllAlbumIds" /> and <see cref="IGalleryServerRole.Galleries" /> properties.
		/// </summary>
		/// <param name="role">An instance of IGalleryServerRole to persist to the data store.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		void Role_Save(IGalleryServerRole role);

		/// <summary>
		/// Permanently delete this gallery server role from the data store, including the list of role/album relationships
		/// associated with this role. This action cannot be undone.
		/// </summary>
		/// <param name="role">An instance of <see cref="IGalleryServerRole" /> to delete from the data store.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		void Role_Delete(IGalleryServerRole role);

		#endregion

		#region App Error Methods

		/// <summary>
		/// Return a collection representing the application errors. If no objects are found 
		/// in the data store, an empty collection is returned.
		/// </summary>
		/// <returns>Returns a collection object with all application error fields.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		IEnumerable<AppErrorDto> AppError_GetAppErrors();

		/// <summary>
		/// Persist the specified application error to the data store. Return the ID of the error.
		/// </summary>
		/// <param name="appError">The application error to persist to the data store.</param>
		/// <returns>Return the ID of the error. If this is a new error object and a new ID has been
		/// assigned, then this value has also been assigned to the ID property of the object.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		int AppError_Save(IAppError appError);

		/// <summary>
		/// Delete the application error from the data store.
		/// </summary>
		/// <param name="appErrorId">The value that uniquely identifies this application error (<see cref="IAppError.AppErrorId"/>).</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		void AppError_Delete(int appErrorId);

		/// <summary>
		/// Permanently delete all errors from the data store that are system-wide (that is, not associated with a specific gallery) and also
		/// those errors belonging to the specified <paramref name="galleryId" />.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		void AppError_ClearLog(int galleryId);

		#endregion

		#region MIME Type Methods

		/// <summary>
		/// Return a collection representing the MIME types. If no objects are found 
		/// in the data store, an empty collection is returned.
		/// </summary>
		/// <returns>Returns a collection object representing the MIME types.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		IEnumerable<MimeTypeDto> MimeType_GetMimeTypes();

		/// <summary>
		/// Return a collection representing the gallery-specific settings for MIME types. If no objects are found 
		/// in the data store, an empty collection is returned.
		/// </summary>
		/// <returns>Returns a collection object representing the gallery-specific settings for MIME types.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		IEnumerable<MimeTypeGalleryDto> MimeType_GetMimeTypeGalleries();

		/// <summary>
		/// Persist the gallery-specific properties of the <paramref name="mimeType" /> to the data store. Currently, only the 
		/// <see cref="IMimeType.AllowAddToGallery" /> property is unique to the gallery identified in <see cref="IMimeType.GalleryId" />; 
		/// the other properties are application-wide and at present there is no API to modify them. In other words, this method saves whether a 
		/// particular MIME type is enabled or disabled for a particular gallery.
		/// </summary>
		/// <param name="mimeType">The MIME type instance to save.</param>
		/// <exception cref="ArgumentException">Thrown when the <see cref="IMimeType.MimeTypeGalleryId"/> property of the <paramref name="mimeType" />
		/// parameter is not set to a valid value.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="mimeType" /> is null.</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		void MimeType_Save(IMimeType mimeType);

		/// <summary>
		/// Fill the <paramref name="emptyCollection"/> with all the browser templates in the current application. The return value is the same reference
		/// as the parameter.
		/// </summary>
		/// <param name="emptyCollection">An empty <see cref="IBrowserTemplateCollection"/> object to populate with the list of browser templates in the current 
		/// application. This parameter is required because the library that implements this interface does not have
		/// the ability to directly instantiate any object that implements <see cref="IBrowserTemplateCollection"/>.</param>
		/// <returns>
		/// Returns an <see cref="IBrowserTemplateCollection" /> representing the browser templates in the current application. The returned object is the
		/// same object in memory as the <paramref name="emptyCollection"/> parameter.
		/// </returns>
		IBrowserTemplateCollection MimeType_GetBrowserTemplates(IBrowserTemplateCollection emptyCollection);

		#endregion

		#region Profile Methods

		/// <summary>
		/// Gets the profile for the specified user. Guaranteed to not return null.
		/// </summary>
		/// <param name="userName">The user name that uniquely identifies the user.</param>
		/// <param name="factory">An instance of <see cref="IFactory" />. It is used to instantiate the necessary object(s).</param>
		/// <returns>Returns an <see cref="IUserProfile" /> object containing the profile for the user.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="factory" /> is null.</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		IUserProfile Profile_GetUserProfile(string userName, IFactory factory);

		/// <summary>
		/// Persist the specified <paramref name="profile" /> to the data store.
		/// </summary>
		/// <param name="profile">The profile to persist to the data store.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="profile" /> is null.</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		void Profile_Save(IUserProfile profile);

		/// <summary>
		/// Permanently delete the profile records for the specified <paramref name="userName" />.
		/// </summary>
		/// <param name="userName">The user name that uniquely identifies the user.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		void Profile_DeleteProfileForUser(string userName);

		/// <summary>
		/// Permanently delete the profile records associated with the specified <paramref name="galleryId" />.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
		void Profile_DeleteProfilesForGallery(int galleryId);

		#endregion
	}
}
