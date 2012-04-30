<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="usermessage.ascx.cs" Inherits="GalleryServerPro.Web.Controls.usermessage" %>
<asp:Panel ID="pnlMessageContainer" runat="server" CssClass="um0ContainerCss">
	<asp:Image ID="imgIcon" runat="server" style="float:left;padding:0.2em 0 0 0.2em;" />
	<asp:Panel ID="pnlHeaderAndDetailContainer" runat="server" CssClass="um0HeaderDetailWithImageCss">
		<asp:Panel id="pnlHeader" runat="server" CssClass="um0HeaderCss" />
		<asp:Panel id="pnlDetail" runat="server" CssClass="um0DetailCss" />
	</asp:Panel>
</asp:Panel>
