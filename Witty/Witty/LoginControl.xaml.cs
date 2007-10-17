using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using TwitterLib;

namespace Witty
{
    public partial class LoginControl
	{
        private Properties.Settings AppSettings = Properties.Settings.Default;

		public LoginControl()
		{
			this.InitializeComponent();

            UsernameTextBox.Focus();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            TwitterNet twitterNet = new TwitterNet(UsernameTextBox.Text, PasswordTextBox.Password);
            App.LoggedInUser = twitterNet.Login();
            if (App.LoggedInUser != null)
            {
                AppSettings.Username = UsernameTextBox.Text;
                AppSettings.Password = PasswordTextBox.Password;
                AppSettings.LastUpdated = string.Empty;

                AppSettings.Save();

                RaiseEvent(new RoutedEventArgs(LoginEvent));
            }
            else
                MessageBox.Show("Incorrect username or password. Please try again");
        }

        public static readonly RoutedEvent LoginEvent =
            EventManager.RegisterRoutedEvent("Login", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LoginControl));

        public event RoutedEventHandler Login
        {
            add { AddHandler(LoginEvent, value); }
            remove { RemoveHandler(LoginEvent, value); }
        }
	}
}