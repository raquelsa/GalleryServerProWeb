<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="synchronize.ascx.cs"	Inherits="GalleryServerPro.Web.Pages.Task.synchronize" %>
<%@ Register TagPrefix="ComponentArt" Namespace="ComponentArt.Web.UI" Assembly="ComponentArt.Web.UI" %>
<%@ Register Assembly="GalleryServerPro.WebControls" Namespace="GalleryServerPro.WebControls"	TagPrefix="tis" %>
<%@ Register Src="../../controls/popupinfo.ascx" TagName="popup" TagPrefix="uc1" %>
<%@ Import Namespace="GalleryServerPro.Web" %>
<script type="text/javascript">
	Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(synchPageLoad);
</script>
<div class="gsp_content">
	<asp:PlaceHolder ID="phTaskHeader" runat="server" />
	<div class="gsp_addleftpadding1">
		<p class="gsp_h3">
			<asp:Literal ID="Literal1" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Synch_Album_Title_Prefix_Text %>" />&nbsp;<asp:Label
				ID="lblAlbumTitle" runat="server" /></p>
		<p class="gsp_textcol">
			<asp:Label ID="lblInstructions" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Synch_Body_Text %>" /></p>
		<div class="gsp_addleftpadding6">
			<p>
				<input id="chkIncludeChildAlbums" type="checkbox" /><label for="chkIncludeChildAlbums"><asp:Literal
					ID="lit1" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Synch_IncludeChildAlbums_Text %>" /></label>
				<asp:Label ID="lblIncludeChildAlbums" runat="server" /></p>
			<p>
				<input id="chkOverwriteThumbnails" type="checkbox" /><label for="chkOverwriteThumbnails"><asp:Literal
					ID="lit2" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Synch_OverwriteThumbnails_Text %>" /></label>
				<asp:Label ID="lblOverwriteThumbnails" runat="server" /></p>
			<p>
				<input id="chkOverwriteCompressed" type="checkbox" /><label for="chkOverwriteCompressed"><asp:Literal
					ID="lit3" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Synch_OverwriteCompressed_Text %>" /></label>
				<asp:Label ID="lblOverwriteCompressed" runat="server" /></p>
			<p>
				<asp:CheckBox ID="chkRegenerateMetadata" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Synch_RegenerateMetadata_Text %>" />
				<asp:Label ID="lblRegenerateMetadata" runat="server" />
			</p>
		</div>
	</div>
	<ComponentArt:Dialog ID="dgSynch" runat="server" Title="<%$ Resources:GalleryServerPro, Task_Synch_Progress_Title_Text %>"
		AnimationDirectionElement="btnStart" CloseTransition="Fade" ShowTransition="Fade"
		AnimationSlide="Linear" AnimationType="Outline" AnimationPath="Direct" AnimationDuration="400"
		TransitionDuration="400" Icon="pencil.gif" Alignment="MiddleCentre" AllowResize="True"
		Height="350" Width="540" ContentCssClass="dg4ContentCss" HeaderCssClass="dg4HeaderCss gsp_roundedtop6"
		CssClass="gsp_dg4DialogCss gsp_ns gsp_rounded10">
		<HeaderTemplate>
			<div onmousedown="dgSynch.StartDrag(event);">
				<img id="dg0DialogIconImage" src="<%= Utils.GalleryRoot %>/images/componentart/dialog/pencil.gif"
					alt="" />
				<asp:Literal ID="l1" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Synch_Progress_Title_Text %>" />
			</div>
		</HeaderTemplate>
		<ContentTemplate>
			<div class="gsp_addpadding1">
				<p id="synchPopupHeader" class="gsp_h1">
					<asp:Literal ID="l2" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Synch_Progress_SynchInProgress_Hdr %>" /></p>
				<img id="synchAnimation" src="<%= Utils.GalleryRoot %>/images/synch.gif" alt="" style="display: block;
					margin: 0 auto 1em auto; width: 272px; height: 40px;" />
				<div style="background-color: #cccccc; width: 100%; height: 20px;">
					<div id="progressbar" style="background-color: #336699; height: 20px; width: 1%">
					</div>
				</div>
				<table>
					<tr>
						<td class="gsp_bold">
							<asp:Literal ID="l3" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Synch_Progress_ETL_Text %>" />
						</td>
						<td id="synchEtl">
						</td>
					</tr>
					<tr>
						<td class="gsp_bold">
							<asp:Literal ID="l4" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Synch_Progress_SynchRate_Text %>" />
						</td>
						<td id="synchRate">
						</td>
					</tr>
					<tr>
						<td class="gsp_bold">
							<asp:Literal ID="l5" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Synch_Progress_Status_Text %>" />
						</td>
						<td id="status">
						</td>
					</tr>
				</table>
				<div id="errorMessage" style="height: 100px; overflow: auto;">
				</div>
				<div class="gsp_rightBottomAbsolute">
					<p class="gsp_collapse">
						<input id="btnCancel" onclick="cancelSynch()" title='<asp:Literal ID="l6" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Synch_Cancel_Button_Tooltip %>" />' type="button"
							value='<asp:Literal ID="l7" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Synch_Cancel_Button_Text %>" />' />
						<input id="btnClose" onclick="closeSynchWindow()" disabled="disabled" title='<asp:Literal ID="l8" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Synch_Close_Button_Tooltip %>" />'
							type="button" value='<asp:Literal ID="l9" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Synch_Close_Button_Text %>" />' />
				</div>
			</div>
		</ContentTemplate>
	</ComponentArt:Dialog>
	<tis:PopupInfo ID="PopupInfo" runat="server" DialogControlId="dgPopup" DefaultDialogTitleCss="dg5ContentTitleCss"
		DefaultDialogBodyCss="dg5ContentBodyCss">
		<PopupInfoItems>
			<tis:PopupInfoItem ID="p1" runat="server" ControlId="lblIncludeChildAlbums" DialogTitle="<%$ Resources:GalleryServerPro, Task_Synch_IncludeChildAlbums_Hlp_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Task_Synch_IncludeChildAlbums_Hlp_Bdy %>" />
			<tis:PopupInfoItem ID="p2" runat="server" ControlId="lblOverwriteThumbnails" DialogTitle="<%$ Resources:GalleryServerPro, Task_Synch_OverwriteThumbnails_Hlp_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Task_Synch_OverwriteThumbnails_Hlp_Bdy %>" />
			<tis:PopupInfoItem ID="p3" runat="server" ControlId="lblOverwriteCompressed" DialogTitle="<%$ Resources:GalleryServerPro, Task_Synch_OverwriteCompressed_Hlp_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Task_Synch_OverwriteCompressed_Hlp_Bdy %>" />
			<tis:PopupInfoItem ID="p4" runat="server" ControlId="lblRegenerateMetadata" DialogTitle="<%$ Resources:GalleryServerPro, Task_Synch_RegenerateMetadata_Hlp_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Task_Synch_RegenerateMetadata_Hlp_Bdy %>" />
			<tis:PopupInfoItem ID="p5" runat="server" ControlId="lblInstructions" DialogTitle="<%$ Resources:GalleryServerPro, Task_Synch_Options_Hlp_Hdr %>"
				DialogBody="<%$ Resources:GalleryServerPro, Task_Synch_Options_Hlp_Bdy %>" />
		</PopupInfoItems>
	</tis:PopupInfo>
	<uc1:popup ID="ucPopupContainer" runat="server" />
	<asp:PlaceHolder ID="phTaskFooter" runat="server" />
</div>
