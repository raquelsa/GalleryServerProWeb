<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="galleries.ascx.cs" Inherits="GalleryServerPro.Web.Pages.Admin.galleries" %>
<%@ Register Src="../../Controls/popupinfo.ascx" TagName="popup" TagPrefix="uc1" %>
<%@ Register Assembly="GalleryServerPro.WebControls" Namespace="GalleryServerPro.WebControls"
	TagPrefix="tis" %>
<%@ Register Namespace="ComponentArt.Web.UI" Assembly="ComponentArt.Web.UI" TagPrefix="ComponentArt" %>
<%@ Import Namespace="GalleryServerPro.Web" %>
<div id="css" runat="server" visible="false">
	<link href="../../styles/gallery.css" rel="stylesheet" type="text/css" />
</div>
<div class="gsp_indentedContent">
	<asp:PlaceHolder ID="phAdminHeader" runat="server" />
	<div class="gsp_addpadding1">
		<p class="gsp_msgdark">
			<asp:Label ID="lbl1" runat="server" CssClass="gsp_bold" Text="<%$ Resources:GalleryServerPro, Admin_Settings_Apply_To_Label %>"
				EnableViewState="false" />&nbsp;<asp:Literal ID="Literal2" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_All_Galleries_Label %>" /></p>
		<tis:wwErrorDisplay ID="wwMessage" runat="server" UserMessage="<%$ Resources:GalleryServerPro, Validation_Summary_Text %>"
			CellPadding="2" UseFixedHeightWhenHiding="False" Center="False" Width="500px">
		</tis:wwErrorDisplay>
		<p class="admin_h3" style="margin-top: 0;">
			<asp:Label ID="lblCurrentGalleryHdr" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_CurrentGallery_Hdr %>" /></p>
		<div class="gsp_addleftpadding6">
			<p class="gsp_addtopmargin5">
				<asp:Label ID="lblCurrentGallery" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_CurrentGallery_Label %>" />&nbsp;<asp:DropDownList
					ID="ddlCurrentGallery" runat="server" />
				&nbsp;
				<asp:LinkButton ID="lbChangeGallery" runat="server" OnClick="lbChangeGallery_Click"
					Text="<%$ Resources: GalleryServerPro, Admin_Gallery_Settings_Change_Gallery_Label %>"
					ToolTip="<%$ Resources: GalleryServerPro, Admin_Gallery_Settings_Change_Gallery_Tooltip %>"></asp:LinkButton>
			</p>
		</div>
		<p class="admin_h3">
			<asp:Label ID="lblManageGalleriesHdr" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_Galleries_Hdr %>" /></p>
		<div class="gsp_addleftpadding6">
			<asp:GridView ID="gvGalleries" runat="server" DataSourceID="odsGalleries" EnableViewState="true"
				AllowPaging="True" AutoGenerateColumns="False" ShowFooter="True" DataKeyNames="GalleryId"
				OnRowCommand="gvGalleries_RowCommand" OnRowDataBound="gvGalleries_RowDataBound"
				CssClass="gsp_displayTable" HeaderStyle-CssClass="gsp_displayTableRow">
				<Columns>
					<asp:TemplateField ShowHeader="False">
						<EditItemTemplate>
							<div class="gsp_g_edit_cell">
								<asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="True" CommandName="Update"
									Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_Update_Gallery_Text %>"
									ToolTip="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_Update_Gallery_Tooltip %>"></asp:LinkButton>
								<asp:LinkButton ID="LinkButton2" runat="server" CausesValidation="False" CommandName="Cancel"
									Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_Cancel_Edit_Gallery_Text %>"
									ToolTip="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_Cancel_Edit_Gallery_Tooltip %>"></asp:LinkButton>
							</div>
						</EditItemTemplate>
						<FooterTemplate>
							<div class="gsp_g_edit_cell">
								<asp:LinkButton ID="lbInsert" runat="server" CommandName="Insert" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_Create_Gallery_Link_Text %>"
									ToolTip="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_Create_Gallery_Link_ToolTip %>" />
							</div>
						</FooterTemplate>
						<ItemTemplate>
							<div class="gsp_g_edit_cell">
								<asp:HyperLink ID="hlViewGallery" runat="server" Visible="False" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_View_Gallery_Link_Text %>"
									ToolTip="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_View_Gallery_Link_Tooltip %>" />
								<asp:LinkButton ID="lbEditGallery" runat="server" CausesValidation="False" CommandName="Edit"
									Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_Edit_Gallery_Link_Text %>"
									ToolTip="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_Edit_Gallery_Link_ToolTip %>"></asp:LinkButton>
								<asp:LinkButton ID="lbDeleteGallery" runat="server" CausesValidation="False" CommandName="Delete"
									Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_Delete_Gallery_Link_Text %>"
									ToolTip="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_Delete_Gallery_Link_ToolTip %>"></asp:LinkButton></div>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:BoundField DataField="GalleryId" HeaderText="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_Gallery_ID_Hdr_Text %>"
						InsertVisible="False" ReadOnly="True" SortExpression="GalleryId">
						<ItemStyle HorizontalAlign="Center" />
					</asp:BoundField>
					<asp:TemplateField HeaderText="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_Gallery_Description_Hdr_Text %>"
						SortExpression="Description" HeaderStyle-HorizontalAlign="Left">
						<EditItemTemplate>
							<asp:TextBox ID="txtDescriptionUpdate" runat="server" Text='<%# Bind("Description") %>'></asp:TextBox>
						</EditItemTemplate>
						<ItemTemplate>
							<asp:Label ID="Label1" runat="server" Text='<%# Utils.HtmlEncode((string)Eval("Description")) %>'></asp:Label>
						</ItemTemplate>
						<FooterTemplate>
							<asp:TextBox ID="txtDescriptionInsert" runat="server" Text="" />
						</FooterTemplate>
					</asp:TemplateField>
					<asp:BoundField DataField="CreationDate" HeaderText="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_Gallery_DateCreated_Hdr_Text %>"
						InsertVisible="false" ReadOnly="true" SortExpression="CreationDate" DataFormatString="{0:G}" />
					<asp:TemplateField HeaderText="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_MediaPath_Hdr_Text %>"
						SortExpression="Description" HeaderStyle-HorizontalAlign="Left">
						<ItemTemplate>
							<asp:Label ID="lblMediaPath" runat="server"></asp:Label>
						</ItemTemplate>
						<EditItemTemplate>
							<asp:Label ID="lblMediaPath" runat="server"></asp:Label>
						</EditItemTemplate>
					</asp:TemplateField>
				</Columns>
			</asp:GridView>
		</div>
	</div>
	<tis:PopupInfo ID="PopupInfo" runat="server" DialogControlId="dgPopup" DefaultDialogTitleCss="dg5ContentTitleCss"
		DefaultDialogBodyCss="dg5ContentBodyCss">
		<PopupInfoItems>
			<tis:PopupInfoItem ID="poi1" runat="server" ControlId="lblCurrentGalleryHdr" DialogTitle="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_CurrentGallery_Info_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_CurrentGallery_Info_Bdy %>" />
			<tis:PopupInfoItem ID="poi2" runat="server" ControlId="lblManageGalleriesHdr" DialogTitle="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_ManageGalleries_Info_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_ManageGalleries_Info_Bdy %>" />
		</PopupInfoItems>
	</tis:PopupInfo>
	<uc1:popup ID="ucPopupContainer" runat="server" />
	<asp:PlaceHolder ID="phAdminFooter" runat="server" />
</div>
<asp:ObjectDataSource ID="odsGalleries" runat="server" DataObjectTypeName="GalleryServerPro.Business.Gallery"
	DeleteMethod="DeleteGallery" SelectMethod="GetGalleriesCurrentUserCanAdminister"
	TypeName="GalleryServerPro.Web.Controller.GalleryController" UpdateMethod="UpdateGallery"
	OnUpdated="odsGalleries_Updated" OnUpdating="odsGalleries_Updating" OnDeleted="odsGalleries_Deleted"
	OnDeleting="odsGalleries_Deleting">
	<UpdateParameters>
		<asp:Parameter Name="CreationDate" Type="DateTime" />
	</UpdateParameters>
</asp:ObjectDataSource>
