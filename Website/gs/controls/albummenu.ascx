<%@ Control Language="C#" AutoEventWireup="true" Codebehind="albummenu.ascx.cs" Inherits="GalleryServerPro.Web.Controls.albummenu" %>
<%@ Register tagPrefix="ComponentArt" namespace="ComponentArt.Web.UI" assembly="ComponentArt.Web.UI" %>
<div class="albumMenuContainer">
	<asp:PlaceHolder ID="phActionMenu" runat="server" />
	<div class="<% =GetAlbumMenuClass() %>">
		<asp:PlaceHolder ID="phMenu" runat="server" />
	</div>
</div>