using System;
using System.Web.UI;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.Business;

namespace GalleryServerPro.Web.Pages.Err
{
	/// <summary>
	/// A page-like user control that is used when un unhandled exception occurs.
	/// </summary>
	public partial class generic : UserControl
	{
		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			this.ConfigureControls();
		}

		private void ConfigureControls()
		{
			imgGspLogo.ImageUrl = Utils.GetUrl("/images/gsp_logo_313x75.png");
			hlSiteAdmin.NavigateUrl = Utils.GetUrl(PageId.admin_sitesettings);
			hlHome.NavigateUrl = Utils.GetCurrentPageUrl();

			// The global error handler in Gallery.cs should have, just prior to transferring to this page, placed the original
			// error in the HttpContext Items bag.
			IAppError error = null;
			if (System.Web.HttpContext.Current != null)
			{
				error = System.Web.HttpContext.Current.Items["CurrentAppError"] as IAppError;
			}

			if (ShouldShowErrorDetails(error))
			{
				pErrorDtl2.Visible = false;
				lblSeparator.Visible = false;
				hlSiteAdmin.Visible = false;

				if (error != null)
				{
					this.Page.Header.Controls.Add(new LiteralControl(error.CssStyles));

					litErrorDetails.Text = error.ToHtml();
				}
				else
				{
					litErrorDetails.Text = "<p class='gsp_msgwarning'>Error information missing from HttpContext Items bag. Please submit bug report to the <a href='http://forum.galleryserverpro.com'>Gallery Server Pro forum</a>.</p>";
				}
			}
		}

		/// <summary>
		/// Determines whether details about the <paramref name="error" /> can be shown to the user. When the app is in debug 
		/// mode (debug = true in web.config), then always show the error details. If debug = false, then use the ShowErrorDetails setting.
		/// </summary>
		/// <param name="error">The error.</param>
		/// <returns></returns>
		private static bool ShouldShowErrorDetails(IAppError error)
		{
			bool isInDebugMode = System.Web.HttpContext.Current.IsDebuggingEnabled;

			bool showErrorDetailsSettingFromConfig = false;
			if ((error != null) && (error.GalleryId > Int32.MinValue))
			{
				try
				{
					// We may get an error trying to load the setting from the database. In this case, just ignore the error and use the default
					// value.
					showErrorDetailsSettingFromConfig = Factory.LoadGallerySetting(error.GalleryId).ShowErrorDetails;
				}
				catch { }
			}

			return (isInDebugMode || showErrorDetailsSettingFromConfig);
		}
	}
}