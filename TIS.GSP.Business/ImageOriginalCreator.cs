using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.ErrorHandler.CustomExceptions;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// Contains functionality for manipulating the original image files associated with <see cref="Image" /> gallery objects.
	/// The only time a new original image must be generated is when the user rotates it. This will only
	/// occur for existing objects.
	/// </summary>
	public class ImageOriginalCreator : IDisplayObjectCreator
	{
		private readonly Image _imageObject;

		/// <summary>
		/// Initializes a new instance of the <see cref="ImageOriginalCreator"/> class.
		/// </summary>
		/// <param name="imageObject">The image object.</param>
		public ImageOriginalCreator(Image imageObject)
		{
			this._imageObject = imageObject;
		}
		/// <summary>
		/// Generate the file for this display object and save it to the file system. The routine may decide that
		/// a file does not need to be generated, usually because it already exists. However, it will always be
		/// created if the relevant flag is set on the parent <see cref="IGalleryObject" />. (Example: If
		/// <see cref="IGalleryObject.RegenerateThumbnailOnSave" /> = true, the thumbnail file will always be created.) No data is
		/// persisted to the data store.
		/// </summary>
		/// <exception cref="GalleryServerPro.ErrorHandler.CustomExceptions.UnsupportedImageTypeException">Thrown when the original image cannot 
		/// be loaded into a <see cref="Bitmap"/> class because it is an incompatible or corrupted image type.</exception>
		public void GenerateAndSaveFile()
		{
			// The only time we need to generate a new original image is when the user rotates it. This will only
			// occur for existing objects.
			if ((this._imageObject.IsNew) || (this._imageObject.Rotation == RotateFlipType.RotateNoneFlipNone))
				return;

			string filepath = this._imageObject.Original.FileNamePhysicalPath;

			if (!File.Exists(filepath))
				throw new GalleryServerPro.ErrorHandler.CustomExceptions.BusinessException(String.Format(CultureInfo.CurrentCulture, "Cannot rotate image because no file exists at {0}.", filepath));

			// Don't call Dispose() on originalBitmap unless an exception occurs. That is because it is a reference to a 
			// bitmap of the original image, and there is code in the Image class's Saved event that calls Dispose().
			Bitmap originalBitmap = null;
			try
			{
				// Get reference to the bitmap from which the optimized image will be generated.
				originalBitmap = this._imageObject.Original.Bitmap;
				ImageFormat imgFormat = originalBitmap.RawFormat; // Need to grab the format before we rotate or else we lose it (it changes to MemoryBmp)
				
				try
				{
					originalBitmap.RotateFlip(this._imageObject.Rotation);
				}
				catch (System.Runtime.InteropServices.ExternalException)
				{
					throw new UnsupportedImageTypeException();
				}

				// Get JPEG quality value (0 - 100). This is ignored if imgFormat is not JPEG.
				int jpegQuality = Factory.LoadGallerySetting(this._imageObject.GalleryId).OriginalImageJpegQuality;
				ImageHelper.SaveImageToDisk(originalBitmap, filepath, imgFormat, jpegQuality);

				this._imageObject.Original.Width = this._imageObject.Original.Bitmap.Width;
				this._imageObject.Original.Height = this._imageObject.Original.Bitmap.Height;
			}
			catch (GalleryServerPro.ErrorHandler.CustomExceptions.UnsupportedImageTypeException)
			{
				if (originalBitmap != null)
					originalBitmap.Dispose();

				throw;
			}

			int fileSize = (int)(this._imageObject.Original.FileInfo.Length / 1024);
			this._imageObject.Original.FileSizeKB = (fileSize < 1 ? 1 : fileSize); // Very small files should be 1, not 0.
		}
	}
}

