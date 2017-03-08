using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TorrentWebSiteASP
{
    public partial class SearchPage : System.Web.UI.Page
    {
        private List<DAL.Entities.DBPeer> peersFromDB = new List<DAL.Entities.DBPeer>();
        private int onlinePeersCounter = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            peersFromDB = DAL.DBAccess.GetAllPeers();
            foreach (var peer in peersFromDB)
                if (peer.IsOnline == true)
                    onlinePeersCounter++;

            totalPeers.Text = peersFromDB.Count.ToString();       
            onLinePeers.Text = onlinePeersCounter.ToString();
            filesAvailable.Text = DAL.DBAccess.SearchFiles("*").Keys.Count.ToString();
            //filesAvailable.Text = DAL.DBAccess.GetAllFiles().Count.ToString();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            List<DAL.Entities.DBFile> files = DAL.DBAccess.SearchFiles(FileName.Text).Keys.ToList();
            updateTable(files);
        }

        //Inserting elements in the table and refresh.
        private void updateTable(object objects)
        {
            FilesGridView.DataSource = objects;
            FilesGridView.DataBind();
        }
    }
}