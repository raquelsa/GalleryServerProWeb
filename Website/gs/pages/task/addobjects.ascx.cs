using System;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.ErrorHandler.CustomExceptions;
using GalleryServerPro.Web.Controller;
using GalleryServerPro.WebControls;

namespace GalleryServerPro.Web.Pages.Task
{
	/// <summary>
	/// A page-like user control that handles the Add objects task.
	/// </summary>
	public partial class addobjects : Pages.TaskPage
	{
		#region Properties

		protected string AddObjectsInstruction
		{
			get { return Resources.GalleryServerPro.Task_Add_Objects_Local_Media_Tab_Dtl; }
		}

		protected string AddObjectsUploadingText
		{
			get { return Resources.GalleryServerPro.Task_Add_Objects_Uploading_Text; }
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
			//JQueryRequired = true;
			JQueryUiRequired = true;
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

			this.CheckUserSecurity(SecurityActions.AddMediaObject);

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
			//An event from the control has bubbled up.  If it's the Ok button, then run the
			//code to save the data to the database; otherwise ignore.
			Button btn = source as Button;
			if ((btn != null) && (((btn.ID == "btnOkTop") || (btn.ID == "btnOkBottom"))))
			{
				if (AddExternalHtmlContent())
					RedirectToAlbumViewPage();
			}

			return true;
		}


		/// <summary>
		/// Handles the Click event of the btnCancel control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void btnCancel_Click(object sender, EventArgs e)
		{
			this.RedirectToPreviousPage();
		}

		#endregion

		#region Private Methods

		private void ConfigureControlsFirstTime()
		{
			//this.OkButtonIsVisible = false; // Instead, we'll use our own buttons inside the tab control.
			//this.CancelButtonIsVisible = false;
			this.OkButtonText = Resources.GalleryServerPro.Task_Add_Objects_OK_Btn_Text;
			this.TaskHeaderText = Resources.GalleryServerPro.Task_Add_Objects_Header_Text;
			this.TaskBodyText = Resources.GalleryServerPro.Task_Add_Objects_Body_Text;

			this.PageTitle = Resources.GalleryServerPro.Task_Add_Objects_Page_Title;

			ConfigureTabStrip();

			if (!HelperFunctions.IsFileAuthorizedForAddingToGallery("dummy.zip", GalleryId))
			{
				chkDoNotExtractZipFile.Enabled = false;
				chkDoNotExtractZipFile.CssClass = "gsp_disabledtext";
			}

			if (GallerySettings.DiscardOriginalImageDuringImport)
			{
				chkDiscardOriginal.Checked = true;
				chkDiscardOriginal.Enabled = false;
				chkDiscardOriginal.CssClass = "gsp_disabledtext";
			}

			HttpContext.Current.Session.Remove(GlobalConstants.SkippedFilesDuringUploadSessionKey);
			MediaObjectHashKeys.Clear();
		}

		private void ConfigureControlsEveryPageLoad()
		{
			RegisterJavascriptFiles();

			AddPopupInfoItems();
		}

		private void RegisterJavascriptFiles()
		{
			ScriptManager sm = ScriptManager.GetCurrent(this.Page);
			if (sm == null)
				throw new WebException("Gallery Server Pro requires a ScriptManager on the page.");

			sm.Scripts.Add(new ScriptReference(Utils.GetUrl("/script/plupload/plupload.full.js")));
			sm.Scripts.Add(new ScriptReference(Utils.GetUrl("/script/plupload/jquery.plupload.queue.js")));

#if DEBUG
			sm.Scripts.Add(new ScriptReference(Utils.GetUrl("/script/gallery.js")));
#else
			sm.Scripts.Add(new ScriptReference(Utils.GetUrl("/script/gallery.min.js")));
#endif
		}

		private void ConfigureTabStrip()
		{
			tsAddObjects.ImagesBaseUrl = String.Concat(Utils.GalleryRoot, "/images/componentart/tabstrip/");
			tsAddObjects.TopGroupSeparatorImagesFolderUrl = String.Concat(Utils.GalleryRoot, "/images/componentart/tabstrip/");

			// By default both tabs are invisible. Check config settings to see which ones are enabled, and set
			// visibility as needed.
			bool allowLocalContent = GallerySettings.AllowAddLocalContent;
			bool allowExternalContent = GallerySettings.AllowAddExternalContent;

			if (allowLocalContent)
			{
				tabLocalMedia.Visible = true;
				mpAddObjects.SelectPageById(pvAddLocal.ID);
			}

			if (allowExternalContent)
			{
				tabExternal.Visible = true;
				if (!allowLocalContent)
					mpAddObjects.SelectPageById(pvAddExternal.ID);
			}

			if ((!allowLocalContent) && (!allowExternalContent))
			{
				// Both settings are disabled, which means no objects can be added. This is probably a mis-configuration,
				// so give a friendly message to help point the administrator in the right direction for changing it.
				mpAddObjects.Visible = false;
				wwMessage.ShowMessage(Resources.GalleryServerPro.Task_Add_Objects_All_Adding_Types_Disabled_Msg);
				wwMessage.CssClass = "wwErrorSuccess gsp_msgwarning";
			}
		}

		private bool AddExternalHtmlContent()
		{
			string externalHtmlSource = txtExternalHtmlSource.Text.Trim();

			if (!this.ValidateExternalHtmlSource(externalHtmlSource))
				return false;

			MimeTypeCategory mimeTypeCategory = MimeTypeCategory.Other;
			string mimeTypeCategoryString = ddlMediaTypes.SelectedValue;
			try
			{
				mimeTypeCategory = (MimeTypeCategory)Enum.Parse(typeof(MimeTypeCategory), mimeTypeCategoryString, true);
			}
			catch { } // Suppress any parse errors so that category remains the default value 'Other'.

			string title = txtTitle.Text.Trim();
			if (String.IsNullOrEmpty(title))
			{
				// If user didn't enter a title, use the media category (e.g. Video, Audio, Image, Other).
				title = mimeTypeCategory.ToString();
			}

			using (IGalleryObject mediaObject = Factory.CreateExternalMediaObjectInstance(externalHtmlSource, mimeTypeCategory, this.GetAlbum()))
			{
				mediaObject.Title = Utils.CleanHtmlTags(title, GalleryId);
				GalleryObjectController.SaveGalleryObject(mediaObject);
			}
			HelperFunctions.PurgeCache();

			return true;
		}

		private bool ValidateExternalHtmlSource(string externalHtmlSource)
		{
			IHtmlValidator htmlValidator = Factory.GetHtmlValidator(externalHtmlSource, GalleryId);
			htmlValidator.Validate();
			if (!htmlValidator.IsValid)
			{
				string invalidHtmlTags = String.Join(", ", htmlValidator.InvalidHtmlTags.ToArray());
				string invalidHtmlAttributes = String.Join(", ", htmlValidator.InvalidHtmlAttributes.ToArray());
				string javascriptDetected = (htmlValidator.InvalidJavascriptDetected ? Resources.GalleryServerPro.Task_Add_Objects_External_Tab_Javascript_Detected_Yes : Resources.GalleryServerPro.Task_Add_Objects_External_Tab_Javascript_Detected_No);

				if (String.IsNullOrEmpty(invalidHtmlTags))
					invalidHtmlTags = Resources.GalleryServerPro.Task_Add_Objects_External_Tab_No_Invalid_Html;

				if (String.IsNullOrEmpty(invalidHtmlAttributes))
					invalidHtmlAttributes = Resources.GalleryServerPro.Task_Add_Objects_External_Tab_No_Invalid_Html;

				this.wwMessage.ShowMessage(String.Format(CultureInfo.InvariantCulture, Resources.GalleryServerPro.Task_Add_Objects_External_Tab_Invalid_Html_Msg, invalidHtmlTags, invalidHtmlAttributes, javascriptDetected));
				this.wwMessage.CssClass = "wwErrorSuccess gsp_msgattention";
				return false;
			}
			return true;
		}

		/// <summary>
		/// Add any PopupInfoItem controls that cannot be declaratively created in the aspx page because one or more
		/// properties are computed.
		/// </summary>
		private void AddPopupInfoItems()
		{
			// Create the popup for the local media tab's body text in the overview section, just below the text
			// "Select one or more files on your hard drive". If it wasn't for the string formatting of the DialogBody property, 
			// we could have declared it in the aspx page like this:

			//<tis:PopupInfoItem ID="PopupInfoItem2" runat="server" ControlId="lblLocalMediaOverview" DialogTitle="<%$ Resources:GalleryServerPro, Task_Add_Objects_Local_Media_Overview_Hdr %>"
			//DialogBody="<% =GetLocalMediaPopupBodyText();%>" />
			int maxUploadSize = GallerySettings.MaxUploadSize;

			PopupInfoItem popupInfoItem = new PopupInfoItem();
			popupInfoItem.ID = "poi7";
			popupInfoItem.ControlId = "lblLocalMediaOverview";
			popupInfoItem.DialogTitle = Resources.GalleryServerPro.Task_Add_Objects_Local_Media_Overview_Hdr;
			popupInfoItem.DialogBody = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.Task_Add_Objects_Local_Media_Overview_Bdy, maxUploadSize);

			PopupInfo1.PopupInfoItems.Add(popupInfoItem);
		}

		#endregion
	}
}