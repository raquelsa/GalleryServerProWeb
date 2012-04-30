<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="albumheader.ascx.cs"
	Inherits="GalleryServerPro.Web.Controls.albumheader" EnableViewState="false" %>
<%@ Register tagPrefix="ComponentArt" namespace="ComponentArt.Web.UI" assembly="ComponentArt.Web.UI" %>
<%@ Import Namespace="GalleryServerPro.Web" %>
<asp:Panel ID="pnlAlbumHeader" runat="server">
	<div class="gsp_albumdetail">
		<p class="albumHeaderStats"><asp:Literal ID="litStats" runat="server" /></p>
		<p id="pADC" runat="server" class="gsp_useroption gsp_downloadzip"><a href='<%= Utils.GetUrl(PageId.task_downloadobjects, "aid={0}", GalleryPage.GetAlbumId()) %>' class=""><img src="<%= Utils.GalleryRoot %>/images/downloadzip.png" style="width:32px;height:32px;" alt='<asp:Literal
						ID="l1" runat="server" Text="<%$ Resources:GalleryServerPro, UC_Album_Header_Album_Download_Zip_Tooltip%>" />' title='<asp:Literal
						ID="Literal1" runat="server" Text="<%$ Resources:GalleryServerPro, UC_Album_Header_Album_Download_Zip_Tooltip%>" />' /></a></p>
	</div>
	<p class="gsp_h1">
		<asp:Literal ID="litAlbum" runat="server" Text="<%$ Resources:GalleryServerPro, UC_Album_Header_Album_Title_Header_Text %>" />
		<img id="albumPrivateIcon" src="<%= Utils.GalleryRoot %>/images/lock_24x24.png" style="width: 24px;
			height: 24px; margin-bottom: -5px;" class="<%= GetAlbumPrivateCssClass() %>" alt='<asp:Literal runat="server" Text="<%$ Resources:GalleryServerPro, UC_Album_Header_Album_Is_Private_Icon_Tooltip %>" />'
			title='<asp:Literal runat="server" Text="<%$ Resources:GalleryServerPro, UC_Album_Header_Album_Is_Private_Icon_Tooltip %>" />' />
		<asp:Label ID="lblTitle" runat="server" /></p>
	<div class="gsp_minimargin">
		<asp:Label ID="lblSummaryLabel" runat="server" CssClass="gsp_bold" Text="<%$ Resources:GalleryServerPro, UC_Album_Header_Album_Summary_Header_Text %>" />
		<asp:Label ID="lblSummary" runat="server" /></div>
	<p id="dateContainer" runat="server">
		<asp:Label ID="lblDateLabel" runat="server" CssClass="gsp_bold" Text="<%$ Resources:GalleryServerPro, UC_Album_Header_Album_Date_Header_Text %>" />
		<asp:Label ID="lblDate" runat="server" /></p>
</asp:Panel>
<asp:PlaceHolder ID="phEditAlbumDialog" runat="server" />