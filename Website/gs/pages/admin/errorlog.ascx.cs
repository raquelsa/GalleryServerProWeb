using System;
using System.Data;
using System.Globalization;
using ComponentArt.Web.UI;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.Web.Controller;

namespace GalleryServerPro.Web.Pages.Admin
{
	/// <summary>
	/// A page-like user control for interacting with the application's event log.
	/// </summary>
	public partial class errorlog : Pages.AdminPage
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
		}

		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			this.CheckUserSecurity(SecurityActions.AdministerSite | SecurityActions.AdministerGallery);

			if (!IsPostBack)
			{
				ValidateErrorLog();

				ConfigureControlsFirstPageLoad();

				BindData();
			}

			ConfigureControlsEveryPageLoad();
		}

		/// <summary>
		/// Handles the Click event of the btnClearLog control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void btnClearLog_Click(object sender, EventArgs e)
		{
			// When the user is a sys admin, delete all errors. When user is a gallery admin, just delete errors the user
			// has permission to administer.
			if (UserCanAdministerSite)
			{
				foreach (IGallery gallery in Factory.LoadGalleries())
				{
					ErrorHandler.Error.ClearErrorLog(gallery.GalleryId);
				}
			}
			else if (UserCanAdministerGallery)
			{
				foreach (IGallery gallery in UserController.GetGalleriesCurrentUserCanAdminister())
				{
					ErrorHandler.Error.ClearErrorLog(gallery.GalleryId);
				}
			}

			HelperFunctions.PurgeCache();

			BindData();
		}

		#endregion

		#region Private Methods

		private void ConfigureControlsFirstPageLoad()
		{
			AdminPageTitle = Resources.GalleryServerPro.Admin_Site_Settings_Error_Log_Page_Header;
			OkButtonIsVisible = false;
			CancelButtonIsVisible = false;

			if (AppSetting.Instance.License.IsInReducedFunctionalityMode)
			{
				wwMessage.ShowMessage(Resources.GalleryServerPro.Admin_Need_Product_Key_Msg2);
				wwMessage.CssClass = "wwErrorSuccess gsp_msgwarning";
				btnClearLog.Enabled = false;
			}

			btnClearLog.OnClientClick = String.Concat("return confirm('", Resources.GalleryServerPro.Admin_Error_ClearLog_Confirm_Txt, "')");

			ConfigureGrid();
		}

		private void ConfigureControlsEveryPageLoad()
		{
			this.PageTitle = Resources.GalleryServerPro.Admin_Site_Settings_Error_Log_Page_Header;

			if (AppSetting.Instance.License.IsInReducedFunctionalityMode)
			{
				wwMessage.ShowMessage(Resources.GalleryServerPro.Admin_Need_Product_Key_Msg2);
				wwMessage.CssClass = "wwErrorSuccess gsp_msgwarning";
				OkButtonBottom.Enabled = false;
				OkButtonTop.Enabled = false;
			}
			else
			{
				AddEditColumnClientTemplate();
			}

			AddGridClientTemplates();
		}

		private void ConfigureGrid()
		{
			gd.ImagesBaseUrl = String.Concat(Utils.GalleryRoot, "/images/componentart/grid/");
			gd.TreeLineImagesFolderUrl = String.Concat(Utils.GalleryRoot, "/images/componentart/grid/lines/");

			string emptyGridText = String.Format(CultureInfo.CurrentCulture, "<span class=\"gsp_msgfriendly gdInfoEmptyGridText\">{0}</span>", Resources.GalleryServerPro.Admin_Error_Grid_Empty_Text);
			gd.EmptyGridText = emptyGridText;
		}

		private void AddEditColumnClientTemplate()
		{
			//Add the client template containing the editing controls.
			ClientTemplate optionsClientTemplate = new ClientTemplate();
			optionsClientTemplate.ID = "ctOptions";

			optionsClientTemplate.Text = String.Format(CultureInfo.InvariantCulture, @"
<a href=""javascript:deleteRow('## DataItem.ClientId ##')"" title='{0}' class=""gsp_addleftmargin1"">
				<img src='{1}' alt='{0}' /></a>
					",
				Resources.GalleryServerPro.Admin_Error_Grid_Ex_Delete_Tooltip,
				Utils.GetUrl("images/componentart/grid/delete.png")
				);

			gd.ClientTemplates.Add(optionsClientTemplate);
		}

		private void AddGridClientTemplates()
		{
			AddErrDetailsClientTemplate();

			AddGalleryIdClientTemplate();
		}

		private void AddErrDetailsClientTemplate()
		{
			ClientTemplate ctErrDetails = new ClientTemplate();
			ctErrDetails.ID = "ctErrDetails";
			ctErrDetails.Text = Resources.GalleryServerPro.Admin_Error_Error_Details_Hdr;

			gd.ClientTemplates.Add(ctErrDetails);
		}

		private void AddGalleryIdClientTemplate()
		{
			// Add edit column client template
			ClientTemplate galleryIdColumn = new ClientTemplate();

			galleryIdColumn.ID = "ctGalleryId";
			galleryIdColumn.Text = String.Format(CultureInfo.InvariantCulture, @"
## DataItem.getMember('GalleryId').get_value() == -2147483648 ? ""{0}"" : DataItem.getMember('GalleryId').get_value() ##",
				Resources.GalleryServerPro.Admin_Error_Gallery_ID_Not_Available);

			gd.ClientTemplates.Add(galleryIdColumn);
		}

		private void BindData()
		{
			BindGrid();
		}

		private void BindGrid()
		{
			DataSet errorsDataSet = null;
			if (UserCanAdministerSite)
			{
				errorsDataSet = AppErrorController.GetAppErrorsDataSet(int.MinValue, true);
			}
			else
			{
				// Get errors for all galleries current user is a gallery admin for.
				foreach (IGallery gallery in UserController.GetGalleriesCurrentUserCanAdminister())
				{
					if (errorsDataSet == null)
					{
						errorsDataSet = AppErrorController.GetAppErrorsDataSet(gallery.GalleryId, true);
					}
					else
					{
						DataSet errorsforGallery = AppErrorController.GetAppErrorsDataSet(gallery.GalleryId, false);

						foreach (DataTable dataTable in errorsforGallery.Tables)
						{
							errorsDataSet.Merge(dataTable);
						}
					}
				}
			}

			gd.DataSource = errorsDataSet;
			gd.DataBind();
		}

		/// <summary>
		/// Remove errors if needed to ensure log does not exceed max log size. Normally the log size is validated each time an error
		/// occurs, but we run it here in case the user just reduced the log size setting.
		/// </summary>
		private static void ValidateErrorLog()
		{
			int numItemsDeleted = ErrorHandler.Error.ValidateLogSize(AppSetting.Instance.MaxNumberErrorItems);

			if (numItemsDeleted > 0)
			{
				HelperFunctions.PurgeCache();
			}
		}

		#endregion
	}
}