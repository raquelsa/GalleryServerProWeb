<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="errorlog.ascx.cs" Inherits="GalleryServerPro.Web.Pages.Admin.errorlog" %>
<%@ Register TagPrefix="ComponentArt" Namespace="ComponentArt.Web.UI" Assembly="ComponentArt.Web.UI" %>
<%@ Register Assembly="GalleryServerPro.WebControls" Namespace="GalleryServerPro.WebControls"
	TagPrefix="tis" %>
<%@ Register Src="../../Controls/popupinfo.ascx" TagName="popup" TagPrefix="uc1" %>
<div id="Div1" runat="server">
	<script type="text/javascript">

		function gd_onItemExpand(sender, e)
		{
			gd.select(e.get_item());
		}

		function deleteRow(rowId)
		{
			var row = gd.getItemFromClientId(rowId);
			var id = row.getMemberAt(0).Value;
			gd.deleteItem(row);
			Gsp.Gallery.DeleteAppError(id);
		}

	</script>
</div>
<div class="gsp_indentedContent">
	<asp:PlaceHolder ID="phAdminHeader" runat="server" />
	<div class="gsp_addpadding1">
		<p class="gsp_msgdark">
			<asp:Label ID="lbl1" runat="server" CssClass="gsp_bold" Text="<%$ Resources:GalleryServerPro, Admin_Settings_Apply_To_Label %>"
				EnableViewState="false" />&nbsp;<asp:Literal ID="Literal1" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_All_Galleries_User_Is_Admin_For_Label %>" /></p>
		<tis:wwErrorDisplay ID="wwMessage" runat="server" UserMessage="<%$ Resources:GalleryServerPro, Validation_Summary_Text %>"
			CellPadding="2" UseFixedHeightWhenHiding="False" Center="False" Width="500px">
		</tis:wwErrorDisplay>
		<p>
			<asp:Button ID="btnClearLog" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_Error_ClearLog_Lbl %>"
				OnClick="btnClearLog_Click" /></p>
		<div style="width: 98%; height: 100%;">
			<ComponentArt:Grid ID="gd" runat="server" RunningMode="Client" AllowPaging="true"
				SearchText="<%$ Resources:GalleryServerPro, Site_Grid_Search_Label %>" PageSize="10"
				CssClass="gdErr0Grid" ShowHeader="True" ShowSearchBox="true" AutoFocusSearchBox="true"
				SearchTextCssClass="gdErr0GridHeaderText" SearchOnKeyPress="True" HeaderCssClass="gdErr0GridHeader"
				FooterCssClass="gdErr0GridFooter" GroupingNotificationText="" AllowHorizontalScrolling="false"
				FillContainer="true" AllowTextSelection="true" EditOnClickSelectedItem="false">
				<ClientEvents>
					<ItemExpand EventHandler="gd_onItemExpand" />
				</ClientEvents>
				<Levels>
					<ComponentArt:GridLevel DataKeyField="AppErrorId" DataMember="AppErrors" AllowGrouping="False"
						TableHeadingClientTemplateId="" AllowReordering="false" RowCssClass="gdErr0Row"
						DataCellCssClass="gdErr0DataCell" HeadingCellCssClass="gdErr0HeadingCell" TableHeadingCssClass="gdErr0TableHeader"
						ShowTableHeading="false" HeadingCellHoverCssClass="gdErr0HeadingCellHover" HeadingCellActiveCssClass="gdErr0HeadingCellActive"
						HeadingRowCssClass="gdErr0HeadingRow" HeadingTextCssClass="gdErr0HeadingCellText"
						SortAscendingImageUrl="asc.gif" SortDescendingImageUrl="desc.gif" SortImageWidth="10"
						SortImageHeight="19" ShowSelectorCells="false" SelectedRowCssClass="gdErr0SelectedRow">
						<Columns>
							<ComponentArt:GridColumn DataField="AppErrorId" Visible="false" />
							<ComponentArt:GridColumn DataCellClientTemplateId="ctOptions" DataCellCssClass="gdErr0DataCellOptions"
								Width="25" FixedWidth="true" />
							<ComponentArt:GridColumn DataField="TimeStamp" HeadingText="<%$ Resources:GalleryServerPro, Admin_Error_Grid_Timestamp_Hdr %>"
								Width="175" DataCellCssClass="gdErr0DataCellTimestamp" />
							<ComponentArt:GridColumn DataField="GalleryId" Align="Center" DataCellClientTemplateId="ctGalleryId"
								DataCellCssClass="gdErr0DataCellGalleryId" Width="80" HeadingText="<%$ Resources:GalleryServerPro, Admin_Error_Grid_GalleryId_Header %>" />
							<ComponentArt:GridColumn DataField="ExceptionType" HeadingText="<%$ Resources:GalleryServerPro, Admin_Error_Grid_Type_Hdr %>" />
							<ComponentArt:GridColumn DataField="Message" TextWrap="true" HeadingText="<%$ Resources:GalleryServerPro, Admin_Error_Grid_Message_Hdr %>" />
						</Columns>
					</ComponentArt:GridLevel>
					<ComponentArt:GridLevel DataKeyField="Name" DataMember="AppErrorItems" AllowGrouping="False"
						ShowHeadingCells="false" TableHeadingClientTemplateId="ctErrDetails" AllowReordering="false"
						RowCssClass="gdErr1Row" HeadingCellCssClass="gdErr1HeadingCell" TableHeadingCssClass="gdErr1TableHeader"
						ShowTableHeading="true" HeadingCellHoverCssClass="gdErr1HeadingCellHover" HeadingCellActiveCssClass="gdErr1HeadingCellActive"
						HeadingRowCssClass="gdErr1HeadingRow" HeadingTextCssClass="gdErr1HeadingCellText"
						SortAscendingImageUrl="asc.gif" SortDescendingImageUrl="desc.gif" SortImageWidth="10"
						SortImageHeight="19">
						<Columns>
							<ComponentArt:GridColumn DataField="Name" DataCellCssClass="gdErr1Column1" />
							<ComponentArt:GridColumn DataField="Value" DataCellCssClass="gdErr1Column2" />
						</Columns>
					</ComponentArt:GridLevel>
				</Levels>
			</ComponentArt:Grid>
		</div>
	</div>
	<asp:PlaceHolder ID="phAdminFooter" runat="server" />
	<tis:PopupInfo ID="PopupInfo" runat="server" DialogControlId="dgPopup" DefaultDialogTitleCss="dg5ContentTitleCss"
		DefaultDialogBodyCss="dg5ContentBodyCss">
		<PopupInfoItems>
		</PopupInfoItems>
	</tis:PopupInfo>
	<uc1:popup ID="ucPopupContainer" runat="server" />
</div>
