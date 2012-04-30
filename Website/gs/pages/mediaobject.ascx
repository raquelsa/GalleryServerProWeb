<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="mediaobject.ascx.cs" Inherits="GalleryServerPro.Web.Pages.mediaobject" %>
<%@ Register src="../controls/mediaobjectview.ascx" tagname="mediaobjectview" tagprefix="gs" %>
<asp:PlaceHolder ID="phMessage" runat="server" EnableViewState="False" />
<gs:mediaobjectview ID="mediaobjectview1" runat="server" />
