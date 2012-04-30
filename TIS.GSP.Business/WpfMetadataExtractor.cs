using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Media.Imaging;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.Business.Metadata;
using System.IO;
using GalleryServerPro.Business.Properties;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// Contains functionality for extracting image metadata using the .NET 3.0 Windows Presentation Foundation (WPF) classes.
	/// </summary>
	public static class WpfMetadataExtractor
	{
		private const string DateTimeFormatStringForMetadata = "ddd, dd MMM yyyy h:mm:ss tt";
		private const string IptcQueryFormatString = "/app13/irb/8bimiptc/iptc/{{str={0}}}";

		/// <summary>
		/// Add items to the specified collection from metadata accessed through the Windows Presentation Foundation (WPF)
		/// classes. This includes the following items: Title, Author, Data taken, Camera model, Camera manufacturer, Keywords,
		/// Rating, Comment, Copyright, Subject. If any of these items are null, they are not added.
		/// </summary>
		/// <param name="imageFilePath">The image file path.</param>
		/// <param name="metadataItems">The collection of <see cref="IGalleryObjectMetadataItem" /> objects to add to.</param>
		/// <exception cref="System.Security.SecurityException">This function requires running under Full Trust, and will
		/// throw a security exception if it doesn't have it.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="metadataItems" /> is null.</exception>
		public static void AddWpfBitmapMetadata(string imageFilePath, IGalleryObjectMetadataItemCollection metadataItems)
		{
			if (metadataItems == null)
				throw new ArgumentNullException("metadataItems");

			BitmapMetadata bmpMetadata = GetBitmapMetadata(imageFilePath);

			if (bmpMetadata != null)
			{
				AddGpsMetadata(bmpMetadata, metadataItems);

				AddIptcMetadata(bmpMetadata, metadataItems);

				try
				{
					if ((bmpMetadata.Title != null) && (!String.IsNullOrEmpty(bmpMetadata.Title.Trim())))
						metadataItems.AddNew(int.MinValue, FormattedMetadataItemName.Title, Resources.Metadata_Title, bmpMetadata.Title.Trim(), true);

					if (bmpMetadata.Author != null)
						metadataItems.AddNew(int.MinValue, FormattedMetadataItemName.Author, Resources.Metadata_Author, ConvertStringCollectionToDelimitedString(bmpMetadata.Author), true);

					if (bmpMetadata.DateTaken != null)
					{
						DateTime dateTaken = TryParseDate(bmpMetadata.DateTaken);
						if (dateTaken.Year > DateTime.MinValue.Year)
						{
							metadataItems.AddNew(int.MinValue, FormattedMetadataItemName.DatePictureTaken, Resources.Metadata_DatePictureTaken, dateTaken.ToString(DateTimeFormatStringForMetadata, CultureInfo.InvariantCulture), true);
						}
					}

					if ((bmpMetadata.CameraModel != null) && (!String.IsNullOrEmpty(bmpMetadata.CameraModel.Trim())))
						metadataItems.AddNew(int.MinValue, FormattedMetadataItemName.CameraModel, Resources.Metadata_CameraModel, bmpMetadata.CameraModel.Trim(), true);

					if ((bmpMetadata.CameraManufacturer != null) && (!String.IsNullOrEmpty(bmpMetadata.CameraManufacturer.Trim())))
						metadataItems.AddNew(int.MinValue, FormattedMetadataItemName.EquipmentManufacturer, Resources.Metadata_EquipmentManufacturer, bmpMetadata.CameraManufacturer.Trim(), true);

					if (bmpMetadata.Keywords != null)
						metadataItems.AddNew(int.MinValue, FormattedMetadataItemName.Keywords, Resources.Metadata_Keywords, ConvertStringCollectionToDelimitedString(bmpMetadata.Keywords), true);

					if (bmpMetadata.Rating > 0)
						metadataItems.AddNew(int.MinValue, FormattedMetadataItemName.Rating, Resources.Metadata_Rating, bmpMetadata.Rating.ToString(CultureInfo.InvariantCulture), true);

					if ((bmpMetadata.Comment != null) && (!String.IsNullOrEmpty(bmpMetadata.Comment.Trim())))
						metadataItems.AddNew(int.MinValue, FormattedMetadataItemName.Comment, Resources.Metadata_Comment, bmpMetadata.Comment.Trim(), true);

					if ((bmpMetadata.Copyright != null) && (!String.IsNullOrEmpty(bmpMetadata.Copyright.Trim())))
						metadataItems.AddNew(int.MinValue, FormattedMetadataItemName.Copyright, Resources.Metadata_Copyright, bmpMetadata.Copyright.Trim(), true);

					if ((bmpMetadata.Subject != null) && (!String.IsNullOrEmpty(bmpMetadata.Subject.Trim())))
						metadataItems.AddNew(int.MinValue, FormattedMetadataItemName.Subject, Resources.Metadata_Subject, bmpMetadata.Subject.Trim(), true);
				}
				catch (NotSupportedException) { } // Some image types, such as png, throw a NotSupportedException. Let's swallow them and move on.
				catch (ArgumentException) { }
				catch (InvalidOperationException) { }
			}
		}

		/// <summary>
		/// Adds GPS data from the <paramref name="bmpMetadata" /> to the <paramref name="metadataItems" /> collection.
		/// </summary>
		/// <param name="bmpMetadata">An object containing the metadata.</param>
		/// <param name="metadataItems">The metadata items.</param>
		private static void AddGpsMetadata(BitmapMetadata bmpMetadata, IGalleryObjectMetadataItemCollection metadataItems)
		{
			GpsLocation gps = GpsLocation.Parse(bmpMetadata);

			if (!String.IsNullOrEmpty(gps.Version))
			{
				metadataItems.AddNew(int.MinValue, FormattedMetadataItemName.GpsVersion, GetResource(FormattedMetadataItemName.GpsVersion), gps.Version, true);
			}

			if ((gps.Latitude != null) && (gps.Longitude != null))
			{
				metadataItems.AddNew(int.MinValue, FormattedMetadataItemName.GpsLocation, GetResource(FormattedMetadataItemName.GpsLocation), gps.ToLatitudeLongitudeDecimalString(), true);
				metadataItems.AddNew(int.MinValue, FormattedMetadataItemName.GpsLatitude, GetResource(FormattedMetadataItemName.GpsLatitude), gps.Latitude.ToDouble().ToString("F6", CultureInfo.InvariantCulture), true);
				metadataItems.AddNew(int.MinValue, FormattedMetadataItemName.GpsLongitude, GetResource(FormattedMetadataItemName.GpsLongitude), gps.Longitude.ToDouble().ToString("F6", CultureInfo.InvariantCulture), true);
			}

			if (gps.Altitude.HasValue)
			{
				string altitude = String.Concat(gps.Altitude.Value.ToString("N0", CultureInfo.CurrentCulture), " ", Resources.Metadata_meters);
				metadataItems.AddNew(int.MinValue, FormattedMetadataItemName.GpsAltitude, GetResource(FormattedMetadataItemName.GpsAltitude), altitude, true);
			}

			if ((gps.DestLatitude != null) && (gps.DestLongitude != null))
			{
				metadataItems.AddNew(int.MinValue, FormattedMetadataItemName.GpsDestLocation, GetResource(FormattedMetadataItemName.GpsDestLocation), gps.ToDestLatitudeLongitudeDecimalString(), true);
				metadataItems.AddNew(int.MinValue, FormattedMetadataItemName.GpsDestLatitude, GetResource(FormattedMetadataItemName.GpsDestLatitude), gps.DestLatitude.ToDouble().ToString("F6", CultureInfo.InvariantCulture), true);
				metadataItems.AddNew(int.MinValue, FormattedMetadataItemName.GpsDestLongitude, GetResource(FormattedMetadataItemName.GpsDestLongitude), gps.DestLongitude.ToDouble().ToString("F6", CultureInfo.InvariantCulture), true);
			}
		}

		private static void AddIptcMetadata(BitmapMetadata bmpMetadata, IGalleryObjectMetadataItemCollection metadataItems)
		{
			foreach (KeyValuePair<FormattedMetadataItemName, string> iptcQueryParm in GetIptcQueryParameters())
			{
				string iptcValue;
				try
				{
					iptcValue = bmpMetadata.GetQuery(String.Format(CultureInfo.InvariantCulture, IptcQueryFormatString, iptcQueryParm.Value)) as string;
				}
				catch (InvalidOperationException) 
				{
					// Some images throw this exception. When this happens, just exit.
					break;
				}

				if (!String.IsNullOrEmpty(iptcValue))
				{
					switch (iptcQueryParm.Key)
					{
						case FormattedMetadataItemName.IptcDateCreated:
							{
								DateTime dateTaken = TryParseDate(iptcValue);

								if (dateTaken.Year > DateTime.MinValue.Year)
								{
									metadataItems.AddNew(int.MinValue, iptcQueryParm.Key, GetResource(iptcQueryParm.Key), dateTaken.ToString(DateTimeFormatStringForMetadata, CultureInfo.InvariantCulture), true);
								}
								break;
							}
						default:
							{
								metadataItems.AddNew(int.MinValue, iptcQueryParm.Key, GetResource(iptcQueryParm.Key), iptcValue, true);
								break;
							}
					}
				}
			}
		}

		/// <summary>
		/// Try to convert <paramref name="dteRaw" /> to a valid <see cref="DateTime" /> object. If it cannot be converted, return
		/// <see cref="DateTime.MinValue" />.
		/// </summary>
		/// <param name="dteRaw">The string containing the date/time to convert.</param>
		/// <returns>Returns a <see cref="DateTime" /> instance.</returns>
		/// <remarks>The IPTC specs do not define an exact format for the ITPC Date Created field, so it is unclear how to reliably parse
		/// it. However, an analysis of sample photos, including those provided by IPTC (http://www.iptc.org), show that the format
		/// yyyyMMdd is consistently used, so we'll try that if the more generic parsing doesnt work.</remarks>
		private static DateTime TryParseDate(string dteRaw)
		{
			DateTime result;
			if (DateTime.TryParse(dteRaw, out result))
			{
				return result;
			}
			else if (DateTime.TryParseExact(dteRaw, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
			{
				return result;
			}

			return DateTime.MinValue;
		}

		private static string GetResource(FormattedMetadataItemName formattedMetadataItemName)
		{
			const string resourcePrefix = "Metadata_";

			return Resources.ResourceManager.GetString(String.Concat(resourcePrefix, formattedMetadataItemName.ToString())) ?? formattedMetadataItemName.ToString();
		}

		private static Dictionary<FormattedMetadataItemName, string> GetIptcQueryParameters()
		{
			Dictionary<FormattedMetadataItemName, string> iptcQueryParms = new Dictionary<FormattedMetadataItemName, string>();

			iptcQueryParms.Add(FormattedMetadataItemName.IptcByline, "By-Line");
			iptcQueryParms.Add(FormattedMetadataItemName.IptcBylineTitle, "By-line Title");
			iptcQueryParms.Add(FormattedMetadataItemName.IptcCaption, "Caption");
			iptcQueryParms.Add(FormattedMetadataItemName.IptcCity, "City");
			iptcQueryParms.Add(FormattedMetadataItemName.IptcCopyrightNotice, "Copyright Notice");
			iptcQueryParms.Add(FormattedMetadataItemName.IptcCountryPrimaryLocationName, "Country/Primary Location Name");
			iptcQueryParms.Add(FormattedMetadataItemName.IptcCredit, "Credit");
			iptcQueryParms.Add(FormattedMetadataItemName.IptcDateCreated, "Date Created");
			iptcQueryParms.Add(FormattedMetadataItemName.IptcHeadline, "Headline");
			iptcQueryParms.Add(FormattedMetadataItemName.IptcKeywords, "Keywords");
			iptcQueryParms.Add(FormattedMetadataItemName.IptcObjectName, "Object Name");
			iptcQueryParms.Add(FormattedMetadataItemName.IptcOriginalTransmissionReference, "Original Transmission Reference");
			iptcQueryParms.Add(FormattedMetadataItemName.IptcProvinceState, "Province/State");
			iptcQueryParms.Add(FormattedMetadataItemName.IptcRecordVersion, "Record Version");
			iptcQueryParms.Add(FormattedMetadataItemName.IptcSource, "Source");
			iptcQueryParms.Add(FormattedMetadataItemName.IptcSpecialInstructions, "Special Instructions");
			iptcQueryParms.Add(FormattedMetadataItemName.IptcSublocation, "Sub-location");
			iptcQueryParms.Add(FormattedMetadataItemName.IptcWriterEditor, "Writer/Editor");

			return iptcQueryParms;
		}

		/// <summary>
		/// Get a reference to the BitmapMetadata object for this image file that contains the metadata such as title, keywords, etc.
		/// Returns null if the metadata is not accessible.
		/// </summary>
		/// <returns> Returns a reference to the BitmapMetadata object for this image file that contains the metadata such as title, keywords, etc.</returns>
		/// <remarks>A BitmapDecoder object is created from the absolute filepath passed into the constructor. Through trial and
		/// error, the relevant metadata appears to be stored in the first frame in the BitmapDecoder property of the first frame
		/// of the root-level BitmapDecoder object. One might expect the Metadata property of the root-level BitmapDecoder object to
		/// contain the metadata, but it seems to always be null.</remarks>
		private static BitmapMetadata GetBitmapMetadata(string imageFilePath)
		{
			// Do not use the BitmapCacheOption.Default or None option, as it will hold a lock on the file until garbage collection. I discovered
			// this problem and it has been submitted to MS as a bug. See thread in the managed newsgroup:
			// http://www.microsoft.com/communities/newsgroups/en-us/default.aspx?dg=microsoft.public.dotnet.framework&tid=b694ada2-10c4-4999-81f8-97295eb024a9&cat=en_US_a4ab6128-1a11-4169-8005-1d640f3bd725&lang=en&cr=US&sloc=en-us&m=1&p=1
			// Also do not use BitmapCacheOption.OnLoad as suggested in the thread, as it causes the memory to not be released until 
			// eventually IIS crashes when you do things like synchronize 100 images.
			// BitmapCacheOption.OnDemand seems to be the only option that doesn't lock the file or crash IIS.
			// Update 2007-07-29: OnDemand seems to also lock the file. There is no good solution! Acckkk
			// Update 2007-08-04: After installing VS 2008 beta 2, which also installs .NET 2.0 SP1, I discovered that OnLoad no longer crashes IIS.
			// Update 2008-05-19: The Create method doesn't release the file lock when an exception occurs, such as when the file is a WMF. See:
			// http://www.microsoft.com/communities/newsgroups/en-us/default.aspx?dg=microsoft.public.dotnet.framework&tid=fe3fb82f-0191-40a3-b789-0602cc4445d3&cat=&lang=&cr=&sloc=&p=1
			// Bug submission: https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=344914
			// The workaround is to use a different overload of Create that takes a FileStream.

			if (String.IsNullOrEmpty(imageFilePath))
				return null;

			BitmapDecoder fileBitmapDecoder = GetBitmapDecoder(imageFilePath);

			if ((fileBitmapDecoder == null) || (fileBitmapDecoder.Frames.Count == 0))
				return null;

			BitmapFrame fileFirstFrame = fileBitmapDecoder.Frames[0];

			if (fileFirstFrame == null)
				return null;

			BitmapDecoder firstFrameBitmapDecoder = fileFirstFrame.Decoder;

			if ((firstFrameBitmapDecoder == null) || (firstFrameBitmapDecoder.Frames.Count == 0))
				return null;

			BitmapFrame firstFrameInDecoderInFirstFrameOfFile = firstFrameBitmapDecoder.Frames[0];

			// The Metadata property is of type ImageMetadata, so we must cast it to BitmapMetadata.
			return firstFrameInDecoderInFirstFrameOfFile.Metadata as BitmapMetadata;
		}

		private static BitmapDecoder GetBitmapDecoder(string imageFilePath)
		{
			BitmapDecoder fileBitmapDecoder = null;
			using (Stream stream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				try
				{
					fileBitmapDecoder = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
					// DO NOT USE: fileBitmapDecoder = BitmapDecoder.Create(new Uri(imageFilePath, UriKind.Absolute), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
				}
				catch (NotSupportedException) { } // Thrown by some file types such as wmf
				catch (InvalidOperationException) { } // Reported by some users
				catch (ArgumentException) { } // Reported by some users
				catch (FileFormatException) { } // Reported by some users
				catch (IOException) { } // Reported by some users
				catch (OverflowException) { } // Reported by some users
				catch (OutOfMemoryException)
				{
					// The garbage collector will automatically run to try to clean up memory, so let's wait for it to finish and 
					// try again. If it still doesn't work because the image is just too large and the system doesn't have enough
					// memory, just give up.
					GC.WaitForPendingFinalizers();
					try
					{
						fileBitmapDecoder = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
					}
					catch (NotSupportedException) { }
					catch (InvalidOperationException) { }
					catch (ArgumentException) { }
					catch (OutOfMemoryException) { }
				}
			}

			return fileBitmapDecoder;
		}

		private static string ConvertStringCollectionToDelimitedString(System.Collections.ObjectModel.ReadOnlyCollection<string> stringCollection)
		{
			const string delimiter = "; ";
			string[] strings = new string[stringCollection.Count];
			stringCollection.CopyTo(strings, 0);
			return String.Join(delimiter, strings);
		}

	}
}
