using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WPFPageSwitch
{
    public partial class Comfirm : UserControl, ISwitchable
    {
        private readonly DispatcherTimer _activityTimer;
        private Point _inactiveMousePosition = new Point(0, 0);
        public Comfirm(string Str_Value, string ID, string name)
        {
            // Required to initialize variables
            InitializeComponent();
            TextBox_confirm.Content = Str_Value;
            ID_Text_Box.Text = ID;
            Name_Label.Content = name;

            InputManager.Current.PreProcessInput += Comfirm_OnActivity;
            _activityTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1), IsEnabled = true };
            _activityTimer.Tick += Comfirm_OnInactivity;
        }

        #region ISwitchable Members
        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }


        #endregion

        void Comfirm_OnInactivity(object sender, EventArgs e)
        {
            // remember mouse position
            _inactiveMousePosition = Mouse.GetPosition(LayoutRoot);
            // If user is inactivity for more than 60 seconds, we will help him/her to complete the transaction, 
            //using the InsertChargeJournal  stored procedure 
            
            // it will the same functionality with the click YES button. 
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("InsertChargeJournal"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = con;
                    cmd.Parameters.AddWithValue("@FobId", ID_Text_Box.Text);
                    cmd.Parameters.AddWithValue("@ChargeAmount", TextBox_confirm.Content);
                    cmd.Parameters.AddWithValue("@TransactionType", 0);
                    con.Open();
                    SqlParameter returnParameter = cmd.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    cmd.ExecuteNonQuery();
                    int returnCd = (int)returnParameter.Value;
                    con.Close();

                    //return returnCd;
                }
            }

            _activityTimer.Stop();

            //stop the the inputmanager input 
            InputManager.Current.PreProcessInput -= Comfirm_OnActivity;
            Switcher.Switch(new MainMenu());

        }

        void Comfirm_OnActivity(object sender, PreProcessInputEventArgs e)
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

                // set UI on activity
                // rectangle.Visibility = Visibility.Visible;

                _activityTimer.Stop();
                _activityTimer.Start();
            }
        }

        private void YES_Button_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("InsertChargeJournal"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = con;
                    cmd.Parameters.AddWithValue("@FobId", ID_Text_Box.Text);
                    cmd.Parameters.AddWithValue("@ChargeAmount", TextBox_confirm.Content);
                    cmd.Parameters.AddWithValue("@TransactionType", 0);
                    con.Open();
                    SqlParameter returnParameter = cmd.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    cmd.ExecuteNonQuery();
                    int returnCd = (int)returnParameter.Value;
                    con.Close();

                    //return returnCd;
                }
            }
            // stop the dispatcher timer
            _activityTimer.IsEnabled = false;
            _activityTimer.Stop();

            //stop the the inputmanager input 
            InputManager.Current.PreProcessInput -= Comfirm_OnActivity;
            Switcher.Switch(new MainMenu());

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _activityTimer.IsEnabled = false;
            _activityTimer.Stop();
            //stop the the inputmanager input 
            InputManager.Current.PreProcessInput -= Comfirm_OnActivity;
            Switcher.Switch(new Enter_Amount(ID_Text_Box.Text));
        }

    }
}