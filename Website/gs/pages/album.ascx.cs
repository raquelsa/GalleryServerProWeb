using System;
using GalleryServerPro.Web.Controls;

namespace GalleryServerPro.Web.Pages
{
	/// <summary>
	/// A page-like user control that displays the contents of an album.
	/// </summary>
	public partial class album : Pages.GalleryPage
	{
		#region Private Fields


		#endregion
		
		/// <summary>
		/// Initializes a new instance of the <see cref="album"/> class.
		/// </summary>
		protected album()
		{
			this.BeforeHeaderControlsAdded += AlbumBeforeHeaderControlsAdded;
		}

		#region Public Properties

		#endregion

		#region Protected Events

		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			ShowMessage();
		}

		#endregion

		#region Private Static Methods

		#endregion

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

		private void AlbumBeforeHeaderControlsAdded(object sender, EventArgs e)
		{
			ShowAlbumTreeViewForAlbum = (this.GalleryControl.ShowAlbumTreeViewForAlbum.HasValue ? this.GalleryControl.ShowAlbumTreeViewForAlbum.Value : false);
		}
	}
}