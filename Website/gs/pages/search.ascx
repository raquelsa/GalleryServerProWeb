<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="search.ascx.cs" Inherits="GalleryServerPro.Web.Pages.search" %>
<%@ Register Src="../controls/thumbnailview.ascx" TagName="thumbnailview" TagPrefix="gs" %>
<%@ Register Src="../controls/popupinfo.ascx" TagName="popup" TagPrefix="gs" %>
<%@ Register Assembly="GalleryServerPro.WebControls" Namespace="GalleryServerPro.WebControls" TagPrefix="tis" %>
<%@ Import Namespace="GalleryServerPro.Web" %>
<asp:Image ID="imgSearchIcon" runat="server" style="display:block;float:left;padding:10px 5px 5px 0;width:48px;height:48px;" AlternateText="" /><p class="gsp_h1">
	<asp:Literal ID="lit1" runat="server" Text="<%$ Resources:GalleryServerPro, Search_Hdr %>" /></p>
<p>
	<asp:Literal ID="lit2" runat="server" Text="<%$ Resources:GalleryServerPro, Search_Box_Hdr %>" />
	<asp:TextBox ID="txtSearch" runat="server" Width="350px"></asp:TextBox>&nbsp;<asp:Button
		ID="btnSearch" runat="server" Text="<%$ Resources:GalleryServerPro, Search_Search_Button_Text %>" /><asp:Label
			ID="lblZipInfo" runat="server" /></p>
<p id="searchResultTitle" runat="server" class="gsp_h2">
</p>
<gs:thumbnailview ID="tv" runat="server" EnableViewState="false" />
<tis:PopupInfo ID="PopupInfo" runat="server" DialogControlId="dgPopup" DefaultDialogTitleCss="dg5ContentTitleCss"
	DefaultDialogBodyCss="dg5ContentBodyCss">
	<PopupInfoItems>
		<tis:PopupInfoItem ID="pi1" runat="server" ControlId="lblZipInfo" DialogTitle="<%$ Resources:GalleryServerPro, Search_Popup_Help_Hdr %>"
			DialogBody="<%$ Resources:GalleryServerPro, Search_Popup_Help_Bdy %>" />
	</PopupInfoItems>
</tis:PopupInfo>
<gs:popup ID="ucPopupContainer" runat="server" />
