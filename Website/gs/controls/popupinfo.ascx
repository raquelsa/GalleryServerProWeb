<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="popupinfo.ascx.cs" Inherits="GalleryServerPro.Web.Controls.popupinfo" %>
<%@ Register namespace="ComponentArt.Web.UI" assembly="ComponentArt.Web.UI" tagPrefix="ComponentArt" %>
<%@ Import Namespace="GalleryServerPro.Web" %>
<ComponentArt:Dialog ID="dgPopup" OffsetX="10" OffsetY="10" AlignmentElement="" HeaderCssClass="dg5HeaderCss"
	ContentCssClass="dg5ContentCss" CssClass="gsp_dg5DialogCss gsp_ns" AllowDrag="false" CloseTransition="Fade"
	ShowTransition="Fade" AnimationDuration="1000" Title="Alert" runat="server" Width="303">
	<Header>
		<img src='<%= Utils.GetUrl("/images/componentart/dialog/popup/top.gif") %>' alt="" style="display: block;width: 303px;
			height: 25px;" />
	</Header>
	<Footer>
		<img src='<%= Utils.GetUrl("/images/componentart/dialog/popup/bottom.gif") %>' style="display: block;width: 303px; height: 6px;" alt="" />
	</Footer>
</ComponentArt:Dialog>
