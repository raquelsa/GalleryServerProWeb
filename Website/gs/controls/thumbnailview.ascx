<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="thumbnailview.ascx.cs"
	Inherits="GalleryServerPro.Web.Controls.thumbnailview" EnableViewState="false" %>
<%@ Register Namespace="ComponentArt.Web.UI" Assembly="ComponentArt.Web.UI" TagPrefix="ComponentArt" %>
<%@ Import Namespace="GalleryServerPro.Business.Interfaces" %>
<asp:PlaceHolder ID="phMsg" runat="server" />
<asp:PlaceHolder ID="phPagerTop" runat="server" EnableViewState="false" />
<asp:Repeater ID="rptr" runat="server" EnableViewState="false">
	<HeaderTemplate>
		<div id="thmbCtnr" class="<%# GetThumbnailContainerCssClass() %>">
	</HeaderTemplate>
	<ItemTemplate>
		<div class="<%# GetThumbnailCssClass(Container.DataItem.GetType()) %>">
			<%# GalleryPage.GetThumbnailHtml((IGalleryObject) Container.DataItem, GalleryPage.GalleryControl.AllowUrlOverride, true) %>
			<%# GetGalleryObjectText(Eval("Title").ToString(), Container.DataItem.GetType())%>
		</div>
	</ItemTemplate>
	<FooterTemplate>
		</div>
	</FooterTemplate>
</asp:Repeater>
<asp:PlaceHolder ID="phPagerBtm" runat="server" EnableViewState="false" />
