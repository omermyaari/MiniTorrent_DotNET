using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TorrentWebSiteASP
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        { }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (txtUserName.Text.Length>=3 && txtUserName.Text.Length <=10 &&
                txtPwd.Text.Length >= 3 && txtPwd.Text.Length <= 10)
            {
                DAL.Entities.DBPeer peer = new DAL.Entities.DBPeer(txtUserName.Text, txtPwd.Text, "", 0);
                if (!DAL.DBAccess.PeerExists(peer))
                {                   
                    DAL.DBAccess.RegisterPeer(new DAL.Entities.DBPeer(txtUserName.Text, txtPwd.Text, "", 0));
                    lblMsg.ForeColor = System.Drawing.Color.Green;
                    lblMsg.Text = "User Registration successful";
                }
                else
                {
                    lblMsg.ForeColor = System.Drawing.Color.Red;
                    lblMsg.Text = "The User already exists!";
                }
            }
            else
            {
                lblMsg.ForeColor = System.Drawing.Color.Red;
                lblMsg.Text = "The User Name and Password must be 3 to 10 characters Length!";
            }         
        }
    }
}