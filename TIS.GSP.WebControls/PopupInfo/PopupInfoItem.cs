using System;
using System.ComponentModel;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GalleryServerPro.WebControls
{
	/// <summary>
	/// An individual popup item. The object is a child for the PopupInfo object which acts as a master
	/// object. Binding Items can be attached to controls if the control implements IPopupInfo
	/// </summary>
	[ToolboxData("<{0}:PopupInfoItem runat=server />")]
	[Category("Appearance")]
	[Description("An individual popupinfo item that allows you to attach a popup info dialog to a target control")]
	[Serializable]
	public class PopupInfoItem : Control
	{
		#region Private Fields

		private PopupInfo _PopupInfoContainer = null;
		private Control _controlInstance = null;
		private string _controlId = "";
		private string _dialogTitle = "";
		private string _dialogBody = "";
		private string _dialogTitleCss = "";
		private string _dialogBodyCss = "";

		#endregion

		#region Protected Fields

		/// <summary>
		/// Explicitly set designmode flag - stock doesn't work on Collection child items
		/// </summary>
		protected new bool DesignMode = (HttpContext.Current == null);

		#endregion

		#region Constructors

		/// <summary>
		/// Default Constructor
		/// </summary>
		public PopupInfoItem()
		{
		}

		/// <summary>
		/// Overridden constructor to allow PopupInfo to be passed
		/// as a reference. Unfortunately ASP.NET doesn't fire this when
		/// creating the PopupInfoItem child items.
		/// </summary>
		/// <param name="Parent"></param>
		public PopupInfoItem(PopupInfo Parent)
		{
			this._PopupInfoContainer = Parent;
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Reference to the PopupInfo parent object.
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public PopupInfo PopupInfoContainer
		{
			get { return _PopupInfoContainer; }
			set { _PopupInfoContainer = value; }
		}

		/// <summary>
		/// The ID of the control to extend with a popup dialog.
		/// </summary>
		[NotifyParentProperty(true)]
		[Description("The ID of the control to extend with a popup dialog."), DefaultValue("")]
		[TypeConverter(typeof(ControlIDConverter))]
		[Browsable(true)]
		public string ControlId
		{
			get
			{
				return _controlId;
			}
			set
			{
				_controlId = value;
				if (this.DesignMode && this.PopupInfoContainer != null)
					this.PopupInfoContainer.NotifyDesigner();
			}
		}

		/// <summary>
		/// An optional instance of the control that can be assigned. Used internally
		/// by the PopupInfo to assign the control whenever possible as the instance
		/// is more efficient and reliable than the string name.
		/// </summary>
		[NotifyParentProperty(false)]
		[Description("An instance value for the controls")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public Control ControlInstance
		{
			get
			{
				return _controlInstance;
			}
			set
			{
				_controlInstance = value;
			}
		}

		/// <summary>
		/// The header text to show in the dialog when the popup is activated.
		/// </summary>
		[NotifyParentProperty(true)]
		[Description("The header text to show in the dialog when the popup is activated."), DefaultValue("")]
		[Browsable(true)]
		public string DialogTitle
		{
			get
			{
				return _dialogTitle;
			}
			set
			{
				_dialogTitle = value;
				if (this.DesignMode && this.PopupInfoContainer != null)
					this.PopupInfoContainer.NotifyDesigner();
			}
		}

		/// <summary>
		/// The content text to show in the dialog when the popup is activated. May contain HTML.
		/// </summary>
		[NotifyParentProperty(true)]
		[Description("The content text to show in the dialog when the popup is activated. May contain HTML."), DefaultValue("")]
		[Browsable(true)]
		public string DialogBody
		{
			get
			{
				return _dialogBody;
			}
			set
			{
				_dialogBody = value;
				if (this.DesignMode && this.PopupInfoContainer != null)
					this.PopupInfoContainer.NotifyDesigner();
			}
		}

		/// <summary>
		/// The CSS class to apply to the title text.
		/// </summary>
		[NotifyParentProperty(true)]
		[Description("The CSS class to apply to the title text."), DefaultValue("")]
		[Browsable(true)]
		public string DialogTitleCss
		{
			get
			{
				return _dialogTitleCss;
			}
			set
			{
				_dialogTitleCss = value;
				if (this.DesignMode && this.PopupInfoContainer != null)
					this.PopupInfoContainer.NotifyDesigner();
			}
		}

		/// <summary>
		/// The CSS class to apply to the body text.
		/// </summary>
		[NotifyParentProperty(true)]
		[Description("The CSS class to apply to the body text."), DefaultValue("")]
		[Browsable(true)]
		public string DialogBodyCss
		{
			get
			{
				return _dialogBodyCss;
			}
			set
			{
				_dialogBodyCss = value;
				if (this.DesignMode && this.PopupInfoContainer != null)
					this.PopupInfoContainer.NotifyDesigner();
			}
		}

		#region Hide Properties for the Designer
		[Browsable(false)]
		public override string ID
		{
			get
			{
				return base.ID;
			}
			set
			{
				base.ID = value;
			}
		}

		[Browsable(false)]
		public override bool Visible
		{
			get
			{
				return base.Visible;
			}
			set
			{
				base.Visible = value;
			}
		}

		[Browsable(false)]
		public override bool EnableViewState
		{
			get
			{
				return base.EnableViewState;
			}
			set
			{
				base.EnableViewState = value;
			}
		}
		#endregion

		#endregion

		#region Protected Methods

		#endregion

		#region Public Methods

		public string DataBind(Control WebPage)
		{
			this.ControlInstance = GalleryServerPro.WebControls.Tools.wwWebUtils.FindControlRecursive(WebPage, this.ControlId);

			string imagePath = null;
			if (string.IsNullOrEmpty(this.PopupInfoContainer.IconUrl) || this.PopupInfoContainer.IconUrl == "WebResource")
				imagePath = this.PopupInfoContainer.IconWebResource;
			else
				imagePath = this.ResolveUrl(this.PopupInfoContainer.IconUrl);

			// Get the HTML content of the popup control. This will be assigned to the content property of the CA dialog control.
			string dialogContent = GenerateDialogContent();

			// Generate the function call to be invoked when the help icon is clicked.
			string helpIconClickFunc = String.Format(CultureInfo.CurrentCulture, "helpIconClick({0}, '{1}', '{2}');", this.PopupInfoContainer.DialogControlId, this.ControlInstance.ClientID + "_help", dialogContent);

			// Combine everything together to generate the html to be insert after the target control.
			string html = String.Format(CultureInfo.CurrentCulture, "&nbsp;<img id=\"{0}\" src=\"{1}\" alt=\"\" style=\"width:16px;height:16px;vertical-align:bottom;padding-bottom:2px;\" onclick=\"{2}\" onmouseover=\"style.cursor='pointer';_popupInMouseOver=true;\" onmouseout=\"_popupInMouseOver=false;\" />&nbsp;", this.ControlInstance.ClientID + "_help", imagePath, helpIconClickFunc);

			// Fix up message so ' are allowed
			html = html.Replace("'", @"\'");

			// Since we can't directly add controls to the page, we have to do it via HTML. Generate the call to the 
			// javascript function that will render our HTML for us. We'll return it and let the calling code ensure
			// the javascript is sent to the page.
			return String.Format(CultureInfo.CurrentCulture, "addHtmlAfterControl('{0}','{1}');\r\n", this.ControlInstance.ClientID, html);
			
			//this.PopupInfoContainer.Page.ClientScript.RegisterStartupScript(this.GetType(), this.ControlId,
			//  String.Format(CultureInfo.CurrentCulture, "addHtmlAfterControl('{0}','{1}');\r\n", this.ControlInstance.ClientID, html), true);
		}

		private string GenerateDialogContent()
		{
			string titleCss = (!String.IsNullOrEmpty(this.DialogTitleCss) ? this.DialogTitleCss : this.PopupInfoContainer.DefaultDialogTitleCss);
			string bodyCss = (!String.IsNullOrEmpty(this.DialogBodyCss) ? this.DialogBodyCss : this.PopupInfoContainer.DefaultDialogBodyCss);

			string html = String.Format(System.Globalization.CultureInfo.CurrentCulture, "<div onmouseover=&quot;_popupInMouseOver=true;&quot; onmouseout=&quot;_popupInMouseOver=false;&quot;><div class=&quot;{0}&quot;>{1}</div><div class=&quot;{2}&quot;>{3}</div></div>", titleCss, this.DialogTitle, bodyCss, this.DialogBody);

			return html;
		}

		#endregion

	}

}
