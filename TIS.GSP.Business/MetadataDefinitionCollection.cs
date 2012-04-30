using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.Business.Metadata;
using GalleryServerPro.ErrorHandler.CustomExceptions;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// A collection of <see cref="IMetadataDefinition" /> objects.
	/// </summary>
	public class MetadataDefinitionCollection : KeyedCollection<FormattedMetadataItemName, IMetadataDefinition>, IMetadataDefinitionCollection
	{
		/// <summary>
		/// Adds the specified metadata definition.
		/// </summary>
		/// <param name="item">The metadata definition to add.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="item" /> is null.</exception>
		public new void Add(IMetadataDefinition item)
		{
			if (item == null)
				throw new ArgumentNullException("item", "Cannot add null to an existing MetadataDefinitionCollection. Items.Count = " + Items.Count);

			base.Add(item);
		}

		/// <summary>
		/// Find the metadata definition in the collection that matches the specified <paramref name="metadataItemName" />. If no matching object is found,
		/// null is returned.
		/// </summary>
		/// <param name="metadataItemName">The metadata item to find.</param>
		/// <returns>Returns an <see cref="IMetadataDefinition" />object from the collection that matches the specified <paramref name="metadataItemName" />,
		/// or null if no matching object is found.</returns>
		public IMetadataDefinition Find(FormattedMetadataItemName metadataItemName)
		{
			return base[metadataItemName];
		}

		/// <summary>
		/// Verify that an item exists in this collection for every enumeration value of <see cref="FormattedMetadataItemName" />.
		/// If an item is missing, one is added with default values of <see cref="IMetadataDefinition.IsVisible" />=<c>true</c>,
		/// <see cref="IMetadataDefinition.Sequence" />=<see cref="Int32.MaxValue" />, and 
		/// <see cref="IMetadataDefinition.GalleryId" />=<paramref name="galleryId" />. This should be called after the collection is populated
		/// from the gallery settings. Doing this validation guarantees that later calls to <see cref="IMetadataDefinitionCollection.Find" />
		/// will never fail and helps to automatically add items for newly added <see cref="FormattedMetadataItemName" /> values. 
		/// </summary>
		/// <param name="galleryId">The ID of the gallery associated with this collection.</param>
		public void Validate(int galleryId)
		{
			foreach (FormattedMetadataItemName item in Enum.GetValues(typeof(FormattedMetadataItemName)))
			{
				if (!base.Contains(item))
				{
					base.Add(new MetadataDefinition(item, true, int.MaxValue, galleryId));
				}
			}
		}

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
		public string Serialize()
		{
			// Convert items to a string like this: "0:T,1:T,2:T,3:T,4:T, ... 31:T,32:T,33:T,34:T,35:T,36:T,37:T,38:T,39:T"
			StringBuilder sb = new StringBuilder();
			List<IMetadataDefinition> metadataDefs = base.Items as List<IMetadataDefinition>;

			if (metadataDefs != null)
			{
				metadataDefs.Sort();
			}

			// Now that it is sorted, we can iterate in increasing sequence. Validate as we go along to ensure each sequence is equal to
			// or higher than the one before.
			int lastSeq = 0;
			foreach (IMetadataDefinition metadataDef in base.Items)
			{
				if (metadataDef.Sequence < lastSeq)
				{
					throw new BusinessException("Cannot serialize MetadataDefinitionCollection because the underlying collection is not in ascending sequence.");
				}

				sb.AppendFormat(CultureInfo.InvariantCulture, "{0}:{1},", (int)metadataDef.MetadataItem, (metadataDef.IsVisible ? "T" : "F"));

				lastSeq = metadataDef.Sequence;
			}

			sb.Remove(sb.Length - 1, 1); // Remove the final comma

			return sb.ToString();
		}

		/// <summary>
		/// When implemented in a derived class, extracts the key from the specified element.
		/// </summary>
		/// <returns>
		/// The key for the specified element.
		/// </returns>
		/// <param name="item">The element from which to extract the key.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="" /> is null.</exception>
		protected override FormattedMetadataItemName GetKeyForItem(IMetadataDefinition item)
		{
			if (item == null)
				throw new ArgumentNullException("item"); 
			
			return item.MetadataItem;
		}
	}
}
