using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.ErrorHandler.CustomExceptions;
using GalleryServerPro.Web.Controller;
using GalleryServerPro.WebControls;

namespace GalleryServerPro.Web.Pages.Admin
{
	/// <summary>
	/// A page-like user control for administering user settings.
	/// </summary>
	public partial class usersettings : Pages.AdminPage
	{
		private List<String> _defaultRolesForNewUsers;
		private List<String> _usersToNotifyForNewAccounts;

		#region Properties

		/// <summary>
		/// Gets or sets a comma-delimited list of roles to assign when a user registers a new account. The text property of the
		/// cboUserRoles ComboBox binds to this property rather than <see cref="IGallerySettings.DefaultRolesForSelfRegisteredUser" />
		/// because it needs a string and the other property is a string array. This property essentially acts as a type 
		/// converter to the "real" property in <see cref="IGallerySettings" />. DotNetNuke only: The names are returned without 
		/// the _{GalleryId} suffix.
		/// </summary>
		/// <value>The default roles for self registered users.</value>
		public string DefaultRolesForSelfRegisteredUser
		{
			get
			{
				return String.Join(", ", RoleController.ParseRoleNameFromGspRoleNames(GallerySettingsUpdateable.DefaultRolesForSelfRegisteredUser));
			}
			set
			{
				string[] roleNames = String.IsNullOrEmpty(value) ? new string[]{} : value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

				// Trim any leading and trailing spaces
				for (int i = 0; i < roleNames.Length; i++)
				{
					roleNames[i] = RoleController.MakeRoleNameUnique(roleNames[i].Trim());
				}

				GallerySettingsUpdateable.DefaultRolesForSelfRegisteredUser = roleNames;
			}
		}

		/// <summary>
		/// Gets a list of roles to assign when a user registers a new account. The value is a collection that is parsed from the comma-delimited string 
		/// stored in the DefaultRolesForSelfRegisteredUser configuration setting. During postbacks the value is retrieved from the combobox.
		/// DotNetNuke only: The names include the _{GalleryId} suffix.
		/// </summary>
		/// <value>The default roles for self registered users.</value>
		private List<String> DefaultRolesForSelfRegisteredUserCollection
		{
			get
			{
				if (this._defaultRolesForNewUsers == null)
				{
					string[] defaultRoles;

					if (IsPostBack)
					{
						defaultRoles = cboUserRoles.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
					}
					else
					{
						defaultRoles = GallerySettingsUpdateable.DefaultRolesForSelfRegisteredUser;
					}

					this._defaultRolesForNewUsers = new List<string>();

					foreach (string roleName in defaultRoles)
					{
						this._defaultRolesForNewUsers.Add(RoleController.MakeRoleNameUnique(roleName.Trim()));
					}
				}

				return this._defaultRolesForNewUsers;
			}
		}

		/// <summary>
		/// Gets or sets a comma-delimited list of account names of users to receive an e-mail notification when an account is 
		/// created. The text property of the cboUsersToNotify ComboBox binds to this property rather than 
		/// <see cref="IGallerySettings.UsersToNotifyWhenAccountIsCreated" /> because it needs a string and the other property 
		/// is a string array. This property essentially acts as a type converter to the "real" property in <see cref="IGallerySettings" />.
		/// </summary>
		public string UsersToNotifyWhenAccountIsCreated
		{
			get
			{
				return String.Join(", ", GallerySettingsUpdateable.UsersToNotifyWhenAccountIsCreated.GetUserNames());
			}
			set
			{
				string[] userNames = String.IsNullOrEmpty(value) ? new string[] { } : value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

				GallerySettingsUpdateable.UsersToNotifyWhenAccountIsCreated.Clear();

				foreach (string userName in userNames)
				{
					IUserAccount user = UserController.GetUser(userName.Trim(), false);

					if (user != null)
					{
						GallerySettingsUpdateable.UsersToNotifyWhenAccountIsCreated.Add(user);
					}
				}
			}
		}

		/// <summary>
		/// Gets or sets a comma-delimited list of valid HTML tags. The text property of the
		/// txtAllowedHtmlTags TextBox binds to this property rather than <see cref="IGallerySettings.AllowedHtmlTags" />
		/// because it needs a string and the other property is a string array. This property essentially acts as a type 
		/// converter to the "real" property in <see cref="IGallerySettings" />.
		/// </summary>
		public string AllowedHtmlTags
		{
			get
			{
				return String.Join(", ", GallerySettingsUpdateable.AllowedHtmlTags);
			}
			set
			{
				string[] allowedTags = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

				// Trim any leading and trailing spaces
				for (int i = 0; i < allowedTags.Length; i++)
				{
					allowedTags[i] = allowedTags[i].Trim();
				}

				GallerySettingsUpdateable.AllowedHtmlTags = allowedTags;
			}
		}

		/// <summary>
		/// Gets or sets a comma-delimited list of valid HTML attributes. The text property of the
		/// txtAllowedHtmlAttributes TextBox binds to this property rather than <see cref="IGallerySettings.AllowedHtmlAttributes" />
		/// because it needs a string and the other property is a string array. This property essentially acts as a type 
		/// converter to the "real" property in <see cref="IGallerySettings" />.
		/// </summary>
		public string AllowedHtmlAttributes
		{
			get
			{
				return String.Join(", ", GallerySettingsUpdateable.AllowedHtmlAttributes);
			}
			set
			{
				string[] allowedAtts = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

				// Trim any leading and trailing spaces
				for (int i = 0; i < allowedAtts.Length; i++)
				{
					allowedAtts[i] = allowedAtts[i].Trim();
				}


				GallerySettingsUpdateable.AllowedHtmlAttributes = allowedAtts;
			}
		}

		/// <summary>
		/// Gets the list of user names of accounts to notify when an account is created. The value is a collection that is 
		/// parsed from the comma-delimited string stored in the UsersToNotifyWhenAccountIsCreated configuration setting.
		/// During postbacks the value is retrieved from the combobox.
		/// </summary>
		/// <value>The list of user names of accounts to notify when an account is created.</value>
		private List<String> UsersToNotifyForNewAccounts
		{
			get
			{
				if (this._usersToNotifyForNewAccounts == null)
				{
					this._usersToNotifyForNewAccounts = new List<string>();

					if (IsPostBack)
					{
						string[] usersToNotify = cboUsersToNotify.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

						foreach (string userName in usersToNotify)
						{
							this._usersToNotifyForNewAccounts.Add(userName.Trim());
						}
					}
					else
					{
						foreach (IUserAccount user in GallerySettingsUpdateable.UsersToNotifyWhenAccountIsCreated)
						{
							this._usersToNotifyForNewAccounts.Add(user.UserName);
						}
					}
				}

				return this._usersToNotifyForNewAccounts;
			}
		}

		#endregion

		#region Protected Events

		/// <summary>
		/// Handles the Init event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Init(object sender, EventArgs e)
		{
			this.AdminHeaderPlaceHolder = phAdminHeader;
			this.AdminFooterPlaceHolder = phAdminFooter;
		}

		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			this.CheckUserSecurity(SecurityActions.AdministerSite | SecurityActions.AdministerGallery);

			if (!IsPostBack)
			{
				ConfigureControlsFirstTime();
			}

			ConfigureControlsEveryTime();
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

			ConfigureUsersToNotifyComboBoxFirstTime();

			BindDefaultRolesComboBox();

			return true;
		}

		/// <summary>
		/// Handles the OnAfterBindControl event of the wwDataBinder control.
		/// </summary>
		/// <param name="item">The wwDataBindingItem item.</param>
		protected void wwDataBinder_AfterBindControl(GalleryServerPro.WebControls.wwDataBindingItem item)
		{
			// We need to HTML decode the role name that appears in the combo box
			if (item.ControlId == cboUserRoles.ID)
			{
				cboUserRoles.Text = Utils.HtmlDecode(cboUserRoles.Text);
			}
		}

		/// <summary>
		/// Handles the OnBeforeUnBindControl event of the wwDataBinder control.
		/// </summary>
		/// <param name="item">The wwDataBindingItem item.</param>
		protected bool wwDataBinder_BeforeUnbindControl(WebControls.wwDataBindingItem item)
		{
			if (!BeforeUnbind_ProcessEnableSelfRegistrationControls(item))
				return false;

			if (!BeforeUnbind_ProcessEnableUserAlbumsControls(item))
				return false;

			if (!BeforeUnbind_ProcessUserAccountControls(item))
				return false;

			return true;
		}

		/// <summary>
		/// Handles the Click event of the btnEnableUserAlbums control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void btnEnableUserAlbums_Click(object sender, EventArgs e)
		{
			TurnOnUserAlbumsForAllUsers();

			this.wwMessage.CssClass = "wwErrorSuccess gsp_msgfriendly gsp_bold";
			this.wwMessage.ShowMessage(Resources.GalleryServerPro.Admin_Save_Success_Text);
		}

		/// <summary>
		/// Handles the Click event of the btnDisableUserAlbums control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void btnDisableUserAlbums_Click(object sender, EventArgs e)
		{
			TurnOffUserAlbumsForAllUsers();

			this.wwMessage.CssClass = "wwErrorSuccess gsp_msgfriendly gsp_bold";
			this.wwMessage.ShowMessage(Resources.GalleryServerPro.Admin_Save_Success_Text);
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Gets an URL to the specified album.
		/// </summary>
		/// <param name="albumId">The album ID.</param>
		/// <returns>Returns an URL to the specified album.</returns>
		protected static string GetAlbumUrl(int albumId)
		{
			return Utils.AddQueryStringParameter(Utils.GetCurrentPageUrl(), String.Concat("aid=", albumId));
		}

		#endregion

		#region Private Methods

		private void TurnOnUserAlbumsForAllUsers()
		{
			UpdateUserAlbumProfileSetting(true);
		}

		private void TurnOffUserAlbumsForAllUsers()
		{
			UpdateUserAlbumProfileSetting(false);
		}

		private void UpdateUserAlbumProfileSetting(bool enableUserAlbum)
		{
			HelperFunctions.BeginTransaction();

			try
			{
				foreach (IUserAccount user in UserController.GetAllUsers())
				{
					IUserProfile profile = ProfileController.GetProfile(user.UserName);

					profile.GetGalleryProfile(GalleryId).EnableUserAlbum = enableUserAlbum;

					ProfileController.SaveProfile(profile);
				}
				HelperFunctions.CommitTransaction();
				HelperFunctions.PurgeCache();
			}
			catch
			{
				HelperFunctions.RollbackTransaction();
				throw;
			}
		}

		private bool BeforeUnbind_ProcessUserAccountControls(wwDataBindingItem item)
		{
			// When allow HTML is unchecked, several child items are disabled via javascript. Disabled HTML items are not
			// posted during a postback, so we don't have accurate information about their states. For these controls don't save
			// anything by returning false. Furthermore, to prevent these child controls from incorrectly reverting to an
			// empty or unchecked state in the UI, assign their properties to their config setting. 

			// Step 1: Handle the "allow HTML" checkbox
			if (!chkAllowHtml.Checked)
			{
				if (item.ControlId == txtAllowedHtmlTags.ID)
				{
					txtAllowedHtmlTags.Text = AllowedHtmlTags;
					return false;
				}

				if (item.ControlId == txtAllowedHtmlAttributes.ID)
				{
					txtAllowedHtmlAttributes.Text = AllowedHtmlAttributes;
					return false;
				}
			}
			else
			{
				// User may have hit Return while editing one of the textboxes. Remove any return characters to be safe.
				if (item.ControlId == txtAllowedHtmlTags.ID)
				{
					txtAllowedHtmlTags.Text = txtAllowedHtmlTags.Text.Replace("\r\n", String.Empty);
				}

				if (item.ControlId == txtAllowedHtmlAttributes.ID)
				{
					txtAllowedHtmlAttributes.Text = txtAllowedHtmlAttributes.Text.Replace("\r\n", String.Empty);
				}
			}

			// Step 2: Handle the "allow user account management" checkbox
			if (!this.chkAllowManageAccount.Checked)
			{
				if (item.ControlId == this.chkAllowDeleteOwnAccount.ID)
				{
					this.chkAllowDeleteOwnAccount.Checked = GallerySettingsUpdateable.AllowDeleteOwnAccount;
					return false;
				}
			}

			return true;
		}

		private bool BeforeUnbind_ProcessEnableSelfRegistrationControls(wwDataBindingItem item)
		{
			if (!this.chkEnableSelfRegistration.Checked)
			{
				// When self registration is unchecked, several child items are disabled via javascript. Disabled HTML items are not
				// posted during a postback, so we don't have accurate information about their states. For these controls don't save
				// anything by returning false. Furthermore, to prevent these child controls from incorrectly reverting to an
				// empty or unchecked state in the UI, assign their properties to their config setting. 
				if (item.ControlId == this.chkRequireEmailValidation.ID)
				{
					this.chkRequireEmailValidation.Checked = GallerySettings.RequireEmailValidationForSelfRegisteredUser;
					return false;
				}

				if (item.ControlId == this.chkRequireAdminApproval.ID)
				{
					this.chkRequireAdminApproval.Checked = GallerySettings.RequireApprovalForSelfRegisteredUser;
					return false;
				}

				if (item.ControlId == this.chkUseEmailForAccountName.ID)
				{
					this.chkUseEmailForAccountName.Checked = GallerySettings.UseEmailForAccountName;
					return false;
				}
			}

			return true;
		}

		private bool BeforeUnbind_ProcessEnableUserAlbumsControls(wwDataBindingItem item)
		{
			if (!this.chkEnableUserAlbums.Checked)
			{
				// When user albums is unchecked, several child items are disabled via javascript. Disabled HTML items are not
				// posted during a postback, so we don't have accurate information about their states. For these controls don't save
				// anything by returning false. Furthermore, to prevent these child controls from incorrectly reverting to an
				// empty or unchecked state in the UI, assign their properties to their config setting. 
				if (item.ControlId == this.chkRedirectAfterLogin.ID)
				{
					this.chkRedirectAfterLogin.Checked = GallerySettings.RedirectToUserAlbumAfterLogin;
					return false;
				}

				if (item.ControlId == this.txtAlbumNameTemplate.ID)
				{
					this.txtAlbumNameTemplate.Text = GallerySettings.UserAlbumNameTemplate;
					return false;
				}

				if (item.ControlId == this.txtAlbumSummaryTemplate.ID)
				{
					this.txtAlbumSummaryTemplate.Text = GallerySettings.UserAlbumSummaryTemplate;
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Handles the OnValidateControl event of the wwDataBinder control.
		/// </summary>
		/// <param name="item">The wwDataBindingItem item.</param>
		/// <returns>Returns <c>true</c> if the item is valid; otherwise returns <c>false</c>.</returns>
		protected bool wwDataBinder_ValidateControl(WebControls.wwDataBindingItem item)
		{
			if (!ValidateUserCanEnableSelfRegistration(item))
				return false;

			if (!ValidateDefaultRolesForSelfRegisteredUser(item))
				return false;

			if (!ValidateUsersToNotifyWhenAccountIsCreated(item))
				return false;

			if (!ValidateUserAlbums(item))
				return false;

			return true;
		}

		private bool ValidateUserCanEnableSelfRegistration(wwDataBindingItem item)
		{
			if ((item.ControlInstance == this.chkEnableSelfRegistration))
			{
				if (!UserCanEditUsersAndRoles)
				{
					item.BindingErrorMessage = Resources.GalleryServerPro.Admin_User_Settings_Cannot_Enable_Self_Registration_Msg;
					return false;
				}
			}

			return true;
		}

		private bool ValidateDefaultRolesForSelfRegisteredUser(wwDataBindingItem item)
		{
			if ((item.ControlInstance == this.cboUserRoles))
			{
				string roleNames = String.Join(", ", GallerySettings.DefaultRolesForSelfRegisteredUser);

				if ((!this.cboUserRoles.Text.Equals(roleNames)))
				{
					// User has updated the list of default roles. Validate.
					if (!VerifyDefaultRolesForSelfRegisteredUserExist(item)) return false;

					if (!VerifyUserHasPermissionToAddUserToDefaultRolesForSelfRegisteredUser(item)) return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Verify the roles in the DefaultRolesForSelfRegisteredUser setting exist. Returns false if one or more do not exist. The error
		/// message is assigned to the <see cref="wwDataBindingItem.BindingErrorMessage" /> property of <paramref name="item" />.
		/// </summary>
		/// <param name="item">The data binding item.</param>
		/// <returns>Returns true if every role in the DefaultRolesForSelfRegisteredUser setting exists; otherwise false.</returns>
		private bool VerifyDefaultRolesForSelfRegisteredUserExist(wwDataBindingItem item)
		{
			foreach (string roleName in GallerySettingsUpdateable.DefaultRolesForSelfRegisteredUser)
			{
				if (!RoleController.RoleExists(Utils.HtmlDecode(roleName.Trim())))
				{
					item.BindingErrorMessage = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.Admin_User_Settings_Invalid_Role_Name_Msg, roleName);
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Verifies the logged on user has permission to add users to the roles defined in the DefaultRolesForSelfRegisteredUser setting.
		/// </summary>
		/// <param name="item">The data binding item.</param>
		/// <returns>Returns true if logged on user has permission to add users to the roles defined in the DefaultRolesForSelfRegisteredUser setting;
		/// otherwise false.</returns>
		private bool VerifyUserHasPermissionToAddUserToDefaultRolesForSelfRegisteredUser(wwDataBindingItem item)
		{
			IUserAccount sampleNewUser = new UserAccount(Guid.NewGuid().ToString());
			try
			{
				UserController.ValidateLoggedOnUserHasPermissionToSaveUser(sampleNewUser, GallerySettingsUpdateable.DefaultRolesForSelfRegisteredUser, null);
			}
			catch (GallerySecurityException ex)
			{
				item.BindingErrorMessage = ex.Message;
				return false;
			}
			return true;
		}

		private bool ValidateUsersToNotifyWhenAccountIsCreated(wwDataBindingItem item)
		{
			if ((item.ControlInstance == this.cboUsersToNotify))
			{
				string userNamesCurrent = String.Join(", ", GallerySettings.UsersToNotifyWhenAccountIsCreated.GetUserNames());

				if ((!this.cboUsersToNotify.Text.Equals(userNamesCurrent)))
				{
					// User has updated the list of users to notify. Make sure they represent valid user account names.
					foreach (string userName in this.cboUsersToNotify.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
					{
						if (UserController.GetUser(Utils.HtmlDecode(userName.Trim()), false) == null)
						{
							item.BindingErrorMessage = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.Admin_User_Settings_Invalid_User_Name_Msg, userName);
							return false;
						}
					}
				}
			}
			return true;
		}

		private bool ValidateUserAlbums(wwDataBindingItem item)
		{
			if ((item.ControlInstance == this.chkEnableUserAlbums) && (chkEnableUserAlbums.Checked))
			{
				// User albums are selected. Make sure this isn't a read-only gallery.
				if (GallerySettings.MediaObjectPathIsReadOnly)
				{
					item.BindingErrorMessage = Resources.GalleryServerPro.Admin_User_Settings_Cannot_Enable_User_Albums_In_Read_Only_Gallery;
					return false;
				}
			}

			if ((item.ControlInstance == this.cboUserAlbumParent) && (chkEnableUserAlbums.Checked))
			{
				// User albums are selected. Make sure an album has been chosen to serve as the container for the user albums.
				int albumId;

				if ((tvUC.SelectedNode != null) && (Int32.TryParse(tvUC.SelectedNode.Value, out albumId)))
				{
					return true;
				}
				else
				{
					item.BindingErrorMessage = Resources.GalleryServerPro.Admin_User_Settings_Invalid_UserAlbumParent_Msg;
					return false;
				}
			}

			return true;
		}

		private void ConfigureControlsEveryTime()
		{
			this.PageTitle = Resources.GalleryServerPro.Admin_User_Settings_Page_Header;
			lblGalleryDescription.Text = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServerPro.Admin_Gallery_Description_Label, Utils.GetCurrentPageUrl(), Utils.HtmlEncode(Factory.LoadGallery(GalleryId).Description));
		}

		private void ConfigureControlsFirstTime()
		{
			AdminPageTitle = Resources.GalleryServerPro.Admin_User_Settings_Page_Header;

			if (AppSetting.Instance.License.IsInReducedFunctionalityMode)
			{
				wwMessage.ShowMessage(Resources.GalleryServerPro.Admin_Need_Product_Key_Msg2);
				wwMessage.CssClass = "wwErrorSuccess gsp_msgwarning";
				OkButtonBottom.Enabled = false;
				OkButtonTop.Enabled = false;
			}

			this.wwDataBinder.DataBind();

			ConfigureSelfRegistrationSection();

			ConfigureComboBoxesFirstTime();

			ConfigureOrphanUserAlbums();
		}

		private void ConfigureSelfRegistrationSection()
		{
			// Current user is a gallery admin and the setting to allow managing users/roles is disabled, so disable the checkbox
			// that allows self registration to be enabled, since we can't allow this user to do anything that creates a user.
			chkEnableSelfRegistration.Enabled = UserCanEditUsersAndRoles;
		}

		private void ConfigureOrphanUserAlbums()
		{
			// Check for user albums in the user album container that do not belong to a user. If we find some, display a message and give the admin the 
			// opportunity to delete them. Orphaned user albums might occur if an administrator has deleted a user outside of GSP.

			if (!GallerySettingsUpdateable.EnableUserAlbum)
			{
				pnlOrphanUserAlbums.Visible = false;
				return;
			}

			List<IAlbum> orphanUserAlbums = GetOrphanUserAlbums();

			if (orphanUserAlbums.Count > 0)
			{
				string userAlbumParentTitle = Utils.RemoveHtmlTags(AlbumController.LoadAlbumInstance(GallerySettingsUpdateable.UserAlbumParentAlbumId, false).Title);

				if (orphanUserAlbums.Count > 1)
					lblOrphanUserAlbumsMsg.Text = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.Admin_User_Settings_Orphan_User_Albums_Many_Lbl, orphanUserAlbums.Count, userAlbumParentTitle);
				else
					lblOrphanUserAlbumsMsg.Text = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.Admin_User_Settings_Orphan_User_Albums_One_Lbl, orphanUserAlbums.Count, userAlbumParentTitle);

				rptrOrphanUserAlbums.DataSource = orphanUserAlbums;
				rptrOrphanUserAlbums.DataBind();

				pnlOrphanUserAlbums.Visible = true;
			}
			else
			{
				pnlOrphanUserAlbums.Visible = false;
			}
		}

		private List<IAlbum> GetOrphanUserAlbums()
		{
			// Get a list of all the albums in the user album container that do not belong to a user.
			List<int> userAlbumIds = new List<int>();
			List<IAlbum> orphanUserAlbums = new List<IAlbum>();

			// Step 1: Get list of user album ID's.
			foreach (UserAccount user in UserController.GetAllUsers())
			{
				int userAlbumId = UserController.GetUserAlbumId(user.UserName, GalleryId);
				if (userAlbumId > int.MinValue)
				{
					userAlbumIds.Add(userAlbumId);
				}
			}

			// Step 2: Loop through each album in the user album container and see if the album is in our list of user album IDs. If not, add
			// to our list of orpan user albums.
			int albumId = GallerySettingsUpdateable.UserAlbumParentAlbumId;
			IAlbum userAlbumParent = null;

			if (albumId > 0)
			{
				try
				{
					userAlbumParent = AlbumController.LoadAlbumInstance(albumId, false);

					foreach (IAlbum album in userAlbumParent.GetChildGalleryObjects(GalleryObjectType.Album))
					{
						if (!userAlbumIds.Contains(album.Id))
						{
							orphanUserAlbums.Add(album);
						}
					}
				}
				catch (InvalidAlbumException) { }
			}

			return orphanUserAlbums;
		}

		private void ConfigureComboBoxesFirstTime()
		{
			ConfigureUsersToNotifyComboBoxFirstTime();

			BindDefaultRolesComboBox();

			ConfigureUserAlbumParentComboBoxFirstTime();
		}

		private void ConfigureUsersToNotifyComboBoxFirstTime()
		{
			// Configure the USER LIST ComboBox.
			cboUsersToNotify.DropHoverImageUrl = Utils.GetUrl("/images/componentart/combobox/ddn-hover.png");
			cboUsersToNotify.DropImageUrl = Utils.GetUrl("/images/componentart/combobox/ddn.png");
			cboUsersToNotify.Text = String.Join(", ", this.UsersToNotifyForNewAccounts.ToArray()).TrimEnd(new char[] { ' ', ',' });

			// Add the users to the list, pre-selecting any that are specified in the config file
			List<ListItem> userListItems = new List<ListItem>();

			foreach (IUserAccount user in GetUsersCurrentUserCanView())
			{
				userListItems.Add(new ListItem(Utils.HtmlEncode(user.UserName), Utils.HtmlEncode(user.UserName)));

				if (this.UsersToNotifyForNewAccounts.Contains(user.UserName))
				{
					userListItems[userListItems.Count - 1].Selected = true;
				}

				if (!this.UsersWithAdminPermission.Contains(user.UserName))
				{
					userListItems[userListItems.Count - 1].Attributes["class"] = "gsp_j_notadmin";
				}

			}
			cblU.Items.Clear();
			cblU.Items.AddRange(userListItems.ToArray());
		}

		private void BindDefaultRolesComboBox()
		{
			// Configure the ROLE LIST ComboBox.
			cboUserRoles.DropHoverImageUrl = Utils.GetUrl("/images/componentart/combobox/ddn-hover.png");
			cboUserRoles.DropImageUrl = Utils.GetUrl("/images/componentart/combobox/ddn.png");
			cboUserRoles.Text = DefaultRolesForSelfRegisteredUser;

			// Add the roles to the list, pre-selecting any that are specified in the config file
			List<ListItem> roleListItems = new List<ListItem>();
			foreach (IGalleryServerRole role in GetRolesCurrentUserCanView())
			{
				roleListItems.Add(new ListItem(RoleController.ParseRoleNameFromGspRoleName(Utils.HtmlEncode(role.RoleName)), Utils.HtmlEncode(role.RoleName)));

				if (this.DefaultRolesForSelfRegisteredUserCollection.Contains(role.RoleName))
				{
					roleListItems[roleListItems.Count - 1].Selected = true;
				}

				if (RoleController.IsRoleAnAlbumOwnerRole(role.RoleName) || RoleController.IsRoleAnAlbumOwnerTemplateRole(role.RoleName))
				{
					roleListItems[roleListItems.Count - 1].Attributes["class"] = "gsp_j_albumownerrole";
				}

			}
			cblR.Items.Clear();
			cblR.Items.AddRange(roleListItems.ToArray());
		}

		private void ConfigureUserAlbumParentComboBoxFirstTime()
		{
			// Configure the album treeview ComboBox.
			this.cboUserAlbumParent.DropHoverImageUrl = Utils.GetUrl("/images/componentart/combobox/ddn-hover.png");
			this.cboUserAlbumParent.DropImageUrl = Utils.GetUrl("/images/componentart/combobox/ddn.png");
			this.tvUC.RequiredSecurityPermissions = SecurityActions.AdministerSite | SecurityActions.AdministerGallery;

			string cboText;
			int albumId = GallerySettingsUpdateable.UserAlbumParentAlbumId;
			IAlbum albumToSelect = null;
			if (albumId > 0)
			{
				try
				{
					albumToSelect = AlbumController.LoadAlbumInstance(albumId, false);
					cboText = albumToSelect.Title;
					this.tvUC.BindTreeView(albumToSelect);
				}
				catch (InvalidAlbumException)
				{
					cboText = Resources.GalleryServerPro.Admin_User_Settings_User_Album_Parent_Is_Invalid_Text;
					this.tvUC.BindTreeView();
				}
			}
			else
			{
				this.tvUC.BindTreeView();
				cboText = Resources.GalleryServerPro.Admin_User_Settings_User_Album_Parent_Not_Assigned_Text;
			}

			this.cboUserAlbumParent.Text = cboText;
		}

		private void SaveSettings()
		{
			// Step 1: Update config manually with those items that are not managed via the wwDataBinder
			int albumId;

			if ((tvUC.SelectedNode != null) && (Int32.TryParse(tvUC.SelectedNode.Value, out albumId)))
			{
				GallerySettingsUpdateable.UserAlbumParentAlbumId = albumId;
			}

			// Step 2: Save
			this.wwDataBinder.Unbind(this);

			if (wwDataBinder.BindingErrors.Count > 0)
			{
				this.wwMessage.CssClass = "wwErrorFailure gsp_msgwarning";
				this.wwMessage.Text = wwDataBinder.BindingErrors.ToHtml();
				return;
			}

			GallerySettingsUpdateable.Save();

			ConfigureOrphanUserAlbums();

			this.wwMessage.CssClass = "wwErrorSuccess gsp_msgfriendly gsp_bold";
			this.wwMessage.ShowMessage(Resources.GalleryServerPro.Admin_Save_Success_Text);
		}

		#endregion
	}
}