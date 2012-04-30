<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="sitesettings.ascx.cs"
	Inherits="GalleryServerPro.Web.Pages.Admin.sitesettings" %>
<%@ Register Src="../../Controls/popupinfo.ascx" TagName="popup" TagPrefix="uc1" %>
<%@ Register Assembly="GalleryServerPro.WebControls" Namespace="GalleryServerPro.WebControls"
	TagPrefix="tis" %>
<%@ Register Namespace="ComponentArt.Web.UI" Assembly="ComponentArt.Web.UI" TagPrefix="ComponentArt" %>
<div id="d1" runat="server">
	<script type="text/javascript">
		Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(errorlogPageLoad);

		function errorlogPageLoad(sender, args) {
			configControls();
		}

		function configControls() {
			var txtMaxErrItems = $get('<%= txtMaxErrorItems.ClientID %>');
			var chkAutoTrimLog = $get('<%= chkAutoTrimLog.ClientID %>');

			txtMaxErrItems.disabled = (!chkAutoTrimLog.checked || chkAutoTrimLog.disabled);
		}

		function chkAutoTrimLog_Click(chk) {
			configControls();
		}

	</script>
</div>
<div id="css" runat="server" visible="false">
	<link href="../../styles/gallery.css" rel="stylesheet" type="text/css" />
</div>
<div class="gsp_indentedContent">
	<asp:PlaceHolder ID="phAdminHeader" runat="server" />
	<div class="gsp_addpadding1">
		<p class="gsp_msgdark">
			<asp:Label ID="lbl1" runat="server" CssClass="gsp_bold" Text="<%$ Resources:GalleryServerPro, Admin_Settings_Apply_To_Label %>"
				EnableViewState="false" />&nbsp;<asp:Literal ID="l5" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_All_Galleries_Label %>" /></p>
		<tis:wwErrorDisplay ID="wwMessage" runat="server" UserMessage="<%$ Resources:GalleryServerPro, Validation_Summary_Text %>"
			CellPadding="2" UseFixedHeightWhenHiding="False" Center="False" Width="500px">
		</tis:wwErrorDisplay>
		<div id="verContainer" class="gsp_rounded10">
			<p class="verHdr">
				<asp:Literal ID="l1" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Site_Settings_GSP_Hdr %>" />
				<asp:Label ID="lblVersion" runat="server" CssClass="gsp_msgfriendly" /></p>
			<p class="gsp_nopadding">
				<asp:Literal ID="l2" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Site_Settings_GSP_Home_Page %>" />
			</p>
		</div>
		<p class="admin_h3">
			<asp:Literal ID="l3" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Site_Settings_ProductKey_Hdr %>" /></p>
		<div class="gsp_addleftpadding6">
			<p class="gsp_addtopmargin5">
				<asp:Label ID="lblProductKey" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Site_Settings_ProductKey_Label %>" /></p>
			<p>
				<asp:TextBox ID="txtProductKey" runat="server" CssClass="gsp_prodkeytxt" />&nbsp;<asp:Button
					ID="btnEnterProductKey" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Site_Settings_ProductKey_Btn_Lbl %>"
					OnClick="btnEnterProductKey_Click" />
			</p>
			<p class="gsp_nopadding">
				<asp:Image ID="imgProductKeyValidation" runat="server" Width="16" Height="16" Visible="false" />&nbsp;<asp:Label
					ID="lblProductKeyValidationMsg" runat="server" />
			</p>
		</div>
		<p class="admin_h3">
			<asp:Literal ID="l4" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Site_Settings_AppSettings_Hdr %>" />
		</p>
		<div class="gsp_addtopmargin5 gsp_addleftmargin5">
			<p>
				<asp:CheckBox ID="chkEnableCache" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Site_Settings_EnableCache_Label %>" />
			</p>
			<p>
				<asp:CheckBox ID="chkAllowGalleryAdminToManageUsersAndRoles" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Site_Settings_AllowGalleryAdminToManageUsersAndRoles_Label %>" />
			</p>
			<p>
				<asp:CheckBox ID="chkAllowGalleryAdminToViewAllUsersAndRoles" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Site_Settings_AllowGalleryAdminToViewAllUsersAndRoles_Label %>" />
			</p>
			<p>
				<asp:CheckBox ID="chkAutoTrimLog" runat="server" onclick="chkAutoTrimLog_Click(this)"
					Text="<%$ Resources:GalleryServerPro, Admin_Site_Settings_MaxNumberErrorItems_Lbl %>" /></p>
			<p class="gsp_addleftmargin10">
				<asp:Literal ID="l6" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Site_Settings_MaxNumberErrorItems_Label %>" />:
				<asp:TextBox ID="txtMaxErrorItems" runat="server" />&nbsp;<asp:RangeValidator ID="rvMaxErrorItems"
					runat="server" Display="Dynamic" ControlToValidate="txtMaxErrorItems" Type="Integer"
					MinimumValue="1" MaximumValue="2147483647" Text="<%$ Resources:GalleryServerPro, Validation_Positive_Int_Text %>" /></p>
			<table class="gsp_standardTable gsp_addtopmargin5">
				<tr>
					<td class="gsp_col1">
						<asp:Label ID="lblJQuery" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Site_Settings_JQuery_Label %>" />
					</td>
					<td>
						<asp:TextBox ID="txtJQueryScriptPath" runat="server" CssClass="gsp_textbox" />
					</td>
				</tr>
				<tr>
					<td class="gsp_col1">
						<asp:Label ID="lblJQueryUI" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Site_Settings_JQueryUI_Label %>" />
					</td>
					<td>
						<asp:TextBox ID="txtJQueryUiScriptPath" runat="server" CssClass="gsp_textbox" />
					</td>
				</tr>
			</table>
		</div>
		<p class="admin_h3">
			<asp:Literal ID="l10" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Site_Settings_AppInfo_Hdr %>" />
		</p>
		<div class="gsp_addtopmargin5 gsp_addleftmargin5">
			<table class="gsp_standardTable">
				<tr>
					<td class="gsp_col1">
						<asp:Label ID="lblTrustLevelLbl" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Site_Settings_TrustLevel_Label %>" />
					</td>
					<td>
						<asp:Label ID="lblTrustLevel" runat="server" CssClass="gsp_msgfriendly" />
					</td>
				</tr>
				<tr>
					<td class="gsp_col1">
						<asp:Label ID="lblNetVersionLbl" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Site_Settings_NetVersion_Label %>" />
					</td>
					<td>
						<asp:Label ID="lblNetVersion" runat="server" CssClass="gsp_msgfriendly" />
					</td>
				</tr>
				<tr>
					<td class="gsp_col1">
						<asp:Label ID="lblIisAppPoolLbl" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Site_Settings_IisAppPoolIdentity_Label %>" />
					</td>
					<td>
						<asp:Label ID="lblIisAppPool" runat="server" CssClass="gsp_msgfriendly" />
					</td>
				</tr>
				<tr>
					<td class="gsp_col1">
						<asp:Label ID="lblFFmpegLbl" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Site_Settings_FFmpeg_Label %>" />
					</td>
					<td>
						<span class="gsp_msgfriendly" runat="server">
							<%= String.IsNullOrEmpty(AppSettings.FFmpegPath) ? Resources.GalleryServerPro.Site_No : Resources.GalleryServerPro.Site_Yes %></span>
					</td>
				</tr>
				<tr>
					<td class="gsp_col1">
						<asp:Label ID="lblImageMagickLbl" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Site_Settings_ImageMagick_Label %>" />
					</td>
					<td>
						<span class="gsp_msgfriendly" runat="server">
							<%= String.IsNullOrEmpty(AppSettings.ImageMagickConvertPath) ? Resources.GalleryServerPro.Site_No : Resources.GalleryServerPro.Site_Yes %></span>
					</td>
				</tr>
			</table>
		</div>
		<p class="admin_h3">
			<asp:Label ID="lblProviderLabel" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Site_Settings_DataProvider_Hdr %>" /></p>
		<div class="gsp_addleftpadding6">
			<table class="gsp_addleftpadding6 gsp_standardTable">
				<tr>
					<td colspan="2">
						<p class="gsp_bold">
							<asp:Label ID="lblGalleryDataLabel" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Site_Settings_DataProvider_GalleryData_Hdr %>" /></p>
					</td>
				</tr>
				<tr>
					<td class="gsp_col1">
						<asp:Label ID="lblGalleryDataProviderLabel" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Site_Settings_DataProvider_Description_Label %>" />
					</td>
					<td>
						<asp:Label ID="lblGalleryDataProvider" runat="server" CssClass="gsp_msgfriendly" />
						-
						<asp:Label ID="lblGalleryDataProviderDesc" runat="server" CssClass="gsp_msgfriendly" />
					</td>
				</tr>
				<tr>
					<td class="gsp_col1">
						<asp:Label ID="lblGalleryDataAppNameLabel" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Site_Settings_DataProvider_AppName_Label %>" />
					</td>
					<td>
						<asp:Label ID="lblGalleryDataAppName" runat="server" CssClass="gsp_msgfriendly" />
					</td>
				</tr>
				<tr id="trCompact" runat="server" visible="false">
					<td class="gsp_col1 gsp_aligntop">
						Data file:
					</td>
					<td class="gsp_msgfriendly">
						<asp:Label ID="lblDbFilename" runat="server" />
						<asp:Label ID="lblDbFileSize" runat="server" /><p>
							<asp:LinkButton ID="lbCompactDb" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Site_Settings_DataProvider_CompactDb_Btn_Lbl %>"
								ToolTip="<%$ Resources:GalleryServerPro, Admin_Site_Settings_DataProvider_CompactDb_Btn_Lbl %>"
								OnClick="lbCompactDb_Click" /></p>
					</td>
				</tr>
				<tr>
					<td colspan="2">
						<p class="gsp_bold gsp_addtopmargin5">
							<asp:Label ID="lblMembershipLabel" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Site_Settings_DataProvider_Membership_Hdr %>" /></p>
					</td>
				</tr>
				<tr>
					<td class="gsp_col1">
						<asp:Label ID="lblMembershipDataProviderLabel" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Site_Settings_DataProvider_Description_Label %>" />
					</td>
					<td>
						<asp:Label ID="lblMembershipDataProvider" runat="server" CssClass="gsp_msgfriendly" />
						-
						<asp:Label ID="lblMembershipDataProviderDesc" runat="server" CssClass="gsp_msgfriendly" />
					</td>
				</tr>
				<tr>
					<td class="gsp_col1">
						<asp:Label ID="lblMembershipAppNameLabel" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Site_Settings_DataProvider_AppName_Label %>" />
					</td>
					<td>
						<asp:Label ID="lblMembershipAppName" runat="server" CssClass="gsp_msgfriendly" />
					</td>
				</tr>
				<tr>
					<td colspan="2">
						<p class="gsp_bold gsp_addtopmargin5">
							<asp:Label ID="lblRoleLabel" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Site_Settings_DataProvider_Roles_Hdr %>" /></p>
					</td>
				</tr>
				<tr>
					<td class="gsp_col1">
						<asp:Label ID="lblRoleDataProviderLabel" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Site_Settings_DataProvider_Description_Label %>" />
					</td>
					<td>
						<asp:Label ID="lblRoleDataProvider" runat="server" CssClass="gsp_msgfriendly" />
						-
						<asp:Label ID="lblRoleDataProviderDesc" runat="server" CssClass="gsp_msgfriendly" />
					</td>
				</tr>
				<tr>
					<td class="gsp_col1">
						<asp:Label ID="lblRoleAppNameLabel" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Site_Settings_DataProvider_AppName_Label %>" />
					</td>
					<td>
						<asp:Label ID="lblRoleAppName" runat="server" CssClass="gsp_msgfriendly" />
					</td>
				</tr>
			</table>
		</div>
	</div>
	<tis:wwDataBinder ID="wwDataBinder" runat="server" OnBeforeUnbindControl="wwDataBinder_BeforeUnbindControl"
		OnAfterBindControl="wwDataBinder_AfterBindControl" OnValidateControl="wwDataBinder_ValidateControl">
		<DataBindingItems>
			<tis:wwDataBindingItem ID="wbi1" runat="server" BindingSource="AppSettingsUpdateable"
				BindingSourceMember="EnableCache" ControlId="chkEnableCache" BindingProperty="Checked"
				UserFieldName="<%$ Resources:GalleryServerPro, Admin_Site_Settings_EnableCache_Label %>" />
			<tis:wwDataBindingItem ID="wbi2" runat="server" BindingSource="AppSettingsUpdateable"
				BindingSourceMember="AllowGalleryAdminToManageUsersAndRoles" ControlId="chkAllowGalleryAdminToManageUsersAndRoles"
				BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServerPro, Admin_Site_Settings_AllowGalleryAdminToManageUsersAndRoles_Label %>" />
			<tis:wwDataBindingItem ID="wbi3" runat="server" BindingSource="AppSettingsUpdateable"
				BindingSourceMember="AllowGalleryAdminToViewAllUsersAndRoles" ControlId="chkAllowGalleryAdminToViewAllUsersAndRoles"
				BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServerPro, Admin_Site_Settings_AllowGalleryAdminToViewAllUsersAndRoles_Label %>" />
			<tis:wwDataBindingItem ID="wbi4" runat="server" BindingSource="AppSettingsUpdateable"
				BindingSourceMember="MaxNumberErrorItems" ControlId="txtMaxErrorItems" UserFieldName="<%$ Resources:GalleryServerPro, Admin_Site_Settings_MaxNumberErrorItems_Label %>" />
			<tis:wwDataBindingItem ID="wbi5" runat="server" ControlId="chkAutoTrimLog">
			</tis:wwDataBindingItem>
			<tis:wwDataBindingItem ID="wbi6" runat="server" ControlId="txtJQueryScriptPath" BindingSource="AppSettingsUpdateable"
				BindingSourceMember="JQueryScriptPath" UserFieldName="<%$ Resources:GalleryServerPro, Admin_Site_Settings_JQuery_Label %>"
				IsRequired="False" />
			<tis:wwDataBindingItem ID="wbi7" runat="server" ControlId="txtJQueryUiScriptPath"
				BindingSource="AppSettingsUpdateable" BindingSourceMember="JQueryUiScriptPath"
				UserFieldName="<%$ Resources:GalleryServerPro, Admin_Site_Settings_JQueryUI_Label %>"
				IsRequired="False" />
			<tis:wwDataBindingItem ID="wbi8" runat="server" ControlId="lblTrustLevel" BindingSource="AppSettings"
				BindingSourceMember="AppTrustLevel" BindingMode="OneWay" />
			<tis:wwDataBindingItem ID="wbi9" runat="server" ControlId="lblNetVersion" BindingSource="AppSettings"
				BindingSourceMember="DotNetFrameworkVersion" BindingMode="OneWay" />
			<tis:wwDataBindingItem ID="wbi10" runat="server" ControlId="lblIisAppPool" BindingSource="AppSettings"
				BindingSourceMember="IisAppPoolIdentity" BindingMode="OneWay" />
			<tis:wwDataBindingItem ID="wbi13" runat="server" BindingSource="GalleryDataProvider"
				BindingSourceMember="Name" ControlId="lblGalleryDataProvider" BindingMode="OneWay" />
			<tis:wwDataBindingItem ID="wbi14" runat="server" BindingSource="GalleryDataProvider"
				BindingSourceMember="Description" ControlId="lblGalleryDataProviderDesc" BindingMode="OneWay" />
			<tis:wwDataBindingItem ID="wbi15" runat="server" BindingSource="GalleryDataProvider"
				BindingSourceMember="ApplicationName" ControlId="lblGalleryDataAppName" BindingMode="OneWay" />
			<tis:wwDataBindingItem ID="wbi16" runat="server" BindingSource="MembershipGsp" BindingSourceMember="Name"
				ControlId="lblMembershipDataProvider" BindingMode="OneWay" />
			<tis:wwDataBindingItem ID="wbi17" runat="server" BindingSource="MembershipGsp" BindingSourceMember="Description"
				ControlId="lblMembershipDataProviderDesc" BindingMode="OneWay" />
			<tis:wwDataBindingItem ID="wbi18" runat="server" BindingSource="MembershipGsp" BindingSourceMember="ApplicationName"
				ControlId="lblMembershipAppName" BindingMode="OneWay" />
			<tis:wwDataBindingItem ID="wbi19" runat="server" BindingSource="RoleGsp" BindingSourceMember="Name"
				ControlId="lblRoleDataProvider" BindingMode="OneWay" />
			<tis:wwDataBindingItem ID="wbi20" runat="server" BindingSource="RoleGsp" BindingSourceMember="Description"
				ControlId="lblRoleDataProviderDesc" BindingMode="OneWay" />
			<tis:wwDataBindingItem ID="wbi21" runat="server" BindingSource="RoleGsp" BindingSourceMember="ApplicationName"
				ControlId="lblRoleAppName" BindingMode="OneWay" />
		</DataBindingItems>
	</tis:wwDataBinder>
	<tis:PopupInfo ID="PopupInfo" runat="server" DialogControlId="dgPopup" DefaultDialogTitleCss="dg5ContentTitleCss"
		DefaultDialogBodyCss="dg5ContentBodyCss">
		<PopupInfoItems>
			<tis:PopupInfoItem ID="poi1" runat="server" ControlId="lblProductKey" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_productKey_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_productKey_Bdy %>" />
			<tis:PopupInfoItem ID="poi2" runat="server" ControlId="chkEnableCache" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_enableCache_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_enableCache_Bdy %>" />
			<tis:PopupInfoItem ID="poi3" runat="server" ControlId="chkAllowGalleryAdminToManageUsersAndRoles"
				DialogTitle="<%$ Resources:GalleryServerPro, Cfg_allowGalleryAdminToManageUsersAndRole_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_allowGalleryAdminToManageUsersAndRole_Bdy %>" />
			<tis:PopupInfoItem ID="poi5" runat="server" ControlId="chkAllowGalleryAdminToViewAllUsersAndRoles"
				DialogTitle="<%$ Resources:GalleryServerPro, Cfg_allowGalleryAdminViewAllUsersAndRoles_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_allowGalleryAdminViewAllUsersAndRoles_Bdy %>" />
			<tis:PopupInfoItem ID="poi6" runat="server" ControlId="chkAutoTrimLog" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_maxNumberErrorItems_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_maxNumberErrorItems_Bdy %>" />
			<tis:PopupInfoItem ID="poi7" runat="server" ControlId="lblJQuery" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_jQueryScriptPath_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_jQueryScriptPath_Bdy %>" />
			<tis:PopupInfoItem ID="poi8" runat="server" ControlId="lblJQueryUI" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_jQueryUIScriptPath_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_jQueryUIScriptPath_Bdy %>" />
			<tis:PopupInfoItem ID="poi9" runat="server" ControlId="lblFFmpegLbl" DialogTitle="<%$ Resources:GalleryServerPro, Admin_Site_Settings_FFmpeg_Info_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Admin_Site_Settings_FFmpeg_Info_Bdy %>" />
			<tis:PopupInfoItem ID="poi10" runat="server" ControlId="lblImageMagickLbl" DialogTitle="<%$ Resources:GalleryServerPro, Admin_Site_Settings_ImageMagick_Info_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Admin_Site_Settings_ImageMagick_Info_Bdy %>" />
			<tis:PopupInfoItem ID="poi20" runat="server" ControlId="lblGalleryDataLabel" DialogTitle="<%$ Resources:GalleryServerPro, Admin_Site_Settings_DataProvider_GalleryData_Popup_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Admin_Site_Settings_DataProvider_GalleryData_Popup_Bdy %>" />
			<tis:PopupInfoItem ID="poi21" runat="server" ControlId="lblMembershipLabel" DialogTitle="<%$ Resources:GalleryServerPro, Admin_Site_Settings_DataProvider_Membership_Popup_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Admin_Site_Settings_DataProvider_Membership_Popup_Bdy %>" />
			<tis:PopupInfoItem ID="poi22" runat="server" ControlId="lblRoleLabel" DialogTitle="<%$ Resources:GalleryServerPro, Admin_Site_Settings_DataProvider_Role_Popup_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Admin_Site_Settings_DataProvider_Role_Popup_Bdy %>" />
			<tis:PopupInfoItem ID="poi23" runat="server" ControlId="lbCompactDb" DialogTitle="<%$ Resources:GalleryServerPro, Admin_Site_Settings_DataProvider_Compact_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Admin_Site_Settings_DataProvider_Compact_Bdy %>" />
		</PopupInfoItems>
	</tis:PopupInfo>
	<uc1:popup ID="ucPopupContainer" runat="server" />
	<asp:PlaceHolder ID="phAdminFooter" runat="server" />
</div>
