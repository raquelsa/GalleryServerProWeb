using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using GalleryServerPro.Business.Interfaces;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// Contains image manipulation functions useful for Gallery Server Pro.
	/// </summary>
	public static class ImageHelper
	{
		#region Private Fields

		private static readonly object _sharedLock = new object();

		#endregion

		#region Public Static Methods

		/// <summary>
		/// Generate a new image from the bitmap with the specified format, width, and height, and at the specified location.
		/// </summary>
		/// <param name="sourceBmp">The bitmap containing an image from which to generate a new image with the
		/// specified settings. This bitmap is not modified.</param>
		/// <param name="newFilePath">The location on disk to store the image that is generated.</param>
		/// <param name="newImageFormat">The new image format.</param>
		/// <param name="newWidth">The width to make the new image.</param>
		/// <param name="newHeight">The height to make the new image.</param>
		/// <param name="newJpegQuality">The JPEG quality setting (0 - 100) for the new image. Only used if the
		/// image format paramater is JPEG; ignored for all other formats.</param>
		/// <exception cref="GalleryServerPro.ErrorHandler.CustomExceptions.UnsupportedImageTypeException">Thrown when <paramref name="sourceBmp"/> 
		/// cannot be resized to the requested dimensions.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="sourceBmp" /> is null.</exception>
		public static void SaveImageFile(System.Drawing.Image sourceBmp, string newFilePath, ImageFormat newImageFormat, int newWidth, int newHeight, int newJpegQuality)
		{
			if (sourceBmp == null)
				throw new ArgumentNullException("sourceBmp");

			//Create new bitmap with the new dimensions and in the specified format.
			Bitmap destinationBmp = CreateResizedBitmap(sourceBmp, sourceBmp.Size.Width, sourceBmp.Size.Height, newWidth, newHeight);

			try
			{
				SaveImageToDisk(destinationBmp, newFilePath, newImageFormat, newJpegQuality);
			}
			finally
			{
				destinationBmp.Dispose();
			}
		}

		/// <summary>
		/// Calculate the required width and height of a thumbnail image based on the user settings and while preserving the
		/// aspect ratio of the original dimensions. If the autoEnlarge parameter is false, then preserve the original
		/// dimensions if they are smaller than the user-specified thumbnail dimensions. If true, then ensure that at
		/// least one of the out parameters (newWidth or newHeight) meets the user-specified thumbnail dimensions
		/// (i.e. enlarge a small image if necessary). This method does not create a thumbnail image.
		/// </summary>
		/// <param name="originalWidth">An integer specifying the width, in pixels, of the source image.</param>
		/// <param name="originalHeight">An integer specifying the height, in pixels, of the source image.</param>
		/// <param name="newWidth">An integer specifying the width, in pixels, of a thumbnail image that preserves
		/// the aspect ratio of the original's width and height.</param>
		/// <param name="newHeight">An integer specifying the height, in pixels, of a thumbnail image that preserves
		/// the aspect ratio of the original's width and height.</param>
		/// <param name="autoEnlarge">A value indicating whether to enlarge images that are smaller than the
		/// MaxThumbnailLength configuration setting. If true, the new width and height will be increased if necessary
		/// so that at least one of the values is larger than the MaxThumbnailLength value. If false, the original
		/// width and height are returned when their dimensions are smaller than the MaxThumbnailLength value. This
		/// parameter has no effect when MaxThumbnailLength is greater than both originalWidth and originalHeight.</param>
		/// <param name="maxThumbnailLength">The length (in pixels) of the longest edge of a thumbnail image.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
		public static void CalculateThumbnailWidthAndHeight(int originalWidth, int originalHeight, out int newWidth, out int newHeight, bool autoEnlarge, int maxThumbnailLength)
		{
			if (!autoEnlarge && (maxThumbnailLength > originalWidth) && (maxThumbnailLength > originalHeight))
			{
				// Bitmap is smaller than desired thumbnail dimensions but autoEnlarge = false. Don't enlarge thumbnail; 
				// just use original size.
				newWidth = originalWidth;
				newHeight = originalHeight;
			}
			else if (originalWidth > originalHeight)
			{
				// Bitmap is in landscape format (width > height). The width will be the longest dimension.
				newWidth = maxThumbnailLength;
				newHeight = originalHeight*newWidth/originalWidth;
			}
			else
			{
				// Bitmap is in portrait format (height > width). The height will be the longest dimension.
				newHeight = maxThumbnailLength;
				newWidth = originalWidth*newHeight/originalHeight;
			}
		}

		/// <summary>
		/// Calculate the required width and height of the optimized image based on the specified Bitmap. The aspect ratio
		/// of the bitmap image is preserved in the calculated values. This method does not create the optimized image -
		/// it only calculates the dimensions it should be created with.
		/// </summary>
		/// <param name="bmp">The Bitmap containing an image from which the optimized width and height values should be calculated.</param>
		/// <param name="width">The calculated width of the optimized image.</param>
		/// <param name="height">The calculated height of the optimized image.</param>
		/// <param name="galleryId">The gallery ID.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="bmp" /> is null.</exception>
		public static void CalculateOptimizedWidthAndHeight(System.Drawing.Image bmp, out int width, out int height, int galleryId)
		{
			if (bmp == null)
				throw new ArgumentNullException("bmp");

			// Calculate the width and height based on the user settings and aspect ratio of the specified bitmap.
			int maxLength = Factory.LoadGallerySetting(galleryId).MaxOptimizedLength;
			int newWidth, newHeight;

			int originalWidth = bmp.Width;
			int originalHeight = bmp.Height;

			if ((maxLength > originalWidth) && (maxLength > originalHeight))
			{
				// Bitmap is smaller than desired optimized dimensions. Don't enlarge optimized; just use bitmap size.
				newWidth = originalWidth;
				newHeight = originalHeight;
			}
			else if (originalWidth > originalHeight)
			{
				// Bitmap is in landscape format (width > height). The width will be the longest dimension.
				newWidth = maxLength;
				newHeight = originalHeight*newWidth/originalWidth;
			}
			else
			{
				// Bitmap is in portrait format (height > width). The height will be the longest dimension.
				newHeight = maxLength;
				newWidth = originalWidth*newHeight/originalHeight;
			}

			width = newWidth;
			height = newHeight;
		}

		/// <summary>
		/// Overlay the text and/or image watermark over the image specified in the <paramref name="filePath" /> parameter and return.
		/// </summary>
		/// <param name="filePath">A string representing the full path to the image file
		/// (e.g. "C:\mypics\myprettypony.jpg", "myprettypony.jpg").</param>
		/// <param name="galleryId">The gallery ID. The watermark associated with this gallery is applied to the file.</param>
		/// <returns>
		/// Returns a System.Drawing.Image instance containing the image with the watermark applied.
		/// </returns>
		public static System.Drawing.Image AddWatermark(string filePath, int galleryId)
		{
			Watermark wm = Factory.GetWatermarkInstance(galleryId);
			return wm.ApplyWatermark(filePath);
		}

		/// <summary>
		/// Create a new Bitmap with the specified dimensions.
		/// </summary>
		/// <param name="inputBmp">The source bitmap to use.</param>
		/// <param name="sourceBmpWidth">The width of the input bitmap. This should be equal to inputBmp.Size.Width, but it is added as
		/// a parameter so that calling code can send a cached value rather than requiring this method to query the bitmap for the data.
		/// If a value less than zero is specified, then inputBmp.Size.Width is used.
		/// </param>
		/// <param name="sourceBmpHeight">The height of the input bitmap. This should be equal to inputBmp.Size.Height, but it is added as
		/// a parameter so that calling code can send a cached value rather than requiring this method to query the bitmap for the data.</param>
		/// If a value less than zero is specified, then inputBmp.Size.Height is used.
		/// <param name="newWidth">The width of the new bitmap.</param>
		/// <param name="newHeight">The height of the new bitmap.</param>
		/// <returns>Returns a new Bitmap with the specified dimensions.</returns>
		/// <exception cref="GalleryServerPro.ErrorHandler.CustomExceptions.UnsupportedImageTypeException">Thrown when <paramref name="inputBmp"/> 
		/// cannot be resized to the requested dimensions. Typically this will occur during 
		/// <see cref="Graphics.DrawImage(Image, Rectangle, Rectangle, GraphicsUnit)"/> because there is not enough system memory.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="inputBmp" /> is null.</exception>
		public static Bitmap CreateResizedBitmap(System.Drawing.Image inputBmp, int sourceBmpWidth, int sourceBmpHeight, int newWidth, int newHeight)
		{
			//Adapted (but mostly copied) from http://www.codeproject.com/cs/media/bitmapmanip.asp
			//Create a new bitmap object based on the input
			if (inputBmp == null)
				throw new ArgumentNullException("inputBmp");

			if (sourceBmpWidth <= 0)
				sourceBmpWidth = inputBmp.Size.Width;

			if (sourceBmpHeight <= 0)
				sourceBmpHeight = inputBmp.Size.Height;

			double xScaleFactor = (float) newWidth/(float) sourceBmpWidth;
			double yScaleFactor = (float) newHeight/(float) sourceBmpHeight;

			int calculatedNewWidth = (int) (sourceBmpWidth*xScaleFactor);
			int calculatedNewHeight = (int) (sourceBmpHeight*yScaleFactor);

			if (calculatedNewWidth <= 0)
			{
				calculatedNewWidth = 1; // Make sure the value is at least 1.
				xScaleFactor = (float) calculatedNewWidth/(float) sourceBmpWidth; // Update the scale factor to reflect the new width
			}

			if (calculatedNewHeight <= 0)
			{
				calculatedNewHeight = 1; // Make sure the value is at least 1.
				yScaleFactor = (float) calculatedNewHeight/(float) sourceBmpHeight; // Update the scale factor to reflect the new height
			}

			Bitmap newBmp = null;
			try
			{
				newBmp = new Bitmap(calculatedNewWidth, calculatedNewHeight, PixelFormat.Format24bppRgb); //Graphics.FromImage doesn't like Indexed pixel format

				//Create a graphics object attached to the new bitmap
				using (Graphics newBmpGraphics = Graphics.FromImage(newBmp))
				{
					newBmpGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

					// Make background white. Without this a thin grey line is rendered along the top and left.
					// See http://social.msdn.microsoft.com/Forums/en-US/winforms/thread/2c9ac8d0-366c-4919-8f92-3a91c56f41e0/
					newBmpGraphics.Clear(Color.White);

					newBmpGraphics.ScaleTransform((float) xScaleFactor, (float) yScaleFactor);

					//Draw the bitmap in the graphics object, which will apply the scale transform.
					//Note that pixel units must be specified to ensure the framework doesn't attempt
					//to compensate for varying horizontal resolutions in images by resizing; in this case,
					//that's the opposite of what we want.
					Rectangle drawRect = new Rectangle(0, 0, sourceBmpWidth, sourceBmpHeight);

					lock (_sharedLock)
					{
						try
						{
							try
							{
								newBmpGraphics.DrawImage(inputBmp, drawRect, drawRect, GraphicsUnit.Pixel);
							}
							catch (OutOfMemoryException)
							{
								// The garbage collector will automatically run to try to clean up memory, so let's wait for it to finish and 
								// try again. If it still doesn't work because the image is just too large and the system doesn't have enough
								// memory, catch the OutOfMemoryException and throw one of our UnsupportedImageTypeException exceptions instead.
								GC.WaitForPendingFinalizers();
								newBmpGraphics.DrawImage(inputBmp, drawRect, drawRect, GraphicsUnit.Pixel);
							}
						}
						catch (OutOfMemoryException)
						{
							throw new ErrorHandler.CustomExceptions.UnsupportedImageTypeException();
						}
					}
				}
			}
			catch
			{
				if (newBmp != null)
					newBmp.Dispose();

				throw;
			}

			return newBmp;
		}

		/// <summary>
		/// Persist the specified image to disk at the specified path. If the directory to contain the file does not exist, it
		/// is automatically created.
		/// </summary>
		/// <param name="image">The image to persist to disk.</param>
		/// <param name="newFilePath">The full physical path, including the file name to where the image is to be stored. Ex: C:\mypics\cache\2008\May\flower.jpg</param>
		/// <param name="imageFormat">The file format for the image.</param>
		/// <param name="jpegQuality">The quality value to save JPEG images at. This is a value between 1 and 100. This parameter
		/// is ignored if the image format is not JPEG.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="imageFormat" /> is null.</exception>
		public static void SaveImageToDisk(System.Drawing.Image image, string newFilePath, ImageFormat imageFormat, int jpegQuality)
		{
			if (imageFormat == null)
				throw new ArgumentNullException("imageFormat");

			VerifyDirectoryExistsForNewFile(newFilePath);

			if (imageFormat.Equals(ImageFormat.Jpeg))
				SaveJpgImageToDisk(image, newFilePath, jpegQuality);
			else
				SaveNonJpgImageToDisk(image, newFilePath, imageFormat);
		}

		#endregion

		#region Private Static Methods

		private static void SaveJpgImageToDisk(System.Drawing.Image image, string newFilepath, long jpegQuality)
		{
			//Save the image in the JPG format using the specified compression value.
			using (EncoderParameters eps = new EncoderParameters(1))
			{
				eps.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, jpegQuality);
				ImageCodecInfo ici = GetEncoderInfo("image/jpeg");
				image.Save(newFilepath, ici, eps);
			}
		}

		/// <summary>
		/// Make sure the directory exists for the file at the specified path. It is created if it does not exist. 
		/// (For example, it might not exist when the user changes the thumbnail or optimized location and subsequently 
		/// synchronizes. This process creates a new directory structure to match the directory structure where the 
		/// originals are stored, and there may be cases where we need to save a file to a directory that doesn't yet exist.
		/// </summary>
		/// <param name="newFilepath">The full physical path for which to verify the directory exists. Ex: C:\mypics\cache\2008\May\flower.jpg</param>
		private static void VerifyDirectoryExistsForNewFile(string newFilepath)
		{
			if (!Directory.Exists(Path.GetDirectoryName(newFilepath)))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(newFilepath));
			}
		}

		private static void SaveNonJpgImageToDisk(System.Drawing.Image image, string newFilepath, System.Drawing.Imaging.ImageFormat imgFormat)
		{
			image.Save(newFilepath, imgFormat);
		}

		private static ImageCodecInfo GetEncoderInfo(String mimeType)
		{
			//Get the image codec information for the specified mime type.
			ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
			for (int j = 0; j < encoders.Length; ++j)
			{
				if (encoders[j].MimeType == mimeType)
					return encoders[j];
			}
			return null;
		}

		#endregion
	}
}
