using System;
using System.Globalization;
using System.Web.UI.WebControls;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.Business.Metadata;

namespace GalleryServerPro.Web.Pages.Admin
{
	/// <summary>
	/// A page-like user control for administering media object metadata settings.
	/// </summary>
	public partial class metadata : Pages.AdminPage
	{
		#region Protected Events

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
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			this.CheckUserSecurity(SecurityActions.AdministerSite | SecurityActions.AdministerGallery);

			ConfigureControlsEveryTime();

			if (!IsPostBack)
			{
				ConfigureControlsFirstTime();
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
				SaveSettings();
			}

			return true;
		}

		/// <summary>
		/// Handles the OnValidateControl event of the wwDataBinder control.
		/// </summary>
		/// <param name="item">The wwDataBindingItem item.</param>
		/// <returns>Returns <c>true</c> if the item is valid; otherwise returns <c>false</c>.</returns>
		protected bool wwDataBinder_ValidateControl(GalleryServerPro.WebControls.wwDataBindingItem item)
		{
			// Validate various settings to make sure they don't conflict with each other.
			return true;
		}

		/// <summary>
		/// Handles the OnBeforeUnBindControl event of the wwDataBinder control.
		/// </summary>
		/// <param name="item">The wwDataBindingItem item.</param>
		protected bool wwDataBinder_BeforeUnbindControl(WebControls.wwDataBindingItem item)
		{
			// Disabled HTML items are not posted during a postback, so we don't have accurate information about their states. 
			// Look for the checkboxes that cause other controls to be disabled, and assign the value of the disabled control to their
			// database setting. This allows disabled controls to retain their original value if an admin later re-enables them.
			if (!this.chkExtractMetadata.Checked)
			{
				// When the metadata feature is unchecked, the enhanced metadata checkbox is disabled via javascript.
				if (item.ControlId == this.chkExtractMetadataUsingWpf.ID)
				{
					this.chkExtractMetadataUsingWpf.Checked = GallerySettings.ExtractMetadataUsingWpf;
					return false;
				}
			}
			return true;
		}

		#endregion

		#region Private Methods

		private void ConfigureControlsEveryTime()
		{
			this.PageTitle = Resources.GalleryServerPro.Admin_Media_Objects_Metadata_Page_Header;
			lblGalleryDescription.Text = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServerPro.Admin_Gallery_Description_Label, Utils.GetCurrentPageUrl(), Utils.HtmlEncode(Factory.LoadGallery(GalleryId).Description));
		}

		private void ConfigureControlsFirstTime()
		{
			AdminPageTitle = Resources.GalleryServerPro.Admin_Media_Objects_Metadata_Page_Header;

			if (AppSetting.Instance.License.IsInReducedFunctionalityMode)
			{
				wwMessage.ShowMessage(Resources.GalleryServerPro.Admin_Need_Product_Key_Msg2);
				wwMessage.CssClass = "wwErrorSuccess gsp_msgwarning";
				OkButtonBottom.Enabled = false;
				OkButtonTop.Enabled = false;
			}

			BindMetadata();

			this.wwDataBinder.DataBind();
		}

		private void BindMetadata()
		{
			rptrMetadata.DataSource = GallerySettingsUpdateable.MetadataDisplaySettings;
			rptrMetadata.DataBind();
		}

		private void SaveSettings()
		{
			this.wwDataBinder.Unbind(this);

			UnbindMetadataDisplayOptions();

			if (wwDataBinder.BindingErrors.Count > 0)
			{
				this.wwMessage.CssClass = "wwErrorFailure gsp_msgwarning";
				this.wwMessage.Text = wwDataBinder.BindingErrors.ToHtml();

				return;
			}

			GallerySettingsUpdateable.Save();

			HelperFunctions.PurgeCache();

			BindMetadata();

			this.wwMessage.CssClass = "wwErrorSuccess gsp_msgfriendly gsp_bold";
			this.wwMessage.ShowMessage(Resources.GalleryServerPro.Admin_Save_Success_Text);
		}

		private void UnbindMetadataDisplayOptions()
		{
			// 2|0|1|3|4|5|6|7|8|9|10|11|12|13|14|15|16|17|18|19|20|21|22|23|24|25|26|27|28|29|30|31|32|33|34|35|36|37|38|39|40|
			string[] metadataIds = Request.Form["gsp_seq"].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
			string[] checkedIds = Request.Form["gsp_chk"].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

			for (int seqIterator = 0; seqIterator < metadataIds.Length; seqIterator++)
			{
				string metadataId = metadataIds[seqIterator];
				IMetadataDefinition metadataDefinition = GallerySettingsUpdateable.MetadataDisplaySettings.Find((FormattedMetadataItemName)Convert.ToInt32(metadataId, CultureInfo.InvariantCulture));
				metadataDefinition.Sequence = seqIterator;
				metadataDefinition.IsVisible = (Array.IndexOf(checkedIds, metadataId) >= 0);
			}
		}

		#endregion
	}
}