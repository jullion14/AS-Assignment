using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;

namespace SITConnect
{
    public partial class _Default : Page
    {
        string MYDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ASDB"].ConnectionString;
        static string finalHash;
        static string salt;
        byte[] Key;
        byte[] IV;
        protected void Page_Load(object sender, EventArgs e)
        {
          
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

        protected void Submit_register_Click(object sender, EventArgs e)
        {
            if (ValidateCaptcha())
            {
                // Checking if email already exists
                bool emailExists = EmailCheck();
                if (emailExists)
                {
                    errorOrSuccess.Text = "Email already registered in database. Please use another email account";
                    errorOrSuccess.ForeColor = Color.Red;
                    return;
                }
                // Checking for appropriate credit card number length
                if (ccardInfo.Text.Length != 16)
                {
                    errorOrSuccess.Text = "Invalid Credit Card Number";
                    errorOrSuccess.ForeColor = Color.Red;
                    return;
                }
                // For password validation
                int scores = CheckPassword(pwdInput.Text);
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
                    errorOrSuccess.Text += " Please think of another password.";
                    return;
                }
                errorOrSuccess.ForeColor = Color.Green;

                // hashing/salting password
                string pwd = pwdInput.Text.ToString().Trim();

                //Generate random "salt" 
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                byte[] saltByte = new byte[8];

                //Fills array of bytes with a cryptographically strong sequence of random values.
                rng.GetBytes(saltByte);
                salt = Convert.ToBase64String(saltByte);

                SHA512Managed hashing = new SHA512Managed();

                string pwdWithSalt = pwd + salt;
                byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));

                finalHash = Convert.ToBase64String(hashWithSalt);

                RijndaelManaged cipher = new RijndaelManaged();
                cipher.GenerateKey();
                Key = cipher.Key;
                IV = cipher.IV;


                CreateAccount();
                errorOrSuccess.Text = "Account successfully created. Please log in.";
                errorOrSuccess.ForeColor = Color.Green;
            }
            else
            {
                errorOrSuccess.Text = "There was an error.";
                errorOrSuccess.ForeColor = Color.Red;
            }
        }
        protected void CreateAccount()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(MYDBConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO Account VALUES(@Email, @FirstName, @LastName, @CreditCard, @PasswordHash, @PasswordSalt, @DateOfBirth, @IV, @Key, @LockStatus, @AttemptCount, @StartLockTime, @EndLockTime)"))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@Email", HttpUtility.HtmlEncode(emailAddr.Text.Trim()));
                            cmd.Parameters.AddWithValue("@FirstName", HttpUtility.HtmlEncode(firstName.Text.Trim()));
                            cmd.Parameters.AddWithValue("@LastName", HttpUtility.HtmlEncode(lastName.Text.Trim()));
                            cmd.Parameters.AddWithValue("@CreditCard", Convert.ToBase64String(EncryptData(ccardInfo.Text.Trim())));
                            cmd.Parameters.AddWithValue("@PasswordHash", finalHash);
                            cmd.Parameters.AddWithValue("@PasswordSalt", salt);
                            cmd.Parameters.AddWithValue("@DateOfBirth", HttpUtility.HtmlEncode(tb_date.Text.Trim()));
                            cmd.Parameters.AddWithValue("@IV", Convert.ToBase64String(IV));
                            cmd.Parameters.AddWithValue("@Key", Convert.ToBase64String(Key));
                            cmd.Parameters.AddWithValue("@LockStatus", "F"); // F means not locked by default                       
                            cmd.Parameters.AddWithValue("@AttemptCount", 0);
                            cmd.Parameters.AddWithValue("@StartLockTime", DBNull.Value);
                            cmd.Parameters.AddWithValue("@EndLockTime", DBNull.Value);

                            cmd.Connection = con;
                            con.Open();
                            cmd.ExecuteNonQuery();
                            con.Close();
                        }
                    }
                }
            }catch(Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            
        }
            
     
        protected byte[] EncryptData(string data)
        {
            byte[] cipherText = null;
            try
            {
                RijndaelManaged cipher = new RijndaelManaged();
                cipher.IV = IV;
                cipher.Key = Key;
                ICryptoTransform encryptTransform = cipher.CreateEncryptor();
                //ICryptoTransform decryptTransform = cipher.CreateDecryptor();
                byte[] plainText = Encoding.UTF8.GetBytes(data);
                cipherText = encryptTransform.TransformFinalBlock(plainText, 0, plainText.Length);

            }
            catch (Exception ex)
            {
                
                errorOrSuccess.Text = "There was an error";
            }

            finally { }
            return cipherText;
        }
        private bool EmailCheck()
        {
            SqlConnection con = new SqlConnection(MYDBConnectionString);
            SqlCommand cmd = new SqlCommand("Select * from Account where Email= @Email", con);
            cmd.Parameters.AddWithValue("@Email", emailAddr.Text);
            con.Open();
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                // Make sure that email has a row in the database (exists)
                if (dr.HasRows == true)
                {
                    errorOrSuccess.Text = "Email already exists!";
                    errorOrSuccess.ForeColor = Color.Red;
                    return true;
                }
            }
            return false;
        }
        public bool ValidateCaptcha()
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