using System;
using GalleryServerPro.Business;

namespace GalleryServerPro.Web.Controls.Admin
{
	/// <summary>
	/// The menu in the Site admin area.
	/// </summary>
	public partial class adminmenu : GalleryUserControl
	{
		#region Event Handlers

		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			ConfigureControlsEveryTime();
		}

		#endregion

		#region Private Methods

		private void ConfigureControlsEveryTime()
		{
			nbAdminMenu.ImagesBaseUrl = String.Concat(Utils.GalleryRoot, "/images/componentart/navbar/");

			nbiSiteGeneral.NavigateUrl = Utils.GetUrl(PageId.admin_sitesettings, "aid={0}", GalleryPage.GetAlbumId());
			nbiBackupRestore.NavigateUrl = Utils.GetUrl(PageId.admin_backuprestore, "aid={0}", GalleryPage.GetAlbumId());
			nbiGalleries.NavigateUrl = Utils.GetUrl(PageId.admin_galleries, "aid={0}", GalleryPage.GetAlbumId());
			nbiGallerySetting.NavigateUrl = Utils.GetUrl(PageId.admin_gallerysettings, "aid={0}", GalleryPage.GetAlbumId());
			nbiGalleryControl.NavigateUrl = Utils.GetUrl(PageId.admin_gallerycontrolsettings, "aid={0}", GalleryPage.GetAlbumId());
			nbiErrorLog.NavigateUrl = Utils.GetUrl(PageId.admin_errorlog, "aid={0}", GalleryPage.GetAlbumId());
			nbiUserSettings.NavigateUrl = Utils.GetUrl(PageId.admin_usersettings, "aid={0}", GalleryPage.GetAlbumId());
			nbiManageUsers.NavigateUrl = Utils.GetUrl(PageId.admin_manageusers, "aid={0}", GalleryPage.GetAlbumId());
			nbiManageRoles.NavigateUrl = Utils.GetUrl(PageId.admin_manageroles, "aid={0}", GalleryPage.GetAlbumId());
			nbiAlbumsGeneral.NavigateUrl = Utils.GetUrl(PageId.admin_albums, "aid={0}", GalleryPage.GetAlbumId());
			nbiMediaObjectsGeneral.NavigateUrl = Utils.GetUrl(PageId.admin_mediaobjects, "aid={0}", GalleryPage.GetAlbumId());
			nbiMetadata.NavigateUrl = Utils.GetUrl(PageId.admin_metadata, "aid={0}", GalleryPage.GetAlbumId());
			nbiMediaObjectTypes.NavigateUrl = Utils.GetUrl(PageId.admin_mediaobjecttypes, "aid={0}", GalleryPage.GetAlbumId());
			nbiImages.NavigateUrl = Utils.GetUrl(PageId.admin_images, "aid={0}", GalleryPage.GetAlbumId());
			nbiVideoAudioOther.NavigateUrl = Utils.GetUrl(PageId.admin_videoaudioother, "aid={0}", GalleryPage.GetAlbumId());

			if (this.GalleryPage.UserCanAdministerSite || this.GalleryPage.UserCanAdministerGallery)
			{
				// Proactive security: Even though the pages that use this control have their own security that make this redundant,
				// we do it anyway for extra protection. Only show menu when user is a site or gallery admin.
				nbAdminMenu.Visible = true;

				if (!this.GalleryPage.UserCanAdministerSite)
				{
					// Hide the site-level settings from gallery administators
					nbiSiteSettings.Visible = false;

					// Hide the user/role management pages from gallery admins when the app setting says that can't manage them.
					nbiManageUsers.Visible = AppSetting.Instance.AllowGalleryAdminToManageUsersAndRoles;
					nbiManageRoles.Visible = AppSetting.Instance.AllowGalleryAdminToManageUsersAndRoles;
				}
			}
		}

		#endregion
	}
}