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
using log4net;
using TwitterLib;

namespace Witty
{
    public partial class MainWindow
    {
        private static readonly ILog logger = LogManager.GetLogger("Witty.Logging");

        public MainWindow()
        {
            this.InitializeComponent();

            #region Minimize to tray setup

            m_notifyIcon = new System.Windows.Forms.NotifyIcon();
            m_notifyIcon.BalloonTipText = "Richt-click for more options";
            m_notifyIcon.BalloonTipTitle = "Witty";
            m_notifyIcon.Text = "Witty - The WPF Twitter Client";
            m_notifyIcon.Icon = Witty.Properties.Resources.AppIcon;
            m_notifyIcon.Click += new EventHandler(m_notifyIcon_Click);

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

            // Set the data context for all of the tabs
            LayoutRoot.DataContext = tweets;
            RepliesListBox.ItemsSource = replies;
            UserTab.DataContext = userTweets;
            MessagesListBox.ItemsSource = messages;

            // Set how often to get updates from Twitter
            refreshInterval = new TimeSpan(0, int.Parse(AppSettings.RefreshInterval), 0);

            this.Topmost = AlwaysOnTopMenuItem.IsChecked = AppSettings.AlwaysOnTop;

            // Does the user need to login?
            if (string.IsNullOrEmpty(AppSettings.Username))
            {
                PlayStoryboard("ShowLogin");
            }
            else
            {
                LoginControl.Visibility = Visibility.Hidden;

                twitter = new TwitterNet(AppSettings.Username, AppSettings.Password, WebProxyHelper.GetConfiguredWebProxy());

                // Let the user know what's going on
                StatusTextBlock.Text = Properties.Resources.TryLogin;
                PlayStoryboard("Fetching");

                // Create a Dispatcher to attempt login on new thread
                NoArgDelegate loginFetcher = new NoArgDelegate(this.TryLogin);
                loginFetcher.BeginInvoke(null, null);
            }
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
        private delegate void LoginDelegate(User arg);        

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
                    case 3:
                        return CurrentView.Messages;
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
                    DispatcherPriority.Normal,
                    new OneArgDelegate(UpdateUserInterface), twitter.GetFriendsTimeline());
            }
            catch (WebException ex)
            {
                logger.Debug(String.Format("There was a problem fetching new tweets from Twitter.com: {0}", ex.ToString()));
            }
        }

        private void UpdateUserInterface(TweetCollection newTweets)
        {
            DateTime lastUpdated = DateTime.Now;
            StatusTextBlock.Text = "Last Updated: " + lastUpdated.ToLongTimeString();

            AppSettings.LastUpdated = lastUpdated.ToString();
            AppSettings.Save();

            // Update existing tweets
            foreach (Tweet tweet in tweets)
            {
                tweet.IsNew = false;
                tweet.UpdateRelativeTime();
            }

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
            }

            if (tweetAdded > 0 && AppSettings.PlaySounds)
            {
                // Play tweets found sound
                SoundPlayer player = new SoundPlayer(Witty.Properties.Resources.alert);
                player.Play();
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
                    DispatcherPriority.Normal,
                    new OneStringArgDelegate(AddTweet), TweetTextBox.Text);
            }
        }

        private void AddTweet(string tweetText)
        {
            try
            {
                //bmsullivan If tweet is short enough, leave real URLs for clarity
                if (tweetText.Length > 140)
                {
                    //parse the text here and tiny up any URLs found.
                    TinyUrlHelper tinyUrls = new TinyUrlHelper();
                    tweetText = tinyUrls.ConvertUrlsToTinyUrls(tweetText);
                }
                Tweet tweet = twitter.AddTweet(tweetText);

                // Schedule the update function in the UI thread.
                LayoutRoot.Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new AddTweetUpdateDelegate(UpdatePostUserInterface), tweet);
            }
            catch (WebException ex)
            {
                UpdateTextBlock.Text = "Update failed.";
                logger.Debug(String.Format("There was a problem fetching new tweets from Twitter.com: {0}", ex.ToString()));
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
                logger.Error("There was a problem posting your tweet to Twitter.com.");
                MessageBox.Show("There was a problem posting your tweet to Twitter.com.");
            }
        }

        private void Update_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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
                    DispatcherPriority.Normal,
                    new OneArgDelegate(UpdateRepliesInterface), twitter.GetReplies());
            }
            catch (WebException ex)
            {
                logger.Debug(String.Format("There was a problem fetching your replies from Twitter.com. ", ex.Message));
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
                    DispatcherPriority.Normal,
                    new MessagesDelegate(UpdateMessagesInterface), twitter.RetrieveMessages());
            }
            catch (WebException ex)
            {
                logger.Debug(String.Format("There was a problem fetching your direct messages from Twitter.com: {0}", ex.ToString()));
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
                    DispatcherPriority.Normal,
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
                    DispatcherPriority.Normal,
                    new NoArgDelegate(UpdateMessageUserInterface));
            }
            catch (WebException ex)
            {
                UpdateTextBlock.Text = "Message failed.";
                logger.Debug(String.Format("There was a problem sending your message: {0}", ex.ToString()));
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

        private void Message_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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
                DispatcherPriority.Normal,
                new OneStringArgDelegate(GetUserTimeline), userId);
        }

        private void GetUserTimeline(string userId)
        {
            try
            {
                // Schedule the update function in the UI thread.
                LayoutRoot.Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new OneArgDelegate(UpdateUsersTimelineInterface), twitter.GetUserTimeline(userId));
            }
            catch (WebException ex)
            {
                logger.Debug(String.Format("There was a problem fetching the user's timeline from Twitter.com: {0}", ex.ToString()));
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

        #region Login

        private void TryLogin()
        {
            try
            {
                // Schedule the update function in the UI thread.
                LayoutRoot.Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new LoginDelegate(UpdatePostLoginInterface), twitter.Login());
            }
            catch (WebException ex)
            {
                logger.Debug(String.Format("There was a problem logging in to Twitter: {0}", ex.ToString()));
            }
            catch (ProxyAuthenticationRequiredException ex)
            {
                logger.Error("Incorrect proxy configuration.");
                MessageBox.Show("Proxy server is configured incorrectly.  Please correct the settings on the Options menu.");
                LayoutRoot.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new NoArgDelegate(UpdateLoginFailedInterface));           
            }
        }

        private void UpdatePostLoginInterface(User user)
        {
            isLoggedIn = true;
            RefreshButton.IsEnabled = true;
            OptionsButton.IsEnabled = true;
            SearchButton.IsEnabled = true;
            AppSettings.LastUpdated = string.Empty;
            Search.IsEnabled = true;

            App.LoggedInUser = user;

            DelegateRecentFetch();

            // Setup refresh timer
            refreshTimer.Interval = refreshInterval;
            refreshTimer.Tick += new EventHandler(Timer_Elapsed);
            refreshTimer.Start();
        }

        private void UpdateLoginFailedInterface()
        {
            isLoggedIn = false;
            OptionsButton.IsEnabled = true;
        }

        private void LoginControl_Login(object sender, RoutedEventArgs e)
        {
            twitter = new TwitterNet(AppSettings.Username, AppSettings.Password, WebProxyHelper.GetConfiguredWebProxy());

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
            SearchButton.IsEnabled = true;
            Search.IsEnabled = true;
        }

        #endregion

        #region Misc Methods and Event Handlers

        /// <summary>
        /// Checks for keyboard shortcuts
        /// </summary>
        /// <param name="sender"></param>
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
                            createDirectMessage();
                            break;
                        case Key.U:
                            ToggleUpdate();
                            break;
                        case Key.R:
                            createReply();
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

        private void createDirectMessage()
        {
            //Direct message to user
            if (!isExpanded)
            {
                ToggleUpdate();
            }
            TweetTextBox.Text = "";
            TweetTextBox.Text = "D ";
            if (null != TweetsListBox.SelectedItem)
            {
                TweetTextBox.Text += ((Tweet)TweetsListBox.SelectedItem).User.ScreenName;
            }
            TweetTextBox.Select(TweetTextBox.Text.Length, 0);
        }

        private void createReply()
        {
            Tweet selectedTweet = null;
            //reply to user
            if (this.currentView == CurrentView.Replies)
            {
                if (null != RepliesListBox.SelectedItem) selectedTweet = (Tweet)RepliesListBox.SelectedItem;
            }
            else if (this.currentView == CurrentView.Messages)
            {
                if (null != MessagesListBox.SelectedItem) selectedTweet = (Tweet)MessagesListBox.SelectedItem;
            }
            else
            {
                if (null != TweetsListBox.SelectedItem) selectedTweet = (Tweet)TweetsListBox.SelectedItem;
            }

            if (null != selectedTweet)
            {
                if (!isExpanded)
                {
                    this.Tabs.SelectedIndex = 0;
                    ToggleUpdate();
                }
                TweetTextBox.Text = "";
                TweetTextBox.Text = "@" + selectedTweet.User.ScreenName + " ";
                TweetTextBox.Select(TweetTextBox.Text.Length, 0);
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
                case CurrentView.Messages:
                    DelegateMessagesFetch();
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

            // clear the filter text since it isn't applied when switching tabs
            FilterTextBox.Text = string.Empty;
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

        private void TweetsListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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
                    logger.Debug(String.Format("Exception: {0}", ex.ToString()));
                }
            }
        }

        private void MessagesListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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
                    logger.Debug(String.Format("Exception: {0}", ex.ToString()));
                }
            }
        }

        void AlwaysOnTop_Click(object sender, RoutedEventArgs e)
        {
            if (((MenuItem)sender).IsChecked)
                this.Topmost = true;
            else
                this.Topmost = false;

            AppSettings.AlwaysOnTop = this.Topmost;
            AppSettings.Save();
        }

        private void Url_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            try
            {
                System.Diagnostics.Process.Start(textBlock.Text);
            }
            catch (Win32Exception ex)
            {
                logger.Debug(String.Format("Exception: {0}", ex.ToString()));
            }
        }

        private void ScreenName_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            try
            {
                System.Diagnostics.Process.Start(textBlock.Tag.ToString());
            }
            catch (Win32Exception ex)
            {
                logger.Debug(String.Format("Exception: {0}", ex.ToString()));
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
                Search.IsEnabled = false;

                PlayStoryboard("ShowLogin");
            }
        }

        private void ContextMenuReply_Click(object sender, RoutedEventArgs e)
        {
            createReply();
        }

        private void ContextMenuDirectMessage_Click(object sender, RoutedEventArgs e)
        {
            createDirectMessage();
        }

        private void ContextMenuFollow_Click(object sender, RoutedEventArgs e)
        {
            logger.Debug("Follow not implemented.");
            throw new NotImplementedException();
        }

        private void ContextMenuDelete_Click(object sender, RoutedEventArgs e)
        {
            logger.Debug("Delete not implemented.");
            throw new NotImplementedException();
        }


        #endregion

        #region Search Filtering

        // Delegate for performing filter in background thread for performance improvements
        private delegate void FilterDelegate();

        /// <summary>
        /// Handles the filtering and search
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Start an async operation that filters the list.
            this.Dispatcher.BeginInvoke(
                DispatcherPriority.ApplicationIdle,
                new FilterDelegate(FilterWorker));          
        }

        /// <summary>
        /// Worker method that filters the list.
        /// </summary>
        private void FilterWorker()
        {
            //Use collection view to filter the listbox
            ICollectionView collectionView = CollectionViewSource.GetDefaultView(tweets);

            switch (currentView)
            {
                case CurrentView.Recent:
                    collectionView = CollectionViewSource.GetDefaultView(tweets);
                    break;
                case CurrentView.Replies:
                    collectionView = CollectionViewSource.GetDefaultView(replies);
                    break;
                case CurrentView.Messages:
                    collectionView = CollectionViewSource.GetDefaultView(messages);
                    break;
                case CurrentView.User:
                    collectionView = CollectionViewSource.GetDefaultView(userTweets);
                    break;
                default:
                    collectionView = CollectionViewSource.GetDefaultView(tweets);
                    break;
            }

            if (currentView == CurrentView.Messages)
                // messages aren't tweets
                collectionView.Filter = new Predicate<object>(MessageFilter);
            else
                collectionView.Filter = new Predicate<object>(TweetFilter);
        }

        /// <summary>
        /// Delegate to filter the tweet text and by the tweet user's screenname.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool TweetFilter(object item)
        {
            Tweet tweet = item as Tweet;

            // this will prevent the fade animation from starting when the tweet is filtered
            tweet.IsNew = false;

            return (tweet.Text.ToLower().Contains(FilterTextBox.Text.ToLower()))
                   || (tweet.User.ScreenName.ToLower().Contains(FilterTextBox.Text.ToLower()));
        }

        /// <summary>
        /// Delegate to filter the tweet text and by the tweet user's screenname.
        /// </summary>
        public bool MessageFilter(object item)
        {
            DirectMessage message = item as DirectMessage;
            return (message.Text.ToLower().Contains(FilterTextBox.Text.ToLower()))
                   || (message.Sender.ScreenName.ToLower().Contains(FilterTextBox.Text.ToLower()));
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (Search.Visibility == Visibility.Hidden)
            {
                PlayStoryboard("ShowSearchPanel");
            }
            else
            {
                PlayStoryboard("HideSearchPanel");
            }
        }

        #endregion

        #region Minimize to Tray

        private System.Windows.Forms.NotifyIcon m_notifyIcon;

        void OnClosed(object sender, EventArgs e)
        {
            if (!AppSettings.PersistLogin)
            {
                AppSettings.Username = string.Empty;
                AppSettings.Password = string.Empty;
                AppSettings.Save();
            }

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
            if (AppSettings.MinimizeToTray)
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

        #region Clear Methods

        internal void ClearTweets()
        {
            tweets.Clear();
        }

        internal void ClearReplies()
        {
            replies.Clear();
        }

        #endregion
    }
}
