<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="assignthumbnail.ascx.cs"
	Inherits="GalleryServerPro.Web.Pages.Task.assignthumbnail" %>
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
					<asp:RadioButton ID="rb" runat="server" GroupName="thmb" Text='<%# GetTitle(Eval("Title").ToString()) %>'
						Checked='<%# IsAlbumThumbnail(Convert.ToInt32(Eval("ID"))) %>' onclick="SetUniqueRadioButton('rptr.*thmb', this);" />
				</p>
				<asp:HiddenField ID="hdn" runat="server" Value="<%# GetId((IGalleryObject) Container.DataItem) %>" />
			</div>
		</ItemTemplate>
		<FooterTemplate>
			</div>
		</FooterTemplate>
	</asp:Repeater>
	<asp:PlaceHolder ID="phTaskFooter" runat="server" />
</div>
