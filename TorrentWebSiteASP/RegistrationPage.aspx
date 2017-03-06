<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="RegistrationPage.aspx.cs" Inherits="TorrentWebSiteASP.WebForm1" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .auto-style1 {
            width: 653px;
            height: 289px;
        }
        .auto-style3 {
            width: 226px;
        }
        .auto-style4 {
            width: 304px;
        }
        .auto-style5 {
            height: 200px;
            width: 610px;
        }
        .auto-style6 {
            border-right: 1px solid #3366FF;
            border-top: 1px solid #3366FF;
            border-bottom: 1px solid #3366FF;
            border-left: 4px solid #3366FF;
        }
    </style>
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">


    <fieldset class="auto-style1">
        <legend>
            <h1>New Torrent User Registration:</h1>
        </legend>
        <table class="auto-style5">
            <tr>
                <td class="auto-style4">UserName:</td>
                <td>
                    <asp:TextBox ID="txtUserName" runat="server" Width="230px" CssClass="auto-style6"></asp:TextBox>
                </td>
                <td class="auto-style3">
                    <asp:RequiredFieldValidator
                        ID="rfvUserName"
                        runat="server"
                        ControlToValidate="txtUserName"
                        ErrorMessage="UserName can't be left blank"
                        SetFocusOnError="True">*
                    </asp:RequiredFieldValidator>
                </td>
            </tr>

            <tr>
                <td class="auto-style4">Password:</td>
                <td>
                    <asp:TextBox ID="txtPwd" runat="server" TextMode="Password" CssClass="authTxtBox">
                    </asp:TextBox>
                </td>
                <td class="auto-style3">
                    <asp:RequiredFieldValidator ID="rfvPwd"
                        runat="server" ControlToValidate="txtPwd"
                        ErrorMessage="Password can't be left blank"
                        SetFocusOnError="True">*
                    </asp:RequiredFieldValidator>
                </td>
            </tr>

            <tr>
                <td class="auto-style4">Confirm Password:</td>
                <td>
                    <asp:TextBox ID="txtRePwd" runat="server" TextMode="Password" CssClass="authTxtBox">
                    </asp:TextBox>
                </td>
                <td class="auto-style3">
                    <asp:CompareValidator ID="CompareValidator1"
                        runat="server"
                        ControlToCompare="txtRePwd"
                        ControlToValidate="txtPwd"
                        Operator="Equal"
                        ErrorMessage="Password and confirm password do not match!"
                        SetFocusOnError="True" Font-Bold ="true"
                        >
                    </asp:CompareValidator>
                </td>
            </tr>

            <tr>
                <td colspan="2" align ="center">
                    <asp:Label ID="lblMsg" runat="server" Font-Size="Large">
                    </asp:Label>
                </td>
            </tr>

            <tr>
                <td class="auto-style4" align ="center">
                    <asp:Button ID="btnSave" runat="server" CssClass ="submitButton"
                        Text="Sign Up"
                        OnClick="btnSave_Click" />
                </td>
            </tr>
        </table>

    </fieldset>


</asp:Content>
