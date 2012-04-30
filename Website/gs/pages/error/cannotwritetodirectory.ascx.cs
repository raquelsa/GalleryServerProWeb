using System;
using GalleryServerPro.ErrorHandler.CustomExceptions;
using System.Web.UI;

namespace GalleryServerPro.Web.Pages.Err
{
	/// <summary>
	/// A page-like user control that is displayed when the application cannot write to a directory.
	/// </summary>
	public partial class cannotwritetodirectory : UserControl
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
			hlHome.NavigateUrl = Utils.GetCurrentPageUrl();

			// The global error handler in Gallery.cs should have, just prior to transferring to this page, the original
			// CannotWriteToDirectoryException instance. Grab this instance and display its message.
			if (System.Web.HttpContext.Current != null)
			{
				CannotWriteToDirectoryException ex = System.Web.HttpContext.Current.Items["CurrentException"] as CannotWriteToDirectoryException;
				if (ex != null)
				{
					litErrorInfo.Text = Utils.HtmlEncode(ex.Message);
				}
				else
				{
					litErrorInfo.Text = Utils.HtmlEncode(new CannotWriteToDirectoryException().Message);
				}
			}
			else
			{
				litErrorInfo.Text = Utils.HtmlEncode(new CannotWriteToDirectoryException().Message);
			}
		}
	}
}