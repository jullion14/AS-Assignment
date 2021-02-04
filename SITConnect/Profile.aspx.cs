using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Web.Configuration;
using System.Data.SqlClient;

namespace SITConnect
{
    public partial class Profile : System.Web.UI.Page
    {
        string MYDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ASDB"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            if (Session["LoggedIn"] != null && Session["AuthToken"] != null && Request.Cookies["AuthToken"] != null)
            {
                if (!Session["AuthToken"].ToString().Equals(Request.Cookies["AuthToken"].Value))
                {
                    Response.Redirect("Login.aspx", false);
                }
                else
                {
                    // Login Successful
                    if (!Page.IsPostBack)
                    {                        
                        Configuration con = WebConfigurationManager.OpenWebConfiguration("~/Web.Config/");
                        SessionStateSection section = (SessionStateSection)con.GetSection("system.web/sessionState");
                        int timeout = (int)section.Timeout.TotalMinutes * 1000 * 60;
                        ClientScript.RegisterStartupScript(GetType(), "sessionAlert", "sessionExpireAlert(" + timeout + ");", true);
                    }
                    string fullName = getNamebyEmail(Session["LoggedIn"].ToString());
                    username.Text = fullName;
                    Page.Title = $"{fullName} | SITConnect";
                }
            }
            else
            {
                Response.Redirect("Login.aspx", false);
            }
        }
        protected void Logout(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Session.RemoveAll();

            Response.Redirect("Login.aspx", false);

            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddMonths(-20);
            }
            if (Request.Cookies["AuthToken"] != null)
            {
                Response.Cookies["AuthToken"].Value = string.Empty;
                Response.Cookies["AuthToken"].Expires = DateTime.Now.AddMonths(-20);
            }
        }
        protected string getNamebyEmail(string email)
        {
            string fullName = "";
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "SELECT FirstName, LastName FROM Account WHERE Email=@Email";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", Session["LoggedIn"]);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["FirstName"] != null && reader["LastName"] != null)
                        {
                            if (reader["FirstName"] != DBNull.Value && reader["LastName"] != DBNull.Value)
                            {
                                fullName = reader["FirstName"].ToString() + " " + reader["LastName"].ToString();
                            }
                        }
                    }
                }
            }catch(Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { connection.Close(); }
            return fullName;
        }

        protected void update_pwd(object sender, EventArgs e)
        {
            Response.Redirect("ChangePassword.aspx", false);
        }
    }
}