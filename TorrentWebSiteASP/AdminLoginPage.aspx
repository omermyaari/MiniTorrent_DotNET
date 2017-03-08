<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="AdminLogInPage.aspx.cs" Inherits="TorrentWebSiteASP.AdminLoginPage1" %>

<asp:Content ID="AdminLoginHead" ContentPlaceHolderID="head" runat="server"> </asp:Content>
<asp:Content ID="AdminLogin" ContentPlaceHolderID="MainContent" runat="Server">

    
    <fieldset style ="display: inline-block">
        <legend><h1>Administrator LogIn:</h1></legend>
        <p><h3>Username: </h3><asp:TextBox ID="UserName" CssClass="authTxtBox" runat="server" Width="225px"></asp:TextBox></p>
        <p><h3>Password: </h3><asp:TextBox ID="Password" CssClass="authTxtBox" runat="server" TextMode="Password" Width="225px"></asp:TextBox></p>
        <p> <asp:CheckBox ID="RememberMe" runat="server" Text="Remember Me" />&nbsp;</p>
        <p> <asp:Button ID="LoginButton" runat="server" Text="Login" OnClick="LoginButton_Click" CssClass =" submitButton"/></p>
        <p> <asp:Label ID="InvalidCredentialsMessage" runat="server" ForeColor="Red" Text="Your username or password is invalid. Please try again."
                Visible="False"></asp:Label>&nbsp;
        </p>
    </fieldset>
</asp:Content>
