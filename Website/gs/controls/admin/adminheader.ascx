<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="adminheader.ascx.cs" Inherits="GalleryServerPro.Web.Controls.Admin.adminheader" %>
<p class="admin_h1">
	<asp:Literal ID="litPageHeader" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Header %>" />
</p>
<div style="float: right">
	<p class="gsp_minimargin">
		<asp:Button ID="btnOkTop" runat="server" Text="<%$ Resources:GalleryServerPro, Default_Task_Ok_Button_Text %>"
			TabIndex="1" AccessKey="O" />
		<asp:Button ID="btnCancelTop" runat="server" OnClick="btnCancel_Click" CausesValidation="false"
			Text="<%$ Resources:GalleryServerPro, Default_Task_Cancel_Button_Text %>" ToolTip="<%$ Resources:GalleryServerPro, Default_Task_Cancel_Button_Tooltip %>" />&nbsp;</p>
</div>
<p class="admin_h2">
	<asp:Label ID="lblAdminPageHeader" runat="server" />
</p>
