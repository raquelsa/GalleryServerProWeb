using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using ComponentArt.Web.UI;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.ErrorHandler.CustomExceptions;
using GalleryServerPro.Web.Controller;
using GalleryServerPro.Web.Entity;

namespace GalleryServerPro.Web.Pages.Admin
{
	/// <summary>
	/// A page-like user control for administering users.
	/// </summary>
	public partial class manageusers : Pages.AdminPage
	{
		private enum UserCallbackStatus
		{
			ShowEditUserSuccess,
			AddUserSaveSuccess,
			EditUserSaveSuccess,
			PasswordUpdated,
			UserUnlocked
		}

		#region Protected Events

		/// <summary>
		/// Handles the Init event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Init(object sender, EventArgs e)
		{
			// Set up triggers here. We can't do it declaratively because the UpdatePanel can't find the trigger control.
			AsyncPostBackTrigger triggerSaveUser = new AsyncPostBackTrigger();
			triggerSaveUser.ControlID = btnSave.UniqueID;
			triggerSaveUser.EventName = "Click";
			upEditUser.Triggers.Add(triggerSaveUser);

			AsyncPostBackTrigger triggerAddUser = new AsyncPostBackTrigger();
			triggerAddUser.ControlID = btnCreateUser.UniqueID;
			triggerAddUser.EventName = "Click";
			upAddUser.Triggers.Add(triggerAddUser);
		}

		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			// User must be a site or gallery admin.
			this.CheckUserSecurity(SecurityActions.AdministerSite | SecurityActions.AdministerGallery);

			ProcessQueryString();

			ConfigureControlsEveryTime();

			if (!ScriptManager.GetCurrent(this.Page).IsInAsyncPostBack)
			{
				ConfigureControlsFirstTime();
			}
		}

		protected void btnCreateUser_Click(object sender, EventArgs e)
		{
			try
			{
				AddUser();

				ScriptManager.GetCurrent(this.Page).RegisterDataItem(hdnCallbackStatus, UserCallbackStatus.AddUserSaveSuccess.ToString(), false);
			}
			catch (MembershipCreateUserException ex)
			{
				// Just in case we created the user and the exception occured at a later step, like adding the roles, delete the user,
				// but only if the user exists AND the error wasn't 'DuplicateUserName'.
				if ((ex.StatusCode != MembershipCreateStatus.DuplicateUserName) && (UserController.GetUser(txtNewUserUserName.Text, false) != null))
				{
					UserController.DeleteUser(txtNewUserEmail.Text);
				}

				ShowErrorInDialog(Resources.GalleryServerPro.Admin_Manage_Users_Cannot_Create_User_Msg, UserController.GetAddUserErrorMessage(ex.StatusCode), pnlAddUserMessage);
			}
			catch (Exception ex)
			{
				LogError(ex);

				// Just in case we created the user and the exception occured at a later step, like ading the roles, delete the user.
				if (UserController.GetUser(txtNewUserUserName.Text, false) != null)
				{
					UserController.DeleteUser(txtNewUserUserName.Text);
				}

				ShowErrorInDialog(Resources.GalleryServerPro.Admin_Manage_Users_Cannot_Create_User_Msg, ex.Message, pnlAddUserMessage);
			}
		}

		protected void lbEditUser_Click(object sender, EventArgs e)
		{
			try
			{
				PopulateControlsWithUserData(hdnUserName.Value, true);

				ScriptManager.GetCurrent(this.Page).RegisterDataItem(hdnCallbackStatus, UserCallbackStatus.ShowEditUserSuccess.ToString(), false);
			}
			catch (Exception ex)
			{
				LogError(ex);

				ShowErrorInDialog(ex, Resources.GalleryServerPro.Site_Error_Hdr, phEditUserMessage);
			}
		}

		protected void lbUnlockUser_Click(object sender, EventArgs e)
		{
			try
			{
				UnlockUser(hdnUserName.Value);

				ScriptManager.GetCurrent(this.Page).RegisterDataItem(hdnCallbackStatus, UserCallbackStatus.UserUnlocked.ToString(), false);
			}
			catch (Exception ex)
			{
				LogError(ex);

				ShowErrorInDialog(ex, Resources.GalleryServerPro.Site_Error_Hdr, phEditUserMessage);
			}
		}

		protected void btnSave_Click(object sender, EventArgs e)
		{
			try
			{
				SaveUser(hdnUserName.Value);

				ScriptManager.GetCurrent(this.Page).RegisterDataItem(hdnCallbackStatus, UserCallbackStatus.EditUserSaveSuccess.ToString(), false);
			}
			catch (Exception ex)
			{
				LogError(ex);

				ShowErrorInDialog(ex, Resources.GalleryServerPro.Admin_Manage_Users_Cannot_Save_User_Hdr, phEditUserMessage);
			}
		}

		protected void btnUpdatePassword_Click(object sender, EventArgs e)
		{
			try
			{
				string userName = hdnUserName.Value;

				UpdatePassword(userName);

				PopulateControlsWithUserData(userName, false);

				ScriptManager.GetCurrent(this.Page).RegisterDataItem(hdnCallbackStatus, UserCallbackStatus.PasswordUpdated.ToString(), false);
			}
			catch (Exception ex)
			{
				LogError(ex);

				ShowErrorInDialog(ex, Resources.GalleryServerPro.Admin_Manage_Users_Cannot_Save_User_Hdr, phEditUserMessage);
			}
		}

		/// <summary>
		/// Handles the DeleteCommand event of the gdUsers control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="ComponentArt.Web.UI.GridItemEventArgs"/> instance containing the event data.</param>
		protected void gdUsers_DeleteCommand(object sender, ComponentArt.Web.UI.GridItemEventArgs e)
		{
			// Remove the user from all roles, and then delete the user. If an exception is thrown, such as during
			// ValidateDeleteUser(), the client side function gdUsers_CallbackError will catch it and display
			// the message to the user.
			try
			{
				string userName = Utils.HtmlDecode(e.Item["UserName"].ToString());

				UserController.DeleteGalleryServerProUser(userName, true);

				BindUsersGrid();
			}
			catch (Exception ex)
			{
				LogError(ex);
				throw;
			}
		}

		/// <summary>
		/// Handles the Callback event of the cbValidateNewUserName control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="ComponentArt.Web.UI.CallBackEventArgs"/> instance containing the event data.</param>
		protected void cbValidateNewUserName_Callback(object sender, ComponentArt.Web.UI.CallBackEventArgs e)
		{
			// The user just typed in a user name in the Add New User wizard. Let's check to see if it already
			// exists and let the user know the result.
			string requestedUsername = e.Parameter;

			if (String.IsNullOrEmpty(requestedUsername))
			{
				lblUserNameValidationResult.Text = String.Empty;
			}
			else
			{
				IUserAccount user = UserController.GetUser(requestedUsername, false);

				bool userNameIsInUse = (user != null);

				if (userNameIsInUse)
				{
					lblUserNameValidationResult.Text = Resources.GalleryServerPro.Admin_Manage_Users_Username_Already_In_Use_Msg;
					lblUserNameValidationResult.CssClass = "gsp_msgwarning";
				}
				else
				{
					lblUserNameValidationResult.Text = Resources.GalleryServerPro.Admin_Manage_Users_Username_Already_Is_Valid_Msg;
					lblUserNameValidationResult.CssClass = "gsp_msgfriendly";
				}
			}

			lblUserNameValidationResult.RenderControl(e.Output);
		}

		/// <summary>
		/// Handles the DataBound event of the cblAvailableRolesForExistingUser control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void cblAvailableRolesForExistingUser_DataBound(object sender, EventArgs e)
		{
			foreach (ListItem item in cblAvailableRolesForExistingUser.Items)
			{
				// HTML encode the values.
				item.Text = RoleController.ParseRoleNameFromGspRoleName(Utils.HtmlEncode(item.Text));
				item.Value = Utils.HtmlEncode(item.Value);
			}
		}

		/// <summary>
		/// Handles the DataBound event of the cblAvailableRolesForNewUser control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void cblAvailableRolesForNewUser_DataBound(object sender, EventArgs e)
		{
			foreach (ListItem checkbox in cblAvailableRolesForNewUser.Items)
			{
				string roleName = RoleController.ParseRoleNameFromGspRoleName(checkbox.Text);

				// Mark all roles with a special class so that jQuery can show/hide them.
				if (RoleController.IsRoleAnAlbumOwnerRole(roleName) || RoleController.IsRoleAnAlbumOwnerTemplateRole(roleName))
				{
					checkbox.Attributes["class"] = "gsp_j_aaor";
				}

				// HTML encode the values.
				checkbox.Text = Utils.HtmlEncode(roleName);
				checkbox.Value = Utils.HtmlEncode(checkbox.Value);
			}
		}

		#endregion

		#region Public Properties

		#region Controls in dgEditUser

		/// <summary>
		/// Gets a reference to the control named pnlEditUserDialogContent.
		/// </summary>
		/// <value>The pnlEditUserDialogContent control.</value>
		public Panel pnlEditUserDialogContent
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditUser, "pnlEditUserDialogContent");
				if (ctrl == null) throw new WebException("Cannot find Panel pnlEditUserDialogContent.");
				return (Panel)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named tsEditUser.
		/// </summary>
		/// <value>The tsEditUser control.</value>
		public TabStrip tsEditUser
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditUser, "tsEditUser");
				if (ctrl == null) throw new WebException("Cannot find TabStrip tsEditUser.");
				return (TabStrip)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named phEditUserMessage.
		/// </summary>
		/// <value>The phEditUserMessage control.</value>
		public PlaceHolder phEditUserMessage
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditUser, "phEditUserMessage");
				if (ctrl == null) throw new WebException("Cannot find PlaceHolder phEditUserMessage.");
				return (PlaceHolder)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named phDialogMessagePasswordTab.
		/// </summary>
		/// <value>The phDialogMessagePasswordTab control.</value>
		public PlaceHolder phDialogMessagePasswordTab
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditUser, "phDialogMessagePasswordTab");
				if (ctrl == null) throw new WebException("Cannot find PlaceHolder phDialogMessagePasswordTab.");
				return (PlaceHolder)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named lblUserName.
		/// </summary>
		/// <value>The lblUserName control.</value>
		public Label lblUserName
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditUser, "lblUserName");
				if (ctrl == null) throw new WebException("Cannot find Label lblUserName.");
				return (Label)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named txtComment.
		/// </summary>
		/// <value>The txtComment control.</value>
		public TextBox txtComment
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditUser, "txtComment");
				if (ctrl == null) throw new WebException("Cannot find TextBox txtComment.");
				return (TextBox)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named txtEmail.
		/// </summary>
		/// <value>The txtEmail control.</value>
		public TextBox txtEmail
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditUser, "txtEmail");
				if (ctrl == null) throw new WebException("Cannot find TextBox txtEmail.");
				return (TextBox)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named rbApprovedYes.
		/// </summary>
		/// <value>The rbApprovedYes control.</value>
		public RadioButton rbApprovedYes
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditUser, "rbApprovedYes");
				if (ctrl == null) throw new WebException("Cannot find RadioButton rbApprovedYes.");
				return (RadioButton)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named rbApprovedNo.
		/// </summary>
		/// <value>The rbApprovedNo control.</value>
		public RadioButton rbApprovedNo
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditUser, "rbApprovedNo");
				if (ctrl == null) throw new WebException("Cannot find RadioButton rbApprovedNo.");
				return (RadioButton)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named trUserAlbumTableRow.
		/// </summary>
		/// <value>The trUserAlbumTableRow control.</value>
		public HtmlTableRow trUserAlbumTableRow
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditUser, "trUserAlbumTableRow");
				if (ctrl == null) throw new WebException("Cannot find HtmlTableRow trUserAlbumTableRow.");
				return (HtmlTableRow)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named rbUserAlbumYes.
		/// </summary>
		/// <value>The rbUserAlbumYes control.</value>
		public RadioButton rbUserAlbumYes
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditUser, "rbUserAlbumYes");
				if (ctrl == null) throw new WebException("Cannot find RadioButton rbUserAlbumYes.");
				return (RadioButton)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named rbUserAlbumNo.
		/// </summary>
		/// <value>The rbUserAlbumNo control.</value>
		public RadioButton rbUserAlbumNo
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditUser, "rbUserAlbumNo");
				if (ctrl == null) throw new WebException("Cannot find RadioButton rbUserAlbumNo.");
				return (RadioButton)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named lblLastActivityDate.
		/// </summary>
		/// <value>The lblLastActivityDate control.</value>
		public Label lblLastActivityDate
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditUser, "lblLastActivityDate");
				if (ctrl == null) throw new WebException("Cannot find Label lblLastActivityDate.");
				return (Label)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named lblLastLogOnDate.
		/// </summary>
		/// <value>The lblLastLogOnDate control.</value>
		public Label lblLastLogOnDate
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditUser, "lblLastLogOnDate");
				if (ctrl == null) throw new WebException("Cannot find Label lblLastLogOnDate.");
				return (Label)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named lblLastPasswordChangedDate.
		/// </summary>
		/// <value>The lblLastPasswordChangedDate control.</value>
		public Label lblLastPasswordChangedDate
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditUser, "lblLastPasswordChangedDate");
				if (ctrl == null) throw new WebException("Cannot find Label lblLastPasswordChangedDate.");
				return (Label)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named lblCreationDate.
		/// </summary>
		/// <value>The lblCreationDate control.</value>
		public Label lblCreationDate
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditUser, "lblCreationDate");
				if (ctrl == null) throw new WebException("Cannot find Label lblCreationDate.");
				return (Label)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named cblAvailableRolesForExistingUser.
		/// </summary>
		/// <value>The cblAvailableRolesForExistingUser control.</value>
		public CheckBoxList cblAvailableRolesForExistingUser
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditUser, "cblAvailableRolesForExistingUser");
				if (ctrl == null) throw new WebException("Cannot find CheckBoxList cblAvailableRolesForExistingUser.");
				return (CheckBoxList)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named rbResetPassword.
		/// </summary>
		/// <value>The rbResetPassword control.</value>
		public RadioButton rbResetPassword
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditUser, "rbResetPassword");
				if (ctrl == null) throw new WebException("Cannot find RadioButton rbResetPassword.");
				return (RadioButton)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named rbChangePassword.
		/// </summary>
		/// <value>The rbChangePassword control.</value>
		public RadioButton rbChangePassword
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditUser, "rbChangePassword");
				if (ctrl == null) throw new WebException("Cannot find RadioButton rbChangePassword.");
				return (RadioButton)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named txtPassword1.
		/// </summary>
		/// <value>The txtPassword1 control.</value>
		public TextBox txtPassword1
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditUser, "txtPassword1");
				if (ctrl == null) throw new WebException("Cannot find TextBox txtPassword1.");
				return (TextBox)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named txtPassword2.
		/// </summary>
		/// <value>The txtPassword2 control.</value>
		public TextBox txtPassword2
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditUser, "txtPassword2");
				if (ctrl == null) throw new WebException("Cannot find TextBox txtPassword2.");
				return (TextBox)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named lblNotMatchingPasswords.
		/// </summary>
		/// <value>The lblNotMatchingPasswords control.</value>
		public Label lblNotMatchingPasswords
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditUser, "lblNotMatchingPasswords");
				if (ctrl == null) throw new WebException("Cannot find Label lblNotMatchingPasswords.");
				return (Label)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named chkEmailNewPasswordToUser.
		/// </summary>
		/// <value>The chkEmailNewPasswordToUser control.</value>
		public CheckBox chkEmailNewPasswordToUser
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditUser, "chkEmailNewPasswordToUser");
				if (ctrl == null) throw new WebException("Cannot find CheckBox chkEmailNewPasswordToUser.");
				return (CheckBox)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named chkShowOwnerRoles.
		/// </summary>
		/// <value>The chkShowOwnerRoles control.</value>
		public CheckBox chkShowOwnerRoles
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditUser, "chkShowOwnerRoles");
				if (ctrl == null) throw new WebException("Cannot find CheckBox chkShowOwnerRoles.");
				return (CheckBox)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named lbEditUser.
		/// </summary>
		/// <value>The lbEditUser control.</value>
		public LinkButton lbEditUser
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditUser, "lbEditUser");
				if (ctrl == null) throw new WebException("Cannot find LinkButton lbEditUser.");
				return (LinkButton)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named lbUnlockUser.
		/// </summary>
		/// <value>The lbUnlockUser control.</value>
		public LinkButton lbUnlockUser
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditUser, "lbUnlockUser");
				if (ctrl == null) throw new WebException("Cannot find LinkButton lbUnlockUser.");
				return (LinkButton)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named btnSave.
		/// </summary>
		/// <value>The btnSave control.</value>
		public Button btnSave
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditUser, "btnSave");
				if (ctrl == null) throw new WebException("Cannot find user control btnSave.");
				return (Button)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named upEditUser.
		/// </summary>
		/// <value>The upEditUser control.</value>
		public UpdatePanel upEditUser
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditUser, "upEditUser");
				if (ctrl == null) throw new WebException("Cannot find user control upEditUser.");
				return (UpdatePanel)ctrl;
			}
		}

		#endregion

		#region Controls in dgAddUser

		/// <summary>
		/// Gets a reference to the control named btnCreateUser.
		/// </summary>
		/// <value>The btnCreateUser control.</value>
		public Button btnCreateUser
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgAddUser, "btnCreateUser");
				if (ctrl == null) throw new WebException("Cannot find Button btnCreateUser.");
				return (Button)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named pnlAddUserMessage.
		/// </summary>
		/// <value>The pnlAddUserMessage control.</value>
		public Panel pnlAddUserMessage
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgAddUser, "pnlAddUserMessage");
				if (ctrl == null) throw new WebException("Cannot find Panel pnlAddUserMessage.");
				return (Panel)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named txtNewUserUserName.
		/// </summary>
		/// <value>The txtNewUserUserName control.</value>
		public TextBox txtNewUserUserName
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgAddUser, "txtNewUserUserName");
				if (ctrl == null) throw new WebException("Cannot find TextBox txtNewUserUserName.");
				return (TextBox)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named lblUserNameValidationResult.
		/// </summary>
		/// <value>The lblUserNameValidationResult control.</value>
		public Label lblUserNameValidationResult
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgAddUser, "lblUserNameValidationResult");
				if (ctrl == null) throw new WebException("Cannot find Label lblUserNameValidationResult.");
				return (Label)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named txtNewUserEmail.
		/// </summary>
		/// <value>The txtNewUserEmail control.</value>
		public TextBox txtNewUserEmail
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgAddUser, "txtNewUserEmail");
				if (ctrl == null) throw new WebException("Cannot find TextBox txtNewUserEmail.");
				return (TextBox)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named txtNewUserPassword1.
		/// </summary>
		/// <value>The txtNewUserPassword1 control.</value>
		public TextBox txtNewUserPassword1
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgAddUser, "txtNewUserPassword1");
				if (ctrl == null) throw new WebException("Cannot find TextBox txtNewUserPassword1.");
				return (TextBox)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named txtNewUserPassword2.
		/// </summary>
		/// <value>The txtNewUserPassword2 control.</value>
		public TextBox txtNewUserPassword2
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgAddUser, "txtNewUserPassword2");
				if (ctrl == null) throw new WebException("Cannot find TextBox txtNewUserPassword2.");
				return (TextBox)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named cblAvailableRolesForNewUser.
		/// </summary>
		/// <value>The cblAvailableRolesForNewUser control.</value>
		public CheckBoxList cblAvailableRolesForNewUser
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgAddUser, "cblAvailableRolesForNewUser");
				if (ctrl == null) throw new WebException("Cannot find CheckBoxList cblAvailableRolesForNewUser.");
				return (CheckBoxList)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named mpAddUser.
		/// </summary>
		/// <value>The mpAddUser control.</value>
		public MultiPage mpAddUser
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgAddUser, "mpAddUser");
				if (ctrl == null) throw new WebException("Cannot find MultiPage mpAddUser.");
				return (MultiPage)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named upAddUser.
		/// </summary>
		/// <value>The upAddUser control.</value>
		public UpdatePanel upAddUser
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgAddUser, "upAddUser");
				if (ctrl == null) throw new WebException("Cannot find UpdatePanel upAddUser.");
				return (UpdatePanel)ctrl;
			}
		}

		#endregion

		#endregion

		#region Private Methods

		private void ShowErrorInDialog(Exception ex, string messageTitle, Control containingControl)
		{
			ShowErrorInDialog(messageTitle, ex.Message, containingControl);
		}

		private void ShowErrorInDialog(string messageTitle, string messageDetail, Control containingControl)
		{
			Controls.usermessage msgBox = (Controls.usermessage)LoadControl(Utils.GetUrl("/controls/usermessage.ascx"));
			msgBox.IconStyle = MessageStyle.Error;
			msgBox.MessageTitle = messageTitle;
			msgBox.MessageDetail = messageDetail;
			containingControl.Controls.Clear();
			containingControl.Controls.Add(msgBox);
		}

		private void ProcessQueryString()
		{
			if ((Utils.GetQueryStringParameterString("action") == "edit") && (!String.IsNullOrEmpty(Utils.GetQueryStringParameterString("user"))))
			{
				// We want to invoke the edit user dialog, so call the editUser() javascript function when the page loads.
				try
				{
					string userName = HelperFunctions.Decrypt(Utils.GetQueryStringParameterString("user"));

					string script = GetPageLoadScript(String.Format(CultureInfo.InvariantCulture, "editUser('{0}');", userName));

					ScriptManager.RegisterStartupScript(this, this.GetType(), "startupScript", script, true);
				}
				catch (FormatException ex)
				{
					LogError(ex);
				}
			}
		}

		private void ConfigureControlsFirstTime()
		{
			pAddUser.Visible = (UserCanEditUsersAndRoles && !AppSetting.Instance.License.IsInReducedFunctionalityMode);

			if (AppSetting.Instance.License.IsInReducedFunctionalityMode)
			{
				wwMessage.ShowMessage(Resources.GalleryServerPro.Admin_Need_Product_Key_Msg2);
				wwMessage.CssClass = "wwErrorSuccess gsp_msgwarning";
			}

			this.PageTitle = Resources.GalleryServerPro.Admin_Manage_Users_Page_Header;

			ConfigureGrid();

			ConfigureDialogs();

			ConfigureTabStrip();

			RegisterJavaScript();

			CallBack cbValidateNewUserName = FindControlRecursive(mpAddUser, "cbValidateNewUserName") as CallBack;
			if (cbValidateNewUserName != null)
				cbValidateNewUserName.CallbackPrefix = Utils.GetCurrentPageUri().ToString();

			cblAvailableRolesForNewUser.DataSource = GetRolesCurrentUserCanView();
			cblAvailableRolesForNewUser.DataBind();

			cblAvailableRolesForExistingUser.DataSource = GetRolesCurrentUserCanView();
			cblAvailableRolesForExistingUser.DataBind();
		}

		private void ConfigureControlsEveryTime()
		{
			tsEditUser.ImagesBaseUrl = String.Concat(Utils.GalleryRoot, "/images/componentart/tabstrip/");
			tsEditUser.TopGroupSeparatorImagesFolderUrl = String.Concat(Utils.GalleryRoot, "/images/componentart/tabstrip/");
		}

		private void ConfigureDialogs()
		{
			trUserAlbumTableRow.Visible = GallerySettings.EnableUserAlbum;
		}

		private void ConfigureGrid()
		{
			gdUsers.ImagesBaseUrl = String.Concat(Utils.GalleryRoot, "/images/componentart/grid/");
			gdUsers.PagerImagesFolderUrl = String.Concat(Utils.GalleryRoot, "/images/componentart/grid/pager/");

			if (GetUsersCurrentUserCanView().Count > gdUsers.PageSize)
				gdUsers.AutoFocusSearchBox = false;

			if (UserCanEditUsersAndRoles && !AppSetting.Instance.License.IsInReducedFunctionalityMode)
				AddEditColumnClientTemplate();

			BindUsersGrid();
		}

		private void BindUsersGrid()
		{
			gdUsers.DataSource = GetUsersCurrentUserCanView();
			gdUsers.DataBind();
		}

		private ComponentArt.Web.UI.ClientTemplate AddEditColumnClientTemplate()
		{
			// Add edit column client template
			ComponentArt.Web.UI.ClientTemplate userEditColumn = new ComponentArt.Web.UI.ClientTemplate();

			userEditColumn.ID = "userEditColumn";
			userEditColumn.Text = String.Format(CultureInfo.InvariantCulture, @"<p>
						<a id=""## makeValidForId(DataItem.getMember('UserName').get_value()) ##"" href=""javascript:editUser(decodeURI('## getUserName(DataItem) ##'));""
							title=""{0}"">{1}</a> <a href=""javascript:hideUserMessage();if (ConfirmDelete(decodeURI('## getUserName(DataItem) ##'))) gdUsers.deleteItem(gdUsers.getItemFromKey(0, '## getUserNameNoEncode(DataItem) ##'));"">
								{2}</a>
					</p>",
																					Resources.GalleryServerPro.Admin_Manage_Roles_Edit_User_Tooltip_Text,
																					Resources.GalleryServerPro.Admin_Manage_Roles_Edit_User_Hyperlink_Text,
																					Resources.GalleryServerPro.Admin_Manage_Roles_Delete_User_Hyperlink_Text);

			gdUsers.ClientTemplates.Add(userEditColumn);
			return userEditColumn;
		}

		private void ConfigureTabStrip()
		{
			foreach (TabStripTab tab in tsEditUser.Tabs)
			{
				switch (tab.ID)
				{
					case "tabGeneral": tab.Text = Resources.GalleryServerPro.Admin_Manage_Users_Edit_User_General_Tab_Text; break;
					case "tabRoles": tab.Text = Resources.GalleryServerPro.Admin_Manage_Users_Edit_User_Roles_Tab_Text; break;
					case "tabPassword": tab.Text = Resources.GalleryServerPro.Admin_Manage_Users_Edit_User_Password_Tab_Text; break;
				}
			}
		}

		/// <summary>
		/// Set the properties of the controls in the dialog with user-specific data. This method assumes the controls, such as
		/// the CheckBoxList of roles have already been databound. The CallBack control automatically preserves viewstate of 
		/// user-writeable controls such as textboxes and radio buttons, so during callbacks within the open dialog - such as 
		/// changing the user's password - we only want to refresh the read-only controls such as labels (since they lost 
		/// their info during the postback). Use the updateWriteableControls to control this.
		/// </summary>
		/// <param name="userName">The user name whose information should be bound to the controls.</param>
		/// <param name="updateWriteableControls">Indicates whether read-only controls such as labels should be updated, or all 
		/// controls, even writeable ones such as textboxes should be updated. Specify true to update all controls or false
		/// to only update read-only controls.</param>
		private void PopulateControlsWithUserData(string userName, bool updateWriteableControls)
		{
			IUserAccount user = UserController.GetUser(userName, false);

			// In some cases DNN stores a HTML-encoded version of the username, so if we get null, try again with an HTML-encoded
			// version. This isn't really needed in the stand-alone version but it doesn't hurt and helps keep the code the same.
			if (user == null)
				user = UserController.GetUser(Utils.HtmlEncode(userName), false);

			if (user == null)
				throw new WebException(Resources.GalleryServerPro.Admin_Manage_Users_Invalid_User_Text);

			CheckForLockedUser(user);

			BindUserInfoControls(user, updateWriteableControls);

			BindRolePermissionCheckboxes(user);

			BindPasswordControls(HelperFunctions.IsValidEmail(user.Email));
		}

		private void CheckForLockedUser(IUserAccount user)
		{
			if (!user.IsLockedOut) return;

			string msgHeader = Resources.GalleryServerPro.Admin_Manage_Users_Locked_User_Hdr;
			string msgDetail = String.Format(CultureInfo.CurrentCulture, "{0} <a href=\"javascript:unlockUser()\">{1}</a>", Resources.GalleryServerPro.Admin_Manage_Users_Locked_User_Dtl, Resources.GalleryServerPro.Admin_Manage_Users_Unlock_User_Hyperlink_Text);

			GalleryServerPro.Web.Controls.usermessage msgBox = (GalleryServerPro.Web.Controls.usermessage)LoadControl(Utils.GetUrl("/controls/usermessage.ascx"));
			msgBox.IconStyle = GalleryServerPro.Web.MessageStyle.Information;
			msgBox.MessageTitle = msgHeader;
			msgBox.MessageDetail = msgDetail;
			phEditUserMessage.Controls.Add(msgBox);
		}

		private void BindUserInfoControls(IUserAccount user, bool updateWriteableControls)
		{
			lblUserName.Text = Utils.HtmlEncode(user.UserName);

			if (updateWriteableControls)
			{
				txtComment.Text = user.Comment;
				txtEmail.Text = user.Email;
				rbApprovedYes.Checked = user.IsApproved;
				rbApprovedNo.Checked = !user.IsApproved;

				IUserGalleryProfile profile = ProfileController.GetProfileForGallery(user.UserName, GalleryId);
				rbUserAlbumYes.Checked = profile.EnableUserAlbum;
				rbUserAlbumNo.Checked = !profile.EnableUserAlbum;
			}

			try
			{
				lblCreationDate.Text = String.Format(CultureInfo.CurrentCulture, "{0:F} (GMT)", user.CreationDate);
				lblLastActivityDate.Text = String.Format(CultureInfo.CurrentCulture, "{0:F} (GMT)", user.LastActivityDate);
				lblLastLogOnDate.Text = String.Format(CultureInfo.CurrentCulture, "{0:F} (GMT)", user.LastLoginDate);
				lblLastPasswordChangedDate.Text = String.Format(CultureInfo.CurrentCulture, "{0:F} (GMT)", user.LastPasswordChangedDate);
			}
			catch (NotSupportedException) { /* Ignore if provider does not support one or more properties */}
		}

		/// <summary>
		/// Select the checkboxes corresponding to the roles to which the specified user belong. This method assumes the checkboxlist 
		/// has already been created and databound with the objectdatasource.
		/// </summary>
		/// <param name="user">An instance of <see cref="MembershipUser"/> that represents a user in the application.</param>
		private void BindRolePermissionCheckboxes(IUserAccount user)
		{
			cblAvailableRolesForExistingUser.ClearSelection();
			string[] rolesForUser = RoleController.GetRolesForUser(user.UserName);
			foreach (ListItem checkbox in cblAvailableRolesForExistingUser.Items)
			{
				string checkboxRoleName = Utils.HtmlDecode(checkbox.Text);
				if (Array.Exists(rolesForUser, delegate(string roleName)
																				{
																					return (roleName.Equals(checkboxRoleName, StringComparison.OrdinalIgnoreCase));
																				}))
				{
					checkbox.Selected = true;
				}

				if (RoleController.IsRoleAnAlbumOwnerRole(checkboxRoleName) || RoleController.IsRoleAnAlbumOwnerTemplateRole(checkboxRoleName))
				{
					checkbox.Attributes["class"] = "gsp_j_eaor";
				}
			}
		}

		private void BindPasswordControls(bool userHasValidEmail)
		{
			bool needToShowMsg = false;
			GalleryServerPro.Web.MessageStyle iconStyle = GalleryServerPro.Web.MessageStyle.Information;
			string msg = "<ul style='list-style-type:disc;'>";

			if (UserController.RequiresQuestionAndAnswer)
			{
				msg += String.Format(CultureInfo.CurrentCulture, "<li>{0}</li>", Resources.GalleryServerPro.Admin_Manage_Users_Question_Answer_Enabled_Msg);
				needToShowMsg = true;
				// disable 1 and 2
				iconStyle = GalleryServerPro.Web.MessageStyle.Warning;
				rbChangePassword.Checked = false;
				rbChangePassword.Enabled = false;
				rbResetPassword.Checked = false;
				rbResetPassword.Enabled = false;
				txtPassword1.Enabled = false;
				txtPassword2.Enabled = false;
				chkEmailNewPasswordToUser.Enabled = false;
				chkEmailNewPasswordToUser.CssClass = "gsp_disabledtext";
			}

			if (!UserController.EnablePasswordReset)
			{
				msg = String.Format(CultureInfo.CurrentCulture, "<li>{0}</li>", Resources.GalleryServerPro.Admin_Manage_Users_Pwd_Rest_Disabled_Msg);
				needToShowMsg = true;
				// disable 1
				rbResetPassword.Checked = false;
				rbResetPassword.Enabled = false;
				rbChangePassword.Checked = rbChangePassword.Enabled & true;
			}

			if (!UserController.EnablePasswordRetrieval)
			{
				msg += String.Format(CultureInfo.CurrentCulture, "<li>{0}</li>", Resources.GalleryServerPro.Admin_Manage_Users_Pwd_Retrieval_Disabled_Msg);
				needToShowMsg = true;
				// disable 2
				rbChangePassword.Checked = false;
				rbChangePassword.Enabled = false;
				txtPassword1.Enabled = false;
				txtPassword2.Enabled = false;
				rbResetPassword.Checked = rbResetPassword.Enabled & true;
			}

			if (userHasValidEmail)
			{
				chkEmailNewPasswordToUser.Enabled = true;
				chkEmailNewPasswordToUser.CssClass = "";
			}
			else
			{
				msg += String.Format(CultureInfo.CurrentCulture, "<li>{0}</li>", Resources.GalleryServerPro.Admin_Manage_Users_No_User_Email_Msg);
				needToShowMsg = true;
				chkEmailNewPasswordToUser.Enabled = false;
				chkEmailNewPasswordToUser.CssClass = "gsp_disabledtext";
			}

			// Don't need to check the following situation: When web.config is set this way, it throws a Configuration Exception during app startup.
			//if ((Membership.EnablePasswordRetrieval) && (Membership.Provider.PasswordFormat == MembershipPasswordFormat.Hashed))
			//  msg += "<li>Cannot change password. Gallery Server is configured to store passwords in a hashed format, which means they cannot be retrieved. Since the Membership provider requires the original password when changing it, you are unable to change the password. You are able, however, to reset the password.</li>";

			if (needToShowMsg)
			{
				msg += "</ul>";
				GalleryServerPro.Web.Controls.usermessage msgBox = (GalleryServerPro.Web.Controls.usermessage)LoadControl(Utils.GetUrl("/controls/usermessage.ascx"));
				msgBox.IconStyle = iconStyle;
				msgBox.MessageTitle = Resources.GalleryServerPro.Admin_Manage_Users_Msg_Hdr;
				msgBox.MessageDetail = msg;
				msgBox.CssClass = "um1ContainerCss";
				msgBox.HeaderCssClass = "um1HeaderCss";
				msgBox.DetailCssClass = "um1DetailCss";
				phDialogMessagePasswordTab.Controls.Add(msgBox);
			}
		}

		private void SaveUser(string userName)
		{
			// Step 1: Update general info (1st tab).
			IUserAccount user = UserController.GetUser(userName, false);

			// In some cases DNN stores a HTML-encoded version of the username, so if we get null, try again with an HTML-encoded
			// version. This isn't really needed in the stand-alone version but it doesn't hurt and helps keep the code the same.
			if (user == null)
				user = UserController.GetUser(Utils.HtmlEncode(userName), false);

			user.Email = txtEmail.Text;
			user.Comment = txtComment.Text;

			bool userWasApproved = false;
			if (!user.IsApproved && rbApprovedYes.Checked)
			{
				// Administrator is approving user. Make a note of it; we'll send an e-mail notification later in this function.
				userWasApproved = true;
			}

			user.IsApproved = rbApprovedYes.Checked;

			// Step 2: Update role membership (2nd tab).
			List<string> rolesToAdd = new List<string>();
			List<string> rolesToRemove = new List<string>();

			foreach (ListItem checkbox in cblAvailableRolesForExistingUser.Items)
			{
				string roleName = Utils.HtmlDecode(checkbox.Value);
				if (checkbox.Selected)
				{
					// Make sure user is in this role.);
					if (!RoleController.IsUserInRole(userName, roleName))
					{
						rolesToAdd.Add(roleName);
					}
				}
				else
				{
					// Make sure user is NOT in this role.
					if (RoleController.IsUserInRole(userName, roleName))
					{
						rolesToRemove.Add(roleName);
					}
				}
			}

			UserController.SaveUser(user, rolesToAdd.ToArray(), rolesToRemove.ToArray(), GalleryId);

			SaveProfile(userName);

			if (userWasApproved)
			{
				// Administrator is approving user. Send notification e-mail to user.
				EmailController.SendNotificationEmail(user, EmailTemplateForm.UserNotificationAccountCreatedApprovalGiven, GalleryId);
			}

			HelperFunctions.PurgeCache();
		}

		private void SaveProfile(string userName)
		{
			if (!GallerySettings.EnableUserAlbum)
				return; // User albums are disabled system-wide, so there is nothing to save.

			// Get reference to user's album. We need to do this *before* saving the profile, because if the admin disabled the user album,
			// this method will return null after saving the profile.
			IAlbum album = UserController.GetUserAlbum(userName, GalleryId);

			IUserProfile userProfile = ProfileController.GetProfile(userName);
			IUserGalleryProfile profile = userProfile.GetGalleryProfile(GalleryId);

			profile.EnableUserAlbum = rbUserAlbumYes.Checked;

			if (!profile.EnableUserAlbum)
			{
				profile.UserAlbumId = 0;
			}

			ProfileController.SaveProfile(userProfile);

			if (!profile.EnableUserAlbum)
			{
				AlbumController.DeleteAlbum(album);
			}
		}

		private void UpdatePassword(string userName)
		{
			UpdatePassword(userName, false);
		}

		private void UpdatePassword(string userName, bool userNameIsHtmlEncoded)
		{
			try
			{
				GalleryServerPro.Web.MessageStyle iconStyle = GalleryServerPro.Web.MessageStyle.Information;
				string msgTitle = Resources.GalleryServerPro.Admin_Manage_Users_Pwd_Changed_Text;
				string msgDetail = String.Empty;

				#region Update Password

				if (rbResetPassword.Checked)
				{
					string newPassword = UserController.ResetPassword(userName);
					string msg = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.Admin_Manage_Users_New_Pwd_Text, newPassword);
					msgDetail = String.Format(CultureInfo.CurrentCulture, "<p>{0}</p>", msg);
				}
				else if (rbChangePassword.Checked)
				{
					string newPassword = this.txtPassword1.Text;
					if (newPassword != txtPassword2.Text)
					{
						lblNotMatchingPasswords.Visible = true;
						return;
					}

					if (!String.IsNullOrEmpty(newPassword))
					{
						UserController.ChangePassword(userName, UserController.GetPassword(userName), newPassword);
					}
					else
					{
						msgTitle = Resources.GalleryServerPro.Admin_Manage_Users_No_Pwd_Supplied_Hdr;
						msgDetail = Resources.GalleryServerPro.Admin_Manage_Users_No_Pwd_Supplied_Dtl;
						iconStyle = GalleryServerPro.Web.MessageStyle.Warning;
					}
				}

				#endregion

				#region Email User

				IUserAccount user = UserController.GetUser(userName, false);

				// In some cases DNN stores a HTML-encoded version of the username, so if we get null, try again with an HTML-encoded
				// version. This isn't really needed in the stand-alone version but it doesn't hurt and helps keep the code the same.
				if (user == null)
					user = UserController.GetUser(Utils.HtmlEncode(userName), false);

				if (chkEmailNewPasswordToUser.Checked)
				{
					if (HelperFunctions.IsValidEmail(user.Email))
					{
						try
						{
							EmailController.SendNotificationEmail(user, EmailTemplateForm.UserNotificationPasswordChangedByAdmin, GalleryId, false);
						}
						catch (Exception ex)
						{
							string msg = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.Admin_Manage_Users_Pwd_Change_Email_Error_Msg, Utils.GetExceptionDetails(ex));
							msgDetail += String.Concat("<p>", msg, "</p>");
						}
					}
					else
					{
						msgDetail += String.Format(CultureInfo.CurrentCulture, "<p>{0}</p>", Resources.GalleryServerPro.Admin_Manage_Users_Pwd_Change_Email_Invalid_Msg);
					}
				}

				#endregion

				#region Render Confirmation Message

				phDialogMessagePasswordTab.Controls.Clear();
				GalleryServerPro.Web.Controls.usermessage msgBox = (GalleryServerPro.Web.Controls.usermessage)LoadControl(Utils.GetUrl("/controls/usermessage.ascx"));
				msgBox.IconStyle = iconStyle;
				msgBox.MessageTitle = msgTitle;
				msgBox.MessageDetail = msgDetail;
				msgBox.CssClass = "um1ContainerCss";
				msgBox.HeaderCssClass = "um1HeaderCss";
				msgBox.DetailCssClass = "um1DetailCss";
				phDialogMessagePasswordTab.Controls.Add(msgBox);

				#endregion
			}
			catch (NullReferenceException)
			{
				// In some cases DNN stores a HTML-encoded version of the username, so if we get a NullReferenceException, try again with an HTML-encoded
				// version. This isn't really needed in the stand-alone version but it doesn't hurt and helps keep the code the same.
				if (!userNameIsHtmlEncoded)
				{
					UpdatePassword(Utils.HtmlEncode(userName), true);
				}
				else
					throw;
			}
		}

		private void UnlockUser(string userName)
		{
			UserController.UnlockUser(userName);

			#region Render Confirmation Message

			string msgTitle = Resources.GalleryServerPro.Admin_Manage_Users_User_Unlocked_Msg;

			phEditUserMessage.Controls.Clear();
			GalleryServerPro.Web.Controls.usermessage msgBox = (GalleryServerPro.Web.Controls.usermessage)LoadControl(Utils.GetUrl("/controls/usermessage.ascx"));
			msgBox.IconStyle = GalleryServerPro.Web.MessageStyle.Information;
			msgBox.MessageTitle = msgTitle;
			phEditUserMessage.Controls.Add(msgBox);

			#endregion
		}

		private void AddUser()
		{
			string newUserName = txtNewUserUserName.Text;
			string newUserPassword1 = txtNewUserPassword1.Text;
			string newUserPassword2 = txtNewUserPassword2.Text;

			if (newUserPassword1 != newUserPassword2)
				throw new WebException(Resources.GalleryServerPro.Admin_Manage_Users_Passwords_Not_Matching_Error);

			UserController.CreateUser(newUserName, newUserPassword1, txtNewUserEmail.Text, GetSelectedRolesForNewUser(), false, GalleryId);
		}

		private void RegisterJavaScript()
		{
			// Add reference to a few script files.
			string script = String.Format(CultureInfo.InvariantCulture, @"
			function setText(node, newText)
			{{
				var childNodes = node.childNodes;
				for (var i=0; i < childNodes.length; i++)
				{{
					node.removeChild(childNodes[i]);
				}}
				if ((newText != null) && (newText.length > 0))
					node.appendChild(document.createTextNode(newText));
			}}

			function ConfirmDelete(userName)
			{{
				return confirm('{0} ' + userName + '?');
			}}

			function manageUsersInitializeRequest(sender, args) {{
				$('#{17}').hide();
				$('#{12}').hide();

				if (tsEditUser != null)
					tsEditUser.dispose();

				if (mpEditUser != null)
					mpEditUser.dispose();
			}} 

			function manageUsersEndRequest(sender, args) {{
				var callbackStatus = args.get_dataItems()['{18}'];

				if (callbackStatus != '{20}')
				{{
					$('#{17}').show(); // Show dialog contents for all scenarios except successfully saving a user
					$('#{12}').show();
				}}

				if (callbackStatus == '{19}')
					onAddUserCallbackComplete();
				else if (callbackStatus == '{20}')
					onSaveUserCallbackComplete();
				else if (callbackStatus == '{22}')
					onShowEditUserCallbackComplete();
				else if (callbackStatus == '{23}')
					onPwdUpdatedCallbackComplete();
				else if (callbackStatus == '{24}')
					onUnlockUserCallbackComplete();
			}}

			function onAddUserCallbackComplete()
			{{
					$('#{12}').show();
					mpAddUser.goLast();
					gdUsers.callback();
					setText($get('newUsername'), $get('{2}').value);
			}}

			function onSaveUserCallbackComplete()
			{{
				dgEditUser.Close();
			}}

			function onShowEditUserCallbackComplete()
			{{
				$('#{17}').removeClass('gsp_invisible');
			
				var chkShowOwnerRoles = $get('{13}');
				if ((chkShowOwnerRoles != null) && (chkShowOwnerRoles.checked == false))
					hideOwnerRoles();
			}}

			function onPwdUpdatedCallbackComplete()
			{{
			}}

			function onUnlockUserCallbackComplete()
			{{
			}}

			function editUser(userName)
			{{
				// Decode escaped quotes
				userName = userName.replace(/\\""/g, '\""');

				hideUserMessage();

				if(dgEditUser.get_isShowing())
				{{
					dgEditUser.Close();
				}}
				else if (dgAddUser.get_isShowing())
				{{
					dgAddUser.Close();
				}}
				else
				{{
					$('#{16}').val(userName); // Assign to hidden field; used by server code
					eval(""{15}""); // Initiates postback
					setText($get('editUserDialogHeader'), '{1} - ' + userName);
					dgEditUser.beginUpdate();

					if (gdUsersLoaded)
						dgEditUser.set_animationDirectionElement(makeValidForId(userName));
					else
						dgEditUser.set_animationDirectionElement('addnewuser');

					dgEditUser.set_value(userName);
					dgEditUser.set_title('{1}: ' + userName);
					dgEditUser.endUpdate();

					try {{
						dgEditUser.Show();
					}} catch(e) {{
						dgEditUser.set_animationDirectionElement('addnewuser');
						dgEditUser.Show();
					}}
				}}
			}}

			function unlockUser()
			{{
				eval(""{25}""); // Initiates postback
			}}
	
			function addUser()
			{{
				if (dgEditUser.get_isShowing())
				{{
					dgEditUser.Close();
				}}
				else if(dgAddUser.get_isShowing())
				{{
					dgAddUser.Close();
				}}
				else
				{{
					$get('{3}').disabled = true;
					mpAddUser.goFirst();
					dgAddUser.Show();
				}}
			}}

			function dgAddUser_onShow(sender, eventArgs)
			{{
				initializeAddUser()
			}}

			function initializeAddUser()
			{{
				$('#{12}').show();
				$('.newUserWizStep1Table input:text,.newUserWizStep1Table input:password').val(''); // Clear textboxes
				$("".gsp_newUserWiz_roles input[type='checkbox']:not([disabled='disabled'])"").removeAttr('checked'); // Uncheck optional roles
				mpAddUser.goFirst();
				$('#{9}').hide();
				$('#{2}').focus();
			}}

			function removeQSParm(url, param)
			{{
				// Note: Requires param to be after a '&'
				var re = new RegExp('(&)' + param + '=.*?(&|$)', 'i');
				if (url.match(re))
					return url.replace(re, '$2');
				else
					return url;
			}}

			function addUserWizard_onCompleteStep1()
			{{
					validateNewUser();
					mpAddUser.goNext();
			}}

			function addUserWizard_onPreviousClickStep2()
			{{
				hideAddUserMessage();
				mpAddUser.goFirst();
			}}

			function closeAddUserWizard()
			{{
				hideAddUserMessage();
				dgAddUser.Close('cancelled');
			}}
		
			function hideAddUserMessage()
			{{
				var pnl = $get('{8}');
				if (pnl)
					pnl.style.display = 'none';
			}}

			function validateNewUserName(userNameTextbox)
			{{
				var newUserName = userNameTextbox.value;
				cbValidateNewUserName.callback(newUserName);
			}}
	
			function validateNewUser()
			{{
				var foundValidationError = false;
				var newUsername = $get('{4}').value;
				var newUserPW1 = $get('{5}').value;
				var newUserPW2 = $get('{6}').value;
				
				if (newUsername.length == 0) foundValidationError = true;
				if (newUserPW1.length == 0) foundValidationError = true;
				if (newUserPW2.length == 0) foundValidationError = true;
				
				if (newUserPW1 != newUserPW2) foundValidationError = true;
				
				// If we get here we have passed validation. Enable Create User button.
				var createUserBtn = $get('{3}');
				createUserBtn.disabled = foundValidationError;
			}}

			function gdUsers_CallbackError(sender, args)
			{{
				showUserMessage(args.get_errorMessage());

				gdUsers.callback();
			}}

			function showUserMessage(msg)
			{{
				setText($get('{10}'), msg);

				var usrMsg = $get('{11}');
				if (Sys.UI.DomElement.containsCssClass(usrMsg, 'gsp_invisible'))
				{{
					Sys.UI.DomElement.removeCssClass(usrMsg, 'gsp_invisible');
				}}
			}}

			function hideUserMessage()
			{{
				var usrMsg = $get('{11}');
				if (!Sys.UI.DomElement.containsCssClass(usrMsg, 'gsp_invisible'))
				{{
					Sys.UI.DomElement.addCssClass(usrMsg, 'gsp_invisible');
				}}
			}}

			function htmlEncode(value)
			{{
				return $('<div/>').text(value).html();
			}}

			function htmlDecode(value)
			{{
				return $('<div/>').html(value).text();
			}}

			function makeValidForId(userName)
			{{
				// Undo CA's encoding of <, finally remove any character that isn't A-Z, a-z, 0-9, or _
				return userName.replace(/#%cLt#%/gi, '<').replace(/[^\w]/gi, '');

				// For DNN, first decode username, since DNN encodes any HTML in a username.
				//return htmlDecode(userName).replace(/#%cLt#%/gi, '<').replace(/[^\w]/gi, '');
			}}

			function getUserName(dataItem)
			{{
				var userName = dataItem.getMember('UserName').get_value();

				// Escape quotes, apostrophes and back slashes. Replace encoded < symbol (#%cLt#%) caused by CA with <
				return encodeURI(userName.replace(/""/g, '\\\""').replace(/\\/g, '\\\\').replace(/\'/g, ""\\'"").replace(/#%cLt#%/g, '<'));
			}}

			function getUserNameNoEncode(dataItem)
			{{
				var userName = dataItem.getMember('UserName').get_value();

				// Escape quotes, apostrophes and back slashes
				return userName.replace(/""/g, '\\\""').replace(/\\/g, '\\\\').replace(/\'/g, ""\\'"");
			}}

			var gdUsersLoaded = false;
			function gdUsers_Load(sender, eventArgs)
			{{
				gdUsersLoaded = true;

				// Fix URL for port forwarded configurations by replacing URL generated by CA with my own URL that has correct port
				var newUrl = '{14}';
				var oldPrefix = sender.CallbackPrefix;
				var idx = oldPrefix.lastIndexOf(""&Cart_"");
				var newPrefix = newUrl + oldPrefix.substring(idx);
				sender.CallbackPrefix = newPrefix;
			}}

",
																		Resources.GalleryServerPro.Admin_Manage_Users_Confirm_Delete_Text, // 0
																		Resources.GalleryServerPro.Admin_Dialog_Title_Edit_User, // 1
																		txtNewUserUserName.ClientID, // 2
																		btnCreateUser.ClientID, // 3
																		txtNewUserUserName.ClientID, // 4
																		txtNewUserPassword1.ClientID, // 5
																		txtNewUserPassword2.ClientID, // 6
																		String.Empty, // 7
																		pnlAddUserMessage.ClientID, // 8
																		lblUserNameValidationResult.ClientID, // 9
																		ucUserMessage.MessageDetailContainer.ClientID, // 10
																		ucUserMessage.MessageContainer.ClientID, // 11
																		mpAddUser.ClientID, // 12
																		chkShowOwnerRoles.ClientID, // 13
																		Utils.GetCurrentPageUri(), // 14
																		Page.ClientScript.GetPostBackEventReference(lbEditUser, null), // 15
																		hdnUserName.ClientID, // 16
																		pnlEditUserDialogContent.ClientID, // 17
																		hdnCallbackStatus.ClientID, // 18
																		UserCallbackStatus.AddUserSaveSuccess, // 19
																		UserCallbackStatus.EditUserSaveSuccess, // 20
																		String.Empty, // 21
																		UserCallbackStatus.ShowEditUserSuccess, // 22
																		UserCallbackStatus.PasswordUpdated, // 23
																		UserCallbackStatus.UserUnlocked, // 24
																		Page.ClientScript.GetPostBackEventReference(lbUnlockUser, null) // 25
				);

			ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "pageFunctions", script, true);
		}

		private string[] GetSelectedRolesForNewUser()
		{
			//// Step 2: Add roles to user.
			System.Collections.Generic.List<string> selectedRoleNames = new System.Collections.Generic.List<string>();
			foreach (ListItem roleItem in cblAvailableRolesForNewUser.Items)
			{
				if (roleItem.Selected)
				{
					selectedRoleNames.Add(Utils.HtmlDecode(roleItem.Value));
				}
			}

			string[] roleNames = new string[selectedRoleNames.Count];
			selectedRoleNames.CopyTo(roleNames);

			return roleNames;
		}

		#endregion
	}
}