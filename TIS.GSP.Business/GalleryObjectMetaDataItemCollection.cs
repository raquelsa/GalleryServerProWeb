using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.ErrorHandler.CustomExceptions;

namespace GalleryServerPro.Business.Metadata
{
	/// <summary>
	/// A collection of <see cref="IGalleryObjectMetadataItem" /> objects.
	/// </summary>
	[Serializable]
	class GalleryObjectMetadataItemCollection : Collection<IGalleryObjectMetadataItem>, IGalleryObjectMetadataItemCollection
	{
		private bool? _regenerateAllOnSaveEmptyCollection;
		private FormattedMetadataItemName[] _fileMetadataItemNames;

		/// <summary>
		/// Initializes a new instance of the <see cref="GalleryObjectMetadataItemCollection"/> class.
		/// </summary>
		public GalleryObjectMetadataItemCollection()
			: base(new System.Collections.Generic.List<IGalleryObjectMetadataItem>())
		{
		}

		/// <summary>
		/// Gets an array of file-related metadata item names. These items are sometimes treated differently than other metadata items.
		/// For example, the metadata properties for these names are updated with the current values when a media object file is updated.
		/// These properties are also automatically updated during a synchronization, even when <see cref="SynchronizationManager.RegenerateMetadata" />
		/// is <c>false</c>.
		/// </summary>
		/// <value>The file-related metadata item names.</value>
		public FormattedMetadataItemName[] FileMetadataItemNames
		{
			get
			{
				if (_fileMetadataItemNames == null)
				{
					_fileMetadataItemNames = new FormattedMetadataItemName[] { 
						FormattedMetadataItemName.DateFileCreated, FormattedMetadataItemName.DateFileCreatedUtc, FormattedMetadataItemName.DateFileLastModified, 
						FormattedMetadataItemName.DateFileLastModifiedUtc, FormattedMetadataItemName.FileName, FormattedMetadataItemName.FileNameWithoutExtension,
						FormattedMetadataItemName.FileSizeKb };
				}

				return _fileMetadataItemNames;
			}
		}

		/// <summary>
		/// Determines whether the <paramref name="item"/> is a member of the collection. An object is considered a member
		/// of the collection if the value of its <see cref="IGalleryObjectMetadataItem.MetadataItemName"/> property matches one in the existing collection.
		/// </summary>
		/// <param name="item">The <see cref="IGalleryObjectMetadataItem"/> to search for.</param>
		/// <returns>
		/// Returns <c>true</c> if <paramref name="item"/> is a member of the current collection;
		/// otherwise returns <c>false</c>.
		/// </returns>
		/// <overloads>
		/// Determines whether the collection contains a particular item.
		/// </overloads>
		public new bool Contains(IGalleryObjectMetadataItem item)
		{
			if (item == null)
				return false;

			foreach (IGalleryObjectMetadataItem metadataItemIterator in (System.Collections.Generic.List<IGalleryObjectMetadataItem>)Items)
			{
				if (item.MetadataItemName == metadataItemIterator.MetadataItemName)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Determines whether the <paramref name="metadataItemName"/> is a member of the collection.
		/// </summary>
		/// <param name="metadataItemName">The <see cref="Metadata.FormattedMetadataItemName"/> to search for.</param>
		/// <returns>Returns <c>true</c> if <paramref name="metadataItemName"/> is in the current collection;
		/// otherwise returns <c>false</c>.
		/// </returns>
		public bool Contains(FormattedMetadataItemName metadataItemName)
		{
			IGalleryObjectMetadataItem metadataItem;
			return TryGetMetadataItem(metadataItemName, out metadataItem);
		}

		/// <summary>
		/// Create a new <see cref="IGalleryObjectMetadataItem"/> item from the specified parameters and add it to the collection. Return a
		/// reference to the new item.
		/// </summary>
		/// <param name="mediaObjectMetadataId">A value that uniquely indentifies this metadata item.</param>
		/// <param name="metadataItemName">The name of this metadata item.</param>
		/// <param name="description">The description of the metadata item (e.g. "Exposure time", "Camera model")</param>
		/// <param name="value">The value of the metadata item (e.g. "F5.7", "1/500 sec.").</param>
		/// <param name="hasChanges">A value indicating whether this metadata item has changes that have not been persisted to the database.</param>
		/// <returns>Returns a reference to the new item.</returns>
		public IGalleryObjectMetadataItem AddNew(int mediaObjectMetadataId, FormattedMetadataItemName metadataItemName, string description, string value, bool hasChanges)
		{
			IGalleryObjectMetadataItem metadataItem = new GalleryObjectMetadataItem(mediaObjectMetadataId, metadataItemName, description, value, hasChanges);
			Items.Add(metadataItem);

			return metadataItem;
		}

		/// <summary>
		/// Adds the metadata items to the current collection.
		/// </summary>
		/// <param name="galleryObjectMetadataItems">The metadata items to add to the collection.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryObjectMetadataItems" /> is null.</exception>
		public void AddRange(IGalleryObjectMetadataItemCollection galleryObjectMetadataItems)
		{
			if (galleryObjectMetadataItems == null)
				throw new ArgumentNullException("galleryObjectMetadataItems");

			foreach (IGalleryObjectMetadataItem metadataItem in galleryObjectMetadataItems)
			{
				Items.Add(metadataItem);
			}
		}

		/// <summary>
		/// Apply the <paramref name="metadataDisplayOptions"/> to the items in the collection. This includes sorting the items and updating
		/// the <see cref="IGalleryObjectMetadataItem.IsVisible"/> property.
		/// </summary>
		/// <param name="metadataDisplayOptions">A collection of metadata definition items. Specify <see cref="IGallerySettings.MetadataDisplaySettings"/>
		/// for this parameter.</param>
		public void ApplyDisplayOptions(IMetadataDefinitionCollection metadataDisplayOptions)
		{
			// We know galleryObjectMetadataItems is actually a List<IGalleryObjectMetadataItem> because we passed it to the constructor.
			List<IGalleryObjectMetadataItem> galleryObjectMetadataItems = (List<IGalleryObjectMetadataItem>)Items;

			galleryObjectMetadataItems.Sort(new GalleryObjectMetadataItemComparer(metadataDisplayOptions));

			galleryObjectMetadataItems.ForEach(delegate(IGalleryObjectMetadataItem metaItem)
				{
					IMetadataDefinition metadataDef = metadataDisplayOptions.Find(metaItem.MetadataItemName);
					metaItem.IsVisible = metadataDef.IsVisible;
				});
		}

		/// <summary>
		/// Gets the <see cref="IGalleryObjectMetadataItem"/> object that matches the specified
		/// <see cref="GalleryServerPro.Business.Metadata.FormattedMetadataItemName"/>. The <paramref name="metadataItem"/>
		/// parameter remains null if no matching object is in the collection.
		/// </summary>
		/// <param name="metadataName">The <see cref="GalleryServerPro.Business.Metadata.FormattedMetadataItemName"/> of the
		/// <see cref="IGalleryObjectMetadataItem"/> to get.</param>
		/// <param name="metadataItem">When this method returns, contains the <see cref="IGalleryObjectMetadataItem"/> associated with the
		/// specified <see cref="GalleryServerPro.Business.Metadata.FormattedMetadataItemName"/>, if the key is found; otherwise, the
		/// parameter remains null. This parameter is passed uninitialized.</param>
		/// <returns>
		/// Returns true if the <see cref="IGalleryObjectMetadataItemCollection"/> contains an element with the specified
		/// <see cref="GalleryServerPro.Business.Metadata.FormattedMetadataItemName"/>; otherwise, false.
		/// </returns>
		public bool TryGetMetadataItem(FormattedMetadataItemName metadataName, out IGalleryObjectMetadataItem metadataItem)
		{
			// We know galleryObjectMetadataItems is actually a List<IGalleryObjectMetadataItem> because we passed it to the constructor.
			List<IGalleryObjectMetadataItem> galleryObjectMetadataItems = (List<IGalleryObjectMetadataItem>)Items;

			metadataItem = galleryObjectMetadataItems.Find(delegate(IGalleryObjectMetadataItem metaItem)
				{
					return (metaItem.MetadataItemName == metadataName);
				});

			return (metadataItem != null);
		}

		/// <summary>
		/// Gets or sets a value indicating whether file-related metadata, such as file size and last modified timestamp, is to be updated
		/// with the current file properties when the media object is saved. Setting this to <c>true</c> sets the
		/// <see cref="IGalleryObjectMetadataItem.ExtractFromFileOnSave"/> to <c>true</c> for each of the file-related metadata items.
		/// The <see cref="IGalleryObject.Save"/> routine will notice this and update the properties.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if file-related metadata is to be updated during a save operation; otherwise, <c>false</c>.
		/// </value>
		public bool RefreshFileMetadataOnSave
		{
			get
			{
				List<IGalleryObjectMetadataItem> galleryObjectMetadataItems = (List<IGalleryObjectMetadataItem>)Items;

				return galleryObjectMetadataItems.Exists(delegate(IGalleryObjectMetadataItem metaItem)
					{
						return (metaItem.ExtractFromFileOnSave && (Array.IndexOf(FileMetadataItemNames, metaItem.MetadataItemName) >= 0));
					});
			}
			set
			{
				foreach (FormattedMetadataItemName fileMetadataItemName in FileMetadataItemNames)
				{
					IGalleryObjectMetadataItem metadataItem;
					if (TryGetMetadataItem(fileMetadataItemName, out metadataItem))
					{
						metadataItem.ExtractFromFileOnSave = value;
					}
				}
			}
		}

		/// <summary>
		/// Get a list of items whose metadata must be updated with the metadata currently in the media object's file. All
		/// IGalleryObjectMetadataItem whose ExtractFromFileOnSave property are true are returned. This is called during a
		/// save operation to indicate which metadata items must be updated. Guaranteed to not return null. If no items
		/// are found, an empty collection is returned.
		/// </summary>
		/// <returns>
		/// Returns a list of items whose metadata must be updated with the metadata currently in the media object's file.
		/// </returns>
		public IGalleryObjectMetadataItemCollection GetItemsToUpdate()
		{
			// We know galleryObjectMetadataItems is actually a List<IGalleryObjectMetadataItem> because we passed it to the constructor.
			System.Collections.Generic.List<IGalleryObjectMetadataItem> galleryObjectMetadataItems = (System.Collections.Generic.List<IGalleryObjectMetadataItem>)Items;
			IGalleryObjectMetadataItemCollection metadataItemsCollection = new GalleryObjectMetadataItemCollection();

			galleryObjectMetadataItems.ForEach(delegate(IGalleryObjectMetadataItem metaItem)
				{
					if (metaItem.ExtractFromFileOnSave)
					{
						metadataItemsCollection.Add(metaItem);
					}
				});

			return metadataItemsCollection;
		}

		/// <summary>
		/// Get a list of items whose metadata must be persisted to the data store, either because it has been added or because
		/// it has been modified. All IGalleryObjectMetadataItem whose HasChanges property are true are returned. This is called during a
		/// save operation to indicate which metadata items must be saved. Guaranteed to not return null. If no items
		/// are found, an empty collection is returned.
		/// </summary>
		/// <returns>
		/// Returns a list of items whose metadata must be updated with the metadata currently in the media object's file.
		/// </returns>
		public IGalleryObjectMetadataItemCollection GetItemsToSave()
		{
			// We know galleryObjectMetadataItems is actually a List<IGalleryObjectMetadataItem> because we passed it to the constructor.
			System.Collections.Generic.List<IGalleryObjectMetadataItem> galleryObjectMetadataItems = (System.Collections.Generic.List<IGalleryObjectMetadataItem>)Items;
			IGalleryObjectMetadataItemCollection metadataItemsCollection = new GalleryObjectMetadataItemCollection();

			galleryObjectMetadataItems.ForEach(delegate(IGalleryObjectMetadataItem metaItem)
				{
					if (metaItem.HasChanges)
					{
						metadataItemsCollection.Add(metaItem);
					}
				});

			return metadataItemsCollection;
		}

		/// <summary>
		/// Perform a deep copy of this metadata collection.
		/// </summary>
		/// <returns>
		/// Returns a deep copy of this metadata collection.
		/// </returns>
		public IGalleryObjectMetadataItemCollection Copy()
		{
			IGalleryObjectMetadataItemCollection metaDataItemCollectionCopy = new GalleryObjectMetadataItemCollection();

			foreach (IGalleryObjectMetadataItem metaDataItem in this.Items)
			{
				metaDataItemCollectionCopy.Add(metaDataItem.Copy());
			}

			return metaDataItemCollectionCopy;
		}

		/// <summary>
		/// Gets the items in the collection that are visible to the UI. That is, get the items where <see cref="IGalleryObjectMetadataItem.IsVisible" />
		/// = <c>true</c>.
		/// </summary>
		/// <returns>Returns a list of items that are visible to the UI.</returns>
		public IGalleryObjectMetadataItemCollection GetVisibleItems()
		{
			// We know galleryObjectMetadataItems is actually a List<IGalleryObjectMetadataItem> because we passed it to the constructor.
			List<IGalleryObjectMetadataItem> galleryObjectMetadataItems = (List<IGalleryObjectMetadataItem>)Items;
			IGalleryObjectMetadataItemCollection metadataItemsCollection = new GalleryObjectMetadataItemCollection();

			galleryObjectMetadataItems.ForEach(delegate(IGalleryObjectMetadataItem metaItem)
				{
					if (metaItem.IsVisible)
					{
						metadataItemsCollection.Add(metaItem);
					}
				});

			return metadataItemsCollection;
		}

		/// <summary>
		/// Gets or sets a value indicating whether all metadata items in the collection should be replaced with the current
		/// metadata in the image file. This property is calculated based on the <see cref="IGalleryObjectMetadataItem.ExtractFromFileOnSave"/>
		/// property on each metadata item in the collection. Setting this property causes the
		/// <see cref="IGalleryObjectMetadataItem.ExtractFromFileOnSave"/>  property to be set to the specified value for *every* metadata item in the collection.
		/// If the collection is empty, then the value is stored in a private class field. Note that since new items added to
		/// the collection have their <see cref="IGalleryObjectMetadataItem.ExtractFromFileOnSave"/> property set to false, if you set <see cref="ExtractOnSave"/> = "true" on
		/// an empty collection, then add one or more items, this property will subsequently return false. This property is
		/// ignored for <see cref="IAlbum"/> objects.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if <see cref="IGalleryObjectMetadataItem.ExtractFromFileOnSave"/>  =  true for *every* metadata item in
		/// the collection; otherwise, <c>false</c>.
		/// </value>
		public bool ExtractOnSave
		{
			get
			{
				// We know galleryObjectMetadataItems is actually a List<IGalleryObjectMetadataItem> because we passed it to the constructor.
				// Return true if all metadata items have the ExtractFromFileOnSave property set to true. If we have an empty collection,
				// return the value of a private field if it was previously set; otherwise return false.
				System.Collections.Generic.List<IGalleryObjectMetadataItem> galleryObjectMetadataItems = (System.Collections.Generic.List<IGalleryObjectMetadataItem>)Items;

				if (galleryObjectMetadataItems.Count > 0)
				{
					return galleryObjectMetadataItems.TrueForAll(delegate(IGalleryObjectMetadataItem metaItem)
						{
							return metaItem.ExtractFromFileOnSave;
						});
				}
				else if (this._regenerateAllOnSaveEmptyCollection.HasValue)
				{
					return this._regenerateAllOnSaveEmptyCollection.Value;
				}
				else
				{
					return false;
				}
			}
			set
			{
				// We know galleryObjectMetadataItems is actually a List<IGalleryObjectMetadataItem> because we passed it to the constructor.
				// Store value in ExtractFromFileOnSave property on each item. If we have an empty collection, store in a private field.
				System.Collections.Generic.List<IGalleryObjectMetadataItem> galleryObjectMetadataItems = (System.Collections.Generic.List<IGalleryObjectMetadataItem>)Items;

				if (galleryObjectMetadataItems.Count > 0)
				{
					galleryObjectMetadataItems.ForEach(delegate(IGalleryObjectMetadataItem metaItem)
						{
							metaItem.ExtractFromFileOnSave = value;
						});
				}
				else
				{
					this._regenerateAllOnSaveEmptyCollection = value;
				}
			}
		}
	}

	/// <summary>
	/// Defines a method for comparing two instances of <see cref="IGalleryObjectMetadataItem" /> objects. The items are compared using
	/// the <see cref="IMetadataDefinition.Sequence" /> property of the <see cref="IMetadataDefinitionCollection" /> passed to the
	/// constructor.
	/// </summary>
	/// <remarks>Instances of <see cref="IMetadataDefinitionCollection" /> are sorted according to the sequence defined in the 
	/// gallery setting <see cref="IGallerySettings.MetadataDisplaySettings" />. That is, this class looks up the corresponding
	/// metadata item in this property and uses its <see cref="IMetadataDefinition.Sequence" /> property for the comparison.</remarks>
	public class GalleryObjectMetadataItemComparer : IComparer<IGalleryObjectMetadataItem>
	{
		private readonly IMetadataDefinitionCollection _metadataDisplayOptions;

		/// <summary>
		/// Initializes a new instance of the <see cref="GalleryObjectMetadataItemComparer"/> class. The items are compared using
		/// the <see cref="IMetadataDefinition.Sequence" /> property of the <paramref name="metadataDisplayOptions" /> parameter.
		/// </summary>
		/// <param name="metadataDisplayOptions">The metadata display options.</param>
		public GalleryObjectMetadataItemComparer(IMetadataDefinitionCollection metadataDisplayOptions)
		{
			_metadataDisplayOptions = metadataDisplayOptions;
		}

		/// <summary>
		/// Compares the two instances and returns a value indicating their sort relation to each other.
		/// -1: obj1 is less than obj2
		/// 0: obj1 is equal to obj2
		/// 1: obj1 is greater than obj2
		/// </summary>
		/// <param name="x">One of the instances to compare.</param>
		/// <param name="y">One of the instances to compare.</param>
		/// <returns>Returns in integer indicating the objects' sort relation to each other.</returns>
		public int Compare(IGalleryObjectMetadataItem x, IGalleryObjectMetadataItem y)
		{
			if (x == null)
			{
				// If obj1 is null and obj2 is null, they're equal.
				// If obj1 is null and obj2 is not null, obj2 is greater.
				return (y == null ? 0 : -1);
			}
			else
			{
				if (y == null)
				{
					return 1; // obj1 is not null and obj2 is null, so obj1 is greater.
				}

				// Neither is null. Look up the display settings for each item and sort by its associated sequence property.
				IMetadataDefinition obj1MetadataDefinition = _metadataDisplayOptions.Find(x.MetadataItemName);
				IMetadataDefinition obj2MetadataDefinition = _metadataDisplayOptions.Find(y.MetadataItemName);

				if ((obj1MetadataDefinition != null) && (obj2MetadataDefinition != null))
				{
					return obj1MetadataDefinition.Sequence.CompareTo(obj2MetadataDefinition.Sequence);
				}
				else
				{
					// Can't find one of the display settings. This should never occur because the IMetadataDefinitionCollection should 
					// have an entry for every value of the FormattedMetadataItemName enumeration.
					throw new BusinessException(String.Format(CultureInfo.CurrentCulture, "The IMetadataDefinitionCollection instance passed to the GalleryObjectMetadataItemComparer constructor did not have an item corresponding to one of these FormattedMetadataItemName enum values: {0}, {1}. This collection should contain an item for every enum value.", x.MetadataItemName, y.MetadataItemName));
				}
			}
		}
	}
}
