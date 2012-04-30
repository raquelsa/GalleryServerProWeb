<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="myaccount.ascx.cs" Inherits="GalleryServerPro.Web.Controls.myaccount" %>
<%@ Import Namespace="GalleryServerPro.Web" %>
<p class="gsp_useroption">
	<a href="<%= Utils.GetUrl(PageId.myaccount, "aid={0}", this.GalleryPage.GetAlbumId()) %>" class="gsp_myaccountlink">
		<img src="<%= Utils.GalleryRoot %>/images/edituser_32x32.png" style="width:32px;height:32px;" alt='<asp:Literal
			ID="l1" runat="server" Text="<%$ Resources:GalleryServerPro, Login_My_Account_Link_Text%>" />' title='<asp:Literal
			ID="l2" runat="server" Text="<%$ Resources:GalleryServerPro, Login_My_Account_Link_Text%>" />' /></a>
</p>