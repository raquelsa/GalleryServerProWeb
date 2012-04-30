<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="upgrade.ascx.cs" Inherits="GalleryServerPro.Web.Pages.upgrade" %>
<%@ Import Namespace="GalleryServerPro.Web" %>
<link href="<%= Utils.GalleryRoot %>/styles/gallery.css" rel="stylesheet" type="text/css" />
<link href="<%= Utils.GalleryRoot %>/styles/ca_styles.css" rel="stylesheet" type="text/css" />
<div id="css" runat="server" visible="false">
	<link href="../styles/gallery.css" rel="stylesheet" type="text/css" />
</div>
<!-- Enable or disable this installer by changing the value of the ENABLE_SETUP hidden field to true or false. -->
<asp:HiddenField ID="ENABLE_SETUP" runat="server" Value="false" />
<div class="gsp_ns">
	<div id="wizInstall" class="gsp_rounded10">
		<div id="wizHdr" class="gsp_rounded10">
			<img src="<%= Utils.GalleryRoot %>/images/gsp_logo_313x75.png" style="width: 313px;
				height: 75px;" alt="Gallery Server Pro logo" />
		</div>
		<div id="wizCtnt" class="wizCtnt" runat="server">
			<asp:Panel ID="Welcome" runat="server" Visible="false">
				<p class="gsp_h2">
					<asp:Literal ID="l1" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_Upgrade_Welcome_Hdr %>" />
				</p>
				<p>
					<asp:Literal ID="l2" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_Upgrade_Welcome_Dtl1 %>" />
				</p>
				<p class="gsp_bold gsp_addleftmargin5 gsp_addtopmargin5">
					<img src="<%= Utils.GalleryRoot %>/images/go_14x14.png" alt="" style="width: 14px;
						height: 14px; padding-right: .5em;" />Database
				</p>
				<ul style="list-style-type: disc; margin-left: 3em;">
					<li>
						<asp:Literal ID="l6" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_Upgrade_Db_Items1 %>" />
					</li>
				</ul>
				<p class="gsp_bold gsp_addleftmargin5 gsp_addtopmargin5">
					<img src="<%= Utils.GalleryRoot %>/images/go_14x14.png" alt="" style="width: 14px;
						height: 14px; padding-right: .5em;" />web.config
				</p>
				<ul style="list-style-type: disc; margin-left: 3em;">
					<li>
						<asp:Literal ID="l2c" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_Upgrade_WebConfig_Imported_Items1 %>" />
					</li>
					<li>
						<asp:Literal ID="l2d" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_Upgrade_WebConfig_Imported_Items2 %>" />
					</li>
				</ul>
				<p class="gsp_bold gsp_addleftmargin5 gsp_addtopmargin5">
					<img src="<%= Utils.GalleryRoot %>/images/go_14x14.png" alt="" style="width: 14px;
						height: 14px; padding-right: .5em;" />Unused files deleted
				</p>
				<ul style="list-style-type: disc; margin-left: 3em;">
					<li>Files that are no longer needed are deleted from the bin directory: AjaxControlToolkit.dll,
						GalleryServerPro.Business.Wpf.dll, GalleryServerPro.Data.SQLite.dll, System.Data.SQLite.DLL,
						TechInfoSystems.TracingTools.dll, Microsoft.Practices.EnterpriseLibrary.Caching.dll,
						Microsoft.Practices.EnterpriseLibrary.Common.dll, Microsoft.Practices.ObjectBuilder.dll</li>
					<li class="gsp_bold gsp_msgattention gsp_addtopmargin4">ATTN: If you are using this
						gallery in an application that requires one or more of these files, make a copy
						of them now and restore them when the upgrade is complete.</li>
				</ul>
				<p class="gsp_bold gsp_addleftmargin5 gsp_addtopmargin5">
					<img src="<%= Utils.GalleryRoot %>/images/go_14x14.png" alt="" style="width: 14px;
						height: 14px; padding-right: .5em;" />galleryserverpro.config
				</p>
				<ul style="list-style-type: disc; margin-left: 3em;">
					<li>
						<asp:Literal ID="l4" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_Upgrade_GspConfig_Imported_Items1 %>" /></li>
				</ul>
			</asp:Panel>
			<asp:Panel ID="ReadyToUpgrade" runat="server" Visible="false">
				<p id="lblReadyToUpgradeHeaderMsg" runat="server" class="gsp_h2" enableviewstate="false">
					<asp:Label ID="lblReadyToUpgradeHdrMsg" runat="server" />
				</p>
				<p id="pReadyToUpgradeDetail1Msg" runat="server" class="gsp_addtopmargin5" enableviewstate="false">
					<asp:Image ID="imgReadyToUpgradeStatus" runat="server" Style="float: left; padding-right: 0.5em;"
						AlternateText="" />
					<asp:Label ID="lblReadyToUpgradeDetail1Msg" runat="server" />
					<asp:LinkButton ID="lbTryAgain" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_Upgrade_ReadyToUpgrade_Try_Again_Text %>"
						OnClick="lbTryAgain_Click" Visible="false" EnableViewState="false" />
				</p>
				<p id="pReadyToUpgradeDetail2Msg" runat="server" enableviewstate="false">
					<asp:Label ID="lblReadyToUpgradeDetail2Msg" runat="server" />
				</p>
				<asp:Panel ID="pnlReadyToUpgradeConfigInfo" runat="server" CssClass="gsp_addleftmargin5">
					<p class="gsp_bold gsp_addtopmargin5">
						<asp:Image ID="imgReadyToUpgradeDbStatus" runat="server" Width="14" Height="14" Style="padding-right: 0.5em;"
							AlternateText="" /><asp:Label ID="lblReadyToUpgradeDbHeader" runat="server" /></p>
					<p class="gsp_addleftmargin5">
						<asp:Label ID="lblReadyToUpgradeDbStatus" runat="server" />
					</p>
					<p class="gsp_bold gsp_addtopmargin5">
						<asp:Image ID="imgReadyToUpgradeWebConfigStatus" runat="server" Width="14" Height="14"
							Style="padding-right: 0.5em;" AlternateText="" />web.config</p>
					<p class="gsp_addleftmargin5">
						<asp:Label ID="lblReadyToUpgradeWebConfigStatus" runat="server" />
					</p>
					<p class="gsp_bold gsp_addtopmargin5">
						<asp:Image ID="imgReadyToUpgradeGspConfigStatus" runat="server" Width="14" Height="14"
							Style="padding-right: 0.5em;" AlternateText="" />galleryserverpro.config</p>
					<p class="gsp_addleftmargin5">
						<asp:Label ID="lblReadyToUpgradeGspConfigStatus" runat="server" />
					</p>
				</asp:Panel>
			</asp:Panel>
			<asp:Panel ID="ImportProfiles" runat="server" Visible="false">
				<p class="gsp_h2">
					<asp:Literal ID="l21" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_Upgrade_ImportProfiles_Hdr %>" />
				</p>
				<p>
					<asp:Literal ID="l22" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_Upgrade_ImportProfiles_Dtl1 %>" />
				</p>
				<p>
					<asp:Literal ID="l23" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_Upgrade_ImportProfiles_Dtl2 %>" />
				</p>
				<p class="gsp_msgattention" style="margin-top: 100px;">
					<asp:Literal ID="l24" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_Upgrade_ImportProfiles_Dtl3 %>" />
				</p>
				<pre class="gsp_msgfriendly">
&lt;profile defaultProvider="SqlProfileProvider"&gt;
 &lt;providers&gt;
   &lt;clear /&gt;
   &lt;add applicationName="Gallery Server Pro" connectionStringName="SQLiteDbConnection" name="SQLiteProfileProvider" type="GalleryServerPro.Data.SQLite.SQLiteProfileProvider" /&gt;
   &lt;add applicationName="Gallery Server Pro" connectionStringName="SqlServerDbConnection" name="SqlProfileProvider" type="System.Web.Profile.SqlProfileProvider" /&gt;
  &lt;/providers&gt;
 &lt;properties&gt;
  &lt;add name="ShowMediaObjectMetadata" defaultValue="false" type="String" allowAnonymous="false" /&gt;
  &lt;add name="UserAlbumId" defaultValue="0" type="Int32" allowAnonymous="false" /&gt;
  &lt;add name="EnableUserAlbum" defaultValue="true" type="String" allowAnonymous="false" /&gt;
 &lt;/properties&gt;
&lt;/profile&gt;
				</pre>
			</asp:Panel>
			<asp:Panel ID="MigrateToSqlCe" runat="server" Visible="false">
				<p class="gsp_h2">
					<asp:Literal ID="l25" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_Upgrade_MigrateToSqlCe_Hdr %>" />
				</p>
				<p>
					<asp:Literal ID="l26" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_Upgrade_MigrateToSqlCe_Dtl1 %>" />
				</p>
				<p>
					<asp:Literal ID="l27" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_Upgrade_MigrateToSqlCe_Dtl2 %>" />
				</p>
				<p class="gsp_bold gsp_msgfriendly">
					<asp:Literal ID="l28" runat="server" Text="<%$ Resources:GalleryServerPro, Installer_Upgrade_MigrateToSqlCe_Dtl3 %>" />
				</p>
			</asp:Panel>
			<asp:Panel ID="Finished" runat="server" Visible="false">
				<p class="gsp_h2">
					<asp:Label ID="lblFinishedHdrMsg" runat="server" />
				</p>
				<div>
					<asp:Image ID="imgFinishedIcon" runat="server" AlternateText="" Style="float: left;
						padding-top: 0.6em;" />
					<p style="margin-left: 50px; min-height: 50px;">
						<asp:Literal ID="l61" runat="server" />
					</p>
					<p class="gsp_bold">
						<asp:Image ID="imgFinishedDbStatus" runat="server" Style="padding-right: .5em;" AlternateText="" />Database</p>
					<p style="margin-left: 25px;">
						<asp:Label ID="lblFinishedDbStatus" runat="server" />
					</p>
					<p style="margin-left: 25px;">
						<asp:Label ID="lblFinishedDbSql" runat="server" Visible="false" />
					</p>
					<p class="gsp_bold">
						<asp:Image ID="imgFinishedWebConfigStatus" runat="server" Style="padding-right: .5em;"
							AlternateText="" />web.config</p>
					<p style="margin-left: 25px;">
						<asp:Label ID="lblFinishedWebConfigStatus" runat="server" />
					</p>
					<p class="gsp_bold gsp_addtopmargin5">
						<asp:Image ID="imgFinishedGspConfigStatus" runat="server" Style="padding-right: .5em;"
							AlternateText="" />galleryserverpro.config</p>
					<p style="margin-left: 25px;">
						<asp:Label ID="lblFinishedGspConfigStatus" runat="server" />
					</p>
					<p class="gsp_bold gsp_addtopmargin5">
						<asp:Image ID="imgFinishedProfilesStatus" runat="server" Style="padding-right: .5em;"
							AlternateText="" />Profiles</p>
					<p style="margin-left: 25px;">
						<asp:Label ID="lblFinishedProfilesStatus" runat="server" />
					</p>
					<p class="gsp_msgattention gsp_addtopmargin5">
						<asp:Label ID="lblWizardDisableMsg" runat="server" Visible="false" />
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
					<asp:Button ID="btnNext" runat="server" EnableViewState="false" Text="<%$ Resources:GalleryServerPro, Installer_Next_Button_Text %>"
						OnClick="btnNext_Click" TabIndex="0" /></p>
			</div>
		</div>
	</div>
</div>
