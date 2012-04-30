<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="generic.ascx.cs" Inherits="GalleryServerPro.Web.Pages.Err.generic" %>
<style type="text/css">
	img { border: none; }
	body {font: 12px Verdana, Arial, Helvetica, sans-serif;}
	.gsp_msgwarning { color: #c03; }
	div#gsp_errContainer { margin: 25px auto auto auto; width: 800px; height: 500px; border: 3px solid #61002B;border-radius:10px;-moz-border-radius:10px;-webkit-border-radius:10px; }
	div#gsp_errContainer div#gsp_errHeader { height: 75px; background-color: #ECF1F6; border-bottom: 1px solid #61002B; position: relative;border-radius:6px 6px 0 0;-moz-border-radius:6px 6px 0 0;-webkit-border-top-right-radius:6px;-webkit-border-top-left-radius:6px; }
	div#gsp_errContainer div#gsp_errContent { height: 395px; overflow: auto; background-color: #ECECD9; padding: 1em; }
	div#gsp_errContainer div#gsp_errContent p.gsp_h2 { margin: 0 0 1em 0; padding: 0; background-color: Transparent;font-size: 1.2em; font-weight: bold;  }
</style>
<div id="gsp_errContainer" class="gsp_ns">
	<div id="gsp_errHeader">
		<asp:Image ID="imgGspLogo" runat="server" AlternateText="Gallery Server Pro logo" Width="313" Height="75" />
	</div>
	<div id="gsp_errContent">
		<p>
			<asp:HyperLink ID="hlHome" runat="server" ToolTip="<%$ Resources:GalleryServerPro, Error_Home_Page_Link_Text %>"
				Text="<%$ Resources:GalleryServerPro, Error_Home_Page_Link_Text %>" /><asp:Label ID="lblSeparator" runat="server" Text=" | " /><asp:HyperLink ID="hlSiteAdmin" runat="server" ToolTip="<%$ Resources:GalleryServerPro, UC_ActionMenu_Admin_Console_Tooltip %>"
				Text="<%$ Resources:GalleryServerPro, UC_ActionMenu_Admin_Console_Text %>" /></p>
		<p class="gsp_h2">
		<asp:Literal ID="l2" runat="server" Text="<%$ Resources:GalleryServerPro, Error_Hdr %>" /></p>
		<p>
			<asp:Literal ID="l3" runat="server" Text="<%$ Resources:GalleryServerPro, Error_Dtl1 %>" /></p>
		<p id="pErrorDtl2" runat="server">
			<asp:Literal ID="l4" runat="server" Text="<%$ Resources:GalleryServerPro, Error_Dtl2 %>" /></p>
		<asp:Literal ID="litErrorDetails" runat="server" />
	</div>
</div>
