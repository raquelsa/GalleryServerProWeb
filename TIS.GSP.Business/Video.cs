using System;
using System.Data;
using System.Globalization;
using System.IO;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.Business.Metadata;
using GalleryServerPro.Business.Properties;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// The Video class represents a media object within Gallery Server Pro that is a video.
	/// </summary>
	public class Video : GalleryObject
	{
		#region Private Fields


		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of a <see cref="Video" /> object.
		/// </summary>
		/// <param name="videoFile">A <see cref="FileInfo"/> object containing the original video file for this object. This is intended to be 
		/// specified when creating a new media object from a file. Specify null when instantiating an object for an existing database
		/// record.</param>
		/// <param name="parentAlbum">The album that contains this object. This is a required parameter.</param>
		/// <exception cref="GalleryServerPro.ErrorHandler.CustomExceptions.InvalidMediaObjectException">Thrown when 
		/// <paramref name="videoFile"/> refers to a file that is not in the same directory as the parent album's directory.</exception>
		/// <exception cref="GalleryServerPro.ErrorHandler.CustomExceptions.UnsupportedMediaObjectTypeException">Thrown when
		/// <paramref name="videoFile"/> is specified (not null) and its file extension does not correspond to an video MIME
		/// type, as determined by the MIME type definition in the configuration file.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="parentAlbum" /> is null.</exception>
		/// <remarks>This constructor does not verify that <paramref name="videoFile"/> refers to a file type that is enabled in the 
		/// configuration file.</remarks>
		internal Video(System.IO.FileInfo videoFile, IAlbum parentAlbum)
			: this(int.MinValue, parentAlbum, string.Empty, string.Empty, string.Empty,
						int.MinValue, int.MinValue, int.MinValue, string.Empty, int.MinValue, int.MinValue,
						int.MinValue, string.Empty, int.MinValue, int.MinValue, int.MinValue, int.MinValue,
						String.Empty, DateTime.MinValue, String.Empty, DateTime.MinValue, parentAlbum != null ? parentAlbum.IsPrivate : false, false, videoFile)
		{
		}

		/// <summary>
		/// Initializes a new instance of an <see cref="Video" /> object.
		/// </summary>
		/// <param name="id">The ID that uniquely identifies this object. Specify int.MinValue for a new object.</param>
		/// <param name="parentAlbum">The album that contains this object. This is a required parameter.</param>
		/// <param name="title">The title of this image.</param>
		/// <param name="hashKey">The hash key that uniquely identifies the original image file.</param>
		/// <param name="thumbnailFilename">The filename of the thumbnail image.</param>
		/// <param name="thumbnailWidth">The width (px) of the thumbnail image.</param>
		/// <param name="thumbnailHeight">The height (px) of the thumbnail image.</param>
		/// <param name="thumbnailSizeKb">The size (KB) of the thumbnail image.</param>
		/// <param name="optimizedFilename">The filename of the optimized image.</param>
		/// <param name="optimizedWidth">The width (px) of the optimized image.</param>
		/// <param name="optimizedHeight">The height (px) of the optimized image.</param>
		/// <param name="optimizedSizeKb">The size (KB) of the optimized image.</param>
		/// <param name="originalFilename">The filename of the original image.</param>
		/// <param name="originalWidth">The width (px) of the original image.</param>
		/// <param name="originalHeight">The height (px) of the original image.</param>
		/// <param name="originalSizeKb">The size (KB) of the original image.</param>
		/// <param name="sequence">An integer that represents the order in which this image should appear when displayed.</param>
		/// <param name="createdByUsername">The user name of the account that originally added this object to the data store.</param>
		/// <param name="dateAdded">The date this image was added to the data store.</param>
		/// <param name="lastModifiedByUsername">The user name of the account that last modified this object.</param>
		/// <param name="dateLastModified">The date this object was last modified.</param>
		/// <param name="isPrivate">Indicates whether this object should be hidden from un-authenticated (anonymous) users.</param>
		/// <param name="isInflated">A bool indicating whether this object is fully inflated.</param>
		/// <param name="videoFile">A <see cref="FileInfo"/> object containing the original video file for this object. This is intended to be 
		/// specified when creating a new media object from a file. Specify null when instantiating an object for an existing database
		/// record.</param>
		/// <exception cref="GalleryServerPro.ErrorHandler.CustomExceptions.InvalidMediaObjectException">Thrown when
		/// <paramref name="videoFile"/> is specified (not null) and the file it refers to is not in the same directory
		/// as the parent album's directory.</exception>
		/// <exception cref="GalleryServerPro.ErrorHandler.CustomExceptions.UnsupportedMediaObjectTypeException">Thrown when
		/// <paramref name="videoFile"/> is specified (not null) and its file extension does not correspond to an video MIME
		/// type, as determined by the MIME type definition in the configuration file.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="parentAlbum" /> is null.</exception>
		/// <remarks>This constructor does not verify that <paramref name="videoFile"/> refers to a file type that is enabled in the 
		/// configuration file.</remarks>
		internal Video(int id, IAlbum parentAlbum, string title, string hashKey, string thumbnailFilename, int thumbnailWidth, int thumbnailHeight, int thumbnailSizeKb, string optimizedFilename, int optimizedWidth, int optimizedHeight, int optimizedSizeKb, string originalFilename, int originalWidth, int originalHeight, int originalSizeKb, int sequence, string createdByUsername, DateTime dateAdded, string lastModifiedByUsername, DateTime dateLastModified, bool isPrivate, bool isInflated, FileInfo videoFile)
		{
			if (parentAlbum == null)
				throw new ArgumentNullException("parentAlbum");

			this.Id = id;
			this.Parent = parentAlbum;
			this.GalleryId = this.Parent.GalleryId;
			this.Title = title;
			this.Sequence = sequence;
			this.Hashkey = hashKey;
			this.CreatedByUserName = createdByUsername;
			this.DateAdded = dateAdded;
			this.LastModifiedByUserName = lastModifiedByUsername;
			this.DateLastModified = dateLastModified;
			this.IsPrivate = isPrivate;

			string parentPhysicalPath = this.Parent.FullPhysicalPathOnDisk;

			IGallerySettings gallerySetting = Factory.LoadGallerySetting(GalleryId);

			// Thumbnail image
			this.Thumbnail = DisplayObject.CreateInstance(this, thumbnailFilename, thumbnailWidth, thumbnailHeight, DisplayObjectType.Thumbnail, new VideoThumbnailCreator(this));
			this.Thumbnail.FileSizeKB = thumbnailSizeKb;
			if (thumbnailFilename.Length > 0)
			{
				// The thumbnail is stored in either the album's physical path or an alternate location (if thumbnailPath config setting is specified) .
				string thumbnailPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(parentPhysicalPath, gallerySetting.FullThumbnailPath, gallerySetting.FullMediaObjectPath);
				this.Thumbnail.FileNamePhysicalPath = System.IO.Path.Combine(thumbnailPath, thumbnailFilename);
			}

			// Optimized video
			this.Optimized = DisplayObject.CreateInstance(this, optimizedFilename, optimizedWidth, optimizedHeight, DisplayObjectType.Optimized, new VideoOptimizedCreator(this));
			this.Optimized.FileSizeKB = optimizedSizeKb;
			if (optimizedFilename.Length > 0)
			{
				// Calcululate the full file path to the optimized video. If the optimized filename is equal to the original filename, then no
				// optimized version exists, and we'll just point to the original. If the names are different, then there is a separate optimized
				// image file, and it is stored in either the album's physical path or an alternate location (if optimizedPath config setting is specified).
				string optimizedPath = parentPhysicalPath;

				if (optimizedFilename != originalFilename)
					optimizedPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(parentPhysicalPath, gallerySetting.FullOptimizedPath, gallerySetting.FullMediaObjectPath);

				this.Optimized.FileNamePhysicalPath = System.IO.Path.Combine(optimizedPath, optimizedFilename);
			}

			// Original video file
			this.Original = DisplayObject.CreateInstance(this, originalFilename, originalWidth, originalHeight, DisplayObjectType.Original, new NullObjects.NullDisplayObjectCreator());
			this.Original.ExternalHtmlSource = String.Empty;
			this.Original.ExternalType = MimeTypeCategory.NotSet;

			if (videoFile != null)
			{
				this.Hashkey = HelperFunctions.GetHashKeyUnique(videoFile);
				this.Original.FileInfo = videoFile; // Will throw InvalidMediaObjectException if the file's directory is not the same as the album's directory.

				if (this.MimeType.TypeCategory != MimeTypeCategory.Video)
				{
					throw new GalleryServerPro.ErrorHandler.CustomExceptions.UnsupportedMediaObjectTypeException(this.Original.FileInfo);
				}

				this.Original.Width = gallerySetting.DefaultVideoPlayerWidth;
				this.Original.Height = gallerySetting.DefaultVideoPlayerHeight;

				int fileSize = (int)(videoFile.Length / 1024);
				this.Original.FileSizeKB = (fileSize < 1 ? 1 : fileSize); // Very small files should be 1, not 0.

				if (gallerySetting.ExtractMetadata)
				{
					// Get the metadata found in the original file.
					MediaObjectMetadataExtractor metadata = new MediaObjectMetadataExtractor(videoFile.FullName, this.GalleryId);
					MetadataItems.AddRange(metadata.GetGalleryObjectMetadataItemCollection());
				}

				// Assign the title based on the template, resorting to the filename if necessary.
				if (String.IsNullOrEmpty(title))
				{
					SetTitle(gallerySetting.MediaObjectCaptionTemplate);

					if (String.IsNullOrEmpty(this.Title))
					{
						this.Title = videoFile.Name;
					}
				}
			}
			else
			{
				this.Original.FileNamePhysicalPath = System.IO.Path.Combine(parentPhysicalPath, originalFilename);
				this.Original.FileSizeKB = originalSizeKb;
			}

			this.SaveBehavior = Factory.GetMediaObjectSaveBehavior(this);
			this.DeleteBehavior = Factory.GetMediaObjectDeleteBehavior(this);

			this.IsInflated = isInflated;

			// Setting the previous properties has caused HasChanges = true, but we don't want this while
			// we're instantiating a new object. Reset to false.
			this.HasChanges = false;

			// Set up our event handlers.
			//this.Saving += new EventHandler(Image_Saving); // Don't need
			this.Saved += MediaObject_Saved;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Inflate the current object by loading all properties from the data store. If the object is already inflated 
		/// (<see cref="IGalleryObject.IsInflated"/>=true), no action is taken.
		/// </summary>
		public override void Inflate()
		{
			// If this is not a new object, and it has not been inflated
			// from the database, go to the database and retrieve the info for this object.
			if ((!this.IsNew) && (!this.IsInflated))
			{
				Factory.LoadVideoInstance(this);

				if ((!this.IsInflated) || (this.HasChanges))
					throw new System.InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.Video_Inflate_Ex_Msg, this.IsInflated, this.HasChanges));
			}
		}

		#endregion

		#region Event Handlers

		void MediaObject_Saved(object sender, EventArgs e)
		{
			// This event is fired when the Save() method is called, after all data is saved.

			#region Assign DisplayObject.MediaObjectId

			// If the MediaObjectId has not yet been assigned, do so now. This will occur after a media object is first
			// saved, since that is when the ID is generated.
			if (this.Thumbnail.MediaObjectId == int.MinValue)
			{
				this.Thumbnail.MediaObjectId = this.Id;
			}

			if (this.Optimized.MediaObjectId == int.MinValue)
			{
				this.Optimized.MediaObjectId = this.Id;
			}

			if (this.Original.MediaObjectId == int.MinValue)
			{
				this.Original.MediaObjectId = this.Id;
			}

			#endregion

			// Create web-friendly version *after* item has been persisted to data store.
			// We need to do it this way because we need the media object ID for the media queue table.
			this.Optimized.GenerateAndSaveFile();
		}

		#endregion
	}
}
