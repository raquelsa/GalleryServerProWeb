using System;
using System.Collections.Generic;
using GalleryServerPro.Business.Interfaces;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// Represents a gallery within Gallery Server Pro.
	/// </summary>
	[System.Diagnostics.DebuggerDisplay("Gallery ID = {_id}")]
	public class Gallery : IGallery, IComparable
	{
		#region Private Fields

		private int _id;
		private int _rootAlbumId = int.MinValue;
		private string _description;
		private DateTime _creationDate;
		private Dictionary<int, List<int>> _albums;

		#endregion

		#region Events

		/// <summary>
		/// Occurs when a gallery is first created, just after it is persisted to the data store.
		/// </summary>
		public static event EventHandler<GalleryCreatedEventArgs> GalleryCreated;

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets the unique identifier for this gallery.
		/// </summary>
		/// <value>The unique identifier for this gallery.</value>
		public int GalleryId
		{
			get { return _id; }
			set { _id = value; }
		}

		/// <summary>
		/// Gets a value indicating whether this object is new and has not yet been persisted to the data store.
		/// </summary>
		/// <value><c>true</c> if this instance is new; otherwise, <c>false</c>.</value>
		public bool IsNew
		{
			get
			{
				return (_id == int.MinValue);
			}
		}

		/// <summary>
		/// Gets or sets the description for this gallery.
		/// </summary>
		/// <value>The description for this gallery.</value>
		public string Description
		{
			get { return _description; }
			set { _description = value; }
		}

		/// <summary>
		/// Gets or sets the date this gallery was created.
		/// </summary>
		/// <value>The date this gallery was created.</value>
		public DateTime CreationDate
		{
			get { return _creationDate; }
			set { _creationDate = value; }
		}

		/// <summary>
		/// Gets or sets the ID of the root album of this gallery.
		/// </summary>
		/// <value>The ID of the root album of this gallery</value>
		public int RootAlbumId
		{
			get
			{
				if (_rootAlbumId == int.MinValue)
				{
					// The root album is the item in the Albums dictionary with the most number of child albums.
					int maxCount = int.MinValue;
					foreach (KeyValuePair<int, List<int>> kvp in Albums)
					{
						if (kvp.Value.Count > maxCount)
						{
							maxCount = kvp.Value.Count;
							_rootAlbumId = kvp.Key;
						}
					}
				}
				return _rootAlbumId;
			}
			set { _rootAlbumId = value; }
		}

		/// <summary>
		/// Gets or sets a dictionary containing a list of album IDs (key) and the flattened list of
		/// all child album IDs below each album.
		/// </summary>
		/// <value>An instance of Dictionary&lt;int, List&lt;int&gt;&gt;.</value>
		public Dictionary<int, List<int>> Albums
		{
			get { return _albums; }
			set { _albums = value; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Gallery"/> class.
		/// </summary>
		public Gallery()
		{
			this._id = int.MinValue;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Creates a deep copy of this instance.
		/// </summary>
		/// <returns>Returns a deep copy of this instance.</returns>
		public IGallery Copy()
		{
			IGallery galleryCopy = new Gallery();

			galleryCopy.GalleryId = this.GalleryId;
			galleryCopy.Description = this.Description;
			galleryCopy.CreationDate = this.CreationDate;

			galleryCopy.Albums = new Dictionary<int, List<int>>(this.Albums.Count);
			foreach (KeyValuePair<int, List<int>> kvp in this.Albums)
			{
				galleryCopy.Albums.Add(kvp.Key, new List<int>(kvp.Value));
			}

			return galleryCopy;
		}

		/// <summary>
		/// Persist this gallery object to the data store.
		/// </summary>
		public void Save()
		{
			bool isNew = IsNew;

			_id = Factory.GetDataProvider().Gallery_Save(this);

			// For new galleries, configure it and then trigger the created event.
			if (isNew)
			{
				Configure();

				EventHandler<GalleryCreatedEventArgs> galleryCreated = GalleryCreated;
				if (galleryCreated != null)
				{
					galleryCreated(null, new GalleryCreatedEventArgs(GalleryId));
				}
			}

			Factory.ClearAllCaches();
		}

		/// <summary>
		/// Permanently delete the current gallery from the data store, including all related records. This action cannot
		/// be undone.
		/// </summary>
		public void Delete()
		{
			Factory.GetDataProvider().Gallery_Delete(this);

			Factory.ClearAllCaches();
		}

		/// <summary>
		/// Configure the gallery by verifying that a default set of
		/// records exist in the supporting tables (gs_Album, gs_GallerySetting, gs_MimeTypeGallery, gs_Synchronize, gs_Role_Album). 
		/// No changes are made to the file system as part of this operation. This method does not overwrite existing data, but it
		/// does insert missing data. This function can be used during application initialization to validate the data integrity for 
		/// a gallery. For example, if the user has added a record to the MIME types or template gallery settings tables, this method 
		/// will ensure that the new records are associated with this gallery.
		/// </summary>
		public void Configure()
		{
			Factory.GetDataProvider().Gallery_Configure(this);
		}

		#endregion

		#region IComparable Members

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
				IGallery other = obj as IGallery;
				if (other != null)
					return this.GalleryId.CompareTo(other.GalleryId);
				else
					return 1;
			}
		}

		#endregion
	}

	/// <summary>
	/// Provides data for the <see cref="Gallery.GalleryCreated" /> event.
	/// </summary>
	public class GalleryCreatedEventArgs : EventArgs
	{
		private readonly int _galleryId;

		/// <summary>
		/// Initializes a new instance of the <see cref="GalleryCreatedEventArgs"/> class.
		/// </summary>
		/// <param name="galleryId">The ID of the newly created gallery.</param>
		public GalleryCreatedEventArgs(int galleryId)
		{
			_galleryId = galleryId;
		}

		/// <summary>
		/// Gets the ID of the newly created gallery.
		/// </summary>
		/// <value>The gallery ID.</value>
		public int GalleryId
		{
			get { return _galleryId; }
		}
	}
}
