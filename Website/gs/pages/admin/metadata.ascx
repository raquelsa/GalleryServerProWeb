<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="metadata.ascx.cs" Inherits="GalleryServerPro.Web.Pages.Admin.metadata" %>
<%@ Register Src="../../Controls/popupinfo.ascx" TagName="popup" TagPrefix="uc1" %>
<%@ Import Namespace="GalleryServerPro.Business.Metadata" %>
<%@ Register Assembly="GalleryServerPro.WebControls" Namespace="GalleryServerPro.WebControls"
	TagPrefix="tis" %>
<div id="d1" runat="server">
	<div id="jqueryDoc" runat="server" visible="false">
		<script src="../../script/jquery-1.4.4.js" type="text/javascript" />
	</div>
	<script type="text/javascript">

		$(function ()
		{
			// Make checkbox list sortable and add code to update hidden input after each sort that stores the sequence
			$("#gsp_MetadataPropertiesContainer").sortable({
				axis: 'y',
				stop: function () { configControls(); }
			});

			$("#gsp_MetadataPropertiesContainer ul, #gsp_MetadataPropertiesContainer li").disableSelection();

			// When clicking checkbox, update hidden input with list of IDs of all checked items
			$("#gsp_MetadataPropertiesContainer input:checkbox").bind("click", function () { configControls(); });

			$("#<%= chkExtractMetadata.ClientID %>").bind("click", function () { configControls(); });
			$("#<%= chkEnableMetadata.ClientID %>").bind("click", function () { configControls(); });

			configControls();
		});

		function configControls()
		{
			var chkExtractMetadata = $('#<%= chkExtractMetadata.ClientID %>');
			$('#<%= chkExtractMetadataUsingWpf.ClientID %>').prop('disabled', (!chkExtractMetadata.prop('checked') || chkExtractMetadata.prop('disabled')));

			// Update hidden input 'gsp_seq' with the current sequence of metadata names (ex: "4|0|3|1| ... 37|38|39|")
			var str = "";
			$("#gsp_MetadataPropertiesContainer li input[type=hidden]").each(function ()
			{
				str = str + $(this).val() + "|";
			});
			$("#gsp_seq").val(str);

			// Update hidden input 'gsp_chk' with the enum values of all checked metadata names (ex: "4|0|3|1| ... 37|38|39|")
			str = "";
			$("#gsp_MetadataPropertiesContainer li input:checkbox:checked ~ input[type=hidden]").each(function ()
			{
				str = str + $(this).val() + "|";
			});
			$("#gsp_chk").val(str);

			var chkEnableMetadata = $('#<%= chkEnableMetadata.ClientID %>');
			var enableMetadata = (chkEnableMetadata.prop('checked') && !chkEnableMetadata.prop('disabled'));
			if (enableMetadata)
			{
				enableMetadataList();
			}
			else
			{
				disableMetadataList();
			}
		}

		function enableMetadataList()
		{
			$("#gsp_MetadataPropertiesContainer").sortable("enable")
			$("#gsp_MetadataPropertiesContainer li").hover(function () { $(this).addClass('gsp_highlight'); }, function () { $(this).removeClass('gsp_highlight'); });
			$("#gsp_MetadataPropertiesContainer").removeClass("gsp_disabledtext")
			$("#gsp_MetadataPropertiesContainer li input:checkbox").prop('disabled', false);
		}

		function disableMetadataList()
		{
			$("#gsp_MetadataPropertiesContainer").sortable("disable")
			$("#gsp_MetadataPropertiesContainer li").unbind('mouseenter mouseleave');
			$("#gsp_MetadataPropertiesContainer").addClass("gsp_disabledtext")
			$("#gsp_MetadataPropertiesContainer li input:checkbox").prop('disabled', true);
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
		<p class="admin_h3">
			<asp:Literal ID="l1" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Metadata_Options_Hdr %>" />
		</p>
		<p class="gsp_addtopmargin5">
			<asp:CheckBox ID="chkExtractMetadata" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Metadata_ExtractMetadata_Label %>" />
		</p>
		<p class="gsp_addleftmargin5">
			<asp:CheckBox ID="chkExtractMetadataUsingWpf" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Metadata_ExtractMetadataUsingWpf_Label %>" />
		</p>
		<p class="admin_h3">
			<asp:Literal ID="l11" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Metadata_AutoCaption_Hdr %>" />
		</p>
		<p class="gsp_addtopmargin5">
			<asp:Label ID="lblMediaObjectCaptionTemplate" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Metadata_MediaObjectCaptionTemplate_Label %>" />
		</p>
		<p class="gsp_collapse gsp_addleftpadding6">
			<asp:TextBox ID="txtMediaObjectCaptionTemplate" runat="server" CssClass="gsp_textbox" />
		</p>
		<p class="admin_h3">
			<asp:Literal ID="l12" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Metadata_GpsMapService_Hdr %>" />
		</p>
		<p class="gsp_addtopmargin5">
			<asp:Label ID="lblGpsMapUrlTemplate" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Metadata_GpsMapUrlTemplate_Label %>" />
		</p>
		<p class="gsp_collapse gsp_addleftpadding6">
			<asp:TextBox ID="txtGpsMapUrlTemplate" runat="server" CssClass="gsp_textbox" style="width:100%;" />
		</p>
		<p class="admin_h3">
			<asp:Literal ID="l2" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Metadata_Display_Settings_Hdr %>" />
		</p>
		<p>
			<asp:CheckBox ID="chkEnableMetadata" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Metadata_EnableMetadata_Label %>" />
		</p>
		<div class="gsp_addleftmargin5">
			<p class="gsp_bold addtopmargin10">
				<asp:Label ID="lblMetadataHdr" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Metadata_List_Hdr %>" /></p>
			<p>
				<asp:Label ID="lblMetadata" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Metadata_Display_Settings_Label %>" />
				<asp:Repeater ID="rptrMetadata" runat="server" EnableViewState="false">
					<HeaderTemplate>
						<input id="gsp_seq" name="gsp_seq" type="hidden" />
						<input id="gsp_chk" name="gsp_chk" type="hidden" />
						<ul id="gsp_MetadataPropertiesContainer">
					</HeaderTemplate>
					<ItemTemplate>
						<li class="gsp_ui-state-default">
							<input type="checkbox" <%# (bool)Eval("IsVisible") ? "checked" : "" %> /><span class="gsp_metadataName"><%# Eval("MetadataItem") %></span><input
								name="gsp_idx" type="hidden" value="<%# (int)(FormattedMetadataItemName)Eval("MetadataItem") %>" /></li>
					</ItemTemplate>
					<FooterTemplate>
						</ul>
					</FooterTemplate>
				</asp:Repeater>
			</p>
		</div>
	</div>
	<tis:wwDataBinder ID="wwDataBinder" runat="server" OnValidateControl="wwDataBinder_ValidateControl"
		OnBeforeUnbindControl="wwDataBinder_BeforeUnbindControl">
		<DataBindingItems>
			<tis:wwDataBindingItem ID="wbi1" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="ExtractMetadata" ControlId="chkExtractMetadata" BindingProperty="Checked"
				UserFieldName="<%$ Resources:GalleryServerPro, Admin_Metadata_ExtractMetadata_Label %>" />
			<tis:wwDataBindingItem ID="wbi2" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="ExtractMetadataUsingWpf" ControlId="chkExtractMetadataUsingWpf"
				BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServerPro, Admin_Metadata_ExtractMetadataUsingWpf_Label %>" />
			<tis:wwDataBindingItem ID="wbi3" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="MediaObjectCaptionTemplate" ControlId="txtMediaObjectCaptionTemplate"
				UserFieldName="<%$ Resources:GalleryServerPro, Admin_Metadata_MediaObjectCaptionTemplate_Label %>" />
			<tis:wwDataBindingItem ID="wbi4" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="EnableMetadata" ControlId="chkEnableMetadata" BindingProperty="Checked"
				UserFieldName="<%$ Resources:GalleryServerPro, Admin_Metadata_EnableMetadata_Label %>" />
			<tis:wwDataBindingItem ID="wbi5" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="GpsMapUrlTemplate" ControlId="txtGpsMapUrlTemplate"
				UserFieldName="<%$ Resources:GalleryServerPro, Admin_Metadata_GpsMapUrlTemplate_Label %>" />
		</DataBindingItems>
	</tis:wwDataBinder>
	<tis:PopupInfo ID="PopupInfo" runat="server" DialogControlId="dgPopup" DefaultDialogTitleCss="dg5ContentTitleCss"
		DefaultDialogBodyCss="dg5ContentBodyCss">
		<PopupInfoItems>
			<tis:PopupInfoItem ID="pi1" runat="server" ControlId="chkExtractMetadata" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_extractMetadata_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_extractMetadata_Bdy %>" />
			<tis:PopupInfoItem ID="pi2" runat="server" ControlId="chkExtractMetadataUsingWpf"
				DialogTitle="<%$ Resources:GalleryServerPro, Cfg_extractMetadataUsingWpf_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_extractMetadataUsingWpf_Bdy %>" />
			<tis:PopupInfoItem ID="pi3" runat="server" ControlId="chkEnableMetadata" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_enableMetadata_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_enableMetadata_Bdy %>" />
			<tis:PopupInfoItem ID="pi4" runat="server" ControlId="lblMetadata" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_metadataDisplaySettings_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_metadataDisplaySettings_Bdy %>" />
			<tis:PopupInfoItem ID="pi5" runat="server" ControlId="lblMediaObjectCaptionTemplate"
				DialogTitle="<%$ Resources:GalleryServerPro, Cfg_mediaObjectCaptionTemplate_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_mediaObjectCaptionTemplate_Bdy %>" />
			<tis:PopupInfoItem ID="pi6" runat="server" ControlId="lblGpsMapUrlTemplate"
				DialogTitle="<%$ Resources:GalleryServerPro, Cfg_gpsMapUrlTemplate_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_gpsMapUrlTemplate_Bdy %>" />
		</PopupInfoItems>
	</tis:PopupInfo>
	<uc1:popup ID="ucPopupContainer" runat="server" />
	<asp:PlaceHolder ID="phAdminFooter" runat="server" />
</div>
