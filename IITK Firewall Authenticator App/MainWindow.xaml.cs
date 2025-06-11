using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Management;
using CredentialsLibrary;

namespace IITK_Firewall_Authenticator_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {

            String username = username_textbox.Text;
            String password = password_passwordbox.Password;

            if (username == "" || password == "")
            {
                MessageBox.Show($"Username or password cannot be empty.");
                return;
            }

            WriteToCredentialsFile(username, password);

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void ShowPassword_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            password_textbox_hidden.Text = password_passwordbox.Password.ToString();
            password_textbox_hidden.Visibility = Visibility.Visible;
            password_passwordbox.Visibility = Visibility.Hidden;
        }

        private void ShowPassword_MouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            password_textbox_hidden.Text = "";
            password_textbox_hidden.Visibility = Visibility.Hidden;
            password_passwordbox.Visibility = Visibility.Visible;
        }

        private void WriteToCredentialsFile(string username, string password)
        {


            Credentials creds = new Credentials
            {
                username = username,
                password = password,
            };


            try
            {
                CredentialsManager.SaveCredentials(creds);
                MessageBox.Show("Successfully updated the credentials.");

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while updating credentials file. {ex.ToString()}");

            }

        }

        string GetBinaryPath(string serviceName)
        {
            string query = $"SELECT * FROM Win32_Service WHERE Name = '{serviceName}'";

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            using (ManagementObjectCollection results = searcher.Get())
            {
                foreach (ManagementObject service in results)
                {
                    return service["PathName"]?.ToString();
                }
            }
            return null;
        }
    }
}
