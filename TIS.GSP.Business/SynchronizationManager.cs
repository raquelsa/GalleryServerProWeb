using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.Business.Properties;
using GalleryServerPro.ErrorHandler;
using GalleryServerPro.ErrorHandler.CustomExceptions;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// Contains functionality for synchronizing the media object files on the hard drive with the records in the data store.
	/// </summary>
	public class SynchronizationManager
	{
		#region Private Fields

		private readonly int _galleryId;
		private readonly string _thumbnailRootPath;
		private readonly string _optimizedRootPath;
		private readonly long _optimizedTriggerSizeKb;
		private readonly int _optimizedMaxLength;
		private readonly string _thumbnailPrefix;
		private readonly string _optimizedPrefix;
		private readonly int _fullMediaObjectPathLength;

		private bool _isRecursive;
		private bool _rebuildOptimized;
		private bool _rebuildThumbnail;
		private bool _regenerateMetadata;
		private string _userName;
		private int _lastTransactionCommitFileIndex;
		private IGallerySettings _gallerySetting;

		// About the synch status object: When a synch is started, we grab a reference to the singleton synch status for the gallery and
		// update its properties with the current synch info, then we persist to the database so other processes (such as an external 
		// utility) can check for synch status info. However, as the synch progresses we only update the in-memory version of the object and
		// do not write to the data store until the synch is complete, where we then mark the synch record as being complete.
		private ISynchronizationStatus _synchStatus;

		// Read-only lists pulled from data store at beginning
		private Dictionary<String, IAlbum> _albumsFromDataStore;
		private Dictionary<String, IGalleryObject> _mediaObjectsFromDataStore;

		#endregion

		#region Constructor

		/// <summary>
		/// Instantiates a new <see cref="SynchronizationManager" /> object, with the properties 
		/// <see cref="IsRecursive" />, <see cref="RebuildOptimized" />, and <see cref="RebuildThumbnail" />
		/// all defaulted to true. The property <see cref="RegenerateMetadata" /> is set to false.
		/// </summary>
		/// <param name="galleryId">The value that uniquely identifies the gallery to be synchronized.</param>
		public SynchronizationManager(int galleryId)
		{
			_galleryId = galleryId;
			_thumbnailRootPath = GallerySettings.FullThumbnailPath;
			_optimizedRootPath = GallerySettings.FullOptimizedPath;
			_optimizedTriggerSizeKb = GallerySettings.OptimizedImageTriggerSizeKb;
			_optimizedMaxLength = GallerySettings.MaxOptimizedLength;
			_thumbnailPrefix = GallerySettings.ThumbnailFileNamePrefix;
			_optimizedPrefix = GallerySettings.OptimizedFileNamePrefix;
			_fullMediaObjectPathLength = GallerySettings.FullMediaObjectPath.Length;

			_isRecursive = true;
			_rebuildOptimized = true;
			_rebuildThumbnail = true;
			_regenerateMetadata = false;
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Indicates whether the synchronization continues drilling down into directories
		/// below the current one. The default value is true.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the synchronization procedure recursively
		/// synchronizes all directories within the current one; otherwise, <c>false</c>.
		/// </value>
		public bool IsRecursive
		{
			get { return this._isRecursive; }
			set { this._isRecursive = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the optimized version is deleted and overwritten 
		/// with a new one based on the original file. Only relevant for images and for video/audio 
		/// files when FFmpeg is installed and an applicable encoder setting exists. The default 
		/// value is true.
		/// </summary>
		/// <value><c>true</c> if optimized images are overwritten during a synchronization; 
		/// otherwise, <c>false</c>.</value>
		public bool RebuildOptimized
		{
			get { return this._rebuildOptimized; }
			set { this._rebuildOptimized = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether to replace the existing metadata for each media object with
		/// the metadata stored within the media object file. The default value is false.
		/// </summary>
		/// <value><c>true</c> if metadata is to be regenerated during a synchronization; otherwise, <c>false</c>.</value>
		public bool RegenerateMetadata
		{
			get { return _regenerateMetadata; }
			set { _regenerateMetadata = value; }
		}

		/// <summary>
		/// Gets or sets the user name for the logged on user. This is used for the audit fields in the album and media
		/// objects.
		/// </summary>
		/// <value>The user name for the logged on user.</value>
		public string UserName
		{
			get { return _userName; }
			set { _userName = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether a thumbnail image is deleted and overwritten 
		/// with a new one based on the original file. Applies to all media objects. The default 
		/// value is true.
		/// </summary>
		/// <value><c>true</c> if thumbnail images are overwritten during a synchronization; 
		/// otherwise, <c>false</c>.</value>
		public bool RebuildThumbnail
		{
			get { return this._rebuildThumbnail; }
			set { this._rebuildThumbnail = value; }
		}

		#endregion

		#region Private Properties

		private IGallerySettings GallerySettings
		{
			get
			{
				if (_gallerySetting == null)
				{
					_gallerySetting = Factory.LoadGallerySetting(_galleryId);
				}

				return _gallerySetting;
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Synchronize the media object library, starting with the root album. Optionally specify that only the 
		/// specified album is synchronized. If <see cref="IsRecursive" /> = true, then child albums are recursively synchronized;
		/// otherwise, only the root album (or the specified album if that overload is used) is synchronized.
		/// </summary>
		/// <param name="synchId">A GUID that uniquely indentifies the synchronization. If another synchronization is in 
		/// progress, a <see cref="GalleryServerPro.ErrorHandler.CustomExceptions.SynchronizationInProgressException" /> exception is thrown.</param>
		/// <param name="userName">The user name for the logged on user. This is used for the audit fields in the album 
		/// and media objects.</param>
		/// <exception cref="GalleryServerPro.ErrorHandler.CustomExceptions.SynchronizationInProgressException">
		/// Thrown if another synchronization is in progress.</exception>
		public void Synchronize(string synchId, string userName)
		{
			this.Synchronize(synchId, Factory.LoadRootAlbumInstance(_galleryId, false), userName);
		}

		/// <summary>
		/// Synchronize the media object library, starting with the root album. Optionally specify that only the 
		/// specified album is synchronized. If <see cref="IsRecursive" /> = true, then child albums are recursively synchronized;
		/// otherwise, only the root album (or the specified album if that overload is used) is synchronized.
		/// </summary>
		/// <param name="synchId">A GUID that uniquely indentifies the synchronization. If another synchronization is in 
		/// progress, a <see cref="GalleryServerPro.ErrorHandler.CustomExceptions.SynchronizationInProgressException" /> exception is thrown.</param>
		/// <param name="userName">The user name for the logged on user. This is used for the audit fields in the album 
		/// and media objects.</param>
		/// <param name="album">The album to synchronize.</param>
		/// <exception cref="GalleryServerPro.ErrorHandler.CustomExceptions.SynchronizationInProgressException">
		/// Thrown if another synchronization is in progress.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="album" /> is null.</exception>
		public void Synchronize(string synchId, IAlbum album, string userName)
		{
			if (album == null)
				throw new ArgumentNullException("album");

			try
			{
				Initialize(synchId, album, userName); // Will throw SynchronizationInProgressException if another is in progress. Will be caught by upstream code.

				Factory.GetDataProvider().BeginTransaction();

				DirectoryInfo albumDirectory = new DirectoryInfo(album.FullPhysicalPathOnDisk);

				// Update this album.
				album.IsSynchronized = true;

				// Synchronize the files in this album. No recursive action.
				SynchronizeMediaObjectFiles(albumDirectory, album);

				// Synchronize any external media objects previously added. No recursive action.
				SynchronizeExternalMediaObjects(album);

				if (this.IsRecursive)
				{
					// Synchronize the child directories and their files. Acts recursively.
					SynchronizeChildDirectories(albumDirectory, album);
				}

				// Persist synchronized objects to the data store and delete the unsynchronized ones.
				DeleteUnsynchronizedObjects();

				DeleteOrphanedImages(album);

				Album.AssignAlbumThumbnail(album, false, true, this._userName);
			}
			catch (GalleryServerPro.ErrorHandler.CustomExceptions.SynchronizationTerminationRequestedException)
			{
				// The user has cancelled the synchronization. Swallow the exception and return.
				Factory.GetDataProvider().RollbackTransaction();

				return;
			}
			catch
			{
				Factory.GetDataProvider().RollbackTransaction();
				throw;
			}
			finally
			{
				// Set the list of hash keys to null to clean up memory.
				MediaObjectHashKeys.Clear();

				HelperFunctions.PurgeCache();

				if (this._synchStatus != null)
					this._synchStatus.Finish();
			}

			Factory.GetDataProvider().CommitTransaction();
		}

		#endregion

		#region Private methods

		private void Initialize(string synchId, IAlbum album, string userName)
		{
			if (album == null)
				throw new ArgumentNullException("album");

			if (String.IsNullOrEmpty(userName))
				throw new ArgumentNullException("userName");

			this._userName = userName;

			#region Set up the _synchStatus instance

			// Tell the status instance we are starting a new synchronization. It will throw
			// SynchronizationInProgressException if another is in progress.
			this._synchStatus = SynchronizationStatus.Start(synchId, album.GalleryId);

			this._synchStatus.Update(SynchronizationState.NotSet, CountFiles(album.FullPhysicalPathOnDisk), null, null, null, null, true);

			#endregion

			#region Populate the _albumsFromDataStore and _mediaObjectsFromDataStore dictionary objects and set each to IsSynchronized = false

			this._albumsFromDataStore = new Dictionary<String, IAlbum>();
			this._mediaObjectsFromDataStore = new Dictionary<String, IGalleryObject>(this._synchStatus.TotalFileCount);

			// Fill _albums and _mediaObjects with the albums and media objects for this album as currently stored 
			// in the data store. We'll be comparing these objects with those we find on the hard drive. Act recursively
			// if IsRecursive = true. Set IsSynchronized = false for each object. (We'll be setting it back to true
			// as we synchronize each object.)
			album.IsSynchronized = false;
			album.RegenerateThumbnailOnSave = this.RebuildThumbnail;

			if (!album.IsWritable)
			{
				throw new WebException(String.Format(CultureInfo.CurrentCulture, "The album is not writeable (ID {0}, Title='{1}')", album.Id, album.Title));
			}

			this._albumsFromDataStore.Add(album.FullPhysicalPathOnDisk, album);

			foreach (IGalleryObject mediaObject in album.GetChildGalleryObjects(GalleryObjectType.MediaObject))
			{
				if (!mediaObject.IsWritable)
				{
					throw new WebException(String.Format(CultureInfo.CurrentCulture, "The media object is not writeable (ID {0}, Title='{1}')", mediaObject.Id, mediaObject.Title));
				}

				mediaObject.IsSynchronized = false;
				mediaObject.RegenerateThumbnailOnSave = this.RebuildThumbnail;
				mediaObject.RegenerateOptimizedOnSave = this.RebuildOptimized;
				mediaObject.ExtractMetadataOnSave = this.RegenerateMetadata;

				if (!String.IsNullOrEmpty(mediaObject.Hashkey))
				{
					this._mediaObjectsFromDataStore.Add(mediaObject.Hashkey, mediaObject);
				}
			}

			if (this._isRecursive)
			{
				AddChildAlbumsAndGalleryObjectsAndSetToUnsynchronized(this._albumsFromDataStore, this._mediaObjectsFromDataStore, album);
			}

			#endregion

			// Clear the list of hash keys so we're starting with a fresh load from the data store.
			MediaObjectHashKeys.Clear();
		}

		/// <summary>
		/// Add the child albums and media objects as stored on disk to the specified dictionary objects. Set
		/// IsSynchronized = false for each album and media object. This will be set to true as each is processed.
		/// This method calls itself recursively if IsRecursive = true.
		/// </summary>
		/// <param name="albums">A Dictionary object containing relevant albums for this synchronization. The album specified
		/// in the parentAlbum parameter will be added to this object.</param>
		/// <param name="mediaObjects">A Dictionary object containing relevant media objects for this synchronization.
		/// Media objects within the parentAlbum parameter will be added to this object.</param>
		/// <param name="parentAlbum">The album used as the source for populating the albums and mediaObjects
		/// parameters.</param>
		private void AddChildAlbumsAndGalleryObjectsAndSetToUnsynchronized(Dictionary<string, IAlbum> albums, Dictionary<string, IGalleryObject> mediaObjects, IAlbum parentAlbum)
		{
			foreach (IAlbum childAlbum in parentAlbum.GetChildGalleryObjects(GalleryObjectType.Album))
			{
				if (!childAlbum.IsWritable)
				{
					throw new WebException(String.Format(CultureInfo.CurrentCulture, "The album is not writeable (ID {0}, Title='{1}')", childAlbum.Id, childAlbum.Title));
				}

				childAlbum.IsSynchronized = false;
				childAlbum.RegenerateThumbnailOnSave = this.RebuildThumbnail;

				try
				{
					// There can be situations where the database becomes corrupt, and the same album has two records. When this happens,
					// the following line will fail. Instead of letting the exception cause the synch to fail, we swallow the exception.
					// This will cause the album that caused the exception to eventually be deleted, which is what we want.
					albums.Add(childAlbum.FullPhysicalPathOnDisk, childAlbum);
				}
				catch (System.ArgumentException) { }

				foreach (IGalleryObject mediaObject in childAlbum.GetChildGalleryObjects(GalleryObjectType.MediaObject, false))
				{
					if (!String.IsNullOrEmpty(mediaObject.Hashkey))
					{
						if (!mediaObject.IsWritable)
						{
							throw new WebException(String.Format(CultureInfo.CurrentCulture, "The media object is not writeable (ID {0}, Title='{1}')", mediaObject.Id, mediaObject.Title));
						}

						mediaObject.IsSynchronized = false;
						mediaObject.RegenerateOptimizedOnSave = this.RebuildOptimized;
						mediaObject.RegenerateThumbnailOnSave = this.RebuildThumbnail;
						mediaObject.ExtractMetadataOnSave = this.RegenerateMetadata;

						try
						{
							// There may be situations where the database becomes corrupt, and the same media object has two records. When this happens,
							// the following line will fail. Instead of letting the exception cause the synch to fail, we swallow the exception.
							// This will cause the media object that caused the exception to eventually be deleted, which is what we want.
							mediaObjects.Add(mediaObject.Hashkey, mediaObject);
						}
						catch (System.ArgumentException) { }
					}
				}

				string albumMsg = String.Format(CultureInfo.InvariantCulture, Resources.SynchronizationStatus_Loading_Album_Msg, childAlbum.Title);
				this._synchStatus.Update(SynchronizationState.NotSet, null, String.Empty, 0, albumMsg, null, false);

				if (this._isRecursive)
				{
					AddChildAlbumsAndGalleryObjectsAndSetToUnsynchronized(albums, mediaObjects, childAlbum);
				}
			}

		}

		/// <summary>
		/// Get the number of files in the specified directory path, including any subdirectories if
		/// IsRecursive = true. But don't count any optimized or thumbnail files.
		/// </summary>
		/// <param name="directoryPath"></param>
		/// <returns></returns>
		/// <exception cref="System.IO.DirectoryNotFoundException">Thrown if the specified directory does not exist.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="directoryPath" /> is null or an empty string.</exception>
		private int CountFiles(string directoryPath)
		{
			if (String.IsNullOrEmpty(directoryPath))
				throw new ArgumentOutOfRangeException("directoryPath");

			int countTotal;

			try
			{
				countTotal = Directory.GetFiles(directoryPath).Length;
			}
			catch (UnauthorizedAccessException)
			{
				return 0;
			}

			// Get a count of the thumbnail and optimized images, but only if they are stored in the media objects directory.
			int countThumbnail = 0;
			if (GallerySettings.FullThumbnailPath.Equals(GallerySettings.FullMediaObjectPath))
			{
				try
				{
					countThumbnail = Directory.GetFiles(directoryPath, GallerySettings.ThumbnailFileNamePrefix + "*").Length;
				}
				catch (UnauthorizedAccessException) { }
			}

			int countOptimized = 0;
			if (GallerySettings.FullOptimizedPath.Equals(GallerySettings.FullMediaObjectPath))
			{
				try
				{
					countOptimized = Directory.GetFiles(directoryPath, GallerySettings.OptimizedFileNamePrefix + "*").Length;
				}
				catch (UnauthorizedAccessException) { }
			}

			string[] dirs = null;
			try
			{
				dirs = Directory.GetDirectories(directoryPath);
			}
			catch (UnauthorizedAccessException) { }

			if (_isRecursive && (dirs != null))
			{
				foreach (string dir in dirs)
				{
					countTotal += CountFiles(dir);
				}
			}

			int totalNumFiles = countTotal - countThumbnail - countOptimized;

			// If we compute a number < 0, then just return 0.
			return (totalNumFiles < 0 ? 0 : totalNumFiles);
		}

		/// <summary>
		/// Ensure the directories and media object files within parentDirectory have corresponding albums 
		/// and media objects. An exception is thrown if parentAlbum.FullPhysicalPathOnDisk does not equal
		/// parentDirectory.FullName. If IsRecursive = true, this method recursively calls itself.
		/// </summary>
		/// <param name="parentDirectory">A DirectoryInfo instance corresponding to the FullPhysicalPathOnDisk
		/// property of parentAlbum.</param>
		/// <param name="parentAlbum">An album instance. Directories under the parentDirectory parameter will be
		/// added (or updated if they already exist) as child albums of this instance.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="parentDirectory" /> or <paramref name="parentAlbum" /> is null.</exception>
		private void SynchronizeChildDirectories(DirectoryInfo parentDirectory, IAlbum parentAlbum)
		{
			#region Parameter validation

			if (parentDirectory == null)
				throw new ArgumentNullException("parentDirectory");

			if (parentAlbum == null)
				throw new ArgumentNullException("parentAlbum");

			if (parentDirectory.FullName != parentAlbum.FullPhysicalPath)
				throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.SynchronizationManager_SynchronizeChildDirectories_Ex_Msg));

			#endregion

			// Recursively traverse all subdirectories and their files and synchronize each object we find.
			// Skip any hidden directories.
			DirectoryInfo[] childDirectories;
			try
			{
				childDirectories = parentDirectory.GetDirectories();
			}
			catch (UnauthorizedAccessException)
			{
				return;
			}

			foreach (DirectoryInfo subdirectory in childDirectories)
			{
				if ((subdirectory.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
				{
					this._synchStatus.SkippedMediaObjects.Add(new KeyValuePair<string, string>(subdirectory.FullName.Remove(0, _fullMediaObjectPathLength + 1), Resources.SynchronizationStatus_Hidden_Directory_Msg));
					continue;
				}

				IAlbum childAlbum = SynchronizeDirectory(subdirectory, parentAlbum);

				try
				{
					SynchronizeMediaObjectFiles(subdirectory, childAlbum);

					SynchronizeExternalMediaObjects(childAlbum);

					SynchronizeChildDirectories(subdirectory, childAlbum);
				}
				catch (UnauthorizedAccessException)
				{
					childAlbum.DeleteFromGallery();
				}
			}
		}

		/// <summary>
		/// Synchronizes the media object files.
		/// </summary>
		/// <param name="directory">The directory.</param>
		/// <param name="album">The album.</param>
		/// <exception cref="UnauthorizedAccessException">Thrown when the IIS app pool identity cannot access the files in the directory.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="album" /> or <paramref name="directory" /> is null.</exception>
		/// <exception cref="ArgumentException">Thrown when the full directory path of <paramref name="directory" /> does not match the directory path of 
		/// <paramref name="album" />.</exception>
		private void SynchronizeMediaObjectFiles(DirectoryInfo directory, IAlbum album)
		{
			#region Parameter validation

			if (directory == null)
				throw new ArgumentNullException("directory");

			if (album == null)
				throw new ArgumentNullException("album");

			if (!directory.FullName.Equals(album.FullPhysicalPath, StringComparison.OrdinalIgnoreCase))
				throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "Error in SynchronizeMediaObjectFiles(): The full directory path of the parameter 'directory' does not match the directory path of the parameter 'album'. directory.FullName='{0}'; album.FullPhysicalPath='{1}'", directory.FullName, album.FullPhysicalPath));

			#endregion

			//Update the media object table in the database with the file attributes of all
			//files in the directory passed to this function. Skip any hidden files.
			FileInfo[] files;
			try
			{
				files = directory.GetFiles();
			}
			catch (UnauthorizedAccessException)
			{
				_synchStatus.SkippedMediaObjects.Add(new KeyValuePair<string, string>(directory.Name, Resources.SynchronizationStatus_Restricted_Directory_Msg));
				throw;
			}

			// First sort by the filename.
			Array.Sort(files, (a, b) => String.Compare(a.Name, b.Name, StringComparison.Ordinal));

			foreach (FileInfo file in files)
			{
				if ((file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
				{
					_synchStatus.SkippedMediaObjects.Add(new KeyValuePair<string, string>(file.FullName.Remove(0, _fullMediaObjectPathLength + 1), Resources.SynchronizationStatus_Hidden_File_Msg));
					continue;
				}

				#region Process thumbnail or optimized image

				if (file.Name.StartsWith(_thumbnailPrefix, StringComparison.OrdinalIgnoreCase))
				{
					// We have a thumbnail image. If we are storing thumbnails in a different directory, delete the file, but only if the path
					// is writeable. The user may have just specified a new thumbnail path, and we need to delete all the previous thumbnails 
					// from their original location.
					if (_thumbnailRootPath != GallerySettings.FullMediaObjectPath && !GallerySettings.MediaObjectPathIsReadOnly)
					{
						File.Delete(file.FullName);
					}
					continue;
				}

				if (file.Name.StartsWith(_optimizedPrefix, StringComparison.OrdinalIgnoreCase))
				{
					// We have an optimized image. If we are storing optimized images in a different directory, delete the file, but only if the path
					// is writeable. The user may have just specified a new optimized path, and we need to delete all the previous optimized images 
					// from their original location.
					if (_optimizedRootPath != GallerySettings.FullMediaObjectPath && !GallerySettings.MediaObjectPathIsReadOnly)
					{
						File.Delete(file.FullName);
					}
					continue;
				}

				#endregion

				IGalleryObject mediaObject = null;
				// See if this file is an existing media object. First look in the album's children. If not there, search the hash
				// keys - maybe it was moved from another directory.
				foreach (IGalleryObject existingMediaObject in album.GetChildGalleryObjects(GalleryObjectType.MediaObject))
				{
					if (existingMediaObject.Original.FileNamePhysicalPath.Equals(file.FullName, StringComparison.OrdinalIgnoreCase))
					{
						mediaObject = existingMediaObject;
						break;
					}
				}

				if ((mediaObject != null) || (_mediaObjectsFromDataStore.TryGetValue(HelperFunctions.GetHashKey(file), out mediaObject)))
				{
					// Found an existing media object matching the file on disk. Update properties, but only if its file extension
					// is enabled. (If this is a media object that had been added to Gallery Server but its file type was 
					// subsequently disabled, we do not want to synchronize it - we want its info in the data store to be deleted.)
					if (HelperFunctions.IsFileAuthorizedForAddingToGallery(file.Name, album.GalleryId))
					{
						UpdateExistingMediaObject(album, mediaObject);
					}
				}
				else
				{
					// No media object exists for this file. Create a new one.
					CreateNewMediaObject(album, file);
				}

				int newFileIndex = this._synchStatus.CurrentFileIndex + 1;
				if (newFileIndex < this._synchStatus.TotalFileCount)
				{
					UpdateStatus(newFileIndex, file.DirectoryName, file.Name);
				}

				lock (this._synchStatus)
				{
					if (this._synchStatus.ShouldTerminate)
					{
						// Immediately set this property back to false so that we don't trigger this code again, then throw a special exception
						// that will be caught and used to cancel the synch.
						this._synchStatus.Update(SynchronizationState.NotSet, null, null, null, null, false, false);
						throw new SynchronizationTerminationRequestedException();
					}
				}
			}
		}

		private void UpdateStatus(int currentFileIndex, string filepath, string filename)
		{
			string currentFilePath = filepath.Remove(0, _fullMediaObjectPathLength).TrimStart(new char[] { Path.DirectorySeparatorChar });

			this._synchStatus.Update(SynchronizationState.NotSet, null, filename, currentFileIndex, currentFilePath, null, false);

		}

		private void CreateNewMediaObject(IAlbum album, FileInfo file)
		{
			try
			{
				using (IGalleryObject mediaObject = Factory.CreateMediaObjectInstance(file, album))
				{
					HelperFunctions.UpdateAuditFields(mediaObject, this._userName);
					mediaObject.Save();

					if (!GallerySettings.MediaObjectPathIsReadOnly && (GallerySettings.DiscardOriginalImageDuringImport))
					{
						mediaObject.DeleteOriginalFile();
						mediaObject.Save();
					}

					mediaObject.IsSynchronized = true;
				}
			}
			catch (UnsupportedMediaObjectTypeException)
			{
				this._synchStatus.SkippedMediaObjects.Add(new KeyValuePair<string, string>(file.FullName.Remove(0, _fullMediaObjectPathLength + 1), Resources.SynchronizationStatus_Disabled_File_Type_Msg));
			}
		}

		private void UpdateExistingMediaObject(IAlbum album, IGalleryObject mediaObject)
		{
			if (mediaObject.Parent.Id != album.Id)
			{
				mediaObject.Parent = album;
			}

			// If the generated hash key is the same as the one already assigned, then do nothing. Otherwise,
			// generate a guaranteed unique hash key with the GetHashKeyUnique function.
			if (mediaObject.Hashkey != HelperFunctions.GetHashKey(mediaObject.Original.FileInfo))
			{
				mediaObject.Hashkey = HelperFunctions.GetHashKeyUnique(mediaObject.Original.FileInfo);
			}

			// Check for existence of thumbnail.
			if (!File.Exists(mediaObject.Thumbnail.FileNamePhysicalPath))
			{
				mediaObject.RegenerateThumbnailOnSave = true;
			}

			Image image = mediaObject as Image;
			if (image != null)
			{
				EvaluateOriginalImage(image);

				EvaluateOptimizedImage(image);
			}
			else
			{
				UpdateNonImageWidthAndHeight(mediaObject);
			}

			HelperFunctions.UpdateAuditFields(mediaObject, this._userName);
			mediaObject.Save();
			mediaObject.IsSynchronized = true;
		}

		private void SynchronizeExternalMediaObjects(IAlbum album)
		{
			foreach (IGalleryObject mediaObject in album.GetChildGalleryObjects(GalleryObjectType.External))
			{
				// Check for existence of thumbnail.
				if (this.RebuildThumbnail || !File.Exists(mediaObject.Thumbnail.FileNamePhysicalPath))
				{
					mediaObject.RegenerateThumbnailOnSave = true;
					HelperFunctions.UpdateAuditFields(mediaObject, this._userName);
					mediaObject.Save();
					mediaObject.IsSynchronized = true;
				}
			}
		}

		/// <summary>
		/// Find, or create if necessary, the album corresponding to the specified directory and set it as the 
		/// child of the parentAlbum parameter.
		/// </summary>
		/// <param name="directory">The directory for which to obtain a matching album object.</param>
		/// <param name="parentAlbum">The album that contains the album at the specified directory.</param>
		/// <returns>Returns an album object corresponding to the specified directory and having the specified
		/// parent album.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="directory" /> or <paramref name="parentAlbum" /> is null.</exception>
		/// <exception cref="ArgumentException">Thrown when </exception>
		/// <exception cref="ArgumentException">Thrown when the full directory path of the parent of <paramref name="directory" /> does not match the 
		/// directory path of <paramref name="parentAlbum" />.</exception>
		private IAlbum SynchronizeDirectory(DirectoryInfo directory, IAlbum parentAlbum)
		{
			#region Parameter validation

			if (directory == null)
				throw new ArgumentNullException("directory");

			if (parentAlbum == null)
				throw new ArgumentNullException("parentAlbum");

			if (directory.Parent.FullName != parentAlbum.FullPhysicalPathOnDisk.TrimEnd(new char[] { Path.DirectorySeparatorChar }))
				throw new ArgumentException("Error in SynchronizeDirectory().");

			#endregion

			IAlbum childAlbum = null;
			try
			{
				if (this._albumsFromDataStore.TryGetValue(directory.FullName, out childAlbum))
				{
					// Found the album. Update properties.
					childAlbum.IsSynchronized = true;
					childAlbum.IsPrivate = (parentAlbum.IsPrivate ? true : childAlbum.IsPrivate); // Only set to private if parent is private
					childAlbum.RegenerateThumbnailOnSave = this.RebuildThumbnail;
				}
				else
				{
					// No album exists for this directory. Create a new one.
					childAlbum = Factory.CreateEmptyAlbumInstance(parentAlbum.GalleryId);
					childAlbum.Parent = parentAlbum;

					string directoryName = directory.Name;
					childAlbum.Title = directoryName;
					//childAlbum.ThumbnailMediaObjectId = 0; // not needed
					childAlbum.DirectoryName = directoryName;
					childAlbum.FullPhysicalPathOnDisk = Path.Combine(parentAlbum.FullPhysicalPathOnDisk, directoryName);
					childAlbum.IsPrivate = parentAlbum.IsPrivate;
				}

				if (childAlbum.IsNew || childAlbum.HasChanges)
				{
					HelperFunctions.UpdateAuditFields(childAlbum, this._userName);
					childAlbum.Save();
				}

				// Commit the transaction to the database for every 100 media objects that are processed.
				if ((this._synchStatus.CurrentFileIndex - this._lastTransactionCommitFileIndex) >= 100)
				{
					HelperFunctions.CommitTransaction();
					HelperFunctions.BeginTransaction();
					this._lastTransactionCommitFileIndex = this._synchStatus.CurrentFileIndex;
				}
			}
			catch
			{
				if (childAlbum != null)
					childAlbum.Dispose();

				throw;
			}

			return childAlbum;
		}

		private void DeleteUnsynchronizedObjects()
		{
			// Save each synchronized object, and delete each unsynchronized object.
			this._synchStatus.Update(SynchronizationState.PersistingToDataStore, null, null, null, null, null, false);

			// Delete unsynchronized albums.
			foreach (KeyValuePair<String, IAlbum> albumKeyValue in this._albumsFromDataStore)
			{
				IAlbum album = albumKeyValue.Value;

				if (!album.IsSynchronized)
				{
					album.DeleteFromGallery();
				}
			}

			// Delete unsynchronized  media objects.
			foreach (KeyValuePair<String, IGalleryObject> mediaObjectKeyValue in this._mediaObjectsFromDataStore)
			{
				IGalleryObject galleryObject = mediaObjectKeyValue.Value;

				if (!galleryObject.IsSynchronized)
				{
					galleryObject.DeleteFromGallery();
				}
			}
		}

		/// <summary>
		/// Delete any thumbnail and optimized images that do not have matching media objects.
		/// This can occur when a user manually transfers (e.g. uses Windows Explorer)
		/// original images to a new directory and leaves the thumbnail and optimized
		/// images in the original directory or when a user deletes the original media file in 
		/// Explorer. This function *only* deletes files that begin the the thumbnail and optimized
		/// prefix (e.g. zThumb_, zOpt_).
		/// </summary>
		/// <param name="album">The album whose directory is to be processed for orphaned image files.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="album" /> is null.</exception>
		private void DeleteOrphanedImages(IAlbum album)
		{
			if (album == null)
				throw new ArgumentNullException("album");

			// STEP 1: Get list of directories that may contain thumbnail or optimized images for the current album
			string originalPath = album.FullPhysicalPathOnDisk;
			string thumbnailPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(album.FullPhysicalPathOnDisk, GallerySettings.FullThumbnailPath, GallerySettings.FullMediaObjectPath);
			string optimizedPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(album.FullPhysicalPathOnDisk, GallerySettings.FullOptimizedPath, GallerySettings.FullMediaObjectPath);

			List<string> albumPaths = new List<string>(3);

			// The original path may contain thumbnails or optimized images when the thumbnail/optimized path is the same as the original path
			if ((GallerySettings.FullThumbnailPath.Equals(GallerySettings.FullMediaObjectPath, StringComparison.OrdinalIgnoreCase)) ||
				(GallerySettings.FullOptimizedPath.Equals(GallerySettings.FullMediaObjectPath, StringComparison.OrdinalIgnoreCase)))
			{
				albumPaths.Add(originalPath);
			}

			if (!albumPaths.Contains(thumbnailPath))
				albumPaths.Add(thumbnailPath);

			if (!albumPaths.Contains(optimizedPath))
				albumPaths.Add(optimizedPath);


			string thumbnailPrefix = GallerySettings.ThumbnailFileNamePrefix;
			string optimizedPrefix = GallerySettings.OptimizedFileNamePrefix;

			IGalleryObjectCollection mediaObjects = album.GetChildGalleryObjects(GalleryObjectType.MediaObject);

			// STEP 2: Loop through each path and make sure all thumbnail and optimized files in each directory have 
			// matching media objects. Delete any files that do not.
			foreach (string albumPath in albumPaths)
			{
				if (!Directory.Exists(albumPath))
					return;

				DirectoryInfo directory = new DirectoryInfo(albumPath);

				// Loop through each file in the directory.
				FileInfo[] files;
				try
				{
					files = directory.GetFiles();
				}
				catch (UnauthorizedAccessException)
				{
					return;
				}

				foreach (FileInfo file in files)
				{
					if ((file.Name.StartsWith(thumbnailPrefix, StringComparison.OrdinalIgnoreCase)) || (file.Name.StartsWith(optimizedPrefix, StringComparison.OrdinalIgnoreCase)))
					{
						// This file is a thumbnail or optimized file. Check to see if any media object in this album
						// refers to it.
						bool foundMediaObject = false;
						foreach (IGalleryObject mediaObject in mediaObjects)
						{
							if ((mediaObject.Optimized.FileName.Equals(file.Name, StringComparison.OrdinalIgnoreCase)) ||
								(mediaObject.Thumbnail.FileName.Equals(file.Name, StringComparison.OrdinalIgnoreCase)))
							{
								foundMediaObject = true;
								break;
							}
						}

						if (!foundMediaObject)
						{
							// No media object in this album refers to this thumbnail or optimized image. Smoke it!
							try
							{
								file.Delete();
							}
							catch (IOException ex)
							{
								// An exception occurred, probably because the account ASP.NET is running under does not
								// have permission to delete the file. Let's record the error, but otherwise ignore it.
								Error.Record(ex, this._galleryId, Factory.LoadGallerySettings(), AppSetting.Instance);
							}
							catch (System.Security.SecurityException ex)
							{
								// An exception occurred, probably because the account ASP.NET is running under does not
								// have permission to delete the file. Let's record the error, but otherwise ignore it.
								Error.Record(ex, this._galleryId, Factory.LoadGallerySettings(), AppSetting.Instance);
							}
							catch (UnauthorizedAccessException ex)
							{
								// An exception occurred, probably because the account ASP.NET is running under does not
								// have permission to delete the file. Let's record the error, but otherwise ignore it.
								Error.Record(ex, this._galleryId, Factory.LoadGallerySettings(), AppSetting.Instance);
							}
						}
					}
				}
			}

			// Now recursively loop through any child albums.
			IGalleryObjectCollection childAlbums = album.GetChildGalleryObjects(GalleryObjectType.Album);
			if (this.IsRecursive)
			{
				foreach (IAlbum childAlbum in childAlbums)
				{
					DeleteOrphanedImages(childAlbum);
				}
			}
		}

		private bool DoesOriginalExceedOptimizedTriggers(IGalleryObject mediaObject)
		{
			// Note: This function also exists in the ImageOptimizedCreator class.

			// Test 1: Is the file size of the original greater than OptimizedImageTriggerSizeKB?
			bool isOriginalFileSizeGreaterThanTriggerSize = false;

			if (mediaObject.Original.FileSizeKB > _optimizedTriggerSizeKb)
			{
				isOriginalFileSizeGreaterThanTriggerSize = true;
			}

			// Test 2: Is the width or length of the original greater than the MaxOptimizedLength?
			bool isOriginalLengthGreaterThanMaxAllowedLength = false;

			int originalWidth = 0;
			int originalHeight = 0;
			try
			{
				originalWidth = mediaObject.Original.Bitmap.Width;
				originalHeight = mediaObject.Original.Bitmap.Height;
			}
			catch (UnsupportedImageTypeException ex)
			{
				Error.Record(ex, this._galleryId, Factory.LoadGallerySettings(), AppSetting.Instance);
			}

			if ((originalWidth > _optimizedMaxLength) || (originalHeight > _optimizedMaxLength))
			{
				isOriginalLengthGreaterThanMaxAllowedLength = true;
			}

			return (isOriginalFileSizeGreaterThanTriggerSize | isOriginalLengthGreaterThanMaxAllowedLength);
		}

		/// <summary>
		/// If the rebuild thumbnail or rebuild image options are selected, then get the latest statistics about the 
		/// original image. Perhaps the user edited the object (such as rotating) in another program.
		/// </summary>
		/// <param name="mediaObject">The media object whose original image is to be checked.</param>
		private void EvaluateOriginalImage(Image mediaObject)
		{
			if (mediaObject == null)
				return;

			if (_rebuildThumbnail || _rebuildOptimized)
			{
				try
				{
					mediaObject.Original.Width = mediaObject.Original.Bitmap.Width;
					mediaObject.Original.Height = mediaObject.Original.Bitmap.Height;
				}
				catch (UnsupportedImageTypeException) { }

				int fileSize = (int)(mediaObject.Original.FileInfo.Length / 1024);
				mediaObject.Original.FileSizeKB = (fileSize < 1 ? 1 : fileSize); // Very small files should be 1, not 0.
			}
		}

		/// <summary>
		/// Check that the optimized image exists. <paramref name="mediaObject"/> *must* be an <see cref="Image"/> type.
		/// If "overwrite compressed" option is selected, also check whether it the optimized version is really needed.
		/// </summary>
		/// <param name="mediaObject">The media object whose optimized image is to be checked.</param>
		/// <remarks>Note that the ValidateSave() method in the GalleryObject class also checks for the existence of 
		/// the thumbnail and optimized images. However, we need to do it here because the UpdateAuditFields method
		/// that is called after this function is executed updates the audit fields only when HasChanges = true. If 
		/// we don't check for these images, then the media object might have HasChanges = false, which causes the 
		/// audit fields to remain unchanged. But then if ValidateSave updates them, we'll get an error because the 
		/// GalleryObject class doesn't update the audit fields (it knows nothing about the current user.)</remarks>
		private void EvaluateOptimizedImage(Image mediaObject)
		{
			if (mediaObject == null)
				return;

			// Check for existence of optimized image.
			if (!File.Exists(mediaObject.Optimized.FileNamePhysicalPath))
			{
				// Optimized image doesn't exist, but maybe we don't need it anyway. Check for this possibility.
				if (DoesOriginalExceedOptimizedTriggers(mediaObject))
				{
					mediaObject.RegenerateOptimizedOnSave = true; // Yup, we need to generate the opt. image.
				}
				else
				{
					// The original isn't big enough to need an optimized image, so make sure the optimized properties
					// are the same as the original's properties.
					mediaObject.Optimized.FileName = mediaObject.Original.FileName;
					mediaObject.Optimized.Width = mediaObject.Original.Width;
					mediaObject.Optimized.Height = mediaObject.Original.Height;
					mediaObject.Optimized.FileSizeKB = mediaObject.Original.FileSizeKB;
				}
			}
			else
			{
				// We have an image where the optimized image exists. But perhaps the user changed some optimized trigger settings
				// and we no longer need the optimized image. Check for this possibility, and if true, update the optimized properties
				// to be the same as the original. Note: We only check if user selected the "overwrite compresssed" option - this is 
				// because checking the dimensions of an image is very resource intensive, so we'll only do this if necessary.
				if (this.RebuildOptimized && !DoesOriginalExceedOptimizedTriggers(mediaObject))
				{
					mediaObject.Optimized.FileName = mediaObject.Original.FileName;
					mediaObject.Optimized.Width = mediaObject.Original.Width;
					mediaObject.Optimized.Height = mediaObject.Original.Height;
					mediaObject.Optimized.FileSizeKB = mediaObject.Original.FileSizeKB;
				}
			}
		}

		#endregion

		#region Private Static Methods

		/// <summary>
		/// Update the width and height values to the default values specified for audio, video, and generic objects.
		/// This method has no effect on <see cref="Image"/> or <see cref="ExternalMediaObject"/> objects.
		/// </summary>
		/// <param name="mediaObject">The <see cref="IGalleryObject"/> whose <see cref="DisplayObject.Width"/> and 
		/// <see cref="DisplayObject.Height"/> properties of the <see cref="IGalleryObject.Original"/> property is to be 
		/// updated with the current default values.</param>
		private void UpdateNonImageWidthAndHeight(IGalleryObject mediaObject)
		{
			if (mediaObject is Video)
			{
				mediaObject.Original.Width = GallerySettings.DefaultVideoPlayerWidth;
				mediaObject.Original.Height = GallerySettings.DefaultVideoPlayerHeight;
			}
			else if ((mediaObject is GenericMediaObject) && (mediaObject.MimeType.TypeCategory == MimeTypeCategory.Other))
			{
				// We want to update the width and height only when the TypeCategory is Other. If we don't check for this, we might
				// assign a width and height to a corrupt JPG that is being treated as a GenericMediaObject.
				mediaObject.Original.Width = GallerySettings.DefaultGenericObjectWidth;
				mediaObject.Original.Height = GallerySettings.DefaultGenericObjectHeight;
			}
			else if (mediaObject is Audio)
			{
				mediaObject.Original.Width = GallerySettings.DefaultAudioPlayerWidth;
				mediaObject.Original.Height = GallerySettings.DefaultAudioPlayerHeight;
			}
		}

		#endregion
	}
}
