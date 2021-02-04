using System;
using System.Collections.Generic;
using System.Web;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Script.Serialization;
using System.Net;
using System.IO;
using System.Timers;

namespace SITConnect
{
    public partial class Login : System.Web.UI.Page
    {
        string MYDBConnectionString = ConfigurationManager.ConnectionStrings["ASDB"].ConnectionString;
        public string success { get; set; }
        public List<string> ErrorMessage { get; set; }
        protected void Page_Load(object sender, EventArgs e)
        {
        
        }
        // Login button click event handler
        protected void Login_Click(object sender, EventArgs e)
        {
            string errorMsg = "";
            string email = HttpUtility.HtmlEncode(emailAddr.Text.ToString());
            string pwd = HttpUtility.HtmlEncode(pwdInput.Text.ToString());
            bool emailExists = EmailCheck(emailAddr.Text.ToString());
            if (ValidateCaptcha())
            {
                if (emailExists)
                {
                    string status = getLockStatus(emailAddr.Text.ToString());
                    if (status == "F")
                    {
                        // Comparing of hash & salts start here
                        SHA512Managed hashing = new SHA512Managed();
                        string dbHash = getDBHash(email);
                        string dbSalt = getDBSalt(email);
                        try
                        {
                            // If salt and hash exists in database
                            if (dbSalt != null && dbSalt.Length > 0 && dbHash != null && dbHash.Length > 0)
                            {
                                string pwdwithSalt = pwd + dbSalt;
                                byte[] hashwithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdwithSalt));
                                string userhash = Convert.ToBase64String(hashwithSalt);
                                // Password with Hash matches
                                if (userhash.Equals(dbHash))
                                {
                                    Session["LoggedIn"] = email;
                                    //Create a new GUID and save to session as AuthToken
                                    string guid = Guid.NewGuid().ToString();
                                    Session["AuthToken"] = guid;

                                    //Create a new cookie with this guid value
                                    Response.Cookies.Add(new HttpCookie("AuthToken", guid));
                                    Response.Redirect("Profile.aspx", false);
                                }                               
                                // else if not locked and have not reached 3 attempts
                                else
                                {
                                    int old_count = GetAttemptCount(email); //2
                                    PlusAttemptCount(email, old_count); // email, 2 -> counter become 3 
                                    int new_count = GetAttemptCount(email); // 3
                                    if (new_count == 3)
                                    {
                                        errorMsg = "Your account has been temporarily locked due to three invalid login attempts.";
                                        SetLockStatus(email);
                                        // Setting Start & End Lock Times
                                        DateTime startLock = DateTime.Now;
                                        DateTime endLock = startLock.AddMinutes(1);
                                        SetStartTime(email, startLock);
                                        SetEndTime(email, endLock);
                                    }
                                    else
                                    {                                                         
                                        errorMsg = $"Email or password is invalid. Attempt Count:{new_count}";
                                    }
                                    errorOrSuccess.Text = errorMsg;
                                    errorOrSuccess.ForeColor = Color.Red;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.ToString());
                        }
                        finally { }
                    }
                    else if (status == "T")
                    {
                        DateTime endtime = GetEndLockTime(email);
                        TimeSpan diff = endtime.Subtract(DateTime.Now);
                        if (diff <= TimeSpan.Zero)
                        {
                            SetLockStatusFalse(email);
                            errorOrSuccess.Text = "Your account is unlocked now";
                            errorOrSuccess.ForeColor = Color.Green;
                        }
                        else
                        {
                            errorOrSuccess.Text = $"Your account is locked. You have {diff.ToString("%m")} minutes and {diff.ToString("%s")} seconds left before your account is unlocked.";
                            errorOrSuccess.ForeColor = Color.Red;
                        }                                            
                    }
                }
                else
                {
                    errorOrSuccess.Text = "Your email is not registered.";
                    errorOrSuccess.ForeColor = Color.Red;
                }
            }
            else
            {
                errorOrSuccess.Text = "There was an error.";
                errorOrSuccess.ForeColor = Color.Red;
            }   
        }
        protected string getDBHash(string email)
        {
            string h = null;
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select PasswordHash FROM Account WHERE Email=@Email";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", email);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        if (reader["PasswordHash"] != null)
                        {
                            if (reader["PasswordHash"] != DBNull.Value)
                            {
                                h = reader["PasswordHash"].ToString();
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { connection.Close(); }
            return h;
        }
        protected string getDBSalt(string email)
        {
            string s = null;
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select PasswordSalt FROM Account WHERE Email=@Email";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", email);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["PasswordSalt"] != null)
                        {
                            if (reader["PasswordSalt"] != DBNull.Value)
                            {
                                s = reader["PasswordSalt"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { connection.Close(); }
            return s;
        }
        protected string getLockStatus(string email)
        {
            string s = null;
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select LockStatus FROM Account WHERE Email=@Email";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", email);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["LockStatus"] != null)
                        {
                            if (reader["LockStatus"] != DBNull.Value)
                            {
                                s = reader["LockStatus"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { connection.Close(); }
            return s;
        }
        private void SetLockStatus(string email)
        {
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "UPDATE Account SET LockStatus=@LockStatus, AttemptCount=@AttemptCount WHERE Email=@Email";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@LockStatus", "T");
            command.Parameters.AddWithValue("@AttemptCount", 0);
            command.Parameters.AddWithValue("@Email", email);
            command.Connection = connection;
            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
        }
        private void SetLockStatusFalse(string email)
        {
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "UPDATE Account SET LockStatus=@LockStatus, AttemptCount=@AttemptCount WHERE Email=@Email";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@LockStatus", "F");
            command.Parameters.AddWithValue("@AttemptCount", 0);
            command.Parameters.AddWithValue("@Email", email);
            command.Connection = connection;
            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
        }
        private bool EmailCheck(string email)
        {
            SqlConnection con = new SqlConnection(MYDBConnectionString);
            SqlCommand cmd = new SqlCommand("Select * from Account where Email= @Email", con);
            cmd.Parameters.AddWithValue("@Email", email);
            con.Open();
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                // Make sure that email has a row in the database (exists)
                if (dr.HasRows == true)
                {
                    return true;
                }
            }
            return false;
        }
        private int GetAttemptCount(string email)
        {
            // To get old attempt count
            int count = 0;            
            SqlConnection con = new SqlConnection(MYDBConnectionString);
            SqlCommand cmd = new SqlCommand("Select AttemptCount from Account where Email= @Email", con);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Connection = con;
            try
            {
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["AttemptCount"] != null)
                        {
                            if (reader["AttemptCount"] != DBNull.Value)
                            {
                                count = Convert.ToInt32(reader["AttemptCount"]);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { con.Close(); }
            return count;
        }
        private void PlusAttemptCount(string email, int old_count)
        {
            int new_count = old_count + 1;
            SqlConnection con = new SqlConnection(MYDBConnectionString);
            SqlCommand cmd = new SqlCommand("UPDATE Account SET AttemptCount=@Count WHERE Email= @Email", con);
            cmd.Parameters.AddWithValue("@Count", new_count);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Connection = con;
            try
            {
                con.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { con.Close(); }
        }
        private void SetStartTime(string email, DateTime startTime)
        {
            SqlConnection con = new SqlConnection(MYDBConnectionString);
            SqlCommand cmd = new SqlCommand("UPDATE Account SET StartLockTime=@StartLock WHERE Email= @Email", con);
            cmd.Parameters.AddWithValue("@StartLock", startTime);            
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Connection = con;
            try
            {
                con.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { con.Close(); }
        }
        private void SetEndTime(string email, DateTime endTime)
        {
            SqlConnection con = new SqlConnection(MYDBConnectionString);
            SqlCommand cmd = new SqlCommand("UPDATE Account SET EndLockTime=@EndLock WHERE Email= @Email", con);
            cmd.Parameters.AddWithValue("@EndLock", endTime);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Connection = con;
            try
            {
                con.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { con.Close(); }
        }
        private DateTime GetStartLockTime(string email)
        {
            string t = null;
            SqlConnection con = new SqlConnection(MYDBConnectionString);
            SqlCommand cmd = new SqlCommand("SELECT StartLockTime FROM Account WHERE Email= @Email", con);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Connection = con;
            try
            {
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["StartLockTime"] != null)
                        {
                            if (reader["StartLockTime"] != DBNull.Value)
                            {
                                t = reader["StartLockTime"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { con.Close(); }
            DateTime startTime = Convert.ToDateTime(t);
            return startTime;
        }
        private DateTime GetEndLockTime(string email)
        {
            string t = null;
            SqlConnection con = new SqlConnection(MYDBConnectionString);
            SqlCommand cmd = new SqlCommand("SELECT EndLockTime FROM Account WHERE Email= @Email", con);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Connection = con;
            try
            {
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["EndLockTime"] != null)
                        {
                            if (reader["EndLockTime"] != DBNull.Value)
                            {
                                t = reader["EndLockTime"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { con.Close(); }
            DateTime endTime = Convert.ToDateTime(t);
            return endTime;
        }
        private bool ValidateCaptcha()
        {
            bool result = true;
            string captchaResponse = Request.Form["g-recaptcha-response"];
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://www.google.com/recaptcha/api/siteverify?secret=6LcB7j8aAAAAAHWlMJ4ILbSmsh7aiP6qoSwHWTJJ &response=" + captchaResponse);

            try
            {
                using (WebResponse wResponse = req.GetResponse())
                {
                    using (StreamReader readStream = new StreamReader(wResponse.GetResponseStream()))
                    {
                        string jsonResponse = readStream.ReadToEnd();

                        JavaScriptSerializer js = new JavaScriptSerializer();

                        Login jsonObject = js.Deserialize<Login>(jsonResponse);
                        result = Convert.ToBoolean(jsonObject.success);
                    }
                }
                return result;
            }
            catch (WebException ex)
            {
                throw ex;
            }
        }
    }
}