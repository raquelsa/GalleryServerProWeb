using System;
using System.Globalization;
using System.Web.UI.WebControls;
using ComponentArt.Web.UI;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.Web.Controller;

namespace GalleryServerPro.Web.Pages.Task
{
	/// <summary>
	/// A page-like user control that handles the Create album task.
	/// </summary>
	public partial class createalbum : Pages.TaskPage
	{
		#region Private Fields

		private int _msgId;
		private int _currentAlbumId = int.MinValue;

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

			if (!IsPostBack)
			{
				ConfigureControls();
			}
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
			//An event from the control has bubbled up.  If it's the Ok button, then run the
			//code to save the data to the database; otherwise ignore.
			Button btn = source as Button;
			if ((btn != null) && (((btn.ID == "btnOkTop") || (btn.ID == "btnOkBottom"))))
			{
				if (Page.IsValid)
				{
					int newAlbumID = btnOkClicked();

					redirectToNewAlbumPage(newAlbumID); // Redirect to the page for the newly created album.
				}
			}

			return true;
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets the album ID for the current album.
		/// </summary>
		public int CurrentAlbumId
		{
			get
			{
				if (this._currentAlbumId == int.MinValue)
					this._currentAlbumId = this.GetAlbumId();

				return this._currentAlbumId;
			}
		}

		#endregion

		#region Private Methods

		private void ConfigureControls()
		{
			this.TaskHeaderText = Resources.GalleryServerPro.Task_Create_Album_Header_Text;
			this.TaskBodyText = Resources.GalleryServerPro.Task_Create_Album_Body_Text;
			this.OkButtonText = Resources.GalleryServerPro.Task_Create_Album_Ok_Button_Text;
			this.OkButtonToolTip = Resources.GalleryServerPro.Task_Create_Album_Ok_Button_Tooltip;

			this.PageTitle = Resources.GalleryServerPro.Task_Create_Album_Page_Title;

			txtTitle.MaxLength = DataConstants.AlbumTitleLength;

			lblMaxTitleLengthInfo.Text = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.Task_Create_Album_Title_Max_Length_Text, DataConstants.AlbumTitleLength.ToString(CultureInfo.InvariantCulture));

			tvUC.RequiredSecurityPermissions = SecurityActions.AddChildAlbum;

			if (this.GetAlbum().IsPrivate)
			{
				chkIsPrivate.Checked = true;
				chkIsPrivate.Enabled = false;
				lblPrivateAlbumIsInherited.Text = Resources.GalleryServerPro.Task_Create_Album_Is_Private_Disabled_Text;
			}

			IAlbum albumToSelect = this.GetAlbum();
			if (!IsUserAuthorized(SecurityActions.AddChildAlbum, albumToSelect))
			{
				albumToSelect = AlbumController.GetHighestLevelAlbumWithCreatePermission(GalleryId);
			}

			if (albumToSelect == null)
				tvUC.BindTreeView();
			else
				tvUC.BindTreeView(albumToSelect);

			this.Page.Form.DefaultFocus = txtTitle.ClientID;
		}

		private int btnOkClicked()
		{
			//User clicked 'Create album'. Create the new album and return the new album ID.
			TreeViewNode selectedNode = tvUC.SelectedNode;
			int parentAlbumId = Int32.Parse(selectedNode.Value, CultureInfo.InvariantCulture);
			IAlbum parentAlbum = AlbumController.LoadAlbumInstance(parentAlbumId, false);

			this.CheckUserSecurity(SecurityActions.AddChildAlbum, parentAlbum);
			
			int newAlbumId;

			if (parentAlbumId > 0)
			{
				using (IAlbum newAlbum = Factory.CreateEmptyAlbumInstance(parentAlbum.GalleryId))
				{
					newAlbum.Title = GetAlbumTitle();
					//newAlbum.ThumbnailMediaObjectId = 0; // not needed
					newAlbum.Parent = parentAlbum;
					newAlbum.IsPrivate = (parentAlbum.IsPrivate ? true : chkIsPrivate.Checked);
					GalleryObjectController.SaveGalleryObject(newAlbum);
					newAlbumId = newAlbum.Id;
				}
				HelperFunctions.PurgeCache();
			}
			else
				throw new GalleryServerPro.ErrorHandler.CustomExceptions.InvalidAlbumException(parentAlbumId);

			return newAlbumId;
		}

		private string GetAlbumTitle()
		{
			// Get the title the user entered for this album. If the length exceeds our maximum, set the messageId
			// variable so that the receiving page is notified of the situation.
			string title = Utils.CleanHtmlTags(txtTitle.Text.Trim(), GalleryId);

			const int maxLength = DataConstants.AlbumTitleLength;

			if ((maxLength > 0) && (title.Length > maxLength))
			{
				title = title.Substring(0, maxLength).Trim();
				_msgId = (int)GalleryServerPro.Web.Message.AlbumNameExceededMaxLength;
			}

			return title;
		}

		private void redirectToNewAlbumPage(int newAlbumID)
		{
			if (_msgId > 0)
				Utils.Redirect(PageId.album, "aid={0}&msg={1}", newAlbumID, _msgId);
			else
				Utils.Redirect(PageId.album, "aid={0}", newAlbumID);
		}

		#endregion
	}
}