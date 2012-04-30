using System;

namespace GalleryServerPro.Business.Interfaces
{
	/// <summary>
	/// A collection of <see cref="IGalleryControlSettings" /> objects. There is a maximum of one item for each instance of a Gallery
	/// control that is used in an application. An item will exist in this collection only if at least one control-specific setting
	/// has been saved for a particular control.
	/// </summary>
	public interface IGalleryControlSettingsCollection : System.Collections.Generic.ICollection<IGalleryControlSettings>
	{
		/// <summary>
		/// Adds the gallery control settings to the current collection.
		/// </summary>
		/// <param name="galleryControlSettings">The gallery control settings to add to the current collection.</param>
		void AddRange(System.Collections.Generic.IEnumerable<IGalleryControlSettings> galleryControlSettings);

		/// <summary>
		/// Gets a reference to the <see cref="IGalleryControlSettings" /> object at the specified index position.
		/// </summary>
		/// <param name="indexPosition">An integer specifying the position of the object within this collection to
		/// return. Zero returns the first item.</param>
		/// <returns>Returns a reference to the <see cref="IGalleryControlSettings" /> object at the specified index position.</returns>
		IGalleryControlSettings this[Int32 indexPosition]
		{
			get;
			set;
		}

		/// <summary>
		/// Determines whether the <paramref name="item"/> is already a member of the collection. An object is considered a member
		/// of the collection if they both have the same <see cref="IGalleryControlSettings.ControlId" />.
		/// </summary>
		/// <param name="item">An <see cref="IGalleryControlSettings"/> to determine whether it is a member of the current collection.</param>
		/// <returns>Returns <c>true</c> if <paramref name="item"/> is a member of the current collection;
		/// otherwise returns <c>false</c>.</returns>
		new bool Contains(IGalleryControlSettings item);

		/// <summary>
		/// Adds the specified gallery control settings.
		/// </summary>
		/// <param name="item">The gallery control settings to add.</param>
		new void Add(IGalleryControlSettings item);

		/// <summary>
		/// Find the gallery control settings in the collection that matches the specified <paramref name="controlId" />. If no matching object is found,
		/// null is returned.
		/// </summary>
		/// <param name="controlId">The ID that uniquely identifies the control containing the gallery.</param>
		/// <returns>Returns an <see cref="IGalleryControlSettings" />object from the collection that matches the specified <paramref name="controlId" />,
		/// or null if no matching object is found.</returns>
		IGalleryControlSettings FindByControlId(string controlId);
	}
}
