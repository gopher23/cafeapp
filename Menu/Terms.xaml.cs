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
using System.Net.Mail;
using System.Net;

namespace WPFPageSwitch
{
    /// <summary>
    /// Interaction logic for Terms.xaml
    /// </summary>
    public partial class Terms_Conditions : UserControl, ISwitchable
    {
        public Terms_Conditions()
        {
            InitializeComponent();
        }
  
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
            message.Subject = "Test";
            message.From = new System.Net.Mail.MailAddress(Email_box.Text);
            message.Body = "Tis is a test mail ";
            message.To.Add("shujun.liu@digi.com");

            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("smtp.gmail.com");
            smtp.Credentials = new System.Net.NetworkCredential(Email_box.Text, Password_Box.Password);
            smtp.EnableSsl = true;
            smtp.Port = 587;
            smtp.Send(message);

       
            
        }


        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }
    }
}
