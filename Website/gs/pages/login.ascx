<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="login.ascx.cs" Inherits="GalleryServerPro.Web.Pages.login" %>
<%@ Import Namespace="GalleryServerPro.Web" %>
<asp:Panel ID="pnlInvalidLogin" runat="server" CssClass="gsp_invalidLoginMsg" EnableViewState="false"
	Visible="false" />
<asp:Panel ID="pnlLoginContainer" runat="server" CssClass="gsp_loginContainerPage gsp_rounded10"
	DefaultButton="Login1$LoginButton">
	<p class="gsp_loginTitle gsp_roundedtop6">
		<asp:Literal ID="l1" runat="server" Text="<%$ Resources:GalleryServerPro, Login_Title_Text %>" /></p>
	<img src="<%= Utils.GalleryRoot %>/images/user_48x48.png" alt="" style="width: 48px;
		height: 48px; float: left;" class="gsp_addtoppadding1" />
	<asp:Login ID="Login1" runat="server" CssClass="gsp_login gsp_addbottommargin2" BorderPadding="0"
		RememberMeSet="false" DisplayRememberMe="True" Width="100%" Orientation="Vertical"
		LabelStyle-CssClass="gsp_loginlabel" TextBoxStyle-CssClass="gsp_logintextbox" TextLayout="TextOnLeft"
		TitleText="" LoginButtonType="Button" LoginButtonStyle-CssClass="gsp_loginbutton"
		EnableViewState="true" OnLoggedIn="Login1_LoggedIn" FailureText="<%$ Resources:GalleryServerPro, Login_Failure_Text %>"
		LoginButtonText="<%$ Resources:GalleryServerPro, Login_Button_Text %>" PasswordLabelText="<%$ Resources:GalleryServerPro, Login_Password_Label_Text %>"
		PasswordRequiredErrorMessage="<%$ Resources:GalleryServerPro, Login_Password_Required_Error_Msg %>"
		RememberMeText="<%$ Resources:GalleryServerPro, Login_Remember_Me_Text %>" UserNameLabelText="<%$ Resources:GalleryServerPro, Login_UserName_Label_Text %>"
		UserNameRequiredErrorMessage="<%$ Resources:GalleryServerPro, Login_UserName_Required_Error_Msg %>"
		CheckBoxStyle-CssClass="gsp_rememberme" HyperLinkStyle-CssClass="gsp_loginhyperlinks"
		PasswordRecoveryText="<%$ Resources:GalleryServerPro, Login_Password_Recovery_Text %>">
		<FailureTextStyle CssClass="gsp_msgwarning gsp_addtoppadding2" ForeColor="" />
		<ValidatorTextStyle CssClass="gsp_msgwarning" ForeColor="" />
	</asp:Login>
</asp:Panel>
