namespace GalleryServerPro.Business.Interfaces
{
	/// <summary>
	/// Represents a profile for a user in the current application.
	/// </summary>
	public interface IUserProfile
	{
		/// <summary>
		/// Gets or sets the account name of the user these profile settings belong to.
		/// </summary>
		/// <value>The account name of the user.</value>
		string UserName { get; set; }
		
		/// <summary>
		/// Gets the collection of gallery profiles for the user. A gallery profile is a set of properties for a user that 
		/// are specific to a particular gallery. Guaranteed to not return null.
		/// </summary>
		/// <value>The gallery profiles.</value>
		IUserGalleryProfileCollection GalleryProfiles { get; }

		/// <summary>
		/// Gets the gallery profile for the specified <paramref name="galleryId" />. Guaranteed to not return null.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <returns>A IUserGalleryProfile containing profile information.</returns>
		IUserGalleryProfile GetGalleryProfile(int galleryId);

		/// <summary>
		/// Creates a new instance containing a deep copy of the items it contains.
		/// </summary>
		/// <returns>Returns a new instance containing a deep copy of the items it contains.</returns>
		IUserProfile Copy();
	}
}