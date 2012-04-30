using System;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using ComponentArt.Web.UI;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.Web.Controller;

namespace GalleryServerPro.Web.Controls
{
	/// <summary>
	/// A user control to handle user logins.
	/// </summary>
	public partial class login : GalleryUserControl
	{
		#region Private Fields

		private Login _login1;
		private LoginStatus _loginStatus1;
		private LoginName _loginName1;

		#endregion

		#region Properties

		/// <summary>
		/// Gets a reference to the Login control on the page. This property recursively searches the Dialog control
		/// to find it.
		/// </summary>
		/// <value>The Login control on the page.</value>
		protected Login Login1
		{
			get
			{
				if (_login1 == null)
				{
					Dialog dg = (Dialog)this.GalleryPage.FindControlRecursive(lv, "dgLogin");
					this._login1 = (Login)this.GalleryPage.FindControlRecursive(dg, "Login1");
				}

				return this._login1;
			}
		}

		/// <summary>
		/// Gets a reference to the LoginStatus control on the page. This property recursively searches the LoginView control
		/// to find it. Returns null when no user is logged on.
		/// </summary>
		/// <value>The LoginStatus control on the page.</value>
		protected LoginStatus LoginStatus1
		{
			get
			{
				if (_loginStatus1 == null)
				{
					this._loginStatus1 = (LoginStatus)this.GalleryPage.FindControlRecursive(lv, "LoginStatus1");
				}

				return this._loginStatus1;
			}
		}

		/// <summary>
		/// Gets a reference to the LoginName control on the page. This property recursively searches the LoginView control
		/// to find it. Returns null when no user is logged on.
		/// </summary>
		/// <value>The LoginName control on the page.</value>
		protected LoginName LoginName1
		{
			get
			{
				if (_loginName1 == null)
				{
					this._loginName1 = (LoginName)this.GalleryPage.FindControlRecursive(lv, "LoginName1");
				}

				return this._loginName1;
			}
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			ConfigureControls();

			RegisterJavascript();
		}

		/// <summary>
		/// Handles the LoginError event of the Login control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Login1_LoginError(object sender, EventArgs e)
		{
			// The user has entered an invalid user name and/or error. Redirect to login page and append message.
			Utils.Redirect(PageId.login, "msg={0}&ReturnUrl={1}", ((int)Message.UserNameOrPasswordIncorrect).ToString(CultureInfo.InvariantCulture), Utils.UrlEncode(Utils.GetCurrentPageUrl(true)));
		}

		/// <summary>
		/// Handles the LoggedIn event of the Login control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Login1_LoggedIn(object sender, EventArgs e)
		{
			// Get the user. This will ensure we get the username with the correct case, regardless of how the user logged on (Admin vs. admin, etc).
			IUserAccount user = UserController.GetUser(Login1.UserName, false);

			UserController.UserLoggedOn(user.UserName, this.GalleryPage.GalleryId);

			if (this.GalleryPage.GallerySettings.EnableUserAlbum && this.GalleryPage.GallerySettings.RedirectToUserAlbumAfterLogin)
			{
				Utils.Redirect(Utils.GetUrl(PageId.album, "aid={0}", UserController.GetUserAlbumId(user.UserName, this.GalleryPage.GalleryId)));
			}

			ReloadPage();
		}

		/// <summary>
		/// Handles the LoggedOut event of the LoginStatus control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void LoginStatus1_LoggedOut(object sender, EventArgs e)
		{
			Controller.UserController.UserLoggedOff();

			ReloadPage();
		}

		#endregion

		#region Private Methods

		private void ConfigureControls()
		{
			if (this.GalleryPage.ShowLogin)
			{
				ConfigureLogin();
			}
		}

		private void ConfigureLogin()
		{
			if (String.IsNullOrEmpty(Resources.GalleryServerPro.Login_Logged_On_Msg))
			{
				LoginName1.Visible = false;
			}

			ConfigureLoginDialog();
		}

		private void ConfigureLoginDialog()
		{
			if (this.GalleryPage.IsAnonymousUser)
			{
				Login1.MembershipProvider = Controller.UserController.MembershipGsp.Name;
				Login1.PasswordRecoveryUrl = Utils.GetUrl(Web.PageId.recoverpassword);

				if (this.GalleryPage.GallerySettings.EnableSelfRegistration)
				{
					Login1.CreateUserText = Resources.GalleryServerPro.Login_Create_Account_Text;
					Login1.CreateUserUrl = Utils.GetUrl(Web.PageId.createaccount);
				}
			}
			else
			{
				if (LoginStatus1 != null)
				{
					// The LoginStatus control should really never be null for logged on users, but in reality it sometimes is.
					// Not sure why - may be a bug in the control. We'll get around it by checking for null. Not a big deal since
					// all that happens is there is text instead of an image for the logout link.
					LoginStatus1.LogoutImageUrl = Utils.GetUrl("/images/logoff.png");
				}
			}
		}

		private void RegisterJavascript()
		{
			if (this.GalleryPage.IsAnonymousUser && this.GalleryPage.ShowLogin)
			{
				// Get reference to the UserName textbox.
				TextBox tb = (TextBox)this.GalleryPage.FindControlRecursive(Login1, "UserName");

				string script = String.Format(CultureInfo.InvariantCulture, @"
function toggleLogin()
{{
	if (typeof(dgSearch) !== 'undefined')
	 dgSearch.close();

	if (dgLogin.get_isShowing())
		dgLogin.close();
	else
		dgLogin.show();
}}

function dgLogin_OnShow()
{{
	$get('{0}').focus();
}}", tb.ClientID);

				ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "loginFocusScript", script, true);
			}
		}

		private void ReloadPage()
		{
			// If currently looking at a media object or album, update query string to point to current media object or
			// album page (if album paging is enabled) and redirect. Otherwise just navigate to current album.
			PageId pageId = this.GalleryPage.PageId;
			if ((pageId == PageId.album) || (pageId == PageId.mediaobject))
			{
				string url = Request.Url.PathAndQuery;

				url = Utils.RemoveQueryStringParameter(url, "msg"); // Remove any messages

				if (this.GalleryPage.GetMediaObjectId() > int.MinValue)
				{
					url = Utils.RemoveQueryStringParameter(url, "moid");
					url = Utils.AddQueryStringParameter(url, String.Concat("moid=", this.GalleryPage.GetMediaObjectId()));
				}

				int page = Utils.GetQueryStringParameterInt32(this.GalleryPage.PreviousUri, "page");
				if (page > int.MinValue)
				{
					url = Utils.RemoveQueryStringParameter(url, "page");
					url = Utils.AddQueryStringParameter(url, String.Concat("page=", page));
				}

				Utils.Redirect(url);
			}
			else
			{
				Utils.Redirect(PageId.album, "aid={0}", this.GalleryPage.GetAlbumId());
			}
		}

		#endregion

	}
}