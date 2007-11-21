using System;
using System.ComponentModel;
using System.Media;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using TwitterLib;

namespace Witty
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            this.InitializeComponent();

            #region Minimize to tray setup
            m_notifyIcon = new System.Windows.Forms.NotifyIcon();
            m_notifyIcon.BalloonTipText = "Richt-click for more options";
            m_notifyIcon.BalloonTipTitle = "Witty";
            m_notifyIcon.Text = "Witty - The WPF Twitter Client";
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

            // Set the datacontext
            LayoutRoot.DataContext = tweets;
            RepliesListBox.ItemsSource = replies;
            UserTab.DataContext = userTweets;
            MessagesListBox.ItemsSource = messages;

            // set the refresh interval
            if (string.IsNullOrEmpty(AppSettings.RefreshInterval))
            {
                AppSettings.RefreshInterval = "2"; // 2 minutes default
                AppSettings.Save();
            }
            refreshInterval = new TimeSpan(0, int.Parse(AppSettings.RefreshInterval), 0);

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
                OptionsButton.IsEnabled = true;
                AppSettings.LastUpdated = string.Empty;

                twitter = new TwitterNet(AppSettings.Username, AppSettings.Password);
                App.LoggedInUser = twitter.Login();

                DelegateRecentFetch();

                // Setup refresh timer
                refreshTimer.Interval = refreshInterval;
                refreshTimer.Tick += new EventHandler(Timer_Elapsed);
                refreshTimer.Start();
            }

            AlwaysOnTopMenuItem.IsChecked = this.Topmost;
        }

        #region Fields and Properties

        // Main collection of tweets
        private TweetCollection tweets = new TweetCollection();

        // Main collection of replies
        private TweetCollection replies = new TweetCollection();

        private DateTime repliesLastUpdated;

        // Main collection of user Tweets
        private TweetCollection userTweets = new TweetCollection();

        // Main collection of direct messages
        private DirectMessageCollection messages = new DirectMessageCollection();

        private DateTime messagesLastUpdated;

        // Main TwitterNet object used to make Twitter API calls
        private TwitterNet twitter;

        // Timer used for automatic tweet updates
        private DispatcherTimer refreshTimer = new DispatcherTimer();

        // How often the automatic tweet updates occur.  TODO: Make this configurable
        private TimeSpan refreshInterval;

        // Delegates for placing jobs onto the thread dispatcher.  
        // Used for making asynchronous calls to Twitter so that the UI does not lock up.
        private delegate void NoArgDelegate();
        private delegate void OneArgDelegate(TweetCollection arg);
        private delegate void OneStringArgDelegate(string arg);
        private delegate void AddTweetUpdateDelegate(Tweet arg);
        private delegate void MessagesDelegate(DirectMessageCollection arg);
        private delegate void SendMessageDelegate(string user, string text);

        // Settings used by the application
        private Properties.Settings AppSettings = Properties.Settings.Default;

        // booleans to keep track of state
        private bool isExpanded;
        private bool isLoggedIn;
        private bool isMessageExpanded;

        private enum CurrentView
        {
            Recent, Replies, User, Messages
        }

        private CurrentView currentView
        {
            get
            {
                switch (Tabs.SelectedIndex)
                {
                    case 0:
                        return CurrentView.Recent;
                    case 1:
                        return CurrentView.Replies;
                    case 2:
                        return CurrentView.User;
                    default:
                        return CurrentView.Recent;
                }
            }
        }

        private string displayUser;

        #endregion

        #region Retrieve new tweets

        /// <summary>
        /// Encapsulated method to create dispatcher for fetching new tweets asynchronously
        /// </summary>
        private void DelegateRecentFetch()
        {
            // Let the user know what's going on
            StatusTextBlock.Text = "Retrieving tweets...";

            PlayStoryboard("Fetching");

            // Create a Dispatcher to fetching new tweets
            NoArgDelegate fetcher = new NoArgDelegate(
                this.GetTweets);

            fetcher.BeginInvoke(null, null);
        }

        private void Timer_Elapsed(object sender, EventArgs e)
        {
            DelegateRecentFetch();
        }

        private void GetTweets()
        {
            try
            {
                // Schedule the update function in the UI thread.
                LayoutRoot.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new OneArgDelegate(UpdateUserInterface), twitter.GetFriendsTimeline());
            }
            catch (WebException ex)
            {
#if DEBUG
                MessageBox.Show("There was a problem fetching new tweets from Twitter.com. " + ex.Message);
#endif
            }
        }

        private void UpdateUserInterface(TweetCollection newTweets)
        {
            DateTime lastUpdated = DateTime.Now;
            StatusTextBlock.Text = "Last Updated: " + lastUpdated.ToLongTimeString();

            AppSettings.LastUpdated = lastUpdated.ToString();
            AppSettings.Save();

            int tweetAdded = 0;

            for (int i = newTweets.Count - 1; i >= 0; i--)
            {
                Tweet tweet = newTweets[i];
                if (!tweets.Contains(tweet))
                {
                    tweets.Insert(0, tweet);
                    tweet.Index = tweets.Count;
                    tweet.IsNew = true;
                    tweetAdded++;
                }
                else
                {
                    // update the relativetime for existing tweets
                    tweets[i].UpdateRelativeTime();
                }
            }

            if (tweetAdded > 0 && AppSettings.PlaySounds)
            {
                // Play tweets found sound
                SoundPlayer player = new SoundPlayer(Witty.Properties.Resources.alert);
                player.Play();

                //MediaElement media = new MediaElement();
                //media.Source = new Uri("alert.wav");

            }

            StopStoryboard("Fetching");
        }

        #endregion

        #region Add new tweet update

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TweetTextBox.Text))
            {
                // Schedule posting the tweet
                UpdateButton.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new OneStringArgDelegate(AddTweet), TweetTextBox.Text);
            }
        }

        private void AddTweet(string tweetText)
        {
            try
            {
                //parse the text here and tiny up any URLs found.
                TinyUrlHelper tinyUrls = new TinyUrlHelper();
                string tinyfiedText = tinyUrls.ConvertUrlsToTinyUrls(tweetText);
                Tweet tweet = twitter.AddTweet(tinyfiedText);

                // Schedule the update function in the UI thread.
                LayoutRoot.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new AddTweetUpdateDelegate(UpdatePostUserInterface), tweet);
            }
            catch (WebException ex)
            {
                UpdateTextBlock.Text = "Update failed.";

#if DEBUG
                MessageBox.Show("There was a problem posting your tweet to Twitter.com. " + ex.Message);
#endif
            }
        }

        private void UpdatePostUserInterface(Tweet newlyAdded)
        {
            if (newlyAdded != null)
            {
                UpdateTextBlock.Text = "Update";
                StatusTextBlock.Text = "Status Updated!";
                PlayStoryboard("CollapseUpdate");
                isExpanded = false;
                TweetTextBox.Clear();
                tweets.Insert(0, newlyAdded);
            }
            else
            {
                MessageBox.Show("There was a problem posting your tweet to Twitter.com.");
            }
        }

        private void Update_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ToggleUpdate();
        }

        private void ToggleUpdate()
        {
            if (isLoggedIn)
            {
                if (!isExpanded)
                {
                    PlayStoryboard("ExpandUpdate");
                    TweetTextBox.Focus();
                    isExpanded = true;
                }
                else
                {
                    PlayStoryboard("CollapseUpdate");
                    isExpanded = false;
                }
            }
        }
        #endregion

        #region Replies

        private void DelegateRepliesFetch()
        {
            // Let the user know what's going on
            StatusTextBlock.Text = "Retrieving replies...";

            PlayStoryboard("Fetching");

            // Create a Dispatcher to fetching new tweets
            NoArgDelegate fetcher = new NoArgDelegate(
                this.GetReplies);

            fetcher.BeginInvoke(null, null);
        }

        private void GetReplies()
        {
            try
            {
                // Schedule the update function in the UI thread.
                LayoutRoot.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new OneArgDelegate(UpdateRepliesInterface), twitter.GetReplies());
            }
            catch (WebException ex)
            {
#if DEBUG
                MessageBox.Show("There was a problem fetching your replies from Twitter.com. " + ex.Message);
#endif
            }
        }

        private void UpdateRepliesInterface(TweetCollection newReplies)
        {
            repliesLastUpdated = DateTime.Now;
            StatusTextBlock.Text = "Replies Updated: " + repliesLastUpdated.ToLongTimeString();

            for (int i = newReplies.Count - 1; i >= 0; i--)
            {
                Tweet reply = newReplies[i];
                if (!replies.Contains(reply))
                {
                    replies.Insert(0, reply);
                    reply.Index = replies.Count;
                    reply.IsNew = true;
                }
                else
                {
                    // update the relativetime for existing tweets
                    replies[i].UpdateRelativeTime();
                }
            }

            StopStoryboard("Fetching");
        }

        #endregion

        #region Messages

        private void DelegateMessagesFetch()
        {
            // Let the user know what's going on
            StatusTextBlock.Text = "Retrieving direct messages...";

            PlayStoryboard("Fetching");

            // Create a Dispatcher to fetching new tweets
            NoArgDelegate fetcher = new NoArgDelegate(
                this.GetMessages);

            fetcher.BeginInvoke(null, null);
        }

        private void GetMessages()
        {
            try
            {
                // Schedule the update function in the UI thread.
                LayoutRoot.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new MessagesDelegate(UpdateMessagesInterface), twitter.RetrieveMessages());
            }
            catch (WebException ex)
            {
#if DEBUG
                MessageBox.Show("There was a problem fetching your direct messages from Twitter.com. " + ex.Message);
#endif
            }
        }

        private void UpdateMessagesInterface(DirectMessageCollection newMessages)
        {
            messagesLastUpdated = DateTime.Now;
            StatusTextBlock.Text = "Messages Updated: " + messagesLastUpdated.ToLongTimeString();

            for (int i = newMessages.Count - 1; i >= 0; i--)
            {
                DirectMessage message = newMessages[i];
                if (!messages.Contains(message))
                {
                    messages.Insert(0, message);
                    message.IsNew = true;
                }
                else
                {
                    // update the relativetime for existing messages
                    //messages[i].UpdateRelativeTime();
                }
            }

            StopStoryboard("Fetching");
        }

        #endregion

        #region Send messages

        private void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(MessageUserTextBox.Text) && !string.IsNullOrEmpty(MessageTextBox.Text))
            {
                // Schedule posting the tweet
                UpdateButton.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new SendMessageDelegate(SendMessage), MessageUserTextBox.Text, MessageTextBox.Text);
            }
        }

        private void SendMessage(string user, string messageText)
        {
            try
            {
                twitter.SendMessage(user, messageText);

                // Schedule the update function in the UI thread.
                LayoutRoot.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new NoArgDelegate(UpdateMessageUserInterface));
            }
            catch (WebException ex)
            {
                UpdateTextBlock.Text = "Message failed.";

#if DEBUG
                MessageBox.Show("There was a problem sending your message. " + ex.Message);
#endif
            }
        }

        private void UpdateMessageUserInterface()
        {
            UpdateTextBlock.Text = "Send Message";
            StatusTextBlock.Text = "Message Sent!";
            PlayStoryboard("CollapseMessage");
            isMessageExpanded = false;
            MessageTextBox.Clear();
        }

        private void Message_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ToggleMessage();
        }

        private void ToggleMessage()
        {
            if (isLoggedIn)
            {
                if (!isMessageExpanded)
                {
                    PlayStoryboard("ExpandMessage");
                    MessageTextBox.Focus();
                    isMessageExpanded = true;
                }
                else
                {
                    PlayStoryboard("CollapseMessage");
                    isMessageExpanded = false;
                }
            }
        }
        #endregion

        #region User Timline

        private void DelegateUserTimelineFetch(string userId)
        {
            displayUser = userId;

            UserTab.IsSelected = true;
            userTweets.Clear();

            // Let the user know what's going on
            StatusTextBlock.Text = "Retrieving user's tweets...";

            PlayStoryboard("Fetching");

            // Create a Dispatcher to fetching new tweets
            LayoutRoot.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new OneStringArgDelegate(GetUserTimeline), userId);
        }

        private void GetUserTimeline(string userId)
        {
            try
            {
                // Schedule the update function in the UI thread.
                LayoutRoot.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new OneArgDelegate(UpdateUsersTimelineInterface), twitter.GetUserTimeline(userId));
            }
            catch (WebException ex)
            {
#if DEBUG
                MessageBox.Show("There was a problem fetching the user's timeline from Twitter.com. " + ex.Message);
#endif
            }
        }

        private void UpdateUsersTimelineInterface(TweetCollection newTweets)
        {
            StatusTextBlock.Text = displayUser + "'s Timeline Updated: " + repliesLastUpdated.ToLongTimeString();

            for (int i = newTweets.Count - 1; i >= 0; i--)
            {
                Tweet tweet = newTweets[i];
                if (!userTweets.Contains(tweet))
                {
                    userTweets.Insert(0, tweet);
                    tweet.IsNew = true;
                }
                else
                {
                    // update the relativetime for existing tweets
                    userTweets[i].UpdateRelativeTime();
                }
            }

            if (userTweets.Count > 0)
                UserTimelineListBox.SelectedIndex = 0;

            StopStoryboard("Fetching");
        }

        #endregion

        #region Misc Methods and Event Handlers

        /// <summary>
        /// Checks for keyboard shortcuts
        /// </summary>
        /// <param name="e">EventArgs</param>
        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (isLoggedIn)
                {
                    switch (e.Key)
                    {
                        case Key.D:
                            //Direct message to user
                            ToggleUpdate();
                            TweetTextBox.Text = "";
                            TweetTextBox.Text = "D ";
                            if (null != TweetsListBox.SelectedItem)
                            {
                                TweetTextBox.Text += ((Tweet)TweetsListBox.SelectedItem).User.ScreenName;
                            }
                            TweetTextBox.Select(TweetTextBox.Text.Length, 0);
                            break;
                        case Key.U:
                            ToggleUpdate();
                            break;
                        case Key.R:
                            //reply to user
                            if (null != TweetsListBox.SelectedItem)
                            {
                                ToggleUpdate();
                                TweetTextBox.Text = "";
                                TweetTextBox.Text = "@" + ((Tweet)TweetsListBox.SelectedItem).User.ScreenName + " ";
                                TweetTextBox.Select(TweetTextBox.Text.Length, 0);
                            }
                            break;
                        case Key.O:
                            showOptions();
                            break;
                        case Key.D1:
                            //show the "Recent" tab
                            Tabs.SelectedIndex = 0;
                            break;
                        case Key.D2:
                            //show the "Replies" tab
                            Tabs.SelectedIndex = 1;
                            break;
                        case Key.D3:
                            //show the "Users" tab
                            Tabs.SelectedIndex = 2;
                            break;
                        case Key.D4:
                            //show the "Messages" tab
                            Tabs.SelectedIndex = 3;
                            break;
                    }
                }
                else
                {
                    if (e.Key == Key.Q) { App.Current.Shutdown(); };
                }
            }
            else
            {
                if (e.Key == Key.F5) { this.Refresh(); };

                if (e.Key == Key.Escape) { this.WindowState = WindowState.Minimized; };
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            this.Refresh();
        }

        private void Refresh()
        {
            switch (currentView)
            {
                case CurrentView.Recent:
                    DelegateRecentFetch();
                    break;
                case CurrentView.Replies:
                    DelegateRepliesFetch();
                    break;
                case CurrentView.User:
                    DelegateUserTimelineFetch(displayUser);
                    break;
            }
        }

        private void Tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl tabs = (TabControl)sender;

            if (tabs.SelectedIndex == 0)
            {
                displayUser = string.Empty;
            }

            if (tabs.SelectedIndex == 1 && isLoggedIn)
            {
                // limit updating replies to no more than once a minute
                long ticks = DateTime.Now.Ticks - repliesLastUpdated.Ticks;
                TimeSpan ts = new TimeSpan(ticks);
                if (ts.TotalMinutes > 1)
                {
                    DelegateRepliesFetch();
                }

                displayUser = string.Empty;
            }

            if (tabs.SelectedIndex == 2 && string.IsNullOrEmpty(displayUser))
            {
                DelegateUserTimelineFetch(AppSettings.Username);
            }

            if (tabs.SelectedIndex == 3 && isLoggedIn)
            {
                // limit updating replies to no more than once a minute
                long ticks = DateTime.Now.Ticks - messagesLastUpdated.Ticks;
                TimeSpan ts = new TimeSpan(ticks);
                if (ts.TotalMinutes > 1)
                {
                    DelegateMessagesFetch();
                }

                displayUser = string.Empty;
            }
        }

        private void LoginControl_Login(object sender, RoutedEventArgs e)
        {
            twitter = new TwitterNet(AppSettings.Username, AppSettings.Password);

            // fetch new tweets
            DelegateRecentFetch();

            // Setup refresh timer to get subsequent tweets
            refreshTimer.Interval = refreshInterval;
            refreshTimer.Tick += new EventHandler(Timer_Elapsed);
            refreshTimer.Start();

            PlayStoryboard("HideLogin");

            isExpanded = false;
            isLoggedIn = true;
            OptionsButton.IsEnabled = true;
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

                try
                {
                    ListBox listbox = (ListBox)sender;

                    if (textBlock.Name == "ScreenName")
                    {
                        if (listbox.SelectedItem != null && currentView != CurrentView.User)
                        {
                            Tweet tweet = (Tweet)listbox.SelectedItem;
                            //System.Diagnostics.Process.Start(tweet.User.TwitterUrl);
                            DelegateUserTimelineFetch(tweet.User.ScreenName);
                        }
                    }
                }
                catch (Win32Exception ex)
                {
#if DEBUG
                    MessageBox.Show(ex.ToString());
#endif
                }
            }
        }

        private void MessagesListBox_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.MouseDevice.DirectlyOver.GetType() == typeof(TextBlock))
            {
                TextBlock textBlock = (TextBlock)e.MouseDevice.DirectlyOver;

                try
                {
                    ListBox listbox = (ListBox)sender;

                    if (textBlock.Name == "ScreenName")
                    {
                        if (listbox.SelectedItem != null && currentView != CurrentView.User)
                        {
                            DirectMessage tweet = (DirectMessage)listbox.SelectedItem;

                            ToggleMessage();
                            MessageUserTextBox.Text = textBlock.Text;
                            MessageTextBox.Focus();
                        }
                    }
                }
                catch (Win32Exception ex)
                {
#if DEBUG
                    MessageBox.Show(ex.ToString());
#endif
                }
            }
        }

        private void Friends_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (isLoggedIn)
            {
                StatusTextBlock.Text = "Loading People You Follow...";

                PeopleYouFollow peopleYouFollow = new PeopleYouFollow();
                peopleYouFollow.Show();
            }
        }

        void AlwaysOnTop_Click(object sender, RoutedEventArgs e)
        {
            if (((MenuItem)sender).IsChecked)
                this.Topmost = true;
            else
                this.Topmost = false;
        }

        private void Url_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            try
            {
                System.Diagnostics.Process.Start(textBlock.Text);
            }
            catch (Win32Exception ex)
            {
#if DEBUG
                MessageBox.Show(ex.ToString());
#endif
            }
        }

        private void ScreenName_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            try
            {
                System.Diagnostics.Process.Start(textBlock.Tag.ToString());
            }
            catch (Win32Exception ex)
            {
#if DEBUG
                MessageBox.Show(ex.ToString());
#endif
            }
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            showOptions();
        }

        private void showOptions()
        {
            Options options = new Options();

            Binding binding = new Binding();
            binding.Path = new PropertyPath("Topmost");
            binding.Source = this;
            binding.Mode = BindingMode.TwoWay;
            options.SetBinding(Options.TopmostProperty, binding);
            options.Owner = this;

            bool? dialogResult = options.ShowDialog();
            switch (dialogResult)
            {
                case true:
                    // User wants to save options

                    // update the refresh interval
                    int minutes = int.Parse(AppSettings.RefreshInterval);

                    refreshTimer.Stop();
                    if (minutes > 0)
                    {
                        refreshInterval = new TimeSpan(0, minutes, 0);
                        refreshTimer.Interval = refreshInterval;
                        refreshTimer.Start();
                    }

                    StatusTextBlock.Text = "Options Updated";

                    break;
                case false:
                    break;
                default:
                    // Indeterminate, do nothing
                    break;
            }
            if (string.IsNullOrEmpty(AppSettings.Username))
            {
                // User wants to logout
                isLoggedIn = false;
                tweets.Clear();
                StatusTextBlock.Text = "Login";

                PlayStoryboard("ShowLogin");
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
