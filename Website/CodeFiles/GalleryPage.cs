using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.ErrorHandler.CustomExceptions;
using GalleryServerPro.Web.Controller;
using GalleryServerPro.Web.Controls;
using GalleryServerPro.Web.Entity;

namespace GalleryServerPro.Web.Pages
{
	/// <summary>
	/// The base class user control used in Gallery Server Pro to represent page-like functionality.
	/// </summary>
	public abstract class GalleryPage : UserControl
	{
		#region Private Fields

		private readonly object _lockObject = new object();

		private int _galleryId = int.MinValue;
		private IAlbum _album;
		private int? _mediaObjectId;
		private IGalleryObject _mediaObject;
		private Message _message = Message.None;
		private IGalleryServerRoleCollection _roles;
		private string _pageTitle = String.Empty;
		private bool? _userCanAddAdministerSite;
		private bool? _userCanAdministerGallery;
		private bool? _userCanCreateAlbum;
		private bool? _userCanEditAlbum;
		private bool? _userCanAddMediaObject;
		private bool? _userCanEditMediaObject;
		private bool? _userCanDeleteCurrentAlbum;
		private bool? _userCanDeleteChildAlbum;
		private bool? _userCanDeleteMediaObject;
		private bool? _userCanSynchronize;
		private bool? _userCanViewHiResImage;
		private bool? _userCanAddMediaObjectToAtLeastOneAlbum;
		private bool? _userCanAddAlbumToAtLeastOneAlbum;
		private Gallery _galleryControl;
		private IGallerySettings _gallerySetting;
		private Controls.albummenu _albumMenu;
		private Controls.galleryheader _galleryHeader;
		private int _currentPage;
		private bool? _isComponentArtCallback;
		private PageId _pageId;
		private bool? _showLogin;
		private bool? _showSearch;
		private bool? _allowAnonymousBrowsing;
		private bool? _showAlbumTreeViewForAlbum;
		private bool? _showAlbumTreeViewForMediaObject;
		private bool? _showActionMenu;
		private bool? _showAlbumBreadCrumb;
		private bool? _showHeader;
		private string _galleryTitle;
		private string _galleryTitleUrl;
		private bool? _showMediaObjectToolbar;
		private bool? _showMediaObjectTitle;
		private bool? _showMediaObjectNavigation;
		private bool? _showMediaObjectIndexPosition;
		private bool? _showMetadataButton;
		private bool? _showMediaObjectDownloadButton;
		private bool? _showMediaObjectZipDownloadButton;
		private bool? _showHighResImageButton;
		private bool? _showPermalinkButton;
		private bool? _showSlideShowButton;
		private bool? _showTransferMediaObjectButton;
		private bool? _showCopyMediaObjectButton;
		private bool? _showRotateMediaObjectButton;
		private bool? _showDeleteMediaObjectButton;
		private bool? _autoPlaySlideShow;
		private bool _albumTreeViewIsVisible;
		private bool _footerHasBeenRendered;
		private bool _jQueryRequired;
		private bool _jQueryUiRequired;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes the <see cref="GalleryPage"/> class.
		/// </summary>
		static GalleryPage()
		{
			CheckForInstallOrUpgrade();

			if (!GalleryController.IsInitialized)
			{
				GalleryController.InitializeGspApplication();
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GalleryPage"/> class.
		/// </summary>
		protected GalleryPage()
		{
			// Ensure the app is initialized. This should have been done in the static constructor, but if anything went wrong
			// there, it may not be initialized, so we check again.
			if (!GalleryController.IsInitialized)
			{
				GalleryController.InitializeGspApplication();
			}

			this.Init += GalleryPage_Init;
			this.Load +=GalleryPage_Load;
			//this.Unload += this.GalleryPage_Unload;
			//this.Error += this.GalleryPage_Error;
			this.PreRender += (GalleryPage_PreRender);
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// Handles the Init event of the GalleryPage control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void GalleryPage_Init(object sender, System.EventArgs e)
		{
			InitializePage();
		}

		/// <summary>
		/// Handles the PreRender event of the GalleryPage control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void GalleryPage_PreRender(object sender, EventArgs e)
		{
			HtmlHead header = this.Page.Header;
			if (header == null)
				throw new WebException(Resources.GalleryServerPro.Error_Head_Tag_Missing_Server_Attribute_Ex_Msg);

			SetupHeadControl(header);
		}

		private void GalleryPage_Load(object sender, EventArgs e)
		{
			AddjQuery();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the value that uniquely identifies the gallery the current instance belongs to. This value is retrieved from the 
		/// requested media object or album, or from the <see cref="Gallery.GalleryId" /> property of the <see cref="Gallery" /> control 
		/// that created this instance. If no gallery ID is found by the previous search, then return the first gallery found in the database.
		/// Retrieving this value causes the <see cref="Gallery.GalleryId" /> on the containing control to be set to the same value.
		/// </summary>
		/// <value>The gallery ID for the current gallery.</value>
		/// <exception cref="InvalidAlbumException">Thrown when the requested album does not exist.</exception>
		/// <exception cref="GallerySecurityException">Thrown when the user is requesting an album or media object they don't have 
		/// permission to view.</exception>
		public int GalleryId
		{
			get
			{
				if (_galleryId == int.MinValue)
				{
					if (GetMediaObjectId() > int.MinValue)
					{
						_galleryId = GetMediaObject().GalleryId;
					}
					else if (ParseAlbumId() > int.MinValue)
					{
						_galleryId = GetAlbum().GalleryId;
					}
					else if (this.GalleryControl.GalleryId > int.MinValue)
					{
						_galleryId = this.GalleryControl.GalleryId;
					}
					else
					{
						// There is no album or media object to get the gallery ID from, and no gallery ID has been specified on the control.
						// Just grab the first gallery in the database, creating it if necessary.
						IGalleryCollection galleries = Factory.LoadGalleries();
						if (galleries.Count > 0)
						{
							_galleryId = galleries[0].GalleryId;
							this.GalleryControl.GalleryControlSettings.GalleryId = _galleryId;
							this.GalleryControl.GalleryControlSettings.Save();
						}
						else
						{
							// No gallery found anywhere, including the data store. Create one and assign it to this control instance.
							IGallery gallery = Factory.CreateGalleryInstance();
							gallery.Description = "My gallery";
							gallery.CreationDate = DateTime.Now;
							gallery.Save();
							this.GalleryControl.GalleryControlSettings.GalleryId = gallery.GalleryId;
							this.GalleryControl.GalleryControlSettings.Save();
							_galleryId = gallery.GalleryId;
						}
					}
				}

				if (this.GalleryControl.GalleryId == int.MinValue)
				{
					this.GalleryControl.GalleryId = _galleryId;
				}

				return _galleryId;
			}
		}

		/// <summary>
		/// Gets the gallery settings for the current gallery.
		/// </summary>
		/// <value>The gallery settings for the current gallery.</value>
		public IGallerySettings GallerySettings
		{
			get
			{
				return _gallerySetting;
			}
		}

		/// <summary>
		/// Gets or sets the page index when paging is enabled and active. This is one-based, so the first page is one, the second
		/// is two, and so one.
		/// </summary>
		/// <value>The current page index.</value>
		public int CurrentPage
		{
			get
			{
				if (this._currentPage == 0)
				{
					int page = Utils.GetQueryStringParameterInt32("page");

					this._currentPage = (page > 0 ? page : 1);
				}

				return this._currentPage;
			}
			set
			{
				this._currentPage = value;

				if (HttpContext.Current.Session != null)
				{
					Uri backURL = this.PreviousUri;
					if (backURL != null)
					{
						// Update the page query string parameter so that the referring url points to the current page index.
						backURL = UpdateUriQueryString(backURL, "page", this._currentPage.ToString(CultureInfo.InvariantCulture));
					}
					else
					{
						backURL = UpdateUriQueryString(Utils.GetCurrentPageUri(), "page", this._currentPage.ToString(CultureInfo.InvariantCulture));
					}
					this.PreviousUri = backURL;
				}

			}
		}

		/// <summary>
		/// Gets a value indicating whether the current user is anonymous. If the user has authenticated with a user name/password, 
		/// this property is false.
		/// </summary>
		public bool IsAnonymousUser
		{
			// Note: Do not store in a private field that lasts the lifetime of the page request, as this may give the wrong
			// value after logon and logoff events.
			get
			{
				return !Utils.IsAuthenticated;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the current user has permission to administer the site. If true, the user
		/// has all possible permissions and there is nothing he or she can't do.
		/// </summary>
		public bool UserCanAdministerSite
		{
			get
			{
				if (!this._userCanAddAdministerSite.HasValue)
					EvaluateUserPermissions();

				return this._userCanAddAdministerSite.Value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the logged on user is a gallery administrator for the current gallery.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the logged on user is a gallery administrator for the current gallery; otherwise, <c>false</c>.
		/// </value>
		public bool UserCanAdministerGallery
		{
			get
			{
				if (!this._userCanAdministerGallery.HasValue)
					EvaluateUserPermissions();

				return this._userCanAdministerGallery.Value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the current user has permission to create a new album within the current album.
		/// </summary>
		public bool UserCanCreateAlbum
		{
			get
			{
				if (!this._userCanCreateAlbum.HasValue)
					EvaluateUserPermissions();

				return this._userCanCreateAlbum.Value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the current user has permission to edit information about the current album.
		/// This includes changing the album's title, description, start and end dates, assigning the album's thumbnail image,
		/// and rearranging the order of objects within the album.
		/// </summary>
		public bool UserCanEditAlbum
		{
			get
			{
				if (!this._userCanEditAlbum.HasValue)
					EvaluateUserPermissions();

				return this._userCanEditAlbum.Value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the current user has permission to add media objects to the current album.
		/// </summary>
		public bool UserCanAddMediaObject
		{
			get
			{
				if (!this._userCanAddMediaObject.HasValue)
					EvaluateUserPermissions();

				return this._userCanAddMediaObject.Value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the current user has permission to edit the current media object. This includes 
		/// changing the media object's caption, rotating the object (if it is an image), and deleting the high resolution
		/// version of the object (applies only if it is an image).
		/// </summary>
		public bool UserCanEditMediaObject
		{
			get
			{
				if (!this._userCanEditMediaObject.HasValue)
					EvaluateUserPermissions();

				return this._userCanEditMediaObject.Value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the current user has permission to delete the current album.
		/// </summary>
		public bool UserCanDeleteCurrentAlbum
		{
			get
			{
				if (!this._userCanDeleteCurrentAlbum.HasValue)
					EvaluateUserPermissions();

				return this._userCanDeleteCurrentAlbum.Value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the current user has permission to delete albums within the current album.
		/// </summary>
		public bool UserCanDeleteChildAlbum
		{
			get
			{
				if (!this._userCanDeleteChildAlbum.HasValue)
					EvaluateUserPermissions();

				return this._userCanDeleteChildAlbum.Value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the current user has permission to delete a media object in the current album.
		/// </summary>
		public bool UserCanDeleteMediaObject
		{
			get
			{
				if (!this._userCanDeleteMediaObject.HasValue)
					EvaluateUserPermissions();

				return this._userCanDeleteMediaObject.Value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the current user has permission to synchronize the current album.
		/// </summary>
		public bool UserCanSynchronize
		{
			get
			{
				if (!this._userCanSynchronize.HasValue)
					EvaluateUserPermissions();

				return this._userCanSynchronize.Value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the current user has permission to view the original high resolution version of an image.
		/// </summary>
		public bool UserCanViewHiResImage
		{
			get
			{
				if (!this._userCanViewHiResImage.HasValue)
					EvaluateUserPermissions();

				return this._userCanViewHiResImage.Value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the current user has permission to add media objects to at least one album.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if current user has permission to add media objects to at least one album; otherwise, <c>false</c>.
		/// </value>
		public bool UserCanAddMediaObjectToAtLeastOneAlbum
		{
			get
			{
				if (!this._userCanAddMediaObjectToAtLeastOneAlbum.HasValue)
					EvaluateUserPermissions();

				return this._userCanAddMediaObjectToAtLeastOneAlbum.Value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the current user has permission to add albums to at least one album.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the current user has permission to add albums to at least one album; otherwise, <c>false</c>.
		/// </value>
		public bool UserCanAddAlbumToAtLeastOneAlbum
		{
			get
			{
				if (!this._userCanAddAlbumToAtLeastOneAlbum.HasValue)
					EvaluateUserPermissions();

				return this._userCanAddAlbumToAtLeastOneAlbum.Value;
			}
		}

		/// <summary>
		/// Gets or sets the message used to display user messages, such as "Invalid login". The value is retrieved from the
		/// "msgId" query string parameter or from a private field if it was explicitly assigned earlier in the current page's
		/// life cycle. Returns int.MinValue if the parameter is not found, it is not a valid integer, or it is &lt;= 0.
		/// Setting this property sets a private field that lives as long as the current page lifecycle. It is not persisted across
		/// postbacks or added to the querystring. Set the value only when you will use it later in the current page's lifecycle.
		///  Defaults to Message.None.
		/// </summary>
		protected Message Message
		{
			get
			{
				if (this._message == Message.None)
				{
					if (this.GalleryControl.Message != Message.None)
					{
						this._message = this.GalleryControl.Message;
					}
					else
					{
						int msgId = Utils.GetQueryStringParameterInt32("msg");
						if (msgId > int.MinValue)
						{
							this._message = (Message)Enum.Parse(typeof(Message), msgId.ToString(CultureInfo.InvariantCulture));
						}
					}
				}
				return this._message;
			}
			set
			{
				this._message = value;
			}
		}

		/// <summary>
		/// Gets or sets the value that identifies the type of gallery page that is currently being displayed.
		/// </summary>
		/// <value>The value that identifies the type of gallery page that is currently being displayed.</value>
		/// <exception cref="InvalidOperationException">Thrown when the property is accessed before it has been set.</exception>
		public PageId PageId
		{
			get
			{
				if (this._pageId == 0)
					throw new InvalidOperationException("The PageId property has not been set to a valid value.");

				return this._pageId;
			}
			set
			{
				this._pageId = value;
			}
		}

		/// <summary>
		/// Gets or sets the instance of the user control that created this user control.
		/// </summary>
		/// <value>The user control that created this user control.</value>
		/// <exception cref="WebException">Thrown when an instance of the <see cref="Gallery" /> control is not found in the parent 
		/// heirarchy of the current control.</exception>
		public Gallery GalleryControl
		{
			get
			{
				if (_galleryControl != null)
					return _galleryControl;

				System.Web.UI.Control ctl = Parent;
				while (ctl.GetType() != typeof(Gallery))
				{
					ctl = ctl.Parent;
					if (ctl == null)
					{
						throw new WebException(String.Format(CultureInfo.CurrentCulture, "Could not find an instance of {0} that contains the current control ({1}). All user controls in Gallery Server Pro must be loaded dynamically within the {0} control.", typeof(Gallery), this.GetType()));
					}
				}

				_galleryControl = (Gallery)ctl;
				return _galleryControl;
			}
			set
			{
				_galleryControl = value;
			}
		}

		/// <summary>
		/// Gets or sets a value that can be used in the title tag in the HTML page header. If this property is not set by the user
		/// control, the current album's title is used.
		/// </summary>
		/// <value>A value that can be used in the title tag in the HTML page header.</value>
		public virtual string PageTitle
		{
			get
			{
				if (String.IsNullOrEmpty(_pageTitle))
				{
					// Get an HTML-cleaned version of the current album's title, limited to the first 50 characters.
					string title = Utils.RemoveHtmlTags(GetAlbum().Title);
					title = title.Substring(0, title.Length < 50 ? title.Length : 50);

					return String.Concat(Resources.GalleryServerPro.UC_ThumbnailView_Album_Title_Prefix_Text, " ", title);
				}
				else
					return _pageTitle;
			}
			set
			{
				this._pageTitle = value;
			}
		}

		/// <summary>
		/// Gets a reference to the <see cref="albummenu"/> control on the page.
		/// </summary>
		/// <value>The <see cref="albummenu"/> control on the page.</value>
		public Controls.albummenu AlbumMenu
		{
			get
			{
				return this._albumMenu;
			}
		}

		/// <summary>
		/// Gets a reference to the <see cref="galleryheader"/> control on the page.
		/// </summary>
		/// <value>The <see cref="galleryheader"/> control on the page.</value>
		public Controls.galleryheader GalleryHeader
		{
			get
			{
				return this._galleryHeader;
			}
		}

		/// <summary>
		/// Gets or sets the URI of the previous page the user was viewing. The value is stored in the user's session, and 
		/// can be used after a user has completed a task to return to the original page. If the Session object is not available,
		/// no value is saved in the setter and a null is returned in the getter.
		/// </summary>
		/// <value>The URI of the previous page the user was viewing.</value>
		public Uri PreviousUri
		{
			get
			{
				return Utils.PreviousUri;
			}
			set
			{
				Utils.PreviousUri = value;
			}
		}

		/// <summary>
		/// Gets the URL of the previous page the user was viewing. The value is based on the <see cref="PreviousUri" /> property
		/// and is relative to the application root. If <see cref="PreviousUri" /> is null, such as when the Session object is not
		/// available or it has never been assigned, return String.Empty. Remove the query string parameter "msg" if present. 
		/// Ex: "/gallery/gs/default.aspx?moid=770"
		/// </summary>
		/// <value>The URL of the previous page the user was viewing.</value>
		public string PreviousUrl
		{
			get
			{
				if (PreviousUri != null)
					return Utils.RemoveQueryStringParameter(PreviousUri.PathAndQuery, "msg");
				else
					return String.Empty;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the page request is a result of a callback. This is determined by checking 
		/// <see cref="Page.IsCallback"/> and <see cref="IsCallbackCausedByComponentArt"/>. If either is true, then return
		/// true; otherwise return false.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the page request is a result of a callback; otherwise, <c>false</c>.
		/// </value>
		public bool IsCallback
		{
			get
			{
				return (base.Page.IsCallback || IsCallbackCausedByComponentArt);
			}
		}

		/// <summary>
		/// Gets a value indicating whether the page request is a result of a callback initiated by the ComponentArt
		/// Callback control. This property is required because the native <see cref="Page.IsCallback"/> does
		/// not return true during a ComponentArt callback.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the page request is a result of a callback initiated by the ComponentArt Callback control; 
		/// otherwise, <c>false</c>.
		/// </value>
		public bool IsCallbackCausedByComponentArt
		{
			get
			{
				if (!this._isComponentArtCallback.HasValue)
				{
					this._isComponentArtCallback = false;

					for (int i = 0; i < Request.Form.Count; i++)
					{
						if (!String.IsNullOrEmpty(Request.Form.Keys[i]) && Request.Form.Keys[i].IndexOf("_Callback_Param", StringComparison.Ordinal) > -1)
						{
							this._isComponentArtCallback = true;
							break;
						}
					}
				}

				return this._isComponentArtCallback.Value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is a new page load. That is, the page is not a post back and not 
		/// involved in any AJAX callbacks.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is a new page load; otherwise, <c>false</c>.
		/// </value>
		public bool IsNewPageLoad
		{
			get
			{
				return (!(IsPostBack) && (!this.Page.IsCallback) && !(IsCallbackCausedByComponentArt));
			}
		}

		/// <summary>
		/// Gets a value indicating whether to show the login controls at the top right of each page. When false, no login controls
		/// are shown, but the user can still navigate directly to the login page to log on. This value is retrieved from the 
		/// <see cref="Gallery.ShowLogin" /> property if specified; if not, it inherits the value from <see cref="IGallerySettings.ShowLogin" />. 
		/// </summary>
		/// <value><c>true</c> if login controls are visible; otherwise, <c>false</c>.</value>
		public bool ShowLogin
		{
			get
			{
				if (!_showLogin.HasValue)
				{
					this._showLogin = (this.GalleryControl.ShowLogin.HasValue ? this.GalleryControl.ShowLogin.Value : this.GallerySettings.ShowLogin);
				}

				return this._showLogin.Value;
			}
			protected set { _showLogin = value; }
		}

		/// <summary>
		/// Gets a value indicating whether to show the search box at the top right of each page. This value is retrieved from the 
		/// <see cref="Gallery.ShowSearch" /> property if specified; if not, it inherits the value from <see cref="IGallerySettings.ShowSearch" />. 
		/// </summary>
		/// <value><c>true</c> if the search box is visible; otherwise, <c>false</c>.</value>
		public bool ShowSearch
		{
			get
			{
				if (!_showSearch.HasValue)
				{
					this._showSearch = (this.GalleryControl.ShowSearch.HasValue ? this.GalleryControl.ShowSearch.Value : this.GallerySettings.ShowSearch);
				}

				return this._showSearch.Value;
			}
			protected set { _showSearch = value; }
		}

		/// <summary>
		/// Gets a value indicating whether users can view galleries without logging in. When false, users are redirected to a login
		/// page when any album is requested. Private albums are never shown to anonymous users, even when this property is true. 
		/// This value is retrieved from the <see cref="Gallery.AllowAnonymousBrowsing" /> property if specified; if not, it inherits 
		/// the value from <see cref="IGallerySettings.AllowAnonymousBrowsing" />.
		/// </summary>
		/// <value><c>true</c> if anonymous users can view the gallery; otherwise, <c>false</c>.</value>
		public bool AllowAnonymousBrowsing
		{
			get
			{
				if (!_allowAnonymousBrowsing.HasValue)
				{
					this._allowAnonymousBrowsing = (this.GalleryControl.AllowAnonymousBrowsing.HasValue ? this.GalleryControl.AllowAnonymousBrowsing.Value : this.GallerySettings.AllowAnonymousBrowsing);
				}

				return this._allowAnonymousBrowsing.Value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether to render a treeview of albums along the left side when an album is being displayed.
		/// This value is retrieved from the <see cref="Gallery.ShowAlbumTreeViewForAlbum" /> property if specified; if not, it uses a 
		/// default value of <c>false</c>.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the treeview is to be rendered; otherwise, <c>false</c>.
		/// </value>
		public bool ShowAlbumTreeViewForAlbum
		{
			get
			{
				if (!this._showAlbumTreeViewForAlbum.HasValue)
				{
					this._showAlbumTreeViewForAlbum = (this.GalleryControl.ShowAlbumTreeViewForAlbum.HasValue ? this.GalleryControl.ShowAlbumTreeViewForAlbum.Value : false);
				}

				return this._showAlbumTreeViewForAlbum.Value;
			}
			set
			{
				this._showAlbumTreeViewForAlbum = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether to render a treeview of albums along the left side when a single media object is
		/// being displayed. This value is retrieved from the <see cref="Gallery.ShowAlbumTreeViewForMediaObject" /> 
		/// property if specified; if not, it uses a default value of <c>false</c>.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the treeview is to be rendered when a single media object is being displayed; otherwise, <c>false</c>.
		/// </value>
		public bool ShowAlbumTreeViewForMediaObject
		{
			get
			{
				if (!this._showAlbumTreeViewForMediaObject.HasValue)
				{
					this._showAlbumTreeViewForMediaObject = (this.GalleryControl.ShowAlbumTreeViewForMediaObject.HasValue ? this.GalleryControl.ShowAlbumTreeViewForMediaObject.Value : false);
				}

				return this._showAlbumTreeViewForMediaObject.Value;
			}
			set
			{
				this._showAlbumTreeViewForMediaObject = value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether to render the Actions menu. This value is retrieved from the 
		/// <see cref="Gallery.ShowActionMenu" /> property if specified; if not, it uses a default value of <c>true</c>. Note that calling 
		/// code may determine the Actions menu should be hidden even if this property returns <c>true</c>. For example, this will happen
		/// when the currently logged on user does not have permission to execute any of the actions in the menu.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the Actions menu is to be rendered; otherwise, <c>false</c>.
		/// </value>
		public bool ShowActionMenu
		{
			get
			{
				if (!this._showActionMenu.HasValue)
				{
					this._showActionMenu = (this.GalleryControl.ShowActionMenu.HasValue ? this.GalleryControl.ShowActionMenu.Value : true);
				}

				return this._showActionMenu.Value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether to render the album bread crumb links. This value is retrieved from the 
		/// <see cref="Gallery.ShowAlbumBreadCrumb" /> property if specified; if not, it uses a default value of <c>true</c>.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the album bread crumb links are to be visible; otherwise, <c>false</c>.
		/// </value>
		public bool ShowAlbumBreadCrumb
		{
			get
			{
				if (!this._showAlbumBreadCrumb.HasValue)
				{
					this._showAlbumBreadCrumb = (this.GalleryControl.ShowAlbumBreadCrumb.HasValue ? this.GalleryControl.ShowAlbumBreadCrumb.Value : true);
				}

				return this._showAlbumBreadCrumb.Value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether to render the header at the top of the gallery. This value is retrieved from the 
		/// <see cref="Gallery.ShowHeader" /> property if specified; if not, it inherits the value from <see cref="IGallerySettings.ShowHeader" />.
		/// The header includes the gallery title, login/logout controls, user account management link, and search 
		/// function. The title, login/logout controls and search function can be individually controlled via the <see cref="GalleryTitle" />,
		/// <see cref="ShowLogin" /> and <see cref="ShowSearch" /> properties.
		/// </summary>
		/// <value><c>true</c> if the header is to be dislayed; otherwise, <c>false</c>.</value>
		public bool ShowHeader
		{
			get
			{
				if (!this._showHeader.HasValue)
				{
					this._showHeader = (this.GalleryControl.ShowHeader.HasValue ? this.GalleryControl.ShowHeader.Value : this.GallerySettings.ShowHeader);
				}

				return this._showHeader.Value;
			}
		}

		/// <summary>
		/// Gets the header text that appears at the top of each web page. This value is retrieved from the 
		/// <see cref="Gallery.GalleryTitle" /> property if specified; if not, it inherits the value from <see cref="IGallerySettings.GalleryTitle" />.
		/// </summary>
		/// <value>The gallery title.</value>
		public string GalleryTitle
		{
			get
			{
				if (_galleryTitle == null)
				{
					this._galleryTitle = (GalleryControl.GalleryTitle != null ? this.GalleryControl.GalleryTitle : this.GallerySettings.GalleryTitle);
				}

				return this._galleryTitle;
			}
		}

		/// <summary>
		/// Gets the URL the user will be directed to when she clicks the gallery title. This value is retrieved from the 
		/// <see cref="Gallery.GalleryTitleUrl" /> property if specified; if not, it inherits the value from <see cref="IGallerySettings.GalleryTitleUrl" />.
		/// </summary>
		/// <value>The gallery title.</value>
		public string GalleryTitleUrl
		{
			get
			{
				if (_galleryTitleUrl == null)
				{
					this._galleryTitleUrl = (GalleryControl.GalleryTitleUrl != null ? this.GalleryControl.GalleryTitleUrl : this.GallerySettings.GalleryTitleUrl);
				}

				return this._galleryTitleUrl;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the toolbar is rendered above individual media objects. This value is retrieved from the 
		/// <see cref="Gallery.ShowMediaObjectToolbar" /> property if specified; if not, it uses a default value of <c>true</c>.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the toolbar is rendered above individual media objects; otherwise, <c>false</c>.
		/// </value>
		public bool ShowMediaObjectToolbar
		{
			get
			{
				if (!this._showMediaObjectToolbar.HasValue)
				{
					this._showMediaObjectToolbar = (this.GalleryControl.ShowMediaObjectToolbar.HasValue ? this.GalleryControl.ShowMediaObjectToolbar.Value : true);
				}

				return this._showMediaObjectToolbar.Value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the title is displayed beneath individual media objects. This value is retrieved from the 
		/// <see cref="Gallery.ShowMediaObjectTitle" /> property if specified; if not, it uses a default value of <c>true</c>.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the title is displayed beneath individual media objects; otherwise, <c>false</c>.
		/// </value>
		public bool ShowMediaObjectTitle
		{
			get
			{
				if (!this._showMediaObjectTitle.HasValue)
				{
					this._showMediaObjectTitle = (this.GalleryControl.ShowMediaObjectTitle.HasValue ? this.GalleryControl.ShowMediaObjectTitle.Value : true);
				}

				return this._showMediaObjectTitle.Value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the next and previous buttons are rendered for individual media objects. This value is retrieved 
		/// from the <see cref="Gallery.ShowMediaObjectNavigation" /> property if specified; if not, it uses a default value of <c>true</c>.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the next and previous buttons are rendered for individual media objects; otherwise, <c>false</c>.
		/// </value>
		public bool ShowMediaObjectNavigation
		{
			get
			{
				if (!this._showMediaObjectNavigation.HasValue)
				{
					this._showMediaObjectNavigation = (this.GalleryControl.ShowMediaObjectNavigation.HasValue ? this.GalleryControl.ShowMediaObjectNavigation.Value : true);
				}

				return this._showMediaObjectNavigation.Value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether to display the relative position of a media object within an album (example: (3 of 24)). 
		/// This value is retrieved from the <see cref="Gallery.ShowMediaObjectNavigation" /> property if specified; if not, it uses a 
		/// default value of <c>true</c>. Applicable only when a single media object is displayed.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the relative position of a media object within an album is to be rendered; otherwise, <c>false</c>.
		/// </value>
		public bool ShowMediaObjectIndexPosition
		{
			get
			{
				if (!this._showMediaObjectIndexPosition.HasValue)
				{
					this._showMediaObjectIndexPosition = (this.GalleryControl.ShowMediaObjectIndexPosition.HasValue ? this.GalleryControl.ShowMediaObjectIndexPosition.Value : true);
				}

				return this._showMediaObjectIndexPosition.Value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the show metadata button is visible above a media object. This value is retrieved from the 
		/// <see cref="Gallery.ShowMetadataButton" /> property if specified; if not, it inherits the value from 
		/// <see cref="IGallerySettings.EnableMetadata" />.  When <see cref="ShowMediaObjectToolbar" />=<c>false</c>, this property is ignored.
		/// </summary>
		/// <value><c>true</c> if the show metadata button is visible above a media object; otherwise, <c>false</c>.</value>
		public bool ShowMetadataButton
		{
			get
			{
				if (!_showMetadataButton.HasValue)
				{
					this._showMetadataButton = (this.GalleryControl.ShowMetadataButton.HasValue ? this.GalleryControl.ShowMetadataButton.Value : this.GallerySettings.EnableMetadata);
				}

				return this._showMetadataButton.Value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the download media object button is visible above a media object. This value is retrieved from the 
		/// <see cref="Gallery.ShowMediaObjectDownloadButton" /> property if specified; if not, it inherits the value from 
		/// <see cref="IGallerySettings.EnableMediaObjectDownload" />.  When <see cref="ShowMediaObjectToolbar" />=<c>false</c>, this property is ignored.
		/// </summary>
		/// <value><c>true</c> if the download media object button is visible above a media object; otherwise, <c>false</c>.</value>
		public bool ShowMediaObjectDownloadButton
		{
			get
			{
				if (!_showMediaObjectDownloadButton.HasValue)
				{
					this._showMediaObjectDownloadButton = (this.GalleryControl.ShowMediaObjectDownloadButton.HasValue ? this.GalleryControl.ShowMediaObjectDownloadButton.Value : this.GallerySettings.EnableMediaObjectDownload);
				}

				return this._showMediaObjectDownloadButton.Value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the download ZIP archive button is visible above a media object. This value is retrieved from the 
		/// <see cref="Gallery.ShowMediaObjectZipDownloadButton" /> property if specified; if not, it inherits the value from 
		/// <see cref="IGallerySettings.EnableGalleryObjectZipDownload" />.  When <see cref="ShowMediaObjectToolbar" />=<c>false</c>, this property is ignored.
		/// </summary>
		/// <value><c>true</c> if the download ZIP archive button is visible above a media object; otherwise, <c>false</c>.</value>
		public bool ShowMediaObjectZipDownloadButton
		{
			get
			{
				if (!_showMediaObjectZipDownloadButton.HasValue)
				{
					this._showMediaObjectZipDownloadButton = (this.GalleryControl.ShowMediaObjectZipDownloadButton.HasValue ? this.GalleryControl.ShowMediaObjectZipDownloadButton.Value : this.GallerySettings.EnableGalleryObjectZipDownload);
				}

				return this._showMediaObjectZipDownloadButton.Value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the show high resolution image button is visible above a media object. This value is retrieved 
		/// from the <see cref="Gallery.ShowHighResImageButton" /> property if specified; if not, it uses a default value of <c>true</c>. This property
		/// is ignored if the current user is not allowed to view the high-res image (due to <see cref="IGallerySettings.AllowAnonymousHiResViewing" />
		/// =<c>false</c> for anonymous users or a logged-on user not being in a role with <see cref="IGalleryServerRole.AllowViewOriginalImage" /> 
		/// permission). If the image does not have a high-resolution version, the button is not shown, even if this property is <c>true</c>.
		/// When <see cref="ShowMediaObjectToolbar" />=<c>false</c>, this property is ignored.
		/// </summary>
		/// <value><c>true</c> if the show high resolution image button is visible above a media object; otherwise, <c>false</c>.</value>
		public bool ShowHighResImageButton
		{
			get
			{
				if (!_showHighResImageButton.HasValue)
				{
					this._showHighResImageButton = (this.GalleryControl.ShowHighResImageButton.HasValue ? this.GalleryControl.ShowHighResImageButton.Value : true);
				}

				return this._showHighResImageButton.Value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the show permalink button is visible above a media object. This value is retrieved from the 
		/// <see cref="Gallery.ShowPermalinkButton" /> property if specified; if not, it inherits the value from 
		/// <see cref="IGallerySettings.EnablePermalink" />.  When <see cref="ShowMediaObjectToolbar" />=<c>false</c>, this property is ignored.
		/// </summary>
		/// <value><c>true</c> if the show permalink button is visible above a media object; otherwise, <c>false</c>.</value>
		public bool ShowPermalinkButton
		{
			get
			{
				if (!_showPermalinkButton.HasValue)
				{
					this._showPermalinkButton = (this.GalleryControl.ShowPermalinkButton.HasValue ? this.GalleryControl.ShowPermalinkButton.Value : this.GallerySettings.EnablePermalink);
				}

				return this._showPermalinkButton.Value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the play/pause slide show button is visible above a media object. This value is retrieved from the 
		/// <see cref="Gallery.ShowSlideShowButton" /> property if specified; if not, it inherits the value from 
		/// <see cref="IGallerySettings.EnableSlideShow" />.  When <see cref="ShowMediaObjectToolbar" />=<c>false</c>, this property is ignored.
		/// </summary>
		/// <value><c>true</c> if the play/pause slide show button is visible above a media object; otherwise, <c>false</c>.</value>
		public bool ShowSlideShowButton
		{
			get
			{
				if (!_showSlideShowButton.HasValue)
				{
					this._showSlideShowButton = (this.GalleryControl.ShowSlideShowButton.HasValue ? this.GalleryControl.ShowSlideShowButton.Value : this.GallerySettings.EnableSlideShow);
				}

				return this._showSlideShowButton.Value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the transfer media object button is visible above a media object. The button is not
		/// shown if the current user does not have permission to move media objects, even if this property is <c>true</c>. This 
		/// value is retrieved from the <see cref="Gallery.ShowTransferMediaObjectButton" /> property if specified; if not, it uses
		/// a default value of <c>true</c>. When <see cref="ShowMediaObjectToolbar" />=<c>false</c>, this property is ignored.
		/// </summary>
		/// <value><c>true</c> if the transfer media object button is visible above a media object; otherwise, <c>false</c>.</value>
		public bool ShowTransferMediaObjectButton
		{
			get
			{
				if (!this._showTransferMediaObjectButton.HasValue)
				{
					this._showTransferMediaObjectButton = (this.GalleryControl.ShowTransferMediaObjectButton.HasValue ? this.GalleryControl.ShowTransferMediaObjectButton.Value : true);
				}

				return this._showTransferMediaObjectButton.Value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the copy media object button is visible above a media object. The button is not
		/// shown if the current user does not have permission to copy media objects, even if this property is <c>true</c>. This 
		/// value is retrieved from the <see cref="Gallery.ShowCopyMediaObjectButton" /> property if specified; if not, it uses
		/// a default value of <c>true</c>. When <see cref="ShowMediaObjectToolbar" />=<c>false</c>, this property is ignored.
		/// </summary>
		/// <value><c>true</c> if the copy media object button is visible above a media object; otherwise, <c>false</c>.</value>
		public bool ShowCopyMediaObjectButton
		{
			get
			{
				if (!this._showCopyMediaObjectButton.HasValue)
				{
					this._showCopyMediaObjectButton = (this.GalleryControl.ShowCopyMediaObjectButton.HasValue ? this.GalleryControl.ShowCopyMediaObjectButton.Value : true);
				}

				return this._showCopyMediaObjectButton.Value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the rotate media object button is visible above a media object. The button is not
		/// shown if the current user does not have permission to rotate media objects, even if this property is <c>true</c>. This 
		/// value is retrieved from the <see cref="Gallery.ShowRotateMediaObjectButton" /> property if specified; if not, it uses
		/// a default value of <c>true</c>. When <see cref="ShowMediaObjectToolbar" />=<c>false</c>, this property is ignored.
		/// </summary>
		/// <value><c>true</c> if the rotate media object button is visible above a media object; otherwise, <c>false</c>.</value>
		public bool ShowRotateMediaObjectButton
		{
			get
			{
				if (!this._showRotateMediaObjectButton.HasValue)
				{
					this._showRotateMediaObjectButton = (this.GalleryControl.ShowRotateMediaObjectButton.HasValue ? this.GalleryControl.ShowRotateMediaObjectButton.Value : true);
				}

				return this._showRotateMediaObjectButton.Value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the delete media object button is visible above a media object. The button is not
		/// shown if the current user does not have permission to delete media objects, even if this property is <c>true</c>. This 
		/// value is retrieved from the <see cref="Gallery.ShowDeleteMediaObjectButton" /> property if specified; if not, it uses
		/// a default value of <c>true</c>. When <see cref="ShowMediaObjectToolbar" />=<c>false</c>, this property is ignored.
		/// </summary>
		/// <value><c>true</c> if the delete media object button is visible above a media object; otherwise, <c>false</c>.</value>
		public bool ShowDeleteMediaObjectButton
		{
			get
			{
				if (!this._showDeleteMediaObjectButton.HasValue)
				{
					this._showDeleteMediaObjectButton = (this.GalleryControl.ShowDeleteMediaObjectButton.HasValue ? this.GalleryControl.ShowDeleteMediaObjectButton.Value : true);
				}

				return this._showDeleteMediaObjectButton.Value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether a slide show of image media objects automatically starts playing when the page loads. This value is retrieved 
		/// from the <see cref="Gallery.AutoPlaySlideShow" /> property if specified; if not, it uses a default value of <c>false</c>. This setting 
		/// applies only when the application is showing a single media object.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if a slide show of image media objects will automatically start playing; otherwise, <c>false</c>.
		/// </value>
		public bool AutoPlaySlideShow
		{
			get
			{
				if (!this._autoPlaySlideShow.HasValue)
				{
					this._autoPlaySlideShow = (this.GalleryControl.AutoPlaySlideShow.HasValue ? this.GalleryControl.AutoPlaySlideShow.Value : false);
				}

				return this._autoPlaySlideShow.Value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the album tree view is being displayed on this page.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if album tree view is visible; otherwise, <c>false</c>.
		/// </value>
		public bool AlbumTreeViewIsVisible
		{
			get { return _albumTreeViewIsVisible; }
			private set { _albumTreeViewIsVisible = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the footer has been rendered to the page.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the footer has been rendered; otherwise, <c>false</c>.
		/// </value>
		public bool FooterHasBeenRendered
		{
			get
			{
				return _footerHasBeenRendered;
			}
			set
			{
				_footerHasBeenRendered = value;
			}
		}

		///// <summary>
		///// Gets or sets a value indicating whether jQuery is required on this page. When <c>true</c>, the HTML rendered to the
		///// client includes a reference to the jQuery script file. Note that if an administrator has disabled the jQuery setting
		///// by setting the jQuery path to an empty string in the application settings, than the HTML will not include a script
		///// reference, even if this property is <c>true</c>.
		///// </summary>
		///// <value><c>true</c> if jQuery is required on this page; otherwise, <c>false</c>.</value>
		//public bool JQueryRequired
		//{
		//  get { return _jQueryRequired; }
		//  set { _jQueryRequired = value; }
		//}

		/// <summary>
		/// Gets or sets a value indicating whether jQuery is required on this page. When <c>true</c>, the HTML rendered to the
		/// client includes a reference to the jQuery script file. Note that if an administrator has disabled the jQuery setting
		/// by setting the jQuery path to an empty string in the application settings, than the HTML will not include a script
		/// reference, even if this property is <c>true</c>.
		/// </summary>
		/// <value><c>true</c> if jQuery is required on this page; otherwise, <c>false</c>.</value>
		public bool JQueryUiRequired
		{
			get { return _jQueryUiRequired; }
			set { _jQueryUiRequired = value; }
		}

		#endregion

		#region Public Events

		/// <summary>
		/// Occurs just before the gallery header and album breadcrumb menu controls are added to the control collection. This event is an
		/// opportunity for inheritors to insert controls of their own at the zero position using the Controls.AddAt(0, myControl) method.
		/// Viewstate is lost if inheritors add controls at any index other than 0, so the way to deal with this is to use this 
		/// event handler to add controls. For example, the Site Settings admin menu is added in the event handler in the <see cref="AdminPage"/> class.
		/// </summary>
		protected event EventHandler BeforeHeaderControlsAdded;

		#endregion

		#region Public Methods

		/// <overloads>
		/// Gets the album ID corresponding to the current album.
		/// </overloads>
		/// <summary>
		/// Gets the album ID corresponding to the current album. The value is determined in the following sequence: (1) If 
		/// <see cref="GetMediaObject" /> returns an object (which will happen when a particular media object has been requested), then 
		/// use the album ID of the media object's parent. (2) When no media object is available, then look for the "aid" query string 
		/// parameter. (3) If not there, or if <see cref="Gallery.AllowUrlOverride" /> has been set to <c>false</c>, look for an album 
		/// ID on the containing <see cref="Gallery" /> control. (4) If we haven't found an album yet, load the top-level album 
		/// for which the current user has view permission. This function verifies the album exists and the current user has permission 
		/// to view it. If the album does not exist, a <see cref="InvalidAlbumException" /> is thrown. If the user does not have permission to
		/// view the album, a <see cref="GallerySecurityException" /> is thrown. Guaranteed to return a valid album ID, except
		/// when the user does not have view permissions to any album and when the top-level album is a virtual album, in which case
		/// it returns <see cref="Int32.MinValue" />.
		/// </summary>
		/// <returns>Returns the album ID corresponding to the current album.</returns>
		/// <exception cref="InvalidAlbumException">Thrown when the requested album does not exist.</exception>
		/// <exception cref="GallerySecurityException">Thrown when the user is requesting an album or media object they don't have 
		/// permission to view.</exception>
		public int GetAlbumId()
		{
			if (_album != null)
			{
				return _album.Id;
			}
			else
			{
				return GetAlbumId(out _album);
			}
		}

		/// <summary>
		/// Gets the album ID corresponding to the current album and assigns the album to the <paramref name="album" /> parameter. 
		/// The value is determined in the following sequence: (1) If <see cref="GalleryPage.GetMediaObject"/> returns an 
		/// object (which will happen when a particular media object has been requested), then use the album ID of the 
		/// media object's parent. (2) When no media object is available, then look for the "aid" query string parameter.
		/// (3) If not there, or if <see cref="Gallery.AllowUrlOverride"/> has been set to <c>false</c>, look for an album
		/// ID on the containing <see cref="Gallery"/> control. (4) If we haven't found an album yet, load the top-level album
		/// for which the current user has view permission. This function verifies the album exists and the current user has permission
		/// to view it. If the album does not exist, a <see cref="InvalidAlbumException" /> is thrown. If the user does not have permission to
		/// view the album, a <see cref="GallerySecurityException" /> is thrown. Guaranteed to return a valid album ID, except
		/// when the user does not have view permissions to any album and when the top-level album is a virtual album, in which case
		/// it returns <see cref="Int32.MinValue"/>.
		/// </summary>
		/// <param name="album">The album associated with the current page.</param>
		/// <returns>
		/// Returns the album ID corresponding to the current album. 
		/// </returns>
		/// <exception cref="InvalidAlbumException">Thrown when the requested album does not exist.</exception>
		/// <exception cref="GallerySecurityException">Thrown when the user is requesting an album or media object they don't have 
		/// permission to view.</exception>
		public int GetAlbumId(out IAlbum album)
		{
			if (_album != null)
			{
				album = _album;
				return album.Id;
			}

			int aid;

			// If we have a media object, get it's album ID.
			IGalleryObject mediaObject = GetMediaObject();
			if (mediaObject != null)
			{
				aid = mediaObject.Parent.Id;
			}
			else
			{
				aid = ParseAlbumId();
			}

			if (aid > int.MinValue)
			{
				ValidateAlbum(aid, out _album);
			}
			else
			{
				// Nothing in viewstate, the query string, and no media object is specified. Get the highest album the user can view.
				_album = GetHighestAlbumUserCanView();
				aid = _album.Id;
			}

			album = _album;

			return aid;
		}

		/// <overloads>
		/// Get a fully inflated album instance for the requested album.
		/// </overloads>
		/// <summary>
		/// Get a fully inflated, read-only album instance for the requested album. The album can be specified in the following places:  (1) Through 
		/// the <see cref="Gallery.AlbumId" /> property of the Gallery user control (2) From the requested media object by accessing its 
		/// parent object (3) Through the "aid" query string parameter. If this album contains child objects, they are added but not inflated. 
		/// If the album does not exist, a <see cref="InvalidAlbumException" /> is thrown. If the user does not have permission to
		/// view the album, a <see cref="GallerySecurityException" /> is thrown. Guaranteed to never return null.
		/// </summary>
		/// <returns>Returns an IAlbum object.</returns>
		/// <exception cref="InvalidAlbumException">Thrown when the requested album does not exist.</exception>
		/// <exception cref="GallerySecurityException">Thrown when the user is requesting an album or media object they don't have 
		/// permission to view.</exception>
		public IAlbum GetAlbum()
		{
			return GetAlbum(false);
		}

		/// <summary>
		/// Get a fully inflated album instance for the requested album. Specify <c>true</c> for the <paramref name="isWritable"/>
		/// parameter to get an instance that can be modified. The album can be specified in the following places:  (1) Through
		/// the <see cref="Gallery.AlbumId"/> property of the Gallery user control (2) From the requested media object by accessing its
		/// parent object (3) Through the "aid" query string parameter. If this album contains child objects, they are added but not 
		/// inflated. If the album does not exist, a <see cref="InvalidAlbumException" /> is thrown. If the user does not have permission to
		/// view the album, a <see cref="GallerySecurityException" /> is thrown. Guaranteed to never return null.
		/// </summary>
		/// <param name="isWritable">if set to <c>true</c> return an updateable instance.</param>
		/// <returns>Returns an IAlbum object.</returns>
		/// <exception cref="InvalidAlbumException">Thrown when the requested album does not exist.</exception>
		/// <exception cref="GallerySecurityException">Thrown when the user is requesting an album or media object they don't have 
		/// permission to view.</exception>
		public IAlbum GetAlbum(bool isWritable)
		{
			if (isWritable)
			{
				return AlbumController.LoadAlbumInstance(GetAlbumId(), true, isWritable);
			}

			if (this._album == null)
			{
				int albumId = GetAlbumId(); // Getting the album ID will set the _album variable.

				if (this._album == null)
					throw new InvalidOperationException("Retrieving the album ID should have also assigned an album to the _album member variable, but it did not.");
			}

			return this._album;
		}

		//public void SetAlbumId(int albumId)
		//{
		//  ValidateAlbum(albumId);

		//  ViewState["aid"] = albumId;
		//  this._mediaObject = null;
		//  this._album = null;
		//  this._galleryId = int.MinValue;
		//}

		/// <summary>
		/// Gets the media object ID corresponding to the current media object, or <see cref="Int32.MinValue" /> if no valid media 
		/// object is available. The value is determined in the following sequence: (1) See if code earlier in the page's life cycle
		/// assigned an ID to the class member variable (this happens during Ajax postbacks). (2) Look for the "moid" query string parameter.
		/// (3) If not there, or if <see cref="Gallery.AllowUrlOverride" /> has been set to <c>false</c>, look at the <see cref="Gallery" />
		/// control to see if we need to get a media object. This function verifies the media object exists and the 
		/// current user has permission to view it. If either is not true, the function returns <see cref="Int32.MinValue"/>.
		/// </summary>
		/// <returns>Returns the media object ID corresponding to the current media object, or <see cref="Int32.MinValue" /> if 
		/// no valid media object is available.</returns>
		public int GetMediaObjectId()
		{
			if (_mediaObject != null)
			{
				return _mediaObject.Id; // We already figured out the media object for this page instance, so just get the ID.
			}

			int moid;

			// See if it has been assigned to the member variable. This happens during Ajax postbacks.
			if (this._mediaObjectId.HasValue)
			{
				moid = this._mediaObjectId.Value;
			}
			else
			{
				// Try to figure it out based on the query string and various <see cref="Gallery" /> control properties.
				this._mediaObjectId = ParseMediaObjectId();
				moid = this._mediaObjectId.Value;
			}

			if ((moid > int.MinValue) && !ValidateMediaObject(moid, out _mediaObject))
			{
				// Media object is not valid or user does not have permission to view it. Default to int.MinValue.
				moid = int.MinValue;
			}

			return moid;
		}

		/// <summary>
		/// Get a fully inflated, properly typed media object instance for the requested media object. The media object can be specified 
		/// in the following places:  (1) Through the <see cref="Gallery.MediaObjectId" /> property of the Gallery user control (2) Through 
		/// the "moid" query string parameter. If the requested media object doesn't exist or the user does not have permission to view it, 
		/// a null value is returned. An automatic security check is performed to make sure the user has view permission for the specified 
		/// media object.
		/// </summary>
		/// <returns>Returns an <see cref="IGalleryObject" /> object that represents the relevant derived media object type 
		/// (e.g. <see cref="Image" />, <see cref="Video" />, etc), or null if no media object is specified.</returns>
		public IGalleryObject GetMediaObject()
		{
			if (this._mediaObject == null)
			{
				int mediaObjectId = GetMediaObjectId(); // If a media object has been requested, getting its ID will set the _mediaObject variable.

				if ((mediaObjectId > int.MinValue) && this._mediaObject == null)
					throw new InvalidOperationException("Retrieving the media object ID should have also assigned a media object to the _mediaObject member variable, but it did not.");
			}

			return this._mediaObject;
		}

		/// <summary>
		/// Get the URL to the thumbnail image of the specified gallery object. Either a media object or album may be specified. Example:
		/// /dev/gs/handler/getmediaobject.ashx?moid=34&amp;dt=1&amp;g=1
		/// The URL can be used to assign to the src attribute of an image tag (&lt;img src='...' /&gt;).
		/// </summary>
		/// <param name="galleryObject">The gallery object for which an URL to its thumbnail image is to be generated.
		/// Either a media object or album may be specified.</param>
		/// <returns>Returns the URL to the thumbnail image of the specified gallery object.</returns>
		public string GetThumbnailUrl(IGalleryObject galleryObject)
		{
			if (galleryObject is Album)
				return GetAlbumThumbnailUrl(galleryObject);
			else
				return GetMediaObjectUrl(galleryObject, DisplayObjectType.Thumbnail);
		}

		/// <summary>
		/// Get the URL to the optimized image of the specified gallery object. Example:
		/// /dev/gs/handler/getmediaobject.ashx?moid=34&amp;dt=1&amp;g=1
		/// The URL can be used to assign to the src attribute of an image tag (&lt;img src='...' /&gt;).
		/// </summary>
		/// <param name="galleryObject">The gallery object for which an URL to its optimized image is to be generated.</param>
		/// <returns>Returns the URL to the optimized image of the specified gallery object.</returns>
		public static string GetOptimizedUrl(IGalleryObject galleryObject)
		{
			return GetMediaObjectUrl(galleryObject, DisplayObjectType.Optimized);
		}

		/// <summary>
		/// Get the URL to the original image of the specified gallery object. Example:
		/// /dev/gs/handler/getmediaobject.ashx?moid=34&amp;dt=1&amp;g=1
		/// The URL can be used to assign to the src attribute of an image tag (&lt;img src='...' /&gt;).
		/// </summary>
		/// <param name="galleryObject">The gallery object for which an URL to its original image is to be generated.</param>
		/// <returns>Returns the URL to the original image of the specified gallery object.</returns>
		public static string GetOriginalUrl(IGalleryObject galleryObject)
		{
			return GetMediaObjectUrl(galleryObject, DisplayObjectType.Original);
		}

		/// <summary>
		/// Get the URL to the thumbnail, optimized, or original media object. Example:
		/// /dev/gs/handler/getmediaobject.ashx?moid=34&amp;dt=1&amp;g=1
		/// The URL can be used to assign to the src attribute of an image tag (&lt;img src='...' /&gt;).
		/// Not tested: It should be possible to pass an album and request the url to its thumbnail image.
		/// </summary>
		/// <param name="galleryObject">The gallery object for which an URL to the specified image is to be generated.</param>
		/// <param name="displayType">A DisplayObjectType enumeration value indicating the version of the
		/// object for which the URL should be generated. Possible values: Thumbnail, Optimized, Original.
		/// An exception is thrown if any other enumeration is passed.</param>
		/// <returns>Returns the URL to the thumbnail, optimized, or original version of the requested media object.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryObject" /> is null.</exception>
		public static string GetMediaObjectUrl(IGalleryObject galleryObject, DisplayObjectType displayType)
		{
			if (galleryObject == null)
				throw new ArgumentNullException("galleryObject");

			return MediaObjectHtmlBuilder.GenerateUrl(galleryObject.GalleryId, galleryObject.Id, displayType);
		}

		/// <summary>
		/// Remove all HTML tags from the specified string and HTML-encodes the result.
		/// </summary>
		/// <param name="textWithHtml">The string containing HTML tags to remove.</param>
		/// <returns>Returns a string with all HTML tags removed, including the brackets.</returns>
		/// <returns>Returns an HTML-encoded string with all HTML tags removed.</returns>
		public string RemoveHtmlTags(string textWithHtml)
		{
			// Return the text with all HTML removed.
			return Utils.HtmlEncode(Utils.RemoveHtmlTags(textWithHtml));
		}

		/// <overloads>
		/// Throw a <see cref="GallerySecurityException" /> if the current user does not have the permission to perform the requested action.
		/// </overloads>
		/// <summary>
		/// Check to ensure user has permission to perform at least one of the specified security actions against the current album 
		/// (identified in <see cref="GetAlbumId()" />). Throw a <see cref="GallerySecurityException" />
		/// if the permission isn't granted to the logged on user. Un-authenticated users (anonymous users) are always considered 
		/// NOT authorized (that is, this method returns false) except when the requested security action is <see cref="SecurityActions.ViewAlbumOrMediaObject" /> 
		/// or <see cref="SecurityActions.ViewOriginalImage" />, since Gallery Server is configured by default to allow anonymous viewing access but it does 
		/// not allow anonymous editing of any kind. This method behaves similarly to <see cref="IsUserAuthorized(SecurityActions)" /> except that it throws an
		/// exception instead of returning false when the user is not authorized.
		/// </summary>
		/// <param name="securityActions">Represents the permission or permissions being requested. Multiple actions can be specified by using 
		/// a bitwise OR between them (example: <see cref="SecurityActions.AdministerSite" /> | <see cref="SecurityActions.AdministerGallery" />).
		/// If multiple actions are specified, the method is successful if the user has permission for at least one of the actions. If you require 
		/// that all actions be satisfied to be successful, call one of the overloads that accept a <see cref="SecurityActionsOption" /> and 
		/// specify <see cref="SecurityActionsOption.RequireAll" />.</param>
		/// <exception cref="GalleryServerPro.ErrorHandler.CustomExceptions.GallerySecurityException">Thrown when the logged on user 
		/// does not belong to a role that authorizes the specified security action, or if an anonymous user is requesting any permission 
		/// other than a viewing-related permission (i.e., <see cref="SecurityActions.ViewAlbumOrMediaObject" /> or 
		/// <see cref="SecurityActions.ViewOriginalImage" />).</exception>
		public void CheckUserSecurity(SecurityActions securityActions)
		{
			CheckUserSecurity(securityActions, SecurityActionsOption.RequireOne);
		}

		/// <summary>
		/// Check to ensure user has permission to perform the specified security actions against the current album (identified in 
		/// <see cref="GetAlbumId()" />). Throw a <see cref="GallerySecurityException"/>
		/// if the permission isn't granted to the logged on user. When multiple security actions are passed, use 
		/// <paramref name="secActionsOption" /> to specify whether all of the actions must be satisfied to be successful or only one item
		/// must be satisfied. Un-authenticated users (anonymous users) are always considered NOT authorized (that is, this method 
		/// returns false) except when the requested security action is <see cref="SecurityActions.ViewAlbumOrMediaObject"/> or 
		/// <see cref="SecurityActions.ViewOriginalImage"/>, since Gallery Server is configured by default to allow anonymous viewing access 
		/// but it does not allow anonymous editing of any kind. This method behaves similarly to 
		/// <see cref="IsUserAuthorized(SecurityActions, SecurityActionsOption)"/> except that 
		/// it throws an exception instead of returning false when the user is not authorized.
		/// </summary>
		/// <param name="securityActions">Represents the permission or permissions being requested. Multiple actions can be specified by using
		/// a bitwise OR between them (example: <see cref="SecurityActions.AdministerSite" /> | <see cref="SecurityActions.AdministerGallery" />). 
		/// If multiple actions are specified, use <paramref name="secActionsOption" /> to specify whether all of the actions must be satisfied 
		/// to be successful or only one item must be satisfied.</param>
		/// <param name="secActionsOption">Specifies whether the user must have permission for all items in <paramref name="securityActions" />
		/// to be successful or just one. This parameter is applicable only when <paramref name="securityActions" /> contains more than one item.</param>
		/// <exception cref="GalleryServerPro.ErrorHandler.CustomExceptions.GallerySecurityException">Thrown when the logged on user
		/// does not belong to a role that authorizes the specified security action, or if an anonymous user is requesting any permission
		/// other than a viewing-related permission (i.e., <see cref="SecurityActions.ViewAlbumOrMediaObject"/> or
		/// <see cref="SecurityActions.ViewOriginalImage"/>).</exception>
		public void CheckUserSecurity(SecurityActions securityActions, SecurityActionsOption secActionsOption)
		{
			if (!Utils.IsUserAuthorized(securityActions, GetGalleryServerRolesForUser(), this.GetAlbumId(), this.GalleryId, this.GetAlbum().IsPrivate, secActionsOption))
			{
				if (this.IsAnonymousUser)
				{
					throw new GallerySecurityException(String.Format(CultureInfo.CurrentCulture, "Anonymous user does not have permission '{0}' for album ID {1}.", securityActions.ToString(), this.GetAlbumId()));
				}
				else
				{
					throw new GallerySecurityException(String.Format(CultureInfo.CurrentCulture, "User '{0}' does not have permission '{1}' for album ID {2}.", Utils.UserName, securityActions.ToString(), this.GetAlbumId()));
				}
			}
		}

		/// <summary>
		/// Check to ensure user has permission to perform at least one of the specified security actions for the specified <paramref name="album" />. 
		/// Throw a <see cref="GallerySecurityException" /> if the permission isn't granted to the logged on user. Un-authenticated users 
		/// (anonymous users) are always considered NOT authorized (that is, this method returns false) except when the requested security 
		/// action is <see cref="SecurityActions.ViewAlbumOrMediaObject" /> or <see cref="SecurityActions.ViewOriginalImage" />, since 
		/// Gallery Server is configured by default to allow anonymous viewing access but it does not allow anonymous editing of any kind. 
		/// This method behaves similarly to <see cref="IsUserAuthorized(SecurityActions, IAlbum)" /> except that it throws an exception 
		/// instead of returning false when the user is not authorized.
		/// </summary>
		/// <param name="securityActions">Represents the permission or permissions being requested. Multiple actions can be specified by using 
		/// a bitwise OR between them (example: <see cref="SecurityActions.AdministerSite" /> | <see cref="SecurityActions.AdministerGallery" />). 
		/// If multiple actions are specified, the method is successful if the user has permission for at least one of the actions. If you require 
		/// that all actions be satisfied to be successful, call one of the overloads that accept a <see cref="SecurityActionsOption" /> and 
		/// specify <see cref="SecurityActionsOption.RequireAll" />.</param>
		/// <param name="album">The album for which the security check is to be applied.</param>
		/// <exception cref="GalleryServerPro.ErrorHandler.CustomExceptions.GallerySecurityException">Thrown when the logged on user
		/// does not belong to a role that authorizes the specified security action, or if an anonymous user is requesting any permission
		/// other than a viewing-related permission (i.e., <see cref="SecurityActions.ViewAlbumOrMediaObject"/> or
		/// <see cref="SecurityActions.ViewOriginalImage"/>).</exception>
		public void CheckUserSecurity(SecurityActions securityActions, IAlbum album)
		{
			CheckUserSecurity(securityActions, album, SecurityActionsOption.RequireOne);
		}

		/// <summary>
		/// Check to ensure user has permission to perform the specified security actions for the specified <paramref name="album" />. 
		/// Throw a <see cref="GallerySecurityException" /> if the permission isn't granted to the logged on user. When multiple 
		/// security actions are passed, use <paramref name="secActionsOption" /> to specify whether all of the actions must be 
		/// satisfied to be successful or only one item must be satisfied. Un-authenticated users (anonymous users) are always 
		/// considered NOT authorized (that is, this method returns false) except when the requested security action is 
		/// <see cref="SecurityActions.ViewAlbumOrMediaObject"/> or <see cref="SecurityActions.ViewOriginalImage"/>, since Gallery 
		/// Server is configured by default to allow anonymous viewing access but it does not allow anonymous editing of any kind. 
		/// This method behaves similarly to <see cref="IsUserAuthorized(SecurityActions, IAlbum, SecurityActionsOption)"/> except 
		/// that it throws an exception instead of returning false when the user is not authorized.
		/// </summary>
		/// <param name="securityActions">Represents the permission or permissions being requested. Multiple actions can be specified by using
		/// a bitwise OR between them (example: <see cref="SecurityActions.AdministerSite" /> | <see cref="SecurityActions.AdministerGallery" />). 
		/// If multiple actions are specified, use <paramref name="secActionsOption" /> to specify whether all of the actions must be satisfied 
		/// to be successful or only one item must be satisfied.</param>
		/// <param name="album">The album for which the security check is to be applied.</param>
		/// <param name="secActionsOption">Specifies whether the user must have permission for all items in <paramref name="securityActions" />
		/// to be successful or just one. This parameter is applicable only when <paramref name="securityActions" /> contains more than one item.</param>
		/// <exception cref="GalleryServerPro.ErrorHandler.CustomExceptions.GallerySecurityException">Thrown when the logged on user
		/// does not belong to a role that authorizes the specified security action, or if an anonymous user is requesting any permission
		/// other than a viewing-related permission (i.e., <see cref="SecurityActions.ViewAlbumOrMediaObject"/> or
		/// <see cref="SecurityActions.ViewOriginalImage"/>).</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="album" /> is null.</exception>
		public void CheckUserSecurity(SecurityActions securityActions, IAlbum album, SecurityActionsOption secActionsOption)
		{
			if (album == null)
				throw new ArgumentNullException("album");

			if (!Utils.IsUserAuthorized(securityActions, GetGalleryServerRolesForUser(), album.Id, album.GalleryId, album.IsPrivate, secActionsOption))
			{
				if (this.IsAnonymousUser)
				{
					throw new GallerySecurityException(String.Format(CultureInfo.CurrentCulture, "Anonymous user does not have permission '{0}' for album ID {1}.", securityActions.ToString(), album.Id));
				}
				else
				{
					throw new GallerySecurityException(String.Format(CultureInfo.CurrentCulture, "User '{0}' does not have permission '{1}' for album ID {2}.", Utils.UserName, securityActions.ToString(), album.Id));
				}
			}
		}

		/// <overloads>
		/// Determine if the current user has permission to perform the requested action.
		/// </overloads>
		/// <summary>
		/// Determine whether user has permission to perform at least one of the specified security actions against the current album 
		/// (identified in <see cref="GetAlbumId()" />). Un-authenticated users (anonymous users) are always considered NOT authorized (that 
		/// is, this method returns false) except when the requested security action is <see cref="SecurityActions.ViewAlbumOrMediaObject" /> 
		/// or <see cref="SecurityActions.ViewOriginalImage" />, since Gallery Server is configured by default to allow anonymous viewing 
		/// access but it does not allow anonymous editing of any kind.
		/// </summary>
		/// <param name="securityActions">Represents the permission or permissions being requested. Multiple actions can be specified by using 
		/// a bitwise OR between them (example: <see cref="SecurityActions.AdministerSite" /> | <see cref="SecurityActions.AdministerGallery" />). 
		/// If multiple actions are specified, the method is successful if the user has permission for at least one of the actions. If you require 
		/// that all actions be satisfied to be successful, call one of the overloads that accept a <see cref="SecurityActionsOption" /> and 
		/// specify <see cref="SecurityActionsOption.RequireAll" />.</param>
		/// <returns>Returns true when the user is authorized to perform the specified security action; otherwise returns false.</returns>
		public bool IsUserAuthorized(SecurityActions securityActions)
		{
			return IsUserAuthorized(securityActions, SecurityActionsOption.RequireOne);
		}

		/// <summary>
		/// Determine whether user has permission to perform the specified security actions against the current album (identified in 
		/// <see cref="GetAlbumId()" />). When multiple security actions are passed, use 
		/// <paramref name="secActionsOption" /> to specify whether all of the actions must be satisfied to be successful or only one item
		/// must be satisfied. Un-authenticated users (anonymous users) are always considered NOT authorized (that 
		/// is, this method returns false) except when the requested security action is <see cref="SecurityActions.ViewAlbumOrMediaObject" /> 
		/// or <see cref="SecurityActions.ViewOriginalImage" />, since Gallery Server is configured by default to allow anonymous viewing 
		/// access but it does not allow anonymous editing of any kind.
		/// </summary>
		/// <param name="securityActions">Represents the permission or permissions being requested. Multiple actions can be specified by using
		/// a bitwise OR between them (example: <see cref="SecurityActions.AdministerSite" /> | <see cref="SecurityActions.AdministerGallery" />). 
		/// If multiple actions are specified, use <paramref name="secActionsOption" /> to specify whether all of the actions must be satisfied 
		/// to be successful or only one item must be satisfied. This parameter is applicable only when <paramref name="securityActions" /> 
		/// contains more than one item.</param>
		/// <param name="secActionsOption">Specifies whether the user must have permission for all items in <paramref name="securityActions" />
		/// to be successful or just one.</param>
		/// <returns>Returns true when the user is authorized to perform the specified security action; otherwise returns false.</returns>
		public bool IsUserAuthorized(SecurityActions securityActions, SecurityActionsOption secActionsOption)
		{
			return Utils.IsUserAuthorized(securityActions, GetGalleryServerRolesForUser(), this.GetAlbumId(), this.GalleryId, this.GetAlbum().IsPrivate, secActionsOption);
		}

		/// <summary>
		/// Determine whether user has permission to perform at least one of the specified security actions. Un-authenticated users (anonymous users) are
		/// always considered NOT authorized (that is, this method returns false) except when the requested security action is
		/// <see cref="SecurityActions.ViewAlbumOrMediaObject" /> or <see cref="SecurityActions.ViewOriginalImage" />, 
		/// since Gallery Server is configured by default to allow anonymous viewing access but it does not allow anonymous editing of 
		/// any kind. This method will continue to work correctly if the webmaster configures Gallery Server to require users to log 
		/// in in order to view objects, since at that point there will be no such thing as un-authenticated users, and the standard 
		/// gallery server role functionality applies.
		/// </summary>
		/// <param name="securityActions">Represents the permission or permissions being requested. Multiple actions can be specified by using 
		/// 	a bitwise OR between them (example: <see cref="SecurityActions.AdministerSite" /> | <see cref="SecurityActions.AdministerGallery" />). 
		/// 	If multiple actions are specified, the method is successful if the user has permission for at least one of the actions.</param>
		/// <param name="albumId">The album ID to which the security action applies.</param>
		/// <param name="galleryId">The ID for the gallery the user is requesting permission in. The <paramref name="albumId" /> must exist 
		/// 	in this gallery.</param>
		/// <returns>
		/// Returns true when the user is authorized to perform the specified security action against the specified album;
		/// otherwise returns false.
		/// </returns>
		/// <exception cref="NotSupportedException">Thrown when <paramref name="securityActions" /> is <see cref="SecurityActions.ViewAlbumOrMediaObject" /> 
		/// or <see cref="SecurityActions.ViewOriginalImage" /> and the user is anonymous (not logged on).</exception>
		internal bool IsUserAuthorized(SecurityActions securityActions, int albumId, int galleryId)
		{
			if (((securityActions == SecurityActions.ViewAlbumOrMediaObject) || (securityActions == SecurityActions.ViewOriginalImage))
					&& (!Utils.IsAuthenticated))
				throw new NotSupportedException("Wrong method call: You must call the overload of GalleryPage.IsUserAuthorized that has the isPrivate parameter when the security action is ViewAlbumOrMediaObject or ViewOriginalImage and the user is anonymous (not logged on).");

			return IsUserAuthorized(securityActions, albumId, galleryId, false);
		}

		/// <summary>
		/// Determine whether user has permission to perform at least one of the specified security actions. Un-authenticated users (anonymous users) are
		/// always considered NOT authorized (that is, this method returns false) except when the requested security action is
		/// <see cref="SecurityActions.ViewAlbumOrMediaObject" /> or <see cref="SecurityActions.ViewOriginalImage" />, 
		/// since Gallery Server is configured by default to allow anonymous viewing access but it does not allow anonymous editing of 
		/// any kind. This method will continue to work correctly if the webmaster configures Gallery Server to require users to log 
		/// in in order to view objects, since at that point there will be no such thing as un-authenticated users, and the standard 
		/// gallery server role functionality applies.
		/// </summary>
		/// <param name="securityActions">Represents the permission or permissions being requested. Multiple actions can be specified by using 
		/// a bitwise OR between them (example: <see cref="SecurityActions.AdministerSite" /> | <see cref="SecurityActions.AdministerGallery" />). 
		/// If multiple actions are specified, the method is successful if the user has permission for at least one of the actions.</param>
		/// <param name="albumId">The album ID to which the security action applies.</param>
		/// <param name="galleryId">The ID for the gallery the user is requesting permission in. The <paramref name="albumId" /> must exist 
		/// in this gallery.</param>
		/// <param name="isPrivate">Indicates whether the specified album is private (hidden from anonymous users). The parameter
		/// 	is ignored for logged on users.</param>
		/// <returns>
		/// Returns true when the user is authorized to perform the specified security action against the specified album;
		/// otherwise returns false.
		/// </returns>
		internal bool IsUserAuthorized(SecurityActions securityActions, int albumId, int galleryId, bool isPrivate)
		{
			return Utils.IsUserAuthorized(securityActions, GetGalleryServerRolesForUser(), albumId, galleryId, isPrivate);
		}

		/// <summary>
		/// Determine whether user has permission to perform at least one of the specified security actions against the specified <paramref name="album" />. 
		/// Un-authenticated users (anonymous users) are always considered NOT authorized (that is, this method returns false) except 
		/// when the requested security action is <see cref="SecurityActions.ViewAlbumOrMediaObject" /> or 
		/// <see cref="SecurityActions.ViewOriginalImage" />, since Gallery Server is configured by default to allow anonymous viewing access 
		/// but it does not allow anonymous editing of any kind.
		/// </summary>
		/// <param name="securityActions">Represents the permission or permissions being requested. Multiple actions can be specified by using 
		/// a bitwise OR between them (example: <see cref="SecurityActions.AdministerSite" /> | <see cref="SecurityActions.AdministerGallery" />). 
		/// If multiple actions are specified, the method is successful if the user has permission for at least one of the actions. If you require 
		/// that all actions be satisfied to be successful, call one of the overloads that accept a <see cref="SecurityActionsOption" /> and 
		/// specify <see cref="SecurityActionsOption.RequireAll" />.</param>
		/// <param name="album">The album for which the security check is to be applied.</param>
		/// <returns>Returns true when the user is authorized to perform the specified security action; otherwise returns false.</returns>
		public bool IsUserAuthorized(SecurityActions securityActions, IAlbum album)
		{
			return IsUserAuthorized(securityActions, album, SecurityActionsOption.RequireOne);
		}

		/// <summary>
		/// Determine whether user has permission to perform the specified security action against the specified album. If no album 
		/// is specified, then the current album (as returned by GetAlbum()) is used. Un-authenticated users (anonymous users) are 
		/// always considered NOT authorized (that is, this method returns false) except when the requested security action is 
		/// ViewAlbumOrMediaObject or ViewOriginalImage, since Gallery Server is configured by default to allow anonymous viewing access
		/// but it does not allow anonymous editing of any kind.
		/// </summary>
		/// <param name="securityActions">Represents the permission or permissions being requested. Multiple actions can be specified by using
		/// a bitwise OR between them (example: <see cref="SecurityActions.AdministerSite" /> | <see cref="SecurityActions.AdministerGallery" />). 
		/// If multiple actions are specified, use <paramref name="secActionsOption" /> to specify whether all of the actions must be satisfied 
		/// to be successful or only one item must be satisfied.</param>
		/// <param name="album">The album for which the security check is to be applied.</param>
		/// <param name="secActionsOption">Specifies whether the user must have permission for all items in <paramref name="securityActions" />
		/// to be successful or just one. This parameter is applicable only when <paramref name="securityActions" /> contains more than one item.</param>
		/// <returns>Returns true when the user is authorized to perform the specified security action; otherwise returns false.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="album" /> is null.</exception>
		public bool IsUserAuthorized(SecurityActions securityActions, IAlbum album, SecurityActionsOption secActionsOption)
		{
			if (album == null)
				throw new ArgumentNullException("album");

			return Utils.IsUserAuthorized(securityActions, GetGalleryServerRolesForUser(), album.Id, album.GalleryId, album.IsPrivate, secActionsOption);
		}

		/// <summary>
		/// Gets Gallery Server roles representing the roles for the currently logged-on user and belonging to the current gallery. 
		/// Returns an empty collection if no user is logged in or the user is logged in but not assigned to any roles relevant 
		/// to the current gallery (Count = 0).
		/// </summary>
		/// <returns>Returns a collection of Gallery Server roles representing the roles for the currently logged-on user. 
		/// Returns an empty collection if no user is logged in or the user is logged in but not assigned to any roles relevant 
		/// to the current gallery (Count = 0).</returns>
		[DataObjectMethod(DataObjectMethodType.Select)]
		public IGalleryServerRoleCollection GetGalleryServerRolesForUser()
		{
			if (this._roles == null)
			{
				this._roles = RoleController.GetGalleryServerRolesForUser();
			}

			return this._roles;
		}

		/// <overloads>
		/// Redirect the user to the previous page he or she was on, optionally appending a query string name/value.
		/// </overloads>
		/// <summary>
		/// Redirect the user to the previous page he or she was on. The previous page is retrieved from a session variable that was stored during 
		/// the Page_Init event. If the original query string contains a "msg" parameter, it is removed so that the message 
		/// is not shown again to the user. If no previous page URL is available - perhaps because the user navigated directly to
		/// the page or has just logged in - the user is redirected to the application root.
		/// </summary>
		public void RedirectToPreviousPage()
		{
			RedirectToPreviousPage(String.Empty, String.Empty);
		}

		/// <summary>
		/// Redirect the user to the previous page he or she was on. If a query string name/pair value is specified, append that 
		/// to the URL.
		/// </summary>
		/// <param name="queryStringName">The query string name.</param>
		/// <param name="queryStringValue">The query string value.</param>
		public void RedirectToPreviousPage(string queryStringName, string queryStringValue)
		{
			#region Validation

			if (!String.IsNullOrEmpty(queryStringName) && String.IsNullOrEmpty(queryStringValue))
				throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "The queryStringValue parameter is required when the queryStringName parameter is specified. (queryStringName='{0}', queryStringValue='{1}')", queryStringName, queryStringValue));

			if (!String.IsNullOrEmpty(queryStringValue) && String.IsNullOrEmpty(queryStringName))
				throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "The queryStringName parameter is required when the queryStringValue parameter is specified. (queryStringName='{0}', queryStringValue='{1}')", queryStringName, queryStringValue));

			#endregion

			string url = this.PreviousUrl;

			if (String.IsNullOrEmpty(url))
				url = Utils.GetCurrentPageUrl(); // No previous url is available. Default to the current page.

			if (!String.IsNullOrEmpty(queryStringName))
				url = Utils.AddQueryStringParameter(url, String.Concat(queryStringName, "=", queryStringValue));

			this.PreviousUri = null;

			Page.Response.Redirect(url, true);
		}

		/// <overloads>Redirects to album view page of the current album.</overloads>
		/// <summary>
		/// Redirects to album view page of the current album.
		/// </summary>
		public void RedirectToAlbumViewPage()
		{
			Utils.Redirect(PageId.album, "aid={0}", GetAlbumId());
		}

		/// <summary>
		/// Redirects to album view page of the current album and with the specified <paramref name="args"/> appended as query string 
		/// parameters. Example: If the current page is /dev/gs/gallery.aspx, the user is viewing album 218, <paramref name="format"/> 
		/// is "msg={0}", and <paramref name="args"/> is "23", this function redirects to /dev/gs/gallery.aspx?g=album&amp;aid=218&amp;msg=23.
		/// </summary>
		/// <param name="format">A format string whose placeholders are replaced by values in <paramref name="args"/>. Do not use a '?'
		/// or '&amp;' at the beginning of the format string. Example: "msg={0}".</param>
		/// <param name="args">The values to be inserted into the <paramref name="format"/> string.</param>
		public void RedirectToAlbumViewPage(string format, params object[] args)
		{
			if (format == null)
				format = String.Empty;

			if (format.StartsWith("?", StringComparison.Ordinal))
				format = format.Remove(0, 1); // Remove leading '?' if present

			string queryString = String.Format(CultureInfo.InvariantCulture, format, args);
			if (!queryString.StartsWith("&", StringComparison.Ordinal))
				queryString = String.Concat("&", queryString); // Append leading '&' if not present

			Utils.Redirect(PageId.album, String.Concat("aid={0}", queryString), GetAlbumId());
		}

		/// <summary>
		/// Recursively iterate through the children of the specified containing control, searching for a child control with
		/// the specified server ID. If the control is found, return it; otherwise return null. This method is useful for finding
		/// child controls of composite controls like GridView and ComponentArt's controls.
		/// </summary>
		/// <param name="containingControl">The containing control whose child controls should be searched.</param>
		/// <param name="id">The server ID of the child control to search for.</param>
		/// <returns>Returns a Control matching the specified server id, or null if no matching control is found.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="containingControl" /> is null.</exception>
		/// <exception cref="ArgumentException">Thrown when <paramref name="id" /> is null or an empty string.</exception>
		public Control FindControlRecursive(Control containingControl, string id)
		{
			if (containingControl == null)
				throw new ArgumentNullException("containingControl");

			if (String.IsNullOrEmpty(id))
				throw new ArgumentException("The parameter 'id' is null or empty.");

			foreach (Control ctrl in containingControl.Controls)
			{
				if (ctrl.ID == id)
					return ctrl;

				if (ctrl.HasControls())
				{
					Control foundCtrl = FindControlRecursive(ctrl, id);
					if (foundCtrl != null)
						return foundCtrl;
				}
			}
			return null;
		}

		/// <overloads>Set a page level CSS style defining the width and height of the containers of the thumbnail image of each gallery object.</overloads>
		/// <summary>
		/// Set a page level CSS style defining the width and height of the containers of the thumbnail image of each gallery object.
		/// The width and height is based on the widest thumbnail image and the height of the tallest thumbnail image in the collection of
		/// specified gallery objects, plus a predefined buffer width and height specified by the configuration settings ThumbnailWidthBuffer
		/// and ThumbnailHeightBuffer. The page level style is named "thmb" and is hard-coded to apply to a div tag. The style written to 
		/// the page may look like this:
		/// &lt;style type="text/css"&gt;&lt;!-- div.thmb {width:145px;height:180px;} --&gt;&lt;/style&gt;
		/// </summary>
		/// <param name="galleryObjects">A collection of gallery objects from which the width and height is to be calculated.</param>
		/// <remarks>If the thumbnail images were always the same dimension, the width and height for the thumbnail image container
		/// could be hardcoded in the global style sheet. But since it is variable, we need to programmatically set it.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="galleryObjects"/> is null.</exception>
		public void SetThumbnailCssStyle(IGalleryObjectCollection galleryObjects)
		{
			SetThumbnailCssStyle(galleryObjects, 0, 0, new string[] { });
		}

		/// <summary>
		/// Set a page level CSS style defining the width and height of the containers of the thumbnail image of each gallery object.
		/// The width and height is based on the widest thumbnail image and the height of the tallest thumbnail image in the collection of
		/// specified gallery objects, plus a predefined buffer width and height specified by the configuration settings ThumbnailWidthBuffer
		/// and ThumbnailHeightBuffer. The page level style is applied to <paramref name="thumbnailCssClass"/> and is hard-coded to apply
		/// to a div tag. The style written to the page may look like this:
		/// &lt;style type="text/css"&gt;&lt;!-- div.thmb {width:145px;height:180px;} --&gt;&lt;/style&gt;
		/// </summary>
		/// <param name="galleryObjects">A collection of gallery objects from which the width and height is to be calculated.</param>
		/// <param name="thumbnailCssClass">A string representing a CSS class. The calculated width and height will be applied to this
		/// class and written to the page header as a page level style. If not specified (null), this parameter defaults to "thmb".</param>
		/// <remarks>If the thumbnail images were always the same dimension, the width and height for the thumbnail image container
		/// could be hardcoded in the global style sheet. But since it is variable, we need to programmatically set it.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="galleryObjects"/> is null.</exception>
		public void SetThumbnailCssStyle(IGalleryObjectCollection galleryObjects, string thumbnailCssClass)
		{
			SetThumbnailCssStyle(galleryObjects, 0, 0, new string[] { thumbnailCssClass });
		}

		/// <summary>
		/// Set a page level CSS style defining the width and height of the containers of the thumbnail image of each gallery object.
		/// The width and height is based on the widest thumbnail image and the height of the tallest thumbnail image in the collection of
		/// specified gallery objects, plus a predefined buffer width and height specified by the configuration settings ThumbnailWidthBuffer
		/// and ThumbnailHeightBuffer. The values in <paramref name="widthBuffer"/> and <paramref name="heightBuffer"/> are added to the calculated
		/// width and height. This is useful when extra space is needed; for example, to make room the textbox on the edit captions page
		/// or the rotate icons on the rotate images page. If no thumbnail CSS class is specified, the page level style defaults to 
		/// setting the width and height on a class named "thmb". The style written to the page may look like this:
		/// &lt;style type="text/css"&gt;&lt;!-- div.thmb {width:145px;height:180px;} --&gt;&lt;/style&gt;
		/// </summary>
		/// <param name="galleryObjects">A collection of gallery objects from which the width and height is to be calculated.</param>
		/// <param name="widthBuffer">A value indicating extra horizontal padding for the thumbnail image container. An integer larger
		/// than zero increases the width; less than zero causes the width to decrease from its calculated value. This parameter is 
		/// typically specified when extra space is needed to make room for elements within the thumbnail image container, such as
		/// the textbox on the edit captions page or the rotate icons on the rotate images page.</param>
		/// <param name="heightBuffer">A value indicating extra vertical padding for the thumbnail image container. An integer larger
		/// than zero increases the height; less than zero causes the height to decrease from its calculated value. This parameter is 
		/// typically specified when extra space is needed to make room for elements within the thumbnail image container, such as
		/// the textbox on the edit captions page or the rotate icons on the rotate images page.</param>
		/// <remarks>If the thumbnail images were always the same dimension, the width and height for the thumbnail image container
		/// could be hardcoded in the global style sheet. But since it is variable, we need to programmatically set it.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="galleryObjects"/> is null.</exception>
		public void SetThumbnailCssStyle(IGalleryObjectCollection galleryObjects, int widthBuffer, int heightBuffer)
		{
			SetThumbnailCssStyle(galleryObjects, widthBuffer, heightBuffer, new string[] { });
		}

		/// <summary>
		/// Set a page level CSS style defining the width and height of the containers of the thumbnail image of each gallery object.
		/// The width and height is based on the widest thumbnail image and the height of the tallest thumbnail image in the collection of
		/// specified gallery objects, plus a predefined buffer width and height specified by the configuration settings ThumbnailWidthBuffer
		/// and ThumbnailHeightBuffer. The values in <paramref name="widthBuffer"/> and <paramref name="heightBuffer"/> are added to the calculated
		/// width and height. This is useful when extra space is needed; for example, to make room the textbox on the edit captions page
		/// or the rotate icons on the rotate images page. The page level style is applied to <paramref name="thumbnailCssClass"/> and is 
		/// hard-coded to apply to a div tag. The style written to the page may look like this:
		/// &lt;style type="text/css"&gt;&lt;!-- div.thmb {width:145px;height:180px;} --&gt;&lt;/style&gt;
		/// </summary>
		/// <param name="galleryObjects">A collection of gallery objects from which the width and height is to be calculated.</param>
		/// <param name="widthBuffer">A value indicating extra horizontal padding for the thumbnail image container. An integer larger
		/// than zero increases the width; less than zero causes the width to decrease from its calculated value. This parameter is 
		/// typically specified when extra space is needed to make room for elements within the thumbnail image container, such as
		/// the textbox on the edit captions page or the rotate icons on the rotate images page.</param>
		/// <param name="heightBuffer">A value indicating extra vertical padding for the thumbnail image container. An integer larger
		/// than zero increases the height; less than zero causes the height to decrease from its calculated value. This parameter is 
		/// typically specified when extra space is needed to make room for elements within the thumbnail image container, such as
		/// the textbox on the edit captions page or the rotate icons on the rotate images page.</param>
		/// <param name="thumbnailCssClass">A string representing a CSS class. The calculated width and height will be applied to this
		/// class and written to the page header as a page level style. If not specified (null), this parameter defaults to "thmb".</param>
		/// <remarks>If the thumbnail images were always the same dimension, the width and height for the thumbnail image container
		/// could be hardcoded in the global style sheet. But since it is variable, we need to programmatically set it.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the galleryObjects parameter is null.</exception>
		public void SetThumbnailCssStyle(IGalleryObjectCollection galleryObjects, int widthBuffer, int heightBuffer, string thumbnailCssClass)
		{
			SetThumbnailCssStyle(galleryObjects, widthBuffer, heightBuffer, new string[] { thumbnailCssClass });
		}

		/// <summary>
		/// Set a page level CSS style defining the width and height of the containers of the thumbnail image of each gallery object.
		/// The width and height is based on the widest thumbnail image and the height of the tallest thumbnail image in the collection of
		/// specified gallery objects, plus a predefined buffer width and height specified by the configuration settings ThumbnailWidthBuffer
		/// and ThumbnailHeightBuffer. The values in <paramref name="widthBuffer"/> and <paramref name="heightBuffer"/> are added to the calculated
		/// width and height. This is useful when extra space is needed; for example, to make room the textbox on the edit captions page
		/// or the rotate icons on the rotate images page. The page level style is applied to each of the strings in
		/// <paramref name="thumbnailCssClasses"/> and is hard-coded to apply to a div tag. The style written to the page may look like this:
		/// &lt;style type="text/css"&gt;&lt;!-- div.thmb {width:145px;height:180px;} --&gt;&lt;/style&gt;
		/// </summary>
		/// <param name="galleryObjects">A collection of gallery objects from which the width and height is to be calculated.</param>
		/// <param name="widthBuffer">A value indicating extra horizontal padding for the thumbnail image container. An integer larger
		/// than zero increases the width; less than zero causes the width to decrease from its calculated value. This parameter is 
		/// typically specified when extra space is needed to make room for elements within the thumbnail image container, such as
		/// the textbox on the edit captions page or the rotate icons on the rotate images page.</param>
		/// <param name="heightBuffer">A value indicating extra vertical padding for the thumbnail image container. An integer larger
		/// than zero increases the height; less than zero causes the height to decrease from its calculated value. This parameter is 
		/// typically specified when extra space is needed to make room for elements within the thumbnail image container, such as
		/// the textbox on the edit captions page or the rotate icons on the rotate images page.</param>
		/// <param name="thumbnailCssClasses">A string array of CSS classes. The calculated width and height will be applied to these
		/// classes and written to the page header as a page level style. If not specified (null) or it has a length of zero, this 
		/// parameter defaults to a single string "thmb".</param>
		/// <remarks>If the thumbnail images were always the same dimension, the width and height for the thumbnail image container
		/// could be hardcoded in the global style sheet. But since it is variable, we need to programmatically set it.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="galleryObjects" /> is null.</exception>
		public void SetThumbnailCssStyle(IGalleryObjectCollection galleryObjects, int widthBuffer, int heightBuffer, string[] thumbnailCssClasses)
		{
			if (galleryObjects == null)
				throw new ArgumentNullException("galleryObjects");

			if ((thumbnailCssClasses == null) || (thumbnailCssClasses.Length == 0))
			{
				thumbnailCssClasses = new string[] { "thmb" };
			}

			// Calculate the width of the widest thumbnail image and the height of the tallest thumbnail 
			// image in this album. 
			int maxMoWidth = 0;
			int maxMoHeight = 0;

			foreach (IGalleryObject mo in galleryObjects)
			{
				if (mo.Thumbnail.Width > maxMoWidth)
					maxMoWidth = mo.Thumbnail.Width;

				if (mo.Thumbnail.Height > maxMoHeight)
					maxMoHeight = mo.Thumbnail.Height;
			}

			// If no width or height have been set, set to the default thumbnail width and height so
			// that we have reasonable minimum values.
			if ((maxMoWidth == 0) || (maxMoHeight == 0))
			{
				int maxLength = GallerySettings.MaxThumbnailLength;
				float ratio = GallerySettings.EmptyAlbumThumbnailWidthToHeightRatio;
				if (ratio > 1) // Landscape (width is greater than height)
				{
					maxMoWidth = maxLength;
					maxMoHeight = Convert.ToInt32((float)maxLength / ratio);
				}
				else // Portrait (width is less than height)
				{
					maxMoHeight = maxLength;
					maxMoWidth = Convert.ToInt32((float)maxLength * ratio);
				}
			}

			int maxWidth = maxMoWidth + GallerySettings.ThumbnailWidthBuffer + widthBuffer;
			int maxHeight = maxMoHeight + GallerySettings.ThumbnailHeightBuffer + heightBuffer;

			string pageStyle = "\n<style type=\"text/css\"><!-- ";
			foreach (string cssClass in thumbnailCssClasses)
			{
				pageStyle += String.Format(CultureInfo.CurrentCulture, "div.{0} {{width:{1}px;height:{2}px;}} ", cssClass, maxWidth, maxHeight);
			}
			pageStyle += "--></style>\n";

			this.Page.Header.Controls.Add(new System.Web.UI.LiteralControl(pageStyle));
		}

		/// <summary>
		/// Gets an instance of he usermessage.ascx user control that is formatted and pre-configured with a message for the user.
		/// The message is based on the <see cref="Message"/> property. The control can be added to the control collection of the
		/// page, typically a PlaceHolder contro.
		/// </summary>
		/// <returns>Returns an instance of he usermessage.ascx user control that is formatted and pre-configured with a message 
		/// for the user.</returns>
		public usermessage GetMessageControl()
		{
			const string resourcePrefix = "Msg_";
			const string headerSuffix = "_Hdr";
			const string detailSuffix = "_Dtl";

			string headerMsg = Resources.GalleryServerPro.ResourceManager.GetString(String.Concat(resourcePrefix, this.Message.ToString(), headerSuffix));
			string detailMsg = Resources.GalleryServerPro.ResourceManager.GetString(String.Concat(resourcePrefix, this.Message.ToString(), detailSuffix));

			switch (this.Message)
			{
				case Message.CaptionExceededMaxLength:
				case Message.AlbumNameExceededMaxLength:
					{
						detailMsg = String.Format(CultureInfo.CurrentCulture, detailMsg, DataConstants.AlbumTitleLength);
						break;
					}
				case Message.ObjectsSkippedDuringUpload:
					{
						List<ActionResult> uploadResults = GetUploadErrors(Session[GlobalConstants.SkippedFilesDuringUploadSessionKey] as List<ActionResult>);

						detailMsg = string.Empty;
						if (uploadResults != null)
						{
							// This message is unique in that we need to choose one of two detail messages from the resource file. One is for when a single
							// file has been skipped; the other is when multiple files have been skipped.
							if (uploadResults.Count == 1)
							{
								string detailMsgTemplate = Resources.GalleryServerPro.ResourceManager.GetString(String.Concat(resourcePrefix, this.Message.ToString(), "Single", detailSuffix));
								detailMsg = String.Format(CultureInfo.CurrentCulture, detailMsgTemplate, uploadResults[0].Title, uploadResults[0].Message);
							}
							else if (uploadResults.Count > 1)
							{
								string detailMsgTemplate = Resources.GalleryServerPro.ResourceManager.GetString(String.Concat(resourcePrefix, this.Message.ToString(), "Multiple", detailSuffix));
								detailMsg = String.Format(CultureInfo.CurrentCulture, detailMsgTemplate, ConvertListToHtmlBullets(uploadResults));
							}
						}
						break;
					}
			}
			GalleryServerPro.Web.Controls.usermessage msgBox = (GalleryServerPro.Web.Controls.usermessage)LoadControl(Utils.GetUrl("/controls/usermessage.ascx"));
			msgBox.IconStyle = GalleryServerPro.Web.MessageStyle.Information;
			msgBox.MessageTitle = headerMsg;
			msgBox.MessageDetail = detailMsg;
			msgBox.HeaderCssClass = "um2HeaderCss";
			msgBox.DetailCssClass = "um2DetailCss";
			msgBox.CssClass = "um2ContainerCss";

			if (AlbumTreeViewIsVisible)
			{
				msgBox.CssClass += " gsp_treeviewBuffer";
			}

			return msgBox;
		}

		/// <summary>
		/// Record the error and optionally notify an administrator via e-mail.
		/// </summary>
		/// <param name="ex">The exception to record.</param>
		/// <returns>Returns an integer that uniquely identifies this application error (<see cref="IAppError.AppErrorId"/>).</returns>
		public int LogError(Exception ex)
		{
			return AppErrorController.LogError(ex, this.GalleryId);
		}

		/// <summary>
		/// Gets a collection of users the current user has permission to view. Users who have administer site permission can view all users.
		/// Users with administer gallery permission can only view users in galleries they have gallery admin permission in. Note that
		/// a user may be able to view a user but not update it. This can happen when the user belongs to roles that are associated with
		/// galleries the current user is not an admin for. The users may be returned from a cache. Guaranteed to not return null.
		/// </summary>
		/// <returns>Returns an <see cref="IUserAccountCollection" /> containing a list of roles the user has permission to view.</returns>
		public IUserAccountCollection GetUsersCurrentUserCanView()
		{
			return UserController.GetUsersCurrentUserCanView(UserCanAdministerSite, UserCanAdministerGallery);
		}

		/// <summary>
		/// Gets the list of roles the user has permission to view. Users who have administer site permission can view all roles.
		/// Users with administer gallery permission can only view roles they have been associated with or roles that aren't 
		/// associated with *any* gallery.
		/// </summary>
		/// <returns>Returns an <see cref="IGalleryServerRoleCollection" /> containing a list of roles the user has permission to view.</returns>
		public IGalleryServerRoleCollection GetRolesCurrentUserCanView()
		{
			return RoleController.GetRolesCurrentUserCanView(UserCanAdministerSite, UserCanAdministerGallery);
		}

		/// <overloads>
		/// Gets the HTML to display a nicely formatted thumbnail image of the specified <paramref name="galleryObject" />, including a 
		/// border, shadows and (possibly) rounded corners.
		/// </overloads>
		/// <summary>
		/// Gets the HTML to display a nicely formatted thumbnail image of the specified <paramref name="galleryObject" />, including a 
		/// border, shadows and (possibly) rounded corners. This function is the same as calling the overloaded version with 
		/// includeHyperlinkToObject and allowAlbumTextWrapping parameters both set to <c>false</c>.
		/// </summary>
		/// <param name="galleryObject">The gallery object to be used as the source for the thumbnail image.</param>
		/// <returns>Returns HTML that displays a nicely formatted thumbnail image of the specified <paramref name="galleryObject" /></returns>
		protected string GetThumbnailHtml(IGalleryObject galleryObject)
		{
			return GetThumbnailHtml(galleryObject, false, false);
		}

		/// <summary>
		/// Gets the HTML to display a nicely formatted thumbnail image of the specified <paramref name="galleryObject" />, including a 
		/// border, shadows and (possibly) rounded corners.
		/// </summary>
		/// <param name="galleryObject">The gallery object to be used as the source for the thumbnail image.</param>
		/// <param name="includeHyperlinkToObject">if set to <c>true</c> wrap the image tag with a hyperlink so the user can click through
		/// to the media object view of the item.</param>
		/// <param name="allowAlbumTextWrapping">if set to <c>true</c> the album title is allowed to wrap to a second line if needed.
		/// Set to <c>false</c> when vertical space is limited.</param>
		/// <returns>Returns HTML that displays a nicely formatted thumbnail image of the specified <paramref name="galleryObject" /></returns>
		public string GetThumbnailHtml(IGalleryObject galleryObject, bool includeHyperlinkToObject, bool allowAlbumTextWrapping)
		{
			return MediaObjectHtmlBuilder.GenerateThumbnailHtml(galleryObject, GallerySettings, Request.Browser, includeHyperlinkToObject, allowAlbumTextWrapping);
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Sends server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter"/> object, which writes the content to be rendered on the client.
		/// </summary>
		/// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the server control content.</param>
		protected override void Render(System.Web.UI.HtmlTextWriter writer)
		{
			// Wrap HTML in an enclosing <div id="gsp_container" class="gsp_ns"> tag. The CSS class 'gsp_ns' is used as a pseudo namespace 
			// that is used to limit the influence CSS has to only the Gallery Server code, thus preventing the CSS from affecting 
			// HTML that may exist in the master page or other areas outside the user control.
			writer.AddAttribute("id", "gsp_container");
			writer.AddAttribute("class", "gsp_ns");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);

			// Write out the HTML for this control.
			base.Render(writer);

			// Add the GSP logo to the end
			if (!AppSetting.Instance.ProductKey.Equals(GlobalConstants.ProductKeyNoPageFooter))
			{
				AddGspLogo(writer);

				FooterHasBeenRendered = true;
			}

			// Close out the <div> tag.
			writer.RenderEndTag();
		}

		#endregion

		#region Internal Static Methods

		/// <summary>
		/// Get information for the specified media object, including its previous and next media object.
		/// </summary>
		/// <param name="mediaObject">The media object.</param>
		/// <param name="displayType">The type of display object to receive (thumbnail, optimized, original).</param>
		/// <param name="isCallBack">Indicates whether the current invocation is caused by an AJAX callback.</param>
		/// <returns>
		/// Returns an instance of MediaObjectWebEntity containing information for the specified media object,
		/// including its previous and next media object.
		/// </returns>
		internal static MediaObjectWebEntity GetMediaObjectHtml(IGalleryObject mediaObject, DisplayObjectType displayType, bool isCallBack)
		{
			// Get the information about the specified media object, its previous one, next one, and the next one in a slide show.
			Array browsers = Utils.GetBrowserIdsForCurrentRequest();

			if ((displayType == DisplayObjectType.Original) && (!Utils.IsUserAuthorized(SecurityActions.ViewOriginalImage, RoleController.GetGalleryServerRolesForUser(), mediaObject.Parent.Id, mediaObject.GalleryId, ((IAlbum)mediaObject.Parent).IsPrivate)))
			{
				displayType = DisplayObjectType.Optimized;
			}

			bool excludePrivateObjects = !Utils.IsAuthenticated;

			MediaObjectWebEntity mo = new MediaObjectWebEntity();

			#region Step 1: Process current media object

			if (mediaObject.Id > 0)
			{
				// This section is enclosed in the above if statement to force all declared variables within it to be local so they are
				// not accidentally re-used in steps 2 or 3. In reality, mediaObject.Id should ALWAYS be greater than 0.
				IDisplayObject displayObject = GalleryObjectController.GetDisplayObject(mediaObject, displayType);

				string htmlOutput = String.Empty;
				string scriptOutput = String.Empty;
				if (!String.IsNullOrEmpty(mediaObject.Original.ExternalHtmlSource))
				{
					IMediaObjectHtmlBuilder moBuilder = new MediaObjectHtmlBuilder(mediaObject.Original.ExternalHtmlSource, mediaObject.GalleryId);
					htmlOutput = moBuilder.GenerateHtml();
				}
				else if ((displayObject.Width > 0) && (displayObject.Height > 0))
				{
					IMediaObjectHtmlBuilder moBuilder = new MediaObjectHtmlBuilder(mediaObject.Id, mediaObject.Parent.Id, displayObject.MimeType, displayObject.FileNamePhysicalPath, displayObject.Width, displayObject.Height, mediaObject.Title, browsers, displayType, mediaObject.IsPrivate, mediaObject.GalleryId);
					htmlOutput = moBuilder.GenerateHtml();
					scriptOutput = moBuilder.GenerateScript();
				}

				bool isBeingProcessed = MediaConversionQueue.Instance.IsWaitingInQueueOrProcessing(mediaObject.Id);
				if (String.IsNullOrEmpty(htmlOutput))
				{
					// We'll get here when the user is trying to view a media object that cannot be displayed in the browser or the
					// config file does not have a definition for this MIME type. Default to a standard message noting that the user
					// can download the object via one of the toolbar commands.
					if (isBeingProcessed)
						htmlOutput = String.Format(CultureInfo.CurrentCulture, "<p class='gsp_item_process_msg'>{0}</p>", Resources.GalleryServerPro.UC_MediaObjectView_Media_Object_Being_Processed_Text);
					else
						htmlOutput = String.Format(CultureInfo.CurrentCulture, "<p class='gsp_msgfriendly'>{0}</p>", Resources.GalleryServerPro.UC_MediaObjectView_Browser_Cannot_Display_Media_Object_Text);
				}

				// Get the siblings of this media object and the index that specifies its position within its siblings.

				//TODO: This technique for identifying the index is very expensive when there are a lot of objects in the album.
				IGalleryObjectCollection siblings = ((IAlbum)mediaObject.Parent).GetChildGalleryObjects(GalleryObjectType.MediaObject, true, excludePrivateObjects);
				int mediaObjectIndex = siblings.IndexOf(mediaObject);

				if (mediaObjectIndex < 0)
				{
					// The media object we have is not the same one that is in it's album. This is probably a bug.
					Exception ex = new BusinessException(string.Format("Error: Can't find media object in album (index = {0}).", mediaObjectIndex));
					AppErrorController.LogError(ex);

					HelperFunctions.PurgeCache();

					siblings = ((IAlbum)mediaObject.Parent).GetChildGalleryObjects(GalleryObjectType.MediaObject, true, excludePrivateObjects);
					mediaObject = Factory.LoadMediaObjectInstance(mediaObject.Id, HelperFunctions.DetermineMediaObjectType(mediaObject.Original.FileName));

					mediaObjectIndex = siblings.IndexOf(mediaObject);
				}

				// Build up the entity object we'll be sending to the client.
				bool moIsExternalObject = (mediaObject is GalleryServerPro.Business.ExternalMediaObject);
				mo.Id = mediaObject.Id;
				mo.Index = mediaObjectIndex;
				mo.NumObjectsInAlbum = siblings.Count;
				mo.Title = mediaObject.Title;
				mo.PrevId = GetPreviousMediaObjectId(mediaObjectIndex, siblings);
				mo.NextId = GetNextMediaObjectId(mediaObjectIndex, siblings);
				mo.NextSSId = GetNextMediaObjectIdForSlideshow(mediaObjectIndex, siblings);
				mo.HtmlOutput = htmlOutput;
				mo.ScriptOutput = scriptOutput;
				mo.Width = displayObject.Width;
				mo.Height = displayObject.Height;
				mo.HiResAvailable = isBeingProcessed || (!String.IsNullOrEmpty(mediaObject.Optimized.FileName)) && (mediaObject.Original.FileName != mediaObject.Optimized.FileName);
				mo.IsDownloadable = !moIsExternalObject;
			}

			#endregion

			#region Step 2: Process previous media object

			if (mo.PrevId > 0)
			{
				IGalleryObject prevMO = Factory.LoadMediaObjectInstance(mo.PrevId);

				IDisplayObject displayObject = GalleryObjectController.GetDisplayObject(prevMO, displayType);

				string htmlOutput = String.Empty;
				string scriptOutput = String.Empty;
				if (!String.IsNullOrEmpty(prevMO.Original.ExternalHtmlSource))
				{
					IMediaObjectHtmlBuilder moBuilder = new MediaObjectHtmlBuilder(prevMO.Original.ExternalHtmlSource, prevMO.GalleryId);
					htmlOutput = moBuilder.GenerateHtml();
				}
				else if ((displayObject.Width > 0) && (displayObject.Height > 0))
				{
					IMediaObjectHtmlBuilder moBuilder = new MediaObjectHtmlBuilder(prevMO.Id, prevMO.Parent.Id, displayObject.MimeType, displayObject.FileNamePhysicalPath, displayObject.Width, displayObject.Height, prevMO.Title, browsers, displayType, prevMO.IsPrivate, prevMO.GalleryId);
					htmlOutput = moBuilder.GenerateHtml();
					scriptOutput = moBuilder.GenerateScript();
				}

				bool isBeingProcessed = MediaConversionQueue.Instance.IsWaitingInQueueOrProcessing(prevMO.Id);
				if (String.IsNullOrEmpty(htmlOutput))
				{
					// We'll get here when the user is trying to view a media object that cannot be displayed in the browser or the
					// config file does not have a definition for this MIME type. Default to a standard message noting that the user
					// can download the object via one of the toolbar commands.
					if (isBeingProcessed)
						htmlOutput = String.Format(CultureInfo.CurrentCulture, "<p class='gsp_item_process_msg'>{0}</p>", Resources.GalleryServerPro.UC_MediaObjectView_Media_Object_Being_Processed_Text);
					else
						htmlOutput = String.Format(CultureInfo.CurrentCulture, "<p class='gsp_msgfriendly'>{0}</p>", Resources.GalleryServerPro.UC_MediaObjectView_Browser_Cannot_Display_Media_Object_Text);
				}

				// Build up the entity object we'll be sending to the client.
				bool prevMoIsExternalObject = (prevMO is GalleryServerPro.Business.ExternalMediaObject);
				mo.PrevTitle = prevMO.Title;
				mo.PrevHtmlOutput = htmlOutput;
				mo.PrevScriptOutput = scriptOutput;
				mo.PrevWidth = displayObject.Width;
				mo.PrevHeight = displayObject.Height;
				mo.PrevHiResAvailable = isBeingProcessed || (!String.IsNullOrEmpty(prevMO.Optimized.FileName)) && (prevMO.Original.FileName != prevMO.Optimized.FileName);
				mo.PrevIsDownloadable = !prevMoIsExternalObject;
			}

			#endregion

			#region Step 3: Process next media object

			if (mo.NextId > 0)
			{
				IGalleryObject nextMO = Factory.LoadMediaObjectInstance(mo.NextId);

				IDisplayObject displayObject = GalleryObjectController.GetDisplayObject(nextMO, displayType);

				string htmlOutput = String.Empty;
				string scriptOutput = String.Empty;
				if (!String.IsNullOrEmpty(nextMO.Original.ExternalHtmlSource))
				{
					IMediaObjectHtmlBuilder moBuilder = new MediaObjectHtmlBuilder(nextMO.Original.ExternalHtmlSource, nextMO.GalleryId);
					htmlOutput = moBuilder.GenerateHtml();
				}
				else if ((displayObject.Width > 0) && (displayObject.Height > 0))
				{
					IMediaObjectHtmlBuilder moBuilder = new MediaObjectHtmlBuilder(nextMO.Id, nextMO.Parent.Id, displayObject.MimeType, displayObject.FileNamePhysicalPath, displayObject.Width, displayObject.Height, nextMO.Title, browsers, displayType, nextMO.IsPrivate, nextMO.GalleryId);
					htmlOutput = moBuilder.GenerateHtml();
					scriptOutput = moBuilder.GenerateScript();
				}

				bool isBeingProcessed = MediaConversionQueue.Instance.IsWaitingInQueueOrProcessing(nextMO.Id);
				if (String.IsNullOrEmpty(htmlOutput))
				{
					// We'll get here when the user is trying to view a media object that cannot be displayed in the browser or the
					// config file does not have a definition for this MIME type. Default to a standard message noting that the user
					// can download the object via one of the toolbar commands.
					if (isBeingProcessed)
						htmlOutput = String.Format(CultureInfo.CurrentCulture, "<p class='gsp_item_process_msg'>{0}</p>", Resources.GalleryServerPro.UC_MediaObjectView_Media_Object_Being_Processed_Text);
					else
						htmlOutput = String.Format(CultureInfo.CurrentCulture, "<p class='gsp_msgfriendly'>{0}</p>", Resources.GalleryServerPro.UC_MediaObjectView_Browser_Cannot_Display_Media_Object_Text);
				}

				// Build up the entity object we'll be sending to the client.
				bool nextMoIsExternalObject = (nextMO is GalleryServerPro.Business.ExternalMediaObject);
				mo.NextTitle = nextMO.Title;
				mo.NextHtmlOutput = htmlOutput;
				mo.NextScriptOutput = scriptOutput;
				mo.NextWidth = displayObject.Width;
				mo.NextHeight = displayObject.Height;
				mo.NextHiResAvailable = isBeingProcessed || (!String.IsNullOrEmpty(nextMO.Optimized.FileName)) && (nextMO.Original.FileName != nextMO.Optimized.FileName);
				mo.NextIsDownloadable = !nextMoIsExternalObject;
			}

			#endregion

			#region Step 4: Process next slide show media object

			if (mo.NextSSId > 0)
			{
				IGalleryObject nextSSMO = Factory.LoadMediaObjectInstance(mo.NextSSId);

				IDisplayObject displayObject = GalleryObjectController.GetDisplayObject(nextSSMO, displayType);

				string htmlOutput = String.Empty;
				string scriptOutput = String.Empty;
				string url = String.Empty;
				if (!String.IsNullOrEmpty(nextSSMO.Original.ExternalHtmlSource))
				{
					IMediaObjectHtmlBuilder moBuilder = new MediaObjectHtmlBuilder(nextSSMO.Original.ExternalHtmlSource, nextSSMO.GalleryId);
					htmlOutput = moBuilder.GenerateHtml();
				}
				else if ((displayObject.Width > 0) && (displayObject.Height > 0))
				{
					IMediaObjectHtmlBuilder moBuilder = new MediaObjectHtmlBuilder(nextSSMO.Id, nextSSMO.Parent.Id, displayObject.MimeType, displayObject.FileNamePhysicalPath, displayObject.Width, displayObject.Height, nextSSMO.Title, browsers, displayType, nextSSMO.IsPrivate, nextSSMO.GalleryId);
					htmlOutput = moBuilder.GenerateHtml();
					scriptOutput = moBuilder.GenerateScript();
					url = moBuilder.GenerateUrl();
				}

				bool isBeingProcessed = MediaConversionQueue.Instance.IsWaitingInQueueOrProcessing(nextSSMO.Id);
				if (String.IsNullOrEmpty(htmlOutput))
				{
					// We'll get here when the user is trying to view a media object that cannot be displayed in the browser or the
					// config file does not have a definition for this MIME type. Default to a standard message noting that the user
					// can download the object via one of the toolbar commands.
					if (isBeingProcessed)
						htmlOutput = String.Format(CultureInfo.CurrentCulture, "<p class='gsp_item_process_msg'>{0}</p>", Resources.GalleryServerPro.UC_MediaObjectView_Media_Object_Being_Processed_Text);
					else
						htmlOutput = String.Format(CultureInfo.CurrentCulture, "<p class='gsp_msgfriendly'>{0}</p>", Resources.GalleryServerPro.UC_MediaObjectView_Browser_Cannot_Display_Media_Object_Text);
				}

				// Get the siblings of this media object and the index that specifies its position within its siblings.

				//TODO: This technique for identifying the index is very expensive when there are a lot of objects in the album.
				IGalleryObjectCollection siblings = ((IAlbum)nextSSMO.Parent).GetChildGalleryObjects(GalleryObjectType.MediaObject, true, excludePrivateObjects);
				int mediaObjectIndex = siblings.IndexOf(nextSSMO);

				// Build up the entity object we'll be sending to the client.
				mo.NextSSIndex = mediaObjectIndex;
				mo.NextSSTitle = nextSSMO.Title;
				mo.NextSSUrl = url;
				mo.NextSSHtmlOutput = htmlOutput;
				mo.NextSSScriptOutput = scriptOutput;
				mo.NextSSWidth = displayObject.Width;
				mo.NextSSHeight = displayObject.Height;
				mo.NextSSHiResAvailable = isBeingProcessed || (!String.IsNullOrEmpty(nextSSMO.Optimized.FileName)) && (nextSSMO.Original.FileName != nextSSMO.Optimized.FileName);
				mo.NextSSIsDownloadable = true; // Slide show objects are always locally stored images and are therefore always downloadable
			}

			#endregion

			#region Step 5: Update Previous Uri variable

			if (HttpContext.Current.Session != null)
			{
				Uri backURL = Utils.PreviousUri;
				if (isCallBack && (backURL != null))
				{
					// We are in a callback. Even though the page hasn't changed, the user is probably viewing a different media object,
					// so update the moid query string parameter so that the referring url points to the current media object.
					backURL = UpdateUriQueryString(backURL, "moid", mediaObject.Id.ToString(CultureInfo.InvariantCulture));
				}
				else
				{
					backURL = Utils.GetCurrentPageUri();
				}

				Utils.PreviousUri = backURL;
			}

			#endregion

			return mo;
		}

		#endregion

		#region Private Static Methods

		private static int GetPreviousMediaObjectId(int mediaObjectIndex, IGalleryObjectCollection siblings)
		{
			int previousMediaObjectId = 0;
			if (mediaObjectIndex > 0)
			{
				previousMediaObjectId = siblings[mediaObjectIndex - 1].Id;
			}

			return previousMediaObjectId;
		}

		private static int GetNextMediaObjectId(int mediaObjectIndex, IGalleryObjectCollection siblings)
		{
			int nextMediaObjectId = 0;
			if (mediaObjectIndex < (siblings.Count - 1))
			{
				nextMediaObjectId = siblings[mediaObjectIndex + 1].Id;
			}

			return nextMediaObjectId;
		}

		private static int GetNextMediaObjectIdForSlideshow(int mediaObjectIndex, IGalleryObjectCollection siblings)
		{
			int nextMediaObjectId = 0;
			while (mediaObjectIndex < (siblings.Count - 1))
			{
				IGalleryObject nextMediaObject = siblings[mediaObjectIndex + 1];
				if (nextMediaObject is GalleryServerPro.Business.Image)
				{
					nextMediaObjectId = nextMediaObject.Id;
					break;
				}

				mediaObjectIndex += 1;
			}

			return nextMediaObjectId;
		}

		private static HtmlLink MakeStyleSheetControl(string href)
		{
			HtmlLink stylesheet = new HtmlLink();
			stylesheet.Href = href;
			stylesheet.Attributes.Add("rel", "stylesheet");
			stylesheet.Attributes.Add("type", "text/css");

			return stylesheet;
		}

		/// <summary>
		/// Updates the query string parameter in the <paramref name="uri"/> with the specified value. If the 
		/// <paramref name="queryStringName"/> is not present, it is added. The modified URI is returned. The <paramref name="uri"/>
		/// is not modified.
		/// </summary>
		/// <param name="uri">The URI that is to receive the updated or added query string <paramref name="queryStringName">name</paramref>
		/// and <paramref name="queryStringValue">value</paramref>. This object is not modified; rather, a new URI is created
		/// and returned.</param>
		/// <param name="queryStringName">Name of the query string to include in the URI.</param>
		/// <param name="queryStringValue">The query string value to include in the URI.</param>
		/// <returns>Returns the uri with the specified query string name and value updated or added.</returns>
		private static Uri UpdateUriQueryString(Uri uri, string queryStringName, string queryStringValue)
		{
			Uri updatedUri = null;
			string newQueryString = uri.Query;

			if (Utils.IsQueryStringParameterPresent(uri, queryStringName))
			{
				if (Utils.GetQueryStringParameterString(uri, queryStringName) != queryStringValue)
				{
					// The URI has the query string parm and it is different than the value. Update the URI.
					newQueryString = Utils.RemoveQueryStringParameter(newQueryString, queryStringName);
					newQueryString = Utils.AddQueryStringParameter(newQueryString, String.Format(CultureInfo.CurrentCulture, "{0}={1}", queryStringName, queryStringValue));

					UriBuilder uriBuilder = new UriBuilder(uri);
					uriBuilder.Query = newQueryString.TrimStart(new char[] { '?' });
					updatedUri = uriBuilder.Uri;
				}
				//else {} // Query string is present and already has the requested value. Do nothing.
			}
			else
			{
				// Query string parm not present. Add it.
				newQueryString = Utils.AddQueryStringParameter(newQueryString, String.Format(CultureInfo.CurrentCulture, "{0}={1}", queryStringName, queryStringValue));

				UriBuilder uriBuilder = new UriBuilder(uri);
				uriBuilder.Query = newQueryString.TrimStart(new char[] { '?' });
				updatedUri = uriBuilder.Uri;
			}
			return updatedUri ?? uri;
		}

		private List<ActionResult> GetUploadErrors(IEnumerable<ActionResult> uploadResults)
		{
			if (uploadResults == null)
				return null;

			return (from m in uploadResults where m.Status == ActionResultStatus.Error select m).ToList();
		}

		private static string ConvertListToHtmlBullets(IEnumerable<ActionResult> skippedFiles)
		{
			string html = "<ul>";
			foreach (ActionResult kvp in skippedFiles)
			{
				html += String.Format(CultureInfo.CurrentCulture, "<li>{0}: {1}</li>", kvp.Title, kvp.Message);
			}
			html += "</ul>";

			return html;
		}

		/// <summary>
		/// Verifies the media object exists and the user has permission to view it. If valid, the media object is assigned to the
		/// _mediaObject member variable and the function returns <c>true</c>; otherwise returns <c>false</c>.
		/// </summary>
		/// <param name="mediaObjectId">The media object ID to validate. Throws a <see cref="ArgumentOutOfRangeException"/>
		/// if the value is <see cref="Int32.MinValue"/>.</param>
		/// <param name="mediaObject">The media object.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="mediaObjectId"/> is <see cref="Int32.MinValue"/>.</exception>
		private static bool ValidateMediaObject(int mediaObjectId, out IGalleryObject mediaObject)
		{
			if (mediaObjectId == int.MinValue)
				throw new ArgumentOutOfRangeException("mediaObjectId", String.Format(CultureInfo.CurrentCulture, "A valid media object ID must be passed to this function. Instead, the value was {0}.", mediaObjectId));

			mediaObject = null;
			bool isValid = false;
			IGalleryObject tempMediaObject = null;
			try
			{
				tempMediaObject = Factory.LoadMediaObjectInstance(mediaObjectId);
			}
			catch (ArgumentException) { }
			catch (InvalidMediaObjectException) { }

			if (tempMediaObject != null)
			{
				// Perform a basic security check to make sure user can view media object. Another, more detailed security check is performed by child
				// user controls if necessary. (e.g. Perhaps the user is requesting the high-res version but he does not have the ViewOriginalImage 
				// permission. The view media object user control will verify this.)
				if (Utils.IsUserAuthorized(SecurityActions.ViewAlbumOrMediaObject | SecurityActions.ViewOriginalImage, RoleController.GetGalleryServerRolesForUser(), tempMediaObject.Parent.Id, tempMediaObject.GalleryId, tempMediaObject.IsPrivate, SecurityActionsOption.RequireOne))
				{
					// User is authorized. Assign to page-level variable.
					mediaObject = tempMediaObject;

					isValid = true;
				}
			}

			return isValid;
		}

		/// <summary>
		/// Check for existence of galleryserverpro.config and install.txt files. If present, redirect to upgrade or install
		/// wizard.
		/// </summary>
		private static void CheckForInstallOrUpgrade()
		{
			string appDataPath = System.IO.Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, GlobalConstants.AppDataDirectory);
			string upgradeFilePath = System.IO.Path.Combine(appDataPath, GlobalConstants.UpgradeTriggerFileName);
			string installFilePath = System.IO.Path.Combine(appDataPath, GlobalConstants.InstallTriggerFileName);

			if (System.IO.File.Exists(upgradeFilePath))
			{
				try
				{
					Utils.Redirect(PageId.upgrade);
				}
				catch (System.Threading.ThreadAbortException) {}
			}

			if (System.IO.File.Exists(installFilePath))
			{
				try
				{
					Utils.Redirect(PageId.install);
				}
				catch (System.Threading.ThreadAbortException) {}
			}
		}

		#endregion

		#region Private Methods

		private void InitializePage()
		{
			InitializeGallerySettings();

			lock (_lockObject)
			{
				if (AppSetting.Instance.SampleObjectsNeeded)
				{
					GalleryController.CreateSampleObjects(GalleryId);

					AppSetting.Instance.SampleObjectsNeeded = false;
				}
			}

			// Redirect to the logon page if the user has to log in. (Note that the InitializeGallerySettings() function
			// may also check RequiresLogin() and do a redirect when a GallerySecurityException is thrown.)
			if (RequiresLogin())
			{
				Utils.Redirect(PageId.login, true, "ReturnUrl={0}", Utils.UrlEncode(Utils.GetCurrentPageUrl(true)));
			}

			// Add a ScriptManager if the page doesn't already have one.
			ScriptManager sm = ScriptManager.GetCurrent(this.Page);
			if (sm == null)
			{
				sm = new ScriptManager();
				sm.EnableScriptGlobalization = true;
				this.Controls.AddAt(0, sm);
			}

			this.StoreCurrentPageUri();

			if (Utils.IsAuthenticated && GallerySettings.EnableUserAlbum)
			{
				UserController.ValidateUserAlbum(Utils.UserName, GalleryId);
			}

			if (IsPostBack)
			{
				// Postback: Since the user may have been navigating several media objects in this album through AJAX calls, we need to check
				// a hidden field to discover the current media object. Assign this object's ID to our base user control. The base 
				// control is smart enough to retrieve the new media object if it is different than what was previously set.
				object formFieldMoid = Request.Form["moid"];
				int moid;
				if ((formFieldMoid != null) && (Int32.TryParse(formFieldMoid.ToString(), out moid)))
				{
					this.SetMediaObjectId(moid);
				}
			}

			if (!IsPostBack)
			{
				//JQueryRequired = ((PageId == PageId.mediaobject) && (GallerySettings.MediaObjectTransitionType == MediaObjectTransitionType.Fade));

				RegisterHiddenFields();
			}

			sm.Services.Add(new ServiceReference(Utils.GetUrl("/services/Gallery.asmx")));

			// Add user controls to the page, such as the header and album breadcrumb menu.
			this.AddUserControls();

			RunAutoSynchIfNeeded();
		}

		/// <summary>
		/// Assign reference to gallery settings for the current gallery. If the user does not have permission to the requested
		/// album or media object, the user is automatically redirected as needed (e.g. the login page or the highest level album
		/// the user has permission to view). One exception to this is if a particular album is assigned to the control and the
		/// user does not have permission to view it, an empty album is used and a relevant message is assigned to the 
		/// <see cref="Message" /> property.
		/// </summary>
		private void InitializeGallerySettings()
		{
			try
			{
				LoadGallerySettings();
			}
			catch (InvalidMediaObjectException) { }
			catch (InvalidAlbumException ex)
			{
				CheckForInvalidAlbumIdInGalleryControlSetting(ex.AlbumId);

				Utils.Redirect(Utils.AddQueryStringParameter(Utils.GetCurrentPageUrl(), "msg=" + (int)Message.AlbumDoesNotExist));
			}
			catch (GallerySecurityException)
			{
				// Redirect to the logon page if the user has to log in.
				if (RequiresLogin())
				{
					Utils.Redirect(PageId.login, true, "ReturnUrl={0}", Utils.UrlEncode(Utils.GetCurrentPageUrl(true)));
				}
				else
				{
					if (this.GalleryControl.AlbumId > int.MinValue)
					{
						// User does not have access to the album specified as the default gallery object.
						this._album = CreateEmptyAlbum(AlbumController.LoadAlbumInstance(this.GalleryControl.AlbumId, false).GalleryId);

						this.Message = Message.AlbumNotAuthorizedForUser;
					}
					else
					{
						Utils.Redirect(PageId.album);
					}
				}
			}
		}

		/// <summary>
		/// Assign reference to gallery settings for the current gallery.
		/// </summary>
		/// <exception cref="InvalidAlbumException">Thrown when an album is requested but does not exist.</exception>
		/// <exception cref="GallerySecurityException">Thrown when the user is requesting an album or media object they don't have 
		/// permission to view.</exception>
		/// <remarks>This must be called from <see cref="GalleryPage_Init" />! It can't go in the <see cref="GalleryPage" /> constructor 
		/// because that is too early to access the GalleryId property, and it can't go in the GallerySettings property getter because 
		/// that is too late if a gallery has to be dynamically created.)</remarks>
		private void LoadGallerySettings()
		{
			try
			{
				this._gallerySetting = Factory.LoadGallerySetting(GalleryId);
			}
			catch (GallerySecurityException)
			{
				// The user is requesting an album or media object they don't have permission to view. Manually load the gallery settings
				// from the query string parameter and assign the gallery ID property so that they are available in the RequiresLogin() 
				// function later in GalleryPage_Init(). That code will take care of redirecting the user to the login page.
				int albumId = Utils.GetQueryStringParameterInt32("aid");
				int mediaObjectId = Utils.GetQueryStringParameterInt32("moid");

				if ((albumId == int.MinValue))
				{
					albumId = this.GalleryControl.AlbumId;
				}

				if (mediaObjectId == int.MinValue)
				{
					mediaObjectId = this.GalleryControl.MediaObjectId;
				}

				if (albumId > int.MinValue)
				{
					try
					{
						_galleryId = AlbumController.LoadAlbumInstance(albumId, false).GalleryId;
						this._gallerySetting = Factory.LoadGallerySetting(_galleryId);
					}
					catch (InvalidAlbumException) { }
				}
				else if (mediaObjectId > int.MinValue)
				{
					try
					{
						_galleryId = Factory.LoadMediaObjectInstance(mediaObjectId).Parent.GalleryId;
						this._gallerySetting = Factory.LoadGallerySetting(_galleryId);
					}
					catch (InvalidMediaObjectException) { }
					catch (InvalidAlbumException) { }
				}

				throw; // Re-throw GallerySecurityException
			}
		}

		/// <summary>
		/// Determines whether the current user must be logged in to access the requested page.
		/// </summary>
		/// <returns>Returns <c>true</c> if the user must be logged in to access the requested page; otherwise
		/// returns <c>false</c>.</returns>
		private bool RequiresLogin()
		{
			if ((this.PageId == PageId.login) || (this.PageId == PageId.createaccount) || (this.PageId == PageId.recoverpassword))
				return false; // The login, create account, & recover password pages never require one to be logged in

			if (!this.IsAnonymousUser)
				return false; // Already logged in

			if (!AllowAnonymousBrowsing)
				return true; // Not logged in, anonymous browsing disabled

			// Some pages allow anonymous browsing. If it is one of those, return false; otherwise return true;
			switch (this.PageId)
			{
				//case PageId.createaccount:
				//case PageId.login:
				//case PageId.recoverpassword: // These 3 are redundent because we already handle them above
				case PageId.album:
				case PageId.albumtreeview:
				case PageId.mediaobject:
				case PageId.search:
				case PageId.task_downloadobjects:
					return false;
				default:
					return true;
			}
		}

		private void AddAlbumTreeView()
		{
			if (ShouldPageHaveTreeView())
			{
				Controls.albumtreeview albumTreeView = (Controls.albumtreeview)LoadControl(Utils.GetUrl("/controls/albumtreeview.ascx"));

				this.Controls.AddAt(0, albumTreeView);

				albumTreeView.RequiredSecurityPermissions = Business.SecurityActions.ViewAlbumOrMediaObject;
				albumTreeView.TreeView.CssClass = "tv1TreeView";
				albumTreeView.ShowCheckbox = false;
				albumTreeView.NavigateUrl = Utils.GetCurrentPageUrl();

				int albumId = GetAlbumId();
				if (albumId > int.MinValue)
				{
					albumTreeView.BindTreeView(GetAlbum());
				}
				else
				{
					albumTreeView.BindTreeView();
				}

				if (albumTreeView.TreeView.Nodes.Count > 0)
				{
					AlbumTreeViewIsVisible = true;
				}
			}
		}

		private bool ShouldPageHaveTreeView()
		{
			// The only pages that should display an album treeview are the album, media object, and search pages.
			switch (PageId)
			{
				case PageId.album:
				case PageId.search:
					return ShowAlbumTreeViewForAlbum;

				case PageId.mediaobject:
					return ShowAlbumTreeViewForMediaObject;

				default:
					return false;
			}
		}

		private void AddAlbumMenu()
		{
			Controls.albummenu albumMenu = (Controls.albummenu)LoadControl(Utils.GetUrl("/controls/albummenu.ascx"));
			this._albumMenu = albumMenu;
			this.Controls.AddAt(0, albumMenu);
		}

		private void AddGalleryHeader()
		{
			Controls.galleryheader header = (Controls.galleryheader)LoadControl(Utils.GetUrl("/controls/galleryheader.ascx"));
			this._galleryHeader = header;
			this.Controls.AddAt(0, header);
		}

		/// <summary>
		/// Stores the URI of the current album or media object page so that we can return to it later, if desired. This
		/// method store the current URI only for fresh page loads (no postbacks or callbacks) and when the current page
		/// is displaying an album view or media object. Other pages, such as task or admin pages, 
		/// are not stored since we do not want to return to them. This method assigns the current URI to the 
		/// <see cref="PreviousUri"/> property. After assigning this property, one can use 
		/// <see cref="RedirectToPreviousPage()"/> to navigate to the page. If session state is disabled, this method does nothing.
		/// </summary>		
		private void StoreCurrentPageUri()
		{
			if (IsNewPageLoad)
			{
				if ((this.PageId == PageId.album) || (this.PageId == PageId.mediaobject))
					this.PreviousUri = Utils.GetCurrentPageUri();
			}
		}

		/// <summary>
		/// Set the public properties on this class related to user permissions. This method is called as needed from
		/// within the property getters.
		/// </summary>
		private void EvaluateUserPermissions()
		{
			bool isPhysicalAlbum = !this.GetAlbum().IsVirtualAlbum;

			// We need include isPhysicalAlbum in the expressions below because the IsUserAuthorized function uses
			// the album ID of the current album to evaluate the user's ability to perform the action. In the case
			// of a virtual album, the album ID is int.MinValue and the method is therefore not able to evaluate the permission.

			this._userCanCreateAlbum = isPhysicalAlbum && this.IsUserAuthorized(SecurityActions.AddChildAlbum);
			this._userCanEditAlbum = isPhysicalAlbum && this.IsUserAuthorized(SecurityActions.EditAlbum);
			this._userCanAddMediaObject = isPhysicalAlbum && this.IsUserAuthorized(SecurityActions.AddMediaObject);
			this._userCanEditMediaObject = isPhysicalAlbum && this.IsUserAuthorized(SecurityActions.EditMediaObject);
			this._userCanAddAdministerSite = isPhysicalAlbum && this.IsUserAuthorized(SecurityActions.AdministerSite);
			this._userCanAdministerGallery = isPhysicalAlbum && this.IsUserAuthorized(SecurityActions.AdministerGallery);
			this._userCanDeleteCurrentAlbum = isPhysicalAlbum && this.IsUserAuthorized(SecurityActions.DeleteAlbum);
			this._userCanDeleteChildAlbum = isPhysicalAlbum && this.IsUserAuthorized(SecurityActions.DeleteChildAlbum);
			this._userCanDeleteMediaObject = isPhysicalAlbum && this.IsUserAuthorized(SecurityActions.DeleteMediaObject);
			this._userCanSynchronize = isPhysicalAlbum && this.IsUserAuthorized(SecurityActions.Synchronize);
			this._userCanViewHiResImage = isPhysicalAlbum && this.IsUserAuthorized(SecurityActions.ViewOriginalImage);

			IGallery gallery = Factory.LoadGallery(GalleryId);

			foreach (IGalleryServerRole role in GetGalleryServerRolesForUser())
			{
				if (role.Galleries.Contains(gallery))
				{
					if (role.AllowAdministerSite)
					{
						this._userCanAddMediaObjectToAtLeastOneAlbum = true;
						this._userCanAddAlbumToAtLeastOneAlbum = true;
						break;
					}

					if (role.AllowAddMediaObject)
						this._userCanAddMediaObjectToAtLeastOneAlbum = true;

					if (role.AllowAddChildAlbum)
						this._userCanAddAlbumToAtLeastOneAlbum = true;
				}
			}

			// If UserCanAddAlbumToAtLeastOneAlbum or UserCanAddMediaObjectToAtLeastOneAlbum havn't been set to
			// true by now, then user does not have permission to do those things. Set to false.
			if (!this._userCanAddAlbumToAtLeastOneAlbum.HasValue)
			{
				this._userCanAddAlbumToAtLeastOneAlbum = false;
			}

			if (!this._userCanAddMediaObjectToAtLeastOneAlbum.HasValue)
			{
				this._userCanAddMediaObjectToAtLeastOneAlbum = false;
			}
		}

		private static string GetAlbumThumbnailUrl(IGalleryObject galleryObject)
		{
			// Get a reference to the path to the thumbnail. If the user is anonymous and the thumbnail is from a private
			// media object or album, then specify int.MinValue for the media object ID. This will be interpreted
			// by the image handler to generate a default, empty thumbnail image.
			int mediaObjectId = galleryObject.Thumbnail.MediaObjectId;

			if (mediaObjectId == 0)
			{
				mediaObjectId = int.MinValue;
			}

			if (!Utils.IsAuthenticated && (galleryObject.Thumbnail.MediaObjectId > 0))
			{
				try
				{
					IGalleryObject mediaObject = Factory.LoadMediaObjectInstance(galleryObject.Thumbnail.MediaObjectId);
					if (mediaObject.Parent.IsPrivate || mediaObject.IsPrivate)
					{
						mediaObjectId = int.MinValue;
					}
				}
				catch (InvalidMediaObjectException)
				{
					// We'll get here if the ID for the thumbnail doesn't represent an existing media object.
					mediaObjectId = int.MinValue;
				}
			}

			return MediaObjectHtmlBuilder.GenerateUrl(galleryObject.GalleryId, mediaObjectId, DisplayObjectType.Thumbnail);
		}

		private void RegisterHiddenFields()
		{
			if (GetMediaObjectId() > int.MinValue)
				ScriptManager.RegisterHiddenField(this, "moid", GetMediaObjectId().ToString(CultureInfo.InvariantCulture));

			if (GetAlbumId() > int.MinValue)
				ScriptManager.RegisterHiddenField(this, "aid", GetAlbumId().ToString(CultureInfo.InvariantCulture));
		}

		private void AddUserControls()
		{
			// If any inheritors subscribed to the event, fire it.
			if (BeforeHeaderControlsAdded != null)
			{
				BeforeHeaderControlsAdded(this, new EventArgs());
			}

			AddAlbumTreeView();

			if (PageId != PageId.login)
			{
				if (UserCanAdministerSite || UserCanAdministerGallery || this.ShowActionMenu || this.ShowAlbumBreadCrumb)
				{
					this.AddAlbumMenu();
				}
			}

			if (this.ShowHeader)
			{
				this.AddGalleryHeader();
			}
		}

		/// <summary>
		/// Write out the Gallery Server Pro logo to the <paramref name="writer"/>.
		/// </summary>
		/// <param name="writer">The writer.</param>
		private void AddGspLogo(HtmlTextWriter writer)
		{
			if (this.GalleryControl.ViewMode == ViewMode.TreeView)
			{
				return;
			}

			// This function writes out HTML like this:
			// <div class="gsp_addtopmargin5 gsp_footer">
			//  <a href="http://www.galleryserverpro.com" title="Powered by Gallery Server Pro v2.1.3222">
			//   <img src="/images/gsp_ftr_logo_170x46.png" alt="Powered by Gallery Server Pro v2.1.3222" style="width:170px;height:46px;" />
			//  </a>
			// </div>
			string tooltip = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.Footer_Logo_Tooltip, Utils.GetGalleryServerVersion());
			//string url = Page.ClientScript.GetWebResourceUrl(typeof(footer), "GalleryServerPro.Web.gs.images.gsp_ftr_logo_170x46.png");

			// Create <div> tag that wraps the <a> and <img> tags.<div id="gs_footer">
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "gsp_addtopmargin5 gsp_footer");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);

			// Create <a> tag that wraps <img> tag.
			writer.AddAttribute(HtmlTextWriterAttribute.Title, tooltip);
			writer.AddAttribute(HtmlTextWriterAttribute.Href, "http://www.galleryserverpro.com");
			writer.RenderBeginTag(HtmlTextWriterTag.A);

			// Create <img> tag.
			writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "170px");
			writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "46px");
			writer.AddStyleAttribute(HtmlTextWriterStyle.VerticalAlign, "middle");
			writer.AddAttribute(HtmlTextWriterAttribute.Src, Page.ClientScript.GetWebResourceUrl(this.GetType().BaseType, "GalleryServerPro.Web.gs.images.gsp_ftr_logo_170x46.png"));
			writer.AddAttribute(HtmlTextWriterAttribute.Alt, tooltip);
			writer.RenderBeginTag(HtmlTextWriterTag.Img);
			writer.RenderEndTag();

			// Close out the <a> tag.
			writer.RenderEndTag();

			// Close out the <div> tag.
			writer.RenderEndTag();
		}

		/// <summary>
		/// Set up the items within the head portion of the HTML page. Specifically, assign the title tag and add links to the 
		/// CSS and jQuery files.
		/// </summary>
		/// <param name="head">The head portion of the current HTML page.</param>
		private void SetupHeadControl(HtmlHead head)
		{
			if (String.IsNullOrEmpty(head.Title))
				head.Title = PageTitle;

			// Add CSS links to the header, but only if they haven't already been added by another Gallery control on the page.
			object cssFilesAddedObject = HttpContext.Current.Items["GSP_CssFilesAdded"];
			bool cssFilesAdded = false;
			bool foundCssFilesAddedVar = ((cssFilesAddedObject != null) && Boolean.TryParse(cssFilesAddedObject.ToString(), out cssFilesAdded));
			if (!foundCssFilesAddedVar || (!cssFilesAdded))
			{
				foreach (string cssPath in GetCssPaths())
				{
					head.Controls.Add(MakeStyleSheetControl(cssPath));
				}

				HttpContext.Current.Items["GSP_CssFilesAdded"] = bool.TrueString;
			}
		}

		private void AddjQuery()
		{
			ScriptManager sm = ScriptManager.GetCurrent(this.Page);
			if (sm == null)
			{
				throw new WebException("Gallery Server Pro requires a ScriptManager on the page.");
			}

			// Add jQuery reference.
			IAppSetting appSetting = AppSetting.Instance;
			if (!String.IsNullOrEmpty(appSetting.JQueryScriptPath))
			{
				if (Utils.IsAbsoluteUrl(appSetting.JQueryScriptPath))
				{
					sm.Scripts.Add(new ScriptReference(appSetting.JQueryScriptPath));
				}
				else
				{
					sm.Scripts.Add(new ScriptReference(this.Page.ResolveUrl(appSetting.JQueryScriptPath)));
				}
			}

			// Add jQuery UI reference.
			if (JQueryUiRequired && !String.IsNullOrEmpty(appSetting.JQueryUiScriptPath))
			{
				if (Utils.IsAbsoluteUrl(appSetting.JQueryUiScriptPath))
				{
					sm.Scripts.Add(new ScriptReference(appSetting.JQueryUiScriptPath));
				}
				else
				{
					sm.Scripts.Add(new ScriptReference(this.Page.ResolveUrl(appSetting.JQueryUiScriptPath)));
				}
			}
		}

		/// <summary>
		/// Gets the paths, relative to the web site root, of the CSS files needed by GSP. Example: "/dev/gsweb/gs/styles/gallery.css"
		/// </summary>
		/// <returns>Returns an array of strings containing the CSS paths.</returns>
		private static IEnumerable<string> GetCssPaths()
		{
			return new string[] { Utils.GetUrl("/styles/gallery.css"), Utils.GetUrl("/styles/ca_styles.css") };
		}

		/// <summary>
		/// If auto-sync is enabled and another synchronization is needed, start a synchronization of the root album in this gallery
		/// on a new thread.
		/// </summary>
		private void RunAutoSynchIfNeeded()
		{
			if (NeedToRunAutoSync())
			{
				// Start sync on new thread
				SynchronizeSettingsEntity syncSettings = new SynchronizeSettingsEntity();
				syncSettings.SyncInitiator = SyncInitiator.AutoSync;
				syncSettings.AlbumToSynchronize = Factory.LoadRootAlbumInstance(GalleryId, false);
				syncSettings.IsRecursive = true;
				syncSettings.RebuildThumbnails = false;
				syncSettings.RebuildOptimized = false;
				syncSettings.RegenerateMetadata = false;

				System.Threading.Thread notifyAdminThread = new System.Threading.Thread(GalleryController.Synchronize);
				notifyAdminThread.Start(syncSettings);
			}
		}

		/// <summary>
		/// Gets a value indicating whether an auto-sync must be performed. It is needed when auto-sync is enabled and the specified
		/// interval has passed since the last sync.
		/// </summary>
		/// <returns><c>true</c> if a sync must be run; otherwise <c>false</c>.</returns>
		private bool NeedToRunAutoSync()
		{
			IGallerySettings gallerySettings = Factory.LoadGallerySetting(GalleryId);

			if (gallerySettings.EnableAutoSync)
			{
				// Auto sync is enabled.
				double numMinutesSinceLastSync = DateTime.Now.Subtract(gallerySettings.LastAutoSync).TotalMinutes;

				if (numMinutesSinceLastSync > gallerySettings.AutoSyncIntervalMinutes)
				{
					// It is time to do another sync.
					ISynchronizationStatus synchStatus = SynchronizationStatus.GetInstance(GalleryId);

					if ((synchStatus.Status != SynchronizationState.SynchronizingFiles) && (synchStatus.Status != SynchronizationState.PersistingToDataStore))
					{
						// No other sync is in progress - we need to do one!
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Evaluate the query string and properties of the Gallery control to discover which, if any, media object to display.
		/// Returns <see cref="Int32.MinValue" /> if no ID is discovered. This function does not evaluate the ID to see if it is
		/// valid or whether the current user has permission to view it.
		/// </summary>
		/// <returns>Returns the ID for the media object to display, or <see cref="Int32.MinValue" /> if no ID is discovered.</returns>
		private int ParseMediaObjectId()
		{
			// Determine the ID for the media object to display, if any. Follow these rules:
			// 1. If an album has been requested and no media object specified, return Int32.MinValue.
			// 2. If AllowUrlOverride=true and a media object ID has been specified in the query string, use that.
			// 3. If AllowUrlOverride=true and an album ID has been specified in the query string, get one of it's media objects.
			// 4. If a media object ID has been specified on Gallery.MediaObjectId, use that.
			// 5. If ViewMode is Single or SingleRandom and an album ID has been specified on Gallery.AlbumId, get one of it's media objects.
			// 6. If ViewMode is Single or SingleRandom, get one of the media objects in the root album.
			// 7. If none of the above, return Int32.MinValue.

			int aidGc = this.GalleryControl.AlbumId;
			int moidGc = this.GalleryControl.MediaObjectId;
			int moidQs = Utils.GetQueryStringParameterInt32("moid");
			int aidQs = Utils.GetQueryStringParameterInt32("aid");
			bool isAlbumView = (this.GalleryControl.ViewMode == ViewMode.Multiple);
			bool allowUrlOverride = this.GalleryControl.AllowUrlOverride;

			if (isAlbumView && ((aidQs > int.MinValue) || (aidGc > int.MinValue)) && (moidQs == int.MinValue) && (moidGc == int.MinValue))
				return int.MinValue; // Matched rule 1

			if (allowUrlOverride)
			{
				if (moidQs > int.MinValue)
					return moidQs; // Matched rule 2

				if (aidQs > int.MinValue)
					return GetMediaObjectIdInAlbum(aidQs); // Matched rule 3
			}

			if (moidGc > int.MinValue)
				return moidGc; // Matched rule 4

			if (!isAlbumView && (aidGc > int.MinValue))
				return GetMediaObjectIdInAlbum(aidGc); // Matched rule 5

			if (!isAlbumView)
				return GetMediaObjectInRootAlbum(); // Matched rule 6

			return int.MinValue; // Matched rule 7
		}

		/// <summary>
		/// Get the ID for one of the media objects in the root album of the current gallery. The ID selected depends on the
		/// <see cref="Gallery.ViewMode" /> and whether <see cref="AutoPlaySlideShow" /> has been enabled. Returns <see cref="Int32.MinValue" />
		/// if the album does not contain a suitable media object.
		/// </summary>
		/// <returns>Returns the ID for one of the media objects in the root album, or <see cref="Int32.MinValue" /> if no suitable ID is found.</returns>
		private int GetMediaObjectInRootAlbum()
		{
			if (this.GalleryControl.GalleryId > int.MinValue)
			{
				return GetMediaObjectIdInAlbum(Factory.LoadRootAlbumInstance(this.GalleryControl.GalleryId).Id);
			}

			// No gallery ID has been assigned, so just use the first one we find. I am not sure this code will ever be hit, since it is possible
			// the gallery ID will always be assigned by this point.
			IGalleryCollection galleries = Factory.LoadGalleries();
			if (galleries.Count > 0)
			{
				return GetMediaObjectIdInAlbum(Factory.LoadRootAlbumInstance(galleries[0].GalleryId).Id);
			}

			return int.MinValue;
		}

		/// <summary>
		/// Get the ID for one of the media objects in the specified <paramref name="albumId" />. The ID selected depends on the
		/// <see cref="Gallery.ViewMode" /> and whether <see cref="AutoPlaySlideShow" /> has been enabled. Returns <see cref="Int32.MinValue" />
		/// if the album does not contain a suitable media object.
		/// </summary>
		/// <param name="albumId">The album ID.</param>
		/// <returns>Returns the ID for one of the media objects in the album, or <see cref="Int32.MinValue" /> if no suitable ID is found.</returns>
		private int GetMediaObjectIdInAlbum(int albumId)
		{
			int moid = int.MinValue;

			if (this.GalleryControl.ViewMode == ViewMode.Single)
			{
				// Choose the first media object in the album, unless <see cref="AutoPlaySlideShow" /> is enabled, in which case we want
				// to choose the first *image* in the album.
				IAlbum album = null;
				IGalleryObjectCollection galleryObjects = null;

				try
				{
					album = AlbumController.LoadAlbumInstance(albumId, true);
				}
				catch (InvalidAlbumException) { }

				if (album != null)
				{
					if (this.AutoPlaySlideShow)
					{
						galleryObjects = album.GetChildGalleryObjects(GalleryObjectType.Image, true); // Get all images in album
					}
					else
					{
						galleryObjects = album.GetChildGalleryObjects(GalleryObjectType.MediaObject, true); // Get all media objects in album
					}
				}

				if ((galleryObjects != null) && (galleryObjects.Count > 0))
				{
					moid = galleryObjects[0].Id;
				}
			}
			else if (this.GalleryControl.ViewMode == ViewMode.SingleRandom)
			{
				//TODO: Implement ViewMode.SingleRandom functionality
				throw new NotImplementedException("The functionality to support ViewMode.SingleRandom has not been implemented.");
			}

			return moid;
		}

		/// <summary>
		/// Gets the highest-level album the current user can view. Guaranteed to not return null. If a user does not have permission to 
		/// view any objects, this function returns a virtual album with no objects and automatically assigns the <see cref="Message" /> 
		/// property to Message.NoAuthorizedAlbumForUser, which will cause a message to be displayed to the user.
		/// </summary>
		/// <returns>Returns an IAlbum representing the highest-level album the current user can view.</returns>
		private IAlbum GetHighestAlbumUserCanView()
		{
			// Load the top level album for which the current user has permission to view.
			IAlbum tempAlbum = Factory.LoadRootAlbum(GalleryId, SecurityActions.ViewAlbumOrMediaObject | SecurityActions.ViewOriginalImage, RoleController.GetGalleryServerRolesForUser(), Utils.IsAuthenticated);

			if ((tempAlbum.IsVirtualAlbum) && (tempAlbum.GetChildGalleryObjects().Count == 0))
			{
				this.Message = Message.NoAuthorizedAlbumForUser;
			}

			return tempAlbum;
		}

		/// <summary>
		/// Gets the album ID corresponding to the current album, or <see cref="Int32.MinValue" /> if no valid album is available. The value 
		/// is determined in the following sequence: (1) If no media object is available, then look for the "aid" query string parameter. 
		/// (2) If not there, or if <see cref="Gallery.AllowUrlOverride" /> has been set to <c>false</c>, look for an album ID on the 
		/// containing <see cref="Gallery" /> control. This function does NOT perform any validation that the album exists and the current 
		/// user has permission to view it.
		/// </summary>
		/// <returns>Returns the album ID corresponding to the current album, or <see cref="Int32.MinValue" /> if no valid album is available.</returns>
		private int ParseAlbumId()
		{
			int aid;
			object viewstateAid = ViewState["aid"];

			if ((viewstateAid == null) || (!Int32.TryParse(ViewState["aid"].ToString(), out aid)))
			{
				// Not in viewstate. See if it is on the "aid" query string.
				if ((this.GalleryControl.AllowUrlOverride) && (Utils.GetQueryStringParameterInt32("aid") > int.MinValue))
				{
					aid = Utils.GetQueryStringParameterInt32("aid");
				}
				else
				{
					// Use the album ID property on this user control. May return int.MinValue.
					aid = this.GalleryControl.AlbumId;
				}

				ViewState["aid"] = aid;
			}

			return aid;
		}

		/// <summary>
		/// Verifies the album exists and the user has permission to view it Throws a <see cref="InvalidAlbumException" /> when an 
		/// album associated with the <paramref name="albumId" /> does not exist. Throws a <see cref="GallerySecurityException" /> 
		/// when the user requests an album he or she does not have permission to view. An instance of the album is assigned to the 
		/// album output parameter, and is guaranteed to not be null.
		/// </summary>
		/// <param name="albumId">The album ID to validate. Throws a <see cref="ArgumentOutOfRangeException"/>
		/// if the value is <see cref="Int32.MinValue"/>.</param>
		/// <param name="album">The album associated with the ID = <paramref name="albumId" />.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="albumId"/> is <see cref="Int32.MinValue"/>.</exception>
		/// <exception cref="InvalidAlbumException">Thrown when an album associated with the <paramref name="albumId" /> does not exist.</exception>
		/// <exception cref="GallerySecurityException">Thrown when the user is requesting an album they don't have permission to view.</exception>
		private void ValidateAlbum(int albumId, out IAlbum album)
		{
			if (albumId == int.MinValue)
				throw new ArgumentOutOfRangeException("albumId", String.Format(CultureInfo.CurrentCulture, "A valid album ID must be passed to this function. Instead, the value was {0}.", albumId));

			album = null;
			IAlbum tempAlbum = null;

			// TEST 1: If the current media object's album matches the ID we are validating, get a reference to that album.
			IGalleryObject mediaObject = GetMediaObject();
			if (mediaObject != null)
			{
				if (mediaObject.Parent.Id != albumId)
					throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, "The requested media object (ID={0}) does not exist in the requested album (ID={1}).", mediaObject.Id, albumId));

				// Instead of loading it from disk, just grab the reference to the media object's parent.
				tempAlbum = (IAlbum)mediaObject.Parent;
			}
			else
			{
				// No media object is part of this HTTP request, so load it from disk.
				tempAlbum = AlbumController.LoadAlbumInstance(albumId, false);
			}

			// TEST 2: Does user have permission to view it?
			if (tempAlbum != null)
			{
				if (Utils.IsUserAuthorized(SecurityActions.ViewAlbumOrMediaObject, RoleController.GetGalleryServerRolesForUser(), tempAlbum.Id, tempAlbum.GalleryId, tempAlbum.IsPrivate))
				{
					// User is authorized. Assign to output parameter.
					album = tempAlbum;
				}
				else
				{
					throw new GallerySecurityException(); // User does not have permission to view the album.
				}
			}
		}

		private static IAlbum CreateEmptyAlbum(int galleryId)
		{
			IAlbum album = null;
			try
			{
				album = Factory.CreateEmptyAlbumInstance(galleryId);
				album.IsVirtualAlbum = true;
				album.Title = Resources.GalleryServerPro.Site_Virtual_Album_Title;
			}
			catch
			{
				if (album != null)
					album.Dispose();

				throw;
			}

			return album;
		}

		/// <summary>
		/// Check the albumId to see if it matches the <see cref="IGalleryControlSettings.AlbumId" /> value for the 
		/// <see cref="Gallery.GalleryControlSettings" /> property on the <see cref="Gallery" /> user control. If it does, that means the setting
		/// contains an ID for an album that no longer exists. Delete the setting.
		/// </summary>
		/// <param name="albumId">The album ID.</param>
		private void CheckForInvalidAlbumIdInGalleryControlSetting(int albumId)
		{
			if (this.GalleryControl.GalleryControlSettings.AlbumId == albumId)
			{
				IGalleryControlSettings galleryControlSettings = Factory.LoadGalleryControlSetting(this.GalleryControl.ControlId, true);
				galleryControlSettings.AlbumId = null;
				galleryControlSettings.Save();
			}
		}

		private void SetMediaObjectId(int mediaObjectId)
		{
			this._mediaObjectId = mediaObjectId;
			this._mediaObject = null;
			this._album = null;
			this._galleryId = int.MinValue;
		}

		#endregion
	}
}
