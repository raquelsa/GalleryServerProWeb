using System;
using GalleryServerPro.Business.Interfaces;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// Represents a set of properties for a user that are specific to a particular gallery.
	/// </summary>
	[Serializable]
	public class UserGalleryProfile : IUserGalleryProfile, IComparable
	{
		#region Private Fields

		private int _galleryId;
		private string _userName = string.Empty;
		private bool _showMediaObjectMetadata;
		private int _userAlbumId;
		private bool _enableUserAlbum;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="UserGalleryProfile"/> class.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		public UserGalleryProfile(int galleryId)
		{
			_galleryId = galleryId;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the ID of the gallery the profile properties are associated with.
		/// </summary>
		/// <value>The gallery ID.</value>
		public int GalleryId
		{
			get { return _galleryId; }
			set { _galleryId = value; }
		}

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
		/// Gets or sets a value indicating whether the user wants the metadata popup window to be displayed.
		/// </summary>
		/// <value>
		/// A value indicating whether the user wants the metadata popup window to be displayed.
		/// </value>
		public bool ShowMediaObjectMetadata
		{
			get { return _showMediaObjectMetadata; }
			set { _showMediaObjectMetadata = value; }
		}

		/// <summary>
		/// Gets or sets the ID for the user's personal album (aka user album).
		/// </summary>
		/// <value>The ID for the user's personal album (aka user album).</value>
		public int UserAlbumId
		{
			get { return _userAlbumId; }
			set { _userAlbumId = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the user has enabled or disabled her personal album (aka user album).
		/// </summary>
		/// <value>
		/// A value indicating whether the user has enabled or disabled her personal album (aka user album).
		/// </value>
		public bool EnableUserAlbum
		{
			get { return _enableUserAlbum; }
			set { _enableUserAlbum = value; }
		}

		#endregion

		#region Methods

		/// <summary>
		/// Creates a new instance containing a deep copy of the items it contains.
		/// </summary>
		/// <returns>Returns a new instance containing a deep copy of the items it contains.</returns>
		public IUserGalleryProfile Copy()
		{
			IUserGalleryProfile copy = new UserGalleryProfile(GalleryId);

			copy.UserName = this.UserName;
			copy.ShowMediaObjectMetadata = this.ShowMediaObjectMetadata;
			copy.EnableUserAlbum = this.EnableUserAlbum;
			copy.UserAlbumId = this.UserAlbumId;

			return copy;
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
				IUserGalleryProfile other = obj as IUserGalleryProfile;
				if (other != null)
					return this.GalleryId.CompareTo(other.GalleryId);
				else
					return 1;
			}
		}

		#endregion
	}
}