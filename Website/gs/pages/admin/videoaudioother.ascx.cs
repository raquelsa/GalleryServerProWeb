using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.Data;
using GalleryServerPro.ErrorHandler.CustomExceptions;
using GalleryServerPro.Web.Controller;

namespace GalleryServerPro.Web.Pages.Admin
{
	/// <summary>
	/// A page-like user control for administering settings for video, audio, and generic media objects.
	/// </summary>
	public partial class videoaudioother : Pages.AdminPage
	{
		#region Event Handlers

		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			this.CheckUserSecurity(SecurityActions.AdministerSite | SecurityActions.AdministerGallery);

			ConfigureControlsEveryTime();

			RegisterJavascript();

			if (!IsPostBack)
			{
				ConfigureControlsFirstTime();
			}
		}

		/// <summary>
		/// Handles the Init event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Init(object sender, EventArgs e)
		{
			this.AdminHeaderPlaceHolder = phAdminHeader;
			this.AdminFooterPlaceHolder = phAdminFooter;

			JQueryUiRequired = true;
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
				SaveSettings();
			}

			return true;
		}

		#endregion

		#region Properties

		protected string EsConvertString
		{
			get { return Resources.GalleryServerPro.Admin_VidAudOther_EncoderSettings_Convert; }
		}

		protected string EsToString
		{
			get { return Resources.GalleryServerPro.Admin_VidAudOther_EncoderSettings_To; }
		}

		protected string EsFFmpegArgsString
		{
			get { return Resources.GalleryServerPro.Admin_VidAudOther_EncoderSettings_FFmpegArgs; }
		}

		protected string EsMoveTooltip
		{
			get { return Resources.GalleryServerPro.Admin_VidAudOther_EncoderSettings_MoveTooltip; }
		}

		protected string EqExpandString
		{
			get { return Resources.GalleryServerPro.Admin_VidAudOther_EncoderQueue_Expand_Text; }
		}

		protected string EqCollapseString
		{
			get { return Resources.GalleryServerPro.Admin_VidAudOther_EncoderQueue_Collapse_Text; }
		}

		protected string EqMinutesString
		{
			get { return Resources.GalleryServerPro.Admin_VidAudOther_EncoderQueue_Minutes_Text; }
		}

		#endregion

		#region Private Methods

		private void ConfigureControlsEveryTime()
		{
			this.PageTitle = Resources.GalleryServerPro.Admin_Video_Audio_Other_General_Page_Header;
			lblGalleryDescription.Text = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServerPro.Admin_Gallery_Description_Label, Utils.GetCurrentPageUrl(), Utils.HtmlEncode(Factory.LoadGallery(GalleryId).Description));

			ConfigureEncoderStatus();
		}

		private void ConfigureEncoderStatus()
		{
			if (AppSetting.Instance.AppTrustLevel != ApplicationTrustLevel.Full)
			{
				lblEncoderStatus.Text = String.Format(Resources.GalleryServerPro.Admin_VidAudOther_EncoderSettings_InsufficientTrust_Msg, AppSetting.Instance.AppTrustLevel);
				lblEncoderStatus.CssClass = "gsp_msgattention";
				return;
			}

			if (String.IsNullOrEmpty(AppSetting.Instance.FFmpegPath))
			{
				lblEncoderStatus.Text = Resources.GalleryServerPro.Admin_VidAudOther_EncoderSettings_FFmpegMissing_Msg;
				lblEncoderStatus.CssClass = "gsp_msgattention";
				return;
			}

			lblEncoderStatus.Text = MediaConversionQueue.Instance.Status.ToString();
			lblEncoderStatus.CssClass = "gsp_msgfriendly";
		}

		private void RegisterJavascript()
		{
			ScriptManager sm = ScriptManager.GetCurrent(this.Page);
			if (sm == null)
			{
				throw new WebException("Gallery Server Pro requires a ScriptManager on the page.");
			}

#if DEBUG
			sm.Scripts.Add(new ScriptReference(Utils.GetUrl("/script/gallery.js")));
#else
			sm.Scripts.Add(new ScriptReference(Utils.GetUrl("/script/gallery.min.js")));
#endif

			sm.Scripts.Add(new ScriptReference(Utils.GetUrl("/script/globalize.js")));
		}

		private void ConfigureControlsFirstTime()
		{
			AdminPageTitle = Resources.GalleryServerPro.Admin_Video_Audio_Other_General_Page_Header;

			AssignHiddenFormFields();

			BindQueue();

			if (AppSetting.Instance.License.IsInReducedFunctionalityMode)
			{
				wwMessage.ShowMessage(Resources.GalleryServerPro.Admin_Need_Product_Key_Msg2);
				wwMessage.CssClass = "wwErrorSuccess gsp_msgwarning";
				OkButtonBottom.Enabled = false;
				OkButtonTop.Enabled = false;
			}

			this.wwDataBinder.DataBind();
		}

		private void BindQueue()
		{
			List<MediaQueueDto> queueItems = (from mo in MediaConversionQueue.Instance.MediaQueueItems orderby mo.DateAdded descending select mo).ToList();

			RemoveItemsCurrentUserDoesNotHavePermissionToSee(queueItems);

			hdnQueueItems.Value = queueItems.ToJson();
		}

		private void RemoveItemsCurrentUserDoesNotHavePermissionToSee(List<MediaQueueDto> queueItems)
		{
			if (UserCanAdministerSite)
			{
				return;
			}
			else if (!UserCanAdministerSite && UserCanAdministerGallery)
			{
				// Trim the list of queue items to only those that belong to galleries the current
				// user is an administrator for.
				List<MediaQueueDto> itemsToRemove = new List<MediaQueueDto>();
				IGalleryCollection galleries = UserController.GetGalleriesCurrentUserCanAdminister();

				foreach (MediaQueueDto item in queueItems)
				{
					if (galleries.FindById(Factory.LoadMediaObjectInstance(item.FKMediaObjectId).GalleryId) == null)
					{
						itemsToRemove.Add(item);
					}
				}

				foreach (MediaQueueDto item in itemsToRemove)
				{
					queueItems.Remove(item);
				}
			}
			else
			{
				queueItems.Clear(); // Not a site or gallery admin; they shouldn't see anything
			}
		}

		private void AssignHiddenFormFields()
		{
			hdnEncoderSettings.Value = MediaEncoderSettingsController.ToEntities(GallerySettings.MediaEncoderSettings).ToJson();

			Entity.FileExtension[] destAvailableFileExtensions = MediaEncoderSettingsController.GetAvailableFileExtensions();
			var srcExtensions = new List<Entity.FileExtension>(destAvailableFileExtensions.Length + 2)
			  {
			    new Entity.FileExtension {Value = "*audio",Text = Resources.GalleryServerPro.Admin_VidAudOther_SourceFileExt_All_Audio},
			    new Entity.FileExtension {Value = "*video",Text = Resources.GalleryServerPro.Admin_VidAudOther_SourceFileExt_All_Video}
			  };

			srcExtensions.AddRange(destAvailableFileExtensions);

			hdnSourceAvailableFileExtensions.Value = srcExtensions.ToArray().ToJson();
			hdnDestinationAvailableFileExtensions.Value = destAvailableFileExtensions.ToJson();
		}

		private void SaveSettings()
		{
			string encoderSettingsStr = hdnEncoderSettings.Value;

			Entity.MediaEncoderSettings[] encoderSettings = encoderSettingsStr.FromJson<Entity.MediaEncoderSettings[]>();

			GallerySettingsUpdateable.MediaEncoderSettings = MediaEncoderSettingsController.ToMediaEncoderSettingsCollection(encoderSettings);


			this.wwDataBinder.Unbind(this);

			if (wwDataBinder.BindingErrors.Count > 0)
			{
				this.wwMessage.CssClass = "wwErrorFailure gsp_msgwarning";
				this.wwMessage.Text = wwDataBinder.BindingErrors.ToHtml();

				return;
			}

			GallerySettingsUpdateable.Save();

			this.wwMessage.CssClass = "wwErrorSuccess gsp_msgfriendly gsp_bold";
			this.wwMessage.ShowMessage(Resources.GalleryServerPro.Admin_Save_Success_Text);

			hdnEncoderSettings.Value = MediaEncoderSettingsController.ToEntities(GallerySettingsUpdateable.MediaEncoderSettings).ToJson();
		}

		#endregion
	}
}