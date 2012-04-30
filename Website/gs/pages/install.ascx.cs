using System;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Web.UI;
using System.Web.UI.WebControls;
using GalleryServerPro.Business;
using GalleryServerPro.ErrorHandler.CustomExceptions;
using GalleryServerPro.Web.Controller;
using GalleryServerPro.Web.Entity;
using GalleryServerPro.Web.Sql;

namespace GalleryServerPro.Web.Pages
{
	#region Enum declarations

	/// <summary>
	/// Represents a page of the install wizard.
	/// </summary>
	public enum WizardPanel
	{
		Welcome,
		License,
		DataProvider,
		DbAdmin,
		ChooseDb,
		DbRuntime,
		SetupOptions,
		GsAdmin,
		ReadyToInstall,
		Finished,
	}

	/// <summary>
	/// Indicates a version of SQL Server.
	/// </summary>
	public enum SqlVersion
	{
		Unknown = 0,
		PreSql2000 = 1,
		Sql2000 = 2,
		Sql2005 = 3,
		Sql2008 = 4,
		PostSql2008 = 5
	}

	#endregion


	/// <summary>
	/// A page-like user control that handles the installation.
	/// </summary>
	public partial class install : UserControl
	{
		#region Private Fields

		private bool _webConfigSuccessfullyUpdated;
		private SqlServerHelper _sqlHelper;
		private bool _appRestarted;

		protected const string SQL_SERVER_CN_STRING_NAME = "SqlServerDbConnection"; // Name of Sql Server connection string in web.config. Note that if you change this,
		// also change it in the Installer_Finished_WebCfg_Need_Updating_Dtl resource setting.
		private const string APP_NAME = "Gallery Server Pro"; // The application name to be specified in the connection string

		#region SQL Scripts

		private const string AddSqlLogin = @"
/* Add SQL login to SQL 2005 and higher. Add login to database. */
DECLARE @UserName varchar(200)
DECLARE @Password varchar(200)
DECLARE @DbName varchar(200)
SET @UserName = N'#GalleryServerWebUserName#'
SET @Password = N'#GalleryServerWebUserPwd#'
SET @DbName = N'#DbName#'

BEGIN TRAN

/* Create login if it doesn't exist */
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = @UserName AND type = 'S')
	EXEC ('CREATE LOGIN [' + @UserName + '] WITH PASSWORD=''' + @Password + ''', DEFAULT_DATABASE=[' + @DbName + ']')

IF @@ERROR > 0
BEGIN
	ROLLBACK TRAN
	RETURN
END
	
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = @UserName AND type = 'S')
	EXEC ('CREATE USER [' + @UserName + '] FOR LOGIN [' + @UserName + ']')
	
IF @@ERROR > 0
BEGIN
	ROLLBACK TRAN
	RETURN
END

COMMIT TRAN

GO
";

		private const string AddSqlWindowsLogin = @"
/* Add Windows login to SQL 2005 and higher. Add login to database. */
DECLARE @UserName varchar(200)
DECLARE @DbName varchar(200)
SET @UserName = N'#GalleryServerWebUserName#'
SET @DbName = N'#DbName#'

BEGIN TRAN

/* Create login if it doesn't exist */
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = @UserName AND type = 'U')
	EXEC ('CREATE LOGIN [' + @UserName + '] FROM WINDOWS WITH DEFAULT_DATABASE=[' + @DbName + ']')

IF @@ERROR > 0
BEGIN
	ROLLBACK TRAN
	RETURN
END

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = @UserName AND type = 'U')
	EXEC ('CREATE USER [' + @UserName + '] FOR LOGIN [' + @UserName + ']')

IF @@ERROR > 0
BEGIN
	ROLLBACK TRAN
	RETURN
END

COMMIT TRAN

GO
";

		#endregion

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets a reference to an object that can assist with SQL Server execution and other management.
		/// </summary>
		/// <value>The SQL helper.</value>
		public SqlServerHelper Sql
		{
			get
			{
				if (_sqlHelper == null)
				{
					_sqlHelper = new SqlServerHelper(GetDbAdminConnectionString(true));
				}

				return _sqlHelper;
			}
		}

		/// <summary>
		/// Gets or sets the current wizard panel.
		/// </summary>
		/// <value>The current wizard panel.</value>
		public WizardPanel CurrentWizardPanel
		{
			get
			{
				if (ViewState["WizardPanel"] != null)
					return (WizardPanel)ViewState["WizardPanel"];

				return WizardPanel.Welcome;
			}
			set
			{
				ViewState["WizardPanel"] = value;
			}
		}

		/// <summary>
		/// Gets or sets the DB admin password.
		/// </summary>
		/// <value>The DB admin password.</value>
		protected string DbAdminPassword
		{
			get
			{
				string dbPwd = ViewState["dbAdminPwd"] as string;
				if (dbPwd == null)
				{
					dbPwd = String.Empty;
					ViewState["dbAdminPwd"] = dbPwd;
				}
				return dbPwd;
			}
			set
			{
				ViewState["dbAdminPwd"] = value;
			}
		}

		/// <summary>
		/// Gets or sets the DB runtime password.
		/// </summary>
		/// <value>The DB runtime password.</value>
		protected string DbRuntimePassword
		{
			get
			{
				string dbPwd = ViewState["dbRuntimePwd"] as string;
				if (dbPwd == null)
				{
					dbPwd = String.Empty;
					ViewState["dbRuntimePwd"] = dbPwd;
				}
				return dbPwd;
			}
			set
			{
				ViewState["dbRuntimePwd"] = value;
			}
		}

		/// <summary>
		/// Gets or sets the admin password.
		/// </summary>
		/// <value>The admin password.</value>
		protected string GsAdminPassword
		{
			get
			{
				string gsPwd = ViewState["gsAdminPwd"] as string;
				if (gsPwd == null)
				{
					gsPwd = String.Empty;
					ViewState["gsAdminPwd"] = gsPwd;
				}
				return gsPwd;
			}
			set
			{
				ViewState["gsAdminPwd"] = value;
			}
		}

		/// <summary>
		/// Gets the IIS application pool identity.
		/// </summary>
		/// <value>The IIS application pool identity.</value>
		protected static string IisIdentity
		{
			get
			{
				WindowsIdentity identity = WindowsIdentity.GetCurrent();
				return (identity != null ? identity.Name : String.Empty);
			}
		}

		/// <summary>
		/// Gets the provider DB.
		/// </summary>
		/// <value>The provider DB.</value>
		protected ProviderDataStore ProviderDb
		{
			get
			{
				return (rbDataProviderSqlCe.Checked ? ProviderDataStore.SqlCe : ProviderDataStore.SqlServer);
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the wizard was successfully disabled.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the wizard was successfully disabled; otherwise, <c>false</c>.
		/// </value>
		public bool WizardsSuccessfullyDisabled
		{
			get
			{
				if (ViewState["UpgradeWizardSuccessfullyDisabled"] != null)
					return (bool)ViewState["UpgradeWizardSuccessfullyDisabled"];

				return false;
			}
			set
			{
				ViewState["UpgradeWizardSuccessfullyDisabled"] = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the install file was successfully deleted.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the install file was successfully deleted; otherwise, <c>false</c>.
		/// </value>
		public bool InstallFileSuccessfullyDeleted
		{
			get
			{
				if (ViewState["InstallFileSuccessfullyDeleted"] != null)
					return (bool)ViewState["InstallFileSuccessfullyDeleted"];

				return false;
			}
			set
			{
				ViewState["InstallFileSuccessfullyDeleted"] = value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the installation was successful.
		/// </summary>
		/// <value><c>true</c> if the installation was successful; otherwise, <c>false</c>.</value>
		public bool InstallSuccessful
		{
			get
			{
				return _webConfigSuccessfullyUpdated && InstallFileSuccessfullyDeleted;
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
			bool setupEnabled;
			if (Boolean.TryParse(ENABLE_SETUP.Value, out setupEnabled) && setupEnabled)
			{
				if (!Page.IsPostBack)
				{
					// Make sure the App_Data directory is writeable. We might need to write a file here as part of the installation.
					HelperFunctions.ValidatePhysicalPathExistsAndIsReadWritable(Server.MapPath(Path.Combine(Utils.AppRoot, GlobalConstants.AppDataDirectory)));

					SetCurrentPanel(WizardPanel.Welcome, Welcome);
				}

				ConfigureControls();
			}
			else
			{
				Response.Write(String.Format(CultureInfo.CurrentCulture, "<h1>{0}</h1>", Resources.GalleryServerPro.Installer_Disabled_Msg));
				Response.Flush();
				Response.End();
			}
		}

		/// <summary>
		/// Handles the ServerValidate event of the cvDbAdminSqlLogOn control.
		/// </summary>
		/// <param name="source">The source of the event.</param>
		/// <param name="args">The <see cref="System.Web.UI.WebControls.ServerValidateEventArgs"/> instance containing the event data.</param>
		protected void cvDbAdminSqlLogOn_ServerValidate(object source, ServerValidateEventArgs args)
		{
			if ((rblDbAdminConnectType.SelectedIndex == 1) && (String.IsNullOrEmpty(txtDbAdminUserName.Text)))
				args.IsValid = false;
			else
				args.IsValid = true;
		}

		/// <summary>
		/// Handles the ServerValidate event of the cvDbAdminSqlPassword control.
		/// </summary>
		/// <param name="source">The source of the event.</param>
		/// <param name="args">The <see cref="System.Web.UI.WebControls.ServerValidateEventArgs"/> instance containing the event data.</param>
		protected void cvDbAdminSqlPassword_ServerValidate(object source, ServerValidateEventArgs args)
		{
			if ((rblDbAdminConnectType.SelectedIndex == 1) && (String.IsNullOrEmpty(txtDbAdminPassword.Text)))
				args.IsValid = false;
			else
				args.IsValid = true;
		}

		/// <summary>
		/// Handles the ServerValidate event of the cvDbRuntimeSqlLogOn control.
		/// </summary>
		/// <param name="source">The source of the event.</param>
		/// <param name="args">The <see cref="System.Web.UI.WebControls.ServerValidateEventArgs"/> instance containing the event data.</param>
		protected void cvDbRuntimeSqlLogOn_ServerValidate(object source, ServerValidateEventArgs args)
		{
			if ((rblDbRuntimeConnectType.SelectedIndex == 2) && (String.IsNullOrEmpty(txtDbRuntimeUserName.Text)))
				args.IsValid = false;
			else
				args.IsValid = true;
		}

		/// <summary>
		/// Handles the ServerValidate event of the cvDbRuntimeSqlPassword control.
		/// </summary>
		/// <param name="source">The source of the event.</param>
		/// <param name="args">The <see cref="System.Web.UI.WebControls.ServerValidateEventArgs"/> instance containing the event data.</param>
		protected void cvDbRuntimeSqlPassword_ServerValidate(object source, ServerValidateEventArgs args)
		{
			if ((rblDbRuntimeConnectType.SelectedIndex == 2) && (String.IsNullOrEmpty(txtDbRuntimePassword.Text)))
				args.IsValid = false;
			else
				args.IsValid = true;
		}

		/// <summary>
		/// Handles the Click event of the btnNext control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void btnNext_Click(object sender, EventArgs e)
		{
			if (Page.IsValid)
				ShowNextPanel();
		}

		/// <summary>
		/// Handles the Click event of the btnPrevious control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void btnPrevious_Click(object sender, EventArgs e)
		{
			ShowPreviousPanel();
		}

		#endregion

		#region Private Methods

		private void ConfigureControls()
		{
			if (!IsPostBack)
				ConfigureControlsFirstTime();

			Page.Form.DefaultFocus = btnNext.ClientID;

			#region Configure Validators

			//TODO: Figure out how to get the minimum password length without accessing the db. May not be possible.
			//string validationMsg = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.Installer_GsAdmin_Pwd_Min_Length_Msg, UserController.MinRequiredPasswordLength);
			string validationMsg = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.Installer_GsAdmin_Pwd_Min_Length_Msg, 3);
			//rfvGsAdminPassword.ErrorMessage = validationMsg;

			//regGsAdminPassword.ValidationExpression = String.Format(CultureInfo.InvariantCulture, @"\S{{{0},128}}", UserController.MinRequiredPasswordLength);
			regGsAdminPassword.ValidationExpression = String.Format(CultureInfo.InvariantCulture, @".{{{0},128}}", 3);
			regGsAdminPassword.ErrorMessage = validationMsg;

			#endregion
		}

		private void ConfigureControlsFirstTime()
		{
			// Make sure user is logged off. If the user remains logged in during installation, he may end up with an "Insufficient Permission"
			// error after it is complete, with no access to any gallery objects.
			UserController.LogOffUser();

			rblDbAdminConnectType.Items.Add(new ListItem(String.Format(CultureInfo.CurrentCulture, "{0} ({1})", Resources.GalleryServerPro.Installer_DbAdmin_Connect_Type_Item1, IisIdentity), "0"));
			rblDbAdminConnectType.Items.Add(new ListItem(Resources.GalleryServerPro.Installer_DbAdmin_Connect_Type_Item2, "1"));
			rblDbAdminConnectType.SelectedIndex = 0;

			rblDbRuntimeConnectType.Items.Add(new ListItem(Resources.GalleryServerPro.Installer_DbRuntime_Connect_Type_Item1, "0"));
			rblDbRuntimeConnectType.Items.Add(new ListItem(String.Format(CultureInfo.CurrentCulture, "{0} ({1})", Resources.GalleryServerPro.Installer_DbRuntime_Connect_Type_Item2, IisIdentity), "1"));
			rblDbRuntimeConnectType.Items.Add(new ListItem(Resources.GalleryServerPro.Installer_DbRuntime_Connect_Type_Item3, "2"));
			rblDbRuntimeConnectType.SelectedIndex = 0;

			string version = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.Footer_Gsp_Version_Text, Utils.GetGalleryServerVersion());
			litVersion.Text = version;
		}

		private void SetCurrentPanel(WizardPanel panel, Control controlToShow)
		{
			Panel currentPanel = wizCtnt.FindControl(CurrentWizardPanel.ToString()) as Panel;
			if (currentPanel != null)
				currentPanel.Visible = false;

			switch (panel)
			{
				case WizardPanel.Welcome:
					btnPrevious.Enabled = false;
					License.Visible = false;
					break;
				case WizardPanel.Finished:
					btnNext.Enabled = false;
					btnPrevious.Enabled = false;
					break;
				default:
					btnPrevious.Enabled = true;
					btnNext.Enabled = true;
					break;
			}

			controlToShow.Visible = true;
			CurrentWizardPanel = panel;
		}

		private void ShowNextPanel()
		{
			switch (CurrentWizardPanel)
			{
				case WizardPanel.Welcome:
					{
						SetCurrentPanel(WizardPanel.License, License);
						break;
					}
				case WizardPanel.License:
					{
						if (chkLicenseAgreement.Checked)
						{
							ConfigureDbEngineChoices();
							SetCurrentPanel(WizardPanel.DataProvider, DataProvider);
						}
						break;
					}
				case WizardPanel.DataProvider:
					{
						if (ValidateDataProvider())
						{
							if (rbDataProviderSqlCe.Checked)
								SetCurrentPanel(WizardPanel.GsAdmin, GsAdmin);
							else
								SetCurrentPanel(WizardPanel.DbAdmin, DbAdmin);
						}
						break;
					}
				case WizardPanel.DbAdmin:
					{
						DbAdminPassword = txtDbAdminPassword.Text;
						try
						{
							BindDatabaseDropdownlist();
							SetCurrentPanel(WizardPanel.ChooseDb, ChooseDb);
						}
						catch (Exception ex)
						{
							ShowErrorMsgThatOccurredDuringInstallation(ex.Message, null, null, lblErrMsgDbAdmin, null, null);
						}
						break;
					}
				case WizardPanel.ChooseDb:
					{
						try
						{
							ValidateChooseDb();

							SetCurrentPanel(WizardPanel.DbRuntime, DbRuntime);
						}
						catch (Exception ex)
						{
							ShowErrorMsgThatOccurredDuringInstallation(ex.Message, null, null, lblErrMsgChooseDb, null, null);
						}
						break;
					}
				case WizardPanel.DbRuntime:
					{
						DbRuntimePassword = txtDbRuntimePassword.Text;

						try
						{
							ValidateRuntimeLogin();

							SetCurrentPanel(WizardPanel.SetupOptions, SetupOptions);
						}
						catch (Exception ex)
						{
							string msg = String.Concat(Resources.GalleryServerPro.Installer_Cannot_Validate_User_Msg, " ", ex.Message);
							ShowErrorMsgThatOccurredDuringInstallation(msg, null, null, lblErrMsgDbRuntime, null, null);
						}

						break;
					}
				case WizardPanel.SetupOptions:
					{
						SetCurrentPanel(WizardPanel.GsAdmin, GsAdmin);
						break;
					}
				case WizardPanel.GsAdmin:
					{
						GsAdminPassword = txtGsAdminPassword.Text;
						SetCurrentPanel(WizardPanel.ReadyToInstall, ReadyToInstall);
						break;
					}
				case WizardPanel.ReadyToInstall:
					{
						try
						{
							ExecuteInstallation();

							ConfigureFinishedControls();

							SetCurrentPanel(WizardPanel.Finished, Finished);
						}
						catch (Exception ex)
						{
							string sql = null;
							if (ex.Data.Contains(SqlServerHelper.ExceptionDataId))
								sql = ex.Data[SqlServerHelper.ExceptionDataId].ToString();

							ShowErrorMsgThatOccurredDuringInstallation(ex.Message, sql, ex.StackTrace, lblErrMsgReadyToInstall, lblErrMsgReadyToInstallSql, lblErrMsgReadyToInstallCallStack);
						}

						break;
					}
			}
		}

		private void ConfigureFinishedControls()
		{
			if (InstallSuccessful)
			{
				imgFinishedIcon.ImageUrl = Utils.GetUrl("/images/ok_26x26.png");
				imgFinishedIcon.Width = Unit.Pixel(26);
				imgFinishedIcon.Height = Unit.Pixel(26);
				l61.Text = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServerPro.Installer_Finished_No_Addl_Steps_Reqd, Utils.GetCurrentPageUrl());
			}
			else
			{
				imgFinishedIcon.ImageUrl = Utils.GetUrl("/images/warning_32x32.png");
				imgFinishedIcon.Width = Unit.Pixel(32);
				imgFinishedIcon.Height = Unit.Pixel(32);
				l61.Text = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServerPro.Installer_Finished_Addl_Steps_Reqd, Utils.GetCurrentPageUrl());
			}

			pnlWebConfigNeedUpdating.Visible = !_webConfigSuccessfullyUpdated;
			pnlAppCouldNotBeRestarted.Visible = !_appRestarted;
			pnlInstallFileNeedsDeleting.Visible = !InstallFileSuccessfullyDeleted;

			if (WizardsSuccessfullyDisabled)
			{
				lblWizardDisableMsg.Text = Resources.GalleryServerPro.Installer_Finished_WizardsDisabled_Msg;
			}
			else
			{
				lblWizardDisableMsg.Text = Resources.GalleryServerPro.Installer_Finished_NeedToDisableWizard_Msg;
			}
		}

		/// <summary>
		/// Check web.config to make sure they contain references to the SQL CE and SQL Server providers.
		/// Disable the radio button if it is missing configuration info. This *does not* check whether any 
		/// particular provider is selected, nor does it verify the installer has permission to edit the file; that is done
		/// in the ValidateDataProvider method.
		/// </summary>
		private void ConfigureDbEngineChoices()
		{
			if (WebConfigController.AreProvidersAvailableInWebConfig(ProviderDataStore.SqlCe))
			{
				// Web.config contain references to SQL CE providers, so it is a valid choice. Select it, unless one 
				// of the radio buttons is already selected.
				if (!rbDataProviderSqlCe.Checked && !rbDataProviderSqlServer.Checked)
				{
					rbDataProviderSqlCe.Checked = true;
				}
			}
			else
			{
				rbDataProviderSqlCe.Enabled = false;
			}

			if (WebConfigController.AreProvidersAvailableInWebConfig(ProviderDataStore.SqlServer))
			{
				// Web.config contains references to SQL Server providers, so it is a valid choice. 
				// Select it, unless one of the radio buttons is already selected.
				if (!rbDataProviderSqlCe.Checked && !rbDataProviderSqlServer.Checked)
				{
					rbDataProviderSqlServer.Checked = true;
				}
			}
			else
			{
				rbDataProviderSqlServer.Enabled = false;
			}

			if ((!rbDataProviderSqlCe.Enabled) && (rbDataProviderSqlServer.Enabled))
			{
				// SQL CE is disabled and SQL Server is enabled. Give user message why.
				lblErrMsgChooseDbEngine.InnerText = Resources.GalleryServerPro.Installer_DataProvider_SqlCe_Not_Available_Msg;
				lblErrMsgChooseDbEngine.Attributes["class"] = "gsp_msgfriendly";
				pnlDbEngineMsg.Visible = true;
			}
			else if ((rbDataProviderSqlCe.Enabled) && (!rbDataProviderSqlServer.Enabled))
			{
				// SQL CE is enabled and SQL Server is disabled. Give user message why.
				lblErrMsgChooseDbEngine.InnerText = Resources.GalleryServerPro.Installer_DataProvider_SqlServer_Not_Available_Msg;
				lblErrMsgChooseDbEngine.Attributes["class"] = "gsp_msgfriendly";
				pnlDbEngineMsg.Visible = true;
			}
			else if ((!rbDataProviderSqlCe.Enabled) && (!rbDataProviderSqlServer.Enabled))
			{
				// Both SQL CE and SQL Server are disabled. Give user message why.
				lblErrMsgChooseDbEngine.InnerText = Resources.GalleryServerPro.Installer_DataProvider_SqlCe_And_SqlServer_Not_Available_Msg;
				lblErrMsgChooseDbEngine.Attributes["class"] = "gsp_msgwarning";
				pnlDbEngineMsg.Visible = true;
				btnNext.Enabled = false;
			}
		}

		/// <summary>
		/// Using the information gathered from the user, execute the installation.
		/// </summary>
		private void ExecuteInstallation()
		{
			if (rbDataProviderSqlCe.Checked)
			{
				ExecuteSqlCeInstallation();
			}
			else
			{
				ExecuteSqlServerInstallation();
			}

			SetFlagForMembershipConfiguration();

			DeleteInstallFileTrigger();

			DeleteGalleryServerProConfigFile();

			DisableInstallAndUpgradeWizards();
		}

		private void ExecuteSqlCeInstallation()
		{
			UpdateWebConfigFile();
		}

		/// <summary>
		/// We need to set up the sys admin role and user, but we can't do this at the moment because web.config may have
		/// been updated and it won't take effect until the app restarts. We want to use the Membership and Role API to 
		/// configure the membership rather than update the tables directly. So instead we write a small file to the 
		/// App_Data directory that will be noticed at the next application restart. This file will trigger the code 
		/// to create the role and user for us.
		/// </summary>
		private void SetFlagForMembershipConfiguration()
		{
			string filePath = Path.Combine(Request.PhysicalApplicationPath, Path.Combine(GlobalConstants.AppDataDirectory, GlobalConstants.InstallMembershipFileName));

			File.Delete(filePath);

			using (StreamWriter sw = File.CreateText(filePath))
			{
				sw.WriteLine(txtGsAdminUserName.Text);
				sw.WriteLine(GsAdminPassword);
				sw.WriteLine(txtGsAdminEmail.Text);
			}
		}

		private void ExecuteSqlServerInstallation()
		{
			if (chkScriptMembership.Checked)
			{
				ConfigureAspNetMembership();
			}

			ConfigureGalleryServerSchemaForSqlServer();

			ConfigureRuntimeLogin();

			UpdateWebConfigFile();
		}

		private void ConfigureRuntimeLogin()
		{
			#region Step 1: Ensure login exists in the database

			// Can the account log in to the data server?
			if (!CanConnectToDatabase(GetDbRuntimeConnectionString(true)))
			{
				// Account does not exist or is not valid. Create it.
				Sql.ExecuteSql(GetAddSqlUserScript());
			}

			#endregion

			#region Step 2: Grant login appropriate permission to database objects

			// The SQL role named gs_GalleryServerProRole was configured in the script InstallGalleryServerPro2000.sql 
			// (or InstallGalleryServerPro2005.sql for SQL Server 2005 and later) to have all appropriate permissions, 
			// so all we have to do is add the user to this SQL role. If the user is sa, do not add it to the role because
			// it already has permission (trying to do so throws the error "User or role 'sa' does not exist in this database.")
			if (!GetRuntimeSqlAccountName().Equals("sa", StringComparison.OrdinalIgnoreCase))
			{
				string sql = String.Format(CultureInfo.CurrentCulture, "EXEC sp_addrolemember @rolename=N'gs_GalleryServerProRole', @membername=N'{0}'", GetRuntimeSqlAccountName());

				try
				{
					Sql.ExecuteSql(sql);
				}
				catch
				{
					// An error occurred adding the user to the role. This can happen if the user is the database owner. Let's check
					// for this condition, and if she is, then it's ok that the SQL failed, and we can continue.
					using (SqlConnection cn = new SqlConnection(GetDbRuntimeConnectionString(true)))
					{
						bool isOwner = IsInSqlRole(cn, "db_owner");
						if (!isOwner)
							throw;
					}
				}
			}
			#endregion
		}

		private void UpdateWebConfigFile()
		{
			try
			{
				bool webConfigNeedsUpdating = false;

				WebConfigEntity webConfigEntity = WebConfigController.GetWebConfigEntity();

				if (ProviderDb == ProviderDataStore.SqlServer)
				{
					// Update SQL Server connection string. (The SQL CE connection string never changes, so we don't have to update it.)
					string cnString = GetDbRuntimeConnectionString(true);
					webConfigNeedsUpdating = (webConfigEntity.SqlServerConnectionStringValue != cnString);
					webConfigEntity.SqlServerConnectionStringValue = cnString;
				}

				if (!AreProvidersSpecifiedInWebConfig())
				{
					// Update membership, role, and profile providers.
					webConfigEntity.MembershipDefaultProvider = (ProviderDb == ProviderDataStore.SqlCe ? MembershipDataProvider.SqlCeMembershipProvider : MembershipDataProvider.SqlMembershipProvider);
					webConfigEntity.RoleDefaultProvider = (ProviderDb == ProviderDataStore.SqlCe ? RoleDataProvider.SqlCeRoleProvider : RoleDataProvider.SqlRoleProvider);
					webConfigEntity.GalleryDataDefaultProvider = (ProviderDb == ProviderDataStore.SqlCe ? GalleryDataProvider.SqlCeGalleryServerProProvider : GalleryDataProvider.SqlServerGalleryServerProProvider);

					webConfigNeedsUpdating = true;
				}

				if (webConfigNeedsUpdating)
				{
					WebConfigController.Save(webConfigEntity);
					_appRestarted = true;
				}
				else
				{
					try
					{
						WebConfigController.Touch();
						_appRestarted = true;
					}
					catch (UnauthorizedAccessException) { }
				}

				_webConfigSuccessfullyUpdated = true;
			}
			catch (Exception ex)
			{
				// Record exception and swallow; we will be able to notice this failed because the _webConfigSuccessfullyUpdated flag remains false.
				try { ErrorHandler.Error.Record(ex); }
				catch { }
			}
		}

		/// <summary>
		/// Verify that the specified Win/SQL login can used as the runtime account for Gallery Server Pro.	An exception is
		/// thrown if the account cannot be used.</summary>
		private void ValidateRuntimeLogin()
		{
			string sql = String.Empty;

			#region Test 1: Try to connect

			if (CanConnectToDatabase(GetDbRuntimeConnectionString(false)))
				return; // Login account already exists and we're able to log in, so no further testing is needed.

			#endregion

			#region Test 2: If user specified a SQL login, see if it exists

			SqlVersion sqlVersion = GetSqlVersion();
			if (rblDbRuntimeConnectType.SelectedIndex == 2) // SQL login
			{
				// See if SQL login exists.
				bool loginExists = false;
				switch (sqlVersion)
				{
					case SqlVersion.Sql2005:
					case SqlVersion.Sql2008:
					case SqlVersion.PostSql2008: sql = "SELECT COUNT(*) FROM sys.server_principals WHERE name = @UserName AND type = 'S'"; break;
				}

				using (SqlConnection cn = new SqlConnection(GetDbAdminConnectionString(false)))
				{
					using (SqlCommand cmd = new SqlCommand(sql, cn))
					{
						cmd.Parameters.Add(new SqlParameter("@UserName", txtDbRuntimeUserName.Text));
						cn.Open();
						using (System.Data.IDataReader dr = cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection))
						{
							if (dr.Read() && (dr.GetInt32(0) > 0))
							{
								loginExists = true;
							}
						}
					}
				}

				if (loginExists)
				{
					// If we get here that means the SQL login exists but the user-supplied password is incorrect. Show error and exit.
					string msg = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.Installer_Invalid_Pwd_For_Existing_Sql_Login_Msg, txtDbRuntimeUserName.Text);
					throw new WebException(msg);
				}
			}

			#endregion

			#region Test 3: Run the add user script wrapped in a rollback transaction

			sql = String.Format(CultureInfo.CurrentCulture, "BEGIN TRAN {0} ROLLBACK TRAN", GetAddSqlUserScript());

			Sql.ExecuteSql(sql);

			#endregion
		}

		private void ShowErrorMsgThatOccurredDuringInstallation(string errorMsg, string sqlThatCausedError, string callStack,
			System.Web.UI.HtmlControls.HtmlGenericControl errorMsgControl,
			System.Web.UI.HtmlControls.HtmlGenericControl errorSqlControl,
			System.Web.UI.HtmlControls.HtmlGenericControl errorCallStackControl)
		{
			if (errorMsgControl == null)
				throw new ArgumentNullException("errorMsgControl");

			#region Show error message

			if (!String.IsNullOrEmpty(errorMsg))
			{
				errorMsgControl.InnerHtml = errorMsg;
				errorMsgControl.Attributes["class"] = "gsp_msgwarning gsp_visible";
			}
			else
			{
				errorMsgControl.InnerHtml = String.Empty;
				errorMsgControl.Attributes["class"] = "gsp_invisible";
			}

			#endregion

			#region Show SQL

			if (errorSqlControl != null)
			{
				if (!String.IsNullOrEmpty(sqlThatCausedError))
				{
					errorSqlControl.InnerHtml = String.Format(CultureInfo.CurrentCulture, "<span class='gsp_bold'>{0}</span> {1}", Resources.GalleryServerPro.Installer_Sql_Error_Msg, Utils.HtmlEncode(sqlThatCausedError));

					errorSqlControl.Attributes["class"] = "gsp_visible";
				}
				else
				{
					errorSqlControl.InnerHtml = String.Empty;
					errorSqlControl.Attributes["class"] = "gsp_invisible";
				}
			}

			#endregion

			#region Show callstack

			if (errorCallStackControl != null)
			{
				if (!String.IsNullOrEmpty(callStack))
				{
					errorCallStackControl.InnerHtml = String.Format(CultureInfo.CurrentCulture, "<span class='gsp_bold'>{0}</span> {1}", Resources.GalleryServerPro.Installer_Sql_Error_CallStack_Label, Utils.HtmlEncode(callStack));

					errorCallStackControl.Attributes["class"] = "gsp_visible";
				}
				else
				{
					errorCallStackControl.InnerHtml = String.Empty;
					errorCallStackControl.Attributes["class"] = "gsp_invisible";
				}
			}

			#endregion

			if ((errorMsgControl.ID == lblErrMsgReadyToInstall.ID))
			{
				lblReadyToInstallHeaderMsg.InnerText = Resources.GalleryServerPro.Installer_Install_Error_Hdr;
				lblReadyToInstallDetailMsg.Attributes["class"] = "gsp_invisible";
			}
		}

		/// <summary>
		/// Get the Windows account or SQL login the user specified to be used during runtime operation of Gallery Server.
		/// Ex: DOMAIN\SqlUser, GalleryServerWebUser
		/// </summary>
		/// <returns>Returns the Windows account or SQL login the user specified to be used during runtime operation of Gallery Server.</returns>
		private string GetRuntimeSqlAccountName()
		{
			string userName;
			if (rblDbRuntimeConnectType.SelectedIndex == 0) // Use same connection as previously specified
			{
				userName = (rblDbAdminConnectType.SelectedIndex == 0 ? IisIdentity : txtDbAdminUserName.Text);
			}
			else if (rblDbRuntimeConnectType.SelectedIndex == 1) // Use Win Authentication
			{
				userName = IisIdentity;
			}
			else // SQL Authentication
			{
				userName = txtDbRuntimeUserName.Text;
			}
			return userName;
		}

		/// <summary>
		/// Get the script that adds the user to the database. The placeholder variables in the script for user name, password, 
		/// and default database have been replaced with their actual values.
		/// </summary>
		/// <returns>Returns a script that adds the user to the database.</returns>
		private string GetAddSqlUserScript()
		{
			// <param name="sourceScriptName">The name of the script used as the template for creating the SQL. This parameter
			// can be used in any error messages should a problem occur when running the script.</param>
			string sql = String.Empty;
			bool isSqlLogin = rblDbRuntimeConnectType.SelectedIndex == 2 ? true : false;

			switch (GetSqlVersion())
			{
				case SqlVersion.Sql2005:
				case SqlVersion.Sql2008:
				case SqlVersion.PostSql2008: sql = (isSqlLogin ? AddSqlLogin : AddSqlWindowsLogin); break;
			}

			sql = sql.Replace("#GalleryServerWebUserName#", MakeSqlSafe(GetRuntimeSqlAccountName()));
			sql = sql.Replace("#GalleryServerWebUserPwd#", MakeSqlSafe(DbRuntimePassword));
			sql = sql.Replace("#DbName#", MakeSqlSafe(ddlDbList.SelectedValue));

			return sql;
		}

		private static string MakeSqlSafe(string sql)
		{
			return sql.Replace("'", "''");
		}

		private static bool CanConnectToDatabase(string cnString)
		{
			bool canConnect = false;
			try
			{
				using (SqlConnection cn = new SqlConnection(cnString))
				{
					cn.Open();
					canConnect = true;
				}
			}
			catch (InvalidOperationException) { }
			catch (SqlException) { }

			return canConnect;
		}

		private void ConfigureAspNetMembership()
		{
			string[] sqlFiles = new string[] { "InstallCommon.sql", "InstallMembership.sql", "InstallProfile.sql", "InstallRoles.sql" };

			foreach (string sqlFile in sqlFiles)
			{
				Sql.ExecuteSqlInFile(sqlFile);

				lblErrMsgReadyToInstall.InnerText = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.Installer_Sql_In_File_Error_Msg, Path.GetFileName(sqlFile), lblErrMsgReadyToInstall.InnerText);
			}
		}

		private void ConfigureGalleryServerSchemaForSqlServer()
		{
			string sqlScriptName = String.Empty;

			switch (GetSqlVersion())
			{
				case SqlVersion.Sql2005:
				case SqlVersion.Sql2008:
				case SqlVersion.PostSql2008: sqlScriptName = "InstallGalleryServerProSql2005.sql"; break;
			}

			Sql.ExecuteSqlInFile(sqlScriptName);

			lblErrMsgReadyToInstall.InnerText = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.Installer_Sql_In_File_Error_Msg, Path.GetFileName(sqlScriptName), lblErrMsgReadyToInstall.InnerText);
		}

		/// <summary>
		/// Validate that the selected database can be logged in to by the user-specified credentials and that
		/// it does not already contain Gallery Server tables. If not valid, an exception is thrown.
		/// </summary>
		private void ValidateChooseDb()
		{
			ConnectToDatabaseInAdminMode();

			if (DbContainsGalleryServerTables())
			{
				throw new WebException(Resources.GalleryServerPro.Installer_ChooseDb_Existing_Data_Found_Msg);
			}
		}

		/// <summary>
		/// Verify the SQL CE or SQL Server providers are configured in web.config. The requested provider does not have to be specified as the 
		/// defaultProvider, but if not, then make sure the installer has permission to edit the file. If the provider is not configured, 
		/// or if the installer does not have permission to edit the file, alert the user with a message.
		/// </summary>
		/// <returns>Returns <c>true</c> if the requested provider is already configured, or the installer has permission to change it if
		/// it is not configured; otherwise returns <c>false</c>.</returns>
		private bool ValidateDataProvider()
		{
			// web.config: Check to see if the selected provider is specified as the default provider or, if it is not, that the installer
			// has permission to edit it.
			if (!AreProvidersSpecifiedInWebConfig())
			{
				if (!WebConfigController.IsWebConfigUpdateable())
				{
					if (rbDataProviderSqlCe.Checked)
					{
						lblErrMsgChooseDbEngine.InnerText = Resources.GalleryServerPro.Installer_DataProvider_WebConfig_Not_UpdateableToSqlCe_InsufficientPermission;
					}
					else
					{
						lblErrMsgChooseDbEngine.InnerText = Resources.GalleryServerPro.Installer_DataProvider_WebConfig_Not_UpdateableToSqlServer_InsufficientPermission;
					}

					lblErrMsgChooseDbEngine.Attributes["class"] = "gsp_msgwarning";
					pnlDbEngineMsg.Visible = true;
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Determines whether the membership, role, and gallery data providers in web.config are configured for the desired data provider.
		/// </summary>
		/// <returns>Returns <c>true</c> if the membership, role, and gallery data providers in web.config are configured for the data provider
		/// selected by the user; otherwise returns <c>false</c>.</returns>
		private bool AreProvidersSpecifiedInWebConfig()
		{
			WebConfigEntity wce = WebConfigController.GetWebConfigEntity();

			MembershipDataProvider membershipProviderName = (ProviderDb == ProviderDataStore.SqlCe ? MembershipDataProvider.SqlCeMembershipProvider : MembershipDataProvider.SqlMembershipProvider);
			RoleDataProvider roleProviderName = (ProviderDb == ProviderDataStore.SqlCe ? RoleDataProvider.SqlCeRoleProvider : RoleDataProvider.SqlRoleProvider);
			GalleryDataProvider galleryDataProviderName = (ProviderDb == ProviderDataStore.SqlCe ? GalleryDataProvider.SqlCeGalleryServerProProvider : GalleryDataProvider.SqlServerGalleryServerProProvider);

			return ((wce.MembershipDefaultProvider == membershipProviderName)
							&& (wce.RoleDefaultProvider == roleProviderName)
							&& (wce.GalleryDataDefaultProvider == galleryDataProviderName));
		}

		private void ShowPreviousPanel()
		{
			switch (CurrentWizardPanel)
			{
				case WizardPanel.Welcome: break;
				case WizardPanel.License:
					{
						SetCurrentPanel(WizardPanel.Welcome, Welcome);
						break;
					}
				case WizardPanel.DataProvider:
					{
						SetCurrentPanel(WizardPanel.License, License);
						break;
					}
				case WizardPanel.DbAdmin:
					{
						ConfigureDbEngineChoices();
						SetCurrentPanel(WizardPanel.DataProvider, DataProvider);
						break;
					}
				case WizardPanel.ChooseDb:
					{
						SetCurrentPanel(WizardPanel.DbAdmin, DbAdmin);
						break;
					}
				case WizardPanel.DbRuntime:
					{
						SetCurrentPanel(WizardPanel.ChooseDb, ChooseDb);
						break;
					}
				case WizardPanel.SetupOptions:
					{
						SetCurrentPanel(WizardPanel.DbRuntime, DbRuntime);
						break;
					}
				case WizardPanel.GsAdmin:
					{
						if (rbDataProviderSqlCe.Checked)
						{
							ConfigureDbEngineChoices();
							SetCurrentPanel(WizardPanel.DataProvider, DataProvider);
						}
						else
							SetCurrentPanel(WizardPanel.SetupOptions, SetupOptions);
						break;
					}
				case WizardPanel.ReadyToInstall:
					{
						SetCurrentPanel(WizardPanel.GsAdmin, GsAdmin);
						break;
					}
			}
		}

		/// <summary>
		/// Get a SQL Server connection string to be used during the configuration of Gallery Server. If the includeDatabaseName
		/// parameter is true, then include the database selected during the "Choose Database" wizard step. The connection string is
		/// generated from user supplied data in one of the wizard steps.
		/// </summary>
		/// <param name="includeDatabaseName">A value indicating whether the connection string includes the database selected during
		/// the "Choose Database" wizard step.</param>
		/// <returns>Returns a connection string that can be used to connect to the specified SQL Server.</returns>
		private string GetDbAdminConnectionString(bool includeDatabaseName)
		{
			string useWinAuthentication = (rblDbAdminConnectType.SelectedIndex == 0 ? "yes" : "no");

			if (includeDatabaseName)
			{
				return String.Format(CultureInfo.CurrentCulture, "server={0};uid={1};pwd={2};Trusted_Connection={3};database={4};Application Name={5}", txtDbSqlName.Text, txtDbAdminUserName.Text, DbAdminPassword, useWinAuthentication, ddlDbList.SelectedValue, APP_NAME);
			}
			else
			{
				return String.Format(CultureInfo.CurrentCulture, "server={0};uid={1};pwd={2};Trusted_Connection={3};Application Name={4}", txtDbSqlName.Text, txtDbAdminUserName.Text, DbAdminPassword, useWinAuthentication, APP_NAME);
			}
		}

		/// <summary>
		/// Get a SQL Server connection string to be used during normal operation of Gallery Server. If the includeDatabaseName
		/// parameter is true, then include the database selected during the "Choose Database" wizard step. The connection string is
		/// generated from user supplied data in one of the wizard steps. This method generates the connection string that will be 
		/// written to the web.config file.
		/// </summary>
		/// <param name="includeDatabaseName">A value indicating whether the connection string includes the database selected during
		/// the "Choose Database" wizard step.</param>
		/// <returns>Returns a connection string that can be used to connect to the specified SQL Server.</returns>
		protected string GetDbRuntimeConnectionString(bool includeDatabaseName)
		{
			if (rblDbRuntimeConnectType.SelectedIndex == 0)
			{
				// User wants to use the same account used to configure the database.
				return GetDbAdminConnectionString(includeDatabaseName);
			}

			string useWinAuthentication = (rblDbRuntimeConnectType.SelectedIndex == 1 ? "yes" : "no");

			if (includeDatabaseName)
			{
				return String.Format(CultureInfo.CurrentCulture, "server={0};uid={1};pwd={2};Trusted_Connection={3};database={4}", txtDbSqlName.Text, txtDbRuntimeUserName.Text, DbRuntimePassword, useWinAuthentication, ddlDbList.SelectedValue);
			}
			else
			{
				return String.Format(CultureInfo.CurrentCulture, "server={0};uid={1};pwd={2};Trusted_Connection={3}", txtDbSqlName.Text, txtDbRuntimeUserName.Text, DbRuntimePassword, useWinAuthentication);
			}
		}

		private void ConnectToDatabaseInAdminMode()
		{
			try
			{
				using (SqlConnection cn = new SqlConnection(GetDbAdminConnectionString(true)))
				{
					cn.Open();
				}
			}
			catch (SqlException ex)
			{
				string errorMessage;
				switch (ex.Number)
				{
					case 4060:	// 4060 = Login failure
						if (rblDbAdminConnectType.SelectedIndex == 0)
						{
							string webAccount = IisIdentity;
							errorMessage = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.Installer_DbAdmin_WinAuth_Error_Msg, ddlDbList.SelectedValue, webAccount);
						}
						else
						{
							string msg = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.Installer_DbAdmin_SqlAuth_Error_Msg, txtDbAdminUserName.Text, ddlDbList.SelectedValue);
							errorMessage = String.Concat(msg, " ", ex.Message);
						}
						break;
					default:
						errorMessage = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.Installer_DbAdmin_SqlConnect_Error_Msg, ddlDbList.SelectedValue, ex.Number, ex.Message);
						break;
				}
				throw new WebException(errorMessage);
			}
		}

		private void BindDatabaseDropdownlist()
		{
			using (SqlConnection cn = new SqlConnection(GetDbAdminConnectionString(false)))
			{
				cn.Open();

				using (SqlCommand cmd = new SqlCommand("select name from master..sysdatabases order by name asc", cn))
				{
					using (System.Data.IDataReader dr = cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection))
					{
						ddlDbList.Items.Clear();

						while (dr.Read())
						{
							string dbName = dr["name"] as String;
							if (dbName != null)
							{
								if (!(dbName == "master" || dbName == "msdb" || dbName == "tempdb" || dbName == "model"))
								{
									ddlDbList.Items.Add(dbName); // Only add non-system databases
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Returns true if the specified database contain non-empty Gallery Server related tables. Specifically, it
		/// checks for the existence of a table named gs_Album and requests a count of its records. If there is at least
		/// one record, the function returns true. If it has zero records or the table is not found, it returns false.
		/// </summary>
		/// <returns>Returns true if the database contains a table named gs_Album and it has at least one record; otherwise
		/// returns false.</returns>
		private bool DbContainsGalleryServerTables()
		{
			bool dbContainsGalleryServerTables = false;

			using (SqlConnection cn = new SqlConnection(GetDbAdminConnectionString(true)))
			{
				cn.Open();

				try
				{
					using (SqlCommand cmd = new SqlCommand("select count(*) from gs_Album", cn))
					{
						using (System.Data.IDataReader dr = cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection))
						{
							if (dr.Read() && (dr.GetInt32(0) > 0))
							{
								dbContainsGalleryServerTables = true;
							}
						}
					}
				}
				catch { /* Swallow exception */ }
			}

			return dbContainsGalleryServerTables;
		}

		/// <summary>
		/// Indicates whether the user in the specified <paramref name="cn">connection</paramref> is a member of a
		/// Microsoft Windows group or Microsoft SQL Server database <paramref name="roleName">role</paramref>.
		/// </summary>
		/// <param name="cn">The SQL Server connection.</param>
		/// <param name="roleName">Name of the role.</param>
		/// <returns>
		/// 	<c>true</c> if the user in the connection is a member of the specified role; otherwise, <c>false</c>.
		/// </returns>
		private static bool IsInSqlRole(SqlConnection cn, string roleName)
		{
			cn.Open();

			try
			{
				using (SqlCommand cmd = new SqlCommand(String.Format(CultureInfo.InvariantCulture, "SELECT IS_MEMBER ('{0}')", roleName), cn))
				{
					return Convert.ToBoolean(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
				}
			}
			finally
			{
				cn.Close();
			}
		}

		/// <summary>
		/// Gets the version of SQL Server based on the authentication settings supplied by the user. Throws an exception if a version
		/// earlier than SQL Server 2000 is found.
		/// </summary>
		/// <returns>Returns an enumeration value that indicates the version of SQL Server the web installer is connected to.</returns>
		/// <exception cref="System.NotSupportedException">Thrown for versions of SQL Server earlier than SQL Server 2000.</exception>
		private SqlVersion GetSqlVersion()
		{
			SqlVersion version = SqlVersion.Unknown;

			using (SqlConnection cn = new SqlConnection(GetDbAdminConnectionString(false)))
			{
				using (SqlCommand cmd = new SqlCommand("SELECT SERVERPROPERTY('productversion')", cn))
				{
					cn.Open();
					using (SqlDataReader dr = cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection))
					{
						while (dr.Read())
						{
							string sqlVersion = dr.GetString(0);
							if (!String.IsNullOrEmpty(sqlVersion))
							{
								int majorVersion;
								if (Int32.TryParse(sqlVersion.Substring(0, sqlVersion.IndexOf(".", StringComparison.Ordinal)), out majorVersion))
								{
									if (majorVersion < 7) version = SqlVersion.PreSql2000;
									else if (majorVersion == 8) version = SqlVersion.Sql2000;
									else if (majorVersion == 9) version = SqlVersion.Sql2005;
									else if (majorVersion == 10) version = SqlVersion.Sql2008;
									else if (majorVersion > 10) version = SqlVersion.PostSql2008;
								}
							}
							break;
						}
					}
				}
			}

			if (version < SqlVersion.Sql2005)
				throw new NotSupportedException(Resources.GalleryServerPro.Installer_PreSql2005_Error_Msg);

			return version;
		}

		private void DeleteInstallFileTrigger()
		{
			// Note: This function is also in the install wizard page (but slightly modified).
			string installFilePath = Path.Combine(Request.PhysicalApplicationPath, Path.Combine(GlobalConstants.AppDataDirectory, GlobalConstants.InstallTriggerFileName));

			if (File.Exists(installFilePath))
			{
				try
				{
					File.Delete(installFilePath);
					InstallFileSuccessfullyDeleted = true;
				}
				catch (IOException) { }
				catch (UnauthorizedAccessException) { }
				catch (System.Security.SecurityException) { }
			}
			else
			{
				InstallFileSuccessfullyDeleted = true; // We didn't actually delete it, but it doesn't exist, so we don't want to tell the user to delete it			
			}
		}

		private void DeleteGalleryServerProConfigFile()
		{
			// Delete galleryserverpro.config file, but don't worry if it fails - the app doesn't use it anyway.
			try
			{
				File.Delete(Server.MapPath(Utils.GetUrl("/config/galleryserverpro.config")));
			}
			catch (IOException) { }
			catch (UnauthorizedAccessException) { }
			catch (System.Security.SecurityException) { }
		}

		private void DisableInstallAndUpgradeWizards()
		{
			// We want to automatically disable the install and upgrade wizards so they can't be run again.
			// Note: This function is also in the install wizard page (but slightly modified).
			string upgradeWizardFilePath = Server.MapPath(Utils.GetUrl("/pages/upgrade.ascx"));
			string installWizardFilePath = Server.MapPath(Utils.GetUrl("/pages/install.ascx"));
			string[] wizardFilePaths = new string[] { upgradeWizardFilePath, installWizardFilePath };

			bool wizardDisableFailed = false;

			foreach (string wizardFilePath in wizardFilePaths)
			{
				if (File.Exists(wizardFilePath))
				{
					try
					{
						const string hiddenFieldUpgradeEnabled = "<asp:HiddenField ID=\"ENABLE_SETUP\" runat=\"server\" Value=\"true\" />";
						const string hiddenFieldUpgradeDisabled = "<asp:HiddenField ID=\"ENABLE_SETUP\" runat=\"server\" Value=\"false\" />";

						string[] upgradeWizardFileLines = File.ReadAllLines(wizardFilePath);
						System.Text.StringBuilder newFile = new System.Text.StringBuilder();
						bool foundHiddenField = false;

						foreach (string line in upgradeWizardFileLines)
						{
							if (!foundHiddenField && line.Contains(hiddenFieldUpgradeEnabled))
							{
								newFile.AppendLine(line.Replace(hiddenFieldUpgradeEnabled, hiddenFieldUpgradeDisabled));
								foundHiddenField = true;
							}
							else
							{
								newFile.AppendLine(line);
							}
						}

						File.WriteAllText(wizardFilePath, newFile.ToString());
					}
					catch (IOException) { wizardDisableFailed = true; }
					catch (UnauthorizedAccessException) { wizardDisableFailed = true; }
					catch (System.Security.SecurityException) { wizardDisableFailed = true; }
				}
			}

			WizardsSuccessfullyDisabled = !wizardDisableFailed;
		}

		#endregion
	}
}