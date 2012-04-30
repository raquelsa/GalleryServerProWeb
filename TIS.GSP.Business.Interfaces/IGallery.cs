using System;
using System.Collections.Generic;

namespace GalleryServerPro.Business.Interfaces
{
	/// <summary>
	/// Represents a gallery within Gallery Server Pro.
	/// </summary>
	public interface IGallery
	{
		/// <summary>
		/// Gets or sets the unique identifier for this gallery.
		/// </summary>
		/// <value>The unique identifier for this gallery.</value>
		int GalleryId
		{
			get;
			set;
		}

		/// <summary>
		/// Gets a value indicating whether this object is new and has not yet been persisted to the data store.
		/// </summary>
		/// <value><c>true</c> if this instance is new; otherwise, <c>false</c>.</value>
		bool IsNew { get; }

		/// <summary>
		/// Gets or sets the description for this gallery.
		/// </summary>
		/// <value>The description for this gallery.</value>
		string Description
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the date this gallery was created.
		/// </summary>
		/// <value>The date this gallery was created.</value>
		DateTime CreationDate
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the ID of the root album of this gallery.
		/// </summary>
		/// <value>The ID of the root album of this gallery</value>
		int RootAlbumId
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a dictionary containing a list of album IDs (key) and the flattened list of
		/// all child album IDs below each album.
		/// </summary>
		/// <value>An instance of Dictionary&lt;int, List&lt;int&gt;&gt;.</value>
		Dictionary<int, List<int>> Albums
		{
			get;
			set;
		}

		/// <summary>
		/// Creates a deep copy of this instance.
		/// </summary>
		/// <returns>Returns a deep copy of this instance.</returns>
		IGallery Copy();

		/// <summary>
		/// Persist this gallery object to the data store.
		/// </summary>
		void Save();

		/// <summary>
		/// Permanently delete the current gallery from the data store, including all related records. This action cannot
		/// be undone.
		/// </summary>
		void Delete();

		/// <summary>
		/// Configure the gallery by verifying that a default set of
		/// records exist in the supporting tables (gs_Album, gs_GallerySetting, gs_MimeTypeGallery, gs_Synchronize, gs_Role_Album). 
		/// No changes are made to the file system as part of this operation. This method does not overwrite existing data, but it
		/// does insert missing data. This function can be used during application initialization to validate the data integrity for 
		/// a gallery. For example, if the user has added a record to the MIME types or template gallery settings tables, this method 
		/// will ensure that the new records are associated with this gallery.
		/// </summary>
		void Configure();
	}
}
