using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Linq.Expressions;

namespace SaleProject
{
    public partial class _Default : System.Web.UI.Page
    {
        SaleService.SaleServiceClient proxy;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                 proxy=new SaleService.SaleServiceClient();
                GridView1.DataSource=proxy.GetAllCustomer();
                GridView1.DataBind();
            }

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            proxy = new SaleService.SaleServiceClient();
            SaleService.Customer objcust = new SaleService.Customer() { CustomerID=5, CustomerName=TextBox1.Text,
            Address=TextBox2.Text,EmailId=TextBox3.Text  };

            proxy.InsertCustomer(objcust);

            GridView1.DataSource = proxy.GetAllCustomer();
            GridView1.DataBind();
            Label1.Text = "Record Saved Successfully";
        }

        protected void GridView1_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int userid = Convert.ToInt32(GridView1.DataKeys[e.RowIndex].Values["CustomerID"].ToString());
            proxy = new SaleService.SaleServiceClient();

            bool check = proxy.DeleteCustomer(userid);
              Label1.Text = "Record Deleted Successfully";
              GridView1.DataSource = proxy.GetAllCustomer();
              GridView1.DataBind();
        }

        public SortDirection GridViewSortDirection
        {
            get
            {
                if (ViewState["sortDirection"] == null)
                    ViewState["sortDirection"] = SortDirection.Ascending;

                return (SortDirection)ViewState["sortDirection"];
            }
            set { ViewState["sortDirection"] = value; }
        }

        protected void GridView1_Sorting(object sender, GridViewSortEventArgs e)
        {
           
            
        }

        protected void TextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
