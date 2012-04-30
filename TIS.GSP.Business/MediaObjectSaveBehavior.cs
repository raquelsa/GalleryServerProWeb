using System;
using System.IO;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.ErrorHandler.CustomExceptions;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// Provides functionality for persisting a media object to the data store and file system.
	/// </summary>
	public class MediaObjectSaveBehavior : ISaveBehavior
	{
		private readonly IGalleryObject _galleryObject;

		/// <summary>
		/// Initializes a new instance of the <see cref="MediaObjectSaveBehavior"/> class.
		/// </summary>
		/// <param name="galleryObject">The gallery object.</param>
		public MediaObjectSaveBehavior(IGalleryObject galleryObject)
		{
			this._galleryObject = galleryObject;
		}

		/// <summary>
		/// Persist the object to which this behavior belongs to the data store. Also persist to the file system, if
		/// the object has a representation on disk, such as albums (stored as directories) and media objects (stored
		/// as files). New objects with ID = int.MinValue will have a new <see cref="IGalleryObject.Id"/> assigned
		/// and <see cref="IGalleryObject.IsNew"/> set to false.
		/// All validation should have taken place before calling this method.
		/// </summary>
		public void Save()
		{
			// If the user requested a rotation, then rotate and save the original. If no rotation is requested,
			// the following line does nothing.
			this._galleryObject.Original.GenerateAndSaveFile();

			// Generate the thumbnail and optimized versions. These must run after the previous statement because when
			// the image is rotated, these methods assume the original has already been rotated.
			try
			{
				this._galleryObject.Thumbnail.GenerateAndSaveFile();
				this._galleryObject.Optimized.GenerateAndSaveFile();

				try
				{
					// Now delete the temp file, but no worries if an error happens. The file is in the temp directory
					// which is cleaned out each time the app starts anyway.
					if (File.Exists(_galleryObject.Original.TempFilePath))
					{
						File.Delete(_galleryObject.Original.TempFilePath);
					}
				}
				catch (IOException ex)
				{
					ErrorHandler.Error.Record(ex, this._galleryObject.GalleryId, Factory.LoadGallerySettings(), AppSetting.Instance);
				}
				catch (NotSupportedException ex)
				{
					ErrorHandler.Error.Record(ex, this._galleryObject.GalleryId, Factory.LoadGallerySettings(), AppSetting.Instance);
				}
				catch (UnauthorizedAccessException ex)
				{
					ErrorHandler.Error.Record(ex, this._galleryObject.GalleryId, Factory.LoadGallerySettings(), AppSetting.Instance);
				}
			}
			catch (UnsupportedImageTypeException)
			{
				// We'll get here when there is a corrupt image or the server's memory is not sufficient to process the image.
				// When this happens, replace the thumbnail creator object with a GenericThumbnailCreator. That one uses a
				// hard-coded thumbnail image rather than trying to generate a thumbnail from the original image.
				// Also, null out the Optimized object and don't bother to try to create an optimized image.
				this._galleryObject.Thumbnail.DisplayObjectCreator = new GenericThumbnailCreator(this._galleryObject);
				this._galleryObject.Thumbnail.GenerateAndSaveFile();

				this._galleryObject.Optimized = new NullObjects.NullDisplayObject();
			}

			// Update the metadata if required.
			UpdateMetadata();

			// Save the data to the data store
			Factory.GetDataProvider().MediaObject_Save(this._galleryObject);

			foreach (IGalleryObjectMetadataItem item in this._galleryObject.MetadataItems)
			{
				item.HasChanges = false;
			}
		}

		/// <summary>
		/// If any of the metadata items for this media object has its <see cref="IGalleryObject.ExtractMetadataOnSave" /> property 
		/// set to true, then open the original file, extract the items, and update the <see cref="IGalleryObject.MetadataItems" /> 
		/// property on our media object. The <see cref="IGalleryObject.ExtractMetadataOnSave" /> property is not changed to false 
		/// at this time, since the Save method uses it to know which items to persist to the data store.
		/// </summary>
		private void UpdateMetadata()
		{
			if (this._galleryObject.ExtractMetadataOnSave)
			{
				// Replace all metadata with the metadata found in the original file.
				Metadata.MediaObjectMetadataExtractor metadata;
				try
				{
					metadata = new Metadata.MediaObjectMetadataExtractor(this._galleryObject.Original.FileNamePhysicalPath, this._galleryObject.GalleryId);
				}
				catch (OutOfMemoryException)
				{
					// Normally, the Dispose method is called during the Image_Saved event. But when we get this exception, it
					// never executes and therefore doesn't release the file lock. So we explicitly do so here and then 
					// re-throw the exception.
					this._galleryObject.Original.Dispose();
					throw new UnsupportedImageTypeException();
				}

				this._galleryObject.MetadataItems.Clear();
				this._galleryObject.MetadataItems.AddRange(metadata.GetGalleryObjectMetadataItemCollection());
				this._galleryObject.ExtractMetadataOnSave = true;
			}
			else
			{
				// If any individual metadata items have been set to ExtractFromFileOnSave = true, then update those selected ones with
				// the latest metadata from the file. If the metadata item is not found in the file, then set the value to an empty string.
				// The data layer will delete any items with an empty or null string.
				IGalleryObjectMetadataItemCollection metadataItemsToUpdate = this._galleryObject.MetadataItems.GetItemsToUpdate();
				if (metadataItemsToUpdate.Count > 0)
				{
					Metadata.MediaObjectMetadataExtractor metadata;
					try
					{
						metadata = new Metadata.MediaObjectMetadataExtractor(this._galleryObject.Original.FileNamePhysicalPath, this._galleryObject.GalleryId);
					}
					catch (OutOfMemoryException)
					{
						// Normally, the Dispose method is called during the Image_Saved event. But when we get this exception, it
						// never executes and therefore doesn't release the file lock. So we explicitly do so here and then 
						// re-throw the exception.
						this._galleryObject.Original.Dispose();
						throw new UnsupportedImageTypeException();
					}

					foreach (IGalleryObjectMetadataItem metadataItem in metadataItemsToUpdate)
					{
						IGalleryObjectMetadataItem extractedMetadataItem;
						if (metadata.GetGalleryObjectMetadataItemCollection().TryGetMetadataItem(metadataItem.MetadataItemName, out extractedMetadataItem))
						{
							metadataItem.Value = extractedMetadataItem.Value;
						}
						else
						{
							metadataItem.Value = String.Empty;
						}
					}
				}
			}
		}
	}
}
