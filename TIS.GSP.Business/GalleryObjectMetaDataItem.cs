using System;
using GalleryServerPro.Business.Interfaces;

namespace GalleryServerPro.Business.Metadata
{
	/// <summary>
	/// Represents an item of metadata in media objects such as JPEG, TIFF, and PNG image files.
	/// </summary>
	[System.Diagnostics.DebuggerDisplay("{_description} = {_value}")]
	[Serializable]
	public class GalleryObjectMetadataItem : IGalleryObjectMetadataItem
	{
		#region Private Fields

		private int _mediaObjectMetadataId;
		private FormattedMetadataItemName _metadataItemName;
		private string _description;
		private string _value;
		private bool _extractFromFileOnSave;
		private bool _hasChanges;
		private bool _isVisible;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="GalleryObjectMetadataItem"/> class.
		/// </summary>
		/// <param name="mediaObjectMetadataId">The value that uniquely indentifies this metadata item.</param>
		/// <param name="metadataItemName">Name of the metadata item.</param>
		/// <param name="description">The description of the metadata item (e.g. "Exposure time", "Camera model").</param>
		/// <param name="value">The value of the metadata item (e.g. "F5.7", "1/500 sec.").</param>
		/// <param name="hasChanges">if set to <c>true</c> this object has changes that have not been persisted to the database.</param>
		public GalleryObjectMetadataItem(int mediaObjectMetadataId, FormattedMetadataItemName metadataItemName, string description, string value, bool hasChanges)
		{
			_mediaObjectMetadataId = mediaObjectMetadataId;
			_metadataItemName = metadataItemName;
			_description = description;
			_value = value;
			_extractFromFileOnSave = false;
			_hasChanges = hasChanges;
			_isVisible = false;
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets a value that uniquely indentifies this metadata item.
		/// </summary>
		/// <value>The value that uniquely indentifies this metadata item.</value>
		public int MediaObjectMetadataId
		{
			get { return _mediaObjectMetadataId; }
			set { _mediaObjectMetadataId = value; }
		}

		/// <summary>
		/// Gets or sets the description of the metadata item (e.g. "Exposure time", "Camera model"). Setting this to a new
		/// value causes <see cref="HasChanges" /> to be <c>true</c>.
		/// </summary>
		/// <value>The description of the metadata item.</value>
		public string Description
		{
			get { return _description; }
			set
			{
				if (_description != value)
				{
					_description = value;
					_hasChanges = true;
				}
			}
		}

		/// <summary>
		/// Gets or sets the value of the metadata item (e.g. "F5.7", "1/500 sec."). Setting this to a new
		/// value causes <see cref="HasChanges" /> to be <c>true</c>.
		/// </summary>
		/// <value>The value of the metadata item.</value>
		public string Value
		{
			get { return _value; }
			set
			{
				if (_value != value)
				{
					_value = value;
					_hasChanges = true;
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this metadata item is visible in the UI. Setting this to a new
		/// value does not affect <see cref="HasChanges" />.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this metadata item is visible in the UI; otherwise, <c>false</c>.
		/// </value>
		public bool IsVisible
		{
			get { return _isVisible; }
			set { _isVisible = value; }
		}

		/// <summary>
		/// Gets or sets the name of this metadata item. Setting this to a new
		/// value causes <see cref="HasChanges" /> to be <c>true</c>.
		/// </summary>
		/// <value>The name of the metadata item.</value>
		public FormattedMetadataItemName MetadataItemName
		{
			get { return _metadataItemName; }
			set
			{
				if (_metadataItemName != value)
				{
					_metadataItemName = value;
					_hasChanges = true;
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this metadata item should be extracted from the original image file the
		/// next time the media object is saved. This will cause the existing metadata item in the data store to be overwritten
		/// with the new value. The default value is false.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this metadata item should be extracted from the original image file the
		/// next time the media object is saved; otherwise, <c>false</c>.
		/// </value>
		public bool ExtractFromFileOnSave
		{
			get { return _extractFromFileOnSave; }
			set { _extractFromFileOnSave = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this object has changes that have not been persisted to the database.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance has changes; otherwise, <c>false</c>.
		/// </value>
		public bool HasChanges
		{
			get { return _hasChanges; }
			set { _hasChanges = value; }
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Perform a deep copy of this metadata item.
		/// </summary>
		/// <returns>
		/// Returns a deep copy of this metadata item.
		/// </returns>
		public IGalleryObjectMetadataItem Copy()
		{
			return new GalleryObjectMetadataItem(MediaObjectMetadataId, MetadataItemName, Description, Value, HasChanges);
		}

		#endregion

		#region IComparable

		/// <summary>
		/// Compares the current object with another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>.
		/// </returns>
		public int CompareTo(IGalleryObjectMetadataItem other)
		{
			if (other == null)
				return 1;
			else
			{
				return String.Compare(Description, other.Description, StringComparison.CurrentCulture);
			}
		}

		#endregion

		/// <summary>
		/// Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="GalleryObjectMetadataItem"/>.
		/// </returns>
		public override int GetHashCode()
		{
			return ((IGalleryObjectMetadataItem)this).MetadataItemName.GetHashCode();
		}

	}
}
