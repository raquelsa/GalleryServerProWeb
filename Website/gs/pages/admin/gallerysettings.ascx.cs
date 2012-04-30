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
	/// A page-like user control for administering gallery settings.
	/// </summary>
	public partial class gallerysettings : Pages.AdminPage
	{
		#region Private Fields

		private string _messageText;
		private string _messageCssClass;
		private bool _messageIsError;
		private List<String> _usersToNotifyWhenErrorOccurs;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets a comma-delimited list of account names of users to receive an e-mail notification when an account is 
		/// created. The text property of the cboUsersToNotify ComboBox binds to this property rather than 
		/// <see cref="IGallerySettings.UsersToNotifyWhenAccountIsCreated" /> because it needs a string and the other property 
		/// is a string array. This property essentially acts as a type converter to the "real" property in <see cref="IGallerySettings" />.
		/// </summary>
		public string UsersToNotifyWhenErrorOccurs
		{
			get
			{
				return String.Join(", ", GallerySettingsUpdateable.UsersToNotifyWhenErrorOccurs.GetUserNames());
			}
			set
			{
				GallerySettingsUpdateable.UsersToNotifyWhenErrorOccurs.Clear();

				if (String.IsNullOrEmpty(value))
					return;

				string[] userNames = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

				foreach (string userName in userNames)
				{
					IUserAccount user = UserController.GetUser(userName.Trim(), false);

					if (user != null)
					{
						GallerySettingsUpdateable.UsersToNotifyWhenErrorOccurs.Add(user);
					}
				}
			}
		}

		/// <summary>
		/// Gets the list of user names of accounts to notify when an error occurs. The value is a collection that is 
		/// parsed from the comma-delimited string stored in the UsersToNotifyWhenErrorOccurs configuration setting.
		/// During postbacks the value is retrieved from the combobox.
		/// </summary>
		/// <value>The list of user names of accounts to notify when an error occurs.</value>
		private List<String> UsersToNotifyWhenErrorOccursCollection
		{
			get
			{
				if (this._usersToNotifyWhenErrorOccurs == null)
				{
					this._usersToNotifyWhenErrorOccurs = new List<string>();

					if (IsPostBack)
					{
						string[] usersToNotify = cboUsersToNotify.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

						foreach (string userName in usersToNotify)
						{
							this._usersToNotifyWhenErrorOccurs.Add(userName.Trim());
						}
					}
					else
					{
						foreach (IUserAccount user in GallerySettingsUpdateable.UsersToNotifyWhenErrorOccurs)
						{
							this._usersToNotifyWhenErrorOccurs.Add(user.UserName);
						}
					}
				}

				return this._usersToNotifyWhenErrorOccurs;
			}
		}

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
		/// Gets or sets a value indicating whether the message is an error.
		/// </summary>
		/// <value><c>true</c> if the message is an error; otherwise, <c>false</c>.</value>
		private bool MessageIsError
		{
			get { return _messageIsError; }
			set { _messageIsError = value; }
		}

		/// <summary>
		/// Gets a value indicating whether the user is allowed to save changes on this page.
		/// </summary>
		/// <value><c>true</c> if saving is enabled; otherwise, <c>false</c>.</value>
		private static bool SavingIsEnabled
		{
			get
			{
				return (AppSetting.Instance.License.IsInTrialPeriod || AppSetting.Instance.License.IsValid);
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

			ConfigureControlsEveryTime();

			if (!IsPostBack)
			{
				ConfigureControlsFirstTime();
			}
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

			return true;
		}

		/// <summary>
		/// Handles the Click event of the btnThrowError control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void btnThrowError_Click(object sender, EventArgs e)
		{
			throw new WebException("This is a test error generated by Gallery Server Pro.");
		}

		/// <summary>
		/// Handles the Click event of the btnEmailTest control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void btnEmailTest_Click(object sender, EventArgs e)
		{
			SendTestEmail();
		}

		/// <summary>
		/// Handles the OnValidateControl event of the wwDataBinder control.
		/// </summary>
		/// <param name="item">The wwDataBindingItem item.</param>
		/// <returns>Returns <c>true</c> if the item is valid; otherwise returns <c>false</c>.</returns>
		protected bool wwDataBinder_ValidateControl(WebControls.wwDataBindingItem item)
		{
			if (!ValidateUsersToNotifyWhenErrorOccurs(item))
				return false;

			return true;
		}

		/// <summary>
		/// Handles the OnBeforeUnBindControl event of the wwDataBinder control.
		/// </summary>
		/// <param name="item">The wwDataBindingItem item.</param>
		protected bool wwDataBinder_BeforeUnbindControl(WebControls.wwDataBindingItem item)
		{
			if (!BeforeUnbind_ProcessDisplayBehaviorControls(item))
				return false;

			return true;
		}

		private bool BeforeUnbind_ProcessDisplayBehaviorControls(wwDataBindingItem item)
		{
			// When allow HTML is unchecked, several child items are disabled via javascript. Disabled HTML items are not
			// posted during a postback, so we don't have accurate information about their states. For these controls don't save
			// anything by returning false. Furthermore, to prevent these child controls from incorrectly reverting to an
			// empty or unchecked state in the UI, assign their properties to their config setting. 

			if (!this.chkShowHeader.Checked)
			{
				if (item.ControlId == this.txtWebsiteTitle.ID)
				{
					txtWebsiteTitle.Text = GallerySettingsUpdateable.GalleryTitle;
					return false;
				}

				if (item.ControlId == this.txtWebsiteTitleUrl.ID)
				{
					txtWebsiteTitleUrl.Text = GallerySettingsUpdateable.GalleryTitleUrl;
					return false;
				}

				if (item.ControlId == this.chkShowLogin.ID)
				{
					this.chkShowLogin.Checked = GallerySettingsUpdateable.ShowLogin;
					return false;
				}

				if (item.ControlId == this.chkShowSearch.ID)
				{
					this.chkShowSearch.Checked = GallerySettingsUpdateable.ShowSearch;
					return false;
				}
			}

			if (!this.chkSendEmail.Checked)
			{
				if (item.ControlId == this.cboUsersToNotify.ID)
				{
					cboUsersToNotify.Text = String.Empty;
					return false;
				}
			}
			return true;
		}

		#endregion

		#region Private Methods

		private void ConfigureControlsFirstTime()
		{
			AdminPageTitle = Resources.GalleryServerPro.Admin_Gallery_Settings_General_Page_Header;

			OkButtonBottom.Enabled = SavingIsEnabled;
			OkButtonTop.Enabled = SavingIsEnabled;

			if (AppSetting.Instance.License.IsInReducedFunctionalityMode)
			{
				MessageText = Resources.GalleryServerPro.Admin_Need_Product_Key_Msg2;
				MessageCssClass = "wwErrorSuccess gsp_msgwarning";
				OkButtonBottom.Enabled = false;
				OkButtonTop.Enabled = false;
			}

			this.wwDataBinder.DataBind();

			ConfigureUsersToNotifyComboBoxFirstTime();

			CheckForMessages();

			UpdateUI();
		}

		private void ConfigureUsersToNotifyComboBoxFirstTime()
		{
			// Configure the USER LIST ComboBox.
			cboUsersToNotify.DropHoverImageUrl = Utils.GetUrl("/images/componentart/combobox/ddn-hover.png");
			cboUsersToNotify.DropImageUrl = Utils.GetUrl("/images/componentart/combobox/ddn.png");
			cboUsersToNotify.Text = String.Join(", ", this.UsersToNotifyWhenErrorOccursCollection.ToArray()).TrimEnd(new char[] { ' ', ',' });

			// Add the users to the list, pre-selecting any that are specified in the config file
			List<ListItem> userListItems = new List<ListItem>();

			foreach (IUserAccount user in GetUsersCurrentUserCanView())
			{
				userListItems.Add(new ListItem(Utils.HtmlEncode(user.UserName), Utils.HtmlEncode(user.UserName)));

				if (this.UsersToNotifyWhenErrorOccursCollection.Contains(user.UserName))
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

		private void ConfigureControlsEveryTime()
		{
			this.PageTitle = Resources.GalleryServerPro.Admin_Gallery_Settings_General_Page_Header;
			lblGalleryDescription.Text = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServerPro.Admin_Gallery_Description_Label, Utils.GetCurrentPageUrl(), Utils.HtmlEncode(Factory.LoadGallery(GalleryId).Description));
		}

		private bool ValidateUsersToNotifyWhenErrorOccurs(wwDataBindingItem item)
		{
			if ((item.ControlInstance == this.cboUsersToNotify))
			{
				string userNamesCurrent = String.Join(", ", GallerySettings.UsersToNotifyWhenErrorOccurs.GetUserNames());

				if ((!this.cboUsersToNotify.Text.Equals(userNamesCurrent)))
				{
					// User has updated the list of users to notify. Make sure they represent valid user account names.
					foreach (string userName in this.cboUsersToNotify.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
					{
						IUserAccount user = UserController.GetUser(Utils.HtmlDecode(userName.Trim()), false);
						if (user == null)
						{
							item.BindingErrorMessage = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServerPro.Admin_General_Invalid_User_Name_Msg, userName.Trim());
							return false;
						}

						if (!HelperFunctions.IsValidEmail(user.Email))
						{
							item.BindingErrorMessage = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServerPro.Admin_General_Invalid_User_Email_Msg, userName.Trim());
							return false;
						}
					}
				}
			}
			return true;
		}

		private void SaveSettings()
		{
			this.wwDataBinder.Unbind(this);

			if (wwDataBinder.BindingErrors.Count > 0)
			{
				this.MessageText = wwDataBinder.BindingErrors.ToHtml();
				this.MessageIsError = true;
				UpdateUI();
				return;
			}

			GallerySettingsUpdateable.Save();

			// Since we are changing settings that affect how and which controls are rendered to the page, let us redirect to the current page and
			// show the save success message. If we simply show a message without redirecting, two things happen: (1) the user doesn't see the effect
			// of their change until the next page load, (2) there is the potential for a viewstate validation error
			const Message msg = Message.SettingsSuccessfullyChanged;

			Utils.Redirect(PageId.admin_gallerysettings, "aid={0}&msg={1}", GetAlbumId(), ((int)msg).ToString(CultureInfo.InvariantCulture));
		}

		private void SendTestEmail()
		{
			string subject = Resources.GalleryServerPro.Admin_Gallery_Settings_Test_Email_Subject;
			string body = Resources.GalleryServerPro.Admin_Gallery_Settings_Test_Email_Body;
			string msgResult = String.Empty;
			bool emailSent = false;
			IUserAccount user = UserController.GetUser();

			if (HelperFunctions.IsValidEmail(user.Email))
			{
				try
				{
					EmailController.SendEmail(user, subject, body, GalleryId);
					emailSent = true;
				}
				catch (Exception ex)
				{
					string errorMsg = Utils.GetExceptionDetails(ex);

					msgResult = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.Admin_Gallery_Settings_Test_Email_Failure_Text, errorMsg);
				}
			}
			else
			{
				msgResult = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.Admin_Gallery_Settings_Test_Email_Invalid_Text, user.UserName);
			}

			if (emailSent)
			{
				MessageCssClass = "wwErrorSuccess gsp_msgfriendly";
				msgResult = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.Admin_Gallery_Settings_Test_Email_Success_Text, user.UserName, user.Email);
			}
			else
			{
				MessageCssClass = "wwErrorFailure gsp_msgwarning";
			}

			MessageText = msgResult;

			UpdateUI();
		}

		private void UpdateUI()
		{
			if (!String.IsNullOrEmpty(MessageText))
			{
				if (MessageIsError)
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

		private void CheckForMessages()
		{
			if (Message == Message.SettingsSuccessfullyChanged)
			{
				MessageText = Resources.GalleryServerPro.Admin_Save_Success_Text;
			}

			// Check for a second situation we might need to tell the user about. Since we don't want to overwrite the first message,
			// we'll just directly update the second message control on the page.
			if (!GallerySettingsUpdateable.ShowHeader && GallerySettingsUpdateable.AllowManageOwnAccount)
			{
				wwMessage2.CssClass = "wwErrorSuccess gsp_msgattention";
				wwMessage2.ShowMessage(Resources.GalleryServerPro.Admin_Gallery_Settings_Cannot_Display_Account_Edit_Link_Msg);
			}
		}

		#endregion
	}
}