<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="createaccount.ascx.cs"	Inherits="GalleryServerPro.Web.Pages.createaccount" %>
<%@ Register TagPrefix="ComponentArt" Namespace="ComponentArt.Web.UI" Assembly="ComponentArt.Web.UI" %>
<%@ Import Namespace="GalleryServerPro.Web" %>
<asp:Panel ID="pnlMsgContainer" runat="server" Visible="false" />
<asp:Panel ID="pnlCreateUser" runat="server" CssClass="gsp_createuser gsp_rounded10"
	DefaultButton="btnCreateAccount">
	<p class="gsp_h1">
		<img src="<%= Utils.GalleryRoot %>/images/user_48x48.png" alt="" style="width: 48px;
			height: 48px; vertical-align: middle;" /><asp:Literal ID="l1" runat="server" Text="<%$ Resources:GalleryServerPro, CreateAccount_Header %>" /></p>
	<p class="gsp_addleftpadding1">
		<asp:Label ID="lblInstructions" runat="server" /></p>
	<table>
		<tr style="height: 60px;">
			<td class="gsp_col1">
				<asp:Literal ID="l2" runat="server" Text="<%$ Resources:GalleryServerPro, CreateAccount_Username_Header_Text %>" /><span
					class="gsp_fs gsp_msgwarning">*</span>
			</td>
			<td>
				<asp:TextBox ID="txtNewUserUserName" runat="server" onchange="validateNewUserName(this);"></asp:TextBox>
				<asp:RequiredFieldValidator ID="rfvUsername" runat="server" CssClass="gsp_msgwarning"
					ForeColor="" ControlToValidate="txtNewUserUsername" ErrorMessage="<%$ Resources:GalleryServerPro, Site_Field_Required_Text %>"
					SetFocusOnError="True"></asp:RequiredFieldValidator>
				<ComponentArt:CallBack ID="cbValidateNewUserName" runat="server" CssClass="gsp_addtopmargin1"
					OnCallback="cbValidateNewUserName_Callback">
					<Content>
						<asp:Label ID="lblUserNameValidationResult" runat="server" /></Content>
				</ComponentArt:CallBack>
			</td>
		</tr>
		<tr id="trEmail" runat="server">
			<td class="gsp_col1">
				<asp:Literal ID="l3" runat="server" Text="<%$ Resources:GalleryServerPro, CreateAccount_Email_Header_Text %>" /><asp:Label
					ID="lblEmailReqd" runat="server" Visible="false" CssClass="gsp_fs gsp_msgwarning"
					Text="*" />
			</td>
			<td>
				<asp:TextBox ID="txtNewUserEmail" runat="server" />
				<asp:RequiredFieldValidator ID="rfvEmail" runat="server" Enabled="false" CssClass="gsp_msgwarning"
					ForeColor="" Display="Dynamic" ControlToValidate="txtNewUserEmail" ErrorMessage="<%$ Resources:GalleryServerPro, Site_Field_Required_Text %>"
					SetFocusOnError="True" />
				<asp:RegularExpressionValidator ID="revEmail" runat="server" Display="Dynamic" CssClass="gsp_msgwarning"
					ForeColor="" ControlToValidate="txtNewUserEmail" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
					SetFocusOnError="true" ErrorMessage="<%$ Resources:GalleryServerPro, Site_Invalid_Text %>" />
			</td>
		</tr>
		<tr>
			<td class="gsp_col1">
				<asp:Literal ID="l4" runat="server" Text="<%$ Resources:GalleryServerPro, CreateAccount_Password_Header_Text %>" /><span
					class="gsp_fs gsp_msgwarning">*</span>
			</td>
			<td>
				<asp:TextBox ID="txtNewUserPassword1" runat="server" TextMode="Password"></asp:TextBox>
				<asp:RequiredFieldValidator ID="rfvPassword1" runat="server" CssClass="gsp_msgwarning"
					ForeColor="" ControlToValidate="txtNewUserPassword1" ErrorMessage="<%$ Resources:GalleryServerPro, Site_Field_Required_Text %>"
					SetFocusOnError="True"></asp:RequiredFieldValidator>
			</td>
		</tr>
		<tr>
			<td class="gsp_col1">
				<asp:Literal ID="l5" runat="server" Text="<%$ Resources:GalleryServerPro, CreateAccount_Confirm_Password_Header_Text %>" /><span
					class="gsp_fs gsp_msgwarning">*</span>
			</td>
			<td>
				<asp:TextBox ID="txtNewUserPassword2" runat="server" TextMode="Password"></asp:TextBox>
				<asp:RequiredFieldValidator ID="rfvPassword2" runat="server" CssClass="gsp_msgwarning"
					ForeColor="" ControlToValidate="txtNewUserPassword2" Display="Dynamic" ErrorMessage="<%$ Resources:GalleryServerPro, Site_Field_Required_Text %>"></asp:RequiredFieldValidator>
				<asp:CompareValidator ID="cvPasswordsEqual" runat="server" CssClass="gsp_msgwarning"
					ForeColor="" ControlToCompare="txtNewUserPassword1" ControlToValidate="txtNewUserPassword2"
					Display="Dynamic" ErrorMessage="<%$ Resources:GalleryServerPro, CreateAccount_Passwords_Not_Matching_Error %>"
					SetFocusOnError="True"></asp:CompareValidator>
			</td>
		</tr>
		<tr>
			<td colspan="2">
				<p class="gsp_fs gsp_msgwarning">
					<asp:Literal ID="l6" runat="server" Text="<%$ Resources:GalleryServerPro, Site_Field_Required_Text %>" /></p>
			</td>
		</tr>
	</table>
	<p class="gsp_rightBottom">
		<asp:Button ID="btnCreateAccount" runat="server" Text="<%$ Resources:GalleryServerPro, Login_Create_Account_Text %>"
			OnClick="btnCreateAccount_Click" /></p>
</asp:Panel>
