using System;

namespace GalleryServerPro.Web.Pages
{
	/// <summary>
	/// A page-like user control that renders the albums in a treeview.
	/// </summary>
	public partial class albumtreeview : Pages.GalleryPage
	{
		#region Private Fields

		private bool _showCheckbox;
		private bool _selectCurrentAlbum;
		private int _rootAlbumId = int.MinValue;

		#endregion

		#region Event Handlers

		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
				ConfigureControlsFirstTime();
			}
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets a value indicating whether a checkbox is to be rendered for each album in the treeview. The default value
		/// is false. This property should be set before calling DataBind().
		/// </summary>
		public bool ShowCheckbox
		{
			get { return _showCheckbox; }
			set { _showCheckbox = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the current album should be selected in the treeview.
		/// </summary>
		/// <value><c>true</c> if the current album is to be selected; otherwise, <c>false</c>.</value>
		public bool SelectCurrentAlbum
		{
			get { return _selectCurrentAlbum; }
			set { _selectCurrentAlbum = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating the top level album to render. When not specified, the highest level album for which the current
		/// user has permission to view is shown.
		/// </summary>
		/// <value>The top level album to render.</value>
		public int RootAlbumId
		{
			get { return _rootAlbumId; }
			set { _rootAlbumId = value; }
		}

		#endregion

		#region Private Methods

		private void ConfigureControlsFirstTime()
		{
			tvUC.ShowCheckbox = ShowCheckbox;
			tvUC.RequiredSecurityPermissions = Business.SecurityActions.ViewAlbumOrMediaObject;

			if (!String.IsNullOrEmpty(GalleryControl.TreeViewNavigateUrl))
			{
				tvUC.NavigateUrl = GalleryControl.TreeViewNavigateUrl;
			}

			if (RootAlbumId > int.MinValue)
			{
				tvUC.RootAlbumId = RootAlbumId;
			}

			int albumId = GetAlbumId();
			if (albumId > int.MinValue)
			{
				tvUC.BindTreeView(GetAlbum());
			}
			else
			{
				tvUC.BindTreeView();
			}
		}

		#endregion

	}
}