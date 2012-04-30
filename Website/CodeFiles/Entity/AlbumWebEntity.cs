using System;

namespace GalleryServerPro.Web.Entity
{
	/// <summary>
	/// A simple object that contains album information. This class is used to pass information between the browser and the web server
	/// via AJAX callbacks.
	/// </summary>
	public class AlbumWebEntity
	{
		/// <summary>
		/// The album ID.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The album title.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// The album summary.
		/// </summary>
		public string Summary { get; set; }

		/// <summary>
		/// The album owner.
		/// </summary>
		public string Owner { get; set; }

		/// <summary>
		/// The starting date of this album.
		/// </summary>
		public DateTime DateStart { get; set; }

		/// <summary>
		/// The ending date of this album.
		/// </summary>
		public DateTime DateEnd { get; set; }

		/// <summary>
		/// Indicates whether this album is hidden from anonymous users.
		/// </summary>
		public bool IsPrivate { get; set; }
	}
}

