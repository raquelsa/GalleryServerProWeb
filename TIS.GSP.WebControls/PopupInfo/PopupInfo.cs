using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.Design;
using System.ComponentModel.Design;

namespace GalleryServerPro.WebControls
{
	/// <summary>
	/// The PopupInfo class provides a means to enable help functionality for ASP.NET controls.
	/// </summary>
	//[NonVisualControl, Designer(typeof(PopupInfoDesigner))]
	[ProvideProperty("DataBindingItem", typeof(Control))]
	[ParseChildren(true, "PopupInfoItems")]
	[PersistChildren(false)]
	[DefaultProperty("PopupInfoItems")]
	public class PopupInfo : Control, IExtenderProvider
	{
		#region Private Fields

		private bool _isExtender = true;
		private PopupInfoItemCollection _popupInfoItems = null;
		private bool _AutoLoadDataBoundControls = false;
		private string _iconWebResource = "";
		private string _iconUrl = "WebResource";
		private string _defaultDialogControlId = "";
		private string _defaultDialogTitleCss = "";
		private string _defaultDialogBodyCss = "";

		/// <summary>
		/// Flag that determines whether controls were auto-loaded from the page.
		/// </summary>
		private bool _AutoLoadedDataBoundControls = false;

		#endregion

		#region Constructors

		public PopupInfo()
		{
			this._popupInfoItems = new PopupInfoItemCollection(this);
		}

		#endregion

		#region Public Fields

		/// <summary>
		/// Specifies whether user is in design mode.
		/// </summary>
		public new bool DesignMode = (HttpContext.Current == null);

		#endregion

		#region Internal Properties

		/// <summary>
		/// The Web Resource Url used to access retrieve the Icon.
		/// Used to minimize reloading this URL from the resource manager repeatedly.
		/// </summary>
		internal string IconWebResource
		{
			get
			{
				if (string.IsNullOrEmpty(this._iconWebResource))
					this._iconWebResource = Page.ClientScript.GetWebResourceUrl(this.GetType(), ControlsResources.HELP_ICON_RESOURCE);

				return _iconWebResource;
			}
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// A collection of all the PopupInfoItems that are to be bound. Each PopupInfoItem contains
		/// the information needed to attach a dialog popup to a Control.
		/// <seealso>Class PopupInfoItem</seealso>
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		public PopupInfoItemCollection PopupInfoItems
		{
			get
			{
				return _popupInfoItems;
			}
		}

		/// <summary>
		/// Optional Url to the Warning and Info Icons.
		/// Note: Used only if the control uses images.
		/// </summary>
		[Description("Optional Image Url for the Popup Icon."),
		Editor("System.Web.UI.Design.ImageUrlEditor", typeof(System.Drawing.Design.UITypeEditor)),
			DefaultValue("WebResource")]
		public string IconUrl
		{
			get { return _iconUrl; }
			set { _iconUrl = value; }
		}

		/// <summary>
		/// The ID of a ComponentArt.Web.UI.Dialog control to show when the popup trigger is activated. If this value is specified,
		/// the DialogControlId for child items do not need to specified unless it requires a different dialog control.
		/// </summary>
		[Browsable(true), Description("The ID of a ComponentArt.Web.UI.Dialog control to show when the popup trigger is activated. If this value is specified, the DialogControlId for child items do not need to specified unless it requires a different dialog control."),
			DefaultValue("")]
		[TypeConverter(typeof(ControlIDConverter))]
		public string DialogControlId
		{
			get
			{
				return _defaultDialogControlId;
			}
			set
			{
				_defaultDialogControlId = value;
			}
		}

		/// <summary>
		/// The CSS class to apply to the title text of the popup. If this value is specified, the DialogTitleCss
		/// for child items do not need to specified unless it requires a different dialog control.
		/// </summary>
		[Browsable(true), Description("The CSS class to apply to the title text of the popup. If this value is specified, the DialogTitleCss for child items do not need to specified unless it requires a different dialog control."),
			DefaultValue("")]
		public string DefaultDialogTitleCss
		{
			get
			{
				return _defaultDialogTitleCss;
			}
			set
			{
				_defaultDialogTitleCss = value;
			}
		}

		/// <summary>
		/// The CSS class to apply to the body text of the popup. If this value is specified, the DialogBodyCss
		/// for child items do not need to specified unless it requires a different dialog control.
		/// </summary>
		[Browsable(true), Description("The CSS class to apply to the body text of the popup. If this value is specified, the DialogBodyCss for child items do not need to specified unless it requires a different dialog control."),
			DefaultValue("")]
		public string DefaultDialogBodyCss
		{
			get
			{
				return _defaultDialogBodyCss;
			}
			set
			{
				_defaultDialogBodyCss = value;
			}
		}

		/// <summary>
		/// Determines whether this control works as an Extender object to other controls on the form.
		/// In some situations it might be useful to disable the extender functionality such
		/// as when all databinding is driven through code or when using the IPopupInfo
		/// interface with custom designed controls that have their own DataBinder objects.
		/// </summary>
		[Browsable(true), Description("Determines whether this control works as an Extender object to other controls on the form"), DefaultValue(true)]
		public bool IsExtender
		{
			get { return _isExtender; }
			set { _isExtender = value; }
		}

		/// <summary>
		/// Automatically imports all controls on the form that implement the IPopupInfo interface and adds them to the DataBinder
		/// </summary>
		[Description("Automatically imports all controls on the form that implement the IPopupInfo interface and adds them to the DataBinder"),
		 Browsable(true), DefaultValue(false)]
		public bool AutoLoadDataBoundControls
		{
			get { return _AutoLoadDataBoundControls; }
			set { _AutoLoadDataBoundControls = value; }
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// OnLoad override
		/// </summary>
		/// <param name="e">event args</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			RegisterJavascript();

			string script = "function popupInfoRender()\r\n{\r\n";
			foreach (PopupInfoItem item in this.PopupInfoItems)
			{
				script += item.DataBind(this.Page);
			}
			script += "\r\n}\r\n";

			// Since we can't directly add controls to the page, we have to do it via HTML. Register the javascript function call
			// that will render our HTML for us.
			this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), this.ClientID + "_renderscript", script, true);

			// Now register the function call to invoke the generated javascript.
			string startupScript = "popupInfoRender();";
			this.Page.ClientScript.RegisterStartupScript(this.GetType(), this.ClientID + "_startupscript", startupScript, true);
			
			// Register an empty OnSubmit statement so the ASP.NET WebForm_OnSubmit method will be automatically
			// created and our behavior will be able to wrap it if necessary
			//ScriptManager.RegisterOnSubmitStatement(this, typeof(ConfirmButtonExtender), "ConfirmButtonExtenderOnSubmit", "null;");
		}

		#endregion

		#region Internal Methods

		/// <summary>
		/// This method is used to ensure that designer is notified
		/// every time there is a change in the sub-ordinate validators
		/// </summary>
		internal void NotifyDesigner()
		{
			if (this.DesignMode)
			{
				IDesignerHost Host = this.Site.Container as IDesignerHost;
				ControlDesigner Designer = Host.GetDesigner(this) as ControlDesigner;
				PropertyDescriptor Descriptor = null;
				try
				{
					Descriptor = TypeDescriptor.GetProperties(this)["PopupInfoItems"];
				}
				catch
				{
					return;
				}

				ComponentChangedEventArgs ccea = new ComponentChangedEventArgs(this, Descriptor, null, this.PopupInfoItems);
				Designer.OnComponentChanged(this, ccea);
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Picks up all controls on the form that implement the IPopupInfo interface
		/// and adds them to the PopupInfoItems Collection
		/// </summary>
		/// <param name="Container"></param>
		/// <returns></returns>
		public void LoadFromControls(Control Container)
		{
			// *** Only allow loading of controls implicitly once
			if (this._AutoLoadedDataBoundControls)
				return;
			this._AutoLoadedDataBoundControls = true;

			LoadDataBoundControls(Container);
		}

		/// <summary>
		/// Returns a specific DataBinding Item for a given control.
		/// Always returns an item even if the Control is not found.
		/// If you need to check whether this is a valid item check
		/// the BindingSource property for being blank.
		/// 
		/// Extender Property Get method
		/// </summary>
		/// <param name="control"></param>
		/// <returns></returns>
		public PopupInfoItem GetDataBindingItem(Control control)
		{
			foreach (PopupInfoItem Item in this.PopupInfoItems)
			{
				if (Item.ControlId == control.ID)
				{
					// *** Ensure the binder is set on the item
					Item.PopupInfoContainer = this;
					return Item;
				}
			}

			PopupInfoItem NewItem = new PopupInfoItem(this);
			NewItem.ControlId = control.ID;
			NewItem.ControlInstance = control;

			this.PopupInfoItems.Add(NewItem);

			return NewItem;
		}

		/// <summary>
		/// Return a specific databinding item for a give control id.
		/// Note unlike the ControlInstance version return null if the
		/// ControlId isn't found. 
		/// </summary>
		/// <param name="ControlId"></param>
		/// <returns></returns>
		public PopupInfoItem GetDataBindingItem(string ControlId)
		{
			for (int i = 0; i < this.PopupInfoItems.Count; i++)
			{
				if (this.PopupInfoItems[i].ControlId == ControlId)
					return this.PopupInfoItems[i];
			}

			return null;
		}

		/// <summary>
		/// Adds a binding to the control. This method is a simple
		/// way to establish a binding.
		/// 
		/// Returns the Item so you can customize properties further
		/// </summary>
		/// <param name="ControlToBind"></param>
		/// <param name="ControlPropertyToBind"></param>
		/// <param name="SourceObjectNameToBindTo"></param>
		/// <param name="SourceMemberToBindTo"></param>
		/// <returns></returns>
		//public PopupInfoItem AddBinding(Control ControlToBind, string ControlPropertyToBind,
		//                  string SourceObjectNameToBindTo, string SourceMemberToBindTo)
		//{
		//  PopupInfoItem Item = new PopupInfoItem(this);

		//  Item.ControlInstance = ControlToBind;
		//  Item.ControlId = ControlToBind.ID;
		//  Item.Page = this.Page;
		//  //Item.BindingSource = SourceObjectNameToBindTo;
		//  //Item.BindingSourceMember = SourceMemberToBindTo;

		//  this.PopupInfoItems.Add(Item);

		//  return Item;
		//}

		/// <summary>
		/// This method only adds a data binding item, but doesn't bind it
		/// to anything. This can be useful for only displaying errors
		/// </summary>
		/// <param name="ControlToBind"></param>
		/// <returns></returns>
		//public PopupInfoItem AddBinding(Control ControlToBind)
		//{
		//  PopupInfoItem Item = new PopupInfoItem(this);

		//  Item.ControlInstance = ControlToBind;
		//  Item.ControlId = ControlToBind.ID;
		//  Item.Page = this.Page;

		//  this.PopupInfoItems.Add(Item);

		//  return Item;
		//}

		#endregion

		#region Private Methods

		/// <summary>
		/// This method adds the javascript needed to support this control. Adds these javascript functions:
		/// popupInfoPageLoad: Registers function to handle click event of html tag.
		/// html_onclick: Fires when any element is clicked in page. Closes the popup if it is open and if the mouse is not hovering over it.
		/// addHtmlAfterControl: Adds the specified htmlMarkup to the page DOM after the specified controlId.
		/// helpIconClick: The function that is invoked when the user clicks the icon image. The function shows the dialog.
		/// </summary>
		private void RegisterJavascript()
		{
			// Add page load script that will add click handler to html element
			const string startupScript = "Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(popupInfoPageLoad);";

			this.Page.ClientScript.RegisterStartupScript(this.GetType(), "popupInfoStartup", startupScript, true);

			// Add remaining functions
			string script = String.Format(@"
var _popupInMouseOver = false;

function popupInfoPageLoad(sender, args)
{{
	$addHandler(document.getElementsByTagName('html')[0], 'click', html_onclick);
}}

function html_onclick(e)
{{
	if (!_popupInMouseOver && {0}.get_isShowing())
		{0}.close();
}}

function addHtmlAfterControl(controlId,htmlMarkup)
{{
	var ctl = document.getElementById(controlId);
	if (ctl == null)
	 return;
	 
	var hcId = controlId + '_popupHelpContainer';
	var sibling = ctl.nextSibling;
	if ((sibling != null) && (sibling.id == hcId))
	{{
		sibling.innerHTML = htmlMarkup;
	}}
	else
	{{
		var htmlContainer = document.createElement('span');
		htmlContainer.setAttribute('id', hcId);
		htmlContainer.innerHTML = htmlMarkup;

		if (sibling == null)
			ctl.parentNode.appendChild(htmlContainer);
		else
			ctl.parentNode.insertBefore(htmlContainer,sibling);
	}}
}}

function helpIconClick(dgCtl, senderId, contentText)
{{
	dgCtl.beginUpdate();
	dgCtl.set_alignmentElement(senderId);
	dgCtl.set_content(contentText);
	dgCtl.endUpdate();
	dgCtl.show();
}}

", this.DialogControlId);

			this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "PopupInfoScript", script, true);
		}

		/// <summary>
		/// Loop through all of the contained controls of the form and
		/// check for all that implement IPopupInfo. If found
		/// add the BindingItem to this Databinder
		/// </summary>
		/// <param name="Container"></param>
		private void LoadDataBoundControls(Control Container)
		{
			foreach (Control Ctl in Container.Controls)
			{
				// ** Recursively call down into any containers
				if (Ctl.Controls.Count > 0)
					this.LoadDataBoundControls(Ctl);

				if (Ctl is IPopupInfo)
					this.PopupInfoItems.Add(((IPopupInfo)Ctl).PopupInfoItem);
			}
		}

		#endregion

		#region IExtenderProvider Members

		/// <summary>
		/// Determines whether a control can be extended. Basically
		/// we allow ANYTHING to be extended so all controls except
		/// the popupinfo itself are extendable.
		/// 
		/// Optionally the control can be set up to not act as 
		/// an extender in which case the IsExtender property 
		/// can be set to false
		/// </summary>
		/// <param name="extendee"></param>
		/// <returns></returns>
		public bool CanExtend(object extendee)
		{
			if (!this.IsExtender)
				return false;

			// *** Don't extend ourself <g>
			if (extendee is PopupInfo)
				return false;

			if (extendee is Control)
				return true;

			return false;
		}

		#endregion
	}

	///// <summary>
	///// Control designer used so we get a grey button display instead of the 
	///// default label display for the control.
	///// </summary>
	//[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	//public class PopupInfoDesigner : ControlDesigner
	//{
	//  public override string GetDesignTimeHtml()
	//  {
	//    return base.CreatePlaceHolderDesignTimeHtml("Control Extender");
	//  }
	//}

}
