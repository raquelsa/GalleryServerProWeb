<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="createalbum.ascx.cs"
	Inherits="GalleryServerPro.Web.Pages.Task.createalbum" %>
<%@ Register Src="../../controls/albumtreeview.ascx" TagName="albumtreeview" TagPrefix="uc1" %>
<%@ Register Src="../../controls/popupinfo.ascx" TagName="popup" TagPrefix="uc1" %>
<%@ Register Assembly="GalleryServerPro.WebControls" Namespace="GalleryServerPro.WebControls"
	TagPrefix="tis" %>
<%@ Import Namespace="GalleryServerPro.Web" %>
<div class="gsp_content">
	<asp:PlaceHolder ID="phTaskHeader" runat="server" />
	<p class="gsp_h3">
		<asp:Literal ID="litStep1" runat="server" Text="<%$ Resources:GalleryServerPro, Task_All_Step_1_Text %>" /></p>
	<p>
		<asp:Label ID="lblTitle" runat="server" AssociatedControlID="txtTitle" Text="<%$ Resources:GalleryServerPro, Task_Create_Album_Title_Label_Text %>" />
		<asp:TextBox ID="txtTitle" runat="server" Style="width: 450px;" />
		<asp:Label ID="lblMaxTitleLengthInfo" runat="server" AssociatedControlID="txtTitle"
			CssClass="gsp_fs" />
		<asp:RequiredFieldValidator ID="rfv1" runat="server" ControlToValidate="txtTitle"
			Display="Dynamic" ErrorMessage="<%$ Resources:GalleryServerPro, Task_Create_Album_Missing_Title_Text %>"
			ForeColor="" CssClass="gsp_msgfailure">
		</asp:RequiredFieldValidator>
	</p>
	<p class="gsp_addleftmargin10" runat="server">
		<img src="<%= Utils.GalleryRoot %>/images/lock_24x24.png" style="width: 24px; height: 24px;
			vertical-align: bottom;" alt="" /><asp:CheckBox ID="chkIsPrivate" runat="server"
				Text="<%$ Resources:GalleryServerPro, Task_Create_Album_Is_Private_Text %>" />
		<asp:Label ID="lblPrivateAlbumIsInherited" runat="server" CssClass="gsp_fs gsp_em" />
	</p>
	<p class="gsp_h3 gsp_addtopmargin5">
		<asp:Literal ID="litStep2" runat="server" Text="<%$ Resources:GalleryServerPro, Task_All_Step_2_Text %>" /></p>
	<p>
		<asp:Literal ID="litInstructions" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Create_Album_Instructions %>" /></p>
	<div>
		<uc1:albumtreeview ID="tvUC" runat="server" AllowMultiCheck="false" RequireAlbumSelection="true" />
	</div>
	<tis:PopupInfo ID="PopupInfo1" runat="server" DialogControlId="dgPopup" DefaultDialogTitleCss="dg5ContentTitleCss"
		DefaultDialogBodyCss="dg5ContentBodyCss">
		<PopupInfoItems>
			<tis:PopupInfoItem ID="poi1" runat="server" ControlId="chkIsPrivate" DialogTitle="<%$ Resources:GalleryServerPro, Task_Create_Album_Make_Private_Album_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Task_Create_Album_Make_Private_Album_Bdy %>" />
		</PopupInfoItems>
	</tis:PopupInfo>
	<uc1:popup ID="ucPopupContainer" runat="server" />
	<asp:PlaceHolder ID="phTaskFooter" runat="server" />
</div>
