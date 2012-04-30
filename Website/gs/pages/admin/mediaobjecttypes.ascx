<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="mediaobjecttypes.ascx.cs"
	Inherits="GalleryServerPro.Web.Pages.Admin.mediaobjecttypes" %>
<%@ Register TagPrefix="ComponentArt" Namespace="ComponentArt.Web.UI" Assembly="ComponentArt.Web.UI" %>
<%@ Register Src="../../Controls/popupinfo.ascx" TagName="popup" TagPrefix="uc1" %>
<%@ Register Assembly="GalleryServerPro.WebControls" Namespace="GalleryServerPro.WebControls"
	TagPrefix="tis" %>
<%@ Import Namespace="GalleryServerPro.Web" %>

<script type="text/javascript">

	var chkAllCheckboxIsChecked = false;
	var sortDesc = false;

	function setGridCheck(columnNumber, checked)
	{
		var gridItem;
		var itemIndex = 0;
		chkAllCheckboxIsChecked = checked;
		while (gridItem = gdMimeTypes.Table.GetRow(itemIndex))
		{
			gridItem.SetValue(columnNumber, checked);
			itemIndex++;
		}

		gdMimeTypes.Render();
	} 
</script>

<div class="gsp_indentedContent">
	<asp:PlaceHolder ID="phAdminHeader" runat="server" />
	<div class="gsp_addpadding1">
		<p class="gsp_msgdark">
			<asp:Label ID="lbl1" runat="server" CssClass="gsp_bold" Text="<%$ Resources:GalleryServerPro, Admin_Settings_Apply_To_Label %>"
				EnableViewState="false" />&nbsp;<asp:Label ID="lblGalleryDescription" runat="server"
					EnableViewState="false" /></p>
		<tis:wwErrorDisplay ID="wwMessage" runat="server" CellPadding="2" UseFixedHeightWhenHiding="False"
			Center="False" Width="500px" />
		<p>
			<asp:CheckBox ID="chkAllowAll" runat="server" Text="<%$ Resources:GalleryServerPro, Admin_MimeTypes_AllowAll_Label %>" />
		</p>
		<div style="width: 98%; height: 100%;">
			<ComponentArt:Grid ID="gdMimeTypes" runat="server" RunningMode="Client" AllowPaging="true"
				SearchText="<%$ Resources:GalleryServerPro, Site_Grid_Search_Label %>" PageSize="500"
				CssClass="gd0Grid" ShowHeader="True" ShowSearchBox="True" SearchTextCssClass="gd0GridHeaderText"
				SearchOnKeyPress="True" HeaderCssClass="gd0GridHeader" FooterCssClass="gd0GridFooter"
				GroupingNotificationText="" AllowHorizontalScrolling="true" FillContainer="true">
				<Levels>
					<ComponentArt:GridLevel DataKeyField="FileExtension" DataMember="none" AllowGrouping="False"
						TableHeadingClientTemplateId="enabledHeader" AllowReordering="false" RowCssClass="gd0Row"
						DataCellCssClass="gd0DataCell" HeadingCellCssClass="gd0HeadingCell" TableHeadingCssClass="gd0TableHeader"
						ShowTableHeading="true" HeadingCellHoverCssClass="gd0HeadingCellHover" HeadingCellActiveCssClass="gd0HeadingCellActive"
						HeadingRowCssClass="gd0HeadingRow" HeadingTextCssClass="gd0HeadingCellText" SortAscendingImageUrl="asc.gif"
						SortDescendingImageUrl="desc.gif" SortImageWidth="10" SortImageHeight="19">
						<Columns>
							<ComponentArt:GridColumn DataField="AllowAddToGallery" ColumnType="checkBox" HeadingText="<%$ Resources:GalleryServerPro, Admin_MimeTypes_Grid_AllowAddToGallery_Header %>"
								AllowEditing="true" Width="100" Align="center" />
							<ComponentArt:GridColumn DataField="Extension" HeadingText="<%$ Resources:GalleryServerPro, Admin_MimeTypes_Grid_FileExtension_Header %>"
								Width="175" />
							<ComponentArt:GridColumn DataField="FullType" HeadingText="<%$ Resources:GalleryServerPro, Admin_MimeTypes_Grid_MimeType_Header %>"
								Width="600" />
						</Columns>
					</ComponentArt:GridLevel>
				</Levels>
			</ComponentArt:Grid>
		</div>
	</div>
	<tis:wwDataBinder ID="wwDataBinder" runat="server">
		<DataBindingItems>
			<tis:wwDataBindingItem ID="wbi1" runat="server" BindingSource="GallerySettingsUpdateable" BindingSourceMember="AllowUnspecifiedMimeTypes"
				ControlId="chkAllowAll" BindingProperty="Checked" />
		</DataBindingItems>
	</tis:wwDataBinder>
	<tis:PopupInfo ID="PopupInfo" runat="server" DialogControlId="dgPopup" DefaultDialogTitleCss="dg5ContentTitleCss"
		DefaultDialogBodyCss="dg5ContentBodyCss">
		<PopupInfoItems>
			<tis:PopupInfoItem ID="poi1" runat="server" ControlId="lblAdminPageHeader" DialogTitle="<%$ Resources:GalleryServerPro, Admin_MimeTypes_Overview_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Admin_MimeTypes_Overview_Bdy %>" />
			<tis:PopupInfoItem ID="poi2" runat="server" ControlId="chkAllowAll" DialogTitle="<%$ Resources:GalleryServerPro, Cfg_allowUnspecifiedMimeTypes_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Cfg_allowUnspecifiedMimeTypes_Bdy %>" />
		</PopupInfoItems>
	</tis:PopupInfo>
	<uc1:popup ID="ucPopupContainer" runat="server" />
	<asp:PlaceHolder ID="phAdminFooter" runat="server" />
</div>
