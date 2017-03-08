<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="RegistrationPage.aspx.cs" Inherits="TorrentWebSiteASP.WebForm1" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="head" runat="server"> </asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <fieldset style ="display: inline-block">
        <legend>
            <h1>New Torrent User Registration:</h1>
        </legend>
        <table>
            <tr>
                <td ><h3>User Name:</h3></td>
                <td>
                    <asp:TextBox ID="txtUserNameReg" runat="server" CssClass="authTxtBox">
                    </asp:TextBox>
                </td>
                <td>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="txtUserNameReg"
                        ErrorMessage="Password can't be left blank" SetFocusOnError="True">*
                    </asp:RequiredFieldValidator>
                    <asp:RegularExpressionValidator ID="revUserNameReg" runat="server"
                        ControlToValidate="txtUserNameReg" ErrorMessage="UserName must be 3 to 10 characters!"
                        SetFocusOnError="True" ForeColor="Red" ValidationExpression="^[a-zA-Z0-9]{3,10}$"> 
                    </asp:RegularExpressionValidator>
                </td>
            </tr>

            <tr>
                <td><h3>Password:</h3></td>
                <td>
                    <asp:TextBox ID="txtPwdReg" runat="server" TextMode="Password" CssClass="authTxtBox">
                    </asp:TextBox>
                </td>
                <td>
                    <asp:RequiredFieldValidator ID="rfvPwd" runat="server" ControlToValidate="txtPwdReg"
                        ErrorMessage="Password can't be left blank" SetFocusOnError="True">*
                    </asp:RequiredFieldValidator>
                    <asp:RegularExpressionValidator ID="revPwdReg" runat="server" ControlToValidate="txtPwdReg"
                        ErrorMessage="Password must be 3 to 10 characters!" ForeColor="Red" SetFocusOnError="True" ValidationExpression="^[a-zA-Z0-9]{3,10}$">
                    </asp:RegularExpressionValidator>
                </td>
            </tr>

            <tr>
                <td><h3>Confirm Password:</h3></td>
                <td>
                    <asp:TextBox ID="txtRePwdReg" runat="server" TextMode="Password" CssClass="authTxtBox">
                    </asp:TextBox>
                </td>
                <td>
                    <asp:CompareValidator ID="CompareValidator1"
                        runat="server"
                        ControlToCompare="txtRePwdReg"
                        ControlToValidate="txtPwdReg"
                        Operator="Equal"
                        ErrorMessage="Password and confirm password do not match!"
                        SetFocusOnError="True" Font-Bold="true">
                    </asp:CompareValidator>
                </td>
            </tr>

            <tr>
                <td colspan="2" align="center">
                    <asp:Label ID="lblMsg" runat="server" Font-Size="Large"> </asp:Label>
                </td>
            </tr>

            <tr>
                <td align="center">
                    <asp:Button ID="btnSave" runat="server" CssClass="submitButton"
                        Text="Sign Up"
                        OnClick="btnSave_Click" />
                </td>
            </tr>
        </table>

    </fieldset>


</asp:Content>
