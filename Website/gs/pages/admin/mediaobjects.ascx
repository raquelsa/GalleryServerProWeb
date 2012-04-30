<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="mediaobjects.ascx.cs"
	Inherits="GalleryServerPro.Web.Pages.Admin.mediaobjects" %>
<%@ Register Src="../../Controls/popupinfo.ascx" TagName="popup" TagPrefix="uc1" %>
<%@ Register Assembly="GalleryServerPro.WebControls" Namespace="GalleryServerPro.WebControls"
	TagPrefix="tis" %>
<div id="d1" runat="server">
	<script type="text/javascript">

		Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(moPageLoad);

		function moPageLoad(sender, args) {
			configControls();
		}

		function chkExtractMetadata_Click() {
			configControls();
		}

		function chkEnableSlideShow_Click() {
			configControls();
		}

		function chkEnableGoZipDownload_Click() {
			configControls();
		}

		function configControls() {
			var txtSlideShowInterval = $get('<%= txtSlideShowInterval.ClientID %>');
			var chkEnableSlideShow = $get('<%= chkEnableSlideShow.ClientID %>');

			txtSlideShowInterval.disabled = (!chkEnableSlideShow.checked || chkEnableSlideShow.disabled);

			var chkEnableGoZipDownload = $get('<%= chkEnableGoZipDownload.ClientID %>');
			$get('<%= chkEnableAlbumZipDownload.ClientID %>').disabled = (!chkEnableGoZipDownload.checked || chkEnableGoZipDownload.disabled);
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
			<asp:Literal ID="l1" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_Storage_Hdr %>" />
		</p>
		<div class="gsp_addleftpadding6">
			<p class="gsp_bold">
				<asp:Literal ID="l2" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_Original_Storage_Label %>" />
			</p>
			<div class="gsp_addleftpadding6">
				<p class="gsp_addtopmargin5">
					<asp:Label ID="lblMediaPath" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_MoPath_Label %>" />
				</p>
				<p class="gsp_collapse">
					<asp:TextBox ID="txtMoPath" runat="server" CssClass="gsp_textbox" />
				</p>
				<p class="gsp_addtopmargin5">
					<asp:Literal ID="l4" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_CurrentMoPath_Label %>" />
				</p>
				<p class="gsp_addleftpadding6">
					<asp:Label ID="lblMoPath" runat="server" CssClass="gsp_msgfriendly" /></p>
				<p class="gsp_addtopmargin5">
					<asp:CheckBox ID="chkPathIsReadOnly" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_MoPathIsReadOnly_Label %>" />
				</p>
				<p class="gsp_addtopmargin5">
					<asp:CheckBox ID="chkSynchAlbumTitleAndDirectoryName" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_SynchAlbumTitleAndDirectoryName_Label %>" />
				</p>
			</div>
			<p class="gsp_bold gsp_addtopmargin10">
				<asp:Literal ID="l5" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_Thumbnail_Storage_Label %>" />
			</p>
			<div class="gsp_addleftpadding6">
				<p>
					<asp:Label ID="lblThumbCachePath" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_ThumbnailCachePath_Label %>" />
				</p>
				<p class="gsp_collapse">
					<asp:TextBox ID="txtThumbnailCachePath" runat="server" CssClass="gsp_textbox" />
				</p>
				<p class="gsp_addtopmargin5">
					<asp:Literal ID="l7" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_CurrentThumbnailCachePath_Label %>" />
				</p>
				<p class="gsp_addleftpadding6">
					<asp:Label ID="lblThumbnailCachePath" runat="server" CssClass="gsp_msgfriendly" /></p>
			</div>
			<p class="gsp_bold gsp_addtopmargin10">
				<asp:Literal ID="l8" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_Compressed_Storage_Label %>" />
			</p>
			<div class="gsp_addleftpadding6">
				<p>
					<asp:Label ID="lblOptCachePath" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_OptimizedCachePath_Label %>" />
				</p>
				<p class="gsp_collapse">
					<asp:TextBox ID="txtOptimizedCachePath" runat="server" CssClass="gsp_textbox" />
				</p>
				<p class="gsp_addtopmargin5">
					<asp:Literal ID="l10" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_CurrentOptimizedCachePath_Label %>" />
				</p>
				<p class="gsp_addleftpadding6">
					<asp:Label ID="lblOptimizedCachePath" runat="server" CssClass="gsp_msgfriendly" /></p>
			</div>
		</div>
		<p class="admin_h3">
			<asp:Literal ID="l12" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_Options_Hdr %>" />
		</p>
					<p class="gsp_addtopmargin5">
				<asp:CheckBox ID="chkShowOriginal" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_ShowOriginal_Label %>" />
			</p>
<p class="gsp_addtopmargin5">
			<asp:CheckBox ID="chkEnableMoDownload" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_EnableMoDownload_Label %>" />
		</p>
		<p class="gsp_addtopmargin5">
			<asp:CheckBox ID="chkEnableGoZipDownload" runat="server" onclick="chkEnableGoZipDownload_Click()" Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_EnableMoZipDownload_Label %>" />
		</p>
		<p class="gsp_addleftmargin5">
			<asp:CheckBox ID="chkEnableAlbumZipDownload" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_EnableAlbumZipDownload_Label %>" />
		</p>
		<p class="gsp_addtopmargin5">
			<asp:CheckBox ID="chkEnablePermalink" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_EnablePermalink_Label %>" />
		</p>
		<p class="gsp_addtopmargin5">
			<asp:CheckBox ID="chkEnableSlideShow" runat="server" onclick="chkEnableSlideShow_Click()"
				Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_EnableSlideShow_Label %>" />
		</p>
		<p class="gsp_addleftmargin10">
			<asp:Label ID="lblSlideShowInterval" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_SlideShowInterval_Label %>" />
			<asp:TextBox ID="txtSlideShowInterval" runat="server" />&nbsp;<asp:RangeValidator
				ID="rvSlideShowInterval" runat="server" Display="Dynamic" ControlToValidate="txtSlideShowInterval"
				Type="Integer" MinimumValue="1" MaximumValue="2147483647" Text="<%$ Resources:GalleryServerPro, Validation_Positive_Int_Text %>"
				ErrorMessage="<%$ Resources:GalleryServerPro, Validation_Positive_Int_Text %>" /></p>
		<p class="admin_h3">
			<asp:Literal ID="l11" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_TransitionEffects_Hdr %>" />
		</p>
		<div class="gsp_addleftpadding6">
			<p>
				<asp:Label ID="lblTransType" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_TransType_Label %>" />
				<asp:DropDownList ID="ddlTransType" runat="server" />
			</p>
			<p class="gsp_addtopmargin5">
				<asp:Label ID="lblTransDuration" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_TransDuration_Label %>" />
				<asp:TextBox ID="txtTransDuration" runat="server" />&nbsp;<asp:RangeValidator ID="rvTransDuration"
					runat="server" Display="Dynamic" ControlToValidate="txtTransDuration" Type="Double"
					CultureInvariantValues="true" MinimumValue=".000001" MaximumValue="2147483647"
					Text="<%$ Resources:GalleryServerPro, Validation_Positive_Double_Text %>" ErrorMessage="<%$ Resources:GalleryServerPro, Validation_Positive_Double_Text %>" />
			</p>
		</div>
		<p class="admin_h3">
			<asp:Literal ID="l17" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_Upload_Hdr %>" />
		</p>
		<div class="gsp_addleftpadding6">
			<p class="gsp_addtopmargin5">
				<asp:CheckBox ID="chkAllowAddLocalContent" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_AllowAddLocalContent_Label %>" />
			</p>
			<p>
				<asp:CheckBox ID="chkAllowAddExternalContent" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_AllowAddExternalContent_Label %>" />
			</p>
			<p class="gsp_addtopmargin5">
				<asp:Label ID="lblMaxUploadSize" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_MaxUploadSize_Label %>" />
				<asp:TextBox ID="txtMaxUploadSize" runat="server" />&nbsp;<asp:RangeValidator ID="rvMaxUploadSize"
					runat="server" Display="Dynamic" ControlToValidate="txtMaxUploadSize" Type="Integer"
					MinimumValue="0" MaximumValue="2147483647" Text="<%$ Resources:GalleryServerPro, Validation_Positive_Int_Text %>" />
			</p>
		</div>
		<p class="admin_h3">
			<asp:Literal ID="l14" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_Thumbnail_Hdr %>" />
		</p>
		<table class="gsp_addtopmargin5 gsp_standardTable">
			<tr>
				<td class="gsp_col1">
					<asp:Label ID="lblMaxThumbnailLength" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_MaxThumbnailLength_Label %>" />
				</td>
				<td>
					<asp:TextBox ID="txtMaxThumbnailLength" runat="server" />&nbsp;<asp:RangeValidator
						ID="rvMaxThumbnailLength" runat="server" Display="Dynamic" ControlToValidate="txtMaxThumbnailLength"
						Type="Integer" MinimumValue="10" MaximumValue="100000" Text="<%$ Resources:GalleryServerPro, Validation_Int_10_To_100000_Text %>" />
				</td>
			</tr>
			<tr>
				<td class="gsp_col1">
					<asp:Label ID="lblTitleDisplayLength" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_TitleDisplayLength_Label %>" />
				</td>
				<td>
					<asp:TextBox ID="txtTitleDisplayLength" runat="server" />&nbsp;<asp:RangeValidator
						ID="rvTitleDisplayLength" runat="server" Display="Dynamic" ControlToValidate="txtTitleDisplayLength"
						Type="Integer" MinimumValue="1" MaximumValue="100" Text="<%$ Resources:GalleryServerPro, Validation_Int_1_To_100_Text %>" />
				</td>
			</tr>
			<tr>
				<td class="gsp_col1">
					<asp:Label ID="lblThumbJpegQuality" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_ThumbnailJpegQuality_Label %>" />
				</td>
				<td>
					<asp:TextBox ID="txtThumbJpegQuality" runat="server" />&nbsp;<asp:RangeValidator
						ID="rvThumbJpegQuality" runat="server" Display="Dynamic" ControlToValidate="txtThumbJpegQuality"
						Type="Integer" MinimumValue="1" MaximumValue="100" Text="<%$ Resources:GalleryServerPro, Validation_Int_1_To_100_Text %>" />
					&nbsp;<asp:Label ID="l15" runat="server" CssClass="gsp_fs" Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_ThumbnailJpegQuality_Label2 %>" />
				</td>
			</tr>
			<tr>
				<td class="gsp_col1">
					<asp:Label ID="lblThumbFileNamePrefix" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_MediaObjects_ThumbFileNamePrefix_Label %>" />
				</td>
				<td>
					<asp:TextBox ID="txtThumbFileNamePrefix" runat="server" />
					<asp:RequiredFieldValidator ID="rfv1" runat="server" ControlToValidate="txtThumbFileNamePrefix"
						Display="Static" ErrorMessage="<%$ Resources:GalleryServerPro, Site_Field_Required_Text %>"
						ForeColor="" CssClass="gsp_msgfailure">
					</asp:RequiredFieldValidator>
				</td>
			</tr>
		</table>
	</div>
	<tis:wwDataBinder ID="wwDataBinder" runat="server" OnValidateControl="wwDataBinder_ValidateControl"
		OnBeforeUnbindControl="wwDataBinder_BeforeUnbindControl">
		<DataBindingItems>
			<tis:wwDataBindingItem ID="WwDataBindingItem1" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="MediaObjectPath" ControlId="txtMoPath" IsRequired="True" UserFieldName="<%$ Resources:GalleryServerPro, Admin_MediaObjects_MoPath_Label %>" />
			<tis:wwDataBindingItem ID="WwDataBindingItem1d" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="MediaObjectPathIsReadOnly" ControlId="chkPathIsReadOnly" BindingProperty="Checked"
				UserFieldName="<%$ Resources:GalleryServerPro, Admin_MediaObjects_MoPathIsReadOnly_Label %>" />
			<tis:wwDataBindingItem ID="WwDataBindingItem1b" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="SynchAlbumTitleAndDirectoryName" ControlId="chkSynchAlbumTitleAndDirectoryName"
				BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServerPro, Admin_MediaObjects_SynchAlbumTitleAndDirectoryName_Label %>" />
			<tis:wwDataBindingItem ID="WwDataBindingItem2" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="ThumbnailPath" ControlId="txtThumbnailCachePath" UserFieldName="<%$ Resources:GalleryServerPro, Admin_MediaObjects_ThumbnailCachePath_Label %>"
				IsRequired="false" />
			<tis:wwDataBindingItem ID="WwDataBindingItem3" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="OptimizedPath" ControlId="txtOptimizedCachePath" UserFieldName="<%$ Resources:GalleryServerPro, Admin_MediaObjects_OptimizedCachePath_Label %>"
				IsRequired="false" />
			<tis:wwDataBindingItem ID="WwDataBindingItem12" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="ThumbnailClickShowsOriginal" ControlId="chkShowOriginal" BindingProperty="Checked"
				UserFieldName="<%$ Resources:GalleryServerPro, Admin_MediaObjects_ShowOriginal_Label %>" />
			<tis:wwDataBindingItem ID="WwDataBindingItem9e" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="EnableMediaObjectDownload" ControlId="chkEnableMoDownload"
				BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServerPro, Admin_MediaObjects_EnableMoDownload_Label %>" />
			<tis:wwDataBindingItem ID="WwDataBindingItem9f" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="EnableGalleryObjectZipDownload" ControlId="chkEnableGoZipDownload"
				BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServerPro, Admin_MediaObjects_EnableMoZipDownload_Label %>" />
			<tis:wwDataBindingItem ID="WwDataBindingItem9j" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="EnableAlbumZipDownload" ControlId="chkEnableAlbumZipDownload"
				BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServerPro, Admin_MediaObjects_EnableMoZipDownload_Label %>" />
			<tis:wwDataBindingItem ID="WwDataBindingItem9g" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="EnablePermalink" ControlId="chkEnablePermalink" BindingProperty="Checked"
				UserFieldName="<%$ Resources:GalleryServerPro, Admin_MediaObjects_EnablePermalink_Label %>" />
			<tis:wwDataBindingItem ID="WwDataBindingItem9h" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="EnableSlideShow" ControlId="chkEnableSlideShow" BindingProperty="Checked"
				UserFieldName="<%$ Resources:GalleryServerPro, Admin_MediaObjects_EnableSlideShow_Label %>" />
			<tis:wwDataBindingItem ID="WwDataBindingItem9i" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="SlideshowInterval" ControlId="txtSlideShowInterval" UserFieldName="<%$ Resources:GalleryServerPro, Admin_MediaObjects_SlideShowInterval_Label %>" />
			<tis:wwDataBindingItem ID="WwDataBindingItem4" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="MediaObjectTransitionType" ControlId="ddlTransType" BindingProperty="SelectedValue"
				UserFieldName="<%$ Resources:GalleryServerPro, Admin_MediaObjects_TransType_Label %>" />
			<tis:wwDataBindingItem ID="WwDataBindingItem5" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="MediaObjectTransitionDuration" ControlId="txtTransDuration"
				UserFieldName="<%$ Resources:GalleryServerPro, Admin_MediaObjects_TransDuration_Label %>" />
			<tis:wwDataBindingItem ID="WwDataBindingItem6" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="MaxThumbnailLength" ControlId="txtMaxThumbnailLength" UserFieldName="<%$ Resources:GalleryServerPro, Admin_MediaObjects_MaxThumbnailLength_Label %>" />
			<tis:wwDataBindingItem ID="WwDataBindingItem7" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="MaxMediaObjectThumbnailTitleDisplayLength" ControlId="txtTitleDisplayLength"
				UserFieldName="<%$ Resources:GalleryServerPro, Admin_MediaObjects_TitleDisplayLength_Label %>" />
			<tis:wwDataBindingItem ID="WwDataBindingItem8" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="AllowAddLocalContent" ControlId="chkAllowAddLocalContent"
				BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServerPro, Admin_MediaObjects_AllowAddLocalContent_Label %>" />
			<tis:wwDataBindingItem ID="WwDataBindingItem9" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="AllowAddExternalContent" ControlId="chkAllowAddExternalContent"
				BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServerPro, Admin_MediaObjects_AllowAddExternalContent_Label %>" />
			<tis:wwDataBindingItem ID="WwDataBindingItem10" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="MaxUploadSize" ControlId="txtMaxUploadSize" UserFieldName="<%$ Resources:GalleryServerPro, Admin_MediaObjects_MaxUploadSize_Label %>" />
			<tis:wwDataBindingItem ID="WwDataBindingItem11" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="ThumbnailImageJpegQuality" ControlId="txtThumbJpegQuality"
				UserFieldName="<%$ Resources:GalleryServerPro, Admin_MediaObjects_ThumbnailJpegQuality_Label %>" />
			<tis:wwDataBindingItem ID="WwDataBindingItem4b" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="ThumbnailFileNamePrefix" ControlId="txtThumbFileNamePrefix"
				UserFieldName="<%$ Resources:GalleryServerPro, Admin_MediaObjects_ThumbFileNamePrefix_Label %>" />
		</DataBindingItems>
	</tis:wwDataBinder>
	<tis:PopupInfo ID="PopupInfo" runat="server" DialogControlId="dgPopup" DefaultDialogTitleCss="dg5ContentTitleCss"
		DefaultDialogBodyCss="dg5ContentBodyCss">
		<PopupInfoItems>
			<tis:PopupInfoItem ID="PopupInfoItem1" runat="server" ControlId="lblMediaPath" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_mediaObjectPath_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_mediaObjectPath_Bdy %>" />
			<tis:PopupInfoItem ID="PopupInfoItem1b" runat="server" ControlId="chkPathIsReadOnly"
				DialogTitle="<%$ Resources:GalleryServerPro, Cfg_mediaObjectPathIsReadOnly_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_mediaObjectPathIsReadOnly_Bdy %>" />
			<tis:PopupInfoItem ID="PopupInfoItem1c" runat="server" ControlId="chkSynchAlbumTitleAndDirectoryName"
				DialogTitle="<%$ Resources:GalleryServerPro, Cfg_synchAlbumTitleAndDirectoryName_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_synchAlbumTitleAndDirectoryName_Bdy %>" />
			<tis:PopupInfoItem ID="PopupInfoItem2" runat="server" ControlId="lblThumbCachePath"
				DialogTitle="<%$ Resources:GalleryServerPro, Cfg_thumbnailPath_Hdr %>" DialogBody="<%$ Resources:GalleryServerPro, Cfg_thumbnailPath_Bdy %>" />
			<tis:PopupInfoItem ID="PopupInfoItem3" runat="server" ControlId="lblOptCachePath"
				DialogTitle="<%$ Resources:GalleryServerPro, Cfg_optimizedPath_Hdr %>" DialogBody="<%$ Resources:GalleryServerPro, Cfg_optimizedPath_Bdy %>" />
			<tis:PopupInfoItem ID="PopupInfoItem13" runat="server" ControlId="chkShowOriginal"
				DialogTitle="<%$ Resources:GalleryServerPro, Cfg_thumbnailClickShowsOriginal_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_thumbnailClickShowsOriginal_Bdy %>" />
			<tis:PopupInfoItem ID="PopupInfoItem3e" runat="server" ControlId="chkEnableMoDownload"
				DialogTitle="<%$ Resources:GalleryServerPro, Cfg_enableMoDownload_Hdr %>" DialogBody="<%$ Resources:GalleryServerPro, Cfg_enableMoDownload_Bdy %>" />
			<tis:PopupInfoItem ID="PopupInfoItem3f" runat="server" ControlId="chkEnableGoZipDownload"
				DialogTitle="<%$ Resources:GalleryServerPro, Cfg_enableMoZipDownload_Hdr %>" DialogBody="<%$ Resources:GalleryServerPro, Cfg_enableMoZipDownload_Bdy %>" />
			<tis:PopupInfoItem ID="PopupInfoItem3i" runat="server" ControlId="chkEnableAlbumZipDownload"
				DialogTitle="<%$ Resources:GalleryServerPro, Cfg_enableAlbumZipDownload_Hdr %>" DialogBody="<%$ Resources:GalleryServerPro, Cfg_enableAlbumZipDownload_Bdy %>" />
			<tis:PopupInfoItem ID="PopupInfoItem3g" runat="server" ControlId="chkEnablePermalink"
				DialogTitle="<%$ Resources:GalleryServerPro, Cfg_enablePermalink_Hdr %>" DialogBody="<%$ Resources:GalleryServerPro, Cfg_enablePermalink_Bdy %>" />
			<tis:PopupInfoItem ID="PopupInfoItem3h" runat="server" ControlId="chkEnableSlideShow"
				DialogTitle="<%$ Resources:GalleryServerPro, Cfg_enableSlideShow_Hdr %>" DialogBody="<%$ Resources:GalleryServerPro, Cfg_enableSlideShow_Bdy %>" />
			<tis:PopupInfoItem ID="PopupInfoItem4" runat="server" ControlId="lblTransType" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_mediaObjectTransitionType_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_mediaObjectTransitionType_Bdy %>" />
			<tis:PopupInfoItem ID="PopupInfoItem5" runat="server" ControlId="lblTransDuration"
				DialogTitle="<%$ Resources:GalleryServerPro, Cfg_mediaObjectTransitionDuration_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_mediaObjectTransitionDuration_Bdy %>" />
			<tis:PopupInfoItem ID="PopupInfoItem6" runat="server" ControlId="chkAllowAddLocalContent"
				DialogTitle="<%$ Resources:GalleryServerPro, Cfg_AllowAddLocalContent_Hdr %>" DialogBody="<%$ Resources:GalleryServerPro, Cfg_AllowAddLocalContent_Bdy %>" />
			<tis:PopupInfoItem ID="PopupInfoItem7" runat="server" ControlId="chkAllowAddExternalContent"
				DialogTitle="<%$ Resources:GalleryServerPro, Cfg_AllowAddExternalContent_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_AllowAddExternalContent_Bdy %>" />
			<tis:PopupInfoItem ID="PopupInfoItem8" runat="server" ControlId="lblMaxUploadSize"
				DialogTitle="<%$ Resources:GalleryServerPro, Cfg_MaxUploadSize_Hdr %>" DialogBody="<%$ Resources:GalleryServerPro, Cfg_MaxUploadSize_Bdy %>" />
			<tis:PopupInfoItem ID="PopupInfoItem9" runat="server" ControlId="lblMaxThumbnailLength"
				DialogTitle="<%$ Resources:GalleryServerPro, Cfg_maxThumbnailLength_Hdr %>" DialogBody="<%$ Resources:GalleryServerPro, Cfg_maxThumbnailLength_Bdy %>" />
			<tis:PopupInfoItem ID="PopupInfoItem10" runat="server" ControlId="lblTitleDisplayLength"
				DialogTitle="<%$ Resources:GalleryServerPro, Cfg_maxMediaObjectThumbnailTitleDisplayLength_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_maxMediaObjectThumbnailTitleDisplayLength_Bdy %>" />
			<tis:PopupInfoItem ID="PopupInfoItem11" runat="server" ControlId="lblThumbJpegQuality"
				DialogTitle="<%$ Resources:GalleryServerPro, Cfg_thumbnailImageJpegQuality_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_thumbnailImageJpegQuality_Bdy %>" />
			<tis:PopupInfoItem ID="PopupInfoItem12" runat="server" ControlId="lblThumbFileNamePrefix"
				DialogTitle="<%$ Resources:GalleryServerPro, Cfg_thumbnailFileNamePrefix_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_thumbnailFileNamePrefix_Bdy %>" />
		</PopupInfoItems>
	</tis:PopupInfo>
	<uc1:popup ID="ucPopupContainer" runat="server" />
	<asp:PlaceHolder ID="phAdminFooter" runat="server" />
</div>
