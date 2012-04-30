using System;
using System.Web;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;

namespace GalleryServerPro.Web.Controller
{
	/// <summary>
	/// Contains functionality related to managing the user profile.
	/// </summary>
	public static class ProfileController
	{
		#region Public Methods

		/// <overloads>
		/// Gets the gallery-specific user profile for a user.
		/// </overloads>
		/// <summary>
		/// Gets the gallery-specific user profile for the currently logged on user and specified <paramref name="galleryId"/>.
		/// Guaranteed to not return null (returns an empty object if no profile is found).
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <returns>Gets the profile for the current user and the specified gallery.</returns>
		public static IUserGalleryProfile GetProfileForGallery(int galleryId)
		{
			return GetProfileForGallery(Utils.UserName, galleryId);
		}

		/// <summary>
		/// Gets the gallery-specific user profile for the specified <paramref name="userName"/> and <paramref name="galleryId"/>.
		/// Guaranteed to not return null (returns an empty object if no profile is found).
		/// </summary>
		/// <param name="userName">The account name for the user whose profile settings are to be retrieved. You can specify null or an empty string
		/// for anonymous users.</param>
		/// <param name="galleryId">The gallery ID.</param>
		/// <returns>Gets the profile for the specified user and gallery.</returns>
		public static IUserGalleryProfile GetProfileForGallery(string userName, int galleryId)
		{
			return GetProfile(userName).GetGalleryProfile(galleryId);
		}

		/// <overloads>
		/// Gets a user's profile. The UserName property will be an empty string 
		/// for anonymous users and the remaining properties will be set to default values.
		/// </overloads>
		/// <summary>
		/// Gets the profile for the current user.
		/// </summary>
		/// <returns>Gets the profile for the current user.</returns>
		public static IUserProfile GetProfile()
		{
			return GetProfile(Utils.UserName);
		}

		/// <summary>
		/// Gets the user profile for the specified <paramref name="userName" />. Guaranteed to not
		/// return null (returns an empty object if no profile is found).
		/// </summary>
		/// <param name="userName">The account name for the user whose profile settings are to be retrieved. You can specify null or an empty string
		/// for anonymous users.</param>
		/// <returns>Gets the profile for the specified user.</returns>
		public static IUserProfile GetProfile(string userName)
		{
			if (String.IsNullOrEmpty(userName))
			{
				// Anonymous user. Get from session. If not found in session, return an empty object.
				return GetProfileFromSession() ?? new UserProfile();
			}
			else
			{
				return Factory.LoadUserProfile(userName);
			}
		}

		/// <summary>
		/// Saves the specified <paramref name="userProfile" />. Anonymous profiles (those with an 
		/// empty string in <see cref="IUserProfile.UserName" />) are saved to session; profiles for 
		/// users with accounts are persisted to the data store. The profile cache is automatically
		/// cleared.
		/// </summary>
		/// <param name="userProfile">The user profile to save.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="userProfile" /> is null.</exception>
		public static void SaveProfile(IUserProfile userProfile)
		{
			if (userProfile == null)
				throw new ArgumentNullException("userProfile");

			if (String.IsNullOrEmpty(userProfile.UserName))
				SaveProfileToSession(userProfile);
			else
			{
				Factory.SaveUserProfile(userProfile);
			}
		}

		/// <summary>
		/// Permanently delete the profile records for the specified <paramref name="userName" />.
		/// </summary>
		/// <param name="userName">The user name that uniquely identifies the user.</param>
		public static void DeleteProfileForUser(string userName)
		{
			Factory.DeleteUserProfile(userName);
		}

		/// <summary>
		/// Permanently delete the profile records associated with the specified <paramref name="gallery" />.
		/// </summary>
		/// <param name="gallery">The gallery.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="gallery" /> is null.</exception>
		public static void DeleteProfileForGallery(Business.Gallery gallery)
		{
			if (gallery == null)
				throw new ArgumentNullException("gallery");

			Factory.GetDataProvider().Profile_DeleteProfilesForGallery(gallery.GalleryId);
		}

		#endregion

		#region Private Functions

		/// <summary>
		/// Gets the current user's profile from session. Returns null if no object is found.
		/// </summary>
		/// <returns>Returns an instance of <see cref="IUserProfile" /> or null if no profile
		/// is found in session.</returns>
		private static IUserProfile GetProfileFromSession()
		{
			IUserProfile pc = null;

			if (HttpContext.Current.Session != null)
			{
				pc = HttpContext.Current.Session["_Profile"] as IUserProfile;
			}

			return pc;
		}

		private static void SaveProfileToSession(IUserProfile userProfile)
		{
			if (HttpContext.Current.Session != null)
			{
				HttpContext.Current.Session["_Profile"] = userProfile;
			}
		}

		#endregion
	}
}
