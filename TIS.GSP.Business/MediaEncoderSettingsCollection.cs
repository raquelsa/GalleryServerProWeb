using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.ErrorHandler.CustomExceptions;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// A collection of <see cref="IMediaEncoderSettings" /> objects.
	/// </summary>
	public class MediaEncoderSettingsCollection : Collection<IMediaEncoderSettings>, IMediaEncoderSettingsCollection
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MediaEncoderSettingsCollection"/> class.
		/// </summary>
		public MediaEncoderSettingsCollection()
			: base(new List<IMediaEncoderSettings>())
		{
		}

		public MediaEncoderSettingsCollection(IEnumerable<IMediaEncoderSettings> encoderSettings)
		{
			AddRange(encoderSettings);
		}

		/// <summary>
		/// Adds the media encoder settings to the current collection.
		/// </summary>
		/// <param name="mediaEncoderSettings">The media encoder settings to add to the current collection.</param>
		public void AddRange(IEnumerable<IMediaEncoderSettings> mediaEncoderSettings)
		{
			if (mediaEncoderSettings == null)
				throw new ArgumentNullException("mediaEncoderSettings");

			foreach (IMediaEncoderSettings mediaEncoderSetting in mediaEncoderSettings)
			{
				Add(mediaEncoderSetting);
			}
		}

		/// <summary>
		/// Adds the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		public new void Add(IMediaEncoderSettings item)
		{
			if (item == null)
				throw new ArgumentNullException("item", "Cannot add null to an existing MediaEncoderSettingsCollection. Items.Count = " + Items.Count);

			base.Add(item);
		}

		/// <summary>
		/// Verifies the items in the collection contain valid data.
		/// </summary>
		/// <exception cref="UnsupportedMediaObjectTypeException">Thrown when one of the items references
		/// a file type not recognized by the application.</exception>
		public void Validate()
		{
			foreach (IMediaEncoderSettings setting in base.Items)
			{
				setting.Validate();
			}
		}

		/// <summary>
		/// Generates as string representation of the items in the collection. Use this to convert the collection
		/// to a form that can be stored in the gallery settings table.
		/// Example: Ex: ".avi||.mp4||-i {SourceFilePath} {DestinationFilePath}~~.avi||.flv||-i {SourceFilePath} {DestinationFilePath}"
		/// </summary>
		/// <returns>
		/// Returns a string representation of the items in the collection.
		/// </returns>
		/// <remarks>Each triple-pipe-delimited string represents an <see cref="IMediaEncoderSettings"/> in the collection.
		/// Each of these, in turn, is double-pipe-delimited to separate the properties of the instance
		/// (e.g. ".avi||.mp4||-i {SourceFilePath} {DestinationFilePath}"). The order of the items in the
		/// return value maps to the <see cref="IMediaEncoderSettings.Sequence"/>.</remarks>
		public string Serialize()
		{
			StringBuilder sb = new StringBuilder();
			List<IMediaEncoderSettings> encoderSettings = base.Items as List<IMediaEncoderSettings>;

			if (encoderSettings != null)
			{
				encoderSettings.Sort();
			}

			// Now that it is sorted, we can iterate in increasing sequence. Validate as we go along to ensure each 
			// sequence is equal to or higher than the one before.
			int lastSeq = 0;
			foreach (IMediaEncoderSettings encoderSetting in base.Items)
			{
				if (encoderSetting.Sequence < lastSeq)
				{
					throw new BusinessException("Cannot serialize MediaEncoderSettingsCollection because the underlying collection is not in ascending sequence.");
				}

				sb.AppendFormat(CultureInfo.InvariantCulture, "{0}||{1}||{2}~~", encoderSetting.SourceFileExtension, encoderSetting.DestinationFileExtension, encoderSetting.EncoderArguments);

				lastSeq = encoderSetting.Sequence;
			}

			sb.Remove(sb.Length - 2, 2); // Remove the final ~~

			return sb.ToString();
		}
	}
}
