using System;
using System.Globalization;
using System.IO;
using System.Web.UI;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.Web.Controller;
using GalleryServerPro.Web.Pages;

namespace GalleryServerPro.Web
{
	/// <summary>
	/// The top level user control that acts as a container for other user controls used in Gallery Server Pro.
	/// </summary>
	[ToolboxData("<{0}:Gallery runat=\"server\"></{0}:Gallery>")]
	public class Gallery : UserControl
	{
		#region Private Fields

		private string _controlId;
		private IGalleryControlSettings _galleryControlSettings;
		private Message _message = Message.None;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Gallery"/> class.
		/// </summary>
		public Gallery()
		{
			this.Init += Gallery_Init;
		}

		#endregion

		#region Event Handlers

		private void Gallery_Init(object sender, EventArgs e)
		{
			// Set up our "global" error handling. Since every page in GSP passes through this event handler, attaching error handling 
			// code to the page's Error event handler is roughly equivalent to the global error handling in web.config. We prefer to
			// set up our error handling this way so as not to interfere with the user's own error handling configuration she may be
			// using.
			this.Page.Error += Gallery_Error;

			// Check the query string for the desired page and add it to the page's controls.
			this.LoadRequestedPage();
		}

		private void Gallery_Error(object sender, EventArgs e)
		{
			// Grab a handle to the exception that is giving us grief.
			Exception ex = Server.GetLastError();

			if (Context != null)
			{
				ex = Context.Error;
			}

			if (ex != null)
			{
				try
				{
					AppErrorController.HandleGalleryException(ex, GalleryId);
				}
				catch {}
			}
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets a value that uniquely identifies this control. This value is used to identify its settings in the gallery control settings
		/// table. Example: "~/Default.aspx|gsp"
		/// </summary>
		/// <value>A value that uniquely identifies this control.</value>
		/// <remarks>We use an application-relative file path rather than a server-relative path. This allows an admin to move the 
		/// application around and not lose control settings, but this means that if multiple applications are using the same database, then
		/// we must take care to use unique web page names or unique IDs.</remarks>
		public string ControlId
		{
			get
			{
				if (String.IsNullOrEmpty(_controlId))
				{
					_controlId = String.Concat(System.Web.HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath, "|", this.ClientID);
				}

				return _controlId;
			}
		}

		/// <summary>
		/// Gets the gallery control settings for the current instance.
		/// </summary>
		/// <value>The gallery control settings for the current instance.</value>
		public IGalleryControlSettings GalleryControlSettings
		{
			get
			{
				if (_galleryControlSettings == null)
				{
					_galleryControlSettings = Factory.LoadGalleryControlSetting(ControlId);
				}

				return _galleryControlSettings;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether to render the header at the top of the gallery. If the property has not been explicitly 
		/// assigned a value, it returns the value of <see cref="IGalleryControlSettings.ShowHeader" />. Returns a null value if no gallery
		/// control setting exists. The header includes the gallery title, login/logout controls, user account management link, and search 
		/// function. The title, login/logout controls and search function can be individually controlled via the <see cref="GalleryTitle" />,
		/// <see cref="ShowLogin" /> and <see cref="ShowSearch" /> properties.
		/// </summary>
		/// <value><c>true</c> if the header is to be displayed; otherwise, <c>false</c>.</value>
		public bool? ShowHeader
		{
			get
			{
				return GalleryControlSettings.ShowHeader;
			}
			set
			{
				GalleryControlSettings.ShowHeader = value;
			}
		}

		/// <summary>
		/// Gets or sets the header text that appears at the top of each web page. If the property has not been explicitly assigned a
		/// value, it returns the value of <see cref="IGalleryControlSettings.GalleryTitle" />. Returns null if no gallery control 
		/// setting exists.
		/// </summary>
		/// <value>The gallery title.</value>
		public string GalleryTitle
		{
			get
			{
				return GalleryControlSettings.GalleryTitle;
			}
			set
			{
				GalleryControlSettings.GalleryTitle = value;
			}
		}

		/// <summary>
		/// Gets or sets the URL the user will be directed to when she clicks the gallery title. Optional. If not present, no link 
		/// will be rendered. Examples: "http://www.mysite.com", "/" (the root of the web site), "~/" (the top level album). If the 
		/// property has not been explicitly assigned a value, it returns the value of <see cref="IGalleryControlSettings.GalleryTitleUrl" />.
		/// Returns null if no gallery control setting exists.
		/// </summary>
		/// <value>The gallery title URL.</value>
		public string GalleryTitleUrl
		{
			get
			{
				return GalleryControlSettings.GalleryTitleUrl;
			}
			set
			{
				GalleryControlSettings.GalleryTitleUrl = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether to show the login controls at the top right of each page. When false, no login controls
		/// are shown, but the user can still navigate directly to the login page to log on. If the property has not been explicitly 
		/// assigned a value, it returns the value of <see cref="IGalleryControlSettings.ShowLogin" />. Returns a null value if no gallery
		/// control setting exists.
		/// </summary>
		/// <value><c>true</c> if login controls are visible; otherwise, <c>false</c>.</value>
		public bool? ShowLogin
		{
			get
			{
				return GalleryControlSettings.ShowLogin;
			}
			set
			{
				GalleryControlSettings.ShowLogin = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether to show the search box at the top right of each page. If the property has not been explicitly 
		/// assigned a value, it returns the value of <see cref="IGalleryControlSettings.ShowSearch" />. Returns a null value if no gallery
		/// control setting exists.
		/// </summary>
		/// <value><c>true</c> if the search box is visible; otherwise, <c>false</c>.</value>
		public bool? ShowSearch
		{
			get
			{
				return GalleryControlSettings.ShowSearch;
			}
			set
			{
				GalleryControlSettings.ShowSearch = value;
			}
		}

		/// <summary>
		/// Gets or sets the ID of the gallery to be displayed. When <see cref="AlbumId" /> or <see cref="MediaObjectId" /> is specified,
		/// or if an album or media object is specified in the query string, this property is overwritten with the gallery ID for the album 
		/// or media object. If a value is specified and no gallery exists, one is automatically created. Only one of these properties 
		/// should be set: <see cref="GalleryId" />, <see cref="AlbumId" />, <see cref="MediaObjectId" />.
		/// </summary>
		/// <value>The gallery ID.</value>
		public int GalleryId
		{
			get
			{
				if (GalleryControlSettings.GalleryId.HasValue)
				{
					return GalleryControlSettings.GalleryId.Value;
				}
				else
				{
					return int.MinValue;
				}
			}
			set
			{
				GalleryControlSettings.GalleryId = value;
			}
		}

		/// <summary>
		/// Gets or sets the ID of the album to be displayed. This setting can be used to specify that a particular album be displayed. When
		/// specified, the <see cref="GalleryId" /> is ignored. Only one of these properties should be set: <see cref="GalleryId" />, 
		/// <see cref="AlbumId" />, <see cref="MediaObjectId" />. Defaults to <see cref="Int32.MinValue" /> when not specified.
		/// </summary>
		/// <value>The album ID.</value>
		public int AlbumId
		{
			get
			{
				if (GalleryControlSettings.AlbumId.HasValue)
				{
					return GalleryControlSettings.AlbumId.Value;
				}
				else
				{
					return int.MinValue;
				}
			}
			set
			{
				GalleryControlSettings.AlbumId = value;
			}
		}

		/// <summary>
		/// Gets or sets the ID of the media object to be displayed. This setting can be used to specify that a particular media object be displayed. When
		/// specified, the <see cref="GalleryId" /> is ignored and the <see cref="ViewMode" /> is automatically set to ViewMode.Single. Only one of these 
		/// properties should be set: <see cref="GalleryId" />, <see cref="AlbumId" />, <see cref="MediaObjectId" />. Defaults to <see cref="Int32.MinValue" />
		/// when not specified.
		/// </summary>
		/// <value>The media object ID.</value>
		public int MediaObjectId
		{
			get
			{
				if (GalleryControlSettings.MediaObjectId.HasValue)
				{
					return GalleryControlSettings.MediaObjectId.Value;
				}
				else
				{
					return int.MinValue;
				}
			}
			set
			{
				GalleryControlSettings.MediaObjectId = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating how the media objects are to be rendered in the browser. The default value is ViewMode.Multiple. When the value is 
		/// ViewMode.Multiple, the contents of an album are shown as a set of thumbnail images. When set to ViewMode.Single, a single media object is 
		/// displayed. When set to ViewMode.SingleRandom, a single media object is displayed that is randomly selected. When a <see cref="MediaObjectId" /> is 
		/// specified, the <see cref="ViewMode" /> is automatically set to ViewMode.Single.
		/// </summary>
		/// <value>A value indicating how the media objects are to be rendered in the browser.</value>
		public ViewMode ViewMode
		{
			get
			{
				if (GalleryControlSettings.ViewMode != Business.ViewMode.NotSet)
				{
					return GalleryControlSettings.ViewMode;
				}
				else
				{
					return ViewMode.Multiple;
				}
			}
			set
			{
				GalleryControlSettings.ViewMode = value;
			}
		}

		/// <summary>
		/// Gets or sets the base URL to invoke when a tree node is clicked. Applies only when <see cref="ViewMode" /> = ViewMode.TreeView.
		/// The album ID of the selected album is passed to the URL as the query string parameter "aid".
		/// Example: "Gallery.aspx, http://site.com/gallery.aspx" If the property has not been explicitly assigned a value, it returns the 
		/// value of <see cref="IGalleryControlSettings.GalleryTitleUrl" />. Returns an empty string if no gallery control setting exists.
		/// </summary>
		public string TreeViewNavigateUrl
		{
			get
			{
				if (!String.IsNullOrEmpty(GalleryControlSettings.TreeViewNavigateUrl))
				{
					return GalleryControlSettings.TreeViewNavigateUrl;
				}
				else
				{
					return String.Empty;
				}
			}
			set
			{
				GalleryControlSettings.TreeViewNavigateUrl = value;
			}
		}

		/// <summary>
		/// Gets or sets the message used to display user messages, such as "No album authorized for user". Defaults to Message.None.
		/// </summary>
		/// <value>The message to display to the user.</value>
		public Message Message
		{
			get { return _message; }
			private set { _message = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether users can view galleries without logging in. When false, users are redirected to a login
		/// page when any album is requested. Private albums are never shown to anonymous users, even when this property is true. If the 
		/// property has not been explicitly assigned a value, it returns the value of <see cref="IGalleryControlSettings.AllowAnonymousBrowsing" />.
		/// Returns a null value if no gallery control setting exists.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if anonymous users can view the gallery; otherwise, <c>false</c>.
		/// </value>
		public bool? AllowAnonymousBrowsing
		{
			get
			{
				return GalleryControlSettings.AllowAnonymousBrowsing;
			}
			set
			{
				GalleryControlSettings.AllowAnonymousBrowsing = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether to render a treeview of albums along the left side when an album is being displayed.
		/// If the property has not been explicitly assigned a value, it returns the value of 
		/// <see cref="IGalleryControlSettings.ShowAlbumTreeViewForAlbum" />. Returns a null value if no gallery control setting exists.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the treeview is to be rendered; otherwise, <c>false</c>.
		/// </value>
		public bool? ShowAlbumTreeViewForAlbum
		{
			get
			{
				return GalleryControlSettings.ShowAlbumTreeViewForAlbum;
			}
			set
			{
				GalleryControlSettings.ShowAlbumTreeViewForAlbum = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether to render a treeview of albums along the left side when a single media object is
		/// being displayed. If the property has not been explicitly assigned a value, it returns the value of 
		/// <see cref="IGalleryControlSettings.ShowAlbumTreeViewForMediaObject" />. Returns a null value if no gallery control setting exists.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the treeview is to be rendered; otherwise, <c>false</c>.
		/// </value>
		public bool? ShowAlbumTreeViewForMediaObject
		{
			get
			{
				return GalleryControlSettings.ShowAlbumTreeViewForMediaObject;
			}
			set
			{
				GalleryControlSettings.ShowAlbumTreeViewForMediaObject = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether to render the Actions menu. If the property has not been explicitly assigned a value, 
		/// it returns the value of <see cref="IGalleryControlSettings.ShowActionMenu" />. Returns a null value if no gallery control setting exists.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the Actions menu is to be rendered; otherwise, <c>false</c>.
		/// </value>
		public bool? ShowActionMenu
		{
			get
			{
				return GalleryControlSettings.ShowActionMenu;
			}
			set
			{
				GalleryControlSettings.ShowActionMenu = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether to render the album bread crumb links, including the Actions menu. If the 
		/// property has not been explicitly assigned a value, it returns the value of <see cref="IGalleryControlSettings.ShowAlbumBreadCrumb" />.
		/// Returns a null value if no gallery control setting exists.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the album bread crumb links are to be visible; otherwise, <c>false</c>.
		/// </value>
		public bool? ShowAlbumBreadCrumb
		{
			get
			{
				return GalleryControlSettings.ShowAlbumBreadCrumb;
			}
			set
			{
				GalleryControlSettings.ShowAlbumBreadCrumb = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the toolbar is rendered above individual media objects. If the 
		/// property has not been explicitly assigned a value, it returns the value of <see cref="IGalleryControlSettings.ShowMediaObjectToolbar" />.
		/// Returns a null value if no gallery control setting exists.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the toolbar is rendered above individual media objects; otherwise, <c>false</c>.
		/// </value>
		public bool? ShowMediaObjectToolbar
		{
			get
			{
				return GalleryControlSettings.ShowMediaObjectToolbar;
			}
			set
			{
				GalleryControlSettings.ShowMediaObjectToolbar = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the title is displayed beneath individual media objects. If the 
		/// property has not been explicitly assigned a value, it returns the value of <see cref="IGalleryControlSettings.ShowMediaObjectTitle" />.
		/// Returns a null value if no gallery control setting exists.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the title is displayed beneath individual media objects; otherwise, <c>false</c>.
		/// </value>
		public bool? ShowMediaObjectTitle
		{
			get
			{
				return GalleryControlSettings.ShowMediaObjectTitle;
			}
			set
			{
				GalleryControlSettings.ShowMediaObjectTitle = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the next and previous buttons are rendered for individual media objects. If the 
		/// property has not been explicitly assigned a value, it returns the value of <see cref="IGalleryControlSettings.ShowMediaObjectNavigation" />.
		/// Returns a null value if no gallery control setting exists.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the next and previous buttons are rendered for individual media objects; otherwise, <c>false</c>.
		/// </value>
		public bool? ShowMediaObjectNavigation
		{
			get
			{
				return GalleryControlSettings.ShowMediaObjectNavigation;
			}
			set
			{
				GalleryControlSettings.ShowMediaObjectNavigation = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether to display the relative position of a media object within an album (example: (3 of 24)). 
		/// Applicable only when a single media object is displayed. If the property has not been explicitly assigned a value, it returns 
		/// the value of <see cref="IGalleryControlSettings.ShowMediaObjectIndexPosition" />. Returns a null value if no gallery control 
		/// setting exists.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the relative position of a media object within an album is to be rendered; otherwise, <c>false</c>.
		/// </value>
		public bool? ShowMediaObjectIndexPosition
		{
			get
			{
				return GalleryControlSettings.ShowMediaObjectIndexPosition;
			}
			set
			{
				GalleryControlSettings.ShowMediaObjectIndexPosition = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the show metadata button is visible above a media object. If the 
		/// property has not been explicitly assigned a value, it returns the value of <see cref="IGalleryControlSettings.ShowMetadataButton" />.
		/// Returns a null value if no gallery control setting exists. When <see cref="ShowMediaObjectToolbar" />=<c>false</c>, this property is ignored.
		/// </summary>
		/// <value><c>true</c> if the show metadata button is visible above a media object; otherwise, <c>false</c>.</value>
		public bool? ShowMetadataButton
		{
			get
			{
				return GalleryControlSettings.ShowMetadataButton;
			}
			set
			{
				GalleryControlSettings.ShowMetadataButton = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the download media object button is visible above a media object. If the 
		/// property has not been explicitly assigned a value, it returns the value of <see cref="IGalleryControlSettings.ShowMediaObjectDownloadButton" />.
		/// Returns a null value if no gallery control setting exists. When <see cref="ShowMediaObjectToolbar" />=<c>false</c>, this property is ignored.
		/// </summary>
		/// <value><c>true</c> if the download media object button is visible above a media object; otherwise, <c>false</c>.</value>
		public bool? ShowMediaObjectDownloadButton
		{
			get
			{
				return GalleryControlSettings.ShowMediaObjectDownloadButton;
			}
			set
			{
				GalleryControlSettings.ShowMediaObjectDownloadButton = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the download ZIP archive button is visible above a media object. If the 
		/// property has not been explicitly assigned a value, it returns the value of <see cref="IGalleryControlSettings.ShowMediaObjectZipDownloadButton" />.
		/// Returns a null value if no gallery control setting exists. When <see cref="ShowMediaObjectToolbar" />=<c>false</c>, this property is ignored.
		/// </summary>
		/// <value><c>true</c> if the download ZIP archive button is visible above a media object; otherwise, <c>false</c>.</value>
		public bool? ShowMediaObjectZipDownloadButton
		{
			get
			{
				return GalleryControlSettings.ShowMediaObjectZipDownloadButton;
			}
			set
			{
				GalleryControlSettings.ShowMediaObjectZipDownloadButton = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the show high resolution image button is visible above a media object.  This property
		/// is ignored if the current user is not allowed to view the high-res image (due to <see cref="IGallerySettings.AllowAnonymousHiResViewing" />
		/// =<c>false</c> for anonymous users or a logged-on user not being in a role with <see cref="IGalleryServerRole.AllowViewOriginalImage" /> 
		/// permission). If the image does not have a high-resolution version, the button is not shown, even if this property is <c>true</c>. If the 
		/// property has not been explicitly assigned a value, it returns the value of <see cref="IGalleryControlSettings.ShowHighResImageButton" />.
		/// Returns a null value if no gallery control setting exists. When <see cref="ShowMediaObjectToolbar" />=<c>false</c>, this property is ignored.
		/// </summary>
		/// <value><c>true</c> if the show high resolution image button is visible above a media object; otherwise, <c>false</c>.</value>
		public bool? ShowHighResImageButton
		{
			get
			{
				return GalleryControlSettings.ShowHighResImageButton;
			}
			set
			{
				GalleryControlSettings.ShowHighResImageButton = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the show permalink button is visible above a media object. If the 
		/// property has not been explicitly assigned a value, it returns the value of <see cref="IGalleryControlSettings.ShowPermalinkButton" />.
		/// Returns a null value if no gallery control setting exists. When <see cref="ShowMediaObjectToolbar" />=<c>false</c>, this property is ignored.
		/// </summary>
		/// <value><c>true</c> if the show permalink button is visible above a media object; otherwise, <c>false</c>.</value>
		public bool? ShowPermalinkButton
		{
			get
			{
				return GalleryControlSettings.ShowPermalinkButton;
			}
			set
			{
				GalleryControlSettings.ShowPermalinkButton = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the play/pause slide show button is visible above a media object. If the 
		/// property has not been explicitly assigned a value, it returns the value of <see cref="IGalleryControlSettings.ShowSlideShowButton" />.
		/// Returns a null value if no gallery control setting exists. When <see cref="ShowMediaObjectToolbar" />=<c>false</c>, this property is ignored.
		/// </summary>
		/// <value><c>true</c> if the play/pause slide show button is visible above a media object; otherwise, <c>false</c>.</value>
		public bool? ShowSlideShowButton
		{
			get
			{
				return GalleryControlSettings.ShowSlideShowButton;
			}
			set
			{
				GalleryControlSettings.ShowSlideShowButton = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the transfer media object button is visible above a media object. The button is not
		/// shown if the current user does not have permission to move media objects, even if this property is <c>true</c>. If the 
		/// property has not been explicitly assigned a value, it returns the value of <see cref="IGalleryControlSettings.ShowTransferMediaObjectButton" />.
		/// Returns a null value if no gallery control setting exists. When <see cref="ShowMediaObjectToolbar" />=<c>false</c>, this property is ignored.
		/// </summary>
		/// <value><c>true</c> if the transfer media object button is visible above a media object; otherwise, <c>false</c>.</value>
		public bool? ShowTransferMediaObjectButton
		{
			get
			{
				return GalleryControlSettings.ShowTransferMediaObjectButton;
			}
			set
			{
				GalleryControlSettings.ShowTransferMediaObjectButton = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the copy media object button is visible above a media object. The button is not
		/// shown if the current user does not have permission to copy media objects, even if this property is <c>true</c>. If the 
		/// property has not been explicitly assigned a value, it returns the value of <see cref="IGalleryControlSettings.ShowCopyMediaObjectButton" />.
		/// Returns a null value if no gallery control setting exists. When <see cref="ShowMediaObjectToolbar" />=<c>false</c>, this property is ignored.
		/// </summary>
		/// <value><c>true</c> if the copy media object button is visible above a media object; otherwise, <c>false</c>.</value>
		public bool? ShowCopyMediaObjectButton
		{
			get
			{
				return GalleryControlSettings.ShowCopyMediaObjectButton;
			}
			set
			{
				GalleryControlSettings.ShowCopyMediaObjectButton = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the rotate media object button is visible above a media object. The button is not
		/// shown if the current user does not have permission to rotate media objects, even if this property is <c>true</c>. If the 
		/// property has not been explicitly assigned a value, it returns the value of <see cref="IGalleryControlSettings.ShowRotateMediaObjectButton" />.
		/// Returns a null value if no gallery control setting exists. When <see cref="ShowMediaObjectToolbar" />=<c>false</c>, this property is ignored.
		/// </summary>
		/// <value><c>true</c> if the rotate media object button is visible above a media object; otherwise, <c>false</c>.</value>
		public bool? ShowRotateMediaObjectButton
		{
			get
			{
				return GalleryControlSettings.ShowRotateMediaObjectButton;
			}
			set
			{
				GalleryControlSettings.ShowRotateMediaObjectButton = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the delete media object button is visible above a media object. The button is not
		/// shown if the current user does not have permission to delete media objects, even if this property is <c>true</c>. If the 
		/// property has not been explicitly assigned a value, it returns the value of <see cref="IGalleryControlSettings.ShowDeleteMediaObjectButton" />.
		/// Returns a null value if no gallery control setting exists. When <see cref="ShowMediaObjectToolbar" />=<c>false</c>, this property is ignored.
		/// </summary>
		/// <value><c>true</c> if the delete media object button is visible above a media object; otherwise, <c>false</c>.</value>
		public bool? ShowDeleteMediaObjectButton
		{
			get
			{
				return GalleryControlSettings.ShowDeleteMediaObjectButton;
			}
			set
			{
				GalleryControlSettings.ShowDeleteMediaObjectButton = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether a slide show of media objects automatically starts playing when the page loads. This setting 
		/// applies only when the <see cref="ViewMode" /> is set to ViewMode.Single or ViewMode.SingleRandom and either an album or media object 
		/// is specified (that is, the <see cref="AlbumId" /> or <see cref="MediaObjectId" /> is assigned a value). If a media object is 
		/// specified, all images in the object's album will be shown in the slide show. If the property has not been explicitly assigned a value, 
		/// it returns the value of <see cref="IGalleryControlSettings.AutoPlaySlideShow" />. Returns a null value if no gallery control setting exists.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if a slide show of media objects will automatically start playing; otherwise, <c>false</c>.
		/// </value>
		public bool? AutoPlaySlideShow
		{
			get
			{
				return GalleryControlSettings.AutoPlaySlideShow;
			}
			set
			{
				GalleryControlSettings.AutoPlaySlideShow = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether an album or media object specified in the URL can override the <see cref="GalleryId" />,
		/// <see cref="AlbumId" />, and <see cref="MediaObjectId" /> properties of this control. Use the query string parameter "aid" to 
		/// specify an album; use "moid" for a media object (example: default.aspx?aid=12 for album ID=12, default.aspx?moid=37 for media
		/// object ID=37). If the property has not been explicitly assigned a value, it returns the value of 
		/// <see cref="IGalleryControlSettings.AllowUrlOverride" />. Returns a default value of <c>true</c> if no gallery control setting exists.
		/// </summary>
		/// <value><c>true</c> if an album or media object specified in the query string can override one specified as a control property; otherwise,
		///  <c>false</c>.</value>
		public bool AllowUrlOverride
		{
			get
			{
				if (GalleryControlSettings.AllowUrlOverride.HasValue)
				{
					return GalleryControlSettings.AllowUrlOverride.Value;
				}
				else
				{
					return true; // Default to true if not specified
				}
			}
			set
			{
				GalleryControlSettings.AllowUrlOverride = value;
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Load the requested page and add it to the current <see cref="UserControl.Controls" /> collection.
		/// </summary>
		private void LoadRequestedPage()
		{
			PageId page = GetPageFromQueryString();

			if ((page == PageId.install) || (page == PageId.upgrade))
			{
				// We don't want the error handler to fire for install or upgrade scenarios. Since it talks to the database, which often
				// isn't working on these pages, it might throw an error, which masks the "real" error.
				this.Page.Error -= Gallery_Error;
			}

			string src = GetUserControlPath(page);

			try
			{
				Control control = LoadControl(src); // This will fire GalleryController.InitializeGspApplication() for pages that implement <see cref="GalleryPage" />.

				// If the control is an instance of Pages.GalleryPage, then assign its GalleryControl property to the current instance.
				// This gives the control convenient access to the Page property.
				GalleryPage galleryControl = control as GalleryPage;
				if (galleryControl != null)
				{
					galleryControl.GalleryControl = this;
					galleryControl.PageId = page;

					// Verify we loaded the right control. The initial decision was based on the query string, but there are other
					// configuration settings that can override it.
					control = ValidateRequestedPage(galleryControl, page);
				}

				this.Controls.Add(control);
			}
			catch (FileNotFoundException)
			{
				throw new ErrorHandler.CustomExceptions.WebException(String.Format(CultureInfo.InvariantCulture, Resources.GalleryServerPro.Error_Cannot_Load_User_Control_Ex_Msg, src));
			}
		}

		/// <summary>
		/// Verify the user control we created is the right one, correcting it if necessary. When our initial guess proves correct, this function
		/// returns the same instance that is passed in. If required, replace the <paramref name="galleryControl" /> parameter with the correct one, 
		/// and return its base <see cref="Control" /> class.
		/// </summary>
		/// <param name="galleryControl">The gallery control to validate.</param>
		/// <param name="page">The requested page as determined by an analysis of the query string.</param>
		/// <returns>Returns a <see cref="Control" /> that is correct for the current configuration. May be the same control that is passed in.</returns>
		/// <remarks>We initially determined which page to load by looking at the query string, but the developer may have overridden this by setting
		/// various properties on this control. For example, she may have specified a media object on the <see cref="Gallery.MediaObjectId" /> property, 
		/// or she may have also specified a <see cref="Gallery.AlbumId" /> combined with <see cref="Gallery.ViewMode" /> = Single or SingleRandom, in
		/// which case we need to display a particular media object rather than album thumbnails. If this situation is detected, replace the 
		/// <paramref name="galleryControl" /> parameter with the correct one, and return its base <see cref="Control" /> class. If you are wondering 
		/// why we didn't just determine the right page the first time, that is because we can't invoke <see cref="GalleryPage.GetMediaObjectId" /> until 
		/// after <see cref="Utils.InitializeApplication" /> fires, which won't happen until after a user control has been loaded. So we make our best guess
		/// by looking at the query string and correcting it here if necessary. Our initial guess should be right more than 99% of the time.</remarks>
		private Control ValidateRequestedPage(GalleryPage galleryControl, PageId page)
		{
			Control control = galleryControl;

			if (((page == PageId.album) || (page == PageId.mediaobject)) && (ViewMode == ViewMode.TreeView))
			{
				// We need to render in treeview mode (pages/albumtreeview.ascx).
				return ConfigureTreeViewControl();
			}

			if ((page == PageId.album) && (galleryControl.GetMediaObjectId() > int.MinValue))
			{
				// We need to render in single media object mode (pages/mediaobject.ascx).
				control = ConfigureMediaObjectControl();
			}

			if ((page == PageId.mediaobject) && (AllowUrlOverride == false) && (AlbumId > int.MinValue) && (ViewMode == ViewMode.Multiple))
			{
				// We need to render in album thumbnail mode (pages/album.ascx).
				control = ConfigureAlbumThumbnailControl();
			}

			return control;
		}

		/// <summary>
		/// Configures and attach the treeview page to this instance.
		/// </summary>
		/// <returns>Returns an instance of <see cref="albumtreeview" />.</returns>
		private Control ConfigureTreeViewControl()
		{
			const PageId page = PageId.albumtreeview;

			string src = GetUserControlPath(page);
			Control control = LoadControl(src);

			GalleryPage galleryControl = control as GalleryPage;

			if (galleryControl != null)
			{
				galleryControl.GalleryControl = this;
				galleryControl.PageId = page;
			}

			if (AlbumId > int.MinValue)
			{
				albumtreeview albumTreeviewcontrol = control as albumtreeview;

				if (albumTreeviewcontrol != null)
				{
					albumTreeviewcontrol.RootAlbumId = AlbumId;
				}
			}

			return control;
		}

		/// <summary>
		/// Configures and attach the media object page to this instance.
		/// </summary>
		/// <returns>Returns an instance of <see cref="mediaobject" />.</returns>
		private Control ConfigureMediaObjectControl()
		{
			const PageId page = PageId.mediaobject;
			string src = GetUserControlPath(page);
			Control control = LoadControl(src);

			GalleryPage galleryControl = control as GalleryPage;

			if (galleryControl != null)
			{
				galleryControl.GalleryControl = this;
				galleryControl.PageId = page;
			}

			return control;
		}

		/// <summary>
		/// Configures and attach the album thumbnail page to this instance.
		/// </summary>
		/// <returns>Returns an instance of <see cref="album" />.</returns>
		private Control ConfigureAlbumThumbnailControl()
		{
			const PageId page = PageId.album;
			string src = GetUserControlPath(page);
			Control control = LoadControl(src);

			GalleryPage galleryControl = control as GalleryPage;

			if (galleryControl != null)
			{
				galleryControl.GalleryControl = this;
				galleryControl.PageId = page;
			}

			return control;
		}

		/// <summary>
		/// Gets the value that identifies the type of gallery page that is currently being displayed. This value is 
		/// retrieved from the "g" query string parameter. If the parameter is not present, the query string is searched
		/// for a "moid" parameter. If "moid" is found, the page is <see cref="Web.PageId.mediaobject"/>. If
		/// not found, the page is <see cref="Web.PageId.album"/>.
		/// </summary>
		/// <returns>Returns the value that identifies the type of gallery page that is currently being displayed.</returns>
		private static PageId GetPageFromQueryString()
		{
			PageId page;
			string requestedPage = Utils.GetQueryStringParameterString("g");

			if (String.IsNullOrEmpty(requestedPage))
			{
				// No 'g' query string parm. Look for 'moid' parameter, which might be present without the 'g' parm. Default
				// to album if 'moid' parameter is missing.
				if (Utils.GetQueryStringParameterInt32("moid") > int.MinValue)
					page = PageId.mediaobject;
				else
					page = PageId.album;
			}
			else
			{
				try
				{
					page = (PageId)Enum.Parse(typeof(PageId), requestedPage, true);
				}
				catch (ArgumentException)
				{
					page = PageId.album;
				}
			}
			return page;
		}

		private static string GetUserControlPath(PageId page)
		{
			string src = String.Concat(Utils.GalleryRoot, "/pages/", page, ".ascx");

			if (src.IndexOf("/admin_", StringComparison.Ordinal) >= 0)
				src = src.Replace("/admin_", "/admin/");
			if (src.IndexOf("/error", StringComparison.Ordinal) >= 0)
				src = src.Replace("/error_", "/error/");
			if (src.IndexOf("/task_", StringComparison.Ordinal) >= 0)
				src = src.Replace("/task_", "/task/");

			return src;
		}

		#endregion
	}
}