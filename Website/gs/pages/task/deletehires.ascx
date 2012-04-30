<%@ Import Namespace="GalleryServerPro.Business" %>
<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="deletehires.ascx.cs"
	Inherits="GalleryServerPro.Web.Pages.Task.deletehires" %>
<%@ Import Namespace="GalleryServerPro.Business.Interfaces" %>

<script type="text/javascript">

	function ToggleSelectAll(masterCheckbox)
	{
		var checked = masterCheckbox.checked;
		var inputs = document.getElementsByTagName("input");
		for (var i = 0; i < inputs.length; i++)
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
	<div class="gsp_addleftpadding1">
		<p class="gsp_textcol gsp_msgwarning" style="border-bottom: #369 1px solid;">
			<asp:Literal ID="litId" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Delete_HiRes_Warning %>" /></p>
		<p class="gsp_h3" runat="server">
			<% =GetPotentialSavings() %>
		</p>
		<p class="gsp_textcol">
			<asp:Literal ID="l1" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Delete_HiRes_Potential_Savings_Hdr %>" />
		</p>
	</div>
	<asp:PlaceHolder ID="phMsg" runat="server" />
	<p>
		<input type="checkbox" id="chkSelectAll" onclick="javascript:ToggleSelectAll(this);" /><label
			for="chkSelectAll"><asp:Literal ID="l2" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Delete_HiRes_Check_Uncheck_All_Label_Text %>" /></label></p>
	<asp:Repeater ID="rptr" runat="server">
		<HeaderTemplate>
			<div class="gsp_floatcontainer">
		</HeaderTemplate>
		<ItemTemplate>
			<div class="<%# GetThumbnailCssClass(Container.DataItem.GetType()) %>">
				<%# GetThumbnailHtml((IGalleryObject) Container.DataItem, false, true) %>
				<p id="p1" runat="server" visible="<%# Container.DataItem.GetType() != typeof(Album) %>">
					<%# GetGalleryObjectText(Eval("Title").ToString(), Container.DataItem.GetType()) %></p>
				<p id="p2" runat="server" class="gsp_msgwarning" visible='<%# ShouldShowNoOriginalFileMsg((IGalleryObject) Container.DataItem) %>'>
					<asp:Literal ID="l3" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Delete_Original_Not_Available_Text%>" /></p>
				<p id="p3" runat="server" class="gsp_em" visible='<%# ShouldShowCheckbox((IGalleryObject) Container.DataItem) %>'>
					<asp:CheckBox ID="chkbx" runat="server" Text='<%# GetSavings((IGalleryObject)Container.DataItem) %>' />
					<asp:HiddenField ID="hdn" runat="server" Value="<%# GetId((IGalleryObject) Container.DataItem) %>" />
				</p>
			</div>
		</ItemTemplate>
		<FooterTemplate>
			</div>
		</FooterTemplate>
	</asp:Repeater>
	<asp:PlaceHolder ID="phTaskFooter" runat="server" />
</div>
