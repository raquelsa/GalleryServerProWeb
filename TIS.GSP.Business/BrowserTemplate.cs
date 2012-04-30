using System.Diagnostics;
using GalleryServerPro.Business.Interfaces;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// Represents a browser template within Gallery Server Pro. A browser template describes the HTML and javascript that is used
	/// to render a media object in a particular browser.
	/// </summary>
	[DebuggerDisplay("{_mimeType}, Browser ID = {_browserId})")]
	public class BrowserTemplate : IBrowserTemplate
	{
		#region Private Fields

		private string _browserId;
		private string _htmlTemplate;
		private string _scriptTemplate;
		private string _mimeType;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="BrowserTemplate"/> class.
		/// </summary>
		internal BrowserTemplate()
		{
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets the identifier of a browser as specified in the .Net Framework's browser definition file. Every MIME type must
		/// have one browser template where <see cref="IBrowserTemplate.BrowserId" /> = "default". Additional <see cref="IBrowserTemplate" /> objects
		/// may represent a more specific browser or browser family, such as Internet Explorer (<see cref="IBrowserTemplate.BrowserId" /> = "ie").
		/// </summary>
		/// <value>The identifier of a browser as specified in the .Net Framework's browser definition file.</value>
		public string BrowserId
		{
			get { return _browserId; }
			set { _browserId = value; }
		}

		/// <summary>
		/// Gets or sets the HTML template to use to render a media object in a web browser.
		/// </summary>
		/// <value>The HTML template to use to render a media object in a web browser.</value>
		public string HtmlTemplate
		{
			get { return _htmlTemplate; }
			set { _htmlTemplate = value; }
		}

		/// <summary>
		/// Gets or sets the javascript template to use when rendering a media object in a web browser.
		/// </summary>
		/// <value>The javascript template to use when rendering a media object in a web browser.</value>
		public string ScriptTemplate
		{
			get { return _scriptTemplate; }
			set { _scriptTemplate = value; }
		}

		/// <summary>
		/// Gets or sets the MIME type this browser template applies to. Examples: image/*, video/*, video/quicktime, application/pdf.
		/// Notice that an asterisk (*) can be used to represent all subtypes within a type (e.g. "video/*" matches all videos).
		/// </summary>
		/// <value>The MIME type this browser template applies to.</value>
		public string MimeType
		{
			get { return _mimeType; }
			set { _mimeType = value; }
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Creates a deep copy of this instance.
		/// </summary>
		/// <returns>Returns a deep copy of this instance.</returns>
		public IBrowserTemplate Copy()
		{
			IBrowserTemplate bp = new BrowserTemplate();

			bp.BrowserId = this.BrowserId;
			bp.HtmlTemplate = this.HtmlTemplate;
			bp.ScriptTemplate = this.ScriptTemplate;
			bp.MimeType = this.MimeType;

			return bp;
		}

		#endregion
	}
}
