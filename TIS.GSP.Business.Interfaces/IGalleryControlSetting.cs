namespace GalleryServerPro.Business.Interfaces
{
	/// <summary>
	/// Represents a set of settings for a specific instance of a Gallery control.
	/// </summary>
	public interface IGalleryControlSettings
	{
		/// <summary>
		/// Gets or sets the ID for the gallery control setting.
		/// </summary>
		/// <value>The gallery control setting ID.</value>
		int GalleryControlSettingId { get; set; }

		/// <summary>
		/// Gets or sets the value that uniquely identifies the Gallery control. This is a concatenation of the relative
		/// path to the control and its client ID. For example: "\default.aspx|gsp"
		/// </summary>
		/// <value>The value that uniquely identifies the Gallery control.</value>
		string ControlId { get; set; }

		/// <summary>
		/// Gets or sets the ID of the gallery associated with the control.
		/// </summary>
		/// <value>The gallery ID.</value>
		int? GalleryId { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to render the header at the top of the gallery. If not specified, the application 
		/// uses <see cref="IGallerySettings.ShowHeader" />; when specified, this property overrides it. The header includes the 
		/// gallery title, login/logout controls, and search function. The login/logout controls and search function can be individually 
		/// controlled via the <see cref="ShowLogin" /> and <see cref="ShowSearch" /> properties.
		/// </summary>
		/// <value><c>true</c> if the header is to be dislayed; otherwise, <c>false</c>.</value>
		bool? ShowHeader { get; set; }

		/// <summary>
		/// Gets or sets the header text that appears at the top of each web page. Requires that <see cref="ShowHeader" /> be set to
		/// <c>true</c> in order to be visible. If not specified, the application uses <see cref="IGallerySettings.GalleryTitle" />;
		/// when specified, this property overrides it.
		/// </summary>
		/// <value>The gallery title.</value>
		string GalleryTitle { get; set; }

		/// <summary>
		/// Gets or sets the URL the user will be directed to when she clicks the gallery title. Optional. If not 
		/// present, no link will be rendered. Examples: "http://www.mysite.com", "/" (the root of the web site),
		/// "~/" (the top level album). If not specified, the application uses <see cref="IGallerySettings.GalleryTitleUrl" />;
		/// when specified, this property overrides it.
		/// </summary>
		/// <value>The gallery title URL.</value>
		string GalleryTitleUrl { get; set; }

		/// <summary>
		/// Indicates whether to show the login controls at the top right of each page. When false, no login controls
		/// are shown, but the user can navigate directly to the login page to log on. If not specified, the application 
		/// uses <see cref="IGallerySettings.ShowLogin" />; when specified, this property overrides it.
		/// </summary>
		/// <value><c>true</c> if login controls are visible; otherwise, <c>false</c>.</value>
		bool? ShowLogin { get; set; }

		/// <summary>
		/// Indicates whether to show the search box at the top right of each page. If not specified, the application 
		/// uses <see cref="IGallerySettings.ShowSearch" />; when specified, this property overrides it.
		/// </summary>
		/// <value><c>true</c> if the search box is visible; otherwise, <c>false</c>.</value>
		bool? ShowSearch { get; set; }

		/// <summary>
		/// Gets or sets the ID of the album to be displayed. This setting can be used to specify that a particular album be displayed. When
		/// specified, the <see cref="GalleryId" /> is ignored. Only one of these properties should be set: <see cref="GalleryId" />, 
		/// <see cref="AlbumId" />, <see cref="MediaObjectId" />. Defaults to <see cref="int.MinValue" /> when not specified.
		/// </summary>
		/// <value>The album ID.</value>
		int? AlbumId { get; set; }

		/// <summary>
		/// Gets or sets the ID of the media object to be displayed. This setting can be used to specify that a particular media object be displayed. When
		/// specified, the <see cref="GalleryId" /> is ignored and the <see cref="ViewMode" /> is automatically set to ViewMode.Single. Only one of these 
		/// properties should be set: <see cref="GalleryId" />, <see cref="AlbumId" />, <see cref="MediaObjectId" />. Defaults to <see cref="int.MinValue" />
		/// when not specified.
		/// </summary>
		/// <value>The media object ID.</value>
		int? MediaObjectId { get; set; }

		/// <summary>
		/// Gets or sets a value indicating how the media objects are to be rendered in the browser. The default value is ViewMode.Multiple. When the value is 
		/// ViewMode.Multiple, the contents of an album are shown as a set of thumbnail images. When set to ViewMode.Single, a single media object is 
		/// displayed. When set to ViewMode.SingleRandom, a single media object is displayed that is randomly selected. When a <see cref="MediaObjectId" /> is 
		/// specified, the <see cref="ViewMode" /> is automatically set to ViewMode.Single.
		/// </summary>
		/// <value>A value indicating how the media objects are to be rendered in the browser.</value>
		ViewMode ViewMode { get; set; }

		/// <summary>
		/// Gets or sets the base URL to invoke when a tree node is clicked. Applies only when <see cref="ViewMode" /> = ViewMode.TreeView.
		/// The album ID of the selected album is passed to the URL as the query string parameter "aid".
		/// Example: "Gallery.aspx, http://site.com/gallery.aspx"
		/// </summary>
		string TreeViewNavigateUrl { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether users can view galleries without logging in. When false, users are redirected to a login
		/// page when any album is requested. Private albums are never shown to anonymous users, even when this property is true. If not 
		/// specified, the application uses <see cref="IGallerySettings.AllowAnonymousBrowsing" />; when specified, this property overrides it.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if anonymous users can view the gallery; otherwise, <c>false</c>.
		/// </value>
		bool? AllowAnonymousBrowsing { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to render a treeview of albums along the left side when an album is being displayed. 
		/// If not specified, the application uses a default value of <c>false</c>.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the album treeview is to be rendered; otherwise, <c>false</c>.
		/// </value>
		bool? ShowAlbumTreeViewForAlbum { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to render a treeview of albums along the left side when a single media object is being displayed.
		/// If not specified, the application uses a default value of <c>false</c>.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the album treeview is to be rendered when a single media object is being displayed; otherwise, <c>false</c>.
		/// </value>
		bool? ShowAlbumTreeViewForMediaObject { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to render the Actions menu. If not specified, the application uses a default value of <c>true</c>.
		/// If the currently logged on user does not have permission to perform any of the items on the Actions menu, it is not displayed, even
		/// if this value is <c>true</c>.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the Actions menu is to be rendered; otherwise, <c>false</c>.
		/// </value>
		bool? ShowActionMenu { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to render the album bread crumb links. If not specified, the application
		/// uses a default value of <c>true</c>.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the album bread crumb links are to be visible; otherwise, <c>false</c>.
		/// </value>
		bool? ShowAlbumBreadCrumb { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the toolbar is rendered above individual media objects. If not specified, the application uses
		/// a default value of <c>true</c>.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the toolbar is rendered above individual media objects; otherwise, <c>false</c>.
		/// </value>
		bool? ShowMediaObjectToolbar { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the title is displayed beneath individual media objects. If not specified, the application uses
		/// a default value of <c>true</c>.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the title is displayed beneath individual media objects; otherwise, <c>false</c>.
		/// </value>
		bool? ShowMediaObjectTitle { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the next and previous buttons are rendered for individual media objects. If not specified, the 
		/// application uses a default value of <c>true</c>.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the next and previous buttons are rendered for individual media objects; otherwise, <c>false</c>.
		/// </value>
		bool? ShowMediaObjectNavigation { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to display the relative position of a media object within an album (example: (3 of 24)). 
		/// Applicable only when a single media object is displayed. If not specified, the application uses a default value of <c>true</c>.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the relative position of a media object within an album is to be rendered; otherwise, <c>false</c>.
		/// </value>
		bool? ShowMediaObjectIndexPosition { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the show metadata button is visible above a media object. If not specified, the 
		/// application uses <see cref="IGallerySettings.EnableMetadata" />; when specified, this property overrides it. When 
		/// <see cref="ShowMediaObjectToolbar" />=<c>false</c>, this property is ignored.
		/// </summary>
		/// <value><c>true</c> if the show metadata button is visible above a media object; otherwise, <c>false</c>.</value>
		bool? ShowMetadataButton { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the download media object button is visible above a media object. If not specified, the 
		/// application uses <see cref="IGallerySettings.EnableMediaObjectDownload" />; when specified, this property overrides it. When 
		/// <see cref="ShowMediaObjectToolbar" />=<c>false</c>, this property is ignored.
		/// </summary>
		/// <value><c>true</c> if the download media object button is visible above a media object; otherwise, <c>false</c>.</value>
		bool? ShowMediaObjectDownloadButton { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the download ZIP archive button is visible above a media object. If not specified, the 
		/// application uses <see cref="IGallerySettings.EnableGalleryObjectZipDownload" />; when specified, this property overrides it. When 
		/// <see cref="ShowMediaObjectToolbar" />=<c>false</c>, this property is ignored.
		/// </summary>
		/// <value><c>true</c> if the download ZIP archive button is visible above a media object; otherwise, <c>false</c>.</value>
		bool? ShowMediaObjectZipDownloadButton { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the show high resolution image button is visible above a media object. This property
		/// is ignored if the current user is not allowed to view the high-res image (due to <see cref="IGallerySettings.AllowAnonymousHiResViewing" />
		/// =<c>false</c> for anonymous users or a logged-on user not being in a role with <see cref="IGalleryServerRole.AllowViewOriginalImage" /> 
		/// permission). If the image does not have a high-resolution version, the button is not shown, even if this property is <c>true</c>.
		/// When <see cref="ShowMediaObjectToolbar" />=<c>false</c>, this property is ignored.
		/// </summary>
		/// <value><c>true</c> if the show high resolution image button is visible above a media object; otherwise, <c>false</c>.</value>
		bool? ShowHighResImageButton { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the show permalink button is visible above a media object. If not specified, the 
		/// application uses <see cref="IGallerySettings.EnablePermalink" />; when specified, this property overrides it. When 
		/// <see cref="ShowMediaObjectToolbar" />=<c>false</c>, this property is ignored.
		/// </summary>
		/// <value><c>true</c> if the show permalink button is visible above a media object; otherwise, <c>false</c>.</value>
		bool? ShowPermalinkButton { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the play/pause slide show button is visible above a media object. If not specified, the 
		/// application uses <see cref="IGallerySettings.EnableSlideShow" />; when specified, this property overrides it. When 
		/// <see cref="ShowMediaObjectToolbar" />=<c>false</c>, this property is ignored.
		/// </summary>
		/// <value><c>true</c> if the play/pause slide show button is visible above a media object; otherwise, <c>false</c>.</value>
		bool? ShowSlideShowButton { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the transfer media object button is visible above a media object. The button is not
		/// shown if the current user does not have permission to move media objects, even if this property is <c>true</c>. When 
		/// <see cref="ShowMediaObjectToolbar" />=<c>false</c>, this property is ignored.
		/// </summary>
		/// <value><c>true</c> if the transfer media object button is visible above a media object; otherwise, <c>false</c>.</value>
		bool? ShowTransferMediaObjectButton { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the copy media object button is visible above a media object. The button is not
		/// shown if the current user does not have permission to copy media objects, even if this property is <c>true</c>. When 
		/// <see cref="ShowMediaObjectToolbar" />=<c>false</c>, this property is ignored.
		/// </summary>
		/// <value><c>true</c> if the copy media object button is visible above a media object; otherwise, <c>false</c>.</value>
		bool? ShowCopyMediaObjectButton { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the rotate media object button is visible above a media object. The button is not
		/// shown if the current user does not have permission to rotate media objects, even if this property is <c>true</c>. When 
		/// <see cref="ShowMediaObjectToolbar" />=<c>false</c>, this property is ignored.
		/// </summary>
		/// <value><c>true</c> if the rotate media object button is visible above a media object; otherwise, <c>false</c>.</value>
		bool? ShowRotateMediaObjectButton { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the delete media object button is visible above a media object. The button is not
		/// shown if the current user does not have permission to delete media objects, even if this property is <c>true</c>. When 
		/// <see cref="ShowMediaObjectToolbar" />=<c>false</c>, this property is ignored.
		/// </summary>
		/// <value><c>true</c> if the delete media object button is visible above a media object; otherwise, <c>false</c>.</value>
		bool? ShowDeleteMediaObjectButton { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether a slide show of media objects automatically starts playing when the page loads. The 
		/// default value is <c>false</c>. This setting applies only when the <see cref="ViewMode" /> is set to ViewMode.Single or ViewMode.SingleRandom
		/// and either an album or media object is specified (that is, the <see cref="AlbumId" /> or <see cref="MediaObjectId" /> is assigned a value). 
		/// If a media object is specified, all images in the object's album will be shown in the slide show.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if a slide show of media objects will automatically start playing; otherwise, <c>false</c>.
		/// </value>
		bool? AutoPlaySlideShow { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether an album or media object specified in the URL can override the <see cref="GalleryId" />,
		/// <see cref="AlbumId" />, and <see cref="MediaObjectId" /> properties of this control. Use the query string parameter "aid" to 
		/// specify an album; use "moid" for a media object (example: default.aspx?aid=12 for album ID=12, default.aspx?moid=37 for media
		/// object ID=37)
		/// </summary>
		/// <value><c>true</c> if an album or media object specified in the query string can override one specified as a control property; otherwise,
		///  <c>false</c>.</value>
		bool? AllowUrlOverride { get; set; }

		/// <summary>
		/// Persist the current gallery control settings to the data store.
		/// </summary>
		void Save();

		/// <summary>
		/// Delete the current gallery control settings from the data store.
		/// </summary>
		void Delete();
	}
}
