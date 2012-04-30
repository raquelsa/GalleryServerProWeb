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
	/// A page-like user control for administering roles.
	/// </summary>
	public partial class manageroles : Pages.AdminPage
	{
		#region Public Properties

		/// <summary>
		/// Gets a reference to the control named pnlDialogContent.
		/// </summary>
		/// <value>The pnlDialogContent control.</value>
		public Panel pnlDialogContent
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditRole, "pnlDialogContent");
				if (ctrl == null) throw new WebException("Cannot find Panel pnlDialogContent.");
				return (Panel)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named chkAdministerSite.
		/// </summary>
		/// <value>The chkAdministerSite control.</value>
		public CheckBox chkAdministerSite
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditRole, "chkAdministerSite");
				if (ctrl == null) throw new WebException("Cannot find checkbox chkAdministerSite.");
				return (CheckBox)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named chkAdministerGallery.
		/// </summary>
		/// <value>The chkAdministerGallery control.</value>
		public CheckBox chkAdministerGallery
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditRole, "chkAdministerGallery");
				if (ctrl == null) throw new WebException("Cannot find checkbox chkAdministerGallery.");
				return (CheckBox)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named chkViewObject.
		/// </summary>
		/// <value>The chkViewObject control.</value>
		public CheckBox chkViewObject
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditRole, "chkViewObject");
				if (ctrl == null) throw new WebException("Cannot find checkbox chkViewObject.");
				return (CheckBox)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named chkViewHiResImage.
		/// </summary>
		/// <value>The chkViewHiResImage control.</value>
		public CheckBox chkViewHiResImage
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditRole, "chkViewHiResImage");
				if (ctrl == null) throw new WebException("Cannot find checkbox chkViewHiResImage.");
				return (CheckBox)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named chkAddAlbum.
		/// </summary>
		/// <value>The chkAddAlbum control.</value>
		public CheckBox chkAddAlbum
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditRole, "chkAddAlbum");
				if (ctrl == null) throw new WebException("Cannot find checkbox chkAddAlbum.");
				return (CheckBox)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named chkAddMediaObject.
		/// </summary>
		/// <value>The chkAddMediaObject control.</value>
		public CheckBox chkAddMediaObject
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditRole, "chkAddMediaObject");
				if (ctrl == null) throw new WebException("Cannot find checkbox chkAddMediaObject.");
				return (CheckBox)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named chkEditAlbum.
		/// </summary>
		/// <value>The chkEditAlbum control.</value>
		public CheckBox chkEditAlbum
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditRole, "chkEditAlbum");
				if (ctrl == null) throw new WebException("Cannot find checkbox chkEditAlbum.");
				return (CheckBox)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named chkEditMediaObject.
		/// </summary>
		/// <value>The chkEditMediaObject control.</value>
		public CheckBox chkEditMediaObject
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditRole, "chkEditMediaObject");
				if (ctrl == null) throw new WebException("Cannot find checkbox chkEditMediaObject.");
				return (CheckBox)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named chkDeleteChildAlbum.
		/// </summary>
		/// <value>The chkDeleteChildAlbum control.</value>
		public CheckBox chkDeleteChildAlbum
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditRole, "chkDeleteChildAlbum");
				if (ctrl == null) throw new WebException("Cannot find checkbox chkDeleteChildAlbum.");
				return (CheckBox)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named chkDeleteMediaObject.
		/// </summary>
		/// <value>The chkDeleteMediaObject control.</value>
		public CheckBox chkDeleteMediaObject
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditRole, "chkDeleteMediaObject");
				if (ctrl == null) throw new WebException("Cannot find checkbox chkDeleteMediaObject.");
				return (CheckBox)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named chkSynchronize.
		/// </summary>
		/// <value>The chkSynchronize control.</value>
		public CheckBox chkSynchronize
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditRole, "chkSynchronize");
				if (ctrl == null) throw new WebException("Cannot find checkbox chkSynchronize.");
				return (CheckBox)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named chkHideWatermark.
		/// </summary>
		/// <value>The chkHideWatermark control.</value>
		public CheckBox chkHideWatermark
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditRole, "chkHideWatermark");
				if (ctrl == null) throw new WebException("Cannot find checkbox chkHideWatermark.");
				return (CheckBox)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named phMessage.
		/// </summary>
		/// <value>The phMessage control.</value>
		public PlaceHolder phMessage
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditRole, "phMessage");
				if (ctrl == null) throw new WebException("Cannot find PlaceHolder phMessage.");
				return (PlaceHolder)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named lblRoleName.
		/// </summary>
		/// <value>The lblRoleName control.</value>
		public Label lblRoleName
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditRole, "lblRoleName");
				if (ctrl == null) throw new WebException("Cannot find Label lblRoleName.");
				return (Label)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named txtRoleName.
		/// </summary>
		/// <value>The txtRoleName control.</value>
		public TextBox txtRoleName
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditRole, "txtRoleName");
				if (ctrl == null) throw new WebException("Cannot find TextBox txtRoleName.");
				return (TextBox)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named tvUC.
		/// </summary>
		/// <value>The tvUC control.</value>
		public GalleryServerPro.Web.Controls.albumtreeview tvUC
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditRole, "tvUC");
				if (ctrl == null) throw new WebException("Cannot find user control tvUC.");
				return (GalleryServerPro.Web.Controls.albumtreeview)ctrl;
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
				Control ctrl = this.FindControlRecursive(dgEditRole, "btnSave");
				if (ctrl == null) throw new WebException("Cannot find user control btnSave.");
				return (Button)ctrl;
			}
		}

		/// <summary>
		/// Gets a reference to the control named upEditRole.
		/// </summary>
		/// <value>The upEditRole control.</value>
		public UpdatePanel upEditRole
		{
			get
			{
				Control ctrl = this.FindControlRecursive(dgEditRole, "upEditRole");
				if (ctrl == null) throw new WebException("Cannot find user control upEditRole.");
				return (UpdatePanel)ctrl;
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
			// Set up trigger here. We can't do it declaratively because the UpdatePanel can't find the trigger control.
			AsyncPostBackTrigger trigger = new AsyncPostBackTrigger();
			trigger.ControlID = btnSave.UniqueID;
			trigger.EventName = "Click";
			upEditRole.Triggers.Add(trigger);
		}

		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			// Test #1: User must be a site or gallery admin.
			this.CheckUserSecurity(SecurityActions.AdministerSite | SecurityActions.AdministerGallery);

			if (!ScriptManager.GetCurrent(this.Page).IsInAsyncPostBack)
			{
				ConfigureControls(); // Don't run this during UpdatePanel postback
			}
		}

		protected void lbAddRole_Click(object sender, EventArgs e)
		{
			// User clicked the Add new role link. This is also invoked by javascript when a user clicks the Edit link.
			try
			{
				ConfigureRoleDialogForAddOrEdit();

				const string script = "popupInfoRender();";
				ScriptManager.RegisterStartupScript(chkAdministerGallery, typeof(CheckBox), "popupInfoRender", script, true);
			}
			catch (Exception ex)
			{
				LogError(ex);

				ShowErrorInDialog(ex, Resources.GalleryServerPro.Site_Error_Hdr);

				ConfigureAdminAccess();
			}
		}

		protected void btnSave_Click(object sender, EventArgs e)
		{
			try
			{
				string roleName = hdnRoleName.Value;
				SaveRole(roleName);

				// Empty the cache so the next request will pull them from the data store.
				HelperFunctions.PurgeCache();

				const string script = "saveRoleComplete('success');";
				ScriptManager.RegisterStartupScript(chkAdministerGallery, typeof(CheckBox), "saveRoleComplete", script, true);
			}
			catch (Exception ex)
			{
				LogError(ex);

				ShowErrorInDialog(ex, Resources.GalleryServerPro.Admin_Manage_Roles_Cannot_Save_Role_Msg);

				ConfigureAdminAccess();
			}
		}


		/// <summary>
		/// Handles the DeleteCommand event of the gdRoles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="ComponentArt.Web.UI.GridItemEventArgs"/> instance containing the event data.</param>
		protected void gdRoles_DeleteCommand(object sender, ComponentArt.Web.UI.GridItemEventArgs e)
		{
			// Delete the gallery server role. This includes the Gallery Server role and the corresponding ASP.NET role.
			// If an exception is thrown, such as during ValidateDeleteUser(), the client side function 
			// gdRoles_CallbackError will catch it and display the message to the user.
			try
			{
				string roleName = Utils.HtmlDecode(e.Item["RoleName"].ToString());

				RoleController.DeleteGalleryServerProRole(roleName);

				BindRolesGrid();
			}
			catch (Exception ex)
			{
				LogError(ex);
				throw;
			}
		}

		#endregion

		#region Private Methods

		private void ConfigureControls()
		{
			pAddRole.Visible = (UserCanEditUsersAndRoles && !AppSetting.Instance.License.IsInReducedFunctionalityMode);
			txtRoleName.MaxLength = Constants.RoleNameLength;

			if (AppSetting.Instance.License.IsInReducedFunctionalityMode)
			{
				wwMessage.ShowMessage(Resources.GalleryServerPro.Admin_Need_Product_Key_Msg2);
				wwMessage.CssClass = "wwErrorSuccess gsp_msgwarning";
			}

			this.PageTitle = Resources.GalleryServerPro.Admin_Manage_Roles_Page_Header;

			ConfigureGrid();

			RegisterJavaScript();
		}

		private void ConfigureRoleDialogForAddOrEdit()
		{
			string roleName = hdnRoleName.Value;

			SetControlVisibility(roleName);

			ConfigureAdminAccess();

			PopulateControlsWithRoleData(roleName);
		}

		private void ShowErrorInDialog(Exception ex, string messageTitle)
		{
			Controls.usermessage msgBox = (Controls.usermessage)LoadControl(Utils.GetUrl("/controls/usermessage.ascx"));
			msgBox.IconStyle = MessageStyle.Error;
			msgBox.MessageTitle = messageTitle;
			msgBox.MessageDetail = ex.Message;
			phMessage.Controls.Clear();
			phMessage.Controls.Add(msgBox);
		}

		private void ConfigureAdminAccess()
		{
			if (UserCanAdministerSite)
			{
				chkAdministerSite.Enabled = true;
				chkAdministerSite.CssClass = String.Empty;
			}
		}

		private void ConfigureGrid()
		{
			gdRoles.ImagesBaseUrl = String.Concat(Utils.GalleryRoot, "/images/componentart/grid/");
			gdRoles.PagerImagesFolderUrl = String.Concat(Utils.GalleryRoot, "/images/componentart/grid/pager/");

			if (UserCanEditUsersAndRoles && !AppSetting.Instance.License.IsInReducedFunctionalityMode)
				AddEditColumnClientTemplate();

			AddRoleNameColumnClientTemplate();

			BindRolesGrid();
		}

		private void BindRolesGrid()
		{
			gdRoles.DataSource = GetRolesCurrentUserCanView();
			gdRoles.DataBind();
		}

		private void AddEditColumnClientTemplate()
		{
			ComponentArt.Web.UI.ClientTemplate roleEditColumn = new ComponentArt.Web.UI.ClientTemplate();

			roleEditColumn.ID = "roleEditColumn";

			roleEditColumn.Text = String.Format(CultureInfo.InvariantCulture, @"<p>
						<a id=""## makeValidForId(DataItem.getMember('RoleName').get_value()) ##"" href=""javascript:editRole(decodeURI('## getRoleName(DataItem) ##'));""
							title=""{0}"">{1}</a> <a href=""javascript:hideUserMessage();if (ConfirmDelete(decodeURI('## getRoleName(DataItem) ##'))) gdRoles.deleteItem(gdRoles.getItemFromKey(0, '## getRoleNameNoEncode(DataItem) ##'));"">
								{2}</a>
					</p>",
				Resources.GalleryServerPro.Admin_Manage_Roles_Edit_Role_Tooltip_Text,
				Resources.GalleryServerPro.Admin_Manage_Roles_Edit_Role_Hyperlink_Text,
				Resources.GalleryServerPro.Admin_Manage_Roles_Delete_Role_Hyperlink_Text);

			gdRoles.ClientTemplates.Add(roleEditColumn);
		}

		private void AddRoleNameColumnClientTemplate()
		{
			ComponentArt.Web.UI.ClientTemplate roleNameColumn = new ComponentArt.Web.UI.ClientTemplate();

			roleNameColumn.ID = "roleNameColumn";

			roleNameColumn.Text = "## htmlEncode(parseRoleNameFromGspRoleName(DataItem.getMember('RoleName').get_value()).replace(/#%cLt#%/g, '<')) ##";

			gdRoles.ClientTemplates.Add(roleNameColumn);
		}

		/// <summary>
		/// Bind the checkbox list and the treeview to the specified role. If adding a new role, pass null or an empty
		/// string to the roleName parameter.
		/// </summary>
		/// <param name="roleName">The name of the role to be bound to the checkbox list and treeview, or null if adding
		/// a new role.</param>
		private void PopulateControlsWithRoleData(string roleName)
		{
			// Gets called by the Callback control when the user clicks Add or Edit for a particular role.
			// Populate the controls with data specific to this role, especially in regard to checking the appropriate
			// checkboxes to indicate the level of permission granted to this role (nothing will be checked when 
			// adding a new role).

			IGalleryServerRole role = null;
			if (!String.IsNullOrEmpty(roleName))
			{
				role = Factory.LoadGalleryServerRole(roleName);

				if (role == null)
					throw new InvalidGalleryServerRoleException();

				lblRoleName.Text = RoleController.ParseRoleNameFromGspRoleName(Utils.HtmlEncode(role.RoleName));
			}

			BindRolePermissionCheckboxes(role);

			BindAlbumTreeview(role);
		}

		/// <summary>
		/// Select (check) the permissions checkboxes corresponding to the permissions of the specified role. Specify null
		/// when adding a new role and the checkboxes will be set to their default values (unselected.)
		/// </summary>
		/// <param name="role">The Gallery Server role to be bound to the checkbox list of permissions, or null if adding
		/// a new role.</param>
		private void BindRolePermissionCheckboxes(IGalleryServerRole role)
		{
			if (role == null)
			{
				chkAddAlbum.Checked = false;
				chkAddMediaObject.Checked = false;
				chkAdministerSite.Checked = false;
				chkAdministerGallery.Checked = false;
				chkDeleteChildAlbum.Checked = false;
				chkDeleteMediaObject.Checked = false;
				chkEditAlbum.Checked = false;
				chkEditMediaObject.Checked = false;
				chkSynchronize.Checked = false;
				chkViewHiResImage.Checked = false;
				chkViewObject.Checked = false;
				chkHideWatermark.Checked = false;
			}
			else
			{
				chkAddAlbum.Checked = role.AllowAddChildAlbum;
				chkAddMediaObject.Checked = role.AllowAddMediaObject;
				chkAdministerSite.Checked = role.AllowAdministerSite;
				chkAdministerGallery.Checked = role.AllowAdministerGallery;
				chkDeleteChildAlbum.Checked = role.AllowDeleteChildAlbum;
				chkDeleteMediaObject.Checked = role.AllowDeleteMediaObject;
				chkEditAlbum.Checked = role.AllowEditAlbum;
				chkEditMediaObject.Checked = role.AllowEditMediaObject;
				chkSynchronize.Checked = role.AllowSynchronize;
				chkViewHiResImage.Checked = role.AllowViewOriginalImage;
				chkViewObject.Checked = role.AllowViewAlbumOrMediaObject;
				chkHideWatermark.Checked = role.HideWatermark;
			}
		}

		/// <summary>
		/// Fill the treeview with all albums. All nodes representing albums for which the specified role has permission
		/// will be checked. If the overload that doesn't take a role parameter is used, then check all checkboxes if the
		/// isAdministratorChecked parameter is true.
		/// </summary>
		/// <param name="role">The role to be updated. If adding a new role, then set this parameter to null.</param>
		private void BindAlbumTreeview(IGalleryServerRole role)
		{
			bool isAdmin = ((role != null) && (role.AllowAdministerSite));
			BindAlbumTreeview(role, isAdmin);
		}

		/// <summary>
		/// Fill the treeview with all albums. All nodes representing albums for which the specified role has permission
		/// will be checked. If the overload that doesn't take a role parameter is used, then check all checkboxes if the
		/// isAdministratorChecked parameter is true.
		/// </summary>
		/// <param name="role">The role to be updated. If adding a new role, then set this parameter to null.</param>
		/// <param name="isAdministrator">Indicates whether the administrator permission checkbox has been
		/// checked or the specified role has administrative permission. Since administrative permission applies to all 
		/// albums, when this parameter is true, all checkboxes for all albums will be checked. An exception is thrown
		/// if the role.AllowAdministerSite property and the isAdministrator parameter do not match.</param>
		private void BindAlbumTreeview(IGalleryServerRole role, bool isAdministrator)
		{
			if ((role != null) && (role.AllowAdministerSite != isAdministrator))
			{
				throw new ArgumentException("Invalid arguments passed to BindAlbumTreeview method: The role.AllowAdministerSite property and the isAdministrator parameter must match.");
			}

			if (role != null) // Role will be null when user is adding a new role
			{
				IIntegerCollection albumIds = tvUC.AlbumIdsToCheck;
				albumIds.Clear();
				albumIds.AddRange(role.RootAlbumIds);

				foreach (IGallery gallery in Factory.LoadGalleries())
				{
					IAlbum rootAlbum = Factory.LoadRootAlbumInstance(gallery.GalleryId);

					if (role.RootAlbumIds.Contains(rootAlbum.Id))
					{
						// The role applies to all albums. Since the treeview initially renders to two levels, we need
						// to add the album IDs for the root album's child albums.
						foreach (IGalleryObject album in rootAlbum.GetChildGalleryObjects(GalleryObjectType.Album))
						{
							albumIds.Add(album.Id);
						}
					}
				}
			}

			tvUC.RequiredSecurityPermissions = SecurityActions.AdministerSite | SecurityActions.AdministerGallery;
			tvUC.Galleries = Factory.LoadGalleries();
			tvUC.RootAlbumPrefix = String.Concat(Resources.GalleryServerPro.Site_Gallery_Text, " '{GalleryDescription}': ");
			tvUC.BindTreeView();
		}

		private void SetControlVisibility(string roleName)
		{
			if (String.IsNullOrEmpty(roleName))
			{
				lblRoleName.Visible = false;
				txtRoleName.Visible = true;
				txtRoleName.Text = String.Empty;
			}
			else
			{
				lblRoleName.Visible = true;
				txtRoleName.Visible = false;
			}
		}

		private void SaveRole(string roleName)
		{
			// Gets called by the Callback control when the user clicks Save.
			this.CheckUserSecurity(SecurityActions.AdministerSite | SecurityActions.AdministerGallery);

			try
			{
				if (String.IsNullOrEmpty(roleName))
				{
					AddNewRole();
				}
				else
				{
					UpdateExistingRole(roleName);
				}
			}
			finally
			{
				HelperFunctions.PurgeCache();
			}
		}

		private void UpdateExistingRole(string roleName)
		{
			IGalleryServerRole role = Factory.LoadGalleryServerRole(roleName, true);

			if (role == null)
				throw new InvalidGalleryServerRoleException();

			role.AllowAddChildAlbum = chkAddAlbum.Checked;
			role.AllowAddMediaObject = chkAddMediaObject.Checked;
			role.AllowAdministerSite = chkAdministerSite.Checked;
			role.AllowAdministerGallery = chkAdministerGallery.Checked;
			role.AllowDeleteChildAlbum = chkDeleteChildAlbum.Checked;
			role.AllowDeleteMediaObject = chkDeleteMediaObject.Checked;
			role.AllowEditAlbum = chkEditAlbum.Checked;
			role.AllowEditMediaObject = chkEditMediaObject.Checked;
			role.AllowSynchronize = chkSynchronize.Checked;
			role.AllowViewOriginalImage = chkViewHiResImage.Checked;
			role.AllowViewAlbumOrMediaObject = chkViewObject.Checked;
			role.HideWatermark = chkHideWatermark.Checked;

			RoleController.Save(role, tvUC.TopLevelCheckedAlbumIds);
		}

		private void AddNewRole()
		{
			RoleController.CreateRole(txtRoleName.Text.Trim(), chkViewObject.Checked, chkViewHiResImage.Checked,
				chkAddMediaObject.Checked, chkAddAlbum.Checked, chkEditMediaObject.Checked, chkEditAlbum.Checked, chkDeleteMediaObject.Checked,
				chkDeleteChildAlbum.Checked, chkSynchronize.Checked, chkAdministerSite.Checked, chkAdministerGallery.Checked, chkHideWatermark.Checked, tvUC.TopLevelCheckedAlbumIds);
		}

		private void RegisterJavaScript()
		{
			string script = String.Format(CultureInfo.InvariantCulture, @"
			var isAddingRole = false; // Set to true when preparing to add a role; used by manageRolesEndRequest event

			function addRoleClicked()
			{{
				hideUserMessage();

				if(dgEditRole.get_isShowing())
				{{
					dgEditRole.Close();
				}}
				else
				{{
					isAddingRole = true;
					$('#{21}').val('');
					eval(""{22}""); // Initiates postback
					$('#dialogHeader').text('{0}');
					dgEditRole.beginUpdate();
					dgEditRole.set_animationDirectionElement('{20}');
					dgEditRole.set_value('');
					dgEditRole.set_title('{0}');
					dgEditRole.endUpdate();
					dgEditRole.Show();
				}}
			}}

			function manageRolesInitializeRequest(sender, args) {{
				$('#{23}').hide();
			}} 

			function manageRolesEndRequest(sender, args) {{
				$('#{23}').show();

				if (isAddingRole)
					$('#{13}').focus();

				isAddingRole = false;
			}}

			function editRole(roleName)
			{{
				// Decode escaped quotes
				roleName = roleName.replace(/\\""/g, '\""');

				hideUserMessage();

				if(dgEditRole.get_isShowing())
				{{
					dgEditRole.Close();
				}}
				else
				{{
					$('#{21}').val(roleName); // Assign to hidden field; used by server code
					eval(""{22}""); // Initiates postback
					$('#dialogHeader').text('{1} - ' + parseRoleNameFromGspRoleName(roleName));
					dgEditRole.beginUpdate();
					dgEditRole.set_animationDirectionElement(makeValidForId(roleName));
					dgEditRole.set_value(roleName);
					dgEditRole.set_title('{1} - ' + parseRoleNameFromGspRoleName(roleName));
					dgEditRole.endUpdate();
					dgEditRole.Show();
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

			function parseRoleNameFromGspRoleName(roleName)
			{{
				return roleName;
				//return roleName.replace(/_\d+$/, ''); // Remove _[GalleryId] (DotNetNuke only)
			}}

			function ConfirmDelete(roleName)
			{{
				return confirm('{2} ' + parseRoleNameFromGspRoleName(roleName.replace(/#%cLt#%/g, '<')) + '?');
			}}

			function IsAdminChecked()
			{{
				// Invoked from the onclick event of checkboxes. Return true if either the 'Administer Site' or 
				// 'Administer Gallery' checkboxes is selected.
				return ($('#{3}').prop('checked') || $('#{18}').prop('checked'));
			}}

			function chkAdministerSite_CheckChanged(adminCheckBox)
			{{
				if (!adminCheckBox) return;
				
				var isAdminChecked = adminCheckBox.checked;
				if (!isAdminChecked) return;
				
				$get('{18}').checked = true;
				
				setRoles(true);

				// Select all albums
				{14}.checkAll();
			}}

			function chkAdministerGallery_CheckChanged(adminCheckBox)
			{{
				if (!adminCheckBox) return true;
				
				if ($('#{3}').prop('checked')) return false;
				
				if (adminCheckBox.checked)
				{{
				  setRoles(true);
				}}
				return true;
			}}

			function setRoles(isChecked)
			{{
				$get('{4}').checked = isChecked;
				$get('{5}').checked = isChecked;
				$get('{6}').checked = isChecked;
				$get('{7}').checked = isChecked;
				$get('{8}').checked = isChecked;
				$get('{9}').checked = isChecked;
				$get('{10}').checked = isChecked;
				$get('{11}').checked = isChecked;
				$get('{12}').checked = isChecked;
			}}

			function saveRoleComplete(callbackStatus)
			{{
				if (callbackStatus == 'success')
				{{
					dgEditRole.Close();
					var isNewRole = ($('#{21}').val().length == 0);
					if (isNewRole)	gdRoles.callback();
				}}
				
				popupInfoRender();
			}}

			function gdRoles_CallbackError(sender, args)
			{{
				showUserMessage(args.get_errorMessage());

				gdRoles.callback();
			}}

			function showUserMessage(msg)
			{{
				$('#{15}').text(msg);

				var usrMsg = $get('{16}');
				if (Sys.UI.DomElement.containsCssClass(usrMsg, 'gsp_invisible'))
				{{
					Sys.UI.DomElement.removeCssClass(usrMsg, 'gsp_invisible');
					Sys.UI.DomElement.addCssClass(usrMsg, 'gsp_visible');
				}}
			}}

			function hideUserMessage()
			{{
				var usrMsg = $get('{16}');
				if (Sys.UI.DomElement.containsCssClass(usrMsg, 'gsp_visible'))
				{{
					Sys.UI.DomElement.removeCssClass(usrMsg, 'gsp_visible');
					Sys.UI.DomElement.addCssClass(usrMsg, 'gsp_invisible');
				}}
			}}

			function makeValidForId(roleName)
			{{
				// Remove quotes, apostrophes, and <. Remove encoded < symbol (#%cLt#%) caused by CA.
				return roleName.replace(/""/g, '').replace(/'/g, '').replace(/</g, '').replace(/#%cLt#%/g, '');
			}}

			function getRoleName(dataItem)
			{{
				var roleName = dataItem.getMember('RoleName').get_value();
				// Escape quotes, apostrophes and back slashes. Replace encoded < symbol (#%cLt#%) caused by CA with <
				return encodeURI(roleName.replace(/""/g, '\\\""').replace(/\\/g, '\\\\').replace(/\'/g, ""\\'"").replace(/#%cLt#%/g, '<'));
			}}

			function getRoleNameNoEncode(dataItem)
			{{
				var roleName = dataItem.getMember('RoleName').get_value();
				// Escape quotes, apostrophes and back slashes
				return roleName.replace(/""/g, '\\\""').replace(/\\/g, '\\\\').replace(/\'/g, ""\\'"");
			}}

			function toggleOwnerRoles(chk)
			{{
				if (chk.checked)
					gdRoles.filter("""");
				else
					gdRoles.filter(""DataItem.getMember('RoleName').get_value().indexOf('{17}') < 0"");
					
				gdRoles.render();
			}}

			function gdRoles_onLoad(sender, eventArgs)
			{{
				toggleOwnerRoles($get('chkShowOwnerRoles'));

				// Fix URL for port forwarded configurations by replacing URL generated by CA with the URL that has correct port
				var newUrl = '{19}';
				var oldPrefix = sender.CallbackPrefix;
				var idx = oldPrefix.lastIndexOf(""&Cart_"");
				var newPrefix = newUrl + oldPrefix.substring(idx);
				sender.CallbackPrefix = newPrefix;
			}}

			", Resources.GalleryServerPro.Admin_Dialog_Title_Add_Role, // 0
																		Resources.GalleryServerPro.Admin_Dialog_Title_Edit_Role, // 1
																		Resources.GalleryServerPro.Admin_Manage_Roles_Confirm_Delete_Text, // 2
																		chkAdministerSite.ClientID, // 3
																		chkViewObject.ClientID, // 4
																		chkViewHiResImage.ClientID, // 5
																		chkAddAlbum.ClientID, // 6
																		chkAddMediaObject.ClientID, // 7
																		chkEditAlbum.ClientID, // 8
																		chkEditMediaObject.ClientID, // 9
																		chkDeleteChildAlbum.ClientID, // 10
																		chkDeleteMediaObject.ClientID, // 11
																		chkSynchronize.ClientID, // 12
																		txtRoleName.ClientID, // 13
																		tvUC.TreeView.ClientObjectId, // 14
																		ucUserMessage.MessageDetailContainer.ClientID, // 15
																		ucUserMessage.MessageContainer.ClientID, // 16
																		GlobalConstants.AlbumOwnerRoleNamePrefix, // 17
																		chkAdministerGallery.ClientID, // 18
																		Utils.GetCurrentPageUri(), // 19
																		lbAddRole.ClientID, // 20
																		hdnRoleName.ClientID, // 21
																		Page.ClientScript.GetPostBackEventReference(lbAddRole, null), // 22
																		pnlDialogContent.ClientID // 23
				);

			ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "manageRoleFunctions", script, true);
		}

		#endregion
	}
}