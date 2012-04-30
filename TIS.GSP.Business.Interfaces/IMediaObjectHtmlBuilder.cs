using System;
using System.Collections.Generic;
using System.Text;

namespace GalleryServerPro.Business.Interfaces
{
	/// <summary>
	/// Provides functionality for generating the HTML that can be sent to a client browser to render a
	/// particular media object. Objects implementing this interface use the HTML templates in the configuration
	/// file. Replaceable parameters in the template are indicated by the open and close brackets, such as 
	/// {Width}. These parameters are replaced with the relevant values.
	/// TODO: Add caching functionality to speed up the ability to generate HTML.
	/// </summary>
	public interface IMediaObjectHtmlBuilder
	{
		/// <summary>
		/// Gets the gallery ID.
		/// </summary>
		/// <value>The gallery ID.</value>
		int GalleryId
		{ 
			get;
		}

		/// <summary>
		/// Gets or sets the MIME type of this media object.
		/// </summary>
		/// <value>The MIME type of this media object.</value>
		IMimeType MimeType
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the list of CSS classes to apply to the HTML output of this media object. Each class must
		/// represent a valid, pre-existing CSS class that can be accessed by the client browser. The classes will
		/// be transformed into a space-delimited string and used to replace the {CssClasses} replacement parameter,
		/// if one exists, in the HTML template.
		/// </summary>
		/// <value>The list of CSS classes to apply to the HTML output of this media object.</value>
		System.Collections.Specialized.StringCollection CssClasses
		{
			get;
		}

		/// <summary>
		/// Gets or sets the physical path to this media object, including the object's name. Example:
		/// C:\Inetpub\wwwroot\galleryserverpro\mediaobjects\Summer 2005\sunsets\desert sunsets\sonorandesert.jpg
		/// </summary>
		/// <value>The physical path to this media object, including the object's name.</value>
		string MediaObjectPhysicalPath
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the width of this object, in pixels.
		/// </summary>
		/// <value>The width of this object, in pixels.</value>
		int Width
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the height of this object, in pixels.
		/// </summary>
		/// <value>The height of this object, in pixels.</value>
		int Height
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the title for this gallery object.
		/// </summary>
		/// <value>The title for this gallery object.</value>
		string Title
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether to automatically begin playing the media object as soon as
		/// possible in the client browser. This setting is applicable only to objects that can be played, such
		/// as audio and video files.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if Gallery Server Pro is to automatically begin playing the media object as soon as
		/// possible in the client browser; otherwise, <c>false</c>.
		/// </value>
		bool AutoStartMediaObject
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the unique identifier for this media object.
		/// </summary>
		/// <value>The unique identifier for this media object.</value>
		int MediaObjectId
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the unique identifier for album containing the media object.
		/// </summary>
		/// <value>The unique identifier for album containing the media object.</value>
		int AlbumId
		{
			get;
			set;
		}

		/// <summary>
		/// An <see cref="System.Array"/> of browser ids for the current browser. This is a list of strings
		/// that represent the various categories of browsers the current browser belongs to. This is typically populated by
		/// calling ToArray() on the Request.Browser.Browsers property.
		/// </summary>
		/// <value>
		/// The <see cref="System.Array"/> of browser ids for the current browser.
		/// </value>
		Array Browsers
		{
			get;
		}

		/// <summary>
		/// Gets or sets the type of the display object.
		/// </summary>
		/// <value>The display type.</value>
		DisplayObjectType DisplayType
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether the media object is marked as private. Private albums and media
		/// objects are hidden from anonymous (unauthenticated) users.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is private; otherwise, <c>false</c>.
		/// </value>
		bool IsPrivate
		{
			get;
			set;
		}

		/// <summary>
		/// Generate the HTML that can be sent to a browser to render the media object. If the configuration file
		/// does not specify an HTML template for this MIME type, an empty string is returned. If the media object is
		/// an image but cannot be displayed in a browser (such as TIF), then return an empty string.
		/// </summary>
		/// <returns>Returns a string of valid HTML that can be sent to a browser to render the media object, or an empty
		/// string if the media object cannot be displayed in a browser.</returns>
		/// <remarks>The HTML templates used to generate the HTML code in this method are stored in the Gallery 
		/// Server Pro configuration file, specifically at this location:
		/// GalleryServerPro/galleryObject/mediaObjects/mediaObject</remarks>
		string GenerateHtml();

		/// <summary>
		/// Generate the ECMA script (javascript) that can be sent to a browser to assist with rendering the media object. 
		/// If the configuration file does not specify a scriptOutput template for this MIME type, an empty string is returned.
		/// </summary>
		/// <returns>Returns the ECMA script (javascript) that can be sent to a browser to assist with rendering the media object.</returns>
		string GenerateScript();

		/// <summary>
		/// Generate the URL to the media object. For example, for images this url can be assigned to the src attribute of an img tag.
		/// (ex: /galleryserverpro/handler/getmediaobject.ashx?moid=34&amp;dt=1&amp;g=1)
		/// The query string parameter will be encrypted if that option is enabled.
		/// </summary>
		/// <returns>Gets the URL to the media object.</returns>
		string GenerateUrl();
	}
}
