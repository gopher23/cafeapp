using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.DirectoryServices;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Threading;
using System.Runtime.InteropServices;



namespace WPFPageSwitch
{
    public partial class Forget_FOB : UserControl, ISwitchable
    {


        private readonly DispatcherTimer _activityTimer;
        private Point _inactiveMousePosition = new Point(0, 0);
        public Forget_FOB()
        {
            InitializeComponent();
            InputManager.Current.PreProcessInput += Forget_FOB_OnActivity;
            _activityTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1), IsEnabled = true };
            _activityTimer.Tick += _Forget_FOB_OnInactivity;

        }
        void _Forget_FOB_OnInactivity(object sender, EventArgs e)
        {
            // remember mouse position
            _inactiveMousePosition = Mouse.GetPosition(Forget_FOBLayoutRoot);

            //stop the dispatchertimer 
            (sender as DispatcherTimer).Stop();
            _activityTimer.Stop();

            //stop the the inputmanager input 
            InputManager.Current.PreProcessInput -= Forget_FOB_OnActivity;
            Switcher.Switch(new MainMenu());

        }

        void Forget_FOB_OnActivity(object sender, PreProcessInputEventArgs e)
        {
            InputEventArgs inputEventArgs = e.StagingItem.Input;

            if (inputEventArgs is MouseEventArgs || inputEventArgs is KeyboardEventArgs)
            {
                if (e.StagingItem.Input is MouseEventArgs)
                {
                    MouseEventArgs mouseEventArgs = (MouseEventArgs)e.StagingItem.Input;

                    // no button is pressed and the position is still the same as the application became inactive
                    if (mouseEventArgs.LeftButton == MouseButtonState.Released &&
                        mouseEventArgs.RightButton == MouseButtonState.Released &&
                        mouseEventArgs.MiddleButton == MouseButtonState.Released &&
                        mouseEventArgs.XButton1 == MouseButtonState.Released &&
                        mouseEventArgs.XButton2 == MouseButtonState.Released &&
                        _inactiveMousePosition == mouseEventArgs.GetPosition(Forget_FOBLayoutRoot))

                        return;
                }

                // set UI on activity
                // rectangle.Visibility = Visibility.Visible;
                this.Visibility = Visibility.Visible;
                _activityTimer.Stop();
                _activityTimer.Start();

            }
        }



        #region ISwitchable Members


        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }

     
        #endregion

        private void Username_Text_Box_TextChanged(object sender, TextChangedEventArgs e)
        {
            string username = Username_Text_Box.Text;
        }

        private void Login_Button_Click(object sender, RoutedEventArgs e)
        {
            {
                //Validate credentials
                bool validCredentials;
                string testMessage;
                string username = "";   // Assign to the entered username
                string password = "";   // Ads

                username = Username_Text_Box.Text;
                password = Password_Box.Password;
                //Call the IsAuthenticaded method to check the username and password
                validCredentials = IsAuthenticated("LDAP://DC=digi,DC=com ", username, password);

                if (validCredentials)
                {
                    testMessage = "valid";

                    try
                    {
                        DirectoryEntry entry = new DirectoryEntry("LDAP://DC=digi,DC=com ");
                        using (DirectorySearcher dsSearcher = new DirectorySearcher(entry))
                        {

                            String Name = Username_Text_Box.Text; //replace with email of scaned user
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
                                                    Fob_ID_Box.Text = dtfob.Rows[0]["FobId"].ToString();

                                                    _activityTimer.Stop();

                                                    //stop the the inputmanager input 
                                                    InputManager.Current.PreProcessInput -= Forget_FOB_OnActivity;
                                                    Switcher.Switch(new Enter_Amount(Fob_ID_Box.Text));

                                                }
                                                if (returnCd == -1)
                                                {
                                                    MessageBox.Show("you have not is not setup in the Cafe Application!");

                                                    _activityTimer.Stop();

                                                    //stop the the inputmanager input 
                                                    InputManager.Current.PreProcessInput -= Forget_FOB_OnActivity;
                                                    Switcher.Switch(new MainMenu());
                                                }
                                                if (returnCd == -2)
                                                {
                                                    MessageBox.Show("you has not have Payroll Deductions enabled");

                                                    _activityTimer.Stop();

                                                    //stop the the inputmanager input 
                                                    InputManager.Current.PreProcessInput -= Forget_FOB_OnActivity;
                                                    Switcher.Switch(new MainMenu());
                                                }
                                                if (returnCd == -3)
                                                {
                                                    MessageBox.Show("you are not active");

                                                    _activityTimer.Stop();

                                                    //stop the the inputmanager input 
                                                    InputManager.Current.PreProcessInput -= Forget_FOB_OnActivity;

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
                                //display unknown user image is user is not found in AD
                                // pictureBox1.Image = test.Properties.Resources.Unknown;
                            }
                        }


                    }
                    catch (Exception Ex)
                    {
                        MessageBox.Show("In login button method"+ Ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("you are not invalid");
                    Username_Text_Box.Text = "";
                    Password_Box.Password = "";

                    testMessage = "invalid";
                }
            }
        }



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
                //MessageBox.Show("A handled exception just occurred: " + cex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                //MessageBox.Show("Wrong username or password.");
        
            }
            catch (Exception ex)
            {
               // MessageBox.Show("A handled exception just occurred: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);                
            }

            return authenticated;
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            //stop the dispatcher timer
            _activityTimer.Stop();
            _activityTimer.IsEnabled = false;
            //stop the the inputmanager input 
            InputManager.Current.PreProcessInput -= Forget_FOB_OnActivity;
            Switcher.Switch(new MainMenu());
        }

    }
}
