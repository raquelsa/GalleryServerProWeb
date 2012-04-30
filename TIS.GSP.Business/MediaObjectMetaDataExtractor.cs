using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.Business.Properties;

namespace GalleryServerPro.Business.Metadata
{
	///<summary>Provides access to and encapsulates functionality around the metadata in media objects such as JPEG, TIFF, PNG image 
	/// files and video files. For images, this class supports any media object that is recognized by the .NET Framework's
	/// <see cref="System.Drawing.Image" /> class and contains valid metadata, such as EXIF data. Video metadata is extracted using
	/// FFmpeg and supports any video file supported by FFmpeg.
	///</summary>
	public class MediaObjectMetadataExtractor
	{
		#region Private Fields

		// Contains an image's System.Drawing.Image.PropertyItems property.
		private System.Drawing.Imaging.PropertyItem[] _propertyItems;

		private readonly string _mediaObjectFilePath;
		private readonly int _galleryId;
		private readonly MimeTypeCategory _mimeTypeCategory;
		private string _ffmpegOutput;
		private int _width, _height;
		private IGalleryObjectMetadataItemCollection _metadataItems;

		private Dictionary<RawMetadataItemName, MetadataItem> _rawMetadata;

		#endregion

		#region Constructors

		/// <overloads>
		/// Initializes a new instance of the <see cref="MediaObjectMetadataExtractor"/> class. This object can
		/// interact with the metadata contained in the specified media object.
		/// </overloads>
		/// <summary>
		/// Initializes a new instance of the <see cref="MediaObjectMetadataExtractor"/> class. This object can
		/// interact with the metadata contained in the specified media object.
		/// </summary>
		/// <param name="mediaObjectFilePath">The path, either absolute or relative, that indicates the
		/// location of the media object file on disk. This value is used in the <see cref="FileStream"/> constructor.
		/// (e.g. C:\folder1\folder2\sunset.jpg, sunset.jpg).</param>
		/// <param name="galleryId">The ID of the gallery the <paramref name="mediaObjectFilePath"/> is associated with.</param>
		/// <exception cref="OutOfMemoryException">
		/// Thrown when the <paramref name="mediaObjectFilePath"/> is an image that is too large to be loaded into memory.</exception>
		public MediaObjectMetadataExtractor(string mediaObjectFilePath, int galleryId)
			: this(mediaObjectFilePath, galleryId, String.Empty)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MediaObjectMetadataExtractor"/> class. This object can
		/// interact with the metadata contained in the specified media object.
		/// </summary>
		/// <param name="mediaObjectFilePath">The path, either absolute or relative, that indicates the
		/// location of the media object file on disk. This value is used in the <see cref="FileStream"/> constructor.
		/// (e.g. C:\folder1\folder2\sunset.jpg, sunset.jpg).</param>
		/// <param name="galleryId">The ID of the gallery the <paramref name="mediaObjectFilePath"/> is associated with.</param>
		/// <param name="ffmpegOutput">The output from the utility FFmpeg. This data can be parsed for pieces of metadata for videos. Optional. 
		/// When null or an empty string, this utility will automatically run FFmpeg to generate the needed data. This parameter is used
		/// only for video media objects.</param>
		/// <exception cref="OutOfMemoryException">
		/// Thrown when the <paramref name="mediaObjectFilePath"/> is an image that is too large to be loaded into memory.</exception>
		public MediaObjectMetadataExtractor(string mediaObjectFilePath, int galleryId, string ffmpegOutput)
		{
			this._galleryId = galleryId;
			this._mediaObjectFilePath = mediaObjectFilePath;
			this._ffmpegOutput = ffmpegOutput;

			IMimeType mimeType = MimeType.LoadMimeType(galleryId, mediaObjectFilePath);
			this._mimeTypeCategory = (mimeType != null ? mimeType.TypeCategory : MimeTypeCategory.NotSet);

			if (this._mimeTypeCategory == MimeTypeCategory.Image)
			{
				ExtractImagePropertyItems(mediaObjectFilePath);
			}
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets the raw metadata associated with the current media object.
		/// </summary>
		/// <value>The raw metadata associated with the current media object.</value>
		public Dictionary<RawMetadataItemName, MetadataItem> RawMetadata
		{
			get
			{
				if (this._rawMetadata == null)
				{
					FillRawMetadataDictionary();
				}
				return this._rawMetadata;
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets a collection of <see cref="IGalleryObjectMetadataItem" /> objects. The collection includes one item for each
		/// <see cref="FormattedMetadataItemName" /> value, unless that metadata item does not exist in the image's metadata. In that case, no item
		/// is generated. 
		/// </summary>
		/// <returns>Returns a <see cref="IGalleryObjectMetadataItemCollection" /> object.</returns>
		/// <remarks>The collection is created the first time this method is called. Subsequent calls return the cached collection
		/// rather than regenerating it from the image file.</remarks>
		public IGalleryObjectMetadataItemCollection GetGalleryObjectMetadataItemCollection()
		{
			if (this._metadataItems == null)
			{
				this._metadataItems = new GalleryObjectMetadataItemCollection();

				AddFileMetadata(_metadataItems);

				if (this._mimeTypeCategory == MimeTypeCategory.Image)
				{
					// The AddWpfBitmapMetadata function requires .NET Framework 3.0 and running under Full Trust, so only call if 
					// these conditions are satisfied. There is also a config setting that enables this functionality, so query that
					// as well. (The config setting allows it to be disabled due to the reliability issues found with the WPF classes.)
					if ((AppSetting.Instance.AppTrustLevel == ApplicationTrustLevel.Full)
							&& (AppSetting.Instance.DotNetFrameworkVersion > new Version("2.0"))
							&& (Factory.LoadGallerySetting(_galleryId).ExtractMetadataUsingWpf))
					{
						AddWpfBitmapMetadata(_metadataItems);
					}

					AddExifMetadata(_metadataItems);
				}

				if (this._mimeTypeCategory == MimeTypeCategory.Video || (this._mimeTypeCategory == MimeTypeCategory.Audio))
				{
					AddVideoMetadata(_metadataItems);
				}

				RemoveInvalidHtmlAndScripts(_metadataItems, _galleryId);
			}

			return _metadataItems;
		}

		/// <summary>
		/// Return the specified raw (unformatted) metadata item. If no matching item is found, null is returned.
		/// </summary>
		/// <param name="metaItemName">The metadata item name to return.</param>
		/// <returns>Returns the specified raw (unformatted) meta item. If no matching item is found, null is returned.</returns>
		public MetadataItem GetRawMetadataItem(RawMetadataItemName metaItemName)
		{
			return this.RawMetadata[metaItemName];
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Removes any HTML and javascript from the metadata values that are not allowed.
		/// </summary>
		/// <param name="metadataItems">The metadata items.</param>
		/// <param name="galleryId">The gallery ID. This is used to look up the appropriate configuration values for the gallery.</param>
		private static void RemoveInvalidHtmlAndScripts(IEnumerable<IGalleryObjectMetadataItem> metadataItems, int galleryId)
		{
			foreach (IGalleryObjectMetadataItem metadataItem in metadataItems)
			{
				metadataItem.Value = HtmlValidator.Clean(metadataItem.Value, galleryId);
			}
		}

		/// <summary>
		/// Fill the class-level _rawMetadata dictionary with MetadataItem objects created from the
		/// PropertyItems property of the image. Skip any items that are not defined in the 
		/// RawMetadataItemName enumeration.
		/// </summary>
		private void FillRawMetadataDictionary()
		{
			this._rawMetadata = new Dictionary<RawMetadataItemName, MetadataItem>();

			foreach (System.Drawing.Imaging.PropertyItem itemIterator in this._propertyItems)
			{
				RawMetadataItemName metadataName = (RawMetadataItemName)itemIterator.Id;
				if (Enum.IsDefined(typeof(RawMetadataItemName), metadataName))
				{
					if (!this._rawMetadata.ContainsKey(metadataName))
					{
						MetadataItem metadataItem = new MetadataItem(itemIterator);
						if (metadataItem.Value != null)
							this._rawMetadata.Add(metadataName, metadataItem);
					}
				}
			}
		}

		/// <summary>
		/// Extract the property items of the specified image to the class-level field variable.
		/// </summary>
		/// <param name="imageFilePath">The path, either absolute or relative, that indicates the
		/// location of the image file on disk. This value is used in the FileStream constructor.
		/// (e.g. C:\folder1\folder2\sunset.jpg, sunset.jpg).</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="imageFilePath" /> is null.</exception>
		private void ExtractImagePropertyItems(string imageFilePath)
		{
			if (String.IsNullOrEmpty(imageFilePath))
				throw new ArgumentNullException("imageFilePath");

			if (AppSetting.Instance.AppTrustLevel == ApplicationTrustLevel.Full)
			{
				GetPropertyItemsUsingFullTrustTechnique(imageFilePath);
			}
			else
			{
				GetPropertyItemsUsingLimitedTrustTechnique(imageFilePath);
			}
		}

		private void GetPropertyItemsUsingFullTrustTechnique(string imageFilePath)
		{
			// This technique is fast but requires full trust. Can only be called when app is running under full trust.
			if (AppSetting.Instance.AppTrustLevel != ApplicationTrustLevel.Full)
				throw new InvalidOperationException("The method MediaObjectMetadataExtractor.GetPropertyItemsUsingFullTrustTechnique can only be called when the application is running under full trust. The application should have already checked for this before calling this method. The developer needs to modify the source code to fix this.");

			using (Stream stream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				try
				{
					using (System.Drawing.Image image = System.Drawing.Image.FromStream(stream, true, false))
					{
						try
						{
							this._propertyItems = image.PropertyItems;
						}
						catch (NotImplementedException ex)
						{
							// Some images, such as wmf, throw this exception. We'll make a note of it and set our field to an empty array.
							if (!ex.Data.Contains("Metadata Extraction Error"))
							{
								ex.Data.Add("Metadata Extraction Error", String.Format(CultureInfo.CurrentCulture, "Cannot extract metadata from file \"{0}\".", imageFilePath));
							}

							LogError(ex, this._galleryId);
							this._propertyItems = new System.Drawing.Imaging.PropertyItem[0];
						}
					}
				}
				catch (ArgumentException ex)
				{
					if (!ex.Data.Contains("Metadata Extraction Error"))
					{
						ex.Data.Add("Metadata Extraction Error", String.Format(CultureInfo.CurrentCulture, "Cannot extract metadata from file \"{0}\".", imageFilePath));
					}

					LogError(ex, this._galleryId);
					this._propertyItems = new System.Drawing.Imaging.PropertyItem[0];
				}
			}
		}

		private void GetPropertyItemsUsingLimitedTrustTechnique(string imageFilePath)
		{
			// This technique is not as fast as the one in the method GetPropertyItemsUsingFullTrustTechnique() but in works in limited
			// trust environments.
			try
			{
				using (System.Drawing.Image image = new System.Drawing.Bitmap(imageFilePath))
				{
					try
					{
						this._propertyItems = image.PropertyItems;
					}
					catch (NotImplementedException ex)
					{
						// Some images, such as wmf, throw this exception. We'll make a note of it and set our field to an empty array.
						LogError(ex, this._galleryId);
						this._propertyItems = new System.Drawing.Imaging.PropertyItem[0];
					}
				}
			}
			catch (ArgumentException ex)
			{
				LogError(ex, this._galleryId);
				this._propertyItems = new System.Drawing.Imaging.PropertyItem[0];
			}
		}

		//private void OutputToDebugWindow()
		//{
		//  foreach (RawMetadataItemName metadataItemName in Enum.GetValues(typeof(RawMetadataItemName)))
		//  {
		//    MetadataItem rawMdi;
		//    if (RawMetadata.TryGetValue(metadataItemName, out rawMdi))
		//    {
		//      string rawValue = rawMdi.Value.ToString().Trim().TrimEnd(new char[] {'\0'});
		//      string msg = String.Format(CultureInfo.CurrentCulture, "{0}: {1} (ID {2}, {3}, {4})", metadataItemName, rawValue, rawMdi.PropertyItem.Id, rawMdi.ExtractedValueType, rawMdi.PropertyTagType);
		//      System.Diagnostics.Trace.WriteLine(msg);
		//    }
		//  }
		//}

		private void AddExifMetadata(IGalleryObjectMetadataItemCollection metadataItems)
		{
			//OutputToDebugWindow();

			foreach (FormattedMetadataItemName metadataItemName in Enum.GetValues(typeof(FormattedMetadataItemName)))
			{
				IGalleryObjectMetadataItem mdi = null;
				switch (metadataItemName)
				{
					case FormattedMetadataItemName.Author: mdi = GetStringMetadataItem(RawMetadataItemName.Artist, FormattedMetadataItemName.Author, Resources.Metadata_Author); break;
					case FormattedMetadataItemName.CameraModel: mdi = GetStringMetadataItem(RawMetadataItemName.EquipModel, FormattedMetadataItemName.CameraModel, Resources.Metadata_CameraModel); break;
					case FormattedMetadataItemName.ColorRepresentation: mdi = GetColorRepresentationMetadataItem(); break;
					case FormattedMetadataItemName.Comment: mdi = GetStringMetadataItem(RawMetadataItemName.ExifUserComment, FormattedMetadataItemName.Comment, Resources.Metadata_Comment); break;
					case FormattedMetadataItemName.Copyright: mdi = GetStringMetadataItem(RawMetadataItemName.Copyright, FormattedMetadataItemName.Copyright, Resources.Metadata_Copyright); break;
					case FormattedMetadataItemName.DatePictureTaken: mdi = GetDatePictureTakenMetadataItem(); break;
					case FormattedMetadataItemName.Description: mdi = GetStringMetadataItem(RawMetadataItemName.ImageDescription, FormattedMetadataItemName.Description, Resources.Metadata_Description); break;
					case FormattedMetadataItemName.Dimensions: mdi = GetDimensionsMetadataItem(); break;
					case FormattedMetadataItemName.EquipmentManufacturer: mdi = GetStringMetadataItem(RawMetadataItemName.EquipMake, FormattedMetadataItemName.EquipmentManufacturer, Resources.Metadata_EquipmentManufacturer); break;
					case FormattedMetadataItemName.ExposureCompensation: mdi = GetExposureCompensationMetadataItem(); break;
					case FormattedMetadataItemName.ExposureProgram: mdi = GetExposureProgramMetadataItem(); break;
					case FormattedMetadataItemName.ExposureTime: mdi = GetExposureTimeMetadataItem(); break;
					case FormattedMetadataItemName.FlashMode: mdi = GetFlashModeMetadataItem(); break;
					case FormattedMetadataItemName.FNumber: mdi = GetFNumberMetadataItem(); break;
					case FormattedMetadataItemName.FocalLength: mdi = GetFocalLengthMetadataItem(); break;
					case FormattedMetadataItemName.Height: mdi = GetHeightMetadataItem(); break;
					case FormattedMetadataItemName.HorizontalResolution: mdi = GetXResolutionMetadataItem(); break;
					case FormattedMetadataItemName.IsoSpeed: mdi = GetStringMetadataItem(RawMetadataItemName.ExifISOSpeed, FormattedMetadataItemName.IsoSpeed, Resources.Metadata_IsoSpeed); break;
					case FormattedMetadataItemName.Keywords: break; // No way to access keywords through Exif, so just skip this one
					case FormattedMetadataItemName.LensAperture: mdi = GetApertureMetadataItem(); break;
					case FormattedMetadataItemName.LightSource: mdi = GetLightSourceMetadataItem(); break;
					case FormattedMetadataItemName.MeteringMode: mdi = GetMeteringModeMetadataItem(); break;
					case FormattedMetadataItemName.Rating: break; // No way to access rating through Exif, so just skip this one
					case FormattedMetadataItemName.Subject: break; // No way to access rating through Exif, so just skip this one
					case FormattedMetadataItemName.SubjectDistance: mdi = GetSubjectDistanceMetadataItem(); break;
					case FormattedMetadataItemName.Title: mdi = GetStringMetadataItem(RawMetadataItemName.ImageTitle, FormattedMetadataItemName.Title, Resources.Metadata_Title); break;
					case FormattedMetadataItemName.VerticalResolution: mdi = GetYResolutionMetadataItem(); break;
					case FormattedMetadataItemName.Width: mdi = GetWidthMetadataItem(); break;
				}
				if ((mdi != null) && (!metadataItems.Contains(mdi)))
				{
					metadataItems.Add(mdi);
				}
			}
		}

		/// <summary>
		/// Gets a metadata item containing the date the picture was taken. The date format conforms to the IETF RFC 1123 specification,
		/// which means it uses this format string: "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'" (e.g. "Mon, 17 Apr 2006 21:38:09 GMT"). See 
		/// the DateTimeFormatInfo.RFC1123Pattern property for more information about the format. Returns null if no date is found 
		/// in the metadata.
		/// </summary>
		/// <returns>Returns a metadata item containing the date the picture was taken. Returns null if no date is found 
		/// in the metadata.</returns>
		private IGalleryObjectMetadataItem GetDatePictureTakenMetadataItem()
		{
			IGalleryObjectMetadataItem mdi = null;
			MetadataItem rawMdi;
			if (RawMetadata.TryGetValue(RawMetadataItemName.ExifDTOrig, out rawMdi))
			{
				DateTime convertedDateTimeValue = ConvertExifDateTimeToDateTime(rawMdi.Value.ToString());
				if (convertedDateTimeValue > DateTime.MinValue)
				{
					mdi = new GalleryObjectMetadataItem(int.MinValue, FormattedMetadataItemName.DatePictureTaken, Resources.Metadata_DatePictureTaken, convertedDateTimeValue.ToString(GlobalConstants.DateTimeFormatStringForMetadata, CultureInfo.InvariantCulture), true);
				}
				else if (!String.IsNullOrEmpty(rawMdi.Value.ToString()))
				{
					mdi = new GalleryObjectMetadataItem(int.MinValue, FormattedMetadataItemName.DatePictureTaken, Resources.Metadata_DatePictureTaken, rawMdi.Value.ToString(), true);
				}
			}
			return mdi;
		}

		/// <summary>
		/// Convert an EXIF-formatted timestamp to the .NET DateTime type. Returns DateTime.MinValue when the date cannot be parsed.
		/// </summary>
		/// <param name="exifDateTime">An EXIF-formatted timestamp. The format is YYYY:MM:DD HH:MM:SS with time shown 
		/// in 24-hour format and the date and time separated by one blank character (0x2000). The character 
		/// string length is 20 bytes including the NULL terminator.</param>
		/// <returns>Returns the EXIF-formatted timestamp as a .NET DateTime type.</returns>
		private static DateTime ConvertExifDateTimeToDateTime(string exifDateTime)
		{
			DateTime convertedDateTimeValue = DateTime.MinValue;
			const int minCharsReqdToSpecifyDate = 10; // Need at least 10 characters to specify a date (e.g. 2010:10:15)

			if (String.IsNullOrEmpty(exifDateTime) || (exifDateTime.Trim().Length < minCharsReqdToSpecifyDate))
				return convertedDateTimeValue; // No date/time is present; just return

			exifDateTime = exifDateTime.Trim();

			string[] ymdhms = exifDateTime.Split(new char[] { ' ', ':' });

			// Default to lowest possible year, first month and first day
			int year = DateTime.MinValue.Year, month = 1, day = 1, hour = 0, minute = 0, second = 0;

			if (ymdhms.Length >= 2)
			{
				Int32.TryParse(ymdhms[0], out year);
				Int32.TryParse(ymdhms[1], out month);
				Int32.TryParse(ymdhms[2], out day);
			}

			if (ymdhms.Length >= 6)
			{
				// The hour, minute and second will default to 0 if it can't be parsed, which is good.
				Int32.TryParse(ymdhms[3], out hour);
				Int32.TryParse(ymdhms[4], out minute);
				Int32.TryParse(ymdhms[5], out second);
			}
			if (year > DateTime.MinValue.Year)
			{
				try
				{
					convertedDateTimeValue = new DateTime(year, month, day, hour, minute, second);
				}
				catch (ArgumentOutOfRangeException) { }
				catch (ArgumentException) { }
			}

			return convertedDateTimeValue;
		}

		private IGalleryObjectMetadataItem GetFocalLengthMetadataItem()
		{
			IGalleryObjectMetadataItem mdi = null;
			MetadataItem rawMdi;
			if (RawMetadata.TryGetValue(RawMetadataItemName.ExifFocalLength, out rawMdi))
			{
				if (rawMdi.ExtractedValueType == ExtractedValueType.Fraction)
				{
					float value = ((Fraction)rawMdi.Value).ToSingle();
					string formattedValue = String.Concat(Math.Round(value), " ", Resources.Metadata_FocalLength_Units);
					mdi = new GalleryObjectMetadataItem(int.MinValue, FormattedMetadataItemName.FocalLength, Resources.Metadata_FocalLength, formattedValue, true);
				}
			}
			return mdi;
		}

		private IGalleryObjectMetadataItem GetExposureCompensationMetadataItem()
		{
			IGalleryObjectMetadataItem mdi = null;
			MetadataItem rawMdi;
			if (RawMetadata.TryGetValue(RawMetadataItemName.ExifExposureBias, out rawMdi))
			{
				if (rawMdi.ExtractedValueType == ExtractedValueType.Fraction)
				{
					float value = ((Fraction)rawMdi.Value).ToSingle();
					string formattedValue = String.Concat(value.ToString("##0.# ", CultureInfo.InvariantCulture), Resources.Metadata_ExposureCompensation_Suffix);
					mdi = new GalleryObjectMetadataItem(int.MinValue, FormattedMetadataItemName.ExposureCompensation, Resources.Metadata_ExposureCompensation, formattedValue, true);
				}
			}
			return mdi;
		}

		private IGalleryObjectMetadataItem GetFNumberMetadataItem()
		{
			IGalleryObjectMetadataItem mdi = null;
			MetadataItem rawMdi;
			if (RawMetadata.TryGetValue(RawMetadataItemName.ExifFNumber, out rawMdi))
			{
				if (rawMdi.ExtractedValueType == ExtractedValueType.Fraction)
				{
					float value = ((Fraction)rawMdi.Value).ToSingle();
					string formattedValue = value.ToString("f/##0.#", CultureInfo.InvariantCulture);
					mdi = new GalleryObjectMetadataItem(int.MinValue, FormattedMetadataItemName.FNumber, Resources.Metadata_FNumber, formattedValue, true);
				}
			}
			return mdi;
		}

		private IGalleryObjectMetadataItem GetMeteringModeMetadataItem()
		{
			IGalleryObjectMetadataItem mdi = null;
			MetadataItem rawMdi;
			if (RawMetadata.TryGetValue(RawMetadataItemName.ExifMeteringMode, out rawMdi))
			{
				if (rawMdi.ExtractedValueType == ExtractedValueType.Int64)
				{
					MeteringMode meterMode = (MeteringMode)(Int64)rawMdi.Value;
					if (MetadataEnumHelper.IsValidMeteringMode(meterMode))
					{
						mdi = new GalleryObjectMetadataItem(int.MinValue, FormattedMetadataItemName.MeteringMode, Resources.Metadata_MeteringMode, meterMode.ToString(), true);
					}
				}
			}
			return mdi;
		}

		private IGalleryObjectMetadataItem GetLightSourceMetadataItem()
		{
			IGalleryObjectMetadataItem mdi = null;
			MetadataItem rawMdi;
			if (RawMetadata.TryGetValue(RawMetadataItemName.ExifLightSource, out rawMdi))
			{
				if (rawMdi.ExtractedValueType == ExtractedValueType.Int64)
				{
					LightSource lightSource = (LightSource)(Int64)rawMdi.Value;
					if (MetadataEnumHelper.IsValidLightSource(lightSource))
					{
						// Don't bother with it if it is "Unknown"
						if (lightSource != LightSource.Unknown)
						{
							mdi = new GalleryObjectMetadataItem(int.MinValue, FormattedMetadataItemName.LightSource, Resources.Metadata_LightSource, lightSource.ToString(), true);
						}
					}
				}
			}
			return mdi;
		}

		private IGalleryObjectMetadataItem GetApertureMetadataItem()
		{
			// The aperture is the same as the F-Number if present; otherwise it is calculated from ExifAperture.
			IGalleryObjectMetadataItem mdi = null;
			MetadataItem rawMdi;
			string aperture = String.Empty;

			if (RawMetadata.TryGetValue(RawMetadataItemName.ExifFNumber, out rawMdi))
			{
				if (rawMdi.ExtractedValueType == ExtractedValueType.Fraction)
				{
					float exifFNumber = ((Fraction)rawMdi.Value).ToSingle();
					aperture = exifFNumber.ToString("f/##0.#", CultureInfo.InvariantCulture);
				}
			}

			if ((String.IsNullOrEmpty(aperture)) && (RawMetadata.TryGetValue(RawMetadataItemName.ExifAperture, out rawMdi)))
			{
				if (rawMdi.ExtractedValueType == ExtractedValueType.Fraction)
				{
					float exifAperture = ((Fraction)rawMdi.Value).ToSingle();
					float exifFNumber = (float)Math.Round(Math.Pow(Math.Sqrt(2), exifAperture), 1);
					aperture = exifFNumber.ToString("f/##0.#", CultureInfo.InvariantCulture);
				}
			}

			if (!String.IsNullOrEmpty(aperture))
			{
				mdi = new GalleryObjectMetadataItem(int.MinValue, FormattedMetadataItemName.LensAperture, Resources.Metadata_LensAperture, aperture, true);
			}

			return mdi;
		}

		private IGalleryObjectMetadataItem GetXResolutionMetadataItem()
		{
			IGalleryObjectMetadataItem mdi = null;
			MetadataItem rawMdi;
			string resolutionUnit = String.Empty;

			if (RawMetadata.TryGetValue(RawMetadataItemName.ResolutionXUnit, out rawMdi))
			{
				resolutionUnit = rawMdi.Value.ToString();
			}

			if ((String.IsNullOrEmpty(resolutionUnit)) && (RawMetadata.TryGetValue(RawMetadataItemName.ResolutionUnit, out rawMdi)))
			{
				if (rawMdi.ExtractedValueType == ExtractedValueType.Int64)
				{
					ResolutionUnit resUnit = (ResolutionUnit)(Int64)rawMdi.Value;
					if (MetadataEnumHelper.IsValidResolutionUnit(resUnit))
					{
						resolutionUnit = resUnit.ToString();
					}
				}
			}

			if (RawMetadata.TryGetValue(RawMetadataItemName.XResolution, out rawMdi))
			{
				string xResolution;
				if (rawMdi.ExtractedValueType == ExtractedValueType.Fraction)
				{
					xResolution = Math.Round(((Fraction)rawMdi.Value).ToSingle(), 2).ToString(CultureInfo.InvariantCulture);
				}
				else
				{
					xResolution = rawMdi.Value.ToString();
				}

				string xResolutionString = String.Concat(xResolution, " ", resolutionUnit);
				mdi = new GalleryObjectMetadataItem(int.MinValue, FormattedMetadataItemName.HorizontalResolution, Resources.Metadata_HorizontalResolution, xResolutionString, true);
			}

			return mdi;
		}

		private IGalleryObjectMetadataItem GetYResolutionMetadataItem()
		{
			IGalleryObjectMetadataItem mdi = null;
			MetadataItem rawMdi;
			string resolutionUnit = String.Empty;

			if (RawMetadata.TryGetValue(RawMetadataItemName.ResolutionYUnit, out rawMdi))
			{
				resolutionUnit = rawMdi.Value.ToString();
			}

			if ((String.IsNullOrEmpty(resolutionUnit)) && (RawMetadata.TryGetValue(RawMetadataItemName.ResolutionUnit, out rawMdi)))
			{
				if (rawMdi.ExtractedValueType == ExtractedValueType.Int64)
				{
					ResolutionUnit resUnit = (ResolutionUnit)(Int64)rawMdi.Value;
					if (MetadataEnumHelper.IsValidResolutionUnit(resUnit))
					{
						resolutionUnit = resUnit.ToString();
					}
				}
			}

			if (RawMetadata.TryGetValue(RawMetadataItemName.YResolution, out rawMdi))
			{
				string yResolution;
				if (rawMdi.ExtractedValueType == ExtractedValueType.Fraction)
				{
					yResolution = Math.Round(((Fraction)rawMdi.Value).ToSingle(), 2).ToString(CultureInfo.InvariantCulture);
				}
				else
				{
					yResolution = rawMdi.Value.ToString();
				}

				string yResolutionString = String.Concat(yResolution, " ", resolutionUnit);
				mdi = new GalleryObjectMetadataItem(int.MinValue, FormattedMetadataItemName.VerticalResolution, Resources.Metadata_VerticalResolution, yResolutionString, true);
			}

			return mdi;
		}

		private IGalleryObjectMetadataItem GetDimensionsMetadataItem()
		{
			IGalleryObjectMetadataItem mdi = null;
			int width = GetWidth();
			int height = GetHeight();

			if ((width > 0) && (height > 0))
			{
				mdi = new GalleryObjectMetadataItem(int.MinValue, FormattedMetadataItemName.Dimensions, Resources.Metadata_Dimensions, String.Concat(width, " x ", height), true);
			}

			return mdi;
		}

		/// <summary>
		/// Get the height of the media object. Extracted from RawMetadataItemName.ExifPixXDim for compressed images and
		/// from RawMetadataItemName.ImageHeight for uncompressed images. The value is stored in a private class level variable
		/// for quicker subsequent access.
		/// </summary>
		/// <returns>Returns the height of the media object.</returns>
		private int GetWidth()
		{
			if (_width > 0)
				return _width;

			MetadataItem rawMdi;
			int width = int.MinValue;
			bool foundWidth = false;

			// Compressed images store their width in ExifPixXDim. Uncompressed images store their width in ImageWidth.
			// First look in ExifPixXDim since most images are likely to be compressed ones. If we don't find that one,
			// look for ImageWidth. If we don't find that one either (which should be unlikely to ever happen), then just give 
			// up and return null.
			if (RawMetadata.TryGetValue(RawMetadataItemName.ExifPixXDim, out rawMdi))
			{
				foundWidth = Int32.TryParse(rawMdi.Value.ToString(), out width);
			}

			if ((!foundWidth) && (RawMetadata.TryGetValue(RawMetadataItemName.ImageWidth, out rawMdi)))
			{
				foundWidth = Int32.TryParse(rawMdi.Value.ToString(), out width);
			}

			if (foundWidth)
				this._width = width;

			return width;
		}

		/// <summary>
		/// Get the width of the media object. Extracted from RawMetadataItemName.ExifPixYDim for compressed images and
		/// from RawMetadataItemName.ImageWidth for uncompressed images. The value is stored in a private class level variable
		/// for quicker subsequent access.
		/// </summary>
		/// <returns>Returns the width of the media object.</returns>
		private int GetHeight()
		{
			if (_height > 0)
				return _height;

			MetadataItem rawMdi;
			int height = int.MinValue;
			bool foundHeight = false;

			// Compressed images store their width in ExifPixXDim. Uncompressed images store their width in ImageWidth.
			// First look in ExifPixXDim since most images are likely to be compressed ones. If we don't find that one,
			// look for ImageWidth. If we don't find that one either (which should be unlikely to ever happen), then just give 
			// up and return null.
			if (RawMetadata.TryGetValue(RawMetadataItemName.ExifPixYDim, out rawMdi))
			{
				foundHeight = Int32.TryParse(rawMdi.Value.ToString(), out height);
			}

			if ((!foundHeight) && (RawMetadata.TryGetValue(RawMetadataItemName.ImageHeight, out rawMdi)))
			{
				foundHeight = Int32.TryParse(rawMdi.Value.ToString(), out height);
			}

			if (foundHeight)
				this._height = height;

			return height;
		}

		private IGalleryObjectMetadataItem GetWidthMetadataItem()
		{
			IGalleryObjectMetadataItem mdi = null;
			int width = GetWidth();

			if (width > 0)
			{
				mdi = new GalleryObjectMetadataItem(int.MinValue, FormattedMetadataItemName.Width, Resources.Metadata_Width, String.Concat(width, " ", Resources.Metadata_Width_Units), true);
			}

			return mdi;
		}

		private IGalleryObjectMetadataItem GetHeightMetadataItem()
		{
			IGalleryObjectMetadataItem mdi = null;
			int height = GetHeight();

			if (height > 0)
			{
				mdi = new GalleryObjectMetadataItem(int.MinValue, FormattedMetadataItemName.Height, Resources.Metadata_Height, String.Concat(height, " ", Resources.Metadata_Height_Units), true);
			}

			return mdi;
		}

		private IGalleryObjectMetadataItem GetFlashModeMetadataItem()
		{
			IGalleryObjectMetadataItem mdi = null;
			MetadataItem rawMdi;
			if (RawMetadata.TryGetValue(RawMetadataItemName.ExifFlash, out rawMdi))
			{
				if (rawMdi.ExtractedValueType == ExtractedValueType.Int64)
				{
					FlashMode flashMode = (FlashMode)(Int64)rawMdi.Value;
					if (MetadataEnumHelper.IsValidFlashMode(flashMode))
					{
						mdi = new GalleryObjectMetadataItem(int.MinValue, FormattedMetadataItemName.FlashMode, Resources.Metadata_FlashMode, flashMode.ToString(), true);
					}
				}
			}
			return mdi;
		}

		private IGalleryObjectMetadataItem GetExposureProgramMetadataItem()
		{
			IGalleryObjectMetadataItem mdi = null;
			MetadataItem rawMdi;
			if (RawMetadata.TryGetValue(RawMetadataItemName.ExifExposureProg, out rawMdi))
			{
				if (rawMdi.ExtractedValueType == ExtractedValueType.Int64)
				{
					ExposureProgram expProgram = (ExposureProgram)(Int64)rawMdi.Value;
					if (MetadataEnumHelper.IsValidExposureProgram(expProgram))
					{
						mdi = new GalleryObjectMetadataItem(int.MinValue, FormattedMetadataItemName.ExposureProgram, Resources.Metadata_ExposureProgram, expProgram.ToString(), true);
					}
				}
			}
			return mdi;
		}

		private IGalleryObjectMetadataItem GetExposureTimeMetadataItem()
		{
			IGalleryObjectMetadataItem mdi = null;
			MetadataItem rawMdi;
			const Single numSeconds = 1; // If the exposure time is less than this # of seconds, format as fraction (1/350 sec.); otherwise convert to Single (2.35 sec.)
			if (RawMetadata.TryGetValue(RawMetadataItemName.ExifExposureTime, out rawMdi))
			{
				string exposureTime;
				if ((rawMdi.ExtractedValueType == ExtractedValueType.Fraction) && ((Fraction)rawMdi.Value).ToSingle() > numSeconds)
				{
					exposureTime = Math.Round(((Fraction)rawMdi.Value).ToSingle(), 2).ToString(CultureInfo.InvariantCulture);
				}
				else
				{
					exposureTime = rawMdi.Value.ToString();
				}

				string exposureTimeString = String.Concat(exposureTime, " ", Resources.Metadata_ExposureTime_Units);
				mdi = new GalleryObjectMetadataItem(int.MinValue, FormattedMetadataItemName.ExposureTime, Resources.Metadata_ExposureTime, exposureTimeString, true);
			}
			return mdi;
		}

		private IGalleryObjectMetadataItem GetColorRepresentationMetadataItem()
		{
			IGalleryObjectMetadataItem mdi = null;
			MetadataItem rawMdi;
			if (RawMetadata.TryGetValue(RawMetadataItemName.ExifColorSpace, out rawMdi))
			{
				string value = rawMdi.Value.ToString().Trim();
				string formattedValue = (value == "1" ? Resources.Metadata_ColorRepresentation_sRGB : Resources.Metadata_ColorRepresentation_Uncalibrated);
				mdi = new GalleryObjectMetadataItem(int.MinValue, FormattedMetadataItemName.ColorRepresentation, Resources.Metadata_ColorRepresentation, formattedValue, true);
			}
			return mdi;
		}

		private IGalleryObjectMetadataItem GetSubjectDistanceMetadataItem()
		{
			IGalleryObjectMetadataItem mdi = null;
			MetadataItem rawMdi;
			if (RawMetadata.TryGetValue(RawMetadataItemName.ExifSubjectDist, out rawMdi))
			{
				if (rawMdi.ExtractedValueType == ExtractedValueType.Fraction)
				{
					double distance = ((Fraction)rawMdi.Value).ToSingle();

					if (distance > 1)
					{
						distance = Math.Round(distance, 1);
					}

					string formattedValue = String.Concat(distance.ToString("0.### ", CultureInfo.InvariantCulture), Resources.Metadata_SubjectDistance_Units);
					mdi = new GalleryObjectMetadataItem(int.MinValue, FormattedMetadataItemName.SubjectDistance, Resources.Metadata_SubjectDistance, formattedValue, true);
				}
				else
				{
					string value = rawMdi.Value.ToString().Trim().TrimEnd(new char[] { '\0' });

					if (!String.IsNullOrEmpty(value))
					{
						mdi = new GalleryObjectMetadataItem(int.MinValue, FormattedMetadataItemName.SubjectDistance, Resources.Metadata_SubjectDistance, String.Format(CultureInfo.CurrentCulture, String.Concat("{0} ", Resources.Metadata_SubjectDistance_Units), value), true);
					}
				}
			}

			return mdi;
		}

		private IGalleryObjectMetadataItem GetStringMetadataItem(RawMetadataItemName sourceRawMetadataName, FormattedMetadataItemName destinationFormattedMetadataName, string metadataDescription)
		{
			return GetStringMetadataItem(sourceRawMetadataName, destinationFormattedMetadataName, metadataDescription, "{0}");
		}

		private IGalleryObjectMetadataItem GetStringMetadataItem(RawMetadataItemName sourceRawMetadataName, FormattedMetadataItemName destinationFormattedMetadataName, string metadataDescription, string formatString)
		{
			IGalleryObjectMetadataItem mdi = null;
			MetadataItem rawMdi;
			if (RawMetadata.TryGetValue(sourceRawMetadataName, out rawMdi))
			{
				string rawValue = rawMdi.Value.ToString().Trim().TrimEnd(new char[] { '\0' });

				if (!String.IsNullOrEmpty(rawValue))
				{
					mdi = new GalleryObjectMetadataItem(int.MinValue, destinationFormattedMetadataName, metadataDescription, String.Format(CultureInfo.CurrentCulture, formatString, rawValue), true);
				}
			}

			return mdi;
		}

		/// <summary>
		/// Add items to the specified collection from metadata accessed through the Windows Presentation Foundation (WPF)
		/// classes. This includes the following items: Title, Author, Data taken, Camera model, Camera manufacturer, Keywords,
		/// Rating, Comment, Copyright, Subject. If any of these items are null, they are not added.
		/// </summary>
		/// <param name="metadataItems">The collection of <see cref="IGalleryObjectMetadataItem" /> objects to add to.</param>
		/// <exception cref="System.Security.SecurityException">This function requires running under Full Trust, and will
		/// throw a security exception if it doesn't have it.</exception>
		private void AddWpfBitmapMetadata(IGalleryObjectMetadataItemCollection metadataItems)
		{
			WpfMetadataExtractor.AddWpfBitmapMetadata(this._mediaObjectFilePath, metadataItems);

			//System.Reflection.Assembly assembly = System.Reflection.Assembly.Load("GalleryServerPro.Business.Wpf");

			//// Get reference to static WpfMetadataExtractor.AddWpfBitmapMetadata() method.
			//Type[] parmTypes = new Type[2];
			//parmTypes[0] = typeof(string);
			//parmTypes[1] = typeof(IGalleryObjectMetadataItemCollection);
			//Type metadataExtractor = assembly.GetType("GalleryServerPro.Business.Wpf.WpfMetadataExtractor");
			//System.Reflection.MethodInfo addMetadataMethod = metadataExtractor.GetMethod("AddWpfBitmapMetadata", parmTypes);

			//// Prepare parameters to pass to BitmapDecoder.Create() method.
			//object[] parameters = new object[2];
			//parameters[0] = this._mediaObjectFilePath;
			//parameters[1] = metadataItems;

			//try
			//{
			//  addMetadataMethod.Invoke(null, parameters);
			//}
			//catch (System.Reflection.TargetInvocationException ex)
			//{
			//  LogError(ex, this._galleryId);
			//}
		}

		/// <summary>
		/// Adds items to <paramref name="metadataItems" /> containing information about the current media object using the FFmpeg utility.
		/// </summary>
		/// <param name="metadataItems">The collection of <see cref="IGalleryObjectMetadataItem" /> objects to add to.</param>
		private void AddVideoMetadata(IGalleryObjectMetadataItemCollection metadataItems)
		{
			if (String.IsNullOrEmpty(this._ffmpegOutput))
			{
				this._ffmpegOutput = FFmpeg.GetOutput(this._mediaObjectFilePath, this._galleryId);
			}

			if (!String.IsNullOrEmpty(this._ffmpegOutput))
			{
				ParseVideoMetadata(this._ffmpegOutput, metadataItems);
			}
		}

		/// <summary>
		/// Parses the <paramref name="ffmpegOutput" /> data and adds useful metadata items to the <paramref name="metadataItems" />
		/// collection.
		/// </summary>
		/// <param name="ffmpegOutput">The text output from the execution of the FFmpeg utility against a media file.</param>
		/// <param name="metadataItems">The collection of <see cref="IGalleryObjectMetadataItem" /> objects to add to.</param>
		private static void ParseVideoMetadata(string ffmpegOutput, IGalleryObjectMetadataItemCollection metadataItems)
		{
			//Use a regular expression to get the different properties from the video parsed out.
			Regex re = new Regex("[D|d]uration:.((\\d|:|\\.)*)");
			Match m = re.Match(ffmpegOutput);

			if (m.Success)
			{
				//string dur = m.Groups[1].Value;
				//string[] timepieces = dur.Split(new char[] { ':', '.' });
				//if (timepieces.Length == 4)
				//{
				//TimeSpan duration = new TimeSpan(0, Convert.ToInt16(timepieces[0]), Convert.ToInt16(timepieces[1]), Convert.ToInt16(timepieces[2]), Convert.ToInt16(timepieces[3]));
				GalleryObjectMetadataItem mdi = new GalleryObjectMetadataItem(int.MinValue, FormattedMetadataItemName.Duration, Resources.Metadata_Duration, m.Groups[1].Value.Trim(), true);
				AddMetadataItem(metadataItems, mdi);
				//}
			}

			//get bit rate
			re = new Regex("[B|b]itrate:.((\\d|:)*)");
			m = re.Match(ffmpegOutput);
			double kb;
			if (m.Success && Double.TryParse(m.Groups[1].Value, out kb))
			{
				//TODO: Parse bitrate units instead of assuming they are kb/s
				// Line we are parsing looks like this: Duration: 00:00:25.27, start: 0.000000, bitrate: 932 kb/s
				GalleryObjectMetadataItem mdi = new GalleryObjectMetadataItem(int.MinValue, FormattedMetadataItemName.BitRate, Resources.Metadata_BitRate, String.Concat(kb, " kb/s"), true);
				AddMetadataItem(metadataItems, mdi);
			}

			//get the audio format
			re = new Regex("[A|a]udio:.*");
			m = re.Match(ffmpegOutput);
			if (m.Success)
			{
				GalleryObjectMetadataItem mdi = new GalleryObjectMetadataItem(int.MinValue, FormattedMetadataItemName.AudioFormat, Resources.Metadata_AudioFormat, m.Value.Trim(), true);
				AddMetadataItem(metadataItems, mdi);
			}

			//get the video format
			re = new Regex("[V|v]ideo:.*");
			m = re.Match(ffmpegOutput);
			if (m.Success)
			{
				GalleryObjectMetadataItem mdi = new GalleryObjectMetadataItem(int.MinValue, FormattedMetadataItemName.VideoFormat, Resources.Metadata_VideoFormat, m.Value.Trim(), true);
				AddMetadataItem(metadataItems, mdi);
			}

			//get the video width and height
			// TODO: Get width and height from the metadata lines rather than looking for "200x300"
			re = new Regex("(\\d{2,4})x(\\d{2,4})");
			m = re.Match(ffmpegOutput);
			if (m.Success)
			{
				int width; int height;
				int.TryParse(m.Groups[1].Value, out width);
				int.TryParse(m.Groups[2].Value, out height);

				GalleryObjectMetadataItem mdiWidth = new GalleryObjectMetadataItem(int.MinValue, FormattedMetadataItemName.Width, Resources.Metadata_Width, String.Concat(width, " ", Resources.Metadata_Width_Units), true);
				GalleryObjectMetadataItem mdiHeight = new GalleryObjectMetadataItem(int.MinValue, FormattedMetadataItemName.Height, Resources.Metadata_Height, String.Concat(height, " ", Resources.Metadata_Height_Units), true);

				AddMetadataItem(metadataItems, mdiWidth);
				AddMetadataItem(metadataItems, mdiHeight);
			}
		}

		private static void AddMetadataItem(IGalleryObjectMetadataItemCollection metadataItems, GalleryObjectMetadataItem metadataItem)
		{
			if ((metadataItem != null) && (!metadataItems.Contains(metadataItem)))
			{
				metadataItems.Add(metadataItem);
			}
		}

		private static void LogError(Exception ex, int galleryId)
		{
			ErrorHandler.Error.Record(ex, galleryId, Factory.LoadGallerySettings(), AppSetting.Instance);
			HelperFunctions.PurgeCache();
		}

		/// <summary>
		/// Adds file-specific information, such as file name, file size, etc, to the <paramref name="metadataItems" />.
		/// </summary>
		/// <param name="metadataItems">The collection of <see cref="IGalleryObjectMetadataItem" /> objects to add to.</param>
		private void AddFileMetadata(IGalleryObjectMetadataItemCollection metadataItems)
		{
			FileInfo file = new FileInfo(this._mediaObjectFilePath);

			int fileSize = (int)(file.Length / 1024);
			fileSize = (fileSize < 1 ? 1 : fileSize); // Very small files should be 1, not 0.
			
			metadataItems.Add(new GalleryObjectMetadataItem(int.MinValue, FormattedMetadataItemName.FileName, Resources.Metadata_FileName, file.Name, true));
			metadataItems.Add(new GalleryObjectMetadataItem(int.MinValue, FormattedMetadataItemName.FileNameWithoutExtension, Resources.Metadata_FileName, Path.GetFileNameWithoutExtension(file.Name), true));
			metadataItems.Add(new GalleryObjectMetadataItem(int.MinValue, FormattedMetadataItemName.FileSizeKb, Resources.Metadata_FileSize, String.Concat(fileSize.ToString("N0", CultureInfo.CurrentCulture), " ", Resources.Metadata_KB), true));
			metadataItems.Add(new GalleryObjectMetadataItem(int.MinValue, FormattedMetadataItemName.DateFileCreated, Resources.Metadata_DateFileCreated, file.CreationTime.ToString(GlobalConstants.DateTimeFormatStringForMetadata, CultureInfo.InvariantCulture), true));
			metadataItems.Add(new GalleryObjectMetadataItem(int.MinValue, FormattedMetadataItemName.DateFileCreatedUtc, Resources.Metadata_DateFileCreatedUtc, file.CreationTimeUtc.ToString(GlobalConstants.DateTimeFormatStringForMetadata, CultureInfo.InvariantCulture), true));
			metadataItems.Add(new GalleryObjectMetadataItem(int.MinValue, FormattedMetadataItemName.DateFileLastModified, Resources.Metadata_DateFileLastModified, file.LastWriteTime.ToString(GlobalConstants.DateTimeFormatStringForMetadata, CultureInfo.InvariantCulture), true));
			metadataItems.Add(new GalleryObjectMetadataItem(int.MinValue, FormattedMetadataItemName.DateFileLastModifiedUtc, Resources.Metadata_DateFileLastModifiedUtc, file.LastWriteTimeUtc.ToString(GlobalConstants.DateTimeFormatStringForMetadata, CultureInfo.InvariantCulture), true));
		}

		#endregion
	}
}
