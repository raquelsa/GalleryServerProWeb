<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="albumedittemplate.ascx.cs"
	Inherits="GalleryServerPro.Web.Controls.albumedittemplate" %>
<%@ Register Namespace="ComponentArt.Web.UI" Assembly="ComponentArt.Web.UI" TagPrefix="ComponentArt" %>
<%@ Register Src="popupinfo.ascx" TagName="popup" TagPrefix="uc1" %>
<%@ Register Assembly="GalleryServerPro.WebControls" Namespace="GalleryServerPro.WebControls"
	TagPrefix="tis" %>
<%@ Import Namespace="GalleryServerPro.Web" %>
<script type="text/javascript">

	Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(editAlbumTemplatePageLoad);

	var _cdrBeginDateId = '<% = cdrBeginDate.ClientObjectId %>';
	var _cdrEndDateId = '<% = cdrEndDate.ClientObjectId %>';
	var _txtTitleId = '<% = txtTitle.ClientID %>';

	function editAlbumTemplatePageLoad(sender, args) {
		gsp_configControlsPageLoad();
	}

	function gsp_configControlsPageLoad() {
		// If the calendar objects aren't ready, wait 1/10 second and check again.
		if ((typeof (cdrBeginDate) === 'undefined') || (typeof (cdrEndDate) === 'undefined')) {
			setTimeout('gsp_configControlsPageLoad()', 100);
		}
		else
			gsp_configControls();
	}

	function gsp_configControls() {
		Sys.Application.addComponent(cdrBeginDate);
		Sys.Application.addComponent(cdrEndDate);
		RenderExampleDate();
	}

	function RenderExampleDate() {
		var exDate = new Date();
		var exDateMsg = "<% = ExampleDatePrefix %>" + exDate.localeFormat(_dateFormatString) + "<% = ExampleDateSuffix %>";
		$get('bdEx').appendChild(document.createTextNode(exDateMsg));
		$get('edEx').appendChild(document.createTextNode(exDateMsg));
	}

	function beginDate_OnClick(alignElement, calendar) {
		if (calendar.get_popUpShowing()) {
			calendar.hide();
		}
		else {
			var beginDate = Date.parseLocale($get('beginDate').value), _dateFormatString;
			if (beginDate == null)
				beginDate = new Date();

			calendar.setSelectedDate(beginDate);
			calendar.show(alignElement);
		}
	}

	function cdrBeginDate_onSelectionChanged(sender, eventArgs) {
		_beginDate = new Date(sender.getSelectedDate());
		$get('beginDate').value = _beginDate.localeFormat(_dateFormatString);
	}

	function endDate_OnClick(alignElement, calendar) {
		if (calendar.get_popUpShowing()) {
			calendar.hide();
		}
		else {
			var endDate = Date.parseLocale($get('endDate').value), _dateFormatString;
			if (endDate == null)
				endDate = new Date();

			calendar.setSelectedDate(endDate);
			calendar.show(alignElement);
		}
	}

	function cdrEndDate_onSelectionChanged(sender, eventArgs) {
		_endDate = new Date(sender.getSelectedDate());
		$get('endDate').value = _endDate.localeFormat(_dateFormatString);
	}

	function closeEditDialog() {
		if (!Page_ClientValidate()) {
			// Force the error popup to close by putting a dummy value in the album title textbox and revalidating
			$get(_txtTitleId).value = '1';
			Page_ClientValidate();
		}
		dgEditAlbum.close();
	}
</script>
<table class='gsp_standardTable gsp_addpadding1' onkeypress="javascript:return WebForm_FireDefaultButton(event, 'btnSave')">
	<tr>
		<td class='gsp_bold gsp_nowrap gsp_col1' style="vertical-align: middle;">
			<asp:Label ID="lblTitle" runat="server" AssociatedControlID="txtTitle" Text="<%$ Resources:GalleryServerPro, UC_Album_Header_Edit_Album_Title_Header_Text %>" />
		</td>
		<td>
			<asp:TextBox ID="txtTitle" runat="server" CssClass="gsp_textbox" Text="1" /><br />
			<asp:RequiredFieldValidator ID="rfv1" runat="server" ControlToValidate="txtTitle"
				Display="Dynamic" ErrorMessage="<%$ Resources:GalleryServerPro, Task_Create_Album_Missing_Title_Text %>"
				ForeColor="" CssClass="gsp_msgwarning">
			</asp:RequiredFieldValidator>
		</td>
		<td class='gsp_fs' style="vertical-align: middle;">
			<asp:Label ID="lblMaxTitleLengthInfo" runat="server" AssociatedControlID="txtTitle" />
		</td>
	</tr>
	<tr>
		<td class='gsp_bold gsp_aligntop gsp_col1'>
			<label for="albumSummary">
				<asp:Literal ID="l1" runat="server" Text="<%$ Resources:GalleryServerPro, UC_Album_Header_Edit_Album_Summary_Header_Text %>" /></label>
		</td>
		<td colspan="2">
			<textarea id='albumSummary' rows='12' cols='70' class='gsp_textarea2'>&nbsp;</textarea>
		</td>
	</tr>
	<tr>
		<td class='gsp_bold gsp_nowrap gsp_aligntop gsp_col1'>
			<p style="padding-top: 2px;">
				<asp:Literal ID="l2" runat="server" Text="<%$ Resources:GalleryServerPro, UC_Album_Header_Edit_Album_Owner_Header_Text %>" /></p>
		</td>
		<td colspan="2">
			<asp:MultiView ID="mvOwner" runat="server">
				<asp:View ID="vwOwnerEditor" runat="server">
					<ComponentArt:ComboBox ID="cboOwner" runat="Server" RunningMode="CallBack" Width="292"
						Height="20" DataTextField="UserName" DataValueField="UserName" AutoHighlight="true"
						AutoComplete="true" AutoFilter="true" KeyboardEnabled="true" DropDownWidth="290"
						DropDownHeight="300" DropDownOffsetY="2" DropDownPageSize="100" ItemCssClass="gsp_ddn-item"
						ItemHoverCssClass="gsp_ddn-item-hover" CssClass="gsp_cmb" TextBoxCssClass="gsp_txt"
						DropDownResizingMode="Corner" DropDownCssClass="gsp_ns gsp_ddn" DropDownContentCssClass="gsp_ddn-con"
						SelectedItemCssClass="gsp_ddn-item-hover">
						<DropDownFooter>
							<div style="text-align: right;">
								<img src="<%= Utils.GetUrl("/images/componentart/combobox/bottom_right.gif") %>" alt="" /></div>
						</DropDownFooter>
					</ComponentArt:ComboBox>
				</asp:View>
				<asp:View ID="vwOwnerReadOnly" runat="server">
					<p class="gsp_msgfriendly">
						<asp:Literal ID="litOwnerReadOnly" runat="server" /></p>
				</asp:View>
			</asp:MultiView>
			<asp:Panel ID="pnlInheritedOwner" runat="server" Visible="false">
				<p class="gsp_em">
					<asp:Literal ID="l5" runat="server" Text="<%$ Resources:GalleryServerPro, UC_Album_Header_Edit_Album_Inherited_Owner_Label %>" />&nbsp;<asp:Literal
						ID="litInheritedOwner" runat="server" /></p>
			</asp:Panel>
		</td>
	</tr>
	<tr>
		<td class='gsp_bold gsp_nowrap gsp_col1' style="vertical-align: middle;">
			<label for="beginDate">
				<asp:Literal ID="l3" runat="server" Text="<%$ Resources:GalleryServerPro, UC_Album_Header_Edit_Album_Begin_Date_Header_Text %>" /></label>
		</td>
		<td colspan="2">
			<input id='beginDate' type='text' />&nbsp;<img alt="" style="vertical-align: bottom;"
				onclick="beginDate_OnClick(this, <%= cdrBeginDate.ClientObjectId %>)" class="gsp_calendar_button"
				src="<%= CalendarIconUrl %>" width="25" height="22" />
			<ComponentArt:Calendar runat="server" ID="cdrBeginDate" AllowMultipleSelection="false"
				AllowWeekSelection="false" AllowMonthSelection="false" ControlType="Calendar" PopUp="Custom"
				CalendarTitleCssClass="title" DayHeaderCssClass="dayheader" DayCssClass="day" DayHoverCssClass="dayhover"
				OtherMonthDayCssClass="othermonthday" SelectedDayCssClass="selectedday" CalendarCssClass="gsp_calendar gsp_ns"
				NextPrevCssClass="nextprev" NextPrevHoverCssClass="nextprevhover" MonthCssClass="month"
				SwapSlide="Linear" SwapDuration="300" DayNameFormat="FirstTwoLetters" PrevImageUrl="cal_prevMonth.gif"
				NextImageUrl="cal_nextMonth.gif" ReactOnSameSelection="true">
				<ClientEvents>
					<SelectionChanged EventHandler="cdrBeginDate_onSelectionChanged" />
				</ClientEvents>
			</ComponentArt:Calendar>
			<span id="bdEx" class="gsp_fs gsp_em"></span>
		</td>
	</tr>
	<tr>
		<td class='gsp_bold gsp_nowrap gsp_col1' style="vertical-align: middle;">
			<label for="endDate">
				<asp:Literal ID="l4" runat="server" Text="<%$ Resources:GalleryServerPro, UC_Album_Header_Edit_Album_End_Date_Header_Text %>" /></label>
		</td>
		<td colspan="2">
			<input id='endDate' type='text' />&nbsp;<img alt="" style="vertical-align: bottom;"
				onclick="endDate_OnClick(this, <%= cdrEndDate.ClientObjectId %>)" class="gsp_calendar_button"
				src="<%= CalendarIconUrl %>" width="25" height="22" />
			<ComponentArt:Calendar runat="server" ID="cdrEndDate" AllowMultipleSelection="false"
				AllowWeekSelection="false" AllowMonthSelection="false" ControlType="Calendar" PopUp="Custom"
				CalendarTitleCssClass="title" DayHeaderCssClass="dayheader" DayCssClass="day" DayHoverCssClass="dayhover"
				OtherMonthDayCssClass="othermonthday" SelectedDayCssClass="selectedday" CalendarCssClass="gsp_calendar gsp_ns"
				NextPrevCssClass="nextprev" NextPrevHoverCssClass="nextprevhover" MonthCssClass="month"
				SwapSlide="Linear" SwapDuration="300" DayNameFormat="FirstTwoLetters" PrevImageUrl="cal_prevMonth.gif"
				NextImageUrl="cal_nextMonth.gif" ReactOnSameSelection="true">
				<ClientEvents>
					<SelectionChanged EventHandler="cdrEndDate_onSelectionChanged" />
				</ClientEvents>
			</ComponentArt:Calendar>
			<span id="edEx" class="gsp_fs gsp_em"></span>
		</td>
	</tr>
	<tr>
		<td>
		</td>
		<td>
			<img src="<%= Utils.GalleryRoot %>/images/lock_24x24.png" style="width: 24px; height: 24px;
				vertical-align: bottom;" alt="" /><input id="private" type="checkbox" /><label for="private"><asp:Literal
					ID="l6" runat="server" Text="<%$ Resources:GalleryServerPro, UC_Album_Header_Edit_Album_Is_Private_Text %>" /></label>
			<asp:Label ID="lblPrivateAlbumIsInherited" runat="server" CssClass="gsp_fs gsp_em" />
		</td>
	</tr>
</table>
<div class='gsp_okCancelContainer'>
	<input id="btnSave" type='button' value='<asp:Literal runat="server" Text="<%$ Resources:GalleryServerPro, UC_Album_Header_Edit_Album_Ok_Button_Text%>" />'
		onclick="if(Page_ClientValidate()) saveAlbumInfo()" />&nbsp;<input onclick="closeEditDialog()"
			type='button' value='<asp:Literal runat="server" Text="<%$ Resources:GalleryServerPro, UC_Album_Header_Edit_Album_Cancel_Button_Text%>" />' />
</div>
<tis:PopupInfo ID="PopupInfo" runat="server" DialogControlId="dgPopup" DefaultDialogTitleCss="dg5ContentTitleCss"
	DefaultDialogBodyCss="dg5ContentBodyCss">
	<PopupInfoItems>
		<tis:PopupInfoItem ID="PopupInfoItem1" runat="server" ControlId="lblPrivateAlbumIsInherited"
			DialogTitle="<%$ Resources:GalleryServerPro, UC_Album_Header_Edit_Album_Is_Private_Info_Hdr %>"
			DialogBody="<%$ Resources:GalleryServerPro, UC_Album_Header_Edit_Album_Is_Private_Info_Bdy %>" />
	</PopupInfoItems>
</tis:PopupInfo>
<uc1:popup ID="ucPopupContainer" runat="server" />
