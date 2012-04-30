using System;
using System.Globalization;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;

namespace GalleryServerPro.Web.Controls
{
	/// <summary>
	/// A user control that contains the Action menu and the album breadcrumb menu.
	/// </summary>
	public partial class albummenu : GalleryUserControl
	{
		#region Private Fields

		private bool? _showActionMenu;

		#endregion

		#region Protected Events

		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			AddActionMenu();

			if (!this.GalleryPage.IsCallback)
			{
				phMenu.Controls.Add(new System.Web.UI.LiteralControl(BuildMenuString()));
			}
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Gets the CSS class to be used for the album breadcrumb menu.
		/// </summary>
		/// <returns></returns>
		protected string GetAlbumMenuClass()
		{
			if (this.ShowActionMenu)
			{
				return "albumMenu indented"; // Add the indented CSS class to make room for the action menu.
			}
			else
			{
				return "albumMenu";
			}
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets a value indicating whether the action menu should be displayed. Always returns false for anonymous users.
		/// Returns true for logged on users if they have permission to execute at least one of the commands in the menu
		/// against the current album and the preference setting is enabled.
		/// </summary>
		public bool ShowActionMenu
		{
			get
			{
				if (!this._showActionMenu.HasValue)
				{
					if (this.GalleryPage.IsAnonymousUser)
					{
						this._showActionMenu = false; // Always returns false for anonymous users.
					}
					else
					{
						// Return true if logged on user if one of the following is true:
						// (1) User is a site or gallery admin
						// (2) User has permission for at least one menu item and the preference setting is enabled.
						bool userHasPermissionForAtLeastOneItemInActionMenu = (this.GalleryPage.UserCanAdministerSite ||
							this.GalleryPage.UserCanAdministerGallery ||
							this.GalleryPage.UserCanEditMediaObject ||
							this.GalleryPage.UserCanEditAlbum ||
							this.GalleryPage.UserCanDeleteCurrentAlbum ||
							this.GalleryPage.UserCanDeleteMediaObject ||
							this.GalleryPage.UserCanSynchronize ||
							this.GalleryPage.UserCanAddAlbumToAtLeastOneAlbum ||
							this.GalleryPage.UserCanAddMediaObjectToAtLeastOneAlbum);

						bool userIsAdmin = (this.GalleryPage.UserCanAdministerSite || this.GalleryPage.UserCanAdministerGallery);

						this._showActionMenu = (userIsAdmin || (userHasPermissionForAtLeastOneItemInActionMenu & this.GalleryPage.ShowActionMenu));
					}
				}

				return this._showActionMenu.Value;
			}
		}

		#endregion

		#region Private Methods

		private void AddActionMenu()
		{
			if (this.ShowActionMenu)
				phActionMenu.Controls.Add(Page.LoadControl(Utils.GetUrl("/controls/actionmenu.ascx")));
		}

		private string BuildMenuString()
		{
			if (!this.GalleryPage.ShowAlbumBreadCrumb)
			{
				return String.Empty;
			}

			string menuString = string.Empty;
			string appPath = Utils.GetCurrentPageUrl();
			bool renderLinks = GalleryPage.GalleryControl.AllowUrlOverride;

			IAlbum album = GalleryPage.GetAlbum();
			IGalleryServerRoleCollection roles = this.GalleryPage.GetGalleryServerRolesForUser();
			string dividerText = Resources.GalleryServerPro.UC_Album_Menu_Album_Divider_Text;
			bool foundTopAlbum = false;
			bool foundBottomAlbum = false;
			while (!foundTopAlbum)
			{
				// Iterate through each album and it's parents, working the way toward the top. For each album, build up a breadcrumb menu item.
				// Eventually we will reach one of three situations: (1) a virtual album that contains the child albums, (2) an album the current
				// user does not have permission to view, or (3) the actual top-level album.
				if (album.IsVirtualAlbum)
				{
					menuString = menuString.Insert(0, String.Format(CultureInfo.CurrentCulture, " {0} <a href=\"{1}\">{2}</a>", dividerText, appPath, Resources.GalleryServerPro.Site_Virtual_Album_Title));
					foundTopAlbum = true;
				}
				else if (!Utils.IsUserAuthorized(SecurityActions.ViewAlbumOrMediaObject, roles, album.Id, album.GalleryId, album.IsPrivate))
				{
					// User is not authorized to view this album. If the user has permission to view more than one top-level album, then we want
					// to display an "All albums" link. To determine this, load the root album. If a virtual album is returned, then we know the
					// user has access to more than one top-level album. If it is an actual album (with a real ID and persisted in the data store),
					// that means that album is the only top-level album the user can view, and thus we do not need to create a link that is one
					// "higher" than that album.
					IAlbum rootAlbum = Factory.LoadRootAlbum(this.GalleryPage.GalleryId, SecurityActions.ViewAlbumOrMediaObject | SecurityActions.ViewOriginalImage, GalleryPage.GetGalleryServerRolesForUser(), Utils.IsAuthenticated);
					if (rootAlbum.IsVirtualAlbum)
					{
						menuString = menuString.Insert(0, String.Format(CultureInfo.CurrentCulture, " {0} <a href=\"{1}\">{2}</a>", dividerText, appPath, Resources.GalleryServerPro.Site_Virtual_Album_Title));
					}
					foundTopAlbum = true;
				}
				else
				{
					// Regular album somewhere in the hierarchy. Create a breadcrumb link.
					string hyperlinkIdString = String.Empty;
					if (!foundBottomAlbum)
					{
						hyperlinkIdString = " id=\"currentAlbumLink\""; // ID is referenced when inline editing an album's title
						foundBottomAlbum = true;
					}

					if (renderLinks)
					{
						menuString = menuString.Insert(0, String.Format(CultureInfo.CurrentCulture, " {0} <a{1} href=\"{2}\">{3}</a>", dividerText, hyperlinkIdString, Utils.AddQueryStringParameter(appPath, String.Concat("aid=", album.Id)), Utils.RemoveHtmlTags(album.Title)));
					}
					else
					{
						menuString = menuString.Insert(0, String.Format(CultureInfo.CurrentCulture, " {0} {1}", dividerText, Utils.RemoveHtmlTags(album.Title)));
					}
				}

				if (album.Parent is GalleryServerPro.Business.NullObjects.NullGalleryObject)
					foundTopAlbum = true;
				else
					album = (IAlbum)album.Parent;
			}

			if (menuString.Length > (dividerText.Length + 2))
			{
				menuString = menuString.Substring(dividerText.Length + 2); // Remove the first divider character
			}

			return menuString;
		}

		#endregion

	}
}