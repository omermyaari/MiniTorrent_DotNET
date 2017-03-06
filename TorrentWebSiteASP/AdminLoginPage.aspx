<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="AdminLogInPage.aspx.cs" Inherits="TorrentWebSiteASP.AdminLoginPage1" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
    .auto-style1 {
        width: 517px;
    }
        .auto-style2 {
            border-right: 1px solid #3366FF;
            border-top: 1px solid #3366FF;
            border-bottom: 1px solid #3366FF;
            border-left: 4px solid #3366FF;
        }
    </style>
</asp:Content>
<asp:Content ID="AdminLogin" ContentPlaceHolderID="MainContent" runat="Server">
    <fieldset class="auto-style1">
        <legend><h1>Administrator LogIn:</h1></legend>
        <p>Username:
            <asp:TextBox ID="UserName" runat="server" CssClass="auto-style2" Width="225px"></asp:TextBox></p>
        <p>Password:
            <asp:TextBox ID="Password" runat="server" TextMode="Password" CssClass="authTxtBox" Width="225px"></asp:TextBox></p>
        <p>
            <asp:CheckBox ID="RememberMe" runat="server" Text="Remember Me" />&nbsp;</p>
        <p>
            <asp:Button ID="LoginButton" runat="server" Text="Login" OnClick="LoginButton_Click" CssClass =" submitButton"/></p>
        <p>
            <asp:Label ID="InvalidCredentialsMessage" runat="server" ForeColor="Red" Text="Your username or password is invalid. Please try again."
                Visible="False"></asp:Label>&nbsp;
        </p>
    </fieldset>
</asp:Content>
