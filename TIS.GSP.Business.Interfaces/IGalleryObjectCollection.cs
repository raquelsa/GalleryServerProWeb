using System;

namespace GalleryServerPro.Business.Interfaces
{
	/// <summary>
	/// A collection of <see cref="IGalleryObject" /> objects.
	/// </summary>
	public interface IGalleryObjectCollection : System.Collections.Generic.ICollection<IGalleryObject>
	{
		/// <summary>
		/// Sort the objects in this collection based on the <see cref="IGalleryObject.Sequence" /> property.
		/// </summary>
		void Sort();

		/// <summary>
		/// Adds the galleryObjects to the current collection.
		/// </summary>
		/// <param name="galleryObjects">The gallery objects to add to the current collection.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryObjects" /> is null.</exception>
		void AddRange(IGalleryObjectCollection galleryObjects);

		/// <summary>
		/// Gets a reference to the <see cref="IGalleryObject" /> object at the specified index position.
		/// </summary>
		/// <param name="indexPosition">An integer specifying the position of the object within this collection to
		/// return. Zero returns the first item.</param>
		/// <returns>Returns a reference to the <see cref="IGalleryObject" /> object at the specified index position.</returns>
		IGalleryObject this[Int32 indexPosition]
		{
			get;
			set;
		}

		/// <summary>
		/// Searches for the specified object and returns the zero-based index of the first occurrence within the collection.  
		/// </summary>
		/// <param name="galleryObject">The gallery object to locate in the collection. The value can be a null 
		/// reference (Nothing in Visual Basic).</param>
		/// <returns>The zero-based index of the first occurrence of galleryObject within the collection, if found; 
		/// otherwise, –1. </returns>
		Int32 IndexOf(IGalleryObject galleryObject);

		/// <summary>
		/// Determines whether the <paramref name="item"/> is already a member of the collection. An object is considered a member
		/// of the collection if one of the following scenarios is true: (1) They are both of the same type, each ID is 
		/// greater than int.MinValue, and the IDs are equal to each other, or (2) They are new objects that haven't yet
		/// been saved to the data store, the physical path to the original file has been specified, and the paths
		/// are equal to each other.
		/// </summary>
		/// <param name="item">An <see cref="IGalleryObject"/> to determine whether it is a member of the current collection.</param>
		/// <returns>Returns <c>true</c> if <paramref name="item"/> is a member of the current collection;
		/// otherwise returns <c>false</c>.</returns>
		new bool Contains(IGalleryObject item);

		/// <summary>
		/// Adds the specified gallery object.
		/// </summary>
		/// <param name="item">The gallery object.</param>
		new void Add(IGalleryObject item);

		/// <summary>
		/// Return an unsorted list of items in the collection that match the specified gallery object type. Returns an empty
		/// collection if no matching objects are found.
		/// </summary>
		/// <param name="galleryObjectType">The type of gallery object to return. <see cref="GalleryObjectType.MediaObject" /> specifies
		/// all non-album media objects.</param>
		/// <returns>Returns an unsorted list of items in the collection that match the specified gallery object type.</returns>
		IGalleryObjectCollection FindAll(GalleryObjectType galleryObjectType);

		/// <summary>
		/// Find the gallery object in the collection that matches the specified <see cref="IGalleryObject.Id">ID</see> and type. If no matching object is found,
		/// null is returned.
		/// </summary>
		/// <param name="galleryObjectId">The <see cref="IGalleryObject.Id">ID</see> that uniquely identifies the album or media object.</param>
		/// <param name="galleryObjectType">The type of gallery object to which the galleryObjectId applies. Valid values
		/// are <see cref="GalleryObjectType.Album" /> and <see cref="GalleryObjectType.MediaObject" />. An exception is thrown if any other value is specified.</param>
		/// <returns>Returns an <see cref="IGalleryObject" />object from the collection that matches the specified 
		/// <see cref="IGalleryObject.Id">ID</see> and type, or null if no matching object is found.</returns>
		/// <remarks>The <see cref="IGalleryObject.Id">ID</see> for albums and media objects are managed separately. As a result, the same 
		/// <see cref="IGalleryObject.Id">ID</see> may be used for both an album and media object, and these two objects may end up in the 
		/// same collection (for example, if they are both in the same parent album.) Therefore, this method requires that we specify the 
		/// type of gallery object.</remarks>
		IGalleryObject FindById(int galleryObjectId, GalleryObjectType galleryObjectType);
	}
}
