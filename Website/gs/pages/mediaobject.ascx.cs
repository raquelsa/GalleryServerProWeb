using System;
using GalleryServerPro.Web.Controls;

namespace GalleryServerPro.Web.Pages
{
	/// <summary>
	/// A page-like user control that renders a media object.
	/// </summary>
	public partial class mediaobject : Pages.GalleryPage
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="mediaobject"/> class.
		/// </summary>
		protected mediaobject()
		{
			this.BeforeHeaderControlsAdded += MediaObjectBeforeHeaderControlsAdded;
		}

		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			ShowMessage();
		}

		/// <summary>
		/// Renders the <see cref="Message"/>. No action is taken if <see cref="Message"/> is Message.None.
		/// </summary>
		private void ShowMessage()
		{
			if (this.Message == Message.None)
				return;

			usermessage msgBox = this.GetMessageControl();

			phMessage.Controls.Add(msgBox);
		}

		private void MediaObjectBeforeHeaderControlsAdded(object sender, EventArgs e)
		{
			bool showAlbumTreeViewSetting = (this.GalleryControl.ShowAlbumTreeViewForAlbum.HasValue ? this.GalleryControl.ShowAlbumTreeViewForAlbum.Value : false);
			
			ShowAlbumTreeViewForAlbum = (ShowAlbumTreeViewForMediaObject & showAlbumTreeViewSetting);
		}
	}
}