<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="myaccount.ascx.cs" Inherits="GalleryServerPro.Web.Pages.myaccount" %>
<%@ Register Assembly="GalleryServerPro.WebControls" Namespace="GalleryServerPro.WebControls"
	TagPrefix="tis" %>
<%@ Register Src="../Controls/popupinfo.ascx" TagName="popup" TagPrefix="uc1" %>
<%@ Register Src="../Controls/usermessage.ascx" TagName="usermessage" TagPrefix="uc1" %>
<%@ Import Namespace="GalleryServerPro.Web" %>
<div class="gsp_addpadding1" runat="server">
	<img src="<%= Utils.GalleryRoot %>/images/user_48x48.png" alt="" style="width: 48px;
		height: 48px; float: left;" class="gsp_addtoppadding1" />
	<p class="admin_h2">
		<asp:Literal ID="lit1" runat="server" Text="<%$ Resources:GalleryServerPro, MyAccount_Hdr %>" /></p>
	<tis:wwErrorDisplay ID="wwMessage" runat="server" CellPadding="2" UseFixedHeightWhenHiding="False"
		Center="False" Width="500px">
	</tis:wwErrorDisplay>
	<asp:Panel ID="pnlAccountInfo" runat="server">
		<p class="admin_h3">
			<asp:Literal ID="l1" runat="server" Text="<%$ Resources:GalleryServerPro, MyAccount_Info_Hdr %>" /></p>
		<div class="gsp_addleftpadding6">
			<table class="gsp_standardTable">
				<tr>
					<td class="gsp_bold gsp_fll">
						<asp:Literal ID="l2" runat="server" Text="<%$ Resources:GalleryServerPro, MyAccount_UserName_Label %>" />
					</td>
					<td>
						<asp:Label ID="lblUserName" runat="server" CssClass="gsp_fll"></asp:Label>
					</td>
				</tr>
				<tr>
					<td class="gsp_bold">
						<asp:Literal ID="l3" runat="server" Text="<%$ Resources:GalleryServerPro, MyAccount_Password_Label %>" />
					</td>
					<td>
						<asp:HyperLink ID="hlChangePwd" runat="server" Text="<%$ Resources:GalleryServerPro, MyAccount_Change_Pwd_Hyperlink_Text %>" />
					</td>
				</tr>
				<tr>
					<td class="gsp_bold">
						<asp:Label ID="lblEmail" runat="server" AssociatedControlID="txtEmail" Text="<%$ Resources:GalleryServerPro, MyAccount_Email_Address_Label %>" />
					</td>
					<td>
						<asp:TextBox ID="txtEmail" runat="server" CssClass="gsp_textbox" />
						<asp:RegularExpressionValidator ID="revEmail" runat="server" Display="Dynamic" CssClass="gsp_msgwarning"
							ForeColor="" ControlToValidate="txtEmail" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
							SetFocusOnError="true" ErrorMessage="<%$ Resources:GalleryServerPro, Site_Invalid_Text %>" />
					</td>
				</tr>
				<tr>
					<td class="gsp_bold gsp_aligntop">
						<asp:Label ID="lblComment" runat="server" AssociatedControlID="txtComment" Text="<%$ Resources:GalleryServerPro, MyAccount_Comment_Label %>" />
					</td>
					<td>
						<asp:TextBox ID="txtComment" runat="server" TextMode="MultiLine" CssClass="gsp_textarea1"></asp:TextBox>
					</td>
				</tr>
				<tr>
					<td class="gsp_bold">
						<asp:Literal ID="Literal2" runat="server" Text="<%$ Resources:GalleryServerPro, MyAccount_LastActivityDate_Label %>" />
					</td>
					<td>
						<asp:Label ID="lblLastActivityDate" runat="server"></asp:Label>
					</td>
				</tr>
				<tr>
					<td class="gsp_bold">
						<asp:Literal ID="Literal3" runat="server" Text="<%$ Resources:GalleryServerPro, MyAccount_LastLogonDate_Label %>" />
					</td>
					<td>
						<asp:Label ID="lblLastLogOnDate" runat="server"></asp:Label>
					</td>
				</tr>
				<tr>
					<td class="gsp_bold">
						<asp:Literal ID="Literal4" runat="server" Text="<%$ Resources:GalleryServerPro, MyAccount_LastPwdChangeDate_Label %>" />
					</td>
					<td>
						<asp:Label ID="lblLastPasswordChangedDate" runat="server"></asp:Label>
					</td>
				</tr>
				<tr>
					<td class="gsp_bold">
						<asp:Literal ID="Literal5" runat="server" Text="<%$ Resources:GalleryServerPro, MyAccount_AccountCreatedDate_Label %>" />
					</td>
					<td>
						<asp:Label ID="lblCreationDate" runat="server"></asp:Label>
					</td>
				</tr>
			</table>
		</div>
	</asp:Panel> 
	<asp:Panel ID="pnlUserAlbum" runat="server">
		<p class="admin_h3">
			<asp:Literal ID="l4" runat="server" Text="<%$ Resources:GalleryServerPro, MyAccount_UserAlbum_Hdr %>" /></p>
		<div class="gsp_addleftpadding6">
			<p>
				<asp:CheckBox ID="chkEnableUserAlbum" runat="server" Text="<%$ Resources:GalleryServerPro, MyAccount_EnableUserAlbum_Label %>"
					onclick="toggleWarning(this);" /></p>
			<tis:wwErrorDisplay ID="wwUserAlbumWarning" runat="server" CssClass="gsp_msgwarning gsp_addleftpadding4"
				CellPadding="2" UseFixedHeightWhenHiding="False" Center="False" Width="500px" Text="<%$ Resources:GalleryServerPro, MyAccount_EnableUserAlbum_Warning %>">
			</tis:wwErrorDisplay>
		</div>
	</asp:Panel>
	<asp:Panel ID="pnlDeleteAccount" runat="server" Visible="False">
		<p class="admin_h3">
			<asp:Literal ID="l5" runat="server" Text="<%$ Resources:GalleryServerPro, MyAccount_Delete_Account_Hdr %>" /></p>
		<div class="gsp_addleftpadding6">
			<p>
				<asp:Literal ID="l6" runat="server" Text="<%$ Resources:GalleryServerPro, MyAccount_Delete_Account_Overview %>" />
			</p>
			<p class="gsp_msgwarning" style="width: 550px;">
				<asp:Literal ID="litDeleteAccountWarning" runat="server" /></p>
			<p>
				<img src="<%= Utils.GalleryRoot %>/images/error_16x16.png" alt="" style="width: 16px;
					height: 16px;" />
				<asp:LinkButton ID="lbDeleteAccount" runat="server" Text="<%$ Resources:GalleryServerPro, MyAccount_Delete_Account_Command_Text %>"
					ToolTip="<%$ Resources:GalleryServerPro, MyAccount_Delete_Account_Command_Text %>"
					OnClick="lbDeleteAccount_Click" /></p>
		</div>
	</asp:Panel>
</div>
<div class="gsp_rightBottom">
	<p class="gsp_minimargin">
		<asp:Button ID="btnSave" runat="server" OnClick="btnSave_Click" Text="<%$ Resources:GalleryServerPro, Default_Task_Ok_Button_Text %>" />
		<asp:Button ID="btnCancel" runat="server" OnClick="btnCancel_Click" CausesValidation="false"
			Text="<%$ Resources:GalleryServerPro, Default_Task_Cancel_Button_Text %>" ToolTip="<%$ Resources:GalleryServerPro, Default_Task_Cancel_Button_Tooltip %>" />&nbsp;</p>
</div>
<tis:wwDataBinder ID="wwDataBinder" runat="server" OnAfterBindControl="wwDataBinder_AfterBindControl">
	<DataBindingItems>
		<tis:wwDataBindingItem ID="wbi1" runat="server" BindingSource="CurrentUser" BindingSourceMember="UserName"
			ControlId="lblUserName" BindingMode="OneWay" />
		<tis:wwDataBindingItem ID="wbi2" runat="server" BindingSource="CurrentUser" BindingSourceMember="Email"
			ControlId="txtEmail" />
		<tis:wwDataBindingItem ID="wbi3" runat="server" BindingSource="CurrentUser" BindingSourceMember="Comment"
			ControlId="txtComment" />
		<tis:wwDataBindingItem ID="wbi4" runat="server" BindingSource="CurrentUser" BindingSourceMember="LastActivityDate"
			ControlId="lblLastActivityDate" BindingMode="OneWay" DisplayFormat="{0:F} (GMT)" />
		<tis:wwDataBindingItem ID="wbi5" runat="server" BindingSource="CurrentUser" BindingSourceMember="LastLoginDate"
			ControlId="lblLastLogOnDate" BindingMode="OneWay" DisplayFormat="{0:F} (GMT)" />
		<tis:wwDataBindingItem ID="wbi6" runat="server" BindingSource="CurrentUser" BindingSourceMember="LastPasswordChangedDate"
			ControlId="lblLastPasswordChangedDate" BindingMode="OneWay" DisplayFormat="{0:F} (GMT)" />
		<tis:wwDataBindingItem ID="wbi7" runat="server" BindingSource="CurrentUser" BindingSourceMember="CreationDate"
			ControlId="lblCreationDate" BindingMode="OneWay" DisplayFormat="{0:F} (GMT)" />
		<tis:wwDataBindingItem ID="wbi8" runat="server" BindingSource="CurrentGalleryProfile" BindingSourceMember="EnableUserAlbum"
			ControlId="chkEnableUserAlbum" BindingProperty="Checked" />
	</DataBindingItems>
</tis:wwDataBinder>
<tis:PopupInfo ID="PopupInfo" runat="server" DialogControlId="dgPopup" DefaultDialogTitleCss="dg5ContentTitleCss"
	DefaultDialogBodyCss="dg5ContentBodyCss">
	<PopupInfoItems>
		<tis:PopupInfoItem ID="poi1" runat="server" ControlId="lblEmail" DialogTitle="<%$ Resources:GalleryServerPro, MyAccount_Email_Address_Hdr %>"
			DialogBody="<%$ Resources:GalleryServerPro, MyAccount_Email_Address_Bdy %>" />
		<tis:PopupInfoItem ID="poi2" runat="server" ControlId="chkEnableUserAlbum" DialogTitle="<%$ Resources:GalleryServerPro, MyAccount_EnableUserAlbum_Hdr %>"
			DialogBody="<%$ Resources:GalleryServerPro, MyAccount_EnableUserAlbum_Bdy %>" />
	</PopupInfoItems>
</tis:PopupInfo>
<uc1:popup ID="ucPopupContainer" runat="server" />
