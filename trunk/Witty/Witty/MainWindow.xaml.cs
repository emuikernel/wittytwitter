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
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace Witty
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            this.InitializeComponent();

            #region Minimize to tray setup
            m_notifyIcon = new System.Windows.Forms.NotifyIcon();
            m_notifyIcon.BalloonTipText = "Richt-click for more options.";
            m_notifyIcon.BalloonTipTitle = "Witty";
            m_notifyIcon.Text = "Right-click to exit.";
            m_notifyIcon.Icon = new System.Drawing.Icon("AppIcon.ico");
            m_notifyIcon.Click += new EventHandler(m_notifyIcon_Click);
            m_notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(m_notifyIcon_MouseDoubleClick);

            System.Windows.Forms.ContextMenu notifyMenu = new System.Windows.Forms.ContextMenu();
            System.Windows.Forms.MenuItem openMenuItem = new System.Windows.Forms.MenuItem();
            System.Windows.Forms.MenuItem exitMenuItem = new System.Windows.Forms.MenuItem();

            notifyMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] { openMenuItem, exitMenuItem });
            openMenuItem.Index = 0;
            openMenuItem.Text = "Open";
            openMenuItem.Click += new EventHandler(openMenuItem_Click);
            exitMenuItem.Index = 1;
            exitMenuItem.Text = "Exit";
            exitMenuItem.Click += new EventHandler(exitMenuItem_Click);

            m_notifyIcon.ContextMenu = notifyMenu;
            this.Closed += new EventHandler(OnClosed);
            this.StateChanged += new EventHandler(OnStateChanged);
            this.IsVisibleChanged += new DependencyPropertyChangedEventHandler(OnIsVisibleChanged);

            #endregion

            #region Single instance setup
            // Enforce single instance for release mode
#if !DEBUG
            Application.Current.Exit += new ExitEventHandler(Current_Exit);
            m_Event = new SingleInstanceManager(this, ShowApplication); 
#endif
            #endregion

            // Does the user need to login
            if (string.IsNullOrEmpty(AppSettings.Username))
            {
                PlayStoryboard("ShowLogin");
            }
            else
            {
                isLoggedIn = true;
                LoginControl.Visibility = Visibility.Hidden;
                RefreshButton.IsEnabled = true;
                AppSettings.LastUpdated = string.Empty;

                twitter = new TwitterNet(AppSettings.Username, AppSettings.Password);
                LayoutRoot.DataContext = tweets;
                DelegateFetch();

                // Setup refresh timer
                refreshTimer.Interval = refreshInterval;
                refreshTimer.Tick += new EventHandler(Timer_Elapsed);
                refreshTimer.Start();
            }
        }

        #region Fields and Properties

        // Main collection of tweets
        public static Tweets tweets = new Tweets();

        // Main TwitterNet object
        private TwitterNet twitter;

        // Timer used for automatic tweet updates
        private DispatcherTimer refreshTimer = new DispatcherTimer();

        // How often the automatic tweet updates occur.  TODO: Make this configurable
        private TimeSpan refreshInterval = new TimeSpan(0, 2, 0);  // 2 minutes

        // Delegates for placing jobs onto the thread dispatcher.  
        // Used for making asynchronous calls to Twitter so that the UI does not lock up.
        private delegate void NoArgDelegate();
        private delegate void OneArgDelegate(Tweets arg);
        private delegate void AddTweetDelegate(string arg);

        // Settings used by the application
        private Properties.Settings AppSettings = Properties.Settings.Default;

        // booleans to keep track of state
        private bool isExpanded;
        private bool isLoggedIn;

        #endregion

        #region Retrieve new tweets

        /// <summary>
        /// Encapsulated method to create dispatcher for fetching new tweets asynchronously
        /// </summary>
        private void DelegateFetch()
        {
            // Let the user know what's going on
            StatusTextBlock.Text = "Fetching new tweets...";

            PlayStoryboard("Fetching");

            // Create a Dispatcher to fetching new tweets
            NoArgDelegate fetcher = new NoArgDelegate(
                this.GetTweets);

            fetcher.BeginInvoke(null, null);
        }

        private void Timer_Elapsed(object sender, EventArgs e)
        {
            DelegateFetch();
        }

        private void GetTweets()
        {
            try
            {
                // Schedule the update function in the UI thread.
                LayoutRoot.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new OneArgDelegate(UpdateUserInterface), twitter.GetFriendsTimeline(AppSettings.LastUpdated));
            }
            catch (WebException ex)
            {
#if DEBUG
                MessageBox.Show("There was a problem fetching new tweets from Twitter.com. " + ex.Message);
#endif
            }
        }

        private void UpdateUserInterface(Tweets newTweets)
        {
            // Refresh the grid so that relativeTime updates
            LayoutRoot.DataContext = null;
            LayoutRoot.DataContext = tweets;

            DateTime lastUpdated = DateTime.Now;
            StatusTextBlock.Text = "Last Updated: " + lastUpdated.ToString();

            AppSettings.LastUpdated = lastUpdated.ToString();
            AppSettings.Save();

            int tweetAdded = 0;

            for (int i = newTweets.Count - 1; i >= 0; i--)
            {
                Tweet tweet = newTweets[i];
                if (!tweets.Contains(tweet))
                {
                    tweets.Insert(0, tweet);
                    tweetAdded++;
                }
            }

            StopStoryboard("Fetching");
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            DelegateFetch();
        }

        #endregion

        #region Add new tweet update

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            // Schedule posting the tweet
            UpdateButton.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new AddTweetDelegate(AddTweet), TweetTextBox.Text);
            TweetTextBox.Clear();
            TweetTextBox.Focus();
        }

        private void AddTweet(string text)
        {
            try
            {
                twitter.AddTweet(text);

                // Schedule the update function in the UI thread.
                LayoutRoot.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new AddTweetDelegate(UpdatePostUserInterface), text);
            }
            catch (WebException ex)
            {
                UpdateTextBlock.Text = "Update failed.";

#if DEBUG
                MessageBox.Show("There was a problem posting your tweet to Twitter.com. " + ex.Message);
#endif
            }
        }

        private void UpdatePostUserInterface(string text)
        {
            UpdateTextBlock.Text = "Updated!";

            PlayStoryboard("CollapseUpdate");
        }

        private void Update_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (isLoggedIn)
            {
                if (!isExpanded)
                {
                    UpdateTextBlock.Text = "Cancel Update";
                    PlayStoryboard("ExpandUpdate");
                    TweetTextBox.Focus();
                    isExpanded = true;
                }
                else
                {
                    UpdateTextBlock.Text = "Update";
                    PlayStoryboard("CollapseUpdate");
                    isExpanded = false;
                }
            }
        }

        #endregion

        #region Misc Methods and Event Handlers

        private void LoginControl_Login(object sender, RoutedEventArgs e)
        {
            twitter = new TwitterNet(AppSettings.Username, AppSettings.Password);

            // Set the datacontext
            LayoutRoot.DataContext = tweets;

            // fetch new tweets
            DelegateFetch();

            // Setup refresh timer to get subsequent tweets
            refreshTimer.Interval = refreshInterval;
            refreshTimer.Tick += new EventHandler(Timer_Elapsed);
            refreshTimer.Start();

            PlayStoryboard("HideLogin");

            isExpanded = false;
            isLoggedIn = true;
        }

        private void PlayStoryboard(string storyboardName)
        {
            Object o = TryFindResource(storyboardName);
            if (o != null)
            {
                Storyboard storyboard = (Storyboard)o;
                storyboard.Begin(this);
            }
        }

        private void StopStoryboard(string storyboardName)
        {
            Object o = TryFindResource(storyboardName);
            if (o != null)
            {
                Storyboard storyboard = (Storyboard)o;
                storyboard.Stop(this);
            }
        }

        private void TweetsListBox_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.MouseDevice.DirectlyOver.GetType() == typeof(TextBlock))
            {
                TextBlock textBlock = (TextBlock)e.MouseDevice.DirectlyOver;
                if (textBlock.Name == "TweetText")
                {
                    if (TweetsListBox.SelectedValue != null && !string.IsNullOrEmpty(TweetsListBox.SelectedValue.ToString()))
                        System.Diagnostics.Process.Start(TweetsListBox.SelectedValue.ToString());
                }

                if (textBlock.Name == "ScreenName")
                {
                    if (TweetsListBox.SelectedItem != null)
                    {
                        Tweet tweet = (Tweet)TweetsListBox.SelectedItem;
                        System.Diagnostics.Process.Start(tweet.User.TwitterUrl);
                    }
                }
            }
        }

        #endregion

        #region Minimize to Tray

        private System.Windows.Forms.NotifyIcon m_notifyIcon;

        void OnClosed(object sender, EventArgs e)
        {
            m_notifyIcon.Dispose();
            m_notifyIcon = null;
        }

        void OnClosing(object sender, CancelEventArgs args)
        {
            args.Cancel = true;

            if (WindowState == WindowState.Normal)
            {
                if (m_notifyIcon != null)
                    m_notifyIcon.ShowBalloonTip(2000);
            }
            else
                m_storedWindowState = WindowState;
        }

        private WindowState m_storedWindowState = WindowState.Normal;

        DispatcherTimer hideTimer = new DispatcherTimer();

        void OnStateChanged(object sender, EventArgs args)
        {
            if (WindowState == WindowState.Minimized)
            {
                hideTimer.Interval = new TimeSpan(500);
                hideTimer.Tick += new EventHandler(HideTimer_Elapsed);
                hideTimer.Start();
            }
            else
                m_storedWindowState = WindowState;
        }

        private void HideTimer_Elapsed(object sender, EventArgs e)
        {
            this.Hide();
            hideTimer.Stop();
        }

        void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            CheckTrayIcon();
        }

        void m_notifyIcon_Click(object sender, EventArgs e)
        {
            //m_notifyIcon.ContextMenu.Show((System.Windows.Forms.Control)sender, new System.Drawing.Point(20, 20));
        }

        void m_notifyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Show();
            WindowState = m_storedWindowState;
        }

        void CheckTrayIcon()
        {
            ShowTrayIcon(!IsVisible);
        }

        void ShowTrayIcon(bool show)
        {
            if (m_notifyIcon != null)
                m_notifyIcon.Visible = show;
        }

        void openMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = m_storedWindowState;
        }
        
        void exitMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion

        #region Single Instance

        SingleInstanceManager m_Event;

        public void ShowApplication()
        {
            if (this.Visibility == Visibility.Hidden)
            {
                this.Visibility = Visibility.Visible;
            }
        }

        void Current_Exit(object sender, ExitEventArgs e)
        {
            Environment.Exit(0);
        }
        #endregion
    }
}