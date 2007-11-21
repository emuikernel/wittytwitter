using System.Windows;
using TwitterLib;

namespace Witty
{
    public partial class LoginControl
    {
        private Properties.Settings AppSettings = Properties.Settings.Default;

        public LoginControl()
        {
            this.InitializeComponent();

        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            TwitterNet twitterNet = new TwitterNet(UsernameTextBox.Text, PasswordTextBox.Password);

            try
            {
                App.LoggedInUser = twitterNet.Login();
                if (App.LoggedInUser != null)
                {
                    AppSettings.Username = UsernameTextBox.Text;
                    AppSettings.Password = PasswordTextBox.Password;
                    AppSettings.LastUpdated = string.Empty;

                    AppSettings.Save();

                    UsernameTextBox.Text = string.Empty;
                    PasswordTextBox.Password = string.Empty;

                    RaiseEvent(new RoutedEventArgs(LoginEvent));
                }
                else
                    MessageBox.Show("Incorrect username or password. Please try again");
            }
            catch (RateLimitException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static readonly RoutedEvent LoginEvent =
            EventManager.RegisterRoutedEvent("Login", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LoginControl));

        public event RoutedEventHandler Login
        {
            add { AddHandler(LoginEvent, value); }
            remove { RemoveHandler(LoginEvent, value); }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Set Username textbox as default focus
            UsernameTextBox.Focus();
        }
    }
}