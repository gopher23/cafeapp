using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WPFPageSwitch
{
    public partial class Enter_Amount : UserControl, ISwitchable
    {
       
        private readonly DispatcherTimer _activityTimer;
        private Point _inactiveMousePosition = new Point(0, 0);
        public Enter_Amount(String ID)
        {
            // Required to initialize variables
            InitializeComponent();
            ID_Text.Text = ID;

            
            InputManager.Current.PreProcessInput += Enter_Amount_OnActivity;
            _activityTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1), IsEnabled = true };
            _activityTimer.Tick += Enter_Amount_OnInactivity;


            DataTable dtSigners = new DataTable();
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetEmployeeDetail"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = con;
                    cmd.Parameters.AddWithValue("@FobId", ID_Text.Text);
                    SqlDataAdapter adp = new SqlDataAdapter(cmd);
                    adp.Fill(dtSigners);
                    con.Open();
                    SqlParameter returnParameter = cmd.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    cmd.ExecuteNonQuery();
                    int returnCd = (int)returnParameter.Value;

                }
                //mail            
                Mail_box.Text = dtSigners.Rows[0]["EmailAddress"].ToString();
                balance.Content = dtSigners.Rows[0]["CurrentBalance"].ToString();
                Name_Box.Content = dtSigners.Rows[0]["Name"].ToString();
                MaxDebitAmount_Textbox.Text = dtSigners.Rows[0]["MaxDebitAmount"].ToString();
                Remain.Content = dtSigners.Rows[0]["RemainingBalance"].ToString();
                bool admin = Convert.ToBoolean(dtSigners.Rows[0]["isAdmin"]);
                bool isOpen = Convert.ToBoolean(dtSigners.Rows[0]["isOpen"]);
                Close_Register_Button.IsEnabled = false;
                Open_Button.IsEnabled = false;
                if (admin)
                {
                    if (isOpen)
                    {
                        Close_Register_Button.IsEnabled = true;
                        Open_Button.IsEnabled = false;
                    }
                    else
                    {
                        Close_Register_Button.IsEnabled = false;
                        Open_Button.IsEnabled = true;
                    }
                }
                /**
                if(isOpen)
                {
                    Close_Register_Button.IsEnabled = true;
                    Open_Button.IsEnabled = false;
                }
                else
                {                   
                    Close_Register_Button.IsEnabled = false;
                    Open_Button.IsEnabled = true; 
                }
                 * */
            }

            try
            {
                DirectoryEntry entry = new DirectoryEntry("LDAP://DC=digi,DC=com ");
                using (DirectorySearcher dsSearcher = new DirectorySearcher(entry))
                {
                    String Name = Mail_box.Text; //replace with email of scaned user
                    dsSearcher.Filter = "(&(objectClass=user)(mail=" + Name + "))";
                    SearchResult result = dsSearcher.FindOne();

                    if (result != null)
                    {
                        using (DirectoryEntry user = new DirectoryEntry(result.Path))
                        {
                            byte[] data = user.Properties["thumbnailPhoto"].Value as byte[];

                            if (data != null)
                            {
                                MemoryStream ms = new MemoryStream(data);
                                var bitmap = new BitmapImage();
                                bitmap.BeginInit();
                                bitmap.StreamSource = ms;
                                bitmap.EndInit();
                                pictureBox1.Source = bitmap;

                            }
                            else
                            {
                                //display unknown user image if user does not have a image                             
                            }
                        }
                    }
                    else
                    {
                        //display unknown user image is user is not found in AD                      
                    }
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message);
            }
        }

        void Enter_Amount_OnInactivity(object sender, EventArgs e)
        {
            // remember mouse position
            _inactiveMousePosition = Mouse.GetPosition(LayoutRoot);

            // if it is inactivity for a minute, then stop the DispathcerTimer
            (sender as DispatcherTimer).Stop();
            _activityTimer.Stop();

            //stop the the inputmanager input 
            InputManager.Current.PreProcessInput -= Enter_Amount_OnActivity;
            // Switch to the MainMenu user control page
            Switcher.Switch(new MainMenu());
        }

       
        void Enter_Amount_OnActivity(object sender, PreProcessInputEventArgs e)
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
                        _inactiveMousePosition == mouseEventArgs.GetPosition(LayoutRoot))
                        return;
                }

                // set page on activity
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

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            decimal result = Convert.ToDecimal(TextBox_Message.Text) * 10;
            decimal newresult = result + Convert.ToDecimal("0.01") * 7;
            TextBox_Message.Text = newresult.ToString();
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            decimal result = Convert.ToDecimal(TextBox_Message.Text) * 10;
            decimal newresult = result + Convert.ToDecimal("0.01") * 8;
            TextBox_Message.Text = newresult.ToString();
        }

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            decimal result = Convert.ToDecimal(TextBox_Message.Text) * 10;
            decimal newresult = result + Convert.ToDecimal("0.01") * 9;
            TextBox_Message.Text = newresult.ToString();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            decimal result = Convert.ToDecimal(TextBox_Message.Text) * 10;
            decimal newresult = result + Convert.ToDecimal("0.01") * 4;
            TextBox_Message.Text = newresult.ToString();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            decimal result = Convert.ToDecimal(TextBox_Message.Text) * 10;
            decimal newresult = result + Convert.ToDecimal("0.01") * 5;
            TextBox_Message.Text = newresult.ToString();
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            decimal result = Convert.ToDecimal(TextBox_Message.Text) * 10;
            decimal newresult = result + Convert.ToDecimal("0.01") * 6;
            TextBox_Message.Text = newresult.ToString();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            decimal result = Convert.ToDecimal(TextBox_Message.Text) * 10;
            decimal newresult = result + Convert.ToDecimal("0.01") * 1;
            TextBox_Message.Text = newresult.ToString();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            decimal result = Convert.ToDecimal(TextBox_Message.Text) * 10;
            decimal newresult = result + Convert.ToDecimal("0.01") * 2;
            TextBox_Message.Text = newresult.ToString();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            decimal result = Convert.ToDecimal(TextBox_Message.Text) * 10;
            decimal newresult = result + Convert.ToDecimal("0.01") * 3;
            TextBox_Message.Text = newresult.ToString();
        }

        private void Button_Click_0(object sender, RoutedEventArgs e)
        {
            decimal result = Convert.ToDecimal(TextBox_Message.Text) * 10;
            decimal newresult = result + Convert.ToDecimal("0.01") * 0;
            TextBox_Message.Text = newresult.ToString();
        }

        private void Button_Click_00(object sender, RoutedEventArgs e)
        {
            decimal result = Convert.ToDecimal(TextBox_Message.Text) * 100;
            decimal newresult = result + Convert.ToDecimal("0.01") * 0;
            TextBox_Message.Text = newresult.ToString();
        }

        private void Button_Click_Clear(object sender, RoutedEventArgs e)
        {
            TextBox_Message.Text = "0.00";
        }

        private void TextBox_Message_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Close_Register_Button_Click(object sender, RoutedEventArgs e)
        {

            // click close register button and then call InsertChargeJournal and Get ChargesForDay 
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("InsertChargeJournal"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = con;
                    cmd.Parameters.AddWithValue("@FobId", ID_Text.Text);
                    cmd.Parameters.AddWithValue("@ChargeAmount", 0);
                    cmd.Parameters.AddWithValue("@TransactionType", 4);
                    con.Open();
                    SqlParameter returnParameter = cmd.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    cmd.ExecuteNonQuery();
                    int returnCd = (int)returnParameter.Value;
                    con.Close();
                }
            }

            DataTable dtSigners = new DataTable();
            string sum = "";
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetChargesForDay"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = con;
                    SqlDataAdapter adp = new SqlDataAdapter(cmd);

                    adp.Fill(dtSigners);
                    con.Open();
                    SqlParameter returnParameter = cmd.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
                sum = dtSigners.Rows[0]["TodaysCharges"].ToString();
            }

            // stop the timer 
            _activityTimer.Stop();

            //stop the the inputmanager input 
            InputManager.Current.PreProcessInput -= Enter_Amount_OnActivity;
            // switch to the close_Register page 
            Switcher.Switch(new Close_Register(sum));
        }

        private void Open_Button_Click(object sender, RoutedEventArgs e)
        {

            // Call the InsertChargeJournal stored procedure
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("InsertChargeJournal"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = con;
                    cmd.Parameters.AddWithValue("@FobId", ID_Text.Text);
                    cmd.Parameters.AddWithValue("@ChargeAmount", 0);
                    cmd.Parameters.AddWithValue("@TransactionType", 3);
                    con.Open();
                    SqlParameter returnParameter = cmd.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    cmd.ExecuteNonQuery();
                    int returnCd = (int)returnParameter.Value;
                    con.Close();
                }
            }
            // stop the timer 
            _activityTimer.Stop();
            //stop the the inputmanager input 
            InputManager.Current.PreProcessInput -= Enter_Amount_OnActivity;
            // switch to the MainMenu page
            Switcher.Switch(new MainMenu());
        }

        private void Charge_Button_Click(object sender, RoutedEventArgs e)
        {
            if (TextBox_Message.Text == "0.00")
            {
                MessageBox.Show("The amount you enter is 0.00");
            }
            else
            {
                decimal num = 0;
                num = Convert.ToDecimal(TextBox_Message.Text);
                decimal remain = 0;
                remain = Convert.ToDecimal(Remain.Content);

                if (num > remain)
                {
                    MessageBox.Show("Your remaining balance is " + Remain.Content + "\n" + "Your maximum charge limit is " + MaxDebitAmount_Textbox.Text + ".");

                    _activityTimer.Stop();

                    //stop the the inputmanager input 
                    InputManager.Current.PreProcessInput -= Enter_Amount_OnActivity;
                    Switcher.Switch(new MainMenu());
                }

                _activityTimer.Stop();

                //stop the the inputmanager input 
                InputManager.Current.PreProcessInput -= Enter_Amount_OnActivity;
                Switcher.Switch(new Comfirm(TextBox_Message.Text, ID_Text.Text, Name_Box.Content.ToString()));
            }
        }



        private void ID_Text_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

 

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            _activityTimer.Stop();

            //stop the the inputmanager input 
            InputManager.Current.PreProcessInput -= Enter_Amount_OnActivity;
            Switcher.Switch(new MainMenu());
        }
    }
}