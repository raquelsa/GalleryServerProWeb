<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="actionmenu.ascx.cs"
	Inherits="GalleryServerPro.Web.Controls.actionmenu" %>
<%@ Register namespace="ComponentArt.Web.UI" assembly="ComponentArt.Web.UI" tagPrefix="ComponentArt" %>

<script type="text/javascript">
	Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(actionMenuPageLoad);

	function actionMenuPageLoad(sender, args)
	{
		var mnuEditAlbum = Menu1.findItemById("mnuEditAlbum");
		if (window.editAlbumInfo == null)
		{
			mnuEditAlbum.set_enabled(false);
			mnuEditAlbum.set_toolTip("<%= DisabledDueToInsufficientPermissionText %>");
		}
	}
</script>

<ComponentArt:Menu ID="Menu1" CssClass="gsp_mnu0TopGroup" DefaultGroupCssClass="gsp_mnu0MenuGroup"
	DefaultItemLookId="DefaultItemLook" DefaultGroupItemSpacing="1px" DefaultDisabledItemLookId="DefaultDisabledItemLook"
	ExpandDelay="100" EnableViewState="False" runat="server" ScrollDownLookId="DefaultItemLook"
	ScrollUpLookId="DefaultItemLook" TopGroupItemSpacing="1px" OverlayWindowedElements="true">
	<ItemLooks>
		<ComponentArt:ItemLook LookId="TopItemLook" CssClass="gsp_mnu0TopMenuItem" HoverCssClass="gsp_mnu0TopMenuItemHover"
			ExpandedCssClass="gsp_mnu0TopMenuItemHover" LeftIconUrl="doubledownarrow.png" LeftIconWidth="16px"
			LeftIconHeight="16px" LabelPaddingLeft="3px" LabelPaddingRight="5px" LabelPaddingTop="2px"
			LabelPaddingBottom="3px" LeftIconVisibility="Always" />
		<ComponentArt:ItemLook LookId="DefaultItemLook" LabelPaddingBottom="3px" LabelPaddingLeft="7px"
			LabelPaddingRight="5px" LabelPaddingTop="2px" LeftIconHeight="16px" LeftIconWidth="16px"
			CssClass="gsp_mnu0MenuItem" ExpandedCssClass="gsp_mnu0MenuItemHover" HoverCssClass="gsp_mnu0MenuItemHover" />
		<ComponentArt:ItemLook LookId="DefaultDisabledItemLook" CssClass="gsp_mnu0DisabledMenuItem" HoverCssClass="gsp_mnu0DisabledMenuItemHover"
			LabelPaddingBottom="3px" LabelPaddingLeft="10px" LabelPaddingRight="5px" LabelPaddingTop="2px"
			LeftIconHeight="16px" LeftIconVisibility="Always" LeftIconWidth="16px" />
		<ComponentArt:ItemLook LookId="BreakItem" CssClass="gsp_mnu0MenuBreak" />
	</ItemLooks>
	<Items>
		<ComponentArt:MenuItem ID="mnuAction" runat="server" LookId="TopItemLook" ChildSelectedLook-LeftIconVisibility="Always"
			ChildSelectedLook-RightIconVisibility="Always" DefaultSubGroupCssClass="gsp_mnu0MenuGroup"
			DefaultSubGroupItemSpacing="1px" DisabledLook-LeftIconVisibility="Always" DisabledLook-RightIconVisibility="Always"
			Look-LeftIconVisibility="Always" Look-RightIconVisibility="Always" SelectedLook-LeftIconVisibility="Always"
			SelectedLook-RightIconVisibility="Always" SubGroupCssClass="gsp_ns gsp_mnu0MenuGroup" SubGroupItemSpacing="1px"
			Text="<%$ Resources:GalleryServerPro, UC_ActionMenu_Root_Text %>" Width="75">
			<ComponentArt:MenuItem ID="mnuCreateAlbum" runat="server" Look-LeftIconUrl="addalbum.png" />
			<ComponentArt:MenuItem ID="mnuEditAlbum" runat="server" Look-LeftIconUrl="addalbum.png" />
			<ComponentArt:MenuItem LookId="BreakItem" />
			<ComponentArt:MenuItem ID="mnuAddObjects" runat="server" Look-LeftIconUrl="addobject.png" />
			<ComponentArt:MenuItem LookId="BreakItem" />
			<ComponentArt:MenuItem ID="mnuMoveObjects" runat="server" Look-LeftIconUrl="moveobjects.png" />
			<ComponentArt:MenuItem ID="mnuCopyObjects" runat="server" Look-LeftIconUrl="copyobjects.png" />
			<ComponentArt:MenuItem ID="mnuMoveAlbum" runat="server" Look-LeftIconUrl="movefolder.png" />
			<ComponentArt:MenuItem ID="mnuCopyAlbum" runat="server" Look-LeftIconUrl="copyfolder.png" />
			<ComponentArt:MenuItem LookId="BreakItem" />
			<ComponentArt:MenuItem ID="mnuDownloadObjects" runat="server" Look-LeftIconUrl="downloadzip.png" />
			<ComponentArt:MenuItem LookId="BreakItem" />
			<ComponentArt:MenuItem ID="mnuEditCaptions" runat="server" Look-LeftIconUrl="copyobjects.png" />
			<ComponentArt:MenuItem ID="mnuAssignThumbnail" runat="server" Look-LeftIconUrl="copyobjects.png" />
			<ComponentArt:MenuItem ID="mnuRearrangeObjects" runat="server" Look-LeftIconUrl="rearrange.png" />
			<ComponentArt:MenuItem ID="mnuRotate" runat="server" Look-LeftIconUrl="rotate.png" />
			<ComponentArt:MenuItem LookId="BreakItem" />
			<ComponentArt:MenuItem ID="mnuDeleteObjects" runat="server" Look-LeftIconUrl="delete.png" />
			<ComponentArt:MenuItem ID="mnuDeleteHiRes" runat="server" Look-LeftIconUrl="delete.png" />
			<ComponentArt:MenuItem ID="mnuDeleteAlbum" runat="server" Look-LeftIconUrl="deletealbum.png" />
			<ComponentArt:MenuItem LookId="BreakItem" />
			<ComponentArt:MenuItem ID="mnuSynch" runat="server" Look-LeftIconUrl="synchronize.png" />
			<ComponentArt:MenuItem LookId="BreakItem" />
			<ComponentArt:MenuItem ID="mnuSiteSettings" runat="server" Look-LeftIconUrl="sitesettings.png" />
			<ComponentArt:MenuItem LookId="BreakItem" />
			<ComponentArt:MenuItem ID="mnuLogout" runat="server" Look-LeftIconUrl="disconnect.png" AutoPostBackOnSelect="true" />
		</ComponentArt:MenuItem>
	</Items>
</ComponentArt:Menu>