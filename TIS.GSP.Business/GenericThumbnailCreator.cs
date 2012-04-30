using System;
using System.IO;
using System.Globalization;
using System.Drawing;
using System.Drawing.Imaging;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.Business.Properties;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// Provides functionality for creating and saving the thumbnail image files associated with <see cref="GenericMediaObject" /> gallery objects.
	/// </summary>
	public class GenericThumbnailCreator : IDisplayObjectCreator
	{
		private readonly IGalleryObject _galleryObject;

		/// <summary>
		/// Initializes a new instance of the <see cref="GenericThumbnailCreator"/> class.
		/// </summary>
		/// <param name="galleryObject">The gallery object.</param>
		public GenericThumbnailCreator(IGalleryObject galleryObject)
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
			// If necessary, generate and save the thumbnail version of the original image.
			if (!(IsThumbnailImageRequired()))
			{
				return; // No thumbnail image required.
			}

			IGallerySettings gallerySetting = Factory.LoadGallerySetting(_galleryObject.GalleryId);

			// Determine file name and path of the thumbnail image.
			string thumbnailPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(this._galleryObject.Original.FileInfo.DirectoryName, gallerySetting.FullThumbnailPath, gallerySetting.FullMediaObjectPath);
			string newFilename = GenerateNewFilename(thumbnailPath, ImageFormat.Jpeg, gallerySetting.ThumbnailFileNamePrefix);
			string newFilePath = Path.Combine(thumbnailPath, newFilename);

			if (Array.IndexOf<string>(gallerySetting.ImageMagickFileTypes, Path.GetExtension(_galleryObject.Original.FileName).ToLowerInvariant()) >= 0)
			{
				GenerateThumbnailImageUsingImageMagick(newFilePath, gallerySetting);
			}
			else
			{
				GenerateGenericThumbnailImage(newFilePath, gallerySetting);
			}

			this._galleryObject.Thumbnail.FileName = newFilename;
			this._galleryObject.Thumbnail.FileNamePhysicalPath = newFilePath;

			int fileSize = (int)(this._galleryObject.Thumbnail.FileInfo.Length / 1024);

			this._galleryObject.Thumbnail.FileSizeKB = (fileSize < 1 ? 1 : fileSize); // Very small files should be 1, not 0.
		}

		private void GenerateThumbnailImageUsingImageMagick(string newFilePath, IGallerySettings gallerySetting)
		{
			// Generate a temporary filename to store the thumbnail created by ImageMagick.
			string tmpImageThumbnailPath = Path.Combine(AppSetting.Instance.TempUploadDirectory, String.Concat(Guid.NewGuid().ToString(), ".jpg"));

			// Request that ImageMagick create the thumbnail. If successful, the file will be created. If not, it fails silently.
			ImageMagick.GenerateThumbnail(this._galleryObject.Original.FileNamePhysicalPath, tmpImageThumbnailPath, this._galleryObject.GalleryId);

			if (File.Exists(tmpImageThumbnailPath))
			{
				int newWidth, newHeight;
				// ImageMagick successfully created a thumbnail image. Now resize it to the width and height we need.
				using (Bitmap originalBitmap = new Bitmap(tmpImageThumbnailPath))
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
					File.Delete(tmpImageThumbnailPath);
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

				this._galleryObject.Thumbnail.Width = newWidth;
				this._galleryObject.Thumbnail.Height = newHeight;
			}
			else
			{
				// ImageMagick didn't create an image, so default to a generic one.
				GenerateGenericThumbnailImage(newFilePath, gallerySetting);
			}
		}

		private void GenerateGenericThumbnailImage(string newFilePath, IGallerySettings gallerySetting)
		{
			int newWidth, newHeight;

			// Build a generic thumbnail.
			using (Bitmap originalBitmap = GetGenericThumbnailBitmap(this._galleryObject.MimeType))
			{
				ImageHelper.CalculateThumbnailWidthAndHeight(originalBitmap.Width, originalBitmap.Height, out newWidth, out newHeight, true, gallerySetting.MaxThumbnailLength);

				// Get JPEG quality value (0 - 100).
				int jpegQuality = gallerySetting.ThumbnailImageJpegQuality;

				// Generate the new image and save to disk.
				ImageHelper.SaveImageFile(originalBitmap, newFilePath, ImageFormat.Jpeg, newWidth, newHeight, jpegQuality);
			}

			this._galleryObject.Thumbnail.Width = newWidth;
			this._galleryObject.Thumbnail.Height = newHeight;
		}

		private static Bitmap GetGenericThumbnailBitmap(IMimeType mimeType)
		{
			Bitmap thumbnailBitmap = null;

			switch (mimeType.MajorType.ToUpperInvariant())
			{
				case "AUDIO": thumbnailBitmap = Resources.GenericThumbnailImage_Audio; break;
				case "VIDEO": thumbnailBitmap = Resources.GenericThumbnailImage_Video; break;
				case "IMAGE": thumbnailBitmap = Resources.GenericThumbnailImage_Image; break;
				case "APPLICATION": thumbnailBitmap = GetGenericThumbnailBitmapByFileExtension(mimeType.Extension); break;
				default: thumbnailBitmap = Resources.GenericThumbnailImage_Unknown; break;
			}

			return thumbnailBitmap;
		}

		private static Bitmap GetGenericThumbnailBitmapByFileExtension(string fileExtension)
		{
			Bitmap thumbnailBitmap = null;

			switch (fileExtension)
			{
				case ".doc":
				case ".dot":
				case ".docm":
				case ".dotm":
				case ".dotx":
				case ".docx": thumbnailBitmap = Resources.GenericThumbnailImage_Doc; break;
				case ".xls":
				case ".xlam":
				case ".xlsb":
				case ".xlsm":
				case ".xltm":
				case ".xltx":
				case ".xlsx": thumbnailBitmap = Resources.GenericThumbnailImage_Excel; break;
				case ".ppt":
				case ".pps":
				case ".pptx":
				case ".potm":
				case ".ppam":
				case ".ppsm": thumbnailBitmap = Resources.GenericThumbnailImage_PowerPoint; break;
				case ".pdf": thumbnailBitmap = Resources.GenericThumbnailImage_PDF; break;
				default: thumbnailBitmap = Resources.GenericThumbnailImage_Unknown; break;
			}
			return thumbnailBitmap;
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