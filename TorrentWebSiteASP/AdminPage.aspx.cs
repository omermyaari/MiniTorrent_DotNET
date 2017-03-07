using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TorrentWebSiteASP
{
    public partial class AdminLoginPage : System.Web.UI.Page
    {
        private static DAL.Entities.DBPeer peerBeforeUpdate;

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
            List<DAL.Entities.DBFile> files = DAL.DBAccess.SearchFiles("*").Keys.ToList();
          //  List<DAL.Entities.DBFile> files = DAL.DBAccess.GetAllFiles();
                updateTable(files);
        }

        //Set the columns of "Edit" and "Delite" visible true or false.
        private void commandFieldsSetVisible(bool visible)
        {
            regPanel.Visible = visible;
            btnAddPeer.Visible = visible;
            AuthorsGridView.Columns[0].Visible = visible;
            AuthorsGridView.Columns[1].Visible = visible;
        }

        //Inserting elements in the table and refresh.
        private void updateTable(object objects)
        {           
            AuthorsGridView.DataSource = objects;
            AuthorsGridView.DataBind();
        }

        protected void AuthorsGridView_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            AuthorsGridView.EditIndex = -1;
            viewAllPeers();

        }

        protected void AuthorsGridView_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            if(DAL.DBAccess.DeletePeer(AuthorsGridView.Rows[e.RowIndex].Cells[2].Text))
                viewAllPeers();
            //else ERROR MESSAGE
            
        }

        protected void AuthorsGridView_RowEditing(object sender, GridViewEditEventArgs e)
        {
            peerBeforeUpdate = new DAL.Entities.DBPeer
            {
                Name = AuthorsGridView.Rows[e.NewEditIndex].Cells[2].Text.Trim(),
                Password = AuthorsGridView.Rows[e.NewEditIndex].Cells[3].Text,
                Ip = AuthorsGridView.Rows[e.NewEditIndex].Cells[4].Text,
                Port = int.Parse(AuthorsGridView.Rows[e.NewEditIndex].Cells[5].Text)
            };
           
            AuthorsGridView.EditIndex = e.NewEditIndex;
            viewAllPeers();
        }

        protected void AuthorsGridView_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            DAL.Entities.DBPeer newPeer = new DAL.Entities.DBPeer();
            newPeer.Name = ((TextBox)AuthorsGridView.Rows[e.RowIndex].Cells[2].Controls[0]).Text;
            newPeer.Password = ((TextBox)AuthorsGridView.Rows[e.RowIndex].Cells[3].Controls[0]).Text;
            if(!string.IsNullOrEmpty(((TextBox)AuthorsGridView.Rows[e.RowIndex].Cells[4].Controls[0]).Text))
                newPeer.Ip = ((TextBox)AuthorsGridView.Rows[e.RowIndex].Cells[4].Controls[0]).Text;
            if (!string.IsNullOrEmpty(((TextBox)AuthorsGridView.Rows[e.RowIndex].Cells[5].Controls[0]).Text))
                newPeer.Port = int.Parse(((TextBox)AuthorsGridView.Rows[e.RowIndex].Cells[5].Controls[0]).Text);
            DAL.DBAccess.UpdatePeer(peerBeforeUpdate, newPeer);
            AuthorsGridView.EditIndex = -1;
            viewAllPeers();
        }

        protected void btnAddPeer_Click(object sender, EventArgs e)
        {
            DAL.Entities.DBPeer newPeer = new DAL.Entities.DBPeer
            {
                Name = txtUserName.Text,
                Password = txtPwd.Text,
                Ip = txtIP.Text,
                Port = int.Parse(txtPort.Text)
            };
            clearFields();
            if(!DAL.DBAccess.PeerExists(newPeer))
                DAL.DBAccess.RegisterPeer(newPeer);
            viewAllPeers();
        }

        private void clearFields()
        {
            txtUserName.Text = string.Empty;
            txtPwd.Text = string.Empty;
            txtIP.Text = string.Empty;
            txtPort.Text = string.Empty;
        }
    }
}