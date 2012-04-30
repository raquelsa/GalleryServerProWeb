<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="backuprestore.ascx.cs"
	Inherits="GalleryServerPro.Web.Pages.Admin.backuprestore" %>
<%@ Register Namespace="ComponentArt.Web.UI" Assembly="ComponentArt.Web.UI" TagPrefix="ComponentArt" %>
<%@ Register Src="../../Controls/popupinfo.ascx" TagName="popup" TagPrefix="uc1" %>
<%@ Register Src="../../Controls/usermessage.ascx" TagName="usermessage" TagPrefix="uc1" %>
<%@ Register Assembly="GalleryServerPro.WebControls" Namespace="GalleryServerPro.WebControls"
	TagPrefix="tis" %>
<%@ Import Namespace="GalleryServerPro.Web" %>
<div class="gsp_indentedContent">
	<asp:PlaceHolder ID="phAdminHeader" runat="server" />
	<div runat="server">
		<script type="text/javascript">

			//	File size formatting function
			//	n = size in bytes
			//	fmt = output format ("b","kb","mb","gb") [optional; if omitted it will return the best match]
			function format_file_size(n, fmt)
			{
				if (!fmt)
				{		//	no formatting specified; automatically select the best format
					if (n < 1000) fmt = "b";
					else if (n < 1000000) fmt = "kb";
					else if (n < 1000000000) fmt = "mb";
					else fmt = "gb";
				}

				switch (fmt.toLowerCase())
				{
					case "kb": return String((n * 0.001).toFixed(2)) + " <asp:Literal ID='l11' runat='server' Text='<%$ Resources:GalleryServerPro, Site_Kilobyte_Abbreviation %>' />"; break;
					case "mb": return String((n * 0.000001).toFixed(2)) + " <asp:Literal ID='l12' runat='server' Text='<%$ Resources:GalleryServerPro, Site_Megabyte_Abbreviation %>' />"; break;
					case "gb": return String((n * 0.000000001).toFixed(2)) + " <asp:Literal ID='l13' runat='server' Text='<%$ Resources:GalleryServerPro, Site_Gigabyte_Abbreviation %>' />"; break;
					default: return String(n.toFixed(2)) + " <asp:Literal ID='l14' runat='server' Text='<%$ Resources:GalleryServerPro, Site_Byte_Abbreviation %>' />";
				}
			}

			function get_percentage(n) { return String(Math.round(n * 100)); }

			//	Time formatting function
			//	t = time in seconds
			//	returns a string formatted thusly: hh:mm:ss [hours is only present if needed]
			function format_time(t)
			{
				var s = Math.floor(t);
				var m = Math.floor(s / 60);
				var h = Math.floor(m / 60);

				s = pad_time(s % 60);
				m = pad_time(m % 60) + ":";
				h = (h == 0) ? "" : pad_time(h % 60) + ":";

				return (h + m + s);
			}

			function pad_time(t) { return String(((t > 9) ? "" : "0") + t); }

			function init_upload(ctrl)
			{
				if (ctrl.GetFiles().length > 0 && !ctrl.Uploading)
				{
					ctrl.Upload();
					UploadDialog.Show();
				}
			}

			//	Background image preloader
			(new Image()).src = "<%= Utils.GalleryRoot %>/images/componentart/upload/vertical.png";
			(new Image()).src = "<%= Utils.GalleryRoot %>/images/spinner.gif";
		</script>
	</div>
	<div class="gsp_addpadding1">
		<p class="gsp_msgdark">
			<asp:Label ID="lbl1" runat="server" CssClass="gsp_bold" Text="<%$ Resources:GalleryServerPro, Admin_Settings_Apply_To_Label %>"
				EnableViewState="false" />&nbsp;<asp:Literal ID="Literal1" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_All_Galleries_Label %>" /></p>
		<tis:wwErrorDisplay ID="wwMessage" runat="server" CellPadding="2" UseFixedHeightWhenHiding="False"
			Center="False" Width="500px">
		</tis:wwErrorDisplay>
		<ComponentArt:TabStrip ID="tsBackupRestore" MultiPageId="mpBackupRestore" TopGroupCssClass="ts0TopGroup"
			DefaultSelectedItemLookId="SelectedTab" DefaultItemLookId="DefaultTab" DefaultChildSelectedItemLookId="SelectedTab"
			TopGroupShowSeparators="true" DefaultGroupSeparatorWidth="22" DefaultGroupSeparatorHeight="22"
			DefaultGroupFirstSeparatorWidth="15" DefaultGroupFirstSeparatorHeight="22" DefaultGroupLastSeparatorWidth="22"
			DefaultGroupLastSeparatorHeight="22" Width="99%" runat="server">
			<ItemLooks>
				<ComponentArt:ItemLook LookId="DefaultTab" CssClass="ts0DefaultTab" HoverCssClass="ts0DefaultTabHover"
					LabelPaddingLeft="10" LabelPaddingRight="17" LabelPaddingTop="2" LabelPaddingBottom="6" />
				<ComponentArt:ItemLook LookId="SelectedTab" CssClass="ts0SelectedTab" LabelPaddingLeft="10"
					LabelPaddingRight="17" LabelPaddingTop="2" LabelPaddingBottom="6" />
			</ItemLooks>
			<Tabs>
				<ComponentArt:TabStripTab ID="tabGeneral" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Backup_Restore_Backup_Tab_Title %>" />
				<ComponentArt:TabStripTab ID="tabRoles" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Backup_Restore_Restore_Tab_Title %>" />
			</Tabs>
		</ComponentArt:TabStrip>
		<ComponentArt:MultiPage ID="mpBackupRestore" CssClass="mp2MultiPage" runat="server">
			<ComponentArt:PageView ID="PageView1" CssClass="mp2MultiPageContent" runat="server">
				<p class="gsp_h3">
					<asp:Literal ID="l1" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Backup_Restore_Backup_Tab_Hdr %>" /></p>
				<p class="gsp_addtopmargin5">
					<asp:Label ID="lblBackupDtl" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Backup_Restore_Backup_Tab_Dtl %>" /></p>
				<uc1:usermessage ID="ucUserMessageBackup" runat="server" CssClass="um0ContainerCss"
					IconStyle="Information" MessageTitle="" DetailCssClass="um1DetailCss" MessageDetail="<%$ Resources:GalleryServerPro, Admin_Backup_Restore_Backup_Tab_Note %>" />
				<p class="gsp_addtopmargin5">
					<asp:CheckBox ID="chkExportMembership" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Backup_Restore_Backup_Tab_Export_Users_Checkbox_Text %>"
						Checked="true" /></p>
				<p>
					<asp:CheckBox ID="chkExportGalleryData" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Backup_Restore_Backup_Tab_Export_Gallery_Data_Checkbox_Text %>"
						Checked="true" /></p>
				<p>
					<asp:Button ID="btnExportData" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Backup_Restore_Backup_Tab_Export_Btn_Text %>"
						OnClick="btnExportData_Click" />
				</p>
			</ComponentArt:PageView>
			<ComponentArt:PageView ID="PageView2" CssClass="mp2MultiPageContent" runat="server"
				Height="300">
				<p class="gsp_h3">
					<asp:Literal ID="l3" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Backup_Restore_Restore_Tab_Hdr %>" />
				</p>
				<p>
					<asp:Literal ID="l4" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Backup_Restore_Restore_Tab_Dtl %>" /></p>
				<uc1:usermessage ID="ucUserMessageRestore" runat="server" CssClass="um0ContainerCss"
					IconStyle="Information" MessageTitle="" DetailCssClass="um1DetailCss" MessageDetail="<%$ Resources:GalleryServerPro, Admin_Backup_Restore_Restore_Tab_Note %>" />
				<asp:PlaceHolder ID="phUpload" runat="server" />
				<asp:Panel ID="pnlRestoreFileInfo" runat="server" Visible="true">
					<table id="restoreFileContainer" class="gsp_rounded10" cellpadding="0" cellspacing="0">
						<tr class="gsp_tableSummaryRow gsp_roundedtop6">
							<td colspan="3">
								<p>
									<asp:LinkButton ID="lbRemoveRestoreFile" runat="server" Text="Remove" CssClass="gsp_fs"
										Visible="false" OnClick="lbRemoveRestoreFile_Click" />
									<asp:Literal ID="l5" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Backup_Restore_Restore_Tab_Filename_Hdr %>" />
									<asp:Label ID="lblRestoreFilename" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Backup_Restore_Restore_Tab_Empty_Filename %>"
										CssClass="gsp_msgwarning" /></p>
								<p>
									<asp:Literal ID="l6" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Backup_Restore_Restore_Tab_Schema_Hdr %>" />&nbsp;<asp:Label
										ID="lblSchemaVersion" runat="server" CssClass="gsp_msgdark" /></p>
								<p>
									<asp:Image ID="imgValidationResult" runat="server" Width="16" Height="16" Style="vertical-align: middle;"
										Visible="false" />&nbsp;<asp:Label ID="lblValidationResult" runat="server" /></p>
							</td>
						</tr>
						<tr class="gsp_tableHeaderRow">
							<td class="gsp_topBorder gsp_bottomBorder">
								&nbsp;
							</td>
							<td class="gsp_topBorder gsp_bottomBorder">
								<asp:Literal ID="l7" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Backup_Restore_Restore_Tab_Table_Column_Hdr %>" />
							</td>
							<td class="gsp_numRecords gsp_topBorder gsp_bottomBorder">
								<asp:Literal ID="l8" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Backup_Restore_Restore_Tab_NumRecords_Column_Hdr %>" />
							</td>
						</tr>
						<tr>
							<td rowspan="6" style="width: 150px;" class="gsp_bottomBorder">
								<asp:CheckBox ID="chkImportMembership" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Backup_Restore_Restore_Tab_Import_Users_Checkbox_Text %>"
									Checked="True" />
							</td>
							<td style="width: 250px;">
								aspnet_Applications
							</td>
							<td style="width: 100px;" class="gsp_numRecords">
								<asp:Label ID="lblNumApps" runat="server"></asp:Label>
							</td>
						</tr>
						<tr>
							<td>
								aspnet_Profile
							</td>
							<td class="gsp_numRecords">
								<asp:Label ID="lblNumProfiles" runat="server"></asp:Label>
							</td>
						</tr>
						<tr>
							<td>
								aspnet_Roles
							</td>
							<td class="gsp_numRecords">
								<asp:Label ID="lblNumRoles" runat="server"></asp:Label>
							</td>
						</tr>
						<tr>
							<td>
								aspnet_Membership
							</td>
							<td class="gsp_numRecords">
								<asp:Label ID="lblNumMembers" runat="server"></asp:Label>
							</td>
						</tr>
						<tr>
							<td>
								aspnet_Users
							</td>
							<td class="gsp_numRecords">
								<asp:Label ID="lblNumUsers" runat="server"></asp:Label>
							</td>
						</tr>
						<tr class="gsp_bottomBorder">
							<td class="gsp_bottomBorder">
								aspnet_UsersInRoles
							</td>
							<td class="gsp_numRecords gsp_bottomBorder">
								<asp:Label ID="lblNumUsersInRoles" runat="server"></asp:Label>
							</td>
						</tr>
						<tr>
							<td rowspan="14">
								<asp:CheckBox ID="chkImportGalleryData" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Backup_Restore_Restore_Tab_Import_Gallery_Data_Checkbox_Text %>"
									Checked="True" />
							</td>
							<td>
								gs_Gallery
							</td>
							<td class="gsp_numRecords">
								<asp:Label ID="lblNumGalleries" runat="server"></asp:Label>
							</td>
						</tr>
						<tr>
							<td>
								gs_Album
							</td>
							<td class="gsp_numRecords">
								<asp:Label ID="lblNumAlbums" runat="server"></asp:Label>
							</td>
						</tr>
						<tr>
							<td>
								gs_MediaObject
							</td>
							<td class="gsp_numRecords">
								<asp:Label ID="lblNumMediaObjects" runat="server"></asp:Label>
							</td>
						</tr>
						<tr>
							<td>
								gs_MediaObjectMetadata
							</td>
							<td class="gsp_numRecords">
								<asp:Label ID="lblNumMetadata" runat="server"></asp:Label>
							</td>
						</tr>
						<tr>
							<td>
								gs_Role_Album
							</td>
							<td class="gsp_numRecords">
								<asp:Label ID="lblNumRoleAlbums" runat="server"></asp:Label>
							</td>
						</tr>
						<tr>
							<td>
								gs_Role
							</td>
							<td class="gsp_numRecords">
								<asp:Label ID="lblNumGalleryRoles" runat="server"></asp:Label>
							</td>
						</tr>
						<tr>
							<td>
								gs_AppSetting
							</td>
							<td class="gsp_numRecords">
								<asp:Label ID="lblNumAppSettings" runat="server"></asp:Label>
							</td>
						</tr>
						<tr>
							<td>
								gs_GalleryControlSetting
							</td>
							<td class="gsp_numRecords">
								<asp:Label ID="lblNumGalleryControlSettings" runat="server"></asp:Label>
							</td>
						</tr>
						<tr>
							<td>
								gs_GallerySetting
							</td>
							<td class="gsp_numRecords">
								<asp:Label ID="lblNumGallerySettings" runat="server"></asp:Label>
							</td>
						</tr>
						<tr>
							<td>
								gs_BrowserTemplate
							</td>
							<td class="gsp_numRecords">
								<asp:Label ID="lblNumBrowserTemplates" runat="server"></asp:Label>
							</td>
						</tr>
						<tr>
							<td>
								gs_MimeType
							</td>
							<td class="gsp_numRecords">
								<asp:Label ID="lblNumMimeTypes" runat="server"></asp:Label>
							</td>
						</tr>
						<tr>
							<td>
								gs_MimeTypeGallery
							</td>
							<td class="gsp_numRecords">
								<asp:Label ID="lblNumMimeTypeGalleries" runat="server"></asp:Label>
							</td>
						</tr>
						<tr>
							<td>
								gs_AppError
							</td>
							<td class="gsp_numRecords">
								<asp:Label ID="lblNumAppErrors" runat="server"></asp:Label>
							</td>
						</tr>
						<tr>
							<td>
								gs_UserGalleryProfile
							</td>
							<td class="gsp_numRecords">
								<asp:Label ID="lblNumUserGalleryProfiles" runat="server"></asp:Label>
							</td>
						</tr>
					</table>
					<div>
						<div style="height: 80px; float: left; margin-top: 10px; margin-right: 5px;">
							<asp:Button ID="btnRestore" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Backup_Restore_Restore_Tab_Restore_Btn_Text %>"
								Enabled="False" OnClick="btnRestore_Click" /></div>
						<p style="width: 400px;">
							<span class="gsp_fs gsp_msgwarning"><span class="gsp_bold">
								<asp:Literal ID="l9" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Backup_Restore_Restore_Tab_Restore_Warning_Hdr %>" /></span>&nbsp;<asp:Literal
									ID="l10" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Backup_Restore_Restore_Tab_Restore_Warning_Dtl %>" /></span></p>
					</div>
				</asp:Panel>
			</ComponentArt:PageView>
		</ComponentArt:MultiPage>
		<ComponentArt:Dialog ID="UploadDialog" runat="server" Visible="false" AllowDrag="true"
			AllowResize="false" Modal="true" Alignment="MiddleCentre" Width="658" Height="347"
			ContentCssClass="dlg-up" ContentClientTemplateId="UploadContent" CssClass="gsp_ns" />
	</div>
	<tis:PopupInfo ID="PopupInfo" runat="server" DialogControlId="dgPopup" DefaultDialogTitleCss="dg5ContentTitleCss"
		DefaultDialogBodyCss="dg5ContentBodyCss">
		<PopupInfoItems>
			<tis:PopupInfoItem ID="PopupInfoItem1" runat="server" ControlId="lblBackupDtl" DialogTitle="<%$ Resources:GalleryServerPro, Admin_BackupRestore_Backup_Overview_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Admin_BackupRestore_Backup_Overview_Bdy %>" />
			<tis:PopupInfoItem ID="PopupInfoItem2" runat="server" ControlId="chkExportMembership"
				DialogTitle="<%$ Resources:GalleryServerPro, Admin_BackupRestore_ExportUserAccounts_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Admin_BackupRestore_ExportUserAccounts_Bdy %>" />
			<tis:PopupInfoItem ID="PopupInfoItem3" runat="server" ControlId="chkExportGalleryData"
				DialogTitle="<%$ Resources:GalleryServerPro, Admin_BackupRestore_ExportGalleryData_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Admin_BackupRestore_ExportGalleryData_Bdy %>" />
		</PopupInfoItems>
	</tis:PopupInfo>
	<uc1:popup ID="ucPopupContainer" runat="server" />
	<asp:PlaceHolder ID="phAdminFooter" runat="server" />
</div>
