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
            DAL.Entities.DBPeer peer = new DAL.Entities.DBPeer(txtUserNameReg.Text, txtPwdReg.Text, "0.0.0.0", 0);
                if (!DAL.DBAccess.PeerExists(peer))
                {                   
                    DAL.DBAccess.RegisterPeer(new DAL.Entities.DBPeer(txtUserNameReg.Text, txtPwdReg.Text, "", 0));
                    lblMsg.ForeColor = System.Drawing.Color.Green;
                    lblMsg.Text = "User Registration successful";
                }
                else
                {
                    lblMsg.ForeColor = System.Drawing.Color.Red;
                    lblMsg.Text = "The User already exists!";
                }       
        }
    }
}