<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="manageroles.ascx.cs"
	Inherits="GalleryServerPro.Web.Pages.Admin.manageroles" %>
<%@ Register Namespace="ComponentArt.Web.UI" Assembly="ComponentArt.Web.UI" TagPrefix="ComponentArt" %>
<%@ Register Src="../../Controls/popupinfo.ascx" TagName="popup" TagPrefix="uc1" %>
<%@ Register Src="../../Controls/albumtreeview.ascx" TagName="albumtreeview" TagPrefix="uc1" %>
<%@ Register Src="../../Controls/usermessage.ascx" TagName="usermessage" TagPrefix="uc1" %>
<%@ Register Assembly="GalleryServerPro.WebControls" Namespace="GalleryServerPro.WebControls"
	TagPrefix="tis" %>
<%@ Import Namespace="GalleryServerPro.Web" %>
<script type="text/javascript">
	Sys.WebForms.PageRequestManager.getInstance().add_initializeRequest(manageRolesInitializeRequest);
	Sys.WebForms.PageRequestManager.getInstance().add_endRequest(manageRolesEndRequest);
</script>
<div class="gsp_indentedContent">
	<p class="admin_h2">
		<asp:Label ID="lblAdminPageHeader" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_Page_Header %>" />
	</p>
	<div class="gsp_addpadding1">
		<p class="gsp_msgdark">
			<asp:Label ID="lbl1" runat="server" CssClass="gsp_bold" Text="<%$ Resources:GalleryServerPro, Admin_Settings_Apply_To_Label %>"
				EnableViewState="false" />&nbsp;<asp:Literal ID="Literal2" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_All_Galleries_Label %>" /></p>
		<uc1:usermessage ID="ucUserMessage" runat="server" CssClass="um0ContainerCss gsp_invisible"
			IconStyle="Error" MessageTitle="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_User_Message_Hdr %>"
			MessageDetail="Placeholder text - do not delete" />
		<tis:wwErrorDisplay ID="wwMessage" runat="server" CellPadding="2" UseFixedHeightWhenHiding="False"
			Center="False" Width="500px" />
		<p id="pAddRole" runat="server">
			<a id="addnewrole" href="javascript:addRoleClicked();">
				<asp:Literal ID="l0" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_Add_Role_Link_Text %>" /></a>
			<asp:LinkButton ID="lbAddRole" runat="server" OnClick="lbAddRole_Click" CssClass="gsp_hidden" />
		</p>
		<p>
			<input type="checkbox" id="chkShowOwnerRoles" onclick="javascript:toggleOwnerRoles(this);" /><asp:Label
				ID="lblShowOwnerRoles" runat="server" /><label for="chkShowOwnerRoles"><asp:Literal
					ID="l1" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_Show_Owner_Roles_Lbl %>" /></label></p>
		<ComponentArt:Grid ID="gdRoles" runat="server" Width="70%" Height="400px" PageSize="100"
			AutoCallBackOnDelete="true" OnDeleteCommand="gdRoles_DeleteCommand" CssClass="gd0Grid"
			ShowHeader="True" ShowSearchBox="True" SearchTextCssClass="gd0GridHeaderText" SearchOnKeyPress="True"
			SearchText="<%$ Resources:GalleryServerPro, Site_Grid_Search_Label %>" HeaderCssClass="gd0GridHeader"
			FooterCssClass="gd0GridFooter" GroupingNotificationText="" EnableViewState="false">
			<ClientEvents>
				<Load EventHandler="gdRoles_onLoad" />
				<CallbackError EventHandler="gdRoles_CallbackError" />
			</ClientEvents>
			<Levels>
				<ComponentArt:GridLevel DataKeyField="RoleName" AllowGrouping="False" AllowReordering="false"
					RowCssClass="gd0Row" DataCellCssClass="gd0DataCell" HeadingCellCssClass="gd0HeadingCell"
					HeadingCellHoverCssClass="gd0HeadingCellHover" HeadingCellActiveCssClass="gd0HeadingCellActive"
					HeadingRowCssClass="gd0HeadingRow" HeadingTextCssClass="gd0HeadingCellText" SortAscendingImageUrl="asc.gif"
					SortDescendingImageUrl="desc.gif" SortImageWidth="10" SortImageHeight="19">
					<Columns>
						<ComponentArt:GridColumn DataCellClientTemplateId="roleEditColumn" EditControlType="EditCommand"
							Width="100" AllowSorting="false" />
						<ComponentArt:GridColumn DataCellClientTemplateId="roleNameColumn" DataField="RoleName"
							AllowHtmlContent="True" HeadingText="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_Grid_RoleName_Header %>" />
					</Columns>
				</ComponentArt:GridLevel>
			</Levels>
		</ComponentArt:Grid>
	</div>
	<ComponentArt:Dialog ID="dgEditRole" runat="server" CloseTransition="Fade" ShowTransition="Fade"
		AnimationSlide="Linear" AnimationType="Outline" AnimationPath="Direct" AnimationDuration="400"
		TransitionDuration="400" Icon="pencil.gif" Alignment="MiddleCentre" AllowResize="True"
		Height="500" Width="640" ContentCssClass="dg0ContentCss" HeaderCssClass="dg0HeaderCss"
		CssClass="gsp_dg0DialogCss gsp_ns" FooterCssClass="dg0FooterCss">
		<HeaderTemplate>
			<div onmousedown="dgEditRole.StartDrag(event);">
				<img id="dg0DialogCloseImage" onclick="dgEditRole.Close('cancelled');" src="<%= Utils.GalleryRoot %>/images/componentart/dialog/close.gif"
					alt="<asp:Literal ID='litPageHeader' runat='server' Text='<%$ Resources:GalleryServerPro, Site_Wizard_Close_Button_Text %>' />" /><img
						id="dg0DialogIconImage" src="<%= Utils.GalleryRoot %>/images/componentart/dialog/pencil.gif"
						alt="" />
				<span id="dialogHeader"></span>
			</div>
		</HeaderTemplate>
		<ContentTemplate>
			<asp:UpdatePanel ID="upEditRole" runat="server">
				<Triggers>
					<asp:AsyncPostBackTrigger ControlID="lbAddRole" EventName="Click" />
				</Triggers>
				<ContentTemplate>
					<asp:Panel ID="pnlDialogContent" runat="server" CssClass="editRoleDialogContent">
						<asp:PlaceHolder ID="phMessage" runat="server" />
						<p class="gsp_fll gsp_bold gsp_nopadding gsp_nomargin">
							<asp:Literal ID="litRoleName" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_Role_Name %>"></asp:Literal>:&nbsp;<asp:Label
								ID="lblRoleName" runat="server" CssClass="roleNameReadOnly"></asp:Label><asp:TextBox
									ID="txtRoleName" runat="server" Style="width: 75%">
								</asp:TextBox></p>
						<table>
							<tr>
								<td class="gsp_editRoleCol1">
									<p class="gsp_bold" style="padding-right: 3em;">
										<asp:Literal ID="litRolePermissionsHeader" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_Permission_Header %>"></asp:Literal></p>
									<p>
										<asp:CheckBox ID="chkAdministerSite" runat="server" Enabled="false" CssClass="gsp_disabledtext"
											Text="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_Admin_Text %>" onclick="javascript:chkAdministerSite_CheckChanged(this);" />
										<asp:Label ID="lblAdministerSite" runat="server" /></p>
									<div class="gsp_addleftpadding6">
										<p>
											<asp:CheckBox ID="chkAdministerGallery" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_Admin_Gallery_Text %>"
												onclick="return chkAdministerGallery_CheckChanged(this);" />
											<asp:Label ID="lblAdministerGallery" runat="server" /></p>
										<div class="gsp_addleftpadding6">
											<p>
												<asp:CheckBox ID="chkViewObject" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_View_Object_Text %>"
													onclick="return !IsAdminChecked();" />
												<asp:Label ID="lblViewObject" runat="server" /></p>
											<p>
												<asp:CheckBox ID="chkViewHiResImage" runat="server" onclick="return !IsAdminChecked();"
													Text="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_View_HiRes_Text %>" />
												<asp:Label ID="lblViewHiResImage" runat="server" /></p>
											<p>
												<asp:CheckBox ID="chkAddAlbum" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_Add_Album_Text %>"
													onclick="return !IsAdminChecked();" />
												<asp:Label ID="lblAddAlbum" runat="server" /></p>
											<p>
												<asp:CheckBox ID="chkAddMediaObject" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_Add_Media_Object_Text %>"
													onclick="return !IsAdminChecked();" />
												<asp:Label ID="lblAddMediaObject" runat="server" /></p>
											<p>
												<asp:CheckBox ID="chkEditAlbum" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_Edit_Album_Text %>"
													onclick="return !IsAdminChecked();" />
												<asp:Label ID="lblEditAlbum" runat="server" /></p>
											<p>
												<asp:CheckBox ID="chkEditMediaObject" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_Edit_Media_Object_Text %>"
													onclick="return !IsAdminChecked();" />
												<asp:Label ID="lblEditMediaObject" runat="server" /></p>
											<p>
												<asp:CheckBox ID="chkDeleteChildAlbum" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_Delete_Album_Text %>"
													onclick="return !IsAdminChecked();" />
												<asp:Label ID="lblDeleteChildAlbum" runat="server" /></p>
											<p>
												<asp:CheckBox ID="chkDeleteMediaObject" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_Delete_Media_Object_Text %>"
													onclick="return !IsAdminChecked();" />
												<asp:Label ID="lblDeleteMediaObject" runat="server" /></p>
											<p>
												<asp:CheckBox ID="chkSynchronize" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_Synch_Text %>"
													onclick="return !IsAdminChecked();" />
												<asp:Label ID="lblSynchronize" runat="server" /></p>
										</div>
									</div>
									<p>
										<asp:CheckBox ID="chkHideWatermark" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_Hide_Watermark_Text %>" />
										<asp:Label ID="lblHideWatermark" runat="server" /></p>
								</td>
								<td class="gsp_editRoleCol2">
									<p class="gsp_bold">
										<asp:Literal ID="litAlbumHeader" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_Album_Header %>"></asp:Literal></p>
									<uc1:albumtreeview ID="tvUC" runat="server" AllowMultiCheck="true" Width="100%" Height="358" />
								</td>
							</tr>
						</table>
					</asp:Panel>
				</ContentTemplate>
			</asp:UpdatePanel>
			<asp:UpdateProgress ID="uProg" runat="server">
				<ProgressTemplate>
					<p class="gsp_h3 gsp_textcenter" style="margin-top: 200px;">
						<asp:Literal ID="litLoading" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_Processing_Text %>" /></p>
					<p class="gsp_textcenter">
						<img src="<%= Utils.GetUrl("images/componentart/callback/spinner.gif") %>" style="width: 16px;
							height: 16px;" alt="" /></p>
				</ProgressTemplate>
			</asp:UpdateProgress>
		</ContentTemplate>
		<FooterTemplate>
			<div class="gsp_rightBottom">
				<asp:Button ID="btnSave" runat="server" Text="<%$ Resources:GalleryServerPro, Default_Task_Ok_Button_Text %>"
					ToolTip="<%$ Resources:GalleryServerPro, Default_Task_Ok_Button_Text %>" OnClick="btnSave_Click"
					OnClientClick="tv.SaveData();" /> <!-- tv.SaveData() is hack to force TreeView to retain state during postbacks in non-IE browsers -->
				<input id="Button2" runat="server" onclick="dgEditRole.Close('cancelled');" title="<%$ Resources:GalleryServerPro, Site_Dialog_Cancel_Button_Tooltip %>"
					type="button" value="<%$ Resources:GalleryServerPro, Default_Task_Close_Button_Text %>" />
			</div>
		</FooterTemplate>
	</ComponentArt:Dialog>
	<asp:HiddenField ID="hdnRoleName" runat="server" />
	<tis:PopupInfo ID="PopupInfo" runat="server" DialogControlId="dgPopup" DefaultDialogTitleCss="dg5ContentTitleCss"
		DefaultDialogBodyCss="dg5ContentBodyCss">
		<PopupInfoItems>
			<tis:PopupInfoItem ID="poi1" runat="server" ControlId="lblAdminPageHeader" DialogTitle="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_Overview_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_Overview_Bdy %>" />
			<tis:PopupInfoItem ID="poi2" runat="server" ControlId="lblAdministerSite" DialogTitle="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_AdminSite_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_AdminSite_Bdy %>" />
			<tis:PopupInfoItem ID="poi3" runat="server" ControlId="lblAdministerGallery" DialogTitle="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_AdminGallery_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_AdminGallery_Bdy %>" />
			<tis:PopupInfoItem ID="PopupInfoItem1" runat="server" ControlId="lblViewObject" DialogTitle="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_ViewObject_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_ViewObject_Bdy %>" />
			<tis:PopupInfoItem ID="PopupInfoItem2" runat="server" ControlId="lblViewHiResImage"
				DialogTitle="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_ViewHiResImage_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_ViewHiResImage_Bdy %>" />
			<tis:PopupInfoItem ID="PopupInfoItem3" runat="server" ControlId="lblAddAlbum" DialogTitle="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_AddAlbum_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_AddAlbum_Bdy %>" />
			<tis:PopupInfoItem ID="PopupInfoItem4" runat="server" ControlId="lblAddMediaObject"
				DialogTitle="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_AddMediaObject_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_AddMediaObject_Bdy %>" />
			<tis:PopupInfoItem ID="PopupInfoItem5" runat="server" ControlId="lblEditAlbum" DialogTitle="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_EditAlbum_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_EditAlbum_Bdy %>" />
			<tis:PopupInfoItem ID="PopupInfoItem6" runat="server" ControlId="lblEditMediaObject"
				DialogTitle="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_EditMediaObject_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_EditMediaObject_Bdy %>" />
			<tis:PopupInfoItem ID="PopupInfoItem7" runat="server" ControlId="lblDeleteChildAlbum"
				DialogTitle="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_DeleteChildAlbum_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_DeleteChildAlbum_Bdy %>" />
			<tis:PopupInfoItem ID="PopupInfoItem8" runat="server" ControlId="lblDeleteMediaObject"
				DialogTitle="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_DeleteMediaObject_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_DeleteMediaObject_Bdy %>" />
			<tis:PopupInfoItem ID="PopupInfoItem9" runat="server" ControlId="lblSynchronize"
				DialogTitle="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_Synchronize_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_Synchronize_Bdy %>" />
			<tis:PopupInfoItem ID="PopupInfoItem10" runat="server" ControlId="lblHideWatermark"
				DialogTitle="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_HideWatermark_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_HideWatermark_Bdy %>" />
			<tis:PopupInfoItem ID="PopupInfoItem11" runat="server" ControlId="lblShowOwnerRoles"
				DialogTitle="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_ShowOwnerRoles_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Admin_Manage_Roles_ShowOwnerRoles_Bdy %>" />
		</PopupInfoItems>
	</tis:PopupInfo>
	<uc1:popup ID="ucPopupContainer" runat="server" />
</div>
