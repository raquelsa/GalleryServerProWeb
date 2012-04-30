using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using GalleryServerPro.Business.Interfaces;
using System.Globalization;
using GalleryServerPro.ErrorHandler.CustomExceptions;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// Provides functionality for creating and saving the thumbnail image files associated with <see cref="Image" /> gallery objects.
	/// </summary>
	public class ImageOptimizedCreator : IDisplayObjectCreator
	{
		private readonly IGalleryObject _galleryObject;

		/// <summary>
		/// Initializes a new instance of the <see cref="ImageOptimizedCreator"/> class.
		/// </summary>
		/// <param name="galleryObject">The media object.</param>
		public ImageOptimizedCreator(IGalleryObject galleryObject)
		{
			this._galleryObject = galleryObject;
		}

		/// <summary>
		/// Generate the file for this display object and save it to the file system. The routine may decide that
		/// a file does not need to be generated, usually because it already exists. However, it will always be
		/// created if the relevant flag is set on the parent <see cref="IGalleryObject" />. (Example: If
		/// <see cref="IGalleryObject.RegenerateThumbnailOnSave" /> = true, the thumbnail file will always be created.) No data is
		/// persisted to the data store.
		/// </summary>
		/// <exception cref="GalleryServerPro.ErrorHandler.CustomExceptions.UnsupportedImageTypeException">Thrown when the original
		/// image associated with this gallery object cannot be loaded into a <see cref="Bitmap"/> class because it is an 
		/// incompatible or corrupted image type.</exception>
		public void GenerateAndSaveFile()
		{
			// If necessary, generate and save the optimized version of the original image.
			if (!(IsOptimizedImageRequired()))
			{
				bool rotateIsRequested = (this._galleryObject.Rotation != RotateFlipType.RotateNoneFlipNone);

				if (rotateIsRequested || ((this._galleryObject.IsNew) && (String.IsNullOrEmpty(this._galleryObject.Optimized.FileName))))
				{
					// One of the following is true:
					// 1. The original is being rotated and there isn't a separate optimized image.
					// 2. This is a new object that doesn't need a separate optimized image.
					// In either case, set the optimized properties equal to the original properties.
					this._galleryObject.Optimized.FileName = this._galleryObject.Original.FileName;
					this._galleryObject.Optimized.Width = this._galleryObject.Original.Width;
					this._galleryObject.Optimized.Height = this._galleryObject.Original.Height;
					this._galleryObject.Optimized.FileSizeKB = this._galleryObject.Original.FileSizeKB;
				}
				return; // No optimized image required.
			}

			IGallerySettings gallerySetting = Factory.LoadGallerySetting(_galleryObject.GalleryId);

			// Determine file name and path of the optimized image.
			string optimizedPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(this._galleryObject.Original.FileInfo.DirectoryName, gallerySetting.FullOptimizedPath, gallerySetting.FullMediaObjectPath);
			string newFilename = GenerateNewFilename(optimizedPath, ImageFormat.Jpeg, gallerySetting.OptimizedFileNamePrefix);
			string newFilePath = Path.Combine(optimizedPath, newFilename);

			bool thumbnailCreated = false;

			if (Array.IndexOf<string>(gallerySetting.ImageMagickFileTypes, Path.GetExtension(_galleryObject.Original.FileName).ToLowerInvariant()) >= 0)
			{
				thumbnailCreated = GenerateOptimizedImageUsingImageMagick(newFilePath, gallerySetting);
			}

			if (!thumbnailCreated)
			{
				GenerateOptimizedImageUsingDotNet(newFilePath, gallerySetting);
			}

			this._galleryObject.Optimized.FileName = newFilename;
			this._galleryObject.Optimized.FileNamePhysicalPath = newFilePath;

			int fileSize = (int)(this._galleryObject.Optimized.FileInfo.Length / 1024);

			this._galleryObject.Optimized.FileSizeKB = (fileSize < 1 ? 1 : fileSize); // Very small files should be 1, not 0.
		}

		private bool GenerateOptimizedImageUsingImageMagick(string newFilePath, IGallerySettings gallerySetting)
		{
			// Generate a temporary filename to store the thumbnail created by ImageMagick.
			string tmpImageOptimizedPath = Path.Combine(AppSetting.Instance.TempUploadDirectory, String.Concat(Guid.NewGuid().ToString(), ".jpg"));

			if (!String.IsNullOrEmpty(_galleryObject.Original.TempFilePath))
			{
				// Use the image that was created earlier in the thumbnail generator.
				tmpImageOptimizedPath = _galleryObject.Original.TempFilePath;
			}

			// Request that ImageMagick create the thumbnail. If successful, the file will be created. If not, it fails silently.
			if (!File.Exists(tmpImageOptimizedPath))
			{
				ImageMagick.GenerateThumbnail(this._galleryObject.Original.FileNamePhysicalPath, tmpImageOptimizedPath, this._galleryObject.GalleryId);
			}

			if (File.Exists(tmpImageOptimizedPath))
			{
				int newWidth;
				int newHeight;
				// ImageMagick successfully created a thumbnail image. Now resize it to the width and height we need.
				using (Bitmap originalBitmap = new Bitmap(tmpImageOptimizedPath))
				{
					ImageHelper.CalculateOptimizedWidthAndHeight(originalBitmap, out newWidth, out newHeight, _galleryObject.GalleryId);

					// Get JPEG quality value (0 - 100). This is ignored if imgFormat = GIF.
					int jpegQuality = gallerySetting.OptimizedImageJpegQuality;

					// Generate the new image and save to disk.
					ImageHelper.SaveImageFile(originalBitmap, newFilePath, ImageFormat.Jpeg, newWidth, newHeight, jpegQuality);
				}

				this._galleryObject.Optimized.Width = newWidth;
				this._galleryObject.Optimized.Height = newHeight;

				return true;
			}
			else
			{
				return false;
			}
		}

		private void GenerateOptimizedImageUsingDotNet(string newFilePath, IGallerySettings gallerySetting)
		{
			// All optimized images should be JPEG format. (Making GIFs from GIF originals resulted in poor quality images
			// GIFs, so we'll create JPEGs, even those from GIFs.)
			ImageFormat imgFormat = ImageFormat.Jpeg;

			// Don't call Dispose() on originalBitmap unless an exception occurs. That is because it is a reference to a 
			// bitmap of the original image, and there is code in the Image class's Saved event that calls Dispose().
			Bitmap originalBitmap = null;
			int newWidth, newHeight;
			try
			{
				// Get reference to the bitmap from which the optimized image will be generated.
				originalBitmap = this._galleryObject.Original.Bitmap;
				ImageHelper.CalculateOptimizedWidthAndHeight(originalBitmap, out newWidth, out newHeight, _galleryObject.GalleryId);

				// Get JPEG quality value (0 - 100). This is ignored if imgFormat = GIF.
				int jpegQuality = gallerySetting.OptimizedImageJpegQuality;

				// Generate the new image and save to disk.
				ImageHelper.SaveImageFile(originalBitmap, newFilePath, imgFormat, newWidth, newHeight, jpegQuality);
			}
			catch (UnsupportedImageTypeException)
			{
				if (originalBitmap != null)
					originalBitmap.Dispose();

				throw;
			}

			this._galleryObject.Optimized.Width = newWidth;
			this._galleryObject.Optimized.Height = newHeight;
		}

		private bool IsOptimizedImageRequired()
		{
			// We must create an optimized image in the following circumstances:
			// 1. The file corresponding to a previously created optimized image file does not exist.
			//    OR
			// 2. The overwrite flag is true.
			//    OR
			// 3. There is a request to rotate the image.
			//    AND
			// 4. The size of width/height dimensions of the original exceed the optimized triggers.
			//    OR
			// 5. The original image is not a JPEG.
			// In other words: image required = ((1 || 2 || 3) && (4 || 5))

			bool optimizedImageMissing = IsOptimizedImageFileMissing(); // Test 1

			bool overwriteFlag = this._galleryObject.RegenerateOptimizedOnSave; // Test 2

			bool rotateIsRequested = (this._galleryObject.Rotation != RotateFlipType.RotateNoneFlipNone); // Test 3

			bool originalExceedsOptimizedDimensionTriggers = false;
			bool isOriginalNonJpegImage = false;
			if (optimizedImageMissing || overwriteFlag || rotateIsRequested)
			{
				// Only need to run test 3 and 4 if test 1 or test 2 is true.
				originalExceedsOptimizedDimensionTriggers = DoesOriginalExceedOptimizedDimensionTriggers(); // Test 4

				isOriginalNonJpegImage = IsOriginalNonJpegImage(); // Test 5
			}

			return ((optimizedImageMissing || overwriteFlag || rotateIsRequested) && (originalExceedsOptimizedDimensionTriggers || isOriginalNonJpegImage));
		}

		private bool IsOriginalNonJpegImage()
		{
			// Return true if the original image is not a JPEG.
			string[] jpegImageTypes = new string[] { ".jpg", ".jpeg" };
			string originalFileExtension = Path.GetExtension(this._galleryObject.Original.FileName).ToLowerInvariant();

			bool isOriginalNonJpegImage = false;
			if (Array.IndexOf<string>(jpegImageTypes, originalFileExtension) < 0)
			{
				isOriginalNonJpegImage = true;
			}

			return isOriginalNonJpegImage;
		}

		private bool DoesOriginalExceedOptimizedDimensionTriggers()
		{
			IGallerySettings gallerySetting = Factory.LoadGallerySetting(this._galleryObject.GalleryId);

			// Test 1: Is the file size of the original greater than OptimizedImageTriggerSizeKB?
			bool isOriginalFileSizeGreaterThanTriggerSize = false;
			if (this._galleryObject.Original.FileSizeKB > gallerySetting.OptimizedImageTriggerSizeKb)
			{
				isOriginalFileSizeGreaterThanTriggerSize = true;
			}

			// Test 2: Is the width or length of the original greater than the MaxOptimizedLength?
			bool isOriginalLengthGreaterThanMaxAllowedLength = false;
			int optimizedMaxLength = gallerySetting.MaxOptimizedLength;
			int originalWidth = 0;
			int originalHeight = 0;

			try
			{
				originalWidth = this._galleryObject.Original.Bitmap.Width;
				originalHeight = this._galleryObject.Original.Bitmap.Height;
			}
			catch (UnsupportedImageTypeException) { }

			if ((originalWidth > optimizedMaxLength) || (originalHeight > optimizedMaxLength))
			{
				isOriginalLengthGreaterThanMaxAllowedLength = true;
			}

			return (isOriginalFileSizeGreaterThanTriggerSize | isOriginalLengthGreaterThanMaxAllowedLength);
		}

		private bool IsOptimizedImageFileMissing()
		{
			// Does the optimized image file exist? (Maybe it was accidentally deleted or moved by the user,
			// or maybe it's a new object.)
			bool optimizedImageExists = false;
			//bool objectExistsInDataStore = !this._imageObject.IsNew;

			// Does this image object specify that a separate optimized image should exist? When the optimized and original filenames
			// are equal, that means there isn't a separate optimized image.
			bool imageSpecifiesOptimizedVersion = (this._galleryObject.Optimized.FileName != this._galleryObject.Original.FileName);

			// Does a file exist matching the value in the optimized filepath variable?
			optimizedImageExists = File.Exists(this._galleryObject.Optimized.FileNamePhysicalPath);

			// The optimized image is considered to be missing if the image objects says there should be one and we don't find
			// on one the hard drive. (Note that later, when we analyze the dimensions, we may decide
			// we don't need to create an optimized image anyway.)
			bool optimizedImageIsMissing = (!optimizedImageExists && imageSpecifiesOptimizedVersion);

			return optimizedImageIsMissing;
		}

		/// <summary>
		/// Determine name of new file and ensure it is unique in the directory. (Example: If original = puppy.jpg, 
		/// thumbnail = zOpt_puppy.jpg)
		/// </summary>
		/// <param name="optimizedPath">The path to the directory where the optimized file is to be created.</param>
		/// <param name="imgFormat">The image format of the thumbnail.</param>
		/// <param name="filenamePrefix">A string to prepend to the filename. Example: "zThumb_"</param>
		/// <returns>Returns the name of the new thumbnail file name and ensure it is unique in the directory.</returns>
		private string GenerateNewFilename(string optimizedPath, ImageFormat imgFormat, string filenamePrefix)
		{
			string nameWithoutExtension = Path.GetFileNameWithoutExtension(this._galleryObject.Original.FileInfo.Name);
			string optimizedFilename = String.Format(CultureInfo.CurrentCulture, "{0}{1}.{2}", filenamePrefix, nameWithoutExtension, imgFormat.ToString().ToLower(CultureInfo.CurrentCulture));

				optimizedFilename = HelperFunctions.ValidateFileName(optimizedPath, optimizedFilename);

			return optimizedFilename;
		}
	}
}
