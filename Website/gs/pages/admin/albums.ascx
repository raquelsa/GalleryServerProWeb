<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="albums.ascx.cs" Inherits="GalleryServerPro.Web.Pages.Admin.albums" %>
<%@ Register Src="../../Controls/popupinfo.ascx" TagName="popup" TagPrefix="uc1" %>
<%@ Register Assembly="GalleryServerPro.WebControls" Namespace="GalleryServerPro.WebControls"
	TagPrefix="tis" %>
<%@ Import Namespace="GalleryServerPro.Web" %>
<div id="d1" runat="server">
	<script type="text/javascript">

		Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(albumPageLoad);

		function albumPageLoad(sender, args) {
			configControls();
		}

		function chkEnablePaging_Click() {
			configControls();
		}

		function chkEnableAutoSync_Click() {
			configControls();
		}

		function chkEnableRemoteSync_Click() {
			configControls();
		}

		function configControls() {
			var txtPageSize = $get('<%= txtPageSize.ClientID %>');
			var chkEnablePaging = $get('<%= chkEnablePaging.ClientID %>');
			var ddlPagerLocation = $get('<%= ddlPagerLocation.ClientID %>');

			txtPageSize.disabled = (!chkEnablePaging.checked || chkEnablePaging.disabled);
			ddlPagerLocation.disabled = txtPageSize.disabled;

			var chkEnableAutoSync = $get('<%= chkEnableAutoSync.ClientID %>');
			$get('<%= txtAutoSyncIntervalMinutes.ClientID %>').disabled = (!chkEnableAutoSync.checked || chkEnableAutoSync.disabled);

			var chkEnableRemoteSync = $get('<%= chkEnableRemoteSync.ClientID %>');
			$get('<%= txtRemoteAccessPassword.ClientID %>').disabled = (!chkEnableRemoteSync.checked || chkEnableRemoteSync.disabled);
		}

	</script>
</div>
<div class="gsp_indentedContent">
	<asp:PlaceHolder ID="phAdminHeader" runat="server" />
	<div class="gsp_addpadding1">
		<p class="gsp_msgdark">
			<asp:Label ID="lbl1" runat="server" CssClass="gsp_bold" Text="<%$ Resources:GalleryServerPro, Admin_Settings_Apply_To_Label %>"
				EnableViewState="false" />&nbsp;<asp:Label ID="lblGalleryDescription" runat="server"
					EnableViewState="false" /></p>
		<tis:wwErrorDisplay ID="wwMessage" runat="server" UserMessage="<%$ Resources:GalleryServerPro, Validation_Summary_Text %>"
			CellPadding="2" UseFixedHeightWhenHiding="False" Center="False" Width="500px">
		</tis:wwErrorDisplay>
		<p class="admin_h3" style="margin-top: 0;">
			<asp:Literal ID="l1" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Albums_Thumbnail_Settings_Hdr %>" />
		</p>
		<div class="gsp_addleftpadding6">
			<p>
				<asp:CheckBox ID="chkEnablePaging" runat="server" onclick="chkEnablePaging_Click()"
					Text="<%$ Resources:GalleryServerPro, Admin_Albums_EnablePaging_Lbl %>" /></p>
			<table class="gsp_standardTable gsp_addleftmargin10">
				<tr>
					<td class="gsp_col1">
						<asp:Label ID="lblPageSize" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Albums_PageSize_Label %>" />
					</td>
					<td>
						<asp:TextBox ID="txtPageSize" runat="server" />&nbsp;<asp:RangeValidator ID="rvPageSize"
							runat="server" Display="Dynamic" ControlToValidate="txtPageSize" Type="Integer"
							MinimumValue="1" MaximumValue="2147483647" Text="<%$ Resources:GalleryServerPro, Validation_Positive_Int_Text %>" />
					</td>
				</tr>
				<tr>
					<td class="gsp_col1">
						<asp:Label ID="lblPagerLocation" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Albums_PagerLocation_Label %>" />
					</td>
					<td>
						<asp:DropDownList ID="ddlPagerLocation" runat="server" />
					</td>
				</tr>
			</table>
			<p class="gsp_addtopmargin10">
				<asp:Label ID="lblTitleDisplayLength" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Albums_TitleDisplayLength_Label %>" />&nbsp;<asp:TextBox
					ID="txtTitleDisplayLength" runat="server" />&nbsp;<asp:RangeValidator ID="rvTitleDisplayLength"
						runat="server" Display="Dynamic" ControlToValidate="txtTitleDisplayLength" Type="Integer"
						MinimumValue="1" MaximumValue="100" Text="<%$ Resources:GalleryServerPro, Validation_Int_1_To_100_Text %>" />
			</p>
		</div>
		<p class="admin_h3">
			<asp:Literal ID="l3" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Albums_Sync_Settings_Hdr %>" />
		</p>
		<div class="gsp_addleftpadding6">
			<p>
				<asp:CheckBox ID="chkEnableAutoSync" runat="server" onclick="chkEnableAutoSync_Click()"
					Text="<%$ Resources:GalleryServerPro, Admin_Albums_EnableAutoSync_Lbl %>" /></p>
			<div class="gsp_addleftmargin10">
				<p>
					<asp:Literal ID="l4" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Albums_AutoSyncIntervalMinutes_Lbl1 %>" />
					&nbsp;<asp:TextBox ID="txtAutoSyncIntervalMinutes" runat="server" CssClass="gsp_textbox_narrow" />&nbsp;<asp:Label
						ID="lblAutoSyncIntervalMinutesLbl" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Albums_AutoSyncIntervalMinutes_Lbl2 %>" /></p>
				<p>
					<asp:RangeValidator ID="rv1" runat="server" Display="Dynamic" ControlToValidate="txtAutoSyncIntervalMinutes"
						Type="Integer" MinimumValue="1" MaximumValue="2147483647" Text="<%$ Resources:GalleryServerPro, Validation_Positive_Int_Text %>" /></p>
				<p>
					<asp:Literal ID="l6" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Albums_LastAutoSync_Lbl %>" />&nbsp;<asp:Label
						ID="lblLastAutoSync" runat="server" CssClass="gsp_msgfriendly" /></p>
			</div>
			<p class="gsp_addtopmargin10">
				<asp:CheckBox ID="chkEnableRemoteSync" runat="server" onclick="chkEnableRemoteSync_Click()"
					Text="<%$ Resources:GalleryServerPro, Admin_Albums_EnableRemoteSync_Lbl %>" /></p>
			<div class="gsp_addleftpadding6">
				<p>
					<asp:Literal ID="l7" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Albums_RemoteAccessPassword_Lbl %>" />
					&nbsp;<asp:TextBox ID="txtRemoteAccessPassword" runat="server" /></p>
				<p>
					<asp:Literal ID="l8" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Albums_RemoteAccess_Msg %>" /></p>
					<p class="gsp_msgfriendly" runat="server"><a href="<%= SyncAllGalleriesUrl %>"><%= SyncAllGalleriesUrl %></a></p>
					<p class="gsp_msgfriendly" runat="server"><a href="<%= SyncAlbumUrl %>"><%= SyncAlbumUrl%></a></p>
			</div>
		</div>
		<p class="admin_h3">
			<asp:Literal ID="l3a" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Albums_Empty_Album_Thumbnail_Settings_Hdr %>" />
		</p>
		<div class="gsp_addleftpadding6">
			<p class="gsp_textcol">
				<asp:Literal ID="l4b" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Albums_Empty_Album_Thumbnail_Settings_Dtl %>" />
			</p>
			<table class="gsp_standardTable">
				<tr>
					<td class="gsp_col1">
						<asp:Label ID="lblThumbnailText" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Albums_Text_Label %>" />
					</td>
					<td>
						<asp:TextBox ID="txtText" runat="server" />
					</td>
				</tr>
				<tr>
					<td class="gsp_col1">
						<asp:Label ID="lblFontName" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Albums_FontName_Label %>" />
					</td>
					<td>
						<asp:TextBox ID="txtFontName" runat="server" />
					</td>
				</tr>
				<tr>
					<td class="gsp_col1">
						<asp:Label ID="lblFontSize" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Albums_FontSize_Label %>" />
					</td>
					<td>
						<asp:TextBox ID="txtFontSize" runat="server" />&nbsp;<asp:RangeValidator ID="rvFontSize"
							runat="server" Display="Dynamic" ControlToValidate="txtFontSize" Type="Integer"
							MinimumValue="6" MaximumValue="100" Text="<%$ Resources:GalleryServerPro, Validation_Font_Size_Text %>" />
					</td>
				</tr>
				<tr>
					<td class="gsp_col1">
						<asp:Label ID="lblFontColor" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Albums_FontColor_Label %>" />
					</td>
					<td>
						<asp:TextBox ID="txtFontColor" runat="server" />
						<asp:RequiredFieldValidator ID="rfv2" runat="server" ControlToValidate="txtFontColor"
							Display="Static" ErrorMessage="<%$ Resources:GalleryServerPro, Admin_Albums_General_Invalid_Color_Text %>"
							ForeColor="" CssClass="gsp_msgfailure">
						</asp:RequiredFieldValidator>
						<asp:CustomValidator ID="cvFontColor" runat="server" ControlToValidate="txtFontColor"
							Text="<%$ Resources:GalleryServerPro, Admin_Albums_General_Invalid_Color_Text %>"
							ErrorMessage="<%$ Resources:GalleryServerPro, Admin_Albums_General_Invalid_Color_Text %>"
							CssClass="gsp_msgwarning" OnServerValidate="cvColor_ServerValidate"></asp:CustomValidator>
					</td>
				</tr>
				<tr>
					<td class="gsp_col1">
						<asp:Label ID="lblBackgroundColor" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Albums_BackgroundColor_Label %>" />
					</td>
					<td>
						<asp:TextBox ID="txtBackgroundColor" runat="server" />
						<asp:RequiredFieldValidator ID="rfv3" runat="server" ControlToValidate="txtBackgroundColor"
							Display="Static" ErrorMessage="<%$ Resources:GalleryServerPro, Admin_Albums_General_Invalid_Color_Text %>"
							ForeColor="" CssClass="gsp_msgfailure">
						</asp:RequiredFieldValidator>
						<asp:CustomValidator ID="cvBackgroundColor" runat="server" ControlToValidate="txtBackgroundColor"
							Text="<%$ Resources:GalleryServerPro, Admin_Albums_General_Invalid_Color_Text %>"
							ErrorMessage="<%$ Resources:GalleryServerPro, Admin_Albums_General_Invalid_Color_Text %>"
							CssClass="gsp_msgwarning" OnServerValidate="cvColor_ServerValidate"></asp:CustomValidator>
					</td>
				</tr>
				<tr>
					<td class="gsp_col1">
						<asp:Label ID="lblAspectRatio" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Albums_AspectRatio_Label %>" />
					</td>
					<td>
						<asp:TextBox ID="txtAspectRatio" runat="server" />
					</td>
				</tr>
			</table>
		</div>
	</div>
	<tis:wwDataBinder ID="wwDataBinder" runat="server" OnAfterBindControl="wwDataBinder_AfterBindControl"
		OnBeforeUnbindControl="wwDataBinder_BeforeUnbindControl" OnValidateControl="wwDataBinder_ValidateControl">
		<DataBindingItems>
			<tis:wwDataBindingItem ID="bi1" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="PageSize" ControlId="txtPageSize" UserFieldName="<%$ Resources:GalleryServerPro, Admin_Albums_PageSize_Label %>" />
			<tis:wwDataBindingItem ID="bi2" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="PagerLocation" ControlId="ddlPagerLocation" BindingProperty="SelectedValue"
				UserFieldName="<%$ Resources:GalleryServerPro, Admin_Albums_PagerLocation_Label %>" />
			<tis:wwDataBindingItem ID="bi3" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="MaxAlbumThumbnailTitleDisplayLength" ControlId="txtTitleDisplayLength"
				UserFieldName="<%$ Resources:GalleryServerPro, Admin_Albums_TitleDisplayLength_Label %>" />
			<tis:wwDataBindingItem ID="bi4" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="EnableAutoSync" ControlId="chkEnableAutoSync" UserFieldName="<%$ Resources:GalleryServerPro, Admin_Albums_EnableAutoSync_Lbl %>"
				BindingProperty="Checked" />
			<tis:wwDataBindingItem ID="bi5" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="AutoSyncIntervalMinutes" ControlId="txtAutoSyncIntervalMinutes"
				UserFieldName="<%$ Resources:GalleryServerPro, Admin_Albums_AutoSyncIntervalMinutes_Lbl %>" />
			<tis:wwDataBindingItem ID="bi6" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="LastAutoSync" ControlId="lblLastAutoSync" BindingMode="OneWay" />
			<tis:wwDataBindingItem ID="bi7" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="EnableRemoteSync" ControlId="chkEnableRemoteSync" UserFieldName="<%$ Resources:GalleryServerPro, Admin_Albums_EnableRemoteSync_Lbl %>"
				BindingProperty="Checked" />
			<tis:wwDataBindingItem ID="bi8" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="RemoteAccessPassword" ControlId="txtRemoteAccessPassword"
				UserFieldName="<%$ Resources:GalleryServerPro, Admin_Albums_RemoteAccessPassword_Lbl %>" />
			<tis:wwDataBindingItem ID="bi20" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="EmptyAlbumThumbnailText" ControlId="txtText" UserFieldName="<%$ Resources:GalleryServerPro, Admin_Albums_Text_Label %>" />
			<tis:wwDataBindingItem ID="bi21" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="EmptyAlbumThumbnailFontName" ControlId="txtFontName" UserFieldName="<%$ Resources:GalleryServerPro, Admin_Albums_FontName_Label %>" />
			<tis:wwDataBindingItem ID="bi22" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="EmptyAlbumThumbnailFontSize" ControlId="txtFontSize" UserFieldName="<%$ Resources:GalleryServerPro, Admin_Albums_FontSize_Label %>" />
			<tis:wwDataBindingItem ID="bi23" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="EmptyAlbumThumbnailFontColor" ControlId="txtFontColor" UserFieldName="<%$ Resources:GalleryServerPro, Admin_Albums_FontColor_Label %>" />
			<tis:wwDataBindingItem ID="bi24" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="EmptyAlbumThumbnailBackgroundColor" ControlId="txtBackgroundColor"
				UserFieldName="<%$ Resources:GalleryServerPro, Admin_Albums_BackgroundColor_Label %>" />
			<tis:wwDataBindingItem ID="bi25" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="EmptyAlbumThumbnailWidthToHeightRatio" ControlId="txtAspectRatio"
				UserFieldName="<%$ Resources:GalleryServerPro, Admin_Albums_AspectRatio_Label %>" />
			<tis:wwDataBindingItem runat="server" ControlId="chkEnablePaging">
			</tis:wwDataBindingItem>
		</DataBindingItems>
	</tis:wwDataBinder>
	<tis:PopupInfo ID="PopupInfo" runat="server" DialogControlId="dgPopup" DefaultDialogTitleCss="dg5ContentTitleCss"
		DefaultDialogBodyCss="dg5ContentBodyCss">
		<PopupInfoItems>
			<tis:PopupInfoItem ID="pi1" runat="server" ControlId="chkEnablePaging" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_enabledPaging_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_enabledPaging_Bdy %>" />
			<tis:PopupInfoItem ID="pi2" runat="server" ControlId="lblPageSize" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_pageSize_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_pageSize_Bdy %>" />
			<tis:PopupInfoItem ID="pi3" runat="server" ControlId="lblPagerLocation" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_pagerLocation_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_pagerLocation_Bdy %>" />
			<tis:PopupInfoItem ID="pi4" runat="server" ControlId="lblTitleDisplayLength" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_maxAlbumThumbnailTitleDisplayLength_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_maxAlbumThumbnailTitleDisplayLength_Bdy %>" />
			<tis:PopupInfoItem ID="pi5" runat="server" ControlId="chkEnableAutoSync" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_enableAutoSync_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_enableAutoSync_Bdy %>" />
			<tis:PopupInfoItem ID="pi6" runat="server" ControlId="lblAutoSyncIntervalMinutesLbl"
				DialogTitle="<%$ Resources:GalleryServerPro, Cfg_autoSyncIntervalMinutes_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_autoSyncIntervalMinutes_Bdy %>" />
			<tis:PopupInfoItem ID="pi7" runat="server" ControlId="chkEnableRemoteSync" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_enableRemoteSync_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_enableRemoteSync_Bdy %>" />
			<tis:PopupInfoItem ID="pi8" runat="server" ControlId="txtRemoteAccessPassword" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_remoteAccessPassword_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_remoteAccessPassword_Bdy %>" />
			<tis:PopupInfoItem ID="pi20" runat="server" ControlId="lblThumbnailText" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_defaultAlbumThumbnailText_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_defaultAlbumThumbnailText_Bdy %>" />
			<tis:PopupInfoItem ID="pi21" runat="server" ControlId="lblFontName" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_defaultAlbumThumbnailFontName_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_defaultAlbumThumbnailFontName_Bdy %>" />
			<tis:PopupInfoItem ID="pi22" runat="server" ControlId="lblFontSize" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_defaultAlbumThumbnailFontSize_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_defaultAlbumThumbnailFontSize_Bdy %>" />
			<tis:PopupInfoItem ID="pi23" runat="server" ControlId="lblFontColor" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_defaultAlbumThumbnailFontColor_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_defaultAlbumThumbnailFontColor_Bdy %>" />
			<tis:PopupInfoItem ID="pi24" runat="server" ControlId="lblBackgroundColor" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_defaultAlbumThumbnailBackgroundColor_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_defaultAlbumThumbnailBackgroundColor_Bdy %>" />
			<tis:PopupInfoItem ID="pi25" runat="server" ControlId="lblAspectRatio" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_defaultAlbumThumbnailWidthToHeightRatio_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_defaultAlbumThumbnailWidthToHeightRatio_Bdy %>" />
		</PopupInfoItems>
	</tis:PopupInfo>
	<uc1:popup ID="ucPopupContainer" runat="server" />
	<asp:PlaceHolder ID="phAdminFooter" runat="server" />
</div>
