using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalleryServerPro.Business.Interfaces;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// A collection of <see cref="IGallery" /> objects.
	/// </summary>
	public class GalleryCollection : Collection<IGallery>, IGalleryCollection
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GalleryCollection"/> class.
		/// </summary>
		public GalleryCollection()
			: base(new List<IGallery>())
		{
		}

		/// <summary>
		/// Sort the objects in this collection based on the <see cref="IGallery.GalleryId" /> property.
		/// </summary>
		public void Sort()
		{
			// We know galleries is actually a List<IGallery> because we passed it to the constructor.
			List<IGallery> galleries = (List<IGallery>)Items;

			galleries.Sort();
		}

		/// <summary>
		/// Adds the galleries to the current collection.
		/// </summary>
		/// <param name="galleries">The galleries to add to the current collection.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="galleries" /> is null.</exception>
		public void AddRange(IEnumerable<IGallery> galleries)
		{
			if (galleries == null)
				throw new ArgumentNullException("galleries");

			foreach (IGallery gallery in galleries)
			{
				this.Add(gallery);
			}
		}

		/// <summary>
		/// Find the gallery in the collection that matches the specified <paramref name="galleryId" />. If no matching object is found,
		/// null is returned.
		/// </summary>
		/// <param name="galleryId">The ID that uniquely identifies the gallery.</param>
		/// <returns>Returns an <see cref="IGallery" />object from the collection that matches the specified <paramref name="galleryId" />,
		/// or null if no matching object is found.</returns>
		public IGallery FindById(int galleryId)
		{
			List<IGallery> galleries = (List<IGallery>)Items;

			return galleries.Find(delegate(IGallery gallery)
			{
				return (gallery.GalleryId == galleryId);
			});
		}

		/// <summary>
		/// Creates a new, empty instance of an <see cref="IGallery" /> object. This method can be used by code that only has a 
		/// reference to the interface layer and therefore cannot create a new instance of an object on its own.
		/// </summary>
		/// <returns>Returns a new, empty instance of an <see cref="IGallery" /> object.</returns>
		public IGallery CreateEmptyGalleryInstance()
		{
			return new Gallery();
		}

		/// <summary>
		/// Creates a new, empty instance of an <see cref="IGalleryCollection" /> object. This method can be used by code that only has a 
		/// reference to the interface layer and therefore cannot create a new instance of an object on its own.
		/// </summary>
		/// <returns>Returns a new, empty instance of an <see cref="IGalleryCollection" /> object.</returns>
		public IGalleryCollection CreateEmptyGalleryCollection()
		{
			return new GalleryCollection();
		}

		/// <summary>
		/// Creates a new collection containing deep copies of the items it contains.
		/// </summary>
		/// <returns>Returns a new collection containing deep copies of the items it contains.</returns>
		public IGalleryCollection Copy()
		{
			IGalleryCollection copy = new GalleryCollection();

			foreach (IGallery gallery in (List<IGallery>)Items)
			{
				copy.Add(gallery.Copy());
			}

			return copy;
		}

		/// <summary>
		/// Determines whether the <paramref name="item"/> is already a member of the collection. An object is considered a member
		/// of the collection if they both have the same <see cref="IGallery.GalleryId" />.
		/// </summary>
		/// <param name="item">An <see cref="IGallery"/> to determine whether it is a member of the current collection.</param>
		/// <returns>Returns <c>true</c> if <paramref name="item"/> is a member of the current collection;
		/// otherwise returns <c>false</c>.</returns>
		public new bool Contains(IGallery item)
		{
			if (item == null)
				return false;

			foreach (IGallery galleryInCollection in (List<IGallery>)Items)
			{
				if (galleryInCollection.GalleryId == item.GalleryId)
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
		public new void Add(IGallery item)
		{
			if (item == null)
				throw new ArgumentNullException("item", "Cannot add null to an existing GalleryCollection. Items.Count = " + Items.Count);

			base.Add(item);
		}
	}
}
