using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web.UI.WebControls;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.ErrorHandler.CustomExceptions;

namespace GalleryServerPro.Web.Pages.Task
{
	/// <summary>
	/// A page-like user control that handles the Download objects task.
	/// </summary>
	public partial class downloadobjects : Pages.TaskPage
	{
		#region Private Fields

		private IGalleryObjectCollection _galleryObjects;

		private const DisplayObjectType DEFAULT_IMAGE_SIZE = DisplayObjectType.Optimized;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the gallery objects that are candidates for deleting.
		/// </summary>
		/// <value>The gallery objects that are candidates for deleting.</value>
		public IGalleryObjectCollection GalleryObjects
		{
			get
			{
				if (this._galleryObjects == null)
				{
					GalleryObjectType galleryObjectType;

					if (GallerySettings.EnableGalleryObjectZipDownload && GallerySettings.EnableAlbumZipDownload)
					{
						galleryObjectType = GalleryObjectType.All;
					}
					else
					{
						galleryObjectType = GalleryObjectType.MediaObject;
					}

					this._galleryObjects = this.GetAlbum().GetChildGalleryObjects(galleryObjectType, true, IsAnonymousUser);
				}

				return this._galleryObjects;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the current user can download media objects in a ZIP file.
		/// </summary>
		/// <value>
		/// 	Returns <c>true</c> when the current user can download media objects in a ZIP file; otherwise, <c>false</c>.
		/// </value>
		private bool DownloadMediaObjectsEnabled
		{
			get
			{
				// Return true when the gallery setting is true or if the gallery setting has been overridden by the Gallery Control Setting.
				return GallerySettings.EnableGalleryObjectZipDownload || ShowMediaObjectZipDownloadButton;
			}
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// Handles the Init event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Init(object sender, EventArgs e)
		{
			this.TaskHeaderPlaceHolder = phTaskHeader;
			this.TaskFooterPlaceHolder = phTaskFooter;
		}

		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			// We do not need to verify user can view objects because the GalleryPage.GetAlbum method will return ONLY
			// those objects user can view.
			//this.CheckUserSecurity(SecurityActions.ViewAlbumOrMediaObject);

			if (!DownloadMediaObjectsEnabled)
			{
				RedirectToAlbumViewPage();
			}

			if (!IsPostBack)
			{
				ConfigureControlsFirstTime();
			}

			ConfigureControlsEveryPageLoad();
		}

		/// <summary>
		/// Determines whether the event for the server control is passed up the page's UI server control hierarchy.
		/// </summary>
		/// <param name="source">The source of the event.</param>
		/// <param name="args">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
		/// <returns>
		/// true if the event has been canceled; otherwise, false. The default is false.
		/// </returns>
		protected override bool OnBubbleEvent(object source, EventArgs args)
		{
			//An event from a control has bubbled up.  If it's the Ok button, then run the
			//code to synchronize; otherwise ignore.
			Button btn = source as Button;
			if ((btn != null) && (((btn.ID == "btnOkTop") || (btn.ID == "btnOkBottom"))))
			{
				btnOkClicked();
			}

			return true;
		}

		private void btnOkClicked()
		{
			// User clicked the download button. Gather selected items, build a ZIP file, and send to user.
			List<int> albumIds, mediaObjectIds;
			RetrieveUserSelections(out albumIds, out mediaObjectIds);

			if ((albumIds.Count == 0) && (mediaObjectIds.Count == 0))
			{
				// No objects were selected. Inform user and exit function.
				string msg = String.Format(CultureInfo.CurrentCulture, "<p class='gsp_msgwarning'><span class='gsp_bold'>{0} </span>{1}</p>", Resources.GalleryServerPro.Task_No_Objects_Selected_Hdr, Resources.GalleryServerPro.Task_No_Objects_Selected_Dtl);
				phMsg.Controls.Clear();
				phMsg.Controls.Add(new System.Web.UI.LiteralControl(msg));

				return;
			}

			BuildAndSendZipFile(albumIds, mediaObjectIds);
		}

		private void BuildAndSendZipFile(List<int> albumIds, List<int> mediaObjectIds)
		{
			IMimeType mimeType = MimeType.LoadMimeType("dummy.zip");
			string zipFilename = Utils.UrlEncode("Media Files".Replace(" ", "_"));

			using (ZipUtility zip = new ZipUtility(Utils.UserName, GetGalleryServerRolesForUser()))
			{
				int bufferSize = AppSetting.Instance.MediaObjectDownloadBufferSize;
				byte[] buffer = new byte[bufferSize];

				Stream stream = null;
				try
				{
					// Create an in-memory ZIP file.
					stream = zip.CreateZipStream(this.GetAlbumId(), albumIds, mediaObjectIds, GetImageSize());

					// Send to user.
					Response.AddHeader("Content-Disposition", "attachment; filename=" + zipFilename + ".zip");

					Response.Clear();
					Response.ContentType = (mimeType != null ? mimeType.FullType : "application/octet-stream");
					Response.Buffer = false;

					stream.Position = 0;
					int byteCount;
					while ((byteCount = stream.Read(buffer, 0, buffer.Length)) > 0)
					{
						if (Response.IsClientConnected)
						{
							Response.OutputStream.Write(buffer, 0, byteCount);
							Response.Flush();
						}
						else
						{
							return;
						}
					}
				}
				finally
				{
					if (stream != null)
						stream.Close();

					Response.End();
				}
			}
		}

		private DisplayObjectType GetImageSize()
		{
			DisplayObjectType displayType = DEFAULT_IMAGE_SIZE;

			try
			{
				displayType = (DisplayObjectType)Convert.ToInt32(this.ddlImageSize.SelectedValue, CultureInfo.InvariantCulture);
			}
			catch (FormatException) { } // Suppress any parse errors
			catch (OverflowException) { } // Suppress any parse errors
			catch (ArgumentOutOfRangeException) { } // Suppress any parse errors

			if (!DisplayObjectTypeEnumHelper.IsValidDisplayObjectType(displayType))
			{
				displayType = DEFAULT_IMAGE_SIZE;
			}

			if ((displayType == DisplayObjectType.Original) && (!this.IsUserAuthorized(SecurityActions.ViewOriginalImage)))
			{
				displayType = DEFAULT_IMAGE_SIZE;
			}

			return displayType;
		}

		/// <summary>
		/// Gets the CSS class to apply to the thumbnail object.
		/// </summary>
		/// <param name="galleryObjectType">The gallery object.</param>
		/// <returns>Returns a CSS class.</returns>
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
		/// Return an HTML formatted string representing the title of the gallery object. It is truncated and purged of HTML tags
		/// if necessary.
		/// </summary>
		/// <param name="title">The title of the gallery object as stored in the data store.</param>
		/// <param name="galleryObjectType">The type of the object to which the title belongs.</param>
		/// <returns>Returns a string representing the title of the media object. It is truncated and purged of HTML tags
		/// if necessary.</returns>
		protected string GetGalleryObjectText(string title, Type galleryObjectType)
		{
			if (String.IsNullOrEmpty(title))
				return String.Empty;

			// If this is an album, return an empty string. Otherwise, return the title, truncated and purged of HTML
			// tags if necessary. If the title is truncated, add an ellipses to the text.
			//<asp:Label ID="lblAlbumPrefix" runat="server" CssClass="gsp_bold" Text="<%$ Resources:GalleryServerPro, UC_ThumbnailView_Album_Title_Prefix_Text %>" />&nbsp;<%# GetGalleryObjectText(Eval("Title").ToString(), Container.DataItem.GetType())%>

			int maxLength = GallerySettings.MaxMediaObjectThumbnailTitleDisplayLength;
			string titlePrefix = String.Empty;

			if (galleryObjectType == typeof(Album))
			{
				// Album titles need a prefix, so assign that now.
				titlePrefix = String.Format(CultureInfo.CurrentCulture, "<span class='gsp_bold'>{0} </span>", Resources.GalleryServerPro.UC_ThumbnailView_Album_Title_Prefix_Text);

				// Override the previous max length with the value that is appropriate for albums.
				maxLength = GallerySettings.MaxAlbumThumbnailTitleDisplayLength;
			}

			string truncatedText = Utils.TruncateTextForWeb(title, maxLength);

			if (truncatedText.Length != title.Length)
				return String.Concat(titlePrefix, truncatedText, "...");
			else
				return String.Concat(titlePrefix, truncatedText);
		}

		/// <summary>
		/// Gets a value that uniquely identifies the specified <paramref name="galleryObject" /> (ex: "a25", "m223").
		/// </summary>
		/// <param name="galleryObject">The gallery object.</param>
		/// <returns>Returns an ID.</returns>
		protected static string GetId(IGalleryObject galleryObject)
		{
			if (galleryObject == null)
				throw new ArgumentNullException("galleryObject");

			// Prepend an 'a' (for album) or 'm' (for media object) to the ID to indicate whether it is
			// an album ID or media object ID.
			if (galleryObject is Album)
				return "a" + galleryObject.Id.ToString(CultureInfo.InvariantCulture);
			else
				return "m" + galleryObject.Id.ToString(CultureInfo.InvariantCulture);
		}

		#endregion

		#region Private Methods

		private void ConfigureControlsFirstTime()
		{
			this.TaskHeaderText = Resources.GalleryServerPro.Task_Download_Objects_Header_Text;
			this.TaskBodyText = Resources.GalleryServerPro.Task_Download_Objects_Body_Text;
			this.OkButtonText = Resources.GalleryServerPro.Task_Download_Objects_Ok_Button_Text;
			this.OkButtonToolTip = Resources.GalleryServerPro.Task_Download_Objects_Ok_Button_Tooltip;

			this.PageTitle = Resources.GalleryServerPro.Task_Download_Objects_Page_Title;

			if (GalleryObjects.Count > 0)
			{
				rptr.DataSource = GalleryObjects;
				rptr.DataBind();
			}
			else
			{
				this.RedirectToAlbumViewPage("msg={0}", ((int)Message.CannotDownloadObjectsNoObjectsExistInAlbum).ToString(CultureInfo.InvariantCulture));
			}

			ConfigureImageSizeDropDown();
		}

		private void ConfigureImageSizeDropDown()
		{
			// Add options to the image size dropdown box.
			this.ddlImageSize.Items.Add(new ListItem(Resources.GalleryServerPro.Task_Download_Objects_Select_Image_Size_Thumbnail_Option, ((int)DisplayObjectType.Thumbnail).ToString(CultureInfo.InvariantCulture)));
			this.ddlImageSize.Items.Add(new ListItem(Resources.GalleryServerPro.Task_Download_Objects_Select_Image_Size_Compressed_Option, ((int)DisplayObjectType.Optimized).ToString(CultureInfo.InvariantCulture)));

			if (this.IsUserAuthorized(SecurityActions.ViewOriginalImage))
			{
				this.ddlImageSize.Items.Add(new ListItem(Resources.GalleryServerPro.Task_Download_Objects_Select_Image_Size_Original_Option, ((int)DisplayObjectType.Original).ToString(CultureInfo.InvariantCulture)));
			}

			// Pre-select the compressed image size. Subtract one from the enum value because it starts at 1 and the index is zero-based.
			this.ddlImageSize.SelectedIndex = (int)DEFAULT_IMAGE_SIZE - 1;
		}

		private void ConfigureControlsEveryPageLoad()
		{
			SetThumbnailCssStyle(GalleryObjects);
		}

		private void RetrieveUserSelections(out List<int> albumIds, out List<int> mediaObjectIds)
		{
			// Iterate through all the checkboxes, saving checked ones to an array. The gallery object IDs are stored 
			// in a hidden input tag. Albums have an 'a' prefix; images have a 'm' prefix (e.g. "a322", "m999")
			albumIds = new List<int>();
			mediaObjectIds = new List<int>();

			// Loop through each item in the repeater control. If an item is checked, extract the ID.
			foreach (RepeaterItem rptrItem in rptr.Items)
			{
				// Each item will have one checkbox named chk.
				CheckBox chkbx = rptrItem.FindControl("chk") as CheckBox;

				if ((chkbx == null) || (chkbx.Visible == false))
					throw new WebException("Cannot find a checkbox named chk or it has been made invisible");

				if (chkbx.Checked)
				{
					// Checkbox is checked. Find ID and save to appropriate List.
					HiddenField gc = (HiddenField)rptrItem.FindControl("hdn");

					if ((gc == null) || (gc.Value.Length < 2))
						continue;

					int id = Convert.ToInt32(gc.Value.Substring(1), CultureInfo.InvariantCulture);
					char idType = Convert.ToChar(gc.Value.Substring(0, 1), CultureInfo.InvariantCulture); // 'a' or 'm'

					if (idType == 'm')
					{
						mediaObjectIds.Add(id);
					}
					else if (idType == 'a')
					{
						albumIds.Add(id);
					}
				}
			}
		}

		#endregion
	}
}