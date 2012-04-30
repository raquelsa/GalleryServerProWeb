namespace GalleryServerPro.Business.Interfaces
{
	/// <summary>
	/// Represents a set of properties for a user that are specific to a particular gallery.
	/// </summary>
	public interface IUserGalleryProfile
	{
		/// <summary>
		/// Gets or sets the ID of the gallery the profile properties are associated with.
		/// </summary>
		/// <value>The gallery ID.</value>
		int GalleryId { get; set; }

		/// <summary>
		/// Gets or sets the account name of the user these profile settings belong to.
		/// </summary>
		/// <value>The account name of the user.</value>
		string UserName { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the user wants the metadata popup window to be displayed.
		/// </summary>
		/// <value>A value indicating whether the user wants the metadata popup window to be displayed.</value>
		bool ShowMediaObjectMetadata { get; set; }

		/// <summary>
		/// Gets or sets the ID for the user's personal album (aka user album).
		/// </summary>
		/// <value>The ID for the user's personal album (aka user album).</value>
		int UserAlbumId { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the user has enabled or disabled her personal album (aka user album).
		/// </summary>
		/// <value>A value indicating whether the user has enabled or disabled her personal album (aka user album).</value>
		bool EnableUserAlbum { get; set; }

		/// <summary>
		/// Creates a new instance containing a deep copy of the items it contains.
		/// </summary>
		/// <returns>Returns a new instance containing a deep copy of the items it contains.</returns>
		IUserGalleryProfile Copy();
	}
}