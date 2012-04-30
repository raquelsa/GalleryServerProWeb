using System;
using System.IO;
using System.Globalization;
using System.Drawing;
using System.Drawing.Imaging;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.ErrorHandler.CustomExceptions;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// Provides functionality for creating and saving the thumbnail image files associated with <see cref="Image" /> gallery objects.
	/// </summary>
	public class ImageThumbnailCreator : IDisplayObjectCreator
	{
		private readonly Image _imageObject;

		/// <summary>
		/// Initializes a new instance of the <see cref="ImageThumbnailCreator"/> class.
		/// </summary>
		/// <param name="imageObject">The image object.</param>
		public ImageThumbnailCreator(Image imageObject)
		{
			this._imageObject = imageObject;
		}

		/// <summary>
		/// Generate the file for this display object and save it to the file system. The routine may decide that
		/// a file does not need to be generated, usually because it already exists. However, it will always be
		/// created if the relevant flag is set on the parent IGalleryObject. (Example: If
		/// <see cref="IGalleryObject.RegenerateThumbnailOnSave" /> = true, the thumbnail file will 
		/// always be created.) No data is persisted to the data store.
		/// </summary>
		/// <exception cref="GalleryServerPro.ErrorHandler.CustomExceptions.UnsupportedImageTypeException">Thrown when the original
		/// image associated with this gallery object cannot be loaded into a <see cref="Bitmap"/> class because it is an 
		/// incompatible or corrupted image type.</exception>
		public void GenerateAndSaveFile()
		{
			// If necessary, generate and save the thumbnail version of the original image.
			if (!(IsThumbnailImageRequired()))
			{
				return; // No thumbnail image required.
			}

			IGallerySettings gallerySetting = Factory.LoadGallerySetting(_imageObject.GalleryId);

			// Determine file name and path of the thumbnail image.
			string thumbnailPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(this._imageObject.Original.FileInfo.DirectoryName, gallerySetting.FullThumbnailPath, gallerySetting.FullMediaObjectPath);
			string newFilename = GenerateNewFilename(thumbnailPath, ImageFormat.Jpeg, gallerySetting.ThumbnailFileNamePrefix);
			string newFilePath = Path.Combine(thumbnailPath, newFilename);

			bool thumbnailCreated = false;

			if (Array.IndexOf<string>(gallerySetting.ImageMagickFileTypes, Path.GetExtension(_imageObject.Original.FileName).ToLowerInvariant()) >= 0)
			{
				thumbnailCreated = GenerateThumbnailImageUsingImageMagick(newFilePath, gallerySetting);
			}

			if (!thumbnailCreated)
			{
				GenerateThumbnailImageUsingDotNet(newFilePath, gallerySetting);
			}

			this._imageObject.Thumbnail.FileName = newFilename;
			this._imageObject.Thumbnail.FileNamePhysicalPath = newFilePath;

			int fileSize = (int)(this._imageObject.Thumbnail.FileInfo.Length / 1024);

			this._imageObject.Thumbnail.FileSizeKB = (fileSize < 1 ? 1 : fileSize); // Very small files should be 1, not 0.
		}

		private bool GenerateThumbnailImageUsingImageMagick(string newFilePath, IGallerySettings gallerySetting)
		{
			// Generate a temporary filename to store the thumbnail created by ImageMagick.
			string tmpImageThumbnailPath = Path.Combine(AppSetting.Instance.TempUploadDirectory, String.Concat(Guid.NewGuid().ToString(), ".jpg"));

			// Request that ImageMagick create the thumbnail. If successful, the file will be created. If not, it fails silently.
			ImageMagick.GenerateThumbnail(this._imageObject.Original.FileNamePhysicalPath, tmpImageThumbnailPath, this._imageObject.GalleryId);

			if (File.Exists(tmpImageThumbnailPath))
			{
				// Save the path so it can be used later by the optimized image creator.
				_imageObject.Original.TempFilePath = tmpImageThumbnailPath;

				int newWidth;
				int newHeight;
				// ImageMagick successfully created a thumbnail image. Now resize it to the width and height we need.
				using (Bitmap originalBitmap = new Bitmap(tmpImageThumbnailPath))
				{
					ImageHelper.CalculateThumbnailWidthAndHeight(originalBitmap.Width, originalBitmap.Height, out newWidth, out newHeight, false, gallerySetting.MaxThumbnailLength);

					// Get JPEG quality value (0 - 100).
					int jpegQuality = gallerySetting.ThumbnailImageJpegQuality;

					// Generate the new image and save to disk.
					ImageHelper.SaveImageFile(originalBitmap, newFilePath, ImageFormat.Jpeg, newWidth, newHeight, jpegQuality);
				}

				try
				{
					// Now delete the thumbnail image created by FFmpeg, but no worries if an error happens. The file is in the temp directory
					// which is cleaned out each time the app starts anyway.
					//File.Delete(tmpImageThumbnailPath);
				}
				catch (Exception ex)
				{
					ErrorHandler.Error.Record(ex, this._imageObject.GalleryId, Factory.LoadGallerySettings(), AppSetting.Instance);
				}

				this._imageObject.Thumbnail.Width = newWidth;
				this._imageObject.Thumbnail.Height = newHeight;

				return true;
			}
			else
			{
				return false;
			}
		}

		private void GenerateThumbnailImageUsingDotNet(string newFilePath, IGallerySettings gallerySetting)
		{
			// All thumbnails should be JPEG format. (My tests show that making GIFs from GIF originals resulted in poor quality thumbnail
			// GIFs, so all thumbnails are JPEG, even those from GIFs.)
			ImageFormat imgFormat = ImageFormat.Jpeg;

			// Don't call Dispose() on originalBitmap unless an exception occurs. That is because it is a reference to a 
			// bitmap of the original image, and there is code in the Image class's Saved event that calls Dispose().
			Bitmap originalBitmap = null;
			int newWidth, newHeight;
			try
			{
				// Get reference to the bitmap from which the optimized image will be generated.
				originalBitmap = this._imageObject.Original.Bitmap;
				ImageHelper.CalculateThumbnailWidthAndHeight(originalBitmap.Width, originalBitmap.Height, out newWidth, out newHeight, false, gallerySetting.MaxThumbnailLength);

				// Get JPEG quality value (0 - 100). This is ignored if imgFormat = GIF.
				int jpegQuality = gallerySetting.ThumbnailImageJpegQuality;

				// Generate the new image and save to disk.
				ImageHelper.SaveImageFile(originalBitmap, newFilePath, imgFormat, newWidth, newHeight, jpegQuality);
			}
			catch (ErrorHandler.CustomExceptions.UnsupportedImageTypeException)
			{
				if (originalBitmap != null)
					originalBitmap.Dispose();

				throw;
			}

			this._imageObject.Thumbnail.Width = newWidth;
			this._imageObject.Thumbnail.Height = newHeight;
		}

		private bool IsThumbnailImageRequired()
		{
			// We must create a thumbnail image in the following circumstances:
			// 1. The file corresponding to a previously created thumbnail image file does not exist.
			//    OR
			// 2. The overwrite flag is true.
			//    OR
			// 3. There is a request to rotate the image.

			bool thumbnailImageMissing = IsThumbnailImageFileMissing(); // Test 1

			bool overwriteFlag = this._imageObject.RegenerateThumbnailOnSave; // Test 2

			bool rotateIsRequested = (this._imageObject.Rotation != RotateFlipType.RotateNoneFlipNone);

			return (thumbnailImageMissing || overwriteFlag || rotateIsRequested);
		}

		private bool IsThumbnailImageFileMissing()
		{
			// Does the thumbnail image file exist? (Maybe it was accidentally deleted or moved by the user,
			// or maybe it's a new object.)
			bool thumbnailImageExists = false;
			if (File.Exists(this._imageObject.Thumbnail.FileNamePhysicalPath))
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
			string nameWithoutExtension = Path.GetFileNameWithoutExtension(this._imageObject.Original.FileInfo.Name);
			string thumbnailFilename = String.Format(CultureInfo.CurrentCulture, "{0}{1}.{2}", filenamePrefix, nameWithoutExtension, imgFormat.ToString().ToLower(CultureInfo.CurrentCulture));

			thumbnailFilename = HelperFunctions.ValidateFileName(thumbnailPath, thumbnailFilename);

			return thumbnailFilename;
		}
	}
}
