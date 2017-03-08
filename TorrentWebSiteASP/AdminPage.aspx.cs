using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;

namespace TorrentWebSiteASP
{
    public enum FileColomn { Id, Bame, Size };
    public partial class AdminLoginPage : System.Web.UI.Page
    {
        private static DAL.Entities.DBPeer oldPeer;

        private enum PeerColumn { Edit , Delete , Name , Password , IP , Port, IsOnline }
        private enum Validation { NameOrPassword, IP, Port}

        protected void Page_Load(object sender, EventArgs e)
        {}

        //Clicking on menu buttons.
        protected void NavigationMenu_MenuItemClick(object sender, MenuEventArgs e)
        {
            commandFieldsSetVisible(false);
            switch (NavigationMenu.SelectedItem.Text)
            {
                case "Users Info":
                    viewAllPeers();                   
                    break;
                case "All":
                    viewAllPeers();
                    break;
                case "Online":
                    viewAllPeersOnLine(true);
                    break;
                case "Offline":
                    viewAllPeersOnLine(false);
                    break;
                case "User Management":
                    viewAllPeers();
                    commandFieldsSetVisible(true);
                    break;
                case "Available Files":                    
                    viewAllFiles();
                    break;
                default:
                    break;
            }
        }

        //Displays all peers by parameter "isOnline": true = online, false = offline.
        private void viewAllPeersOnLine(bool isOnline)
        {
            List<DAL.Entities.DBPeer> peers = DAL.DBAccess.GetAllPeers();
            List<DAL.Entities.DBPeer> peersOnline = new List<DAL.Entities.DBPeer>();
            List<DAL.Entities.DBPeer> peersOffline = new List<DAL.Entities.DBPeer>();

            for (int i = 0; i < peers.Count; i++)
                if (peers[i].IsOnline)
                    peersOnline.Add(peers[i]);
                else
                    peersOffline.Add(peers[i]);

            if (isOnline) updateTable(peersOnline);
            else updateTable(peersOffline);
        }

        private void viewAllPeers()
        {
            List<DAL.Entities.DBPeer> peers = DAL.DBAccess.GetAllPeers();
                updateTable(peers);
        }

        private void viewAllFiles()
        {
            List<DAL.Entities.DBFile> files = DAL.DBAccess.GetAllFiles();
            updateTable(files);
        }

        //Set the columns of "Edit" and "Delite" visible true or false.
        private void commandFieldsSetVisible(bool visible)
        {
            regPanel.Visible = visible;
            btnAddPeer.Visible = visible;
            MainGridView.Columns[0].Visible = visible;
            MainGridView.Columns[1].Visible = visible;
        }

        //Inserting elements in the table and refresh.
        private void updateTable(object objects)
        {           
            MainGridView.DataSource = objects;
            MainGridView.DataBind();
        }

        //When the editing mode is cancelled.
        protected void MainGridView_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            MainGridView.EditIndex = -1;
            viewAllPeers();
        }

        //When the Peer "Delete" button is clicked.
        protected void MainGridView_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            if (DAL.DBAccess.DeletePeer(MainGridView.Rows[e.RowIndex].Cells[2].Text))
                viewAllPeers();           
        }

        //When the Peer "Edit" button is clicked.
        protected void MainGridView_RowEditing(object sender, GridViewEditEventArgs e)
        {
            oldPeer = new DAL.Entities.DBPeer();

            oldPeer.Name = MainGridView.Rows[e.NewEditIndex].Cells[(int)PeerColumn.Name].Text.Trim();
            oldPeer.Password = MainGridView.Rows[e.NewEditIndex].Cells[(int)PeerColumn.Password].Text;
            oldPeer.Ip = MainGridView.Rows[e.NewEditIndex].Cells[(int)PeerColumn.IP].Text;
            oldPeer.Port = int.Parse(MainGridView.Rows[e.NewEditIndex].Cells[(int)PeerColumn.Port].Text);            
            
            MainGridView.EditIndex = e.NewEditIndex;
            viewAllPeers();
        }

        //When the Peer "Update" button is clicked. 
        protected void MainGridView_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {           
            string name = ((TextBox)MainGridView.Rows[e.RowIndex].Cells[(int)PeerColumn.Name].Controls[0]).Text.Trim();
            string pass = ((TextBox)MainGridView.Rows[e.RowIndex].Cells[(int)PeerColumn.Password].Controls[0]).Text.Trim();
            string ip = ((TextBox)MainGridView.Rows[e.RowIndex].Cells[(int)PeerColumn.IP].Controls[0]).Text.Trim();
            string port = ((TextBox)MainGridView.Rows[e.RowIndex].Cells[(int)PeerColumn.Port].Controls[0]).Text.Trim();

            var newPeer = new DAL.Entities.DBPeer(name, pass, ip, int.Parse(port));
            newPeer.IsOnline = ((CheckBox)MainGridView.Rows[e.RowIndex].Cells[(int)PeerColumn.IsOnline].Controls[0]).Checked;

            if (validData(name, Validation.NameOrPassword) && validData(pass, Validation.NameOrPassword) &&
                validData(ip, Validation.IP) && validData(port, Validation.Port)){
                DAL.DBAccess.UpdatePeer(oldPeer, newPeer);
            }

            MainGridView.EditIndex = -1;
            viewAllPeers();
        }

        //When the button of Adding new peer is clicked.
        protected void btnAddPeer_Click(object sender, EventArgs e)
        {
            DAL.Entities.DBPeer newPeer = new DAL.Entities.DBPeer();
            newPeer.Name = txtUserName.Text;
            newPeer.Password = txtPwd.Text;
            if (txtIP.Text == null)
                newPeer.Ip = txtIP.Text;
            else
                newPeer.Ip = "0.0.0.0";
            if (!String.IsNullOrEmpty(txtPort.Text))
                newPeer.Port = int.Parse(txtPort.Text);
            clearFields();

            if(!DAL.DBAccess.PeerExists(newPeer))
                DAL.DBAccess.RegisterPeer(newPeer);
            viewAllPeers();
        }

        //Clear the textFeields after new Peer creation.
        private void clearFields()
        {
            txtUserName.Text = string.Empty;
            txtPwd.Text = string.Empty;
            txtIP.Text = string.Empty;
            txtPort.Text = string.Empty;
        }

        //Check validation of input data in Peer Row (when updated).
        private bool validData(string data, Validation val)
        {
            if (String.IsNullOrEmpty(data))
                return false;
            switch (val)
            {
                case Validation.NameOrPassword:
                    return Regex.IsMatch(data, @"^[a-zA-Z0-9]{3,10}$");
                case Validation.IP:
                    return Regex.IsMatch(data, @"\b(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b");
                case Validation.Port:
                    return Regex.IsMatch(data, @"^(?:[0-5]?[0-9]{1,4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-6])");
                default:
                    return false;
            }
        }
    }
}