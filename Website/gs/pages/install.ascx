<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="install.ascx.cs" Inherits="GalleryServerPro.Web.Pages.install" %>
<%@ Import Namespace="GalleryServerPro.Web" %>
<link href="<%= Utils.GalleryRoot %>/styles/gallery.css" rel="stylesheet" type="text/css" />
<link href="<%= Utils.GalleryRoot %>/styles/ca_styles.css" rel="stylesheet" type="text/css" />
<!-- Enable or disable this installer by changing the value of the ENABLE_SETUP hidden field to true or false. -->
<asp:HiddenField ID="ENABLE_SETUP" runat="server" Value="false" />
<script type="text/javascript">

	Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(gsp_installPageLoad);

	function gsp_installPageLoad(sender, args) {
		gsp_addHandlers();
		gsp_configControls();
	}

	function gsp_addHandlers() {
		// Add click event handlers to the radio buttons so that we can enable/disable
		// the username/password textboxes as needed.
		var rblDbAdminConnectType = $get('<%= rblDbAdminConnectType.ClientID %>');
		if (rblDbAdminConnectType) {
			var rbWinAuth = rblDbAdminConnectType.getElementsByTagName("input")[0];
			var rbSqlAuth = rblDbAdminConnectType.getElementsByTagName("input")[1];

			$addHandler(rbWinAuth, 'click', gsp_configControls);
			$addHandler(rbSqlAuth, 'click', gsp_configControls);
		}

		var rblDbRuntimeConnectType = $get('<%= rblDbRuntimeConnectType.ClientID %>');
		if (rblDbRuntimeConnectType) {
			var rbUsePrev = rblDbRuntimeConnectType.getElementsByTagName("input")[0];
			var rbWinAuth = rblDbRuntimeConnectType.getElementsByTagName("input")[1];
			var rbSqlAuth = rblDbRuntimeConnectType.getElementsByTagName("input")[2];

			$addHandler(rbUsePrev, 'click', gsp_configControls);
			$addHandler(rbWinAuth, 'click', gsp_configControls);
			$addHandler(rbSqlAuth, 'click', gsp_configControls);
		}
	}

	function gsp_configControls() {
		// Enable the username/password textbox only when SQL authentication is selected.
		// Step 1: Process DB Admin account
		var rblDbAdminConnectType = $get('<%= rblDbAdminConnectType.ClientID %>');
		var txtDbAdminUserName = $get('<%= txtDbAdminUserName.ClientID %>');
		var txtDbAdminPassword = $get('<%= txtDbAdminPassword.ClientID %>');

		if (rblDbAdminConnectType) {
			var rbWinAuth = rblDbAdminConnectType.getElementsByTagName("input")[0];
			var rbSqlAuth = rblDbAdminConnectType.getElementsByTagName("input")[1];

			txtDbAdminUserName.disabled = (!rbSqlAuth.checked);
			txtDbAdminPassword.disabled = (!rbSqlAuth.checked);
		}

		// Step 2: Process DB Runtime account
		var rblDbRuntimeConnectType = $get('<%= rblDbRuntimeConnectType.ClientID %>');
		var txtDbRuntimeUserName = $get('<%= txtDbRuntimeUserName.ClientID %>');
		var txtDbRuntimePassword = $get('<%= txtDbRuntimePassword.ClientID %>');

		if (rblDbRuntimeConnectType) {
			var rbUsePrev = rblDbRuntimeConnectType.getElementsByTagName("input")[0];
			var rbWinAuth = rblDbRuntimeConnectType.getElementsByTagName("input")[1];
			var rbSqlAuth = rblDbRuntimeConnectType.getElementsByTagName("input")[2];

			txtDbRuntimeUserName.disabled = (!rbSqlAuth.checked);
			txtDbRuntimePassword.disabled = (!rbSqlAuth.checked);
		}
	}

</script>
<div class="gsp_ns">
	<div id="wizInstall" class="gsp_rounded10">
		<div id="wizHdr" class="gsp_rounded10">
			<img src="<%= Utils.GalleryRoot %>/images/gsp_logo_313x75.png" style="width: 313px;
				height: 75px;" alt="Gallery Server Pro logo" />
		</div>
		<div id="wizCtnt" class="wizCtnt" runat="server">
			<asp:Panel ID="Welcome" runat="server" Visible="false">
				<p class="gsp_h2">
					<asp:Literal ID="l1" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_Welcome_Hdr %>" />
				</p>
				<p>
					<asp:Literal ID="l2" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_Welcome_Dtl %>" />
				</p>
				<p class="gsp_msgfriendly">
					<asp:Literal ID="l2b" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_Welcome_Dtl2 %>" />
				</p>
				<p class="gsp_bold gsp_addtopmargin5">
					<asp:Literal ID="l3" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_Welcome_Server_Req_Hdr %>" />
				</p>
				<ul style="list-style-type: disc; margin-left: 3em;">
					<li>
						<asp:Literal ID="l4" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_Welcome_Server_Req1 %>" /></li>
					<li>
						<asp:Literal ID="l5" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_Welcome_Server_Req2 %>" /></li>
				</ul>
				<p class="gsp_bold gsp_addleftmargin5 gsp_addtopmargin5">
					<asp:Literal ID="l8" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_Welcome_Server_Opt_Req_Hdr %>" /></p>
				<ul style="list-style-type: disc; margin-left: 3em;">
					<li>
						<asp:Literal ID="l9" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_Welcome_Server_Opt_Req1 %>" /></li>
				</ul>
				<p class="gsp_bold gsp_addtopmargin5">
					<asp:Literal ID="l10" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_Welcome_Client_Req_Hdr %>" />
				</p>
				<ul style="list-style-type: disc; margin-left: 3em;">
					<li>
						<asp:Literal ID="Literal1" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_Welcome_Client_Req1 %>" /></li>
				</ul>
			</asp:Panel>
			<asp:Panel ID="License" runat="server" Visible="false">
				<p class="gsp_h2">
					<asp:Literal ID="l11" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_License_Hdr %>" />
				</p>
				<p style="text-align: right; margin-top: -1em;">
					<a href="<%= Utils.GalleryRoot %>/pages/installer/eula.txt" target="_blank">
						<asp:Literal ID="l12" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_License_Printable_Version %>" /></a></p>
				<iframe frameborder="1" scrolling="yes" src="<%= Utils.GalleryRoot %>/pages/installer/eula.htm"
					style="width: 100%; height: 350px;"></iframe>
				<p style="text-align: right;">
					<asp:CheckBox ID="chkLicenseAgreement" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_License_Agree_Text %>" /></p>
			</asp:Panel>
			<asp:Panel ID="DataProvider" runat="server" Visible="false">
				<p class="gsp_h2">
					<asp:Literal ID="l57" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_DataProvider_Hdr %>" />
				</p>
				<p id="lblErrMsgDataProvider" runat="server" class="gsp_invisible" enableviewstate="false" />
				<p>
					<asp:Literal ID="l58" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_DataProvider_Dtl1 %>" />
				</p>
				<p>
					<asp:Literal ID="l59" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_DataProvider_Dtl2 %>" />
				</p>
				<p class="gsp_bold gsp_addtopmargin5">
					<asp:Literal ID="l60" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_DataProvider_Select_Hdr %>" />
				</p>
				<div class="gsp_addleftpadding5">
					<p>
						<asp:RadioButton ID="rbDataProviderSqlCe" runat="server" GroupName="DataProvider"
							Text="SQL CE (SQL Server Compact)" /></p>
					<p>
						<asp:RadioButton ID="rbDataProviderSqlServer" runat="server" GroupName="DataProvider"
							Text="SQL Server / SQL Server Express (2005 or higher)" /></p>
				</div>
				<asp:Panel ID="pnlDbEngineMsg" runat="server" CssClass="gsp_addleftpadding5" Visible="false"
					EnableViewState="false">
					<img src="<%= Utils.GalleryRoot %>/images/info_32x32.png" alt="" style="width: 32px;
						height: 32px; float: left; padding-top: 0.6em;" />
					<div style="margin-left: 40px;">
						<p id="lblErrMsgChooseDbEngine" runat="server" enableviewstate="false" />
					</div>
				</asp:Panel>
			</asp:Panel>
			<asp:Panel ID="DbAdmin" runat="server" Visible="false">
				<p class="gsp_h2">
					<asp:Literal ID="l13" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_DbAdmin_Hdr %>" />
				</p>
				<p id="lblErrMsgDbAdmin" runat="server" class="gsp_invisible" enableviewstate="false" />
				<p>
					<asp:Literal ID="l14" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_DbAdmin_Dtl1 %>" />
				</p>
				<p class="gsp_msgfriendly">
					<asp:Literal ID="l15" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_DbAdmin_Dtl2 %>" />
				</p>
				<div>
					<p class="gsp_bold gsp_addtopmargin5">
						<asp:Literal ID="l16" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_DbAdmin_Sql_Hdr %>" />
					</p>
					<div class="gsp_addleftpadding5">
						<p class="gsp_collapse">
							<asp:TextBox ID="txtDbSqlName" runat="server" Text="(local)" CssClass="textcol" /></p>
						<p class="gsp_fss gsp_minimargin">
							<asp:Literal ID="l17" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_DbAdmin_Sql_Name_Examples %>" />
						</p>
					</div>
					<p class="gsp_bold gsp_addtopmargin5">
						<asp:Literal ID="l18" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_DbAdmin_Sql_Auth_Hdr %>" />
					</p>
					<div class="gsp_addleftpadding5">
						<asp:RadioButtonList ID="rblDbAdminConnectType" runat="server">
						</asp:RadioButtonList>
						<div class="gsp_addtopmargin2">
							<table style="padding-left: 50px;">
								<tr>
									<td>
										<asp:Literal ID="l19" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_DbAdmin_Sql_Auth_UserName_Label %>" />
									</td>
									<td>
										<asp:TextBox ID="txtDbAdminUserName" runat="server" />&nbsp;<asp:CustomValidator
											ID="cvDbAdminSqlLogOn" runat="server" CssClass="msgwarning" Display="Dynamic" ErrorMessage="<%$ Resources:GalleryServerPro, Installer_DbAdmin_Sql_Auth_UserName_Val_Err %>"
											OnServerValidate="cvDbAdminSqlLogOn_ServerValidate" />
									</td>
								</tr>
								<tr>
									<td>
										<asp:Literal ID="l20" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_DbAdmin_Sql_Auth_Pwd_Label %>" />
									</td>
									<td>
										<asp:TextBox ID="txtDbAdminPassword" runat="server" TextMode="Password" />&nbsp;<asp:CustomValidator
											ID="cvDbAdminSqlPassword" runat="server" CssClass="msgwarning" Display="Dynamic"
											ErrorMessage="<%$ Resources:GalleryServerPro, Installer_DbAdmin_Sql_Auth_Pwd_Val_Err %>"
											OnServerValidate="cvDbAdminSqlPassword_ServerValidate" />
									</td>
								</tr>
							</table>
						</div>
					</div>
				</div>
			</asp:Panel>
			<asp:Panel ID="ChooseDb" runat="server" Visible="false">
				<p class="gsp_h2">
					<asp:Literal ID="l21" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_ChooseDb_Hdr %>" />
				</p>
				<p>
					<asp:Literal ID="l22" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_ChooseDb_Dtl %>" />
				</p>
				<div style="padding-left: 20px; padding-top: 20px">
					<div style="padding-left: 20px; padding-top: 10px">
						<asp:Literal ID="l23" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_ChooseDb_Avail_Db_Label %>" />
						<asp:DropDownList ID="ddlDbList" runat="server" />
					</div>
					<p id="lblErrMsgChooseDb" runat="server" class="gsp_invisible" enableviewstate="false" />
				</div>
			</asp:Panel>
			<asp:Panel ID="DbRuntime" runat="server" Visible="false">
				<p class="gsp_h2">
					<asp:Literal ID="l27" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_DbRuntime_Hdr %>" />
				</p>
				<p id="lblErrMsgDbRuntime" runat="server" class="gsp_invisible" enableviewstate="false" />
				<p class="gsp_bold">
					<asp:Literal ID="l28" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_DbRuntime_Dtl1 %>" />
				</p>
				<p>
					<asp:Literal ID="l29" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_DbRuntime_Dtl2 %>" />
				</p>
				<p class="gsp_bold gsp_addtopmargin5">
					<asp:Literal ID="l30" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_DbRuntime_Sql_Auth_Hdr %>" />
				</p>
				<div class="gsp_addleftpadding5">
					<asp:RadioButtonList ID="rblDbRuntimeConnectType" runat="server">
					</asp:RadioButtonList>
					<div class="gsp_addtopmargin2">
						<table style="padding-left: 50px;">
							<tr>
								<td>
									<asp:Literal ID="l31" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_DbRuntime_Sql_Auth_UserName_Label %>" />
								</td>
								<td>
									<asp:TextBox ID="txtDbRuntimeUserName" runat="server" />&nbsp;<asp:CustomValidator
										ID="cvDbRuntimeSqlLogOn" runat="server" CssClass="msgwarning" Display="Dynamic"
										ErrorMessage="<%$ Resources:GalleryServerPro, Installer_DbRuntime_Sql_Auth_UserName_Val_Err %>"
										OnServerValidate="cvDbRuntimeSqlLogOn_ServerValidate" />
								</td>
							</tr>
							<tr>
								<td>
									<asp:Literal ID="l32" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_DbRuntime_Sql_Auth_Pwd_Label %>" />
								</td>
								<td>
									<asp:TextBox ID="txtDbRuntimePassword" runat="server" TextMode="Password" />&nbsp;<asp:CustomValidator
										ID="cvDbRuntimeSqlPassword" runat="server" CssClass="msgwarning" Display="Dynamic"
										ErrorMessage="<%$ Resources:GalleryServerPro, Installer_DbRuntime_Sql_Auth_Pwd_Val_Err %>"
										OnServerValidate="cvDbRuntimeSqlPassword_ServerValidate" />
								</td>
							</tr>
						</table>
					</div>
				</div>
				<p class="gsp_msgfriendly">
					<asp:Literal ID="l33" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_DbRuntime_Dtl3 %>" />
				</p>
			</asp:Panel>
			<asp:Panel ID="SetupOptions" runat="server" Visible="false">
				<p class="gsp_h2">
					<asp:Literal ID="l34" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_SetupOptions_Hdr %>" />
				</p>
				<p>
					<asp:Literal ID="l35" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_SetupOptions_Dtl1 %>" />
				</p>
				<div class="gsp_addleftpadding5 gsp_addtoppadding5">
					<asp:CheckBox ID="chkScriptMembership" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_SetupOptions_Script_Label %>"
						Checked="True" CssClass="gsp_bold"></asp:CheckBox>
					<p class="gsp_addleftpadding6">
						<asp:Literal ID="l36" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_SetupOptions_Dtl2 %>" />
					</p>
					<p class="gsp_addleftpadding6">
						<asp:Literal ID="l37" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_SetupOptions_Dtl3 %>" />
					</p>
					<p class="gsp_msgfriendly gsp_addleftpadding6">
						<asp:Literal ID="l38" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_SetupOptions_Dtl4 %>" />
					</p>
				</div>
			</asp:Panel>
			<asp:Panel ID="GsAdmin" runat="server" Visible="false">
				<p class="gsp_h2">
					<asp:Literal ID="l39" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_GsAdmin_Hdr %>" />
				</p>
				<p>
					<asp:Literal ID="l40" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_GsAdmin_Dtl1 %>" />
				</p>
				<p>
					<asp:Literal ID="l41" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_GsAdmin_Dtl2 %>" />
				</p>
				<asp:Panel ID="pnlMembershipWarning" runat="server">
				</asp:Panel>
				<p class="gsp_msgfriendly gsp_addleftpadding6" style="display: <%= (this.ProviderDb == GalleryServerPro.Business.ProviderDataStore.SqlServer ? "block;" : "none;") %>">
					<asp:Literal ID="l42" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_GsAdmin_Dtl3 %>" />
				</p>
				<div class="gsp_addtopmargin5">
					<table style="padding-left: 50px;" class="gsp_standardTable">
						<tr>
							<td class="gsp_nowrap">
								<asp:Literal ID="l43" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_GsAdmin_UserName_Label %>" />
							</td>
							<td>
								<asp:TextBox ID="txtGsAdminUserName" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_GsAdmin_UserName_Default %>"></asp:TextBox>
							</td>
							<td class="gsp_fs">
								<asp:RequiredFieldValidator ID="rfvGsAdminUserName" runat="server" ControlToValidate="txtGsAdminUsername"
									Enabled="true" Display="Dynamic" ErrorMessage="<%$ Resources:GalleryServerPro, Site_Field_Required_Text %>" />
							</td>
						</tr>
						<tr>
							<td class="gsp_nowrap">
								<asp:Literal ID="l44" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_GsAdmin_Pwd_Label %>" />
							</td>
							<td>
								<asp:TextBox ID="txtGsAdminPassword" runat="server" TextMode="Password" required></asp:TextBox>
							</td>
							<td class="gsp_fs">
								<asp:RegularExpressionValidator ID="regGsAdminPassword" runat="server" ControlToValidate="txtGsAdminPassword"
									Display="dynamic"></asp:RegularExpressionValidator>
							</td>
						</tr>
						<tr>
							<td class="gsp_nowrap">
								<asp:Literal ID="l45" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_GsAdmin_Pwd_Confirm_Label %>" />
							</td>
							<td>
								<asp:TextBox ID="txtGsAdminPasswordConfirm" runat="server" TextMode="Password" required></asp:TextBox>
							</td>
							<td class="gsp_fs">
								<asp:CompareValidator EnableClientScript="True" Enabled="True" ID="ComparePassword"
									runat="server" ControlToValidate="txtGsAdminPasswordConfirm" ControlToCompare="txtGsAdminPassword"
									CssClass="gsp_msgwarning" Display="Dynamic" ErrorMessage="<%$ Resources:GalleryServerPro, Admin_Manage_Users_Passwords_Not_Matching_Error %>" />
							</td>
						</tr>
						<tr>
							<td class="gsp_nowrap">
								<asp:Literal ID="l46" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_GsAdmin_Email_Label %>" />
							</td>
							<td>
								<asp:TextBox ID="txtGsAdminEmail" runat="server" />
							</td>
							<td>
							</td>
						</tr>
					</table>
				</div>
			</asp:Panel>
			<asp:Panel ID="ReadyToInstall" runat="server" Visible="false">
				<p id="lblReadyToInstallHeaderMsg" runat="server" class="gsp_h2" enableviewstate="false">
					<asp:Literal ID="l47" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_ReadyToInstall_Hdr %>" />
				</p>
				<p id="lblErrMsgReadyToInstall" runat="server" class="gsp_invisible" enableviewstate="false" />
				<p id="lblErrMsgReadyToInstallSql" runat="server" class="gsp_invisible" enableviewstate="false" />
				<p id="lblErrMsgReadyToInstallCallStack" runat="server" class="gsp_invisible" enableviewstate="false" />
				<p id="lblReadyToInstallDetailMsg" runat="server" enableviewstate="false">
					<asp:Literal ID="l48" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_ReadyToInstall_Dtl1 %>" />
				</p>
			</asp:Panel>
			<asp:Panel ID="Finished" runat="server" Visible="false">
				<p class="gsp_h2">
					<asp:Literal ID="l49" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_Finished_Hdr %>" />
				</p>
				<div>
					<div class="gsp_floatcontainer">
						<asp:Image ID="imgFinishedIcon" runat="server" AlternateText="" Style="float: left;
							padding-top: 0.6em;" />
						<p style="margin-left: 50px;">
							<asp:Literal ID="l61" runat="server" />
						</p>
					</div>
					<asp:Panel ID="pnlAppCouldNotBeRestarted" runat="server" Visible="false" CssClass="gsp_addtopmargin5">
						<img src="<%= Utils.GalleryRoot %>/images/info.gif" alt="" style="width: 16px;
							height: 16px; float: left; padding-top: 0.6em;" />
						<div style="margin-left: 25px;">
							<p>
								<asp:Label ID="lblCouldNotRestartAppMsg" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_Finished_CouldNotRestartApp_Msg %>" />
							</p>
						</div>
					</asp:Panel>
					<asp:Panel ID="pnlWebConfigNeedUpdating" runat="server" Visible="False" CssClass="gsp_addtopmargin5">
						<img src="<%= Utils.GalleryRoot %>/images/go_14x14.png" alt="" style="width: 14px;
							height: 14px; float: left; padding-top: 0.6em;" />
						<div style="margin-left: 25px;">
							<p>
								<span class="gsp_bold">Web.config: </span>
								<asp:Literal ID="l62" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_Finished_WebCfg_Need_Updating_Dtl %>" />
							</p>
							<pre class="gsp_msgfriendly">&lt;add name="<%= SQL_SERVER_CN_STRING_NAME%>" connectionString="<%= GetDbRuntimeConnectionString(true)%>" /&gt;</pre>
						</div>
					</asp:Panel>
					<asp:Panel ID="pnlInstallFileNeedsDeleting" runat="server" Visible="False" CssClass="gsp_addtopmargin5">
						<img src="<%= Utils.GalleryRoot %>/images/go_14x14.png" alt="" style="width: 14px;
							height: 14px; float: left; padding-top: 0.6em;" />
						<div style="margin-left: 25px;">
							<p>
								<asp:Label ID="lblInstallFileMsg" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_Finished_NeedToDeleteInstallTextFile_Msg %>" />
							</p>
						</div>
					</asp:Panel>
					<p class="gsp_msgattention gsp_addtopmargin5">
						<asp:Label ID="lblWizardDisableMsg" runat="server" />
					</p>
				</div>
			</asp:Panel>
		</div>
		<div id="wizFtr">
			<a href="http://www.techinfosystems.com" title='<asp:Literal ID="l56" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_Footer_TIS_Link_Tooltip %>" />'
				tabindex="-1" style="display: block; float: left;">
				<img src="<%= Utils.GalleryRoot %>/images/tis_logo.gif" alt="" style="width: 132px;
					height: 76px;" /></a>
			<p class="gsp_fss" style="margin: 50px auto auto 175px;">
				<asp:Literal ID="litVersion" runat="server" /></p>
			<div class="gsp_rightBottomAbsolute">
				<p class="gsp_minimargin">
					<asp:Button ID="btnPrevious" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_Previous_Button_Text %>"
						CausesValidation="false" OnClick="btnPrevious_Click" TabIndex="0" />&nbsp;
					<asp:Button ID="btnNext" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_Next_Button_Text %>"
						OnClick="btnNext_Click" TabIndex="0" /></p>
			</div>
		</div>
	</div>
</div>
