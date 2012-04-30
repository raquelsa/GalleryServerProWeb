namespace GalleryServerPro.Business.Interfaces
{
	/// <summary>
	/// Represents a browser template within Gallery Server Pro. A browser template describes the HTML and javascript that is used
	/// to render a media object in a particular browser.
	/// </summary>
	public interface IBrowserTemplate
	{
		/// <summary>
		/// Gets or sets the identifier of a browser as specified in the .Net Framework's browser definition file. Every MIME type must
		/// have one browser template where <see cref="BrowserId" /> = "default". Additional <see cref="IBrowserTemplate" /> objects
		/// may represent a more specific browser or browser family, such as Internet Explorer (<see cref="BrowserId" /> = "ie").
		/// </summary>
		/// <value>The identifier of a browser as specified in the .Net Framework's browser definition file.</value>
		string BrowserId
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the HTML template to use to render a media object in a web browser.
		/// </summary>
		/// <value>The HTML template to use to render a media object in a web browser.</value>
		string HtmlTemplate
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the javascript template to use when rendering a media object in a web browser.
		/// </summary>
		/// <value>The javascript template to use when rendering a media object in a web browser.</value>
		string ScriptTemplate
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the MIME type this browser template applies to. Examples: image/*, video/*, video/quicktime, application/pdf.
		/// Notice that an asterisk (*) can be used to represent all subtypes within a type (e.g. "video/*" matches all videos).
		/// </summary>
		/// <value>The MIME type this browser template applies to.</value>
		string MimeType
		{
			get;
			set;
		}

		/// <summary>
		/// Creates a deep copy of this instance.
		/// </summary>
		/// <returns>Returns a deep copy of this instance.</returns>
		IBrowserTemplate Copy();
	}

}
