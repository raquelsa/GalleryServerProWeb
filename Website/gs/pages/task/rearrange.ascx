<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="rearrange.ascx.cs" Inherits="GalleryServerPro.Web.Pages.Task.rearrange" %>
<%@ Register TagPrefix="ComponentArt" Namespace="ComponentArt.Web.UI" Assembly="ComponentArt.Web.UI" %>
<%@ Import Namespace="GalleryServerPro.Business.Interfaces" %>
<script type="text/javascript">

	Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(rearrange_PageLoad);

	function rearrange_PageLoad(sender, args)
	{
		if (Sys.Browser.agent == Sys.Browser.InternetExplorer && Sys.Browser.version < 7) hideScrollDivsFromIE6();
	}

	function SwapSnaps(sender, eventArgs)
	{
		// User has dragged a thumbnail and is hovering over another thumbnail. The snap control automatically inserts
		// the dragged thumbnail into the destination, but we need to pull out the existing thumbnail and and shift
		// all thumbnails between the source and destination by one.
		var destPanel; // Panel user is hovering over
		var swapToPanel; // Panel to receive thumbnail from left or right sibling
		var swapFromPanel; // Panel with two thumbnails where we need to extract one and assign it to its left or right sibling
		var n; var j;
		if (eventArgs.get_elementId() == null || eventArgs.get_index() < 0) return; // Ignore

		var destPanelId = parseInt(eventArgs.get_elementId().substring(2));

		for (curPanelId = 0; curPanelId < _snapDockIds.length; curPanelId++)
		{
			var curPanel = document.getElementById("sd" + curPanelId);
			if ((curPanel.childNodes.length == 0) || ((curPanel.childNodes.length == 1) && (curPanel.childNodes[0].nodeType == 3)))
			{
				if (curPanelId > destPanelId)
				{
					// User is moving a thumbnail closer to the beginning
					for (j = curPanelId; j > destPanelId; j--)
					{
						swapToPanel = document.getElementById("sd" + j);
						swapFromPanel = document.getElementById("sd" + (j - 1));
						// Grab the last node that is an element and move it to the next panel
						for (n = swapFromPanel.lastChild; n != null; n = n.previousSibling)
						{
							if (n.nodeType == 1) { swapToPanel.appendChild(n); break; }
						}
					}
				}
				else
				{
					// User is moving a thumbnail closer to the end
					for (j = curPanelId; j < destPanelId; j++)
					{
						swapToPanel = document.getElementById("sd" + j);
						swapFromPanel = document.getElementById("sd" + (j + 1));
						// Grab the last node that is an element and move it to the previous panel
						for (n = swapFromPanel.lastChild; n != null; n = n.previousSibling)
						{
							if (n.nodeType == 1) { swapToPanel.appendChild(n); break; }
						}
					}
				}
				break;
			}
		}
	}

	var _intervalId;
	var _dy;

	function beginScroll(dy)
	{
		if (_intervalId != null)
			return;
		_dy = dy;
		scroll();
		_intervalId = window.setInterval(scroll, 10);
	}

	function endScroll()
	{
		window.clearInterval(_intervalId);
		_intervalId = null;
	}

	function scroll()
	{
		window.scrollBy(0, _dy);
	}

	function retrieveOrder()
	{
		// Determine the current order of the thumbnails and write it to the hidden field 'goIds' as a comma-delimited
		// string. This field is used by the server script to indicate the sequence.
		var divTags = document.getElementsByTagName("div");
		var goIds = new Array();
		for (var i = 0; i < divTags.length; i++)
		{
			if (Sys.UI.DomElement.containsCssClass(divTags[i], "snapDock"))
			{
				// Get reference to the hidden input in this node
				var n = findNode(divTags[i], "hdId");
				if (n != null)
					Array.add(goIds, n.value);
			}
		}

		$get('goIds').value = goIds.join();
	}

	function findNode(n, startsWith)
	{
		// Get reference to the child node whose id starts with the specified value
		for (var m = n.firstChild; m != null; m = m.nextSibling)
		{
			if ((m.id) && (m.id.startsWith(startsWith)))
				return m;
			else
			{
				var foundNode = findNode(m, startsWith);
				if (foundNode != null) return foundNode;
			}
		}
		return null;
	}

	function hideScrollDivsFromIE6()
	{
		// IE 6 and earlier do not support position:fixed, so remove these divs. These users won't be able to drag and scroll.
		var n = $get('divScrollUp');
		if (n != null) n.parentNode.removeChild(n);
		n = null;
		n = $get('divScrollDown');
		if (n != null) n.parentNode.removeChild(n);
	}

</script>
<div class="gsp_content">
	<asp:PlaceHolder ID="phTaskHeader" runat="server" />
	<div id="divScrollUp" class="scrollBox" onmouseover="beginScroll(-10);" onmouseout="endScroll();">
		<asp:Literal ID="Literal1" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Rearrange_Objects_Scroll_Up_Div_Text %>" />
	</div>
	<div id="divScrollDown" class="scrollBox" onmouseover="beginScroll(10);" onmouseout="endScroll();">
		<asp:Literal ID="Literal2" runat="server" Text="<%$ Resources:GalleryServerPro, Task_Rearrange_Objects_Scroll_Down_Div_Text %>" />
	</div>
	<asp:Repeater ID="rprDock" runat="server">
		<HeaderTemplate>
			<div class="gsp_floatcontainer">
		</HeaderTemplate>
		<ItemTemplate>
			<div id="<%# GetSnapDockId() %>" class="snapDock">
			</div>
		</ItemTemplate>
		<FooterTemplate>
			</div>
		</FooterTemplate>
	</asp:Repeater>
	<asp:Repeater ID="rprSnap" runat="server" OnItemDataBound="rprSnap_ItemDataBound">
		<ItemTemplate>
			<ComponentArt:Snap id="sp" dockingcontainers="<%# SnapDockIds %>" currentdockingcontainer="<%# GetSnapDockingContainerId() %>"
				currentdockingindex="0" draggingstyle="Original" dockingstyle="Original" mustbedocked="True"
				iscollapsed="false" runat="server">
				<ClientEvents>
					<SnapDock EventHandler="SwapSnaps" />
				</ClientEvents>
				<Content>
					<%# GetHiddenInputIdTag((IGalleryObject)((RepeaterItem)Container.Parent).DataItem) %>
					<div class="<%# GetThumbnailCssClass(((RepeaterItem)Container.Parent).DataItem.GetType()) %>"
						onmousedown="eval(this.parentNode.parentNode.id + '.startDragging(event);');">
						<%# GetThumbnailHtml((IGalleryObject)((RepeaterItem)Container.Parent).DataItem, false, false) %>
						<%# GetMediaObjectText(Eval("Title").ToString(), ((RepeaterItem)Container.Parent).DataItem.GetType())%>
					</div>
				</Content>
			</ComponentArt:Snap>
		</ItemTemplate>
	</asp:Repeater>
	<input id="goIds" name="goIds" type="hidden" />
	<asp:PlaceHolder ID="phTaskFooter" runat="server" />
</div>
