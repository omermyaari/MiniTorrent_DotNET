<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="SearchPage.aspx.cs" Inherits="TorrentWebSiteASP.SearchPage" %>

<asp:Content ID="SearchHead" ContentPlaceHolderID="head" runat="server"> </asp:Content>
<asp:Content ID="SearchFiles" ContentPlaceHolderID="MainContent" runat="Server">
    <div align="center">
        <h2>
            <asp:Label ID="TotalPeersLbl" runat="server" Text="Total peers: "></asp:Label>
            <asp:Label ID="totalPeers" runat="server" Text=""></asp:Label></h2>
    </div>
    <div align="center">
        <h2>
            <asp:Label ID="OnLinePeersLbl" runat="server" Text="Peers online: "></asp:Label>
            <asp:Label ID="onLinePeers" runat="server" Text=""></asp:Label></h2>
    </div>
    <div align="center">
        <h2>
            <asp:Label ID="FilesAvailableLbl" runat="server" Text="Files Available: "></asp:Label>
            <asp:Label ID="filesAvailable" runat="server" Text=""></asp:Label></h2>
    </div>
    <fieldset style ="display: inline-block">
        <legend>
            <h1>Search Files:</h1>
        </legend>

        <table>
            <tr>
                <td align="center">
                    <h3>File Name:</h3>
                    <asp:TextBox ID="FileName" CssClass="authTxtBox" runat="server" Width="225px"></asp:TextBox>
                    <br />
                    <asp:RequiredFieldValidator ID="rfvFileName" runat="server" ControlToValidate="FileName"
                        ForeColor="Red" SetFocusOnError="True">File Name can't be left blank!
                    </asp:RequiredFieldValidator>
                    <br />
                    <asp:Button ID="btnSearch" runat="server" CssClass="submitButton"
                        Text="Search"
                        OnClick="btnSearch_Click" />

                </td>
                <td style="margin-left: 45px">

                    <asp:GridView ID="FilesGridView" runat="server" ShowHeaderWhenEmpty="true" BackColor="White" BorderColor="#E7E7FF" BorderStyle="None" BorderWidth="1px" CellPadding="3" GridLines="Horizontal">
                        <AlternatingRowStyle BackColor="#F7F7F7" />
                        <FooterStyle BackColor="#B5C7DE" ForeColor="#4A3C8C" />
                        <HeaderStyle BackColor="#4A3C8C" Font-Bold="True" ForeColor="#F7F7F7" />
                        <PagerStyle BackColor="#E7E7FF" ForeColor="#4A3C8C" HorizontalAlign="Right" />
                        <RowStyle BackColor="#E7E7FF" ForeColor="#4A3C8C" />
                        <SelectedRowStyle BackColor="#738A9C" Font-Bold="True" ForeColor="#F7F7F7" />
                        <SortedAscendingCellStyle BackColor="#F4F4FD" />
                        <SortedAscendingHeaderStyle BackColor="#5A4C9D" />
                        <SortedDescendingCellStyle BackColor="#D8D8F0" />
                        <SortedDescendingHeaderStyle BackColor="#3E3277" />
                    </asp:GridView>


                </td>
            </tr>

        </table>

        &nbsp;</p>
        

    </fieldset>
</asp:Content>
