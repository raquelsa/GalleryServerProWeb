using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GalleryServerPro.Web.Controls
{
	/// <summary>
	/// A user control used for communicating messages to the user.
	/// </summary>
	public partial class usermessage : GalleryUserControl
	{
		#region Constant Definitions

		private const string DEFAULT_HEADER_DETAIL_WITH_IMAGE_CSS = "um0HeaderDetailWithImageCss";
		private const string DEFAULT_HEADER_DETAIL_WITHOUT_IMAGE_CSS = "um0HeaderDetailWithoutImageCss";

		#endregion

		#region Private Fields

		private MessageStyle _iconStyle;

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets the type of image to be displayed on the left side of the control. Each enum value is mapped to a specific image
		/// that is 48px wide by 48px high.
		/// </summary>
		public MessageStyle IconStyle
		{
			get { return _iconStyle; }
			set
			{
				_iconStyle = value;

				if (this._iconStyle == MessageStyle.None)
					RemoveImage();
				else
					ConfigureImage();
			}
		}

		/// <summary>
		/// Gets or sets the title to be displayed to the user. May contain HTML.
		/// </summary>
		public string MessageTitle
		{
			get
			{
				if (pnlHeader.HasControls())
				{
					LiteralControl textControl = pnlHeader.Controls[0] as LiteralControl;
					if (textControl != null)
					{
						return textControl.Text;
					}
				}
				return String.Empty;
			}
			set
			{
				pnlHeader.Controls.Clear();
				pnlHeader.Controls.Add(new LiteralControl(value));
			}
		}

		/// <summary>
		/// Gets or sets the message text to be displayed to the user. May contain HTML.
		/// </summary>
		public string MessageDetail
		{
			get
			{
				if (pnlDetail.HasControls())
				{
					LiteralControl textControl = pnlDetail.Controls[0] as LiteralControl;
					if (textControl != null)
					{
						return textControl.Text;
					}
				}
				return String.Empty;
			}
			set
			{
				pnlDetail.Controls.Clear();
				pnlDetail.Controls.Add(new LiteralControl(value));
			}
		}

		/// <summary>
		/// Gets or sets the CSS class used to style the div containing this user message. Default is um0ContainerCss.
		/// </summary>
		public string CssClass
		{
			get { return pnlMessageContainer.CssClass; }
			set { pnlMessageContainer.CssClass = value; }
		}

		/// <summary>
		/// Gets or sets the CSS class used to style the div containing the title text. Default is um0HeaderCss.
		/// </summary>
		public string HeaderCssClass
		{
			get { return pnlHeader.CssClass; }
			set { pnlHeader.CssClass = value; }
		}

		/// <summary>
		/// Gets or sets the CSS class used to style the div containing the message text. Default is um0DetailCss.
		/// </summary>
		public string DetailCssClass
		{
			get { return pnlDetail.CssClass; }
			set { pnlDetail.CssClass = value; }
		}

		/// <summary>
		/// Gets a reference to the Panel control that is the container for this message control.
		/// </summary>
		public Panel MessageContainer
		{
			get
			{
				return pnlMessageContainer;
			}
		}

		/// <summary>
		/// Gets a reference to the Panel control that is the container for the header message.
		/// </summary>
		public Panel MessageHeaderContainer
		{
			get
			{
				return pnlHeader;
			}
		}

		/// <summary>
		/// Gets a reference to the Panel control that is the container for the detail message.
		/// </summary>
		public Panel MessageDetailContainer
		{
			get
			{
				return pnlDetail;
			}
		}

		#endregion

		#region Protected Events

		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
		}

		/// <summary>
		/// Handles the PreRender event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_PreRender(object sender, EventArgs e)
		{
			// If no text has been assigned to the header or detail control, then remove the corresponding panel
			// so that no <div> tag is rendered.
			if (String.IsNullOrEmpty(this.MessageTitle))
			{
				pnlHeaderAndDetailContainer.Controls.Remove(pnlHeader);
			}

			if (String.IsNullOrEmpty(this.MessageDetail))
			{
				pnlHeaderAndDetailContainer.Controls.Remove(pnlDetail);
			}
		}

		#endregion

		#region Private Methods

		private void RemoveImage()
		{
			pnlMessageContainer.Controls.Remove(this.imgIcon);
			pnlHeaderAndDetailContainer.CssClass = pnlHeaderAndDetailContainer.CssClass.Replace(DEFAULT_HEADER_DETAIL_WITH_IMAGE_CSS, String.Empty);
		}

		private void ConfigureImage()
		{
			string imagePath = string.Concat(Utils.GalleryRoot, "/images");
			string imageName = null;
			switch (this.IconStyle)
			{
				case MessageStyle.Information: imageName = "info_48x48.png"; break;
				case MessageStyle.Warning: imageName = "warning_48x48.png"; break;
				case MessageStyle.Error: imageName = "error_48x48.png"; break;
				default: throw new System.ComponentModel.InvalidEnumArgumentException("The ConfigureImage() method in the usermessage control has encountered a MessageStyle enumeration item it has not been designed to handle.");
			}

			imgIcon.ImageUrl = string.Concat(imagePath, "/", imageName);
			imgIcon.Width = new Unit(48);
			imgIcon.Height = new Unit(48);

			// Make sure the correct css class is specified.
			if (!pnlHeaderAndDetailContainer.CssClass.Contains(DEFAULT_HEADER_DETAIL_WITH_IMAGE_CSS))
			{
				// Doesn't have the class, so add it.
				pnlHeaderAndDetailContainer.CssClass = String.Concat(pnlHeaderAndDetailContainer.CssClass, " ", DEFAULT_HEADER_DETAIL_WITH_IMAGE_CSS);
			}
			else if (pnlHeaderAndDetailContainer.CssClass.Contains(DEFAULT_HEADER_DETAIL_WITHOUT_IMAGE_CSS))
			{
				// It has the class associated when no image is specified, so replace it with the correct one.
				pnlHeaderAndDetailContainer.CssClass = pnlHeaderAndDetailContainer.CssClass.Replace(DEFAULT_HEADER_DETAIL_WITHOUT_IMAGE_CSS, DEFAULT_HEADER_DETAIL_WITH_IMAGE_CSS);
			}

			// If the image was removed from its containing panel become the developer previously set the IconStyle
			// property to None, then add it back. If we don't, it won't render because it is not contained in any control.
			if (pnlMessageContainer.FindControl(imgIcon.ID) == null)
			{
				pnlMessageContainer.Controls.AddAt(0, imgIcon);
			}
		}

		#endregion
	}
}