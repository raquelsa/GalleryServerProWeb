using System;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.ErrorHandler.CustomExceptions;
using GalleryServerPro.Web.Controller;

namespace GalleryServerPro.Web.Pages.Admin
{
	/// <summary>
	/// A page-like user control for administering gallery control settings.
	/// </summary>
	public partial class gallerycontrolsettings : Pages.AdminPage
	{
		#region Private Fields

		private string _messageText;
		private string _messageCssClass;
		private bool _unbindError;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the text to display to the user.
		/// </summary>
		/// <value>The text to display to the user.</value>
		private string MessageText
		{
			get { return _messageText; }
			set { _messageText = value; }
		}

		/// <summary>
		/// Gets or sets the CSS class to use to format the text to display to the user.
		/// </summary>
		/// <value>The CSS class to use to format the text to display to the user.</value>
		private string MessageCssClass
		{
			get { return _messageCssClass; }
			set { _messageCssClass = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether an error occurred while preparing the data to save. When setting this to <c>true</c>, 
		/// one should also set the <see cref="MessageText" /> property.
		/// </summary>
		/// <value><c>true</c> if an error is preventing the data from being saved; otherwise, <c>false</c>.</value>
		private bool UnbindError
		{
			get { return _unbindError; }
			set { _unbindError = value; }
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			CheckUserSecurity(SecurityActions.AdministerSite | SecurityActions.AdministerGallery);

			ConfigureControlsEveryTime();

			if (!IsPostBack)
			{
				ConfigureControlsFirstTime();
			}
		}

		/// <summary>
		/// Handles the Init event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Init(object sender, EventArgs e)
		{
			AdminHeaderPlaceHolder = phAdminHeader;
			AdminFooterPlaceHolder = phAdminFooter;
		}

		/// <summary>
		/// Determines whether the event for the server control is passed up the page's UI server control hierarchy.
		/// </summary>
		/// <param name="source">The source of the event.</param>
		/// <param name="args">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
		/// <returns>
		/// true if the event has been canceled; otherwise, false. The default is false.
		/// </returns>
		protected override bool OnBubbleEvent(object source, EventArgs args)
		{
			//An event from the control has bubbled up.  If it's the Ok button, then run the
			//code to save the data to the database; otherwise ignore.
			Button btn = source as Button;
			if ((btn != null) && (((btn.ID == "btnOkTop") || (btn.ID == "btnOkBottom"))))
			{
				SaveSettings();
			}

			return true;
		}

		#endregion

		#region Private Methods

		private void ConfigureControlsEveryTime()
		{
			PageTitle = Resources.GalleryServerPro.Admin_Gallery_Control_Settings_Page_Header;
			lblGalleryDescription.Text = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServerPro.Admin_Gallery_Control_Description_Label, GalleryControl.ClientID, Utils.GetCurrentPageUrl());
		}

		private void ConfigureControlsFirstTime()
		{
			AdminPageTitle = Resources.GalleryServerPro.Admin_Gallery_Control_Settings_Page_Header;

			rbDefaultGallery.Text = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServerPro.Admin_Gallery_Control_Settings_Default_Gallery_Label,
																						Utils.HtmlEncode(Factory.LoadGallery(GalleryId).Description));

			CheckForMessages();

			if (AppSetting.Instance.License.IsInReducedFunctionalityMode)
			{
				wwMessage.ShowMessage(Resources.GalleryServerPro.Admin_Need_Product_Key_Msg2);
				wwMessage.CssClass = "wwErrorSuccess gsp_msgwarning";
				OkButtonBottom.Enabled = false;
				OkButtonTop.Enabled = false;
			}

			DataBindControls();
		}

		/// <summary>
		/// Determine if there are any messages we need to display to the user.
		/// </summary>
		private void CheckForMessages()
		{
			if (Message == Message.SettingsSuccessfullyChanged)
			{
				MessageText = Resources.GalleryServerPro.Admin_Save_Success_Text;
			}

			if (GalleryControl.GalleryControlSettings.MediaObjectId.HasValue)
			{
				try
				{
					Factory.LoadMediaObjectInstance(GalleryControl.GalleryControlSettings.MediaObjectId.Value);
				}
				catch (InvalidMediaObjectException)
				{
					MessageCssClass = "wwErrorFailure gsp_msgwarning";
					MessageText = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.Admin_Gallery_Control_Settings_Invalid_MediaObject_Msg, GalleryControl.GalleryControlSettings.MediaObjectId.Value);
				}
			}
		}

		private void DataBindControls()
		{
			DataBindViewMode();

			DataBindDefaultGalleryObject();

			DataBindBehavior();

			UpdateUI();
		}

		private void DataBindViewMode()
		{
			switch (GalleryControl.ViewMode)
			{
				case ViewMode.Multiple:
					rbViewModeMultiple.Checked = true;
					break;
				case ViewMode.Single:
					rbViewModeSingle.Checked = true;
					break;
				case ViewMode.TreeView:
					rbViewModeTreeview.Checked = true;
					break;
			}

			txtTreeviewNavigateUrl.Text = GalleryControl.TreeViewNavigateUrl;
		}

		private void DataBindDefaultGalleryObject()
		{
			if (GalleryControl.GalleryControlSettings.AlbumId.HasValue)
			{
				rbDefaultAlbum.Checked = true;
			}
			else if (GalleryControl.GalleryControlSettings.MediaObjectId.HasValue)
			{
				rbDefaultMediaObject.Checked = true;
				txtDefaultMediaObjectId.Text = GalleryControl.GalleryControlSettings.MediaObjectId.Value.ToString(CultureInfo.InvariantCulture);
			}
			else
			{
				rbDefaultGallery.Checked = true;
			}

			ConfigureDefaultAlbumComboBoxFirstTime();
		}

		private void DataBindBehavior()
		{
			IGalleryControlSettings controlSettings = GalleryControl.GalleryControlSettings;

			chkAllowUrlOverride.Checked = (!controlSettings.AllowUrlOverride.HasValue || controlSettings.AllowUrlOverride.Value);

			DataBindOverridableSettings();

			SetOverrideCheckbox();
		}

		/// <summary>
		/// Databind the control settings that override matching gallery-level settings.
		/// </summary>
		private void DataBindOverridableSettings()
		{
			chkShowAlbumTreeViewForAlbum.Checked = ShowAlbumTreeViewForAlbum;
			chkShowAlbumTreeViewForMO.Checked = ShowAlbumTreeViewForMediaObject;
			chkShowHeader.Checked = ShowHeader;
			txtGalleryTitle.Text = GalleryTitle;
			txtGalleryTitleUrl.Text = GalleryTitleUrl;
			chkShowLogin.Checked = ShowLogin;
			chkShowSearch.Checked = ShowSearch;
			chkAllowAnonBrowsing.Checked = AllowAnonymousBrowsing;
			chkShowActionMenu.Checked = ShowActionMenu;
			chkShowAlbumBreadcrumb.Checked = ShowAlbumBreadCrumb;
			chkShowMediaObjectNavigation.Checked = ShowMediaObjectNavigation;
			chkShowMediaObjectIndexPosition.Checked = ShowMediaObjectIndexPosition;
			chkShowMediaObjectTitle.Checked = ShowMediaObjectTitle;
			chkAutoPlaySlideshow.Checked = AutoPlaySlideShow;

			chkShowMediaObjectToolbar.Checked = ShowMediaObjectToolbar;

			chkShowMetadataButton.Checked = ShowMetadataButton;
			chkShowDownloadButton.Checked = ShowMediaObjectDownloadButton;
			chkShowDownloadZipButton.Checked = ShowMediaObjectZipDownloadButton;
			chkShowHighResButton.Checked = ShowHighResImageButton;
			chkShowPermalinkButton.Checked = ShowPermalinkButton;
			chkShowSlideShowButton.Checked = ShowSlideShowButton;
			chkShowMoveButton.Checked = ShowTransferMediaObjectButton;
			chkShowCopyButton.Checked = ShowCopyMediaObjectButton;
			chkShowRotateButton.Checked = ShowRotateMediaObjectButton;
			chkShowDeleteButton.Checked = ShowDeleteMediaObjectButton;
		}

		private void SetOverrideCheckbox()
		{
			IGalleryControlSettings settings = GalleryControl.GalleryControlSettings;
			bool areAnyGallerySettingsOverridden = (settings.ShowAlbumTreeViewForAlbum.HasValue || settings.ShowAlbumTreeViewForMediaObject.HasValue ||
																							settings.ShowHeader.HasValue || !String.IsNullOrEmpty(settings.GalleryTitle) ||
																							!String.IsNullOrEmpty(settings.GalleryTitleUrl) || settings.ShowLogin.HasValue || settings.ShowSearch.HasValue ||
																							settings.AllowAnonymousBrowsing.HasValue || settings.ShowActionMenu.HasValue || settings.ShowAlbumBreadCrumb.HasValue ||
																							settings.ShowMediaObjectNavigation.HasValue || settings.ShowMediaObjectIndexPosition.HasValue ||
																							settings.ShowMediaObjectTitle.HasValue || settings.AutoPlaySlideShow.HasValue ||
																							settings.ShowMediaObjectToolbar.HasValue || settings.ShowMetadataButton.HasValue ||
																							settings.ShowMediaObjectDownloadButton.HasValue || settings.ShowMediaObjectZipDownloadButton.HasValue ||
																							settings.ShowHighResImageButton.HasValue || settings.ShowPermalinkButton.HasValue ||
																							settings.ShowSlideShowButton.HasValue || settings.ShowTransferMediaObjectButton.HasValue ||
																							settings.ShowCopyMediaObjectButton.HasValue || settings.ShowRotateMediaObjectButton.HasValue ||
																							settings.ShowDeleteMediaObjectButton.HasValue
																						 );

			chkOverride.Checked = areAnyGallerySettingsOverridden;
		}

		private void ConfigureDefaultAlbumComboBoxFirstTime()
		{
			// Configure the album treeview ComboBox.
			cboDefaultAlbum.DropHoverImageUrl = Utils.GetUrl("/images/componentart/combobox/ddn-hover.png");
			cboDefaultAlbum.DropImageUrl = Utils.GetUrl("/images/componentart/combobox/ddn.png");
			tvUC.RequiredSecurityPermissions = SecurityActions.AdministerSite | SecurityActions.AdministerGallery;

			string cboText;
			int albumId = GalleryControl.GalleryControlSettings.AlbumId ?? 0;
			if (albumId > 0)
			{
				try
				{
					IAlbum albumToSelect = AlbumController.LoadAlbumInstance(albumId, false);
					cboText = albumToSelect.Title;
					tvUC.BindTreeView(albumToSelect);
				}
				catch (InvalidAlbumException)
				{
					cboText = Resources.GalleryServerPro.Admin_User_Settings_User_Album_Parent_Is_Invalid_Text;
					tvUC.BindTreeView();
				}
			}
			else
			{
				tvUC.BindTreeView();
				cboText = Resources.GalleryServerPro.Admin_User_Settings_User_Album_Parent_Not_Assigned_Text;
			}

			cboDefaultAlbum.Text = cboText;
		}

		private void SaveSettings()
		{
			UnbindViewMode();

			UnbindDefaultGalleryObject();

			UnbindBehaviorSettings();

			if (!UnbindError)
			{
				GalleryControlSettingsUpdateable.Save();

				Factory.ClearGalleryControlSettingsCache();

				// Since we are changing settings that affect how and which controls are rendered to the page, let us redirect to the current page and
				// show the save success message. If we simply show a message without redirecting, two things happen: (1) the user doesn't see the effect
				// of their change until the next page load, (2) there is the potential for a viewstate validation error
				const Message msg = Message.SettingsSuccessfullyChanged;

				Utils.Redirect(PageId.admin_gallerycontrolsettings, "aid={0}&msg={1}", GetAlbumId(), ((int)msg).ToString(CultureInfo.InvariantCulture));
			}

			DataBindControls();
		}

		private void UnbindViewMode()
		{
			if (rbViewModeMultiple.Checked && GalleryControl.GalleryControlSettings.ViewMode == ViewMode.NotSet)
			{
				// This setting remains at its default value, so don't set it.
				return;
			}

			if (rbViewModeMultiple.Checked)
			{
				GalleryControlSettingsUpdateable.ViewMode = ViewMode.Multiple;
				GalleryControlSettingsUpdateable.TreeViewNavigateUrl = null;
			}
			else if (rbViewModeSingle.Checked)
			{
				GalleryControlSettingsUpdateable.ViewMode = ViewMode.Single;
				GalleryControlSettingsUpdateable.TreeViewNavigateUrl = null;
			}
			else if (rbViewModeTreeview.Checked)
			{
				GalleryControlSettingsUpdateable.ViewMode = ViewMode.TreeView;
				GalleryControlSettingsUpdateable.TreeViewNavigateUrl = (String.IsNullOrEmpty(txtTreeviewNavigateUrl.Text) ? null : txtTreeviewNavigateUrl.Text);
			}
		}

		private void UnbindDefaultGalleryObject()
		{
			if (rbDefaultGallery.Checked)
			{
				GalleryControlSettingsUpdateable.AlbumId = null;
				GalleryControlSettingsUpdateable.MediaObjectId = null;
			}
			else if (rbDefaultAlbum.Checked)
			{
				int albumId;

				if ((tvUC.SelectedNode != null) && (Int32.TryParse(tvUC.SelectedNode.Value, out albumId)))
				{
					GalleryControlSettingsUpdateable.AlbumId = albumId;
					GalleryControlSettingsUpdateable.MediaObjectId = null;
				}
				else
				{
					UnbindError = true;
					MessageText = Resources.GalleryServerPro.Admin_Gallery_Control_Settings_InvalidAlbum_Msg;
				}
			}
			else if (rbDefaultMediaObject.Checked)
			{
				int mediaObjectId;
				if (Int32.TryParse(txtDefaultMediaObjectId.Text, out mediaObjectId))
				{
					try
					{
						Factory.LoadMediaObjectInstance(mediaObjectId);

						GalleryControlSettingsUpdateable.MediaObjectId = mediaObjectId;
						GalleryControlSettingsUpdateable.AlbumId = null;
					}
					catch (InvalidMediaObjectException)
					{
						UnbindError = true;
						MessageText = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.Admin_Gallery_Control_Settings_Invalid_MediaObject_Msg, mediaObjectId);
					}
				}
				else
				{
					UnbindError = true;
					MessageText = Resources.GalleryServerPro.Admin_Gallery_Control_Settings_InvalidMediaObject_Msg;
				}
			}
		}

		private void UnbindBehaviorSettings()
		{
			GalleryControlSettingsUpdateable.AllowUrlOverride = chkAllowUrlOverride.Checked;

			if (chkOverride.Checked)
			{
				UnbindOverridableSettings();
			}
			else
			{
				SetOverridableSettingsToNull();
			}
		}

		private void UnbindOverridableSettings()
		{
			GalleryControlSettingsUpdateable.ShowAlbumTreeViewForAlbum = chkShowAlbumTreeViewForAlbum.Checked;
			GalleryControlSettingsUpdateable.ShowAlbumTreeViewForMediaObject = chkShowAlbumTreeViewForMO.Checked;
			GalleryControlSettingsUpdateable.ShowHeader = chkShowHeader.Checked;
			GalleryControlSettingsUpdateable.GalleryTitle = txtGalleryTitle.Text;
			GalleryControlSettingsUpdateable.GalleryTitleUrl = txtGalleryTitleUrl.Text;

			if (chkShowHeader.Checked)
			{
				GalleryControlSettingsUpdateable.ShowLogin = chkShowLogin.Checked;
				GalleryControlSettingsUpdateable.ShowSearch = chkShowSearch.Checked;
			}

			GalleryControlSettingsUpdateable.AllowAnonymousBrowsing = chkAllowAnonBrowsing.Checked;
			GalleryControlSettingsUpdateable.ShowActionMenu = chkShowActionMenu.Checked;
			GalleryControlSettingsUpdateable.ShowAlbumBreadCrumb = chkShowAlbumBreadcrumb.Checked;
			GalleryControlSettingsUpdateable.ShowMediaObjectNavigation = chkShowMediaObjectNavigation.Checked;
			GalleryControlSettingsUpdateable.ShowMediaObjectIndexPosition = chkShowMediaObjectIndexPosition.Checked;
			GalleryControlSettingsUpdateable.ShowMediaObjectTitle = chkShowMediaObjectTitle.Checked;
			GalleryControlSettingsUpdateable.ShowMediaObjectToolbar = chkShowMediaObjectToolbar.Checked;
			GalleryControlSettingsUpdateable.AutoPlaySlideShow = chkAutoPlaySlideshow.Checked;

			if (chkShowMediaObjectToolbar.Checked)
			{
				GalleryControlSettingsUpdateable.ShowMetadataButton = chkShowMetadataButton.Checked;
				GalleryControlSettingsUpdateable.ShowMediaObjectDownloadButton = chkShowDownloadButton.Checked;
				GalleryControlSettingsUpdateable.ShowMediaObjectZipDownloadButton = chkShowDownloadZipButton.Checked;
				GalleryControlSettingsUpdateable.ShowHighResImageButton = chkShowHighResButton.Checked;
				GalleryControlSettingsUpdateable.ShowPermalinkButton = chkShowPermalinkButton.Checked;
				GalleryControlSettingsUpdateable.ShowSlideShowButton = chkShowSlideShowButton.Checked;
				GalleryControlSettingsUpdateable.ShowTransferMediaObjectButton = chkShowMoveButton.Checked;
				GalleryControlSettingsUpdateable.ShowCopyMediaObjectButton = chkShowCopyButton.Checked;
				GalleryControlSettingsUpdateable.ShowRotateMediaObjectButton = chkShowRotateButton.Checked;
				GalleryControlSettingsUpdateable.ShowDeleteMediaObjectButton = chkShowDeleteButton.Checked;
			}
		}

		private void SetOverridableSettingsToNull()
		{
			GalleryControlSettingsUpdateable.ShowAlbumTreeViewForAlbum = null;
			GalleryControlSettingsUpdateable.ShowAlbumTreeViewForMediaObject = null;
			GalleryControlSettingsUpdateable.ShowHeader = null;
			GalleryControlSettingsUpdateable.GalleryTitle = null;
			GalleryControlSettingsUpdateable.GalleryTitleUrl = null;
			GalleryControlSettingsUpdateable.ShowLogin = null;
			GalleryControlSettingsUpdateable.ShowSearch = null;
			GalleryControlSettingsUpdateable.AllowAnonymousBrowsing = null;
			GalleryControlSettingsUpdateable.ShowActionMenu = null;
			GalleryControlSettingsUpdateable.ShowAlbumBreadCrumb = null;
			GalleryControlSettingsUpdateable.ShowMediaObjectNavigation = null;
			GalleryControlSettingsUpdateable.ShowMediaObjectIndexPosition = null;
			GalleryControlSettingsUpdateable.ShowMediaObjectTitle = null;
			GalleryControlSettingsUpdateable.AutoPlaySlideShow = null;
			GalleryControlSettingsUpdateable.ShowMediaObjectToolbar = null;
			GalleryControlSettingsUpdateable.ShowMetadataButton = null;
			GalleryControlSettingsUpdateable.ShowMediaObjectDownloadButton = null;
			GalleryControlSettingsUpdateable.ShowMediaObjectZipDownloadButton = null;
			GalleryControlSettingsUpdateable.ShowHighResImageButton = null;
			GalleryControlSettingsUpdateable.ShowPermalinkButton = null;
			GalleryControlSettingsUpdateable.ShowSlideShowButton = null;
			GalleryControlSettingsUpdateable.ShowTransferMediaObjectButton = null;
			GalleryControlSettingsUpdateable.ShowCopyMediaObjectButton = null;
			GalleryControlSettingsUpdateable.ShowRotateMediaObjectButton = null;
			GalleryControlSettingsUpdateable.ShowDeleteMediaObjectButton = null;
		}

		private void UpdateUI()
		{
			if (!String.IsNullOrEmpty(MessageText))
			{
				if (UnbindError)
				{
					wwMessage.CssClass = "wwErrorFailure gsp_msgwarning";
					wwMessage.Text = this.MessageText;
				}
				else
				{
					wwMessage.CssClass = "wwErrorSuccess gsp_msgfriendly gsp_bold";
					wwMessage.ShowMessage(this.MessageText);
				}

				// If a CSS class has been specified, use that instead of the default ones above.
				if (!String.IsNullOrEmpty(MessageCssClass))
				{
					wwMessage.CssClass = MessageCssClass;
				}
			}
		}

		#endregion
	}
}