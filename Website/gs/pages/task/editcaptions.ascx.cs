using System;
using System.Globalization;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.ErrorHandler.CustomExceptions;
using GalleryServerPro.Web.Controller;

namespace GalleryServerPro.Web.Pages.Task
{
	/// <summary>
	/// A page-like user control that handles the Edit captions task.
	/// </summary>
	public partial class editcaptions : Pages.TaskPage
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
			this.CheckUserSecurity(SecurityActions.EditMediaObject);

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
			//An event from a control has bubbled up.  If it's the Ok button, then run the
			//code to synchronize; otherwise ignore.
			Button btn = source as Button;
			if ((btn != null) && (((btn.ID == "btnOkTop") || (btn.ID == "btnOkBottom"))))
			{
				btnOkClicked();

				RedirectToAlbumViewPage();
			}

			return true;
		}

		#endregion

		#region Private Methods

		private void ConfigureControls()
		{
			this.TaskHeaderText = Resources.GalleryServerPro.Task_Edit_Captions_Header_Text;
			this.TaskBodyText = Resources.GalleryServerPro.Task_Edit_Captions_Body_Text;
			this.OkButtonText = Resources.GalleryServerPro.Task_Edit_Captions_Ok_Button_Text;
			this.OkButtonToolTip = Resources.GalleryServerPro.Task_Edit_Captions_Ok_Button_Tooltip;

			this.PageTitle = Resources.GalleryServerPro.Task_Edit_Captions_Page_Title;

			IGalleryObjectCollection albumChildren = this.GetAlbum().GetChildGalleryObjects(GalleryObjectType.MediaObject, true);

			if (albumChildren.Count > 0)
			{
				const int textareaWidthBuffer = 30; // Extra width padding to allow room for the caption.
				const int textareaHeightBuffer = 72; // Extra height padding to allow room for the caption.
				SetThumbnailCssStyle(albumChildren, textareaWidthBuffer, textareaHeightBuffer);

				rptr.DataSource = albumChildren;
				rptr.DataBind();
			}
			else
			{
				this.RedirectToAlbumViewPage("msg={0}", ((int)Message.CannotEditCaptionsNoEditableObjectsExistInAlbum).ToString(CultureInfo.InvariantCulture));
			}
		}

		private void btnOkClicked()
		{
			SaveCaptions();
		}

		private void SaveCaptions()
		{
			// Iterate through all the textboxes, saving any captions that have changed.
			// The media object IDs are stored in a hidden input tag.
			HtmlTextArea ta;
			HtmlInputHidden gc;
			IGalleryObject mo;

			if (!IsUserAuthorized(SecurityActions.EditMediaObject))
				return;

			try
			{
				HelperFunctions.BeginTransaction();

				// Loop through each item in the repeater control. If an item is checked, extract the ID.
				foreach (RepeaterItem rptrItem in rptr.Items)
				{
					ta = (HtmlTextArea) rptrItem.Controls[1]; // The <TEXTAREA> tag
					gc = (HtmlInputHidden) rptrItem.Controls[3]; // The hidden <input> tag

					// Retrieve new title. Since the Value property of <TEXTAREA> HTML ENCODEs the text,
					// and we want to store the actual text, we must decode to get back to the original.
					string newTitle = Utils.HtmlDecode(ta.Value);

					try
					{
						mo = Factory.LoadMediaObjectInstance(Convert.ToInt32(gc.Value, CultureInfo.InvariantCulture), true);
					}
					catch (InvalidMediaObjectException)
					{
						continue; // Gallery object may have been deleted by someone else, so just skip it.
					}

					string previousTitle = mo.Title;

					mo.Title = Utils.CleanHtmlTags(newTitle, GalleryId);

					if (mo.Title != previousTitle)
					{
						GalleryObjectController.SaveGalleryObject(mo);
					}
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