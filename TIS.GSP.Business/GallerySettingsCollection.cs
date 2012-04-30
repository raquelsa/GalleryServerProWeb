using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalleryServerPro.Business.Interfaces;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// Represents a set of gallery-specific settings.
	/// </summary>
	public class GallerySettingsCollection : Collection<IGallerySettings>, IGallerySettingsCollection
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GallerySettingsCollection"/> class.
		/// </summary>
		public GallerySettingsCollection()
			: base(new List<IGallerySettings>())
		{
		}

		/// <summary>
		/// Sort the objects in this collection based on the <see cref="IGallerySettings.GalleryId"/> property.
		/// </summary>
		public void Sort()
		{
			// We know galleries is actually a List<IGallerySettings> because we passed it to the constructor.
			List<IGallerySettings> galleries = (List<IGallerySettings>)Items;

			galleries.Sort();
		}

		/// <summary>
		/// Adds the gallery settings to the current collection.
		/// </summary>
		/// <param name="gallerySettings">The gallery settings to add to the current collection.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="gallerySettings" /> is null.</exception>
		public void AddRange(IEnumerable<IGallerySettings> gallerySettings)
		{
			if (gallerySettings == null)
				throw new ArgumentNullException("gallerySettings");

			foreach (IGallerySettings gallerySetting in gallerySettings)
			{
				this.Add(gallerySetting);
			}
		}

		/// <summary>
		/// Find the gallery settings in the collection that matches the specified <paramref name="galleryId"/>. If no matching object is found,
		/// null is returned.
		/// </summary>
		/// <param name="galleryId">The ID that uniquely identifies the gallery.</param>
		/// <returns>
		/// Returns an <see cref="IGallerySettings"/>object from the collection that matches the specified <paramref name="galleryId"/>,
		/// or null if no matching object is found.
		/// </returns>
		public IGallerySettings FindByGalleryId(int galleryId)
		{
			List<IGallerySettings> gallerySettings = (List<IGallerySettings>)Items;

			return gallerySettings.Find(delegate(IGallerySettings gallery)
			{
				return (gallery.GalleryId == galleryId);
			});
		}

		/// <summary>
		/// Determines whether the <paramref name="item"/> is already a member of the collection. An object is considered a member
		/// of the collection if they both have the same <see cref="IGallerySettings.GalleryId"/>.
		/// </summary>
		/// <param name="item">An <see cref="IGallerySettings"/> to determine whether it is a member of the current collection.</param>
		/// <returns>
		/// Returns <c>true</c> if <paramref name="item"/> is a member of the current collection;
		/// otherwise returns <c>false</c>.
		/// </returns>
		public new bool Contains(IGallerySettings item)
		{
			if (item == null)
				return false;

			foreach (IGallerySettings gallerySettingsInCollection in (List<IGallerySettings>)Items)
			{
				if (gallerySettingsInCollection.GalleryId == item.GalleryId)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Adds the specified gallery.
		/// </summary>
		/// <param name="item">The gallery to add.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="item" /> is null.</exception>
		public new void Add(IGallerySettings item)
		{
			if (item == null)
				throw new ArgumentNullException("item", "Cannot add null to an existing GallerySettingsCollection. Items.Count = " + Items.Count);

			base.Add(item);
		}
	}
}
