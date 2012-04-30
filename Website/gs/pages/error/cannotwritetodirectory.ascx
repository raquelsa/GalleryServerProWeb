<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="cannotwritetodirectory.ascx.cs"
	Inherits="GalleryServerPro.Web.Pages.Err.cannotwritetodirectory" %>
<style type="text/css">
	body {font: 12px Verdana, Arial, Helvetica, sans-serif;}
	img { border: none; }
	div#gsp_errContainer { margin: 25px auto auto auto; width: 800px; height: 500px; border: 3px solid #61002B;border-radius:10px;-moz-border-radius:10px;-webkit-border-radius:10px; }
	div#gsp_errContainer div#gsp_errHeader { height: 75px; background-color: #ECF1F6; border-bottom: 1px solid #61002B; position: relative;border-radius:6px 6px 0 0;-moz-border-radius:6px 6px 0 0;-webkit-border-top-right-radius:6px;-webkit-border-top-left-radius:6px; }
	div#gsp_errContainer div#gsp_errContent { height: 395px; overflow: auto; background-color: #ECECD9; padding: 1em; }
	div#gsp_errContainer div#gsp_errContent p { margin: 0 0 1em 0; }
	div#gsp_errContainer div#gsp_errContent p.gsp_h2 { margin: 0 0 1em 0; padding: 0; background-color: Transparent;font-size: 1.2em; font-weight: bold;  }
</style>
<div id="gsp_errContainer">
	<div id="gsp_errHeader">
		<asp:Image ID="imgGspLogo" runat="server" AlternateText="Gallery Server Pro logo"
			Width="313" Height="75" />
	</div>
	<div id="gsp_errContent">
		<p>
			<asp:HyperLink ID="hlHome" runat="server" ToolTip="<%$ Resources:GalleryServerPro, Error_Home_Page_Link_Text %>"
				Text="<%$ Resources:GalleryServerPro, Error_Home_Page_Link_Text %>" /></p>
		<p class="gsp_h2">
			<asp:Literal ID="l2" runat="server" Text="<%$ Resources:GalleryServerPro, Error_Cannot_Write_To_Directory_Hdr %>" /></p>
		<p>
			<asp:Literal ID="litErrorInfo" runat="server" /></p>
		<p style="font-size: 1.1em; font-weight: bold; margin: 0; color: #063;">
			<asp:Literal ID="l3" runat="server" Text="<%$ Resources:GalleryServerPro, Error_Cannot_Write_To_Directory_Hint_Hdr %>" />
		</p>
		<p style="color: #063;">
			<asp:Literal ID="l4" runat="server" Text="<%$ Resources:GalleryServerPro, Error_Cannot_Write_To_Directory_Hint_Dtl %>" />
		</p>
	</div>
</div>
