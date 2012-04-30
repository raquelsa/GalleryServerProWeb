<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="editcaptions.ascx.cs"
	Inherits="GalleryServerPro.Web.Pages.Task.editcaptions" %>
<%@ Import Namespace="GalleryServerPro.Business.Interfaces" %>
<div class="gsp_content">
	<asp:PlaceHolder ID="phTaskHeader" runat="server" />
	<asp:Repeater ID="rptr" runat="server">
		<HeaderTemplate>
			<div class="gsp_floatcontainer">
		</HeaderTemplate>
		<ItemTemplate>
			<div class="thmb">
				<%# GetThumbnailHtml((IGalleryObject) Container.DataItem) %>
				<p>
					<textarea id="ta" runat="server" rows="5" cols="17" class="textareaEditCaption" onfocus="javascript:this.select();"
						name="ta"><%# Eval("Title") %></textarea>
					<input id="Hidden1" runat="server" type="hidden" value='<%# Eval("ID") %>' />
				</p>
			</div>
		</ItemTemplate>
		<FooterTemplate>
			</div>
		</FooterTemplate>
	</asp:Repeater>
	<asp:PlaceHolder ID="phTaskFooter" runat="server" />
</div>