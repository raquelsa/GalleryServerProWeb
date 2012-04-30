using System;

namespace GalleryServerPro.Business.Interfaces
{
	/// <summary>
	/// Represents an item of metadata in media objects such as JPEG, TIFF, and PNG image files.
	/// </summary>
	public interface IGalleryObjectMetadataItem : IComparable<IGalleryObjectMetadataItem>
	{
		/// <summary>
		/// Gets or sets a value that uniquely indentifies this metadata item.
		/// </summary>
		/// <value>The value that uniquely indentifies this metadata item.</value>
		int MediaObjectMetadataId { get; set; }

		/// <summary>
		/// Gets or sets the description of the metadata item (e.g. "Exposure time", "Camera model"). Setting this to a new
		/// value causes <see cref="HasChanges" /> to be <c>true</c>.
		/// </summary>
		/// <value>The description of the metadata item.</value>
		string Description { get; set; }

		/// <summary>
		/// Gets or sets the value of the metadata item (e.g. "F5.7", "1/500 sec."). Setting this to a new
		/// value causes <see cref="HasChanges" /> to be <c>true</c>.
		/// </summary>
		/// <value>The value of the metadata item.</value>
		string Value { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this metadata item should be extracted from the original image file the
		/// next time the media object is saved. This will cause the existing metadata item in the data store to be overwritten
		/// with the new value. The default value is false.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this metadata item should be extracted from the original image file the
		/// next time the media object is saved; otherwise, <c>false</c>.
		/// </value>
		bool ExtractFromFileOnSave { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this object has changes that have not been persisted to the database.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance has changes; otherwise, <c>false</c>.
		/// </value>
		bool HasChanges { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this metadata item is visible in the UI.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this metadata item is visible in the UI; otherwise, <c>false</c>.
		/// </value>
		bool IsVisible { get; set; }

		/// <summary>
		/// Gets or sets the name of this metadata item. Setting this to a new
		/// value causes <see cref="HasChanges" /> to be <c>true</c>.
		/// </summary>
		/// <value>The name of the metadata item.</value>
		Metadata.FormattedMetadataItemName MetadataItemName { get; set; }

		/// <summary>
		/// Perform a deep copy of this metadata item.
		/// </summary>
		/// <returns>Returns a deep copy of this metadata item.</returns>
		IGalleryObjectMetadataItem Copy();
	}
}
