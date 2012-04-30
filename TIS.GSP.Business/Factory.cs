using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.Business.Metadata;
using GalleryServerPro.Business.NullObjects;
using GalleryServerPro.Business.Properties;
using GalleryServerPro.ErrorHandler;
using GalleryServerPro.ErrorHandler.CustomExceptions;
using GalleryServerPro.Provider;
using GalleryServerPro.Provider.Interfaces;
using GalleryServerPro.Data;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// Contains functionality for creating and retrieving various business objects. Use methods in this class instead of instantiating
	/// certain objects directly. This includes instances of <see cref="Image" />, <see cref="Video" />, <see cref="Audio" />, 
	/// <see cref="GenericMediaObject" />, and <see cref="Album" />.
	/// </summary>
	public class Factory : IFactory
	{
		#region Private Fields

		private static readonly object _sharedLock = new object();
		private static readonly Dictionary<int, ISynchronizationStatus> _syncStatuses = new Dictionary<int, ISynchronizationStatus>(1);
		private static readonly IGalleryCollection _galleries = new GalleryCollection();
		private static readonly Dictionary<int, Watermark> _watermarks = new Dictionary<int, Watermark>(1);
		private static readonly IGallerySettingsCollection _gallerySettings = new GallerySettingsCollection();
		private static readonly IBrowserTemplateCollection _browserTemplates = new BrowserTemplateCollection();

		private static IGalleryControlSettingsCollection _galleryControlSettings;

		#endregion

		#region Gallery Object Methods

		/// <overloads>Create a fully inflated, properly typed gallery object instance based on the specified parameters.</overloads>
		/// <summary>
		/// Create a fully inflated, properly typed instance based on the specified <see cref="IGalleryObject.Id">ID</see>. An 
		/// additional call to the data store is made to determine the object's type. When you know the type you want (<see cref="Album" />,
		/// <see cref="Image" />, etc), use the overload that takes the galleryObjectType parameter, or call the specific Factory method that 
		/// loads the desired type, as those are more efficient. This method is guaranteed to not return null. If no object is found
		/// that matches the ID, an <see cref="UnsupportedMediaObjectTypeException" /> exception is thrown. If both a media object and an 
		/// album exist with the <paramref name = "id" />, the media object reference is returned.
		/// </summary>
		/// <param name="id">An integer representing the <see cref="IGalleryObject.Id">ID</see> of the media object or album to retrieve from the
		/// data store.</param>
		/// <returns>Returns an <see cref="IGalleryObject" /> object for the <see cref="IGalleryObject.Id">ID</see>. This method is guaranteed to not
		/// return null.</returns>
		/// <exception cref="UnsupportedMediaObjectTypeException">Thrown when no media object with the specified <see cref="IGalleryObject.Id">ID</see> 
		/// is found in the data store.</exception>
		public static IGalleryObject LoadGalleryObjectInstance(int id)
		{
			// Figure out what type the ID refers to (album, image, video, etc) and then call the overload of this method.
			return LoadGalleryObjectInstance(id, HelperFunctions.DetermineGalleryObjectType(id));
		}

		/// <summary>
		/// Create a fully inflated, properly typed instance based on the specified parameters. If the galleryObjectType
		/// parameter is All, None, or Unknown, then an additional call to the data store is made
		/// to determine the object's type. If no object is found that matches the ID and gallery object type, an 
		/// <see cref="UnsupportedMediaObjectTypeException" /> exception is thrown. When you know the type you want (<see cref="Album" />,
		/// <see cref="Image" />, etc), specify the exact galleryObjectType, or call the specific Factory method that 
		/// loads the desired type, as that is more efficient. This method is guaranteed to not return null.
		/// </summary>
		/// <param name="id">An integer representing the <see cref="IGalleryObject.Id">ID</see> of the media object or album to retrieve from the
		/// data store.</param>
		/// <param name="galleryObjectType">The type of gallery object that the id parameter represents. If the type is 
		/// unknown, the Unknown enum value can be specified. Specify the actual type if possible (e.g. Video, Audio, Image, 
		/// etc.), as it is more efficient.</param>
		/// <returns>Returns an <see cref="IGalleryObject" /> based on the ID. This method is guaranteed to not return null.</returns>
		/// <exception cref="UnsupportedMediaObjectTypeException">Thrown when a particular media object type is requested (e.g. Image, Video, etc.), 
		/// but no media object with the specified ID is found in the data store.</exception>
		/// <exception cref="InvalidAlbumException">Thrown when an album is requested but no album with the specified ID is found in the data store.</exception>
		public static IGalleryObject LoadGalleryObjectInstance(int id, GalleryObjectType galleryObjectType)
		{
			// If the gallery object type is vague, we need to figure it out.
			if ((galleryObjectType == GalleryObjectType.All) || (galleryObjectType == GalleryObjectType.None) || (galleryObjectType == GalleryObjectType.Unknown))
			{
				galleryObjectType = HelperFunctions.DetermineGalleryObjectType(id);
			}

			IGalleryObject go = null;
			try
			{
				switch (galleryObjectType)
				{
					case GalleryObjectType.Album:
						{
							go = LoadAlbumInstance(id, false);
							break;
						}
					case GalleryObjectType.Image:
						{
							go = LoadImageInstance(id);
							break;
						}
					case GalleryObjectType.Video:
						{
							go = LoadVideoInstance(id);
							break;
						}
					case GalleryObjectType.Audio:
						{
							go = LoadAudioInstance(id);
							break;
						}
					case GalleryObjectType.Generic:
					case GalleryObjectType.Unknown:
						{
							go = LoadGenericMediaObjectInstance(id);
							break;
						}
					default:
						{
							throw new UnsupportedMediaObjectTypeException();
						}
				}
			}
			catch
			{
				if (go != null)
					go.Dispose();

				throw;
			}

			return go;
		}

		#endregion

		#region Media Object Methods

		#region General Media Object Methods

		/// <overloads>
		/// Create a properly typed Gallery Object instance (e.g. <see cref="Image" />, <see cref="Video" />, etc.) from the specified parameters.
		/// </overloads>
		/// <summary>
		/// Create a properly typed Gallery Object instance (e.g. <see cref="Image" />, <see cref="Video" />, etc.) for the media file
		/// represented by <paramref name = "mediaObjectFilePath" /> and belonging to the album specified by <paramref name = "parentAlbum" />.
		/// </summary>
		/// <param name="mediaObjectFilePath">The fully qualified name of the media object file, or the relative filename.
		/// The file must already exist in the album's directory. If the file has a matching record in the data store,
		/// a reference to the existing object is returned. Otherwise, a new instance is returned. For new instances,
		/// call <see cref="IGalleryObject.Save" /> to persist the object to the data store. A
		/// <see cref="UnsupportedMediaObjectTypeException" /> is thrown when the specified file cannot 
		/// be added to Gallery Server, perhaps because it is an unsupported type or the file is corrupt.</param>
		/// <param name="parentAlbum">The album in which the media object exists (for media objects that already exist
		/// in the data store), or should be added to (for new media objects which need to be inserted into the 
		/// data store).</param>
		/// <returns>Returns a properly typed Gallery Object instance corresponding to the specified parameters.</returns>
		/// <exception cref="UnsupportedMediaObjectTypeException">Thrown when <paramref name = "mediaObjectFilePath" /> has a file 
		/// extension that Gallery Server Pro is configured to reject.</exception>
		/// <exception cref="InvalidMediaObjectException">Thrown when the  
		/// mediaObjectFilePath parameter refers to a file that is not in the same directory as the parent album's directory.</exception>
		public static IGalleryObject CreateMediaObjectInstance(string mediaObjectFilePath, IAlbum parentAlbum)
		{
			return CreateMediaObjectInstance(new FileInfo(mediaObjectFilePath), parentAlbum);
		}

		/// <summary>
		/// Create a properly typed Gallery Object instance (e.g. <see cref="Image" />, <see cref="Video" />, etc.) for the media file
		/// represented by <paramref name = "mediaObjectFile" /> and belonging to the album specified by <paramref name = "parentAlbum" />.
		/// </summary>
		/// <param name="mediaObjectFile">A <see cref="System.IO.FileInfo" /> object representing a supported media object type. The file must already
		/// exist in the album's directory. If the file has a matching record in the data store, a reference to the existing 
		/// object is returned; otherwise, a new instance is returned. For new instances, call <see cref="IGalleryObject.Save" /> 
		///		to persist the object to the data store. A <see cref="UnsupportedMediaObjectTypeException" /> is thrown when the specified file cannot 
		/// be added to Gallery Server, perhaps because it is an unsupported type or the file is corrupt.</param>
		/// <param name="parentAlbum">The album in which the media object exists (for media objects that already exist
		/// in the data store), or should be added to (for new media objects which need to be inserted into the 
		/// data store).</param>
		/// <returns>Returns a properly typed Gallery Object instance corresponding to the specified parameters.</returns>
		/// <exception cref="UnsupportedMediaObjectTypeException">Thrown when <paramref name = "mediaObjectFile" /> has a file 
		/// extension that Gallery Server Pro is configured to reject.</exception>
		/// <exception cref="InvalidMediaObjectException">Thrown when   
		/// <paramref name = "mediaObjectFile" /> refers to a file that is not in the same directory as the parent album's directory.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name = "parentAlbum" /> is null.</exception>
		/// <remarks>
		/// This method is marked internal to ensure it is not called from the web layer. It was noticed that
		/// calling this method from the web layer caused the file referenced in the mediaObjectFile parameter to remain
		/// locked beyond the conclusion of the page lifecycle, preventing manual deletion using Windows Explorer. Note 
		/// that restarting IIS (iisreset.exe) released the file lock, and presumably the next garbage collection would 
		/// have released it as well. The web page was modified to call the overload of this method that takes the filepath
		/// as a string parameter and then instantiates a <see cref="System.IO.FileInfo" /> object. I am not sure why, 
		/// but instantiating the <see cref="System.IO.FileInfo" /> object within this DLL in this way caused the file 
		/// lock to be released at the end of the page lifecycle.
		/// </remarks>
		internal static IGalleryObject CreateMediaObjectInstance(FileInfo mediaObjectFile, IAlbum parentAlbum)
		{
			return CreateMediaObjectInstance(mediaObjectFile, parentAlbum, String.Empty, MimeTypeCategory.NotSet);
		}

		/// <summary>
		/// Create a properly typed Gallery Object instance (e.g. <see cref="Image" />, <see cref="Video" />, etc.). If 
		/// <paramref name = "externalHtmlSource" /> is specified, then an <see cref="ExternalMediaObject" /> is created with the
		/// specified <paramref name = "mimeTypeCategory" />; otherwise a new instance is created based on <paramref name = "mediaObjectFile" />,
		/// where the exact type (e.g. <see cref="Image" />, <see cref="Video" />, etc.) is determined by the file's extension.
		/// </summary>
		/// <param name="mediaObjectFile">A <see cref="System.IO.FileInfo" /> object representing a supported media object type. The file must already
		/// exist in the album's directory. If the file has a matching record in the data store, a reference to the existing 
		/// object is returned; otherwise, a new instance is returned. For new instances, call <see cref="IGalleryObject.Save" /> to 
		///		persist the object to the data store. A <see cref="UnsupportedMediaObjectTypeException" /> is thrown when the specified file cannot 
		/// be added to Gallery Server, perhaps because it is an unsupported type or the file is corrupt. Do not specify this parameter
		/// when using the <paramref name = "externalHtmlSource" /> parameter.</param>
		/// <param name="parentAlbum">The album in which the media object exists (for media objects that already exist
		/// in the data store), or should be added to (for new media objects which need to be inserted into the data store).</param>
		/// <param name="externalHtmlSource">The HTML that defines an externally stored media object, such as one hosted at 
		/// Silverlight.net or youtube.com. Using this parameter also requires specifying <paramref name = "mimeTypeCategory" />
		/// and passing null for <paramref name = "mediaObjectFile" />.</param>
		/// <param name="mimeTypeCategory">Specifies the category to which an externally stored media object belongs. 
		/// Must be set to a value other than MimeTypeCategory.NotSet when the <paramref name = "externalHtmlSource" /> is specified.</param>
		/// <returns>Returns a properly typed Gallery Object instance corresponding to the specified parameters.</returns>
		/// <exception cref="ArgumentException">Thrown when <paramref name = "mediaObjectFile" /> and <paramref name = "externalHtmlSource" />
		/// are either both specified, or neither.</exception>
		/// <exception cref="UnsupportedMediaObjectTypeException">Thrown when <paramref name = "mediaObjectFile" /> has a file 
		/// extension that Gallery Server Pro is configured to reject.</exception>
		/// <exception cref="InvalidMediaObjectException">Thrown when the  
		/// mediaObjectFile parameter refers to a file that is not in the same directory as the parent album's directory.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name = "parentAlbum" /> is null.</exception>
		/// <remarks>
		/// This method is marked internal to ensure it is not called from the web layer. It was noticed that
		/// calling this method from the web layer caused the file referenced in the mediaObjectFile parameter to remain
		/// locked beyond the conclusion of the page lifecycle, preventing manual deletion using Windows Explorer. Note 
		/// that restarting IIS (iisreset.exe) released the file lock, and presumably the next garbage collection would 
		/// have released it as well. The web page was modified to call the overload of this method that takes the filepath
		/// as a string parameter and then instantiates a FileInfo object. I am not sure why, but instantiating the FileInfo 
		/// object within this DLL in this way caused the file lock to be released at the end of the page lifecycle.
		/// </remarks>
		internal static IGalleryObject CreateMediaObjectInstance(FileInfo mediaObjectFile, IAlbum parentAlbum, string externalHtmlSource, MimeTypeCategory mimeTypeCategory)
		{
			#region Validation

			// Either mediaObjectFile or externalHtmlSource must be specified, but not both.
			if ((mediaObjectFile == null) && (String.IsNullOrEmpty(externalHtmlSource)))
				throw new ArgumentException("The method GalleryServerPro.Business.Factory.CreateMediaObjectInstance was invoked with invalid parameters. The parameters mediaObjectFile and externalHtmlSource cannot both be null or empty. One of these - but not both - must be populated.");

			if ((mediaObjectFile != null) && (!String.IsNullOrEmpty(externalHtmlSource)))
				throw new ArgumentException("The method GalleryServerPro.Business.Factory.CreateMediaObjectInstance was invoked with invalid parameters. The parameters mediaObjectFile and externalHtmlSource cannot both be specified.");

			if ((!String.IsNullOrEmpty(externalHtmlSource)) && (mimeTypeCategory == MimeTypeCategory.NotSet))
				throw new ArgumentException("The method GalleryServerPro.Business.Factory.CreateMediaObjectInstance was invoked with invalid parameters. The parameters mimeTypeCategory must be set to a value other than MimeTypeCategory.NotSet when the externalHtmlSource parameter is specified.");

			if (parentAlbum == null)
				throw new ArgumentNullException("parentAlbum");

			#endregion

			if (String.IsNullOrEmpty(externalHtmlSource))
				return CreateLocalMediaObjectInstance(mediaObjectFile, parentAlbum);
			else
				return CreateExternalMediaObjectInstance(externalHtmlSource, mimeTypeCategory, parentAlbum);
		}

		/// <summary>
		/// Create a properly typed Gallery Object instance (e.g. <see cref="Image" />, <see cref="Video" />, etc.) from the specified parameters.
		/// </summary>
		/// <param name="mediaObjectFile">A <see cref="System.IO.FileInfo" /> object representing a supported media object type. The file must already
		/// exist in the album's directory. If the file has a matching record in the data store, a reference to the existing 
		/// object is returned; otherwise, a new instance is returned. For new instances, call <see cref="IGalleryObject.Save" /> 
		///		to persist the object to the data store.</param>
		/// <param name="parentAlbum">The album in which the media object exists (for media objects that already exist
		/// in the data store), or should be added to (for new media objects which need to be inserted into the 
		/// data store).</param>
		/// <returns>Returns a properly typed Gallery Object instance corresponding to the specified parameters.</returns>
		/// <exception cref="UnsupportedMediaObjectTypeException">Thrown when <paramref name = "mediaObjectFile" /> has a file 
		/// extension that Gallery Server Pro is configured to reject.</exception>
		/// <exception cref="InvalidMediaObjectException">Thrown when   
		/// <paramref name = "mediaObjectFile" /> refers to a file that is not in the same directory as the parent album's directory.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name = "mediaObjectFile" /> or <paramref name = "parentAlbum" /> is null.</exception>
		private static IGalleryObject CreateLocalMediaObjectInstance(FileInfo mediaObjectFile, IAlbum parentAlbum)
		{
			if (mediaObjectFile == null)
				throw new ArgumentNullException("mediaObjectFile");

			if (parentAlbum == null)
				throw new ArgumentNullException("parentAlbum");

			IGalleryObject go;

			GalleryObjectType goType = HelperFunctions.DetermineMediaObjectType(mediaObjectFile.Name);

			if (goType == GalleryObjectType.Unknown)
			{
				bool allowUnspecifiedMimeTypes = LoadGallerySetting(parentAlbum.GalleryId).AllowUnspecifiedMimeTypes;
				// If we have an unrecognized media object type (because no MIME type element exists in the configuration
				// file that matches the file extension), then treat the object as a generic media object, but only if
				// the "allowUnspecifiedMimeTypes" configuration setting allows adding unknown media object types.
				// If allowUnspecifiedMimeTypes = false, goType remains "Unknown", and we'll be throwing an 
				// UnsupportedMediaObjectTypeException at the end of this method.
				if (allowUnspecifiedMimeTypes)
				{
					goType = GalleryObjectType.Generic;
				}
			}

			switch (goType)
			{
				case GalleryObjectType.Image:
					{
						try
						{
							go = CreateImageInstance(mediaObjectFile, parentAlbum);
							break;
						}
						catch (UnsupportedImageTypeException)
						{
							go = CreateGenericObjectInstance(mediaObjectFile, parentAlbum);
							break;
						}
					}
				case GalleryObjectType.Video:
					{
						go = CreateVideoInstance(mediaObjectFile, parentAlbum);
						break;
					}
				case GalleryObjectType.Audio:
					{
						go = CreateAudioInstance(mediaObjectFile, parentAlbum);
						break;
					}
				case GalleryObjectType.Generic:
					{
						go = CreateGenericObjectInstance(mediaObjectFile, parentAlbum);
						break;
					}
				default:
					{
						throw new UnsupportedMediaObjectTypeException(mediaObjectFile);
					}
			}

			return go;
		}

		/// <overloads>
		/// Create a fully inflated, properly typed media object instance.
		/// </overloads>
		/// <summary>
		/// Create a read-only, fully inflated, properly typed media object instance from the specified <paramref name="id" />.
		/// If <paramref name="id" /> is an image, video, audio, etc, then the appropriate object is returned. 
		/// An exception is thrown if the <paramref name="id" /> refers to an <see cref="Album" /> (use the 
		/// <see cref="LoadGalleryObjectInstance(int)" /> or <see cref="LoadAlbumInstance(int, bool)" /> method if
		/// the <paramref name="id" /> refers to an album). An exception is also thrown if no matching record 
		/// exists for this <paramref name="id" />. This overload makes an additional 
		/// call to the data store to determine the object's type. When you know the type you want (<see cref="Image" />, <see cref="Video" />, 
		/// etc), use the overload that takes the galleryObjectType parameter, or call the specific Factory method that 
		/// loads the desired type, as those are more efficient. This method is guaranteed to never return null.
		/// </summary>
		/// <param name="id">An integer representing the <see cref="IGalleryObject.Id">ID</see> of the media object to retrieve
		/// from the data store.</param>
		/// <returns>Returns a read-only, fully inflated, properly typed media object instance.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the id parameter refers to an album. This method 
		/// should be used only for media objects (image, video, audio, etc).</exception>
		/// <exception cref="InvalidMediaObjectException">Thrown when no record exists in the data store for the specified 
		/// <paramref name="id" />, or when the id parameter refers to an album.</exception>
		public static IGalleryObject LoadMediaObjectInstance(int id)
		{
			return LoadMediaObjectInstance(id, GalleryObjectType.Unknown, false);
		}

		/// <summary>
		/// Create a fully inflated, properly typed, optionally updateable media object instance from the specified <paramref name="id" />.
		/// If <paramref name="id" /> is an image, video, audio, etc, then the appropriate object is returned. 
		/// An exception is thrown if the <paramref name="id" /> refers to an <see cref="Album" /> (use the 
		/// <see cref="LoadGalleryObjectInstance(int)" /> or <see cref="LoadAlbumInstance(int, bool)" /> method if
		/// the <paramref name="id" /> refers to an album). An exception is also thrown if no matching record 
		/// exists for this <paramref name="id" />. This overload makes an additional call to the data store 
		/// to determine the object's type. When you know the type you want (<see cref="Image" />, <see cref="Video" />, 
		/// etc), use the overload that takes the galleryObjectType parameter, or call the specific Factory method that 
		/// loads the desired type, as those are more efficient. This method is guaranteed to never return null.
		/// </summary>
		/// <param name="id">An integer representing the <see cref="IGalleryObject.Id">ID</see> of the media object to retrieve
		/// from the data store.</param>
		/// <param name="isWritable">When set to <c>true</c> then return a unique instance that is not shared across threads.
		/// The resulting instance can be modified and persisted to the data store.</param>
		/// <returns>Returns a fully inflated, properly typed media, optionally updateable object instance.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the id parameter refers to an album. This method 
		/// should be used only for media objects (image, video, audio, etc).</exception>
		/// <exception cref="InvalidMediaObjectException">Thrown when no record exists in the data store for the specified 
		/// <paramref name="id" />, or when the id parameter refers to an album.</exception>
		public static IGalleryObject LoadMediaObjectInstance(int id, bool isWritable)
		{
			return LoadMediaObjectInstance(id, GalleryObjectType.Unknown, isWritable);
		}

		/// <summary>
		/// Create a read-only, fully inflated, properly typed media object instance from the specified <paramref name="id" />. If 
		/// <paramref name="id" /> is an image, video, audio, etc, then the appropriate object is returned. An 
		/// exception is thrown if the <paramref name="id" /> refers to an <see cref="Album" /> (use the <see 
		/// cref="LoadGalleryObjectInstance(int)" /> or <see cref="LoadAlbumInstance(int, bool)" /> method if  the <paramref name="id" />
		/// refers to an album). An exception is also thrown if no matching record exists for this <paramref name="id" />. If the 
		/// <paramref name="galleryObjectType" /> parameter is set to All, None, or Unknown, then an additional call to the data store 
		/// is made to determine the object's type. When you know the type you want (<see cref="Image" />, <see cref="Video" />, etc), 
		/// use the overload that takes the galleryObjectType parameter, or call the specific Factory method that loads the desired type, 
		/// as those are more efficient. This method is guaranteed to never return null.
		/// </summary>
		/// <param name="id">An integer representing the <see cref="IGalleryObject.Id">ID</see> of the media object to retrieve
		/// from the data store.</param>
		/// <param name="galleryObjectType">The type of gallery object that the id parameter represents. If the type is 
		/// unknown, the Unknown enum value can be specified. Specify the actual type if possible (e.g. Video, Audio, Image, 
		/// etc.), as it is more efficient. An exception is thrown if the Album enum value is specified, since this method
		/// is designed only for media objects.</param>
		/// <returns>Returns a read-only, fully inflated, properly typed media object instance.</returns>
		/// <exception cref="InvalidMediaObjectException">Thrown when no record exists in the data store for the specified 
		/// <paramref name="id" />, or when the id parameter refers to an album.</exception>
		public static IGalleryObject LoadMediaObjectInstance(int id, GalleryObjectType galleryObjectType)
		{
			return LoadMediaObjectInstance(id, galleryObjectType, false);
		}

		/// <summary>
		/// Create a fully inflated, properly typed, optionally updateable media object instance from the specified <paramref name="id" />. If 
		/// <paramref name="id" /> is an image, video, audio, etc, then the appropriate object is returned. An 
		/// exception is thrown if the <paramref name="id" /> refers to an <see cref="Album" /> (use the <see 
		/// cref="LoadGalleryObjectInstance(int)" /> or <see cref="LoadAlbumInstance(int, bool)" /> method if  the <paramref name="id" />
		/// refers to an album). An exception is also thrown if no matching record exists for this <paramref name="id" />. If the 
		/// <paramref name="galleryObjectType" /> parameter is set to All, None, or Unknown, then an additional call to the data store 
		/// is made to determine the object's type. When you know the type you want (<see cref="Image" />, <see cref="Video" />, etc), 
		/// use the overload that takes the galleryObjectType parameter, or call the specific Factory method that loads the desired type, 
		/// as those are more efficient. This method is guaranteed to never return null.
		/// </summary>
		/// <param name="id">An integer representing the <see cref="IGalleryObject.Id">ID</see> of the media object to retrieve
		/// from the data store.</param>
		/// <param name="galleryObjectType">The type of gallery object that the id parameter represents. If the type is 
		/// unknown, the Unknown enum value can be specified. Specify the actual type if possible (e.g. Video, Audio, Image, 
		/// etc.), as it is more efficient. An exception is thrown if the Album enum value is specified, since this method
		/// is designed only for media objects.</param>
		/// <param name="isWritable">When set to <c>true</c> then return a unique instance that is not shared across threads.
		/// The resulting instance can be modified and persisted to the data store.</param>
		/// <returns>Returns a read-only, fully inflated, properly typed media object instance.</returns>
		/// <exception cref="InvalidMediaObjectException">Thrown when no record exists in the data store for the specified 
		/// <paramref name="id" />, or when the id parameter refers to an album.</exception>
		public static IGalleryObject LoadMediaObjectInstance(int id, GalleryObjectType galleryObjectType, bool isWritable)
		{
			return (isWritable ? RetrieveMediaObjectFromDataStore(id, galleryObjectType, null) : RetrieveMediaObject(id, galleryObjectType));
		}

		/// <summary>
		/// Create a fully inflated, properly typed media object instance based on the specified data record and
		/// belonging to the specified parent album.
		/// </summary>
		/// <param name="moDto">A media object DTO containing information about the media object.</param>
		/// <param name="parentAlbum">The album that contains the media obect to be returned.</param>
		/// <returns>Returns a fully inflated, properly typed media object instance based on the specified data 
		/// record and belonging to the specified parent album.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name = "moDto" /> or <paramref name = "parentAlbum" /> is null.</exception>
		public static IGalleryObject LoadMediaObjectInstance(MediaObjectDto moDto, IAlbum parentAlbum)
		{
			// SQL:
			// SELECT
			//  mo.MediaObjectID, mo.FKAlbumID, mo.Title, mo.HashKey, mo.ThumbnailFilename, mo.ThumbnailWidth, mo.ThumbnailHeight, 
			//  mo.ThumbnailSizeKB, mo.OptimizedFilename, mo.OptimizedWidth, mo.OptimizedHeight, mo.OptimizedSizeKB, 
			//  mo.OriginalFilename, mo.OriginalWidth, mo.OriginalHeight, mo.OriginalSizeKB, mo.ExternalHtmlSource, mo.ExternalType, mo.Seq, 
			//  mo.CreatedBy, mo.DateAdded, mo.LastModifiedBy, mo.DateLastModified, mo.IsPrivate
			// FROM [gs_MediaObject] mo JOIN [gs_Album] a ON mo.FKAlbumID = a.AlbumID
			// WHERE mo.MediaObjectID = @MediaObjectId AND a.FKGalleryID = @GalleryID

			if (moDto == null)
				throw new ArgumentNullException("moDto");

			if (parentAlbum == null)
				throw new ArgumentNullException("parentAlbum");

#if DEBUG
			//int id = moDto.MediaObjectId;

			//Trace.WriteLine(
			//  String.Format(
			//    "LoadMediaObjectInstance(IDataRecord dr, IAlbum parentAlbum): Retrieving media object {0} from data store...", id));
#endif

			GalleryObjectType goType = HelperFunctions.DetermineMediaObjectType(moDto.OriginalFilename, moDto.ExternalHtmlSource);
			IGalleryObject go = null;

			try
			{
				// It is tempting to look in the media object cache for the desired object, but this does not work. If you try, then when you retrieve the
				// album from the cache on the next page load it will have zero media objects but the AreChildrenInflated property will be true, which
				// causes problems.
				switch (goType)
				{
					case GalleryObjectType.Image:
						{
							#region Create Image

							go = new Image(
								moDto.MediaObjectId,
								parentAlbum,
								moDto.Title.Trim(),
								moDto.HashKey.Trim(),
								moDto.ThumbnailFilename,
								moDto.ThumbnailWidth,
								moDto.ThumbnailHeight,
								moDto.ThumbnailSizeKB,
								moDto.OptimizedFilename.Trim(),
								moDto.OptimizedWidth,
								moDto.OptimizedHeight,
								moDto.OptimizedSizeKB,
								moDto.OriginalFilename.Trim(),
								moDto.OriginalWidth,
								moDto.OriginalHeight,
								moDto.OriginalSizeKB,
								moDto.Seq,
								moDto.CreatedBy.Trim(),
								Convert.ToDateTime(moDto.DateAdded, CultureInfo.CurrentCulture),
								moDto.LastModifiedBy.Trim(),
								HelperFunctions.ToDateTime(moDto.DateLastModified),
								moDto.IsPrivate,
								true,
								null);
							break;

							#endregion
						}
					case GalleryObjectType.Video:
						{
							#region Create Video

							go = new Video(
								moDto.MediaObjectId,
								parentAlbum,
								moDto.Title.Trim(),
								moDto.HashKey.Trim(),
								moDto.ThumbnailFilename,
								moDto.ThumbnailWidth,
								moDto.ThumbnailHeight,
								moDto.ThumbnailSizeKB,
								moDto.OptimizedFilename.Trim(),
								moDto.OptimizedWidth,
								moDto.OptimizedHeight,
								moDto.OptimizedSizeKB,
								moDto.OriginalFilename.Trim(),
								moDto.OriginalWidth,
								moDto.OriginalHeight,
								moDto.OriginalSizeKB,
								moDto.Seq,
								moDto.CreatedBy.Trim(),
								Convert.ToDateTime(moDto.DateAdded, CultureInfo.CurrentCulture),
								moDto.LastModifiedBy.Trim(),
								HelperFunctions.ToDateTime(moDto.DateLastModified),
								moDto.IsPrivate,
								true,
								null);
							break;

							#endregion
						}
					case GalleryObjectType.Audio:
						{
							#region Create Audio

							go = new Audio(
								moDto.MediaObjectId,
								parentAlbum,
								moDto.Title.Trim(),
								moDto.HashKey.Trim(),
								moDto.ThumbnailFilename,
								moDto.ThumbnailWidth,
								moDto.ThumbnailHeight,
								moDto.ThumbnailSizeKB,
								moDto.OptimizedFilename.Trim(),
								moDto.OptimizedWidth,
								moDto.OptimizedHeight,
								moDto.OptimizedSizeKB,
								moDto.OriginalFilename.Trim(),
								moDto.OriginalWidth,
								moDto.OriginalHeight,
								moDto.OriginalSizeKB,
								moDto.Seq,
								moDto.CreatedBy.Trim(),
								Convert.ToDateTime(moDto.DateAdded, CultureInfo.CurrentCulture),
								moDto.LastModifiedBy.Trim(),
								HelperFunctions.ToDateTime(moDto.DateLastModified),
								moDto.IsPrivate,
								true,
								null);
							break;

							#endregion
						}
					case GalleryObjectType.Generic:
					case GalleryObjectType.Unknown:
						{
							#region Create Generic Media Object

							go = new GenericMediaObject(
								moDto.MediaObjectId,
								parentAlbum,
								moDto.Title.Trim(),
								moDto.HashKey.Trim(),
								moDto.ThumbnailFilename,
								moDto.ThumbnailWidth,
								moDto.ThumbnailHeight,
								moDto.ThumbnailSizeKB,
								moDto.OriginalFilename.Trim(),
								moDto.OriginalWidth,
								moDto.OriginalHeight,
								moDto.OriginalSizeKB,
								moDto.Seq,
								moDto.CreatedBy.Trim(),
								Convert.ToDateTime(moDto.DateAdded, CultureInfo.CurrentCulture),
								moDto.LastModifiedBy.Trim(),
								HelperFunctions.ToDateTime(moDto.DateLastModified),
								moDto.IsPrivate,
								true,
								null);
							break;

							#endregion
						}
					case GalleryObjectType.External:
						{
							#region Create External

							go = new ExternalMediaObject(
								moDto.MediaObjectId,
								parentAlbum,
								moDto.Title.Trim(),
								moDto.HashKey.Trim(),
								moDto.ThumbnailFilename,
								moDto.ThumbnailWidth,
								moDto.ThumbnailHeight,
								moDto.ThumbnailSizeKB,
								moDto.ExternalHtmlSource.Trim(),
								MimeTypeEnumHelper.ParseMimeTypeCategory(moDto.ExternalType.Trim()),
								moDto.Seq,
								moDto.CreatedBy.Trim(),
								Convert.ToDateTime(moDto.DateAdded, CultureInfo.CurrentCulture),
								moDto.LastModifiedBy.Trim(),
								HelperFunctions.ToDateTime(moDto.DateLastModified),
								moDto.IsPrivate,
								true);
							break;

							#endregion
						}
					default:
						{
							throw new UnsupportedMediaObjectTypeException(Path.Combine(parentAlbum.FullPhysicalPath, moDto.OriginalFilename));
						}
				}

				AddMediaObjectMetadata(go, moDto);

				AddToMediaObjectCache(go);
			}
			catch
			{
				if (go != null)
					go.Dispose();

				throw;
			}

			return go;
		}

		/// <summary>
		/// Returns an object that knows how to persist media objects to the data store.
		/// </summary>
		/// <param name="galleryObject">A media object to which the save behavior applies. Must be a valid media
		/// object such as <see cref="Image" />, <see cref="Video" />, etc. Do not pass an <see cref="Album" />.</param>
		/// <returns>Returns an object that implements ISaveBehavior.</returns>
		public static ISaveBehavior GetMediaObjectSaveBehavior(IGalleryObject galleryObject)
		{
			Debug.Assert((!(galleryObject is Album)), "It is invalid to pass an album as a parameter to the GetMediaObjectSaveBehavior() method.");

			return new MediaObjectSaveBehavior(galleryObject as GalleryObject);
		}

		/// <summary>
		/// Returns an object that knows how to delete media objects from the data store.
		/// </summary>
		/// <param name="galleryObject">A media object to which the delete behavior applies. Must be a valid media
		/// object such as Image, Video, etc. Do not pass an Album; use <see cref="GetAlbumDeleteBehavior" /> for configuring <see cref="Album" /> objects.</param>
		/// <returns>Returns an object that implements <see cref="IDeleteBehavior" />.</returns>
		public static IDeleteBehavior GetMediaObjectDeleteBehavior(IGalleryObject galleryObject)
		{
			Debug.Assert((!(galleryObject is Album)), "It is invalid to pass an album as a parameter to the GetMediaObjectDeleteBehavior() method.");

			return new MediaObjectDeleteBehavior(galleryObject);
		}

		#endregion

		#region Image Methods

		/// <summary>
		/// Create a minimally populated <see cref="Image" /> instance from the specified parameters.
		/// </summary>
		/// <param name="imageFile">A <see cref="System.IO.FileInfo" /> object representing a supported image type. The file must already
		/// exist in the album's directory. If the file has a matching record in the data store, a reference to the existing 
		/// object is returned; otherwise, a new instance is returned. Otherwise, a new instance is returned. For new instances, 
		///		call <see cref="IGalleryObject.Save" /> to persist the object to the data store.</param>
		/// <param name="parentAlbum">The album in which the image exists (for media objects that already exist
		/// in the data store), or should be added to (for new media objects which need to be inserted into the 
		/// data store).</param>
		/// <returns>Returns an <see cref="Image" /> instance corresponding to the specified parameters.</returns>
		/// <exception cref="InvalidMediaObjectException">Thrown when 
		/// <paramref name = "imageFile" /> refers to a file that is not in the same directory as the parent album's directory.</exception>
		/// <exception cref="UnsupportedMediaObjectTypeException">Thrown when
		/// <paramref name = "imageFile" /> has a file extension that Gallery Server Pro is configured to reject, or it is
		/// associated with a non-image MIME type.</exception>
		/// <exception cref="UnsupportedImageTypeException">Thrown when the 
		/// .NET Framework is unable to load an image file into the <see cref="System.Drawing.Bitmap" /> class. This is 
		/// probably because it is corrupted, not an image supported by the .NET Framework, or the server does not have 
		/// enough memory to process the image. The file cannot, therefore, be handled using the <see cref="Image" /> 
		/// class; use <see cref="GenericMediaObject" /> instead.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name = "imageFile" /> or <paramref name = "parentAlbum" /> is null.</exception>
		public static IGalleryObject CreateImageInstance(FileInfo imageFile, IAlbum parentAlbum)
		{
			if (imageFile == null)
				throw new ArgumentNullException("imageFile");

			if (parentAlbum == null)
				throw new ArgumentNullException("parentAlbum");

			// Validation check: Make sure the configuration settings allow for this particular type of file to be added.
			if (!HelperFunctions.IsFileAuthorizedForAddingToGallery(imageFile.Name, parentAlbum.GalleryId))
				throw new UnsupportedMediaObjectTypeException(imageFile.FullName);

			// If the file belongs to an existing media object, return a reference to it.
			foreach (IGalleryObject childMediaObject in parentAlbum.GetChildGalleryObjects(GalleryObjectType.Image))
			{
				if (childMediaObject.Original.FileNamePhysicalPath == imageFile.FullName)
					return childMediaObject;
			}

			// Create a new image object, which will cause a new record to be inserted in the data store when Save() is called.
			return new Image(imageFile, parentAlbum);
		}

		/// <summary>
		/// Create a fully inflated image instance based on the <see cref="IGalleryObject.Id">ID</see> of the image parameter. Overwrite
		/// properties of the image parameter with the retrieved values from the data store. The returned image
		/// is the same object reference as the image parameter.
		/// </summary>
		/// <param name="image">The image whose properties should be overwritten with the values from the data store.</param>
		/// <returns>Returns an inflated image instance with all properties set to the values from the data store.
		/// </returns>
		/// <exception cref="InvalidMediaObjectException">Thrown when
		/// an image is not found in the data store that matches the <see cref="IGalleryObject.Id">ID</see> of the image parameter in the current gallery.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="image" /> is null.</exception>
		public static IGalleryObject LoadImageInstance(IGalleryObject image)
		{
			if (image == null)
				throw new ArgumentNullException("image");

			IGalleryObject retrievedImage = RetrieveMediaObject(image.Id, GalleryObjectType.Image, (IAlbum)image.Parent);

			image.GalleryId = retrievedImage.GalleryId;
			image.Title = retrievedImage.Title;
			image.CreatedByUserName = retrievedImage.CreatedByUserName;
			image.DateAdded = retrievedImage.DateAdded;
			image.LastModifiedByUserName = retrievedImage.LastModifiedByUserName;
			image.DateLastModified = retrievedImage.DateLastModified;
			image.IsPrivate = retrievedImage.IsPrivate;
			image.Hashkey = retrievedImage.Hashkey;
			image.Sequence = retrievedImage.Sequence;
			image.MetadataItems.Clear();
			image.MetadataItems.AddRange(retrievedImage.MetadataItems.Copy());

			string albumPhysicalPath = image.Parent.FullPhysicalPathOnDisk;

			#region Thumbnail

			image.Thumbnail.MediaObjectId = retrievedImage.Id;
			image.Thumbnail.FileName = retrievedImage.Thumbnail.FileName;
			image.Thumbnail.Height = retrievedImage.Thumbnail.Height;
			image.Thumbnail.Width = retrievedImage.Thumbnail.Width;

			IGallerySettings gallerySetting = LoadGallerySetting(image.GalleryId);

			// The thumbnail is stored in either the album's physical path or an alternate location (if thumbnailPath config setting is specified) .
			string thumbnailPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(albumPhysicalPath, gallerySetting.FullThumbnailPath, gallerySetting.FullMediaObjectPath);
			image.Thumbnail.FileNamePhysicalPath = Path.Combine(thumbnailPath, image.Thumbnail.FileName);

			#endregion

			#region Optimized

			image.Optimized.MediaObjectId = retrievedImage.Id;
			image.Optimized.FileName = retrievedImage.Optimized.FileName;
			image.Optimized.Height = retrievedImage.Optimized.Height;
			image.Optimized.Width = retrievedImage.Optimized.Width;

			// Calcululate the full file path to the optimized image. If the optimized filename is equal to the original filename, then no
			// optimized version exists, and we'll just point to the original. If the names are different, then there is a separate optimized
			// image file, and it is stored in either the album's physical path or an alternate location (if optimizedPath config setting is specified).
			string optimizedPath = albumPhysicalPath;

			if (retrievedImage.Optimized.FileName != retrievedImage.Original.FileName)
				optimizedPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(albumPhysicalPath, gallerySetting.FullOptimizedPath, gallerySetting.FullMediaObjectPath);

			image.Optimized.FileNamePhysicalPath = Path.Combine(optimizedPath, image.Optimized.FileName);

			#endregion

			#region Original

			image.Original.MediaObjectId = retrievedImage.Id;
			image.Original.FileName = retrievedImage.Original.FileName;
			image.Original.Height = retrievedImage.Original.Height;
			image.Original.Width = retrievedImage.Original.Width;
			image.Original.FileNamePhysicalPath = Path.Combine(albumPhysicalPath, image.Original.FileName);
			image.Original.ExternalHtmlSource = retrievedImage.Original.ExternalHtmlSource;
			image.Original.ExternalType = retrievedImage.Original.ExternalType;

			#endregion

			image.IsInflated = true;
			image.HasChanges = false;

			return image;
		}

		/// <summary>
		/// Create a fully inflated image instance based on the mediaObjectId.
		/// </summary>
		/// <param name="mediaObjectId">An <see cref="IGalleryObject.Id">ID</see> that uniquely represents an existing image media object.</param>
		/// <returns>Returns an inflated image instance with all properties set to the values from the data store.</returns>
		/// <exception cref="InvalidMediaObjectException">Thrown when
		/// an image is not found in the data store that matches the mediaObjectId parameter and the current gallery.</exception>
		public static IGalleryObject LoadImageInstance(int mediaObjectId)
		{
			return LoadImageInstance(mediaObjectId, null);
		}

		/// <summary>
		/// Create a fully inflated image instance based on the mediaObjectId.
		/// </summary>
		/// <param name="mediaObjectId">An <see cref="IGalleryObject.Id">ID</see> that uniquely represents an existing image media object.</param>
		/// <param name="parentAlbum">The album containing the media object specified by mediaObjectId. Specify
		/// null if a reference to the album is not available, and it will be created based on the parent album
		/// specified in the data store.</param>
		/// <returns>Returns an inflated image instance with all properties set to the values from the data store.</returns>
		/// <exception cref="InvalidMediaObjectException">Thrown when
		/// an image is not found in the data store that matches the mediaObjectId parameter and the current gallery.</exception>
		public static IGalleryObject LoadImageInstance(int mediaObjectId, IAlbum parentAlbum)
		{
			return RetrieveMediaObject(mediaObjectId, GalleryObjectType.Image, parentAlbum);
		}

		#endregion

		#region Video Methods

		/// <summary>
		/// Create a minimally populated <see cref="Video" /> instance from the specified parameters.
		/// </summary>
		/// <param name="videoFile">A <see cref="System.IO.FileInfo" /> object representing a supported video type. The file must already
		/// exist in the album's directory. If the file has a matching record in the data store, a reference to the existing 
		/// object is returned; otherwise, a new instance is returned. Otherwise, a new instance is returned. For new instances, 
		///		call <see cref="IGalleryObject.Save" /> to persist the object to the data store.</param>
		/// <param name="parentAlbum">The album in which the video exists (for media objects that already exist
		/// in the data store), or should be added to (for new media objects which need to be inserted into the 
		/// data store).</param>
		/// <returns>Returns a <see cref="Video" /> instance corresponding to the specified parameters.</returns>
		/// <exception cref="UnsupportedMediaObjectTypeException">Thrown when
		/// <paramref name = "videoFile" /> has a file extension that Gallery Server Pro is configured to reject, or it is
		/// associated with a non-video MIME type.</exception>
		/// <exception cref="InvalidMediaObjectException">Thrown when   
		/// <paramref name = "videoFile" /> refers to a file that is not in the same directory as the parent album's directory.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name = "videoFile" /> or <paramref name = "parentAlbum" /> is null.</exception>
		public static IGalleryObject CreateVideoInstance(FileInfo videoFile, IAlbum parentAlbum)
		{
			if (videoFile == null)
				throw new ArgumentNullException("videoFile");

			if (parentAlbum == null)
				throw new ArgumentNullException("parentAlbum");

			// Validation check: Make sure the configuration settings allow for this particular type of file to be added.
			if (!HelperFunctions.IsFileAuthorizedForAddingToGallery(videoFile.Name, parentAlbum.GalleryId))
				throw new UnsupportedMediaObjectTypeException(videoFile.FullName);

			// If the file belongs to an existing media object, return a reference to it.
			foreach (IGalleryObject childMediaObject in parentAlbum.GetChildGalleryObjects(GalleryObjectType.Video))
			{
				if (childMediaObject.Original.FileNamePhysicalPath == videoFile.FullName)
					return childMediaObject;
			}

			// Create a new video object, which will cause a new record to be inserted in the data store when Save() is called.
			return new Video(videoFile, parentAlbum);
		}

		/// <summary>
		/// Create a fully inflated <see cref="Video" /> instance based on the <see cref="IGalleryObject.Id">ID</see> of the video parameter. Overwrite
		/// properties of the video parameter with the retrieved values from the data store. The returned video
		/// is the same object reference as the video parameter.
		/// </summary>
		/// <param name="video">The video whose properties should be overwritten with the values from the data store.</param>
		/// <returns>Returns an inflated <see cref="Video" /> instance with all properties set to the values from the data store.
		/// </returns>
		/// <exception cref="InvalidMediaObjectException">Thrown when a video is not found in the data store that matches the 
		/// <see cref="IGalleryObject.Id">ID</see> of the video parameter in the current gallery.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="video" /> is null.</exception>
		public static IGalleryObject LoadVideoInstance(IGalleryObject video)
		{
			if (video == null)
				throw new ArgumentNullException("video");

			IGalleryObject retrievedVideo = RetrieveMediaObject(video.Id, GalleryObjectType.Video, (IAlbum)video.Parent);

			video.GalleryId = retrievedVideo.GalleryId;
			video.Title = retrievedVideo.Title;
			video.CreatedByUserName = retrievedVideo.CreatedByUserName;
			video.DateAdded = retrievedVideo.DateAdded;
			video.LastModifiedByUserName = retrievedVideo.LastModifiedByUserName;
			video.DateLastModified = retrievedVideo.DateLastModified;
			video.IsPrivate = retrievedVideo.IsPrivate;
			video.Hashkey = retrievedVideo.Hashkey;
			video.Sequence = retrievedVideo.Sequence;

			string albumPhysicalPath = video.Parent.FullPhysicalPathOnDisk;

			#region Thumbnail

			video.Thumbnail.MediaObjectId = retrievedVideo.Id;
			video.Thumbnail.FileName = retrievedVideo.Thumbnail.FileName;
			video.Thumbnail.Height = retrievedVideo.Thumbnail.Height;
			video.Thumbnail.Width = retrievedVideo.Thumbnail.Width;

			IGallerySettings gallerySetting = LoadGallerySetting(video.GalleryId);

			// The thumbnail is stored in either the album's physical path or an alternate location (if thumbnailPath config setting is specified) .
			string thumbnailPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(albumPhysicalPath, gallerySetting.FullThumbnailPath, gallerySetting.FullMediaObjectPath);
			video.Thumbnail.FileNamePhysicalPath = Path.Combine(thumbnailPath, video.Thumbnail.FileName);

			#endregion

			#region Optimized

			// Video objects do not have an optimized object.

			#endregion

			#region Original

			video.Original.MediaObjectId = retrievedVideo.Id;
			video.Original.FileName = retrievedVideo.Original.FileName;
			video.Original.Height = retrievedVideo.Original.Height;
			video.Original.Width = retrievedVideo.Original.Width;
			video.Original.FileNamePhysicalPath = Path.Combine(albumPhysicalPath, video.Original.FileName);
			video.Original.ExternalHtmlSource = retrievedVideo.Original.ExternalHtmlSource;
			video.Original.ExternalType = retrievedVideo.Original.ExternalType;

			#endregion

			video.IsInflated = true;
			video.HasChanges = false;

			return video;
		}

		/// <summary>
		/// Create a fully inflated <see cref="Video" /> instance based on the mediaObjectId.
		/// </summary>
		/// <param name="mediaObjectId">An <see cref="IGalleryObject.Id">ID</see> that uniquely represents an existing video object.</param>
		/// <returns>Returns an inflated <see cref="Video" /> instance with all properties set to the values from the data store.</returns>
		/// <exception cref="InvalidMediaObjectException">Thrown when
		/// a video is not found in the data store that matches the mediaObjectId parameter and the current gallery.</exception>
		public static IGalleryObject LoadVideoInstance(int mediaObjectId)
		{
			return LoadVideoInstance(mediaObjectId, null);
		}

		/// <summary>
		/// Create a fully inflated <see cref="Video" /> instance based on the mediaObjectId.
		/// </summary>
		/// <param name="mediaObjectId">An <see cref="IGalleryObject.Id">ID</see> that uniquely represents an existing video object.</param>
		/// <param name="parentAlbum">The album containing the media object specified by mediaObjectId. Specify
		/// null if a reference to the album is not available, and it will be created based on the parent album
		/// specified in the data store.</param>
		/// <returns>Returns an inflated <see cref="Video" /> instance with all properties set to the values from the data store.</returns>
		/// <exception cref="InvalidMediaObjectException">Thrown when
		/// a video is not found in the data store that matches the mediaObjectId parameter and the current gallery.</exception>
		public static IGalleryObject LoadVideoInstance(int mediaObjectId, IAlbum parentAlbum)
		{
			return RetrieveMediaObject(mediaObjectId, GalleryObjectType.Video, parentAlbum);
		}

		#endregion

		#region Audio Methods

		/// <summary>
		/// Create a minimally populated <see cref="Audio" /> instance from the specified parameters.
		/// </summary>
		/// <param name="audioFile">A <see cref="System.IO.FileInfo" /> object representing a supported audio type. The file must already
		/// exist in the album's directory. If the file has a matching record in the data store, a reference to the existing 
		/// object is returned; otherwise, a new instance is returned. Otherwise, a new instance is returned. For new instances, 
		///		call <see cref="IGalleryObject.Save" /> to persist the object to the data store.</param>
		/// <param name="parentAlbum">The album in which the audio exists (for media objects that already exist
		/// in the data store), or should be added to (for new media objects which need to be inserted into the 
		/// data store).</param>
		/// <returns>Returns an <see cref="Audio" /> instance corresponding to the specified parameters.</returns>
		/// <exception cref="InvalidMediaObjectException">Thrown when 
		/// <paramref name = "audioFile" /> refers to a file that is not in the same directory as the parent album's directory.</exception>
		/// <exception cref="UnsupportedMediaObjectTypeException">Thrown when
		/// <paramref name = "audioFile" /> has a file extension that Gallery Server Pro is configured to reject, or it is
		/// associated with a non-audio MIME type.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name = "audioFile" /> or <paramref name = "parentAlbum" /> is null.</exception>
		public static IGalleryObject CreateAudioInstance(FileInfo audioFile, IAlbum parentAlbum)
		{
			if (audioFile == null)
				throw new ArgumentNullException("audioFile");

			if (parentAlbum == null)
				throw new ArgumentNullException("parentAlbum");

			// Validation check: Make sure the configuration settings allow for this particular type of file to be added.
			if (!HelperFunctions.IsFileAuthorizedForAddingToGallery(audioFile.Name, parentAlbum.GalleryId))
				throw new UnsupportedMediaObjectTypeException(audioFile.FullName);

			// If the file belongs to an existing media object, return a reference to it.
			foreach (IGalleryObject childMediaObject in parentAlbum.GetChildGalleryObjects(GalleryObjectType.Audio))
			{
				if (childMediaObject.Original.FileNamePhysicalPath == audioFile.FullName)
					return childMediaObject;
			}

			// Create a new audio object, which will cause a new record to be inserted in the data store when Save() is called.
			return new Audio(audioFile, parentAlbum);
		}

		/// <summary>
		/// Create a fully inflated <see cref="Audio" /> instance based on the <see cref="IGalleryObject.Id">ID</see> of the audio parameter. Overwrite
		/// properties of the audio parameter with the retrieved values from the data store. The returned audio
		/// is the same object reference as the audio parameter.
		/// </summary>
		/// <param name="audio">The <see cref="Audio" /> instance whose properties should be overwritten with the values from the data store.</param>
		/// <returns>Returns an inflated <see cref="Audio" /> instance with all properties set to the values from the data store.
		/// </returns>
		/// <exception cref="InvalidMediaObjectException">Thrown when a audio file is not found in the data store that matches the 
		/// <see cref="IGalleryObject.Id">ID</see> of the audio parameter in the current gallery.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="audio" /> is null.</exception>
		public static IGalleryObject LoadAudioInstance(IGalleryObject audio)
		{
			if (audio == null)
				throw new ArgumentNullException("audio");

			IGalleryObject retrievedAudio = RetrieveMediaObject(audio.Id, GalleryObjectType.Audio, (IAlbum)audio.Parent);

			audio.GalleryId = retrievedAudio.GalleryId;
			audio.Title = retrievedAudio.Title;
			audio.CreatedByUserName = retrievedAudio.CreatedByUserName;
			audio.DateAdded = retrievedAudio.DateAdded;
			audio.LastModifiedByUserName = retrievedAudio.LastModifiedByUserName;
			audio.DateLastModified = retrievedAudio.DateLastModified;
			audio.IsPrivate = retrievedAudio.IsPrivate;
			audio.Hashkey = retrievedAudio.Hashkey;
			audio.Sequence = retrievedAudio.Sequence;

			string albumPhysicalPath = audio.Parent.FullPhysicalPathOnDisk;

			#region Thumbnail

			audio.Thumbnail.MediaObjectId = retrievedAudio.Id;
			audio.Thumbnail.FileName = retrievedAudio.Thumbnail.FileName;
			audio.Thumbnail.Height = retrievedAudio.Thumbnail.Height;
			audio.Thumbnail.Width = retrievedAudio.Thumbnail.Width;

			IGallerySettings gallerySetting = LoadGallerySetting(audio.GalleryId);

			// The thumbnail is stored in either the album's physical path or an alternate location (if thumbnailPath config setting is specified) .
			string thumbnailPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(albumPhysicalPath, gallerySetting.FullThumbnailPath, gallerySetting.FullMediaObjectPath);
			audio.Thumbnail.FileNamePhysicalPath = Path.Combine(thumbnailPath, audio.Thumbnail.FileName);

			#endregion

			#region Optimized

			// Audio objects do not have an optimized object.

			#endregion

			#region Original

			audio.Original.MediaObjectId = retrievedAudio.Id;
			audio.Original.FileName = retrievedAudio.Original.FileName;
			audio.Original.Height = retrievedAudio.Original.Height;
			audio.Original.Width = retrievedAudio.Original.Width;
			audio.Original.FileNamePhysicalPath = Path.Combine(albumPhysicalPath, audio.Original.FileName);
			audio.Original.ExternalHtmlSource = retrievedAudio.Original.ExternalHtmlSource;
			audio.Original.ExternalType = retrievedAudio.Original.ExternalType;

			#endregion

			audio.IsInflated = true;
			audio.HasChanges = false;

			return audio;
		}

		/// <summary>
		/// Create a fully inflated <see cref="Audio" /> instance based on the mediaObjectId.
		/// </summary>
		/// <param name="mediaObjectId">An <see cref="IGalleryObject.Id">ID</see> that uniquely represents an existing audio object.</param>
		/// <returns>Returns an inflated <see cref="Audio" /> instance with all properties set to the values from the data store.</returns>
		/// <exception cref="InvalidMediaObjectException">Thrown when
		/// an audio file is not found in the data store that matches the mediaObjectId parameter and the current gallery.</exception>
		public static IGalleryObject LoadAudioInstance(int mediaObjectId)
		{
			return LoadAudioInstance(mediaObjectId, null);
		}

		/// <summary>
		/// Create a fully inflated <see cref="Audio" /> instance based on the mediaObjectId.
		/// </summary>
		/// <param name="mediaObjectId">An <see cref="IGalleryObject.Id">ID</see> that uniquely represents an existing audio object.</param>
		/// <param name="parentAlbum">The album containing the media object specified by mediaObjectId. Specify
		/// null if a reference to the album is not available, and it will be created based on the parent album
		/// specified in the data store.</param>
		/// <returns>Returns an inflated <see cref="Audio" /> instance with all properties set to the values from the data store.</returns>
		/// <exception cref="InvalidMediaObjectException">Thrown when an audio file is not found in the data store that matches the 
		/// mediaObjectId parameter and the current gallery.</exception>
		public static IGalleryObject LoadAudioInstance(int mediaObjectId, IAlbum parentAlbum)
		{
			return RetrieveMediaObject(mediaObjectId, GalleryObjectType.Audio, parentAlbum);
		}

		#endregion

		#region Generic Media Object Methods

		/// <summary>
		/// Create a minimally populated <see cref="GenericMediaObject" /> instance from the specified parameters.
		/// </summary>
		/// <param name="file">A <see cref="System.IO.FileInfo" /> object representing a file to be managed by Gallery Server Pro. The file must 
		/// already exist in the album's directory. If the file has a matching record in the data store, a reference to the existing 
		/// object is returned; otherwise, a new instance is returned. Otherwise, a new instance is returned. For new instances, 
		///		call <see cref="IGalleryObject.Save" /> to persist the object to the data store.</param>
		/// <param name="parentAlbum">The album in which the file exists (for media objects that already exist
		/// in the data store), or should be added to (for new media objects which need to be inserted into the 
		/// data store).</param>
		/// <returns>Returns a <see cref="GenericMediaObject" /> instance corresponding to the specified parameters.</returns>
		/// <exception cref="UnsupportedMediaObjectTypeException">Thrown when
		/// <paramref name = "file" /> has a file extension that Gallery Server Pro is configured to reject.</exception>
		/// <exception cref="InvalidMediaObjectException">Thrown when   
		/// <paramref name = "file" /> refers to a file that is not in the same directory as the parent album's directory.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name = "file" /> or <paramref name = "parentAlbum" /> is null.</exception>
		public static IGalleryObject CreateGenericObjectInstance(FileInfo file, IAlbum parentAlbum)
		{
			if (file == null)
				throw new ArgumentNullException("file");

			if (parentAlbum == null)
				throw new ArgumentNullException("parentAlbum");

			// Validation check: Make sure the configuration settings allow for this particular type of file to be added.
			if (!HelperFunctions.IsFileAuthorizedForAddingToGallery(file.Name, parentAlbum.GalleryId))
				throw new UnsupportedMediaObjectTypeException(file.FullName);

			// If the file belongs to an existing media object, return a reference to it.
			foreach (IGalleryObject childMediaObject in parentAlbum.GetChildGalleryObjects(GalleryObjectType.Generic))
			{
				if (childMediaObject.Original.FileNamePhysicalPath == file.FullName)
					return childMediaObject;
			}

			// Create a new generic media object, which will cause a new record to be inserted in the data store when Save() is called.
			return new GenericMediaObject(file, parentAlbum);
		}

		/// <summary>
		/// Create a fully inflated <see cref="GenericMediaObject" /> instance based on the <see cref="IGalleryObject.Id">ID</see> of the 
		/// <paramref name = "genericMediaObject" /> parameter. 
		/// Overwrite properties of the <paramref name = "genericMediaObject" /> parameter with the retrieved values from the data store. 
		/// The returned instance is the same object reference as the <paramref name = "genericMediaObject" /> parameter.
		/// </summary>
		/// <param name="genericMediaObject">The object whose properties should be overwritten with the values from 
		/// the data store.</param>
		/// <returns>Returns an inflated <see cref="GenericMediaObject" /> instance with all properties set to the values from the 
		/// data store.</returns>
		/// <exception cref="InvalidMediaObjectException">Thrown when a record is not found in the data store that matches the 
		/// <see cref="IGalleryObject.Id">ID</see> of the <paramref name = "genericMediaObject" /> parameter in the current gallery.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="genericMediaObject" /> is null.</exception>
		public static IGalleryObject LoadGenericMediaObjectInstance(IGalleryObject genericMediaObject)
		{
			if (genericMediaObject == null)
				throw new ArgumentNullException("genericMediaObject");

			IGalleryObject retrievedGenericMediaObject = RetrieveMediaObject(genericMediaObject.Id, GalleryObjectType.Generic, (IAlbum)genericMediaObject.Parent);

			genericMediaObject.GalleryId = retrievedGenericMediaObject.GalleryId;
			genericMediaObject.Title = retrievedGenericMediaObject.Title;
			genericMediaObject.CreatedByUserName = retrievedGenericMediaObject.CreatedByUserName;
			genericMediaObject.DateAdded = retrievedGenericMediaObject.DateAdded;
			genericMediaObject.LastModifiedByUserName = retrievedGenericMediaObject.LastModifiedByUserName;
			genericMediaObject.DateLastModified = retrievedGenericMediaObject.DateLastModified;
			genericMediaObject.IsPrivate = retrievedGenericMediaObject.IsPrivate;
			genericMediaObject.Hashkey = retrievedGenericMediaObject.Hashkey;
			genericMediaObject.Sequence = retrievedGenericMediaObject.Sequence;

			string albumPhysicalPath = genericMediaObject.Parent.FullPhysicalPathOnDisk;

			#region Thumbnail

			genericMediaObject.Thumbnail.MediaObjectId = retrievedGenericMediaObject.Id;
			genericMediaObject.Thumbnail.FileName = retrievedGenericMediaObject.Thumbnail.FileName;
			genericMediaObject.Thumbnail.Height = retrievedGenericMediaObject.Thumbnail.Height;
			genericMediaObject.Thumbnail.Width = retrievedGenericMediaObject.Thumbnail.Width;

			IGallerySettings gallerySetting = LoadGallerySetting(genericMediaObject.GalleryId);

			// The thumbnail is stored in either the album's physical path or an alternate location (if thumbnailPath config setting is specified) .
			string thumbnailPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(albumPhysicalPath, gallerySetting.FullThumbnailPath, gallerySetting.FullMediaObjectPath);
			genericMediaObject.Thumbnail.FileNamePhysicalPath = Path.Combine(thumbnailPath, genericMediaObject.Thumbnail.FileName);

			#endregion

			#region Optimized

			// No optimized object for a generic media object.

			#endregion

			#region Original

			genericMediaObject.Original.MediaObjectId = retrievedGenericMediaObject.Id;
			genericMediaObject.Original.FileName = retrievedGenericMediaObject.Original.FileName;
			genericMediaObject.Original.Height = retrievedGenericMediaObject.Original.Height;
			genericMediaObject.Original.Width = retrievedGenericMediaObject.Original.Width;
			genericMediaObject.Original.FileNamePhysicalPath = Path.Combine(albumPhysicalPath, genericMediaObject.Original.FileName);
			genericMediaObject.Original.ExternalHtmlSource = retrievedGenericMediaObject.Original.ExternalHtmlSource;
			genericMediaObject.Original.ExternalType = retrievedGenericMediaObject.Original.ExternalType;

			#endregion

			genericMediaObject.IsInflated = true;
			genericMediaObject.HasChanges = false;

			return genericMediaObject;
		}

		/// <summary>
		/// Create a fully inflated <see cref="GenericMediaObject" /> instance based on the mediaObjectId.
		/// </summary>
		/// <param name="mediaObjectId">An <see cref="IGalleryObject.Id">ID</see> that uniquely represents an existing 
		/// <see cref="GenericMediaObject" /> object.</param>
		/// <returns>Returns an inflated <see cref="GenericMediaObject" /> instance with all properties set to the values from the 
		/// data store.</returns>
		/// <exception cref="InvalidMediaObjectException">Thrown when a record is not found in the data store that matches the 
		/// mediaObjectId parameter and the current gallery.</exception>
		public static IGalleryObject LoadGenericMediaObjectInstance(int mediaObjectId)
		{
			return LoadGenericMediaObjectInstance(mediaObjectId, null);
		}

		/// <summary>
		/// Create a fully inflated <see cref="GenericMediaObject" /> instance based on the mediaObjectId.
		/// </summary>
		/// <param name="mediaObjectId">An <see cref="IGalleryObject.Id">ID</see> that uniquely represents an existing <see cref="GenericMediaObject" /> object.</param>
		/// <param name="parentAlbum">The album containing the media object specified by mediaObjectId. Specify
		/// null if a reference to the album is not available, and it will be created based on the parent album
		/// specified in the data store.</param>
		/// <returns>Returns an inflated <see cref="GenericMediaObject" /> instance with all properties set to the values from the 
		/// data store.</returns>
		/// <exception cref="InvalidMediaObjectException">Thrown when a record is not found in the data store that matches the 
		/// mediaObjectId parameter and the current gallery.</exception>
		public static IGalleryObject LoadGenericMediaObjectInstance(int mediaObjectId, IAlbum parentAlbum)
		{
			return RetrieveMediaObject(mediaObjectId, GalleryObjectType.Generic, parentAlbum);
		}

		#endregion

		#region External Media Object Methods

		/// <summary>
		/// Create a minimally populated <see cref="ExternalMediaObject" /> instance from the specified parameters.
		/// </summary>
		/// <param name="externalHtmlSource">The HTML that defines an externally stored media object, such as one hosted at 
		/// YouTube or Silverlight.live.com.</param>
		/// <param name="mimeType">Specifies the category to which this mime type belongs. This usually corresponds to the first portion of 
		/// the full mime type description. (e.g. "image" if the full mime type is "image/jpeg").</param>
		/// <param name="parentAlbum">The album in which the file exists (for media objects that already exist
		/// in the data store), or should be added to (for new media objects which need to be inserted into the 
		/// data store).</param>
		/// <returns>Returns a minimally populated <see cref="ExternalMediaObject" /> instance from the specified parameters.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name = "externalHtmlSource" /> is an empty string or null.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="parentAlbum" /> is null.</exception>
		public static IGalleryObject CreateExternalMediaObjectInstance(string externalHtmlSource, MimeTypeCategory mimeType, IAlbum parentAlbum)
		{
			if (String.IsNullOrEmpty(externalHtmlSource))
				throw new ArgumentOutOfRangeException("externalHtmlSource", "The parameter is either null or an empty string.");

			if (parentAlbum == null)
				throw new ArgumentNullException("parentAlbum");

			// Create a new generic media object, which will cause a new record to be inserted in the data store when Save() is called.
			return new ExternalMediaObject(externalHtmlSource, mimeType, parentAlbum);
		}

		/// <summary>
		/// Create a fully inflated <see cref="ExternalMediaObject" /> instance based on the <see cref="IGalleryObject.Id">ID</see> of the 
		/// <paramref name = "externalMediaObject" /> parameter. 
		/// Overwrite properties of the <paramref name = "externalMediaObject" /> parameter with the retrieved values from the data store. 
		/// The returned instance is the same object reference as the <paramref name = "externalMediaObject" /> parameter.
		/// </summary>
		/// <param name="externalMediaObject">The object whose properties should be overwritten with the values from 
		/// the data store.</param>
		/// <returns>Returns an inflated <see cref="ExternalMediaObject" /> instance with all properties set to the values from the 
		/// data store.</returns>
		/// <exception cref="InvalidMediaObjectException">Thrown when a record is not found in the data store that matches the 
		/// <see cref="IGalleryObject.Id">ID</see> of the <paramref name = "externalMediaObject" /> parameter in the current gallery.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="externalMediaObject" /> is null.</exception>
		public static IGalleryObject LoadExternalMediaObjectInstance(IGalleryObject externalMediaObject)
		{
			if (externalMediaObject == null)
				throw new ArgumentNullException("externalMediaObject");

			IGalleryObject retrievedGenericMediaObject = RetrieveMediaObject(externalMediaObject.Id, GalleryObjectType.External, (IAlbum)externalMediaObject.Parent);

			externalMediaObject.GalleryId = retrievedGenericMediaObject.GalleryId;
			externalMediaObject.Title = retrievedGenericMediaObject.Title;
			externalMediaObject.CreatedByUserName = retrievedGenericMediaObject.CreatedByUserName;
			externalMediaObject.DateAdded = retrievedGenericMediaObject.DateAdded;
			externalMediaObject.LastModifiedByUserName = retrievedGenericMediaObject.LastModifiedByUserName;
			externalMediaObject.DateLastModified = retrievedGenericMediaObject.DateLastModified;
			externalMediaObject.IsPrivate = retrievedGenericMediaObject.IsPrivate;
			externalMediaObject.Hashkey = retrievedGenericMediaObject.Hashkey;
			externalMediaObject.Sequence = retrievedGenericMediaObject.Sequence;

			string albumPhysicalPath = externalMediaObject.Parent.FullPhysicalPathOnDisk;

			#region Thumbnail

			externalMediaObject.Thumbnail.FileName = retrievedGenericMediaObject.Thumbnail.FileName;
			externalMediaObject.Thumbnail.Height = retrievedGenericMediaObject.Thumbnail.Height;
			externalMediaObject.Thumbnail.Width = retrievedGenericMediaObject.Thumbnail.Width;

			IGallerySettings gallerySetting = LoadGallerySetting(externalMediaObject.GalleryId);

			// The thumbnail is stored in either the album's physical path or an alternate location (if thumbnailPath config setting is specified) .
			string thumbnailPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(albumPhysicalPath, gallerySetting.FullThumbnailPath, gallerySetting.FullMediaObjectPath);
			externalMediaObject.Thumbnail.FileNamePhysicalPath = Path.Combine(thumbnailPath, externalMediaObject.Thumbnail.FileName);

			#endregion

			#region Optimized

			// No optimized image for a generic media object.

			#endregion

			#region Original

			externalMediaObject.Original.FileName = retrievedGenericMediaObject.Original.FileName;
			externalMediaObject.Original.Height = retrievedGenericMediaObject.Original.Height;
			externalMediaObject.Original.Width = retrievedGenericMediaObject.Original.Width;
			externalMediaObject.Original.FileNamePhysicalPath = Path.Combine(albumPhysicalPath, externalMediaObject.Original.FileName);
			externalMediaObject.Original.ExternalHtmlSource = retrievedGenericMediaObject.Original.ExternalHtmlSource;
			externalMediaObject.Original.ExternalType = retrievedGenericMediaObject.Original.ExternalType;

			#endregion

			externalMediaObject.IsInflated = true;
			externalMediaObject.HasChanges = false;

			return externalMediaObject;
		}

		#endregion

		#endregion

		#region Album Methods

		/// <summary>
		/// Create a new <see cref="Album" /> instance with an unassigned <see cref="IGalleryObject.Id">ID</see> and properties set to default values.
		/// A valid <see cref="IGalleryObject.Id">ID</see> will be generated when the object is persisted to the data store during
		/// the <see cref="IGalleryObject.Save" /> method. Use this overload when creating a new album and it has not yet been persisted
		/// to the data store. Guaranteed to not return null.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <returns>
		/// Returns an <see cref="Album" /> instance corresponding to the specified parameters.
		/// </returns>
		/// <overloads>Create a minimally populated <see cref="Album" /> instance.</overloads>
		public static IAlbum CreateEmptyAlbumInstance(int galleryId)
		{
			return new Album(int.MinValue, galleryId);
		}

		/// <summary>
		/// Creates an empty gallery instance. The <see cref="IGallery.GalleryId" /> will be set to <see cref="int.MinValue" />. 
		/// Generally, gallery instances should be loaded from the data store, but this method can be used to create a new gallery.
		/// </summary>
		/// <returns>Returns an <see cref="IGallery" /> instance.</returns>
		public static IGallery CreateGalleryInstance()
		{
			return new Gallery();
		}

		/// <summary>
		/// Create a minimally populated <see cref="Album" /> instance corresponding to the specified <paramref name = "albumId" />. 
		/// Use this overload when the album already exists in the data store but you do not necessarily need to retrieve its properties. 
		/// A lazy load is performed the first time a property is accessed.
		/// </summary>
		/// <param name="albumId">The <see cref="IGalleryObject.Id">ID</see> that uniquely identifies an existing album.</param>
		/// <param name="galleryId">The gallery ID.</param>
		/// <returns>
		/// Returns an instance that implements <see cref="IAlbum" /> corresponding to the specified parameters.
		/// </returns>
		public static IAlbum CreateAlbumInstance(int albumId, int galleryId)
		{
			return new Album(albumId, galleryId);
		}

		/// <overloads>
		/// Loads a writeable instance of the top-level album from the data store for the specified gallery. 
		/// </overloads>
		///  <summary>
		/// Loads a writeable instance of the top-level album from the data store for the specified gallery. Metadata for media
		/// objects are automatically loaded. If this album contains child objects, they are added but not inflated. Child objects 
		/// are automatically inflated when any of the <see cref="IGalleryObject.GetChildGalleryObjects()" /> overloaded methods 
		/// are called. Guaranteed to not return null.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <returns>
		/// Returns an instance that implements <see cref="IAlbum" /> with all properties set to the values from the data store.
		/// </returns>
		public static IAlbum LoadRootAlbumInstance(int galleryId)
		{
			return LoadRootAlbumInstance(galleryId, true);
		}

		/// <summary>
		/// Loads a writeable instance of the top-level album from the data store for the specified gallery, optionally specifying
		/// whether to suppress the loading of media object metadata. Suppressing metadata loading offers a performance improvement,
		/// so when this data is not needed, set <paramref name="allowMetadataLoading" /> to <c>false</c>. If this album contains
		/// child objects, they are added but not inflated. Child objects are automatically inflated when any of the
		/// <see cref="IGalleryObject.GetChildGalleryObjects()"/> overloaded methods are called. Guaranteed to not return null.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <param name="allowMetadataLoading">if set to <c>false</c> the metadata for media objects are not loaded.</param>
		/// <returns>
		/// Returns an instance that implements <see cref="IAlbum"/> with all properties set to the values from the data store.
		/// </returns>
		public static IAlbum LoadRootAlbumInstance(int galleryId, bool allowMetadataLoading)
		{
			IAlbum album;

			try
			{
				album = Factory.LoadAlbumInstance(Factory.LoadGallery(galleryId).RootAlbumId, true, true, allowMetadataLoading);
			}
			catch (InvalidAlbumException)
			{
				album = CreateRootAlbum(galleryId);
			}

			return album;
		}

		/// <summary>
		/// Return all top-level albums in the specified <paramref name = "galleryId">gallery</paramref> where the <paramref name = "roles" /> 
		/// provide the requested <paramref name = "permissions" />. If more than one album is found, they are wrapped in a virtual container 
		/// album where the <see cref="IAlbum.IsVirtualAlbum" /> property is set to true. If the roles do not provide permission to any
		/// objects in the gallery, then a virtual album is returned where <see cref="IAlbum.IsVirtualAlbum" />=<c>true</c> and 
		/// <see cref="IGalleryObject.Id" />=<see cref="Int32.MinValue" />. Returns null if no matching albums are found.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <param name="permissions">The permissions that must be provided by the roles.</param>
		/// <param name="roles">The roles belonging to a user.</param>
		/// <param name="isAuthenticated">Indicates whether the user belonging to the <paramref name="roles" /> is authenticated.</param>
		/// <returns>
		/// Returns an <see cref="IAlbum" /> that is or contains the top-lvel album(s) that the <paramref name = "roles" />
		/// provide the requested <paramref name = "permissions" />. Returns null if no matching albums are found.
		/// </returns>
		public static IAlbum LoadRootAlbum(int galleryId, SecurityActions permissions, IEnumerable<IGalleryServerRole> roles, bool isAuthenticated)
		{
			if (isAuthenticated)
			{
				return LoadRootAlbumForLoggedOnUser(galleryId, permissions, roles);
			}
			else
			{
				return LoadRootAlbumForAnonymousUser(galleryId, permissions);
			}
		}

		private static IAlbum LoadRootAlbumForLoggedOnUser(int galleryId, SecurityActions permissions, IEnumerable<IGalleryServerRole> roles)
		{
			// Step 1: Compile a list of album IDs having the requested permissions.
			List<int> allAlbumIds = GetAlbumIdsHavingRequestedPermissionsInRoles(galleryId, permissions, roles);

			// Step 2: Convert previous list to contain ONLY top-level albums.
			List<int> rootAlbumIds = RemoveNestedAlbumIds(allAlbumIds);

			// Step 3: Package results into an album container. If there is only one viewable root album, then just create an instance of that album.
			// Otherwise, create a virtual root album to contain the multiple viewable albums.
			IAlbum rootAlbum = null;
			try
			{
				if (rootAlbumIds.Count == 1)
				{
					rootAlbum = LoadAlbumInstance(rootAlbumIds[0], true);
				}
				else
				{
					// Create virtual album to serve as a container for the child albums the user has permission to view.
					rootAlbum = CreateEmptyAlbumInstance(galleryId);
					rootAlbum.IsVirtualAlbum = true;
					rootAlbum.Title = Resources.Virtual_Album_Title;
					foreach (int albumId in rootAlbumIds)
					{
						rootAlbum.Add(LoadAlbumInstance(albumId, false));
					}
				}
			}
			catch
			{
				if (rootAlbum != null)
					rootAlbum.Dispose();

				throw;
			}

			return rootAlbum;
		}

		/// <summary>
		/// Generate a new list containing a subset of <paramref name="allAlbumIds" /> that contains only a list of top-level album IDs.
		/// Any albums that have a parent - at any level - in the list are not included.
		/// </summary>
		/// <param name="allAlbumIds">All album IDs to process.</param>
		/// <returns>Returns an enumerable list of integers representing the album IDs that satisfy the criteria.</returns>
		private static List<int> RemoveNestedAlbumIds(List<int> allAlbumIds)
		{
			// Loop through our list of album IDs. If any album has an ancestor that is also in the list, then remove it. 
			// We only want a list of top level albums.
			List<int> rootAlbumIds = new List<int>(allAlbumIds);
			List<int> albumIdsToRemove = new List<int>(rootAlbumIds.Count);
			foreach (int viewableAlbumId in allAlbumIds)
			{
				IGalleryObject album = LoadAlbumInstance(viewableAlbumId, false);
				while (true)
				{
					album = album.Parent as IAlbum;
					if (album == null)
						break;

					if (allAlbumIds.Contains(album.Id))
					{
						albumIdsToRemove.Add(viewableAlbumId);
						break;
					}
				}
			}
			foreach (int albumId in albumIdsToRemove)
			{
				rootAlbumIds.Remove(albumId);
			}

			return rootAlbumIds;
		}

		/// <summary>
		/// Gets the album IDs in the gallery belonging to <paramref name="galleryId" /> having the specified <paramref name="permissions" /> 
		/// in the <paramref name="roles" />. Generally the returned IDs are top-level albums, but there may be nested album IDs when permissions
		/// from multiple roles overlap and are applied to different albums in the hierarchy. A subsequent function should remove album IDs
		/// that are children of other album IDs in the returned instance.
		/// </summary>
		/// <param name="galleryId">The gallery ID. Only album IDs belonging to this gallery are returned.</param>
		/// <param name="permissions">The requested permissions.</param>
		/// <param name="roles">The roles.</param>
		/// <returns>Returns an enumerable list of integers representing the album IDs that satisfy the criteria.</returns>
		private static List<int> GetAlbumIdsHavingRequestedPermissionsInRoles(int galleryId, SecurityActions permissions, IEnumerable<IGalleryServerRole> roles)
		{
			List<int> allAlbumIds = new List<int>();

			foreach (IGalleryServerRole role in roles)
			{
				// Get the subset of root album ID's that belong to the specified gallery.
				ICollection<int> rootAlbumIdsInGallery = GetAlbumIdsForGallery(role.RootAlbumIds, galleryId);

				foreach (SecurityActions permission in SecurityActionEnumHelper.ParseSecurityAction(permissions))
				{
					switch (permission)
					{
						case SecurityActions.ViewAlbumOrMediaObject:
							if (role.AllowViewAlbumOrMediaObject) AddIntegersToCollectionIfNotPresent(allAlbumIds, rootAlbumIdsInGallery);
							break;
						case SecurityActions.ViewOriginalImage:
							if (role.AllowViewOriginalImage) AddIntegersToCollectionIfNotPresent(allAlbumIds, rootAlbumIdsInGallery);
							break;
						case SecurityActions.AddChildAlbum:
							if (role.AllowAddChildAlbum) AddIntegersToCollectionIfNotPresent(allAlbumIds, rootAlbumIdsInGallery);
							break;
						case SecurityActions.AddMediaObject:
							if (role.AllowAddMediaObject) AddIntegersToCollectionIfNotPresent(allAlbumIds, rootAlbumIdsInGallery);
							break;
						case SecurityActions.AdministerSite:
							if (role.AllowAdministerSite) AddIntegersToCollectionIfNotPresent(allAlbumIds, rootAlbumIdsInGallery);
							break;
						case SecurityActions.AdministerGallery:
							if (role.AllowAdministerGallery) AddIntegersToCollectionIfNotPresent(allAlbumIds, rootAlbumIdsInGallery);
							break;
						case SecurityActions.DeleteAlbum:
							// It is OK to delete the album if the AllowDeleteChildAlbum permission is true and one of the following is true:
							// 1. The album is the root album and its ID is in the list of targeted albums (Note that we never actually delete the root album.
							//    Instead, we delete all objects within the album. But the idea of deleting the top level album to clear out all objects in the
							//    gallery is useful to the user.)
							// 2. The album is not the root album and its parent album's ID is in the list of targeted albums.
							if (role.AllowDeleteChildAlbum)
							{
								foreach (int albumId in rootAlbumIdsInGallery)
								{
									IAlbum album = LoadAlbumInstance(albumId, false);
									if (album.IsRootAlbum)
									{
										if (!role.AllAlbumIds.Contains(album.Id))
											allAlbumIds.Add(albumId);
										break;
									}
									else if (!role.AllAlbumIds.Contains(album.Parent.Id))
										allAlbumIds.Add(albumId);
									break;
								}
							}
							break;
						case SecurityActions.DeleteChildAlbum:
							if (role.AllowDeleteChildAlbum) AddIntegersToCollectionIfNotPresent(allAlbumIds, rootAlbumIdsInGallery);
							break;
						case SecurityActions.DeleteMediaObject:
							if (role.AllowDeleteMediaObject) AddIntegersToCollectionIfNotPresent(allAlbumIds, rootAlbumIdsInGallery);
							break;
						case SecurityActions.EditAlbum:
							if (role.AllowEditAlbum) AddIntegersToCollectionIfNotPresent(allAlbumIds, rootAlbumIdsInGallery);
							break;
						case SecurityActions.EditMediaObject:
							if (role.AllowEditMediaObject) AddIntegersToCollectionIfNotPresent(allAlbumIds, rootAlbumIdsInGallery);
							break;
						case SecurityActions.HideWatermark:
							if (role.HideWatermark) AddIntegersToCollectionIfNotPresent(allAlbumIds, rootAlbumIdsInGallery);
							break;
						case SecurityActions.Synchronize:
							if (role.AllowSynchronize) AddIntegersToCollectionIfNotPresent(allAlbumIds, rootAlbumIdsInGallery);
							break;
						default:
							throw new InvalidEnumArgumentException(String.Format(CultureInfo.CurrentCulture, "Factory.LoadRootAlbum() encountered a SecurityActions enumeration it does not recognize. The method may need to be updated. (Unrecognized SecurityActions enumeration: SecurityActions.{0})", permission));
					}
				}
			}

			return allAlbumIds;
		}

		private static IAlbum LoadRootAlbumForAnonymousUser(int galleryId, SecurityActions permissions)
		{
			IAlbum rootAlbum = null;

			// Anonymous user, not logged on. Get root album as long as it is public.
			IAlbum tmpRootAlbum = Factory.LoadRootAlbumInstance(galleryId);
			if (SecurityManager.IsUserAuthorized(permissions, null, tmpRootAlbum.Id, galleryId, false, tmpRootAlbum.IsPrivate, SecurityActionsOption.RequireOne))
			{
				rootAlbum = tmpRootAlbum;
			}

			if (rootAlbum == null)
			{
				try
				{
					// The user is not logged on and the root album is private or does not have the required permission, so create an empty album.
					rootAlbum = Factory.CreateEmptyAlbumInstance(galleryId);
					rootAlbum.IsVirtualAlbum = true;
					rootAlbum.Title = Resources.Virtual_Album_Title;
				}
				catch
				{
					if (rootAlbum != null)
						rootAlbum.Dispose();

					throw;
				}
				return rootAlbum;
			}

			return rootAlbum;
		}

		/// <overloads>
		/// Generate an inflated <see cref="IAlbum" /> instance with optionally inflated child media objects.
		/// </overloads>
		/// <summary>
		/// Generate an inflated <see cref="IAlbum" /> instance with optionally inflated child media objects. The album's <see cref="IAlbum.ThumbnailMediaObjectId" />
		/// property is set to its value from the data store, but the <see cref="IGalleryObject.Thumbnail" /> property is only inflated when accessed.
		/// </summary>
		/// <param name="album">The album whose properties should be overwritten with the values from the data store.</param>
		/// <param name="inflateChildMediaObjects">When true, the child media objects of the album are added and inflated.
		/// Child albums are added but not inflated. When false, they are not added or inflated.</param>
		/// <exception cref="InvalidAlbumException">Thrown when an album is not found in the data store that matches the 
		/// <see cref="IGalleryObject.Id">ID</see> of the album parameter.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="album" /> is null.</exception>
		/// <exception cref="InvalidOperationException">Thrown when <paramref name="inflateChildMediaObjects" /> is <c>false</c> and the album is inflated.</exception>
		public static void LoadAlbumInstance(IAlbum album, bool inflateChildMediaObjects)
		{
			if (album == null)
				throw new ArgumentNullException("album");

			if (album.IsInflated && !inflateChildMediaObjects)
				throw new InvalidOperationException(Resources.Factory_LoadAlbumInstance_Ex_Msg);

			#region Inflate the album, but only if it's not already inflated.

			if (!(album.IsInflated))
			{
				if (album.Id > int.MinValue)
				{
					InflateAlbumFromDto(album, GetDataProvider().Album_GetAlbumById(album.Id));

					// If the album's parent is an inflated, writeable instance, we want this album to be the same, so don't
					// insert into the cache (since that sets IsWritable to false).
					if (album.Parent.IsInflated && !album.Parent.IsWritable)
					{
						// OK, to put into cache, so do so if not already there.
						Dictionary<int, IAlbum> albumCache = (Dictionary<int, IAlbum>)HelperFunctions.GetCache(CacheItem.Albums) ?? new Dictionary<int, IAlbum>();

						if (!albumCache.ContainsKey(album.Id))
						{
							// The cache exists, but there is no item matching the desired album ID. Add to cache.
							lock (albumCache)
							{
								if (!albumCache.ContainsKey(album.Id))
								{
									album.IsWritable = false;
									albumCache.Add(album.Id, album);
									HelperFunctions.SetCache(CacheItem.Albums, albumCache);
								}
							}
						}
					}
				}

				if (!(album.Parent is NullGalleryObject))
				{
					album.AllowMetadataLoading = ((IAlbum)album.Parent).AllowMetadataLoading;
				}

				album.IsInflated = true;

				Debug.Assert(album.ThumbnailMediaObjectId > int.MinValue, "The album's ThumbnailMediaObjectId should have been assigned in this method.");

				// Since we've just loaded this object from the data store, set the corresponding property.
				album.FullPhysicalPathOnDisk = album.FullPhysicalPath;

				album.HasChanges = false;
			}

			#endregion

			#region Add child objects (CreateInstance)

			// Add child albums and objects, if they exist.
			if (inflateChildMediaObjects)
			{
				AddChildObjects(album);
			}

			#endregion

			if (!album.IsInflated)
				throw new InvalidAlbumException(album.Id);
		}

		/// <summary>
		/// Generate a read-only, inflated <see cref="IAlbum" /> instance with optionally inflated child media objects. Metadata 
		/// for media objects are automatically loaded. The album's <see cref="IAlbum.ThumbnailMediaObjectId" /> property is set 
		/// to its value from the data store, but the <see cref="IGalleryObject.Thumbnail" /> property is only inflated when 
		/// accessed. Guaranteed to not return null.
		/// </summary>
		/// <param name="albumId">The <see cref="IGalleryObject.Id">ID</see> that uniquely identifies the album to retrieve.</param>
		/// <param name="inflateChildMediaObjects">When true, the child media objects of the album are added and inflated.
		/// Child albums are added but not inflated. When false, they are not added or inflated.</param>
		/// <returns>Returns an inflated album instance with all properties set to the values from the data store.</returns>
		/// <exception cref="InvalidAlbumException">Thrown when an album with the specified <paramref name = "albumId" /> 
		/// is not found in the data store.</exception>
		public static IAlbum LoadAlbumInstance(int albumId, bool inflateChildMediaObjects)
		{
			return LoadAlbumInstance(albumId, inflateChildMediaObjects, false, true);
		}

		/// <summary>
		/// Generate an inflated <see cref="IAlbum" /> instance with optionally inflated child media objects. Metadata 
		/// for media objects are automatically loaded. Use the <paramref name="isWritable" /> parameter to specify a writeable, 
		/// thread-safe instance that can be modified and persisted to the data store. The 
		/// album's <see cref="IAlbum.ThumbnailMediaObjectId" /> property is set to its value from the data store, but the 
		/// <see cref="IGalleryObject.Thumbnail" /> property is only inflated when accessed. Guaranteed to not return null.
		/// </summary>
		/// <param name="albumId">The <see cref="IGalleryObject.Id">ID</see> that uniquely identifies the album to retrieve.</param>
		/// <param name="inflateChildMediaObjects">When true, the child media objects of the album are added and inflated.
		/// Child albums are added but not inflated. When false, they are not added or inflated.</param>
		/// <param name="isWritable">When set to <c>true</c> then return a unique instance that is not shared across threads.</param>
		/// <returns>Returns an inflated album instance with all properties set to the values from the data store.</returns>
		/// <exception cref="InvalidAlbumException">Thrown when an album with the specified <paramref name = "albumId" /> 
		/// is not found in the data store.</exception>
		public static IAlbum LoadAlbumInstance(int albumId, bool inflateChildMediaObjects, bool isWritable)
		{
			return LoadAlbumInstance(albumId, inflateChildMediaObjects, isWritable, true);
		}

		/// <summary>
		/// Generate an inflated <see cref="IAlbum" /> instance with optionally inflated child media objects, and optionally specifying
		/// whether to suppress the loading of media object metadata. Use the <paramref name="isWritable" />
		/// parameter to specify a writeable, thread-safe instance that can be modified and persisted to the data store. The 
		/// album's <see cref="IAlbum.ThumbnailMediaObjectId" /> property is set to its value from the data store, but the 
		/// <see cref="IGalleryObject.Thumbnail" /> property is only inflated when accessed. Guaranteed to not return null.
		/// </summary>
		/// <param name="albumId">The <see cref="IGalleryObject.Id">ID</see> that uniquely identifies the album to retrieve.</param>
		/// <param name="inflateChildMediaObjects">When true, the child media objects of the album are added and inflated.
		/// Child albums are added but not inflated. When false, they are not added or inflated.</param>
		/// <param name="isWritable">When set to <c>true</c> then return a unique instance that is not shared across threads.</param>
		/// <param name="allowMetadataLoading">If set to <c>false</c>, the metadata for media objects are not loaded.</param>
		/// <returns>Returns an inflated album instance with all properties set to the values from the data store.</returns>
		/// <exception cref="InvalidAlbumException">Thrown when an album with the specified <paramref name = "albumId" /> 
		/// is not found in the data store.</exception>
		public static IAlbum LoadAlbumInstance(int albumId, bool inflateChildMediaObjects, bool isWritable, bool allowMetadataLoading)
		{
			if (!isWritable && !allowMetadataLoading)
			{
				throw new ArgumentException("Invalid method call. Cannot call LoadAlbumInstance with isWritable and allowMetadataLoading both set to false, since this can cause objects to be stored in the cache with missing metadata.");
			}

			IAlbum album = (isWritable ? RetrieveAlbumFromDataStore(albumId) : RetrieveAlbum(albumId));

			album.AllowMetadataLoading = allowMetadataLoading;

			// Add child albums and objects, if they exist, and if needed.
			if ((inflateChildMediaObjects) && (!album.AreChildrenInflated))
			{
				AddChildObjects(album);
			}

			return album;
		}

		/// <summary>
		/// Returns an instance of an object that knows how to persist albums to the data store.
		/// </summary>
		/// <param name="albumObject">An <see cref="IAlbum" /> to which the save behavior applies.</param>
		/// <returns>Returns an object that implements <see cref="ISaveBehavior" />.</returns>
		public static ISaveBehavior GetAlbumSaveBehavior(IAlbum albumObject)
		{
			return new AlbumSaveBehavior(albumObject);
		}

		/// <summary>
		/// Returns an instance of an object that knows how to delete albums from the data store.
		/// </summary>
		/// <param name="albumObject">An <see cref="IAlbum" /> to which the delete behavior applies.</param>
		/// <returns>Returns an object that implements <see cref="IDeleteBehavior" />.</returns>
		public static IDeleteBehavior GetAlbumDeleteBehavior(IAlbum albumObject)
		{
			return new AlbumDeleteBehavior(albumObject);
		}

		#endregion

		#region Security Methods

		/// <summary>
		/// Create a Gallery Server Pro role corresponding to the specified parameters. Throws an exception if a role with the
		/// specified name already exists in the data store. The role is not persisted to the data store until the
		/// <see cref="IGalleryServerRole.Save"/> method is called.
		/// </summary>
		/// <param name="roleName">A string that uniquely identifies the role.</param>
		/// <param name="allowViewAlbumOrMediaObject">A value indicating whether the user assigned to this role has permission to view albums
		/// and media objects.</param>
		/// <param name="allowViewOriginalImage">A value indicating whether the user assigned to this role has permission to view the original,
		/// high resolution version of an image. This setting applies only to images. It has no effect if there are no
		/// high resolution images in the album or albums to which this role applies.</param>
		/// <param name="allowAddMediaObject">A value indicating whether the user assigned to this role has permission to add media objects to an album.</param>
		/// <param name="allowAddChildAlbum">A value indicating whether the user assigned to this role has permission to create child albums.</param>
		/// <param name="allowEditMediaObject">A value indicating whether the user assigned to this role has permission to edit a media object.</param>
		/// <param name="allowEditAlbum">A value indicating whether the user assigned to this role has permission to edit an album.</param>
		/// <param name="allowDeleteMediaObject">A value indicating whether the user assigned to this role has permission to delete media objects within an album.</param>
		/// <param name="allowDeleteChildAlbum">A value indicating whether the user assigned to this role has permission to delete child albums.</param>
		/// <param name="allowSynchronize">A value indicating whether the user assigned to this role has permission to synchronize an album.</param>
		/// <param name="allowAdministerSite">A value indicating whether the user has administrative permission for all albums. This permission
		/// automatically applies to all albums across all galleries; it cannot be selectively applied.</param>
		/// <param name="allowAdministerGallery">A value indicating whether the user has administrative permission for all albums. This permission
		/// automatically applies to all albums in a particular gallery; it cannot be selectively applied.</param>
		/// <param name="hideWatermark">A value indicating whether the user assigned to this role has a watermark applied to images.
		/// This setting has no effect if watermarks are not used. A true value means the user does not see the watermark;
		/// a false value means the watermark is applied.</param>
		/// <returns>
		/// Returns an <see cref="IGalleryServerRole"/> object corresponding to the specified parameters.
		/// </returns>
		/// <exception cref="InvalidGalleryServerRoleException">Thrown when a role with the specified role name already exists in the data store.</exception>
		public static IGalleryServerRole CreateGalleryServerRoleInstance(string roleName, bool allowViewAlbumOrMediaObject,
																																		 bool allowViewOriginalImage, bool allowAddMediaObject,
																																		 bool allowAddChildAlbum, bool allowEditMediaObject,
																																		 bool allowEditAlbum, bool allowDeleteMediaObject,
																																		 bool allowDeleteChildAlbum, bool allowSynchronize,
																																		 bool allowAdministerSite, bool allowAdministerGallery,
																																		 bool hideWatermark)
		{
			if (LoadGalleryServerRole(roleName) != null)
			{
				throw new InvalidGalleryServerRoleException(Resources.Factory_CreateGalleryServerRoleInstance_Ex_Msg);
			}

			return new GalleryServerRole(roleName, allowViewAlbumOrMediaObject, allowViewOriginalImage, allowAddMediaObject,
																	 allowAddChildAlbum, allowEditMediaObject, allowEditAlbum, allowDeleteMediaObject,
																	 allowDeleteChildAlbum, allowSynchronize, allowAdministerSite, allowAdministerGallery,
																	 hideWatermark);
		}

		/// <overloads>Retrieve a collection of Gallery Server roles.</overloads>
		/// <summary>
		/// Retrieve a collection of all Gallery Server roles. The roles may be returned from a cache. Guaranteed to not return null.
		/// </summary>
		/// <returns>Returns an <see cref="IGalleryServerRoleCollection" /> object that contains all Gallery Server roles.</returns>
		/// <remarks>
		/// The collection of all Gallery Server roles are stored in a cache to improve
		/// performance. <note type = "implementnotes">Note to developer: Any code that modifies the roles in the data store should purge the cache so 
		///              	that they can be freshly retrieved from the data store during the next request. The cache is identified by the
		///              	<see cref="CacheItem.GalleryServerRoles" /> enum.</note>
		/// </remarks>
		[DataObjectMethod(DataObjectMethodType.Select)]
		public static IGalleryServerRoleCollection LoadGalleryServerRoles()
		{
			Dictionary<string, IGalleryServerRoleCollection> rolesCache = (Dictionary<string, IGalleryServerRoleCollection>)HelperFunctions.GetCache(CacheItem.GalleryServerRoles);

			IGalleryServerRoleCollection roles;

			if ((rolesCache != null) && (rolesCache.TryGetValue(GlobalConstants.GalleryServerRoleAllRolesCacheKey, out roles)))
			{
				return roles;
			}

			// No roles in the cache, so get from data store and add to cache.
			roles = GetGalleryServerRolesFromDataStore();

			roles.Sort();

			roles.ValidateIntegrity();

			rolesCache = new Dictionary<string, IGalleryServerRoleCollection>();
			rolesCache.Add(GlobalConstants.GalleryServerRoleAllRolesCacheKey, roles);
			HelperFunctions.SetCache(CacheItem.GalleryServerRoles, rolesCache);

			return roles;
		}

		/// <summary>
		/// Retrieve a collection of Gallery Server roles that match the specified <paramref name = "roleNames" />. 
		/// It is not case sensitive, so that "ReadAll" matches "readall". The roles may be returned from a cache.
		///  Guaranteed to not return null.
		/// </summary>
		/// <param name="roleNames">The name of the roles to return.</param>
		/// <returns>
		/// Returns an <see cref="IGalleryServerRoleCollection" /> object that contains all Gallery Server roles that
		/// match the specified role names.
		/// </returns>
		/// <remarks>
		/// The collection of all Gallery Server roles for the current gallery are stored in a cache to improve
		/// performance. <note type = "implementnotes">Note to developer: Any code that modifies the roles in the data store should purge the cache so 
		///              	that they can be freshly retrieved from the data store during the next request. The cache is identified by the
		///              	<see cref="CacheItem.GalleryServerRoles" /> enum.</note>
		/// </remarks>
		[DataObjectMethod(DataObjectMethodType.Select)]
		public static IGalleryServerRoleCollection LoadGalleryServerRoles(IEnumerable<string> roleNames)
		{
			return LoadGalleryServerRoles().GetRoles(roleNames);
		}

		/// <overloads>
		/// Retrieve the Gallery Server role that matches the specified role name. The role may be returned from a cache.
		/// Returns null if no matching role is found.
		/// </overloads>
		/// <summary>
		/// Retrieve the Gallery Server role that matches the specified role name. The role may be returned from a cache.
		/// Returns null if no matching role is found.
		/// </summary>
		/// <param name="roleName">The name of the role to return.</param>
		/// <returns>
		/// Returns an <see cref="IGalleryServerRole" /> object that matches the specified role name, or null if no matching role is found.
		/// </returns>
		[DataObjectMethod(DataObjectMethodType.Select)]
		public static IGalleryServerRole LoadGalleryServerRole(string roleName)
		{
			return LoadGalleryServerRole(roleName, false);
		}

		/// <summary>
		/// Retrieve the Gallery Server role that matches the specified role name. When <paramref name="isWritable"/>
		/// is <c>true</c>, then return a unique instance that is not shared across threads, thus creating a thread-safe object that can
		/// be updated and persisted back to the data store. Calling this method with <paramref name="isWritable"/> set to <c>false</c>
		/// is the same as calling the overload of this method that takes only a role name. Returns null if no matching role is found.
		/// </summary>
		/// <param name="roleName">The name of the role to return.</param>
		/// <param name="isWritable">When set to <c>true</c> then return a unique instance that is not shared across threads.</param>
		/// <returns>
		/// Returns a writeable instance of <see cref="IGalleryServerRole"/> that matches the specified role name, or null if no matching role is found.
		/// </returns>
		public static IGalleryServerRole LoadGalleryServerRole(string roleName, bool isWritable)
		{
			IGalleryServerRole role = LoadGalleryServerRoles().GetRole(roleName);

			if ((role == null) || (!isWritable))
			{
				return role;
			}
			else
			{
				return role.Copy();
			}
		}

		#endregion

		#region AppError Methods

		/// <summary>
		/// Gets a collection of all application errors from the data store. The items are sorted in descending order on the
		/// <see cref="IAppError.Timestamp" /> property, so the most recent error is first. Returns an empty collection if no
		/// errors exist.
		/// </summary>
		/// <returns>Returns a collection of all application errors from the data store.</returns>
		public static IAppErrorCollection GetAppErrors()
		{
			IAppErrorCollection appErrors = (IAppErrorCollection)HelperFunctions.GetCache(CacheItem.AppErrors);

			if (appErrors != null)
			{
				return appErrors;
			}

			// No errors in the cache, so get from data store and add to cache.
			appErrors = Error.GetAppErrors();

			HelperFunctions.SetCache(CacheItem.AppErrors, appErrors);

			return appErrors;
		}

		#endregion

		#region Gallery and Gallery Setting Methods

		/// <summary>
		/// Loads the gallery specified by the <paramref name = "galleryId" />. Throws a <see cref="InvalidGalleryException" /> if no matching 
		/// gallery is found.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <returns>Returns an instance of <see cref="IGallery" /> containing information about the gallery.</returns>
		/// <exception cref="InvalidGalleryException">Thrown when no gallery matching <paramref name="galleryId" /> exists in the data store.</exception>
		public static IGallery LoadGallery(int galleryId)
		{
			if (_galleries.Count == 0)
			{
				Factory.LoadGalleries();
			}

			IGallery gallery;
			lock (_galleries)
			{
				gallery = _galleries.FindById(galleryId);
			}

			if (gallery == null)
			{
				// When another application instance creates a gallery, this function might try to load a gallery that doesn't exist in our
				// static variable. Reload all data from the database and try again. Specifically, this can happen when a second DotNetNuke
				// portal creates a gallery, which causes one or more roles to be associated with it. If the first portal then loads that role,
				// the role instantiation tries to load the gallery.
				ClearAllCaches();

				gallery = LoadGalleries().FindById(galleryId);

				if (gallery == null)
				{
					throw new InvalidGalleryException(galleryId);
				}
			}

			return gallery;
		}

		/// <summary>
		/// Gets a list of all the galleries in the current application. The returned value is a deep copy of a value stored
		/// in a static variable and is therefore threadsafe. Guaranteed to not be null.
		/// </summary>
		/// <returns>Returns a <see cref="IGalleryCollection" /> representing the galleries in the current application.</returns>
		public static IGalleryCollection LoadGalleries()
		{
			lock (_galleries)
			{
				if (_galleries.Count == 0)
				{
					// Ensure that writes related to instantiation are flushed.
					System.Threading.Thread.MemoryBarrier();

					GetDataProvider().Gallery_GetGalleries(_galleries);
				}
			}

			return _galleries.Copy();
		}


		/// <overloads>
		///		Loads the gallery settings for the gallery specified by <paramref name = "galleryId" />.
		/// </overloads>
		/// <summary>
		/// Loads a read-only instance of gallery settings for the gallery specified by <paramref name = "galleryId" />. Automatically 
		///		creates the gallery and	gallery settings if the data is not found in the data store. Guaranteed to not return null, except 
		///		for when <paramref name = "galleryId" /> is <see cref="Int32.MinValue" />, in which case it throws an <see cref="ArgumentOutOfRangeException" />.
		///		The returned value is a static instance that is shared across threads, so it should be used only for read-only access. Use
		///		a different overload of this method to return a writeable copy of the instance. Calling this method is the same as calling
		///		the overloaded method with the isWritable parameter set to false.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <returns>Returns a read-only instance of <see cref="IGallerySettings" />containing  the gallery settings for the gallery specified by 
		/// <paramref name = "galleryId" />. This is a reference to a static variable that may be shared across threads.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the gallery ID is <see cref="Int32.MinValue" />.</exception>
		public static IGallerySettings LoadGallerySetting(int galleryId)
		{
			if (galleryId == int.MinValue)
			{
				throw new ArgumentOutOfRangeException("galleryId", String.Format(CultureInfo.CurrentCulture, "The gallery ID must be a valid ID. Instead, the value passed was {0}.", galleryId));
			}

			IGallerySettingsCollection gallerySettings = LoadGallerySettings();

			IGallerySettings gs = gallerySettings.FindByGalleryId(galleryId);

			if (gs == null || (!gs.IsInitialized))
			{
				// There isn't an item for the requested gallery ID, *OR* there is an item but it hasn't been initialized (this
				// can happen when an error occurs during initialization, such as a CannotWriteToDirectoryException occurring when checking
				// the media object path).

				// If we didn't find a gallery, create it.
				if (gs == null)
				{
					IGallery gallery = Factory.CreateGalleryInstance();
					gallery.GalleryId = galleryId;
					GetDataProvider().Gallery_Configure(gallery);

					_galleries.Clear();

					// Need to clear the gallery server roles so that they are reloaded from the data store, which should now include sys admin
					// permission to the new gallery.
					HelperFunctions.PurgeCache();
				}

				// Reload the data from the data store.
				_gallerySettings.Clear();
				gallerySettings = LoadGallerySettings();

				gs = gallerySettings.FindByGalleryId(galleryId);

				if (gs == null)
				{
					throw new BusinessException(String.Format(CultureInfo.CurrentCulture, "Factory.LoadGallerySetting() should have created gallery setting records for gallery {0}, but it has not.", galleryId));
				}
			}

			return gs;
		}

		/// <summary>
		/// Loads the gallery settings for the gallery specified by <paramref name="galleryId"/>. When <paramref name="isWritable"/>
		/// is <c>true</c>, then return a unique instance that is not shared across threads, thus creating a thread-safe object that can
		/// be updated and persisted back to the data store. Calling this method with <paramref name="isWritable"/> set to <c>false</c>
		/// is the same as calling the overload of this method that takes only a gallery ID. Guaranteed to not return null, except for when <paramref name="galleryId"/>
		/// is <see cref="Int32.MinValue"/>, in which case it throws an <see cref="ArgumentOutOfRangeException"/>.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <param name="isWritable">When set to <c>true</c> then return a unique instance that is not shared across threads.</param>
		/// <returns>
		/// Returns a writeable instance of <see cref="IGallerySettings"/>containing  the gallery settings for the gallery specified by
		/// <paramref name="galleryId"/>.
		/// </returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the gallery ID is <see cref="Int32.MinValue"/>.</exception>
		public static IGallerySettings LoadGallerySetting(int galleryId, bool isWritable)
		{
			if (galleryId == int.MinValue)
			{
				throw new ArgumentOutOfRangeException("galleryId", String.Format(CultureInfo.CurrentCulture, "The gallery ID must be a valid ID. Instead, the value passed was {0}.", galleryId));
			}

			if (isWritable)
			{
				IGallerySettings gallerySettings = GallerySettings.RetrieveGallerySettingsFromDataStore().FindByGalleryId(galleryId);
				gallerySettings.IsWritable = true;
				return gallerySettings;
			}
			else
			{
				return LoadGallerySetting(galleryId);
			}
		}

		/// <summary>
		/// Loads the settings for all galleries in the application. Guaranteed to not return null.
		/// </summary>
		/// <returns>Returns an <see cref="IGallerySettingsCollection" /> containing settings for all galleries in the application.</returns>
		public static IGallerySettingsCollection LoadGallerySettings()
		{
			lock (_gallerySettings)
			{
				if (_gallerySettings.Count == 0)
				{
					// Ensure that writes related to instantiation are flushed.
					System.Threading.Thread.MemoryBarrier();

					_gallerySettings.AddRange(GallerySettings.RetrieveGallerySettingsFromDataStore());
				}
			}

			return _gallerySettings;
		}

		/// <summary>
		/// Loads the settings for all galleries in the application. Guaranteed to not return null.
		/// </summary>
		/// <returns>Returns an <see cref="IGallerySettingsCollection" /> containing settings for all galleries in the application.</returns>
		public static IGalleryControlSettingsCollection LoadGalleryControlSettings()
		{
			if (_galleryControlSettings == null)
			{
				_galleryControlSettings = new GalleryControlSettingsCollection();

				lock (_galleryControlSettings)
				{
					if (_galleryControlSettings.Count == 0)
					{
						// Ensure that writes related to instantiation are flushed.
						System.Threading.Thread.MemoryBarrier();

						_galleryControlSettings.AddRange(GalleryControlSettings.RetrieveGalleryControlSettingsFromDataStore());
					}
				}
			}

			return _galleryControlSettings;
		}

		/// <summary>
		/// Clears all in-memory representations of data.
		/// </summary>
		public static void ClearAllCaches()
		{
			ClearGalleryControlSettingsCache();
			ClearWatermarkCache();
			HelperFunctions.PurgeCache();
		}

		/// <summary>
		/// Clears the in-memory copy of the current set of gallery control settings. This will force a database retrieval the next time
		/// they are requested.
		/// </summary>
		public static void ClearGalleryCache()
		{
			_galleries.Clear();
		}

		/// <summary>
		/// Clears the in-memory copy of the current set of gallery control settings. This will force a database retrieval the next time
		/// they are requested.
		/// </summary>
		public static void ClearGalleryControlSettingsCache()
		{
			_galleryControlSettings = null;
		}

		/// <summary>
		/// Clears the in-memory copy of the current set of watermarks.
		/// </summary>
		public static void ClearWatermarkCache()
		{
			_watermarks.Clear();
		}

		/// <overloads>Loads the gallery control settings for the specified <paramref name="controlId"/>.</overloads>
		/// <summary>
		/// Loads the gallery control settings for the specified <paramref name="controlId"/>.
		/// </summary>
		/// <param name="controlId">The value that uniquely identifies the control containing the gallery. Example: "Default.aspx|gsp"</param>
		/// <returns>
		/// Returns an instance of <see cref="IGalleryControlSettings"/>containing  the gallery control settings for the gallery 
		/// control specified by <paramref name="controlId"/>.
		/// </returns>
		public static IGalleryControlSettings LoadGalleryControlSetting(string controlId)
		{
			return LoadGalleryControlSetting(controlId, false);
		}

		/// <summary>
		/// Loads the gallery control settings for the specified <paramref name="controlId"/>. When <paramref name="isWritable"/>
		/// is <c>true</c>, then return a unique instance that is not shared across threads, thus creating a thread-safe object that can
		/// be updated and persisted back to the data store. Calling this method with <paramref name="isWritable"/> set to <c>false</c>
		/// is the same as calling the overload of this method that takes only a control ID. Guaranteed to not return null.
		/// </summary>
		/// <param name="controlId">The value that uniquely identifies the control containing the gallery. Example: "Default.aspx|gsp"</param>
		/// <param name="isWritable">When set to <c>true</c> then return a unique instance that is not shared across threads.</param>
		/// <returns>
		/// Returns a writeable instance of <see cref="IGalleryControlSettings"/>containing  the gallery control settings for the gallery 
		/// control specified by <paramref name="controlId"/>.
		/// </returns>
		public static IGalleryControlSettings LoadGalleryControlSetting(string controlId, bool isWritable)
		{
			IGalleryControlSettings galleryControlSettings;

			if (isWritable)
			{
				galleryControlSettings = GalleryControlSettings.RetrieveGalleryControlSettingsFromDataStore().FindByControlId(controlId);
			}
			else
			{
				galleryControlSettings = LoadGalleryControlSettings().FindByControlId(controlId);
			}

			if (galleryControlSettings == null)
			{
				galleryControlSettings = new GalleryControlSettings(int.MinValue, controlId);
			}

			return galleryControlSettings;
		}

		/// <summary>
		/// Gets the watermark instance for the specified <paramref name="galleryId" />.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <returns>Returns a <see cref="Watermark" /> instance for the specified <paramref name="galleryId" />.</returns>
		public static Watermark GetWatermarkInstance(int galleryId)
		{
			if (galleryId == int.MinValue)
			{
				throw new ArgumentOutOfRangeException("galleryId", String.Format(CultureInfo.CurrentCulture, "The gallery ID must be a valid ID. Instead, the value passed was {0}.", galleryId));
			}

			Watermark watermark;

			if (!_watermarks.TryGetValue(galleryId, out watermark))
			{
				lock (_watermarks)
				{
					if (!_watermarks.TryGetValue(galleryId, out watermark))
					{
						// A watermark object for the gallery was not found. Create it and add it to the dictionary.
						Watermark tempWatermark = AppSetting.Instance.License.IsInReducedFunctionalityMode ? Watermark.GetReducedFunctionalityModeWatermark(galleryId) : Watermark.GetUserSpecifiedWatermark(galleryId);

						// Ensure that writes related to instantiation are flushed.
						System.Threading.Thread.MemoryBarrier();

						_watermarks.Add(galleryId, tempWatermark);

						watermark = tempWatermark;
					}
				}
			}

			return watermark;
		}

		#endregion

		#region Data Access

		/// <summary>
		/// Gets the data provider for Gallery Server Pro. The provider contains functionality for interacting with the data store.
		/// </summary>
		/// <returns>Returns an <see cref="GalleryServerPro.Provider.Interfaces.IDataProvider" /> object.</returns>
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		public static IDataProvider GetDataProvider()
		{
			return DataProviderManager.Provider;
		}

		/// <summary>
		/// Gets the specified data provider for Gallery Server Pro. The provider contains functionality for interacting with the data store.
		/// </summary>
		/// <param name="providerName">Name of the provider.</param>
		/// <returns>
		/// Returns an <see cref="GalleryServerPro.Provider.Interfaces.IDataProvider"/> object.
		/// </returns>
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		public static IDataProvider GetDataProvider(GalleryDataProvider providerName)
		{
			return DataProviderManager.Providers[providerName.ToString()];
		}

		#endregion

		#region General

		/// <summary>
		/// Gets an instance of the HTML validator.
		/// </summary>
		/// <param name="html">The HTML to pass to the HTML validator.</param>
		/// <param name="galleryId">The gallery ID. This is used to look up the appropriate configuration values for the gallery.</param>
		/// <returns>Returns an instance of the HTML validator.</returns>
		public static IHtmlValidator GetHtmlValidator(string html, int galleryId)
		{
			return HtmlValidator.Create(html, galleryId);
		}

		/// <summary>
		/// Retrieves a singleton object that represents the current state of a synchronization in the specified gallery. Each gallery uses the
		/// same instance, so any callers must use appropriate locking when updating the object. Guaranteed to not return null.
		/// Note that the properties are NOT updated with the latest values from the data store; to do this call 
		/// <see cref="SynchronizationStatus.RefreshFromDataStore" />.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <returns>Returns an instance of <see cref="ISynchronizationStatus" /> that represents the current state of a 
		/// synchronization in a particular gallery.</returns>
		public static ISynchronizationStatus LoadSynchronizationStatus(int galleryId)
		{
			ISynchronizationStatus syncStatus;

			lock (_sharedLock)
			{
				if (!_syncStatuses.TryGetValue(galleryId, out syncStatus))
				{
					// There is no item matching the desired gallery ID. Create a new one and add to the dictionary.
					syncStatus = new SynchronizationStatus(galleryId);

					if (!_syncStatuses.ContainsKey(galleryId))
					{
						_syncStatuses.Add(galleryId, syncStatus);
					}
				}
			}

			return syncStatus;
		}

		/// <summary>
		/// Gets a list of the browser templates in the current application. The returned value is a static variable and is therefore
		/// not thread safe for updates; it should be considered read-only.
		/// </summary>
		/// <returns>Returns a <see cref="IBrowserTemplateCollection" /> representing the browser templates in the current application.</returns>
		public static IBrowserTemplateCollection LoadBrowserTemplates()
		{
			lock (_browserTemplates)
			{
				if (_browserTemplates.Count == 0)
				{
					// Ensure that writes related to instantiation are flushed.
					System.Threading.Thread.MemoryBarrier();

					GetDataProvider().MimeType_GetBrowserTemplates(_browserTemplates);
				}
			}

			return _browserTemplates;
		}

		#endregion

		#region Profile Methods

		/// <summary>
		/// Retrieves the profile for the specified <paramref name="userName" />. The profile is 
		/// retrieved from the cache if it is there. If not, it is retrieved from the data store and
		/// added to the cache. Guaranteed to not return null.
		/// </summary>
		/// <param name="userName">Name of the user.</param>
		/// <returns>Returns the profile for the specified <paramref name="userName" /></returns>
		public static IUserProfile LoadUserProfile(string userName)
		{
			Dictionary<string, IUserProfile> profileCache = (Dictionary<string, IUserProfile>)HelperFunctions.GetCache(CacheItem.Profiles);

			IUserProfile profile;
			if ((profileCache == null) || !profileCache.TryGetValue(userName, out profile))
			{
				profile = GetDataProvider().Profile_GetUserProfile(userName, new Factory());

				// Add profile to cache.
				if (profileCache == null)
					profileCache = new Dictionary<string, IUserProfile>();

				profileCache.Add(userName, profile);

				HelperFunctions.SetCache(CacheItem.Profiles, profileCache);
			}

			return profile;
		}

		/// <summary>
		/// Persist the user profile to the data store. The profile cache is cleared.
		/// </summary>
		/// <param name="userProfile">The user profile.</param>
		public static void SaveUserProfile(IUserProfile userProfile)
		{
			GetDataProvider().Profile_Save(userProfile);
			HelperFunctions.RemoveCache(CacheItem.Profiles);
		}

		/// <summary>
		/// Permanently delete the profile records for the specified <paramref name="userName" />.
		/// The profile cache is cleared.
		/// </summary>
		/// <param name="userName">Name of the user.</param>
		public static void DeleteUserProfile(string userName)
		{
			GetDataProvider().Profile_DeleteProfileForUser(userName);
			HelperFunctions.RemoveCache(CacheItem.Profiles);
		}

		#endregion

		#region Private Static Methods

		/// <summary>
		/// Gets the album from the data transfer object. Guaranteed to not return null.
		/// </summary>
		/// <param name="albumDto">The album data transfer object.</param>
		/// <returns>Returns an <see cref="IAlbum" />.</returns>
		/// <exception cref="InvalidAlbumException">Thrown when <paramref name="albumDto" /> is null.</exception>
		private static IAlbum GetAlbumFromDto(AlbumDto albumDto)
		{
			if (albumDto == null)
			{
				throw new InvalidAlbumException();
			}

			IAlbum album = null;
			try
			{
				album = new Album(albumDto.AlbumId,
													albumDto.FKGalleryId,
													albumDto.AlbumParentId,
													albumDto.Title,
													albumDto.DirectoryName,
													albumDto.Summary,
													albumDto.ThumbnailMediaObjectId,
													albumDto.Seq,
													HelperFunctions.ToDateTime(albumDto.DateStart),
													HelperFunctions.ToDateTime(albumDto.DateEnd),
													albumDto.CreatedBy.Trim(),
													HelperFunctions.ToDateTime(albumDto.DateAdded),
													albumDto.LastModifiedBy.Trim(),
													HelperFunctions.ToDateTime(albumDto.DateLastModified),
													albumDto.OwnedBy.Trim(),
													albumDto.OwnerRoleName.Trim(),
													albumDto.IsPrivate);

				album.IsInflated = true;
			}
			catch
			{
				if (album != null)
					album.Dispose();

				throw;
			}

			return album;
		}

		/// <summary>
		/// Retrieve the specified album as a read-only instance. Child albums and media objects are not added. The album is 
		/// retrieved from the cache if it is there. If not, it is retrieved from the data store. Guaranteed to not return null.
		/// </summary>
		/// <param name="albumId">The <see cref="IGalleryObject.Id">ID</see> that uniquely identifies the album to retrieve.</param>
		/// <returns>Returns the specified album as a read-only instance without child albums or media objects.</returns>
		/// <exception cref="InvalidAlbumException">Thrown when an album with the specified <paramref name = "albumId" /> 
		/// is not found in the data store.</exception>
		private static IAlbum RetrieveAlbum(int albumId)
		{
			Dictionary<int, IAlbum> albumCache = (Dictionary<int, IAlbum>)HelperFunctions.GetCache(CacheItem.Albums);

			IAlbum album;
			if (albumCache != null)
			{
				if (!albumCache.TryGetValue(albumId, out album))
				{
					// The cache exists, but there is no item matching the desired album ID. Retrieve from data store and add to cache.
					album = RetrieveAlbumFromDataStore(albumId);

					lock (albumCache)
					{
						if (!albumCache.ContainsKey(albumId))
						{
							album.IsWritable = false;
							albumCache.Add(albumId, album);
							HelperFunctions.SetCache(CacheItem.Albums, albumCache);
						}
					}
				}
#if DEBUG
				//else
				//{
				//  Trace.WriteLine(String.Format(CultureInfo.CurrentCulture, "Album {0} retrieved from cache. (AreChildrenInflated={1})", albumId, album.AreChildrenInflated));
				//  Trace.WriteLine(String.Format(CultureInfo.CurrentCulture, "Album {0} has {1} child objects.", albumId, album.GetChildGalleryObjects().Count));
				//}
#endif
			}
			else
			{
				// There is no cache item. Retrieve from data store and create cache item so it's there next time we want it.
				album = RetrieveAlbumFromDataStore(albumId);
				album.IsWritable = false;

				albumCache = new Dictionary<int, IAlbum>();
				albumCache.Add(albumId, album);

				HelperFunctions.SetCache(CacheItem.Albums, albumCache);

#if DEBUG
				//Trace.WriteLine(String.Format(CultureInfo.CurrentCulture, "Album {0} added to cache. (AreChildrenInflated={1})", albumId, album.AreChildrenInflated));
#endif
			}
			return album;
		}

		/// <summary>
		/// Retrieve the specified media object. It is retrieved from the cache if it is there. 
		/// If not, it is retrieved from the data store.
		/// </summary>
		/// <param name="mediaObjectId">The <see cref="IGalleryObject.Id">ID</see> that uniquely identifies the media object to retrieve.</param>
		/// <returns>Returns the specified media object.</returns>
		/// <exception cref="InvalidMediaObjectException">Thrown when
		/// an image is not found in the data store that matches the mediaObjectId parameter and the current gallery.</exception>
		private static IGalleryObject RetrieveMediaObject(int mediaObjectId)
		{
			return RetrieveMediaObject(mediaObjectId, GalleryObjectType.Unknown, null);
		}

		/// <summary>
		/// Retrieve the specified media object. It is retrieved from the cache if it is there. 
		/// If not, it is retrieved from the data store.
		/// </summary>
		/// <param name="mediaObjectId">The ID that uniquely identifies the media object to retrieve.</param>
		/// <param name="galleryObjectType">The type of gallery object that the mediaObjectId parameter represents. If the type is 
		/// unknown, the Unknown enum value can be specified. Specify the actual type if possible (e.g. Video, Audio, Image, 
		/// etc.), as it is more efficient. An exception is thrown if the Album enum value is specified, since this method
		/// is designed only for media objects.</param>
		/// <returns>Returns the specified media object.</returns>
		/// <exception cref="InvalidMediaObjectException">Thrown when
		/// an image is not found in the data store that matches the mediaObjectId parameter and the current gallery.</exception>
		private static IGalleryObject RetrieveMediaObject(int mediaObjectId, GalleryObjectType galleryObjectType)
		{
			return RetrieveMediaObject(mediaObjectId, galleryObjectType, null);
		}

		/// <summary>
		/// Retrieve the specified media object. It is retrieved from the cache if it is there. 
		/// If not, it is retrieved from the data store.
		/// </summary>
		/// <param name="mediaObjectId">The ID that uniquely identifies the media object to retrieve.</param>
		/// <param name="galleryObjectType">The type of gallery object that the mediaObjectId parameter represents. If the type is 
		/// unknown, the Unknown enum value can be specified. Specify the actual type if possible (e.g. Video, Audio, Image, 
		/// etc.), as it is more efficient. An exception is thrown if the Album enum value is specified, since this method
		/// is designed only for media objects.</param>
		/// <param name="parentAlbum">The album containing the media object specified by mediaObjectId. Specify
		/// null if a reference to the album is not available, and it will be created based on the parent album
		/// specified in the data store.</param>
		/// <returns>Returns the specified media object.</returns>
		/// <exception cref="InvalidMediaObjectException">Thrown when
		/// an image is not found in the data store that matches the mediaObjectId parameter and the current gallery.</exception>
		private static IGalleryObject RetrieveMediaObject(int mediaObjectId, GalleryObjectType galleryObjectType, IAlbum parentAlbum)
		{
			// <exception cref="InvalidAlbumException">Thrown when an 
			// album with the specified album ID is not found in the data store.</exception>
			Dictionary<int, IGalleryObject> mediaObjectCache = (Dictionary<int, IGalleryObject>)HelperFunctions.GetCache(CacheItem.MediaObjects);

			IGalleryObject mediaObject;
			if (mediaObjectCache != null)
			{
				if (!mediaObjectCache.TryGetValue(mediaObjectId, out mediaObject))
				{
					// The cache exists, but there is no item matching the desired media object ID. Retrieve from data store and add to cache.
					mediaObject = RetrieveMediaObjectFromDataStore(mediaObjectId, galleryObjectType, parentAlbum);

					AddToMediaObjectCache(mediaObject);
				}
#if DEBUG
				//else
				//{
				//  Trace.WriteLine(String.Format(CultureInfo.CurrentCulture, "Media object {0} retrieved from cache.", mediaObjectId));
				//}
#endif
			}
			else
			{
				// There is no cache item. Retrieve from data store and create cache item so it's there next time we want it.
				mediaObject = RetrieveMediaObjectFromDataStore(mediaObjectId, galleryObjectType, parentAlbum);

				AddToMediaObjectCache(mediaObject);
			}

			return mediaObject;
		}

		private static IGalleryObject RetrieveMediaObjectFromDataStore(int id, GalleryObjectType galleryObjectType, IAlbum parentAlbum)
		{
			// If the gallery object type is vague, we need to figure it out.
			if ((galleryObjectType == GalleryObjectType.All) || (galleryObjectType == GalleryObjectType.None) || (galleryObjectType == GalleryObjectType.Unknown))
			{
				galleryObjectType = HelperFunctions.DetermineGalleryObjectType(id);
			}

			IGalleryObject go;

			switch (galleryObjectType)
			{
				case GalleryObjectType.Image:
					{
						go = RetrieveImageFromDataStore(id, parentAlbum);
						break;
					}
				case GalleryObjectType.Video:
					{
						go = RetrieveVideoFromDataStore(id, parentAlbum);
						break;
					}
				case GalleryObjectType.Audio:
					{
						go = RetrieveAudioFromDataStore(id, parentAlbum);
						break;
					}
				case GalleryObjectType.External:
					{
						go = RetrieveExternalFromDataStore(id, parentAlbum);
						break;
					}
				case GalleryObjectType.Generic:
				case GalleryObjectType.Unknown:
					{
						go = RetrieveGenericMediaObjectFromDataStore(id, parentAlbum);
						break;
					}
				default:
					{
						throw new InvalidMediaObjectException(id);
					}
			}

			return go;
		}

		private static IGalleryObject RetrieveImageFromDataStore(int mediaObjectId, IAlbum parentAlbum)
		{
			bool includeMetadata = (parentAlbum == null || parentAlbum.AllowMetadataLoading);

			MediaObjectDto moDto = GetDataProvider().MediaObject_GetMediaObjectById(mediaObjectId, includeMetadata);

			if (moDto == null)
			{
				throw new InvalidMediaObjectException(mediaObjectId);
			}

			if (parentAlbum == null)
			{
				parentAlbum = Factory.LoadAlbumInstance(moDto.FKAlbumId, false);
			}

			IGalleryObject mo = new Image(
				moDto.MediaObjectId,
				parentAlbum,
				moDto.Title.Trim(),
				moDto.HashKey.Trim(),
				moDto.ThumbnailFilename,
				moDto.ThumbnailWidth,
				moDto.ThumbnailHeight,
				moDto.ThumbnailSizeKB,
				moDto.OptimizedFilename.Trim(),
				moDto.OptimizedWidth,
				moDto.OptimizedHeight,
				moDto.OptimizedSizeKB,
				moDto.OriginalFilename.Trim(),
				moDto.OriginalWidth,
				moDto.OriginalHeight,
				moDto.OriginalSizeKB,
				moDto.Seq,
				moDto.CreatedBy.Trim(),
				moDto.DateAdded,
				moDto.LastModifiedBy.Trim(),
				HelperFunctions.ToDateTime(moDto.DateLastModified),
				moDto.IsPrivate,
				true,
				null);

			AddMediaObjectMetadata(mo, moDto);

			return mo;
		}

		private static IGalleryObject RetrieveVideoFromDataStore(int mediaObjectId, IAlbum parentAlbum)
		{
			bool includeMetadata = (parentAlbum == null || parentAlbum.AllowMetadataLoading);

			MediaObjectDto moDto = GetDataProvider().MediaObject_GetMediaObjectById(mediaObjectId, includeMetadata);

			if (moDto == null)
			{
				throw new InvalidMediaObjectException(mediaObjectId);
			}

			if (parentAlbum == null)
			{
				parentAlbum = Factory.LoadAlbumInstance(moDto.FKAlbumId, false);
			}

			IGalleryObject mo = new Video(
				moDto.MediaObjectId,
				parentAlbum,
				moDto.Title.Trim(),
				moDto.HashKey.Trim(),
				moDto.ThumbnailFilename,
				moDto.ThumbnailWidth,
				moDto.ThumbnailHeight,
				moDto.ThumbnailSizeKB,
				moDto.OptimizedFilename.Trim(),
				moDto.OptimizedWidth,
				moDto.OptimizedHeight,
				moDto.OptimizedSizeKB,
				moDto.OriginalFilename.Trim(),
				moDto.OriginalWidth,
				moDto.OriginalHeight,
				moDto.OriginalSizeKB,
				moDto.Seq,
				moDto.CreatedBy.Trim(),
				moDto.DateAdded,
				moDto.LastModifiedBy.Trim(),
				HelperFunctions.ToDateTime(moDto.DateLastModified),
				moDto.IsPrivate,
				true,
				null);

			AddMediaObjectMetadata(mo, moDto);

			return mo;
		}

		private static IGalleryObject RetrieveAudioFromDataStore(int mediaObjectId, IAlbum parentAlbum)
		{
			bool includeMetadata = (parentAlbum == null || parentAlbum.AllowMetadataLoading);

			MediaObjectDto moDto = GetDataProvider().MediaObject_GetMediaObjectById(mediaObjectId, includeMetadata);

			if (moDto == null)
			{
				throw new InvalidMediaObjectException(mediaObjectId);
			}

			if (parentAlbum == null)
			{
				parentAlbum = Factory.LoadAlbumInstance(moDto.FKAlbumId, false);
			}

			IGalleryObject mo = new Audio(
				moDto.MediaObjectId,
				parentAlbum,
				moDto.Title.Trim(),
				moDto.HashKey.Trim(),
				moDto.ThumbnailFilename,
				moDto.ThumbnailWidth,
				moDto.ThumbnailHeight,
				moDto.ThumbnailSizeKB,
				moDto.OptimizedFilename.Trim(),
				moDto.OptimizedWidth,
				moDto.OptimizedHeight,
				moDto.OptimizedSizeKB,
				moDto.OriginalFilename.Trim(),
				moDto.OriginalWidth,
				moDto.OriginalHeight,
				moDto.OriginalSizeKB,
				moDto.Seq,
				moDto.CreatedBy.Trim(),
				moDto.DateAdded,
				moDto.LastModifiedBy.Trim(),
				HelperFunctions.ToDateTime(moDto.DateLastModified),
				moDto.IsPrivate,
				true,
				null);

			AddMediaObjectMetadata(mo, moDto);

			return mo;
		}

		private static IGalleryObject RetrieveExternalFromDataStore(int mediaObjectId, IAlbum parentAlbum)
		{
			bool includeMetadata = (parentAlbum == null || parentAlbum.AllowMetadataLoading);

			MediaObjectDto moDto = GetDataProvider().MediaObject_GetMediaObjectById(mediaObjectId, includeMetadata);

			if (moDto == null)
			{
				throw new InvalidMediaObjectException(mediaObjectId);
			}

			if (parentAlbum == null)
			{
				parentAlbum = Factory.LoadAlbumInstance(moDto.FKAlbumId, false);
			}

			IGalleryObject mo = new ExternalMediaObject(
				moDto.MediaObjectId,
				parentAlbum,
				moDto.Title.Trim(),
				moDto.HashKey.Trim(),
				moDto.ThumbnailFilename,
				moDto.ThumbnailWidth,
				moDto.ThumbnailHeight,
				moDto.ThumbnailSizeKB,
				moDto.ExternalHtmlSource.Trim(),
				MimeTypeEnumHelper.ParseMimeTypeCategory(moDto.ExternalType.Trim()),
				moDto.Seq,
				moDto.CreatedBy.Trim(),
				moDto.DateAdded,
				moDto.LastModifiedBy.Trim(),
				HelperFunctions.ToDateTime(moDto.DateLastModified),
				moDto.IsPrivate,
				true);

			AddMediaObjectMetadata(mo, moDto);

			return mo;
		}

		private static IGalleryObject RetrieveGenericMediaObjectFromDataStore(int mediaObjectId, IAlbum parentAlbum)
		{
			bool includeMetadata = (parentAlbum == null || parentAlbum.AllowMetadataLoading);

			MediaObjectDto moDto = GetDataProvider().MediaObject_GetMediaObjectById(mediaObjectId, includeMetadata);

			if (moDto == null)
			{
				throw new InvalidMediaObjectException(mediaObjectId);
			}

			if (parentAlbum == null)
			{
				parentAlbum = Factory.LoadAlbumInstance(moDto.FKAlbumId, false);
			}

			IGalleryObject mo = new GenericMediaObject(
				moDto.MediaObjectId,
				parentAlbum,
				moDto.Title.Trim(),
				moDto.HashKey.Trim(),
				moDto.ThumbnailFilename,
				moDto.ThumbnailWidth,
				moDto.ThumbnailHeight,
				moDto.ThumbnailSizeKB,
				moDto.OriginalFilename.Trim(),
				moDto.OriginalWidth,
				moDto.OriginalHeight,
				moDto.OriginalSizeKB,
				moDto.Seq,
				moDto.CreatedBy.Trim(),
				moDto.DateAdded,
				moDto.LastModifiedBy.Trim(),
				HelperFunctions.ToDateTime(moDto.DateLastModified),
				moDto.IsPrivate,
				true,
				null);

			AddMediaObjectMetadata(mo, moDto);

			return mo;
		}

		/// <summary>
		/// Retrieve the specified album from the data store. Child albums and media objects are not added. Guaranteed to not return null.
		/// </summary>
		/// <param name="albumId">The ID that uniquely identifies the album to retrieve.</param>
		/// <returns>Returns the specified album without child albums or media objects.</returns>
		/// <exception cref="InvalidAlbumException">Thrown when an album with the specified album ID is not found in the data store.</exception>
		private static IAlbum RetrieveAlbumFromDataStore(int albumId)
		{
			IAlbum album;

			try
			{
				album = GetAlbumFromDto(GetDataProvider().Album_GetAlbumById(albumId));
			}
			catch (InvalidAlbumException)
			{
				// Throw a new exception instead of the original one, since now we know the album ID and we are able to pass
				// it to the exception constructor.
				throw new InvalidAlbumException(albumId);
			}

			// Since we've just loaded this object from the data store, set the corresponding property.
			album.FullPhysicalPathOnDisk = album.FullPhysicalPath;

			Debug.Assert(album.ThumbnailMediaObjectId > int.MinValue, "The album's ThumbnailMediaObjectId should have been assigned in this method.");

			return album;
		}

		private static void AddChildObjects(IAlbum album)
		{
			if (album == null)
				throw new ArgumentNullException("album");

			#region Add child albums

			foreach (int albumId in GetDataProvider().Album_GetChildAlbumIdsById(album.Id))
			{
				album.Add(CreateAlbumInstance(albumId, album.GalleryId));
			}

			#endregion

			#region Add child media objects

			foreach (MediaObjectDto moDto in GetDataProvider().Album_GetChildMediaObjectsById(album.Id, album.AllowMetadataLoading))
			{
				album.Add(LoadMediaObjectInstance(moDto, album));
			}

			#endregion

			album.AreChildrenInflated = true;
		}

		/// <summary>
		/// Add metadata items to the specified gallery object from the DTO object. No action is taken if 
		/// <paramref name="moDto" /> does not contain any metadata.
		/// </summary>
		/// <param name="go">The gallery object for which metadata items should be added to the MetadataItems collection.</param>
		/// <param name="moDto">A media object DTO containing information about the media object.</param>
		private static void AddMediaObjectMetadata(IGalleryObject go, MediaObjectDto moDto)
		{
			if (moDto.MediaObjectMetadata == null)
				return;

			foreach (MediaObjectMetadataDto mDto in moDto.MediaObjectMetadata)
			{
				FormattedMetadataItemName metaItemName = (FormattedMetadataItemName)mDto.MetadataNameIdentifier;

				go.MetadataItems.Add(new GalleryObjectMetadataItem(
															mDto.MediaObjectMetadataId,
															metaItemName,
															mDto.Description.Trim(),
															mDto.Value.Trim(),
															false));
			}

			go.IsMetadataLoaded = true;
		}

		private static void InflateAlbumFromDto(IAlbum album, AlbumDto albumDto)
		{
			if (album == null)
				throw new ArgumentNullException("album");

			if (albumDto == null)
				throw new ArgumentNullException("albumDto");

			// A parent ID = 0 indicates the root album. Use int.MinValue to send to Album constructor.
			int albumParentId = (albumDto.AlbumParentId == 0 ? int.MinValue : albumDto.AlbumParentId);

			// Assign parent if it hasn't already been assigned.
			if ((album.Parent.Id == int.MinValue) && (albumParentId > int.MinValue))
			{
				album.Parent = CreateAlbumInstance(albumParentId, albumDto.FKGalleryId);
			}

			album.GalleryId = albumDto.FKGalleryId;
			album.Title = albumDto.Title;
			album.DirectoryName = albumDto.DirectoryName;
			album.Summary = albumDto.Summary;
			album.Sequence = albumDto.Seq;
			album.DateStart = HelperFunctions.ToDateTime(albumDto.DateStart);
			album.DateEnd = HelperFunctions.ToDateTime(albumDto.DateEnd);
			album.CreatedByUserName = albumDto.CreatedBy.Trim();
			album.DateAdded = HelperFunctions.ToDateTime(albumDto.DateAdded);
			album.LastModifiedByUserName = albumDto.LastModifiedBy.Trim();
			album.DateLastModified = HelperFunctions.ToDateTime(albumDto.DateLastModified);
			album.OwnerUserName = albumDto.OwnedBy.Trim();
			album.OwnerRoleName = albumDto.OwnerRoleName.Trim();
			album.IsPrivate = albumDto.IsPrivate;

			// Set the album's thumbnail media object ID. Setting this property sets an internal flag that will cause
			// the media object info to be retrieved when the Thumbnail property is accessed. That's why we don't
			// need to set any of the thumbnail properties.
			// WARNING: No matter what, do not call DisplayObject.CreateInstance() because that creates a new object, 
			// and we might be  executing this method from within our Thumbnail display object. Trust me, this 
			// creates hard to find bugs!
			album.ThumbnailMediaObjectId = albumDto.ThumbnailMediaObjectId;
		}

		private static IGalleryServerRoleCollection GetRolesFromRoleDtos(IEnumerable<RoleDto> roleDtos)
		{
			IGalleryServerRoleCollection roles = new GalleryServerRoleCollection();

			foreach (RoleDto roleDto in roleDtos)
			{
				IGalleryServerRole role = new GalleryServerRole(
					roleDto.RoleName,
					roleDto.AllowViewAlbumsAndObjects,
					roleDto.AllowViewOriginalImage,
					roleDto.AllowAddMediaObject,
					roleDto.AllowAddChildAlbum,
					roleDto.AllowEditMediaObject,
					roleDto.AllowEditAlbum,
					roleDto.AllowDeleteMediaObject,
					roleDto.AllowDeleteChildAlbum,
					roleDto.AllowSynchronize,
					roleDto.AllowAdministerSite,
					roleDto.AllowAdministerGallery,
					roleDto.HideWatermark);

				role.RootAlbumIds.AddRange(from r in roleDto.RoleAlbums select r.FKAlbumId);

				roles.Add(role);
			}

			return roles;
		}

		/// <summary>
		/// Get all Gallery Server roles for the current gallery. Guaranteed to not return null.
		/// </summary>
		/// <returns>Returns all Gallery Server roles for the current gallery.</returns>
		private static IGalleryServerRoleCollection GetGalleryServerRolesFromDataStore()
		{
			// Create the roles.
			IGalleryServerRoleCollection roles = GetRolesFromRoleDtos(GetDataProvider().Roles_GetRoles());

			IGalleryCollection galleries = LoadGalleries();
			foreach (IGalleryServerRole role in roles)
			{
				role.Inflate(galleries);
			}

			roles.Sort();

			return roles;
		}

		/// <summary>
		/// Create a new top-level album for the specified <paramref name = "galleryId" /> and persist to the data store. The newly created
		/// album is returned. Guaranteed to not return null.
		/// </summary>
		/// <param name="galleryId">The gallery ID for which the new album is to be the root album.</param>
		/// <returns>Returns an <see cref="Album" /> instance representing the top-level album for the specified <paramref name = "galleryId" />.</returns>
		private static IAlbum CreateRootAlbum(int galleryId)
		{
			IAlbum album = null;
			try
			{
				album = CreateEmptyAlbumInstance(galleryId);

				DateTime currentTimestamp = DateTime.Now;

				album.Parent.Id = 0; // The parent ID of the root album is always zero.
				album.Title = Resources.Root_Album_Default_Title;
				album.DirectoryName = String.Empty; // The root album must have an empty directory name;
				album.Summary = Resources.Root_Album_Default_Summary;
				album.CreatedByUserName = "System";
				album.DateAdded = currentTimestamp;
				album.LastModifiedByUserName = "System";
				album.DateLastModified = currentTimestamp;

				album.Save();
			}
			catch
			{
				if (album != null)
					album.Dispose();

				throw;
			}

			return album;
		}

		private static void AddIntegersToCollectionIfNotPresent(ICollection<int> intCollection, IEnumerable<int> integersToAdd)
		{
			foreach (int intInCollection in integersToAdd)
			{
				if (!intCollection.Contains(intInCollection))
					intCollection.Add(intInCollection);
			}
		}

		/// <summary>
		/// Return only those album ID's in <paramref name = "albumIds" /> that belong to the gallery specified by <paramref name = "galleryId" />.
		/// </summary>
		/// <param name="albumIds">The album ID's.</param>
		/// <param name="galleryId">The gallery ID.</param>
		/// <returns>Returns a collection of integers representing album ID's belonging to albums in the specified gallery.</returns>
		private static ICollection<int> GetAlbumIdsForGallery(ICollection<int> albumIds, int galleryId)
		{
			List<int> galleryAlbumIds = new List<int>(albumIds.Count);

			foreach (int id in albumIds)
			{
				try
				{
					IGalleryObject album = LoadAlbumInstance(id, false);

					if (album.GalleryId == galleryId)
					{
						galleryAlbumIds.Add(id);
					}
				}
				catch (InvalidAlbumException)
				{
				}
			}

			return galleryAlbumIds;
		}

		private static void AddToMediaObjectCache(IGalleryObject go)
		{
			// Add to media object cache, but only if the object's parent is read-only.
			if (go.Parent.IsWritable)
			{
				return;
			}

			Dictionary<int, IGalleryObject> mediaObjectCache = (Dictionary<int, IGalleryObject>)HelperFunctions.GetCache(CacheItem.MediaObjects);

			IGalleryObject mediaObjectInCache;

			if (mediaObjectCache == null)
			{
				mediaObjectCache = new Dictionary<int, IGalleryObject>();
			}

			if (!mediaObjectCache.TryGetValue(go.Id, out mediaObjectInCache))
			{
				lock (mediaObjectCache)
				{
					if (!mediaObjectCache.ContainsKey(go.Id))
					{
						go.IsWritable = false;
						mediaObjectCache.Add(go.Id, go);
						HelperFunctions.SetCache(CacheItem.MediaObjects, mediaObjectCache);
					}
				}
			}
		}

		#endregion

		#region IFactory Implementation

		/// <summary>
		/// Initializes a new instance of the <see cref="ISynchronizationStatus"/> class with the specified properties.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <param name="synchId">The GUID that uniquely identifies the current synchronization.</param>
		/// <param name="synchStatus">The status of the current synchronization.</param>
		/// <param name="totalFileCount">The total number of files in the directory or directories that are being processed in the current
		/// synchronization.</param>
		/// <param name="currentFileName">The name of the current file being processed.</param>
		/// <param name="currentFileIndex">The zero-based index value of the current file being processed. This is a number from 0 to
		/// <see cref="ISynchronizationStatus.TotalFileCount"/> - 1.</param>
		/// <param name="currentFilePath">The path to the current file being processed.</param>
		/// <returns></returns>
		public ISynchronizationStatus CreateSynchronizationStatus(int galleryId, string synchId, SynchronizationState synchStatus, int totalFileCount, string currentFileName, int currentFileIndex, string currentFilePath)
		{
			return new SynchronizationStatus(galleryId, synchId, synchStatus, totalFileCount, currentFileName, currentFileIndex, currentFilePath);
		}

		/// <summary>
		/// Initializes a new, empty instance of <see cref="IGalleryCollection" />.
		/// </summary>
		/// <returns>Returns a new, empty instance of <see cref="IGalleryCollection" />.</returns>
		public IGalleryCollection CreateGalleryCollection()
		{
			return new GalleryCollection();
		}

		/// <summary>
		/// Initializes a new, empty instance of <see cref="IUserProfile" />.
		/// </summary>
		/// <returns>Returns a new, empty instance of <see cref="IUserProfile" />.</returns>
		public IUserProfile CreateUserProfile()
		{
			return new UserProfile();
		}

		#endregion
	}
}