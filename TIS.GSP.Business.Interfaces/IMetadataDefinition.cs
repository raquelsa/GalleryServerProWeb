using System;
using GalleryServerPro.Business.Metadata;

namespace GalleryServerPro.Business.Interfaces
{
	/// <summary>
	/// Represents the definition of a type of metadata that is associated with media objects. Note that this is not an actual
	/// piece of metadata, but rather defines the behavior of metadata stored in <see cref="IGalleryObjectMetadataItem" />.
	/// </summary>
	public interface IMetadataDefinition : IComparable<IMetadataDefinition>
	{
		/// <summary>
		/// Gets or sets the name of the metadata item.
		/// </summary>
		/// <value>The metadata item.</value>
		FormattedMetadataItemName MetadataItem { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether metadata items of this type are visible in the gallery.
		/// </summary>
		/// <value><c>true</c> if metadata items of this type are visible in the gallery; otherwise, <c>false</c>.</value>
		bool IsVisible { get; set; }

		/// <summary>
		/// Gets or sets the order this metadata item is to be displayed in relation to other metadata items.
		/// </summary>
		/// <value>The order this metadata item is to be displayed in relation to other metadata items.</value>
		int Sequence { get; set; }

		/// <summary>
		/// Gets or sets the gallery ID this metadata definition is associated with.
		/// </summary>
		/// <value>The gallery ID this metadata definition is associated with.</value>
		int GalleryId { get; set; }
	}
}