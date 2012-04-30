using System;
using System.Globalization;
using System.Web.UI.WebControls;
using GalleryServerPro.Business;
using GalleryServerPro.Web.Controller;

namespace GalleryServerPro.Web.Pages.Task
{
	/// <summary>
	/// A page-like user control that handles the Delete album task.
	/// </summary>
	public partial class deletealbum : Pages.TaskPage
	{
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

			this.CheckUserSecurity(SecurityActions.DeleteAlbum);

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
				int parentAlbumId = (this.GetAlbum().IsRootAlbum ? this.GetAlbum().Id : this.GetAlbum().Parent.Id);

				if (btnOkClicked())
					Utils.Redirect(PageId.album, "aid={0}", parentAlbumId);
			}

			return true;
		}

		#endregion

		#region Public Properties

		#endregion

		#region Private Methods

		private void ConfigureControls()
		{
			this.TaskHeaderText = Resources.GalleryServerPro.Task_Delete_Album_Header_Text;
			this.TaskBodyText = Resources.GalleryServerPro.Task_Delete_Album_Body_Text;
			this.OkButtonText = Resources.GalleryServerPro.Task_Delete_Album_Ok_Button_Text;
			this.OkButtonToolTip = Resources.GalleryServerPro.Task_Delete_Album_Ok_Button_Tooltip;

			this.PageTitle = Resources.GalleryServerPro.Task_Delete_Album_Page_Title;
		}

		private bool btnOkClicked()
		{
			//User clicked 'Delete album'.
			try
			{
				AlbumController.DeleteAlbum(this.GetAlbum(), !chkDeleteDbRecordsOnly.Checked);

				HelperFunctions.PurgeCache();

				return true;
			}
			catch (ErrorHandler.CustomExceptions.CannotDeleteAlbumException ex)
			{
				ucUserMessage.MessageTitle = Resources.GalleryServerPro.Task_Delete_Album_Cannot_Delete_Contains_User_Album_Parent_Hdr;
				ucUserMessage.MessageDetail = ex.Message;
				ucUserMessage.Visible = true;

				return false;
			}
		}

		#endregion
	}
}