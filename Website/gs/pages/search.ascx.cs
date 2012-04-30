using System;
using System.Globalization;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;

namespace GalleryServerPro.Web.Pages
{
	/// <summary>
	/// A page-like user control that provides search functionality.
	/// </summary>
	public partial class search : Pages.GalleryPage
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="search"/> class.
		/// </summary>
		protected search()
		{
			this.BeforeHeaderControlsAdded += SearchBeforeHeaderControlsAdded;
		}

		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			this.Page.Form.DefaultButton = btnSearch.UniqueID;
			imgSearchIcon.ImageUrl = String.Concat(Utils.GalleryRoot, "/images/search_48x48.png");

			SearchGallery(GetSearchText());
		}

		private string GetSearchText()
		{
			return (IsNewPageLoad ? Server.UrlDecode(Utils.GetQueryStringParameterString("search")) : this.txtSearch.Text.Trim());
		}

		private void SearchGallery(string searchText)
		{
			IGalleryObjectCollection galleryObjects = null;

			if (!String.IsNullOrEmpty(searchText))
			{
				// Search gallery and display results.
				galleryObjects = HelperFunctions.SearchGallery(this.GalleryId, searchText, this.GetGalleryServerRolesForUser(), Utils.IsAuthenticated);

				if (galleryObjects != null)
				{
					tv.GalleryObjectsDataSource = galleryObjects;
					searchResultTitle.InnerText = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.Search_Results_Text, galleryObjects.Count);
				}
				else
				{
					tv.Visible = false;
				}
			}
			else // No search text found
			{
				searchResultTitle.InnerText = Resources.GalleryServerPro.Search_Instructions;
				tv.Visible = false;
			}
		}

		private void SearchBeforeHeaderControlsAdded(object sender, EventArgs e)
		{
			ShowAlbumTreeViewForAlbum = (this.GalleryControl.ShowAlbumTreeViewForAlbum.HasValue ? this.GalleryControl.ShowAlbumTreeViewForAlbum.Value : false);
		}
	}
}