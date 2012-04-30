using GalleryServerPro.Business.Interfaces;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// Defines a list that uniquely identifies cache items stored in the cache.
	/// </summary>
	public enum CacheItem
	{
		/// <summary>
		/// A System.Collections.Generic.Dictionary&lt;<see cref="string" />, <see cref="GalleryServerPro.Business.Interfaces.IGalleryServerRoleCollection" />&gt;
		/// stored in cache. The key is a concatenation of the user's session ID and user name. The corresponding value stores the roles that 
		/// user belongs to. The first item in the dictionary will have a key = "AllRoles", and its dictionary entry holds all 
		/// roles used in the current gallery.
		/// </summary>
		GalleryServerRoles,
		/// <summary>
		/// A <see cref="IUserAccountCollection"/> containing a list of all users as reported by the membership provider (Membership.GetAllUsers()).
		/// </summary>
		Users,
		/// <summary>
		/// A System.Collections.Generic.Dictionary&lt;<see cref="string" />, <see cref="GalleryServerPro.Business.Interfaces.IUserAccountCollection" />&gt;
		/// stored in cache. The key is a concatenation of the user's session ID and user name. The corresponding value stores the users that 
		/// the current user has permission to view.
		/// </summary>
		UsersCurrentUserCanView,
		/// <summary>
		/// A System.Collections.Generic.Dictionary&lt;<see cref="int" />, <see cref="GalleryServerPro.Business.Interfaces.IAlbum" />&gt; 
		/// stored in cache. The key specifies the ID of the album stored in the dictionary entry.
		/// </summary>
		Albums,
		/// <summary>
		/// A System.Collections.Generic.Dictionary&lt;<see cref="int" />, <see cref="GalleryServerPro.Business.Interfaces.IGalleryObject" />&gt; 
		/// stored in cache. The key specifies the ID of the media object stored in the dictionary entry.
		/// </summary>
		MediaObjects,
		/// <summary>
		/// An <see cref="GalleryServerPro.Business.Interfaces.IAppErrorCollection" /> stored in cache.
		/// </summary>
		AppErrors,
		/// <summary>
		/// A System.Collections.Generic.Dictionary&lt;<see cref="string" />, <see cref="GalleryServerPro.Business.Interfaces.IUserProfile" />&gt; 
		/// stored in cache. The key specifies the username of the profile stored in the dictionary entry.
		/// </summary>
		Profiles
	}
}
