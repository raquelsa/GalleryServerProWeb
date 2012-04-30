<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="videoaudioother.ascx.cs"
	Inherits="GalleryServerPro.Web.Pages.Admin.videoaudioother" %>
<%@ Register Src="../../Controls/popupinfo.ascx" TagName="popup" TagPrefix="uc1" %>
<%@ Import Namespace="GalleryServerPro.Web" %>
<%@ Register Assembly="GalleryServerPro.WebControls" Namespace="GalleryServerPro.WebControls"
	TagPrefix="tis" %>
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
			<asp:Literal ID="l1" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_VidAudOther_General_Hdr %>" />
		</p>
		<div class="gsp_addleftpadding6">
			<p>
				<asp:CheckBox ID="chkAutoStart" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_VidAudOther_AutoStart_Label %>" />
			</p>
			<p>
				<asp:Literal ID="l3" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_VidAudOther_VideoThumbnailPosition_Part1_Label %>" />&nbsp;
				<asp:TextBox ID="txtVideoThumbnailPosition" runat="server" CssClass="gsp_textcenter gsp_textbox_narrow" />&nbsp;<asp:RangeValidator
					ID="rvVideoThumbnailPosition" runat="server" Display="Dynamic" ControlToValidate="txtVideoThumbnailPosition"
					Type="Integer" MinimumValue="0" MaximumValue="86400" Text="<%$ Resources:GalleryServerPro, Admin_VidAudOther_Validation_VideoThumbnailPosition_Text %>" />
				&nbsp;<asp:Label ID="lblVideoThumbnailPositionPart2" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_VidAudOther_VideoThumbnailPosition_Part2_Label %>" /></p>
			<p class="gsp_bold gsp_addtopmargin5">
				<asp:Label ID="lblMediaCtrDim" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Media_Container_Dimensions_Label %>" />
			</p>
			<p class="gsp_addleftpadding5">
				<asp:Literal ID="l5" runat="server" Text="<%$ Resources: GalleryServerPro, Admin_VidAudOther_MediaContainer_Video_Lbl %>" />
				<asp:TextBox ID="txtVideoPlayerWidth" runat="server" CssClass="gsp_textcenter gsp_textbox_narrow" />&nbsp;<asp:RangeValidator
					ID="rvVideoPlayerWidth" runat="server" Display="Dynamic" ControlToValidate="txtVideoPlayerWidth"
					Type="Integer" MinimumValue="0" MaximumValue="10000" Text="<%$ Resources:GalleryServerPro, Validation_Int_0_To_10000_Text %>" />
				x&nbsp;
				<asp:TextBox ID="txtVideoPlayerHeight" runat="server" CssClass="gsp_textcenter gsp_textbox_narrow" />&nbsp;<asp:RangeValidator
					ID="rvVideoPlayerHeight" runat="server" Display="Dynamic" ControlToValidate="txtVideoPlayerHeight"
					Type="Integer" MinimumValue="0" MaximumValue="10000" Text="<%$ Resources:GalleryServerPro, Validation_Int_0_To_10000_Text %>" />
				<asp:Literal ID="l6" runat="server" Text="<%$ Resources: GalleryServerPro, Admin_VidAudOther_Pixels_Lbl %>" /></p>
			<p class="gsp_addleftpadding5">
				<asp:Literal ID="l7" runat="server" Text="<%$ Resources: GalleryServerPro, Admin_VidAudOther_MediaContainer_Audio_Lbl %>" />
				<asp:TextBox ID="txtAudioPlayerWidth" runat="server" CssClass="gsp_textcenter gsp_textbox_narrow" />&nbsp;<asp:RangeValidator
					ID="rvAudioPlayerWidth" runat="server" Display="Dynamic" ControlToValidate="txtAudioPlayerWidth"
					Type="Integer" MinimumValue="0" MaximumValue="10000" Text="<%$ Resources:GalleryServerPro, Validation_Int_0_To_10000_Text %>" />
				x&nbsp;
				<asp:TextBox ID="txtAudioPlayerHeight" runat="server" CssClass="gsp_textcenter gsp_textbox_narrow" />&nbsp;<asp:RangeValidator
					ID="rvAudioPlayerHeight" runat="server" Display="Dynamic" ControlToValidate="txtAudioPlayerHeight"
					Type="Integer" MinimumValue="0" MaximumValue="10000" Text="<%$ Resources:GalleryServerPro, Validation_Int_0_To_10000_Text %>" />
				<asp:Literal ID="l9" runat="server" Text="<%$ Resources: GalleryServerPro, Admin_VidAudOther_Pixels_Lbl %>" /></p>
		</div>
		<p class="admin_h3">
			<asp:Label ID="lblEncoderSettingsHdr" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_VidAudOther_EncoderSettings_Hdr %>" />
		</p>
		<p class="gsp_addleftmargin4">
			<asp:Literal ID="l19" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_VidAudOther_EncoderSettings_Status_Hdr %>" />&nbsp;<asp:Label
				ID="lblEncoderStatus" runat="server" /></p>
		<p class="gsp_addleftmargin4">
			<asp:Label ID="lblEncoderTimeout" runat="server" Text="<%$ Resources: GalleryServerPro, Admin_VidAudOther_EncoderTimeout_Lbl %>" />
			<asp:TextBox ID="txtMediaEncoderTimeoutMs" runat="server" /></p>
		<p class="gsp_addleftmargin4 gsp_va_tip">
			<asp:Literal ID="l20" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_VidAudOther_EncoderSettings_Tip %>" /></p>
		<ul id="gsp_encoderSettingsCtr">
		</ul>
		<p class="gsp_addleftmargin4">
			<a href="javascript:void(0);" id="gsp_addEncoderSetting">
				<asp:Literal ID="l4" runat="server" Text="<%$ Resources: GalleryServerPro, Admin_VidAudOther_AddEncoder_CmdText %>" /></a></p>
		<p runat="server" class="admin_h3">
			<asp:Literal ID="l8" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_VidAudOther_EncoderQueue_Hdr %>" />
			<span class="gsp_va_q_pl"><a id="gsp_removeCompletedEncodings" href="javascript:void();">
				<asp:Literal ID="l18" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_VidAudOther_EncoderQueue_RemoveComplete_Text %>" /></a></span>&nbsp;<img
					src="<%=Utils.GetUrl("/images/wait-squares.gif")%>" class="gsp_spinner" alt="" />
		</p>
		<p class="gsp_va_q_empty gsp_invisible">
			<asp:Literal ID="l10" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_VidAudOther_EncoderQueue_Empty_Msg %>" /></p>
		<table class="gsp_va_q_Ctr">
			<thead>
				<tr>
					<th>
					</th>
					<th>
						<asp:Literal ID="l11" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_VidAudOther_EncoderQueue_MediaObject_Hdr %>" />
					</th>
					<th>
						<asp:Literal ID="l12" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_VidAudOther_EncoderQueue_Status_Hdr %>" />
					</th>
					<th>
						<asp:Literal ID="l13" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_VidAudOther_EncoderQueue_DateAdded_Hdr %>" />
					</th>
					<th>
						<asp:Literal ID="l14" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_VidAudOther_EncoderQueue_EncodingStart_Hdr %>" />
					</th>
					<th>
						<asp:Literal ID="l15" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_VidAudOther_EncoderQueue_EncodingComplete_Hdr %>" />
					</th>
					<th>
						<asp:Literal ID="l16" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_VidAudOther_EncoderQueue_Duration_Hdr %>" />
					</th>
					<th class="gsp_va_q_sd">
						<asp:Literal ID="l17" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_VidAudOther_EncoderQueue_Result_Hdr %>" />
					</th>
				</tr>
			</thead>
			<tbody id="gsp_tblQueueBody">
			</tbody>
		</table>
	</div>
	<asp:HiddenField ID="hdnEncoderSettings" runat="server" ClientIDMode="Static" />
	<asp:HiddenField ID="hdnSourceAvailableFileExtensions" runat="server" ClientIDMode="Static" />
	<asp:HiddenField ID="hdnDestinationAvailableFileExtensions" runat="server" ClientIDMode="Static" />
	<asp:HiddenField ID="hdnQueueItems" runat="server" ClientIDMode="Static" />
	<tis:wwDataBinder ID="wwDataBinder" runat="server">
		<DataBindingItems>
			<tis:wwDataBindingItem ID="wbi1" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="AutoStartMediaObject" ControlId="chkAutoStart" BindingProperty="Checked"
				UserFieldName="<%$ Resources:GalleryServerPro, Admin_VidAudOther_AutoStart_Label %>" />
			<tis:wwDataBindingItem ID="wbi2" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="DefaultVideoPlayerWidth" ControlId="txtVideoPlayerWidth" UserFieldName="<%$ Resources:GalleryServerPro, Admin_VidAudOther_VideoPlayerWidth_Label %>" />
			<tis:wwDataBindingItem ID="wbi3" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="DefaultVideoPlayerHeight" ControlId="txtVideoPlayerHeight"
				UserFieldName="<%$ Resources:GalleryServerPro, Admin_VidAudOther_VideoPlayerHeight_Label %>" />
			<tis:wwDataBindingItem ID="wbi4" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="VideoThumbnailPosition" ControlId="txtVideoThumbnailPosition"
				UserFieldName="Video Thumbnail Position" />
			<tis:wwDataBindingItem ID="wbi5" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="MediaEncoderTimeoutMs" ControlId="txtMediaEncoderTimeoutMs"
				UserFieldName="<%$ Resources:GalleryServerPro, Admin_VidAudOther_EncoderTimeout_Lbl %>" />
			<tis:wwDataBindingItem ID="wbi6" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="DefaultAudioPlayerWidth" ControlId="txtAudioPlayerWidth" UserFieldName="<%$ Resources:GalleryServerPro, Admin_VidAudOther_AudioPlayerWidth_Label %>" />
			<tis:wwDataBindingItem ID="wbi7" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="DefaultAudioPlayerHeight" ControlId="txtAudioPlayerHeight"
				UserFieldName="<%$ Resources:GalleryServerPro, Admin_VidAudOther_AudioPlayerHeight_Label %>" />
		</DataBindingItems>
	</tis:wwDataBinder>
	<tis:PopupInfo ID="PopupInfo" runat="server" DialogControlId="dgPopup" DefaultDialogTitleCss="dg5ContentTitleCss"
		DefaultDialogBodyCss="dg5ContentBodyCss">
		<PopupInfoItems>
			<tis:PopupInfoItem ID="poi1" runat="server" ControlId="chkAutoStart" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_autoStartMediaObject_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_autoStartMediaObject_Bdy %>" />
			<tis:PopupInfoItem ID="poi2" runat="server" ControlId="lblVideoThumbnailPositionPart2"
				DialogTitle="<%$ Resources:GalleryServerPro, Cfg_VideoThumbnailPosition_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_VideoThumbnailPosition_Bdy %>" />
			<tis:PopupInfoItem ID="poi3" runat="server" ControlId="lblMediaCtrDim" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_MediaContainerDimensions_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_MediaContainerDimensions_Bdy %>" />
			<tis:PopupInfoItem ID="poi4" runat="server" ControlId="lblEncoderSettingsHdr" DialogTitle="<%$ Resources:GalleryServerPro, Admin_VidAudOther_EncoderSettings_Overview_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Admin_VidAudOther_EncoderSettings_Overview_Bdy %>" />
			<tis:PopupInfoItem ID="poi5" runat="server" ControlId="lblEncoderTimeout" DialogTitle="<%$ Resources:GalleryServerPro, Admin_VidAudOther_EncoderTimeout_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Admin_VidAudOther_EncoderTimeout_Bdy %>" />
		</PopupInfoItems>
	</tis:PopupInfo>
	<uc1:popup ID="ucPopupContainer" runat="server" />
	<asp:PlaceHolder ID="phAdminFooter" runat="server" />
</div>
<div runat="server">
	<script id='tmplEncoderSetting' type='text/x-jquery-tmpl'>
<li class="gsp_va_et">
<table class="gsp_encoderSettingRowCtr">
<tr><td style="white-space:nowrap;">
	<a href="javascript:void(0);" title='Delete' class="gsp_va_et_d_btn gsp_hoverLink"><img src='<%= Utils.GetUrl("images/componentart/grid/delete.png") %>' alt='Delete' /></a>
	<img src='<%= Utils.GetUrl("images/move_updown_16x16.png") %>' class="gsp_hoverLink" title='<%= EsMoveTooltip %>' alt='' />
	 <%= EsConvertString %> <select name="sltSourceFileExtension">{{tmpl(SourceAvailableFileExtensions, { fileExt: SourceFileExtension }) "#tmplEncoderFileExtensionOption"}}</select>
	 <%= EsToString %> <select name="sltDestinationFileExtension">{{tmpl(DestinationAvailableFileExtensions, { fileExt: DestinationFileExtension }) "#tmplEncoderFileExtensionOption"}}</select>
	 <%= EsFFmpegArgsString %> </td>
	<td width="100%"><input type="text" name="iptArgs" value="${EncoderArguments}" style="width:100%;" /></td></tr>
</table>
</li>
	</script>
	<script id='tmplQueue' type='text/x-jquery-tmpl'>
<tr class="${$item.getRowCssClass($item.data)}">
	<td><a href="javascript:void(0);" title='Remove from queue' class="gsp_va_qi_d_btn gsp_hoverLink" data-id="${MediaQueueId}" data-status="${Status}"><img src='<%= Utils.GetUrl("images/componentart/grid/delete.png") %>' alt='Delete' /></a></td>
	<td><a href="<%= Utils.GetUrl(PageId.mediaobject) %>&moid=${FKMediaObjectId}">${FKMediaObjectId}</a></td>
	<td>${Status}</td>
	<td>${$item.getDateAsFormattedString(DateAdded)}</td>
	<td>${$item.getDateAsFormattedString(DateConversionStarted)}</td>
	<td>${$item.getDateAsFormattedString(DateConversionCompleted)}</td>
	<td>${$item.getDuration()}</td>
	<td class="gsp_va_q_sd"><div>[<a class="gsp_va_q_sd_sa" href="javascript:void();"><%= EqExpandString %></a>] ${StatusDetail}</div></td>
</tr>
	</script>
	<script id="tmplEncoderFileExtensionOption" type="text/x-jquery-tmpl">
	<option {{if Value === $item.fileExt}}selected="selected"{{/if}} value="${Value}">${Text}</option>
	</script>
	<script>
		$(document).ready(gsp_videoAudioLoad());

		function gsp_videoAudioLoad() {
			//Globalize.culture("default");
			gsp_bindEncoderSettings();

			gsp_bindQueue();

			$("#gsp_addEncoderSetting").click(gsp_onAddEncoderRow);
			$("#gsp_removeCompletedEncodings").click(gsp_removeCompletedEncodings);
			$('form:first').submit(gsp_onSubmitForm);
		}

		function gsp_bindQueue() {
			var queueItems = $.parseJSON($("#hdnQueueItems").val(), true);

			if (queueItems.length == 0) {
				gsp_hideQueueTable();
				return;
			}
			// Generate the HTML from the template
			var tmplData = $("#tmplQueue").tmpl(queueItems, {
				getDateAsFormattedString: function (dateValue) {
					if (dateValue != null)
						return Globalize.format(dateValue, "MMM d h:mm tt");
					else
						return "";
				},
				getRowCssClass: function (data) {
					if ((data != null) && (data.Status == "Processing"))
						return "gsp_va_q_cr";
					else
						return "";
				},
				getDuration: function () {
					if (this.data.DateConversionStarted == null)
						return "";

					var dtEnd = (this.data.DateConversionCompleted != null ? this.data.DateConversionCompleted : new Date());
					return Globalize.format((dtEnd - this.data.DateConversionStarted) / 60000, "n1") + " <%= EqMinutesString %>";
				}
			});

			$("#gsp_tblQueueBody").html(tmplData); // Add the HTML to the page

			$(".gsp_va_q_Ctr tr:odd").addClass("gsp_va_q_Ctr_alt");

			gsp_bindDeleteBtn(tmplData);
			gsp_bindQueueStatus();
		}

		function gsp_hideQueueTable() {
			$(".gsp_va_q_Ctr").hide();
			$(".gsp_va_q_empty").show();
		}

		function gsp_bindDeleteBtn(tmplData) {
			$(".gsp_va_qi_d_btn", tmplData).click(function (e) {
				gsp_deleteQueueItems(jQuery.makeArray($(this).data("id")));
			});
		}

		function gsp_removeCompletedEncodings(e) {
			// Delete all media queue IDs with a status of 'Complete' or 'Error'
			var ids = $('.gsp_va_qi_d_btn').map(function () {
				var status = $(this).data("status");
				if ((status == "Complete") || (status == "Error"))
					return $(this).data("id");
				else
					return null;
			}).get();

			gsp_deleteQueueItems(ids);

			return false;
		}

		function gsp_deleteQueueItems(mediaQueueIds) {
			var onQueueItemDeleted = function (actionResult) {
				$(".gsp_spinner").hide();

				$.each(mediaQueueIds, function (idx, id) {
					$(".gsp_va_q_Ctr tr:has(a[data-id=" + id + "])").remove(); // Remove row from HTML table
				});

				if ($(".gsp_va_q_Ctr tbody tr").length == 0)
					gsp_hideQueueTable();
			};

			if ((mediaQueueIds == null) || (mediaQueueIds.length == 0))
				return;
			//debugger;
			$(".gsp_spinner").show();

			$.ajax({
				type: "POST",
				url: '<%= Utils.GetUrl("services/Gallery.asmx") %>/DeleteQueueItem',
				data: "{ 'mediaQueueIds': " + JSON.stringify(mediaQueueIds) + " }",
				contentType: "application/json; charset=utf-8",
				dataType: "json",
				success: function (response) { onQueueItemDeleted(response.d); },
				error: function (response) {
					$('body', document).html(response.responseText);
				}
			});
		}

		function gsp_bindEncoderSettings() {
			var encoderSettings = $.parseJSON($("#hdnEncoderSettings").val());

			// Get available file extensions and add as property on each encoder setting
			var sourceAvailableFileExtensions = $.parseJSON($("#hdnSourceAvailableFileExtensions").val());
			var destinationAvailableFileExtensions = $.parseJSON($("#hdnDestinationAvailableFileExtensions").val());

			$.each(encoderSettings, function (i, item) {
				item.SourceAvailableFileExtensions = sourceAvailableFileExtensions;
				item.DestinationAvailableFileExtensions = destinationAvailableFileExtensions;
			});

			var tmplData = $("#tmplEncoderSetting").tmpl(encoderSettings); // Generate the HTML from the template
			$("#gsp_encoderSettingsCtr").html(tmplData); // Add the HTML to the page

			$(".gsp_va_et_d_btn", tmplData).click(function () { $(this).parents("li").remove(); });

			$("#gsp_encoderSettingsCtr").sortable({
				axis: 'y'
			});
		}

		function gsp_onAddEncoderRow(e) {
			gsp_updateHiddenField();

			// Add an item to the encoder settings stored in the hidden field, then re-bind the template
			var encoderSetting = {};
			encoderSetting.SourceFileExtension = "";
			encoderSetting.DestinationFileExtension = "";
			encoderSetting.EncoderArguments = "";

			var encoderSettings = $.parseJSON($("#hdnEncoderSettings").val());
			encoderSettings.push(encoderSetting);
			var serializedStr = JSON.stringify(encoderSettings);
			$("#hdnEncoderSettings").val(serializedStr);

			gsp_bindEncoderSettings();

			return true;
		}

		function gsp_onSubmitForm(e) {
			gsp_updateHiddenField();
			//e.preventDefault();
			return true;
		}

		function gsp_bindQueueStatus() {
			// Remember height of status detail
			$(".gsp_va_q_Ctr").data("resultCellHeight", $("td.gsp_va_q_sd div").height());

			// Expand/collapse the status detail when clicked
			$('.gsp_va_q_sd_sa').toggle(
			function () {
				var parentEl = $(this).parent();
				parentEl.css("height", "100%");
				$(".gsp_va_q_sd_sa", parentEl).text("<%= EqCollapseString %>");
			},
			function () {
				var parentEl = $(this).parent();
				parentEl.css("height", $(".gsp_va_q_Ctr").data("resultCellHeight"));
				$(".gsp_va_q_sd_sa", parentEl).text("<%= EqExpandString %>");
			}
		);
		}

		function gsp_updateHiddenField() {
			// Convert data in encoder settings HTML to MediaEncoderSettings array, then serialize and store in hidden field,
			// where it will be accessed by server code after form submission.
			var encoderSettings = new Array();

			$("#gsp_encoderSettingsCtr li").each(function () {
				var sourceFileExt = $("select[name=sltSourceFileExtension]", $(this)).val();
				var destFileExt = $("select[name=sltDestinationFileExtension]", $(this)).val();
				var args = $("input[name=iptArgs]", $(this)).val();

				var encoderSetting = {};
				encoderSetting.SourceFileExtension = sourceFileExt;
				encoderSetting.DestinationFileExtension = destFileExt;
				encoderSetting.EncoderArguments = args;

				encoderSettings.push(encoderSetting);
			});

			var serializedStr = JSON.stringify(encoderSettings);
			$("#hdnEncoderSettings").val(serializedStr);
		}

	</script>
</div>
