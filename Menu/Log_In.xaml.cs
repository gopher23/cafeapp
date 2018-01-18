using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFPageSwitch
{
    /// <summary>
    /// Interaction logic for Register.xaml
    /// </summary>
    public partial class Login : UserControl, ISwitchable
    {
        public Login()
        {
            InitializeComponent();
        }


        #region ISwitchable Members
        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }
        #endregion

 

        public bool IsAuthenticated(string srvr, string usr, string pwd)
        {
            bool authenticated = false;

            try
            {
                DirectoryEntry entry = new DirectoryEntry(srvr, usr, pwd);
                object nativeObject = entry.NativeObject;
                authenticated = true;
            }
            catch (DirectoryServicesCOMException cex)
            {
               // MessageBox.Show( cex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                //MessageBox.Show("Wrong username or password.");
             
            }
            catch (Exception ex)
            {
               // MessageBox.Show( ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                //not authenticated due to some other exception [this is optional]
            }

            return authenticated;
        }

        private void Username_Text_Box_TextChanged(object sender, TextChangedEventArgs e)
        {
            string username = Login_Username_Text_Box.Text;
        }

    

        private void Login_Button_Click(object sender, RoutedEventArgs e)
        {
            {
                //Validate credentials
                bool validCredentials;
                string testMessage;
                string username = "";   // Assign to the entered username
                string password = "";   // Ads

                username = Login_Username_Text_Box.Text;
                password = Login_Password_Box.Password;

                validCredentials = IsAuthenticated("LDAP://DC=digi,DC=com ", username, password);

                if (validCredentials)
                {
                    testMessage = "valid";

                    try
                    {
                        DirectoryEntry entry = new DirectoryEntry("LDAP://DC=digi,DC=com ");
                        using (DirectorySearcher dsSearcher = new DirectorySearcher(entry))
                        {

                            String Name = Login_Username_Text_Box.Text; //replace with email of scaned user
                            dsSearcher.Filter = "(&(objectClass=user)(sAMAccountName=" + Name + "))";
                            SearchResult result = dsSearcher.FindOne();

                            if (result != null)
                            {
                                using (DirectoryEntry user = new DirectoryEntry(result.Path))
                                {
                                    string email = user.Properties["mail"].Value as string;

                                    if (email != null)
                                    {
                                        using (SqlConnection cons = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                                        {
                                            DataTable dtfob = new DataTable();
                                            using (SqlCommand cmd = new SqlCommand("ValidateEmployeeFromEmail"))
                                            {
                                                cmd.CommandType = CommandType.StoredProcedure;
                                                cmd.Connection = cons;
                                                cmd.Parameters.AddWithValue("@Email", email);
                                                SqlDataAdapter adp = new SqlDataAdapter(cmd);
                                                adp.Fill(dtfob);
                                                cons.Open();
                                                SqlParameter returnParameter = cmd.Parameters.Add("RetVal", SqlDbType.Int);
                                                returnParameter.Direction = ParameterDirection.ReturnValue;
                                                cmd.ExecuteNonQuery();
                                                int returnCd = (int)returnParameter.Value;
                                                cons.Close();
                                                if (returnCd == 0)
                                                {
                                                    Switcher.Switch(new Scan_FOB(email));
                                                }
                                                if (returnCd == -1)
                                                {
                                                    MessageBox.Show("you have not is not setup in the Cafe Application!");
                                                    Switcher.Switch(new MainMenu());
                                                }
                                                if (returnCd == -2)
                                                {
                                                    MessageBox.Show("you has not have Payroll Deductions enabled");
                                                    Switcher.Switch(new MainMenu());
                                                }
                                                if (returnCd == -3)
                                                {
                                                    MessageBox.Show("you are not active");
                                                    Switcher.Switch(new MainMenu());
                                                }
                                            }
                                        }
                                    }

                                        // Get user detail from email


                                    else
                                    {
                                        MessageBox.Show("You are not in the system yet");

                                    }
                                }
                            }
                            else
                            {
                                // put unknown image as the default image, and if find image from the AD, it will replace with the unknown iamges
                                //display unknown user image is user is not found in AD
                                // pictureBox1.Image = test.Properties.Resources.Unknown;
                            }
                        }


                    }
                    catch 
                    {
                       
                    }
                }
                else
                {
                    MessageBox.Show("you are not invalid");
                    Login_Username_Text_Box.Text = "";
                    Login_Password_Box.Password = "";
                    testMessage = "invalid";
                }
            }
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
         
                Switcher.Switch(new MainMenu());
          
       }
         
    }
}