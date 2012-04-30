<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="deletealbum.ascx.cs"
	Inherits="GalleryServerPro.Web.Pages.Task.deletealbum" %>
<%@ Register Src="../../controls/popupinfo.ascx" TagName="popup" TagPrefix="uc1" %>
<%@ Register Src="../../controls/albumheader.ascx" TagName="albumheader" TagPrefix="uc1" %>
<%@ Register Src="../../Controls/usermessage.ascx" TagName="usermessage" TagPrefix="uc1" %>
<%@ Register Assembly="GalleryServerPro.WebControls" Namespace="GalleryServerPro.WebControls" TagPrefix="tis" %>
<div class="gsp_content">
	<asp:PlaceHolder ID="phTaskHeader" runat="server" />
	<uc1:usermessage ID="ucUserMessage" runat="server" Visible="false" CssClass="um0ContainerCss"
		IconStyle="Error" DetailCssClass="um1DetailCss" />
	<p class="gsp_textcol gsp_msgwarning">
		<asp:Literal ID="litId" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Delete_Album_Warning %>" /></p>
	<uc1:albumheader ID="ucAlbumHeader" runat="server" EnableAlbumDownload="false" />
	<p>
		<asp:CheckBox ID="chkDeleteDbRecordsOnly" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Delete_Album_DeleteDbRecordsOnly_Lbl %>" /></p>
	<tis:PopupInfo ID="pi" runat="server" DialogControlId="dgPopup" DefaultDialogTitleCss="dg5ContentTitleCss"
		DefaultDialogBodyCss="dg5ContentBodyCss">
		<PopupInfoItems>
			<tis:PopupInfoItem ID="poi1" runat="server" ControlId="chkDeleteDbRecordsOnly" DialogTitle="<%$ Resources:GalleryServerPro, Task_Delete_Album_DeleteDbRecordsOnly_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Task_Delete_Album_DeleteDbRecordsOnly_Bdy %>" />
		</PopupInfoItems>
	</tis:PopupInfo>
	<uc1:popup ID="ucPopupContainer" runat="server" />
	<asp:PlaceHolder ID="phTaskFooter" runat="server" />
</div>
