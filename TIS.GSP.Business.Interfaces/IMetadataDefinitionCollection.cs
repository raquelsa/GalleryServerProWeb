using GalleryServerPro.Business.Metadata;

namespace GalleryServerPro.Business.Interfaces
{
	/// <summary>
	/// A collection of <see cref="IMetadataDefinition" /> objects.
	/// </summary>
	public interface IMetadataDefinitionCollection : System.Collections.Generic.ICollection<IMetadataDefinition>
	{
		/// <summary>
		/// Adds the specified metadata definition.
		/// </summary>
		/// <param name="item">The metadata definition to add.</param>
		new void Add(IMetadataDefinition item);

		/// <summary>
		/// Find the metadata definition in the collection that matches the specified <paramref name="metadataItemName" />. If no matching object is found,
		/// null is returned.
		/// </summary>
		/// <param name="metadataItemName">The metadata item to find.</param>
		/// <returns>Returns an <see cref="IMetadataDefinition" />object from the collection that matches the specified <paramref name="metadataItemName" />,
		/// or null if no matching object is found.</returns>
		IMetadataDefinition Find(FormattedMetadataItemName metadataItemName);

		/// <summary>
		/// Verify that there exists an item in this collection for every enumeration value of <see cref="FormattedMetadataItemName" />.
		/// This should be called after the collection is filled. Doing this validation guarantees that later calls to <see cref="Find" />
		/// will never fail.
		/// </summary>
		/// <param name="galleryId">The ID of the gallery associated with this collection.</param>
		void Validate(int galleryId);

		/// <summary>
		/// Generates as string representation of the items in the collection. Use this to convert the collection to a form that can be stored in the
		/// gallery settings table.
		/// Example: "0:T,1:T,2:T,3:T,4:T, ... 31:T,32:T,33:T,34:T,35:T,36:T,37:T,38:T,39:T"
		/// </summary>
		/// <returns>Returns a string representation of the items in the collection.</returns>
		/// <remarks>Each comma-delimited string represents an <see cref="IMetadataDefinition" /> in the collection. Each of these, in turn,
		/// is colon-delimited to separate the properties of the instance (e.g. "2:T"). The order of the items in the list maps to
		/// the <see cref="IMetadataDefinition.Sequence" />. The first value in the item string represents the integer value of the
		/// <see cref="FormattedMetadataItemName" /> enumeration; the second value is 'T' or 'F' and maps to 
		/// <see cref="IMetadataDefinition.IsVisible" />.</remarks>
		string Serialize();
	}
}
