<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="rotateimage.ascx.cs" Inherits="GalleryServerPro.Web.Pages.Task.rotateimage" %>
<%@ Import Namespace="GalleryServerPro.Business.Interfaces" %>
<%@ Import Namespace="GalleryServerPro.Web" %>
<div runat="server">
	<script type="text/javascript">

		function rotateClick(curID)
		{
			if (document.getElementById) {
				ID = curID.substring(1);
				//Two variables below hold either t, r, b, or l to represent top, right, bottom, or left
				var oldSelectedSide;
				var newSelectedSide = curID.substring(0,1);
				//Set user's choice in hidden field
				var listitem = document.getElementById("_" + ID);
				var inputs = listitem.getElementsByTagName("input");
				if (inputs.length > 0)
				{
					var ipt = inputs[0];
					oldSelectedSide = ipt.value
					if (!oldSelectedSide)
						oldSelectedSide = 't';
					ipt.value = newSelectedSide;
				}	
				//Clear backgrounds on all sides
				newID = oldSelectedSide + ID;
				e = document.getElementById(newID);
				e.style.backgroundColor = "#fff";
				switch(oldSelectedSide)
				{
					case 't': //top
						e.style.backgroundImage = "url(<%= Utils.GalleryRoot %>/images/rotate/top.gif)";
						break;
					case 'r': //right
						e.style.backgroundImage = "url(<%= Utils.GalleryRoot %>/images/rotate/right.gif)";
						break;
					case 'b': //bottom
						e.style.backgroundImage = "url(<%= Utils.GalleryRoot %>/images/rotate/bottom.gif)";
						break;
					case 'l': //left
						e.style.backgroundImage = "url(<%= Utils.GalleryRoot %>/images/rotate/left.gif)";
						break;
				}
				//Highlight background on selected side
				e = document.getElementById(curID);
				e.style.backgroundColor = "#C7D5E3";
				switch(newSelectedSide)
				{
					case 't': //top
						e.style.backgroundImage = "url(<%= Utils.GalleryRoot %>/images/rotate/top1.gif)";
						break;
					case 'r': //right
						e.style.backgroundImage = "url(<%= Utils.GalleryRoot %>/images/rotate/right1.gif)";
						break;
					case 'b': //bottom
						e.style.backgroundImage = "url(<%= Utils.GalleryRoot %>/images/rotate/bottom1.gif)";
						break;
					case 'l': //left
						e.style.backgroundImage = "url(<%= Utils.GalleryRoot %>/images/rotate/left1.gif)";
						break;
				}
			}
		}

	</script>
</div>
<div class="gsp_content">
	<asp:PlaceHolder ID="phTaskHeader" runat="server" />
	<asp:Repeater ID="rptr" runat="server">
	<ItemTemplate>
		<div id="_<%# Eval("ID") %>" class="thmbRotate" style="float: none;">
			<table cellspacing="1" cellpadding="0" border="0">
				<tr>
					<td colspan="3">
						<a id="t<%# DataBinder.Eval(Container.DataItem, "ID") %>" href="javascript:rotateClick('t<%# Eval("ID") %>');"
							class="gsp_hor" style="background: #C7D5E3 url(<%= Utils.GalleryRoot %>/images/rotate/top1.gif)" title="<asp:Literal ID="l1" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Rotate_Image_0_Rotate_Text %>" />">
						</a>
					</td>
				</tr>
				<tr>
					<td>
						<a id="l<%# Eval("ID") %>" href="javascript:rotateClick('l<%# Eval("ID") %>');" class="gsp_vert"
							style="background-image: url(<%= Utils.GalleryRoot %>/images/rotate/left.gif)" title="<asp:Literal ID="l2" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Rotate_Image_90_Rotate_Text %>" />">
						</a>
					</td>
					<td>
						<div class="hovercontainer">
							<img src="<%# GetImageUrl((IGalleryObject) Container.DataItem) %>" alt="<%# RemoveHtmlTags(Eval("Title").ToString()) %>"
								title="<%# RemoveHtmlTags(Eval("Title").ToString()) %>" class="gsp_rotate" style="width: <%# GetImageWidth((IGalleryObject) Container.DataItem).ToString() %>px;
								height: <%# GetImageHeight((IGalleryObject) Container.DataItem).ToString() %>px;" /></div>
					</td>
					<td>
						<a id="r<%# Eval("ID") %>" href="javascript:rotateClick('r<%# Eval("ID") %>');" class="gsp_vert"
							style="background-image: url(<%= Utils.GalleryRoot %>/images/rotate/right.gif)" title="<asp:Literal ID="l3" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Rotate_Image_270_Rotate_Text %>" />">
						</a>
					</td>
				</tr>
				<tr>
					<td colspan="3">
						<a id="b<%# Eval("ID") %>" href="javascript:rotateClick('b<%# Eval("ID") %>');" class="gsp_hor"
							style="background-image: url(<%= Utils.GalleryRoot %>/images/rotate/bottom.gif)" title="<asp:Literal ID="l4" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Rotate_Image_180_Rotate_Text %>" />">
						</a>
					</td>
				</tr>
			</table>
			<input id="txtSelectedSide" runat="server" type="hidden" />
		</div>
		<input id="moid" runat="server" type="hidden" value='<%# Eval("ID") %>' />
	</ItemTemplate>
	</asp:Repeater>
	<asp:PlaceHolder ID="phTaskFooter" runat="server" />
</div>