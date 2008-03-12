using System.Windows;
using TwitterLib;
using System.Net;
using log4net;
using log4net.Config;

namespace Witty
{
    public partial class LoginControl
    {
        private static readonly ILog logger = LogManager.GetLogger("Witty.Logging");

        private readonly Properties.Settings AppSettings = Properties.Settings.Default;

        private delegate void LoginDelegate(TwitterNet arg);
        private delegate void PostLoginDelegate(User arg);

        public LoginControl()
        {
            this.InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            TwitterNet twitter = new TwitterNet(UsernameTextBox.Text, PasswordTextBox.Password);

            // Attempt login in a new thread
            LoginButton.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new LoginDelegate(TryLogin), twitter);
        }

        private void TryLogin(TwitterNet twitter)
        {
            try
            {
                // Schedule the update function in the UI thread.
                LayoutRoot.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new PostLoginDelegate(UpdatePostLoginInterface), twitter.Login());
            }
            catch (WebException ex)
            {
                logger.Error("There was a problem logging in Twitter.");
                MessageBox.Show("There was a problem logging in to Twitter. " + ex.Message);
            }
            catch (RateLimitException ex)
            {
                logger.Error("There was a rate limit exception.");
                MessageBox.Show(ex.Message);
            }
        }

        private void UpdatePostLoginInterface(User user)
        {
            App.LoggedInUser = user;
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
            {
                MessageBox.Show("Incorrect username or password. Please try again");
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