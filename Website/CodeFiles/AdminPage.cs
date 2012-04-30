using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.Web.Controller;

namespace GalleryServerPro.Web.Pages
{
	/// <summary>
	/// The base class that is used for administration pages.
	/// </summary>
	public abstract class AdminPage : Pages.GalleryPage
	{
		#region Private Fields

		private PlaceHolder _phAdminHeader;
		private Controls.Admin.adminheader _adminHeader;
		private PlaceHolder _phAdminFooter;
		private Controls.Admin.adminfooter _adminFooter;
		private Controls.Admin.adminmenu _adminMenu;
		private IGallerySettings _gallerySettings;
		private IGalleryControlSettings _galleryControlSettings;
		private List<String> _usersWithAdminPermission;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="AdminPage"/> class.
		/// </summary>
		protected AdminPage()
		{
			//this.Init += AdminPage_Init;
			this.Load += AdminPage_Load;
			this.BeforeHeaderControlsAdded += AdminPage_BeforeHeaderControlsAdded;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets a writeable instance of gallery settings.
		/// </summary>
		/// <value>A writeable instance of gallery settings.</value>
		public IGallerySettings GallerySettingsUpdateable
		{
			get
			{
				if (_gallerySettings == null)
				{
					_gallerySettings = Factory.LoadGallerySetting(GalleryId, true);
				}

				return _gallerySettings;
			}
		}

		/// <summary>
		/// Gets a writeable instance of gallery settings.
		/// </summary>
		/// <value>A writeable instance of gallery settings.</value>
		public IGalleryControlSettings GalleryControlSettingsUpdateable
		{
			get
			{
				if (_galleryControlSettings == null)
				{
					_galleryControlSettings = Factory.LoadGalleryControlSetting(this.GalleryControl.ControlId, true);
				}

				return _galleryControlSettings;
			}
		}

		/// <summary>
		/// Gets or sets the location for the <see cref="GalleryServerPro.Web.Controls.Admin.adminheader"/> user control. Classes that inherit 
		/// <see cref="Pages.AdminPage"/> should set this property to the <see cref="PlaceHolder"/> that is to contain
		/// the admin header control. If this property is not assigned by the inheriting class, the admin header control
		/// is not added to the page output.
		/// </summary>
		/// <value>The <see cref="GalleryServerPro.Web.Controls.Admin.adminheader"/> user control.</value>
		public PlaceHolder AdminHeaderPlaceHolder
		{
			get
			{
				return this._phAdminHeader;
			}
			set
			{
				this._phAdminHeader = value;
			}
		}

		/// <summary>
		/// Gets the admin header user control that is rendered near the top of the administration pages. This control contains the 
		/// page title and the top Save/Cancel buttons. (The bottom Save/Cancel buttons are in the <see cref="GalleryServerPro.Web.Controls.Admin.adminfooter"/> user control.
		/// </summary>
		/// <value>The admin header user control that is rendered near the top of the administration pages.</value>
		public Controls.Admin.adminheader AdminHeader
		{
			get
			{
				return this._adminHeader;
			}
		}

		/// <summary>
		/// Gets or sets the location for the <see cref="GalleryServerPro.Web.Controls.Admin.adminfooter"/> user control. Classes that inherit 
		/// <see cref="Pages.AdminPage"/> should set this property to the <see cref="PlaceHolder"/> that is to contain
		/// the admin footer control. If this property is not assigned by the inheriting class, the admin footer control
		/// is not added to the page output.
		/// </summary>
		/// <value>The <see cref="GalleryServerPro.Web.Controls.Admin.adminfooter"/> user control.</value>
		public PlaceHolder AdminFooterPlaceHolder
		{
			get
			{
				return this._phAdminFooter;
			}
			set
			{
				this._phAdminFooter = value;
			}
		}

		/// <summary>
		/// Gets the admin footer user control that is rendered near the bottom of the administration pages. This control contains the 
		/// bottom Save/Cancel buttons. (The top Save/Cancel buttons are in the <see cref="GalleryServerPro.Web.Controls.Admin.adminheader"/> user control.
		/// </summary>
		/// <value>The admin footer user control that is rendered near the bottom of the administration pages.</value>
		public Controls.Admin.adminfooter AdminFooter
		{
			get
			{
				return this._adminFooter;
			}
		}

		/// <summary>
		/// Gets / sets the page title text (e.g. Site Settings - General).
		/// </summary>
		public string AdminPageTitle
		{
			get
			{
				return this._adminHeader.AdminPageTitle;
			}
			set
			{
				this._adminHeader.AdminPageTitle = value;
			}
		}

		/// <summary>
		/// Gets / sets the text that appears on the top and bottom Ok buttons on the page. This is rendered as the value
		/// attribute of the input HTML tag.
		/// </summary>
		public string OkButtonText
		{
			get
			{
				return this.AdminHeader.OkButtonText;
			}
			set
			{
				this.AdminHeader.OkButtonText = value;
				this.AdminFooter.OkButtonText = value;
			}
		}

		/// <summary>
		/// Gets / sets the ToolTip for the top and bottom Ok buttons on the page. The ToolTip is rendered as 
		/// the title attribute of the input HTML tag.
		/// </summary>
		public string OkButtonToolTip
		{
			get
			{
				return this.AdminHeader.OkButtonToolTip;
			}
			set
			{
				this.AdminHeader.OkButtonToolTip = value;
				this.AdminFooter.OkButtonToolTip = value;
			}
		}

		/// <summary>
		/// Gets / sets the text that appears on the top and bottom Cancel buttons on the page. This is rendered as the value
		/// attribute of the input HTML tag.
		/// </summary>
		public string CancelButtonText
		{
			// This is the text that appears on the top and bottom Cancel buttons.
			get
			{
				return this.AdminHeader.CancelButtonText;
			}
			set
			{
				this.AdminHeader.CancelButtonText = value;
				this.AdminFooter.CancelButtonText = value;
			}
		}

		/// <summary>
		/// Gets / sets the ToolTip for the top and bottom Cancel buttons on the page. The ToolTip is rendered as 
		/// the title attribute of the HTML tag.
		/// </summary>
		public string CancelButtonToolTip
		{
			get
			{
				return this.AdminHeader.CancelButtonToolTip;
			}
			set
			{
				this.AdminHeader.CancelButtonToolTip = value;
				this.AdminFooter.CancelButtonToolTip = value;
			}
		}

		/// <summary>
		/// Gets / sets the visibility of the top and bottom Ok buttons on the page. When true, the buttons
		/// are visible. When false, they are not visible (not rendered in the page output.)
		/// </summary>
		public bool OkButtonIsVisible
		{
			get
			{
				return this.AdminHeader.OkButtonIsVisible;
			}
			set
			{
				this.AdminHeader.OkButtonIsVisible = value;
				this.AdminFooter.OkButtonIsVisible = value;
			}
		}

		/// <summary>
		/// Gets / sets the visibility of the top and bottom Cancel buttons on the page. When true, the buttons
		/// are visible. When false, they are not visible (not rendered in the page output.)
		/// </summary>
		public bool CancelButtonIsVisible
		{
			get
			{
				return this.AdminHeader.CancelButtonIsVisible;
			}
			set
			{
				this.AdminHeader.CancelButtonIsVisible = value;
				this.AdminFooter.CancelButtonIsVisible = value;
			}
		}

		/// <summary>
		/// Gets a reference to the top button that initiates the completion of the task.
		/// </summary>
		public Button OkButtonTop
		{
			get
			{
				return this.AdminHeader.OkButtonTop;
			}
		}

		/// <summary>
		/// Gets a reference to the bottom button that initiates the completion of the task.
		/// </summary>
		public Button OkButtonBottom
		{
			get
			{
				return this.AdminFooter.OkButtonBottom;
			}
		}

		/// <summary>
		/// Gets a reference to the <see cref="GalleryServerPro.Web.Controls.Admin.adminmenu"/> control on the page.
		/// </summary>
		/// <value>The <see cref="GalleryServerPro.Web.Controls.Admin.adminmenu"/> control on the page.</value>
		public Controls.Admin.adminmenu AdminMenu
		{
			get
			{
				return this._adminMenu;
			}
		}

		/// <summary>
		/// Gets the list of site and gallery administrators. That is, it returns the user names of accounts 
		/// belonging to roles with AllowAdministerSite or AllowAdministerGallery permission.
		/// </summary>
		/// <value>The list of site and gallery administrators.</value>
		public List<String> UsersWithAdminPermission
		{
			get
			{
				if (this._usersWithAdminPermission == null)
				{
					this._usersWithAdminPermission = new List<string>();

					foreach (IGalleryServerRole role in RoleController.GetGalleryServerRoles())
					{
						if (role.AllowAdministerSite || role.AllowAdministerGallery)
						{
							foreach (string userName in RoleController.GetUsersInRole(role.RoleName))
							{
								if (!this._usersWithAdminPermission.Contains(userName))
								{
									this._usersWithAdminPermission.Add(userName);
								}
							}
						}
					}
				}

				return this._usersWithAdminPermission;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the logged on user can add, edit, or delete users and roles. Returns true when the user is a site administrator
		/// and - for gallery admins - when the application setting <see cref="IAppSetting.AllowGalleryAdminToManageUsersAndRoles" /> is true.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the logged on user can add, edit, or delete users and roles; otherwise, <c>false</c>.
		/// </value>
		public bool UserCanEditUsersAndRoles
		{
			get
			{
				return UserCanAdministerSite || (!UserCanAdministerSite && UserCanAdministerGallery && AppSetting.Instance.AllowGalleryAdminToManageUsersAndRoles);
			}
		}

		#endregion

		#region Event Handlers

		void AdminPage_Load(object sender, EventArgs e)
		{
			ValidateRequest();

			AddUserControls();

			ConfigureControls();

			//JQueryRequired = true;
		}

		protected void AdminPage_BeforeHeaderControlsAdded(object sender, EventArgs e)
		{
			// Add the admin menu to the page. Note that if you use any index other than 0 in the AddAt method, the viewstate
			// is not preserved across postbacks. This is the reason why the <see cref="BeforeHeaderControlsAdded"/> event was created in 
			// <see cref="GalleryPage"/> and handled here. We need to add the admin menu *before* <see cref="GalleryPage"/> adds the album breadcrumb
			// menu and the gallery header.
			Controls.Admin.adminmenu adminMenu = (Controls.Admin.adminmenu)LoadControl(Utils.GetUrl("/controls/admin/adminmenu.ascx"));
			this._adminMenu = adminMenu;
			this.Controls.AddAt(0, adminMenu);
			//this.Controls.AddAt(Controls.IndexOf(AlbumMenu) + 1, adminMenu); // Do not use: viewstate is not preserved
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Generates script that executes the <paramref name="scriptToRun"/> after the DOM is fully loaded. This is needed
		/// because simply passing the script to ScriptManager.RegisterStartupScript may cause it to run before the DOM is 
		/// fully initialized. This method should only be called once for the page because it hard-codes a javascript function
		/// named adminPageLoad.
		/// </summary>
		/// <param name="scriptToRun">The script to run.</param>
		/// <returns>Returns a string that can be passed to the ScriptManager.RegisterStartupScript method. Does not include
		/// the script tags.</returns>
		protected static string GetPageLoadScript(string scriptToRun)
		{
			return String.Format(CultureInfo.InvariantCulture,
@"
Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(adminPageLoad);

function adminPageLoad(sender, args)
{{
	{0}
}}
",
				scriptToRun);
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Sends server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter"/> object, which writes the content to be rendered on the client.
		/// </summary>
		/// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the server control content.</param>
		protected override void Render(HtmlTextWriter writer)
		{
			// Write out the HTML for this control.
			base.Render(writer);

			// Add the GSP logo to the end
			if (!FooterHasBeenRendered)
			{
				AddGspLogo(writer);

				FooterHasBeenRendered = true;
			}

			AddMaintenanceServiceCallIfNeeded();
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Write out the Gallery Server Pro logo to the <paramref name="writer"/>.
		/// </summary>
		/// <param name="writer">The writer.</param>
		private void AddGspLogo(HtmlTextWriter writer)
		{
			// This function writes out HTML like this:
			// <div class="gsp_ns">
			//  <div class="gsp_addtopmargin5 gsp_footer">
			//   <p>
			//    <a href="http://www.galleryserverpro.com" title="Powered by Gallery Server Pro v2.1.3222">
			//     <img src="/images/gsp_ftr_logo_170x46.png" alt="Powered by Gallery Server Pro v2.1.3222" style="width:170px;height:46px;" />
			//    </a>
			//   </p>
			//   <p>v2.4.0</p>
			//  </div>
			// </div>

			// Wrap HTML in an enclosing <div id="gsp_ns"> tag. This is used as a pseudo namespace that is used to limit the
			// influence CSS has to only the Gallery Server code, thus preventing the CSS from affecting HTML that may 
			// exist in the master page or other areas outside the user control.
			writer.AddAttribute("class", "gsp_ns"); // gsp_ns stands for Gallery Server Pro namespace
			writer.RenderBeginTag(HtmlTextWriterTag.Div);

			string tooltip = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.Footer_Logo_Tooltip, Utils.GetGalleryServerVersion());
			//string url = Page.ClientScript.GetWebResourceUrl(typeof(footer), "GalleryServerPro.Web.gs.images.gsp_ftr_logo_170x46.png");

			// Create <div> tag that wraps the <a> and <img> tags.
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "gsp_addtopmargin3 gsp_footer");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);

			// Create <p> tag to contain the logo and link.
			writer.RenderBeginTag(HtmlTextWriterTag.P);

			// Create <a> tag that wraps <img> tag.
			writer.AddAttribute(HtmlTextWriterAttribute.Title, tooltip);
			writer.AddAttribute(HtmlTextWriterAttribute.Href, "http://www.galleryserverpro.com");
			writer.RenderBeginTag(HtmlTextWriterTag.A);

			// Create <img> tag.
			writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "170px");
			writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "46px");
			writer.AddStyleAttribute(HtmlTextWriterStyle.VerticalAlign, "middle");
			writer.AddAttribute(HtmlTextWriterAttribute.Src, Page.ClientScript.GetWebResourceUrl(this.GetType().BaseType, "GalleryServerPro.Web.gs.images.gsp_ftr_logo_170x46.png"));
			writer.AddAttribute(HtmlTextWriterAttribute.Alt, tooltip);
			writer.RenderBeginTag(HtmlTextWriterTag.Img);
			writer.RenderEndTag();

			writer.RenderEndTag(); // Close out the <a> tag.
			writer.RenderEndTag(); // Close out the <p> tag.

			// Create a new line to contain the text of the version number.
			writer.RenderBeginTag(HtmlTextWriterTag.P);
			writer.Write(String.Concat("v", Utils.GetGalleryServerVersion()));
			writer.RenderEndTag(); // Close out the <p> tag.

			// Close out the two outermost <div> tags.
			writer.RenderEndTag();
			writer.RenderEndTag();
		}

		private void AddUserControls()
		{
			Controls.Admin.adminheader adminHeader = (Controls.Admin.adminheader)LoadControl(Utils.GetUrl("/controls/admin/adminheader.ascx"));
			this._adminHeader = adminHeader;
			if (this.AdminHeaderPlaceHolder != null)
				this.AdminHeaderPlaceHolder.Controls.Add(adminHeader);

			Controls.Admin.adminfooter adminFooter = (Controls.Admin.adminfooter)LoadControl(Utils.GetUrl("/controls/admin/adminfooter.ascx"));
			this._adminFooter = adminFooter;
			if (this.AdminFooterPlaceHolder != null)
				this.AdminFooterPlaceHolder.Controls.Add(adminFooter);
		}

		private void ConfigureControls()
		{
			if ((this.AdminHeaderPlaceHolder != null) && (this.AdminHeader != null) && (this.AdminHeader.OkButtonTop != null))
				this.Page.Form.DefaultButton = this.AdminHeader.OkButtonTop.UniqueID;
		}

		/// <summary>
		/// Verify that the inferred gallery ID is the same as the one specified for this page.
		/// If not, remove the aid parameter and reload page. This helps prevent the situation
		/// where a page is assigned to a particular gallery but the album ID specified in the
		/// URL belongs to another gallery, causing the page to render with settings for the 
		/// other gallery, potentially confusing the admin.
		/// </summary>
		private void ValidateRequest()
		{
			// If the inferred gallery ID is different than the one specified for this page,
			// remove the aid parameter and reload page.
			if (GalleryId != GalleryControl.GalleryId)
			{
				string url = Utils.RemoveQueryStringParameter(Request.Url.PathAndQuery, "aid");

				Utils.Redirect(url);
			}
		}

		/// <summary>
		/// If needed, add javascript to start the maintenance routine through a web service call. We use this technique because it can be a 
		/// long-running process and invoking it this way has little impact on the users.
		/// </summary>
		/// <remarks>The first iteration of the maintenace routine was invoked on a background thread, but background threads cannot access
		/// HttpContext.Current, which is required for the DotNetNuke implementation (and potential future versions of GSP's implementation),
		/// so that approach was replaced with this one.</remarks>
		private void AddMaintenanceServiceCallIfNeeded()
		{
			//TODO: After jQuery is used in main browsing area, move this back to GalleryPage.
			if (AppSetting.Instance.MaintenanceStatus == MaintenanceStatus.NotStarted && !IsPostBack)
			{
				const string script = @"
$(function() {
	Gsp.Gallery.PerformMaintenance(function() {}, function() {}); // Swallow error on client
});";

				ScriptManager.RegisterStartupScript(this, this.GetType(), "galleryPageStartupScript", script, true);
			}
		}

		#endregion
	}
}
