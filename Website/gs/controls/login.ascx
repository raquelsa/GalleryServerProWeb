<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="login.ascx.cs" Inherits="GalleryServerPro.Web.Controls.login" %>
<%@ Register Namespace="ComponentArt.Web.UI" Assembly="ComponentArt.Web.UI" TagPrefix="ComponentArt" %>
<%@ Import Namespace="GalleryServerPro.Web" %>
<div class="gsp_loginContainerUC">
	<asp:LoginView ID="lv" runat="server" EnableViewState="false">
		<LoggedInTemplate>
			<p class="loggedonview gsp_useroption">
				<asp:LoginName ID="LoginName1" runat="server" CssClass="gsp_welcome" EnableViewState="false" FormatString="<%$ Resources:GalleryServerPro, Login_Logged_On_Msg %>" />
				<asp:LoginStatus ID="LoginStatus1" runat="server" LogoutImageUrl="<%= Utils.GalleryRoot %>/images/user_48x48.png" EnableViewState="false" OnLoggedOut="LoginStatus1_LoggedOut"
					LoginText="<%$ Resources:GalleryServerPro, Login_Button_Text %>" LogoutText="<%$ Resources:GalleryServerPro, Login_Logout_Text %>"
					ToolTip="<%$ Resources:GalleryServerPro, Login_Logout_Tooltip %>" BorderWidth="1" />
			</p>
		</LoggedInTemplate>
		<AnonymousTemplate>
			<p>
				<a id="l1" href="javascript:toggleLogin();" class="gsp_addrightmargin3">
					<asp:Literal runat="server" Text="<%$ Resources:GalleryServerPro, Login_Button_Text %>" /></a></p>
			<ComponentArt:Dialog ID="dgLogin" runat="server" CloseTransition="Fade" ShowTransition="Fade"
				AnimationPath="Direct" AlignmentElement="l1" TransitionDuration="100" OffsetX="-371"
				OffsetY="40" HeaderCssClass="gsp_invisible" CssClass="dgLogin gsp_ns gsp_rounded10">
				<ContentTemplate>
					<p class="gsp_textright">
						<img onclick="dgLogin.Close('cancelled');" src="<%= Utils.GalleryRoot %>/images/componentart/dialog/close.gif"
							alt="Close" /></p>
					<img src="<%= Utils.GalleryRoot %>/images/user_48x48.png" alt="" style="width: 48px;
						height: 48px; float: left;" class="gsp_addtoppadding1" />
					<asp:Login ID="Login1" runat="server" CssClass="gsp_login" RememberMeSet="false"
						DisplayRememberMe="True" Width="100%" Orientation="Vertical" LabelStyle-CssClass="gsp_loginlabel"
						TextBoxStyle-CssClass="gsp_logintextbox" TextLayout="TextOnLeft" TitleText="" LoginButtonType="Button"
						LoginButtonStyle-CssClass="gsp_loginbutton" EnableViewState="false" OnLoginError="Login1_LoginError"
						OnLoggedIn="Login1_LoggedIn" FailureText="<%$ Resources:GalleryServerPro, Login_Failure_Text %>"
						LoginButtonText="<%$ Resources:GalleryServerPro, Login_Button_Text %>" PasswordLabelText="<%$ Resources:GalleryServerPro, Login_Password_Label_Text %>"
						PasswordRequiredErrorMessage="" RememberMeText="<%$ Resources:GalleryServerPro, Login_Remember_Me_Text %>"
						UserNameLabelText="<%$ Resources:GalleryServerPro, Login_UserName_Label_Text %>"
						UserNameRequiredErrorMessage="" CheckBoxStyle-CssClass="gsp_rememberme" HyperLinkStyle-CssClass="gsp_loginhyperlinks"
						PasswordRecoveryText="<%$ Resources:GalleryServerPro, Login_Password_Recovery_Text %>">
					</asp:Login>
				</ContentTemplate>
				<ClientEvents>
					<OnShow EventHandler="dgLogin_OnShow" />
				</ClientEvents>
			</ComponentArt:Dialog>
		</AnonymousTemplate>
	</asp:LoginView>
</div>
