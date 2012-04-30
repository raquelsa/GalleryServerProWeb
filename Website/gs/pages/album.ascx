<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="album.ascx.cs" Inherits="GalleryServerPro.Web.Pages.album" %>
<%@ Register tagPrefix="ComponentArt" namespace="ComponentArt.Web.UI" assembly="ComponentArt.Web.UI" %>
<%@ Register Src="../controls/albumheader.ascx" TagName="albumheader" TagPrefix="gs" %>
<%@ Register Src="../controls/thumbnailview.ascx" TagName="thumbnailview" TagPrefix="gs" %>
<gs:albumheader ID="ah" runat="server" EnableInlineEditing="true" />
<asp:PlaceHolder ID="phMessage" runat="server" EnableViewState="False" />
<gs:thumbnailview ID="tv" runat="server" />