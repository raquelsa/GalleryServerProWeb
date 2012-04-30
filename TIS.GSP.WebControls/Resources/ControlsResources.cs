using System;
using System.Collections.Generic;
using System.Text;

using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

[assembly: WebResource("GalleryServerPro.WebControls.Resources.wwHoverPanel.js", "text/javascript")]
[assembly: WebResource("GalleryServerPro.WebControls.Resources.warning.gif", "image/gif")]
[assembly: WebResource("GalleryServerPro.WebControls.Resources.info.gif", "image/gif")]
[assembly: WebResource("GalleryServerPro.WebControls.Resources.help_16x16.png", "image/png")]

namespace GalleryServerPro.WebControls
{
	/// <summary>
	/// Class is used as to consolidate access to resources
	/// </summary>
	public class ControlsResources
	{
		public const string HOVERPANEL_SCRIPT_RESOURCE = "GalleryServerPro.WebControls.Resources.wwHoverPanel.js";
		public const string INFO_ICON_RESOURCE = "GalleryServerPro.WebControls.Resources.info.gif";
		public const string WARNING_ICON_RESOURCE = "GalleryServerPro.WebControls.Resources.warning.gif";
		public const string HELP_ICON_RESOURCE = "GalleryServerPro.WebControls.Resources.help_16x16.png";
	}
}
