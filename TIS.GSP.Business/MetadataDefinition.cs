using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.Business.Metadata;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// Represents the definition of a type of metadata that is associated with media objects. Note that this is not an actual
	/// piece of metadata, but rather defines the behavior of metadata stored in <see cref="IGalleryObjectMetadataItem" />.
	/// </summary>
	[System.Diagnostics.DebuggerDisplay("\"{_metadataItem}\", Visible={_isVisible}, Seq={_sequence}")]
	class MetadataDefinition : IMetadataDefinition
	{
		private FormattedMetadataItemName _metadataItem;
		private bool _isVisible;
		private int _sequence;
		private int _galleryId;

		/// <summary>
		/// Initializes a new instance of the <see cref="MetadataDefinition"/> class.
		/// </summary>
		/// <param name="metadataItem">The metadata item.</param>
		/// <param name="isVisible">If set to <c>true</c> metadata items of this type are to be visible to the user.</param>
		/// <param name="sequence">The sequence.</param>
		/// <param name="galleryId">The gallery ID.</param>
		public MetadataDefinition(FormattedMetadataItemName metadataItem, bool isVisible, int sequence, int galleryId)
		{
			MetadataItem = metadataItem;
			IsVisible = isVisible;
			Sequence = sequence;
			GalleryId = galleryId;
		}

		/// <summary>
		/// Gets or sets the name of the metadata item.
		/// </summary>
		/// <value>The metadata item.</value>
		public FormattedMetadataItemName MetadataItem
		{
			get { return _metadataItem; }
			set { _metadataItem = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether metadata items of this type are visible in the gallery.
		/// </summary>
		/// <value><c>true</c> if metadata items of this type are visible in the gallery; otherwise, <c>false</c>.</value>
		public bool IsVisible
		{
			get { return _isVisible; }
			set { _isVisible = value; }
		}

		/// <summary>
		/// Gets or sets the order this metadata item is to be displayed in relation to other metadata items.
		/// </summary>
		/// <value>The order this metadata item is to be displayed in relation to other metadata items.</value>
		public int Sequence
		{
			get { return _sequence; }
			set { _sequence = value; }
		}

		/// <summary>
		/// Gets or sets the gallery ID this metadata definition is associated with.
		/// </summary>
		/// <value>The gallery ID this metadata definition is associated with.</value>
		public int GalleryId
		{
			get { return _galleryId; }
			set { _galleryId = value; }
		}

		#region IComparable

		/// <summary>
		/// Compares the current object with another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>.
		/// </returns>
		public int CompareTo(IMetadataDefinition other)
		{
			if (other == null)
				return 1;
			else
			{
				return Sequence.CompareTo(other.Sequence);
			}
		}

		#endregion

		/// <summary>
		/// Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="MetadataDefinition"/>.
		/// </returns>
		public override int GetHashCode()
		{
			return MetadataItem.GetHashCode();
		}

	}
}
