<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="gallerysettings.ascx.cs"
	Inherits="GalleryServerPro.Web.Pages.Admin.gallerysettings" %>
<%@ Register Src="../../Controls/popupinfo.ascx" TagName="popup" TagPrefix="uc1" %>
<%@ Register Assembly="GalleryServerPro.WebControls" Namespace="GalleryServerPro.WebControls"
	TagPrefix="tis" %>
<%@ Register Namespace="ComponentArt.Web.UI" Assembly="ComponentArt.Web.UI" TagPrefix="ComponentArt" %>
<%@ Import Namespace="GalleryServerPro.Web" %>
<div id="css" runat="server" visible="false">
	<link href="../../styles/gallery.css" rel="stylesheet" type="text/css" />
</div>
<div runat="server">
	<script type="text/javascript">

		Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(generalPageLoad);

		function generalPageLoad(sender, args)
		{
			gsp_configControlsPageLoad();
		}

		function gsp_configControlsPageLoad()
		{
			// If the combobox object isn't ready, wait 1/10 second and check again. When ready call gsp_configControls().
			if (typeof (cboUsersToNotify) === 'undefined')
			{
				setTimeout('gsp_configControlsPageLoad()', 100);
			}
			else
			{
				gsp_configControls();
			}
		}

		function chkShowHeader_Click()
		{
			gsp_configControls();
		}

		function chkSendEmail_Click()
		{
			gsp_configControls();
		}

		function chkEnableExceptionHandler_Click()
		{
			gsp_configControls();
		}

		function chkAllowAnonymousBrowsing_Click()
		{
			gsp_configControls();
		}

		function gsp_configControls()
		{
			// Enable/disable the 'users to notify when error occurs' combobox
			var chkEnableErrorEmailing = $get('<%= chkSendEmail.ClientID %>');

			if (!chkEnableErrorEmailing.checked || chkEnableErrorEmailing.disabled)
				cboUsersToNotify.disable();
			else
				cboUsersToNotify.enable();

			// Enable/disable the 'show error details' checkbox
			var chkEnableExceptionHandler = $get('<%= chkEnableExceptionHandler.ClientID %>');
			$get('<%= chkShowErrorDetails.ClientID %>').disabled = (!chkEnableExceptionHandler.checked || chkEnableExceptionHandler.disabled);

			// Enable/disable the 'show high-res' checkbox
			var chkAllowAnonymousBrowsing = $get('<%= chkAllowAnonymousBrowsing.ClientID %>');
			$get('<%= chkAllowAnonymousHiResBrowsing.ClientID %>').disabled = (!chkAllowAnonymousBrowsing.checked || chkAllowAnonymousBrowsing.disabled);

			if (!$get('<%= chkShowAllUsers.ClientID %>').checked)
				hideNonAdminUsers();

			// Enable/disable the 'Show header' controls
			var chkShowHeader = $get('<%= chkShowHeader.ClientID %>');

			if (!chkShowHeader.checked || chkShowHeader.disabled)
			{
				$get('<%= txtWebsiteTitle.ClientID %>').disabled = true;
				$get('<%= txtWebsiteTitleUrl.ClientID %>').disabled = true;
				$get('<%= chkShowLogin.ClientID %>').disabled = true;
				$get('<%= chkShowSearch.ClientID %>').disabled = true;
			}
			else
			{
				$get('<%= txtWebsiteTitle.ClientID %>').disabled = false;
				$get('<%= txtWebsiteTitleUrl.ClientID %>').disabled = false;
				$get('<%= chkShowLogin.ClientID %>').disabled = false;
				$get('<%= chkShowSearch.ClientID %>').disabled = false;
			}
		}

		function hideNonAdminUsers()
		{
			$("table.gsp_j_userlist tr:has(td > span.gsp_j_notadmin)").hide();
		}

		function gsp_handleUsersToNotifyComboBoxSelection(sender, e)
		{
			var userList = "";
			$('table#<%= cblU.ClientID %> input:checkbox:checked').next().each(function () { userList = userList + this.innerHTML + ', '; });
			userList = userList.replace(/&lt;/g, "<").replace(/&gt;/g, ">");
			cboUsersToNotify.set_text(userList.slice(0, -2));
			cboUsersToNotify.collapse();
		}

		function toggleAdminUsers(chk)
		{
			if (chk.checked)
				$("table.gsp_j_userlist tr:has(td > span.gsp_j_notadmin)").fadeIn();
			else
				$("table.gsp_j_userlist tr:has(td > span.gsp_j_notadmin)").fadeOut();
		}

		function confirmGalleryDeletion(galleryDescription)
		{
			return confirm("Are you sure you want to delete the gallery " + galleryDescription + "?");
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
		<tis:wwErrorDisplay ID="wwMessage2" runat="server" UserMessage="<%$ Resources:GalleryServerPro, Validation_Summary_Text %>"
			CellPadding="2" UseFixedHeightWhenHiding="False" Center="False" Width="500px">
		</tis:wwErrorDisplay>
		<p class="admin_h3" style="margin-top: 0;">
			<asp:Literal ID="l4" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_SystemSettings_Hdr %>" /></p>
		<div class="gsp_addleftpadding6">
			<p class="gsp_addtopmargin5">
				<asp:CheckBox ID="chkShowHeader" runat="server" onclick="chkShowHeader_Click()" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_ShowHeader_Label %>" />
			</p>
			<div class="gsp_collapse gsp_addleftmargin5">
				<table class="gsp_standardTable">
					<tr>
						<td class="gsp_col1">
							<asp:Label ID="lblWebsiteTitle" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_WebsiteTitle_Label %>" />
						</td>
						<td>
							<asp:TextBox ID="txtWebsiteTitle" runat="server" CssClass="gsp_textbox" />
						</td>
					</tr>
					<tr>
						<td class="gsp_col1">
							<asp:Label ID="lblWebsiteTitleUrl" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_WebsiteTitleUrl_Label %>" />
						</td>
						<td>
							<asp:TextBox ID="txtWebsiteTitleUrl" runat="server" CssClass="gsp_textbox" />
						</td>
					</tr>
				</table>
				<p>
					<asp:CheckBox ID="chkShowLogin" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_ShowLogin_Label %>" />
				</p>
				<p>
					<asp:CheckBox ID="chkShowSearch" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_ShowSearch_Label %>" />
				</p>
			</div>
			<p class="gsp_addtopmargin10">
				<asp:CheckBox ID="chkAllowAnonymousBrowsing" runat="server" onclick="chkAllowAnonymousBrowsing_Click()"
					Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_AllowAnonymousBrowsing_Label %>" />
			</p>
			<p class="gsp_addleftmargin5">
				<asp:CheckBox ID="chkAllowAnonymousHiResBrowsing" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_AllowAnonymousHiResBrowsing_Label %>" />
			</p>
		</div>
		<p class="admin_h3">
			<asp:Literal ID="l1" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_Email_Settings_Hdr %>" />
		</p>
		<p class="gsp_bold gsp_addtopmargin5">
			<asp:Literal ID="l3" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_Smtp_Settings_Hdr %>" />
		</p>
		<div class="gsp_addleftpadding6">
			<table class="gsp_standardTable">
				<tr>
					<td class="gsp_col1">
						<asp:Label ID="lblSmtpServer" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_SmtpServer_Label %>" />
					</td>
					<td>
						<asp:TextBox ID="txtSmtpServer" runat="server" />
					</td>
				</tr>
				<tr>
					<td class="gsp_col1">
						<asp:Label ID="lblSmtpPort" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_SmtpPort_Label %>" />
					</td>
					<td>
						<asp:TextBox ID="txtSmtpPort" runat="server" />
					</td>
				</tr>
			</table>
			<p class="gsp_addtopmargin5">
				<asp:CheckBox ID="chkUseSsl" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_UseSsl_Label %>" />
			</p>
		</div>
		<p class="gsp_bold gsp_addtopmargin5">
			<asp:Literal ID="l7" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_From_Hdr %>" />
		</p>
		<table class="gsp_addleftpadding6 gsp_standardTable">
			<tr>
				<td class="gsp_col1">
					<asp:Label ID="lblEmailFromName" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_EmailName_Label %>" />
				</td>
				<td>
					<asp:TextBox ID="txtEmailFromName" runat="server" CssClass="gsp_textbox" />
				</td>
			</tr>
			<tr>
				<td class="gsp_col1">
					<asp:Label ID="lblEmailFromAddress" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_EmailAddress_Label %>" />
				</td>
				<td>
					<asp:TextBox ID="txtEmailFromAddress" runat="server" CssClass="gsp_textbox" />
				</td>
			</tr>
		</table>
		<p class="gsp_addleftpadding6">
			<asp:Button ID="btnEmailTest" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_Sent_Test_Email_Text %>"
				OnClick="btnEmailTest_Click" /></p>
		<p class="admin_h3">
			<asp:Literal ID="l5" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_ErrorHandling_Hdr %>" /></p>
		<div class="gsp_addleftpadding6">
			<p class="gsp_addtopmargin5">
				<asp:CheckBox ID="chkSendEmail" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_SendEmail_Label %>"
					onclick="chkSendEmail_Click()" />
			</p>
			<div class="gsp_addleftpadding6">
				<p>
					<asp:Label ID="lblUsersToNotify" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_General_Users_To_Notify_When_Error_Occurs_Lbl %>" />
				</p>
				<ComponentArt:ComboBox ID="cboUsersToNotify" runat="Server" Width="575" Height="20"
					DropDownWidth="575" DropDownHeight="300" AutoHighlight="true" AutoComplete="true"
					AutoFilter="false" ItemCssClass="gsp_ddn-item" ItemHoverCssClass="gsp_ddn-item-hover"
					CssClass="gsp_cmb" TextBoxCssClass="gsp_txt" DropDownResizingMode="Corner" DropDownCssClass="gsp_ns gsp_ddn"
					DropDownContentCssClass="gsp_ddn-con" SelectedItemCssClass="gsp_ddn-item-hover"
					KeyboardEnabled="true" DropDownOffsetY="2">
					<DropDownHeader>
						<p>
							<asp:CheckBox ID="chkShowAllUsers" runat="server" CssClass="gsp_fs" onclick="toggleAdminUsers(this);"
								Text="<%$ Resources:GalleryServerPro, Admin_General_Show_All_Users_Lbl %>" />
						</p>
						<p class="gsp_addleftpadding2">
							<a href="javascript:gsp_handleUsersToNotifyComboBoxSelection();">
								<asp:Literal ID="l6" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_General_Choose_Items_Lbl %>" /></a></p>
					</DropDownHeader>
					<DropDownContent>
						<asp:CheckBoxList ID="cblU" runat="server" SelectionMode="Multiple" CssClass="gsp_j_userlist"
							EnableViewState="false" />
					</DropDownContent>
					<DropDownFooter>
						<div style="text-align: right;">
							<img src="<%= Utils.GetUrl("/images/componentart/combobox/bottom_right.gif") %>" alt="" /></div>
					</DropDownFooter>
				</ComponentArt:ComboBox>
			</div>
			<p class="gsp_addtopmargin5">
				<asp:CheckBox ID="chkEnableExceptionHandler" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_EnableExceptionHandler_Label %>"
					onclick="chkEnableExceptionHandler_Click()" />
			</p>
			<p class="gsp_addleftmargin5">
				<asp:CheckBox ID="chkShowErrorDetails" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_ShowErrorDetails_Label %>" />
			</p>
			<p>
				<asp:Button ID="btnThrowError" runat="server" Text="Generate sample error" OnClick="btnThrowError_Click" /></p>
		</div>
	</div>
	<tis:wwDataBinder ID="wwDataBinder" runat="server" OnValidateControl="wwDataBinder_ValidateControl"
		OnBeforeUnbindControl="wwDataBinder_BeforeUnbindControl">
		<DataBindingItems>
			<tis:wwDataBindingItem ID="wbi1" runat="server" ControlId="chkShowHeader" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="ShowHeader" UserFieldName="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_ShowHeader_Label %>"
				BindingProperty="Checked" />
			<tis:wwDataBindingItem ID="wbi2" runat="server" ControlId="txtWebsiteTitle" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="GalleryTitle" UserFieldName="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_WebsiteTitle_Label %>"
				IsRequired="False" />
			<tis:wwDataBindingItem ID="wbi3" runat="server" ControlId="txtWebsiteTitleUrl" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="GalleryTitleUrl" UserFieldName="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_WebsiteTitleUrl_Label %>"
				IsRequired="False" />
			<tis:wwDataBindingItem ID="wbi4" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="AllowAnonymousBrowsing" ControlId="chkAllowAnonymousBrowsing"
				BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_AllowAnonymousBrowsing_Label %>" />
			<tis:wwDataBindingItem ID="wbi5" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="AllowAnonymousHiResViewing" ControlId="chkAllowAnonymousHiResBrowsing"
				BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_AllowAnonymousBrowsing_Label %>" />
			<tis:wwDataBindingItem ID="wbi6" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="ShowLogin" ControlId="chkShowLogin" BindingProperty="Checked"
				UserFieldName="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_ShowLogin_Label %>" />
			<tis:wwDataBindingItem ID="wbi7" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="ShowSearch" ControlId="chkShowSearch" BindingProperty="Checked"
				UserFieldName="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_ShowSearch_Label %>" />
			<tis:wwDataBindingItem ID="wbi8" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="EmailFromName" ControlId="txtEmailFromName" UserFieldName="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_EmailName_Label %>" />
			<tis:wwDataBindingItem ID="wbi9" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="EmailFromAddress" ControlId="txtEmailFromAddress" UserFieldName="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_EmailAddress_Label %>" />
			<tis:wwDataBindingItem ID="wbi10" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="SmtpServer" ControlId="txtSmtpServer" UserFieldName="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_SmtpServer_Label %>" />
			<tis:wwDataBindingItem ID="wbi11" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="SmtpServerPort" ControlId="txtSmtpPort" UserFieldName="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_SmtpPort_Label %>" />
			<tis:wwDataBindingItem ID="wbi12" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="SendEmailUsingSsl" ControlId="chkUseSsl" BindingProperty="Checked"
				UserFieldName="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_UseSsl_Label %>" />
			<tis:wwDataBindingItem ID="wbi13" runat="server" ControlId="btnEmailTest">
			</tis:wwDataBindingItem>
			<tis:wwDataBindingItem ID="wbi14" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="SendEmailOnError" ControlId="chkSendEmail" BindingProperty="Checked"
				UserFieldName="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_SendEmail_Label %>" />
			<tis:wwDataBindingItem ID="wbi15" runat="server" BindingSource="this" BindingSourceMember="UsersToNotifyWhenErrorOccurs"
				ControlId="cboUsersToNotify" UserFieldName="<%$ Resources:GalleryServerPro, Admin_General_Users_To_Notify_When_Error_Occurs_Lbl %>" />
			<tis:wwDataBindingItem ID="wbi16" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="EnableExceptionHandler" ControlId="chkEnableExceptionHandler"
				BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_EnableExceptionHandler_Label %>" />
			<tis:wwDataBindingItem ID="wbi17" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="ShowErrorDetails" ControlId="chkShowErrorDetails" BindingProperty="Checked"
				UserFieldName="<%$ Resources:GalleryServerPro, Admin_Gallery_Settings_ShowErrorDetails_Label %>" />
		</DataBindingItems>
	</tis:wwDataBinder>
	<tis:PopupInfo ID="PopupInfo" runat="server" DialogControlId="dgPopup" DefaultDialogTitleCss="dg5ContentTitleCss"
		DefaultDialogBodyCss="dg5ContentBodyCss">
		<PopupInfoItems>
			<tis:PopupInfoItem ID="poi1" runat="server" ControlId="chkShowHeader" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_showHeader_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_showHeader_Bdy %>" />
			<tis:PopupInfoItem ID="poi3" runat="server" ControlId="lblWebsiteTitle" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_pageHeaderText_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_pageHeaderText_Bdy %>" />
			<tis:PopupInfoItem ID="poi4" runat="server" ControlId="lblWebsiteTitleUrl" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_pageHeaderTextUrl_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_pageHeaderTextUrl_Bdy %>" />
			<tis:PopupInfoItem ID="poi5" runat="server" ControlId="chkAllowAnonymousBrowsing"
				DialogTitle="<%$ Resources:GalleryServerPro, Cfg_allowAnonymousBrowsing_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_allowAnonymousBrowsing_Bdy %>" />
			<tis:PopupInfoItem ID="poi6" runat="server" ControlId="chkAllowAnonymousHiResBrowsing"
				DialogTitle="<%$ Resources:GalleryServerPro, Cfg_allowAnonymousHiResBrowsing_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_allowAnonymousHiResBrowsing_Bdy %>" />
			<tis:PopupInfoItem ID="poi7" runat="server" ControlId="chkShowLogin" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_showLogin_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_showLogin_Bdy %>" />
			<tis:PopupInfoItem ID="poi8" runat="server" ControlId="chkShowSearch" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_showSearch_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_showSearch_Bdy %>" />
			<tis:PopupInfoItem ID="poi9" runat="server" ControlId="lblEmailFromName" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_emailFromName_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_emailFromName_Bdy %>" />
			<tis:PopupInfoItem ID="poi10" runat="server" ControlId="lblEmailFromAddress" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_emailFromAddress_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_emailFromAddress_Bdy %>" />
			<tis:PopupInfoItem ID="poi11" runat="server" ControlId="lblSmtpServer" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_smtpServer_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_smtpServer_Bdy %>" />
			<tis:PopupInfoItem ID="poi12" runat="server" ControlId="lblSmtpPort" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_smtpPort_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_smtpPort_Bdy %>" />
			<tis:PopupInfoItem ID="poi13" runat="server" ControlId="chkUseSsl" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_sendEmailUsingSsl_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_sendEmailUsingSsl_Bdy %>" />
			<tis:PopupInfoItem ID="poi14" runat="server" ControlId="chkSendEmail" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_sendEmailOnError_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_sendEmailOnError_Bdy %>" />
			<tis:PopupInfoItem ID="poi15" runat="server" ControlId="chkEnableExceptionHandler"
				DialogTitle="<%$ Resources:GalleryServerPro, Cfg_enableExceptionHandler_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_enableExceptionHandler_Bdy %>" />
			<tis:PopupInfoItem ID="poi16" runat="server" ControlId="chkShowErrorDetails" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_showErrorDetails_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_showErrorDetails_Bdy %>" />
		</PopupInfoItems>
	</tis:PopupInfo>
	<uc1:popup ID="ucPopupContainer" runat="server" />
	<asp:PlaceHolder ID="phAdminFooter" runat="server" />
</div>
