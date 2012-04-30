using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using ComponentArt.Web.UI;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.ErrorHandler.CustomExceptions;
using GalleryServerPro.Web.Controller;

namespace GalleryServerPro.Web.Pages.Task
{
	/// <summary>
	/// A page-like user control that handles the Transfer objects task.
	/// </summary>
	public partial class transferobject : Pages.TaskPage
	{
		#region Enum declarations

		/// <summary>
		/// Indicates the type of transfer.
		/// </summary>
		private enum TransferType { None, Move, Copy }

		/// <summary>
		/// Indicates the current state of the transfer.
		/// </summary>
		private enum TransferObjectState
		{
			None,
			AlbumMoveStep2,
			AlbumCopyStep2,
			MediaObjectMoveStep2,
			MediaObjectCopyStep2,
			ObjectsMoveStep1,
			ObjectsMoveStep2,
			ObjectsCopyStep1,
			ObjectsCopyStep2,
			ReadyToTransfer
		}

		#endregion

		#region Private Fields

		private TransferType _transferType = TransferType.None;
		private TransferObjectState _transferState = TransferObjectState.None;
		private IAlbum _destAlbum;

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
			if (GallerySettings.MediaObjectPathIsReadOnly)
				RedirectToAlbumViewPage("msg={0}", ((int)Message.CannotEditGalleryIsReadOnly).ToString(CultureInfo.InvariantCulture));

			ConfigureControls();
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
			//if (!Page.IsValid)
			//  return true;

			//An event from the control has bubbled up.  If it's the Ok button, then run the
			//code to process the command; otherwise ignore.
			if (IsOkButtonClicked(source) && (ViewState["ShowTreeView"] != null) && (ViewState["ShowTreeView"].ToString() == "1"))
			{
				// Do not show the treeview on subsequent postbacks
				ViewState["ShowTreeView"] = "0";
				btnOkClicked("step1");
			}
			else if (IsOkButtonClicked(source) && (ViewState["ShowTreeView"] != null) && (ViewState["ShowTreeView"].ToString() == "0"))
				btnOkClicked("step2");

			return true;
		}

		#endregion

		#region Properties

		private TransferType TransType
		{
			get
			{
				if (this._transferType == TransferType.None)
				{
					string qsValue = Utils.GetQueryStringParameterString("tt");
					if (qsValue == "move")
						this._transferType = TransferType.Move;
					else if (qsValue == "copy")
						this._transferType = TransferType.Copy;
					//else
					//  throw new GalleryServerPro.ErrorHandler.CustomExceptions.UnexpectedQueryStringException();
				}

				return this._transferType;
			}
		}

		private TransferObjectState TransObjectState
		{
			get
			{
				if (this._transferState == TransferObjectState.None)
				{
					this._transferState = GetTransferObjectState();
				}

				return this._transferState;
			}
		}

		private IAlbum DestinationAlbum
		{
			get
			{
				if (this._destAlbum == null)
				{
					TreeViewNode selectedNode = tvUC.SelectedNode;
					this._destAlbum = AlbumController.LoadAlbumInstance(Convert.ToInt32(selectedNode.Value, CultureInfo.InvariantCulture), false);
				}

				return this._destAlbum;
			}
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Gets the media object title, truncating it if necessary.
		/// </summary>
		/// <param name="title">The title.</param>
		/// <returns>Returns the media object title.</returns>
		protected string GetTitle(string title)
		{
			if (String.IsNullOrEmpty(title))
				return String.Empty;

			// Truncate the Title if it is too long
			int maxLength = GallerySettings.MaxMediaObjectThumbnailTitleDisplayLength;
			string truncatedText = Utils.TruncateTextForWeb(title, maxLength);
			string titleText;
			if (truncatedText.Length != title.Length)
				titleText = String.Format(CultureInfo.CurrentCulture, "{0}...", truncatedText);
			else
				titleText = truncatedText;

			return titleText;
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
		/// Gets a value that uniquely identifies the specified <paramref name="galleryObject" /> (ex: "a25", "m223").
		/// </summary>
		/// <param name="galleryObject">The gallery object.</param>
		/// <returns>Returns an ID.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryObject" /> is null.</exception>
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

		private void ConfigureControls()
		{
			this.PageTitle = Resources.GalleryServerPro.Task_Transfer_Objects_Page_Title;

			switch (this.TransObjectState)
			{
				#region AlbumCopyStep2
				case TransferObjectState.AlbumCopyStep2:
					{
						this.TaskHeaderText = Resources.GalleryServerPro.Task_Transfer_Objects_Copy_Album_Header_Text;
						this.TaskBodyText = Resources.GalleryServerPro.Task_Transfer_Objects_Copy_Album_Body_Text;
						this.OkButtonText = Resources.GalleryServerPro.Task_Transfer_Objects_Copy_Album_Ok_Button_Text;
						this.OkButtonToolTip = Resources.GalleryServerPro.Task_Transfer_Objects_Copy_Album_Ok_Button_Tooltip;

						// Assign the album ID for the object the user selected to the variable used by the moveObjects() method.
						// We use a string array even though we only have a single value so that we are compatible with the
						// view state variable "ids" that is also assigned by the StoreUserSelections() methods, where
						// the user is allowed to select more than one object to copy/move. We prepend an 'a' to the ID to 
						// indicate this is an album ID rather than a media object ID.
						string[] ids = { "a" + this.GetAlbumId().ToString(CultureInfo.InvariantCulture) };

						// Persist the album ID to the view state, so it is available in subsequent postbacks
						ViewState["ids"] = ids;

						// Show the treeview control, so the user can select the destination album
						ShowTreeview();

						// Don't show the treeview on subsequent postbacks
						ViewState["ShowTreeView"] = "0";
						break;
					}
				#endregion

				#region AlbumMoveStep2
				case TransferObjectState.AlbumMoveStep2:
					{
						this.TaskHeaderText = Resources.GalleryServerPro.Task_Transfer_Objects_Move_Album_Header_Text;
						this.TaskBodyText = Resources.GalleryServerPro.Task_Transfer_Objects_Move_Album_Body_Text;
						this.OkButtonText = Resources.GalleryServerPro.Task_Transfer_Objects_Move_Album_Ok_Button_Text;
						this.OkButtonToolTip = Resources.GalleryServerPro.Task_Transfer_Objects_Move_Album_Ok_Button_Tooltip;

						// Assign the album ID for the object the user selected to the variable used by the moveObjects() method.
						// We use a string array even though we only have a single value so that we are compatible with the
						// view state variable "ids" that is also assigned by the StoreUserSelections() methods, where
						// the user is allowed to select more than one object to copy/move. We prepend an 'a' to the ID to 
						// indicate this is an album ID rather than a media object ID.
						string[] ids = { "a" + this.GetAlbumId().ToString(CultureInfo.InvariantCulture) };

						// Persist the album ID to the view state, so it is available in subsequent postbacks
						ViewState["ids"] = ids;

						// Show the treeview control, so the user can select the destination album
						ShowTreeview();

						// Don't show the treeview on subsequent postbacks
						ViewState["ShowTreeView"] = "0";
						break;
					}
				#endregion

				#region MediaObjectCopyStep2
				case TransferObjectState.MediaObjectCopyStep2:
					{
						this.TaskHeaderText = Resources.GalleryServerPro.Task_Transfer_Objects_Copy_Media_Object_Header_Text;
						this.TaskBodyText = Resources.GalleryServerPro.Task_Transfer_Objects_Copy_Media_Object_Body_Text;
						this.OkButtonText = Resources.GalleryServerPro.Task_Transfer_Objects_Copy_Media_Object_Ok_Button_Text;
						this.OkButtonToolTip = Resources.GalleryServerPro.Task_Transfer_Objects_Copy_Media_Object_Ok_Button_Tooltip;

						// Assign the media object ID for the object the user selected to the variable used by the moveObjects() method.
						// We use a string array even though we only have a single value so that we are compatible with the
						// view state variable "ids" that is also assigned by the StoreUserSelections() methods, where
						// the user is allowed to select more than one object to copy/move. We prepend an 'm' to the ID to 
						// indicate this is a media object ID rather than an album ID.
						string[] ids = { "m" + this.GetMediaObjectId().ToString(CultureInfo.InvariantCulture) };

						// Persist the album ID to the view state, so it is available in subsequent postbacks
						ViewState["ids"] = ids;

						// Show the treeview control, so the user can select the destination album
						ShowTreeview();

						// Don't show the treeview on subsequent postbacks
						ViewState["ShowTreeView"] = "0";
						break;
					}
				#endregion

				#region MediaObjectMoveStep2
				case TransferObjectState.MediaObjectMoveStep2:
					{
						this.TaskHeaderText = Resources.GalleryServerPro.Task_Transfer_Objects_Move_Media_Object_Header_Text;
						this.TaskBodyText = Resources.GalleryServerPro.Task_Transfer_Objects_Move_Media_Object_Body_Text;
						this.OkButtonText = Resources.GalleryServerPro.Task_Transfer_Objects_Move_Media_Object_Ok_Button_Text;
						this.OkButtonToolTip = Resources.GalleryServerPro.Task_Transfer_Objects_Move_Media_Object_Ok_Button_Tooltip;

						// Assign the media object ID for the object the user selected to the variable used by the moveObjects() method.
						// We use a string array even though we only have a single value so that we are compatible with the
						// view state variable "ids" that is also assigned by the StoreUserSelections() methods, where
						// the user is allowed to select more than one object to copy/move. We prepend an 'm' to the ID to 
						// indicate this is a media object ID rather than an album ID.
						string[] ids = { "m" + this.GetMediaObjectId().ToString(CultureInfo.InvariantCulture) };

						// Persist the album ID to the view state, so it is available in subsequent postbacks
						ViewState["ids"] = ids;

						// Show the treeview control, so the user can select the destination album
						ShowTreeview();

						// Don't show the treeview on subsequent postbacks
						ViewState["ShowTreeView"] = "0";
						break;
					}
				#endregion

				#region ObjectsCopyStep1
				case TransferObjectState.ObjectsCopyStep1:
					{
						this.TaskHeaderText = Resources.GalleryServerPro.Task_Transfer_Objects_Copy_Media_Objects_Step1_Header_Text;
						this.TaskBodyText = Resources.GalleryServerPro.Task_Transfer_Objects_Copy_Media_Objects_Step1_Body_Text;
						this.OkButtonText = Resources.GalleryServerPro.Task_Transfer_Objects_Copy_Media_Objects_Step1_Ok_Button_Text;
						this.OkButtonToolTip = Resources.GalleryServerPro.Task_Transfer_Objects_Copy_Media_Objects_Step1_Ok_Button_Tooltip;

						// Get the children of the current album the user will be choosing from.
						IGalleryObjectCollection albumChildren = this.GetAlbum().GetChildGalleryObjects(true);

						if (albumChildren.Count == 0)
							this.RedirectToAlbumViewPage("msg={0}", ((int)Message.CannotCopyNoObjectsExistInAlbum).ToString(CultureInfo.InvariantCulture));

						SetThumbnailCssStyle(albumChildren);

						rptr.DataSource = albumChildren;
						rptr.DataBind();

						AddCheckAllInputBox();

						ViewState["ShowTreeView"] = "1";

						break;
					}
				#endregion

				#region ObjectsMoveStep1
				case TransferObjectState.ObjectsMoveStep1:
					{
						this.TaskHeaderText = Resources.GalleryServerPro.Task_Transfer_Objects_Move_Media_Objects_Step1_Header_Text;
						this.TaskBodyText = Resources.GalleryServerPro.Task_Transfer_Objects_Move_Media_Objects_Step1_Body_Text;
						this.OkButtonText = Resources.GalleryServerPro.Task_Transfer_Objects_Move_Media_Objects_Step1_Ok_Button_Text;
						this.OkButtonToolTip = Resources.GalleryServerPro.Task_Transfer_Objects_Move_Media_Objects_Step1_Ok_Button_Tooltip;

						// Get the children of the current album the user will be choosing from.
						IGalleryObjectCollection albumChildren = this.GetAlbum().GetChildGalleryObjects(true);

						if (albumChildren.Count == 0)
							this.RedirectToAlbumViewPage("msg={0}", ((int)Message.CannotMoveNoObjectsExistInAlbum).ToString(CultureInfo.InvariantCulture));

						SetThumbnailCssStyle(albumChildren);

						rptr.DataSource = albumChildren;
						rptr.DataBind();

						AddCheckAllInputBox();

						ViewState["ShowTreeView"] = "1";

						break;
					}
				#endregion

				#region ObjectsCopyStep2
				case TransferObjectState.ObjectsCopyStep2:
					{
						this.TaskHeaderText = Resources.GalleryServerPro.Task_Transfer_Objects_Copy_Media_Objects_Step2_Header_Text;
						this.TaskBodyText = Resources.GalleryServerPro.Task_Transfer_Objects_Copy_Media_Objects_Step2_Body_Text;
						this.OkButtonText = Resources.GalleryServerPro.Task_Transfer_Objects_Copy_Media_Objects_Step2_Ok_Button_Text;
						this.OkButtonToolTip = Resources.GalleryServerPro.Task_Transfer_Objects_Copy_Media_Objects_Step2_Ok_Button_Tooltip;

						break;
					}
				#endregion

				#region ObjectsMoveStep2
				case TransferObjectState.ObjectsMoveStep2:
					{
						this.TaskHeaderText = Resources.GalleryServerPro.Task_Transfer_Objects_Move_Media_Objects_Step2_Header_Text;
						this.TaskBodyText = Resources.GalleryServerPro.Task_Transfer_Objects_Move_Media_Objects_Step2_Body_Text;
						this.OkButtonText = Resources.GalleryServerPro.Task_Transfer_Objects_Move_Media_Objects_Step2_Ok_Button_Text;
						this.OkButtonToolTip = Resources.GalleryServerPro.Task_Transfer_Objects_Move_Media_Objects_Step2_Ok_Button_Tooltip;

						break;
					}
				#endregion

				#region ReadyToTransfer
				case TransferObjectState.ReadyToTransfer: break;
				#endregion
			}

		}

		private void AddCheckAllInputBox()
		{
			string checkAllHtml = String.Format(CultureInfo.InvariantCulture, @"<p><input type='checkbox' id='chkSelectAll' onclick='javascript:ToggleSelectAll(this);' /><label for='chkSelectAll'>{0}</label></p>",
				Resources.GalleryServerPro.Task_Transfer_Objects_Check_Uncheck_All_Label_Text);
			phCheckAllContainer.Controls.Add(new System.Web.UI.LiteralControl(checkAllHtml));
		}

		private void ShowTreeview()
		{
			rptr.Visible = false;

			tvUC.Visible = true;

			// Find out if the objects we are transferring consist of only media objects, only albums, or both.
			// We use this knowledge to set the RequiredSecurityPermission property on the treeview user control
			// so that only albums where the user has permission are available for selection.
			bool hasAlbums = false;
			bool hasMediaObjects = false;
			SecurityActions securityActions = 0;
			foreach (string id in (string[])ViewState["ids"])
			{
				if (id.StartsWith("a", StringComparison.Ordinal))
				{
					securityActions = (((int)securityActions == 0) ? SecurityActions.AddChildAlbum : securityActions | SecurityActions.AddChildAlbum);
					hasAlbums = true;
				}
				if (id.StartsWith("m", StringComparison.Ordinal))
				{
					securityActions = (((int)securityActions == 0) ? SecurityActions.AddMediaObject : securityActions | SecurityActions.AddMediaObject);
					hasMediaObjects = true;
				}
				if (hasAlbums && hasMediaObjects)
					break;
			}

			tvUC.RequiredSecurityPermissions = securityActions;

			if (UserCanAdministerSite || UserCanAdministerGallery)
			{
				// Show all galleries the current user can administer. This allows them to move/copy objects between galleries.
				tvUC.RootAlbumPrefix = String.Concat(Resources.GalleryServerPro.Site_Gallery_Text, " '{GalleryDescription}': ");
				tvUC.Galleries = UserController.GetGalleriesCurrentUserCanAdminister();
			}

			IAlbum albumToSelect = this.GetAlbum();
			if (!IsUserAuthorized(SecurityActions.AddChildAlbum, albumToSelect))
			{
				albumToSelect = AlbumController.GetHighestLevelAlbumWithAddPermission(hasAlbums, hasMediaObjects, GalleryId);
			}

			if (albumToSelect == null)
				tvUC.BindTreeView();
			else
				tvUC.BindTreeView(albumToSelect);
		}

		private void btnOkClicked(string step)
		{
			// User clicked either 'Choose these objects' (step 1) or 'Move objects' (step 2).
			// Carry out the requested action.
			if (step == "step1")
			{
				string[] ids = RetrieveUserSelections();

				if (ids.Length > 0)
				{
					// Persist the gallery object IDs to the view state, so they are available later
					ViewState["ids"] = ids;

					ShowTreeview();
				}
				else
				{
					HandleUserNotSelectingAnyObjects();
				}
			}
			else if (step == "step2")
			{
				try
				{
					TransferObjects();

					HelperFunctions.PurgeCache();

					RedirectAfterSuccessfulTransfer();
				}
				catch (GallerySecurityException)
				{
					// User does not have permission to carry out the operation.
					string msg = String.Format(CultureInfo.CurrentCulture, "<p class='gsp_msgwarning'><span class='gsp_bold'>{0} </span>{1}</p>", Resources.GalleryServerPro.Task_Transfer_Objects_Cannot_Transfer_No_Permission_Msg_Hdr, Resources.GalleryServerPro.Task_Transfer_Objects_Cannot_Transfer_No_Permission_Msg_Dtl);
					phMsg.Controls.Clear();
					phMsg.Controls.Add(new System.Web.UI.LiteralControl(msg));
				}
				catch (CannotTransferAlbumToNestedDirectoryException)
				{
					// User tried to move or copy an album to one of its own subdirectories. This cannot be done.
					string msg = String.Format(CultureInfo.CurrentCulture, "<p class='gsp_msgwarning'><span class='gsp_bold'>{0} </span>{1}</p>", Resources.GalleryServerPro.Task_Transfer_Objects_Cannot_Transfer_To_Nested_Album_Msg_Hdr, Resources.GalleryServerPro.Task_Transfer_Objects_Cannot_Transfer_To_Nested_Album_Msg_Dtl);
					phMsg.Controls.Clear();
					phMsg.Controls.Add(new System.Web.UI.LiteralControl(msg));
				}
			}

		}

		/// <summary>
		/// Redirects the after a successful transfer. The user is sent to the destination album if it is in the current gallery; otherwise
		/// the user is redirected to the current album page.
		/// </summary>
		private void RedirectAfterSuccessfulTransfer()
		{
			Message msg = (this.TransType == TransferType.Copy ? Message.ObjectsSuccessfullyCopied : Message.ObjectsSuccessfullyMoved);
			
			if (DestinationAlbum.GalleryId == GalleryId)
			{
				Utils.Redirect(PageId.album, "aid={0}&msg={1}", this.DestinationAlbum.Id.ToString(CultureInfo.InvariantCulture), ((int)msg).ToString(CultureInfo.InvariantCulture));
			}
			else
			{
				Utils.Redirect(PageId.album, "&msg={0}", ((int)msg).ToString(CultureInfo.InvariantCulture));
			}
		}

		/// <summary>
		/// Handle the situation where the user didn't select any objects on step 1. Show message to user, move transfer state back to
		/// previous step, and re-render the page. This function is only relevant when the page requests the user make a selection
		/// and no selection was made. It is not intended for and in fact throws an exception in situations where the user made a 
		/// selection on a previous page and is then transferred to this page.
		/// </summary>
		private void HandleUserNotSelectingAnyObjects()
		{
			string msg = String.Format(CultureInfo.CurrentCulture, "<p class='gsp_msgwarning'><span class='gsp_bold'>{0} </span>{1}</p>", Resources.GalleryServerPro.Task_No_Objects_Selected_Hdr, Resources.GalleryServerPro.Task_No_Objects_Selected_Dtl);
			phMsg.Controls.Clear();
			phMsg.Controls.Add(new System.Web.UI.LiteralControl(msg));

			if (this.TransObjectState == TransferObjectState.ObjectsCopyStep2)
			{
				this._transferState = TransferObjectState.ObjectsCopyStep1;
			}
			else if (this.TransObjectState == TransferObjectState.ObjectsMoveStep2)
			{
				this._transferState = TransferObjectState.ObjectsMoveStep1;
			}
			else
				throw new WebException("The function HandleUserNotSelectingAnyObjects should never be invoked in cases where the user made a selection on a previous page and is then transferred to this page.");

			ConfigureControls();
		}

		/// <summary>
		/// Move or copy the objects. An exception is thrown if the user does not have the required permission or is 
		/// trying to transfer an object to itself.
		/// </summary>
		/// <exception cref="GalleryServerPro.ErrorHandler.CustomExceptions.GallerySecurityException">Thrown when the logged on 
		/// user does not belong to a role that authorizes the moving or copying.</exception>
		/// <exception cref="GalleryServerPro.ErrorHandler.CustomExceptions.CannotTransferAlbumToNestedDirectoryException">
		/// Thrown when the user tries to move or copy an album to one of its children albums.</exception>
		private void TransferObjects()
		{
			#region Get list of objects to move or copy

			string[] galleryObjectIds = (string[])ViewState["ids"];

			// Convert the string array of IDs to integers. Also assign whether each is an album or media object.
			// (Determined by the first character of each ids string: a=album; m=media object)
			IGalleryObjectCollection objectsToMoveOrCopy = new GalleryObjectCollection();
			for (int i = 0; i < galleryObjectIds.Length; i++)
			{
				int id = Convert.ToInt32(galleryObjectIds[i].Substring(1), CultureInfo.InvariantCulture);
				char idType = Convert.ToChar(galleryObjectIds[i].Substring(0, 1), CultureInfo.InvariantCulture);

				if (idType == 'a')
				{
					try
					{
						IAlbum album = AlbumController.LoadAlbumInstance(id, false, true);

						objectsToMoveOrCopy.Add(album);
					}
					catch (InvalidAlbumException) { /* Album may have been deleted by someone else, so just skip it. */ }
				}
				else if (idType == 'm')
				{
					// Grab a reference to the media object through the base page's album instead of using Factory.LoadMediaObjectInstance().
					// This causes the base page's album object to have an accurate state of the child objects so that when we assign the
					// thumbnail object later in this page life cycle, it works correctly.
					IGalleryObject mediaObject = Factory.LoadMediaObjectInstance(id, true);
					//IGalleryObject mediaObject = this.GetAlbum().GetChildGalleryObjects(GalleryObjectType.MediaObject).FindById(id, GalleryObjectType.MediaObject);

					if (mediaObject != null) /* Media object may have been deleted by someone else, so just skip it. */
					{
						objectsToMoveOrCopy.Add(mediaObject);
					}
				}
				else
					throw new WebException("Invalid object identifier in method transferObjects(). Expected: 'a' or 'm'. Found: " + idType);
			}

			#endregion

			#region Validate (throws exception if it doesn't validate)

			ValidateObjectsCanBeMovedOrCopied(objectsToMoveOrCopy);

			#endregion

			try
			{
				HelperFunctions.BeginTransaction();

				#region Move or copy each object

				foreach (IGalleryObject galleryObject in objectsToMoveOrCopy)
				{
					IAlbum album = galleryObject as IAlbum;
					if (album == null)
					{
						if (this.TransType == TransferType.Move)
							MoveMediaObject(galleryObject);
						else
							CopyMediaObject(galleryObject);
					}
					else
					{
						if (this.TransType == TransferType.Move)
							MoveAlbum(album);
						else
							CopyAlbum(album);
					}
				}

				#endregion

				HelperFunctions.CommitTransaction();
			}
			catch
			{
				HelperFunctions.RollbackTransaction();
				throw;
			}
		}

		/// <summary>
		/// Throw exception if the specified albums and/or media objects cannot be moved or copied for any reason, 
		/// such as lack of user permission or trying to move/copy objects to itself.
		/// </summary>
		/// <param name="objectsToMoveOrCopy">The albums or media objects to move or copy.</param>
		/// <exception cref="GalleryServerPro.ErrorHandler.CustomExceptions.GallerySecurityException">Thrown when the logged on 
		/// user does not belong to a role that authorizes the moving or copying.</exception>
		/// <exception cref="GalleryServerPro.ErrorHandler.CustomExceptions.CannotTransferAlbumToNestedDirectoryException">
		/// Thrown when the user tries to move or copy an album to one of its children albums.</exception>
		private void ValidateObjectsCanBeMovedOrCopied(IGalleryObjectCollection objectsToMoveOrCopy)
		{
			bool movingOrCopyingAtLeastOneAlbum = false;
			bool movingOrCopyingAtLeastOneMediaObject = false;

			#region Validate the albums and media objects we are moving or copying

			bool securityCheckCompleteForAlbum = false;
			bool securityCheckCompleteForMediaObject = false;

			foreach (IGalleryObject galleryObject in objectsToMoveOrCopy)
			{
				if (galleryObject is Album)
				{
					ValidateAlbumCanBeMovedOrCopied((IAlbum)galleryObject);

					if (!securityCheckCompleteForAlbum) // Only need to check albums once, since all albums belong to same parent.
					{
						ValidateSecurityForAlbumOrMediaObject(galleryObject, SecurityActions.DeleteAlbum);
						securityCheckCompleteForAlbum = true;
					}

					movingOrCopyingAtLeastOneAlbum = true; // used below
				}
				else
				{
					if (!securityCheckCompleteForMediaObject) // Only need to check media objects once, since they all belong to same parent.
					{
						ValidateSecurityForAlbumOrMediaObject(galleryObject.Parent, SecurityActions.DeleteMediaObject);
						securityCheckCompleteForMediaObject = true;
					}

					movingOrCopyingAtLeastOneMediaObject = true; // used below
				}
			}

			#endregion

			#region Validate user has permission to add objects to destination album

			if (movingOrCopyingAtLeastOneAlbum && (!IsUserAuthorized(SecurityActions.AddChildAlbum, this.DestinationAlbum.Id, this.DestinationAlbum.GalleryId)))
			{
				throw new GallerySecurityException(String.Format(CultureInfo.CurrentCulture, "User '{0}' does not have permission '{1}' for album ID {2}.", Utils.UserName, SecurityActions.AddChildAlbum, this.DestinationAlbum.Id));
			}

			if (movingOrCopyingAtLeastOneMediaObject && (!IsUserAuthorized(SecurityActions.AddMediaObject, this.DestinationAlbum.Id, this.DestinationAlbum.GalleryId)))
			{
				throw new GallerySecurityException(String.Format(CultureInfo.CurrentCulture, "User '{0}' does not have permission '{1}' for album ID {2}.", Utils.UserName, SecurityActions.AddMediaObject, this.DestinationAlbum.Id));
			}

			#endregion
		}

		/// <summary>
		/// Throw exception if user does not have permission to move the specified gallery object out of the current album.
		/// Moving an album or media object means we are essentially deleting it from the source album, so make sure user has 
		/// the appropriate delete permission for the current album. Does not validate user has permission to add objects to 
		/// destination album. Assumes each gallery object is contained in the current album as retrieved by GspPage.GetAlbum().
		/// No validation is performed if we are copying since no special permissions are needed for copying (except a check 
		/// on the destination album, which we do elsewhere).
		/// </summary>
		/// <param name="galleryObjectToMoveOrCopy">The album or media object to move or copy.</param>
		/// <param name="securityActions">The security permission to validate.</param>
		/// <exception cref="GalleryServerPro.ErrorHandler.CustomExceptions.GallerySecurityException">Thrown when the logged on 
		/// user does not belong to a role that authorizes the specified security action.</exception>
		private void ValidateSecurityForAlbumOrMediaObject(IGalleryObject galleryObjectToMoveOrCopy, SecurityActions securityActions)
		{
			if (this.TransType == TransferType.Move)
			{
				if (!IsUserAuthorized(securityActions, galleryObjectToMoveOrCopy.Id, this.GalleryId))
				{
					throw new GallerySecurityException(String.Format(CultureInfo.CurrentCulture, "User '{0}' does not have permission '{1}' for album ID {2}.", Utils.UserName, securityActions, galleryObjectToMoveOrCopy.Id));
				}
			}
		}

		/// <summary>
		/// Throw exception if user is trying to move or copy an album to one of its children albums.
		/// </summary>
		/// <param name="albumToMoveOrCopy">The album to move or copy.</param>
		/// <exception cref="GalleryServerPro.ErrorHandler.CustomExceptions.CannotTransferAlbumToNestedDirectoryException">
		/// Thrown when the user tries to move or copy an album to one of its children albums.</exception>
		private void ValidateAlbumCanBeMovedOrCopied(IAlbum albumToMoveOrCopy)
		{
			IAlbum albumParent = this.DestinationAlbum;

			while (!albumParent.IsRootAlbum)
			{
				if (albumParent.Id == albumToMoveOrCopy.Id)
				{
					throw new CannotTransferAlbumToNestedDirectoryException();
				}
				albumParent = (IAlbum)albumParent.Parent;
			}
		}

		private void MoveAlbum(IAlbum albumToMove)
		{
			try
			{
				GalleryObjectController.MoveGalleryObject(albumToMove, this.DestinationAlbum);
			}
			catch (System.IO.IOException ex)
			{
				throw new CannotMoveDirectoryException(String.Format(CultureInfo.CurrentCulture, "Error while trying to move album {0}.", albumToMove.Id), ex);
			}
		}

		// Note: Use the following version of UpdateRoleSecurityForMovedAlbum to enforce the rule that moved albums
		// must retain all role permissions it had in its original location, even inherited ones. This would go in 
		// the Album class and be invoked from the MoveTo method.
		///// <summary>
		///// Validate that the moved album retains its original role permissions. If the role is inherited from
		///// a parent album, then explicitly assign that role to the moved album. If the role is explicitly assigned to
		///// the moved album, then check to see if the destination album already includes that role. If it does, there is 
		///// no need to apply it twice, so remove the explicitly assigned role from the moved album and let it simply
		///// inherit the role.
		///// </summary>
		///// <param name="movedAlbum">The album that has just been moved to a new destination album.</param>
		//private void UpdateRoleSecurityForMovedAlbum(IAlbum movedAlbum)
		//{
		//  foreach (IGalleryServerRole role in this.GetGalleryServerRoles())
		//  {
		//    if (role.AllAlbumIds.Contains(movedAlbum.Id))
		//    {
		//      // This role applies to this object.
		//      if (role.RootAlbumIds.Contains(movedAlbum.Id))
		//      {
		//        // The album is directly specified in this role, but if any of this album's new parents are explicitly
		//        // specified, then it is not necessary to specify it at this level. Iterate through all the album's new 
		//        // parent albums to see if this is the case.
		//        IAlbum albumToCheck = (IAlbum)movedAlbum.Parent;
		//        while (true)
		//        {
		//          if (role.RootAlbumIds.Contains(albumToCheck.Id))
		//          {
		//            role.RootAlbumIds.Remove(movedAlbum.Id);
		//            role.Save();
		//            break;
		//          }
		//          albumToCheck = (IAlbum)albumToCheck.Parent;

		//          if (albumToCheck.IsRootAlbum)
		//            break;
		//        }
		//      }
		//      else
		//      {
		//        // The album inherits its role from a parent. If the new parent is not included in this role,
		//        // then add this object so that the role permissions carry over to the new location.
		//        if (!role.AllAlbumIds.Contains(this.DestinationAlbum.Id))
		//        {
		//          role.RootAlbumIds.Add(movedAlbum.Id);
		//          role.Save();
		//        }
		//      }
		//    }
		//  }
		//}

		private void CopyAlbum(IAlbum albumToCopy)
		{
			GalleryObjectController.CopyGalleryObject(albumToCopy, this.DestinationAlbum);
		}

		// Note: Use the following version of UpdateRoleSecurityForCopiedAlbum to enforce the rule that copied albums
		// must retain all role permissions it had in its original location, even inherited ones. This would go in 
		// the Album class and be invoked from the CopyTo method.
		///// <summary>
		///// Validate that the copied album includes the same role permissions as the source album. If a role is
		///// already applied at the destination album, then there is no need to specify it twice, so just let the
		///// copied album inherit from its parent. This method does not remove any role permissions from the copied
		///// album; it only ensures that the copied album includes the role permissions as the original album. It 
		///// may end up with additional permissions that are inherited through the copied album's parent.
		///// </summary>
		///// <param name="sourceAlbumId">The ID of the album the copy was made from.</param>
		///// <param name="copiedAlbum">The album that was just copied.</param>
		//private void UpdateRoleSecurityForCopiedAlbum(int sourceAlbumId, IGalleryObject copiedAlbum)
		//{
		//  foreach (IGalleryServerRole role in this.GetGalleryServerRoles())
		//  {
		//    if (role.AllAlbumIds.Contains(sourceAlbumId))
		//    {
		//      // This role applies to the original album. Apply it to the copied album also, unless it will already inherit
		//      // this role from the destination album.
		//      if (!role.AllAlbumIds.Contains(this.DestinationAlbum.Id))
		//      {
		//        role.RootAlbumIds.Add(copiedAlbum.Id);
		//        role.Save();
		//      }
		//    }
		//  }
		//}

		private void MoveMediaObject(IGalleryObject mediaObjectToMove)
		{
			GalleryObjectController.MoveGalleryObject(mediaObjectToMove, this.DestinationAlbum);
		}

		private void CopyMediaObject(IGalleryObject mediaObjectToCopy)
		{
			GalleryObjectController.CopyGalleryObject(mediaObjectToCopy, this.DestinationAlbum);
		}

		private string[] RetrieveUserSelections()
		{
			// Iterate through all the checkboxes, saving checked ones to an array.
			// The gallery object IDs are stored in a hidden input tag.
			CheckBox chkbx;
			HtmlInputHidden gc;
			List<string> ids = new List<string>();

			// Loop through each item in the repeater control. If an item is checked, extract the ID.
			foreach (RepeaterItem rptrItem in rptr.Items)
			{
				chkbx = (CheckBox)rptrItem.Controls[1]; // The <INPUT TYPE="CHECKBOX"> tag
				if (chkbx.Checked)
				{
					// Checkbox is checked. Save media object ID to array.
					gc = (HtmlInputHidden)rptrItem.Controls[3]; // The hidden <input> tag
					ids.Add(gc.Value);
				}
			}

			// Convert the int array to an array of strings of exactly the right length.
			string[] idArray = new string[ids.Count];
			ids.CopyTo(idArray);

			return idArray;
		}

		private static bool IsOkButtonClicked(object source)
		{
			Button btn = source as Button;
			return ((btn != null) && (((btn.ID == "btnOkTop") || (btn.ID == "btnOkBottom"))));
		}

		/// <summary>
		/// Determine the current state of this page.
		/// </summary>
		/// <returns>Returns the current state of this page.</returns>
		private TransferObjectState GetTransferObjectState()
		{
			// Is the album ID specified on the query string? Notice that we check for the presence of the "aid"
			// parameter instead of checking to see if the value is greater than 0. This is because we might get to
			// this page from a page showing a virtual album, in which case the aid parameter will be passed as
			// int.MinValue.
			bool isAlbumIdSpecified = Utils.IsQueryStringParameterPresent("aid");

			int mediaObjectId = Utils.GetQueryStringParameterInt32("moid");
			bool? skipStep1 = Utils.GetQueryStringParameterBoolean("skipstep1");

			if (!skipStep1.HasValue)
				skipStep1 = false;

			TransferObjectState transferState = TransferObjectState.None;

			if ((!Page.IsPostBack) && (isAlbumIdSpecified) && (skipStep1.Value == false))
			{
				// Not postback, must allow user to select objects to copy/move (step 1)
				if (this.TransType == TransferType.Copy)
					transferState = TransferObjectState.ObjectsCopyStep1;
				else
					transferState = TransferObjectState.ObjectsMoveStep1;
			}
			else if ((!Page.IsPostBack) && (isAlbumIdSpecified) && (this.TransType == TransferType.Move) && (skipStep1.Value == true))
			{
				// Not postback, user selected 'Move album' link. We'll display the treeview to allow the user to select the destination album. 
				transferState = TransferObjectState.AlbumMoveStep2;
			}
			else if ((!Page.IsPostBack) && (isAlbumIdSpecified) && (this.TransType == TransferType.Copy) && (skipStep1.Value == true))
			{
				// Not postback, user selected 'Copy album' link. We'll display the treeview to allow the user to select the destination album. 
				transferState = TransferObjectState.AlbumCopyStep2;
			}
			else if ((!Page.IsPostBack) && (mediaObjectId > 0))
			{
				// Not postback, user already selected a media object to copy/move from a different web page, 
				// We'll display the treeview to allow the user to select the destination album. 
				if (this.TransType == TransferType.Copy)
					transferState = TransferObjectState.MediaObjectCopyStep2;
				else
					transferState = TransferObjectState.MediaObjectMoveStep2;
			}
			else if ((Page.IsPostBack) && (ViewState["ShowTreeView"] != null) && (ViewState["ShowTreeView"].ToString() == "1"))
			{
				// This is a postback where we want to show the treeview control, thus allowing the user to select
				// the destination album to copy/move the objects to.
				if (this.TransType == TransferType.Copy)
					transferState = TransferObjectState.ObjectsCopyStep2;
				else
					transferState = TransferObjectState.ObjectsMoveStep2;
			}
			else if ((Page.IsPostBack) && (ViewState["ShowTreeView"] != null) && (ViewState["ShowTreeView"].ToString() == "0"))
			{
				// This is a postback where we do not want to show the treeview control, which can only mean the user has 
				// finished selecting the destination album and is ready to transfer the objects.
				transferState = TransferObjectState.ReadyToTransfer;
			}

			return transferState;
		}

		#endregion
	}
}