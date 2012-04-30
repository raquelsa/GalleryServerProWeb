using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using GalleryServerPro.WebControls.Tools;
using System.Drawing.Design;

namespace GalleryServerPro.WebControls
{
	/// <summary>
	/// A simple ErrorDisplay control that can be used to display errors consistently
	/// on Web Pages. The class includes several ways to do display data (ShowError,
	/// ShowMessage) as well as direct assignment and lists displays.
	/// </summary>
	[ToolboxBitmap(typeof(ValidationSummary))]
	[ToolboxData("<{0}:wwErrorDisplay runat='server' />")]
	public class wwErrorDisplay : Control
	{
		/// <summary>
		/// The detail text of the error message
		/// </summary>
		[Description("The error message to be displayed."), Category("ErrorMessage"), DefaultValue("")]
		public string Text
		{
			get
			{
				return _Text;
			}
			set
			{
				_Text = value;
			}
		}
		private string _Text = "";


		/// <summary>
		/// The message to display above the error message.
		/// For example: Please correct the following:
		/// </summary>
		[Description("The message to display above the error strings."), Category("ErrorMessage"), DefaultValue("")]
		public string UserMessage
		{
			get
			{
				return _UserMessage;
			}
			set
			{
				_UserMessage = value;
			}
		}
		private string _UserMessage = "";

		/// <summary>
		/// Image displayed with the error message
		/// </summary>
		[Description("The image to display when an error occurs. Default is WarningResource which is loaded as a resource image."), Category("ErrorMessage"),
	 Editor("System.Web.UI.Design.ImageUrlEditor", typeof(UITypeEditor)), DefaultValue("WarningResource")]
		public string ErrorImage
		{
			get
			{
				return _ErrorImage;
			}
			set
			{
				_ErrorImage = value;
			}
		}
		private string _ErrorImage = "WarningResource";


		[Description("The image to display when ShowMessage is called. Default value is InfoResource which loads an image resource."), Category("ErrorMessage"),
		 Editor("System.Web.UI.Design.ImageUrlEditor", typeof(UITypeEditor)), DefaultValue("InfoResource")]
		public string InfoImage
		{
			get { return _InfoImage; }
			set { _InfoImage = value; }
		}
		private string _InfoImage = "InfoResource";



		/// <summary>
		/// Determines whether the display box is centered
		/// </summary>
		[Category("ErrorMessage"), Description("Centers the Error Display on the page."), DefaultValue(true)]
		public bool Center
		{
			get
			{
				return _CenterDisplay;
			}
			set
			{
				_CenterDisplay = value;
			}
		}
		private bool _CenterDisplay = true;

		/// <summary>
		/// Determines whether the control keeps its space padding
		/// when it is is hidden in order not to jump the display
		/// </summary>
		public bool UseFixedHeightWhenHiding
		{
			get { return _UseFixedHeightWhenHiding; }
			set { _UseFixedHeightWhenHiding = value; }
		}
		private bool _UseFixedHeightWhenHiding = false;


		/// <summary>
		/// Determines how the error dialog renders
		/// </summary>
		[Category("ErrorMessage"), Description("Determines whether the control renders text or Html"), DefaultValue(RenderModes.Html)]
		public RenderModes RenderMode
		{
			get
			{
				return _RenderMode;
			}
			set
			{
				_RenderMode = value;
			}
		}
		private RenderModes _RenderMode = RenderModes.Html;

		/// <summary>
		/// The width of the ErrorDisplayBox
		/// </summary>
		[Description("The width for the control")]
		public Unit Width
		{
			get
			{
				return _Width;
			}
			set
			{
				_Width = value;
			}
		}
		private Unit _Width = Unit.Pixel(400);

		/// <summary>
		/// Determines the padding inside of the error display box.
		/// </summary>
		[Description("The Cellpadding for the wrapper table that bounds the Error Display."), DefaultValue("10")]
		public string CellPadding
		{
			get
			{
				return _CellPadding;
			}
			set
			{
				_CellPadding = value;
			}
		}
		private string _CellPadding = "10";

		/// <summary>
		/// The CSS Class used for the table and column to display this item.
		/// </summary>
		[DefaultValue("errordisplay")]
		public string CssClass
		{
			get
			{
				return _CssClass;
			}
			set
			{
				_CssClass = value;
			}
		}
		private string _CssClass = "errordisplay";


		/// <summary>
		/// A timeout in milliseconds for how long the error display is visible. 0 means no timeout.
		/// </summary>
		[Description("A timeout in milliseconds for how long the error display is visible. 0 means no timeout."), DefaultValue(0)]
		public int DisplayTimeout
		{
			get { return _DisplayTimeout; }
			set { _DisplayTimeout = value; }
		}
		private int _DisplayTimeout = 0;

		protected override void Render(HtmlTextWriter writer)
		{
			if (Text == "" && !this.DesignMode)
			{
				base.Render(writer);
				return;
			}

			if (RenderMode == RenderModes.Text)
				Text = wwUtils.DisplayMemo(this.Text);

			//if (this.Center)
			//    writer.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, "center");
			//writer.RenderBeginTag(HtmlTextWriterTag.Div);

			// *** <Center> is still the only reliable way to get block structures centered
			if (this.Center)
				writer.RenderBeginTag(HtmlTextWriterTag.Center);

			writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
			writer.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClass);
			writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, this.CellPadding);
			writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "30px");
			writer.AddStyleAttribute(HtmlTextWriterStyle.Width, this.Width.ToString());
			writer.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, "left");

			writer.RenderBeginTag(HtmlTextWriterTag.Table);

			writer.RenderBeginTag(HtmlTextWriterTag.Tr);

			// *** Set up  image <td> tag
			writer.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClass);
			writer.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
			writer.AddStyleAttribute(HtmlTextWriterStyle.BorderWidth, "0px");
			writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "16px");

			writer.RenderBeginTag(HtmlTextWriterTag.Td);

			if (this.ErrorImage != "")
			{
				string ImageUrl = this.ErrorImage.ToLower();
				if (ImageUrl == "warningresource")
					ImageUrl = this.Page.ClientScript.GetWebResourceUrl(this.GetType(), ControlsResources.WARNING_ICON_RESOURCE);
				else if (ImageUrl == "inforesource")
					ImageUrl = this.Page.ClientScript.GetWebResourceUrl(this.GetType(), ControlsResources.INFO_ICON_RESOURCE);
				else
					ImageUrl = this.ResolveUrl(this.ErrorImage);

				writer.AddAttribute(HtmlTextWriterAttribute.Src, ImageUrl);
				writer.RenderBeginTag(HtmlTextWriterTag.Img);
				writer.RenderEndTag();
			}

			writer.RenderEndTag();  // image <td>

			// *** Render content <td> tag
			writer.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);

			if (this.UserMessage != "")
				writer.Write("<span style='font-weight:normal'>" + this.UserMessage + "</span><hr />");

			writer.Write(this.Text);

			writer.RenderEndTag();  // Content <td>
			writer.RenderEndTag();  // <tr>
			writer.RenderEndTag();  // <table>

			if (this.Center)
				writer.RenderEndTag();   // </center>

			writer.WriteBreak();

			//writer.RenderEndTag();  // </div>
		}

		protected override void OnPreRender(EventArgs e)
		{
			if (this.DisplayTimeout > 0)
			{
				// *** Use wwHoverPanel.js library code
				string ScriptResource = this.Page.ClientScript.GetWebResourceUrl(typeof(wwErrorDisplay), ControlsResources.HOVERPANEL_SCRIPT_RESOURCE);
				this.Page.ClientScript.RegisterClientScriptInclude("wwHoverPanel", ScriptResource);

				string Script =
@"window.setTimeout(""Fadeout('" + this.ClientID + @"',true,2);""," + this.DisplayTimeout.ToString() + @");";

				//@"window.setTimeout(""document.getElementById('" + this.ClientID + @"').style.display='none';""," + this.DisplayTimeout.ToString() + @");";

				this.Page.ClientScript.RegisterStartupScript(typeof(wwErrorDisplay), "DisplayTimeout", Script, true);
			}
			base.OnPreRender(e);
		}

		/// <summary>
		/// Assigns an error message to the control
		/// </summary>
		/// <param name="Text"></param>
		public void ShowError(string Text)
		{
			this.ShowError(Text, null);
		}

		/// <summary>
		/// Assigns an error message to the control as well as a UserMessage
		/// </summary>
		/// <param name="Text"></param>
		/// <param name="Message"></param>
		public void ShowError(string Text, string Message)
		{
			this.Text = Text;

			if (Message != null)
				this.UserMessage = Message;
			else
				this.UserMessage = "";

			this.Visible = true;
		}

		/// <summary>
		/// Displays a simple message in the display area along with the info icon if set.
		/// </summary>
		/// <param name="Message"></param>
		public void ShowMessage(string Message)
		{
			this.UserMessage = "";
			this.ErrorImage = this.InfoImage;
			this.Text = Message;
			this.Visible = true;
		}
	}

	public enum RenderModes
	{
		/// <summary>
		/// Error Text is Text and needs fixing up
		/// </summary>
		Text,
		/// <summary>
		/// The text is HTML and ready to display
		/// </summary>
		Html,
		/// <summary>
		/// Text is plain text and should be rendered as a bullet list
		/// </summary>
		TextAsBulletList
	}
}
