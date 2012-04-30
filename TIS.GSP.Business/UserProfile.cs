using System;
using GalleryServerPro.Business.Interfaces;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// Represents a profile for a user in the current application.
	/// </summary>
	[Serializable]
	public class UserProfile : IUserProfile, IComparable
	{
		#region Private Fields

		private string _userName;
		private readonly IUserGalleryProfileCollection _galleryProfiles = new UserGalleryProfileCollection();

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the account name of the user these profile settings belong to.
		/// </summary>
		/// <value>The account name of the user.</value>
		public string UserName
		{
			get { return _userName; }
			set { _userName = value; }
		}

		/// <summary>
		/// Gets the collection of gallery profiles for the user. A gallery profile is a set of properties for a user that 
		/// are specific to a particular gallery. Guaranteed to not return null.
		/// </summary>
		/// <value>The gallery profiles.</value>
		public IUserGalleryProfileCollection GalleryProfiles
		{
			get { return _galleryProfiles; }
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets the gallery profile for the specified <paramref name="galleryId" />. Guaranteed to not return null.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <returns>A IUserGalleryProfile containing profile information.</returns>
		public IUserGalleryProfile GetGalleryProfile(int galleryId)
		{
			IUserGalleryProfile profile = GalleryProfiles.FindByGalleryId(galleryId);

			if (profile == null)
			{
				profile = CreateDefaultProfile(galleryId);

				GalleryProfiles.Add(profile);
			}

			return profile;
		}

		/// <summary>
		/// Creates a new instance containing a deep copy of the items it contains.
		/// </summary>
		/// <returns>Returns a new instance containing a deep copy of the items it contains.</returns>
		public IUserProfile Copy()
		{
			IUserProfile copy = new UserProfile();

			copy.UserName = this.UserName;

			copy.GalleryProfiles.AddRange(this.GalleryProfiles.Copy());

			return copy;
		}

		#endregion

		#region Private Functions

		private IUserGalleryProfile CreateDefaultProfile(int galleryId)
		{
			IUserGalleryProfile profile = new UserGalleryProfile(galleryId);
			profile.UserName = UserName;
			profile.ShowMediaObjectMetadata = false; // Redundant since this is the default value, but this is for clarity to programmer
			profile.UserAlbumId = 0; // Redundant since this is the default value, but this is for clarity to programmer
			profile.EnableUserAlbum = Factory.LoadGallerySetting(galleryId).EnableUserAlbumDefaultForUser; ;

			return profile;
		}

		#endregion

		#region IComparable

		/// <summary>
		/// Compares the current instance with another object of the same type.
		/// </summary>
		/// <param name="obj">An object to compare with this instance.</param>
		/// <returns>
		/// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance is less than <paramref name="obj"/>. Zero This instance is equal to <paramref name="obj"/>. Greater than zero This instance is greater than <paramref name="obj"/>.
		/// </returns>
		/// <exception cref="T:System.ArgumentException">
		/// 	<paramref name="obj"/> is not the same type as this instance. </exception>
		public int CompareTo(object obj)
		{
			if (obj == null)
				return 1;
			else
			{
				IUserProfile other = obj as IUserProfile;
				if (other != null)
					return String.Compare(this.UserName, other.UserName, StringComparison.Ordinal);
				else
					return 1;
			}
		}

		#endregion
	}
}
