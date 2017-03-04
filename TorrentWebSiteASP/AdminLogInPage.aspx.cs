using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TorrentWebSiteASP
{
    public partial class AdminLoginPage1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void LoginButton_Click(object sender, EventArgs e)
        {
            // Three valid username/password pairs: Vit/password, Omer/password.
            string[] users = { "Vit", "Om"};
            string[] passwords = { "pass", "pass" };

            for (int i = 0; i < users.Length; i++)
            {
                bool validUsername = (string.Compare(UserName.Text, users[i], true) == 0);
                bool validPassword = (string.Compare(Password.Text, passwords[i], false) == 0);

                if (validUsername && validPassword)
                {
                    // Create the cookie that contains the forms authentication ticket
                    HttpCookie authCookie = FormsAuthentication.GetAuthCookie(UserName.Text, RememberMe.Checked);

                    // Get the FormsAuthenticationTicket out of the encrypted cookie
                    FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);

                    // Create a new FormsAuthenticationTicket that includes our custom User Data
                    FormsAuthenticationTicket newTicket = new FormsAuthenticationTicket(ticket.Version, ticket.Name, ticket.IssueDate, ticket.Expiration, ticket.IsPersistent, "qwerty");

                    // Update the authCookie's Value to use the encrypted version of newTicket
                    authCookie.Value = FormsAuthentication.Encrypt(newTicket);

                    // Manually add the authCookie to the Cookies collection
                    Response.Cookies.Add(authCookie);

                    // Determine redirect URL and send user there
                    string redirUrl = FormsAuthentication.GetRedirectUrl(UserName.Text, RememberMe.Checked);
                    Response.Redirect(redirUrl);
                }

                // If we reach here, the user's credentials were invalid
                InvalidCredentialsMessage.Visible = true;
            }
        }
    }
}