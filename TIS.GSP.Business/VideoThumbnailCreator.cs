using System;
using System.IO;
using System.Globalization;
using System.Drawing;
using System.Drawing.Imaging;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.Business.Metadata;
using GalleryServerPro.Business.Properties;

namespace GalleryServerPro.Business
{
	//TODO: Refactor metadata extraction to metadata class so that it can be reloaded during synchs if necessary
	/// <summary>
	/// Provides functionality for creating and saving the thumbnail image files associated with <see cref="Video" /> gallery objects.
	/// </summary>
	public class VideoThumbnailCreator : IDisplayObjectCreator
	{
		private readonly IGalleryObject _galleryObject;

		/// <summary>
		/// Initializes a new instance of the <see cref="VideoThumbnailCreator"/> class.
		/// </summary>
		/// <param name="galleryObject">The gallery object.</param>
		public VideoThumbnailCreator(IGalleryObject galleryObject)
		{
			this._galleryObject = galleryObject;
		}

		/// <summary>
		/// Generate the thumbnail image for this display object and save it to the file system. The routine may decide that
		/// a file does not need to be generated, usually because it already exists. However, it will always be
		/// created if the relevant flag is set on the parent <see cref="IGalleryObject" />. (Example: If
		/// <see cref="IGalleryObject.RegenerateThumbnailOnSave" /> = true, the thumbnail file will always be created.) No data is
		/// persisted to the data store.
		/// </summary>
		public void GenerateAndSaveFile()
		{
			// If necessary, generate and save the thumbnail version of the video.
			if (!(IsThumbnailImageRequired()))
			{
				return; // No thumbnail image required.
			}

			IGallerySettings gallerySetting = Factory.LoadGallerySetting(_galleryObject.GalleryId);

			// Generate a temporary filename to store the thumbnail created by FFmpeg.
			string tmpVideoThumbnailPath = Path.Combine(AppSetting.Instance.TempUploadDirectory, String.Concat(Guid.NewGuid().ToString(), ".jpg"));

			// Request that FFmpeg create the thumbnail. If successful, the file will be created.
			string ffmpegOutput = FFmpeg.GenerateThumbnail(this._galleryObject.Original.FileNamePhysicalPath, tmpVideoThumbnailPath, gallerySetting.VideoThumbnailPosition, this._galleryObject.GalleryId);

			if (!String.IsNullOrEmpty(ffmpegOutput) && this._galleryObject.IsNew && gallerySetting.ExtractMetadata && this._galleryObject.ExtractMetadataOnSave)
			{
				// When metadata extraction is enabled and we have a new video where we have some output from FFmpeg, parse the data.
				MediaObjectMetadataExtractor metadata = new MediaObjectMetadataExtractor(this._galleryObject.Original.FileNamePhysicalPath, this._galleryObject.GalleryId, ffmpegOutput);
				this._galleryObject.MetadataItems.AddRange(metadata.GetGalleryObjectMetadataItemCollection());
				this._galleryObject.ExtractMetadataOnSave = false; // Sends signal to save routine to not re-extract metadata
			}

			// Verify image was created from video, trying again using a different video position setting if necessary.
			ValidateVideoThumbnail(tmpVideoThumbnailPath, gallerySetting.VideoThumbnailPosition);

			// Determine file name and path of the thumbnail image.
			string thumbnailPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(this._galleryObject.Original.FileInfo.DirectoryName, gallerySetting.FullThumbnailPath, gallerySetting.FullMediaObjectPath);
			string newFilename = GenerateNewFilename(thumbnailPath, ImageFormat.Jpeg, gallerySetting.ThumbnailFileNamePrefix);
			string newFilePath = Path.Combine(thumbnailPath, newFilename);

			int newWidth, newHeight;
			if (File.Exists(tmpVideoThumbnailPath))
			{
				// FFmpeg successfully created a thumbnail image the same size as the video. Now resize it to the width and height we need.
				using (Bitmap originalBitmap = new Bitmap(tmpVideoThumbnailPath))
				{
					ImageHelper.CalculateThumbnailWidthAndHeight(originalBitmap.Width, originalBitmap.Height, out newWidth, out newHeight, false, gallerySetting.MaxThumbnailLength);

					// Get JPEG quality value (0 - 100). This is ignored if imgFormat = GIF.
					int jpegQuality = gallerySetting.ThumbnailImageJpegQuality;

					// Generate the new image and save to disk.
					ImageHelper.SaveImageFile(originalBitmap, newFilePath, ImageFormat.Jpeg, newWidth, newHeight, jpegQuality);
				}

				try
				{
					// Now delete the thumbnail image created by FFmpeg, but no worries if an error happens. The file is in the temp directory
					// which is cleaned out each time the app starts anyway.
					File.Delete(tmpVideoThumbnailPath);
				}
				catch (IOException ex)
				{
					ErrorHandler.Error.Record(ex, this._galleryObject.GalleryId, Factory.LoadGallerySettings(), AppSetting.Instance);
				}
				catch (UnauthorizedAccessException ex)
				{
					ErrorHandler.Error.Record(ex, this._galleryObject.GalleryId, Factory.LoadGallerySettings(), AppSetting.Instance);
				}
				catch (NotSupportedException ex)
				{
					ErrorHandler.Error.Record(ex, this._galleryObject.GalleryId, Factory.LoadGallerySettings(), AppSetting.Instance);
				}
			}
			else
			{
				// FFmpeg didn't run or no thumbnail image was created by FFmpeg. Build a generic video thumbnail.
				using (Bitmap originalBitmap = Resources.GenericThumbnailImage_Video)
				{
					ImageHelper.CalculateThumbnailWidthAndHeight(originalBitmap.Width, originalBitmap.Height, out newWidth, out newHeight, true, gallerySetting.MaxThumbnailLength);

					// Get JPEG quality value (0 - 100).
					int jpegQuality = gallerySetting.ThumbnailImageJpegQuality;

					// Generate the new image and save to disk.
					ImageHelper.SaveImageFile(originalBitmap, newFilePath, ImageFormat.Jpeg, newWidth, newHeight, jpegQuality);
				}
			}

			this._galleryObject.Thumbnail.Width = newWidth;
			this._galleryObject.Thumbnail.Height = newHeight;
			this._galleryObject.Thumbnail.FileName = newFilename;
			this._galleryObject.Thumbnail.FileNamePhysicalPath = newFilePath;

			int fileSize = (int)(this._galleryObject.Thumbnail.FileInfo.Length / 1024);

			this._galleryObject.Thumbnail.FileSizeKB = (fileSize < 1 ? 1 : fileSize); // Very small files should be 1, not 0.
		}

		/// <summary>
		/// Verify the image was created from the video. If not, it might be because the video is shorter than the position
		/// where we tried to grab the image. If this is the case, try again, except grab an image from the beginning of the video.
		/// </summary>
		/// <param name="tmpVideoThumbnailPath">The video thumbnail path.</param>
		/// <param name="videoThumbnailPosition">The position, in seconds, in the video where the thumbnail is generated from a frame.</param>
		private void ValidateVideoThumbnail(string tmpVideoThumbnailPath, int videoThumbnailPosition)
		{
			if (!File.Exists(tmpVideoThumbnailPath))
			{
				IGalleryObjectMetadataItem metadataItem;
				if (this._galleryObject.MetadataItems.TryGetMetadataItem(FormattedMetadataItemName.Duration, out metadataItem))
				{
					TimeSpan duration;
					if (TimeSpan.TryParse(metadataItem.Value, out duration))
					{
						if (duration < new TimeSpan(0, 0, videoThumbnailPosition))
						{
							// Video is shorter than the number of seconds where we are suppossed to grab the thumbnail.
							// Try again, except use 1 second instead of the gallery setting.
							const int videoThumbnailPositionFallback = 1;
							FFmpeg.GenerateThumbnail(this._galleryObject.Original.FileNamePhysicalPath, tmpVideoThumbnailPath, videoThumbnailPositionFallback, this._galleryObject.GalleryId);
						}
					}
				}
			}
		}

		private bool IsThumbnailImageRequired()
		{
			// We must create a thumbnail image in the following circumstances:
			// 1. The file corresponding to a previously created thumbnail image file does not exist.
			//    OR
			// 2. The overwrite flag is true.

			bool thumbnailImageMissing = IsThumbnailImageFileMissing(); // Test 1

			bool overwriteFlag = this._galleryObject.RegenerateThumbnailOnSave; // Test 2

			return (thumbnailImageMissing || overwriteFlag);
		}

		private bool IsThumbnailImageFileMissing()
		{
			// Does the thumbnail image file exist? (Maybe it was accidentally deleted or moved by the user,
			// or maybe it's a new object.)
			bool thumbnailImageExists = false;

			if (File.Exists(this._galleryObject.Thumbnail.FileNamePhysicalPath))
			{
				// Thumbnail image file exists.
				thumbnailImageExists = true;
			}

			bool thumbnailImageIsMissing = !thumbnailImageExists;

			return thumbnailImageIsMissing;
		}

		/// <summary>
		/// Determine name of new file and ensure it is unique in the directory. (Example: If original = puppy.jpg,
		/// thumbnail = zThumb_puppy.jpg)
		/// </summary>
		/// <param name="thumbnailPath">The path to the directory where the thumbnail file is to be created.</param>
		/// <param name="imgFormat">The image format of the thumbnail.</param>
		/// <param name="filenamePrefix">A string to prepend to the filename. Example: "zThumb_"</param>
		/// <returns>
		/// Returns the name of the new thumbnail file name and ensure it is unique in the directory.
		/// </returns>
		private string GenerateNewFilename(string thumbnailPath, ImageFormat imgFormat, string filenamePrefix)
		{
			string nameWithoutExtension = Path.GetFileNameWithoutExtension(this._galleryObject.Original.FileInfo.Name);
			string thumbnailFilename = String.Format(CultureInfo.CurrentCulture, "{0}{1}.{2}", filenamePrefix, nameWithoutExtension, imgFormat.ToString().ToLower(CultureInfo.CurrentCulture));

			thumbnailFilename = HelperFunctions.ValidateFileName(thumbnailPath, thumbnailFilename);

			return thumbnailFilename;
		}
	}
}