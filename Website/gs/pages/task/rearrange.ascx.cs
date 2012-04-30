using System;
using System.Globalization;
using System.Web.UI.WebControls;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.Web.Controller;

namespace GalleryServerPro.Web.Pages.Task
{
	/// <summary>
	/// A page-like user control that handles the Rearrange objects task.
	/// </summary>
	public partial class rearrange : Pages.TaskPage
	{
		#region Private Fields

		private int _mediaObjectIndex;
		private string[] _snapDockIds;
		private string[] _galleryObjectIds;
		private string _snapDockIdsString;

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets a comma-delimited string of the IDs of the div tags used as the snap docking containers. This property is assigned
		/// to the DockingContainers property of the Snap control as each one is data bound.
		/// </summary>
		public string SnapDockIds
		{
			get
			{
				if (String.IsNullOrEmpty(this._snapDockIdsString))
				{
					if ((this._snapDockIds == null) || (this._snapDockIds.Length == 0))
						throw new GalleryServerPro.ErrorHandler.CustomExceptions.WebException("The string array _snapDockIds must be assigned before accessing the SnapDockIds property. Instead, this property was null or empty.");

					this._snapDockIdsString = String.Join(",", this._snapDockIds);
				}
				return this._snapDockIdsString;
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
			this.CheckUserSecurity(SecurityActions.EditAlbum);

			if (!IsPostBack)
			{
				ConfigureControls();
			}
		}

		#endregion

		#region Protected Methods

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
			//An event from the control has bubbled up.  If it's the Ok button, then run the
			//code to save the data to the database; otherwise ignore.
			Button btn = source as Button;
			if ((btn != null) && (((btn.ID == "btnOkTop") || (btn.ID == "btnOkBottom"))))
			{
				btnOkClicked();

				this.RedirectToAlbumViewPage("msg={0}", ((int)Message.ObjectsSuccessfullyRearranged).ToString(CultureInfo.InvariantCulture));
			}

			return true;
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
				return "thmb2 album";
			else
				return "thmb2";
		}

		/// <summary>
		/// Return a string representing the title of the media object. It is truncated and purged of HTML tags
		/// if necessary.
		/// </summary>
		/// <param name="title">The title of the media object as stored in the data store.</param>
		/// <param name="galleryObjectType">The type of the object to which the title belongs.</param>
		/// <returns>Returns a string representing the title of the media object. It is truncated and purged of HTML tags
		/// if necessary.</returns>
		protected string GetMediaObjectText(string title, Type galleryObjectType)
		{
			if (String.IsNullOrEmpty(title))
				return String.Empty;

			// If this is an album, return an empty string. Otherwise, we need to truncate the title while trying to 
			// preserve embedded HTML. Do this by checking the length of the title after it has been purged of HTML
			// codes. If the purged length is less than our maximum allowed, return the entire string with HTML.
			// If not, truncate the 
			if (galleryObjectType == typeof(Album))
				return String.Empty;

			int maxLength = GallerySettings.MaxMediaObjectThumbnailTitleDisplayLength;
			string truncatedText = Utils.TruncateTextForWeb(title, maxLength);
			string titleText;
			if (truncatedText.Length != title.Length)
				titleText = String.Format(CultureInfo.CurrentCulture, "<p>{0}...</p>", truncatedText);
			else
				titleText = String.Format(CultureInfo.CurrentCulture, "<p>{0}</p>", truncatedText);

			return titleText;
		}

		/// <summary>
		/// Gets a hidden input tag that identifies the specified <paramref name="galleryObject" />.
		/// </summary>
		/// <param name="galleryObject">The gallery object.</param>
		/// <returns>Returns a string that represents a hidden input tag.</returns>
		protected string GetHiddenInputIdTag(IGalleryObject galleryObject)
		{
			// Get a hidden input tag like this: <input type='hidden' id='hdId0' value='a203' />
			// This field is written to each Snap control (i.e. thumbnail) and is used in the retrieveOrder javascript function
			// to determine the order of the thumbnails. retrieveOrder is invoked from the client click event of the two OK buttons.
			return String.Concat("<input type='hidden' id='hdId", this._mediaObjectIndex.ToString(CultureInfo.InvariantCulture), "' value='", GetGalleryObjectId(galleryObject), "' />");
		}

		/// <summary>
		/// Gets the ID to be used to identify the snap docking element.
		/// </summary>
		/// <returns>Returns the ID to be used to identify the snap docking element.</returns>
		protected string GetSnapDockId()
		{
			this._snapDockIds[this._mediaObjectIndex] = String.Concat("sd", this._mediaObjectIndex.ToString(CultureInfo.InvariantCulture));
			string snapDockId = this._snapDockIds[this._mediaObjectIndex];
			this._mediaObjectIndex++;
			return snapDockId;
		}

		//protected string GetThumbnailImageTag(IGalleryObject galleryObject)
		//{
		//  //<img src="<%# GetThumbnailUrl((IGalleryObject)((RepeaterItem)Container.Parent).DataItem) %>"
		//  //alt="<%# GetHovertip((IGalleryObject) ((RepeaterItem)Container.Parent).DataItem) %>"
		//  //title="<%# GetHovertip((IGalleryObject)((RepeaterItem)Container.Parent).DataItem) %>"
		//  //style="width: <%# DataBinder.Eval(((RepeaterItem)Container.Parent).DataItem, "Thumbnail.Width").ToString() %>px;
		//  //height: <%# DataBinder.Eval(((RepeaterItem)Container.Parent).DataItem, "Thumbnail.Height").ToString() %>px;" />
		//  string hoverTip = GetHovertip(galleryObject);

		//  string thumbImgTag = String.Format(@"<img src=""{0}"" alt=""{1}"" title=""{1}"" style=""width:{2}px;height:{3}px;"" />",
		//    GetThumbnailUrl(galleryObject), // 0
		//    GetHovertip(galleryObject), // 1
		//    galleryObject.Thumbnail.Width, // 2
		//    galleryObject.Thumbnail.Height
		//    );

		//  return thumbImgTag;
		//}

		//protected string GetSnapId(object container)
		//{
		//  //"sp0"
		//  //return String.Concat("sp", this._mediaObjectIndex.ToString());
		//  return ((Snap)container).ClientID;
		//  //return "ctl00_ctl00_ctl00_c1_c2_c3_rprSnap_ctl00_sp";
		//}

		/// <summary>
		/// Gets the snap docking container ID.
		/// </summary>
		/// <returns>Returns the snap docking container ID.</returns>
		protected string GetSnapDockingContainerId()
		{
			//"sd0"
			return String.Concat("sd", this._mediaObjectIndex.ToString(CultureInfo.InvariantCulture));
		}

		/// <summary>
		/// Handles the ItemDataBound event of the rprSnap control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
		protected void rprSnap_ItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			this._mediaObjectIndex++;
		}

		#endregion

		#region Private Methods

		private void ConfigureControls()
		{
			this.TaskHeaderText = Resources.GalleryServerPro.Task_Rearrange_Objects_Header_Text;
			this.TaskBodyText = Resources.GalleryServerPro.Task_Rearrange_Objects_Body_Text;
			this.OkButtonBottom.OnClientClick = "retrieveOrder();";
			this.OkButtonTop.OnClientClick = "retrieveOrder();";

			this.PageTitle = Resources.GalleryServerPro.Task_Rearrange_Objects_Page_Title;

			IGalleryObjectCollection albumChildren = this.GetAlbum().GetChildGalleryObjects(true);
			
			if (albumChildren.Count > 0)
			{
				SetThumbnailCssStyle(albumChildren, 0, 0, new string[] { "thmb2", "snapDock" });

				// Bind the docking containers (really, just div tags) that will hold each snap control.
				this._snapDockIds = new string[albumChildren.Count];
				rprDock.DataSource = albumChildren;
				rprDock.DataBind(); // _snapDockIds is updated within GetSnapDockId() during data binding (it is invoked from aspx markup)

				// Reset media object index to 0 and bind the snaps to the page. Each snap is assigned to one of the snap docks that
				// were bound in the rprDock.DataBind method.
				this._galleryObjectIds = new string[albumChildren.Count];
				this._mediaObjectIndex = 0;
				rprSnap.DataSource = albumChildren;
				rprSnap.DataBind();

				this.Page.ClientScript.RegisterArrayDeclaration("_snapDockIds", String.Concat("'", String.Join("','", this._snapDockIds), "'"));
			}
			else
			{
				this.RedirectToAlbumViewPage("msg={0}", ((int)Message.CannotRearrangeNoObjectsExistInAlbum).ToString(CultureInfo.InvariantCulture));
			}
		}

		private string GetGalleryObjectId(IGalleryObject galleryObject)
		{
			// Prepend an 'a' (for album) or 'm' (for media object) to the ID to indicate whether it is
			// an album ID or media object ID.
			string id;
			if (galleryObject is Album)
				id = "a" + galleryObject.Id.ToString(CultureInfo.InvariantCulture);
			else
				id = "m" + galleryObject.Id.ToString(CultureInfo.InvariantCulture);

			// Add to member variable. Later we'll send these to the client in javascript and a hidden tag 
			// to help manage the rearranging.
			this._galleryObjectIds[this._mediaObjectIndex] = id;

			return id;
		}

		private void btnOkClicked()
		{
			object formFieldGalleryObjectIds = Request.Form["goIds"];
			if ((formFieldGalleryObjectIds == null) || (String.IsNullOrEmpty(formFieldGalleryObjectIds.ToString())))
			{
				// The hidden field will be empty when no changes have been made. Just return.
				return;
			}

			string strGoIds = formFieldGalleryObjectIds.ToString();
			string[] goIds = strGoIds.Split(new char[] { ',' });

			// User wants to persist the reordering changes to the data store. As the user has been dragging and dropping objects
			// in their new locations, server-side code has been keeping the CurrentSequences property synchronized with the order
			// of the objects in the user's browser. Now we want to loop through those objects and update the Sequence property
			// of each one according to it's position within the list.
			IGalleryObjectCollection galleryObjects = this.GetAlbum(true).GetChildGalleryObjects(true);

			int newSequence = 0;
			try
			{
				HelperFunctions.BeginTransaction();
				foreach (string galleryObjectIdentifier in goIds)
				{
					// Parse the string into its 2 parts: (a) The first character is either "a" (for album) or "m" (for media object);
					// (b) The rest of the string is the ID for the album of media object. Ex: "a132" is an album with ID=132.
					GalleryObjectType galleryObjectType = (galleryObjectIdentifier.Substring(0, 1) == "a" ? GalleryObjectType.Album : GalleryObjectType.MediaObject);
					int galleryObjectId = Convert.ToInt32(galleryObjectIdentifier.Substring(1), CultureInfo.InvariantCulture);

					IGalleryObject matchingGalleryObject = galleryObjects.FindById(galleryObjectId, galleryObjectType);

					if ((matchingGalleryObject != null) && (matchingGalleryObject.Sequence != newSequence))
					{
						matchingGalleryObject.Sequence = newSequence;
						GalleryObjectController.SaveGalleryObject(matchingGalleryObject);
					}
					newSequence++;
				}
				HelperFunctions.CommitTransaction();
			}
			catch
			{
				HelperFunctions.RollbackTransaction();
				throw;
			}

			HelperFunctions.PurgeCache();
		}

		#endregion
	}
}