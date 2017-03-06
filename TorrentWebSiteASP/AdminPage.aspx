<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="AdminPage.aspx.cs" Inherits="TorrentWebSiteASP.AdminLoginPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <table class="auto-style5">
            <tr>
                <td class="auto-style4" align ="top">
            <asp:Menu ID="NavigationMenu" runat="server"
                StaticDisplayLevels="2"
                CssClass='float-Right'
                Orientation="Vertical"
                BackColor="#B5C7DE"
                DynamicHorizontalOffset="2"
                Font-Names="Verdana"
                Font-Size="1em"
                ForeColor="#284E98" StaticSubMenuIndent="12px" OnMenuItemClick="NavigationMenu_MenuItemClick">
                <DynamicHoverStyle BackColor="#284E98" ForeColor="White" />
                <DynamicMenuItemStyle HorizontalPadding="5px" VerticalPadding="2px" />
                <DynamicMenuStyle BackColor="#B5C7DE" />
                <DynamicSelectedStyle BackColor="#507CD1" />
                <Items>
                    <asp:MenuItem Text="Users Info" Value="Users Info">
                        <asp:MenuItem Text="All" Value="All"></asp:MenuItem>
                        <asp:MenuItem Text="Online" Value="Online"></asp:MenuItem>
                        <asp:MenuItem Text="Offline" Value="Offline"></asp:MenuItem>
                    </asp:MenuItem>
                    <asp:MenuItem Text="User Management" Value="User Management"></asp:MenuItem>
                    <asp:MenuItem Text="Available Files" Value="Available Files"></asp:MenuItem>
                </Items>
                <StaticHoverStyle BackColor="#284E98" ForeColor="White" />
                <StaticMenuItemStyle HorizontalPadding="5px" VerticalPadding="2px" />
                <StaticSelectedStyle BackColor="#507CD1" />
            </asp:Menu>
                </td>

                <td align="right">
            <asp:GridView ID="AuthorsGridView" Width="600px" OnRowUpdated="AuthorsGridView_RowUpdated" OnRowEditing="AuthorsGridView_RowEditing" OnRowDeleting="AuthorsGridView_RowDeleting" AllowPaging="True" HorizontalAlign="Left"
                CssClass="Grid" AlternatingRowStyle-CssClass="alt" PagerStyle-CssClass="pgr" runat="server" CellPadding="4" ForeColor="#333333" GridLines="None">
                <AlternatingRowStyle CssClass="alt" BackColor="White" ForeColor="#284775"></AlternatingRowStyle>
                                
                <EditRowStyle BackColor="#999999" />
                <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />

                <PagerStyle CssClass="pgr" BackColor="#284775" ForeColor="White" HorizontalAlign="Center"></PagerStyle>
                <RowStyle BackColor="#F7F6F3" ForeColor="#333333" HorizontalAlign="Center" />
                <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
                <SortedAscendingCellStyle BackColor="#E9E7E2" />
                <SortedAscendingHeaderStyle BackColor="#506C8C" />
                <SortedDescendingCellStyle BackColor="#FFFDF8" />
                <SortedDescendingHeaderStyle BackColor="#6F8DAE" />

                <Columns>
                <asp:CommandField ShowEditButton="true" />
                <asp:CommandField ShowDeleteButton="true" />
                </Columns>

                

            </asp:GridView>
                </td>
                <td class="auto-style3"></td>
            </tr>

            <tr>
                <td align ="center" colspan="2">
                    <asp:Button ID="btnAddPeer" runat="server" Visible="false" CssClass ="submitButton" Text="Add Peer" OnClick="btnAddPeer_Click" />
                </td>
            </tr>

            <tr>
                <td> &nbsp;</td>
                <td class="auto-style3"> </td>
            </tr>

            </table>


    <div>

        <div style='float: center;'>
            <br />
        </div>
    </div>
</asp:Content>
