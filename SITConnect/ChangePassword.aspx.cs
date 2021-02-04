using System;
using System.Collections.Generic;
using System.Web;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Web.Script.Serialization;
using System.Net;
using System.IO;
using System.Timers;

namespace SITConnect
{
    public partial class ChangePassword : System.Web.UI.Page
    {
        string MYDBConnectionString = ConfigurationManager.ConnectionStrings["ASDB"].ConnectionString;
        static string finalHash;
        static string salt;
        byte[] Key;
        byte[] IV;
        protected void Page_Load(object sender, EventArgs e)
        {           
            /*if (Session["LoggedIn"] == null || Session["AuthToken"] == null || Request.Cookies["AuthToken"] == null)
            {
               
                   Response.Redirect("Login.aspx", false);
                
            }*/
        }
        
        public bool EmailCheck(string email)
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

        protected void change_pwd_Click(object sender, EventArgs e)
        {
            bool exists = EmailCheck(emailAddr.Text);
          
            if (oldpass.Text.Trim() == newpass.Text.Trim())
            {
                errorOrSuccess.Text = "New password and old password must be different";
                errorOrSuccess.ForeColor = Color.Red;
                return;
            }
            else
            {
                if (!exists)
                {
                    errorOrSuccess.Text = "Your email doesn't exist in the database";
                    errorOrSuccess.ForeColor = Color.Red;
                    return;

                }
                else if (exists)
                {
                    SHA512Managed hashing = new SHA512Managed();
                    string dbHash = getDBHash(HttpUtility.HtmlEncode(emailAddr.Text.Trim()));
                    string dbSalt = getDBSalt(HttpUtility.HtmlEncode(emailAddr.Text.Trim()));
                    try
                    {
                        if (dbSalt != null && dbSalt.Length > 0 && dbHash != null && dbHash.Length > 0)
                        {
                            string pwdwithSalt = tb_oldpass.Text.ToString().Trim() + dbSalt;
                            byte[] hashwithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdwithSalt));
                            string userhash = Convert.ToBase64String(hashwithSalt);
                            // if old password given is correct
                            if (userhash.Equals(dbHash))
                            {
                                
                                // Check for password complexity
                                int scores = CheckPassword(tb_newpass.Text);
                                string status = "";
                                switch (scores)
                                {
                                    case 1:
                                        status = "Very Weak";
                                        break;
                                    case 2:
                                        status = "Weak";
                                        break;
                                    case 3:
                                        status = "Medium";
                                        break;
                                    case 4:
                                        status = "Strong";
                                        break;
                                    case 5:
                                        status = "Excellent";
                                        break;
                                    default:
                                        break;
                                }
                                errorOrSuccess.Text = " Password Status : " + status + ".";
                                if (scores < 4)
                                {
                                    errorOrSuccess.ForeColor = Color.Red;
                                    errorOrSuccess.Text += " Password does not match the required complexity.";
                                    return;
                                }
                                errorOrSuccess.ForeColor = Color.Green;

                                // Update password logic starts here
                                // hashing/salting password


                                //Generate random "salt" 
                                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                                byte[] saltByte = new byte[8];

                                //Fills array of bytes with a cryptographically strong sequence of random values.
                                rng.GetBytes(saltByte);
                                salt = Convert.ToBase64String(saltByte);

                                string pwdWithSalt = tb_newpass.Text.ToString() + salt;
                                byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));

                                finalHash = Convert.ToBase64String(hashWithSalt);

                                
                                updateAccount(emailAddr.Text.ToString(), finalHash, salt);
                                errorOrSuccess.Text = "Password successfully updated";
                                errorOrSuccess.ForeColor = Color.Green;
                            }
                            else
                            {
                                errorOrSuccess.Text = "Original password is wrong";
                            }
                        }
                        else
                        {
                            errorOrSuccess.Text = "An error has occured";
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.ToString());
                    }
                    
                }
            }
        }
        protected string getDBHash(string email)
        {
            string h = null;
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "SELECT PasswordHash FROM Account WHERE Email=@Email";
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
        protected void updateAccount(string email, string hash, string salt)
        {
            try
            {
                SqlConnection connection = new SqlConnection(MYDBConnectionString);
                string sql = "UPDATE Account SET PasswordHash=@hash, PasswordSalt=@salt WHERE Email=@Email";
                SqlCommand command = new SqlCommand(sql, connection);
                command.CommandType = CommandType.Text;
                command.Parameters.AddWithValue("@hash", hash);
                command.Parameters.AddWithValue("@salt", salt);
                command.Parameters.AddWithValue("@Email", email);

                command.Connection = connection;
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }catch(Exception ex)
            {
                throw new Exception(ex.ToString());
            }
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
        private int CheckPassword(string password)
        {
            int score = 0;
            // Score 0 very weak
            if (password.Length < 8)
            {
                return 1;
            }
            else
            {
                score = 1;
            }
            //Score 2 Code
            if (Regex.IsMatch(password, "[a-z]"))
            {
                score++;
            }
            if (Regex.IsMatch(password, "[A-Z]"))
            {
                score++;
            }
            if (Regex.IsMatch(password, "[0-9]"))
            {
                score++;
            }
            if (Regex.IsMatch(password, "[!@#$%^&*()]"))
            {
                score++;
            }
            return score;
        }
    }
}