using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using PagerPosition = GalleryServerPro.Business.PagerPosition;

namespace GalleryServerPro.Web.Controls
{
	/// <summary>
	/// A user control that provides a thumbnail view of gallery objects.
	/// </summary>
	public partial class thumbnailview : GalleryUserControl
	{
		#region Private Fields

		private int? _pageSize;
		private bool? _pagingEnabled;
		private IGalleryObjectCollection _galleryObjectsDataSource;

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets a value indicating whether paging is enabled and the current number of objects is greater than page size.
		/// </summary>
		/// <value><c>true</c> if paging is enabled and the current number of objects is greater than page size;
		/// otherwise, <c>false</c>.</value>
		public bool PagingEnabled
		{
			get
			{
				if (this.GalleryPage.PageId == PageId.search)
				{
					return false;
				}

				if (!this._pagingEnabled.HasValue)
				{
					int pageSize = this.GalleryPage.GallerySettings.PageSize;
					int objectCount = (this.GalleryObjectsDataSource ?? this.GalleryPage.GetAlbum().GetChildGalleryObjects(false, this.GalleryPage.IsAnonymousUser)).Count;

					this._pagingEnabled = ((pageSize > 0) && (objectCount > pageSize));
				}

				return this._pagingEnabled.Value;
			}
		}

		/// <summary>
		/// Gets the page size as specified in the configuration file.
		/// </summary>
		/// <value>The size of the page.</value>
		public int PageSize
		{
			get
			{
				if (!this._pageSize.HasValue)
				{
					this._pageSize = this.GalleryPage.GallerySettings.PageSize;
				}

				return this._pageSize.Value;
			}
		}

		/// <summary>
		/// Gets or sets the media objects to be displayed as thumbnail items. When this property is not specified, 
		/// the gallery items within the current album are displayed.
		/// </summary>
		/// <value>The gallery objects data source.</value>
		public IGalleryObjectCollection GalleryObjectsDataSource
		{
			get
			{
				return this._galleryObjectsDataSource;
			}
			set
			{
				this._galleryObjectsDataSource = value;
			}
		}

		#endregion

		#region Protected Events

		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
		}

		/// <summary>
		/// Handles the PreRender event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_PreRender(object sender, EventArgs e)
		{
				BindData();
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Gets the CSS class to apply to the thumbnail object.
		/// </summary>
		/// <param name="galleryObjectType">Type of the gallery object.</param>
		/// <returns>Returns the CSS class to apply to the thumbnail object.</returns>
		protected static string GetThumbnailCssClass(Type galleryObjectType)
		{
			// If it's an album then specify the appropriate CSS class so that the "Album"
			// header appears over the thumbnail. This is to indicate to the user that the
			// thumbnail represents an album.
			if (galleryObjectType == typeof(Album))
				return "thmb album";
			else
				return "thmb";
		}

		/// <summary>
		/// Return a string representing the title of the media object. It is truncated and purged of HTML tags
		/// if necessary. Returns an empty string if the gallery object is an album (<paramref name="galleryObjectType"/>
		/// == typeof(<see cref="Album"/>))
		/// </summary>
		/// <param name="title">The title of the media object as stored in the data store.</param>
		/// <param name="galleryObjectType">The type of the object to which the title belongs.</param>
		/// <returns>Returns a string representing the title of the media object. It is truncated and purged of HTML tags
		/// if necessary.</returns>
		protected string GetGalleryObjectText(string title, Type galleryObjectType)
		{
			if (String.IsNullOrEmpty(title))
				return title;

			// If this is an album, return an empty string. Otherwise, return the title, truncated and purged of HTML
			// tags if necessary. If the title is truncated, add an ellipses to the text.
			if (galleryObjectType == typeof(Album))
				return String.Empty;

			int maxLength = GalleryPage.GallerySettings.MaxMediaObjectThumbnailTitleDisplayLength;
			string truncatedText = Utils.TruncateTextForWeb(title, maxLength);

			if (truncatedText.Length != title.Length)
				return String.Format(CultureInfo.CurrentCulture, "<p>{0}...</p>", truncatedText);
			else
				return String.Format(CultureInfo.CurrentCulture, "<p>{0}</p>", truncatedText);
		}

		/// <summary>
		/// Gets the CSS class to apply to the container of the thumbnail objects.
		/// </summary>
		/// <returns>Returns the CSS class to apply to the container of the thumbnail objects.</returns>
		protected string GetThumbnailContainerCssClass()
		{
			if (GalleryPage.AlbumTreeViewIsVisible)
				return "gsp_floatcontainerWithTv";
			else
				return "gsp_floatcontainer";
		}

		#endregion

		#region Private Methods

		private void BindData()
		{
			//Get the data associated with the album and display
			if (this.GalleryObjectsDataSource == null)
				DisplayThumbnails(this.GalleryPage.GetAlbum().GetChildGalleryObjects(true, this.GalleryPage.IsAnonymousUser), true);
			else
				DisplayThumbnails(this.GalleryObjectsDataSource, false);
		}


		/// <summary>
		/// Displays thumbnail versions of the specified <paramref name="galleryObjects"/>.
		/// </summary>
		/// <param name="galleryObjects">The gallery objects to display.</param>
		/// <param name="showAddObjectsLink">If set to <c>true</c> show a message and a link allowing the user to add objects to the 
		/// current album as specified in the query string. Set to false when displaying objects that may belong to more than one
		/// album.</param>
		private void DisplayThumbnails(IGalleryObjectCollection galleryObjects, bool showAddObjectsLink)
		{
			string msg;
			if (galleryObjects.Count > 0)
			{
				// At least one album or media object in album.
				//msg = String.Format(CultureInfo.CurrentCulture, "<p class='gsp_addtopmargin2'>{0}</p>", Resources.GalleryServerPro.UC_ThumbnailView_Intro_Text_With_Objects);
				//phMsg.Controls.Add(new LiteralControl(msg));
			}
			else if ((showAddObjectsLink) && (this.GalleryPage.UserCanAddMediaObject) && (!this.GalleryPage.GallerySettings.MediaObjectPathIsReadOnly))
			{
				// We have no objects to display. The user is authorized to add objects to this album and the gallery is writeable, so show 
				// message and link to add objects page.
				string innerMsg = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.UC_ThumbnailView_Intro_Text_No_Objects_User_Has_Add_MediaObject_Permission, Utils.GetUrl(PageId.task_addobjects, "aid={0}", this.GalleryPage.GetAlbumId()));
				msg = String.Format(CultureInfo.CurrentCulture, "<p class='gsp_addtopmargin2 gsp_msgfriendly'>{0}</p>", innerMsg);
				phMsg.Controls.Add(new LiteralControl(msg));
			}
			else
			{
				// No objects and/or user doesn't have permission to add media objects.
				msg = String.Format(CultureInfo.CurrentCulture, "<p class='gsp_addtopmargin2 gsp_msgfriendly'>{0}</p>", Resources.GalleryServerPro.UC_ThumbnailView_Intro_Text_No_Objects);
				phMsg.Controls.Add(new LiteralControl(msg));
			}

			this.GalleryPage.SetThumbnailCssStyle(galleryObjects);

			if (PagingEnabled)
			{
				rptr.DataSource = CreatePaging(galleryObjects);
			}
			else
				rptr.DataSource = galleryObjects;

			rptr.DataBind();
		}

		private PagedDataSource CreatePaging(ICollection<IGalleryObject> galleryObjects)
		{
			PagedDataSource pds = new PagedDataSource();
			pds.DataSource = galleryObjects;
			pds.AllowPaging = true;
			pds.PageSize = PageSize;
			pds.CurrentPageIndex = this.GalleryPage.CurrentPage - 1; // Index is 0-based, CurrentPage is 1-based, so subtract one

			PagerPosition pagerPosition = this.GalleryPage.GallerySettings.PagerLocation;

			if ((pagerPosition == PagerPosition.Top) || (pagerPosition == PagerPosition.TopAndBottom))
			{
				// Configure the top pager.
				pager topPager = (pager)Page.LoadControl(Utils.GetUrl("/controls/pager.ascx"));
				topPager.DataSource = pds;
				topPager.CurrentPage = GalleryPage.CurrentPage;
				phPagerTop.Controls.Add(topPager);
			}

			if ((pagerPosition == PagerPosition.Bottom) || (pagerPosition == PagerPosition.TopAndBottom))
			{
				// Configure the bottom pager.
				pager bottomPager = (pager)Page.LoadControl(Utils.GetUrl("/controls/pager.ascx"));
				bottomPager.DataSource = pds;
				bottomPager.CurrentPage = GalleryPage.CurrentPage;
				phPagerBtm.Controls.Add(bottomPager);
			}

			return pds;
		}

		#region NOT USED: Animation script

		// See the note in GetAnimationScript for why this is commented out.
		//private void RegisterJavascript()
		//{
		//  string script = GetAnimationScript();

		//  if (!String.IsNullOrEmpty(script))
		//    ScriptManager.RegisterStartupScript(this, this.GetType(), "thumbnailViewScript", script, true);
		//}

		//    /// <summary>
		//    /// NOT USED: Fading the thumbnails was very slow, so this method is considered unusable for the time being.
		//    /// Generate the javascript required for thumbnail animation effects. If transition animation is disabled
		//    /// (mediaObjectTransitionType="None" in the config file), then return an empty string.
		//    /// </summary>
		//    /// <returns>Returns the javascript required for media object animation effects, or an empty string if no
		//    /// transition effect is configured.</returns>
		//    private static string GetAnimationScript()
		//    {
		//      string animationScript = String.Empty;
		//      MediaObjectTransitionType transitionType = (MediaObjectTransitionType)Enum.Parse(typeof(MediaObjectTransitionType), GalleryServerPro.Configuration.ConfigManager.GetGalleryServerProConfigSection().Core.MediaObjectTransitionType);
		//      switch (transitionType)
		//      {
		//        case MediaObjectTransitionType.Fade:
		//          {
		//            animationScript = String.Format(CultureInfo.InvariantCulture, @"
		//		_fadeInAlbumAnimation = new AjaxControlToolkit.Animation.FadeInAnimation($get('thmbCtnr'), {0}, 3, 0, 1, true);
		//		_fadeOutAlbumAnimation = new AjaxControlToolkit.Animation.FadeOutAnimation($get('thmbCtnr'), {0}, 3, 0, 1, true);
		//",
		//            GalleryServerPro.Configuration.ConfigManager.GetGalleryServerProConfigSection().Core.MediaObjectTransitionDuration);
		//            break;
		//          }
		//        case MediaObjectTransitionType.None: break;
		//        default: throw new WebException(String.Format(CultureInfo.CurrentCulture, "The function GetAnimationScript() in user control thumbnailview.ascx encountered the MediaObjectTransitionType \"{0}\", which it was not designed to handle. The developer must update this method to process this enum item.", transitionType.ToString()));
		//      }
		//      return animationScript;
		//    }

		// To enable animation, add the following script to the ascx page:

		//  <script type="text/javascript">
		//  var _fadeInAlbumAnimation;
		//  var _fadeOutAlbumAnimation;

		//  function cbThumbnailView_onBeforeCallback(sender, eventArgs)
		//  {
		//    if (_fadeOutAlbumAnimation)
		//    {
		//      _fadeOutAlbumAnimation.set_target($get('thmbCtnr'));
		//      _fadeOutAlbumAnimation.play();
		//    }
		//  }

		//  function cbThumbnailView_onCallbackComplete(sender, eventArgs)
		//  {
		//    if (_fadeInAlbumAnimation)
		//    {
		//      _fadeInAlbumAnimation.set_target($get('thmbCtnr'));
		//      _fadeInAlbumAnimation.play();
		//    }
		//  }
		//</script>

		#endregion

		#endregion
	}
}