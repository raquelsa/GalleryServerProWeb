using System;

namespace GalleryServerPro.Web.Entity
{
	/// <summary>
	/// A simple object that contains media object information. This class is used to pass information between the browser and the web server
	/// via AJAX callbacks.
	/// </summary>
	public class MediaObjectWebEntity
	{
		// Stats about the current media object
		/// <summary>
		/// The media object ID.
		/// </summary>
		public int Id { get; set; }
		/// <summary>
		/// Specifies the zero-based index of this media object among the others in the containing album.
		/// The first media object in an album has index = 0.
		/// </summary>
		public int Index { get; set; }
		/// <summary>
		/// The number of media objects in this album.
		/// </summary>
		public int NumObjectsInAlbum { get; set; }
		/// <summary>
		/// The media object title.
		/// </summary>
		public string Title { get; set; }
		/// <summary>
		/// The HTML fragment that renders this media object.
		/// </summary>
		public string HtmlOutput { get; set; }
		/// <summary>
		/// The ECMA script fragment that renders this media object.
		/// </summary>
		public string ScriptOutput { get; set; }
		/// <summary>
		/// The width, in pixels, of this media object.
		/// </summary>
		public int Width { get; set; }
		/// <summary>
		/// The height, in pixels, of this media object.
		/// </summary>
		public int Height { get; set; }
		/// <summary>
		/// Indicates whether a high resolution version of this image exists and is available for viewing.
		/// </summary>
		public bool HiResAvailable { get; set; }
		/// <summary>
		/// Indicates whether a downloadable version of this media object exists and can be downloaded. External media objects
		/// cannot be downloaded.
		/// </summary>
		public bool IsDownloadable { get; set; }

		// Stats about the previous media object
		/// <summary>
		/// The ID of the previous media object. Specify zero if the current media object is the first item in this album.
		/// </summary>
		public int PrevId { get; set; }
		/// <summary>
		/// The title of the previous media object.
		/// </summary>
		public string PrevTitle { get; set; }
		/// <summary>
		/// The HTML fragment that renders the previous media object.
		/// </summary>
		public string PrevHtmlOutput { get; set; }
		/// <summary>
		/// The ECMA script fragment that renders the previous media object.
		/// </summary>
		public string PrevScriptOutput { get; set; }
		/// <summary>
		/// The width, in pixels, of the previous media object.
		/// </summary>
		public int PrevWidth { get; set; }
		/// <summary>
		/// The height, in pixels, of the previous media object.
		/// </summary>
		public int PrevHeight { get; set; }
		/// <summary>
		/// Indicates whether a high resolution version of the previous media object exists and is available for viewing.
		/// </summary>
		public bool PrevHiResAvailable { get; set; }
		/// <summary>
		/// Indicates whether a downloadable version of the previous media object exists and can be downloaded. External media objects
		/// cannot be downloaded.
		/// </summary>
		public bool PrevIsDownloadable { get; set; }

		// Stats about the next media object
		/// <summary>
		/// The ID of the next media object. Specify zero if the current media object is the last item in this album.
		/// </summary>
		public int NextId { get; set; }
		/// <summary>
		/// The title of the next media object.
		/// </summary>
		public string NextTitle { get; set; }
		/// <summary>
		/// The HTML fragment that renders the next media object.
		/// </summary>
		public string NextHtmlOutput { get; set; }
		/// <summary>
		/// The ECMA script fragment that renders the next media object.
		/// </summary>
		public string NextScriptOutput { get; set; }
		/// <summary>
		/// The width, in pixels, of the next media object.
		/// </summary>
		public int NextWidth { get; set; }
		/// <summary>
		/// The height, in pixels, of the next media object.
		/// </summary>
		public int NextHeight { get; set; }
		/// <summary>
		/// Indicates whether a high resolution version of the next media object exists and is available for viewing.
		/// </summary>
		public bool NextHiResAvailable { get; set; }
		/// Indicates whether a downloadable version of the next media object exists and can be downloaded. External media objects
		/// cannot be downloaded.
		/// </summary>
		public bool NextIsDownloadable { get; set; }

		// Stats about the next media object in a slide show. Slide shows skip over non-image objects, so the
		// next media object in a slide show may or may not be the same as the next media object.
		/// <summary>
		/// The ID of the next media object in a slide show.
		/// </summary>
		public int NextSSId { get; set; }
		/// <summary>
		/// Specifies the zero-based index of the next media object that appears in a slide show.
		/// </summary>
		public int NextSSIndex { get; set; }
		/// <summary>
		/// The title of the next media object in a slide show.
		/// </summary>
		public string NextSSTitle { get; set; }
		/// <summary>
		/// The URL that points directly to the next media object in a slide show. For example, for images this 
		/// URL can be assigned to the src attribute of an img tag.
		/// </summary>
		public string NextSSUrl { get; set; }
		/// <summary>
		/// The HTML fragment that renders the next media object in a slide show.
		/// </summary>
		public string NextSSHtmlOutput { get; set; }
		/// <summary>
		/// The ECMA script fragment that renders the next media object in a slide show.
		/// </summary>
		public string NextSSScriptOutput { get; set; }
		/// <summary>
		/// The width, in pixels, of the next media object in a slide show.
		/// </summary>
		public int NextSSWidth { get; set; }
		/// <summary>
		/// The height, in pixels, of the next media object in a slide show.
		/// </summary>
		public int NextSSHeight { get; set; }
		/// <summary>
		/// Indicates whether a high resolution version of the next media object in the slide show exists and is available for viewing.
		/// </summary>
		public bool NextSSHiResAvailable { get; set; }
		/// <summary>
		/// Indicates whether a downloadable version of the next media object in a slide show exists and can be downloaded. 
		/// External media objects cannot be downloaded, but images, which are the only media objects allowed in a slide show,
		/// are always downloadable. Therefore this property is always true, but I added it anyway for consistency in this
		/// object and to allow for the possibility of future enhancement that might modify this behavior.
		/// </summary>
		public bool NextSSIsDownloadable { get; set; }
	}
}
