using System;

namespace GalleryServerPro.Business.Interfaces
{
	/// <summary>
	/// A collection of <see cref="IGallerySettings" /> objects.
	/// </summary>
	public interface IGallerySettingsCollection : System.Collections.Generic.ICollection<IGallerySettings>
	{
		/// <summary>
		/// Sort the objects in this collection based on the <see cref="IGallerySettings.GalleryId" /> property.
		/// </summary>
		void Sort();

		/// <summary>
		/// Adds the gallery settings to the current collection.
		/// </summary>
		/// <param name="gallerySettings">The gallery settings to add to the current collection.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="gallerySettings" /> is null.</exception>
		void AddRange(System.Collections.Generic.IEnumerable<IGallerySettings> gallerySettings);

		/// <summary>
		/// Gets a reference to the <see cref="IGallerySettings" /> object at the specified index position.
		/// </summary>
		/// <param name="indexPosition">An integer specifying the position of the object within this collection to
		/// return. Zero returns the first item.</param>
		/// <returns>Returns a reference to the <see cref="IGallerySettings" /> object at the specified index position.</returns>
		IGallerySettings this[Int32 indexPosition]
		{
			get;
			set;
		}

		/// <summary>
		/// Searches for the specified object and returns the zero-based index of the first occurrence within the collection.  
		/// </summary>
		/// <param name="gallerySettings">The gallery settings to locate in the collection. The value can be a null 
		/// reference (Nothing in Visual Basic).</param>
		/// <returns>The zero-based index of the first occurrence of a gallery settings object within the collection, if found; 
		/// otherwise, –1. </returns>
		Int32 IndexOf(IGallerySettings gallerySettings);

		/// <summary>
		/// Determines whether the <paramref name="item"/> is already a member of the collection. An object is considered a member
		/// of the collection if they both have the same <see cref="IGallerySettings.GalleryId" />.
		/// </summary>
		/// <param name="item">An <see cref="IGallerySettings"/> to determine whether it is a member of the current collection.</param>
		/// <returns>Returns <c>true</c> if <paramref name="item"/> is a member of the current collection;
		/// otherwise returns <c>false</c>.</returns>
		new bool Contains(IGallerySettings item);

		/// <summary>
		/// Adds the specified gallery settings.
		/// </summary>
		/// <param name="item">The gallery settings to add.</param>
		new void Add(IGallerySettings item);

		/// <summary>
		/// Find the gallery settings in the collection that matches the specified <paramref name="galleryId" />. If no matching object is found,
		/// null is returned.
		/// </summary>
		/// <param name="galleryId">The ID that uniquely identifies the gallery.</param>
		/// <returns>Returns an <see cref="IGallerySettings" />object from the collection that matches the specified <paramref name="galleryId" />,
		/// or null if no matching object is found.</returns>
		IGallerySettings FindByGalleryId(int galleryId);
	}
}
