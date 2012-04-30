using System;
using System.Globalization;
using System.Web.Security;
using System.Web.UI;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.ErrorHandler.CustomExceptions;
using GalleryServerPro.Web.Controller;

namespace GalleryServerPro.Web.Pages
{
	/// <summary>
	/// A page-like user control that allows a user to create a new account.
	/// </summary>
	public partial class createaccount : Pages.GalleryPage
	{

		#region Private Fields

		private bool? _enableUserAlbum;
		private bool? _enableEmailVerification;
		private bool? _requireAdminApproval;
		private bool? _useEmailForAccountName;

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets a value indicating whether user albums are enabled.
		/// </summary>
		/// <value><c>true</c> if user albums are enabled; otherwise, <c>false</c>.</value>
		public bool EnableUserAlbum
		{
			get
			{
				if (!this._enableUserAlbum.HasValue)
					this._enableUserAlbum = GallerySettings.EnableUserAlbum;

				return this._enableUserAlbum.Value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether email verification is required.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if email verification is required; otherwise, <c>false</c>.
		/// </value>
		public bool EnableEmailVerification
		{
			get
			{
				if (!this._enableEmailVerification.HasValue)
					this._enableEmailVerification = GallerySettings.RequireEmailValidationForSelfRegisteredUser;

				return this._enableEmailVerification.Value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether admin approval for new accounts is required.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if admin approval for new accounts is required; otherwise, <c>false</c>.
		/// </value>
		public bool RequireAdminApproval
		{
			get
			{
				if (!this._requireAdminApproval.HasValue)
					this._requireAdminApproval = GallerySettings.RequireApprovalForSelfRegisteredUser;

				return this._requireAdminApproval.Value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the username must consist of an e-mail address.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the username must consist of an e-mail address; otherwise, <c>false</c>.
		/// </value>
		public bool UseEmailForAccountName
		{
			get
			{
				if (!this._useEmailForAccountName.HasValue)
					this._useEmailForAccountName = GallerySettings.UseEmailForAccountName;

				return this._useEmailForAccountName.Value;
			}
		}

		#endregion

		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			if (Utils.IsQueryStringParameterPresent("verify"))
				ValidateUser();

			if (!IsAnonymousUser)
				Utils.Redirect(Web.PageId.album);

			if (!GallerySettings.EnableSelfRegistration)
				Utils.Redirect(Web.PageId.album);

			ConfigureControls();

			if (!IsPostBack)
			{
				ConfigureControlsFirstTime();
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
				if (UseEmailForAccountName && (!HelperFunctions.IsValidEmail(requestedUsername)))
				{
					// App is configured to use an e-mail address as the account name, but the name is not a valid
					// e-mail.
					lblUserNameValidationResult.Text = Resources.GalleryServerPro.CreateAccount_Verification_Username_Not_Valid_Email_Text;
					lblUserNameValidationResult.CssClass = "gsp_msgwarning";
				}
				else if (Utils.RemoveHtmlTags(requestedUsername).Length != requestedUsername.Length)
				{
					// The user name has HTML tags, which are not allowed.
					lblUserNameValidationResult.Text = Resources.GalleryServerPro.Site_Invalid_Text;
					lblUserNameValidationResult.CssClass = "gsp_msgwarning";
				}
				else
				{
					// We passed the first test above. Now verify that the requested user name is not already taken.
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
			}

			lblUserNameValidationResult.RenderControl(e.Output);
		}

		private void ConfigureControls()
		{
			txtNewUserUserName.Focus();

			if (UseEmailForAccountName)
			{
				trEmail.Visible = false;
				l2.Text = Resources.GalleryServerPro.CreateAccount_Email_Header_Text;
			}

			if (this.EnableEmailVerification)
			{
				lblEmailReqd.Visible = true;
				rfvEmail.Enabled = true;
			}

			RegisterJavaScript();
		}

		private void ConfigureControlsFirstTime()
		{
			cbValidateNewUserName.CallbackPrefix = Utils.GetCurrentPageUri().ToString();
		}

		private void RegisterJavaScript()
		{
			string script = String.Format(CultureInfo.InvariantCulture, @"

			function validateNewUserName(userNameTextbox)
			{{
				var newUserName = userNameTextbox.value;
				cbValidateNewUserName.callback(newUserName);
			}}

");

			ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "createUserFunctions", script, true);
		}

		/// <summary>
		/// Handles the Click event of the btnCreateAccount control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void btnCreateAccount_Click(object sender, EventArgs e)
		{
			CreateAccount();
		}

		private void CreateAccount()
		{
			try
			{
				IUserAccount user = this.AddUser();

				ReportSuccess(user);
			}
			catch (MembershipCreateUserException ex)
			{
				// Just in case we created the user and the exception occured at a later step, like adding the roles, delete the user
				// and - if it exists - the user album, but only if the user exists AND the error wasn't 'DuplicateUserName'.
				if ((ex.StatusCode != MembershipCreateStatus.DuplicateUserName) && (UserController.GetUser(this.txtNewUserUserName.Text, false) != null))
				{
					DeleteUserAlbum();

					UserController.DeleteUser(this.txtNewUserEmail.Text);
				}

				this.DisplayErrorMessage(Resources.GalleryServerPro.Admin_Manage_Users_Cannot_Create_User_Msg, UserController.GetAddUserErrorMessage(ex.StatusCode));

				LogError(ex);
			}
			catch (Exception ex)
			{
				// Just in case we created the user and the exception occured at a later step, like adding the roles, delete the user
				// and - if it exists - the user album.
				DeleteUserAlbum();

				if (UserController.GetUser(this.txtNewUserUserName.Text, false) != null)
				{
					UserController.DeleteUser(this.txtNewUserUserName.Text);
				}

				this.DisplayErrorMessage(Resources.GalleryServerPro.Admin_Manage_Users_Cannot_Create_User_Msg, ex.Message);

				LogError(ex);
			}
		}

		private void DeleteUserAlbum()
		{
			if (String.IsNullOrEmpty(this.txtNewUserUserName.Text))
				return;

			if (GallerySettings.EnableUserAlbum)
			{
				IAlbum album = null;

				try
				{
					IUserGalleryProfile profile = ProfileController.GetProfileForGallery(this.txtNewUserUserName.Text, GalleryId);

					if (profile != null)
					{
						album = AlbumController.LoadAlbumInstance(profile.UserAlbumId, false);
					}
				}
				catch (InvalidAlbumException) { return; }

				if (album != null)
				{
					AlbumController.DeleteAlbum(album);
				}
			}
		}

		private void ReportSuccess(IUserAccount user)
		{
			string title = Resources.GalleryServerPro.CreateAccount_Success_Header_Text;

			string detailPendingNotification = String.Concat("<p>", Resources.GalleryServerPro.CreateAccount_Success_Detail1_Text, "</p>");
			detailPendingNotification += String.Concat(@"<p>", String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.CreateAccount_Success_Pending_Notification_Detail2_Text, user.Email), "</p>");
			detailPendingNotification += String.Concat(@"<p>", Resources.GalleryServerPro.CreateAccount_Success_Pending_Notification_Detail3_Text, "</p>");

			string detailPendingApproval = String.Concat("<p>", Resources.GalleryServerPro.CreateAccount_Success_Detail1_Text, "</p>");
			detailPendingApproval += String.Concat(@"<p>", String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.CreateAccount_Success_Pending_Approval_Detail2_Text), "</p>");
			detailPendingApproval += String.Concat(@"<p>", Resources.GalleryServerPro.CreateAccount_Success_Pending_Approval_Detail3_Text, "</p>");

			string detailActivated = String.Format(CultureInfo.InvariantCulture, @"<p>{0}</p><p><a href=""{1}"">{2}</a></p>",
																						 Resources.GalleryServerPro.CreateAccount_Success_Detail1_Text,
																						 Utils.GetCurrentPageUrl(),
																						 Resources.GalleryServerPro.CreateAccount_Gallery_Link_Text);

			if (EnableEmailVerification)
			{
				DisplaySuccessMessage(title, detailPendingNotification);
			}
			else if (RequireAdminApproval)
			{
				DisplaySuccessMessage(title, detailPendingApproval);
			}
			else
			{
				UserController.LogOnUser(user.UserName, GalleryId);

				if (EnableUserAlbum && (UserController.GetUserAlbumId(user.UserName, GalleryId) > int.MinValue))
				{
					detailActivated += String.Format(CultureInfo.InvariantCulture, @"<p><a href=""{0}"">{1}</a></p>",
																																					 Utils.GetUrl(PageId.album, "aid={0}", UserController.GetUserAlbumId(user.UserName, GalleryId)),
																																					 Resources.GalleryServerPro.CreateAccount_User_Album_Link_Text);
				}

				DisplaySuccessMessage(title, detailActivated);
			}

			pnlCreateUser.Visible = false;
		}

		private IUserAccount AddUser()
		{
			string newUserName = txtNewUserUserName.Text;
			string newUserPassword1 = txtNewUserPassword1.Text;
			string newUserPassword2 = txtNewUserPassword2.Text;

			if (newUserPassword1 != newUserPassword2)
				throw new WebException(Resources.GalleryServerPro.Admin_Manage_Users_Passwords_Not_Matching_Error);

			return UserController.CreateUser(newUserName, newUserPassword1, txtNewUserEmail.Text, GallerySettings.DefaultRolesForSelfRegisteredUser, true, GalleryId);
		}

		private void DisplayErrorMessage(string title, string detail)
		{
			DisplayMessage(title, detail, MessageStyle.Error);
		}

		private void DisplaySuccessMessage(string title, string detail)
		{
			DisplayMessage(title, detail, MessageStyle.Information);
		}

		private void DisplayMessage(string title, string detail, MessageStyle iconStyle)
		{
			pnlMsgContainer.Controls.Clear();
			GalleryServerPro.Web.Controls.usermessage msgBox = (GalleryServerPro.Web.Controls.usermessage)Page.LoadControl(Utils.GetUrl("/controls/usermessage.ascx"));
			msgBox.IconStyle = iconStyle;
			msgBox.MessageTitle = title;
			msgBox.MessageDetail = detail;
			msgBox.CssClass = "um3ContainerCss gsp_rounded10 gsp_floatcontainer";
			msgBox.HeaderCssClass = "um1HeaderCss";
			msgBox.DetailCssClass = "um1DetailCss";
			pnlMsgContainer.Controls.Add(msgBox);
			pnlMsgContainer.Visible = true;
		}

		/// <summary>
		/// Update the user account to indicate the e-mail address has been validated. If admin approval is required, send an e-mail
		/// to the administrators. If not required, activate the account. Display results to user.
		/// </summary>
		private void ValidateUser()
		{
			if (!IsAnonymousUser)
			{
				// Log off user and reload page. This prevents the redirection that would occur after 
				// exiting the function. We need to reload page because some page state may have already 
				// been decided based on the logged on user at this point, so we just need to start over.
				UserController.LogOffUser();
				Utils.Redirect(Utils.GetCurrentPageUrl(true));
			}

			pnlCreateUser.Visible = false;

			try
			{
				string userName = HelperFunctions.Decrypt(Utils.GetQueryStringParameterString("verify"));

				UserController.UserEmailValidatedAfterCreation(userName, GalleryId);

				string title = Resources.GalleryServerPro.CreateAccount_Verification_Success_Header_Text;

				string detail = GetEmailValidatedUserMessageDetail(userName);

				DisplaySuccessMessage(title, detail);
			}
			catch (Exception ex)
			{
				LogError(ex);

				string failDetailText = String.Format(CultureInfo.CurrentCulture, "<p>{0}</p>", Resources.GalleryServerPro.CreateAccount_Verification_Fail_Detail_Text);

				DisplayErrorMessage(Resources.GalleryServerPro.CreateAccount_Verification_Fail_Header_Text, failDetailText);
			}
		}

		/// <summary>
		/// Gets the message to display to the user after she validated the account by clicking on the link in the verification
		/// e-mail.
		/// </summary>
		/// <param name="userName">The username whose account has been validated.</param>
		/// <returns>Returns an HTML-formatted string to display to the user.</returns>
		private string GetEmailValidatedUserMessageDetail(string userName)
		{
			if (GallerySettings.RequireApprovalForSelfRegisteredUser)
			{
				return String.Format(CultureInfo.InvariantCulture, @"<p>{0}</p>", Resources.GalleryServerPro.CreateAccount_Verification_Success_Needs_Admin_Approval_Detail_Text);
			}

			string detail = String.Format(CultureInfo.InvariantCulture, @"<p>{0}</p><p><a href=""{1}"">{2}</a></p>",
																		Resources.GalleryServerPro.CreateAccount_Verification_Success_Detail_Text,
																		Utils.GetCurrentPageUrl(),
																		Resources.GalleryServerPro.CreateAccount_Gallery_Link_Text);

			if (GallerySettings.EnableUserAlbum && (UserController.GetUserAlbumId(userName, GalleryId) > int.MinValue))
			{
				detail += String.Format(CultureInfo.InvariantCulture, @"<p><a href=""{0}"">{1}</a></p>",
																Utils.GetUrl(PageId.album, "aid={0}", UserController.GetUserAlbumId(userName, GalleryId)),
																Resources.GalleryServerPro.CreateAccount_User_Album_Link_Text);
			}

			return detail;
		}
	}
}