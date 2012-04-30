using System;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GalleryServerPro.Web.Controls
{
	/// <summary>
	/// A user control that provides search functionality in a popup window.
	/// </summary>
	public partial class search : GalleryUserControl
	{
		#region Protected Events

		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			RegisterJavascript();
		}

		/// <summary>
		/// Handles the Click event of the btnSearch control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void btnSearch_Click(object sender, EventArgs e)
		{
			// Get reference to the Search textbox.
			TextBox txtSearch = (TextBox)this.GalleryPage.FindControlRecursive(dgSearch, "txtSearch");

			Utils.Redirect(PageId.search, String.Format(CultureInfo.InvariantCulture, "aid={0}&search={1}", this.GalleryPage.GetAlbum().Id, Utils.UrlEncode(txtSearch.Text)));
		}

		#endregion

		#region Private Methods

		private void RegisterJavascript()
		{
			if (this.GalleryPage.ShowSearch)
			{
				// Get reference to the UserName textbox.
				TextBox txtSearch = (TextBox)this.GalleryPage.FindControlRecursive(dgSearch, "txtSearch");

				string script = String.Format(CultureInfo.InvariantCulture, @"
function toggleSearch()
{{
	if (typeof(dgLogin) !== 'undefined')
		dgLogin.close();

	if (dgSearch.get_isShowing())
		dgSearch.close();
	else
		dgSearch.show();
}}

function dgSearch_OnShow()
{{
	$get('{0}').focus();
}}", txtSearch.ClientID);

				ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "searchFocusScript", script, true);
			}
		}

		#endregion

	}
}