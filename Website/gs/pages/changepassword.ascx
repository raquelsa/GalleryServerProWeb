<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="changepassword.ascx.cs"
	Inherits="GalleryServerPro.Web.Pages.changepassword" %>
<div class="cpwdcontainer gsp_rounded10">
	<asp:ChangePassword ID="cp1" runat="server" BorderPadding="0" CssClass="cpwd" Width="400"
		CancelButtonText="<%$ Resources:GalleryServerPro, Change_Pwd_CancelButtonText %>"
		ChangePasswordButtonText="<%$ Resources:GalleryServerPro, Change_Pwd_ChangePasswordButtonText %>"
		ChangePasswordFailureText="<%$ Resources:GalleryServerPro, Change_Pwd_ChangePasswordFailureText %>"
		ChangePasswordTitleText="<%$ Resources:GalleryServerPro, Change_Pwd_ChangePasswordTitleText %>"
		ConfirmNewPasswordLabelText="<%$ Resources:GalleryServerPro, Change_Pwd_ConfirmNewPasswordLabelText %>"
		ConfirmPasswordCompareErrorMessage="<%$ Resources:GalleryServerPro, Change_Pwd_ConfirmPasswordCompareErrorMessage %>"
		ConfirmPasswordRequiredErrorMessage="<%$ Resources:GalleryServerPro, Change_Pwd_ConfirmPasswordRequiredErrorMessage %>"
		ContinueButtonText="<%$ Resources:GalleryServerPro, Change_Pwd_ContinueButtonText %>"
		NewPasswordLabelText="<%$ Resources:GalleryServerPro, Change_Pwd_NewPasswordLabelText %>"
		NewPasswordRequiredErrorMessage="<%$ Resources:GalleryServerPro, Change_Pwd_NewPasswordRequiredErrorMessage %>"
		PasswordLabelText="<%$ Resources:GalleryServerPro, Change_Pwd_PasswordLabelText %>"
		PasswordRequiredErrorMessage="<%$ Resources:GalleryServerPro, Change_Pwd_PasswordRequiredErrorMessage %>"
		SuccessText="<%$ Resources:GalleryServerPro, Change_Pwd_SuccessText %>" SuccessTitleText="<%$ Resources:GalleryServerPro, Change_Pwd_SuccessTitleText %>"
		UserNameLabelText="<%$ Resources:GalleryServerPro, Change_Pwd_UserNameLabelText %>"
		UserNameRequiredErrorMessage="<%$ Resources:GalleryServerPro, Change_Pwd_UserNameRequiredErrorMessage %>"
		OnSendingMail="cp1_SendingMail" LabelStyle-CssClass="gsp_cpwdlabel" CancelButtonStyle-CssClass="gsp_cpwdcancelbtn"
		ChangePasswordButtonStyle-CssClass="gsp_cpwdbutton" SuccessTextStyle-CssClass="gsp_cpwdsuccess"
		ContinueButtonStyle-CssClass="gsp_cpwdcontinue" TextBoxStyle-CssClass="gsp_cpwdtxtbox"
		OnCancelButtonClick="cp1_CancelButtonClick">
		<TitleTextStyle CssClass="cpwdTitle gsp_roundedtop6" />
		<FailureTextStyle CssClass="gsp_msgwarning gsp_addbottompadding5" ForeColor="" />
	</asp:ChangePassword>
</div>
