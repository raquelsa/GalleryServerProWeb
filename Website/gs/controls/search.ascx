<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="search.ascx.cs" Inherits="GalleryServerPro.Web.Controls.search" %>
<%@ Register Namespace="ComponentArt.Web.UI" Assembly="ComponentArt.Web.UI" TagPrefix="ComponentArt" %>
<%@ Import Namespace="GalleryServerPro.Web" %>
<p class="gsp_searchlink gsp_useroption">
	<a id="l1" href="javascript: toggleSearch();" class="gsp_addrightmargin2" title='<asp:Literal ID="l3" runat="server" Text="<%$ Resources:GalleryServerPro, Search_Button_Tooltip %>" />'>
		<img src="<%= Utils.GalleryRoot %>/images/search_32x32.png" style="width:32px;height:32px;" alt='<asp:Literal
					ID="l2" runat="server" Text="<%$ Resources:GalleryServerPro, Search_Button_Tooltip %>" />' title='<asp:Literal
					ID="l4" runat="server" Text="<%$ Resources:GalleryServerPro, Search_Button_Tooltip %>" />' /><asp:Label
					ID="l5" runat="server" CssClass="gsp_invisible" Text="<%$ Resources:GalleryServerPro, Search_Button_Tooltip %>" /></a>
</p>
<ComponentArt:Dialog ID="dgSearch" runat="server" CloseTransition="Fade" ShowTransition="Fade"
	AnimationPath="Direct" AlignmentElement="l1" TransitionDuration="100" OffsetX="-382"
	OffsetY="40" HeaderCssClass="gsp_invisible" CssClass="gsp_dgSearch gsp_ns gsp_rounded10">
	<ContentTemplate>
		<asp:Panel ID="pnlSearch" runat="server" DefaultButton="btnSearch" CssClass="gsp_search">
			<p class="gsp_textright">
				<img onclick="dgSearch.Close('cancelled');" src="<%= Utils.GalleryRoot %>/images/componentart/dialog/close.gif"
					alt="Close" /></p>
			<img src="<%= Utils.GalleryRoot %>/images/search_48x48.png" class="gsp_searchicon" alt="" style="width:48px;height:48px;" />
			<p>
				<asp:Literal ID="l1" runat="server" Text="<%$ Resources:GalleryServerPro, Search_Search_Label %>" /></p>
			<p>
				<asp:TextBox ID="txtSearch" runat="server" CssClass="gsp_searchbox" /></p>
			<p class="gsp_textright">
				<asp:Button ID="btnSearch" runat="server" CausesValidation="false" CssClass="gsp_searchbutton"
					Text="<%$ Resources:GalleryServerPro, Search_Button_Text %>" ToolTip="<%$ Resources:GalleryServerPro, Search_Button_Tooltip %>"
					OnClick="btnSearch_Click" /></p>
		</asp:Panel>
	</ContentTemplate>
	<ClientEvents>
		<OnShow EventHandler="dgSearch_OnShow" />
	</ClientEvents>
</ComponentArt:Dialog>
