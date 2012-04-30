<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="transferobject.ascx.cs" Inherits="GalleryServerPro.Web.Pages.Task.transferobject" %>
<%@ Register Src="../../controls/albumtreeview.ascx" TagName="albumtreeview" TagPrefix="uc1" %>
<%@ Import Namespace="GalleryServerPro.Business.Interfaces" %>
<%@ Import Namespace="GalleryServerPro.Web.Pages" %>
<script type="text/javascript">

	function ToggleSelectAll(masterCheckbox)
	{
		var checked = masterCheckbox.checked;
		var inputs = document.getElementsByTagName("input");
		for (var i=0; i < inputs.length; i++)
		{
			if (inputs[i].type == 'checkbox')
			{
				inputs[i].checked = checked;
			}
		}
	}
</script>
<div class="gsp_content">
	<asp:PlaceHolder ID="phTaskHeader" runat="server" />
	<asp:PlaceHolder ID="phMsg" runat="server" />
	<asp:PlaceHolder ID="phCheckAllContainer" runat="server" />
	<asp:Repeater ID="rptr" runat="server">
		<HeaderTemplate>
			<div class="gsp_floatcontainer">
		</HeaderTemplate>
		<ItemTemplate>
			<div class="<%# GetThumbnailCssClass(Container.DataItem.GetType()) %>">
				<%# GetThumbnailHtml((IGalleryObject) Container.DataItem) %>
				<p class="gsp_caption">
					<asp:CheckBox ID="chkbx" runat="server" Text='<%# GetTitle(Eval("Title").ToString()) %>' />
					<input id="Hidden1" runat="server" type="hidden" value='<%# GetId((IGalleryObject) Container.DataItem) %>' />
				</p>
			</div>
		</ItemTemplate>
		<FooterTemplate>
			</div>
		</FooterTemplate>
	</asp:Repeater>
	<asp:PlaceHolder ID="phCustomValidatorContainer" runat="server" Visible="false" />
	<uc1:albumtreeview ID="tvUC" runat="server" Visible="false" RequireAlbumSelection="true" />
	<asp:PlaceHolder ID="phTaskFooter" runat="server" />
</div>