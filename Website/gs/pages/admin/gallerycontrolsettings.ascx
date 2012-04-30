<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="gallerycontrolsettings.ascx.cs"
	Inherits="GalleryServerPro.Web.Pages.Admin.gallerycontrolsettings" %>
<%@ Register Src="../../controls/albumtreeview.ascx" TagName="albumtreeview" TagPrefix="uc1" %>
<%@ Register Src="../../Controls/popupinfo.ascx" TagName="popup" TagPrefix="uc1" %>
<%@ Register Assembly="GalleryServerPro.WebControls" Namespace="GalleryServerPro.WebControls"
	TagPrefix="tis" %>
<%@ Register Namespace="ComponentArt.Web.UI" Assembly="ComponentArt.Web.UI" TagPrefix="ComponentArt" %>
<%@ Import Namespace="GalleryServerPro.Web" %>
<div id="d1" runat="server">
	<script type="text/javascript">

		$(function ()
		{
			bindEvents();

			gsp_configControlsPageLoad();
		});

		function gsp_configControlsPageLoad()
		{
			// If the combobox objects aren't ready, wait 1/10 second and check again. When ready call gsp_configControls().
			if (typeof (cboDefaultAlbum) === 'undefined')
			{
				setTimeout('gsp_configControlsPageLoad()', 100);
			}
			else
			{
				gsp_configAllControls();
			}
		}

		function bindEvents()
		{
			$('#<%= rbViewModeMultiple.ClientID %>').click(gsp_configViewModeControls);
			$('#<%= rbViewModeSingle.ClientID %>').click(gsp_configViewModeControls);
			$('#<%= rbViewModeTreeview.ClientID %>').click(gsp_configViewModeControls);
			$('#<%= rbDefaultGallery.ClientID %>').click(gsp_configDefaultGalleryObjectControls);
			$('#<%= rbDefaultAlbum.ClientID %>').click(gsp_configDefaultGalleryObjectControls);
			$('#<%= rbDefaultMediaObject.ClientID %>').click(gsp_configDefaultGalleryObjectControls);
			$('#<%= chkOverride.ClientID %>').click(gsp_configOverridableControls);
			$('#<%= chkShowHeader.ClientID %>').click(gsp_configShowHeaderControls);
			$('#<%= chkShowMediaObjectToolbar.ClientID %>').click(gsp_configToolbarControls);
		}

		function gsp_configAllControls()
		{
			gsp_configViewModeControls();

			gsp_configDefaultGalleryObjectControls();

			gsp_configOverridableControls();
		}

		function gsp_configViewModeControls()
		{
			var rbViewModeTreeview = $('#<%= rbViewModeTreeview.ClientID %>');

			$('#<%= txtTreeviewNavigateUrl.ClientID %>').prop('disabled', (!rbViewModeTreeview.prop('checked') || rbViewModeTreeview.prop('disabled')));
		}

		function gsp_configDefaultGalleryObjectControls()
		{
			var rbDefaultAlbum = $('#<%= rbDefaultAlbum.ClientID %>');
			var rbDefaultMediaObject = $('#<%= rbDefaultMediaObject.ClientID %>');

			if (!rbDefaultAlbum.prop('checked') || rbDefaultAlbum.prop('disabled'))
				cboDefaultAlbum.disable();
			else
				cboDefaultAlbum.enable();

			$('#<%= txtDefaultMediaObjectId.ClientID %>').prop('disabled', (!rbDefaultMediaObject.prop('checked') || rbDefaultMediaObject.prop('disabled')));
		}

		function gsp_configOverridableControls()
		{
			var chkOverride = $('#<%= chkOverride.ClientID %>');
			var chkShowHeader = $('#<%= chkShowHeader.ClientID %>');

			var isOverrideInactive = (!chkOverride.prop('checked') || chkOverride.prop('disabled'));

			$('#<%= chkShowAlbumTreeViewForAlbum.ClientID %>').prop('disabled', isOverrideInactive);
			$('#<%= chkShowAlbumTreeViewForMO.ClientID %>').prop('disabled', isOverrideInactive);

			chkShowHeader.prop('disabled', isOverrideInactive);

			gsp_configShowHeaderControls();

			$('#<%= chkAllowAnonBrowsing.ClientID %>').prop('disabled', isOverrideInactive);
			$('#<%= chkShowActionMenu.ClientID %>').prop('disabled', isOverrideInactive);
			$('#<%= chkShowAlbumBreadcrumb.ClientID %>').prop('disabled', isOverrideInactive);
			$('#<%= chkShowMediaObjectNavigation.ClientID %>').prop('disabled', isOverrideInactive);
			$('#<%= chkShowMediaObjectIndexPosition.ClientID %>').prop('disabled', isOverrideInactive);
			$('#<%= chkShowMediaObjectTitle.ClientID %>').prop('disabled', isOverrideInactive);
			$('#<%= chkAutoPlaySlideshow.ClientID %>').prop('disabled', isOverrideInactive);
			$('#<%= chkShowMediaObjectToolbar.ClientID %>').prop('disabled', isOverrideInactive);

			gsp_configToolbarControls();
		}

		function gsp_configShowHeaderControls()
		{
			var chkShowHeader = $('#<%= chkShowHeader.ClientID %>');
			var isShowHeaderInactive = (!chkShowHeader.prop('checked') || chkShowHeader.prop('disabled'));

			$('#<%= txtGalleryTitle.ClientID %>').prop('disabled', isShowHeaderInactive);
			$('#<%= txtGalleryTitleUrl.ClientID %>').prop('disabled', isShowHeaderInactive);
			$('#<%= chkShowLogin.ClientID %>').prop('disabled', isShowHeaderInactive);
			$('#<%= chkShowSearch.ClientID %>').prop('disabled', isShowHeaderInactive);
		}

		function gsp_configToolbarControls()
		{
			var chkShowToolbar = $('#<%= chkShowMediaObjectToolbar.ClientID %>');
			var isShowToolbarInactive = (!chkShowToolbar.prop('checked') || chkShowToolbar.prop('disabled'));

			$('#<%= chkShowMetadataButton.ClientID %>').prop('disabled', isShowToolbarInactive);
			$('#<%= chkShowDownloadButton.ClientID %>').prop('disabled', isShowToolbarInactive);
			$('#<%= chkShowDownloadZipButton.ClientID %>').prop('disabled', isShowToolbarInactive);
			$('#<%= chkShowHighResButton.ClientID %>').prop('disabled', isShowToolbarInactive);
			$('#<%= chkShowPermalinkButton.ClientID %>').prop('disabled', isShowToolbarInactive);
			$('#<%= chkShowSlideShowButton.ClientID %>').prop('disabled', isShowToolbarInactive);
			$('#<%= chkShowMoveButton.ClientID %>').prop('disabled', isShowToolbarInactive);
			$('#<%= chkShowCopyButton.ClientID %>').prop('disabled', isShowToolbarInactive);
			$('#<%= chkShowRotateButton.ClientID %>').prop('disabled', isShowToolbarInactive);
			$('#<%= chkShowDeleteButton.ClientID %>').prop('disabled', isShowToolbarInactive);
		}

		function gsp_handleAlbumComboBoxSelection(sender, e)
		{
			$('#<%= rbDefaultAlbum.ClientID %>').prop('checked', true);
			cboDefaultAlbum.set_text(e.get_node().get_text());
			cboDefaultAlbum.collapse();
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
				EnableViewState="false" />&nbsp;<asp:Label ID="lblGalleryDescription" runat="server"
					EnableViewState="false" /></p>
		<tis:wwErrorDisplay ID="wwMessage" runat="server" UserMessage="<%$ Resources:GalleryServerPro, Validation_Summary_Text %>"
			CellPadding="2" UseFixedHeightWhenHiding="False" Center="False" Width="500px">
		</tis:wwErrorDisplay>
		<p class="admin_h3" style="margin-top: 0;">
			<asp:Label ID="lblViewMode" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_ViewMode_Hdr %>" />
		</p>
		<div class="gsp_addleftpadding6">
			<p>
				<asp:RadioButton ID="rbViewModeMultiple" runat="server" GroupName="grpViewMode" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_ViewMode_Multiple_Label %>" />
			</p>
			<div>
				<p>
					<asp:RadioButton ID="rbViewModeSingle" runat="server" GroupName="grpViewMode" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_ViewMode_Single_Label %>" /></p>
			</div>
			<p>
				<asp:RadioButton ID="rbViewModeTreeview" runat="server" GroupName="grpViewMode" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_ViewMode_Treeview_Label %>" />
			</p>
			<div class="gsp_addleftpadding10">
				<asp:Literal ID="lc" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_TreeviewNavUrl_Label %>" />
				<asp:TextBox ID="txtTreeviewNavigateUrl" runat="server" />
			</div>
		</div>
		<p class="admin_h3">
			<asp:Label ID="lblDefaultGalleryObject" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_Default_Gallery_Object_Hdr %>" />
		</p>
		<div class="gsp_addleftpadding6">
			<p>
				<asp:RadioButton ID="rbDefaultGallery" runat="server" GroupName="grpDefaultObject" /></p>
			<div>
				<p>
					<asp:RadioButton ID="rbDefaultAlbum" runat="server" GroupName="grpDefaultObject"
						Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_Default_Album_Label %>" /></p>
				<div class="gsp_addleftpadding10">
					<ComponentArt:ComboBox ID="cboDefaultAlbum" runat="Server" Width="575" Height="20"
						DropDownWidth="575" DropDownHeight="300" AutoHighlight="true" AutoComplete="true"
						AutoFilter="false" ItemCssClass="gsp_ddn-item" ItemHoverCssClass="gsp_ddn-item-hover"
						CssClass="gsp_cmb" TextBoxCssClass="gsp_txt" DropDownResizingMode="Corner" DropDownCssClass="gsp_ns gsp_ddn"
						DropDownContentCssClass="gsp_ddn-con" SelectedItemCssClass="gsp_ddn-item-hover"
						KeyboardEnabled="true" DropDownOffsetY="2">
						<DropDownContent>
							<uc1:albumtreeview ID="tvUC" runat="server" AllowMultiCheck="false" ClientOnTreeNodeSelectJavascriptFunctionName="gsp_handleAlbumComboBoxSelection" />
						</DropDownContent>
						<DropDownFooter>
							<div style="text-align: right;">
								<img src="<%= Utils.GetUrl("/images/componentart/combobox/bottom_right.gif") %>" alt="" /></div>
						</DropDownFooter>
					</ComponentArt:ComboBox>
				</div>
			</div>
			<p>
				<asp:RadioButton ID="rbDefaultMediaObject" runat="server" GroupName="grpDefaultObject"
					Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_Default_MediaObject_Label %>" />
			</p>
			<div class="gsp_addleftpadding10">
				<asp:Literal ID="lb" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_Default_MediaObjectId_Label %>" />
				<asp:TextBox ID="txtDefaultMediaObjectId" runat="server" />
			</div>
			<p class="gsp_addtopmargin10">
				<asp:CheckBox ID="chkAllowUrlOverride" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_AllowUrlOverride_Label %>" /></p>
		</div>
		<p class="admin_h3">
			<asp:Literal ID="l8" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_Behavior_Hdr %>" />
		</p>
		<div class="gsp_addleftpadding6">
			<p>
				<asp:CheckBox ID="chkOverride" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_AllowOverride_Label %>" />
			</p>
			<div class="gsp_addleftpadding6">
				<p>
					<asp:CheckBox ID="chkShowAlbumTreeViewForAlbum" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_ShowAlbumTreeViewForAlbum_Label %>" />
				</p>
				<p>
					<asp:CheckBox ID="chkShowAlbumTreeViewForMO" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_ShowAlbumTreeViewForMediaObject_Label %>" />
				</p>
				<p class="gsp_addtopmargin5">
					<asp:CheckBox ID="chkShowHeader" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_ShowHeader_Label %>" /></p>
				<div class="gsp_collapse gsp_addleftmargin5">
					<table class="gsp_standardTable">
						<tr>
							<td class="gsp_col1">
								<asp:Label ID="lblGalleryTitle" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_WebsiteTitle_Label %>" />
							</td>
							<td>
								<asp:TextBox ID="txtGalleryTitle" runat="server" CssClass="gsp_textbox" />
							</td>
						</tr>
						<tr>
							<td class="gsp_col1">
								<asp:Label ID="lblGalleryTitleUrl" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_WebsiteTitleUrl_Label %>" />
							</td>
							<td>
								<asp:TextBox ID="txtGalleryTitleUrl" runat="server" CssClass="gsp_textbox" />
							</td>
						</tr>
					</table>
					<p>
						<asp:CheckBox ID="chkShowLogin" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_ShowLogin_Label %>" /></p>
					<p>
						<asp:CheckBox ID="chkShowSearch" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_ShowSearch_Label %>" /></p>
				</div>
				<p class="gsp_addtopmargin5">
					<asp:CheckBox ID="chkAllowAnonBrowsing" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_AllowAnonymousBrowsing_Label %>" /></p>
				<p>
					<asp:CheckBox ID="chkShowActionMenu" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_ShowActionMenu_Label %>" /></p>
				<p>
					<asp:CheckBox ID="chkShowAlbumBreadcrumb" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_ShowAlbumBreadcrumb_Label %>" /></p>
				<p>
					<asp:CheckBox ID="chkShowMediaObjectNavigation" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_ShowMediaObjectNavigation_Label %>" /></p>
				<p>
					<asp:CheckBox ID="chkShowMediaObjectIndexPosition" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_ShowMediaObjectIndexPosition_Label %>" /></p>
				<p>
					<asp:CheckBox ID="chkShowMediaObjectTitle" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_ShowMediaObjectTitle_Label %>" /></p>
				<p>
					<asp:CheckBox ID="chkAutoPlaySlideshow" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_AutoPlaySlideShow_Label %>" />
				</p>
				<p class="gsp_addtopmargin5">
					<asp:CheckBox ID="chkShowMediaObjectToolbar" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_ShowMediaObjectToolbar_Label %>" /></p>
				<div class="gsp_addleftpadding6">
					<p>
						<asp:CheckBox ID="chkShowMetadataButton" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_ShowMetadataButton_Label %>" /></p>
					<p>
						<asp:CheckBox ID="chkShowDownloadButton" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_ShowDownloadButton_Label %>" /></p>
					<p>
						<asp:CheckBox ID="chkShowDownloadZipButton" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_ShowDownloadZipButton_Label %>" /></p>
					<p>
						<asp:CheckBox ID="chkShowHighResButton" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_ShowHighResButton_Label %>" /></p>
					<p>
						<asp:CheckBox ID="chkShowPermalinkButton" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_ShowPermalinkButton_Label %>" /></p>
					<p>
						<asp:CheckBox ID="chkShowSlideShowButton" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_ShowSlideShowButton_Label %>" /></p>
					<p>
						<asp:CheckBox ID="chkShowMoveButton" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_ShowMoveButton_Label %>" /></p>
					<p>
						<asp:CheckBox ID="chkShowCopyButton" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_ShowCopyButton_Label %>" /></p>
					<p>
						<asp:CheckBox ID="chkShowRotateButton" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_ShowRotateButton_Label %>" /></p>
					<p>
						<asp:CheckBox ID="chkShowDeleteButton" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Gallery_Control_Settings_ShowDeleteButton_Label %>" /></p>
				</div>
			</div>
		</div>
		<tis:PopupInfo ID="PopupInfo" runat="server" DialogControlId="dgPopup" DefaultDialogTitleCss="dg5ContentTitleCss"
			DefaultDialogBodyCss="dg5ContentBodyCss">
			<PopupInfoItems>
				<tis:PopupInfoItem ID="poi1" runat="server" ControlId="lblAdminPageHeader" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_GCS_Overview_Hdr %>"
					DialogBody="<%$ Resources:GalleryServerPro, Cfg_GCS_Overview_Bdy %>" />
				<tis:PopupInfoItem ID="poi2" runat="server" ControlId="lblViewMode" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_GCS_ViewMode_Hdr %>"
					DialogBody="<%$ Resources:GalleryServerPro, Cfg_GCS_ViewMode_Bdy %>" />
				<tis:PopupInfoItem ID="poi3" runat="server" ControlId="lblDefaultGalleryObject" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_GCS_DefaultGalleryObject_Hdr %>"
					DialogBody="<%$ Resources:GalleryServerPro, Cfg_GCS_DefaultGalleryObject_Bdy %>" />
				<tis:PopupInfoItem ID="poi4" runat="server" ControlId="chkAllowUrlOverride" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_GCS_AllowUrlOverride_Hdr %>"
					DialogBody="<%$ Resources:GalleryServerPro, Cfg_GCS_AllowUrlOverride_Bdy %>" />
				<tis:PopupInfoItem ID="poi5" runat="server" ControlId="chkOverride" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_GCS_OverrideSettings_Hdr %>"
					DialogBody="<%$ Resources:GalleryServerPro, Cfg_GCS_OverrideSettings_Bdy %>" />
				<tis:PopupInfoItem ID="poi6" runat="server" ControlId="chkShowAlbumTreeViewForAlbum" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowAlbumTreeViewForAlbum_Hdr %>"
					DialogBody="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowAlbumTreeViewForAlbum_Bdy %>" />
				<tis:PopupInfoItem ID="poi7" runat="server" ControlId="chkShowAlbumTreeViewForMO" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowAlbumTreeViewForMediaObject_Hdr %>"
					DialogBody="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowAlbumTreeViewForMediaObject_Bdy %>" />
				<tis:PopupInfoItem ID="poi8" runat="server" ControlId="chkShowHeader" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowHeader_Hdr %>"
					DialogBody="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowHeader_Bdy %>" />
				<tis:PopupInfoItem ID="poi9" runat="server" ControlId="lblGalleryTitle" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_GCS_GalleryTitle_Hdr %>"
					DialogBody="<%$ Resources:GalleryServerPro, Cfg_GCS_GalleryTitle_Bdy %>" />
				<tis:PopupInfoItem ID="poi20" runat="server" ControlId="lblGalleryTitleUrl" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_GCS_GalleryTitleUrl_Hdr %>"
					DialogBody="<%$ Resources:GalleryServerPro, Cfg_GCS_GalleryTitleUrl_Bdy %>" />
				<tis:PopupInfoItem ID="poi21" runat="server" ControlId="chkShowLogin" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowLogin_Hdr %>"
					DialogBody="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowLogin_Bdy %>" />
				<tis:PopupInfoItem ID="poi22" runat="server" ControlId="chkShowSearch" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowSearch_Hdr %>"
					DialogBody="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowSearch_Bdy %>" />
				<tis:PopupInfoItem ID="poi23" runat="server" ControlId="chkAllowAnonBrowsing" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_GCS_AllowAnonBrowsing_Hdr %>"
					DialogBody="<%$ Resources:GalleryServerPro, Cfg_GCS_AllowAnonBrowsing_Bdy %>" />
				<tis:PopupInfoItem ID="poi24" runat="server" ControlId="chkShowActionMenu" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowActionMenu_Hdr %>"
					DialogBody="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowActionMenu_Bdy %>" />
				<tis:PopupInfoItem ID="poi25" runat="server" ControlId="chkShowAlbumBreadcrumb" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowAlbumBreadcrumb_Hdr %>"
					DialogBody="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowAlbumBreadcrumb_Bdy %>" />
				<tis:PopupInfoItem ID="poi26" runat="server" ControlId="chkShowMediaObjectNavigation" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_GCS_showMediaObjectNavigation_Hdr %>"
					DialogBody="<%$ Resources:GalleryServerPro, Cfg_GCS_showMediaObjectNavigation_Bdy %>" />
				<tis:PopupInfoItem ID="poi27" runat="server" ControlId="chkShowMediaObjectIndexPosition" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowMediaObjectIndexPosition_Hdr %>"
					DialogBody="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowMediaObjectIndexPosition_Bdy %>" />
				<tis:PopupInfoItem ID="poi28" runat="server" ControlId="chkShowMediaObjectTitle" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowMediaObjectTitle_Hdr %>"
					DialogBody="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowMediaObjectTitle_Bdy %>" />
				<tis:PopupInfoItem ID="poi29" runat="server" ControlId="chkAutoPlaySlideshow" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_GCS_AutoPlaySlideshow_Hdr %>"
					DialogBody="<%$ Resources:GalleryServerPro, Cfg_GCS_AutoPlaySlideshow_Bdy %>" />
				<tis:PopupInfoItem ID="poi40" runat="server" ControlId="chkShowMediaObjectToolbar" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowMediaObjectToolbar_Hdr %>"
					DialogBody="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowMediaObjectToolbar_Bdy %>" />
				<tis:PopupInfoItem ID="poi41" runat="server" ControlId="chkShowMetadataButton" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowMetadataButton_Hdr %>"
					DialogBody="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowMetadataButton_Bdy %>" />
				<tis:PopupInfoItem ID="poi42" runat="server" ControlId="chkShowDownloadButton" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowDownloadButton_Hdr %>"
					DialogBody="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowDownloadButton_Bdy %>" />
				<tis:PopupInfoItem ID="poi43" runat="server" ControlId="chkShowDownloadZipButton" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowDownloadZipButton_Hdr %>"
					DialogBody="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowDownloadZipButton_Bdy %>" />
				<tis:PopupInfoItem ID="poi44" runat="server" ControlId="chkShowHighResButton" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowHighResButton_Hdr %>"
					DialogBody="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowHighResButton_Bdy %>" />
				<tis:PopupInfoItem ID="poi45" runat="server" ControlId="chkShowPermalinkButton" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowPermalinkButton_Hdr %>"
					DialogBody="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowPermalinkButton_Bdy %>" />
				<tis:PopupInfoItem ID="poi46" runat="server" ControlId="chkShowSlideShowButton" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowSlideShowButton_Hdr %>"
					DialogBody="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowSlideShowButton_Bdy %>" />
				<tis:PopupInfoItem ID="poi47" runat="server" ControlId="chkShowMoveButton" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowMoveButton_Hdr %>"
					DialogBody="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowMoveButton_Bdy %>" />
				<tis:PopupInfoItem ID="poi48" runat="server" ControlId="chkShowCopyButton" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowCopyButton_Hdr %>"
					DialogBody="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowCopyButton_Bdy %>" />
				<tis:PopupInfoItem ID="poi49" runat="server" ControlId="chkShowRotateButton" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowRotateButton_Hdr %>"
					DialogBody="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowRotateButton_Bdy %>" />
				<tis:PopupInfoItem ID="poi60" runat="server" ControlId="chkShowDeleteButton" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowDeleteButton_Hdr %>"
					DialogBody="<%$ Resources:GalleryServerPro, Cfg_GCS_ShowDeleteButton_Bdy %>" />
			</PopupInfoItems>
		</tis:PopupInfo>
		<uc1:popup ID="ucPopupContainer" runat="server" />
		<asp:PlaceHolder ID="phAdminFooter" runat="server" />
	</div>
</div>
