using System;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using ComponentArt.Web.UI;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.ErrorHandler.CustomExceptions;
using GalleryServerPro.Web.Entity;

namespace GalleryServerPro.Web.Controls
{
	/// <summary>
	/// A user control that renders a specific media object.
	/// </summary>
	public partial class mediaobjectview : GalleryUserControl
	{
		#region Private Fields

		const int WidthBorderBuffer = 34; // Space to allow for margins, padding, and border on media object. Also defined in script/mediaobjectview.js
		const int MinMediaObjectContainerWidth = 100; // The minimum width of the HTML container for a media object
		const int NavigationButtonsWidth = 88;
		const int IndexPositionWidth = 60;

		private int _numMediaObjectsInAlbum = int.MinValue;
		private int _currentMediaObjectIndex = int.MinValue;
		private int _toolbarWidthEstimate;
		private int _maxMediaObjectWidth = int.MinValue;
		private int _maxOptimizedMediaObjectWidth = int.MinValue;
		private int _mediaObjectWidthWithBuffer = int.MinValue;
		private int _mediaObjectHeaderWidthEstimate = int.MinValue;
		private ToolBarItem _toolbarItemSeparator;
		private bool? _includeSilverlightSupportFiles;
		private MediaObjectWebEntity _mediaObjectEntity;

		#endregion

		#region Protected Events

		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			if (this.GalleryPage.GetMediaObject() == null)
			{
				Utils.Redirect(Utils.AddQueryStringParameter(Utils.GetCurrentPageUrl(), "msg=" + (int)Message.MediaObjectDoesNotExist));
			}

			if (this.GalleryPage.IsNewPageLoad)
			{
				// First time page is loading.
				RenderMediaObject();

				ConfigureMediaObjectTitle();

				ConfigureNavigation();

				ConfigureToolbar();

				ConfigureGrid();

				ShowMediaObjectMetadata();

				ConfigureSecurityRelatedControls();

				RegisterJavascript();
			}
		}

		private void ConfigureNavigation()
		{
			if (this.GalleryPage.ShowMediaObjectNavigation)
			{
				string html = String.Format(CultureInfo.InvariantCulture, @"
				<img id='imgPrevious' src='{0}/images/left_arrow.png' class='gsp_navleft gsp_addpadding2'
					onclick='showPrevMediaObject();' alt='{1}' title='{2}' />",
																		Utils.GalleryRoot, // 0
																		Resources.GalleryServerPro.UC_MediaObjectView_Previous_Text, // 1
																		Resources.GalleryServerPro.UC_MediaObjectView_Previous_Tooltip // 2
					);

				phPrevMediaObject.Controls.Add(new LiteralControl(html));

				html = String.Format(CultureInfo.InvariantCulture, @"
				<img id='imgNext' src='{0}/images/right_arrow.png' class='gsp_navright gsp_addpadding2'
				onclick='showNextMediaObject();' alt='{1}' title='{2}' />",
														 Utils.GalleryRoot, // 0
														 Resources.GalleryServerPro.UC_MediaObjectView_Next_Text, // 1
														 Resources.GalleryServerPro.UC_MediaObjectView_Next_Tooltip // 2
					);

				phNextMediaObject.Controls.Add(new LiteralControl(html));
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Web.UI.Control.PreRender"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
		protected override void OnPreRender(EventArgs e)
		{
			if (!this.GalleryPage.IsCallback)
			{
				ConfigureControls();
			}

			base.OnPreRender(e);
		}

		/// <summary>
		/// Handles the ItemCommand event of the ToolBar control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="ComponentArt.Web.UI.ToolBarItemEventArgs"/> instance containing the event data.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="e" /> is null.</exception>
		protected void tbMediaObjectActions_ItemCommand(object sender, ToolBarItemEventArgs e)
		{
			if (e == null)
				throw new ArgumentNullException("e");

			ToolBarItem tbi = e.Item;
			switch (tbi.ID)
			{
				case "tbiDownload": { DownloadMediaObjectToUser(); break; }
				case "tbiDownloadZip": { RedirectToDownloadZipPage(); break; }
				case "tbiRotate": { RedirectToRotatePage(); break; }
				case "tbiMove": { RedirectToMovePage(); break; }
				case "tbiCopy": { RedirectToCopyPage(); break; }
			}
		}

		/// <summary>
		/// Gets a CSS style setting that can be applied to the style attribute of an HTML element containing the current 
		/// media object, header area (navigation arrows, toolbar buttons, etc.), and caption. May return an empty string. 
		/// Example: "width:640px;"
		/// </summary>
		/// <returns>Returns a string containing a CSS style setting.</returns>
		protected string GetMoViewStyle()
		{
			// Use the larger of the two values, ensuring a minimum width of 100px.
			int width = (MediaObjectHeaderWidthEstimate > MediaObjectWidthWithBuffer ? MediaObjectHeaderWidthEstimate : MediaObjectWidthWithBuffer);

			if (width > 0)
			{
				if (width < MinMediaObjectContainerWidth)
				{
					width = MinMediaObjectContainerWidth;
				}

				return "width:" + width + "px;";
			}
			else
			{
				return String.Empty;
			}
		}

		/// <summary>
		/// Gets the style to be applied to the header area that appears above the media object. The header contains the navigation arrows, 
		/// toolbar buttons, etc. Example: "width:640px;"
		/// </summary>
		/// <returns>Returns a string containing a CSS style setting.</returns>
		protected string GetMediaObjectHeaderContainerStyle()
		{
			if (GalleryPage.ShowMediaObjectNavigation || GalleryPage.ShowMediaObjectToolbar || GalleryPage.ShowMediaObjectIndexPosition)
			{
				// Use the larger of the two values.
				int width = (MediaObjectHeaderWidthEstimate > MediaObjectWidthWithBuffer ? MediaObjectHeaderWidthEstimate : MediaObjectWidthWithBuffer);

				// We don't want the header to be too wide, so when there are very large objects, just set the value to be the width of the widest
				// optimized image. Without this the header is too wide when viewing original images.
				if ((MaxOptimizedMediaObjectWidthWithBuffer > MediaObjectHeaderWidthEstimate) && (width > MaxOptimizedMediaObjectWidthWithBuffer))
				{
					width = MaxOptimizedMediaObjectWidthWithBuffer;
				}

				return String.Concat("width:", width, "px;");
			}
			else
			{
				// Hide when we aren't showing any of the items the header can contain.
				return "display:none;";
			}
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets the number of media objects in the album.
		/// </summary>
		/// <value>The number of media objects in the album.</value>
		private int NumMediaObjectsInAlbum
		{
			get
			{
				if (_numMediaObjectsInAlbum == int.MinValue)
					InitializeVariables();

				return _numMediaObjectsInAlbum;
			}
			set { _numMediaObjectsInAlbum = value; }
		}

		/// <summary>
		/// Gets or sets the zero-based index of the current media object.
		/// </summary>
		/// <value>The index of the current media object.</value>
		private int CurrentMediaObjectIndex
		{
			get
			{
				if (_currentMediaObjectIndex == int.MinValue)
					InitializeVariables();

				return _currentMediaObjectIndex;
			}
			set { _currentMediaObjectIndex = value; }
		}


		/// <summary>
		/// Gets a value indicating whether to display the high resolution version of the image.
		/// </summary>
		/// <value><c>true</c> if [view hi res image]; otherwise, <c>false</c>.</value>
		private bool ViewHiResImage
		{
			get
			{
				// Get from hidden form field first. If not there, look at query string. If not there, look at config file.
				bool viewHiRes;
				object formFieldHiRes = Request.Form["hr"];
				if ((formFieldHiRes == null) || (!Boolean.TryParse(formFieldHiRes.ToString(), out viewHiRes)))
				{
					bool? configValue = Utils.GetQueryStringParameterBoolean("hr");

					viewHiRes = (configValue.HasValue ? configValue.Value : GalleryPage.GallerySettings.ThumbnailClickShowsOriginal);
				}

				if (viewHiRes && !this.GalleryPage.UserCanViewHiResImage)
				{
					// User is not authorized to view the original, so deny it even though it is being requested.
					viewHiRes = false;
				}

				return viewHiRes;
			}
		}

		/// <summary>
		/// Gets or sets the media object entity.
		/// </summary>
		/// <value>The media object entity.</value>
		private MediaObjectWebEntity MediaObjectEntity
		{
			get { return _mediaObjectEntity; }
			set { _mediaObjectEntity = value; }
		}

		/// <summary>
		/// Gets the width of the widest media object in the current album. Returns int.MinValue if none of the media objects
		/// have a width (for example, they are all PSD files that don't have an optimized image).
		/// </summary>
		/// <value>The width of the widest media object in the current album.</value>
		private int MaxMediaObjectWidth
		{
			get
			{
				if (_maxMediaObjectWidth == int.MinValue)
					EvaluateMediaObjects();

				return _maxMediaObjectWidth;
			}
		}

		/// <summary>
		/// Gets the width of the widest media object in the current album, including extra space to account for margins, padding,
		/// and border. Returns int.MinValue if none of the media objects have a width (for example, they are all PSD files that 
		/// don't have an optimized image).
		/// </summary>
		/// <value>The width of the widest media object in the current album, including extra space to account for margins, padding,
		/// and border.</value>
		private int MediaObjectWidthWithBuffer
		{
			get
			{
				if (_mediaObjectWidthWithBuffer == int.MinValue)
				{
					if (MaxMediaObjectWidth > int.MinValue)
					{
						_mediaObjectWidthWithBuffer = MaxMediaObjectWidth + WidthBorderBuffer;
					}
				}

				return _mediaObjectWidthWithBuffer;
			}
		}

		/// <summary>
		/// Gets the width of the widest optimized image in the current album, including extra space to account for margins, padding,
		/// and border. Returns int.MinValue if no optimized images of media objects exist in this album.
		/// </summary>
		/// <value>The width of the widest optimized image in the current album, including extra space to account for margins, padding,
		/// and border.</value>
		private int MaxOptimizedMediaObjectWidthWithBuffer
		{
			get
			{
				if (_maxOptimizedMediaObjectWidth == int.MinValue)
				{
					EvaluateMediaObjects();
				}

				if (_maxOptimizedMediaObjectWidth == int.MinValue)
				{
					return _maxOptimizedMediaObjectWidth;
				}
				else
				{
					return _maxOptimizedMediaObjectWidth + WidthBorderBuffer;
				}
			}
		}

		/// <summary>
		/// Gets the CSS class to be used when the album treeview is displayed in the left column.
		/// </summary>
		/// <value>The CSS class to be used when the album treeview is displayed in the left column.</value>
		protected string AlbumTreeViewBufferCssClass
		{
			get
			{
				if (this.GalleryPage.AlbumTreeViewIsVisible)
				{
					return " gsp_treeviewBuffer";
				}
				else
				{
					return String.Empty;
				}
			}
		}

		/// <summary>
		/// Gets the approximate width of the Toolbar control as it is rendered in the browser. This is only an estimate.
		/// </summary>
		/// <value>The approximate width of the Toolbar control as it is rendered in the browser.</value>
		private int ToolbarWidthEstimate
		{
			get { return _toolbarWidthEstimate; }
		}

		/// <summary>
		/// Gets the approximate width of the HTML table that contains the navigation arrows, Toolbar control, and media object
		/// index counter (e.g. "(3 of 10)"). This is only an estimate.
		/// </summary>
		/// <value>The approximate width of the HTML table that contains the navigation arrows, Toolbar control, and media object
		/// index counter (e.g. "(3 of 10)").</value>
		private int MediaObjectHeaderWidthEstimate
		{
			get
			{
				if (_mediaObjectHeaderWidthEstimate == int.MinValue)
				{
					_mediaObjectHeaderWidthEstimate = CalculateMediaObjectHeaderWidthEstimate();
				}

				return _mediaObjectHeaderWidthEstimate;
			}
		}

		/// <summary>
		/// Calculates the approximate width of the HTML table that contains the navigation arrows, Toolbar control, and media object
		/// index counter (e.g. "(3 of 10)"). This is only an estimate.
		/// </summary>
		/// <returns>Returns approximate width of the HTML table that contains the navigation arrows, Toolbar control, and media object
		/// index counter (e.g. "(3 of 10)").</returns>
		private int CalculateMediaObjectHeaderWidthEstimate()
		{
			int mediaObjectHeaderWidthEstimate = ToolbarWidthEstimate;

			if (GalleryPage.ShowMediaObjectNavigation)
			{
				mediaObjectHeaderWidthEstimate += NavigationButtonsWidth; // Left/right arrows require about 88 px.
			}

			if (GalleryPage.ShowMediaObjectIndexPosition)
			{
				// The index position string requires about 60px (actually, it varies, so this is a little high for safety)
				mediaObjectHeaderWidthEstimate += IndexPositionWidth;
			}

			// Make sure we have a reasonable minimum width.
			if (mediaObjectHeaderWidthEstimate < MinMediaObjectContainerWidth)
			{
				mediaObjectHeaderWidthEstimate = MinMediaObjectContainerWidth;
			}

			return mediaObjectHeaderWidthEstimate;
		}

		/// <summary>
		/// Gets a value indicating whether the Silverlight javascript files should be included in the page output.
		/// This will be true if there is at least one Silverlight-capable media file in the current album.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the Silverlight javascript files should be included in the page output; otherwise, <c>false</c>.
		/// </value>
		private bool IncludeSilverlightSupportFiles
		{
			get
			{
				if (!_includeSilverlightSupportFiles.HasValue)
				{
					EvaluateMediaObjects();
				}

				if (!_includeSilverlightSupportFiles.HasValue)
				{
					throw new WebException("The function EvaluateMediaObjects() should have assigned a value to the field _includeSilverlightSupportFiles, but it did not.");
				}

				return _includeSilverlightSupportFiles.Value;
			}
		}

		#endregion

		#region Private Methods

		private void EvaluateMediaObjects()
		{
			// Take a look at each media object in the album and determine a few things:
			// 1. Calculate the width of the widest media object in the album, taking into account whether the user is currently
			//    viewing a high-res image.
			// 2. Calculate the width of the widest optimized image in the current album.
			// 3. Determine if there are any Silverlight-capable files.
			//
			// This function assigns values to these page variables: _mediaObjectContainerWidth, _toolbarContainerWidth,
			// and _includeSilverlightSupportFiles.
			int maxOptimizedMediaObjectWidth = int.MinValue;
			int maxMediaObjectWidth = int.MinValue;
			bool excludePrivateObjects = this.GalleryPage.IsAnonymousUser;
			bool? viewHiRes = Utils.GetQueryStringParameterBoolean("hr");
			_includeSilverlightSupportFiles = false; // Initialize to false; we'll set to true if we find any Silverlight files
			int curMediaObjectId = this.GalleryPage.GetMediaObjectId();

			foreach (IGalleryObject mo in this.GalleryPage.GetAlbum().GetChildGalleryObjects(false, excludePrivateObjects))
			{
				if (mo is GalleryServerPro.Business.Image)
				{
					// We have an image, so we need to take into consideration the optimized properties
					if (mo.Optimized.Width > maxOptimizedMediaObjectWidth)
					{
						maxOptimizedMediaObjectWidth = mo.Optimized.Width;
					}

					if ((mo.Id == curMediaObjectId) && (viewHiRes.HasValue && viewHiRes.Value))
					{
						if (mo.Original.Width > maxMediaObjectWidth)
						{
							// Only grab the original width from the *current* media object when the user is requesting the high-res version.
							// If we grabbed the original widths for all items, we might end up with a very large width, even though the current
							// high-res image's width might be small, resulting in a small item being centered in a very large DOM element.
							maxMediaObjectWidth = mo.Original.Width;
						}
					}
					else
					{
						if (mo.Optimized.Width > maxMediaObjectWidth)
						{
							maxMediaObjectWidth = mo.Optimized.Width;
						}
					}
				}
				else
				{
					// This is not an image or generic media object.
					if (mo.Original.Width > maxMediaObjectWidth)
					{
						maxMediaObjectWidth = mo.Original.Width;
					}
				}

				if (Array.IndexOf<string>(GalleryPage.GallerySettings.SilverlightFileTypes, mo.Original.MimeType.Extension.ToLowerInvariant()) >= 0)
				{
					this._includeSilverlightSupportFiles = true; // There is a Silverlight-capable file in this album
				}
			}

			_maxMediaObjectWidth = maxMediaObjectWidth;
			_maxOptimizedMediaObjectWidth = maxOptimizedMediaObjectWidth;
		}

		private void BindGrid()
		{
			IGalleryObjectMetadataItemCollection metadata = this.GalleryPage.GetMediaObject().MetadataItems.GetVisibleItems();
			Grid gd = ((Grid)this.GalleryPage.FindControlRecursive(dgMediaObjectInfo, "gdmeta"));
			gd.DataSource = metadata;
			gd.DataBind();
		}

		private void InitializeVariables()
		{
			IGalleryObject mediaObject = this.GalleryPage.GetMediaObject();
			IAlbum album = (IAlbum)mediaObject.Parent;

			System.Diagnostics.Debug.Assert(album.Equals(this.GalleryPage.GetAlbum()), "GalleryPage.GetAlbum() is not the same object in memory as GalleryPage.GetMediaObject().Parent().");

			bool excludePrivateObjects = this.GalleryPage.IsAnonymousUser;
			IGalleryObjectCollection siblings = album.GetChildGalleryObjects(GalleryObjectType.MediaObject, true, excludePrivateObjects);

			this.NumMediaObjectsInAlbum = siblings.Count;
			this.CurrentMediaObjectIndex = siblings.IndexOf(mediaObject);
		}

		private void ConfigureControls()
		{
			ScriptManager.RegisterHiddenField(this, "hr", this.ViewHiResImage.ToString());

			//imgNext.ImageUrl = Utils.GetUrl("/images/right_arrow.png");
			//imgPrevious.ImageUrl = Utils.GetUrl("/images/left_arrow.png");

			// Set the position text
			if (GalleryPage.ShowMediaObjectIndexPosition)
			{
				string positionText = String.Format(CultureInfo.CurrentCulture, "(<span id='lblMoPosition'>{0}</span> {1} <span id='lblMoCount'>{2}</span>)", this.CurrentMediaObjectIndex + 1, Resources.GalleryServerPro.UC_MediaObjectView_Position_Separator_Text, this.NumMediaObjectsInAlbum);
				phPosition.Controls.Add(new System.Web.UI.LiteralControl(positionText));
			}
		}

		private void ConfigureGrid()
		{
			// The EmptyGridText property cannot be declaratively styled or assigned to a resource string, so we'll do it programmatically.
			string emptyGridText = String.Format(System.Globalization.CultureInfo.CurrentCulture, "<span class=\"gsp_msgfriendly gdInfoEmptyGridText\">{0}</span>", Resources.GalleryServerPro.UC_MediaObjectView_Info_Empty_Grid_Text);
			Grid grid = ((Grid)this.GalleryPage.FindControlRecursive(this.dgMediaObjectInfo, "gdmeta"));
			grid.EmptyGridText = emptyGridText;
			grid.ImagesBaseUrl = String.Concat(Utils.GalleryRoot, "/images/componentart/grid/");
		}

		private void RenderMediaObject()
		{
			IGalleryObject mediaObject = this.GalleryPage.GetMediaObject();

			if (mediaObject == null)
				Utils.Redirect(Utils.AddQueryStringParameter(Utils.GetCurrentPageUrl(), "msg=" + (int)Message.MediaObjectDoesNotExist));

			IGalleryObjectCollection siblings = ((IAlbum)mediaObject.Parent).GetChildGalleryObjects(GalleryObjectType.MediaObject, !Utils.IsAuthenticated);
			int mediaObjectIndex = siblings.IndexOf(mediaObject);
			if (mediaObjectIndex < 0)
			{
				// The media object is not the same item in memory as the media object in the album. Clear the cache
				// and reload the page. This is definitely a hack.
				HelperFunctions.PurgeCache();
				Utils.Redirect(PageId.mediaobject, "moid={0}", mediaObject.Id);
			}

			DisplayObjectType displayType = (this.ViewHiResImage ? DisplayObjectType.Original : DisplayObjectType.Optimized);

			//if (mediaObject is GalleryServerPro.Business.Image)
			//{
			//  displayType = (this.ViewHiResImage ? DisplayObjectType.Original : DisplayObjectType.Optimized);
			//}
			//else if (mediaObject is GalleryServerPro.Business.Video && !String.IsNullOrEmpty(mediaObject.Optimized.FileName))
			//{
			//  displayType = DisplayObjectType.Optimized;
			//}

			MediaObjectWebEntity mo = Pages.GalleryPage.GetMediaObjectHtml(mediaObject, displayType, false);

			pnlMediaObject.Controls.Add(new LiteralControl(mo.HtmlOutput));

			// If there is an javascript that needs to be sent, register it now.
			if (!String.IsNullOrEmpty(mo.ScriptOutput))
			{
				ScriptManager.RegisterStartupScript(this, this.GetType(), "mediaObjectStartupScript", mo.ScriptOutput, true);
			}

			this.MediaObjectEntity = mo;

			moTitle.InnerHtml = mo.Title;
		}

		private void DownloadMediaObjectToUser()
		{
			IGalleryObject mediaObject = this.GalleryPage.GetMediaObject();
			IDisplayObject displayObject = mediaObject.Original;
			bool mediaObjectIsImage = (mediaObject is GalleryServerPro.Business.Image);

			// For images, user should always get the original, unless she doesn't have permission
			// to view it. For other media types, user should get whichever one they are currently 
			// viewing.
			if (mediaObjectIsImage)
			{
				if (!this.GalleryPage.IsUserAuthorized(SecurityActions.ViewOriginalImage))
				{
					// User does not have permission to view original, hi-res images. Give user the optimized version, unless
					// it doesn't exist (it won't exist when the original was too small to require an optimized one).
					if (!String.IsNullOrEmpty(mediaObject.Optimized.FileNamePhysicalPath))
						displayObject = mediaObject.Optimized;
				}
			}
			else if ((!this.ViewHiResImage) && (!String.IsNullOrEmpty(mediaObject.Optimized.FileNamePhysicalPath)))
			{
				// Grab optimized version if the object is an image, the user is requesting the optimized version, and
				// the optimized version has a valid filepath. (There may not be a valid filepath for those images that Gallery Server
				// can't create an optimized version for; corrupted images, for example.)
				displayObject = mediaObject.Optimized;
			}

			Response.Clear();
			IMimeType mimeType = mediaObject.MimeType;
			if ((!String.IsNullOrEmpty(mimeType.MajorType)) && (!String.IsNullOrEmpty(mimeType.Subtype)))
				Response.ContentType = mediaObject.MimeType.FullType;
			else
				Response.ContentType = String.Format(CultureInfo.CurrentCulture, "Unknown/{0}", System.IO.Path.GetExtension(displayObject.FileName).Replace(".", String.Empty));

			string fileName = MakeFileNameDownloadFriendly(displayObject.FileName);

			Response.AppendHeader("Content-Disposition", "attachment;filename=\"" + fileName + "\"");

			bool applyWatermark = GalleryPage.GallerySettings.ApplyWatermark;

			if (AppSetting.Instance.License.IsInReducedFunctionalityMode ||
				(applyWatermark && mediaObjectIsImage && (!this.GalleryPage.IsUserAuthorized(SecurityActions.HideWatermark))))
				TransmitMediaObjectWithWatermark(displayObject.FileNamePhysicalPath); // This is an image and user does not have the 'hide watermark' permission
			else
				Response.TransmitFile(displayObject.FileNamePhysicalPath); // User has permission to have watermark applied or this is not an image

			//if (!isMediaObjectImage)
			//{
			// Don't send Response.End if the current media object is an image. It causes metadata popup to give "The data could not be loaded" error 
			// when navigating media objects. However, we need it in order for ZIP files to be sent to the client (without it they should up empty).
			// Since the user probably has the metadata popup only when viewing images, it is a reasonable workaround to send End() for non-images.
			Response.End();
			//}
		}

		/// <summary>
		/// Makes the file name download friendly by removing the optimized prefix ("zOpt_"). Also,
		/// since IE automatically replaces spaces with underscores, we can prevent this by encoding them.
		/// The fileName is unmodified for all other browsers. Example: If fileName="My cat.jpg" and the current browser is IE, this function 
		/// returns "My%20cat.jpg".
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <returns>Returns the <paramref name="fileName" /> parameter, modified if necessary.</returns>
		private string MakeFileNameDownloadFriendly(string fileName)
		{
			if (Request.Browser.Browser.Equals("IE", StringComparison.OrdinalIgnoreCase))
			{
				fileName = fileName.Replace(" ", "%20");
			}

			return fileName.Replace(this.GalleryPage.GallerySettings.OptimizedFileNamePrefix, String.Empty);
		}

		private void TransmitMediaObjectWithWatermark(string filepath)
		{
			// Send the specified file to the client with the watermark overlayed on top.
			try
			{
				System.Drawing.Image watermarkedImage = ImageHelper.AddWatermark(filepath, GalleryPage.GalleryId);
				watermarkedImage.Save(Response.OutputStream, System.Drawing.Imaging.ImageFormat.Jpeg);
			}
			catch
			{
				// Can't apply watermark to image. Abort mission and display error message.
				string redirectUrl = Utils.AddQueryStringParameter(Utils.GetCurrentPageUrl(true), String.Format(CultureInfo.CurrentCulture, "msg={0}", (int)Message.CannotOverlayWatermarkOnImage));
				Response.Redirect(redirectUrl, false);
				System.Web.HttpContext.Current.ApplicationInstance.CompleteRequest();
			}
		}

		private void ShowMediaObjectMetadata()
		{
			if (GalleryPage.ShowMediaObjectToolbar && GalleryPage.GallerySettings.EnableMetadata)
			{
				ToolBarItem tbiInfo = FindToolBarItem("tbiInfo");
				if (tbiInfo != null)
				{
					tbiInfo.Checked = Controller.ProfileController.GetProfileForGallery(GalleryPage.GalleryId).ShowMediaObjectMetadata;
				}

				// Bind the grid to the data. This should not be necessary because we have javascript code run during the client grid load 
				// event that displays the metadata if the toolbar icon is checked (from the previous line of code). However, unless
				// we bind the grid on the server, the column widths are not correctly rendered, and no amount of tinkering seems to 
				// do the trick. In subsequent releases of the CA grid control, we can try commenting out this line. (If you do that,
				// replace the line gdmeta.render(); with refreshMetadata($get('moid').value); in the javascript function gdmeta_onLoad.
				BindGrid();
			}
			else
			{
				dgMediaObjectInfo.Visible = false;
			}
		}

		private ToolBarItem FindToolBarItem(string id)
		{
			foreach (ToolBarItem tbItem in tbMediaObjectActions.Items)
			{
				if (tbItem.ID == id)
					return tbItem;
			}

			return null;
		}

		private void RegisterJavascript()
		{
			// Add reference to a few script files.
			ScriptManager sm = ScriptManager.GetCurrent(GalleryPage.Page);
			if (sm != null)
			{
#if DEBUG
				sm.Scripts.Add(new ScriptReference(Utils.GetUrl("/script/mediaobjectview.debug.js")));
#else
				sm.Scripts.Add(new ScriptReference(Utils.GetUrl("/script/mediaobjectview.js")));
#endif
				if (IncludeSilverlightSupportFiles)
				{
					sm.Scripts.Add(new ScriptReference(Utils.GetUrl("/script/SilverlightControl.js")));
					sm.Scripts.Add(new ScriptReference(Utils.GetUrl("/script/SilverlightMedia.js")));
				}
			}
			else
				throw new WebException("Gallery Server Pro requires a ScriptManager on the page.");

			// Add dynamically built javascript.
			string script = String.Format(CultureInfo.InvariantCulture, @"
	var _mo = '{0}';
	var _moTitle = '{1}';
	var _optimizedMediaObjectContainerWidth = {2};
	var _ssDelay = {3};
	var _viewHiRes = {4};
	var _tbIsVisible = {5};
	var _moid = {6};
	var _galleryResourcesUrl = '{14}';

	function redirectToHomePage()
	{{
		window.location = '{7}';
	}}

	function togglePermalink(toolbarItem)
	{{
		var iptEmbedCodeTag = $get('iptEmbedCode');
		iptEmbedCodeTag.value = getEmbedCode(_moInfo.Id);

		var permalinkUrlTag = $get('permaLinkUrlTag');
		var url = getPermalink(_moInfo.Id);
		permalinkUrlTag.innerHTML = ""<a href='"" + url + ""' title='{8}'>"" + url + ""</a>"";

		var permalinkContainer = $get('divPermalink');
		var showPermalink = toolbarItem.get_checked();
		if (showPermalink)
		{{
			Sys.UI.DomElement.removeCssClass(permalinkContainer, 'gsp_invisible');
			Sys.UI.DomElement.addCssClass(permalinkContainer, 'gsp_visible');
		}}
		else
		{{
			Sys.UI.DomElement.removeCssClass(permalinkContainer, 'gsp_visible');
			Sys.UI.DomElement.addCssClass(permalinkContainer, 'gsp_invisible');
		}}
	}}

	{9}

	{10}

	{11}
	
	{12}	

	{13}		

			",
				pnlMediaObject.ClientID, // 0
				moTitle.ClientID, // 1
				this.MaxOptimizedMediaObjectWidthWithBuffer, // 2
				this.GalleryPage.GallerySettings.SlideshowInterval, // 3
				this.ViewHiResImage.ToString().ToLowerInvariant(), // 4
				tbMediaObjectActions.Visible.ToString().ToLowerInvariant(), // 5
				this.MediaObjectEntity.Id, // 6
				Utils.GetUrl(PageId.album, "aid={0}", this.GalleryPage.GetAlbumId()), // 7
				Resources.GalleryServerPro.UC_MediaObjectView_Permalink_Url_Tooltip, // 8
				GetToggleHiResScript(), // 9
				GetDeleteMediaObjectScript(), // 10
				GetPageLoadScript(), // 11
				GetEditMediaObjectScript(), // 12
				GetEditAlbumScript(), // 13
				Utils.GetGalleryResourcesUrl() // 14
				);

			ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "mediaObjectViewScript", script, true);
		}

		/// <summary>
		/// Generate the javascript to show or hide the high resolution version of an image.
		/// </summary>
		/// <returns>Returns the javascript to show or hide the high resolution version of an image.</returns>
		private string GetToggleHiResScript()
		{
			string script = String.Empty;

			if (this.GalleryPage.UserCanViewHiResImage)
			{
				script = @"
	function toggleHiRes(toolbarItem)
	{
		if (toolbarItem.get_checked())
		{
			if (_moInfo.HiResAvailable)
			{
				_viewHiRes = true;
				$get('hr').value = _viewHiRes;
				showMediaObject(_moInfo.Id);
			}
			else
			{
				_viewHiRes = false;
				toolbarItem.set_checked(false);
			}
		}
		else
		{
			_viewHiRes = false;
			$get('hr').value = _viewHiRes;
			$get('divMoView').style.width = _optimizedMediaObjectContainerWidth + 'px';
			showMediaObject(_moInfo.Id);
		}
	}
";
			}

			return script;
		}

		/// <summary>
		/// Generate the javascript to support deleting the current media object. Returns an empty string 
		/// if the logged on user does not have this permission.
		/// </summary>
		/// <returns>Returns the javascript to support deleting the current media object,
		/// or an empty string if the logged on user does not have this permission.</returns>
		private string GetDeleteMediaObjectScript()
		{
			string script = String.Empty;

			if (this.GalleryPage.UserCanEditMediaObject)
			{
				script = String.Format(CultureInfo.InvariantCulture, @"
	function deleteObject(toolbarItem)
	{{
		var question = '{0}';
		if (confirm(question))
		{{
			Gsp.Gallery.DeleteMediaObject(_moInfo.Id, getDeleteMediaObjectCompleted);
		}}
	}}
	
	function getDeleteMediaObjectCompleted(results, context, methodName)
	{{
		_moInfo.NumObjectsInAlbum = _moInfo.NumObjectsInAlbum - 1;
		_moInfo.Index = _moInfo.Index - 1;
		showNextMediaObject();
	}}
", Resources.GalleryServerPro.UC_MediaObjectView_ToolBar_Delete_Confirmation_Msg);
			}

			return script;
		}

		/// <summary>
		/// Generate the javascript to support the scenario where the logged on user has edit media object permission,
		/// Returns an empty string if the logged on user does not have this permission.
		/// </summary>
		/// <returns>Returns the javascript to support the scenario where the logged on user has edit media object permission,
		/// or an empty string if the logged on user does not have this permission.</returns>
		private string GetEditMediaObjectScript()
		{
			string script = String.Empty;

			if (this.GalleryPage.UserCanEditMediaObject)
			{
				script = String.Format(CultureInfo.InvariantCulture, @"
	function editCaption()
	{{
		var moTitle = $get('{0}');
		var dgHeight = moTitle.clientHeight < 110? 110 : moTitle.clientHeight;
		setDialogSize(dgEditCaption, moTitle.clientWidth, dgHeight);
		dgEditCaption.show();
		$get('taCaption').value = $get(_moTitle).innerHTML.replace(/&amp;/g, '&');
		$get('taCaption').focus();
	}}

	function saveCaption(title)
	{{
		document.body.style.cursor = 'wait';
		Gsp.Gallery.UpdateMediaObjectTitle(_moInfo.Id, title, updateMediaObjectTitleCompleted);
	}}
	
	function updateMediaObjectTitleCompleted(results, context, methodName)
	{{
		$get(_moTitle).innerHTML = results;
		dgEditCaption.close();
		document.body.style.cursor = 'default';
	}}
", pnlMediaObjectTitle.ClientID);
			}

			return script;
		}

		/// <summary>
		/// Generate the javascript to support the scenario where the logged on user has edit album permission,
		/// Returns an empty string if the logged on user does not have this permission.
		/// </summary>
		/// <returns>Returns the javascript to support the scenario where the logged on user has edit album permission,
		/// or an empty string if the logged on user does not have this permission.</returns>
		private string GetEditAlbumScript()
		{
			string script = String.Empty;

			if (this.GalleryPage.UserCanEditAlbum)
			{
				IAlbum album = this.GalleryPage.GetAlbum();
				script = String.Format(CultureInfo.InvariantCulture, @"

	var _dateFormatString = 'd'; // Short date format
	var _albumId = {0};

	function editAlbumInfo()
	{{
		dgEditAlbum.show();
		document.body.style.cursor = 'wait';
		Gsp.Gallery.GetAlbumInfo(_albumId, getAlbumInfoCompleted);
	}}
	
	function getAlbumInfoCompleted(results, context, methodName)
	{{
		$get(_txtTitleId).focus();
		dgEditAlbum.set_title('{1}: ' + results.Title);
		$get(_txtTitleId).value = results.Title;

		$get('albumSummary').value = results.Summary;
		$get('private').checked = results.IsPrivate;
		$get('private').disabled = {2};

		if (results.DateStart > new Date(1,1,1))
		{{
			$get('beginDate').value = results.DateStart.localeFormat(_dateFormatString);
			var cdrDateStart = $find(_cdrBeginDateId);
			cdrDateStart.setSelectedDate(results.DateStart);		
		}}

		if (results.DateEnd > new Date(1,1,1))
		{{
			$get('endDate').value = results.DateEnd.localeFormat(_dateFormatString);
			var cdrDateEnd = $find(_cdrEndDateId);
			cdrDateEnd.setSelectedDate(results.DateEnd);		
		}}
		
		document.body.style.cursor = 'default';
	}}

	function saveAlbumInfo()
	{{
		document.body.style.cursor = 'wait';
		var albumEntity = new Gsp.AlbumWebEntity();
		albumEntity.Id = _albumId;
		albumEntity.Title = $get(_txtTitleId).value;
		albumEntity.Summary = $get('albumSummary').value;
		albumEntity.IsPrivate = $get('private').checked;
				
		if (typeof (cboOwner) != 'undefined')
			albumEntity.Owner = cboOwner.get_text(); 

		var beginDate = Date.parseLocale($get('beginDate').value, _dateFormatString);
		if (beginDate != null)
		{{
			var tzo = (beginDate.getTimezoneOffset()/60)*(-1);
			if (tzo > 0)
			{{
				beginDate.setHours(beginDate.getHours() + tzo);
			}}
			_beginDate = beginDate;
			albumEntity.DateStart = beginDate;
		}}
		else
		{{
			_beginDate = null;
		}}

		var endDate = Date.parseLocale($get('endDate').value, _dateFormatString);
		if (endDate != null)
		{{
			var tzo = (endDate.getTimezoneOffset()/60)*(-1);
			if (tzo > 0)
			{{
				endDate.setHours(endDate.getHours() + tzo);
			}}
			_endDate = endDate;
			albumEntity.DateEnd = endDate;
		}}
		else
		{{
			_endDate = null;
		}}

		Gsp.Gallery.UpdateAlbumInfo(albumEntity, updateAlbumInfoCompleted);
		dgEditAlbum.close();
	}}

	function updateAlbumInfoCompleted(results, context, methodName)
	{{
		setText($get('currentAlbumLink'), results.Title);
		document.body.style.cursor = 'default';
	}}

	function setText(node, newText)
	{{
		var childNodes = node.childNodes;
		for (var i=0; i < childNodes.length; i++)
		{{
			node.removeChild(childNodes[i]);
		}}
		if ((newText != null) && (newText.length > 0))
			node.appendChild(document.createTextNode(newText));
	}}
",
				album.Id, // 0
				Resources.GalleryServerPro.UC_Album_Header_Dialog_Title_Edit_Album, // 1
				album.Parent.IsPrivate.ToString().ToLowerInvariant() // 2
					);

				// Add script reference to the javascript file that defines the Gsp.AlbumWebEntity class. This is needed to allow 
				// javascript to be able to instantiate the class.
				string scriptUrl = Utils.GetUrl("/script/entityobjects.js");
				ScriptManager sm = ScriptManager.GetCurrent(GalleryPage.Page);
				if (sm != null)
					sm.Scripts.Add(new ScriptReference(scriptUrl));
				else
					throw new WebException("Gallery Server Pro requires a ScriptManager on the page.");
			}

			return script;
		}

		/// <summary>
		/// Generate the javascript function that will run when the page loads. It includes support for media object 
		/// animation effects and the keydown handler for navigating between media objects. This function is registered
		/// to run automatically when the page loads with: Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(moViewPageLoad);
		/// This registration occurs in the javascript in mediaobjectview.ascx.
		/// </summary>
		/// <returns>Returns the javascript function to be added to the page output.</returns>
		private string GetPageLoadScript()
		{
			// Note: Even though we programatically set the hidden field using ScriptManager.RegisterHiddenField in the page load
			// event of GalleryPage_Init, we need to explicitly assign it here using $get('moid').value = _moid;
			// This is because Firefox caches the previous value during a page reload (F5), but we need the moid that is in
			// the query string to be the new value. This function is the only place we use the _moid variable. (IE does not have this issue.)
			string pageLoadScript = String.Format(CultureInfo.InvariantCulture, @"	
	function moViewPageLoad(sender, args)
	{{
		$addHandler(document.getElementsByTagName('html')[0], 'keydown', html_onkeydown);
		$get('moid').value = _moid;
		_galleryId = {0};

		_inPrefetch = true;
		_pageLoadHasFired = true;
		Gsp.Gallery.GetMediaObjectHtml(_moid, getDisplayType(), getMediaObjectHtmlCompleted, getMediaObjectHtmlFailureOnNavigate);
		{1}
		{2}
	}}

	{3}
",
			GalleryPage.GalleryId, // 0
			GetAnimationScript(), // 1
			GetAutoStartSlideShowScript(), // 2
			GetKeyDownEventHandler()); // 3

			return pageLoadScript;
		}

		private string GetKeyDownEventHandler()
		{
			if (this.GalleryPage.ShowMediaObjectNavigation)
			{
				return @"
	function html_onkeydown(e)
	{{
		var tag = e.target.tagName.toLowerCase();

		if ((tag == 'input') || (tag == 'textarea'))
			return; // Ignore when focus is in editable box
		
		if (e.keyCode === Sys.UI.Key.right)
			showNextMediaObject();
		else if (e.keyCode === Sys.UI.Key.left)
			showPrevMediaObject();
	}}
";
			}
			else
			{
				return @"function html_onkeydown(e)	{{ }}";
			}
		}

		/// <summary>
		/// Generate the javascript required for media object animation effects.
		/// </summary>
		/// <returns>Returns the javascript required for media object animation effects.</returns>
		private string GetAnimationScript()
		{
			return String.Format(CultureInfo.InvariantCulture, @"
_mediaObjectTransitionType = '{0}';
_mediaObjectTransitionDuration = {1};
", 
			  GalleryPage.GallerySettings.MediaObjectTransitionType.ToString(),
			  ToMilliseconds(GalleryPage.GallerySettings.MediaObjectTransitionDuration));
		}

		private static float ToMilliseconds(float seconds)
		{
			return seconds*1000;
		}

		private string GetAutoStartSlideShowScript()
		{
			string autoStartSlideShowScript = String.Empty;

			if (GalleryPage.AutoPlaySlideShow && (this.GalleryPage.GetMediaObject().MimeType.TypeCategory == MimeTypeCategory.Image))
			{
				autoStartSlideShowScript = "startSlideShow();";
			}

			return autoStartSlideShowScript;
		}

		/// <summary>
		/// Generate the javascript that preloads all optimized or original images in the album.
		/// </summary>
		/// <returns>Returns the javascript that preloads all optimized or original images in the album.</returns>
		//private string GetImagePreloadScript()
		//{
		//  string preloadScript = String.Empty;
		//  string imgPath = String.Empty;
		//  bool viewHiRes = this.ViewHiResImage;
		//  int moIterator = 1;
		//  IGalleryObjectCollection imageObjects = this.GalleryPage.GetAlbum().GetChildGalleryObjects(GalleryObjectType.Image, true);

		//  if (imageObjects.Count > 6)
		//  {
		//    // More than 6 objects - use a StringBuilder
		//    System.Text.StringBuilder sbPreloadScript = new System.Text.StringBuilder(imageObjects.Count);
		//    foreach (IGalleryObject mo in imageObjects)
		//    {
		//      imgPath = (this.ViewHiResImage ? this.GalleryPage.GetOriginalUrl(mo) : this.GalleryPage.GetOptimizedUrl(mo));
		//      sbPreloadScript.Append(String.Format(CultureInfo.CurrentCulture, "var img{0} = new Image(); img{0}.src = \"{1}\";\n", moIterator, imgPath));
		//      moIterator++;
		//    }
		//    preloadScript = sbPreloadScript.ToString();
		//  }
		//  else
		//  {
		//    // Six or less objects - just append a string.
		//    foreach (IGalleryObject mo in imageObjects)
		//    {
		//      imgPath = (this.ViewHiResImage ? this.GalleryPage.GetOriginalUrl(mo) : this.GalleryPage.GetOptimizedUrl(mo));
		//      preloadScript += String.Format(CultureInfo.CurrentCulture, "var img{0} = new Image(); img{0}.src = \"{1}\";\n", moIterator, imgPath);
		//      moIterator++;
		//    }
		//  }

		//  return preloadScript;
		//}

		private void RedirectToDownloadZipPage()
		{
			Utils.Redirect(PageId.task_downloadobjects, "aid={0}", this.GalleryPage.GetAlbumId());
		}

		private void RedirectToRotatePage()
		{
			Utils.Redirect(PageId.task_rotateimage, "moid={0}", this.GalleryPage.GetMediaObjectId());
		}

		private void RedirectToMovePage()
		{
			Utils.Redirect(PageId.task_transferobject, "moid={0}&tt=move&skipstep1=true", this.GalleryPage.GetMediaObjectId());
		}

		private void RedirectToCopyPage()
		{
			Utils.Redirect(PageId.task_transferobject, "moid={0}&tt=copy&skipstep1=true", this.GalleryPage.GetMediaObjectId());
		}

		private void ConfigureMediaObjectTitle()
		{
			pnlMediaObjectTitle.Visible = this.GalleryPage.ShowMediaObjectTitle;
		}

		private void ConfigureToolbar()
		{
			const int toolbarLeftRightPadding = 30;
			int toolbarWidth = 0;
			const int toolbarItemWidth = 22;
			const int separatorBarWidth = 7;

			bool isGalleryWriteable = !this.GalleryPage.GallerySettings.MediaObjectPathIsReadOnly;

			if (!GalleryPage.ShowMediaObjectToolbar)
			{
				tbMediaObjectActions.Visible = false;
				_toolbarWidthEstimate = toolbarWidth;
				return;
			}

			// If we get here either showMediaObjectToolbar or enableSlideShow is true. The enableSlideShow variable overrides showMediaObjectToolbar,
			// so show the slide show control even when showMediaObjectToolbar is false.
			tbMediaObjectActions.ImagesBaseUrl = String.Concat(Utils.GalleryRoot, "/images/componentart/toolbar/");

			ToolBarItem tbItem;

			//<ComponentArt:ToolBarItem ID="tbiInfo" runat="server" ImageUrl="info.png" ItemType="ToggleCheck"
			//ToolTip="<%$ Resources:GalleryServerPro, UC_MediaObjectView_ToolBar_Info_Tooltip %>" />
			if (GalleryPage.ShowMetadataButton)
			{
				tbItem = new ToolBarItem();
				tbItem.ID = "tbiInfo";
				tbItem.ImageUrl = "info.png";
				tbItem.Text = Resources.GalleryServerPro.UC_MediaObjectView_ToolBar_Info_Text;
				tbItem.ItemType = ToolBarItemType.ToggleCheck;
				tbItem.ToolTip = Resources.GalleryServerPro.UC_MediaObjectView_ToolBar_Info_Tooltip;
				tbMediaObjectActions.Items.Add(tbItem);

				// Separator
				tbMediaObjectActions.Items.Add(GetToolBarSeparator());

				toolbarWidth += toolbarItemWidth + separatorBarWidth;
			}

			//<ComponentArt:ToolBarItem ID="tbiDownload" runat="server" AutoPostBackOnSelect="true" ImageUrl="download.png"
			//Text="Download" ToolTip="<%$ Resources:GalleryServerPro, UC_MediaObjectView_ToolBar_Download_Tooltip %>" />
			if (GalleryPage.ShowMediaObjectDownloadButton)
			{
				tbItem = new ToolBarItem();
				tbItem.ID = "tbiDownload";
				tbItem.ImageUrl = "download.png";
				tbItem.Visible = this.MediaObjectEntity.IsDownloadable;
				tbItem.Text = Resources.GalleryServerPro.UC_MediaObjectView_ToolBar_Download_Text;
				tbItem.AutoPostBackOnSelect = true;
				tbItem.ToolTip = Resources.GalleryServerPro.UC_MediaObjectView_ToolBar_Download_Tooltip;
				tbMediaObjectActions.Items.Add(tbItem);

				toolbarWidth += toolbarItemWidth;
			}

			//<ComponentArt:ToolBarItem ID="tbiDownloadZip" runat="server" AutoPostBackOnSelect="true" ImageUrl="download.png"
			//Text="Download" ToolTip="<%$ Resources:GalleryServerPro, UC_MediaObjectView_ToolBar_Download_Tooltip %>" />
			if (GalleryPage.ShowMediaObjectZipDownloadButton)
			{
				tbItem = new ToolBarItem();
				tbItem.ID = "tbiDownloadZip";
				tbItem.ImageUrl = "downloadzip.png";
				tbItem.Visible = true;
				//tbItem.Text = Resources.GalleryServerPro.UC_MediaObjectView_ToolBar_DownloadZip_Text;
				tbItem.AutoPostBackOnSelect = true;
				tbItem.ToolTip = Resources.GalleryServerPro.UC_MediaObjectView_ToolBar_DownloadZip_Tooltip;
				tbMediaObjectActions.Items.Add(tbItem);

				toolbarWidth += toolbarItemWidth;
			}

			//<ComponentArt:ToolBarItem ID="tbiViewHiRes" runat="server" ItemType="ToggleCheck" ImageUrl="hires.png"
			//Text="View hi-res" ToolTip="<%$ Resources:GalleryServerPro, UC_MediaObjectView_ToolBar_HiRes_Tooltip %>" />
			if (this.GalleryPage.UserCanViewHiResImage && GalleryPage.ShowHighResImageButton)
			{
				tbItem = new ToolBarItem();
				tbItem.ID = "tbiViewHiRes";
				tbItem.ImageUrl = "hires.png";
				tbItem.Visible = this.MediaObjectEntity.HiResAvailable;
				tbItem.Text = Resources.GalleryServerPro.UC_MediaObjectView_ToolBar_HiRes_Text;
				tbItem.ItemType = ToolBarItemType.ToggleCheck;
				tbItem.ToolTip = Resources.GalleryServerPro.UC_MediaObjectView_ToolBar_HiRes_Tooltip;
				tbItem.Checked = this.ViewHiResImage;
				tbMediaObjectActions.Items.Add(tbItem);

				toolbarWidth += toolbarItemWidth;
			}


			//<ComponentArt:ToolBarItem ID="tbiPermalink" runat="server" ItemType="ToggleCheck" ImageUrl="hyperlink.png"
			//Text="Permalink" ToolTip="<%$ Resources:GalleryServerPro, UC_MediaObjectView_ToolBar_Permalink_Tooltip %>" />
			if (GalleryPage.ShowPermalinkButton)
			{
				tbItem = new ToolBarItem();
				tbItem.ID = "tbiPermalink";
				tbItem.ImageUrl = "hyperlink.png";
				tbItem.Text = Resources.GalleryServerPro.UC_MediaObjectView_ToolBar_Permalink_Text;
				tbItem.ItemType = ToolBarItemType.ToggleCheck;
				tbItem.ToolTip = Resources.GalleryServerPro.UC_MediaObjectView_ToolBar_Permalink_Tooltip;
				tbMediaObjectActions.Items.Add(tbItem);

				toolbarWidth += toolbarItemWidth;
			}

			// Separator
			InsertToolBarSeparator();
			toolbarWidth += separatorBarWidth;

			//<ComponentArt:ToolBarItem ID="tbiSlideshow" runat="server" ImageUrl="play.png" Text="Slideshow"
			//ToolTip="<%$ Resources:GalleryServerPro, UC_MediaObjectView_ToolBar_Slideshow_Tooltip %>" />
			if (GalleryPage.ShowSlideShowButton)
			{
				tbItem = new ToolBarItem();
				tbItem.ID = "tbiSlideshow";
				tbItem.ImageUrl = "play.png";
				tbItem.Text = Resources.GalleryServerPro.UC_MediaObjectView_ToolBar_Slideshow_Text;
				tbItem.ToolTip = Resources.GalleryServerPro.UC_MediaObjectView_ToolBar_Slideshow_Tooltip;
				tbMediaObjectActions.Items.Add(tbItem);

				// Separator
				tbMediaObjectActions.Items.Add(GetToolBarSeparator());

				toolbarWidth += toolbarItemWidth + separatorBarWidth;
			}

			//<ComponentArt:ToolBarItem ID="tbiMove" runat="server" AutoPostBackOnSelect="true" ImageUrl="move.png"
			//Text="Transfer" ToolTip="<%$ Resources:GalleryServerPro, UC_MediaObjectView_ToolBar_Transfer_Tooltip %>" />
			if (isGalleryWriteable && this.GalleryPage.UserCanDeleteMediaObject && GalleryPage.ShowTransferMediaObjectButton)
			{
				tbItem = new ToolBarItem();
				tbItem.ID = "tbiMove";
				tbItem.ImageUrl = "move.png";
				tbItem.Text = Resources.GalleryServerPro.UC_MediaObjectView_ToolBar_Transfer_Text;
				tbItem.AutoPostBackOnSelect = true;
				tbItem.ToolTip = Resources.GalleryServerPro.UC_MediaObjectView_ToolBar_Transfer_Tooltip;
				tbMediaObjectActions.Items.Add(tbItem);

				toolbarWidth += toolbarItemWidth;
			}

			//<ComponentArt:ToolBarItem ID="tbiCopy" runat="server" AutoPostBackOnSelect="true" ImageUrl="copy.png"
			//Text="Copy" ToolTip="<%$ Resources:GalleryServerPro, UC_MediaObjectView_ToolBar_Copy_Tooltip %>" />
			if (isGalleryWriteable && CanUserCopyCurrentMediaObject() && GalleryPage.ShowCopyMediaObjectButton)
			{
				tbItem = new ToolBarItem();
				tbItem.ID = "tbiCopy";
				tbItem.ImageUrl = "copy.png";
				tbItem.Text = Resources.GalleryServerPro.UC_MediaObjectView_ToolBar_Copy_Text;
				tbItem.AutoPostBackOnSelect = true;
				tbItem.ToolTip = Resources.GalleryServerPro.UC_MediaObjectView_ToolBar_Copy_Tooltip;
				tbMediaObjectActions.Items.Add(tbItem);

				toolbarWidth += toolbarItemWidth;
			}

			//<ComponentArt:ToolBarItem ID="tbiRotate" runat="server" AutoPostBackOnSelect="true" ImageUrl="rotate.png"
			//Text="Rotate" ToolTip="<%$ Resources:GalleryServerPro, UC_MediaObjectView_ToolBar_Rotate_Tooltip %>" />
			if (isGalleryWriteable && this.GalleryPage.UserCanEditMediaObject && GalleryPage.ShowRotateMediaObjectButton)
			{
				tbItem = new ToolBarItem();
				tbItem.ID = "tbiRotate";
				tbItem.ImageUrl = "rotate.png";
				tbItem.Text = Resources.GalleryServerPro.UC_MediaObjectView_ToolBar_Rotate_Text;
				tbItem.AutoPostBackOnSelect = true;
				tbItem.ToolTip = Resources.GalleryServerPro.UC_MediaObjectView_ToolBar_Rotate_Tooltip;
				tbMediaObjectActions.Items.Add(tbItem);

				toolbarWidth += toolbarItemWidth;
			}

			//<ComponentArt:ToolBarItem ID="tbiDelete" runat="server" ImageUrl="delete.png" Text="Delete"
			//ToolTip="<%$ Resources:GalleryServerPro, UC_MediaObjectView_ToolBar_Delete_Tooltip %>" />
			if (isGalleryWriteable && this.GalleryPage.UserCanDeleteMediaObject && GalleryPage.ShowDeleteMediaObjectButton)
			{
				tbItem = new ToolBarItem();
				tbItem.ID = "tbiDelete";
				tbItem.ImageUrl = "delete.png";
				tbItem.Text = Resources.GalleryServerPro.UC_MediaObjectView_ToolBar_Delete_Text;
				tbItem.ToolTip = Resources.GalleryServerPro.UC_MediaObjectView_ToolBar_Delete_Tooltip;
				tbMediaObjectActions.Items.Add(tbItem);

				toolbarWidth += toolbarItemWidth;
			}

			if (tbMediaObjectActions.Items.Count == 0)
			{
				tbMediaObjectActions.Visible = false;
			}
			else if (tbMediaObjectActions.Items[tbMediaObjectActions.Items.Count - 1].ItemType == ToolBarItemType.Separator)
			{
				// The last item is a separator, which we don't want. Remove it.
				tbMediaObjectActions.Items.Remove(tbMediaObjectActions.Items[tbMediaObjectActions.Items.Count - 1]);
				toolbarWidth -= separatorBarWidth;
			}

			if (toolbarWidth > 0)
			{
				toolbarWidth += toolbarLeftRightPadding;
			}

			_toolbarWidthEstimate = toolbarWidth;
		}

		/// <summary>
		/// Insert a separator bar at the end of the toolbar items, but only if the last one isn't already 
		/// a separator bar.
		/// </summary>
		private void InsertToolBarSeparator()
		{
			if (tbMediaObjectActions.Items.Count > 0)
			{
				ToolBarItem tbi = tbMediaObjectActions.Items[tbMediaObjectActions.Items.Count - 1];
				if (tbi.ItemType != ToolBarItemType.Separator)
				{
					tbMediaObjectActions.Items.Add(GetToolBarSeparator());
				}
			}
		}

		/// <summary>
		/// Determine if the current user can copy the current media object into another album. This requires
		/// AllowAddMediaObject permission for at least one album that is NOT the current album. Permission is
		/// not granted if no user is logged in.
		/// </summary>
		/// <returns>Returns true if there is at least one album where the logged on user can copy the current 
		/// media object to; returns false if permission does not exist or current user is not logged in.</returns>
		private bool CanUserCopyCurrentMediaObject()
		{
			bool userCanCopy = false;
			if (Utils.IsAuthenticated)
			{
				IGallery gallery = Factory.LoadGallery(GalleryPage.GalleryId);

				IGalleryServerRoleCollection roles = this.GalleryPage.GetGalleryServerRolesForUser();
				foreach (IGalleryServerRole role in roles)
				{
					if (role.Galleries.Contains(gallery) && role.AllowAddMediaObject)
					{
						if ((role.AllAlbumIds.Count > 0)
								|| ((role.AllAlbumIds.Count == 1) && (!role.AllAlbumIds.Contains(this.GalleryPage.GetAlbumId()))))
						{
							userCanCopy = true;
							break;
						}
					}
				}
			}
			return userCanCopy;
		}

		private ToolBarItem GetToolBarSeparator()
		{
			//<ComponentArt:ToolBarItem ItemType="Separator" ImageUrl="break.gif" ImageHeight="16" ImageWidth="2" />
			if (this._toolbarItemSeparator == null)
			{
				this._toolbarItemSeparator = new ToolBarItem();
				this._toolbarItemSeparator.ItemType = ToolBarItemType.Separator;
				this._toolbarItemSeparator.ImageUrl = "break.gif";
				this._toolbarItemSeparator.ImageHeight = new Unit(16);
				this._toolbarItemSeparator.ImageWidth = new Unit(2);
			}

			return this._toolbarItemSeparator;
		}

		private void ConfigureSecurityRelatedControls()
		{
			if (this.GalleryPage.UserCanEditAlbum)
			{
				AddEditAlbumInfoDialog();
			}

			if (this.GalleryPage.UserCanEditMediaObject)
			{
				AddEditMediaObjectCaptionDialog();
			}
		}

		private void AddEditAlbumInfoDialog()
		{
			Dialog dgEditAlbum = new Dialog();

			dgEditAlbum.ContentTemplate = Page.LoadTemplate(Utils.GetUrl("/controls/albumedittemplate.ascx"));

			#region Set Dialog Properties

			dgEditAlbum.ID = "dgEditAlbum";
			dgEditAlbum.AnimationDirectionElement = "currentAlbumLink";
			dgEditAlbum.CloseTransition = TransitionType.Fade;
			dgEditAlbum.ShowTransition = TransitionType.Fade;
			dgEditAlbum.AnimationSlide = SlideType.Linear;
			dgEditAlbum.AnimationType = DialogAnimationType.Outline;
			dgEditAlbum.AnimationPath = SlidePath.Direct;
			dgEditAlbum.AnimationDuration = 400;
			dgEditAlbum.TransitionDuration = 400;
			dgEditAlbum.Icon = "pencil.gif";
			dgEditAlbum.Alignment = DialogAlignType.MiddleCentre;
			dgEditAlbum.AllowResize = true;
			dgEditAlbum.ContentCssClass = "dg0ContentCss";
			dgEditAlbum.HeaderCssClass = "dg0HeaderCss";
			dgEditAlbum.CssClass = "gsp_dg0DialogCss gsp_ns";
			dgEditAlbum.FooterCssClass = "dg0FooterCss";
			dgEditAlbum.ZIndex = 900;

			dgEditAlbum.HeaderClientTemplateId = "dgEditAlbumHeaderTemplate";

			#endregion

			#region Header Template

			ClientTemplate ctHeader = new ClientTemplate();
			ctHeader.ID = "dgEditAlbumHeaderTemplate";

			ctHeader.Text = String.Format(CultureInfo.InvariantCulture, @"
		<div onmousedown='dgEditAlbum.StartDrag(event);'>
			<img id='dg0DialogCloseImage' onclick=""closeEditDialog();"" src='{0}/images/componentart/dialog/close.gif' /><img
				id='dg0DialogIconImage' src='{0}/images/componentart/dialog/pencil.gif' style='width:27px;height:30px;' />
				## Parent.Title ##
		</div>", Utils.GalleryRoot);

			dgEditAlbum.ClientTemplates.Add(ctHeader);

			#endregion

			phDialogContainer.Controls.Add(dgEditAlbum);
		}

		private void AddEditMediaObjectCaptionDialog()
		{
			// If the user has permission to edit the media object, configure the caption so that when it is double-clicked,
			// a dialog window appears that lets the user edit and save the caption. Note that this code is dependent on the
			// saveCaption javascript function, which is added in the RegisterJavascript method.
			if (this.GalleryPage.ShowMediaObjectTitle && this.GalleryPage.UserCanEditMediaObject)
			{
				pnlMediaObjectTitle.ToolTip = Resources.GalleryServerPro.Site_Editable_Content_Tooltip;
				pnlMediaObjectTitle.CssClass = "editableContentOff";
				pnlMediaObjectTitle.Attributes.Add("onmouseover", "this.className='editableContentOn';");
				pnlMediaObjectTitle.Attributes.Add("onmouseout", "this.className='editableContentOff';");
				pnlMediaObjectTitle.Attributes.Add("ondblclick", "editCaption()");

				Dialog dgEditCaption = new Dialog();
				dgEditCaption.ID = "dgEditCaption";
				dgEditCaption.AlignmentElement = pnlMediaObjectTitle.ClientID;
				dgEditCaption.CssClass = "gsp_dg3DialogCss gsp_ns gsp_rounded10";
				dgEditCaption.ContentCssClass = "dg3ContentCss";
				dgEditCaption.ContentClientTemplateId = "dgEditCaptionContentTemplate";

				ClientTemplate ct = new ClientTemplate();
				ct.ID = "dgEditCaptionContentTemplate";

				ct.Text = String.Format(CultureInfo.InvariantCulture, @"
<textarea id='taCaption' rows='4' cols='75' class='mediaObjectTitleTextArea'>{0}</textarea>
<div class='gsp_okCancelContainer'>
	<input type='button' value='{1}' onclick=""saveCaption($get('taCaption').value)"" />&nbsp;<input type='button' value='{2}' onclick='dgEditCaption.close()' />
</div>",
					this.GalleryPage.GetMediaObject().Title,
					Resources.GalleryServerPro.Default_Task_Ok_Button_Text,
					Resources.GalleryServerPro.Default_Task_Cancel_Button_Text
					);

				dgEditCaption.ClientTemplates.Add(ct);

				phDialogContainer.Controls.Add(dgEditCaption);
			}
		}

		#endregion
	}
}