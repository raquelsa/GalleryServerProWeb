using System;

namespace GalleryServerPro.Business.Interfaces
{
	/// <summary>
	/// A collection of <see cref="IGallery" /> objects.
	/// </summary>
	public interface IGalleryCollection : System.Collections.Generic.ICollection<IGallery>
	{
		/// <summary>
		/// Sort the objects in this collection based on the <see cref="IGallery.GalleryId" /> property.
		/// </summary>
		void Sort();

		/// <summary>
		/// Adds the galleries to the current collection.
		/// </summary>
		/// <param name="galleries">The galleries to add to the current collection.</param>
		void AddRange(System.Collections.Generic.IEnumerable<IGallery> galleries);

		/// <summary>
		/// Gets a reference to the <see cref="IGallery" /> object at the specified index position.
		/// </summary>
		/// <param name="indexPosition">An integer specifying the position of the object within this collection to
		/// return. Zero returns the first item.</param>
		/// <returns>Returns a reference to the <see cref="IGallery" /> object at the specified index position.</returns>
		IGallery this[Int32 indexPosition]
		{
			get;
			set;
		}

		/// <summary>
		/// Searches for the specified object and returns the zero-based index of the first occurrence within the collection.  
		/// </summary>
		/// <param name="gallery">The gallery to locate in the collection. The value can be a null 
		/// reference (Nothing in Visual Basic).</param>
		/// <returns>The zero-based index of the first occurrence of gallery within the collection, if found; 
		/// otherwise, –1. </returns>
		Int32 IndexOf(IGallery gallery);

		/// <summary>
		/// Determines whether the <paramref name="item"/> is already a member of the collection. An object is considered a member
		/// of the collection if they both have the same <see cref="IGallery.GalleryId" />.
		/// </summary>
		/// <param name="item">An <see cref="IGallery"/> to determine whether it is a member of the current collection.</param>
		/// <returns>Returns <c>true</c> if <paramref name="item"/> is a member of the current collection;
		/// otherwise returns <c>false</c>.</returns>
		new bool Contains(IGallery item);

		/// <summary>
		/// Adds the specified gallery.
		/// </summary>
		/// <param name="item">The gallery to add.</param>
		new void Add(IGallery item);

		/// <summary>
		/// Find the gallery in the collection that matches the specified <paramref name="galleryId" />. If no matching object is found,
		/// null is returned.
		/// </summary>
		/// <param name="galleryId">The ID that uniquely identifies the gallery.</param>
		/// <returns>Returns an <see cref="IGallery" />object from the collection that matches the specified <paramref name="galleryId" />,
		/// or null if no matching object is found.</returns>
		IGallery FindById(int galleryId);

		/// <summary>
		/// Creates a new, empty instance of an <see cref="IGallery" /> object. This method can be used by code that only has a 
		/// reference to the interface layer and therefore cannot create a new instance of an object on its own.
		/// </summary>
		/// <returns>Returns a new, empty instance of an <see cref="IGallery" /> object.</returns>
		IGallery CreateEmptyGalleryInstance();

		/// <summary>
		/// Creates a new, empty instance of an <see cref="IGalleryCollection" /> object. This method can be used by code that only has a 
		/// reference to the interface layer and therefore cannot create a new instance of an object on its own.
		/// </summary>
		/// <returns>Returns a new, empty instance of an <see cref="IGalleryCollection" /> object.</returns>
		IGalleryCollection CreateEmptyGalleryCollection();

		/// <summary>
		/// Creates a new collection containing deep copies of the items it contains.
		/// </summary>
		/// <returns>Returns a new collection containing deep copies of the items it contains.</returns>
		IGalleryCollection Copy();
	}
}
