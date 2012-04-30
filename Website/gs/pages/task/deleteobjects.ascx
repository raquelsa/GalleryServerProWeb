<%@ Import Namespace="GalleryServerPro.Business" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="deleteobjects.ascx.cs"
	Inherits="GalleryServerPro.Web.Pages.Task.deleteobjects" %>
<%@ Register Src="../../controls/popupinfo.ascx" TagName="popup" TagPrefix="uc1" %>
<%@ Register Src="../../Controls/usermessage.ascx" TagName="usermessage" TagPrefix="uc1" %>
<%@ Register Assembly="GalleryServerPro.WebControls" Namespace="GalleryServerPro.WebControls"
	TagPrefix="tis" %>
<%@ Import Namespace="GalleryServerPro.Business.Interfaces" %>
<script type="text/javascript">

	function ToggleSelectAll(masterCheckbox) {
		var checked = masterCheckbox.checked;
		var inputs = $get('gsp_thmbcontainer').getElementsByTagName("input");
		for (var i = 0; i < inputs.length; i++) {
			if (inputs[i].type == 'checkbox') {
				inputs[i].checked = checked;
			}
		}
	}
	
</script>
<div class="gsp_content">
	<asp:PlaceHolder ID="phTaskHeader" runat="server" />
	<p class="gsp_textcol gsp_msgwarning gsp_addleftpadding1">
		<asp:Literal ID="litId" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Delete_Objects_Warning %>" /></p>
	<uc1:usermessage ID="ucUserMessage" runat="server" Visible="false" CssClass="um0ContainerCss"
		IconStyle="Information" DetailCssClass="um1DetailCss" />
	<p>
		<asp:CheckBox ID="chkDeleteDbRecordsOnly" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Delete_Objects_DeleteDbRecordsOnly_Lbl %>" /></p>
	<p>
		<input type="checkbox" id="chkSelectAll" onclick="javascript:ToggleSelectAll(this);" /><label
			for="chkSelectAll"><asp:Literal ID="Literal1" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Delete_Objects_Check_Uncheck_All_Label_Text %>" /></label></p>
	<asp:Repeater ID="rptr" runat="server">
		<HeaderTemplate>
			<div id="gsp_thmbcontainer" class="gsp_floatcontainer">
		</HeaderTemplate>
		<ItemTemplate>
			<div class="<%# GetThumbnailCssClass((IGalleryObject) Container.DataItem) %>">
				<%# GetThumbnailHtml((IGalleryObject) Container.DataItem, false, false) %>
				<p id="p" runat="server">
					<asp:CheckBox ID="chk" runat="server" Text='<%# GetGalleryObjectText(Eval("Title").ToString(), Container.DataItem.GetType()) %>'
						Enabled="<%# DoesUserHavePermissionToDeleteGalleryObject((IGalleryObject) Container.DataItem) %>" />
				</p>
				<asp:HiddenField ID="hdn" runat="server" Value="<%# GetId((IGalleryObject) Container.DataItem) %>" />
			</div>
		</ItemTemplate>
		<FooterTemplate>
			</div>
		</FooterTemplate>
	</asp:Repeater>
	<tis:PopupInfo ID="pi" runat="server" DialogControlId="dgPopup" DefaultDialogTitleCss="dg5ContentTitleCss"
		DefaultDialogBodyCss="dg5ContentBodyCss">
		<PopupInfoItems>
			<tis:PopupInfoItem ID="poi1" runat="server" ControlId="chkDeleteDbRecordsOnly" DialogTitle="<%$ Resources:GalleryServerPro, Task_Delete_Objects_DeleteDbRecordsOnly_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Task_Delete_Objects_DeleteDbRecordsOnly_Bdy %>" />
		</PopupInfoItems>
	</tis:PopupInfo>
	<uc1:popup ID="ucPopupContainer" runat="server" />
	<asp:PlaceHolder ID="phTaskFooter" runat="server" />
</div>
