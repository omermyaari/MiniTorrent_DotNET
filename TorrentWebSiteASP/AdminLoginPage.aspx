﻿<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="AdminLogInPage.aspx.cs" Inherits="TorrentWebSiteASP.AdminLoginPage1" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="AdminLogin" ContentPlaceHolderID="MainContent" runat="Server">
    <fieldset class="register">
        <legend>Account Information</legend>
        <h1>Administrator Login:</h1>
        <p>Username:
            <asp:TextBox ID="UserName" runat="server"></asp:TextBox></p>
        <p>Password:
            <asp:TextBox ID="Password" runat="server" TextMode="Password"></asp:TextBox></p>
        <p>
            <asp:CheckBox ID="RememberMe" runat="server" Text="Remember Me" />&nbsp;</p>
        <p>
            <asp:Button ID="LoginButton" runat="server" Text="Login" OnClick="LoginButton_Click" /></p>
        <p>
            <asp:Label ID="InvalidCredentialsMessage" runat="server" ForeColor="Red" Text="Your username or password is invalid. Please try again."
                Visible="False"></asp:Label>&nbsp;
        </p>
    </fieldset>
</asp:Content>
